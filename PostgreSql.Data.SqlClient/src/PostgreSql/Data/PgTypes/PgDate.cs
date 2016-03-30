// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PgTypes
{
    public struct PgDate
        : IComparable, INullable
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

        public PgDate(int dayTicks)
        {
            throw new NotImplementedException();
        }

        public PgDate(DateTime value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgDate(int year, int month, int day)
        {
            _value     = new DateTime(year, month, day);
            _isNotNull = true;
        }

        public static PgDate operator -(PgDate x, TimeSpan t)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator !=(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgDate operator +(PgDate x, TimeSpan t)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <=(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator ==(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >=(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static explicit operator DateTime(PgDate x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgDate(PgString x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDate(DateTime value)
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
            throw new NotImplementedException();
        }

        public int CompareTo(object value)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(PgDate value)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object value)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean Equals(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThan(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThanOrEqual(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThan(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThanOrEqual(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean NotEquals(PgDate x, PgDate y)
        {
            throw new NotImplementedException();
        }

        public static PgDate Parse(string s)
        {
            throw new NotImplementedException();
        }

        public static PgDate Subtract(PgDate x, TimeSpan t)
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
