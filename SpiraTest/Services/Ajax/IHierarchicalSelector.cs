using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [
    ServiceContract(Name = "IHierarchicalSelector", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IHierarchicalSelector
    {
        [OperationContract]
        List<HierarchicalDataItem> HierarchicalSelector_RetrieveAvailable(int projectId, string indentLevel);
    }
}
