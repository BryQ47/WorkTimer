/*
WorkTimer
Author: Marcin Bryk

MainController class
*/

using System;
using System.Collections.Generic;
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
            finishButtonCmd = new ButtonCommand(NormalExit, IsButtonActive);
            configButtonCmd = new ButtonCommand(LoadConfiguration, IsButtonActive);
            statsButtonCmd = new ButtonCommand(ShowStatistics, IsButtonActive);
            saveButtonCmd = new ButtonCommand(SaveExit, IsButtonActive);

            counter = new Counter(this);
            config = new ConfigurationManager();
            config.LoadConfiguration();
            stats = new StatisticsManager(config);
            ConfigureTimer();
            counter.Start(config.StartTime);
        }

        /* Refreshes displaing time
        */
        public void UpdateTimeView(int hour, int minute)
        {
            TxtTime = String.Format("{0:D2}:{1:D2}", hour, minute);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("TxtTime"));
            }
        }

        /* Shows periodic alert
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
                    MessageBox.Show(txt + config.GetAdditionalFinishInfo(), "Time Info", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    displayingUrgentMessage = false;
                }
            }
        }

        /* Sets window taskbar visibility
        */
        private void SetTaskbarVisibilityParam(bool visible)
        {
            Application.Current.MainWindow.ShowInTaskbar = visible;
        }

        /* Closes application:
         * Clears session start time and optionally saves statistics
         */
        private void NormalExit()
        {
            string summary = "";
            bool shutdown = true;
            config.ClearSavedTime();

            if (config.StatsEnabled)
            {
                if (config.SummariesEnabled)
                {
                    SummaryDialog summaryDialog = new SummaryDialog();
                    shutdown = (bool)summaryDialog.ShowDialog();
                    summary = summaryDialog.Summary;
                }
                if(shutdown)
                    stats.SaveWorkTime(summary);
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
            var statsView = new StatisticsWindow(stats);
            statsView.ShowDialog();
        }

        private bool IsButtonActive()
        {
            return true; // Buttons always active
        }

        private void LoadConfiguration()
        {
            config.LoadConfiguration();
            ConfigureTimer();
        }


        private void ConfigureTimer()
        {
            // Performing time balance
            LinkedList<PeriodicMessage> alerts = config.AlertsList;
            int workTime = config.DefaultWorkTime;
            if (workTime > 0)
            {
                if (config.BalanceOn)
                {
                    workTime -= stats.Difference / config.BalanceDays;
                }
                alerts.AddLast(new PeriodicMessage(workTime, false, config.OnFinishMsg, PeriodicMessage.MessageType.URGENT));
            }

            counter.MsgList = alerts;
            SetTaskbarVisibilityParam(config.VisibleInTaskbar);
        }
    }
}
