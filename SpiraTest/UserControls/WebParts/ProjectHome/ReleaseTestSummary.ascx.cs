using System;
using System.Data;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Linq;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the list of releases with associated test execution status
	/// </summary>
	public partial class ReleaseTestSummary : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ReleaseTestSummary::";

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
				//Add event handlers
				this.grdReleaseTestSummary.RowCommand += new GridViewCommandEventHandler(grdReleaseTestSummary_RowCommand);
				this.grdReleaseTestSummary.RowDataBound += new GridViewRowEventHandler(grdReleaseTestSummary_RowDataBound);

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
		/// Loads the control data
		/// </summary>
		public void LoadAndBindData()
		{
            int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
			List<ReleaseView> releases;
            int rowsToDisplay = 50;
            try
			{
                releases = releaseManager.RetrieveTestSummary(ProjectId, (releaseId < 1) ? null : (int?)releaseId);
			}
			catch (ArtifactNotExistsException)
			{
				//The release no longer exists so reset it and reload
                releaseId = -1;
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
                releases = releaseManager.RetrieveTestSummary(ProjectId, null);
			}
            // limit the number of rows and databind
            IEnumerable<ReleaseView> releasesSubset = releases.Take(rowsToDisplay);
            this.grdReleaseTestSummary.DataSource = releasesSubset;
			this.grdReleaseTestSummary.DataBind();
		}

		/// <summary>
		/// This event handler changes the current release to the one clicked in the datagrid
		/// </summary>
        /// <param name="source">The sending object</param>
		/// <param name="e">The event arguments</param>
		void grdReleaseTestSummary_RowCommand(object source, GridViewCommandEventArgs e)
		{
			//See which command was executed
			if (e.CommandName == "SelectRelease")
			{
				//Select the clicked release link
				int releaseId = Int32.Parse((string)e.CommandArgument);
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);

                //Now reload the widgets
                if (this.Page is Inflectra.SpiraTest.Web.ProjectHome)
                {
                    Inflectra.SpiraTest.Web.ProjectHome projectHomePage = (Inflectra.SpiraTest.Web.ProjectHome)this.Page;
                    projectHomePage.ReloadWidgets(true);
                }
                if (this.Page is Inflectra.SpiraTest.Web.ProjectHome2)
                {
                    Inflectra.SpiraTest.Web.ProjectHome2 projectHomePage = (Inflectra.SpiraTest.Web.ProjectHome2)this.Page;
                    projectHomePage.ReloadWidgets(true);
                }
                if (this.Page is Inflectra.SpiraTest.Web.ProjectHome3)
                {
                    Inflectra.SpiraTest.Web.ProjectHome3 projectHomePage = (Inflectra.SpiraTest.Web.ProjectHome3)this.Page;
                    projectHomePage.ReloadWidgets(true);
                }
			}
		}

		/// <summary>
		/// This event handler applies the conditional formatting to the datagrid
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		void grdReleaseTestSummary_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                ReleaseView releaseRow = (ReleaseView)e.Row.DataItem;

                //Add the Ajax tooltip handler to the template field
                if (e.Row.Cells[0].FindControl("btnSelectRelease") != null && releaseRow != null)
				{
					LinkButtonEx linkButton = (LinkButtonEx)e.Row.Cells[0].FindControl("btnSelectRelease");
					linkButton.Attributes.Add("onmouseover", "$find('" + this.grdReleaseTestSummary.ClientID + "').display_tooltip(" + releaseRow.ReleaseId + "," + ProjectId + ")");
					linkButton.Attributes.Add("onmouseout", "$find('" + this.grdReleaseTestSummary.ClientID + "').hide_tooltip()");
				}

				//Configure the equalizer bars for execution status
				int passedCount = 0;
				int failureCount = 0;
				int cautionCount = 0;
				int blockedCount = 0;
				int notRunCount = 0;
				int notApplicableCount = 0;
				if (releaseRow["CountPassed"] != DBNull.Value)
				{
					passedCount = (int)releaseRow["CountPassed"];
				}
				if (releaseRow["CountFailed"] != DBNull.Value)
				{
					failureCount = (int)releaseRow["CountFailed"];
				}
				if (releaseRow["CountCaution"] != DBNull.Value)
				{
					cautionCount = (int)releaseRow["CountCaution"];
				}
				if (releaseRow["CountBlocked"] != DBNull.Value)
				{
					blockedCount = (int)releaseRow["CountBlocked"];
				}
				if (releaseRow["CountNotRun"] != DBNull.Value)
				{
					notRunCount = (int)releaseRow["CountNotRun"];
				}
				if (releaseRow["CountNotApplicable"] != DBNull.Value)
				{
					notApplicableCount = (int)releaseRow["CountNotApplicable"];
				}

				//Calculate the percentages, handling rounding correctly
				//We don't include N/A ones in the total as they are either inactive or folders
				int totalCount = passedCount + failureCount + cautionCount + blockedCount + notRunCount;
				int percentPassed = 0;
				int percentFailed = 0;
				int percentCaution = 0;
				int percentBlocked = 0;
				int percentNotRun = 0;
				int percentNotApplicable = 0;
				if (totalCount != 0)
				{
					//Need check to handle divide by zero case
					percentPassed = (int)Decimal.Round(((decimal)passedCount * (decimal)100) / (decimal)totalCount, 0);
					percentFailed = (int)Decimal.Round(((decimal)failureCount * (decimal)100) / (decimal)totalCount, 0);
					percentCaution = (int)Decimal.Round(((decimal)cautionCount * (decimal)100) / (decimal)totalCount, 0);
					percentBlocked = (int)Decimal.Round(((decimal)blockedCount * (decimal)100) / (decimal)totalCount, 0);
					percentNotRun = (int)Decimal.Round(((decimal)notRunCount * (decimal)100) / (decimal)totalCount, 0);
					percentNotApplicable = (int)Decimal.Round(((decimal)notApplicableCount * (decimal)100) / (decimal)totalCount, 0);
				}

				//Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
                string tooltipText = "# " + Resources.Fields.Passed + "=" + passedCount + ", # " + Resources.Fields.Failed + "=" + failureCount + ", # " + Resources.Fields.Caution + "=" + cautionCount + ", # " + Resources.Fields.Blocked + "=" + blockedCount.ToString() + ", # " + Resources.Fields.NotRun + "=" + notRunCount.ToString();
				e.Row.Cells[2].Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
				e.Row.Cells[2].Attributes["onMouseOut"] = "hideddrivetip();";

				//Now populate the equalizer graph
				Equalizer eqlExecutionStatus = (Equalizer)e.Row.Cells[2].FindControl("eqlExecutionStatus");
				if (eqlExecutionStatus != null)
				{
					eqlExecutionStatus.PercentGreen = percentPassed;
					eqlExecutionStatus.PercentRed = percentFailed;
					eqlExecutionStatus.PercentOrange = percentCaution;
					eqlExecutionStatus.PercentYellow = percentBlocked;
					eqlExecutionStatus.PercentGray = percentNotRun;
				}

				//Finally set the total count label
                //Add the URL to the hyperlink
                if (e.Row.Cells[1].FindControl("lnkTestCaseCount") != null)
                {
                    HyperLinkEx lnkTestCaseCount = (HyperLinkEx)e.Row.Cells[1].FindControl("lnkTestCaseCount");
                    lnkTestCaseCount.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, releaseRow.ReleaseId, GlobalFunctions.PARAMETER_TAB_COVERAGE) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
                    lnkTestCaseCount.Text = totalCount.ToString();
                }
			}
		}
	}
}