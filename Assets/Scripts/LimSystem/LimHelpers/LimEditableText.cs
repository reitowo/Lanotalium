using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimEditableText : MonoBehaviour
{
    private class BackgroundAlpha
    {
        public static float Normal = 0;
        public static float Empty = 0.2f;
        public static float Over = 0.5f;
        public static float Editing = 1;
    }

    public Image BackgroundImg;
    public Text PlaceHolderText;
    public InputField InputFieldRef;
    [HideInInspector]
    public bool isMouseOver, isEditing;
    public string Text
    {
        get
        {
            return InputFieldRef.text;
        }
        set
        {
            InputFieldRef.text = value;
        }
    }
    public string PlaceHolder
    {
        get
        {
            return PlaceHolderText.text;
        }
        set
        {
            PlaceHolderText.text = value;
        }
    }

    private void Update()
    {
        UpdateBackgroundAlpha();
    }
    private void UpdateBackgroundAlpha()
    {
        if (isEditing) { if (BackgroundImg.color.a != BackgroundAlpha.Editing) { BackgroundImg.color = new Color(0.9f, 0.9f, 0.9f, BackgroundAlpha.Editing); return; } else return; };
        if (isMouseOver) { if (BackgroundImg.color.a != BackgroundAlpha.Over) { BackgroundImg.color = new Color(0.9f, 0.9f, 0.9f, BackgroundAlpha.Over); return; } else return; };
        if (InputFieldRef.text == string.Empty) { if (BackgroundImg.color.a != BackgroundAlpha.Empty) { BackgroundImg.color = new Color(0.9f, 0.9f, 0.9f, BackgroundAlpha.Empty); return; } else return; };
        if (BackgroundImg.color.a != BackgroundAlpha.Normal) { BackgroundImg.color = new Color(0.9f, 0.9f, 0.9f, BackgroundAlpha.Normal); return; } else return;
    }
    public void OnPointerEnterText()
    {
        isMouseOver = true;
    }
    public void OnPointerExitText()
    {
        isMouseOver = false;
    }
    public void OnPointerSelectText()
    {
        isEditing = true;
    }
    public void OnPointerDeSelectText()
    {
        isEditing = false;
    }
}
