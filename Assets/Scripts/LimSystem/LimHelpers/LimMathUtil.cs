using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimMathUtil
{
    public static bool InRange(float Value, float Min, float Max)
    {
        return (Value >= Min && Value <= Max && Min <= Max) ? true : false;
    }
    public static bool InRect(Vector2 Point,RectTransform Rect)
    {
        if (InRange(Point.x, Rect.anchoredPosition.x, Rect.anchoredPosition.x + Rect.sizeDelta.x) && InRange(Point.y, Rect.anchoredPosition.y - Rect.sizeDelta.y, Rect.anchoredPosition.y)) return true;
        else return false;
    }

}
