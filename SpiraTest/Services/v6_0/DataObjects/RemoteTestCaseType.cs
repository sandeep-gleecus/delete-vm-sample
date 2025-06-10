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
    /// Represents a test case type in the project
    /// </summary>
    public class RemoteTestCaseType
    {
        /// <summary>
        /// The id of the test case type
        /// </summary>
        public int TestCaseTypeId;

        /// <summary>
        /// The name of the type
        /// </summary>
        public string Name;

        /// <summary>
        /// The id of the workflow the type is associated with, for the current project
        /// </summary>
		public int? WorkflowId;

        /// <summary>
        /// Is this an active type
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// The display position of this type
        /// </summary>
        public int Position;

        /// <summary>
        /// Is this the default test case type
        /// </summary>
        public bool IsDefault;

        /// <summary>
        /// Is this an exploratory test case
        /// </summary>
        public bool IsExploratory;

        /// <summary>
        /// Is this a BDD format test case
        /// </summary>
        public bool IsBdd;
    }
}
