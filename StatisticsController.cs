/*
WorkTimer
Author: Marcin Bryk

StatisticsController class
*/

using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Input;

namespace WorkTimer
{
    public class StatisticsController : INotifyPropertyChanged
    {
        public string Data { get; private set; }
        public string Summary { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private ButtonCommand statsExportCmd;
        public ICommand statsExportBtnClick { get { return statsExportCmd; } }

        private StatisticsManager stats;

        public StatisticsController(StatisticsManager s)
        {
            statsExportCmd = new ButtonCommand(ExportStatistics, ()=>true);
            stats = s;
            LoadData();
        }

        // Loads statistics file
        private void LoadData()
        {
            Data = stats.LoadStatistics();
            Summary = stats.PrepareSummary();

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
    }
}
