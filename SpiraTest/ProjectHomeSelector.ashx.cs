using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Summary description for ProjectHomeSelector
    /// </summary>
    public class ProjectHomeSelector : LocalizedHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ProjectHomeSelector::";

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

                //See which home page they last used (General/Dev/Test)
                if (CurrentUserId.HasValue && ProjectId > 0)
                {
                    ProjectSettingsCollection projectSettingsCollection = new ProjectSettingsCollection(ProjectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS);
                    projectSettingsCollection.Restore();
                    if (projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE] != null && projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE] is string)
                    {
                        //Add on the URL suffix (dev/test)
                        string tabName = (string)projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE];
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId, 0, tabName) + queryStringSuffix, true);
                    }
                    else
                    {
                        //Redirect to the general home page
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, ProjectId, 0, "General") + queryStringSuffix, true);
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
