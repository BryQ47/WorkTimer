/*
WorkTimer
Autor: Marcin Bryk

Główny kontroler programu
Pośredniczy pomiędzy widokiem a modelami: licznika, konfiguracji i statystyk.
*/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace WorkTimer
{
    public class MainController : INotifyPropertyChanged
    {
        private Counter counter;
        private ConfigurationManager config;
        private StatisticsManager stats;
        private ButtonCommand statsButtonCmd;
        private ButtonCommand finishButtonCmd;
        private ButtonCommand configButtonCmd;
        private ButtonCommand saveButtonCmd;
        private volatile bool displayingNormalMessage;
        private volatile bool displayingUrgentMessage;

        public string TxtTime { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        // Commands' properties
        public ICommand statsBtnClick { get { return statsButtonCmd; }}
        public ICommand finishBtnClick { get { return finishButtonCmd; } }
        public ICommand configBtnClick { get { return configButtonCmd; } }
        public ICommand saveBtnClick { get { return saveButtonCmd; } }

        public MainController()
        {
            TxtTime = "00:00";
            displayingNormalMessage = false;
            displayingUrgentMessage = false;
            counter = new Counter(this);
            config = new ConfigurationManager(counter, this);
            stats = new StatisticsManager(config);
            finishButtonCmd = new ButtonCommand(NormalExit, IsButtonActive);
            configButtonCmd = new ButtonCommand(config.ReloadConfiguration, IsButtonActive);
            statsButtonCmd = new ButtonCommand(ShowStatistics, IsButtonActive);
            saveButtonCmd = new ButtonCommand(SaveExit, IsButtonActive);
            config.LoadConfiguration(stats);
            counter.Start();
        }

        /* Odświeżenie podglądu aktualnego czasu pracy
        */
        public void UpdateTimeView(int hour, int minute)
        {
            TxtTime = String.Format("{0:D2}:{1:D2}", hour, minute);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("TxtTime"));
            }
        }

        /* Wyświetlenie komunikatu okresowego
        */
        public void DisplayPeriodicMessage(PeriodicMessage msg)
        {
            string txt = (msg.Repeat) ? string.Format("{0} ({1})", msg.Text, msg.Count) : msg.Text;

            if (msg.Type == PeriodicMessage.MessageType.NORMAL)
            {
                if(!displayingNormalMessage && !displayingUrgentMessage)
                {
                    displayingNormalMessage = true;
                    
                    MessageBox.Show(txt, "Time Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    displayingNormalMessage = false;
                }
            }
            else
            {
                if (!displayingUrgentMessage)
                {
                    displayingUrgentMessage = true;
                    MessageBox.Show(txt + config.getExitEventInfo(), "Time Info", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    displayingUrgentMessage = false;
                }
            }
        }

        /* Wyświetla komunikat dla użytkownika z zapytaniem
         * czy chce kontunuwać pracę z zachowanym uprzenio czasem
         */
        public bool ContinueCountingMessage()
        {
            return (MessageBox.Show("Continue with saved time?", "Time Info", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes);
        }

        /* Ustawia parametr odpowiedzialny za widocznośc w pasku zadań
        */
        public void setTaskbarVisibilityParam(bool visible)
        {
            Application.Current.MainWindow.ShowInTaskbar = visible;
        }

        /* Standardowe zakończenie programu z wyczyszczeniem zapisanego czasu pracy
         * oraz zapisaniem statystyk i ewentualnie podsumowania pracy
         */
        private void NormalExit()
        {
            string summary = "";
            config.ClearSavedTime();

            SummaryDialog summaryDialog = new SummaryDialog();
            if (config.ConfEnableSummaries)
            {
                if (summaryDialog.ShowDialog() == true)
                {
                    summary = summaryDialog.Summary;
                    stats.SaveWorkTime(summary);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                stats.SaveWorkTime(summary);
                Application.Current.Shutdown();
            }
        }

        /* Zakończenie działania programu z opcją wznowienia
         * Nie są zapisywane statystyki
         */
        private void SaveExit()
        {
            Application.Current.Shutdown();
        }

        /* Otworzenie widoku statystyk
        */
        private void ShowStatistics()
        {
            var statsView = new StatisticsWindow(stats);
            statsView.ShowDialog();
        }

        private bool IsButtonActive()
        {
            return true; // Przyciski zawsze aktywne (no chyba, że wyświetlany jest dialog)
        }
    }
}
