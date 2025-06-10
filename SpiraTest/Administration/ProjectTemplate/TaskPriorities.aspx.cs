using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Edit Task Types Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "TaskPriorities_Title", "Template-Tasks/#priority", "TaskPriorities_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class TaskPriorities : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.TaskPriorities";

        //Bound data for the grid
        protected SortedList<string, string> flagList;

        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Redirect if there's no project template selected.
                if (ProjectTemplateId < 1)
                    Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);

                //Add event handlers
                this.ddlFilterType.SelectedIndexChanged += new EventHandler(ddlFilterType_SelectedIndexChanged);
                this.btnAdd.Click += new EventHandler(btnAdd_Click);
                this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadTaskPriorities();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Loads the task priorities configured for the current project template
        /// </summary>
        protected void LoadTaskPriorities()
        {
            const string METHOD_NAME = "LoadTaskPriorities";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            TaskManager taskManager = new TaskManager();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of task priorities for this project
            List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(this.ProjectTemplateId, activeOnly);

            //Databind the grid
            this.grdEditTaskPriorities.DataSource = priorities;
            this.grdEditTaskPriorities.DataBind();

            //Populate any static fields
            this.lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

		/// <summary>
		/// Handles the event raised when the task priority ADD button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnAdd_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try          
            {
                //First update the existing priorities
                UpdatePriorities();

				//Now we need to insert the new task priority (default to white)
				TaskManager taskManager = new TaskManager();
				taskManager.TaskPriority_Insert(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, "ffffff", true, 0);

				//Now we need to reload the bound dataset for the next databind
				LoadTaskPriorities();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the task priority UPDATE button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try
			{
                //Update the priorities
                UpdatePriorities();

                //Now we need to reload the bound dataset for the next databind
                LoadTaskPriorities();

                //Let the user know that the settings were saved
                this.lblMessage.Text = Resources.Messages.Admin_Priorities_Success;
                this.lblMessage.Type = MessageBox.MessageType.Information;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Updates the priorities
        /// </summary>
        protected void UpdatePriorities()
        {
            //First we need to retrieve the existing list of task priorities
            TaskManager taskManager = new TaskManager();
            List<TaskPriority> taskPriorities = taskManager.TaskPriority_Retrieve(this.ProjectTemplateId, false);

            //We need to make sure that at least one priority is active
            int activeCount = 0;

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditTaskPriorities.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditTaskPriorities.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditTaskPriorities.Rows[i].FindControl("txtTaskPriorityName");
                    TextBoxEx txtScore = (TextBoxEx)grdEditTaskPriorities.Rows[i].FindControl("txtScore");
                    ColorPicker colColor = (ColorPicker)grdEditTaskPriorities.Rows[i].FindControl("colTaskPriorityColor");
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditTaskPriorities.Rows[i].FindControl("ddlActive");

                    //Now get the task priority id
                    int priorityId = Int32.Parse(txtDisplayName.MetaData);
                    int score = 0;
                    Int32.TryParse(txtScore.Text, out score);

                    //Find the matching row in the dataset
                    TaskPriority taskPriority = taskPriorities.FirstOrDefault(p => p.TaskPriorityId == priorityId);

                    //Increment the active count if appropriate
                    if (chkActiveFlag.Checked)
                    {
                        activeCount++;
                    }

                    //Make sure we found the matching row
                    if (taskPriority != null)
                    {
                        //Update the various fields
                        taskPriority.StartTracking();
                        taskPriority.Name = txtDisplayName.Text;
                        taskPriority.Color = colColor.Text;
                        taskPriority.IsActive = chkActiveFlag.Checked;
                        taskPriority.Score = score;
                    }
                }
            }

            //Make sure that at least one priority is active
            if (activeCount == 0)
            {
                this.lblMessage.Text = Resources.Messages.Admin_Priorities_AtLeastOneMustBeActive;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Make the updates
            for (int i = 0; i < taskPriorities.Count; i++)
            {
                taskManager.TaskPriority_Update(taskPriorities[i]);
            }
        }

        /// <summary>
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Save the data and then reload
            UpdatePriorities();
            LoadTaskPriorities();
        }
    }
}