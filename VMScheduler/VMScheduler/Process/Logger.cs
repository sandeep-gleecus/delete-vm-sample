using VMScheduler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMScheduler.Process
{
    public class Logger
    {
        public void CreateUsageLogRecord(string logAction, string actionResult, string lookupKeys, DateTime scheduledDateTime,
                                         int scheduleId, Guid scheduledGroupId, int templateId, string templateName, string user)
        {
            try
            {
                var log = new UsageLog
                {
                    ActionResult = actionResult,
                    CreatedDateTime = DateTime.Now,
                    LogAction = logAction,
                    LookupKeys = lookupKeys,
                    ScheduledDateTime = scheduledDateTime,
                    ScheduleGroupId = scheduledGroupId,
                    ScheduleId = scheduleId,
                    TemplateId = templateId,
                    TemplateName = templateName,
                    User = user
                };

                log.AddNewEntry();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateErrorLogRecord(string errorMessage, string errorContext)
        {
            try
            {
                var log = new ErrorLog
                {
                    CallingApplication = "VM Scheduler",
                    CreatedBy = "",
                    ErrorMessage = errorMessage,
                    CreatedDateTimeUtc = DateTime.UtcNow,
                    ErrorContext = errorContext
                };

                log.AddNewEntry();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
