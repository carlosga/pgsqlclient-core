// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBinary
        : INullable, IComparable<PgBinary>, IComparable, IEquatable<PgBinary>
    {
        public static readonly PgBinary Null = new PgBinary(false);

        private readonly bool   _isNotNull;
        private readonly byte[] _value;

        private PgBinary(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _value     = null;
        }

        public PgBinary(byte[] value)
        {
            _isNotNull = (value != null); 
            _value     = value;
        }

        public static PgBoolean operator !=(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBinary operator +(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <=(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator ==(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >=(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static explicit operator byte[](PgBinary x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgBinary(byte[] x)
        {
            throw new NotImplementedException();
        }

        public bool IsNull => !_isNotNull;
        
        public int Length 
        { 
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _value.Length;
            } 
        }
        
        public byte[] Value 
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

        public byte this[int index] 
        { 
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                if (index < 0 || index > _value.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                return _value[index];
            } 
        }

        public static PgBinary Add(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object value)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(PgBinary value)
        {
            throw new NotImplementedException();
        }

        public static PgBinary Concat(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public bool Equals(PgBinary obj)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean Equals(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThan(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThanOrEqual(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThan(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThanOrEqual(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean NotEquals(PgBinary x, PgBinary y)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
