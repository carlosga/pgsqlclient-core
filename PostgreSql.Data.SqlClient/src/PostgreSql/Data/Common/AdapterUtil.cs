// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using PostgreSql.Data.Frontend;
using PostgreSql.Data.PgTypes;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Res = System.SR;

namespace System
{
    internal static partial class SR
    {
        // internal static string GetString(string value)
        // {
        //     return value;
        // }

        // internal static string GetString(string format, params object[] args)
        // {
        //     return SR.Format(format, args);
        // }
    }
}

namespace System.Data.Common
{
    internal static class ADP
    {
        // The class ADP defines the exceptions that are specific to the Adapters.
        // The class contains functions that take the proper informational variables and then construct
        // the appropriate exception with an error string obtained from the resource framework.
        // The exception is then returned to the caller, so that the caller may then throw from its
        // location so that the catcher of the exception will have the appropriate call stack.
        // This class is used so that there will be compile time checking of error messages.

//         internal static Task<T> CreatedTaskWithException<T>(Exception ex)
//         {
//             return Task.FromException<T>(ex);
//         }

//         internal static Task<T> CreatedTaskWithCancellation<T>()
//         {
//             TaskCompletionSource<T> completion = new TaskCompletionSource<T>();
//             completion.SetCanceled();
//             return completion.Task;
//         }

//         // NOTE: Initializing a Task in SQL CLR requires the "UNSAFE" permission set (http://msdn.microsoft.com/en-us/library/ms172338.aspx)
//         // Therefore we are lazily initializing these Tasks to avoid forcing customers to use the "UNSAFE" set when they are actually using no Async features
//         static private Task<bool> s_trueTask = null;
//         internal static Task<bool> TrueTask
//         {
//             get
//             {
//                 if (s_trueTask == null)
//                 {
//                     s_trueTask = Task.FromResult<bool>(true);
//                 }
//                 return s_trueTask;
//             }
//         }

//         static private Task<bool> s_falseTask = null;
//         internal static Task<bool> FalseTask
//         {
//             get
//             {
//                 if (s_falseTask == null)
//                 {
//                     s_falseTask = Task.FromResult<bool>(false);
//                 }
//                 return s_falseTask;
//             }
//         }

        internal static Exception ExceptionWithStackTrace(Exception e)
        {
            try
            {
                throw e;
            }
            catch (Exception caught)
            {
                return caught;
            }
        }

        //
        // COM+ exceptions
        //
        internal static ArgumentException Argument(string error)                   => new ArgumentException(error);
        internal static ArgumentException Argument(string error, Exception inner)  => new ArgumentException(error, inner);
        internal static ArgumentException Argument(string error, string parameter) => new ArgumentException(error, parameter);

        internal static ArgumentNullException ArgumentNull(string parameter) => new ArgumentNullException(parameter);
        internal static ArgumentNullException ArgumentNull(string parameter, string error) => new ArgumentNullException(parameter, error);

//         internal static ArgumentOutOfRangeException ArgumentOutOfRange(string parameterName)
//         {
//             ArgumentOutOfRangeException e = new ArgumentOutOfRangeException(parameterName);
//             return e;
//         }
//         internal static ArgumentOutOfRangeException ArgumentOutOfRange(string message, string parameterName)
//         {
//             ArgumentOutOfRangeException e = new ArgumentOutOfRangeException(parameterName, message);
//             return e;
//         }
//         internal static IndexOutOfRangeException IndexOutOfRange(int value)
//         {
//             IndexOutOfRangeException e = new IndexOutOfRangeException(value.ToString(CultureInfo.InvariantCulture));
//             return e;
//         }
//         internal static IndexOutOfRangeException IndexOutOfRange(string error)
//         {
//             IndexOutOfRangeException e = new IndexOutOfRangeException(error);
//             return e;
//         }
//         internal static IndexOutOfRangeException IndexOutOfRange()
//         {
//             IndexOutOfRangeException e = new IndexOutOfRangeException();
//             return e;
//         }
//         internal static InvalidCastException InvalidCast(string error)
//         {
//             return InvalidCast(error, null);
//         }
//         internal static InvalidCastException InvalidCast(string error, Exception inner)
//         {
//             InvalidCastException e = new InvalidCastException(error, inner);
//             return e;
//         }
        internal static InvalidOperationException InvalidOperation(string error) => new InvalidOperationException(error);
        internal static InvalidOperationException InvalidOperation(string error, Exception inner)
        {
            return new InvalidOperationException(error, inner);
        }

        internal static TimeoutException TimeoutException(string error) => new TimeoutException(error);

        internal static NotSupportedException NotSupported()             => new NotSupportedException();
        internal static NotSupportedException NotSupported(string error) => new NotSupportedException(error);

//         internal static OverflowException Overflow(string error)
//         {
//             return Overflow(error, null);
//         }
//         internal static OverflowException Overflow(string error, Exception inner)
//         {
//             OverflowException e = new OverflowException(error, inner);
//             return e;
//         }

//         internal static PlatformNotSupportedException DbTypeNotSupported(string dbType)
//         {
//             PlatformNotSupportedException e = new PlatformNotSupportedException(Res.GetString(Res.SQL_DbTypeNotSupportedOnThisPlatform, dbType));
//             return e;
//         }
//         internal static InvalidCastException InvalidCast()
//         {
//             InvalidCastException e = new InvalidCastException();
//             return e;
//         }
//         internal static IOException IO(string error)
//         {
//             IOException e = new IOException(error);
//             return e;
//         }
//         internal static IOException IO(string error, Exception inner)
//         {
//             IOException e = new IOException(error, inner);
//             return e;
//         }
//         internal static InvalidOperationException DataAdapter(string error)
//         {
//             return InvalidOperation(error);
//         }
        private static InvalidOperationException Provider(string error)
        {
            return InvalidOperation(error);
        }
//         internal static ObjectDisposedException ObjectDisposed(object instance)
//         {
//             ObjectDisposedException e = new ObjectDisposedException(instance.GetType().Name);
//             return e;
//         }

//         internal static InvalidOperationException MethodCalledTwice(string method)
//         {
//             InvalidOperationException e = new InvalidOperationException(Res.GetString(Res.ADP_CalledTwice, method));
//             return e;
//         }
//         internal static ArgumentException InvalidMultipartName(string property, string value)
//         {
//             ArgumentException e = new ArgumentException(Res.GetString(Res.ADP_InvalidMultipartName, Res.GetString(property), value));
//             return e;
//         }
//         internal static ArgumentException InvalidMultipartNameIncorrectUsageOfQuotes(string property, string value)
//         {
//             ArgumentException e = new ArgumentException(Res.GetString(Res.ADP_InvalidMultipartNameQuoteUsage, Res.GetString(property), value));
//             return e;
//         }
//         internal static ArgumentException InvalidMultipartNameToManyParts(string property, string value, int limit)
//         {
//             ArgumentException e = new ArgumentException(Res.GetString(Res.ADP_InvalidMultipartNameToManyParts, Res.GetString(property), value, limit));
//             return e;
//         }

        internal static void CheckArgumentNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw ArgumentNull(parameterName);
            }
        }

        internal static bool IsCatchableExceptionType(Exception e)
        {
            return !((e is NullReferenceException) || (e is SecurityException));
        }

//         internal static bool IsCatchableOrSecurityExceptionType(Exception e)
//         {
//             // a 'catchable' exception is defined by what it is not.
//             // since IsCatchableExceptionType defined SecurityException as not 'catchable'
//             // this method will return true for SecurityException has being catchable.

//             // the other way to write this method is, but then SecurityException is checked twice
//             // return ((e is SecurityException) || IsCatchableExceptionType(e));

//             Debug.Assert(e != null, "Unexpected null exception!");
//             // Most of the exception types above will cause the process to fail fast
//             // So they are no longer needed in this check
//             return !(e is NullReferenceException);
//         }

//         // Invalid Enumeration

//         internal static ArgumentOutOfRangeException InvalidEnumerationValue(Type type, int value)
//         {
//             return ADP.ArgumentOutOfRange(Res.GetString(Res.ADP_InvalidEnumerationValue, type.Name, value.ToString(System.Globalization.CultureInfo.InvariantCulture)), type.Name);
//         }

        //
        // DbConnectionOptions, DataAccess
        //
        internal static ArgumentException InvalidConnectionStringArgument()
        {
            return Argument("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
        }

        internal static ArgumentException InvalidPacketSizeValue(int value)
        {
            return ADP.Argument($"'Packet Size' value of {value} is not valid. The value should be an integer >= 512 and <= 32767.");
        }

        internal static ArgumentException InvalidConnectRetryCountValue(int value)
        {
            return ADP.Argument($"'Connection Retry Count' value of {value} is not valid. The value should be an integer >= 0 and <= 255.");
        }
        internal static ArgumentException InvalidConnectRetryIntervalValue(int value)
        {
            return ADP.Argument($"'Connection Retry Interval' value of {value} is not valid. The value should be an integer >= 1 and <= 60.");
        }

        internal static ArgumentException InvalidConnectTimeoutValue(int value)
        {
            return ADP.Argument($"'Connection Timeout' value of {value} is not valid. The value should be an integer >= 0 and <= 2147483647.");
        }
        internal static ArgumentException InvalidCommandTimeoutValue(int value)
        {
            return ADP.Argument($"'Command Timeout' value of {value} is not valid. The value should be an integer >= 0 and <= 2147483647.");
        }
        internal static ArgumentException InvalidLockTimeoutValue(int value)
        {
            return ADP.Argument($"'Lock Timeout' value of {value} is not valid. The value should be an integer >= 0 and <= 2147483647.");
        }

        internal static ArgumentException InvalidMinMaxPoolSizeValues()
        {
            return ADP.Argument("Invalid min or max pool size values, min pool size cannot be greater than the max pool size.");
            // return ADP.Argument(Res.GetString(Res.ADP_InvalidMinMaxPoolSizeValues));
        }

//         internal static ArgumentException KeywordNotSupported(string keyword)
//         {
//             return Argument(Res.GetString(Res.ADP_KeywordNotSupported, keyword));
//         }
//         internal static ArgumentException ConvertFailed(Type fromType, Type toType, Exception innerException)
//         {
//             return ADP.Argument(Res.GetString(Res.SqlConvert_ConvertFailed, fromType.FullName, toType.FullName), innerException);
//         }

//         internal static Exception InvalidConnectionOptionValue(string key)
//         {
//             return InvalidConnectionOptionValue(key, null);
//         }
//         internal static Exception InvalidConnectionOptionValueLength(string key, int limit)
//         {
//             return Argument(Res.GetString(Res.ADP_InvalidConnectionOptionValueLength, key, limit));
//         }
//         internal static Exception InvalidConnectionOptionValue(string key, Exception inner)
//         {
//             return Argument(Res.GetString(Res.ADP_InvalidConnectionOptionValue, key), inner);
//         }
//         internal static Exception MissingConnectionOptionValue(string key, string requiredAdditionalKey)
//         {
//             return Argument(Res.GetString(Res.ADP_MissingConnectionOptionValue, key, requiredAdditionalKey));
//         }
//         internal static Exception WrongType(Type got, Type expected)
//         {
//             return Argument(Res.GetString(Res.SQL_WrongType, got.ToString(), expected.ToString()));
//         }

        //
        // DbConnection
        //
        internal static InvalidOperationException NoConnectionString()
        {
            return InvalidOperation("The ConnectionString property has not been initialized.");
        }
        internal static InvalidOperationException NullEmptyTransactionName()
        {
            return InvalidOperation("Invalid transaction or invalid name for a point at which to save within the transaction.");
        }

        internal static Exception MethodNotImplemented([CallerMemberName] string methodName = "")
        {
            return new NotImplementedException();
            // NotImplemented.ByDesignWithMessage(methodName);
        }

        static private string ConnectionStateMsg(ConnectionState state)
        {
            switch (state)
            {
            case ConnectionState.Closed:
            case ConnectionState.Connecting | ConnectionState.Broken:
                return "The connection's current state is closed."; 
                // return Res.GetString(Res.ADP_ConnectionStateMsg_Closed);

            case ConnectionState.Connecting:
                return "The connection's current state is connecting.";
                // return Res.GetString(Res.ADP_ConnectionStateMsg_Connecting);
                
            case ConnectionState.Open:
                return "The connection's current state is open.";
                // return Res.GetString(Res.ADP_ConnectionStateMsg_Open);

            case ConnectionState.Open | ConnectionState.Executing:
                return "The connection's current state is executing."; 
                // return Res.GetString(Res.ADP_ConnectionStateMsg_OpenExecuting);

            case ConnectionState.Open | ConnectionState.Fetching:
                return "The connection's current state is fetching.";
                // return Res.GetString(Res.ADP_ConnectionStateMsg_OpenFetching);

            default:
                return String.Format("The connection's current state: {0}.", state.ToString());
                // return Res.GetString(Res.ADP_ConnectionStateMsg, state.ToString());
            }
        }

        //
        // DbConnectionPool and related
        //
        internal static Exception PooledOpenTimeout()
        {
            return InvalidOperation("Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.  This may have occurred because all pooled connections were in use and max pool size was reached.");
            // return ADP.InvalidOperation(Res.GetString(Res.ADP_PooledOpenTimeout));
        }

        internal static Exception NonPooledOpenTimeout()
        {
            return TimeoutException("Timeout attempting to open the connection.  The time period elapsed prior to attempting to open the connection has been exceeded.  This may have occurred because of too many simultaneous non-pooled connection attempts.");
            // return ADP.InvalidOperation(Res.GetString(Res.ADP_NonPooledOpenTimeout));
        }

        //
        // Generic Data Provider Collection
        //
//         internal static ArgumentException CollectionRemoveInvalidObject(Type itemType, ICollection collection)
//         {
//             return Argument(Res.GetString(Res.ADP_CollectionRemoveInvalidObject, itemType.Name, collection.GetType().Name));
//         }
//         internal static ArgumentNullException CollectionNullValue(string parameter, Type collection, Type itemType)
//         {
//             return ArgumentNull(parameter, Res.GetString(Res.ADP_CollectionNullValue, collection.Name, itemType.Name));
//         }
//         internal static IndexOutOfRangeException CollectionIndexInt32(int index, Type collection, int count)
//         {
//             return IndexOutOfRange(Res.GetString(Res.ADP_CollectionIndexInt32, index.ToString(CultureInfo.InvariantCulture), collection.Name, count.ToString(CultureInfo.InvariantCulture)));
//         }
//         internal static IndexOutOfRangeException CollectionIndexString(Type itemType, string propertyName, string propertyValue, Type collection)
//         {
//             return IndexOutOfRange(Res.GetString(Res.ADP_CollectionIndexString, itemType.Name, propertyName, propertyValue, collection.Name));
//         }
//         internal static InvalidCastException CollectionInvalidType(Type collection, Type itemType, object invalidValue)
//         {
//             return InvalidCast(Res.GetString(Res.ADP_CollectionInvalidType, collection.Name, itemType.Name, invalidValue.GetType().Name));
//         }
//         internal static ArgumentException ParametersIsNotParent(Type parameterType, ICollection collection)
//         {
//             return Argument(Res.GetString(Res.ADP_CollectionIsNotParent, parameterType.Name, collection.GetType().Name));
//         }
//         internal static ArgumentException ParametersIsParent(Type parameterType, ICollection collection)
//         {
//             return Argument(Res.GetString(Res.ADP_CollectionIsNotParent, parameterType.Name, collection.GetType().Name));
//         }

        //
        // : IDbCommand
        //
        internal static InvalidOperationException TransactionConnectionMismatch()
        {
            return Provider("The transaction is either not associated with the current connection or has been completed.");
            // return Provider(Res.GetString(Res.ADP_TransactionConnectionMismatch));
        }
        internal static InvalidOperationException TransactionRequired(string method)
        {
            return Provider($"{method} requires the command to have a transaction when the connection assigned to the command is in a pending local transaction. The Transaction property of the command has not been initialized.");
            // return Provider(Res.GetString(Res.ADP_TransactionRequired, method));
        }

        internal static Exception CommandTextRequired(string method)
        {
            return InvalidOperation("The command text for this Command has not been set.");
            // return InvalidOperation(Res.GetString(Res.ADP_CommandTextRequired, method));
        }
        internal static InvalidOperationException ConnectionRequired(string method)
        {
            return InvalidOperation($"{method} requires an available Connection.");
            // return InvalidOperation(Res.GetString(Res.ADP_ConnectionRequired, method));
        }
        internal static InvalidOperationException OpenConnectionRequired(string method, ConnectionState state)
        {
            return InvalidOperation($"{method} requires an open and available Connection. The connection's current state is {state}.");
            // return InvalidOperation(Res.GetString(Res.ADP_OpenConnectionRequired, method, ADP.ConnectionStateMsg(state)));
        }

        internal static Exception OpenReaderExists()
        {
            return OpenReaderExists(null);
        }
        internal static Exception OpenReaderExists(Exception e)
        {
            return InvalidOperation("There is already an open DataReader associated with this Command which must be closed first.");
            // return InvalidOperation(Res.GetString(Res.ADP_OpenReaderExists), e);
        }

//         internal static Exception InvalidCommandTimeout(int value, [CallerMemberName] string property = "")
//         {
//             return Argument(Res.GetString(Res.ADP_InvalidCommandTimeout, value.ToString(CultureInfo.InvariantCulture)), property);
//         }
//         internal static Exception UninitializedParameterSize(int index, Type dataType)
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_UninitializedParameterSize, index.ToString(CultureInfo.InvariantCulture), dataType.Name));
//         }
//         internal static Exception PrepareParameterType(DbCommand cmd)
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_PrepareParameterType, cmd.GetType().Name));
//         }
//         internal static Exception PrepareParameterSize(DbCommand cmd)
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_PrepareParameterSize, cmd.GetType().Name));
//         }
//         internal static Exception PrepareParameterScale(DbCommand cmd, string type)
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_PrepareParameterScale, cmd.GetType().Name, type));
//         }
//         internal static Exception MismatchedAsyncResult(string expectedMethod, string gotMethod)
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_MismatchedAsyncResult, expectedMethod, gotMethod));
//         }

        //
        // : ConnectionUtil
        //
//         internal static Exception ConnectionIsDisabled(Exception InnerException)
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_ConnectionIsDisabled), InnerException);
//         }

        internal static Exception ClosedConnectionError()
        {
            return InvalidOperation("Invalid operation. The connection is closed.");
            // return InvalidOperation(Res.GetString(Res.ADP_ClosedConnectionError));
        }

        internal static Exception ConnectionAlreadyOpen(ConnectionState state)
        {
            return InvalidOperation("Connection already open, or is broken.");
            //return InvalidOperation(Res.GetString(Res.ADP_ConnectionAlreadyOpen, ADP.ConnectionStateMsg(state)));
        }

        internal static Exception OpenConnectionPropertySet(string property, ConnectionState state)
        {
            return InvalidOperation(String.Format("Not allowed to change the '{0}' property. {1}", property, ConnectionStateMsg(state)));
            // return InvalidOperation(Res.GetString(Res.ADP_OpenConnectionPropertySet, property, ADP.ConnectionStateMsg(state)));
        }

        internal static Exception EmptyDatabaseName()
        {
            return Argument("Database name is not valid.");
            // return Argument(Res.GetString(Res.ADP_EmptyDatabaseName));
        }

        internal enum ConnectionError
        {
            BeginGetConnectionReturnsNull,
            GetConnectionReturnsNull,
            ConnectionOptionsMissing,
            CouldNotSwitchToClosedPreviouslyOpenedState,
        }

        internal enum InternalErrorCode
        {
            UnpooledObjectHasOwner                                 =  0,
            UnpooledObjectHasWrongOwner                            =  1,
            PushingObjectSecondTime                                =  2,
            PooledObjectHasOwner                                   =  3,
            PooledObjectInPoolMoreThanOnce                         =  4,
            CreateObjectReturnedNull                               =  5,
            NewObjectCannotBePooled                                =  6,
            NonPooledObjectUsedMoreThanOnce                        =  7,
            AttemptingToPoolOnRestrictedToken                      =  8,
            AttemptingToConstructReferenceCollectionOnStaticObject = 12,
            CreateReferenceCollectionReturnedNull                  = 14,
            PooledObjectWithoutPool                                = 15,
            UnexpectedWaitAnyResult                                = 16,
            SynchronousConnectReturnedPending                      = 17,
            CompletedConnectReturnedPending                        = 18
        }

        internal static Exception InternalConnectionError(ConnectionError internalError)
        {
            return InvalidOperation("Internal DbConnection Error: {(int)internalError}");
            // return InvalidOperation(Res.GetString(Res.ADP_InternalConnectionError, (int)internalError));
        }

        internal static Exception InternalError(InternalErrorCode internalError)
        {
            return InvalidOperation($"Internal .Net Framework Data Provider error {(int)internalError}.");
            // return InvalidOperation(Res.GetString(Res.ADP_InternalProviderError, (int)internalError));
        }

        //
        // : DataReader
        //
        internal static Exception InvalidRead()
        {
             return InvalidOperation("Invalid attempt to read when no data is present.");
        }

//         internal static Exception NonSeqByteAccess(long badIndex, long currIndex, string method)
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_NonSeqByteAccess, badIndex.ToString(CultureInfo.InvariantCulture), currIndex.ToString(CultureInfo.InvariantCulture), method));
//         }
//         internal static Exception NegativeParameter(string parameterName)
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_NegativeParameter, parameterName));
//         }
//         internal static Exception InvalidSeekOrigin(string parameterName)
//         {
//             return ArgumentOutOfRange(Res.GetString(Res.ADP_InvalidSeekOrigin), parameterName);
//         }

//         internal static Exception DataReaderClosed([CallerMemberName] string method = "")
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_DataReaderClosed, method));
//         }
//         internal static ArgumentOutOfRangeException InvalidSourceBufferIndex(int maxLen, long srcOffset, string parameterName)
//         {
//             return ArgumentOutOfRange(Res.GetString(Res.ADP_InvalidSourceBufferIndex, maxLen.ToString(CultureInfo.InvariantCulture), srcOffset.ToString(CultureInfo.InvariantCulture)), parameterName);
//         }
//         internal static ArgumentOutOfRangeException InvalidDestinationBufferIndex(int maxLen, int dstOffset, string parameterName)
//         {
//             return ArgumentOutOfRange(Res.GetString(Res.ADP_InvalidDestinationBufferIndex, maxLen.ToString(CultureInfo.InvariantCulture), dstOffset.ToString(CultureInfo.InvariantCulture)), parameterName);
//         }
//         internal static IndexOutOfRangeException InvalidBufferSizeOrIndex(int numBytes, int bufferIndex)
//         {
//             return IndexOutOfRange(Res.GetString(Res.SQL_InvalidBufferSizeOrIndex, numBytes.ToString(CultureInfo.InvariantCulture), bufferIndex.ToString(CultureInfo.InvariantCulture)));
//         }
//         internal static Exception InvalidDataLength(long length)
//         {
//             return IndexOutOfRange(Res.GetString(Res.SQL_InvalidDataLength, length.ToString(CultureInfo.InvariantCulture)));
//         }
//         internal static InvalidOperationException AsyncOperationPending()
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_PendingAsyncOperation));
//         }

        //
        // : Stream
        //
//         internal static Exception StreamClosed([CallerMemberName] string method = "")
//         {
//             return InvalidOperation(Res.GetString(Res.ADP_StreamClosed, method));
//         }
//         internal static IOException ErrorReadingFromStream(Exception internalException)
//         {
//             return IO(Res.GetString(Res.SqlMisc_StreamErrorMessage), internalException);
//         }

//         internal static ArgumentException InvalidDataType(string typeName)
//         {
//             return Argument(Res.GetString(Res.ADP_InvalidDataType, typeName));
//         }
//         internal static ArgumentException UnknownDataType(Type dataType)
//         {
//             return Argument(Res.GetString(Res.ADP_UnknownDataType, dataType.FullName));
//         }
//         internal static ArgumentException DbTypeNotSupported(System.Data.DbType type, Type enumtype)
//         {
//             return Argument(Res.GetString(Res.ADP_DbTypeNotSupported, type.ToString(), enumtype.Name));
//         }
//         internal static ArgumentException InvalidOffsetValue(int value)
//         {
//             return Argument(Res.GetString(Res.ADP_InvalidOffsetValue, value.ToString(CultureInfo.InvariantCulture)));
//         }
//         internal static ArgumentException InvalidSizeValue(int value)
//         {
//             return Argument(Res.GetString(Res.ADP_InvalidSizeValue, value.ToString(CultureInfo.InvariantCulture)));
//         }
//         internal static ArgumentException ParameterValueOutOfRange(Decimal value)
//         {
//             return ADP.Argument(Res.GetString(Res.ADP_ParameterValueOutOfRange, value.ToString((IFormatProvider)null)));
//         }
//         internal static ArgumentException ParameterValueOutOfRange(PgDecimal value)
//         {
//             return ADP.Argument(Res.GetString(Res.ADP_ParameterValueOutOfRange, value.ToString()));
//         }

//         internal static ArgumentException VersionDoesNotSupportDataType(string typeName)
//         {
//             return Argument(Res.GetString(Res.ADP_VersionDoesNotSupportDataType, typeName));
//         }
//         internal static Exception ParameterConversionFailed(object value, Type destType, Exception inner)
//         {
//             Debug.Assert(null != value, "null value on conversion failure");
//             Debug.Assert(null != inner, "null inner on conversion failure");

//             Exception e;
//             string message = Res.GetString(Res.ADP_ParameterConversionFailed, value.GetType().Name, destType.Name);
//             if (inner is ArgumentException)
//             {
//                 e = new ArgumentException(message, inner);
//             }
//             else if (inner is FormatException)
//             {
//                 e = new FormatException(message, inner);
//             }
//             else if (inner is InvalidCastException)
//             {
//                 e = new InvalidCastException(message, inner);
//             }
//             else if (inner is OverflowException)
//             {
//                 e = new OverflowException(message, inner);
//             }
//             else
//             {
//                 e = inner;
//             }
//             return e;
//         }

        //
        // : IDataParameterCollection
        //
//         internal static Exception ParametersMappingIndex(int index, DbParameterCollection collection)
//         {
//             return CollectionIndexInt32(index, collection.GetType(), collection.Count);
//         }
//         internal static Exception ParametersSourceIndex(string parameterName, DbParameterCollection collection, Type parameterType)
//         {
//             return CollectionIndexString(parameterType, ADP.ParameterName, parameterName, collection.GetType());
//         }
//         internal static Exception ParameterNull(string parameter, DbParameterCollection collection, Type parameterType)
//         {
//             return CollectionNullValue(parameter, collection.GetType(), parameterType);
//         }
//         internal static Exception InvalidParameterType(DbParameterCollection collection, Type parameterType, object invalidValue)
//         {
//             return CollectionInvalidType(collection.GetType(), parameterType, invalidValue);
//         }

        //
        // : IDbTransaction
        //
        internal static Exception ParallelTransactionsNotSupported(DbConnection obj)
        {
            return InvalidOperation("A transaction is currently active. Parallel transactions are not supported.");
        }
        internal static Exception TransactionZombied(DbTransaction obj)
        {
            return InvalidOperation($"This {obj.GetType().Name} has completed; it is no longer usable.");
        }

        //
        // : Timers
        //

        internal static void TimerCurrent(out long ticks) 
        {
            ticks = DateTime.UtcNow.ToFileTimeUtc();
        }

        internal static long TimerCurrent()                => DateTime.UtcNow.ToFileTimeUtc();
        internal static long TimerFromSeconds(int seconds) => checked((long)seconds * TimeSpan.TicksPerSecond);

        internal static long TimerFromMilliseconds(long milliseconds)
        {
            return checked(milliseconds * TimeSpan.TicksPerMillisecond);
        }

        internal static bool TimerHasExpired(long timerExpire) => TimerCurrent() > timerExpire;
        internal static long TimerRemaining(long timerExpire)  => checked(timerExpire - TimerCurrent());

        internal static long TimerRemainingMilliseconds(long timerExpire) => TimerToMilliseconds(TimerRemaining(timerExpire));
        internal static long TimerRemainingSeconds(long timerExpire)      => TimerToSeconds(TimerRemaining(timerExpire));

        internal static long TimerToMilliseconds(long timerValue) => timerValue / TimeSpan.TicksPerMillisecond;
        private static long TimerToSeconds(long timerValue)       => timerValue / TimeSpan.TicksPerSecond;

        //
        // : Misc
        //

//         internal static string BuildQuotedString(string quotePrefix, string quoteSuffix, string unQuotedString)
//         {
//             StringBuilder resultString = new StringBuilder();
//             if (string.IsNullOrEmpty(quotePrefix) == false)
//             {
//                 resultString.Append(quotePrefix);
//             }

//             // Assuming that the suffix is escaped by doubling it. i.e. foo"bar becomes "foo""bar".
//             if (string.IsNullOrEmpty(quoteSuffix) == false)
//             {
//                 resultString.Append(unQuotedString.Replace(quoteSuffix, quoteSuffix + quoteSuffix));
//                 resultString.Append(quoteSuffix);
//             }
//             else
//             {
//                 resultString.Append(unQuotedString);
//             }

//             return resultString.ToString();
//         }

//         static private int GenerateUniqueName(Dictionary<string, int> hash, ref string columnName, int index, int uniqueIndex)
//         {
//             for (; ; ++uniqueIndex)
//             {
//                 string uniqueName = columnName + uniqueIndex.ToString(CultureInfo.InvariantCulture);
//                 string lowerName = uniqueName.ToLowerInvariant();
//                 if (!hash.ContainsKey(lowerName))
//                 {
//                     columnName = uniqueName;
//                     hash.Add(lowerName, index);
//                     break;
//                 }
//             }
//             return uniqueIndex;
//         }

//         internal static bool IsDirection(DbParameter value, ParameterDirection condition) => (condition == (condition & value.Direction));

        internal static bool IsNull(object value)
        {
            if ((value == null) || (DBNull.Value == value))
            {
                return true;
            }
            INullable nullable = (value as INullable);
            return (nullable != null && nullable.IsNull);
        }

//         internal static void IsNullOrSqlType(object value, out bool isNull, out bool isSqlType)
//         {
//             if ((value == null) || (value == DBNull.Value))
//             {
//                 isNull = true;
//                 isSqlType = false;
//             }
//             else
//             {
//                 INullable nullable = (value as INullable);
//                 if (nullable != null)
//                 {
//                     isNull = nullable.IsNull;
//                     // Duplicated from DataStorage.cs
//                     // For back-compat, SqlXml is not in this list
//                     isSqlType = ((value is SqlBinary) ||
//                                 (value is SqlBoolean) ||
//                                 (value is SqlByte) ||
//                                 (value is SqlBytes) ||
//                                 (value is SqlChars) ||
//                                 (value is SqlDateTime) ||
//                                 (value is SqlDecimal) ||
//                                 (value is SqlDouble) ||
//                                 (value is SqlGuid) ||
//                                 (value is SqlInt16) ||
//                                 (value is SqlInt32) ||
//                                 (value is SqlInt64) ||
//                                 (value is SqlMoney) ||
//                                 (value is SqlSingle) ||
//                                 (value is SqlString));
//                 }
//                 else
//                 {
//                     isNull = false;
//                     isSqlType = false;
//                 }
//             }
//         }
    }
}
