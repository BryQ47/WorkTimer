/*
WorkTimer
Author: Marcin Bryk

ConfigurationManager class
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WorkTimer
{
    public class ConfigurationManager
    {
        private static readonly string CONFIGURATION_FILE = "ConfigurationFiles\\Config.wt";        // File with configuration variables
        private static readonly string SAVED_TIME_FILE = "ConfigurationFiles\\Saved.wt";            // File with program start time
        private static readonly string ALERTS_FILE = "ConfigurationFiles\\Alerts.wt";               // File with alerst definitions
        private static readonly string[] SEPARATORS = { "\t" };                 // Separators used in alerts file
        private static readonly string TRUE = "true";                           // Value indicating boolean true in configuration files
        private static readonly string FALSE = "false";                         // Value indicating boolean false in configuration files
        private static readonly string NONE = "none";                           // Empty value in program start-time file

        private Counter counter;
        private MainController mainController;
        private Dictionary<string, string> configVariables;

        // Configuration variables
        public bool EnableFinishEventInfo
        {
            get
            {
                return (configVariables["FinishAdditionalInfoEnabled"] == TRUE);
            }
            set
            {
                if (value == true)
                    configVariables["FinishAdditionalInfoEnabled"] = TRUE;
                else
                    configVariables["FinishAdditionalInfoEnabled"] = FALSE;
            }
        }

        public int FinishEventInfoNumber
        {
            get
            {
                return int.Parse(configVariables["FinishAdditionalInfoNumber"]);
            }
            set
            {
                configVariables["FinishAdditionalInfoNumber"] = value.ToString();
            }
        }
                
        public string FinishAdditionalInfoHeader
        {
            get
            {
                return configVariables["FinishAdditionalInfoHeader"];
            }
            set
            {
                configVariables["FinishAdditionalInfoHeader"] = value;
            }
        }

        public string FinishAdditionalInfoFile
        {
            get
            {
                return configVariables["FinishAdditionalInfoFile"];
            }
            set
            {
                configVariables["FinishAdditionalInfoFile"] = value;
            }
        }

        public bool SummariesEnabled
        {
            get
            {
                return (configVariables["SummariesEnabled"] == TRUE);
            }
            set
            {
                if (value == true)
                    configVariables["SummariesEnabled"] = TRUE;
                else
                    configVariables["SummariesEnabled"] = FALSE;
            }
        }

        public bool VisibleInTaskbar
        {
            get
            {
                return (configVariables["VisibleInTaskbar"] == TRUE);
            }
            set
            {
                if (value == true)
                    configVariables["VisibleInTaskbar"] = TRUE;
                else
                    configVariables["VisibleInTaskbar"] = FALSE;
            }
        }

        public bool StatsEnabled
        {
            get
            {
                return (configVariables["StatsEnables"] == TRUE);
            }
            set
            {
                if (value == true)
                    configVariables["StatsEnables"] = TRUE;
                else
                    configVariables["StatsEnables"] = FALSE;
            }
        }

        public string HdrBeginnings
        {
            get
            {
                return configVariables["StatsBeginningsHeader"];
            }
            set
            {
                configVariables["StatsBeginningsHeader"] = value;
            }
        }

        public string HdrEndings
        {
            get
            {
                return configVariables["StatsEndingsHeader"];
            }
            set
            {
                configVariables["StatsEndingsHeader"] = value;
            }
        }

        public string HdrTime
        {
            get
            {
                return configVariables["StatsTimeHeader"];
            }
            set
            {
                configVariables["StatsTimeHeader"] = value;
            }
        }

        public string HdrComments
        {
            get
            {
                return configVariables["StatsCommentsHeader"];
            }
            set
            {
                configVariables["StatsCommentsHeader"] = value;
            }
        }

        public string HdrSum
        {
            get
            {
                return configVariables["StatsSumHeader"];
            }
            set
            {
                configVariables["StatsSumHeader"] = value;
            }
        }

        public string HdrAvg
        {
            get
            {
                return configVariables["StatsAverageHeader"];
            }
            set
            {
                configVariables["StatsAverageHeader"] = value;
            }
        }

        public int DefaultWorkTime
        {
            get
            {
                return int.Parse(configVariables["DefaultWorkTime"]);
            }
            set
            {
                configVariables["DefaultWorkTime"] = value.ToString();
            }
        }

        public string OnFinishMsg
        {
            get
            {
                return configVariables["OnFinishMessage"];
            }
            set
            {
                configVariables["OnFinishMessage"] = value;
            }
        }

        public bool BalanceOn
        {
            get
            {
                return (configVariables["BalanceOn"] == TRUE);
            }
            set
            {
                if (value == true)
                    configVariables["BalanceOn"] = TRUE;
                else
                    configVariables["BalanceOn"] = FALSE;
            }
        }

        public int BalanceDays
        {
            get
            {
                return int.Parse(configVariables["BalanceDays"]);
            }
            set
            {
                configVariables["BalanceDays"] = value.ToString();
            }
        }

        public ConfigurationManager(Counter cnt, MainController v)
        {
            counter = cnt;
            mainController = v;
            configVariables = new Dictionary<string, string>();
        }

        /* Loads program configuration from configuration files and beginning time of last session (if saved)
         */
        public void LoadConfiguration(StatisticsManager statistics)
        {
            string savedValue = File.ReadAllText(SAVED_TIME_FILE);
            DateTime startedAt;

            // Loads program start time
            if (!savedValue.Contains(NONE) && mainController.ContinueCountingMessage())
            {
                startedAt = DateTime.Parse(savedValue);
                TimeSpan dt = DateTime.Now.Subtract(startedAt);
#if DEBUG
                counter.Set(dt.Minutes, dt.Seconds);
#else
                counter.Set(dt.Hours, dt.Minutes);
#endif
            }
            else
            {
                startedAt = DateTime.Now;
                counter.Set(0, 0);
            }
            statistics.StartTime = startedAt;
            File.WriteAllText(SAVED_TIME_FILE, startedAt.ToString());

            // Load remaining configuration elements
            ReloadConfiguration();
        }

        /* Loads alerts and configuration variables from files
         */
        public void ReloadConfiguration()
        {
            loadConfigVariables();
            LinkedList<PeriodicMessage> alerts = LoadMessageList();
            alerts.AddLast(new PeriodicMessage(DefaultWorkTime, false, OnFinishMsg, PeriodicMessage.MessageType.URGENT));
            counter.MsgList = alerts;
            mainController.setTaskbarVisibilityParam(VisibleInTaskbar);
        }

        /* Clears last session saved time
         */
        public void ClearSavedTime()
        {
            File.WriteAllText(SAVED_TIME_FILE, NONE);
        }

        /* Zwraca informace wyświetlane przy komunikacie końcowym (w postaci string) jeśli opcja ta została wybrana w pliku konfiguracyjnym
         * W przeciwnym wypadku zwrócony zostanie pusty string
         */
        public string GetAdditionalFinishInfo()
        {
            if (EnableFinishEventInfo)
            {
                StringBuilder finishInfo = new StringBuilder("\n" + FinishAdditionalInfoHeader);
                string line;
                string[] lineContent;
                string[] separators = { ":" };
                var now = DateTime.Now;
                using (var finishInfoFile = new StreamReader(FinishAdditionalInfoFile))
                {
                    while((line = finishInfoFile.ReadLine()) != null)
                    {
                        lineContent = line.Split(separators, StringSplitOptions.None);
                        if ((now.Hour > Convert.ToInt32(lineContent[0])) || ((now.Hour == Convert.ToInt32(lineContent[0])) && (now.Minute > Convert.ToInt32(lineContent[1]))))
                            continue;
                        for(int i = 0; (i < FinishEventInfoNumber) && (line != null); ++i)
                        {
                            lineContent = line.Split(separators, StringSplitOptions.None);
                            finishInfo.Append("\n" + lineContent[0] + ":" + lineContent[1] + " - " + lineContent[2]);
                            line = finishInfoFile.ReadLine();
                        }
                        break;
                    }
                }
                return finishInfo.ToString();
            }
            else
                return "";
        }

        /* Loads configuration variables from file
        */
        private void loadConfigVariables()
        {
            string[] lines = File.ReadAllLines(CONFIGURATION_FILE, Encoding.Unicode);

            foreach (string line in lines)
            {
                if (line.StartsWith("#") || line == "") // Skip comment and empty lines
                    continue;
                int separatorIndex = line.IndexOf('=');
                configVariables[line.Substring(0, separatorIndex)] = line.Substring(separatorIndex + 1);
            }
        }

        /* Loads alerts list form file
         */
        private LinkedList<PeriodicMessage> LoadMessageList()
        {
            LinkedList<PeriodicMessage> tmplist = new LinkedList<PeriodicMessage>();
            string[] lines = File.ReadAllLines(ALERTS_FILE, Encoding.Unicode);
            string[] lineContent;

            for (int i = 1; i < lines.Length; ++i)
            {
                lineContent = lines[i].Split(SEPARATORS, StringSplitOptions.None);
                if (lineContent[0].Contains(TRUE))
                {
                    int period = Convert.ToInt32(lineContent[1]);
                    bool repeat = (lineContent[2].Contains(TRUE)) ? true : false;
                    string msgtxt = lineContent[3];
                    tmplist.AddLast(new PeriodicMessage(period, repeat, msgtxt, PeriodicMessage.MessageType.NORMAL));
                }
            }

           return tmplist;
        }
    }
}
