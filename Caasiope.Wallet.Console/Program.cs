using System;
using System.Configuration;
using System.Linq;
using Caasiope.Node;
using Caasiope.Protocol.Types;
using Caasiope.Wallet.CommandLineConsole.Commands;
using Caasiope.Wallet.Services;
using Helios.Common;
using Helios.Common.Synchronization;
using ConsoleCommandProcessor = Caasiope.Node.ConsoleCommands.ConsoleCommandProcessor;

namespace Caasiope.Wallet.CommandLineConsole
{
    class Program
    {
        private static void Main(string[] args)
        {
            AssertionHandler.CatchAssertions();
            PrivateLock.OnDeadLock += () => Console.WriteLine("!!!!!!!!!!!!! DEADLOCK !!!!!!!!!!!!");

            var services = new ServiceManager();
            var node = new BlockchainNode(services);
            var wallet = (IWalletService) new WalletService();
            services.Add(wallet);

            wallet.AddressListener.RegisterWalletUpdated(input =>
            {
                var verb = input.IsInput ? "Sent" : "Received";
                var amount = Amount.ToWholeDecimal(input.Amount);
                var currency = Currency.ToSymbol(input.Currency);
                Console.WriteLine($"{verb} {input.Address.Encoded} {amount} {currency}");
            });

            wallet.TransactionSubmissionListener.OnSuccess += (transaction, elapsed) => { Console.WriteLine($"Transaction Included ! Elapsed Time {elapsed.TotalSeconds}s");};

            Console.WriteLine("Initializing...");
            services.Initialize();
            Console.WriteLine("Starting...");
            services.Start();
            Console.WriteLine("Running...");

            var console = new ConsoleCommandProcessor(typeof(SetActiveKeyCommand).Assembly);
            console.Initialize();
            console.RunCommand("loadinstructions", new []{Configuration.InstructionsFile});

            if(!wallet.GetPrivateKeys().Any())
                console.RunCommand("initializewallet", new string[0]);

            console.Run();
        }
    }

    public class Configuration
    {
        public static string InstructionsFile => ConfigurationManager.AppSettings["InstructionsFile"];
    }
}
