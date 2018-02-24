using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LimLaunchManager : MonoBehaviour
{
    public Text Message, VersionText;
    public Slider EnterLanotaliumSlider;
    public Text EnterLanotaliumText, TutorialText, SupportMeText, QuitText, ChartZoneText;
    public GameObject SupportMePanel;
    //public Dropdown PaypalQuantityDropdown;

    private Dictionary<string, string> _LaunchLanguageDict = new Dictionary<string, string>();

    private void Start()
    {
        VersionText.text = LimSystem.Version;
        SetLanguageDict();
        SetTexts();
        StartCoroutine(LoadNotification());
        LimSystem.LanotaliumServer = RemoteSettings.GetString("LanotaliumServer", "http://lanotalium.cn");
    }
    private void SetLanguageDict()
    {
        _LaunchLanguageDict.Add("EnterLanotalium_ZhCn", "<size=30>进入</size>  <size=45>Lanotalium</size>");
        _LaunchLanguageDict.Add("EnterLanotalium_En", "<size=30>Enter</size>  <size=45>Lanotalium</size>");
        _LaunchLanguageDict.Add("Tutorial_ZhCn", "教程");
        _LaunchLanguageDict.Add("Tutorial_En", "Tutorial");
        _LaunchLanguageDict.Add("SupportMe_ZhCn", "支持");
        _LaunchLanguageDict.Add("SupportMe_En", "Donate");
        _LaunchLanguageDict.Add("About_ZhCn", "关于");
        _LaunchLanguageDict.Add("About_En", "About");
        _LaunchLanguageDict.Add("Quit_ZhCn", "退出");
        _LaunchLanguageDict.Add("Quit_En", "Quit");
        _LaunchLanguageDict.Add("ChartZone_ZhCn", "谱面中心");
        _LaunchLanguageDict.Add("ChartZone_En", "ChartZone");
    }
    private void SetTexts()
    {
        if (LimSystem.Preferences.LanguageName == "简体中文")
        {
            ChartZoneText.text = _LaunchLanguageDict["ChartZone_ZhCn"];
            EnterLanotaliumText.text = _LaunchLanguageDict["EnterLanotalium_ZhCn"];
            TutorialText.text = _LaunchLanguageDict["Tutorial_ZhCn"];
            SupportMeText.text = _LaunchLanguageDict["SupportMe_ZhCn"];
            QuitText.text = _LaunchLanguageDict["Quit_ZhCn"];
        }
        else
        {
            ChartZoneText.text = _LaunchLanguageDict["ChartZone_En"];
            EnterLanotaliumText.text = _LaunchLanguageDict["EnterLanotalium_En"];
            TutorialText.text = _LaunchLanguageDict["Tutorial_En"];
            SupportMeText.text = _LaunchLanguageDict["SupportMe_En"];
            QuitText.text = _LaunchLanguageDict["Quit_En"];
        }
    }
    public void OpenSupportMePanel()
    {
        if (SupportMePanel.activeInHierarchy) SupportMePanel.SetActive(false);
        else SupportMePanel.SetActive(true);
    }
    public void QuitFromMenu()
    {
        UnityEngine.Application.Quit();
    }
    IEnumerator LoadNotification()
    {
        if (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable)
        {
            Message.text = (LimSystem.Preferences.LanguageName == "简体中文" ? "网络不可用" : "Network Unavaliable.");
            yield break;
        }
        WWW LoadNotification = new WWW(LimSystem.LanotaliumServer + (LimSystem.Preferences.LanguageName == "简体中文" ? "/lanotalium/notification-zhcn.txt" : "/lanotalium/notification-en.txt"));
        yield return LoadNotification;
        if (LoadNotification != null && string.IsNullOrEmpty(LoadNotification.error))
        {
            Message.text = LoadNotification.text;
        }
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
        LimChartZoneManager.FromScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene("LimChartZone");
    }
    public void RaisePaypalRequest()
    {
        Application.OpenURL("http://lanotalium.cn/PaypalSupport.html");
    }
}
