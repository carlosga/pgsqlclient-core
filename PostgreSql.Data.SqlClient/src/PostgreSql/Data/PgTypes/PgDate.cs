// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgDate
        : INullable, IComparable<PgDate>, IComparable, IEquatable<PgDate>
    {
        public static readonly PgDate MaxValue = DateTime.MaxValue.Date;    // .NET => 01/01/0001 => Postgres =>    4713 BC
        public static readonly PgDate MinValue = DateTime.MinValue.Date;    // .NET => 31/12/999  => Postgres => 5874897 AD
        public static readonly PgDate Null     = new PgDate();

        internal static readonly string   DateStyle         = "ISO";

        internal static readonly long     UnixEpochDays     = 2440588; // 1970, 1, 1
        internal static readonly DateTime UnixBaseDate      = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static readonly long     PostgresEpochDays = 2451545; // 2000, 1, 1
        internal static readonly DateTime PostgresBaseDate  = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private bool     _isNotNull;
        private DateTime _value;

        private PgDate(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _value     = PostgresBaseDate;
        }

        public PgDate(int days)
        {
            _isNotNull = true;
            _value     = PostgresBaseDate.AddDays(days);
        }

        public PgDate(DateTime value)
        {
            _value     = value.Date;
            _isNotNull = true;
        }

        public PgDate(int year, int month, int day)
        {
            _value     = new DateTime(year, month, day);
            _isNotNull = true;
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
            return (x.Value.Add(t));
        }

        public static PgBoolean operator !=(PgDate x, PgDate y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value != y._value);
        }

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

        public static implicit operator PgDate(DateTime value)
        {
            return new PgDate(value);
        }

        public int DayTicks
        { 
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return (int)_value.Date.Ticks;
            } 
        }

        public bool IsNull => !_isNotNull;

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

        public static PgDate Add(PgDate x, TimeSpan t)
        {
            return (x + t);
        }

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

            if (this < value)
            {
                return -1;
            }
            if (this > value)
            {
                return 1;
            }
            return 0;
        }

        public bool Equals(PgDate other)
        {
            return (this == other).Value;
        }

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

        public static PgBoolean Equals(PgDate x, PgDate y)
        {
            return (x == y);
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return _value.GetHashCode();
        }

        public static PgBoolean GreaterThan(PgDate x, PgDate y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgDate x, PgDate y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgDate x, PgDate y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgDate x, PgDate y)
        {
            return (x <= y);
        }

        public static PgBoolean NotEquals(PgDate x, PgDate y)
        {
            return (x != y);
        }

        public static PgDate Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return DateTime.Parse(s);
        }

        public static PgDate Subtract(PgDate x, TimeSpan t)
        {
            return (x - t);
        }

        public PgString ToPgString()
        {
            return ToString();
        }

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
