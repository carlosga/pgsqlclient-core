// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPolygon
        : INullable, IEquatable<PgPolygon>
    {
        public static readonly PgPolygon Null = new PgPolygon(false);

        public static PgPolygon Parse(string s)
        {
            throw new NotSupportedException();
        }

        private readonly bool      _isNotNull;
        private readonly PgPoint[] _points;

        public bool IsNull => !_isNotNull;

        public PgPoint[] Points
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _points;
            }
        }

        private PgPolygon(bool isNotNull)
        {
            _isNotNull = false;
            _points    = null;
        }

        public PgPolygon(PgPoint[] points)
        {
            _isNotNull = true;
            _points    = (PgPoint[])points.Clone();
        }

        public static bool operator ==(PgPolygon left, PgPolygon right)
        {
            bool equals = false;

            if (left.Points.Length == right.Points.Length)
            {
                equals = true;

                for (int i = 0; i < left.Points.Length; ++i)
                {
                    if (left.Points[i] != right.Points[i])
                    {
                        equals = false;
                        break;
                    }
                }
            }

            return equals;
        }

        public static bool operator !=(PgPolygon left, PgPolygon right)
        {
            bool notequals = true;

            if (left.Points.Length == right.Points.Length)
            {
                notequals = false;

                for (int i = 0; i < left.Points.Length; ++i)
                {
                    if (left.Points[i] != right.Points[i])
                    {
                        notequals = true;
                        break;
                    }
                }
            }

            return notequals;
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

            System.Text.StringBuilder b = new System.Text.StringBuilder();

            b.Append("(");

            for (int i = 0; i < _points.Length; i++)
            {
                if (b.Length > 1)
                {
                    b.Append(",");
                }

                b.Append(_points[i].ToString());
            }

            b.Append(")");

            return b.ToString();
        }

        public override int GetHashCode() 
        {
            if (IsNull)
            {
                return 0;
            }
            
            return (_points.GetHashCode());
        }

        public bool Equals(PgPolygon other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgPolygon))
            {
                return false;
            }
            return Equals((PgPolygon)obj);
        }
    }
}
