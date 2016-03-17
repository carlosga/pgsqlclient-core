// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgTimeSpan 
        : IComparable
    {
        public static readonly PgTimeSpan MaxValue = new PgTimeSpan(TimeSpan.MaxValue);
        public static readonly PgTimeSpan MinValue = new PgTimeSpan(TimeSpan.MinValue);
        public static readonly PgTimeSpan Null     = new PgTimeSpan(TimeSpan.Zero);

        private readonly TimeSpan _interval;

        public int Days
        {
            get { return _interval.Days; }
        }

        public int Hours
        {
            get { return _interval.Hours; }
        }

        public int Milliseconds
        {
            get { return _interval.Milliseconds; }
        }

        public int Minutes
        {
            get { return _interval.Minutes; }
        }

        public int Seconds
        {
            get { return _interval.Seconds; }
        }

        public TimeSpan Value
        {
            get { return _interval; }
        }

        public PgTimeSpan(TimeSpan interval)
        {
            _interval = interval;
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgTimeSpan))
            {
                return -1;
            }
            
            var pgt = (PgTimeSpan)obj;
            
            if (pgt == null)
            {
                return -1;
            }
            
            return _interval.CompareTo(pgt._interval);
        }

        public static bool GreatherThan(PgTimeSpan x, PgTimeSpan y)
        {
            return (x > y);
        }

        public static bool GreatherThanOrEqual(PgTimeSpan x, PgTimeSpan y)
        {
            return (x >= y);
        }

        public static bool LessThan(PgTimeSpan x, PgTimeSpan y)
        {
            return (x < y);
        }

        public static bool LessThanOrEqual(PgTimeSpan x, PgTimeSpan y)
        {
            return (x <= y);
        }

        public static bool NotEquals(PgTimeSpan x, PgTimeSpan y)
        {
            return (x != y);
        }

        public static PgTimeSpan Parse(string s)
        {
            return new PgTimeSpan(TimeSpan.Parse(s));
        }

        public static bool operator ==(PgTimeSpan left, PgTimeSpan right)
        {
            bool equals = false;

            if (left.Value == right.Value)
            {
                equals = true;
            }

            return equals;
        }

        public static bool operator !=(PgTimeSpan left, PgTimeSpan right)
        {
            bool notequals = false;

            if (left.Value != right.Value)
            {
                notequals = true;
            }

            return notequals;
        }

        public static bool operator >(PgTimeSpan left, PgTimeSpan right)
        {
            bool greater = false;

            if (left.Value > right.Value)
            {
                greater = true;
            }

            return greater;
        }

        public static bool operator >=(PgTimeSpan left, PgTimeSpan right)
        {
            bool greater = false;

            if (left.Value >= right.Value)
            {
                greater = true;
            }

            return greater;
        }

        public static bool operator <(PgTimeSpan left, PgTimeSpan right)
        {
            bool less = false;

            if (left.Value < right.Value)
            {
                less = true;
            }

            return less;
        }

        public static bool operator <=(PgTimeSpan left, PgTimeSpan right)
        {
            bool less = false;

            if (left.Value <= right.Value)
            {
                less = true;
            }

            return less;
        }

        public static explicit operator TimeSpan(PgTimeSpan x)
        {
            return x.Value;
        }

        public static explicit operator PgTimeSpan(string x)
        {
            return new PgTimeSpan(TimeSpan.Parse(x));
        }

        public override string ToString()
        {
            return _interval.ToString();
        }

        public override int GetHashCode()
        {
            return _interval.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PgTimeSpan))
            {
                return false;
            }
            if (obj == null)
            {
                return false;
            }

            PgTimeSpan value = (PgTimeSpan)obj;

            return ((PgTimeSpan)value) == this;
        }
    }
}