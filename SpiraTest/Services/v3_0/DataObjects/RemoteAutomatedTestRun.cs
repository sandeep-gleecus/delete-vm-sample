using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Represents an automated test run in the system
    /// </summary>
    public class RemoteAutomatedTestRun : RemoteTestRun
    {
        /// <summary>
        /// The id of the current project
        /// </summary>
        public Nullable<int> ProjectId;

        /// <summary>
        /// The name of the external automated tool that executed the test
        /// </summary>
        public string RunnerName;

        /// <summary>
        /// The name of the test case as it is known in the external tool
        /// </summary>
        public string RunnerTestName;

        /// <summary>
        /// The number of assertions/errors reported during the automated test execution
        /// </summary>
        public Nullable<int> RunnerAssertCount;

        /// <summary>
        /// The summary result of the test case
        /// </summary>
        public string RunnerMessage;

        /// <summary>
        /// The detailed trace of test results reported back from the automated testing tool
        /// </summary>
        public string RunnerStackTrace;

        /// <summary>
        /// The id of the automation host that the result is being recorded for
        /// </summary>
        public Nullable<int> AutomationHostId;

        /// <summary>
        /// The id of the automation engine that the result is being recorded for
        /// </summary>
        public Nullable<int> AutomationEngineId;

        /// <summary>
        /// The token of the automation engine that the result is being recorded for (read-only)
        /// </summary>
        public string AutomationEngineToken;

        /// <summary>
        /// The id of the attachment that is being used to store the test script (file or url)
        /// </summary>
        public Nullable<int> AutomationAttachmentId;

        /// <summary>
        /// The list of test case parameters that have been provided
        /// </summary>
        public List<RemoteTestSetTestCaseParameter> Parameters;

        /// <summary>
        /// The datetime the test was scheduled for
        /// </summary>
        public Nullable<DateTime> ScheduledDate;
    }
}