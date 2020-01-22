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

    public partial class Aphrodite71 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Recipe", "MapNpcId", c => c.Int(nullable: false));
            DropForeignKey("dbo.RecipeList", "RecipeId", "dbo.Recipe");
            DropForeignKey("dbo.RecipeList", "MapNpcId", "dbo.MapNpc");
            DropForeignKey("dbo.RecipeList", "ItemVNum", "dbo.Item");
            DropIndex("dbo.RecipeList", new[] { "RecipeId" });
            DropIndex("dbo.RecipeList", new[] { "MapNpcId" });
            DropIndex("dbo.RecipeList", new[] { "ItemVNum" });
            DropTable("dbo.RecipeList");
            CreateIndex("dbo.Recipe", "MapNpcId");
            AddForeignKey("dbo.Recipe", "MapNpcId", "dbo.MapNpc", "MapNpcId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.Recipe", "MapNpcId", "dbo.MapNpc");
            DropIndex("dbo.Recipe", new[] { "MapNpcId" });
            CreateTable(
                "dbo.RecipeList",
                c => new
                {
                    RecipeListId = c.Int(nullable: false, identity: true),
                    ItemVNum = c.Short(),
                    MapNpcId = c.Int(),
                    RecipeId = c.Short(nullable: false),
                })
                .PrimaryKey(t => t.RecipeListId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .ForeignKey("dbo.MapNpc", t => t.MapNpcId)
                .ForeignKey("dbo.Recipe", t => t.RecipeId)
                .Index(t => t.ItemVNum)
                .Index(t => t.MapNpcId)
                .Index(t => t.RecipeId);
            DropColumn("dbo.Recipe", "MapNpcId");
        }

        #endregion
    }
}