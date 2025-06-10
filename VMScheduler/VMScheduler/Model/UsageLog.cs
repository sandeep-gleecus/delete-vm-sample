using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using VMScheduler.Data;

namespace VMScheduler.Model
{
	[Table("TST_USAGE_LOG")]
	public class UsageLog : LogRepository<UsageLog>
	{
		[Key]
		public int LogId { get; set; }
		public int ScheduleId { get; set; }
		public int TemplateId { get; set; }
		public string TemplateName { get; set; }
		public string LookupKeys { get; set; }
		public Guid ScheduleGroupId { get; set; }
		public DateTime ScheduledDateTime { get; set; }
		public string LogAction { get; set; }
		public string ActionResult { get; set; }
		public DateTime CreatedDateTime { get; set; }
		public string User { get; set; }

		public void AddNewEntry()
		{
			Add(this);
			var x = SaveChanges();
		}
	}

}
