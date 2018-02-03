using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LimBackgroundManager : MonoBehaviour
{
    private bool isInitialized = false;

    public Lanotalium.Chart.ChartBackground BackgroundData;
    public RenderTexture BackgroundRenderTexture;
    public Material BackgroundRenderTextureMaterial;
    public LimTunerManager Tuner;
    public VideoPlayer BackgroundVideoPlayer;
    public RectTransform TunerWindowRectTransform;
    public Image ColorImg, GrayImg, LinearImg;
    public RectTransform ColorRect, GrayRect, LinearRect;
    public LimDisplayManager DisplayManager;
    public LimMediaPlayerManager MediaPlayerManager;
    public Lanotalium.Background.BackgroundMode Mode = Lanotalium.Background.BackgroundMode.None;
    public float HorizontalRatio, VerticalRatio;

    public float VideoTime
    {
        set
        {
            BackgroundVideoPlayer.time = value;
        }
        get
        {
            return (float)BackgroundVideoPlayer.time;
        }
    }
    public float VideoLength
    {
        get
        {
            return BackgroundVideoPlayer.frameCount / BackgroundVideoPlayer.frameRate;
        }
    }

    private void Update()
    {
        if (LimSystem.ChartContainer == null) return;
        if (!isInitialized) return;
        SyncRenderTextureWithWindowSize();
        BackgroundImageSizeUpdator();
        BackgroundUpdator();
    }

    public void Initialize(Lanotalium.Chart.ChartBackground BackgroundData, Lanotalium.Chart.ChartLoadResult LoadResult)
    {
        this.BackgroundData = BackgroundData;
        if (LoadResult.isBackgroundLoaded) Mode = Lanotalium.Background.BackgroundMode.Single;
        if (LoadResult.isBackgroundLoaded && LoadResult.isBackgroundGrayLoaded) Mode = Lanotalium.Background.BackgroundMode.Duo;
        if (LoadResult.isBackgroundLoaded && LoadResult.isBackgroundGrayLoaded && LoadResult.isBackgroundLinearLoaded) Mode = Lanotalium.Background.BackgroundMode.Triple;
        if (LoadResult.isBackgroundVideoDetected)
        {
            Mode = Lanotalium.Background.BackgroundMode.Video;
            VideoBackgroundInitialize();
        }
        else ImageBackgroundInitialize();
        isInitialized = true;
    }
    private void VideoBackgroundInitialize()
    {
        BackgroundVideoPlayer.url = BackgroundData.VideoPath;
        BackgroundVideoPlayer.waitForFirstFrame = true;
        BackgroundVideoPlayer.Prepare();
        BackgroundVideoPlayer.prepareCompleted += BackgroundVideoPlayer_prepareCompleted;
        BackgroundVideoPlayer.seekCompleted += BackgroundVideoPlayer_seekCompleted;
        ColorImg.material = BackgroundRenderTextureMaterial;
        ColorImg.color = new Color(1, 1, 1, 1);
        GrayImg.color = new Color(1, 1, 1, 0);
        LinearImg.color = new Color(1, 1, 1, 0);
        LimNotifyIcon.ShowMessage(LimLanguageManager.NotificationDict["Background_VideoWarning"]);
    }

    private void ImageBackgroundInitialize()
    {
        ColorImg.material = null;
        if (Mode == Lanotalium.Background.BackgroundMode.Single)
        {
            ColorImg.sprite = BackgroundData.Color;
            ColorImg.color = new Color(1, 1, 1, 1);
            GrayImg.color = new Color(1, 1, 1, 0);
            LinearImg.color = new Color(1, 1, 1, 0);
        }
        else if (Mode == Lanotalium.Background.BackgroundMode.Duo)
        {
            ColorImg.sprite = BackgroundData.Color;
            GrayImg.sprite = BackgroundData.Gray;
            ColorImg.color = new Color(1, 1, 1, 1);
            GrayImg.color = new Color(1, 1, 1, 1);
            LinearImg.color = new Color(1, 1, 1, 0);
        }
        else if (Mode == Lanotalium.Background.BackgroundMode.Triple)
        {
            ColorImg.sprite = BackgroundData.Color;
            GrayImg.sprite = BackgroundData.Gray;
            LinearImg.sprite = BackgroundData.Linear;
            ColorImg.color = new Color(1, 1, 1, 1);
            GrayImg.color = new Color(1, 1, 1, 1);
            LinearImg.color = new Color(1, 1, 1, 1);
        }
    }

    private void BackgroundVideoPlayer_seekCompleted(VideoPlayer source)
    {
        MediaPlayerManager.OnTimeSeekEnd();
    }
    private void BackgroundVideoPlayer_prepareCompleted(VideoPlayer source)
    {
        MediaPlayerManager.OnPrepared();
        Play();
    }

    private void SyncRenderTextureWithWindowSize()
    {
        if (Mode == Lanotalium.Background.BackgroundMode.Video)
        {
            if (!DisplayManager.FullScreenTuner)
            {
                if (new Vector2(BackgroundRenderTexture.width, BackgroundRenderTexture.height) != TunerWindowRectTransform.sizeDelta)
                {
                    BackgroundRenderTexture.Release();
                    BackgroundRenderTexture.width = Mathf.RoundToInt(TunerWindowRectTransform.sizeDelta.x);
                    BackgroundRenderTexture.height = Mathf.RoundToInt(TunerWindowRectTransform.sizeDelta.y);
                    BackgroundRenderTexture.Create();
                }
            }
            else
            {
                if (BackgroundRenderTexture.width != Screen.width || BackgroundRenderTexture.height != Screen.height)
                {
                    BackgroundRenderTexture.Release();
                    BackgroundRenderTexture.width = Screen.width;
                    BackgroundRenderTexture.height = Screen.height;
                    BackgroundRenderTexture.Create();
                }
            }
        }
    }

    public void Play()
    {
        if (Mode != Lanotalium.Background.BackgroundMode.Video) return;
        BackgroundVideoPlayer.Play();
    }
    public void Pause()
    {
        if (Mode != Lanotalium.Background.BackgroundMode.Video) return;
        BackgroundVideoPlayer.Pause();
    }
    public void Stop()
    {
        if (Mode != Lanotalium.Background.BackgroundMode.Video) return;
        VideoTime = 0;
        BackgroundVideoPlayer.Pause();
    }
    public void SetVideoTimeTo(float Time)
    {
        if (Mode != Lanotalium.Background.BackgroundMode.Video) return;
        VideoTime = Time;
    }

    public void BackgroundUpdator()
    {
        float Progress = Tuner.ChartTime / Tuner.MediaPlayerManager.Length;
        if (Mode == Lanotalium.Background.BackgroundMode.Duo)
        {
            GrayImg.color = new Color(1, 1, 1, 1 - Progress);
        }
        else if (Mode == Lanotalium.Background.BackgroundMode.Triple)
        {
            if (Progress > 0 && Progress <= 0.5f)
            {
                float Fade = (0.5f - Progress) / 0.5f;
                GrayImg.color = new Color(1, 1, 1, 1);
                LinearImg.color = new Color(1, 1, 1, Fade);
            }
            else if (Progress > 0.5f && Progress <= 1)
            {
                float Fade = (1 - Progress) / 0.5f;
                GrayImg.color = new Color(1, 1, 1, Fade);
                LinearImg.color = new Color(1, 1, 1, 0);
            }
        }
    }
    public void BackgroundImageSizeUpdator()
    {
        if (DisplayManager.FullScreenTuner)
        {
            if (Mode != Lanotalium.Background.BackgroundMode.Video) SetBackgroundImageSize(Screen.height);
            else
            {
                ColorRect.sizeDelta = new Vector2(Screen.width, Screen.height);
            }
            return;
        }
        Vector2 Size = new Vector2(TunerWindowRectTransform.sizeDelta.y * (LimSystem.Preferences.StretchBGA ? HorizontalRatio : 1), TunerWindowRectTransform.sizeDelta.y * (LimSystem.Preferences.StretchBGA ? VerticalRatio : 1));
        if (Mode == Lanotalium.Background.BackgroundMode.Single) ColorRect.sizeDelta = Size;
        else if (Mode == Lanotalium.Background.BackgroundMode.Duo)
        {
            ColorRect.sizeDelta = Size;
            GrayRect.sizeDelta = Size;
        }
        else if (Mode == Lanotalium.Background.BackgroundMode.Triple)
        {
            ColorRect.sizeDelta = Size;
            GrayRect.sizeDelta = Size;
            LinearRect.sizeDelta = Size;
        }
        else if (Mode == Lanotalium.Background.BackgroundMode.Video)
        {
            ColorRect.sizeDelta = TunerWindowRectTransform.sizeDelta;
        }
    }
    public void SetBackgroundImageSize(int Height)
    {
        Vector2 Size = new Vector2(Height * (LimSystem.Preferences.StretchBGA ? HorizontalRatio : 1), Height * (LimSystem.Preferences.StretchBGA ? VerticalRatio : 1));
        if (Mode == Lanotalium.Background.BackgroundMode.Single) ColorRect.sizeDelta = Size;
        else if (Mode == Lanotalium.Background.BackgroundMode.Duo)
        {
            ColorRect.sizeDelta = Size;
            GrayRect.sizeDelta = Size;
        }
        else if (Mode == Lanotalium.Background.BackgroundMode.Triple)
        {
            ColorRect.sizeDelta = Size;
            GrayRect.sizeDelta = Size;
            LinearRect.sizeDelta = Size;
        }
    }
}