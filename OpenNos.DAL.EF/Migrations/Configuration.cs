using System.Data.Entity.Migrations;
using OpenNos.DAL.EF.Context;

namespace OpenNos.DAL.EF.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<OpenNosContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(OpenNosContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
