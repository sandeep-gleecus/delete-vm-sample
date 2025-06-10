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
    /// Stores the mapping/coverage between Test Sets and Test Cases
    /// </summary>
    public class RemoteTestSetTestCaseMapping
    {
        /// <summary>
        /// The unique id of the test case in the test set
        /// </summary>
        public int TestSetTestCaseId;

        /// <summary>
        /// The id of the test set
        /// </summary>
        public int TestSetId;

        /// <summary>
        /// The id of the test case
        /// </summary>
        public int TestCaseId;

        /// <summary>
        /// The id of the owner of the test case in the test set
        /// </summary>
        /// <remarks>
        /// Leave as null to default to the test set's owner
        /// </remarks>
        public Nullable<int> OwnerId;
    }
}
