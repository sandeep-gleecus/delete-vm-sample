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
    /// Communicates with the HierarchicalGrid AJAX component for displaying/updating test case data
    /// </summary>
    [
    ServiceContract(Name = "TestCaseService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestCaseService : ISortedListService, IAssociationPanelService, INavigationService, IFormService, ICommentService, ITreeViewService, IWorkflowOperationsService, IItemSelectorService
    {
        [OperationContract]
        List<DataItem> RetrieveParameters(int testCaseId, bool includeInherited, bool includeAlreadySet);

        [OperationContract]
        void SaveParameters(int projectId, int testCaseId, List<DataItem> parameters);

        [OperationContract]
        JsonDictionaryOfStrings RetrieveTestFolders(int projectId);

        [OperationContract]
        List<GraphEntry> TestCase_RetrieveExecutionSummary(int projectId, int? releaseId);

        [OperationContract]
        List<GraphEntry> TestCase_RetrieveGroupExecutionSummary(int projectGroupId, bool activeReleasesOnly);

		//[OperationContract]
		//List<GraphEntry> TestCase_RetrieveExecutedSummary(int projectId);

		[OperationContract]
		GraphData TestCase_RetrieveTestPreparationStatusSummary(int projectId);

	}
}
