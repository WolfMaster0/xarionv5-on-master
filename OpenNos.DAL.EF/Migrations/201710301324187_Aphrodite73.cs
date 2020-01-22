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

    public partial class Aphrodite73 : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.QuestProgressId)
                .ForeignKey("dbo.Quest", t => t.QuestId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.QuestId)
                .Index(t => t.CharacterId);

            CreateTable(
                "dbo.Quest",
                c => new
                    {
                        QuestId = c.Long(nullable: false, identity: true),
                        QuestData = c.String(),
                    })
                .PrimaryKey(t => t.QuestId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.QuestProgress", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.QuestProgress", "QuestId", "dbo.Quest");
            DropIndex("dbo.QuestProgress", new[] { "CharacterId" });
            DropIndex("dbo.QuestProgress", new[] { "QuestId" });
            DropTable("dbo.Quest");
            DropTable("dbo.QuestProgress");
        }
    }
}
