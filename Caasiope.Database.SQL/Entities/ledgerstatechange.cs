using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Database.SQL.Entities
{
    public class ledgerstatechange
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long ledger_height { get; set; }
        public byte[] raw { get; set; }
    }
}