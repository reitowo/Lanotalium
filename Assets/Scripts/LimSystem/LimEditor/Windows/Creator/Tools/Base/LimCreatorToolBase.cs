using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimCreatorToolBase : MonoBehaviour
{
    public LimCreatorManager CreatorManager;
    public RectTransform ToolRect, ViewRect;
    public float ToolHeight;

    private bool isFolded = true;

    public bool Fold
    {
        get
        {
            return isFolded;
        }
        set
        {
            isFolded = value;
            if (value)
            {
                ViewRect.sizeDelta = new Vector2(0, 0);
            }
            else
            {
                ViewRect.sizeDelta = new Vector2(0, ToolHeight);
            }
            CreatorManager.ArrangeCreatorsUi();
        }
    }
    public float Height
    {
        get
        {
            return ViewRect.sizeDelta.y + 30;
        }
        set
        {
            if (value < 30) return;
            ToolHeight = value - 30;
            if (!Fold)
            {
                ViewRect.sizeDelta = new Vector2(0, ToolHeight);
            }
            CreatorManager.ArrangeCreatorsUi();
        }
    }

    public void FoldTool()
    {
        if (Fold) Fold = false;
        else Fold = true;
    }
}
