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
    /// Redirects to either the Release List page, Release Gantt chart, or the Release Map view
    /// </summary>
    public class ReleaseSelector : LocalizedHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ReleaseSelector::";

        /// <summary>
        /// Redirects to the appropriate project home page
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessRequest(HttpContext context)
        {
            //Call the base class functionality
            base.ProcessRequest(context);

            const string METHOD_NAME = "ProcessRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Get any error message
                string queryStringSuffix = "";
                if (!String.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]))
                {
                    queryStringSuffix = "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + context.Server.UrlEncode(context.Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE].Trim());
                }

                //See which page they last used (Tree/Map/Gantt), SpiraTest doesn't support the Map or Gantt views
                if (CurrentUserId.HasValue && ProjectId > 0)
                {
                    ProjectSettingsCollection projectSettingsCollection = new ProjectSettingsCollection(ProjectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS);
                    projectSettingsCollection.Restore();
                    if (projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE] != null && ((string)projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE]) == "Map" && License.LicenseProductName != LicenseProductNameEnum.SpiraTest)
                    {
                        //Redirect to Mind Map (-8)
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -8) + queryStringSuffix, true);
                    }
                    else if (projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE] != null && ((string)projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE]) == "Gantt" && License.LicenseProductName != LicenseProductNameEnum.SpiraTest)
                    {
                        //Redirect to Gantt (-9)
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -9) + queryStringSuffix, true);
                    }
                    else
                    {
                        //Redirect to Tree (-6)
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -6) + queryStringSuffix, true);
                    }
                }
                else
                {
                    //Redirect to My Page
                    context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage) + queryStringSuffix, true);
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