using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Threading;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// Launches a saved report from the passed in saved report id
	/// </summary>
	/// <remarks>
	/// Replaces the use of LinkButtons and postbacks that made it hard to share report links
	/// </remarks>
	public class ReportDisplaySaved : LocalizedHttpHandler
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ReportDisplaySaved::";

		/// <summary>
		/// Redirects to the appropriate saved report
		/// </summary>
		/// <param name="context">The current HTTP context</param>
		public override void ProcessRequest(HttpContext context)
		{
			//Call the base class functionality
			base.ProcessRequest(context);

			const string METHOD_NAME = "ProcessRequest";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the theme name if specified
				string themeName = "InflectraTheme";    //Default unless specified
				if (!String.IsNullOrWhiteSpace(context.Request.Params[GlobalFunctions.PARAMETER_THEME_NAME]))
				{
					themeName = context.Request.Params[GlobalFunctions.PARAMETER_THEME_NAME].Trim();
				}

				//Load a particular saved report, put that project in session and redirect to the report viewer
				if (String.IsNullOrEmpty(context.Request.QueryString[GlobalFunctions.PARAMETER_SAVED_REPORT_ID]))
				{
					//Redirect to My Page
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "No saved report id provided");
					context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage), true);
				}
				else
				{
					int reportSavedId;
					if (Int32.TryParse(context.Request.QueryString[GlobalFunctions.PARAMETER_SAVED_REPORT_ID], out reportSavedId))
					{

						//Get the id of the report it's based on so that we can determine the format and project
						//Make sure it is for the current project and that either shared or from the current user
						ReportManager reportManager = new ReportManager();
						SavedReportView savedReport = reportManager.RetrieveSavedById(reportSavedId);
						if (savedReport.ProjectId.HasValue && savedReport.ProjectId == ProjectId && (savedReport.IsShared || savedReport.UserId == CurrentUserId))
						{
							int projectId = savedReport.ProjectId.Value;
							int reportFormatId = savedReport.ReportFormatId;
							string queryStringSuffix = savedReport.Parameters;

							//Add on the theme name
							queryStringSuffix += "&" + GlobalFunctions.PARAMETER_THEME_NAME + "=" + Microsoft.Security.Application.Encoder.UrlEncode(themeName);

							//Now retrieve the report itself to determine the report
							ReportFormat reportFormat = reportManager.RetrieveFormatById(reportFormatId);
							//Finally actually go to the report rendering page
							string viewerUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, projectId, savedReport.ReportId, GlobalFunctions.PARAMETER_TAB_REPORT_VIEWER));
							context.Response.Redirect(viewerUrl + "?" + queryStringSuffix, true);
						}
						else
						{
							//Redirect to My Page
							Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Invalid saved report id provided");
							context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage), true);
						}
					}
					else
					{
						//Redirect to My Page
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Invalid saved report id provided");
						context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage), true);
					}
				}
			}
			catch (ThreadAbortException)
			{
				//Do nothing
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();

				//Redirect to My Page
				context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage), true);
			}
		}

		public override bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}
