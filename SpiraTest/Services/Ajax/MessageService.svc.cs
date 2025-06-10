using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Instant-Messenger related AJAX web services
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class MessageService : AjaxWebServiceBase, IMessageService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.MessageService::";

        #region IMessageService methods

        /// <summary>
        /// Marks all the messages in the current conversation as read
        /// </summary>
        /// <param name="senderUserId">The id of the user I'm conversing with</param>
        public void Message_MarkAllAsRead(int senderUserId)
        {
            const string METHOD_NAME = "Message_MarkAllAsRead";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get all the unread messages from that person
                MessageManager messageManager = new MessageManager();
                List<Message> messages = messageManager.Message_RetrieveUnread(userId);
                foreach (Message message in messages)
                {
                    //If it's from the specified sender, mark as unread
                    if (message.SenderUserId == senderUserId)
                    {
                        messageManager.Message_MarkAsRead(message.MessageId);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Posts the selected messages to the specific artifact
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="artifactTypeId">The type of artifact</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <param name="messageIds">The list of messages to post</param>
        public void Message_PostArtifactComments(int projectId, int artifactTypeId, int artifactId, List<int> messageIds)
        {
            const string METHOD_NAME = "Message_PostArtifactComments";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            if (messageIds == null || messageIds.Count < 1)
            {
                //Ignore, no messages
                return;
            }

            try
            {
                MessageManager messageManager = new MessageManager();
                List<long> validatedMessageIds = new List<long>();
                //We need to validate that the messages are sent/received from this user and that it exists
                foreach (int messageId in messageIds)
                {
                    try
                    {
                        Message message = messageManager.Message_RetrieveById(messageId);
                        if (message != null && (message.RecipientUserId == userId || message.SenderUserId == userId))
                        {
                            validatedMessageIds.Add(messageId);
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }
                }
                messageManager.Message_PostToArtifact(projectId, (Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, validatedMessageIds);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Posts a new instant message
        /// </summary>
        /// <param name="recipientUserId">The id of the user receiving the message</param>
        /// <param name="message">The body of the message</param>
        public void Message_PostNew(int recipientUserId, string message)
        {
            const string METHOD_NAME = "Message_PostNew";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Make sure the user exists and is active
                User user = new UserManager().GetUserById(recipientUserId);
                if (user != null)
                {
                    new MessageManager().Message_Create(userId, recipientUserId, message);
                }
            }
            catch (ArtifactNotExistsException)
            {
                //Ignore
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of senders that this user has unread messages for, with the number of unread messages for each
        /// </summary>
        /// <returns>Dictionary of users/unread</returns>
        public JsonDictionaryOfIntegers Message_GetUnreadMessageSenders()
        {
            const string METHOD_NAME = "Message_GetUnreadMessageSenders";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int recipientUserId = this.CurrentUserId.Value;

            try
            {
                JsonDictionaryOfIntegers senderUsers = new JsonDictionaryOfIntegers();

                //Get the list of unread messages for this user
                List<Message> unreadMessages = new MessageManager().Message_RetrieveUnread(recipientUserId);
                foreach (Message message in unreadMessages)
                {
                    if (senderUsers.ContainsKey(message.SenderUserId.ToString()))
                    {
                        senderUsers[message.SenderUserId.ToString()] = senderUsers[message.SenderUserId.ToString()] + 1;
                    }
                    else
                    {
                        senderUsers.Add(message.SenderUserId.ToString(), 1);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return senderUsers;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the full name of a specific user (used in popup messages)
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>The name</returns>
        public string Message_GetUserName(int userId)
        {
            const string METHOD_NAME = "Message_GetUserName";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }

            try
            {
                User user = new UserManager().GetUserById(userId);
                if (user != null && user.Profile != null)
                {
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return user.Profile.FullName;
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return "";
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the latest unread message for the current user
        /// </summary>
        /// <param name="recipientUserId">The id of the person we're conversing with</param>
        /// <returns>Latest message in the thread</returns>
        public CommentItem Message_RetrieveLatestUnread()
        {
            const string METHOD_NAME = "Message_RetrieveLatestUnread";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get a list of the messages between the two users
                MessageManager messageManager = new MessageManager();
                Message latestMessage = messageManager.Message_RetrieveLatestUnread(userId);

                if (latestMessage == null)
                {
                    return null;
                }

                //Return the API data object
                CommentItem comment = new CommentItem();
                comment.creationDate = GlobalFunctions.LocalizeDate(latestMessage.CreationDate);
                comment.creationDateText = GlobalFunctions.LocalizeDate(latestMessage.CreationDate).ToNiceString(GlobalFunctions.LocalizeDate(DateTime.UtcNow));
                comment.primaryKey = (int)latestMessage.MessageId;
                comment.text = latestMessage.Body;
                comment.creatorId = latestMessage.SenderUserId;
                comment.creatorName = latestMessage.SenderName;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return comment;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the list of online users and the number of unread messages for the current user
        /// </summary>
        /// <returns>list of online users and unread message count</returns>
        public MessageInfo Message_GetInfo()
        {
            const string METHOD_NAME = "Message_GetInfo";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                MessageInfo messageInfo = new DataObjects.MessageInfo();

                //Get the number of unread messages
                User user = new UserManager().GetUserById(userId);
                if (user.Profile != null)
                {
                    messageInfo.UnreadMessages = user.Profile.UnreadMessages;
                }

                //See if the user is in the hashtable of active user sessions
                messageInfo.OnlineUsers = Global.GetActiveUserIds();

                //Finally see if we need to purge old messages
                if (DateTime.UtcNow.AddDays(-ConfigurationSettings.Default.Message_RetentionPeriod) > ConfigurationSettings.Default.Message_LastPurgeAttempt)
                {
                    new MessageManager().Message_PurgeOld();
                    ConfigurationSettings.Default.Message_LastPurgeAttempt = DateTime.UtcNow;
                    ConfigurationSettings.Default.Save();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return messageInfo;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region ICommentService Methods

        /// <summary>
        /// Retrieves a list of messages
        /// </summary>
        /// <param name="projectId">Not used</param>
        /// <param name="recipientUserId">The id of the person we're conversing with</param>
        /// <returns>List of messages in the thread</returns>
        public List<CommentItem> Comment_Retrieve(int projectId, int recipientUserId)
        {
            const string METHOD_NAME = "Comment_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get a list of the messages between the two users
                MessageManager messageManager = new MessageManager();
                long count;
                List<Message> messages = messageManager.Message_Retrieve(userId, recipientUserId, 0, 9999, true, out count);

                //Return the API data objects, switch order to newest at the bottom
                List<CommentItem> comments = new List<CommentItem>();
                if (messages != null && messages.Count > 0)
                {
                    for (int i = messages.Count - 1; i >= 0; i--)
                    {
                        Message message = messages[i];
                        CommentItem comment = new CommentItem();
                        comment.creationDate = GlobalFunctions.LocalizeDate(message.CreationDate);
                        comment.creationDateText = GlobalFunctions.LocalizeDate(message.CreationDate).ToNiceString(GlobalFunctions.LocalizeDate(DateTime.UtcNow));
                        comment.primaryKey = (int)message.MessageId;
                        comment.text = message.Body;
                        comment.creatorId = message.SenderUserId;
                        comment.creatorName = message.SenderName;
                        comment.deleteable = (message.SenderUserId == userId);
                        //Mark as unread as long as we didn't send the message
                        comment.isUnread = (!message.IsRead && message.SenderUserId != userId);
                        comments.Add(comment);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return comments;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
        
        /// <summary>
        /// Deletes a message in a thread
        /// </summary>
        /// <param name="projectId">Not used</param>
        /// <param name="recipientUserId">The user we're conversing with</param>
        /// <param name="messageId">The id of the message to be deleted</param>
        public void Comment_Delete(int projectId, int recipientUserId, int messageId)
        {
            const string METHOD_NAME = "Comment_Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Retrieve the specified message
                MessageManager messageManager = new MessageManager();
                Message message = messageManager.Message_RetrieveById(messageId);
                if (message != null)
                {
                    //Make sure we're the sender
                    if (message.SenderUserId == userId)
                    {
                        messageManager.Message_Delete(messageId);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (ArtifactNotExistsException)
            {
                //Ignore
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Not used by this service
        /// </summary>
        public void Comment_UpdateSortDirection(int projectId, int sortDirectionId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not used by this service
        /// </summary>
        public int Comment_Add(int projectId, int artifactId, string comment)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
