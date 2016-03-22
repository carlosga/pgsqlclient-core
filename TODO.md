# postgresqlclient - ado.net data provider for .net core

## TO BE DONE
           
- Basic MARS support ( client side ).
- Merge PgStatement Parse & Describe in one single step, sending both packets in a single roundtrip.
- Merge PgStatement Bind & Execute in one single step, sending both packets in a single roundtrip.
- Implement IDbColumnSchemaGenerator on PgDataReader.

  Custom DbColumn (https://github.com/dotnet/corefx/blob/master/src/System.Data.Common/src/System/Data/Common/DbColumn.cs) class.

- Write unit tests ( port the .net core sql client ones ?? ).
- Complext types support.
- COPY Support.
- Custom struct types for postgresql types ?? ( PgDecimal, PgString, PgBinary, PgDateTime, PgTimestamp, ... )
- Query cancellation.
- Wire up SSL support ( TLS 1.2 only if possible )
- Reimplement connection pooling
    ==> https://dpaoliello.wordpress.com/2014/03/30/connection-resiliency-in-ado-net-2/
    ==> http://javawithswaranga.blogspot.com.es/2011/10/generic-and-concurrent-object-pool.html 
    
    - ConnectRetryCount. Controls the number of reconnection attempts after the client identifies an idle connection failure. Valid values are 0 to 255. The default is 1. 0 means do not attempt to reconnect (disable connection resiliency). For additional information about idle connection resiliency,  see Technical Atricle – Idle Connection Resiliency.
    - ConnectRetryInterval. Specifies the time between each connection retry attempt (ConnectRetryCount). Valid values are 1 (default) to 60 seconds, applied after the first reconnection attempt. When a broken connection is detected, the client immediately attempts to reconnect; this is the first     reconnection attempt and only occurs if ConnectRetryCount is greater than 0. If the first reconnection attempt fails and ConnectRetryCount is greater than 1, the client waits ConnectRetryInterval to try the second and subsequent reconnection attempts. For additional information about idle       connection resiliency, see Technical Atricle – Idle Connection Resiliency.
- Time zone support.
- Implement an EF 7 provider ??.
- Async support ??.
- SSPI auth ??.
- Thread safety ??.

## DONE

- Get the basic features working ( open connection, transaction & run a simple query ).
- New connection string options:
    - search_path.
    - fetch size.
    - MultipleActiveResultSets.
- Renamed connection string options:    
    - ssl -> Encrypt.