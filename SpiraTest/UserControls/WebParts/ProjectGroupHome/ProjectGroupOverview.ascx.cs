using System;
using System.Linq;
using System.Collections.Generic;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome
{
	/// <summary>
	/// Displays the project group overview information and list of projects in the group
	/// </summary>
	public partial class ProjectGroupOverview : WebPartBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.ProjectGroupOverview::";

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
                //Register event handlers
                this.btnPortfolio.Click += new EventHandler(btnPortfolio_Click);

                //Now load the content
                if (!IsPostBack)
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
		/// Called when the project group link is clicked on
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnPortfolio_Click(object sender, EventArgs e)
        {
            //First get the portfolio
            try
            {
                ProjectGroupManager projectGroupManager = new ProjectGroupManager();
                ProjectGroup projectGroup = projectGroupManager.RetrieveById(ProjectGroupId);
                int? portfolioId = projectGroup.PortfolioId;

                if (portfolioId.HasValue)
                {
                    //Now redirect to that portfolio home page
                    Response.Redirect(UrlRewriterModule.RetrievePortfolioRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.PortfolioHome, portfolioId.Value), true);
                }
            }
            catch (ArtifactNotExistsException)
            {
                //The portfolio no longer exists, so redirect to the my page
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The portfolio you selected is not available in the system (it may be inactive or deleted).", true);
            }
        }

        /// <summary>
        /// Loads the control data
        /// </summary>
        protected void LoadAndBindData()
		{
			try
			{
                //Get the project group id
                int projectGroupId = ProjectGroupId;

				//Get the project group description and list of owners
				ProjectGroupManager projectGroupManager = new ProjectGroupManager();
				ProjectGroup projectGroup = projectGroupManager.RetrieveById(projectGroupId, true);

				//Filter out non-owner roles
                List<ProjectGroupUser> projectGroupOwnerUsers = projectGroup.Users.Where(p => p.ProjectGroupRoleId == (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner).ToList();
				this.rptProjectGroupOwners.DataSource = projectGroupOwnerUsers;
				this.rptProjectGroupOwners.DataBind();

                //Populate the Portfolio information
                //Make sure we're authorized to view portfolios / the relevant row
                if (Common.Global.Feature_Portfolios)
                {
                    int? portfolioId = projectGroup.PortfolioId;
                    //Make sure we have a portfolio for this program
                    if (portfolioId.HasValue)
                    {
                        PortfolioManager portfolioManager = new PortfolioManager();
                        Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId.Value);
                        this.rowPortfolio.Visible = true;
                        this.btnPortfolio.Text = Microsoft.Security.Application.Encoder.HtmlEncode(portfolio.Name);

                        //See if the hyperlink to the portfolio dashboard should be active or not
                        this.btnPortfolio.Enabled = UserIsPortfolioViewer;
                    }
                }

                //Populate the project group overview details
                if (String.IsNullOrEmpty(projectGroup.Description))
				{
					this.lblProjectGroupDescription.Text = "";
				}
				else
				{
					this.lblProjectGroupDescription.Text = projectGroup.Description;
				}
				if (String.IsNullOrEmpty(projectGroup.Website))
				{
					this.lnkProjectGroupWebsite.Text = "";
					this.lnkProjectGroupWebsite.NavigateUrl = "";
				}
				else
				{
					this.lnkProjectGroupWebsite.Text = Microsoft.Security.Application.Encoder.HtmlEncode(projectGroup.Website);
					this.lnkProjectGroupWebsite.NavigateUrl = GlobalFunctions.FormNavigatableUrl(projectGroup.Website);
				}
			}
			catch (ArtifactNotExistsException)
			{
				//Project Group no longer exists to redirect to the My Page
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The project group you selected has been deleted from the system.", true);
			}
		}
	}
}
