using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimAudioEffectManager : MonoBehaviour
{
    public List<AudioClip> Lanota, Stellights;
    public AudioSource Click, FlickIn, FlickOut, Rail, RailEnd;

    private void SetEffectAudioClips(List<AudioClip> Set)
    {
        Click.clip = Set[0];
        FlickIn.clip = Set[1];
        FlickOut.clip = Set[2];
        Rail.clip = Set[3];
        RailEnd.clip = Set[4];
    }
    public void SetEffectTheme()
    {
        switch (LimSystem.Preferences.AudioEffectTheme)
        {
            case Lanotalium.Tuner.AudioEffectTheme.Lanota: SetEffectAudioClips(Lanota); break;
            case Lanotalium.Tuner.AudioEffectTheme.Stellights: SetEffectAudioClips(Stellights); break;
        }
    }
    private void Start()
    {
        SetEffectTheme();
    }
    public void PlayClick()
    {
        if (Click.isPlaying) Click.time = 0;
        else Click.Play();
    }
    public void PlayFlickIn()
    {
        if (FlickIn.isPlaying) FlickIn.time = 0;
        else FlickIn.Play();
    }
    public void PlayFlickOut()
    {
        if (FlickOut.isPlaying) FlickOut.time = 0;
        else FlickOut.Play();
    }
    public void PlayRailEnd()
    {
        if (RailEnd.isPlaying) RailEnd.time = 0;
        else RailEnd.Play();
        Rail.Stop();
    }
    public void StartPlayRail()
    {
        if (!Rail.isPlaying) Rail.Play();
    }
    public void StopPlayRail()
    {
        if (Rail.isPlaying) Rail.Stop();
    }
}
