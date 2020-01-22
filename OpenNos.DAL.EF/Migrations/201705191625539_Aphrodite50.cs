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

    public partial class Aphrodite50 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                "dbo.ItemCard",
                c => new
                {
                    ItemVNum = c.Short(nullable: false),
                    CardId = c.Short(nullable: false),
                    CardChance = c.Short(nullable: false),
                })
                .PrimaryKey(t => new { t.ItemVNum, t.CardId });

            DropForeignKey("dbo.BCard", "ItemVnum", "dbo.Item");
            DropIndex("dbo.BCard", new[] { "ItemVnum" });
            DropIndex("dbo.BCard", new[] { "CardId" });
            AlterColumn("dbo.BCard", "CardId", c => c.Short(nullable: false));
            DropColumn("dbo.BCard", "ItemVnum");
            CreateIndex("dbo.ItemCard", "CardId");
            CreateIndex("dbo.ItemCard", "ItemVNum");
            CreateIndex("dbo.BCard", "CardId");
            AddForeignKey("dbo.ItemCard", "ItemVNum", "dbo.Item", "VNum");
            AddForeignKey("dbo.ItemCard", "CardId", "dbo.Card", "CardId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.ItemCard", "CardId", "dbo.Card");
            DropForeignKey("dbo.ItemCard", "ItemVNum", "dbo.Item");
            DropIndex("dbo.BCard", new[] { "CardId" });
            DropIndex("dbo.ItemCard", new[] { "ItemVNum" });
            DropIndex("dbo.ItemCard", new[] { "CardId" });
            AddColumn("dbo.BCard", "ItemVnum", c => c.Short());
            AlterColumn("dbo.BCard", "CardId", c => c.Short());
            CreateIndex("dbo.BCard", "CardId");
            CreateIndex("dbo.BCard", "ItemVnum");
            AddForeignKey("dbo.BCard", "ItemVnum", "dbo.Item", "VNum");
            DropTable("dbo.ItemCard");
        }

        #endregion
    }
}