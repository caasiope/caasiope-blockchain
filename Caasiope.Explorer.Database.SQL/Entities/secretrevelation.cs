using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Explorer.Database.SQL.Entities
{
    public class secretrevelation
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long declaration_id { get; set; }
        public byte[] secret { get; set; }
    }
}
