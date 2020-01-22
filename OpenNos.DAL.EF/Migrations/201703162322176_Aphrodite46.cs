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

    public partial class Aphrodite46 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                "dbo.TimeSpace",
                c => new
                {
                    TimespaceId = c.Short(nullable: false, identity: true),
                    MapId = c.Short(nullable: false),
                    PositionX = c.Short(nullable: false),
                    PositionY = c.Short(nullable: false),
                    Winner = c.String(maxLength: 255),
                    Script = c.String(),
                    WinnerScore = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.TimespaceId);

            DropForeignKey("dbo.ScriptedInstance", "MapId", "dbo.Map");
            DropIndex("dbo.ScriptedInstance", new[] { "MapId" });
            DropTable("dbo.ScriptedInstance");
            CreateIndex("dbo.TimeSpace", "MapId");
            AddForeignKey("dbo.TimeSpace", "MapId", "dbo.Map", "MapId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.TimeSpace", "MapId", "dbo.Map");
            DropIndex("dbo.TimeSpace", new[] { "MapId" });
            CreateTable(
                "dbo.ScriptedInstance",
                c => new
                {
                    ScriptedInstanceId = c.Short(nullable: false, identity: true),
                    Type = c.Byte(nullable: false),
                    MapId = c.Short(nullable: false),
                    PositionX = c.Short(nullable: false),
                    PositionY = c.Short(nullable: false),
                    Winner = c.String(maxLength: 255),
                    Script = c.String(),
                    WinnerScore = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ScriptedInstanceId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .Index(t => t.MapId);

            DropTable("dbo.TimeSpace");
        }

        #endregion
    }
}