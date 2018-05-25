using System.Collections;
using System.Collections.Generic;
using Lanotalium.Chart;
using UnityEngine;

public class LimScrollManager : MonoBehaviour
{
    private bool isInitialized = false;
    private List<LanotaScroll> _Scroll;
    private List<LanotaScroll> _DisabledSpeedList = new List<LanotaScroll>() { new LanotaScroll() { Speed = 1, Time = -10 } };
    public LimTunerManager Tuner;
    public bool IsBackwarding = false, IsStopped = false, WillBackward = false;
    public float CurrentScrollSpeed;
    public bool DisableChartSpeed = false;
    public List<LanotaScroll> Scroll
    {
        get
        {
            if (DisableChartSpeed) return _DisabledSpeedList;
            return _Scroll;
        }
        set
        {
            _Scroll = value;
        }
    }

    void Update()
    {
        if (!isInitialized) return;
        UpdateCurrentScrollSpeed();
    }
    public void Initialize(List<Lanotalium.Chart.LanotaScroll> ScrollData)
    {
        Scroll = ScrollData;
        isInitialized = true;
    }
    public void SortScrollList()
    {
        Scroll.Sort((Lanotalium.Chart.LanotaScroll A, Lanotalium.Chart.LanotaScroll B) =>
        {
            return A.Time.CompareTo(B.Time);
        });
    }
    public void UpdateCurrentScrollSpeed()
    {
        IsBackwarding = false;
        IsStopped = false;
        if (Scroll.Count == 0)
        {
            CurrentScrollSpeed = 1;
            return;
        }
        for (int i = 0; i < Scroll.Count - 1; ++i)
        {
            if (Tuner.ChartTime >= Scroll[i].Time && Tuner.ChartTime < Scroll[i + 1].Time)
            {
                CurrentScrollSpeed = Scroll[i].Speed;
                if (Scroll[i].Speed < 0) IsBackwarding = true;
                else if (Scroll[i].Speed == 0) IsStopped = true;
                if (IsStopped)
                {
                    int k = i;
                    while (k < Scroll.Count - 1)
                    {
                        k++;
                        if (Scroll[k].Speed < 0)
                        {
                            WillBackward = true;
                            break;
                        }
                        else if (Scroll[k].Speed > 0)
                        {
                            WillBackward = false;
                            break;
                        }
                    }
                }
                else WillBackward = false;
                break;
            }
        }
        if (Tuner.ChartTime > Scroll[Scroll.Count - 1].Time)
        {
            CurrentScrollSpeed = Scroll[Scroll.Count - 1].Speed;
            if (Scroll[Scroll.Count - 1].Speed < 0) IsBackwarding = true;
            else if (Scroll[Scroll.Count - 1].Speed == 0) IsStopped = true;
        }
    }
}

