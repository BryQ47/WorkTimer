/*
WorkTimer
Author: Marcin Bryk

ConfigurationManager class
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

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
        private Dictionary<string, string> configVariables;

        // Configuration variables
        public bool EnableFinishEventInfo
        {
            get
            {
                return (configVariables["FinishAdditionalInfoEnabled"].ToLower().Contains(TRUE));
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
                return (configVariables["SummariesEnabled"].ToLower().Contains(TRUE));
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
                return (configVariables["VisibleInTaskbar"].ToLower().Contains(TRUE));
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
                return (configVariables["StatsEnables"].ToLower().Contains(TRUE));
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
                return (configVariables["BalanceOn"].ToLower().Contains(TRUE));
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

        public DateTime StartTime { get; private set; }

        public LinkedList<PeriodicMessage> AlertsList { get; set; }

        public ConfigurationManager()
        {
            configVariables = new Dictionary<string, string>();
            Initialize();
        }

        /* Loads start time
         */
        private void Initialize()
        {
            string savedValue = File.ReadAllText(SAVED_TIME_FILE);

            // Check if there is value saved from last session
            if (!savedValue.Contains(NONE) && ShowContinueLastSessionMsgBox())
                StartTime = DateTime.Parse(savedValue);
            else
                StartTime = DateTime.Now;

            File.WriteAllText(SAVED_TIME_FILE, StartTime.ToString());
        }

        /* Loads alerts and configuration variables from files
         */
        public void LoadConfiguration()
        {
            loadConfigVariables();
            AlertsList = LoadMessageList();
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

        private bool ShowContinueLastSessionMsgBox()
        {
            return (MessageBox.Show("Continue with saved time?", "Time Info", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes);
        }
    }
}
