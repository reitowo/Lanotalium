using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentTypeManager : MonoBehaviour
{

    public RectTransform ViewRect, ComponentRect;
    public int UnFoldHeight;
    public Text LabelText, TypeText;
    public Dropdown Type;
    public LimInspectorManager InspectorManager;
    public LimOperationManager OperationManager;

    private bool isFolded = false, EnableValueChange = false;
    private Lanotalium.Editor.ComponentTypeMode Mode = Lanotalium.Editor.ComponentTypeMode.Idle;
    private float UiWidth;

    void Start()
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
        Type.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * Ratio, 30);
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
        LabelText.text = LimLanguageManager.TextDict["Component_Type_Label"];
        TypeText.text = LimLanguageManager.TextDict["Component_Type_Type"];
        List<Dropdown.OptionData> Options = new List<Dropdown.OptionData>();
        Options.Add(new Dropdown.OptionData(LimLanguageManager.TextDict["Component_Type_DropDown_Type0"]));
        Options.Add(new Dropdown.OptionData(LimLanguageManager.TextDict["Component_Type_DropDown_Type2"]));
        Options.Add(new Dropdown.OptionData(LimLanguageManager.TextDict["Component_Type_DropDown_Type3"]));
        Options.Add(new Dropdown.OptionData(LimLanguageManager.TextDict["Component_Type_DropDown_Type4"]));
        Options.Add(new Dropdown.OptionData(LimLanguageManager.TextDict["Component_Type_DropDown_Type5"]));
        Type.options = Options;
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

    private int ConvertValueToType(int Value)
    {
        return Value == 0 ? 0 : Value + 1;
    }
    private int ConvertTypeToValue(int Type)
    {
        return Type == 0 ? 0 : Type - 1;
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
        EnsureDropdownUseable(Type);
        if (OperationManager.SelectedTapNote.Count == 1 && OperationManager.SelectedHoldNote.Count == 0)
        {
            Mode = Lanotalium.Editor.ComponentTypeMode.Work;
            Type.value = ConvertTypeToValue(OperationManager.SelectedTapNote[0].Type);
            gameObject.SetActive(true);
        }
        else if (OperationManager.SelectedTapNote.Count == 0 && OperationManager.SelectedHoldNote.Count == 1)
        {
            Mode = Lanotalium.Editor.ComponentTypeMode.Work;
            Type.value = ConvertTypeToValue(OperationManager.SelectedHoldNote[0].Type);
            gameObject.SetActive(true);
        }
        else if (OperationManager.SelectedTapNote.Count + OperationManager.SelectedHoldNote.Count > 1)
        {
            Mode = Lanotalium.Editor.ComponentTypeMode.Work;
            Type.value = 0;
            gameObject.SetActive(true);
        }
        else if (OperationManager.SelectedTapNote.Count == 0 && OperationManager.SelectedHoldNote.Count == 0)
        {
            Mode = Lanotalium.Editor.ComponentTypeMode.Idle;
            gameObject.SetActive(false);
        }
        EnableValueChange = true;
    }

    public void OnTypeChange()
    {
        if (!EnableValueChange) return;
        int TypeTmp = ConvertValueToType(Type.value);
        List<Lanotalium.Chart.LanotaTapNote> ToConvertTapNote = new List<Lanotalium.Chart.LanotaTapNote>();
        List<Lanotalium.Chart.LanotaHoldNote> ToConvertHoldNote = new List<Lanotalium.Chart.LanotaHoldNote>();
        if (Mode == Lanotalium.Editor.ComponentTypeMode.Work)
        {
            foreach (Lanotalium.Chart.LanotaTapNote Tap in OperationManager.SelectedTapNote)
            {
                if (TypeTmp == 5)
                {
                    ToConvertTapNote.Add(Tap);
                }
                else
                {
                    OperationManager.SetTapNoteType(Tap, TypeTmp);
                }
            }
            foreach (Lanotalium.Chart.LanotaHoldNote Hold in OperationManager.SelectedHoldNote)
            {
                if (TypeTmp != 5)
                {
                    ToConvertHoldNote.Add(Hold);
                }
            }
            foreach (Lanotalium.Chart.LanotaTapNote Tap in ToConvertTapNote)
            {
                OperationManager.ConvertTapNoteToHoldNote(Tap);
            }
            foreach (Lanotalium.Chart.LanotaHoldNote Hold in ToConvertHoldNote)
            {
                OperationManager.ConvertHoldNoteToTapNote(Hold, TypeTmp);
            }
            OperationManager.SelectNothing();
        }
    }
}
