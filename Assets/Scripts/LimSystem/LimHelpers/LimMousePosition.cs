using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimMousePosition : MonoBehaviour
{
    public static Vector3 LastMousePosition;
    public static Vector3 MousePosition;
    public static Vector2 DeltaPosition
    {
        get
        {
            return new Vector2(MousePosition.x - LastMousePosition.x, MousePosition.y - LastMousePosition.y);
        }
    }

    void Update()
    {
        float Ratio = 1920f / Screen.width;
        LastMousePosition = MousePosition;
        MousePosition.x = Input.mousePosition.x * Ratio;
        MousePosition.y = (Input.mousePosition.y - Screen.height) * Ratio;
    }
    public static Vector2 MousePositionInWindow(RectTransform Window)
    {
        return new Vector2(MousePosition.x - Window.anchoredPosition.x, MousePosition.y - Window.anchoredPosition.y);
    }
    public static bool IsMouseOverWindow(RectTransform Window)
    {
        Vector2 PosInWindow = MousePositionInWindow(Window);
        if (PosInWindow.x < 0 || PosInWindow.x > Window.sizeDelta.x) return false;
        if (PosInWindow.y > 0 || PosInWindow.y < -Window.sizeDelta.y) return false;
        return true;
    }

}
