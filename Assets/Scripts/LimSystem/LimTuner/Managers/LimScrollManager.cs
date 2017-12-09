using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimScrollManager : MonoBehaviour
{
    private bool isInitialized = false;
    public List<Lanotalium.Chart.LanotaScroll> Scroll;
    public LimTunerManager Tuner;

    void Update()
    {
        if (!isInitialized) return;
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

}
