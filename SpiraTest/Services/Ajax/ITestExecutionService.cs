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
    /// Used by TestExecution.aspx to handle the execution of manual test cases by the user
    /// </summary>
    [
    ServiceContract(Name = "TestExecutionService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestExecutionService : INavigationService, IFormService
    {
        [OperationContract]
        void TestExecution_AddIncidentAssociation(int projectId, int testRunStepId, List<int> incidentIds);

        [OperationContract]
        TestRunsPendingModel TestExecution_RetrieveTestRunsPending(int projectId, int testRunsPendingId);

        [OperationContract]
        DataItem RetrieveTestRunStep(int projectId, int testRunStepId);
        
        [OperationContract]
        JsonDictionaryOfStrings PassTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem);

        [OperationContract]
        JsonDictionaryOfStrings NotApplicableTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem);

        [OperationContract]
        JsonDictionaryOfStrings PassAllTestRunSteps(int projectId, int testRunsPendingId, int testRunId, string actualResult, int testRunStepId, DateTime startDate, DateTime endDate, int actualDuration);

        [OperationContract]
        JsonDictionaryOfStrings FailTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem);

        [OperationContract]
        JsonDictionaryOfStrings BlockTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem);

        [OperationContract]
        JsonDictionaryOfStrings CautionTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem);

        [OperationContract]
        void UpdateTestRunActualResult(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string textField
            );

        [OperationContract]
        JsonDictionaryOfStrings TestExecution_LogIncident(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DataItem incidentDataItem);

        [OperationContract]
        int TestExecution_LogTask(
            int projectId,
            int testCaseId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string name,
            string description,
            int? ownerId
            );

        [OperationContract]
        void TestExecution_LogStepTiming(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, DateTime startDate, DateTime endDate, int actualDuration);

        [OperationContract]
        void TestExecution_LogCurrentPosition(int projectId, int? testRunStepId, int? testRunId);

        [OperationContract]
        void TestExecution_LogDisplaySettings(int projectId, int? displayModeMain, int? displayModeSub, bool alwaysShowTestRun, bool showCustomProperties, bool guidedTourSeen);
    }
}
