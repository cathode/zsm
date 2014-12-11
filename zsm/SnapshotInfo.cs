using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class SnapshotInfo
    {
        public SnapshotInfo()
        {
            this.Buckets = new Collection<Bucket>();
        }

        public string DatasetName { get; set; }
        public string SnapshotName { get; set; }
        public DateTime Creation { get; set; }

        public int MatchedWindows { get; set; }

        public int CandidateWindows { get; set; }

        public SnapshotInfo Next { get; set; }

        public SnapshotInfo Previous { get; set; }

        public Collection<Bucket> Buckets { get; set; }
    }
}
