﻿namespace VMScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SchedulerContext_Changed2 : DbMigration
    {
        public override void Up()
        {
            //RenameTable(name: "dbo.Schedulers", newName: "Schedules");
        }
        
        public override void Down()
        {
            //RenameTable(name: "dbo.Schedules", newName: "Schedulers");
        }
    }
}
