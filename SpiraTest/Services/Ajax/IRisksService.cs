using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating risks
    /// </summary>
    [
    ServiceContract(Name = "RisksService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IRisksService : ISortedListService, INavigationService, IWorkflowOperationsService, IFormService, ICommentService
    {
        [OperationContract]
        bool Risk_CheckExists(int? projectId, int riskId);

        [OperationContract]
        GraphData Risk_RetrieveCountByExposure(int projectId);
    }
}
