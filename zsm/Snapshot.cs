using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class Snapshot : IEquatable<Snapshot>
    {
        public string Name { get; set; }

        public bool IsRecursive { get; set; }

        public DateTime Creation { get; set; }

        public DateTime Expiration { get; set; }

        public bool Equals(Snapshot other)
        {
            return string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase)
                && this.IsRecursive == other.IsRecursive
                && this.Creation == other.Creation
                && this.Expiration == other.Expiration;
        }
    }
}
