using System;
using System.Linq;
using System.Collections;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using NUnit.Framework;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the ArtifactLink business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class ArtifactLinkTest
	{
		protected static Business.ArtifactLinkManager artifactLinkManager;
		protected static Business.RequirementManager requirementManager;
		protected static Business.IncidentManager incidentManager;
		protected static Business.TestCaseManager testCaseManager;
		protected static TaskManager taskManager;

		protected static int artifactLinkId1;
		protected static int artifactLinkId2;
		protected static int artifactLinkId3;
		protected static int artifactLinkId4;
		protected static int artifactLinkId5;
		protected static int requirementId1;
		protected static int requirementId2;
		protected static int incidentId1;
		protected static int incidentId2;
		protected static int testCaseId;
		protected static int testStepId;
		protected static int taskId1;
		protected static int taskId2;

		private static int projectId;
		private static int projectTemplateId;

		private const int PROJECT_ID = 1;
		private const int PROJECT_EMPTY_ID = 3;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		[TestFixtureSetUp]
		public void Init()
		{
			artifactLinkManager = new Business.ArtifactLinkManager();
			requirementManager = new Business.RequirementManager();
			incidentManager = new IncidentManager();
			testCaseManager = new Business.TestCaseManager();
			taskManager = new TaskManager();

			//Create new projects for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("ArtifactLinkTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");
			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Purge any deleted items in the projects
			Business.HistoryManager history = new Business.HistoryManager();
			history.PurgeAllDeleted(PROJECT_ID, USER_ID_FRED_BLOGGS);

			//Delete the temporary projects and templates
			new ProjectManager().Delete(USER_ID_FRED_BLOGGS, projectId);
			new TemplateManager().Delete(USER_ID_FRED_BLOGGS, projectTemplateId);
		}

		[
		Test,
		SpiraTestCase(322)
		]
		public void _01_RetrieveArtifactLinks()
		{
			//First verify that we can get the list of artifact links for an existing requirement
			List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, 4, "ArtifactStatusName", false);
			Assert.AreEqual(4, artifactLinks.Count);
			//Row 0
			Assert.AreEqual("Cannot install system on Oracle 9i", artifactLinks[0].ArtifactName);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Incident, artifactLinks[0].ArtifactTypeId);
			Assert.AreEqual(5, artifactLinks[0].ArtifactId);
			Assert.AreEqual("This bug affects the requirement", artifactLinks[0].Comment);
			Assert.AreEqual("Related-to", artifactLinks[0].ArtifactLinkTypeName);
			Assert.AreEqual("Open", artifactLinks[0].ArtifactStatusName);
			//Row 1
			Assert.AreEqual("Ability to delete existing books in the system", artifactLinks[1].ArtifactName);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactLinks[1].ArtifactTypeId);
			Assert.AreEqual(6, artifactLinks[1].ArtifactId);
			Assert.AreEqual("These two requirements are related", artifactLinks[1].Comment);
			Assert.AreEqual("Related-to", artifactLinks[1].ArtifactLinkTypeName);
			Assert.AreEqual("Completed", artifactLinks[1].ArtifactStatusName);
			//Row 2
			Assert.AreEqual("Creating a new book in the system", artifactLinks[2].ArtifactName);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactLinks[2].ArtifactTypeId);
			Assert.AreEqual(30, artifactLinks[2].ArtifactId);
			Assert.AreEqual("This use case defines the steps for creating a book", artifactLinks[2].Comment);
			Assert.AreEqual("Related-to", artifactLinks[2].ArtifactLinkTypeName);
			Assert.AreEqual("Completed", artifactLinks[2].ArtifactStatusName);
			//Row 3
			Assert.AreEqual("Cannot add a new book to the system", artifactLinks[3].ArtifactName);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Incident, artifactLinks[3].ArtifactTypeId);
			Assert.AreEqual(7, artifactLinks[3].ArtifactId);
			Assert.AreEqual("Test Run: Ability to create new book", artifactLinks[3].Comment);
			Assert.AreEqual("Implicit", artifactLinks[3].ArtifactLinkTypeName);
			Assert.AreEqual("Assigned", artifactLinks[3].ArtifactStatusName);

			//First verify that we can get the list of artifact links for an existing incident
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, 7, "ArtifactName", true);
			Assert.AreEqual(6, artifactLinks.Count);
			//Indirectly linked requirement
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactLinks[0].ArtifactTypeId);
			Assert.AreEqual(4, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Ability to add new books to the system", artifactLinks[0].ArtifactName);
			Assert.AreEqual("Test Run: Ability to create new book", artifactLinks[0].Comment);
			Assert.AreEqual("Implicit", artifactLinks[0].ArtifactLinkTypeName);
			Assert.AreEqual("Completed", artifactLinks[0].ArtifactStatusName);
			//Directly linked requirement
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactLinks[1].ArtifactTypeId);
			Assert.AreEqual(9, artifactLinks[1].ArtifactId);
			Assert.AreEqual("Ability to associate books with different editions", artifactLinks[1].ArtifactName);
			Assert.IsNull(artifactLinks[1].Comment);
			Assert.AreEqual("Related-to", artifactLinks[1].ArtifactLinkTypeName);
			Assert.AreEqual("Completed", artifactLinks[1].ArtifactStatusName);
			//Indirectly linked test run
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.TestRun, artifactLinks[2].ArtifactTypeId);
			Assert.AreEqual(9, artifactLinks[2].ArtifactId);
			Assert.AreEqual("Ability to create new book", artifactLinks[2].ArtifactName);
			Assert.AreEqual("Test Run: Ability to create new book", artifactLinks[2].Comment);
			Assert.AreEqual("Implicit", artifactLinks[2].ArtifactLinkTypeName);
			Assert.AreEqual("Failed", artifactLinks[2].ArtifactStatusName);
			//Directly linked test step
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.TestStep, artifactLinks[3].ArtifactTypeId);
			Assert.AreEqual(2, artifactLinks[3].ArtifactId);
			Assert.AreEqual("Ability to create new book (Step 2)", artifactLinks[3].ArtifactName);
			Assert.AreEqual("This incident is related to the test step", artifactLinks[3].Comment);
			Assert.AreEqual("Related-to", artifactLinks[3].ArtifactLinkTypeName);
			Assert.AreEqual("Passed", artifactLinks[3].ArtifactStatusName);
			//Indirectly linked requirement
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactLinks[4].ArtifactTypeId);
			Assert.AreEqual(30, artifactLinks[4].ArtifactId);
			Assert.AreEqual("Creating a new book in the system", artifactLinks[4].ArtifactName);
			Assert.AreEqual("Test Run: Ability to create new book", artifactLinks[4].Comment);
			Assert.AreEqual("Implicit", artifactLinks[4].ArtifactLinkTypeName);
			Assert.AreEqual("Completed", artifactLinks[4].ArtifactStatusName);
			//Directly linked incident
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Incident, artifactLinks[5].ArtifactTypeId);
			Assert.AreEqual(6, artifactLinks[5].ArtifactId);
			Assert.AreEqual("The book listing screen doesn't sort", artifactLinks[5].ArtifactName);
			Assert.IsNull(artifactLinks[5].Comment);
			Assert.AreEqual("Related-to", artifactLinks[5].ArtifactLinkTypeName);
			Assert.AreEqual("Open", artifactLinks[5].ArtifactStatusName);

			//First verify that we can get the list of artifact links for an existing test step
			//This tests that we can retrieve a direct link
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.TestStep, 2);
			Assert.AreEqual(1, artifactLinks.Count);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Incident, artifactLinks[0].ArtifactTypeId);
			Assert.AreEqual(7, artifactLinks[0].ArtifactId);
			Assert.AreEqual("This incident is related to the test step", artifactLinks[0].Comment);
			//This tests that we can retrieve an indirect link
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.TestStep, 3);
			Assert.AreEqual(1, artifactLinks.Count);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Incident, artifactLinks[0].ArtifactTypeId);
			Assert.AreEqual(7, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Test Run: Ability to create new book", artifactLinks[0].Comment);

			//Test that we can get the list of links between and incident and a task
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, 11);
			Assert.AreEqual(1, artifactLinks.Count);
			//Directly linked task
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Task, artifactLinks[0].ArtifactTypeId);
			Assert.AreEqual(14, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Refactor book details screen to include author drop-down", artifactLinks[0].ArtifactName);
			Assert.AreEqual("Need to refactor the screen before fixing the bug", artifactLinks[0].Comment);
			Assert.AreEqual("Related-to", artifactLinks[0].ArtifactLinkTypeName);
			Assert.AreEqual("Completed", artifactLinks[0].ArtifactStatusName);

			//Test that we can get the list of links between two tasks
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Task, 14);
			Assert.AreEqual(2, artifactLinks.Count);
			//Directly linked incident
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Incident, artifactLinks[0].ArtifactTypeId);
			Assert.AreEqual(11, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Validation on the edit book page", artifactLinks[0].ArtifactName);
			Assert.AreEqual("Need to refactor the screen before fixing the bug", artifactLinks[0].Comment);
			Assert.AreEqual("Related-to", artifactLinks[0].ArtifactLinkTypeName);
			Assert.AreEqual("Resolved", artifactLinks[0].ArtifactStatusName);
			//Directly linked task
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Task, artifactLinks[1].ArtifactTypeId);
			Assert.AreEqual(4, artifactLinks[1].ArtifactId);
			Assert.AreEqual("Develop edit book details screen", artifactLinks[1].ArtifactName);
			Assert.AreEqual("Need to create the screen before refactoring", artifactLinks[1].Comment);
			Assert.AreEqual("Depends-on", artifactLinks[1].ArtifactLinkTypeName);
			Assert.AreEqual("Completed", artifactLinks[1].ArtifactStatusName);

			//Test that the dependency name is reversed correctly for the dependent task
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Task, 4);
			Assert.AreEqual(1, artifactLinks.Count);
			//Directly linked task
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Task, artifactLinks[0].ArtifactTypeId);
			Assert.AreEqual(14, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Refactor book details screen to include author drop-down", artifactLinks[0].ArtifactName);
			Assert.AreEqual("Need to create the screen before refactoring", artifactLinks[0].Comment);
			Assert.AreEqual("Prerequisite-for", artifactLinks[0].ArtifactLinkTypeName);
			Assert.AreEqual("Completed", artifactLinks[0].ArtifactStatusName);

			//Now test that we can filter and sort the list of artifact links
			//Filter by Type, sort by Name ascending
			Hashtable filters = new Hashtable();
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add((int)DataModel.Artifact.ArtifactTypeEnum.Requirement);
			filters.Add("ArtifactTypeId", multiValueFilter);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, 7, "ArtifactName", true, filters);
			//Verify data
			Assert.AreEqual(3, artifactLinks.Count);
			Assert.AreEqual("Ability to add new books to the system", (string)artifactLinks[0].ArtifactName);
			Assert.AreEqual("Ability to associate books with different editions", (string)artifactLinks[1].ArtifactName);
			Assert.AreEqual("Creating a new book in the system", (string)artifactLinks[2].ArtifactName);

			//Filter by creator, sort by artifact id descending
			filters.Clear();
			multiValueFilter.Clear();
			multiValueFilter.Values.Add(USER_ID_JOE_SMITH);
			filters.Add("CreatorId", multiValueFilter);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, 7, "ArtifactId", false, filters);
			//Verify data
			Assert.AreEqual(3, artifactLinks.Count);
			Assert.AreEqual("Ability to create new book", (string)artifactLinks[0].ArtifactName);
			Assert.AreEqual("Ability to associate books with different editions", (string)artifactLinks[1].ArtifactName);
			Assert.AreEqual("The book listing screen doesn't sort", (string)artifactLinks[2].ArtifactName);
		}

		[
		Test,
		SpiraTestCase(323)
		]
		public void _02_CreateArtifactLinks()
		{
			//First lets create two new requirements, a test step, two tasks and incidents that we can use to link between
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, 120, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 2", null, 180, USER_ID_FRED_BLOGGS);
			incidentId1 = incidentManager.Insert(PROJECT_ID, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 1", "Incident 1 Description", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			incidentId2 = incidentManager.Insert(PROJECT_ID, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 1", "Incident 1 Description", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, USER_ID_FRED_BLOGGS, null, "Test Case For Linking", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			testStepId = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Test Step For Linking", null, null);
			taskId1 = taskManager.Insert(PROJECT_ID, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 1", "Task 1 Description", null, null, null, null, null, null);
			taskId2 = taskManager.Insert(PROJECT_ID, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 2", "Task 2 Description", null, null, null, null, null, null);

			//Now lets create links between the tasks, incidents, test steps and requirements
			artifactLinkId1 = artifactLinkManager.Insert(PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId1, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId2, USER_ID_FRED_BLOGGS, "Requirement linked to requirement", DateTime.UtcNow, ArtifactLink.ArtifactLinkTypeEnum.RelatedTo);
			artifactLinkId2 = artifactLinkManager.Insert(PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId1, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, USER_ID_FRED_BLOGGS, "Requirement linked to incident", DateTime.UtcNow, ArtifactLink.ArtifactLinkTypeEnum.RelatedTo);
			artifactLinkId3 = artifactLinkManager.Insert(PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId2, USER_ID_FRED_BLOGGS, null, DateTime.UtcNow, ArtifactLink.ArtifactLinkTypeEnum.RelatedTo);
			artifactLinkId4 = artifactLinkManager.Insert(PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.TestStep, testStepId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, USER_ID_FRED_BLOGGS, "Test Step Incident link", DateTime.UtcNow, ArtifactLink.ArtifactLinkTypeEnum.RelatedTo);
			artifactLinkId5 = artifactLinkManager.Insert(PROJECT_ID, Artifact.ArtifactTypeEnum.Task, taskId1, Artifact.ArtifactTypeEnum.Task, taskId2, USER_ID_FRED_BLOGGS, "Task 2 depends on Task 1", DateTime.UtcNow, ArtifactLink.ArtifactLinkTypeEnum.DependentOn);

			//Verify that they created successfully
			List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId1);
			Assert.AreEqual(2, artifactLinks.Count);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId2);
			Assert.AreEqual(1, artifactLinks.Count);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1);
			Assert.AreEqual(3, artifactLinks.Count);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId2);
			Assert.AreEqual(1, artifactLinks.Count);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.TestStep, testStepId);
			Assert.AreEqual(1, artifactLinks.Count);
			//Make sure that the task type handles the reverse direction OK when a directional type
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Task, taskId1);
			Assert.AreEqual(1, artifactLinks.Count);
			Assert.AreEqual("Depends-on", artifactLinks[0].ArtifactLinkTypeName);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Task, taskId2);
			Assert.AreEqual(1, artifactLinks.Count);
			Assert.AreEqual("Prerequisite-for", artifactLinks[0].ArtifactLinkTypeName);

			//Verify that creating a self-referential link throws an exception
			bool exceptionThrown = false;
			try
			{
				artifactLinkManager.Insert(PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId1, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId1, USER_ID_FRED_BLOGGS, null, DateTime.Now);
			}
			catch (ArtifactLinkSelfReferentialException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Shouldn't allow creation of self-referential links");

			//Verify that you can create links to/from summary requirements (change in v3.0)
			artifactLinkManager.Insert(PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId1, DataModel.Artifact.ArtifactTypeEnum.Requirement, 1, USER_ID_FRED_BLOGGS, null, DateTime.Now);
			//Verify that it was created successfully
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId1);
			Assert.AreEqual(3, artifactLinks.Count);

			//Verify that creating a link to a non-existant incident throws an exception
			exceptionThrown = false;
			try
			{
				artifactLinkManager.Insert(PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, DataModel.Artifact.ArtifactTypeEnum.Incident, -2, USER_ID_FRED_BLOGGS, null, DateTime.Now);
			}
			catch (ArtifactLinkDestNotFoundException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Shouldn't allow creation of links to non-existant incidents");

			//Verify that creating a duplicate link throws an exception
			exceptionThrown = false;
			try
			{
				artifactLinkManager.Insert(PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId2, USER_ID_FRED_BLOGGS, null, DateTime.Now);
			}
			catch (ArtifactLinkDuplicateException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Shouldn't allow creation of duplicate links");
		}

		/// <summary>
		/// Tests that we can modify the comments/type of an existing artifact link and also that we can retrieve
		/// a single link by it's id
		/// </summary>
		[
		Test,
		SpiraTestCase(577)
		]
		public void _03_ModifyArtifactLinks()
		{
			//Retrieve an artifact link by its ID
			ArtifactLink artifactLink = artifactLinkManager.RetrieveById(artifactLinkId2);
			Assert.AreEqual("Requirement linked to incident", artifactLink.Comment);
			Assert.AreEqual((int)ArtifactLink.ArtifactLinkTypeEnum.RelatedTo, artifactLink.ArtifactLinkTypeId);

			//Modify the comment and and update
			artifactLink.StartTracking();
			artifactLink.Comment = "Testing 123";
			artifactLink.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.DependentOn;
			artifactLinkManager.Update(artifactLink, USER_ID_FRED_BLOGGS, projectId);
			artifactLink = artifactLinkManager.RetrieveById(artifactLinkId2);
			Assert.AreEqual("Testing 123", artifactLink.Comment);
			Assert.AreEqual((int)ArtifactLink.ArtifactLinkTypeEnum.DependentOn, artifactLink.ArtifactLinkTypeId);

			//Verify that it can handle NULLs
			artifactLink.StartTracking();
			artifactLink.Comment = null;
			artifactLinkManager.Update(artifactLink, USER_ID_FRED_BLOGGS, projectId);
			artifactLink = artifactLinkManager.RetrieveById(artifactLinkId2);
			Assert.IsNull(artifactLink.Comment);
		}

		[
		Test,
		SpiraTestCase(324)
		]
		public void _04_DeleteArtifactLinks()
		{
			//Verify that we can delete a single link
			artifactLinkManager.Delete(artifactLinkId2, DataModel.Artifact.ArtifactTypeEnum.ArtifactLink, projectId, USER_ID_FRED_BLOGGS);
			List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId1);
			Assert.AreEqual(2, artifactLinks.Count);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1);
			Assert.AreEqual(2, artifactLinks.Count);

			//Finally test that we can delete the artifacts and have the artifact links automatically delete
			//Delete a requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId1);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId2);
			Assert.AreEqual(0, artifactLinks.Count);
			//Delete a requirement
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId2);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId2);
			Assert.AreEqual(1, artifactLinks.Count);
			//Delete a test case (and associated step)
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1);
			Assert.AreEqual(2, artifactLinks.Count);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, testCaseId);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1);
			Assert.AreEqual(1, artifactLinks.Count);
			//Delete an incident
			incidentManager.MarkAsDeleted(PROJECT_ID, incidentId1, USER_ID_FRED_BLOGGS);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId2);
			Assert.AreEqual(0, artifactLinks.Count);
			//Delete an incident
			incidentManager.MarkAsDeleted(PROJECT_ID, incidentId2, USER_ID_FRED_BLOGGS);

			//Delete the tasks
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(PROJECT_ID, taskId2, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that you can retrieve the tasks for a test run
		/// </summary>
		[
		Test,
		SpiraTestCase(1609)
		]
		public void _05_AssociateTasksToTestCaseAndTestRun()
		{
			//First we have to create a new test case
			TestCaseManager testCaseManager = new TestCaseManager();
			int exploratoryTestTypeId = testCaseManager.TestCaseType_Retrieve(projectTemplateId).FirstOrDefault(t => t.IsExploratory).TestCaseTypeId;
			int criticalPriorityId = testCaseManager.TestCasePriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 1).TestCasePriorityId;
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Ability to create new book", "Description of ability to create new book", exploratoryTestTypeId, TestCase.TestCaseStatusEnum.ReadyForTest, criticalPriorityId, null, 20, null, null);
			int testStepId = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Do something", "It works OK", null);

			//Then we execute it to create a test run
			TestRunManager testRunManager = new TestRunManager();
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, null, new List<int>() { testCaseId }, true);
			testRunManager.Save(testRunsPending, projectId, false);

			int pendingId = testRunsPending.TestRunsPendingId;
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			testRunsPending.TestRuns[0].TestRunSteps[0].ActualResult = "Needs work.";
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, null, true, false, true);

			//Now we create the tasks and link to the test run and test case
			TaskManager taskManager = new TaskManager();
			int testRunId = testRunsPending.TestRuns[0].TestRunId;
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 1", null, null, null, 4000, null, null);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 2", null, null, null, 4000, null, null);
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Task, taskId1, Artifact.ArtifactTypeEnum.TestRun, testRunId, USER_ID_FRED_BLOGGS, null, DateTime.UtcNow);
			artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Task, taskId2, Artifact.ArtifactTypeEnum.TestRun, testRunId, USER_ID_FRED_BLOGGS, null, DateTime.UtcNow);
			artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Task, taskId1, Artifact.ArtifactTypeEnum.TestCase, testCaseId, USER_ID_FRED_BLOGGS, null, DateTime.UtcNow);
			artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Task, taskId2, Artifact.ArtifactTypeEnum.TestCase, testCaseId, USER_ID_FRED_BLOGGS, null, DateTime.UtcNow);

			//Finally verify that we can retrieve for the test run
			List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.Task, taskId1);
			Assert.AreEqual(2, artifactLinks.Count);
			Assert.AreEqual("Test Case", artifactLinks[0].ArtifactTypeName);
			Assert.AreEqual(testCaseId, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Test Run", artifactLinks[1].ArtifactTypeName);
			Assert.AreEqual(testRunId, artifactLinks[1].ArtifactId);
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.Task, taskId2);
			Assert.AreEqual("Test Case", artifactLinks[0].ArtifactTypeName);
			Assert.AreEqual(testCaseId, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Test Run", artifactLinks[1].ArtifactTypeName);
			Assert.AreEqual(testRunId, artifactLinks[1].ArtifactId);

			//Now verify the other direction

			//Test Run
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.TestRun, testRunId);
			Assert.AreEqual(2, artifactLinks.Count);
			Assert.AreEqual("Task", artifactLinks[0].ArtifactTypeName);
			Assert.AreEqual(taskId2, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Task", artifactLinks[1].ArtifactTypeName);
			Assert.AreEqual(taskId1, artifactLinks[1].ArtifactId);

			//Test Case
			artifactLinks = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.TestCase, testCaseId);
			Assert.AreEqual(2, artifactLinks.Count);
			Assert.AreEqual("Task", artifactLinks[0].ArtifactTypeName);
			Assert.AreEqual(taskId2, artifactLinks[0].ArtifactId);
			Assert.AreEqual("Task", artifactLinks[1].ArtifactTypeName);
			Assert.AreEqual(taskId1, artifactLinks[1].ArtifactId);
		}
	}
}
