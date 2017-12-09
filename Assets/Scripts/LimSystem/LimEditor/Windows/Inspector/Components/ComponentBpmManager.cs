using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentBpmManager : MonoBehaviour
{
    public LimInspectorManager InspectorManager;
    public LimOperationManager OperationManager;
    public LimTunerManager TunerManager;
    public RectTransform ViewRect, ComponentRect;
    public Color InvalidColor, ValidColor, PressedColor, UnPressedColor;
    public GameObject TimeValuePrefab, BeatlinePrefab;
    public Transform PairsTransform, BeatlineTransform;
    public Text LabelText, TimingText, BpmText, BeatlineText, DensityText, FixSelectedText, FixAllText;
    public List<float> BeatlineTimes = new List<float>();
    public List<GameObject> Beatlines = new List<GameObject>();
    public InputField DensityInputField;
    public Image BeatlineImg, DensityImg;
    public float UnFoldHeight;
    public bool EnableBeatline
    {
        get
        {
            return isBeatlineOpen;
        }
        set
        {
            isBeatlineOpen = value;
            if (value)
            {
                ReCalculateBeatlineTimes();
                BeatlineImg.color = PressedColor;
            }
            else
            {
                GenerateCorrectQuantityBeatline(0);
                BeatlineImg.color = UnPressedColor;
            }
        }
    }
    public float BeatlineDensity
    {
        get
        {
            return Density;
        }
        set
        {
            Density = value;
            ReCalculateBeatlineTimes();
        }
    }

    private bool isFolded = false, isBeatlineOpen = false;
    private float UiWidth, Density = 1;

    private void Start()
    {
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        RefreshUiWidth();
    }
    private void Update()
    {
        OnUiWidthChange();
        BeatlineUpdator();
    }
    private void OnDisable()
    {
        EnableBeatline = false;
    }
    public void RefreshUiWidth()
    {
        UiWidth = ViewRect.rect.width;
        float Ratio = UiWidth / 500f;
        TimingText.GetComponent<RectTransform>().anchoredPosition = new Vector2(10 * Ratio, 0);
        TimingText.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * Ratio, 30);
        BpmText.GetComponent<RectTransform>().anchoredPosition = new Vector2(200 * Ratio, 0);
        BpmText.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * Ratio, 30);
    }
    public void OnUiWidthChange()
    {
        if (UiWidth != ViewRect.rect.width)
        {
            RefreshUiWidth();
        }
    }
    public void SetTexts()
    {
        LabelText.text = LimLanguageManager.TextDict["Component_Bpm_Label"];
        TimingText.text = LimLanguageManager.TextDict["Component_Bpm_Timing"];
        BpmText.text = LimLanguageManager.TextDict["Component_Bpm_Bpm"];
        BeatlineText.text = LimLanguageManager.TextDict["Component_Bpm_Beatline"];
        DensityText.text = LimLanguageManager.TextDict["Component_Bpm_Density"];
        FixSelectedText.text = LimLanguageManager.TextDict["Component_Bpm_FixSelected"];
        FixAllText.text = LimLanguageManager.TextDict["Component_Bpm_FixAll"];
    }
    public void Fold()
    {
        if (isFolded)
        {
            ViewRect.sizeDelta = new Vector2(0, UnFoldHeight); isFolded = false;
        }
        else if (!isFolded)
        {
            ViewRect.sizeDelta = new Vector2(0, 0); isFolded = true;
        }
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        InspectorManager.ArrangeComponentsUi();
    }
    public void InstantiateBpmList()
    {
        float Height = 0;
        foreach (Lanotalium.Chart.LanotaChangeBpm Bpm in OperationManager.TunerManager.BpmManager.Bpm)
        {
            if (Bpm.ListGameObject != null) Destroy(Bpm.ListGameObject);
            Bpm.ListGameObject = Instantiate(TimeValuePrefab, PairsTransform);
            Bpm.ListGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Height);
            Bpm.ListGameObject.GetComponent<TimeValuePairManager>().OperationManager = OperationManager;
            Bpm.ListGameObject.GetComponent<TimeValuePairManager>().Initialize(Bpm);
            Bpm.InstanceId = Bpm.ListGameObject.GetInstanceID();
            Height -= 30;
        }
        ViewRect.sizeDelta = new Vector2(0, 70 - Height);
        UnFoldHeight = ViewRect.sizeDelta.y;
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        isFolded = false;
        InspectorManager.ArrangeComponentsUi();
    }

    public void StartDetectBpm()
    {

    }
    IEnumerator ManuallyDetectBpmCoroutine()
    {
        yield return null;
    }

    public void OnDensityChange()
    {
        float DensityTmp;
        if (!float.TryParse(DensityInputField.text, out DensityTmp))
        {
            DensityImg.color = InvalidColor;
            return;
        }
        if (DensityTmp > 16 || DensityTmp < 0)
        {
            DensityImg.color = InvalidColor;
            return;
        }
        BeatlineDensity = DensityTmp;
        DensityImg.color = ValidColor;
    }
    public void OnClickBeatlineBtn()
    {
        if (LimSystem.ChartContainer == null) return;
        if (EnableBeatline == true) EnableBeatline = false;
        else EnableBeatline = true;
    }
    public void ReCalculateBeatlineTimes()
    {
        BeatlineTimes.Clear();
        for (int i = 0; i < TunerManager.BpmManager.Bpm.Count; ++i)
        {
            float BpmDeltaTime = (60 / TunerManager.BpmManager.Bpm[i].Bpm) / Density;
            float StartTime = (i == 0 ? 0 : TunerManager.BpmManager.Bpm[i].Time);
            float EndTime = (i == TunerManager.BpmManager.Bpm.Count - 1 ? TunerManager.MusicPlayerManager.Length : TunerManager.BpmManager.Bpm[i + 1].Time);
            for (float t = StartTime; t <= EndTime; t += BpmDeltaTime) BeatlineTimes.Add(t);
        }
    }
    public int FindBeatlineTimesPositionByTime(float Time)
    {
        int Index = 0;
        foreach (float t in BeatlineTimes)
        {
            if (t > Time) break;
            Index++;
        }
        return Index;
    }
    private float CalculateMovePercent(float JudgeTime)
    {
        int StartScroll = 0, EndScroll = 0;
        float Percent = 100;
        for (int i = 0; i < TunerManager.ScrollManager.Scroll.Count - 1; ++i)
        {
            if (TunerManager.ChartTime >= TunerManager.ScrollManager.Scroll[i].Time && TunerManager.ChartTime < TunerManager.ScrollManager.Scroll[i + 1].Time) StartScroll = i;
            if (JudgeTime >= TunerManager.ScrollManager.Scroll[i].Time && JudgeTime < TunerManager.ScrollManager.Scroll[i + 1].Time) EndScroll = i;
        }
        if (TunerManager.ScrollManager.Scroll.Count != 0)
        {
            if (TunerManager.ChartTime >= TunerManager.ScrollManager.Scroll[TunerManager.ScrollManager.Scroll.Count - 1].Time) StartScroll = TunerManager.ScrollManager.Scroll.Count - 1;
            if (JudgeTime >= TunerManager.ScrollManager.Scroll[TunerManager.ScrollManager.Scroll.Count - 1].Time) EndScroll = TunerManager.ScrollManager.Scroll.Count - 1;
        }
        for (int i = StartScroll; i <= EndScroll; ++i)
        {
            if (StartScroll == EndScroll) Percent -= (JudgeTime - TunerManager.ChartTime) * TunerManager.ScrollManager.Scroll[i].Speed * 10 * TunerManager.ChartPlaySpeed;
            else if (StartScroll != EndScroll)
            {
                if (i == StartScroll) Percent -= (TunerManager.ScrollManager.Scroll[i + 1].Time - TunerManager.ChartTime) * TunerManager.ScrollManager.Scroll[i].Speed * 10 * TunerManager.ChartPlaySpeed;
                else if (i != EndScroll && i != StartScroll) Percent -= (TunerManager.ScrollManager.Scroll[i + 1].Time - TunerManager.ScrollManager.Scroll[i].Time) * TunerManager.ScrollManager.Scroll[i].Speed * 10 * TunerManager.ChartPlaySpeed;
                else if (i == EndScroll) Percent -= (JudgeTime - TunerManager.ScrollManager.Scroll[i].Time) * TunerManager.ScrollManager.Scroll[i].Speed * 10 * TunerManager.ChartPlaySpeed;
            }
        }
        Percent = Mathf.Clamp(Percent, 0, 100);
        return Percent;
    }
    private float CalculateEasedPercent(float Percent)
    {
        return Mathf.Pow(2, 10 * (Percent / 100 - 1)) * 100;
    }
    private void GenerateCorrectQuantityBeatline(int BeatlineCount)
    {
        int DeltaQuantity = BeatlineCount - Beatlines.Count;
        if (DeltaQuantity == 0) return;
        if (DeltaQuantity < 0)
        {
            for (int i = 0; i > DeltaQuantity; i--)
            {
                Destroy(Beatlines[Beatlines.Count - 1]);
                Beatlines.RemoveAt(Beatlines.Count - 1);
            }
        }
        else if (DeltaQuantity > 0)
        {
            for (int i = 0; i < DeltaQuantity; ++i)
            {
                Beatlines.Add(Instantiate(BeatlinePrefab, BeatlineTransform));
            }
        }
    }
    private Vector3[] DrawCircle(float Radius)
    {
        List<Vector3> Points = new List<Vector3>();
        for (int i = 0; i < 360; ++i)
        {
            Points.Add(new Vector3(Radius * Mathf.Cos(i * Mathf.Deg2Rad), 0, Radius * Mathf.Sin(i * Mathf.Deg2Rad)));
        }
        return Points.ToArray();
    }
    public void BeatlineUpdator()
    {
        if (!isBeatlineOpen) return;
        float CurrentTime = TunerManager.ChartTime;
        int StartIndex = FindBeatlineTimesPositionByTime(CurrentTime);
        List<float> BeatlinesToDrawPercent = new List<float>();
        for (int i = StartIndex; i < BeatlineTimes.Count; ++i)
        {
            float Percent = CalculateEasedPercent(CalculateMovePercent(BeatlineTimes[i]));
            if (Percent < 20) break;
            BeatlinesToDrawPercent.Add(Percent);
        }
        GenerateCorrectQuantityBeatline(BeatlinesToDrawPercent.Count);
        for (int i = 0; i < BeatlinesToDrawPercent.Count; ++i)
        {
            Beatlines[i].GetComponent<LineRenderer>().SetPositions(DrawCircle(BeatlinesToDrawPercent[i] / 10));
        }
    }

    public float FindPrevOrNextBeatline(float Time, bool Forward)
    {
        if (Beatlines.Count == 0) return Time;
        int Index = OperationManager.FindNearestBeatlineIndexByTime(Time);
        if (Forward) return BeatlineTimes[Mathf.Clamp(Index + 1, 0, BeatlineTimes.Count - 1)];
        else return BeatlineTimes[Mathf.Clamp(Index - 1, 0, BeatlineTimes.Count - 1)];
    }

    public void FixSelectedNotes()
    {
        if (LimSystem.ChartContainer == null) return;
        ReCalculateBeatlineTimes();
        OperationManager.FixSelectedNotesToBeatline();
    }
    public void FixAllNotes()
    {
        if (LimSystem.ChartContainer == null) return;
        ReCalculateBeatlineTimes();
        OperationManager.FixAllNotesToBeatline();
    }
}
