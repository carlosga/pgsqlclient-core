// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PostgreSql.Data.SqlClient.Tests
{
    /// <summary>
    /// Defines a collection of types to be used by the test. Tests can start with CreateSql2005Collection or 
    /// CreateSql2008Collection and add/remove types, as needed.
    /// </summary>
    public sealed class SqlRandomTypeInfoCollection 
        : KeyedCollection<PgDbType, SqlRandomTypeInfo>
    {
        private static readonly SqlRandomTypeInfo[] s_sqlTypes =
        {
            // var types
            new SqlVarCharTypeInfo(),
            
            // Boolean types
            new SqlBooleanTypeInfo(),

            // integer data types
            new SqlSmallIntTypeInfo(),
            new SqlIntTypeInfo(),
            new SqlBigIntTypeInfo(),

            // date/time types
            new SqlDateTypeInfo(),
            new SqlTimeTypeInfo(),
            // new SqlTimestampTypeInfo(),
            // new SqlTimeTzTypeInfo(),
            // new SqlTimestampTzTypeInfo(),
            // new SqlIntervalTypeInfo(),

            // fixed length blobs
            // new SqlCharTypeInfo(),
            // new SqlBinaryTypeInfo(),

            // large blobs
            new SqlTextTypeInfo(),

            // decimal
            new SqlDecimalTypeInfo(),

            // money types
            new SqlMoneyTypeInfo(),

            // float types
            new SqlFloatTypeInfo(),
            new SqlDoubleTypeInfo()
        };

        // reset it each time collection is modified
        private IList<SqlRandomTypeInfo> _sparseColumns = null;

        public IList<SqlRandomTypeInfo> SparseColumns
        {
            get
            {
                if (_sparseColumns == null)
                {
                    // rebuild it
                    var sparseColumns = this.Where(t => t.CanBeSparseColumn).ToArray();
                    _sparseColumns    = new ReadOnlyCollection<SqlRandomTypeInfo>(sparseColumns);
                }

                return _sparseColumns;
            }
        }

        public SqlRandomTypeInfoCollection(params SqlRandomTypeInfo[] typeSet1)
            : this(typeSet1, null)
        {
        }

        protected override void ClearItems()
        {
            _sparseColumns = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, SqlRandomTypeInfo item)
        {
            _sparseColumns = null;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            _sparseColumns = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, SqlRandomTypeInfo item)
        {
            _sparseColumns = null;
            base.SetItem(index, item);
        }

        /// <summary>
        /// helper c-tor to fill one or two type sets
        /// </summary>
        private SqlRandomTypeInfoCollection(SqlRandomTypeInfo[] typeSet1, SqlRandomTypeInfo[] typeSet2)
        {
            if (typeSet1 != null)
            {
                AddRange(typeSet1);
            }
            if (typeSet2 != null)
            {
                AddRange(typeSet2);
            }
        }

        protected override PgDbType GetKeyForItem(SqlRandomTypeInfo item)
        {
            return item.Type;
        }

        public void AddRange(SqlRandomTypeInfo[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                Add(types[i]);   
            }
        }

        /// <summary>
        /// creates a collection of supported types 
        /// </summary>
        public static SqlRandomTypeInfoCollection CreateSqlTypesCollection()
        {
            return new SqlRandomTypeInfoCollection(s_sqlTypes);
        }

        /// <summary>
        /// returns random type info
        /// </summary>
        public SqlRandomTypeInfo Next(SqlRandomizer rand)
        {
            return base[rand.NextIntInclusive(0, maxValueInclusive: Count - 1)];
        }

        /// <summary>
        /// returns random type info from the columns that can be sparse
        /// </summary>
        public SqlRandomTypeInfo NextSparse(SqlRandomizer rand)
        {
            var sparseColumns = SparseColumns;
            return sparseColumns[rand.NextIntInclusive(0, maxValueInclusive: sparseColumns.Count - 1)];
        }
    }
}
