using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;

namespace Inflectra.SpiraTest.Web.Services.Ajax.Activation
{
    /// <summary>
    /// Webbehaviour used to install the SpiraErrorHandler JSON error handler
    /// </summary>
    internal class AjaxErrorHandlerBehavior : IEndpointBehavior 
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Adds the custom error handler to the AJAX JSON endpoints
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="endpointDispatcher"></param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            //Remove all other error handlers 
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();
            //Add our own 
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new SpiraErrorHandler());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}