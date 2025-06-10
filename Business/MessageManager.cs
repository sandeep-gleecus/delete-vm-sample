using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Data.Objects;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// Responsible for the storage of Instant Messages in the system
    /// </summary>
    public class MessageManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.MessageManager::";

        #region Public Methods

        /// <summary>
        /// Creates a new instant messsage in the system
        /// </summary>
        /// <param name="senderUserId">The id of the user that the message is from</param>
        /// <param name="recipientUserId">The id of the user that the message is to</param>
        /// <param name="body">The body of the message</param>
        /// <returns>The message object with its ID populated</returns>
        public Message Message_Create(int senderUserId, int recipientUserId, string body)
        {
            const string METHOD_NAME = "Message_Create";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Message message;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create the new instant message
                    message = new Message();
                    message.Body = body;
                    message.CreationDate = DateTime.UtcNow;
                    message.LastUpdateDate = DateTime.UtcNow;
                    message.IsRead = false;
                    message.IsDeleted = false;
                    message.SenderUserId = senderUserId;
                    message.RecipientUserId = recipientUserId;

                    //Save
                    context.Messages.AddObject(message);
                    context.SaveChanges();
                }

                //Also update the recipient's profile with the #unread messages
                this.UpdateUserUnreadCount(recipientUserId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return message;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves all of the unread messages for a specific recipient
        /// </summary>
        /// <param name="recipientUserId">The receiving user</param>
        /// <returns>The list of messages</returns>
        public List<Message> Message_RetrieveUnread(int recipientUserId)
        {
            const string METHOD_NAME = "Message_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<Message> messages;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from m in context.Messages
                                where m.RecipientUserId == recipientUserId && !m.IsDeleted && !m.IsRead
                                orderby m.CreationDate descending, m.MessageId
                                select m;

                    messages = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return messages;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves the latest unread message for a specific recipient
        /// </summary>
        /// <param name="recipientUserId">The receiving user</param>
        /// <returns>The latest messages</returns>
        public Message Message_RetrieveLatestUnread(int recipientUserId)
        {
            const string METHOD_NAME = "Message_RetrieveLatestUnread";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Message message;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from m in context.Messages
                                    .Include(m => m.Sender)
                                    .Include(m => m.Sender.Profile)
                                where m.RecipientUserId == recipientUserId && !m.IsDeleted && !m.IsRead
                                orderby m.CreationDate descending, m.MessageId
                                select m;

                    message = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return message;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves all of the messages between two users, including any replies if specified
        /// </summary>
        /// <param name="senderUserId">The sending user</param>
        /// <param name="recipientUserId">The receiving user</param>
        /// <param name="includeReplies">Should we include replies (where sender is actually the receiver)</param>
        /// <param name="numberOfRows">How many rows should we retrieve (used in pagination)</param>
        /// <param name="startIndex">The start index (used in pagination)</param>
        /// <param name="count">The total count of messages</param>
        /// <returns>The list of messages</returns>
        public List<Message> Message_Retrieve(int senderUserId, int recipientUserId, int startIndex, int numberOfRows, bool includeReplies, out long count)
        {
            const string METHOD_NAME = "Message_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<Message> messages;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First get the messages that were from SENDER > RECIPIENT
                    ObjectQuery<Message> baseQuery = context.Messages.Include("Sender").Include("Sender.Profile").Include("Recipient").Include("Recipient.Profile");
                    var query = from m in baseQuery
                                where m.SenderUserId == senderUserId && m.RecipientUserId == recipientUserId && !m.IsDeleted
                                select m;

                    if (includeReplies)
                    {
                        //Union any replies from RECIPIENT > SENDER
                        var query2 = from m in baseQuery
                                     where m.SenderUserId == recipientUserId && m.RecipientUserId == senderUserId && !m.IsDeleted
                                    select m;
                        query = query.Union(query2);
                    }

                    //Order by creation date descending
                    query = query.OrderByDescending(m => m.CreationDate).ThenBy(m => m.MessageId);

                    //Get the count
                    count = query.LongCount();

                    //Make pagination is in range
                    if (startIndex < 0 || startIndex > count - 1)
                    {
                        startIndex = 0;
                    }

                    //Execute the query
                    messages = query
                        .Skip(startIndex)
                        .Take(numberOfRows)
                        .ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return messages;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single message by its id
        /// </summary>
        /// <param name="includeDeleted">Include deleted</param>
        /// <param name="messageId">The id of the message</param>
        /// <returns>The message</returns>
        public Message Message_RetrieveById(long messageId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "Message_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Message message;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the message by its id
                    var query = from m in context.Messages.Include("Sender").Include("Sender.Profile").Include("Recipient").Include("Recipient.Profile")
                                where m.MessageId == messageId && (!m.IsDeleted || includeDeleted)
                                select m;

                    message = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return message;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Posts a set of messages as a comment to a specific artifact in a specific project
        /// </summary>
        /// <param name="messageIds">The list of messages to post</param>
        /// <param name="artifactType">The type of artifact to attach it to</param>
        /// <param name="artifactId">The id of the artifact to post it to</param>
        /// <param name="projectId">The id of the project</param>
        public void Message_PostToArtifact(int projectId, Artifact.ArtifactTypeEnum artifactType, int artifactId, List<long> messageIds)
        {
            const string METHOD_NAME = "Message_PostToArtifact";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //If we have a null or empty list just return
            if (messageIds == null || messageIds.Count < 1)
            {
                return;
            }

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the list of messages that have those IDs
                    var query = from m in context.Messages
                                where !m.IsDeleted && messageIds.Contains(m.MessageId)
                                orderby m.CreationDate, m.MessageId
                                select m;

                    //Get the list
                    List<Message> messages = query.ToList();
                    if (messages.Count > 0)
                    {
                        //Add to the artifact - have to handle incidents separately
                        if (artifactType == Artifact.ArtifactTypeEnum.Incident)
                        {
                            IncidentManager incidentManager = new IncidentManager();
                            foreach (Message message in messages)
                            {
                                incidentManager.InsertResolution(artifactId, message.Body, DateTime.UtcNow, message.SenderUserId,true);
                            }
                        }
                        else
                        {
                            DiscussionManager discussionManager = new DiscussionManager();
                            foreach (Message message in messages)
                            {
                                discussionManager.Insert(message.SenderUserId, artifactId, artifactType, message.Body, projectId, false, true);
                            }
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }              
        }

        /// <summary>
        /// Updates a specific user's profile with the latest count of unread messages for that user
        /// </summary>
        /// <param name="userId">The id of the user</param>
        protected void UpdateUserUnreadCount(int userId)
        {
            const string METHOD_NAME = "UpdateUserUnreadCount";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the list of unread messages received by the user
                    var query = from m in context.Messages
                                where !m.IsDeleted && !m.IsRead && m.RecipientUserId == userId
                                select m;

                    int unreadCount = query.Count();

                    //Now update the user's profile. We do it directly here for performance reasons
                    //rather than using the user manager
                    var query2 = from u in context.UserProfiles
                                where u.UserId == userId
                                select u;

                    UserProfile userProfile = query2.FirstOrDefault();
                    if (userProfile != null)
                    {
                        userProfile.StartTracking();
                        userProfile.LastUpdateDate = DateTime.UtcNow;
                        userProfile.UnreadMessages = unreadCount;
                        context.SaveChanges();
                    }
                }
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }  
        }

        /// <summary>
        /// Updates an instant message in the system
        /// </summary>
        /// <param name="message">The message to update</param>
        /// <remarks>Not currently being used because it could allow a user to 'rewrite history'</remarks>
        protected internal void Message_Update(Message message)
        {
            const string METHOD_NAME = "Message_Update";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //If we have a null entity just return
            if (message == null)
            {
                return;
            }

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Update the last-update date
                    message.StartTracking();
                    message.IsRead = false; //mark as unread since changed
                    message.LastUpdateDate = DateTime.UtcNow;

                    //Now apply the changes
                    context.Messages.ApplyChanges(message);

                    //Persist the changes
                    context.SaveChanges();
                }

                //Also update the recipient's profile with the #unread messages
                this.UpdateUserUnreadCount(message.RecipientUserId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }              
        }

        /// <summary>
        /// Deletes an instant message in the system
        /// </summary>
        /// <param name="messageId">The id of the message to delete</param>
        /// <remarks>Does a soft-delete only</remarks>
        public void Message_Delete(long messageId)
        {
            const string METHOD_NAME = "Message_Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int? recipientUserId = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the message by its id
                    var query = from m in context.Messages
                                where m.MessageId == messageId && !m.IsDeleted
                                select m;

                    Message message = query.FirstOrDefault();
                    if (message != null)
                    {
                        //Get the recipient
                        recipientUserId = message.RecipientUserId;

                        message.StartTracking();
                        message.IsDeleted = true;
                        message.LastUpdateDate = DateTime.UtcNow;
                        context.SaveChanges();
                    }
                }

                //Also update the recipient's profile with the #unread messages
                if (recipientUserId.HasValue)
                {
                    this.UpdateUserUnreadCount(recipientUserId.Value);
                }
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }              
        }

        /// <summary>
        /// Marks a message as Read
        /// </summary>
        /// <param name="messageId">The id of the message to mark</param>
        /// <remarks>Fails quietly if message no longer exists</remarks>
        public void Message_MarkAsRead(long messageId)
        {
            const string METHOD_NAME = "Message_MarkAsRead";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int? recipientUserId = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the message by its id
                    var query = from m in context.Messages
                                where m.MessageId == messageId && !m.IsDeleted
                                select m;

                    Message message = query.FirstOrDefault();
                    if (message != null)
                    {
                        //Get the recipient
                        recipientUserId = message.RecipientUserId;

                        message.StartTracking();
                        message.IsRead = true;
                        message.LastUpdateDate = DateTime.UtcNow;
                        context.SaveChanges();
                    }
                }

                //Also update the recipient's profile with the #unread messages
                if (recipientUserId.HasValue)
                {
                    this.UpdateUserUnreadCount(recipientUserId.Value);
                }
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }
        }

        /// <summary>
        /// Marks a message as Unread
        /// </summary>
        /// <param name="messageId">The id of the message to mark</param>
        /// <remarks>Fails quietly if message no longer exists</remarks>
        public void Message_MarkAsUnread(long messageId)
        {
            const string METHOD_NAME = "Message_MarkAsUnread";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int? recipientUserId = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the message by its id
                    var query = from m in context.Messages
                                where m.MessageId == messageId && !m.IsDeleted
                                select m;

                    Message message = query.FirstOrDefault();
                    if (message != null)
                    {
                        //Get the recipient
                        recipientUserId = message.RecipientUserId;

                        message.StartTracking();
                        message.IsRead = false;
                        message.LastUpdateDate = DateTime.UtcNow;
                        context.SaveChanges();
                    }
                }

                //Also update the recipient's profile with the #unread messages
                if (recipientUserId.HasValue)
                {
                    this.UpdateUserUnreadCount(recipientUserId.Value);
                }
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }
        }

        /// <summary>
        /// Purges all old read or deleted messages that are older than the configured retention period
        /// </summary>
        /// <param name="includeUnread">Should it also include unread messages</param>
        /// <param name="overridePurgeDate">Specifies an override purge date, otherwise the retention period in settings is used instead</param>
        /// <remarks>
        /// Unlike the Delete function this is a physical delete (to avoid space issues). Also we don't update users' profiles with the
        /// UnreadMessages value since it doesn't purge unread messages in most cases.
        /// </remarks>
        public void Message_PurgeOld(bool includeUnread = false, DateTime? overridePurgeDate = null)
        {
            const string METHOD_NAME = "Message_PurgeOld";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Calculate the purge date
                DateTime purgeDate;
                if (overridePurgeDate.HasValue)
                {
                    purgeDate = overridePurgeDate.Value;
                }
                else
                {
                    purgeDate = DateTime.UtcNow.AddDays(-Common.ConfigurationSettings.Default.Message_RetentionPeriod);
                }

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Call the stored procedure to do the delete
                    context.Message_PurgeOld(purgeDate, includeUnread);
                }
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
            }              
        }

        #endregion
    }
}
