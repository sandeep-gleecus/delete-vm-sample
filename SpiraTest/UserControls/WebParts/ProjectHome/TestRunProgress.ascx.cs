using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    public partial class TestRunProgress : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TestRunProgress::";

        /// <summary>
        /// The date format to use
        /// </summary>
        public bool DateFormatMonthFirst { get; set; }

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
        /// Loads the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
		    //Now load the content
            if (WebPartVisible)
            {
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Loads the data in the control
        /// </summary>
        public void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

			try
			{
                //Get the release id from settings
                int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);
                if (releaseId.HasValue && releaseId < 0)
                {
                    releaseId = null;
                }

                //We need to find the most recent test run for this release
                DateTime endDate = DateTime.UtcNow;
                Hashtable filters = new Hashtable();
                if (releaseId.HasValue)
                {
                    filters.Add("ReleaseId", releaseId.Value);
                    hdnReleaseId.Value = releaseId.Value.ToString();
                }
                else
                {
                    hdnReleaseId.Value = null;
                }
                TestRunView lastTestRun = new TestRunManager().Retrieve(ProjectId, "EndDate", false, 1, 1, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset()).FirstOrDefault();
                if (lastTestRun != null && lastTestRun.EndDate.HasValue)
                {
                    endDate = lastTestRun.EndDate.Value;
                }
                DateRange dateRange = new DateRange();
                dateRange.ConsiderTimes = false;
                dateRange.StartDate = endDate.AddDays(-30);
                dateRange.EndDate = endDate;
                this.hdnDateRange.Value = dateRange.ToString();

                //Finally we need to specify if the current date-format is M/D or D/M (no years displayed to save space)
                string dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern.ToLowerInvariant();
                this.DateFormatMonthFirst = dateFormat.StartsWith("m");
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
    }
}