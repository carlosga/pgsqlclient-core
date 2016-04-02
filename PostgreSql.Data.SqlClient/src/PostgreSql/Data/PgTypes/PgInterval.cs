
// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgInterval 
        : INullable,  IEquatable<PgInterval>
    {
        public static readonly PgInterval Null     = new PgInterval(TimeSpan.Zero);
        public static readonly PgInterval MaxValue = new PgInterval(TimeSpan.MaxValue);
        public static readonly PgInterval MinValue = new PgInterval(TimeSpan.MinValue);

        public static PgInterval Parse(string s)
        {
            return new PgInterval(TimeSpan.Parse(s));
        }
        
        internal static PgInterval FromInterval(double seconds, double days)
        {
            return new PgInterval(TimeSpan.FromSeconds(seconds).Add(TimeSpan.FromDays(days * 30)));
        }

        private readonly bool     _isNotNull;
        private readonly TimeSpan _value;

        public bool   IsNull            => !_isNotNull;
        public int    Days              => Value.Days;
        public int    Hours             => Value.Hours;
        public int    Milliseconds      => Value.Milliseconds;
        public int    Minutes           => Value.Minutes;
        public int    Seconds           => Value.Seconds;
        public double TotalDays         => Value.TotalDays;
        public double TotalHours        => Value.TotalHours;
        public double TotalMinutes      => Value.TotalMinutes;
        public double TotalSeconds      => Value.TotalSeconds;
        public double TotalMilliseconds => Value.TotalMilliseconds;
        
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

        private PgInterval(bool isNotNull)
        {
            _isNotNull = false;
            _value     = TimeSpan.Zero;
        }

        public PgInterval(TimeSpan interval)
        {
            _isNotNull = true;
            _value     = interval;
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgInterval))
            {
                return -1;
            }
            
            var pgt = (PgInterval)obj;
                        
            return _value.CompareTo(pgt._value);
        }

        public static PgBoolean GreatherThan(PgInterval x, PgInterval y)        => (x > y);
        public static PgBoolean GreatherThanOrEqual(PgInterval x, PgInterval y) => (x >= y);
        public static PgBoolean LessThan(PgInterval x, PgInterval y)            => (x < y);
        public static PgBoolean LessThanOrEqual(PgInterval x, PgInterval y)     => (x <= y);
        public static PgBoolean NotEquals(PgInterval x, PgInterval y)           => (x != y);

        public static PgInterval operator -(PgInterval x, TimeSpan t)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (x._value.Subtract(t));
        }

        public static PgBoolean operator ==(PgInterval x, PgInterval y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator !=(PgInterval x, PgInterval y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value != y._value);
        }

        public static PgBoolean operator >(PgInterval x, PgInterval y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgInterval x, PgInterval y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static PgBoolean operator <(PgInterval x, PgInterval y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgInterval x, PgInterval y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static explicit operator TimeSpan(PgInterval x) => x.Value;
        public static explicit operator PgInterval(string x)   => new PgInterval(TimeSpan.Parse(x));

        public static implicit operator PgInterval(TimeSpan value)
        {
            return new PgInterval(value);
        }

        public string ToPgString()
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

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return _value.GetHashCode();
        }

        public bool Equals(PgInterval other)
        {
            return (this == other).Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgInterval))
            {
                return false;
            }
            return Equals((PgInterval)obj);
        }
    }
}
