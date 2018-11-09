using Caasiope.Node.Services;

//The blockchain node of Caasiope blockchain
//Authors:
//  Guillaume Bonnot
//    Github : guillaumebonnot
//  Ilia Palekhov
//    Github : duke009

namespace Caasiope.Node
{

    public class BlockchainNode
	{
		public IDatabaseService DatabaseService;
		public ILiveService LiveService;
		public IConnectionService ConnectionService;
		public ILedgerService LedgerService;
		public IDataTransformationService DataTransformationService;

        public BlockchainNode(ServiceManager services)
        {
            if (!NodeConfiguration.IsInitialized()) NodeConfiguration.Initialize();

            var factory = new RealNodeServiceFactory(NodeConfiguration.GetNetwork());

            ConnectionService = services.Add(factory.CreateConnectionService());
			DatabaseService = services.Add(factory.CreateDatabaseService());
			LiveService = services.Add(factory.CreateLiveService());
			LedgerService = services.Add(factory.CreateLedgerService());
		    DataTransformationService = services.Add(factory.CreateDataTransformationService());
		}
	}
}