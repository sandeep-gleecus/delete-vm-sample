using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
	public partial class TestSetList : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.TestSetList::";

		#region User Configurable Properties

		/// <summary>
		/// Stores how many rows of data to display, default is 10
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
		WebDisplayName("# Rows To Display"),
		WebDescription("This property limits the number of rows of data displayed"),
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
                    string rssUrl = this.ResolveUrl("~/Feeds/" + UserId + "/" + UserRssToken + "/AssignedTestSets.aspx");
                    this.Subtitle = GlobalFunctions.WEBPART_SUBTITLE_RSS + rssUrl;
                }
                
                //Register the event handlers
				this.grdTestSets.RowDataBound += new GridViewRowEventHandler(grdTestSets_RowDataBound);

                //Add the client event handler to the background task process
                Dictionary<string, string> handlers = new Dictionary<string, string>();
                handlers.Add("succeeded", WebPartUniqueId + "_ajxBackgroundProcessManager_success");
                this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

				//Now load the content if visible
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
		/// This event handler applies any conditional formatting to the datagrid before display
		/// </summary>
		/// <param name="sender">The object that raised the event</param>
		/// <param name="e">The parameters passed to handler</param>
		void grdTestSets_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				//Get the data item
                TestSetView testSetView = (TestSetView)e.Row.DataItem;

				//If the due-date is in the past, change its css class to indicate this
				//(we only consider the date component not the time component)
                if (testSetView.PlannedDate.HasValue)
				{
                    DateTime plannedDate = testSetView.PlannedDate.Value;
					if (plannedDate.Date < DateTime.UtcNow.Date)
					{
                        e.Row.Cells[2].CssClass = "Warning priority2";
					}
				}

				//Add the Ajax handler to the template field
                if (e.Row.Cells[0].FindControl("lnkViewTestSet") != null && e.Row.DataItem != null)
				{
                    HyperLinkEx hyperlink = (HyperLinkEx)e.Row.Cells[0].FindControl("lnkViewTestSet");
                    hyperlink.Attributes.Add("onmouseover", "$find('" + this.grdTestSets.ClientID + "').display_tooltip(" + testSetView.TestSetId + "," + testSetView.ProjectId + ")");
                    hyperlink.Attributes.Add("onmouseout", "$find('" + this.grdTestSets.ClientID + "').hide_tooltip()");

                    //Need to set the actual URL of the HyperLink
                    hyperlink.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, testSetView.ProjectId, testSetView.TestSetId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
				}
			}
		}

		/// <summary>
		/// Returns a handle to the interface
		/// </summary>
		/// <returns>IWebPartReloadable</returns>
		[ConnectionProvider("ReloadableProvider", "ReloadableProvider", AllowsMultipleConnections = true)]
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

			//Now get the list of test sets owned by the user
			TestSetManager testSetManager = new Business.TestSetManager();
            List<TestSetView> testSets = testSetManager.RetrieveByOwnerId(UserId, filterProjectId, this.rowsToDisplay);

            this.grdTestSets.DataSource = testSets;
            this.grdTestSets.DataBind();

            //Databind so that it gets the message control client id
            this.ajxBackgroundProcessManager.DataBind();
		}
	}
}
