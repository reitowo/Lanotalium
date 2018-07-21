using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LimMouseOverHint : MonoBehaviour
{
    public string HintTextDictKey;
    public Font Font;
    private EventTrigger Trigger;
    private bool isMouseOver = false;
    private bool isGUIInitialized = false;
    private GUIStyle Style;
    private Vector2 Size;
    void Start()
    {
        Trigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry PointerEnter = new EventTrigger.Entry();
        UnityAction<BaseEventData> PointerEnterAction = new UnityAction<BaseEventData>(OnPointerEnter);
        PointerEnter.callback.AddListener(PointerEnterAction);
        PointerEnter.eventID = EventTriggerType.PointerEnter;
        Trigger.triggers.Add(PointerEnter);

        EventTrigger.Entry PointerExit = new EventTrigger.Entry();
        UnityAction<BaseEventData> PointerExitAction = new UnityAction<BaseEventData>(OnPointerExit);
        PointerExit.callback.AddListener(PointerExitAction);
        PointerExit.eventID = EventTriggerType.PointerExit;
        Trigger.triggers.Add(PointerExit);

    }

    private void OnGUI()
    {
        if (!isMouseOver) return;
        if (!isGUIInitialized)
        {
            Style = new GUIStyle(GUI.skin.box);
            Style.font = Font;
            Style.fontSize = 18;
            Style.normal.textColor = new Color(255, 255, 255, 255);
            Style.stretchHeight = true;
            Style.stretchWidth = true;
            Style.alignment = TextAnchor.LowerLeft;
            isGUIInitialized = true;
        }
        Size = Style.CalcSize(new GUIContent(LimLanguageManager.HintDict[HintTextDictKey]));
        Vector2 MousePosition = Input.mousePosition;
        MousePosition.y = Screen.height - MousePosition.y - Size.y;
        GUI.Box(new Rect(MousePosition, Size), LimLanguageManager.HintDict[HintTextDictKey], Style);
    }

    public void OnPointerEnter(BaseEventData data)
    {
        isMouseOver = true;
    }
    public void OnPointerExit(BaseEventData data)
    {
        isMouseOver = false;
    }
    private void OnDisable()
    {
        isMouseOver = false;
    }
}
