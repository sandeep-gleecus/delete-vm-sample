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
    /// Provides the web service used to interacting with the various client-side graphing AJAX components
    /// </summary>
    [
    ServiceContract(Name = "GraphingService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IGraphingService
    {
        [OperationContract]
        GraphData RetrieveDateRange(int projectId, int graphId, string dateRange, JsonDictionaryOfStrings filters);

        [OperationContract]
        void UpdateSettings(int projectId, string webPartUniqueId, JsonDictionaryOfStrings settings);

        [OperationContract]
        GraphData RetrieveSummary(int projectId, int artifactTypeId, string xAxisField, string groupByField);

        [OperationContract]
        GraphData RetrieveSnapshot(int projectId, int graphId, JsonDictionaryOfStrings filters);

        [OperationContract]
        GraphData CustomGraph_Retrieve(int projectId, int graphId);

        [OperationContract]
        GraphData CustomGraph_RetrievePreview(int projectId, string sql);
    }
}
