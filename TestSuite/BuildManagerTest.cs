using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Collections;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the BuildManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class BuildManagerTest
	{
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		protected static Business.BuildManager buildManager;
		protected static Business.ReleaseManager releaseManager;
		protected static int iterationId;
		protected static int projectId;
		private static int projectTemplateId;

		/// <summary>
		/// Sets up the fixture
		/// </summary>
		[TestFixtureSetUp]
		public void SetUp()
		{
			buildManager = new BuildManager();
			releaseManager = new ReleaseManager();

			//Create a new project for the tests
			ProjectManager projectManager = new ProjectManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("BuildManagerTest Sample Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		/// <summary>
		/// Cleans up after testing is done
		/// </summary>
		[TestFixtureTearDown]
		public void TearDown()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_FRED_BLOGGS, projectId);
			new TemplateManager().Delete(USER_ID_FRED_BLOGGS, projectTemplateId);
		}

		/// <summary>
		/// Tests that you can create, view and delete builds
		/// </summary>
		[
		Test,
		SpiraTestCase(809)
		]
		public void _01_CreateViewDeleteBuilds()
		{
			//First we need to create a new iteration to associate any builds with
			iterationId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", null, "1.0.0.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 1, 0, null);

			//Now create three builds for this iteration
			int buildId1 = buildManager.Insert(projectId, iterationId, "Build 0001", "Nice Build", DateTime.Now, Build.BuildStatusEnum.Succeeded, USER_ID_FRED_BLOGGS).BuildId;
			int buildId2 = buildManager.Insert(projectId, iterationId, "Build 0002", "Naughty Build", DateTime.Now, Build.BuildStatusEnum.Failed, USER_ID_FRED_BLOGGS).BuildId;
			int buildId3 = buildManager.Insert(projectId, iterationId, "Build 0003", null, DateTime.Now, Build.BuildStatusEnum.Succeeded, USER_ID_FRED_BLOGGS).BuildId;

			//Make sure that it checks that the release exists in the project
			bool exceptionThrown = false;
			try
			{
				buildManager.Insert(projectId, 1, "Build 0004", "Nice Build", DateTime.Now, Build.BuildStatusEnum.Succeeded, USER_ID_FRED_BLOGGS);
			}
			catch (EntityForeignKeyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Verify that we can retrieve the list of builds by release
			List<BuildView> builds = buildManager.RetrieveForRelease(projectId, iterationId, InternalRoutines.UTC_OFFSET);
			//Verify data
			Assert.AreEqual(3, builds.Count);
			Assert.AreEqual("Build 0001", builds[0].Name);
			Assert.AreEqual("Succeeded", builds[0].BuildStatusName);
			Assert.AreEqual("Build 0002", builds[1].Name);
			Assert.AreEqual("Failed", builds[1].BuildStatusName);

			//Verify that we can retrieve the list of builds with a filter and sort applied
			int artifactCount;
			Hashtable filters = new Hashtable();
			filters.Add("BuildStatusId", (int)Build.BuildStatusEnum.Succeeded);
			builds = buildManager.RetrieveForRelease(projectId, iterationId, "Name DESC", 0, 15, filters, InternalRoutines.UTC_OFFSET, out artifactCount);
			//Verify data
			Assert.AreEqual(2, builds.Count);
			Assert.AreEqual(2, artifactCount);
			Assert.AreEqual("Build 0003", builds[0].Name);
			Assert.AreEqual("Succeeded", builds[0].BuildStatusName);
			Assert.AreEqual("Build 0001", builds[1].Name);
			Assert.AreEqual("Succeeded", builds[1].BuildStatusName);

			//Verify that we can retrieve the list of builds in the project (all releases/iterations)
			builds = buildManager.RetrieveForProject(projectId);
			//Verify data
			Assert.AreEqual(3, builds.Count);
			Assert.AreEqual("Build 0001", builds[0].Name);
			Assert.AreEqual((int)Build.BuildStatusEnum.Succeeded, builds[0].BuildStatusId);
			Assert.AreEqual("Build 0002", builds[1].Name);
			Assert.AreEqual((int)Build.BuildStatusEnum.Failed, builds[1].BuildStatusId);

			//Verify that we can retrieve the list of builds in the project (all releases/iterations)
			builds = buildManager.RetrieveForProject(projectId);
			//Verify data
			Assert.AreEqual(3, builds.Count);
			Assert.AreEqual("Build 0001", builds[0].Name);
			Assert.AreEqual((int)Build.BuildStatusEnum.Succeeded, builds[0].BuildStatusId);
			Assert.AreEqual("Build 0002", builds[1].Name);
			Assert.AreEqual((int)Build.BuildStatusEnum.Failed, builds[1].BuildStatusId);

			//Verify that we can retrieve the list of builds in the project (all releases/iterations) with pagination and custom filter/sort
			builds = buildManager.RetrieveForProject(projectId, "Name DESC", 0, 15, filters, out artifactCount, InternalRoutines.UTC_OFFSET);
			//Verify data
			Assert.AreEqual(2, artifactCount);
			Assert.AreEqual(2, builds.Count);
			Assert.AreEqual("Build 0003", builds[0].Name);
			Assert.AreEqual((int)Build.BuildStatusEnum.Succeeded, builds[0].BuildStatusId);
			Assert.AreEqual("Build 0001", builds[1].Name);
			Assert.AreEqual("Succeeded", builds[1].BuildStatusName);

			//Test that we can retrieve a build by its ID (which includes its long description)
			Build build = buildManager.RetrieveById(buildId1);
			Assert.AreEqual("Build 0001", build.Name);
			Assert.AreEqual("Nice Build", build.Description);
			Assert.AreEqual("Succeeded", build.BuildStatusName);

			//Test that we can get the list of build statuses
			List<BuildStatus> statuses = buildManager.RetrieveStatuses();
			Assert.AreEqual(4, statuses.Count);
			Assert.AreEqual("Aborted", statuses[0].Name);
			Assert.AreEqual("Failed", statuses[1].Name);
			Assert.AreEqual("Succeeded", statuses[2].Name);
			Assert.AreEqual("Unstable", statuses[3].Name);

			//Test that we can retrieve the most recent build for all the projects and releases in the current group
			builds = buildManager.RetrieveForProjectGroup(1, true);
			Assert.AreEqual(1, builds.Count);
			Assert.AreEqual("Build 0003", builds[0].Name);
			Assert.AreEqual("Succeeded", builds[0].BuildStatusName);
			Assert.AreEqual("BuildManagerTest Sample Project", builds[0].ProjectName);

			//Test that we can get all the builds for all the projects (not releases) in the group
			builds = buildManager.RetrieveForProjectGroup(1, false);
			Assert.AreEqual(1, builds.Count);
			Assert.AreEqual("Build 0003", builds[0].Name);
			Assert.AreEqual("Succeeded", builds[0].BuildStatusName);
			Assert.AreEqual("BuildManagerTest Sample Project", builds[0].ProjectName);

			//Now we need to test that we can get the builds for multiple active releases and sprints for the group
			//We need to create an additional build and sprint
			int iterationId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2", null, "1.0.0.0 002", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 1, 0, null);
			int buildId5 = buildManager.Insert(projectId, iterationId2, "Build 0005", null, DateTime.Now, Build.BuildStatusEnum.Succeeded, USER_ID_FRED_BLOGGS).BuildId;

			//Get the most recent build for the projects in the group
			builds = buildManager.RetrieveForProjectGroup(1, false);
			Assert.AreEqual(1, builds.Count);
			Assert.AreEqual("Build 0005", builds[0].Name);
			Assert.AreEqual("Succeeded", builds[0].BuildStatusName);
			Assert.AreEqual("BuildManagerTest Sample Project", builds[0].ProjectName);

			//Get the most recent build for the projects and active releases in the group
			builds = buildManager.RetrieveForProjectGroup(1, true);
			Assert.AreEqual(2, builds.Count);
			Assert.AreEqual("Build 0005", builds[0].Name);
			Assert.AreEqual("Succeeded", builds[0].BuildStatusName);
			Assert.AreEqual("BuildManagerTest Sample Project", builds[0].ProjectName);
			Assert.AreEqual("Build 0003", builds[1].Name);
			Assert.AreEqual("Succeeded", builds[1].BuildStatusName);
			Assert.AreEqual("BuildManagerTest Sample Project", builds[1].ProjectName);

			//Test that soft-deleting an iteration also soft deletes the associated builds
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId);
			//Verify data
			builds = buildManager.RetrieveForRelease(projectId, iterationId, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, builds.Count);
			exceptionThrown = false;
			try
			{
				build = buildManager.RetrieveById(buildId1);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Verify that it's still truly there (setting includeDeleted=true)
			builds = buildManager.RetrieveForRelease(projectId, iterationId, InternalRoutines.UTC_OFFSET, true);
			Assert.AreEqual(3, builds.Count);
			build = buildManager.RetrieveById(buildId1, true);
			Assert.AreEqual("Build 0001", build.Name);
			Assert.AreEqual("Nice Build", build.Description);
			Assert.AreEqual("Succeeded", build.BuildStatusName);

			//Test that undeleting the iteration also restores the builds
			releaseManager.UnDelete(iterationId, USER_ID_FRED_BLOGGS, 0);
			builds = buildManager.RetrieveForRelease(projectId, iterationId, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, builds.Count);
			build = buildManager.RetrieveById(buildId1);
			Assert.AreEqual("Build 0001", build.Name);
			Assert.AreEqual("Nice Build", build.Description);
			Assert.AreEqual("Succeeded", build.BuildStatusName);

			//Finally test that we can hard-delete a release and it cascades correctly
			releaseManager.DeleteFromDatabase(iterationId, USER_ID_FRED_BLOGGS);
			//Verify data
			builds = buildManager.RetrieveForRelease(projectId, iterationId, InternalRoutines.UTC_OFFSET, true);
			Assert.AreEqual(0, builds.Count);
			exceptionThrown = false;
			try
			{
				build = buildManager.RetrieveById(buildId1, true);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);
		}

		/// <summary>
		/// Tests that you can add, delete and view source code revisions associated with a build
		/// </summary>
		[
		Test,
		SpiraTestCase(810)
		]
		public void _02_LinkRevisionsToBuilds()
		{
			//First we need to create a new iteration to associate any builds with
			iterationId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.0.0.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 1, 0, null);

			//Now create one builds for this iteration
			int buildId1 = buildManager.Insert(projectId, iterationId, "Build 0001", "Nice Build", DateTime.Now, Build.BuildStatusEnum.Succeeded, USER_ID_FRED_BLOGGS).BuildId;

			//Now add some source code revisions linked to this iteration
			buildManager.InsertSourceCodeRevision(buildId1, "rev001", DateTime.Now);
			buildManager.InsertSourceCodeRevision(buildId1, "rev002", DateTime.Now);
			buildManager.InsertSourceCodeRevision(buildId1, "rev003", DateTime.Now);

			//Verify the data
			List<BuildSourceCode> revisions = buildManager.RetrieveRevisionsForBuild(projectId, buildId1);
			Assert.AreEqual(3, revisions.Count);
			Assert.AreEqual("rev001", revisions[0].RevisionKey);
			Assert.AreEqual("rev002", revisions[1].RevisionKey);
			Assert.AreEqual("rev003", revisions[2].RevisionKey);

			//Test that we can get the build associated with a revision
			Build build = buildManager.RetrieveForRevision(projectId, "rev002").FirstOrDefault();
			Assert.AreEqual(buildId1, build.BuildId);
			Assert.AreEqual("Build 0001", build.Name);

			//Test that soft-deleting the iteration makes the revisions hidden
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId);
			revisions = buildManager.RetrieveRevisionsForBuild(projectId, buildId1);
			Assert.AreEqual(0, revisions.Count);

			//Test that undeleting the iteration also makes the revisions visible
			releaseManager.UnDelete(iterationId, USER_ID_FRED_BLOGGS, 0);
			revisions = buildManager.RetrieveRevisionsForBuild(projectId, buildId1);
			Assert.AreEqual(3, revisions.Count);

			//Finally test that we can hard-delete a release and it cascades correctly
			releaseManager.DeleteFromDatabase(iterationId, USER_ID_FRED_BLOGGS);
			revisions = buildManager.RetrieveRevisionsForBuild(projectId, buildId1);
			Assert.AreEqual(0, revisions.Count);
		}

		/// <summary>
		/// Tests that we can have the running of a build auto-schedule test sets linked to the build's release/iteration
		/// </summary>
		[Test]
		[SpiraTestCase(1543)]
		public void _03_AutoScheduleTestSets()
		{
			//First we need to create a new iteration to associate any builds with
			iterationId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", null, "1.0.0.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 1, 0, null);

			//Create an automation host
			AutomationManager automationManager = new AutomationManager();
			int automationHostId = automationManager.InsertHost(projectId, "Host 001", "Host001", null, true, USER_ID_FRED_BLOGGS);

			//Now let's create two test sets linked to this iteration, one marked as auto-schedule and one not (to verify the flag is being used correctly)
			TestSetManager testSetManager = new TestSetManager();
			int testSetId1 = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, iterationId, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Automated, automationHostId, null, /*AutoScheduled*/true, false, /* 5 Minutes */300);
			int testSetId2 = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, iterationId, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 2", null, null, TestRun.TestRunTypeEnum.Automated, automationHostId, null, /*NotAutoScheduled*/false);

			//Now lets record a build against the iteration that succeeds
			buildManager.Insert(projectId, iterationId, "Build 001", null, DateTime.UtcNow, Build.BuildStatusEnum.Succeeded, USER_ID_FRED_BLOGGS);

			//Verify that one of the test sets is now auto-scheduled
			TestSetView testSet1 = testSetManager.RetrieveById(projectId, testSetId1);
			Assert.IsNotNull(testSet1.PlannedDate);
			Assert.IsTrue(testSet1.PlannedDate >= DateTime.UtcNow);
			TestSetView testSet2 = testSetManager.RetrieveById(projectId, testSetId2);
			Assert.IsNull(testSet2.PlannedDate);

			//Reset the test set back to not planned
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId1);
			testSet.PlannedDate = null;
			testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Now verify that for failed builds neither is auto-scheduled
			buildManager.Insert(projectId, iterationId, "Build 001", null, DateTime.UtcNow, Build.BuildStatusEnum.Failed, USER_ID_FRED_BLOGGS);

			//Verify that neither is auto-scheduled
			testSet1 = testSetManager.RetrieveById(projectId, testSetId1);
			Assert.IsNull(testSet1.PlannedDate);
			testSet2 = testSetManager.RetrieveById(projectId, testSetId2);
			Assert.IsNull(testSet2.PlannedDate);

			//Finally delete the iteration and automation host
			automationManager.DeleteHostFromDatabase(automationHostId, USER_ID_FRED_BLOGGS);
			releaseManager.DeleteFromDatabase(iterationId, USER_ID_FRED_BLOGGS);
		}
	}
}
