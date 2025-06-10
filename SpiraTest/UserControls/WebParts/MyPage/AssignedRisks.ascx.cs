using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
	public partial class AssignedRisks : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.AssignedRisks::";

		#region User Configurable Properties

		/// <summary>
		/// Stores how many rows of data to display, default is unlimited
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
		DefaultValue(10)
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
		protected int rowsToDisplay = 10;

		#endregion

		/// <summary>Loads the control data</summary>
		/// <param name="sender">Page</param>
		/// <param name="e">EventArgs</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			try
			{
                //Set the RSS Feed link if it's enabled
                if (!String.IsNullOrEmpty(UserRssToken))
                {
                    string rssUrl = this.ResolveUrl("~/Feeds/" + UserId + "/" + UserRssToken + "/AssignedRisks.aspx");
                    this.Subtitle = GlobalFunctions.WEBPART_SUBTITLE_RSS + rssUrl;
                }

				//Register event handlers
				grdOwnedRisks.RowDataBound += new GridViewRowEventHandler(grdOwnedRisks_RowDataBound);

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
		/// Loads and binds the data in the control
		/// </summary>
		public void LoadAndBindData()
		{
			//Get the current project filter (if any)
			Nullable<int> filterProjectId = null;
			if (GetUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false) && ProjectId > 0)
            {
                filterProjectId = ProjectId;
                //Display the release field
                this.grdOwnedRisks.Columns[2].Visible = false;
                this.grdOwnedRisks.Columns[3].Visible = true;
            }
            else
            {
                //Display the project field
                this.grdOwnedRisks.Columns[2].Visible = true;
                this.grdOwnedRisks.Columns[3].Visible = false;
            }

			//Now get the list of open risks owned by the user in order of risk type
			RiskManager riskManager = new RiskManager();
			List<RiskView> ownedRisks = riskManager.Risk_RetrieveOpenByOwnerId(UserId, filterProjectId, null, this.rowsToDisplay);

            //Set the base url for the Requirement URL field so that we get a HyperLink and not a LinkButton
            //The actual URL will be set during databinding
            ((NameDescriptionFieldEx)this.grdOwnedRisks.Columns[1]).NavigateUrlFormat = "{0}";

            this.grdOwnedRisks.DataSource = ownedRisks;
			this.grdOwnedRisks.DataBind();
		}

		/// <summary>
		/// This event handler applies any conditional formatting to the datagrid before display
		/// </summary>
		/// <param name="sender">The object that raised the event</param>
		/// <param name="e">The parameters passed to handler</param>
		private void grdOwnedRisks_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Don't touch headers, footers or subheaders
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                //Get the data item
                RiskView risk = (RiskView)(e.Row.DataItem);

				//First lets handle the color of the exposure column
                if (risk.RiskExposure.HasValue && !String.IsNullOrEmpty(risk.RiskProbabilityColor) && !String.IsNullOrEmpty(risk.RiskImpactColor))
				{
					Color backColor = GlobalFunctions.InterpolateColor2(risk.RiskProbabilityScore.HasValue ? risk.RiskProbabilityScore.Value : 1, risk.RiskImpactScore.HasValue ? risk.RiskImpactScore.Value : 1);
					e.Row.Cells[5].BackColor = backColor;
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


                //Need to set the actual URL of the HyperLink
                HyperLinkEx hyperlink = (HyperLinkEx)e.Row.Cells[1].Controls[0];
                if (hyperlink != null)
                {
                    hyperlink.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, risk.ProjectId, risk.RiskId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
                }
			}
		}
	}
}
