using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime;
using System.IO;

namespace ztools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Counting snapshots.");

            var startinfo = new ProcessStartInfo
            {
                FileName = "zfs",
                Arguments = "list -H -p -t snapshot -o name,creation -r tank",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false
            };

            var proc = new Process { StartInfo = startinfo };

            //proc.Start();

            //var lines = new List<string>();
            var snaps = new List<SnapshotInfo>();

            var startsWith = "auto";

            //var sr = new StreamReader("sample.txt");
            var all = File.ReadAllLines("sample.txt");

            foreach (var ln in all)
            {
                //var ln = sr.ReadLine();
                //lines.Add(ln);
                var parts = ln.Split('\t');
                if (parts.Length > 0)
                {
                    var si = new SnapshotInfo();
                    var namePortion = parts[0];
                    var datePortion = parts[1];
                    var dsNameLength = namePortion.IndexOf('@');
                    si.DatasetName = namePortion.Substring(0, dsNameLength);
                    si.SnapshotName = namePortion.Substring(dsNameLength + 1);

                    if (!si.SnapshotName.StartsWith(startsWith))
                        continue;

                    // zfs list output (with -p) for date is UNIX date stamp (seconds since 00:00:00, Jan 1st, 1970).
                    var unixStamp = long.Parse(datePortion);
                    si.Creation = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixStamp).ToLocalTime();

                    snaps.Add(si);
                    if ((snaps.Count % 100) == 0)
                    {
                        Console.WriteLine("Found {0} snapshots so far.", snaps.Count);
                    }
                }
            }

            // The next-oldest snapshot with the same dataset name.
            SnapshotInfo prev = null;

            foreach (var group in snaps.GroupBy(k => k.DatasetName))
            {
                foreach (var current in group.OrderBy(k => k.Creation))
                {
                    current.Previous = prev;

                    if (prev != null)
                        prev.Next = current;

                    prev = current;
                }
            }

            Console.WriteLine("There are {0} snapshots.", snaps.Count);

            var reference = DateTime.Now;

            var windows = new List<SnapshotWindow>();

            // Apply snapshot windows to list

            // Keep all snapshots in last hour
            windows.Add(new SnapshotWindow(BucketUnit.Minute, 60, 1, 0));

            // For the last 10 hours, keep up to 12 snapshots per hour.
            windows.Add(new SnapshotWindow(BucketUnit.Hour, 1, 10, 12));

            // For the last 72 hours, keep up to 4 snapshots per hour.
            windows.Add(new SnapshotWindow(BucketUnit.Hour, 1, 72, 4));

            // For the last 21 days, keep up to 4 snapshots per day.
            windows.Add(new SnapshotWindow(BucketUnit.Day, 1, 21, 4));

            // For the last 60 days, keep up to 2 snapshots per day.
            windows.Add(new SnapshotWindow(BucketUnit.Day, 1, 60, 2));

            // For the last 12 months, keep 8 snapshots per month
            windows.Add(new SnapshotWindow(BucketUnit.Month, 1, 12, 8));

            // For the last 10 years, keep 4 snapshots per year
            windows.Add(new SnapshotWindow(BucketUnit.Year, 1, 10, 4));

            DateTime cutoff = reference;
            foreach (var window in windows)
            {
                window.MakeBuckets(reference, cutoff);

                //cutoff = (cutoff - window.GetTotalLength()).AddMilliseconds(-1.0);
            }

            foreach (var snap in snaps)
            {
                for (int i = 0; i < windows.Count; ++i)
                {
                    var w = windows[i];

                    for (int j = 0; j < w.Buckets.Count; ++j)
                    {
                        var bucket = w.Buckets[j];

                        if (snap.Creation >= bucket.Start && snap.Creation <= bucket.End)
                        {
                            bucket.Snapshots.Add(snap);
                            snap.Buckets.Add(bucket);
                        }
                    }
                }
            }
            var unmatched = snaps.Where(e => e.Buckets.Count == 0);

            Console.WriteLine(unmatched.Count().ToString() + " snapshots did not land in any buckets.");

            foreach (var window in windows)
            {
                foreach (var bucket in window.Buckets)
                {
                    bucket.Purge();
                }
            }

            var denied = snaps.Where(e => e.Buckets.Count == 0).ToArray();

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var before = new StringBuilder();
            var after = new StringBuilder();
            foreach (var s in snaps.OrderBy(k => k.DatasetName))
            {
                var line = string.Format("{0}@{1}\t{2}", s.DatasetName, s.SnapshotName, Convert.ToInt64((s.Creation - epoch).TotalSeconds));

                before.AppendLine(line);
                
                if (denied.Contains(s))
                    after.AppendLine();
                else
                    after.AppendLine(line);
            }
            File.WriteAllText("before.txt", before.ToString());
            File.WriteAllText("after.txt", after.ToString());
        }
    }
}


