using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Caasiope.Protocol.Types;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class InitializeWalletCommand : InjectedConsoleCommand
    {
        public InitializeWalletCommand() : base("initializewallet") { }

        protected override void ExecuteCommand(string[] args)
        {
            while (true)
            {
                var answer = ReadAnswer("Would you like to initialize the wallet?");
                if (answer == "n")
                    return;
                if (answer == "y")
                    break;
            }

            var wallet = GeneratePrivateKey();

            Console.WriteLine("Private key generated!");
            Console.WriteLine("-----------------------------------");
            wallet.Dump();
            Console.WriteLine("-----------------------------------");

            Console.WriteLine("Please input an alias for the private key:");

            string alias;

            while (true)
            {
                alias = Console.ReadLine();

                if (string.IsNullOrEmpty(alias))
                {
                    Console.WriteLine("Please input a correct alias");
                    continue;
                }

                if (!WalletService.AliasManager.TryGetByAlias(alias, out var item))
                    break;

                Console.WriteLine($"Sorry, alias {alias} already exists, please try a different alias");
            }

            WalletService.ImportPrivateKey(alias, wallet);

            if (WalletService.SetActiveKey(alias))
            {
                Console.WriteLine("The privatekey has been successfully set as active.");

                while (true)
                {
                    var answer = ReadAnswer("Would you like the key to be loaded automatically next time?");
                    if (answer == "n")
                    {
                        Console.WriteLine("Initialization finished!");
                        return;
                    }

                    if (answer == "y")
                        break;
                }

                var lines = new List<string>()
                {
                    $"{new ImportPrivateKeyCommand().Name} {alias} {wallet.PrivateKey.ToBase64()}",
                    $"{new SetActiveKeyCommand().Name} {alias}"
                };

                File.WriteAllLines(Configuration.InstructionsFile, lines);

                Console.WriteLine("Initialization finished!");
            }
            else
            {
                Console.WriteLine("Cannot set the private key as active!");
                return;
            }
        }

        private PrivateKeyNotWallet GeneratePrivateKey()
        {
            return ConsoleHelper.GeneratePrivateKey();
        }

        private string ReadAnswer(string question)
        {
            Console.WriteLine($"{question} (Y/N)");

            try
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return string.Empty;
                return line.ToLower(CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}