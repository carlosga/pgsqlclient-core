// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using PostgreSql.Data.PostgreSqlClient.Tests.SystemDataInternals;

namespace PostgreSql.Data.PostgreSqlClient.Tests
{
    public static class SystemDataExtensions
    {
        public static void CompletePendingReadWithSuccess(this PgDataReader reader, bool resetForcePendingReadsToWait)
        {
            DataReaderHelper.CompletePendingReadWithSuccess(reader, resetForcePendingReadsToWait);
        }

        public static void CompletePendingReadWithFailure(this PgDataReader reader, int errorCode, bool resetForcePendingReadsToWait)
        {
            DataReaderHelper.CompletePendingReadWithFailure(reader, errorCode, resetForcePendingReadsToWait);
        }

        public static void CompletePendingReadWithSuccess(this PgCommand command, bool resetForcePendingReadsToWait)
        {
            CommandHelper.CompletePendingReadWithSuccess(command, resetForcePendingReadsToWait);
        }

        public static void CompletePendingReadWithFailure(this PgCommand command, int errorCode, bool resetForcePendingReadsToWait)
        {
            CommandHelper.CompletePendingReadWithFailure(command, errorCode, resetForcePendingReadsToWait);
        }

        public static void SetDefaultTimeout(this PgDataReader reader, long milliseconds)
        {
            DataReaderHelper.SetDefaultTimeout(reader, milliseconds);
        }

        public static T GetSchemaEntry<T>(this PgDataReader reader, int row, string schemaEntry)
        {
            return DataReaderHelper.GetSchemaEntry<T>(reader, row, schemaEntry);
        }

        public static object[] GetMetaEntries(this PgDataReader reader)
        {
            return DataReaderHelper.GetMetaEntries(reader);
        }

        public static bool IsLong(this PgDataReader reader, int row)
        {
            return DataReaderHelper.IsLong(reader, row);
        }
    }
}
