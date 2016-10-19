using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class Schedule
    {
        public Schedule()
        {
            this.Offset = TimeSpan.Zero;
            this.Days = Days.All;
        }

        public string Name { get; set; }

        public TimeSpan Offset { get; set; }

        public TimeSpan RetentionPeriod { get; set; }

        public int CountPerUnit { get; set; }

        public TimeUnit Unit { get; set; }

        public Days Days { get; set; }

        public DateTime GetNextOccurrence()
        {
            return this.GetNextOccurrence(DateTime.Now);
        }

        public DateTime GetNextOccurrence(DateTime reference)
        {
            if (this.Days == Days.None)
                throw new InvalidOperationException();

            //var n = DateTime.Parse("2016-10-18 16:45:00");
            var n = reference;
            var result = n;
            double period;
            TimeSpan interval;
            bool daysCorrected = false;
            // Handle allowed days:
            while (!this.Days.HasFlag(n.DayOfWeek.ToDays()))
            {
                n = n.Date.AddDays(1.0);
                daysCorrected = true;
            }

            if (daysCorrected)
            {
                n = n.Subtract(TimeSpan.FromSeconds(0.1));
            }

            switch (this.Unit)
            {
                case TimeUnit.Minute:
                    var sec = n.Second;
                    var secBetween = 60.0 / this.CountPerUnit;

                    interval = TimeSpan.FromSeconds(Math.Ceiling(n.Second / secBetween) * secBetween);
                    result = n.Date.Add(interval)
                        .Add(this.Offset);

                    break;

                case TimeUnit.Hour:
                    var min = n.Minute + (Math.Max(n.Second, 1.0) / 60.0);
                    var minutesBetween = 60.0 / this.CountPerUnit;

                    interval = TimeSpan.FromMinutes(Math.Ceiling(min / minutesBetween) * minutesBetween);

                    result = n.Date.Add(interval)
                        .AddHours(n.Hour)
                        .Add(this.Offset);

                    break;

                case TimeUnit.Day:
                    var hr = n.Hour;
                    var hoursBetween = 24.0 / this.CountPerUnit;

                    var ts = TimeSpan.FromHours(hoursBetween);

                    var hrNext = (n.Hour + (n.Minute / 60.0) + (Math.Max(n.Second, 0.1) / (60 * 60))) / hoursBetween;
                    hrNext = Math.Ceiling(hrNext);
                    ts = TimeSpan.FromHours((hrNext * hoursBetween));

                    result = n.Date.Add(ts)
                        .Add(this.Offset);

                    break;

                case TimeUnit.Week:
                    var day = (int)n.DayOfWeek;
                    var daysBetween = 7.0 / this.CountPerUnit;

                    var dayNext = Math.Ceiling(day / daysBetween);
                    result = (n.Date - TimeSpan.FromDays(day))
                        .AddDays(dayNext)
                        .Add(this.Offset);

                    if (result <= n)
                        result = result.AddDays(1.0);

                    break;

                case TimeUnit.Month:
                    var mday = (int)n.Day;
                    var mdaysBetween = (double)DateTime.DaysInMonth(n.Year, n.Month) / this.CountPerUnit;

                    var mdayNext = Math.Ceiling(mday / mdaysBetween) * Math.Ceiling(mdaysBetween);

                    result = n.Date.AddDays(mdayNext - n.Day).Add(this.Offset);
                    break;

                case TimeUnit.Quarter:

                    result = n.AddDays(100);
                    break;

                case TimeUnit.Year:
                    var yday = n.DayOfYear;
                    var ydayBetween = (double)(new DateTime(n.Year, 12, 31).DayOfYear) / this.CountPerUnit;

                    var ydayNext = Math.Ceiling(yday / ydayBetween) * Math.Ceiling(ydayBetween);

                    result = n.Date.AddDays(ydayNext - n.DayOfYear).Add(this.Offset);

                    while (!this.Days.HasFlag(result.DayOfWeek.ToDays()))
                    {
                        result = result.Date.AddDays(1.0).Add(this.Offset);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            // correct calculated next occurrence
            //while (!this.Days.HasFlag(result.DayOfWeek.ToDays()))
            //{
            //    result = result.Date.AddDays(1.0);  
            //}

            return result;
        }

        public IEnumerable<DateTime> GetOccurrences(DateTime start, DateTime end)
        {
            var t = start;

            while (t < end)
            {
                t = this.GetNextOccurrence(t);
                yield return t;
            }
        }
    }
}
