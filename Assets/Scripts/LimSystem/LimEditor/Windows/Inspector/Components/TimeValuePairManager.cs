using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeValuePairManager : MonoBehaviour
{
    public RectTransform ViewRect;
    public Lanotalium.Chart.LanotaScroll ScrollData;
    public Lanotalium.Chart.LanotaChangeBpm BpmData;
    public InputField Time, Value;
    public Image TimeImg, ValueImg;
    public Button Delete;
    public Color InvalidColor, ValidColor;
    public LimOperationManager OperationManager;
    private Lanotalium.Editor.TimeValuePairMode Mode = Lanotalium.Editor.TimeValuePairMode.Idle;
    private bool EnableValueChange = true;

    private float UiWidth;

    private void Update()
    {
        OnUiWidthChange();
    }
    public void RefreshUiWidth()
    {
        UiWidth = ViewRect.rect.width;
        float Ratio = UiWidth / 500f;
        Time.GetComponent<RectTransform>().anchoredPosition = new Vector2(5 * Ratio, 0);
        Time.GetComponent<RectTransform>().sizeDelta = new Vector2(190 * Ratio, 30);
        Value.GetComponent<RectTransform>().anchoredPosition = new Vector2(200 * Ratio, 0);
        Value.GetComponent<RectTransform>().sizeDelta = new Vector2(190 * Ratio, 30);
        Delete.GetComponent<RectTransform>().anchoredPosition = new Vector2(395 * Ratio, 0);
        Delete.GetComponent<RectTransform>().sizeDelta = new Vector2(100 * Ratio, 30);
    }
    public void OnUiWidthChange()
    {
        if (UiWidth != ViewRect.rect.width)
        {
            RefreshUiWidth();
        }
    }

    public void Initialize(Lanotalium.Chart.LanotaScroll ScrollData)
    {
        EnableValueChange = false;
        Mode = Lanotalium.Editor.TimeValuePairMode.ScrollSpeed;
        this.ScrollData = ScrollData;
        Time.text = ScrollData.Time.ToString("f5");
        Value.text = ScrollData.Speed.ToString("f5");
        if (ScrollData.Time == -10f)
        {
            Time.interactable = false;
            Delete.interactable = false;
        }
        EnableValueChange = true;
    }
    public void Initialize(Lanotalium.Chart.LanotaChangeBpm BpmData)
    {
        EnableValueChange = false;
        Mode = Lanotalium.Editor.TimeValuePairMode.Bpm;
        this.BpmData = BpmData;
        Time.text = BpmData.Time.ToString("f5");
        Value.text = BpmData.Bpm.ToString("f5");
        if (BpmData.Time == -3f)
        {
            Time.interactable = false;
            Delete.interactable = false;
        }
        EnableValueChange = true;
    }
    public void OnTimingChange()
    {
        if (!EnableValueChange) return;
        float TimingTmp;
        if (!float.TryParse(Time.text, out TimingTmp))
        {
            TimeImg.color = InvalidColor;
            return;
        }
        if (Mode == Lanotalium.Editor.TimeValuePairMode.ScrollSpeed)
        {
            if (!OperationManager.CheckScrollTimeValid(ScrollData, TimingTmp))
            {
                TimeImg.color = InvalidColor;
                return;
            }
            OperationManager.SetScrollTime(ScrollData, TimingTmp);
        }
        else if (Mode == Lanotalium.Editor.TimeValuePairMode.Bpm)
        {
            if (!OperationManager.CheckBpmTimeValid(BpmData, TimingTmp))
            {
                TimeImg.color = InvalidColor;
                return;
            }
            OperationManager.SetBpmTime(BpmData, TimingTmp);
        }
        TimeImg.color = ValidColor;
    }
    public void OnValueChange()
    {
        if (!EnableValueChange) return;
        float ValueTmp;
        if (!float.TryParse(Value.text, out ValueTmp))
        {
            ValueImg.color = InvalidColor;
            return;
        }
        if (Mode == Lanotalium.Editor.TimeValuePairMode.ScrollSpeed)
        {
            OperationManager.SetScrollSpeed(ScrollData, ValueTmp);
        }
        else if (Mode == Lanotalium.Editor.TimeValuePairMode.Bpm)
        {
            if (ValueTmp <= 0 || ValueTmp > 1000)
            {
                ValueImg.color = InvalidColor;
                return;
            }
            OperationManager.SetBpmBpm(BpmData, ValueTmp);
        }
        ValueImg.color = ValidColor;
    }
    public void DeleteThis()
    {
        if (Mode == Lanotalium.Editor.TimeValuePairMode.ScrollSpeed)
        {
            OperationManager.DeleteScroll(ScrollData);
        }
        else if (Mode == Lanotalium.Editor.TimeValuePairMode.Bpm)
        {
            OperationManager.DeleteBpm(BpmData);
        }
    }
}
