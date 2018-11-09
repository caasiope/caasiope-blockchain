using System;
using Caasiope.Protocol.Types;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public static class ConsoleExtensions
    {
        public static void Dump(this PrivateKeyNotWallet wallet)
        {
            var privkey = wallet.PrivateKey;
            var pubkey = wallet.PublicKey;
            var address = pubkey.GetAddress();

            Console.WriteLine("Private Key : {0}", privkey.ToBase64());
            Console.WriteLine("Public Key : {0}", pubkey.ToBase64());
            Console.WriteLine("Address : {0}", address.Encoded);
        }
    }

    public static class ConsoleHelper
    {
        public static PrivateKeyNotWallet GeneratePrivateKey()
        {
            var gen = new ECKeyPairGenerator("EC");
            var keyGenParam = new KeyGenerationParameters(new SecureRandom(), 256);
            gen.Init(keyGenParam);
            var keyPair = gen.GenerateKeyPair();
            var privateBytes = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArray();

            return PrivateKeyNotWallet.FromBytes(privateBytes);
        }
    }

    public class GeneratePrivateKeyCommand : InjectedConsoleCommand
    {
        public GeneratePrivateKeyCommand() : base("generateprivatekey")
        {
        }

        protected override void ExecuteCommand(string[] args)
        {
            ConsoleHelper.GeneratePrivateKey().Dump();
        }
    }
}