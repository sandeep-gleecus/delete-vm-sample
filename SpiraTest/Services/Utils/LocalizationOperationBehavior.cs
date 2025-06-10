using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using Inflectra.SpiraTest.Web.Classes;
using System.Threading;

namespace Inflectra.SpiraTest.Web.Services.Utils
{
    /// <summary>
    /// Operation behavior that sets the web service's culture to the one specified in SpiraContext
    /// </summary>
    public class LocalizationOperationBehavior : IOperationBehavior
    {
        #region IOperationBehavior
        
        /// <summary>
        /// Add the operation invoker to the various operations
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="dispatchOperation"></param>
        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)   
        {
            dispatchOperation.Invoker = new LocalizationOperationInvoker(dispatchOperation.Invoker);
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
        
        #endregion
    }

    /// <summary>
    /// Sets the thread culture before the operation is invoked
    /// </summary>
    public class LocalizationOperationInvoker : IOperationInvoker
    {
        IOperationInvoker innerOperationInvoker;

        public LocalizationOperationInvoker(IOperationInvoker innerOperationInvoker)
        {
            this.innerOperationInvoker = innerOperationInvoker;
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            //Set the thread culture
            if (!String.IsNullOrEmpty(SpiraContext.Current.CultureName) && (Thread.CurrentThread.CurrentCulture.Name != SpiraContext.Current.CultureName || Thread.CurrentThread.CurrentUICulture.Name != SpiraContext.Current.CultureName))
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(SpiraContext.Current.CultureName);
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(SpiraContext.Current.CultureName);
            }

            //Simply call the inner invoker
            return this.innerOperationInvoker.Invoke(instance, inputs, out outputs);
        }

        // remaining methods elided
        // they simply delegate to innerOperationInvoker

        public object[] AllocateInputs()
        {
            return innerOperationInvoker.AllocateInputs();
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            return innerOperationInvoker.InvokeBegin(instance, inputs, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            return innerOperationInvoker.InvokeEnd(instance, out outputs, result);
        }

        public bool IsSynchronous
        {
            get
            {
                return innerOperationInvoker.IsSynchronous;
            }
        }
    }
}