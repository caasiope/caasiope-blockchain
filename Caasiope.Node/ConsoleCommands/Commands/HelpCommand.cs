using System;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class HelpCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            Console.WriteLine("Available commands : ");
            foreach (var commandName in ConsoleCommandProcessor.GetRegisteredCommands().OrderBy(_ => _))
            {
                Console.WriteLine(commandName);
            }
        }
    }
}