using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading.Tasks;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used by all services that are called by the Workspace dashboard components
    /// </summary>
    [
    ServiceContract(Name = "IWorkspaceService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IWorkspaceService
    {
        [OperationContract]
        WorkspaceData Workspace_RetrieveCompletionData(int workspaceId);
    }
}
