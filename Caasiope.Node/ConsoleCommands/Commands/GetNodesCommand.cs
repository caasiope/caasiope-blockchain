using System;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetNodesCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var nodes = ConnectionService.GetAllNodes().ToList();
            Console.WriteLine($"Number of nodes : {nodes.Count}");
            foreach (var node in nodes)
            {
                var privateText = node.IsPrivateEndPoint.HasValue && node.IsPrivateEndPoint.Value? "; Private Endpoint" : "";
                Console.WriteLine($"IP : {node.EndPoint}{privateText}");
            }
        }
    }
}