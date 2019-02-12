namespace Caasiope.Protocol.Types
{
    public class VendingMachineAccount
    {
        public readonly Address Address;
        public readonly Address Owner;
        public readonly Currency CurrencyIn;
        public readonly Currency CurrencyOut;
        public readonly Amount Rate;

        public VendingMachineAccount(Address address, Address owner, Currency currencyIn, Currency currencyOut, Amount rate)
        {
            Address = address;
            Owner = owner;
            CurrencyIn = currencyIn;
            CurrencyOut = currencyOut;
            Rate = rate;
        }
    }
}