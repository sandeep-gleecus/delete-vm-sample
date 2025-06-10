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
    /// Administration Edit Incident Types Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "IncidentTypes_Title", "Template-Incidents/#edit-types", "IncidentTypes_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class IncidentTypes : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentTypes";

        //Bound data for the grid
        protected SortedList<string, string> flagList;
        protected List<Workflow> workflows;

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
                this.btnIncidentTypesAdd.Click += new EventHandler(btnIncidentTypesAdd_Click);
                this.btnIncidentTypesUpdate.Click += new EventHandler(btnIncidentTypesUpdate_Click);

			    //Only load the data once
                if (!IsPostBack)
                {
                    LoadIncidentTypes();
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
        /// Handles the event raised when the incident types ADD button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnIncidentTypesAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnIncidentTypesAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Update the types
                UpdateTypes();

                //Now we need to insert the new incident type
                IncidentManager incidentManager = new IncidentManager();
                incidentManager.InsertIncidentType(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, null, false, false, false, true);

                //Now we need to reload the bound dataset for the next databind
                LoadIncidentTypes();
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
        /// Handles the event raised when the incident types UPDATE button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnIncidentTypesUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnIncidentTypesUpdate_Click";

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
                    LoadIncidentTypes();

                    //Let the user know that the settings were saved
                    this.lblMessage.Text = Resources.Messages.IncidentTypes_Success;
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
        /// Update the incident types
        /// </summary>
        protected bool UpdateTypes()
        {
            //First we need to retrieve the existing list of incident types
            IncidentManager incidentManager = new IncidentManager();
            List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(this.ProjectTemplateId, false);

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditIncidentTypes.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditIncidentTypes.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditIncidentTypes.Rows[i].Cells[1].Controls[1];
                    DropDownListEx ddlWorkflow = (DropDownListEx)grdEditIncidentTypes.Rows[i].Cells[2].Controls[1];
                    CheckBoxEx chkIssue = (CheckBoxEx)grdEditIncidentTypes.Rows[i].Cells[3].Controls[1];
                    RadioButtonEx radDefault = (RadioButtonEx)grdEditIncidentTypes.Rows[i].Cells[4].Controls[1];
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditIncidentTypes.Rows[i].Cells[5].Controls[1];

                    //Need to make sure that the default item is an active one
                    if (radDefault.Checked && !(chkActiveFlag.Checked))
                    {
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        this.lblMessage.Text = Resources.Messages.IncidentTypes_CannotSetDefaultTypeInactive;
                        return false;
                    }

                    //Now get the incident type id
                    int incidentTypeId = Int32.Parse(txtDisplayName.MetaData);

                    //Find the matching row in the dataset
                    IncidentType incidentType = incidentTypes.FirstOrDefault(t => t.IncidentTypeId == incidentTypeId);

                    //Make sure we found the matching row
                    if (incidentType != null)
                    {
                        //Update the various fields
                        incidentType.StartTracking();
                        incidentType.Name = txtDisplayName.Text.Trim();
                        incidentType.WorkflowId = Int32.Parse(ddlWorkflow.SelectedValue);
                        incidentType.IsIssue = chkIssue.Checked;
                        incidentType.IsRisk = false;    //Not used any more, replaced by Risk artifact
                        incidentType.IsDefault = radDefault.Checked;
                        incidentType.IsActive = chkActiveFlag.Checked;
                    }
                }
            }

            foreach (IncidentType incidentType in incidentTypes)
            {
                incidentManager.IncidentType_Update(incidentType);
            }
            return true;
        }

        /// <summary>
        /// Loads the incident types configured for the current project
        /// </summary>
        protected void LoadIncidentTypes()
        {
            const string METHOD_NAME = "LoadIncidentTypes";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            IncidentManager incidentManager = new IncidentManager();
            WorkflowManager workflowManager = new WorkflowManager();

            //Get the yes/no flag list
            this.flagList = incidentManager.RetrieveFlagLookup();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of incident types for this project
            List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(this.ProjectTemplateId, activeOnly);
            this.grdEditIncidentTypes.DataSource = incidentTypes;

            //Get the list of active workflows for this project (used as a lookup)
            this.workflows = workflowManager.Workflow_Retrieve(this.ProjectTemplateId, true);

            //Databind the grid
            this.grdEditIncidentTypes.DataBind();

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
            LoadIncidentTypes();
        }
    }
}