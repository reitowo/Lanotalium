using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentMotionManager : MonoBehaviour
{
    public RectTransform ViewRect, ComponentRect;
    public int UnFoldHeight;
    public LimInspectorManager InspectorManager;
    public LimOperationManager OperationManager;
    public LimGizmoMotionManager GizmoMotionManager;
    public bool EnableValueChange = true;
    public InputField Timing, Duration, Cfmi, Ctp0, Ctp1;
    public Image TimingImg, DurationImg, CfmiImg, Ctp0Img, Ctp1Img;
    public Text LabelText, TimingText, DurationText, CfmiText, Ctp0Text, Ctp1Text, TypeText, ManuallyMotionText;
    public Color InvalidColor, ValidColor;
    public Button Type8, Type11;

    private bool isFolded = false;
    public Lanotalium.Editor.ComponentMotionMode Mode = Lanotalium.Editor.ComponentMotionMode.Idle;
    public int Index = 0;
    private float UiWidth;

    private void Start()
    {
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
        Timing.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Duration.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Cfmi.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Ctp0.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Ctp1.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Type8.GetComponent<RectTransform>().sizeDelta = new Vector2(100 * Ratio, 30);
        Type11.GetComponent<RectTransform>().sizeDelta = new Vector2(100 * Ratio, 30);
        Type11.GetComponent<RectTransform>().anchoredPosition = new Vector2(-105 * Ratio, -5);
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
        LabelText.text = LimLanguageManager.TextDict["Component_Motion_Label"];
        TimingText.text = LimLanguageManager.TextDict["Component_Motion_Timing"];
        DurationText.text = LimLanguageManager.TextDict["Component_Motion_Duration"];
        CfmiText.text = LimLanguageManager.TextDict["Component_Motion_Cfmi"];
        Ctp0Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp0"];
        Ctp1Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp1"];
        TypeText.text = LimLanguageManager.TextDict["Component_Motion_Type"];
        ManuallyMotionText.text = LimLanguageManager.TextDict["Component_Motion_ManuallyMotion"];
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
    public void SetMode(Lanotalium.Editor.ComponentMotionMode Mode, int Index = -1)
    {
        EnableValueChange = false;
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Idle || Index == -1)
        {
            gameObject.SetActive(false);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            gameObject.SetActive(true);
            if (OperationManager.TunerManager.CameraManager.Horizontal[Index].Type == 8)
            {
                Ctp0Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp0_Tp8"];
                Ctp1Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp1_Tp8"];
                Type8.interactable = false;
                Type11.interactable = true;
            }
            else if (OperationManager.TunerManager.CameraManager.Horizontal[Index].Type == 11)
            {
                Ctp0Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp0_Tp11"];
                Ctp1Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp1_Tp11"];
                Type8.interactable = true;
                Type11.interactable = false;
            }
            Timing.text = OperationManager.TunerManager.CameraManager.Horizontal[Index].Time.ToString("f5");
            Duration.text = OperationManager.TunerManager.CameraManager.Horizontal[Index].Duration.ToString("f5");
            Cfmi.text = OperationManager.TunerManager.CameraManager.Horizontal[Index].cfmi.ToString();
            Ctp0.text = OperationManager.TunerManager.CameraManager.Horizontal[Index].ctp.ToString("f5");
            Ctp1.text = OperationManager.TunerManager.CameraManager.Horizontal[Index].ctp1.ToString("f5");
            Ctp1.interactable = true;
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Vertical)
        {
            gameObject.SetActive(true);
            Ctp0Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp0_Tp10"];
            Ctp1Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp1_Tp10"];
            Timing.text = OperationManager.TunerManager.CameraManager.Vertical[Index].Time.ToString("f5");
            Duration.text = OperationManager.TunerManager.CameraManager.Vertical[Index].Duration.ToString("f5");
            Cfmi.text = OperationManager.TunerManager.CameraManager.Vertical[Index].cfmi.ToString();
            Ctp0.text = OperationManager.TunerManager.CameraManager.Vertical[Index].ctp.ToString("f5");
            Ctp1.interactable = false;
            Type8.interactable = false;
            Type11.interactable = false;
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Rotation)
        {
            gameObject.SetActive(true);
            Ctp0Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp0_Tp13"];
            Ctp1Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp1_Tp13"];
            Timing.text = OperationManager.TunerManager.CameraManager.Rotation[Index].Time.ToString("f5");
            Duration.text = OperationManager.TunerManager.CameraManager.Rotation[Index].Duration.ToString("f5");
            Cfmi.text = OperationManager.TunerManager.CameraManager.Rotation[Index].cfmi.ToString();
            Ctp0.text = OperationManager.TunerManager.CameraManager.Rotation[Index].ctp.ToString("f5");
            Ctp1.interactable = false;
            Type8.interactable = false;
            Type11.interactable = false;
        }
        this.Index = Index;
        this.Mode = Mode;
        EnableValueChange = true;
    }
    public void DeleteCurrentSelected()
    {
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            Destroy(OperationManager.TunerManager.CameraManager.Horizontal[Index].TimeLineGameObject);
            OperationManager.TunerManager.CameraManager.Horizontal.RemoveAt(Index);
            SetMode(Lanotalium.Editor.ComponentMotionMode.Idle);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Vertical)
        {
            Destroy(OperationManager.TunerManager.CameraManager.Vertical[Index].TimeLineGameObject);
            OperationManager.TunerManager.CameraManager.Vertical.RemoveAt(Index);
            SetMode(Lanotalium.Editor.ComponentMotionMode.Idle);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Rotation)
        {
            Destroy(OperationManager.TunerManager.CameraManager.Rotation[Index].TimeLineGameObject);
            OperationManager.TunerManager.CameraManager.Rotation.RemoveAt(Index);
            SetMode(Lanotalium.Editor.ComponentMotionMode.Idle);
        }
    }
    public void SetTypeTo8()
    {
        if (!EnableValueChange) return;
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            OperationManager.SetHorizontalType(OperationManager.TunerManager.CameraManager.Horizontal[Index], 8);
            Ctp0Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp0_Tp8"];
            Ctp1Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp1_Tp8"];
        }
        Type8.interactable = false;
        Type11.interactable = true;
    }
    public void SetTypeTo11()
    {
        if (!EnableValueChange) return;
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            OperationManager.SetHorizontalType(OperationManager.TunerManager.CameraManager.Horizontal[Index], 11);
            Ctp0Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp0_Tp11"];
            Ctp1Text.text = LimLanguageManager.TextDict["Component_Motion_Ctp1_Tp11"];
        }
        Type8.interactable = true;
        Type11.interactable = false;
    }
    public void OnTimingChange()
    {
        if (!EnableValueChange) return;
        float TimingTmp;
        if (!float.TryParse(Timing.text, out TimingTmp))
        {
            TimingImg.color = InvalidColor;
            return;
        }
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            if (!OperationManager.CheckHorizontalTimeValid(OperationManager.TunerManager.CameraManager.Horizontal[Index], TimingTmp))
            {
                TimingImg.color = InvalidColor;
                return;
            }
            OperationManager.SetHorizontalTime(OperationManager.TunerManager.CameraManager.Horizontal[Index], TimingTmp);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Vertical)
        {
            if (!OperationManager.CheckVerticalTimeValid(OperationManager.TunerManager.CameraManager.Vertical[Index], TimingTmp))
            {
                TimingImg.color = InvalidColor;
                return;
            }
            OperationManager.SetVerticalTime(OperationManager.TunerManager.CameraManager.Vertical[Index], TimingTmp);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Rotation)
        {
            if (!OperationManager.CheckRotationTimeValid(OperationManager.TunerManager.CameraManager.Rotation[Index], TimingTmp))
            {
                TimingImg.color = InvalidColor;
                return;
            }
            OperationManager.SetRotationTime(OperationManager.TunerManager.CameraManager.Rotation[Index], TimingTmp);
        }
        TimingImg.color = ValidColor;
    }
    public void OnDurationChange()
    {
        if (!EnableValueChange) return;
        float DurationTmp;
        if (!float.TryParse(Duration.text, out DurationTmp))
        {
            DurationImg.color = InvalidColor;
            return;
        }
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            if (!OperationManager.CheckHorizontalDurationValid(OperationManager.TunerManager.CameraManager.Horizontal[Index], DurationTmp))
            {
                DurationImg.color = InvalidColor;
                return;
            }
            OperationManager.SetHorizontalDuration(OperationManager.TunerManager.CameraManager.Horizontal[Index], DurationTmp);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Vertical)
        {
            if (!OperationManager.CheckVerticalDurationValid(OperationManager.TunerManager.CameraManager.Vertical[Index], DurationTmp))
            {
                DurationImg.color = InvalidColor;
                return;
            }
            OperationManager.SetVerticalDuration(OperationManager.TunerManager.CameraManager.Vertical[Index], DurationTmp);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Rotation)
        {
            if (!OperationManager.CheckRotationDurationValid(OperationManager.TunerManager.CameraManager.Rotation[Index], DurationTmp))
            {
                DurationImg.color = InvalidColor;
                return;
            }
            OperationManager.SetRotationDuration(OperationManager.TunerManager.CameraManager.Rotation[Index], DurationTmp);
        }
        DurationImg.color = ValidColor;
    }
    public void OnEaseChange()
    {
        if (!EnableValueChange) return;
        int EaseTmp;
        if (!int.TryParse(Cfmi.text, out EaseTmp))
        {
            CfmiImg.color = InvalidColor;
            return;
        }
        if (EaseTmp < 0 || EaseTmp > 12)
        {
            CfmiImg.color = InvalidColor;
            return;
        }
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            OperationManager.SetHorizontalEase(OperationManager.TunerManager.CameraManager.Horizontal[Index], EaseTmp);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Vertical)
        {
            OperationManager.SetVerticalEase(OperationManager.TunerManager.CameraManager.Vertical[Index], EaseTmp);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Rotation)
        {
            OperationManager.SetRotationEase(OperationManager.TunerManager.CameraManager.Rotation[Index], EaseTmp);
        }
        CfmiImg.color = ValidColor;
    }
    public void OnCtp0Change()
    {
        if (!EnableValueChange) return;
        float Ctp0Tmp;
        if (!float.TryParse(Ctp0.text, out Ctp0Tmp))
        {
            Ctp0Img.color = InvalidColor;
            return;
        }
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            OperationManager.SetHorizontalDegree(OperationManager.TunerManager.CameraManager.Horizontal[Index], Ctp0Tmp);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Vertical)
        {
            OperationManager.SetVerticalHeight(OperationManager.TunerManager.CameraManager.Vertical[Index], Ctp0Tmp);
        }
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Rotation)
        {
            OperationManager.SetRotationDegree(OperationManager.TunerManager.CameraManager.Rotation[Index], Ctp0Tmp);
        }
        Ctp0Img.color = ValidColor;
    }
    public void OnCtp1Change()
    {
        if (!EnableValueChange) return;
        float Ctp1Tmp;
        if (!float.TryParse(Ctp1.text, out Ctp1Tmp))
        {
            Ctp1Img.color = InvalidColor;
            return;
        }
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            OperationManager.SetHorizontalRadius(OperationManager.TunerManager.CameraManager.Horizontal[Index], Ctp1Tmp);
        }
        Ctp1Img.color = ValidColor;
    }
    public void OpenManuallyMotionEditor()
    {
        OperationManager.TunerManager.MusicPlayerManager.isPlaying = false;
        if (Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal) GizmoMotionManager.Edit(OperationManager.TunerManager.CameraManager.Horizontal[Index]);
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Vertical) GizmoMotionManager.Edit(OperationManager.TunerManager.CameraManager.Vertical[Index]);
        else if (Mode == Lanotalium.Editor.ComponentMotionMode.Rotation) GizmoMotionManager.Edit(OperationManager.TunerManager.CameraManager.Rotation[Index]);
    }
}
