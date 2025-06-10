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
    /// Provides the web service used to interacting with the various client-side artifact association AJAX components
    /// </summary>
    [
    ServiceContract(Name = "HistoryService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IHistoryService : ISortedListService
    {
        [OperationContract]
        int History_Count(int projectId, ArtifactReference artifact);
    }
}
