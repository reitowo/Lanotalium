using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Schwarzer.Chart.Lanota.Managed;

namespace Schwarzer.Chart.Lanota
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
    namespace Managed
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
                foreach (SpriteRenderer s in HoldNoteGameObject.GetComponentsInChildren<SpriteRenderer>())
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
        namespace Lanotalium
        {
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
            public class ChartContainer
            {
                public LanotaChartManaged ChartData;
                public ChartBackground ChartBackground = new ChartBackground();
                public ChartLoadResult ChartLoadResult;
                public ChartProperty ChartProperty;
                public ChartMusic ChartMusic;
                public void CleanUp()
                {
                    UnityEngine.Object.DestroyImmediate(ChartBackground.Color);
                    UnityEngine.Object.DestroyImmediate(ChartBackground.Gray);
                    UnityEngine.Object.DestroyImmediate(ChartBackground.Linear);
                    UnityEngine.Object.DestroyImmediate(ChartMusic.Music);
                }
            }
        }
    }
    namespace Lanotalium
    {
        namespace Project
        {
            public class LanotaliumProject
            {
                public string Name;
                public string Designer;
                public string ChartPath;
                public string MusicPath;
                public string BGA0Path;
                public string BGA1Path;
                public string BGA2Path;
            }
        }
    }
    public class LanotaChartManaged
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
        public LanotaChartManaged()
        {
            LanotaCameraRot = new List<LanotaCameraRot>();
            LanotaCameraXZ = new List<LanotaCameraXZ>();
            LanotaCameraY = new List<LanotaCameraY>();
            LanotaChangeBpm = new List<LanotaChangeBpm>();
            LanotaDefault = new LanotaDefault();
            LanotaHoldNote = new List<LanotaHoldNote>();
            LanotaScroll = new List<LanotaScroll>();
            LanotaTapNote = new List<LanotaTapNote>();
            SongLength = 0;
        }
        public LanotaChartManaged(string Text)
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
    public class LanotaChartHandler
    {

    }
}