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

    public partial class Aphrodite56 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.BCard", "Delay", c => c.Short(nullable: false));
            AddColumn("dbo.BCard", "IsDelayed", c => c.Boolean(nullable: false));
            DropColumn("dbo.BCard", "ThirdData");
            DropColumn("dbo.BCard", "CastType");
        }

        public override void Up()
        {
            AddColumn("dbo.BCard", "CastType", c => c.Byte(nullable: false));
            AddColumn("dbo.BCard", "ThirdData", c => c.Int(nullable: false));
            DropColumn("dbo.BCard", "IsDelayed");
            DropColumn("dbo.BCard", "Delay");
        }

        #endregion
    }
}