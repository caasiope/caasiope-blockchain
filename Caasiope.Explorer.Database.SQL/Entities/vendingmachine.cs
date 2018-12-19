using System.ComponentModel.DataAnnotations.Schema;

namespace Caasiope.Explorer.Database.SQL.Entities
{
    public class vendingmachine
    {
        [DatabaseGe‌​nerated(DatabaseGen‌​eratedOption.None)]
        public long declaration_id { get; set; }
        public byte[] account { get; set; }
        public byte[] owner { get; set; }
        public short currency_in { get; set; }
        public short currency_out { get; set; }
        public long rate { get; set; }
    }
}