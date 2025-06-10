using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Inflectra.SpiraTest.Business;
using System.Collections.Specialized;
using System.Threading;
using System.Security.Principal;
using Inflectra.SpiraTest.Common;
using System.ServiceModel.Web;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using System.Web.Security;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// Custom service authorization manager that lets you provide the login name and API token (the RSS token)
    /// either in the HTTP Header "api-key" or as a custom URL token "?api-key=yyyy"
    /// </summary>
    public class RestServiceAuthorizationManager : ServiceAuthorizationManager
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Rest.RestServiceAuthorizationManager::";

        public const string API_KEY_NAME = "api-key";
        public const string API_USERNAME = "username";
        public const string HEADER_AUTHORIZATION = "Authorization";

        private const string PLUGIN_REST_API = "Rest API";

        /// <summary>
        /// Constructor
        /// </summary>
        public RestServiceAuthorizationManager()
        {
        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            const string METHOD_NAME = "CheckAccessCore";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            bool authenticated = false;

            try
            {
                //If the querystring contains the callback parameter this is a JSONP call, so force the content-type to JSON
                //since JSONP calls cannot specify it in the HTTP Header themselves
                //For now this only works when the callback parameter is called "callback". Ideally we should get the configured
                //callback name from the WebHttpBehavior, but that's unnecessarily complex for now!
                if (HttpContext.Current.Request.QueryString["callback"] != null)
                {
                    HttpContext.Current.Request.Headers.Remove("accept");
                    HttpContext.Current.Request.Headers.Add("accept", "application/json");
                }
                
                //See if they're using the standard BASIC AUTH header
                string basicAuth = HttpContext.Current.Request.Headers[HEADER_AUTHORIZATION];
                if (!String.IsNullOrWhiteSpace(basicAuth) && basicAuth.StartsWith("Basic "))
                {
                    //Decode the header
                    string header = basicAuth.Substring("Basic ".Length);
                    byte[] decodedHeader = Convert.FromBase64String(header);
                    if (decodedHeader != null && decodedHeader.Length > 0)
                    {
                        string[] usernamePassword = new UTF8Encoding().GetString(decodedHeader).Split(':');
                        string username = usernamePassword[0];
                        string apiKey3 = "";
                        if (usernamePassword.Length > 1)
                        {
                            apiKey3 = usernamePassword[1];
                        }

                        if (String.IsNullOrWhiteSpace(username))
                        {
                            authenticated = false;
                        }
                        else
                        {
                            //Validate the user based in username and RSS token combination. Locks the user if retries exceeded
                            //which prevents a brute force attack
                            SpiraMembershipProvider spiraProvider = (SpiraMembershipProvider)Membership.Provider;
                            authenticated = spiraProvider.ValidateUserByRssToken(username, apiKey3);
                            if (authenticated)
                            {
                                User user = spiraProvider.GetProviderUser(username);
                                if (user == null)
                                {
                                    throw new ApplicationException("User object null when checking BASIC header");
                                }

                                //Now need to make sure that we have enough concurrent user licenses
                                if (HttpContext.Current.Session == null)
                                {
                                    throw new ApplicationException("Session object null when checking BASIC header");
                                }
                                else
                                {
                                    Web.Global.RegisterSession(HttpContext.Current.Session.SessionID, user.UserId, PLUGIN_REST_API);
                                }

                                //Make sure we've not exceeded our count of allowed licenses
                                if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
                                {
                                    int concurrentUserCount = Web.Global.ConcurrentUsersCount();
                                    if (concurrentUserCount > Common.License.Number)
                                    {
                                        //Log an error and throw an exception
                                        LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
                                        Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                                        return false;
                                    }
                                }

                                GenericIdentity identity = new GenericIdentity(user.UserId.ToString());
                                Thread.CurrentPrincipal = new GenericPrincipal(identity, new string[] { });
                                operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] = new GenericPrincipal(identity, new string[] { });
                            }
                        }
                    }
                }

                //See if they're using the older custom API header
                if (!authenticated)
                {
                    string apiKey = HttpContext.Current.Request.Headers[API_KEY_NAME];
                    string username3 = HttpContext.Current.Request.Headers[API_USERNAME];
                    if (!String.IsNullOrWhiteSpace(apiKey) && !String.IsNullOrWhiteSpace(username3))
                    {
                        //Validate the user based in username and RSS token combination. Locks the user if retries exceeded
                        //which prevents a brute force attack
                        SpiraMembershipProvider spiraProvider = (SpiraMembershipProvider)Membership.Provider;
                        authenticated = spiraProvider.ValidateUserByRssToken(username3, apiKey);
                        if (authenticated)
                        {
                            User user = spiraProvider.GetProviderUser(username3);
                            if (user == null)
                            {
                                throw new ApplicationException("User object null when checking custom API header");
                            }

                            //Now need to make sure that we have enough concurrent user licenses
                            if (HttpContext.Current.Session == null)
                            {
                                throw new ApplicationException("Session object null when checking custom API header");
                            }
                            else
                            {
                                Web.Global.RegisterSession(HttpContext.Current.Session.SessionID, user.UserId, PLUGIN_REST_API);
                            }

                            //Make sure we've not exceeded our count of allowed licenses
                            if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
                            {
                                int concurrentUserCount = Web.Global.ConcurrentUsersCount();
                                if (concurrentUserCount > Common.License.Number)
                                {
                                    //Log an error and throw an exception
                                    LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
                                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                                    return false;
                                }
                            }

                            GenericIdentity identity = new GenericIdentity(user.UserId.ToString());
                            Thread.CurrentPrincipal = new GenericPrincipal(identity, new string[] { });
                            operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] = new GenericPrincipal(identity, new string[] { });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                return false;
            }

            //See if we have a URL token instead
            if (!authenticated)
            {
                try
                {
                    Uri uri = new Uri(operationContext.IncomingMessageProperties.Via.OriginalString);
                    NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
                    if (!String.IsNullOrWhiteSpace(query[API_KEY_NAME]))
                    {
                        string apiKey2 = query[API_KEY_NAME].Trim();
                        string username2 = query[API_USERNAME];
                        if (String.IsNullOrWhiteSpace(username2))
                        {
                            authenticated = false;
                        }
                        else
                        {
                            username2 = username2.Trim();
                            //Validate the user based in username and RSS token combination. Locks the user if retries exceeded
                            //which prevents a brute force attack
                            SpiraMembershipProvider spiraProvider = (SpiraMembershipProvider)Membership.Provider;
                            authenticated = spiraProvider.ValidateUserByRssToken(username2, apiKey2);
                            if (authenticated)
                            {
                                User user = spiraProvider.GetProviderUser(username2);
                                if (user == null)
                                {
                                    throw new ApplicationException("User object null when checking URL token");
                                }

                                //Now need to make sure that we have enough concurrent user licenses
                                if (HttpContext.Current.Session == null)
                                {
                                    throw new ApplicationException("Session object null when checking URL token");
                                }
                                else
                                {
                                    Web.Global.RegisterSession(HttpContext.Current.Session.SessionID, user.UserId, PLUGIN_REST_API);
                                }

                                //Make sure we've not exceeded our count of allowed licenses
                                if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
                                {
                                    int concurrentUserCount = Web.Global.ConcurrentUsersCount();
                                    if (concurrentUserCount > Common.License.Number)
                                    {
                                        //Log an error and throw an exception
                                        LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
                                        Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                                        return false;
                                    }
                                }

                                GenericIdentity identity = new GenericIdentity(user.UserId.ToString());
                                Thread.CurrentPrincipal = new GenericPrincipal(identity, new string[] { });
                                operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] = new GenericPrincipal(identity, new string[] { });
                            }
                            if (WebOperationContext.Current.IncomingRequest != null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters != null)
                            {
                                WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.Remove(API_USERNAME);
                            }
                        }
                        if (WebOperationContext.Current.IncomingRequest != null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters != null)
                        {
                            WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.Remove(API_KEY_NAME);
                        }
                    }
                }
                catch (UriFormatException)
                {
                    authenticated = false;
                }
                catch (ArgumentNullException)
                {
                    authenticated = false;
                }
                catch (Exception exception)
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                }
            }

            //Return the authentication status
            Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Authenticated=" + authenticated);
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            return authenticated;
        }
    }
}