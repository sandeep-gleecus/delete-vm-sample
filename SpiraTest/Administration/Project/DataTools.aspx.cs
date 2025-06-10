using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Microsoft.Security.Application;
using System;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>This webform code-behind class is responsible to displaying the Administration Page and handling all raised events</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_DataTools_Title", "Product-General-Settings/#product-data-tools", "Admin_DataTools_Title")]
	public partial class DataTools : AdministrationBase
	{
		//Member variables
		protected string projectName = "";

		protected SortedList<string, string> flagList;
		protected System.Web.UI.WebControls.Label lblProjectName;
		protected System.Web.UI.WebControls.LinkButton btnPrevious;
		protected System.Web.UI.WebControls.Repeater rptPaginationLinks;
		protected System.Web.UI.WebControls.LinkButton btnNext;

		//Lookups
		protected List<Workflow> workflows;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectDataTools::";

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load";

			Logger.LogEnteringEvent(METHOD_NAME);

			//Redirect if there's no project selected.
			if (ProjectId < 1)
				Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);

			//Reset the error messages
			this.lblMessage.Text = "";

			//Display the project name
			this.lblDataCachingProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
			this.lnkAdminHome.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

			//Add the client event handler to the background task process
			Dictionary<string, string> handlers = new Dictionary<string, string>();
			handlers.Add("succeeded", "ajxBackgroundProcessManager_success");
			this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

			//Display initial status
			LoadProjectDataTools();

			//Only load the data once
			if (!IsPostBack)
			{
				//Instantiate the business objects
				Business.ProjectManager project = new Business.ProjectManager();

				//Retrieve any generic (non-panel specific) datasets
				this.flagList = project.RetrieveFlagLookup();

				this.DataBind();
			}

			//Finally, see if we have any error message passed from a calling page
			if (!String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]))
			{
				this.lblMessage.Text = Encoder.HtmlEncode(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]);
				this.lblMessage.Type = MessageBox.MessageType.Error;
			}

			//Hide the database index refresh if hosted
			if (!Common.Properties.Settings.Default.LicenseEditable)
			{
				this.lnkRefreshIndexes.Visible = false;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Sets the initial status to 'unknown'</summary>
		void LoadProjectDataTools()
		{
            //Set labels.
            this.lblCheckReq.Text = Resources.Main.Admin_DataTools_CurrentStatusUnknown;
            this.lblCheckRel.Text = Resources.Main.Admin_DataTools_CurrentStatusUnknown;
        }
	}
}
