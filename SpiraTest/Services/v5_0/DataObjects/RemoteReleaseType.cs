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
    /// Represents a release type in the project
    /// </summary>
    public class RemoteReleaseType
    {
        /// <summary>
        /// The id of the release type
        /// </summary>
        public int ReleaseTypeId;

        /// <summary>
        /// The name of the release type
        /// </summary>
        public string Name;

        /// <summary>
        /// The id of the workflow the release type is associated with, for the current project
        /// </summary>
		public int? WorkflowId;

        /// <summary>
        /// Is this an active release type
        /// </summary>
        public bool Active;

        /// <summary>
        /// The display position of this type
        /// </summary>
        public int Position;
    }
}
