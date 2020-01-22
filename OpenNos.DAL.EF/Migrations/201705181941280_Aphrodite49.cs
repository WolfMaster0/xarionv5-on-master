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

    public partial class Aphrodite49 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Card", "Period", c => c.Short(nullable: false));
            DropColumn("dbo.Card", "BuffType");
            DropColumn("dbo.Card", "TimeoutBuffChance");
            DropColumn("dbo.Card", "TimeoutBuff");
            DropColumn("dbo.Card", "Delay");
        }

        public override void Up()
        {
            AddColumn("dbo.Card", "Delay", c => c.Int(nullable: false));
            AddColumn("dbo.Card", "TimeoutBuff", c => c.Short(nullable: false));
            AddColumn("dbo.Card", "TimeoutBuffChance", c => c.Byte(nullable: false));
            AddColumn("dbo.Card", "BuffType", c => c.Byte(nullable: false));
            DropColumn("dbo.Card", "Period");
        }

        #endregion
    }
}