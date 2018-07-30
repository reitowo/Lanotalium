using Lanotalium.Plugin;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class LimPlugin
{
    public Assembly Assembly;
    public ILanotaliumPlugin Interface;
}
public class PluginExecutionException : Exception
{
    public PluginExecutionException(Exception exception) : base("当前插件执行出现异常", exception)
    {

    }
}
public class LimPluginManager : MonoBehaviour
{
    public string PluginPath
    {
        get
        {
            return Application.streamingAssetsPath + "/Plugins";
        }
    }
    public LanotaliumContext Context
    {
        get
        {
            return new LanotaliumContext()
            {
                IsProjectLoaded = (LimProjectManager.CurrentProject != null && LimTunerManager.Instance.isInitialized),
                CurrentLanguage = LimLanguageManager.CurrentLanguage == "简体中文" ? Language.简体中文 : Language.English,
                EditorManager = LimEditorManager.Instance,
                TunerManager = LimTunerManager.Instance,
                OperationManager = LimOperationManager.Instance,
                UserRequest = EasyRequest.EasyRequestManager.Instance,
                MessageBox = MessageBoxManager.Instance
            };
        }
    }
    public Language LanguageCode
    {
        get
        {
            return LimLanguageManager.CurrentLanguage == "简体中文" ? Language.简体中文 : Language.English;
        }
    }

    private List<LimPlugin> plugins = new List<LimPlugin>();
    private List<GameObject> pluginGameObjects = new List<GameObject>();

    public RectTransform PluginView, PluginListContent;
    public Text TitleText, DescriptionText;
    public GameObject PluginPrefab;

    private void Start()
    {
        Refresh();
    }

    public void SwitchPanelActive()
    {
        PluginView.gameObject.SetActive(!PluginView.gameObject.activeInHierarchy);
        DescriptionText.text = LimLanguageManager.TextDict["Plugin_Title_Default"];
    }
    public void Refresh()
    {
        CleanUp();
        ScanPlugins();
        MakePluginList();
        DescriptionText.text = LimLanguageManager.TextDict["Plugin_Title_Default"];
    }

    private void CleanUp()
    {
        plugins.Clear();
        foreach (var g in pluginGameObjects) Destroy(g);
        pluginGameObjects.Clear();
    }
    private void ScanPlugins()
    {
        string[] files = Directory.GetFiles(PluginPath);
        foreach (string file in files)
        {
            try
            {
                Assembly a = Assembly.LoadFile(file);
                foreach (Type t in a.GetTypes())
                {
                    Type i = t.GetInterface("ILanotaliumPlugin");
                    if (i == null) continue;
                    ILanotaliumPlugin p = Activator.CreateInstance(t) as ILanotaliumPlugin;
                    plugins.Add(new LimPlugin() { Assembly = a, Interface = p });
                }
            }
            catch (Exception)
            {
                continue;
            }
        }
    }
    private void MakePluginList()
    {
        float y = 0;
        foreach (var i in plugins)
        {
            GameObject g = Instantiate(PluginPrefab, PluginListContent);
            g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y);
            LimPluginComponent c = g.GetComponent<LimPluginComponent>();
            c.Name.text = i.Interface.Name(LanguageCode);
            c.Interface = i.Interface;
            c.ShowDescription.onClick.AddListener(() => ShowDescription(c.Interface));
            c.Execute.onClick.AddListener(() => Execute(c.Interface));
            y -= 50;
        }
        PluginListContent.sizeDelta = new Vector2(0, -y);
    }
    private void ShowDescription(ILanotaliumPlugin plugin)
    {
        DescriptionText.text = plugin.Description(LanguageCode);
    }

    private IEnumerator pluginProcess = null;
    private bool ExecutePlugin(IEnumerator enumerator)
    {
        if (enumerator.Current is IEnumerator)
        {
            if (ExecutePlugin(enumerator.Current as IEnumerator))
                return true;
        }
        return enumerator.MoveNext();
    }
    private void Update()
    {
        if (pluginProcess == null) return;
        try
        {
            if (!ExecutePlugin(pluginProcess))
            {
                pluginProcess = null;
            }
        }
        catch (Exception Ex)
        {
            pluginProcess = null;
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Plugin_Exception"]);
            File.WriteAllText(Application.dataPath + "/Plugin_Exception_" + ((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds).ToString() + ".txt", Ex.ToString());
        }
    }
    private void Execute(ILanotaliumPlugin plugin)
    {
        pluginProcess = plugin.Process(Context);
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}
