using System.Collections.Generic;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [ServiceContract(Name = "ISortedListService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    interface ISortedListService : IFilteredListService
    {
        [OperationContract]
        int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId);

        [OperationContract]
        SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId);
        
        /// <summary>
        /// Used when you edit a row to load the data before you make changes. Not used for read-only grids
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the artifact we're editing</param>
        /// <param name="displayTypeId">The type of Association this is. Only used by Association Service, but needed to be part of interface, so most services will ignore</param>
        /// <returns>The single row to be edited</returns>
        [OperationContract]
        SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId);
        
        [OperationContract]
        List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId);

        /// <summary>
        /// Used to update the sort
        /// </summary>
        /// <param name="sortAscending">Should we sort ascending (vs. descending)</param>
        /// <param name="sortProperty">The field to sort on</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="displayTypeId">The type of Association this is. Only used by Association Service, but needed to be part of interface, so most services will ignore</param>
        /// <returns>Error message if failed</returns>
        [OperationContract]
        string SortedList_UpdateSort(int projectId, string sortProperty, bool sortAscending, int? displayTypeId);
        
        [OperationContract]
        void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId);
        
        [OperationContract]
        void SortedList_Copy(int projectId, List<string> items);

        [OperationContract]
        void SortedList_Move(int projectId, List<string> items);
        
        [OperationContract]
        void SortedList_Export(int destProjectId, List<string> items);

        [OperationContract]
        int? SortedList_FocusOn(int projectId, int artifactId, bool clearFilters);
     }
}
