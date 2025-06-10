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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "RequirementImportances_Title", "Template-Requirements/#importance", "RequirementImportances_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RequirementImportances : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RequirementImportances";

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
                this.btnAdd.Click += new EventHandler(btnAdd_Click);
                this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadRequirementImportances();
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
        /// Loads the requirement priorities configured for the current project template
        /// </summary>
        protected void LoadRequirementImportances()
        {
            const string METHOD_NAME = "LoadRequirementImportances";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            RequirementManager requirementManager = new RequirementManager();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of requirement importances for this project
            List<Importance> importances = requirementManager.RequirementImportance_Retrieve(this.ProjectTemplateId, activeOnly);

            //Databind the grid
            this.grdEditRequirementImportances.DataSource = importances;
            this.grdEditRequirementImportances.DataBind();

            //Populate any static fields
            this.lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

		/// <summary>
		/// Handles the event raised when the requirement priority ADD button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnAdd_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try          
            {
                //First update the existing priorities
                UpdateImportances();

				//Now we need to insert the new requirement priority (default to white)
				RequirementManager requirementManager = new RequirementManager();
				requirementManager.RequirementImportance_Insert(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, "ffffff", true, 0);

				//Now we need to reload the bound dataset for the next databind
				LoadRequirementImportances();
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
		/// Handles the event raised when the requirement priority UPDATE button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try
			{
                //Update the priorities
                UpdateImportances();

                //Now we need to reload the bound dataset for the next databind
                LoadRequirementImportances();

                //Let the user know that the settings were saved
                this.lblMessage.Text = Resources.Messages.Admin_Priorities_Success;
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
        protected void UpdateImportances()
        {
            //First we need to retrieve the existing list of requirement priorities
            RequirementManager requirementManager = new RequirementManager();
            List<Importance> importances = requirementManager.RequirementImportance_Retrieve(this.ProjectTemplateId, false);

            //We need to make sure that at least one priority is active
            int activeCount = 0;

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditRequirementImportances.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditRequirementImportances.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditRequirementImportances.Rows[i].FindControl("txtRequirementImportanceName");
                    TextBoxEx txtScore = (TextBoxEx)grdEditRequirementImportances.Rows[i].FindControl("txtScore");
                    ColorPicker colColor = (ColorPicker)grdEditRequirementImportances.Rows[i].FindControl("colRequirementImportanceColor");
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditRequirementImportances.Rows[i].FindControl("ddlActive");

                    //Now get the requirement priority id
                    int priorityId = Int32.Parse(txtDisplayName.MetaData);
                    int score = 0;
                    Int32.TryParse(txtScore.Text, out score);

                    //Find the matching row in the dataset
                    Importance importance = importances.FirstOrDefault(p => p.ImportanceId == priorityId);

                    //Increment the active count if appropriate
                    if (chkActiveFlag.Checked)
                    {
                        activeCount++;
                    }

                    //Make sure we found the matching row
                    if (importance != null)
                    {
                        //Update the various fields
                        importance.StartTracking();
                        importance.Name = txtDisplayName.Text;
                        importance.Color = colColor.Text;
                        importance.IsActive = chkActiveFlag.Checked;
                        importance.Score = score;
                    }
                }
            }

            //Make sure that at least one importance is active
            if (activeCount == 0)
            {
                this.lblMessage.Text = Resources.Messages.Admin_Importances_AtLeastOneMustBeActive;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Make the updates
            for (int i = 0; i < importances.Count; i++)
            {
                requirementManager.RequirementImportance_Update(importances[i]);
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
            UpdateImportances();
            LoadRequirementImportances();
        }
    }
}