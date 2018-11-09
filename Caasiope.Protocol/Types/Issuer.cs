namespace Caasiope.Protocol.Types
{
    public class Issuer
    {
        public readonly Address Address;
        public readonly Currency Currency;

        public Issuer(Address address, Currency currency)
        {
            Currency = currency;
            Address = address;
        }
    }
}