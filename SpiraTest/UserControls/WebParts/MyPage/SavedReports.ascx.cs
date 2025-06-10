using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
	/// <summary>
	/// Displays a list of reports the user has saved
	/// </summary>
	public partial class SavedReports : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.SavedReports::";

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
				//We have to set the message box programmatically for items that start out in the catalog
				this.MessageBoxId = "lblMessage";

				//Add event handlers
				this.grdSavedReports.RowCommand += new GridViewCommandEventHandler(grdSavedReports_RowCommand);

				//Now load the content
                if (WebPartVisible)
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
		/// Returns a handle to the interface
		/// </summary>
		/// <returns>IWebPartReloadable</returns>
		[ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

		/// <summary>
		/// Loads and binds the data
		/// </summary>
		public void LoadAndBindData()
		{
			//Get the current project filter (if any)
            ReportManager reportManager = new ReportManager();
			List<SavedReportView> savedReports;
			if (GetUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false) && ProjectId > 0)
			{
				//Now get the list of saved reports that belong to the current user and current project (no project-shared reports)
				int filterProjectId = ProjectId;
                savedReports = reportManager.RetrieveSaved(UserId, filterProjectId, false);
			}
			else
			{
				//Now get the list of saved reports that belong to the current user
                savedReports = reportManager.RetrieveSaved(UserId);
			}

            this.grdSavedReports.DataSource = savedReports;
			this.grdSavedReports.DataBind();
		}

		/// <summary>
		/// Called when link-buttons in the saved reports grid are clicked
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		void grdSavedReports_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "DeleteReport")
			{
				//Delete the saved report
                ReportManager reportManager = new ReportManager();
				int reportSavedId = Int32.Parse((string)e.CommandArgument);
				reportManager.DeleteSaved(reportSavedId);

				//Now refresh the list
				LoadAndBindData();
			}
		}
	}
}
