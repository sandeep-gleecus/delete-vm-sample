using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of test steps in a test case
    /// </summary>
    [
    ServiceContract(Name = "TestStepService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestStepService : IOrderedListService, INavigationService, IFormService
    {
        [OperationContract]
        List<DataItem> RetrieveParameters(int testStepId, int linkedTestCaseId);

        [OperationContract]
        void UpdateParameters(int projectId, int testStepId, List<DataItem> testStepParameterValues);

        [OperationContract]
        void TestStep_ImportTestCase(int projectId, int testCaseId, int testCaseToImportId, int? testStepId);

        [OperationContract]
        void TestStep_CreateNewLinkedTestCase(int projectId, int testCaseId, int? testStepId, int? testCaseFolderId, string testCaseName, List<DataItem> parameters);
    }
}
