using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome
{
	/// <summary>
	/// Displays the list of project risks
	/// </summary>
	public partial class OpenRisks : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.OpenRisks::";

		#region Enumerations

		public enum OpenRiskOrganizeBy
		{
			Priority = 1,
			Severity = 2
		}

		#endregion

		#region User Configurable Properties

		/// <summary>
		/// Stores how many rows of data to display, default is unlimited
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
				this.rowsToDisplay = value;
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
        DefaultValue(false)
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
        protected bool displayOwner = false;

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
				//We have to set the message box programmatically for items that start out in the catalog
				this.MessageBoxId = "lblMessage";

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
            //Get the project group id
            int projectGroupId = ProjectGroupId;

            //Now get the list of open risks in order of descreasing exposure 

			RiskManager riskManager = new RiskManager();
            List<RiskView> openRisks = riskManager.Risk_RetrieveOpenForGroup(projectGroupId, RowsToDisplay);

            //Specify whether to display type and/or owner
            if (this.displayOwner)
            {
                this.grdRiskList.Columns[5].Visible = true;
            }
            else
            {
                this.grdRiskList.Columns[5].Visible = false;
            }
            if (this.displayType)
            {
                this.grdRiskList.Columns[4].Visible = true;
            }
            else
            {
                this.grdRiskList.Columns[4].Visible = false;
            }

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
                RiskView risk = (RiskView)(e.Row.DataItem);

                //First lets handle the color of the exposure column
                if (risk.RiskExposure.HasValue && !String.IsNullOrEmpty(risk.RiskProbabilityColor) && !String.IsNullOrEmpty(risk.RiskImpactColor))
                {
                    Color backColor = GlobalFunctions.InterpolateColor2(risk.RiskProbabilityScore.HasValue ? risk.RiskProbabilityScore.Value : 1, risk.RiskImpactScore.HasValue ? risk.RiskImpactScore.Value : 1);
                    e.Row.Cells[3].BackColor = backColor;
                }

                //If the review-date is in the past, change its css class to indicate this
                //(we only consider the date component not the time component)
                if (risk.ReviewDate.HasValue)
                {
                    if (risk.ReviewDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        e.Row.Cells[6].CssClass = "Warning priority4";
                    }
                }

                //Specify the project id and artifact id in the url
                HyperLinkEx hyperlink = (HyperLinkEx)e.Row.Cells[1].Controls[0];
                if (hyperlink != null)
                {
                    hyperlink.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, risk.ProjectId, risk.RiskId);
                }
			}
		}

	}
}
