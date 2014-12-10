using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ztools
{
    /// <summary>
    /// Represents a time interval that contains snapshots.
    /// </summary>
    public class Bucket
    {
        public Bucket()
        {
            this.Snapshots = new Collection<SnapshotInfo>();
        }

        public int Capacity { get; set; }

        public TimeSpan? MinDelta { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public Collection<SnapshotInfo> Snapshots { get; set; }

        internal void Purge()
        {
            if (this.Snapshots.Count == 0)
                return;

            var perDataset = this.Snapshots.GroupBy(k => k.DatasetName);

            if (this.Capacity > 0)
            {
                foreach (var dataset in perDataset)
                {
                    
                    var snaps = dataset.OrderBy(k => k.Creation).ToList();
                    var snapCount = snaps.Count;

                    while (snapCount > this.Capacity)
                    {
                        var needToRemove = snapCount - this.Capacity;
                        var limit = Math.Min(Math.Floor(snapCount / 2.0), needToRemove);

                        var removal = new Queue<SnapshotInfo>();

                        // Mark every other entry for removal
                        for (int i = 0, j = 1; i < limit; ++i, j += 2)
                            removal.Enqueue(snaps[j]);

                       
                        while (removal.Count > 0)
                        {
                            var s = removal.Dequeue();
                            snaps.Remove(s);
                            this.Snapshots.Remove(s);
                            s.Buckets.Remove(this);
                        }
                        snapCount = snaps.Count;
                    }
                }
            }


            if (MinDelta != null)
            {

            }
        }


    }

}
