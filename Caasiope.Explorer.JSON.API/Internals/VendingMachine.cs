namespace Caasiope.Explorer.JSON.API.Internals
{
    public class VendingMachine : TxDeclaration
    {
        public readonly string Owner;
        public readonly string CurrencyIn;
        public readonly string CurrencyOut;
        public readonly decimal Rate;

        public VendingMachine(string owner, string currencyIn, string currencyOut, long rate)
        {
            Owner = owner;
            CurrencyIn = currencyIn;
            CurrencyOut = currencyOut;
            Rate = rate;
        }
    }
}