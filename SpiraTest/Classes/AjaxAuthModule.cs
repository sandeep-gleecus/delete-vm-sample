using System;
using System.Web;
using System.Text;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Overrides the standard handling of missing authentication cookies when an AJAX JSON web service is called
    /// to return a specific error message so that the client code knows
    /// to redirect to the login page rather than displaying an error.
    /// </summary>
    /// <remarks>
    /// This only affects secured web services used by AJAX not the regular API ones that don't required an authentication cookie
    /// </remarks>
    public class AjaxAuthModule : IHttpModule
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Classes.AjaxAuthModule::";

        public const string AUTHENTICATION_MESSAGE = "Authentication failed.";  //Do not localize
        public const string AUTHORIZATION_MESSAGE = "Authorization failed"; //Do not localize

        #region IHttpModule Implementation

        public void Init(System.Web.HttpApplication context)
        {
            if (context == null)
            {
                throw new System.ArgumentNullException("context");
            }

            context.EndRequest += OnEndRequest;
        }

        public void Dispose()
        {
            //There are no resources to release
        }

        #endregion

        #region Private Methods

        private void OnEndRequest(object sender, EventArgs e)
        {
            Logger.LogTraceEvent(CLASS_NAME + "OnEndRequest", System.Web.HttpContext.Current.Request.FilePath.ToLowerInvariant() + "," + HttpContext.Current.Request.ContentType);
            //if this is a web service call with no auth cookie, send a different response than the generic error so that we can handle it appropriately 
            //NOTE: we're returning a matching json object that WCF is already configured to return
            //Need to make sure we have /Services/Ajax in the path to avoid interfering with REST API services
            if (System.IO.Path.GetExtension(System.Web.HttpContext.Current.Request.FilePath.ToLowerInvariant()) == ".svc" &&
                HttpContext.Current.Request.Cookies.Get(System.Web.Security.FormsAuthentication.FormsCookieName) == null &&
                HttpContext.Current.Request.Path.Contains("/Services/Ajax") &&
                HttpContext.Current.Request.ContentType.Contains("application/json")
                )
            {
                WriteAccessDeniedExceptionJsonString(HttpContext.Current);
            }
        }

        /// <summary>
        /// Writes the JSON message the client components know means force a logout and re-authenticate
        /// </summary>
        /// <param name="context"></param>
        private static void WriteAccessDeniedExceptionJsonString(HttpContext context)
        {
            Logger.LogTraceEvent(CLASS_NAME + "WriteAccessDeniedExceptionJsonString", "Sending 'Authentication failed' response");
            context.Response.ClearHeaders();
            context.Response.ClearContent();
            context.Response.Clear();
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription(500);
            context.Response.ContentType = "application/json";
            context.Response.AddHeader("jsonerror", "true");
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(context.Response.OutputStream, new UTF8Encoding(false)))
            {
                //Do not localize this Message as it's checked on the client end
                writer.Write(new WebServiceError(AUTHENTICATION_MESSAGE, string.Empty, string.Empty).ToJson());
                writer.Flush();
            }
        }

        #endregion

        #region Auth Cookie missing on Web Service request response

        //this class is used to replicate the response from the .NET framework when an json web service call 
        //is denied because of missing auth ticket 
        private class WebServiceError
        {

            #region Declarations

            private const string JsonTemplate = "{{\"Message\":\"{0}\",\"StackTrace\":\"{1}\",\"ExceptionType\":\"{2}\"}}";
            private string _exceptionType;
            private string _message;
            private string _stackTrace;

            #endregion

            #region Constructors

            public WebServiceError(string msg, string stack, string type)
            {
                _message = msg;
                _stackTrace = stack;
                _exceptionType = type;
            }

            #endregion

            #region Public Methods

            public string ToJson()
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, JsonTemplate, _message, _stackTrace, _exceptionType);
            }
            #endregion
        }

        #endregion
    }
}