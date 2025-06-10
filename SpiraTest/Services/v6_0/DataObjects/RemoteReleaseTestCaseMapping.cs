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
    /// Stores the mapping/coverage between Releases and Test Cases
    /// </summary>
    public class RemoteReleaseTestCaseMapping
    {
        /// <summary>
        /// The id of the release
        /// </summary>
        public int ReleaseId;

        /// <summary>
        /// The id of the test case
        /// </summary>
        public int TestCaseId;
    }
}
