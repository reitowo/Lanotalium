using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Windows.Forms;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class LimOperationManager : MonoBehaviour
{
    public Camera TunerCamera;
    public RectTransform TunerWindowRect;
    public LimTunerManager TunerManager;
    public LimInspectorManager InspectorManager;
    public LimTimeLineManager TimeLineManager;

    public List<Lanotalium.Chart.LanotaTapNote> SelectedTapNote = new List<Lanotalium.Chart.LanotaTapNote>();
    public List<Lanotalium.Chart.LanotaHoldNote> SelectedHoldNote = new List<Lanotalium.Chart.LanotaHoldNote>();
    public List<Lanotalium.Chart.LanotaCameraBase> SelectedMotions = new List<Lanotalium.Chart.LanotaCameraBase>();
    private List<Lanotalium.Editor.OperationSave> OperationSaver = new List<Lanotalium.Editor.OperationSave>();
    private int CurrentOperationSaverPosition = -1;

    public void AddToOperationSaver(Lanotalium.Editor.OperationSave OpSave)
    {
        if (CurrentOperationSaverPosition != OperationSaver.Count - 1)
        {
            for (int i = OperationSaver.Count - 1; i > CurrentOperationSaverPosition; --i)
            {
                OperationSaver.RemoveAt(i);
            }
        }
        OperationSaver.Add(OpSave);
        CurrentOperationSaverPosition = OperationSaver.Count - 1;
    }
    public void Undo()
    {
        if (OperationSaver.Count == 0) return;
        if (CurrentOperationSaverPosition == -1) return;
        OperationSaver[CurrentOperationSaverPosition].Reverse();
        CurrentOperationSaverPosition = Mathf.Clamp(CurrentOperationSaverPosition - 1, -1, OperationSaver.Count - 1);
    }
    public void Redo()
    {
        if (OperationSaver.Count == 0) return;
        if (CurrentOperationSaverPosition == OperationSaver.Count - 1) return;
        CurrentOperationSaverPosition = Mathf.Clamp(CurrentOperationSaverPosition + 1, -1, OperationSaver.Count - 1);
        OperationSaver[CurrentOperationSaverPosition].Forward();
    }

    private void Update()
    {
        if (LimSystem.ChartContainer == null) return;
        DetectNoteSelection();
        DetectDeleteRequest();
        DetectUndoRedo();
    }

    public void DetectNoteSelection()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 MousePosition = LimMousePosition.MousePosition;
            Vector3 TunerPosition = new Vector3();
            TunerPosition.x = MousePosition.x - TunerWindowRect.anchoredPosition.x;
            TunerPosition.y = TunerWindowRect.sizeDelta.y + (MousePosition.y - TunerWindowRect.anchoredPosition.y);
            Ray ray = TunerCamera.ScreenPointToRay(TunerPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                int InstanceId = hit.collider.gameObject.GetInstanceID();
                int TryFindTapNote = FindTapNoteIndexByInstanceID(InstanceId);
                if (TryFindTapNote == -1)
                {
                    int TryFindHoldNote = FindHoldNoteIndexByInstanceID(InstanceId);
                    if (TryFindHoldNote == -1) return;
                    else
                    {
                        SelectHoldNote(TunerManager.HoldNoteManager.HoldNote[TryFindHoldNote]);
                    }
                }
                else
                {
                    SelectTapNote(TunerManager.TapNoteManager.TapNote[TryFindTapNote]);
                }
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            SelectNothing();
            DeSelectAllMotions();
        }
    }
    public void DetectDeleteRequest()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DeleteAllSelected();
        }
    }
    public void DetectUndoRedo()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Z)) Undo();
            else if (Input.GetKeyDown(KeyCode.Y)) Redo();
        }
    }
    public void SelectNothing()
    {
        foreach (Lanotalium.Chart.LanotaTapNote Tap in SelectedTapNote) Tap.OnSelect = false;
        foreach (Lanotalium.Chart.LanotaHoldNote Hold in SelectedHoldNote) Hold.OnSelect = false;
        SelectedTapNote.Clear();
        SelectedHoldNote.Clear();
        InspectorManager.OnSelectChange();
        InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Idle);
        InspectorManager.ArrangeComponentsUi();
    }
    public void DeleteAllSelected()
    {
        foreach (Lanotalium.Chart.LanotaTapNote Tap in SelectedTapNote) DeleteTapNote(Tap);
        foreach (Lanotalium.Chart.LanotaHoldNote Hold in SelectedHoldNote) DeleteHoldNote(Hold);
        SelectedTapNote.Clear();
        SelectedHoldNote.Clear();
        InspectorManager.OnSelectChange();
        DeleteSelectedMotions();
        InspectorManager.ArrangeComponentsUi();
    }
    public void DeleteSelectedMotions()
    {
        InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Idle);
        foreach (Lanotalium.Chart.LanotaCameraBase Base in SelectedMotions)
        {
            Destroy(Base.TimeLineGameObject);
            switch (Base.Type)
            {
                case 8:
                case 11:
                    TunerManager.CameraManager.Horizontal.Remove(Base as Lanotalium.Chart.LanotaCameraXZ); break;
                case 10:
                    TunerManager.CameraManager.Vertical.Remove(Base as Lanotalium.Chart.LanotaCameraY); break;
                case 13:
                    TunerManager.CameraManager.Rotation.Remove(Base as Lanotalium.Chart.LanotaCameraRot); break;
            }
        }
        SelectedMotions.Clear();
    }
    public void SelectTapNote(Lanotalium.Chart.LanotaTapNote TapNoteData, bool MultiSelect = false)
    {
        if (IsTapNoteSelected(TapNoteData))
        {
            if (!Input.GetKey(KeyCode.LeftControl)) SelectNothing();
            else DeSelectTapNote(TapNoteData);
            InspectorManager.OnSelectChange();
            return;
        }
        if (!Input.GetKey(KeyCode.LeftControl) && !MultiSelect) SelectNothing();
        TapNoteData.OnSelect = true;
        SelectedTapNote.Add(TapNoteData);
        InspectorManager.OnSelectChange();
    }
    public void SelectHoldNote(Lanotalium.Chart.LanotaHoldNote HoldNoteData, bool MultiSelect = false)
    {
        if (IsHoldNoteSelected(HoldNoteData))
        {
            if (!Input.GetKey(KeyCode.LeftControl)) SelectNothing();
            else DeSelectHoldNote(HoldNoteData);
            InspectorManager.OnSelectChange();
            return;
        }
        if (!Input.GetKey(KeyCode.LeftControl) && !MultiSelect) SelectNothing();
        HoldNoteData.OnSelect = true;
        SelectedHoldNote.Add(HoldNoteData);
        InspectorManager.OnSelectChange();
    }
    public void DeSelectTapNote(Lanotalium.Chart.LanotaTapNote TapNoteData)
    {
        int Index = 0;
        foreach (Lanotalium.Chart.LanotaTapNote Note in SelectedTapNote)
        {
            if (Note.InstanceId == TapNoteData.InstanceId) break;
            Index++;
        }
        TapNoteData.OnSelect = false;
        SelectedTapNote.RemoveAt(Index);
        InspectorManager.OnSelectChange();
    }
    public void DeSelectHoldNote(Lanotalium.Chart.LanotaHoldNote HoldNoteData)
    {
        int Index = 0;
        foreach (Lanotalium.Chart.LanotaHoldNote Note in SelectedHoldNote)
        {
            if (Note.InstanceId == HoldNoteData.InstanceId) break;
            Index++;
        }
        HoldNoteData.OnSelect = false;
        SelectedHoldNote.RemoveAt(Index);
        InspectorManager.OnSelectChange();
    }
    public bool IsTapNoteSelected(Lanotalium.Chart.LanotaTapNote TapNoteData)
    {
        foreach (Lanotalium.Chart.LanotaTapNote Note in SelectedTapNote)
        {
            if (Note.InstanceId == TapNoteData.InstanceId) return true;
        }
        return false;
    }
    public bool IsHoldNoteSelected(Lanotalium.Chart.LanotaHoldNote HoldNoteData)
    {
        foreach (Lanotalium.Chart.LanotaHoldNote Note in SelectedHoldNote)
        {
            if (Note.InstanceId == HoldNoteData.InstanceId) return true;
        }
        return false;
    }

    public int FindTapNoteIndexByInstanceID(int InstanceId)
    {
        int Index = 0;
        if (TunerManager.TapNoteManager.TapNote == null) return -1;
        foreach (Lanotalium.Chart.LanotaTapNote Note in TunerManager.TapNoteManager.TapNote)
        {
            if (Note.InstanceId == InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public int FindTapNoteIndexByReference(Lanotalium.Chart.LanotaTapNote TapNoteData)
    {
        int Index = 0;
        if (TunerManager.TapNoteManager.TapNote == null) return -1;
        foreach (Lanotalium.Chart.LanotaTapNote Note in TunerManager.TapNoteManager.TapNote)
        {
            if (Note.InstanceId == TapNoteData.InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public void AddTapNote(Lanotalium.Chart.LanotaTapNote TapNoteData, bool SaveOperation = true, bool SelectNote = false, bool CallSelectNothing = true)
    {
        TunerManager.TapNoteManager.InstantiateNote(TapNoteData);
        TunerManager.TapNoteManager.TapNote.Add(TapNoteData);
        TunerManager.TapNoteManager.SortTapNoteList();
        if (CallSelectNothing) SelectNothing();
        if (SelectNote) SelectTapNote(TapNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { AddTapNote(TapNoteData, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { DeleteTapNote(TapNoteData, false); });
        AddToOperationSaver(OpSave);
    }
    public void SetTapNoteCombination(Lanotalium.Chart.LanotaTapNote TapNoteData, bool Combination, bool SaveOperation = true)
    {
        bool OriginCombination = TapNoteData.Combination;
        TapNoteData.Combination = Combination;
        Destroy(TapNoteData.TapNoteGameObject);
        TunerManager.TapNoteManager.InstantiateNote(TapNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetTapNoteCombination(TapNoteData, Combination, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetTapNoteCombination(TapNoteData, OriginCombination, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetTapNoteSize(Lanotalium.Chart.LanotaTapNote TapNoteData, int Size, bool SaveOperation = true)
    {
        int OriginSize = TapNoteData.Size;
        TapNoteData.Size = Size;
        Destroy(TapNoteData.TapNoteGameObject);
        TunerManager.TapNoteManager.InstantiateNote(TapNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetTapNoteSize(TapNoteData, Size, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetTapNoteSize(TapNoteData, OriginSize, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetTapNoteType(Lanotalium.Chart.LanotaTapNote TapNoteData, int Type, bool SaveOperation = true)
    {
        int OriginType = TapNoteData.Type;
        TapNoteData.Type = Type;
        Destroy(TapNoteData.TapNoteGameObject);
        TunerManager.TapNoteManager.InstantiateNote(TapNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetTapNoteType(TapNoteData, Type, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetTapNoteType(TapNoteData, OriginType, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetTapNoteDegree(Lanotalium.Chart.LanotaTapNote TapNoteData, float Degree, bool isAbsolute, bool SaveOperation = true)
    {
        float OriginDegree = TapNoteData.Degree;
        if (isAbsolute) TapNoteData.Degree = Degree - TunerManager.CameraManager.CalculateCameraRotation(TapNoteData.Time);
        else TapNoteData.Degree = Degree;
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetTapNoteDegree(TapNoteData, Degree, isAbsolute, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetTapNoteDegree(TapNoteData, OriginDegree, false, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetTapNoteTime(Lanotalium.Chart.LanotaTapNote TapNoteData, float Time, bool SaveOperation = true)
    {
        float OriginTime = TapNoteData.Time;
        TapNoteData.Time = Time;
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetTapNoteTime(TapNoteData, Time, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetTapNoteTime(TapNoteData, OriginTime, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void DeleteTapNote(Lanotalium.Chart.LanotaTapNote TapNoteData, bool SaveOperation = true)
    {
        Destroy(TapNoteData.TapNoteGameObject);
        TunerManager.TapNoteManager.TapNote.Remove(TapNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { DeleteTapNote(TapNoteData, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { AddTapNote(TapNoteData, false); });
        AddToOperationSaver(OpSave);
    }
    public void ConvertTapNoteToHoldNote(Lanotalium.Chart.LanotaTapNote TapNoteData, bool SaveOperation = true)
    {
        DeleteTapNote(TapNoteData, false);
        Lanotalium.Chart.LanotaHoldNote NewHoldNote = TapNoteData.ToHoldNote();
        AddHoldNote(NewHoldNote, false);
    }

    public int FindHoldNoteIndexByInstanceID(int InstanceId)
    {
        int Index = 0;
        if (TunerManager.HoldNoteManager.HoldNote == null) return -1;
        foreach (Lanotalium.Chart.LanotaHoldNote Note in TunerManager.HoldNoteManager.HoldNote)
        {
            if (Note.InstanceId == InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public int FindJointNoteIndexByReference(Lanotalium.Chart.LanotaHoldNote HoldNoteData, Lanotalium.Chart.LanotaJoints JointNoteData)
    {
        int Index = 0;
        if (HoldNoteData.Joints == null) return -2;
        foreach (Lanotalium.Chart.LanotaJoints Joint in HoldNoteData.Joints)
        {
            if (Joint.InstanceId == JointNoteData.InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public void AddHoldNote(Lanotalium.Chart.LanotaHoldNote HoldNoteData, bool SaveOperation = true, bool SelectNote = false, bool CallSelectNothing = true)
    {
        TunerManager.HoldNoteManager.InstantiateHeadNote(HoldNoteData);
        TunerManager.HoldNoteManager.InstantiateAllJointNote(HoldNoteData);
        TunerManager.HoldNoteManager.HoldNote.Add(HoldNoteData);
        TunerManager.HoldNoteManager.SortHoldNoteList();
        RefreshJointAbsoluteValues(HoldNoteData);
        if (CallSelectNothing) SelectNothing();
        if (SelectNote) SelectHoldNote(HoldNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { AddHoldNote(HoldNoteData, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { DeleteHoldNote(HoldNoteData, false); });
        AddToOperationSaver(OpSave);
    }
    public void AddJointNoteFromVoid(Lanotalium.Chart.LanotaHoldNote HoldNoteData, bool SaveOperation = true)
    {
        Lanotalium.Chart.LanotaJoints NewJointNoteData = new Lanotalium.Chart.LanotaJoints();
        if (HoldNoteData.Joints == null) HoldNoteData.Joints = new List<Lanotalium.Chart.LanotaJoints>();
        NewJointNoteData.dTime = HoldNoteData.Duration;
        TunerManager.HoldNoteManager.InstantiateJointNote(NewJointNoteData);
        HoldNoteData.Joints.Insert(0, NewJointNoteData);
        HoldNoteData.Jcount = HoldNoteData.Joints.Count;
        RefreshJointAbsoluteValues(HoldNoteData);
    }
    public void AddJointNoteFromExisted(Lanotalium.Chart.LanotaHoldNote HoldNoteData, Lanotalium.Chart.LanotaJoints JointNoteData, bool SaveOperation = true)
    {
        int InsertIndex = FindJointNoteIndexByReference(HoldNoteData, JointNoteData);
        Lanotalium.Chart.LanotaJoints NewJointNoteData = new Lanotalium.Chart.LanotaJoints();
        if (HoldNoteData.Joints == null) HoldNoteData.Joints = new List<Lanotalium.Chart.LanotaJoints>();
        if (HoldNoteData.Joints.Count < InsertIndex) throw new Lanotalium.Exceptions.JointNoteOutOfRangeException();
        NewJointNoteData.dTime = JointNoteData.dTime / 2;
        JointNoteData.dTime /= 2;
        NewJointNoteData.dDegree = JointNoteData.dDegree / 2;
        JointNoteData.dDegree /= 2;
        TunerManager.HoldNoteManager.InstantiateJointNote(NewJointNoteData);
        HoldNoteData.Joints.Insert(InsertIndex, NewJointNoteData);
        HoldNoteData.Jcount = HoldNoteData.Joints.Count;
        RefreshJointAbsoluteValues(HoldNoteData);
    }
    public void SetHoldNoteCombination(Lanotalium.Chart.LanotaHoldNote HoldNoteData, bool Combination, bool SaveOperation = true)
    {
        bool OriginCombination = HoldNoteData.Combination;
        HoldNoteData.Combination = Combination;
        Destroy(HoldNoteData.HoldNoteGameObject);
        TunerManager.HoldNoteManager.InstantiateHeadNote(HoldNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetHoldNoteCombination(HoldNoteData, Combination, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetHoldNoteCombination(HoldNoteData, OriginCombination, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetHoldNoteSize(Lanotalium.Chart.LanotaHoldNote HoldNoteData, int Size, bool SaveOperation = true)
    {
        int OriginSize = HoldNoteData.Size;
        HoldNoteData.Size = Size;
        Destroy(HoldNoteData.HoldNoteGameObject);
        TunerManager.HoldNoteManager.InstantiateHeadNote(HoldNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetHoldNoteSize(HoldNoteData, Size, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetHoldNoteSize(HoldNoteData, OriginSize, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetHoldNoteDuration(Lanotalium.Chart.LanotaHoldNote HoldNoteData, float Duration, bool SaveOperation = true)
    {
        float OriginDuration = HoldNoteData.Duration;
        HoldNoteData.Duration = Duration;
        InspectorManager.ComponentHoldNote.RefreshJointValues();
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetHoldNoteDuration(HoldNoteData, Duration, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetHoldNoteDuration(HoldNoteData, OriginDuration, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetHoldNoteJCount(Lanotalium.Chart.LanotaHoldNote HoldNoteData, int JCount, bool SaveOperation = true)
    {
        int OriginJCount = HoldNoteData.Jcount;
        if (OriginJCount == JCount) return;
        if (OriginJCount < JCount)
        {
            if (OriginJCount == 0)
            {
                AddJointNoteFromVoid(HoldNoteData);
                for (int i = 0; i < JCount - OriginJCount - 1; ++i)
                {
                    AddJointNoteFromExisted(HoldNoteData, HoldNoteData.Joints[HoldNoteData.Joints.Count - 1]);
                }
            }
            else
            {
                for (int i = 0; i < JCount - OriginJCount; ++i)
                {
                    AddJointNoteFromExisted(HoldNoteData, HoldNoteData.Joints[HoldNoteData.Joints.Count - 1]);
                }
            }
        }
        else if (OriginJCount > JCount)
        {
            int DeleteCount = OriginJCount - JCount;
            for (int i = 0; i < DeleteCount; ++i)
            {
                if (HoldNoteData.Joints.Count != 1) DeleteJointNote(HoldNoteData, HoldNoteData.Joints[HoldNoteData.Joints.Count - 2]);
                else DeleteJointNote(HoldNoteData, HoldNoteData.Joints[0]);
            }
        }
        int OriginJcount = HoldNoteData.Jcount;
        HoldNoteData.Jcount = JCount;
        if (HoldNoteData.Jcount != HoldNoteData.Joints.Count) throw new Lanotalium.Exceptions.JCountMismatchException();
        InspectorManager.ComponentHoldNote.RefreshJointList();
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetHoldNoteJCount(HoldNoteData, JCount, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetHoldNoteJCount(HoldNoteData, OriginJcount, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetHoldNoteTime(Lanotalium.Chart.LanotaHoldNote HoldNoteData, float Time, bool SaveOperation = true)
    {
        float OriginTime = HoldNoteData.Time;
        HoldNoteData.Time = Time;
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetHoldNoteTime(HoldNoteData, Time, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetHoldNoteTime(HoldNoteData, OriginTime, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetHoldNoteDegree(Lanotalium.Chart.LanotaHoldNote HoldNoteData, float Degree, bool isAbsolute, bool SaveOperation = true)
    {
        float OriginDegree = HoldNoteData.Degree;
        if (isAbsolute) HoldNoteData.Degree = Degree - TunerManager.CameraManager.CalculateCameraRotation(HoldNoteData.Time);
        else HoldNoteData.Degree = Degree;
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { SetHoldNoteDegree(HoldNoteData, Degree, isAbsolute, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { SetHoldNoteDegree(HoldNoteData, OriginDegree, false, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void SetJointNoteDegree(Lanotalium.Chart.LanotaHoldNote HoldNoteData, Lanotalium.Chart.LanotaJoints JointNoteData, float Degree, bool isAbsolute, bool isChained, bool SaveOperation = true)
    {
        if (!isAbsolute)
        {
            if (!isChained)
            {
                int Index = FindJointNoteIndexByReference(HoldNoteData, JointNoteData);
                if (Index == -2) throw new Lanotalium.Exceptions.NullJointsReferenceException();
                if (Index == -1) throw new Lanotalium.Exceptions.JointNoteNotFoundException();
                if (Index == HoldNoteData.Joints.Count - 1)
                {
                    HoldNoteData.Joints[Index].dDegree = Degree;
                }
                else
                {
                    float ThisdDegree = HoldNoteData.Joints[Index].dDegree;
                    float NextdDegree = HoldNoteData.Joints[Index + 1].dDegree;
                    HoldNoteData.Joints[Index].dDegree = Degree;
                    HoldNoteData.Joints[Index + 1].dDegree = NextdDegree - (HoldNoteData.Joints[Index].dDegree - ThisdDegree);
                }
            }
            else if (isChained) JointNoteData.dDegree = Degree;
        }
        else if (isAbsolute)
        {
            int Index = FindJointNoteIndexByReference(HoldNoteData, JointNoteData);
            if (Index == -2) throw new Lanotalium.Exceptions.NullJointsReferenceException();
            if (Index == -1) throw new Lanotalium.Exceptions.JointNoteNotFoundException();
            if (Index == 0 && HoldNoteData.Jcount >= 2)
            {
                if (!isChained)
                {
                    float ThisdDegree = HoldNoteData.Joints[Index].dDegree;
                    float NextdDegree = HoldNoteData.Joints[Index + 1].dDegree;
                    JointNoteData.dDegree = Degree - HoldNoteData.Degree;
                    HoldNoteData.Joints[Index + 1].dDegree = NextdDegree - (HoldNoteData.Joints[Index].dDegree - ThisdDegree);
                }
                else if (isChained)
                {
                    JointNoteData.dDegree = Degree - HoldNoteData.Degree;
                }
            }
            else if (Index == 0 && HoldNoteData.Jcount == 1)
            {
                JointNoteData.dDegree = Degree - HoldNoteData.Degree;
            }
            else if (Index != 0 && Index == HoldNoteData.Joints.Count - 1)
            {
                JointNoteData.dDegree = Degree - HoldNoteData.Joints[Index - 1].aDegree;
            }
            else
            {
                if (!isChained)
                {
                    float ThisdDegree = HoldNoteData.Joints[Index].dDegree;
                    float NextdDegree = HoldNoteData.Joints[Index + 1].dDegree;
                    JointNoteData.dDegree = Degree - HoldNoteData.Joints[Index - 1].aDegree;
                    HoldNoteData.Joints[Index + 1].dDegree = NextdDegree - (HoldNoteData.Joints[Index].dDegree - ThisdDegree);
                }
                else if (isChained)
                {
                    JointNoteData.dDegree = Degree - HoldNoteData.Joints[Index - 1].aDegree;
                }
            }
        }
        RefreshJointAbsoluteValues(HoldNoteData);
    }
    public void SetJointNoteTime(Lanotalium.Chart.LanotaHoldNote HoldNoteData, Lanotalium.Chart.LanotaJoints JointNoteData, float Time, bool isAbsolute, bool isChained, bool SaveOperation = true)
    {
        if (!isAbsolute)
        {
            if (!isChained)
            {
                int Index = FindJointNoteIndexByReference(HoldNoteData, JointNoteData);
                if (Index == -2) throw new Lanotalium.Exceptions.NullJointsReferenceException();
                if (Index == -1) throw new Lanotalium.Exceptions.JointNoteNotFoundException();
                if (Index == HoldNoteData.Joints.Count - 1)
                {
                    HoldNoteData.Joints[Index].dTime = Time;
                }
                else
                {
                    float ThisdTime = HoldNoteData.Joints[Index].dTime;
                    float NextdTime = HoldNoteData.Joints[Index + 1].dTime;
                    HoldNoteData.Joints[Index].dTime = Time;
                    HoldNoteData.Joints[Index + 1].dTime = NextdTime - (HoldNoteData.Joints[Index].dTime - ThisdTime);
                }
            }
            else if (isChained) JointNoteData.dTime = Time;
        }
        else if (isAbsolute)
        {
            int Index = FindJointNoteIndexByReference(HoldNoteData, JointNoteData);
            if (Index == -2) throw new Lanotalium.Exceptions.NullJointsReferenceException();
            if (Index == -1) throw new Lanotalium.Exceptions.JointNoteNotFoundException();
            if (Index == 0 && HoldNoteData.Jcount >= 2)
            {
                if (!isChained)
                {
                    float ThisdTime = HoldNoteData.Joints[Index].dTime;
                    float NextdTime = HoldNoteData.Joints[Index + 1].dTime;
                    JointNoteData.dTime = Time - HoldNoteData.Time;
                    HoldNoteData.Joints[Index + 1].dTime = NextdTime - (HoldNoteData.Joints[Index].dTime - ThisdTime);
                }
                else if (isChained)
                {
                    JointNoteData.dTime = Time - HoldNoteData.Time;
                }
            }
            else if (Index == 0 && HoldNoteData.Jcount == 1)
            {
                JointNoteData.dTime = Time - HoldNoteData.Time;
            }
            else if (Index != 0 && Index == HoldNoteData.Joints.Count - 1)
            {
                JointNoteData.dTime = Time - HoldNoteData.Joints[Index - 1].aTime;
            }
            else
            {
                if (!isChained)
                {
                    float ThisdTime = HoldNoteData.Joints[Index].dTime;
                    float NextdTime = HoldNoteData.Joints[Index + 1].dTime;
                    JointNoteData.dTime = Time - HoldNoteData.Joints[Index - 1].aTime;
                    HoldNoteData.Joints[Index + 1].dTime = NextdTime - (HoldNoteData.Joints[Index].dTime - ThisdTime);
                }
                else if (isChained)
                {
                    JointNoteData.dTime = Time - HoldNoteData.Joints[Index - 1].aTime;
                }
            }
        }
        RefreshJointAbsoluteValues(HoldNoteData);
    }
    public void SetJointNoteEase(Lanotalium.Chart.LanotaJoints JointNoteData, int Ease, bool SaveOperation = true)
    {
        JointNoteData.Cfmi = Ease;
    }
    public void DeleteHoldNote(Lanotalium.Chart.LanotaHoldNote HoldNoteData, bool SaveOperation = true)
    {
        Destroy(HoldNoteData.HoldNoteGameObject);
        if (HoldNoteData.Joints != null) foreach (Lanotalium.Chart.LanotaJoints Joint in HoldNoteData.Joints) Destroy(Joint.JointGameObject);
        TunerManager.HoldNoteManager.HoldNote.Remove(HoldNoteData);
        if (!SaveOperation) return;
        Lanotalium.Editor.OperationSave OpSave = new Lanotalium.Editor.OperationSave();
        OpSave.Forward = new Lanotalium.Editor.OperationForward(() => { DeleteHoldNote(HoldNoteData, false); });
        OpSave.Reverse = new Lanotalium.Editor.OperationReverse(() => { AddHoldNote(HoldNoteData, false); InspectorManager.OnSelectChange(); });
        AddToOperationSaver(OpSave);
    }
    public void DeleteJointNote(Lanotalium.Chart.LanotaHoldNote HoldNoteData, Lanotalium.Chart.LanotaJoints JointNoteData, bool SaveOperation = true)
    {
        if (HoldNoteData.Joints == null) throw new Lanotalium.Exceptions.NullJointsReferenceException();
        Destroy(JointNoteData.JointGameObject);
        HoldNoteData.Joints.Remove(JointNoteData);
        HoldNoteData.Jcount = HoldNoteData.Joints.Count;
        RefreshJointAbsoluteValues(HoldNoteData);
    }
    public void ConvertHoldNoteToTapNote(Lanotalium.Chart.LanotaHoldNote HoldNoteData, int Type, bool SaveOperation = true)
    {
        DeleteHoldNote(HoldNoteData, false);
        Lanotalium.Chart.LanotaTapNote NewTapNote = HoldNoteData.ToTapNote(Type);
        AddTapNote(NewTapNote, false);
    }
    public void RefreshJointAbsoluteValues(Lanotalium.Chart.LanotaHoldNote HoldNoteData)
    {
        float aTime = HoldNoteData.Time;
        float aDegree = HoldNoteData.Degree + TunerManager.CameraManager.CurrentRotation;
        if (HoldNoteData.Joints != null)
        {
            for (int i = 0; i <= HoldNoteData.Joints.Count - 1; ++i)
            {
                Lanotalium.Chart.LanotaJoints Joint = HoldNoteData.Joints[i];
                aTime += Joint.dTime;
                aDegree += Joint.dDegree;
                Joint.aTime = aTime;
                Joint.aDegree = aDegree - TunerManager.CameraManager.CurrentRotation;
            }
        }
    }
    public void EnsureHoldNoteJointsValid(Lanotalium.Chart.LanotaHoldNote HoldNoteData)
    {
        if (HoldNoteData.Joints == null) return;
        if (HoldNoteData.Joints.Count == 0) return;
        if (HoldNoteData.Joints.Count == 1)
        {
            HoldNoteData.Joints[0].dTime = HoldNoteData.Duration;
        }
        else
        {
            RefreshJointAbsoluteValues(HoldNoteData);
            HoldNoteData.Joints[HoldNoteData.Joints.Count - 1].dTime = HoldNoteData.Time + HoldNoteData.Duration - HoldNoteData.Joints[HoldNoteData.Joints.Count - 2].aTime;
        }
        RefreshJointAbsoluteValues(HoldNoteData);
    }
    public bool CheckJointNoteTimeIsValid(Lanotalium.Chart.LanotaHoldNote HoldNoteData)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        float TimeCount = HoldNoteData.Time;
        if (HoldNoteData.Joints == null) throw new Lanotalium.Exceptions.NullJointsReferenceException();
        foreach (Lanotalium.Chart.LanotaJoints Joint in HoldNoteData.Joints)
        {
            TimeCount += Joint.dTime;
            if (Joint.dTime <= 0) return false;
        }
        if (TimeCount > HoldNoteData.Time + HoldNoteData.Duration) return false;
        return true;
    }
    public bool CheckJointNoteTimeIsValidByJoint(Lanotalium.Chart.LanotaHoldNote HoldNoteData, Lanotalium.Chart.LanotaJoints JointNoteData)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        if (HoldNoteData.Joints == null) throw new Lanotalium.Exceptions.NullJointsReferenceException();
        int Index = FindJointNoteIndexByReference(HoldNoteData, JointNoteData);
        if (Index == -2) throw new Lanotalium.Exceptions.NullJointsReferenceException();
        if (Index == -1) throw new Lanotalium.Exceptions.JointNoteNotFoundException();
        float TimeCount = HoldNoteData.Time;
        for (int i = 0; i <= Index; ++i)
        {
            if (HoldNoteData.Joints[i].dTime <= 0) return false;
            TimeCount += HoldNoteData.Joints[i].dTime;
        }
        if (TimeCount <= HoldNoteData.Time + HoldNoteData.Duration) return true;
        if (Mathf.Approximately(TimeCount, HoldNoteData.Time + HoldNoteData.Duration)) return true;
        return false;
    }

    public void OnTimeLineClick()
    {
        SelectNothing();
        int InstanceId = EventSystem.current.currentSelectedGameObject.gameObject.GetInstanceID();
        if (!Input.GetKey(KeyCode.LeftControl)) DeSelectAllMotions();
        Lanotalium.Chart.LanotaCameraBase MotionBase = FindMotionBase(InstanceId);
        if (MotionBase == null) return;
        SelectedMotions.Add(MotionBase);
        MotionBase.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Selected;
        if (SelectedMotions.Count >= 2)
        {
            InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Multiple, 0);
            InspectorManager.ArrangeComponentsUi();
            return;
        }
        int Index = FindHorizontalIndexByInstanceId(InstanceId);
        if (Index != -1) InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Horizontal, Index);
        else
        {
            Index = FindVerticalIndexByInstanceId(InstanceId);
            if (Index != -1) InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Vertical, Index);
            else
            {
                Index = FindRotationIndexByInstanceId(InstanceId);
                if (Index == -1) return;
                else InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Rotation, Index);
            }
        }
        InspectorManager.ArrangeComponentsUi();

    }
    public void DeSelectMotion(Lanotalium.Chart.LanotaCameraBase Base)
    {
        switch (Base.Type)
        {
            case 8: Base.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Tp8; break;
            case 10: Base.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Tp10; break;
            case 11: Base.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Tp11; break;
            case 13: Base.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Tp13; break;
        }
        SelectedMotions.Remove(Base);
    }
    public void DeSelectAllMotions()
    {
        foreach (Lanotalium.Chart.LanotaCameraBase Base in SelectedMotions)
        {
            switch (Base.Type)
            {
                case 8: Base.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Tp8; break;
                case 10: Base.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Tp10; break;
                case 11: Base.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Tp11; break;
                case 13: Base.TimeLineGameObject.GetComponent<Image>().color = TimeLineManager.Tp13; break;
            }
        }
        SelectedMotions.Clear();
    }
    public Lanotalium.Chart.LanotaCameraBase FindMotionBase(int InstanceId)
    {
        int Index = FindHorizontalIndexByInstanceId(InstanceId);
        if (Index != -1) return TunerManager.CameraManager.Horizontal[Index];
        else
        {
            Index = FindVerticalIndexByInstanceId(InstanceId);
            if (Index != -1) return TunerManager.CameraManager.Vertical[Index];
            else
            {
                Index = FindRotationIndexByInstanceId(InstanceId);
                if (Index == -1) return null;
                else return TunerManager.CameraManager.Rotation[Index];
            }
        }
    }
    public int FindHorizontalIndexByInstanceId(int InstanceId)
    {
        int Index = 0;
        if (TunerManager.CameraManager.Horizontal == null) return -1;
        foreach (Lanotalium.Chart.LanotaCameraXZ Hor in TunerManager.CameraManager.Horizontal)
        {
            if (Hor.InstanceId == InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public int FindVerticalIndexByInstanceId(int InstanceId)
    {
        int Index = 0;
        if (TunerManager.CameraManager.Vertical == null) return -1;
        foreach (Lanotalium.Chart.LanotaCameraY Ver in TunerManager.CameraManager.Vertical)
        {
            if (Ver.InstanceId == InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public int FindRotationIndexByInstanceId(int InstanceId)
    {
        int Index = 0;
        if (TunerManager.CameraManager.Rotation == null) return -1;
        foreach (Lanotalium.Chart.LanotaCameraRot Rot in TunerManager.CameraManager.Rotation)
        {
            if (Rot.InstanceId == InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public bool AddHorizontal(Lanotalium.Chart.LanotaCameraXZ Hor, bool AutoDuration = true, bool SaveOperation = true, bool CallSelectNothing = true)
    {
        if (CheckNewHorizontalTimeExisted(Hor))
        {
            LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_TimeExisted"], ToolTipIcon.Warning);
            return false;
        }
        TunerManager.CameraManager.Horizontal.Add(Hor);
        TunerManager.CameraManager.SortHorizontalList();
        TimeLineManager.InstantiateSingleHorizontal(Hor);
        if (CheckHorizontalInPreviousDuration(Hor))
        {
            LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_InPrevious"], ToolTipIcon.Warning);
            DeleteHorizontal(Hor);
            return false;
        }
        if (AutoDuration) Hor.Duration = GetNewHorizontalDuration(Hor);
        else if (!AutoDuration)
        {
            if (!CheckHorizontalDurationValid(Hor, Hor.Duration))
            {
                LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_DurationOverflow"], ToolTipIcon.Warning);
                DeleteHorizontal(Hor);
                return false;
            }
        }
        TimeLineManager.InstantiateSingleHorizontal(Hor);
        if (CallSelectNothing) SelectNothing();
        InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Horizontal, FindHorizontalIndexByInstanceId(Hor.InstanceId));
        return true;
    }
    public bool AddVertical(Lanotalium.Chart.LanotaCameraY Ver, bool AutoDuration = true, bool SaveOperation = true, bool CallSelectNothing = true)
    {
        if (CheckNewVerticalTimeExisted(Ver))
        {
            LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_TimeExisted"], ToolTipIcon.Warning);
            return false;
        }
        TunerManager.CameraManager.Vertical.Add(Ver);
        TunerManager.CameraManager.SortVerticalList();
        TimeLineManager.InstantiateSingleVertical(Ver);
        if (CheckVerticalInPreviousDuration(Ver))
        {
            LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_InPrevious"], ToolTipIcon.Warning);
            DeleteVertical(Ver);
            return false;
        }
        if (AutoDuration) Ver.Duration = GetNewVerticalDuration(Ver);
        else if (!AutoDuration)
        {
            if (!CheckVerticalDurationValid(Ver, Ver.Duration))
            {
                LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_DurationOverflow"], ToolTipIcon.Warning);
                DeleteVertical(Ver);
                return false;
            }
        }
        TimeLineManager.InstantiateSingleVertical(Ver);
        if (CallSelectNothing) SelectNothing();
        InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Vertical, FindVerticalIndexByInstanceId(Ver.InstanceId));
        return true;
    }
    public bool AddRotation(Lanotalium.Chart.LanotaCameraRot Rot, bool AutoDuration = true, bool SaveOperation = true, bool CallSelectNothing = true)
    {
        if (CheckNewRotationTimeExisted(Rot))
        {
            LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_TimeExisted"], ToolTipIcon.Warning);
            return false;
        }
        TunerManager.CameraManager.Rotation.Add(Rot);
        TunerManager.CameraManager.SortRotationList();
        TimeLineManager.InstantiateSingleRotation(Rot);
        if (CheckRotationInPreviousDuration(Rot))
        {
            LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_InPrevious"], ToolTipIcon.Warning);
            DeleteRotation(Rot);
            return false;
        }
        if (AutoDuration) Rot.Duration = GetNewRotationDuration(Rot);
        else if (!AutoDuration)
        {
            if (!CheckRotationDurationValid(Rot, Rot.Duration))
            {
                LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Motion_DurationOverflow"], ToolTipIcon.Warning);
                DeleteRotation(Rot);
                return false;
            }
        }
        TimeLineManager.InstantiateSingleRotation(Rot);
        if (CallSelectNothing) SelectNothing();
        InspectorManager.ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Rotation, FindRotationIndexByInstanceId(Rot.InstanceId));
        return true;
    }
    public void DeleteHorizontal(Lanotalium.Chart.LanotaCameraXZ Hor, bool SaveOperation = true)
    {
        Destroy(Hor.TimeLineGameObject);
        TunerManager.CameraManager.Horizontal.Remove(Hor);
    }
    public void DeleteVertical(Lanotalium.Chart.LanotaCameraY Ver, bool SaveOperation = true)
    {
        Destroy(Ver.TimeLineGameObject);
        TunerManager.CameraManager.Vertical.Remove(Ver);
    }
    public void DeleteRotation(Lanotalium.Chart.LanotaCameraRot Rot, bool SaveOperation = true)
    {
        Destroy(Rot.TimeLineGameObject);
        TunerManager.CameraManager.Rotation.Remove(Rot);
    }
    public void SetHorizontalType(Lanotalium.Chart.LanotaCameraXZ Hor, int Type, bool SaveOperation = true)
    {
        Hor.Type = Type;
        TimeLineManager.InstantiateSingleHorizontal(Hor);
    }
    public void SetHorizontalTime(Lanotalium.Chart.LanotaCameraXZ Hor, float Time, bool SaveOperation = true)
    {
        Hor.Time = Time;
        TimeLineManager.InstantiateSingleHorizontal(Hor);
    }
    public void SetHorizontalDuration(Lanotalium.Chart.LanotaCameraXZ Hor, float Duration, bool SaveOperation = true)
    {
        Hor.Duration = Duration;
        TimeLineManager.InstantiateSingleHorizontal(Hor);
    }
    public void SetHorizontalEase(Lanotalium.Chart.LanotaCameraXZ Hor, int Ease, bool SaveOperation = true)
    {
        Hor.cfmi = Ease;
    }
    public void SetHorizontalDegree(Lanotalium.Chart.LanotaCameraXZ Hor, float Degree, bool SaveOperation = true)
    {
        Hor.ctp = Degree;
    }
    public void SetHorizontalRadius(Lanotalium.Chart.LanotaCameraXZ Hor, float Radius, bool SaveOperation = true)
    {
        Hor.ctp1 = Radius;
    }
    public void SetVerticalTime(Lanotalium.Chart.LanotaCameraY Ver, float Time, bool SaveOperation = true)
    {
        Ver.Time = Time;
        TimeLineManager.InstantiateSingleVertical(Ver);
    }
    public void SetVerticalDuration(Lanotalium.Chart.LanotaCameraY Ver, float Duration, bool SaveOperation = true)
    {
        Ver.Duration = Duration;
        TimeLineManager.InstantiateSingleVertical(Ver);
    }
    public void SetVerticalEase(Lanotalium.Chart.LanotaCameraY Ver, int Ease, bool SaveOperation = true)
    {
        Ver.cfmi = Ease;
    }
    public void SetVerticalHeight(Lanotalium.Chart.LanotaCameraY Ver, float Height, bool SaveOperation = true)
    {
        Ver.ctp = Height;
    }
    public void SetRotationTime(Lanotalium.Chart.LanotaCameraRot Rot, float Time, bool SaveOperation = true)
    {
        Rot.Time = Time;
        TimeLineManager.InstantiateSingleRotation(Rot);
    }
    public void SetRotationDuration(Lanotalium.Chart.LanotaCameraRot Rot, float Duration, bool SaveOperation = true)
    {
        Rot.Duration = Duration;
        TimeLineManager.InstantiateSingleRotation(Rot);
    }
    public void SetRotationEase(Lanotalium.Chart.LanotaCameraRot Rot, int Ease, bool SaveOperation = true)
    {
        Rot.cfmi = Ease;
    }
    public void SetRotationDegree(Lanotalium.Chart.LanotaCameraRot Rot, float Degree, bool SaveOperation = true)
    {
        Rot.ctp = Degree;
    }
    public float GetNewHorizontalDuration(Lanotalium.Chart.LanotaCameraXZ Hor)
    {
        int Index = FindHorizontalIndexByInstanceId(Hor.InstanceId);
        if (Index == TunerManager.CameraManager.Horizontal.Count - 1) return LimSystem.ChartContainer.ChartMusic.Length - Hor.Time > 1 ? 1 : LimSystem.ChartContainer.ChartMusic.Length - Hor.Time;
        else return TunerManager.CameraManager.Horizontal[Index + 1].Time - TunerManager.CameraManager.Horizontal[Index].Time > 1 ? 1 : TunerManager.CameraManager.Horizontal[Index + 1].Time - TunerManager.CameraManager.Horizontal[Index].Time;
    }
    public float GetNewVerticalDuration(Lanotalium.Chart.LanotaCameraY Ver)
    {
        int Index = FindVerticalIndexByInstanceId(Ver.InstanceId);
        if (Index == TunerManager.CameraManager.Vertical.Count - 1) return LimSystem.ChartContainer.ChartMusic.Length - Ver.Time > 1 ? 1 : LimSystem.ChartContainer.ChartMusic.Length - Ver.Time;
        else return TunerManager.CameraManager.Vertical[Index + 1].Time - TunerManager.CameraManager.Vertical[Index].Time > 1 ? 1 : TunerManager.CameraManager.Vertical[Index + 1].Time - TunerManager.CameraManager.Vertical[Index].Time;
    }
    public float GetNewRotationDuration(Lanotalium.Chart.LanotaCameraRot Rot)
    {
        int Index = FindRotationIndexByInstanceId(Rot.InstanceId);
        if (Index == TunerManager.CameraManager.Rotation.Count - 1) return LimSystem.ChartContainer.ChartMusic.Length - Rot.Time > 1 ? 1 : LimSystem.ChartContainer.ChartMusic.Length - Rot.Time;
        else return TunerManager.CameraManager.Rotation[Index + 1].Time - TunerManager.CameraManager.Rotation[Index].Time > 1 ? 1 : TunerManager.CameraManager.Rotation[Index + 1].Time - TunerManager.CameraManager.Rotation[Index].Time;
    }
    public bool CheckHorizontalInPreviousDuration(Lanotalium.Chart.LanotaCameraXZ Hor)
    {
        if (LimSystem.Preferences.Unsafe) return false;
        int Index = FindHorizontalIndexByInstanceId(Hor.InstanceId);
        if (Index == 0) return false;
        else
        {
            if (TunerManager.CameraManager.Horizontal[Index - 1].Time + TunerManager.CameraManager.Horizontal[Index - 1].Duration > Hor.Time) return true;
        }
        return false;
    }
    public bool CheckVerticalInPreviousDuration(Lanotalium.Chart.LanotaCameraY Ver)
    {
        if (LimSystem.Preferences.Unsafe) return false;
        int Index = FindVerticalIndexByInstanceId(Ver.InstanceId);
        if (Index == 0) return false;
        else
        {
            if (TunerManager.CameraManager.Vertical[Index - 1].Time + TunerManager.CameraManager.Vertical[Index - 1].Duration > Ver.Time) return true;
        }
        return false;
    }
    public bool CheckRotationInPreviousDuration(Lanotalium.Chart.LanotaCameraRot Rot)
    {
        if (LimSystem.Preferences.Unsafe) return false;
        int Index = FindRotationIndexByInstanceId(Rot.InstanceId);
        if (Index == 0) return false;
        else
        {
            if (TunerManager.CameraManager.Rotation[Index - 1].Time + TunerManager.CameraManager.Rotation[Index - 1].Duration > Rot.Time) return true;
        }
        return false;
    }
    public bool CheckNewHorizontalTimeExisted(Lanotalium.Chart.LanotaCameraXZ Hor)
    {
        if (LimSystem.Preferences.Unsafe) return false;
        foreach (Lanotalium.Chart.LanotaCameraXZ Hori in TunerManager.CameraManager.Horizontal)
        {
            if (Hori.Time == Hor.Time) return true;
        }
        return false;
    }
    public bool CheckNewVerticalTimeExisted(Lanotalium.Chart.LanotaCameraY Ver)
    {
        if (LimSystem.Preferences.Unsafe) return false;
        foreach (Lanotalium.Chart.LanotaCameraY Vert in TunerManager.CameraManager.Vertical)
        {
            if (Vert.Time == Ver.Time) return true;
        }
        return false;
    }
    public bool CheckNewRotationTimeExisted(Lanotalium.Chart.LanotaCameraRot Rot)
    {
        if (LimSystem.Preferences.Unsafe) return false;
        foreach (Lanotalium.Chart.LanotaCameraRot Rota in TunerManager.CameraManager.Rotation)
        {
            if (Rota.Time == Rot.Time) return true;
        }
        return false;
    }
    public bool CheckHorizontalTimeValid(Lanotalium.Chart.LanotaCameraXZ Hor, float Time)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        int Index = FindHorizontalIndexByInstanceId(Hor.InstanceId);
        if (Index == -1) return false;
        if (Index + 1 <= TunerManager.CameraManager.Horizontal.Count - 1)
        {
            if (Time + Hor.Duration > TunerManager.CameraManager.Horizontal[Index + 1].Time) return false;
        }
        else
        {
            if (Time > LimSystem.ChartContainer.ChartMusic.Length) return false;
        }
        if (Index - 1 >= 0)
        {
            if (TunerManager.CameraManager.Horizontal[Index - 1].Time + TunerManager.CameraManager.Horizontal[Index - 1].Duration > Time) return false;
        }
        else
        {
            if (Time < 0) return false;
        }
        return true;
    }
    public bool CheckHorizontalDurationValid(Lanotalium.Chart.LanotaCameraXZ Hor, float Duration)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        if (Duration < 0.0001f) return false;
        int Index = FindHorizontalIndexByInstanceId(Hor.InstanceId);
        if (Index == -1) return false;
        if (Index + 1 <= TunerManager.CameraManager.Horizontal.Count - 1)
        {
            if (Hor.Time + Duration > TunerManager.CameraManager.Horizontal[Index + 1].Time) return false;
        }
        else
        {
            if (Hor.Time > LimSystem.ChartContainer.ChartMusic.Length) return false;
        }
        if (Index - 1 >= 0)
        {
            if (TunerManager.CameraManager.Horizontal[Index - 1].Time + TunerManager.CameraManager.Horizontal[Index - 1].Duration > Hor.Time) return false;
        }
        else
        {
            if (Hor.Time < 0) return false;
        }
        return true;
    }
    public bool CheckVerticalTimeValid(Lanotalium.Chart.LanotaCameraY Ver, float Time)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        int Index = FindVerticalIndexByInstanceId(Ver.InstanceId);
        if (Index == -1) return false;
        if (Index + 1 <= TunerManager.CameraManager.Vertical.Count - 1)
        {
            if (Time + Ver.Duration > TunerManager.CameraManager.Vertical[Index + 1].Time) return false;
        }
        else
        {
            if (Time > LimSystem.ChartContainer.ChartMusic.Length) return false;
        }
        if (Index - 1 >= 0)
        {
            if (TunerManager.CameraManager.Vertical[Index - 1].Time + TunerManager.CameraManager.Vertical[Index - 1].Duration > Time) return false;
        }
        else
        {
            if (Time < 0) return false;
        }
        return true;
    }
    public bool CheckVerticalDurationValid(Lanotalium.Chart.LanotaCameraY Ver, float Duration)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        if (Duration < 0.0001f) return false;
        int Index = FindVerticalIndexByInstanceId(Ver.InstanceId);
        if (Index == -1) return false;
        if (Index + 1 <= TunerManager.CameraManager.Vertical.Count - 1)
        {
            if (Ver.Time + Duration > TunerManager.CameraManager.Vertical[Index + 1].Time) return false;
        }
        else
        {
            if (Ver.Time > LimSystem.ChartContainer.ChartMusic.Length) return false;
        }
        if (Index - 1 >= 0)
        {
            if (TunerManager.CameraManager.Vertical[Index - 1].Time + TunerManager.CameraManager.Vertical[Index - 1].Duration > Ver.Time) return false;
        }
        else
        {
            if (Ver.Time < 0) return false;
        }
        return true;
    }
    public bool CheckRotationTimeValid(Lanotalium.Chart.LanotaCameraRot Rot, float Time)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        int Index = FindRotationIndexByInstanceId(Rot.InstanceId);
        if (Index == -1) return false;
        if (Index + 1 <= TunerManager.CameraManager.Rotation.Count - 1)
        {
            if (Time + Rot.Duration > TunerManager.CameraManager.Rotation[Index + 1].Time) return false;
        }
        else
        {
            if (Time > LimSystem.ChartContainer.ChartMusic.Length) return false;
        }
        if (Index - 1 >= 0)
        {
            if (TunerManager.CameraManager.Rotation[Index - 1].Time + TunerManager.CameraManager.Rotation[Index - 1].Duration > Time) return false;
        }
        else
        {
            if (Time < 0) return false;
        }
        return true;
    }
    public bool CheckRotationDurationValid(Lanotalium.Chart.LanotaCameraRot Rot, float Duration)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        if (Duration < 0.0001f) return false;
        int Index = FindRotationIndexByInstanceId(Rot.InstanceId);
        if (Index == -1) return false;
        if (Index + 1 <= TunerManager.CameraManager.Rotation.Count - 1)
        {
            if (Rot.Time + Duration > TunerManager.CameraManager.Rotation[Index + 1].Time) return false;
        }
        else
        {
            if (Rot.Time > LimSystem.ChartContainer.ChartMusic.Length) return false;
        }
        if (Index - 1 >= 0)
        {
            if (TunerManager.CameraManager.Rotation[Index - 1].Time + TunerManager.CameraManager.Rotation[Index - 1].Duration > Rot.Time) return false;
        }
        else
        {
            if (Rot.Time < 0) return false;
        }
        return true;
    }

    public int FindBpmIndexByInstanceId(int InstanceId)
    {
        int Index = 0;
        if (TunerManager.BpmManager.Bpm == null) return -1;
        foreach (Lanotalium.Chart.LanotaChangeBpm Bpm in TunerManager.BpmManager.Bpm)
        {
            if (Bpm.InstanceId == InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public void AddBpm(Lanotalium.Chart.LanotaChangeBpm BpmData, bool SaveOperation = true)
    {
        if (CheckNewBpmTimeExisted(BpmData)) return;
        TunerManager.BpmManager.Bpm.Add(BpmData);
        TunerManager.BpmManager.SortBpmList();
        InspectorManager.ComponentBpm.InstantiateBpmList();
        InspectorManager.ComponentBpm.ReCalculateBeatlineTimes();
    }
    public void SetBpmTime(Lanotalium.Chart.LanotaChangeBpm BpmData, float Time, bool SaveOperation = true)
    {
        BpmData.Time = Time;
        InspectorManager.ComponentBpm.ReCalculateBeatlineTimes();
    }
    public void SetBpmBpm(Lanotalium.Chart.LanotaChangeBpm BpmData, float Bpm, bool SaveOperation = true)
    {
        BpmData.Bpm = Bpm;
        InspectorManager.ComponentBpm.ReCalculateBeatlineTimes();
    }
    public void DeleteBpm(Lanotalium.Chart.LanotaChangeBpm BpmData, bool SaveOperation = true)
    {
        Destroy(BpmData.ListGameObject);
        TunerManager.BpmManager.Bpm.Remove(BpmData);
        InspectorManager.ComponentBpm.InstantiateBpmList();
        InspectorManager.ComponentBpm.ReCalculateBeatlineTimes();
    }
    public bool CheckNewBpmTimeExisted(Lanotalium.Chart.LanotaChangeBpm BpmData)
    {
        if (LimSystem.Preferences.Unsafe) return false;
        foreach (Lanotalium.Chart.LanotaChangeBpm Bpm in TunerManager.BpmManager.Bpm)
        {
            if (Bpm.Time == BpmData.Time) return true;
        }
        return false;
    }
    public bool CheckBpmTimeValid(Lanotalium.Chart.LanotaChangeBpm BpmData, float Time)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        if (Time < 0) return false;
        int Index = FindBpmIndexByInstanceId(BpmData.InstanceId);
        if (TunerManager.BpmManager.Bpm[Index - 1].Time >= Time) return false;
        if (Index != TunerManager.BpmManager.Bpm.Count - 1)
        {
            if (Time >= TunerManager.BpmManager.Bpm[Index + 1].Time) return false;
        }
        return true;
    }
    public float FindAttachToBeatlineByTime(float Time, float Threshold)
    {
        if (InspectorManager.ComponentBpm.BeatlineTimes == null) throw new Lanotalium.Exceptions.NullBeatlineTimesException();
        int Index = InspectorManager.ComponentBpm.FindBeatlineTimesPositionByTime(Time);
        if (Index == 0) return Time;
        else if (Index == InspectorManager.ComponentBpm.BeatlineTimes.Count)
        {
            if (Time - InspectorManager.ComponentBpm.BeatlineTimes[InspectorManager.ComponentBpm.BeatlineTimes.Count - 1] < Threshold)
                return InspectorManager.ComponentBpm.BeatlineTimes[InspectorManager.ComponentBpm.BeatlineTimes.Count - 1];
            else return Time;
        }
        else
        {
            float TimeToPrev = Time - InspectorManager.ComponentBpm.BeatlineTimes[Index - 1];
            float TimeToNext = InspectorManager.ComponentBpm.BeatlineTimes[Index] - Time;
            if (TimeToPrev > TimeToNext)
            {
                if (TimeToNext < Threshold) return InspectorManager.ComponentBpm.BeatlineTimes[Index];
                else return Time;
            }
            else
            {
                if (TimeToPrev < Threshold) return InspectorManager.ComponentBpm.BeatlineTimes[Index - 1];
                else return Time;
            }
        }
    }
    public float FindNearestBeatlineByTime(float Time)
    {
        if (InspectorManager.ComponentBpm.BeatlineTimes == null) throw new Lanotalium.Exceptions.NullBeatlineTimesException();
        int Index = InspectorManager.ComponentBpm.FindBeatlineTimesPositionByTime(Time);
        if (Index == 0) return 0;
        else if (Index == InspectorManager.ComponentBpm.BeatlineTimes.Count) return InspectorManager.ComponentBpm.BeatlineTimes[InspectorManager.ComponentBpm.BeatlineTimes.Count - 1];
        else
        {
            float TimeToPrev = Time - InspectorManager.ComponentBpm.BeatlineTimes[Index - 1];
            float TimeToNext = InspectorManager.ComponentBpm.BeatlineTimes[Index] - Time;
            return TimeToPrev > TimeToNext ? InspectorManager.ComponentBpm.BeatlineTimes[Index] : InspectorManager.ComponentBpm.BeatlineTimes[Index - 1];
        }
    }
    public int FindNearestBeatlineIndexByTime(float Time)
    {
        if (InspectorManager.ComponentBpm.BeatlineTimes == null) throw new Lanotalium.Exceptions.NullBeatlineTimesException();
        int Index = InspectorManager.ComponentBpm.FindBeatlineTimesPositionByTime(Time);
        if (Index == 0) return 0;
        else if (Index == InspectorManager.ComponentBpm.BeatlineTimes.Count) return InspectorManager.ComponentBpm.BeatlineTimes.Count - 1;
        else
        {
            float TimeToPrev = Time - InspectorManager.ComponentBpm.BeatlineTimes[Index - 1];
            float TimeToNext = InspectorManager.ComponentBpm.BeatlineTimes[Index] - Time;
            return TimeToPrev > TimeToNext ? Index : Index - 1;
        }
    }
    public void FixTapNoteTimeToBeatline(Lanotalium.Chart.LanotaTapNote TapNoteData)
    {
        SetTapNoteTime(TapNoteData, FindNearestBeatlineByTime(TapNoteData.Time));
    }
    public void FixHoldNoteTimeToBeatline(Lanotalium.Chart.LanotaHoldNote HoldNoteData)
    {
        SetHoldNoteTime(HoldNoteData, FindNearestBeatlineByTime(HoldNoteData.Time));
        RefreshJointAbsoluteValues(HoldNoteData);
        if (HoldNoteData.Joints != null)
        {
            for (int i = 0; i < HoldNoteData.Joints.Count; ++i)
            {
                SetJointNoteTime(HoldNoteData, HoldNoteData.Joints[i], FindNearestBeatlineByTime(HoldNoteData.Joints[i].aTime), true, false);
                RefreshJointAbsoluteValues(HoldNoteData);
            }
        }
    }
    public void FixSelectedNotesToBeatline()
    {
        if (InspectorManager.ComponentBpm.BeatlineTimes == null) return;
        foreach (Lanotalium.Chart.LanotaTapNote Tap in SelectedTapNote)
        {
            FixTapNoteTimeToBeatline(Tap);
        }
        foreach (Lanotalium.Chart.LanotaHoldNote Hold in SelectedHoldNote)
        {
            FixHoldNoteTimeToBeatline(Hold);
        }
        SelectNothing();
    }
    public void FixAllNotesToBeatline()
    {
        if (InspectorManager.ComponentBpm.BeatlineTimes == null) return;
        foreach (Lanotalium.Chart.LanotaTapNote Tap in TunerManager.TapNoteManager.TapNote)
        {
            FixTapNoteTimeToBeatline(Tap);
        }
        foreach (Lanotalium.Chart.LanotaHoldNote Hold in TunerManager.HoldNoteManager.HoldNote)
        {
            FixHoldNoteTimeToBeatline(Hold);
        }
        SelectNothing();
    }

    public int FindScrollIndexByInstanceId(int InstanceId)
    {
        int Index = 0;
        if (TunerManager.ScrollManager.Scroll == null) return -1;
        foreach (Lanotalium.Chart.LanotaScroll Scroll in TunerManager.ScrollManager.Scroll)
        {
            if (Scroll.InstanceId == InstanceId) return Index;
            Index++;
        }
        return -1;
    }
    public void AddScrollSpeed(Lanotalium.Chart.LanotaScroll ScrollData, bool SaveOperation = true)
    {
        if (CheckNewScrollTimeExisted(ScrollData)) return;
        TunerManager.ScrollManager.Scroll.Add(ScrollData);
        TunerManager.ScrollManager.SortScrollList();
        InspectorManager.ComponentScrollSpeed.InstantiateScrollSpeedList();
    }
    public void SetScrollTime(Lanotalium.Chart.LanotaScroll ScrollData, float Time, bool SaveOperation = true)
    {
        ScrollData.Time = Time;
    }
    public void SetScrollSpeed(Lanotalium.Chart.LanotaScroll ScrollData, float Speed, bool SaveOperation = true)
    {
        ScrollData.Speed = Speed;
    }
    public void DeleteScroll(Lanotalium.Chart.LanotaScroll ScrollData, bool SaveOperation = true)
    {
        Destroy(ScrollData.ListGameObject);
        TunerManager.ScrollManager.Scroll.Remove(ScrollData);
        InspectorManager.ComponentScrollSpeed.InstantiateScrollSpeedList();
    }
    public bool CheckNewScrollTimeExisted(Lanotalium.Chart.LanotaScroll ScrollData)
    {
        if (LimSystem.Preferences.Unsafe) return false;
        foreach (Lanotalium.Chart.LanotaScroll Scroll in TunerManager.ScrollManager.Scroll)
        {
            if (Scroll.Time == ScrollData.Time) return true;
        }
        return false;
    }
    public bool CheckScrollTimeValid(Lanotalium.Chart.LanotaScroll ScrollData, float Time)
    {
        if (LimSystem.Preferences.Unsafe) return true;
        if (Time < 0) return false;
        int Index = FindScrollIndexByInstanceId(ScrollData.InstanceId);
        if (TunerManager.ScrollManager.Scroll[Index - 1].Time >= Time) return false;
        if (Index != TunerManager.ScrollManager.Scroll.Count - 1)
        {
            if (Time >= TunerManager.ScrollManager.Scroll[Index + 1].Time) return false;
        }
        return true;
    }

    public void SetDefaultRadius(float Radius, bool SaveOperation = true)
    {
        TunerManager.ChartContainer.ChartData.LanotaDefault.CamRou = Radius;
    }
    public void SetDefaultDegree(float Degree, bool SaveOperation = true)
    {
        TunerManager.ChartContainer.ChartData.LanotaDefault.CamTheta = Degree;
    }
    public void SetDefaultHeight(float Height, bool SaveOperation = true)
    {
        TunerManager.ChartContainer.ChartData.LanotaDefault.CamHeight = Height;
    }
    public void SetDefaultRotation(float Rotation, bool SaveOperation = true)
    {
        TunerManager.ChartContainer.ChartData.LanotaDefault.Degree = Rotation;
    }
}