using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Helios.Common.Extensions;
using Helios.Common.Logs;

//The Peer to Peer library for Caasiope network
//Authors:
//  Guillaume Bonnot
//    Github : guillaumebonnot
//  Ilia Palekhov
//    Github : duke009

namespace Caasiope.P2P
{
    // TODO we need to be able to identify peer externally
    // TODO we need to be able to restrict which channel can be used based on the peer (in this layer? i dont think so)
    public interface IP2PConnection
    {
        void Start();
        void Stop();

        IChannel CreateChannel(byte header);
        X509Certificate2 GetCertificate();

        void OnConnected(Action<ISession> callback);
    }

    public class P2PConnection : IP2PConnection, IChannelConnection
    {
        public readonly Dictionary<byte, IChannel> channels = new Dictionary<byte, IChannel>();

        private ILogger logger;
        private readonly IChannel DISCOVER_CHANNEL;
        private readonly Server server;
        private readonly NodeManager nodeManager;
        private readonly PeerManager peerManager;
        private readonly ConnectionsManager connectionsManager;
        private Action<ISession> onConnected;
        private readonly Timer timer; // TODO put this outside
        private readonly DiscoveryProtocol discovery;
        private readonly X509Certificate2 certificate;
        private readonly NodeStorage nodeStorage;
        private bool isStarted;

        public P2PConnection(P2PConfiguration configuration, IEnumerable<IPEndPoint> nodes, int min, int max)
        {
            server = new Server(configuration);
            server.OnClientAuthenticated += OnClientAuthenticated;
            discovery = new DiscoveryProtocol
            {
                OnServerPort = HandleServerPort,
                OnEndPointTable = HandleEndPointTable,
                OnPing = HandlePing,
                OnPong = HandlePong
            };

            certificate = configuration.Certificate;

            // todo GetNode(configuration.IPEndpoint)
            var self = new Persona(certificate);
            nodeManager = new NodeManager(self, configuration.IPEndpoint.Address, configuration.ForwardedPort, nodes);
            peerManager = new PeerManager();
            nodeStorage = new NodeStorage(logger);
            connectionsManager = new ConnectionsManager(min, max);
            timer = new Timer(OnTick, null, Timeout.Infinite, Timeout.Infinite);

            peerManager.OnPeerConnected = peer =>
            {
                connectionsManager.NewConnection();
                onConnected.Call(peer);
            };
            peerManager.OnPeerDisconnected = peer =>
            {
                connectionsManager.PeerDisconnected();
            };
            peerManager.OnReceived = (peer, channel, message) =>
            {
                OnReceived(peer, GetChannel(channel), message);
            };

            DISCOVER_CHANNEL = CreateChannel(0);
        }

        private void HandlePong(PeerSession session)
        {
            if (!session.OnPong())
                ; // problem
        }

        private void HandlePing(PeerSession session)
        {
            session.Send(DISCOVER_CHANNEL, discovery.Pong());
        }

        private void HandleEndPointTable(PeerSession session, List<IPEndPoint> endpoints)
        {
            IEnumerable<IPEndPoint> list = endpoints;

            if (nodeManager.ServerPort > 0)
            {
                // TODO THIS works only on local networks. Need to resolve the public u
                // nodeManager.UpdateSelfEndpoint(new IPEndPoint(session.LocalEndPoint.Address, nodeManager.ServerPort));
            }

            nodeManager.RegisterEndPoints(list);
        }

        private void HandleServerPort(PeerSession peer, int port)
        {
            var address = peer.RemoteEndPoint.Address;
            var endpoint = new IPEndPoint(address, port);
            peer.SetNodeEndPoint(endpoint);
            nodeManager.UpdateNode(endpoint, peer);
        }

        // we come here when we receive a client authenticated on our server 
        private void OnClientAuthenticated(NodeSession session)
        {
            if (connectionsManager.IsReachedMaxPeerLimit())
            {
                CloseSession(session, DisconnectReason.TooManyPeers);
                return;
            }

            if (session.Persona.Thumbprint.Equals(nodeManager.Self.Thumbprint))
            {
                CloseSession(session, DisconnectReason.ClientWithSameThumbprint);
                return;
            }

            // get identity of the peer
            // get or create peer session
            var peer = peerManager.GetOrCreatePeerByPersona(session.Persona, null);

            if (peer.Peer.Node.HasServer) // If it's someone whom we don't know, then we need to update the state on HandleServerPort()
            {
                nodeManager.UpdateNode(peer.Peer.Node.EndPoint, peer);
                var node = nodeManager.GetByEndpoint(peer.Peer.Node.EndPoint);
                session.OnDisconnected += node.Release;
                if (node.IsAvailable)
                    node.Acquire();
            }

            if (peer.PeerState == PeerState.Connected)
            {
                CloseSession(session, DisconnectReason.PeerAlreadyConnected);
                return;
            }
            // we will register peer end point when he sends it
            // if already connected, close connection

            if (!TrySetSession(session, peer))
                return;

            OpenDiscoveryChannel(peer);

            var endpoints = nodeManager.GetEndPointTable(session.RemoteEndPoint);
            if (endpoints.Count > 0)
                peer.Send(DISCOVER_CHANNEL, discovery.EndPointTable(endpoints));
        }

        // we come here when we succesfully authenticated to a peer server
        private void OnServerAuthenticated(NodeSession session, IPEndPoint local, IPEndPoint remote)
        {
            if (session.Persona.Thumbprint.Equals(nodeManager.Self.Thumbprint))
            {
                var selfPeer = peerManager.GetOrCreatePeerByPersona(session.Persona, remote);
                nodeManager.UpdateSelfEndpoint(selfPeer.Peer.Node.EndPoint);
                CloseSession(session, DisconnectReason.ConnectedToSelf);
                return;
            }

            var peer = peerManager.GetOrCreatePeerByPersona(session.Persona, remote);
            //TODO as far as peerManager created a new peer everytime, we need to update it in serverManager. May be it's not the best place
            nodeManager.UpdateNode(remote, peer);

            if (!TrySetSession(session, peer))
                return;

            // send the url of the server
            OpenDiscoveryChannel(peer);

            // TODO handle the case where we are on local network and need to send the local port instead of the forwarded port
            // TODO we should only send that when requested by the peer
            var port = nodeManager.ServerPort;
            if (port != 0)
            {
                peer.Send(DISCOVER_CHANNEL, discovery.ServerPort(port));
            }
        }

        private void OpenDiscoveryChannel(PeerSession peer)
        {
            foreach (var channel in channels.Values)
            {
                if (channel == DISCOVER_CHANNEL)
                    peer.OpenChannel(channel);
            }
        }

        private bool TrySetSession(NodeSession session, PeerSession peer)
        {
            if (peer.TrySetSession(session))
                return true;

            session.Disconnect(DisconnectReason.CannotSetSession);
            return false;
        }

        private void CloseSession(NodeSession session, DisconnectReason reason)
        {
            session.Disconnect(reason);
        }

        private IChannel GetChannel(byte channel)
        {
            return channels[channel];
        }

        private void OnReceived(PeerSession peer, IChannel channel, byte[] message)
        {
            // we cheat
            if (channel.GetChannelByte() == DISCOVER_CHANNEL.GetChannelByte())
                discovery.HandleMessage(peer, message);
            else
                channel.OnReceived(peer, message);
        }

        public void Start()
        {
            isStarted = true;

            //TODO set channels on each session
            nodeManager.Initialize(nodeStorage.LoadEndpoints());
            peerManager.Initialize(channels.Count);

            server.Start();

            // TODO put a cap on maximum peers to try
            while (nodeManager.TryGetAvailableNode(out var available))
            {
                ConnectToNode(available);
            }

            timer.Change(1000, Timeout.Infinite);
        }

        // we open a connection with the peer
        private void OnTick(object state)
        {
            // if we have less than min peers
            // connect to more peers

            while (connectionsManager.IsRequireMorePeers() && nodeManager.TryGetAvailableNode(out var peer))
            {
                ConnectToNode(peer);
            }

            // foreach connected peer
            // send ping
            foreach (var peerSession in peerManager.GetAllConnected())
            {
                Ping(peerSession);
            }

            ProcessSaveNodes();

            timer.Change(1000, Timeout.Infinite);
        }

        private void ConnectToNode(NodeEntry available)
        {
            available.Acquire();
            var client = new Client(available.Node.EndPoint, certificate, logger);
            var session = client.NodeSession;
            session.OnAuthenticated += (local, remote) => OnServerAuthenticated(session, local, remote);
            session.OnDisconnected += available.Release;
            // TODO handle when session is disconnected
            client.Start();
        }

        public void Stop()
        {
            server.Start();
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public IChannel CreateChannel(byte header)
        {
            if (isStarted)
                throw new Exception("you cannot create new channel after the server is already started");

            var channel = new Channel(header, this, channels.Count);
            channels.Add(channel.GetChannelByte(), channel);
            return channel;
        }

        public void Broadcast(IChannel channel, byte[] data)
        {
            peerManager.Broadcast(channel, data);
        }

        public void Send(ISession session, IChannel channel, byte[] data)
        {
            session.Send(channel, data);
        }

        public void OnConnected(Action<ISession> callback)
        {
            onConnected += callback;
        }

        private void Ping(PeerSession session)
        {
            if (session.OnPing())
                session.Send(DISCOVER_CHANNEL, discovery.Ping());
        }

        private byte secondsTicked;

        private void ProcessSaveNodes()
        {
            if (++secondsTicked % 55 == 0)
                return;

            var nodes = nodeManager.GetAllNodes().ToList();
            nodeStorage.SaveNodes(nodes);
        }

        public X509Certificate2 GetCertificate()
        {
            return nodeManager.Self.Certificate;
        }

        public void Initialize(ILogger logger)
        {
            this.logger = logger;
            server.Initialize(logger);
        }

        public IEnumerable<IPeer> GetConnectedPeers()
        {
            return peerManager.GetAllConnected();
        }

        public IEnumerable<Node> GetAllNodes()
        {
            return nodeManager.GetAllNodes();
        }

        public IEnumerable<IPEndPoint> GetSelfEndPoints()
        {
            return nodeManager.GetSelfEndPoints();
        }

        public void WipeNodeList()
        {
            nodeManager.WipeNodeList();
            nodeStorage.WipeNodes();
        }
    }

    public interface ISession
    {
        void Send(IChannel channel, byte[] message);
        void OpenChannel(IChannel channel);
        X509Certificate2 GetCertificate();
        IPeer Peer { get; }
    }
}
