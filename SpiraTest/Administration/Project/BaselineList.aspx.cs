using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using System;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>Displays the administration Baseline List page</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_BaselineList", "Product-General-Settings/#baselines", "Admin_BaselineList")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator | AdministrationLevelAttribute.AdministrationLevels.ProjectOwner)]
	public partial class BaselineList : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.Project.BaselineList::";
		protected string productName = "";

		/// <summary>Called when the control is first loaded</summary>
		/// <param name="sender">Page</param>
		/// <param name="e">EventArgs</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			//Redirect if there's no project selected.
			if (ProjectId < 1)
				Response.Redirect("~/Administration/Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);

			//Check that Baselines is even enabled.
			if (!Common.Global.Feature_Baselines)
			{
				Response.Redirect("~/Administration/Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_BaselinesNotAvailable, true);
			}
			else
			{
				var projSettings = new ProjectSettings(ProjectId);
				if (projSettings == null || !projSettings.BaseliningEnabled)
				{
					Response.Redirect("~/Administration/Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_BaselinesNotEnabled, true);
				}
			}

			//Set the licensed product name (used in several places) and url
			productName = ConfigurationSettings.Default.License_ProductType;
			lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
			lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

			grdBaselines.ProjectId = ProjectId;
			grdBaselines.BaseUrl = "~/" + ProjectId + "/Administration/BaselineDetails/{art}.aspx";

			if (!IsPostBack)
				DataBind();
		}
	}
}
