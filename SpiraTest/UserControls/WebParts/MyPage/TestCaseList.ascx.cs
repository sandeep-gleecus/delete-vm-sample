using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
	public partial class TestCaseList : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.TestCaseList::";

        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is 10
        /// </summary>
        [
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
		DefaultValue(10)
		]
		public int RowsToDisplay
		{
			get
			{
				return this.rowsToDisplay;
			}
			set
			{
				int rowsToDisplayMax = 50;
				this.rowsToDisplay = value < rowsToDisplayMax ? value : rowsToDisplayMax;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected int rowsToDisplay = 10;

		/// <summary>
		/// Should we show the last execution date (Default: FALSE)
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
		LocalizedWebDisplayName("TestCaseList_ShowLastExecuted"),
		LocalizedWebDescription("TestCaseList_ShowLastExecuted"),
		DefaultValue(true)
		]
		public bool ShowLastExecuted
		{
			get
			{
				return this.showLastExecuted;
			}
			set
			{
				this.showLastExecuted = value;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected bool showLastExecuted = false;

		/// <summary>
		/// Should we show the workflow status (Default: TRUE)
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
		LocalizedWebDisplayName("TestCaseList_ShowWorkflowStatus"),
		LocalizedWebDescription("TestCaseList_ShowWorkflowStatus"),
		DefaultValue(true)
		]
		public bool ShowWorkflowStatus
		{
			get
			{
				return this.showWorkflowStatus;
			}
			set
			{
				this.showWorkflowStatus = value;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected bool showWorkflowStatus = true;


		#endregion

		#region Other Properties

		/// <summary>
		/// Returns the base url for redirecting to a created pending test run entry
		/// </summary>
		public string TestRunsPendingUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestExecute, -2, -2) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE);
            }
        }

        /// <summary>
        /// Returns the base url for redirecting to a created pending test run entry - when execution should be in exploratory mode
        /// </summary>
        protected string TestRunsPendingExploratoryUrl
        {
            get
            {
                //The referrer is the Test Case list because the test execution page doesn't currently handle returning to the test case details page
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestExecuteExploratory, -2, -2) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE);
            }
        }

        #endregion

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
                //Set the RSS Feed link if it's enabled
                if (!String.IsNullOrEmpty(UserRssToken))
                {
                    string rssUrl = this.ResolveUrl("~/Feeds/" + UserId + "/" + UserRssToken + "/AssignedTestCases.aspx");
                    this.Subtitle = GlobalFunctions.WEBPART_SUBTITLE_RSS + rssUrl;
                }

                //Register event handlers
                this.grdTestCases.RowDataBound += new GridViewRowEventHandler(grdTestCases_RowDataBound);

                //Add the client event handler to the background task process
                Dictionary<string, string> handlers = new Dictionary<string, string>();
                handlers.Add("succeeded", WebPartUniqueId + "_ajxBackgroundProcessManager_success");
                this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

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
		/// Loads the data in the web part
		/// </summary>
		public void LoadAndBindData()
		{
			//Get the current project filter (if any)
			int? filterProjectId = null;
			if (GetUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false) && ProjectId > 0)
			{
				filterProjectId = ProjectId;
			}

			//See if we should hide any columns (to save space)
			this.grdTestCases.Columns[4].Visible = this.ShowLastExecuted;
			this.grdTestCases.Columns[5].Visible = this.ShowWorkflowStatus;

			//Now get the list of test cases owned by the user
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseView> testCases = testCaseManager.RetrieveByOwnerId(UserId, filterProjectId, this.rowsToDisplay);

            this.grdTestCases.DataSource = testCases;
			this.grdTestCases.DataBind();

            //Databind the background process manager so that it picks up the MessageBox client id
            this.ajxBackgroundProcessManager.DataBind();
        }

		/// <summary>
		/// This event handler applies any conditional formatting to the datagrid before display
		/// </summary>
		/// <param name="sender">The object that raised the event</param>
		/// <param name="e">The parameters passed to handler</param>
		private void grdTestCases_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                TestCaseView testCaseView = ((TestCaseView)e.Row.DataItem);
                //Add the Ajax handler to the template field
				if (e.Row.Cells[1].FindControl("lnkViewTest") != null && e.Row.DataItem != null)
				{
                    HyperLinkEx hyperlink = (HyperLinkEx)e.Row.Cells[1].FindControl("lnkViewTest");
                    hyperlink.Attributes.Add("onmouseover", "$find('" + this.grdTestCases.ClientID + "').display_tooltip(" + testCaseView.TestCaseId + "," + testCaseView.ProjectId + ")");
                    hyperlink.Attributes.Add("onmouseout", "$find('" + this.grdTestCases.ClientID + "').hide_tooltip()");
                        
                    //Need to set the actual URL of the HyperLink
                    hyperlink.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, testCaseView.ProjectId, testCaseView.TestCaseId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;

                    //Get the ProjectSettings
                    ProjectSettings projectSettings = null;
                    if (testCaseView.ProjectId > 0)
                    {
                        projectSettings = new ProjectSettings(testCaseView.ProjectId);
                    }
                    //Disable the execute button if testing settings do not allow test cases to be executed
                    if (projectSettings != null && projectSettings.Testing_ExecuteSetsOnly)
                    {
                        HyperLinkEx btnExecute = (HyperLinkEx)e.Row.Cells[5].FindControl("btnExecuteTest");
                        btnExecute.Visible = false;
                    }
                }

				//Add the Css Execution status
                string cssClass = GlobalFunctions.GetExecutionStatusCssClass(testCaseView.ExecutionStatusId);
				e.Row.Cells[3].CssClass = cssClass;
			}
		}
	}
}
