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
    public partial class Aphrodite21 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.MinilandObject", "MinilandObjectVNum", c => c.Short(nullable: false));
            DropForeignKey("dbo.MinilandObject", "ItemInstanceId", "dbo.ItemInstance");
            DropIndex("dbo.MinilandObject", new[] { "ItemInstanceId" });
            DropColumn("dbo.MinilandObject", "ItemInstanceId");
            CreateIndex("dbo.MinilandObject", "MinilandObjectVNum");
            AddForeignKey("dbo.MinilandObject", "MinilandObjectVNum", "dbo.Item", "VNum");
        }

        public override void Up()
        {
            DropForeignKey("dbo.MinilandObject", "MinilandObjectVNum", "dbo.Item");
            DropIndex("dbo.MinilandObject", new[] { "MinilandObjectVNum" });
            AddColumn("dbo.MinilandObject", "ItemInstanceId", c => c.Guid());
            CreateIndex("dbo.MinilandObject", "ItemInstanceId");
            AddForeignKey("dbo.MinilandObject", "ItemInstanceId", "dbo.ItemInstance", "Id");
            DropColumn("dbo.MinilandObject", "MinilandObjectVNum");
        }

        #endregion
    }
}