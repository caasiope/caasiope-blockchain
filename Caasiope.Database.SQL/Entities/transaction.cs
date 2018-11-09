namespace Caasiope.Database.SQL.Entities
{
    public class transaction
    {
        public byte[] hash { get; set; }

        public long ledger_height { get; set; }

        public long expire { get; set; }
    }
}