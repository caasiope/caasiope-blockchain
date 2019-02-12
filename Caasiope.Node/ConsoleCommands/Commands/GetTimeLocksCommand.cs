using System;
using System.Linq;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetTimeLocksCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var results = LiveService.AccountManager.GetAccounts().Values
                .Where(account => account.Address.Type == AddressType.TimeLock && account.Declaration != null)
                .Select(account => (TimeLock)account.Declaration).ToList();

            Console.WriteLine($"Number of TimeLocks : {results.Count}");

            foreach (var timeLock in results)
            {
                Console.WriteLine($"Address : {timeLock.Address.Encoded} Hash : {timeLock.Hash.ToBase64()} Timestamp : {timeLock.Timestamp}");
            }
        }
    }
}