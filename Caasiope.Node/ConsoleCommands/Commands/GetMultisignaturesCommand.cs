using System;
using System.Linq;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetMultiSignaturesCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var results = LiveService.AccountManager.GetAccounts().Values
                .Where(account => account.Address.Type == AddressType.MultiSignatureECDSA && account.Declaration != null)
                .Select(account => (MultiSignature)account.Declaration).ToList();

            Console.WriteLine($"Number of MultiSignatures : {results.Count}");

            foreach (var multiSignature in results)
            {
                Console.WriteLine($"Hash : {multiSignature.Hash.ToBase64()} Required : {multiSignature.Required} Signers : ");

                foreach (var multiSignatureSigner in multiSignature.Signers)
                {
                    Console.WriteLine($"Address : {multiSignatureSigner.Encoded} AddressType : {multiSignatureSigner.Type}");
                }
            }
        }
    }
}