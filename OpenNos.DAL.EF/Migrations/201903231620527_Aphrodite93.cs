namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite93 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PartnerSkills",
                c => new
                    {
                        PartnerVnum = c.Short(nullable: false),
                        FirstSkill = c.Short(nullable: false),
                        SecondSkill = c.Short(nullable: false),
                        ThirdSkill = c.Short(nullable: false),
                        SpecialBuffId = c.Short(nullable: false),
                        IdentifierKey = c.String(),
                    })
                .PrimaryKey(t => t.PartnerVnum);
            
            AddColumn("dbo.Character", "MaxPartnerCount", c => c.Byte(nullable: false));
            AddColumn("dbo.Character", "IsPetAutoRelive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "IsPartnerAutoRelive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItemInstance", "Agility", c => c.Byte(nullable: false));
            AddColumn("dbo.ItemInstance", "FirstPartnerSkill", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "SecondPartnerSkill", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "ThirdPartnerSkill", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "FirstPartnerSkillRank", c => c.Byte(nullable: false));
            AddColumn("dbo.ItemInstance", "SecondPartnerSkillRank", c => c.Byte(nullable: false));
            AddColumn("dbo.ItemInstance", "ThirdPartnerSkillRank", c => c.Byte(nullable: false));
            AddColumn("dbo.ItemInstance", "MinimumLevel", c => c.Byte(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseMinDamage", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseMaxDamage", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseConcentrate", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseHitRate", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseDefenceDodge", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseDistanceDefenceDodge", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseDistanceDefence", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseMagicDefence", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "BaseCloseDefence", c => c.Short(nullable: false));
            AddColumn("dbo.Item", "SpecialistType", c => c.Byte(nullable: false));
            AddColumn("dbo.NpcMonster", "SpecialistType", c => c.Byte(nullable: false));
            AddColumn("dbo.Mate", "MateSlot", c => c.Short(nullable: false));
            AddColumn("dbo.Mate", "PartnerSlot", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Mate", "PartnerSlot");
            DropColumn("dbo.Mate", "MateSlot");
            DropColumn("dbo.NpcMonster", "SpecialistType");
            DropColumn("dbo.Item", "SpecialistType");
            DropColumn("dbo.ItemInstance", "BaseCloseDefence");
            DropColumn("dbo.ItemInstance", "BaseMagicDefence");
            DropColumn("dbo.ItemInstance", "BaseDistanceDefence");
            DropColumn("dbo.ItemInstance", "BaseDistanceDefenceDodge");
            DropColumn("dbo.ItemInstance", "BaseDefenceDodge");
            DropColumn("dbo.ItemInstance", "BaseHitRate");
            DropColumn("dbo.ItemInstance", "BaseConcentrate");
            DropColumn("dbo.ItemInstance", "BaseMaxDamage");
            DropColumn("dbo.ItemInstance", "BaseMinDamage");
            DropColumn("dbo.ItemInstance", "MinimumLevel");
            DropColumn("dbo.ItemInstance", "ThirdPartnerSkillRank");
            DropColumn("dbo.ItemInstance", "SecondPartnerSkillRank");
            DropColumn("dbo.ItemInstance", "FirstPartnerSkillRank");
            DropColumn("dbo.ItemInstance", "ThirdPartnerSkill");
            DropColumn("dbo.ItemInstance", "SecondPartnerSkill");
            DropColumn("dbo.ItemInstance", "FirstPartnerSkill");
            DropColumn("dbo.ItemInstance", "Agility");
            DropColumn("dbo.Character", "IsPartnerAutoRelive");
            DropColumn("dbo.Character", "IsPetAutoRelive");
            DropColumn("dbo.Character", "MaxPartnerCount");
            DropTable("dbo.PartnerSkills");
        }
    }
}
