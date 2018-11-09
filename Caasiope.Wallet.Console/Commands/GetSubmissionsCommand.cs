using System;
using System.Runtime.InteropServices;
using Caasiope.Node;
using Caasiope.Node.Processors.Commands;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    // TODO find a good name for this command
    class GetSubmissionsCommand : InjectedConsoleCommand
    {
        public GetSubmissionsCommand() : base("getsubmissions") { }

        protected override void ExecuteCommand(string[] args)
        {
            WalletService.TransactionSubmissionListener.GetStatistics(out var success, out var failure, out var pending);
            var total = success + failure + pending;
            Console.WriteLine($"Total : {total} Success : {success} Failure : {failure} Pending : {pending}");
        }
    }
}
