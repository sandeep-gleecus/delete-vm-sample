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
    /// Implemented by the service that provides data to the PlanningBoard control
    /// </summary>
    [ServiceContract(Name = "PlanningBoardService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    public interface IPlanningBoardService
    {
        [OperationContract]
        void PlanningBoard_UpdateRelease(int projectId, int releaseId);

        [OperationContract]
        void PlanningBoard_UpdateGroupBy(int projectId, int groupById);

        [OperationContract]
        void PlanningBoard_UpdateOptions(int projectId, string option, bool optionValue);

        [OperationContract]
        PlanningData PlanningBoard_RetrieveGroupByContainers(int projectId, int releaseId, int groupById);

        [OperationContract]
        PlanningData PlanningBoard_RetrieveReleaseIterationInfo(int projectId, int releaseId, int groupById);

        [OperationContract]
        PlanningData PlanningBoard_RetrieveItems(int projectId, int releaseId, bool isIteration, int groupById, int? containerId, bool includeDetails, bool includeIncidents, bool includeTasks, bool includeTestCases);

        [OperationContract]
        string PlanningBoard_RetrieveItemTooltip(int projectId, int artifactId, int artifactTypeId);

        [OperationContract]
        void PlanningBoard_UpdateExpandCollapsed(int projectId, int groupById, int? containerId, bool expanded);

        [OperationContract]
        void PlanningBoard_MoveItems(int projectId, int releaseId, bool isIteration, int groupById, int? containerId, List<ArtifactReference> items, int? existingArtifactTypeId, int? existingArtifactId);
    }
}
