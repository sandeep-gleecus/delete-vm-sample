namespace VMScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTables : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Schedules", "TemplateId", c => c.Int(nullable: false));
            DropColumn("dbo.Schedules", "DocumentSetId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Schedules", "DocumentSetId", c => c.Guid(nullable: false));
            AlterColumn("dbo.Schedules", "TemplateId", c => c.Guid(nullable: false));
        }
    }
}
