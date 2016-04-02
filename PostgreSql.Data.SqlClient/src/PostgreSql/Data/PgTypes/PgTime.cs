// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgTime 
        : INullable, IComparable<PgTime>, IComparable, IEquatable<PgTime>
    {
        public static readonly PgTime MinValue = new PgTime( 0, 0, 0);
        public static readonly PgTime MaxValue = new PgTime(24, 0, 0);
        public static readonly PgTime Null     = new PgTime(false);

        public static PgTime Parse(string s)
        {
            return new PgTime(TimeSpan.Parse(s));
        }

        internal static PgTime FromMicroseconds(long microseconds)
        {
            return new PgTime(TimeSpan.FromMilliseconds(microseconds * 0.001));
        }

        private readonly bool     _isNotNull;
        private readonly TimeSpan _value;

        public bool IsNull            => !_isNotNull;
        public int  Hours             => Value.Hours;
        public int  Milliseconds      => Value.Milliseconds;
        public int  Minutes           => Value.Minutes;
        public int  Seconds           => Value.Seconds;
        public long TotalMicroseconds => (long)Value.TotalMilliseconds * 1000;

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

        public PgTime(bool isNotNull)
        {
            _isNotNull = false;
            _value     = TimeSpan.Zero;
        }

        public PgTime(TimeSpan value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgTime(int hours, int minutes, int seconds)
        {
            _value     = new TimeSpan(hours, minutes, seconds);
            _isNotNull = true;
        }

        public PgTime(int hours, int minutes, int seconds, int milliseconds)
        {
            _value     = new TimeSpan(hours, minutes, seconds, milliseconds);
            _isNotNull = true;
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgTime))
            {
                return -1;
            }

            return CompareTo((PgTime)obj);
        }

        public int CompareTo(PgTime other)
        {
            if (IsNull)
            {
                return ((other.IsNull) ? 0 : -1);
            }
            else if (other.IsNull)
            {
                return 1;
            }
            
            return _value.CompareTo(other._value);
        }

        public static bool GreatherThan(PgTime x, PgTime y)        => (x > y);
        public static bool GreatherThanOrEqual(PgTime x, PgTime y) => (x >= y);
        public static bool LessThan(PgTime x, PgTime y)            => (x < y);
        public static bool LessThanOrEqual(PgTime x, PgTime y)     => (x <= y);
        public static bool NotEquals(PgTime x, PgTime y)           => (x != y);

        public static bool operator ==(PgTime left, PgTime right)
        {
            bool equals = false;

            if (left.Value == right.Value)
            {
                equals = true;
            }

            return equals;
        }

        public static bool operator !=(PgTime left, PgTime right)
        {
            bool notequals = false;

            if (left.Value != right.Value)
            {
                notequals = true;
            }

            return notequals;
        }

        public static bool operator >(PgTime left, PgTime right)
        {
            bool greater = false;

            if (left.Value > right.Value)
            {
                greater = true;
            }

            return greater;
        }

        public static bool operator >=(PgTime left, PgTime right)
        {
            bool greater = false;

            if (left.Value >= right.Value)
            {
                greater = true;
            }

            return greater;
        }

        public static bool operator <(PgTime left, PgTime right)
        {
            bool less = false;

            if (left.Value < right.Value)
            {
                less = true;
            }

            return less;
        }

        public static bool operator <=(PgTime left, PgTime right)
        {
            bool less = false;

            if (left.Value <= right.Value)
            {
                less = true;
            }

            return less;
        }

        public static explicit operator TimeSpan(PgTime x) => x.Value;
        public static explicit operator PgTime(string x)   => new PgTime(TimeSpan.Parse(x));
        
        public override string ToString() => _value.ToString();
        public override int GetHashCode() => _value.GetHashCode();

        public bool Equals(PgTime other)
        {
            return (this == other);
        }

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
    }
}
