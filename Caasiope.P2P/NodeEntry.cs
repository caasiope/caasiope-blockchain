using System;
using System.Diagnostics;

namespace Caasiope.P2P
{
    // refers to a known node entry
    public class NodeEntry
    {
        public readonly Node Node;
        private int count; // TODO synchronize ?

        public DateTime DisconnectedTime { get; private set; } = DateTime.MinValue;
        public int Score { get; private set; }

        public bool IsAvailable => count == 0;

        public NodeEntry(Node node)
        {
            Debug.Assert(node.HasServer);
            Node = node;
        }

        public void Acquire()
        {
            count++;
            Debug.Assert(count <= 1);
            // Console.WriteLine($"Acquire Node {Node.EndPoint} Count : {count}");
        }

        public void Release(DisconnectReason reason)
        {
            DisconnectedTime = DateTime.Now;
            Score += GetScoreModifier(reason);
            Debug.Assert(count > 0);
            // TODO Here based on the reason we decide, if we need to release the node or not. It doesn't look like a good place for such decision
            count -= GetReleaseSubtrahend(reason);
            // Console.WriteLine($"Node Released : {count == 0}. {Node.EndPoint} Count : {count} Reason {reason}");
        }

        private int GetReleaseSubtrahend(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.CannotSetSession:
                case DisconnectReason.PeerAlreadyConnected:
                    return 0;
                default:
                    return 1;
            }
        }

        private int GetScoreModifier(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.ConnectedToSelf:
                case DisconnectReason.ClientWithSameThumbprint:
                    return -1;

                case DisconnectReason.CannotConnectToNode: // the peer is down
                    return 0;
                case DisconnectReason.PeerAlreadyConnected:
                    return 0;
                case DisconnectReason.AuthenticationFailed: // something wrong with the certificate
                    return 0;
                case DisconnectReason.NoThreadAvailable: 
                    return 0;
                case DisconnectReason.InitializationFailed: // something went wrong during the Authentication
                    return 0;
                case DisconnectReason.CannotSetSession: // the node has a session already
                    return 0;
                case DisconnectReason.ErrorWhenWrite: // network problem / the peer disconnected us / peer is down now
                    return 1;
                case DisconnectReason.ErrorInReadLoop: // network problem / the peer disconnected us / peer is down now
                    return 1;
                case DisconnectReason.ErrorInSendLoop: // network problem / the peer disconnected us / peer is down now
                    return 1;
                case DisconnectReason.TooManyPeers:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}