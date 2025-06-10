using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// This object represents a single step in a test run in the system
    /// </summary>
    public class RemoteTestRunStep
    {
        /// <summary>
        /// The id of the test run step
        /// </summary>
        public Nullable<int> TestRunStepId;

        /// <summary>
        /// The is of the parent test run
        /// </summary>
        public int TestRunId;

        /// <summary>
        /// The id of the test step the test run step is based on
        /// </summary>
        public Nullable<int> TestStepId;

        /// <summary>
        /// The id of the test case that the test run step is based on
        /// </summary>
        /// <remarks>
        /// May be different from the TestRun.TestCaseId in the case of Linked Test Cases
        /// </remarks>
        public Nullable<int> TestCaseId;

        /// <summary>
        /// The id of the execution status of the test run step result
        /// </summary>
        /// <remarks>
        /// Failed = 1;
        /// Passed = 2;
        /// NotRun = 3;
        /// NotApplicable = 4;
        /// Blocked = 5;
        /// Caution = 6;
        /// </remarks>
        public int ExecutionStatusId;

        /// <summary>
        /// The positional order of the test run step in the test run
        /// </summary>
        [ReadOnly]
        public int Position;

        /// <summary>
        /// The description of what the tester should do when executing the step
        /// </summary>
        [ReadOnly]
        public string Description;

        /// <summary>
        /// The expected result that should oocur when the tester executes the step
        /// </summary>
        [ReadOnly]
        public string ExpectedResult;

        /// <summary>
        /// The sample data that should be used by the tester
        /// </summary>
        [ReadOnly]
        public string SampleData;

        /// <summary>
        /// The actual result that occurs when the tester executes the step
        /// </summary>
        public string ActualResult;

        /// <summary>
        /// The actual duration of the test run step
        /// </summary>
        public int? ActualDuration;

        /// <summary>
        /// The start date/time of the test run step
        /// </summary>
        public DateTime? StartDate;

        /// <summary>
        /// The end date/time of the test run step
        /// </summary>
        public DateTime? EndDate;
    }
}
