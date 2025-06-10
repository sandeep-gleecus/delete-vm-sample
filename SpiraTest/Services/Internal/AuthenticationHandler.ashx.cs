using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.Internal
{
    /// <summary>
    /// Accepts a username and password and sends back the HTTP authentication ticket/cookie
    /// </summary>
    /// <remarks>
    /// Used by functional/performance tests to login quietly without using the main Login page.
    /// Should *NOT* be deployed with the MSI installer or hosted sites
    /// </remarks>
    public class AuthenticationHandler : IHttpHandler
    {

        /// <summary>
        /// Processes the login/password which should be sent as a FORM POST
        /// username=X
        /// password=X
        /// persistent=[0|1]
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form["username"] != null && context.Request.Form["password"] != null)
            {
                string username = context.Request.Form["username"];
                string password = context.Request.Form["password"];
                bool persistentCookie = false;
                if (context.Request.Form["persistent"] != null)
                {
                    string persistent = context.Request.Form["persistent"];
                    persistentCookie = (persistent == "1");
                }

                //Authenticate
                bool success = Membership.ValidateUser(username, password);
                if (success)
                {
                    FormsAuthentication.SetAuthCookie(username, persistentCookie);
                }
                context.Response.Write((success) ? "Success" : "Failed");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}