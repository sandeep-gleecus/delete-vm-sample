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
    /// Represents an incident status in the project
    /// </summary>

    public class RemoteIncidentStatus
    {
        /// <summary>
        /// The id of the incident status
        /// </summary>
        public Nullable<int> IncidentStatusId;

        /// <summary>
        /// The name of the incident status
        /// </summary>
        public string Name;

        /// <summary>
        /// Whether this status is active or not
        /// </summary>
        public bool Active;

        /// <summary>
        /// Whether this status is considered an 'open' status or not
        /// </summary>
        public bool Open;
    }
}
