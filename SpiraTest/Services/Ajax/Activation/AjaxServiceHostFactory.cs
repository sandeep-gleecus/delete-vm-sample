using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Activation;
using System.ServiceModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax.Activation
{
    /// <summary>
    /// WCF Service Host Factory that dynamically creates the appropriate endpoints
    /// </summary>
    /// <remarks>
    /// Reduces the amount of configuration needed in Web.Config and avoids need to manually
    /// create separate HTTP and HTTPS binding endpoints for each service
    /// </remarks>
    public class AjaxServiceHostFactory : ServiceHostFactory
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            return base.CreateServiceHost(constructorString, baseAddresses);
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new AjaxServiceHost(serviceType, baseAddresses);
        }
    }
}