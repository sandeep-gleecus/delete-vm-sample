using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Collections.Specialized;
using Inflectra.SpiraTest.Common;
using System.ServiceModel.Web;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// This version adds support for CORS cross-domain requests.
    /// </summary>
    /// <remarks>
    /// This version also implements the Newtonsoft JSON serialization format
    /// </remarks>
    public class RestWebHttpBehavior2 : RestWebHttpBehavior
    {
        protected override IDispatchMessageFormatter GetRequestDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            //Pass the fallback formatter in case we are not given JSON
            IDispatchMessageFormatter fallbackFormatter = base.GetRequestDispatchFormatter(operationDescription, endpoint);
            return new UriTemplateDispatchFormatter(
                operationDescription,
                new NewtonsoftJsonDispatchFormatter(operationDescription, true, fallbackFormatter),
                GetQueryStringConverter(operationDescription),
                endpoint.Contract.Name,
                endpoint.Address.Uri);
        }

        protected override IDispatchMessageFormatter GetReplyDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            //Pass the fallback formatter in case we are not given JSON
            IDispatchMessageFormatter fallbackFormatter = base.GetReplyDispatchFormatter(operationDescription, endpoint);
            return new NewtonsoftJsonDispatchFormatter(operationDescription, false, fallbackFormatter);
        }
    }
}