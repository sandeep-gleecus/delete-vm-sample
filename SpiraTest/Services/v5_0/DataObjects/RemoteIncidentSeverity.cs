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
    /// Represents an incident severity in the project
    /// </summary>
    public class RemoteIncidentSeverity
    {
        /// <summary>
        /// The id of the incident severity (integer)
        /// </summary>
        public Nullable<int> SeverityId;

        /// <summary>
        /// The name of the severity (string)
        /// </summary>
        public string Name;

        /// <summary>
        /// Whether the severity is active or not (boolean)
        /// </summary>
        public bool Active;

        /// <summary>
        /// The hex color code associated with the severity (string)
        /// </summary>
        public string Color;
    }
}
