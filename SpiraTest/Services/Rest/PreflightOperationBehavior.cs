using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// The behavior for REST-CORS cross-domain preflight requests
    /// </summary>
    class PreflightOperationBehavior : IOperationBehavior
    {
        private OperationDescription preflightOperation;
        private List<string> allowedMethods;

        public PreflightOperationBehavior(OperationDescription preflightOperation)
        {
            this.preflightOperation = preflightOperation;
            this.allowedMethods = new List<string>();
        }

        public void AddAllowedMethod(string httpMethod)
        {
            this.allowedMethods.Add(httpMethod);
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            int parameterCount = operationDescription.Messages[0].Body.Parts.Count;
            dispatchOperation.Invoker = new PreflightOperationInvoker(operationDescription.Messages[1].Action, this.allowedMethods, parameterCount);
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    } 
}