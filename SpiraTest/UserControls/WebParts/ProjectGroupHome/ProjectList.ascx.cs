using System;
using System.Collections;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome
{
	public partial class ProjectList : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.ProjectList::";

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
                //Configure the project name link to have the right URL
                NameDescriptionFieldEx field = (NameDescriptionFieldEx)this.grdProjectList.Columns[1];
                if (field != null)
                {
                    field.NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, -3);
                }

				//Now load the content
				if (!IsPostBack)
				{
                    if (WebPartVisible)
                    {
                        LoadAndBindData();
                    }
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
				//Get the project group id
				int projectGroupId = ProjectGroupId;

				//Now get the list of projects
				ProjectManager projectManager = new ProjectManager();
				Hashtable filters = new Hashtable();
				filters.Add("ProjectGroupId", projectGroupId);
                filters.Add("ActiveYn", "Y");
                List<ProjectView> projects = projectManager.Retrieve(filters, null);
                this.grdProjectList.DataSource = projects;
				this.grdProjectList.DataBind();
			}
			catch (ArtifactNotExistsException)
			{
				//Project Group no longer exists to redirect to the My Page
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The project group you selected has been deleted from the system.", true);
			}
		}
	}
}