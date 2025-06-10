using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// All AJAX web services that are called by the WorkflowOperations server control need to implement this interface
    /// </summary>
    [ServiceContract(Name = "IWorkflowOperationsService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    interface IWorkflowOperationsService
    {
        [OperationContract]
        List<DataItem> WorkflowOperations_Retrieve(int projectId, int artifactId, int? typeId);
    }
}
