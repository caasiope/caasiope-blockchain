using System;
using Caasiope.Protocol.Types;
using Helios.Common;
using Caasiope.NBitcoin;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class ImportTimeLock : InjectedConsoleCommand
    {
        private readonly CommandArgument aliasArgument;
        private readonly CommandArgument dateArgument;
        private readonly CommandArgument timeArgument;

        // importtimelock timelock 2018/08/06 12:00:00
        public ImportTimeLock() : base("importtimelock")
        {
            aliasArgument = RegisterArgument(new CommandArgument("alias"));
            dateArgument = RegisterArgument(new CommandArgument("date (yyyy/MM/dd)"));
            timeArgument = RegisterArgument(new CommandArgument("time (HH:mm:ss)"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var date = dateArgument.Value;
            var time = timeArgument.Value;

            var datetime = DateTime.Parse($"{date} {time}"); // "MM/dd/yyyy HH:mm:ss"
            var timelock = new TimeLock(datetime.ToUnixTimestamp());

            WalletService.ImportDeclaration(aliasArgument.Value, timelock);

            Console.WriteLine($"TimeLock Address {timelock.Address.Encoded}");
        }
    }
}