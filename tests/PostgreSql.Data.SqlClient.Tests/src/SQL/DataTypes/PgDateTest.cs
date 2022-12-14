// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.PgTypes;
using System;
using System.Collections.Generic;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class PgDateTest
    {
        [Fact]
        public static void DefaultConstructor()
        {
            PgDate date = new PgDate();

            Assert.Equal(PgDate.MinValue, date);
        }

        [Fact]
        public static void MinDateTest()
        {
            PgDate date = PgDate.MinValue;

            Assert.Equal(-4713           , date.Year);
            Assert.Equal(11              , date.Month);
            Assert.Equal(24              , date.Day);
            Assert.Equal(Era.BeforeCommon, date.Era);
            Assert.Equal(DayOfWeek.Monday, date.DayOfWeek);
            Assert.Equal(328             , date.DayOfYear);
            Assert.Equal(-2451545        , date.DaysSinceEpoch);
        }

        [Fact]
        public static void MaxDateTest()
        {
            PgDate date = PgDate.MaxValue;

            Assert.Equal(5874897             , date.Year);
            Assert.Equal(12                  , date.Month);
            Assert.Equal(31                  , date.Day);
            Assert.Equal(Era.Common          , date.Era);
            Assert.Equal(DayOfWeek.Tuesday   , date.DayOfWeek);
            Assert.Equal(365                 , date.DayOfYear);
            Assert.Equal(2147483493 - 2451545, date.DaysSinceEpoch);
        }

        [Fact]
        public static void EpochTest()
        {
            PgDate date = PgDate.Epoch;

            Assert.Equal(2000              , date.Year);
            Assert.Equal(1                 , date.Month);
            Assert.Equal(1                 , date.Day);
            Assert.Equal(Era.Common        , date.Era);
            Assert.Equal(DayOfWeek.Saturday, date.DayOfWeek);
            Assert.Equal(1                 , date.DayOfYear);
            Assert.Equal(0                 , date.DaysSinceEpoch);
        }

        [Fact]
        public static void TodayTest()
        {
            PgDate date = PgDate.Today;

            Assert.Equal(DateTime.Today.Date.Year     , date.Year);
            Assert.Equal(DateTime.Today.Date.Month    , date.Month);
            Assert.Equal(DateTime.Today.Date.Day      , date.Day);
            Assert.Equal(Era.Common                   , date.Era);
            Assert.Equal(DateTime.Today.Date.DayOfWeek, date.DayOfWeek);
            Assert.Equal(DateTime.Today.Date.DayOfYear, date.DayOfYear);
        }

        [Theory]
        [MemberData(nameof(DayOfWeek_TestData))]
        public static void DayOfWeekTest(PgDate date, DayOfWeek expected)
        {
            Assert.Equal(expected, date.DayOfWeek);
        }

        [Theory]
        [MemberData(nameof(DayOfYear_TestData))]
        public static void DayOfYear(PgDate date, int expected)
        {
            Assert.Equal(expected, date.DayOfYear);
        }

        [Theory]
        [InlineData(2000, true)]
        [InlineData(2400, true)]
        [InlineData(1800, false)]
        [InlineData(1900, false)]
        [InlineData(2100, false)]
        [InlineData(2200, false)]
        [InlineData(2300, false)]
        [InlineData(2500, false)]
        public static void IsLeapYear(int year, bool expected)
        {
            Assert.Equal(expected, PgDate.IsLeapYear(year));
        }

        /// Ported from corefx sources 
        [Theory]
        [MemberData(nameof(AddDays_TestData))]
        public static void AddDays(PgDate date, int days, PgDate expected)
        {
            Assert.Equal(expected, date.AddDays(days));
        }

        /// Ported from corefx sources 
        [Theory]
        [MemberData(nameof(AddMonths_TestData))]
        public static void AddMonths(PgDate date, int months, PgDate expected)
        {
            Assert.Equal(expected, date.AddMonths(months));
        }

        /// Ported from corefx sources
        [Theory]
        [MemberData(nameof(AddYears_TestData))]
        public static void AddYears(PgDate date, int years, PgDate expected)
        {
            Assert.Equal(expected, date.AddYears(years));
        }

        public static IEnumerable<object[]> DayOfWeek_TestData()
        {
            yield return new object[] { PgDate.MinValue         , DayOfWeek.Monday  };
            yield return new object[] { PgDate.MaxValue         , DayOfWeek.Tuesday };
            yield return new object[] { new PgDate(2000, 02, 29), DayOfWeek.Tuesday };
            yield return new object[] { new PgDate(1986, 02, 28), DayOfWeek.Friday  };
            yield return new object[] { new PgDate(2017, 06, 30), DayOfWeek.Friday  };
        }

        public static IEnumerable<object[]> DayOfYear_TestData()
        {
            yield return new object[] { PgDate.MinValue         , 328 };
            yield return new object[] { PgDate.MaxValue         , 365 };
            yield return new object[] { new PgDate(2000, 02, 29),  60 };
            yield return new object[] { new PgDate(1986, 02, 28),  59 };
            yield return new object[] { new PgDate(2017, 06, 30), 181 };
        }

        /// Ported from corefx sources
        public static IEnumerable<object[]> AddDays_TestData()
        {
            yield return new object[] { new PgDate(1986, 8, 15),  2, new PgDate(1986, 8, 17) };
            yield return new object[] { new PgDate(1986, 8, 15),  0, new PgDate(1986, 8, 15) };
            yield return new object[] { new PgDate(1986, 8, 15), -2, new PgDate(1986, 8, 13) };
            yield return new object[] { new PgDate(2015, 2, 28),  1, new PgDate(2015, 3, 01) };
            yield return new object[] { new PgDate(2015, 3, 01), -1, new PgDate(2015, 2, 28) };
            yield return new object[] { new PgDate(2016, 2, 28),  1, new PgDate(2016, 2, 29) };
            yield return new object[] { new PgDate(2016, 2, 28),  2, new PgDate(2016, 3, 01) };
            yield return new object[] { new PgDate(2016, 3, 01), -2, new PgDate(2016, 2, 28) };
        }

        /// Ported from corefx sources
        public static IEnumerable<object[]> AddMonths_TestData()
        {
            yield return new object[] { new PgDate(1986, 8, 15),  2, new PgDate(1986, 10, 15) };
            yield return new object[] { new PgDate(1986, 8, 15),  0, new PgDate(1986,  8, 15) };
            yield return new object[] { new PgDate(1986, 8, 15), -2, new PgDate(1986,  6, 15) };
        }

        /// Ported from corefx sources
        public static IEnumerable<object[]> AddYears_TestData()
        {
            yield return new object[] { new PgDate(1986, 8, 15),  10, new PgDate(1996, 8, 15) };
            yield return new object[] { new PgDate(1986, 8, 15),   0, new PgDate(1986, 8, 15) };
            yield return new object[] { new PgDate(1986, 8, 15), -10, new PgDate(1976, 8, 15) };
        }
    }
}
