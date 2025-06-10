using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome
{
	/// <summary>
	/// Displays the incident aging for the group and the open count for its constituent projects
	/// </summary>
	public partial class IncidentAging : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.IncidentAging::";

		#region User Configurable Properties

		/// <summary>
		/// Should we display the project group summary graph
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("ProjectGroupHome_DisplaySummaryGraphSetting"),
        LocalizedWebDescription("ProjectGroupHome_DisplaySummaryGraphSettingTooltip"),
		DefaultValue(true)
		]
		public bool DisplaySummaryGraph
		{
			get
			{
				return this.displaySummaryGraph;
			}
			set
			{
				this.displaySummaryGraph = value;
				LoadAndBindData();
			}
		}
		protected bool displaySummaryGraph = true;

		/// <summary>
		/// Should we display the individual detailed records for each project
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("ProjectGroupHome_DisplayProjectDetailSetting"),
        LocalizedWebDescription("ProjectGroupHome_DisplayProjectDetailSettingTooltip"),
		DefaultValue(true)
		]
		public bool DisplayProjectDetail
		{
			get
			{
				return this.displayProjectDetail;
			}
			set
			{
				this.displayProjectDetail = value;
				LoadAndBindData();
			}
		}
		protected bool displayProjectDetail = true;

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
				//Register event handlers
				this.grdProjectIncidentOpenCount.RowCreated += new GridViewRowEventHandler(grdProjectIncidentOpenCount_RowCreated);

                //Configure the project name link to have the right URL
                NameDescriptionFieldEx field = (NameDescriptionFieldEx)this.grdProjectIncidentOpenCount.Columns[1];
                if (field != null)
                {
                    field.NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, -3);
                }

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
		/// Gets the open incident count (by priority) for each project
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdProjectIncidentOpenCount_RowCreated(object sender, GridViewRowEventArgs e)
		{
			const string METHOD_NAME = "grdProjectIncidentOpenCount_RowCreated";

			try
			{
				//Get the project id
				if (e.Row.RowType == DataControlRowType.DataRow)
				{
                    ProjectView projectView = (ProjectView)e.Row.DataItem;
                    int projectId = projectView.ProjectId;

					//Now get the open incident count for this project and build an "equalizer" graph
					//we can't use the built-in equalizer control as the colors are custom for incidents
					//as are the number of statuses!!

					IncidentManager incidentManager = new IncidentManager();
					List<IncidentOpenCountByPrioritySeverity> openCounts = incidentManager.RetrieveOpenCountByPrioritySeverity(projectId, null, false, false);
					double incidentCount = 0;
                    foreach (IncidentOpenCountByPrioritySeverity openCount in openCounts)
					{
						incidentCount += openCount.Count.Value;
					}
					//Generate the equalizer with the percentages
					TableCell tableCell = e.Row.Cells[3];
					if (tableCell != null)
					{
						Label parentControl = new Label();
                        parentControl.CssClass = "Equalizer";
						tableCell.Controls.Add(parentControl);
						string tooltipText = "";
                        foreach (IncidentOpenCountByPrioritySeverity openCount in openCounts)
						{
							string colorName = "e0e0e0";
                            if (!String.IsNullOrEmpty(openCount.PrioritySeverityColor))
							{
								colorName = openCount.PrioritySeverityColor;
							}
							string prioritySeverityName = openCount.PrioritySeverityName;
                            int count = openCount.Count.Value;
							int percentage = (int)(((double)count) / incidentCount * 100.0D);
							if (tooltipText != "")
							{
								tooltipText += ", ";
							}
							tooltipText += "[" + prioritySeverityName + "] = " + count.ToString();

							//Need to add additional tags in the case of Firefox which doesn't support inline SPAN widths
							bool isFirefox = false;
							if (Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "mozilla" || Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "firefox" || Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "netscape")
							{
								isFirefox = true;
							}

							//Add a new label to the end of the bar to build out the "equalizer" graph
							Label label = new Label();
							if (isFirefox)
							{
								label.Style.Add(HtmlTextWriterStyle.PaddingLeft, percentage.ToString() + "px");
							}
							else
							{
								label.Width = Unit.Pixel(percentage);
							}
							label.CssClass = "EqualizerGeneric";
							label.BackColor = Color.FromName("#" + colorName);
							parentControl.Controls.Add(label);
						}
						//Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
						tableCell.Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
						tableCell.Attributes["onMouseOut"] = "hideddrivetip();";
					}

					//Populate the total number of open incident
					e.Row.Cells[2].Text = incidentCount.ToString();
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
		/// Loads the control data
		/// </summary>
		protected void LoadAndBindData()
		{
            //Get the project group id
            int projectGroupId = ProjectGroupId;

			//Get the incident aging for all the projects in the group aggregated
			if (this.DisplaySummaryGraph)
			{
                //Set the other values
                this.hdnMaxAging.Value = 90.ToString();
                this.hdnTimeInterval.Value = 15.ToString();
				this.plcIncidentAging.Visible = true;
			}
			else
			{
				this.plcIncidentAging.Visible = false;
			}

			//Now get the list of projects
			if (this.DisplayProjectDetail)
			{
				ProjectManager projectManager = new ProjectManager();
				Hashtable filters = new Hashtable();
				filters.Add("ProjectGroupId", projectGroupId);
                filters.Add("ActiveYn", "Y");
                List<ProjectView> projects = projectManager.Retrieve(filters, null);
                this.grdProjectIncidentOpenCount.DataSource = projects;
				this.grdProjectIncidentOpenCount.DataBind();
				this.grdProjectIncidentOpenCount.Visible = true;
			}
			else
			{
				this.grdProjectIncidentOpenCount.Visible = false;
			}
		}
	}
}