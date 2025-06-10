using System;
using System.Data;
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
    /// Stores the mapping/coverage between Requirements and Test Cases
    /// </summary>
    public class RemoteRequirementTestCaseMapping
    {
        /// <summary>
        /// The id of the requirement
        /// </summary>
        public int RequirementId;

        /// <summary>
        /// The id of the test case
        /// </summary>
        public int TestCaseId;
    }
}
