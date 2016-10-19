using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class ZsmConfiguration
    {

        public static ZsmConfiguration GetDefaultConfiguration()
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<Schedule> GetDefaultSchedules()
        {
            return new Schedule[]
            {
                new Schedule
                {
                    Unit = TimeUnit.Hour,
                    CountPerUnit = 4,
                    RetentionPeriod = TimeSpan.FromDays(1),
                    Days = Days.Weekdays
                },
                new Schedule
                {
                    Unit = TimeUnit.Day,
                    CountPerUnit = 12,
                    RetentionPeriod = TimeSpan.FromDays(7),
                    Days = Days.Weekdays
                },
                new Schedule
                {
                    Unit = TimeUnit.Day,
                    CountPerUnit = 2,
                    RetentionPeriod = TimeSpan.FromDays(14),
                    Days = Days.All
                },
                new Schedule
                {
                    Unit = TimeUnit.Week,
                    CountPerUnit = 7,
                    RetentionPeriod = TimeSpan.FromDays(60),
                    Days = Days.All,
                    Offset = TimeSpan.Parse("8:00:00")
                },
                new Schedule
                {
                    Unit = TimeUnit.Month,
                    CountPerUnit = 4,
                    RetentionPeriod = TimeSpan.FromDays(180),
                },
                new Schedule
                {
                    Unit = TimeUnit.Year,
                    CountPerUnit = 8,
                    RetentionPeriod = TimeSpan.FromDays(360),
                    Days = Days.Sunday,
                    Offset = TimeSpan.Parse("08:00:00")
                }
            };
        }
    }
}
