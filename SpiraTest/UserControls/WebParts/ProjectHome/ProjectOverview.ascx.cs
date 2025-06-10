using System;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the project overview information
	/// </summary>
	public partial class ProjectOverview : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ProjectOverview::";

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
				//Register event handlers
				this.btnProjectGroup.Click += new EventHandler(btnProjectGroup_Click);
                this.btnProjectTemplate.Click += new EventHandler(btnProjectTemplate_Click);

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
		/// Called when the project group link is clicked on
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnProjectGroup_Click(object sender, EventArgs e)
		{
			//First get the group id
			try
			{
                ProjectManager projectManager = new ProjectManager();
                Project project = projectManager.RetrieveById(ProjectId);
                int projectGroupId = project.ProjectGroupId;

				//Now redirect to that group home page
				Response.Redirect(UrlRewriterModule.RetrieveGroupRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.ProjectGroupHome, projectGroupId), true);
			}
			catch (ArtifactNotExistsException)
			{
				//The project no longer exists, so redirect to the my page
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The product you selected has been deleted from the system.", true);
			}
		}

        /// <summary>
		/// Called when the project template link is clicked on
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnProjectTemplate_Click(object sender, EventArgs e)
        {
            //First get the project template id
            try
            {
                ProjectManager projectManager = new ProjectManager();
                Project project = projectManager.RetrieveById(ProjectId);
                int projectTemplateId = project.ProjectTemplateId;

                //Now redirect to that group home page
                Response.Redirect(Inflectra.SpiraTest.Common.UrlRoots.RetrieveTemplateAdminUrl(projectTemplateId, "Default"));
            }
            catch (ArtifactNotExistsException)
            {
                //The project no longer exists, so redirect to the my page
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The product template you selected has been deleted from the system.", true);
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
				if (String.IsNullOrEmpty(project.Description))
				{
					this.lblProjectDescription.Text = "";
				}
				else
				{
					this.lblProjectDescription.Text = project.Description;
				}
				if (String.IsNullOrEmpty(project.Website))
				{
					this.lnkProjectWebsite.Text = "";
					this.lnkProjectWebsite.NavigateUrl = "";
				}
				else
				{
                    this.lnkProjectWebsite.Text = Microsoft.Security.Application.Encoder.HtmlEncode(project.Website);
                    this.lnkProjectWebsite.NavigateUrl = GlobalFunctions.FormNavigatableUrl(project.Website);
				}
                this.btnProjectGroup.Text = Microsoft.Security.Application.Encoder.HtmlEncode(project.ProjectGroupName);
				//See if the hyperlink to the group dashboard should be active or not
                ProjectGroupManager projectGroupManager = new ProjectGroupManager();
                this.btnProjectGroup.Enabled = (projectGroupManager.IsAuthorized(UserId, project.ProjectGroupId));

                this.btnProjectTemplate.Text = Microsoft.Security.Application.Encoder.HtmlEncode(project.ProjectTemplateName);
                //See if the hyperlink to the group dashboard should be active or not
                TemplateManager templateManager = new TemplateManager();
                this.btnProjectTemplate.Enabled = (templateManager.IsAuthorizedToEditTemplate(UserId, project.ProjectTemplateId));
            }
			catch (ArtifactNotExistsException)
			{
				//Project no longer exists to redirect to the My Page
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The project you selected has been deleted from the system.", true);
			}
		}
	}
}
