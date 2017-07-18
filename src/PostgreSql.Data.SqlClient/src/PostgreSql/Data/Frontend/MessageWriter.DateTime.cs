// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        private void Write(DateTime value) => Write(value.Date.Subtract(PgDate.EpochDateTime).Days);
        private void Write(TimeSpan value) => Write((long)(value.Ticks * 0.1M));
        private void WriteTimeStamp(DateTime value)
        {
            Write((long)(value.Subtract(PgTimestamp.EpochDateTime).TotalMilliseconds * 1000));
        } 

        private void WriteTimeTZ(DateTimeOffset value)
        {
            EnsureCapacity(12);

            Write(value.TimeOfDay);
            Write((int)(value.Offset.TotalSeconds));
        }

        private void WriteTimestampTZ(DateTimeOffset value)
        {
            var dt        = TimeZoneInfo.ConvertTime(value, _sessionData.TimeZoneInfo);
            var timestamp = (long)(value.Subtract(PgTimestamp.EpochDateTime).TotalMilliseconds * 1000
                          + (int)dt.Offset.TotalSeconds);

            Write(timestamp);
        }

        private void Write(PgInterval value)
        {
            EnsureCapacity(8);

            Write((value - TimeSpan.FromDays(value.TotalDays)).TotalSeconds);
            Write(value.Days / 30);
        }
    }
}