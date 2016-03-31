// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgTimestamp
        : IComparable, INullable
    {
        public static readonly PgTimestamp MaxValue = DateTime.MaxValue;
        public static readonly PgTimestamp MinValue = DateTime.MinValue;
        public static readonly PgTimestamp Null     = new PgTimestamp();

        internal const long MicrosecondsPerDay    = 86400000000L;
        internal const long MicrosecondsPerHour   = 3600000000L;
        internal const long MicrosecondsPerMinute = 60000000L;
        internal const long MicrosecondsPerSecond = 1000000L;
        internal const long SecondsPerDay	      = 86400L;

        // Julian-date equivalents of Day 0 in Unix and Postgres
        internal static readonly long MicrosecondsBetweenEpoch = ((PgDate.PostgresEpochDays - PgDate.UnixEpochDays) * MicrosecondsPerDay);

        private readonly bool     _isNotNull;
        private readonly DateTime _value;

        public PgTimestamp(DateTime value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgTimestamp(int dayTicks, int timeTicks)
        {
            throw new NotImplementedException();
        }

        public PgTimestamp(int year, int month, int day)
        {
            _value      = new DateTime(year, month, day);
             _isNotNull = true;
        }

        public PgTimestamp(int year, int month, int day, int hour, int minute, int second)
        {
            _value     = new DateTime(year, month, day, hour, minute, second);
            _isNotNull = true;
        }

        public PgTimestamp(int year, int month, int day, int hour, int minute, int second, double millisecond)
        {
            _value    = new DateTime(year, month, day, hour, minute, second);
           _isNotNull = true;
        }

        public PgTimestamp(int year, int month, int day, int hour, int minute, int second, int bilisecond)
        {
            throw new NotImplementedException();
        }

        public PgTimestamp(long microseconds)
        {
            throw new NotImplementedException();
        }

        public static PgTimestamp operator -(PgTimestamp x, TimeSpan t)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator !=(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgTimestamp operator +(PgTimestamp x, TimeSpan t)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <=(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator ==(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >=(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static explicit operator DateTime(PgTimestamp x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgTimestamp(PgString x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgTimestamp(DateTime value)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public int CompareTo(object value)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(PgTimestamp value)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object value)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean Equals(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThan(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThanOrEqual(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThan(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThanOrEqual(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean NotEquals(PgTimestamp x, PgTimestamp y)
        {
            throw new NotImplementedException();
        }

        public static PgTimestamp Parse(string s)
        {
            throw new NotImplementedException();
        }

        public static PgTimestamp Subtract(PgTimestamp x, TimeSpan t)
        {
            throw new NotImplementedException();
        }

        public PgString ToPgString()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
