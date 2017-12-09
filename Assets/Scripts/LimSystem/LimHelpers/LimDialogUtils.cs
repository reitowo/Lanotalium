using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_STANDALONE
using System.Windows.Forms;
#endif

public class LimDialogUtils : MonoBehaviour
{
    public MessageBoxManager MessageBox;
    public ProgressBarManager ProgressBar;

#if UNITY_STANDALONE
    public static string OpenFileDialog(string Title, string Filter, string InitPath)
    {
        OpenFileDialog Dialog = new OpenFileDialog();
        Dialog.CheckFileExists = false;
        Dialog.CheckPathExists = true;
        Dialog.Filter = Filter;
        if (InitPath != "") Dialog.InitialDirectory = InitPath;
        else Dialog.RestoreDirectory = true;
        Dialog.Multiselect = false;
        Dialog.Title = Title;
        if (Dialog.ShowDialog() == DialogResult.OK) return Dialog.FileName;
        else return null;
    }
    public static string OpenFolderDialog(string Description)
    {
        FolderBrowserDialog Dialog = new FolderBrowserDialog();
        Dialog.ShowNewFolderButton = true;
        Dialog.Description = Description;
        if (Dialog.ShowDialog() == DialogResult.OK) return Dialog.SelectedPath;
        else return null;
    }
    public static string SaveFileDialog(string Title, string Filter, string InitPath)
    {
        SaveFileDialog Dialog = new SaveFileDialog();
        Dialog.CheckFileExists = false;
        Dialog.Title = Title;
        Dialog.Filter = Filter;
        Dialog.FilterIndex = 1;
        if (InitPath != "") Dialog.InitialDirectory = InitPath;
        else Dialog.RestoreDirectory = true;
        if (Dialog.ShowDialog() == DialogResult.OK) return Dialog.FileName;
        else return null;
    }
#endif

#if UNITY_ANDROID
    
#endif
}
