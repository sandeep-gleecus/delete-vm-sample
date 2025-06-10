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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "IncidentPriorities_Title", "Template-Incidents/#priorities", "IncidentPriorities_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class IncidentPriorities : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.IncidentPriorities";

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
                this.btnIncidentPriorityAdd.Click += new EventHandler(btnIncidentPriorityAdd_Click);
                this.btnIncidentPriorityUpdate.Click += new EventHandler(btnIncidentPriorityUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadIncidentPriorities();
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
        /// Loads the incident priorities configured for the current project
        /// </summary>
        protected void LoadIncidentPriorities()
        {
            const string METHOD_NAME = "LoadIncidentPriorities";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            IncidentManager incidentManager = new IncidentManager();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of incident priorities for this project
            List<IncidentPriority> priorities = incidentManager.RetrieveIncidentPriorities(this.ProjectTemplateId, activeOnly);

            //Databind the grid
            this.grdEditIncidentPriorities.DataSource = priorities;
            this.grdEditIncidentPriorities.DataBind();

            //Populate any static fields
            this.lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

		/// <summary>
		/// Handles the event raised when the incident priority ADD button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnIncidentPriorityAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnIncidentPriorityAdd_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try          
            {
                //First update the existing priorities
                UpdatePriorities();

				//Now we need to insert the new incident priority (default to white)
				IncidentManager incidentManager = new IncidentManager();
				incidentManager.InsertIncidentPriority(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, "ffffff", true);

				//Now we need to reload the bound dataset for the next databind
				LoadIncidentPriorities();
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
		/// Handles the event raised when the incident priority UPDATE button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnIncidentPriorityUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnIncidentPriorityUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try
			{
                //Update the priorities
                UpdatePriorities();

                //Now we need to reload the bound dataset for the next databind
                LoadIncidentPriorities();

                //Let the user know that the settings were saved
                this.lblMessage.Text = Resources.Messages.IncidentPriorities_Success;
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
        /// Updates the priorities
        /// </summary>
        protected void UpdatePriorities()
        {
            //First we need to retrieve the existing list of incident priorities
            IncidentManager incidentManager = new IncidentManager();
            List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(this.ProjectTemplateId, false);

            //We need to make sure that at least one priority is active
            int activeCount = 0;

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditIncidentPriorities.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditIncidentPriorities.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditIncidentPriorities.Rows[i].Cells[1].Controls[1];
                    ColorPicker colColor = (ColorPicker)grdEditIncidentPriorities.Rows[i].Cells[2].Controls[1];
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditIncidentPriorities.Rows[i].Cells[3].Controls[1];

                    //Now get the incident priority id
                    int priorityId = Int32.Parse(txtDisplayName.MetaData);

                    //Find the matching row in the dataset
                    IncidentPriority incidentPriority = incidentPriorities.FirstOrDefault(p => p.PriorityId == priorityId);

                    //Increment the active count if appropriate
                    if (chkActiveFlag.Checked)
                    {
                        activeCount++;
                    }

                    //Make sure we found the matching row
                    if (incidentPriority != null)
                    {
                        //Update the various fields
                        incidentPriority.StartTracking();
                        incidentPriority.Name = txtDisplayName.Text;
                        incidentPriority.Color = colColor.Text;
                        incidentPriority.IsActive = chkActiveFlag.Checked;
                    }
                }
            }

            //Make sure that at least one priority is active
            if (activeCount == 0)
            {
                this.lblMessage.Text = Resources.Messages.IncidentPriorities_AtLeastOneMustBeActive;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Make the updates
            for (int i = 0; i < incidentPriorities.Count; i++)
            {
                incidentManager.IncidentPriority_Update(incidentPriorities[i]);
            }
        }

        /// <summary>
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Save the data and then reload
            UpdatePriorities();
            LoadIncidentPriorities();
        }
    }
}