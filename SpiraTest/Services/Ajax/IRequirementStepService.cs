using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of steps in a use case
    /// </summary>
    [
    ServiceContract(Name = "RequirementStepService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IRequirementStepService : IOrderedListService
    {
        [OperationContract]
        string RequirementStep_RetrieveAsDotNotation(int projectId, int requirementId);
    }
}
