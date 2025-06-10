using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extended properties to the report object
    /// </summary>
    public partial class Report : Entity
    {
        #region Enumerations

        /// <summary>
        /// Contains the ids of the standard reports used when you click 'Print' on various artifacts in the system
        /// </summary>
        public static class StandardReports
        {
            public const string RequirementSummary = "RequirementSummary";
            public const string RequirementDetailed = "RequirementDetailed";
            public const string TestCaseSummary = "TestCaseSummary";
            public const string TestCaseDetailed = "TestCaseDetailed";
            public const string TestSetSummary = "TestSetSummary";
            public const string TestSetDetailed = "TestSetDetailed";
            public const string TestRunSummary = "TestRunSummary";
            public const string TestRunDetailed = "TestRunDetailed";
            public const string IncidentSummary = "IncidentSummary";
            public const string IncidentDetailed = "IncidentDetailed";
            public const string TaskSummary = "TaskSummary";
            public const string TaskDetailed = "TaskDetailed";
            public const string ReleaseSummary = "ReleaseSummary";
            public const string ReleaseDetailed = "ReleaseDetailed";
            public const string RiskSummary = "RiskSummary";
            public const string RiskDetailed = "RiskDetailed";
			public const string AllHistoryList = "AllHistoryList";
			public const string ProjectAuditTrail = "ProjectAuditTrail";
			public const string AdminAuditTrail = "AdminAuditTrail";
			public const string AllAdminAuditList = "AllAdminAuditList";
			public const string AuditTrail = "AuditTrail";
			public const string AllAuditList = "AllAuditList";
			public const string AllUserAuditList = "AllUserAuditList";
			public const string UserAuditTrail = "UserAuditTrail";
			public const string SystemUsageReport = "SystemUsageReport";
			public const string OldProjectAuditTrail = "OldProjectAuditTrail";
		}

        /// <summary>
        /// Contains some of the standard report format ids
        /// </summary>
        public enum ReportFormatEnum
        {
            Html = 1,
            MsWord2003 = 2,
            MsExcel2003 = 3,
            MsProj2003 = 4,
            Xml = 5,
            MsWord2007 = 6,
            MsExcel2007 = 7,
            Pdf = 8
        }

        #endregion
    }
}
