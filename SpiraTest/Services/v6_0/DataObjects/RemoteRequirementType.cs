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
    /// Represents a requirement type in the project
    /// </summary>
    public class RemoteRequirementType
    {
        /// <summary>
        /// The id of the requirement type
        /// </summary>
        public int RequirementTypeId;

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
        /// Is this the default type
        /// </summary>
        public bool IsDefault;

        /// <summary>
        /// Does this type have steps
        /// </summary>
        public bool IsSteps;
    }
}
