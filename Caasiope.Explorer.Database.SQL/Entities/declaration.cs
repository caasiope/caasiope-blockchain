using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Explorer.Database.SQL.Entities
{
    public class declaration
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long declaration_id { get; set; }
        public byte type { get; set; }
    }
}
