using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome
{
	/// <summary>
	/// Displays the task progress for the group and its constituent projects
	/// </summary>
	public partial class TaskProgress : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.TaskProgress::";

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
				this.grdProjectTaskProgress.RowDataBound += new GridViewRowEventHandler(grdProjectTaskProgress_RowDataBound);

                //Configure the project name link to have the right URL
                NameDescriptionFieldEx field = (NameDescriptionFieldEx)this.grdProjectTaskProgress.Columns[1];
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
		/// Gets the detailed task progress info for each project
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdProjectTaskProgress_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			const string METHOD_NAME = "grdProjectTaskProgress_RowDataBound";

			try
			{
				//Get the project id
				if (e.Row.RowType == DataControlRowType.DataRow)
				{
                    ProjectTaskProgressEntryView projectTaskProgressEntry = (ProjectTaskProgressEntryView)e.Row.DataItem;
                    int taskCount = projectTaskProgressEntry.TaskCount;

					//Now get the task progress for this project and populate the equalizer
					Equalizer eqlTaskProgress = (Equalizer)e.Row.Cells[5].FindControl("eqlTaskProgress");
					//Handle the no tasks case first
					if (taskCount == 0)
					{
                        e.Row.Cells[5].Text = GlobalFunctions.JSEncode(Business.ProjectGroupManager.GenerateTaskProgressTooltip(projectTaskProgressEntry));
						e.Row.Cells[5].CssClass = "NotCovered";
					}
					else
					{
						//Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
                        string tooltipText = GlobalFunctions.JSEncode(Business.ProjectGroupManager.GenerateTaskProgressTooltip(projectTaskProgressEntry));
						e.Row.Cells[5].Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
						e.Row.Cells[5].Attributes["onMouseOut"] = "hideddrivetip();";

						//Now populate the equalizer graph
						if (eqlTaskProgress != null)
						{
                            eqlTaskProgress.PercentGreen = (int)projectTaskProgressEntry.TaskPercentOnTime;
                            eqlTaskProgress.PercentRed = (int)projectTaskProgressEntry.TaskPercentLateFinish;
                            eqlTaskProgress.PercentYellow = (int)projectTaskProgressEntry.TaskPercentLateStart;
                            eqlTaskProgress.PercentGray = (int)projectTaskProgressEntry.TaskPercentNotStart;
						}

						//Finally if the actual effort is greater than the estimated effort, indicate with warning css class
						if (projectTaskProgressEntry.TaskActualEffort.HasValue && projectTaskProgressEntry.TaskEstimatedEffort.HasValue)
						{
							if (projectTaskProgressEntry.TaskActualEffort.Value > projectTaskProgressEntry.TaskEstimatedEffort.Value)
							{
								e.Row.Cells[4].CssClass = "Warning";
							}
						}
					}
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

			//Get the task progress for all the projects in the group aggregated as well as the individual projects
			TaskManager taskManager = new TaskManager();
            List<ProjectTaskProgressEntryView> projectResults = taskManager.RetrieveProgressByProject(projectGroupId, activeReleasesOnly);

			//Now bind the chart to the aggregate results
			if (this.DisplaySummaryGraph)
			{
				this.plcGroupSummaryGraph.Visible = true;
			}
			else
			{
                this.plcGroupSummaryGraph.Visible = false;
			}

			//Now bind the grid to the individual projects
			if (this.DisplayProjectDetail)
			{
                this.grdProjectTaskProgress.DataSource = projectResults;
				this.grdProjectTaskProgress.DataBind();
				this.grdProjectTaskProgress.Visible = true;
			}
			else
			{
				this.grdProjectTaskProgress.Visible = false;
			}
		}
	}
}