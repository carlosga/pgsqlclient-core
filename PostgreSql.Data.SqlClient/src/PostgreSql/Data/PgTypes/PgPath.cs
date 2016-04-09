// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPath
        : INullable, IEquatable<PgPath>
    {
        public static readonly PgPath Null = new PgPath(false);

        public static PgPath Parse(string s)
        {
            throw new NotSupportedException();
        }

        private readonly bool      _isNotNull;
        private readonly PgPoint[] _points;
        private readonly bool      _isClosedPath;

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

        public bool IsClosedPath
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _isClosedPath;
            }
        }

        private PgPath(bool isNotNull)
        {
            _isNotNull    = isNotNull;
            _points       = null;
            _isClosedPath = false;
        }

        public PgPath(PgPoint[] points, bool isClosedPath = false)
        {
            _isNotNull    = true;
            _points       = points;
            _isClosedPath = isClosedPath;
        }

        public static PgBoolean operator ==(PgPath x, PgPath y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }

            bool equals = false;

            if (x.Points.Length == y.Points.Length)
            {
                equals = true;

                for (int i = 0; i < x.Points.Length; ++i)
                {
                    if (x.Points[i] != y.Points[i])
                    {
                        equals = false;
                        break;
                    }
                }
            }

            return equals;
        }

        public static PgBoolean operator !=(PgPath x, PgPath y)
        {
            return !(x == y);
        }

        public PgString ToPgString()
        {
            return ToString();
        }

        public override string ToString()
        {
            if (IsNull)
            {
                return TypeInfoProvider.NullString;
            }

            System.Text.StringBuilder b = new System.Text.StringBuilder();

            b.Append(_isClosedPath ? "(" : "[");

            for (int i = 0; i < _points.Length; i++)
            {
                if (b.Length > 1)
                {
                    b.Append(",");
                }

                b.Append(_points[i].ToString());
            }

            b.Append(_isClosedPath ? ")" : "]");

            return b.ToString();
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return (_points.GetHashCode() ^ _isClosedPath.GetHashCode());
        }

        public bool Equals(PgPath other)
        {
            return (this == other).Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgPath))
            {
                return false;
            }
            return Equals((PgPath)obj);
        }
    }
}
