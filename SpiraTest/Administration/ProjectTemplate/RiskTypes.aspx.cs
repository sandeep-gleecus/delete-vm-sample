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
    /// Administration Edit Risk Types Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "RiskTypes_Title", "Template-Risks/#edit-types", "RiskTypes_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RiskTypes : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskTypes";

        //Bound data for the grid
        protected List<RiskWorkflow> workflows;

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
                this.btnRiskTypesAdd.Click += new EventHandler(btnRiskTypesAdd_Click);
                this.btnRiskTypesUpdate.Click += new EventHandler(btnRiskTypesUpdate_Click);

			    //Only load the data once
                if (!IsPostBack)
                {
                    LoadRiskTypes();
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
        /// Handles the event raised when the risk types ADD button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnRiskTypesAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRiskTypesAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Update the types
                UpdateTypes();

                //Now we need to insert the new risk type
                RiskManager riskManager = new RiskManager();
                riskManager.RiskType_Insert(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, null, false, true);

                //Now we need to reload the bound dataset for the next databind
                LoadRiskTypes();
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
        /// Handles the event raised when the risk types UPDATE button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnRiskTypesUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRiskTypesUpdate_Click";

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
                    LoadRiskTypes();

                    //Let the user know that the settings were saved
                    this.lblMessage.Text = Resources.Messages.RiskTypes_Success;
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
        /// Update the risk types
        /// </summary>
        protected bool UpdateTypes()
        {
            //First we need to retrieve the existing list of risk types
            RiskManager riskManager = new RiskManager();
            List<RiskType> riskTypes = riskManager.RiskType_Retrieve(this.ProjectTemplateId, false);

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditRiskTypes.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditRiskTypes.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditRiskTypes.Rows[i].FindControl("txtRiskTypeName");
                    DropDownListEx ddlWorkflow = (DropDownListEx)grdEditRiskTypes.Rows[i].FindControl("ddlWorkflow");
                    RadioButtonEx radDefault = (RadioButtonEx)grdEditRiskTypes.Rows[i].FindControl("radDefault");
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditRiskTypes.Rows[i].FindControl("chkActiveYn");

                    //Need to make sure that the default item is an active one
                    if (radDefault.Checked && !(chkActiveFlag.Checked))
                    {
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        this.lblMessage.Text = Resources.Messages.RiskTypes_CannotSetDefaultTypeInactive;
                        return false;
                    }

                    //Now get the risk type id
                    int riskTypeId = Int32.Parse(txtDisplayName.MetaData);

                    //Find the matching row in the dataset
                    RiskType riskType = riskTypes.FirstOrDefault(t => t.RiskTypeId == riskTypeId);

                    //Make sure we found the matching row
                    if (riskType != null)
                    {
                        //Update the various fields
                        riskType.StartTracking();
                        riskType.Name = txtDisplayName.Text.Trim();
                        riskType.RiskWorkflowId = Int32.Parse(ddlWorkflow.SelectedValue);
                        riskType.IsDefault = radDefault.Checked;
                        riskType.IsActive = chkActiveFlag.Checked;
                    }
                }
            }

            foreach (RiskType riskType in riskTypes)
            {
                riskManager.RiskType_Update(riskType);
            }
            return true;
        }

        /// <summary>
        /// Loads the risk types configured for the current project
        /// </summary>
        protected void LoadRiskTypes()
        {
            const string METHOD_NAME = "LoadRiskTypes";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            RiskManager riskManager = new RiskManager();
            RiskWorkflowManager workflowManager = new RiskWorkflowManager();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of risk types for this project
            List<RiskType> riskTypes = riskManager.RiskType_Retrieve(this.ProjectTemplateId, activeOnly);
            this.grdEditRiskTypes.DataSource = riskTypes;

            //Get the list of active workflows for this project (used as a lookup)
            this.workflows = workflowManager.Workflow_Retrieve(this.ProjectTemplateId, true);

            //Databind the grid
            this.grdEditRiskTypes.DataBind();

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
            LoadRiskTypes();
        }
    }
}