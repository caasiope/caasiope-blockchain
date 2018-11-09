namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    class AddInputTransaction : AddInputOutputTransaction
    {
        // addinput qywradf3szdt33q7d9ccgjm4t4hgj0gdq9x7sxg7 BTC 1.3
        public AddInputTransaction() : base("addinput") { }
        protected override bool IsInput() => true;
    }
}