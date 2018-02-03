using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Schwarzer.Chart.Bms.Bemusilization.Utils;

namespace Schwarzer.Chart.Bms.Bemusilization
{
    public abstract partial class Chart
    {
        protected string title;
        protected string subTitle;
        protected string artist;
        protected string subArtist;
        protected string comments;
        protected string genre;
        protected int playerCount;
        protected float initialBPM;
        protected float minBpm;
        protected int playLevel;
        protected int rank;
        protected float volume;
        protected int maxCombos;
        protected BMSKeyLayout layout;
        private readonly List<BMSEvent> bmsEvents = new List<BMSEvent>();
        private readonly Dictionary<ResourceId, BMSResourceData> resourceDatas = new Dictionary<ResourceId, BMSResourceData>();
        private readonly Dictionary<ResourceId, BMSResourceData> metaResourceDatas = new Dictionary<ResourceId, BMSResourceData>();
        private readonly HashSet<int> allChannels = new HashSet<int>();

        private readonly List<WeakReference> eventDispatchers = new List<WeakReference>();

        public virtual string Title { get { return title; } }
        public virtual string SubTitle { get { return subTitle; } }
        public virtual string Artist { get { return artist; } }
        public virtual string SubArtist { get { return subArtist; } }
        public virtual string Comments { get { return comments; } }
        public virtual string Genre { get { return genre; } }
        public virtual int PlayerCount { get { return playerCount; } }
        public virtual float BPM { get { return initialBPM; } }
        public virtual float MinBPM { get { return minBpm; } }
        public virtual float PlayLevel { get { return playLevel; } }
        public virtual int Rank { get { return rank; } }
        public virtual float Volume { get { return volume; } }
        public virtual int MaxCombos { get { return maxCombos; } }
        public virtual string RawContent { get { return string.Empty; } }
        public virtual bool Randomized { get { return false; } }
        public virtual int ChannelCount { get { return allChannels.Count; } }
        public virtual BMSKeyLayout Layout { get { return layout; } }

        public IList<BMSEvent> Events
        {
            get { return new ReadOnlyCollection<BMSEvent>(bmsEvents); }
        }

        public ICollection<int> AllChannels
        {
            get { return allChannels; }
        }

        public virtual void Parse(ParseType parseType)
        {
            if ((parseType & ParseType.Content) == ParseType.Content)
                OnDataRefresh();
            ParseLayout();
        }

        private void OnDataRefresh()
        {
            lock (eventDispatchers)
                for (int i = 0; i < eventDispatchers.Count; i++)
                {
                    WeakReference wr = eventDispatchers[i];
                    if (!wr.IsAlive)
                    {
                        eventDispatchers.RemoveAt(i--);
                        continue;
                    }
                    EventDispatcher target = wr.Target as EventDispatcher;
                    if (target != null)
                        target.OnBMSRefresh();
                }
        }

        private void ParseLayout()
        {
            layout = BMSKeyLayout.None;
            foreach (int channel in allChannels)
            {
                if (channel > 10 && channel < 20)
                    layout |= (BMSKeyLayout)(1 << (channel - 11));
                else if (channel > 20 && channel < 30)
                    layout |= (BMSKeyLayout)(1 << (channel - 12));
                else if (channel > 50 && channel < 60)
                    layout |= (BMSKeyLayout)(1 << (channel - 51));
                else if (channel > 60 && channel < 70)
                    layout |= (BMSKeyLayout)(1 << (channel - 52));
            }
        }

        protected void ResetAllData(ParseType parseType)
        {
            if ((parseType & ParseType.Header) == ParseType.Header)
            {
                title = "";
                artist = "";
                subArtist = "";
                comments = "";
                genre = "";
                playerCount = 1;
                initialBPM = 130;
                minBpm = float.PositiveInfinity;
                playLevel = 0;
                rank = 0;
                volume = 1;
                metaResourceDatas.Clear();
            }
            if ((parseType & ParseType.Resources) == ParseType.Resources)
            {
                resourceDatas.Clear();
                foreach (var kv in metaResourceDatas)
                    resourceDatas[kv.Key] = kv.Value;
            }
            if ((parseType & ParseType.Content) == ParseType.Content)
            {
                maxCombos = 0;
                bmsEvents.Clear();
                allChannels.Clear();
                OnDataRefresh();
                layout = BMSKeyLayout.None;
            }
            else if ((parseType & ParseType.Content) == ParseType.Content)
            {
                maxCombos = 0;
                allChannels.Clear();
                layout = BMSKeyLayout.None;
            }
        }

        protected void AddResource(ResourceType type, long id, string dataPath, object additionalData = null)
        {
            ResourceId resId = new ResourceId(type, id);
            BMSResourceData resData = new BMSResourceData
            {
                type = type,
                resourceId = id,
                dataPath = dataPath,
                additionalData = additionalData
            };
            resourceDatas[resId] = resData;
            if (id < 0) metaResourceDatas[resId] = resData;
        }

        protected int AddEvent(BMSEvent ev)
        {
            if (ev.IsNote)
            {
                allChannels.Add(ev.data1);
                maxCombos++;
            }
            return bmsEvents.InsertInOrdered(ev);
        }

        protected void AddEvents(IEnumerable<BMSEvent> events)
        {
            bmsEvents.InsertInOrdered(events);
            allChannels.UnionWith(
                events.Where(ev => ev.IsNote)
                .Select(ev => ev.data1)
            );
        }

        protected void ReportChannels(IEnumerable<int> channels)
        {
            allChannels.UnionWith(channels);
        }

        protected int FindEventIndex(BMSEvent ev)
        {
            int firstIndex = bmsEvents.BinarySearchIndex(ev, BinarySearchMethod.FirstExact);
            int lastIndex = bmsEvents.BinarySearchIndex(ev, BinarySearchMethod.LastExact, firstIndex);
            return firstIndex == lastIndex ? firstIndex :
                bmsEvents.IndexOf(ev, firstIndex, lastIndex - firstIndex + 1);
        }

        protected void ReplaceEvent(int index, BMSEvent newEv)
        {
            BMSEvent original = bmsEvents[index];
            if (original.CompareTo(newEv) == 0)
            {
                bmsEvents[index] = newEv;
                return;
            }
            bmsEvents.RemoveAt(index);
            bmsEvents.InsertInOrdered(newEv);
        }

        public EventDispatcher GetEventDispatcher()
        {
            return new EventDispatcher(this);
        }

        public IEnumerable<BMSResourceData> IterateResourceData(ResourceType type = ResourceType.Unknown)
        {
            if (type == ResourceType.Unknown)
                return resourceDatas.Values;
            return from kv in resourceDatas
                   where kv.Key.type == type
                   select kv.Value;
        }

        public BMSResourceData GetResourceData(ResourceType type, long id)
        {
            BMSResourceData result;
            resourceDatas.TryGetValue(new ResourceId(type, id), out result);
            return result;
        }

        public bool TryGetResourceData(ResourceType type, long id, out BMSResourceData result)
        {
            return resourceDatas.TryGetValue(new ResourceId(type, id), out result);
        }
    }

    [Flags]
    public enum ParseType
    {
        None = 0,
        Header = 0x1,
        Resources = 0x2,
        Content = 0x4,
        ContentSummary = 0x8,
        All = Header | Resources | Content
    }

    [Flags]
    public enum BMSKeyLayout
    {
        // Default
        None = 0x0,

        // Single Key Definition
        P11 = 0x1,
        P12 = 0x2,
        P13 = 0x4,
        P14 = 0x8,
        P15 = 0x10,
        P16 = 0x20,
        P17 = 0x40,
        P18 = 0x80,
        P19 = 0x100,
        P21 = 0x200,
        P22 = 0x400,
        P23 = 0x800,
        P24 = 0x1000,
        P25 = 0x2000,
        P26 = 0x4000,
        P27 = 0x8000,
        P28 = 0x10000,
        P29 = 0x20000,

        // Known Layout Definition
        Single5Key = P11 | P12 | P13 | P14 | P15 | P16,
        Single7Key = P11 | P12 | P13 | P14 | P15 | P18 | P19 | P16,
        Single9Key = P11 | P12 | P13 | P14 | P15 | P22 | P23 | P24 | P25,
        Single9KeyAlt = P11 | P12 | P13 | P14 | P15 | P16 | P17 | P18 | P19,
        Duel10Key = P11 | P12 | P13 | P14 | P15 | P16 |
            P21 | P22 | P23 | P24 | P25 | P26,
        Duel14Key = P11 | P12 | P13 | P14 | P15 | P18 | P19 | P16 |
            P21 | P22 | P23 | P24 | P25 | P28 | P29 | P26,
    }
}
