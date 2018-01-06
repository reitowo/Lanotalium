using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimWindowArranger : MonoBehaviour
{
    public List<LimWindowManager> Windows = new List<LimWindowManager>();
    private void Start()
    {
        foreach (LimWindowManager Window in Windows)
        {
            Window.OnWindowSorting.AddListener(SortWindows);
        }
        SortWindows(null);
    }
    public void SortWindows(LimWindowManager Caller)
    {
        Windows.Sort((LimWindowManager a, LimWindowManager b) => { return a.SortingOrder.CompareTo(b.SortingOrder); });
        int SortingOrder = 0;
        foreach (LimWindowManager Window in Windows)
        {
            if (Window == Caller) continue;
            Window.SortingOrder = SortingOrder;
            SortingOrder += Window.RequiredSortingOrder;
        }
        if (Caller == null) return;
        Caller.SortingOrder = SortingOrder;
    }
}
