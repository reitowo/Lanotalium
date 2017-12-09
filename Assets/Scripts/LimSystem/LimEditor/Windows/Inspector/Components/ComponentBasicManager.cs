using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ComponentBasicManager : MonoBehaviour
{
    public RectTransform ViewRect, ComponentRect;
    public int UnFoldHeight;
    public InputField Timing, Degree;
    public Image TimingImg, DegreeImg;
    public Text LabelText, SizeText, TimingText, DegreeText, CombinationText, AbsoluteText;
    public Toggle Combination;
    public Dropdown Size;
    public Color InvalidColor, ValidColor;
    public LimInspectorManager InspectorManager;
    public LimOperationManager OperationManager;
    public Toggle Absolute;

    private bool isFolded = false, EnableValueChange = false, isAbsolute = false;
    private Lanotalium.Editor.ComponentBasicMode Mode = Lanotalium.Editor.ComponentBasicMode.Idle;
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
        Timing.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Degree.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Size.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
        Absolute.GetComponent<RectTransform>().anchoredPosition = new Vector2(-210 * Ratio, -5);
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
        LabelText.text = LimLanguageManager.TextDict["Component_Basic_Label"];
        SizeText.text = LimLanguageManager.TextDict["Component_Basic_Size"];
        TimingText.text = LimLanguageManager.TextDict["Component_Basic_Timing"];
        DegreeText.text = LimLanguageManager.TextDict["Component_Basic_Degree"];
        CombinationText.text = LimLanguageManager.TextDict["Component_Basic_Combination"];
        AbsoluteText.text = LimLanguageManager.TextDict["Component_Basic_Degree_Absolute"];
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
    private void EnsureDropdownUseable(Dropdown DropDown)
    {
        Transform DropdownList = DropDown.transform.Find("Dropdown List");
        if (DropdownList == null) return;
        Destroy(DropdownList.gameObject);
    }
    public void OnSelectChange()
    {
        EnableValueChange = false;
        EnsureDropdownUseable(Size);
        if (OperationManager.SelectedTapNote.Count == 1 && OperationManager.SelectedHoldNote.Count == 0)
        {
            Mode = Lanotalium.Editor.ComponentBasicMode.Work;
            Timing.text = OperationManager.SelectedTapNote[0].Time.ToString("f5");
            if (isAbsolute) Degree.text = (OperationManager.SelectedTapNote[0].Degree + OperationManager.TunerManager.CameraManager.CalculateCameraRotation(OperationManager.SelectedTapNote[0].Time)).ToString("f5");
            else Degree.text = OperationManager.SelectedTapNote[0].Degree.ToString("f5");
            Combination.isOn = OperationManager.SelectedTapNote[0].Combination;
            Size.value = OperationManager.SelectedTapNote[0].Size;
            gameObject.SetActive(true);
        }
        else if (OperationManager.SelectedTapNote.Count == 0 && OperationManager.SelectedHoldNote.Count == 1)
        {
            Mode = Lanotalium.Editor.ComponentBasicMode.Work;
            Timing.text = OperationManager.SelectedHoldNote[0].Time.ToString("f5");
            if (isAbsolute) Degree.text = (OperationManager.SelectedHoldNote[0].Degree + OperationManager.TunerManager.CameraManager.CalculateCameraRotation(OperationManager.SelectedHoldNote[0].Time)).ToString("f5");
            else Degree.text = OperationManager.SelectedHoldNote[0].Degree.ToString("f5");
            Combination.isOn = OperationManager.SelectedHoldNote[0].Combination;
            Size.value = OperationManager.SelectedHoldNote[0].Size;
            gameObject.SetActive(true);
        }
        else if (OperationManager.SelectedTapNote.Count + OperationManager.SelectedHoldNote.Count > 1)
        {
            Mode = Lanotalium.Editor.ComponentBasicMode.Work;
            Timing.text = "-";
            Degree.text = "-";
            Combination.isOn = false;
            Size.value = 0;
            gameObject.SetActive(true);
        }
        else if (OperationManager.SelectedTapNote.Count == 0 && OperationManager.SelectedHoldNote.Count == 0)
        {
            Mode = Lanotalium.Editor.ComponentBasicMode.Idle;
            gameObject.SetActive(false);
        }
        EnableValueChange = true;
    }
    public void OnAbsoluteChange()
    {
        isAbsolute = Absolute.isOn;
        OnSelectChange();
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
        if (Mode == Lanotalium.Editor.ComponentBasicMode.Work)
        {
            foreach (Lanotalium.Chart.LanotaTapNote Tap in OperationManager.SelectedTapNote) OperationManager.SetTapNoteTime(Tap, TimingTmp);
            foreach (Lanotalium.Chart.LanotaHoldNote Hold in OperationManager.SelectedHoldNote) OperationManager.SetHoldNoteTime(Hold, TimingTmp);
        }
        TimingImg.color = ValidColor;
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
        if (Mode == Lanotalium.Editor.ComponentBasicMode.Work)
        {
            foreach (Lanotalium.Chart.LanotaTapNote Tap in OperationManager.SelectedTapNote) OperationManager.SetTapNoteDegree(Tap, DegreeTmp, isAbsolute);
            foreach (Lanotalium.Chart.LanotaHoldNote Hold in OperationManager.SelectedHoldNote) OperationManager.SetHoldNoteDegree(Hold, DegreeTmp, isAbsolute);
        }
        DegreeImg.color = ValidColor;
    }
    public void OnSizeChange()
    {
        if (!EnableValueChange) return;
        int SizeTmp = Size.value;
        if (Mode == Lanotalium.Editor.ComponentBasicMode.Work)
        {
            foreach (Lanotalium.Chart.LanotaTapNote Tap in OperationManager.SelectedTapNote) OperationManager.SetTapNoteSize(Tap, SizeTmp);
            foreach (Lanotalium.Chart.LanotaHoldNote Hold in OperationManager.SelectedHoldNote) OperationManager.SetHoldNoteSize(Hold, SizeTmp);
        }
    }
    public void OnCombinationChange()
    {
        if (!EnableValueChange) return;
        bool CombinationTmp = Combination.isOn;
        if (Mode == Lanotalium.Editor.ComponentBasicMode.Work)
        {
            foreach (Lanotalium.Chart.LanotaTapNote Tap in OperationManager.SelectedTapNote) OperationManager.SetTapNoteCombination(Tap, CombinationTmp);
            foreach (Lanotalium.Chart.LanotaHoldNote Hold in OperationManager.SelectedHoldNote) OperationManager.SetHoldNoteCombination(Hold, CombinationTmp);
        }
    }
}
