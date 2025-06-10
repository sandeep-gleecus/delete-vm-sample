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
    /// Administration Edit Risks Workflows page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_EditRiskWorkflows_Title", "Template-Risks/#risk-workflows", "Admin_EditRiskWorkflows_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RiskWorkflows : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskWorkflows";

        //Bound data for the grid
        protected SortedList<string, string> flagList;
        protected List<RiskWorkflow> workflows;

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
                //Display the project name
                this.ltrTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(this.ProjectTemplateName);
                this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

                //Register event handlers
                this.btnCancel.Click += new EventHandler(btnCancel_Click);
                this.btnSave.Click += new EventHandler(btnSave_Click);
                this.grdEditWorkflows.RowCommand += new GridViewCommandEventHandler(grdEditWorkflows_RowCommand);

                //Load the page if not postback
                if (!Page.IsPostBack)
                {
                    LoadAndBindData();
                }

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
                //Add a workflow
                if (e.CommandName.ToLowerInvariant() == "workflowadd")
                {
                    //Now we need to insert the new workflow                
                    new RiskWorkflowManager().Workflow_Insert(ProjectTemplateId, Resources.Main.Admin_Workflows_NewWorkflowName, false);

                    //Now we need to reload the bound data for the next databind
                    LoadAndBindData();
                }

                //Copy the workflow
                if (e.CommandName.ToLowerInvariant() == "workflowcopy")
                {
                    //Copy the workflow
                    int workflowId = Int32.Parse(e.CommandArgument.ToString());
                    new RiskWorkflowManager().Workflow_Copy(workflowId);

                    //Now we need to reload the bound dataset for the next databind
                    LoadAndBindData();

                    //Display success
                    this.lblMessage.Text = Resources.Messages.Admin_Workflows_Default_CopySuccess;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }

                //Delete the workflow
                if (e.CommandName.ToLowerInvariant() == "workflowdelete")
                {
                    //Delete the workflow
                    int workflowId = Int32.Parse(e.CommandArgument.ToString());
                    try
                    {
                        new RiskWorkflowManager().Workflow_Delete(workflowId);

                        //Now we need to reload the bound dataset for the next databind
                        LoadAndBindData();

                        //Display success
                        this.lblMessage.Text = Resources.Messages.Admin_Workflows_Default_DeleteSuccess;
                        this.lblMessage.Type = MessageBox.MessageType.Information;
                    }
                    catch (WorkflowInUseException exception)
                    {
                        this.lblMessage.Text = exception.Message;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                    }
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
        /// Loads the risk types and risk workflows configured for the current project
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the business objects
                RiskManager riskManager = new Business.RiskManager();
                RiskWorkflowManager riskWorkflowManager = new RiskWorkflowManager();

                //Get the yes/no flag list
                this.flagList = riskWorkflowManager.RetrieveFlagLookup();

                //Get the list of active workflows for this project (used as a lookup)
                this.workflows = riskWorkflowManager.Workflow_Retrieve(this.ProjectTemplateId, true);

                //Now get the list of all workflows and bind the workflow grid to that
                List<RiskWorkflow> riskWorkflows = riskWorkflowManager.Workflow_Retrieve(ProjectTemplateId, false);
                this.grdEditWorkflows.DataSource = riskWorkflows;

                //Databind the grid
                this.grdEditWorkflows.DataBind();

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
                //Next we need to retrieve the existing list of workflows and update them
                RiskWorkflowManager riskWorkflowManager = new RiskWorkflowManager();
                List<RiskWorkflow> riskWorkflows = riskWorkflowManager.Workflow_Retrieve(ProjectTemplateId, false);

                //We need to make sure that at least one workflow is active
                int activeCount = 0;

                //Now iterate through the rows and get the id and various control values
                for (int i = 0; i < this.grdEditWorkflows.Rows.Count; i++)
                {
                    //We only look at data rows (i.e. not headers and footers)
                    GridViewRow gvr = this.grdEditWorkflows.Rows[i];
                    if (gvr.RowType == DataControlRowType.DataRow)
                    {
                        //Extract the various controls from the datagrid
                        TextBoxEx txtName = (TextBoxEx)gvr.FindControl("txtName");
                        RadioButtonEx radWorkflowDefault = (RadioButtonEx)gvr.FindControl("radWorkflowDefault");
                        CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)gvr.FindControl("chkActiveFlag");

                        //Now get the workflow id
                        int workflowId = (int)this.grdEditWorkflows.DataKeys[i].Value;

                        //Find the matching row in the dataset
                        RiskWorkflow riskWorkflow = riskWorkflows.Find(w => w.RiskWorkflowId == workflowId);

                        //Increment the active count if appropriate
                        if (chkActiveFlag.Checked)
                        {
                            activeCount++;
                        }

                        //Make sure we found the matching row
                        if (riskWorkflow != null)
                        {
                            //Update the various fields
                            riskWorkflow.StartTracking();
                            riskWorkflow.Name = txtName.Text;
                            riskWorkflow.IsDefault = radWorkflowDefault.Checked;
                            riskWorkflow.IsActive = chkActiveFlag.Checked;
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
                    foreach (RiskWorkflow riskWorkflow in riskWorkflows)
                    {
                        riskWorkflowManager.Workflow_Update(riskWorkflow);
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
    }
}