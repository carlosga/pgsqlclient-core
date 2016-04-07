// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NUnit.Framework;
using PostgreSql.Data.PgTypes;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    [TestFixture]
    public class PgDateTest
    {
        [Test]
        public void DefaultConstructor()
        {
            PgDate date = new PgDate();

            Assert.AreEqual(date, PgDate.MinValue);
        }

        [Test]
        public void MinDateTest()
        {
            PgDate date = PgDate.MinValue;

            Assert.AreEqual(date.Year          , -4713);
            Assert.AreEqual(date.Month         , 11);
            Assert.AreEqual(date.Day           , 24);
            Assert.AreEqual(date.Era           , Era.BeforeCommon);
            Assert.AreEqual(date.DayOfWeek     , System.DayOfWeek.Monday);
            Assert.AreEqual(date.DayOfYear     , 328);
            Assert.AreEqual(date.DaysSinceEpoch, -2451545);
        }

        [Test]
        public void MaxDateTest()
        {
            PgDate date = PgDate.MaxValue;

            Assert.AreEqual(date.Year          , 5874897);
            Assert.AreEqual(date.Month         , 12);
            Assert.AreEqual(date.Day           , 31);
            Assert.AreEqual(date.Era           , Era.Common);
            Assert.AreEqual(date.DayOfWeek     , System.DayOfWeek.Tuesday);
            Assert.AreEqual(date.DayOfYear     , 365);
            Assert.AreEqual(date.DaysSinceEpoch, 2147483493 - 2451545);
        }

        [Test]
        public void EpochTest()
        {
            PgDate date = PgDate.Epoch;

            Assert.AreEqual(date.Year          , 2000);
            Assert.AreEqual(date.Month         , 1);
            Assert.AreEqual(date.Day           , 1);
            Assert.AreEqual(date.Era           , Era.Common);
            Assert.AreEqual(date.DayOfWeek     , System.DayOfWeek.Saturday);
            Assert.AreEqual(date.DayOfYear     , 1);
            Assert.AreEqual(date.DaysSinceEpoch, 0);
        }

        [Test]
        public void TodayTest()
        {
            PgDate date = PgDate.Today;

            Assert.AreEqual(date.Year          , DateTime.Today.Date.Year);
            Assert.AreEqual(date.Month         , DateTime.Today.Date.Month);
            Assert.AreEqual(date.Day           , DateTime.Today.Date.Day);
            Assert.AreEqual(date.Era           , Era.Common);
            Assert.AreEqual(date.DayOfWeek     , DateTime.Today.Date.DayOfWeek);
            Assert.AreEqual(date.DayOfYear     , DateTime.Today.Date.DayOfYear);
        }

        [Test]
        public void IsLeapYear()
        {
            Assert.IsTrue(PgDate.IsLeapYear(2000));
            Assert.IsTrue(PgDate.IsLeapYear(2400));
            Assert.IsFalse(PgDate.IsLeapYear(1800));
            Assert.IsFalse(PgDate.IsLeapYear(1900));
            Assert.IsFalse(PgDate.IsLeapYear(2100));
            Assert.IsFalse(PgDate.IsLeapYear(2200));
            Assert.IsFalse(PgDate.IsLeapYear(2300));
            Assert.IsFalse(PgDate.IsLeapYear(2500));
        }
    }
}
