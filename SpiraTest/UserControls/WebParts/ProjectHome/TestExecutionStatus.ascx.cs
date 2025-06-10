using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the test execution summary graph
	/// </summary>
	public partial class TestExecutionStatus : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TestExecutionStatus::";

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
				//Add any event handlers
                this.lstDailyRunCount.ItemCommand += new System.Web.UI.WebControls.DataListCommandEventHandler(lstDailyRunCount_ItemCommand);

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
        /// Called when a link in the daily run count is clicked
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void lstDailyRunCount_ItemCommand(object source, System.Web.UI.WebControls.DataListCommandEventArgs e)
        {
            if (e.CommandArgument != null)
            {
                DateTime endDate;
                if (DateTime.TryParse(e.CommandArgument.ToString(), out endDate))
                {
                    //Redirect to the test run list page, setting a filter on end date
                    ProjectSettingsCollection filterList = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST);
                    filterList.Clear();
                    DateRange dateRangeFilter = new DateRange();
                    dateRangeFilter.ConsiderTimes = false;
                    dateRangeFilter.StartDate = endDate;
                    dateRangeFilter.EndDate = endDate;
                    filterList.Add("EndDate", dateRangeFilter);
                    filterList.Save();
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, this.ProjectId));
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
		/// Loads the control data
		/// </summary>
		public void LoadAndBindData()
		{
            int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

			Business.TestRunManager testRunManager = new Business.TestRunManager();

            //First get the current timezone offset so that we get the daily count with respect to the correct
            //timezone
            TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(SpiraContext.Current.TimezoneId);
            double utcOffset = timezone.GetUtcOffset(DateTime.Now).TotalHours;

			//Now get the test run daily count and overall count
            List<TestRun_DailyCount> dailyRunCounts = testRunManager.RetrieveDailyCount(ProjectId, utcOffset, (releaseId < 1) ? null : (int?)releaseId);
            int runCount;
			if (releaseId < 1)
			{
                runCount = testRunManager.Count(ProjectId);
                this.hdnReleaseId.Value = "";
            }
			else
			{
                this.hdnReleaseId.Value = releaseId.ToString();
				Hashtable filters = new Hashtable();
				filters.Add("ReleaseId", releaseId);
                runCount = testRunManager.Count(ProjectId, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
			}

			//Databind
            this.lstDailyRunCount.DataSource = dailyRunCounts;
			this.lstDailyRunCount.DataBind();

			//Populate the test execution status widget
			this.lblTotalRunCount.Text = runCount.ToString();
		}
	}
}