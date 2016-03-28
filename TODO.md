# postgresqlclient - ado.net data provider for .net core

## New .net core/asp.net configuration system

https://github.com/aspnet/Configuration/

## Work in progress

- Write unit tests ( port the .net core sql client ones ?? ).

        https://github.com/dotnet/corefx/pull/7164

  Add new unit tests for PostgreSql specific features:
  
    - Arrays
    - Complex Types
    - Time zones
    - COPY protocol
    - ...
           
- Basic MARS support ( client side ).

    - Check how it works when using several parametrized queries.

- Implement IDbColumnSchemaGenerator on PgDataReader.

  Custom DbColumn (https://github.com/dotnet/corefx/blob/master/src/System.Data.Common/src/System/Data/Common/DbColumn.cs) class.

## TO BE DONE

- Complext types support & unit tests.
- COPY support & unit tests.
- Custom struct types for postgresql types ?? ( PgDecimal, PgString, PgBinary, PgDateTime, PgTimestamp, ... )
- Query cancellation.
- Provider statistics ( SqlClient reference https://msdn.microsoft.com/en-us/library/7h2ahss8(v=vs.110).aspx )
- Look at the missing authentication methods and see if they can be implemented.
  SSPI auth ??. If it gets implemented implement a new Authentication connection string parameter.
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

## DONE

- Get the basic features working ( open connection, transaction & run a simple query ).
- New connection string options:
    - search_path.
    - fetch size.
    - MultipleActiveResultSets.
- Renamed connection string options:    
    - ssl -> Encrypt.
- Wire up SSL support ( TLS 1.2 only if possible ).
- Merge PgStatement Parse & Describe in one single step, sending both packets in a single roundtrip.
- Merge PgStatement Bind & Execute in one single step, sending both packets in a single roundtrip.
    
## 50.2.7. Canceling Requests in Progress

During the processing of a query, the frontend might request cancellation of the query. The cancel request is not sent directly on the open connection 
to the backend for reasons of implementation efficiency: we don't want to have the backend constantly checking for new input from the frontend during 
query processing. Cancel requests should be relatively infrequent, so we make them slightly cumbersome in order to avoid a penalty in the normal case.

To issue a cancel request, the frontend opens a new connection to the server and sends a CancelRequest message, 
rather than the StartupMessage message that would ordinarily be sent across a new connection.
The server will process this request and then close the connection. For security reasons, no direct reply is made to the cancel request message.

A CancelRequest message will be ignored unless it contains the same key data (PID and secret key) passed to the frontend during connection start-up. 
If the request matches the PID and secret key for a currently executing backend, the processing of the current query is aborted. 
(In the existing implementation, this is done by sending a special signal to the backend process that is processing the query.)

The cancellation signal might or might not have any effect — for example, if it arrives after the backend has finished processing the query,
then it will have no effect. If the cancellation is effective, it results in the current command being terminated early with an error message.

The upshot of all this is that for reasons of both security and efficiency, the frontend has no direct way to tell whether a cancel request has succeeded. 
It must continue to wait for the backend to respond to the query. Issuing a cancel simply improves the odds that the current query will finish soon, 
and improves the odds that it will fail with an error message instead of succeeding.

Since the cancel request is sent across a new connection to the server and not across the regular frontend/backend communication link,
it is possible for the cancel request to be issued by any process, not just the frontend whose query is to be canceled. 
This might provide additional flexibility when building multiple-process applications. It also introduces a security risk, 
in that unauthorized persons might try to cancel queries. 
The security risk is addressed by requiring a dynamically generated secret key to be supplied in cancel requests.            
