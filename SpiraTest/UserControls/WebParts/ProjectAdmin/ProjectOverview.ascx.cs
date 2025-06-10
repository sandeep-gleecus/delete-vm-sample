using System;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;
using Microsoft.Security.Application;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin
{
	/// <summary>
	/// Displays the project overview information
	/// </summary>
	public partial class ProjectOverview : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.ProjectOverview::";

        /// <summary>
        /// Loads the control data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			try
			{
				//Now load the content
				if (!IsPostBack)
				{
					LoadAndBindData();
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow as this is loaded by an update panel and can't redirect to error page
				if (this.Message != null)
				{
					this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
					this.Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

		/// <summary>
		/// Loads the control data
		/// </summary>
		protected void LoadAndBindData()
		{
			try
			{
				Business.ProjectManager projectManager = new Business.ProjectManager();
                Business.UserManager userManager = new Business.UserManager();
                ProjectView project = projectManager.RetrieveById2(ProjectId);
				List<DataModel.User> projectOwners = userManager.RetrieveOwnersByProjectId(ProjectId);
                this.rptProjectOwners.DataSource = projectOwners;
				this.rptProjectOwners.DataBind();

				//Populate the project overview widget
                this.lblProjectDescription.Text = project.Description;
                this.lnkProgram.Text = Encoder.HtmlEncode(project.ProjectGroupName);
                this.lnkProgram.NavigateUrl = UrlRewriterModule.RetrieveGroupAdminUrl(project.ProjectGroupId, "Edit");
                this.lnkTemplate.Text = Encoder.HtmlEncode(project.ProjectTemplateName);
                this.lnkTemplate.NavigateUrl = UrlRewriterModule.RetrieveTemplateAdminUrl(project.ProjectTemplateId, "Default");
                //See if the hyperlink to the group dashboard should be active or not, same for template
                ProjectGroupManager projectGroupManager = new ProjectGroupManager();
                this.lnkProgram.Enabled = (projectGroupManager.IsAuthorized(UserId, project.ProjectGroupId));
                this.lnkTemplate.Enabled = SpiraContext.Current.IsTemplateAdmin;

                //Populate the Planning Options
                this.txtWorkingHoursPerDay.Text = project.WorkingHours.ToString();
                this.txtWorkingDaysPerWeek.Text = project.WorkingDays.ToString();
                this.txtPointEffort.Text = GlobalFunctions.GetEffortInFractionalHours(project.ReqPointEffort);
                this.lnkPlanningOptions.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "PlanningOptions");

                //Handle baseline information
                if (Common.Global.Feature_Baselines)
                {
                    //show the table row
                    this.plcBaselines.Visible = true;

                    //show in the ui if baselining is enabled/disabled
                    ProjectSettings projectSettings = null;
                    if (ProjectId > 0) projectSettings = new ProjectSettings(ProjectId);

                    if (projectSettings != null)
                    {
                        this.lblBaselines.Text = projectSettings.BaseliningEnabled ? Resources.Main.Global_Enabled : Resources.Main.Global_Disabled;
                    }

                }
            }
            catch (ArtifactNotExistsException)
			{
				//Project no longer exists to redirect to the My Page
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The project you selected has been deleted from the system.", true);
			}
		}
	}
}