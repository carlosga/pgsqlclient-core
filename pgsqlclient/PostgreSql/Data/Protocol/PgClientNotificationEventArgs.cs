// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgClientNotificationEventArgs
        : EventArgs
    {
        private readonly int    _processId;
        private readonly string _condition;
        private readonly string _aditional;

        internal int ProcessId
        {
            get { return _processId; }
        }

        internal string Condition
        {
            get { return _condition; }
        }

        internal string Aditional
        {
            get { return _aditional; }
        }

        internal PgClientNotificationEventArgs(int processId, string condition, string addtional)
        {
            _processId = processId;
            _condition = condition;
            _aditional = addtional;
        }
    }
}