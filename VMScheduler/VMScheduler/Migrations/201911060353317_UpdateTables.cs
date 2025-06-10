namespace VMScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTables : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ErrorLogs", newName: "ErrorLog");
            RenameTable(name: "dbo.Schedulers", newName: "Schedules");
            RenameTable(name: "dbo.UsageLogs", newName: "UsageLog");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.UsageLog", newName: "UsageLogs");
            RenameTable(name: "dbo.Schedules", newName: "Schedulers");
            RenameTable(name: "dbo.ErrorLog", newName: "ErrorLogs");
        }
    }
}
