using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using Caasiope.Explorer.Database.SQL;

namespace Caasiope.Explorer.Database.Managers
{
    public static class WipeDatabaseHelper
    {
        public static void WipeDatabase()
        {
            using (var context = new ExplorerEntities())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                foreach (var tableName in objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName, DataSpace.CSpace).BaseEntitySets)
                    context.Database.ExecuteSqlCommand($"TRUNCATE {tableName.Name}");

                context.SaveChanges();
            }
        }
    }
}
