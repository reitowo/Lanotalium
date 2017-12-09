using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentScrollSpeedManager : MonoBehaviour
{
    public LimInspectorManager InspectorManager;
    public LimOperationManager OperationManager;
    public RectTransform ViewRect, ComponentRect;
    public Color InvalidColor, ValidColor;
    public GameObject TimeValuePrefab;
    public Transform PairsTransform;
    public Text LabelText, TimingText, SpeedText, ChartSpeedText, CurrentChartSpeedText;
    public Slider ChartSpeedSlider;
    public float UnFoldHeight;

    private bool isFolded = false;
    private float UiWidth;

    private void Start()
    {
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        RefreshUiWidth();
    }
    private void Update()
    {
        OnUiWidthChange();
        CurrentChartSpeedText.text = (OperationManager.TunerManager.ChartPlaySpeed * 2).ToString("f1");
    }
    public void RefreshUiWidth()
    {
        UiWidth = ViewRect.rect.width;
        float Ratio = UiWidth / 500f;
        TimingText.GetComponent<RectTransform>().anchoredPosition = new Vector2(10 * Ratio, 0);
        TimingText.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * Ratio, 30);
        SpeedText.GetComponent<RectTransform>().anchoredPosition = new Vector2(200 * Ratio, 0);
        SpeedText.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * Ratio, 30);
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
        LabelText.text = LimLanguageManager.TextDict["Component_ScrollSpeed_Label"];
        TimingText.text = LimLanguageManager.TextDict["Component_ScrollSpeed_Timing"];
        SpeedText.text = LimLanguageManager.TextDict["Component_ScrollSpeed_Speed"];
        ChartSpeedText.text = LimLanguageManager.TextDict["Component_ScrollSpeed_ChartSpeed"];
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
    public void InstantiateScrollSpeedList()
    {
        float Height = 0;
        foreach (Lanotalium.Chart.LanotaScroll Scroll in OperationManager.TunerManager.ScrollManager.Scroll)
        {
            if (Scroll.ListGameObject != null) Destroy(Scroll.ListGameObject);
            Scroll.ListGameObject = Instantiate(TimeValuePrefab, PairsTransform);
            Scroll.ListGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Height);
            Scroll.ListGameObject.GetComponent<TimeValuePairManager>().OperationManager = OperationManager;
            Scroll.ListGameObject.GetComponent<TimeValuePairManager>().Initialize(Scroll);
            Scroll.InstanceId = Scroll.ListGameObject.GetInstanceID();
            Height -= 30;
        }
        ViewRect.sizeDelta = new Vector2(0, 65 - Height);
        UnFoldHeight = ViewRect.sizeDelta.y;
        ComponentRect.sizeDelta = new Vector2(0, ViewRect.sizeDelta.y - ViewRect.anchoredPosition.y);
        isFolded = false;
        InspectorManager.ArrangeComponentsUi();
    }
    public void OnChartSpeedChange()
    {
        OperationManager.TunerManager.ChartPlaySpeed = ChartSpeedSlider.value / 2;
    }
}
