// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgTime 
        : IComparable, INullable
    {
        internal static readonly string[] TimeFormats = new string[]
        {
            "HH:mm:ss",
            "HH:mm:ss.f",
            "HH:mm:ss.ff",
            "HH:mm:ss.fff",
            "HH:mm:ss.ffff",
            "HH:mm:ss.fffff",
            "HH:mm:ss.ffffff",
            "HH:mm:sszz",
            "HH:mm:ss.fzz",
            "HH:mm:ss.ffzz",
            "HH:mm:ss.fffzz",
            "HH:mm:ss.ffffzz",
            "HH:mm:ss.fffffzz",
            "HH:mm:ss.ffffffzz",
            "HH:mm:sszz",
            "HH:mm:ss.f zz",
            "HH:mm:ss.ff zz",
            "HH:mm:ss.fff zz",
            "HH:mm:ss.ffff zz",
            "HH:mm:ss.fffff zz",
            "HH:mm:ss.ffffff zz",
        };

        public static PgTime Parse(string s)
        {
            return new PgTime(TimeSpan.Parse(s));
        }

        public static readonly PgTime Null     = new PgTime();
        public static readonly PgTime MinValue = new PgTime( 0, 0, 0);
        public static readonly PgTime MaxValue = new PgTime(24, 0, 0);

        private readonly bool     _isNotNull;
        private readonly TimeSpan _value;

        public bool IsNull       => !_isNotNull;
        public int  Hours        => _value.Hours;
        public int  Milliseconds => _value.Milliseconds;
        public int  Minutes      => _value.Minutes;
        public int  Seconds      => _value.Seconds;

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
            
            var pgt = (PgTime)obj;
            
            if (pgt == null)
            {
                return -1;
            }
            
            return _value.CompareTo(pgt._value);
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

            PgTime value = (PgTime)obj;

            return ((PgTime)value) == this;
        }
    }
}
