using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [
    ServiceContract(Name = "IHierarchicalListService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IHierarchicalListService : IFilteredListService
    {
        [OperationContract]
        int HierarchicalList_Insert(int projectId, JsonDictionaryOfStrings standardFilters, int artifactId, string artifact);

        [OperationContract]
        HierarchicalData HierarchicalList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, bool updatedRecordsOnly);
        
        [OperationContract]
        HierarchicalData HierarchicalList_RetrieveSelection(int projectId, JsonDictionaryOfStrings standardFilters, int startIndex, int itemCount);
        
        [OperationContract]
        HierarchicalDataItem Refresh(int projectId, int artifactId);
        
        [OperationContract]
        List<ValidationMessage> HierarchicalList_Update(int projectId, List<HierarchicalDataItem> dataItems);
        
        [OperationContract]
        void Expand(int projectId, int artifactId);
        
        [OperationContract]
        void Collapse(int projectId, int artifactId);
        
        [OperationContract]
        string Indent(int projectId, List<string> items);
        
        [OperationContract]
        string Outdent(int projectId, List<string> items);
        
        [OperationContract]
        void Move(int projectId, List<string> sourceItems, int? destId);
        
        [OperationContract]
        void ExpandToLevel(int projectId, int level, JsonDictionaryOfStrings standardFilters);
        
        [OperationContract]
        void Delete(int projectId, List<string> items);
        
        [OperationContract]
        void Copy(int projectId, List<string> sourceItems, int destId);
        
        [OperationContract]
        void Export(int sourceProjectId, int destProjectId, List<string> items);
        
        [OperationContract]
        JsonDictionaryOfStrings RetrieveHierarchy(int projectId, JsonDictionaryOfStrings standardFilters);

		[OperationContract]
		int? Form_InsertChild(int projectId, int artifactId);
	}
}
