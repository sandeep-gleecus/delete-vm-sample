//using Microsoft.EntityFrameworkCore;
using System.Data.Entity;

using VMScheduler.Model;

namespace VMScheduler.Data
{
	public class SchedulerContext : DbContext
	{
		public SchedulerContext() : base("DbConnectionScheduler") { }

		public DbSet<Scheduler> Schedules { get; set; }
		public DbSet<UsageLog> UsageLogs { get; set; }
		public DbSet<ErrorLog> ErrorLogs { get; set; }
	}
}
