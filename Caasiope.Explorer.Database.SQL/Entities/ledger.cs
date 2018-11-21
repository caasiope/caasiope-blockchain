using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Explorer.Database.SQL.Entities
{
    public class ledger
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long height { get; set; }
        public long timestamp { get; set; }
        public byte[] hash { get; set; }
        public byte[] merkle_root_hash { get; set; }
        public byte[] previous_hash { get; set; }
        public byte version { get; set; }
        public byte[] raw { get; set; }
    }
}
