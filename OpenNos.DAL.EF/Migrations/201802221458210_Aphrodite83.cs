namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite83 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuestProgress", "QuestId", "dbo.Quest");
            DropForeignKey("dbo.QuestProgress", "CharacterId", "dbo.Character");
            DropIndex("dbo.QuestProgress", new[] { "QuestId" });
            DropIndex("dbo.QuestProgress", new[] { "CharacterId" });
            DropTable("dbo.QuestProgress");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.QuestProgress",
                c => new
                    {
                        QuestProgressId = c.Long(nullable: false, identity: true),
                        QuestId = c.Long(nullable: false),
                        QuestData = c.String(),
                        CharacterId = c.Long(nullable: false),
                        IsFinished = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.QuestProgressId);

            CreateIndex("dbo.QuestProgress", "CharacterId");
            CreateIndex("dbo.QuestProgress", "QuestId");
            AddForeignKey("dbo.QuestProgress", "CharacterId", "dbo.Character", "CharacterId");
            AddForeignKey("dbo.QuestProgress", "QuestId", "dbo.Quest", "QuestId");
        }
    }
}
