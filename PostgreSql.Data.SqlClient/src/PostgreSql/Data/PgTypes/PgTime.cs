// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgTime 
        : INullable, IComparable<PgTime>, IComparable, IEquatable<PgTime>
    {
        public static readonly PgTime MinValue = new PgTime(true ,  0);
        public static readonly PgTime MaxValue = new PgTime(true , 24);
        public static readonly PgTime Null     = new PgTime(false,  0);

        private readonly bool     _isNotNull;
        private readonly TimeSpan _value;

        private PgTime(bool isNotNull, int hours)
        {
            _isNotNull = isNotNull;
            _value     = new TimeSpan(0, hours, 0, 0, 0);
        }

        public PgTime(int hours, int minutes, int seconds)
            : this(hours, minutes, seconds, 0)
        {
        }

        public PgTime(int hours, int minutes, int seconds, int milliseconds)
            : this(new TimeSpan(0, hours, minutes, seconds, milliseconds))
        {
        }

        public PgTime(long microseconds)
            : this(TimeSpan.FromMilliseconds(microseconds * 0.001))
        {
        }

        public PgTime(TimeSpan value)
        {
            _value     = value;
            _isNotNull = true;

            if (_value < MinValue._value || _value > MaxValue._value)
            {
                throw new OverflowException();
            }
        }

        public static PgTime operator -(PgTime x, PgTime y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            var value = x._value - y._value;
            if (value < MinValue._value || value > MaxValue._value)
            {
                throw new OverflowException();
            }
            return (value);
        }

        public static PgBoolean operator !=(PgTime x, PgTime y) => !(x == y);

        public static PgTime operator +(PgTime x, PgTime y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            var value = x._value + y._value;
            if (value < MinValue._value || value > MaxValue._value)
            {
                throw new OverflowException();
            }
            return (value);
        }

        public static PgBoolean operator <(PgTime x, PgTime y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgTime x, PgTime y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgTime x, PgTime y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgTime x, PgTime y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgTime x, PgTime y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator TimeSpan(PgTime x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgTime(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgTime(TimeSpan value) => new PgTime(value);

        public int  Hours             => Value.Hours;
        public int  Milliseconds      => Value.Milliseconds;
        public int  Minutes           => Value.Minutes;
        public int  Seconds           => Value.Seconds;
        public long TotalMicroseconds => (long)Value.TotalMilliseconds * 1000;
        public bool IsNull            => !_isNotNull;

        public TimeSpan Value
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

        public static PgTime Add(PgTime x, PgTime t) => (x + t);

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgTime))
            {
                return -1;
            }

            return CompareTo((PgTime)obj);
        }

        public int CompareTo(PgTime value)
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

        public bool Equals(PgTime other) => (this == other).Value;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgTime))
            {
                return false;
            }
            return Equals((PgTime)obj);
        }

        public static PgBoolean Equals(PgTime x, PgTime y) => (x == y);

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return _value.GetHashCode();
        }

        public static PgBoolean GreatherThan(PgTime x, PgTime y)        => (x > y);
        public static PgBoolean GreatherThanOrEqual(PgTime x, PgTime y) => (x >= y);
        public static PgBoolean LessThan(PgTime x, PgTime y)            => (x < y);
        public static PgBoolean LessThanOrEqual(PgTime x, PgTime y)     => (x <= y);
        public static PgBoolean NotEquals(PgTime x, PgTime y)           => (x != y);

        public static PgTime Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return new PgTime(TimeSpan.Parse(s));
        }

        public static PgTime Subtract(PgTime x, PgTime t) => (x - t);

        public PgString ToPgString() => ToString();

        public override string ToString()
        {
            if (IsNull)
            {
                return PgTypeInfoProvider.NullString;
            }
            return _value.ToString(/*"HH:mm:ss.ffffff", PgTypeInfoProvider.InvariantCulture*/);
        }
    }
}
