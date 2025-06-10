using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Description;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Wsdl;
using Inflectra.SpiraTest.Web.Services.Utils;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Net;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// WCF Service Host that dynamically creates the appropriate endpoints
    /// </summary>
    /// <remarks>
    /// Reduces the amount of configuration needed in Web.Config and avoids need to manually
    /// create separate HTTP and HTTPS binding endpoints for each service
    /// 
    /// Implements the CORS protocol for cross-domain REST requests
    /// https://code.msdn.microsoft.com/Implementing-CORS-support-c1f9cd4b/
    /// 
    /// Also this version uses the Newtonsoft JSON serializer instead of the built-in WCF one for better JSON handling
    /// </remarks>
    public class RestServiceHost2 : ServiceHost
    {
        public RestServiceHost2()
        {
        }

        public RestServiceHost2(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
        }

        public RestServiceHost2(object singeltonInstance, params Uri[] baseAddresses)
            : base(singeltonInstance, baseAddresses)
        {
        }

        /// <summary>
        /// Responsible for actually provisioning the end points
        /// </summary>
        protected override void ApplyConfiguration()
        {
            base.ApplyConfiguration();

            //Create the endpoint behavior for the RESTful web services
            //Also specify that it can dynamically use either XML or JSON depending on the Content-Type HTTP Header
            RestWebHttpBehavior2 restWebHttpBehavior = new RestWebHttpBehavior2();
            restWebHttpBehavior.DefaultBodyStyle = WebMessageBodyStyle.Bare;
            restWebHttpBehavior.AutomaticFormatSelectionEnabled = true;
            //We default to using XML because we handle JSON separately in this service host
            //using the special Newtonsoft JSON serializer
            restWebHttpBehavior.DefaultOutgoingRequestFormat = System.ServiceModel.Web.WebMessageFormat.Xml;
            restWebHttpBehavior.DefaultOutgoingResponseFormat = System.ServiceModel.Web.WebMessageFormat.Xml;

            // Create the endpoint based on the service name and the binding derived from the scheme name
            ContractDescription contract = ContractDescription.GetContract(this.Description.ServiceType);
            bool httpBaseAddressAvailable = false;
            bool httpsBaseAddressAvailable = false;
            foreach (Uri address in this.BaseAddresses)
            {
                //Create the appropriate web binding for the URL scheme (http/https)
                //Allow cross domain script access (for use in AJAX API web calls)
                //All REST services are stateless so no need to use cookies
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = Int32.MaxValue;
                binding.CrossDomainScriptAccessEnabled = true;
                binding.AllowCookies = false;
                binding.ContentTypeMapper = new NewtonsoftJsonContentTypeMapper();
                if (address.Scheme.ToLowerInvariant() == "https")
                {
                    binding.Security.Mode = WebHttpSecurityMode.Transport;
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                    httpsBaseAddressAvailable = true;
                }
                else
                {
                    binding.Security.Mode = WebHttpSecurityMode.None;
                    httpBaseAddressAvailable = true;
                }

                //Set the reader quotas to unlimited
                binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
                binding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
                binding.ReaderQuotas.MaxDepth = Int32.MaxValue;
                binding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
                binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

                //Create the endpoint and specify its endpoint behavior
                ServiceEndpoint serviceEndpoint = new ServiceEndpoint(contract, binding, new EndpointAddress(address));
                serviceEndpoint.Behaviors.Add(restWebHttpBehavior);

                //Add the endpoint
                this.Description.Endpoints.Add(serviceEndpoint);
            }

            //Specify the service behavior. Easier to do it here than in the web.config file

            //<dataContractSerializer maxItemsInObjectGraph="2147483647" />
            List<OperationDescription> operationsToAddPreflight = new List<OperationDescription>();
            foreach (OperationDescription operationDescription in contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractSerializer = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractSerializer == null)
                {
                    dataContractSerializer = new DataContractSerializerOperationBehavior(operationDescription);
                    operationDescription.Behaviors.Add(dataContractSerializer);
                }
                dataContractSerializer.MaxItemsInObjectGraph = Int32.MaxValue;

                //Add preflight support to this description
                //We used to only do it if a CORS origin was set, but then the problem is that if an origin is set
                //it is not picked up until the application pool is next recycled, which is unpredictable
                operationsToAddPreflight.Add(operationDescription);
            }

            //Add REST-CORS preflight OPTIONS operations as needed
            Dictionary<UriTemplate, PreflightOperationBehavior> uriTemplates = new Dictionary<UriTemplate, PreflightOperationBehavior>();
            foreach (OperationDescription operationDescription in operationsToAddPreflight)
            {
                AddPreflightOperation(operationDescription, uriTemplates);
            }

            //Log out all of the CORS options
            if (Common.Properties.Settings.Default.TraceLogging_Enable)
            {
                foreach (KeyValuePair<UriTemplate, PreflightOperationBehavior> uriTemplate in uriTemplates)
                {
                    Logger.LogTraceEvent("AddPreflightOperation", "OPTIONS: " + uriTemplate.Key.ToString());
                }
            }

            //Log out all of the operations, useful for debugging - leave commented for production use
            //LogOperations(contract);

            //<serviceMetadata httpGetEnabled="false" httpsGetEnabled="false" />
            ServiceMetadataBehavior serviceMetaData = this.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (serviceMetaData == null)
            {
                serviceMetaData = new ServiceMetadataBehavior();
                this.Description.Behaviors.Add(serviceMetaData);
            }

			//Add an API throttle if specified
			if (Common.ConfigurationSettings.Default.Api_MaxConcurrentCallsThrottle > 0)
			{
				ServiceThrottlingBehavior serviceThrottlingBehavior = new ServiceThrottlingBehavior();
				serviceThrottlingBehavior.MaxConcurrentCalls = Common.ConfigurationSettings.Default.Api_MaxConcurrentCallsThrottle;
				this.Description.Behaviors.Add(serviceThrottlingBehavior);
			}

			//We dynamically set these based on the available base addresses
			serviceMetaData.HttpGetEnabled = httpBaseAddressAvailable;
            serviceMetaData.HttpsGetEnabled = httpsBaseAddressAvailable;

            //<serviceDebug includeExceptionDetailInFaults="true" />
            ServiceDebugBehavior serviceDebug = this.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (serviceDebug == null)
            {
                serviceDebug = new ServiceDebugBehavior();
                this.Description.Behaviors.Add(serviceDebug);
            }
            serviceDebug.IncludeExceptionDetailInFaults = true;
        }

        /// <summary>
        /// Logs the various contract operations
        /// </summary>
        /// <param name="contract"></param>
        /// <remarks>Only used for debugging</remarks>
        public static void LogOperations(ContractDescription contract)
        {
            Logger.LogTraceEvent("RestServiceHost", "Contract Name: " + contract.Name);
            foreach (OperationDescription operationDescription in contract.Operations)
            {
                WebGetAttribute webGet = operationDescription.Behaviors.Find<WebGetAttribute>();
                if (webGet != null)
                {
                    Logger.LogTraceEvent("RestServiceHost", "GET: " + webGet.UriTemplate + " (" + operationDescription.Name + ")");
                }
                WebInvokeAttribute webInvoke = operationDescription.Behaviors.Find<WebInvokeAttribute>();
                if (webInvoke != null)
                {
                    Logger.LogTraceEvent("RestServiceHost", webInvoke.Method + ": " + webInvoke.UriTemplate + " (" + operationDescription.Name + ")");                    
                }
            }
        }

        #region Preflight CORS Support

        /// <summary>
        /// Adds a preflight operation
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="operation"></param>
        private void AddPreflightOperation(OperationDescription operation, Dictionary<UriTemplate, PreflightOperationBehavior> uriTemplates)
        {
            UriTemplate originalUriTemplate = null;
            WebInvokeAttribute originalWia = operation.Behaviors.Find<WebInvokeAttribute>();
            WebGetAttribute originalWga = operation.Behaviors.Find<WebGetAttribute>();

            string originalMethod = "";
            if (originalWia != null)
            {
                if (originalWia.UriTemplate != null)
                {
                    originalUriTemplate = new UriTemplate(originalWia.UriTemplate);
                }
                else
                {
                    originalUriTemplate = new UriTemplate(operation.Name);
                }
                originalMethod = originalWia != null && originalWia.Method != null ? originalWia.Method : "POST";
            }
            else if (originalWga != null)
            {
                if (originalWga.UriTemplate != null)
                {
                    originalUriTemplate = new UriTemplate(originalWga.UriTemplate);
                }
                else
                {
                    originalUriTemplate = new UriTemplate(operation.Name);
                }
                originalMethod = "GET";
            }

            if (originalUriTemplate != null)
            {
                //Useful to see what preflight operations are supported
                //Logger.LogTraceEvent("AddPreflightOperation", originalMethod + ": " + originalUriTemplate);

                UriTemplate matchingUriTemplate = uriTemplates.Keys.FirstOrDefault(u => u.IsEquivalentToWithoutQueryString(originalUriTemplate) || u.IsEquivalentTo(originalUriTemplate));
                if (matchingUriTemplate != null)
                {
                    // there is already an OPTIONS operation for this URI, we can reuse it 
                    PreflightOperationBehavior operationBehavior = uriTemplates[matchingUriTemplate];
                    operationBehavior.AddAllowedMethod(originalMethod);
                }
                else
                {
                    ContractDescription contract = operation.DeclaringContract;
                    OperationDescription preflightOperation;
                    PreflightOperationBehavior preflightOperationBehavior;
                    CreatePreflightOperation(operation, originalUriTemplate, originalMethod, contract, out preflightOperation, out preflightOperationBehavior);
                    uriTemplates.Add(originalUriTemplate, preflightOperationBehavior);

                    contract.Operations.Add(preflightOperation);
                }
            }
        }

        /// <summary>
        /// Creates a special Preflight OPTIONS operation
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="originalUriTemplate"></param>
        /// <param name="originalMethod"></param>
        /// <param name="contract"></param>
        /// <param name="preflightOperation"></param>
        /// <param name="preflightOperationBehavior"></param>
        private static void CreatePreflightOperation(OperationDescription operation, UriTemplate originalUriTemplate, string originalMethod, ContractDescription contract, out OperationDescription preflightOperation, out PreflightOperationBehavior preflightOperationBehavior)
        {
            preflightOperation = new OperationDescription(operation.Name + CorsConstants.PreflightSuffix, contract);
            
            //First the input message
            MessageDescription inputMessage = new MessageDescription(operation.Messages[0].Action + CorsConstants.PreflightSuffix, MessageDirection.Input);
            preflightOperation.Messages.Add(inputMessage);

            //We need to mirror the input parameters in the URI template
            //First any variables in the path
            if (originalUriTemplate.PathSegmentVariableNames != null && originalUriTemplate.PathSegmentVariableNames.Count > 0)
            {
                foreach (string uriParameter in originalUriTemplate.PathSegmentVariableNames)
                {
                    inputMessage.Body.Parts.Add(new MessagePartDescription(uriParameter, "") { Type = typeof(string) });
                }
            }
            //Next any in the querystring
            if (originalUriTemplate.QueryValueVariableNames != null && originalUriTemplate.QueryValueVariableNames.Count > 0)
            {
                foreach (string uriParameter in originalUriTemplate.QueryValueVariableNames)
                {
                    inputMessage.Body.Parts.Add(new MessagePartDescription(uriParameter, "") { Type = typeof(string) });
                }
            }

            //Now the output message, we only need the CORS headers in reality
            MessageDescription outputMessage = new MessageDescription(operation.Messages[1].Action + CorsConstants.PreflightSuffix, MessageDirection.Output);
            //outputMessage.Body.ReturnValue = new MessagePartDescription(preflightOperation.Name + "Return", contract.Namespace) { Type = typeof(Message) };
            preflightOperation.Messages.Add(outputMessage);

            WebInvokeAttribute wia = new WebInvokeAttribute();
            wia.UriTemplate = originalUriTemplate.ToString();
            wia.Method = "OPTIONS";

            preflightOperation.Behaviors.Add(wia);
            preflightOperation.Behaviors.Add(new DataContractSerializerOperationBehavior(preflightOperation));
            preflightOperationBehavior = new PreflightOperationBehavior(preflightOperation);
            preflightOperationBehavior.AddAllowedMethod(originalMethod);
            preflightOperation.Behaviors.Add(preflightOperationBehavior);
        }

        #endregion
    }
}
