using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class LimWaveformManager : MonoBehaviour
{
    public WindowsDialogUtility DialogUtils;
    public LimTimeLineManager TimeLineManager;
    public LimTunerManager TunerManager;
    public LineRenderer LineL, LineR;
    public RectTransform Blocker;
    private static List<float> FormL = new List<float>(), FormR = new List<float>();
    public int SamplesToRead = 10000;
    public int WaveScale = 25;

    private bool isDataLoaded = false;

    public void OnMusicLoaded()
    {
        try
        {
            float[] Data = new float[LimSystem.ChartContainer.ChartMusic.Music.samples * LimSystem.ChartContainer.ChartMusic.Music.channels];
            LimSystem.ChartContainer.ChartMusic.Music.GetData(Data, 0);
            FormL.Clear();
            FormR.Clear();
            FormL.Capacity = LimSystem.ChartContainer.ChartMusic.Music.samples;
            FormR.Capacity = LimSystem.ChartContainer.ChartMusic.Music.samples;
            for (int i = 0; i < LimSystem.ChartContainer.ChartMusic.Music.samples * LimSystem.ChartContainer.ChartMusic.Music.channels; i += 2)
            {
                FormL.Add(Data[i]);
                FormR.Add(Data[i + 1]);
            }
            isDataLoaded = true;
            Data = null;
            GC.Collect();
        }
        catch (Exception)
        {
            DialogUtils.MessageBox.ShowMessage(LimLanguageManager.TextDict["Window_TimeLine_Waveform_LowMemory"]);
        }
    }
    private void Start()
    {
        Blocker.anchoredPosition = new Vector2(Mathf.Clamp(LimSystem.Preferences.WaveformBlockerPosition, 200, TimeLineManager.ViewRect.sizeDelta.x - 25), Blocker.anchoredPosition.y);
    }
    public void Update()
    {
        try
        {
            if (LimSystem.ChartContainer == null) return;
            MoveBlocker();
            if (!isDataLoaded || !TunerManager.isInitialized) return;
            UpdateWaveform();
        }
        catch (Exception)
        {
            isDataLoaded = false;
        }
    }
    public int FindNearestSampleInt(int Current, int SampleDelta)
    {
        while (Current % SampleDelta != 0) Current++;
        return Current;
    }
    public void UpdateWaveform()
    {
        int ScaledSamplesToRead = (int)((Blocker.anchoredPosition.x - 200f) / 1220f * SamplesToRead);
        if (ScaledSamplesToRead <= 0)
        {
            LineL.positionCount = 0;
            LineR.positionCount = 0;
            return;
        }
        int StartSample = TunerManager.MediaPlayerManager.MusicPlayer.timeSamples;
        int CurrentSample = StartSample;
        int EndSample = (int)((Blocker.anchoredPosition.x - 200f) / TimeLineManager.Scale * LimSystem.ChartContainer.ChartMusic.Music.frequency) + StartSample;
        int SampleDelta = Mathf.CeilToInt((EndSample - CurrentSample) * 1f / ScaledSamplesToRead);
        int PositionCount = (int)(ScaledSamplesToRead * (EndSample > LimSystem.ChartContainer.ChartMusic.Music.samples ? (LimSystem.ChartContainer.ChartMusic.Music.samples - CurrentSample) * 1f / (EndSample - CurrentSample) : 1));
        if (PositionCount <= 0) return;
        LineL.positionCount = PositionCount;
        LineR.positionCount = PositionCount;
        int PositionIndex = 0;
        if (TunerManager.MediaPlayerManager.IsPlaying)
        {
            if (CurrentSample % SampleDelta != 0)
            {
                LineL.SetPosition(PositionIndex, new Vector3(0, WaveScale * FormL[CurrentSample]));
                LineR.SetPosition(PositionIndex, new Vector3(0, WaveScale * FormR[CurrentSample]));
                CurrentSample = FindNearestSampleInt(CurrentSample, SampleDelta);
                PositionIndex++;
            }
        }
        for (int i = PositionIndex; i < PositionCount; ++i)
        {
            float x = 1f * (CurrentSample - StartSample) / LimSystem.ChartContainer.ChartMusic.Music.frequency * TimeLineManager.Scale;
            if (x > Blocker.anchoredPosition.x - 200f)
            {
                LineL.positionCount = i;
                LineR.positionCount = i;
                break;
            }
            if (i >= PositionCount || CurrentSample > LimSystem.ChartContainer.ChartMusic.Music.samples) break;
            LineL.SetPosition(i, new Vector3(x, WaveScale * FormL[CurrentSample]));
            LineR.SetPosition(i, new Vector3(x, WaveScale * FormR[CurrentSample]));
            CurrentSample += SampleDelta;
        }
    }
    private bool isBlockerDown = false;
    public void OnBlockerDown()
    {
        isBlockerDown = true;
    }
    public void MoveBlocker()
    {
        Blocker.anchoredPosition = new Vector2(Mathf.Clamp(Blocker.anchoredPosition.x, 200, TimeLineManager.ViewRect.sizeDelta.x - 25), Blocker.anchoredPosition.y);
        if (!isBlockerDown) return;
        if (!Blocker.GetComponent<Button>().interactable) return;
        Blocker.anchoredPosition = new Vector2(Mathf.Clamp(Blocker.anchoredPosition.x + LimMousePosition.DeltaPosition.x, 200, TimeLineManager.ViewRect.sizeDelta.x - 25), Blocker.anchoredPosition.y);
        LimSystem.Preferences.WaveformBlockerPosition = Blocker.anchoredPosition.x;
    }
    public void OnBlockerUp()
    {
        isBlockerDown = false;
    }
    public void Show()
    {
        if (Blocker == null) return;
        Blocker.GetComponent<Button>().interactable = true;
        Blocker.anchoredPosition = new Vector2(Mathf.Clamp(LimSystem.Preferences.WaveformBlockerPosition, 200, TimeLineManager.ViewRect.sizeDelta.x - 25), Blocker.anchoredPosition.y);
    }
    public void Hide()
    {
        if (Blocker == null) return;
        LineL.positionCount = 0;
        LineR.positionCount = 0;
        Blocker.GetComponent<Button>().interactable = false;
        Blocker.anchoredPosition = new Vector2(200, Blocker.anchoredPosition.y);
    }
}
