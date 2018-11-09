namespace Caasiope.Database.SQL.Entities
{
    public class ledgersignature
    {
        public long ledger_height { get; set; }
        public byte[] validator_publickey { get; set; }
        public byte[] validator_signature { get; set; }
    }
}