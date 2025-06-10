using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the various test run step AJAX components on the pages
    /// </summary>
    [
    ServiceContract(Name = "TestRunStepService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestRunStepService
    {
        [OperationContract]
        OrderedData TestRunStep_Retrieve(int projectId, int testRunId);

        [OperationContract]
        void TestRunStep_AddIncidentAssociation(int projectId, int testRunStepId, List<int> incidentIds, string comment);
    }
}
