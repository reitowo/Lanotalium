using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimHoldNoteManager : MonoBehaviour
{
    private bool isInitialized = false;
    public float OnTouchWidthAdd = 0.1f;
    public List<Lanotalium.Chart.LanotaHoldNote> HoldNote;
    public LimTunerManager Tuner;
    public Material HoldUntouch, HoldTouch;
    public Color OnSelectColor, NormalColor;

    public GameObject cT5S0, cT5S1, cT5S2, cT5S3;
    public GameObject oT5S0, oT5S1, oT5S2, oT5S3;
    public GameObject oTJoint;

    void Update()
    {
        if (!isInitialized) return;
        UpdateAllNoteTransforms();
        UpdateAllLineMaterial();
        UpdateAllLineRenderers();
        UpdateAllNoteActive();
        UpdateAllJointActive();
        UpdateAllNoteColor();
        UpdateNoteAudioEffect();
    }

    public void Initialize(List<Lanotalium.Chart.LanotaHoldNote> HoldNoteData)
    {
        HoldNote = HoldNoteData;
        InstantiateNotes();
        isInitialized = true;
    }

    public GameObject GetPrefab(int Size, bool Combination)
    {
        if (Combination)
        {
            switch (Size)
            {
                case 0: return cT5S0;
                case 1: return cT5S1;
                case 2: return cT5S2;
                case 3: return cT5S3;
            }
        }
        else if (!Combination)
        {
            switch (Size)
            {
                case 0: return oT5S0;
                case 1: return oT5S1;
                case 2: return oT5S2;
                case 3: return oT5S3;
            }
        }
        return null;
    }
    public void InstantiateHeadNote(Lanotalium.Chart.LanotaHoldNote Note)
    {
        Note.HoldNoteGameObject = Instantiate(GetPrefab(Note.Size, Note.Combination), transform);
        Note.LineRenderer = Note.HoldNoteGameObject.GetComponentInChildren<LineRenderer>();
        Note.InstanceId = Note.HoldNoteGameObject.GetInstanceID();
        Note.Sprite = Note.HoldNoteGameObject.GetComponentInChildren<SpriteRenderer>();
        Note.OnTouch = false;
        Note.HoldNoteGameObject.SetActive(false);
    }
    public void InstantiateAllJointNote(Lanotalium.Chart.LanotaHoldNote Note)
    {
        if (Note.Joints != null)
        {
            for (int i = 0; i < Note.Joints.Count - 1; ++i)
            {
                Note.Joints[i].JointGameObject = Instantiate(oTJoint, transform);
                Note.Joints[i].InstanceId = Note.Joints[i].JointGameObject.GetInstanceID();
                Note.Joints[i].JointGameObject.SetActive(false);
            }
        }
    }
    public void InstantiateJointNote(Lanotalium.Chart.LanotaJoints Joint)
    {
        Joint.JointGameObject = Instantiate(oTJoint, transform);
        Joint.InstanceId = Joint.JointGameObject.GetInstanceID();
        Joint.JointGameObject.SetActive(false);
    }
    private void InstantiateNotes()
    {
        foreach (Lanotalium.Chart.LanotaHoldNote Note in HoldNote)
        {
            InstantiateHeadNote(Note);
            InstantiateAllJointNote(Note);
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
    private float CalculateEasedCurve(float Percent, int Mode)
    {
        if (Percent >= 1.0) return 1.0f;
        else if (Percent <= 0.0) return 0.0f;
        switch (Mode)
        {
            case 0:
                return Percent;
            case 1:
                return Percent * Percent * Percent * Percent;
            case 2:
                return -(Percent - 1) * (Percent - 1) * (Percent - 1) * (Percent - 1) + 1;
            case 3:
                return (Percent < 0.5) ? (Percent * Percent * Percent * Percent * 8) : ((Percent - 1) * (Percent - 1) * (Percent - 1) * (Percent - 1) * -8 + 1);
            case 4:
                return Percent * Percent * Percent;
            case 5:
                return (Percent - 1) * (Percent - 1) * (Percent - 1) + 1;
            case 6:
                return (Percent < 0.5) ? (Percent * Percent * Percent * 4) : ((Percent - 1) * (Percent - 1) * (Percent - 1) * 4 + 1);
            case 7:
                return Mathf.Pow(2, 10 * (float)(Percent - 1));
            case 8:
                return -Mathf.Pow(2, -10 * (float)Percent) + 1;
            case 9:
                return (Percent < 0.5) ? (Mathf.Pow(2, 10 * (2 * (float)Percent - 1)) / 2) : ((-Mathf.Pow(2, -10 * (2 * (float)Percent - 1)) + 2) / 2);
            case 10:
                return -Mathf.Cos((float)Percent * Mathf.PI / 2) + 1;
            case 11:
                return Mathf.Sin((float)Percent * Mathf.PI / 2);
            case 12:
                return (Mathf.Cos((float)Percent * Mathf.PI) - 1) / -2;
        }
        return 1;
    }
    private float CalculateHeadRotation(Lanotalium.Chart.LanotaHoldNote Note)
    {
        float LastaTime = Note.Time, LastaDegree = Note.Degree;
        if (Note.Joints != null)
        {
            foreach (Lanotalium.Chart.LanotaJoints Joint in Note.Joints)
            {
                if (Tuner.ChartTime > Joint.aTime)
                {
                    LastaTime = Joint.aTime;
                    LastaDegree = Joint.aDegree;
                    continue;
                }
                float Percent = (Tuner.ChartTime - LastaTime) / Joint.dTime;
                return Tuner.CameraManager.CurrentRotation + LastaDegree + Joint.dDegree * CalculateEasedCurve(Percent, Joint.Cfmi);
            }
            if (Note.Joints.Count != 0)
                return Tuner.CameraManager.CurrentRotation + Note.Joints[Note.Joints.Count - 1].aDegree;
        }
        return Tuner.CameraManager.CurrentRotation + Note.Degree;
    }
    private Vector3 CalculateLineRendererPoint(float Percent, float Degree)
    {
        return new Vector3(-Percent / 10 * Mathf.Sin(Degree * Mathf.Deg2Rad), 0, -Percent / 10 * Mathf.Cos(Degree * Mathf.Deg2Rad));
    }

    private void UpdateJointTransform(Lanotalium.Chart.LanotaHoldNote Note)
    {
        float aTime = Note.Time;
        float aDegree = Note.Degree + Tuner.CameraManager.CurrentRotation;
        if (Note.Joints != null)
        {
            for (int i = 0; i < Note.Joints.Count - 1; ++i)
            {
                Lanotalium.Chart.LanotaJoints Joint = Note.Joints[i];
                aTime += Joint.dTime;
                aDegree += Joint.dDegree;
                float Percent = CalculateMovePercent(aTime);
                Percent = CalculateEasedPercent(Percent);
                Joint.Percent = Percent;
                if (Percent < 20) continue;
                Joint.JointGameObject.transform.rotation = Quaternion.Euler(new Vector3(90, aDegree, 0));
                Joint.JointGameObject.transform.position = new Vector3(-Percent / 10 * Mathf.Sin(aDegree * Mathf.Deg2Rad), 0, -Percent / 10 * Mathf.Cos(aDegree * Mathf.Deg2Rad));
                Joint.JointGameObject.transform.localScale = new Vector3(Percent / 100, Percent / 100, 0);
                Joint.aTime = aTime;
                Joint.aDegree = aDegree - Tuner.CameraManager.CurrentRotation;
            }
        }
    }
    private void UpdateAllNoteTransforms()
    {
        foreach (Lanotalium.Chart.LanotaHoldNote Note in HoldNote)
        {
            if (!LimScanTime.IsHoldNoteinScanRange(Note))
            {
                if (!Tuner.ScrollManager.IsStopped(Tuner.ChartTime))
                {
                    continue;
                }
            }
            float Percent = CalculateMovePercent(Note.Time);
            Percent = CalculateEasedPercent(Percent);
            Note.Percent = Percent;
            UpdateJointTransform(Note);
            if (Percent < 20) continue;
            float RotatedDegree = CalculateHeadRotation(Note);
            Note.HoldNoteGameObject.transform.rotation = Quaternion.Euler(new Vector3(90, RotatedDegree, 0));
            Note.HoldNoteGameObject.transform.position = new Vector3(-Percent / 10 * Mathf.Sin(RotatedDegree * Mathf.Deg2Rad), 0, -Percent / 10 * Mathf.Cos(RotatedDegree * Mathf.Deg2Rad));
            Note.HoldNoteGameObject.transform.localScale = new Vector3(Percent / 100, Percent / 100, 0);
            Note.FinalDegree = RotatedDegree;
        }
    }
    private void UpdateAllLineRenderers()
    {
        foreach (Lanotalium.Chart.LanotaHoldNote Note in HoldNote)
        {
            if (!LimScanTime.IsHoldNoteinScanRange(Note))
            {
                if (!Tuner.ScrollManager.IsStopped(Tuner.ChartTime))
                { 
                    continue;
                }
            }
            if (Note.Percent < 20) continue;
            if (Note.Jcount == 0)
            {
                float Rotation = Note.Degree + Tuner.CameraManager.CurrentRotation;
                float EndPercent = CalculateMovePercent(Note.Time + Note.Duration);
                Note.LineRenderer.positionCount = 10;
                Note.LineRenderer.startWidth = Note.Percent / 100 + (Note.OnTouch ? OnTouchWidthAdd : 0);
                Note.LineRenderer.endWidth = CalculateEasedPercent(EndPercent) / 100 + (Note.OnTouch ? OnTouchWidthAdd : 0);
                Vector3 Start = CalculateLineRendererPoint(Note.Percent, Rotation);
                Vector3 End = CalculateLineRendererPoint(CalculateEasedPercent(EndPercent), Rotation);
                Vector3 Delta = (End - Start) / 9;
                for (int i = 0; i < 10; ++i) Note.LineRenderer.SetPosition(i, Start + i * Delta);
            }
            else
            {
                float EndPercent = 0;
                List<Vector3> Positions = new List<Vector3>();
                float LastaTime = Note.Time, LastaDegree = Note.Degree;
                for (int i = 0; i < Note.Joints.Count; ++i)
                {
                    float Interval = Mathf.Clamp(Mathf.Abs(Note.Joints[i].dTime / (Note.Joints[i].dDegree == 0 ? 1 : Note.Joints[i].dDegree) * 10), 0.0001f, 0.01f);
                    for (float t = LastaTime > Tuner.ChartTime ? LastaTime : Tuner.ChartTime; t < Note.Joints[i].aTime; t += Interval)
                    {
                        float Percent = (t - LastaTime) / Note.Joints[i].dTime;
                        float LocationPercent = CalculateMovePercent(t);
                        EndPercent = CalculateEasedPercent(LocationPercent);
                        if (Tuner.ScrollManager.IsBackwarding(Tuner.ChartTime))
                        {
                            if (EndPercent != 100)
                                Positions.Add(CalculateLineRendererPoint(EndPercent, Tuner.CameraManager.CurrentRotation + LastaDegree + Note.Joints[i].dDegree * CalculateEasedCurve(Percent, Note.Joints[i].Cfmi)));
                        }
                        else Positions.Add(CalculateLineRendererPoint(EndPercent, Tuner.CameraManager.CurrentRotation + LastaDegree + Note.Joints[i].dDegree * CalculateEasedCurve(Percent, Note.Joints[i].Cfmi)));
                        if (t + Interval > Note.Joints[i].aTime)
                        {
                            float TempPercent = CalculateMovePercent(Note.Joints[i].aTime);
                            if (Tuner.ScrollManager.IsBackwarding(Tuner.ChartTime))
                            {
                                if (TempPercent != 100)
                                    Positions.Add(CalculateLineRendererPoint(CalculateEasedPercent(TempPercent), Tuner.CameraManager.CurrentRotation + LastaDegree + Note.Joints[i].dDegree));
                            }
                            else Positions.Add(CalculateLineRendererPoint(CalculateEasedPercent(TempPercent), Tuner.CameraManager.CurrentRotation + LastaDegree + Note.Joints[i].dDegree));
                            break;
                        }
                    }
                    LastaTime = Note.Joints[i].aTime;
                    LastaDegree = Note.Joints[i].aDegree;
                }
                Note.LineRenderer.positionCount = Positions.Count;
                Note.LineRenderer.SetPositions(Positions.ToArray());
                Note.LineRenderer.startWidth = Note.Percent / 100 + (Note.OnTouch ? OnTouchWidthAdd : 0);
                Note.LineRenderer.endWidth = EndPercent / 100 + (Note.OnTouch ? OnTouchWidthAdd : 0);
            }
        }
    }
    private void UpdateAllNoteActive()
    {
        foreach (Lanotalium.Chart.LanotaHoldNote Note in HoldNote)
        {
            if (!LimScanTime.IsHoldNoteinScanRange(Note))
            {
                if (!Tuner.ScrollManager.IsStopped(Tuner.ChartTime))
                {
                    if (Note.HoldNoteGameObject.activeInHierarchy) Note.HoldNoteGameObject.SetActive(false);
                    continue;
                }
            }
            if (Tuner.ChartTime > Note.Time + Note.Duration && Note.HoldNoteGameObject.activeInHierarchy) Note.HoldNoteGameObject.SetActive(false);
            else if (Tuner.ChartTime < Note.Time)
            {
                if (Note.Percent <= 20 || Note.Percent >= 100)
                {
                    if (Tuner.ScrollManager.IsBackwarding(Tuner.ChartTime))
                    {
                        if (Note.Percent == 100 && !Note.HoldNoteGameObject.activeInHierarchy)
                        {
                            Note.HoldNoteGameObject.SetActive(true);
                            Note.SetSpritesActive(false);
                        }
                    }
                    else if (Note.HoldNoteGameObject.activeInHierarchy) Note.HoldNoteGameObject.SetActive(false);
                }
                else
                {
                    Note.SetSpritesActive(true);
                    if (!Note.HoldNoteGameObject.activeInHierarchy) Note.HoldNoteGameObject.SetActive(true);
                }
            }
            else if (Tuner.ChartTime >= Note.Time && Tuner.ChartTime <= Note.Time + Note.Duration)
            {
                if (Note.Percent == 100 && !Note.HoldNoteGameObject.activeInHierarchy) Note.HoldNoteGameObject.SetActive(true);
            }
        }
    }
    private void UpdateAllJointActive()
    {
        foreach (Lanotalium.Chart.LanotaHoldNote Note in HoldNote)
        {
            if (!LimScanTime.IsHoldNoteinScanRange(Note))
            {
                if (!Tuner.ScrollManager.IsStopped(Tuner.ChartTime))
                {
                    if (Note.Joints == null) continue;
                    for (int i = 0; i < Note.Joints.Count - 1; ++i)
                    {
                        Lanotalium.Chart.LanotaJoints Joint = Note.Joints[i];
                        if (Joint.JointGameObject.activeInHierarchy) Joint.JointGameObject.SetActive(false);
                    }
                    continue;
                }
            }
            if (Note.Joints == null) continue;
            for (int i = 0; i < Note.Joints.Count - 1; ++i)
            {
                Lanotalium.Chart.LanotaJoints Joint = Note.Joints[i];
                if (Tuner.ChartTime > Joint.aTime && Joint.JointGameObject.activeInHierarchy) Joint.JointGameObject.SetActive(false);
                else if (Tuner.ChartTime < Joint.aTime)
                {
                    if (Joint.Percent <= 20 || Joint.Percent >= 100)
                    {
                        if (Joint.JointGameObject.activeInHierarchy) Joint.JointGameObject.SetActive(false);
                    }
                    else
                    {
                        if (!Joint.JointGameObject.activeInHierarchy) Joint.JointGameObject.SetActive(true);
                    }
                }
            }
        }
    }
    private void UpdateAllLineMaterial()
    {
        foreach (Lanotalium.Chart.LanotaHoldNote Note in HoldNote)
        {
            if (!LimScanTime.IsHoldNoteinScanRange(Note))
            {
                if (!Tuner.ScrollManager.IsStopped(Tuner.ChartTime))
                {
                    continue;
                }
            }
            if (Note.Percent < 20) continue;
            if (Tuner.ChartTime >= Note.Time && Tuner.ChartTime < Note.Time + Note.Duration)
            {
                if (!Note.OnTouch)
                {
                    Note.LineRenderer.material = HoldTouch;
                    Note.OnTouch = true;
                }
            }
            else
            {
                if (Note.OnTouch)
                {
                    Note.LineRenderer.material = HoldUntouch;
                    Note.OnTouch = false;
                }
            }
        }
    }
    private void UpdateAllNoteColor()
    {
        foreach (Lanotalium.Chart.LanotaHoldNote Note in HoldNote)
        {
            if (!LimScanTime.IsHoldNoteinScanRange(Note))
            {
                if (!Tuner.ScrollManager.IsStopped(Tuner.ChartTime))
                {
                    continue;
                }
            }
            if (Note.OnSelect) { if (Note.Sprite.color == NormalColor) Note.Sprite.color = OnSelectColor; }
            else if (!Note.OnSelect) { if (Note.Sprite.color == OnSelectColor) Note.Sprite.color = NormalColor; }
        }
    }
    private void UpdateNoteAudioEffect()
    {
        if (!LimSystem.Preferences.AudioEffect) return;
        bool ShouldPlayRail = false;
        foreach (Lanotalium.Chart.LanotaHoldNote Note in HoldNote)
        {
            if (!LimScanTime.IsHoldNoteinScanRange(Note))
            {
                if (!Tuner.ScrollManager.IsStopped(Tuner.ChartTime))
                {
                    continue;
                }
            }
            if (Note.Time + Note.Duration <= Tuner.ChartTime)
            {
                if (!Note.EndEffectPlayed)
                {
                    if (Tuner.MediaPlayerManager.IsPlaying) Tuner.AudioEffectManager.PlayRailEnd();
                    Note.EndEffectPlayed = true;
                }
            }
            else if (Note.Time <= Tuner.ChartTime && Note.Time + Note.Duration > Tuner.ChartTime)
            {
                if (!Note.StartEffectPlayed)
                {
                    if (Tuner.MediaPlayerManager.IsPlaying) Tuner.AudioEffectManager.PlayClick();
                    Note.StartEffectPlayed = true;
                }
                ShouldPlayRail = true;
                Note.EndEffectPlayed = false;
            }
            else if (Note.Time > Tuner.ChartTime)
            {
                Note.StartEffectPlayed = false;
                Note.EndEffectPlayed = false;
            }
        }
        if (Tuner.MediaPlayerManager.IsPlaying)
        {
            if (ShouldPlayRail) Tuner.AudioEffectManager.StartPlayRail();
            else Tuner.AudioEffectManager.StopPlayRail();
        }
        else Tuner.AudioEffectManager.StopPlayRail();
    }

    public void SortHoldNoteList()
    {
        HoldNote.Sort((Lanotalium.Chart.LanotaHoldNote A, Lanotalium.Chart.LanotaHoldNote B) =>
        {
            return A.Time.CompareTo(B.Time);
        });
    }
}
