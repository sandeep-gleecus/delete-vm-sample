using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used by TestExecutionExploratory.aspx to handle the execution and editing of manual test cases by the user
    /// </summary>
    [
    ServiceContract(Name = "TestExecutionExploratoryService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestExecutionExploratoryService : INavigationService, IFormService
    {
        [OperationContract]
        void TestExecution_AddIncidentAssociation(int projectId, int testRunStepId, List<int> incidentIds);

        [OperationContract]
        TestRunsPendingModel TestExecution_RetrieveTestRunsPending(int projectId, int testRunsPendingId);

        [OperationContract]
        DataItem RetrieveTestRunStep(int projectId, int testRunStepId);
        
        [OperationContract]
        JsonDictionaryOfStrings PassTestRunStep(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string description,
            string expectedResult,
            string sampleData,
            string actualResult,
            DateTime startDate,
            DateTime endDate,
            int actualDuration,
            DataItem incidentDataItem
            );

        [OperationContract]
        JsonDictionaryOfStrings NotApplicableTestRunStep(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string description,
            string expectedResult,
            string sampleData,
            string actualResult,
            DateTime startDate,
            DateTime endDate,
            int actualDuration,
            DataItem incidentDataItem
            );

        [OperationContract]
        JsonDictionaryOfStrings PassAllTestRunSteps(int projectId, int testRunsPendingId, int testRunId, string actualResult, int testRunStepId, DateTime startDate, DateTime endDate, int actualDuration);

        [OperationContract]
        JsonDictionaryOfStrings FailTestRunStep(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string description,
            string expectedResult,
            string sampleData,
            string actualResult,
            DateTime startDate,
            DateTime endDate,
            int actualDuration,
            DataItem incidentDataItem
            );

        [OperationContract]
        JsonDictionaryOfStrings BlockTestRunStep(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string description,
            string expectedResult,
            string sampleData,
            string actualResult,
            DateTime startDate,
            DateTime endDate,
            int actualDuration,
            DataItem incidentDataItem
            );

        [OperationContract]
        JsonDictionaryOfStrings CautionTestRunStep(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string description,
            string expectedResult,
            string sampleData,
            string actualResult,
            DateTime startDate,
            DateTime endDate,
            int actualDuration,
            DataItem incidentDataItem
            );

        [OperationContract]
        void UpdateTestRunSingleField(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int? testRunStepId,
            int fieldToUpdate,
            string textField
            );

        [OperationContract]
        void UpdateTestRunStepPositions(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            List<TestRunStepPosition> testRunStepPositions
            );

        [OperationContract]
        int? DeleteExploratoryTestRunStep(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId
            );

        [OperationContract]
        int? CreateNewExploratoryTestRunStep(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testCaseId
            );

        [OperationContract]
        int? CloneExploratoryTestRunStep(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testCaseId,
            string description,
            string expectedResult,
            string sampleData
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
        void TestExecution_LogDisplaySettings(
            int projectId,
            bool showCaseDescription, 
            bool showExpectedResult,
            bool showSampleData,
            bool showCustomProperties, 
            bool showLastResult,
            bool guidedTourSeen
            );
    }

}
