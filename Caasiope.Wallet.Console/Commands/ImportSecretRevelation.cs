using System;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class ImportSecretRevelation : InjectedConsoleCommand
    {
        private readonly CommandArgument aliasArgument;
        private readonly CommandArgument secretArgument;

        // importsecretrevelation revelation njTBWqIIeWVPPgGGXn3KUZ8U0aHmrIknxxakLbsHTVI=
        public ImportSecretRevelation() : base("importsecretrevelation")
        {
            aliasArgument = RegisterArgument(new CommandArgument("alias"));
            secretArgument = RegisterArgument(new CommandArgument("secret"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            // TODO register hash lock account somewhere
            var bytes = Convert.FromBase64String(secretArgument.Value);
            var secret = new Secret(bytes);
            var revelation = new SecretRevelation(secret);

            WalletService.ImportDeclaration(aliasArgument.Value, revelation);

            var hash = secret.ComputeSecretHash(SecretHashType.SHA3);
            Console.WriteLine($"Hash {hash.Hash.ToBase64()}");
        }
    }
}