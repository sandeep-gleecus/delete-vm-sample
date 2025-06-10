using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// This object represents a single test run instance in the system
    /// </summary>
    public class RemoteTestRun : RemoteArtifact
    {
        public Nullable<int> TestRunId;
        public string Name;
        public int TestCaseId;
        public int TestRunTypeId;
        public int TesterId;
        public int ExecutionStatusId;
        public Nullable<int> ReleaseId;
        public Nullable<int> TestSetId;
        public DateTime StartDate;
        public Nullable<DateTime> EndDate;
        public string RunnerName;
        public string RunnerTestName;
        public Nullable<int> RunnerAssertCount;
        public string RunnerMessage;
        public string RunnerStackTrace;
        public List<RemoteTestRunStep> TestRunSteps;
    }
}
