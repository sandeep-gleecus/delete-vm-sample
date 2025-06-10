using System;
using System.Data;
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
    /// This object represents a single test run step in the system
    /// </summary>
    public class RemoteTestRunStep
    {
        public Nullable<int> TestRunStepId;
        public int TestRunId;
        public Nullable<int> TestStepId;
        public Nullable<int> TestCaseId;
        public int ExecutionStatusId;
        public int Position;
        public string Description;
        public string ExpectedResult;
        public string SampleData;
        public string ActualResult;

    }
}
