// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.PgTypes;
using System;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
    public class PgGeometricTypesTest
        : PgBaseTest
    {
        [Fact]
        public void PointTest()
        {
            using (var command = new PgCommand("select point_field from public.geometric_table where pk = @pk", Connection))
            {
                command.Parameters.Add("@pk", PgDbType.Int4).Value = 50;

                PgPoint point = (PgPoint)command.ExecuteScalar();

                Console.WriteLine("Point value: {0}", point.ToString());

                Assert.AreEqual(50, point.X, "Invalid X coord in point");
                Assert.AreEqual(60, point.Y, "Invalid Y coord in point");
            }
        }

        [Fact]
        public void BoxTest()
        {
            using (var command = new PgCommand("SELECT box_field FROM public.geometric_table WHERE pk = @pk", Connection))
            {
                command.Parameters.Add("@pk", PgDbType.Int4).Value = 70;

                PgBox box = (PgBox)command.ExecuteScalar();

                Console.WriteLine("Box value: {0}", box.ToString());

                Assert.AreEqual(0, box.LowerLeft.X, "Invalid X coord in Lower Left corner");
                Assert.AreEqual(70, box.LowerLeft.Y, "Invalid Y coord in Lower Left corner");

                Assert.AreEqual(70, box.UpperRight.X, "Invalid X coord in Upper Right corner");
                Assert.AreEqual(70, box.UpperRight.Y, "Invalid Y coord in Upper Right corner");
            }
        }

        [Fact]
        public void CircleTest()
        {
            using (var command = new PgCommand("select circle_field from public.geometric_table where pk = @pk", Connection))
            {
                command.Parameters.Add("@pk", PgDbType.Int4).Value = 30;

                PgCircle circle = (PgCircle)command.ExecuteScalar();

                Console.WriteLine("Circle value: {0}", circle.ToString());

                Assert.AreEqual(30, circle.Center.X, "Invalid X coord in circle");
                Assert.AreEqual(0, circle.Center.Y, "Invalid Y coord in circle");
                Assert.AreEqual(30, circle.Radius, "Invalid RADIUS coord in circle");
            }
        }

        [Fact]
        public void LineSegmentTest()
        {
            using (var command = new PgCommand("select lseg_field from public.geometric_table where pk = @pk", Connection))
            {
                command.Parameters.Add("@pk", PgDbType.Int4).Value = 20;

                PgLSeg lseg = (PgLSeg)command.ExecuteScalar();

                Console.WriteLine("LSeg value: {0}", lseg.ToString());

                Assert.AreEqual(-1, lseg.StartPoint.X, "Invalid X coord in start point");
                Assert.AreEqual(0, lseg.StartPoint.Y, "Invalid Y coord in start point");

                Assert.AreEqual(1, lseg.EndPoint.X, "Invalid X coord in end point");
                Assert.AreEqual(0, lseg.EndPoint.Y, "Invalid Y coord in end point");
            }
        }

        [Fact]
        public void PathTest()
        {
            using (var command = new PgCommand("select path_field from public.geometric_table where pk = @pk", Connection))
            {
                command.Parameters.Add("@pk", PgDbType.Int4).Value = 10;

                PgPath path = (PgPath)command.ExecuteScalar();

                Console.WriteLine("Path value: {0}", path.ToString());

                Assert.AreEqual(0, path.Points[0].X, "Invalid X coord in path point 0");
                Assert.AreEqual(0, path.Points[0].Y, "Invalid Y coord in path point 0");

                Assert.AreEqual(1, path.Points[1].X, "Invalid X coord in path point 1");
                Assert.AreEqual(0, path.Points[1].Y, "Invalid Y coord in path point 1");
            }
        }

        [Fact]
        public void PolygonTest()
        {
            using (var command = new PgCommand("select polygon_field from public.geometric_table where pk = @pk", Connection))
            {
                command.Parameters.Add("@pk", PgDbType.Int4).Value = 10;

                PgPolygon polygon = (PgPolygon)command.ExecuteScalar();

                Console.WriteLine("Polygon value: {0}", polygon.ToString());

                Assert.AreEqual(1, polygon.Points[0].X, "Invalid X coord in polygon point 0");
                Assert.AreEqual(1, polygon.Points[0].Y, "Invalid Y coord in polygon point 0");

                Assert.AreEqual(0, polygon.Points[1].X, "Invalid X coord in polygon point 1");
                Assert.AreEqual(0, polygon.Points[1].Y, "Invalid Y coord in polygon point 1");
            }
        }

        public void BoxArrayTest()
        {
        }

        public void PointArrayTest()
        {
        }

        public void LineSegmentArrayTest()
        {
        }

        public void PathArrayTest()
        {
        }

        public void PolygonArrayTest()
        {
        }

        public void CircleArrayTest()
        {
        }
    }
}