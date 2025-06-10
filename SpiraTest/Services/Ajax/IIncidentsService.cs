using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating incidents data
    /// </summary>
    [
    ServiceContract(Name = "IncidentsService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IIncidentsService : ISortedListService, INavigationService, IWorkflowOperationsService, IFormService, ICommentService, IAssociationPanelService
    {
        [OperationContract]
        SortedData RetrieveByTestRunStepId(int projectId, int testRunStepId, int? testStepId);

        [OperationContract]
        void Incident_UpdateReleaseDetailsFilter(int projectId, int releaseFilterType);

        [OperationContract]
        GraphData Incident_RetrieveCountByOpenClosedStatus(int projectId, int? releaseId, bool useResolvedRelease);

        [OperationContract]
        GraphData Incident_RetrieveCountByPriority(int projectId, int? releaseId, bool useResolvedRelease);

        [OperationContract]
        GraphData Incident_RetrieveCountBySeverity(int projectId, int? releaseId, bool useResolvedRelease);

        [OperationContract]
        List<GraphEntry> Incident_RetrieveAging(int projectId, int? releaseId, int maximumAge, int ageInterval);

        [OperationContract]
        List<GraphEntry> Incident_RetrieveTestCoverage(int projectId, int? releaseId);

        [OperationContract]
        List<GraphEntry> Incident_RetrieveGroupAging(int projectGroupId, int maximumAge, int ageInterval);

        [OperationContract]
        int Incident_Count(int projectId, ArtifactReference artifact);

        [OperationContract]
        bool Incident_CheckExists(int? projectId, int incidentId);
    }
}
