using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WorkTimer.Logic;
using WorkTimer.Helpers;
using WorkTimer.Views;
using WorkTimer.Models;

namespace WorkTimer.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        private Counter counter;
        private ButtonCommand statsButtonCmd;
        private ButtonCommand finishButtonCmd;
        private ButtonCommand configButtonCmd;
        private ButtonCommand saveButtonCmd;
        private volatile bool displayingNormalMessage;
        private volatile bool displayingUrgentMessage;

        public string TxtTime { get; private set; }
        public bool VisibleInTaskbar { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        // Commands' properties
        public ICommand statsBtnClick { get { return statsButtonCmd; }}
        public ICommand finishBtnClick { get { return finishButtonCmd; } }
        public ICommand configBtnClick { get { return configButtonCmd; } }
        public ICommand saveBtnClick { get { return saveButtonCmd; } }

        public MainViewModel()
        {
            TxtTime = "00:00";
            displayingNormalMessage = false;
            displayingUrgentMessage = false;

            var isButtonActive = new Func<bool>(() => true);
            finishButtonCmd = new ButtonCommand(NormalExit, isButtonActive);
            configButtonCmd = new ButtonCommand(LoadConfiguration, isButtonActive);
            statsButtonCmd = new ButtonCommand(ShowStatistics, isButtonActive);
            saveButtonCmd = new ButtonCommand(SaveExit, isButtonActive);

            LoadConfiguration();
        }

        /* Refreshes displaing time
        */
        public void UpdateTimeView(int minutes)
        {
            int hh = minutes / 60;
            int mm = minutes - 60 * hh;
            TxtTime = String.Format("{0:D2}:{1:D2}", hh, mm);
            var x = PropertyChanged;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TxtTime"));
        }

        /* Shows periodic alert
        */
        public void DisplayAlert(Alert msg)
        {
            if (msg.Type == Alert.MessageType.NORMAL)
            {
                if(!displayingNormalMessage && !displayingUrgentMessage)
                {
                    displayingNormalMessage = true;
                    
                    MessageBox.Show(msg.Text, "Time Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    displayingNormalMessage = false;
                }
            }
            else
            {
                if (!displayingUrgentMessage)
                {
                    displayingUrgentMessage = true;
                    MessageBox.Show(msg.Text, "Time Info", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    displayingUrgentMessage = false;
                }
            }
        }

        /* Sets window taskbar visibility
        */
        private void SetTaskbarVisibilityParam(bool visible)
        {
            VisibleInTaskbar = visible;
        }

        /* Closes application:
         * Clears session start time and optionally saves statistics
         */
        private void NormalExit()
        {
            var config = ConfigurationManager.Instance;
            string summary = "";
            bool shutdown = true;
            config.ClearSavedTime();

            if (config.GatherStatistics)
            {
                if (config.GatherSummaries)
                {
                    SummaryDialog summaryDialog = new SummaryDialog();
                    shutdown = (bool)summaryDialog.ShowDialog();
                    summary = summaryDialog.Summary;
                }
                if(shutdown)
                    StatisticsManager.SaveWorkTime(config.StartTime, summary);
            }
            if(shutdown)
                Application.Current.Shutdown();
        }

        /* Closes application without clearing session start time or saving staistics
         */
        private void SaveExit()
        {
            Application.Current.Shutdown();
        }

        /* Shows statistics view
        */
        private void ShowStatistics()
        {
            var statsView = new StatsView();
            statsView.ShowDialog();
        }

        private void LoadConfiguration()
        {
            var config = ConfigurationManager.Instance;

            SetTaskbarVisibilityParam(config.VisibleInTaskbar);

            // Performing time balance
            LinkedList<Alert> alerts = config.LoadAlerts();
            int workTime = config.DefaultWorkTime - config.AdditionalTime;
            if (config.BalanceOn)
                workTime -= StatisticsManager.Instance.Difference / config.BalanceDays;
            if (workTime > 0)
                alerts.AddLast(new Alert(workTime, false, config.OnFinishMessage, Alert.MessageType.URGENT));

            if (counter != null)
                counter.Dispose();
            counter = new Counter(config.StartTime, UpdateTimeView, DisplayAlert, alerts);
        }
    }
}
