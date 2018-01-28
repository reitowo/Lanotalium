using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimTutorialComponent : MonoBehaviour
{
    public string RectTransformId;
    private void Awake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;
        if (LimTutorialManager.UIRectTransformDict.ContainsKey(RectTransformId)) return;
        LimTutorialManager.UIRectTransformDict.Add(RectTransformId, rectTransform);
    }
    private void OnDestroy()
    {
        LimTutorialManager.UIRectTransformDict.Remove(RectTransformId);
    }
}
