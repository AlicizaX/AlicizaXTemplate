using AlicizaX;

namespace Unity.Startup.Procedure
{
    internal enum LaunchState
    {
        Checking,
        NetworkUnavailable,
        Ready
    }

    internal readonly struct LaunchStateChangedEvent : IPayloadEventArgs
    {
        public readonly LaunchState State;

        public LaunchStateChangedEvent(LaunchState state) => State = state;
    }

    internal readonly struct LaunchStartRequestedEvent : IEmptyEventArgs { }
}
