namespace Caasiope.Explorer.Database.SQL.Entities
{
    public class transactionsignature
    {
        public byte[] transaction_hash { get; set; }

        public byte[] publickey { get; set; }

        public byte[] signature { get; set; }
    }
}
