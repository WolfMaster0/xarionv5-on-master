namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite86 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "AllowRevivalPet", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "AllowRevivalPartner", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Character", "AllowRevivalPartner");
            DropColumn("dbo.Character", "AllowRevivalPet");
        }
    }
}
