using System;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetTimeLocksCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var results = LiveService.TimeLockManager.GetTimeLocks().ToList();

            Console.WriteLine($"Number of TimeLocks : {results.Count}");

            foreach (var timeLock in results)
            {
                Console.WriteLine($"Address : {timeLock.Address.Encoded} Hash : {timeLock.Hash.ToBase64()} Timestamp : {timeLock.Timestamp}");
            }
        }
    }
}