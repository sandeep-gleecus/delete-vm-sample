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
    /// Represents a single incident resolution/comment entry in the system
    /// </summary>
    public class RemoteIncidentResolution
    {
        /// <summary>
        /// The id of the incident resolution
        /// </summary>
        public Nullable<int> IncidentResolutionId;

        /// <summary>
        /// The id of the incident the resolution/comment belongs to
        /// </summary>
        public int IncidentId;

        /// <summary>
        /// The id of the user that added the comment
        /// </summary>
        /// <remarks>
        /// If no value is specified, the authenticated user is used
        /// </remarks>
        public Nullable<int> CreatorId;

        /// <summary>
        /// The text of the resolution/comment
        /// </summary>
        public String Resolution;

        /// <summary>
        /// The date/time that the comment was added
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// The display name of the user that added the comment
        /// </summary>
        public String CreatorName;
    }
}
