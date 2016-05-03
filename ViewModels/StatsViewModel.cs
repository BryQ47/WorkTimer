using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Input;
using System.Collections.Generic;
using System.Text;
using WorkTimer.Helpers;
using WorkTimer.Logic;

namespace WorkTimer.ViewModels
{
    public class StatsViewModel : INotifyPropertyChanged
    {
        public string Data { get; private set; }
        public string Summary { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private ButtonCommand statsExportCmd;
        public ICommand statsExportBtnClick { get { return statsExportCmd; } }

        private StatisticsManager stats;

        public StatsViewModel(StatisticsManager s)
        {
            statsExportCmd = new ButtonCommand(ExportStatistics, ()=>true);
            stats = s;
            LoadData();
        }

        // Loads statistics file
        private void LoadData()
        {
            Data = ConvertStatsToString(stats.Data);
            Summary = PrepareSummary();

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
                stats.ExportStatistics(exportDialog.FileName);
        }

        private string ConvertStatsToString(List<TimeInfo> data)
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
            string status = "    Status: " + ((stats.Difference > 0) ? ("+" + stats.Difference) : (stats.Difference.ToString())) + "m";
            return stats.SumHeader + ": " + StatisticsManager.ParseTime(stats.Sum) + "    " + stats.AvgHeader + ": " + StatisticsManager.ParseTime(stats.Avg) + status;

        }
    }
}
