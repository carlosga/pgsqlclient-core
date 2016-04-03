// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgTimestamp
        : INullable, IComparable<PgTimestamp>, IComparable, IEquatable<PgTimestamp>
    {
        public static readonly PgTimestamp MaxValue = DateTime.MaxValue;
        public static readonly PgTimestamp MinValue = DateTime.MinValue;
        public static readonly PgTimestamp Null     = new PgTimestamp(false);

        internal const long MicrosecondsPerDay    = 86400000000L;
        // internal const long MicrosecondsPerHour   = 3600000000L;
        // internal const long MicrosecondsPerMinute = 60000000L;
        // internal const long MicrosecondsPerSecond = 1000000L;
        // internal const long SecondsPerDay	      = 86400L;

        internal static readonly long MicrosecondsBetweenEpoch = ((PgDate.PostgresEpochDays - PgDate.UnixEpochDays) * MicrosecondsPerDay);

        private readonly bool     _isNotNull;
        private readonly DateTime _value;
                
        public long TotalMicroseconds => (long)(Value.Subtract(PgDate.PostgresBaseDate).TotalMilliseconds * 1000);

        private PgTimestamp(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _value     = DateTime.Now;
        }

        public PgTimestamp(DateTime value)
        {
            _isNotNull = true;
            _value     = value;
        }

        public PgTimestamp(int year, int month, int day)
            : this(year, month, day, 0, 0, 0, 0)
        {
        }

        public PgTimestamp(int year, int month, int day, int hour, int minute, int second)
            : this(year, month, day, hour, minute, second, 0)
        {
        }

        public PgTimestamp(int year, int month, int day, int hour, int minute, int second, double millisecond)
        {
            _value    = new DateTime(year, month, day, hour, minute, second, (int)millisecond);
           _isNotNull = true;
        }

        public PgTimestamp(long microseconds)
        {
            _isNotNull = true;
            _value     = PgDate.UnixBaseDate.AddMilliseconds(microseconds * 0.001);
        }

        public static PgTimestamp operator -(PgTimestamp x, TimeSpan t)
        {
            if (x.IsNull)
            {
                return Null;
            }
            var value = (x.Value - t);
            if (value < MinValue || value > MaxValue)
            {
                throw new OverflowException();
            }
            return value;
        }

        public static PgBoolean operator !=(PgTimestamp x, PgTimestamp y)
        {
            return !(x == y);
        }

        public static PgTimestamp operator +(PgTimestamp x, TimeSpan t)
        {
            if (x.IsNull)
            {
                return Null;
            }
            var value = (x.Value + t);
            if (value < MinValue || value > MaxValue)
            {
                throw new OverflowException();
            }
            return value;
        }

        public static PgBoolean operator <(PgTimestamp x, PgTimestamp y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgTimestamp x, PgTimestamp y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgTimestamp x, PgTimestamp y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgTimestamp x, PgTimestamp y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgTimestamp x, PgTimestamp y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator DateTime(PgTimestamp x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgDate(PgTimestamp x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgDate(x._value.Date);
        }

        public static explicit operator PgTime(PgTimestamp x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgTime(x._value.TimeOfDay);
        }

        public static explicit operator PgTimestamp(PgString x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return Parse(x.Value);
        }

        public static implicit operator PgTimestamp(DateTime value)
        {
            return new PgTimestamp(value);
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

        public int TimeTicks 
        { 
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return (int)_value.TimeOfDay.Ticks;
            }
        }

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

        public long Microseconds
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                throw new NotImplementedException();
            }
        }

        public static PgTimestamp Add(PgTimestamp x, TimeSpan t)
        {
            return (x + t);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgTimestamp))
            {
                return -1;
            }

            return CompareTo((PgDate)obj);
        }

        public int CompareTo(PgTimestamp value)
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

        public bool Equals(PgTimestamp other)
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

        public static PgBoolean Equals(PgTimestamp x, PgTimestamp y)
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

        public static PgBoolean GreaterThan(PgTimestamp x, PgTimestamp y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgTimestamp x, PgTimestamp y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgTimestamp x, PgTimestamp y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgTimestamp x, PgTimestamp y)
        {
            return (x <= y);
        }

        public static PgBoolean NotEquals(PgTimestamp x, PgTimestamp y)
        {
            return (x != y);
        }

        public static PgTimestamp Parse(string s)
        {
            return DateTime.Parse(s);
        }

        public static PgTimestamp Subtract(PgTimestamp x, TimeSpan t)
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
