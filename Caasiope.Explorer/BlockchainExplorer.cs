using Caasiope.Node;

//The blockchain explorer of Caasiope blockchain
//Authors:
//  Guillaume Bonnot
//    Github : guillaumebonnot
//  Ilia Palekhov
//    Github : duke009

namespace Caasiope.Explorer
{
	public class BlockchainExplorer
	{
		public readonly IExplorerConnectionService ExplorerConnectionService;

		public BlockchainExplorer(ServiceManager services, IExplorerServiceFactory factory = null)
		{
			if (factory == null)
				factory = new RealExplorerServiceFactory();
			
			ExplorerConnectionService = services.Add(factory.CreateExplorerConnectionService());
		}
	}

	public class RealExplorerServiceFactory : IExplorerServiceFactory
	{
		public IExplorerConnectionService CreateExplorerConnectionService()
		{
			var server = new WebSocketServer(NodeConfiguration.GetPath("explorer_server.txt"));
		    var connection = new ExplorerConnectionService(server);
		    var dispatcher = new Dispatcher(connection.Logger);
            connection.SetDispatcher(dispatcher);
		    return connection;
		}
    }

	public interface IExplorerServiceFactory
	{
		IExplorerConnectionService CreateExplorerConnectionService();
	}
}
