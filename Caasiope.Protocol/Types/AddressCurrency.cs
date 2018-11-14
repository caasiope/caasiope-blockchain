namespace Caasiope.Protocol.Types
{
    public class AddressCurrency
    {
        public readonly Address Address;
        public readonly Currency Currency;

        public AddressCurrency(Address address, Currency currency)
        {
            Address = address;
            Currency = currency;
        }

        public override bool Equals(object obj)
        {
            var currency = obj as AddressCurrency;
            return currency != null && Currency.Equals(currency.Currency) && Address.Equals(currency.Address);
        }

        protected bool Equals(AddressCurrency other)
        {
            return Equals(Address, other.Address) && Equals(Currency, other.Currency);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Address.GetHashCode() * 397) ^ Currency.GetHashCode();
            }
        }
    }
}