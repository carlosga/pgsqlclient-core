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
        
        private List<PgParameter> _parameters;

        public new PgParameter this[string parameterName]
        {
            get { return this[IndexOf(parameterName)]; }
            set { this[IndexOf(parameterName)] = value; }
        }

        public new PgParameter this[int index]
        {
            get { return _parameters[index]; }
            set { _parameters[index] = value; }
        }

        public override int    Count    => _parameters.Count;
        public override object SyncRoot => SyncObject;

        internal PgParameterCollection()
        {
            _parameters = new List<PgParameter>();
        }

        public override void CopyTo(Array array, int index) => _parameters.CopyTo((PgParameter[])array, index);
        public override void Clear()                        => _parameters.Clear();        
        public override IEnumerator GetEnumerator()         => _parameters.GetEnumerator();

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
                throw new ArgumentException("The value parameter is null.");
            }
            if (value.Parent != null)
            {
                throw new ArgumentException("The PgParameter specified in the value parameter is already added to this or another FbParameterCollection.");
            }
            if (value.ParameterName == null || value.ParameterName.Length == 0)
            {
                //value.ParameterName = GenerateParameterName();
            }
            else
            {
                if (IndexOf(value) != -1)
                {
                    throw new ArgumentException("PgParameterCollection already contains PgParameter with ParameterName '" + value.ParameterName + "'.");
                }
            }

            _parameters.Add(value);

            return value;
        }

        public override int Add(object value)
        {
            var parameter = value as PgParameter;

            if (value == null)
            {
                throw new InvalidCastException("The parameter passed was not a PgParameter.");
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

        public override void Insert(int index, object value) => _parameters.Insert(index, value as PgParameter);

        public override void Remove(object value)
        {
            var parameter = value as PgParameter;

            if (value == null)
            {
                throw new InvalidCastException("The parameter passed was not a PgParameter.");
            }
            if (!Contains(parameter))
            {
                throw new Exception("The parameter does not exist in the collection.");
            }

            _parameters.Remove(parameter);

            parameter.Parent = null;
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
        }

        protected override DbParameter GetParameter(string parameterName) => this[parameterName];
        protected override DbParameter GetParameter(int index)            => _parameters[index];

        protected override void SetParameter(int index, DbParameter value)
        {
            _parameters[index] = value as PgParameter;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            this[parameterName] = value as PgParameter;
        }
    }
}
