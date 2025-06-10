using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating test configuration sets
    /// </summary>
    [
    ServiceContract(Name = "TestConfigurationSetService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ITestConfigurationSetService : ISortedListService, INavigationService, IFormService
    {
    }
}
