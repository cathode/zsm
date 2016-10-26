using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace zsm
{
    public class SnapshotManager
    {
        private ZsmConfiguration config;

        //Action<string> Zfs;

        public SnapshotManager()
        {
            this.Policies = new List<SnapshotPolicy>();
        }

        public List<SnapshotPolicy> Policies { get; set; }

        public SnapshotHistory History { get; set; }

        public bool IsRunning { get; set; }

        public void ApplyConfiguration(ZsmConfiguration config)
        {
            this.config = config;
            this.Policies = new List<SnapshotPolicy>(config.Policies);

            // Load history
            this.History = new SnapshotHistory(config);
            if (System.IO.File.Exists(config.HistoryFilePath))
            {
                Logger.Write("History file exists at {0}, loading snapshot history...", config.HistoryFilePath);
                this.History.LoadHistoryJson(config.HistoryFilePath);
                Logger.Write("Loaded {0} snapshots from {1}.", this.History.Snapshots.Count, config.HistoryFilePath);
            }
            else
            {
                Logger.Write("No history file exists at {0}, scanning existing snapshots...", config.HistoryFilePath);
                this.History.ScanHistory();
                Logger.Write("Found {0} existing snapshots that matched configured policies.", this.History.Snapshots.Count);

                Logger.Write("Applying zsm expiration to all matched snapshots");

                foreach (var snap in this.History.Snapshots)
                {
                    this.Zfs(string.Format("set zsm:expiration=\"{0}\" {1}", snap.Expiration, snap.Name));
                }

                this.History.SaveHistoryJson(config.HistoryFilePath);
            }
            
        }

        public void Start()
        {

            while (true)
            {
                var now = DateTime.Now;

                var actions = this.Policies.Select(p => new { Policy = p, Action = p.GetNextAction(now) }).OrderBy(k => k.Action.Creation);

                var next = actions.FirstOrDefault();
                var toRun = actions.Where(e => e.Action.Creation == next.Action.Creation).ToArray();
                var sleepTime = (next.Action.Creation - now);

                Logger.Write("Sleeping {0} until next snapshot action.", sleepTime);

                this.SleepUntil(next.Action.Creation);

                foreach (var item in toRun)
                {
                    var act = item.Action;

                    foreach (var ds in act.Datasets)
                    {
                        var snap = new Snapshot();
                        snap.Dataset = ds.Name;
                        snap.Creation = act.Creation;
                        snap.IsRecursive = ds.Recursive;
                        snap.Expiration = act.Expiration;

                        snap.Name = item.Policy.GetSnapshotName(snap);

                        Logger.Write("Performing snapshot on {0} (recursive: {1}), will expire {2}.", ds.Name, ds.Recursive, act.Expiration);

                        var args = string.Format("snapshot {0} -o zsm:expiration=\"{1}\" {2}", ds.Recursive ? "-r" : "", act.Expiration, snap.Name);

                        try
                        {
                            this.Zfs(args);
                            this.History.RecordSnapshot(snap);
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(ex);
                        }
                    }
                }

                // Purge expired snapshots
                var expired = this.History.GetExpiredSnapshots(next.Action.Creation).ToArray();
                if (expired.Length > 0)
                {
                    foreach (var ex in expired)
                    {
                        Logger.Write("Destroying expired snapshot {0}", ex.Name);
                        if (this.DestroySnapshot(ex))
                        {
                            this.History.Snapshots.Remove(ex);
                        }
                        else
                        {
                            Logger.Write("Failed to destroy snapshot {0}", ex.Name);
                        }
                    }
                    Logger.Write("Destroyed {0} expired snapshots", expired.Length);

                    var remain = this.History.Snapshots.Except(expired).ToArray();
                }
                

                this.History.SaveHistoryJson(this.config.HistoryFilePath);
            }
        }

        

        private void Zfs(string args)
        {
            var destroyProcInfo = new ProcessStartInfo
            {
                FileName = this.config.ZfsPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false
            };

            using (var proc = Process.Start(destroyProcInfo))
            {
                proc.WaitForExit();
            }
        }

        private void SleepUntil(DateTime end)
        {
            var remain = end - DateTime.Now;

            while (remain.TotalMilliseconds > 0)
            {
                Thread.Sleep(remain);
                remain = end - DateTime.Now;
            }
        }

        private bool DestroySnapshot(Snapshot sn)
        {
            string args;
            if (sn.IsRecursive)
                args = string.Format("destroy -r \"{0}\"", sn.Name);
            else
                args = string.Format("destroy \"{0}\"", sn.Name);

            try
            {
                this.Zfs(args);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
