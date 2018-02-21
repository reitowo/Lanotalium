using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LimGizmoMotionManager : MonoBehaviour
{
    public Text LabelText, MotionToCreateText;
    public Text BeginRadiusText, BeginDegreeText, BeginHeightText, BeginRotationText;
    public Text EditModeText, FromText, ToText, RadiusText, DegreeText, HeightText, RotationText, TimeText, WithText, EaseText, DurationText, ApplyText;
    public Color PressedColor, UnPressedColor;
    public Color ValidColor, InvalidColor;
    public Image Tp8Btn, Tp10Btn, Tp11Btn, Tp13Btn, MoveBtn, RotateBtn, InputBtn;
    public Image RadImg, DegImg, HeiImg, RotImg, TimeStartImg, TimeEndImg, DurationImg, EaseImg;
    public InputField RadiusInput, DegreeInput, HeightInput, RotationInput, TimeStartInput, TimeEndInput, DurationInput, EaseInput;
    public RectTransform TunerWindowRect;
    public GameObject TunerGameObject;
    public Camera TunerCamera;
    public LineRenderer MotionPreviewer;
    public LimTunerManager TunerManager;
    public LimCameraManager CameraManager;
    public LimOperationManager OperationManager;

    private void OnDisable()
    {
        ResetGizmoMotionEditor();
    }
    public bool IsOpen
    {
        get
        {
            return gameObject.activeInHierarchy;
        }
    }
    public bool UseType8
    {
        get
        {
            return _Tp8;
        }
        set
        {
            _Tp8 = value;
            if (value)
            {
                if (UseType11) UseType11 = false;
                Tp8Btn.color = PressedColor;
                if (UseInput)
                {
                    RadiusInput.interactable = true;
                    DegreeInput.interactable = true;
                }
            }
            else
            {
                Tp8Btn.color = UnPressedColor;
                RadiusInput.interactable = false;
                DegreeInput.interactable = false;
                if (_Mode == Lanotalium.Editor.GizmoMotionMode.Horizontal) UseType11 = true;
            }
        }
    }
    public bool UseType10
    {
        get
        {
            return _Tp10;
        }
        set
        {
            _Tp10 = value;
            if (value)
            {
                Tp10Btn.color = PressedColor;
                if (UseInput)
                {
                    HeightInput.interactable = true;
                }
            }
            else
            {
                Tp10Btn.color = UnPressedColor;
                HeightInput.interactable = false;
            }
        }
    }
    public bool UseType11
    {
        get
        {
            return _Tp11;
        }
        set
        {
            _Tp11 = value;
            if (value)
            {
                if (UseType8) UseType8 = false;
                Tp11Btn.color = PressedColor;
                if (UseInput)
                {
                    RadiusInput.interactable = true;
                    DegreeInput.interactable = true;
                }
            }
            else
            {
                Tp11Btn.color = UnPressedColor;
                RadiusInput.interactable = false;
                DegreeInput.interactable = false;
                if (_Mode == Lanotalium.Editor.GizmoMotionMode.Horizontal) UseType8 = true;
            }
        }
    }
    public bool UseType13
    {
        get
        {
            return _Tp13;
        }
        set
        {
            _Tp13 = value;
            if (value)
            {
                Tp13Btn.color = PressedColor;
                if (UseInput)
                {
                    RotationInput.interactable = true;
                }
            }
            else
            {
                Tp13Btn.color = UnPressedColor;
                RotationInput.interactable = false;
            }
        }
    }
    public bool UseMove
    {
        get
        {
            return _ModeMove;
        }
        set
        {
            _ModeMove = value;
            if (value)
            {
                UseRotate = false;
                UseInput = false;
                _UsingMode = MoveModeUpdate;
                MoveBtn.color = PressedColor;
                if (UseType8 || UseType11) RadiusInput.interactable = false;
                if (UseType8 || UseType11) DegreeInput.interactable = false;
                if (UseType10) HeightInput.interactable = false;
                if (UseType13) RotationInput.interactable = false;
                CameraManager.DisableCameraUpdate = true;
            }
            else
            {
                _UsingMode = null;
                MoveBtn.color = UnPressedColor;
                CameraManager.DisableCameraUpdate = false;
            }
        }
    }
    public bool UseRotate
    {
        get
        {
            return _ModeRotate;
        }
        set
        {
            _ModeRotate = value;
            if (value)
            {
                UseMove = false;
                UseInput = false;
                _UsingMode = RotateModeUpdate;
                RotateBtn.color = PressedColor;
                if (UseType8 || UseType11) RadiusInput.interactable = false;
                if (UseType8 || UseType11) DegreeInput.interactable = false;
                if (UseType10) HeightInput.interactable = false;
                if (UseType13) RotationInput.interactable = false;
                CameraManager.DisableCameraUpdate = true;
            }
            else
            {
                _UsingMode = null;
                RotateBtn.color = UnPressedColor;
                CameraManager.DisableCameraUpdate = false;
            }
        }
    }
    public bool UseInput
    {
        get
        {
            return _ModeInput;
        }
        set
        {
            _ModeInput = value;
            if (value)
            {
                UseMove = false;
                UseRotate = false;
                _UsingMode = InputModeUpdate;
                InputBtn.color = PressedColor;
                if (UseType8 || UseType11) RadiusInput.interactable = true;
                if (UseType8 || UseType11) DegreeInput.interactable = true;
                if (UseType10) HeightInput.interactable = true;
                if (UseType13) RotationInput.interactable = true;
                CameraManager.DisableCameraUpdate = true;
            }
            else
            {
                _UsingMode = null;
                InputBtn.color = UnPressedColor;
                if (UseType8 || UseType11) RadiusInput.interactable = false;
                if (UseType8 || UseType11) DegreeInput.interactable = false;
                if (UseType10) HeightInput.interactable = false;
                if (UseType13) RotationInput.interactable = false;
                CameraManager.DisableCameraUpdate = false;
            }
        }
    }
    public bool EditMode;

    private bool _Tp8, _Tp10, _Tp11, _Tp13, _ModeMove, _ModeRotate, _ModeInput = true, _IsPointerinTunerWindow = false;
    private Lanotalium.Editor.GizmoEditMode _UsingMode = null;
    private Vector3 _LastMousePosition;
    private float _TimeToCreate;
    private Lanotalium.Editor.GizmoMotionMode _Mode = Lanotalium.Editor.GizmoMotionMode.Create;
    private Lanotalium.Chart.LanotaCameraXZ _HorReference;
    private Lanotalium.Chart.LanotaCameraY _VerReference;
    private Lanotalium.Chart.LanotaCameraRot _RotReference;

    private void Update()
    {
        if (LimSystem.ChartContainer == null || !TunerManager.isInitialized) return;
        if (_UsingMode != null) _UsingMode();
        if (!UseInput) SetTunerStatusToInputField();
        SetStartStatusToText();
        SetTimeToInputField();
        SetMotionPreviewer();
        _LastMousePosition = Input.mousePosition;
    }

    public void ResetGizmoMotionEditor()
    {
        CameraManager.DisableCameraUpdate = false;
        if (MotionPreviewer != null) MotionPreviewer.positionCount = 0;
        _Mode = Lanotalium.Editor.GizmoMotionMode.Idle;
        UseType8 = false;
        UseType10 = false;
        UseType11 = false;
        UseType13 = false;
        UseMove = false;
        UseRotate = false;
        UseInput = false;
    }
    public void SetTexts()
    {
        LabelText.text = LimLanguageManager.TextDict["GizmoMotion_Label"];
        MotionToCreateText.text = LimLanguageManager.TextDict["GizmoMotion_ToCreate"];
        EditModeText.text = LimLanguageManager.TextDict["GizmoMotion_EditMode"];
        FromText.text = LimLanguageManager.TextDict["GizmoMotion_From"];
        ToText.text = LimLanguageManager.TextDict["GizmoMotion_To"];
        RadiusText.text = LimLanguageManager.TextDict["GizmoMotion_Radius"];
        DegreeText.text = LimLanguageManager.TextDict["GizmoMotion_Degree"];
        HeightText.text = LimLanguageManager.TextDict["GizmoMotion_Height"];
        RotationText.text = LimLanguageManager.TextDict["GizmoMotion_Rotation"];
        TimeText.text = LimLanguageManager.TextDict["GizmoMotion_Time"];
        WithText.text = LimLanguageManager.TextDict["GizmoMotion_With"];
        EaseText.text = LimLanguageManager.TextDict["GizmoMotion_Ease"];
        DurationText.text = LimLanguageManager.TextDict["GizmoMotion_Duration"];
        ApplyText.text = LimLanguageManager.TextDict["GizmoMotion_Apply"];
    }
    public void OnClickTp8()
    {
        if (EditMode && _Mode != Lanotalium.Editor.GizmoMotionMode.Horizontal) return;
        if (UseType8) UseType8 = false;
        else UseType8 = true;
    }
    public void OnClickTp10()
    {
        if (EditMode) return;
        if (UseType10) UseType10 = false;
        else UseType10 = true;
    }
    public void OnClickTp11()
    {
        if (EditMode && _Mode != Lanotalium.Editor.GizmoMotionMode.Horizontal) return;
        if (UseType11) UseType11 = false;
        else UseType11 = true;
    }
    public void OnClickTp13()
    {
        if (EditMode) return;
        if (UseType13) UseType13 = false;
        else UseType13 = true;
    }
    public void OnClickMoveMode()
    {
        if (UseMove) UseMove = false;
        else UseMove = true;
    }
    public void OnClickRotateMode()
    {
        if (UseRotate) UseRotate = false;
        else UseRotate = true;
    }
    public void OnClickInputMode()
    {
        if (UseInput) UseInput = false;
        else UseInput = true;
    }

    public void MoveModeUpdate()
    {
        if (!_IsPointerinTunerWindow) return;
        Vector3 Old = TunerCamera.transform.position;
        if (UseType10) TunerCamera.transform.position = new Vector3(Old.x, Old.y + Input.GetAxis("Mouse ScrollWheel") * (Input.GetKey(KeyCode.LeftControl) ? 50 : 5), Old.z);
        if (Input.GetMouseButton(0) && (UseType8 || UseType11))
        {
            Vector3 MousePosition = Input.mousePosition;
            Vector3 WorldMousePosition = TunerCamera.ScreenToWorldPoint(new Vector3(MousePosition.x, MousePosition.y, -Old.y));
            Vector3 WorldLastMousePosition = TunerCamera.ScreenToWorldPoint(new Vector3(_LastMousePosition.x, _LastMousePosition.y, -Old.y));
            Vector3 Delta = WorldLastMousePosition - WorldMousePosition;
            TunerCamera.transform.position = new Vector3(Old.x + Delta.x, Old.y, Old.z + Delta.z);
        }
        SetTunerStatusToInputField();
    }
    public void RotateModeUpdate()
    {
        if (!_IsPointerinTunerWindow) return;
        Vector3 Old = TunerGameObject.transform.rotation.eulerAngles;
        if (Input.GetMouseButton(0))
        {
            Vector3 CenterPosition = TunerCamera.WorldToScreenPoint(TunerGameObject.transform.position);
            CenterPosition.z = 0;
            Vector3 MousePosition = Input.mousePosition;
            MousePosition.x -= TunerWindowRect.anchoredPosition.x;
            MousePosition.y -= (Screen.height - TunerWindowRect.anchoredPosition.y - TunerWindowRect.sizeDelta.y);
            _LastMousePosition.x -= TunerWindowRect.anchoredPosition.x;
            _LastMousePosition.y -= (Screen.height - TunerWindowRect.anchoredPosition.y - TunerWindowRect.sizeDelta.y);
            float Angle = Vector3.Angle(MousePosition - CenterPosition, _LastMousePosition - CenterPosition);
            bool Clockwise = Vector3.Cross(MousePosition - CenterPosition, _LastMousePosition - CenterPosition).z > 0 ? true : false;
            TunerGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, Old.y + (Clockwise ? -1 : 1) * Angle * (Input.GetKey(KeyCode.LeftControl) ? 3 : 1), 0));
            CameraManager.CurrentRotation = TunerGameObject.transform.rotation.eulerAngles.y;
            _LastMousePosition.x += TunerWindowRect.anchoredPosition.x;
            _LastMousePosition.y += (Screen.height - TunerWindowRect.anchoredPosition.y - TunerWindowRect.sizeDelta.y);
        }
        SetTunerStatusToInputField();
    }
    public void InputModeUpdate()
    {
        float Rad = 0, Deg = 0, Hei = 0, Rot = 0;
        if (UseType8 || UseType11)
        {
            if (!float.TryParse(RadiusInput.text, out Rad))
            { RadImg.color = InvalidColor; return; }
            else RadImg.color = ValidColor;
            if (!float.TryParse(DegreeInput.text, out Deg))
            { DegImg.color = InvalidColor; return; }
            else DegImg.color = ValidColor;
        }
        else
        {
            if (!float.TryParse(BeginRadiusText.text, out Rad)) return;
            if (!float.TryParse(BeginDegreeText.text, out Deg)) return;
        }
        if (UseType10)
        {
            if (!float.TryParse(HeightInput.text, out Hei))
            { HeiImg.color = InvalidColor; return; }
            else HeiImg.color = ValidColor;
        }
        else
        {
            if (!float.TryParse(BeginHeightText.text, out Hei)) return;
        }
        if (UseType13)
        {
            if (!float.TryParse(RotationInput.text, out Rot))
            { RotImg.color = InvalidColor; return; }
            else RotImg.color = ValidColor;
        }
        else
        {
            if (!float.TryParse(BeginRotationText.text, out Rot)) return;
        }
        TunerCamera.transform.position = new Vector3(-Rad * Mathf.Cos(Deg * Mathf.Deg2Rad), Hei, Rad * Mathf.Sin(Deg * Mathf.Deg2Rad));
        TunerGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, Rot, 0));
    }
    private void SetTunerStatusToInputField()
    {
        if (!UseInput && !UseMove && !UseRotate) return;
        if (UseMove)
        {
            Vector3 Position = TunerCamera.transform.position;
            RadiusInput.text = Mathf.Sqrt(Position.x * Position.x + Position.z * Position.z).ToString();
            DegreeInput.text = (180f - Mathf.Atan2(Position.z, Position.x) * Mathf.Rad2Deg).ToString();
            HeightInput.text = Position.y.ToString();
        }
        if (UseRotate)
        {
            Vector3 Rotation = TunerGameObject.transform.rotation.eulerAngles;
            RotationInput.text = Rotation.y.ToString();
        }
    }
    private void SetStartStatusToText()
    {
        float Rou, Theta;
        CameraManager.CalculateCameraHorizontal(_TimeToCreate, out Rou, out Theta);
        float Height = CameraManager.CalculateCameraVertical(_TimeToCreate);
        float Rotation = CameraManager.CalculateCameraRotation(_TimeToCreate);
        BeginRadiusText.text = Rou.ToString();
        BeginDegreeText.text = Theta.ToString();
        BeginHeightText.text = Height.ToString();
        BeginRotationText.text = Rotation.ToString();
    }
    private void SetTimeToInputField()
    {
        if (!TunerManager.MediaPlayerManager.IsPlaying) return;
        TimeStartInput.text = TunerManager.ChartTime.ToString();
    }
    private void SetMotionPreviewer()
    {
        if (TunerManager.MediaPlayerManager.IsPlaying)
        {
            MotionPreviewer.positionCount = 0;
            return;
        }
        if (UseType8)
        {
            float StartT, StartR, StartH, EndT, EndR, EndH, dT, dR, dH;
            if (!float.TryParse(BeginRadiusText.text, out StartR)) return;
            if (!float.TryParse(BeginDegreeText.text, out StartT)) return;
            if (!float.TryParse(BeginHeightText.text, out StartH)) return;
            if (!float.TryParse(RadiusInput.text, out EndR)) return;
            if (!float.TryParse(DegreeInput.text, out EndT)) return;
            if (!float.TryParse(HeightInput.text, out EndH)) return;
            dT = (EndT - StartT) / 199;
            dR = (EndR - StartR) / 199;
            dH = (EndH - StartH) / 199;
            MotionPreviewer.positionCount = 200;
            for (int i = 0; i < 200; ++i)
            {
                MotionPreviewer.SetPosition(i, new Vector3(-(StartR + dR * i) * Mathf.Cos((StartT + dT * i) / 180 * Mathf.PI), -(StartR + dR * i) * Mathf.Sin((StartT + dT * i) / 180 * Mathf.PI), -(StartH + dH * i)));
            }
        }
        else if (UseType11 || UseType10)
        {
            float StartT, StartR, StartH, EndT, EndR, EndH;
            if (!float.TryParse(BeginRadiusText.text, out StartR)) return;
            if (!float.TryParse(BeginDegreeText.text, out StartT)) return;
            if (!float.TryParse(BeginHeightText.text, out StartH)) return;
            if (!float.TryParse(RadiusInput.text, out EndR)) return;
            if (!float.TryParse(DegreeInput.text, out EndT)) return;
            if (!float.TryParse(HeightInput.text, out EndH)) return;
            Vector3 Start = new Vector3(-StartR * Mathf.Cos(StartT * Mathf.Deg2Rad), -StartR * Mathf.Sin(StartT * Mathf.Deg2Rad), -StartH);
            Vector3 End = new Vector3(-EndR * Mathf.Cos(EndT * Mathf.Deg2Rad), -EndR * Mathf.Sin(EndT * Mathf.Deg2Rad), -EndH);
            MotionPreviewer.positionCount = 2;
            MotionPreviewer.SetPosition(0, Start);
            MotionPreviewer.SetPosition(1, End);
        }
    }
    public void OnDurationChange()
    {
        float Duration;
        if (!float.TryParse(DurationInput.text, out Duration))
        {
            DurationImg.color = InvalidColor;
            return;
        }
        if (Duration < 0.0001f)
        { DurationImg.color = InvalidColor; return; }
        else DurationImg.color = ValidColor;
        DurationImg.color = ValidColor;
        TimeEndInput.text = (_TimeToCreate + Duration).ToString();
    }
    public void OnTimeStartChange()
    {
        if (LimSystem.ChartContainer == null || !TunerManager.isInitialized) return;
        float TimeTmp;
        if (!float.TryParse(TimeStartInput.text, out TimeTmp))
        {
            TimeStartImg.color = InvalidColor;
            return;
        }
        if (TimeTmp < 0 || TimeTmp > TunerManager.MediaPlayerManager.Length)
        {
            TimeStartImg.color = InvalidColor;
            return;
        }
        TimeStartImg.color = ValidColor;
        _TimeToCreate = TimeTmp;
        OnDurationChange();
        if (TunerManager.MediaPlayerManager.IsPlaying) TunerManager.MediaPlayerManager.Time = TimeTmp;
    }
    public void OnTimeEndChange()
    {
        if (LimSystem.ChartContainer == null || !TunerManager.isInitialized) return;
        float TimeTmp;
        if (!float.TryParse(TimeEndInput.text, out TimeTmp))
        {
            TimeEndImg.color = InvalidColor;
            return;
        }
        if (TimeTmp < _TimeToCreate + 0.0001f)
        {
            TimeEndImg.color = InvalidColor;
            return;
        }
        TimeEndImg.color = ValidColor;
        DurationInput.text = (TimeTmp - _TimeToCreate).ToString();
    }
    public void OnPointerEnterTunerWindow()
    {
        _IsPointerinTunerWindow = true;
    }
    public void OnPointerExitTunerWindow()
    {
        _IsPointerinTunerWindow = false;
    }

    public void Create()
    {
        ResetGizmoMotionEditor();
        _Mode = Lanotalium.Editor.GizmoMotionMode.Create;
        EditMode = false;
        UseType8 = false;
        UseType10 = false;
        UseType11 = false;
        UseType13 = false;
        UseMove = false;
        UseRotate = false;
        UseInput = true;
        TimeStartInput.text = TunerManager.ChartTime.ToString();
        _TimeToCreate = TunerManager.ChartTime;
        SetStartStatusToText();
        DurationInput.text = 1.ToString();
        EaseInput.text = 0.ToString();
        SetTunerStatusToInputField();
        gameObject.SetActive(true);
        OnDurationChange();
    }
    public void Edit(Lanotalium.Chart.LanotaCameraXZ Hor)
    {
        ResetGizmoMotionEditor();
        _Mode = Lanotalium.Editor.GizmoMotionMode.Horizontal;
        EditMode = true;
        _HorReference = Hor;
        if (Hor.Type == 8) UseType8 = true;
        else UseType8 = false;
        UseType10 = false;
        if (Hor.Type == 11) UseType11 = true;
        else UseType11 = false;
        UseType13 = false;
        UseMove = false;
        UseRotate = false;
        UseInput = true;
        TimeStartInput.text = Hor.Time.ToString();
        _TimeToCreate = Hor.Time;
        SetStartStatusToText();
        DurationInput.text = Hor.Duration.ToString();
        TimeEndInput.text = (_TimeToCreate + Hor.Duration).ToString();
        EaseInput.text = Hor.cfmi.ToString();
        if (UseType8)
        {
            float BeginRadius, BeginDegree;
            if (!float.TryParse(BeginRadiusText.text, out BeginRadius)) return;
            if (!float.TryParse(BeginDegreeText.text, out BeginDegree)) return;
            RadiusInput.text = (BeginRadius + Hor.ctp1).ToString();
            DegreeInput.text = (BeginDegree + Hor.ctp).ToString();
        }
        else if (UseType11)
        {
            RadiusInput.text = Hor.ctp1.ToString();
            DegreeInput.text = Hor.ctp.ToString();
        }
        gameObject.SetActive(true);
    }
    public void Edit(Lanotalium.Chart.LanotaCameraY Ver)
    {
        ResetGizmoMotionEditor();
        _Mode = Lanotalium.Editor.GizmoMotionMode.Vertical;
        EditMode = true;
        _VerReference = Ver;
        UseType8 = false;
        UseType10 = true;
        UseType11 = false;
        UseType13 = false;
        UseMove = false;
        UseRotate = false;
        UseInput = true;
        TimeStartInput.text = Ver.Time.ToString();
        _TimeToCreate = Ver.Time;
        SetStartStatusToText();
        DurationInput.text = Ver.Duration.ToString();
        TimeEndInput.text = (_TimeToCreate + Ver.Duration).ToString();
        EaseInput.text = Ver.cfmi.ToString();
        float BeginHeight;
        if (!float.TryParse(BeginHeightText.text, out BeginHeight)) return;
        HeightInput.text = (BeginHeight + Ver.ctp).ToString();
        gameObject.SetActive(true);
    }
    public void Edit(Lanotalium.Chart.LanotaCameraRot Rot)
    {
        ResetGizmoMotionEditor();
        _Mode = Lanotalium.Editor.GizmoMotionMode.Rotation;
        EditMode = true;
        _RotReference = Rot;
        UseType8 = false;
        UseType10 = false;
        UseType11 = false;
        UseType13 = true;
        UseMove = false;
        UseRotate = false;
        UseInput = true;
        TimeStartInput.text = Rot.Time.ToString();
        _TimeToCreate = Rot.Time;
        SetStartStatusToText();
        DurationInput.text = Rot.Duration.ToString();
        TimeEndInput.text = (_TimeToCreate + Rot.Duration).ToString();
        EaseInput.text = Rot.cfmi.ToString();
        float BeginRotation;
        if (!float.TryParse(BeginRotationText.text, out BeginRotation)) return;
        RotationInput.text = (BeginRotation + Rot.ctp).ToString();
        gameObject.SetActive(true);
    }
    public void Apply()
    {
        int Ease;
        float Rad = 0, Deg = 0, Hei = 0, Rot = 0, Time = 0, Duration = 0;
        float StartT, StartR, StartH, StartRotation;
        if (!float.TryParse(BeginRadiusText.text, out StartR)) return;
        if (!float.TryParse(BeginDegreeText.text, out StartT)) return;
        if (!float.TryParse(BeginHeightText.text, out StartH)) return;
        if (!float.TryParse(BeginRotationText.text, out StartRotation)) return;
        if (UseType8 || UseType11)
        {
            if (!float.TryParse(RadiusInput.text, out Rad))
            { RadImg.color = InvalidColor; return; }
            else RadImg.color = ValidColor;
            if (!float.TryParse(DegreeInput.text, out Deg))
            { DegImg.color = InvalidColor; return; }
            else DegImg.color = ValidColor;
        }
        if (UseType10)
        {
            if (!float.TryParse(HeightInput.text, out Hei))
            { HeiImg.color = InvalidColor; return; }
            else HeiImg.color = ValidColor;
        }
        if (UseType13)
        {
            if (!float.TryParse(RotationInput.text, out Rot))
            { RotImg.color = InvalidColor; return; }
            else RotImg.color = ValidColor;
        }
        if (!float.TryParse(TimeStartInput.text, out Time))
        { TimeStartImg.color = InvalidColor; return; }
        else TimeStartImg.color = ValidColor;
        if (!float.TryParse(DurationInput.text, out Duration))
        { DurationImg.color = InvalidColor; return; }
        else DurationImg.color = ValidColor;
        if (Duration < 0.0001f)
        { DurationImg.color = InvalidColor; return; }
        else DurationImg.color = ValidColor;
        if (!int.TryParse(EaseInput.text, out Ease))
        { EaseImg.color = InvalidColor; return; }
        else EaseImg.color = ValidColor;
        if (Ease < 0 || Ease > 12)
        { EaseImg.color = InvalidColor; return; }
        else EaseImg.color = ValidColor;
        if (EditMode)
        {
            if (_Mode == Lanotalium.Editor.GizmoMotionMode.Horizontal)
            {
                if (!OperationManager.CheckHorizontalTimeValid(_HorReference, Time))
                { TimeStartImg.color = InvalidColor; return; }
                else TimeStartImg.color = ValidColor;
                OperationManager.SetHorizontalTime(_HorReference, Time);
                if (!OperationManager.CheckHorizontalDurationValid(_HorReference, Duration))
                { DurationImg.color = InvalidColor; return; }
                else DurationImg.color = ValidColor;
                OperationManager.SetHorizontalDuration(_HorReference, Duration);
                if (UseType8)
                {
                    OperationManager.SetHorizontalDegree(_HorReference, Deg - StartT);
                    OperationManager.SetHorizontalRadius(_HorReference, Rad - StartR);
                }
                else if (UseType11)
                {
                    OperationManager.SetHorizontalDegree(_HorReference, Deg);
                    OperationManager.SetHorizontalRadius(_HorReference, Rad);
                }
                OperationManager.SetHorizontalEase(_HorReference, Ease);
                if (UseType8) OperationManager.SetHorizontalType(_HorReference, 8);
                else if (UseType11) OperationManager.SetHorizontalType(_HorReference, 11);
            }
            else if (_Mode == Lanotalium.Editor.GizmoMotionMode.Vertical)
            {
                if (!OperationManager.CheckVerticalTimeValid(_VerReference, Time))
                { TimeStartImg.color = InvalidColor; return; }
                else TimeStartImg.color = ValidColor;
                OperationManager.SetVerticalTime(_VerReference, Time);
                if (!OperationManager.CheckVerticalDurationValid(_VerReference, Duration))
                { DurationImg.color = InvalidColor; return; }
                else DurationImg.color = ValidColor;
                OperationManager.SetVerticalDuration(_VerReference, Duration);
                OperationManager.SetVerticalHeight(_VerReference, Hei - StartH);
                OperationManager.SetVerticalEase(_VerReference, Ease);
            }
            else if (_Mode == Lanotalium.Editor.GizmoMotionMode.Rotation)
            {
                if (!OperationManager.CheckRotationTimeValid(_RotReference, Time))
                { TimeStartImg.color = InvalidColor; return; }
                else TimeStartImg.color = ValidColor;
                OperationManager.SetRotationTime(_RotReference, Time);
                if (!OperationManager.CheckRotationDurationValid(_RotReference, Duration))
                { DurationImg.color = InvalidColor; return; }
                else DurationImg.color = ValidColor;
                OperationManager.SetRotationDuration(_RotReference, Duration);
                OperationManager.SetRotationDegree(_RotReference, Rot - StartRotation);
                OperationManager.SetRotationEase(_RotReference, Ease);
            }
        }
        else if (!EditMode)
        {
            if (UseType8)
            {
                Lanotalium.Chart.LanotaCameraXZ New = new Lanotalium.Chart.LanotaCameraXZ();
                New.Time = Time;
                New.Duration = Duration;
                New.ctp = Deg - StartT;
                New.ctp1 = Rad - StartR;
                New.cfmi = Ease;
                New.Type = 8;
                if (!OperationManager.AddHorizontal(New, false)) return;
            }
            if (UseType10)
            {
                Lanotalium.Chart.LanotaCameraY New = new Lanotalium.Chart.LanotaCameraY();
                New.Time = Time;
                New.Duration = Duration;
                New.ctp = Hei - StartH;
                New.cfmi = Ease;
                New.Type = 10;
                if (!OperationManager.AddVertical(New, false)) return;
            }
            if (UseType11)
            {
                Lanotalium.Chart.LanotaCameraXZ New = new Lanotalium.Chart.LanotaCameraXZ();
                New.Time = Time;
                New.Duration = Duration;
                New.ctp = Deg;
                New.ctp1 = Rad;
                New.cfmi = Ease;
                New.Type = 11;
                if (!OperationManager.AddHorizontal(New, false)) return;
            }
            if (UseType13)
            {
                Lanotalium.Chart.LanotaCameraRot New = new Lanotalium.Chart.LanotaCameraRot();
                New.Time = Time;
                New.Duration = Duration;
                New.ctp = Rot - StartRotation;
                New.cfmi = Ease;
                New.Type = 13;
                if (!OperationManager.AddRotation(New, false)) return;
            }
        }
        gameObject.SetActive(false);
    }
}
