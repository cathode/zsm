using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    [Flags]
    public enum Days
    {
        None = 0x00,
        Monday = 0x01,
        Tuesday = 0x02,
        Wednesday = 0x04,
        Thursday = 0x08,
        Friday = 0x10,
        Saturday = 0x20,
        Sunday = 0x40,

        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekend = Saturday | Sunday,

        All = Weekdays | Weekend
    }

    public static class DaysExtensions
    {
        public static Days ToDays(this System.DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Monday:
                    return Days.Monday;

                case DayOfWeek.Tuesday:
                    return Days.Tuesday;

                case DayOfWeek.Wednesday:
                    return Days.Wednesday;

                case DayOfWeek.Thursday:
                    return Days.Thursday;

                case DayOfWeek.Friday:
                    return Days.Friday;

                case DayOfWeek.Saturday:
                    return Days.Saturday;

                case DayOfWeek.Sunday:
                    return Days.Sunday;

                default:
                    return Days.None;
            }
        }
    }
}
