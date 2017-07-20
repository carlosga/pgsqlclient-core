// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        private DateTime   ReadDate()      => PgDate.EpochDateTime.AddDays(ReadInt32());
        private TimeSpan   ReadTime()      => new TimeSpan(ReadInt64() * 10);
        private DateTime   ReadTimestamp() => PgTimestamp.EpochDateTime.AddMilliseconds(ReadInt64() * 0.001);
        private PgInterval ReadInterval()  => PgInterval.FromInterval(ReadInt64(), ReadInt64());

        private DateTimeOffset ReadTimeWithTZ()
        {
            return new DateTimeOffset(ReadInt64() * 10, TimeSpan.FromSeconds(ReadInt32()));
        }

        private DateTimeOffset ReadTimestampWithTZ()
        {
            var dt = PgTimestamp.EpochDateTimeOffsetUtc.AddMilliseconds(ReadInt64() * 0.001);
            return ((_sessionData.TimeZoneInfo == TimeZoneInfo.Utc) ? dt : TimeZoneInfo.ConvertTime(dt, _sessionData.TimeZoneInfo));
        }
    }
}