// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// --------------------------------------------------------------------------------------------------------------------------
// ToDate (j2date), ToDays (date2j) and DayOfWeek (j2day) source code has been ported from Ported from PostgreSql Source Code
// Portions Copyright (c) 1996-2016, PostgreSQL Global Development Group
// Portions Copyright (c) 1994, Regents of the University of California

using System;
using System.Runtime.InteropServices;

namespace PostgreSql.Data.PgTypes
{
    public enum Era
    {
        /// The "before common" era (BCE).
        BeforeCommon = 0
        /// The "Common" era (CE).
      , Common       = 1
    }

    /// Immutable struct representing a PostgreSQL date value.
    [StructLayout(LayoutKind.Sequential)]
    public struct PgDate
        : IComparable<PgDate>, IComparable, IEquatable<PgDate>, IFormattable
    {
        enum DatePart
        {
            Year,
            Month,
            Day
        }

        private const double DaysPerYear     = 365.25;	
        private const int    DaysPer4Years   = 1461;        // 365.25   * 4
        private const int    DaysPer400Years = 146097;      // 365.2425 * 400
        private const double DaysPerMonth    = 30.436875;   // 365.2425 / 12
        private const int    MonthsPerYear   = 12;
        private const int    DaysPerWeek     = 7;
        private const int    MinYear         = -4713;
        private const int    MaxYear         = 294276;
        private const int    UnixEpochDays   = 2440588;     // 1970-01-01
        private const int    EpochDays       = 2451545;     // 2000-01-01
        private const int    CommonEraDays   = 1721426;     // 0001-01-01
        private const int    MaxDays         = 109203528;   // 294277-1-1 (with integers)
        private const int    MinDays         = 0;           // 4713 BC

        private static readonly int[]  DaysPerMonthOnNonLeapYear = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        private static readonly int[]  DaysPerMonthOnLeapYear    = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        internal static readonly string DateStyle     = "ISO";
        internal static readonly PgDate CommonEraDate = new PgDate(CommonEraDays);

        public static readonly PgDate MaxValue  = new PgDate(MaxDays);
        public static readonly PgDate MinValue  = new PgDate(MinDays);
        public static readonly PgDate Epoch     = new PgDate(EpochDays);

        public static PgDate Today => new PgDate(DateTime.Today.Date);

        private readonly int _days;

        public Era Era            => (this < CommonEraDate) ? Era.BeforeCommon : Era.Common;
        public int Year           => GetDatePart(_days, DatePart.Year);
        public int Month          => GetDatePart(_days, DatePart.Month);
        public int Day            => GetDatePart(_days, DatePart.Day);
        public int DaysSinceEpoch => (_days - EpochDays);  // Number of days since the start of the PostgreSQL epoch.

        /// Ported from PostgreSql Source Code ( j2day() )
        /// Portions Copyright (c) 1996-2016, PostgreSQL Global Development Group
        /// Portions Copyright (c) 1994, Regents of the University of California
        public DayOfWeek DayOfWeek
        {
            get
            {
                int dow = _days;

                dow += 1;
                dow %= DaysPerWeek;

                // Cope if division truncates towards zero, as it probably does
                if (dow < 0)
                {
                    dow += DaysPerWeek;
                }

                return (DayOfWeek)dow;
            }
        }

        /// The day of the year, expressed as a value between 1 and 366.
        public int DayOfYear
        {
            get
            {
                int day   = 0;
                int month = 0;
                int year  = 0;

                ToDate(_days, ref year, ref month, ref day);

                if (month < 2)
                {
                    return day;
                }

                return (int)((month - 1) * DaysPerMonth) + day;
            }
        }

        public PgDate(DateTime value)
            : this(value.Date.Year, value.Date.Month, value.Date.Day)
        {
        }

        public PgDate(int year, int month, int day)
            : this(ToDays(year, month, day))
        {
            if (!IsValidDate())
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        internal PgDate(int days)
        {
            _days = days;
        }

        //public static PgTimestamp operator -(PgDate x, TimeSpan t)
        //{
        //    if (x.IsNull)
        //    {
        //        return Null;
        //    }
        //    if (t.Hours > 0 || t.Minutes > 0 || t.Seconds > 0 || t.Milliseconds > 0)
        //    {
        //        throw new OverflowException();
        //    }
        //    return (x._value.Add(t));
        //}

        public static bool operator !=(PgDate x, PgDate y) => !(x == y);

        //public static PgTimestamp operator +(PgDate x, TimeSpan t)
        //{
        //    if (x.IsNull)
        //    {
        //        return Null;
        //    }
        //    if (t.Hours > 0 || t.Minutes > 0 || t.Seconds > 0 || t.Milliseconds > 0)
        //    {
        //        throw new OverflowException();
        //    }
        //    return (x.Value.Subtract(t));
        //}

        //public static PgDate operator +(PgDate x, PgInterval i)
        //{
        //}

        public static bool operator <(PgDate x, PgDate y)  => (x._days < y._days);
        public static bool operator <=(PgDate x, PgDate y) => (x._days <= y._days);
        public static bool operator ==(PgDate x, PgDate y) => (x._days == y._days);
        public static bool operator >(PgDate x, PgDate y)  => (x._days > y._days);
        public static bool operator >=(PgDate x, PgDate y) => (x._days >= y._days);

        public static explicit operator DateTime(PgDate x)
        {
            return new DateTime(x.Year, x.Month, x.Day);
        }

        //public static explicit operator PgDate(PgString x)
        //{
        //    if (x.IsNull)
        //    {
        //        return Null;
        //    }
        //    return Parse(x.Value);
        //}

        public static implicit operator PgDate(DateTime x) => new PgDate(x);

        //public static PgTimeSpan Add(PgDate x, TimeSpan t)   => (x + t);
        //public static PgDate     Add(PgDate x, PgInterval i) => (x + i);

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }
            if (!(obj is PgDate))
            {
                throw new System.ArgumentException("obj");
            }
            return CompareTo((PgDate)obj);
        }

        public int CompareTo(PgDate value)
        {
            return _days.CompareTo(value._days);
        }

        public bool Equals(PgDate other) => (this == other);

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgDate))
            {
                throw new System.ArgumentException("obj");
            }
            return Equals((PgDate)obj);
        }

        public static bool Equals(PgDate x, PgDate y)             => (x == y);
        public static bool GreaterThan(PgDate x, PgDate y)        => (x > y);
        public static bool GreaterThanOrEqual(PgDate x, PgDate y) => (x >= y);
        public static bool LessThan(PgDate x, PgDate y)           => (x < y);
        public static bool LessThanOrEqual(PgDate x, PgDate y)    => (x <= y);
        public static bool NotEquals(PgDate x, PgDate y)          => (x != y);

        public override int GetHashCode() => _days.GetHashCode();

        //public static PgDate Parse(string s) => new PgDate();

        //public static PgDate Subtract(PgDate x, TimeSpan t) => (x - t);

        //public PgString ToPgString() => ToString();
        
        public DateTime ToDateTime()
        {
            return (DateTime)this;
        }

        public override string ToString()
        {
            return ToString(null, System.Globalization.CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            int day   = 0;
            int month = 0;
            int year  = 0;

            ToDate(_days, ref year, ref month, ref day);

            return $"{year}/{month}/{day}";
        }

        public static bool IsLeapYear(int year)
        {
            // http://www.postgresql.org/docs/8.0/static/datetime-units-history.html
            // Every year divisible by 4 is a leap year.
            // However, every year divisible by 100 is not a leap year.
            // However, every year divisible by 400 is a leap year after all.
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }

        public PgDate AddDays(int value)   => new PgDate(_days + value);
        public PgDate AddMonths(int value) => AddDays((int)(value * DaysPerMonth));
        public PgDate AddYears(int value)  => AddDays((int)(value * DaysPerYear));
        public PgDate AddWeeks(int value)  => AddDays((int)(value * DaysPerWeek));

        bool IsValidDate()
        {
            int year  = 0;
            int month = 0;
            int day   = 0;

            ToDate(_days, ref year, ref month, ref day);

            return IsValidDate(year, month, day) && (this >= MinValue && this <= MaxValue);
        }

        static bool IsValidDate(int year, int month, int day)
        {
            int maxDay = IsLeapYear(year) ? DaysPerMonthOnLeapYear[month - 1] : DaysPerMonthOnNonLeapYear[month - 1];

            return (year  != 0 && year  >= MinYear && year <= MaxYear
                 && month >= 1 && month <= 12
                 && day   >= 1 && day   <= maxDay);
        }

        static int GetDatePart(int days, DatePart datePart)
        {
            int year  = 0;
            int month = 0;
            int day   = 0;
           
            ToDate(days, ref year, ref month, ref day);

            switch (datePart)
            {
                case DatePart.Year:
                    return year;
                case DatePart.Month:
                    return month;
                case DatePart.Day:
                default:
                    return day;
            }
        }

        /// Ported from PostgreSql Source Code
        /// Portions Copyright (c) 1996-2016, PostgreSQL Global Development Group
        /// Portions Copyright (c) 1994, Regents of the University of California
        static void ToDate(int days, ref int year, ref int month, ref int day)
        {
            int julian  = days + 32044;
            int quad    = julian / DaysPer400Years;
            int extra   = (julian - quad * DaysPer400Years) * 4 + 3;
            int y;

            julian += 60 + quad * 3 + extra / DaysPer400Years;
            quad    = julian / DaysPer4Years;
            julian -= quad * DaysPer4Years;
            y       = julian * 4 / DaysPer4Years;
            julian  = ((y != 0) ? ((julian + 305) % 365) : ((julian + 306) % 366)) + 123;
            y      += quad * 4;
            year    = y - 4800;     // 4800 = months per 400 years ??
            quad    = julian * 2141 / 65536;
            day     = julian - 7834 * quad / 256;
            month   = (quad + 10) % MonthsPerYear + 1;
        }

        /// Ported from PostgreSql Source Code
        /// Portions Copyright (c) 1996-2016, PostgreSQL Global Development Group
        /// Portions Copyright (c) 1994, Regents of the University of California
        static int ToDays(int year, int month, int day)
        {
            int	julian;
            int	century;

            if (month > 2)
            {
                month += 1;
                year  += 4800;      // 4800 = months per 400 years ??
            }
            else
            {
                month += 13;
                year  += 4799;
            }

            century = year / 100;
            julian  = year * 365 - 32167;
            julian += year / 4 - century + century / 4;
            julian += 7834 * month / 256 + day;

            return julian;
        }
    }
}

/// Format specifiers:

// https://msdn.microsoft.com/en-us/library/26etazsy.aspx#IFormattable

/// "d"  | Short date pattern.
/// 2009-06-15T13:45:30 -> 6/15/2009 (en-US)
/// 2009-06-15T13:45:30 -> 15/06/2009 (fr-FR)
/// 2009-06-15T13:45:30 -> 2009/06/15 (ja-JP)

/// "D" | Long date pattern.
/// 2009-06-15T13:45:30 -> Monday, June 15, 2009 (en-US)
/// 2009-06-15T13:45:30 -> 15 июня 2009 г. (ru-RU)
/// 2009-06-15T13:45:30 -> Montag, 15. Juni 2009 (de-DE)

/// "g" | General date/time pattern (short time).
/// 2009-06-15T13:45:30 -> 6/15/2009 1:45 PM (en-US)
/// 2009-06-15T13:45:30 -> 15/06/2009 13:45 (es-ES)
/// 2009-06-15T13:45:30 -> 2009/6/15 13:45 (zh-CN)

///"G" | General date/time pattern (long time).
/// 2009-06-15T13:45:30 -> 6/15/2009 1:45:30 PM (en-US)
/// 2009-06-15T13:45:30 -> 15/06/2009 13:45:30 (es-ES)
/// 2009-06-15T13:45:30 -> 2009/6/15 13:45:30 (zh-CN)

/// "M", "m" | Month/day pattern.
/// 2009-06-15T13:45:30 -> June 15 (en-US)
/// 2009-06-15T13:45:30 -> 15. juni (da-DK)
/// 2009-06-15T13:45:30 -> 15 Juni (id-ID)

/// "s" | Sortable date/time pattern.
/// 2009-06-15T13:45:30 (DateTimeKind.Local) -> 2009-06-15T13:45:30
/// 2009-06-15T13:45:30 (DateTimeKind.Utc) -> 2009-06-15T13:45:30

/// "u" | Universal sortable date/time pattern.
/// With a DateTime value: 2009-06-15T13:45:30 -> 2009-06-15 13:45:30Z
/// With a DateTimeOffset value: 2009-06-15T13:45:30 -> 2009-06-15 20:45:30Z

/// "Y", "y" | Year month pattern.
/// 2009-06-15T13:45:30 -> June, 2009 (en-US)
/// 2009-06-15T13:45:30 -> juni 2009 (da-DK)
/// 2009-06-15T13:45:30 -> Juni 2009 (id-ID)
