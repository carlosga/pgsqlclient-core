// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgParameterCollection
        : DbParameterCollection
    {
        private static readonly object SyncObject = new object();
        
        private readonly List<PgParameter> _parameters;
        private int                        _paramCount;
        private bool                       _isDirty;

        public new PgParameter this[string parameterName]
        {
            get { return GetParameter(parameterName) as PgParameter; }
            set { SetParameter(parameterName, value); }
        }

        public new PgParameter this[int index]
        {
            get { return GetParameter(index) as PgParameter; }
            set { SetParameter(index, value); }
        }

        public override int    Count    => _parameters.Count;
        public override object SyncRoot => SyncObject;

        internal bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

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
                throw new ArgumentNullException("The PgParameterCollection only accepts non-null PgParameter type objects.");
            }
            if (value.Parent != null)
            {
                throw new ArgumentException("The PgParameter is already contained by another PgParameterCollection.");
            }
            if (value.ParameterName == null || value.ParameterName.Length == 0)
            {
                value.ParameterName = $"Parameter{++_paramCount}";
            }
            else if (IndexOf(value) != -1)
            {
                throw new ArgumentException("PgParameterCollection already contains PgParameter with ParameterName '" + value.ParameterName + "'.");
            }

            value.Parent = this;

            _parameters.Add(value);
            
            _isDirty = true;

            return value;
        }

        public override int Add(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException($"The PgParameterCollection only accepts non-null PgParameter type objects.");
            }

            var parameter = value as PgParameter;

            if (parameter == null)
            {
                throw new InvalidCastException($"The PgParameterCollection only accepts non-null PgParameter type objects, not {value.GetType().Name} objects.");
            }

            return IndexOf(Add(parameter));
        }

        public override bool Contains(object value) => _parameters.Contains(value);

        public override bool Contains(string parameterName) => (-1 != IndexOf(parameterName));

        public override int IndexOf(object value) => _parameters.IndexOf(value as PgParameter);

        public override int IndexOf(string parameterName)
        {
            for (int i = 0; i < _parameters.Count; i++)
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
                throw new InvalidCastException($"The PgParameterCollection only accepts non-null PgParameter type objects, not {value.GetType().Name} objects.");
            }
            if (parameter != null)
            {
                throw new ArgumentException("The PgParameter is already contained by another PgParameterCollection.");
            }
            if (parameter.ParameterName == null || parameter.ParameterName.Length == 0)
            {
                parameter.ParameterName = $"Parameter{++_paramCount}";
            }
            else if (IndexOf(parameter) != -1)
            {
                throw new ArgumentException("PgParameterCollection already contains PgParameter with ParameterName '" + parameter.ParameterName + "'.");
            }
            
            _parameters.Insert(index, parameter);
            parameter.Parent = this;
            _isDirty         = true;
        }

        public override void Remove(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("The PgParameterCollection only accepts non-null PgParameter type objects.");
            }

            var parameter = value as PgParameter;

            if (parameter == null)
            {
                throw new InvalidCastException($"The PgParameterCollection only accepts non-null PgParameter type objects, not {value.GetType().Name} objects.");
            }

            if (!Contains(parameter))
            {
                throw new ArgumentException("Attempted to remove an PgParameter that is not contained by this PgParameterCollection.");
            }

            _parameters.Remove(parameter);
            parameter.Parent = null;
            _isDirty         = true;
        }

        public override void RemoveAt(string parameterName) => RemoveAt(IndexOf(parameterName));

        public override void RemoveAt(int index)
        {
            if (index < 0 || index > Count)
            {
                throw new IndexOutOfRangeException("The specified index does not exist.");
            }

            _parameters[index].Parent = null;
            _parameters.RemoveAt(index);
            _isDirty = true;
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            int index = IndexOf(parameterName);
            
            if (index == -1)
            {
                throw new IndexOutOfRangeException($"An PgParameter with ParameterName '{parameterName}' is not contained by this PgParameterCollection.");   
            }

            return _parameters[index];
        } 
        
        protected override DbParameter GetParameter(int index)
        {
            if (index < 0 || index >= _parameters.Count)
            {
                throw new IndexOutOfRangeException($"Invalid index {index} for this PgParameterCollection with Count={Count}.");
            }
            
            return _parameters[index];
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            if (index < 0 || index >= _parameters.Count)
            {
                throw new IndexOutOfRangeException($"Invalid index {index} for this PgParameterCollection with Count={Count}.");
            }

            _parameters[index] = value as PgParameter;
            _isDirty           = true;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            int index = IndexOf(parameterName);
            
            if (index == -1)
            {
                throw new IndexOutOfRangeException($"An PgParameter with ParameterName '{parameterName}' is not contained by this PgParameterCollection.");   
            }

            _parameters[index] = value as PgParameter;
            _isDirty           = true;
        }
    }
}
