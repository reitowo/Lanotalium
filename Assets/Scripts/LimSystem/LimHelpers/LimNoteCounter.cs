using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimNoteCounter : MonoBehaviour
{
    public Text NoteCountText;
    public LimTunerManager TunerManager;
    public float ApproximatelyThreshold = 0.0003f;
    private void Update()
    {
        if (TunerManager.isInitialized == false) return;
        NoteCountText.text = string.Format("{0}/{1}", CalculateNoteCount(TunerManager.ChartTime), CalculateNoteCount(TunerManager.MusicPlayerManager.Length));
    }

    private bool Approximately(float a,float b)
    {
        if (Mathf.Abs(a - b) < ApproximatelyThreshold) return true;
        return false;
    }
    private int CalculateHoldNoteCount(Lanotalium.Chart.LanotaHoldNote Hold, float Time)
    {
        int NoteCount = 0;
        float Duration = (Hold.Time + Hold.Duration) < Time ? Hold.Duration : (Time - Hold.Time);
        float JudgeDelta = 30f / TunerManager.BpmManager.CalculateBpm(Hold.Time);
        while (NoteCount * JudgeDelta < Duration && !Approximately(NoteCount * JudgeDelta, Duration))
            NoteCount++;
        if (Time > Hold.Time + Hold.Duration) NoteCount += 1;
        return NoteCount;
    }
    private int CalculateNoteCount(float Time)
    {
        if (TunerManager.isInitialized == false) return 0;
        int NoteCount = 0;
        foreach (Lanotalium.Chart.LanotaTapNote Tap in TunerManager.TapNoteManager.TapNote)
        {
            if (Tap.Time < Time) NoteCount++;
        }
        foreach (Lanotalium.Chart.LanotaHoldNote Hold in TunerManager.HoldNoteManager.HoldNote)
        {
            if (Hold.Time < Time) NoteCount += CalculateHoldNoteCount(Hold, Time);
        }
        return NoteCount;
    }
}
