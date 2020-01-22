namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite90 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "TotpVerified", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "TotpVerified");
        }
    }
}
