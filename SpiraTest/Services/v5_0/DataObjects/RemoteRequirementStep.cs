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
    /// This represents a single requirement scenario step in the system
    /// </summary>
    public class RemoteRequirementStep
    {
        /// <summary>
        /// The id of the step
        /// </summary>
        public int? RequirementStepId;

        /// <summary>
        /// The id of the requirement the step belongs to
        /// </summary>
        public int RequirementId;

        /// <summary>
        /// The position of the step in the requirement
        /// </summary>
        public int Position;
        
        /// <summary>
        /// The description of this step
        /// </summary>
        public string Description;

        /// <summary>
        /// The date the step was last updated
        /// </summary>
        public DateTime LastUpdateDate;

        /// <summary>
        /// The date the step was edited
        /// </summary>
        public DateTime ConcurrencyDate;

        /// <summary>
        /// The date the step was created
        /// </summary>
        public DateTime CreationDate;
    }
}
