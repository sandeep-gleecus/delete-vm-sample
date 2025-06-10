using System;
using System.Linq;
using System.Collections.Generic;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.PortfolioHome
{
    public partial class PortfolioOverview : WebPartBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.PortfolioHome.PortfolioOverview::";

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
        /// Loads the control data
        /// </summary>
        protected void LoadAndBindData()
        {
            try
            {
                //Get the portfolio description
                PortfolioManager portfolioManager = new PortfolioManager();
                Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(PortfolioId);

                if (portfolio != null)
                {
                    //Populate the portfolio overview details
                    if (String.IsNullOrEmpty(portfolio.Description))
                    {
                        this.lblPortfolioDescription.Text = "";
                    }
                    else
                    {
                        this.lblPortfolioDescription.Text = portfolio.Description;
                    }
                }
                else
                {
                    //Portfolio no longer exists to redirect to the My Page
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The portfolio you selected has been deleted from the system.", true);
                }
            }
            catch (ArtifactNotExistsException)
            {
                //Portfolio no longer exists to redirect to the My Page
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "The portfolio you selected has been deleted from the system.", true);
            }
        }
    }
}