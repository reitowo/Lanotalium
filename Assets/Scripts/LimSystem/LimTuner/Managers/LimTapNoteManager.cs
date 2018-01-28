using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimTapNoteManager : MonoBehaviour
{
    private bool isInitialized = false;

    public List<Lanotalium.Chart.LanotaTapNote> TapNote;
    public LimTunerManager Tuner;
    public Color OnSelectColor, NormalColor, OnJudgeColor;

    public GameObject cT0S0, cT0S1, cT0S2, cT0S3;
    public GameObject cT2S0, cT2S1, cT2S2, cT2S3;
    public GameObject cT3S0, cT3S1, cT3S2, cT3S3;
    public GameObject cT4S0, cT4S1, cT4S2, cT4S3;
    public GameObject oT0S0, oT0S1, oT0S2, oT0S3;
    public GameObject oT2S0, oT2S1, oT2S2, oT2S3;
    public GameObject oT3S0, oT3S1, oT3S2, oT3S3;
    public GameObject oT4S0, oT4S1, oT4S2, oT4S3;

    void Update()
    {
        if (!isInitialized) return;
        UpdateAllNoteTransforms();
        UpdateAllNoteActive();
        UpdateAllNoteColor();
        UpdateNoteAudioEffect();
    }

    public void Initialize(List<Lanotalium.Chart.LanotaTapNote> TapNoteData)
    {
        TapNote = TapNoteData;
        InstantiateNotes();
        isInitialized = true;
    }

    public GameObject GetPrefab(int Type, int Size, bool Combination)
    {
        if (Combination == true)
        {
            if (Type == 0)
            {
                if (Size == 0) return cT0S0;
                else if (Size == 1) return cT0S1;
                else if (Size == 2) return cT0S2;
                else if (Size == 3) return cT0S3;
            }
            else if (Type == 2)
            {
                if (Size == 0) return cT2S0;
                else if (Size == 1) return cT2S1;
                else if (Size == 2) return cT2S2;
                else if (Size == 3) return cT2S3;
            }
            else if (Type == 3)
            {
                if (Size == 0) return cT3S0;
                else if (Size == 1) return cT3S1;
                else if (Size == 2) return cT3S2;
                else if (Size == 3) return cT3S3;
            }
            else if (Type == 4)
            {
                if (Size == 0) return cT4S0;
                else if (Size == 1) return cT4S1;
                else if (Size == 2) return cT4S2;
                else if (Size == 3) return cT4S3;
            }
        }
        else if (Combination == false)
        {
            if (Type == 0)
            {
                if (Size == 0) return oT0S0;
                else if (Size == 1) return oT0S1;
                else if (Size == 2) return oT0S2;
                else if (Size == 3) return oT0S3;
            }
            else if (Type == 2)
            {
                if (Size == 0) return oT2S0;
                else if (Size == 1) return oT2S1;
                else if (Size == 2) return oT2S2;
                else if (Size == 3) return oT2S3;
            }
            else if (Type == 3)
            {
                if (Size == 0) return oT3S0;
                else if (Size == 1) return oT3S1;
                else if (Size == 2) return oT3S2;
                else if (Size == 3) return oT3S3;
            }
            else if (Type == 4)
            {
                if (Size == 0) return oT4S0;
                else if (Size == 1) return oT4S1;
                else if (Size == 2) return oT4S2;
                else if (Size == 3) return oT4S3;
            }
        }
        return null;
    }
    public void InstantiateNote(Lanotalium.Chart.LanotaTapNote Note)
    {
        Note.TapNoteGameObject = Instantiate(GetPrefab(Note.Type, Note.Size, Note.Combination), transform);
        Note.InstanceId = Note.TapNoteGameObject.GetInstanceID();
        Note.Sprite = Note.TapNoteGameObject.GetComponentInChildren<SpriteRenderer>();
        Note.TapNoteGameObject.SetActive(false);
    }
    private void InstantiateNotes()
    {
        foreach (Lanotalium.Chart.LanotaTapNote Note in TapNote)
        {
            InstantiateNote(Note);
        }
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

    private void UpdateAllNoteTransforms()
    {
        foreach (Lanotalium.Chart.LanotaTapNote Note in TapNote)
        {
            if (!LimScanTime.IsTapNoteinScanRange(Note)) continue;
            float Percent = CalculateMovePercent(Note.Time);
            Percent = CalculateEasedPercent(Percent);
            Note.Percent = Percent;
            if (Percent < 20) continue;
            float RotatedDegree = Note.Degree + Tuner.CameraManager.CurrentRotation;
            Note.TapNoteGameObject.transform.rotation = Quaternion.Euler(new Vector3(90, RotatedDegree, 0));
            Note.TapNoteGameObject.transform.position = new Vector3(-Percent / 10 * Mathf.Sin(RotatedDegree * Mathf.Deg2Rad), 0, -Percent / 10 * Mathf.Cos(RotatedDegree * Mathf.Deg2Rad));
            Note.TapNoteGameObject.transform.localScale = new Vector3(Percent / 100, Percent / 100, 0);
        }
    }
    private void UpdateAllNoteActive()
    {
        foreach (Lanotalium.Chart.LanotaTapNote Note in TapNote)
        {
            if (!LimScanTime.IsTapNoteinScanRange(Note))
            { if (Note.TapNoteGameObject.activeInHierarchy) Note.TapNoteGameObject.SetActive(false); continue; }
            if (Tuner.ChartTime > Note.Time + 0.1f && Note.TapNoteGameObject.activeInHierarchy) Note.TapNoteGameObject.SetActive(false);
            else if (Tuner.ChartTime < Note.Time)
            {
                if (Note.Percent <= 20 || Note.Percent >= 100)
                {
                    if (Note.TapNoteGameObject.activeInHierarchy) Note.TapNoteGameObject.SetActive(false);
                }
                else
                {
                    if (!Note.TapNoteGameObject.activeInHierarchy) Note.TapNoteGameObject.SetActive(true);
                }
            }
            else if (Tuner.ChartTime >= Note.Time && Tuner.ChartTime <= Note.Time + 0.1f)
            {
                if (Note.Percent == 100 && !Note.TapNoteGameObject.activeInHierarchy) Note.TapNoteGameObject.SetActive(true);
            }
        }
    }
    private void UpdateAllNoteColor()
    {
        foreach (Lanotalium.Chart.LanotaTapNote Note in TapNote)
        {
            if (!LimScanTime.IsTapNoteinScanRange(Note)) continue;
            if (Note.OnSelect) { if (Note.Sprite.color != OnSelectColor) Note.Sprite.color = OnSelectColor; }
            else if (!Note.OnSelect) { if (Note.Sprite.color != NormalColor) Note.Sprite.color = NormalColor; }
            if (LimSystem.Preferences.JudgeColor && !Note.OnSelect)
            {
                if (Tuner.ChartTime - Note.Time >= 0)
                {
                    if (Note.Sprite.color != OnJudgeColor) Note.Sprite.color = OnJudgeColor;
                }
            }
        }
    }
    private void UpdateNoteAudioEffect()
    {
        if (!LimSystem.Preferences.AudioEffect) return;
        foreach (Lanotalium.Chart.LanotaTapNote Note in TapNote)
        {
            if (!LimScanTime.IsTapNoteinScanRange(Note)) continue;
            if (Note.Time <= Tuner.ChartTime)
            {
                if (!Note.AudioEffectPlayed)
                {
                    switch (Note.Type)
                    {
                        case 0:
                        case 4:
                            if (Tuner.MediaPlayerManager.IsPlaying) Tuner.AudioEffectManager.PlayClick();
                            break;
                        case 2:
                            if (Tuner.MediaPlayerManager.IsPlaying) Tuner.AudioEffectManager.PlayFlickIn();
                            break;
                        case 3:
                            if (Tuner.MediaPlayerManager.IsPlaying) Tuner.AudioEffectManager.PlayFlickOut();
                            break;
                    }
                    Note.AudioEffectPlayed = true;
                }
            }
            else
            {
                Note.AudioEffectPlayed = false;
            }
        }
    }

    public void SortTapNoteList()
    {
        TapNote.Sort((Lanotalium.Chart.LanotaTapNote A, Lanotalium.Chart.LanotaTapNote B) =>
        {
            return A.Time.CompareTo(B.Time);
        });
    }
}