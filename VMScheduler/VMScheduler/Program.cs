using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

using VMScheduler.Data;
using VMScheduler.Model;
using VMScheduler.Process;

namespace VMScheduler
{

	class Program
	{
		private static Logger _logger = new Logger();

		static void Main(string[] args)
		{
			RunScheduler();
		}

		private static void RunScheduler()
		{
			try
			{
				_logger = new Logger();

				string user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
				Debug.Print(Environment.UserName);
				Console.WriteLine($"Scheduler started on {DateTime.Now.ToString()}");
				string truncateDays = ConfigurationManager.AppSettings.Get("DaysToTruncateSchedules");

				//UTC
				//eventTime = DateTime.UtcNow.AddMinutes(1); //UTC = CDT + 5
				//truncateTime = DateTime.UtcNow.AddDays((-1) * (Convert.ToInt16(truncateDays)));
				//truncateUsage = DateTime.UtcNow.AddDays(-7);
				//LOCAL
				var eventTime = DateTime.Now.AddMinutes(1);
				var truncateUsage = DateTime.Now.AddDays((-1) * (Convert.ToInt16(truncateDays)));

				_logger = new Logger();
				_logger.CreateUsageLogRecord("Scheduler Started", "Start Application", String.Empty, DateTime.Now, 0, Guid.Empty, 0, String.Empty, user);

				using (var db = new SchedulerContext())
				{
					//Check if we need to truncate Usage table
					var usageToDelete = from s in db.UsageLogs
										where s.ActionResult == "Exit Application" || s.ActionResult == "Start Application"
										where s.CreatedDateTime <= truncateUsage
										select s;
					if (usageToDelete.Any())
					{
						var recCount = usageToDelete.Count();
						db.UsageLogs.RemoveRange(usageToDelete.ToList());
						db.SaveChanges();
						_logger.CreateUsageLogRecord("Truncated Usage table (" + recCount + ")", String.Empty, String.Empty, DateTime.Now, 0, Guid.Empty, 0, String.Empty, "System");
					}

					//START HERE
					var schedules = from s in db.Schedules
									where s.Status == "Ready"
									where s.ScheduledTime <= eventTime
									select s;
					var scheduleTableCopy = schedules.ToList();

					if (!schedules.Any())
					{
						_logger.CreateUsageLogRecord("No records to process.", "Exit Application", String.Empty, DateTime.Now, 0, Guid.Empty, 0, String.Empty, "System");
						return; // nothing to process, exit application
					}

					_logger.CreateUsageLogRecord($"{schedules.Count()} records to process.", String.Empty, String.Empty, DateTime.Now, 0, Guid.Empty, 0, String.Empty, "System");

					//There are records to process
					//1. Update status to "In Process"
					schedules.ToList().ForEach(s => s.Status = "In Process");
					db.SaveChanges();

					//2. For each record call VM API
					Guid scheduleGroupId = Guid.Empty;
					IList<string> files = new List<string>();
					Guid groupToProcess = Guid.Empty;

					foreach (var schedule in scheduleTableCopy.OrderBy(c => c.ScheduleGroupId))
					{
						//Check if this group already processed
						if (groupToProcess != schedule.ScheduleGroupId)
						{
							groupToProcess = schedule.ScheduleGroupId;
							string filename = string.Empty;
							string customFileName = string.Empty;
							if (!string.IsNullOrEmpty(schedule.OutputFileName))
								customFileName = schedule.OutputFileName.Trim().Length > 0 ? schedule.OutputFileName.Trim() : schedule.TemplateName;
							else
								customFileName = schedule.TemplateName;

							IList<Result> results = new List<Result>();
							var group = scheduleTableCopy.Where(c => c.ScheduleGroupId == groupToProcess);

							foreach (var groupItem in group)
							{
								Result result = new Result
								{
									TemplateId = groupItem.TemplateId,
									TemplateName = groupItem.TemplateName,
									Parameters = groupItem.Parameters,
									Status = "Fail",
									Filename = String.Empty,
									DeliveryLocation = groupItem.DeliveryLocation,
									ScheduleId = groupItem.ScheduleId,
									OutputFileName = customFileName
								};
								try
								{
									var obj = new ReportGenerator(_logger);
									_logger.CreateUsageLogRecord("Ready to generate document", "Start GenerateReport", groupItem.Parameters, groupItem.ScheduledTime, groupItem.ScheduleId, groupItem.ScheduleGroupId, groupItem.TemplateId, groupItem.TemplateName, user);

									filename = obj.GenerateReport(groupItem.Parameters, groupItem.OutputType, groupItem.ScheduleId.ToString(), groupItem.TemplateId.ToString(), groupItem.CallingApp, groupItem.OutputFileName);

									_logger.CreateUsageLogRecord("Document generation completed", $"Document name - {filename}", groupItem.Parameters, groupItem.ScheduledTime, groupItem.ScheduleId, groupItem.ScheduleGroupId, groupItem.TemplateId, groupItem.TemplateName, user);

									if (!string.IsNullOrEmpty(filename))
									{
										result.Status = filename.EndsWith(".txt") ? "Fail" : "Success";
										result.Filename = string.IsNullOrEmpty(customFileName) ? filename : $"{customFileName}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")}";
									}
								}
								catch (Exception ex)
								{
									Console.WriteLine(ex.Message);
									_logger.CreateErrorLogRecord(ex.Message, $"Document generation failed for scheduleId {groupItem.ScheduleId}");
									//Send error message
									result.Status = result.Status + " - " + ex.Message;
								}
								results.Add(result);

								//SENDING TO SHAREPOINT
								if (results.Count > 0)
								{
									_logger.CreateUsageLogRecord("Document delivery starting", $"Delivery Location - {groupItem.DeliveryLocation}", groupItem.Parameters, groupItem.ScheduledTime, groupItem.ScheduleId, groupItem.ScheduleGroupId, groupItem.TemplateId, groupItem.TemplateName, user);
									if (groupItem.DeliveryType.ToLower().Contains("sharepoint"))
									{
										var filenameOnly = filename.Substring(filename.LastIndexOf(@"\") + 1);
										_logger.CreateUsageLogRecord("Document delivery status", $"FileName - {filenameOnly}", groupItem.Parameters, groupItem.ScheduledTime, groupItem.ScheduleId, groupItem.ScheduleGroupId, groupItem.TemplateId, groupItem.TemplateName, user);
										_logger.CreateUsageLogRecord("Document delivery status", $"PathAndFileName - {filename}", groupItem.Parameters, groupItem.ScheduledTime, groupItem.ScheduleId, groupItem.ScheduleGroupId, groupItem.TemplateId, groupItem.TemplateName, user);

										SharePointProcessor SharePointProcessor = new SharePointProcessor(_logger);

										switch (groupItem.DeliveryType.ToLower())
										{
											case "sharepointonline":
												SharePointProcessor.UploadFileSP365(filenameOnly, filename, groupItem.DeliveryLocation, groupItem.UserFolder);
												break;
											case "sharepointonprem":
												SharePointProcessor.UploadFileSPonPrem(filenameOnly, filename, groupItem.DeliveryLocation, groupItem.UserFolder);
												break;
										}
									}

									foreach (Result r in results)
									{
										//Delete local file
										if (File.Exists(r.Filename)) File.Delete(r.Filename);
									}

									//We are all done. Update status of the scheduled records
									//Update status to "Completed" or "Failed"
									foreach (var g in group)
									{
										g.Status = results.Where(r => r.ScheduleId == g.ScheduleId).Select(r => r.Status).FirstOrDefault() == "Fail" ? "Failed" : "Completed";
									}
									db.SaveChanges();
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				var messages = new List<string>();
				do
				{
					messages.Add(ex.Message);
					ex = ex.InnerException;
				}
				while (ex != null);
				var message = string.Join(" - ", messages);

				_logger.CreateErrorLogRecord(message, "RunScheduler");
			}
		}
	}
}

