using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PostgreSql.Data.PostgreSqlClient;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource               = "localhost";
            csb.InitialCatalog           = "northwind";
            csb.UserID                   = "northwind";
            csb.Password                 = "northwind@2";
            csb.PortNumber               = 5432;
            csb.Encrypt                  = false;
            csb.Pooling                  = false;
            csb.MultipleActiveResultSets = true;

            var com = new PgCommand("select * from Orders");
            var con = new PgConnection(csb.ToString());
            
            com.Connection = con;
            con.Open();

            Task<int> nonQueryTask = com.ExecuteNonQueryAsync();
            
            try
            {
                com.ExecuteNonQueryAsync().Wait();
            }
            catch (AggregateException agrEx)
            {
            }
            
//             Assert.True(failure, "FAILED: No exception thrown after trying second ExecuteNonQueryAsync.");
//             failure = false;

//             taskCompleted = nonQueryTask.Wait(TaskTimeout);
//             Assert.True(taskCompleted, "FAILED: ExecuteNonQueryAsync Task did not complete successfully.");

// #warning TODO: Needs implementation in the provider to return it as PgDataReader
//             Task<DbDataReader> readerTask = com.ExecuteReaderAsync();
//             try
//             {
//                 com.ExecuteReaderAsync().Wait(TaskTimeout);
//             }
//             catch (AggregateException agrEx)
//             {
//                 agrEx.Handle(
//                     (ex) =>
//                     {
//                         Assert.True(ex is InvalidOperationException, "FAILED: Thrown exception for ExecuteReaderAsync was not an InvalidOperationException: " + ex.Message);
//                         failure = true;
//                         return true;
//                     });
//             }
//             Assert.True(failure, "FAILED: No exception thrown after trying second ExecuteReaderAsync.");

//             taskCompleted = readerTask.Wait(TaskTimeout);
//             Assert.True(taskCompleted, "FAILED: ExecuteReaderAsync Task did not complete successfully.");

//            readerTask.Result.Dispose();
            con.Close();
        } 
    }
}
