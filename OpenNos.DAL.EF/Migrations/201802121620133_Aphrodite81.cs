namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite81 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.EventScript", "DateStartMonth", c => c.Byte(nullable: false));
            AddColumn("dbo.EventScript", "DateStartDay", c => c.Byte(nullable: false));
            AddColumn("dbo.EventScript", "DateEndMonth", c => c.Byte(nullable: false));
            AddColumn("dbo.EventScript", "DateEndDay", c => c.Byte(nullable: false));
            DropColumn("dbo.EventScript", "IsNewYearShift");
            DropColumn("dbo.EventScript", "DateStart");
            DropColumn("dbo.EventScript", "DateEnd");
        }

        public override void Up()
        {
            AddColumn("dbo.EventScript", "DateEnd", c => c.Int(nullable: false));
            AddColumn("dbo.EventScript", "DateStart", c => c.Int(nullable: false));
            AddColumn("dbo.EventScript", "IsNewYearShift", c => c.Boolean(nullable: false));
            DropColumn("dbo.EventScript", "DateEndDay");
            DropColumn("dbo.EventScript", "DateEndMonth");
            DropColumn("dbo.EventScript", "DateStartDay");
            DropColumn("dbo.EventScript", "DateStartMonth");
        }

        #endregion
    }
}