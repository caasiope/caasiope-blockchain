using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class VendingMachine : TxDeclaration
    {
        public string Owner;
        public string CurrencyIn;
        public string CurrencyOut;
        public decimal Rate;

        public VendingMachine(string owner, string currencyIn, string currencyOut, long rate) : this()
        {
            Owner = owner;
            CurrencyIn = currencyIn;
            CurrencyOut = currencyOut;
            Rate = rate;
        }

        public VendingMachine() : base((byte)DeclarationType.VendingMachine) { }
    }
}