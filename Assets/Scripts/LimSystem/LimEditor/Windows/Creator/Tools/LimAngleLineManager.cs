using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimAngleLineManager : MonoBehaviour
{
    public LimCreatorToolBase ToolBase;
    public LimTunerManager TunerManager;
    public InputField AnglelineInputField;
    public Image AnglelineImg;
    public Color InvalidColor, ValidColor;
    public GameObject AnglelinePrefab;
    public Transform AnglelineTransform;
    public Toggle EnableToggle;
    private List<float> Angles = new List<float>();
    private List<float> RotatedAngles = new List<float>();
    private List<LineRenderer> Anglelines = new List<LineRenderer>();

    public bool Enable
    {
        get
        {
            return EnableToggle.isOn;
        }
        set
        {
            EnableToggle.isOn = value;
            if (!value)
            { foreach (LineRenderer g in Anglelines) Destroy(g.gameObject); Anglelines.Clear(); }
        }
    }

    private void Update()
    {
        if (!Enable) return;
        UpdateRotatedAngles();
        UpdateAnglelines();
    }

    private float CalculateRoundedDegree(float Degree)
    {
        while (Degree > 360) Degree -= 360;
        while (Degree < 0) Degree += 360;
        return Degree;
    }
    public float FindAttachToAnglelineByDegree(float Degree, float Threshold)
    {
        for (int i = 0; i < RotatedAngles.Count; ++i)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(Degree, RotatedAngles[i])) < Threshold) return RotatedAngles[i];
        }
        return Degree;
    }

    private void GenerateCorrectQuantityAngleline(int AnglelineCount)
    {
        int DeltaQuantity = AnglelineCount - Anglelines.Count;
        if (DeltaQuantity == 0) return;
        if (DeltaQuantity < 0)
        {
            for (int i = 0; i > DeltaQuantity; i--)
            {
                Destroy(Anglelines[Anglelines.Count - 1].gameObject);
                Anglelines.RemoveAt(Anglelines.Count - 1);
            }
        }
        else if (DeltaQuantity > 0)
        {
            for (int i = 0; i < DeltaQuantity; ++i)
            {
                Anglelines.Add(Instantiate(AnglelinePrefab, AnglelineTransform).GetComponent<LineRenderer>());
            }
        }
    }
    private void UpdateRotatedAngles()
    {
        RotatedAngles.Clear();
        foreach (float a in Angles) RotatedAngles.Add(CalculateRoundedDegree(a + TunerManager.CameraManager.CurrentRotation));
    }
    private void UpdateAnglelines()
    {
        GenerateCorrectQuantityAngleline(Angles.Count);
        int Index = 0;
        foreach (LineRenderer Line in Anglelines)
        {
            float RotatedDegree = RotatedAngles[Index];
            Line.SetPosition(1, new Vector3(-10 * Mathf.Sin(RotatedDegree * Mathf.Deg2Rad), 0, -10 * Mathf.Cos(RotatedDegree * Mathf.Deg2Rad)));
            Index++;
        }
    }

    private bool isAngleExisted(float Angle)
    {
        foreach (float iAngle in Angles) if (iAngle == Angle) return true;
        return false;
    }
    private void SortAngles()
    {
        if (Angles.Count == 0 || Angles.Count == 1) return;
        Angles.Sort((float a, float b) => { return a.CompareTo(b); });
    }
    private void TryAddAngle(float Angle)
    {
        while (Angle > 360) Angle -= 360;
        while (Angle < 0) Angle += 360;
        if (!isAngleExisted(Angle)) Angles.Add(Angle);
    }
    private bool TryParseRange(string s)
    {
        float Begin, End;
        int Part;
        string[] Split1 = s.Split('>');
        if (Split1.Length != 2) return false;
        string[] Split2 = Split1[1].Split('/');
        if (Split2.Length != 2) return false;
        if (!float.TryParse(Split1[0], out Begin)) return false;
        if (!float.TryParse(Split2[0], out End)) return false;
        if (!int.TryParse(Split2[1], out Part)) return false;
        if (Begin >= End) return false;
        if (Part <= 0 || Part > 24) return false;
        float DeltaAngle = (End - Begin) / Part;
        TryAddAngle(Begin);
        for (int i = 1; i <= Part; ++i)
        {
            TryAddAngle(Begin + i * DeltaAngle);
        }
        return true;
    }
    private bool TryParseAngle(string s)
    {
        float Angle;
        if (!float.TryParse(s, out Angle)) return false;
        TryAddAngle(Angle);
        return true;
    }
    private bool TryParse(string s)
    {
        Angles.Clear();
        string[] AngleStrs = s.Split(',');
        foreach (string AngleStr in AngleStrs)
        {
            if (!TryParseAngle(AngleStr))
                if (!TryParseRange(AngleStr))
                    return false;
        }
        SortAngles();
        return true;
    }

    public void OnAnglelineInputFieldChange()
    {
        if (!TryParse(AnglelineInputField.text))
        {
            AnglelineImg.color = InvalidColor;
            return;
        }
        AnglelineImg.color = ValidColor;
    }
    public void OnEnableToggleChange()
    {
        Enable = EnableToggle.isOn;
    }
}
