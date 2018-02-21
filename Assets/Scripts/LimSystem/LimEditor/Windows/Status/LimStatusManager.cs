using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimStatusManager : MonoBehaviour
{
    public LimWindowManager BaseWindow;
    public LimTunerManager TunerManager;
    public Text StatusText, StatusKeyText, StatusValueText;

    public void SetTexts()
    {
        StatusText.text = LimLanguageManager.TextDict["Status"];
    }
    private void Class(string Name)
    {
        StatusKeyText.text += string.Format(">{0}\n", Name);
        StatusValueText.text += "\n";
    }
    private void Int(string Name, int Value)
    {
        StatusKeyText.text += string.Format("<size=16> >{0}</size>\n", Name);
        StatusValueText.text += string.Format("{0}\n", Value);
    }
    private void Float(string Name, float Value)
    {
        StatusKeyText.text += string.Format("<size=16> >{0}</size>\n", Name);
        StatusValueText.text += string.Format("{0}\n", Value.ToString("f4"));
    }
    private void Coordinate(string Name, float Value1, float Value2)
    {
        StatusKeyText.text += string.Format("<size=16> >{0}</size>\n", Name);
        StatusValueText.text += string.Format("({0}, {1})\n", Value1.ToString("f4"), Value2.ToString("f4"));
    }
    void Update()
    {
        StatusKeyText.text = string.Empty;
        StatusValueText.text = string.Empty;
        if (TunerManager.isInitialized)
        {
            Class(LimLanguageManager.TextDict["Status_TunerStatus"]);
            Float(LimLanguageManager.TextDict["Status_Rotation"], TunerManager.CameraManager.CurrentRotation);
            Float(LimLanguageManager.TextDict["Status_Height"], TunerManager.CameraManager.CurrentVerticalY);
            Coordinate(LimLanguageManager.TextDict["Status_Coordinate_Polar"], TunerManager.CameraManager.CurrentRou, TunerManager.CameraManager.CurrentTheta);
            Coordinate(LimLanguageManager.TextDict["Status_Coordinate_Rect"], TunerManager.CameraManager.CurrentHorizontalX, TunerManager.CameraManager.CurrentHorizontalZ);
            Class(LimLanguageManager.TextDict["Status_ChartStatus"]);
            Int(LimLanguageManager.TextDict["Status_TapNote"], TunerManager.TapNoteManager.TapNote.Count);
            Int(LimLanguageManager.TextDict["Status_HoldNote"], TunerManager.HoldNoteManager.HoldNote.Count);
            Float(LimLanguageManager.TextDict["Status_CurrentBpm"], TunerManager.BpmManager.CurrentBpm);
            Float(LimLanguageManager.TextDict["Status_CurrentSpeed"], TunerManager.ScrollManager.CurrentScrollSpeed);
        }
        else
        {
            Class(LimLanguageManager.TextDict["Status_LoadProject"]);
        }
    }
}
