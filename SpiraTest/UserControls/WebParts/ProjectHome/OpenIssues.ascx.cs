using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the list of open project issues
	/// </summary>
	public partial class OpenIssues : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.OpenIssues::";

		#region Enumerations

		public enum OpenIssueOrganizeBy
		{
			Priority = 1,
			Severity = 2
		}

		#endregion

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
                int rowsToDisplayMax = 50;
                this.rowsToDisplay = value < rowsToDisplayMax ? value : rowsToDisplayMax;
                //Force the data to reload
                LoadAndBindData();
			}
		}
		protected int rowsToDisplay = 5;

		/// <summary>
		/// Determines whether to display priority or severity as the displayed/sorted column
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("Global_UsePrioritySeveritySetting"),
        LocalizedWebDescription("Global_UsePrioritySeveritySettingTooltip")
		]
		public OpenIssueOrganizeBy OrganizeBy
		{
			get
			{
				return this.organizeBy;
			}
			set
			{
				this.organizeBy = value;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected OpenIssueOrganizeBy organizeBy = OpenIssueOrganizeBy.Priority;

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
				//Add the event handlers
				this.grdIssueList.RowDataBound += new GridViewRowEventHandler(grdIssueList_RowDataBound);

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
			//Get the release id from settings
            int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);
            if (releaseId.HasValue && releaseId < 0)
            {
                releaseId = null;
            }

			IncidentManager incidentManager = new IncidentManager();
			//Now get the list of open issues in order of descreasing priority or severity as appropriate
			bool useSeverity = (this.OrganizeBy == OpenIssueOrganizeBy.Severity);
            List<IncidentView> openOssues;
			try
			{
                openOssues = incidentManager.RetrieveOpenIssues(ProjectId, releaseId, RowsToDisplay, useSeverity);
			}
			catch (ArtifactNotExistsException)
			{
				//The release no longer exists so reset it and reload
				releaseId = null;
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
                openOssues = incidentManager.RetrieveOpenIssues(ProjectId, releaseId, RowsToDisplay, useSeverity);
			}

			//Specify whether to display severity or priority
			if (useSeverity)
			{
				this.grdIssueList.Columns[2].Visible = false;
				this.grdIssueList.Columns[3].Visible = true;
			}
			else
			{
				this.grdIssueList.Columns[2].Visible = true;
				this.grdIssueList.Columns[3].Visible = false;
			}

            //Set the navigate url for the name field
            ((NameDescriptionFieldEx)this.grdIssueList.Columns[1]).NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -3);

            this.grdIssueList.DataSource = openOssues;
			this.grdIssueList.DataBind();
		}

		/// <summary>
		/// This event handler applies the conditional formatting to the datagrid
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void grdIssueList_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				//First lets handle the color of the priority column
                IncidentView incident = (IncidentView)(e.Row.DataItem);
                if (incident.PriorityId.HasValue)
				{
                    Color backColor = Color.FromName("#" + incident.PriorityColor);
					e.Row.Cells[2].BackColor = backColor;
				}

				//Next lets handle the color of the severity column
                if (incident.SeverityId.HasValue)
                {
                    Color backColor = Color.FromName("#" + incident.SeverityColor);
					e.Row.Cells[3].BackColor = backColor;
				}
			}
		}
	}
}