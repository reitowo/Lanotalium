using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimTunerHeadManager : MonoBehaviour
{
    public Lanotalium.Editor.TunerHeadMode Mode
    {
        get
        {
            if (RelativeParentRect == TunerWindowCanvas) return Lanotalium.Editor.TunerHeadMode.InEditor;
            else if (RelativeParentRect == TunerWindowCanvas) return Lanotalium.Editor.TunerHeadMode.InTuner;
            else
            {
                Mode = Lanotalium.Editor.TunerHeadMode.InTuner;
                return Lanotalium.Editor.TunerHeadMode.InTuner;
            }
        }
        set
        {
            if (value == Lanotalium.Editor.TunerHeadMode.InTuner)
            {
                RelativeParentRect = PreserveCanvasRect;
                HeadRect.SetParent(RelativeParentRect, false);
            }
            else
            {
                RelativeParentRect = TunerWindowCanvas;
                HeadRect.SetParent(RelativeParentRect, false);
                HeadRect.SetSiblingIndex(4);
            }
        }
    }

    public RectTransform PreserveCanvasRect, TunerWindowCanvas;
    public RectTransform HeadRect, RelativeParentRect;
    public Text ChartName, ChartDesignerHint;
    public InputField DesignerInputField;

    private void Start()
    {
        Mode = Lanotalium.Editor.TunerHeadMode.InEditor;
        if (LimSystem.ChartContainer != null) ChartName.text = LimSystem.ChartContainer.ChartProperty.ChartName;
        DesignerInputField.text = LimSystem.Preferences.Designer;
    }
    public void SetTexts()
    {
        ChartDesignerHint.text = LimLanguageManager.TextDict["Window_Tuner_Head_Designer"];
    }
    private void Update()
    {
        HeadRect.sizeDelta = new Vector2(0, RelativeParentRect.sizeDelta.y * 0.07711f);
    }
    public void OnDesignerChange()
    {
        LimSystem.Preferences.Designer = DesignerInputField.text;
    }
}
