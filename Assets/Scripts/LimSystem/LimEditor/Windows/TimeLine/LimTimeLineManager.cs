using System.Collections;
using System.Collections.Generic;
using Lanotalium.Editor;
using UnityEngine;
using UnityEngine.UI;

public class LimTimeLineManager : MonoBehaviour
{
    public LimTunerManager TunerManager;
    public LimWindowManager BaseWindow;
    public LimOperationManager OperationManager;
    public LimCameraManager CameraManager;
    public ComponentMotionManager ComponentMotion;
    public Text HorText, VerText, RotText, TimingText, WaveLText, WaveRText, WaveformText;
    public RectTransform ViewRect, ComponentRect;
    public RectTransform HorTransform, VerTransform, RotTransform, TimeTransform;
    public GameObject TimeLineObject, TimeLineTime;
    public List<TimeLineTimeContainer> TimeLineTimes = new List<TimeLineTimeContainer>();
    public Color Tp8, Tp11, Tp10, Tp13;
    public Toggle WaveformToggle;
    public LimWaveformManager WaveformManager;
    public RectTransform TimePointer;
    public float Scale;

    void Start()
    {
        Initialize();
        BaseWindow.OnWindowSorted.AddListener(OnWindowSorted);
    }
    public void Initialize()
    {
        WaveformToggle.isOn = LimSystem.Preferences.Waveform;
        OnWaveformToggle();
        if (LimSystem.ChartContainer == null) return;
        ApplyScale();
    }
    public void SetTexts()
    {
        BaseWindow.WindowName = LimLanguageManager.TextDict["Window_TimeLine_Label"];
        HorText.text = LimLanguageManager.TextDict["Window_TimeLine_Hor_Label"];
        VerText.text = LimLanguageManager.TextDict["Window_TimeLine_Ver_Label"];
        RotText.text = LimLanguageManager.TextDict["Window_TimeLine_Rot_Label"];
        WaveLText.text = LimLanguageManager.TextDict["Window_TimeLine_WaveformL"];
        WaveRText.text = LimLanguageManager.TextDict["Window_TimeLine_WaveformR"];
        WaveformText.text = LimLanguageManager.TextDict["Window_TimeLine_Waveform"];
    }
    void Update()
    {
        if (LimSystem.ChartContainer == null) return;
        if (!TunerManager.isInitialized) return;
        UpdateMotionBarActive();
        UpdateTimePointer();
        UpdateTimelineTimes();
        UpdateTimeLinePosition();
        DetectScaleChange();
        TimingText.text = TunerManager.ChartTime.ToString("f4");
    }

    public void InstantiateAllTimeLine()
    {
        InstantiateHorizontal();
        InstantiateVertical();
        InstantiateRotation();
    }
    public void InstantiateHorizontal()
    {
        foreach (Lanotalium.Chart.LanotaCameraXZ Hor in CameraManager.Horizontal)
        {
            if (Hor.TimeLineGameObject != null) Destroy(Hor.TimeLineGameObject);
            Hor.TimeLineGameObject = Instantiate(TimeLineObject, HorTransform);
            Hor.TimeLineGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(Hor.Time * Scale, 0);
            Hor.TimeLineGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Hor.Duration * Scale, 30);
            Hor.TimeLineGameObject.GetComponent<Image>().color = Hor.Type == 8 ? Tp8 : Tp11;
            Hor.TimeLineGameObject.GetComponent<Button>().onClick.AddListener(OperationManager.OnTimeLineClick);
            Hor.InstanceId = Hor.TimeLineGameObject.GetInstanceID();
        }
    }
    public void InstantiateVertical()
    {
        foreach (Lanotalium.Chart.LanotaCameraY Ver in CameraManager.Vertical)
        {
            if (Ver.TimeLineGameObject != null) Destroy(Ver.TimeLineGameObject);
            Ver.TimeLineGameObject = Instantiate(TimeLineObject, VerTransform);
            Ver.TimeLineGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(Ver.Time * Scale, 0);
            Ver.TimeLineGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Ver.Duration * Scale, 30);
            Ver.TimeLineGameObject.GetComponent<Image>().color = Tp10;
            Ver.TimeLineGameObject.GetComponent<Button>().onClick.AddListener(OperationManager.OnTimeLineClick);
            Ver.InstanceId = Ver.TimeLineGameObject.GetInstanceID();
        }
    }
    public void InstantiateRotation()
    {
        foreach (Lanotalium.Chart.LanotaCameraRot Rot in CameraManager.Rotation)
        {
            if (Rot.TimeLineGameObject != null) Destroy(Rot.TimeLineGameObject);
            Rot.TimeLineGameObject = Instantiate(TimeLineObject, RotTransform);
            Rot.TimeLineGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(Rot.Time * Scale, 0);
            Rot.TimeLineGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Rot.Duration * Scale, 30);
            Rot.TimeLineGameObject.GetComponent<Image>().color = Tp13;
            Rot.TimeLineGameObject.GetComponent<Button>().onClick.AddListener(OperationManager.OnTimeLineClick);
            Rot.InstanceId = Rot.TimeLineGameObject.GetInstanceID();
        }
    }
    public void InstantiateSingleHorizontal(Lanotalium.Chart.LanotaCameraXZ Hor)
    {
        if (Hor.TimeLineGameObject != null) Destroy(Hor.TimeLineGameObject);
        Hor.TimeLineGameObject = Instantiate(TimeLineObject, HorTransform);
        Hor.TimeLineGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(Hor.Time * Scale, 0);
        Hor.TimeLineGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Hor.Duration * Scale, 30);
        Hor.TimeLineGameObject.GetComponent<Image>().color = Hor.Type == 8 ? Tp8 : Tp11;
        Hor.TimeLineGameObject.GetComponent<Button>().onClick.AddListener(OperationManager.OnTimeLineClick);
        Hor.InstanceId = Hor.TimeLineGameObject.GetInstanceID();
    }
    public void InstantiateSingleVertical(Lanotalium.Chart.LanotaCameraY Ver)
    {
        if (Ver.TimeLineGameObject != null) Destroy(Ver.TimeLineGameObject);
        Ver.TimeLineGameObject = Instantiate(TimeLineObject, VerTransform);
        Ver.TimeLineGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(Ver.Time * Scale, 0);
        Ver.TimeLineGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Ver.Duration * Scale, 30);
        Ver.TimeLineGameObject.GetComponent<Image>().color = Tp10;
        Ver.TimeLineGameObject.GetComponent<Button>().onClick.AddListener(OperationManager.OnTimeLineClick);
        Ver.InstanceId = Ver.TimeLineGameObject.GetInstanceID();
    }
    public void InstantiateSingleRotation(Lanotalium.Chart.LanotaCameraRot Rot)
    {
        if (Rot.TimeLineGameObject != null) Destroy(Rot.TimeLineGameObject);
        Rot.TimeLineGameObject = Instantiate(TimeLineObject, RotTransform);
        Rot.TimeLineGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(Rot.Time * Scale, 0);
        Rot.TimeLineGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Rot.Duration * Scale, 30);
        Rot.TimeLineGameObject.GetComponent<Image>().color = Tp13;
        Rot.TimeLineGameObject.GetComponent<Button>().onClick.AddListener(OperationManager.OnTimeLineClick);
        Rot.InstanceId = Rot.TimeLineGameObject.GetInstanceID();
    }

    public void UpdateTimelineTimes()
    {
        float Start = TunerManager.ChartTime - 70f / Scale;
        float End = TunerManager.ChartTime + (ViewRect.sizeDelta.x - 200f) / Scale;
        foreach (TimeLineTimeContainer c in TimeLineTimes)
        {
            if (c.Timing < Start || c.Timing > End)
            {
                if (c.GameObject != null) Destroy(c.GameObject);
                continue;
            }
            if (c.GameObject == null)
            {
                c.GameObject = Instantiate(TimeLineTime, TimeTransform);
                c.GameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(c.Timing * Scale, 0);
                c.GameObject.GetComponentInChildren<Text>().text = c.Timing.ToString("f2");
            }
        }
    }
    public void UpdateTimeLinePosition()
    {
        float ScaledTime = -TunerManager.ChartTime * Scale;
        TimeTransform.anchoredPosition = new Vector2(ScaledTime, 0);
        HorTransform.anchoredPosition = new Vector2(ScaledTime, 0);
        VerTransform.anchoredPosition = new Vector2(ScaledTime, 0);
        RotTransform.anchoredPosition = new Vector2(ScaledTime, 0);
    }
    public void UpdateTimePointer()
    {
        Vector3 Mouse = LimMousePosition.MousePosition;
        if (Mouse.x >= ViewRect.anchoredPosition.x + 200 && Mouse.x <= ViewRect.anchoredPosition.x + ViewRect.sizeDelta.x)
        {
            if (Mouse.y >= ViewRect.anchoredPosition.y - ViewRect.sizeDelta.y && Mouse.y <= ViewRect.anchoredPosition.y)
            {
                if (!TimePointer.gameObject.activeInHierarchy) TimePointer.gameObject.SetActive(true);
                TimePointer.anchoredPosition = new Vector2((Mouse.x - ViewRect.anchoredPosition.x), -28);
                float PointerTiming = (Mouse.x - ViewRect.anchoredPosition.x - 200) / Scale + TunerManager.ChartTime;
                PointerTiming = Mathf.Clamp(PointerTiming, 0, TunerManager.MusicPlayerManager.Length);
                TimePointer.GetComponentInChildren<Text>().text = PointerTiming.ToString("f4");
                if (Mouse.y >= ViewRect.anchoredPosition.y - 120 && Mouse.y <= ViewRect.anchoredPosition.y)
                {
                    if (Input.GetMouseButton(0))
                    {
                        float Delta = LimMousePosition.MousePosition.x - LimMousePosition.LastMousePosition.x;
                        float DeltaTime = -Delta / Scale;
                        TunerManager.MusicPlayerManager.Time = Mathf.Clamp(TunerManager.ChartTime + DeltaTime, 0, TunerManager.MusicPlayerManager.Length);
                    }
                }
            }
            else
            {
                if (TimePointer.gameObject.activeInHierarchy) TimePointer.gameObject.SetActive(false);
            }
        }
        else
        {
            if (TimePointer.gameObject.activeInHierarchy) TimePointer.gameObject.SetActive(false);
        }
    }
    public void UpdateMotionBarActive()
    {
        float Start = TunerManager.ChartTime - 70f / Scale;
        float End = TunerManager.ChartTime + (ViewRect.sizeDelta.x - 200f) / Scale;
        foreach (Lanotalium.Chart.LanotaCameraXZ Hor in CameraManager.Horizontal)
        {
            if (Hor.TimeLineGameObject == null) continue;
            if ((Hor.Time + Hor.Duration <= Start || Hor.Time >= End) && Hor.TimeLineGameObject.activeInHierarchy)
            {
                Hor.TimeLineGameObject.SetActive(false);
            }
            else if (Hor.Time + Hor.Duration > Start && Hor.Time < End && !Hor.TimeLineGameObject.activeInHierarchy)
            {
                Hor.TimeLineGameObject.SetActive(true);
            }
        }
        foreach (Lanotalium.Chart.LanotaCameraY Ver in CameraManager.Vertical)
        {
            if (Ver.TimeLineGameObject == null) continue;
            if ((Ver.Time + Ver.Duration <= Start || Ver.Time >= End) && Ver.TimeLineGameObject.activeInHierarchy)
            {
                Ver.TimeLineGameObject.SetActive(false);
            }
            else if (Ver.Time + Ver.Duration > Start && Ver.Time < End && !Ver.TimeLineGameObject.activeInHierarchy)
            {
                Ver.TimeLineGameObject.SetActive(true);
            }
        }
        foreach (Lanotalium.Chart.LanotaCameraRot Rot in CameraManager.Rotation)
        {
            if (Rot.TimeLineGameObject == null) continue;
            if ((Rot.Time + Rot.Duration <= Start || Rot.Time >= End) && Rot.TimeLineGameObject.activeInHierarchy)
            {
                Rot.TimeLineGameObject.SetActive(false);
            }
            else if (Rot.Time + Rot.Duration > Start && Rot.Time < End && !Rot.TimeLineGameObject.activeInHierarchy)
            {
                Rot.TimeLineGameObject.SetActive(true);
            }
        }
    }
    public void ApplyScale()
    {
        foreach (TimeLineTimeContainer t in TimeLineTimes)
        {
            if (t.GameObject != null) Destroy(t.GameObject);
        }
        TimeLineTimes.Clear();
        for (float t = 0; t < TunerManager.MusicPlayerManager.Length; t += 70f / Scale)
        {
            TimeLineTimeContainer c = new TimeLineTimeContainer();
            c.Timing = t;
            TimeLineTimes.Add(c);
        }
        InstantiateAllTimeLine();
    }
    public void DetectScaleChange()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            float Scroll = Input.GetAxis("Mouse ScrollWheel") * 500;
            if (Scroll != 0)
            {
                Vector3 Mouse = LimMousePosition.MousePosition;
                if (Mouse.x >= ViewRect.anchoredPosition.x && Mouse.x <= ViewRect.anchoredPosition.x + ViewRect.sizeDelta.x)
                {
                    if (Mouse.y >= ViewRect.anchoredPosition.y - ViewRect.sizeDelta.y && Mouse.y <= ViewRect.anchoredPosition.y)
                    {
                        if (Scroll > 0)
                        {
                            if (Scale < 10000)
                            {
                                Scale += Scroll;
                                Scale = Mathf.Min(10000, Scale);
                                ApplyScale();
                            }
                        }
                        else
                        {
                            if (Scale > 10)
                            {
                                Scale += Scroll;
                                Scale = Mathf.Max(10, Scale);
                                ApplyScale();
                            }
                        }
                    }
                }
            }
        }
    }

    public void ChangeSelectedMotionTo(int Delta)
    {
        if (ComponentMotion.Mode == Lanotalium.Editor.ComponentMotionMode.Idle) return;
        else if (ComponentMotion.Mode == Lanotalium.Editor.ComponentMotionMode.Horizontal)
        {
            if (CameraManager.Horizontal == null || CameraManager.Horizontal.Count == 0) return;
            ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Horizontal, Mathf.Clamp(ComponentMotion.Index + Delta, 0, CameraManager.Horizontal.Count - 1));
        }
        else if (ComponentMotion.Mode == Lanotalium.Editor.ComponentMotionMode.Vertical)
        {
            if (CameraManager.Vertical == null || CameraManager.Vertical.Count == 0) return;
            ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Vertical, Mathf.Clamp(ComponentMotion.Index + Delta, 0, CameraManager.Vertical.Count - 1));
        }
        else if (ComponentMotion.Mode == Lanotalium.Editor.ComponentMotionMode.Rotation)
        {
            if (CameraManager.Rotation == null || CameraManager.Rotation.Count == 0) return;
            ComponentMotion.SetMode(Lanotalium.Editor.ComponentMotionMode.Rotation, Mathf.Clamp(ComponentMotion.Index + Delta, 0, CameraManager.Rotation.Count - 1));
        }
    }
    public void OnWaveformToggle()
    {
        LimSystem.Preferences.Waveform = WaveformToggle.isOn;
        if (LimSystem.Preferences.Waveform)
        {
            WaveformManager.Show();
        }
        else
        {
            WaveformManager.Hide();
        }
    }
    public void OnWindowSorted(int Order)
    {
        WaveformManager.LineL.sortingOrder = Order + 1;
        WaveformManager.LineR.sortingOrder = Order + 1;
    }
}