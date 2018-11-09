using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Database.SQL.Entities
{
    public class timelock
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long declaration_id { get; set; }
        public byte[] account { get; set; }
        public long timestamp { get; set; }
    }
}