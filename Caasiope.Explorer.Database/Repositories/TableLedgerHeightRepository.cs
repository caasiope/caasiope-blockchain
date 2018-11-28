using System.Data.Entity;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;

namespace Caasiope.Explorer.Database.Repositories
{
    public class TableLedgerHeightRepository : Repository<TableLedgerHeight, tableledgerheight, string>
    {
        protected override string GetKey(TableLedgerHeight item)
        {
            return item.TableName;
        }

        protected override tableledgerheight ToEntity(TableLedgerHeight item)
        {
            return new tableledgerheight
            {
                table_name = item.TableName,
                processed_ledger_height = item.Height
            };
        }

        protected override TableLedgerHeight ToItem(tableledgerheight entity)
        {
            return new TableLedgerHeight(entity.table_name, entity.processed_ledger_height);
        }

        protected override DbSet<tableledgerheight> GetDbSet(ExplorerEntities entities)
        {
            return entities.tableledgerheights;
        }
    }
}
