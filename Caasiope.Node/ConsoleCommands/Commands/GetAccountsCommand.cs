using System;
using System.Linq;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetAccountsCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var results = LiveService.AccountManager.GetAccounts().ToList();
            var issuers = LiveService.IssuerManager.GetIssuers().ToDictionary(_ => _.Address);

            Console.WriteLine($"Number of Accounts : {results.Count}");

            foreach (var account in results)
            {
                var issuerInfo = "";
                if (issuers.TryGetValue(account.Address, out var issuer))
                    issuerInfo = $"Issuer for {Currency.ToSymbol(issuer.Currency)}; ";
                Console.WriteLine($"Address : {account.Address.Encoded}; {issuerInfo}Balances : ");

                foreach (var balance in account.Balances)
                {
                    Console.WriteLine($"    Currency : {Currency.ToSymbol(balance.Currency)}; Amount : {Amount.ToWholeDecimal(balance.Amount)};");
                }
            }
        }
    }
}