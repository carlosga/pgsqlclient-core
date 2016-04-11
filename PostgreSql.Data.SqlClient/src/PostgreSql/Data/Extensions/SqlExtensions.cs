// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace System
{
    internal static class SqlExtensions
    {
        internal static List<string> SplitCommandText(this string commandText, char separator = ';')
        {
            if (commandText == null || commandText.Length == 0 || commandText.IndexOf(';') == -1)
            {
                return null;
            }

            var  queries   = new List<string>();
            var  inLiteral = false;
            int  from      = 0;
            int  offset    = 0;
            int  lastIndex = (commandText.Length - 1);
            char sym;

            for (int i = 0; i < commandText.Length; ++i)
            {
                sym = commandText[i];

                if (sym == '\'' || sym == '\"')
                {
                    inLiteral = !inLiteral;
                }
                else if (!inLiteral && (sym == separator || i == lastIndex))
                {
                    if (i == (commandText.Length - 1) && sym != separator)
                    {
                        offset = 1;
                    }

                    var query = commandText.Substring(from, i - from + offset).Trim();
                    if (query.Length > 0)
                    {
                        queries.Add(query);
                    }

                    from = i + 1;
                }
            }

            return queries;
        }

        internal static string ToStoredProcedureCall(this string commandText, PgParameterCollection parameters)
        {
           if (commandText.Trim().ToLower().StartsWith("select "))
           {
               return commandText;
           }

           var paramsText = new StringBuilder();

           // Append the stored proc parameter name
           paramsText.Append(commandText);
           paramsText.Append("(");

           for (int i = 0; i < parameters.Count; ++i)
           {
               var parameter = parameters[i];

               if (parameter.Direction == ParameterDirection.Input
                || parameter.Direction == ParameterDirection.InputOutput)
               {
                   // Append parameter name to parameter list
                   paramsText.Append(parameters[i].ParameterName);

                   if (i != parameters.Count - 1)
                   {
                       paramsText.Append(",");
                   }
               }
           }

           paramsText.Append(")");
           paramsText.Replace(",)", ")");

           return $"SELECT * FROM {paramsText.ToString()}";
        }

        internal static string ParseCommandText(this string           commandText
                                              , PgParameterCollection parameters
                                              , ref List<int>         parameterIndices)
        {
            var builder         = new StringBuilder();
            var paramBuilder    = new StringBuilder();
            var inLiteral       = false;
            var inParam         = false;
            int paramIndex      = 0;

            parameterIndices.Clear();

            if (commandText.IndexOf('@') == -1)
            {
                return commandText;
            }

            char sym;

            for (int i = 0; i < commandText.Length; ++i)
            {
                sym = commandText[i];

                if (inParam)
                {
                    if (Char.IsLetterOrDigit(sym) || sym == '_' || sym == '$')
                    {
                        paramBuilder.Append(sym);
                    }
                    else
                    {
                        parameterIndices.Add(parameters.IndexOf(paramBuilder.ToString()));
                        paramBuilder.Length = 0;
                        builder.AppendFormat("${0}", ++paramIndex);
                        builder.Append(sym);
                        inParam = false;
                    }
                }
                else
                {
                    if (sym == '\'' || sym == '\"')
                    {
                        inLiteral = !inLiteral;
                    }
                    else if (!inLiteral && sym == '@')
                    {
                        inParam = true;
                        paramBuilder.Append(sym);
                        continue;
                    }

                    builder.Append(sym);
                }
            }

            if (inParam)
            {
                parameters.Add(parameters.IndexOf(paramBuilder.ToString()));
                builder.AppendFormat("${0}", ++paramIndex);
            }

            return builder.ToString();
        }
    }
}
