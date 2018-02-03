using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UnityEngine.Events;

namespace Lanotalium
{
    namespace Json
    {
        [System.Serializable]
        public class j
        {
            public int idx;
            public float d_deg;
            public float d_time;
            public int d_e;
        }
        [System.Serializable]
        public class joints
        {
            public int j_count;
            public List<j> j;
        }
        [System.Serializable]
        public class events
        {
            public int Id;
            public int Type;
            public float Timing;
            public float Duration;
            public float ctp;
            public float ctp1;
            public float ctp2;
            public int cfmi;
            public bool cflg;
            public float Degree;
            public int Size;
            public bool Critical;
            public bool Combination;
            public float Bpm;
            public joints joints;
        }
        [System.Serializable]
        public class bpm
        {
            public int Id;
            public int Type;
            public float Timing;
            public float Duration;
            public float Degree;
            public int Size;
            public bool Critical;
            public bool Combination;
            public float Bpm;
        }
        [System.Serializable]
        public class scroll
        {
            public float speed;
            public float timing;
        }
        [System.Serializable]
        public class LnmReadJson
        {
            public List<events> events;
            public float eos;
            public List<bpm> bpm;
            public List<scroll> scroll;
        }
    }
    namespace Chart
    {
        public class LanotaDefault
        {
            public float Time = -3f;
            public float Duration = 0;
            public float CamRou;
            public float CamTheta;
            public float CamHeight = -20f;
            public float Cfmi, Degree;
            public bool Cflg;
        };
        public class LanotaTapNote
        {
            public int Type;
            public float Time;
            public float Duration;
            public float Degree;
            public int Size;
            public bool Critical;
            public bool Combination;
            public float Bpm;

            public bool AudioEffectPlayed;
            public bool OnSelect;
            public float Percent;
            public GameObject TapNoteGameObject;
            public int InstanceId;
            public SpriteRenderer Sprite;

            public LanotaHoldNote ToHoldNote()
            {
                LanotaHoldNote HoldNote = new LanotaHoldNote();
                HoldNote.Bpm = Bpm;
                HoldNote.Combination = Combination;
                HoldNote.Critical = Critical;
                HoldNote.Degree = Degree;
                HoldNote.Duration = 1;
                HoldNote.Jcount = 0;
                HoldNote.Joints = null;
                HoldNote.OnSelect = false;
                HoldNote.Size = Size;
                HoldNote.Time = Time;
                HoldNote.Type = 5;
                return HoldNote;
            }
            public LanotaTapNote DeepCopy()
            {
                LanotaTapNote New = new LanotaTapNote();
                New.Type = Type;
                New.Time = Time;
                New.Duration = Duration;
                New.Degree = Degree;
                New.Size = Size;
                New.Critical = Critical;
                New.Combination = Combination;
                New.Bpm = Bpm;
                return New;
            }
        };
        public class LanotaJoints
        {
            public int Cfmi;
            public float dDegree;
            public float dTime;
            public float aDegree;
            public float aTime;

            public float Percent;
            public GameObject JointGameObject;
            public int InstanceId;

            public LanotaJoints DeepCopy()
            {
                LanotaJoints New = new LanotaJoints();
                New.Cfmi = Cfmi;
                New.dDegree = dDegree;
                New.dTime = dTime;
                return New;
            }
        };
        public class LanotaHoldNote
        {
            public int Type;
            public float Time;
            public float Duration;
            public int Jcount;
            public List<LanotaJoints> Joints;
            public float Degree;
            public int Size;
            public bool Critical;
            public bool Combination;
            public float Bpm;
            public float FinalDegree;

            public bool StartEffectPlayed;
            public bool EndEffectPlayed;
            public bool OnTouch;
            public bool OnSelect;
            public float Percent;
            public GameObject HoldNoteGameObject;
            public LineRenderer LineRenderer;
            public int InstanceId;
            public SpriteRenderer Sprite;

            public void SetSpritesActive(bool Active)
            {
                foreach(SpriteRenderer s in HoldNoteGameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    s.enabled = Active;
                }
            }
            public LanotaTapNote ToTapNote(int Type)
            {
                LanotaTapNote TapNote = new LanotaTapNote();
                TapNote.Bpm = Bpm;
                TapNote.Combination = Combination;
                TapNote.Critical = Critical;
                TapNote.Degree = Degree;
                TapNote.Duration = Duration;
                TapNote.OnSelect = false;
                TapNote.Size = Size;
                TapNote.Time = Time;
                TapNote.Type = Type;
                return TapNote;
            }
            public LanotaHoldNote DeepCopy()
            {
                LanotaHoldNote New = new LanotaHoldNote();
                New.Type = Type;
                New.Time = Time;
                New.Duration = Duration;
                New.Jcount = Jcount;
                if (Joints != null)
                {
                    New.Joints = new List<LanotaJoints>();
                    foreach (LanotaJoints j in Joints)
                    {
                        New.Joints.Add(j.DeepCopy());
                    }
                }
                New.Degree = Degree;
                New.Size = Size;
                New.Critical = Critical;
                New.Combination = Combination;
                New.Bpm = Bpm;
                return New;
            }
        };
        public class LanotaChangeBpm
        {
            public int Type;
            public float Time;
            public float Duration;
            public float Degree;
            public int Size;
            public bool Critical;
            public bool Combination;
            public float Bpm;

            public GameObject ListGameObject;
            public int InstanceId;
        };
        public class LanotaCameraBase
        {
            public int Type;
            public float Time;
            public float Duration;
            public float ctp, ctp1, ctp2;
            public int cfmi;
            public bool cflg;

            public GameObject TimeLineGameObject;
            public int InstanceId;
        }
        public class LanotaCameraXZ : LanotaCameraBase
        {
            public LanotaCameraXZ DeepCopy()
            {
                LanotaCameraXZ New = new LanotaCameraXZ
                {
                    Type = Type,
                    Time = Time,
                    Duration = Duration,
                    ctp = ctp,
                    ctp1 = ctp1,
                    ctp2 = ctp2,
                    cfmi = cfmi,
                    cflg = cflg
                };
                return New;
            }
        };
        public class LanotaCameraY : LanotaCameraBase
        {
            public LanotaCameraY DeepCopy()
            {
                LanotaCameraY New = new LanotaCameraY
                {
                    Type = Type,
                    Time = Time,
                    Duration = Duration,
                    ctp = ctp,
                    ctp1 = ctp1,
                    ctp2 = ctp2,
                    cfmi = cfmi,
                    cflg = cflg
                };
                return New;
            }
        };
        public class LanotaCameraRot : LanotaCameraBase
        {
            public LanotaCameraRot DeepCopy()
            {
                LanotaCameraRot New = new LanotaCameraRot
                {
                    Type = Type,
                    Time = Time,
                    Duration = Duration,
                    ctp = ctp,
                    ctp1 = ctp1,
                    ctp2 = ctp2,
                    cfmi = cfmi,
                    cflg = cflg
                };
                return New;
            }
        };
        public class LanotaScroll
        {
            public float Speed;
            public float Time;

            public GameObject ListGameObject;
            public int InstanceId;
        };
        public class ChartData
        {
            public List<LanotaTapNote> LanotaTapNote;
            public List<LanotaHoldNote> LanotaHoldNote;
            public List<LanotaChangeBpm> LanotaChangeBpm;
            public List<LanotaCameraXZ> LanotaCameraXZ;
            public List<LanotaCameraY> LanotaCameraY;
            public List<LanotaCameraRot> LanotaCameraRot;
            public List<LanotaScroll> LanotaScroll;
            public LanotaDefault LanotaDefault;
            public float SongLength;
            public ChartData(string Text)
            {
                Json.LnmReadJson MidJson;
                MidJson = JsonConvert.DeserializeObject<Json.LnmReadJson>(Text);
                if (MidJson == null) MidJson = JsonConvert.DeserializeObject<Json.LnmReadJson>("{\"events\":null,\"eos\":0,\"bpm\":null,\"scroll\":null}");
                LanotaCameraRot = new List<LanotaCameraRot>();
                LanotaCameraXZ = new List<LanotaCameraXZ>();
                LanotaCameraY = new List<LanotaCameraY>();
                LanotaChangeBpm = new List<LanotaChangeBpm>();
                LanotaDefault = new LanotaDefault();
                LanotaHoldNote = new List<LanotaHoldNote>();
                LanotaScroll = new List<LanotaScroll>();
                LanotaTapNote = new List<LanotaTapNote>();
                //Events
                if (MidJson.events != null)
                {
                    for (int i = 0; i < MidJson.events.Count; ++i)
                    {
                        int Type = MidJson.events[i].Type;
                        if (Type == 0 || Type == 2 || Type == 3 || Type == 4)
                        {
                            LanotaTapNote TmpTap = new LanotaTapNote();
                            TmpTap.Type = Type;
                            TmpTap.Time = MidJson.events[i].Timing;
                            TmpTap.Duration = MidJson.events[i].Duration;
                            TmpTap.Degree = MidJson.events[i].Degree;
                            TmpTap.Size = MidJson.events[i].Size;
                            TmpTap.Critical = MidJson.events[i].Critical;
                            TmpTap.Combination = MidJson.events[i].Combination;
                            TmpTap.Bpm = MidJson.events[i].Bpm;
                            LanotaTapNote.Add(TmpTap);
                        }
                        else if (Type == 5)
                        {
                            LanotaHoldNote TmpHold = new LanotaHoldNote();
                            TmpHold.Type = Type;
                            TmpHold.Time = MidJson.events[i].Timing;
                            TmpHold.Duration = MidJson.events[i].Duration;
                            TmpHold.Jcount = MidJson.events[i].joints.j_count;
                            TmpHold.Degree = MidJson.events[i].Degree;
                            TmpHold.Combination = MidJson.events[i].Combination;
                            if (TmpHold.Jcount != 0)
                            {
                                TmpHold.Joints = new List<LanotaJoints>();
                                for (int j = 0; j < MidJson.events[i].joints.j.Count; ++j)
                                {
                                    LanotaJoints JTmpHold = new LanotaJoints();
                                    JTmpHold.dDegree = MidJson.events[i].joints.j[j].d_deg;
                                    JTmpHold.dTime = MidJson.events[i].joints.j[j].d_time;
                                    float TmpDegree = TmpHold.Degree + JTmpHold.dDegree;
                                    float TmpTime = TmpHold.Time + JTmpHold.dTime;
                                    for (int k = 0; k < j; ++k)
                                    {
                                        TmpDegree += TmpHold.Joints[k].dDegree;
                                        TmpTime += TmpHold.Joints[k].dTime;
                                    }
                                    JTmpHold.aDegree = TmpDegree;
                                    JTmpHold.aTime = TmpTime;
                                    JTmpHold.Cfmi = MidJson.events[i].joints.j[j].d_e;
                                    TmpHold.Joints.Add(JTmpHold);
                                }
                            }
                            TmpHold.Size = MidJson.events[i].Size;
                            TmpHold.Critical = MidJson.events[i].Critical;
                            TmpHold.Bpm = MidJson.events[i].Bpm;
                            LanotaHoldNote.Add(TmpHold);
                        }
                        else if (Type == 8 || Type == 11)
                        {
                            LanotaCameraXZ TmpCam = new LanotaCameraXZ();
                            TmpCam.Type = Type;
                            TmpCam.Time = MidJson.events[i].Timing;
                            TmpCam.Duration = MidJson.events[i].Duration;
                            TmpCam.ctp = MidJson.events[i].ctp;
                            TmpCam.ctp1 = MidJson.events[i].ctp1;
                            TmpCam.ctp2 = MidJson.events[i].ctp2;
                            TmpCam.cfmi = MidJson.events[i].cfmi;
                            TmpCam.cflg = MidJson.events[i].cflg;
                            LanotaCameraXZ.Add(TmpCam);
                        }
                        else if (Type == 10)
                        {
                            LanotaCameraY TmpCam = new LanotaCameraY();
                            TmpCam.Type = Type;
                            TmpCam.Time = MidJson.events[i].Timing;
                            TmpCam.Duration = MidJson.events[i].Duration;
                            TmpCam.ctp = MidJson.events[i].ctp;
                            TmpCam.ctp1 = MidJson.events[i].ctp1;
                            TmpCam.ctp2 = MidJson.events[i].ctp2;
                            TmpCam.cfmi = MidJson.events[i].cfmi;
                            TmpCam.cflg = MidJson.events[i].cflg;
                            LanotaCameraY.Add(TmpCam);
                        }
                        else if (Type == 12)
                        {
                            LanotaDefault.CamRou = MidJson.events[i].ctp;
                            LanotaDefault.CamTheta = MidJson.events[i].ctp1;
                            LanotaDefault.CamHeight = MidJson.events[i].ctp2;
                            LanotaDefault.Degree = MidJson.events[i].Degree;
                        }
                        else if (Type == 13)
                        {
                            LanotaCameraRot TmpCam = new LanotaCameraRot();
                            TmpCam.Type = Type;
                            TmpCam.Time = MidJson.events[i].Timing;
                            TmpCam.Duration = MidJson.events[i].Duration;
                            TmpCam.ctp = MidJson.events[i].ctp * -1;
                            TmpCam.ctp1 = MidJson.events[i].ctp1;
                            TmpCam.ctp2 = MidJson.events[i].ctp2;
                            TmpCam.cfmi = MidJson.events[i].cfmi;
                            TmpCam.cflg = MidJson.events[i].cflg;
                            LanotaCameraRot.Add(TmpCam);
                        }
                    }
                }
                //Bpm
                if (MidJson.bpm != null)
                {
                    if (MidJson.bpm.Count != 0)
                    {
                        LanotaChangeBpm FirstTmpBpm = new LanotaChangeBpm();
                        FirstTmpBpm.Type = MidJson.bpm[0].Type;
                        FirstTmpBpm.Time = -3;
                        FirstTmpBpm.Duration = MidJson.bpm[0].Duration;
                        FirstTmpBpm.Degree = MidJson.bpm[0].Degree;
                        FirstTmpBpm.Size = MidJson.bpm[0].Size;
                        FirstTmpBpm.Critical = MidJson.bpm[0].Critical;
                        FirstTmpBpm.Combination = MidJson.bpm[0].Combination;
                        FirstTmpBpm.Bpm = MidJson.bpm[0].Bpm;
                        LanotaChangeBpm.Add(FirstTmpBpm);
                        for (int i = 1; i < MidJson.bpm.Count; ++i)
                        {
                            LanotaChangeBpm TmpBpm = new LanotaChangeBpm();
                            TmpBpm.Type = MidJson.bpm[i].Type;
                            TmpBpm.Time = MidJson.bpm[i].Timing;
                            TmpBpm.Duration = MidJson.bpm[i].Duration;
                            TmpBpm.Degree = MidJson.bpm[i].Degree;
                            TmpBpm.Size = MidJson.bpm[i].Size;
                            TmpBpm.Critical = MidJson.bpm[i].Critical;
                            TmpBpm.Combination = MidJson.bpm[i].Combination;
                            TmpBpm.Bpm = MidJson.bpm[i].Bpm;
                            LanotaChangeBpm.Add(TmpBpm);
                        }
                    }
                    else
                    {
                        LanotaChangeBpm FirstTmpBpm = new LanotaChangeBpm();
                        FirstTmpBpm.Type = 6;
                        FirstTmpBpm.Time = -3;
                        FirstTmpBpm.Bpm = 100;
                        LanotaChangeBpm.Add(FirstTmpBpm);
                    }
                }
                else
                {
                    LanotaChangeBpm FirstTmpBpm = new LanotaChangeBpm();
                    FirstTmpBpm.Type = 6;
                    FirstTmpBpm.Time = -3;
                    FirstTmpBpm.Bpm = 100;
                    LanotaChangeBpm.Add(FirstTmpBpm);
                }
                //Scroll
                if (MidJson.scroll != null)
                {
                    if (MidJson.scroll.Count != 0)
                    {
                        LanotaScroll TmpScrollFirst = new LanotaScroll();
                        TmpScrollFirst.Speed = MidJson.scroll[0].speed;
                        TmpScrollFirst.Time = -10;
                        LanotaScroll.Add(TmpScrollFirst);
                        for (int i = 1; i < MidJson.scroll.Count; ++i)
                        {
                            LanotaScroll TmpScroll = new LanotaScroll();
                            TmpScroll.Speed = MidJson.scroll[i].speed;
                            TmpScroll.Time = MidJson.scroll[i].timing;
                            LanotaScroll.Add(TmpScroll);
                        }
                    }
                    else
                    {
                        LanotaScroll TmpScrollFirst = new LanotaScroll();
                        TmpScrollFirst.Speed = 1;
                        TmpScrollFirst.Time = -10;
                        LanotaScroll.Add(TmpScrollFirst);
                    }
                }
                else
                {
                    LanotaScroll TmpScrollFirst = new LanotaScroll();
                    TmpScrollFirst.Speed = 1;
                    TmpScrollFirst.Time = -10;
                    LanotaScroll.Add(TmpScrollFirst);
                }
                SongLength = MidJson.eos;
            }
            public override string ToString()
            {
                Json.LnmReadJson OutputTmp = new Json.LnmReadJson();
                OutputTmp.events = new List<Json.events>();
                Json.events defaulttmp = new Json.events();
                defaulttmp.Type = 12;
                defaulttmp.Timing = -3;
                defaulttmp.ctp = LanotaDefault.CamRou;
                defaulttmp.ctp1 = LanotaDefault.CamTheta;
                defaulttmp.ctp2 = LanotaDefault.CamHeight;
                defaulttmp.Degree = LanotaDefault.Degree;
                OutputTmp.events.Add(defaulttmp);
                for (int i = 0; i < LanotaTapNote.Count; ++i)
                {
                    Json.events eventtmp = new Json.events();
                    eventtmp.Type = LanotaTapNote[i].Type;
                    eventtmp.Timing = LanotaTapNote[i].Time;
                    eventtmp.Duration = LanotaTapNote[i].Duration;
                    eventtmp.Degree = LanotaTapNote[i].Degree;
                    eventtmp.Size = LanotaTapNote[i].Size;
                    eventtmp.Critical = LanotaTapNote[i].Critical;
                    eventtmp.Combination = LanotaTapNote[i].Combination;
                    eventtmp.Bpm = LanotaTapNote[i].Bpm;
                    OutputTmp.events.Add(eventtmp);
                }
                for (int i = 0; i < LanotaHoldNote.Count; ++i)
                {
                    Json.events eventtmp = new Json.events();
                    eventtmp.Type = LanotaHoldNote[i].Type;
                    eventtmp.Timing = LanotaHoldNote[i].Time;
                    eventtmp.Duration = LanotaHoldNote[i].Duration;
                    eventtmp.Combination = LanotaHoldNote[i].Combination;
                    eventtmp.joints = new Json.joints();
                    eventtmp.joints.j_count = LanotaHoldNote[i].Jcount;
                    eventtmp.Degree = LanotaHoldNote[i].Degree;
                    eventtmp.joints.j = new List<Json.j>();
                    if (eventtmp.joints.j_count != 0)
                    {
                        for (int k = 0; k < eventtmp.joints.j_count; ++k)
                        {
                            Json.j jTmp = new Json.j();
                            jTmp.idx = k;
                            jTmp.d_deg = LanotaHoldNote[i].Joints[k].dDegree;
                            jTmp.d_time = LanotaHoldNote[i].Joints[k].dTime;
                            jTmp.d_e = LanotaHoldNote[i].Joints[k].Cfmi;
                            eventtmp.joints.j.Add(jTmp);
                        }
                    }
                    eventtmp.Size = LanotaHoldNote[i].Size;
                    eventtmp.Critical = LanotaHoldNote[i].Critical;
                    eventtmp.Bpm = LanotaHoldNote[i].Bpm;
                    OutputTmp.events.Add(eventtmp);
                }
                for (int i = 0; i < LanotaCameraXZ.Count; ++i)
                {
                    Json.events eventtmp = new Json.events();
                    eventtmp.Type = LanotaCameraXZ[i].Type;
                    eventtmp.Timing = LanotaCameraXZ[i].Time;
                    eventtmp.Duration = LanotaCameraXZ[i].Duration;
                    eventtmp.ctp = LanotaCameraXZ[i].ctp;
                    eventtmp.ctp1 = LanotaCameraXZ[i].ctp1;
                    eventtmp.ctp2 = LanotaCameraXZ[i].ctp2;
                    eventtmp.cfmi = LanotaCameraXZ[i].cfmi;
                    eventtmp.cflg = LanotaCameraXZ[i].cflg;
                    OutputTmp.events.Add(eventtmp);
                }
                for (int i = 0; i < LanotaCameraY.Count; ++i)
                {
                    Json.events eventtmp = new Json.events();
                    eventtmp.Type = LanotaCameraY[i].Type;
                    eventtmp.Timing = LanotaCameraY[i].Time;
                    eventtmp.Duration = LanotaCameraY[i].Duration;
                    eventtmp.ctp = LanotaCameraY[i].ctp;
                    eventtmp.ctp1 = LanotaCameraY[i].ctp1;
                    eventtmp.ctp2 = LanotaCameraY[i].ctp2;
                    eventtmp.cfmi = LanotaCameraY[i].cfmi;
                    eventtmp.cflg = LanotaCameraY[i].cflg;
                    OutputTmp.events.Add(eventtmp);
                }
                for (int i = 0; i < LanotaCameraRot.Count; ++i)
                {
                    Json.events eventtmp = new Json.events();
                    eventtmp.Type = LanotaCameraRot[i].Type;
                    eventtmp.Timing = LanotaCameraRot[i].Time;
                    eventtmp.Duration = LanotaCameraRot[i].Duration;
                    eventtmp.ctp = LanotaCameraRot[i].ctp * -1;
                    eventtmp.ctp1 = LanotaCameraRot[i].ctp1;
                    eventtmp.ctp2 = LanotaCameraRot[i].ctp2;
                    eventtmp.cfmi = LanotaCameraRot[i].cfmi;
                    eventtmp.cflg = LanotaCameraRot[i].cflg;
                    OutputTmp.events.Add(eventtmp);
                }
                OutputTmp.events.Sort(delegate (Json.events a, Json.events b)
                {
                    return a.Timing.CompareTo(b.Timing);
                });
                OutputTmp.eos = SongLength;
                OutputTmp.bpm = new List<Json.bpm>();
                for (int i = 0; i < LanotaChangeBpm.Count; ++i)
                {
                    Json.bpm bpmtmp = new Json.bpm();
                    bpmtmp.Type = LanotaChangeBpm[i].Type;
                    bpmtmp.Timing = LanotaChangeBpm[i].Time;
                    bpmtmp.Duration = LanotaChangeBpm[i].Duration; // Contains offset !
                    bpmtmp.Degree = LanotaChangeBpm[i].Degree;
                    bpmtmp.Size = LanotaChangeBpm[i].Size;
                    bpmtmp.Critical = LanotaChangeBpm[i].Critical;
                    bpmtmp.Combination = LanotaChangeBpm[i].Combination;
                    bpmtmp.Bpm = LanotaChangeBpm[i].Bpm;
                    OutputTmp.bpm.Add(bpmtmp);
                }
                OutputTmp.scroll = new List<Json.scroll>();
                for (int i = 0; i < LanotaScroll.Count; ++i)
                {
                    Json.scroll scrolltmp = new Json.scroll();
                    scrolltmp.speed = LanotaScroll[i].Speed;
                    scrolltmp.timing = LanotaScroll[i].Time;
                    OutputTmp.scroll.Add(scrolltmp);
                }

                string JsonFile = JsonConvert.SerializeObject(OutputTmp);
                return JsonFile;
            }
        }
        public class ChartProperty
        {
            public string ChartPath;
            public string ChartFolder;
            public string ChartName;
            public ChartProperty(string ChartPath)
            {
                this.ChartPath = ChartPath;
                ChartFolder = Directory.GetParent(ChartPath).FullName.Replace('\\', '/');
                ChartName = new DirectoryInfo(ChartFolder).Name;
            }
        }
        public class ChartLoadResult
        {
            public bool isMusicLoaded;
            public bool isBackgroundLoaded;
            public bool isBackgroundGrayLoaded;
            public bool isBackgroundLinearLoaded;
            public bool isBackgroundVideoDetected;
            public bool isChartLoaded;
        }
        public class ChartBackground
        {
            public Sprite Linear, Gray, Color;
            public string VideoPath;
        }
        public class ChartMusic
        {
            public AudioClip Music;
            public float Length;
            public ChartMusic(AudioClip Music)
            {
                this.Music = Music;
                Length = Music.length;
            }
        }
    }
    namespace Tuner
    {
        public enum AudioEffectTheme
        {
            Lanota,
            Stellights
        }
    }
    namespace MediaPlayer
    {
        public class OnPauseEvent : UnityEvent<float>
        {

        }
        public class OnPlayEvent : UnityEvent<float>
        {

        }
        public class OnMusicLoadEvent : UnityEvent<AudioClip>
        {

        }
        public delegate void SyncValuesDelegate();
        public delegate void PlayMediaDelegate();
        public delegate void PauseMediaDelegate();
        public delegate void StopMediaDelegate();
        public enum MediaPlayerMode
        {
            MusicSync,
            MusicPrecise,
            Video
        }
    }
    namespace Editor
    {
        public enum TunerHeadMode
        {
            InTuner,
            InEditor
        }
        public enum ComponentBasicMode
        {
            Idle,
            Work,
            Create
        }
        public enum ComponentTypeMode
        {
            Idle,
            Work,
            Create
        }
        public enum ComponentHoldNoteMode
        {
            Idle,
            NotSupport,
            Work,
            Create
        }
        public enum ComponentMotionMode
        {
            Idle,
            Horizontal,
            Vertical,
            Rotation,
            Multiple
        }
        public enum TimeValuePairMode
        {
            Idle,
            Bpm,
            ScrollSpeed
        }
        public enum GizmoMotionMode
        {
            Idle,
            Create,
            Horizontal,
            Vertical,
            Rotation
        }
        public enum TunerSkin
        {
            Ritmo,
            Fisica
        }
        public class TimeLineTimeContainer
        {
            public float Timing;
            public GameObject GameObject = null;
        }
        public class Vector2Save
        {
            public float x, y;
            public Vector2Save(Vector2 Src)
            {
                x = Src.x;
                y = Src.y;
            }
            public Vector2Save()
            {

            }
            public Vector2 ToVector2()
            {
                return new Vector2(x, y);
            }
            public bool isZero()
            {
                if (x == 0 && y == 0) return true;
                return false;
            }
        }
        public class EditorLayout
        {
            public Vector2Save MusicPlayerPos = new Vector2Save(), MusicPlayerSize = new Vector2Save(),
                InspectorPos = new Vector2Save(), InspectorSize = new Vector2Save(),
                TimelinePos = new Vector2Save(), TimelineSize = new Vector2Save(),
                TunerWindowPos = new Vector2Save(), TunerWindowSize = new Vector2Save(),
                CreatorPos = new Vector2Save(), CreatorSize = new Vector2Save(),
            SpectrumPos = new Vector2Save(), SpectrumSize = new Vector2Save();
            public bool IsLayoutValid()
            {
                if (MusicPlayerSize.isZero()) return false;
                if (InspectorSize.isZero()) return false;
                if (TimelineSize.isZero()) return false;
                if (TunerWindowSize.isZero()) return false;
                if (CreatorSize.isZero()) return false;
                if (SpectrumSize.isZero()) return false;
                return true;
            }
        }
        public class OperationSave
        {
            public OperationForward Forward;
            public OperationReverse Reverse;
        }
        public class LanguagePackage
        {
            public Dictionary<string, string> TextDict = new Dictionary<string, string>();
            public Dictionary<string, string> NotificationDict = new Dictionary<string, string>();
            public Dictionary<string, string> HintDict = new Dictionary<string, string>();
            public Dictionary<string, string> TutorialDict = new Dictionary<string, string>();
            public string LanguageName;
        }
        public class OnWindowSortingEvent : UnityEvent<LimWindowManager>
        {

        }
        public class OnWindowSortedEvent : UnityEvent<int>
        {

        }
        public delegate void SetTextDelegate();
        public delegate void GizmoEditMode();
        public delegate void OperationReverse();
        public delegate void OperationForward();
    }
    namespace Exceptions
    {
        public class JointNoteOutOfRangeException : SystemException
        {
            public JointNoteOutOfRangeException()
            {

            }
        }
        public class NullJointsReferenceException : SystemException
        {
            public NullJointsReferenceException()
            {

            }
        }
        public class JointNoteNotFoundException : SystemException
        {
            public JointNoteNotFoundException()
            {

            }
        }
        public class JCountMismatchException : SystemException
        {
            public JCountMismatchException()
            {

            }
        }
        public class NullBeatlineTimesException : SystemException
        {
            public NullBeatlineTimesException()
            {

            }
        }
    }
    namespace Background
    {
        public enum BackgroundMode
        {
            None,
            Video,
            Single,
            Duo,
            Triple
        }
    }
    namespace Plugin
    {
        public class PluginDepend
        {
            public string RelativelyPath;
            public string Sha1Hash;
            public PluginDepend()
            {

            }
            public PluginDepend(string RelativelyPath, string Sha1Hash)
            {
                this.RelativelyPath = RelativelyPath;
                this.Sha1Hash = Sha1Hash;
            }
        }
        public class PluginContainer
        {
            public bool isValid;
            public string Name;
            public List<PluginDepend> Depends = new List<PluginDepend>();
        }
    }
    namespace Service
    {
        namespace Cloud
        {
            public enum Status
            {
                Running,
                NetworkNotReachable,
                UnsupportUserId,
                NoProjectLoaded
            }
            public enum TransferType
            {
                Chart,
                Music,
                Backup
            }
        }
    }
    namespace ChartZone
    {
        public class ChartZoneChart
        {
            public string ChartName;
            public string Designer;
            public string Size;
            public int NoteCount;
            public int BilibiliAvIndex;
        }
    }
    namespace Project
    {
        public class LanotaliumProject
        {
            public string Name;
            public string Designer;
            private string chartPath;
            private string musicPath;
            private string bGA0Path;
            private string bGA1Path;
            private string bGA2Path;

            public string ProjectFolder
            {
                get
                {
                    if (LimProjectManager.LapFolder == null) return null;
                    return (LimProjectManager.LapFolder).Replace("\\", "/");
                }
            }
            public string ChartPath
            {
                get
                {
                    if (chartPath == null) return null;
                    if (File.Exists(chartPath)) return chartPath.Replace("\\", "/");
                    else return (LimProjectManager.LapFolder + "/" + Path.GetFileName(chartPath)).Replace("\\", "/");
                }
                set
                {
                    chartPath = value;
                }
            }
            public string MusicPath
            {
                get
                {
                    if (musicPath == null) return null;
                    if (File.Exists(musicPath)) return musicPath.Replace("\\", "/");
                    else return (LimProjectManager.LapFolder + "/" + Path.GetFileName(musicPath)).Replace("\\", "/");
                }
                set
                {
                    musicPath = value;
                }
            }
            public string BGA0Path
            {
                get
                {
                    if (bGA0Path == null) return null;
                    if (File.Exists(bGA0Path)) return bGA0Path.Replace("\\", "/");
                    else return (LimProjectManager.LapFolder + "/" + Path.GetFileName(bGA0Path)).Replace("\\", "/");
                }
                set
                {
                    bGA0Path = value;
                }
            }
            public string BGA1Path
            {
                get
                {
                    if (bGA1Path == null) return null;
                    if (File.Exists(bGA1Path)) return bGA1Path.Replace("\\", "/");
                    else return (LimProjectManager.LapFolder + "/" + Path.GetFileName(bGA1Path)).Replace("\\", "/");
                }
                set
                {
                    bGA1Path = value;
                }
            }
            public string BGA2Path
            {
                get
                {
                    if (bGA2Path == null) return null;
                    if (File.Exists(bGA2Path)) return bGA2Path.Replace("\\", "/");
                    else return (LimProjectManager.LapFolder + "/" + Path.GetFileName(bGA2Path)).Replace("\\", "/");
                }
                set
                {
                    bGA2Path = value;
                }
            }

            public int BGACount()
            {
                int Count = 0;
                if (BGA0Path != null) Count++;
                if (BGA1Path != null) Count++;
                if (BGA2Path != null) Count++;
                return Count;
            }
            public bool IsValid()
            {
                if (ChartPath == null) return false;
                if (MusicPath == null) return false;
                if (BGACount() <= 0 || BGACount() > 3) return false;
                return true;
            }
        }
    }
    public class ChartContainer
    {
        public Chart.ChartData ChartData;
        public Chart.ChartBackground ChartBackground = new Chart.ChartBackground();
        public Chart.ChartLoadResult ChartLoadResult;
        public Chart.ChartProperty ChartProperty;
        public Chart.ChartMusic ChartMusic;
        public void CleanUp()
        {
            UnityEngine.Object.DestroyImmediate(ChartBackground.Color);
            UnityEngine.Object.DestroyImmediate(ChartBackground.Gray);
            UnityEngine.Object.DestroyImmediate(ChartBackground.Linear);
            UnityEngine.Object.DestroyImmediate(ChartMusic.Music);
        }
    }
    public class PreferencesContainer
    {
        public string LastOpenedChartFolder = string.Empty;
        public string LanguageName = "简体中文";
        public string Designer = string.Empty;
        public float MusicPlayerPreciseOffset = 0;
        public float WaveformBlockerPosition = 0;
        public int Build = 39;
        public bool Autosave = true;
        public bool JudgeColor = true;
        public bool CloudAutosave = false;
        public bool Waveform = false;
        public bool AudioEffect = true;
        public bool Unsafe = false;
        public bool HideWhatsNew = false;
        public bool DoNotRunAsAdmin = false;
        public bool LapInjected = false;
        public bool PlayWorkingBGM = false;
        public bool TimeLineBeatLine = true;
        public bool StretchBGA = true;
        public Editor.TunerSkin TunerSkin;
        public Tuner.AudioEffectTheme AudioEffectTheme = Tuner.AudioEffectTheme.Lanota;
    }
}

public class LimSystem : MonoBehaviour
{
    public static string Version = "v1.8.8";
    public static int Build = 39;
    public static Lanotalium.ChartContainer ChartContainer;
    public LimTunerManager TunerManager;
    public LimEditorManager EditorManager;
    public LimOperationManager OperationManager;
    public WindowsDialogUtility DialogUtils;
    public LimProjectManager ProjectManager;

#if UNITY_STANDALONE
    public string PreferencesSavePath
    {
        get
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Lanotalium/Preferences.json";
        }
    }
    public string EditorLayoutSavePath
    {
        get
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Lanotalium/EditorLayout.json";
        }
    }
    public string AppDataRoaming
    {
        get
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Lanotalium";
        }
    }
#endif
#if UNITY_IOS
     public string PreferencesSavePath;
     public string EditorLayoutSavePath;
     public string AppDataRoaming;
#endif
    public static Lanotalium.PreferencesContainer Preferences = new Lanotalium.PreferencesContainer();
    public static Lanotalium.Editor.EditorLayout EditorLayout = new Lanotalium.Editor.EditorLayout();
    public static string LanotaliumServer = "http://lanotalium.cn";

    public void RestorePreferences()
    {
        if (!File.Exists(PreferencesSavePath)) return;
        string PreferencesStr = File.ReadAllText(PreferencesSavePath);
        Preferences = JsonConvert.DeserializeObject<Lanotalium.PreferencesContainer>(PreferencesStr);
        if (Preferences.Build < Build)
        {
            Preferences.HideWhatsNew = false;
            Preferences.DoNotRunAsAdmin = false;
            Preferences.LapInjected = false;
        }
        Preferences.Build = Build;
        if (!File.Exists(EditorLayoutSavePath)) return;
        string EditorLayoutStr = File.ReadAllText(EditorLayoutSavePath);
        EditorLayout = JsonConvert.DeserializeObject<Lanotalium.Editor.EditorLayout>(EditorLayoutStr);
        if (EditorManager != null) EditorManager.RestoreEditorLayout();
    }
    public void SavePreferences()
    {
        if (EditorManager != null) EditorManager.SaveEditorLayout();
        string EditorLayoutStr = JsonConvert.SerializeObject(EditorLayout);
        File.WriteAllText(EditorLayoutSavePath, EditorLayoutStr);
        if (ChartContainer != null) Preferences.LastOpenedChartFolder = ChartContainer.ChartProperty.ChartFolder;
        string PreferencesStr = JsonConvert.SerializeObject(Preferences);
        File.WriteAllText(PreferencesSavePath, PreferencesStr);
    }
    private void Start()
    {
#if UNITY_IOS
        Application.targetFrameRate = 60;
        PreferencesSavePath = Application.persistentDataPath + "/Lanotalium/Preferences.json";
        EditorLayoutSavePath = Application.persistentDataPath + "/Lanotalium/EditorLayout.json";
        AppDataRoaming = Application.persistentDataPath + "/Lanotalium";
#endif
#if UNITY_EDITOR
        //if (File.Exists(PreferencesSavePath)) File.Delete(PreferencesSavePath);
#endif
        
        if (Directory.Exists(AppDataRoaming + "/Updator")) Directory.Delete(AppDataRoaming + "/Updator", true);
        if (!Directory.Exists(AppDataRoaming)) Directory.CreateDirectory(AppDataRoaming);
        RestorePreferences();
        Application.logMessageReceived += ReceiveUnityLog;
        if (ProjectManager == null) return;
#if UNITY_STANDALONE
        DragAndDrop.DragAndDrop.Enable(ProjectManager.OnDragFile, Application.productName);
#endif
    }

    private string LastLog;
    private void ReceiveUnityLog(string condition, string stackTrace, LogType type)
    {
        if (condition == LastLog) return;
        LimNotifyIcon.ShowMessage(condition);
        LastLog = condition;
    }

    private void OnDestroy()
    {
        SavePreferences();
    }
    private void OnApplicationQuit()
    {
        SavePreferences();
#if UNITY_STANDALONE
        DragAndDrop.DragAndDrop.Disable();
#endif
    }
}