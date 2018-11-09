using System;
using System.Linq;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class GetMultiSignaturesCommand : InjectedConsoleCommand
    {
        protected override void ExecuteCommand(string[] args)
        {
            var results = LiveService.MultiSignatureManager.GetMultiSignatures().ToList();

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