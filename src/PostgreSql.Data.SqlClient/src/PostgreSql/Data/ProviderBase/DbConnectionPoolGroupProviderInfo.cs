// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// --------------------------------------------------------------------------------------------------
// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Data.ProviderBase
{
    internal sealed class DbConnectionPoolGroupProviderInfo
    {
        private DbConnectionPoolGroup _poolGroup;

        internal DbConnectionPoolGroup PoolGroup
        {
            get => _poolGroup;
            set => _poolGroup = value;
        }

        internal DbConnectionPoolGroupProviderInfo()
        {            
        }
    }
}
