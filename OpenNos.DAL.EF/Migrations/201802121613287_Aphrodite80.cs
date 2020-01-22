namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite80 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.EventScript", "DateStart", c => c.String(maxLength: 5));
            AddColumn("dbo.EventScript", "DateEnd", c => c.String(maxLength: 5));
            DropColumn("dbo.EventScript", "DateStartMonth");
            DropColumn("dbo.EventScript", "DateStartDay");
            DropColumn("dbo.EventScript", "DateEndMonth");
            DropColumn("dbo.EventScript", "DateEndDay");
        }

        public override void Up()
        {
            AddColumn("dbo.EventScript", "DateEndDay", c => c.Byte(nullable: false));
            AddColumn("dbo.EventScript", "DateEndMonth", c => c.Byte(nullable: false));
            AddColumn("dbo.EventScript", "DateStartDay", c => c.Byte(nullable: false));
            AddColumn("dbo.EventScript", "DateStartMonth", c => c.Byte(nullable: false));
            DropColumn("dbo.EventScript", "DateEnd");
            DropColumn("dbo.EventScript", "DateStart");
        }

        #endregion
    }
}