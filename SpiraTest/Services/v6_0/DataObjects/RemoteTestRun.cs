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

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// This object represents a single test run instance in the system
    /// </summary>
    public class RemoteTestRun : RemoteArtifact
    {
        /// <summary>
        /// The id of the test run
        /// </summary>
        public Nullable<int> TestRunId;

        /// <summary>
        /// The name of the test run (usually the same as the test case)
        /// </summary>
        public string Name;

        /// <summary>
        /// The id of the test case that the test run is an instance of
        /// </summary>
        public int TestCaseId;

        /// <summary>
        /// The id of the type of test run (automated vs. manual)
        /// </summary>
        public int TestRunTypeId;

        /// <summary>
        /// The id of the user that executed the test
        /// </summary>
        /// <remarks>
        /// The authenticated user is used if no value is provided
        /// </remarks>
        public Nullable<int> TesterId;

        /// <summary>
        /// The id of overall execution status for the test run
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
        /// The id of the release that the test run should be reported against
        /// </summary>
        public Nullable<int> ReleaseId;

        /// <summary>
        /// The id of the test set that the test run should be reported against
        /// </summary>
        public Nullable<int> TestSetId;

        /// <summary>
        /// The id of the unique test case entry in the test set
        /// </summary>
        public Nullable<int> TestSetTestCaseId;

        /// <summary>
        /// The date/time that the test execution was started
        /// </summary>
        public DateTime StartDate;

        /// <summary>
        /// The date/time that the test execution was completed
        /// </summary>
        public Nullable<DateTime> EndDate;

        /// <summary>
        /// The id of the build that the test was executed against
        /// </summary>
        public Nullable<int> BuildId;

        /// <summary>
        /// The estimated duration of how long the test should take to execute (read-only)
        /// </summary>
        /// <remarks>
        /// This field is populated from the test case being executed
        /// </remarks>
        public int? EstimatedDuration;

        /// <summary>
        /// The actual duration of how long the test should take to execute (read-only)
        /// </summary>
        /// <remarks>
        /// This field is calculated from the start/end dates provided during execution
        /// </remarks>
        public int? ActualDuration;

        /// <summary>
        /// The id of the specific test configuration that was used
        /// </summary>
        public int? TestConfigurationId;
    }
}
