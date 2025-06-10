using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// The interface for Instant-Messenger related AJAX web services
    /// </summary>
    [ServiceContract(Name = "MessageService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    interface IMessageService : ICommentService
    {
        [OperationContract]
        MessageInfo Message_GetInfo();

        [OperationContract]
        string Message_GetUserName(int userId);

        [OperationContract]
        void Message_PostNew(int recipientUserId, string message);

        [OperationContract]
        void Message_MarkAllAsRead(int senderUserId);

        [OperationContract]
        JsonDictionaryOfIntegers Message_GetUnreadMessageSenders();

        [OperationContract]
        void Message_PostArtifactComments(int projectId, int artifactTypeId, int artifactId, List<int> messageIds);

        [OperationContract]
        CommentItem Message_RetrieveLatestUnread();
    }
}
