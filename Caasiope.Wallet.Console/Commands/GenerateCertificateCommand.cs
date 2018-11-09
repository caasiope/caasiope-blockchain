using System;
using Caasiope.P2P.Security;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    // generate a self signed certificate
    public class GenerateCertificateCommand : ConsoleCommand
    {
        readonly CommandArgument pathArgument;

        public GenerateCertificateCommand() : base("generatecertificate")
        {
            pathArgument = RegisterArgument(new CommandArgument("path"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var certificate = CertificateHelper.GenerateCertificate(pathArgument.Value);
            Console.WriteLine("Certificate Generated !");
        }
    }
}