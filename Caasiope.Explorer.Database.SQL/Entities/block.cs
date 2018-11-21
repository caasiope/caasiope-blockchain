using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Explorer.Database.SQL.Entities
{
    public class block
    {
        public byte[] hash { get; set; }
        // TODO It must be ulong. Unsigned types are not supported by EF
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long ledger_height { get; set; }
        // TODO It must be ushort. Unsigned types are not supported by EF
        public short? fee_transaction_index { get; set; }
    }
}
