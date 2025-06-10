using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>Displays the administration history list page</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_HistoryChangeset", "Product-General-Settings/#product-history-changes", "Admin_HistoryChangeset")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator | AdministrationLevelAttribute.AdministrationLevels.ProjectOwner)]
	public partial class HistoryList : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.Project.HistoryList::";

		protected string productName = "";

		/// <summary>Called when the control is first loaded</summary>
		/// <param name="sender">Page</param>
		/// <param name="e">EventArgs</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			//Redirect if there's no project selected.
			if (ProjectId < 1)
				Response.Redirect("~/Administration/Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);

			//See if we need to display a message.
			if (Session["MSGTYPE"] != null && Session["MSG"] != null)
			{
				lblMessage2.Type = (ServerControls.MessageBox.MessageType)Session["MSGTYPE"];
				lblMessage2.Text = (string)Session["MSG"];

				Session.Remove("MSG");
				Session.Remove("MSGTYPE");
			}

			//Add the client event handler to the background task process
			Dictionary<string, string> handlers = new Dictionary<string, string>();
			handlers.Add("succeeded", "purgeall_success");
			ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

			//Set the licensed product name (used in several places) and url
			productName = ConfigurationSettings.Default.License_ProductType;
			lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
			lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

			sgHistory.ProjectId = ProjectId;
			sgHistory.BaseUrl = "~/" + ProjectId + "/Administration/HistoryDetails/{art}.aspx";

			//Hide the Purge button if Baselining is enabled.
			ProjectSettings projectSettings = new ProjectSettings(ProjectId);
			btnPurgeAll1.Visible = !(projectSettings.BaseliningEnabled && Common.Global.Feature_Baselines);

			//See if we have a passed in artifact type or artifact id
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID]) && !string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_ID]))
			{
				int artifactTypeId;
				int artifactId;
				if (int.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID], out artifactTypeId))
				{
					if (int.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_ID], out artifactId))
					{
						//Set the filter on the service before the page loads. We don't use 'standard filters' because
						//we want to allow the user to unset the filter if necessary
						ProjectSettingsCollection settings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_HISTORY_ADMINFILTERS_LIST);
						settings.Clear();
						settings.Add("ArtifactId", artifactId);
						settings.Add("ArtifactTypeId", artifactTypeId);
						settings.Save();
					}
				}
			}

			if (!IsPostBack)
				DataBind();
		}
	}
}
