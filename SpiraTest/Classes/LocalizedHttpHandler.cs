using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Globalization;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System.Web.SessionState;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// The base class for all HTTP Handlers that need to display localized text
    /// </summary>
    public abstract class LocalizedHttpHandler : IHttpHandler, IRequiresSessionState
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.LocalizedHttpHandler::";

        /// <summary>
        /// Sets the thread culture based on the server settings and optionally the user's culture passed in through the querystring
        /// </summary>
        /// <param name="context"></param>
        public virtual void ProcessRequest(HttpContext context)
        {
            const string METHOD_NAME = "ProcessRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Set the culture appropriately
            try
            {
                //See if we have a system-wide culture set
                if (!String.IsNullOrEmpty(ConfigurationSettings.Default.Globalization_DefaultCulture))
                {
                    if (Thread.CurrentThread.CurrentCulture.Name != ConfigurationSettings.Default.Globalization_DefaultCulture)
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo(ConfigurationSettings.Default.Globalization_DefaultCulture);
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(ConfigurationSettings.Default.Globalization_DefaultCulture);
                    }
                }

                //See if we have a user-specified culture
                if (!string.IsNullOrEmpty(context.Request.QueryString["cultureName"]))
                {
                    string cultureName = context.Request.QueryString["cultureName"];
                    if (Thread.CurrentThread.CurrentCulture.Name != cultureName)
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
                    }
                }

                //Now try and set the project id in session
                if (!String.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]))
                {
                    int intValue;
                    if (Int32.TryParse(context.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID], out intValue))
                    {
                        this.projectId = intValue;
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();

                //Display the error message on the screen
                context.Response.Write(exception.Message);
            }
        }

        public abstract bool IsReusable { get; }

        /// <summary>
        /// Returns the currently logged-in user
        /// </summary>
        public MembershipUser CurrentUser
        {
            get
            {
                return Membership.GetUser();
            }
        }


        /// <summary>
        /// Returns the ID of the currently logged-in user
        /// </summary>
        public int? CurrentUserId
        {
            get
            {
                MembershipUser user = Membership.GetUser();
                if (user != null)
                {
                    return (int)user.ProviderUserKey;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the name of the currently logged-in user
        /// </summary>
        public string CurrentUserName
        {
            get
            {
                MembershipUser user = Membership.GetUser();
                if (user != null)
                {
                    return user.UserName;
                }
                return null;
            }
        }

        /// <summary>
        /// Contains the id of the current project
        /// </summary>
        public int ProjectId
        {
            get
            {
                return this.projectId;
            }
        }
        private int projectId = -1;
    }
}