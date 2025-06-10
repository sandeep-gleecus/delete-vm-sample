using System;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;
using System.Linq;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the list of releases with the summary of the task progress per release
	/// </summary>
	public partial class ReleaseTaskProgress : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ReleaseTaskProgress::";

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
				//Add the event handlers
				this.grdReleaseTaskProgress.RowCommand += new GridViewCommandEventHandler(grdReleaseTaskProgress_RowCommand);
				this.grdReleaseTaskProgress.RowDataBound += new GridViewRowEventHandler(grdReleaseTaskProgress_RowDataBound);

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
				if (this.Message == null)
                {
                    Response.Write("Unable to load '" + this.Title + "'");
                }
                else
				{
					this.Message.Text = "Unable to load '" + this.Title + "'";
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
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                releases = releaseManager.RetrieveTestSummary(ProjectId, null);
			}

            // limit the number of rows and databind
            IEnumerable<ReleaseView> releasesSubset = releases.Take(rowsToDisplay);
            this.grdReleaseTaskProgress.DataSource = releasesSubset;
			this.grdReleaseTaskProgress.DataBind();
		}

		/// <summary>
		/// This event handler changes the current release to the one clicked in the datagrid
		/// </summary>
        /// <param name="source">The sending object</param>
		/// <param name="e">The event arguments</param>
		void grdReleaseTaskProgress_RowCommand(object source, GridViewCommandEventArgs e)
		{
			//See which command was executed
			if (e.CommandName == "SelectRelease")
			{
				//Select the clicked release link
				int releaseId = Int32.Parse((string)e.CommandArgument);
				//this.ddlSelectRelease.SelectedValue = releaseId.ToString();
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
		void grdReleaseTaskProgress_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                ReleaseView releaseRow = (ReleaseView)e.Row.DataItem;

				//Add the Ajax tooltip handler to the template field
                if (e.Row.Cells[0].FindControl("btnSelectRelease") != null && releaseRow != null)
				{
					LinkButtonEx linkButton = (LinkButtonEx)e.Row.Cells[0].FindControl("btnSelectRelease");
                    linkButton.Attributes.Add("onmouseover", "$find('" + this.grdReleaseTaskProgress.ClientID + "').display_tooltip(" + releaseRow.ReleaseId + "," + ProjectId + ")");
					linkButton.Attributes.Add("onmouseout", "$find('" + this.grdReleaseTaskProgress.ClientID + "').hide_tooltip()");
				}

                //Add the URL to the hyperlink
                if (e.Row.Cells[1].FindControl("lnkTaskCount") != null)
                {
                    HyperLinkEx lnkTaskCount = (HyperLinkEx)e.Row.Cells[1].FindControl("lnkTaskCount");
                    lnkTaskCount.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, releaseRow.ReleaseId, GlobalFunctions.PARAMETER_TAB_TASK) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_HOME + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
                }

				//First see how many tasks we have
				int taskCount = releaseRow.TaskCount;

				//Handle the no tasks case first
				if (taskCount == 0)
				{
                    e.Row.Cells[4].Text = ReleaseManager.GenerateTaskProgressTooltip(releaseRow);
					e.Row.Cells[4].CssClass = "NotCovered";
				}
				else
				{
					//Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
                    string tooltipText = GlobalFunctions.JSEncode(ReleaseManager.GenerateTaskProgressTooltip(releaseRow));
					e.Row.Cells[4].Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
					e.Row.Cells[4].Attributes["onMouseOut"] = "hideddrivetip();";

					//Now populate the equalizer graph
					Equalizer eqlExecutionStatus = (Equalizer)e.Row.Cells[4].FindControl("eqlTaskProgress");
					if (eqlExecutionStatus != null)
					{
						eqlExecutionStatus.PercentGreen = releaseRow.TaskPercentOnTime;
						eqlExecutionStatus.PercentRed = releaseRow.TaskPercentLateFinish;
						eqlExecutionStatus.PercentYellow = releaseRow.TaskPercentLateStart;
						eqlExecutionStatus.PercentGray = releaseRow.TaskPercentNotStart;
					}

					//Finally if the projected effort is greater than the estimated effort, indicate with warning css class
					if (releaseRow.TaskProjectedEffort.HasValue && releaseRow.TaskEstimatedEffort.HasValue)
					{
						if (releaseRow.TaskProjectedEffort.Value > releaseRow.TaskEstimatedEffort.Value)
						{
                            e.Row.Cells[3].CssClass = "Warning priority3";
						}
					}
				}
			}
		}
	}
}