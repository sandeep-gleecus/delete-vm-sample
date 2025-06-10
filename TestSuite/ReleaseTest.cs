using System;
using System.Linq;
using System.Collections;
using System.Data;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Release business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class ReleaseTest
	{
		protected static Business.ReleaseManager releaseManager;

		protected static int projectId = 0;
		protected static int projectId2 = 0;

		protected static int projectTemplateId = 0;
		protected static int projectTemplateId2 = 0;

		protected static int releaseId1;
		protected static int releaseId2;
		protected static int releaseId3;
		protected static int releaseId4;
		protected static int releaseId5;

		protected static int iterationId1;
		protected static int iterationId2;

		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;

		protected static int startingRelease1;
		protected static int startingRelease2;

		protected static int releaseToDeleteId1;
		protected static int releaseToDeleteId2;
		protected static int iterationToDeleteId1;

		private const int PROJECT_ID = 1;
		private const int PROJECT_GROUP_ID = 1;
		private const int SAMPLE_EMPTY_PROJECT_ID = 2;
		private const int SAMPLE_PROJECT_GROUP_ID = 2;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		[TestFixtureSetUp]
		public void Init()
		{
			releaseManager = new Business.ReleaseManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Create new projects for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("ReleaseTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			projectId2 = projectManager.Insert("ReleaseTest Project 2", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
			projectTemplateId2 = new TemplateManager().RetrieveForProject(projectId2).ProjectTemplateId;

			//Get the last artifact id
			Business.HistoryManager history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Purge any deleted items in the projects
			Business.HistoryManager history = new Business.HistoryManager();
			history.PurgeAllDeleted(PROJECT_ID, USER_ID_FRED_BLOGGS);

			//Delete the temporary project(s) and templates
			ProjectManager projectManager = new ProjectManager();
			TemplateManager templateManager = new TemplateManager();
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId);
			templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateId);
			if (projectId2 > 0)
			{
				projectManager.Delete(USER_ID_SYS_ADMIN, projectId2);
				templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateId2);
			}

			//We need to delete any artifact history items created
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());
		}

		[
		Test,
		SpiraTestCase(116)
		]
		public void _01_Retrieves()
		{
			//Lets test that we can retrieve an individual release in the sample project
			ReleaseView releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 2);

			//Verify the data
			Assert.AreEqual("AAAAAA", releaseView.IndentLevel);
			Assert.AreEqual("Library System Release 1 SP1", releaseView.Name);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Closed, releaseView.ReleaseStatusId);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, releaseView.ReleaseTypeId);
			Assert.AreEqual("1.0.1.0", releaseView.VersionNumber);
			Assert.AreEqual(1, releaseView.ProjectId);
			Assert.AreEqual("Joe P Smith", releaseView.CreatorName);
			Assert.IsTrue(releaseView.CreationDate >= DateTime.UtcNow.AddDays(-142));
			Assert.AreEqual("This service pack fixes identified bugs and a small security vulnerability", releaseView.Description);
			Assert.AreEqual(false, releaseView.IsActive, "ActiveYn");
			Assert.AreEqual(true, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(true, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");

			//Now test that we can retrieve the same item using the method that doesn't use user navigation data
			releaseView = releaseManager.RetrieveById2(PROJECT_ID, 2);
			Assert.AreEqual("AAAAAA", releaseView.IndentLevel);
			Assert.AreEqual("Library System Release 1 SP1", releaseView.Name);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Closed, releaseView.ReleaseStatusId);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, releaseView.ReleaseTypeId);
			Assert.AreEqual("1.0.1.0", releaseView.VersionNumber);
			Assert.AreEqual(1, releaseView.ProjectId);
			Assert.AreEqual("Joe P Smith", releaseView.CreatorName);
			Assert.IsTrue(releaseView.CreationDate >= DateTime.UtcNow.AddDays(-142));
			Assert.AreEqual("This service pack fixes identified bugs and a small security vulnerability", releaseView.Description);
			Assert.AreEqual(false, releaseView.IsActive, "ActiveYn");
			Assert.AreEqual(true, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");

			//Finally test that we can retrieve the actual release vs. the view
			Release release = releaseManager.RetrieveById3(PROJECT_ID, 2);
			Assert.AreEqual("AAAAAA", release.IndentLevel);
			Assert.AreEqual("Library System Release 1 SP1", release.Name);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Closed, release.ReleaseStatusId);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, release.ReleaseTypeId);
			Assert.AreEqual("1.0.1.0", release.VersionNumber);
			Assert.AreEqual(1, release.ProjectId);
			Assert.AreEqual(USER_ID_JOE_SMITH, release.CreatorId);
			Assert.IsTrue(release.CreationDate >= DateTime.UtcNow.AddDays(-142));
			Assert.AreEqual("This service pack fixes identified bugs and a small security vulnerability", release.Description);
			Assert.AreEqual(false, release.IsActive, "ActiveYn");
			Assert.AreEqual(true, release.IsSummary, "SummaryYn");
			Assert.AreEqual(false, release.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, release.IsIteration, "IterationYn");

			//Now Lets test that we can retrieve all the releases for a specific project (both all and active only)
			//Check that the results are different depending on the user (since Fred Bloggs has some collapsed)
			Hashtable activeFilter = new Hashtable();
			MultiValueFilter mvf = new MultiValueFilter();
			mvf.Values.Add((int)Release.ReleaseStatusEnum.Planned);
			mvf.Values.Add((int)Release.ReleaseStatusEnum.InProgress);
			mvf.Values.Add((int)Release.ReleaseStatusEnum.Completed);
			activeFilter.Add("ReleaseStatusId", mvf);
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(14, releases.Count);
			releases = releaseManager.RetrieveByProjectId(USER_ID_JOE_SMITH, PROJECT_ID, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(19, releases.Count);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, activeFilter, 0);
			Assert.AreEqual(6, releases.Count);
			releases = releaseManager.RetrieveByProjectId(USER_ID_JOE_SMITH, PROJECT_ID, 1, Int32.MaxValue, activeFilter, 0);
			Assert.AreEqual(10, releases.Count);

			//Verify the records

			//First Record
			releaseView = releases[0];
			Assert.AreEqual("AAA", releaseView.IndentLevel);
			Assert.AreEqual("Library System Release 1", releaseView.Name);
			Assert.AreEqual("1.0.0.0", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			Assert.IsTrue(releaseView.CreationDate >= DateTime.UtcNow.AddDays(-150));
			Assert.AreEqual("This is the initial release of the Library Management System", releaseView.Description);
			Assert.AreEqual(true, releaseView.IsActive, "ActiveYn");
			Assert.AreEqual(true, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(true, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");

			//Last Record
			releaseView = releases[9];
			Assert.AreEqual("AAC", releaseView.IndentLevel);
			Assert.AreEqual("Library System Release 1.2", releaseView.Name);
			Assert.AreEqual("1.2.0.0", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			//Assert.IsTrue(releaseView.CreationDate >= DateTime.UtcNow.AddDays(-121));
			Assert.IsTrue(releaseView.Description.IsNull());
			Assert.AreEqual(true, releaseView.IsActive, "ActiveYn");
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");

			//Now test that we can retrieve just the releases without the iterations/phases
			Hashtable releasesOnlyFilter = new Hashtable();
			mvf = new MultiValueFilter();
			mvf.Values.Add((int)Release.ReleaseTypeEnum.MajorRelease);
			mvf.Values.Add((int)Release.ReleaseTypeEnum.MinorRelease);
			releasesOnlyFilter.Add("ReleaseTypeId", mvf);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, Int32.MaxValue, releasesOnlyFilter, 0);
			Assert.AreEqual(5, releases.Count);

			//Test that we can retrieve a release together with its peers, parent and children
			releases = releaseManager.RetrievePeersChildrenAndParent(USER_ID_FRED_BLOGGS, PROJECT_ID, "AAAAAA");
			Assert.AreEqual(9, releases.Count);
			Assert.AreEqual("Library System Release 1", releases[0].Name);
			Assert.AreEqual("Library System Release 1 SP1", releases[1].Name);
			Assert.AreEqual("SP1 Sprint 001", releases[2].Name);
			Assert.AreEqual("Library System Release 1 SP2", releases[5].Name);

			//If the release has no parent then we should only retrieve the peers and children
			releases = releaseManager.RetrievePeersChildrenAndParent(USER_ID_FRED_BLOGGS, PROJECT_ID, "AAA");
			Assert.AreEqual(8, releases.Count);
			Assert.AreEqual("Library System Release 1", releases[0].Name);
			Assert.AreEqual("Library System Release 1 SP1", releases[1].Name);
			Assert.AreEqual("Library System Release 1 SP2", releases[2].Name);
			Assert.AreEqual("Patch Sprint 001", releases[3].Name);
			Assert.AreEqual("Library System Release 1.1", releases[6].Name);

			//Verify that we can retrieve the releases with the appropriate test status information
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(14, releases.Count);
			ReleaseView releaseRow = releases[0];
			Assert.AreEqual("Library System Release 1", releaseRow.Name);
			Assert.AreEqual(0, releaseRow.CountNotRun);
			Assert.AreEqual(4, releaseRow.CountPassed);
			Assert.AreEqual(2, releaseRow.CountFailed);
			Assert.AreEqual(1, releaseRow.CountCaution);
			Assert.AreEqual(0, releaseRow.CountBlocked);
			releaseRow = releases[1];
			Assert.AreEqual("Library System Release 1 SP1", releaseRow.Name);
			Assert.AreEqual(3, releaseRow.CountNotRun);
			Assert.AreEqual(3, releaseRow.CountPassed);
			Assert.AreEqual(0, releaseRow.CountFailed);
			Assert.AreEqual(0, releaseRow.CountCaution);
			Assert.AreEqual(1, releaseRow.CountBlocked);
			releaseRow = releases[2];
			Assert.AreEqual("SP1 Sprint 001", releaseRow.Name);
			Assert.AreEqual(0, releaseRow.CountNotRun);
			Assert.AreEqual(0, releaseRow.CountPassed);
			Assert.AreEqual(0, releaseRow.CountFailed);
			Assert.AreEqual(0, releaseRow.CountCaution);
			Assert.AreEqual(0, releaseRow.CountBlocked);
			releaseRow = releases[5];
			Assert.AreEqual("Library System Release 1 SP2", releaseRow.Name);
			Assert.AreEqual(0, releaseRow.CountNotRun);
			Assert.AreEqual(2, releaseRow.CountPassed);
			Assert.AreEqual(0, releaseRow.CountFailed);
			Assert.AreEqual(0, releaseRow.CountCaution);
			Assert.AreEqual(0, releaseRow.CountBlocked);
			releaseRow = releases[12];
			Assert.AreEqual("Library System Release 1.1", releaseRow.Name);
			Assert.AreEqual(2, releaseRow.CountNotRun);
			Assert.AreEqual(3, releaseRow.CountPassed);
			Assert.AreEqual(2, releaseRow.CountFailed);
			Assert.AreEqual(2, releaseRow.CountCaution);
			Assert.AreEqual(0, releaseRow.CountBlocked);
			releaseRow = releases[13];
			Assert.AreEqual("Library System Release 1.2", releaseRow.Name);
			Assert.AreEqual(7, releaseRow.CountNotRun);
			Assert.AreEqual(0, releaseRow.CountPassed);
			Assert.AreEqual(0, releaseRow.CountFailed);
			Assert.AreEqual(0, releaseRow.CountCaution);
			Assert.AreEqual(0, releaseRow.CountBlocked);

			//Now verify that we can retrieve the list of iterations for a given release
			//this is used in the Iteration Planning Module
			releases = releaseManager.RetrieveSelfAndIterations(PROJECT_ID, 1, false);
			Assert.AreEqual(4, releases.Count);
			Assert.AreEqual("Library System Release 1", releases[0].Name);
			Assert.AreEqual("Patch Sprint 001", releases[1].Name);
			Assert.AreEqual("Patch Sprint 002", releases[2].Name);
			Assert.AreEqual("Patch Sprint 003", releases[3].Name);
			int parentReleaseId = releases[0].ReleaseId;

			//Now verify that we can retrieve the list of iterations without the release itself
			releases = releaseManager.RetrieveIterations(PROJECT_ID, 1, false);
			Assert.AreEqual(3, releases.Count);
			Assert.AreEqual("Patch Sprint 001", releases[0].Name);
			Assert.AreEqual("Patch Sprint 002", releases[1].Name);
			Assert.AreEqual("Patch Sprint 003", releases[2].Name);

			//Now verify that we can retrieve the parent ID of an iteration or release
			int? testReleaseId = releaseManager.GetParentReleaseId(releases[0].ReleaseId);
			Assert.IsNotNull(testReleaseId);
			Assert.AreEqual(parentReleaseId, testReleaseId.Value);

			//Verify that we can get the list of child ids for all three cases
			List<int> releaseIds = releaseManager.GetSelfAndIterations(PROJECT_ID, 1);
			Assert.AreEqual(4, releaseIds.Count);
			releaseIds = releaseManager.GetSelfAndChildRollupChildren(PROJECT_ID, 1, false);
			Assert.AreEqual(12, releaseIds.Count);
			releaseIds = releaseManager.GetSelfAndChildRollupChildren(PROJECT_ID, 1, true);
			Assert.AreEqual(12, releaseIds.Count);


			/*
             * Verify that we can retrieve releases for a particular program
             */
			//get initial number of active releases in the two projects in the group (assume the 3rd project is empty and unused completely)
			int activeReleaseCountProjectLIS = releaseManager.RetrieveByProjectId(PROJECT_ID, true).Count();
			int activeReleaseCountProjectSample = releaseManager.RetrieveByProjectId(SAMPLE_EMPTY_PROJECT_ID, true).Count();
			int activeReleaseCountAtStart = activeReleaseCountProjectLIS + activeReleaseCountProjectSample;

			//SETUP
			int sampleEmptyProjectReleaseId = releaseManager.Insert(
				USER_ID_SYS_ADMIN,
				SAMPLE_EMPTY_PROJECT_ID,
				USER_ID_SYS_ADMIN,
				"sampleEmptyProduct1Release",
				"Description for sampleEmptyProduct1Release",
				"sample1",
				"AAA",
				Release.ReleaseStatusEnum.InProgress,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.UtcNow.AddDays(-10),
				DateTime.UtcNow.AddDays(-5),
				0,
				0,
				null,
				true
				);
			//check the number of active releases for the program is 1 more (due to the new release in sample empty product) than those from LIS
			List<Release> releasesByProjectGroup = releaseManager.Release_RetrieveByProjectGroup(SAMPLE_PROJECT_GROUP_ID, true);
			int activeReleaseCountProjectGroup = releasesByProjectGroup.Count();
			//Assert.AreEqual(activeReleaseCountAtStart + 1, activeReleaseCountProjectGroup, "active releases for Program should be 1 more than those for project LIS");

			//VERIFY: can also retrieve inactive and active releases for a project group
			releasesByProjectGroup = releaseManager.Release_RetrieveByProjectGroup(SAMPLE_PROJECT_GROUP_ID, false);
			Assert.Less(activeReleaseCountProjectGroup, releasesByProjectGroup.Count(), "should be fewer active releases in project group than all releases");

			//Make the new release closed 
			release = releaseManager.RetrieveById3(SAMPLE_EMPTY_PROJECT_ID, sampleEmptyProjectReleaseId);
			release.StartTracking();
			release.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Closed;
			releaseManager.Update(new List<Release>() { release }, USER_ID_SYS_ADMIN, SAMPLE_EMPTY_PROJECT_ID);

			//VERIFY: that the inactive release is not returned
			releasesByProjectGroup = releaseManager.Release_RetrieveByProjectGroup(SAMPLE_PROJECT_GROUP_ID, true);
			Assert.AreEqual(activeReleaseCountProjectGroup - 1, releasesByProjectGroup.Count(), "should have 1 less release now that 1 has been made inactive in the project group");

			//Make the new release in progress again 
			release = releaseManager.RetrieveById3(SAMPLE_EMPTY_PROJECT_ID, sampleEmptyProjectReleaseId);
			release.StartTracking();
			release.ReleaseStatusId = (int)Release.ReleaseStatusEnum.InProgress;
			releaseManager.Update(new List<Release>() { release }, USER_ID_SYS_ADMIN, SAMPLE_EMPTY_PROJECT_ID);

			//make sample empty product inactive
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			Project sampleEmptyProject = projectManager.RetrieveById(SAMPLE_EMPTY_PROJECT_ID);
			sampleEmptyProject.StartTracking();
			sampleEmptyProject.IsActive = false;
			projectManager.Update(sampleEmptyProject, 1, adminSectionId, "Updated Project");

			//VERIFY: that its release is not retrieved with the rest of those in the project group
			releasesByProjectGroup = releaseManager.Release_RetrieveByProjectGroup(SAMPLE_PROJECT_GROUP_ID, true);
			//Assert.AreEqual(activeReleaseCountProjectLIS, releasesByProjectGroup.Count(), "releases for Program should match those in LIS");

			//make sample empty product active again
			sampleEmptyProject = projectManager.RetrieveById(SAMPLE_EMPTY_PROJECT_ID);
			sampleEmptyProject.StartTracking();
			sampleEmptyProject.IsActive = true;
			projectManager.Update(sampleEmptyProject, 1, adminSectionId, "Updated Project");

			//VERIFY: again that the number of active releases for the program is 1 more (due to the new release in sample empty product) than those from LIS
			releasesByProjectGroup = releaseManager.Release_RetrieveByProjectGroup(SAMPLE_PROJECT_GROUP_ID, true);
			//Assert.AreEqual(activeReleaseCountAtStart + 1, releasesByProjectGroup.Count(), "releases for Program should be 1 more than those for project LIS");

			//CLEANUP: delete the release created above
			releaseManager.DeleteFromDatabase(sampleEmptyProjectReleaseId, USER_ID_SYS_ADMIN);

			//VERIFY: again that the number of active releases for the program is 1 more (due to the new release in sample empty product) than those from LIS
			releasesByProjectGroup = releaseManager.Release_RetrieveByProjectGroup(SAMPLE_PROJECT_GROUP_ID, true);
			//Assert.AreEqual(activeReleaseCountAtStart, releasesByProjectGroup.Count(), "releases for Program should only come from LIS");
		}

		[
		Test,
		SpiraTestCase(117)
		]
		public void _02_InsertRelease()
		{
			//First lets create two releases as a starting point
			startingRelease1 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 1.0",
				"Test Description of Release 1.0",
				"1.0.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				2,
				2,
				null,
				false
				);

			startingRelease2 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 1.2",
				"Test Description of Release 1.2",
				"1.2.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				2,
				2,
				null,
				false
				);

			//First lets test that we can insert a release in the new project in the middle of this list
			releaseId1 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 1.1",
				"Test Description of Release 1.1",
				"",
				startingRelease2,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				2,
				2,
				null,
				false
				);

			//Verify the data and position of the new items
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(3, releases.Count);
			Assert.AreEqual(startingRelease1, releases[0].ReleaseId);
			Assert.AreEqual(releaseId1, releases[1].ReleaseId);
			Assert.AreEqual(startingRelease2, releases[2].ReleaseId);

			//Now verify the data of our newly inserted release
			ReleaseView releaseView = releases[1];
			Assert.AreEqual(releaseId1, releaseView.ReleaseId);
			Assert.AreEqual("Test Release 1.1", releaseView.Name);
			Assert.AreEqual("1.2.0.1", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			Assert.AreEqual("AAB", releaseView.IndentLevel);
			Assert.AreEqual("Test Description of Release 1.1", releaseView.Description);
			Assert.AreEqual("Planned", releaseView.ReleaseStatusName);
			Assert.AreEqual("Major Release", releaseView.ReleaseTypeName);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(DateTime.Parse("1/1/2004"), releaseView.StartDate, "StartDate");
			Assert.AreEqual(DateTime.Parse("1/31/2004"), releaseView.EndDate, "EndDate");
			Assert.AreEqual(2, releaseView.ResourceCount, "ResourceCount");
			Assert.AreEqual(2, releaseView.DaysNonWorking, "DaysNonWorking");
			Assert.AreEqual(20160, releaseView.PlannedEffort, "PlannedEffort");
			Assert.AreEqual(20160, releaseView.AvailableEffort, "AvailableEffort");

			//Verify that the next item was moved down one position
			releaseView = releases[2];
			Assert.AreEqual("AAC", releaseView.IndentLevel);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual("Planned", releaseView.ReleaseStatusName);
			Assert.AreEqual("Major Release", releaseView.ReleaseTypeName);

			//Next lets test that we can insert a release in the end of the hierarchy
			releaseId2 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 2.0",
				null,
				"2.0.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				2,
				2,
				null,
				false
				);

			//Verify the data and position of the new item
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(4, releases.Count);
			releaseView = releases[3];
			Assert.AreEqual(releaseId2, releaseView.ReleaseId);
			Assert.AreEqual("Test Release 2.0", releaseView.Name);
			Assert.AreEqual("2.0.0.0", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			Assert.AreEqual("AAD", releaseView.IndentLevel);
			Assert.IsTrue(releaseView.Description.IsNull());
			Assert.AreEqual(true, releaseView.IsActive);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");
			Assert.AreEqual("Planned", releaseView.ReleaseStatusName);
			Assert.AreEqual("Major Release", releaseView.ReleaseTypeName);

			//Now make sure that we can insert a child release as well
			releaseId3 = releaseManager.InsertChild(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 1.1 SP1",
				"Test Description of Release 1.1 SP1",
				"1.1.1.0",
				releaseId1,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MinorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				1,
				1,
				null,
				false
				);

			//Verify the data and position of the new item
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(5, releases.Count);
			releaseView = releases[2];
			Assert.AreEqual(releaseId3, releaseView.ReleaseId);
			Assert.AreEqual("Test Release 1.1 SP1", releaseView.Name);
			Assert.AreEqual("1.1.1.0", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			Assert.AreEqual("AABAAA", releaseView.IndentLevel);
			Assert.AreEqual("Test Description of Release 1.1 SP1", releaseView.Description);
			Assert.AreEqual(true, releaseView.IsActive);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");
			Assert.AreEqual("Planned", releaseView.ReleaseStatusName);
			Assert.AreEqual("Minor Release", releaseView.ReleaseTypeName);

			//Verify that the parent is now a 'summary' item
			releaseView = releases[1];
			Assert.AreEqual(releaseId1, releaseView.ReleaseId);
			Assert.AreEqual("Test Release 1.1", releaseView.Name);
			Assert.AreEqual("1.2.0.1", releaseView.VersionNumber);
			Assert.AreEqual("Test Description of Release 1.1", releaseView.Description);
			Assert.AreEqual("AAB", releaseView.IndentLevel);
			Assert.AreEqual(true, releaseView.IsActive);
			Assert.AreEqual(true, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(true, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");

			//Now insert an item in front of this one
			releaseId4 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 1.1 SP2",
				null,
				"1.1.2.0",
				releaseId3,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MinorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				1,
				1,
				null,
				false
				);

			//Verify the data and position of the new item
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(6, releases.Count);
			releaseView = releases[2];
			Assert.AreEqual(releaseId4, releaseView.ReleaseId);
			Assert.AreEqual("Test Release 1.1 SP2", releaseView.Name);
			Assert.AreEqual("1.1.2.0", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			Assert.AreEqual("AABAAA", releaseView.IndentLevel);
			Assert.IsTrue(String.IsNullOrEmpty(releaseView.Description));
			Assert.AreEqual(true, releaseView.IsActive);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");
			Assert.AreEqual("Planned", releaseView.ReleaseStatusName);
			Assert.AreEqual("Minor Release", releaseView.ReleaseTypeName);

			//Verify that the next item was moved down one position
			releaseView = releases[3];
			Assert.AreEqual("Test Release 1.1 SP1", releaseView.Name);
			Assert.AreEqual("1.1.1.0", releaseView.VersionNumber);
			Assert.AreEqual("AABAAB", releaseView.IndentLevel);
			Assert.AreEqual(true, releaseView.IsActive);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");
			Assert.AreEqual("Planned", releaseView.ReleaseStatusName);
			Assert.AreEqual("Minor Release", releaseView.ReleaseTypeName);

			//Now make sure that we can insert a release before the first one in the list
			releaseId5 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 4.0",
				"Test Description of Release 4.0",
				"4.0.0.0",
				startingRelease1,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				2,
				2,
				null,
				false
				);

			//Verify the data and position of the new item
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(7, releases.Count);
			releaseView = releases[0];
			Assert.AreEqual(releaseId5, releaseView.ReleaseId);
			Assert.AreEqual("Test Release 4.0", releaseView.Name);
			Assert.AreEqual("4.0.0.0", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			Assert.AreEqual("AAA", releaseView.IndentLevel);
			Assert.AreEqual("Test Description of Release 4.0", releaseView.Description);
			Assert.AreEqual(true, releaseView.IsActive);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");
			Assert.AreEqual("Planned", releaseView.ReleaseStatusName);
			Assert.AreEqual("Major Release", releaseView.ReleaseTypeName);

			//Verify that the next item was moved down one position
			releaseView = releases[1];
			Assert.AreEqual(startingRelease1, releaseView.ReleaseId);
			Assert.AreEqual("Test Release 1.0", releaseView.Name);
			Assert.AreEqual("AAB", releaseView.IndentLevel);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(false, releaseView.IsIteration, "IterationYn");
			Assert.AreEqual("Planned", releaseView.ReleaseStatusName);
			Assert.AreEqual("Major Release", releaseView.ReleaseTypeName);

			//Verify the complete hierarchy
			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(startingRelease1, releases[1].ReleaseId);
			Assert.AreEqual("AAB", releases[1].IndentLevel);
			Assert.AreEqual(releaseId1, releases[2].ReleaseId);
			Assert.AreEqual("AAC", releases[2].IndentLevel);
			Assert.AreEqual(releaseId4, releases[3].ReleaseId);
			Assert.AreEqual("AACAAA", releases[3].IndentLevel);
			Assert.AreEqual(releaseId3, releases[4].ReleaseId);
			Assert.AreEqual("AACAAB", releases[4].IndentLevel);
			Assert.AreEqual(startingRelease2, releases[5].ReleaseId);
			Assert.AreEqual("AAD", releases[5].IndentLevel);
			Assert.AreEqual(releaseId2, releases[6].ReleaseId);
			Assert.AreEqual("AAE", releases[6].IndentLevel);
		}

		[
		Test,
		SpiraTestCase(314)
		]
		public void _03_InsertIteration()
		{
			//Now make sure that we can insert an iteration in the middle of the list, under the summary release
			iterationId1 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Sprint 001",
				"Test Description of Sprint 001",
				null,
				releaseId3,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				2,
				2,
				null,
				false
				);

			//Verify the data and indent level of the new item
			ReleaseView releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual(iterationId1, releaseView.ReleaseId);
			Assert.AreEqual("Test Sprint 001", releaseView.Name);
			Assert.AreEqual("1.0.0.0.0000", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);
			Assert.AreEqual("Test Description of Sprint 001", releaseView.Description);
			Assert.AreEqual(true, releaseView.IsActive);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(true, releaseView.IsIteration, "IsIteration");
			Assert.AreEqual(true, releaseView.IsIterationOrPhase, "IsIterationOrPhase");

			//Next test that we can insert a child iteration under a release
			iterationId2 = releaseManager.InsertChild(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Sprint 002",
				"Test Description of Sprint 002",
				null,
				releaseId5,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				2,
				2,
				null,
				false
				);

			//Verify new item
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			Assert.AreEqual(iterationId2, releaseView.ReleaseId);
			Assert.AreEqual("Test Sprint 002", releaseView.Name);
			Assert.AreEqual("1.0.0.0.0001", releaseView.VersionNumber);
			Assert.AreEqual("Fred Bloggs", releaseView.CreatorName);
			Assert.AreEqual("AAAAAA", releaseView.IndentLevel);
			Assert.AreEqual("Test Description of Sprint 002", releaseView.Description);
			Assert.AreEqual(true, releaseView.IsActive);
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, releaseView.IsVisible, "VisibleYn");
			Assert.AreEqual(false, releaseView.IsAttachments, "AttachmentsYn");
			Assert.AreEqual(true, releaseView.IsIteration, "IsIteration");
			Assert.AreEqual(true, releaseView.IsIterationOrPhase, "IsIterationOrPhase");

			//Verify parent is now a summary item
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId5);
			Assert.AreEqual(true, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(true, releaseView.IsExpanded, "ExpandedYn");

			//Finally make sure that we cannot insert a child iteration under another iteration/phase
			bool exceptionThrown = false;
			try
			{
				releaseManager.InsertChild(
					USER_ID_FRED_BLOGGS,
					projectId,
					USER_ID_FRED_BLOGGS,
					"Test Sprint 003",
					"Test Description of Sprint 003",
					null,
					iterationId1,
					Release.ReleaseStatusEnum.Planned,
					Release.ReleaseTypeEnum.Iteration,
					DateTime.Parse("1/1/2004"),
					DateTime.Parse("1/31/2004"),
					2,
					2,
					null,
					false
					);
			}
			catch (IterationSummaryException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Verify the complete hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(9, releases.Count);

			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(iterationId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAB", releases[5].IndentLevel);
			Assert.AreEqual(releaseId3, releases[6].ReleaseId);
			Assert.AreEqual("AACAAC", releases[6].IndentLevel);
			Assert.AreEqual(startingRelease2, releases[7].ReleaseId);
			Assert.AreEqual("AAD", releases[7].IndentLevel);
			Assert.AreEqual(releaseId2, releases[8].ReleaseId);
			Assert.AreEqual("AAE", releases[8].IndentLevel);
		}

		[
		Test,
		SpiraTestCase(118)
		]
		public void _04_UpdateRelease()
		{
			//Retrieve an existing release and make some changes
			Release release = releaseManager.RetrieveById3(projectId, releaseId1);
			release.StartTracking();
			release.Name = "Test Release 1.1";
			release.Description = "Description of Test Release 1.1";
			release.VersionNumber = "1.1.0.8";
			release.OwnerId = USER_ID_JOE_SMITH;
			release.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Deferred;
			releaseManager.Update(new List<Release>() { release }, USER_ID_FRED_BLOGGS, projectId);

			//Verify the data was updated correctly
			ReleaseView releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(releaseId1, releaseView.ReleaseId);
			Assert.AreEqual("Test Release 1.1", releaseView.Name);
			Assert.AreEqual("Description of Test Release 1.1", releaseView.Description);
			Assert.AreEqual("1.1.0.8", releaseView.VersionNumber);
			Assert.AreEqual("Joe P Smith", releaseView.OwnerName);
			Assert.AreEqual("Deferred", releaseView.ReleaseStatusName);
			Assert.AreEqual(false, releaseView.IsActive);

			//Now change the status of the release back to 'Planned'
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			release.StartTracking();
			release.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
			releaseManager.Update(new List<Release>() { release }, USER_ID_FRED_BLOGGS, projectId);
		}

		[
		Test,
		SpiraTestCase(119)
		]
		public void _05_IndentOutdent()
		{
			//This tests that we can correctly indent and outdent a single release/iteration
			ReleaseView release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual(false, release.IsSummary, "SummaryYn");
			Assert.AreEqual("AACAAA", release.IndentLevel, "IndentLevel");
			Assert.AreEqual(false, release.IsExpanded, "ExpandedYn");
			Assert.AreEqual(false, release.IsIteration, "IterationYn");

			//Do the indent and check that adjacent items are correct
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual(true, release.IsSummary, "SummaryYn");
			Assert.AreEqual("AACAAA", release.IndentLevel, "IndentLevel");
			Assert.AreEqual(true, release.IsExpanded, "ExpandedYn");
			Assert.AreEqual(false, release.IsIteration, "IterationYn");

			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAAAAA", release.IndentLevel, "Indent Level");
			Assert.AreEqual(false, release.IsSummary, "SummaryYn");
			Assert.AreEqual(false, release.IsExpanded, "ExpandedYn");
			Assert.AreEqual(true, release.IsIteration, "IterationYn");

			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAB", release.IndentLevel, "IndentLevel");

			//Do the outdent and check that adjacent items are correct
			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual(false, release.IsSummary, "SummaryYn");
			Assert.AreEqual("AACAAA", release.IndentLevel, "Indent Level");
			Assert.AreEqual(false, release.IsExpanded, "ExpandedYn");
			Assert.AreEqual(false, release.IsIteration, "IterationYn");

			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAB", release.IndentLevel, "IndentLevel");

			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAC", release.IndentLevel, "IndentLevel");

			//Now let's test that we can indent an item when its predecessor already has child releases/iterations
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, startingRelease2);
			Assert.AreEqual("AAD", release.IndentLevel, "IndentLevel");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAE", release.IndentLevel, "IndentLevel");

			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, startingRelease2);
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, startingRelease2);
			Assert.AreEqual("AACAAD", release.IndentLevel, "Indent Level");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAD", release.IndentLevel, "Indent Level");

			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId, startingRelease2);
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, startingRelease2);
			Assert.AreEqual("AAD", release.IndentLevel, "Indent Level");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAE", release.IndentLevel, "Indent Level");

			//Now lets test that we can indent an item that has existing children
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, startingRelease1);
			Assert.AreEqual("AAB", release.IndentLevel, "IndentLevel");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AAC", release.IndentLevel, "IndentLevel");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AACAAA", release.IndentLevel, "IndentLevel");

			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, startingRelease1);
			Assert.AreEqual("AAB", release.IndentLevel, "IndentLevel");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AABAAA", release.IndentLevel, "IndentLevel");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AABAAAAAA", release.IndentLevel, "IndentLevel");

			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, startingRelease1);
			Assert.AreEqual("AAB", release.IndentLevel, "IndentLevel");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AAC", release.IndentLevel, "IndentLevel");
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AACAAA", release.IndentLevel, "IndentLevel");

			//Now lets test that indenting an existing iteration below another iteration is not allowed
			bool exceptionThrown = false;
			try
			{
				releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			}
			catch (IterationSummaryException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to make a summary sprint");

			//Verify the complete hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(9, releases.Count);

			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(iterationId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAB", releases[5].IndentLevel);
			Assert.AreEqual(releaseId3, releases[6].ReleaseId);
			Assert.AreEqual("AACAAC", releases[6].IndentLevel);
			Assert.AreEqual(startingRelease2, releases[7].ReleaseId);
			Assert.AreEqual("AAD", releases[7].IndentLevel);
			Assert.AreEqual(releaseId2, releases[8].ReleaseId);
			Assert.AreEqual("AAE", releases[8].IndentLevel);
		}

		[
		Test,
		SpiraTestCase(120)
		]
		public void _06_DeleteRelease()
		{
			//Let's create some new releases/iterations to delete, in between the other releases
			releaseToDeleteId1 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Dummy Release 3.0",
				"Test Description of Release 3.0",
				"3.0.0.0",
				startingRelease2,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				1,
				1,
				null,
				false
				);

			releaseToDeleteId2 = releaseManager.InsertChild(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Dummy Release 3.1",
				"Test Description of Release 3.1",
				"3.1.0.0",
				releaseToDeleteId1,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MinorRelease,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				1,
				1,
				null,
				false
				);

			iterationToDeleteId1 = releaseManager.InsertChild(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Dummy Release 3.1 Sprint 001",
				"Test Description of Release 3.1 Sprint 001",
				"3.1.0.0-001",
				releaseToDeleteId2,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				1,
				1,
				null,
				false
				);

			//Lets attach the release to a new incident - tests that the delete should un-associate first
			IncidentManager incidentManager = new IncidentManager();
			int incidentId1 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 1", "Really bad incident", releaseToDeleteId1, iterationToDeleteId1, iterationToDeleteId1, null, null, DateTime.Now, DateTime.Parse("4/1/2004"), null, 2400, null, null, null, null, USER_ID_FRED_BLOGGS);

			//Lets attach the release to a new requirement - the delete should un-associate first
			RequirementManager requirementManager = new RequirementManager();
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseToDeleteId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 1", null, 1.0M, USER_ID_FRED_BLOGGS);

			//Verify the complete hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(12, releases.Count);

			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(iterationId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAB", releases[5].IndentLevel);
			Assert.AreEqual(releaseId3, releases[6].ReleaseId);
			Assert.AreEqual("AACAAC", releases[6].IndentLevel);

			Assert.AreEqual(releaseToDeleteId1, releases[7].ReleaseId);
			Assert.AreEqual("AAD", releases[7].IndentLevel);
			Assert.AreEqual(releaseToDeleteId2, releases[8].ReleaseId);
			Assert.AreEqual("AADAAA", releases[8].IndentLevel);
			Assert.AreEqual(iterationToDeleteId1, releases[9].ReleaseId);
			Assert.AreEqual("AADAAAAAA", releases[9].IndentLevel);

			Assert.AreEqual(startingRelease2, releases[10].ReleaseId);
			Assert.AreEqual("AAE", releases[10].IndentLevel);
			Assert.AreEqual(releaseId2, releases[11].ReleaseId);
			Assert.AreEqual("AAF", releases[11].IndentLevel);

			//Verify that we can delete the new releases and one iteration by simply deleting the parent
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseToDeleteId1);

			//Verify that the items are removed, but since they were not purged, the indent levels will stay the same
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(9, releases.Count);

			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(iterationId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAB", releases[5].IndentLevel);
			Assert.AreEqual(releaseId3, releases[6].ReleaseId);
			Assert.AreEqual("AACAAC", releases[6].IndentLevel);
			Assert.AreEqual(startingRelease2, releases[7].ReleaseId);
			Assert.AreEqual("AAE", releases[7].IndentLevel);
			Assert.AreEqual(releaseId2, releases[8].ReleaseId);
			Assert.AreEqual("AAF", releases[8].IndentLevel);
		}

		[
		Test,
		SpiraTestCase(260)
		]
		public void _07_ExpandCollapse()
		{
			//Add a new child item to create a three-level hierarchy
			iterationToDeleteId1 = releaseManager.InsertChild(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Dummy Release 1.1SP1 Sprint 001",
				"Test Description of Release 1.1SP1 Sprint 001",
				"1.1.2.0-001",
				releaseId4,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.Parse("1/1/2004"),
				DateTime.Parse("1/31/2004"),
				1,
				1,
				null,
				false
				);

			//First lets test the initial state to ensure that it's fully expanded
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(10, releases.Count);

			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(true, releases[0].IsExpanded);
			Assert.AreEqual(true, releases[0].IsVisible);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(false, releases[1].IsExpanded);
			Assert.AreEqual(true, releases[1].IsVisible);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(false, releases[2].IsExpanded);
			Assert.AreEqual(true, releases[2].IsVisible);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(true, releases[3].IsExpanded);
			Assert.AreEqual(true, releases[3].IsVisible);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(true, releases[4].IsExpanded);
			Assert.AreEqual(true, releases[4].IsVisible);
			Assert.AreEqual(iterationToDeleteId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAAAAA", releases[5].IndentLevel);
			Assert.AreEqual(false, releases[5].IsExpanded);
			Assert.AreEqual(true, releases[5].IsVisible);
			Assert.AreEqual(iterationId1, releases[6].ReleaseId);
			Assert.AreEqual("AACAAB", releases[6].IndentLevel);
			Assert.AreEqual(false, releases[6].IsExpanded);
			Assert.AreEqual(true, releases[6].IsVisible);
			Assert.AreEqual(releaseId3, releases[7].ReleaseId);
			Assert.AreEqual("AACAAC", releases[7].IndentLevel);
			Assert.AreEqual(false, releases[7].IsExpanded);
			Assert.AreEqual(true, releases[7].IsVisible);
			Assert.AreEqual(startingRelease2, releases[8].ReleaseId);
			Assert.AreEqual("AAE", releases[8].IndentLevel);
			Assert.AreEqual(false, releases[8].IsExpanded);
			Assert.AreEqual(true, releases[8].IsVisible);
			Assert.AreEqual(releaseId2, releases[9].ReleaseId);
			Assert.AreEqual("AAF", releases[9].IndentLevel);
			Assert.AreEqual(false, releases[9].IsExpanded);
			Assert.AreEqual(true, releases[9].IsVisible);

			//Now lets collapse the 3-level tree and test that it worked correctly
			releaseManager.Collapse(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			ReleaseView releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual(false, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(true, releaseView.IsVisible, "Visible");
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual(false, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(false, releaseView.IsVisible, "Visible");
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationToDeleteId1);
			Assert.AreEqual(false, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(false, releaseView.IsVisible, "Visible");

			//Now lets expand the top-level, and it should expand one-level only
			releaseManager.Expand(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual(true, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(true, releaseView.IsVisible, "Visible");
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual(false, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(true, releaseView.IsVisible, "Visible");
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationToDeleteId1);
			Assert.AreEqual(false, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(false, releaseView.IsVisible, "Visible");

			//Now lets expand the next level
			releaseManager.Expand(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual(true, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(true, releaseView.IsVisible, "Visible");
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual(true, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(true, releaseView.IsVisible, "Visible");
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationToDeleteId1);
			Assert.AreEqual(false, releaseView.IsExpanded, "Expanded");
			Assert.AreEqual(true, releaseView.IsVisible, "Visible");

			//Now test that we can expand to specific level 1
			releaseManager.ExpandToLevel(USER_ID_FRED_BLOGGS, projectId, 1);

			//Verify the hierarchy
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(5, releases.Count);
			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(false, releases[0].IsExpanded);
			Assert.AreEqual(true, releases[0].IsVisible);
			Assert.AreEqual(startingRelease1, releases[1].ReleaseId);
			Assert.AreEqual("AAB", releases[1].IndentLevel);
			Assert.AreEqual(false, releases[1].IsExpanded);
			Assert.AreEqual(true, releases[1].IsVisible);
			Assert.AreEqual(releaseId1, releases[2].ReleaseId);
			Assert.AreEqual("AAC", releases[2].IndentLevel);
			Assert.AreEqual(false, releases[2].IsExpanded);
			Assert.AreEqual(true, releases[2].IsVisible);
			Assert.AreEqual(startingRelease2, releases[3].ReleaseId);
			Assert.AreEqual("AAE", releases[3].IndentLevel);
			Assert.AreEqual(false, releases[3].IsExpanded);
			Assert.AreEqual(true, releases[3].IsVisible);
			Assert.AreEqual(releaseId2, releases[4].ReleaseId);
			Assert.AreEqual("AAF", releases[4].IndentLevel);
			Assert.AreEqual(false, releases[4].IsExpanded);
			Assert.AreEqual(true, releases[4].IsVisible);

			//Now test that we can expand to specific level 2
			releaseManager.ExpandToLevel(USER_ID_FRED_BLOGGS, projectId, 2);

			//Verify the hierarchy
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(9, releases.Count);
			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(true, releases[0].IsExpanded);
			Assert.AreEqual(true, releases[0].IsVisible);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(false, releases[1].IsExpanded);
			Assert.AreEqual(true, releases[1].IsVisible);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(false, releases[2].IsExpanded);
			Assert.AreEqual(true, releases[2].IsVisible);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(true, releases[3].IsExpanded);
			Assert.AreEqual(true, releases[3].IsVisible);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(false, releases[4].IsExpanded);
			Assert.AreEqual(true, releases[4].IsVisible);
			Assert.AreEqual(iterationId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAB", releases[5].IndentLevel);
			Assert.AreEqual(false, releases[5].IsExpanded);
			Assert.AreEqual(true, releases[5].IsVisible);
			Assert.AreEqual(releaseId3, releases[6].ReleaseId);
			Assert.AreEqual("AACAAC", releases[6].IndentLevel);
			Assert.AreEqual(false, releases[6].IsExpanded);
			Assert.AreEqual(true, releases[6].IsVisible);
			Assert.AreEqual(startingRelease2, releases[7].ReleaseId);
			Assert.AreEqual("AAE", releases[7].IndentLevel);
			Assert.AreEqual(false, releases[7].IsExpanded);
			Assert.AreEqual(true, releases[7].IsVisible);
			Assert.AreEqual(releaseId2, releases[8].ReleaseId);
			Assert.AreEqual("AAF", releases[8].IndentLevel);
			Assert.AreEqual(false, releases[8].IsExpanded);
			Assert.AreEqual(true, releases[8].IsVisible);

			//Now test that we can expand to show all levels
			releaseManager.ExpandToLevel(USER_ID_FRED_BLOGGS, projectId, null);

			//Verify the hierarchy
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(10, releases.Count);
			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(true, releases[0].IsExpanded);
			Assert.AreEqual(true, releases[0].IsVisible);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(false, releases[1].IsExpanded);
			Assert.AreEqual(true, releases[1].IsVisible);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(false, releases[2].IsExpanded);
			Assert.AreEqual(true, releases[2].IsVisible);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(true, releases[3].IsExpanded);
			Assert.AreEqual(true, releases[3].IsVisible);
			Assert.AreEqual(true, releases[3].IsSummary);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(true, releases[4].IsExpanded);
			Assert.AreEqual(true, releases[4].IsVisible);
			Assert.AreEqual(true, releases[4].IsSummary);
			Assert.AreEqual(iterationToDeleteId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAAAAA", releases[5].IndentLevel);
			Assert.AreEqual(false, releases[5].IsExpanded);
			Assert.AreEqual(true, releases[5].IsVisible);
			Assert.AreEqual(false, releases[5].IsSummary);
			Assert.AreEqual(iterationId1, releases[6].ReleaseId);
			Assert.AreEqual("AACAAB", releases[6].IndentLevel);
			Assert.AreEqual(false, releases[6].IsExpanded);
			Assert.AreEqual(true, releases[6].IsVisible);
			Assert.AreEqual(releaseId3, releases[7].ReleaseId);
			Assert.AreEqual("AACAAC", releases[7].IndentLevel);
			Assert.AreEqual(false, releases[7].IsExpanded);
			Assert.AreEqual(true, releases[7].IsVisible);
			Assert.AreEqual(startingRelease2, releases[8].ReleaseId);
			Assert.AreEqual("AAE", releases[8].IndentLevel);
			Assert.AreEqual(false, releases[8].IsExpanded);
			Assert.AreEqual(true, releases[8].IsVisible);
			Assert.AreEqual(releaseId2, releases[9].ReleaseId);
			Assert.AreEqual("AAF", releases[9].IndentLevel);
			Assert.AreEqual(false, releases[9].IsExpanded);
			Assert.AreEqual(true, releases[9].IsVisible);

			//Clean up by deleting the new iteration
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationToDeleteId1);
			releaseManager.DeleteFromDatabase(iterationToDeleteId1, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(180)
		]
		public void _08_RetrieveFilteredList()
		{
			//Now lets test that we can retrieve using the various filter options in the sample project
			//We need to test as both Fred Bloggs and Joe Smith to see how the user navigation changes the results
			Hashtable filters = new Hashtable();

			//Lets test that we can retrieve the count of releases in the system with a filter applied

			//Filter by Name LIKE SP1
			filters.Add("Name", "SP1");
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, releases.Count, "FredCount");
			Assert.AreEqual(4, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			releases = releaseManager.RetrieveByProjectId(USER_ID_JOE_SMITH, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(5, releases.Count, "JoeCount");
			Assert.AreEqual(5, releaseManager.Count(USER_ID_JOE_SMITH, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Version LIKE 1.1
			filters.Clear();
			filters.Add("VersionNumber", "1.1");
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, releases.Count);
			Assert.AreEqual(1, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			releases = releaseManager.RetrieveByProjectId(USER_ID_JOE_SMITH, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, releases.Count);
			Assert.AreEqual(6, releaseManager.Count(USER_ID_JOE_SMITH, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by CreationDate
			filters.Clear();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.AddDays(-110);
			dateRange.EndDate = DateTime.UtcNow.AddDays(-100);
			filters.Add("CreationDate", dateRange);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, releases.Count);
			Assert.AreEqual(0, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			releases = releaseManager.RetrieveByProjectId(USER_ID_JOE_SMITH, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, releases.Count);
			Assert.AreEqual(0, releaseManager.Count(USER_ID_JOE_SMITH, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by CreatorId = 2 (Fred Bloggs)
			filters.Clear();
			filters.Add("CreatorId", 2);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, releases.Count);
			releases = releaseManager.RetrieveByProjectId(USER_ID_JOE_SMITH, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(10, releases.Count);

			//Filter by active statuses = Y and CreatorId = 2 (Fred Bloggs)
			MultiValueFilter activeStatusFilter = new MultiValueFilter();
			activeStatusFilter.Values.Add((int)Release.ReleaseStatusEnum.Planned);
			activeStatusFilter.Values.Add((int)Release.ReleaseStatusEnum.InProgress);
			activeStatusFilter.Values.Add((int)Release.ReleaseStatusEnum.Completed);
			filters.Add("ReleaseStatusId", activeStatusFilter);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, releases.Count);
			releases = releaseManager.RetrieveByProjectId(USER_ID_JOE_SMITH, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(9, releases.Count);

			//Test that we can use multi-valued filters
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add(USER_ID_FRED_BLOGGS);
			multiValueFilter.Values.Add(USER_ID_JOE_SMITH);
			filters.Clear();
			filters.Add("CreatorId", multiValueFilter);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(14, releases.Count);
			Assert.AreEqual(14, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by ReleaseId = 2
			filters.Clear();
			filters.Add("ReleaseId", 2);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, releases.Count);
			releases = releaseManager.RetrieveByProjectId(USER_ID_JOE_SMITH, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, releases.Count);

			//Now lets try filtering on some of the custom properties
			//Filter on Operating System=Windows Vista
			filters.Clear();
			filters.Add("Custom_02", 10);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, releases.Count);
			Assert.AreEqual(1, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			Assert.AreEqual(1, releases[0].ReleaseId);
			Assert.AreEqual(10, releases[0].Custom_02.FromDatabaseSerialization_List_Int32()[0], "Custom_02");
			Assert.IsTrue(releases[0].Custom_03.IsNull());
			Assert.AreEqual("This is the first version of the system", releases[0].Custom_01.FromDatabaseSerialization_String());

			//Filter on Notes LIKE 'version'
			filters.Clear();
			filters.Add("Custom_01", "version");
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, releases.Count);
			Assert.AreEqual(1, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			Assert.AreEqual(1, releases[0].ReleaseId);
			Assert.AreEqual(10, releases[0].Custom_02.FromDatabaseSerialization_List_Int32()[0], "Custom_02");
			Assert.IsTrue(releases[0].Custom_03.IsNull());
			Assert.AreEqual("This is the first version of the system", releases[0].Custom_01.FromDatabaseSerialization_String());

			//Filter on Test States = Not Covered
			filters.Clear();
			filters.Add("CoverageId", 1);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, releases.Count, "Count");
			Assert.AreEqual(8, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
			Assert.AreEqual(11, releases[0].ReleaseId, "ReleaseId");

			//Filter on Test States = 0% Run
			filters.Clear();
			filters.Add("CoverageId", 2);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, releases.Count, "Count");
			Assert.AreEqual(6, releases[0].ReleaseId, "ReleaseId");

			//Filter on Test States > 0% Failed
			filters.Clear();
			filters.Add("CoverageId", 5);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, releases.Count, "Count");
			Assert.AreEqual(1, releases[0].ReleaseId, "ReleaseId");

			//Filter on Test States > 0% Caution
			filters.Clear();
			filters.Add("CoverageId", 8);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, releases.Count, "Count");

			//Now we need to test that we can also filter by task progress
			//Filter by Task Progress='Not Started'
			filters.Clear();
			filters.Add("ProgressId", 1);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.IsTrue(releases.Count >= 1 && releases.Count <= 2);

			//Filter by Task Progress='Starting Late'
			filters.Clear();
			filters.Add("ProgressId", 2);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, releases.Count);
			Assert.AreEqual(2, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));

			//Filter by Task Progress='on Schedule'
			filters.Clear();
			filters.Add("ProgressId", 3);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, releases.Count);

			//Filter by Task Progress='Running Late'
			filters.Clear();
			filters.Add("ProgressId", 4);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, releases.Count);

			//Filter by Task Progress='Completed'
			filters.Clear();
			filters.Add("ProgressId", 5);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, releases.Count);

			//Next lets get all releases that have certain fields set to null
			filters.Clear();
			MultiValueFilter multiFilterValue = new MultiValueFilter();
			multiFilterValue.IsNone = true;
			filters.Add("Custom_02", multiFilterValue);
			releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(13, releases.Count);
			Assert.AreEqual(13, releaseManager.Count(USER_ID_FRED_BLOGGS, PROJECT_ID, filters, InternalRoutines.UTC_OFFSET));
		}

		[
		Test,
		SpiraTestCase(767)
		]
		public void _09_PurgeReleases()
		{
			//Now that we know that the retrieves work with the deleted data, we now purge the previously deleted items
			releaseManager.DeleteFromDatabase(releaseToDeleteId1, USER_ID_FRED_BLOGGS);

			//Verify that the data returned to normal
			//The indent levels should now revert
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(9, releases.Count);

			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(iterationId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAB", releases[5].IndentLevel);
			Assert.AreEqual(releaseId3, releases[6].ReleaseId);
			Assert.AreEqual("AACAAC", releases[6].IndentLevel);
			Assert.AreEqual(startingRelease2, releases[7].ReleaseId);
			Assert.AreEqual("AAD", releases[7].IndentLevel);
			Assert.AreEqual(releaseId2, releases[8].ReleaseId);
			Assert.AreEqual("AAE", releases[8].IndentLevel);
		}

		[
		Test,
		SpiraTestCase(262)
		]
		public void _10_MoveReleases()
		{
			//First verify the current hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(9, releases.Count);

			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(true, releases[0].IsSummary);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(false, releases[1].IsSummary);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(false, releases[2].IsSummary);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(true, releases[3].IsSummary);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(false, releases[4].IsSummary);
			Assert.AreEqual(iterationId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAB", releases[5].IndentLevel);
			Assert.AreEqual(false, releases[5].IsSummary);
			Assert.AreEqual(releaseId3, releases[6].ReleaseId);
			Assert.AreEqual("AACAAC", releases[6].IndentLevel);
			Assert.AreEqual(false, releases[6].IsSummary);
			Assert.AreEqual(startingRelease2, releases[7].ReleaseId);
			Assert.AreEqual("AAD", releases[7].IndentLevel);
			Assert.AreEqual(false, releases[7].IsSummary);
			Assert.AreEqual(releaseId2, releases[8].ReleaseId);
			Assert.AreEqual("AAE", releases[8].IndentLevel);
			Assert.AreEqual(false, releases[8].IsSummary);

			//******** Moving a Single Release ***********

			//First lets try moving a single release to a position at the same level
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, releaseId3, iterationId1);
			//Verify the new positions
			ReleaseView releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAC", releaseView.IndentLevel);

			//Now move the release back
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, iterationId1, releaseId3);
			//Verify that it returned correctly
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAC", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);

			//Now lets try moving a single release to the first position in the list
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, iterationId1, releaseId5);
			//Verify the new positions
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId5);
			Assert.AreEqual("AAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AADAAB", releaseView.IndentLevel);

			//Now move the release back
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, iterationId1, releaseId3);
			//Verify the new positions
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId5);
			Assert.AreEqual("AAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAC", releaseView.IndentLevel);

			//Now lets try moving a single release to the last position in the list
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, iterationId1, null);
			//Verify the new positions
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AAF", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);

			//Now move the release back
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, iterationId1, releaseId3);
			//Verify that it returned correctly
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAC", releaseView.IndentLevel);

			//******** Moving a Nested Set of Releases ***********

			//First lets try moving a nested set of releases to a position at the same level
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, releaseId1, releaseId2);
			//Verify the new positions (including the child items)
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAE", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AAD", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AADAAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AADAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AADAAC", releaseView.IndentLevel);

			//Now move the release back
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, releaseId1, startingRelease2);
			//Verify that it returned correctly (including the child items)
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAE", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AAC", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AACAAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAC", releaseView.IndentLevel);

			//Next lets try moving a nested set of releases to the first position in the list
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, releaseId1, releaseId5);
			//Verify the new positions (including the child items)
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId5);
			Assert.AreEqual("AAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AAAAAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AAAAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AAAAAC", releaseView.IndentLevel);

			//Now move the release back
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, releaseId1, startingRelease2);
			//Verify that it returned correctly (including the child items)
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId5);
			Assert.AreEqual("AAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AAC", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AACAAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAC", releaseView.IndentLevel);

			//Next lets try moving a nested set of releases to to the last position in the list
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, releaseId1, null);
			//Verify the new positions (including the child items)
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAD", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AAE", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AAEAAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AAEAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AAEAAC", releaseView.IndentLevel);

			//Now move the release back
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, releaseId1, startingRelease2);
			//Verify that it returned correctly (including the child items)
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAE", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual("AAC", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId4);
			Assert.AreEqual("AACAAA", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			Assert.AreEqual("AACAAC", releaseView.IndentLevel);
		}

		[
		Test,
		SpiraTestCase(263)
		]
		public void _11_CopyReleases()
		{
			//First verify the current hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveByProjectId(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(9, releases.Count);

			Assert.AreEqual(releaseId5, releases[0].ReleaseId);
			Assert.AreEqual("AAA", releases[0].IndentLevel);
			Assert.AreEqual(true, releases[0].IsSummary);
			Assert.AreEqual(iterationId2, releases[1].ReleaseId);
			Assert.AreEqual("AAAAAA", releases[1].IndentLevel);
			Assert.AreEqual(false, releases[1].IsSummary);
			Assert.AreEqual(startingRelease1, releases[2].ReleaseId);
			Assert.AreEqual("AAB", releases[2].IndentLevel);
			Assert.AreEqual(false, releases[2].IsSummary);
			Assert.AreEqual(releaseId1, releases[3].ReleaseId);
			Assert.AreEqual("AAC", releases[3].IndentLevel);
			Assert.AreEqual(true, releases[3].IsSummary);
			Assert.AreEqual(releaseId4, releases[4].ReleaseId);
			Assert.AreEqual("AACAAA", releases[4].IndentLevel);
			Assert.AreEqual(false, releases[4].IsSummary);
			Assert.AreEqual(iterationId1, releases[5].ReleaseId);
			Assert.AreEqual("AACAAB", releases[5].IndentLevel);
			Assert.AreEqual(false, releases[5].IsSummary);
			Assert.AreEqual(releaseId3, releases[6].ReleaseId);
			Assert.AreEqual("AACAAC", releases[6].IndentLevel);
			Assert.AreEqual(false, releases[6].IsSummary);
			Assert.AreEqual(startingRelease2, releases[7].ReleaseId);
			Assert.AreEqual("AAD", releases[7].IndentLevel);
			Assert.AreEqual(false, releases[7].IsSummary);
			Assert.AreEqual(releaseId2, releases[8].ReleaseId);
			Assert.AreEqual("AAE", releases[8].IndentLevel);
			Assert.AreEqual(false, releases[8].IsSummary);

			//******** Copying a Single Release ***********

			//First lets try copying a single release to a position at the same level
			int copiedReleaseId = releaseManager.Copy(USER_ID_FRED_BLOGGS, projectId, projectTemplateId, releaseId3, iterationId1);

			//Verify the new positions and that the data copied correctly
			ReleaseView releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, copiedReleaseId);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);
			Assert.AreEqual("Test Release 1.1 SP1 - Copy", releaseView.Name);
			Assert.AreEqual("Test Description of Release 1.1 SP1", releaseView.Description);
			Assert.AreEqual("4.0.0.1", releaseView.VersionNumber, "VersionNumber1");
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, releaseView.ReleaseStatusId);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, releaseView.ReleaseTypeId);

			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAC", releaseView.IndentLevel);

			//Now delete the release
			releaseManager.DeleteFromDatabase(copiedReleaseId, USER_ID_FRED_BLOGGS);
			//Verify that the other releases returned correctly
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			Assert.AreEqual("AACAAB", releaseView.IndentLevel);

			//Now lets try copying a single release to the first position in the list
			copiedReleaseId = releaseManager.Copy(USER_ID_FRED_BLOGGS, projectId, projectTemplateId, releaseId3, releaseId5);
			//Verify the new positions and that the data copied correctly
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, copiedReleaseId);
			Assert.AreEqual("AAA", releaseView.IndentLevel);
			Assert.AreEqual("Test Release 1.1 SP1 - Copy", releaseView.Name);
			Assert.AreEqual("Test Description of Release 1.1 SP1", releaseView.Description);
			Assert.AreEqual("4.0.0.1", releaseView.VersionNumber, "VersionNumber1");
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, releaseView.ReleaseStatusId);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, releaseView.ReleaseTypeId);

			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId5);
			Assert.AreEqual("AAB", releaseView.IndentLevel);

			//Now delete the release
			releaseManager.DeleteFromDatabase(copiedReleaseId, USER_ID_FRED_BLOGGS);
			//Verify that the other releases returned correctly
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId5);
			Assert.AreEqual("AAA", releaseView.IndentLevel);

			//Now lets try copying a single release to the end of the list
			copiedReleaseId = releaseManager.Copy(USER_ID_FRED_BLOGGS, projectId, projectTemplateId, releaseId3, null);
			//Verify the new positions and that the data copied correctly
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, copiedReleaseId);
			Assert.AreEqual("AAF", releaseView.IndentLevel);
			Assert.AreEqual("Test Release 1.1 SP1 - Copy", releaseView.Name);
			Assert.AreEqual("Test Description of Release 1.1 SP1", releaseView.Description);
			Assert.AreEqual("4.0.0.1", releaseView.VersionNumber, "VersionNumber1");
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, releaseView.ReleaseStatusId);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, releaseView.ReleaseTypeId);

			//Now delete the release
			releaseManager.DeleteFromDatabase(copiedReleaseId, USER_ID_FRED_BLOGGS);

			//******** Copying a Nested Set of Releases ***********

			//First lets try copying a release sub-tree to a position at the same level
			//Need to expand it afterwards to verify that the children copied
			copiedReleaseId = releaseManager.Copy(USER_ID_FRED_BLOGGS, projectId, projectTemplateId, releaseId1, releaseId2);
			releaseManager.Expand(USER_ID_FRED_BLOGGS, projectId, copiedReleaseId);

			//Verify the new positions and that the data copied correctly
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, copiedReleaseId);
			Assert.AreEqual("AAE", releaseView.IndentLevel);
			Assert.AreEqual("Test Release 1.1 - Copy", releaseView.Name);
			Assert.AreEqual(true, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(true, releaseView.IsExpanded, "ExpandedYn");

			//Verify that the succeeding item was moved down one position
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAF", releaseView.IndentLevel);

			//Now verify that the child releases also copied correctly (names shouldn't change)
			releaseView = releaseManager.RetrieveByIndentLevel(USER_ID_FRED_BLOGGS, projectId, "AAEAAA")[0];
			Assert.AreEqual("AAEAAA", releaseView.IndentLevel);
			Assert.AreEqual("Test Release 1.1 SP2", releaseView.Name);
			Assert.AreEqual("4.0.0.2", releaseView.VersionNumber, "VersionNumber4");
			Assert.AreEqual(false, releaseView.IsSummary, "SummaryYn");
			Assert.AreEqual(false, releaseView.IsExpanded, "ExpandedYn");

			//Now delete the release (need to collapse first)
			releaseManager.Collapse(USER_ID_FRED_BLOGGS, projectId, copiedReleaseId);
			releaseManager.DeleteFromDatabase(copiedReleaseId, USER_ID_FRED_BLOGGS);

			//Verify that the other releases returned correctly
			releaseView = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual("AAE", releaseView.IndentLevel);
		}

		[
		Test,
		SpiraTestCase(315)
		]
		public void _12_ChangeReleaseToIteration()
		{
			//First lets verify the iteration flags of existing items
			Release release = releaseManager.RetrieveById3(projectId, releaseId3);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, release.ReleaseTypeId);
			Assert.AreEqual(false, release.IsIteration);
			Assert.AreEqual(false, release.IsIterationOrPhase);
			release = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.Iteration, release.ReleaseTypeId);
			Assert.AreEqual(true, release.IsIteration);
			Assert.AreEqual(true, release.IsIterationOrPhase);

			//First lets test that we can change an iteration into a release
			release.StartTracking();
			release.ReleaseTypeId = (int)Release.ReleaseTypeEnum.MinorRelease;
			releaseManager.Update(new List<Release>() { release }, USER_ID_FRED_BLOGGS, projectId);

			//Verify change
			release = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, release.ReleaseTypeId);
			Assert.AreEqual(false, release.IsIteration);
			Assert.AreEqual(false, release.IsIterationOrPhase);

			//Now lets test that we can turn it back into an iteration
			release.StartTracking();
			release.ReleaseTypeId = (int)Release.ReleaseTypeEnum.Iteration;
			releaseManager.Update(new List<Release>() { release }, USER_ID_FRED_BLOGGS, projectId);

			//Verify change
			release = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.Iteration, release.ReleaseTypeId);
			Assert.AreEqual(true, release.IsIteration);
			Assert.AreEqual(true, release.IsIterationOrPhase);

			//Finally lets verify that you can't turn a summary release into an iteration
			release = releaseManager.RetrieveById3(projectId, releaseId1);

			bool exceptionThrown = false;
			try
			{
				release.StartTracking();
				release.ReleaseTypeId = (int)Release.ReleaseTypeEnum.Iteration;
				releaseManager.Update(new List<Release>() { release }, USER_ID_FRED_BLOGGS, projectId);
			}
			catch (IterationSummaryException)
			{
				exceptionThrown = true;

			}
			Assert.IsTrue(exceptionThrown, "Should not be able to make a summary iteration");
		}

		[
		Test,
		SpiraTestCase(352)
		]
		public void _13_RetrieveSummaryData()
		{
			//Tests that we can retrieve the list of releases and their test execution summary
			//This is used on the home page

			//First lets test that we can retrieve the release summary for a project
			//This contains just the releases (no iterations) with an active status
			List<ReleaseView> releases = releaseManager.RetrieveTestSummary(PROJECT_ID, null);

			//Verify the data
			Assert.AreEqual(4, releases.Count, "ReleaseCount");
			Assert.AreEqual(false, releases[0].IsIteration);
			Assert.AreEqual(0, releases[0].CountNotRun, "Not run wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(4, releases[0].CountPassed, "Passed wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(2, releases[0].CountFailed, "Failed wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(0, releases[0].CountBlocked, "Blocked wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(1, releases[0].CountCaution, "Caution wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(false, releases[1].IsIteration);
			Assert.AreEqual(2, releases[1].CountNotRun, "Not run wrong for RL:" + releases[1].ReleaseId);
			Assert.AreEqual(3, releases[1].CountPassed, "Passed wrong for RL:" + releases[1].ReleaseId);
			Assert.AreEqual(2, releases[1].CountFailed, "Failed wrong for RL:" + releases[1].ReleaseId);
			Assert.AreEqual(0, releases[1].CountBlocked, "Blocked wrong for RL:" + releases[1].ReleaseId);
			Assert.AreEqual(2, releases[1].CountCaution, "Caution wrong for RL:" + releases[1].ReleaseId);

			//Now lets test that we can retrieve the release summary for a particular release
			//This contains just the selected release and its child iterations
			releases = releaseManager.RetrieveTestSummary(PROJECT_ID, 4);

			//Verify the data
			Assert.AreEqual(4, releases.Count);
			Assert.AreEqual(false, releases[0].IsIteration);
			Assert.AreEqual(2, releases[0].CountNotRun, "Not run wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(3, releases[0].CountPassed, "Passed wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(2, releases[0].CountFailed, "Failed wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(0, releases[0].CountBlocked, "Blocked wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(2, releases[0].CountCaution, "Caution wrong for RL:" + releases[0].ReleaseId);
			Assert.AreEqual(true, releases[2].IsIteration);
			Assert.AreEqual(0, releases[2].CountNotRun, "Not run wrong for RL:" + releases[1].ReleaseId);
			Assert.AreEqual(1, releases[2].CountPassed, "Passed wrong for RL:" + releases[1].ReleaseId);
			Assert.AreEqual(0, releases[2].CountFailed, "Failed wrong for RL:" + releases[1].ReleaseId);
			Assert.AreEqual(1, releases[2].CountBlocked, "Blocked wrong for RL:" + releases[1].ReleaseId);
			Assert.AreEqual(1, releases[2].CountCaution, "Caution wrong for RL:" + releases[1].ReleaseId);
		}

		[
		Test,
		SpiraTestCase(356)
		]
		public void _14_ExportReleases()
		{
			//First we need to create a new project that we want to copy to
			ProjectManager projectManager = new ProjectManager();
			TemplateManager templateManager = new TemplateManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			int destProjectId = projectManager.Insert("Test Export Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			int destProjectTemplateId = templateManager.RetrieveForProject(destProjectId).ProjectTemplateId;

			//Now export a whole tree of releases over to this new project
			int exportedReleaseId1 = releaseManager.Export(USER_ID_FRED_BLOGGS, projectId, releaseId1, destProjectId);

			//Now verify that they exported correctly

			//First the summary item containing the other releases and iterations
			ReleaseView releaseView = releaseManager.RetrieveById2(destProjectId, exportedReleaseId1);
			Assert.AreEqual("Test Release 1.1", releaseView.Name);
			Assert.AreEqual("1.1.0.8", releaseView.VersionNumber);
			Assert.AreEqual(true, releaseView.IsSummary);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MajorRelease, releaseView.ReleaseTypeId);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, releaseView.ReleaseStatusId);

			//Next verify that the children exported correctly
			List<ReleaseView> childReleases = releaseManager.RetrieveChildren(USER_ID_FRED_BLOGGS, destProjectId, releaseView.IndentLevel, true);
			Assert.AreEqual(3, childReleases.Count);
			//First Child Release
			Assert.AreEqual("Test Release 1.1 SP2", childReleases[0].Name);
			Assert.AreEqual("1.1.2.0", childReleases[0].VersionNumber);
			Assert.AreEqual(false, childReleases[0].IsSummary);
			Assert.AreEqual(false, childReleases[0].IsIteration);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MinorRelease, childReleases[0].ReleaseTypeId);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, childReleases[0].ReleaseStatusId);
			//First Iteration
			Assert.AreEqual("Test Sprint 001", childReleases[1].Name);
			Assert.AreEqual("1.0.0.0.0000", childReleases[1].VersionNumber);
			Assert.AreEqual(false, childReleases[1].IsSummary);
			Assert.AreEqual(true, childReleases[1].IsIteration);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.Iteration, childReleases[1].ReleaseTypeId);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, childReleases[1].ReleaseStatusId);

			//Now try exporting the same release to the destination project and verify that it was issued with a new version number
			int exportedReleaseId2 = releaseManager.Export(USER_ID_FRED_BLOGGS, projectId, releaseId1, destProjectId);
			releaseView = releaseManager.RetrieveById2(destProjectId, exportedReleaseId2);
			Assert.AreEqual("Test Release 1.1", releaseView.Name);
			Assert.AreEqual("1.1.2.1", releaseView.VersionNumber);
			Assert.AreEqual(true, releaseView.IsSummary);
			Assert.AreEqual((int)Release.ReleaseTypeEnum.MajorRelease, releaseView.ReleaseTypeId);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, releaseView.ReleaseStatusId);

			//Finally delete the project to clean-up
			projectManager.Delete(USER_ID_FRED_BLOGGS, destProjectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, destProjectTemplateId);
		}

		[
		Test,
		SpiraTestCase(382)
		]
		public void _15_TaskProgressAndEffortRollups()
		{
			//First create a release/iteration/phase hierarchy
			//Need to have major/minor releases as well
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int releaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.1", "", "1.1", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MinorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int releaseId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.2", "", "1.2", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int iterationId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.1.0001", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2", "", "1.1.0002", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int phaseId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 1", "", "1.2 SIT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int phaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 2", "", "1.2 UAT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int iterationId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0001", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId4 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0002", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);

			//Verify the hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveSelfAndChildren(projectId, releaseId1, true, true);
			Assert.AreEqual(9, releases.Count);
			Assert.AreEqual(releaseId1, releases[0].ReleaseId);
			Assert.AreEqual(releaseId2, releases[1].ReleaseId);
			Assert.AreEqual(iterationId1, releases[2].ReleaseId);
			Assert.AreEqual(iterationId2, releases[3].ReleaseId);
			Assert.AreEqual(releaseId3, releases[4].ReleaseId);
			Assert.AreEqual(phaseId1, releases[5].ReleaseId);
			Assert.AreEqual(phaseId2, releases[6].ReleaseId);
			Assert.AreEqual(iterationId3, releases[7].ReleaseId);
			Assert.AreEqual(iterationId4, releases[8].ReleaseId);

			//Verify the overall progress, planned effort and available effort values
			//Release 1
			Release release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(60480, release.AvailableEffort);
			//Release 2
			release = releaseManager.RetrieveById3(projectId, releaseId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(20160, release.PlannedEffort);
			Assert.AreEqual(20160, release.AvailableEffort);
			//Iteration 1
			release = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Iteration 2
			release = releaseManager.RetrieveById3(projectId, iterationId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			//Release 3
			release = releaseManager.RetrieveById3(projectId, releaseId3);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(20160, release.PlannedEffort);
			Assert.AreEqual(20160, release.AvailableEffort);
			//Phase 1
			release = releaseManager.RetrieveById3(projectId, phaseId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Phase 2
			release = releaseManager.RetrieveById3(projectId, phaseId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			//Iteration 3
			release = releaseManager.RetrieveById3(projectId, iterationId3);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Iteration 4
			release = releaseManager.RetrieveById3(projectId, iterationId4);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);

			//Verify that editing the date and # non-working days changes the effort for an iteration
			release = releaseManager.RetrieveById3(projectId, iterationId2);
			release.StartTracking();
			release.StartDate = DateTime.Parse("4/16/2004");
			release.EndDate = DateTime.Parse("4/29/2004");
			release.DaysNonWorking = 2;
			releaseManager.Update(new List<Release>() { release }, USER_ID_FRED_BLOGGS, projectId);

			//Verify the changes
			release = releaseManager.RetrieveById3(projectId, iterationId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(8640, release.PlannedEffort);
			Assert.AreEqual(8640, release.AvailableEffort);

			//Now we need to attach some tasks to both the releases themself and their iterations/phases
			TaskManager taskManager = new TaskManager();
			//Releases
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, releaseId1, null, null, "Task 1", "", DateTime.Parse("4/1/2004"), DateTime.Parse("4/10/2004"), 2400, null, null, USER_ID_FRED_BLOGGS);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, releaseId2, null, null, "Task 2", "", DateTime.Parse("4/1/2004"), DateTime.Parse("4/10/2004"), 2400, null, null, USER_ID_FRED_BLOGGS);
			int taskId3 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, releaseId3, null, null, "Task 3", "", DateTime.Parse("4/1/2004"), DateTime.Parse("4/10/2004"), 2400, null, null, USER_ID_FRED_BLOGGS);
			//Iterations
			int taskId4 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, iterationId1, null, null, "Task 4", "", DateTime.Parse("4/1/2004"), DateTime.Parse("4/10/2004"), 4800, null, null, USER_ID_FRED_BLOGGS);
			int taskId5 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, iterationId1, null, null, "Task 5", "", DateTime.Parse("4/8/2004"), DateTime.Parse("4/15/2004"), 2400, null, null, USER_ID_FRED_BLOGGS);
			int taskId6 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, iterationId2, null, null, "Task 6", "", DateTime.Parse("4/16/2004"), DateTime.Parse("4/20/2004"), 1920, null, null, USER_ID_FRED_BLOGGS);
			int taskId7 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, iterationId2, null, null, "Task 7", "", DateTime.Parse("4/21/2004"), DateTime.Parse("4/29/2004"), 3840, null, null, USER_ID_FRED_BLOGGS);
			int taskId8 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, iterationId3, null, null, "Task 8", "", DateTime.Parse("4/1/2004"), DateTime.Parse("4/10/2004"), 4800, null, null, USER_ID_FRED_BLOGGS);
			int taskId9 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, iterationId3, null, null, "Task 9", "", DateTime.Parse("4/8/2004"), DateTime.Parse("4/15/2004"), 2400, null, null, USER_ID_FRED_BLOGGS);
			int taskId10 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, iterationId4, null, null, "Task 10", "", DateTime.Parse("4/16/2004"), DateTime.Parse("4/20/2004"), 1920, null, null, USER_ID_FRED_BLOGGS);
			int taskId11 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, iterationId4, null, null, "Task 11", "", DateTime.Parse("4/21/2004"), DateTime.Parse("4/29/2004"), 3840, null, null, USER_ID_FRED_BLOGGS);
			//Phases
			int taskId12 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, phaseId1, null, null, "Task 12", "", DateTime.Parse("4/1/2004"), DateTime.Parse("4/10/2004"), 4800, null, null, USER_ID_FRED_BLOGGS);
			int taskId13 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, phaseId1, null, null, "Task 13", "", DateTime.Parse("4/8/2004"), DateTime.Parse("4/15/2004"), 2400, null, null, USER_ID_FRED_BLOGGS);
			int taskId14 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, phaseId2, null, null, "Task 14", "", DateTime.Parse("4/16/2004"), DateTime.Parse("4/20/2004"), 1920, null, null, USER_ID_FRED_BLOGGS);
			int taskId15 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, phaseId2, null, null, "Task 15", "", DateTime.Parse("4/21/2004"), DateTime.Parse("4/29/2004"), 3840, null, null, USER_ID_FRED_BLOGGS);


			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(30720, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(30720, release.TaskRemainingEffort);
			Assert.AreEqual(30720, release.TaskProjectedEffort);
			Assert.AreEqual(29760, release.AvailableEffort);
			Assert.AreEqual(10, release.TaskCount);
			Assert.AreEqual(0, release.TaskPercentOnTime);
			Assert.AreEqual(100, release.TaskPercentLateStart);
			Assert.AreEqual(0, release.TaskPercentLateFinish);
			Assert.AreEqual(0, release.TaskPercentNotStart);

			//Now lets make one of the tasks finish late
			Task task = taskManager.RetrieveById(taskId2);
			task.StartTracking();
			task.RemainingEffort = 1200;
			task.ActualEffort = 4950;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(30720, release.TaskEstimatedEffort);
			Assert.AreEqual(4950, release.TaskActualEffort);
			Assert.AreEqual(29520, release.TaskRemainingEffort);
			Assert.AreEqual(34470, release.TaskProjectedEffort);
			Assert.AreEqual(26010, release.AvailableEffort);
			Assert.AreEqual(10, release.TaskCount);
			Assert.AreEqual(0, release.TaskPercentOnTime);
			Assert.AreEqual(90, release.TaskPercentLateStart);
			Assert.AreEqual(5, release.TaskPercentLateFinish);
			Assert.AreEqual(5, release.TaskPercentNotStart);

			//Now delete one task
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(28320, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(28320, release.TaskRemainingEffort);
			Assert.AreEqual(28320, release.TaskProjectedEffort);
			Assert.AreEqual(32160, release.AvailableEffort);
			Assert.AreEqual(9, release.TaskCount);
			Assert.AreEqual(0, release.TaskPercentOnTime);
			Assert.AreEqual(100, release.TaskPercentLateStart);
			Assert.AreEqual(0, release.TaskPercentLateFinish);
			Assert.AreEqual(0, release.TaskPercentNotStart);

			//Now deassociate one task
			taskManager.RemoveReleaseAssociation(new List<int> { taskId4 }, USER_ID_FRED_BLOGGS);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(23520, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(23520, release.TaskRemainingEffort);
			Assert.AreEqual(23520, release.TaskProjectedEffort);
			Assert.AreEqual(36960, release.AvailableEffort);
			Assert.AreEqual(8, release.TaskCount);
			Assert.AreEqual(0, release.TaskPercentOnTime);
			Assert.AreEqual(100, release.TaskPercentLateStart);
			Assert.AreEqual(0, release.TaskPercentLateFinish);
			Assert.AreEqual(0, release.TaskPercentNotStart);

			//Now outdent one of the iterations, should not change the values because still under the same major release
			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(23520, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(23520, release.TaskRemainingEffort);
			Assert.AreEqual(23520, release.TaskProjectedEffort);
			Assert.AreEqual(36960, release.AvailableEffort);
			Assert.AreEqual(8, release.TaskCount);
			Assert.AreEqual(0, release.TaskPercentOnTime);
			Assert.AreEqual(100, release.TaskPercentLateStart);
			Assert.AreEqual(0, release.TaskPercentLateFinish);
			Assert.AreEqual(0, release.TaskPercentNotStart);

			//Now outdent it a second time now it won't roll up
			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Verify the data
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(17760, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(17760, release.TaskRemainingEffort);
			Assert.AreEqual(17760, release.TaskProjectedEffort);
			Assert.AreEqual(42720, release.AvailableEffort);
			Assert.AreEqual(6, release.TaskCount);
			Assert.AreEqual(0, release.TaskPercentOnTime);
			Assert.AreEqual(100, release.TaskPercentLateStart);
			Assert.AreEqual(0, release.TaskPercentLateFinish);
			Assert.AreEqual(0, release.TaskPercentNotStart);

			//Now re-indent the iteration and verify that it returns back OK
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(23520, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(23520, release.TaskRemainingEffort);
			Assert.AreEqual(23520, release.TaskProjectedEffort);
			Assert.AreEqual(36960, release.AvailableEffort);
			Assert.AreEqual(8, release.TaskCount);
			Assert.AreEqual(0, release.TaskPercentOnTime);
			Assert.AreEqual(100, release.TaskPercentLateStart);
			Assert.AreEqual(0, release.TaskPercentLateFinish);
			Assert.AreEqual(0, release.TaskPercentNotStart);

			//Finally clean up
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId3, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId4, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId5, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId6, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId7, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId8, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId9, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId10, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId11, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId12, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId13, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId14, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId15, USER_ID_FRED_BLOGGS);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId4);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
		}

		/// <summary>
		/// Tests that the working days calculation works as expected
		/// </summary>
		[
		Test,
		SpiraTestCase(438)
		]
		public void _16_WorkingDaysCalculations()
		{
			//First lets try a 5-day week (Mon-Fri only)
			long days = ReleaseManager.WorkingDays(DateTime.Parse("3/2/2009"), DateTime.Parse("3/6/2009"), 5);
			Assert.AreEqual((long)5, days);

			//Now lets try a single day
			days = ReleaseManager.WorkingDays(DateTime.Parse("3/2/2009"), DateTime.Parse("3/2/2009"), 5);
			Assert.AreEqual((long)1, days);

			//Now lets try two working weeks with a weekend in the middle
			days = ReleaseManager.WorkingDays(DateTime.Parse("3/2/2009"), DateTime.Parse("3/13/2009"), 5);
			Assert.AreEqual((long)10, days);

			//Now the same weeks, but including the second weekend days
			days = ReleaseManager.WorkingDays(DateTime.Parse("3/2/2009"), DateTime.Parse("3/15/2009"), 5);
			Assert.AreEqual((long)10, days);

			//First lets try a 6-day week (Mon-Sat only)
			days = ReleaseManager.WorkingDays(DateTime.Parse("3/2/2009"), DateTime.Parse("3/7/2009"), 6);
			Assert.AreEqual((long)6, days);

			//Now lets try two working weeks with a weekend in the middle
			days = ReleaseManager.WorkingDays(DateTime.Parse("3/2/2009"), DateTime.Parse("3/14/2009"), 6);
			Assert.AreEqual((long)12, days);

			//Now the same weeks, but including the second weekend days
			days = ReleaseManager.WorkingDays(DateTime.Parse("3/2/2009"), DateTime.Parse("3/15/2009"), 6);
			Assert.AreEqual((long)12, days);
		}

		/// <summary>
		/// Tests that changes to the effort values for an incident change the release effort values
		/// </summary>
		[
		Test,
		SpiraTestCase(439)
		]
		public void _18_IncidentEffortRollups()
		{
			//First create a release/iteration/phase hierarchy
			//Need to have major/minor releases as well
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int releaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.1", "", "1.1", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MinorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int releaseId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.2", "", "1.2", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int iterationId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.1.0001", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2", "", "1.1.0002", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int phaseId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 1", "", "1.2 SIT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int phaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 2", "", "1.2 UAT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int iterationId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0001", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId4 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0002", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);

			//Verify the hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveSelfAndChildren(projectId, releaseId1, true, true);
			Assert.AreEqual(9, releases.Count);
			Assert.AreEqual(releaseId1, releases[0].ReleaseId);
			Assert.AreEqual(releaseId2, releases[1].ReleaseId);
			Assert.AreEqual(iterationId1, releases[2].ReleaseId);
			Assert.AreEqual(iterationId2, releases[3].ReleaseId);
			Assert.AreEqual(releaseId3, releases[4].ReleaseId);
			Assert.AreEqual(phaseId1, releases[5].ReleaseId);
			Assert.AreEqual(phaseId2, releases[6].ReleaseId);
			Assert.AreEqual(iterationId3, releases[7].ReleaseId);
			Assert.AreEqual(iterationId4, releases[8].ReleaseId);

			//Verify the overall progress, planned effort and available effort values
			//Release 1
			Release release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(60480, release.AvailableEffort);
			//Release 2
			release = releaseManager.RetrieveById3(projectId, releaseId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(20160, release.PlannedEffort);
			Assert.AreEqual(20160, release.AvailableEffort);
			//Iteration 1
			release = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Iteration 2
			release = releaseManager.RetrieveById3(projectId, iterationId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			//Release 3
			release = releaseManager.RetrieveById3(projectId, releaseId3);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(20160, release.PlannedEffort);
			Assert.AreEqual(20160, release.AvailableEffort);
			//Phase 1
			release = releaseManager.RetrieveById3(projectId, phaseId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Phase 2
			release = releaseManager.RetrieveById3(projectId, phaseId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			//Iteration 3
			release = releaseManager.RetrieveById3(projectId, iterationId3);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Iteration 4
			release = releaseManager.RetrieveById3(projectId, iterationId4);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);

			//Now we need to associate some incidents to both the release itself and its iterations
			IncidentManager incidentManager = new IncidentManager();
			//Releases
			int incidentId1 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 1", "Really bad incident", releaseId1, releaseId1, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 2400, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId2 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 2", "Really bad incident", releaseId2, releaseId2, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 2400, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId3 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 3", "Really bad incident", releaseId3, releaseId3, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 2400, null, null, null, null, USER_ID_FRED_BLOGGS);
			//Iterations
			int incidentId4 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 4", "Really bad incident", iterationId1, iterationId1, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 4800, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId5 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 5", "Really bad incident", iterationId1, iterationId1, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 2400, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId6 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 6", "Really bad incident", iterationId2, iterationId2, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 1920, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId7 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 7", "Really bad incident", iterationId2, iterationId2, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 3840, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId8 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 8", "Really bad incident", iterationId3, iterationId3, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 4800, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId9 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 9", "Really bad incident", iterationId3, iterationId3, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 2400, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId10 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 10", "Really bad incident", iterationId4, iterationId4, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 1920, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId11 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 11", "Really bad incident", iterationId4, iterationId4, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 3840, null, null, null, null, USER_ID_FRED_BLOGGS);
			//Phases
			int incidentId12 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 12", "Really bad incident", phaseId1, phaseId1, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 4800, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId13 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 13", "Really bad incident", phaseId1, phaseId1, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 2400, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId14 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 14", "Really bad incident", phaseId2, phaseId2, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 1920, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId15 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 15", "Really bad incident", phaseId2, phaseId2, null, null, null, DateTime.Parse("4/1/2004"), DateTime.Parse("4/1/2004"), null, 3840, null, null, null, null, USER_ID_FRED_BLOGGS);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(30720, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(30720, release.TaskRemainingEffort);
			Assert.AreEqual(30720, release.TaskProjectedEffort);
			Assert.AreEqual(29760, release.AvailableEffort);

			//Set the actual and remaining effort for an incident
			Incident incident = incidentManager.RetrieveById(incidentId2, false);
			incident.StartTracking();
			incident.ActualEffort = 4950;
			incident.RemainingEffort = 1200;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(30720, release.TaskEstimatedEffort);
			Assert.AreEqual(4950, release.TaskActualEffort);
			Assert.AreEqual(29520, release.TaskRemainingEffort);
			Assert.AreEqual(34470, release.TaskProjectedEffort);
			Assert.AreEqual(26010, release.AvailableEffort);

			//Now delete an incident
			incidentManager.MarkAsDeleted(projectId, incidentId2, USER_ID_FRED_BLOGGS);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(28320, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(28320, release.TaskRemainingEffort);
			Assert.AreEqual(28320, release.TaskProjectedEffort);
			Assert.AreEqual(32160, release.AvailableEffort);

			//Now deassociate an incident from an iteration
			incidentManager.RemoveReleaseAssociation(new List<int> { incidentId4 }, USER_ID_FRED_BLOGGS);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(23520, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(23520, release.TaskRemainingEffort);
			Assert.AreEqual(23520, release.TaskProjectedEffort);
			Assert.AreEqual(36960, release.AvailableEffort);

			//Now outdent one of the iterations, should not change the values because still under the same major release
			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(23520, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(23520, release.TaskRemainingEffort);
			Assert.AreEqual(23520, release.TaskProjectedEffort);
			Assert.AreEqual(36960, release.AvailableEffort);

			//Now outdent it a second time now it won't roll up
			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Verify the data
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(17760, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(17760, release.TaskRemainingEffort);
			Assert.AreEqual(17760, release.TaskProjectedEffort);
			Assert.AreEqual(42720, release.AvailableEffort);

			//Now re-indent the iteration and verify that it returns back OK
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Now we need to verify that the values all rolled-up correctly
			release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(23520, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(23520, release.TaskRemainingEffort);
			Assert.AreEqual(23520, release.TaskProjectedEffort);
			Assert.AreEqual(36960, release.AvailableEffort);

			//Finally clean up
			incidentManager.MarkAsDeleted(projectId, incidentId1, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId3, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId4, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId5, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId6, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId7, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId8, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId9, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId10, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId11, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId12, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId13, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId14, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId15, USER_ID_FRED_BLOGGS);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId4);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
		}

		/// <summary>
		/// Tests that changes to the effort values for an requirement change the release effort values
		/// Also since v6.5 checks that changes to the requirements update the requirement count, total points, and % complete
		/// </summary>
		[
		Test,
		SpiraTestCase(1280)
		]
		public void _19_RequirementEffortRollups()
		{
			//First create a release/iteration/phase hierarchy
			//Need to have major/minor releases as well
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int releaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.1", "", "1.1", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MinorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int releaseId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.2", "", "1.2", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int iterationId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.1.0001", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2", "", "1.1.0002", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int phaseId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 1", "", "1.2 SIT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int phaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 2", "", "1.2 UAT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int iterationId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0001", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId4 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0002", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);

			//Verify the hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveSelfAndChildren(projectId, releaseId1, true, true);
			Assert.AreEqual(9, releases.Count);
			Assert.AreEqual(releaseId1, releases[0].ReleaseId);
			Assert.AreEqual(releaseId2, releases[1].ReleaseId);
			Assert.AreEqual(iterationId1, releases[2].ReleaseId);
			Assert.AreEqual(iterationId2, releases[3].ReleaseId);
			Assert.AreEqual(releaseId3, releases[4].ReleaseId);
			Assert.AreEqual(phaseId1, releases[5].ReleaseId);
			Assert.AreEqual(phaseId2, releases[6].ReleaseId);
			Assert.AreEqual(iterationId3, releases[7].ReleaseId);
			Assert.AreEqual(iterationId4, releases[8].ReleaseId);

			//Verify the overall progress, planned effort and available effort values
			//Also verify the requirement count, % complete and estimate point totals
			//Release 1
			Release release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(60480, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);
			//Release 2
			release = releaseManager.RetrieveById3(projectId, releaseId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(20160, release.PlannedEffort);
			Assert.AreEqual(20160, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);
			//Iteration 1
			release = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);
			//Iteration 2
			release = releaseManager.RetrieveById3(projectId, iterationId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);
			//Release 3
			release = releaseManager.RetrieveById3(projectId, releaseId3);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(20160, release.PlannedEffort);
			Assert.AreEqual(20160, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);
			//Phase 1
			release = releaseManager.RetrieveById3(projectId, phaseId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);
			//Phase 2
			release = releaseManager.RetrieveById3(projectId, phaseId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);
			//Iteration 3
			release = releaseManager.RetrieveById3(projectId, iterationId3);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);
			//Iteration 4
			release = releaseManager.RetrieveById3(projectId, iterationId4);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			Assert.AreEqual(0, release.RequirementCount);
			Assert.AreEqual(null, release.RequirementPoints);
			Assert.AreEqual(0, release.PercentComplete);

			//Now we need to associate some requirements (that have no tasks) to both the release itself and its iterations
			RequirementManager requirementManager = new RequirementManager();
			//Releases
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 1", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId2, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 2", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId3, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 3", null, 1.0M, USER_ID_FRED_BLOGGS);
			//Iterations
			int requirementId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 4", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId5 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 5", null, 2.0M, USER_ID_FRED_BLOGGS);
			int requirementId6 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId2, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 6", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId7 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId2, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 7", null, 2.0M, USER_ID_FRED_BLOGGS);
			int requirementId8 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId3, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 8", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId9 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId3, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 9", null, 2.0M, USER_ID_FRED_BLOGGS);
			int requirementId10 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId4, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 10", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId11 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId4, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 11", null, 2.0M, USER_ID_FRED_BLOGGS);
			//Phases
			int requirementId12 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, phaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 12", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId13 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, phaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 13", null, 1.5M, USER_ID_FRED_BLOGGS);
			int requirementId14 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, phaseId2, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 14", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId15 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, phaseId2, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 15", null, 1.5M, USER_ID_FRED_BLOGGS);

			//Now we need to verify that the values all rolled-up correctly
			int expectedRequirementEffort = ProjectManager.DEFAULT_POINT_EFFORT * 14;
			ReleaseView releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedRequirementEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskRemainingEffort);
			Assert.AreEqual(10, releaseView.RequirementCount);
			Assert.AreEqual(14M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);

			//Now we need to create a new requirement, not assigned to a release and assign it after the fact
			int requirementId16 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 5", null, 1.0M, USER_ID_FRED_BLOGGS);
			requirementManager.AssociateToIteration(new List<int> { requirementId16 }, iterationId1, USER_ID_FRED_BLOGGS);

			//Verify the effort and counts updated
			expectedRequirementEffort = ProjectManager.DEFAULT_POINT_EFFORT * 15;
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedRequirementEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskRemainingEffort);
			Assert.AreEqual(11, releaseView.RequirementCount);
			Assert.AreEqual(15M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);

			//Now do the same thing, but use the Update() command instead of the AssociateToIteration() one
			int requirementId17 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 5", null, 1.0M, USER_ID_FRED_BLOGGS);
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId17);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ReleaseId = iterationId2;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify the effort updated
			expectedRequirementEffort = ProjectManager.DEFAULT_POINT_EFFORT * 16;
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedRequirementEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskRemainingEffort);
			Assert.AreEqual(12, releaseView.RequirementCount);
			Assert.AreEqual(16M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);

			//Now remove one of the requirements using the RemoveAssociation() function
			requirementManager.RemoveReleaseAssociation(USER_ID_FRED_BLOGGS, new List<int> { requirementId16 });

			//Verify the effort updated
			expectedRequirementEffort = ProjectManager.DEFAULT_POINT_EFFORT * 15;
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedRequirementEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskRemainingEffort);
			Assert.AreEqual(11, releaseView.RequirementCount);
			Assert.AreEqual(15M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);

			//Now remove one of the requirements using the Update() function
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId17);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ReleaseId = null;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify the effort updated
			expectedRequirementEffort = ProjectManager.DEFAULT_POINT_EFFORT * 14;
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedRequirementEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskRemainingEffort);
			Assert.AreEqual(10, releaseView.RequirementCount);
			Assert.AreEqual(14M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);

			//Next we want to verify that changing the points estimate updates the release points count and task effort
			requirement = requirementManager.RetrieveById3(projectId, requirementId1);
			requirement.StartTracking();
			requirement.EstimatePoints = 2.5M;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify the effort/progress/counts updated
			expectedRequirementEffort = (int)((decimal)(ProjectManager.DEFAULT_POINT_EFFORT) * 15.5M);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedRequirementEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskRemainingEffort);
			Assert.AreEqual(10, releaseView.RequirementCount);
			Assert.AreEqual(15.5M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);

			//Next we want to verify that changing the status of the requirement, updates the % complete of the release
			requirement = requirementManager.RetrieveById3(projectId, requirementId1);
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Completed;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify the effort/progress/counts updated
			expectedRequirementEffort = (int)((decimal)(ProjectManager.DEFAULT_POINT_EFFORT) * 15.5M);
			int remainingRequirementEffort = (int)((decimal)(ProjectManager.DEFAULT_POINT_EFFORT) * (15.5M - 2.5M));
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedRequirementEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(remainingRequirementEffort, releaseView.TaskRemainingEffort);
			Assert.AreEqual(10, releaseView.RequirementCount);
			Assert.AreEqual(15.5M, releaseView.RequirementPoints);
			Assert.AreEqual(10, releaseView.PercentComplete);

			//Change it back and verify it updates back
			requirement = requirementManager.RetrieveById3(projectId, requirementId1);
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Planned;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });

			//Verify the effort/progress/counts updated
			expectedRequirementEffort = (int)((decimal)(ProjectManager.DEFAULT_POINT_EFFORT) * 15.5M);
			remainingRequirementEffort = expectedRequirementEffort;
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedRequirementEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedRequirementEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(remainingRequirementEffort, releaseView.TaskRemainingEffort);
			Assert.AreEqual(10, releaseView.RequirementCount);
			Assert.AreEqual(15.5M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);

			//Finally clean up
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId3);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId4);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId5);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId6);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId7);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId8);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId9);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId10);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId11);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId12);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId13);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId14);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId15);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId4);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
		}

		/// <summary>
		/// Tests that changes to the effort values for an test case change the release effort values
		/// </summary>
		[
		Test,
		SpiraTestCase(1281)
		]
		public void _20_TestCaseEffortRollups()
		{
			//First create a release/iteration/phase hierarchy
			//Need to have major/minor releases as well
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int releaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.1", "", "1.1", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MinorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int releaseId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.2", "", "1.2", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int iterationId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.1.0001", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2", "", "1.1.0002", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int phaseId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 1", "", "1.2 SIT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int phaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 2", "", "1.2 UAT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int iterationId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0001", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId4 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0002", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);

			//Verify the hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveSelfAndChildren(projectId, releaseId1, true, true);
			Assert.AreEqual(9, releases.Count);
			Assert.AreEqual(releaseId1, releases[0].ReleaseId);
			Assert.AreEqual(releaseId2, releases[1].ReleaseId);
			Assert.AreEqual(iterationId1, releases[2].ReleaseId);
			Assert.AreEqual(iterationId2, releases[3].ReleaseId);
			Assert.AreEqual(releaseId3, releases[4].ReleaseId);
			Assert.AreEqual(phaseId1, releases[5].ReleaseId);
			Assert.AreEqual(phaseId2, releases[6].ReleaseId);
			Assert.AreEqual(iterationId3, releases[7].ReleaseId);
			Assert.AreEqual(iterationId4, releases[8].ReleaseId);

			//Verify the overall progress, planned effort and available effort values
			//Release 1
			Release release = releaseManager.RetrieveById3(projectId, releaseId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(60480, release.PlannedEffort);
			Assert.AreEqual(60480, release.AvailableEffort);
			//Release 2
			release = releaseManager.RetrieveById3(projectId, releaseId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(20160, release.PlannedEffort);
			Assert.AreEqual(20160, release.AvailableEffort);
			//Iteration 1
			release = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Iteration 2
			release = releaseManager.RetrieveById3(projectId, iterationId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			//Release 3
			release = releaseManager.RetrieveById3(projectId, releaseId3);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(20160, release.PlannedEffort);
			Assert.AreEqual(20160, release.AvailableEffort);
			//Phase 1
			release = releaseManager.RetrieveById3(projectId, phaseId1);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Phase 2
			release = releaseManager.RetrieveById3(projectId, phaseId2);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);
			//Iteration 3
			release = releaseManager.RetrieveById3(projectId, iterationId3);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(9600, release.PlannedEffort);
			Assert.AreEqual(9600, release.AvailableEffort);
			//Iteration 4
			release = releaseManager.RetrieveById3(projectId, iterationId4);
			Assert.AreEqual(0, release.TaskCount);
			Assert.AreEqual(10560, release.PlannedEffort);
			Assert.AreEqual(10560, release.AvailableEffort);

			//We need to enable the inclusion of testing effort in the calculations
			ProjectManager projectManager = new ProjectManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			Project project = projectManager.RetrieveById(projectId);
			Assert.IsFalse(project.IsEffortTestCases);
			project.StartTracking();
			project.IsEffortTestCases = true;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");
			project = projectManager.RetrieveById(projectId);
			Assert.IsTrue(project.IsEffortTestCases);

			//Now we need to associate some test cases to both the release itself and its iterations
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId3 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId4 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId5 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId6 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);

			//Add to the release/iterations
			testCaseManager.AddToRelease(projectId, releaseId1, new List<int>() { testCaseId1, testCaseId2, testCaseId3, testCaseId4 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, releaseId2, new List<int>() { testCaseId1, testCaseId2, testCaseId3, testCaseId4, testCaseId5 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, releaseId3, new List<int>() { testCaseId1, testCaseId2, testCaseId3, testCaseId4, testCaseId6 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, iterationId1, new List<int>() { testCaseId1, testCaseId2 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, iterationId2, new List<int>() { testCaseId3, testCaseId4 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, iterationId3, new List<int>() { testCaseId1, testCaseId2 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, iterationId4, new List<int>() { testCaseId3, testCaseId4, testCaseId5 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, phaseId1, new List<int>() { testCaseId1, testCaseId2 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, phaseId2, new List<int>() { testCaseId1, testCaseId2, testCaseId6 }, USER_ID_SYS_ADMIN);

			//Now we need to verify that the values all rolled-up correctly
			int expectedTestCaseEffort = 480 * 18;
			ReleaseView releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedTestCaseEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedTestCaseEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedTestCaseEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(expectedTestCaseEffort, releaseView.TaskRemainingEffort);

			//Now remove one of the test cases
			testCaseManager.RemoveFromRelease(projectId, releaseId1, new List<int>() { testCaseId3 }, USER_ID_SYS_ADMIN);
			testCaseManager.RemoveFromRelease(projectId, iterationId2, new List<int>() { testCaseId3 }, USER_ID_SYS_ADMIN);

			//Now we need to verify that the values all rolled-up correctly
			expectedTestCaseEffort = 480 * 16;
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(expectedTestCaseEffort, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(expectedTestCaseEffort, releaseView.TaskProjectedEffort);
			Assert.AreEqual(60480 - expectedTestCaseEffort, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(expectedTestCaseEffort, releaseView.TaskRemainingEffort);

			//Now, we need to disable the inclusion of testing effort in the calculations
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.IsEffortTestCases = false;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");
			project = projectManager.RetrieveById(projectId);
			Assert.IsFalse(project.IsEffortTestCases);

			//Verify that the effort is no longer taken into account
			releaseManager.RefreshProgressEffortTestStatus(projectId, releaseId1);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(0, releaseView.TaskCount);
			Assert.AreEqual(60480, releaseView.PlannedEffort);
			Assert.AreEqual(60480, releaseView.AvailableEffort);
			Assert.AreEqual(0, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(0, releaseView.TaskActualEffort);
			Assert.AreEqual(0, releaseView.TaskProjectedEffort);
			Assert.AreEqual(0, releaseView.TaskRemainingEffort);

			//Finally clean up
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId2);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId3);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId4);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId5);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId6);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId4);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
		}

		/// <summary>
		/// Verify that multiple, concurrent updates to releases are handled correctly
		/// in accordance with optimistic concurrency rules
		/// </summary>
		[
		Test,
		SpiraTestCase(671)
		]
		public void _17_Concurrency_Handling()
		{
			//First we need to create a new release to verify the handling
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);

			//Now retrieve the release back and keep a copy of the entity
			ReleaseView releaseView = releaseManager.RetrieveById2(projectId, releaseId);
			Release release1 = releaseView.ConvertTo<ReleaseView, Release>();
			Release release2 = releaseView.ConvertTo<ReleaseView, Release>();

			//Now make a change to field and update
			release1.StartTracking();
			release1.DaysNonWorking = 3;
			releaseManager.Update(new List<Release>() { release1 }, USER_ID_FRED_BLOGGS, projectId);

			//Verify it updated correctly using separate entity
			releaseView = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(3, releaseView.DaysNonWorking);

			//Now try making a change using the out of date entity (has the wrong ConcurrencyDate)
			bool exceptionThrown = false;
			try
			{
				release2.StartTracking();
				release2.DaysNonWorking = 4;
				releaseManager.Update(new List<Release>() { release2 }, USER_ID_FRED_BLOGGS, projectId);
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate entity
			releaseView = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(3, releaseView.DaysNonWorking);

			//Now refresh the old dataset and try again and verify it works
			releaseView = releaseManager.RetrieveById2(projectId, releaseId);
			release2 = releaseView.ConvertTo<ReleaseView, Release>();
			release2.StartTracking();
			release2.DaysNonWorking = 4;
			releaseManager.Update(new List<Release>() { release2 }, USER_ID_FRED_BLOGGS, projectId);

			//Verify it updated correctly using separate dataset
			releaseView = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(4, releaseView.DaysNonWorking);

			//Clean up
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);
		}


		/// <summary>
		/// Tests that changes to the execution status for an test case rollup the release hierarchy correctly
		/// </summary>
		[
		Test,
		SpiraTestCase(1371)
		]
		public void _21_TestCaseExecutionStatusRollups()
		{
			//First create a release/iteration/phase hierarchy
			//Need to have major/minor releases as well
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int releaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.1", "", "1.1", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MinorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int releaseId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.2", "", "1.2", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 2, null, false);
			int iterationId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.1.0001", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 2", "", "1.1.0002", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int phaseId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 1", "", "1.2 SIT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int phaseId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Phase 2", "", "1.2 UAT", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Phase, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);
			int iterationId3 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0001", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);
			int iterationId4 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 3", "", "1.0.0002", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/16/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);

			//Verify the hierarchy
			List<ReleaseView> releases = releaseManager.RetrieveSelfAndChildren(projectId, releaseId1, true, true);
			Assert.AreEqual(9, releases.Count);
			Assert.AreEqual(releaseId1, releases[0].ReleaseId);
			Assert.AreEqual(releaseId2, releases[1].ReleaseId);
			Assert.AreEqual(iterationId1, releases[2].ReleaseId);
			Assert.AreEqual(iterationId2, releases[3].ReleaseId);
			Assert.AreEqual(releaseId3, releases[4].ReleaseId);
			Assert.AreEqual(phaseId1, releases[5].ReleaseId);
			Assert.AreEqual(phaseId2, releases[6].ReleaseId);
			Assert.AreEqual(iterationId3, releases[7].ReleaseId);
			Assert.AreEqual(iterationId4, releases[8].ReleaseId);

			//Now we need to associate some test cases to both the release itself and its iterations
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId3 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId4 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId5 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);
			int testCaseId6 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null);

			//Add to the release/iterations
			testCaseManager.AddToRelease(projectId, releaseId1, new List<int>() { testCaseId1, testCaseId2, testCaseId3, testCaseId4 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, releaseId2, new List<int>() { testCaseId1, testCaseId2, testCaseId3, testCaseId4, testCaseId5 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, releaseId3, new List<int>() { testCaseId1, testCaseId2, testCaseId3, testCaseId4, testCaseId6 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, iterationId1, new List<int>() { testCaseId1, testCaseId2 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, iterationId2, new List<int>() { testCaseId3, testCaseId4 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, iterationId3, new List<int>() { testCaseId1, testCaseId2 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, iterationId4, new List<int>() { testCaseId3, testCaseId4, testCaseId5 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, phaseId1, new List<int>() { testCaseId1, testCaseId2 }, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, phaseId2, new List<int>() { testCaseId1, testCaseId2, testCaseId3 }, USER_ID_SYS_ADMIN);

			//Verify the initial test case counts and execution status
			//Releases
			ReleaseView releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(4, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId2);
			Assert.AreEqual(5, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId3);
			Assert.AreEqual(5, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			//Iterations
			releaseView = releaseManager.RetrieveById2(projectId, iterationId1);
			Assert.AreEqual(2, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, iterationId2);
			Assert.AreEqual(2, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, iterationId3);
			Assert.AreEqual(2, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, iterationId4);
			Assert.AreEqual(3, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			//Phases
			releaseView = releaseManager.RetrieveById2(projectId, phaseId1);
			Assert.AreEqual(2, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, phaseId2);
			Assert.AreEqual(3, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);

			//Now record a test run against some of the iterations/phases and see which ones roll up
			TestRunManager testRunManager = new TestRunManager();
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId4, releaseId1, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5), (int)TestCase.ExecutionStatusEnum.Passed, "Unit Test", "Test1", 0, "Passed", "Passed", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId5, releaseId2, null, null, DateTime.UtcNow.AddMinutes(1), DateTime.UtcNow.AddMinutes(6), (int)TestCase.ExecutionStatusEnum.Blocked, "Unit Test", "Test1", 1, "Blocked", "Blocked", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId4, releaseId3, null, null, DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(7), (int)TestCase.ExecutionStatusEnum.Blocked, "Unit Test", "Test1", 1, "Blocked", "Blocked", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId1, iterationId1, null, null, DateTime.UtcNow.AddMinutes(3), DateTime.UtcNow.AddMinutes(8), (int)TestCase.ExecutionStatusEnum.Failed, "Unit Test", "Test1", 1, "Failed", "Failed", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId3, iterationId2, null, null, DateTime.UtcNow.AddMinutes(4), DateTime.UtcNow.AddMinutes(9), (int)TestCase.ExecutionStatusEnum.Blocked, "Unit Test", "Test1", 1, "Blocked", "Blocked", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId2, iterationId3, null, null, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow.AddMinutes(10), (int)TestCase.ExecutionStatusEnum.Failed, "Unit Test", "Test1", 1, "Failed", "Failed", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId5, iterationId4, null, null, DateTime.UtcNow.AddMinutes(6), DateTime.UtcNow.AddMinutes(11), (int)TestCase.ExecutionStatusEnum.Caution, "Unit Test", "Test1", 1, "Caution", "Caution", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId1, phaseId1, null, null, DateTime.UtcNow.AddMinutes(7), DateTime.UtcNow.AddMinutes(12), (int)TestCase.ExecutionStatusEnum.Failed, "Unit Test", "Test1", 1, "Failed", "Failed", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId3, phaseId2, null, null, DateTime.UtcNow.AddMinutes(8), DateTime.UtcNow.AddMinutes(13), (int)TestCase.ExecutionStatusEnum.Blocked, "Unit Test", "Test1", 1, "Blocked", "Blocked", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);

			//Verify the execution status for the releases and iterations/phases
			//Releases
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(0, releaseView.CountNotRun);
			Assert.AreEqual(1, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(2, releaseView.CountFailed);
			Assert.AreEqual(1, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId2);
			Assert.AreEqual(2, releaseView.CountNotRun);
			Assert.AreEqual(2, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(1, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId3);
			Assert.AreEqual(2, releaseView.CountNotRun);
			Assert.AreEqual(2, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(1, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			//Iterations
			releaseView = releaseManager.RetrieveById2(projectId, iterationId1);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(1, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, iterationId2);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(1, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, iterationId3);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(1, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, iterationId4);
			Assert.AreEqual(2, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(1, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			//Phases
			releaseView = releaseManager.RetrieveById2(projectId, phaseId1);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(1, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId, phaseId2);
			Assert.AreEqual(2, releaseView.CountNotRun);
			Assert.AreEqual(1, releaseView.CountBlocked);
			Assert.AreEqual(0, releaseView.CountCaution);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);

			//Finally clean up
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId2);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId3);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId4);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId5);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId6);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, phaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId4);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
		}

		/// <summary>
		/// Tests the code that is used in the Project Group > Releases list page to allow selection of columns
		/// </summary>
		[Test, SpiraTestCase(1646)]
		public void _22_ProjectGroupReleasesTests()
		{
			//First we need to get the default list of fields to be displayed on the project group > releases page
			List<ArtifactListFieldDisplay> releaseFields = releaseManager.RetrieveFieldsForProjectGroupLists(PROJECT_GROUP_ID, USER_ID_FRED_BLOGGS, Web.GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_COLUMNS);
			Assert.AreEqual(27, releaseFields.Count);
			Assert.IsTrue(releaseFields.FirstOrDefault(r => r.Name == "ReleaseStatusId").IsVisible);

			//Toggle the field
			releaseManager.ToggleProjectGroupColumnVisibility(USER_ID_FRED_BLOGGS, PROJECT_GROUP_ID, "ReleaseStatusId", Web.GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_COLUMNS);
			releaseFields = releaseManager.RetrieveFieldsForProjectGroupLists(PROJECT_GROUP_ID, USER_ID_FRED_BLOGGS, Web.GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_COLUMNS);
			Assert.AreEqual(27, releaseFields.Count);
			Assert.IsFalse(releaseFields.FirstOrDefault(r => r.Name == "ReleaseStatusId").IsVisible);

			//Toggle it back
			releaseManager.ToggleProjectGroupColumnVisibility(USER_ID_FRED_BLOGGS, PROJECT_GROUP_ID, "ReleaseStatusId", Web.GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_COLUMNS);
			releaseFields = releaseManager.RetrieveFieldsForProjectGroupLists(PROJECT_GROUP_ID, USER_ID_FRED_BLOGGS, Web.GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_COLUMNS);
			Assert.AreEqual(27, releaseFields.Count);
			Assert.IsTrue(releaseFields.FirstOrDefault(r => r.Name == "ReleaseStatusId").IsVisible);
		}

		/// <summary>
		/// We need to test that deleting a test run causes the release associated to update correctly
		/// </summary>
		[Test]
		[SpiraTestCase(1663)]
		public void _23_UpdatingExecutionStatusWhenLastRunDeleted()
		{
			//Lets create a new release
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1a", "", "1.0.A", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);

			//lets create a new test case (with a step) and associate with release
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null, true, true);
			testCaseManager.AddToRelease(projectId, releaseId1, new List<int>() { testCaseId1 }, USER_ID_SYS_ADMIN);

			//Verify that we have one test case in the release, not run
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCaseView.ExecutionStatusId);
			ReleaseView releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);

			//Now record two test runs against this test case / release
			TestRunManager testRunManager = new TestRunManager();
			int testRunId1 = testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId1, releaseId1, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5), (int)TestCase.ExecutionStatusEnum.Passed, "Unit Test", "Test1", 0, "Passed", "Passed", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);
			int testRunId2 = testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId1, releaseId1, null, null, DateTime.UtcNow.AddMinutes(1), DateTime.UtcNow.AddMinutes(6), (int)TestCase.ExecutionStatusEnum.Failed, "Unit Test", "Test1", 1, "Failed", "Failed", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);

			//Verify the results
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCaseView.ExecutionStatusId);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(0, releaseView.CountNotRun);
			Assert.AreEqual(1, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);

			//Now delete the newer test run
			testRunManager.Delete(testRunId2, USER_ID_FRED_BLOGGS, projectId);

			//Verify the results
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCaseView.ExecutionStatusId);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(0, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(1, releaseView.CountPassed);

			//Now delete the older test run
			testRunManager.Delete(testRunId1, USER_ID_FRED_BLOGGS, projectId);

			//Verify the results
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCaseView.ExecutionStatusId);
			releaseView = releaseManager.RetrieveById2(projectId, releaseId1);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);

			//Clean Up
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
		}

		/// <summary>
		/// We need to test that moving an iteration updates the execution status correctly
		/// Also that indent/outdenting does the same thing
		/// </summary>
		[Test]
		[SpiraTestCase(1664)]
		public void _24_MovingIterationUpdatesReleaseExecutionStatus()
		{
			//Lets create two releases and a child iteration under the first
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, "Release 2", "", "2.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int iterationId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, "Sprint", "", "1.0.0000", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);

			//lets create a new test case (with a step) and associate with both releases and iteration 1
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, 480, null, null, true, true);
			releaseManager.AddToTestCase(projectId2, testCaseId1, new List<int>() { releaseId1, releaseId2, iterationId1 }, USER_ID_FRED_BLOGGS);

			//Verify the release/iteration counts
			ReleaseView releaseView;
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);

			//Now record a test runs against this test case / iteration
			TestRunManager testRunManager = new TestRunManager();
			int testRunId1 = testRunManager.Record(projectId2, USER_ID_FRED_BLOGGS, testCaseId1, iterationId1, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5), (int)TestCase.ExecutionStatusEnum.Passed, "Unit Test", "Test1", 0, "Passed", "Passed", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);

			//Verify the release/iteration counts
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(0, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(1, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(0, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(1, releaseView.CountPassed);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(1, releaseView.CountNotRun);
			Assert.AreEqual(0, releaseView.CountFailed);
			Assert.AreEqual(0, releaseView.CountPassed);

			//Now move the iteration under the other release
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId2, iterationId1, null);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId2, iterationId1);

			//Verify the hierarchy
			List<ReleaseView> releases;
			releases = releaseManager.Retrieve(USER_ID_FRED_BLOGGS, projectId2, "");
			Assert.AreEqual(3, releases.Count);
			Assert.AreEqual(releaseId1, releases[0].ReleaseId);
			Assert.AreEqual(releaseId2, releases[1].ReleaseId);
			Assert.AreEqual(iterationId1, releases[2].ReleaseId);
			Assert.AreEqual(releases[1].IndentLevel + "AAA", releases[2].IndentLevel);

			//Verify the release/iteration counts
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(0, releaseView.CountNotRun, "Sprint: " + iterationId1);
			Assert.AreEqual(0, releaseView.CountFailed, "Sprint: " + iterationId1);
			Assert.AreEqual(1, releaseView.CountPassed, "Sprint: " + iterationId1);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(1, releaseView.CountNotRun, "Release: " + releaseId1);
			Assert.AreEqual(0, releaseView.CountFailed, "Release: " + releaseId1);
			Assert.AreEqual(0, releaseView.CountPassed, "Release: " + releaseId1);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(0, releaseView.CountNotRun, "Release: " + releaseId2);
			Assert.AreEqual(0, releaseView.CountFailed, "Release: " + releaseId2);
			Assert.AreEqual(1, releaseView.CountPassed, "Release: " + releaseId2);

			//Outdent the iteration
			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId2, iterationId1);

			//Verify the release/iteration counts
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(0, releaseView.CountNotRun, "Sprint: " + iterationId1);
			Assert.AreEqual(0, releaseView.CountFailed, "Sprint: " + iterationId1);
			Assert.AreEqual(1, releaseView.CountPassed, "Sprint: " + iterationId1);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(1, releaseView.CountNotRun, "Release: " + releaseId1);
			Assert.AreEqual(0, releaseView.CountFailed, "Release: " + releaseId1);
			Assert.AreEqual(0, releaseView.CountPassed, "Release: " + releaseId1);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(1, releaseView.CountNotRun, "Release: " + releaseId2);
			Assert.AreEqual(0, releaseView.CountFailed, "Release: " + releaseId2);
			Assert.AreEqual(0, releaseView.CountPassed, "Release: " + releaseId2);

			//Indent the iteration back
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId2, iterationId1);

			//Verify the release/iteration counts
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(0, releaseView.CountNotRun, "Sprint: " + iterationId1);
			Assert.AreEqual(0, releaseView.CountFailed, "Sprint: " + iterationId1);
			Assert.AreEqual(1, releaseView.CountPassed, "Sprint: " + iterationId1);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(1, releaseView.CountNotRun, "Release: " + releaseId1);
			Assert.AreEqual(0, releaseView.CountFailed, "Release: " + releaseId1);
			Assert.AreEqual(0, releaseView.CountPassed, "Release: " + releaseId1);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(0, releaseView.CountNotRun, "Release: " + releaseId2);
			Assert.AreEqual(0, releaseView.CountFailed, "Release: " + releaseId2);
			Assert.AreEqual(1, releaseView.CountPassed, "Release: " + releaseId2);

			//Clean Up
			testCaseManager.DeleteFromDatabase(testCaseId1, USER_ID_FRED_BLOGGS);
			releaseManager.DeleteFromDatabase(iterationId1, USER_ID_FRED_BLOGGS);
			releaseManager.DeleteFromDatabase(releaseId1, USER_ID_FRED_BLOGGS);
			releaseManager.DeleteFromDatabase(releaseId2, USER_ID_FRED_BLOGGS);
		}


		/// <summary>
		/// We need to test that moving an iteration updates the task progress and requirements completion metrics
		/// Also that indent/outdenting does the same thing
		/// </summary>
		[Test]
		[SpiraTestCase(2399)]
		public void _25_MovingIterationUpdatesTaskProgressAndRequirementsCompletion()
		{
			//Lets create two releases and a child iteration under the first
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, "Release 2", "", "2.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 6, 6, null, false);
			int iterationId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, "Sprint", "", "1.0.0000", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/15/2004"), 2, 2, null, false);

			//Lets create a requirement associated with each
			RequirementManager requirementManager = new RequirementManager();
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId2, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 1", null, 1.0M, USER_ID_FRED_BLOGGS);
			int requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId2, releaseId2, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 2", null, 2.0M, USER_ID_FRED_BLOGGS);
			int requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId2, iterationId1, null, (int?)null, Requirement.RequirementStatusEnum.Completed, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement 3", null, 4.0M, USER_ID_FRED_BLOGGS);

			//Verify the release/iteration totals
			ReleaseView releaseView;
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(2, releaseView.RequirementCount);
			Assert.AreEqual(5M, releaseView.RequirementPoints);
			Assert.AreEqual(50, releaseView.PercentComplete);
			Assert.AreEqual(5M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(2M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);
			Assert.AreEqual(2M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(2M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(4M, releaseView.RequirementPoints);
			Assert.AreEqual(100, releaseView.PercentComplete);
			Assert.AreEqual(4M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(0, releaseView.TaskRemainingEffort);

			//Now move the iteration under the other release
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId2, iterationId1, null);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId2, iterationId1);

			//Verify the hierarchy
			List<ReleaseView> releases;
			releases = releaseManager.Retrieve(USER_ID_FRED_BLOGGS, projectId2, "");
			Assert.AreEqual(3, releases.Count);
			Assert.AreEqual(releaseId1, releases[0].ReleaseId);
			Assert.AreEqual(releaseId2, releases[1].ReleaseId);
			Assert.AreEqual(iterationId1, releases[2].ReleaseId);
			Assert.AreEqual(releases[1].IndentLevel + "AAA", releases[2].IndentLevel);

			//Verify the release/iteration counts
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(1M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(2, releaseView.RequirementCount);
			Assert.AreEqual(6M, releaseView.RequirementPoints);
			Assert.AreEqual(50, releaseView.PercentComplete);
			Assert.AreEqual(6M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(2M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(4M, releaseView.RequirementPoints);
			Assert.AreEqual(100, releaseView.PercentComplete);
			Assert.AreEqual(4M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(0, releaseView.TaskRemainingEffort);

			//Outdent the iteration
			releaseManager.Outdent(USER_ID_FRED_BLOGGS, projectId2, iterationId1);

			//Verify the release/iteration counts
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(1M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(2M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);
			Assert.AreEqual(2M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(2M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(4M, releaseView.RequirementPoints);
			Assert.AreEqual(100, releaseView.PercentComplete);
			Assert.AreEqual(4M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(0, releaseView.TaskRemainingEffort);

			//Indent the iteration back
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId2, iterationId1);

			//Verify the release/iteration counts
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(1M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(2, releaseView.RequirementCount);
			Assert.AreEqual(6M, releaseView.RequirementPoints);
			Assert.AreEqual(50, releaseView.PercentComplete);
			Assert.AreEqual(6M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(2M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(4M, releaseView.RequirementPoints);
			Assert.AreEqual(100, releaseView.PercentComplete);
			Assert.AreEqual(4M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(0, releaseView.TaskRemainingEffort);

			//Make one of the releases completed
			Release release = releaseManager.RetrieveById3(projectId2, iterationId1);
			release.StartTracking();
			release.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Closed;
			releaseManager.Update(new List<Release>() { release }, USER_ID_FRED_BLOGGS, projectId2);

			//Verify the release/iteration counts (should be the same)
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(1M, releaseView.RequirementPoints);
			Assert.AreEqual(0, releaseView.PercentComplete);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(1M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, releaseId2);
			Assert.AreEqual(2, releaseView.RequirementCount);
			Assert.AreEqual(6M, releaseView.RequirementPoints);
			Assert.AreEqual(50, releaseView.PercentComplete);
			Assert.AreEqual(6M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(2M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskRemainingEffort);
			releaseView = releaseManager.RetrieveById2(projectId2, iterationId1);
			Assert.AreEqual(1, releaseView.RequirementCount);
			Assert.AreEqual(4M, releaseView.RequirementPoints);
			Assert.AreEqual(100, releaseView.PercentComplete);
			Assert.AreEqual(4M * ProjectManager.DEFAULT_POINT_EFFORT, releaseView.TaskEstimatedEffort);
			Assert.AreEqual(0, releaseView.TaskRemainingEffort);

			//Clean Up
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId2, requirementId1);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId2, requirementId2);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId2, requirementId3);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId2, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId2, releaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId2, releaseId2);
		}
	}
}
