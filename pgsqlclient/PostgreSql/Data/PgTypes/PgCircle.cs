// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgCircle
    {
        private readonly PgPoint _center;
        private readonly double _radius;

        public PgPoint Center
        {
            get { return _center; }
        }

        public double Radius
        {
            get { return _radius; }
        }

        public PgCircle(PgPoint center, double radius)
        {
            _center = center;
            _radius = radius;
        }

        public PgCircle(double x, double y, double radius)
        {
            _center = new PgPoint(x, y);
            _radius = radius;
        }

        public static bool operator ==(PgCircle left, PgCircle right)
        {
            return (left.Center == right.Center && left.Radius == right.Radius);
        }

        public static bool operator !=(PgCircle left, PgCircle right)
        {
            return (left.Center != right.Center || left.Radius != right.Radius);
        }

        public override string ToString()
        {
            return String.Format("<{0},{1}>", _center, _radius);
        }

        public override int GetHashCode()
        {
            return _center.GetHashCode() ^ _radius.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PgCircle))
            {
                return false;
            }
            if (obj == null)
            {
                return false;
            }

            PgCircle value = (PgCircle)obj;

            return ((PgCircle)value) == this;
        }

        public static PgCircle Parse(string s)
        {
            throw new NotSupportedException();
        }
    }
}