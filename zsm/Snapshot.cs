using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class Snapshot 
    {
        public string Dataset { get; set; }

        public string Name { get; set; }

        public bool IsRecursive { get; set; }

        public DateTime Creation { get; set; }

        public DateTime Expiration { get; set; }
    }
}
