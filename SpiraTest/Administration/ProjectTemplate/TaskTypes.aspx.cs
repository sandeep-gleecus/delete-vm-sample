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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "TaskTypes_Title", "Template-Tasks/#types", "TaskTypes_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class TaskTypes : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.TaskTypes";

        //Bound data for the grid
        protected SortedList<string, string> flagList;
        protected List<TaskWorkflow> workflows;

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
                this.btnTaskTypesAdd.Click += new EventHandler(btnTaskTypesAdd_Click);
                this.btnTaskTypesUpdate.Click += new EventHandler(btnTaskTypesUpdate_Click);

                //Hide the 'is code review' column - not used currently
                this.grdEditTaskTypes.Columns[3].Visible = false;

			    //Only load the data once
                if (!IsPostBack)
                {
                    LoadTaskTypes();
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
        /// Handles the event raised when the task types ADD button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnTaskTypesAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnTaskTypesAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Update the types
                UpdateTypes();

                //Now we need to insert the new task type
                TaskManager taskManager = new TaskManager();
                taskManager.TaskType_Insert(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, null, false, true, false, false);

                //Now we need to reload the bound dataset for the next databind
                LoadTaskTypes();
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
        /// Handles the event raised when the task types UPDATE button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnTaskTypesUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnTaskTypesUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            try
            {
                //Update the types
                bool success = UpdateTypes();
                
                //Now we need to reload the bound dataset for the next databind
                if (success)
                {
                    LoadTaskTypes();

                    //Let the user know that the settings were saved
                    this.lblMessage.Text = Resources.Messages.TaskTypes_Success;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }
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
        /// Update the task types
        /// </summary>
        protected bool UpdateTypes()
        {
            //First we need to retrieve the existing list of task types
            TaskManager taskManager = new TaskManager();
            List<TaskType> taskTypes = taskManager.TaskType_Retrieve(this.ProjectTemplateId, false);

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditTaskTypes.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditTaskTypes.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtTaskTypeName = (TextBoxEx)grdEditTaskTypes.Rows[i].FindControl("txtTaskTypeName");
                    DropDownListEx ddlWorkflow = (DropDownListEx)grdEditTaskTypes.Rows[i].FindControl("ddlWorkflow");
                    RadioButtonEx radDefault = (RadioButtonEx)grdEditTaskTypes.Rows[i].FindControl("radDefault");
                    CheckBoxYnEx chkActiveYn = (CheckBoxYnEx)grdEditTaskTypes.Rows[i].FindControl("chkActiveYn");
                    CheckBoxYnEx chkCodeReview = (CheckBoxYnEx)grdEditTaskTypes.Rows[i].FindControl("chkCodeReview");
                    CheckBoxYnEx chkPullRequest = (CheckBoxYnEx)grdEditTaskTypes.Rows[i].FindControl("chkPullRequest");

                    //Need to make sure that the default item is an active one
                    if (radDefault.Checked && !(chkActiveYn.Checked))
                    {
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        this.lblMessage.Text = Resources.Messages.TaskTypes_CannotSetDefaultTypeInactive;
                        return false;
                    }

                    //Now get the task type id
                    int taskTypeId = Int32.Parse(txtTaskTypeName.MetaData);

                    //Find the matching row in the dataset
                    TaskType taskType = taskTypes.FirstOrDefault(t => t.TaskTypeId == taskTypeId);

                    //Make sure we found the matching row
                    if (taskType != null)
                    {
                        //Update the various fields
                        taskType.StartTracking();
                        taskType.Name = txtTaskTypeName.Text.Trim();
                        taskType.TaskWorkflowId = Int32.Parse(ddlWorkflow.SelectedValue);
                        taskType.IsDefault = radDefault.Checked;
                        taskType.IsActive = chkActiveYn.Checked;
                        taskType.IsCodeReview = chkCodeReview.Checked;
                        taskType.IsPullRequest = chkPullRequest.Checked;
                    }
                }
            }

            foreach (TaskType taskType in taskTypes)
            {
                taskManager.TaskType_Update(taskType);
            }
            return true;
        }

        /// <summary>
        /// Loads the task types configured for the current project
        /// </summary>
        protected void LoadTaskTypes()
        {
            const string METHOD_NAME = "LoadTaskTypes";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            TaskManager taskManager = new TaskManager();
            TaskWorkflowManager workflowManager = new TaskWorkflowManager();

            //Get the yes/no flag list
            this.flagList = taskManager.RetrieveFlagLookup();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of task types for this project
            List<TaskType> taskTypes = taskManager.TaskType_Retrieve(this.ProjectTemplateId, activeOnly);
            this.grdEditTaskTypes.DataSource = taskTypes;

            //Get the list of active workflows for this project (used as a lookup)
            this.workflows = workflowManager.Workflow_Retrieve(this.ProjectTemplateId, true);

            //Databind the grid
            this.grdEditTaskTypes.DataBind();

            //Populate any static fields
            this.lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Save the data and then reload
            UpdateTypes();
            LoadTaskTypes();
        }
    }
}