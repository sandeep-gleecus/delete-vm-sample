using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome
{
	/// <summary>
	/// Displays the list of open project issues
	/// </summary>
	public partial class OpenIssues : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.OpenIssues::";

		#region Enumerations

		public enum OpenIssueOrganizeBy
		{
			Priority = 1,
			Severity = 2
		}

		#endregion

		#region User Configurable Properties

		/// <summary>
		/// Stores how many rows of data to display, default is unlimited
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

		/// <summary>
		/// Determines whether to display priority or severity as the displayed/sorted column
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
		WebDisplayName("Use Priority or Severity"),
		WebDescription("Determines whether to display priority or severity as the displayed/sorted column")
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
				//We have to set the message box programmatically for items that start out in the catalog
				this.MessageBoxId = "lblMessage";

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
            //Get the project group id
            int projectGroupId = ProjectGroupId;

			IncidentManager incidentManager = new IncidentManager();
			//Now get the list of open issues in order of descreasing priority or severity as appropriate
			bool useSeverity = (this.OrganizeBy == OpenIssueOrganizeBy.Severity);
            List<IncidentView> openIssues = incidentManager.RetrieveOpenIssues(projectGroupId, RowsToDisplay, useSeverity);

			//Specify whether to display severity or priority
			if (useSeverity)
			{
				this.grdIssueList.Columns[3].Visible = false;
				this.grdIssueList.Columns[4].Visible = true;
			}
			else
			{
				this.grdIssueList.Columns[3].Visible = true;
				this.grdIssueList.Columns[4].Visible = false;
			}

            this.grdIssueList.DataSource = openIssues;
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
                IncidentView incident = (IncidentView)(e.Row.DataItem);
				//First lets handle the color of the priority column
                if (incident.PriorityId.HasValue)
				{
                    Color backColor = Color.FromName("#" + incident.PriorityColor);
					e.Row.Cells[3].BackColor = backColor;
				}

				//Next lets handle the color of the severity column
                if (incident.SeverityId.HasValue)
				{
                    Color backColor = Color.FromName("#" + incident.SeverityColor);
					e.Row.Cells[4].BackColor = backColor;
				}

                //Specify the project id and artifact id in the url
                HyperLinkEx hyperlink = (HyperLinkEx)e.Row.Cells[1].Controls[0];
                if (hyperlink != null)
                {
                    hyperlink.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, incident.ProjectId, incident.IncidentId);
                }
            }
		}
	}
}