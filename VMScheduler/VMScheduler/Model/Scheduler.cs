using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using VMScheduler.Data;

namespace VMScheduler.Model
{
	[Table("TST_SCHEDULES")]
	public class Scheduler : SchedulerRepository<Scheduler>
	{
		[Key]
		public int ScheduleId { get; set; }

		[Display(Name = "Delivery Type")]
		public string DeliveryType { get; set; }

		[Display(Name = "Output Type")]
		public string OutputType { get; set; }

		public string Parameters { get; set; }

		[Display(Name = "Schedule Date and Time(UTC)")]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy HH:mm:ss}")] // "{0:MM/dd/yyyy}")]
		public DateTime ScheduledTime { get; set; }

		public string Status { get; set; }

		public int TemplateId { get; set; }

		[Display(Name = "Template Name")]
		public string TemplateName { get; set; }

		[Display(Name = "Calling App")]
		public string CallingApp { get; set; }

		[Display(Name = "Delivery Location")]
		public string DeliveryLocation { get; set; }

		[Display(Name = "User Folder")]
		public string UserFolder { get; set; }

		[Display(Name = "Output File Name")]
		public string OutputFileName { get; set; }

		public Guid ScheduleGroupId { get; set; }

		[Display(Name = "Created Date(UTC)")]
		[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy HH:mm:ss}")]

		public DateTime CreatedDate { get; set; }

		public void AddNewEntry()
		{
			Add(this);
			SaveChanges();
		}
	}
}
