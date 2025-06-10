using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;
using System.Drawing;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
	public partial class TaskList : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.TaskList::";

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
        /// Should we display tasks in 'completed' statuses (Default: FALSE)
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("TaskList_IncludeCompleted"),
        LocalizedWebDescription("TaskList_IncludeCompletedTooltip"),
        DefaultValue(false)
        ]
        public bool IncludeCompleted
        {
            get
            {
                return this.includeCompleted;
            }
            set
            {
                this.includeCompleted = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected bool includeCompleted = false;

        /// <summary>
        /// Should we display tasks in the 'deferred' status (Default: FALSE)
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("TaskList_IncludeDeferred"),
        LocalizedWebDescription("TaskList_IncludeDeferredTooltip"),
        DefaultValue(false)
        ]
        public bool IncludeDeferred
        {
            get
            {
                return this.includeDeferred;
            }
            set
            {
                this.includeDeferred = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected bool includeDeferred = false;

        /// <summary>
        /// Should we display tasks in the 'blocked' statuses (Default: TRUE)
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("TaskList_IncludeBlocked"),
        LocalizedWebDescription("TaskList_IncludeBlockedTooltip"),
        DefaultValue(true)
        ]
        public bool IncludeBlocked
        {
            get
            {
                return this.includeBlocked;
            }
            set
            {
                this.includeBlocked = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected bool includeBlocked = true;

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
                    string rssUrl = this.ResolveUrl("~/Feeds/" + UserId + "/" + UserRssToken + "/AssignedTasks.aspx");
                    this.Subtitle = GlobalFunctions.WEBPART_SUBTITLE_RSS + rssUrl;
                }
                
                //Register event handlers
				grdTasks.RowDataBound += new GridViewRowEventHandler(grdTasks_RowDataBound);

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
			//Get the current project filter (if any)
			Nullable<int> filterProjectId = null;
			if (GetUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false) && ProjectId > 0)
			{
				filterProjectId = ProjectId;
				//Display the release field
				this.grdTasks.Columns[2].Visible = false;
				this.grdTasks.Columns[3].Visible = true;
			}
			else
			{
				//Display the project field
				this.grdTasks.Columns[2].Visible = true;
				this.grdTasks.Columns[3].Visible = false;
			}

            //Set the base url for the Requirement URL field so that we get a HyperLink and not a LinkButton
            //The actual URL will be set during databinding
            ((NameDescriptionFieldEx)this.grdTasks.Columns[1]).NavigateUrlFormat = "{0}";


			//Now get the list of tasks owned by the user
			Business.TaskManager taskManager = new Business.TaskManager();
            List<TaskView> tasks = taskManager.RetrieveByOwnerId(UserId, filterProjectId, null, includeCompleted, this.rowsToDisplay, includeDeferred, includeBlocked);

            this.grdTasks.DataSource = tasks;
            this.grdTasks.DataBind();
		}

		/// <summary>
		/// Applies selective formatting to the task list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdTasks_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                TaskView taskView = (TaskView)(e.Row.DataItem);

				//Next lets handle the color of the priority column
                if (taskView.TaskPriorityId.HasValue)
				{
                    Color backColor = Color.FromName("#" + taskView.TaskPriorityColor);
                    e.Row.Cells[5].BackColor = backColor;
                }

				//If the end-date is in the past, change its css class to indicate this
				//(we only consider the date component not the time component)
                if (taskView.EndDate.HasValue)
				{
                    LabelEx lblEndDate = (LabelEx)e.Row.Cells[6].FindControl("lblEndDate");
                    lblEndDate.Text = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(taskView.EndDate.Value));
                    lblEndDate.ToolTip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(taskView.EndDate.Value));
                    if (taskView.EndDate.Value.Date < DateTime.UtcNow.Date)
					{
                        e.Row.Cells[6].CssClass = "Warning priority4";
					}
				}

                //If it's a pull request, change the icon
                if (taskView.IsPullRequest)
                {
                    ImageEx imgIcon = (ImageEx)e.Row.FindControl("imgIcon");
                    if (imgIcon != null)
                    {
                        imgIcon.ImageUrl = "Images/artifact-PullRequest.svg";
                    }
                }

				//Calculate the information to display in the progress column
				int percentGreen;
				int percentRed;
				int percentYellow;
				int percentGray;
                Task task = taskView.ConvertTo<TaskView, Task>();
                string tooltipText = TaskManager.CalculateProgress(task, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out percentGreen, out percentRed, out percentYellow, out percentGray);

				//Now populate the equalizer graph
				Equalizer eqlProgress = (Equalizer)e.Row.Cells[3].FindControl("eqlProgress");
				if (eqlProgress != null)
				{
					eqlProgress.PercentGreen = percentGreen;
					eqlProgress.PercentRed = percentRed;
					eqlProgress.PercentYellow = percentYellow;
					eqlProgress.PercentGray = percentGray;
				}

				//Add a persistent tooltip to the cell (can't use the tooltip property) for the raw data
				e.Row.Cells[4].Attributes["onMouseOver"] = "ddrivetip('" + tooltipText + "');";
				e.Row.Cells[4].Attributes["onMouseOut"] = "hideddrivetip();";

                //Need to set the actual URL of the HyperLink
                HyperLinkEx hyperlink = (HyperLinkEx)e.Row.Cells[1].Controls[0];
                if (hyperlink != null)
                {
                    hyperlink.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, taskView.ProjectId, taskView.TaskId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
                }
			}
		}
	}
}
