// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Data.ProviderBase;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgReferenceCollection 
        : DbReferenceCollection
    {
        internal const int DataReaderTag = 1;
        internal const int CommandTag    = 2;
        internal const int BulkCopyTag   = 3;

        override public void Add(object value, int tag)
        {
            Debug.Assert(DataReaderTag == tag || CommandTag == tag || BulkCopyTag == tag, "unexpected tag?");
            Debug.Assert(DataReaderTag != tag || value is PgDataReader, "tag doesn't match object type: SqlDataReader");
            Debug.Assert(CommandTag    != tag || value is PgCommand   , "tag doesn't match object type: SqlCommand");
            // Debug.Assert(BulkCopyTag   != tag || value is PgBulkCopy  , "tag doesn't match object type: SqlBulkCopy");

            base.AddItem(value, tag);
        }

        internal void Deactivate()
        {
            base.Notify(0);
        }

        internal PgDataReader FindLiveReader(PgCommand command)
        {
            if (command == null)
            {
                // if null == command, will find first live datareader
                return FindItem<PgDataReader>(DataReaderTag, (dataReader) => (!dataReader.IsClosed));
            }
            else
            {
                // else will find live datareader associated with the command
                return FindItem<PgDataReader>(DataReaderTag, (dataReader) => ((!dataReader.IsClosed) && (command == dataReader.Command)));
            }
        }

        // Finds a PgCommand associated with the given StateObject
        // internal PgCommand FindLiveCommand(TdsParserStateObject stateObj)
        // {
        //     return FindItem<PgCommand>(CommandTag, (command) => (command.StateObject == stateObj));
        // }

        protected override void NotifyItem(int message, int tag, object value)
        {
            Debug.Assert(0 == message, "unexpected message?");
            Debug.Assert(DataReaderTag == tag || CommandTag == tag || BulkCopyTag == tag, "unexpected tag?");

            if (tag == DataReaderTag)
            {
                var rdr = value as PgDataReader;
                Debug.Assert(rdr != null, "Incorrect object type");
                if (!rdr.IsClosed)
                {
                    rdr.CloseReaderFromConnection();
                }
            }
            else if (tag == CommandTag)
            {
                var cmd = value as PgCommand;
                Debug.Assert(cmd != null, "Incorrect object type");
                cmd.CloseCommandFromConnection();
            }
            // else if (tag == BulkCopyTag)
            // {
            //     Debug.Assert(value is SqlBulkCopy, "Incorrect object type");
            //     ((SqlBulkCopy)value).OnConnectionClosed();
            // }
        }

        override public void Remove(object value)
        {
            Debug.Assert(value is PgDataReader || value is PgCommand /*|| value is SqlBulkCopy*/, "PgReferenceCollection.Remove expected a PgDataReader or PgCommand"); // or SqlBulkCopy

            base.RemoveItem(value);
        }
    }
}
