// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Xunit;

namespace PostgreSql.Data.SqlClient.Tests
{
    public sealed class DDAsyncTest
    {
        [Fact(Skip="disabled")]
        public void OpenConnection_WithAsyncTrue_ThrowsNotSupportedException()
        {
            var asyncConnectionString = DataTestClass.PostgreSql9_Pubs + "async=true";
            Assert.Throws<NotSupportedException>(() => { new PgConnection(asyncConnectionString); });
        }

        [Fact]
        public void ExecuteCommand_WithNewConnection_ShouldPerformAsyncByDefault()
        {
            var executedProcessList = new List<string>();

            var task1 = ExecuteCommandWithNewConnectionAsync("A", "SELECT * FROM Orders limit 10"  , executedProcessList);            
            var task2 = ExecuteCommandWithNewConnectionAsync("B", "SELECT * FROM Products limit 10", executedProcessList);
            
            //wait all before verifying the results
            Task.WaitAll(task1, task2);

            //verify whether it executed async
            Assert.True(DoesProcessExecutedAsync(executedProcessList));
        }

        [Fact]
        public void ExecuteCommand_WithSharedConnection_ShouldPerformAsyncByDefault()
        {
            var executedProcessList = new List<string>();

            //for shared connection we need to add MARS capabilities
            using (var conn = new PgConnection(DataTestClass.PostgreSql9_Northwind + "MultipleActiveResultSets=true;"))
            {
                conn.Open();
                
                var task1 = ExecuteCommandWithSharedConnectionAsync(conn, "C", "SELECT * FROM Orders limit 10"  , executedProcessList);
                var task2 = ExecuteCommandWithSharedConnectionAsync(conn, "D", "SELECT * FROM Products limit 10", executedProcessList);
                
                //wait all before verifying the results
                Task.WaitAll(task1, task2);
            }

            //verify whether it executed async
            Assert.True(DoesProcessExecutedAsync(executedProcessList));
        }

        private static bool DoesProcessExecutedAsync(IReadOnlyList<string> executedProcessList)
        {
            for (var i = 1; i < executedProcessList.Count; i++)
            {
                if (executedProcessList[i] != executedProcessList[i - 1])
                {
                    return true;
                }
            }
            
            return false;
        }

        private static async Task ExecuteCommandWithNewConnectionAsync(string processName, string cmdText, ICollection<string> executedProcessList)
        {
            var conn = new PgConnection(DataTestClass.PostgreSql9_Northwind);

            await conn.OpenAsync();
            
            var cmd = new PgCommand(cmdText, conn);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    executedProcessList.Add(processName);
                }
            }
        }

        private static async Task ExecuteCommandWithSharedConnectionAsync(PgConnection conn, string processName, string cmdText, ICollection<string> executedProcessList)
        {
            var cmd = new PgCommand(cmdText, conn);

            using (var reader = (PgDataReader)await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    executedProcessList.Add(processName);
                }
            }
        }
    }
}
