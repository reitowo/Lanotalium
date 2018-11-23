using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WindowsDialogUtility : MonoBehaviour
{
    public MessageBoxManager MessageBox;
    public ProgressBarManager ProgressBar;

#if UNITY_STANDALONE
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        [MarshalAs(UnmanagedType.LPTStr)]
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class BrowserInfo
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pszDisplayName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpszTitle;
        public uint ulFlags;
        public IntPtr lpfn;
        public IntPtr lParam;
        public int iImage;
    }
    [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
    [DllImport("shell32.dll")]
    private static extern IntPtr SHBrowseForFolder([In, Out] BrowserInfo bi);
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    [DllImport("shell32.dll")]
    public static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);
    public static string OpenFileDialog(string Title, string Filter, string InitPath)
    {
#if UNITY_EDITOR
        return EditorUtility.OpenFilePanel(Title, InitPath, Filter);
#endif
        OpenFileName ofn = new OpenFileName();
        ofn.dlgOwner = GetActiveWindow();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = Filter.Replace("|", "\0") + "\0";
        ofn.file = new String(new char[1024]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new String(new char[256]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = InitPath?.Replace("/", "\\");
        ofn.title = Title;
        ofn.flags = 0x00000008;
        bool Result = GetOpenFileName(ofn);
        if (Result) return ofn.file;
        else return null;
    }
    public static string OpenFolderDialog(string Description)
    {
#if UNITY_EDITOR
        return EditorUtility.OpenFolderPanel("", "", "");
#endif
        BrowserInfo bi = new BrowserInfo
        {
            hwndOwner = GetActiveWindow(),
            lpszTitle = Description,
            ulFlags = 0x00000001
        };
        IntPtr Result = SHBrowseForFolder(bi);
        if (Result == IntPtr.Zero) return null;
        StringBuilder sb = new StringBuilder(1024);
        if (!SHGetPathFromIDListW(Result, sb)) return null;
        return sb.ToString();
    }
    public static string SaveFileDialog(string Title, string Filter, string InitPath)
    {
#if UNITY_EDITOR
        return EditorUtility.SaveFilePanel(Title, InitPath, "", Filter);
#endif
        OpenFileName ofn = new OpenFileName();
        ofn.dlgOwner = GetActiveWindow();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = Filter.Replace("|", "\0") + "\0";
        ofn.file = new String(new char[1024]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new String(new char[256]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = InitPath?.Replace("/", "\\");
        ofn.title = Title;
        ofn.flags = 0x00000008;
        ofn.defExt = ".txt";
        bool Result = GetSaveFileName(ofn);
        if (Result) return ofn.file;
        else return null;
    }
    public static void OpenExplorer(string SelectPath)
    {
        Process.Start("explorer.exe", "/select," + SelectPath?.Replace("/", "\\"));
    }
#endif

#if UNITY_ANDROID
    
#endif

#if UNITY_IOS
    public static string OpenFileDialog(string Title, string Filter, string InitPath)
    {
        return null;
    }
    public static string OpenFolderDialog(string Description)
    {
        return null;
    }
    public static string SaveFileDialog(string Title, string Filter, string InitPath)
    {
        return null;
    }
#endif
}
