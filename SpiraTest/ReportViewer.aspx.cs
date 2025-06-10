using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Specialized;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Displays the users report on a background thread (using the AJAX Background Manager)
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Reports, null, "Reports-Center")]
    public partial class ReportViewer : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ReportViewer::";

        protected int reportId;
        private string queryString;

        #region Properties

        /// <summary>
        /// Returns a quote-safe version of the QueryString
        /// </summary>
        protected string QueryString
        {
            get
            {
                return GlobalFunctions.JSEncode(this.queryString);
            }
        }

        /// <summary>
        /// Returns the template for the report generated URL
        /// </summary>
        protected string ReportGeneratedUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, ProjectId, -2, "Generated"));
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Next get the report and the format we're trying to create
                if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_REPORT_ID]))
                {
                    this.lblMessage.Text = "You need to supply a report id.";
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }
                string reportIdString = Request.QueryString[GlobalFunctions.PARAMETER_REPORT_ID];
                if (!Int32.TryParse(reportIdString, out this.reportId))
                {
                    //See if we have a report id consisting of two numbers separated by commas
                    //This can happen if we have old, legacy saved reports that include the report id in the querystring suffix
                    //the new ones that use the UrlRewriting no longer need to provide these
                    if (reportIdString.Contains(","))
                    {
                        string[] reportIdParts = reportIdString.Split(',');
                        if (!Int32.TryParse(reportIdParts[0], out this.reportId))
                        {
                            this.lblMessage.Text = Resources.Messages.ReportViewer_ReportIdNotValid;
                            this.lblMessage.Type = MessageBox.MessageType.Error;
                            return;
                        }
                    }
                    else
                    {
                        this.lblMessage.Text = Resources.Messages.ReportViewer_ReportIdNotValid;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;
                    }
                }

                if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_REPORT_FORMAT_ID]))
                {
                    this.lblMessage.Text = Resources.Messages.ReportViewer_ReportFormatIdMissing;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }

                //Load the report entity that contains the available formats and elements
                ReportManager reportManager = new ReportManager();
                Report report = reportManager.RetrieveById(reportId);
                string reportName = report.Name;

                //Display the report title
                ((MasterPages.Main)(this.Master)).TstGlobalNavigation.BreadcrumbText = reportName;
                ((MasterPages.Main)(this.Master)).PageTitle = reportName + " " + Resources.Fields.Report;
                this.ltrReportTitle.Text = Microsoft.Security.Application.Encoder.HtmlEncode(reportName + " " + Resources.Fields.Report);

                //Add the client event handler to the background task process
                Dictionary<string, string> handlers = new Dictionary<string, string>();
                handlers.Add("succeeded", "ajxBackgroundProcessManager_success");
                handlers.Add("init", "ajxBackgroundProcessManager_init");
                this.ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

                //Store the querystring since that will be provided to the background service
                this.queryString = Request.Url.Query;

                //Display the link back to the reporting configuration screen. We include all the sections, elements, filters
                //and sorts so the user doesn't have to re-enter them again
                NameValueCollection configurationQuery = HttpUtility.ParseQueryString(queryString);
                //Remove the project id and report id if specified
                if (configurationQuery["projectId"] != null)
                {
                    configurationQuery.Remove("projectId");
                }
                if (configurationQuery["reportId"] != null)
                {
                    configurationQuery.Remove("reportId");
                }
                this.lnkBackToReportConfiguration.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, ProjectId, this.reportId, "Configure") + "?" + configurationQuery + "&" + GlobalFunctions.PARAMETER_REPORT_CONFIGURATION_SPECIFIED + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
            }
            catch (ArtifactNotExistsException)
            {
                this.lblMessage.Text = Resources.Messages.ReportConfiguration_ReportIDNotExists;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }
}
