// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgNotificationEventArgs
        : EventArgs
    {
        private readonly int    _processID;
        private readonly string _condition;
        private readonly string _aditional;

        public int ProcessID
        {
            get { return _processID; }
        }

        public string Condition
        {
            get { return _condition; }
        }

        public string Aditional
        {
            get { return _aditional; }
        }

        internal PgNotificationEventArgs(int processID, string condition, string addtional)
        {
            _processID = processID;
            _condition = condition;
            _aditional = addtional;
        }
    }
}