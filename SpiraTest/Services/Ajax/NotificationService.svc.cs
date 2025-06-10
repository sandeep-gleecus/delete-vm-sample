using Inflectra.SpiraTest.Business;
using System.ServiceModel.Activation;
using Inflectra.SpiraTest.Common;
using System;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "NotificationService" in code, svc and config file together.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class NotificationService : AjaxWebServiceBase, INotificationService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.NotificationService::";

        /// <summary>
        /// Subscribes to the specified artifact
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactTypeId">The id of the artifact type</param>
        /// <param name="artifactId">The id of the artifact</param>
        public void Notification_SubscribeToArtifact(int projectId, int artifactTypeId, int artifactId)
        {
            const string METHOD_NAME = "Notification_SubscribeToArtifact";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Verify this user's not already subscribed and add him.
                NotificationManager notificationManager = new NotificationManager();
                if (!notificationManager.IsUserSubscribed(userId, artifactTypeId, artifactId))
                {
                    notificationManager.AddUserSubscription(userId, artifactTypeId, artifactId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Unsubscribes from the specified artifact
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactTypeId">The id of the artifact type</param>
        /// <param name="artifactId">The id of the artifact</param>
        public void Notification_UnsubscribeFromArtifact(int projectId, int artifactTypeId, int artifactId)
        {
            const string METHOD_NAME = "Notification_UnsubscribeFromArtifact";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Verify this user is already subscribed and remove him.
                NotificationManager notificationManager = new NotificationManager();
                if (notificationManager.IsUserSubscribed(userId, artifactTypeId, artifactId))
                {
                    notificationManager.RemoveUserSubscription(userId, artifactTypeId, artifactId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Unsubscribes from the specified artifact
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactTypeId">The id of the artifact type</param>
        /// <param name="artifactId">The id of the artifact</param>
        public void Notification_UnsubscribeSelectedUserFromArtifact(int projectId, int artifactTypeId, int artifactId, int selectedUserId)
        {
            const string METHOD_NAME = "Notification_UnsubscribeFromArtifact";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int currentUserId = this.CurrentUserId.Value;

            //Make sure we're fully authorized (limited is not OK - so that only users with higher level access can unsubscribe other users)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Verify this user is already subscribed and remove him/her.
                NotificationManager notificationManager = new NotificationManager();
                if (notificationManager.IsUserSubscribed(selectedUserId, artifactTypeId, artifactId))
                {
                    notificationManager.RemoveUserSubscription(selectedUserId, artifactTypeId, artifactId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }


        /// <summary>
        /// Subscribes a selected user to the specified artifact
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactTypeId">The id of the artifact type</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <param name="userId">The id of the project</param>
        public void Notification_SubscribeSelectedUserToArtifact(int projectId, int artifactTypeId, int artifactId, int selectedUserId)
        {
            const string METHOD_NAME = "Notification_SubscribeToArtifact";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int currentUserId = this.CurrentUserId.Value;

            //Make sure we're fully authorized (limited is not OK - so that only users with higher level access can subscribe other users)
            Project.AuthorizationState currentUserAuthorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
            if (currentUserAuthorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Make sure the selected user is authorized (limited is OK)
                Project.AuthorizationState selectedUserAuthorizationState = IsUserAuthorized(selectedUserId, projectId, Project.PermissionEnum.View, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
                if (selectedUserAuthorizationState == Project.AuthorizationState.Prohibited)
                {
                    throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.Services_UserSelectedNotAuthorized } });
                }
                try
                {
                    //Verify this user's not already subscribed and add him/her.
                    NotificationManager notificationManager = new NotificationManager();
                    if (!notificationManager.IsUserSubscribed(selectedUserId, artifactTypeId, artifactId))
                    {
                        notificationManager.AddUserSubscription(selectedUserId, artifactTypeId, artifactId);
                    }
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                }
                catch (Exception exception)
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                    throw;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>Sends the specified notification to the specified SpiraTest user.</summary>
        /// <param name="fromUserId">The user sending the notification.</param>
        /// <param name="toUserId">The user receiving the notification.</param>
        /// <param name="artifactTypeId">The type of the artifact. Number from DataModel.Artifact.ArtifactTypeEnum.*</param>
        /// <param name="artifactId">The number of the artifact.</param>
        /// <param name="projId">The project number of the Artifact.</param>
        /// <param name="subject">An optional subject line to send.</param>
        void INotificationService.SendMailNotificationToUser(int toUserId, int projId, int artifactTypeId, int artifactId, string subject)
        {
            const string METHOD_NAME = "SendMailNotificationToUser";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int fromUserId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projId, Project.PermissionEnum.View, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(subject))
                    new NotificationManager().SendNotification(projId, artifactId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, fromUserId, toUserId);
                else
                    new NotificationManager().SendNotification(projId, artifactId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, fromUserId, toUserId, subject);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>Sends a specified artifact notification to the e-mail addresses provided.</summary>
        /// <param name="fromUserId">The user sending the notification.</param>
        /// <param name="artifactTypeId">The type of the artifact. Number from DataModel.Artifact.ArtifactTypeEnum.*</param>
        /// <param name="artifactId">The number of the artifact.</param>
        /// <param name="projId">The project number of the Artifact.</param>
        /// <param name="subject">An optional subject line to send.</param>
        /// <param name="toEmailAddresses">A list of email addresses to send to, separated by semicolons (;).</param>
        void INotificationService.SendMailNotificationToEmail(string toEmailAddresses, int projId, int artifactTypeId, int artifactId, string subject)
        {
            const string METHOD_NAME = "SendMailNotificationToEmail";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int fromUserId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projId, Project.PermissionEnum.View, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            } 
            
            try
            {
                if (string.IsNullOrWhiteSpace(subject))
                    new NotificationManager().SendNotification(projId, artifactId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, fromUserId, toEmailAddresses);
                else
                    new NotificationManager().SendNotification(projId, artifactId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, fromUserId, toEmailAddresses, subject);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns whether the current user is subscribed to the specified artifact
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactTypeId">The id of the artifact type</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <returns>true = subscribed</returns>
        public bool Notification_IsUserSubscribed(int projectId, int artifactTypeId, int artifactId)
        {
            const string METHOD_NAME = "Notification_IsUserSubscribed";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                // See if we're subscribed
                bool isSubscribed = new NotificationManager().IsUserSubscribed(userId, artifactTypeId, artifactId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return isSubscribed;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves list of followers / subscribers for specified artifact id
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactTypeId">The id of the artifact type</param>
        /// <param name="artifactId">The id of the artifact</param>
        public List<FollowerModel> Notification_FollowersOfArtifact(int projectId, int artifactTypeId, int artifactId)
        {
            const string METHOD_NAME = "Notification_FollowersOfArtifact";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
               throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
               throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                // Retrieve the list of subscriptions for specified artifact.
                List<NotificationUserSubscription> notificationUserSubscription = new NotificationManager().RetrieveSubscriptionsForArtifact(artifactTypeId, artifactId);

                //Create a simplified user list to add releveant data from the above into
                List<FollowerModel> followers = new List<FollowerModel>();

                // iterate through all returns users in the list
                foreach (NotificationUserSubscription user in notificationUserSubscription)
                {
                    //Create the data-item
                    FollowerModel follower = new FollowerModel();

                    //Populate the necessary fields
                    follower.UserId = user.UserId;
                    follower.FullName = user.User.FullName;
                    follower.HasIcon = !string.IsNullOrEmpty(user.User.Profile.AvatarImage);
                    follower.FirstName = user.User.Profile.FirstName;
                    follower.LastName = user.User.Profile.LastName;
                    follower.Department = user.User.Profile.Department;

                    followers.Add(follower);
                }
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                return followers;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }

    [DataContract(Namespace = "tst.dataObjects")]
    public class FollowerModel
    {
        [DataMember(Name = "userId")]
        public int UserId;

        [DataMember(EmitDefaultValue = false, Name="fullName")]
        public string FullName;

        [DataMember(EmitDefaultValue = false, Name="firstName")]
        public string FirstName;

        [DataMember(EmitDefaultValue = false, Name="lastName")]
        public string LastName;

        [DataMember(EmitDefaultValue = false, Name = "department")]
        public string Department;

        [DataMember(EmitDefaultValue = false, Name="hasIcon")]
        public bool HasIcon;
    }
}
