using System.Collections.Generic;
using System.Reflection;
using Caasiope.Node.ConsoleCommands.Commands;

namespace Caasiope.Node.ConsoleCommands
{
    public class ConsoleCommandProcessor : Helios.Common.ConsoleCommandProcessor
    {
        public ConsoleCommandProcessor(Assembly assembly) : base(new List<Assembly> { assembly, typeof(GetPeersCommand).Assembly })
        {
            Injector.Add(this);
        }
    }
}