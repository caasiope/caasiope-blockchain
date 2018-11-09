using System;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class ImportPrivateKeyCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument privatekeyArgument;
        private readonly CommandArgument aliasArgument;

        // importprivatekey btcissuer n6uPVvs4x8A80yt3yw/KTUSEZQpqsu0FM/gSm7EPmXs=
        public ImportPrivateKeyCommand() : base("importprivatekey")
        {
            aliasArgument = RegisterArgument(new CommandArgument("alias"));
            privatekeyArgument = RegisterArgument(new CommandArgument("private key"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var wallet = PrivateKeyNotWallet.FromBase64(privatekeyArgument.Value);
            var alias = aliasArgument.Value;
            if (WalletService.ImportPrivateKey(alias, wallet))
                Console.WriteLine($"Successfully imported key : {alias}");
        }
    }
}