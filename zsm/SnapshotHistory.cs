using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace zsm
{
    public class SnapshotHistory
    {
        private ZsmConfiguration config;

        public SnapshotHistory(ZsmConfiguration configuration)
        {
            this.config = configuration;
            this.Snapshots = new List<Snapshot>();
        }

        public List<Snapshot> Snapshots { get; set; }

        public void RecordSnapshot(Snapshot snap)
        {
            this.Snapshots.Add(snap);
        }

        public void ScanHistory()
        {
            var configuredDatasets = this.config.Policies.SelectMany(s => s.Datasets.Select(d => d.Name)).Distinct();

            //string datasetQueryResult;
            string snapshotQueryResult = this.ReadSnapshotHistory();

            // Build tree of datasets
            //datasetQueryResult = File.ReadAllText("datasets.txt");
            //snapshotQueryResult = File.ReadAllText("snapshots.txt");

            var lines = snapshotQueryResult.Split('\n')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));

            var snaps = new List<Snapshot>();

            // Get the full list of acceptable snapshots
            foreach (var ln in lines)
            {
                var parts = ln.Split('\t');

                if (2 == parts.Length)
                {
                    var namePortion = parts[0];
                    var datePortion = parts[1];
                    var dsNameLength = namePortion.IndexOf('@');
                    var dataset = namePortion.Substring(0, dsNameLength);
                    var snapTag = namePortion.Substring(dsNameLength + 1);

                    var policy = config.Policies.FirstOrDefault(p => p.Datasets.Any(e => dataset.StartsWith(e.Name)));

                    if (policy != null)
                    {
                        var snap = new Snapshot();
                        snap.Expiration = DateTime.MaxValue;
                        snap.Dataset = dataset;
                        snap.Name = namePortion;

                        var unixStamp = long.Parse(datePortion);
                        // Round to nearest minute
                        unixStamp = unixStamp - (unixStamp % 60);

                        snap.Creation = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixStamp).ToLocalTime();

                        // Check if prefix matches
                        if (snapTag.StartsWith(policy.Prefix))
                        {
                            snaps.Add(snap);
                        }
                        else
                        {
                            //Logger.Write("Rejecting {0}", namePortion);
                        }
                    }
                }
            }

            var keep = new List<Snapshot>();

            // Filter snapshots to defined policies, and collapse recursive snapshots
            foreach (var policy in config.Policies)
            {
                foreach (var dsp in policy.Datasets)
                {

                    foreach (var group in snaps.Where(p => p.Dataset.StartsWith(dsp.Name)).GroupBy(k => k.Creation))
                    {
                        var creation = group.Key;

                        var next = policy.GetNextAction(creation.Subtract(TimeSpan.FromSeconds(1)));

                        if (next.Creation == creation)
                        {
                            if (dsp.Recursive)
                            {
                                var snap = group.OrderBy(k => k.Dataset).First();
                                snap.Expiration = next.Expiration;
                                keep.Add(snap);
                                //Logger.Write("Keeping {0} (recursive)", snap.Name);
                            }
                            else
                            {
                                foreach (var snap in group)
                                {
                                    snap.Expiration = next.Expiration;
                                    keep.Add(snap);
                                    //Logger.Write("Keeping {0}", snap.Name);
                                }
                            }
                        }
                    }
                }
            }

            this.Snapshots = keep;
        }

        public void LoadHistoryJson(string path)
        {
            if (File.Exists(path))
            {
                var loaded = JsonConvert.DeserializeObject<List<Snapshot>>(File.ReadAllText(path)) ?? new List<Snapshot>();

                //var unique = loaded.GroupBy(k => new { k.Name, k.Creation, k.IsRecursive }).Select(g => g.First()).ToList();
                //var distinct = loaded.Distinct().ToList();

                this.Snapshots = loaded.GroupBy(k => new { k.Name, k.Creation, k.IsRecursive }).Select(g => g.First()).ToList();
            }
        }

        public void SaveHistoryJson(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this.Snapshots, Formatting.None));
        }

        public IEnumerable<Snapshot> GetExpiredSnapshots()
        {
            return this.GetExpiredSnapshots(DateTime.Now);
        }

        public IEnumerable<Snapshot> GetExpiredSnapshots(DateTime asOfTimestamp)
        {

            foreach (var s in this.Snapshots)
            {
                if (s.Expiration <= asOfTimestamp)
                    yield return s;
            }

            yield break;
        }
        
        private string ReadSnapshotHistory()
        {
            var psi = new ProcessStartInfo
            {
                FileName = config.ZfsPath,
                Arguments = "list -H -p -t snapshot -o name,creation",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false
            };

            var proc = new Process { StartInfo = psi };

            proc.Start();

            using (var sr = proc.StandardOutput)
            {
                var str = sr.ReadToEnd();

                return str;
            }
        }
    }
}
