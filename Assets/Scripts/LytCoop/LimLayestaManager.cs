using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Linq;

public class LimLayestaManager : MonoBehaviour
{
    public GameObject SendPanel;
    public Button SendToButton;
    public Toggle OverwriteToggle;
    public Text Hint, SendText, OverwriteText, Status, BuildTitle;
    public Text BuildName, BuildArtist, BuildDesigner, BuildChart, BuildChartAddFile, BuildChartAddThis, BuildBuild;
    public InputField Name, Artist, Designer;
    public GameObject LayestaPanel, DevicePrefab;
    public RectTransform DevicesContent;
    public GameObject BuildChartPrefab;
    public RectTransform BuildChartListContent;
    public Button AddInstanceButton;
    private bool hasDepends = false;

    public bool Debug = false;
    public string FFMpegPath
    {
        get
        {
            if (Application.platform == RuntimePlatform.WindowsEditor) return "H:/ffmpeg.exe";
            return Application.streamingAssetsPath + "/Plugins/ffmpeg.exe";
        }
    }
    private static LimLayestaManager instance;
    public static LimLayestaManager Instance
    {
        get
        {
            return instance;
        }
    }

    private List<LimLayestaDevice> deviceViews = new List<LimLayestaDevice>();
    private LayestaDevice deviceSendTo;

    private List<LimLayestaBuildChart> buildChartViews = new List<LimLayestaBuildChart>();

    private void Start()
    {
        instance = this;
        LimLanguageManager.OnLanguageChanged.AddListener(SetTexts);
        CheckDepends();
        buildChartViews.Clear();
    }
    private void OnDestroy()
    {
        LimLanguageManager.OnLanguageChanged.RemoveListener(SetTexts);
        GenerateDeviceList(0);
    }

    private void Update()
    {
        UpdateDeviceViews();
        try
        {
            sendCoroutine?.MoveNext();
        }
        catch (Exception)
        {
            SendToButton.interactable = true;
            Status.text = "";
            sendCoroutine = null;
        }
    }
    public void SetTexts()
    {
        Hint.text = LimLanguageManager.TextDict["Layesta_Send_Hint"];
        SendText.text = LimLanguageManager.TextDict["Layesta_Send"];
        OverwriteText.text = LimLanguageManager.TextDict["Layesta_Overwrite"];
        BuildTitle.text = LimLanguageManager.TextDict["Layesta_Build_Title"];
        BuildName.text = LimLanguageManager.TextDict["Layesta_Build_Name"];
        BuildArtist.text = LimLanguageManager.TextDict["Layesta_Build_Artist"];
        BuildDesigner.text = LimLanguageManager.TextDict["Layesta_Build_Designer"];
        BuildChart.text = LimLanguageManager.TextDict["Layesta_Build_Chart"];
        BuildChartAddThis.text = LimLanguageManager.TextDict["Layesta_Build_Chart_AddThis"];
        BuildChartAddFile.text = LimLanguageManager.TextDict["Layesta_Build_Chart_AddFile"];
        BuildBuild.text = LimLanguageManager.TextDict["Layesta_Build"];
    }
    public void GenerateDeviceList(int Count)
    {
        if (deviceViews.Count == Count) return;
        int origin = deviceViews.Count;
        if (origin > Count)
        {
            for (int i = 0; i < origin - Count; ++i)
            {
                Destroy(deviceViews[0].gameObject);
                deviceViews.RemoveAt(0);
            }
        }
        else
        {
            for (int i = 0; i < Count - origin; ++i)
            {
                LimLayestaDevice l = Instantiate(DevicePrefab, DevicesContent).GetComponent<LimLayestaDevice>();
                deviceViews.Add(l);
            }
        }
        float height = 0;
        foreach (LimLayestaDevice l in deviceViews)
        {
            l.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, height);
            height -= 50;
        }
        DevicesContent.sizeDelta = new Vector2(0, -height);
    }
    public void UpdateDeviceViews()
    {
        List<LayestaDevice> l = LayestaCoopServer.Instance.Devices;
        lock (l)
        {
            GenerateDeviceList(l.Count);
            for (int i = 0; i < deviceViews.Count; ++i)
            {
                deviceViews[i].Init(l[i]);
            }
        }
    }
    public void SwitchPanelActive()
    {
        if (!hasDepends)
        {
            LayestaDependsDownloader.Instance.Show();
            return;
        }
        LayestaPanel.SetActive(!LayestaPanel.activeInHierarchy);
        if (!LimTunerManager.Instance.isInitialized)
        {
            return;
        }
        if (LayestaPanel.activeInHierarchy)
        {
            if (buildChartViews.Count == 0) BuildAddChartInstance();
            Name.text = LimProjectManager.CurrentProject.Name;
            Designer.text = LimProjectManager.CurrentProject.Designer;
        }
    }
    public void Play(LayestaDevice device)
    {
        if (!LimTunerManager.Instance.isInitialized)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Layesta_NoProject"]);
            return;
        }
        deviceSendTo = device;
        SendPanel.SetActive(true);
    }
    public void ResetSend()
    {
        if (sendCoroutine != null) StopCoroutine(sendCoroutine);
        SendToButton.interactable = true;
        Status.text = "";
    }

    private List<string> filesToZip = new List<string>();
    private void EncodeBackgrounds(bool overwrite)
    {
        if (LimSystem.ChartContainer.ChartLoadResult.isBackgroundLoaded)
        {
            filesToZip.Add(LimProjectManager.LapFolder + "/Layesta/background.jpg");
            if (!File.Exists(LimProjectManager.LapFolder + "/Layesta/background.jpg") || overwrite)
                File.WriteAllBytes(LimProjectManager.LapFolder + "/Layesta/background.jpg", LimSystem.ChartContainer.ChartBackground.Color.texture.EncodeToJPG());
        }
        if (LimSystem.ChartContainer.ChartLoadResult.isBackgroundGrayLoaded)
        {
            filesToZip.Add(LimProjectManager.LapFolder + "/Layesta/background_gray.jpg");
            if (!File.Exists(LimProjectManager.LapFolder + "/Layesta/background_gray.jpg") || overwrite)
                File.WriteAllBytes(LimProjectManager.LapFolder + "/Layesta/background_gray.jpg", LimSystem.ChartContainer.ChartBackground.Gray.texture.EncodeToJPG());
        }
        if (LimSystem.ChartContainer.ChartLoadResult.isBackgroundLinearLoaded)
        {
            filesToZip.Add(LimProjectManager.LapFolder + "/Layesta/background_linear.jpg");
            if (!File.Exists(LimProjectManager.LapFolder + "/Layesta/background_linear.jpg") || overwrite)
                File.WriteAllBytes(LimProjectManager.LapFolder + "/Layesta/background_linear.jpg", LimSystem.ChartContainer.ChartBackground.Linear.texture.EncodeToJPG());
        }
    }
    private async Task<string> CalculateMD5Async(string path)
    {
        return await Task.Run(() =>
        {
            MD5 m = MD5.Create();
            FileStream fs = File.Open(path, FileMode.Open);
            byte[] hash = m.ComputeHash(fs);
            fs.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        });
    }
    private async Task ConvertAudioAsync(bool overwrite)
    {
        await Task.Run(() =>
        {
            string outPath = LimProjectManager.LapFolder + (Debug ? "/Layesta/music.ogg" : "/Layesta/music.mp3");
            filesToZip.Add(outPath);
            if (File.Exists(outPath) && !overwrite) return;
            string midPath = LimProjectManager.LapFolder + "/temp.wav";
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = FFMpegPath,
                Arguments = $"-i \"{LimProjectManager.CurrentProject.MusicPath}\" -y \"{midPath}\"",
                //WindowStyle = ProcessWindowStyle.Hidden
            };
            Process p = Process.Start(info);
            p.WaitForExit();
            info = new ProcessStartInfo
            {
                FileName = FFMpegPath,
                Arguments = $"-i \"{midPath}\" -y -b:a 192k -ar 44100 -ac 2 \"{outPath}\"",
                //WindowStyle = ProcessWindowStyle.Hidden
            };
            p = Process.Start(info);
            p.WaitForExit();
            File.Delete(midPath);
        });
    }
    private async Task ConvertBackgroundBlurAsync(bool overwrite)
    {
        await Task.Run(() =>
        {
            filesToZip.Add(LimProjectManager.LapFolder + "/Layesta/background_blur.png");
            if (File.Exists(LimProjectManager.LapFolder + "/Layesta/background_blur.png") && !overwrite) return;
            Kaliko.ImageLibrary.KalikoImage k = new Kaliko.ImageLibrary.KalikoImage(LimProjectManager.LapFolder + "/Layesta/background.jpg");
            k = k.Scale(new Kaliko.ImageLibrary.Scaling.PadScaling((int)(64f / k.Height * k.Width), 64));
            k.ApplyFilter(new Kaliko.ImageLibrary.Filters.GaussianBlurFilter(5));
            k.SavePng(LimProjectManager.LapFolder + "/Layesta/background_blur.png");
        });
    }
    private async Task ConvertChartAsync()
    {
        await Task.Run(() =>
        {
            foreach (LimLayestaBuildChart c in buildChartViews)
            {
                if (string.IsNullOrWhiteSpace(c.Difficulty)) continue;
                if (c.ChartPath != "Instance")
                {
                    File.Copy(c.ChartPath, LimProjectManager.LapFolder + $"/Layesta/chart_{Convert.ToBase64String(Encoding.UTF8.GetBytes(c.Difficulty))}.txt", true);
                }
                else
                {
                    File.WriteAllText(LimProjectManager.LapFolder + $"/Layesta/chart_{Convert.ToBase64String(Encoding.UTF8.GetBytes(c.Difficulty))}.txt", LimTunerManager.Instance.ChartContainer.ChartData.ToString());
                }
                filesToZip.Add(LimProjectManager.LapFolder + $"/Layesta/chart_{Convert.ToBase64String(Encoding.UTF8.GetBytes(c.Difficulty))}.txt");
            }
        });
    }
    private async Task PackLayestaFileAsync()
    {
        await Task.Run(() =>
        {
            FileStream fs = new FileStream(LimProjectManager.LapFolder + "/Layesta/info.bytes", FileMode.Create, FileAccess.Write);
            BinaryWriter b = new BinaryWriter(fs);
            b.Write(Name.text);
            b.Write(Designer.text);
            b.Write(buildChartViews.Count);
            foreach (LimLayestaBuildChart c in buildChartViews)
            {
                b.Write(c.Difficulty);
            }
            Kaliko.ImageLibrary.KalikoImage k = new Kaliko.ImageLibrary.KalikoImage(LimProjectManager.LapFolder + "/Layesta/background.jpg");
            b.Write(k.Width);
            b.Write(k.Height);
            b.Write(Artist.text);
            b.Write(2);
            b.Close();
            fs.Close();
            filesToZip.Add(LimProjectManager.LapFolder + "/Layesta/info.bytes");
            lzip.compress_File_List(0, LimProjectManager.LapFolder + "/instance.layesta", filesToZip.ToArray(), null, false, filesToZip.Select((s) => Path.GetFileName(s)).ToArray());
        });
    }
    private async Task PrepareLayestaFileAsync(bool overwrite)
    {
        Directory.CreateDirectory(LimProjectManager.LapFolder + "/Layesta");
        Status.text = LimLanguageManager.TextDict["Layesta_2"];
        await ConvertAudioAsync(overwrite);
        Status.text = LimLanguageManager.TextDict["Layesta_3"];
        await ConvertBackgroundBlurAsync(overwrite);
        Status.text = LimLanguageManager.TextDict["Layesta_4"];
        await ConvertChartAsync();
        await PackLayestaFileAsync();
    }
    private async Task PrepareLayestaFileInstanceAsync(bool overwrite)
    {
        Directory.CreateDirectory(LimProjectManager.LapFolder + "/Layesta");
        Status.text = LimLanguageManager.TextDict["Layesta_2"];
        await ConvertAudioAsync(overwrite);
        Status.text = LimLanguageManager.TextDict["Layesta_3"];
        await ConvertBackgroundBlurAsync(overwrite);
        Status.text = LimLanguageManager.TextDict["Layesta_4"];
        await ConvertChartInstanceAsync();
        await PackLayestaFileInstanceAsync();
    }
    private async Task PackLayestaFileInstanceAsync()
    {
        await Task.Run(() =>
        {
            FileStream fs = new FileStream(LimProjectManager.LapFolder + "/Layesta/info.bytes", FileMode.Create, FileAccess.Write);
            BinaryWriter b = new BinaryWriter(fs);
            b.Write(Name.text);
            b.Write(Designer.text);
            b.Write(1);
            b.Write("Master");
            Kaliko.ImageLibrary.KalikoImage k = new Kaliko.ImageLibrary.KalikoImage(LimProjectManager.LapFolder + "/Layesta/background.jpg");
            b.Write(k.Width);
            b.Write(k.Height);
            b.Write(Artist.text);
            b.Write(2);
            b.Close();
            fs.Close();
            filesToZip.Add(LimProjectManager.LapFolder + "/Layesta/info.bytes");
            lzip.compress_File_List(0, LimProjectManager.LapFolder + "/instance.layesta", filesToZip.ToArray(), null, false, filesToZip.Select((s) => Path.GetFileName(s)).ToArray());
        });
    }
    private async Task ConvertChartInstanceAsync()
    {
        await Task.Run(() =>
        {
            File.WriteAllText(LimProjectManager.LapFolder + $"/Layesta/chart_{Convert.ToBase64String(Encoding.UTF8.GetBytes("Master"))}.txt", LimTunerManager.Instance.ChartContainer.ChartData.ToString());
            filesToZip.Add(LimProjectManager.LapFolder + $"/Layesta/chart_{Convert.ToBase64String(Encoding.UTF8.GetBytes("Master"))}.txt");
        });
    }

    IEnumerator sendCoroutine;
    public void SendLayesta()
    {
        SendToButton.interactable = false;
        sendCoroutine = SendLayestaCoroutine();
    }
    IEnumerator SendLayestaCoroutine()
    {
        filesToZip.Clear();
        Directory.CreateDirectory(LimProjectManager.LapFolder + "/Layesta");
        Status.text = LimLanguageManager.TextDict["Layesta_1"];
        EncodeBackgrounds(OverwriteToggle.isOn);
        Task t = PrepareLayestaFileInstanceAsync(OverwriteToggle.isOn);
        while (!t.IsCompleted) yield return null;
        Status.text = LimLanguageManager.TextDict["Layesta_5"];
        byte[] b = File.ReadAllBytes(LimProjectManager.LapFolder + "/instance.layesta");

        TcpClient client = new TcpClient();
        t = client.ConnectAsync(deviceSendTo.IpEndPoint.Address, LayestaCoopServer.ProxyPort);
        while (!t.IsCompleted) yield return null;
        client.SendTimeout = 1000;
        NetworkStream s = client.GetStream();
        BinaryWriter r = new BinaryWriter(s, Encoding.UTF8, true);
        r.Write(b.Length);
        string filename = Regex.Replace(LimProjectManager.CurrentProject.Name.ToLower(), @"[^a-z0-9]", String.Empty);
        r.Write(filename + "_lanotalium.layesta");
        r.Write(LimTunerManager.Instance.ChartTime);
        r.Close();

        int length = b.Length;
        int blocksize = 32768;
        int blocks = length / blocksize;
        int remain = length % blocksize;

        for (int i = 0; i < blocks; ++i)
        {
            t = s.WriteAsync(b, i * blocksize, blocksize);
            while (!t.IsCompleted) yield return null;
            if (t.Status != TaskStatus.RanToCompletion)
            {
                ResetSend();
                yield break;
            }
            Status.text = LimLanguageManager.TextDict["Layesta_5"] + " (" + (100f * i * blocksize / length).ToString("f2") + "%)";
        }
        t = s.WriteAsync(b, blocks * blocksize, remain);
        while (!t.IsCompleted) yield return null;
        if (t.Status != TaskStatus.RanToCompletion)
        {
            ResetSend();
            yield break;
        }
        s.Close();
        client.Close();

        SendToButton.interactable = true;
        Status.text = "";
        sendCoroutine = null;
    }

    public void BuildLayestaFilePre()
    {
        if (DateTime.Now < new DateTime(2019, 2, 24, 0, 0, 0))
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Layesta_Contest_Warn"], () => BuildLayestaFile());
        }
        else
        {
            BuildLayestaFile();
        }
    }
    public async void BuildLayestaFile()
    {
        if (!LimTunerManager.Instance.isInitialized)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Layesta_NoProject"]);
            return;
        }
        string path = WindowsDialogUtility.SaveFileDialog(LimLanguageManager.TextDict["Layesta_Build_Window"], "Layesta Level|*.layesta", LimProjectManager.LapFolder);
        if (path == null) return;
        if (!path.EndsWith(".layesta")) path += ".layesta";
        filesToZip.Clear();
        Directory.CreateDirectory(LimProjectManager.LapFolder + "/Layesta");
        EncodeBackgrounds(true);
        await PrepareLayestaFileAsync(true);
        if (File.Exists(path)) File.Delete(path);
        File.Move(LimProjectManager.LapFolder + "/instance.layesta", path);
        WindowsDialogUtility.OpenExplorer(path);
    }
    public void BuildAddChartInstance()
    {
        if (!LimTunerManager.Instance.isInitialized)
        {
            return;
        }
        GameObject temp = Instantiate(BuildChartPrefab, BuildChartListContent);
        LimLayestaBuildChart chart = temp.GetComponent<LimLayestaBuildChart>();
        chart.Initialize("Instance");
        chart.DifficultyText.text = "Master";
        buildChartViews.Insert(0, chart);
        AddInstanceButton.interactable = false;
        BuildRefreshChartList();
    }
    public void BuildAddChartFromFile()
    {
        if (!LimTunerManager.Instance.isInitialized)
        {
            return;
        }
        string path = WindowsDialogUtility.OpenFileDialog("", "*.txt|*.txt", LimProjectManager.LapFolder);
        if (path == null) return;
        GameObject temp = Instantiate(BuildChartPrefab, BuildChartListContent);
        LimLayestaBuildChart chart = temp.GetComponent<LimLayestaBuildChart>();
        chart.Initialize(path);
        buildChartViews.Insert(0, chart);
        BuildRefreshChartList();
    }
    public void BuildRemoveChart(LimLayestaBuildChart chart)
    {
        buildChartViews.Remove(chart);
        if (chart.ChartPath == "Instance")
        {
            AddInstanceButton.interactable = true;
        }
        Destroy(chart.gameObject);
        BuildRefreshChartList();
    }
    private void BuildRefreshChartList()
    {
        float y = 0;
        foreach (LimLayestaBuildChart c in buildChartViews)
        {
            c.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y);
            y -= 40;
        }
        BuildChartListContent.sizeDelta = new Vector2(0, -y);
    }

    [Obsolete("Use BuildLayestaFile")]
    public async void GenerateOnly()
    {
        if (!LimTunerManager.Instance.isInitialized)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Layesta_NoProject"]);
            return;
        }
        string path = WindowsDialogUtility.OpenFileDialog(LimLanguageManager.TextDict["Layesta_Export"], "Layesta Level|*.layesta", LimProjectManager.LapFolder);
        if (path == null) return;
        if (!path.EndsWith(".layesta")) path += ".layesta";
        filesToZip.Clear();
        Directory.CreateDirectory(LimProjectManager.LapFolder + "/Layesta");
        EncodeBackgrounds(true);
        await PrepareLayestaFileAsync(true);
        File.Move(LimProjectManager.LapFolder + "/instance.layesta", path);
        WindowsDialogUtility.OpenExplorer(path);
    }
#if UNITY_EDITOR
    public async void GenerateInternal()
    {
        if (!LimTunerManager.Instance.isInitialized)
        {
            MessageBoxManager.Instance.ShowMessage(LimLanguageManager.TextDict["Layesta_NoProject"]);
            return;
        }
        string path = $"H:/LayestaGit/Charts/{LimProjectManager.CurrentProject.Name}_{LimProjectManager.CurrentProject.Designer}.layesta";
        filesToZip.Clear();
        Directory.CreateDirectory(LimProjectManager.LapFolder + "/Layesta");
        EncodeBackgrounds(true);
        await PrepareLayestaFileAsync(true);
        File.Move(LimProjectManager.LapFolder + "/instance.layesta", path);
    }
    public async void GenerateMultiple()
    {
        string projectName = LimProjectManager.CurrentProject.Name;
        string projectAuthor = LimProjectManager.CurrentProject.Designer;
        string projectArtist = "Yooh";
        string outputPath = $"E:/LanotaNS/{projectName}.layesta";
        string originFolder = @"E:\ns\ThefuckingswitchLanota\marianne";
        Directory.CreateDirectory(LimProjectManager.LapFolder + "/Layesta");
        filesToZip.Clear();
        EncodeBackgrounds(true);
        await ConvertAudioAsync(true);
        await ConvertBackgroundBlurAsync(true);
        filesToZip.Add(originFolder + @"\chart_Acoustic.txt");
        filesToZip.Add(originFolder + @"\chart_Whisper.txt");
        filesToZip.Add(originFolder + @"\chart_Ultra.txt");
        filesToZip.Add(originFolder + @"\chart_Master.txt");
        await Task.Run(() =>
        {
            FileStream fs = new FileStream(LimProjectManager.LapFolder + "/Layesta/info.bytes", FileMode.Create, FileAccess.Write);
            BinaryWriter b = new BinaryWriter(fs);
            b.Write(projectName);
            b.Write(projectAuthor);
            b.Write(4);
            b.Write("Whisper");
            b.Write("Acoustic");
            b.Write("Ultra");
            b.Write("Master");
            Kaliko.ImageLibrary.KalikoImage k = new Kaliko.ImageLibrary.KalikoImage(LimProjectManager.LapFolder + "/Layesta/background.jpg");
            b.Write(k.Width);
            b.Write(k.Height);
            b.Write(projectArtist);
            b.Close();
            fs.Close();
            filesToZip.Add(LimProjectManager.LapFolder + "/Layesta/info.bytes");
            lzip.compress_File_List(0, LimProjectManager.LapFolder + "/instance.layesta", filesToZip.ToArray(), null, false, filesToZip.Select((s) => Path.GetFileName(s)).ToArray());
        });
        File.Move(LimProjectManager.LapFolder + "/instance.layesta", outputPath);
    }
#endif
    public void CheckDepends()
    {
        Task.Run(() =>
        {
            if (!File.Exists(FFMpegPath)) return;
            MD5 m = MD5.Create();
            FileStream fs = File.Open(FFMpegPath, FileMode.Open);
            byte[] ffmpegHash = m.ComputeHash(fs);
            fs.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ffmpegHash.Length; i++)
            {
                sb.Append(ffmpegHash[i].ToString("x2"));
            }
            string ffmpeg = sb.ToString();
            if (ffmpeg.ToUpper() != "54665FF6258FFAD22984D0C85DA37047") return;
            hasDepends = true;
        });
    }
}
