// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System;
using Xunit;
using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.SqlClient.Tests
{
    public abstract class DataTestClass
    {
        protected static StringBuilder globalBuilder = new StringBuilder();
        public static readonly string BinariesDropPath = (Environment.GetEnvironmentVariable("BVT_BinariesDropPath") ?? Environment.GetEnvironmentVariable("_NTPOSTBLD")) ?? Environment.GetEnvironmentVariable("DD_BuiltTarget");
        
        private static Dictionary<string, string> s_xmlConnectionStringMap = null;
        private static readonly object s_connStringMapLock = new object();

        protected abstract void RunDataTest();

        protected bool RunTest()
        {
            try
            {
                RunDataTest();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return true;
        }

        public static string GetConnectionString(string connectionStringName)
        {
            lock (s_connStringMapLock)
            {
                return GetConnectionStringFromXml(connectionStringName);
            }
        }

        private static string GetConnectionStringFromXml(string key)
        {
            string connectionString = null;
            if (s_xmlConnectionStringMap == null)
            {
                PopulateConnectionStrings();
            }

            bool foundConnString = s_xmlConnectionStringMap.TryGetValue(key, out connectionString);
            if (!foundConnString || string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Could not find a valid connection string for the key: " + key);
            }

            string password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

            return (new PgConnectionStringBuilder(connectionString) { Password = password }).ConnectionString;
        }

        private static void PopulateConnectionStrings()
        {
            s_xmlConnectionStringMap = new Dictionary<string, string>();

            using (TextReader connectionStringReader = File.OpenText(@"ConnectionString.xml"))
            {
                var document  = new XmlDocument();
                var xmlString = connectionStringReader.ToString();
                var settings  = new XmlReaderSettings
                {
                    IgnoreWhitespace = true
                  , DtdProcessing    = DtdProcessing.Ignore
                };

                using (StringReader reader = new StringReader(connectionStringReader.ReadToEnd()))
                {
                    using (XmlReader xReader = XmlReader.Create(reader, settings))
                    {
                        document.Load(xReader);
                        XmlNodeList nodeList = document.GetElementsByTagName("ConnectionString");
                        foreach (XmlNode node in nodeList)
                        {
                            var connectionStringKey   = node.Attributes["id"].Value;
                            var connectionStringXml   = node.FirstChild as XmlText;
                            var connectionStringValue = connectionStringXml.Data;

                            s_xmlConnectionStringMap[connectionStringKey] = connectionStringValue;
                        }
                    }
                }
            }
        }

        public static string PostgreSql_Northwind => GetConnectionString("PostgreSql9_Northwind");
        public static string PostgreSql_Pubs      => GetConnectionString("PostgreSql9_Pubs");

        // the name length will be no more then (16 + prefix.Length + escapeLeft.Length + escapeRight.Length)
        // some providers does not support names (Oracle supports up to 30)
        public static string GetUniqueName(string prefix, string escapeLeft, string escapeRight)
        {
            return string.Format(
                "{0}{1}_{2}_{3}{4}"
              , escapeLeft
              , prefix
              , DateTime.Now.Ticks.ToString("X", CultureInfo.InvariantCulture)  // up to 8 characters
              , Guid.NewGuid().ToString().Substring(0, 6)                       // take the first 6 characters only
              , escapeRight);
        }

        public static string GetUniqueNameForPostgreSql(string prefix)
        {
            string hostName       = System.Net.Dns.GetHostName();
            string extendedPrefix = string.Format(
                "{1}_{2}"
              , prefix
              , hostName
              , DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture));
            string name = GetUniqueName(extendedPrefix, "\"", "\"");
            
            if (name.Length > 128)
            {
                throw new ArgumentOutOfRangeException("the name is too long - SQL Server names are limited to 128");
            }
            
            return name;
        }

        // creates temporary table name for PostgreSql Server
        public static string UniqueTempTableName => GetUniqueNameForPostgreSql("#T");

        public static void PrintException(Type expected, Exception e, params string[] values)
        {
            try
            {
                Debug.Assert(null != e, "PrintException: null exception");

                globalBuilder.Length = 0;
                globalBuilder.Append(e.GetType().Name).Append(": ");

                if (e is COMException)
                {
                    globalBuilder.Append("0x").Append((((COMException)e).HResult).ToString("X8"));
                    if (expected != e.GetType())
                    {
                        globalBuilder.Append(": ").Append(e.ToString());
                    }
                }
                else
                {
                    globalBuilder.Append(e.Message);
                }
                AssemblyFilter(globalBuilder);
                Console.WriteLine(globalBuilder.ToString());

                if (expected != e.GetType())
                {
                    Console.WriteLine(e.StackTrace);
                }
                if (values != null)
                {
                    foreach (string value in values)
                    {
                        Console.WriteLine(value);
                    }
                }
                if (e.InnerException != null)
                {
                    PrintException(e.InnerException.GetType(), e.InnerException);
                }
                Console.Out.Flush();
            }
            catch (Exception f)
            {
                Console.WriteLine(f);
            }
        }

        public static void DumpParameters(DbCommand cmd)
        {
            DumpParameters((PgCommand)cmd);
        }

        public static void DumpParameters(PgCommand cmd)
        {
            Debug.Assert(cmd != null, "DumpParameters: null PgCommand");

            foreach (PgParameter p in cmd.Parameters)
            {
                byte precision = p.Precision;
                byte scale = p.Scale;
                Console.WriteLine("\t\"" + p.ParameterName + "\" AS " + p.DbType.ToString("G") + " OF " + p.PgDbType.ToString("G") + " FOR " + p.SourceColumn + "\"");
                Console.WriteLine("\t\t" + p.Size.ToString() + ", " + precision.ToString() + ", " + scale.ToString() + ", " + p.Direction.ToString("G") + ", " + DBConvertToString(p.Value));
            }
        }

        public static void WriteEntry(string entry)
        {
            Console.WriteLine(entry);
        }

        private static StringBuilder s_outputBuilder;
        private static string[]      s_outputFilter;

        public static StreamWriter NewWriter()
        {
            return new StreamWriter(new MemoryStream(), System.Text.Encoding.UTF8);
        }

        public static string AssemblyFilter(StreamWriter writer)
        {
            if (s_outputBuilder == null)
            {
                s_outputBuilder = new StringBuilder();
            }
            s_outputBuilder.Length = 0;

            byte[] utf8 = ((MemoryStream)writer.BaseStream).ToArray();
            string value = System.Text.Encoding.UTF8.GetString(utf8, 3, utf8.Length - 3); // skip 0xEF, 0xBB, 0xBF
            s_outputBuilder.Append(value);
            AssemblyFilter(s_outputBuilder);
            return s_outputBuilder.ToString();
        }

        public static void AssemblyFilter(StringBuilder builder)
        {
            string[] filter = s_outputFilter;
            if (filter == null)
            {
                filter = new string[5];
                string tmp = typeof(System.Guid).AssemblyQualifiedName;
                filter[0] = tmp.Substring(tmp.IndexOf(','));
                filter[1] = filter[0].Replace("mscorlib", "System");
                filter[2] = filter[0].Replace("mscorlib", "System.Data");
                filter[3] = filter[0].Replace("mscorlib", "System.Data.OracleClient");
                filter[4] = filter[0].Replace("mscorlib", "System.Xml");
                s_outputFilter = filter;
            }

            for (int i = 0; i < filter.Length; ++i)
            {
                builder.Replace(filter[i], string.Empty);
            }
        }

        public static string ToInvariatString(object value)
        {
            return (
                (value is DateTime) ? ((DateTime)value).ToString("MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo) :
                (value is decimal)  ? ((decimal)value).ToString(NumberFormatInfo.InvariantInfo) :
                (value is double)   ? ((double)value).ToString(NumberFormatInfo.InvariantInfo) :
                (value is float)    ? ((float)value).ToString(NumberFormatInfo.InvariantInfo) :
                (value is short)    ? ((short)value).ToString(NumberFormatInfo.InvariantInfo) :
                (value is int)      ? ((int)value).ToString(NumberFormatInfo.InvariantInfo) :
                (value is long)     ? ((Int64)value).ToString(NumberFormatInfo.InvariantInfo) :
                /*default: */ value.ToString()
            );
        }

        public static string DBConvertToString(object value)
        {
            StringWriter stringWriter = new StringWriter();
            WriteObject(stringWriter, value, CultureInfo.InvariantCulture, null, 0, int.MaxValue);
            return stringWriter.ToString();
        }

        public static void DumpValue(object value)
        {
            DumpValue(Console.Out, value, int.MaxValue, CultureInfo.InvariantCulture);
        }
        public static void DumpValue(object value, int recursionLimit)
        {
            DumpValue(Console.Out, value, recursionLimit, CultureInfo.InvariantCulture);
        }

        public static void DumpValue(TextWriter textWriter, object value, CultureInfo cultureInfo)
        {
            DumpValue(textWriter, value, int.MaxValue, cultureInfo);
        }

        public static void DumpValue(TextWriter textWriter, object value, int recursionLimit, CultureInfo cultureInfo)
        {
            if (value is DbDataReader)
            {
                WriteDbDataReader(textWriter, value as DbDataReader, cultureInfo, string.Empty, recursionLimit);
            }
            else
            {
                WriteObject(textWriter, value, recursionLimit, cultureInfo);
            }
        }

        private static void WriteDbDataReader(TextWriter textWriter, DbDataReader reader, CultureInfo cultureInfo, string prefix, int recursionLimit)
        {
            if (null == reader)      { throw new ArgumentNullException(nameof(reader)); }
            if (null == textWriter)  { throw new ArgumentNullException(nameof(textWriter)); }
            if (null == cultureInfo) { throw new ArgumentNullException(nameof(cultureInfo)); }

            if (0 > --recursionLimit)
            {
                return;
            }
            if (reader.IsClosed)
            {
                return;
            }

            int resultCount = 0;
            int lastRecordsAffected = 0;
            object value = null;
            do
            {
                try
                {
                    textWriter.WriteLine(prefix + "ResultSetIndex=" + resultCount);

                    int fieldCount = reader.FieldCount;
                    if (0 < fieldCount)
                    {
                        for (int i = 0; i < fieldCount; ++i)
                        {
                            textWriter.WriteLine(prefix + "Field[" + i + "] = " + reader.GetName(i) + "(" + reader.GetDataTypeName(i) + ")");
                        }
                        int rowCount = 0;
                        while (reader.Read())
                        {
                            textWriter.WriteLine(prefix + "RowIndex=" + rowCount);

                            for (int index = 0; index < fieldCount; ++index)
                            {
                                try
                                {
                                    value = reader.GetValue(index);
                                    if (value is DbDataReader)
                                    {
                                        DbDataReader hierarchialResult = (DbDataReader)value;
                                        textWriter.WriteLine(prefix + "Value[" + index + "] is " + value.GetType().Name + " Depth=" + hierarchialResult.Depth);
                                        WriteDbDataReader(textWriter, hierarchialResult, cultureInfo, prefix + "\t", recursionLimit);
                                        hierarchialResult.Dispose();
                                        value = null;
                                    }
                                    else
                                    {
                                        textWriter.Write(prefix + "Value[" + index + "] = ");
                                        WriteObject(textWriter, value, cultureInfo, null, 0, recursionLimit);
                                        textWriter.Write(Environment.NewLine);
                                    }
                                }
                                catch (Exception e)
                                {
                                    PrintException(null, e);
                                    if (value is IDisposable)
                                    {
                                        ((IDisposable)value).Dispose();
                                    }
                                    value = null;
                                }
                            }
                            ++rowCount;
                        }
                        int cumlativeRecordsAffected = reader.RecordsAffected;
                        textWriter.WriteLine(prefix + "RecordsAffected=" + (cumlativeRecordsAffected - lastRecordsAffected));
                        lastRecordsAffected = Math.Min(cumlativeRecordsAffected, 0);
                    }
                }
                catch (Exception e)
                {
                    PrintException(null, e);
                }
                resultCount++;
            } while (reader.NextResult());

            reader.Dispose();
        }

        public static void WriteObject(TextWriter textWriter, object value, CultureInfo cultureInfo)
        {
            WriteObject(textWriter, value, int.MaxValue, cultureInfo);
        }

        public static void WriteObject(TextWriter textWriter, object value, int recursionLimit, CultureInfo cultureInfo)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException("textWriter");
            }
            if (cultureInfo == null)
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }
            WriteObject(textWriter, value, cultureInfo, new Dictionary<string, string>(), 1, recursionLimit);
        }

        private static void WriteObject(TextWriter                 textWriter
                                      , object                     value
                                      , CultureInfo                cultureInfo
                                      , Dictionary<string, string> used
                                      , int                        indent
                                      , int                        recursionLimit)
        {
            if (0 > --recursionLimit)
            {
                return;
            }
            if (value == null)
            {
                textWriter.Write("DEFAULT");
            }
            else if (DBNull.Value.Equals(value))
            {
                textWriter.Write("ISNULL");
            }
            else
            {
                Type valuetype = value.GetType();

                if (value is string)
                {
                    textWriter.Write(valuetype.Name);
                    textWriter.Write(":");
                    textWriter.Write(((string)value).Length);
                    textWriter.Write("<");
                    textWriter.Write((string)value);
                    textWriter.Write(">");
                }
                else if (value is DateTime)
                {
                    textWriter.Write(valuetype.Name);
                    textWriter.Write("<");
                    textWriter.Write(((DateTime)value).ToString("s", cultureInfo));
                    textWriter.Write(">");
                }
                else if (value is DateTimeOffset)
                {
                    textWriter.Write(valuetype.Name);
                    textWriter.Write("<");
                    textWriter.Write(((DateTimeOffset)value).ToString("s", cultureInfo));
                    textWriter.Write(">");
                }
                else if (value is Single)
                {
                    textWriter.Write(valuetype.Name);
                    textWriter.Write("<");
                    textWriter.Write(((float)value).ToString(cultureInfo));
                    textWriter.Write(">");
                }
                else if (value is Double)
                {
                    textWriter.Write(valuetype.Name);
                    textWriter.Write("<");
                    textWriter.Write(((double)value).ToString(cultureInfo));
                    textWriter.Write(">");
                }
                else if (value is decimal)
                {
                    textWriter.Write(valuetype.Name);
                    textWriter.Write("<");
                    textWriter.Write(((decimal)value).ToString(cultureInfo));
                    textWriter.Write(">");
                }
                else if (value is INullable && ((INullable)value).IsNull)
                {
                    textWriter.Write(valuetype.Name);
                    textWriter.Write(" ISNULL");
                }
                else if (valuetype.IsArray)
                {
                    textWriter.Write(valuetype.Name);
                    Array array = (Array)value;

                    if (1 < array.Rank)
                    {
                        textWriter.Write("{");
                    }

                    for (int i = 0; i < array.Rank; ++i)
                    {
                        int count = array.GetUpperBound(i);

                        textWriter.Write(' ');
                        textWriter.Write(count - array.GetLowerBound(i) + 1);
                        textWriter.Write("{ ");
                        for (int k = array.GetLowerBound(i); k <= count; ++k)
                        {
                            AppendNewLineIndent(textWriter, indent + 1);
                            textWriter.Write(',');
                            WriteObject(textWriter, array.GetValue(k), cultureInfo, used, 0, recursionLimit);
                            textWriter.Write(' ');
                        }
                        AppendNewLineIndent(textWriter, indent);
                        textWriter.Write("}");
                    }
                    if (1 < array.Rank)
                    {
                        textWriter.Write('}');
                    }
                }
                else if (value is ICollection)
                {
                    textWriter.Write(valuetype.Name);
                    var collection = (ICollection)value;
                    var newvalue   = new object[collection.Count];
                    collection.CopyTo(newvalue, 0);

                    textWriter.Write(' ');
                    textWriter.Write(newvalue.Length);
                    textWriter.Write('{');
                    for (int k = 0; k < newvalue.Length; ++k)
                    {
                        AppendNewLineIndent(textWriter, indent + 1);
                        textWriter.Write(',');
                        WriteObject(textWriter, newvalue[k], cultureInfo, used, indent + 1, recursionLimit);
                    }
                    AppendNewLineIndent(textWriter, indent);
                    textWriter.Write('}');
                }
                else if (value is Type)
                {
                    textWriter.Write(valuetype.Name);
                    textWriter.Write('<');
                    textWriter.Write((value as Type).FullName);
                    textWriter.Write('>');
                }
                else
                {
                    string fullName = valuetype.FullName;
                    if ("System.ComponentModel.ExtendedPropertyDescriptor" == fullName)
                    {
                        textWriter.Write(fullName);
                    }
                    else
                    {
                        var fields     = valuetype.GetFields(BindingFlags.Instance | BindingFlags.Public);
                        var properties = valuetype.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        var hasinfo    = false;
                        
                        if ((fields != null) && (0 < fields.Length))
                        {
                            textWriter.Write(fullName);
                            fullName = null;

                            Array.Sort(fields, FieldInfoCompare.Default);
                            for (int i = 0; i < fields.Length; ++i)
                            {
                                FieldInfo field = fields[i];

                                AppendNewLineIndent(textWriter, indent + 1);
                                textWriter.Write(field.Name);
                                textWriter.Write('=');
                                object newvalue = field.GetValue(value);
                                WriteObject(textWriter, newvalue, cultureInfo, used, indent + 1, recursionLimit);
                            }
                            hasinfo = true;
                        }
                        if ((properties != null) && (properties.Length > 0))
                        {
                            if (fullName != null)
                            {
                                textWriter.Write(fullName);
                                fullName = null;
                            }

                            Array.Sort(properties, PropertyInfoCompare.Default);
                            for (int i = 0; i < properties.Length; ++i)
                            {
                                PropertyInfo property = properties[i];
                                if (property.CanRead)
                                {
                                    ParameterInfo[] parameters = property.GetIndexParameters();
                                    if ((parameters == null) || (parameters.Length == 0))
                                    {
                                        AppendNewLineIndent(textWriter, indent + 1);
                                        textWriter.Write(property.Name);
                                        textWriter.Write('=');
                                        object newvalue = null;
                                        bool haveValue = false;
                                        try
                                        {
                                            newvalue = property.GetValue(value);
                                            haveValue = true;
                                        }
                                        catch (TargetInvocationException e)
                                        {
                                            textWriter.Write(e.InnerException.GetType().Name);
                                            textWriter.Write(": ");
                                            textWriter.Write(e.InnerException.Message);
                                        }
                                        if (haveValue)
                                        {
                                            WriteObject(textWriter, newvalue, cultureInfo, used, indent + 1, recursionLimit);
                                        }
                                    }
                                }
                            }
                            hasinfo = true;
                        }
                        if (!hasinfo)
                        {
                            textWriter.Write(valuetype.Name);
                            textWriter.Write('<');
                            MethodInfo method = valuetype.GetMethod("ToString", new Type[] { typeof(IFormatProvider) });
                            if (null != method)
                            {
                                textWriter.Write((string)method.Invoke(value, new object[] { cultureInfo }));
                            }
                            else
                            {
                                string text = value.ToString();
                                textWriter.Write(text);
                            }
                            textWriter.Write('>');
                        }
                    }
                }
            }
        }

        private static char[] s_appendNewLineIndentBuffer = new char[0];
        private static void AppendNewLineIndent(TextWriter textWriter, int indent)
        {
            textWriter.Write(Environment.NewLine);
            char[] buf = s_appendNewLineIndentBuffer;
            if (buf.Length < indent * 4)
            {
                buf = new char[indent * 4];
                for (int i = 0; i < buf.Length; ++i)
                {
                    buf[i] = ' ';
                }
                s_appendNewLineIndentBuffer = buf;
            }
            textWriter.Write(buf, 0, indent * 4);
        }

        private sealed class FieldInfoCompare : IComparer<FieldInfo>
        {
            internal static FieldInfoCompare Default = new FieldInfoCompare();

            private FieldInfoCompare()
            {
            }

            public int Compare(FieldInfo x, FieldInfo y)
            {
                string fieldInfoName1 = x.Name;
                string fieldInfoName2 = y.Name;

                return CultureInfo.InvariantCulture.CompareInfo.Compare(fieldInfoName1, fieldInfoName2, CompareOptions.IgnoreCase);
            }
        }

        private sealed class PropertyInfoCompare : IComparer<PropertyInfo>
        {
            internal static PropertyInfoCompare Default = new PropertyInfoCompare();

            private PropertyInfoCompare()
            {
            }

            public int Compare(PropertyInfo x, PropertyInfo y)
            {
                string propertyInfoName1 = x.Name;
                string propertyInfoName2 = y.Name;

                return CultureInfo.InvariantCulture.CompareInfo.Compare(propertyInfoName1, propertyInfoName2, CompareOptions.IgnoreCase);
            }
        }

        private static bool CheckException<TException>(Exception ex, string exceptionMessage, bool innerExceptionMustBeNull) 
            where TException : Exception
        {
            return ((ex != null) 
                 && (ex is TException) 
                 && ((string.IsNullOrEmpty(exceptionMessage)) || (ex.Message.Contains(exceptionMessage))) 
                 && ((!innerExceptionMustBeNull) || (ex.InnerException == null)));
        }

        public static void AssertEqualsWithDescription(object expectedValue, object actualValue, string failMessage)
        {
            var msg = $"{failMessage}{Environment.NewLine}Expected: {expectedValue}{Environment.NewLine}Actual: {actualValue}";
            if (expectedValue == null || actualValue == null)
            {
                Assert.True(expectedValue == actualValue, msg);
            }
            else
            {
                Assert.True(expectedValue.Equals(actualValue), msg);
            }
        }

        public static TException AssertThrowsWrapper<TException>(
            Action                 actionThatFails
          , string                 exceptionMessage         = null
          , bool                   innerExceptionMustBeNull = false
          , Func<TException, bool> customExceptionVerifier  = null) where TException : Exception
        {
            TException ex = Assert.Throws<TException>(actionThatFails);
            if (exceptionMessage != null)
            {
                Assert.True(ex.Message.Contains(exceptionMessage),
                    string.Format("FAILED: Exception did not contain expected message.\nExpected: {0}\nActual: {1}", exceptionMessage, ex.Message));
            }

            if (innerExceptionMustBeNull)
            {
                Assert.True(ex.InnerException == null, "FAILED: Expected InnerException to be null.");
            }

            if (customExceptionVerifier != null)
            {
                Assert.True(customExceptionVerifier(ex), "FAILED: Custom exception verifier returned false for this exception.");
            }

            return ex;
        }

        public static TException AssertThrowsWrapper<TException, TInnerException>(
            Action                 actionThatFails
          , string                 exceptionMessage         = null
          , string                 innerExceptionMessage    = null
          , bool                   innerExceptionMustBeNull = false
          , Func<TException, bool> customExceptionVerifier  = null) where TException : Exception
        {
            TException ex = AssertThrowsWrapper<TException>(actionThatFails, exceptionMessage, innerExceptionMustBeNull, customExceptionVerifier);

            if (innerExceptionMessage != null)
            {
                Assert.True(ex.InnerException != null, "FAILED: Cannot check innerExceptionMessage because InnerException is null.");
                Assert.True(ex.InnerException.Message.Contains(innerExceptionMessage),
                    string.Format("FAILED: Inner Exception did not contain expected message.\nExpected: {0}\nActual: {1}", innerExceptionMessage, ex.InnerException.Message));
            }

            return ex;
        }

        public static TException AssertThrowsWrapper<TException, TInnerException, TInnerInnerException>(
            Action actionThatFails
          , string exceptionMessage                   = null
          , string innerExceptionMessage              = null
          , string innerInnerExceptionMessage         = null
          , bool   innerInnerInnerExceptionMustBeNull = false) where TException      : Exception 
                                                               where TInnerException : Exception 
                                                               where TInnerInnerException : Exception
        {
            TException ex = AssertThrowsWrapper<TException, TInnerException>(actionThatFails, exceptionMessage, innerExceptionMessage);
            if (innerInnerInnerExceptionMustBeNull)
            {
                Assert.True(ex.InnerException != null, "FAILED: Cannot check innerInnerInnerExceptionMustBeNull since InnerException is null");
                Assert.True(ex.InnerException.InnerException == null, "FAILED: Expected InnerInnerException to be null.");
            }

            if (innerInnerExceptionMessage != null)
            {
                Assert.True(ex.InnerException != null, "FAILED: Cannot check innerInnerExceptionMessage since InnerException is null");
                Assert.True(ex.InnerException.InnerException != null, "FAILED: Cannot check innerInnerExceptionMessage since InnerInnerException is null");
                Assert.True(ex.InnerException.InnerException.Message.Contains(innerInnerExceptionMessage),
                    string.Format("FAILED: Inner Exception did not contain expected message.\nExpected: {0}\nActual: {1}", innerInnerExceptionMessage, ex.InnerException.InnerException.Message));
            }

            return ex;
        }

        public static TException ExpectFailure<TException>(
            Action                 actionThatFails
          , string                 exceptionMessage         = null
          , bool                   innerExceptionMustBeNull = false
          , Func<TException, bool> customExceptionVerifier  = null) where TException : Exception
        {
            try
            {
                actionThatFails();
                Console.WriteLine("ERROR: Did not get expected exception");
                return null;
            }
            catch (Exception ex)
            {
                if ((CheckException<TException>(ex, exceptionMessage, innerExceptionMustBeNull)) && ((customExceptionVerifier == null) || (customExceptionVerifier(ex as TException))))
                {
                    return (ex as TException);
                }
                else
                {
                    throw;
                }
            }
        }

        public static TException ExpectFailure<TException, TInnerException>(
            Action actionThatFails
          , string exceptionMessage              = null
          , string innerExceptionMessage         = null
          , bool   innerInnerExceptionMustBeNull = false) where TException      : Exception 
                                                          where TInnerException : Exception
        {
            try
            {
                actionThatFails();
                Console.WriteLine("ERROR: Did not get expected exception");
                return null;
            }
            catch (Exception ex)
            {
                if ((CheckException<TException>(ex, exceptionMessage, false)) && (CheckException<TInnerException>(ex.InnerException, innerExceptionMessage, innerInnerExceptionMustBeNull)))
                {
                    return (ex as TException);
                }
                else
                {
                    throw;
                }
            }
        }

        public static TException ExpectFailure<TException, TInnerException, TInnerInnerException>(
            Action actionThatFails
          , string exceptionMessage                   = null
          , string innerExceptionMessage              = null
          , string innerInnerExceptionMessage         = null
          , bool   innerInnerInnerExceptionMustBeNull = false) where TException           : Exception 
                                                               where TInnerException      : Exception 
                                                               where TInnerInnerException : Exception
        {
            try
            {
                actionThatFails();
                Console.WriteLine("ERROR: Did not get expected exception");
                return null;
            }
            catch (Exception ex)
            {
                if ((CheckException<TException>(ex, exceptionMessage, false)) && (CheckException<TInnerException>(ex.InnerException, innerExceptionMessage, false)) && (CheckException<TInnerInnerException>(ex.InnerException.InnerException, innerInnerExceptionMessage, innerInnerInnerExceptionMustBeNull)))
                {
                    return (ex as TException);
                }
                else
                {
                    throw;
                }
            }
        }

        public static void ExpectAsyncFailure<TException>(Func<Task> actionThatFails, string exceptionMessage = null, bool innerExceptionMustBeNull = false) where TException : Exception
        {
            ExpectFailure<AggregateException, TException>(() => actionThatFails().Wait(), null, exceptionMessage, innerExceptionMustBeNull);
        }

        public static void ExpectAsyncFailure<TException, TInnerException>(Func<Task> actionThatFails, string exceptionMessage = null, string innerExceptionMessage = null, bool innerInnerExceptionMustBeNull = false) where TException : Exception where TInnerException : Exception
        {
            ExpectFailure<AggregateException, TException, TInnerException>(() => actionThatFails().Wait(), null, exceptionMessage, innerExceptionMessage, innerInnerExceptionMustBeNull);
        }

        private string GetTestName()
        {
            return GetType().Name;
        }

        public bool RunTestCoreAndCompareWithBaseline()
        {
            string outputPath   = GetTestName() + ".out";
            string baselinePath = GetTestName() + ".bsl";

            var fstream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            var swriter = new StreamWriter(fstream, Encoding.UTF8);
            // Convert all string writes of '\n' to '\r\n' so output files can be 'text' not 'binary'
            var twriter = new CarriageReturnLineFeedReplacer(swriter);
            Console.SetOut(twriter); // "redirect" Console.Out

            // Run Test
            RunTest();

            Console.Out.Flush();
            Console.Out.Dispose();

            // Recover the standard output stream
            StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush    = true;
            Console.SetOut(standardOutput);

            // Compare output file
            var comparisonResult = FindDiffFromBaseline(baselinePath, outputPath);

            if (string.IsNullOrEmpty(comparisonResult))
            {
                return true;
            }

            Console.WriteLine("Test Failed!");
            Console.WriteLine("Please compare baseline : {0} with output :{1}", Path.GetFullPath(baselinePath), Path.GetFullPath(outputPath));
            Console.WriteLine("Comparison Results : ");
            Console.WriteLine(comparisonResult);
            return false;
        }

        private string FindDiffFromBaseline(string baselinePath, string outputPath)
        {
            var expectedLines = File.ReadAllLines(baselinePath);
            var outputLines   = File.ReadAllLines(outputPath);
            var comparisonSb  = new StringBuilder();

            // Start compare results
            var expectedLength = expectedLines.Length;
            var outputLength   = outputLines.Length;
            var findDiffLength = Math.Min(expectedLength, outputLength);

            // Find diff for each lines
            for (var lineNo = 0; lineNo < findDiffLength; lineNo++)
            {
                if (!expectedLines[lineNo].Equals(outputLines[lineNo]))
                {
                    comparisonSb.AppendFormat("** DIFF at line {0} \n", lineNo);
                    comparisonSb.AppendFormat("A : {0} \n", outputLines[lineNo]);
                    comparisonSb.AppendFormat("E : {0} \n", expectedLines[lineNo]);
                }
            }

            var startIndex = findDiffLength - 1;
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            if (findDiffLength < expectedLength)
            {
                comparisonSb.AppendFormat("** MISSING \n");
                for (var lineNo = startIndex; lineNo < expectedLength; lineNo++)
                {
                    comparisonSb.AppendFormat("{0} : {1}", lineNo, expectedLines[lineNo]);
                }
            }
            if (findDiffLength < outputLength)
            {
                comparisonSb.AppendFormat("** EXTRA \n");
                for (var lineNo = startIndex; lineNo < outputLength; lineNo++)
                {
                    comparisonSb.AppendFormat("{0} : {1}", lineNo, outputLines[lineNo]);
                }
            }

            return comparisonSb.ToString();
        }
    }
}
