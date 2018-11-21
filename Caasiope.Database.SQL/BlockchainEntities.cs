using System.Data.Entity;
using Caasiope.Database.SQL.Entities;

namespace Caasiope.Database.SQL
{
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class BlockchainEntities: DbContext
    {
        public BlockchainEntities()
            : base("name=BlockchainEntities")
        {
            Configuration.LazyLoadingEnabled = false;
            System.Data.Entity.Database.SetInitializer<BlockchainEntities>(null);
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<account>().HasKey(u => new
            {
                u.address
            });

            modelBuilder.Entity<transactiondeclaration>().HasKey(u => new
            {
                account = u.address
            });
            modelBuilder.Entity<ledger>().HasKey(u => new
            {
                u.height
            });
            modelBuilder.Entity<ledgersignature>().HasKey(u => new
            {
                u.ledger_height,
                u.validator_publickey,
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
        public virtual DbSet<transactiondeclaration> transactiondeclarations { get; set; }
        public virtual DbSet<ledger> ledgers { get; set; }
        public virtual DbSet<ledgersignature> ledgersignatures { get; set; }
        public virtual DbSet<tableledgerheight> tableledgerheights { get; set; }
        public virtual DbSet<ledgerstatechange> ledgerstatechanges { get; set; }
    }
}