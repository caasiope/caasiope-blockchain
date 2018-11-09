using System;
using System.IO;
using System.Linq;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class LoadInstructionsCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument pathArgument;

        // loadinstructions instructions.txt
        public LoadInstructionsCommand() : base("loadinstructions")
        {
            pathArgument = RegisterArgument(new CommandArgument("path"));
        }

        protected override void ExecuteCommand(string[] a)
        {
            var instructions = File.ReadAllLines(pathArgument.Value);
            instructions = instructions.Where(_ => !string.IsNullOrEmpty(_.Trim())).ToArray();
            var index = 0;
            foreach (var instruction in instructions)
            {
                var args = instruction.Split(' ');

                var name = args[0];

                if (name == "exit")
                {
                    return;
                }

                if (!ConsoleCommandProcessor.Run(name, args))
                {
                    Console.WriteLine($"Instruction at line {index} corrupted ({name})");
                }
                index++;
            }
        }
    }
}