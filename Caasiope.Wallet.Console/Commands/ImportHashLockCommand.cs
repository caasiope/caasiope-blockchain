using System;
using Caasiope.Protocol.Types;
using Helios.Common;
using Caasiope.NBitcoin;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class ImportHashLockCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument aliasArgument;
        private readonly CommandArgument hashArgument;

        // importhashlock hashlock rzKc1TO9M4TLnZyIEqx7xeAmjmCr+uMtB9M+uz7BHIM=
        public ImportHashLockCommand() : base("importhashlock")
        {
            aliasArgument = RegisterArgument(new CommandArgument("alias"));
            hashArgument = RegisterArgument(new CommandArgument("hash"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            // TODO register hash lock account somewhere
            var bytes = Convert.FromBase64String(hashArgument.Value);
            var hashlock = new HashLock(new SecretHash(SecretHashType.SHA3, new Hash256(bytes)));

            WalletService.ImportDeclaration(aliasArgument.Value, hashlock);

            Console.WriteLine($"HashLock Address {hashlock.Address.Encoded}");
        }
    }
}