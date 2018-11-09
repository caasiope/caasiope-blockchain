using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Caasiope.P2P;
using Helios.Common.Logs;

namespace Caasiope.Node.Connections
{
    public interface IP2PConnection
    {
        void Start();
        void Stop();

        IChannel CreateChannel(byte header);

        void OnConnected(Action<ISession> callback);
        X509Certificate2 GetCertificate();
        void Initialize(ILogger logger);
        IEnumerable<IPeer> GetConnectedPeers();
        IEnumerable<P2P.Node> GetAllNodes();
        IEnumerable<IPEndPoint> GetSelfEndPoints();
        void WipeNodeList();
    }

    public class P2PConnection : IP2PConnection
    {
        private readonly P2P.P2PConnection connection;

        public P2PConnection(P2PConfiguration configuration, IEnumerable<IPEndPoint> nodes, int min, int max)
        {
            connection = new P2P.P2PConnection(configuration, nodes, min, max);
        }

        public void Start()
        {
            connection.Start();
        }

        public void Stop()
        {
            connection.Stop();
        }

        public IChannel CreateChannel(byte header)
        {
            return connection.CreateChannel(header);
        }

        public void OnConnected(Action<ISession> callback)
        {
            connection.OnConnected(callback);
        }

        public X509Certificate2 GetCertificate()
        {
            return connection.GetCertificate();
        }

        public void Initialize(ILogger logger)
        {
            connection.Initialize(logger);
        }

        public IEnumerable<IPeer> GetConnectedPeers()
        {
            return connection.GetConnectedPeers();
        }

        public IEnumerable<P2P.Node> GetAllNodes()
        {
            return connection.GetAllNodes();
        }

        public IEnumerable<IPEndPoint> GetSelfEndPoints()
        {
            return connection.GetSelfEndPoints();
        }

        public void WipeNodeList()
        {
            connection.WipeNodeList();
        }
    }
}