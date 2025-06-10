using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [
    ServiceContract(Name = "IListService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IListService
    {
        [OperationContract]
        JsonDictionaryOfStrings RetrieveLookupList(int projectId, string operation);
     
        [OperationContract]
        string RetrieveNameDesc(int? projectId, int artifactId, int? displayTypeId);
        
        [OperationContract]
        string CustomListOperation(string operation, int projectId, int destId, List<string> items);

        [OperationContract]
        JsonDictionaryOfStrings RetrievePaginationOptions(int projectId);

        [OperationContract]
        void UpdatePagination(int projectId, int pageSize, int currentPage);

        [OperationContract]
        void ToggleColumnVisibility(int projectId, string fieldName);

        [OperationContract]
        string CustomOperation(int projectId, string operation, string value);

        [OperationContract]
        string CustomOperationEx(int projectId, string operation, JsonDictionaryOfStrings parameters);

        [OperationContract]
        void List_ChangeColumnPosition(int projectId, string fieldName, int newIndex);

        [OperationContract]
        void List_ChangeColumnWidth(int projectId, string fieldName, int width);
    }
}
