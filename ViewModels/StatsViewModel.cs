using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Input;
using System.Collections.Generic;
using System.Text;
using WorkTimer.Helpers;
using WorkTimer.Logic;
using WorkTimer.Models;

namespace WorkTimer.ViewModels
{
    public class StatsViewModel : INotifyPropertyChanged
    {
        public string Data { get; private set; }
        public string Summary { get; private set; }

        public List<TimeRecord> TimeRecords { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private ButtonCommand statsExportCmd;
        public ICommand statsExportBtnClick { get { return statsExportCmd; } }

        public StatsViewModel()
        {
            statsExportCmd = new ButtonCommand(ExportStatistics, ()=>true);
            LoadData();
        }

        // Loads statistics file
        private void LoadData()
        {
            Data = ConvertStatsToString(StatisticsManager.Instance.Data);
            Summary = PrepareSummary();
            TimeRecords = StatisticsManager.Instance.Data;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Data"));
                PropertyChanged(this, new PropertyChangedEventArgs("Summary"));
            }
        }

        // Exports statistics to csv file
        private void ExportStatistics()
        {
            SaveFileDialog exportDialog = new SaveFileDialog();
            exportDialog.DefaultExt = "csv";
            if (exportDialog.ShowDialog() == true)
                StatisticsManager.Instance.ExportStatistics(exportDialog.FileName);
        }

        private string ConvertStatsToString(List<TimeRecord> data)
        {
            int i = 0;
            StringBuilder output = new StringBuilder();
            foreach(var element in data)
            {
                output.AppendLine(
                        (++i).ToString() + ".\t" +
                        StatisticsManager.ParseDate(element.Beginning) + "    " +
                        StatisticsManager.ParseDate(element.End) + "    " +
                        StatisticsManager.ParseTime(element.Duration) + "    " +
                        element.Description);
            }

            return output.ToString();
        }

        private string PrepareSummary()
        {
            var stats = StatisticsManager.Instance;
            string status = "    Status: " + ((stats.Difference > 0) ? ("+" + stats.Difference) : (stats.Difference.ToString())) + "m";
            return "Sum: " + StatisticsManager.ParseTime(stats.Sum) + "    Average: " + StatisticsManager.ParseTime(stats.Avg) + status;

        }
    }
}
