using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace zsm
{
    public class PurgeTool
    {
        public void Run(params string[] args)
        {
            string zfsBinaryPath = "/sbin/zfs";
            bool interactive = true;

            Console.WriteLine("Purging expired snapshots.");

            var lines = new List<string>();

            System.IO.StreamReader sr;

            if (interactive)
            {
                var startinfo = new ProcessStartInfo
                {
                    FileName = zfsBinaryPath,
                    Arguments = "list -H -p -t snapshot -o name,creation -r tank",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                };

                var proc = new Process { StartInfo = startinfo };

                proc.Start();

                sr = proc.StandardOutput;
            }
            else
            {
                sr = new System.IO.StreamReader(Console.OpenStandardInput());
            }

            // Read lines (each line is a snapshot entry) from input.
            string lineIn;
            do
            {
                lineIn = sr.ReadLine();

                if (lineIn != null)
                    lines.Add(lineIn);
            }
            while (lineIn != null);

            var snaps = new List<SnapshotInfo>();

            var startsWith = "auto";

            foreach (var line in lines)
            {
                var ln = line.Trim();
                if (string.IsNullOrWhiteSpace(ln))
                    continue;

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
                }
            }

            var reference = DateTime.Now;
            //var reference = new DateTime(2014, 12, 9, 11, 58, 22, DateTimeKind.Local);

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

            foreach (var window in windows)
            {
                foreach (var bucket in window.Buckets)
                {
                    bucket.Purge();
                }
            }

            var destroy = snaps.Where(e => e.Buckets.Count == 0).ToArray();

            int total = destroy.Count();
            int current = 1;

            Console.WriteLine("{0} total snapshots found to be expired.", total);

            foreach (var d in destroy)
            {
                var destroyProcInfo = new ProcessStartInfo
                {
                    FileName = zfsBinaryPath,
                    Arguments = "destroy " + d.DatasetName + "@" + d.SnapshotName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                };

                Console.Write("{1} / {2} Destroying {0}", d.DatasetName + "@" + d.SnapshotName, current, total);

                using (var proc = Process.Start(destroyProcInfo))
                {
                    proc.WaitForExit();
                }
                Console.WriteLine(" ... done.");
                current++;
            }
        }
    }
}
