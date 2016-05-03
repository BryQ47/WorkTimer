using System;
using System.Collections.Generic;
using System.Timers;
using WorkTimer.Models;

namespace WorkTimer.Logic
{
    class Counter
    {

#if DEBUG
        private static readonly int MINUTE_INTERVAL = 1000;
#else
        private static readonly int MINUTE_INTERVAL = 60000;
#endif

        private Timer systemTimer;
        private int minutesCount;
        private Action<int> TimeUpdateCallback;
        private Action<Alert> DisplayAlertCallback;
        private LinkedList<Alert> alertsList;

        public Counter(Action<int> timeUpdateCallback, Action<Alert> displayAlertCallback)
        {
            TimeUpdateCallback = timeUpdateCallback;
            DisplayAlertCallback = displayAlertCallback;
            systemTimer = new Timer(MINUTE_INTERVAL);
            systemTimer.Elapsed += TimerEventHandler;
            alertsList = new LinkedList<Alert>();
        }

        public void Set(DateTime startTime)
        {
            TimeSpan dt = DateTime.Now.Subtract(startTime);
#if DEBUG
            minutesCount = dt.Minutes * 60 + dt.Seconds;
#else
            minutesCount = dt.Hours * 60 + dt.Minutes;
#endif
        }

        public void SetAlerts(LinkedList<Alert> alerts)
        {
            alertsList = alerts;
        }

        public void Start()
        {

            systemTimer.Start();
            TimeUpdateCallback(minutesCount);
        }

        private void TimerEventHandler(Object source, ElapsedEventArgs e)
        {
            ++minutesCount;
            TimeUpdateCallback(minutesCount);

            Alert activatedAlert = null;
            var alerts = alertsList;
            LinkedListNode<Alert> node = alerts.First;
            LinkedListNode<Alert> next;
            
            while(node != null)
            {
                next = node.Next;

                if (!node.Value.Repeat && node.Value.Period < minutesCount)
                    alerts.Remove(node);
                else if (minutesCount % node.Value.Period == 0)
                    activatedAlert = node.Value;
                node = next;
            }

            if(activatedAlert != null)
                DisplayAlertCallback(activatedAlert);
        }
    }
}

