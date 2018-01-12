using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimPreferencesManager : MonoBehaviour
{
    public LimWindowManager BaseWindow;
    public LimLanguageManager LanguageManager;
    public LimAudioEffectManager AudioEffectManager;
    public Text LabelText;
    public Text LanguageText, AutosaveText, JudgeColorText, AudioEffectText, AudioEffectThemeText, UnsafeText, WorkingBGMText;
    public Dropdown LanguageDropDown, AudioEffectThemeDropdown;
    public Toggle AutosaveToggle, JudgeColorToggle, AudioEffectToggle, UnsafeToggle, WorkingBGMToggle;

    private void OnEnable()
    {
        BaseWindow.transform.SetSiblingIndex(transform.parent.childCount - 2);
    }
    public void SetTexts()
    {
        LabelText.text = LimLanguageManager.TextDict["Preferences_Label"];
        LanguageText.text = LimLanguageManager.TextDict["Preferences_Language"];
        AutosaveText.text = LimLanguageManager.TextDict["Preferences_Autosave"];
        JudgeColorText.text = LimLanguageManager.TextDict["Preferences_JudgeColor"];
        AudioEffectText.text = LimLanguageManager.TextDict["Preferences_AudioEffect"];
        AudioEffectThemeText.text = LimLanguageManager.TextDict["Preferences_AudioEffectTheme"];
        UnsafeText.text = LimLanguageManager.TextDict["Preferences_Unsafe"];
        WorkingBGMText.text = LimLanguageManager.TextDict["Preferences_WorkingBGM"];
    }
    public void OpenPreferencesMenu()
    {
        LanguageDropDown.options = LanguageManager.GetLanguageDropdownOptionDataList();
        LanguageDropDown.value = LanguageManager.FindLanguageDropdownValueByName(LanguageDropDown.options, LimSystem.Preferences.LanguageName);
        AutosaveToggle.isOn = LimSystem.Preferences.Autosave;
        JudgeColorToggle.isOn = LimSystem.Preferences.JudgeColor;
        AudioEffectToggle.isOn = LimSystem.Preferences.AudioEffect;
        AudioEffectThemeDropdown.value = (int)LimSystem.Preferences.AudioEffectTheme;
        UnsafeToggle.isOn = LimSystem.Preferences.Unsafe;
        gameObject.SetActive(true);
    }
    public void OnLanguageDropdownChange()
    {
        string LanguageName = LanguageDropDown.options[LanguageDropDown.value].text;
        LanguageManager.SetLanguage(LanguageName);
        LimSystem.Preferences.LanguageName = LanguageName;
    }
    public void OnAutosaveToggleChange()
    {
        LimSystem.Preferences.Autosave = AutosaveToggle.isOn;
    }
    public void OnJudgeColorToggleChange()
    {
        LimSystem.Preferences.JudgeColor = JudgeColorToggle.isOn;
    }
    public void OnAudioEffectToggleChange()
    {
        LimSystem.Preferences.AudioEffect = AudioEffectToggle.isOn;
    }
    public void OnAudioEffectThemeDropdownChange()
    {
        LimSystem.Preferences.AudioEffectTheme = (Lanotalium.Tuner.AudioEffectTheme)AudioEffectThemeDropdown.value;
        AudioEffectManager.SetEffectTheme();
    }
    public void OnUnsafeToggleChange()
    {
        LimSystem.Preferences.Unsafe = UnsafeToggle.isOn;
    }
    public void OnWorkingBGMToggleChange()
    {
        LimSystem.Preferences.PlayWorkingBGM = WorkingBGMToggle.isOn;
    }
}
