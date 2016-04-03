using System;
using PostgreSql.Data.PgTypes;

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
            csb.Password                 = "northwind";
            csb.PortNumber               = 5432;
            csb.Encrypt                  = false;
            csb.Pooling                  = false;
            csb.MultipleActiveResultSets = true;
             
             using (var connection = new PgConnection(csb.ToString()))
             {
                 connection.Open();
                 using (var command = new PgCommand("SELECT * FROM temp_table", connection))
                 {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) { }
                    }  
                 }
             }
             
             Console.WriteLine("Finished !!");
        } 
    }
}
