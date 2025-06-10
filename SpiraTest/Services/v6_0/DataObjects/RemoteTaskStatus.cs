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
    /// Represents a task status in the project
    /// </summary>
    public class RemoteTaskStatus
    {
        /// <summary>
        /// The id of the task status
        /// </summary>
        public int TaskStatusId;

        /// <summary>
        /// The name of the task status
        /// </summary>
        public string Name;

        /// <summary>
        /// Is this an active task status
        /// </summary>
        public bool Active;

        /// <summary>
        /// The display position of this status
        /// </summary>
        public int Position;

    }
}
