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
    public partial class Aphrodite7 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.BazaarItem", "ItemInstanceId", "dbo.ItemInstance");
            DropForeignKey("dbo.BazaarItem", "SellerId", "dbo.Character");
            DropIndex("dbo.BazaarItem", new[] { "SellerId" });
            DropIndex("dbo.BazaarItem", new[] { "ItemInstanceId" });
            DropColumn("dbo.ItemInstance", "BazaarItemId");
            DropTable("dbo.BazaarItem");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.BazaarItem",
                c => new
                {
                    BazaarItemId = c.Long(nullable: false, identity: true),
                    DateStart = c.DateTime(nullable: false),
                    Duration = c.Short(nullable: false),
                    ItemInstanceId = c.Guid(nullable: false),
                    Price = c.Long(nullable: false),
                    SellerId = c.Long(nullable: false)
                })
                .PrimaryKey(t => t.BazaarItemId)
                .ForeignKey("dbo.Character", t => t.SellerId)
                .ForeignKey("dbo.ItemInstance", t => t.ItemInstanceId)
                .Index(t => t.ItemInstanceId)
                .Index(t => t.SellerId);

            AddColumn("dbo.ItemInstance", "BazaarItemId", c => c.Long());
        }

        #endregion
    }
}