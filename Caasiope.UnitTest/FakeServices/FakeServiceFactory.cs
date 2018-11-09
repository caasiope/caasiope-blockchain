using System;
using System.Collections.Generic;
using Helios.Common.Logs;
using Caasiope.JSON;
using Caasiope.Node;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Configurations;

namespace Caasiope.UnitTest.FakeServices
{
    class FakeNodeServiceFactory
    {
        public Network Network;

        public FakeNodeServiceFactory()
        {
            Network = NodeConfiguration.GetNetwork();
        }

        public IDatabaseService CreateDatabaseService()
        {
            return new DatabaseService();
        }

        public ILiveService CreateLiveService()
        {
            var nodes = new UrlConfiguration(NodeConfiguration.GetPath("validators.txt")).Lines;
            var quorum = int.Parse(nodes[0]);
            var validators = new List<PublicKey>();
            for (var i = 1; i < nodes.Count; i++)
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

        public IConnectionService CreateConnectionService()
        {
            // TODO dont duplicate with real and create method
            var service = new ConnectionService(new FakeP2PConnection(), false);
            var dispatcher = new Caasiope.Node.Connections.Dispatcher(service.Logger);
            var blockchainChannel = service.CreateChannel(ConnectionService.BLOCKCHAIN_CHANNEL, dispatcher, new BlockchainApi().JsonMessageFactory, session => true);
            service.SetBlockchainChannel(blockchainChannel);
            return service;
        }

        public ILedgerService CreateLedgerService()
        {
            return new LedgerService(Network);
        }

        public IDataTransformationService CreateDataTransformationService()
        {
            return new DataTransformationService();
        }
    }

    public class FakeLogger : ILogger
    {
        public void Log(string message, Exception exception = null) {}
        public void LogInfo(string message, Exception exception = null) {}
        public void LogDebug(string message, Exception exception = null) {}
    }
}
