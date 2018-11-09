namespace Caasiope.P2P
{
    public enum DisconnectReason
    {
        AuthenticationFailed,
        NoThreadAvailable,
        InitializationFailed,
        ErrorInSendLoop,
        TooManyPeers,
        ConnectedToSelf,
        PeerAlreadyConnected,
        CannotSetSession,
        ClientWithSameThumbprint,
        CannotConnectToNode,
        ErrorWhenWrite,
        ErrorInReadLoop
    }
}