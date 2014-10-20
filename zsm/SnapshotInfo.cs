using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class SnapshotInfo
    {
        public string DatasetName { get; set; }
        public string SnapshotName { get; set; }
        public DateTime Creation { get; set; }

        public int MatchedWindows { get; set; }

        public int CandidateWindows { get; set; }
    }
}
