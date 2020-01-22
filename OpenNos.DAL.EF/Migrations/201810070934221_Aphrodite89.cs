namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite89 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "TotpSecret", c => c.String(maxLength: 32));
            AddColumn("dbo.Account", "TotpResetPassword", c => c.String(maxLength: 255));
        }

        public override void Down()
        {
            DropColumn("dbo.Account", "TotpResetPassword");
            DropColumn("dbo.Account", "TotpSecret");
        }
    }
}
