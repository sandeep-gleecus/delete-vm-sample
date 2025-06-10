namespace VMScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ErrorLog",
                c => new
                    {
                        ErrorLogId = c.Int(nullable: false, identity: true),
                        ErrorMessage = c.String(),
                        ErrorContext = c.String(),
                        CreatedDateTimeUtc = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        CallingApplication = c.String(),
                    })
                .PrimaryKey(t => t.ErrorLogId);
            
            CreateTable(
                "dbo.Schedules",
                c => new
                    {
                        ScheduleId = c.Int(nullable: false, identity: true),
                        ScheduleGroupId = c.Guid(nullable: false),
                        TemplateId = c.Int(nullable: false),
                        TemplateName = c.String(),
                        Parameters = c.String(),
                        OutputType = c.String(),
                        ScheduledTime = c.DateTime(nullable: false),
                        User = c.String(),
                        DeliveryType = c.String(),
                        Status = c.String(),
                        TemplateUNID = c.String(),
                        EmployeeNumber = c.String(),
                        CallingApp = c.String(),
                        DeliveryLocation = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ScheduleId);
            
            CreateTable(
                "dbo.UsageLog",
                c => new
                    {
                        LogId = c.Int(nullable: false, identity: true),
                        ScheduleId = c.Int(nullable: false),
                        TemplateId = c.Int(nullable: false),
                        TemplateName = c.String(),
                        LookupKeys = c.String(),
                        ScheduleGroupId = c.Guid(nullable: false),
                        ScheduledDateTime = c.DateTime(nullable: false),
                        LogAction = c.String(),
                        ActionResult = c.String(),
                        CreatedDateTime = c.DateTime(nullable: false),
                        User = c.String(),
                    })
                .PrimaryKey(t => t.LogId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UsageLog");
            DropTable("dbo.Schedules");
            DropTable("dbo.ErrorLog");
        }
    }
}
