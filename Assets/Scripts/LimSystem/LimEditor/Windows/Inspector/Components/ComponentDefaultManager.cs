using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentDefaultManager : MonoBehaviour
{
    public LimInspectorManager InspectorManager;
    public LimOperationManager OperationManager;
    public RectTransform ViewRect, ComponentRect;
    public Color InvalidColor, ValidColor;
    public Text LabelText, RadiusText, DegreeText, HeightText, RotationText;
    public InputField Radius, Degree, Height, Rotation;
    public Image RadiusImg, DegreeImg, HeightImg, RotationImg;
    public float UnFoldHeight;

    private bool isFolded = false,EnableValueChange = true;
    private float UiWidth;

    private void Start()
    {
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        RefreshUiWidth();
    }
    private void Update()
    {
        OnUiWidthChange();
    }
    public void RefreshUiWidth()
    {
        UiWidth = ViewRect.rect.width;
        float Ratio = UiWidth / 500f;
        Radius.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Degree.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Height.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Rotation.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        RadiusText.GetComponent<RectTransform>().sizeDelta = new Vector2(290 * Ratio, 30);
        DegreeText.GetComponent<RectTransform>().sizeDelta = new Vector2(290 * Ratio, 30);
        HeightText.GetComponent<RectTransform>().sizeDelta = new Vector2(290 * Ratio, 30);
        RotationText.GetComponent<RectTransform>().sizeDelta = new Vector2(290 * Ratio, 30);
    }
    public void OnUiWidthChange()
    {
        if (UiWidth != ViewRect.rect.width)
        {
            RefreshUiWidth();
        }
    }
    public void SetTexts()
    {
        LabelText.text = LimLanguageManager.TextDict["Component_Default_Label"];
        RadiusText.text = LimLanguageManager.TextDict["Component_Default_Radius"];
        DegreeText.text = LimLanguageManager.TextDict["Component_Default_Degree"];
        HeightText.text = LimLanguageManager.TextDict["Component_Default_Height"];
        RotationText.text = LimLanguageManager.TextDict["Component_Default_Rotation"];
    }
    public void Fold()
    {
        if (isFolded)
        {
            ViewRect.sizeDelta = new Vector2(0, UnFoldHeight); isFolded = false;
        }
        else if (!isFolded)
        {
            ViewRect.sizeDelta = new Vector2(0, 0); isFolded = true;
        }
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        InspectorManager.ArrangeComponentsUi();
    }
    public void LoadDefaultValues()
    {
        EnableValueChange = false;
        Lanotalium.Chart.LanotaDefault Default = OperationManager.TunerManager.ChartContainer.ChartData.LanotaDefault;
        Radius.text = Default.CamRou.ToString("f5");
        Degree.text = Default.CamTheta.ToString("f5");
        Height.text = Default.CamHeight.ToString("f5");
        Rotation.text = Default.Degree.ToString("f5");
        EnableValueChange = true;
    }
    public void OnRadiusChange()
    {
        if (!EnableValueChange) return;
        float RadiusTmp;
        if (!float.TryParse(Radius.text, out RadiusTmp))
        {
            RadiusImg.color = InvalidColor;
            return;
        }
        OperationManager.SetDefaultRadius(RadiusTmp);
        RadiusImg.color = ValidColor;
    }
    public void OnDegreeChange()
    {
        if (!EnableValueChange) return;
        float DegreeTmp;
        if (!float.TryParse(Degree.text, out DegreeTmp))
        {
            DegreeImg.color = InvalidColor;
            return;
        }
        OperationManager.SetDefaultDegree(DegreeTmp);
        DegreeImg.color = ValidColor;
    }
    public void OnHeightChange()
    {
        if (!EnableValueChange) return;
        float HeightTmp;
        if (!float.TryParse(Height.text, out HeightTmp))
        {
            HeightImg.color = InvalidColor;
            return;
        }
        OperationManager.SetDefaultHeight(HeightTmp);
        HeightImg.color = ValidColor;
    }
    public void OnRotationChange()
    {
        if (!EnableValueChange) return;
        float RotationTmp;
        if (!float.TryParse(Rotation.text, out RotationTmp))
        {
            RotationImg.color = InvalidColor;
            return;
        }
        OperationManager.SetDefaultRotation(RotationTmp);
        RotationImg.color = ValidColor;
    }
}
