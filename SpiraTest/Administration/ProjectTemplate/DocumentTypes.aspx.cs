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
    /// Administration Edit Document Types Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "DocumentTypes_Title", "Template-Documents/#document-types", "DocumentTypes_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class DocumentTypes : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.DocumentTypes";

        //Bound data for the grid
        protected SortedList<string, string> flagList;
        protected List<DocumentWorkflow> workflows;

        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Redirect if there's no project template selected.
                if (ProjectTemplateId < 1)
                    Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);

                //Register event handlers
                this.btnAddDocumentType.Click += new EventHandler(btnAddDocumentType_Click);
                this.btnUpdateDocumentTypes.Click += new EventHandler(btnUpdateDocumentTypes_Click);
                this.ddlFilterType.SelectedIndexChanged += new EventHandler(ddlFilterType_SelectedIndexChanged);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadDocumentTypes();
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
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //reload the data
            LoadDocumentTypes();
        }

        /// <summary>Updates the document types</summary>
        protected void UpdateDocumentTypes()
        {
            //First we need to retrieve the existing document types
            AttachmentManager attachmentManager = new AttachmentManager();
            List<DocumentType> projectAttachmentTypes = attachmentManager.RetrieveDocumentTypes(this.ProjectTemplateId, false);

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdDocumentTypes.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdDocumentTypes.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the textbox and drop-down list from the gridview
                    TextBoxEx txtName = (TextBoxEx)grdDocumentTypes.Rows[i].FindControl("txtName");
                    TextBoxEx txtDescription = (TextBoxEx)grdDocumentTypes.Rows[i].FindControl("txtDescription");
                    RadioButtonEx radDefault = (RadioButtonEx)grdDocumentTypes.Rows[i].FindControl("radDefaultYn");
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdDocumentTypes.Rows[i].FindControl("chkActiveYn");
                    DropDownListEx ddlWorkflow = (DropDownListEx)grdDocumentTypes.Rows[i].FindControl("ddlWorkflow");

                    //Now get the document tyoe name and the document type id
                    int documentTypeId = Int32.Parse(txtName.MetaData);
                    string documentName = txtName.Text.Trim();
                    string documentDesc = txtDescription.Text.Trim();
                    bool isActive = (chkActiveFlag.Checked);
                    bool isDefault = radDefault.Checked;
                    int workflowId = Int32.Parse(ddlWorkflow.SelectedValue);

                    //Find the matching row in the dataset
                    DocumentType typeRow = projectAttachmentTypes.FirstOrDefault(p => p.DocumentTypeId == documentTypeId);

                    //Make sure we found the matching row
                    if (typeRow != null)
                    {
                        //Track changes
                        typeRow.StartTracking();

                        //See if the name changed
                        if (typeRow.Name != documentName)
                        {
                            //Update the name
                            typeRow.Name = documentName;
                        }

                        //Set the workflow if changed
                        if (typeRow.DocumentWorkflowId != workflowId)
                        {
                            typeRow.DocumentWorkflowId = workflowId;
                        }

                        //See if the description changed
                        if (typeRow.Description != documentDesc)
                        {
                            //Update the description
                            if (String.IsNullOrEmpty(documentDesc))
                            {
                                typeRow.Description = null;
                            }
                            else
                            {
                                typeRow.Description = documentDesc;
                            }
                        }

                        //See if the active flag changed
                        if (typeRow.IsActive != isActive)
                        {
                            //Update the active flag
                            typeRow.IsActive = isActive;
                        }

                        //See if the default flag changed
                        if (typeRow.IsDefault != isDefault)
                        {
                            //Update the default flag
                            typeRow.IsDefault = isDefault;
                        }
                    }
                }
            }

            //Perform the update, handling any business exceptions
            try
            {
                attachmentManager.UpdateDocumentTypes(projectAttachmentTypes);
            }
            catch (ProjectDefaultAttachmentTypeException exception)
            {
                //Display these in a message box
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }

        /// <summary>Handles the event raised when the document types UPDATE button is clicked</summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        void btnUpdateDocumentTypes_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdateDocumentTypes_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            try
            {
                //Now update the document types
                UpdateDocumentTypes();

                //Refresh the datagrid
                LoadDocumentTypes();

                //Display a message indicating success
                if (this.lblMessage.Text == "")
                {
                    this.lblMessage.Text = Resources.Messages.DocumentTypes_Success;
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
        /// Loads the list of document types
        /// </summary>
        protected void LoadDocumentTypes()
        {
            const string METHOD_NAME = "LoadDocumentTypes";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            AttachmentManager attachment = new AttachmentManager();
            DocumentWorkflowManager workflowManager = new DocumentWorkflowManager();

            //Get the yes/no flag list
            this.flagList = attachment.RetrieveFlagLookup();

            //See the current filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the current document types for the project template
            List<DocumentType> documentTypes = attachment.RetrieveDocumentTypes(this.ProjectTemplateId, activeOnly);

            //Get the list of active workflows for this project (used as a lookup)
            this.workflows = workflowManager.Workflow_Retrieve(this.ProjectTemplateId, true);

            //Databind the grid
            this.grdDocumentTypes.DataSource = documentTypes;
            this.grdDocumentTypes.DataBind();

            //Populate any static fields
            this.lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>Handles the event raised when the document types ADD button is clicked</summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        void btnAddDocumentType_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnAddDocumentType_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            try
            {
                //First update the values (in case user has modified them before clicking Add)
                UpdateDocumentTypes();

                //Now we need to insert the new document type entry
                AttachmentManager attachmentManager = new AttachmentManager();

                attachmentManager.InsertDocumentType(this.ProjectTemplateId, Resources.Dialogs.Global_NewDocumentType, null, true, false);

                //Now we need to reload the bound dataset for the next databind
                LoadDocumentTypes();
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