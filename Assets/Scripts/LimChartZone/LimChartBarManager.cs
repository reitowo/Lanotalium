using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
#if UNITY_IOS
using System.IO.Compression;
#endif

public class LimChartBarManager : MonoBehaviour
{
    public Text ChartNameText, DesignerText, NoteCountText, SizeText, RatingText;
    public Slider DownloadSlider, RatingSlider;
    public Image BilibiliQrCode;

    private bool isDownloading;
    private float QrCodeAnimateDuration = 0.25f;
    private Coroutine QrCodeAnimateCoroutine;
    private Lanotalium.ChartZone.ChartZoneChart Data = new Lanotalium.ChartZone.ChartZoneChart();
    private int UserRating;
    private float OnlineRating;
    public string BilibiliUrl;
    private bool isInitialized = false;

    private void Update()
    {
        RatingText.text = string.Format("{0} : {1} | {2} : {3}", LimLanguageManager.TextDict["ChartZone_Rating_You"], UserRating, LimLanguageManager.TextDict["ChartZone_Rating_Online"], OnlineRating == 0 ? "-.-" : OnlineRating.ToString("f1"));
    }

    public void Initialize(Lanotalium.ChartZone.ChartZoneChart Data)
    {
        this.Data = Data;
        BilibiliUrl = string.Format("{0}av{1}", LimChartZoneManager.BilibiliVideoPrefix, Data.BilibiliAvIndex.ToString());
        StartCoroutine(GetBilibiliQrCode());
        StartCoroutine(GetRating());
        ChartNameText.text = Data.ChartName;
        DesignerText.text = Data.Designer;
        NoteCountText.text = Data.NoteCount.ToString();
        SizeText.text = Data.Size;
        isInitialized = true;
    }
    IEnumerator GetBilibiliQrCode()
    {
        WWW Qr = new WWW(LimChartZoneManager.QrCodeProvider + BilibiliUrl);
        yield return Qr;
        Sprite QrSprite = Sprite.Create(Qr.texture, new Rect(0, 0, Qr.texture.width, Qr.texture.height), new Vector2(0.5f, 0.5f));
        BilibiliQrCode.sprite = QrSprite;
    }

    public void OpenBilibili()
    {
        UnityEngine.Application.OpenURL(BilibiliUrl);
    }
    public void StartDownloadChart()
    {
        if (!isInitialized) return;
        if (isDownloading) return;
        StartCoroutine(DownloadChart());
    }
    IEnumerator DownloadChart()
    {
        isDownloading = true;
        WWW Download = new WWW(LimSystem.LanotaliumServer + "/lanotalium/chartzone/" + Data.ChartName + "/" + Data.ChartName + ".zip");
        DownloadSlider.value = 0;
        while (!Download.isDone)
        {
            DownloadSlider.value = Download.progress;
            SizeText.text = (Download.progress * 100).ToString("f2") + "%";
            yield return null;
        }
        DownloadSlider.value = 1;
        SizeText.text = "100.00%";
        SizeText.text = Data.Size;
        byte[] Chart = Download.bytes;
#if UNITY_STANDALONE
        string SavePath = WindowsDialogUtility.SaveFileDialog("", "(.zip)|*.zip", Data.ChartName + ".zip");
        if (SavePath == null) yield break;
        File.WriteAllBytes(SavePath, Chart);
        Process.Start("explorer.exe", "/select," + SavePath.Replace("/", "\\"));
#endif
#if UNITY_IOS
        string SaveDirectory = UnityEngine.Application.persistentDataPath + "/chartzone/" + Data.ChartName;
        string SavePath = UnityEngine.Application.persistentDataPath + "/chartzone/" + Data.ChartName + "/" + Data.ChartName + ".zip";
        Directory.CreateDirectory(Directory.GetParent(SavePath).FullName);
        File.WriteAllBytes(SavePath, Chart);
        ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
        Task ExtractTask = new Task(() => fastZip.ExtractZip(SavePath, SaveDirectory, ""));
        ExtractTask.Start();
        while (ExtractTask.Status == TaskStatus.Running) yield return null;
        Lanotalium.Project.LanotaliumProject lanotaliumProject = new Lanotalium.Project.LanotaliumProject
        {
            Name = Data.ChartName,
            Designer = Data.Designer,
            BGA0Path = SaveDirectory + "/background0.png",
            ChartPath = SaveDirectory + "/chart.txt",
            MusicPath = SaveDirectory + "/music.ogg"
        };
        File.WriteAllText(SaveDirectory + "/project.lap", Newtonsoft.Json.JsonConvert.SerializeObject(lanotaliumProject));
        LimChartZoneManager.OpenDownloadedChart = true;
        LimChartZoneManager.DownloadedChartLapPath = SaveDirectory + "/project.lap";
        isDownloading = false;
        SceneManager.LoadScene("LimTuner");
#endif
        isDownloading = false;
    }

    private bool isReceivingUserRating = false;
    public void ReceiveUserRating()
    {
        isReceivingUserRating = true;
        RatingSlider.interactable = true;
        RatingSlider.wholeNumbers = true;
        RatingSlider.value = UserRating;
        RatingSlider.onValueChanged.AddListener(OnRatingChange);
    }
    public void ShowOnlineRating()
    {
        isReceivingUserRating = false;
        RatingSlider.onValueChanged.RemoveAllListeners();
        RatingSlider.interactable = false;
        RatingSlider.wholeNumbers = false;
        RatingSlider.value = OnlineRating;
    }
    public void OnRatingChange(float Value)
    {
        if (!isInitialized) return;
        bool Post = false;
        if (UserRating != Value) Post = true;
        UserRating = (int)Value;
        if (Post) StartPostRating();
    }
    public void StartPostRating()
    {
        if (!isInitialized) return;
        StartCoroutine(PostRating());
    }
    IEnumerator PostRating()
    {
        WWWForm Form = new WWWForm();
        Form.AddField("ChartName", Data.ChartName);
        Form.AddField("Rating", UserRating.ToString());
        Form.AddField("UserId", SystemInfo.deviceUniqueIdentifier);
        WWW Rating = new WWW(LimSystem.LanotaliumServer + "/lanotalium/chartzone/ChartZonePostRating.php", Form);
        yield return Rating;
        StartCoroutine(GetRating());
    }
    IEnumerator GetRating()
    {
        WWWForm Form = new WWWForm();
        Form.AddField("ChartName", Data.ChartName);
        WWW Rating = new WWW(LimSystem.LanotaliumServer + "/lanotalium/chartzone/ChartZoneGetRating.php", Form);
        yield return Rating;
        float.TryParse(Rating.text, out OnlineRating);
        if (!isReceivingUserRating) RatingSlider.value = OnlineRating;
        Form.AddField("UserId", SystemInfo.deviceUniqueIdentifier);
        Rating = new WWW(LimSystem.LanotaliumServer + "/lanotalium/chartzone/ChartZoneGetRating.php", Form);
        yield return Rating;
        int.TryParse(Rating.text, out UserRating);
    }

    public void StartShowQrCode()
    {
        if (QrCodeAnimateCoroutine != null) StopCoroutine(QrCodeAnimateCoroutine);
        QrCodeAnimateCoroutine = StartCoroutine(ShowQrCode());
    }
    public void StartHideQrCode()
    {
        if (QrCodeAnimateCoroutine != null) StopCoroutine(QrCodeAnimateCoroutine);
        QrCodeAnimateCoroutine = StartCoroutine(HideQrCode());
    }
    IEnumerator ShowQrCode()
    {
        transform.SetAsLastSibling();
        float TimeCount = 0;
        BilibiliQrCode.gameObject.SetActive(true);
        while (TimeCount < QrCodeAnimateDuration)
        {
            TimeCount += Time.deltaTime;
            TimeCount = Mathf.Clamp(TimeCount, 0, QrCodeAnimateDuration);
            BilibiliQrCode.color = new Color(1, 1, 1, TimeCount / QrCodeAnimateDuration);
            yield return null;
        }
    }
    IEnumerator HideQrCode()
    {
        float TimeCount = 0;
        while (TimeCount < QrCodeAnimateDuration)
        {
            TimeCount += Time.deltaTime;
            TimeCount = Mathf.Clamp(TimeCount, 0, QrCodeAnimateDuration);
            BilibiliQrCode.color = new Color(1, 1, 1, 1 - TimeCount / QrCodeAnimateDuration);
            yield return null;
        }
        BilibiliQrCode.gameObject.SetActive(false);
    }
}
