using System;
using Caasiope.Node;
using Helios.Common;
using Helios.Common.Synchronization;
using ConsoleCommandProcessor = Caasiope.Node.ConsoleCommands.ConsoleCommandProcessor;

namespace Caasiope.Explorer
{
	class Program
	{
		static void Main(string[] args)
		{
			AssertionHandler.CatchAssertions();
		    PrivateLock.OnDeadLock += () => Console.WriteLine("!!!!!!!!!!!!! DEADLOCK !!!!!!!!!!!!");

            var services = new ServiceManager();
			var node = new BlockchainNode(services);
			var explorer = new BlockchainExplorer(services);

		    Console.WriteLine("Initializing...");
		    services.Initialize();
		    Console.WriteLine("Starting...");
		    services.Start();
		    Console.WriteLine("Running...");

		    var console = new ConsoleCommandProcessor(typeof(Program).Assembly);
		    console.Initialize();

		    console.Run();
        }
	}
}
