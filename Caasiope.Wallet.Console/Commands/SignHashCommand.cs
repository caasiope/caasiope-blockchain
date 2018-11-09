using System;
using Caasiope.NBitcoin;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class SignHashCommand : InjectedConsoleCommand
    {
        readonly CommandArgument privatekeyArgument;
        readonly CommandArgument hashArgument;

        public SignHashCommand() : base("signhash")
        {
            privatekeyArgument = RegisterArgument(new CommandArgument("private key"));
            hashArgument = RegisterArgument(new CommandArgument("hash"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var privatekey = privatekeyArgument.Value;
            var hash = hashArgument.Value;

            var wallet = PrivateKeyNotWallet.FromBase64(privatekey);

            var signature = wallet.CreateSignature(new Hash256(Convert.FromBase64String(hash)), LedgerService.LedgerManager.Network);

            Console.WriteLine("Signature : {0}", Convert.ToBase64String(signature.SignatureByte.Bytes));
        }
    }
}