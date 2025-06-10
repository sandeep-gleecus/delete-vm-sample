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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "IncidentSeverities_Title", "Template-Incidents/#severities", "IncidentSeverities_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class IncidentSeverities : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentSeverities";

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
                this.btnIncidentSeverityAdd.Click += new EventHandler(btnIncidentSeverityAdd_Click);
                this.btnIncidentSeverityUpdate.Click += new EventHandler(btnIncidentSeverityUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadIncidentSeverities();
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
        /// Handles the event raised when the incident severity ADD button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnIncidentSeverityAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnIncidentSeverityAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            try
            {
                //Update the severities
                UpdateSeverities();

                //Now we need to insert the new incident severity (default to white)
                IncidentManager incidentManager = new IncidentManager();
                incidentManager.InsertIncidentSeverity(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, "ffffff", true);

                //Now we need to reload the bound dataset for the next databind
                LoadIncidentSeverities();
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

        protected void UpdateSeverities()
        {
            //First we need to retrieve the existing list of incident severities
            IncidentManager incidentManager = new IncidentManager();
            List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(this.ProjectTemplateId, false);

            //We need to make sure that at least one priority is active
            int activeCount = 0;

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditIncidentSeverities.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditIncidentSeverities.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditIncidentSeverities.Rows[i].Cells[1].Controls[1];
                    ColorPicker colColor = (ColorPicker)grdEditIncidentSeverities.Rows[i].Cells[2].Controls[1];
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditIncidentSeverities.Rows[i].Cells[3].Controls[1];

                    //Now get the incident severity id
                    int severityId = Int32.Parse(txtDisplayName.MetaData);

                    //Find the matching row in the dataset
                    IncidentSeverity incidentSeverity = incidentSeverities.FirstOrDefault(p => p.SeverityId == severityId);

                    //Increment the active count if appropriate
                    if (chkActiveFlag.Checked)
                    {
                        activeCount++;
                    }

                    //Make sure we found the matching row
                    if (incidentSeverity != null)
                    {
                        //Update the various fields
                        incidentSeverity.StartTracking();
                        incidentSeverity.Name = txtDisplayName.Text;
                        incidentSeverity.Color = colColor.Text;
                        incidentSeverity.IsActive = chkActiveFlag.Checked;
                    }
                }
            }

            //Make sure that at least one priority is active
            if (activeCount == 0)
            {
                this.lblMessage.Text = Resources.Messages.IncidentSeverities_AtLeastOneMustBeActive;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Make the updates
            for (int i = 0; i < incidentSeverities.Count; i++)
            {
                incidentManager.IncidentSeverity_Update(incidentSeverities[i]);
            }
        }

        /// <summary>
        /// Handles the event raised when the incident severity UPDATE button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnIncidentSeverityUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnIncidentSeverityUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            try
            {
                //Update the severities
                UpdateSeverities();

                //Now we need to reload the bound dataset for the next databind
                LoadIncidentSeverities();

                //Let the user know that the settings were saved
                this.lblMessage.Text = Resources.Messages.IncidentSeverities_Success;
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
        /// Loads the incident severities configured for the current project
        /// </summary>
        protected void LoadIncidentSeverities()
        {
            const string METHOD_NAME = "LoadIncidentSeverities";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            IncidentManager incidentManager = new IncidentManager();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of incident severities for this project
            List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(this.ProjectTemplateId, activeOnly);

            //Databind the grid
            this.grdEditIncidentSeverities.DataSource = incidentSeverities;
            this.grdEditIncidentSeverities.DataBind();

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
            UpdateSeverities();
            LoadIncidentSeverities();
        }
    }
}