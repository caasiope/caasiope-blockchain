using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;

namespace Caasiope.P2P
{
    public interface IPeer
    {
        string ID { get; }
        IPEndPoint IP { get; }
        long Height { get; set; }
        int Ping { get; }
        DateTime LastPong { get; }
    }

    public class PersonaThumbprint
    {
        private readonly string id;

        public PersonaThumbprint(X509Certificate2 certificate)
        {
            id = certificate.Thumbprint;
        }

        public PersonaThumbprint(string thumbprint)
        {
            id = thumbprint;
        }

        public override string ToString()
        {
            return id;
        }

        public override bool Equals(object obj)
        {
            return obj is PersonaThumbprint other && other.id == id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }

    public class Persona
    {
        public readonly PersonaThumbprint Thumbprint;
        public readonly X509Certificate2 Certificate;

        public Persona(X509Certificate2 certificate)
        {
            Thumbprint = new PersonaThumbprint(certificate);
            Certificate = certificate;
        }
    }

    // this is used to isolate the tcp session and the io threads from the peer session

    public class Peer
    {
        public readonly Node Node;
        public Persona Persona;

        public Peer(Node node)
        {
            Node = node;
        }

        public Peer(Node node, Persona persona)
        {
            Node = node;
            Persona = persona;
        }
    }

    public enum PeerState
    {
        Connected = 1,
        Connecting = 2,
        Disconnected = 3,
    }

    public class PeerSession : ISession, IPeer, ISessionListener
    {
        // peers that know their server url should put it in the origin field
        // public readonly string ServerUrl;
        public PeerState PeerState
        {
            get
            {
                if (nodeSession != null && nodeSession.GetConnectionState() == ConnectionState.Connected)
                    return PeerState.Connected;

                return PeerState.Disconnected;
            }
        }

        // this peer can be updated when we get more informations
        public Peer Peer { get; }

        private NodeSession nodeSession;
        private readonly MonitorLocker locker = new MonitorLocker();

        public IPEndPoint LocalEndPoint => nodeSession?.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => nodeSession?.RemoteEndPoint;

        public Action<byte, byte[]> OnReceived;
        public Action OnConnected;
        public Action OnClosed;

        // which channels are opened on this session
        private bool[] alloweds;

        public PeerSession(Peer peer, int channels)
        {
            alloweds = new bool[channels];
            Peer = peer;
        }

        public void Send(IChannel channel, byte[] data)
        {
            try
            {
                if (!IsChannelAllowed(channel))
                    return;

                if (nodeSession == null)
                    return;

                nodeSession.Send(channel.GetChannelByte(), data);
            }
            catch (Exception e)
            {
                // Console.WriteLine(e);
            }
        }

        public void OpenChannel(IChannel channel)
        {
            // automatically increase capacity
            var index = channel.GetIndex();
            var current = alloweds.Length;
            Debug.Assert(current > 0);
            if (current <= index)
            {
                var buffer = new bool[current * 2];
                Array.Copy(alloweds, buffer, current);
                alloweds = buffer;
            }

            alloweds[index] = true;
        }

        public X509Certificate2 GetCertificate()
        {
            return Peer.Persona.Certificate;
        }

        private bool IsChannelAllowed(IChannel channel)
        {
            return alloweds[channel.GetIndex()];
        }

        // set websocket when it is already opened
        internal bool TrySetSession(NodeSession session)
        {
            using (locker.CreateLock())
            {
                if (nodeSession != null)
                    return false;
                Debug.Assert(PeerState != PeerState.Connected);
                Debug.Assert(nodeSession == null);
                nodeSession = session;
                nodeSession.Connect(this);
                OnConnected();
                return true;
            }
        }

        void ISessionListener.OnClose()
        {
            Disconnect();
            OnClosed.Call();
        }

        // TODO is it usefull ?
        void ISessionListener.OnError()
        {
        }

        void ISessionListener.OnMessage(byte channel, byte[] data)
        {
            OnReceived(channel, data);
        }

        public void SetNodeEndPoint(IPEndPoint endpoint)
        {
            Peer.Node.SetEndPoint(endpoint);
        }

        public void Disconnect()
        {
            using (locker.CreateLock())
            {
                Debug.Assert(PeerState == PeerState.Disconnected);
                nodeSession = null;
            }
        }

        public string ID => Peer.Persona?.Thumbprint.ToString();
        public IPEndPoint IP => Peer.Node.EndPoint;
        public long Height { get; set; }
        public int Ping => nodeSession.GetPing().Milliseconds;
        public DateTime LastPong => nodeSession.GetLastPong();

        IPeer ISession.Peer => this;

        public bool OnPing()
        {
            return nodeSession?.OnPing() ?? false;
        }

        public bool OnPong()
        {
            return nodeSession?.OnPong() ?? false;
        }
    }

    public interface ISessionListener
    {
        void OnMessage(byte channel, byte[] bytes);
        void OnError();
        void OnClose();
    }
}