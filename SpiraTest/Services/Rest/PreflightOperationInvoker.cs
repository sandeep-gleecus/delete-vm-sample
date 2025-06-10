using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Net;
using Inflectra.SpiraTest.Common;
using System.ServiceModel;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// Invokes REST-CORS cross-domain preflight requests
    /// </summary>
    class PreflightOperationInvoker : IOperationInvoker
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Rest.PreflightOperationInvoker";

        private string replyAction;
        List<string> allowedHttpMethods;
        private int parameterCount;

        public PreflightOperationInvoker(string replyAction, List<string> allowedHttpMethods, int parameterCount)
        {
            this.replyAction = replyAction;
            this.allowedHttpMethods = allowedHttpMethods;
            this.parameterCount = parameterCount;
        }

        public object[] AllocateInputs()
        {
            return new object[this.parameterCount];
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            const string METHOD_NAME = "Invoke";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //We can ignore the inputs and just deal with the headers
                MessageProperties incomingMessageProperties = OperationContext.Current.IncomingMessageProperties;
                MessageProperties outgoingMessageProperties = OperationContext.Current.OutgoingMessageProperties;
                outputs = null;
                HandlePreflight(incomingMessageProperties, outgoingMessageProperties);

                //We don't return any body
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("Only synchronous invocation");
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotSupportedException("Only synchronous invocation");
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        void HandlePreflight(MessageProperties incomingMessageProperties, MessageProperties outgoingMessageProperties)
        {
            HttpRequestMessageProperty httpRequest = (HttpRequestMessageProperty)incomingMessageProperties[HttpRequestMessageProperty.Name];
            string origin = httpRequest.Headers[CorsConstants.Origin];
            string requestMethod = httpRequest.Headers[CorsConstants.AccessControlRequestMethod];
            string requestHeaders = httpRequest.Headers[CorsConstants.AccessControlRequestHeaders];

            //Send the list of allowed methods in the output
            HttpResponseMessageProperty httpResponse = new HttpResponseMessageProperty();
            outgoingMessageProperties.Add(HttpResponseMessageProperty.Name, httpResponse);

            httpResponse.SuppressEntityBody = true;
            httpResponse.StatusCode = HttpStatusCode.OK;
            
            //We don't need to add the Access-Control-Allow-Origin header because that was already added by the MessageInspector

            //We do need to reply with the allowed methods
            if (requestMethod != null && this.allowedHttpMethods.Contains(requestMethod))
            {
                httpResponse.Headers.Add(CorsConstants.AccessControlAllowMethods, string.Join(",", this.allowedHttpMethods));
            }

            if (requestHeaders != null)
            {
                httpResponse.Headers.Add(CorsConstants.AccessControlAllowHeaders, requestHeaders);
            }
        }
    } 
}