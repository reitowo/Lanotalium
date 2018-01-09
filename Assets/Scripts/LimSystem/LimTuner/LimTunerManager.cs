using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LimTunerManager : MonoBehaviour
{
    public Lanotalium.ChartContainer ChartContainer;
    public Transform NotesTransform;
    public LimBpmManager BpmManager;
    public LimCameraManager CameraManager;
    public LimScrollManager ScrollManager;
    public LimTapNoteManager TapNoteManager;
    public LimHoldNoteManager HoldNoteManager;
    public LimMediaPlayerManager MediaPlayerManager;
    public LimBackgroundManager BackgroundManager;
    public LimAudioEffectManager AudioEffectManager;
    public bool isInitialized = false;
    public float ChartTime = 0;
    public float ChartPlaySpeed = 2;

    void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        if (LimSystem.ChartContainer == null) return;
        ChartContainer = LimSystem.ChartContainer;
        BpmManager.Initialize(ChartContainer.ChartData.LanotaChangeBpm);
        CameraManager.Initialize(ChartContainer.ChartData.LanotaCameraRot,
            ChartContainer.ChartData.LanotaCameraXZ,
            ChartContainer.ChartData.LanotaCameraY,
            ChartContainer.ChartData.LanotaDefault);
        ScrollManager.Initialize(ChartContainer.ChartData.LanotaScroll);
        TapNoteManager.Initialize(ChartContainer.ChartData.LanotaTapNote);
        HoldNoteManager.Initialize(ChartContainer.ChartData.LanotaHoldNote);
        MediaPlayerManager.Initialize(ChartContainer.ChartMusic, ChartContainer.ChartProperty);
        BackgroundManager.Initialize(ChartContainer.ChartBackground, ChartContainer.ChartLoadResult);
        isInitialized = true;
    }
    private void UpdateChartTime()
    {
        ChartTime = MediaPlayerManager.CurrentTime;
    }
    void Update()
    {
        UpdateChartTime();
    }
}
