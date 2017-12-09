using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimBpmManager : MonoBehaviour
{
    private bool isInitialized = false;
    public Transform Background, Border, JudgeLine, Arrow, Core;
    public List<Lanotalium.Chart.LanotaChangeBpm> Bpm;
    public LimTunerManager Tuner;
    //public LimCapturer Capturer;
    public float CurrentBpm = 100f;

    void Update()
    {
        UpdateTunerRotation();
        if (!isInitialized) return;
        CurrentBpm = CalculateCurrentBpm();
    }

    public void Initialize(List<Lanotalium.Chart.LanotaChangeBpm> BpmData)
    {
        Bpm = BpmData;
        isInitialized = true;
    }
    public float CalculateBpm(float Time)
    {
        if (Bpm == null) return 0;
        for (int i = 0; i < Bpm.Count - 1; ++i)
        {
            if (Time > Bpm[i].Time && Time <= Bpm[i + 1].Time) return Bpm[i].Bpm;
        }
        return Bpm[Bpm.Count - 1].Bpm;
    }
    private float CalculateCurrentBpm()
    {
        if (Bpm == null) return 0;
        for (int i = 0; i < Bpm.Count - 1; ++i)
        {
            if (Tuner.ChartTime > Bpm[i].Time && Tuner.ChartTime <= Bpm[i + 1].Time) return Bpm[i].Bpm;
        }
        return Bpm[Bpm.Count - 1].Bpm;
    }
    public void RotateTunerOnce(float DeltaTime)
    {
        Background.Rotate(new Vector3(0, 0, DeltaTime * CurrentBpm / 10));
        Border.Rotate(new Vector3(0, 0, -DeltaTime * 2 * CurrentBpm / 10));
        JudgeLine.Rotate(new Vector3(0, 0, -DeltaTime * 2 * CurrentBpm / 10));
        Arrow.Rotate(new Vector3(0, 0, DeltaTime * 3 * CurrentBpm / 10));
        Core.Rotate(new Vector3(0, 0, -DeltaTime * 0.3f * CurrentBpm / 10));
    }
    private void UpdateTunerRotation()
    {
        //if (Capturer != null) if (Capturer.isCapturing) return;
        RotateTunerOnce(Time.deltaTime);
    }
    public void SortBpmList()
    {
        Bpm.Sort((Lanotalium.Chart.LanotaChangeBpm A, Lanotalium.Chart.LanotaChangeBpm B) =>
        {
            return A.Time.CompareTo(B.Time);
        });
    }
}
