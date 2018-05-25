using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LayestaDependsDownloader : MonoBehaviour
{
    public GameObject Panel;
    public Text Hint, DownloadText;
    public Button Download;
    public Slider Progress;

    private static LayestaDependsDownloader instance;
    public static LayestaDependsDownloader Instance
    {
        get
        {
            return instance;
        }
    }

    private void Start()
    {
        instance = this;
        LimLanguageManager.OnLanguageChanged.AddListener(SetTexts);
    }
    private void OnDestroy()
    {
        LimLanguageManager.OnLanguageChanged.RemoveListener(SetTexts);
    }
    public void SetTexts()
    {
        Hint.text = LimLanguageManager.TextDict["Layesta_Download_Hint"];
        DownloadText.text = LimLanguageManager.TextDict["Layesta_Download"];
    }
    public void Show()
    {
        Panel.SetActive(true);
    }
    public void StartDownload()
    {
        Download.interactable = false;
        StartCoroutine(DownloadCoroutine());
    }
    IEnumerator DownloadCoroutine()
    {
        UnityWebRequest web = new UnityWebRequest("https://gitee.com/Schwarzer/Layesta/raw/master/Tools/ffmpeg.zip")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
        var w = web.SendWebRequest();
        while (!w.isDone)
        {
            Progress.value = (float)(web.downloadedBytes / 17710286.0);
            yield return null;
        }
        if (web.error != null || web.downloadHandler == null || web.downloadHandler.data == null)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Error_Network"]);
            Download.interactable = true;
            Progress.value = 0;
            yield break;
        }
        Progress.value = 1;
        byte[] ffmpeg = lzip.entry2Buffer("", "ffmpeg.exe", web.downloadHandler.data);
        if (ffmpeg == null)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Error_Network"]);
            Download.interactable = true;
            Progress.value = 0;
            yield break;
        }
        File.WriteAllBytes(LimLayestaManager.Instance.FFMpegPath, ffmpeg);
        LimLayestaManager.Instance.CheckDepends();
        Panel.SetActive(false);
    }
}
