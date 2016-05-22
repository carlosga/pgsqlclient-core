// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System;
using Xunit;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class ParametersTest
    {
        [Fact]
        public static void CodeCoveragePgSqlClient()
        {
            PgParameterCollection opc = new PgCommand().Parameters;

            Assert.True(opc.Count == 0, string.Format("FAILED: Expected count: {0}. Actual count: {1}.", 0, opc.Count));
            Assert.False(((IList)opc).IsReadOnly    , "FAILED: Expected collection to NOT be read only.");
            Assert.False(((IList)opc).IsFixedSize   , "FAILED: Expected collection to NOT be fixed size.");
            Assert.False(((IList)opc).IsSynchronized, "FAILED: Expected collection to NOT be synchronized.");
            DataTestClass.AssertEqualsWithDescription("Object", ((IList)opc).SyncRoot.GetType().Name, "FAILED: Incorrect SyncRoot Name");

            {
                string failValue;
                DataTestClass.AssertThrowsWrapper<IndexOutOfRangeException>(() => failValue = opc[0].ParameterName    , "Invalid index 0 for this PgParameterCollection with Count=0.");
                DataTestClass.AssertThrowsWrapper<IndexOutOfRangeException>(() => failValue = opc["@p1"].ParameterName, "An PgParameter with ParameterName '@p1' is not contained by this PgParameterCollection.");
            }
            
            DataTestClass.AssertThrowsWrapper<ArgumentNullException>(() => opc.Add(null), "The PgParameterCollection only accepts non-null PgParameter type objects.");

            opc.Add((object)new PgParameter());
            IEnumerator enm = opc.GetEnumerator();
            Assert.True(enm.MoveNext(), "FAILED: Expected MoveNext to be true");
            DataTestClass.AssertEqualsWithDescription("Parameter1", ((PgParameter)enm.Current).ParameterName, "FAILED: Incorrect ParameterName");

            opc.Add(new PgParameter());
            DataTestClass.AssertEqualsWithDescription("Parameter2", opc[1].ParameterName, "FAILED: Incorrect ParameterName");

            opc.Add(new PgParameter(null, null));
            opc.Add(new PgParameter(null, PgDbType.Integer));
            DataTestClass.AssertEqualsWithDescription("Parameter4", opc["Parameter4"].ParameterName, "FAILED: Incorrect ParameterName");

            opc.Add(new PgParameter("Parameter5", PgDbType.VarChar, 20));
            opc.Add(new PgParameter(null        , PgDbType.VarChar, 20, "a"));
            opc.RemoveAt(opc[3].ParameterName);
            DataTestClass.AssertEqualsWithDescription(-1, opc.IndexOf(null), "FAILED: Incorrect index for null value");

            PgParameter p = opc[0];

            DataTestClass.AssertThrowsWrapper<ArgumentException>(() => opc.Add((object)p)               , "The PgParameter is already contained by another PgParameterCollection.");
            DataTestClass.AssertThrowsWrapper<ArgumentException>(() => new PgCommand().Parameters.Add(p), "The PgParameter is already contained by another PgParameterCollection.");
            DataTestClass.AssertThrowsWrapper<ArgumentNullException>(() => opc.Remove(null)             , "The PgParameterCollection only accepts non-null PgParameter type objects.");

            string pname = p.ParameterName;
            p.ParameterName = pname;
            p.ParameterName = pname.ToUpper();
            p.ParameterName = pname.ToLower();
            p.ParameterName = "@p1";
            p.ParameterName = pname;

            opc.Clear();
            opc.Add(p);

            opc.Clear();
            opc.AddWithValue("@p1", null);

            DataTestClass.AssertEqualsWithDescription(-1, opc.IndexOf(p.ParameterName), "FAILED: Incorrect index for parameter name");

            opc[0] = p;
            DataTestClass.AssertEqualsWithDescription(0, opc.IndexOf(p.ParameterName), "FAILED: Incorrect index for parameter name");

            Assert.True(opc.Contains(p.ParameterName), "FAILED: Expected collection to contain provided parameter.");
            Assert.True(opc.Contains(opc[0])         , "FAILED: Expected collection to contain provided parameter.");

            opc[0] = p;
            opc[p.ParameterName] = new PgParameter(p.ParameterName, null);
            opc[p.ParameterName] = new PgParameter();
            opc.RemoveAt(0);

            new PgCommand().Parameters.Clear();
            new PgCommand().Parameters.CopyTo(new object[0], 0);
                        
            Assert.False(new PgCommand().Parameters.GetEnumerator().MoveNext(), "FAILED: Expected MoveNext to be false");

            DataTestClass.AssertThrowsWrapper<InvalidCastException>(() => new PgCommand().Parameters.Add(0)      , "The PgParameterCollection only accepts non-null PgParameter type objects, not Int32 objects.");
            DataTestClass.AssertThrowsWrapper<InvalidCastException>(() => new PgCommand().Parameters.Insert(0, 0), "The PgParameterCollection only accepts non-null PgParameter type objects, not Int32 objects.");
            DataTestClass.AssertThrowsWrapper<InvalidCastException>(() => new PgCommand().Parameters.Remove(0)   , "The PgParameterCollection only accepts non-null PgParameter type objects, not Int32 objects.");

            DataTestClass.AssertThrowsWrapper<ArgumentException>(() => new PgCommand().Parameters.Remove(new PgParameter()), "Attempted to remove an PgParameter that is not contained by this PgParameterCollection.");
        }
    }
}
