using System;
using System.Linq;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetIssuersCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var issuers = LiveService.IssuerManager.GetIssuers().ToList();
            Console.WriteLine($"Number of issuers : {issuers.Count}");
            foreach (var issuer in issuers)
            {
                Console.WriteLine($"Currency : {Currency.ToSymbol(issuer.Currency)} Address : {issuer.Address.Encoded}");
            }
        }
    }
}