using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using VMScheduler.Data;

namespace VMScheduler.Model
{
	[Table("TST_ERROR_LOG")]
	public class ErrorLog : LogRepository<ErrorLog>
	{
		[Key]
		public int ErrorLogId { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorContext { get; set; }
		public DateTime CreatedDateTimeUtc { get; set; }
		public string CreatedBy { get; set; }
		public string CallingApplication { get; set; }

		public void AddNewEntry()
		{
			Add(this);
			var x = SaveChanges();
		}
	}
}
