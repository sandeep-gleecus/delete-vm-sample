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
    /// Used by services that are called by the ItemSelector Ajax control if they don't want to use their normal retrieve method
    /// </summary>
    [
    ServiceContract(Name = "IItemSelectorService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IItemSelectorService
    {
        [OperationContract]
        ItemSelectorData ItemSelector_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters);
    }
}
