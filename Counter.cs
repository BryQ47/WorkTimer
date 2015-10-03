/*
WorkTimer
Autor: Marcin Bryk

Licznik
Odpowiada za odmierzanie czasu na bieżąco, generację i reagowanie na zdarzenia czasowe.
*/

using System;
using System.Collections.Generic;
using System.Timers;

namespace WorkTimer
{
    public class Counter
    {
        private static readonly int MINUTE_INTERVAL = 60000; //60000; // @debug

        private MainController view;
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

        public Counter(MainController cv)
        {
            view = cv;
            valueLocker = new object();
            systemTimer = new Timer(MINUTE_INTERVAL);
            systemTimer.Elapsed += TimerEventHandler;
        }

        /* Ustawia Counter na zadaną wartość
         * Metoda nie powinna być używana w trakcie działania timera
         */
        public void Set(int hh, int mm)
        {
            hour = hh;
            min = mm;
            counterValue = hh*60+mm;
        }

        /* Start odmierzania czasu
        */
        public void Start()
        {
            systemTimer.Start();
            view.UpdateTimeView(hour, min);
        }

        /* Procedura obsługi wywoływana cyklicznie przez timer
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

