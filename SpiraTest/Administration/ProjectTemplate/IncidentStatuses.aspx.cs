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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "IncidentStatuses_Title", "Template-Incidents/#edit-statuses", "IncidentStatuses_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class IncidentStatuses : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentStatuses";

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

                //Register the event handlers
                this.ddlFilterType.SelectedIndexChanged += new EventHandler(ddlFilterType_SelectedIndexChanged);
                this.btnIncidentStatusAdd.Click += new EventHandler(btnIncidentStatusAdd_Click);
                this.btnIncidentStatusUpdate.Click += new EventHandler(btnIncidentStatusUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadIncidentStatuses();
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
        /// Handles the event raised when the incident status UPDATE button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnIncidentStatusUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnIncidentStatusUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            try
            {
                //Update the statuses
                bool success = UpdatedStatuses();

                //Now we need to reload the bound dataset for the next databind
                if (success)
                {
                    LoadIncidentStatuses();

                    //Let the user know that the settings were saved
                    this.lblMessage.Text = Resources.Messages.IncidentStatuses_Success;
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
        /// Loads the incident types configured for the current project
        /// </summary>
        protected void LoadIncidentStatuses()
        {
            const string METHOD_NAME = "LoadIncidentStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            IncidentManager incidentManager = new IncidentManager();

            //Get the yes/no flag list
            this.flagList = incidentManager.RetrieveFlagLookup();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of incident statuses for this project
            List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(this.ProjectTemplateId, activeOnly);
            this.grdEditIncidentStatuses.DataSource = incidentStati;

            //Databind the grid
            this.grdEditIncidentStatuses.DataBind();

            //Populate any static fields
            this.lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates the statuses
        /// </summary>
        protected bool UpdatedStatuses()
        {
            //First we need to retrieve the existing list of incident statuses
            IncidentManager incidentManager = new IncidentManager();
            List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(this.ProjectTemplateId, false);

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditIncidentStatuses.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditIncidentStatuses.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditIncidentStatuses.Rows[i].Cells[1].Controls[1];
                    CheckBoxEx chkOpenStatus = (CheckBoxEx)grdEditIncidentStatuses.Rows[i].Cells[2].Controls[1];
                    RadioButtonEx radDefault = (RadioButtonEx)grdEditIncidentStatuses.Rows[i].Cells[3].Controls[1];
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditIncidentStatuses.Rows[i].Cells[4].Controls[1];

                    //Need to make sure that the default item is an active one
                    if (radDefault.Checked && !(chkActiveFlag.Checked))
                    {
                        this.lblMessage.Type = MessageBox.MessageType.Error; 
                        this.lblMessage.Text = "You cannot set the default incident status to an inactive value";
                        return false;
                    }

                    //Now get the incident status id
                    int incidentStatusId = Int32.Parse(txtDisplayName.MetaData);

                    //Find the matching row in the dataset
                    IncidentStatus incidentStatus = incidentStati.FirstOrDefault(s => s.IncidentStatusId == incidentStatusId);

                    //Make sure we found the matching row
                    if (incidentStatus != null)
                    {
                        //Update the various fields
                        incidentStatus.StartTracking();
                        incidentStatus.Name = txtDisplayName.Text.Trim();
                        incidentStatus.IsOpenStatus = chkOpenStatus.Checked;
                        incidentStatus.IsDefault = radDefault.Checked;
                        incidentStatus.IsActive = chkActiveFlag.Checked;
                    }
                }
            }

            foreach (IncidentStatus incidentStatus in incidentStati)
            {
                incidentManager.IncidentStatus_Update(incidentStatus);
            }
            return true;
        }

        /// <summary>
        /// Handles the event raised when the incident status ADD button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnIncidentStatusAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnIncidentStatusAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First update the existing statuses
                UpdatedStatuses();

                //Now we need to insert the new incident status
                IncidentManager incidentManager = new IncidentManager();
                incidentManager.IncidentStatus_Insert(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, true, false, true);

                //Now we need to reload the bound dataset for the next databind
                LoadIncidentStatuses();
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
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Save the data and then reload
            UpdatedStatuses();
            LoadIncidentStatuses();
        }
    }
}