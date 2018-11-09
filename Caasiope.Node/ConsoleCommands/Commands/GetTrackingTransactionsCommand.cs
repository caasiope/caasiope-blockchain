using System;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetTrackingTransactionsCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var hashes = LiveService.TransactionManager.GetAllHashes();
            Console.WriteLine($"Number of tracking transactions : {hashes.Count}");
            if (!hashes.Any())
                return;

            Console.WriteLine("Hashes : ");
            foreach (var hash in hashes)
            {
                Console.WriteLine($"{hash.ToBase64()}");
            }
        }
    }
}