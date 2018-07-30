using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LimLanguageManager : MonoBehaviour
{
    public LimUpdateDetector UpdateDetector;
    public LimExceptionManager ExceptionManager;
    public LimEditorManager EditorManager;
    public LimChartZoneManager ChartZoneManager;
    public List<Lanotalium.Editor.SetTextDelegate> SetTextDelegates = new List<Lanotalium.Editor.SetTextDelegate>();
    public Dictionary<string, Lanotalium.Editor.LanguagePackage> LanguagePackages;

    public static string CurrentLanguage;
    public static Dictionary<string, string> TextDict;
    public static Dictionary<string, string> NotificationDict;
    public static Dictionary<string, string> HintDict;
    public static Dictionary<string, string> TutorialDict;

    public static UnityEvent OnLanguageChanged = new UnityEvent();

    private string _LanguagePackageFolder;

    void Start()
    {
        LanguagePackages = new Dictionary<string, Lanotalium.Editor.LanguagePackage>();
        _LanguagePackageFolder = Application.streamingAssetsPath + "/Language";
        LoadAllLanguagePackages();
        CollectSetTextsDelegates();
        SetLanguage(LimSystem.Preferences.LanguageName);
    }
    private void LoadPackageFromFile(string Path)
    {
        string[] Lines = File.ReadAllLines(Path);
        if (Lines.Length == 0) return;
        if (!Lines[0].StartsWith("!")) return;
        Lanotalium.Editor.LanguagePackage Lang = new Lanotalium.Editor.LanguagePackage();
        Lang.LanguageName = Lines[0].Remove(0, 1);
        int CurrentWriteDict = -1;
        for (int i = 1; i < Lines.Length; ++i)
        {
            if (Lines[i].StartsWith("#")) CurrentWriteDict++;
            else
            {
                string[] KeyValue = Lines[i].Split('$');
                if (CurrentWriteDict == 0) Lang.TextDict.Add(KeyValue[0], KeyValue[1]);
                else if (CurrentWriteDict == 1) Lang.NotificationDict.Add(KeyValue[0], KeyValue[1]);
                else if (CurrentWriteDict == 2) Lang.HintDict.Add(KeyValue[0], KeyValue[1].Replace("<br>", "\n"));
                else if (CurrentWriteDict == 3) Lang.TutorialDict.Add(KeyValue[0], KeyValue[1].Replace("<br>", "\n"));
            }
        }
        LanguagePackages.Add(Lang.LanguageName, Lang);
    }
    private void LoadAllLanguagePackages()
    {
        string[] Paths = Directory.GetFiles(_LanguagePackageFolder);
        foreach (string Path in Paths) LoadPackageFromFile(Path);
    }
    public void CollectSetTextsDelegates()
    {
        if (EditorManager != null)
        {
            SetTextDelegates.Add(EditorManager.MusicPlayerWindow.SetTexts);
            SetTextDelegates.Add(EditorManager.InspectorWindow.SetTexts);
            SetTextDelegates.Add(EditorManager.TunerWindow.SetTexts);
            SetTextDelegates.Add(EditorManager.TimeLineWindow.SetTexts);
            SetTextDelegates.Add(EditorManager.CreatorWindow.SetTexts);
            SetTextDelegates.Add(EditorManager.InspectorWindow.ComponentBasic.SetTexts);
            SetTextDelegates.Add(EditorManager.InspectorWindow.ComponentHoldNote.SetTexts);
            SetTextDelegates.Add(EditorManager.InspectorWindow.ComponentMotion.SetTexts);
            SetTextDelegates.Add(EditorManager.InspectorWindow.ComponentType.SetTexts);
            SetTextDelegates.Add(EditorManager.InspectorWindow.ComponentBpm.SetTexts);
            SetTextDelegates.Add(EditorManager.InspectorWindow.ComponentScrollSpeed.SetTexts);
            SetTextDelegates.Add(EditorManager.InspectorWindow.ComponentDefault.SetTexts);
            SetTextDelegates.Add(EditorManager.TopMenu.SetTexts);
            SetTextDelegates.Add(EditorManager.PreferencesWindow.SetTexts);
            SetTextDelegates.Add(EditorManager.GizmoMotionWindow.SetTexts);
            SetTextDelegates.Add(EditorManager.CreatorWindow.ClickToCreateManager.SetTexts);
            SetTextDelegates.Add(EditorManager.CloudManager.SetTexts);
            SetTextDelegates.Add(EditorManager.TunerWindow.TunerHeadManager.SetTexts);
            SetTextDelegates.Add(EditorManager.CreatorWindow.CopierManager.SetTexts);
            SetTextDelegates.Add(EditorManager.TopMenu.ProjectManager.SetTexts);
            SetTextDelegates.Add(EditorManager.SetTexts);
            SetTextDelegates.Add(EditorManager.SpectrumWindow.SetTexts);
            SetTextDelegates.Add(EditorManager.StatusManager.SetTexts);
            SetTextDelegates.Add(EditorManager.SubmitManager.SetTexts);
        }
        if (ChartZoneManager != null)
        {
            SetTextDelegates.Add(ChartZoneManager.SetTexts);
        }
        if (ExceptionManager != null)
        {
            SetTextDelegates.Add(ExceptionManager.SetTexts);
        }
        if (UpdateDetector != null)
        {
            SetTextDelegates.Add(UpdateDetector.SetTexts);
        }
    }
    public void CallAllSetTexts()
    {
        OnLanguageChanged.Invoke();
        foreach (Lanotalium.Editor.SetTextDelegate SetTexts in SetTextDelegates) SetTexts();
    }
    public void SetLanguage(string LanguageName)
    {
        CurrentLanguage = LanguageName;
        if (!LanguagePackages.ContainsKey(LanguageName))
        {
            LimNotifyIcon.ShowMessage("Language Not Found !");
            return;
        }
        TextDict = LanguagePackages[LanguageName].TextDict;
        NotificationDict = LanguagePackages[LanguageName].NotificationDict;
        HintDict = LanguagePackages[LanguageName].HintDict;
        TutorialDict = LanguagePackages[LanguageName].TutorialDict;
        CallAllSetTexts();
    }

    public List<Dropdown.OptionData> GetLanguageDropdownOptionDataList()
    {
        List<Dropdown.OptionData> LangList = new List<Dropdown.OptionData>();
        foreach (string Key in LanguagePackages.Keys) LangList.Add(new Dropdown.OptionData(Key));
        return LangList;
    }
    public int FindLanguageDropdownValueByName(List<Dropdown.OptionData> Options, string LanguageName)
    {
        int Index = 0;
        foreach (Dropdown.OptionData Name in Options)
        {
            if (Name.text == LanguageName) break;
            Index++;
        }
        return Index;
    }
}
