using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls
{	/// <summary>
	///	This user control displays the list of requirements with tasks nested underneath their parent requirement
	///	It is typically enclosed in a panel.
	/// </summary>
	public partial class RequirementTaskPanel : ArtifactUserControlBase, IArtifactUserControl
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.RequirementTaskPanel::";

		//Viewstate keys
		protected const string ViewStateKey_MessageControlName = "MessageControlName";

        /// <summary>
		/// This sets up the user control upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            //Populate the user and project id in the task grid control
            this.grdReqTaskList.ProjectId = this.ProjectId;

            //See if we're planning by points or hours
            ProjectSettings projectSettings = new ProjectSettings(ProjectId);
            bool useHours = projectSettings.DisplayHoursOnPlanningBoard;
            this.plcHoursLegend.Visible = useHours;
            this.plcPointsLegend.Visible = !useHours;

            Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        public void LoadAndBindData(bool dataBind)
        {
            //Does nothing since we have an AJAX control
        }
	}
}
