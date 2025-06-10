using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents an incident type in the project
    /// </summary>
    public class RemoteIncidentType
    {
        /// <summary>
        /// The id of the incident type
        /// </summary>
        public Nullable<int> IncidentTypeId;

        /// <summary>
        /// The name of the incident type
        /// </summary>
        public string Name;

        /// <summary>
        /// Whether the incident type is active or not
        /// </summary>
        public bool Active;

        /// <summary>
        /// Should incidents of this type appear in the 'Top Open Issues' section
        /// </summary>
        public bool Issue;

        /// <summary>
        /// Should incidents of this type appear in the 'Top Open Issues' section
        /// </summary>
        public bool Risk;

        /// <summary>
        /// The id of the workflow the incidents are associated with
        /// </summary>
		public int WorkflowId;
    }
}
