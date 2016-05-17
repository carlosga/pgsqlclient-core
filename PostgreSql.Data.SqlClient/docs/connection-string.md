# Connection string parameters

| Name             | Type   | Default     | Description |
|------------------|--------|-------------|-------------|
| Application Name -or- App | string | pgsqlclient | The name of the application associated with the connection string |
| Command Timeout -or- Statement Timeout | int    | 0           | The wait time (in seconds) before terminating the attempt to execute a command and generating an error. |
| Connect Retry Count | int | 1 | The number of reconnections attempted after identifying that there was an idle connection failure. This must be an integer between 0 and 255. Default is 1. Set to 0 to disable reconnecting on idle connection failures. |
| Connect Retry Interval | int | 10 | Amount of time (in seconds) between each reconnection attempt after identifying that there was an idle connection failure. This must be an integer between 1 and 60. The default is 10 seconds. |
| Connection Timeout -or- Connect Timeout -or- Timeout | int | 0 | The length of time (in seconds) to wait for a connection to the server before terminating the attempt and generating an error. |
| Connection Lifetime -or- Load Balance Timeout | int | 0  | The minimum time, in seconds, for the connection to live in the connection pool before being destroyed. |
| Data Source -or- Host -or- Server | string | localhost | The name or network address of the instance of PostgreSql Server to connect to |
| Default Transaction Read Only | bool | false | The default read-only status of each new transaction |
| Default Table Space | string |  | The default tablespace in which to create objects (tables and indexes) when a CREATE command does not explicitly specify a tablespace |
| Encrypt          | bool   | false | Indicates whether PostgreSQL Server uses TLS encryption for all data sent between the client and server if the server has a certificate installed |
| Initial Catalog -or- Database | string |  | The name of the database associated with the connection |
| Lock Timeout | int | 0 | Abort any statement that waits longer than the specified number of milliseconds while attempting to acquire a lock on a table, index, row, or other database object |
| Max Pool Size | int | 100 | The maximum number of connections allowed in the connection pool for this specific connection string |
| Min Pool Size | int | 0 | The minimum number of connections allowed in the connection pool for this specific connection string |
| Multiple Active Result Sets | false | bool | When true, an application can maintain multiple active result sets (MARS). When false, an application must process or cancel all result sets from one batch before it can execute any other batch on that connection |
| Packet Size | int | 8192 | The size in bytes of the network packets used to communicate with an instance of PostgreSQL Server. |
| Password -or- User Password | string | | The password for the PostgreSQL Server account |
| Port Number -or- Port | int | 5432 | The TCP TCP port the PostgreSQL Server is listening on |
| Pooling | bool | true | Indicates whether the connection will be pooled or explicitly opened every time that the connection is requested. |
| Search Path | string |  | specifies the order in which schemas are searched when an object (table, data type, function, etc.) is referenced by a simple name with no schema specified |
| User ID -or- User Name -or- User | string | | The user ID to be used when connecting to PostgreSQL Server |
