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
    /// Represents an incident priority in the project
    /// </summary>
    public class RemoteIncidentPriority
    {
        /// <summary>
        /// The id of the incident priority (integer)
        /// </summary>
        public Nullable<int> PriorityId;

        /// <summary>
        /// The name of the incident priority (string)
        /// </summary>
        public string Name;

        /// <summary>
        /// Whether the priority is active or not (boolean)
        /// </summary>
        public bool Active;

        /// <summary>
        /// The hex color code associated with the priority (string)
        /// </summary>
        public string Color;

        /// <summary>
        /// The score value
        /// </summary>
        public int? Score;
    }
}
