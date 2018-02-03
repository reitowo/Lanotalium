using System;
using System.Collections.Generic;

using Schwarzer.Chart.Bms.Bemusilization.Utils;

namespace Schwarzer.Chart.Bms.Bemusilization
{
    public delegate void OnBMSEvent(BMSEvent bmsEvent);

    public abstract partial class Chart
    {
        public class EventDispatcher
        {
            protected readonly Chart chart;
            protected TimeSpan currentTime, endTime;
            protected int currentIndex, length;
            protected OnBMSEvent bmsEvent;

            public event OnBMSEvent BMSEvent
            {
                add { bmsEvent += value; }
                remove { bmsEvent -= value; }
            }

            protected internal EventDispatcher(Chart chart)
            {
                this.chart = chart;
                lock (chart.eventDispatchers)
                    chart.eventDispatchers.Add(new WeakReference(this));
                OnBMSRefresh();
            }

            public TimeSpan CurrentTime
            {
                get { return currentTime; }
            }

            public bool IsStart
            {
                get { return currentIndex <= 0; }
            }

            public bool IsEnd
            {
                get { return currentIndex >= length - 1; }
            }

            public TimeSpan EndTime
            {
                get { return endTime; }
            }

            public long Index
            {
                get { return currentIndex; }
            }

            public virtual void Seek(TimeSpan newTime, bool dispatchEvents = true)
            {
                InternalSeek(chart.bmsEvents, newTime, dispatchEvents);
            }

            protected void InternalSeek(IList<BMSEvent> bmsEvents, TimeSpan newTime, bool dispatchEvents)
            {
                if (newTime > currentTime)
                {
                    currentTime = newTime;
                    if (currentIndex < 0) currentIndex = 0;
                    if (!dispatchEvents || bmsEvent == null)
                        // If it does not require to dispatch events,
                        // use a quicker way to seek to position.
                        currentIndex = bmsEvents.BinarySearchIndex(
                            new BMSEvent { time = currentTime },
                            BinarySearchMethod.LastExact | BinarySearchMethod.FloorClosest,
                            currentIndex);
                    else
                        while (currentIndex < length && bmsEvents[currentIndex].time <= currentTime)
                        {
                            bmsEvent.Invoke(bmsEvents[currentIndex]);
                            currentIndex++;
                        }
                }
                else if (newTime < currentTime)
                {
                    currentTime = newTime;
                    if (currentIndex >= length) currentIndex = length - 1;
                    if (!dispatchEvents || bmsEvent == null)
                        currentIndex = bmsEvents.BinarySearchIndex(
                            new BMSEvent { time = currentTime },
                            BinarySearchMethod.FirstExact | BinarySearchMethod.CeilClosest,
                            0, currentIndex);
                    else
                        while (currentIndex >= 0 && bmsEvents[currentIndex].time >= currentTime)
                        {
                            bmsEvent.Invoke(bmsEvents[currentIndex]);
                            currentIndex--;
                        }
                }
            }

            protected internal virtual void OnBMSRefresh()
            {
                length = chart.bmsEvents.Count;
                if (length > 0)
                    endTime = length > 0 ? chart.bmsEvents[length - 1].time : TimeSpan.Zero;
                currentTime = TimeSpan.MinValue;
                currentIndex = 0;
            }
        }
    }
}
