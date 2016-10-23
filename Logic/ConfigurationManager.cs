using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using WorkTimer.Models;
using WorkTimer.Properties;

namespace WorkTimer.Logic
{
    public class ConfigurationManager
    {
        private Settings _settings = Settings.Default;
        private static readonly string SAVED_TIME_FILE = "ConfigurationFiles\\Saved.txt";            // File with program start time
        private static readonly string ALERTS_FILE = "ConfigurationFiles\\Alerts.txt";               // File with alerst definitions
        private static readonly string[] SEPARATORS = { "\t" };                 // Separators used in alerts file
        private static readonly string TRUE = "true";                           // Value indicating boolean true in configuration files
        private static readonly string NONE = "none";                           // Empty value in program start-time file

        public bool GatherSummaries
        {
            get
            {
                return _settings.GatherSummaries;
            }
        }

        public bool VisibleInTaskbar
        {
            get
            {
                return _settings.VisibleInTaskbar;
            }
        }

        public bool GatherStatistics
        {
            get
            {
                return _settings.GatherStatistics;
            }
        }

        public int DefaultWorkTime
        {
            get
            {
                return _settings.DefaultWorkTime;
            }
        }

        public string OnFinishMessage
        {
            get
            {
                return _settings.OnFinishMessage;
            }
        }

        public bool BalanceOn
        {
            get
            {
                return _settings.BalanceOn;
            }
        }

        public int BalanceDays
        {
            get
            {
                return _settings.BalanceDays;
            }
        }

        public int AdditionalTime
        {
            get
            {
                return _settings.AdditionalTime;
            }
        }

        public DateTime StartTime { get; private set; }

        public ConfigurationManager()
        {
            string savedValue = File.ReadAllText(SAVED_TIME_FILE);

            // Check if there is value saved from last session
            if (!savedValue.Contains(NONE) && ShowContinueLastSessionMsgBox())
                StartTime = DateTime.Parse(savedValue);
            else
                StartTime = DateTime.Now;

            File.WriteAllText(SAVED_TIME_FILE, StartTime.ToString());
        }

        public void ClearSavedTime()
        {
            File.WriteAllText(SAVED_TIME_FILE, NONE);
        }

        public LinkedList<Alert> LoadAlerts()
        {
            LinkedList<Alert> tmplist = new LinkedList<Alert>();
            string[] lines = File.ReadAllLines(ALERTS_FILE, Encoding.UTF8);
            string[] lineContent;

            for (int i = 1; i < lines.Length; ++i)
            {
                lineContent = lines[i].Split(SEPARATORS, StringSplitOptions.None);
                if (lineContent[0].Contains(TRUE))
                {
                    int period = Convert.ToInt32(lineContent[1]);
                    bool repeat = (lineContent[2].Contains(TRUE)) ? true : false;
                    string msgtxt = lineContent[3];
                    tmplist.AddLast(new Alert(period, repeat, msgtxt, Alert.MessageType.NORMAL));
                }
            }

           return tmplist;
        }

        private bool ShowContinueLastSessionMsgBox()
        {
            return (MessageBox.Show("Continue with saved time?", "Time Info", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes);
        }
    }
}
