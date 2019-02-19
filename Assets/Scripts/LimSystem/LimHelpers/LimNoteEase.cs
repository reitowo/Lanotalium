using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LimNoteEase : MonoBehaviour
{
    public static LimNoteEase Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        BuildCurve();
    }
    public Vector2 Point1, Point2;
    public AnimationCurve curve;
    public bool DemoMode; 
    public float CalculateEasedPercent(float Percent)
    {
        if (!DemoMode) return Mathf.Pow(2, 10 * (Percent / 100 - 1)) * 100;
        else return curve.Evaluate(Percent / 100) * 100;
    }
    private void BuildCurve()
    {
        curve = new AnimationCurve();
        for (int i = 0; i <= 20; ++i)
        {
            float t = 0.05f * i;
            Vector2 pt = CubicBeizer(t);
            curve.AddKey(pt.x, pt.y);
        }
    }
    private Vector2 CubicBeizer(float t)
    {
        return 3 * Mathf.Pow(1 - t, 2) * t * Point1 + 3 * Mathf.Pow(t, 2) * (1 - t) * Point2 + Mathf.Pow(t, 3) * new Vector2(1, 1);
    }
}
