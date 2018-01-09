using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Principal;
using System.IO;
using UnityEngine.UI;
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;
#if UNITY_STANDALONE
using Microsoft.Win32;
#endif

public class LimHighPermission : MonoBehaviour
{
    public GameObject RequestAdminPanel;
    public Text RequestAdminText;

    private void Start()
    {
        if (Application.isEditor) return;
        if (!LimSystem.Preferences.LapInjected) RegisterLapFormat();
    }
#if UNITY_STANDALONE
    #region PInvoke
    [StructLayout(LayoutKind.Sequential)]
    struct TokenElevation
    {
        public uint TokenIsElevated;
    }
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTokenInformation(IntPtr TokenHandle, int TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);
    #endregion
    private bool IsAdministrator()
    {
        IntPtr TokenHandle;
        if (!OpenProcessToken(Process.GetCurrentProcess().Handle, 0x0008, out TokenHandle)) return false;
        int ReturnLength;
        IntPtr TokenInformation = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TokenElevation)));
        if (!GetTokenInformation(TokenHandle, 20, TokenInformation, Marshal.SizeOf(typeof(TokenElevation)), out ReturnLength))
        {
            Marshal.FreeHGlobal(TokenInformation);
            return false;
        }
        if (ReturnLength == Marshal.SizeOf(typeof(TokenElevation)))
        {
            TokenElevation Elevation = (TokenElevation)Marshal.PtrToStructure(TokenInformation, typeof(TokenElevation));
            bool IsElevated = Elevation.TokenIsElevated == 0 ? false : true;
            Marshal.FreeHGlobal(TokenInformation);
            return IsElevated;
        }
        else
        {
            Marshal.FreeHGlobal(TokenInformation);
            return false;
        }
    }
    public void RunAsNormal()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor) return;
        Application.Quit();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
        Process.Start(startInfo);
        Process.GetCurrentProcess().Kill();
    }
    public void RunAsAdministrator()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor) return;
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
        startInfo.Arguments = string.Join(" ", Environment.GetCommandLineArgs());
        startInfo.Verb = "runas";
        Process.Start(startInfo);
        Process.GetCurrentProcess().Kill();
    }
    public void DoNotRunAsAdministrator()
    {
        RequestAdminPanel.SetActive(false);
        LimSystem.Preferences.DoNotRunAsAdmin = true;
    }
    private void RequestAdministrator()
    {
        if (LimSystem.Preferences.DoNotRunAsAdmin) return;
        RequestAdminPanel.SetActive(true);
        RequestAdminText.text = (LimSystem.Preferences.LanguageName == "简体中文") ?
            "为了关联Lanotalium 工程文件\n后缀名(*.lap)，每次更新后，\n需要以管理员身份运行\n<color=red>一次</color>\nLanotalium。是否同意？" :
            "To associate Lanotalium Project\n files (*.lap) with Lanotalium,\n you need to run Lanotalium as Administrator <color=red>once</color>\n after each update. Do you agree?";
    }
    private void RegisterLapFormat()
    {
        if (!IsAdministrator()) { RequestAdministrator(); return; }
        try
        {
            RegistryKey Key = Registry.ClassesRoot.OpenSubKey("Lanotalium.Project");
            if (Key == null)
            {
                Key = Registry.ClassesRoot.CreateSubKey("Lanotalium.Project");
                Key.SetValue("", "Lanotalium Project");
                RegistryKey Shell = Key.CreateSubKey("shell");
                Shell = Shell.CreateSubKey("open");
                Shell = Shell.CreateSubKey("command");
                Shell.SetValue("", "\"" + Directory.GetParent(Application.dataPath).FullName + "\\Lanotalium.exe\"" + " \"%1\"");
                RegistryKey Icon = Key.CreateSubKey("DefaultIcon");
                Icon.SetValue("", "\"" + Application.streamingAssetsPath + "\\Icon\\LapIcon.ico\"");
            }
            else
            {
                RegistryKey Shell = Key.OpenSubKey("shell");
                Shell = Shell.OpenSubKey("open");
                Shell = Shell.OpenSubKey("command", true);
                Shell.SetValue("", "\"" + Directory.GetParent(Application.dataPath).FullName + "\\Lanotalium.exe\"" + " \"%1\"");
                RegistryKey Icon = Key.OpenSubKey("DefaultIcon", true);
                Icon.SetValue("", "\"" + Application.streamingAssetsPath + "\\Icon\\LapIcon.ico\"");
            }

            Key = Registry.ClassesRoot.OpenSubKey(".lap");
            if (Key == null)
            {
                Key = Registry.ClassesRoot.CreateSubKey(".lap");
                Key.SetValue("", "Lanotalium.Project");
            }
            LimSystem.Preferences.LapInjected = true;
        }
        catch
        {
            LimSystem.Preferences.LapInjected = false;
            RequestAdministrator();
            return;
        }
    }
#endif
}
