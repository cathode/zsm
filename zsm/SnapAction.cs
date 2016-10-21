using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class SnapAction
    {
        public SnapAction()
        {

        }
        public SnapAction(DateTime creation, DateTime expiration)
        {
            this.Creation = creation;
            this.Expiration = expiration;
        }

        public SnapAction(DateTime creation, TimeSpan retention)
        {
            this.Creation = creation;
            this.Expiration = creation.Add(retention);
        }
        public IEnumerable<DatasetPolicy> Datasets { get; set; }

        public DateTime Creation { get; set; }

        public DateTime Expiration { get; set; }
    }
}
