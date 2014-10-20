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

            var sr = new StreamReader("list.txt");
            while (!sr.EndOfStream)
            {
                var ln = sr.ReadLine();
                lines.Add(ln);

                var si = new SnapshotInfo();
                var namePortion = ln.Split(' ')[0];
                var datePortion = ln.Split(new char[] { ' ' }, 2)[1].Trim().Split(new char[] { ' ' }, 2)[1].Trim();
                var dsNameLength = namePortion.IndexOf('@');
                si.DatasetName = namePortion.Substring(0, dsNameLength);
                si.SnapshotName = namePortion.Substring(dsNameLength);

                while (datePortion.Contains("  "))
                {
                    datePortion = datePortion.Replace("  ", " ");
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

            Console.WriteLine("Press d to see a dump of the snapshots");
            var k = Console.ReadKey();

            if (k.KeyChar == 'd')
            {
                foreach (var snap in snaps)
                {
                    Console.WriteLine("Dataset: {0}\t\tSnapshot: {1}\t\tCreation: {2}", snap.DatasetName, snap.SnapshotName, snap.Creation.ToString("u"));
                }
            }

            // Apply snapshot windows to list

            var windows = new List<SnapshotWindow>();
            // Keep all snapshots in last hour
            windows.Add(new SnapshotWindow { Begin = TimeSpan.FromHours(1.0), End = TimeSpan.FromSeconds(0), Capacity = -1 });

            // Keep up to 48 snapshots within the last 24 hours.
            windows.Add(new SnapshotWindow { Begin = TimeSpan.FromHours(24), End = TimeSpan.FromHours(0), Capacity = 48 });

            // Keep 36 snapshots up to a week.
            windows.Add(new SnapshotWindow { Begin = TimeSpan.FromDays(8), End = TimeSpan.FromHours(24), Capacity = 36 });
        }
    }

    public class SnapshotInfo
    {
        public string DatasetName { get; set; }
        public string SnapshotName { get; set; }
        public DateTime Creation { get; set; }
    }

    /// <summary>
    /// Represents a data retention policy rule.
    /// </summary>
    public class SnapshotWindow
    {
        /// <summary>
        /// Gets or sets the age (behind 'now') at which the snapshot window begins.
        /// </summary>
        public TimeSpan Begin { get; set; }

        /// <summary>
        /// Gets or sets the age (behind 'now') at which the snapshot window ends.
        /// </summary>
        public TimeSpan End { get; set; }

        /// <summary>
        /// Gets or sets the number of snapshots that are retained by this window.
        /// </summary>
        public int Capacity { get; set; }

        public bool Match(DateTime dt)
        {

        }
    }
}
