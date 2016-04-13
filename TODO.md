# postgresqlclient - ado.net data provider for .net core

## New .net core/asp.net configuration system

https://github.com/aspnet/Configuration/

## Work in progress

- Connection runtime configuration. http://www.postgresql.org/docs/9.5/static/runtime-config-client.html

- postgresql, data type support.

## TO BE DONE

- State pattern for commands and connections ??

- Write unit tests ( port the .net core sql client ones ?? ).

        https://github.com/dotnet/corefx/pull/7164

  Add new unit tests for PostgreSql specific features:

    - PgDataReader.GetColumnSchema
    - Arrays
    - Complex Types
    - Time zones
    - COPY protocol
    - ...
- Complex types support & unit tests.
- COPY support & unit tests.
- Custom struct types for postgresql types ?? ( PgDecimal, PgString, PgBinary, PgDateTime, PgTimestamp, ... )
- Provider statistics ( SqlClient reference https://msdn.microsoft.com/en-us/library/7h2ahss8(v=vs.110).aspx )
- Look at the missing authentication methods and see if they can be implemented.
- Add ApplicationName as a new connection string parameter 
  (https://msdn.microsoft.com/es-es/library/system.data.sqlclient.sqlconnectionstringbuilder.applicationname(v=vs.110).aspx)).
- Reimplement connection pooling
    ==> https://blogs.msdn.microsoft.com/dhuba/2011/05/01/concurrent-object-pool/
    ==> https://dpaoliello.wordpress.com/2014/03/30/connection-resiliency-in-ado-net-2/
    ==> http://javawithswaranga.blogspot.com.es/2011/10/generic-and-concurrent-object-pool.html 
    
    - ConnectRetryCount. Controls the number of reconnection attempts after the client identifies an idle connection failure. Valid values are 0 to 255. The default is 1. 0 means do not attempt to reconnect (disable connection resiliency). For additional information about idle connection resiliency,  see Technical Atricle – Idle Connection Resiliency.
    - ConnectRetryInterval. Specifies the time between each connection retry attempt (ConnectRetryCount). Valid values are 1 (default) to 60 seconds, applied after the first reconnection attempt. When a broken connection is detected, the client immediately attempts to reconnect; this is the first     reconnection attempt and only occurs if ConnectRetryCount is greater than 0. If the first reconnection attempt fails and ConnectRetryCount is greater than 1, the client waits ConnectRetryInterval to try the second and subsequent reconnection attempts. For additional information about idle       connection resiliency, see Technical Atricle – Idle Connection Resiliency.
- Time zone support.
- Implement an EF 7 provider ??.
- Async support ??.
  (https://msdn.microsoft.com/es-es/library/system.data.sqlclient.sqlconnectionstringbuilder.authentication(v=vs.110).aspx)
- Thread safety ??.
- Geometric types operators. http://www.postgresql.org/docs/8.2/static/functions-geometry.html

## DONE

- Get the basic features working ( open connection, transaction & run a simple query ).
- New connection string options:
    - search_path.
    - fetch size.
    - MultipleActiveResultSets.
- Rename connection string options:
    - ssl -> Encrypt.
- Wire up SSL support ( TLS 1.2 only if possible ).
- Merge PgStatement Parse & Describe in one single step, sending both packets in a single roundtrip.
- Merge PgStatement Bind & Execute in one single step, sending both packets in a single roundtrip.
- Implement IDbColumnSchemaGenerator on PgDataReader.
- Query cancellation.
- Basic MARS support ( client side ).
    - Check how it works when using several parametrized queries.
