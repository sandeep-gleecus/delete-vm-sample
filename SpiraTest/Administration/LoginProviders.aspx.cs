using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using System;
using System.Linq;
using System.Web.UI.WebControls;
using static Inflectra.SpiraTest.Web.UserControls.GlobalNavigation;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>Displays the admin page for managing users in the system</summary>
	[HeaderSettings(NavigationHighlightedLink.Administration, "Admin_OAuthList_Title", "System-Users/#login-providers", "Admin_OAuthList_Title")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class LoginProviders : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.LoginList::";

		/// <summary>Called when the page is first loaded</summary>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Reset the error message
			lblMessage.Text = "";
			litSummary.Text = string.Format(Resources.Main.Admin_OAuthList_Summary, ConfigurationSettings.Default.License_ProductType);

			//Load and bind data
			LoadAndBindData();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Loads the user management datagrid and populates the filter controls if necessary</summary>
		protected void LoadAndBindData()
		{
			const string METHOD_NAME = CLASS_NAME + "LoadAndBindData()";
			Logger.LogEnteringEvent(METHOD_NAME);

			var providers = new OAuthManager().Providers_RetrieveAll(true);
			grdproviders.DataSource = providers.OrderBy(t => t.Name);
			grdproviders.DataBind();

			//Display a warning if any provider is configured and UNloaded.
			if (providers.Where(p => !p.IsLoaded && p.IsActive).Any(p => p.Users.Count > 0))
			{
				// There are Configured providers that are NOT loaded, and have Users assigned to them!
				lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderUnloadedUsers;
				lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
			}
			else if (providers.Where(p => !p.IsLoaded && p.IsActive).Any(p => p.Users.Count == 0))
			{
				//There are configured providers that are not loaded, but there are no users for them.
				lblMessage.Text = Resources.Messages.Admin_OAuth_ProviderUnloadedNoUser;
				lblMessage.Type = ServerControls.MessageBox.MessageType.Warning;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Handles the click event on the item links in the User Management datagrid</summary>
		private void grdUserManagement_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "grdUserManagement_RowCommand()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Redirect to the appropriate page
			if (e.CommandName == "Edit")
			{
				Response.Redirect("UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + e.CommandArgument);
			}
			if (e.CommandName == "SortColumns")
			{
				//Update the sort
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_USER_LIST_PAGINATION, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, e.CommandArgument.ToString());
				LoadAndBindData();
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}
	}
}
