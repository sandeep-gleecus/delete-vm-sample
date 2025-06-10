namespace VMScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TableUpdates_v1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Schedules", "User");
            DropColumn("dbo.Schedules", "TemplateUNID");
            DropColumn("dbo.Schedules", "EmployeeNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Schedules", "EmployeeNumber", c => c.String());
            AddColumn("dbo.Schedules", "TemplateUNID", c => c.String());
            AddColumn("dbo.Schedules", "User", c => c.String());
        }
    }
}
