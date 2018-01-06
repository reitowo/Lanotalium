using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimSpectrumBarManager : MonoBehaviour
{
    public LineRenderer Bar;
    public float Amplitude
    {
        get
        {
            return Bar.GetPosition(1).y;
        }
        set
        {
            Bar.SetPosition(1, new Vector3(0, value, 0));
        }
    }
    public float Width
    {
        get
        {
            return Bar.startWidth * 9.35f;
        }
        set
        {
            Bar.startWidth = value / 9.35f;
            Bar.endWidth = value / 9.35f;
        }
    }
}
