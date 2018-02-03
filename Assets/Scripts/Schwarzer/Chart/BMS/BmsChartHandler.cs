using System.Collections;
using System.Collections.Generic;
using Schwarzer.Chart.Bms.Managed;
using Schwarzer.Chart.Bms.Bemusilization;
using UnityEngine;
using Schwarzer.Chart.Bms.Audio;
using System.IO;
using System.Threading.Tasks;

namespace Schwarzer.Chart.Bms
{
    namespace Managed
    {
        public enum BmsEventType
        {
            Wav,
            Bpm,
            Stop,
            BeatReset
        }
        public class BmsEvent
        {
            public BmsEventType Type;
            public float Timing;
            public BMSEvent NativeEvent;
            public BmsEvent(BmsEventType Type, float Timing, BMSEvent NativeEvent)
            {
                this.Type = Type;
                this.Timing = Timing;
                this.NativeEvent = NativeEvent;
            }
        }
        public class BmsNote
        {
            public float Timing;
            public int Track;
            public BMSEvent NativeEvent;
            public BmsNote(float Timing, int Track, BMSEvent NativeEvent)
            {
                this.Timing = Timing;
                this.Track = Track;
                this.NativeEvent = NativeEvent;
            }
        }
        public class BmsLongNote
        {
            public float Timing;
            public int Track;
            public float Duration;
            public BMSEvent NativeEvent;
            public BmsLongNote(float Timing, int Track, float Duration, BMSEvent NativeEvent)
            {
                this.Timing = Timing;
                this.Track = Track;
                this.Duration = Duration;
                this.NativeEvent = NativeEvent;
            }
        }
    }
    namespace Audio
    {
        public class FullAudioClipData
        {
            public bool IsCompleted;
            public float Percent;
            public float[] Data;

            public Task WriteDataToWavAsync(string WavFilePath)
            {
                return Task.Run(() => WriteDataToWav(WavFilePath));
            }
            public void WriteDataToWav(string WavFilePath)
            {
                if (File.Exists(WavFilePath)) File.Delete(WavFilePath);
                NAudio.Wave.WaveFileWriter waveFileWriter = new NAudio.Wave.WaveFileWriter(WavFilePath, NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
                try
                {
                    waveFileWriter.WriteSamples(Data, 0, Data.Length);
                }
                finally
                {
                    waveFileWriter.Close();
                }
            }
            public Task WriteDataToOggAsync(string OggFilePath)
            {
                return Task.Run(() => WriteDataToOgg(OggFilePath));
            }
            public void WriteDataToOgg(string OggFilePath)
            {
                if (File.Exists(OggFilePath)) File.Delete(OggFilePath);
            }
        }
        public class BmsAudioGenerator
        {
            private List<float> _Buffer = new List<float>();
            private int _SampleRate;
            private int _Channel;

            public int SampleRate
            {
                get
                {
                    return _SampleRate;
                }

                set
                {
                    _SampleRate = value;
                }
            }
            public int Channel
            {
                get
                {
                    return _Channel;
                }

                set
                {
                    _Channel = value;
                }
            }
            public float[] Data
            {
                get
                {
                    return _Buffer.ToArray();
                }
            }

            public BmsAudioGenerator(int SampleRate, int Channel)
            {
                _SampleRate = SampleRate;
                _Channel = Channel;
            }
            private void AcquireBufferCapacity(int Capacity)
            {
                if (_Buffer.Capacity >= Capacity) return;
                int OriginCapacity = _Buffer.Capacity;
                _Buffer.Capacity = Capacity;
                for (int i = 0; i < Capacity - OriginCapacity; ++i)
                {
                    _Buffer.Add(0);
                }
            }
            public void PutAudioAt(float Timing, float[] Data)
            {
                int TargetSample = Mathf.FloorToInt(Timing * _SampleRate * _Channel);
                int Length = Data.Length;
                AcquireBufferCapacity(TargetSample + Length);
                for (int i = 0; i < Length; ++i)
                {
                    _Buffer[TargetSample + i] += Data[i];
                }
            }
        }
    }
    public class BmsChartManaged
    {
        public string BmsChartPath;
        public string BmsChartFolder
        {
            get
            {
                return Directory.GetParent(BmsChartPath).FullName;
            }
        }
        public string BmsChartFileName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(BmsChartPath);
            }
        }
        public string StageFile
        {
            get
            {
                return NativeBmsChart.GetResourceData(ResourceType.bmp, -1).dataPath;
            }
        }
        public BMSChart NativeBmsChart;
        public List<BmsNote> Notes = new List<BmsNote>();
        public List<BmsLongNote> LongNotes = new List<BmsLongNote>();
        public List<BmsEvent> Bpms = new List<BmsEvent>();
        public List<BmsEvent> Wavs = new List<BmsEvent>();
        public List<BmsEvent> BeatResets = new List<BmsEvent>();
        public List<BmsEvent> Stops = new List<BmsEvent>();

        public BmsChartManaged(string BmsChartPath)
        {
            this.BmsChartPath = BmsChartPath;
            string BmsContent = File.ReadAllText(BmsChartPath);
            NativeBmsChart = new BMSChart(BmsContent);
            NativeBmsChart.Parse(ParseType.All);
            foreach (BMSEvent BmsEvent in NativeBmsChart.Events)
            {
                switch (BmsEvent.type)
                {
                    case BMSEventType.BeatReset:
                        BeatResets.Add(new BmsEvent(BmsEventType.BeatReset, (float)BmsEvent.time.TotalSeconds, BmsEvent));
                        break;
                    case BMSEventType.BPM:
                        Bpms.Add(new BmsEvent(BmsEventType.Bpm, (float)BmsEvent.time.TotalSeconds, BmsEvent));
                        break;
                    case BMSEventType.Note:
                        Notes.Add(new BmsNote((float)BmsEvent.time.TotalSeconds, BmsEvent.data1, BmsEvent));
                        break;
                    case BMSEventType.LongNoteStart:
                        LongNotes.Add(new BmsLongNote((float)BmsEvent.time.TotalSeconds, BmsEvent.data1, (float)(BmsEvent.time2.TotalSeconds - BmsEvent.time.TotalSeconds), BmsEvent));
                        break;
                    case BMSEventType.STOP:
                        Stops.Add(new BmsEvent(BmsEventType.Stop, (float)BmsEvent.time.TotalSeconds, BmsEvent));
                        break;
                    case BMSEventType.WAV:
                        Wavs.Add(new BmsEvent(BmsEventType.Wav, (float)BmsEvent.time.TotalSeconds, BmsEvent));
                        break;
                }
            }
        }
        public Task ProduceFullAudioClipDataAsync(FullAudioClipData fullAudioClipData)
        {
            return Task.Run(() => ProduceFullAudioClipData(fullAudioClipData));
        }
        public void ProduceFullAudioClipData(FullAudioClipData fullAudioClipData)
        {
            string ResourcesRootDirectory = BmsChartFolder;
            BmsAudioGenerator bmsAudioGenerator = new BmsAudioGenerator(44100, 2);
            int TotalCount = NativeBmsChart.Events.Count;
            int LoadedCount = 0;
            foreach (BMSEvent Event in NativeBmsChart.Events)
            {
                switch (Event.type)
                {
                    case BMSEventType.LongNoteEnd:
                    case BMSEventType.LongNoteStart:
                    case BMSEventType.Note:
                    case BMSEventType.WAV:
                        BMSResourceData resourceData = new BMSResourceData();
                        if (!NativeBmsChart.TryGetResourceData(ResourceType.wav, Event.data2, out resourceData)) continue;
                        string FileName = Path.GetFileNameWithoutExtension(resourceData.dataPath);
                        string OggResourcePath = (ResourcesRootDirectory + "/" + FileName + ".ogg").Replace("\\", "/");
                        string WavResourcePath = (ResourcesRootDirectory + "/" + FileName + ".wav").Replace("\\", "/");
                        if (File.Exists(OggResourcePath))
                        {
                            NVorbis.VorbisReader vorbisReader = new NVorbis.VorbisReader(OggResourcePath);
                            float[] oggData = new float[vorbisReader.TotalSamples * vorbisReader.Channels];
                            vorbisReader.ReadSamples(oggData, 0, (int)(vorbisReader.TotalSamples * vorbisReader.Channels));
                            bmsAudioGenerator.PutAudioAt((float)Event.time.TotalSeconds, oggData);
                        }
                        else if (File.Exists(WavResourcePath))
                        {
                            List<float> wavData = new List<float>();
                            NAudio.Wave.WaveFileReader waveFileReader = new NAudio.Wave.WaveFileReader(WavResourcePath);
                            float[] wavFrame;
                            while(true)
                            {
                                wavFrame = waveFileReader.ReadNextSampleFrame();
                                if (wavFrame == null) break;
                                wavData.AddRange(wavFrame);
                            }
                            bmsAudioGenerator.PutAudioAt((float)Event.time.TotalSeconds, wavData.ToArray());
                        }
                        break;
                }
                LoadedCount++;
                fullAudioClipData.Percent = 100f * LoadedCount / TotalCount;
            }
            fullAudioClipData.Data = bmsAudioGenerator.Data;
            fullAudioClipData.IsCompleted = true;
        }
    }
    public class BmsChartHandler
    {
        public static BMSChart GetBemusilizedChart(string BmsContent)
        {
            return new BMSChart(BmsContent);
        }
        public static BmsChartManaged GetManagedBmsChart(string BmsChartPath)
        {
            if (!File.Exists(BmsChartPath)) return null;
            return new BmsChartManaged(BmsChartPath);
        }
    }
}