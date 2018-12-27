namespace Caasiope.Explorer.JSON.API.Internals
{
    public class Order
    {
        public readonly char Side;
        public readonly decimal Size;
        public readonly decimal Price;

        public Order(char side, decimal size, decimal price)
        {
            Side = side;
            Size = size;
            Price = price;
        }
    }
}