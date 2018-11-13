using System;
using Caasiope.NBitcoin;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Node.ConsoleCommands.Commands
{
    public class AnalyzeAddressCommand : ConsoleCommand
    {
        private readonly CommandArgument addressArgument;

        public AnalyzeAddressCommand()
        {
            addressArgument = RegisterArgument(new CommandArgument("address"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var encoded = addressArgument.Value;
            if (!Address.Verify(encoded))
            {
                Console.WriteLine($"[{encoded}] is not a Caasiope address!");
            }

            var address = new Address(encoded);

            LogAddress(address);
        }

        private void LogAddress(Address address)
        {
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine($"[{address.Encoded}] is a Caasiope address : ");
            Console.WriteLine($"Type : {address.Type}");

            switch (address.Type)
            {
                case AddressType.ECDSA:
                case AddressType.MultiSignatureECDSA:
                case AddressType.HashLock:
                    // TODO ?
                    break;
                case AddressType.TimeLock:
                    // TODO extension to Address?
                    var bytes = address.ToRawBytes();
                    var timestampLong = BitConverter.ToInt64(bytes, 0);
                    var timestamp = Utils.UnixTimeToDateTime(timestampLong);
                    Console.WriteLine($"Timestamp : {timestamp}");

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Console.WriteLine("---------------------------------------------");
        }
    }
}