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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "RiskStatuses_Title", "Template-Risks/#edit-statuses", "RiskStatuses_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RiskStatuses : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskStatuses";

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
                this.btnRiskStatusAdd.Click += new EventHandler(btnRiskStatusAdd_Click);
                this.btnRiskStatusUpdate.Click += new EventHandler(btnRiskStatusUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadRiskStatuses();
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
        /// Handles the event raised when the risk status UPDATE button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnRiskStatusUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRiskStatusUpdate_Click";

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
                    LoadRiskStatuses();

                    //Let the user know that the settings were saved
                    this.lblMessage.Text = Resources.Messages.RiskStatuses_Success;
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
        /// Loads the risk types configured for the current project
        /// </summary>
        protected void LoadRiskStatuses()
        {
            const string METHOD_NAME = "LoadRiskStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            RiskManager riskManager = new RiskManager();

            //Get the yes/no flag list
            this.flagList = riskManager.RetrieveFlagLookup();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of risk statuses for this project
            List<RiskStatus> riskStati = riskManager.RiskStatus_Retrieve(this.ProjectTemplateId, activeOnly);
            this.grdEditRiskStatuses.DataSource = riskStati;

            //Databind the grid
            this.grdEditRiskStatuses.DataBind();

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
            //First we need to retrieve the existing list of risk statuses
            RiskManager riskManager = new RiskManager();
            List<RiskStatus> riskStati = riskManager.RiskStatus_Retrieve(this.ProjectTemplateId, false);

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditRiskStatuses.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditRiskStatuses.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditRiskStatuses.Rows[i].FindControl("txtRiskStatusName");
                    CheckBoxEx chkOpenStatus = (CheckBoxEx)grdEditRiskStatuses.Rows[i].FindControl("chkOpenRiskStatus");
                    RadioButtonEx radDefault = (RadioButtonEx)grdEditRiskStatuses.Rows[i].FindControl("radRiskStatusDefault");
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditRiskStatuses.Rows[i].FindControl("chkActiveFlagYn");
                    TextBoxEx txtPosition = (TextBoxEx)grdEditRiskStatuses.Rows[i].FindControl("txtPosition");

                    //Need to make sure that the default item is an active one
                    if (radDefault.Checked && !(chkActiveFlag.Checked))
                    {
                        this.lblMessage.Type = MessageBox.MessageType.Error; 
                        this.lblMessage.Text = "You cannot set the default risk status to an inactive value";
                        return false;
                    }

                    //Now get the risk status id
                    int riskStatusId = Int32.Parse(txtDisplayName.MetaData);

                    //Get the position
                    int position = 1;
                    Int32.TryParse(txtPosition.Text, out position);

                    //Find the matching row in the list
                    RiskStatus riskStatus = riskStati.FirstOrDefault(s => s.RiskStatusId == riskStatusId);

                    //Make sure we found the matching row
                    if (riskStatus != null)
                    {
                        //Update the various fields
                        riskStatus.StartTracking();
                        riskStatus.Name = txtDisplayName.Text.Trim();
                        riskStatus.IsOpen = chkOpenStatus.Checked;
                        riskStatus.IsDefault = radDefault.Checked;
                        riskStatus.IsActive = chkActiveFlag.Checked;
                        riskStatus.Position = position;
                    }
                }
            }

            foreach (RiskStatus riskStatus in riskStati)
            {
                riskManager.RiskStatus_Update(riskStatus);
            }
            return true;
        }

        /// <summary>
        /// Handles the event raised when the risk status ADD button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnRiskStatusAdd_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRiskStatusAdd_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First update the existing statuses
                UpdatedStatuses();

                //Now we need to insert the new risk status
                RiskManager riskManager = new RiskManager();
                riskManager.RiskStatus_Insert(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, true, false, true);

                //Now we need to reload the bound dataset for the next databind
                LoadRiskStatuses();
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
            LoadRiskStatuses();
        }
    }
}