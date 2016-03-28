using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args, new ColorConsoleWriter(), Console.In);
        }
    }
}
