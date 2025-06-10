using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Inflectra.SpiraTest.Common;
using System.Threading;
using System.Globalization;
using System.Xml;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Services.Utils
{
    /// <summary>
    /// Checks that the XSRF token in the header and cookie match, otherwise it rejects thee request
    /// </summary>
    public class AjaxXsrfValidatingBehavior : IEndpointBehavior
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public AjaxXsrfValidatingBehavior()
        {
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        #region IEndpointBehavior

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            //Scan the incoming message for the header
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new AjaxXsrfMessageInspector());
        }


        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }

     /// <summary>
    /// Checks the CSRF token in the cookie and request headers to make sure they match (if header specified)
    /// </summary>
    public class AjaxXsrfMessageInspector : IDispatchMessageInspector
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Utils.AjaxXsrfMessageInspector::";

        /// <summary>
        /// Default constructor
        /// </summary>
        public AjaxXsrfMessageInspector()
        {
        }

        public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            const string METHOD_NAME = "AfterReceiveRequest";

            bool isValid = false;

            //See if we can get the HTTP headers, and see if it contains the special XSRF token
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (!string.IsNullOrEmpty(httpRequestMessage.Headers[GlobalFunctions.AntiXsrfHeader]))
                {
                    string headerToken = httpRequestMessage.Headers[GlobalFunctions.AntiXsrfHeader];
                    if (!String.IsNullOrEmpty(headerToken) && HttpContext.Current != null && HttpContext.Current.Request != null)
                    {
                        //Compare the token with the current cookie
                        HttpRequest httpRequest = HttpContext.Current.Request;
                        HttpCookie cookie = httpRequest.Cookies[GlobalFunctions.AntiXsrfTokenKey];
                        if (cookie != null)
                        {
                            string cookieToken = cookie.Value;
                            if (cookieToken == headerToken)
                            {
                                isValid = true;
                            }
                        }
                    }
                }
            }

            if (!isValid)
            {
                //Token is not valid, we need to abort the request
                Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.MasterPage_ValidationOfXsrfTokenFailed);
                throw new XsrfViolationException(Resources.Messages.MasterPage_ValidationOfXsrfTokenFailed);
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}