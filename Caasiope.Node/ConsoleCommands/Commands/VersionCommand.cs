using System;
using System.Diagnostics;
using System.Reflection;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class VersionCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.ProductVersion == "%ASSEMBLYINFORMATIONALVERSION%" ? "Unknown" : fvi.ProductVersion;
            Console.WriteLine($"{fvi.ProductName} v.{version}");
        }
    }
}