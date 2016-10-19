using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class SnapshotManager
    {
        public SnapshotManager()
        {
            this.Schedules = new List<Schedule>();
        }

        public List<Schedule> Schedules { get; set; }

        public void Start()
        {

        }
    }
}
