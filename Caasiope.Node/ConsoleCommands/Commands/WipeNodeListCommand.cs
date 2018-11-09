using System;
using System.IO;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class WipeNodeListCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            ConnectionService.WipeNodeList();

            Console.WriteLine("Wiped");
        }
    }
}