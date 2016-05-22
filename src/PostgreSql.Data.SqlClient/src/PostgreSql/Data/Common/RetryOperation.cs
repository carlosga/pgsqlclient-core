// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;

namespace System.Data.Common
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/dn589788.aspx
    /// </summary>
    internal sealed class RetryOperation
    {
        private readonly int  _retryCount;
        private readonly int  _retryInterval;
        private readonly bool _applyTransientFaultHandling;

        internal RetryOperation(int retryCount, int retryInterval, bool applyTransientFaultHandling = false)
        {
            _retryCount                  = retryCount;
            _retryInterval               = retryInterval;
            _applyTransientFaultHandling = applyTransientFaultHandling;
        }

        internal void Execute(Action action, Func<Exception, bool> transientFaultHandler = null)
        {
            if (action == null)
            {
                throw ADP.ArgumentNull("action");
            }
            if (_applyTransientFaultHandling && transientFaultHandler == null)
            {
                throw ADP.ArgumentNull("isTransient");
            }

            int currentRetry = 0;

            for (;;)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    if (++currentRetry > _retryCount || !_applyTransientFaultHandling || !transientFaultHandler(ex))
                    {
                        // If this is not a transient error or we should not retry re-throw the exception.
                        throw;
                    }
                }

                Thread.Sleep(_retryInterval);
            }
        }
    }
}
