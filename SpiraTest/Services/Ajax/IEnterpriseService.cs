using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used for making Ajax calls that display enterprise-wide data
    /// </summary>
    [
    ServiceContract(Name = "EnterpriseService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IEnterpriseService : IWorkspaceService
    {
        [OperationContract]
        WorkspaceData Enterprise_RetrieveBuilds(int rowsToDisplay);
    }
}
