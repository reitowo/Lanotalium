using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LimEditorManager : MonoBehaviour
{
    public LimMusicPlayerManager MusicPlayerWindow;
    public LimInspectorManager InspectorWindow;
    public LimTunerWindowManager TunerWindow;
    public LimTimeLineManager TimeLineWindow;
    public LimCreatorManager CreatorWindow;
    public LimPreferencesManager PreferencesWindow;
    public LimGizmoMotionManager GizmoMotionWindow;
    //public LimCapturer CapturerWindow;
    public LimCloudManager CloudManager;
    public LimPluginManager PluginManager;
    public LimTopMenuManager TopMenu;

    public GameObject WhatsNew, NewCn, NewEn;
    public Toggle WhatsNewHideToggle;
    public Text WhatsNewText, WhatsNewToggleText;

    public void RestoreEditorLayout()
    {
        if (!LimSystem.EditorLayout.isLayoutValid()) return;
        MusicPlayerWindow.BaseWindow.WindowRectTransform.anchoredPosition = LimSystem.EditorLayout.MusicPlayerPos.ToVector2();
        InspectorWindow.BaseWindow.WindowRectTransform.anchoredPosition = LimSystem.EditorLayout.InspectorPos.ToVector2();
        TunerWindow.BaseWindow.WindowRectTransform.anchoredPosition = LimSystem.EditorLayout.TunerWindowPos.ToVector2();
        TimeLineWindow.BaseWindow.WindowRectTransform.anchoredPosition = LimSystem.EditorLayout.TimelinePos.ToVector2();
        CreatorWindow.BaseWindow.WindowRectTransform.anchoredPosition = LimSystem.EditorLayout.CreatorPos.ToVector2();
        MusicPlayerWindow.BaseWindow.WindowRectTransform.sizeDelta = LimSystem.EditorLayout.MusicPlayerSize.ToVector2();
        InspectorWindow.BaseWindow.WindowRectTransform.sizeDelta = LimSystem.EditorLayout.InspectorSize.ToVector2();
        TunerWindow.BaseWindow.WindowRectTransform.sizeDelta = LimSystem.EditorLayout.TunerWindowSize.ToVector2();
        TimeLineWindow.BaseWindow.WindowRectTransform.sizeDelta = LimSystem.EditorLayout.TimelineSize.ToVector2();
        CreatorWindow.BaseWindow.WindowRectTransform.sizeDelta = LimSystem.EditorLayout.CreatorSize.ToVector2();
        MusicPlayerWindow.BaseWindow.WindowRectTransform.SetSiblingIndex(LimSystem.EditorLayout.MusicPlayerSibling);
        InspectorWindow.BaseWindow.WindowRectTransform.SetSiblingIndex(LimSystem.EditorLayout.InspectorSibling);
        TunerWindow.BaseWindow.WindowRectTransform.SetSiblingIndex(LimSystem.EditorLayout.TunerWindowSibling);
        TimeLineWindow.BaseWindow.WindowRectTransform.SetSiblingIndex(LimSystem.EditorLayout.TimeLineSibling);
        CreatorWindow.BaseWindow.WindowRectTransform.SetSiblingIndex(LimSystem.EditorLayout.CreatorSibling);
    }
    public void SaveEditorLayout()
    {
        LimSystem.EditorLayout.MusicPlayerPos = new Lanotalium.Editor.Vector2Save(MusicPlayerWindow.BaseWindow.WindowRectTransform.anchoredPosition);
        LimSystem.EditorLayout.InspectorPos = new Lanotalium.Editor.Vector2Save(InspectorWindow.BaseWindow.WindowRectTransform.anchoredPosition);
        LimSystem.EditorLayout.TunerWindowPos = new Lanotalium.Editor.Vector2Save(TunerWindow.BaseWindow.WindowRectTransform.anchoredPosition);
        LimSystem.EditorLayout.TimelinePos = new Lanotalium.Editor.Vector2Save(TimeLineWindow.BaseWindow.WindowRectTransform.anchoredPosition);
        LimSystem.EditorLayout.CreatorPos = new Lanotalium.Editor.Vector2Save(CreatorWindow.BaseWindow.WindowRectTransform.anchoredPosition);
        LimSystem.EditorLayout.MusicPlayerSize = new Lanotalium.Editor.Vector2Save(MusicPlayerWindow.BaseWindow.WindowRectTransform.sizeDelta);
        LimSystem.EditorLayout.InspectorSize = new Lanotalium.Editor.Vector2Save(InspectorWindow.BaseWindow.WindowRectTransform.sizeDelta);
        LimSystem.EditorLayout.TunerWindowSize = new Lanotalium.Editor.Vector2Save(TunerWindow.BaseWindow.WindowRectTransform.sizeDelta);
        LimSystem.EditorLayout.TimelineSize = new Lanotalium.Editor.Vector2Save(TimeLineWindow.BaseWindow.WindowRectTransform.sizeDelta);
        LimSystem.EditorLayout.CreatorSize = new Lanotalium.Editor.Vector2Save(CreatorWindow.BaseWindow.WindowRectTransform.sizeDelta);
        LimSystem.EditorLayout.MusicPlayerSibling = MusicPlayerWindow.BaseWindow.WindowRectTransform.GetSiblingIndex();
        LimSystem.EditorLayout.InspectorSibling = InspectorWindow.BaseWindow.WindowRectTransform.GetSiblingIndex();
        LimSystem.EditorLayout.TunerWindowSibling = TunerWindow.BaseWindow.WindowRectTransform.GetSiblingIndex();
        LimSystem.EditorLayout.TimeLineSibling = TimeLineWindow.BaseWindow.WindowRectTransform.GetSiblingIndex();
        LimSystem.EditorLayout.CreatorSibling = CreatorWindow.BaseWindow.WindowRectTransform.GetSiblingIndex();
    }

    private void Start()
    {
        if (!LimSystem.Preferences.HideWhatsNew)
        {
            WhatsNew.SetActive(true);
            if (LimSystem.Preferences.LanguageName == "简体中文") NewCn.SetActive(true);
            else NewEn.SetActive(true);
        }
    }
    public void SetTexts()
    {
        WhatsNewText.text = LimLanguageManager.TextDict["WhatsNew_Label"];
        WhatsNewToggleText.text = LimLanguageManager.TextDict["WhatsNew_Toggle"];
    }
    public void OnWhatsNewHideToggle()
    {
        LimSystem.Preferences.HideWhatsNew = WhatsNewHideToggle.isOn;
    }
}
