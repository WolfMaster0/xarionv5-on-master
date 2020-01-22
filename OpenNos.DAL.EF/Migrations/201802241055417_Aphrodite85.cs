namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite85 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "NextQuestId", c => c.Long(nullable: false));
            AlterColumn("dbo.EventScript", "DateEnd", c => c.String(maxLength: 5));
            AlterColumn("dbo.EventScript", "DateStart", c => c.String(maxLength: 5));
        }

        public override void Down()
        {
            AlterColumn("dbo.EventScript", "DateStart", c => c.Int(nullable: false));
            AlterColumn("dbo.EventScript", "DateEnd", c => c.Int(nullable: false));
            DropColumn("dbo.Quest", "NextQuestId");
        }
    }
}
