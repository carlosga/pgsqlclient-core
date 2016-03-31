// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed class PgClientNotificationEventArgs
        : EventArgs
    {
        private readonly int    _processId;
        private readonly string _condition;
        private readonly string _aditional;

        internal int    ProcessId => _processId;
        internal string Condition => _condition;
        internal string Aditional => _aditional;

        internal PgClientNotificationEventArgs(int processId, string condition, string addtional)
        {
            _processId = processId;
            _condition = condition;
            _aditional = addtional;
        }
    }
}
