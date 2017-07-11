// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace System
{
    internal static class SqlExtensions
    {
        private const char CommandSeparator = ';';

        internal static List<string> SplitCommandText(this string commandText, ref List<string> commands)
        {
            if (commands == null)
            {
                commands = new List<string>(1);
            }
            else
            {
                commands.Clear();
            }
            
            if (commandText != null && commandText.IndexOf(CommandSeparator) != -1)
            {
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
                    else if (!inLiteral && (sym == CommandSeparator || i == lastIndex))
                    {
                        if (i == lastIndex && sym != CommandSeparator)
                        {
                            offset = 1;
                        }

                        var query = commandText.Substring(from, i - from + offset).Trim();
                        if (query.Length > 0)
                        {
                            commands.Add(query);
                        }

                        from = i + 1;
                    }
                }
            }
            else
            {
                commands.Add(commandText);
            }

            return commands;
        }

        internal static string ToStoredProcedureCall(this string commandText, PgParameterCollection parameters)
        {
           if (commandText.Trim().ToLower().StartsWith("select "))
           {
               return commandText;
           }

           var spCall    = new StringBuilder();
           var lastIndex = parameters.Count - 1;

           spCall.AppendFormat("SELECT * FROM {0}(", commandText);

           for (int i = 0; i < parameters.Count; ++i)
           {
               var parameter = parameters[i];

               if (parameter.Direction == ParameterDirection.Input
                || parameter.Direction == ParameterDirection.InputOutput)
               {
                   spCall.Append(parameters[i].ParameterName);

                   if (i != lastIndex)
                   {
                       spCall.Append(",");
                   }
               }
           }

           spCall.Append(")");

           return spCall.ToString();
        }

        internal static string ParseCommandText(this string           commandText
                                              , PgParameterCollection parameters
                                              , ref List<int>         parameterIndices)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return commandText;
            }
            if (commandText.IndexOf('@') == -1)
            {
                return commandText;
            }

            var  builder      = new StringBuilder(commandText.Length);
            var  paramBuilder = new StringBuilder(15);
            var  inLiteral    = false;
            var  inParam      = false;
            int  paramIndex   = 0;
            char sym;

            if (parameterIndices == null)
            {
                parameterIndices = new List<int>(parameters.Count);
            }
            else
            {
                parameterIndices.Clear();
                parameterIndices.Capacity = parameters.Count;
            }

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
                        var paramName = paramBuilder.ToString();
                        var index     = parameters.IndexOf(paramName);

                        if (paramIndex <= index)
                        {
                            parameterIndices.Add(index);
                            builder.AppendFormat("${0}{1}", ++paramIndex, sym);
                        }
                        else
                        {
                            builder.AppendFormat("${0}{1}", index + 1, sym);
                        }
                        
                        paramBuilder.Clear();
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
                parameterIndices.Add(parameters.IndexOf(paramBuilder.ToString()));
                builder.AppendFormat("${0}", (++paramIndex).ToString());
            }

            return builder.ToString();
        }
    }
}
