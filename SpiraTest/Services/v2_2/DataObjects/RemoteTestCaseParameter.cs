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
    /// Represents a test case parameter (used when you have linked test case steps)
    /// </summary>
    public class RemoteTestCaseParameter
    {
        public Nullable<int> TestCaseParameterId;
        public int TestCaseId;
        public string Name;
        public string DefaultValue;
    }
}
