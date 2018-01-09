using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LimEditorManager : MonoBehaviour
{
    public LimMediaPlayerManager MusicPlayerWindow;
    public LimInspectorManager InspectorWindow;
    public LimTunerWindowManager TunerWindow;
    public LimTimeLineManager TimeLineWindow;
    public LimCreatorManager CreatorWindow;
    public LimPreferencesManager PreferencesWindow;
    public LimGizmoMotionManager GizmoMotionWindow;
    public LimCloudManager CloudManager;
    public LimPluginManager PluginManager;
    public LimTopMenuManager TopMenu;

    public GameObject WhatsNew, NewCn, NewEn;
    public Toggle WhatsNewHideToggle;
    public Text WhatsNewText, WhatsNewToggleText;

    public void ResetEditorLayout()
    {
        MusicPlayerWindow.BaseWindow.WindowRectTransform.anchoredPosition = new Vector2(1420, -890);
        InspectorWindow.BaseWindow.WindowRectTransform.anchoredPosition = new Vector2(1420, -60);
        TunerWindow.BaseWindow.WindowRectTransform.anchoredPosition = new Vector2(0, -60);
        TimeLineWindow.BaseWindow.WindowRectTransform.anchoredPosition = new Vector2(0, -860);
        CreatorWindow.BaseWindow.WindowRectTransform.anchoredPosition = new Vector2(1000, -60);
        MusicPlayerWindow.BaseWindow.WindowRectTransform.sizeDelta = new Vector2(500, 190);
        InspectorWindow.BaseWindow.WindowRectTransform.sizeDelta = new Vector2(500, 800);
        TunerWindow.BaseWindow.WindowRectTransform.sizeDelta = new Vector2(1000, 562.5f);
        TimeLineWindow.BaseWindow.WindowRectTransform.sizeDelta = new Vector2(1420, 220);
        CreatorWindow.BaseWindow.WindowRectTransform.sizeDelta = new Vector2(420, 562.5f);
    }
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
