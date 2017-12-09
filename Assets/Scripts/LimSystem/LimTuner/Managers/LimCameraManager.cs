using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimCameraManager : MonoBehaviour
{
    private bool isInitialized = false;
    public LimTunerManager Tuner;
    public List<Lanotalium.Chart.LanotaCameraRot> Rotation;
    public List<Lanotalium.Chart.LanotaCameraXZ> Horizontal;
    public List<Lanotalium.Chart.LanotaCameraY> Vertical;
    public Lanotalium.Chart.LanotaDefault Default;
    public GameObject TunerCamera, TunerGameObject;
    public bool DisableCameraUpdate = false;
    public bool DisableMotion = false;

    public float CurrentRotation = 0;
    public float CurrentHorizontalX = 0;
    public float CurrentHorizontalZ = 0;
    public float CurrentVerticalY = 0;

    void Update()
    {
        if (!isInitialized) return;
        if (DisableCameraUpdate) return;
        if (DisableMotion)
        {
            DisableMotionUpdate();
            return;
        }
        UpdateCameraRotation();
        UpdateCameraHorizontal();
        UpdateCameraVertical();
        UpdateCameraStatus();
    }

    public void Initialize(
        List<Lanotalium.Chart.LanotaCameraRot> RotationData,
        List<Lanotalium.Chart.LanotaCameraXZ> HorizontalData,
        List<Lanotalium.Chart.LanotaCameraY> VerticalData,
        Lanotalium.Chart.LanotaDefault DefaultData)
    {
        Rotation = RotationData;
        Horizontal = HorizontalData;
        Vertical = VerticalData;
        Default = DefaultData;
        isInitialized = true;
    }

    private float CalculateMovePercent(float JudgeTime)
    {
        int StartScroll = 0, EndScroll = 0;
        float Percent = 100;
        for (int i = 0; i < Tuner.ScrollManager.Scroll.Count - 1; ++i)
        {
            if (Tuner.ChartTime >= Tuner.ScrollManager.Scroll[i].Time && Tuner.ChartTime < Tuner.ScrollManager.Scroll[i + 1].Time) StartScroll = i;
            if (JudgeTime >= Tuner.ScrollManager.Scroll[i].Time && JudgeTime < Tuner.ScrollManager.Scroll[i + 1].Time) EndScroll = i;
        }
        if (Tuner.ScrollManager.Scroll.Count != 0)
        {
            if (Tuner.ChartTime >= Tuner.ScrollManager.Scroll[Tuner.ScrollManager.Scroll.Count - 1].Time) StartScroll = Tuner.ScrollManager.Scroll.Count - 1;
            if (JudgeTime >= Tuner.ScrollManager.Scroll[Tuner.ScrollManager.Scroll.Count - 1].Time) EndScroll = Tuner.ScrollManager.Scroll.Count - 1;
        }
        for (int i = StartScroll; i <= EndScroll; ++i)
        {
            if (StartScroll == EndScroll) Percent -= (JudgeTime - Tuner.ChartTime) * Tuner.ScrollManager.Scroll[i].Speed * 10 * Tuner.ChartPlaySpeed;
            else if (StartScroll != EndScroll)
            {
                if (i == StartScroll) Percent -= (Tuner.ScrollManager.Scroll[i + 1].Time - Tuner.ChartTime) * Tuner.ScrollManager.Scroll[i].Speed * 10 * Tuner.ChartPlaySpeed;
                else if (i != EndScroll && i != StartScroll) Percent -= (Tuner.ScrollManager.Scroll[i + 1].Time - Tuner.ScrollManager.Scroll[i].Time) * Tuner.ScrollManager.Scroll[i].Speed * 10 * Tuner.ChartPlaySpeed;
                else if (i == EndScroll) Percent -= (JudgeTime - Tuner.ScrollManager.Scroll[i].Time) * Tuner.ScrollManager.Scroll[i].Speed * 10 * Tuner.ChartPlaySpeed;
            }
        }
        Percent = Mathf.Clamp(Percent, 0, 100);
        return Percent;
    }
    private float CalculateEasedPercent(float Percent)
    {
        return Mathf.Pow(2, 10 * (Percent / 100 - 1)) * 100;
    }
    private float CalculateEasedCurve(float Percent, int Mode)
    {
        if (Percent >= 1.0) return 1.0f;
        else if (Percent <= 0.0) return 0.0f;
        switch (Mode)
        {
            case 0:
                return Percent;
            case 1:
                return Percent * Percent * Percent * Percent;
            case 2:
                return -(Percent - 1) * (Percent - 1) * (Percent - 1) * (Percent - 1) + 1;
            case 3:
                return (Percent < 0.5) ? (Percent * Percent * Percent * Percent * 8) : ((Percent - 1) * (Percent - 1) * (Percent - 1) * (Percent - 1) * -8 + 1);
            case 4:
                return Percent * Percent * Percent;
            case 5:
                return (Percent - 1) * (Percent - 1) * (Percent - 1) + 1;
            case 6:
                return (Percent < 0.5) ? (Percent * Percent * Percent * 4) : ((Percent - 1) * (Percent - 1) * (Percent - 1) * 4 + 1);
            case 7:
                return Mathf.Pow(2, 10 * (float)(Percent - 1));
            case 8:
                return -Mathf.Pow(2, -10 * (float)Percent) + 1;
            case 9:
                return (Percent < 0.5) ? (Mathf.Pow(2, 10 * (2 * (float)Percent - 1)) / 2) : ((-Mathf.Pow(2, -10 * (2 * (float)Percent - 1)) + 2) / 2);
            case 10:
                return -Mathf.Cos((float)Percent * Mathf.PI / 2) + 1;
            case 11:
                return Mathf.Sin((float)Percent * Mathf.PI / 2);
            case 12:
                return (Mathf.Cos((float)Percent * Mathf.PI) - 1) / -2;
        }
        return 1;
    }
    private void CalculatePolarToRectangle(float Rou, float Theta)
    {
        CurrentHorizontalX = -Rou * Mathf.Cos(Theta * Mathf.Deg2Rad);
        CurrentHorizontalZ = Rou * Mathf.Sin(Theta * Mathf.Deg2Rad);
    }

    private void DisableMotionUpdate()
    {
        CurrentRotation = 0;
        CurrentHorizontalX = 0;
        CurrentHorizontalZ = 1;
        CurrentVerticalY = -22;
        UpdateCameraStatus();
    }

    private void UpdateCameraRotation()
    {
        int EndRotation = -1;
        CurrentRotation = -Default.Degree;
        for (int i = 0; i < Rotation.Count - 1; ++i)
        {
            if (Tuner.ChartTime >= Rotation[i].Time && Tuner.ChartTime < Rotation[i + 1].Time)
            {
                EndRotation = i;
                break;
            }
        }
        if (Rotation.Count != 0)
            if (Tuner.ChartTime >= Rotation[Rotation.Count - 1].Time) EndRotation = Rotation.Count - 1;
        for (int i = -1; i <= EndRotation; ++i)
        {
            if (EndRotation == -1)
            {
                return;
            }
            else if (i != -1)
            {
                if (i == EndRotation) CurrentRotation += Rotation[i].ctp * CalculateEasedCurve((Tuner.ChartTime - Rotation[i].Time) / Rotation[i].Duration, Rotation[i].cfmi);
                else if (i != EndRotation)
                {
                    if (Rotation[i + 1].Time - Rotation[i].Time < Rotation[i].Duration)
                        CurrentRotation += Rotation[i].ctp * CalculateEasedCurve((Rotation[i + 1].Time - Rotation[i].Time) / Rotation[i].Duration, Rotation[i].cfmi);
                    else CurrentRotation += Rotation[i].ctp;
                }
            }
        }
    }
    private void UpdateCameraHorizontal()
    {
        int EndHorizontal = -1;
        float Rou = Default.CamRou, Theta = Default.CamTheta;
        float StartX, StartZ, EndX, EndZ, DeltaX, DeltaZ, ResultX, ResultZ;
        for (int i = 0; i < Horizontal.Count - 1; ++i)
        {
            if (Tuner.ChartTime >= Horizontal[i].Time && Tuner.ChartTime < Horizontal[i + 1].Time)
            {
                EndHorizontal = i;
                break;
            }
        }
        if (Horizontal.Count != 0)
            if (Tuner.ChartTime >= Horizontal[Horizontal.Count - 1].Time) EndHorizontal = Horizontal.Count - 1;
        for (int i = -1; i <= EndHorizontal; ++i)
        {
            if (EndHorizontal == -1)
            {
                CalculatePolarToRectangle(Rou, Theta);
                return;
            }
            else if (i != -1)
            {
                if (i == EndHorizontal)
                {
                    if (Horizontal[i].Type == 8)
                    {
                        Rou += Horizontal[i].ctp1 * CalculateEasedCurve((Tuner.ChartTime - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                        Theta += Horizontal[i].ctp * CalculateEasedCurve((Tuner.ChartTime - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                    }
                    else if (Horizontal[i].Type == 11)
                    {
                        StartX = -Rou * Mathf.Cos(Theta * Mathf.Deg2Rad);
                        StartZ = Rou * Mathf.Sin(Theta * Mathf.Deg2Rad);
                        EndX = -Horizontal[i].ctp1 * Mathf.Cos(Horizontal[i].ctp * Mathf.Deg2Rad);
                        EndZ = Horizontal[i].ctp1 * Mathf.Sin(Horizontal[i].ctp * Mathf.Deg2Rad);
                        DeltaX = EndX - StartX;
                        DeltaZ = EndZ - StartZ;
                        ResultX = StartX + DeltaX * CalculateEasedCurve((Tuner.ChartTime - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                        ResultZ = StartZ + DeltaZ * CalculateEasedCurve((Tuner.ChartTime - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                        Rou = Mathf.Sqrt((ResultX * ResultX + ResultZ * ResultZ));
                        Theta = 180 - Mathf.Atan2(ResultZ, ResultX) * Mathf.Rad2Deg;
                    }
                }
                else if (i != EndHorizontal)
                {
                    if (Horizontal[i].Type == 8)
                    {
                        if ((Horizontal[i + 1].Time - Horizontal[i].Time < Horizontal[i].Duration))
                        {
                            Rou += Horizontal[i].ctp1 * CalculateEasedCurve((Horizontal[i + 1].Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                            Theta += Horizontal[i].ctp * CalculateEasedCurve((Horizontal[i + 1].Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                        }
                        else
                        {
                            Rou += Horizontal[i].ctp1;
                            Theta += Horizontal[i].ctp;
                        }
                    }
                    else if (Horizontal[i].Type == 11)
                    {
                        if ((Horizontal[i + 1].Time - Horizontal[i].Time < Horizontal[i].Duration))
                        {
                            StartX = -Rou * Mathf.Cos(Theta * Mathf.Deg2Rad);
                            StartZ = Rou * Mathf.Sin(Theta * Mathf.Deg2Rad);
                            EndX = -Horizontal[i].ctp1 * Mathf.Cos(Horizontal[i].ctp * Mathf.Deg2Rad);
                            EndZ = Horizontal[i].ctp1 * Mathf.Sin(Horizontal[i].ctp * Mathf.Deg2Rad);
                            DeltaX = EndX - StartX;
                            DeltaZ = EndZ - StartZ;
                            ResultX = StartX + DeltaX * CalculateEasedCurve((Horizontal[i + 1].Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                            ResultZ = StartZ + DeltaZ * CalculateEasedCurve((Horizontal[i + 1].Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                            Rou = Mathf.Sqrt((ResultX * ResultX + ResultZ * ResultZ));
                            Theta = 180 - Mathf.Atan2(ResultZ, ResultX) * Mathf.Rad2Deg;
                        }
                        else
                        {
                            Rou = Horizontal[i].ctp1;
                            Theta = Horizontal[i].ctp;
                        }
                    }
                    float TmpEndX = -Rou * Mathf.Cos(Theta * Mathf.Deg2Rad);
                    float TmpEndZ = Rou * Mathf.Sin(Theta * Mathf.Deg2Rad);
                    Rou = Mathf.Sqrt((TmpEndX * TmpEndX + TmpEndZ * TmpEndZ));
                    Theta = 180 - Mathf.Atan2(TmpEndZ, TmpEndX) * Mathf.Rad2Deg;
                }
            }
        }
        CalculatePolarToRectangle(Rou, Theta);
    }
    private void UpdateCameraVertical()
    {
        int EndVertical = -1;
        CurrentVerticalY = Default.CamHeight;
        for (int i = 0; i < Vertical.Count - 1; ++i)
        {
            if (Tuner.ChartTime >= Vertical[i].Time && Tuner.ChartTime < Vertical[i + 1].Time)
            {
                EndVertical = i;
                break;
            }
        }
        if (Vertical.Count != 0)
            if (Tuner.ChartTime >= Vertical[Vertical.Count - 1].Time) EndVertical = Vertical.Count - 1;
        for (int i = -1; i <= EndVertical; ++i)
        {
            if (EndVertical == -1) return;
            else if (i != -1)
            {
                if (i == EndVertical) CurrentVerticalY += Vertical[i].ctp * CalculateEasedCurve((Tuner.ChartTime - Vertical[i].Time) / Vertical[i].Duration, Vertical[i].cfmi);
                else if (i != EndVertical)
                {
                    if (Vertical[i + 1].Time - Vertical[i].Time < Vertical[i].Duration)
                        CurrentVerticalY += Vertical[i].ctp * CalculateEasedCurve((Vertical[i + 1].Time - Vertical[i].Time) / Vertical[i].Duration, Vertical[i].cfmi);
                    else CurrentVerticalY += Vertical[i].ctp;
                }
            }
        }
    }
    private void UpdateCameraStatus()
    {
        TunerCamera.transform.position = new Vector3(CurrentHorizontalX, CurrentVerticalY, CurrentHorizontalZ);
        TunerGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, CurrentRotation, 0));
    }

    public float CalculateCameraRotation(float Time)
    {
        int EndRotation = -1;
        float CurrentRotation = -Default.Degree;
        for (int i = 0; i < Rotation.Count - 1; ++i)
        {
            if (Time >= Rotation[i].Time && Time < Rotation[i + 1].Time)
            {
                EndRotation = i;
                break;
            }
        }
        if (Rotation.Count != 0)
            if (Time >= Rotation[Rotation.Count - 1].Time) EndRotation = Rotation.Count - 1;
        for (int i = -1; i <= EndRotation; ++i)
        {
            if (EndRotation == -1)
            {
                return CurrentRotation;
            }
            else if (i != -1)
            {
                if (i == EndRotation) CurrentRotation += Rotation[i].ctp * CalculateEasedCurve((Time - Rotation[i].Time) / Rotation[i].Duration, Rotation[i].cfmi);
                else if (i != EndRotation)
                {
                    if (Rotation[i + 1].Time - Rotation[i].Time < Rotation[i].Duration)
                        CurrentRotation += Rotation[i].ctp * CalculateEasedCurve((Rotation[i + 1].Time - Rotation[i].Time) / Rotation[i].Duration, Rotation[i].cfmi);
                    else CurrentRotation += Rotation[i].ctp;
                }
            }
        }
        return CurrentRotation;
    }
    public float CalculateCameraVertical(float Time)
    {
        int EndVertical = -1;
        float CurrentVerticalY = Default.CamHeight;
        for (int i = 0; i < Vertical.Count - 1; ++i)
        {
            if (Time >= Vertical[i].Time && Time < Vertical[i + 1].Time)
            {
                EndVertical = i;
                break;
            }
        }
        if (Vertical.Count != 0)
            if (Time >= Vertical[Vertical.Count - 1].Time) EndVertical = Vertical.Count - 1;
        for (int i = -1; i <= EndVertical; ++i)
        {
            if (EndVertical == -1) return CurrentVerticalY;
            else if (i != -1)
            {
                if (i == EndVertical) CurrentVerticalY += Vertical[i].ctp * CalculateEasedCurve((Time - Vertical[i].Time) / Vertical[i].Duration, Vertical[i].cfmi);
                else if (i != EndVertical)
                {
                    if (Vertical[i + 1].Time - Vertical[i].Time < Vertical[i].Duration)
                        CurrentVerticalY += Vertical[i].ctp * CalculateEasedCurve((Vertical[i + 1].Time - Vertical[i].Time) / Vertical[i].Duration, Vertical[i].cfmi);
                    else CurrentVerticalY += Vertical[i].ctp;
                }
            }
        }
        return CurrentVerticalY;
    }
    public void CalculateCameraHorizontal(float Time, out float ResultRou, out float ResultTheta)
    {
        int EndHorizontal = -1;
        float Rou = Default.CamRou, Theta = Default.CamTheta;
        float StartX, StartZ, EndX, EndZ, DeltaX, DeltaZ, ResultX, ResultZ;
        for (int i = 0; i < Horizontal.Count - 1; ++i)
        {
            if (Time >= Horizontal[i].Time && Time < Horizontal[i + 1].Time)
            {
                EndHorizontal = i;
                break;
            }
        }
        if (Horizontal.Count != 0)
            if (Time >= Horizontal[Horizontal.Count - 1].Time) EndHorizontal = Horizontal.Count - 1;
        for (int i = -1; i <= EndHorizontal; ++i)
        {
            if (EndHorizontal == -1)
            {
                ResultRou = Rou;
                ResultTheta = Theta;
                return;
            }
            else if (i != -1)
            {
                if (i == EndHorizontal)
                {
                    if (Horizontal[i].Type == 8)
                    {
                        Rou += Horizontal[i].ctp1 * CalculateEasedCurve((Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                        Theta += Horizontal[i].ctp * CalculateEasedCurve((Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                    }
                    else if (Horizontal[i].Type == 11)
                    {
                        StartX = -Rou * Mathf.Cos(Theta * Mathf.Deg2Rad);
                        StartZ = Rou * Mathf.Sin(Theta * Mathf.Deg2Rad);
                        EndX = -Horizontal[i].ctp1 * Mathf.Cos(Horizontal[i].ctp * Mathf.Deg2Rad);
                        EndZ = Horizontal[i].ctp1 * Mathf.Sin(Horizontal[i].ctp * Mathf.Deg2Rad);
                        DeltaX = EndX - StartX;
                        DeltaZ = EndZ - StartZ;
                        ResultX = StartX + DeltaX * CalculateEasedCurve((Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                        ResultZ = StartZ + DeltaZ * CalculateEasedCurve((Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                        Rou = Mathf.Sqrt((ResultX * ResultX + ResultZ * ResultZ));
                        Theta = 180 - Mathf.Atan2(ResultZ, ResultX) * Mathf.Rad2Deg;
                    }
                }
                else if (i != EndHorizontal)
                {
                    if (Horizontal[i].Type == 8)
                    {
                        if ((Horizontal[i + 1].Time - Horizontal[i].Time < Horizontal[i].Duration))
                        {
                            Rou += Horizontal[i].ctp1 * CalculateEasedCurve((Horizontal[i + 1].Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                            Theta += Horizontal[i].ctp * CalculateEasedCurve((Horizontal[i + 1].Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                        }
                        else
                        {
                            Rou += Horizontal[i].ctp1;
                            Theta += Horizontal[i].ctp;
                        }
                    }
                    else if (Horizontal[i].Type == 11)
                    {
                        if ((Horizontal[i + 1].Time - Horizontal[i].Time < Horizontal[i].Duration))
                        {
                            StartX = -Rou * Mathf.Cos(Theta * Mathf.Deg2Rad);
                            StartZ = Rou * Mathf.Sin(Theta * Mathf.Deg2Rad);
                            EndX = -Horizontal[i].ctp1 * Mathf.Cos(Horizontal[i].ctp * Mathf.Deg2Rad);
                            EndZ = Horizontal[i].ctp1 * Mathf.Sin(Horizontal[i].ctp * Mathf.Deg2Rad);
                            DeltaX = EndX - StartX;
                            DeltaZ = EndZ - StartZ;
                            ResultX = StartX + DeltaX * CalculateEasedCurve((Horizontal[i + 1].Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                            ResultZ = StartZ + DeltaZ * CalculateEasedCurve((Horizontal[i + 1].Time - Horizontal[i].Time) / Horizontal[i].Duration, Horizontal[i].cfmi);
                            Rou = Mathf.Sqrt((ResultX * ResultX + ResultZ * ResultZ));
                            Theta = 180 - Mathf.Atan2(ResultZ, ResultX) * Mathf.Rad2Deg;
                        }
                        else
                        {
                            Rou = Horizontal[i].ctp1;
                            Theta = Horizontal[i].ctp;
                        }
                    }
                }
            }
        }
        ResultRou = Rou;
        ResultTheta = Theta;
    }

    public void SortHorizontalList()
    {
        Horizontal.Sort((Lanotalium.Chart.LanotaCameraXZ A, Lanotalium.Chart.LanotaCameraXZ B) =>
        {
            return A.Time.CompareTo(B.Time);
        });
    }
    public void SortVerticalList()
    {
        Vertical.Sort((Lanotalium.Chart.LanotaCameraY A, Lanotalium.Chart.LanotaCameraY B) =>
        {
            return A.Time.CompareTo(B.Time);
        });
    }
    public void SortRotationList()
    {
        Rotation.Sort((Lanotalium.Chart.LanotaCameraRot A, Lanotalium.Chart.LanotaCameraRot B) =>
        {
            return A.Time.CompareTo(B.Time);
        });
    }
}
