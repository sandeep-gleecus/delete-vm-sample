using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;
using System.ComponentModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the test set execution summary graph
	/// </summary>
	public partial class TestSetStatus : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TestSetStatus::";

        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is 5
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        DefaultValue(5)
        ]
        public int RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
                this.rowsToDisplay = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected int rowsToDisplay = 5;

        #endregion

        /// <summary>
        /// Returns the base URL for redirecting to the test case list page with the appropriate filter
        /// </summary>
        protected string RedirectBaseUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl("~/GraphRedirect.ashx");
            }
        }

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
				//We have to set the subtitle programmatically for items that start out in the catalog
				this.Subtitle = "View Details";

				//Add any event handlers
				this.grdOverdueTestSets.RowCommand += new GridViewCommandEventHandler(grdOverdueTestSets_RowCommand);
                this.grdOverdueTestSets.RowDataBound += new GridViewRowEventHandler(grdOverdueTestSets_RowDataBound);

				//Specify the format of certain fields
                ((NameDescriptionFieldEx)(this.grdOverdueTestSets.Columns[0])).DataFormatString = GlobalFunctions.ARTIFACT_PREFIX_TEST_SET + GlobalFunctions.FORMAT_ID;

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
					this.Message.Text = "Unable to load '" + this.Title + "'";
					this.Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

        /// <summary>
        /// Displays the localized date in the overdue test set field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdOverdueTestSets_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                TestSetView testSetView = ((TestSetView)e.Row.DataItem);
                if (testSetView.PlannedDate.HasValue)
                {
                    LabelEx lblPlannedDate = (LabelEx)e.Row.Cells[1].FindControl("lblPlannedDate");
                    lblPlannedDate.Text = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(testSetView.PlannedDate.Value));
                    lblPlannedDate.ToolTip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(testSetView.PlannedDate.Value));
                }
            }
        }

		/// <summary>
		/// Called when an item in the overdue test set list is called
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdOverdueTestSets_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			//See what command was executed
			if (e.CommandName == "ViewTestSet")
			{
                int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

				//Set the release and coverage filter id and redirect
				ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST);
				filters.Clear();
				if (releaseId > 0)
				{
					filters.Add("ReleaseId", releaseId);
				}
				DateRange dateRange = new DateRange();
				dateRange.EndDate = DateTime.UtcNow.Date;
				filters.Add("PlannedDate", dateRange);
				filters.Save();

				//Get the test set id and redirect
				int testSetId = Int32.Parse((string)e.CommandArgument);
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, testSetId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE, true);
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
		/// Loads the control data
		/// </summary>
		public void LoadAndBindData()
		{
            int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

			Business.TestSetManager testSetManager = new Business.TestSetManager();

			//Now get the list of overdue test sets
            List<TestSetView> overdueTestSets = testSetManager.RetrieveOverdue(ProjectId, (releaseId < 1) ? null : (int?)releaseId).Take(rowsToDisplay).ToList();

            //Set the release field for the graph
            if (releaseId < 1)
            {
                this.hdnReleaseId.Value = "";
            }
            else
            {
                this.hdnReleaseId.Value = releaseId.ToString();
            }

			//Databind
            this.grdOverdueTestSets.DataSource = overdueTestSets;
			this.grdOverdueTestSets.DataBind();
		}
	}
}