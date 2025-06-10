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
    /// Administration Edit Workflows page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_EditIncidentWorkflows_Title", "Template-Incidents/#incident-workflows", "Admin_EditIncidentWorkflows_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class IncidentWorkflows : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentWorkflows";

        protected Dictionary<string, string> flagList;

        /// <summary>
        /// Called when the page is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Redirect if there's no project template selected.
            if (ProjectTemplateId < 1)
                Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);

            try
            {
                //Display the project template name
                this.ltrTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(this.ProjectTemplateName);
                this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

                //Load the page if not postback
                if (!Page.IsPostBack)
                {
                    LoadAndBindData();
                }

                //Register event handlers
                this.btnCancel.Click += new EventHandler(btnCancel_Click);
                this.btnSave.Click += new EventHandler(btnSave_Click);
                this.grdEditWorkflows.RowCommand += new GridViewCommandEventHandler(grdEditWorkflows_RowCommand);

                //Set the default button
                this.Form.DefaultButton = this.btnSave.UniqueID;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Called when a workflow link is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdEditWorkflows_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdEditWorkflows_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to get the workflow id command
                //Copy the workflow
                if (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "workflowcopy")
                {
                    //Copy the workflow
                    int workflowId = Int32.Parse(e.CommandArgument.ToString());
                    new WorkflowManager().Workflow_Copy(workflowId);

                    //Now we need to reload the bound dataset for the next databind
                    LoadAndBindData();

                    //Display success
                    this.lblMessage.Text = Resources.Messages.Admin_Workflows_Default_CopySuccess;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }

                if (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "workflowadd")
                {
                    //Now we need to insert the new workflow                
                    new WorkflowManager().Workflow_Insert(ProjectTemplateId, Resources.Main.Admin_Workflows_NewWorkflowName, false, true);

                    //Now we need to reload the bound data for the next databind
                    LoadAndBindData();
                }


                //Delete the workflow
                if (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "workflowdelete")
                {
                    //Delete the workflow
                    int workflowId = Int32.Parse(e.CommandArgument.ToString());
                    try
                    {
                        new WorkflowManager().Workflow_Delete(workflowId);
                    }
                    catch (WorkflowInUseException exception)
                    {
                        this.lblMessage.Text = exception.Message;
                    }

                    //Now we need to reload the bound dataset for the next databind
                    LoadAndBindData();

                    //Display success
                    this.lblMessage.Text = Resources.Messages.Admin_Workflows_Default_DeleteSuccess;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }

                //View/edit the workflow steps and transitions
                if (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "workflowsteps")
                {
                    //Get the workflow ID and redirect to the appropriate page
                    int workflowId = Int32.Parse(e.CommandArgument.ToString());
                    Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "IncidentWorkflowDetails") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString());
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                //Ignore this error as it's due to response.redirect ending the thread
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
        /// Called when the save button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSave_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnSave_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            try
            {
                //First we need to retrieve the existing list of workflows
                WorkflowManager workflowManager = new Business.WorkflowManager();
                List<Workflow> workflows = workflowManager.Workflow_Retrieve(ProjectTemplateId, false);

                //We need to make sure that at least one workflow is active
                int activeCount = 0;

                //Now iterate through the rows and get the id and various control values
                foreach (GridViewRow gvr in this.grdEditWorkflows.Rows)
                {
                    //We only look at item rows (i.e. not headers and footers)
                    if (gvr.RowType == DataControlRowType.DataRow)
                    {
                        //Extract the various controls from the datagrid
                        TextBoxEx txtName = (TextBoxEx)gvr.FindControl("txtName");
                        RadioButtonEx radWorkflowDefault = (RadioButtonEx)gvr.FindControl("radWorkflowDefault");
                        CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)gvr.FindControl("chkActiveFlag");
                        CheckBoxEx chkNotify = (CheckBoxEx)gvr.FindControl("chkNotify");

                        //Now get the workflow id
                        int workflowId = Int32.Parse(txtName.MetaData);

                        //Find the matching row in the dataset
                        Workflow workflow = workflows.Find(w => w.WorkflowId == workflowId);

                        //Increment the active count if appropriate
                        if (chkActiveFlag.Checked)
                        {
                            activeCount++;
                        }

                        //Make sure we found the matching row
                        if (workflow != null)
                        {
                            //Update the various fields
                            workflow.StartTracking();
                            workflow.Name = txtName.Text;
                            workflow.IsDefault = radWorkflowDefault.Checked;
                            workflow.IsActive = chkActiveFlag.Checked;
                            workflow.IsNotify = chkNotify.Checked;
                        }
                    }
                }

                //Make sure that at least one workflow is active
                if (activeCount == 0)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_Workflows_Default_OneNeedsToBeActive;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }

                //Persist the changes, handling certain defined business exceptions
                try
                {
                    foreach (Workflow workflow in workflows)
                    {
                        workflowManager.Workflow_Update(workflow);
                    }
                }
                catch (WorkflowInUseException exception)
                {
                    this.lblMessage.Text = exception.Message;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }
                catch (WorkflowMustBeActiveException exception)
                {
                    this.lblMessage.Text = exception.Message;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }

                //Now we need to reload the bound dataset for the next databind
                LoadAndBindData();

                //Display a confirmation
                this.lblMessage.Text = Resources.Messages.Admin_Workflows_Default_SaveConfirm;
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
        /// Called when the Cancel button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            //Return to the project template home page
            Response.Redirect(UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default"), true);
        }

        /// <summary>
        /// Loads and displays the panel's contents when called
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            WorkflowManager workflowManager = new WorkflowManager();

            //Get the yes/no flag list
            this.flagList = workflowManager.RetrieveFlagLookupDictionary();

            //Get the list of workflows
            List<Workflow> workflows = workflowManager.Workflow_Retrieve(ProjectTemplateId, false);
            this.grdEditWorkflows.DataSource = workflows;

            //Databind the grid
            this.grdEditWorkflows.DataBind();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}