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
    /// Represents a task type in the project
    /// </summary>
    public class RemoteTaskType
    {
        /// <summary>
        /// The id of the task type
        /// </summary>
        public int TaskTypeId;

        /// <summary>
        /// The name of the task type
        /// </summary>
        public string Name;

        /// <summary>
        /// The id of the workflow the task type is associated with, for the current project
        /// </summary>
		public int? WorkflowId;

        /// <summary>
        /// Is this an active task type
        /// </summary>
        public bool Active;

        /// <summary>
        /// The display position of this type
        /// </summary>
        public int Position;
    }
}
