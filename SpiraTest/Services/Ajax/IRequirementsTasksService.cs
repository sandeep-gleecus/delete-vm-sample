using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the HierarchicalGrid AJAX component for displaying non-summary requirements with tasks
    /// nested underneath
    /// </summary>
    [
    ServiceContract(Name = "RequirementsTaskService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IRequirementsTaskService : IHierarchicalListService
    {
        [OperationContract]
        int RequirementsTask_Count(int projectId, ArtifactReference artifact);
    }
}
