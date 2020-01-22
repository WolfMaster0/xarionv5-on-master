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

    public partial class Aphrodite48 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Card", "Type", c => c.Short(nullable: false));
            AddColumn("dbo.Card", "SubType", c => c.Byte(nullable: false));
            AddColumn("dbo.Card", "SecondData", c => c.Int(nullable: false));
            AddColumn("dbo.Card", "FirstData", c => c.Int(nullable: false));
            DropForeignKey("dbo.BCard", "CardId", "dbo.Card");
            DropIndex("dbo.BCard", new[] { "CardId" });
            DropTable("dbo.BCard");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.BCard",
                c => new
                {
                    BCardId = c.Short(nullable: false, identity: true),
                    SubType = c.Byte(nullable: false),
                    Type = c.Byte(nullable: false),
                    FirstData = c.Int(nullable: false),
                    SecondData = c.Int(nullable: false),
                    CardId = c.Short(nullable: false),
                    Delayed = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.BCardId)
                .ForeignKey("dbo.Card", t => t.CardId)
                .Index(t => t.CardId);

            DropColumn("dbo.Card", "FirstData");
            DropColumn("dbo.Card", "SecondData");
            DropColumn("dbo.Card", "SubType");
            DropColumn("dbo.Card", "Type");
        }

        #endregion
    }
}