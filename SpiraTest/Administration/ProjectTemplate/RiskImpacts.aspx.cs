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
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "RiskImpacts_Title", "Template-Risks/#impact", "RiskImpacts_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class RiskImpacts : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskImpacts";
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
                this.btnRiskImpactUpdate.Click += new EventHandler(btnRiskImpactUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadRiskImpacts();
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

         

        protected void UpdateImpacts()
        {
            //First we need to retrieve the existing list of risk impacts
            RiskManager riskManager = new RiskManager();
            List<RiskImpact> riskImpacts = riskManager.RiskImpact_Retrieve(this.ProjectTemplateId, false);

            //We need to make sure that at least one probability is active
            int activeCount = 0;
			List<RiskImpact> changedEntities = new List<RiskImpact>();
			//Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
			for (int i = 0; i < this.grdEditRiskImpacts.Rows.Count; i++)
            {
				//We only look at item rows (i.e. not headers and footers)
				if (grdEditRiskImpacts.Rows[i].RowType == DataControlRowType.DataRow)
				{
					//Extract the various controls from the datagrid
					TextBoxEx txtName = (TextBoxEx)grdEditRiskImpacts.Rows[i].FindControl("txtRiskImpactName");
					CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditRiskImpacts.Rows[i].FindControl("chkActive");
					TextBoxEx txtScore = (TextBoxEx)grdEditRiskImpacts.Rows[i].FindControl("txtScore");
					TextBoxEx txtPosition = (TextBoxEx)grdEditRiskImpacts.Rows[i].FindControl("txtPosition");

					//Now get the risk impact id
					int impactId = Int32.Parse(txtName.MetaData);

					//Get the score and position
					int score = 0;
					int position = 1;
					Int32.TryParse(txtScore.Text, out score);
					Int32.TryParse(txtPosition.Text, out position);

					//Find the matching row in the dataset
					RiskImpact riskImpact = riskImpacts.FirstOrDefault(p => p.RiskImpactId == impactId);

					//Increment the active count if appropriate
					if (chkActiveFlag.Checked)
					{
						activeCount++;
					}

					if (riskImpact == null)
					{
						riskImpact = new RiskImpact();
						riskImpact.ProjectTemplateId = this.ProjectTemplateId;
						riskImpact.MarkAsAdded();
						riskImpact.Color = "FFFFFF";
					}
					else
					{
						//Update the various fields
						riskImpact.MarkAsModified();
					}


					//Update the various fields
					riskImpact.StartTracking();
					riskImpact.Name = txtName.Text;
					riskImpact.Color = "FFFFFF";
					riskImpact.Score = score;
					riskImpact.Position = position;
					riskImpact.IsActive = true;
					changedEntities.Add(riskImpact);

				}
            }

            //Make sure that at least one probability is active
            if (activeCount == 0)
            {
                this.lblMessage.Text = Resources.Messages.RiskImpacts_AtLeastOneMustBeActive;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

			var obseleteEntites = riskImpacts.Where(f1 => changedEntities.All(f2 => f2.RiskImpactId != f1.RiskImpactId)).ToList();
			for (int i = 0; i < obseleteEntites.Count(); i++)
			{
				obseleteEntites[i].IsActive = false;
				obseleteEntites[i].MarkAsModified();
				riskManager.RiskImpact_Update(obseleteEntites[i]);
			}

			//Make the updates
			for (int i = 0; i < changedEntities.Count; i++)
            {
                riskManager.RiskImpact_Update(changedEntities[i]);
            }
        }

        /// <summary>
        /// Handles the event raised when the risk impact UPDATE button is clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnRiskImpactUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRiskImpactUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
            if (!this.IsValid)
            {
                return;
            }

            try
            {
                //Update the impacts
                UpdateImpacts();

                //Now we need to reload the bound dataset for the next databind
                LoadRiskImpacts();

                //Let the user know that the settings were saved
                this.lblMessage.Text = Resources.Messages.RiskImpacts_Success;
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
		/// Loads the risk impacts configured for the current project
		/// </summary>
		protected void LoadRiskImpacts(int? take = null)
        {
            const string METHOD_NAME = "LoadRiskImpacts";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            RiskManager riskManager = new RiskManager();

             
            //Get the list of risk impacts for this project
            List<RiskImpact> riskImpacts = riskManager.RiskImpact_Retrieve(this.ProjectTemplateId, true);
			int takeRowCount = 0;

			if (take.HasValue)
			{
				takeRowCount = take.Value;
			}
			else
			{
				if (riskImpacts.Count >= MAX_ROW_COUNT)
				{
					ddlFilterType.SelectedValue = MAX_ROW_COUNT.ToString();
					takeRowCount = MAX_ROW_COUNT;
				}
				else
				{
					ddlFilterType.SelectedValue = riskImpacts.Count.ToString();
					takeRowCount = riskImpacts.Count;
				}
			}

			selectedFilter.Value = takeRowCount.ToString();
			var source = riskImpacts.Take(takeRowCount).ToList();
			if (source.Count() < takeRowCount)
			{
				int addMoreRows = takeRowCount - source.Count();
				for (int i = 0; i < addMoreRows; i++)
				{
					source.Add(new RiskImpact
					{
						Color = "FFFFFF",
						IsActive = true,
						Name = String.Empty,
						Position = i,
						RiskImpactId = 0,
						Score = 0
					});

				}
			}
			
			//Databind the grid
			this.grdEditRiskImpacts.DataSource = source;
            this.grdEditRiskImpacts.DataBind();

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
            LoadRiskImpacts(Int32.Parse(ddlFilterType.SelectedValue));
        }

		protected void grdEditRiskImpacts_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			int maxScore = Int32.Parse(ddlFilterType.SelectedValue) + 1;

			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				var dataItem = e.Row.DataItem as RiskImpact;
				TextBoxEx txtScore = e.Row.FindControl("txtScore") as TextBoxEx;
				txtScore.Text = (maxScore - dataItem.Position).ToString();

			}
		}
	}
}
