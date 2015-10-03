/*
WorkTimer
Autor: Marcin Bryk

Menedżer konfiguracji
Odpowiada za odczyt plików konfiguracyjnych i zgodną z nimi konfigurację programu
Przechowuje zmienne konfiguracyjne wykorzystywane przez inne moduły.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WorkTimer
{
    public class ConfigurationManager
    {
        private static readonly string CONFIGURATION_FILE = "WTconfig.conf";    // Plik konfigracyjny
        private static readonly string SAVED_TIME_FILE = "WTsaved.conf";        // Plik z czasem rozpoczęcia działania programu
        private static readonly string[] SEPARATORS = { "\t" };                 // Separatory w pliku konfiguracyjnym
        private static readonly string TRUE = "True";                           // Wartość wskazująca wybranie opcji w pliku konf.
        //private static readonly string FALSE = "False";
        private static readonly string NONE = "###";                            // Pusta wartośc w pliku z czasem rozpoczęcia działania programu

        private Counter counter;
        private MainController mainController;

        // Zmienne konfiguracyjne
        public bool ConfEnableExitEventInfo { get; private set; }       // Czy wyświetlane są wiadomości o zadarzeniach czasowych przy komunikacie końcowym
        public int ConfExitEventInfoNumber { get; private set; }        // Liczba wyświelanych zdarzeń czasowych
        public string ConfExitEventInfoHeader { get; private set; }     // Nagłówek dla zdarzeń czasowych
        public string ConfExitEventInfoFile { get; private set; }       // Plik ze zdarzeniami czasowymi
        public bool ConfEnableSummaries { get; private set; }           // Opcja zbierania podsumowań na zakończenie pracy
        public bool ConfVisibleOnTaskbar { get; private set; }          // Widoczność w pasku zadań
        public string ConfHdrBeginnings { get; private set; }           // Nagłówek 1 kolumny w pliku eksportu statystyk
        public string ConfHdrEndings { get; private set; }              // Nagłówek 2 kolumny w pliku eksportu statystyk
        public string ConfHdrPeriods { get; private set; }              // Nagłówek 3 kolumny w pliku eksportu statystyk
        public string ConfHdrComments { get; private set; }             // Nagłówek 4 kolumny w pliku eksportu statystyk
        public string ConfHdrSum { get; private set; }                  // Opis dla sumy czasów
        public string ConfHdrAvg { get; private set; }                  // Opis dla średniej czasów
        public int ConfDefaultWorkTime { get; private set; }            // Domyślny czas pracy - pobierany z okresu powtarzania wiadomości pilnej

        public ConfigurationManager(Counter cnt, MainController v)
        {
            counter = cnt;
            mainController = v;

            ConfEnableExitEventInfo = false;
            ConfExitEventInfoNumber = 0;
            ConfExitEventInfoHeader = "";
            ConfExitEventInfoFile = "";     
            ConfEnableSummaries = false;
            ConfVisibleOnTaskbar = true;
            ConfHdrBeginnings = "Beginning";
            ConfHdrEndings = "Ending";
            ConfHdrPeriods = "Time";
            ConfHdrComments = "Description";
            ConfHdrSum = "Sum";
            ConfHdrAvg = "Average";
    }

        /* Wczytuje zawartość pliku konfiguracyjnrgo
         * W zależności od jego zawartości i decyzji użytkownika:
         * 1) Ładuje zawartość Countera na podstawie zapisanej zawartości
         * 2) Ładuje Counter wartością 00:00 i zapisuje obecny czas w pliku konfiguracyjnym
         * Wczytuje listę komunikatów i zapisuje ją timerze
         * Ustawia czas początku pracy dla zarządcy statystyk (statisctic)
         * Wczytuje aktualne ustawienia dotyczące wyświetlania informacji dodatkowych przy komunikacie końcowym
         * Wczytuje pozostałe ustawienia konfiguracyjne
         */
        public void LoadConfiguration(StatisticsManager statistics)
        {
            string savedValue = File.ReadAllText(SAVED_TIME_FILE);
            DateTime startedAt;

            // Wczytywanie czasu startu aplikacji
            if (!savedValue.Contains(NONE) && mainController.ContinueCountingMessage())
            {
                startedAt = DateTime.Parse(savedValue);
                TimeSpan dt = DateTime.Now.Subtract(startedAt);
                counter.Set(dt.Hours, dt.Minutes);
                //counter.Set(dt.Minutes, dt.Seconds); // @debug
            }
            else
            {
                startedAt = DateTime.Now;
                counter.Set(0, 0);
            }
            statistics.StartTime = startedAt;
            File.WriteAllText(SAVED_TIME_FILE, startedAt.ToString());


            string[] lines = File.ReadAllLines(CONFIGURATION_FILE, Encoding.Unicode);
            int msgOffset = loadConfigVariables(lines);
            counter.MsgList = LoadMessageList(lines, msgOffset);
            mainController.setTaskbarVisibilityParam(ConfVisibleOnTaskbar);
        }

        /* Ładuje do Countera aktualną zawartość listy komunikatów i ponownie wczytuje zmienne konfiguracyjne
         */
        public void ReloadConfiguration()
        {
            string[] lines = File.ReadAllLines(CONFIGURATION_FILE);
            int msgOffset = loadConfigVariables(lines);
            counter.MsgList = LoadMessageList(lines, msgOffset);
            mainController.setTaskbarVisibilityParam(ConfVisibleOnTaskbar);
        }

        /* Unieważnia w pliku konfiguracyjnym wpis dotyczący początku odmierzania czasu
         */
        public void ClearSavedTime()
        {
            File.WriteAllText(SAVED_TIME_FILE, NONE);
        }

        /* Zwraca informace wyświetlane przy komunikacie końcowym (w postaci string) jeśli opcja ta została wybrana w pliku konfiguracyjnym
         * W przeciwnym wypadku zwrócony zostanie pusty string
         */
        public string getExitEventInfo()
        {
            if (ConfEnableExitEventInfo)
            {
                StringBuilder finishInfo = new StringBuilder("\n" + ConfExitEventInfoHeader);
                string line;
                string[] lineContent;
                string[] separators = { ":" };
                var now = DateTime.Now;
                using (var finishInfoFile = new StreamReader(ConfExitEventInfoFile))
                {
                    while((line = finishInfoFile.ReadLine()) != null)
                    {
                        lineContent = line.Split(separators, StringSplitOptions.None);
                        if ((now.Hour > Convert.ToInt32(lineContent[0])) || ((now.Hour == Convert.ToInt32(lineContent[0])) && (now.Minute > Convert.ToInt32(lineContent[1]))))
                            continue;
                        for(int i = 0; (i < ConfExitEventInfoNumber) && (line != null); ++i)
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

        /* Wczytuje zmienne konfiguracyjne
        */
        private int loadConfigVariables(string[] lines)
        {
            int i = 0;

            foreach(string line in lines)
            {

                if(line.StartsWith("ExitEventsEnabled", StringComparison.Ordinal))
                {
                    ConfEnableExitEventInfo = (line.Contains(TRUE)) ? true : false;
                }
                else if (line.StartsWith("ExitEventsCount", StringComparison.Ordinal))
                {
                    ConfExitEventInfoNumber = Convert.ToInt32(line.Substring(line.IndexOf('=') + 1));
                }
                else if (line.StartsWith("ExitEventsFile", StringComparison.Ordinal))
                {
                    ConfExitEventInfoFile = line.Substring(line.IndexOf('=') + 1);
                }
                else if (line.StartsWith("ExitEventsHeader", StringComparison.Ordinal))
                {
                    ConfExitEventInfoHeader = line.Substring(line.IndexOf('=') + 1);
                }
                else if (line.StartsWith("StatisticsBeginningsHeader", StringComparison.Ordinal))
                {
                    ConfHdrBeginnings = line.Substring(line.IndexOf('=') + 1);
                }
                else if (line.StartsWith("StatisticsEndingsHeader", StringComparison.Ordinal))
                {
                    ConfHdrEndings = line.Substring(line.IndexOf('=') + 1);
                }
                else if (line.StartsWith("StatisticsPeriodsHeader", StringComparison.Ordinal))
                {
                    ConfHdrPeriods = line.Substring(line.IndexOf('=') + 1);
                }
                else if (line.StartsWith("StatisticsCommentsHeader", StringComparison.Ordinal))
                {
                    ConfHdrComments = line.Substring(line.IndexOf('=') + 1);
                }
                else if (line.StartsWith("StatisticsSumHeader", StringComparison.Ordinal))
                {
                    ConfHdrSum = line.Substring(line.IndexOf('=') + 1);
                }
                else if (line.StartsWith("StatisticsAverageHeader", StringComparison.Ordinal))
                {
                    ConfHdrAvg = line.Substring(line.IndexOf('=') + 1);
                }
                else if (line.StartsWith("SummariesEnabled", StringComparison.Ordinal))
                {
                    ConfEnableSummaries = (line.Contains(TRUE)) ? true : false;
                }
                else if (line.StartsWith("TaskbarVisibility", StringComparison.Ordinal))
                {
                    ConfVisibleOnTaskbar = (line.Contains(TRUE)) ? true : false;
                }
                else if (line.StartsWith("[MessagesInPriorityOrder]", StringComparison.Ordinal))
                {
                    return i + 2;
                }
                ++i;
            }

            return 1000000;
        }

        /* Zwraca odczytaną z podanej tablicy listę wiadomości
         */
        private LinkedList<PeriodicMessage> LoadMessageList(string[] lines, int tableOffset)
        {
            LinkedList<PeriodicMessage> tmplist = new LinkedList<PeriodicMessage>();
            string[] lineContent;
            ConfDefaultWorkTime = -1;

            for (int i = tableOffset; i < lines.Length; ++i)
            {
                lineContent = lines[i].Split(SEPARATORS, StringSplitOptions.None);
                if (lineContent[0].Contains(TRUE))
                {
                    int period = Convert.ToInt32(lineContent[1]);
                    bool repeat = (lineContent[2].Contains(TRUE)) ? true : false;
                    PeriodicMessage.MessageType type = (lineContent[3].Contains(TRUE)) ? PeriodicMessage.MessageType.URGENT : PeriodicMessage.MessageType.NORMAL;
                    if (type == PeriodicMessage.MessageType.URGENT)
                        ConfDefaultWorkTime = period;
                    string msgtxt = lineContent[4];
                    tmplist.AddLast(new PeriodicMessage(period, repeat, msgtxt, type));
                }
            }

            return tmplist;
        }
    }
}
