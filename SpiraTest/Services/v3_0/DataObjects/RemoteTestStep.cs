using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// This represents a single test step in the system
    /// </summary>
    public class RemoteTestStep : RemoteArtifact
    {
        /// <summary>
        /// The id of the test step
        /// </summary>
        public Nullable<int> TestStepId;

        /// <summary>
        /// The id of the test case the step belongs to
        /// </summary>
        public int TestCaseId;

        /// <summary>
        /// The id of the execution status for the last time it was executed
        /// </summary>
        public Nullable<int> ExecutionStatusId;

        /// <summary>
        /// The position of the step in the test case
        /// </summary>
        public int Position;
        
        /// <summary>
        /// The description of what the tester should do to execute this step
        /// </summary>
        public string Description;

        /// <summary>
        /// The description of what the tester should see if the step succeeds
        /// </summary>
        public String ExpectedResult;

        /// <summary>
        /// Any sample data that the tester should use during execution
        /// </summary>
        public String SampleData;

        /// <summary>
        /// If this step is really a linked test case, this is the id of the linked test case
        /// </summary>
        public Nullable<int> LinkedTestCaseId;

        /// <summary>
        /// The date the test step was last updated
        /// </summary>
        public DateTime LastUpdateDate;
    }
}
