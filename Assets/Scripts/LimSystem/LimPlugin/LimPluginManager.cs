using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using UnityEngine.UI;

public class LimPluginManager : MonoBehaviour
{
    [HideInInspector]
    public bool isPluginLoadFinished = false;
    public LimDialogUtils DialogUtils;
    public Transform PluginListTransform;
    public RectTransform PluginListContent;
    public GameObject PluginPrefab;
    public GameObject PluginCenterGameObject;
    public Text PluginCenterLabel;

    private bool HavePluginDownloading = false;
    private string LanotaliumDataPath;
    private string LanotaliumServerDataPath = "http://www.lanotalium.cn/lanotalium/plugin";
    private List<Lanotalium.Plugin.PluginContainer> Plugins = new List<Lanotalium.Plugin.PluginContainer>();
    private List<GameObject> PluginInstances = new List<GameObject>();

    private void Start()
    {
        LanotaliumDataPath = Application.dataPath;
        if (!Directory.Exists(Application.streamingAssetsPath + "/Plugins")) Directory.CreateDirectory(Application.streamingAssetsPath + "/Plugins");
        StartCoroutine(LoadPlugins());
    }
    public void SetTexts()
    {
        PluginCenterLabel.text = LimLanguageManager.TextDict["Plugin_Center_Label"];
    }
    private string GetSha1Hash(string Path)
    {
        string strResult = "";
        string strHashData = "";
        byte[] arrbytHashValue;
        FileStream oFileStream = null;
        System.Security.Cryptography.SHA1CryptoServiceProvider osha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
        try
        {
            oFileStream = new FileStream(Path.Replace("\"", ""), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            arrbytHashValue = osha1.ComputeHash(oFileStream);
            oFileStream.Close();
            strHashData = BitConverter.ToString(arrbytHashValue);
            strHashData = strHashData.Replace("-", "");
            strResult = strHashData;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return strResult;
    }
    private void ValidateAllPluginsAsync()
    {
        new Thread(new ThreadStart(() =>
        {
            foreach (Lanotalium.Plugin.PluginContainer Plugin in Plugins)
            {
                Plugin.isValid = true;
                foreach (Lanotalium.Plugin.PluginDepend Depend in Plugin.Depends)
                {
                    if (!File.Exists(LanotaliumDataPath + Depend.RelativelyPath))
                    {
                        Plugin.isValid = false;
                        break;
                    }
                    if (GetSha1Hash(LanotaliumDataPath + Depend.RelativelyPath) != Depend.Sha1Hash)
                    {
                        Plugin.isValid = false;
                        break;
                    }
                }
            }
            isPluginLoadFinished = true;
        })).Start();
    }
    private bool ValidateDepend(Lanotalium.Plugin.PluginDepend Depend)
    {
        if (!File.Exists(LanotaliumDataPath + Depend.RelativelyPath))
        {
            return false;
        }
        if (GetSha1Hash(LanotaliumDataPath + Depend.RelativelyPath) != Depend.Sha1Hash)
        {
            return false;
        }
        return true;
    }
    private IEnumerator LoadPlugins()
    {
        yield return StartCoroutine(DownloadPluginList());
        if (!File.Exists(Application.streamingAssetsPath + "/Plugins/PluginList.json")) yield break;
        string PluginListString = File.ReadAllText(Application.streamingAssetsPath + "/Plugins/PluginList.json");
        Plugins = JsonConvert.DeserializeObject<List<Lanotalium.Plugin.PluginContainer>>(PluginListString);
        if (Plugins == null) Plugins = new List<Lanotalium.Plugin.PluginContainer>();
        ValidateAllPluginsAsync();
    }
    private IEnumerator DownloadPluginList()
    {
        WWW DownloadPluginList = new WWW("http://www.lanotalium.cn/lanotalium/plugin" + "/PluginList.json");
        yield return DownloadPluginList;
        if (DownloadPluginList != null && string.IsNullOrEmpty(DownloadPluginList.error))
        {
            File.WriteAllText(Application.streamingAssetsPath + "/Plugins/PluginList.json", DownloadPluginList.text);
        }
    }
    private IEnumerator DownloadPlugin(string Name, Button Caller = null)
    {
        HavePluginDownloading = true;
        Lanotalium.Plugin.PluginContainer Plugin = FindPluginContainerByName(Name);
        if (Plugin == null)
        {
            DialogUtils.MessageBox.ShowMessage(LimLanguageManager.TextDict["Plugin_NotFound"]);
            yield break;
        }
        bool isDownloadFinished = false;
        float Progress = 0;
        float ProgressDelta = Plugin.Depends.Count == 0 ? 1 : 1f / Plugin.Depends.Count;
        int FileDownloadedCount = 0;
        DialogUtils.ProgressBar.ShowProgress(() => { return isDownloadFinished; }, () => { return Progress; });
        foreach (Lanotalium.Plugin.PluginDepend Depend in Plugin.Depends)
        {
            if (!ValidateDepend(Depend))
            {
                WWW DownloadDepend = new WWW(LanotaliumServerDataPath + Depend.RelativelyPath);
                while (!DownloadDepend.isDone)
                {
                    Progress = FileDownloadedCount * ProgressDelta + ProgressDelta * DownloadDepend.progress;
                    yield return null;
                }
                if (DownloadDepend != null && string.IsNullOrEmpty(DownloadDepend.error))
                {
                    File.WriteAllBytes(LanotaliumDataPath + Depend.RelativelyPath, DownloadDepend.bytes);
                }
            }
            FileDownloadedCount++;
        }
        Progress = 1;
        isDownloadFinished = true;
        HavePluginDownloading = false;
        if (Caller != null) Caller.GetComponentInChildren<Text>().text = LimLanguageManager.TextDict["Plugin_Complete"];
        ValidateAllPluginsAsync();
    }
    private Lanotalium.Plugin.PluginContainer FindPluginContainerByName(string Name)
    {
        foreach (Lanotalium.Plugin.PluginContainer Plugin in Plugins)
        {
            if (Plugin.Name == Name) return Plugin;
        }
        return null;
    }
    public bool CheckPluginValid(string Name)
    {
        if (!isPluginLoadFinished)
        {
            DialogUtils.MessageBox.ShowMessage(LimLanguageManager.TextDict["Plugin_LoadNotComplete"]);
            return false;
        }
        Lanotalium.Plugin.PluginContainer Plugin = FindPluginContainerByName(Name);
        if (Plugin == null)
        {
            DialogUtils.MessageBox.ShowMessage(LimLanguageManager.TextDict["Plugin_NotFound"]);
            return false;
        }
        if (!Plugin.isValid)
        {
            DialogUtils.MessageBox.ShowMessage(LimLanguageManager.TextDict["Plugin_NotValid"]);
            return false;
        }
        return true;
    }

    public void OpenPluginCenter()
    {
        if (!isPluginLoadFinished)
        {
            DialogUtils.MessageBox.ShowMessage(LimLanguageManager.TextDict["Plugin_LoadNotComplete"]);
            return;
        }
        PluginCenterGameObject.SetActive(true);
        InstantiatePluginList();
    }
    private void InstantiatePluginList()
    {
        foreach (GameObject PluginInstance in PluginInstances) Destroy(PluginInstance);
        PluginInstances.Clear();
        float Height = -30;
        foreach (Lanotalium.Plugin.PluginContainer Plugin in Plugins)
        {
            GameObject PluginInstance = Instantiate(PluginPrefab, PluginListTransform);
            PluginInstance.GetComponentInChildren<Text>().text = Plugin.Name;
            Button DownloadBtn = PluginInstance.GetComponentInChildren<Button>();
            DownloadBtn.GetComponentInChildren<Text>().text = LimLanguageManager.TextDict["Plugin_Download"];
            DownloadBtn.onClick.AddListener(() => { if (HavePluginDownloading) return; StartCoroutine(DownloadPlugin(Plugin.Name, DownloadBtn)); DownloadBtn.interactable = false; });
            PluginInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Height);
            PluginInstances.Add(PluginInstance);
            Height -= 50;
        }
        PluginListContent.sizeDelta = new Vector2(0, -Height);
    }
}