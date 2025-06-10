using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome
{
	/// <summary>
	/// Displays the test execution status for the group and its constituent projects
	/// </summary>
	public partial class TestExecutionStatus : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.TestExecutionStatus::";

        #region User Configurable Properties

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
				this.grdProjectExecutionStatus.RowCreated += new GridViewRowEventHandler(grdProjectExecutionStatus_RowCreated);

                //Configure the project name link to have the right URL
                NameDescriptionFieldEx field = (NameDescriptionFieldEx)this.grdProjectExecutionStatus.Columns[1];
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
		/// Gets the detailed test execution info for each project
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdProjectExecutionStatus_RowCreated(object sender, GridViewRowEventArgs e)
		{
			const string METHOD_NAME = "grdProjectExecutionStatus_RowCreated";

			try
			{
				//Get the project id
				if (e.Row.RowType == DataControlRowType.DataRow)
				{
                    ProjectView projectView = (ProjectView)e.Row.DataItem;
                    int projectId = projectView.ProjectId;

					//Now get the coverage for this project and populate the equalizer
					Equalizer eqlExecutionStatus = (Equalizer)e.Row.Cells[4].FindControl("eqlExecutionStatus");
					Business.TestCaseManager testCaseManager = new Business.TestCaseManager();
                    List<TestCase_ExecutionStatusSummary> executionSummary = testCaseManager.RetrieveExecutionStatusSummary(projectId, (activeReleasesOnly) ? (int?)TestCaseManager.RELEASE_ID_ACTIVE_RELEASES_ONLY : null);
					double testCount = 0;
                    foreach (TestCase_ExecutionStatusSummary executionSummaryRow in executionSummary)
					{
						testCount += executionSummaryRow.StatusCount.Value;
					}
					//Populate the equalizer with the percentages
					if (eqlExecutionStatus != null)
					{
						string tooltipText = "";
                        foreach (TestCase_ExecutionStatusSummary executionSummaryRow in executionSummary)
						{
                            int executionStatusId = executionSummaryRow.ExecutionStatusId;
                            string executionStatusName = executionSummaryRow.ExecutionStatusName;
							int statusCount = executionSummaryRow.StatusCount.Value;
							int percentage = (int)(((double)statusCount) / testCount * 100.0D);
							if (tooltipText != "")
							{
								tooltipText += ", ";
							}
							tooltipText += "# " + executionStatusName + " = " + statusCount.ToString();
							switch (executionStatusId)
							{
								case (int)TestCase.ExecutionStatusEnum.Passed:
									eqlExecutionStatus.PercentGreen = percentage;
									break;
                                case (int)TestCase.ExecutionStatusEnum.Failed:
									eqlExecutionStatus.PercentRed = percentage;
									break;
                                case (int)TestCase.ExecutionStatusEnum.Blocked:
									eqlExecutionStatus.PercentYellow = percentage;
									break;
                                case (int)TestCase.ExecutionStatusEnum.Caution:
									eqlExecutionStatus.PercentOrange = percentage;
									break;
                                case (int)TestCase.ExecutionStatusEnum.NotRun:
									eqlExecutionStatus.PercentGray = percentage;
									break;
                                case (int)TestCase.ExecutionStatusEnum.NotApplicable:
                                    eqlExecutionStatus.PercentDarkGray = percentage;
                                    break;
							}
						}
						//Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
						e.Row.Cells[4].Attributes["onMouseOver"] = "ddrivetip('" + GlobalFunctions.JSEncode(tooltipText) + "');";
						e.Row.Cells[4].Attributes["onMouseOut"] = "hideddrivetip();";
					}

					//Populate the test count cells
					TestRunManager testRunManager = new TestRunManager();
                    e.Row.Cells[3].Text = testRunManager.Count(projectId, null, 0, activeReleasesOnly).ToString();
					e.Row.Cells[2].Text = ((int)testCount).ToString();
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

			//Get the test execution status for all the projects in the group aggregated
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
				ProjectManager projectManager = new ProjectManager();
				Hashtable filters = new Hashtable();
				filters.Add("ProjectGroupId", projectGroupId);
                filters.Add("ActiveYn", "Y");
                List<ProjectView> projects = projectManager.Retrieve(filters, null);
                this.grdProjectExecutionStatus.DataSource = projects;
				this.grdProjectExecutionStatus.DataBind();
				this.grdProjectExecutionStatus.Visible = true;
			}
			else
			{
				this.grdProjectExecutionStatus.Visible = false;
			}
		}
	}
}
