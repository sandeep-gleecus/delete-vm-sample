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
    /// This represents a single test step in the system
    /// </summary>
    public class RemoteTestStep : RemoteArtifact
    {
        public Nullable<int> TestStepId;
        public int TestCaseId;
        public int ExecutionStatusId;
        public int Position;
        public string Description;
        public String ExpectedResult;
        public String SampleData;
        public Nullable<int> LinkedTestCaseId;
    }
}
