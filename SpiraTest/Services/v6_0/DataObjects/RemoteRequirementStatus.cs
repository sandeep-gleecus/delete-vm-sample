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
    /// Represents a requirement status in the project
    /// </summary>
    public class RemoteRequirementStatus
    {
        /// <summary>
        /// The id of the requirement status
        /// </summary>
        public int RequirementStatusId;

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
