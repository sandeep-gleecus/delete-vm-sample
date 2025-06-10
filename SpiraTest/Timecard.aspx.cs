using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.DataModel;
using System.Data;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// User Profile Edit Page and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.MyTimecard, "SiteMap_MyTimecard", "User-Product-Management/#my-timecard")]
    public partial class Timecard : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Timecard::";

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Register event handlers
            //Need to check during row data binding if the project is enabled for time-tracking
            this.grdIncidents.RowDataBound += new GridViewRowEventHandler(grdIncidents_RowDataBound);
            this.grdTasks.RowDataBound += new GridViewRowEventHandler(grdTasks_RowDataBound);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
            this.btnSubmit.Click += new EventHandler(btnSubmit_Click);

            //Specify the base URLs for the two grids
            ((NameDescriptionFieldEx)this.grdTasks.Columns[1]).NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -3);
            ((NameDescriptionFieldEx)this.grdIncidents.Columns[1]).NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -3);

            //Only load the data once
            if (!IsPostBack)
            {
                //Display the full name of the current user
                this.lblTimecardFullName.Text = UserFullName;

                //Load and bind the datagrids
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the datagrids
        /// </summary>
        protected void LoadAndBindData()
        {
            //Reset the message
            this.lblMessage.Text = "";

            //Load the list of open tasks for the current user - all projects
            TaskManager taskManager = new TaskManager();
            List<TaskView> tasks = taskManager.RetrieveByOwnerId(UserId, null, null, false);

            //Databind the tasks grid
            this.grdTasks.DataSource = tasks;
            this.grdTasks.DataBind();

            //Load the list of open incidents for the current user - all projects
            IncidentManager incidentManager = new IncidentManager();
            List<IncidentView> incidents = incidentManager.RetrieveOpenByOwnerId(UserId, null, null);

            //Databind the incident grid
            this.grdIncidents.DataSource = incidents;
            this.grdIncidents.DataBind();
        }

        /// <summary>
        /// Handles clicks on the Submit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSubmit_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnSubmit_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure that there were no validation errors
            if (!IsValid)
            {
                return;
            }

            //We need to update the Actual and Remaining Effort values

            //Load the list of open tasks for the current user - all projects
            TaskManager taskManager = new TaskManager();
            List<TaskView> taskViews = taskManager.RetrieveByOwnerId(UserId, null, null, false);

            //First iterate through the Tasks
            List<Task> tasks = new List<Task>();
            foreach (GridViewRow gvr in this.grdTasks.Rows)
            {
                //We only want data-rows
                if (gvr.RowType == DataControlRowType.DataRow && gvr.Visible)
                {
                    //Get the primary key for the row-index
                    int taskId = (int)this.grdTasks.DataKeys[gvr.RowIndex].Value;

                    //Get the corresponding row in the list
                    TaskView taskView = taskViews.FirstOrDefault(t => t.TaskId == taskId);
                    if (taskView != null)
                    {
                        //convert to the updatable entity
                        bool changeMade = false;
                        Task task = taskView.ConvertTo<TaskView, Task>();
                        task.StartTracking();

                        //Now get the additional effort and remaining effort

                        //Additional Effort
                        TextBoxEx txtAdditionalEffort = (TextBoxEx)gvr.Cells[7].FindControl("txtAdditionalEffort");
                        if (txtAdditionalEffort != null)
                        {
                            decimal effort;
                            if (Decimal.TryParse(txtAdditionalEffort.Text, out effort))
                            {
                                int effortMins = (int)(effort * (decimal)60);

                                //Add or subtract this from the current effort unless it's null
                                if (task.ActualEffort.HasValue)
                                {
                                    task.ActualEffort += effortMins;
                                }
                                else
                                {
                                    task.ActualEffort = effortMins;
                                }

                                if (task.ActualEffort.Value < 0)
                                {
                                    //The actual effort cannot be negative
                                    task.ActualEffort = 0;
                                }
                                changeMade = true;
                            }
                        }

                        //Remaining Effort
                        TextBoxEx txtRemainingEffort = (TextBoxEx)gvr.Cells[8].FindControl("txtRemainingEffort");
                        if (txtRemainingEffort != null)
                        {
                            //Handle the empty case (set to zero unless already blank, in which case leave)
                            if (String.IsNullOrWhiteSpace(txtRemainingEffort.Text))
                            {
                                if (task.RemainingEffort.HasValue && task.RemainingEffort > 0)
                                {
                                    task.RemainingEffort = 0;
                                    changeMade = true;
                                }
                            }
                            else
                            {
                                decimal effort;
                                if (Decimal.TryParse(txtRemainingEffort.Text, out effort))
                                {
                                    int effortMins = (int)(effort * (decimal)60);

                                    //Update the remaining effort
                                    if (!task.RemainingEffort.HasValue || task.RemainingEffort != effortMins)
                                    {
                                        task.RemainingEffort = effortMins;
                                        changeMade = true;
                                    }
                                }
                            }
                        }

                        if (changeMade)
                        {
                            tasks.Add(task);
                        }
                    }
                }
            }

            //Update the tasks
            try
            {
                foreach (Task task in tasks)
                {
                    taskManager.Update(task, UserId);
                }
            }
            catch (DataValidationException exception)
            {
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }
            catch (OptimisticConcurrencyException)
            {
                this.lblMessage.Text = Resources.Messages.Global_DataChangedBySomeoneElse;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Load the list of open incidents for the current user - all projects
            IncidentManager incidentManager = new IncidentManager();
            List<IncidentView> incidentViews = incidentManager.RetrieveOpenByOwnerId(UserId, null, null);

            //Now iterate through the Incidents
            List<Incident> incidents = new List<Incident>();
            foreach (GridViewRow gvr in this.grdIncidents.Rows)
            {
                //We only want data-rows
                if (gvr.RowType == DataControlRowType.DataRow && gvr.Visible)
                {
                    //Get the primary key for the row-index
                    int incidentId = (int)this.grdIncidents.DataKeys[gvr.RowIndex].Value;

                    //Get the corresponding row in the list
                    IncidentView incidentView = incidentViews.FirstOrDefault(i => i.IncidentId == incidentId);
                    if (incidentView != null)
                    {
                        //convert to the updatable entity
                        bool changeMade = false;
                        Incident incident = incidentView.ConvertTo<IncidentView, Incident>();
                        incident.StartTracking();

                        //Now get the additional effort and remaining effort

                        //Additional Effort
                        TextBoxEx txtAdditionalEffort = (TextBoxEx)gvr.Cells[7].FindControl("txtAdditionalEffort");
                        if (txtAdditionalEffort != null)
                        {
                            decimal effort;
                            if (Decimal.TryParse(txtAdditionalEffort.Text, out effort))
                            {
                                int effortMins = (int)(effort * (decimal)60);

                                //Add or subtract this from the current effort unless it's null
                                if (incident.ActualEffort.HasValue)
                                {
                                    incident.ActualEffort += effortMins;
                                }
                                else
                                {
                                    incident.ActualEffort = effortMins;
                                }
                                if (incident.ActualEffort < 0)
                                {
                                    //The actual effort cannot be negative
                                    incident.ActualEffort = 0;
                                }
                                changeMade = true;
                            }
                        }

                        //Remaining Effort
                        TextBoxEx txtRemainingEffort = (TextBoxEx)gvr.Cells[8].FindControl("txtRemainingEffort");
                        if (txtRemainingEffort != null)
                        {
                            //Handle the empty case (set to zero unless already blank, in which case leave)
                            if (String.IsNullOrWhiteSpace(txtRemainingEffort.Text))
                            {
                                if (incident.RemainingEffort.HasValue && incident.RemainingEffort > 0)
                                {
                                    incident.RemainingEffort = 0;
                                    changeMade = true;
                                }
                            }
                            else
                            {
                                decimal effort;
                                if (Decimal.TryParse(txtRemainingEffort.Text, out effort))
                                {
                                    int effortMins = (int)(effort * (decimal)60);

                                    //Update the remaining effort
                                    if (!incident.RemainingEffort.HasValue || incident.RemainingEffort != effortMins)
                                    {
                                        changeMade = true;
                                        incident.RemainingEffort = effortMins;
                                    }
                                }
                            }
                        }

                        if (changeMade)
                        {
                            incidents.Add(incident);
                        }
                    }
                }
            }

            //Update the incident incidents
            try
            {
                foreach (Incident incident in incidents)
                {
                    incidentManager.Update(incident, UserId);
                }
            }
            catch (DataValidationException exception)
            {
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }
            catch (OptimisticConcurrencyException)
            {
                this.lblMessage.Text = Resources.Messages.Global_DataChangedBySomeoneElse;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Reload the data in the grid
            LoadAndBindData();

            //Display a confirmation message
            this.lblMessage.Text = Resources.Dialogs.Timecard_SubmitSuccess;
            this.lblMessage.Type = MessageBox.MessageType.Information;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Handles clicks on the Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCancel_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Simply redirect back to the My Page
            Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId), true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// This event handler applies any conditional formatting to the datagrid before display
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The parameters passed to handler</param>
        private void grdIncidents_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Don't touch headers, footers or subheaders
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get the data item
                IncidentView incidentView = (IncidentView)(e.Row.DataItem);

                //Make sure time tracking is enabled for this project
                ProjectManager projectManager = new ProjectManager();
                Project project = projectManager.RetrieveById(incidentView.ProjectId);
                if (!project.IsTimeTrackIncidents)
                {
                    e.Row.Visible = false;
                }

                //First lets handle the color of the priority column
                if (incidentView.PriorityId.HasValue)
                {
                    Color backColor = Color.FromName("#" + incidentView.PriorityColor);
                    e.Row.Cells[2].BackColor = backColor;
                }

                //Next the severity column
                if (incidentView.SeverityId.HasValue)
                {
                    Color backColor = Color.FromName("#" + incidentView.SeverityColor);
                    e.Row.Cells[3].BackColor = backColor;
                }

                //If the start-date is in the past, change its css class to indicate this
                //(we only consider the date component not the time component)
                if (incidentView.StartDate.HasValue)
                {
                    Literal literal = (Literal)e.Row.Cells[4].FindControl("ltrStartDate");
                    literal.Text = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(incidentView.StartDate.Value));
                    e.Row.Cells[4].ToolTip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(incidentView.StartDate.Value));
                    if (incidentView.StartDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        e.Row.Cells[4].CssClass = "Warning priority4";
                    }
                }
            }
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

                //See if task timecards are enabled for this project
                ProjectManager projectManager = new ProjectManager();
                Project project = projectManager.RetrieveById(taskView.ProjectId);
                if (!project.IsTimeTrackTasks)
                {
                    e.Row.Visible = false;
                }

                //Now lets handle the color of the priority column
                if (taskView.TaskPriorityId.HasValue)
                {
                    Color backColor = Color.FromName("#" + taskView.TaskPriorityColor);
                    e.Row.Cells[2].BackColor = backColor;
                }
                e.Row.Cells[2].CssClass = "priority4";

                //If the start-date is in the past, change its css class to indicate this
                //(we only consider the date component not the time component)
                if (taskView.StartDate.HasValue)
                {
                    Literal literal = (Literal)e.Row.Cells[3].FindControl("ltrStartDate");
                    literal.Text = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(taskView.StartDate.Value));
                    e.Row.Cells[3].ToolTip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(taskView.StartDate.Value));
                    if (taskView.StartDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        e.Row.Cells[3].CssClass = "Warning priority4";
                    }
                }

                //If the end-date is in the past, change its css class to indicate this
                //(we only consider the date component not the time component)
                if (taskView.EndDate.HasValue)
                {
                    Literal literal = (Literal)e.Row.Cells[4].FindControl("ltrEndDate");
                    literal.Text = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(taskView.EndDate.Value));
                    e.Row.Cells[4].ToolTip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(taskView.EndDate.Value));
                    if (taskView.EndDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        e.Row.Cells[4].CssClass = "Warning priority2";
                    }
                }
            }
        }
    }
}