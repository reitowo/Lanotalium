using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimWaveformManager : MonoBehaviour
{
    public LimTimeLineManager TimeLineManager;
    public LimTunerManager TunerManager;
    public LineRenderer LineL, LineR;
    public float[] FormL, FormR;
    public int SamplesToRead = 10000;
    public int WaveScale = 25;

    private AudioClip Music;
    private bool isDataLoaded = false;

    public void OnMusicLoaded()
    {
        Music = LimSystem.ChartContainer.ChartMusic.Music;
        float[] Data = new float[Music.samples * Music.channels];
        Music.GetData(Data, 0);
        List<float> L = new List<float>();
        List<float> R = new List<float>();
        for (int i = 0; i < Music.samples * Music.channels; i += 2)
        {
            L.Add(Data[i]);
            R.Add(Data[i + 1]);
        }
        FormL = L.ToArray();
        FormR = R.ToArray();
        isDataLoaded = true;
    }
    public void Update()
    {
        if (!isDataLoaded || !TunerManager.isInitialized) return;
        UpdateWaveform();
    }
    public int FindNearestSampleInt(int Current, int SampleDelta)
    {
        while (Current % SampleDelta != 0) Current++;
        return Current;
    }
    public void UpdateWaveform()
    {
        int ScaledSamplesToRead = (int)((TimeLineManager.ViewRect.sizeDelta.x - 200f) / 1220f * SamplesToRead);
        if (ScaledSamplesToRead <= 0) return;
        int StartSample = TunerManager.MusicPlayerManager.MusicPlayer.timeSamples;
        int CurrentSample = StartSample;
        int EndSample = (int)((TimeLineManager.ViewRect.sizeDelta.x - 200f) / TimeLineManager.Scale * Music.frequency) + StartSample;
        int SampleDelta = Mathf.CeilToInt((EndSample - CurrentSample) * 1f / ScaledSamplesToRead);
        int PositionCount = (int)(ScaledSamplesToRead * (EndSample > Music.samples ? (Music.samples - CurrentSample) * 1f / (EndSample - CurrentSample) : 1));
        if (PositionCount <= 0) return;
        LineL.positionCount = PositionCount;
        LineR.positionCount = PositionCount;
        int PositionIndex = 0;
        if (CurrentSample % SampleDelta != 0)
        {
            LineL.SetPosition(PositionIndex, new Vector3(0, WaveScale * FormL[CurrentSample]));
            LineR.SetPosition(PositionIndex, new Vector3(0, WaveScale * FormR[CurrentSample]));
            CurrentSample = FindNearestSampleInt(CurrentSample, SampleDelta);
            PositionIndex++;
        }
        for (int i = PositionIndex; i < PositionCount; ++i)
        {
            float x = 1f * (CurrentSample - StartSample) / Music.frequency * TimeLineManager.Scale;
            if (x > TimeLineManager.ViewRect.sizeDelta.x - 200f)
            {
                LineL.positionCount = i;
                LineR.positionCount = i;
                break;
            }
            if (i >= PositionCount || CurrentSample > Music.samples) break;
            LineL.SetPosition(i, new Vector3(x, WaveScale * FormL[CurrentSample]));
            LineR.SetPosition(i, new Vector3(x, WaveScale * FormR[CurrentSample]));
            CurrentSample += SampleDelta;
        }
    }
    public void Hide()
    {
        LineL.positionCount = 0;
        LineR.positionCount = 0;
    }
}
