using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Forms;
using System.IO;
using System;
using System.Reflection;
using System.ComponentModel;

public class LimNotifyIcon : MonoBehaviour
{
#if UNITY_STANDALONE
    /*public Texture2D Icon = null;
    private static NotifyIcon Notify = null;
    private static bool Initialized = false;
    private static string MessageBoxMessage = null, MessageBoxTitle = null;*/
    void Start()
    {
        /*try
        {
            if (UnityEngine.Application.isEditor) return;
            if (Environment.GetCommandLineArgs().Length == 2 && !LimProjectManager.LapDirectOpened) return;
            Notify = new NotifyIcon();
            MemoryStream memStream = new MemoryStream(Icon.EncodeToPNG());
            memStream.Seek(0, SeekOrigin.Begin);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(memStream);
            Notify.Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
            Notify.Visible = true;
            Notify.ContextMenu = new System.Windows.Forms.ContextMenu(new MenuItem[] { new MenuItem("Exit", new System.EventHandler((object o, System.EventArgs e) => { UnityEngine.Application.Quit(); })) });
            Notify.BalloonTipClicked += new EventHandler((object o, EventArgs e) => { ShowMessageBox(); });
            Notify.Text = "Lanotalium";
            memStream.Close();
            Initialized = true;
        }
        catch(Exception)
        {
            
        }*/
    }
    public static void ShowMessage(string Message, ToolTipIcon Mode, string Title = "Lanotalium", string MessageBoxOnlyText = "")
    {
        /*if (UnityEngine.Application.isEditor) return;
        if (UnityEngine.Application.platform != RuntimePlatform.WindowsPlayer) return;
        if (!Initialized) { Debug.Log("NotifyIcon: " + Message); return; }
        MessageBoxMessage = Message + "\n" + MessageBoxOnlyText;
        MessageBoxTitle = Title;
        Notify.ShowBalloonTip(3000, Title, Message, Mode);*/
    }
    private static void ShowMessageBox()
    {
        //MessageBox.Show(MessageBoxMessage, MessageBoxTitle);
    }
    private void OnApplicationQuit()
    {
        //if (Notify != null) Notify.Dispose();
    }
    private void OnDestroy()
    {
        //if (Notify != null) Notify.Dispose();
    }
#endif
#if UNITY_IOS
    public static void ShowMessage(string Message, ToolTipIcon Mode, string Title = "Lanotalium", string MessageBoxOnlyText = "")
    {
        Debug.Log(Message);
    }
#endif
}
