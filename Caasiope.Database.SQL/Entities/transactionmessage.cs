namespace Caasiope.Database.SQL.Entities
{
    public class transactionmessage
    {
        public byte[] transaction_hash { get; set; }

        public byte[] message { get; set; }
    }
}