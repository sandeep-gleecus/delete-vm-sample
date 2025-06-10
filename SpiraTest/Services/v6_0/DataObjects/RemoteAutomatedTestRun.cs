using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents an automated test run in the system
    /// </summary>
    public class RemoteAutomatedTestRun : RemoteTestRun
    {
        /// <summary>
        /// The format of the automation results (1=Plain Text, 2=HTML) stored in the 'RunnerStackTrace' field - required
        /// </summary>
        /// <see cref="RunnerStackTrace"/>
        public int TestRunFormatId;

        /// <summary>
        /// The name of the external automated tool that executed the test - required
        /// </summary>
        public string RunnerName;

        /// <summary>
        /// The name of the test case as it is known in the external tool - required
        /// </summary>
        public string RunnerTestName;

        /// <summary>
        /// The number of assertions/errors reported during the automated test execution
        /// </summary>
        public Nullable<int> RunnerAssertCount;

        /// <summary>
        /// The summary result of the test case - required
        /// </summary>
        public string RunnerMessage;

        /// <summary>
        /// The detailed trace of test results reported back from the automated testing tool - required
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

        /// <summary>
        /// The list of test steps that comprise the automated test
        /// </summary>
        /// <remarks>
        /// These are optional for automated test runs. The status of the test run steps
        /// does not change the overall status of the automated test run. They are used to
        /// simply make reporting clearer inside the system. They will also update the status of
        /// appropriate Test Step(s) if a valid test step id is provided.
        /// </remarks>
        public List<RemoteTestRunStep> TestRunSteps;
    }
}