namespace VMScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SchedulerContext_Changed : DbMigration
    {
        public override void Up()
        {
            //RenameTable(name: "dbo.Schedules", newName: "Schedulers");
            //AddColumn("dbo.Schedulers", "UserFolder", c => c.String());
            //AddColumn("dbo.Schedulers", "OutputFileName", c => c.String());
        }
        
        public override void Down()
        {
            //DropColumn("dbo.Schedulers", "OutputFileName");
            //DropColumn("dbo.Schedulers", "UserFolder");
            //RenameTable(name: "dbo.Schedulers", newName: "Schedules");
        }
    }
}
