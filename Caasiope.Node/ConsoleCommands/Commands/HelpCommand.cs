using System;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class HelpCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            Console.WriteLine("Available commands : ");
            foreach (var commandName in ConsoleCommandProcessor.GetRegisteredCommands())
            {
                Console.WriteLine(commandName);
            }
        }
    }
}