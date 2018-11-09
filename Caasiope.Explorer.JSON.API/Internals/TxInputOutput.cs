namespace Caasiope.Explorer.JSON.API.Internals
{
    public class TxInputOutput
    {
        public string Address;
        public string Currency;
        public decimal Amount;
    }

    public class TxInput : TxInputOutput { }

    public class TxOutput : TxInputOutput { }
}