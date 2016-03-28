using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PostgreSql.Data.SqlClient.Sample
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
            
            var query = "SELECT *, orderid AS Order_ID, 'Hello World' FROM orders";
            
            using (PgConnection connection = new PgConnection(csb.ToString()))
            {
                connection.Open();
                
                using (PgCommand cmd = new PgCommand(query, connection))
                {
                    using (PgDataReader reader = cmd.ExecuteReader())
                    {
                        var schemas = reader.GetColumnSchema();
                        
                        foreach (var schema in schemas)
                        {
                            Console.WriteLine($"{schema.BaseSchemaName}.{schema.BaseTableName}.{schema.BaseColumnName} (IsExpression = {schema.IsExpression})");
                        }
                    }
                }
            }
        
            Console.WriteLine("Finished !!");
        } 
    }
}
