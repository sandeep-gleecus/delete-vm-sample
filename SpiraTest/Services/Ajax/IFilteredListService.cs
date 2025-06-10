using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [
    ServiceContract(Name = "IFilteredListService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IFilteredListService : IListService
    {
        [OperationContract]
        JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared);

        [OperationContract]
        string RestoreSavedFilter(int savedFilterId);

        [OperationContract]
        string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId);

        [OperationContract]
        string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns);
    }
}
