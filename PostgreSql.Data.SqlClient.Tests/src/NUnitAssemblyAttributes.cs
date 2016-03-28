// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NUnit.Framework;

// Connection pools are created per process, per connection string. So having a bunch of tests
// run in parallel with identical connection strings causes Clear Pool tests to report incorrect results.
[assembly: LevelOfParallelism(1)]
[assembly: Parallelizable(ParallelScope.None)]
