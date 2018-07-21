using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace Schwarzer.Chart.Arcaea
{
    public enum EventType
    {
        Timing,
        Tap,
        Hold,
        Arc,
        Unknown
    }
    public class ArcaeaAffEvent
    {
        public int Timing;
        public EventType Type;
    }
    public class ArcaeaAffTiming : ArcaeaAffEvent
    {
        public float Bpm;
        public float BeatsPerLine;
    }
    public class ArcaeaAffTap : ArcaeaAffEvent
    {
        public int Track;
    }
    public class ArcaeaAffHold : ArcaeaAffEvent
    {
        public int EndTiming;
        public int Track;
    }
    public class ArcaeaAffArc : ArcaeaAffEvent
    {
        public int EndTiming;
        public float XStart;
        public float XEnd;
        public string LineType;
        public float YStart;
        public float YEnd;
        public int Color;
        public bool IsVoid;
        public List<int> ArcTaps;
    }
    public class ArcaeaAffReader
    {
        public float AudioOffset;
        public List<ArcaeaAffEvent> Events = new List<ArcaeaAffEvent>();
        public ArcaeaAffReader(string path)
        {
            Parse(path);
        }
        private EventType DetermineType(string line)
        {
            if (line.StartsWith("(")) return EventType.Tap;
            else if (line.StartsWith("timing")) return EventType.Timing;
            else if (line.StartsWith("hold")) return EventType.Hold;
            else if (line.StartsWith("arc")) return EventType.Arc;
            return EventType.Unknown;
        }
        private void ParseTiming(string line)
        {
            StringParser s = new StringParser(line);
            s.Skip(7);
            int tick = s.ReadInt(",");
            float bpm = s.ReadFloat(",");
            float beatsPerLine = s.ReadFloat(")");
            Events.Add(new ArcaeaAffTiming()
            {
                Timing = tick,
                BeatsPerLine = beatsPerLine,
                Bpm = bpm,
                Type = EventType.Timing
            });
        }
        private void ParseTap(string line)
        {
            StringParser s = new StringParser(line);
            s.Skip(1);
            int tick = s.ReadInt(",");
            int track = s.ReadInt(")");
            Events.Add(new ArcaeaAffTap()
            {
                Timing = tick,
                Track = track,
                Type = EventType.Tap
            });
        }
        private void ParseHold(string line)
        {
            StringParser s = new StringParser(line);
            s.Skip(5);
            int tick = s.ReadInt(",");
            int endtick = s.ReadInt(",");
            int track = s.ReadInt(")");
            Events.Add(new ArcaeaAffHold()
            {
                Timing = tick,
                EndTiming = endtick,
                Track = track,
                Type = EventType.Hold
            });
        }
        private void ParseArc(string line)
        {
            StringParser s = new StringParser(line);
            s.Skip(4);
            int tick = s.ReadInt(",");
            int endtick = s.ReadInt(",");
            float startx = s.ReadFloat(",");
            float endx = s.ReadFloat(",");
            string linetype = s.ReadString(",");
            float starty = s.ReadFloat(",");
            float endy = s.ReadFloat(",");
            int color = s.ReadInt(",");
            string unknown = s.ReadString(",");
            bool isvoid = s.ReadBool(")");
            List<int> arctap = null;
            if(s.Current != ";")
            {
                arctap = new List<int>();
                while (true)
                {
                    s.Skip(8);
                    arctap.Add(s.ReadInt(")"));
                    if (s.Current != ",") break;
                }
            }
            Events.Add(new ArcaeaAffArc()
            {
                Timing = tick,
                EndTiming = endtick,
                XStart = startx,
                XEnd = endx,
                LineType = linetype,
                YStart = starty,
                YEnd = endy,
                Color = color,
                IsVoid = isvoid,
                Type = EventType.Arc,
                ArcTaps = arctap
            });
        }
        public void Parse(string path)
        {
            try
            {
                string[] lines = File.ReadAllLines(path);
                AudioOffset = int.Parse(lines[0].Replace("AudioOffset:", ""));
                ParseTiming(lines[2]);
                for(int i = 3; i < lines.Length;++i)
                {
                    string line = lines[i];
                    switch(DetermineType(line))
                    {
                        case EventType.Timing:
                            ParseTiming(line);
                            break;
                        case EventType.Tap:
                            ParseTap(line);
                            break;
                        case EventType.Hold:
                            ParseHold(line);
                            break;
                        case EventType.Arc:
                            ParseArc(line);
                            break;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.LogException(Ex);
            }
        }
    }
    public class ArcaeaUtility
    {
        public static float ConvertDegree(float x,float y,int hand)
        {
            return Mathf.Rad2Deg * Mathf.Atan2(y, x - 0.5f) * 0.5f + (hand == 0 ? 235 : 45);
        }
        public static int DetermineEase(string type)
        {
            switch(type)
            {
                case "s":return 0;
                case "b":return 6;
                case "si": case "siso": case "sisi": return 11;
                case "so": case "soso": case "sosi": return 10;
            }
            return 0;
        }
        public static float CalculateEasedCurve(float Percent, int Mode)
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
    }
}