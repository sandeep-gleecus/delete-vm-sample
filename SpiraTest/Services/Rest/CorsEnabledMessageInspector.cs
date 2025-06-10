using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// This allows Spira to handle cross-domain requests using the CORS convention within WCF
    /// Code was adapted from https://code.msdn.microsoft.com/Implementing-CORS-support-c1f9cd4b/
    /// </summary>
    public class CorsEnabledMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            HttpRequestMessageProperty httpProp = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
            string origin = httpProp.Headers[CorsConstants.Origin];
            if (origin != null)
            {
                //There are several standard trusted domains
                //See if this is one of our allowed origins, or we allow all
                string allowedOriginsSetting = "";
                if (!String.IsNullOrWhiteSpace(ConfigurationSettings.Default.Api_AllowedCorsOrigins))
                {
                    allowedOriginsSetting += "," + ConfigurationSettings.Default.Api_AllowedCorsOrigins;
                }
                string[] allowedOrigins = allowedOriginsSetting.Split(',');
                bool allowed = false;
                foreach (string allowedOrigin in allowedOrigins)
                {
                    if (allowedOrigin.Trim().ToLowerInvariant() == origin.Trim().ToLowerInvariant() || allowedOrigin.Trim() == CorsConstants.AllowOriginAll)
                    {
                        allowed = true;
                        break;
                    }
                }
                return (allowed) ? origin : null;
            }

            return null; 
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            string origin = correlationState as string;
            if (origin != null)
            {
                HttpResponseMessageProperty httpProp = null;
                if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
                {
                    httpProp = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
                }
                else
                {
                    httpProp = new HttpResponseMessageProperty();
                    reply.Properties.Add(HttpResponseMessageProperty.Name, httpProp);
                }

                httpProp.Headers.Add(CorsConstants.AccessControlAllowOrigin, origin);
            }
        }
    }
}
