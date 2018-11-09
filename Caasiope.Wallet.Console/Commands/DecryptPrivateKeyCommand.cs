using System;
using Caasiope.Node.Services;
using Caasiope.Protocol.Formats;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class DecryptPrivateKeyCommand : InjectedConsoleCommand
    {
        readonly CommandArgument encryptedArgument;
        readonly CommandArgument passwordArgument;

        public DecryptPrivateKeyCommand() : base("decryptprivatekey")
        {
            encryptedArgument = RegisterArgument(new CommandArgument("encrypted private key"));
            passwordArgument = RegisterArgument(new CommandArgument("password"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var encrypted = encryptedArgument.Value;
            var password = passwordArgument.Value;
            var decrypted = EncryptedPrivateKeyFormat.Decrypt(encrypted, password, LedgerService.LedgerManager.Network);
			
            decrypted.Dump();
        }
    }
}