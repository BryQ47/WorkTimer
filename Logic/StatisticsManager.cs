using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WorkTimer.Models;

namespace WorkTimer.Logic
{
    class StatisticsManager
    {
        private static StatisticsManager _instance;

        public static StatisticsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StatisticsManager();
                return _instance;
            }
        }

        private static readonly string STATS_FILE = "ConfigurationFiles\\Stats.txt";
        private static readonly string BACKUP_FILE = "ConfigurationFiles\\StatsBak.txt";
        private static readonly char[] SEPARATOR = { '\t' };

        public List<TimeRecord> Data { get; private set; }
        public TimeSpan Sum { get; private set; }
        public TimeSpan Avg { get; private set; }
        public int Difference { get; private set; }

        private enum Hdr { Beginnings, Endings, Time, Comments, Sum, Avg };

        //private Dictionary<Hdr, string> _headers = new Dictionary<Hdr, string>();

        private Dictionary<Hdr, string> LoadHeaders()
        {
            var hdrsTmp = new Dictionary<string, string>();

            try
            {
                if (File.Exists("ConfigurationFiles\\StatsHeaders.txt"))
                {
                    string[] lines = File.ReadAllLines("ConfigurationFiles\\StatsHeaders.txt", Encoding.UTF8);
                    string[] lineContent;
                    char[] separators = new char[] { '=' };

                    for (int i = 0; i < lines.Length; ++i)
                    {
                        lineContent = lines[i].Split(separators, StringSplitOptions.None);
                        if (!string.IsNullOrWhiteSpace(lineContent[1]))
                            hdrsTmp[lineContent[0]] = lineContent[1];
                    }
                }
            }
            catch (Exception ex)
            {

            }

            var hdrs = new Dictionary<Hdr, string>();

            Action<Hdr, string, string> loadHdr = (hdrType, hdrName, hdrDefaultContent) => { hdrs[hdrType] = hdrsTmp.ContainsKey(hdrName) ? hdrsTmp[hdrName] : hdrDefaultContent; };
            loadHdr(Hdr.Beginnings, "StatsBeginningsHeader", "Beginning");
            loadHdr(Hdr.Endings, "StatsEndingsHeader", "Ending");
            loadHdr(Hdr.Time, "StatsTimeHeader", "Time");
            loadHdr(Hdr.Comments, "StatsCommentsHeader", "Description");
            loadHdr(Hdr.Sum, "StatsSumHeader", "Sum");
            loadHdr(Hdr.Avg, "StatsAverageHeader", "Average");

            return hdrs;
        }

        private StatisticsManager()
        {
            int defaultWorkTime = ConfigurationManager.Instance.DefaultWorkTime;
            Data = new List<TimeRecord>();

            using (var statsFile = new StreamReader(STATS_FILE))
            {
                TimeRecord sample;
                string line;
                string[] lineContent;
                Sum = TimeSpan.Zero;
                Avg = TimeSpan.Zero;

                while ((line = statsFile.ReadLine()) != null)
                {
                    sample = new TimeRecord();
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

            Difference = (int)Sum.TotalMinutes - defaultWorkTime * Data.Count;
        }

        // Saves worktime to statistics file
        public static void SaveWorkTime(DateTime startTime, string summary)
        {
            using (var statsFile = new StreamWriter(STATS_FILE, true))
            {
                statsFile.WriteLine(
                    startTime.ToString() + SEPARATOR[0]
                    + DateTime.Now.ToString() + SEPARATOR[0]
                    + DateTime.Now.Subtract(startTime).ToString() + SEPARATOR[0]
                    + summary);
            }
        }

        // Exports statistics to csv
        public void ExportStatistics(string exportPath)
        {
            var hdrs = LoadHeaders();
            var columnHeaders = new string[4];
            columnHeaders[0] = hdrs[Hdr.Beginnings];
            columnHeaders[1] = hdrs[Hdr.Endings];
            columnHeaders[2] = hdrs[Hdr.Time];
            columnHeaders[3] = hdrs[Hdr.Comments];


            using (var exportFile = new StreamWriter(exportPath, false, Encoding.UTF8))
            {

                exportFile.Write("\"");

                exportFile.Write("\",\"" + hdrs[Hdr.Beginnings]);
                exportFile.Write("\",\"" + hdrs[Hdr.Endings]);
                exportFile.Write("\",\"" + hdrs[Hdr.Time]);
                exportFile.Write("\",\"" + hdrs[Hdr.Comments]);

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
                exportFile.WriteLine("\"" + hdrs[Hdr.Sum] + "\",\"" + ParseTimeFull(Sum) + "\"");
                exportFile.WriteLine("\"" + hdrs[Hdr.Avg] + "\",\"" + ParseTimeFull(Avg) + "\"");
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
}
