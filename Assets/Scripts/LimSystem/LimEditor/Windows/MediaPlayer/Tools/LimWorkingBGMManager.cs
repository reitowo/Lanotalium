using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimWorkingBGMManager : MonoBehaviour
{
    public AudioSource MusicPlayer;
    public AudioLowPassFilter LowPassFilter;
    public AudioHighPassFilter HighPassFilter;

    private Coroutine FadeToWorkingCoroutine;

    public void Start()
    {
        LimMediaPlayerManager.OnMusicLoad.AddListener(OnMediaPlayerLoadMusic);
        LimMediaPlayerManager.OnPause.AddListener(PlayBGM);
        LimMediaPlayerManager.OnPlay.AddListener(PauseBGM);
    }
    private void OnMediaPlayerLoadMusic(AudioClip Clip)
    {
        MusicPlayer.clip = Clip;
    }
    private void PlayBGM(float Time)
    {
        if (!LimSystem.Preferences.PlayWorkingBGM) return;
        if (FadeToWorkingCoroutine != null) StopCoroutine(FadeToWorkingCoroutine);
        FadeToWorkingCoroutine = StartCoroutine(FadeToWorking(Time));
    }
    IEnumerator FadeToWorking(float Time)
    {
        MusicPlayer.time = Time;
        MusicPlayer.Play();
        float Duration = 0.5f;
        while (Duration > 0)
        {
            float Percent = (1 - Duration / 0.5f);
            LowPassFilter.cutoffFrequency = 22000 - 19000 * Percent;
            HighPassFilter.cutoffFrequency = 1500 * Percent;
            Duration -= UnityEngine.Time.deltaTime;
            yield return null;
        }
        LowPassFilter.cutoffFrequency = 3000;
        HighPassFilter.cutoffFrequency = 1500;
    }
    private void PauseBGM(float Time)
    {
        MusicPlayer.time = Time;
        MusicPlayer.Pause();
    }
}
