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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "RiskProbabilities_Title", "Template-Risks/#probability", "RiskProbabilities_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RiskProbabilities : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskProbabilities";

		const int MAX_ROW_COUNT = 5;

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
                 
                this.btnRiskProbabilityUpdate.Click += new EventHandler(btnRiskProbabilityUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadRiskProbabilities();
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
        /// Loads the risk probabilities configured for the current project
        /// </summary>
        protected void LoadRiskProbabilities(int? take = null)
        {
            const string METHOD_NAME = "LoadRiskProbabilities";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            RiskManager riskManager = new RiskManager();

            //Get the list of risk probabilities for this project
            List<RiskProbability> probabilities = riskManager.RiskProbability_Retrieve(this.ProjectTemplateId, true);
			int takeRowCount = 0;

			if (take.HasValue)
			{
				takeRowCount = take.Value;	
			}
			else
			{
				if (probabilities.Count >= MAX_ROW_COUNT)
				{
					ddlFilterType.SelectedValue = MAX_ROW_COUNT.ToString();
					takeRowCount = MAX_ROW_COUNT;
				}
				else
				{
					ddlFilterType.SelectedValue = probabilities.Count.ToString();
					takeRowCount = probabilities.Count;
				}
			}

			selectedFilter.Value = takeRowCount.ToString();
			var source = probabilities.Take(takeRowCount).ToList();
			if(source.Count() < takeRowCount)
			{
				int addMoreRows = takeRowCount - source.Count();
				for (int i = 0; i < addMoreRows; i++)
				{
					source.Add(new RiskProbability
					{
						 Color = "FFFFFF",
						 IsActive = true,
						 Name = String.Empty,
						 Position = i,
						 RiskProbabilityId = 0,
						 Score = 0
					});

				}
			}

			//Databind the grid
			this.grdEditRiskProbabilities.DataSource = source;
            this.grdEditRiskProbabilities.DataBind();

            //Populate any static fields
            this.lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

		 

			/// <summary>
			/// Handles the event raised when the risk probability UPDATE button is clicked
			/// </summary>
			/// <param name="sender">The sending object</param>
			/// <param name="e">The event arguments</param>
			private void btnRiskProbabilityUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnRiskProbabilityUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try
			{
                //Update the probabilities
                UpdateProbabilities();

				//Now we need to reload the bound dataset for the next databind
				LoadRiskProbabilities();

				//Let the user know that the settings were saved
				this.lblMessage.Text = Resources.Messages.RiskProbabilities_Success;
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
        /// Updates the probabilities
        /// </summary>
        protected void UpdateProbabilities()
        {
            //First we need to retrieve the existing list of risk probabilities
            RiskManager riskManager = new RiskManager();
            List<RiskProbability> riskProbabilities = riskManager.RiskProbability_Retrieve(this.ProjectTemplateId, false);

			List<RiskProbability> changedEntities = new List<RiskProbability>();

			//We need to make sure that at least one probability is active
			int activeCount = 0;

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditRiskProbabilities.Rows.Count; i++)
            {
				//We only look at item rows (i.e. not headers and footers)
				if (grdEditRiskProbabilities.Rows[i].RowType == DataControlRowType.DataRow)
				{
					//Extract the various controls from the datagrid
					TextBoxEx txtName = (TextBoxEx)grdEditRiskProbabilities.Rows[i].FindControl("txtRiskProbabilityName");
					//ColorPicker colColor = (ColorPicker)grdEditRiskProbabilities.Rows[i].FindControl("colRiskProbabilityColor");
					CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditRiskProbabilities.Rows[i].FindControl("chkActive");
					TextBoxEx txtScore = (TextBoxEx)grdEditRiskProbabilities.Rows[i].FindControl("txtScore");
					TextBoxEx txtPosition = (TextBoxEx)grdEditRiskProbabilities.Rows[i].FindControl("txtPosition");

					//Now get the risk probability id
					int probabilityId = Int32.Parse(txtName.MetaData);

					//Get the score and position
					int score = 0;
					int position = 1;
					Int32.TryParse(txtScore.Text, out score);
					Int32.TryParse(txtPosition.Text, out position);

					//Find the matching row in the list
					RiskProbability riskProbability = riskProbabilities.FirstOrDefault(p => p.RiskProbabilityId == probabilityId);

					//Increment the active count if appropriate
					if (chkActiveFlag.Checked)
					{
						activeCount++;
					}

					if (riskProbability == null)
					{
						riskProbability = new RiskProbability();
						riskProbability.ProjectTemplateId = this.ProjectTemplateId;
						riskProbability.MarkAsAdded();
						riskProbability.Color = "FFFFFF";
					}
					else
					{
						//Update the various fields
						riskProbability.MarkAsModified();
					}

					
					riskProbability.Name = txtName.Text;
					riskProbability.IsActive = true;
					riskProbability.Score = score;
					riskProbability.Position = position;
					changedEntities.Add(riskProbability);
				}
            }

            //Make sure that at least one probability is active
            if (activeCount == 0)
            {
                this.lblMessage.Text = Resources.Messages.RiskProbabilities_AtLeastOneMustBeActive;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

			var obseleteEntites = riskProbabilities.Where(f1 => changedEntities.All(f2 => f2.RiskProbabilityId != f1.RiskProbabilityId)).ToList();
			for (int i = 0; i < obseleteEntites.Count(); i++)
			{
				obseleteEntites[i].IsActive = false;
				obseleteEntites[i].MarkAsModified();
				riskManager.RiskProbability_Update(obseleteEntites[i]);
			}

			//Make the updates
			for (int i = 0; i < changedEntities.Count; i++)
            {
                riskManager.RiskProbability_Update(changedEntities[i]);
            }
        }

        /// <summary>
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
			LoadRiskProbabilities(Int32.Parse(ddlFilterType.SelectedValue));
		}

		protected void grdEditRiskProbabilities_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			int maxScore = Int32.Parse(ddlFilterType.SelectedValue) + 1;

			if(e.Row.RowType == DataControlRowType.DataRow)
			{
				var dataItem = e.Row.DataItem as RiskProbability;
				TextBoxEx txtScore = e.Row.FindControl("txtScore") as TextBoxEx;
				txtScore.Text = (maxScore - dataItem.Position).ToString();

			}
		}
	}
}
