using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    /// <summary>
    /// Represents a data retention policy rule.
    /// </summary>
    public class SnapshotWindow
    {

        public SnapshotWindow()
        {
            this.Candidates = new List<SnapshotInfo>();

        }

        public List<SnapshotInfo> Candidates { get; set; }
        /// <summary>
        /// Gets or sets the age (behind 'now') that the snapshot window covers.
        /// </summary>
        public TimeSpan Age { get; set; }

        /// <summary>
        /// Gets or sets the number of snapshots that are retained by this window.
        /// </summary>
        public int? Capacity { get; set; }

        public TimeSpan? MinDelta { get; set; }

        public bool Match(DateTime dt)
        {
            var begin = DateTime.Now - this.Age;
            var end = DateTime.Now;

            if (dt > begin && dt < end)
                return true;

            return false;
        }

        public void Consider(SnapshotInfo snap)
        {
            if (!this.Candidates.Contains(snap))
            {
                this.Candidates.Add(snap);
                snap.CandidateWindows++;
            }
        }

        public void Apply()
        {
            // Apply decimation of snapshots that have not been matched by a narrower window.

            // Group by dataset name

            var grouped = this.Candidates.GroupBy(k => k.DatasetName);
            foreach (var g in grouped)
            {
                var snapshots = new LinkedList<SnapshotInfo>(g.Where(e => e.MatchedWindows == 0).OrderByDescending(k => k.Creation).ToArray());

                // Find upper and lower bounds, avoid rejecting these
                var newest = snapshots.First;
                var oldest = snapshots.Last;

                var rejected = new List<SnapshotInfo>();


                foreach (var e in snapshots)
                {
                    
                }
            }
        }
    }
}
