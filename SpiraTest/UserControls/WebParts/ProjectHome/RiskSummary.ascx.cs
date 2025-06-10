using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the table summarizing the count of risks
	/// </summary>
	public partial class RisksSummary : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RisksSummary::";

        protected List<RiskProbability> probabilities;
        protected List<RiskView> openRisks;

		private string[,] riskMatrix = new string[5, 5] { 
															{ "35B729 ", "FF9900", "FF9900","DE3700","DE3700" },
															{ "35B729 ", "FFCD05", "FF9900","FF9900","DE3700"},
															{ "35B729 ", "35B729", "FFBF00","FF9900","FF9900" },
															{ "0077BE", "35B729", "35B729","FFBF00","FF9900" },
															{ "0077BE", "0077BE", "35B729","35B729","35B729"},
														};

        /// <summary>
        /// Overrides the onInit method to add any dynamic columns
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //Add the dynamic columns to the risk summary datagrid
            //Needs to be added in the OnInit method since they need
            //to be added before viewstate is loaded and load() methods called
            AddDynamicColumns();
        }

        /// <summary>
        /// Loads the control data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			try
			{
				//Add Event handlers
				this.grdRiskSummary.RowCommand += new GridViewCommandEventHandler(grdRiskSummary_RowCommand);
                this.grdRiskSummary.RowDataBound += GrdRiskSummary_RowDataBound;

				//Now load the content
				if (!IsPostBack)
				{
                    if (WebPartVisible)
                    {
                        LoadAndBindData();
                    }
                }
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow as this is loaded by an update panel and can't redirect to error page
				if (this.Message != null)
				{
					this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
					this.Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

        /// <summary>
        /// Adds the risk background color and sets the number of risks per cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdRiskSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Colorize each cell in the row
                for (int i = 0; i < e.Row.Cells.Count; i++)
                {
                    DataControlField field = this.grdRiskSummary.Columns[i];
                    TableCell cell = e.Row.Cells[i];
                    if (field is ButtonFieldEx)
                    {
                        ButtonFieldEx buttonField = (ButtonFieldEx)field;
                        if (buttonField.MetaData == "Probability")
                        {
                            int probabilityId = Int32.Parse(buttonField.CommandName);
                            RiskImpact impact = (RiskImpact)e.Row.DataItem;
                            if (probabilities != null)
                            {
                                RiskProbability probability = probabilities.FirstOrDefault(p => p.RiskProbabilityId == probabilityId);
                                if (probability != null)
                                {
									//Set the color
									if (!String.IsNullOrEmpty(impact.Color) && !String.IsNullOrEmpty(probability.Color))
									{
										cell.BackColor = GlobalFunctions.InterpolateColor2(probability.Score, impact.Score);
										try
										{
											//cell.BackColor = System.Drawing.ColorTranslator.FromHtml($"#{riskMatrix[e.Row.RowIndex, i - 1]}");
										}
										catch (Exception ex)
										{


										}
										

									}
									
		


									//Set the number of risks
									if (this.openRisks != null)
                                    {
                                        int count = this.openRisks.Count(r => r.RiskProbabilityId == probabilityId && r.RiskImpactId == impact.RiskImpactId);
                                        if (count > 0)
                                        {
                                            //Find the button
                                            LinkButton button = (LinkButton)cell.Controls[0];
                                            button.Text = count.ToString();
                                            button.CommandArgument = probabilityId + "," + impact.RiskImpactId;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a handle to the interface
        /// </summary>
        /// <returns>IWebPartReloadable</returns>
        [ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

        /// <summary>
        /// Adds the configurable fields to the list of columns
        /// </summary>
        private void AddDynamicColumns()
        {
            if (ProjectTemplateId > 0)
            {
                //We need to dynamically add the risk probabilities as columns to the Risk Summary table
                //We sort them in order of increasing score, not position, so need to resort first
                RiskManager riskManager = new RiskManager();
                probabilities = riskManager.RiskProbability_Retrieve(ProjectTemplateId).OrderBy(p => p.Score).ThenBy(p => p.RiskProbabilityId).ToList();
                for (int i = 0; i < probabilities.Count; i++)
                {
                    Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx buttonColumn = new Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx();
                    buttonColumn.CommandName = probabilities[i].RiskProbabilityId.ToString();
                    buttonColumn.CommandArgumentField = "RiskProbabilityId";
                    buttonColumn.ButtonType = ButtonType.Link;
                    buttonColumn.SubHeaderText = Microsoft.Security.Application.Encoder.HtmlEncode(probabilities[i].Name);
                    buttonColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                    buttonColumn.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
                    buttonColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                    buttonColumn.ItemStyle.CssClass = "risk-summary-content";
                    buttonColumn.MetaData = "Probability";
                    if (i == 0)
                    {
                        buttonColumn.HeaderColumnSpan = probabilities.Count;
                        buttonColumn.HeaderText = Resources.Fields.RiskProbabilityId;
                    }
                    else
                    {
                        buttonColumn.HeaderColumnSpan = -1;
                    }
                    this.grdRiskSummary.Columns.Add(buttonColumn);
                }
            }
        }

        /// <summary>
        /// Loads the control data
        /// </summary>
        public void LoadAndBindData()
		{
			int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);

            //Convert old -1 saved releases to null
            if (releaseId.HasValue && releaseId < 1)
            {
                releaseId = null;
            }

            //Get the list of risks impacts, sorted by score descending then id
			Business.RiskManager riskManager = new Business.RiskManager();
            List<RiskImpact> impacts = riskManager.RiskImpact_Retrieve(ProjectTemplateId).OrderByDescending(p => p.Score).ThenBy(p => p.RiskImpactId).ToList();

            //Get the list of open risks in the project/release
            Hashtable filters = new Hashtable();
            filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllOpen);
            if (releaseId.HasValue)
            {
                filters.Add("ReleaseId", releaseId.Value);
            }
            openRisks = riskManager.Risk_Retrieve(ProjectId, "RiskId", true, 1, Int32.MaxValue, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());

            //Bind the grid
			this.grdRiskSummary.DataSource = impacts;
			this.grdRiskSummary.DataBind();
		}

		/// <summary>
		/// This event handler handles click-events from the risks summary datagrid
		/// </summary>
        /// <param name="source">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void grdRiskSummary_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = "grdRiskSummary_RowCommand";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);

			//We need to redirect to the risks list, filtered by where in the table the
			//link was clicked (importance vs status)
			ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS);
			filters.Clear();
			//Get the probability and impact
            if (((string)e.CommandArgument).IndexOf(",") >= 0)
            {
                string[] ids = ((string)e.CommandArgument).Split(',');
                int probabilityId = Int32.Parse(ids[0]);
                int impactId = Int32.Parse(ids[1]);
                filters.Add("RiskProbabilityId", probabilityId);
                filters.Add("RiskImpactId", impactId);
                filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllOpen);
                if (releaseId.HasValue && releaseId > 0)
                {
                    filters.Add("ReleaseId", releaseId.Value);
                }
                filters.Save();
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId, 0), true);
            }

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
