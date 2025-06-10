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
    /// This represents a single risk mitigation in the system
    /// </summary>
    public class RemoteRiskMitigation
    {
        /// <summary>
        /// The id of the mitigation
        /// </summary>
        public int? RiskMitigationId;

        /// <summary>
        /// The id of the risk the mitigation belongs to
        /// </summary>
        public int RiskId;

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

		/// <summary>
		/// Is this mitigation deleted?
		/// </summary>
		public bool IsDeleted;

		/// <summary>
		/// Is this mitigation active?
		/// </summary>
		public bool IsActive;

		/// <summary>
		/// The scheduled review date (optional)
		/// </summary>
		public DateTime? ReviewDate;
	}
}
