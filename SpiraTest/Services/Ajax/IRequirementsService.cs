using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;


namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the HierarchicalGrid AJAX component for displaying/updating requirements data
    /// </summary>
    [
    ServiceContract(Name = "RequirementsService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IRequirementsService : IHierarchicalListService, IHierarchicalDocument, IAssociationPanelService, INavigationService, IFormService, ICommentService, IWorkflowOperationsService, IHierarchicalSelector
    {
        [OperationContract]
        string Requirement_RetrieveAsDotNotation(int projectId, int? numberOfLevels, bool includeAssociations, int? releaseId);

        [OperationContract]
        List<GraphEntry> Requirement_RetrieveTestCoverage(int projectId, int? releaseId, bool showRegressionCoverage);

        [OperationContract]
        List<GraphEntry> Requirement_RetrieveGroupTestCoverage(int projectGroupId, bool activeReleasesOnly);

        [OperationContract]
        GraphData Requirement_RetrieveBurndown(int projectId, int? releaseId);

        [OperationContract]
        int Requirement_Split(int projectId, int requirementId, string name, decimal? estimatePoints, int? ownerId, string comment);
	}
}
