// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgParameterCollection
        : DbParameterCollection
    {
        internal static readonly PgParameterCollection Empty = new PgParameterCollection();
        private static readonly object SyncObject = new object();

        private readonly List<PgParameter> _parameters;
        private int                        _paramCount;

        public new PgParameter this[int index]
        {
            get => GetParameter(index) as PgParameter;
            set => SetParameter(index, value);
        }

        public new PgParameter this[string parameterName]
        {
            get => GetParameter(parameterName) as PgParameter;
            set => SetParameter(parameterName, value);
        }

        public override int    Count    => _parameters.Count;
        public override object SyncRoot => SyncObject;

        internal PgParameterCollection()
        {
            _parameters = new List<PgParameter>();
        }

        public override void CopyTo(Array array, int index) => (_parameters as ICollection).CopyTo(array, index);
        
        public override void Clear()
        {
            _parameters.ForEach(p => p.Parent = null);
            _parameters.Clear();
        }
        
        public override IEnumerator GetEnumerator() => _parameters.GetEnumerator();

        public override void AddRange(Array values)
        {
            foreach (PgParameter parameter in values)
            {
                Add(parameter);
            }
        }

        public PgParameter AddWithValue(string parameterName, object value)  => Add(new PgParameter(parameterName, value));
        public PgParameter Add(string parameterName, object value)  => Add(new PgParameter(parameterName, value));
        public PgParameter Add(string parameterName, PgDbType type) => Add(parameterName, type, 0);
        public PgParameter Add(string parameterName, PgDbType providerType, int size) => Add(parameterName, providerType, size, null);

        public PgParameter Add(string parameterName, PgDbType providerType, int size, string sourceColumn)
        {
            return Add(new PgParameter(parameterName, providerType, size, sourceColumn));
        }

        public PgParameter Add(PgParameter value)
        {
            if (value == null)
            {
                throw ADP.ArgumentNull("The PgParameterCollection only accepts non-null PgParameter type objects.");
            }
            if (value.Parent != null)
            {
                throw ADP.Argument("The PgParameter is already contained by another PgParameterCollection.");
            }
            if (string.IsNullOrEmpty(value.ParameterName))
            {
                value.ParameterName = $"Parameter{Interlocked.Increment(ref _paramCount)}";
            }
            else if (IndexOf(value) != -1)
            {
                throw ADP.Argument($"PgParameterCollection already contains PgParameter with ParameterName '{value.ParameterName}'.");
            }

            value.Parent = this;

            _parameters.Add(value);

            return value;
        }

        public override int Add(object value)
        {
            if (value == null)
            {
                throw ADP.ArgumentNull("The PgParameterCollection only accepts non-null PgParameter type objects.");
            }

            var parameter = value as PgParameter;

            if (parameter == null)
            {
                throw ADP.InvalidCast($"The PgParameterCollection only accepts non-null PgParameter type objects, not {value.GetType().Name} objects.");
            }

            return IndexOf(Add(parameter));
        }

        public override bool Contains(object value)
        {
            if (value == null)
            {
                throw ADP.ArgumentNull("The PgParameterCollection only accepts non-null PgParameter type objects.");
            }

            var parameter = value as PgParameter;

            if (parameter == null)
            {
                throw ADP.InvalidCast($"The PgParameterCollection only accepts non-null PgParameter type objects, not {value.GetType().Name} objects.");
            }

            return _parameters.Contains(parameter);
        } 

        public override bool Contains(string parameterName) => (IndexOf(parameterName) != -1);

        public override int IndexOf(object value)
        {
            if (value == null)
            {
                throw ADP.ArgumentNull("The PgParameterCollection only accepts non-null PgParameter type objects.");
            }

            var parameter = value as PgParameter;

            if (parameter == null)
            {
                throw ADP.InvalidCast($"The PgParameterCollection only accepts non-null PgParameter type objects, not {value.GetType().Name} objects.");
            }

            return _parameters.IndexOf(parameter);
        }

        public override int IndexOf(string parameterName)
        {
            for (int i = 0; i < _parameters.Count; ++i)
            {
                if (_parameters[i].ParameterName == parameterName)
                {
                    return i;
                }
            }

            return -1;
        }

        public override void Insert(int index, object value)
        {
            var parameter = value as PgParameter;

            if (parameter == null)
            {
                throw ADP.InvalidCast($"The PgParameterCollection only accepts non-null PgParameter type objects, not {value.GetType().Name} objects.");
            }
            if (parameter.Parent != null)
            {
                throw ADP.Argument("The PgParameter is already contained by another PgParameterCollection.");
            }
            if (string.IsNullOrEmpty(parameter.ParameterName))
            {
                parameter.ParameterName = $"Parameter{Interlocked.Increment(ref _paramCount)}";
            }
            else if (IndexOf(parameter) != -1)
            {
                throw ADP.Argument($"PgParameterCollection already contains PgParameter with ParameterName '{parameter.ParameterName}'.");
            }
            
            _parameters.Insert(index, parameter);
            parameter.Parent = this;
        }

        public override void Remove(object value)
        {
            if (value == null)
            {
                throw ADP.ArgumentNull("The PgParameterCollection only accepts non-null PgParameter type objects.");
            }

            var parameter = value as PgParameter;

            if (parameter == null)
            {
                throw ADP.InvalidCast($"The PgParameterCollection only accepts non-null PgParameter type objects, not {value.GetType().Name} objects.");
            }

            if (!Contains(parameter))
            {
                throw ADP.Argument("Attempted to remove an PgParameter that is not contained by this PgParameterCollection.");
            }

            _parameters.Remove(parameter);
            parameter.Parent = null;
        }

        public override void RemoveAt(string parameterName) => RemoveAt(IndexOf(parameterName));

        public override void RemoveAt(int index)
        {
            if (index < 0 || index > Count)
            {
                throw ADP.IndexOutOfRange("The specified index does not exist.");
            }

            _parameters[index].Parent = null;
            _parameters.RemoveAt(index);
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            int index = IndexOf(parameterName);
            
            if (index == -1)
            {
                throw ADP.IndexOutOfRange($"An PgParameter with ParameterName '{parameterName}' is not contained by this PgParameterCollection.");   
            }

            return _parameters[index];
        } 
        
        protected override DbParameter GetParameter(int index)
        {
            if (index < 0 || index >= _parameters.Count)
            {
                throw ADP.IndexOutOfRange($"Invalid index {index} for this PgParameterCollection with Count={Count}.");
            }
            
            return _parameters[index];
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            if (index < 0 || index >= _parameters.Count)
            {
                throw ADP.IndexOutOfRange($"Invalid index {index} for this PgParameterCollection with Count={Count}.");
            }

            _parameters[index] = value as PgParameter;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            int index = IndexOf(parameterName);
            
            if (index == -1)
            {
                throw ADP.IndexOutOfRange($"An PgParameter with ParameterName '{parameterName}' is not contained by this PgParameterCollection.");   
            }

            _parameters[index] = value as PgParameter;
        }
    }
}
