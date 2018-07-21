using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LimCopierManager : MonoBehaviour
{
    public LimCreatorToolBase ToolBase;
    public InputField CopyInstructionInputField;
    public LimOperationManager OperationManager;
    public Color InvalidColor, ValidColor;
    public Image CopyInstructionImg;
    public Text CopyNoteText, CopyMotionText, HintText;

    private List<float> CopyTargets = new List<float>();

    public void SetTexts()
    {
        HintText.text = LimLanguageManager.TextDict["Copier_Hint"];
        CopyNoteText.text = LimLanguageManager.TextDict["Copier_CopyNote"];
        CopyMotionText.text = LimLanguageManager.TextDict["Copier_CopyMotion"];
    }

    public bool ValidateCopyInstruction()
    {
        bool Result = AnalysisCopyInstruction();
        if (Result)
        {
            CopyInstructionImg.color = ValidColor;
        }
        else
        {
            CopyInstructionImg.color = InvalidColor;
        }
        return Result;
    }
    private bool AnalysisCopyInstruction()
    {
        CopyTargets.Clear();
        string Instrus = CopyInstructionInputField.text;
        string[] InstruList = Instrus.Split(';');
        foreach (string Instru in InstruList)
        {
            float tTarget;
            if (float.TryParse(Instru, out tTarget)) CopyTargets.Add(tTarget);
            else
            {
                float tStart, tInterval;
                int tTimes;
                string[] Split = Instru.Split('+', '*');
                if (Split.Length != 3) return false;
                if (!float.TryParse(Split[0], out tStart)) return false;
                if (!float.TryParse(Split[1], out tInterval)) return false;
                if (!int.TryParse(Split[2], out tTimes)) return false;
                for (int i = 0; i <= tTimes; ++i)
                {
                    CopyTargets.Add(tStart + i * tInterval);
                }
            }
        }
        return true;
    }
    public void CopySelectedNotes()
    {
        if (!OperationManager.TunerManager.isInitialized) return;
        if (!ValidateCopyInstruction()) return;

        bool HasTap = false, HasHold = false;
        float FirstNoteTime = 0;
        if (OperationManager.SelectedTapNote.Count != 0)
        {
            OperationManager.SelectedTapNote.Sort((Lanotalium.Chart.LanotaTapNote a, Lanotalium.Chart.LanotaTapNote b) => { return a.Time.CompareTo(b.Time); });
            FirstNoteTime = OperationManager.SelectedTapNote[0].Time;
            HasTap = true;
        }
        if (OperationManager.SelectedHoldNote.Count != 0)
        {
            OperationManager.SelectedHoldNote.Sort((Lanotalium.Chart.LanotaHoldNote a, Lanotalium.Chart.LanotaHoldNote b) => { return a.Time.CompareTo(b.Time); });
            FirstNoteTime = OperationManager.SelectedHoldNote[0].Time;
            HasHold = true;
        }
        if (HasTap && HasHold) FirstNoteTime = Mathf.Min(OperationManager.SelectedTapNote[0].Time, OperationManager.SelectedHoldNote[0].Time);
        if (!HasTap && !HasHold) return;
        if (HasTap)
        {
            foreach (float Target in CopyTargets)
            {
                float Delta = Target - FirstNoteTime;
                foreach (Lanotalium.Chart.LanotaTapNote Tap in OperationManager.SelectedTapNote)
                {
                    Lanotalium.Chart.LanotaTapNote New = Tap.DeepCopy();
                    New.Time += Delta;
                    OperationManager.AddTapNote(New, true, false, false);
                }
            }
        }
        if (HasHold)
        {
            foreach (float Target in CopyTargets)
            {
                float Delta = Target - FirstNoteTime;
                foreach (Lanotalium.Chart.LanotaHoldNote Hold in OperationManager.SelectedHoldNote)
                {
                    Lanotalium.Chart.LanotaHoldNote New = Hold.DeepCopy();
                    New.Time += Delta;
                    OperationManager.AddHoldNote(New, true, false, false);
                }
            }
        }
        OperationManager.SelectNothing();
        LimNotifyIcon.ShowMessage(LimLanguageManager.TextDict["Copier_Msg_Success"]);
    }
    public void CopySelectedMotion()
    {
        if (!OperationManager.TunerManager.isInitialized) return;
        if (!ValidateCopyInstruction()) return;
        if (OperationManager.SelectedMotions.Count < 1) return;
        OperationManager.SelectedMotions.Sort((Lanotalium.Chart.LanotaCameraBase a, Lanotalium.Chart.LanotaCameraBase b) => { return a.Time.CompareTo(b.Time); });
        float FirstTime = OperationManager.SelectedMotions[0].Time;
        foreach (float Target in CopyTargets)
        {
            foreach (Lanotalium.Chart.LanotaCameraBase Base in OperationManager.SelectedMotions)
            {
                switch (Base.Type)
                {
                    case 8:
                    case 11:
                        Lanotalium.Chart.LanotaCameraXZ NewH = (Base as Lanotalium.Chart.LanotaCameraXZ).DeepCopy();
                        NewH.Time = Base.Time - FirstTime + Target;
                        OperationManager.AddHorizontal(NewH, false, true, false); break;
                    case 10:
                        Lanotalium.Chart.LanotaCameraY NewV = (Base as Lanotalium.Chart.LanotaCameraY).DeepCopy();
                        NewV.Time = Base.Time - FirstTime + Target;
                        OperationManager.AddVertical(NewV, false, true, false); break;
                    case 13:
                        Lanotalium.Chart.LanotaCameraRot NewR = (Base as Lanotalium.Chart.LanotaCameraRot).DeepCopy();
                        NewR.Time = Base.Time - FirstTime + Target;
                        OperationManager.AddRotation(NewR, false, true, false); break;
                }
            }
        }
        LimNotifyIcon.ShowMessage(LimLanguageManager.TextDict["Copier_Msg_Success"]);
    }
    public void OnCopyInstructionChange()
    {
        ValidateCopyInstruction();
    }
}
