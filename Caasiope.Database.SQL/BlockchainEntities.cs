using System.Data.Entity;
using Caasiope.Database.SQL.Entities;
using SQLite.CodeFirst;

namespace Caasiope.Database.SQL
{
    public class BlockchainEntities: DbContext
    {
        public BlockchainEntities() : base("Data Source=|DataDirectory|blockchain.db")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<BlockchainEntities>(modelBuilder);
            //System.Data.Entity.Database.SetInitializer(sqliteConnectionInitializer);

            modelBuilder.Entity<account>().HasKey(u => new
            {
                u.address
            });
            modelBuilder.Entity<ledger>().HasKey(u => new
            {
                u.height
            });
            modelBuilder.Entity<tableledgerheight>().HasKey(u => new
            {
                u.table_name
            });
            modelBuilder.Entity<ledgerstatechange>().HasKey(u => new
            {
                u.ledger_height
            });
        }

        public virtual DbSet<account> accounts { get; set; }
        public virtual DbSet<ledger> ledgers { get; set; }
        public virtual DbSet<ledgerstatechange> ledgerstatechanges { get; set; }
        public virtual DbSet<tableledgerheight> tableledgerheights { get; set; }
    }
}