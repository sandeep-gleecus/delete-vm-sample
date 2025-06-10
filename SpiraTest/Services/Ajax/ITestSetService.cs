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
    /// Communicates with the HierarchicalGrid AJAX component for displaying/updating test set data
    /// </summary>
    [
    ServiceContract(Name = "TestSetService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestSetService : ISortedListService, INavigationService, IFormService, ICommentService, ITreeViewService, IItemSelectorService
    {
        [OperationContract]
        JsonDictionaryOfStrings RetrieveParameters(int projectId, int testSetId);

        [OperationContract]
        List<DataItem> RetrieveParameterValues(int projectId, int testSetId);

        [OperationContract]
        void DeleteParameterValue(int projectId, int testSetId, int testCaseParameterId);

        [OperationContract]
        void UpdateParameterValue(int projectId, int testSetId, int testCaseParameterId, string newValue);

        [OperationContract]
        void AddParameterValue(int projectId, int testSetId, int testCaseParameterId, string newValue);

        [OperationContract]
        JsonDictionaryOfStrings RetrieveTestSetFolders(int projectId);

        [OperationContract]
        List<GraphEntry> TestSet_RetrieveExecutionSummary(int projectId, int? releaseId);

        [OperationContract]
        List<GraphEntry> TestSet_RetrieveScheduleSummary(int projectId, int? releaseId);

        [OperationContract]
        int TestSet_Count(int projectId, ArtifactReference artifact);
    }
}
