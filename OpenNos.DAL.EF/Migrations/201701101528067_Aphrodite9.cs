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
using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite9 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.StaticBonus", "CharacterId", "dbo.Character");
            DropIndex("dbo.StaticBonus", new[] { "CharacterId" });
            DropTable("dbo.StaticBonus");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.StaticBonus",
                c => new
                {
                    StaticBonusId = c.Long(nullable: false, identity: true),
                    CharacterId = c.Long(nullable: false),
                    DateEnd = c.DateTime(nullable: false),
                    StaticBonusType = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.StaticBonusId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);
        }

        #endregion
    }
}