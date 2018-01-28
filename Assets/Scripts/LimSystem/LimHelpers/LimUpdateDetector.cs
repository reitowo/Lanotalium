using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FileSize
{
    public long B;
    public float KB
    {
        get
        {
            return B / 1024f;
        }
    }
    public float MB
    {
        get
        {
            return KB / 1024f;
        }
    }
    public float GB
    {
        get
        {
            return MB / 1024f;
        }
    }
    public float TB
    {
        get
        {
            return GB / 1024f;
        }
    }
    public string StrB
    {
        get
        {
            return B.ToString() + " B";
        }
    }
    public string StrKB
    {
        get
        {
            return KB.ToString("f2") + " KB";
        }
    }
    public string StrMB
    {
        get
        {
            return MB.ToString("f2") + " MB";
        }
    }
    public string StrGB
    {
        get
        {
            return GB.ToString("f2") + " GB";
        }
    }
    public string StrTB
    {
        get
        {
            return TB.ToString("f2") + " TB";
        }
    }
    public FileSize(long ByteCount)
    {
        B = ByteCount;
    }
}
public class ValueRef<T>
{
    public T Value;
}
public class LimUpdateDetector : MonoBehaviour
{
    public LimProjectManager ProjectManager;
    public RectTransform Banner;
    public Text LabelText, VersionText, WhatsNewText, UpdateText, IgnoreText, FileSizeText;
    private Coroutine BannerAnimationCoroutine;
    private bool DeltaUpdate = false;

    private void Start()
    {
        StartCoroutine(CheckUpdateCoroutine());
    }
    IEnumerator GetWebFileLength(ValueRef<long> Length, string FileUri)
    {
        HttpWebRequest Request = (HttpWebRequest)WebRequest.CreateDefault(new Uri(FileUri));
        Request.Method = "HEAD";
        Request.Timeout = 5000;
        Task<WebResponse> AsyncResponse = Request.GetResponseAsync();
        while (!AsyncResponse.IsCompleted) yield return null;
        try
        {
            HttpWebResponse Response = AsyncResponse.Result as HttpWebResponse;
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                Length.Value = Response.ContentLength;
            }
            else
            {
                Length.Value = -1;
            }
            Response.Close();
        }
        catch(Exception)
        {
            Length.Value = -1;
        }
    }
    IEnumerator CheckUpdateCoroutine()
    {
        LimSystem.LanotaliumServer = RemoteSettings.GetString("LanotaliumServer", "http://lanotalium.cn");
        WWW CheckUpdate = new WWW(LimSystem.LanotaliumServer + "/lanotalium/build.txt");
        yield return CheckUpdate;
        if (CheckUpdate != null && string.IsNullOrEmpty(CheckUpdate.error))
        {
            string[] Splited = CheckUpdate.text.Split(' ');
            int LatestBuild = 0;
            if (!int.TryParse(Splited[0], out LatestBuild)) yield break;
            string LatestVersion = Splited[1];
            VersionText.text = LatestVersion;
            if (LatestBuild > LimSystem.Build)
            {
                #region Load Whatsnew
                WWW LoadWhatsnew = new WWW(LimSystem.LanotaliumServer + (LimSystem.Preferences.LanguageName == "简体中文" ? "/lanotalium/whatsnew-zhcn.txt" : "/lanotalium/whatsnew-en.txt"));
                yield return LoadWhatsnew;
                if (LoadWhatsnew != null && string.IsNullOrEmpty(LoadWhatsnew.error))
                {
                    WhatsNewText.text = LoadWhatsnew.text;
                }
                #endregion
                #region Get Filesize
                ValueRef<long> Filesize = new ValueRef<long>();
                yield return GetWebFileLength(Filesize, LimSystem.LanotaliumServer + "/lanotalium/updator/patch/" + LimSystem.Version + ".lapatch");
                if (Filesize.Value == -1 || !Environment.Is64BitProcess)
                {
                    ValueRef<long> FullFilesize = new ValueRef<long>();
                    yield return GetWebFileLength(FullFilesize, LimSystem.LanotaliumServer + "/lanotalium/full/Lanotalium_Latest.zip");
                    FileSizeText.text = string.Format(LimLanguageManager.TextDict["Updator_FullFileSize"], new FileSize(FullFilesize.Value).StrMB);
                }
                else
                {
                    FileSizeText.text = string.Format(LimLanguageManager.TextDict["Updator_DeltaFileSize"], new FileSize(Filesize.Value).StrKB);
                    DeltaUpdate = true;
                }
                #endregion
                StartBannerAnimation(true);
            }
        }
    }
    public void SetTexts()
    {
        LabelText.text = LimLanguageManager.TextDict["Updator_Update"];
        UpdateText.text = LimLanguageManager.TextDict["Updator_DoUpdate"];
        IgnoreText.text = LimLanguageManager.TextDict["Updator_Ignore"];
    }
    public void DownloadUpdate()
    {
        ProjectManager.SaveProject();
        if (DeltaUpdate)
        {
            if (!File.Exists(Application.streamingAssetsPath + "/Updator/Newtonsoft.Json.dll"))
                File.Copy(Application.dataPath + "/Managed/Newtonsoft.Json.dll", Application.streamingAssetsPath + "/Updator/Newtonsoft.Json.dll", true);
            ProcessStartInfo Updator = new ProcessStartInfo
            {
                FileName = Application.streamingAssetsPath + "/Updator/LanotaliumUpdateClient.exe",
                Arguments = string.Format("{0} {1} {2}", Directory.GetParent(Application.dataPath).FullName.Replace(" ", "%20"), LimSystem.Version, "http://lanotalium.cn/lanotalium/updator")
            };
            Process.Start(Updator);
            Process.GetCurrentProcess().Kill();
        }
        else
        {
            Application.OpenURL("https://github.com/cnSchwarzer/Lanotalium/releases");
        }
    }
    public void StartBannerAnimation(bool Show)
    {
        if (BannerAnimationCoroutine != null) StopCoroutine(BannerAnimationCoroutine);
        if (Show) BannerAnimationCoroutine = StartCoroutine(ShowBannerCoroutine());
        else BannerAnimationCoroutine = StartCoroutine(HideBannerCoroutine());
    }
    IEnumerator ShowBannerCoroutine()
    {
        Banner.gameObject.SetActive(true);
        float Start = Banner.sizeDelta.y;
        float Delta = 600 - Start;
        float Duration = 0.3f;
        while (Duration > 0)
        {
            Banner.sizeDelta = new Vector2(Banner.sizeDelta.x, Start + Delta * (0.3f - Duration) / 0.3f);
            Duration -= Time.deltaTime;
            yield return null;
        }
        Banner.sizeDelta = new Vector2(Banner.sizeDelta.x, 600);
    }
    IEnumerator HideBannerCoroutine()
    {
        float Start = Banner.sizeDelta.y;
        float Delta = 0 - Start;
        float Duration = 0.3f;
        while (Duration > 0)
        {
            Banner.sizeDelta = new Vector2(Banner.sizeDelta.x, Start + Delta * (0.3f - Duration) / 0.3f);
            Duration -= Time.deltaTime;
            yield return null;
        }
        Banner.sizeDelta = new Vector2(Banner.sizeDelta.x, 0);
        Banner.gameObject.SetActive(false);
    }
}
