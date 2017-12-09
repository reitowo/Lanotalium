using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentJointManager : MonoBehaviour
{
    public RectTransform ViewRect;
    public InputField JointDegree, JointTime, JointEase;
    public Button AddJointBtn, RemoveJointBtn;
    public Image JointDegreeImg, JointTimeImg, JointEaseImg;
    public Color Invalid, Valid, Error;

    private LimOperationManager OperationManager;
    private ComponentHoldNoteManager ComponentHoldNote;
    private Lanotalium.Chart.LanotaHoldNote HoldNoteData;
    private Lanotalium.Chart.LanotaJoints JointNoteData;
    private float UiWidth;

    private void Update()
    {
        OnUiWidthChange();
    }
    public void Initialize(LimOperationManager OperationManager, Lanotalium.Chart.LanotaHoldNote HoldNoteData, Lanotalium.Chart.LanotaJoints JointNoteData, ComponentHoldNoteManager ComponentHoldNote)
    {
        this.OperationManager = OperationManager;
        this.HoldNoteData = HoldNoteData;
        this.JointNoteData = JointNoteData;
        this.ComponentHoldNote = ComponentHoldNote;
        RefreshUiWidth();
    }
    public void RefreshUiWidth()
    {
        UiWidth = ViewRect.rect.width;
        float Ratio = UiWidth / 500f;
        JointDegree.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * Ratio, 30);
        JointDegree.GetComponent<RectTransform>().anchoredPosition = new Vector2(10 * Ratio, 0);
        JointTime.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * Ratio, 30);
        JointTime.GetComponent<RectTransform>().anchoredPosition = new Vector2(180 * Ratio, 0);
        JointEase.GetComponent<RectTransform>().sizeDelta = new Vector2(60 * Ratio, 30);
        JointEase.GetComponent<RectTransform>().anchoredPosition = new Vector2(350 * Ratio, 0);
        AddJointBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(30 * Ratio, 30);
        AddJointBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(420 * Ratio, 0);
        RemoveJointBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(30 * Ratio, 30);
        RemoveJointBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(460 * Ratio, 0);
    }
    public void OnUiWidthChange()
    {
        if (UiWidth != ViewRect.rect.width)
        {
            RefreshUiWidth();
        }
    }
    public void RefreshJointValue()
    {
        if (!ComponentHoldNote.isAbsolute)
        {
            JointDegree.text = JointNoteData.dDegree.ToString("f5");
            JointTime.text = JointNoteData.dTime.ToString("f5");
        }
        else if (ComponentHoldNote.isAbsolute)
        {
            JointDegree.text = JointNoteData.aDegree.ToString("f5");
            JointTime.text = JointNoteData.aTime.ToString("f5");
        }
        JointEase.text = JointNoteData.Cfmi.ToString();
        if (!OperationManager.CheckJointNoteTimeIsValidByJoint(HoldNoteData, JointNoteData)) JointTimeImg.color = Error;
        else JointTimeImg.color = Valid;
        JointDegreeImg.color = Valid;
        JointEaseImg.color = Valid;
    }
    public void AddJoint()
    {
        OperationManager.AddJointNoteFromExisted(HoldNoteData, JointNoteData);
        OperationManager.EnsureHoldNoteJointsValid(HoldNoteData);
        ComponentHoldNote.RefreshJointList();
    }
    public void RemoveJoint()
    {
        OperationManager.DeleteJointNote(HoldNoteData, JointNoteData);
        OperationManager.EnsureHoldNoteJointsValid(HoldNoteData);
        ComponentHoldNote.RefreshJointList();
    }
    public void OnDegreeChange()
    {
        if (!ComponentHoldNote.EnableValueChange) return;
        float DegreeTmp;
        if (!float.TryParse(JointDegree.text, out DegreeTmp))
        {
            JointDegreeImg.color = Invalid;
            return;
        }
        OperationManager.SetJointNoteDegree(HoldNoteData, JointNoteData, DegreeTmp, ComponentHoldNote.isAbsolute, ComponentHoldNote.isChained);
        JointDegreeImg.color = Valid;
        ComponentHoldNote.RefreshJointValues(this);
    }
    public void OnTimeChange()
    {
        if (!ComponentHoldNote.EnableValueChange) return;
        float TimeTmp;
        if (!float.TryParse(JointTime.text, out TimeTmp))
        {
            JointTimeImg.color = Invalid;
            return;
        }
        OperationManager.SetJointNoteTime(HoldNoteData, JointNoteData, TimeTmp, ComponentHoldNote.isAbsolute, ComponentHoldNote.isChained);
        OperationManager.EnsureHoldNoteJointsValid(HoldNoteData);
        JointTimeImg.color = Valid;
        if (!OperationManager.CheckJointNoteTimeIsValidByJoint(HoldNoteData, JointNoteData)) JointTimeImg.color = Error;
        ComponentHoldNote.RefreshJointValues(this);
    }
    public void OnEaseChange()
    {
        if (!ComponentHoldNote.EnableValueChange) return;
        int EaseTmp;
        if (!int.TryParse(JointEase.text, out EaseTmp))
        {
            JointEaseImg.color = Invalid;
            return;
        }
        if (EaseTmp < 0 || EaseTmp > 12)
        {
            JointEaseImg.color = Invalid;
            return;
        }
        OperationManager.SetJointNoteEase(JointNoteData, EaseTmp);
        JointEaseImg.color = Valid;
        ComponentHoldNote.RefreshJointValues();
    }
}
