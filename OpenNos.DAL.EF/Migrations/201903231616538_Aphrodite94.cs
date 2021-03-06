﻿namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite94 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "AllowRevivalPartner", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "AllowRevivalPet", c => c.Boolean(nullable: false));
            DropColumn("dbo.Character", "AllowRevivalPet");
            DropColumn("dbo.Character", "AllowRevivalPartner");
        }
        
        public override void Down()
        {
            
        }
    }
}
