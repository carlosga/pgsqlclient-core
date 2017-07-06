[![build status](https://gitlab.com/carlosga/pgsqlclient-core/badges/master/build.svg)](https://gitlab.com/carlosga/pgsqlclient-core/commits/master)

# pgsqlclient core

[pgsqlclient](https://gitlab.com/carlosga/pgsqlclient) rewrite under .net core (*netstandard1.5*).

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

| Name      | Version          |
|-----------|------------------|
| .NET Core | .netstandard 2.0 |
| git       |                  |

```
sudo apt-get install git
```

### Installing (linux)

1. Clone repository

```
git clone --recursive https://gitlab.com/carlosga/pgsqlclient-core
```

2. Build (Debug)

```
./build.sh
```

## Running the tests (linux)

1. Step into the test project directory

```
cd ./tests/PostgreSql.Data.SqlClient.Tests
```

2. Build the tests (**right now you will need to manually copy the sqlite binaries to the build directory**)

```
dotnet build
```

3. Run the tests

```
dotnet test
```

# Documentation

* [Connection String](docs/connection-string.md)
* [Data types](docs/data-types.md)
   * [Composite types](docs/composite-bindings.md)

## Built With

| Library                                           | Source | License |
|---------------------------------------------------|--------|---------|
| [**libidn**](http://www.gnu.org/software/libidn/) | Source | LGPLv3  |

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Authors

* **Carlos Guzmán Álvarez** - *Initial work* - [carlosga](https://gitlab.com/carlosga)

## License

This project is licensed under the **MIT license** - see the [LICENSE.md](license.md) file for details

## Acknowledgments

* The connection pooling & unit test suite implementations are based on the [Microsoft SqlClient ADO.NET Provider](https://github.com/dotnet/corefx) ones, licensed under the [MIT License](https://github.com/dotnet/corefx/blob/master/LICENSE).
* Some parts of the PgDate structure has been ported from PostgreSql source code.
* GNU libidn, licensend under the terms of the GNU Lesser General Public License.
* The [template](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2) this README is based on
* The [template](https://gist.githubusercontent.com/PurpleBooth/b24679402957c63ec426/raw/5c4f62c1e50c1e6654e76e873aba3df2b0cdeea2/Good-CONTRIBUTING.md-template.md) the project contribution guide is based on.