using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimBoxSelectionManager : MonoBehaviour
{
    public Camera TunerCamera;
    public Image BoxSelectionImage;
    public RectTransform BoxSelectionPanel;
    public LimTunerWindowManager TunerWindowManager;
    public LimTunerManager TunerManager;
    public LimOperationManager OperationManager;

    private void Update()
    {
        if (!TunerManager.isInitialized) return;
        if (!DrawBoxSelectionArea()) return;
        SelectNotesInBoxArea();
    }
    private bool DrawBoxSelectionArea()
    {
        if (Input.GetMouseButtonUp(0))
        {
            BoxSelectionPanel.sizeDelta = new Vector2();
        }
        Vector2 MousePositionInWindow = LimMousePosition.MousePositionInWindow(TunerWindowManager.BaseWindow.WindowRectTransform);
        if (Input.GetMouseButtonDown(0) && LimMousePosition.IsMouseOverWindow(TunerWindowManager.BaseWindow.WindowRectTransform))
        {
            BoxSelectionPanel.sizeDelta = new Vector2();
            BoxSelectionPanel.anchoredPosition = new Vector2(MousePositionInWindow.x, MousePositionInWindow.y);
        }
        if (Input.GetMouseButton(0) && LimMousePosition.IsMouseOverWindow(TunerWindowManager.BaseWindow.WindowRectTransform))
        {
            BoxSelectionPanel.sizeDelta = new Vector2(MousePositionInWindow.x - BoxSelectionPanel.anchoredPosition.x, BoxSelectionPanel.anchoredPosition.y - MousePositionInWindow.y);
        }
        if (BoxSelectionPanel.sizeDelta.magnitude > 50)
        {
            if (!BoxSelectionImage.enabled) BoxSelectionImage.enabled = true;
        }
        else
        {
            if (BoxSelectionImage.enabled) BoxSelectionImage.enabled = false;
        }
        return BoxSelectionImage.enabled;
    }
    public Vector2 TunerScreenToWindowPosition(Vector3 ScreenPosition, RectTransform Window)
    {
        return new Vector2(ScreenPosition.x, ScreenPosition.y - Window.sizeDelta.y);
    }
    private bool IsNoteInBoxArea(GameObject Note)
    {
        Vector3 ScreenPosition = TunerCamera.WorldToScreenPoint(Note.transform.position);
        Vector2 WindowPosition = TunerScreenToWindowPosition(ScreenPosition, TunerWindowManager.BaseWindow.WindowRectTransform);
        return LimMathUtil.InRect(WindowPosition, BoxSelectionPanel);
    }
    private void SelectNotesInBoxArea()
    {
        if (!Input.GetMouseButton(0)) return;
        if (!LimMousePosition.IsMouseOverWindow(TunerWindowManager.BaseWindow.WindowRectTransform)) return;
        foreach (Lanotalium.Chart.LanotaTapNote Tap in TunerManager.TapNoteManager.TapNote)
        {
            if (Tap.TapNoteGameObject.activeInHierarchy)
            {
                if (IsNoteInBoxArea(Tap.TapNoteGameObject))
                {
                    if (!Tap.OnSelect)
                    {
                        OperationManager.SelectTapNote(Tap, true);
                    }
                }
                else if (!Input.GetKey(KeyCode.LeftControl))
                {
                    if (Tap.OnSelect)
                    {
                        OperationManager.DeSelectTapNote(Tap);
                    }
                }
            }
        }
        foreach (Lanotalium.Chart.LanotaHoldNote Hold in TunerManager.HoldNoteManager.HoldNote)
        {
            if (Hold.HoldNoteGameObject.activeInHierarchy)
            {
                if (IsNoteInBoxArea(Hold.HoldNoteGameObject))
                {
                    if (!Hold.OnSelect)
                    {
                        OperationManager.SelectHoldNote(Hold, true);
                    }
                }
                else if (!Input.GetKey(KeyCode.LeftControl))
                {
                    if (Hold.OnSelect)
                    {
                        OperationManager.DeSelectHoldNote(Hold);
                    }
                }
            }
        }
    }
}
