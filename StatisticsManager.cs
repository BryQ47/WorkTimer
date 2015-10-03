/*
WorkTimer
Author: Marcin Bryk

StatisticsManager class
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WorkTimer
{
    public class StatisticsManager
    {
        private static readonly string STATS_FILE = "ConfigurationFiles\\Stats.wt";
        private static readonly string BACKUP_FILE = "ConfigurationFiles\\StatsBak.wt";
        private static readonly char[] SEPARATOR = { '\t' };
        public DateTime StartTime { private get; set; }

        private List<TimeInfo> data;
        private ConfigurationManager config;
        private TimeSpan sum;
        private TimeSpan avg;

        public StatisticsManager(ConfigurationManager configuration)
        {
            data = new List<TimeInfo>();
            config = configuration;
        }

        // Saves worktime to statistics file
        public void SaveWorkTime(string summary)
        {
            using (var statsFile = new StreamWriter(STATS_FILE, true))
            {
                statsFile.WriteLine(
                    StartTime.ToString() + SEPARATOR[0]
                    + DateTime.Now.ToString() + SEPARATOR[0]
                    + DateTime.Now.Subtract(StartTime).ToString() + SEPARATOR[0]
                    + summary);
            }
        }

        // Returns sum and average of work time
        public string PrepareSummary()
        {
            string status;
            if (config.DefaultWorkTime > 0)
            {
                int diff = (int)sum.TotalMinutes - config.DefaultWorkTime * data.Count;
                status = "    Status: " + ((diff > 0) ? ("+" + diff) : (diff.ToString())) + "m";
            }
            else
                status = "";
            return config.HdrSum + ": " + ParseTimeFull(sum) + "    " + config.HdrAvg + ": " + ParseTimeFull(avg) + status;
        }

        // Loads statistics file
        public string LoadStatistics()
        {
            StringBuilder output = new StringBuilder();

            data.Clear();

            using (var statsFile = new StreamReader(STATS_FILE))
            {
                TimeInfo sample;
                string line;
                string[] lineContent;
                int i = 0;

                while ((line = statsFile.ReadLine()) != null)
                {
                    lineContent = line.Split(SEPARATOR);
                    sample.Beginning = DateTime.Parse(lineContent[0]);
                    sample.End = DateTime.Parse(lineContent[1]);
                    sample.Duration = TimeSpan.Parse(lineContent[2]);
                    sample.Description = lineContent[3];
                    data.Add(sample);

                    output.AppendLine(
                        (++i).ToString() + ".\t" +
                        ParseDate(sample.Beginning) + "    " +
                        ParseDate(sample.End) + "    " +
                        ParseTime(sample.Duration)+ "    " +
                        sample.Description);
                }
            }

            sum = TimeSpan.Zero;

            foreach (var element in data)
            {
                sum += element.Duration;
            }

            if (data.Count != 0)
                avg = TimeSpan.FromSeconds(sum.TotalSeconds / data.Count);
            else
                avg = TimeSpan.Zero;

            return output.ToString();
        }

        // Exports statistics to csv
        public void ExportStatistics(string exportPath)
        {
            using (var exportFile = new StreamWriter(exportPath))
            {
                exportFile.WriteLine("\"\",\"" + config.HdrBeginnings + "\",\"" + config.HdrEndings + "\",\"" + config.HdrTime + "\",\"" + config.HdrComments + "\"");

                int i = 0;
                foreach (var element in data)
                {
                    exportFile.WriteLine(
                        "\"" + (++i).ToString() + "\",\"" +
                        ParseDate(element.Beginning) + "\",\"" +
                        ParseDate(element.End) + "\",\"" +
                        ParseTime(element.Duration) + "\",\"" +
                        element.Description + "\"");
                }
                exportFile.WriteLine("\"\",\"\",\"\",\"\",\"\"");
                exportFile.WriteLine("\"" + config.HdrSum + "\",\"" + ParseTimeFull(sum) + "\"");
                exportFile.WriteLine("\"" + config.HdrAvg + "\",\"" + ParseTimeFull(avg) + "\"");
            }

            File.Delete(BACKUP_FILE);
            File.Move(STATS_FILE, BACKUP_FILE);
            using (var statsFile = new StreamWriter(STATS_FILE))
            {
                statsFile.Write("");
            }
        }

        private string ParseDate(DateTime dt)
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

        private string ParseTime(TimeSpan ts)
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

        private string ParseTimeFull(TimeSpan ts)
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
