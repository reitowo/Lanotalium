using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.Apple.ReplayKit;
#endif

public class LimiOSReplayKitManager : MonoBehaviour
{
    public GameObject ControllerView;
    public LimMediaPlayerManager MediaPlayerManager;
    public LimDisplayManager DisplayManager;
    public Color True, False;
    public Image Record, API;
    private Coroutine RecordChartCr;
#if UNITY_IOS
    private void Update()
    {
        if (DisplayManager.FullScreenTuner)
        {
            if (!ControllerView.activeInHierarchy)
            {
                ControllerView.SetActive(true);
            }
        }
        else if (!DisplayManager.FullScreenTuner)
        {
            if (ControllerView.activeInHierarchy)
            {
                ControllerView.SetActive(false);
            }
        }
        Record.color = ReplayKit.isRecording ? True : False;
        API.color = ReplayKit.APIAvailable ? True : False;
    }
    public void StopChartRecord()
    {
        if (RecordChartCr != null) StopCoroutine(RecordChartCr);
        ReplayKit.StopRecording();
    }
    public void BackToEditor()
    {
        DisplayManager.FullScreenTuner = false;
    }
    public void RecordChart()
    {
        RecordChartCr = StartCoroutine(RecordChartCoroutine());
    }
    IEnumerator RecordChartCoroutine()
    {
        MediaPlayerManager.StopMedia();
        yield return null;
        ReplayKit.StartRecording();
        yield return new WaitForSeconds(1);
        MediaPlayerManager.PlayMedia();
        while (MediaPlayerManager.IsPlaying) yield return null;
        ReplayKit.StopRecording();
        while (!ReplayKit.recordingAvailable) yield return null;
        ReplayKit.Preview();
    }
#endif
}
