using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;
using Lanotalium.ChartZone.WebApi;

public class LimChartZoneManager : MonoBehaviour
{
    public static int FromScene = 0;
    public static bool OpenDownloadedChart = false;
    public static string DownloadedChartLapPath = string.Empty;
    public static string BilibiliVideoPrefix = "https://www.bilibili.com/video/";
    public static string QrCodeProvider = "http://qr.liantu.com/api.php?text=";
    public Text HeadText, ChartNameText, DesignerText, NotesText, DownloadText, VideoText, RatingText, CopyRightText;
    public RectTransform ChartListContent;
    public GameObject ChartBarPrefab;
    public InputField NameFilter;
    private List<LimChartBarManager> _BarManagers = new List<LimChartBarManager>();
    private bool _LoadChartListFinished = false;
    private string _CurrentNameFilter;
    void Start()
    {
        BilibiliVideoPrefix = RemoteSettings.GetString("BilibiliVideoPrefix", "https://www.bilibili.com/video/");
        QrCodeProvider = RemoteSettings.GetString("QrCodeProvider", "http://qr.liantu.com/api.php?text=");
        StartCoroutine(GetChartList());
    }
    public void SetTexts()
    {
        HeadText.text = LimLanguageManager.TextDict["ChartZone_Head"];
        ChartNameText.text = LimLanguageManager.TextDict["ChartZone_ChartName"];
        DesignerText.text = LimLanguageManager.TextDict["ChartZone_Designer"];
        NotesText.text = LimLanguageManager.TextDict["ChartZone_Notes"];
        DownloadText.text = LimLanguageManager.TextDict["ChartZone_Download"];
        VideoText.text = LimLanguageManager.TextDict["ChartZone_Video"];
        RatingText.text = LimLanguageManager.TextDict["ChartZone_Rating"];
        CopyRightText.text = LimLanguageManager.TextDict["ChartZone_CopyRight"].Replace("<br>", "\n");
    }
    IEnumerator GetChartList()
    {
        #region Seed
        /*List<Lanotalium.ChartZone.ChartZoneChart> charts = JsonConvert.DeserializeObject<List<Lanotalium.ChartZone.ChartZoneChart>>(File.ReadAllText(@"H:\Server\html\lanotalium\chartzone\ChartList.json"));
        foreach (var chart in charts)
        {
            yield return LimChartZoneWebApi.AddChart(new LimChartZoneWebApi.ChartDto()
            {
                ChartName = chart.ChartName,
                Designer = chart.Designer,
                NoteCount = chart.NoteCount,
                Size = chart.Size,
                BilibiliAvIndex = chart.BilibiliAvIndex
            });
        }
        Dictionary<string, int> rat = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(@"H:\Server\html\lanotalium\chartzone\Modelista\Rating.json"));
        foreach (string key in rat.Keys)
        {
            LimChartZoneWebApi.Rating rating = new LimChartZoneWebApi.Rating
            {
                Rate = rat[key],
                UserId = key
            };
            yield return LimChartZoneWebApi.PostRating(14, rating);
        }*/
        #endregion
        ObjectWrap<List<ChartDto>> Charts = new ObjectWrap<List<ChartDto>>();
        yield return LimChartZoneWebApi.GetAllCharts(Charts);
        Charts.Reference.Reverse();

        int HeightCount = 0;
        foreach (ChartDto Chart in Charts.Reference)
        {
            GameObject tGO = Instantiate(ChartBarPrefab, ChartListContent);
            tGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, HeightCount);
            tGO.GetComponent<LimChartBarManager>().Initialize(Chart);
            _BarManagers.Add(tGO.GetComponent<LimChartBarManager>());
            HeightCount -= 50;
        }
        ChartListContent.sizeDelta = new Vector2(0, -HeightCount);
        _LoadChartListFinished = true;
    }

    public void OnNameFilterChanged()
    {
        _CurrentNameFilter = NameFilter.text;
        ReArrangeChartList();
    }

    public void ReArrangeChartList()
    {
        int HeightCount = 0;
        foreach (LimChartBarManager chartBarManager in _BarManagers)
        {
            if (string.IsNullOrEmpty(_CurrentNameFilter)) chartBarManager.gameObject.SetActive(true);
            else
            {
                if (!chartBarManager.Name.ToLower().Contains(_CurrentNameFilter.ToLower()))
                {
                    chartBarManager.gameObject.SetActive(false);
                    continue;
                }
                else chartBarManager.gameObject.SetActive(true);
            }
            chartBarManager.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, HeightCount);
            HeightCount -= 50;
        }
        ChartListContent.sizeDelta = new Vector2(0, -HeightCount);
    }

    private bool _IsNameLargeAbove = false;
    public void SortByName()
    {
        if (!_LoadChartListFinished) return;
        _IsNameLargeAbove = !_IsNameLargeAbove;
        _BarManagers.Sort((LimChartBarManager a, LimChartBarManager b) =>
        {
            if (_IsNameLargeAbove) return a.Name.CompareTo(b.Name);
            else return b.Name.CompareTo(a.Name);
        });
        ReArrangeChartList();
    }

    private bool _IsDesignerLargeAbove = false;
    public void SortByDesigner()
    {
        if (!_LoadChartListFinished) return;
        _IsDesignerLargeAbove = !_IsDesignerLargeAbove;
        _BarManagers.Sort((LimChartBarManager a, LimChartBarManager b) =>
        {
            if (_IsDesignerLargeAbove) return a.Designer.CompareTo(b.Designer);
            else return b.Designer.CompareTo(a.Designer);
        });
        ReArrangeChartList();
    }

    private bool _IsNoteCountLargeAbove = true;
    public void SortByNoteCount()
    {
        if (!_LoadChartListFinished) return;
        _IsNoteCountLargeAbove = !_IsNoteCountLargeAbove;
        _BarManagers.Sort((LimChartBarManager a, LimChartBarManager b) =>
        {
            if (_IsNoteCountLargeAbove) return b.Data.NoteCount.CompareTo(a.Data.NoteCount);
            else return a.Data.NoteCount.CompareTo(b.Data.NoteCount);
        });
        ReArrangeChartList();
    }

    private bool _IsSizeLargeAbove = false;
    public void SortBySize()
    {
        if (!_LoadChartListFinished) return;
        _IsSizeLargeAbove = !_IsSizeLargeAbove;
        _BarManagers.Sort((LimChartBarManager a, LimChartBarManager b) =>
        {
            if (_IsSizeLargeAbove) return b.Size.CompareTo(a.Size);
            else return a.Size.CompareTo(b.Size);
        });
        ReArrangeChartList();
    }

    private bool _IsAvIndexLargeAbove = true;
    public void SortByAvIndex()
    {
        if (!_LoadChartListFinished) return;
        _IsAvIndexLargeAbove = !_IsAvIndexLargeAbove;
        _BarManagers.Sort((LimChartBarManager a, LimChartBarManager b) =>
        {
            if (_IsAvIndexLargeAbove) return b.Data.BilibiliAvIndex.CompareTo(a.Data.BilibiliAvIndex);
            else return a.Data.BilibiliAvIndex.CompareTo(b.Data.BilibiliAvIndex);
        });
        ReArrangeChartList();
    }

    private bool _IsRatingLargeAbove = true;
    public void SortByRating()
    {
        if (!_LoadChartListFinished) return;
        _IsRatingLargeAbove = !_IsRatingLargeAbove;
        _BarManagers.Sort((LimChartBarManager a, LimChartBarManager b) =>
        {
            if (_IsRatingLargeAbove) return b.OnlineRating.CompareTo(a.OnlineRating);
            else return a.OnlineRating.CompareTo(b.OnlineRating);
        });
        ReArrangeChartList();
    }

    public void Back()
    {
        SceneManager.LoadScene(FromScene);
    }
}
