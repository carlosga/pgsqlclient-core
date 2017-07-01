[![build status](https://gitlab.com/carlosga/pgsqlclient-core/badges/master/build.svg)](https://gitlab.com/carlosga/pgsqlclient-core/commits/master)

# pgsqlclient core

[pgsqlclient](https://gitlab.com/carlosga/pgsqlclient) rewrite under .net core (*netstandard1.5*) on linux.

# documentation

* [Connection String](docs/connection-string.md)
* [Data types](docs/data-types.md)
   * [Composite types](docs/composite-bindings.md)

# License

Licensed under the [MIT license](license.md).

# Credits

* The connection pooling & unit test suite implementations are based on the [Microsoft SqlClient ADO.NET Provider](https://github.com/dotnet/corefx) ones, licensed under the [MIT License](https://github.com/dotnet/corefx/blob/master/LICENSE).
* Some parts of the PgDate structure has been ported from PostgreSql source code.
* GNU libidn, licensend under the terms of the GNU Lesser General Public License.
