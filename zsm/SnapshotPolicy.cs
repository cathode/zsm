using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class SnapshotPolicy
    {
        public SnapshotPolicy()
        {
            this.Format = "{0}@{1}{2}";
            this.Prefix = "auto-";
            this.DateFormat = "{0:yyyy.MM.dd-HH.mm.ss}";
        }

        public string Format { get; set; }

        public string Prefix { get; set; }

        public string DateFormat { get; set; }
        
        public DatasetPolicy[] Datasets { get; set; }

        public Schedule[] Schedules { get; set; }

        public string GetSnapshotName(Snapshot snap)
        {
            return string.Format(this.Format, snap.Dataset, this.Prefix, string.Format(this.DateFormat, snap.Creation));
        }

        /// <summary>
        /// Calculates the next snapshot action based on the configured schedules and datasets in the current policy.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public SnapAction GetNextAction(DateTime reference)
        {

            var run = this.Schedules
                .Select(e => new SnapAction(e.GetNextOccurrence(reference), e.RetentionPeriod))
                .GroupBy(k => k.Creation)
                .Select(s => new SnapAction(s.Key, s.Max(g => g.Expiration)))
                .OrderBy(e => e.Creation)
                .FirstOrDefault();
                
            if (run != null)
            {
                run.Datasets = this.Datasets;
                return run;
            }

            throw new NotImplementedException();
        }
    }
}
