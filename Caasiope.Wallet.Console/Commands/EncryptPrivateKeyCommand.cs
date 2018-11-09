using System;
using System.Diagnostics;
using Caasiope.Protocol.Formats;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class EncryptPrivateKeyCommand : InjectedConsoleCommand
    {
        readonly CommandArgument privateKeyArgument;
        readonly CommandArgument passwordArgument;

        public EncryptPrivateKeyCommand() : base("encryptprivatekey")
        {
            privateKeyArgument = RegisterArgument(new CommandArgument("private key"));
            passwordArgument = RegisterArgument(new CommandArgument("password"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var privatekey = privateKeyArgument.Value;
            var wallet = PrivateKeyNotWallet.FromBase64(privatekey);
            var password = passwordArgument.Value;

            var encrypted = EncryptedPrivateKeyFormat.Encrypt(wallet, password, LedgerService.LedgerManager.Network);
            var decrypted = EncryptedPrivateKeyFormat.Decrypt(encrypted, password, LedgerService.LedgerManager.Network);

            Debug.Assert(decrypted.PrivateKey.ToBase64() == privatekey);

            Console.WriteLine("Success ! Encrypted : {0}", encrypted);
        }
    }
}