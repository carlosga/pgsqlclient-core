// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    // public class AsyncDebugScope : IDisposable
    // {
    //     public bool ForceAllPends
    //     {
    //         get { return TdsParserStateObjectHelper.ForceAllPends; }
    //         set { TdsParserStateObjectHelper.ForceAllPends = value; }
    //     }

    //     public bool SkipSendAttention
    //     {
    //         get { return TdsParserStateObjectHelper.SkipSendAttention; }
    //         set { TdsParserStateObjectHelper.SkipSendAttention = value; }
    //     }

    //     public bool ForceSyncOverAsyncAfterFirstPend
    //     {
    //         get { return TdsParserStateObjectHelper.ForceSyncOverAsyncAfterFirstPend; }
    //         set { TdsParserStateObjectHelper.ForceSyncOverAsyncAfterFirstPend = value; }
    //     }

    //     public bool ForcePendingReadsToWaitForUser
    //     {
    //         get { return TdsParserStateObjectHelper.ForcePendingReadsToWaitForUser; }
    //         set { TdsParserStateObjectHelper.ForcePendingReadsToWaitForUser = value; }
    //     }

    //     public bool FailAsyncPends
    //     {
    //         get { return TdsParserStateObjectHelper.FailAsyncPends; }
    //         set { TdsParserStateObjectHelper.FailAsyncPends = value; }
    //     }

    //     public int ForceAsyncWriteDelay
    //     {
    //         get { return CommandHelper.ForceAsyncWriteDelay; }
    //         set { CommandHelper.ForceAsyncWriteDelay = value; }
    //     }

    //     public void Dispose()
    //     {
    //         TdsParserStateObjectHelper.FailAsyncPends = false;
    //         TdsParserStateObjectHelper.ForceAllPends = false;
    //         TdsParserStateObjectHelper.ForcePendingReadsToWaitForUser = false;
    //         TdsParserStateObjectHelper.ForceSyncOverAsyncAfterFirstPend = false;
    //         TdsParserStateObjectHelper.SkipSendAttention = false;
    //         CommandHelper.ForceAsyncWriteDelay = 0;
    //     }
    // }

    public class PendAsyncReadsScope : IDisposable
    {
        private PgCommand    _command   = null;
        private PgDataReader _reader    = null;
        private int?         _errorCode = null;

        public PendAsyncReadsScope(PgCommand command, int? errorCode = null)
        {
            _command   = command;
            _errorCode = errorCode;
        }

        public PendAsyncReadsScope(PgDataReader reader, int? errorCode = null)
        {
            _reader    = reader;
            _errorCode = errorCode;
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
