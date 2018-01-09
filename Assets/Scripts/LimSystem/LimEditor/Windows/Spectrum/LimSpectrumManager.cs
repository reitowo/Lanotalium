using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LimSpectrumManager : MonoBehaviour
{
    public LimWindowManager BaseWindow;
    [HideInInspector]
    public List<LimSpectrumBarManager> SpectrumBars = new List<LimSpectrumBarManager>();
    public AudioSource MusicPlayer;
    public GameObject SpectrumBarsPrefab;
    public RectTransform SpectrumContent;
    public LimWindowManager WindowManager;
    public LineRenderer AxisX;
    public float IgnoreRatio = 0.7f, IgnoreBeforeOffset = 0, IgnoreForeOffset = 1;
    public int BarCount = 256;
    public int SampleCount = 8192;
    public FFTWindow FFTType = FFTWindow.Triangle;

    private int[] LogAxis;
    private float[] SpectrumDataL;
    private float[] SpectrumDataR;
    private float LastContentWidth = 0;
    private void Start()
    {
        SpectrumDataL = new float[SampleCount];
        SpectrumDataR = new float[SampleCount];
        LogAxis = new int[BarCount];
        InstantiateBars();
        FillLogAxis();
        BaseWindow.OnWindowSorted.AddListener(OnWindowSorted);
    }
    private void Update()
    {
        DetectRectChange();
        if (MusicPlayer.clip == null) return;
        DrawSpectrumData();
    }
    public void OnWindowSorted(int Order)
    {
        AxisX.sortingOrder = Order + 1;
        foreach (LimSpectrumBarManager Bar in SpectrumBars) Bar.Bar.sortingOrder = Order + 1;
    }

    private void InstantiateBars()
    {
        foreach (LimSpectrumBarManager BarManager in SpectrumBars)
        {
            if (BarManager != null)
                if (BarManager.gameObject != null)
                    Destroy(BarManager.gameObject);
        }
        SpectrumBars.Clear();
        float ContentWidth = SpectrumContent.rect.width;
        float BarWidth = ContentWidth / BarCount;
        for (int i = 0; i < BarCount; ++i)
        {
            LimSpectrumBarManager BarManager = Instantiate(SpectrumBarsPrefab, SpectrumContent).GetComponent<LimSpectrumBarManager>();
            SpectrumBars.Add(BarManager);
            BarManager.Width = BarWidth;
            BarManager.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(BarWidth, 0);
            BarManager.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * BarWidth, 0);
        }
        LastContentWidth = ContentWidth;
    }
    private float Log10By24000 = Mathf.Log10(24000);
    private int GetLogFrequencyIndex(int BarIndex)
    {
        float X = Mathf.Clamp(Log10By24000 * BarIndex / BarCount + IgnoreBeforeOffset, 0, Log10By24000);
        X = (X * IgnoreRatio + IgnoreForeOffset);
        float Freq = Mathf.Pow(10, X);
        return Mathf.Clamp(Mathf.RoundToInt(Freq / 24000f * SampleCount), 0, SampleCount - 1);
    }
    private void DetectRectChange()
    {
        if (LastContentWidth != SpectrumContent.rect.width)
        {
            float ContentWidth = SpectrumContent.rect.width;
            float BarWidth = ContentWidth / BarCount;
            for (int i = 0; i < BarCount; ++i)
            {
                SpectrumBars[i].Width = BarWidth;
                SpectrumBars[i].gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(BarWidth, 0);
                SpectrumBars[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * BarWidth, 0);
            }
            LastContentWidth = SpectrumContent.rect.width;
            AxisX.SetPosition(1, new Vector3(SpectrumContent.rect.width, 0, 0));
        }
    }
    private void FillLogAxis()
    {
        for (int i = 0; i < BarCount; ++i)
        {
            LogAxis[i] = GetLogFrequencyIndex(i);
        }
    }
    private void DrawSpectrumData()
    {
        MusicPlayer.GetSpectrumData(SpectrumDataL, 0, FFTType);
        MusicPlayer.GetSpectrumData(SpectrumDataR, 0, FFTType);
        for (int i = 0; i < BarCount; ++i)
        {
            SpectrumBars[i].Amplitude = Mathf.Clamp((SpectrumDataL[LogAxis[i]] + SpectrumDataR[LogAxis[i]]) * 4.16f * SpectrumContent.rect.height, 0, SpectrumContent.rect.height);
        }
    }

}
