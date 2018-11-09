using System;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetSelfEndpointsCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var endPoints = ConnectionService.GetSelfEndPoints().ToList();
            Console.WriteLine($"Number of known self endpoints : {endPoints.Count}");
            foreach (var endPoint in endPoints)
            {
                Console.WriteLine($"Endpoint : {endPoint}");
            }
        }
    }
}