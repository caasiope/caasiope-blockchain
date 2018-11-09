namespace Caasiope.Database.SQL.Entities
{
    public class transactioninputoutput
    {
        public byte[] transaction_hash { get; set; }
        public byte index { get; set; }
        public bool is_input { get; set; }
        public byte[] account { get; set; }
        public short currency { get; set; }
        public long amount { get; set; }
    }
}