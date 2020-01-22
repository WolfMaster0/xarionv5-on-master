namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite78 : DbMigration
    {
        public override void Up() => AddColumn("dbo.ItemInstance", "ShellRarity", c => c.Short());

        public override void Down() => DropColumn("dbo.ItemInstance", "ShellRarity");
    }
}
