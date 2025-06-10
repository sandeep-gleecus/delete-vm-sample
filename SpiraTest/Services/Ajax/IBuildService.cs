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
    /// Communicates with the SortedGrid AJAX component for displaying/updating build data
    /// </summary>
    [
    ServiceContract(Name = "BuildService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IBuildService : ISortedListService, INavigationService, IFormService
    {
        [OperationContract]
        JsonDictionaryOfStrings GetBuildsForRelease(int projectId, int releaseId);
    }
}
