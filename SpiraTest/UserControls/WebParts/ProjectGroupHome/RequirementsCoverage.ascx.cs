using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
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
	/// Displays the requirements coverage for the group and its constituent projects
	/// </summary>
	public partial class RequirementsCoverage : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RequirementsCoverageNew::";

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

        /// <summary>
        /// Should we display only the data related to active releases in the projects
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("ProjectGroupHome_ActiveReleasesOnlySetting"),
        LocalizedWebDescription("ProjectGroupHome_ActiveReleasesOnlySettingTooltip"),
        DefaultValue(true)
        ]
        public bool ActiveReleasesOnly
        {
            get
            {
                return this.activeReleasesOnly;
            }
            set
            {
                this.activeReleasesOnly = value;
                LoadAndBindData();
            }
        }
        protected bool activeReleasesOnly = true;

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
				this.grdProjectRequirementsCoverage.RowCreated += new GridViewRowEventHandler(grdProjectRequirementsCoverage_RowCreated);

                //Configure the project name link to have the right URL
                NameDescriptionFieldEx field = (NameDescriptionFieldEx)this.grdProjectRequirementsCoverage.Columns[1];
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
		/// Gets the detailed requirements coverage info for each project
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdProjectRequirementsCoverage_RowCreated(object sender, GridViewRowEventArgs e)
		{
			const string METHOD_NAME = "grdProjectRequirementsCoverage_RowCreated";

			try
			{
				//Get the project id
				if (e.Row.RowType == DataControlRowType.DataRow)
				{
                    ProjectView projectView = (ProjectView)e.Row.DataItem;
                    int projectId = projectView.ProjectId;

					//Now get the coverage for this project and populate the equalizer
					Equalizer eqlExecutionStatus = (Equalizer)e.Row.Cells[3].FindControl("eqlReqCoverage");
					RequirementManager requirementManager = new RequirementManager();
                    List<RequirementCoverageSummary> requirementCoverages = requirementManager.RetrieveCoverageSummary(projectId, (activeReleasesOnly) ? (int?)RequirementManager.RELEASE_ID_ACTIVE_RELEASES_ONLY : null, false);
					double reqCount = 0;
                    foreach (RequirementCoverageSummary requirementCoverage in requirementCoverages)
					{
                        if (requirementCoverage.CoverageCount.HasValue)
						{
                            reqCount += requirementCoverage.CoverageCount.Value;
						}
					}
					//Populate the equalizer with the percentages
					if (eqlExecutionStatus != null && reqCount > 0)
					{
						string tooltipText = "";
                        foreach (RequirementCoverageSummary requirementCoverage in requirementCoverages)
						{
							int coverageStatus = requirementCoverage.CoverageStatusOrder;
							string coverageCaption = requirementCoverage.CoverageStatus;
                            double coverageCount = requirementCoverage.CoverageCount.Value;
							int percentage = (int)(coverageCount / reqCount * 100.0D);
							if (percentage < 0 || percentage > 100)
							{
								percentage = 0;
							}
							if (tooltipText != "")
							{
								tooltipText += ", ";
							}
							tooltipText += "# " + coverageCaption + " = " + coverageCount.ToString("0.0");
							switch (coverageStatus)
							{
								case 1:
									//Passed
									eqlExecutionStatus.PercentGreen = percentage;
									break;
								case 2:
									//Failed
									eqlExecutionStatus.PercentRed = percentage;
									break;
								case 3:
									//Blocked
									eqlExecutionStatus.PercentYellow = percentage;
									break;
								case 4:
									//Caution
									eqlExecutionStatus.PercentOrange = percentage;
									break;
								case 5:
									//Not Run
									eqlExecutionStatus.PercentGray = percentage;
									break;
								case 6:
									//Not Covered
									eqlExecutionStatus.PercentBlue = percentage;
									break;
							}
						}
						//Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
						e.Row.Cells[3].Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
						e.Row.Cells[3].Attributes["onMouseOut"] = "hideddrivetip();";
					}

					//Populate the requirement count cell
					e.Row.Cells[2].Text = ((int)reqCount).ToString();
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

			//Get the requirements coverage data for all the projects in the group aggregated
			if (this.DisplaySummaryGraph)
			{
                this.plcGroupSummaryGraph.Visible = true;
			}
			else
			{
                this.plcGroupSummaryGraph.Visible = false;
			}

			//Now get the list of projects
			if (this.DisplayProjectDetail)
			{
				Business.ProjectManager projectManager = new Business.ProjectManager();
				Hashtable filters = new Hashtable();
                filters.Add("ActiveYn", "Y"); 
                filters.Add("ProjectGroupId", projectGroupId);
                List<ProjectView> projects = projectManager.Retrieve(filters, null);
                this.grdProjectRequirementsCoverage.DataSource = projects;
				this.grdProjectRequirementsCoverage.DataBind();
				this.grdProjectRequirementsCoverage.Visible = true;
			}
			else
			{
				this.grdProjectRequirementsCoverage.Visible = false;
			}
		}
	}
}