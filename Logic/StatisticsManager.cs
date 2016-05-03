using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WorkTimer.Logic
{
    public class StatisticsManager
    {
        private static readonly string STATS_FILE = "ConfigurationFiles\\Stats.wt";
        private static readonly string BACKUP_FILE = "ConfigurationFiles\\StatsBak.wt";
        private static readonly char[] SEPARATOR = { '\t' };
        private ConfigurationManager config;

        public List<TimeInfo> Data { get; private set; }
        public TimeSpan Sum { get; private set; }
        public TimeSpan Avg { get; private set; }
        public int Difference { get; private set; }
        public string[] ColumnHeaders { get; private set; }
        public string SumHeader { get; private set; }
        public string AvgHeader { get; private set; }

        public StatisticsManager(ConfigurationManager configuration)
        {
            Data = new List<TimeInfo>();
            config = configuration;
            LoadStatistics();
        }

        // Saves worktime to statistics file
        public void SaveWorkTime(string summary)
        {
            using (var statsFile = new StreamWriter(STATS_FILE, true))
            {
                statsFile.WriteLine(
                    config.StartTime.ToString() + SEPARATOR[0]
                    + DateTime.Now.ToString() + SEPARATOR[0]
                    + DateTime.Now.Subtract(config.StartTime).ToString() + SEPARATOR[0]
                    + summary);
            }
        }

        // Loads statistics file
        private void LoadStatistics()
        {
            Data.Clear();

            using (var statsFile = new StreamReader(STATS_FILE))
            {
                TimeInfo sample;
                string line;
                string[] lineContent;
                Sum = TimeSpan.Zero;
                Avg = TimeSpan.Zero;

                while ((line = statsFile.ReadLine()) != null)
                {
                    lineContent = line.Split(SEPARATOR);
                    sample.Beginning = DateTime.Parse(lineContent[0]);
                    sample.End = DateTime.Parse(lineContent[1]);
                    sample.Duration = TimeSpan.Parse(lineContent[2]);
                    sample.Description = lineContent[3];
                    Data.Add(sample);

                    Sum += sample.Duration;
                }
            }

            if (Data.Count != 0)
                Avg = TimeSpan.FromSeconds(Sum.TotalSeconds / Data.Count);

            Difference = (int)Sum.TotalMinutes - config.DefaultWorkTime * Data.Count;

            ColumnHeaders = new string[4];
            ColumnHeaders[0] = config.HdrBeginnings;
            ColumnHeaders[1] = config.HdrEndings;
            ColumnHeaders[2] = config.HdrTime;
            ColumnHeaders[3] = config.HdrComments;
            SumHeader = config.HdrSum;
            AvgHeader = config.HdrAvg;
        }

        // Exports statistics to csv
        public void ExportStatistics(string exportPath)
        {
            using (var exportFile = new StreamWriter(exportPath, false, Encoding.UTF8))
            {

                exportFile.Write("\"");
                foreach (var hdr in ColumnHeaders)
                    exportFile.Write("\",\"" + hdr);
                exportFile.WriteLine("\"");

                int i = 0;
                foreach (var element in Data)
                {
                    exportFile.WriteLine(
                        "\"" + (++i).ToString() + "\",\"" +
                        ParseDate(element.Beginning) + "\",\"" +
                        ParseDate(element.End) + "\",\"" +
                        ParseTime(element.Duration) + "\",\"" +
                        element.Description + "\"");
                }
                exportFile.WriteLine("\"\",\"\",\"\",\"\",\"\"");
                exportFile.WriteLine("\"" + SumHeader + "\",\"" + ParseTimeFull(Sum) + "\"");
                exportFile.WriteLine("\"" + AvgHeader + "\",\"" + ParseTimeFull(Avg) + "\"");
            }

            
            File.Delete(BACKUP_FILE);
            File.Move(STATS_FILE, BACKUP_FILE);
            using (var statsFile = new StreamWriter(STATS_FILE))
            {
                statsFile.Write("");
            }
        }

        static public string ParseDate(DateTime dt)
        {
            StringBuilder s = new StringBuilder();
            if (dt.Day < 10)
                s.Append("0");
            s.Append(dt.Day + ".");
            if (dt.Month < 10)
                s.Append("0");
            s.Append(dt.Month + "." + dt.Year + " ");
            if (dt.Hour < 10)
                s.Append("0");
            s.Append(dt.Hour + ":");
            if (dt.Minute < 10)
                s.Append("0");
            s.Append(dt.Minute);

            return s.ToString();
        }

        static public string ParseTime(TimeSpan ts)
        {
            StringBuilder s = new StringBuilder();
            int hours = ts.Hours + 24*ts.Days;
            int minutes = ts.Minutes;

            if (hours < 10)
                s.Append("0");
            s.Append(hours + ":");
            if (minutes < 10)
                s.Append("0");
            s.Append(minutes);

            return s.ToString();
        }

        static public string ParseTimeFull(TimeSpan ts)
        {
            StringBuilder s = new StringBuilder();
            int hours = ts.Hours + 24 * ts.Days;
            int minutes = ts.Minutes;

            s.Append(hours + "h ");
            s.Append(minutes + "m");

            return s.ToString();
        }
    }

    // Single entry of stats file
    public struct TimeInfo
    {
        public DateTime Beginning;  // Work beginning
        public DateTime End;        // Work end
        public TimeSpan Duration;   // Work time
        public string Description;  // Work summary
    }
}
