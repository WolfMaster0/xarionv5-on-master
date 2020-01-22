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
    public partial class Aphrodite14 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Character", "FamilyCharacterId", c => c.Long());
            CreateIndex("dbo.Character", "FamilyCharacterId");
            AddForeignKey("dbo.Character", "FamilyCharacterId", "dbo.FamilyCharacter", "FamilyCharacterId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.Character", "FamilyCharacterId", "dbo.FamilyCharacter");
            DropIndex("dbo.Character", new[] { "FamilyCharacterId" });
            DropColumn("dbo.Character", "FamilyCharacterId");
        }

        #endregion
    }
}