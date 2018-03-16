using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LimTopMenuManager : MonoBehaviour
{
    public LimProjectManager ProjectManager;
    public WindowsDialogUtility DialogUtility;
    public GameObject FilePanel, WindowPanel, SettingPanel, PluginPanel, ChartConvertPanel;
    public Text FileText;
    public Text NewProjectText, OpenProjectText, SaveProjectText, SaveAsProjectText, ExitText;
    public Text WindowText;
    public Text InspectorText, TunerWindowText, TimeLineText, MusicPlayerText, CreatorText, SpectrumText, StatusText;
    public Text SettingText;
    public Text PreferencesText, ResetLayoutText;
    public Text PluginText;
    public Text PluginCenterText;
    public Text ChartConvertText;
    public Text ChartConvertBmsText;
    public Text TopText;
    public Text ChartZoneText;

    private void Start()
    {
        LimTutorialManager.ShowTutorial("FirstProject1");
        if (LimSystem.ChartContainer == null) return;
        TopText.text = string.Format("Lanotalium - <{0}>", LimProjectManager.CurrentProject.Name);
    }
    public void SetTexts()
    {
        FileText.text = LimLanguageManager.TextDict["TopMenu_File"];
        NewProjectText.text = LimLanguageManager.TextDict["TopMenu_File_NewProject"];
        OpenProjectText.text = LimLanguageManager.TextDict["TopMenu_File_OpenProject"];
        SaveProjectText.text = LimLanguageManager.TextDict["TopMenu_File_SaveProject"];
        SaveAsProjectText.text = LimLanguageManager.TextDict["TopMenu_File_SaveAsProject"];
        ExitText.text = LimLanguageManager.TextDict["TopMenu_File_Exit"];
        WindowText.text = LimLanguageManager.TextDict["TopMenu_Window"];
        InspectorText.text = LimLanguageManager.TextDict["TopMenu_Window_Inspector"];
        TunerWindowText.text = LimLanguageManager.TextDict["TopMenu_Window_Tuner"];
        TimeLineText.text = LimLanguageManager.TextDict["TopMenu_Window_TimeLine"];
        MusicPlayerText.text = LimLanguageManager.TextDict["TopMenu_Window_MediaPlayer"];
        CreatorText.text = LimLanguageManager.TextDict["TopMenu_Window_Creator"];
        SpectrumText.text = LimLanguageManager.TextDict["TopMenu_Window_Spectrum"];
        SettingText.text = LimLanguageManager.TextDict["TopMenu_Setting"];
        PreferencesText.text = LimLanguageManager.TextDict["TopMenu_Setting_Preferences"];
        ResetLayoutText.text = LimLanguageManager.TextDict["TopMenu_Setting_ResetLayout"];
        PluginText.text = LimLanguageManager.TextDict["TopMenu_Plugin"];
        PluginCenterText.text = LimLanguageManager.TextDict["TopMenu_Plugin_PluginCenter"];
        ChartConvertText.text = LimLanguageManager.TextDict["TopMenu_ChartConvert"];
        ChartConvertBmsText.text = LimLanguageManager.TextDict["TopMenu_ChartConvert_Bms"];
        ChartZoneText.text = LimLanguageManager.TextDict["TopMenu_ChartZone"];
        StatusText.text = LimLanguageManager.TextDict["TopMenu_Status"];
    }
    public void OpenFileMenu()
    {
        if (FilePanel.activeInHierarchy) FilePanel.SetActive(false);
        else FilePanel.SetActive(true);
        LimTutorialManager.ShowTutorial("FirstProject2");
    }
    public void NewProject()
    {
        ProjectManager.CreateProject();
    }
    public void LoadProject()
    {
        ProjectManager.LoadProject();
    }
    public void SaveProject()
    {
        if (LimSystem.ChartContainer == null) return;
        ProjectManager.SaveProject();
    }
    public void SaveAsProject()
    {
        if (LimSystem.ChartContainer == null) return;
        ProjectManager.SaveAsProject();
    }
    public void QuitApplication()
    {
        Application.Quit();
    }
    public void OpenWindowMenu()
    {
        if (WindowPanel.activeInHierarchy) WindowPanel.SetActive(false);
        else WindowPanel.SetActive(true);
    }
    public void OpenSettingMenu()
    {
        if (SettingPanel.activeInHierarchy) SettingPanel.SetActive(false);
        else SettingPanel.SetActive(true);
    }
    public void OpenPluginMenu()
    {
        if (PluginPanel.activeInHierarchy) PluginPanel.SetActive(false);
        else PluginPanel.SetActive(true);
    }
    public void OpenChartConvertMenu()
    {
        ChartConvertPanel.SetActive(!ChartConvertPanel.activeInHierarchy);
    }
    public void GotoChartZone()
    {
        DialogUtility.MessageBox.ShowMessage(LimLanguageManager.TextDict["TopMenu_ChartZone_MessageBox"],
            new MessageBoxManager.MessageBoxCallBack(() => {
                LimChartZoneManager.FromScene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(3);
            }));
    }
}
