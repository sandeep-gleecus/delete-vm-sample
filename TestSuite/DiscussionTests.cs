using System;
using System.Linq;
using System.Collections.Generic;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

using NUnit.Framework;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Notification business object
	/// </summary>
	[TestFixture]
	[SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)]
	public class DiscussionTest
	{
		private static int requirementDiscId = -1;
		private static int taskDiscId = -1;
		private static int releaseDiscId = -1;
		private static int testCaseDiscId = -1;
		private static int testSetDiscId = -1;
		private static int documentDiscId = -1;
		private static int riskDiscId = -1;
		private static int projectId;
		private static int projectTemplateId;

		private const string MESSAGE_TEXT = "New message text!";
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			//Create a new project, used by some of the tests
			ProjectManager projectManager = new ProjectManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("DiscussionTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}


		[Test]
		[SpiraTestCase(717)]
		[Description("Creates five new discussion messages for the first artifact of each type.")]
		public void _101_CreateNew()
		{
			DiscussionManager discussionManager = new DiscussionManager();
			requirementDiscId = discussionManager.Insert(USER_ID_SYS_ADMIN, 1, DataModel.Artifact.ArtifactTypeEnum.Requirement, MESSAGE_TEXT, DateTime.UtcNow, 1, false, false);
			taskDiscId = discussionManager.Insert(USER_ID_SYS_ADMIN, 1, DataModel.Artifact.ArtifactTypeEnum.Task, MESSAGE_TEXT, DateTime.UtcNow, 1, false, false);
			releaseDiscId = discussionManager.Insert(USER_ID_SYS_ADMIN, 1, DataModel.Artifact.ArtifactTypeEnum.Release, MESSAGE_TEXT, DateTime.UtcNow, 1, false, false);
			testCaseDiscId = discussionManager.Insert(USER_ID_SYS_ADMIN, 2, DataModel.Artifact.ArtifactTypeEnum.TestCase, MESSAGE_TEXT, DateTime.UtcNow, 1, false, false);
			testSetDiscId = discussionManager.Insert(USER_ID_SYS_ADMIN, 1, DataModel.Artifact.ArtifactTypeEnum.TestSet, MESSAGE_TEXT, DateTime.UtcNow, 1, false, false);
			documentDiscId = discussionManager.Insert(USER_ID_SYS_ADMIN, 1, DataModel.Artifact.ArtifactTypeEnum.Document, MESSAGE_TEXT, DateTime.UtcNow, 1, false, false);
			riskDiscId = discussionManager.Insert(USER_ID_SYS_ADMIN, 1, DataModel.Artifact.ArtifactTypeEnum.Risk, MESSAGE_TEXT, DateTime.UtcNow, 1, false, false);

			Assert.AreNotEqual(-1, requirementDiscId, "Requirement Discussion insert failed!");
			Assert.AreNotEqual(-1, taskDiscId, "Task Discussion insert failed!");
			Assert.AreNotEqual(-1, releaseDiscId, "Release Discussion insert failed!");
			Assert.AreNotEqual(-1, testCaseDiscId, "Test Case Discussion insert failed!");
			Assert.AreNotEqual(-1, testSetDiscId, "Test Set Discussion insert failed!");
			Assert.AreNotEqual(-1, documentDiscId, "Document Discussion insert failed!");
			Assert.AreNotEqual(-1, riskDiscId, "Risk Discussion insert failed!");
		}

		[
	   Test,
		SpiraTestCase(718),
		Description("Grabs each of the newly inserted discussions and verifies their data.")
	   ]
		public void _102_VerifyNew()
		{
			DiscussionManager discussionManager = new DiscussionManager();
			IDiscussion disRQ = discussionManager.RetrieveById(requirementDiscId, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			IDiscussion disTK = discussionManager.RetrieveById(taskDiscId, DataModel.Artifact.ArtifactTypeEnum.Task);
			IDiscussion disRL = discussionManager.RetrieveById(releaseDiscId, DataModel.Artifact.ArtifactTypeEnum.Release);
			IDiscussion disTC = discussionManager.RetrieveById(testCaseDiscId, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			IDiscussion disTX = discussionManager.RetrieveById(testSetDiscId, DataModel.Artifact.ArtifactTypeEnum.TestSet);
			IDiscussion disDC = discussionManager.RetrieveById(documentDiscId, DataModel.Artifact.ArtifactTypeEnum.Document);
			IDiscussion disRK = discussionManager.RetrieveById(riskDiscId, DataModel.Artifact.ArtifactTypeEnum.Risk);

			//Requirement discussion.
			Assert.AreEqual(MESSAGE_TEXT, disRQ.Text, "RQ - TEXT did not match.");
			Assert.AreEqual(requirementDiscId, disRQ.DiscussionId, "RQ - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, disRQ.CreatorId, "RQ - CREATOR_ID did not match.");
			Assert.AreEqual(1, disRQ.ArtifactId, "RQ - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, disRQ.IsDeleted, "RQ - DELETED did not match.");
			//Task discussion.
			Assert.AreEqual(MESSAGE_TEXT, disTK.Text, "TK - TEXT did not match.");
			Assert.AreEqual(taskDiscId, disTK.DiscussionId, "TK - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, disTK.CreatorId, "TK - CREATOR_ID did not match.");
			Assert.AreEqual(1, disTK.ArtifactId, "TK - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, disTK.IsDeleted, "TK - DELETED did not match.");
			//Release discussion.
			Assert.AreEqual(MESSAGE_TEXT, disRL.Text, "RL - TEXT did not match.");
			Assert.AreEqual(releaseDiscId, disRL.DiscussionId, "RL - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, disRL.CreatorId, "RL - CREATOR_ID did not match.");
			Assert.AreEqual(1, disRL.ArtifactId, "RL - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, disRL.IsDeleted, "RL - DELETED did not match.");
			//Test Case discussion.
			Assert.AreEqual(MESSAGE_TEXT, disTC.Text, "TC - TEXT did not match.");
			Assert.AreEqual(testCaseDiscId, disTC.DiscussionId, "TC - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, disTC.CreatorId, "TC - CREATOR_ID did not match.");
			Assert.AreEqual(2, disTC.ArtifactId, "TC - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, disTC.IsDeleted, "TC - DELETED did not match.");
			//Test Set discussion.
			Assert.AreEqual(MESSAGE_TEXT, disTX.Text, "TX - TEXT did not match.");
			Assert.AreEqual(testSetDiscId, disTX.DiscussionId, "TX - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, disTX.CreatorId, "TX - CREATOR_ID did not match.");
			Assert.AreEqual(1, disTX.ArtifactId, "TX - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, disTX.IsDeleted, "TX - DELETED did not match.");
			//Document discussion.
			Assert.AreEqual(MESSAGE_TEXT, disDC.Text, "DC - TEXT did not match.");
			Assert.AreEqual(documentDiscId, disDC.DiscussionId, "DC - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, disDC.CreatorId, "DC - CREATOR_ID did not match.");
			Assert.AreEqual(1, disDC.ArtifactId, "DC - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, disDC.IsDeleted, "DC - DELETED did not match.");
			//Risk discussion.
			Assert.AreEqual(MESSAGE_TEXT, disRK.Text, "RK - TEXT did not match.");
			Assert.AreEqual(riskDiscId, disRK.DiscussionId, "RK - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, disRK.CreatorId, "RK - CREATOR_ID did not match.");
			Assert.AreEqual(1, disRK.ArtifactId, "RK - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, disRK.IsDeleted, "RK - DELETED did not match.");

			//Now verify that we can retrieve the list of discussions for a particular artifact
			IEnumerable<IDiscussion> disRQs = discussionManager.Retrieve(1, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			IEnumerable<IDiscussion> disTKs = discussionManager.Retrieve(1, DataModel.Artifact.ArtifactTypeEnum.Task);
			IEnumerable<IDiscussion> disRLs = discussionManager.Retrieve(1, DataModel.Artifact.ArtifactTypeEnum.Release);
			IEnumerable<IDiscussion> disTCs = discussionManager.Retrieve(2, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			IEnumerable<IDiscussion> disTXs = discussionManager.Retrieve(1, DataModel.Artifact.ArtifactTypeEnum.TestSet);
			IEnumerable<IDiscussion> disDCs = discussionManager.Retrieve(1, DataModel.Artifact.ArtifactTypeEnum.Document);
			IEnumerable<IDiscussion> disRKs = discussionManager.Retrieve(1, DataModel.Artifact.ArtifactTypeEnum.Risk);

			//Requirement discussion.
			IDiscussion discRow = disRQs.Last();
			Assert.AreEqual(MESSAGE_TEXT, discRow.Text, "RQ - TEXT did not match.");
			Assert.AreEqual(requirementDiscId, discRow.DiscussionId, "RQ - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, discRow.CreatorId, "RQ - CREATOR_ID did not match.");
			Assert.AreEqual(1, discRow.ArtifactId, "RQ - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, discRow.IsDeleted, "RQ - DELETED did not match.");

			//Task discussion.
			discRow = disTKs.Last();
			Assert.AreEqual(MESSAGE_TEXT, discRow.Text, "TK - TEXT did not match.");
			Assert.AreEqual(taskDiscId, discRow.DiscussionId, "TK - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, discRow.CreatorId, "TK - CREATOR_ID did not match.");
			Assert.AreEqual(1, discRow.ArtifactId, "TK - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, discRow.IsDeleted, "TK - DELETED did not match.");

			//Release discussion.
			discRow = disRLs.Last();
			Assert.AreEqual(MESSAGE_TEXT, discRow.Text, "RL - TEXT did not match.");
			Assert.AreEqual(releaseDiscId, discRow.DiscussionId, "RL - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, discRow.CreatorId, "RL - CREATOR_ID did not match.");
			Assert.AreEqual(1, discRow.ArtifactId, "RL - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, discRow.IsDeleted, "RL - DELETED did not match.");

			//Test Case discussion.
			discRow = disTCs.Last();
			Assert.AreEqual(MESSAGE_TEXT, discRow.Text, "TC - TEXT did not match.");
			Assert.AreEqual(testCaseDiscId, discRow.DiscussionId, "TC - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, discRow.CreatorId, "TC - CREATOR_ID did not match.");
			Assert.AreEqual(2, discRow.ArtifactId, "TC - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, discRow.IsDeleted, "TC - DELETED did not match.");

			//Test Set discussion.
			discRow = disTXs.Last();
			Assert.AreEqual(MESSAGE_TEXT, discRow.Text, "TX - TEXT did not match.");
			Assert.AreEqual(testSetDiscId, discRow.DiscussionId, "TX - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, discRow.CreatorId, "TX - CREATOR_ID did not match.");
			Assert.AreEqual(1, discRow.ArtifactId, "TX - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, discRow.IsDeleted, "TX - DELETED did not match.");

			//Document discussion.
			discRow = disDCs.Last();
			Assert.AreEqual(MESSAGE_TEXT, discRow.Text, "DC - TEXT did not match.");
			Assert.AreEqual(documentDiscId, discRow.DiscussionId, "DC - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, discRow.CreatorId, "DC - CREATOR_ID did not match.");
			Assert.AreEqual(1, discRow.ArtifactId, "DC - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, discRow.IsDeleted, "DC - DELETED did not match.");

			//Risk discussion.
			discRow = disRKs.Last();
			Assert.AreEqual(MESSAGE_TEXT, discRow.Text, "RK - TEXT did not match.");
			Assert.AreEqual(riskDiscId, discRow.DiscussionId, "RK - DISCUSSION_ID did not match.");
			Assert.AreEqual(1, discRow.CreatorId, "RK - CREATOR_ID did not match.");
			Assert.AreEqual(1, discRow.ArtifactId, "RK - ARTIFACT_ID did not match.");
			Assert.AreEqual(false, discRow.IsDeleted, "RK - DELETED did not match.");
		}

		[
		Test,
		SpiraTestCase(766),
		Description("Removes all the newly created discussions")
		]
		public void _103_DeleteDiscussions()
		{
			//Removes all the newly created discussions
			DiscussionManager discussion = new DiscussionManager();
			discussion.DeleteDiscussionId(requirementDiscId, DataModel.Artifact.ArtifactTypeEnum.Requirement, true, USER_ID_FRED_BLOGGS, projectId);
			discussion.DeleteDiscussionId(taskDiscId, DataModel.Artifact.ArtifactTypeEnum.Task, true, USER_ID_FRED_BLOGGS, projectId);
			discussion.DeleteDiscussionId(releaseDiscId, DataModel.Artifact.ArtifactTypeEnum.Release, true, USER_ID_FRED_BLOGGS, projectId);
			discussion.DeleteDiscussionId(testCaseDiscId, DataModel.Artifact.ArtifactTypeEnum.TestCase, true, USER_ID_FRED_BLOGGS, projectId);
			discussion.DeleteDiscussionId(testSetDiscId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true, USER_ID_FRED_BLOGGS, projectId);
			discussion.DeleteDiscussionId(documentDiscId, DataModel.Artifact.ArtifactTypeEnum.Document,true, USER_ID_FRED_BLOGGS, projectId);
			discussion.DeleteDiscussionId(riskDiscId, DataModel.Artifact.ArtifactTypeEnum.Risk, true, USER_ID_FRED_BLOGGS, projectId);
		}

		/// <summary>
		/// Tests that you can create permanent comments that cannot be deleted (used for system comments, signatures, etc.)
		/// </summary>
		[Test]
		[SpiraTestCase(1340)]
		public void _104_PersistentComments()
		{
			//Need to create some artifacts to test against
			int requirementId = new RequirementManager().Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement with Custom Properties", null, 120, USER_ID_FRED_BLOGGS);
			int taskId = new TaskManager().Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 1", "", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 60, null, null, USER_ID_FRED_BLOGGS);
			int testCaseId = new TestCaseManager().Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case with Custom Properties", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int releaseId = new ReleaseManager().Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release with Custom Properties", null, "1.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 2, 0, null, false);
			int testSetId = new TestSetManager().Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);

			//First lets create one permanent and one temporary comment per artifact type
			DiscussionManager discussionManager = new DiscussionManager();
			int requirementDiscId1 = discussionManager.Insert(USER_ID_SYS_ADMIN, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, MESSAGE_TEXT, DateTime.Now, 1, true, false);
			int requirementDiscId2 = discussionManager.Insert(USER_ID_SYS_ADMIN, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, MESSAGE_TEXT, DateTime.Now, 1, false, false);
			int taskDiscId1 = discussionManager.Insert(USER_ID_SYS_ADMIN, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, MESSAGE_TEXT, DateTime.Now, 1, true, false);
			int taskDiscId2 = discussionManager.Insert(USER_ID_SYS_ADMIN, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, MESSAGE_TEXT, DateTime.Now, 1, false, false);
			int releaseDiscId1 = discussionManager.Insert(USER_ID_SYS_ADMIN, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, MESSAGE_TEXT, DateTime.Now, 1, true, false);
			int releaseDiscId2 = discussionManager.Insert(USER_ID_SYS_ADMIN, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, MESSAGE_TEXT, DateTime.Now, 1, false, false);
			int testCaseDiscId1 = discussionManager.Insert(USER_ID_SYS_ADMIN, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, MESSAGE_TEXT, DateTime.Now, 1, true, false);
			int testCaseDiscId2 = discussionManager.Insert(USER_ID_SYS_ADMIN, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, MESSAGE_TEXT, DateTime.Now, 1, false, false);
			int testSetDiscId1 = discussionManager.Insert(USER_ID_SYS_ADMIN, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, MESSAGE_TEXT, DateTime.Now, 1, true, false);
			int testSetDiscId2 = discussionManager.Insert(USER_ID_SYS_ADMIN, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, MESSAGE_TEXT, DateTime.Now, 1, false, false);

			//Verify the flag was set correctly in both cases
			IDiscussion comment = discussionManager.RetrieveById(requirementDiscId1, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(true, comment.IsPermanent);
			comment = discussionManager.RetrieveById(requirementDiscId2, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(false, comment.IsPermanent);
			comment = discussionManager.RetrieveById(taskDiscId1, Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(true, comment.IsPermanent);
			comment = discussionManager.RetrieveById(taskDiscId2, Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(false, comment.IsPermanent);
			comment = discussionManager.RetrieveById(releaseDiscId1, Artifact.ArtifactTypeEnum.Release);
			Assert.AreEqual(true, comment.IsPermanent);
			comment = discussionManager.RetrieveById(releaseDiscId2, Artifact.ArtifactTypeEnum.Release);
			Assert.AreEqual(false, comment.IsPermanent);
			comment = discussionManager.RetrieveById(testCaseDiscId1, Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(true, comment.IsPermanent);
			comment = discussionManager.RetrieveById(testCaseDiscId2, Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(false, comment.IsPermanent);
			comment = discussionManager.RetrieveById(testSetDiscId1, Artifact.ArtifactTypeEnum.TestSet);
			Assert.AreEqual(true, comment.IsPermanent);
			comment = discussionManager.RetrieveById(testSetDiscId2, Artifact.ArtifactTypeEnum.TestSet);
			Assert.AreEqual(false, comment.IsPermanent);

			//Now delete the temporary ones, verify success
			discussionManager.DeleteDiscussionId(requirementDiscId2, Artifact.ArtifactTypeEnum.Requirement, true, USER_ID_SYS_ADMIN, 1);
			discussionManager.DeleteDiscussionId(releaseDiscId2, Artifact.ArtifactTypeEnum.Release, true, USER_ID_SYS_ADMIN, 1);
			discussionManager.DeleteDiscussionId(taskDiscId2, Artifact.ArtifactTypeEnum.Task, true, USER_ID_SYS_ADMIN, 1);
			discussionManager.DeleteDiscussionId(testCaseDiscId2, Artifact.ArtifactTypeEnum.TestCase, true, USER_ID_SYS_ADMIN, 1);
			discussionManager.DeleteDiscussionId(testSetDiscId2, Artifact.ArtifactTypeEnum.TestSet, true, USER_ID_SYS_ADMIN, 1);

			//Verify
			//Assert.IsNull(discussionManager.RetrieveById(requirementDiscId2, Artifact.ArtifactTypeEnum.RequirementDiscussion));
			//Assert.IsNull(discussionManager.RetrieveById(releaseDiscId2, Artifact.ArtifactTypeEnum.ReleaseDiscussion));
			//Assert.IsNull(discussionManager.RetrieveById(taskDiscId2, Artifact.ArtifactTypeEnum.TaskDiscussion));
			//Assert.IsNull(discussionManager.RetrieveById(testCaseDiscId2, Artifact.ArtifactTypeEnum.TestCaseDiscussion));
			//Assert.IsNull(discussionManager.RetrieveById(testSetDiscId2, Artifact.ArtifactTypeEnum.TestSetDiscussion));

			//Now try and delete the permanent ones, verify failure
			bool exceptionThrown = false;
			try
			{
				discussionManager.DeleteDiscussionId(requirementDiscId1, Artifact.ArtifactTypeEnum.Requirement, true, USER_ID_SYS_ADMIN, 1);
			}
			catch (DiscussionCannotBeDeletedIfPermanentException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			exceptionThrown = false;
			try
			{
				discussionManager.DeleteDiscussionId(releaseDiscId1, Artifact.ArtifactTypeEnum.Release, true, USER_ID_SYS_ADMIN, 1);
			}
			catch (DiscussionCannotBeDeletedIfPermanentException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			exceptionThrown = false;
			try
			{
				discussionManager.DeleteDiscussionId(taskDiscId1, Artifact.ArtifactTypeEnum.Task, true, USER_ID_SYS_ADMIN, 1);
			}
			catch (DiscussionCannotBeDeletedIfPermanentException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			exceptionThrown = false;
			try
			{
				discussionManager.DeleteDiscussionId(testCaseDiscId1, Artifact.ArtifactTypeEnum.TestCase, true, USER_ID_SYS_ADMIN, 1);
			}
			catch (DiscussionCannotBeDeletedIfPermanentException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			exceptionThrown = false;
			try
			{
				discussionManager.DeleteDiscussionId(testSetDiscId1, Artifact.ArtifactTypeEnum.TestSet, true, USER_ID_SYS_ADMIN, 1);
			}
			catch (DiscussionCannotBeDeletedIfPermanentException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Verify
			//Assert.IsNotNull(discussionManager.RetrieveById(requirementDiscId1, Artifact.ArtifactTypeEnum.RequirementDiscussion));
			//Assert.IsNotNull(discussionManager.RetrieveById(releaseDiscId1, Artifact.ArtifactTypeEnum.ReleaseDiscussion));
			//Assert.IsNotNull(discussionManager.RetrieveById(taskDiscId1, Artifact.ArtifactTypeEnum.TaskDiscussion));
			//Assert.IsNotNull(discussionManager.RetrieveById(testCaseDiscId1, Artifact.ArtifactTypeEnum.TestCaseDiscussion));
			//Assert.IsNotNull(discussionManager.RetrieveById(testSetDiscId1, Artifact.ArtifactTypeEnum.TestSetDiscussion));
		}
	}
}
