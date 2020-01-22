namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite82 : DbMigration
    {
        #region Methods

        public override void Down() => AddColumn("dbo.EventScript", "IsNewYearShift", c => c.Boolean(nullable: false));

        public override void Up() => DropColumn("dbo.EventScript", "IsNewYearShift");

        #endregion
    }
}