using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimInspectorManager : MonoBehaviour
{
    public RectTransform ViewRect, ComponentRect;
    public LimWindowManager BaseWindow;
    public LimOperationManager OperationManager;
    public Image BpmListSwitcherImg, ScrollListSwitcherImg, DefaultSwitcherImg;
    public Color UnpressedColor, PressedColor;
    public Text BpmSwitcherText, ScrollSpeedSwitcherText, DefaultSwitcherText;

    public ComponentBasicManager ComponentBasic;
    public ComponentTypeManager ComponentType;
    public ComponentHoldNoteManager ComponentHoldNote;
    public ComponentMotionManager ComponentMotion;

    public GameObject ComponentBpmView;
    public ComponentBpmManager ComponentBpm;

    public ComponentScrollSpeedManager ComponentScrollSpeed;
    public ComponentDefaultManager ComponentDefault;

    public void SetTexts()
    {
        BaseWindow.WindowName = LimLanguageManager.TextDict["Window_Inspector_Label"];
        BpmSwitcherText.text = LimLanguageManager.TextDict["Window_Inspector_Switcher_Bpm"];
        ScrollSpeedSwitcherText.text = LimLanguageManager.TextDict["Window_Inspector_Switcher_Scroll"];
        DefaultSwitcherText.text = LimLanguageManager.TextDict["Window_Inspector_Switcher_Default"];
    }
    private void Update()
    {
        DetectMouseScroll();
    }
    private void OnEnable()
    {
        ArrangeComponentsUi();
    }
    private void DetectMouseScroll()
    {
        float Scroll = -Input.GetAxis("Mouse ScrollWheel") * 200;
        if (Scroll != 0)
        {
            Vector3 Mouse = LimMousePosition.MousePosition;
            if (Mouse.x >= ViewRect.anchoredPosition.x && Mouse.x <= ViewRect.anchoredPosition.x + ViewRect.sizeDelta.x)
            {
                if (Mouse.y >= ViewRect.anchoredPosition.y - ViewRect.sizeDelta.y && Mouse.y <= ViewRect.anchoredPosition.y)
                {
                    ComponentRect.anchoredPosition = new Vector2(0, Mathf.Clamp(ComponentRect.anchoredPosition.y + Scroll, 0, Mathf.Max(0, ComponentRect.sizeDelta.y - ViewRect.sizeDelta.y)));
                }
            }
        }
    }
    public void OnSelectChange()
    {
        ComponentBasic.OnSelectChange();
        ComponentType.OnSelectChange();
        ComponentHoldNote.OnSelectChange();
        ArrangeComponentsUi();
    }
    public void ArrangeComponentsUi()
    {
        float Height = 0;
        if (ComponentBasic.gameObject.activeInHierarchy)
        {
            Height -= ComponentBasic.ComponentRect.sizeDelta.y;
        }
        if (ComponentType.gameObject.activeInHierarchy)
        {
            ComponentType.ComponentRect.anchoredPosition = new Vector2(0, Height);
            Height -= ComponentType.ComponentRect.sizeDelta.y;
        }
        if (ComponentHoldNote.gameObject.activeInHierarchy)
        {
            ComponentHoldNote.ComponentRect.anchoredPosition = new Vector2(0, Height);
            Height -= ComponentHoldNote.ComponentRect.sizeDelta.y;
        }
        if (ComponentMotion.gameObject.activeInHierarchy)
        {
            ComponentMotion.ComponentRect.anchoredPosition = new Vector2(0, Height);
            Height -= ComponentMotion.ComponentRect.sizeDelta.y;
        }
        if (ComponentBpm.gameObject.activeInHierarchy)
        {
            ComponentBpm.ComponentRect.anchoredPosition = new Vector2(0, Height);
            Height -= ComponentBpm.ComponentRect.sizeDelta.y;
        }
        if (ComponentScrollSpeed.gameObject.activeInHierarchy)
        {
            ComponentScrollSpeed.ComponentRect.anchoredPosition = new Vector2(0, Height);
            Height -= ComponentScrollSpeed.ComponentRect.sizeDelta.y;
        }
        if (ComponentDefault.gameObject.activeInHierarchy)
        {
            ComponentDefault.ComponentRect.anchoredPosition = new Vector2(0, Height);
            Height -= ComponentDefault.ComponentRect.sizeDelta.y;
        }
        ComponentRect.sizeDelta = new Vector2(0, -Height);
    }
    public void SwitchScrollSpeedList()
    {
        if (LimSystem.ChartContainer == null) return;
        if (ComponentScrollSpeed.gameObject.activeInHierarchy)
        {
            ScrollListSwitcherImg.color = UnpressedColor;
            ComponentScrollSpeed.gameObject.SetActive(false);
            ArrangeComponentsUi();
        }
        else
        {
            ScrollListSwitcherImg.color = PressedColor;
            ComponentScrollSpeed.gameObject.SetActive(true);
            ComponentScrollSpeed.InstantiateScrollSpeedList();
            ArrangeComponentsUi();
        }
    }
    public void SwitchBpmList()
    {
        if (LimSystem.ChartContainer == null) return;
        if (ComponentBpmView.activeInHierarchy)
        {
            BpmListSwitcherImg.color = UnpressedColor;
            ComponentBpmView.SetActive(false);
            ArrangeComponentsUi();
        }
        else
        {
            BpmListSwitcherImg.color = PressedColor;
            ComponentBpmView.SetActive(true);
            ComponentBpm.InstantiateBpmList();
            ArrangeComponentsUi();
        }
    }
    public void SwitchDefault()
    {
        if (LimSystem.ChartContainer == null) return;
        if (ComponentDefault.gameObject.activeInHierarchy)
        {
            DefaultSwitcherImg.color = UnpressedColor;
            ComponentDefault.gameObject.SetActive(false);
            ArrangeComponentsUi();
        }
        else
        {
            DefaultSwitcherImg.color = PressedColor;
            ComponentDefault.gameObject.SetActive(true);
            ComponentDefault.LoadDefaultValues();
            ArrangeComponentsUi();
        }
    }
}
