using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;

namespace Caasiope.P2P
{
    public class PeerManager
    {
        public Action<PeerSession> OnPeerConnected;
        public Action<PeerSession> OnPeerDisconnected;
        public Action<PeerSession, byte, byte[]> OnReceived;

        private readonly MonitorLocker locker = new MonitorLocker();
        private readonly Dictionary<PersonaThumbprint, PeerSession> sessionsById = new Dictionary<PersonaThumbprint, PeerSession>();
        private int channels;

        public void Initialize(int channels)
        {
            this.channels = channels;
        }

        public PeerSession GetOrCreatePeerByPersona(Persona persona, IPEndPoint server)
        {
            using (locker.CreateLock())
            {
                return sessionsById.GetOrCreate(persona.Thumbprint, () => CreatePeer(server, persona));
            }
        }

        private PeerSession CreatePeer(IPEndPoint server, Persona persona)
        {
            var peerSession = new PeerSession(new Peer(new Node(server), persona), channels);

            peerSession.OnConnected += () => OnPeerConnected(peerSession);
            peerSession.OnClosed += () => OnPeerDisconnected(peerSession);
            peerSession.OnReceived += (channel, data) => OnReceived(peerSession, channel, data);

            return peerSession;
        }

        public void Broadcast(IChannel channel, byte[] data)
        {
            // TODO too slow
            using (locker.CreateLock())
            {
                foreach (var peer in sessionsById.Values)
                {
                    peer.Send(channel, data);
                }
            }
        }

        public List<PeerSession> GetAllConnected()
        {
            // TODO too slow
            var result = new List<PeerSession>();
            using (locker.CreateLock())
            {
                result.AddRange(sessionsById.Values.Where(_ => _.PeerState == PeerState.Connected));
            }

            return result;
        }
    }
}