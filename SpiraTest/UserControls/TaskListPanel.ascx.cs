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
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls
{	/// <summary>
	///	This user control displays the list of tasks associated with a requirement or risk
	///	It is typically enclosed in a panel.
	/// </summary>
	public partial class TaskListPanel : ArtifactUserControlBase, IArtifactUserControl
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.TaskListPanel::";

        //Viewstate keys
        protected const string ViewStateKey_MessageControlName = "MessageControlName";

        #region Properties

        /// <summary>
        /// Gets or sets a reference to the form manager on the page the user control is on
        /// </summary>
        public AjaxFormManager FormManager
        {
            get;
            set;
        }

        #endregion

        /// <summary>
		/// This sets up the user control upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            TaskManager task = new TaskManager();

            //Populate the user and project id in the task grid control
            this.grdTaskList.ProjectId = this.ProjectId;
            this.grdTaskList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -2) + "?" + GlobalFunctions.PARAMETER_REFERER_REQUIREMENT_DETAILS + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
            this.grdTaskList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TASK;

            // set the link to create a new task here so we can change it below if we need to
            this.lnkNewTask.ClientScriptMethod = "insert_item('Task')";

            //See if we're linked to a requirement or risk
            if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.Requirement)
            {
                //Set the filter that it only displays tasks associated with this requirement
                Dictionary<string, object> taskFilters = new Dictionary<string, object>();
                taskFilters.Add("RequirementId", this.ArtifactId);
                this.grdTaskList.SetFilters(taskFilters);

                //Specify that the task grid should use different filters and sorts than the main task list
                this.grdTaskList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.Requirement_Tasks;

                //Set the remove link to remove the requirement association (both parent and menu item)
                this.lnkRemoveTask.ClientScriptMethod = "custom_list_operation('RemoveFromRequirement'," + this.ArtifactId + ")";
                this.lnkRemoveTask.DropMenuItems[0].ClientScriptMethod = "custom_list_operation('RemoveFromRequirement'," + this.ArtifactId + ")";

                //Update the create link
                this.lnkNewTask.ClientScriptMethod = "insert_item('Task'," + (int)Artifact.DisplayTypeEnum.Requirement_Tasks + ")";
            }

            if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.Risk)
            {
                //Set the filter that it only displays tasks associated with this requirement
                Dictionary<string, object> taskFilters = new Dictionary<string, object>();
                taskFilters.Add("RiskId", this.ArtifactId);
                this.grdTaskList.SetFilters(taskFilters);

                //Specify that the task grid should use different filters and sorts than the main task list
                this.grdTaskList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.Risk_Tasks;

                //Set the remove link to remove the requirement association (both menu and menu item)
                this.lnkRemoveTask.ClientScriptMethod = "custom_list_operation('RemoveFromRisk'," + this.ArtifactId + ")";
                this.lnkRemoveTask.DropMenuItems[0].ClientScriptMethod = "custom_list_operation('RemoveFromRisk'," + this.ArtifactId + ")";

                //Hide the effort legend (not used for risks)
                this.plcEffortLegend.Visible = false;

                //Update the create link
                this.lnkNewTask.ClientScriptMethod = "insert_item('Task'," + (int)Artifact.DisplayTypeEnum.Risk_Tasks + ")";
            }

            //Add the event handler that will refresh some data on the main release form when the grid is loaded
            if (this.FormManager != null)
            {
                Dictionary<string, string> handlers = new Dictionary<string, string>();
                handlers.Add("loaded", "grdTaskList_loaded");
                this.grdTaskList.SetClientEventHandlers(handlers);
            }

            //Populate the list of test run columns to show/hide and databind
            this.ddlShowHideTaskColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Task);
            this.ddlShowHideTaskColumns.DataBind();

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        public void LoadAndBindData(bool dataBind)
        {
            //Does nothing since we have an AJAX control
        }
	}
}
