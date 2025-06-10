using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using NUnit.Framework;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Requirement business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class RequirementManagerTest
	{
		protected static Business.RequirementManager requirementManager;
		protected static List<RequirementView> requirements;
		protected static RequirementView requirementView;
		protected static int requirementId1;
		protected static int requirementId2;
		protected static int requirementId3;
		protected static int requirementId4;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		private static int projectId;
		private static int projectTemplateId;

		private const int PROJECT_ID = 1;
		private const int PROJECT_ID_2 = 2;
		private const int PROJECT_TEMPLATE_ID = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the business object
			requirementManager = new Business.RequirementManager();

			//Create a new project for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			projectId = projectManager.Insert("RequirementManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			
			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the last artifact id
			Business.HistoryManager history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Purge any delete items in the sample project
			Business.HistoryManager history = new Business.HistoryManager();
			history.PurgeAllDeleted(PROJECT_ID, USER_ID_FRED_BLOGGS);

			//We need to refresh the task progress for the sample project to ensure it matches the start condition
			//once all tests are moved to the temporary project, this will no longer be necessary
			new Business.ProjectManager().RefreshTaskProgressCache(PROJECT_ID);

			//Delete the temporary project and its template
			ProjectManager projectManager = new ProjectManager();
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);

			//We need to delete any artifact history items created during the test run
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());
		}

		[
		Test,
		SpiraTestCase(28)
		]
		public void _01_CreateRequirement()
		{
			//First lets actually test inserting a new requirement
			//Need to test that we can have a name of length 255 characters
			//and description of length 4000 characters with one unicode character
			//No release is specified, so it will remained 'Requested'
			requirementId1 = requirementManager.Insert(
				USER_ID_FRED_BLOGGS,
				PROJECT_ID,
				null,
				null,
				13,
				Requirement.RequirementStatusEnum.Requested,
				null,
				USER_ID_SYS_ADMIN,
				null,
				1,
				"Test Requirement " + InternalRoutines.RepeatString("tes\u05d0", 59),
				InternalRoutines.RepeatString("tes\u05d0", 1000),
				1.0M,
				USER_ID_FRED_BLOGGS
				);

			//Now lets make sure we can retrieve the submitted item
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId1);
			Assert.AreEqual(requirementId1, requirementView.RequirementId, "Requirement ID");
			Assert.AreEqual(null, requirementView.ReleaseId, "ReleaseId");
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Requested, requirementView.RequirementStatusId, "Requirement Status ID");
			Assert.AreEqual(1, requirementView.AuthorId, "Author ID");
			Assert.AreEqual(1, requirementView.ImportanceId, "Importance ID");
			Assert.AreEqual("Test Requirement " + InternalRoutines.RepeatString("tes\u05d0", 59), requirementView.Name, "Name");
			Assert.AreEqual(InternalRoutines.RepeatString("tes\u05d0", 1000), requirementView.Description, "Description");
			Assert.AreEqual("AAAAAAAAC", requirementView.IndentLevel, "Indent Level");
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded Flag");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible Flag");
			Assert.AreEqual(false, requirementView.IsSummary, "Summary Flag");
			Assert.AreEqual("Requested", requirementView.RequirementStatusName, "Scope Level Name");
			Assert.AreEqual("System Administrator", requirementView.AuthorName, "Author Name");
			Assert.AreEqual("1 - Critical", requirementView.ImportanceName, "Importance Name");
			Assert.AreEqual(null, requirementView.ReleaseVersionNumber, "ReleaseVersionNumber");
			Assert.AreEqual(PROJECT_ID, requirementView.ProjectId, "ProjectId");
			Assert.AreEqual(480, requirementView.EstimatedEffort, "EstimatedEffort");
			Assert.AreEqual(1.0M, requirementView.EstimatePoints, "EstimatePoints");
			Assert.IsFalse(requirementView.OwnerId.HasValue, "OwnerId");

			//Next lets make sure the next requirement has been reordered to the next position
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AAAAAAAAD", requirementView.IndentLevel, "Indent Level");

			//Now make sure that we can insert a requirement before an existing one that has no parent
			//A release is specified, so it will actually be listed as Planned
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, null, 1, Requirement.RequirementStatusEnum.Requested, null, USER_ID_SYS_ADMIN, USER_ID_FRED_BLOGGS, 1, "Test No Parent Requirement", "Test Description of No Parent Requirement", 0.5M, USER_ID_FRED_BLOGGS);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId2);
			Assert.AreEqual(1, requirementView.ReleaseId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirementView.RequirementStatusId, "Requirement Status ID");
			Assert.AreEqual("1.0.0.0", requirementView.ReleaseVersionNumber, "ReleaseVersionNumber");

			//Next lets make sure the next requirement has been reordered to the next position
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 1);
			Assert.AreEqual("AAB", requirementView.IndentLevel, "Indent Level");

			//Now make sure that we can insert a requirement that is not being inserted before an existing one
			//Also test that we can have an owner and planned effort value set at insert
			requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, 2, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_SYS_ADMIN, USER_ID_FRED_BLOGGS, 1, "Test No Existing Requirement", "Description of No Existing Requirement", 0.5M, USER_ID_FRED_BLOGGS);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId3);
			Assert.AreEqual(false, requirementView.IsSummary);
			Assert.AreEqual("AADAAD", requirementView.IndentLevel, "IndentLevel");
			Assert.AreEqual(240, requirementView.EstimatedEffort, "EstimatedEffort");
			Assert.AreEqual(0.5M, requirementView.EstimatePoints, "EstimatePoints");
			Assert.AreEqual("Fred Bloggs", requirementView.OwnerName, "OwnerName");

			//Finally make sure that we can insert a child requirement under an existing one
			requirementId4 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, PROJECT_ID, 2, null, requirementId3, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, null, 1, "Test Child Req", "", 250, USER_ID_FRED_BLOGGS);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);
			Assert.AreEqual("AADAADAAA", requirementView.IndentLevel, "IndentLevel");
			Assert.AreEqual("Test Child Req", requirementView.Name);

			//Verify that it made the parent a summary
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId3);
			Assert.AreEqual(true, requirementView.IsSummary);

			//Now delete the child and verify that the parent became a 'non-summary' requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId3);
			Assert.AreEqual(false, requirementView.IsSummary);
		}

		[
		Test,
		SpiraTestCase(52)
		]
		public void _02_ModifyRequirement()
		{
			//Make some changes to the data then call update
			//Note that only scope-level, name, author and importance are changable
			//via the modify method. So we will test that parent id hasn't changed
			Requirement requirement = requirementManager.RetrieveById3(PROJECT_ID, requirementId1);
			requirement.StartTracking();
			requirement.Name = "Modified Requirement";
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Planned;
			requirement.AuthorId = USER_ID_FRED_BLOGGS;
			requirement.ImportanceId = 2;
			requirement.ReleaseId = 2;
			requirement.Description = "Modified Description of Requirement";
			requirement.OwnerId = USER_ID_JOE_SMITH;
			requirement.EstimatePoints = 1.0M;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });

			//Now lets make sure the changes happened
			requirementView = requirementManager.RetrieveById2(PROJECT_ID, requirementId1);
			Assert.AreEqual(requirementId1, requirementView.RequirementId, "Requirement ID");
			Assert.AreEqual(2, requirementView.ReleaseId, "ReleaseId");
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirementView.RequirementStatusId, "Requirement Status ID");
			Assert.AreEqual(2, requirementView.AuthorId, "Author ID");
			Assert.AreEqual(2, requirementView.ImportanceId, "Importance ID");
			Assert.AreEqual("Modified Requirement", requirementView.Name, "Name");
			Assert.AreEqual("Modified Description of Requirement", requirementView.Description, "Description");
			Assert.AreEqual("AABAAAAAC", requirementView.IndentLevel, "Indent Level");
			Assert.AreEqual(false, requirementView.IsSummary, "Summary Flag");
			Assert.AreEqual("Planned", requirementView.RequirementStatusName, "Scope Level Name");
			Assert.AreEqual("Fred Bloggs", requirementView.AuthorName, "Author Name");
			Assert.AreEqual("2 - High", requirementView.ImportanceName, "Importance Name");
			Assert.AreEqual("1.0.1.0", requirementView.ReleaseVersionNumber, "ReleaseVersionNumber");
			Assert.AreEqual(480, requirementView.EstimatedEffort, "EstimatedEffort");
			Assert.AreEqual(1.0M, requirementView.EstimatePoints, "EstimatePoints");
			Assert.AreEqual("Joe P Smith", requirementView.OwnerName, "OwnerName");

			//For the visible and expanded flag, we need to use a different retrieve method
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId1);
			Assert.AreEqual(requirementId1, requirementView.RequirementId, "Requirement ID");
			Assert.AreEqual(2, requirementView.ReleaseId, "ReleaseId");
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirementView.RequirementStatusId, "Requirement Status ID");
			Assert.AreEqual(2, requirementView.AuthorId, "Author ID");
			Assert.AreEqual(2, requirementView.ImportanceId, "Importance ID");
			Assert.AreEqual("Modified Requirement", requirementView.Name, "Name");
			Assert.AreEqual("Modified Description of Requirement", requirementView.Description, "Description");
			Assert.AreEqual("AABAAAAAC", requirementView.IndentLevel, "Indent Level");
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded Flag");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible Flag");
			Assert.AreEqual(false, requirementView.IsSummary, "Summary Flag");
			Assert.AreEqual("Planned", requirementView.RequirementStatusName, "Scope Level Name");
			Assert.AreEqual("Fred Bloggs", requirementView.AuthorName, "Author Name");
			Assert.AreEqual("2 - High", requirementView.ImportanceName, "Importance Name");
			Assert.AreEqual("1.0.1.0", requirementView.ReleaseVersionNumber, "ReleaseVersionNumber");
			Assert.AreEqual(480, requirementView.EstimatedEffort, "EstimatedEffort");
			Assert.AreEqual(1.0M, requirementView.EstimatePoints, "EstimatePoints");
			Assert.AreEqual("Joe P Smith", requirementView.OwnerName, "OwnerName");
		}

		[
		Test,
		SpiraTestCase(53)
		]
		public void _03_DeleteRequirement()
		{
			//Now lets delete the requirements and make sure the indent levels return correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel, "Indent Level");

			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId1);

			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel, "Indent Level");

			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId2);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId3);

			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel, "Indent Level");

			//Make sure that it was actually deleted
			bool artifactExists = true;
			try
			{
				requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Requirement not deleted");

			//Now we need to test that deleting the only subrequirement of a summary item
			//turns that item back into a non-summary task
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 5);
			Assert.AreEqual(false, requirementView.IsSummary, "Summary Flag");

			//Test that when we indent a requirement, the tasks and tests stay attached to the requirement (change from earlier versions of the system)
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, null, null, 6, Requirement.RequirementStatusEnum.Requested, null, USER_ID_SYS_ADMIN, USER_ID_FRED_BLOGGS, null, "Test Deleting sub-item", null, 120, USER_ID_FRED_BLOGGS);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 5);
			Assert.AreEqual(true, requirementView.IsSummary, "Summary Flag");
			Assert.AreEqual(3, requirementView.TaskCount, "TaskCount");
			Assert.AreEqual(2, requirementView.CoverageCountTotal, "CoverageCountTotal");

			//Clean up by deleting the requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId);

			//Verify the data after the deletion
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 5);
			Assert.AreEqual(false, requirementView.IsSummary, "SummaryYn");
			Assert.AreEqual(3, requirementView.TaskCount, "TaskCount");
			Assert.AreEqual(2, requirementView.CoverageCountTotal, "CoverageCountTotal");
		}

		[
		Test,
		SpiraTestCase(54)
		]
		public void _04_IndentOutdent()
		{
			//Coverage testing added to ensure that summary items correctly roll-up coverage on indent changes

			//This tests that we can correctly indent and outdent a single requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual("Ability to associate books with different subjects", requirementView.Name, "Requirement name is correct");
			Assert.AreEqual(false, requirementView.IsSummary, "Summary Flag");
			Assert.AreEqual(false, requirementView.IsSummary, "Expanded Flag");
			//Verify the coverage and task data before indenting
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(1, requirementView.CoverageCountPassed);
			Assert.AreEqual(1, requirementView.CoverageCountCaution);
			Assert.AreEqual(4, requirementView.TaskCount);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			Assert.AreEqual("AABAAAAAAAAE", requirementView.IndentLevel, "Indent Level");
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(2, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(2, requirementView.TaskCount);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 9);
			Assert.AreEqual("AABAAAAAAAAF", requirementView.IndentLevel, "Indent Level");

			//Do the indent and check that adjacent items are correct
			requirementManager.Indent(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);

			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual(true, requirementView.IsSummary, "Summary Flag");
			Assert.AreEqual(true, requirementView.IsSummary, "Expanded Flag");
			//Verify the task and test coverage data after indenting (the coverage and task are from the child requirement and itself)
			Assert.AreEqual(4, requirementView.CoverageCountTotal);
			Assert.AreEqual(3, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(6, requirementView.TaskCount);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			Assert.AreEqual("AABAAAAAAAADAAA", requirementView.IndentLevel, "Indent Level");
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(2, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(2, requirementView.TaskCount);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 9);
			Assert.AreEqual("AABAAAAAAAAE", requirementView.IndentLevel, "Indent Level");

			//Do the outdent and check that adjacent items are correct
			requirementManager.Outdent(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual(false, requirementView.IsSummary, "Summary Flag");
			Assert.AreEqual(false, requirementView.IsSummary, "Expanded Flag");
			//Verify the task and test coverage data after outdenting
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(1, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(4, requirementView.TaskCount);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			Assert.AreEqual("AABAAAAAAAAE", requirementView.IndentLevel, "Indent Level");
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(2, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(2, requirementView.TaskCount);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 9);
			Assert.AreEqual("AABAAAAAAAAF", requirementView.IndentLevel, "Indent Level");

			//Now let's test that we can indent an item when its predecessor already has child requirements
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 11);
			Assert.AreEqual("AABAAAAAB", requirementView.IndentLevel, "Indent Level");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 12);
			Assert.AreEqual("AABAAAAABAAA", requirementView.IndentLevel, "Indent Level");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel, "Indent Level");

			requirementManager.Indent(USER_ID_FRED_BLOGGS, PROJECT_ID, 11);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 11);
			Assert.AreEqual("AABAAAAAAAAH", requirementView.IndentLevel, "Indent Level");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 12);
			Assert.AreEqual("AABAAAAAAAAHAAA", requirementView.IndentLevel, "Indent Level");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAC", requirementView.IndentLevel, "Indent Level");

			requirementManager.Outdent(USER_ID_FRED_BLOGGS, PROJECT_ID, 11);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 11);
			Assert.AreEqual("AABAAAAAB", requirementView.IndentLevel, "Indent Level");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 12);
			Assert.AreEqual("AABAAAAABAAA", requirementView.IndentLevel, "Indent Level");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel, "Indent Level");

			//There is a special case we need to deal with where we have an existing folder that becomes
			//the subfolder of the first folder in the system. We get double-indentation issues
			//This fixes incident [IN:235].

			//First, need to create a new hierarchy from scratch to simulate
			int newRequirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			int newRequirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 2", null, 60, USER_ID_FRED_BLOGGS);
			int newRequirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 3", null, 120, USER_ID_FRED_BLOGGS);
			int newRequirementId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 4", null, 240, USER_ID_FRED_BLOGGS);

			//Now indent the third and fourth items to create a summary item with two children
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, newRequirementId3);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, newRequirementId4);

			//Now indent the second item to make it a child of the first, with two sub-children
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, newRequirementId2);

			//Finally need to verify the hierarchy indent levels
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, newRequirementId1);
			requirements = requirementManager.RetrieveChildren(USER_ID_FRED_BLOGGS, projectId, requirementView.IndentLevel, true);
			Assert.AreEqual("Requirement 2", requirements[0].Name);
			Assert.AreEqual("AAAAAA", requirements[0].IndentLevel);
			Assert.AreEqual("Requirement 3", requirements[1].Name);
			Assert.AreEqual("AAAAAAAAA", requirements[1].IndentLevel);
			Assert.AreEqual("Requirement 4", requirements[2].Name);
			Assert.AreEqual("AAAAAAAAB", requirements[2].IndentLevel);

			//Now we need to clean up and remove these new requirements
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, newRequirementId1);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, newRequirementId2);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, newRequirementId3);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, newRequirementId4);
		}

		[
		Test,
		SpiraTestCase(55)
		]
		public void _05_ExpandCollapse()
		{
			//First lets test the initial state to ensure that it's collapsed and children are non-visible
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(false, requirementView.IsVisible, "Visible");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 15);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(false, requirementView.IsVisible, "Visible");

			//Now lets expand the first tree and test that it worked correctly
			requirementManager.Expand(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual(true, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 15);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(false, requirementView.IsVisible, "Visible");

			//Now lets expand the second tree and test that it worked correctly
			requirementManager.Expand(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual(true, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual(true, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 15);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible");

			//Now lets collapse the entire tree and test that it worked correctly
			requirementManager.Collapse(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(true, requirementView.IsVisible, "Visible");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(false, requirementView.IsVisible, "Visible");
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 15);
			Assert.AreEqual(false, requirementView.IsExpanded, "Expanded");
			Assert.AreEqual(false, requirementView.IsVisible, "Visible");

			//Now test that we can expand to specific level 1
			requirementManager.ExpandToLevel(USER_ID_FRED_BLOGGS, PROJECT_ID, 1);
			//Verify the count
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(3, requirements.Count);

			//Now test that we can expand to specific level 2
			requirementManager.ExpandToLevel(USER_ID_FRED_BLOGGS, PROJECT_ID, 2);
			//Verify the count
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(10, requirements.Count);

			//Now test that we can expand to specific level 3
			requirementManager.ExpandToLevel(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			//Verify the count
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(15, requirements.Count);

			//Now test that we can expand to show all levels
			requirementManager.ExpandToLevel(USER_ID_FRED_BLOGGS, PROJECT_ID, null);
			//Verify the count
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(35, requirements.Count);

			//Now we need to put all the levels back
			requirementManager.ExpandToLevel(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			requirementManager.Expand(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			//Verify the count
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(22, requirements.Count);
		}

		[
		Test,
		SpiraTestCase(56)
		]
		public void _06_ListRequirements()
		{
			//Lets test that we can get a list of all visible requirements
			//There should only be 14 visible by default
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(22, requirements.Count, "Count of Requirements doesn't match");

			//Test that we can retrieve a requirement together with its peers and parent
			requirements = requirementManager.RetrievePeersAndParent(USER_ID_FRED_BLOGGS, PROJECT_ID, "AABAAAAAAAAC", false);
			Assert.AreEqual(8, requirements.Count);
			Assert.AreEqual("Book Management", requirements[0].Name);
			Assert.AreEqual("Ability to add new books to the system", requirements[1].Name);
			Assert.AreEqual("Ability to completely erase all books stored in the system with one click", requirements[7].Name);

			//Test that we can retrieve a requirement together with its peers, parent and immediate children
			requirements = requirementManager.RetrievePeersChildrenAndParent(USER_ID_FRED_BLOGGS, PROJECT_ID, "AABAAAAAA");
			Assert.AreEqual(13, requirements.Count);
			Assert.AreEqual("Online Library Management System", requirements[0].Name);
			Assert.AreEqual("Book Management", requirements[1].Name);
			Assert.AreEqual("Ability to add new books to the system", requirements[2].Name);
			Assert.AreEqual("Administration Functions", requirements[12].Name);
		}

		[
		Test,
		SpiraTestCase(57)
		]
		public void _07_LoadLookups()
		{
			//Lets test that we can load the requirement status lookup list
			//check the different options for the method
			List<RequirementStatus> statuses = requirementManager.RetrieveStatuses(true, true, true);
			Assert.AreEqual(17, statuses.Count);
			statuses = requirementManager.RetrieveStatuses(true, true, false);
			Assert.AreEqual(16, statuses.Count);
			statuses = requirementManager.RetrieveStatuses(true, false, true);
			Assert.AreEqual(4, statuses.Count);
			statuses = requirementManager.RetrieveStatuses(false, true, true);
			Assert.AreEqual(13, statuses.Count);
			statuses = requirementManager.RetrieveStatuses(false, true, false);
			Assert.AreEqual(12, statuses.Count);
			statuses = requirementManager.RetrieveStatuses(false, false, false);
			Assert.AreEqual(0, statuses.Count);
			//now get the default - all set to true - Make sure we have the number of records and values we expect
			statuses = requirementManager.RetrieveStatuses();
			Assert.AreEqual(17, statuses.Count);

			//Make sure we have the number of records and values we expect
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Requested, statuses[0].RequirementStatusId);
			Assert.AreEqual("Requested", statuses[0].Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Accepted, statuses[3].RequirementStatusId);
			Assert.AreEqual("Accepted", statuses[3].Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, statuses[6].RequirementStatusId);
			Assert.AreEqual("Developed", statuses[6].Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Completed, statuses[8].RequirementStatusId);
			Assert.AreEqual("Completed", statuses[8].Name);

			//Lets test that we can load the Importance lookup list
			List<Importance> importances = requirementManager.RequirementImportance_Retrieve(PROJECT_TEMPLATE_ID);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(4, importances.Count);
			Assert.AreEqual(1, importances[3].ImportanceId);
			Assert.AreEqual("1 - Critical", importances[3].Name);
		}

		[
		Test,
		SpiraTestCase(58)
		]
		public void _08_OperationsAsOtherUser()
		{
			//Lets test that we can get a list of requirements as a user without previous viewing metadata
			//There should be 35 visible by default (since it defaults to all expanded)
			requirements = requirementManager.Retrieve(USER_ID_JOE_SMITH, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(35, requirements.Count, "Count of Requirements doesn't match");

			//Lets test that we can collapse a summary item that has no viewing metadata
			requirementView = requirementManager.RetrieveById(USER_ID_JOE_SMITH, PROJECT_ID, 3);
			Assert.AreEqual(true, requirementView.IsExpanded, "ExpandedYn1");
			requirementManager.Collapse(USER_ID_JOE_SMITH, PROJECT_ID, 3);
			requirementView = requirementManager.RetrieveById(USER_ID_JOE_SMITH, PROJECT_ID, 3);
			Assert.AreEqual(false, requirementView.IsExpanded, "ExpandedYn2");
			requirements = requirementManager.Retrieve(USER_ID_JOE_SMITH, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(28, requirements.Count, "Count of Requirements doesn't match");

			//Lets test that we can collapse a summary item that has no viewing metadata
			//and has an initial non-summary item as a child (this was a detected bug in v1.0.0.0)
			requirementManager.Collapse(USER_ID_JOE_SMITH, PROJECT_ID, 22);
			requirementView = requirementManager.RetrieveById(USER_ID_JOE_SMITH, PROJECT_ID, 22);
			Assert.AreEqual(false, requirementView.IsExpanded, "ExpandedYn3");
			requirementView = requirementManager.RetrieveById(USER_ID_JOE_SMITH, PROJECT_ID, 23);
			Assert.AreEqual(false, requirementView.IsVisible, "VisibleYn1");
			requirementView = requirementManager.RetrieveById(USER_ID_JOE_SMITH, PROJECT_ID, 24);
			Assert.AreEqual(false, requirementView.IsExpanded, "ExpandedYn4");
			Assert.AreEqual(false, requirementView.IsVisible, "VisibleYn2");
			requirementView = requirementManager.RetrieveById(USER_ID_JOE_SMITH, PROJECT_ID, 25);
			Assert.AreEqual(false, requirementView.IsVisible);

			//Finally clean up the system to its starting point and verify
			requirementManager.DeleteUserNavigationData(USER_ID_JOE_SMITH);
			requirements = requirementManager.Retrieve(USER_ID_JOE_SMITH, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(35, requirements.Count, "Count of Requirements doesn't match");

			//We need to modify Req ID 5 to put it back to its initial state, the previous tests above
			//will have modified it to Requested. It needs to be back to Developed
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 5);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirementView.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Developed;
			requirement.StartTracking();
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });
		}

		/// <summary>
		/// We need to test that requirement status changes rollup correctly to parent requirements
		/// </summary>
		[
		Test,
		SpiraTestCase(59)
		]
		public void _09_TestStatusRollup()
		{
			//We need to create some new requirements to test this because the sample ones have Tasks linked
			//which will cause the status to not change since the Tasks are all completed
			int newRequirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, null, null, (int?)null, Requirement.RequirementStatusEnum.Developed, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, 2, "Test Req 1", "", null, USER_ID_FRED_BLOGGS);
			int newRequirementId2 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, PROJECT_ID, null, null, newRequirementId1, Requirement.RequirementStatusEnum.Developed, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, 2, "Test Req 1", "", null, USER_ID_FRED_BLOGGS);
			int newRequirementId3 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, PROJECT_ID, null, null, newRequirementId1, Requirement.RequirementStatusEnum.Developed, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, 2, "Test Req 1", "", null, USER_ID_FRED_BLOGGS);
			int newRequirementId4 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, PROJECT_ID, null, null, newRequirementId1, Requirement.RequirementStatusEnum.Developed, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, 2, "Test Req 1", "", null, USER_ID_FRED_BLOGGS);

			//First lets retrieve the two requirements and test the initial state
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId1);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);

			//Now lets update the status of the lower-level item to a different status that should change the parent
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Planned;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });

			//Now lets test the updated status codes
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId1);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirementView.RequirementStatusId);

			//Now change the status back and update
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Developed;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });

			//Now lets test the updated status codes
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId1);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);

			//Need to make sure that if we change the last child item under a parent to Requested
			//where the other items are completed, it rolls-up correctly.
			//This was previously logged as a bug.
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId1);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId4);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);

			//Change the last child to 'Requested' - parent should now change to 'InProgress'
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Requested;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });

			//Now lets test the updated status codes
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId1);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId4);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Requested, requirementView.RequirementStatusId);

			//Change the last child back to 'Completed' - parent should now change to 'Completed'
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Developed;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });

			//Now lets test the updated status codes
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId1);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId4);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);

			//Clean up by deleting these items
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, newRequirementId1);
			requirementManager.DeleteFromDatabase(newRequirementId1, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(60)
		]
		public void _10_RetrieveSummaryData()
		{
			//First get the requirement summary untyped dataset - all releases
			System.Data.DataSet summaryDataSet = requirementManager.RetrieveProjectSummary(PROJECT_ID, PROJECT_TEMPLATE_ID, null);
			//Make sure the data is as expected
			Assert.AreEqual(17, summaryDataSet.Tables["RequirementSummary"].Rows.Count);
			Assert.AreEqual("Requested", summaryDataSet.Tables["RequirementSummary"].Rows[0]["RequirementStatusName"]);
			Assert.AreEqual(6, summaryDataSet.Tables["RequirementSummary"].Rows[0]["Total"]);
			Assert.AreEqual("Completed", summaryDataSet.Tables["RequirementSummary"].Rows[8]["RequirementStatusName"]);
			Assert.AreEqual(10, summaryDataSet.Tables["RequirementSummary"].Rows[8]["1"]);

			//First get the requirement summary untyped dataset - specific release
			summaryDataSet = requirementManager.RetrieveProjectSummary(PROJECT_ID, PROJECT_TEMPLATE_ID, 1);
			//Make sure the data is as expected
			Assert.AreEqual(17, summaryDataSet.Tables["RequirementSummary"].Rows.Count);
			Assert.AreEqual("Requested", summaryDataSet.Tables["RequirementSummary"].Rows[0]["RequirementStatusName"]);
			Assert.AreEqual(0, summaryDataSet.Tables["RequirementSummary"].Rows[0]["Total"]);
			Assert.AreEqual("Completed", summaryDataSet.Tables["RequirementSummary"].Rows[8]["RequirementStatusName"]);
			Assert.AreEqual(5, summaryDataSet.Tables["RequirementSummary"].Rows[8]["1"]);

			//Now get the incident count per requirement - all releases
			List<RequirementIncidentCount> requirementIncidentCounts = requirementManager.RetrieveIncidentCount(PROJECT_ID, null, 10, true);
			Assert.AreEqual(8, requirementIncidentCounts.Count);
			Assert.AreEqual("Ability to add new books to the system", requirementIncidentCounts[1].Name);
			Assert.AreEqual(2, requirementIncidentCounts[1].IncidentOpenCount);
			Assert.AreEqual("Ability to edit existing books in the system", requirementIncidentCounts[2].Name);
			Assert.AreEqual(2, requirementIncidentCounts[2].IncidentTotalCount);

			//Now get the incident count per requirement - specific release (including non-open incident rows)
			requirementIncidentCounts = requirementManager.RetrieveIncidentCount(PROJECT_ID, 1, 5, false);
			Assert.AreEqual(5, requirementIncidentCounts.Count);
			Assert.AreEqual("Ability to associate books with different editions", requirementIncidentCounts[0].Name);
			Assert.AreEqual(3, requirementIncidentCounts[0].IncidentOpenCount);
			Assert.AreEqual("Ability to completely erase all books stored in the system with one click", requirementIncidentCounts[2].Name);
			Assert.AreEqual(2, requirementIncidentCounts[2].IncidentTotalCount);

			//Now test that we can get the project-group requirements coverage summary
			//All Releases
			List<RequirementCoverageSummary> requirementCoverageSummaries;
			requirementCoverageSummaries = requirementManager.RetrieveCoverageSummary(2, false);
			Assert.AreEqual(6, requirementCoverageSummaries.Count);
			Assert.AreEqual("Passed", requirementCoverageSummaries[0].CoverageStatus);
			Assert.AreEqual(8.1, requirementCoverageSummaries[0].CoverageCount);
			Assert.AreEqual("Not Run", requirementCoverageSummaries[4].CoverageStatus);
			Assert.AreEqual(1.6, System.Math.Round((double)(requirementCoverageSummaries[4].CoverageCount), 1));
			Assert.AreEqual("Not Covered", requirementCoverageSummaries[5].CoverageStatus);
			Assert.AreEqual(53, requirementCoverageSummaries[5].CoverageCount);

			//Now test that we can get the project-group requirements coverage summary
			//Active Releases
			requirementCoverageSummaries = requirementManager.RetrieveCoverageSummary(2, true);
			Assert.AreEqual(6, requirementCoverageSummaries.Count);
			Assert.AreEqual("Passed", requirementCoverageSummaries[0].CoverageStatus);
			Assert.AreEqual(5.8, System.Math.Round((double)(requirementCoverageSummaries[0].CoverageCount), 1));
			Assert.AreEqual("Not Run", requirementCoverageSummaries[4].CoverageStatus);
			Assert.AreEqual(1.2, System.Math.Round((double)(requirementCoverageSummaries[4].CoverageCount), 1));
			Assert.AreEqual("Not Covered", requirementCoverageSummaries[5].CoverageStatus);
			Assert.AreEqual(54, requirementCoverageSummaries[5].CoverageCount);
		}

		[
		Test,
		SpiraTestCase(95)
		]
		public void _11_RetrieveRequirementsWithFilters()
		{
			Hashtable filters = new Hashtable();

			//Lets test that we can retrieve the count of requirements in the system with a filter applied
			//we'll use both the main retrieve method and the Count() method that doesn't consider the pagination

			//Filter by Priority=Critical(P1)
			filters.Add("ImportanceId", 1);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(13, requirements.Count);
			Assert.AreEqual(18, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Test that we can paginate the results
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 5, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(5, requirements.Count);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 5, 5, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(5, requirements.Count);
			Assert.AreEqual(18, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Status=Completed
			filters.Clear();
			filters.Add("RequirementStatusId", (int)Requirement.RequirementStatusEnum.Completed);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(17, requirements.Count);
			Assert.AreEqual(21, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Coverage='Not Covered'
			filters.Clear();
			filters.Add("CoverageId", 1);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, requirements.Count);
			Assert.AreEqual(13, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Coverage='0% Run'
			filters.Clear();
			filters.Add("CoverageId", 2);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(9, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Coverage='<= 50% Run'
			filters.Clear();
			filters.Add("CoverageId", 3);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count);
			Assert.AreEqual(10, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Coverage='< 100% Run'
			filters.Clear();
			filters.Add("CoverageId", 4);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, requirements.Count);
			Assert.AreEqual(12, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Coverage='> 0% Failed'
			filters.Clear();
			filters.Add("CoverageId", 5);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, requirements.Count, "CoverageId1");
			Assert.AreEqual(13, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Coverage='>= 50% Failed'
			filters.Clear();
			filters.Add("CoverageId", 6);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(7, requirements.Count, "CoverageId2");

			//Filter by Coverage='= 100% Failed'
			filters.Clear();
			filters.Add("CoverageId", 7);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, requirements.Count);

			//Filter by Coverage='> 0% Caution'
			filters.Clear();
			filters.Add("CoverageId", 8);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, requirements.Count, "CoverageId3");

			//Filter by Coverage='>= 50% Caution'
			filters.Clear();
			filters.Add("CoverageId", 9);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count, "CoverageId4");

			//Filter by Coverage='= 100% Caution'
			filters.Clear();
			filters.Add("CoverageId", 10);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);

			//Filter by Coverage='> 0% Blocked'
			filters.Clear();
			filters.Add("CoverageId", 11);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);

			//Filter by Coverage='>= 50% Blocked'
			filters.Clear();
			filters.Add("CoverageId", 12);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);

			//Filter by Coverage='= 100% Blocked'
			filters.Clear();
			filters.Add("CoverageId", 13);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);

			//Filter by Task Progress='Not Started'
			filters.Clear();
			filters.Add("ProgressId", 1);
			requirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(7, requirements.Count);
			Assert.AreEqual(9, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET), "ProgressId1");

			//Filter by Task Progress='Starting Late'
			filters.Clear();
			filters.Add("ProgressId", 2);
			requirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(5, requirements.Count, "ProgressId");

			//Filter by Task Progress='on Schedule'
			filters.Clear();
			filters.Add("ProgressId", 3);
			requirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);

			//Filter by Task Progress='Running Late'
			filters.Clear();
			filters.Add("ProgressId", 4);
			requirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);

			//Filter by Task Progress='Completed'
			filters.Clear();
			filters.Add("ProgressId", 5);
			requirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, requirements.Count, "ProgressId");

			//Filter by Author=Joe Smith
			filters.Clear();
			filters.Add("AuthorId", 3);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(9, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Name LIKE 'books'
			filters.Clear();
			filters.Add("Name", "books");
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(10, requirements.Count, "Name");
			Assert.AreEqual(16, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by RequirementId=4
			filters.Clear();
			filters.Add("RequirementId", 4);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count, "RequirementId");
			Assert.AreEqual(10, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by LastUpdateDate >= 12/1/2003
			filters.Clear();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.Parse("12/1/2003");
			filters.Add("LastUpdateDate", dateRange);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(18, requirements.Count);
			Assert.AreEqual(22, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Lets test that we can apply more than one filter
			filters.Clear();
			filters.Add("Name", "associate");
			filters.Add("ImportanceId", 1);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, requirements.Count);
			Assert.AreEqual(12, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Now test that we can retrieve requirements with multiple statuses
			//Lets get all the requirements that are Planned or In Progress
			filters.Clear();
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add((int)Requirement.RequirementStatusEnum.InProgress);
			multiValueFilter.Values.Add((int)Requirement.RequirementStatusEnum.Planned);

			//Add the status list to the filter
			filters.Add("RequirementStatusId", multiValueFilter);
			requirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(9, requirements.Count, "RequirementStatusId");
			Assert.AreEqual(15, requirementManager.Count(USER_ID_SYS_ADMIN, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			//Make sure we only have the expected status codes
			for (int i = 0; i < requirements.Count; i++)
			{
				Assert.IsTrue(requirements[i].RequirementStatusId == (int)Requirement.RequirementStatusEnum.InProgress || requirements[i].RequirementStatusId == (int)Requirement.RequirementStatusEnum.Planned || requirements[i].IsSummary);
			}

			//Now lets try filtering on the various custom properties

			//Filter on Difficulty=Moderate
			filters.Clear();
			filters.Add("Custom_02", 2);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count);
			Assert.AreEqual(10, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			Assert.AreEqual("Functional System Requirements", requirements[0].Name);
			Assert.AreEqual("Online Library Management System", requirements[1].Name);
			Assert.AreEqual("Book Management", requirements[2].Name);
			Assert.AreEqual(4, requirements[3].RequirementId);
			Assert.AreEqual(2, requirements[3].Custom_02.FromDatabaseSerialization_Int32().Value);
			Assert.IsTrue(String.IsNullOrEmpty(requirements[3].Custom_03));
			Assert.AreEqual("http://www.libraries.org", requirements[3].Custom_01.FromDatabaseSerialization_String());

			//Filter on URL LIKE 'http://www'
			filters.Clear();
			filters.Add("Custom_01", "http://www");
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count);
			Assert.AreEqual(10, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			Assert.AreEqual("Functional System Requirements", requirements[0].Name);
			Assert.AreEqual("Online Library Management System", requirements[1].Name);
			Assert.AreEqual("Book Management", requirements[2].Name);
			Assert.AreEqual(4, requirements[3].RequirementId);
			Assert.AreEqual(2, requirements[3].Custom_02.FromDatabaseSerialization_Int32().Value);
			Assert.IsTrue(String.IsNullOrEmpty(requirements[3].Custom_03));
			Assert.AreEqual("http://www.libraries.org", requirements[3].Custom_01.FromDatabaseSerialization_String());

			//Filter on Review Date between 7/1/2012 and 7/5/2012
			filters.Clear();
			dateRange = new DateRange();
			dateRange.StartDate = DateTime.Parse("7/1/2012");
			dateRange.EndDate = DateTime.Parse("7/5/2012");
			filters.Add("Custom_05", dateRange);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count);
			Assert.AreEqual(10, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			Assert.AreEqual("Ability to add new books to the system", requirements[3].Name);
			Assert.AreEqual(4, requirements[3].RequirementId);
			Assert.AreEqual(DateTime.Parse("7/3/2012"), requirements[3].Custom_05.FromDatabaseSerialization_DateTime().Value.Date);

			//Filter on Rank between 1-3
			filters.Clear();
			IntRange intRange = new IntRange();
			intRange.MinValue = 1;
			intRange.MaxValue = 3;
			filters.Add("Custom_06", intRange);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count);
			Assert.AreEqual(10, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			Assert.AreEqual("Ability to add new books to the system", requirements[3].Name);
			Assert.AreEqual(4, requirements[3].RequirementId);
			Assert.AreEqual(1, requirements[3].Custom_06.FromDatabaseSerialization_Int32().Value);

			//Filter on Decimal between 1.1 - 1.3
			filters.Clear();
			DecimalRange decimalRange = new DecimalRange();
			decimalRange.MinValue = 1;
			decimalRange.MaxValue = 3;
			filters.Add("Custom_07", decimalRange);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count);
			Assert.AreEqual(10, requirementManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			Assert.AreEqual("Ability to add new books to the system", requirements[3].Name);
			Assert.AreEqual(4, requirements[3].RequirementId);
			Assert.AreEqual(1.234M, requirements[3].Custom_07.FromDatabaseSerialization_Decimal().Value);

			//Next lets get all requirements that have certain fields set to null
			filters.Clear();
			MultiValueFilter multiFilterValue = new MultiValueFilter();
			multiFilterValue.IsNone = true;
			filters.Add("ReleaseId", multiFilterValue);
			filters.Add("Custom_02", multiFilterValue);
			requirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, requirements.Count, "NullFilterValues");

			//Now lets test that we can get several child requirements by their IDs using a multivalue filter
			multiFilterValue.Clear();
			multiFilterValue.Values.Add(5);
			multiFilterValue.Values.Add(7);
			filters.Clear();
			filters.Add("RequirementId", multiFilterValue);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(5, requirements.Count);
			Assert.AreEqual(5, requirements[3].RequirementId);
			Assert.AreEqual(7, requirements[4].RequirementId);

			//Now lets test that filtering on a summary requirement will include its child requirements
			filters.Clear();
			filters.Add("RequirementId", 3);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 3, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, requirements.Count);
			Assert.AreEqual(3, requirements[0].RequirementId);
			Assert.AreEqual(4, requirements[1].RequirementId);
			Assert.AreEqual(10, requirements[7].RequirementId);

			//Now we need to verify that we can get the list and count of non-summary tasks
			//that's used in the various Requirements and Task integrated lists and planning screens
			filters.Clear();
			multiFilterValue = new MultiValueFilter();
			multiFilterValue.IsNone = true;
			filters.Add("ReleaseId", multiFilterValue);
			requirements = requirementManager.RetrieveNonSummary(USER_ID_SYS_ADMIN, PROJECT_ID, 1, 99999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count);
			int reqCount = requirementManager.CountNonSummary(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, reqCount);

			//Now test that we can use the special method that only retrieves requirements directly linked
			//to the specified release/iteration (i.e. ignores the fact that iterations are children of releases)
			//This method is used in the Requirement scheduling/planning modules
			requirements = requirementManager.RetrieveByReleaseId(PROJECT_ID, 8);
			Assert.AreEqual(2, requirements.Count);
			Assert.IsTrue(requirements[0].ReleaseId.HasValue);

			//Now lets verify that we can retrieve the list of open requirements NOT associated with a release
			requirements = requirementManager.RetrieveByReleaseId(PROJECT_ID, null);
			Assert.AreEqual(4, requirements.Count);
			Assert.IsFalse(requirements[0].ReleaseId.HasValue);

			//Verify that we can filter by number-ranges
			decimalRange = new DecimalRange();
			decimalRange.MinValue = 1.4M;
			decimalRange.MaxValue = 2.1M;
			filters.Clear();
			filters.Add("EstimatePoints", decimalRange);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(5, requirements.Count);

			//Verify that we can filter by effort-ranges
			EffortRange effortRange = new EffortRange();
			effortRange.MinValue = 7.5M;
			effortRange.MaxValue = 13.0M;
			filters.Clear();
			filters.Add("EstimatedEffort", effortRange);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(7, requirements.Count);
		}

		[
		Test,
		SpiraTestCase(97)
		]
		public void _12_MoveRequirements()
		{
			//******** Moving a Single Requirement ***********

			//First lets try moving a single requirement to a position at the same level
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, 10);
			//Verify the new positions
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 6);
			Assert.AreEqual("AABAAAAAAAAF", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual("AABAAAAAAAAC", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 10);
			Assert.AreEqual("AABAAAAAAAAG", requirementView.IndentLevel);

			//Now move the requirement back
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, 7);
			//Verify that it returned correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 6);
			Assert.AreEqual("AABAAAAAAAAC", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual("AABAAAAAAAAD", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 10);
			Assert.AreEqual("AABAAAAAAAAG", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 1);
			Assert.AreEqual("AAB", requirementView.IndentLevel);

			//Now lets try moving a single requirement to the first position in the list
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, 1);
			//Verify the new positions
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 6);
			Assert.AreEqual("AAB", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual("AACAAAAAAAAC", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 1);
			Assert.AreEqual("AAC", requirementView.IndentLevel);

			//Now move the requirement back
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, 7);
			//Verify that it returned correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 6);
			Assert.AreEqual("AABAAAAAAAAC", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual("AABAAAAAAAAD", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 1);
			Assert.AreEqual("AAB", requirementView.IndentLevel);

			//Now lets try moving a single requirement to the last position in the list
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, null);
			//Verify the new positions
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 6);
			Assert.AreEqual("AADAAD", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual("AABAAAAAAAAC", requirementView.IndentLevel);

			//Now move the requirement back
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, 7);
			//Verify that it returned correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 6);
			Assert.AreEqual("AABAAAAAAAAC", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual("AABAAAAAAAAD", requirementView.IndentLevel);

			//******** Moving a Nested Set of Requirements ***********

			//First lets try moving a nested set of requirements to a position at the same level
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 13, 22);
			//Verify the new positions (including the child items)
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAE", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual("AABAAAAAEAAA", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 19);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 22);
			Assert.AreEqual("AABAAAAAF", requirementView.IndentLevel);

			//Now move the requirement back
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 13, 19);
			//Verify that it returned correctly (including the child items)
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual("AABAAAAADAAA", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 19);
			Assert.AreEqual("AABAAAAAE", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 22);
			Assert.AreEqual("AABAAAAAF", requirementView.IndentLevel);

			//Next lets try moving a nested set of requirements to a position at a different level
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 13, 3);
			//Verify the new positions (including the child items)
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAA", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual("AABAAAAAAAAA", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 19);
			Assert.AreEqual("AABAAAAAE", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual("AABAAAAAB", requirementView.IndentLevel);

			//Now move the requirement back
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 13, 19);
			//Verify that it returned correctly (including the child items)
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual("AABAAAAADAAA", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 19);
			Assert.AreEqual("AABAAAAAE", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual("AABAAAAAA", requirementView.IndentLevel);

			//Next lets try moving a nested set of requirements to to the last position in the list
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 13, null);
			//Verify the new positions (including the child items)
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AADAAD", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual("AADAADAAA", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 19);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel);

			//Now move the requirement back
			requirementManager.Move(USER_ID_FRED_BLOGGS, PROJECT_ID, 13, 19);
			//Verify that it returned correctly (including the child items)
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 13);
			Assert.AreEqual("AABAAAAAD", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 14);
			Assert.AreEqual("AABAAAAADAAA", requirementView.IndentLevel);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 19);
			Assert.AreEqual("AABAAAAAE", requirementView.IndentLevel);
		}

		[
		Test,
		SpiraTestCase(98)
		]
		public void _13_CopyRequirements()
		{
			//******** Copying a Single Requirement ***********

			//First lets try copying a single requirement to a position at the same level
			int copiedRequirementId = requirementManager.Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, 10);
			//Verify the new positions and that the data copied correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			Assert.AreEqual("AABAAAAAAAAG", requirementView.IndentLevel);
			Assert.AreEqual("Ability to delete existing books in the system - Copy", requirementView.Name);
			Assert.AreEqual("<p>The ability to mark books as inactive, since technically nothing can get truly deleted for archival purposes.</p><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci.</p>", requirementView.Description);
			Assert.AreEqual(1, requirementView.ImportanceId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);
			Assert.AreEqual(9, requirementView.ReleaseId);
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(1, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(0, requirementView.CoverageCountCaution);
			Assert.AreEqual(0, requirementView.CoverageCountBlocked);

			//Verify that the associated tasks were copied across, but that their status has been reset to Not-Started and their %Completion = 0
			Assert.AreEqual(3, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentNotStart);
			Assert.AreEqual(100, requirementView.TaskPercentLateStart); //Listed as Late-Start because of their start/end dates
			Assert.AreEqual(0, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);

			//verify that the indent level of the item it was inserted in front of, has been updated correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 10);
			Assert.AreEqual("AABAAAAAAAAH", requirementView.IndentLevel);

			//Now delete all the tasks that are linked to this new requirement
			TaskManager taskManager = new TaskManager();
			List<TaskView> tasks = taskManager.RetrieveByRequirementId(copiedRequirementId);
			foreach (TaskView taskView in tasks)
			{
				taskManager.MarkAsDeleted(PROJECT_ID, taskView.TaskId, USER_ID_FRED_BLOGGS);
			}

			//Now delete the requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			//Verify that the other requirements returned correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 10);
			Assert.AreEqual("AABAAAAAAAAH", requirementView.IndentLevel);

			//Now lets try copying a single requirement to the first position in the list
			copiedRequirementId = requirementManager.Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, 1);
			//Verify the new positions and that the data copied correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			Assert.AreEqual("AAB", requirementView.IndentLevel);
			Assert.AreEqual("Ability to delete existing books in the system - Copy", requirementView.Name);
			Assert.AreEqual("<p>The ability to mark books as inactive, since technically nothing can get truly deleted for archival purposes.</p><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci.</p>", requirementView.Description);
			Assert.AreEqual(1, requirementView.ImportanceId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);
			Assert.AreEqual(9, requirementView.ReleaseId);
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(1, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(0, requirementView.CoverageCountCaution);
			Assert.AreEqual(0, requirementView.CoverageCountBlocked);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 1);
			Assert.AreEqual("AAC", requirementView.IndentLevel);

			//Now delete all the tasks that are linked to this new requirement
			tasks = taskManager.RetrieveByRequirementId(copiedRequirementId);
			foreach (TaskView taskView in tasks)
			{
				taskManager.MarkAsDeleted(PROJECT_ID, taskView.TaskId, USER_ID_FRED_BLOGGS);
			}

			//Now delete the requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			//Verify that the other requirements returned correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 1);
			Assert.AreEqual("AAC", requirementView.IndentLevel);

			//Now lets try copying a single requirement to the end of the list
			copiedRequirementId = requirementManager.Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, 6, null);
			//Verify the new positions and that the data copied correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			Assert.AreEqual("AAEAAD", requirementView.IndentLevel);
			Assert.AreEqual("Ability to delete existing books in the system - Copy", requirementView.Name);
			Assert.AreEqual("<p>The ability to mark books as inactive, since technically nothing can get truly deleted for archival purposes.</p><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci.</p>", requirementView.Description);
			Assert.AreEqual(1, requirementView.ImportanceId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);
			Assert.AreEqual(9, requirementView.ReleaseId);
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(1, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(0, requirementView.CoverageCountCaution);
			Assert.AreEqual(0, requirementView.CoverageCountBlocked);

			//Now delete all the tasks that are linked to this new requirement
			tasks = taskManager.RetrieveByRequirementId(copiedRequirementId);
			foreach (TaskView taskView in tasks)
			{
				taskManager.MarkAsDeleted(PROJECT_ID, taskView.TaskId, USER_ID_FRED_BLOGGS);
			}

			//Now delete the requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);

			//******** Copying a Nested Set of Requirements ***********

			//First lets try copying a requirement sub-tree to a position at the same level
			copiedRequirementId = requirementManager.Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, 13, 22);
			//Verify the new positions and that the data copied correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			Assert.AreEqual("AACAAAAAF", requirementView.IndentLevel);
			Assert.AreEqual("Author Management - Copy", requirementView.Name);
			Assert.AreEqual("<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci. Aenean vel elit scelerisque mauris. Mollis nunc sed id semper risus. Amet massa vitae tortor condimentum lacinia quis vel. Vitae tortor condimentum lacinia quis vel. Massa eget egestas purus viverra accumsan in.</p>", requirementView.Description);
			Assert.AreEqual(2, requirementView.ImportanceId);
			Assert.IsFalse(requirementView.ReleaseId.HasValue);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);
			Assert.AreEqual(true, requirementView.IsSummary, "SummaryYn1");
			Assert.AreEqual(true, requirementView.IsExpanded, "ExpandedYn1");
			//Verify that the succeeding item was moved down one position
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 22);
			Assert.AreEqual("AACAAAAAG", requirementView.IndentLevel);
			//Now verify that the child requirements also copied correctly (names shouldn't change)
			requirementView = requirementManager.RetrieveByIndentLevel(USER_ID_FRED_BLOGGS, PROJECT_ID, "AACAAAAADAAA");
			Assert.AreEqual("AACAAAAADAAA", requirementView.IndentLevel);
			Assert.AreEqual("Ability to add new authors to the system", requirementView.Name);
			Assert.AreEqual("<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci.</p><ul> <li>Iaculis nunc sed augue lacus viverra vitae congue.</li> <li>Vestibulum mattis ullamcorper velit sed ullamcorper morbi tincidunt.</li> <li>Ac turpis egestas integer eget aliquet.</li></ul>", requirementView.Description);
			Assert.AreEqual(2, requirementView.ImportanceId);
			Assert.AreEqual(1, requirementView.ReleaseId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirementView.RequirementStatusId);
			Assert.AreEqual(true, requirementView.IsSummary, "SummaryYn2");
			Assert.AreEqual(false, requirementView.IsExpanded, "ExpandedYn2");

			//Now delete all the tasks that are linked to these new requirements
			tasks = taskManager.RetrieveByRequirementId(copiedRequirementId);
			foreach (TaskView taskView in tasks)
			{
				taskManager.MarkAsDeleted(PROJECT_ID, taskView.TaskId, USER_ID_FRED_BLOGGS);
			}
			List<RequirementView> childRequirements = requirementManager.RetrieveChildren(USER_ID_FRED_BLOGGS, PROJECT_ID, "AACAAAAAF", true);
			foreach (RequirementView childRequirement in childRequirements)
			{
				tasks = taskManager.RetrieveByRequirementId(childRequirement.RequirementId);
				foreach (TaskView taskView in tasks)
				{
					taskManager.MarkAsDeleted(PROJECT_ID, taskView.TaskId, USER_ID_FRED_BLOGGS);
				}
			}

			//Now delete the requirement (need to collapse first)
			requirementManager.Collapse(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			//Verify that the other requirements returned correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 22);
			Assert.AreEqual("AACAAAAAG", requirementView.IndentLevel);

			//Next lets try copying a requirement sub-tree that has coverage to the last position in the list
			copiedRequirementId = requirementManager.Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, 3, null);
			//Verify the new positions and that the data copied correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			Assert.AreEqual("AAEAAD", requirementView.IndentLevel);
			Assert.AreEqual("Book Management - Copy", requirementView.Name);
			Assert.AreEqual("<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci. Aenean vel elit scelerisque mauris. Mollis nunc sed id semper risus. Amet massa vitae tortor condimentum lacinia quis vel. Vitae tortor condimentum lacinia quis vel. Massa eget egestas purus viverra accumsan in.</p>", requirementView.Description);
			Assert.AreEqual(1, requirementView.ImportanceId);
			Assert.IsFalse(requirementView.ReleaseId.HasValue);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);
			Assert.AreEqual(true, requirementView.IsSummary, "SummaryYn1");
			Assert.AreEqual(true, requirementView.IsExpanded, "ExpandedYn1");
			Assert.AreEqual(17, requirementView.CoverageCountTotal);
			Assert.AreEqual(9, requirementView.CoverageCountPassed);
			Assert.AreEqual(2, requirementView.CoverageCountFailed);
			Assert.AreEqual(3, requirementView.CoverageCountCaution);
			Assert.AreEqual(0, requirementView.CoverageCountBlocked);

			//Now verify that the child requirements also copied correctly
			requirementView = requirementManager.RetrieveByIndentLevel(USER_ID_FRED_BLOGGS, PROJECT_ID, "AAEAADAAA");
			Assert.AreEqual("AAEAADAAA", requirementView.IndentLevel);
			Assert.AreEqual("Ability to add new books to the system", requirementView.Name);
			Assert.AreEqual("<p>The ability to add new books into the system, complete with ISBN, publisher and other related information.</p><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci.</p>", requirementView.Description);
			Assert.AreEqual(1, requirementView.ImportanceId);
			Assert.AreEqual(8, requirementView.ReleaseId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);
			Assert.AreEqual(false, requirementView.IsSummary, "SummaryYn2");
			Assert.AreEqual(false, requirementView.IsExpanded, "ExpandedYn2");
			Assert.AreEqual(3, requirementView.CoverageCountTotal);
			Assert.AreEqual(2, requirementView.CoverageCountPassed);
			Assert.AreEqual(1, requirementView.CoverageCountFailed);
			Assert.AreEqual(0, requirementView.CoverageCountCaution);
			Assert.AreEqual(0, requirementView.CoverageCountBlocked);

			//Also verify that attachment was copied across
			Assert.AreEqual(true, requirementView.IsAttachments);

			//Now delete all the tasks that are linked to these new requirements
			tasks = taskManager.RetrieveByRequirementId(copiedRequirementId);
			foreach (TaskView taskView in tasks)
			{
				taskManager.MarkAsDeleted(PROJECT_ID, taskView.TaskId, USER_ID_FRED_BLOGGS);
			}
			childRequirements = requirementManager.RetrieveChildren(USER_ID_FRED_BLOGGS, PROJECT_ID, "AAEAAD", true);
			foreach (RequirementView childRequirement in childRequirements)
			{
				tasks = taskManager.RetrieveByRequirementId(childRequirement.RequirementId);
				foreach (TaskView taskView in tasks)
				{
					taskManager.MarkAsDeleted(PROJECT_ID, taskView.TaskId, USER_ID_FRED_BLOGGS);
				}
			}

			//Now delete the requirement (need to collapse first)
			requirementManager.Collapse(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, copiedRequirementId);

			//Cleanup - need to remove specific releases from specific test cases to reset test case release coverage to before this test ran
			//This is required because the test cases in the sample data do not all have the releases of the requirements they are covering
			//In 6.7.1 TestCaseManager.AddtoRequirement was improved to add the test case to a release - RequirementManager.Copy in this test calls that method and makes those additions
			TestCaseManager testCaseManager = new TestCaseManager();
			testCaseManager.RemoveFromRelease(PROJECT_ID, 8, new List<int>() { 2, 4, 5, 6, 8, 9, 12 }, USER_ID_FRED_BLOGGS);
			testCaseManager.RemoveFromRelease(PROJECT_ID, 9, new List<int>() { 2, 3, 4, 5, 6, 8, 9, 12 }, USER_ID_FRED_BLOGGS);
			testCaseManager.RemoveFromRelease(PROJECT_ID, 10, new List<int>() { 2, 3, 4, 5, 6, 8, 9, 12 }, USER_ID_FRED_BLOGGS);
			testCaseManager.RemoveFromRelease(PROJECT_ID, 17, new List<int>() { 5, 6, 9, 12, 13 }, USER_ID_FRED_BLOGGS);
			testCaseManager.RemoveFromRelease(PROJECT_ID, 18, new List<int>() { 5, 6, 8, 9, 12, 13 }, USER_ID_FRED_BLOGGS);
			testCaseManager.RemoveFromRelease(PROJECT_ID, 19, new List<int>() { 6, 8, 9, 12, 13 }, USER_ID_FRED_BLOGGS);

			//Also need to reset a test case back to its original state in sample data in the context of a release
			InternalRoutines.ExecuteNonQuery("UPDATE TST_TEST_CASE SET EXECUTION_DATE = DATEADD(day, 0, SYSUTCDATETIME()) WHERE TEST_CASE_ID IN (2,3,4)");
			InternalRoutines.ExecuteNonQuery("UPDATE TST_TEST_CASE SET EXECUTION_STATUS_ID = 2, EXECUTION_DATE = DATEADD(day, -10, SYSUTCDATETIME()) WHERE TEST_CASE_ID = 8");
			InternalRoutines.ExecuteNonQuery("UPDATE TST_RELEASE_TEST_CASE SET EXECUTION_STATUS_ID = 2 WHERE RELEASE_ID = 4 AND TEST_CASE_ID = 8");
			InternalRoutines.ExecuteNonQuery("UPDATE TST_RELEASE_TEST_CASE_FOLDER SET COUNT_BLOCKED = 0, COUNT_CAUTION = 2, COUNT_FAILED = 2, COUNT_NOT_RUN = 1, COUNT_PASSED = 0 WHERE RELEASE_ID = 4 AND TEST_CASE_FOLDER_ID = 1");
			InternalRoutines.ExecuteNonQuery("UPDATE TST_RELEASE_TEST_CASE_FOLDER SET COUNT_BLOCKED = 1, COUNT_CAUTION = 0, COUNT_FAILED = 1, COUNT_NOT_RUN = 0, COUNT_PASSED = 2 WHERE RELEASE_ID = 1 AND TEST_CASE_FOLDER_ID = 1");
			InternalRoutines.ExecuteNonQuery("UPDATE TST_RELEASE SET COUNT_BLOCKED = 0, COUNT_CAUTION = 2, COUNT_FAILED = 2, COUNT_NOT_RUN = 2, COUNT_PASSED = 3 WHERE RELEASE_ID = 4");
		}

		[
		Test,
		SpiraTestCase(358)
		]
		public void _14_ExportRequirements()
		{
			//First we need to create a new project that we want to copy to
			Business.ProjectManager projectManager = new Business.ProjectManager();
			TemplateManager templateManager = new TemplateManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			int destProjectId = projectManager.Insert("Test Export Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			int destProjectTemplateId = templateManager.RetrieveForProject(destProjectId).ProjectTemplateId;

			//Now export a whole tree of requirements over to this new project
			requirementId1 = requirementManager.Export(USER_ID_FRED_BLOGGS, 1, 3, destProjectId);

			//Now verify that they exported correctly

			//First the summary item containing the other requirements
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, destProjectId, requirementId1);
			Assert.AreEqual("Book Management", requirementView.Name);
			Assert.AreEqual(0, requirementView.CoverageCountTotal);
			Assert.AreEqual(true, requirementView.IsSummary);
			Assert.AreEqual(false, requirementView.IsAttachments);

			//Next verify that the children exported correctly
			requirements = requirementManager.RetrieveChildren(USER_ID_FRED_BLOGGS, destProjectId, requirementView.IndentLevel, true);
			Assert.AreEqual(7, requirements.Count);
			//First Item
			Assert.AreEqual("Ability to add new books to the system", requirements[0].Name);
			Assert.AreEqual(0, requirements[0].CoverageCountTotal);
			Assert.AreEqual(false, requirements[0].IsSummary);
			Assert.AreEqual(true, requirements[0].IsAttachments);
			//Last Item
			Assert.AreEqual("Ability to completely erase all books stored in the system with one click", requirements[6].Name);
			Assert.AreEqual(0, requirements[6].CoverageCountTotal);
			Assert.AreEqual(false, requirements[6].IsSummary);
			Assert.AreEqual(false, requirements[6].IsAttachments);

			//Finally delete the project/template to clean-up
			projectManager.Delete(USER_ID_FRED_BLOGGS, destProjectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, destProjectTemplateId);
		}

		/// <summary>
		/// Verifies that the effort values and task progress values roll-up correctly when tasks change
		/// </summary>
		[
		Test,
		SpiraTestCase(381)
		]
		public void _15_TaskProgressAndEffortRollups()
		{
			//First create a simple requirements hierarchy
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, null, "Parent Requirement", null, null, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, null, "Child Requirement 1", null, 0.5M, USER_ID_FRED_BLOGGS);
			requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, null, "Child Requirement 2", null, 1.0M, USER_ID_FRED_BLOGGS);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId3);

			//Also need to create a release for assigning tasks to
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Test Release", null, "1.0.0.1", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now.AddMonths(-2), DateTime.Now.AddMonths(2), 2, 0, null, false);

			//Verify that the planned effort rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(720, requirementView.EstimatedEffort);
			Assert.AreEqual(1.5M, requirementView.EstimatePoints);
			Assert.AreEqual(0, requirementView.TaskCount);

			//Now edit one of the planned efforts and verify the rollup
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.EstimatePoints = 3.5M;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify that the planned effort rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1920, requirementView.EstimatedEffort);
			Assert.AreEqual(4.0M, requirementView.EstimatePoints);

			//Now we need to attach some tasks to the requirements and verify that the effort and progress values
			//aggregate correctly to the requirements
			TaskManager taskManager = new TaskManager();
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId2, null, null, null, "Task 1", "", DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), 60, null, null, USER_ID_FRED_BLOGGS);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId2, null, null, null, "Task 2", "", DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), 45, null, null, USER_ID_FRED_BLOGGS);
			int taskId3 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId3, null, null, null, "Task 2", "", DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), 100, null, null, USER_ID_FRED_BLOGGS);
			int taskId4 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId3, null, null, null, "Task 2", "", DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), 70, null, null, USER_ID_FRED_BLOGGS);

			//Verify that the task estimated effort, task count and progress rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1920, requirementView.EstimatedEffort);
			Assert.AreEqual(4.0M, requirementView.EstimatePoints);
			Assert.AreEqual(275, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(0, requirementView.TaskActualEffort);
			Assert.AreEqual(275, requirementView.TaskRemainingEffort);
			Assert.AreEqual(275, requirementView.TaskProjectedEffort);
			Assert.AreEqual(4, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(0, requirementView.TaskPercentLateStart);
			Assert.AreEqual(0, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(100, requirementView.TaskPercentNotStart);

			//Now lets make one of the tasks start late
			Task task = taskManager.RetrieveById(taskId1);
			task.StartTracking();
			task.StartDate = DateTime.UtcNow.AddDays(-1);
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify that the task estimated effort, task count and progress rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1920, requirementView.EstimatedEffort);
			Assert.AreEqual(4.0M, requirementView.EstimatePoints);
			Assert.AreEqual(275, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(0, requirementView.TaskActualEffort);
			Assert.AreEqual(275, requirementView.TaskRemainingEffort);
			Assert.AreEqual(275, requirementView.TaskProjectedEffort);
			Assert.AreEqual(4, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(25, requirementView.TaskPercentLateStart);
			Assert.AreEqual(0, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(75, requirementView.TaskPercentNotStart);

			//Now lets make one of the other tasks finish late
			//Need to associate it with a release first to pass the business rule validation
			task = taskManager.RetrieveById(taskId3);
			task.StartTracking();
			task.ReleaseId = releaseId;
			task.StartDate = DateTime.Now.AddDays(-2);
			task.EndDate = DateTime.Now.AddDays(-1);
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			task.RemainingEffort = 50;   //50%
			task.ActualEffort = 90;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify that the task estimated effort, task count and progress rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1920, requirementView.EstimatedEffort);
			Assert.AreEqual(4.0M, requirementView.EstimatePoints);
			Assert.AreEqual(275, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(90, requirementView.TaskActualEffort);
			Assert.AreEqual(225, requirementView.TaskRemainingEffort);
			Assert.AreEqual(315, requirementView.TaskProjectedEffort);
			Assert.AreEqual(4, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(25, requirementView.TaskPercentLateStart);
			Assert.AreEqual(12, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(63, requirementView.TaskPercentNotStart);

			//Now delete one task and deassociate another
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
			taskManager.RemoveRequirementAssociation(taskId4, USER_ID_FRED_BLOGGS);

			//Verify that the task estimated effort, task count and progress rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1920, requirementView.EstimatedEffort);
			Assert.AreEqual(4.0M, requirementView.EstimatePoints);
			Assert.AreEqual(160, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(90, requirementView.TaskActualEffort);
			Assert.AreEqual(110, requirementView.TaskRemainingEffort);
			Assert.AreEqual(200, requirementView.TaskProjectedEffort);
			Assert.AreEqual(2, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(50, requirementView.TaskPercentLateStart);
			Assert.AreEqual(25, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(25, requirementView.TaskPercentNotStart);

			//Reassociate the task back and verify
			task = taskManager.RetrieveById(taskId4);
			task.StartTracking();
			task.RequirementId = requirementId3;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify that the task estimated effort, task count and progress rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1920, requirementView.EstimatedEffort);
			Assert.AreEqual(4.0M, requirementView.EstimatePoints);
			Assert.AreEqual(230, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(90, requirementView.TaskActualEffort);
			Assert.AreEqual(180, requirementView.TaskRemainingEffort);
			Assert.AreEqual(270, requirementView.TaskProjectedEffort);
			Assert.AreEqual(3, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(33, requirementView.TaskPercentLateStart);
			Assert.AreEqual(16, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(51, requirementView.TaskPercentNotStart);

			//Now deassociate, but this time use the taskManager.Update method instead
			task = taskManager.RetrieveById(taskId4);
			task.StartTracking();
			task.RequirementId = null;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1920, requirementView.EstimatedEffort);
			Assert.AreEqual(4.0M, requirementView.EstimatePoints);
			Assert.AreEqual(160, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(90, requirementView.TaskActualEffort);
			Assert.AreEqual(110, requirementView.TaskRemainingEffort);
			Assert.AreEqual(200, requirementView.TaskProjectedEffort);
			Assert.AreEqual(2, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(50, requirementView.TaskPercentLateStart);
			Assert.AreEqual(25, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(25, requirementView.TaskPercentNotStart);

			//Now outdent a detail requirement and verify that the summary requirement changes
			requirementManager.Outdent(USER_ID_FRED_BLOGGS, projectId, requirementId3);

			//Verify that the task estimated effort, task count and progress rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(240, requirementView.EstimatedEffort);
			Assert.AreEqual(0.5M, requirementView.EstimatePoints);
			Assert.AreEqual(60, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(0, requirementView.TaskActualEffort);
			Assert.AreEqual(60, requirementView.TaskRemainingEffort);
			Assert.AreEqual(60, requirementView.TaskProjectedEffort);
			Assert.AreEqual(1, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(100, requirementView.TaskPercentLateStart);
			Assert.AreEqual(0, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(0, requirementView.TaskPercentNotStart);

			//Now re-indent the detail requirement and verify that the summary requirement changes back
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId3);

			//Verify that the task estimated effort, task count and progress rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1920, requirementView.EstimatedEffort);
			Assert.AreEqual(4.0M, requirementView.EstimatePoints);
			Assert.AreEqual(160, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(90, requirementView.TaskActualEffort);
			Assert.AreEqual(110, requirementView.TaskRemainingEffort);
			Assert.AreEqual(200, requirementView.TaskProjectedEffort);
			Assert.AreEqual(2, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(50, requirementView.TaskPercentLateStart);
			Assert.AreEqual(25, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(25, requirementView.TaskPercentNotStart);

			//Now we need to delete a requirement and verify that the totals roll-up correctly to the summary
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId2);

			//Verify that the task estimated effort, task count and progress rolled up correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(1680, requirementView.EstimatedEffort);
			Assert.AreEqual(3.5M, requirementView.EstimatePoints);
			Assert.AreEqual(100, requirementView.TaskEstimatedEffort);
			Assert.AreEqual(90, requirementView.TaskActualEffort);
			Assert.AreEqual(50, requirementView.TaskRemainingEffort);
			Assert.AreEqual(140, requirementView.TaskProjectedEffort);
			Assert.AreEqual(1, requirementView.TaskCount);
			Assert.AreEqual(0, requirementView.TaskPercentOnTime);
			Assert.AreEqual(0, requirementView.TaskPercentLateStart);
			Assert.AreEqual(50, requirementView.TaskPercentLateFinish);
			Assert.AreEqual(50, requirementView.TaskPercentNotStart);

			//Finally clean up
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId3, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId4, USER_ID_FRED_BLOGGS);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
		}

		/// <summary>
		/// Test that we can create a new requirement from an already existing incident
		/// </summary>
		[
		Test,
		SpiraTestCase(385)
		]
		public void _16_CreateFromIncident()
		{
			//First lets create an incident in the empty project - associate with a sample release as well.
			//We need to specify a priority to make sure that gets converted into a requirement importance
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			IncidentManager incidentManager = new IncidentManager();
			int incidentPriorityId = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).FirstOrDefault(i => i.Name == "2 - High").PriorityId;
			int incidentId = incidentManager.Insert(projectId, incidentPriorityId, null, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, null, "Incident 1", "Incident 1 Description", releaseId, releaseId, null, null, null, DateTime.Now, null, null, 125, 130, 125, null, null, USER_ID_FRED_BLOGGS);
			incidentManager.InsertResolution(incidentId, "Resolution 1", DateTime.Now, USER_ID_FRED_BLOGGS, false);

			//We also need to add a list custom property to make sure it gets copied across
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			int customListId1 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Test List", true, true).CustomPropertyListId;
			int customValueId1 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Value 1").CustomPropertyValueId;
			int customValueId2 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Value 2").CustomPropertyValueId;
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "Test Prop", null, null, customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.List, 2, "Test Prop", null, null, customListId1);
			ArtifactCustomProperty acp = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId);
			acp.Custom_01 = customValueId1.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(acp, USER_ID_FRED_BLOGGS);

			//We also need to attach a document to make sure it gets copied across
			AttachmentManager attachmentManager = new AttachmentManager();
			int attachmentId = attachmentManager.Insert(projectId, "www.x.com", null, USER_ID_FRED_BLOGGS, incidentId, Artifact.ArtifactTypeEnum.Incident, "1.0", null, null, null, null);

			//Now create a requirement from this incident
			requirementId1 = requirementManager.CreateFromIncident(incidentId, USER_ID_FRED_BLOGGS);

			//Verify the data
			int requirementImportanceId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Name == "2 - High").ImportanceId;
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual("Incident 1", requirementView.Name);
			Assert.AreEqual("Incident 1 Description", requirementView.Description);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, requirementView.AuthorId);
			Assert.AreEqual(USER_ID_JOE_SMITH, requirementView.OwnerId);
			Assert.AreEqual(releaseId, requirementView.ReleaseId);
			Assert.AreEqual(0.3M, requirementView.EstimatePoints);
			Assert.AreEqual(144, requirementView.EstimatedEffort);
			Assert.AreEqual(requirementImportanceId, requirementView.ImportanceId);

			//Verify that a comment was added to the requirement to match the incident
			IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(requirementId1, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(1, comments.Count());
			Assert.AreEqual("Resolution 1", comments.First().Text);

			//Verify the attachment was copied across
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, requirementId1, Artifact.ArtifactTypeEnum.Requirement, null, true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual(attachmentId, attachments[0].AttachmentId);

			//Verify the custom property
			acp = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId1, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(customValueId1, acp.Custom_02.FromDatabaseSerialization_Int32());

			//Clean up
			customPropertyManager.ArtifactCustomProperty_DeleteByArtifactId(incidentId, Artifact.ArtifactTypeEnum.Incident);
			customPropertyManager.ArtifactCustomProperty_DeleteByArtifactId(requirementId1, Artifact.ArtifactTypeEnum.Requirement);
			incidentManager.MarkAsDeleted(projectId, incidentId, USER_ID_FRED_BLOGGS);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);
			attachmentManager.Delete(projectId, attachmentId, 1);
		}

		/// <summary>
		/// Tests that changing the status of tasks associated with a requirement will change
		/// the status of the requirement itself
		/// </summary>
		[
		Test,
		SpiraTestCase(442)
		]
		public void _17_TaskStatusRollup()
		{
			//First locate an existing requirement in the status Completed
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Completed, requirementView.RequirementStatusId);

			//Now add a new task to this requirement
			TaskManager taskManager = new TaskManager();
			int taskId1 = taskManager.Insert(
				PROJECT_ID,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				7,
				null,
				null,
				3,
				"Test Task 1",
				"Description of test task",
				null,
				null,
				300,
				null,
				null, USER_ID_FRED_BLOGGS);

			//Now verify that the status of the requirement changed to In-Progress
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);

			//Also verify that the parent requirement changes as well
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);

			//If we move the task to a different requirement, it should change the status of both requirements
			Task task = taskManager.RetrieveById(taskId1);
			task.StartTracking();
			task.RequirementId = 8;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Now verify that the status of the two requirements
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);

			//Move the status of the Task to Completed, the requirements should change back to Tested
			task = taskManager.RetrieveById(taskId1);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Tested, requirementView.RequirementStatusId);

			//If we switch the Task to In Progress, the requirements should change back to In Progress
			task = taskManager.RetrieveById(taskId1);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);

			//If we delete the task, the requirement should change back to Tested
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, USER_ID_FRED_BLOGGS);

			//Verify the requirements and their parent
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Tested, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);


			//Finally switch back to Completed to match the starting state
			Requirement requirement = requirementManager.RetrieveById3(PROJECT_ID, 8);
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Completed;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });
			requirement = requirementManager.RetrieveById3(PROJECT_ID, 7);
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Completed;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });

			//Verify the requirements and their parent
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 7);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Completed, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 8);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Completed, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);
		}

		[
		Test,
		SpiraTestCase(443)
		]
		public void _18_PrefillData()
		{
			//Need to test that when we assign a release to a requirement that doesn't have a release set,
			//the status of the release automatically switches to Planned
			//This is a configurable option at the project level, so we need to first make sure that this setting is enabled
			ProjectManager projectManager = new ProjectManager();
			Project project = projectManager.RetrieveById(PROJECT_ID);
			Assert.IsTrue(project.IsReqStatusAutoPlanned);

			//Create the requirement we're testing against
			requirementId4 = requirementManager.Insert(
				USER_ID_FRED_BLOGGS,
				PROJECT_ID,
				null,
				null,
				6,
				Requirement.RequirementStatusEnum.Requested,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				1,
				"Test Requirement ",
				"",
				null,
				USER_ID_FRED_BLOGGS);

			//Verify the requirement and status
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Requested, requirementView.RequirementStatusId);
			Assert.IsFalse(requirementView.ReleaseId.HasValue);

			//Also verify the status of the parent requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);

			//Now set a release value
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ReleaseId = 1;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });

			//Verify the release and status
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirementView.RequirementStatusId);
			Assert.AreEqual(1, requirementView.ReleaseId);

			//Also verify the status of the parent requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);

			//Clean up
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);

			//Finally verify the status of the parent requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Now we try again, but this time, disable the project auto-plan setting first
			project = projectManager.RetrieveById(PROJECT_ID);
			project.StartTracking();
			project.IsReqStatusAutoPlanned = false;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Create the requirement we're testing against
			requirementId4 = requirementManager.Insert(
				USER_ID_FRED_BLOGGS,
				PROJECT_ID,
				null,
				null,
				6,
				Requirement.RequirementStatusEnum.Requested,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				1,
				"Test Requirement ",
				"",
				null,
				USER_ID_FRED_BLOGGS);

			//Verify the requirement and status
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Requested, requirementView.RequirementStatusId);
			Assert.IsFalse(requirementView.ReleaseId.HasValue);

			//Also verify the status of the parent requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);

			//Now set a release value
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ReleaseId = 1;
			requirementManager.Update(USER_ID_FRED_BLOGGS, PROJECT_ID, new List<Requirement>() { requirement });

			//Verify the release and status (should be unchanged)
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Requested, requirementView.RequirementStatusId);
			Assert.AreEqual(1, requirementView.ReleaseId);

			//Also verify the status of the parent requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirementView.RequirementStatusId);

			//Clean up
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId4);

			//Verify the status of the parent requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 3);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);

			//Finally set the project auto-plan setting back
			project = projectManager.RetrieveById(PROJECT_ID);
			project.StartTracking();
			project.IsReqStatusAutoPlanned = true;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");
		}

		/// <summary>
		/// Tests that when we assign a requirement from one release to another, any tasks associated with the requirement
		/// that are still in the Not-Started or Deferred Statuses, will get assigned to the new release
		/// </summary>
		[
		Test,
		SpiraTestCase(444)
		]
		public void _19_ReleaseChangesPropagateToTask()
		{
			int rq_importanceCriticalId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 1).ImportanceId;

			//First create a new requirement and release
			requirementId4 = requirementManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.Requested,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				rq_importanceCriticalId,
				"Test Requirement ",
				"",
				null
				, USER_ID_FRED_BLOGGS);
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 1.0",
				"Test Description of Release 1.0",
				"1.0.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.Now.Date,
				DateTime.Now.AddDays(10).Date,
				2,
				0,
				null,
				false);
			int releaseId2 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 2.0",
				"Test Description of Release 2.0",
				"2.0.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.Now.Date,
				DateTime.Now.AddDays(10).Date,
				2,
				0,
				null,
				false
				);

			//Now associate some tasks with the requirement
			TaskManager taskManager = new TaskManager();
			int tk_priorityMediumId = taskManager.TaskPriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 3).TaskPriorityId;
			int taskId1 = taskManager.Insert(
				projectId,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				requirementId4,
				null,
				null,
				tk_priorityMediumId,
				"Test Task 1",
				"Description of test task",
				null,
				null,
				300,
				null,
				null, USER_ID_FRED_BLOGGS);
			int taskId2 = taskManager.Insert(
				projectId,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.Deferred,
				null,
				null,
				requirementId4,
				null,
				null,
				tk_priorityMediumId,
				"Test Task 2",
				"Description of test task",
				null,
				null,
				300,
				null,
				null, USER_ID_FRED_BLOGGS);
			int taskId3 = taskManager.Insert(
				projectId,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.Blocked,
				null,
				null,
				requirementId4,
				null,
				null,
				tk_priorityMediumId,
				"Test Task 3",
				"Description of test task",
				null,
				null,
				300,
				null,
				null, USER_ID_FRED_BLOGGS);

			//Verify data
			List<TaskView> tasks = taskManager.RetrieveByRequirementId(requirementId4);
			Assert.IsFalse(tasks.FirstOrDefault(t => t.TaskId == taskId1).ReleaseId.HasValue);
			Assert.IsFalse(tasks.FirstOrDefault(t => t.TaskId == taskId2).ReleaseId.HasValue);
			Assert.IsFalse(tasks.FirstOrDefault(t => t.TaskId == taskId3).ReleaseId.HasValue);

			//Now associate the requirement with a release and verify that the non-started tasks pick up the change
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ReleaseId = releaseId;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify data
			tasks = taskManager.RetrieveByRequirementId(requirementId4);
			Assert.AreEqual(releaseId, tasks.FirstOrDefault(t => t.TaskId == taskId1).ReleaseId);
			Assert.AreEqual(releaseId, tasks.FirstOrDefault(t => t.TaskId == taskId2).ReleaseId);
			Assert.IsFalse(tasks.FirstOrDefault(t => t.TaskId == taskId3).ReleaseId.HasValue);

			//Clean up tasks
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId3, USER_ID_FRED_BLOGGS);

			//Finally need to verify that requirements with no tasks also work correctly
			//Fixes a bug introduced in 2.3.1 patch 18
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ReleaseId = releaseId2;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			Assert.AreEqual(releaseId2, requirementView.ReleaseId);

			//Clean up requirements, releases
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
		}

		/// <summary>
		/// Verify that multiple, concurrent updates to requirements are handled correctly
		/// in accordance with optimistic concurrency rules
		/// </summary>
		[
		Test,
		SpiraTestCase(670)
		]
		public void _20_Concurrency_Handling()
		{
			int rq_importanceCriticalId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 1).ImportanceId;
			int rq_importanceHighId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 2).ImportanceId;
			int rq_importanceLowId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 4).ImportanceId;

			//First we need to create a new requirement to verify the handling
			int requirementId = requirementManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.Requested,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				rq_importanceCriticalId,
				"Test Requirement ",
				"",
				null
				, USER_ID_FRED_BLOGGS);

			//Now retrieve the requirement back and keep a copy of the entity
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Requirement requirement1 = requirementView.ConvertTo<RequirementView, Requirement>();
			Requirement requirement2 = requirementView.ConvertTo<RequirementView, Requirement>();

			//Now make a change to field and update
			requirement1.StartTracking();
			requirement1.ImportanceId = rq_importanceHighId;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement1 });

			//Verify it updated correctly using separate dataset
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Assert.AreEqual(rq_importanceHighId, requirementView.ImportanceId);

			//Now try making a change using the out of date dataset (has the wrong ConcurrencyDate)
			bool exceptionThrown = false;
			try
			{
				requirement2.StartTracking();
				requirement2.ImportanceId = rq_importanceLowId;
				requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement2 });
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Assert.AreEqual(rq_importanceHighId, requirementView.ImportanceId);

			//Now refresh the old dataset and try again and verify it works
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			requirement2 = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement2.StartTracking();
			requirement2.ImportanceId = rq_importanceLowId;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement2 });

			//Verify it updated correctly using separate dataset
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Assert.AreEqual(rq_importanceLowId, requirementView.ImportanceId);

			//Clean up
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId);
		}

		[
		Test,
		SpiraTestCase(719)
		]
		public void _21_IterationScheduling()
		{
			//This tests that we can associate requirements with an iteration and unassociate;
			//these calls are used in the Iteration Planning screen

			//First create a new iteration inside the empty project
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.0.0000", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			//Now lets create a couple of unassociated requirements
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, null, "Parent Requirement", null, null, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, null, "Child Requirement 1", null, 0.5M, USER_ID_FRED_BLOGGS);

			//Verify that the requirements are unassociated
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsFalse(requirementView.ReleaseId.HasValue);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Accepted, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			Assert.IsFalse(requirementView.ReleaseId.HasValue);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Accepted, requirementView.RequirementStatusId);

			//Now associate the requirements with the iteration
			requirementManager.AssociateToIteration(new List<int> { requirementId1, requirementId2 }, releaseId, USER_ID_FRED_BLOGGS);

			//Verify that they are associated and the status changed to 'Planned'
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(releaseId, requirementView.ReleaseId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			Assert.AreEqual(releaseId, requirementView.ReleaseId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirementView.RequirementStatusId);

			//Finally verify that we can deassociate the requirements
			requirementManager.RemoveReleaseAssociation(USER_ID_FRED_BLOGGS, new List<int> { requirementId1, requirementId2 });

			//Verify that the requirements are unassociated
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsFalse(requirementView.ReleaseId.HasValue);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Accepted, requirementView.RequirementStatusId);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			Assert.IsFalse(requirementView.ReleaseId.HasValue);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Accepted, requirementView.RequirementStatusId);

			//Also we need to test that we can assign requirements to a user and retreive
			//(used in the planning board screens)

			//First verify that the user has no assigned requirements in the current project
			requirements = requirementManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, projectId, null, false);
			Assert.AreEqual(0, requirements.Count);

			//Next assign a requirement to the user
			requirementManager.AssignToUser(requirementId1, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS);
			requirements = requirementManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, projectId, null, false);
			Assert.AreEqual(1, requirements.Count);

			//Next de-assign the requirement
			requirementManager.AssignToUser(requirementId1, null, USER_ID_FRED_BLOGGS);
			requirements = requirementManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, projectId, null, false);
			Assert.AreEqual(0, requirements.Count);

			//Now verify that we can find the requirements that are not assigned to anyone
			requirements = requirementManager.RetrieveByOwnerId(null, projectId, null, false);
			Assert.AreEqual(2, requirements.Count);

			//Clean up
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);
		}

		/// <summary>
		/// Tests that we can have different requirement types and that summary requirements are always displayed
		/// as the special 'Package' type, but that original type is stored in the database
		/// </summary>
		[Test]
		[SpiraTestCase(1211)]
		public void _22_HandleRequirementTypes()
		{
			List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
			int rq_typeUseCaseId = requirementTypes.FirstOrDefault(t => t.Name == "Use Case").RequirementTypeId;
			int rq_typeQualityId = requirementTypes.FirstOrDefault(t => t.Name == "Quality").RequirementTypeId;
			int rq_typeUserStoryId = requirementTypes.FirstOrDefault(t => t.Name == "User Story").RequirementTypeId;
			int rq_typeFeatureId = requirementTypes.FirstOrDefault(t => t.Name == "Feature").RequirementTypeId;

			//First, need to create a hierarchy of requirements, which are assigned to different types
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 2", null, 60, USER_ID_FRED_BLOGGS);
			requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, rq_typeUseCaseId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 3", null, 120, USER_ID_FRED_BLOGGS);
			requirementId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, rq_typeQualityId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 4", null, 240, USER_ID_FRED_BLOGGS);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId4);

			//First get the list of requirements unfiltered, to verify
			//The summary requirement should have it's physical type not 'package'
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual(true, requirements[0].IsSummary);
			Assert.AreEqual(rq_typeFeatureId, requirements[0].RequirementTypeId);
			Assert.AreEqual("Epic", requirements[0].RequirementTypeName);

			Assert.AreEqual("Requirement 2", requirements[1].Name);
			Assert.AreEqual(false, requirements[1].IsSummary);
			Assert.AreEqual(rq_typeFeatureId, requirements[1].RequirementTypeId);
			Assert.AreEqual("Feature", requirements[1].RequirementTypeName);

			Assert.AreEqual("Requirement 3", requirements[2].Name);
			Assert.AreEqual(false, requirements[2].IsSummary);
			Assert.AreEqual(rq_typeUseCaseId, requirements[2].RequirementTypeId);
			Assert.AreEqual("Use Case", requirements[2].RequirementTypeName);

			Assert.AreEqual("Requirement 4", requirements[3].Name);
			Assert.AreEqual(false, requirements[3].IsSummary);
			Assert.AreEqual(rq_typeQualityId, requirements[3].RequirementTypeId);
			Assert.AreEqual("Quality", requirements[3].RequirementTypeName);

			//Now lets retrieve all the requirements of a specific type (includes parents)
			Hashtable filters = new Hashtable();
			filters.Add("RequirementTypeId", rq_typeFeatureId);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual("Requirement 2", requirements[1].Name);

			//Now lets retrieve all the requirements associated with a selection of types
			filters.Clear();
			MultiValueFilter mvf = new MultiValueFilter();
			mvf.Values.Add(rq_typeFeatureId);
			mvf.Values.Add(rq_typeQualityId);
			filters.Add("RequirementTypeId", mvf);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual("Requirement 2", requirements[1].Name);
			Assert.AreEqual("Requirement 4", requirements[2].Name);

			//Next lets change the type of one of the requirements
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementTypeId = rq_typeUserStoryId;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify the change
			filters.Clear();
			filters.Add("RequirementTypeId", rq_typeUserStoryId);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual(true, requirements[0].IsSummary);
			Assert.AreEqual(rq_typeFeatureId, requirements[0].RequirementTypeId);
			Assert.AreEqual("Epic", requirements[0].RequirementTypeName);
			Assert.AreEqual("Requirement 3", requirements[1].Name);
			Assert.AreEqual(false, requirements[1].IsSummary);
			Assert.AreEqual(rq_typeUserStoryId, requirements[1].RequirementTypeId);
			Assert.AreEqual("User Story", requirements[1].RequirementTypeName);

			//Next verify that history entries were correctly recorded
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(requirementId3, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(2, historyChangeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			//Assert.AreEqual(1, historyChangeSets[0].Details.Count);
			//HistoryDetail historyDetail = historyChangeSets[0].Details[0];
			//The entries are newest first
			//Assert.AreEqual(rq_typeUseCaseId, historyDetail.OldValueInt);
			//Assert.AreEqual("Use Case", historyDetail.OldValue);
			//Assert.AreEqual(rq_typeUserStoryId, historyDetail.NewValueInt);
			//Assert.AreEqual("User Story", historyDetail.NewValue);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);

			//Now test that making a requirement into a summary leaves it type alone
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			Assert.AreEqual(rq_typeUserStoryId, requirementView.RequirementTypeId);
			Assert.AreEqual("User Story", requirementView.RequirementTypeName);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId4);

			//Verify
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			Assert.AreEqual(rq_typeUserStoryId, requirementView.RequirementTypeId);
			Assert.AreEqual("Epic", requirementView.RequirementTypeName);

			//Now test that making a summary requirement into a normal one, leaves it type alone
			requirementManager.Outdent(USER_ID_FRED_BLOGGS, projectId, requirementId4);

			//Verify
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			Assert.AreEqual(rq_typeUserStoryId, requirementView.RequirementTypeId);
			Assert.AreEqual("User Story", requirementView.RequirementTypeName);

			//Finally delete the requirement tree (components will be deleted during tear-down)
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);

		}

		/// <summary>
		/// Tests that we can add/remove/edit scenario steps to a requirement
		/// </summary>
		[Test]
		[SpiraTestCase(1213)]
		public void _23_CreateEditDeleteSteps()
		{
			List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
			int rq_typeUseCaseId = requirementTypes.FirstOrDefault(t => t.Name == "Use Case").RequirementTypeId;

			//First create a new requirement that we'll add steps to
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, rq_typeUseCaseId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);

			//Retrieve this requirement (used later on)
			RequirementView requirement1 = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			DateTime previousLastUpdateDate = requirement1.LastUpdateDate;

			//Next verify that it doesn't have any steps initially
			int count = requirementManager.CountSteps(requirementId1);
			Assert.AreEqual(0, count);

			//Now lets add some steps to this requirement
			int requirementStepId1 = requirementManager.InsertStep(projectId, requirementId1, null, "First do this", USER_ID_FRED_BLOGGS);
			int requirementStepId2 = requirementManager.InsertStep(projectId, requirementId1, null, "Then do this", USER_ID_FRED_BLOGGS);
			int requirementStepId3 = requirementManager.InsertStep(projectId, requirementId1, null, "Finally do this", USER_ID_FRED_BLOGGS);

			//Verify that the count increased
			count = requirementManager.CountSteps(requirementId1);
			Assert.AreEqual(3, count);

			//Verify that we can retrieve the list of steps
			List<RequirementStep> requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(3, requirementSteps.Count);
			Assert.AreEqual("First do this", requirementSteps[0].Description);
			Assert.AreEqual(1, requirementSteps[0].Position);
			Assert.AreEqual("Then do this", requirementSteps[1].Description);
			Assert.AreEqual(2, requirementSteps[1].Position);
			Assert.AreEqual("Finally do this", requirementSteps[2].Description);
			Assert.AreEqual(3, requirementSteps[2].Position);

			//Verify that we can retrieve one step
			RequirementStep requirementStep = requirementManager.RetrieveStepById(requirementStepId1, false);
			Assert.AreEqual("First do this", requirementStep.Description);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId1, false, true);
			Assert.AreEqual("First do this", requirementStep.Description);
			Assert.AreEqual(projectId, requirementStep.ProjectId);

			//Verify that we can retrieve one step with its project id field populated

			//Verify that the last updated date of the requirement was changed
			RequirementView requirement2 = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsTrue(requirement2.LastUpdateDate > previousLastUpdateDate);
			previousLastUpdateDate = requirement2.LastUpdateDate;

			//Now verify that we can add a step between two of them, with a specific creation date
			int requirementStepId4 = requirementManager.InsertStep(projectId, requirementId1, requirementStepId3, "Forgot to do this", USER_ID_FRED_BLOGGS, DateTime.UtcNow.AddDays(-1));

			//Verify the insertion
			requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(4, requirementSteps.Count);
			Assert.AreEqual("First do this", requirementSteps[0].Description);
			Assert.AreEqual(1, requirementSteps[0].Position);
			Assert.AreEqual("Then do this", requirementSteps[1].Description);
			Assert.AreEqual(2, requirementSteps[1].Position);
			Assert.AreEqual("Forgot to do this", requirementSteps[2].Description);
			Assert.AreEqual(3, requirementSteps[2].Position);
			Assert.AreEqual("Finally do this", requirementSteps[3].Description);
			Assert.AreEqual(4, requirementSteps[3].Position);

			//Verify that the last updated date of the requirement was changed
			requirement2 = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsTrue(requirement2.LastUpdateDate > previousLastUpdateDate);
			previousLastUpdateDate = requirement2.LastUpdateDate;

			//Now move the new step to the end of the list
			requirementManager.MoveStep(requirementId1, requirementStepId4, null, USER_ID_FRED_BLOGGS);
			//Verify the move
			requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(4, requirementSteps.Count);
			Assert.AreEqual("First do this", requirementSteps[0].Description);
			Assert.AreEqual(1, requirementSteps[0].Position);
			Assert.AreEqual("Then do this", requirementSteps[1].Description);
			Assert.AreEqual(2, requirementSteps[1].Position);
			Assert.AreEqual("Finally do this", requirementSteps[2].Description);
			Assert.AreEqual(3, requirementSteps[2].Position);
			Assert.AreEqual("Forgot to do this", requirementSteps[3].Description);
			Assert.AreEqual(4, requirementSteps[3].Position);

			//Verify that the last updated date of the requirement was changed
			requirement2 = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsTrue(requirement2.LastUpdateDate > previousLastUpdateDate);
			previousLastUpdateDate = requirement2.LastUpdateDate;

			//Move it back
			requirementManager.MoveStep(requirementId1, requirementStepId4, requirementStepId3, USER_ID_SYS_ADMIN);
			//Verify the move
			requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(4, requirementSteps.Count);
			Assert.AreEqual("First do this", requirementSteps[0].Description);
			Assert.AreEqual(1, requirementSteps[0].Position);
			Assert.AreEqual("Then do this", requirementSteps[1].Description);
			Assert.AreEqual(2, requirementSteps[1].Position);
			Assert.AreEqual("Forgot to do this", requirementSteps[2].Description);
			Assert.AreEqual(3, requirementSteps[2].Position);
			Assert.AreEqual("Finally do this", requirementSteps[3].Description);
			Assert.AreEqual(4, requirementSteps[3].Position);

			//Verify that the last updated date of the requirement was changed
			requirement2 = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsTrue(requirement2.LastUpdateDate > previousLastUpdateDate);
			previousLastUpdateDate = requirement2.LastUpdateDate;

			//Now test that we can modify a step
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4);
			requirementStep.StartTracking();
			requirementStep.Description = "Missed doing this";
			requirementManager.UpdateStep(projectId, requirementStep, USER_ID_FRED_BLOGGS);

			//Verify change
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4);
			Assert.AreEqual("Missed doing this", requirementStep.Description);

			//Verify that the last updated date of the requirement was changed
			requirement2 = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsTrue(requirement2.LastUpdateDate > previousLastUpdateDate);
			previousLastUpdateDate = requirement2.LastUpdateDate;

			//Now test that we can delete a step
			requirementManager.DeleteStep(projectId, requirementStepId4, USER_ID_FRED_BLOGGS);

			//Verify it was deleted
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4);
			Assert.IsNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4, true);
			Assert.IsNotNull(requirementStep);
			requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(3, requirementSteps.Count);
			count = requirementManager.CountSteps(requirementId1);
			Assert.AreEqual(3, count);

			//Verify that the last updated date of the requirement was changed
			requirement2 = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsTrue(requirement2.LastUpdateDate > previousLastUpdateDate);
			previousLastUpdateDate = requirement2.LastUpdateDate;

			//Now undelete the step (don't log rollback entry)
			requirementManager.UndeleteStep(projectId, requirementStepId4, USER_ID_FRED_BLOGGS, null);

			//Verify that it was restored
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4);
			Assert.IsNotNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4, true);
			Assert.IsNotNull(requirementStep);
			requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(4, requirementSteps.Count);
			count = requirementManager.CountSteps(requirementId1);
			Assert.AreEqual(4, count);

			//Verify that the last updated date of the requirement was changed
			requirement2 = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.IsTrue(requirement2.LastUpdateDate > previousLastUpdateDate);
			previousLastUpdateDate = requirement2.LastUpdateDate;

			//Now verify that history items were recorded for these operations
			HistoryManager historyManager = new HistoryManager();
			//First using the normal history retrieve that does *NOT* include steps for a requirement
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(requirementId1, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(1, historyChangeSets.Count);
			//Requirement Added
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.Requirement, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual(requirementId1, historyChangeSets[0].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);

			//Now get the history for a single requirement step
			historyChangeSets = historyManager.RetrieveByArtifactId(requirementStepId4, Artifact.ArtifactTypeEnum.RequirementStep);
			Assert.AreEqual(3, historyChangeSets.Count);
			//Step Added
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, historyChangeSets[2].ArtifactTypeId);
			Assert.AreEqual(requirementStepId4, historyChangeSets[2].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Deleted, historyChangeSets[2].ChangeTypeId);
			//Step Changed
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual(requirementStepId4, historyChangeSets[1].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);
			//Step Deleted
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual(requirementStepId4, historyChangeSets[0].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);

			//Verify the modify history changeset has the field level detail
			//TrackableCollection<HistoryDetail> detailRows = historyChangeSets[1].Details;
			//Assert.AreEqual(1, detailRows.Count);
			Assert.AreEqual("Forgot to do this", historyChangeSets[1].OldValue);
			Assert.AreEqual("Missed doing this", historyChangeSets[1].NewValue);

			//All step artifact modify history items are retrieved for the parent requirement when we use the special overload that includes steps
			List<HistoryChangeSetResponse> historyChanges = historyManager.RetrieveByArtifactId(projectId, requirementId1, Artifact.ArtifactTypeEnum.Requirement, "ChangeDate", true, null, 1, 999, InternalRoutines.UTC_OFFSET, true);
			Assert.AreEqual(7, historyChanges.Count);
			//Step Changed
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, historyChanges[4].ArtifactTypeId);
			Assert.AreEqual(requirementStepId4, historyChanges[4].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChanges[4].ChangeTypeId);

			//Verify the modify history changeset has the field level detail
			Assert.AreEqual("Forgot to do this", historyChanges[5].OldValue);
			Assert.AreEqual("Missed doing this", historyChanges[5].NewValue);

			//Now delete the requirement and verify that you cannot now retrieve a step of the requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId1);
			Assert.IsNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId1, true);
			Assert.IsNotNull(requirementStep);
			requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(0, requirementSteps.Count);
			requirementSteps = requirementManager.RetrieveSteps(requirementId1, true);
			Assert.AreEqual(4, requirementSteps.Count);

			//Now restore the requirement and verify that you can retrieve the steps again
			requirementManager.UnDelete(requirementId1, USER_ID_FRED_BLOGGS, 0, false);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId1);
			Assert.IsNotNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId1, true);
			Assert.IsNotNull(requirementStep);
			requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(4, requirementSteps.Count);
			requirementSteps = requirementManager.RetrieveSteps(requirementId1, true);
			Assert.AreEqual(4, requirementSteps.Count);

			//Now delete the step again
			requirementManager.DeleteStep(projectId, requirementStepId4, USER_ID_FRED_BLOGGS);

			//Verify it was deleted but can be retrieved with the includeDeleted=true option
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4);
			Assert.IsNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4, true);
			Assert.IsNotNull(requirementStep);

			//Now do a purge of the step and verify it's completely gone
			requirementManager.PurgeStep(projectId, requirementStepId4, USER_ID_FRED_BLOGGS);

			//Verify it was deleted but can be retrieved with the includeDeleted=true option
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4);
			Assert.IsNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId4, true);
			Assert.IsNull(requirementStep);

			//Also verify that the position numbers are correct.
			requirementSteps = requirementManager.RetrieveSteps(requirementId1);
			Assert.AreEqual(3, requirementSteps.Count);
			Assert.AreEqual("First do this", requirementSteps[0].Description);
			Assert.AreEqual(1, requirementSteps[0].Position);
			Assert.AreEqual("Then do this", requirementSteps[1].Description);
			Assert.AreEqual(2, requirementSteps[1].Position);
			Assert.AreEqual("Finally do this", requirementSteps[2].Description);
			Assert.AreEqual(3, requirementSteps[2].Position);

			//Now verify that copying the requirement also copies the steps
			int copiedRequirementId = requirementManager.Copy(USER_ID_FRED_BLOGGS, projectId, requirementId1, null);
			requirementSteps = requirementManager.RetrieveSteps(copiedRequirementId);
			Assert.AreEqual(3, requirementSteps.Count);

			//Finally clean up by deleting the requirements
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, copiedRequirementId);
		}

		/// <summary>
		/// Tests that we can associate requirements with components
		/// and then retrieve them by component
		/// </summary>
		[Test]
		[SpiraTestCase(1212)]
		public void _25_AssociateRequirementsWithComponents()
		{
			//First we need to add some components
			//Lets create three new components in the project, with two being active
			ComponentManager componentManager = new ComponentManager();
			int componentId1 = componentManager.Component_Insert(projectId, "Component 1");
			int componentId2 = componentManager.Component_Insert(projectId, "Component 2");
			int componentId3 = componentManager.Component_Insert(projectId, "Component 3", false);

			//Next, need to create a hierarchy of requirements, some of which are assigned to a component
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, componentId1, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 2", null, 60, USER_ID_FRED_BLOGGS);
			requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, componentId2, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 3", null, 120, USER_ID_FRED_BLOGGS);
			requirementId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, componentId3, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 4", null, 240, USER_ID_FRED_BLOGGS);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId4);

			//First get the list of requirements unfiltered, to verify
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(5, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[1].Name);
			Assert.AreEqual(true, requirements[1].IsSummary);
			Assert.AreEqual("Requirement 2", requirements[2].Name);
			Assert.AreEqual(false, requirements[2].IsSummary);
			Assert.AreEqual("Requirement 3", requirements[3].Name);
			Assert.AreEqual(false, requirements[3].IsSummary);
			Assert.AreEqual("Requirement 4", requirements[4].Name);
			Assert.AreEqual(false, requirements[4].IsSummary);

			//Now lets retrieve all the requirements associated with a specific component (includes parents)
			Hashtable filters = new Hashtable();
			filters.Add("ComponentId", componentId1);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual("Requirement 2", requirements[1].Name);

			//Now lets retrieve all the requirements associated with a selection of components
			filters.Clear();
			MultiValueFilter mvf = new MultiValueFilter();
			mvf.Values.Add(componentId1);
			mvf.Values.Add(componentId3);
			filters.Add("ComponentId", mvf);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual("Requirement 2", requirements[1].Name);
			Assert.AreEqual("Requirement 4", requirements[2].Name);

			//Next lets remove a component value from one requirement
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ComponentId = null;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Now lets verify that we can retrieve all the requirements with no component set
			filters.Clear();
			mvf = new MultiValueFilter();
			mvf.IsNone = true;
			filters.Add("ComponentId", mvf);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[1].Name);
			Assert.AreEqual("Requirement 3", requirements[2].Name);
			Assert.IsFalse(requirements[1].ComponentId.HasValue);

			//Now put a different value on that requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ComponentId = componentId1;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify the change
			filters.Clear();
			filters.Add("ComponentId", componentId1);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual("Requirement 2", requirements[1].Name);
			Assert.AreEqual(componentId1, requirements[1].ComponentId);
			Assert.AreEqual("Requirement 3", requirements[2].Name);
			Assert.AreEqual(componentId1, requirements[2].ComponentId);

			//Finally verify that history entries were correctly recorded
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(requirementId3, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(3, historyChangeSets.Count);

			//The entries are newest first
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			//HistoryDetail historyDetail = historyChangeSets[0].Details[0];
			//Assert.IsTrue(historyDetail.OldValue.IsNull());
			//Assert.AreEqual(componentId1, historyDetail.NewValueInt);
			//Assert.AreEqual("Component 1", historyDetail.NewValue);

			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);
			//historyDetail = historyChangeSets[1].Details[0];
			//Assert.IsTrue(historyDetail.NewValue.IsNull());
			//Assert.AreEqual(componentId2, historyDetail.OldValueInt);
			//Assert.AreEqual("Component 2", historyDetail.OldValue);

			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[2].ChangeTypeId);

			//Finally delete the requirement tree (components will be deleted during tear-down)
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
		}

		/// <summary>
		/// Tests that we can estimate in story points and use different conversion metrics into hours.
		/// Also that the system can suggest a metric based on current historical data
		/// </summary>
		[Test]
		[SpiraTestCase(1215)]
		public void _26_EstimateCalculations()
		{
			//Verify the current effort/point metric for the project
			ProjectManager projectManager = new ProjectManager();
			Project project = projectManager.RetrieveById(projectId);
			Assert.AreEqual(480, project.ReqPointEffort);

			//Get the default point estimate
			decimal defaultEstimate = project.ReqDefaultEstimate.Value;
			Assert.AreEqual(1.0M, defaultEstimate);

			//First lets create a test release (that we want the effort estimates to update)
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 0.1", "", "0.1.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1, 0, null);

			//First lets create some requirements with several different story point estimates.
			//The default hours/point metric for a new project will be used
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 2", null, 10, USER_ID_FRED_BLOGGS);
			requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 3", null, 20, USER_ID_FRED_BLOGGS);
			requirementId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 4", null, 15, USER_ID_FRED_BLOGGS);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId4);

			//Verify that the points estimate rolled-up to the parent and that the effort in hours was converted correctly
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			int minsPerPointMetric = 8 * 60;
			Assert.AreEqual(4, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[1].Name);
			Assert.AreEqual(45M, requirements[1].EstimatePoints);
			Assert.AreEqual(45 * minsPerPointMetric, requirements[1].EstimatedEffort);
			Assert.AreEqual("Requirement 2", requirements[2].Name);
			Assert.AreEqual(10M, requirements[2].EstimatePoints);
			Assert.AreEqual(10 * minsPerPointMetric, requirements[2].EstimatedEffort);
			Assert.AreEqual("Requirement 3", requirements[3].Name);
			Assert.AreEqual(20M, requirements[3].EstimatePoints);
			Assert.AreEqual(20 * minsPerPointMetric, requirements[3].EstimatedEffort);
			Assert.AreEqual("Requirement 4", requirements[4].Name);
			Assert.AreEqual(15M, requirements[4].EstimatePoints);
			Assert.AreEqual(15 * minsPerPointMetric, requirements[4].EstimatedEffort);

			//Verify the release effort includes these values
			ReleaseView releaseView = releaseManager.RetrieveById(UserManager.UserInternal, projectId, releaseId1);
			Assert.AreEqual(45 * minsPerPointMetric, releaseView.TaskEstimatedEffort);

			//Change a couple of the requirements' estimates
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.EstimatePoints = 25M;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.EstimatePoints = 15M;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify that the points estimate rolled-up to the parent and that the effort in hours was converted correctly
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Requirement 1", requirements[1].Name);
			Assert.AreEqual(55M, requirements[1].EstimatePoints);
			Assert.AreEqual(55 * minsPerPointMetric, requirements[1].EstimatedEffort);
			Assert.AreEqual("Requirement 2", requirements[2].Name);
			Assert.AreEqual(15M, requirements[2].EstimatePoints);
			Assert.AreEqual(15 * minsPerPointMetric, requirements[2].EstimatedEffort);
			Assert.AreEqual("Requirement 3", requirements[3].Name);
			Assert.AreEqual(25M, requirements[3].EstimatePoints);
			Assert.AreEqual(25 * minsPerPointMetric, requirements[3].EstimatedEffort);
			Assert.AreEqual("Requirement 4", requirements[4].Name);
			Assert.AreEqual(15M, requirements[4].EstimatePoints);
			Assert.AreEqual(15 * minsPerPointMetric, requirements[4].EstimatedEffort);

			//Verify the release value updated as well
			releaseView = releaseManager.RetrieveById(UserManager.UserInternal, projectId, releaseId1);
			Assert.AreEqual(55 * minsPerPointMetric, releaseView.TaskEstimatedEffort);

			//Now set one of the requirements to NULL estimate, verify the rollups handle this case correctly
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.EstimatePoints = null;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify that the points estimate rolled-up to the parent and that the effort in hours was converted correctly
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Requirement 1", requirements[1].Name);
			Assert.AreEqual(40M, requirements[1].EstimatePoints);
			Assert.AreEqual(40 * minsPerPointMetric, requirements[1].EstimatedEffort);
			Assert.AreEqual("Requirement 2", requirements[2].Name);
			Assert.AreEqual(15M, requirements[2].EstimatePoints);
			Assert.AreEqual(15 * minsPerPointMetric, requirements[2].EstimatedEffort);
			Assert.AreEqual("Requirement 3", requirements[3].Name);
			Assert.AreEqual(25M, requirements[3].EstimatePoints);
			Assert.AreEqual(25 * minsPerPointMetric, requirements[3].EstimatedEffort);
			Assert.AreEqual("Requirement 4", requirements[4].Name);
			Assert.IsNull(requirements[4].EstimatePoints);
			Assert.IsNull(requirements[4].EstimatedEffort);

			releaseView = releaseManager.RetrieveById(UserManager.UserInternal, projectId, releaseId1);
			Assert.AreEqual(40 * minsPerPointMetric, releaseView.TaskEstimatedEffort);

			//Before we start adding tasks, verify that we get NULL back if we try and get a new suggested minutes/point
			//metric before any tasks are added:
			Assert.IsFalse(requirementManager.SuggestNewPointEffortMetric(projectId).HasValue);

			//Now lets add some tasks to two of the requirements, each of which has a more detailed estimate
			TaskManager taskManager = new TaskManager();
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId2, null, null, null, "Task 1", null, null, null, 4000, null, null);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId2, null, null, null, "Task 2", null, null, null, 3000, null, null);
			int taskId3 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId3, null, null, null, "Task 3", null, null, null, 4000, null, null);
			int taskId4 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId3, null, null, null, "Task 3", null, null, null, 4500, null, null);
			int taskId5 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId3, null, null, null, "Task 3", null, null, null, 4500, null, null);

			//Now lets get the the recommended new hours/story metric based on these tasks
			int newPointEffort = requirementManager.SuggestNewPointEffortMetric(projectId).Value;

			//Verify this number
			int expectedMetric = 500;
			Assert.AreEqual(expectedMetric, newPointEffort);

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Update the metric
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.ReqPointEffort = newPointEffort;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Verify that it updated
			project = projectManager.RetrieveById(projectId);
			Assert.AreEqual(expectedMetric, project.ReqPointEffort);

			//Verify that the requirements are not yet updated
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Requirement 1", requirements[1].Name);
			Assert.AreEqual(40M, requirements[1].EstimatePoints);
			Assert.AreEqual(40 * minsPerPointMetric, requirements[1].EstimatedEffort);
			Assert.AreEqual("Requirement 2", requirements[2].Name);
			Assert.AreEqual(15M, requirements[2].EstimatePoints);
			Assert.AreEqual(15 * minsPerPointMetric, requirements[2].EstimatedEffort);
			Assert.AreEqual("Requirement 3", requirements[3].Name);
			Assert.AreEqual(25M, requirements[3].EstimatePoints);
			Assert.AreEqual(25 * minsPerPointMetric, requirements[3].EstimatedEffort);
			Assert.AreEqual("Requirement 4", requirements[4].Name);
			Assert.IsNull(requirements[4].EstimatePoints);
			Assert.IsNull(requirements[4].EstimatedEffort);

			//Do the bulk refresh of all existing requirements
			projectManager.RefreshTaskProgressCache(projectId);

			//Verify that the points estimate rolled-up to the parent and that the effort in hours was converted correctly
			minsPerPointMetric = newPointEffort;
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Requirement 1", requirements[1].Name);
			Assert.AreEqual(40M, requirements[1].EstimatePoints);
			Assert.AreEqual(40 * minsPerPointMetric, requirements[1].EstimatedEffort);
			Assert.AreEqual("Requirement 2", requirements[2].Name);
			Assert.AreEqual(15M, requirements[2].EstimatePoints);
			Assert.AreEqual(15 * minsPerPointMetric, requirements[2].EstimatedEffort);
			Assert.AreEqual("Requirement 3", requirements[3].Name);
			Assert.AreEqual(25M, requirements[3].EstimatePoints);
			Assert.AreEqual(25 * minsPerPointMetric, requirements[3].EstimatedEffort);
			Assert.AreEqual("Requirement 4", requirements[4].Name);
			Assert.IsNull(requirements[4].EstimatePoints);
			Assert.IsNull(requirements[4].EstimatedEffort);

			//Now lets complete some of the tasks which will give a new project effort for the tasks (need to first assign to a release)
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", "", "1.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1, 0, null);

			Task task = taskManager.RetrieveById(taskId1);
			task.StartTracking();
			task.ReleaseId = releaseId2;
			task.ActualEffort = 2000;
			task.RemainingEffort = 3000;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify the projected effort increased
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual(5000, task.ProjectedEffort);

			//Now lets get the the recommended new hours/story metric based on these tasks
			newPointEffort = requirementManager.SuggestNewPointEffortMetric(projectId).Value;

			//Verify this number
			expectedMetric = 525;
			Assert.AreEqual(expectedMetric, newPointEffort);

			//Update the metric
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.ReqPointEffort = newPointEffort;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Verify that it updated
			project = projectManager.RetrieveById(projectId);
			Assert.AreEqual(expectedMetric, project.ReqPointEffort);

			//Verify that the requirements are not yet updated
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual(40M, requirements[0].EstimatePoints);
			Assert.AreEqual(40 * minsPerPointMetric, requirements[0].EstimatedEffort);
			Assert.AreEqual("Requirement 2", requirements[1].Name);
			Assert.AreEqual(15M, requirements[1].EstimatePoints);
			Assert.AreEqual(15 * minsPerPointMetric, requirements[1].EstimatedEffort);
			Assert.AreEqual("Requirement 3", requirements[2].Name);
			Assert.AreEqual(25M, requirements[2].EstimatePoints);
			Assert.AreEqual(25 * minsPerPointMetric, requirements[2].EstimatedEffort);
			Assert.AreEqual("Requirement 4", requirements[3].Name);
			Assert.IsNull(requirements[3].EstimatePoints);
			Assert.IsNull(requirements[3].EstimatedEffort);

			//Do the bulk refresh of all existing requirements
			projectManager.RefreshTaskProgressCache(projectId);

			//Verify that the points estimate rolled-up to the parent and that the effort in hours was converted correctly
			minsPerPointMetric = newPointEffort;
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual(40M, requirements[0].EstimatePoints);
			Assert.AreEqual(40 * minsPerPointMetric, requirements[0].EstimatedEffort);
			Assert.AreEqual("Requirement 2", requirements[1].Name);
			Assert.AreEqual(15M, requirements[1].EstimatePoints);
			Assert.AreEqual(15 * minsPerPointMetric, requirements[1].EstimatedEffort);
			Assert.AreEqual("Requirement 3", requirements[2].Name);
			Assert.AreEqual(25M, requirements[2].EstimatePoints);
			Assert.AreEqual(25 * minsPerPointMetric, requirements[2].EstimatedEffort);
			Assert.AreEqual("Requirement 4", requirements[3].Name);
			Assert.IsNull(requirements[3].EstimatePoints);
			Assert.IsNull(requirements[3].EstimatedEffort);

			//Verify the conversion functions work as expected
			int estimatedEffort = requirementManager.GetEstimatedEffortFromEstimatePoints(projectId, 10.0M).Value;
			decimal points = requirementManager.GetEstimatePointsFromEffort(projectId, 900).Value;
			Assert.AreEqual(10.0M * minsPerPointMetric, estimatedEffort);
			Assert.AreEqual(900M / minsPerPointMetric, points);

			//Clean up by deleting the requirements and release and returning the project metric to its default value
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.ReqPointEffort = 480;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");
		}

		/// <summary>
		/// Tests that we can focus on a particular requirements branch. This means that the entire tree is collapsed,
		/// except for the selected branch. Normally you would want to clear all filters at the same time, but that would
		/// be done separately.
		/// </summary>
		[Test]
		[SpiraTestCase(1216)]
		public void _27_FocusOnBranch()
		{
			//First lets create some requirements in a hierarchy that will test the focus on functionality
			//-1
			//  -2
			//      -3
			//      -4
			//  -5
			//      -6
			//      -7
			//          -8
			//      -9
			//  -10
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 2", null, null, USER_ID_FRED_BLOGGS);
			requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 3", null, null, USER_ID_FRED_BLOGGS);
			requirementId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 4", null, null, USER_ID_FRED_BLOGGS);
			int requirementId5 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 5", null, null, USER_ID_FRED_BLOGGS);
			int requirementId6 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 6", null, null, USER_ID_FRED_BLOGGS);
			int requirementId7 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 7", null, null, USER_ID_FRED_BLOGGS);
			int requirementId8 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 8", null, null, USER_ID_FRED_BLOGGS);
			int requirementId9 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 9", null, null, USER_ID_FRED_BLOGGS);
			int requirementId10 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 10", null, null, USER_ID_FRED_BLOGGS);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId5);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId6);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId6);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId7);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId7);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId8);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId8);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId8);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId9);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId9);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId10);

			//Now lets expand the entire tree and verify all items are displayed
			requirementManager.ExpandToLevel(USER_ID_FRED_BLOGGS, projectId, null);
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, 999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, requirements.Count);

			//Now lets 'focus-on' a specific non-summary requirement. It should display its parents and peers only
			requirementManager.FocusOn(USER_ID_FRED_BLOGGS, requirementId3);

			//Verify the hierarchy
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, 999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(11, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[1].Name);
			Assert.AreEqual("Requirement 2", requirements[2].Name);
			Assert.AreEqual("Requirement 3", requirements[3].Name);
			Assert.AreEqual("Requirement 4", requirements[4].Name);
			Assert.AreEqual("Requirement 5", requirements[5].Name);
			//Assert.AreEqual("Requirement 10", requirements[5].Name);

			//Now lets 'focus-on' a specific non-summary requirement that has peers with children. It should display its parents and peers only
			requirementManager.FocusOn(USER_ID_FRED_BLOGGS, requirementId6);

			//Verify the hierarchy
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, 999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(7, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual("Requirement 2", requirements[1].Name);
			Assert.AreEqual("Requirement 5", requirements[2].Name);
			Assert.AreEqual("Requirement 6", requirements[3].Name);
			Assert.AreEqual("Requirement 7", requirements[4].Name);
			Assert.AreEqual("Requirement 9", requirements[5].Name);
			Assert.AreEqual("Requirement 10", requirements[6].Name);

			//Now lets 'focus-on' a specific summary requirement. It should display its parents and children only
			requirementManager.FocusOn(USER_ID_FRED_BLOGGS, requirementId5);

			//Verify the hierarchy
			requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, 999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);
			Assert.AreEqual("Requirement 2", requirements[1].Name);
			Assert.AreEqual("Requirement 5", requirements[2].Name);
			Assert.AreEqual("Requirement 6", requirements[3].Name);
			Assert.AreEqual("Requirement 7", requirements[4].Name);
			Assert.AreEqual("Requirement 8", requirements[5].Name);
			Assert.AreEqual("Requirement 9", requirements[6].Name);
			Assert.AreEqual("Requirement 10", requirements[7].Name);

			//Clean up by deleting the entire branch
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
		}

		/// <summary>
		/// Tests that you can plan with requirements in the new Planning Board
		/// </summary>
		[Test]
		[SpiraTestCase(1311)]
		public void _28_PlanningBoardTests()
		{
			//Get the requirement types
			List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
			int rq_typeUserStoryId = requirementTypes.FirstOrDefault(t => t.Name == "User Story").RequirementTypeId;

			//Get the importance values
			List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
			int rq_importanceCriticalId = importances.FirstOrDefault(i => i.Score == 1).ImportanceId;
			int rq_importanceHighId = importances.FirstOrDefault(i => i.Score == 2).ImportanceId;
			int rq_importanceMediumId = importances.FirstOrDefault(i => i.Score == 3).ImportanceId;
			int rq_importanceLowId = importances.FirstOrDefault(i => i.Score == 4).ImportanceId;

			//Get the list of active statuses (involved in at least one workflow)
			//check the different options for the method
			List<RequirementStatus> statuses = requirementManager.RetrieveStatusesInUse(projectTemplateId, true, true, true);
			Assert.AreEqual(10, statuses.Count);
			statuses = requirementManager.RetrieveStatusesInUse(projectTemplateId, true, true, false);
			Assert.AreEqual(9, statuses.Count);
			statuses = requirementManager.RetrieveStatusesInUse(projectTemplateId, true, false, true);
			Assert.AreEqual(4, statuses.Count);
			statuses = requirementManager.RetrieveStatusesInUse(projectTemplateId, false, true, true);
			Assert.AreEqual(6, statuses.Count);
			statuses = requirementManager.RetrieveStatusesInUse(projectTemplateId, false, true, false);
			Assert.AreEqual(5, statuses.Count);
			statuses = requirementManager.RetrieveStatusesInUse(projectTemplateId, false, false, false);
			Assert.AreEqual(0, statuses.Count);
			//now get the default - all set to true
			statuses = requirementManager.RetrieveStatusesInUse(projectTemplateId);
			Assert.AreEqual(10, statuses.Count);

			//First lets create some components
			ComponentManager componentManager = new ComponentManager();
			int componentId1 = componentManager.Component_Insert(projectId, "Component 1");
			int componentId2 = componentManager.Component_Insert(projectId, "Component 2");

			//Next lets create some requirements in a simple 1-level hierarchy
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 2", null, 1.0M, USER_ID_FRED_BLOGGS);
			requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 3", null, 1.0M, USER_ID_FRED_BLOGGS);
			requirementId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 4", null, 1.5M, USER_ID_FRED_BLOGGS);
			int requirementId5 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 5", null, 2.0M, USER_ID_FRED_BLOGGS);
			int requirementId6 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 6", null, 0.5M, USER_ID_FRED_BLOGGS);
			int requirementId7 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 7", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId8 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 8", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId9 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 9", null, 2.5M, USER_ID_FRED_BLOGGS);
			int requirementId10 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 10", null, 1.0M, USER_ID_FRED_BLOGGS);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId5);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId6);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId7);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId8);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId9);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId10);

			//Verify that we can create a new 'in-memory' requirement
			RequirementView requirement = requirementManager.Requirement_New(projectId, USER_ID_FRED_BLOGGS, rq_typeUserStoryId);
			Assert.AreEqual(projectId, requirement.ProjectId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, requirement.AuthorId);
			Assert.AreEqual(rq_typeUserStoryId, requirement.RequirementTypeId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Requested, requirement.RequirementStatusId);
			Assert.IsTrue(String.IsNullOrEmpty(requirement.Name));

			/* Backlog View */

			//Get product backlog by component
			List<RequirementView> requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, null);
			Assert.AreEqual(9, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, componentId2);
			Assert.AreEqual(0, requirements.Count);

			//Get the list of packages
			requirements = requirementManager.Requirement_RetrieveSummaryBacklog(projectId);
			Assert.AreEqual(1, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);

			//Get product backlog by package
			requirements = requirementManager.Requirement_RetrieveBacklogByPackageRequirementId(projectId, null);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByPackageRequirementId(projectId, requirementId1);
			Assert.AreEqual(9, requirements.Count);

			//Get product backlog by status
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, null, null);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, null, (int)Requirement.RequirementStatusEnum.Requested);
			Assert.AreEqual(9, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, null, (int)Requirement.RequirementStatusEnum.Accepted);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, null, (int)Requirement.RequirementStatusEnum.Rejected);
			Assert.AreEqual(0, requirements.Count);

			//Get product backlog by importance
			requirements = requirementManager.Requirement_RetrieveBacklogByImportanceId(projectId, null);
			Assert.AreEqual(9, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByImportanceId(projectId, rq_importanceCriticalId);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByImportanceId(projectId, rq_importanceHighId);
			Assert.AreEqual(0, requirements.Count);

			//Now lets update the backlog
			//Requirement 2
			requirement = requirementManager.RetrieveById2(projectId, requirementId2);
			Requirement requirement2 = requirement.ConvertTo<RequirementView, Requirement>();
			requirement2.StartTracking();
			requirement2.ComponentId = componentId1;
			requirement2.ImportanceId = rq_importanceHighId;
			requirement2.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Accepted;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement2 });
			//Requirement 3
			requirement = requirementManager.RetrieveById2(projectId, requirementId3);
			requirement2 = requirement.ConvertTo<RequirementView, Requirement>();
			requirement2.StartTracking();
			requirement2.ComponentId = componentId2;
			requirement2.ImportanceId = rq_importanceMediumId;
			requirement2.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Rejected;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement2 });
			//Requirement 4
			requirement = requirementManager.RetrieveById2(projectId, requirementId4);
			requirement2 = requirement.ConvertTo<RequirementView, Requirement>();
			requirement2.StartTracking();
			requirement2.ComponentId = componentId1;
			requirement2.ImportanceId = rq_importanceCriticalId;
			requirement2.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Accepted;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement2 });

			//Move this last requirement under the root package
			requirementManager.Requirement_UpdateBacklogPackageRequirementId(projectId, new List<int>() { requirementId5 }, null, USER_ID_FRED_BLOGGS);

			//Get product backlog by component
			requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, null);
			Assert.AreEqual(6, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, componentId2);
			Assert.AreEqual(0, requirements.Count); //Rejected are ignored

			//Get product backlog by package
			requirements = requirementManager.Requirement_RetrieveBacklogByPackageRequirementId(projectId, null);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByPackageRequirementId(projectId, requirementId1);
			Assert.AreEqual(7, requirements.Count); //Rejected are ignored

			//Get product backlog by status
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, null, (int)Requirement.RequirementStatusEnum.Requested);
			Assert.AreEqual(6, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, null, (int)Requirement.RequirementStatusEnum.Accepted);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, null, (int)Requirement.RequirementStatusEnum.Rejected);
			Assert.AreEqual(1, requirements.Count); //Rejected are included in this view

			//Get product backlog by importance
			requirements = requirementManager.Requirement_RetrieveBacklogByImportanceId(projectId, null);
			Assert.AreEqual(6, requirements.Count); //Rejected are ignored
			requirements = requirementManager.Requirement_RetrieveBacklogByImportanceId(projectId, rq_importanceCriticalId);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByImportanceId(projectId, rq_importanceHighId);
			Assert.AreEqual(1, requirements.Count);

			//Check the ordering of the requirements by component
			requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(requirementId4, requirements[0].RequirementId);
			Assert.AreEqual(rq_importanceCriticalId, requirements[0].ImportanceId);
			Assert.AreEqual(requirementId2, requirements[1].RequirementId);
			Assert.AreEqual(rq_importanceHighId, requirements[1].ImportanceId);

			//Re-rank them (requirements grooming)
			requirementManager.Requirement_UpdateRanks(projectId, new List<int>() { requirementId2 }, null);

			//Verify
			requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(requirementId2, requirements[0].RequirementId);
			Assert.AreEqual(rq_importanceHighId, requirements[0].ImportanceId);
			Assert.AreEqual(requirementId4, requirements[1].RequirementId);
			Assert.AreEqual(rq_importanceCriticalId, requirements[1].ImportanceId);

			//Re-rank them again
			int existingRank = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId2).Rank.Value;
			requirementManager.Requirement_UpdateRanks(projectId, new List<int>() { requirementId4 }, existingRank);

			//Verify
			requirements = requirementManager.Requirement_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(requirementId4, requirements[0].RequirementId);
			Assert.AreEqual(rq_importanceCriticalId, requirements[0].ImportanceId);
			Assert.AreEqual(requirementId2, requirements[1].RequirementId);
			Assert.AreEqual(rq_importanceHighId, requirements[1].ImportanceId);

			//Now lets add some test cases and tasks to one of the requirements
			TaskManager taskManager = new TaskManager();
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId2, null, null, null, "Task 1", null, null, null, 4000, null, null);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId2, null, null, null, "Task 2", null, null, null, 3000, null, null);
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			testCaseManager.AddToRequirement(projectId, requirementId2, new List<int>() { testCaseId1, testCaseId2 }, 1);

			/* All Release View */

			//Lets create two releases, with the first one having two iterations
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(2), 2, 0, null, false);
			int iterationId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 001", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			int iterationId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 002", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now.AddMonths(1), DateTime.Now.AddMonths(2), 2, 0, null, false);
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0", null, "2.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now.AddMonths(2), DateTime.Now.AddMonths(4), 2, 0, null, false);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Now lets verify that no releases have requirements
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(8, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, releaseId1, true);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, releaseId2, true);
			Assert.AreEqual(0, requirements.Count);

			//Assign some requirements to one of the releases
			requirementManager.AssociateToIteration(new List<int>() { requirementId2, requirementId4 }, releaseId1, USER_ID_FRED_BLOGGS);
			requirementManager.AssociateToIteration(new List<int>() { requirementId5 }, releaseId2, USER_ID_FRED_BLOGGS);

			//Verify the changes
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(5, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, releaseId1, true);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, releaseId2, true);
			Assert.AreEqual(1, requirements.Count);

			//Check task and test cases and now assigned to the release as well
			Task task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual(releaseId1, task.ReleaseId);
			task = taskManager.RetrieveById(taskId2);
			Assert.AreEqual(releaseId1, task.ReleaseId);

			List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId1);
			Assert.AreEqual(2, mappedTestCases.Count);
			Assert.AreEqual(testCaseId1, mappedTestCases[0].TestCaseId);
			Assert.AreEqual(testCaseId2, mappedTestCases[1].TestCaseId);

			//Verify that we can view all the releases by status
			requirements = requirementManager.Requirement_RetrieveAllReleasesByStatusId(projectId, (int)Requirement.RequirementStatusEnum.Planned);
			Assert.AreEqual(3, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByStatusId(projectId, (int)Requirement.RequirementStatusEnum.InProgress);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByStatusId(projectId, (int)Requirement.RequirementStatusEnum.Tested);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByStatusId(projectId, (int)Requirement.RequirementStatusEnum.Completed);
			Assert.AreEqual(0, requirements.Count);

			//Verify that we can view all the releases by person
			requirements = requirementManager.Requirement_RetrieveAllReleasesByUserId(projectId, null);
			Assert.AreEqual(3, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByUserId(projectId, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByUserId(projectId, USER_ID_JOE_SMITH);
			Assert.AreEqual(0, requirements.Count);

			//Verify that we can view all the releases by priority
			requirements = requirementManager.Requirement_RetrieveAllReleasesByImportanceId(projectId, null);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByImportanceId(projectId, rq_importanceCriticalId);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByImportanceId(projectId, rq_importanceHighId);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByImportanceId(projectId, rq_importanceMediumId);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByImportanceId(projectId, rq_importanceLowId);
			Assert.AreEqual(0, requirements.Count);

			//Verify that we can view all the releases by component
			requirements = requirementManager.Requirement_RetrieveAllReleasesByComponentId(projectId, null);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByComponentId(projectId, componentId1);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByComponentId(projectId, componentId2);
			Assert.AreEqual(0, requirements.Count);

			//Verify that we can view all the releases by package/epic
			requirements = requirementManager.Requirement_RetrieveAllReleasesByPackageRequirementId(projectId, null);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByPackageRequirementId(projectId, requirementId1);
			Assert.AreEqual(2, requirements.Count);

			/* Specific Release View */

			//Get release backlog by iteration
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(5, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, releaseId1, false);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, iterationId1, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, iterationId2, false);
			Assert.AreEqual(0, requirements.Count);

			//Get release backlog by status
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId1, null);
			Assert.AreEqual(5, requirements.Count); //The product backlog
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Requirement.RequirementStatusEnum.Planned);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Requirement.RequirementStatusEnum.InProgress);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Requirement.RequirementStatusEnum.Developed);
			Assert.AreEqual(0, requirements.Count);

			//Get release backlog by person
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(5, requirements.Count); //The product backlog
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(0, requirements.Count);

			//Get release backlog by priority
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, null, releaseId1);
			Assert.AreEqual(0, requirements.Count); //The requirements not assigned an importance
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, rq_importanceCriticalId, releaseId1);
			Assert.AreEqual(1, requirements.Count); //The requirements not assigned an importance
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, rq_importanceHighId, releaseId1);
			Assert.AreEqual(1, requirements.Count); //The requirements not assigned an importance
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, rq_importanceMediumId, releaseId1);
			Assert.AreEqual(0, requirements.Count); //The requirements not assigned an importance
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, rq_importanceLowId, releaseId1);
			Assert.AreEqual(0, requirements.Count); //The requirements not assigned an importance

			//Now we need to assign the requirements to a particular iteration and user.
			//Also move one task to 'In-Progress' that will switch the requirement to In-Progress
			requirementManager.AssociateToIteration(new List<int>() { requirementId2 }, iterationId1, USER_ID_FRED_BLOGGS);
			requirementManager.AssociateToIteration(new List<int>() { requirementId4 }, iterationId2, USER_ID_FRED_BLOGGS);
			requirementManager.AssignToUser(requirementId2, USER_ID_JOE_SMITH, USER_ID_FRED_BLOGGS);

			task = taskManager.RetrieveById(taskId1);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Get release backlog by iteration
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(5, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, releaseId1, false);
			Assert.AreEqual(0, requirements.Count); //Release only
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, releaseId1, true);
			Assert.AreEqual(2, requirements.Count); //Release and child iterations
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, iterationId1, false);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByReleaseId(projectId, iterationId2, false);
			Assert.AreEqual(1, requirements.Count);

			//Get release backlog by status
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId1, null);
			Assert.AreEqual(5, requirements.Count); //The product backlog
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Requirement.RequirementStatusEnum.Planned);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Requirement.RequirementStatusEnum.InProgress);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Requirement.RequirementStatusEnum.Developed);
			Assert.AreEqual(0, requirements.Count);

			//Get release backlog by person
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(5, requirements.Count); //The product backlog
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(1, requirements.Count);

			//Verify that we can view a single release by component
			requirements = requirementManager.Requirement_RetrieveForReleaseByComponentId(projectId, null, releaseId1);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveForReleaseByComponentId(projectId, componentId1, releaseId1);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveForReleaseByComponentId(projectId, componentId2, releaseId1);
			Assert.AreEqual(0, requirements.Count);

			//Verify that we can view a single release by package/epic
			requirements = requirementManager.Requirement_RetrieveForReleaseByPackageRequirementId(projectId, null, releaseId1);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveForReleaseByPackageRequirementId(projectId, requirementId1, releaseId1);
			Assert.AreEqual(2, requirements.Count);


			/* Specific Iteration View */

			//Get iteration backlog by status
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, iterationId1, null);
			Assert.AreEqual(5, requirements.Count); //The product backlog
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Requirement.RequirementStatusEnum.Planned);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Requirement.RequirementStatusEnum.InProgress);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Requirement.RequirementStatusEnum.Developed);
			Assert.AreEqual(0, requirements.Count);

			//Get iteration backlog by person
			//Iteration #1
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(5, requirements.Count); //The product backlog
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId1, null, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(1, requirements.Count);
			//Iteration #2
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(5, requirements.Count); //The product backlog
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId2, null, false);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId2, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId2, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(0, requirements.Count);

			//Get iteration backlog by priority
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, null, iterationId2);
			Assert.AreEqual(0, requirements.Count); //The requirements not assigned an importance
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, rq_importanceCriticalId, iterationId2);
			Assert.AreEqual(1, requirements.Count); //The requirements not assigned an importance
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, rq_importanceHighId, iterationId2);
			Assert.AreEqual(0, requirements.Count); //The requirements not assigned an importance
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, rq_importanceMediumId, iterationId2);
			Assert.AreEqual(0, requirements.Count); //The requirements not assigned an importance
			requirements = requirementManager.Requirement_RetrieveForReleaseByImportanceId(projectId, rq_importanceLowId, iterationId2);
			Assert.AreEqual(0, requirements.Count); //The requirements not assigned an importance

			//Assign the other requirement to Fred
			requirementManager.AssignToUser(requirementId4, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS);

			//Get iteration backlog by person
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(5, requirements.Count); //The product backlog
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId2, null, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId2, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveBacklogByUserId(projectId, iterationId2, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(0, requirements.Count);

			//Finally, verify that we can view all the releases by person
			requirements = requirementManager.Requirement_RetrieveAllReleasesByUserId(projectId, null);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByUserId(projectId, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveAllReleasesByUserId(projectId, USER_ID_JOE_SMITH);
			Assert.AreEqual(1, requirements.Count);

			//Verify that we can view a single iteration by component
			requirements = requirementManager.Requirement_RetrieveForReleaseByComponentId(projectId, null, iterationId2);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveForReleaseByComponentId(projectId, componentId1, iterationId2);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveForReleaseByComponentId(projectId, componentId2, iterationId2);
			Assert.AreEqual(0, requirements.Count);

			//Verify that we can view a single iteration by package/epic
			requirements = requirementManager.Requirement_RetrieveForReleaseByPackageRequirementId(projectId, null, iterationId2);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveForReleaseByPackageRequirementId(projectId, requirementId1, iterationId2);
			Assert.AreEqual(1, requirements.Count);

			//Clean up by deleting the entire branch, test cases, tasks and components
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId5);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId2);
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
			componentManager.Component_Delete(componentId1);
			componentManager.Component_Delete(componentId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
		}


		/// <summary>
		/// Tests that you can add/remove/view requirement-test step associations/coverage
		/// </summary>
		[Test]
		[SpiraTestCase(1516)]
		public void _29_RequirementTestStepCoverage()
		{
			//First lets create some requirements in a simple 1-level hierarchy
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			int requirementId2 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, null, null, requirementId1, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 2", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 3", null, 1.0M, USER_ID_FRED_BLOGGS);

			//Now lets create a test case with a test step
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testStepId1 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, 1, "Step 1", "Result 1", null);

			//Now lets verify no existing coverage
			List<RequirementView> mappedRequirements = requirementManager.RequirementTestStep_RetrieveByTestStepId(USER_ID_FRED_BLOGGS, projectId, testStepId1);
			Assert.AreEqual(0, mappedRequirements.Count);
			List<TestStep> mappedTestSteps = requirementManager.RequirementTestStep_RetrieveByRequirementId(projectId, requirementId1);
			Assert.AreEqual(0, mappedTestSteps.Count);

			//Also verify that the test cases and not mapped to the test case either
			mappedRequirements = requirementManager.RetrieveCoveredByTestCaseId(User.UserInternal, null, testCaseId1);
			Assert.AreEqual(0, mappedRequirements.Count);

			//Now lets add some requirements to the test step
			requirementManager.RequirementTestStep_AddToTestStep(projectId, 1, testStepId1, new List<int> { requirementId1, requirementId3 }, true);

			//Verify all three were added
			mappedRequirements = requirementManager.RequirementTestStep_RetrieveByTestStepId(USER_ID_FRED_BLOGGS, projectId, testStepId1);
			Assert.AreEqual(3, mappedRequirements.Count);
			Assert.IsTrue(mappedRequirements.Any(r => r.RequirementId == requirementId1));
			Assert.IsTrue(mappedRequirements.Any(r => r.RequirementId == requirementId2));
			Assert.IsTrue(mappedRequirements.Any(r => r.RequirementId == requirementId3));

			//Verify that the test case mapping was changed
			mappedRequirements = requirementManager.RetrieveCoveredByTestCaseId(User.UserInternal, null, testCaseId1);
			Assert.AreEqual(3, mappedRequirements.Count);

			mappedTestSteps = requirementManager.RequirementTestStep_RetrieveByRequirementId(projectId, requirementId1);
			Assert.AreEqual(1, mappedTestSteps.Count);
			mappedTestSteps = requirementManager.RequirementTestStep_RetrieveByRequirementId(projectId, requirementId3);
			Assert.AreEqual(1, mappedTestSteps.Count);

			//Remove one of the items
			requirementManager.RequirementTestStep_RemoveFromTestStep(projectId, testStepId1, new List<int> { requirementId3 });

			//Verify that one was removed
			mappedRequirements = requirementManager.RequirementTestStep_RetrieveByTestStepId(USER_ID_FRED_BLOGGS, projectId, testStepId1);
			Assert.AreEqual(2, mappedRequirements.Count);
			Assert.IsTrue(mappedRequirements.Any(r => r.RequirementId == requirementId1));
			Assert.IsTrue(mappedRequirements.Any(r => r.RequirementId == requirementId2));

			//Remove the others
			requirementManager.RequirementTestStep_RemoveFromTestStep(projectId, testStepId1, new List<int> { requirementId1 });
			requirementManager.RequirementTestStep_RemoveFromTestStep(projectId, testStepId1, new List<int> { requirementId2 });

			//Verify all were removed
			mappedRequirements = requirementManager.RequirementTestStep_RetrieveByTestStepId(USER_ID_FRED_BLOGGS, projectId, testStepId1);
			Assert.AreEqual(0, mappedRequirements.Count);

			//Verify that the test case coverage remains (we don't auto-remove since it's not a sufficient reason)
			mappedRequirements = requirementManager.RetrieveCoveredByTestCaseId(User.UserInternal, null, testCaseId1);
			Assert.AreEqual(3, mappedRequirements.Count);

			//Verify that we can add a summary item only (not its children)
			requirementManager.RequirementTestStep_AddToTestStep(projectId, 1, testStepId1, new List<int> { requirementId1 }, false);
			mappedRequirements = requirementManager.RequirementTestStep_RetrieveByTestStepId(USER_ID_FRED_BLOGGS, projectId, testStepId1);
			Assert.AreEqual(1, mappedRequirements.Count);
			Assert.IsTrue(mappedRequirements.Any(r => r.RequirementId == requirementId1));

			/* --- Clean Up --- */

			//Delete the requirements
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId3);

			//Delete the test case with step
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
		}

		/// <summary>
		/// Tests that you can plan with requirements in the Project Group Planning Board
		/// </summary>
		[Test]
		[SpiraTestCase(1539)]
		public void _30_ProjectGroupPlanningBoardTests()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			string adminSectionName1 = "View / Edit Programs";
			var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId1 = adminSection1.ADMIN_SECTION_ID;
			//Get the importance values
			List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
			int rq_importanceCriticalId = importances.FirstOrDefault(i => i.Score == 1).ImportanceId;
			int rq_importanceHighId = importances.FirstOrDefault(i => i.Score == 2).ImportanceId;
			int rq_importanceMediumId = importances.FirstOrDefault(i => i.Score == 3).ImportanceId;
			int rq_importanceLowId = importances.FirstOrDefault(i => i.Score == 4).ImportanceId;

			//We need to create two projects to plan with, both in the same group
			ProjectGroupManager projectGroupManager = new ProjectGroupManager();
			int projectGroupId = projectGroupManager.Insert("RequirementManagerTest Group", null, null, true, false, 1, 1, 1, adminSectionId1, "Inserted Program");

			ProjectManager projectManager = new ProjectManager();
			int projectId1 = projectManager.Insert("RequirementManagerTest Project 1", projectGroupId, null, null, true, null, 1, adminSectionId, "Inserted Project");
			int projectId2 = projectManager.Insert("RequirementManagerTest Project 2", projectGroupId, null, null, true, null, 1, adminSectionId, "Inserted Project");

			TemplateManager templateManager = new TemplateManager();
			int templateId1 = templateManager.RetrieveForProject(projectId1).ProjectTemplateId;
			int templateId2 = templateManager.RetrieveForProject(projectId2).ProjectTemplateId;

			//Add a release to each project
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId1, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(2), 2, 0, null, false);
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(2), 2, 0, null, false);

			//Add some requirements to the backlog
			int reqId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId1, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, 2.0M, USER_ID_FRED_BLOGGS);
			int reqId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId1, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, rq_importanceHighId, "Requirement 2", null, 1.0M, USER_ID_FRED_BLOGGS);
			int reqId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId2, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, rq_importanceHighId, "Requirement 3", null, 1.0M, USER_ID_FRED_BLOGGS);
			int reqId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId2, null, null, (int?)null, Requirement.RequirementStatusEnum.Rejected, null, USER_ID_FRED_BLOGGS, null, rq_importanceMediumId, "Requirement 4", null, 1.5M, USER_ID_FRED_BLOGGS);

			//Backlog by Importance
			List<RequirementView> requirements;
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, null, true);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, rq_importanceCriticalId, true);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, rq_importanceHighId, true);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, rq_importanceMediumId, true);
			Assert.AreEqual(0, requirements.Count); //Not 1, because we exclude Rejected items in this view
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, rq_importanceLowId, true);
			Assert.AreEqual(0, requirements.Count);

			//Backlog by Status
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Requested, null, true);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Accepted, null, true);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Rejected, null, true);
			Assert.AreEqual(1, requirements.Count);

			//Now we need to look at what's planned (vs. the backlog). To do that we need to move the items
			//to Planned status and assign to a user. We need to make the Rejected one Accepted first
			RequirementView requirementView = requirementManager.RetrieveById2(projectId2, reqId4);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Accepted;
			requirementManager.Update(USER_ID_SYS_ADMIN, projectId2, new List<Requirement> { requirement });
			requirementManager.AssociateToIteration(new List<int>() { reqId1, reqId2 }, releaseId1, USER_ID_SYS_ADMIN);
			requirementManager.AssociateToIteration(new List<int>() { reqId3, reqId4 }, releaseId2, USER_ID_SYS_ADMIN);
			requirementManager.AssignToUser(reqId1, USER_ID_FRED_BLOGGS, USER_ID_SYS_ADMIN);
			requirementManager.AssignToUser(reqId2, USER_ID_JOE_SMITH, USER_ID_SYS_ADMIN);

			//All-Projects view, by specific project
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByProjectId(projectGroupId, projectId1);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByProjectId(projectGroupId, projectId2);
			Assert.AreEqual(2, requirements.Count);

			//All-Projects view, by person
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, null, null);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, null, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, null, USER_ID_JOE_SMITH);
			Assert.AreEqual(1, requirements.Count);

			//All-Projects view, by status
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Planned, null, false);
			Assert.AreEqual(4, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.InProgress, null, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Developed, null, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Tested, null, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Completed, null, false);
			Assert.AreEqual(0, requirements.Count);

			//All-Projects view, by importance
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, null, false);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, rq_importanceCriticalId, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, rq_importanceHighId, false);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, rq_importanceMediumId, false);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByImportanceId(projectGroupId, rq_importanceLowId, false);
			Assert.AreEqual(0, requirements.Count);

			//Project View By User
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, projectId1, null);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, projectId1, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, projectId1, USER_ID_JOE_SMITH);
			Assert.AreEqual(1, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, projectId2, null);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, projectId2, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByUserId(projectGroupId, projectId2, USER_ID_JOE_SMITH);
			Assert.AreEqual(0, requirements.Count);

			//Project View by Status
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Planned, projectId1, false);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Developed, projectId1, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Tested, projectId1, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Planned, projectId2, false);
			Assert.AreEqual(2, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Developed, projectId2, false);
			Assert.AreEqual(0, requirements.Count);
			requirements = requirementManager.Requirement_RetrieveGroupBacklogByStatusId(projectGroupId, (int)Requirement.RequirementStatusEnum.Tested, projectId2, false);
			Assert.AreEqual(0, requirements.Count);

			//Delete the two projects
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId1);
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId2);

			//Delete the group
			projectGroupManager.Delete(projectGroupId, 1);

			//Delete the templates
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId1);
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId2);
		}

		/// <summary>
		/// Tests that you can have test cases in one project convert coverage to requirements in another
		/// </summary>
		[Test]
		[SpiraTestCase(1540)]
		public void _31_CrossProjectTestCoverage()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//We need to create two projects to work with
			ProjectManager projectManager = new ProjectManager();
			int projectId1 = projectManager.Insert("RequirementManagerTest Project 1", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			int projectId2 = projectManager.Insert("RequirementManagerTest Project 2", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the templates associated with the projects
			TemplateManager templateManager = new TemplateManager();
			int projectTemplateId1 = templateManager.RetrieveForProject(projectId1).ProjectTemplateId;
			int projectTemplateId2 = templateManager.RetrieveForProject(projectId2).ProjectTemplateId;

			//Make sure that project 2 shares test cases with project 1
			projectManager.ProjectAssociation_Add(projectId2, projectId1, new List<int>() { (int)Artifact.ArtifactTypeEnum.TestCase });

			//Add a requirement to project 1
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId1, null, null, (int?)null, Requirement.RequirementStatusEnum.Developed, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, 2.0M, USER_ID_FRED_BLOGGS);

			//Verify it's coverage
			RequirementView requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId1, requirementId);
			Assert.AreEqual(0, requirement.CoverageCountTotal);
			Assert.AreEqual(0, requirement.CoverageCountPassed);

			//Add two test cases to project 2
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null, true, true);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null, true, true);

			//Add to the requirement
			testCaseManager.AddToRequirement(projectId1, requirementId, new List<int>() { testCaseId1, testCaseId2 }, 1);

			//Verify test coverage
			requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId1, requirementId);
			Assert.AreEqual(2, requirement.CoverageCountTotal);
			Assert.AreEqual(0, requirement.CoverageCountPassed);
			Assert.AreEqual(0, requirement.CoverageCountFailed);

			//Run the test case and pass it
			TestRunManager testRunManager = new TestRunManager();
			TestRunsPending pendingRuns = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId2, null, new List<int>() { testCaseId1 }, true);
			pendingRuns.TestRuns[0].StartTracking();
			pendingRuns.TestRuns[0].TestRunSteps[0].StartTracking();
			pendingRuns.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunManager.UpdateExecutionStatus(projectId2, USER_ID_FRED_BLOGGS, pendingRuns, 0, DateTime.UtcNow, true);
			testRunManager.Save(pendingRuns, projectId2, false);
			testRunManager.CompletePending(pendingRuns.TestRunsPendingId, USER_ID_FRED_BLOGGS);

			//Verify the status
			TestCaseView testCase = testCaseManager.RetrieveById(projectId2, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.ExecutionStatusId);

			//Verify the test coverage
			requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId1, requirementId);
			Assert.AreEqual(2, requirement.CoverageCountTotal);
			Assert.AreEqual(1, requirement.CoverageCountPassed);
			Assert.AreEqual(0, requirement.CoverageCountFailed);

			//Now execute the other test case using the Record method
			testRunManager.Record(projectId2, USER_ID_FRED_BLOGGS, testCaseId2, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), (int)TestCase.ExecutionStatusEnum.Failed, null, null, null, null, null, null, null, null, TestRun.TestRunFormatEnum.PlainText, null);

			//Verify the test coverage
			requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId1, requirementId);
			Assert.AreEqual(2, requirement.CoverageCountTotal);
			Assert.AreEqual(1, requirement.CoverageCountPassed);
			Assert.AreEqual(1, requirement.CoverageCountFailed);

			//Remove from the requirement
			testCaseManager.RemoveFromRequirement(projectId1, requirementId, new List<int>() { testCaseId1 }, 1);

			//Verify the test coverage
			requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId1, requirementId);
			Assert.AreEqual(1, requirement.CoverageCountTotal);
			Assert.AreEqual(0, requirement.CoverageCountPassed);
			Assert.AreEqual(1, requirement.CoverageCountFailed);

			//Add to the requirement
			testCaseManager.AddToRequirement(projectId1, requirementId, new List<int>() { testCaseId1 }, 1);

			//Verify the test coverage
			requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId1, requirementId);
			Assert.AreEqual(2, requirement.CoverageCountTotal);
			Assert.AreEqual(1, requirement.CoverageCountPassed);
			Assert.AreEqual(1, requirement.CoverageCountFailed);

			//CLEAN UP
			//Delete the two projects (make sure that the sharing one can delete first)
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId2);
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId1);

			//Delete the templates
			templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateId2);
			templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateId1);

			//Delete the test cases
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId2);
		}

		/// <summary>
		/// Tests that you can split an existing requirement
		/// </summary>
		[Test]
		[SpiraTestCase(1649)]
		public void _32_SplitRequirement()
		{
			int rq_importanceHighId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 2).ImportanceId;
			int rq_typeFeatureId = requirementManager.RequirementType_Retrieve(projectTemplateId, false).FirstOrDefault(t => t.Name == "Feature").RequirementTypeId;

			//First create a new requirement
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, rq_importanceHighId, "Requirement 1", null, 4.0M, USER_ID_FRED_BLOGGS);

			//Lets split off 1.0 story points;
			int requirementId2 = requirementManager.Split(projectId, requirementId1, "Requirement 2", USER_ID_FRED_BLOGGS, 1.0M, USER_ID_JOE_SMITH, "Giving work to Joe");

			//Verify the old and new requirement
			RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId1);
			Assert.AreEqual(3.0M, requirement.EstimatePoints);
			Assert.AreEqual("Requirement 1", requirement.Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirement.RequirementStatusId);
			Assert.AreEqual(rq_typeFeatureId, requirement.RequirementTypeId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, requirement.OwnerId);
			requirement = requirementManager.RetrieveById2(projectId, requirementId2);
			Assert.AreEqual(1.0M, requirement.EstimatePoints);
			Assert.AreEqual("Requirement 2", requirement.Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirement.RequirementStatusId);
			Assert.AreEqual(rq_typeFeatureId, requirement.RequirementTypeId);
			Assert.AreEqual(USER_ID_JOE_SMITH, requirement.OwnerId);

			//Lets split off just the completed portion, so lets make it 33% done first
			new TaskManager().Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.Completed, null, null, requirementId1, null, null, null, "Task 1", null, DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddDays(40), 30, 30, 0);
			new TaskManager().Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId1, null, null, null, "Task 2", null, DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddDays(40), 60, 0, 60);

			requirement = requirementManager.RetrieveById2(projectId, requirementId1);
			Assert.AreEqual(50, requirement.TaskPercentOnTime);
			int requirementId3 = requirementManager.Split(projectId, requirementId1, "Requirement 3", USER_ID_FRED_BLOGGS, null, USER_ID_JOE_SMITH, "Giving work to Joe");

			//Verify the old and new requirement
			requirement = requirementManager.RetrieveById2(projectId, requirementId1);
			Assert.AreEqual(1.5M, requirement.EstimatePoints);
			Assert.AreEqual("Requirement 1", requirement.Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirement.RequirementStatusId);
			Assert.AreEqual(rq_typeFeatureId, requirement.RequirementTypeId);
			requirement = requirementManager.RetrieveById2(projectId, requirementId3);
			Assert.AreEqual(1.5M, requirement.EstimatePoints);
			Assert.AreEqual("Requirement 3", requirement.Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirement.RequirementStatusId);
			Assert.AreEqual(rq_typeFeatureId, requirement.RequirementTypeId);
		}

		/// <summary>
		/// Tests that you can make a requirement obsolete even if it has a task (which normally controls its status)
		/// </summary>
		[Test]
		[SpiraTestCase(1670)]
		public void _33_ObsoleteRequirement()
		{
			int rq_importanceHighId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 2).ImportanceId;

			//First create a new requirement
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, rq_importanceHighId, "Requirement 1", null, 4.0M, USER_ID_FRED_BLOGGS);

			//Next assign a completed task to it
			int taskId = new TaskManager().Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.Completed, null, null, requirementId, null, null, null, "Task 1", null, DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddDays(40), 30, 30, 0);

			//Verify that the requirement is now 'Developed'
			RequirementView requirementView = requirementManager.RetrieveById2(projectId, requirementId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirementView.RequirementStatusId);

			//Verify that if we move it to obsolete, it stays in that status (and the task doesn't switch it back)
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.LastUpdateDate = DateTime.UtcNow;
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Obsolete;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify that the requirement is now 'Obsolete'
			requirementView = requirementManager.RetrieveById2(projectId, requirementId);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Obsolete, requirementView.RequirementStatusId);

			//Delete the requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId);
		}

		/// <summary>
		/// Tests that you can create, edit, delete requirement types
		/// </summary>
		[Test]
		[SpiraTestCase(1732)]
		public void _34_EditTypes()
		{
			//First lets get the list of types in the current template
			List<RequirementType> types = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(6, types.Count);

			//Next lets add a new type
			//Active, no steps, not default
			int requirementTypeId1 = requirementManager.RequirementType_Insert(
				projectTemplateId,
				"Epic",
				null,
				false,
				true,
				false
				);

			//Verify that it was created
			RequirementType requirementType = requirementManager.RequirementType_RetrieveById(requirementTypeId1);
			Assert.IsNotNull(requirementType);
			Assert.AreEqual("Epic", requirementType.Name);
			Assert.AreEqual(true, requirementType.IsActive);
			Assert.AreEqual(false, requirementType.IsDefault);
			Assert.AreEqual(false, requirementType.IsSteps);

			//Next lets add a new type
			//Active, steps, not default
			int requirementTypeId2 = requirementManager.RequirementType_Insert(
				projectTemplateId,
				"Scenario",
				null,
				false,
				true,
				true
				);

			//Verify that it was created
			requirementType = requirementManager.RequirementType_RetrieveById(requirementTypeId2);
			Assert.IsNotNull(requirementType);
			Assert.AreEqual("Scenario", requirementType.Name);
			Assert.AreEqual(true, requirementType.IsActive);
			Assert.AreEqual(false, requirementType.IsDefault);
			Assert.AreEqual(true, requirementType.IsSteps);

			//Make changes
			requirementType.StartTracking();
			requirementType.Name = "Scenario Steps";
			requirementManager.RequirementType_Update(requirementType);

			//Verify the changes
			//Verify that it was created
			requirementType = requirementManager.RequirementType_RetrieveById(requirementTypeId2);
			Assert.IsNotNull(requirementType);
			Assert.AreEqual("Scenario Steps", requirementType.Name);
			Assert.AreEqual(true, requirementType.IsActive);
			Assert.AreEqual(false, requirementType.IsDefault);
			Assert.AreEqual(true, requirementType.IsSteps);


			//Verify that we can get the total count of types
			types = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(8, types.Count);

			//Verify that we can get the total count of types that have steps
			types = requirementManager.RequirementType_RetrieveUseCases(projectTemplateId);
			Assert.AreEqual(2, types.Count);

			//Verify that we can get the default type
			requirementType = requirementManager.RequirementType_RetrieveDefault(projectTemplateId);
			Assert.IsNotNull(requirementType);
			Assert.AreEqual("Feature", requirementType.Name);
			Assert.AreEqual(true, requirementType.IsActive);
			Assert.AreEqual(true, requirementType.IsDefault);
			Assert.AreEqual(false, requirementType.IsSteps);

			//Delete our new types (internal function only, not possible in the UI)
			requirementManager.RequirementType_Delete(requirementTypeId1);
			requirementManager.RequirementType_Delete(requirementTypeId2);

			//Verify the count
			types = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(6, types.Count);
		}

		/// <summary>
		/// Tests that you can create, edit, delete requirement importances
		/// </summary>
		[Test]
		[SpiraTestCase(1733)]
		public void _35_EditImportances()
		{
			//First lets get the list of importances in the current template
			List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
			Assert.AreEqual(4, importances.Count);

			//Next lets add a new importance
			int importanceId1 = requirementManager.RequirementImportance_Insert(
				projectTemplateId,
				"5 - Minor",
				"eeeeee",
				true,
				5
				);

			//Verify that it was created
			Importance importance = requirementManager.RequirementImportance_RetrieveById(importanceId1);
			Assert.IsNotNull(importance);
			Assert.AreEqual("5 - Minor", importance.Name);
			Assert.AreEqual(true, importance.IsActive);
			Assert.AreEqual("eeeeee", importance.Color);
			Assert.AreEqual(5, importance.Score);

			//Make changes
			importance.StartTracking();
			importance.Name = "5 - Cosmetic";
			importance.Color = "dddddd";
			importance.Score = 6;
			requirementManager.RequirementImportance_Update(importance);

			//Verify the changes
			importance = requirementManager.RequirementImportance_RetrieveById(importanceId1);
			Assert.IsNotNull(importance);
			Assert.AreEqual("5 - Cosmetic", importance.Name);
			Assert.AreEqual(true, importance.IsActive);
			Assert.AreEqual("dddddd", importance.Color);
			Assert.AreEqual(6, importance.Score);

			//Verify that we can get the total count of importances
			importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
			Assert.AreEqual(5, importances.Count);

			//Delete our new importances (internal function only, not possible in the UI)
			requirementManager.RequirementImportance_Delete(importanceId1);

			//Verify the count
			importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
			Assert.AreEqual(4, importances.Count);
		}

		/// <summary>
		/// Additional tests that are used to retrieve data used in the requirements mind map and sortable view
		/// </summary>
		/// <remarks>
		/// We use the sample project for this
		/// </remarks>
		[Test]
		[SpiraTestCase(2043)]
		public void _36_RequirementsMindMapAndSortedListRetrieves()
		{
			//First test the retrieval code for the sorted grid
			List<RequirementView> requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, null, InternalRoutines.UTC_OFFSET, "Name", true);
			int count = requirementManager.Requirement_CountForSorted(PROJECT_ID, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, requirements.Count);
			Assert.AreEqual(35, count);

			//Now filter and sort by something else (simple fields)
			Hashtable filters = new Hashtable();
			filters.Add("ImportanceId", 1);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "RequirementStatusName", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(12, requirements.Count);
			Assert.AreEqual(12, count);

			//Filter by Coverage='Not Covered'
			filters.Clear();
			filters.Add("CoverageId", 1);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, requirements.Count);
			Assert.AreEqual(18, count);

			//Filter by Coverage='0% Run'
			filters.Clear();
			filters.Add("CoverageId", 2);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(0, count);

			//Filter by Coverage='<= 50% Run'
			filters.Clear();
			filters.Add("CoverageId", 3);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, requirements.Count);
			Assert.AreEqual(1, count);

			//Filter by Coverage='< 100% Run'
			filters.Clear();
			filters.Add("CoverageId", 4);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, requirements.Count);
			Assert.AreEqual(6, count);

			//Filter by Coverage='> 0% Failed'
			filters.Clear();
			filters.Add("CoverageId", 5);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, requirements.Count, "CoverageId1");
			Assert.AreEqual(8, count);

			//Filter by Coverage='>= 50% Failed'
			filters.Clear();
			filters.Add("CoverageId", 6);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, requirements.Count, "CoverageId2");
			Assert.AreEqual(4, count);

			//Filter by Coverage='= 100% Failed'
			filters.Clear();
			filters.Add("CoverageId", 7);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, requirements.Count);
			Assert.AreEqual(3, count);

			//Filter by Coverage='> 0% Caution'
			filters.Clear();
			filters.Add("CoverageId", 8);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(9, requirements.Count, "CoverageId3");
			Assert.AreEqual(9, count);

			//Filter by Coverage='>= 50% Caution'
			filters.Clear();
			filters.Add("CoverageId", 9);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, requirements.Count, "CoverageId4");
			Assert.AreEqual(3, count);

			//Filter by Coverage='= 100% Caution'
			filters.Clear();
			filters.Add("CoverageId", 10);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(0, count);

			//Filter by Coverage='> 0% Blocked'
			filters.Clear();
			filters.Add("CoverageId", 11);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(0, count);

			//Filter by Coverage='>= 50% Blocked'
			filters.Clear();
			filters.Add("CoverageId", 12);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(0, count);

			//Filter by Coverage='= 100% Blocked'
			filters.Clear();
			filters.Add("CoverageId", 13);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(0, count);

			//Filter by Task Progress='Not Started'
			filters.Clear();
			filters.Add("ProgressId", 1);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, requirements.Count);
			Assert.AreEqual(6, count);

			//Filter by Task Progress='Starting Late'
			filters.Clear();
			filters.Add("ProgressId", 2);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, requirements.Count, "ProgressId");
			Assert.AreEqual(6, count);

			//Filter by Task Progress='on Schedule'
			filters.Clear();
			filters.Add("ProgressId", 3);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(0, count);

			//Filter by Task Progress='Running Late'
			filters.Clear();
			filters.Add("ProgressId", 4);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, requirements.Count);
			Assert.AreEqual(0, count);

			//Filter by Task Progress='Completed'
			filters.Clear();
			filters.Add("ProgressId", 5);
			requirements = requirementManager.Requirement_RetrieveSorted(PROJECT_ID, 1, 15, filters, InternalRoutines.UTC_OFFSET, "Name", true);
			count = requirementManager.Requirement_CountForSorted(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(12, requirements.Count, "ProgressId");
			Assert.AreEqual(12, count);

			//Now verify that we can retrieve data for the various diagrams
			//Requirements Mind Map
			//All levels
			requirements = requirementManager.Requirement_RetrieveForMindMap(PROJECT_ID, null, out count, 1, Int32.MaxValue);
			Assert.AreEqual(35, requirements.Count);
			//Pagination
			requirements = requirementManager.Requirement_RetrieveForMindMap(PROJECT_ID, null, out count, 1, 7);
			Assert.AreEqual(7, requirements.Count);
			//For a parent release (includes the parent)
			requirements = requirementManager.Requirement_RetrieveForMindMap(PROJECT_ID, null, out count, 1, Int32.MaxValue, 1);
			Assert.AreEqual(27, requirements.Count);
			Assert.AreEqual(1, requirements[0].RequirementId);
			requirements = requirementManager.Requirement_RetrieveForMindMap(PROJECT_ID, null, out count, 1, Int32.MaxValue, 11);
			Assert.AreEqual(2, requirements.Count, "searching for requirement 11 and its children should return 2 requirements");
			Assert.AreEqual(11, requirements[0].RequirementId);
			Assert.AreEqual(12, requirements[1].RequirementId);
			//Levels 1 & 2
			requirements = requirementManager.Requirement_RetrieveForMindMap(PROJECT_ID, 2, out count, 1, Int32.MaxValue);
			Assert.AreEqual(10, requirements.Count);
			//Associations
			List<ArtifactLink> requirementAsscociations = requirementManager.ArtifactLink_RetrieveAllForRequirements(PROJECT_ID, out count, 1, Int32.MaxValue);
			Assert.AreEqual(4, requirementAsscociations.Count);

			//Verify we can retrieve the first summary requirement in a product
			Requirement requirement = requirementManager.Requirement_RetrieveFirstSummary(PROJECT_ID);
			Assert.AreEqual(1, requirement.RequirementId, "Requirement ID does not match");
			requirement = requirementManager.Requirement_RetrieveFirstSummary(PROJECT_ID_2);
			Assert.AreEqual(null, requirement, "There is no summary reqiurement in this project");
		}
	}
}
