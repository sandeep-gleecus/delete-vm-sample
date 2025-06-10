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
    /// Provides the web service used to interacting with the various client-side document management AJAX components
    /// </summary>
    [
    ServiceContract(Name = "DocumentVersionService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    public interface IDocumentVersionService
    {
        [OperationContract]
        int DocumentVersion_CountVersions(int projectId, int attachmentId);

        [OperationContract]
        List<DataItem> DocumentVersion_RetrieveVersions(int projectId, int attachmentId);

        [OperationContract]
        void DocumentVersion_MakeActive(int projectId, int attachmentVersionId);

        [OperationContract]
        void DocumentVersion_Delete(int projectId, int attachmentVersionId);

        [OperationContract]
        int DocumentVersion_UploadFile(int projectId, int attachmentId, string filename, string description, string version, string encodedData, bool makeActive);
    }
}
