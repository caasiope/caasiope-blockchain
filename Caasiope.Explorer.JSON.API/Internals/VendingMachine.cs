using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class VendingMachine : TxAddressDeclaration
    {
        public string Owner;
        public string CurrencyIn;
        public string CurrencyOut;
        public decimal Rate;

        public VendingMachine(string owner, string currencyIn, string currencyOut, decimal rate, string address) : this()
        {
            Owner = owner;
            CurrencyIn = currencyIn;
            CurrencyOut = currencyOut;
            Rate = rate;
            Address = address;
        }

        public VendingMachine() : base((byte)DeclarationType.VendingMachine) { }
    }
}