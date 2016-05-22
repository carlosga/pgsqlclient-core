// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgNotificationEventArgs
        : EventArgs
    {
        private readonly int    _processId;
        private readonly string _condition;
        private readonly string _aditional;

        public int    ProcessId => _processId;
        public string Condition => _condition;
        public string Aditional => _aditional;

        internal PgNotificationEventArgs(int processId, string condition, string addtional)
        {
            _processId = processId;
            _condition = condition;
            _aditional = addtional;
        }
    }
}
