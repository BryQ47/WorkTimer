using System;
using System.Collections.Generic;
using System.Timers;
using WorkTimer.Models;

namespace WorkTimer.Logic
{
    class Counter : IDisposable
    {

#if DEBUG
        private static readonly int MINUTE_INTERVAL = 1000;
#else
        private static readonly int MINUTE_INTERVAL = 60000;
#endif

        private Timer _timer;
        private int _minutesCount;
        private Action<int> _updateDisplay;
        private Action<Alert> _showAlert;
        private LinkedList<Alert> _alerts;

        public Counter(DateTime startTime, Action<int> timeUpdateCallback, Action<Alert> displayAlertCallback, LinkedList<Alert> alerts)
        {
            _updateDisplay = timeUpdateCallback;
            _showAlert = displayAlertCallback;

            TimeSpan dt = DateTime.Now.Subtract(startTime);
#if DEBUG
            _minutesCount = dt.Minutes * 60 + dt.Seconds;
#else
            minutesCount = dt.Hours * 60 + dt.Minutes;
#endif

            _alerts = alerts;

            _timer = new Timer(MINUTE_INTERVAL);
            _timer.Elapsed += TimerEventHandler;
            _timer.Start();
            _updateDisplay(_minutesCount);
        }

        private void TimerEventHandler(Object source, ElapsedEventArgs e)
        {
            ++_minutesCount;
            _updateDisplay(_minutesCount);

            Alert activatedAlert = null;
            var alerts = _alerts;
            LinkedListNode<Alert> node = alerts.First;
            LinkedListNode<Alert> next;
            
            while(node != null)
            {
                next = node.Next;
                
                if (_minutesCount % node.Value.Period == 0)
                {
                    if (node.Value.Repeat)
                    {
                        activatedAlert = node.Value;
                    }
                    else
                    {
                        if (node.Value.Period == _minutesCount)
                            activatedAlert = node.Value;
                        alerts.Remove(node);
                    }
                }
                    
                node = next;
            }

            if(activatedAlert != null)
                _showAlert(activatedAlert);
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}

