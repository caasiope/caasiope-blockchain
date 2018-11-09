using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Database.SQL.Entities
{
    public class hashlock
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long declaration_id { get; set; }
        public byte[] account { get; set; }
        public short secret_type { get; set; }
        public byte[] secret_hash { get; set; }
    }
}