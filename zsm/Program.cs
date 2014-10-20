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
            Console.WriteLine("Counting snapshots.");

            var startinfo = new ProcessStartInfo
            {
                FileName = "zfs",
                Arguments = "list -H -t snapshot -o name,creation -s creation -r zroot",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false
            };

            var proc = new Process { StartInfo = startinfo };

            //proc.Start();

            var lines = new List<string>();
            var snaps = new List<SnapshotInfo>();

            var sr = new StreamReader("sample.txt");
            while (!sr.EndOfStream)
            {
                var ln = sr.ReadLine();
                lines.Add(ln);

                var si = new SnapshotInfo();
                var namePortion = ln.Split(' ', '\t')[0];
                var datePortion = ln.Split(new char[] { ' ', '\t' }, 2)[1].Trim().Split(new char[] { ' ', '\t' }, 2)[1].Trim();
                var dsNameLength = namePortion.IndexOf('@');
                si.DatasetName = namePortion.Substring(0, dsNameLength);
                si.SnapshotName = namePortion.Substring(dsNameLength + 1);

                while (datePortion.Contains("  ") || datePortion.Contains('\t'))
                {
                    datePortion = datePortion.Replace("  ", " ");
                    datePortion = datePortion.Replace('\t', ' ');
                }
                si.Creation = DateTime.ParseExact(datePortion, "MMM d H:mm yyyy", System.Globalization.CultureInfo.InvariantCulture);

                snaps.Add(si);

                //DateTime.ParseExact("ddd MMM d H:ss yyyy")

                if ((lines.Count % 100) == 0)
                {
                    Console.WriteLine("Found {0} snapshots so far.", lines.Count);
                }
            }
            Console.WriteLine("There are {0} snapshots.", lines.Count);

            // Apply snapshot windows to list

            var windows = new List<SnapshotWindow>();

            // Keep all snapshots in last hour
            windows.Add(new SnapshotWindow { Age = TimeSpan.FromHours(1.0) });

            // For the last 8 hours, keep all snapshots that are at least 10 minutes apart.
            windows.Add(new SnapshotWindow { Age = TimeSpan.FromHours(8), MinDelta = TimeSpan.FromMinutes(10) });

            // For the last 24 hours, keep all snapshots that are at least 30 minutes apart.
            windows.Add(new SnapshotWindow { Age = TimeSpan.FromHours(24), MinDelta = TimeSpan.FromMinutes(30) });

            // For the last week, keep up to 100 snapshots.
            windows.Add(new SnapshotWindow { Age = TimeSpan.FromDays(7), Capacity = 50 });

            // For the last four weeks, keep all snapshots that are at least 12 hours apart.
            windows.Add(new SnapshotWindow { Age = TimeSpan.FromDays(28), MinDelta = TimeSpan.FromHours(12) });

            // For the last 180 days, keep up to 90 snapshots that are at least 12 hours apart.
            windows.Add(new SnapshotWindow { Age = TimeSpan.FromDays(180), MinDelta = TimeSpan.FromHours(12), Capacity = 90 });

            // Order the list of windows by age, smallest to largest.
            var windowsOrdered = windows.OrderBy(e => e.Age.TotalSeconds).ToList();


            foreach (var snap in snaps)
            {
                foreach (var w in windowsOrdered)
                    if (w.Match(snap.Creation))
                        w.Consider(snap);
            }

            foreach (var w in windowsOrdered)
                w.Apply();

        }
    }
}
