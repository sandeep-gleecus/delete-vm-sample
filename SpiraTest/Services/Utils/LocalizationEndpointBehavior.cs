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
    /// Endpoint behavior that sets the web service's culture to the one specified in SpiraTest's Administration
    /// </summary>
    public class LocalizationEndpointBehavior : IEndpointBehavior
    {
        private bool checkHeaders = false;
        /// <summary>
        /// Default constructor
        /// </summary>
        public LocalizationEndpointBehavior()
        {
        }

        /// <summary>
        /// Constructor that should be used when you want the service to scan the incoming message for the X-Culture-Info
        /// culture HTTP header that signifies the culture that the client is using and the X-Timezone-Info to see
        /// what timezone the user is using
        /// </summary>
        /// <param name="checkHeaders">Should we check the messages for the culture header</param>
        public LocalizationEndpointBehavior(bool checkHeaders)
        {
            this.checkHeaders = checkHeaders;
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
            //See if we need to scan for operations that provide the user profile info
            if (this.checkHeaders)
            {
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new LocalizationMessageInspector());
            }
            endpointDispatcher.DispatchRuntime.InstanceContextInitializers.Add(new LocalizationInstanceContextInitializer());
        }


        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }

    /// <summary>
    /// Sets the web service culture when the web service is initialized
    /// </summary>
    public class LocalizationInstanceContextInitializer : IInstanceContextInitializer
    {
        public void Initialize(System.ServiceModel.InstanceContext instanceContext, Message message)
        {
            //See if we have a system-wide culture set
            if (!String.IsNullOrEmpty(ConfigurationSettings.Default.Globalization_DefaultCulture))
            {
                if (SpiraContext.Current.CultureName != ConfigurationSettings.Default.Globalization_DefaultCulture)
                {
                    SpiraContext.Current.CultureName = ConfigurationSettings.Default.Globalization_DefaultCulture;
                }
            }

            //See if we have a system-wide timezone set
            if (String.IsNullOrEmpty(ConfigurationSettings.Default.Globalization_DefaultTimezone))
            {
                SpiraContext.Current.TimezoneId = TimeZoneInfo.Local.Id;
            }
            else
            {
                SpiraContext.Current.TimezoneId = ConfigurationSettings.Default.Globalization_DefaultTimezone;
            }

        }
    }

    /// <summary>
    /// Sets the thread culture for the user's profile
    /// </summary>
    public class LocalizationMessageInspector : IDispatchMessageInspector
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LocalizationMessageInspector()
        {
        }

        public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            //See if we can get the HTTP headers, and see if it contains the special CultureInfo or timezone HTTP headers
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (!string.IsNullOrEmpty(httpRequestMessage.Headers["X-Culture-Info"]))
                {
                    string cultureName = httpRequestMessage.Headers["X-Culture-Info"];
                    if (SpiraContext.Current.CultureName != cultureName)
                    {
                        SpiraContext.Current.CultureName = cultureName;
                    }
                }
                if (!string.IsNullOrEmpty(httpRequestMessage.Headers["X-Timezone-Info"]))
                {
                    string timezoneInfo = httpRequestMessage.Headers["X-Timezone-Info"];
                    if (SpiraContext.Current.TimezoneId != timezoneInfo)
                    {
                        SpiraContext.Current.TimezoneId = timezoneInfo;
                    }
                } 
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}