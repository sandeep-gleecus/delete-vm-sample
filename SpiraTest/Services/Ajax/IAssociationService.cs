using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side artifact association AJAX components
    /// </summary>
    [
    ServiceContract(Name = "AssociationService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IAssociationService : ISortedListService
    {
        [OperationContract]
        List<NameValue> Association_RetrieveForDestProjectAndArtifact(int projectId, List<int> artifactTypeIds, int? displayTypeId);

        [OperationContract]
        List<HierarchicalItem> Association_RetrieveArtifactFolders(int projectId, int artifactTypeId);

        [OperationContract]
        List<HierarchicalItem> Association_SearchByProjectAndArtifact(int projectId, int artifactTypeId, int artifactId, int searchArtifactTypeId, int? searchFolderId, int searchProjectId, string searchTerm);

        [OperationContract]
        void Association_Add(int projectId, int artifactTypeId, int artifactId, int displayType, int artifactLinkTypeId, string comment, List<ArtifactReference> selectionAssociations, int? existingItemId);

        [OperationContract]
        string Association_RetrieveTooltip(int projectId, int artifactTypeId, int artifactId);

        [OperationContract]
        int Association_Count(int projectId, ArtifactReference artifact, int displayTypeId);
    }
}
