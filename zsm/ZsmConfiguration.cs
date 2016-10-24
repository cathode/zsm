using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace zsm
{
    public class ZsmConfiguration
    {
        public ZsmConfiguration()
        {

        }
        
        public string HistoryFilePath { get; set; }

        public string ZfsPath { get; set; }

        public SnapshotPolicy[] Policies { get; set; }


        public static ZsmConfiguration GetDefaultConfiguration()
        {
            var config = new ZsmConfiguration();
            config.HistoryFilePath = "history.json";
            config.ZfsPath = "/sbin/zfs";

            var policy = new SnapshotPolicy();

            policy.Datasets = new[] { new DatasetPolicy { Name = "tank", Recursive = true } };

            policy.Schedules = ZsmConfiguration.GetDefaultSchedules();

            config.Policies = new[]
            {
                policy
            };

            return config;
        }

        private static Schedule[] GetDefaultSchedules()
        {
            return new Schedule[]
            {
                new Schedule
                {
                    Name = "Rapid",
                    Unit = TimeUnit.Hour,
                    CountPerUnit = 4,
                    RetentionPeriod = TimeSpan.FromDays(2),
                    Days = Days.Weekdays
                },
                new Schedule
                {
                    Name = "Frequent",
                    Unit = TimeUnit.Day,
                    CountPerUnit = 12,
                    RetentionPeriod = TimeSpan.FromDays(7),
                    Days = Days.Weekdays
                },
                new Schedule
                {
                    Name = "TwiceDaily",
                    Unit = TimeUnit.Day,
                    CountPerUnit = 2,
                    RetentionPeriod = TimeSpan.FromDays(14),
                    Days = Days.All
                },
                new Schedule
                {
                    Name = "Daily",
                    Unit = TimeUnit.Week,
                    CountPerUnit = 7,
                    RetentionPeriod = TimeSpan.FromDays(60),
                    Days = Days.All,
                    Offset = TimeSpan.Parse("8:00:00")
                },
                new Schedule
                {
                    Name = "Weekly",
                    Unit = TimeUnit.Month,
                    CountPerUnit = 4,
                    RetentionPeriod = TimeSpan.FromDays(180),
                },
                new Schedule
                {
                    Name = "Extended",
                    Unit = TimeUnit.Year,
                    CountPerUnit = 8,
                    RetentionPeriod = TimeSpan.FromDays(360),
                    Days = Days.Sunday,
                    Offset = TimeSpan.Parse("08:00:00")
                },
                new Schedule
                {
                    Name = "Archive",
                    Unit = TimeUnit.Year,
                    CountPerUnit = 2,
                    RetentionPeriod = TimeSpan.FromDays(2555), // 7 years
                    Days = Days.Sunday,
                    Offset = TimeSpan.Parse("08:00:00")
                }
            };
        }

        internal void SaveTo(string configPath)
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\r\n", "\n"));
        }

        internal static ZsmConfiguration LoadFrom(string configPath)
        {
            var config = JsonConvert.DeserializeObject<ZsmConfiguration>(File.ReadAllText(configPath));

            return config;
        }
    }
}
