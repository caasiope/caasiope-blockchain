using System;
using System.Text;
using HashLib;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class HashTextCommand : InjectedConsoleCommand
    {
        readonly CommandArgument textArgument;

        public HashTextCommand() : base("hashtext")
        {
            textArgument = RegisterArgument(new CommandArgument("hash"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var text = textArgument.Value;
            var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            var hash = hasher.ComputeBytes(Encoding.UTF8.GetBytes(text)).GetBytes();

            Console.WriteLine("Signature : {0}", Convert.ToBase64String(hash));
        }
    }
}