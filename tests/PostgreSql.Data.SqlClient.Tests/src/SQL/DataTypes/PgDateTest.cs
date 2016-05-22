// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.PgTypes;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class PgDateTest
    {
        [Fact]
        public void DefaultConstructor()
        {
            PgDate date = new PgDate();

            Assert.Equal(date, PgDate.MinValue);
        }

        [Fact]
        public void MinDateTest()
        {
            PgDate date = PgDate.MinValue;

            Assert.Equal(date.Year          , -4713);
            Assert.Equal(date.Month         , 11);
            Assert.Equal(date.Day           , 24);
            Assert.Equal(date.Era           , Era.BeforeCommon);
            Assert.Equal(date.DayOfWeek     , System.DayOfWeek.Monday);
            Assert.Equal(date.DayOfYear     , 328);
            Assert.Equal(date.DaysSinceEpoch, -2451545);
        }

        [Fact]
        public void MaxDateTest()
        {
            PgDate date = PgDate.MaxValue;

            Assert.Equal(date.Year          , 5874897);
            Assert.Equal(date.Month         , 12);
            Assert.Equal(date.Day           , 31);
            Assert.Equal(date.Era           , Era.Common);
            Assert.Equal(date.DayOfWeek     , System.DayOfWeek.Tuesday);
            Assert.Equal(date.DayOfYear     , 365);
            Assert.Equal(date.DaysSinceEpoch, 2147483493 - 2451545);
        }

        [Fact]
        public void EpochTest()
        {
            PgDate date = PgDate.Epoch;

            Assert.Equal(date.Year          , 2000);
            Assert.Equal(date.Month         , 1);
            Assert.Equal(date.Day           , 1);
            Assert.Equal(date.Era           , Era.Common);
            Assert.Equal(date.DayOfWeek     , System.DayOfWeek.Saturday);
            Assert.Equal(date.DayOfYear     , 1);
            Assert.Equal(date.DaysSinceEpoch, 0);
        }

        [Fact]
        public void TodayTest()
        {
            PgDate date = PgDate.Today;

            Assert.Equal(date.Year          , DateTime.Today.Date.Year);
            Assert.Equal(date.Month         , DateTime.Today.Date.Month);
            Assert.Equal(date.Day           , DateTime.Today.Date.Day);
            Assert.Equal(date.Era           , Era.Common);
            Assert.Equal(date.DayOfWeek     , DateTime.Today.Date.DayOfWeek);
            Assert.Equal(date.DayOfYear     , DateTime.Today.Date.DayOfYear);
        }

        [Fact]
        public void IsLeapYear()
        {
            Assert.True(PgDate.IsLeapYear(2000));
            Assert.True(PgDate.IsLeapYear(2400));
            Assert.False(PgDate.IsLeapYear(1800));
            Assert.False(PgDate.IsLeapYear(1900));
            Assert.False(PgDate.IsLeapYear(2100));
            Assert.False(PgDate.IsLeapYear(2200));
            Assert.False(PgDate.IsLeapYear(2300));
            Assert.False(PgDate.IsLeapYear(2500));
        }
    }
}
