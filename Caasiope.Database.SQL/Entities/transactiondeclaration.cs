
namespace Caasiope.Database.SQL.Entities
{
    public class transactiondeclaration
    {
        public byte[] transaction_hash { get; set; }
        public byte index { get; set; }
        public long declaration_id { get; set; }
    }
}