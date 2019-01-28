using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Helios.Common.Synchronization;

namespace Caasiope.P2P
{
    public class NodeManager
    {
        public readonly Persona Self;
        public readonly int ForwardedPort;
        public readonly int ServerPort;

        private readonly HashSet<IPEndPoint> knownSelfEndPoints = new HashSet<IPEndPoint>();
        private readonly Dictionary<IPEndPoint, NodeEntry> nodes = new Dictionary<IPEndPoint, NodeEntry>();
        private readonly MonitorLocker locker = new MonitorLocker();
        private static readonly Random Random = new Random();

        public NodeManager(Persona self, IPEndPoint endPoint, int forwardedPort, IEnumerable<IPEndPoint> endPoints)
        {
            Self = self;
            if(forwardedPort > 0)
                knownSelfEndPoints.Add(new IPEndPoint(endPoint.Address, forwardedPort));

            ServerPort = endPoint.Port;
            ForwardedPort = forwardedPort;
            RegisterEndPoints(endPoints);
        }

        public void Initialize(IEnumerable<IPEndPoint> endPoints)
        {
            RegisterEndPoints(endPoints);
        }

        public void RegisterEndPoints(IEnumerable<IPEndPoint> endPoints)
        {
            foreach (var node in endPoints)
            {
                RegisterEndPoint(node);
            }
        }

        private void RegisterEndPoint(IPEndPoint endPoint)
        {
            if (endPoint == null)
                return;

            var node = new Node(endPoint);
            if (!knownSelfEndPoints.Any() && knownSelfEndPoints.Contains(node.EndPoint))
                return;

            if (nodes.ContainsKey(node.EndPoint))
                return;

            Debug.Assert(node.HasServer);
            nodes.Add(node.EndPoint, new NodeEntry(node));
        }

        public bool TryGetAvailableNode(out NodeEntry node)
        {
            var now = DateTime.Now;
            var candidates = new List<NodeEntry>();
            using (locker.CreateLock())
            {
                foreach (var candidate in nodes.Values)
                {
                    // TODO find out why Debug.Assert(candidate.Node.HasServer); // looks like not thread safe
                    if (!candidate.Node.HasServer)
                        continue;

                    if (candidate.IsAvailable && !knownSelfEndPoints.Contains(candidate.Node.EndPoint) && IsReady(candidate, now))
                        candidates.Add(candidate);
                }
            }

            if (!candidates.Any())
            {
                node = null;
                return false;
            }

            node = candidates[Random.Next(candidates.Count)];
            return true;
        }

        // TODO make somehing more intelligent
        private bool IsReady(NodeEntry candidate, DateTime now)
        {
            var elapsed = now - candidate.DisconnectedTime;
            return elapsed.Seconds > 10;
        }

        // TODO there is a problem when you have a connection with a node that did not register yet
        // the current node session needs to acquire the new node entry and register callback to release it when disconnected
        public void UpdateNode(IPEndPoint endpoint, PeerSession peerSession)
        {
            using (locker.CreateLock())
            {
                if (nodes.ContainsKey(endpoint))
                {
                    if(peerSession.Peer.Node.HasServer)
                        nodes[endpoint].Node.SetEndPoint(peerSession.Peer.Node.EndPoint);
                }
                else
                {
                    RegisterEndPoint(endpoint);
                }
            }
        }

        public List<IPEndPoint> GetEndPointTable(IPEndPoint client)
        {
            if(client == null)
                return new List<IPEndPoint>();

            using (locker.CreateLock())
            {
                var scored = GetGoodScoredNodes(nodes.Values.ToList());
                List<NodeEntry> results;
                
                // send private and public if it's private
                if (client.Address.IsPrivate())
                {
                    results = scored.ToList();
                }
                else
                {
                    results = scored.Where(_ => _.Node.IsPrivateEndPoint.HasValue && !_.Node.IsPrivateEndPoint.Value).ToList();
                }

                return results.Select(_ => _.Node.EndPoint).Take(10).ToList();
            }
        }

        //TODO
        private List<NodeEntry> GetGoodScoredNodes(List<NodeEntry> nodes)
        {
                        // This is the dirty fix
            return nodes.Where(_ => _.Node.HasServer).ToList();
        }

        public List<Node> GetAllNodes()
        {
            using (locker.CreateLock())
            {
                return nodes.Values.Select(entry => entry.Node).ToList();
            }
        }

        public IEnumerable<IPEndPoint> GetSelfEndPoints()
        {
            return knownSelfEndPoints;
        }

        public void UpdateSelfEndpoint(IPEndPoint ipEndPoint)
        {
            knownSelfEndPoints.Add(ipEndPoint);
        }
        
        public NodeEntry GetByEndpoint(IPEndPoint endpoint)
        {
            using (locker.CreateLock())
            {
                return nodes[endpoint];
            }
        }

        public void WipeNodeList()
        {
            using (locker.CreateLock())
            {
                nodes.Clear();
            }
        }
    }
}