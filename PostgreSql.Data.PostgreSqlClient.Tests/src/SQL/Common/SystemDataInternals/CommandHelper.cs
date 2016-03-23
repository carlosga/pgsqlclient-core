// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;

namespace PostgreSql.Data.PostgreSqlClient.Tests.SystemDataInternals
{
    internal static class CommandHelper
    {
        private static Type         s_command                        = typeof(PgCommand);
        private static MethodInfo   s_completePendingReadWithSuccess = s_command.GetMethod("CompletePendingReadWithSuccess", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo   s_completePendingReadWithFailure = s_command.GetMethod("CompletePendingReadWithFailure", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo s_debugForceAsyncWriteDelay      = s_command.GetProperty("DebugForceAsyncWriteDelay"   , BindingFlags.NonPublic | BindingFlags.Static);

        internal static void CompletePendingReadWithSuccess(PgCommand command, bool resetForcePendingReadsToWait)
        {
            s_completePendingReadWithSuccess.Invoke(command, new object[] { resetForcePendingReadsToWait });
        }

        internal static void CompletePendingReadWithFailure(PgCommand command, int errorCode, bool resetForcePendingReadsToWait)
        {
            s_completePendingReadWithFailure.Invoke(command, new object[] { errorCode, resetForcePendingReadsToWait });
        }

        internal static int ForceAsyncWriteDelay
        {
            get { return (int)s_debugForceAsyncWriteDelay.GetValue(null); }
            set { s_debugForceAsyncWriteDelay.SetValue(null, value); }
        }
    }
}
