using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class MacroCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] t)
        {
            Console.WriteLine("Macro mode enabled.\n Type \"save [path]\" to save your macro.\n Type \"exit\" to exit without saving changes.");
            var instructions = new List<string>();
            while (true)
            {
                Console.WriteLine("Please input instructions :");

                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                    continue;

                var args = line.Split(' ');

                var name = args[0];

                if (name == "exit")
                    return;

                if (name == "save")
                {
                    var path = args.Length == 2 ? args[1] : "instructions.txt";
                    SaveInstructions(instructions, path);
                    Console.WriteLine($"Saved to {path}");
                    return;
                }

                if (ConsoleCommandProcessor.Run(name, args))
                {
                    instructions.Add(line);
                }
            }
        }

        private void SaveInstructions(List<string> instructions, string path)
        {
            File.WriteAllLines(path, instructions);
        }
    }
}