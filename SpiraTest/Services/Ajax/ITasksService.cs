using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating tasks data
    /// </summary>
    [
    ServiceContract(Name = "TasksService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITasksService : ISortedListService, INavigationService, IFormService, ICommentService, ITreeViewService, IWorkflowOperationsService, IWorkspaceService
    {
        [OperationContract]
        JsonDictionaryOfStrings GetReleasesForTaskRequirement(int projectId, int? requirementId);

        [OperationContract]
        int Task_Split(int projectId, int taskId, string name, int? effortPercentage, int? ownerId, string comment);

        [OperationContract]
        List<GraphEntry> Task_RetrieveGroupProgress(int projectGroupId, bool activeReleasesOnly);

        [OperationContract]
        int Task_Count(int projectId, ArtifactReference artifact);

        [OperationContract]
        SortedData Task_RetrieveByTestRunId(int projectId, int testRunId);

        [OperationContract]
        List<GraphEntry> Task_RetrieveProgress(int projectId, int? releaseId);

        [OperationContract]
        GraphData Task_RetrieveBurndown(int projectId, int? releaseId);
    }
}
