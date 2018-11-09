namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    class AddOutputTransaction : AddInputOutputTransaction
    {
        // addoutput qyl68tygnjx6qqwrsmynmejmc9wxlw7almv3397j BTC 1.3
        public AddOutputTransaction() : base("addoutput") { }
        protected override bool IsInput() => false;
    }
}