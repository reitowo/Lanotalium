using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LimScanTime : MonoBehaviour
{
    public LimTunerManager TunerManager;
    private static List<float> Starts = new List<float>();
    private static List<float> Ends = new List<float>();
    private static int RangePairCount = 0;
    public bool Enable = true;
    public List<float> KeyTimes = new List<float>();

    public static bool IsTapNoteinScanRange(Lanotalium.Chart.LanotaTapNote Note)
    {
        if (RangePairCount == 0) return true;
        for (int i = 0; i < RangePairCount; ++i)
        {
            if (Note.Time + 0.1 >= Starts[i] && Note.Time <= Ends[i]) return true;
        }
        return false;
    }
    public static bool IsHoldNoteinScanRange(Lanotalium.Chart.LanotaHoldNote Note)
    {
        if (RangePairCount == 0) return true;
        for (int i = 0; i < RangePairCount; ++i)
        {
            if (Note.Time >= Starts[i] && Note.Time <= Ends[i]) return true;
            else if (Note.Time <= Starts[i] && Note.Time + Note.Duration >= Ends[i]) return true;
            else if (Note.Time + Note.Duration >= Starts[i] && Note.Time + Note.Duration <= Ends[i]) return true;
            else if (Note.Time >= Starts[i] && Note.Time + Note.Duration <= Ends[i]) return true;
        } 
        return false;
    }
    private float CalculateTimeByPercent(float Percent, int Depth = 0)
    {
        float EndPercent = Percent;
        int StartScroll = 0;
        float StartPercent = 100.0f;
        for (int i = 0; i < TunerManager.ScrollManager.Scroll.Count - 1; ++i)
        {
            if (TunerManager.ChartTime >= TunerManager.ScrollManager.Scroll[i].Time && TunerManager.ChartTime < TunerManager.ScrollManager.Scroll[i + 1].Time) StartScroll = i;
        }
        if (TunerManager.ChartTime >= TunerManager.ScrollManager.Scroll[TunerManager.ScrollManager.Scroll.Count - 1].Time) StartScroll = TunerManager.ScrollManager.Scroll.Count - 1;
        int EndScroll = TunerManager.ScrollManager.Scroll.Count - 1;
        int BreakLocation = -1;
        int DepthCount = 0;
        float Delta = 0, EndTime = 0;
        if (StartScroll != EndScroll)
        {
            for (int i = StartScroll; i <= EndScroll; ++i)
            {
                if (i == StartScroll)
                {
                    Delta = (TunerManager.ScrollManager.Scroll[i + 1].Time - TunerManager.ChartTime) * TunerManager.ScrollManager.Scroll[i].Speed * 10 * TunerManager.ChartPlaySpeed;
                    if ((StartPercent - Delta < EndPercent && StartPercent >= EndPercent) || (StartPercent - Delta > EndPercent && StartPercent <= EndPercent)) { if (Depth == DepthCount) { BreakLocation = i; break; } else { DepthCount++; } }
                    StartPercent -= Delta;
                }
                else if (i != EndScroll && i != StartScroll)
                {
                    Delta = (TunerManager.ScrollManager.Scroll[i + 1].Time - TunerManager.ScrollManager.Scroll[i].Time) * TunerManager.ScrollManager.Scroll[i].Speed * 10 * TunerManager.ChartPlaySpeed;
                    if ((StartPercent - Delta < EndPercent && StartPercent >= EndPercent) || (StartPercent - Delta > EndPercent && StartPercent <= EndPercent)) { if (Depth == DepthCount) { BreakLocation = i; break; } else { DepthCount++; } }
                    StartPercent -= Delta;
                }
                else if (i == EndScroll)
                {
                    Delta = (TunerManager.MediaPlayerManager.Length - TunerManager.ScrollManager.Scroll[i].Time) * TunerManager.ScrollManager.Scroll[i].Speed * 10 * TunerManager.ChartPlaySpeed;
                    if ((StartPercent - Delta < EndPercent && StartPercent >= EndPercent) || (StartPercent - Delta > EndPercent && StartPercent <= EndPercent)) { if (Depth == DepthCount) { BreakLocation = i; break; } else { DepthCount++; } }
                    StartPercent -= Delta;
                }
            }
        }
        else if (StartScroll == EndScroll)
        {
            Delta = (StartPercent - EndPercent);
            EndTime = Delta / (TunerManager.ScrollManager.Scroll[EndScroll].Speed * 10 * TunerManager.ChartPlaySpeed) + TunerManager.ChartTime;
            if (EndTime > TunerManager.MediaPlayerManager.Length) return TunerManager.MediaPlayerManager.Length;
            else return EndTime;
        }
        if (BreakLocation == StartScroll)
        {
            Delta = (StartPercent - EndPercent);
            EndTime = Delta / (TunerManager.ScrollManager.Scroll[BreakLocation].Speed * 10 * TunerManager.ChartPlaySpeed) + TunerManager.ChartTime;
        }
        else if (BreakLocation != -1)
        {
            Delta = (StartPercent - EndPercent);
            EndTime = Delta / (TunerManager.ScrollManager.Scroll[BreakLocation].Speed * 10 * TunerManager.ChartPlaySpeed) + TunerManager.ScrollManager.Scroll[BreakLocation].Time;
        }
        else if (BreakLocation == -1) EndTime = TunerManager.MediaPlayerManager.Length;
        if (EndTime > TunerManager.MediaPlayerManager.Length) return TunerManager.MediaPlayerManager.Length;
        else return EndTime;
    }
    private void TryCalculateKeyTimes()
    {
        bool KeyTime1End = false, KeyTime2End = false;
        float KeyTime1 = 0, KeyTime2 = 0;
        int Depth = 0;
        do
        {
            KeyTime1 = CalculateTimeByPercent(100, Depth);
            if (KeyTimes.Contains(KeyTime1)) KeyTime1End = true;
            else KeyTimes.Add(KeyTime1);
            KeyTime2 = CalculateTimeByPercent(77, Depth);
            if (KeyTimes.Contains(KeyTime2)) KeyTime2End = true;
            else KeyTimes.Add(KeyTime2);
            Depth++;
            if (Depth == 20)
            {
                Enable = false;
                throw new System.Exception("Too Deep Exception");
            }
        }
        while (!KeyTime1End || !KeyTime2End);
    }
    private void TryGeneratePairs()
    {
        KeyTimes = KeyTimes.Distinct().ToList();
        KeyTimes.Sort();
        if (TunerManager.ScrollManager.IsBackwarding) KeyTimes.RemoveAt(0);
        if (KeyTimes.Count % 2 == 1)
        {
            if (KeyTimes[KeyTimes.Count - 1] == TunerManager.MediaPlayerManager.Length)
            {
                KeyTimes.RemoveAt(KeyTimes.Count - 1);
            }
            else KeyTimes.Add(TunerManager.MediaPlayerManager.Length);
        }
        for (int i = 0; i < KeyTimes.Count; i += 2)
        {
            Starts.Add(KeyTimes[i]);
            Ends.Add(KeyTimes[i + 1]);
            RangePairCount++;
        }
        for (int i = 0; i < RangePairCount; ++i)
        {
            if (Starts[i] > Ends[i])
            {
                float t = Starts[i];
                Starts[i] = Ends[i];
                Ends[i] = t;
            }
        }
    }
    void Update()
    {
        if (LimSystem.ChartContainer == null) return;
        if (!TunerManager.isInitialized) return;
        RangePairCount = 0;
        if (!Enable) return;
        KeyTimes.Clear();
        Starts.Clear();
        Ends.Clear();
        TryCalculateKeyTimes();
        TryGeneratePairs();
    }
}
