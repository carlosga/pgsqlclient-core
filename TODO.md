# postgresqlclient - ado.net data provider for .net core

- ~~Get the basic features working ( open connection, transaction & run a simple query ).~~
- New connection options
    - search_path.
            synonyms.Add("search path", "search path");
            synonyms.Add("search_path", "search path");
            synonyms.Add("searchpath", "search path");
    - fetch size.
            synonyms.Add("fetch size", "fetch size");
            synonyms.Add("fetchsize", "fetch size");
- Complext types support.
- COPY Support.
- Custom struct types for postgresql types ?? ( PgDecimal, PgString, PgBinary, PgDateTime, PgTimestamp, ... )
- Query cancellation.
- SSPI auth ??
- Thread safety ??
- Time zone support.
- Write unit tests ( port the .net core sql client ones ?? ).
- Implement an EF 7 provider.
- Async support.