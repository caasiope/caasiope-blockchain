using System;
using System.Collections.Generic;
using Caasiope.JSON;
using Caasiope.Node.Connections;
using Caasiope.Node.Services;
using Caasiope.P2P;
using Caasiope.Protocol.Types;
using Helios.Common.Configurations;
using P2PConnection = Caasiope.Node.Connections.P2PConnection;

namespace Caasiope.Node
{
    public interface INodeServiceFactory
    {
        IDatabaseService CreateDatabaseService();
        ILiveService CreateLiveService();
        ILedgerService CreateLedgerService();
        IConnectionService CreateConnectionService();
        IDataTransformationService CreateDataTransformationService();
    }
	
    public class RealNodeServiceFactory : INodeServiceFactory
    {
        private readonly Network network;

        public RealNodeServiceFactory(Network network)
        {
            this.network = network;
        }

        public IDatabaseService CreateDatabaseService()
        {
            return new DatabaseService();
        }

        public virtual ILiveService CreateLiveService()
        {
            var nodes = new UrlConfiguration(NodeConfiguration.GetPath("validators.txt")).Lines;
            var quorum = int.Parse(nodes[0]);
            var validators = new List<PublicKey>();
            for (int i = 1; i < nodes.Count; i++)
            {
                validators.Add(new PublicKey(Convert.FromBase64String(nodes[i])));
            }

            var lines = new UrlConfiguration(NodeConfiguration.GetPath("issuers.txt")).Lines;
            var issuers = new List<Issuer>();
            foreach (var line in lines)
            {
                var splited = line.Split('|');
                issuers.Add(new Issuer(new Address(splited[1].Trim()), Currency.FromSymbol(splited[0].Trim())));
            }

            return new LiveService(quorum, validators, issuers);
        }

        public ILedgerService CreateLedgerService()
        {
            return new LedgerService(network);
        }

        public IConnectionService CreateConnectionService()
        {
	        var config = NodeBuilder.BuildConfiguration(NodeConfiguration.GetPath("node_server.txt"), NodeConfiguration.GetPath("node_id.pem"));
	        var nodes = P2PServerConfiguration.ToIPEndpoints(new UrlConfiguration(NodeConfiguration.GetPath("nodes.txt")).Lines);
	        var connection = new Connections.P2PConnection(config, nodes, 5, 20);

            var connectionService = new ConnectionService(connection , false /*TODO to config ?*/);
            var dispatcher = new Dispatcher(connectionService.Logger);
            var bockchainChannel = connectionService.CreateChannel(ConnectionService.BLOCKCHAIN_CHANNEL, dispatcher, new BlockchainApi().JsonMessageFactory, session => true);
            
            connectionService.SetBlockchainChannel(bockchainChannel);

            return connectionService;
        }

        public IDataTransformationService CreateDataTransformationService()
        {
            return new DataTransformationService();
        }
    }
}