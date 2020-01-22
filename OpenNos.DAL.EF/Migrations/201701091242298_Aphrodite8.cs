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
    public partial class Aphrodite8 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.BazaarItem", "Amount");
            DropColumn("dbo.BazaarItem", "IsPackage");
            DropColumn("dbo.BazaarItem", "MedalUsed");
        }

        public override void Up()
        {
            AddColumn("dbo.BazaarItem", "MedalUsed", c => c.Boolean(nullable: false));
            AddColumn("dbo.BazaarItem", "IsPackage", c => c.Boolean(nullable: false));
            AddColumn("dbo.BazaarItem", "Amount", c => c.Byte(nullable: false));
        }

        #endregion
    }
}