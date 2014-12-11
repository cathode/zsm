using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
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

            // Divide up the bucket into slots based on its capacity. Each slot is 1/N of the bucket's total duration.
            var slots = new List<Tuple<DateTime, DateTime>>();

            var step = Math.Floor((End - Start).TotalMilliseconds / Capacity);

            var perDataset = this.Snapshots.GroupBy(k => k.DatasetName);

            if (this.Capacity > 0)
            {
                var start = this.Start;
                for (int n = 0; n < this.Capacity - 1; n++)
                {

                    var slot = new Tuple<DateTime, DateTime>(start, start.AddMilliseconds(step));
                    slots.Add(slot);
                    start = slot.Item2.AddMilliseconds(1);
                }

                slots.Add(new Tuple<DateTime, DateTime>(start, this.End));

                // Operate on snapshots by dataset (independently)
                foreach (var dataset in perDataset)
                {
                    var snaps = dataset.OrderBy(k => k.Creation).ToList();
                    var snapCount = snaps.Count;
                    var needToRemove = snapCount - this.Capacity;

                    if (snapCount > this.Capacity)
                    {
                        foreach (var slot in slots)
                        {
                            // If there are more than 1 potential snapshot in this time slot, drop all of them except the newest.
                            var snapsInTimeSlot = snaps.Where(e => e.Creation >= slot.Item1 && e.Creation <= slot.Item2).OrderBy(k => k.Creation).ToArray();

                            var c = snapsInTimeSlot.Count() - 1;

                            for (int i = 0; i < c; ++i)
                            {
                                var s = snapsInTimeSlot[i];
                                snaps.Remove(s);
                                this.Snapshots.Remove(s);
                                s.Buckets.Remove(this);
                            }
                        }
                    }
                }
            }

            // TODO: implement thinning by minimum delta.
            if (MinDelta != null)
            {


            }

        }
    }
}
