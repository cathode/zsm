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

        public SnapshotManager()
        {
            this.Policies = new List<SnapshotPolicy>();
        }

        public List<SnapshotPolicy> Policies { get; set; }

        public void ApplyConfiguration(ZsmConfiguration config)
        {
            this.Policies = new List<SnapshotPolicy>(config.Policies);
            this.config = config;
            //this.Schedules = new List<Schedule>(config.Schedules);
        }

        public void Start()
        {
            while (true)
            {
                var now = DateTime.Now;

                var actions = this.Policies.Select(p => p.GetNextAction(now)).OrderBy(k => k.Creation);

                var next = actions.FirstOrDefault();
                var toRun = actions.Where(e => e.Creation == next.Creation);
                var sleepTime = (next.Creation - now);

                Console.WriteLine("Sleeping {0} until next snapshot action.", sleepTime);
                Thread.Sleep(sleepTime);

                foreach (var act in toRun)
                {
                    this.RunAction(act);
                }

                // wait at least 2000ms to reduce wierdness
                Thread.Sleep(2000);
            }
        }

        private void RunAction(SnapAction act)
        {
            Action<string> zfs = delegate (string args) { var ps = Process.Start(this.config.ZfsPath, args); ps.WaitForExit(); };


            foreach (var ds in act.Datasets)
            {
                Console.WriteLine("Performing snapshot on {0} (recursive: {1}), will expire {2}.", ds.Name, ds.Recursive, act.Expiration);

                var args = string.Format("snapshot {0} -o zsm:expiration=\"{1}\" {2}@auto-{3:yyyy.MM.dd-HH.mm.ss}", ds.Recursive ? "-r" : "", act.Expiration, ds.Name, act.Creation);

                try
                {
                    zfs(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
