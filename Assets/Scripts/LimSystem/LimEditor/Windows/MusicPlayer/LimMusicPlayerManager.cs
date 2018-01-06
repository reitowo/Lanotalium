using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LimMusicPlayerManager : MonoBehaviour
{
    private bool isInitialized = false;
    public LimTunerManager TunerManager;
    public LimWindowManager BaseWindow;
    public ComponentBpmManager ComponentBpm;
    public Text MusicNameText, ProgressLabel, PitchLabel, PitchResetText, PlayText, PauseText, StopText;
    public Text PreciseControllerLabel, OffsetLabel, SwitchPlayerModeLabel, FixTimeLabel, CurrentModeText;
    public Slider ProgressSlider, PitchSlider;
    public AudioSource MusicPlayer;
    public InputField OffsetInputField, ProgressInputField, PitchInputField;
    public Image OffsetInputFieldImage, ProgressImg, PitchImg;
    public GameObject PreciseModeController;
    public Color InvalidColor, ValidColor;
    public Button PlayBtn, PauseBtn, StopBtn, SwitchModeBtn, ResetBtn, FixTimeBtn;
    public LimWaveformManager WaveformManager;
    public bool isProgressPressed = false, isPitchPressed = false;
    public float CurrentTime = 0;
    public float PreciseModeTimeOffset = 0;

    public string MusicName
    {
        get { return MusicNameText.text; }
        set { MusicNameText.text = value; }
    }
    public float Pitch
    {
        get { return MusicPlayer.pitch; }
        set
        {
            MusicPlayer.pitch = value;
            PitchSlider.value = value;
            TunerManager.BackgroundManager.BackgroundVideoPlayer.playbackSpeed = value;
        }
    }
    public bool isPlaying
    {
        get
        {
            return MusicPlayer.isPlaying;
        }
        set
        {
            if (value) PlayMusic();
            else PauseMusic();
        }
    }
    public float time
    {
        get
        {
            return CurrentTime;
        }
        set
        {
            MusicPlayer.time = value;
            CurrentTime = value;
        }
    }
    public float Length
    {
        get
        {
            return MusicPlayer.clip.length;
        }
    }

    public Lanotalium.MusicPlayer.SyncValuesDelegate SyncValues;
    public Lanotalium.MusicPlayer.PlayMusicDelegate PlayMusic;
    public Lanotalium.MusicPlayer.PauseMusicDelegate PauseMusic;
    public Lanotalium.MusicPlayer.StopMusicDelegate StopMusic;

    private RectTransform ViewRect;
    public Lanotalium.MusicPlayer.MusicPlayerMode MusicPlayerMode = Lanotalium.MusicPlayer.MusicPlayerMode.Sync;
    private float UiWidth;
    private bool ProgressOnEdit, PitchOnEdit;

    void Start()
    {
        UsePreciseMode();
        ViewRect = BaseWindow.WindowRectTransform;
        UiWidth = ViewRect.rect.width;
    }
    public void SetTexts()
    {
        BaseWindow.WindowName = LimLanguageManager.TextDict["Window_MusicPlayer_Label"];
        ProgressLabel.text = LimLanguageManager.TextDict["Window_MusicPlayer_Progress"];
        PitchLabel.text = LimLanguageManager.TextDict["Window_MusicPlayer_Pitch"];
        PitchResetText.text = LimLanguageManager.TextDict["Window_MusicPlayer_PitchReset"];
        PlayText.text = LimLanguageManager.TextDict["Window_MusicPlayer_Play"];
        PauseText.text = LimLanguageManager.TextDict["Window_MusicPlayer_Pause"];
        StopText.text = LimLanguageManager.TextDict["Window_MusicPlayer_Stop"];
        PreciseControllerLabel.text = LimLanguageManager.TextDict["Window_MusicPlayer_PreciseControl"];
        OffsetLabel.text = LimLanguageManager.TextDict["Window_MusicPlayer_Offset"];
        SwitchPlayerModeLabel.text = LimLanguageManager.TextDict["Window_MusicPlayer_SwitchPlayerMode"];
        FixTimeLabel.text = LimLanguageManager.TextDict["Window_MusicPlayer_FixTime"];
        if (MusicPlayerMode == Lanotalium.MusicPlayer.MusicPlayerMode.Precise) CurrentModeText.text = LimLanguageManager.TextDict["Window_MusicPlayer_Precise"];
        else CurrentModeText.text = LimLanguageManager.TextDict["Window_MusicPlayer_Sync"];
    }

    void Update()
    {
        OnUiWidthChange();
        if (!isInitialized) return;
        SyncValues();
        DetectPlayControl();
        DetectHotkey();
    }
    public void OnUiWidthChange()
    {
        if (UiWidth != ViewRect.rect.width)
        {
            UiWidth = ViewRect.rect.width;
            float Ratio = UiWidth / 500f;
            PlayBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(60 * Ratio, 30);
            PlayBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(10 * Ratio, 0);
            PauseBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(60 * Ratio, 30);
            PauseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(80 * Ratio, 0);
            StopBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(60 * Ratio, 30);
            StopBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(150 * Ratio, 0);
            SwitchModeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(120 * Ratio, 30);
            SwitchModeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(220 * Ratio, 0);
            CurrentModeText.GetComponent<RectTransform>().sizeDelta = new Vector2(90 * Ratio, 30);
            CurrentModeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(340 * Ratio, 0);
            ResetBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(50 * Ratio, 30);
            ResetBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(480 * Ratio, 0);
            FixTimeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100 * Ratio, 30);
            FixTimeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(210 * Ratio, 0);
            OffsetLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(60 * Ratio, 30);
            OffsetLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(315 * Ratio, 0);
            OffsetInputField.GetComponent<RectTransform>().sizeDelta = new Vector2(100 * Ratio, 30);
            OffsetInputField.GetComponent<RectTransform>().anchoredPosition = new Vector2(380 * Ratio, 0);
            PreciseControllerLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(190 * Ratio, 30);
            PreciseControllerLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(10 * Ratio, 0);
        }
    }
    public void DetectPlayControl()
    {
        //if (Capturer.isCapturing) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (EventSystem.current.currentSelectedGameObject != null) return;
            if (MusicPlayer.isPlaying) PauseMusic();
            else if (!MusicPlayer.isPlaying) PlayMusic();
        }
    }
    public void DetectHotkey()
    {
        //if (Capturer.isCapturing) return;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) time = ComponentBpm.FindPrevOrNextBeatline(TunerManager.ChartTime, false);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) time = ComponentBpm.FindPrevOrNextBeatline(TunerManager.ChartTime, true);
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) time = Mathf.Clamp(time - 0.001f, 0, Length);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) time = Mathf.Clamp(time + 0.001f, 0, Length);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) time = Mathf.Clamp(time - 0.01f, 0, Length);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) time = Mathf.Clamp(time + 0.01f, 0, Length);
        }
    }

    public void Initialize(Lanotalium.Chart.ChartMusic MusicData, Lanotalium.Chart.ChartProperty PropertyData)
    {
        MusicName = PropertyData.ChartName;
        ProgressSlider.maxValue = MusicData.Length;
        MusicPlayer.clip = MusicData.Music;
        LimSystem.ChartContainer.ChartData.SongLength = MusicData.Length;
        PreciseModeTimeOffset = LimSystem.Preferences.MusicPlayerPreciseOffset;
        OffsetInputField.text = PreciseModeTimeOffset.ToString();
        isInitialized = true;
        WaveformManager.OnMusicLoaded();
    }

    public void SwitchPlayerMode()
    {
        if (MusicPlayerMode == Lanotalium.MusicPlayer.MusicPlayerMode.Sync)
        {
            UsePreciseMode();
        }
        else if (MusicPlayerMode == Lanotalium.MusicPlayer.MusicPlayerMode.Precise)
        {
            UseSyncMode();
        }
    }
    private void UsePreciseMode()
    {
        SyncValues = SyncValuesPreciseMode;
        PlayMusic = PlayMusicPreciseMode;
        PauseMusic = PauseMusicPreciseMode;
        StopMusic = StopMusicPreciseMode;
        PreciseModeController.SetActive(true);
        BaseWindow.WindowRectTransform.sizeDelta = new Vector2(500, 190);
        CurrentModeText.text = LimLanguageManager.TextDict["Window_MusicPlayer_Precise"];
        MusicPlayerMode = Lanotalium.MusicPlayer.MusicPlayerMode.Precise;
    }
    private void UseSyncMode()
    {
        SyncValues = SyncValuesSyncMode;
        PlayMusic = PlayMusicSyncMode;
        PauseMusic = PauseMusicSyncMode;
        StopMusic = StopMusicSyncMode;
        PreciseModeController.SetActive(true);
        BaseWindow.WindowRectTransform.sizeDelta = new Vector2(500, 150);
        CurrentModeText.text = LimLanguageManager.TextDict["Window_MusicPlayer_Sync"];
        MusicPlayerMode = Lanotalium.MusicPlayer.MusicPlayerMode.Sync;
    }

    private void SyncValuesPreciseMode()
    {
        if (MusicPlayer.isPlaying) CurrentTime += Time.deltaTime * MusicPlayer.pitch;
        if (CurrentTime > MusicPlayer.clip.length)
        {
            StopMusic();
            PlayMusic();
        }
        if (!isProgressPressed) ProgressSlider.value = CurrentTime;
        else time = ProgressSlider.value;
        if (isPitchPressed) Pitch = PitchSlider.value;
        if (!ProgressOnEdit) ProgressInputField.text = CurrentTime.ToString("f4");
        if (!PitchOnEdit) PitchInputField.text = MusicPlayer.pitch.ToString("f3");
    }
    private void PlayMusicPreciseMode()
    {
        if (!MusicPlayer.isPlaying)
        {
            MusicPlayer.Play();
            FixCurrentTime();
        }
    }
    private void PauseMusicPreciseMode()
    {
        if (MusicPlayer.isPlaying)
        {
            MusicPlayer.Pause();
        }
    }
    private void StopMusicPreciseMode()
    {
        MusicPlayer.Stop();
        MusicPlayer.time = 0;
        CurrentTime = 0;
    }
    public void FixCurrentTime()
    {
        if (MusicPlayerMode != Lanotalium.MusicPlayer.MusicPlayerMode.Precise) return;
        CurrentTime = MusicPlayer.time + PreciseModeTimeOffset;
        ProgressSlider.value = CurrentTime;
    }
    public void OnOffsetChange()
    {
        float Offset;
        if (!float.TryParse(OffsetInputField.text, out Offset))
        {
            OffsetInputFieldImage.color = InvalidColor;
            return;
        }
        PreciseModeTimeOffset = Offset;
        FixCurrentTime();
        OffsetInputFieldImage.color = ValidColor;
        LimSystem.Preferences.MusicPlayerPreciseOffset = Offset;
    }
    private void OnApplicationFocus(bool focus)
    {
        if (focus) if (isPlaying) FixCurrentTime();
    }
    private void OnApplicationPause(bool pause)
    {
        if (!pause) if (isPlaying) FixCurrentTime();
    }

    private void SyncValuesSyncMode()
    {
        if (!isProgressPressed) ProgressSlider.value = MusicPlayer.time;
        else time = ProgressSlider.value;
        if (isPitchPressed) Pitch = PitchSlider.value;
        if (!ProgressOnEdit) ProgressInputField.text = MusicPlayer.time.ToString("f3");
        if (!PitchOnEdit) PitchInputField.text = MusicPlayer.pitch.ToString("f3");
        CurrentTime = MusicPlayer.time;
    }
    private void PlayMusicSyncMode()
    {
        if (!MusicPlayer.isPlaying) MusicPlayer.Play();
    }
    private void PauseMusicSyncMode()
    {
        if (MusicPlayer.isPlaying)
        {
            MusicPlayer.Pause();
        }
    }
    private void StopMusicSyncMode()
    {
        MusicPlayer.Stop();
    }

    public void CallPlayMusicFromUi()
    {
        PlayMusic();
    }
    public void CallPauseMusicFromUi()
    {
        PauseMusic();
    }
    public void CallStopMusicFromUi()
    {
        StopMusic();
    }

    public void ResetPitch()
    {
        Pitch = 1;
    }
    public void OnProgressDown()
    {
        isProgressPressed = true;
    }
    public void OnProgressUp()
    {
        isProgressPressed = false;
    }
    public void OnPitchDown()
    {
        isPitchPressed = true;
    }
    public void OnPitchUp()
    {
        isPitchPressed = false;
    }
    public void OnProgressInputFieldSelect()
    {
        ProgressOnEdit = true;
    }
    public void OnProgressInputFieldDeselect()
    {
        ProgressOnEdit = false;
    }
    public void OnPitchInputFieldSelect()
    {
        PitchOnEdit = true;
    }
    public void OnPitchInputFieldDeselect()
    {
        PitchOnEdit = false;
    }
    public void OnProgressValueChange()
    {
        if (!ProgressOnEdit) return;
        float ProgressTmp;
        if (!float.TryParse(ProgressInputField.text, out ProgressTmp))
        {
            ProgressImg.color = InvalidColor;
            return;
        }
        if (ProgressTmp < 0 || ProgressTmp > MusicPlayer.clip.length)
        {
            ProgressImg.color = InvalidColor;
            return;
        }
        ProgressImg.color = ValidColor;
        time = ProgressTmp;
    }
    public void OnPitchValueChange()
    {
        if (!PitchOnEdit) return;
        float PitchTmp;
        if (!float.TryParse(PitchInputField.text, out PitchTmp))
        {
            PitchImg.color = InvalidColor;
            return;
        }
        if (Pitch < -3 || Pitch > 3)
        {
            PitchImg.color = InvalidColor;
            return;
        }
        PitchImg.color = ValidColor;
        MusicPlayer.pitch = PitchTmp;
    }
}
