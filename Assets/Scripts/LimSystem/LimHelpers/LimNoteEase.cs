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
    public static float CalculateEasedPercent(float Percent)
    {
        return DOVirtual.EasedValue(0, 100, Percent / 100, Ease.InCirc);
    }
}
