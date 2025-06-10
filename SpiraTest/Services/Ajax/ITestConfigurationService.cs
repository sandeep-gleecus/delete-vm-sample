using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the OrderedGrid AJAX component for displaying/updating the list of test configurations in a test configuration set
    /// </summary>
    [
    ServiceContract(Name = "TestConfigurationService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestConfigurationService : IOrderedListService
    {
        [OperationContract]
        JsonDictionaryOfStrings TestConfiguration_RetrieveParameters(int projectId);

        [OperationContract]
        JsonDictionaryOfStrings TestConfiguration_RetrieveCustomLists(int projectId);

        [OperationContract]
        void TestConfiguration_Populate(int projectId, int testConfigurationSetId, JsonDictionaryOfStrings testParameters);
    }
}
