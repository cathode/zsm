using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ztools
{
    /// <summary>
    /// Represents a set of data retention policy rules.
    /// </summary>
    public class SnapshotWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotWindow"/> class.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="bucketSize"></param>
        /// <param name="bucketCount"></param>
        /// <param name="bucketCapacity"></param>
        public SnapshotWindow(BucketUnit unit, int bucketSize, int bucketCount, int bucketCapacity)
        {
            this.Unit = unit;
            this.UnitsPerBucket = bucketSize;
            this.BucketCount = bucketCount;
            this.BucketCapacity = bucketCapacity;

            this.Buckets = new List<Bucket>();
        }

        /// <summary>
        /// Gets or sets the unit of the buckets in this snapshot window. (hours, minutes, seconds, days, etc)
        /// </summary>
        public BucketUnit Unit { get; set; }

        /// <summary>
        /// Gets or sets a value which determines how many units each bucket consists of (of whatever unit is specified)
        /// </summary>
        public int UnitsPerBucket { get; set; }

        public int BucketCapacity { get; set; }

        public int BucketCount { get; set; }


        public List<Bucket> Buckets { get; private set; }

        public void MakeBuckets(DateTime reference)
        {
            this.MakeBuckets(reference, reference);
        }

        public void MakeBuckets(DateTime reference, DateTime cutoff)
        {
            var r = reference;
            for (int i = 0; i < this.BucketCount; ++i)
            {
                var b = new Bucket();
                b.Capacity = this.BucketCapacity;

                switch (this.Unit)
                {
                    case ztools.BucketUnit.Minute:
                        r = reference.AddMinutes(this.UnitsPerBucket * -1 * i);

                        b.Start = r.AddMilliseconds(r.Millisecond * -1)
                                   .AddSeconds(r.Second * -1)
                                   .AddMinutes(r.Minute % this.UnitsPerBucket * -1);

                        b.End = b.Start.AddMinutes(this.UnitsPerBucket);
                        break;

                    case ztools.BucketUnit.Hour:
                        r = reference.AddHours(this.UnitsPerBucket * -1 * i);

                        b.Start = r.AddMilliseconds(r.Millisecond * -1)
                                   .AddSeconds(r.Second * -1)
                                   .AddMinutes(r.Minute * -1)
                                   .AddHours(r.Hour % this.UnitsPerBucket * -1);


                        b.End = b.Start.AddHours(this.UnitsPerBucket);
                        break;

                    case ztools.BucketUnit.Day:
                        r = reference.AddDays(this.UnitsPerBucket * -1 * i);

                        b.Start = r.AddMilliseconds(r.Millisecond * -1)
                                   .AddSeconds(r.Second * -1)
                                   .AddMinutes(r.Minute * -1)
                                   .AddHours(r.Hour * -1)
                                   .AddDays(r.Day % this.UnitsPerBucket * -1);

                        b.End = b.Start.AddDays(this.UnitsPerBucket);
                        break;

                    case ztools.BucketUnit.Week:
                        r = reference.AddDays(7 * this.UnitsPerBucket * -1 * i);

                        b.Start = r.AddMilliseconds(r.Millisecond * -1)
                                   .AddSeconds(r.Second * -1)
                                   .AddMinutes(r.Minute * -1)
                                   .AddHours(r.Hour * -1)
                                   .AddDays(r.Day % (7 * this.UnitsPerBucket) * -1);

                        b.End = b.Start.AddDays(this.UnitsPerBucket * 7);

                        break;

                    case ztools.BucketUnit.Month:
                        r = reference.AddMonths(this.UnitsPerBucket * -1 * i);

                        b.Start = r.AddMilliseconds(r.Millisecond * -1)
                                  .AddSeconds(r.Second * -1)
                                  .AddMinutes(r.Minute * -1)
                                  .AddHours(r.Hour * -1)
                                  .AddDays(r.Day * -1)
                                  .AddMonths(r.Month % this.UnitsPerBucket * -1);

                        b.End = b.Start.AddMonths(this.UnitsPerBucket);
                        break;

                    case ztools.BucketUnit.Year:
                        r = reference.AddYears(this.UnitsPerBucket * -1 * i);
                        b.Start = r.AddMilliseconds(r.Millisecond * -1)
                                  .AddSeconds(r.Second * -1)
                                  .AddMinutes(r.Minute * -1)
                                  .AddHours(r.Hour * -1)
                                  .AddDays(r.Day * -1)
                                  .AddMonths(r.Month * -1)
                                  .AddYears(r.Year % this.UnitsPerBucket * -1);

                        b.End = b.Start.AddYears(this.UnitsPerBucket);
                        break;


                    default:
                        throw new NotImplementedException();
                }

                // make the bucket end 1ms before to prevent overlap.
                b.End = b.End.AddMilliseconds(-1);

                if (b.End > cutoff)
                    b.End = cutoff;



                if (b.Start > cutoff || b.Start > b.End || (b.End - b.Start).TotalMilliseconds < 1)
                    continue;

                this.Buckets.Add(b);
            }
        }

        public TimeSpan GetTotalLength()
        {
            // buckets are always added to the list from newest to oldest.
            if (this.Buckets.Count > 0)
            {
                var end = this.Buckets.First().End;
                var begin = this.Buckets.Last().Start;

                return end - begin;
            }
            else
                return TimeSpan.Zero;
        }

    }

    public enum RuleOperation
    {
        All,
        Any,
        None,
    }

    public enum BucketUnit
    {
        /// <summary>
        /// A minute. 60 seconds.
        /// </summary>
        Minute,

        /// <summary>
        /// One hour. 60 minutes.
        /// </summary>
        Hour,

        /// <summary>
        /// One day. 24 hours.
        /// </summary>
        Day,

        /// <summary>
        /// One week. 7 days.
        /// </summary>
        Week,

        /// <summary>
        /// An approximated month. 30 days.
        /// </summary>
        Month,

        /// <summary>
        /// An approximated year. 360 days.
        /// </summary>
        Year
    }
}
