using System.Data.Entity;

using Validation.Reports.Windward.Core.Model;

namespace Validation.Reports.Windward.Core.DbSet
{
    public class TemplateDbSet : DbContext
    {
        public TemplateDbSet() : base("DefaultConnection")
        {
            //this.Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Template> Templates { get; set; }
        public DbSet<TemplateParameter> Parameters { get; set; }
        public DbSet<TemplateDataSource> DataSources { get; set; }
        public DbSet<TemplateOutputType> OutputTypes { get; set; }
    }

    public class ReportingDbSet : DbContext
    {
        public ReportingDbSet() : base("ReportingConnection")
        {
            //this.Configuration.ProxyCreationEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<ReportingDbSet>(null);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TestSet> TestSets { get; set; }
        public DbSet<TestRun> TestRuns { get; set; }
        public DbSet<TestCase> TestCases { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Project> Projects { get; set; }
    }
}
