using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Explorer.Database.SQL.Entities
{
    public class multisignatureaccount
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long declaration_id { get; set; }
        public byte[] hash { get; set; }
        public byte[] account { get; set; }
        public short required { get; set; }
    }
}
