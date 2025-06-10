using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [
    ServiceContract(Name = "IHierarchicalDocument", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IHierarchicalDocument
    {
        [OperationContract]
        HierarchicalData HierarchicalDocument_Retrieve(int projectId, int startRow, int numberRows, int? parentRequirementId = null);

		[OperationContract]
		List<ValidationMessage> HierarchicalDocument_Save(int projectId, HierarchicalDataItem dataItem);

		[OperationContract]
		void HierarchicalDocument_ToggleFieldVisibility(int projectId, string fieldName);
	}
}
