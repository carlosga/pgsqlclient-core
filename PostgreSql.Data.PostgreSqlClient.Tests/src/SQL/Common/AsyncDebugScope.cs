// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using PostgreSql.Data.PostgreSqlClient.Tests.SystemDataInternals;

namespace PostgreSql.Data.PostgreSqlClient.Tests
{
    public class PendAsyncReadsScope 
        : IDisposable
    {
        private PgCommand    _command   = null;
        private PgDataReader _reader    = null;
        private int?         _errorCode = null;

        public PendAsyncReadsScope(PgCommand command, int? errorCode = null)
        {
            _command   = command;
            _errorCode = errorCode;
            TdsParserStateObjectHelper.ForcePendingReadsToWaitForUser = true;
        }

        public PendAsyncReadsScope(PgDataReader reader, int? errorCode = null)
        {
            _reader = reader;
            _errorCode = errorCode;
            TdsParserStateObjectHelper.ForcePendingReadsToWaitForUser = true;
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                if (_errorCode.HasValue)
                {
                    _reader.CompletePendingReadWithFailure(_errorCode.Value, true);
                }
                else
                {
                    _reader.CompletePendingReadWithSuccess(true);
                }
            }

            if (_command != null)
            {
                if (_errorCode.HasValue)
                {
                    _command.CompletePendingReadWithFailure(_errorCode.Value, true);
                }
                else
                {
                    _command.CompletePendingReadWithSuccess(true);
                }
            }
        }
    }
}
