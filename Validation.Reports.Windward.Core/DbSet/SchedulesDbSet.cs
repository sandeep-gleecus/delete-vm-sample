using System.Data.Entity;
using Validation.Reports.Windward.Core.Model;

namespace Validation.Reports.Windward.Core.DbSet
{
    public class SchedulesDbSet : DbContext
    {

        public SchedulesDbSet()
           : base("DefaultConnection")
        {//ScheduleConnection
        }

        public DbSet<Schedule> Schedules { get; set; }
    }
}