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

    public partial class Aphrodite68 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                "dbo.CellonOption",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Level = c.Byte(nullable: false),
                    Type = c.Byte(nullable: false),
                    Value = c.Int(nullable: false),
                    EquipmentSerialId = c.Guid(nullable: false),
                    WearableInstance_Id = c.Guid(),
                })
                .PrimaryKey(t => t.Id);

            CreateIndex("dbo.CellonOption", "WearableInstance_Id");
            AddForeignKey("dbo.CellonOption", "WearableInstance_Id", "dbo.ItemInstance", "Id");
        }

        public override void Up()
        {
            DropForeignKey("dbo.CellonOption", "WearableInstance_Id", "dbo.ItemInstance");
            DropIndex("dbo.CellonOption", new[] { "WearableInstance_Id" });
            DropTable("dbo.CellonOption");
        }

        #endregion
    }
}