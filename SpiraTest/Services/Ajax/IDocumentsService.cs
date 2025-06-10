using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side document management AJAX components
    /// </summary>
    [
    ServiceContract(Name = "DocumentsService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IDocumentsService : ITreeViewService, ISortedListService, INavigationService, IWorkflowOperationsService, IFormService, ICommentService, IItemSelectorService
    {
        [OperationContract]
        int UploadFile(int projectId, string filename, string description, int authorId, string encodedData, int? artifactId, int? artifactType, string version, string tags, int? documentTypeId, int? projectAttachmentFolderId);

        [OperationContract]
        int UploadUrl(int projectId, string url, string description, int authorId, int? artifactId, int? artifactType, string version, string tags, int? documentTypeId, int? projectAttachmentFolderId);

        [OperationContract]
        int Documents_Count(int projectId, List<ArtifactReference> artifacts);

        [OperationContract]
        byte[] Documents_OpenFile(int projectId, int attachmentId);

        [OperationContract]
        string Documents_OpenText(int projectId, int attachmentId);

        [OperationContract]
        string Documents_OpenMarkdown(int projectId, int attachmentId);

        [OperationContract]
        int Documents_CreateTextFile(int projectId, string filename, string description, int authorId, string version, string tags, int? documentTypeId, int? projectAttachmentFolderId, string format);
    }
}
