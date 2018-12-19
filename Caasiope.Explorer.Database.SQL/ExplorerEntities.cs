using System.Data.Entity;
using Caasiope.Explorer.Database.SQL.Entities;

namespace Caasiope.Explorer.Database.SQL
{
    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class ExplorerEntities: DbContext
    {
        public ExplorerEntities()
            : base("name=ExplorerEntities")
        {
            Configuration.LazyLoadingEnabled = false;
            System.Data.Entity.Database.SetInitializer<ExplorerEntities>(null);
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<account>().HasKey(u => new
            {
                u.address
            });
            modelBuilder.Entity<balance>().HasKey(u => new
            {
                u.account,
                u.currency
            });
            modelBuilder.Entity<transactioninputoutput>().HasKey(u => new
            {
                u.transaction_hash,
                u.index
            });
            modelBuilder.Entity<transaction>().HasKey(u => new
            {
                u.hash
            });
            modelBuilder.Entity<transactionsignature>().HasKey(u => new
            {
                u.transaction_hash,
                u.publickey
            });
            modelBuilder.Entity<transactiondeclaration>().HasKey(u => new
            {
                u.transaction_hash,
                u.index
            });
            modelBuilder.Entity<declaration>().HasKey(u => new
            {
                u.declaration_id,
            });
            modelBuilder.Entity<multisignatureaccount>().HasKey(u => new
            {
                u.declaration_id
            });
            modelBuilder.Entity<multisignaturesigner>().HasKey(u => new
            {
                u.multisignature_account,
                u.signer
            });
            modelBuilder.Entity<block>().HasKey(u => new
            {
                u.ledger_height
            });
            modelBuilder.Entity<transactionmessage>().HasKey(u => new
            {
                u.transaction_hash
            });
            modelBuilder.Entity<tableledgerheight>().HasKey(u => new
            {
                u.table_name
            });
            modelBuilder.Entity<hashlock>().HasKey(u => new
            {
                u.declaration_id
            });
            modelBuilder.Entity<timelock>().HasKey(u => new
            {
                u.declaration_id
            });
            modelBuilder.Entity<secretrevelation>().HasKey(u => new
            {
                u.declaration_id
            });
            modelBuilder.Entity<vendingmachine>().HasKey(u => new
            {
                u.declaration_id
            });
        }

        public virtual DbSet<account> accounts { get; set; }
        public virtual DbSet<balance> balances { get; set; }
        public virtual DbSet<transactioninputoutput> transactioninputoutputs { get; set; }
        public virtual DbSet<transaction> transactions { get; set; }
        public virtual DbSet<transactionsignature> transactionsignatures { get; set; }
        public virtual DbSet<multisignatureaccount> multisignatureaccounts { get; set; }
        public virtual DbSet<multisignaturesigner> multisignaturesigners { get; set; }
        public virtual DbSet<block> blocks { get; set; }
        public virtual DbSet<transactionmessage> transactionmessages { get; set; }
        public virtual DbSet<transactiondeclaration> transactiondeclarations { get; set; }
        public virtual DbSet<declaration> declarations { get; set; }
        public virtual DbSet<tableledgerheight> tableledgerheights { get; set; }
        public virtual DbSet<hashlock> hashlocks { get; set; }
        public virtual DbSet<timelock> timelocks { get; set; }
        public virtual DbSet<secretrevelation> secretrevelations { get; set; }
        public virtual DbSet<vendingmachine> vendingmachines { get; set; }
    }
}