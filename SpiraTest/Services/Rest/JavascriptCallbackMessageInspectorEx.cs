using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.ServiceModel.Web;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// Based on the standard WCF JavascriptCallbackMessageInspector in .NET 4.0, but allows the service to be called
    /// by authenticated users to make communication with the application easier.
    /// </summary>
    public class JavascriptCallbackMessageInspectorEx : IDispatchMessageInspector
    {
        //Fields
        internal static readonly string applicationJavaScriptMediaType = "application/x-javascript";

        // Properties
        private string CallbackParameterName { get; set; }

        // Methods
        public JavascriptCallbackMessageInspectorEx(string callbackParameterName)
        {
            this.CallbackParameterName = callbackParameterName;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            //We allow authentication users to access the service
            //if (((HttpContext.Current != null) && (HttpContext.Current.User != null)) && ((HttpContext.Current.User.Identity != null) && HttpContext.Current.User.Identity.IsAuthenticated))
            //{
            //    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.CrossDomainJavascriptAuthNotSupported));
            //}
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            WebBodyFormatMessageProperty property;
            JavascriptCallbackResponseMessageProperty property2 = null;
            if ((reply.Properties.TryGetValue<WebBodyFormatMessageProperty>("WebBodyFormatMessageProperty", out property) && (property != null)) && (property.Format == WebContentFormat.Json))
            {
                HttpResponseMessageProperty property3;
                if (!reply.Properties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out property2) || (property2 == null))
                {
                    property2 = JavascriptCallbackMessageInspectorExUtilities.TrySetupJavascriptCallback(this.CallbackParameterName);
                    if (property2 != null)
                    {
                        reply.Properties.Add(JavascriptCallbackResponseMessageProperty.Name, property2);
                    }
                }
                if (((property2 != null) && reply.Properties.TryGetValue<HttpResponseMessageProperty>(HttpResponseMessageProperty.Name, out property3)) && (property3 != null))
                {
                    property3.Headers[HttpResponseHeader.ContentType] = applicationJavaScriptMediaType;
                    if (!property2.StatusCode.HasValue)
                    {
                        property2.StatusCode = new HttpStatusCode?(property3.StatusCode);
                    }
                    property3.StatusCode = HttpStatusCode.OK;
                    if (property3.SuppressEntityBody)
                    {
                        property3.SuppressEntityBody = false;
                        Message message = WebOperationContext.Current.CreateJsonResponse<object>(null);
                        message.Properties.CopyProperties(reply.Properties);
                        reply = message;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Provides some helper static classes that the main class needs
    /// </summary>
    internal static class JavascriptCallbackMessageInspectorExUtilities
    {
        internal static bool TryGetValue<TProperty>(this MessageProperties messageProperties, string name, out TProperty property)
        {
            object obj2;
            if (messageProperties.TryGetValue(name, out obj2))
            {
                property = (TProperty)obj2;
                return true;
            }
            property = default(TProperty);
            return false;
        }

        internal static JavascriptCallbackResponseMessageProperty TrySetupJavascriptCallback(string callbackParameterName)
        {
            JavascriptCallbackResponseMessageProperty property = null;
            if (!string.IsNullOrEmpty(callbackParameterName) && !OperationContext.Current.OutgoingMessageProperties.TryGetValue<JavascriptCallbackResponseMessageProperty>(JavascriptCallbackResponseMessageProperty.Name, out property))
            {
                UriTemplateMatch uriTemplateMatch = WebOperationContext.Current.IncomingRequest.UriTemplateMatch;
                if ((uriTemplateMatch != null) && uriTemplateMatch.QueryParameters.AllKeys.Contains<string>(callbackParameterName))
                {
                    string str = uriTemplateMatch.QueryParameters[callbackParameterName];
                    if (!string.IsNullOrEmpty(str))
                    {
                        JavascriptCallbackResponseMessageProperty property2 = new JavascriptCallbackResponseMessageProperty();
                        property2.CallbackFunctionName = str;
                        property = property2;
                        OperationContext.Current.OutgoingMessageProperties.Add(JavascriptCallbackResponseMessageProperty.Name, property);
                    }
                }
            }
            return property;
        }
    }
}