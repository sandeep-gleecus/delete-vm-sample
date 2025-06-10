using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of test cases in a test set
    /// </summary>
    [
    ServiceContract(Name = "TestSetTestCaseService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestSetTestCaseService : IOrderedListService
    {
        [OperationContract]
        List<DataItem> RetrieveParameters(int testSetTestCaseId);

        [OperationContract]
        void UpdateParameters(int testSetTestCaseId, List<DataItem> testCaseParameterValues);
    }
}
