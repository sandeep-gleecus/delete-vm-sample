using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Text;
using System.Linq;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using System.Threading;
using Inflectra.SpiraTest.DataModel;
using System.Xml.Serialization;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This handler is responsible to displaying the
    /// reports that have been previously generated
    /// </summary>
    public class ReportGeneratedViewer : LocalizedHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ReportGeneratedViewer::";

        protected HttpContext context;

        #region Handler interface

        /// <summary>
        /// Renders out the report
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessRequest(HttpContext context)
        {
            const string METHOD_NAME = "ProcessRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Store in a member variable to avoid having to pass it around
            this.context = context;

            //Increase the timeout so that we can render large reports
            context.Server.ScriptTimeout = 1800; // 30 minutes

            //Call the base class functionality
            base.ProcessRequest(context);

            //Next get the report generated id
            if (String.IsNullOrEmpty(context.Request.QueryString[GlobalFunctions.PARAMETER_REPORT_GENERATED_ID]))
            {
                context.Response.Write(Resources.Messages.ReportViewer_NeedGeneratedId);
                return;
            }
            string reportGeneratedIdString = context.Request.QueryString[GlobalFunctions.PARAMETER_REPORT_GENERATED_ID];
            int reportGeneratedId;
            if (!Int32.TryParse(reportGeneratedIdString, out reportGeneratedId))
            {
                context.Response.Write(Resources.Messages.ReportViewer_InvalidGeneratedId);
                return;
            }

            try
            {
                //Load the report generated record and make sure the user's match (for security)
                ReportManager reportManager = new ReportManager();
                ReportGenerated reportGenerated = reportManager.ReportGenerated_RetrieveById(reportGeneratedId);

                //Make sure we have a report
                if (reportGenerated == null)
                {
                    context.Response.Write(Resources.Messages.ReportViewer_GeneratedReportNotExists);
                    return;
                }

                //Make sure the user matches
                if (!this.CurrentUserId.HasValue || this.CurrentUserId.Value != reportGenerated.UserId)
                {
                    context.Response.Write(Resources.Messages.ReportViewer_NotAuthorizedToViewGeneratedReport);
                    return;
                }

                //Next load the report format
                ReportFormat reportFormat = reportGenerated.Format;
                string reportFormatToken = reportFormat.Token;
                string contentType = reportFormat.ContentType;
                string contentDisposition = reportFormat.ContentDisposition;

                //Finally we need to render the report
                RenderReport(reportGenerated.ReportGeneratedId, contentType, contentDisposition);
            }
            catch (Exception exception)
            {
                //The report doesn't exist, so just display that message and end
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                context.Response.Write(exception.Message);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        public override bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Renders out the XML report with the appropriate headers and mime-types
        /// </summary>
        /// <param name="reportGeneratedId">The report generated id</param>
        /// <param name="contentType">The mime-type header</param>
        /// <param name="contentDisposition">The content disposition header (optional)</param>
        protected void RenderReport(int reportGeneratedId, string contentType, string contentDisposition)
        {
            if (!String.IsNullOrEmpty(contentDisposition))
            {
                //PDFs are a binary XSL-FO format so cannot get report as text
                if (contentDisposition == ReportManager.REPORT_CONTENT_DISPOSITION_XSL_FO)
                {
                    //Specify the MIME type
                    context.Response.AppendHeader("Content-Type", contentType);
                    byte[] pdfData = new ReportManager().GetReportData(reportGeneratedId);
                    context.Response.BinaryWrite(pdfData);
                    return;
                }
                else
                {
                    //Some installations may have the wrong file extension for Excel2007 reports
                    if (contentDisposition == "attachment; filename=Report.xlsx")
                    {
                        contentDisposition = "attachment; filename=Report.xls";
                    }
                    context.Response.AppendHeader("Content-disposition", contentDisposition);
                }
            }

            //We need to get the text of the report
            string reportText = new ReportManager().GetReportText(reportGeneratedId);
            context.Response.AppendHeader("Content-Type", contentType);
            context.Response.Write(reportText);
        }

        #endregion
    }
}