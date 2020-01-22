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
    public partial class Aphrodite30 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                "dbo.WarehouseItem",
                c => new
                {
                    WarehouseItemId = c.Long(nullable: false, identity: true),
                    AccountId = c.Long(),
                    FamilyId = c.Long(),
                    ItemInstanceId = c.Guid(nullable: false)
                })
                .PrimaryKey(t => t.WarehouseItemId);

            CreateIndex("dbo.WarehouseItem", "ItemInstanceId");
            CreateIndex("dbo.WarehouseItem", "FamilyId");
            CreateIndex("dbo.WarehouseItem", "AccountId");
            AddForeignKey("dbo.WarehouseItem", "AccountId", "dbo.Account", "AccountId");
            AddForeignKey("dbo.WarehouseItem", "ItemInstanceId", "dbo.ItemInstance", "Id");
            AddForeignKey("dbo.WarehouseItem", "FamilyId", "dbo.Family", "FamilyId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.WarehouseItem", "FamilyId", "dbo.Family");
            DropForeignKey("dbo.WarehouseItem", "ItemInstanceId", "dbo.ItemInstance");
            DropForeignKey("dbo.WarehouseItem", "AccountId", "dbo.Account");
            DropIndex("dbo.WarehouseItem", new[] { "AccountId" });
            DropIndex("dbo.WarehouseItem", new[] { "FamilyId" });
            DropIndex("dbo.WarehouseItem", new[] { "ItemInstanceId" });
            DropTable("dbo.WarehouseItem");
        }

        #endregion
    }
}