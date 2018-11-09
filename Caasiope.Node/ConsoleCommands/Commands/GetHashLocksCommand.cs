using System;
using System.Linq;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetHashLocksCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var results = LiveService.HashLockManager.GetHashLocks().ToList();

            Console.WriteLine($"Number of HashLocks : {results.Count}");

            foreach (var hashLock in results)
            {
                Console.WriteLine($"Address : {hashLock.Address.Encoded} Hash : {hashLock.Hash.ToBase64()} Secret Hash : {hashLock.SecretHash.Hash.ToBase64()}");
            }
        }
    }
}