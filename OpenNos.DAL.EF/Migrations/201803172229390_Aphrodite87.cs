namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite87 : DbMigration
    {
        public override void Up() => AddColumn("dbo.ItemInstance", "ItemOptions", c => c.String());

        public override void Down() => DropColumn("dbo.ItemInstance", "ItemOptions");
    }
}
