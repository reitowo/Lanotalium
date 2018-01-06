using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Windows.Forms;
using System;

public class LimLaunchManager : MonoBehaviour
{
    public Text Message, VersionText;
    public UnityEngine.UI.Button UpdateBtn;
    public Slider EnterLanotaliumSlider;
    public Text EnterLanotaliumText, TutorialText, SupportMeText, AboutText, QuitText, ChartZoneText;
    public GameObject SupportMePanel, About;

    private int LatestBuild;
    private string LatestVersion;
    private Dictionary<string, string> LaunchLanguageDict = new Dictionary<string, string>();

    private void Start()
    {
        VersionText.text = LimSystem.Version;
        SetLanguageDict();
        SetTexts();
        RemoteSettings.Updated += () =>
        {
            LimSystem.LanotaliumServer = RemoteSettings.GetString("LanotaliumServer", "http://lanotalium.cn");
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("LimLaunch")) StartCoroutine(CheckUpdateCoroutine());
        };
    }
    private void SetLanguageDict()
    {
        LaunchLanguageDict.Add("EnterLanotalium_ZhCn", "<size=30>进入</size>  <size=45>Lanotalium</size>");
        LaunchLanguageDict.Add("EnterLanotalium_En", "<size=30>Enter</size>  <size=45>Lanotalium</size>");
        LaunchLanguageDict.Add("Tutorial_ZhCn", "教程");
        LaunchLanguageDict.Add("Tutorial_En", "Tutorial");
        LaunchLanguageDict.Add("SupportMe_ZhCn", "支持");
        LaunchLanguageDict.Add("SupportMe_En", "Donate");
        LaunchLanguageDict.Add("About_ZhCn", "关于");
        LaunchLanguageDict.Add("About_En", "About");
        LaunchLanguageDict.Add("Quit_ZhCn", "退出");
        LaunchLanguageDict.Add("Quit_En", "Quit");
        LaunchLanguageDict.Add("ChartZone_ZhCn", "谱面中心");
        LaunchLanguageDict.Add("ChartZone_En", "ChartZone");
    }
    private void SetTexts()
    {
        if (LimSystem.Preferences.LanguageName == "简体中文")
        {
            ChartZoneText.text = LaunchLanguageDict["ChartZone_ZhCn"];
            EnterLanotaliumText.text = LaunchLanguageDict["EnterLanotalium_ZhCn"];
            TutorialText.text = LaunchLanguageDict["Tutorial_ZhCn"];
            SupportMeText.text = LaunchLanguageDict["SupportMe_ZhCn"];
            AboutText.text = LaunchLanguageDict["About_ZhCn"];
            QuitText.text = LaunchLanguageDict["Quit_ZhCn"];
        }
        else
        {
            ChartZoneText.text = LaunchLanguageDict["ChartZone_En"];
            EnterLanotaliumText.text = LaunchLanguageDict["EnterLanotalium_En"];
            TutorialText.text = LaunchLanguageDict["Tutorial_En"];
            SupportMeText.text = LaunchLanguageDict["SupportMe_En"];
            AboutText.text = LaunchLanguageDict["About_En"];
            QuitText.text = LaunchLanguageDict["Quit_En"];
        }
    }
    public void OpenSupportMePanel()
    {
        if (SupportMePanel.activeInHierarchy) SupportMePanel.SetActive(false);
        else SupportMePanel.SetActive(true);
    }
    public void OpenAbout()
    {
        if (About.activeInHierarchy) About.SetActive(false);
        else About.SetActive(true);
    }
    public void QuitFromMenu()
    {
        UnityEngine.Application.Quit();
    }
    IEnumerator CheckUpdateCoroutine()
    {
        if (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable)
        {
            Message.text = (LimSystem.Preferences.LanguageName == "简体中文" ? "网络不可用" : "Network Unavaliable.");
            yield break;
        }
        WWW CheckUpdate = new WWW(LimSystem.LanotaliumServer + "/lanotalium/build.txt");
        yield return CheckUpdate;
        if (CheckUpdate != null && string.IsNullOrEmpty(CheckUpdate.error))
        {
            string[] Splited = CheckUpdate.text.Split(' ');
            LatestVersion = Splited[1];
            if (!int.TryParse(Splited[0], out LatestBuild)) yield break;
            if (LatestBuild > LimSystem.Build)
            {
                UpdateBtn.gameObject.SetActive(true);
                Message.alignment = TextAnchor.UpperLeft;
                Message.text = (LimSystem.Preferences.LanguageName == "简体中文" ? "发现更新" : "Update Found! ") + LatestVersion + "\n";
                WWW LoadWhatsnew = new WWW(LimSystem.LanotaliumServer + (LimSystem.Preferences.LanguageName == "简体中文" ? "/lanotalium/whatsnew-zhcn.txt" : "/lanotalium/whatsnew-en.txt"));
                yield return LoadWhatsnew;
                if (LoadWhatsnew != null && string.IsNullOrEmpty(LoadWhatsnew.error))
                {
                    Message.text += LoadWhatsnew.text;
                }
            }
            else
            {
                WWW LoadNotification = new WWW(LimSystem.LanotaliumServer + (LimSystem.Preferences.LanguageName == "简体中文" ? "/lanotalium/notification-zhcn.txt" : "/lanotalium/notification-en.txt"));
                yield return LoadNotification;
                if (LoadNotification != null && string.IsNullOrEmpty(LoadNotification.error))
                {
                    Message.text = LoadNotification.text;
                }
            }
        }
    }
    public void DownloadUpdate()
    {
        if (!File.Exists(UnityEngine.Application.streamingAssetsPath + "/Updator/Newtonsoft.Json.dll"))
            File.Copy(UnityEngine.Application.dataPath + "/Managed/Newtonsoft.Json.dll", UnityEngine.Application.streamingAssetsPath + "/Updator/Newtonsoft.Json.dll", true);
        ProcessStartInfo Updator = new ProcessStartInfo();
        Updator.FileName = UnityEngine.Application.streamingAssetsPath + "/Updator/LanotaliumUpdateClient.exe";
        Updator.Arguments = string.Format("{0} {1} {2}", Directory.GetParent(UnityEngine.Application.dataPath).FullName.Replace(" ", "%20"), LimSystem.Version, "http://lanotalium.cn/lanotalium/updator");
        Process.Start(Updator);
        Process.GetCurrentProcess().Kill();
    }
    public void EnterLanotalium()
    {
        StartCoroutine(EnterLanotaliumCoroutine());
    }
    IEnumerator EnterLanotaliumCoroutine()
    {
        EnterLanotaliumText.text = (LimSystem.Preferences.LanguageName == "简体中文" ? "加载中" : "Now Loading");
        AsyncOperation AsyncLoading = SceneManager.LoadSceneAsync("LimTuner");
        while (!AsyncLoading.isDone)
        {
            EnterLanotaliumSlider.value = AsyncLoading.progress;
            yield return null;
        }
    }
    public void OpenOfficialWebsite()
    {
        UnityEngine.Application.OpenURL(LimSystem.LanotaliumServer);
    }
    public void OpenTutorialWebsite()
    {
        UnityEngine.Application.OpenURL(LimSystem.LanotaliumServer + "/lanotalium/docs/Tutorial_" + (LimSystem.Preferences.LanguageName == "简体中文" ? "ZhCn" : "En") + ".pdf");
    }
    public void ToChartZone()
    {
        SceneManager.LoadScene("LimChartZone");
    }
}
