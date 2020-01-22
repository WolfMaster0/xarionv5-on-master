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

    public partial class Aphrodite51 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                "dbo.SkillCard",
                c => new
                {
                    SkillVNum = c.Short(nullable: false),
                    CardId = c.Short(nullable: false),
                    CardChance = c.Short(nullable: false),
                })
                .PrimaryKey(t => new { t.SkillVNum, t.CardId });

            AddColumn("dbo.Skill", "SkillChance", c => c.Short(nullable: false));
            AddColumn("dbo.Skill", "SecondarySkillVNum", c => c.Short(nullable: false));
            AddColumn("dbo.Skill", "ElementalDamage", c => c.Short(nullable: false));
            AddColumn("dbo.Skill", "Damage", c => c.Short(nullable: false));
            DropForeignKey("dbo.BCard", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.BCard", "NpcMonsterVNum", "dbo.NpcMonster");
            DropIndex("dbo.BCard", new[] { "NpcMonsterVNum" });
            DropIndex("dbo.BCard", new[] { "SkillVNum" });
            DropIndex("dbo.BCard", new[] { "ItemVNum" });
            DropColumn("dbo.BCard", "NpcMonsterVNum");
            DropColumn("dbo.BCard", "SkillVNum");
            CreateIndex("dbo.SkillCard", "CardId");
            CreateIndex("dbo.SkillCard", "SkillVNum");
            CreateIndex("dbo.BCard", "ItemVnum");
            AddForeignKey("dbo.SkillCard", "SkillVNum", "dbo.Skill", "SkillVNum");
            AddForeignKey("dbo.SkillCard", "CardId", "dbo.Card", "CardId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.SkillCard", "CardId", "dbo.Card");
            DropForeignKey("dbo.SkillCard", "SkillVNum", "dbo.Skill");
            DropIndex("dbo.BCard", new[] { "ItemVnum" });
            DropIndex("dbo.SkillCard", new[] { "SkillVNum" });
            DropIndex("dbo.SkillCard", new[] { "CardId" });
            AddColumn("dbo.BCard", "SkillVNum", c => c.Short());
            AddColumn("dbo.BCard", "NpcMonsterVNum", c => c.Short());
            CreateIndex("dbo.BCard", "ItemVNum");
            CreateIndex("dbo.BCard", "SkillVNum");
            CreateIndex("dbo.BCard", "NpcMonsterVNum");
            AddForeignKey("dbo.BCard", "NpcMonsterVNum", "dbo.NpcMonster", "NpcMonsterVNum");
            AddForeignKey("dbo.BCard", "SkillVNum", "dbo.Skill", "SkillVNum");
            DropColumn("dbo.Skill", "Damage");
            DropColumn("dbo.Skill", "ElementalDamage");
            DropColumn("dbo.Skill", "SecondarySkillVNum");
            DropColumn("dbo.Skill", "SkillChance");
            DropTable("dbo.SkillCard");
        }

        #endregion
    }
}