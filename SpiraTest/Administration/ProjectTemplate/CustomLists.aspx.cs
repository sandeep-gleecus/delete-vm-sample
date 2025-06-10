using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Edit Custom Lists Page and handling all raised events
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "CustomLists_Title", "Template-Custom-Properties/#edit-custom-lists", "CustomLists_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class CustomLists : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.CustomLists";

		/// <summary>
		/// Called when the page is first loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Redirect if there's no project template selected.
			if (ProjectTemplateId < 1)
				Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);

			try
			{
				//Display the project template name
				this.ltrProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(this.ProjectTemplateName);
                this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

                //Set the add list link url
                this.btnAddList.NavigateUrl = UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "CustomListDetails");

                //Attach event handlers..
                this.grdCustomLists.RowCommand += new System.Web.UI.WebControls.GridViewCommandEventHandler(grdCustomLists_RowCommand);

				if (!this.IsPostBack)
					this.LoadAndBind();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Load page's data.</summary>
		private void LoadAndBind()
		{
			const string METHOD_NAME = "LoadAndBind";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Load the dataset for the datagrid..
				this.grdCustomLists.DataSource = new CustomPropertyManager().CustomPropertyList_RetrieveForProjectTemplate(ProjectTemplateId, true, true);

				//Load the page..
				this.DataBind();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Called when a command is performed on a datagrid row.</summary>
		/// <param name="sender">grdCustomLists</param>
		/// <param name="e">EventArgs</param>
		void grdCustomLists_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = "grdCustomLists_RowCommand";

			try
			{
				//Depends on what we're doing..
				switch (e.CommandName.ToLowerInvariant().Trim())
				{
					case "createnew":
						//Transfers to the Values page. Done client-side, not called here.
						break;

					case "remove":
						//Remove the specified custom list.
						int listId = int.Parse((string)e.CommandArgument);
						try
						{
							new CustomPropertyManager().CustomPropertyList_Remove(listId);
							this.lblMessage.Text = Resources.Messages.Admin_CustomLists_Removed;
							this.lblMessage.Type = MessageBox.MessageType.Information;
						}
						catch (EntityForeignKeyException)
						{
							this.lblMessage.Text = Resources.Messages.Admin_CustomLists_CannotRemoveInUse;
							this.lblMessage.Type = MessageBox.MessageType.Error;
						}
						break;

					case "editvalues":
						//Transfers to the Values page. Done client-side, not called here.
						break;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			this.LoadAndBind();
		}

	}
}
