using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// All AJAX web services that are called by the CommentList client component need to implement this interface
    /// </summary>
    [ServiceContract(Name = "ICommentService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    interface ICommentService
    {
        [OperationContract]
        List<CommentItem> Comment_Retrieve(int projectId, int artifactId);

        [OperationContract]
        void Comment_UpdateSortDirection(int projectId, int sortDirectionId);

        [OperationContract]
        void Comment_Delete(int projectId, int artifactId, int commentId);

        [OperationContract]
        int Comment_Add(int projectId, int artifactId, string comment);
    }
}
