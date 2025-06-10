using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a test step parameter (used when you have linked test case steps)
    /// </summary>
    public class RemoteTestStepParameter
    {
        /// <summary>
        /// The name of the test step parameter
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the parameter to be passed from the parent test case to the child test case
        /// </summary>
        public string Value;
    }
}
