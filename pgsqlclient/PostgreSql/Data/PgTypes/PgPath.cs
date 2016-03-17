// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPath
    {
        private readonly PgPoint[] _points;
        private readonly bool _isClosedPath;

        public PgPoint[] Points
        {
            get { return _points; }
        }

        public bool IsClosedPath
        {
            get { return _isClosedPath; }
        }

        public PgPath(bool isClosedPath, PgPoint[] points)
        {
            _isClosedPath = isClosedPath;
            _points = (PgPoint[])points.Clone();
        }

        public static bool operator ==(PgPath left, PgPath right)
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

        public static bool operator !=(PgPath left, PgPath right)
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

        public override string ToString()
        {
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
            return (_points.GetHashCode() ^ _isClosedPath.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PgPath))
            {
                return false;
            }
            if (obj == null)
            {
                return false;
            }

            PgPath value = (PgPath)obj;

            return ((PgPath)value) == this;
        }

        public static PgPath Parse(string s)
        {
            throw new NotSupportedException();
        }
    }
}