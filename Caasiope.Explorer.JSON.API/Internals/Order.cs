namespace Caasiope.Explorer.JSON.API.Internals
{
    public class Order
    {
        public readonly char Side;
        public readonly decimal Size;
        public readonly decimal Price;
        public readonly string Address;

        public Order(char side, decimal size, decimal price, string address)
        {
            Side = side;
            Size = size;
            Price = price;
            Address = address;
        }
    }
}