using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime;
using System.IO;

namespace zsm
{
    class Program
    {
        static void Main(string[] args)
        {
            // read config json file, or create default configuration
            var configPath = Path.GetFullPath("./zsm.json");


            ZsmConfiguration config;

            if (File.Exists(configPath))
            {
                Console.WriteLine("Loading configuration from {0}", configPath);
                config = ZsmConfiguration.LoadFrom(configPath);
            }
            else
            {
                config = ZsmConfiguration.GetDefaultConfiguration();
                config.SaveTo(configPath);
                Console.WriteLine("Created default configuration and saved it at {0}", configPath);
            }

            var manager = new SnapshotManager();

            manager.ApplyConfiguration(config);

            manager.Start();

            //var schedules = config.Policies[0].Schedules;
            //var now = DateTime.Now;
            
            //var times = schedules.Select(e => e.GetOccurrences(now, now.Date.AddDays(365.0)).Select(m => new { When = m, Expires = m + e.RetentionPeriod }).ToArray()).ToArray();

            //var consolidated = times.SelectMany(s => s)
            //    .GroupBy(k => k.When)
            //    .Select(g => new { When = g.Key, Expires = g.Max(e => e.Expires) })
            //    .ToArray();
        }
    }
}

