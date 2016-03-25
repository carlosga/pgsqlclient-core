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
            
            Action<object, PgInfoMessageEventArgs> warningCallback =
                (object sender, PgInfoMessageEventArgs imevent) =>
                {
                    for (int i = 0; i < imevent.Errors.Count; i++)
                    {
                        Console.WriteLine(imevent.Errors[i].Message);
                    }
                };
                
            var warningInfoMessage = "DANGER !!!";                

            PgInfoMessageEventHandler handler = new PgInfoMessageEventHandler(warningCallback);
            using (PgConnection connection  = new PgConnection(csb.ToString()))
            {
                connection.InfoMessage += handler;
                connection.Open();

                PgCommand cmd = new PgCommand(string.Format("SELECT RAISE_NOTICE('{0}')", warningInfoMessage), connection);
                cmd.ExecuteNonQuery();

                connection.InfoMessage -= handler;
                cmd.ExecuteNonQuery();
            }

            Thread.Sleep(10000);
            
            Console.WriteLine("Finished !");
        } 
    }
}
