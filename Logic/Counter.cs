using System;
using System.Collections.Generic;
using System.Timers;
using WorkTimer.ViewModels;

namespace WorkTimer.Logic
{
    public class Counter
    {

#if DEBUG
        private static readonly int MINUTE_INTERVAL = 1000;
#else
        private static readonly int MINUTE_INTERVAL = 60000;
#endif

        private MainViewModel view;
        private Timer systemTimer;
        private int hour;
        private int min;
        private int counterValue;
        private readonly object valueLocker;
        public LinkedList<PeriodicMessage> _msgList; 

        public LinkedList<PeriodicMessage> MsgList 
        { 
            private get
            {
                return _msgList;
            } 
            set
            {
                lock(valueLocker)
                {
                    _msgList = value;
                    LinkedListNode<PeriodicMessage> node = _msgList.First;
                    LinkedListNode<PeriodicMessage> next;
            
                    while(node != null)
                    {
                        next = node.Next;
                        if (!node.Value.Repeat && node.Value.Period <= counterValue)
                            _msgList.Remove(node);
                        node = next;
                    }
                }
            } 
        }

        public Counter(MainViewModel cv)
        {
            view = cv;
            valueLocker = new object();
            systemTimer = new Timer(MINUTE_INTERVAL);
            systemTimer.Elapsed += TimerEventHandler;
        }

        public void Set(DateTime startTime)
        {
            TimeSpan dt = DateTime.Now.Subtract(startTime);
#if DEBUG
            hour = dt.Minutes;
            min = dt.Seconds;
#else
            hour = dt.Hours;
            min = dt.Minutes;
#endif
            counterValue = hour * 60 + min;
        }

        /* Starts counter with specific time
        */
        public void Start()
        {

            systemTimer.Start();
            view.UpdateTimeView(hour, min);
        }

        /* Timer event handler
         */
        private void TimerEventHandler(Object source, ElapsedEventArgs e)
        {
            lock (valueLocker)
            {
                ++counterValue;
            }

            if(++min == 60)
            {
                min = 0;
                ++hour;
            }
            view.UpdateTimeView(hour, min);

            PeriodicMessage tmpmsg = null;
            var tmpList = MsgList;
            LinkedListNode<PeriodicMessage> node = tmpList.First;
            LinkedListNode<PeriodicMessage> next;
            
            while(node != null)
            {
                next = node.Next;
                if (counterValue % node.Value.Period == 0)
                {
                    tmpmsg = node.Value;
                    node.Value.Count = counterValue / node.Value.Period;
                    if (!node.Value.Repeat)
                        tmpList.Remove(node);
                }
                node = next;
            }

            if(tmpmsg != null)
                view.DisplayPeriodicMessage(tmpmsg);
        }
    }
}

