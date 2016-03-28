namespace System.Data.Common
{
    internal static class CommandBehaviorExtensions
    {
        internal static bool HasBehavior(this CommandBehavior behavior, CommandBehavior behaviorToCheck)
        {
            return ((behavior & behaviorToCheck) == behaviorToCheck);
        }
    }
}
