using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System.ServiceModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [ServiceContract(Name = "IOrderedListService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    interface IOrderedListService : IListService
    {
        [OperationContract]
        OrderedData OrderedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters);

        [OperationContract]
        void OrderedList_Delete(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items);
        
        [OperationContract]
        void OrderedList_Move(int projectId, JsonDictionaryOfStrings standardFilters, List<string> sourceItems, int? destId);
        
        [OperationContract]
        OrderedDataItem OrderedList_Refresh(int projectId, JsonDictionaryOfStrings standardFilters, int artifactId);
        
        [OperationContract]
        List<ValidationMessage> OrderedList_Update(int projectId, JsonDictionaryOfStrings standardFilters, List<OrderedDataItem> dataItems);
        
        [OperationContract]
        int OrderedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? artifactId);
        
        [OperationContract]
        void OrderedList_Copy(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items);
    }
}
