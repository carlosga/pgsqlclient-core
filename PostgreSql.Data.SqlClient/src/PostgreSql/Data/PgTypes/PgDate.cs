// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgDate
        : INullable, IComparable<PgDate>, IComparable, IEquatable<PgDate>
    {
        public static readonly PgDate MaxValue = new PgDate(true, DateTime.MaxValue.Date); // .NET => 31/12/9999 => Postgres => 5874897 AD
        public static readonly PgDate MinValue = new PgDate(true, DateTime.MinValue.Date); // .NET => 01/01/0001 => Postgres =>    4713 BC
        public static readonly PgDate Null     = new PgDate(false);

        internal static readonly string   DateStyle         = "ISO";
        internal static readonly long     UnixEpochDays     = 2440588; // 1970, 1, 1
        internal static readonly long     PostgresEpochDays = 2451545; // 2000, 1, 1
        internal static readonly DateTime UnixBaseDate      = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly DateTime PostgresBaseDate  = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly bool     _isNotNull;
        private readonly DateTime _value;

        private PgDate(bool isNotNull)
            : this(isNotNull, PostgresBaseDate)
        {
        }

        private PgDate(bool isNotNull, DateTime date)
        {
            _isNotNull = isNotNull;
            _value     = date;
        }

        public PgDate(int year, int month, int day)
            : this(new DateTime(year, month, day))
        {
        }

        public PgDate(int daysSinceEpoch)
            : this(PostgresBaseDate.AddDays(daysSinceEpoch))
        {
        }

        public PgDate(DateTime value)
        {
            _isNotNull = true;
            _value     = value.Date;

            if (_value < MinValue.Value || _value > MaxValue.Value || _value.TimeOfDay != TimeSpan.Zero)
            {
                throw new OverflowException();
            }
        }

        public static PgDate operator -(PgDate x, TimeSpan t)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (t.Hours > 0 || t.Minutes > 0 || t.Seconds > 0 || t.Milliseconds > 0)
            {
                throw new OverflowException();
            }
            return (x._value.Add(t));
        }

        public static PgBoolean operator !=(PgDate x, PgDate y) => !(x == y);

        public static PgDate operator +(PgDate x, TimeSpan t)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (t.Hours > 0 || t.Minutes > 0 || t.Seconds > 0 || t.Milliseconds > 0)
            {
                throw new OverflowException();
            }
            return (x.Value.Subtract(t));
        }

        public static PgBoolean operator <(PgDate x, PgDate y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgDate x, PgDate y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgDate x, PgDate y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgDate x, PgDate y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgDate x, PgDate y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator DateTime(PgDate x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgDate(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgDate(DateTime x) => new PgDate(x);

        public int  DayTicks       => (int)Value.Date.Ticks;
        public int  DaysSinceEpoch => (int)Value.Subtract(PostgresBaseDate).TotalDays;
        public bool IsNull         => !_isNotNull;

        public DateTime Value
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _value;
            }
        }

        public static PgDate Add(PgDate x, TimeSpan t) => (x + t);

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgDate))
            {
                return -1;
            }

            return CompareTo((PgDate)obj);
        }

        public int CompareTo(PgDate value)
        {
            if (IsNull)
            {
                return ((value.IsNull) ? 0 : -1);
            }
            else if (value.IsNull)
            {
                return 1;
            }

            return _value.CompareTo(value._value);
        }

        public bool Equals(PgDate other) => (this == other).Value;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgDate))
            {
                return false;
            }
            return Equals((PgDate)obj);
        }

        public static PgBoolean Equals(PgDate x, PgDate y) => (x == y);

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return _value.GetHashCode();
        }

        public static PgBoolean GreaterThan(PgDate x, PgDate y)        => (x > y);
        public static PgBoolean GreaterThanOrEqual(PgDate x, PgDate y) => (x >= y);
        public static PgBoolean LessThan(PgDate x, PgDate y)           => (x < y);
        public static PgBoolean LessThanOrEqual(PgDate x, PgDate y)    => (x <= y);
        public static PgBoolean NotEquals(PgDate x, PgDate y)          => (x != y);

        public static PgDate Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return DateTime.Parse(s);
        }

        public static PgDate Subtract(PgDate x, TimeSpan t) => (x - t);

        public PgString ToPgString() => ToString();

        public override string ToString()
        {
            if (IsNull)
            {
                return PgTypeInfoProvider.NullString;
            }
            return _value.ToString();
        }
    }
}
