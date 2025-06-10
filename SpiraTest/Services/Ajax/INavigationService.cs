using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used by all services that are called by the NavigationBar Ajax control
    /// </summary>
    [
    ServiceContract(Name = "INavigationService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface INavigationService
    {
        [OperationContract]
        List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId);

        [OperationContract]
        void NavigationBar_UpdateSettings(int projectId, Nullable<int> displayMode, Nullable<int> displayWidth, Nullable<bool> minimized);

        [OperationContract]
        JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId);

        [OperationContract]
        void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage);
    }
}
