// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgLSeg
        : INullable, IEquatable<PgLSeg>
    {
        public static readonly PgLSeg Null = new PgLSeg(false);

        public static PgLSeg Parse(string s)
        {
            throw new NotSupportedException();
        }

        private readonly bool    _isNotNull;
        private readonly PgPoint _startPoint;
        private readonly PgPoint _endPoint;

        public bool IsNull => !_isNotNull;

        public PgPoint StartPoint
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _startPoint;
            }
        }

        public PgPoint EndPoint
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _endPoint;
            }
        }

        private PgLSeg(bool isNotNull)
        {
            _isNotNull  = isNotNull;
            _startPoint = PgPoint.Null;
            _endPoint   = PgPoint.Null;
        }

        public PgLSeg(PgPoint startPoint, PgPoint endPoint)
        {
            _isNotNull  = true;
            _startPoint = startPoint;
            _endPoint   = endPoint;
        }

        public PgLSeg(double x1, double y1, double x2, double y2)
        {
            _isNotNull  = true;
            _startPoint = new PgPoint(x1, y1);
            _endPoint   = new PgPoint(x2, y2);
        }

        public static bool operator ==(PgLSeg left, PgLSeg right)
        {
            return (left.StartPoint == right.StartPoint && left.EndPoint == right.EndPoint);
        }

        public static bool operator !=(PgLSeg left, PgLSeg right)
        {
            return (left.StartPoint != right.StartPoint || left.EndPoint != right.EndPoint);
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
            return String.Format(PgTypeInfoProvider.InvariantCulture
                               , "[({0},{1}),({2},{3})]"
                               , _startPoint.X
                               , _startPoint.Y
                               , _endPoint.X
                               , _endPoint.Y);
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return (_startPoint.GetHashCode() ^ _endPoint.GetHashCode());
        }

        public bool Equals(PgLSeg other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgLSeg))
            {
                return false;
            }
            return Equals((PgLSeg)obj);
        }
    }
}
