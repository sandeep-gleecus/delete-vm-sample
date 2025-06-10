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
    /// Administration Edit Requirement Types Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "RequirementTypes_Title", "Template-Requirements/#types", "RequirementTypes_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RequirementTypes : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RequirementTypes";

        //Bound data for the grid
        protected SortedList<string, string> flagList;
        protected List<RequirementWorkflow> workflows;

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
                this.btnRequirementTypesAdd.Click += new EventHandler(btnRequirementTypesAdd_Click);
                this.btnRequirementTypesUpdate.Click += new EventHandler(btnRequirementTypesUpdate_Click);

			    //Only load the data once
                if (!IsPostBack)
                {
                    LoadRequirementTypes();
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
        /// Handles the event raised when the requirement types ADD button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnRequirementTypesAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRequirementTypesAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Update the types
                UpdateTypes();

                //Now we need to insert the new requirement type
                RequirementManager requirementManager = new RequirementManager();
                requirementManager.RequirementType_Insert(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, null, false, true, false);

                //Now we need to reload the bound dataset for the next databind
                LoadRequirementTypes();
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
        /// Handles the event raised when the requirement types UPDATE button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnRequirementTypesUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRequirementTypesUpdate_Click";

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
                    LoadRequirementTypes();

                    //Let the user know that the settings were saved
                    this.lblMessage.Text = Resources.Messages.RequirementTypes_Success;
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
        /// Update the requirement types
        /// </summary>
        protected bool UpdateTypes()
        {
            //First we need to retrieve the existing list of requirement types
            RequirementManager requirementManager = new RequirementManager();
            List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(this.ProjectTemplateId, false, false);

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditRequirementTypes.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditRequirementTypes.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtRequirementTypeName = (TextBoxEx)grdEditRequirementTypes.Rows[i].FindControl("txtRequirementTypeName");
                    DropDownListEx ddlWorkflow = (DropDownListEx)grdEditRequirementTypes.Rows[i].FindControl("ddlWorkflow");
                    RadioButtonEx radDefault = (RadioButtonEx)grdEditRequirementTypes.Rows[i].FindControl("radDefault");
                    CheckBoxYnEx chkActiveYn = (CheckBoxYnEx)grdEditRequirementTypes.Rows[i].FindControl("chkActiveYn");
                    CheckBoxYnEx chkSteps = (CheckBoxYnEx)grdEditRequirementTypes.Rows[i].FindControl("chkSteps");

                    //Need to make sure that the default item is an active one
                    if (radDefault.Checked && !(chkActiveYn.Checked))
                    {
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        this.lblMessage.Text = Resources.Messages.RequirementTypes_CannotSetDefaultTypeInactive;
                        return false;
                    }

                    //Now get the requirement type id
                    int requirementTypeId = Int32.Parse(txtRequirementTypeName.MetaData);

                    //Find the matching row in the dataset
                    RequirementType requirementType = requirementTypes.FirstOrDefault(t => t.RequirementTypeId == requirementTypeId);

                    //Make sure we found the matching row
                    if (requirementType != null)
                    {
                        //Update the various fields
                        requirementType.StartTracking();
                        requirementType.Name = txtRequirementTypeName.Text.Trim();
                        requirementType.RequirementWorkflowId = Int32.Parse(ddlWorkflow.SelectedValue);
                        requirementType.IsDefault = radDefault.Checked;
                        requirementType.IsActive = chkActiveYn.Checked;
                        requirementType.IsSteps = chkSteps.Checked;
                    }
                }
            }

            foreach (RequirementType requirementType in requirementTypes)
            {
                requirementManager.RequirementType_Update(requirementType);
            }
            return true;
        }

        /// <summary>
        /// Loads the requirement types configured for the current project
        /// </summary>
        protected void LoadRequirementTypes()
        {
            const string METHOD_NAME = "LoadRequirementTypes";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            RequirementManager requirementManager = new RequirementManager();
            RequirementWorkflowManager workflowManager = new RequirementWorkflowManager();

            //Get the yes/no flag list
            this.flagList = requirementManager.RetrieveFlagLookup();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of requirement types for this project
            List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(this.ProjectTemplateId, false, activeOnly);
            this.grdEditRequirementTypes.DataSource = requirementTypes;

            //Get the list of active workflows for this project (used as a lookup)
            this.workflows = workflowManager.Workflow_Retrieve(this.ProjectTemplateId, true);

            //Databind the grid
            this.grdEditRequirementTypes.DataBind();

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
            LoadRequirementTypes();
        }
    }
}
