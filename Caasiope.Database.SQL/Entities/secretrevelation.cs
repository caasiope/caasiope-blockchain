using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Database.SQL.Entities
{
    public class secretrevelation
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long declaration_id { get; set; }
        public byte[] secret { get; set; }
    }
}