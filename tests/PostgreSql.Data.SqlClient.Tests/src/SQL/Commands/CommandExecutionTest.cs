
using Xunit;
using System.Threading;
using System.Data;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class CommandExecutionTest
    {       
        [Fact]
        public static void Ensure_Commands_Can_Be_Executed_Across_Transactions()
        {
            string sql = "select count(*)::int from orders";

            using (var connection = new PgConnection(DataTestClass.PostgreSql_Northwind))
            {
                connection.Open();

                PgCommand command = null;
                bool      failure = false;

                try 
                {
                    command = new PgCommand(sql, connection);

                    using (var transaction = connection.BeginTransaction())
                    {
                        command.Transaction = transaction;

                        using (var reader = command.ExecuteReader())
                        {
                        }
                    }
                    var count = (int)command.ExecuteScalar();
                    using (var transaction = connection.BeginTransaction())
                    {
                        command.Transaction = transaction;

                        using (var reader = command.ExecuteReader())
                        {
                        }
                    }             
                } 
                catch
                {
                    failure = true;
                }
                finally 
                {
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }

                Assert.False(failure, "Execution of command across transaction has failed");
            }
        }

        /// <summary>
        /// #12. CommandText parsing should be delayed until the command is being prepared
        /// </summary>
        [Fact]
        public void Execute_Script_Without_Mars()
        {
            string script = @"
DO $$DECLARE r record;
DECLARE s TEXT;
BEGIN
    FOR r IN SELECT 
                table_schema, 
                table_name 
             FROM 
                information_schema.views 
             WHERE 
                table_schema IN (
                    SELECT SCHEMA_NAME FROM (
                        SELECT
                            pg_namespace.nspname AS SCHEMA_NAME
                            , CASE
                                WHEN pg_namespace.nspname LIKE 'pg_temp_%' THEN 1
                                WHEN ( pg_namespace.nspname LIKE 'pg_%' OR nspname = 'information_schema' ) THEN 0
                                ELSE 3
                            END AS SCHEMA_TYPE
                        FROM pg_namespace
                        LEFT JOIN pg_shadow ON pg_namespace.nspowner = pg_shadow.usesysid
                        LEFT JOIN pg_description ON pg_namespace.oid = pg_description.objoid
                    ) AS USER_SCHEMAS WHERE USER_SCHEMAS.SCHEMA_TYPE = 3 
             )
    LOOP
        s := 'DROP VIEW ' ||  quote_ident(r.table_schema) || '.' || quote_ident(r.table_name) || ';';

        RAISE NOTICE 's = % ',s;
    END LOOP;
END $$;";

            string connectionString = (new PgConnectionStringBuilder(DataTestClass.PostgreSql_Northwind) { MultipleActiveResultSets = false }).ConnectionString;
            bool   failure          = false;

            using (var connection = new PgConnection(connectionString))
            {            
                connection.Open();

                using (var command = new PgCommand(script, connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch
                    {
                        failure = true;
                    }

                    Assert.False(failure, "FAILED: Exception thrown on script execution with MARS disabled.");
                }
            }
        }
    }
}
