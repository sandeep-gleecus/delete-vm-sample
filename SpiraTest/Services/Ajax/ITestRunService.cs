using System;
using System.Collections.Generic;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating test run data
    /// </summary>
    [
    ServiceContract(Name = "TestRunService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestRunService : ISortedListService, INavigationService, IFormService
    {
        [OperationContract]
        int TestRun_Count(int projectId, ArtifactReference artifact);

        [OperationContract]
        GraphData TestRun_RetrieveProgress(int projectId, int? releaseId);

        [OperationContract]
        List<NameValue> RetrievePendingByUserIdAndTestCase(int projectId, int testCaseId);

		[OperationContract]
		void TestRun_ReassignPending(int testRunsPendingId, int newAssigneeId);
	}
}
