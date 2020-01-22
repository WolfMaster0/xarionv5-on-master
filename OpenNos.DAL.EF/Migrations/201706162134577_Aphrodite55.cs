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

    public partial class Aphrodite55 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.RollGeneratedItem", "OriginalItemVNum", "dbo.Item");
            DropForeignKey("dbo.RollGeneratedItem", "ItemGeneratedVNum", "dbo.Item");
            DropIndex("dbo.RollGeneratedItem", new[] { "ItemGeneratedVNum" });
            DropIndex("dbo.RollGeneratedItem", new[] { "OriginalItemVNum" });
            DropTable("dbo.RollGeneratedItem");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.RollGeneratedItem",
                c => new
                {
                    RollGeneratedItemId = c.Short(nullable: false, identity: true),
                    OriginalItemDesign = c.Short(nullable: false),
                    OriginalItemVNum = c.Short(nullable: false),
                    Probability = c.Short(nullable: false),
                    ItemGeneratedAmount = c.Byte(nullable: false),
                    ItemGeneratedVNum = c.Short(nullable: false),
                    IsRareRandom = c.Boolean(nullable: false),
                    MinimumOriginalItemRare = c.Byte(nullable: false),
                    MaximumOriginalItemRare = c.Byte(nullable: false),
                })
                .PrimaryKey(t => t.RollGeneratedItemId)
                .ForeignKey("dbo.Item", t => t.ItemGeneratedVNum)
                .ForeignKey("dbo.Item", t => t.OriginalItemVNum)
                .Index(t => t.OriginalItemVNum)
                .Index(t => t.ItemGeneratedVNum);
        }

        #endregion
    }
}