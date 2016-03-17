// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBox
    {
        public static PgBox Parse(string s)
        {
            throw new NotSupportedException();
        }

        private readonly PgPoint _upperRight;
        private readonly PgPoint _lowerLeft;

        public PgPoint UpperRight
        {
            get { return _upperRight; }
        }

        public PgPoint LowerLeft
        {
            get { return _lowerLeft; }
        }

        public PgBox(PgPoint lowerLeft, PgPoint upperRight)
        {
            _lowerLeft = lowerLeft;
            _upperRight = upperRight;
        }

        public PgBox(double x1, double y1, double x2, double y2)
        {
            _lowerLeft = new PgPoint(x1, y1);
            _upperRight = new PgPoint(x2, y2);
        }

        public static bool operator ==(PgBox left, PgBox right)
        {
            return (left.UpperRight == right.UpperRight && left.LowerLeft == right.LowerLeft);
        }

        public static bool operator !=(PgBox left, PgBox right)
        {
            return (left.UpperRight != right.UpperRight || left.LowerLeft != right.LowerLeft);
        }

        public override string ToString()
        {
            return String.Format("(({0},{1}),({2},{3}))"
                               , _lowerLeft.X
                               , _lowerLeft.Y
                               , _upperRight.X
                               , _upperRight.Y);
        }

        public override int GetHashCode()
        {
            return (UpperRight.GetHashCode() ^ LowerLeft.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PgBox))
            {
                return false;
            }
            if (obj == null)
            {
                return false;
            }

            PgBox value = (PgBox)obj;

            return ((PgBox)value) == this;
        }
    }
}