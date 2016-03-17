// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;
using System.Data;

namespace PostgreSql.Data.Schema
{
    internal abstract class PgSchema
    {
        private readonly PgConnection _connection;

        internal PgConnection Connection
        {
            get { return _connection; }
        }

        internal PgSchema(PgConnection connection)
        {
            _connection = connection;
        }

        protected abstract string BuildSql(string[] restrictions);

        internal DataTable GetSchema(string collectionName, string[] restrictions)
        {
            DataTable dataTable = null;
            PgDataAdapter adapter = null;
            PgCommand command = null;

            try
            {
                command = _connection.CreateCommand();
                command.CommandText = BuildSql(ParseRestrictions(restrictions));

                if (_connection.InnerConnection.ActiveTransaction != null)
                {
                    command.Transaction = _connection.InnerConnection.ActiveTransaction;
                }

                adapter = new PgDataAdapter(command);
                dataTable = new DataTable(collectionName);

                adapter.Fill(dataTable);

                dataTable = ProcessResult(_connection, dataTable);
            }
            catch (PgException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PgException(ex.Message);
            }
            finally
            {
                if (command != null)
                {
                    command.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return dataTable;
        }

        protected virtual string[] ParseRestrictions(string[] restrictions)
        {
            return restrictions;
        }

        protected virtual DataTable ProcessResult(PgConnection connection, DataTable schema)
        {
            return schema;
        }
    }
}