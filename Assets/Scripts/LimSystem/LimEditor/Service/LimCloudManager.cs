using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lanotalium.Service.Cloud;
using System.IO;
using System;
using System.Windows.Forms;
using System.Diagnostics;

public class LimCloudManager : MonoBehaviour
{
    public LimTunerManager TunerManager;
    public Slider ProgressSlider;
    public Text EntryText, ErrorText, ProjectNameText, ChartLabelText, ChartLastMTimeText, CloudAutosaveText;
    public Text BackupText, BackupTimeText;
    public GameObject CloudPanel, ErrorPage;
    public Toggle CloudAutosaveToggle;

    private Status Status
    {
        get
        {
            if (UserId == SystemInfo.unsupportedIdentifier) return Status.UnsupportUserId;
            if (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable) return Status.NetworkNotReachable;
            if (LimSystem.ChartContainer == null || !TunerManager.isInitialized) return Status.NoProjectLoaded;
            return Status.Running;
        }
    }
    private string UserId
    {
        get
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
    private bool isUploading;
    private Coroutine CloudAutosaveCoroutineRef, BackupLoopCr;

    private void Start()
    {
        CloudAutosaveToggle.isOn = LimSystem.Preferences.CloudAutosave;
        if (LimSystem.Preferences.CloudAutosave)
        {
            if (CloudAutosaveCoroutineRef != null) StopCoroutine(CloudAutosaveCoroutineRef);
            CloudAutosaveCoroutineRef = StartCoroutine(CloudAutosaveCoroutine());
        }
        if (UnityEngine.Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (BackupLoopCr != null) StopCoroutine(BackupLoopCr);
            BackupLoopCr = StartCoroutine(BackupLoop());
        }
    }
    public void SetTexts()
    {
        EntryText.text = LimLanguageManager.TextDict["Cloud_Cloud"];
        ChartLabelText.text = LimLanguageManager.TextDict["Cloud_Chart_Label"];
        CloudAutosaveText.text = LimLanguageManager.TextDict["Cloud_CloudAutosave"];
        BackupText.text = LimLanguageManager.TextDict["Cloud_Backup"];
    }

    IEnumerator CloudAutosaveCoroutine()
    {
        while (true)
        {
            UploadChart();
            yield return new WaitForSeconds(30);
        }
    }
    public void OnCloudAutosaveToggleChange()
    {
        LimSystem.Preferences.CloudAutosave = CloudAutosaveToggle.isOn;
        if (LimSystem.Preferences.CloudAutosave)
        {
            if (CloudAutosaveCoroutineRef != null) StopCoroutine(CloudAutosaveCoroutineRef);
            CloudAutosaveCoroutineRef = StartCoroutine(CloudAutosaveCoroutine());
        }
        else
        {
            if (CloudAutosaveCoroutineRef != null) StopCoroutine(CloudAutosaveCoroutineRef);
        }
    }

    private bool CheckStatusError()
    {
        switch (Status)
        {
            case Status.Running: return true;
            case Status.UnsupportUserId: ErrorText.text = LimLanguageManager.TextDict["Cloud_Error_UnsupportUserId"]; return false;
            case Status.NetworkNotReachable: ErrorText.text = LimLanguageManager.TextDict["Cloud_Error_NetworkNotReachable"]; return false;
            case Status.NoProjectLoaded: ErrorText.text = LimLanguageManager.TextDict["Cloud_Error_NoProjectLoaded"]; return false;
        }
        return true;
    }
    public void OpenCloudPanel()
    {
        if (CloudPanel.activeInHierarchy)
        {
            CloudPanel.SetActive(false);
            return;
        }
        if (!CheckStatusError()) ErrorPage.SetActive(true);
        else
        {
            ErrorPage.SetActive(false);
            ProjectNameText.text = LimSystem.ChartContainer.ChartProperty.ChartName;
            StartCoroutine(GetLastModifyTime());
        }
        CloudPanel.SetActive(true);
    }
    IEnumerator GetLastModifyTimeInternal(string FileName, Text TargetText)
    {
        WWWForm RequestForm = new WWWForm();
        RequestForm.AddField("UserId", UserId);
        RequestForm.AddField("FileName", FileName);
        RequestForm.AddField("ProjectName", LimSystem.ChartContainer.ChartProperty.ChartName);
        WWW GetMTime = new WWW(LimSystem.LanotaliumServer + "/lanotalium/cloud/LimCloudGetMTime.php", RequestForm);
        yield return GetMTime;
        if (GetMTime.text == "Not Uploaded Before") TargetText.text = LimLanguageManager.TextDict["Cloud_GetMTime_NotUploadedBefore"];
        else
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(GetMTime.text + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            TargetText.text = LimLanguageManager.TextDict["Cloud_GetMTime_Label"] + " " + dtStart.Add(toNow).ToString("yyyy/MM/dd HH:mm:ss");
        }
    }
    IEnumerator GetLastModifyTime()
    {
        yield return GetLastModifyTimeInternal("/chart.txt", ChartLastMTimeText);
        yield return GetLastModifyTimeInternal("/backup.txt", BackupTimeText);
    }

    public void UploadChart()
    {
        if (Status != Status.Running) return;
        if (isUploading) return;
        StartCoroutine(UploadCoroutine(TransferType.Chart, null, System.Text.Encoding.Default.GetBytes(LimSystem.ChartContainer.ChartData.ToString())));
        isUploading = true;
    }
    IEnumerator UploadCoroutine(TransferType Type, string LocalPath = null, byte[] Bytes = null)
    {
        if (Status != Status.Running) yield break;
        if (LocalPath == null && Bytes == null) yield break;
        ProgressSlider.value = 0;
        switch (Type)
        {
            case TransferType.Chart:
            case TransferType.Music:
                EntryText.text = LimLanguageManager.TextDict["Cloud_Uploading"]; ; break;
            case TransferType.Backup:
                EntryText.text = LimLanguageManager.TextDict["Cloud_Backuping"]; ; break;
        }
        string FileName = string.Empty;
        byte[] FileBytes = Bytes == null ? File.ReadAllBytes(LocalPath) : Bytes;
        switch (Type)
        {
            case TransferType.Chart: FileName = "chart.txt"; break;
            case TransferType.Music: FileName = "music.ogg"; break;
            case TransferType.Backup: FileName = "backup.txt"; break;
        }
        WWWForm UploadForm = new WWWForm();
        UploadForm.AddField("UserId", UserId);
        UploadForm.AddField("FileName", FileName);
        UploadForm.AddField("ProjectName", LimSystem.ChartContainer.ChartProperty.ChartName);
        UploadForm.AddBinaryData("upload", FileBytes);
        WWW Upload = new WWW(LimSystem.LanotaliumServer + "/lanotalium/cloud/LimCloudUploader.php", UploadForm);
        while (!Upload.isDone)
        {
            ProgressSlider.value = Upload.uploadProgress;
            yield return null;
        }
        ProgressSlider.value = 0;
        EntryText.text = LimLanguageManager.TextDict["Cloud_Cloud"];
        StartCoroutine(GetLastModifyTime());
        isUploading = false;
    }

    public void DownloadChart()
    {
        if (Status != Status.Running) return;
        StartCoroutine(DownloadCoroutine(TransferType.Chart));
    }
    IEnumerator DownloadCoroutine(TransferType Type)
    {
        WWWForm DownloadForm = new WWWForm();
        DownloadForm.AddField("UserId", UserId);
        switch (Type)
        {
            case TransferType.Backup:
                DownloadForm.AddField("FileName", "backup.txt");
                break;
            case TransferType.Chart:
                DownloadForm.AddField("FileName", "chart.txt");
                break;
        }
        DownloadForm.AddField("ProjectName", LimSystem.ChartContainer.ChartProperty.ChartName);
        WWW Download = new WWW(LimSystem.LanotaliumServer + "/lanotalium/cloud/LimCloudDownloader.php", DownloadForm);
        ProgressSlider.value = 0;
        EntryText.text = LimLanguageManager.TextDict["Cloud_Downloading"];
        while (!Download.isDone)
        {
            ProgressSlider.value = Download.progress;
            yield return null;
        }
        string Chart = Download.text;
        if (Chart == "F A Q!") yield break;

        string ChartPath = LimDialogUtils.SaveFileDialog("", "Chart (*.txt)|*.txt", "");
        if (ChartPath != null)
        {
            File.WriteAllText(ChartPath, Chart);
            Process.Start("explorer.exe", "/select," + ChartPath);
        }
        ProgressSlider.value = 0;
        EntryText.text = LimLanguageManager.TextDict["Cloud_Cloud"];
    }

    IEnumerator BackupLoop()
    {
        while (true)
        {
            BackupChart();
            yield return new WaitForSeconds(45);
        }
    }
    public void BackupChart()
    {
        if (Status != Status.Running) return;
        if (isUploading) return;
        StartCoroutine(UploadCoroutine(TransferType.Backup, null, System.Text.Encoding.Default.GetBytes(LimSystem.ChartContainer.ChartData.ToString())));
        isUploading = true;
    }
    public void DownloadBackup()
    {
        if (Status != Status.Running) return;
        StartCoroutine(DownloadCoroutine(TransferType.Backup));
    }
}
