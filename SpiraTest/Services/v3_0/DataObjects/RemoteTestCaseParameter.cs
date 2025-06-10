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
    /// Represents a test case parameter (used when you have linked test case steps)
    /// </summary>
    public class RemoteTestCaseParameter
    {
        /// <summary>
        /// The id of the test case parameter
        /// </summary>
        public Nullable<int> TestCaseParameterId;

        /// <summary>
        /// The id of the test case the parameter is defined for
        /// </summary>
        public int TestCaseId;

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name;

        /// <summary>
        /// The default value when none is provided by a parent test case
        /// </summary>
        public string DefaultValue;
    }
}
