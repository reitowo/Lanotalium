using System.Collections;
using System.Collections.Generic;
using Schwarzer.Chart.Lanota;
using Schwarzer.Chart.Bms;
using System.IO;
using Schwarzer.Chart.Arcaea;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Schwarzer.Chart
{
    public class ChartConvertTask
    {
        public Task Task;
        public float Percent;
        public string LapPath;
    }
    public class ChartConvert
    {
        private static IEnumerator WaitForTask(Task task)
        {
            while (!task.IsCompleted) yield return null;
            if (task.Exception != null) throw task.Exception;
        }
        private static LanotaChartManaged BmsToLanota(BmsChartManaged BmsChart)
        {
            LanotaChartManaged LanotaChart = new LanotaChartManaged();

            foreach (Bms.Managed.BmsNote Note in BmsChart.Notes)
            {
                Lanota.Managed.LanotaTapNote Tap = new Lanota.Managed.LanotaTapNote
                {
                    Time = Note.Timing,
                    Size = 1,
                    Degree = Note.Track * 10 - 155
                };
                LanotaChart.LanotaTapNote.Add(Tap);
            }
            foreach (Bms.Managed.BmsLongNote LongNote in BmsChart.LongNotes)
            {
                Lanota.Managed.LanotaHoldNote Hold = new Lanota.Managed.LanotaHoldNote
                {
                    Time = LongNote.Timing,
                    Degree = LongNote.Track * 10 - 155,
                    Duration = LongNote.Duration,
                    Size = 1
                };
                LanotaChart.LanotaHoldNote.Add(Hold);
            }

            float BaseBpm = BmsChart.NativeBmsChart.BPM;
            Lanota.Managed.LanotaChangeBpm DefaultBpm = new Lanota.Managed.LanotaChangeBpm
            {
                Time = -3,
                Bpm = BaseBpm
            };
            LanotaChart.LanotaChangeBpm.Add(DefaultBpm);
            Lanota.Managed.LanotaScroll DefaultScroll = new Lanota.Managed.LanotaScroll
            {
                Speed = 1,
                Time = -10
            };
            LanotaChart.LanotaScroll.Add(DefaultScroll);

            foreach (Bms.Managed.BmsEvent Event in BmsChart.Bpms)
            {
                Lanota.Managed.LanotaChangeBpm Bpm = new Lanota.Managed.LanotaChangeBpm
                {
                    Time = Event.Timing,
                    Bpm = (float)Event.NativeEvent.Data2F
                };
                LanotaChart.LanotaChangeBpm.Add(Bpm);
                Lanota.Managed.LanotaScroll Scroll = new Lanota.Managed.LanotaScroll
                {
                    Speed = Bpm.Bpm / BaseBpm,
                    Time = Event.Timing
                };
                LanotaChart.LanotaScroll.Add(Scroll);
            }

            return LanotaChart;
        }
        public static IEnumerator BmsToLanotalium(string BmsChartPath, bool Overwrite)
        {
            BmsChartManaged bmsChartManaged = new BmsChartManaged(BmsChartPath);
            LanotaChartManaged lanotaChartManaged = BmsToLanota(bmsChartManaged);
            Directory.CreateDirectory(bmsChartManaged.BmsChartFolder + "/Lanota");
            string Chart = lanotaChartManaged.ToString();
            File.WriteAllText(bmsChartManaged.BmsChartFolder + "/Lanota/Lanota_" + bmsChartManaged.BmsChartFileName + ".txt", Chart);
            if (!File.Exists(bmsChartManaged.BmsChartFolder + "/Lanota/" + bmsChartManaged.NativeBmsChart.Title + ".wav") || Overwrite)
            {
                Bms.Audio.FullAudioClipData fullAudioClipData = new Bms.Audio.FullAudioClipData();
                yield return WaitForTask(bmsChartManaged.ProduceFullAudioClipDataAsync(fullAudioClipData));
                yield return WaitForTask(fullAudioClipData.WriteDataToWavAsync(bmsChartManaged.BmsChartFolder + "/Lanota/" + bmsChartManaged.NativeBmsChart.Title + ".wav"));
            }
            if (File.Exists(bmsChartManaged.BmsChartFolder + "/" + bmsChartManaged.StageFile)) File.Copy(bmsChartManaged.BmsChartFolder + "/" + bmsChartManaged.StageFile, bmsChartManaged.BmsChartFolder + "/Lanota/background.png", true);
            Lanota.Lanotalium.Project.LanotaliumProject lanotaliumProject = new Lanota.Lanotalium.Project.LanotaliumProject
            {
                Name = bmsChartManaged.NativeBmsChart.Title,
                Designer = bmsChartManaged.NativeBmsChart.Artist,
                MusicPath = bmsChartManaged.BmsChartFolder + "/Lanota/" + bmsChartManaged.NativeBmsChart.Title + ".wav",
                ChartPath = bmsChartManaged.BmsChartFolder + "/Lanota/Lanota_" + bmsChartManaged.BmsChartFileName + ".txt",
                BGA0Path = bmsChartManaged.BmsChartFolder + "/Lanota/background.png"
            };
            File.WriteAllText(bmsChartManaged.BmsChartFolder + "/Lanota/Lap_" + bmsChartManaged.BmsChartFileName + ".lap", JsonConvert.SerializeObject(lanotaliumProject));
        }
        public static ChartConvertTask BmsToLanotaliumAsync(string BmsChartPath, bool Overwrite)
        {
            ChartConvertTask task = new ChartConvertTask();
            task.Task = Task.Run(() =>
            {
                BmsChartManaged bmsChartManaged = new BmsChartManaged(BmsChartPath);
                LanotaChartManaged lanotaChartManaged = BmsToLanota(bmsChartManaged);
                Directory.CreateDirectory(bmsChartManaged.BmsChartFolder + "/Lanota");
                string Chart = lanotaChartManaged.ToString();
                File.WriteAllText(bmsChartManaged.BmsChartFolder + "/Lanota/Lanota_" + bmsChartManaged.BmsChartFileName + ".txt", Chart);
                task.Percent = 1;
                if (!File.Exists(bmsChartManaged.BmsChartFolder + "/Lanota/" + bmsChartManaged.NativeBmsChart.Title + ".wav") || Overwrite)
                {
                    Bms.Audio.FullAudioClipData fullAudioClipData = new Bms.Audio.FullAudioClipData();
                    Task AudioTask = bmsChartManaged.ProduceFullAudioClipDataAsync(fullAudioClipData);
                    while (!AudioTask.IsCompleted)
                    {
                        task.Percent = 1 + fullAudioClipData.Percent * 0.95f;
                    }
                    if (AudioTask.Exception != null) throw AudioTask.Exception;
                    Task WriteAudioTask = fullAudioClipData.WriteDataToWavAsync(bmsChartManaged.BmsChartFolder + "/Lanota/" + bmsChartManaged.NativeBmsChart.Title + ".wav");
                    while (!WriteAudioTask.IsCompleted) { }
                    if (WriteAudioTask.Exception != null) throw WriteAudioTask.Exception;
                }
                task.Percent = 96;
                if (File.Exists(bmsChartManaged.BmsChartFolder + "/" + bmsChartManaged.StageFile)) File.Copy(bmsChartManaged.BmsChartFolder + "/" + bmsChartManaged.StageFile, bmsChartManaged.BmsChartFolder + "/Lanota/background.png", true);
                Lanota.Lanotalium.Project.LanotaliumProject lanotaliumProject = new Lanota.Lanotalium.Project.LanotaliumProject
                {
                    Name = bmsChartManaged.NativeBmsChart.Title,
                    Designer = bmsChartManaged.NativeBmsChart.Artist,
                    MusicPath = bmsChartManaged.BmsChartFolder + "/Lanota/" + bmsChartManaged.NativeBmsChart.Title + ".wav",
                    ChartPath = bmsChartManaged.BmsChartFolder + "/Lanota/Lanota_" + bmsChartManaged.BmsChartFileName + ".txt",
                    BGA0Path = bmsChartManaged.BmsChartFolder + "/Lanota/background.png"
                };
                File.WriteAllText(bmsChartManaged.BmsChartFolder + "/Lanota/Lap_" + bmsChartManaged.BmsChartFileName + ".lap", JsonConvert.SerializeObject(lanotaliumProject));
                task.LapPath = bmsChartManaged.BmsChartFolder + "/Lanota/Lap_" + bmsChartManaged.BmsChartFileName + ".lap";
                task.Percent = 100;
            });
            return task;
        }
        public static void ArcaeaToLanota(string affPath)
        {
            ArcaeaAffReader reader = new ArcaeaAffReader(affPath);
            
            LanotaChartManaged la = new LanotaChartManaged();
            float baseBpm = (reader.Events[0] as ArcaeaAffTiming).Bpm;
            la.LanotaChangeBpm.Add(new Lanota.Managed.LanotaChangeBpm
            {
                Time = -3,
                Bpm = baseBpm
            });
            la.LanotaScroll.Add(new Lanota.Managed.LanotaScroll
            {
                Speed = 1,
                Time = -10
            });

            foreach (ArcaeaAffEvent e in reader.Events)
            {
                switch(e.Type)
                {
                    case Arcaea.EventType.Timing:
                        ArcaeaAffTiming timing = e as ArcaeaAffTiming;
                        la.LanotaChangeBpm.Add(new Lanota.Managed.LanotaChangeBpm()
                        {
                            Bpm = timing.Bpm,
                            Time = (timing.Timing + reader.AudioOffset) / 1000f
                        });
                        la.LanotaScroll.Add(new Lanota.Managed.LanotaScroll()
                        {
                            Speed = timing.Bpm / baseBpm,
                            Time = (timing.Timing + reader.AudioOffset) / 1000f
                        });
                        break;
                    case Arcaea.EventType.Tap:
                        ArcaeaAffTap tap = e as ArcaeaAffTap;
                        la.LanotaTapNote.Add(new Lanota.Managed.LanotaTapNote()
                        {
                            Type = 0,
                            Time = (tap.Timing + reader.AudioOffset) / 1000f,
                            Size = 1,
                            Degree = tap.Track * 30 - 70
                        });
                        break;
                    case Arcaea.EventType.Hold:
                        ArcaeaAffHold hold = e as ArcaeaAffHold;
                        la.LanotaHoldNote.Add(new Lanota.Managed.LanotaHoldNote()
                        {
                            Type = 5,
                            Time = (hold.Timing + reader.AudioOffset) / 1000f,
                            Degree = hold.Track * 30 - 70,
                            Duration = (hold.EndTiming - hold.Timing) / 1000f,
                            Size = 1
                        });
                        break;
                    case Arcaea.EventType.Arc:
                        ArcaeaAffArc arc = e as ArcaeaAffArc;
                        if (!arc.IsVoid)
                        {
                            la.LanotaHoldNote.Add(new Lanota.Managed.LanotaHoldNote()
                            {
                                Type = 5,
                                Time = (arc.Timing + reader.AudioOffset) / 1000f,
                                Duration = (arc.EndTiming - arc.Timing) / 1000f,
                                Degree = ArcaeaUtility.ConvertDegree(arc.XStart, arc.YStart, arc.Color),
                                Jcount = 1,
                                Size = 1,
                                Joints = new List<Lanota.Managed.LanotaJoints>()
                                {
                                    new Lanota.Managed.LanotaJoints()
                                    {
                                        dDegree = ArcaeaUtility.ConvertDegree(arc.XEnd,arc.YEnd,arc.Color) -  ArcaeaUtility.ConvertDegree(arc.XStart, arc.YStart,arc.Color),
                                        dTime =  (arc.EndTiming - arc.Timing) / 1000f,
                                        Cfmi = ArcaeaUtility.DetermineEase(arc.LineType)
                                    }
                                }
                            });
                        }
                        if(arc.ArcTaps!=null)
                        {
                            float startDegree = ArcaeaUtility.ConvertDegree(arc.XStart, arc.YStart, arc.Color);
                            float deltaDegree = ArcaeaUtility.ConvertDegree(arc.XEnd, arc.YEnd, arc.Color) - ArcaeaUtility.ConvertDegree(arc.XStart, arc.YStart, arc.Color);
                            float duration = (arc.EndTiming - arc.Timing) / 1000f;
                            float startTiming = (arc.Timing + reader.AudioOffset) / 1000f;
                            int ease = ArcaeaUtility.DetermineEase(arc.LineType);
                            foreach (int i in arc.ArcTaps)
                            {
                                float t = (i + reader.AudioOffset) / 1000f;
                                float percent = (t - startTiming) / duration;
                                float degree = startDegree + deltaDegree * ArcaeaUtility.CalculateEasedCurve(percent, ease);
                                la.LanotaTapNote.Add(new Lanota.Managed.LanotaTapNote()
                                {
                                    Type = 0,
                                    Time = t,
                                    Size = 2,
                                    Degree = degree
                                });
                            }
                        }
                        break;
                }
            }

            File.WriteAllText(affPath.Replace(".aff", "_convert.txt"), la.ToString());
        }
    }
}