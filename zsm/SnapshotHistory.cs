using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace zsm
{
    public class SnapshotHistory
    {
        public SnapshotHistory()
        {
            this.Snapshots = new List<Snapshot>();
        }

        public List<Snapshot> Snapshots { get; set; }

        public void RecordSnapshot(Snapshot snap)
        {
            this.Snapshots.Add(snap);
        }
        
        public void LoadHistoryJson(string path)
        {
            if (File.Exists(path))
                this.Snapshots = JsonConvert.DeserializeObject<List<Snapshot>>(File.ReadAllText(path)) ?? new List<Snapshot>();
        }

        public void SaveHistoryJson(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this.Snapshots, Formatting.None));
        }

        public IEnumerable<Snapshot> GetExpiredSnapshots()
        {
            return this.GetExpiredSnapshots(DateTime.Now);
        }

        public IEnumerable<Snapshot> GetExpiredSnapshots(DateTime asOfTimestamp)
        {
            
            foreach (var s in this.Snapshots)
            {
                if (s.Expiration <= asOfTimestamp)
                    yield return s;
            }

            yield break;
        }
    }
}
