using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using NUnit.Framework;
using System.Security.Cryptography;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the MessageManager class
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class MessageManagerTest
	{
		private static MessageManager messageManager;
		private static UserManager userManager;
		private static int projectId;
		private static int projectTemplateId;
		private static int userId1;
		private static int userId2;

		private const int USER_ID_SYSTEM_ADMIN = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			//Create a new project and two users for our testing and instantiate the message manager class being tested
			messageManager = new MessageManager();
			ProjectManager projectManager = new ProjectManager();
			userManager = new UserManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("MessageManagerTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");


			string adminSectionName1 = "View / Edit Users";
			var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId1 = adminSection1.ADMIN_SECTION_ID;

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			string salt = GenerateSalt();
			int errorCode;
			userId1 = userManager.CreateUser("testuser1", "mypassword123", salt, "testuser1@spiratest.com", "What is my first dog named?", "crusher", true, false, 1, null, null, null, out errorCode, null, adminSectionId1, "Inserted User", 1).UserId;
			userId2 = userManager.CreateUser("testuser2", "mypassword123", salt, "testuser2@spiratest.com", "What is my first dog named?", "crusher", true, false, 1, null, null, null, out errorCode, null, adminSectionId1, "Inserted User", 1).UserId;
			//Verify that there were no errors
			Assert.AreEqual(0, errorCode);

			//Add profiles
			User user = userManager.GetUserById(userId1);
			user.Profile = new UserProfile();
			user.Profile.FirstName = "Test";
			user.Profile.LastName = "User 1";
			user.Profile.IsAdmin = false;
			user.Profile.IsEmailEnabled = true;
			user.Profile.Department = "Dept1";
			user.Profile.LastUpdateDate = DateTime.UtcNow;
			userManager.Update(user);

			user = userManager.GetUserById(userId2);
			user.Profile = new UserProfile();
			user.Profile.FirstName = "Test";
			user.Profile.LastName = "User 2";
			user.Profile.IsAdmin = false;
			user.Profile.IsEmailEnabled = true;
			user.Profile.Department = "Dept1";
			user.Profile.LastUpdateDate = DateTime.UtcNow;
			userManager.Update(user);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYSTEM_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId);

			//Delete the newly created users
			userManager.DeleteUser(userId1, true);
			userManager.DeleteUser(userId2, true);
		}

		#region Test functions

		/// <summary>
		/// Tests that we can create instant messages between two users
		/// </summary>
		[Test]
		[SpiraTestCase(1275)]
		public void _01_CreateMessages()
		{
			//First lets create a message from User1 > User2
			messageManager.Message_Create(userId1, userId2, "What do you think about the function XYZ?");

			//Now lets create a reply back from User2 > User1
			messageManager.Message_Create(userId2, userId1, "I think it needs to be refactored!");

			//Finally lets create a reply back from the original sender
			messageManager.Message_Create(userId1, userId2, "OK, I'll make the change");

			//Verify that we can retrieve the list of messages sent by a user to another user (no replies)
			long messageCount;
			List<Message> messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, false, out messageCount);
			Assert.AreEqual(2, messageCount);
			Assert.AreEqual(2, messages.Count);
			Assert.AreEqual("OK, I'll make the change", messages[0].Body);
			Assert.AreEqual("Test User 1", messages[0].SenderName);
			Assert.AreEqual("Test User 2", messages[0].RecipientName);

			//Similarly for the other user
			messages = messageManager.Message_Retrieve(userId2, userId1, 0, 999, false, out messageCount);
			Assert.AreEqual(1, messageCount);
			Assert.AreEqual(1, messages.Count);
			Assert.AreEqual("I think it needs to be refactored!", messages[0].Body);
			Assert.AreEqual("Test User 2", messages[0].SenderName);
			Assert.AreEqual("Test User 1", messages[0].RecipientName);

			//Verify that we can retrieve the list of messages sent by a user to another user (including replies)
			messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.AreEqual(3, messageCount);
			Assert.AreEqual(3, messages.Count);
			Assert.AreEqual("OK, I'll make the change", messages[0].Body);
			Assert.AreEqual("I think it needs to be refactored!", messages[1].Body);
			Assert.AreEqual("What do you think about the function XYZ?", messages[2].Body);

			//Similarly for the other user
			messages = messageManager.Message_Retrieve(userId2, userId1, 0, 999, true, out messageCount);
			Assert.AreEqual(3, messageCount);
			Assert.AreEqual(3, messages.Count);
			Assert.AreEqual("OK, I'll make the change", messages[0].Body);
			Assert.AreEqual("I think it needs to be refactored!", messages[1].Body);
			Assert.AreEqual("What do you think about the function XYZ?", messages[2].Body);

			//Verify that the user profiles show the correct # unread messages
			User user = userManager.GetUserById(userId1);
			Assert.AreEqual(1, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(2, user.Profile.UnreadMessages);

			//Now purge the messages to clean up for the next test
			messageManager.Message_PurgeOld(true, DateTime.UtcNow.AddDays(1));
		}

		/// <summary>
		/// Tests that we can retrieve messages with pagination
		/// </summary>
		[Test]
		[SpiraTestCase(1277)]
		public void _02_RetrieveWithPagination()
		{
			//Create some messages between two users
			messageManager.Message_Create(userId1, userId2, "Message 1");
			messageManager.Message_Create(userId1, userId2, "Message 2");
			messageManager.Message_Create(userId1, userId2, "Message 3");
			messageManager.Message_Create(userId1, userId2, "Message 4");
			messageManager.Message_Create(userId1, userId2, "Message 5");
			messageManager.Message_Create(userId1, userId2, "Message 6");
			messageManager.Message_Create(userId2, userId1, "Reply 1");
			messageManager.Message_Create(userId2, userId1, "Reply 2");
			messageManager.Message_Create(userId2, userId1, "Reply 3");
			messageManager.Message_Create(userId2, userId1, "Reply 4");
			messageManager.Message_Create(userId2, userId1, "Reply 5");
			messageManager.Message_Create(userId2, userId1, "Reply 6");

			//Retrieve the first page of messages
			const int PAGE_SIZE = 5;
			long messageCount;
			List<Message> messages = messageManager.Message_Retrieve(userId2, userId1, 0, PAGE_SIZE, true, out messageCount);
			Assert.AreEqual(12, messageCount);
			Assert.AreEqual(5, messages.Count);
			Assert.AreEqual("Reply 6", messages[0].Body);
			Assert.AreEqual("Reply 5", messages[1].Body);
			Assert.AreEqual("Reply 4", messages[2].Body);

			//Retrieve the second page of messages
			messages = messageManager.Message_Retrieve(userId2, userId1, PAGE_SIZE, PAGE_SIZE, true, out messageCount);
			Assert.AreEqual(12, messageCount);
			Assert.AreEqual(5, messages.Count);
			Assert.AreEqual("Reply 1", messages[0].Body);
			Assert.AreEqual("Message 6", messages[1].Body);
			Assert.AreEqual("Message 5", messages[2].Body);


			//Retrieve the third page of messages
			messages = messageManager.Message_Retrieve(userId2, userId1, (PAGE_SIZE * 2), PAGE_SIZE, true, out messageCount);
			Assert.AreEqual(12, messageCount);
			Assert.AreEqual(2, messages.Count);
			Assert.AreEqual("Message 2", messages[0].Body);
			Assert.AreEqual("Message 1", messages[1].Body);

			//Verify that the function can handle out of range pagination requests gracefully
			//(basically just assumes it's the same as PageIndex = 0)
			messages = messageManager.Message_Retrieve(userId2, userId1, 20, PAGE_SIZE, true, out messageCount);
			Assert.AreEqual(12, messageCount);
			Assert.AreEqual(5, messages.Count);
			Assert.AreEqual("Reply 6", messages[0].Body);
			Assert.AreEqual("Reply 5", messages[1].Body);
			Assert.AreEqual("Reply 4", messages[2].Body);

			messages = messageManager.Message_Retrieve(userId2, userId1, -5, PAGE_SIZE, true, out messageCount);
			Assert.AreEqual(12, messageCount);
			Assert.AreEqual(5, messages.Count);
			Assert.AreEqual("Reply 6", messages[0].Body);
			Assert.AreEqual("Reply 5", messages[1].Body);
			Assert.AreEqual("Reply 4", messages[2].Body);

			//Now purge the messages to clean up for the next test
			messageManager.Message_PurgeOld(true, DateTime.UtcNow.AddDays(1));
		}

		/// <summary>
		/// Tests that we can mark messages as read/unread
		/// </summary>
		[Test]
		[SpiraTestCase(1276)]
		public void _03_MarkAsReadUnRead()
		{
			//Create the sample messages
			messageManager.Message_Create(userId1, userId2, "What do you think about the function XYZ?");
			messageManager.Message_Create(userId2, userId1, "I think it needs to be refactored!");
			messageManager.Message_Create(userId1, userId2, "OK, I'll make the change");

			//Retrieve the list of messages sent by a user to another user (including replies)
			//Verify that they are all marked as 'unread'
			long messageCount;
			List<Message> messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.AreEqual(3, messageCount);
			Assert.AreEqual(3, messages.Count);
			Assert.IsFalse(messages[0].IsRead);
			Assert.IsFalse(messages[1].IsRead);
			Assert.IsFalse(messages[2].IsRead);

			//Verify that the user profiles show the correct # unread messages
			User user = userManager.GetUserById(userId1);
			Assert.AreEqual(1, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(2, user.Profile.UnreadMessages);

			//Now mark one of the messages as read
			messageManager.Message_MarkAsRead(messages[1].MessageId);

			//Verify the change
			messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.IsFalse(messages[0].IsRead);
			Assert.IsTrue(messages[1].IsRead);
			Assert.IsFalse(messages[2].IsRead);

			//Verify that the user profiles show the correct # unread messages
			user = userManager.GetUserById(userId1);
			Assert.AreEqual(0, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(2, user.Profile.UnreadMessages);

			//Mark another message as read
			messageManager.Message_MarkAsRead(messages[2].MessageId);

			//Verify the change
			messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.IsFalse(messages[0].IsRead);
			Assert.IsTrue(messages[1].IsRead);
			Assert.IsTrue(messages[2].IsRead);

			//Verify that the user profiles show the correct # unread messages
			user = userManager.GetUserById(userId1);
			Assert.AreEqual(0, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(1, user.Profile.UnreadMessages);

			//Now mark one of these messages as unread again
			messageManager.Message_MarkAsUnread(messages[1].MessageId);

			//Verify the change
			messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.IsFalse(messages[0].IsRead);
			Assert.IsFalse(messages[1].IsRead);
			Assert.IsTrue(messages[2].IsRead);

			//Verify that the user profiles show the correct # unread messages
			user = userManager.GetUserById(userId1);
			Assert.AreEqual(1, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(1, user.Profile.UnreadMessages);

			//Now purge the messages to clean up for the next test
			messageManager.Message_PurgeOld(true, DateTime.UtcNow.AddDays(1));
		}

		/// <summary>
		/// Tests that we can make updates to an existing message
		/// </summary>
		[Test]
		[SpiraTestCase(1273)]
		public void _04_UpdateMessages()
		{
			//Create the sample messages
			messageManager.Message_Create(userId1, userId2, "What do you think about the function XYZ?");
			messageManager.Message_Create(userId2, userId1, "I think it needs to be refactored!");
			messageManager.Message_Create(userId1, userId2, "OK, I'll make the change");

			//Verify the creation
			long messageCount;
			List<Message> messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.AreEqual(3, messageCount);
			Assert.AreEqual(3, messages.Count);

			//Verify that the user profiles show the correct # unread messages
			User user = userManager.GetUserById(userId1);
			Assert.AreEqual(1, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(2, user.Profile.UnreadMessages);

			//Retrieve a specific message
			Message message = messageManager.Message_RetrieveById(messages[0].MessageId);
			Assert.AreEqual("OK, I'll make the change", message.Body);

			//Mark as read and verify
			messageManager.Message_MarkAsRead(message.MessageId);
			message = messageManager.Message_RetrieveById(messages[0].MessageId);
			Assert.IsTrue(message.IsRead);

			//Verify that the user profiles show the correct # unread messages
			user = userManager.GetUserById(userId1);
			Assert.AreEqual(1, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(1, user.Profile.UnreadMessages);

			//Now make a change to it
			message = messageManager.Message_RetrieveById(messages[0].MessageId);
			message.StartTracking();
			message.Body = "Are you sure you want to make that change?";
			messageManager.Message_Update(message);

			//Verify the change, it will also be switched back to unread
			message = messageManager.Message_RetrieveById(messages[0].MessageId);
			Assert.AreEqual("Are you sure you want to make that change?", message.Body);
			Assert.IsFalse(message.IsRead);

			//Verify the list was updated
			messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.AreEqual(3, messageCount);
			Assert.AreEqual(3, messages.Count);
			Assert.AreEqual("Are you sure you want to make that change?", messages[0].Body);
			Assert.AreEqual("I think it needs to be refactored!", messages[1].Body);
			Assert.AreEqual("What do you think about the function XYZ?", messages[2].Body);

			//Verify that the user profiles show the correct # unread messages
			user = userManager.GetUserById(userId1);
			Assert.AreEqual(1, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(2, user.Profile.UnreadMessages);

			//Now purge the messages to clean up for the next test
			messageManager.Message_PurgeOld(true, DateTime.UtcNow.AddDays(1));
		}

		/// <summary>
		/// Tests that we can delete a specific message
		/// </summary>
		[Test]
		[SpiraTestCase(1274)]
		public void _05_DeleteMessages()
		{
			//Create the sample messages
			messageManager.Message_Create(userId1, userId2, "What do you think about the function XYZ?");
			messageManager.Message_Create(userId2, userId1, "I think it needs to be refactored!");
			messageManager.Message_Create(userId1, userId2, "OK, I'll make the change");

			//Verify the creation
			long messageCount;
			List<Message> messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.AreEqual(3, messageCount);
			Assert.AreEqual(3, messages.Count);

			//Verify that the user profiles show the correct # unread messages
			User user = userManager.GetUserById(userId1);
			Assert.AreEqual(1, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(2, user.Profile.UnreadMessages);

			//Now delete one of the messages
			long messageId = messages[2].MessageId;
			messageManager.Message_Delete(messageId);

			//Verify that the user profiles show the correct # unread messages
			user = userManager.GetUserById(userId1);
			Assert.AreEqual(1, user.Profile.UnreadMessages);
			user = userManager.GetUserById(userId2);
			Assert.AreEqual(1, user.Profile.UnreadMessages);

			//Verify that it was deleted, but can be retrieved when the 'includeDeleted' option is specified
			Message message = messageManager.Message_RetrieveById(messageId);
			Assert.IsNull(message);
			message = messageManager.Message_RetrieveById(messageId, true);
			Assert.IsNotNull(message);
			Assert.IsTrue(message.IsDeleted);

			//Verify that it doesn't show up in the list retrieves either
			messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.AreEqual(2, messageCount);
			Assert.AreEqual(2, messages.Count);

			//Now purge the messages to clean up for the next test
			messageManager.Message_PurgeOld(true, DateTime.UtcNow.AddDays(1));
		}

		/// <summary>
		/// Tests that we can post artifact comments from a selection of messages/replies
		/// </summary>
		[Test]
		[SpiraTestCase(1272)]
		public void _06_PostAsArtifactComment()
		{
			//Create the sample messages
			messageManager.Message_Create(userId1, userId2, "What do you think about the function XYZ?");
			messageManager.Message_Create(userId2, userId1, "I think it needs to be refactored!");
			messageManager.Message_Create(userId1, userId2, "OK, I'll make the change");

			//Verify the creation
			long messageCount;
			List<Message> messages = messageManager.Message_Retrieve(userId1, userId2, 0, 999, true, out messageCount);
			Assert.AreEqual(3, messageCount);
			Assert.AreEqual(3, messages.Count);

			//Create a new task in the project
			TaskManager taskManager = new TaskManager();
			int tk_priorityMediumId = taskManager.TaskPriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 3).TaskPriorityId;
			int taskId = taskManager.Insert(
				projectId,
				userId1,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				null,
				null,
				null,
				tk_priorityMediumId,
				"Test Task 1",
				"Description of test task",
				null,
				null,
				300,
				null,
				null,
				userId1);

			//Verify that it has no comments
			DiscussionManager discussionManager = new DiscussionManager();
			IEnumerable<IDiscussion> comments = discussionManager.Retrieve(taskId, Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(0, comments.Count());

			//Post two of the messages to a specific artifact
			List<long> messageIds = new List<long>();
			messageIds.Add(messages[2].MessageId);
			messageIds.Add(messages[1].MessageId);
			messageManager.Message_PostToArtifact(projectId, Artifact.ArtifactTypeEnum.Task, taskId, messageIds);

			//Verify that the comments were added
			List<IDiscussion> commentsList = discussionManager.Retrieve(taskId, Artifact.ArtifactTypeEnum.Task).ToList();
			Assert.AreEqual(2, commentsList.Count);
			Assert.AreEqual(messages[2].SenderName, commentsList[0].CreatorName);
			Assert.AreEqual(messages[2].Body, commentsList[0].Text);
			Assert.AreEqual(messages[1].SenderName, commentsList[1].CreatorName);
			Assert.AreEqual(messages[1].Body, commentsList[1].Text);

			//Now purge the messages to clean up for the next test
			messageManager.Message_PurgeOld(true, DateTime.UtcNow.AddDays(1));
		}

		#endregion

		#region Internal Helper Functions

		/// <summary>
		/// Generates the SALT
		/// </summary>
		/// <returns></returns>
		private static string GenerateSalt()
		{
			byte[] data = new byte[0x10];
			new RNGCryptoServiceProvider().GetBytes(data);
			return Convert.ToBase64String(data);
		}

		#endregion
	}
}
