using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Allows ajax code to send email notifications and subscribe to artifacts
    /// </summary>
    [
    ServiceContract(Name = "NotificationService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
	public interface INotificationService
	{
		[OperationContract]
		void SendMailNotificationToUser(int toUserId, int projId, int artifactTypeId, int artifactId, string subject = "This item requires your attention!");

		[OperationContract]
        void SendMailNotificationToEmail(string toEmailAddresses, int projId, int artifactTypeId, int artifactId, string subject = "This item requires your attention!");

        [OperationContract]
        void Notification_SubscribeToArtifact(int projectId, int artifactTypeId, int artifactId);

        [OperationContract]
        void Notification_UnsubscribeFromArtifact(int projectId, int artifactTypeId, int artifactId);
        
        [OperationContract]
        void Notification_SubscribeSelectedUserToArtifact(int projectId, int artifactTypeId, int artifactId, int selectedUserId);
        
        [OperationContract]
        void Notification_UnsubscribeSelectedUserFromArtifact(int projectId, int artifactTypeId, int artifactId, int selectedUserId);

        [OperationContract]
        List<FollowerModel> Notification_FollowersOfArtifact(int projectId, int artifactTypeId, int artifactId);

        [OperationContract]
        bool Notification_IsUserSubscribed(int projectId, int artifactTypeId, int artifactId);
    }
}
