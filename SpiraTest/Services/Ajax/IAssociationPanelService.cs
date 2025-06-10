using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used by all services that are called by the MappingSelector Ajax control
    /// </summary>
    [
    ServiceContract(Name = "IAssociationPanelService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IAssociationPanelService
    {        
        [OperationContract]
        string AssociationPanel_CreateNewLinkedItem(int projectId, int artifactId, int artifactTypeId, List<int> selectedItems, int? folderId);
    }
}
