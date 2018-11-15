namespace Caasiope.Database.SQL.Entities
{
    public class account
    {
        public byte[] address { get; set; }
        public long current_ledger_height { get; set; }
        public bool is_declared { get; set; }
    }
}