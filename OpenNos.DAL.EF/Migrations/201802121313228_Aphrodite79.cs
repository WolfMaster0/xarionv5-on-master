namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite79 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventScript",
                c => new
                    {
                        EventScriptId = c.Int(nullable: false, identity: true),
                        DateEnd = c.String(maxLength: 5),
                        DateStart = c.String(maxLength: 5),
                        Script = c.String(),
                    })
                .PrimaryKey(t => t.EventScriptId);
        }

        public override void Down() => DropTable("dbo.EventScript");
    }
}
