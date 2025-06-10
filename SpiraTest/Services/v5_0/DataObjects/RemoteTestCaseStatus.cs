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
    /// Represents a test case status in the project
    /// </summary>
    public class RemoteTestCaseStatus
    {
        /// <summary>
        /// The id of the test case status
        /// </summary>
        public int TestCaseStatusId;

        /// <summary>
        /// The name of the status
        /// </summary>
        public string Name;

        /// <summary>
        /// Is this an active status
        /// </summary>
        public bool Active;

        /// <summary>
        /// The display position of this status
        /// </summary>
        public int Position;

    }
}
