using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the list of project risks
	/// </summary>
	public partial class OpenRisks : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.OpenRisks::";

		#region Enumerations

		public enum OpenRiskOrganizeBy
		{
			Priority = 1,
			Severity = 2
		}

		#endregion

		#region User Configurable Properties

		/// <summary>
		/// Stores how many rows of data to display, default is 5
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
		DefaultValue(5)
		]
		public int RowsToDisplay
		{
			get
			{
				return this.rowsToDisplay;
			}
			set
			{
                int rowsToDisplayMax = 50;
                this.rowsToDisplay = value < rowsToDisplayMax ? value : rowsToDisplayMax;
                //Force the data to reload
                LoadAndBindData();
			}
		}
		protected int rowsToDisplay = 5;

        /// <summary>
        /// Should we display the type of risk
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("OpenRisks_DisplayType"),
        LocalizedWebDescription("OpenRisks_DisplayType"),
        DefaultValue(true)
        ]
        public bool DisplayType
        {
            get
            {
                return this.displayType;
            }
            set
            {
                this.displayType = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected bool displayType = true;

        /// <summary>
        /// Should we display the owner of the risk
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("OpenRisks_DisplayOwner"),
        LocalizedWebDescription("OpenRisks_DisplayOwner"),
        DefaultValue(true)
        ]
        public bool DisplayOwner
        {
            get
            {
                return this.displayOwner;
            }
            set
            {
                this.displayOwner = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected bool displayOwner = true;

        #endregion

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
				//Add the event handlers
				this.grdRiskList.RowDataBound += new GridViewRowEventHandler(grdRiskList_RowDataBound);

				//Now load the content
                if (WebPartVisible)
                {
                    LoadAndBindData();
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
		/// Returns a handle to the interface
		/// </summary>
		/// <returns>IWebPartReloadable</returns>
		[ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

		/// <summary>
		/// Loads the control data
		/// </summary>
		public void LoadAndBindData()
		{
			//Get the release id from settings
			int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);
            if (releaseId.HasValue && releaseId < 0)
            {
                releaseId = null;
            }

			//Now get the list of open risks in order of descreasing exposure 
			RiskManager riskManager = new RiskManager();
            const string sortProperty = "RiskExposure";
            const bool sortAscending = false;
            Hashtable filters = new Hashtable();
            filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllOpen);
            if (releaseId.HasValue)
            {
                filters.Add("ReleaseId", releaseId.Value);
            }
            List<RiskView> openRisks;
			try
			{
                openRisks = riskManager.Risk_Retrieve(ProjectId, sortProperty, sortAscending, 1, RowsToDisplay, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
			}
			catch (ArtifactNotExistsException)
			{
				//The release no longer exists so reset it and reload
				releaseId = null;
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
                filters["ReleaseId"] = null;
                openRisks = riskManager.Risk_Retrieve(ProjectId, sortProperty, sortAscending, 1, RowsToDisplay, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
            }

            //Specify whether to display type and/or owner
            if (this.displayOwner)
            {
                this.grdRiskList.Columns[4].Visible = true;
            }
            else
            {
                this.grdRiskList.Columns[4].Visible = false;
            }
            if (this.displayType)
            {
                this.grdRiskList.Columns[3].Visible = true;
            }
            else
            {
                this.grdRiskList.Columns[3].Visible = false;
            }

            //Set the navigate url for the name field
            ((NameDescriptionFieldEx)this.grdRiskList.Columns[1]).NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId, -3);

            this.grdRiskList.DataSource = openRisks;
			this.grdRiskList.DataBind();
		}

		/// <summary>
		/// This event handler applies the conditional formatting to the datagrid
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void grdRiskList_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                //First lets handle the color of the exposure column
                RiskView risk = (RiskView)(e.Row.DataItem);
                if (risk.RiskExposure.HasValue && !String.IsNullOrEmpty(risk.RiskProbabilityColor) && !String.IsNullOrEmpty(risk.RiskImpactColor))
				{
					Color backColor = GlobalFunctions.InterpolateColor2(risk.RiskProbabilityScore.HasValue ? risk.RiskProbabilityScore.Value : 1, risk.RiskImpactScore.HasValue ? risk.RiskImpactScore.Value : 1);
					e.Row.Cells[2].BackColor = backColor;
				}

                //If the review-date is in the past, change its css class to indicate this
                //(we only consider the date component not the time component)
                if (risk.ReviewDate.HasValue)
                {
                    if (risk.ReviewDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        e.Row.Cells[5].CssClass = "Warning priority4";
                    }
                }
            }
		}
	}
}
