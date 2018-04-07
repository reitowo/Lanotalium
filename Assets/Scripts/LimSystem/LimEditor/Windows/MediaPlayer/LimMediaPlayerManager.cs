using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Lanotalium.MediaPlayer;

public class LimMediaPlayerManager : MonoBehaviour
{
    public static OnPauseEvent OnPause;
    public static OnPlayEvent OnPlay;
    public static OnMusicLoadEvent OnMusicLoad;

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
    public LimBackgroundManager BackgroundManager;
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
    public bool IsPlaying
    {
        get
        {
            if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.Video) return BackgroundManager.BackgroundVideoPlayer.isPlaying;
            else return MusicPlayer.isPlaying;
        }
        set
        {
            if (value)
            {
                PlayMedia();
                OnPlay.Invoke(Time);
            }
            else
            {
                PauseMedia();
                OnPause.Invoke(Time);
            }
        }
    }
    public float Time
    {
        get
        {
            return CurrentTime;
        }
        set
        {
            MusicPlayer.time = Mathf.Clamp(value, 0, Length);
            CurrentTime = Mathf.Clamp(value, 0, Length);
            if (MediaPlayerMode == MediaPlayerMode.MusicPrecise) FixCurrentTime();
        }
    }
    public float Length
    {
        get
        {
            if (MusicPlayer.clip == null) return 0;
            if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.Video) return BackgroundManager.VideoLength;
            else return MusicPlayer.clip.length;
        }
    }

    public SyncValuesDelegate SyncValues;
    public PlayMediaDelegate PlayMedia;
    public PauseMediaDelegate PauseMedia;
    public StopMediaDelegate StopMedia;

    private RectTransform ViewRect;
    public MediaPlayerMode MediaPlayerMode = Lanotalium.MediaPlayer.MediaPlayerMode.MusicSync;
    private float UiWidth;
    private bool ProgressOnEdit, PitchOnEdit;

    void Start()
    {
        OnPause = new OnPauseEvent();
        OnPlay = new OnPlayEvent();
        OnMusicLoad = new OnMusicLoadEvent();
        ViewRect = BaseWindow.WindowRectTransform;
        UiWidth = ViewRect.rect.width;
        if (LimSystem.ChartContainer == null) return;
        if (LimSystem.ChartContainer.ChartLoadResult.isBackgroundVideoDetected)
        {
            InitializeVideo();
            return;
        }
        UsePreciseMode();
    }
    public void SetTexts()
    {
        BaseWindow.WindowName = LimLanguageManager.TextDict["Window_MediaPlayer_Label"];
        ProgressLabel.text = LimLanguageManager.TextDict["Window_MediaPlayer_Progress"];
        PitchLabel.text = LimLanguageManager.TextDict[MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.Video ? "Window_MediaPlayer_PlaybackSpeed" : "Window_MediaPlayer_Pitch"];
        PitchResetText.text = LimLanguageManager.TextDict["Window_MediaPlayer_PitchReset"];
        PlayText.text = LimLanguageManager.TextDict["Window_MediaPlayer_Play"];
        PauseText.text = LimLanguageManager.TextDict["Window_MediaPlayer_Pause"];
        StopText.text = LimLanguageManager.TextDict["Window_MediaPlayer_Stop"];
        PreciseControllerLabel.text = LimLanguageManager.TextDict["Window_MediaPlayer_PreciseControl"];
        OffsetLabel.text = LimLanguageManager.TextDict["Window_MediaPlayer_Offset"];
        SwitchPlayerModeLabel.text = LimLanguageManager.TextDict["Window_MediaPlayer_SwitchPlayerMode"];
        FixTimeLabel.text = LimLanguageManager.TextDict["Window_MediaPlayer_FixTime"];
        if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.MusicPrecise) CurrentModeText.text = LimLanguageManager.TextDict["Window_MediaPlayer_Precise"];
        else CurrentModeText.text = LimLanguageManager.TextDict["Window_MediaPlayer_Sync"];
    }
    void Update()
    {
        OnUiWidthChange();
        if (!isInitialized) return;
        DetectPlayControl();
        SyncValues();
        if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.Video) return;
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IsPlaying = !IsPlaying;
        }
    }
    public void DetectHotkey()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Time = ComponentBpm.FindPrevOrNextBeatline(TunerManager.ChartTime, false);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) Time = ComponentBpm.FindPrevOrNextBeatline(TunerManager.ChartTime, true);
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Time = Mathf.Clamp(Time - 0.001f, 0, Length);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) Time = Mathf.Clamp(Time + 0.001f, 0, Length);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Time = Mathf.Clamp(Time - 0.01f, 0, Length);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) Time = Mathf.Clamp(Time + 0.01f, 0, Length);
        }
    }
    #region MusicPlayer
    public void Initialize(Lanotalium.Chart.ChartMusic MusicData, Lanotalium.Chart.ChartProperty PropertyData)
    {
        if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.Video) return;
        MusicName = LimProjectManager.CurrentProject.Name;
        ProgressSlider.maxValue = MusicData.Length;
        MusicPlayer.clip = MusicData.Music;
        OnMusicLoad.Invoke(MusicPlayer.clip);
        LimSystem.ChartContainer.ChartData.SongLength = MusicData.Length;
        PreciseModeTimeOffset = LimSystem.Preferences.MusicPlayerPreciseOffset;
        OffsetInputField.text = PreciseModeTimeOffset.ToString();
        WaveformManager.OnMusicLoaded();
        IsPlaying = true;
        isInitialized = true;
    }

    public void SwitchPlayerMode()
    {
        if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.MusicSync)
        {
            UsePreciseMode();
        }
        else if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.MusicPrecise)
        {
            UseSyncMode();
        }
    }
    private void UsePreciseMode()
    {
        SyncValues = SyncValuesPreciseMode;
        PlayMedia = PlayMusicPreciseMode;
        PauseMedia = PauseMusicPreciseMode;
        StopMedia = StopMusicPreciseMode;
        PreciseModeController.SetActive(true);
        BaseWindow.WindowRectTransform.sizeDelta = new Vector2(500, 190);
        CurrentModeText.text = LimLanguageManager.TextDict["Window_MediaPlayer_Precise"];
        MediaPlayerMode = Lanotalium.MediaPlayer.MediaPlayerMode.MusicPrecise;
    }
    private void UseSyncMode()
    {
        SyncValues = SyncValuesSyncMode;
        PlayMedia = PlayMusicSyncMode;
        PauseMedia = PauseMusicSyncMode;
        StopMedia = StopMusicSyncMode;
        PreciseModeController.SetActive(true);
        BaseWindow.WindowRectTransform.sizeDelta = new Vector2(500, 150);
        CurrentModeText.text = LimLanguageManager.TextDict["Window_MediaPlayer_Sync"];
        MediaPlayerMode = Lanotalium.MediaPlayer.MediaPlayerMode.MusicSync;
    }

    private void SyncValuesPreciseMode()
    {
        if (MusicPlayer.isPlaying) CurrentTime += UnityEngine.Time.deltaTime * MusicPlayer.pitch;
        if (!isProgressPressed) ProgressSlider.value = CurrentTime;
        else Time = ProgressSlider.value;
        if (isPitchPressed) Pitch = PitchSlider.value;
        if (!ProgressOnEdit) ProgressInputField.text = CurrentTime.ToString("f4");
        if (!PitchOnEdit) PitchInputField.text = MusicPlayer.pitch.ToString("f3");
    }
    private void PlayMusicPreciseMode()
    {
        if (!MusicPlayer.isPlaying)
        {
            if (CurrentTime > Length) Time = 0;
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
        if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.Video)
        {
            CurrentTime = BackgroundManager.VideoTime + PreciseModeTimeOffset;
        }
        if (MediaPlayerMode != Lanotalium.MediaPlayer.MediaPlayerMode.MusicPrecise) return;
        CurrentTime = MusicPlayer.time + PreciseModeTimeOffset;
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
        if (focus) if (IsPlaying) FixCurrentTime();
    }
    private void OnApplicationPause(bool pause)
    {
        if (!pause) if (IsPlaying) FixCurrentTime();
    }

    private void SyncValuesSyncMode()
    {
        if (!isProgressPressed) ProgressSlider.value = MusicPlayer.time;
        else Time = ProgressSlider.value;
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
        IsPlaying = true;
    }
    public void CallPauseMusicFromUi()
    {
        IsPlaying = false;
    }
    public void CallStopMusicFromUi()
    {
        StopMedia();
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
        if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.Video)
        {
            BackgroundManager.SetVideoTimeTo(ProgressSlider.value);
        }
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
        if (!TunerManager.isInitialized) return;
        if (MediaPlayerMode == MediaPlayerMode.Video) return;
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
        Time = ProgressTmp;
    }
    public void OnPitchValueChange()
    {
        if (!TunerManager.isInitialized) return;
        if (!PitchOnEdit) return;
        float PitchTmp;
        if (!float.TryParse(PitchInputField.text, out PitchTmp))
        {
            PitchImg.color = InvalidColor;
            return;
        }
        if (MediaPlayerMode == Lanotalium.MediaPlayer.MediaPlayerMode.Video)
        {
            if (Pitch < 0 || Pitch > 10)
            {
                PitchImg.color = InvalidColor;
                return;
            }
            PitchImg.color = ValidColor;
            BackgroundManager.BackgroundVideoPlayer.playbackSpeed = PitchTmp;
        }
        else
        {
            if (Pitch < -3 || Pitch > 3)
            {
                PitchImg.color = InvalidColor;
                return;
            }
            PitchImg.color = ValidColor;
            MusicPlayer.pitch = PitchTmp;
        }
    }
    #endregion
    #region VideoPlayer
    public void InitializeVideo()
    {
        SyncValues = SyncValuesVideoMode;
        PlayMedia = PlayMediaVideoMode;
        PauseMedia = PauseMediaVideoMode;
        StopMedia = StopMediaVideoMode;
        MediaPlayerMode = Lanotalium.MediaPlayer.MediaPlayerMode.Video;
    }
    public void OnPrepared()
    {
        MusicName = LimProjectManager.CurrentProject.Name;
        ProgressSlider.maxValue = BackgroundManager.VideoLength;
        LimSystem.ChartContainer.ChartData.SongLength = BackgroundManager.VideoLength;
        PreciseModeTimeOffset = LimSystem.Preferences.MusicPlayerPreciseOffset;
        OffsetInputField.text = PreciseModeTimeOffset.ToString();
        PitchLabel.text = LimLanguageManager.TextDict["Window_MediaPlayer_PlaybackSpeed"];
        PitchSlider.maxValue = 10;
        PitchSlider.minValue = 0;
        WaveformManager.OnMusicLoaded();
        isInitialized = true;
    }
    public void OnTimeSeekEnd()
    {
        CurrentTime = BackgroundManager.VideoTime + PreciseModeTimeOffset;
    }
    public void SyncValuesVideoMode()
    {
        if (IsPlaying) CurrentTime += UnityEngine.Time.deltaTime * BackgroundManager.BackgroundVideoPlayer.playbackSpeed;
        if (!isProgressPressed) ProgressSlider.value = CurrentTime;
        if (isPitchPressed) BackgroundManager.BackgroundVideoPlayer.playbackSpeed = PitchSlider.value;
        if (!ProgressOnEdit) ProgressInputField.text = CurrentTime.ToString("f4");
        if (!PitchOnEdit) PitchInputField.text = BackgroundManager.BackgroundVideoPlayer.playbackSpeed.ToString("f3");
    }
    public void PlayMediaVideoMode()
    {
        BackgroundManager.Play();
    }
    public void PauseMediaVideoMode()
    {
        BackgroundManager.Pause();
    }
    public void StopMediaVideoMode()
    {
        BackgroundManager.Stop();
    }
    #endregion
}
