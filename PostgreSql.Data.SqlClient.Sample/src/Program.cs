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

            short[] array = new short[10];
            
            for (int i = 0; i < 10; i++)
            {
                array[i] = (short)i;
            } 

            using (var connection = new PgConnection(csb.ToString()))
            {
                connection.Open();
                
                using (var command = new PgCommand("SELECT carray_6 FROM temp__8d3608a5160e8d0_87cecc", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) {}  
                    }
                }
            }
            
            Console.WriteLine("Finished !!");
        } 
    }
}
