using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the HierarchicalGrid AJAX component for displaying/updating release data
    /// </summary>
    [
    ServiceContract(Name = "ReleasesService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IReleasesService : IHierarchicalListService, IAssociationPanelService, INavigationService, IFormService, ICommentService, IWorkflowOperationsService
    {
        [OperationContract]
        string Release_RetrieveAsDotNotation(int projectId, int? numberOfLevels);
    }
}
