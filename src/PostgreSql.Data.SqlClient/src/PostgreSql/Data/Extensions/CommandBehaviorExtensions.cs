// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Data
{
    internal static class CommandBehaviorExtensions
    {
        internal static bool HasBehavior(this CommandBehavior behavior, CommandBehavior behaviorToCheck)
        {
            return ((behavior & behaviorToCheck) == behaviorToCheck);
        }
    }
}
