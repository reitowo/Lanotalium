using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LimFirstRunManager : MonoBehaviour
{
    public GameObject FirstRunPanel;
    private string PreferencesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Lanotalium/Preferences.json";
    private string AppDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Lanotalium";
    private Lanotalium.PreferencesContainer Preferences;

    private void Start()
    {
        if (!Directory.Exists(AppDataRoaming)) Directory.CreateDirectory(AppDataRoaming);
        if (File.Exists(PreferencesSavePath))
        {
            if (Environment.GetCommandLineArgs().Length == 2)
            {
                SceneManager.LoadScene("LimTuner");
                return;
            }
            SceneManager.LoadScene(1);
            return;
        }
        else
        {
            FirstRunPanel.SetActive(true);
            Preferences = new Lanotalium.PreferencesContainer();
        }
    }
    public void IsChinese()
    {
        Preferences.LanguageName = "简体中文";
        EnterLaunch();
    }
    public void IsOtherCountry()
    {
        Preferences.LanguageName = "English";
        EnterLaunch();
    }
    public void EnterLaunch()
    {
        string PreferencesStr = JsonConvert.SerializeObject(Preferences);
        File.WriteAllText(PreferencesSavePath, PreferencesStr);
        SceneManager.LoadScene(1);
    }
}
