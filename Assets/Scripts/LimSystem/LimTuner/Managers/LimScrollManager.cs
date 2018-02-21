using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimScrollManager : MonoBehaviour
{
    private bool isInitialized = false;
    public List<Lanotalium.Chart.LanotaScroll> Scroll;
    public LimTunerManager Tuner;
    public bool IsBackwarding = false, IsStopped = false;
    public float CurrentScrollSpeed;

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
        for (int i = 0; i < Scroll.Count - 1; ++i)
        {
            if (Tuner.ChartTime >= Scroll[i].Time && Tuner.ChartTime < Scroll[i + 1].Time)
            {
                CurrentScrollSpeed = Scroll[i].Speed;
                if (Scroll[i].Speed < 0) IsBackwarding = true;
                else if (Scroll[i].Speed == 0) IsStopped = true;
            }
        }
        if (Tuner.ChartTime > Scroll[Scroll.Count - 1].Time)
        {
            CurrentScrollSpeed = Scroll[Scroll.Count - 1].Speed;
            if (Scroll[Scroll.Count - 1].Speed < 0) IsBackwarding = true;
            else if(Scroll[Scroll.Count - 1].Speed == 0) IsStopped = true;
        }
    }
}
