using System;
using Caasiope.Protocol.Types;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class CreateSecretCommand : InjectedConsoleCommand
    {
        public CreateSecretCommand() : base("createsecret") { }

        protected override void ExecuteCommand(string[] args)
        {
            var type = SecretHashType.SHA3;
            var secret = Secret.GenerateSecret();
            var hash = secret.ComputeSecretHash(type);
            Console.WriteLine($"Secret {Convert.ToBase64String(secret.Bytes)}");
            Console.WriteLine($"Hash {hash.Hash.ToBase64()}");
            Console.WriteLine($"Type {type}");
        }
    }
}