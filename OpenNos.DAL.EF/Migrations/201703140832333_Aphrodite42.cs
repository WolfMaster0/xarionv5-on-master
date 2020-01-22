// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// conditions found in the LICENSE file.
// 
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR
// CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE.
namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite42 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.TimeSpace", "MapId", "dbo.Map");
            DropIndex("dbo.TimeSpace", new[] { "MapId" });
            DropTable("dbo.TimeSpace");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.TimeSpace",
                c => new
                {
                    TimespaceId = c.Short(nullable: false, identity: true),
                    MapId = c.Short(nullable: false),
                    PositionX = c.Short(nullable: false),
                    PositionY = c.Short(nullable: false),
                    LevelMinimum = c.Int(nullable: false),
                    LevelMaximum = c.Int(nullable: false),
                    Winner = c.String(),
                    DrawItemGift = c.String(),
                    BonusItemGift = c.String(),
                    SpecialItemGift = c.String(),
                    Label = c.String(),
                    WinnerScore = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.TimespaceId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .Index(t => t.MapId);
        }

        #endregion
    }
}