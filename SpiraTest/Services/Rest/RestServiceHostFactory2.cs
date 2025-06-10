using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Activation;
using System.ServiceModel;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// WCF Service Host Factory that dynamically creates the appropriate endpoints
    /// </summary>
    /// <remarks>
    /// Reduces the amount of configuration needed in Web.Config and avoids need to manually
    /// create separate HTTP and HTTPS binding endpoints for each service
    /// This version (2) supports the Newtonsoft JSON serialization formats for JSON.
    /// </remarks>
    public class RestServiceHostFactory2 : ServiceHostFactory
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            return base.CreateServiceHost(constructorString, baseAddresses);
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            //Create the service host
            ServiceHost restServiceHost = new RestServiceHost2(serviceType, baseAddresses);

            //Add the custom security manager that handles the authentication header or URL token
            restServiceHost.Authorization.PrincipalPermissionMode = System.ServiceModel.Description.PrincipalPermissionMode.Custom;
            restServiceHost.Authorization.ServiceAuthorizationManager = new RestServiceAuthorizationManager();
            return restServiceHost;
        }
    }
}