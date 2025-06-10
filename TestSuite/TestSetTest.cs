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
	/// This fixture tests the TestCase business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class TestSetTest
	{
		protected static Business.TestSetManager testSetManager;

		private const int PROJECT_ID = 1;
		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		private static int projectId;
		private static int projectTemplateId;

		private static int testSetFolderId1;
		private static int testSetFolderId2;
		private static int testSetFolderId3;

		private static int testSetId1;
		private static int testSetId2;
		private static int testSetId3;

		private static int testCaseId1;
		private static int testCaseId2;
		private static int testCaseId3;

		private static int releaseId1;

		[TestFixtureSetUp]
		public void Init()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			testSetManager = new TestSetManager();

			//Create a new project for testing with
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("TestSetTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project
			ProjectManager projectManager = new ProjectManager();
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		[
		Test,
		SpiraTestCase(339)
		]
		public void _01_Retrieves()
		{
			//First verify that we don't have any test sets in the root folder
			List<TestSetView> testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, null, InternalRoutines.UTC_OFFSET, null);
		//	Assert.AreEqual(0, testSets.Count);

			//Next retrieve some of the test sets in one of the standard folders
			const int FOLDER_ID1 = 1;
			const int FOLDER_ID2 = 2;

			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, null, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(4, testSets.Count);
			//First row
			TestSetView testSetView = testSets[0];
			Assert.AreEqual("Exploratory Testing", testSetView.Name);
			Assert.IsTrue(testSetView.Description.IsNull());
			Assert.AreEqual("Joe P Smith", testSetView.CreatorName);
			Assert.AreEqual("Fred Bloggs", testSetView.OwnerName);
			Assert.IsNull(testSetView.PlannedDate);
			Assert.IsNull(testSetView.ExecutionDate);
			Assert.AreEqual("Deferred", testSetView.TestSetStatusName);
			Assert.IsTrue(testSetView.ReleaseVersionNumber.IsNull());
			Assert.IsNull(testSetView.EstimatedDuration);
			Assert.IsNull(testSetView.ActualDuration);
			Assert.AreEqual(0, testSetView.CountBlocked);
			Assert.AreEqual(0, testSetView.CountCaution);
			Assert.AreEqual(0, testSetView.CountFailed);
			Assert.AreEqual(0, testSetView.CountNotApplicable);
			Assert.AreEqual(2, testSetView.CountNotRun);
			Assert.AreEqual(0, testSetView.CountPassed);
			//Second row
			testSetView = testSets[1];
			Assert.AreEqual("Testing Cycle for Release 1.0", testSetView.Name);
			Assert.AreEqual("This tests the functionality introduced in release 1.0 of the library system", testSetView.Description);
			Assert.AreEqual("Fred Bloggs", testSetView.CreatorName);
			Assert.AreEqual("Joe P Smith", testSetView.OwnerName);
			//Assert.IsTrue(testSetView.PlannedDate.Value >= DateTime.UtcNow.AddDays(-88) && testSetView.PlannedDate.Value <= DateTime.UtcNow.AddDays(-86), "Planned Date for TX:" + testSetView.ArtifactId + " is " + testSetView.PlannedDate.Value);
			//Assert.IsTrue(testSetView.ExecutionDate.Value >= DateTime.UtcNow.AddDays(-27) && testSetView.ExecutionDate.Value <= DateTime.UtcNow.AddDays(-23), "Execution Date for TX:" + testSetView.ArtifactId + " is " + testSetView.ExecutionDate.Value);
			Assert.AreEqual("In Progress", testSetView.TestSetStatusName);
			Assert.AreEqual("1.0.0.0", testSetView.ReleaseVersionNumber);
			Assert.AreEqual(44, testSetView.EstimatedDuration);
			Assert.AreEqual(255, testSetView.ActualDuration);
			Assert.AreEqual(0, testSetView.CountBlocked);
			Assert.AreEqual(0, testSetView.CountCaution);
			Assert.AreEqual(2, testSetView.CountFailed, "CountFailed1");
			Assert.AreEqual(0, testSetView.CountNotApplicable);
			Assert.AreEqual(4, testSetView.CountNotRun);
			Assert.AreEqual(1, testSetView.CountPassed);

			//Now lets try and filter by some of the standard properties
			Hashtable filters = new Hashtable();
			//TestSetStatus = In Progress
			filters.Add("TestSetStatusId", (int)TestSet.TestSetStatusEnum.InProgress);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(2, testSets.Count);
			Assert.AreEqual(2, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));
			Assert.AreEqual("Testing Cycle for Release 1.0", testSets[0].Name);
			Assert.AreEqual("Testing New Functionality", testSets[1].Name);
			//Release = 1.0 - but displaying execution data for all releases
			filters.Clear();
			filters.Add("ReleaseId", 1);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(1, testSets.Count, "ReleaseId");
			Assert.AreEqual(1, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));
			Assert.AreEqual("Testing Cycle for Release 1.0", testSets[0].Name);
			testSetView = testSets[0];
			Assert.AreEqual(0, testSetView.CountBlocked);
			Assert.AreEqual(0, testSetView.CountCaution);
			Assert.AreEqual(2, testSetView.CountFailed);
			Assert.AreEqual(0, testSetView.CountNotApplicable);
			Assert.AreEqual(4, testSetView.CountNotRun);
			Assert.AreEqual(1, testSetView.CountPassed);
			//Test Set Name or Description LIKE 'Release'
			filters.Clear();
			filters.Add("Name", "Release");
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(2, testSets.Count);
			Assert.AreEqual(2, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));
			Assert.AreEqual("Testing Cycle for Release 1.0", testSets[0].Name);
			Assert.AreEqual("Testing Cycle for Release 1.1", testSets[1].Name);
			//Planned Date
			filters.Clear();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.AddDays(-10);
			dateRange.EndDate = DateTime.UtcNow.AddDays(-8);
			filters.Add("PlannedDate", dateRange);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			//Assert.AreEqual(0, testSets.Count, "PlannedDate");
			//Assert.AreEqual(1, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));
			//Assert.AreEqual("Testing Cycle for Release 1.1", testSets[0].Name);

			//Now by some of the test execution status filters
			//0% Run
			filters.Clear();
			filters.Add("ExecutionStatusId", 1);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(2, testSets.Count);
			Assert.AreEqual(2, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));
			//<=50% Run
			filters.Clear();
			filters.Add("ExecutionStatusId", 2);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(4, testSets.Count);
			//>0% Failed
			filters.Clear();
			filters.Add("ExecutionStatusId", 7);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(1, testSets.Count, "Count3");
			//100% Passed
			filters.Clear();
			filters.Add("ExecutionStatusId", 6);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(0, testSets.Count);

			//Now lets try and filter by some of the custom properties
			//Custom = Windows 8
			filters.Clear();
			filters.Add("Custom_02", 13);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID2);
			Assert.AreEqual(1, testSets.Count, "Custom_02");
			Assert.AreEqual(1, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID2));
			Assert.AreEqual("Regression Testing for Windows 8", testSets[0].Name);
			//Custom_01 = Need to test against the Vista box
			filters.Clear();
			filters.Add("Custom_01", "Need to test against the Vista box");
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID2);
			Assert.AreEqual(1, testSets.Count, "Custom_01");
			Assert.AreEqual(1, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID2));
			Assert.AreEqual("Regression Testing for Windows Vista", testSets[0].Name);

			//Now lets test that we can use a multi-valued filter
			filters.Clear();
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add((int)TestSet.TestSetStatusEnum.InProgress);
			multiValueFilter.Values.Add((int)TestSet.TestSetStatusEnum.NotStarted);
			filters.Add("TestSetStatusId", multiValueFilter);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(3, testSets.Count);
			Assert.AreEqual(3, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));

			//Test filtering for execution data = Release = 1.0 - but displaying all releases
			filters.Clear();
			List<TestSetReleaseView> testSetsByRelease = testSetManager.RetrieveByReleaseId(PROJECT_ID, 1, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(4, testSetsByRelease.Count);
			Assert.AreEqual(4, testSetManager.CountByRelease(PROJECT_ID, 1, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));
			TestSetReleaseView testSetByRelease = testSetsByRelease[1];
			Assert.AreEqual("Testing Cycle for Release 1.0", testSetByRelease.Name);
			Assert.AreEqual(0, testSetByRelease.CountBlocked);
			Assert.AreEqual(0, testSetByRelease.CountCaution);
			Assert.AreEqual(2, testSetByRelease.CountFailed);
			Assert.AreEqual(0, testSetByRelease.CountNotApplicable);
			Assert.AreEqual(0, testSetByRelease.CountNotRun);   /* Not Run Test Case Excluded when displaying execution data for a specific release */
			Assert.AreEqual(1, testSetByRelease.CountPassed);
			testSetByRelease = testSetsByRelease[2];
			Assert.AreEqual("Testing Cycle for Release 1.1", testSetByRelease.Name);
			Assert.AreEqual(0, testSetByRelease.CountBlocked);
			Assert.AreEqual(0, testSetByRelease.CountCaution);
			Assert.AreEqual(0, testSetByRelease.CountFailed);
			Assert.AreEqual(0, testSetByRelease.CountNotApplicable);
			Assert.AreEqual(0, testSetByRelease.CountNotRun);
			Assert.AreEqual(3, testSetByRelease.CountPassed);

			//Test that we can retrieve by id - using filters
			filters.Clear();
			filters.Add("TestSetId", 1);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(1, testSets.Count, "Count2");
			Assert.AreEqual(1, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));
			Assert.AreEqual(1, testSets[0].TestSetId);

			//Now test that we can retrieve by id
			testSetView = testSetManager.RetrieveById(PROJECT_ID, 1);
			Assert.AreEqual(1, testSets.Count);
			Assert.AreEqual(1, testSets[0].TestSetId);
			Assert.AreEqual(1, testSets[0].CountPassed);
			Assert.AreEqual(2, testSets[0].CountFailed);
			Assert.AreEqual(0, testSets[0].CountBlocked);
			Assert.AreEqual(0, testSets[0].CountCaution);
			Assert.AreEqual(4, testSets[0].CountNotRun);

			//Test that we can retrieve by execution status lookup
			List<TestSetStatus> testSetStati = testSetManager.RetrieveStatuses();
			Assert.AreEqual(5, testSetStati.Count);

			//Test that we can retrieve the two lookups used in test sets
			SortedList<int, string> lookupList = testSetManager.RetrieveExecutionStatusFiltersLookup();
			Assert.AreEqual(15, lookupList.Count);

			//Next lets get all test sets that have certain fields set to null
			filters.Clear();
			MultiValueFilter multiFilterValue = new MultiValueFilter();
			multiFilterValue.IsNone = true;
			filters.Add("OwnerId", multiFilterValue);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(0, testSets.Count);
			Assert.AreEqual(0, testSetManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1));

			//Now lets test that we can get several test sets by their IDs using a multivalue filter
			multiFilterValue.Clear();
			multiFilterValue.Values.Add(1);
			multiFilterValue.Values.Add(5);
			filters.Clear();
			filters.Add("TestSetId", multiFilterValue);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET, FOLDER_ID1);
			Assert.AreEqual(2, testSets.Count);
			Assert.AreEqual(1, testSets[0].TestSetId);
			Assert.AreEqual(5, testSets[1].TestSetId);

			//Now lets test that we can retrieve the list of test cases in a test set with the
			//correct statuses and execution dates
			TestCaseManager testCaseManager = new TestCaseManager();

			//The method that does not return the execution data
			List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(1);
			Assert.AreEqual(7, testSetTestCases.Count);
			Assert.AreEqual("Ability to create new book", testSetTestCases[0].Name);
			Assert.AreEqual("Author management", testSetTestCases[6].Name);

			//The method that returns the updateable version
			List<TestSetTestCase> testSetTestCases2 = testSetManager.RetrieveTestCases2(1);
			Assert.AreEqual(7, testSetTestCases2.Count);
			Assert.AreEqual(2, testSetTestCases2[0].TestCaseId);
			Assert.AreEqual(9, testSetTestCases2[6].TestCaseId);

			//The method that does not return the execution data
			List<TestSetTestCaseView> testSetTestCases3 = testSetManager.RetrieveTestCases3(PROJECT_ID, 1, null);
			Assert.AreEqual(7, testSetTestCases3.Count);
			Assert.AreEqual("Ability to create new book", testSetTestCases3[0].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testSetTestCases3[0].ExecutionStatusId);
			Assert.IsTrue(testSetTestCases3[0].ExecutionDate.HasValue);
			Assert.AreEqual("Author management", testSetTestCases3[6].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testSetTestCases3[6].ExecutionStatusId);
			Assert.IsTrue(testSetTestCases3[6].ExecutionDate.IsNull());

			//Verify that you can get the list of test sets that a test case belongs to
			testSets = testSetManager.RetrieveByTestCaseId(PROJECT_ID, 2, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, testSets.Count);
			Assert.AreEqual(1, testSets[0].TestSetId);
			Assert.AreEqual(2, testSets[1].TestSetId);

			//We need to test that if we filter a test set by release, test sets assign to its child iterations are also included
			//Lets create some test sets mapped to iterations under a common release:
			int testSetId1 = testSetManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, null, 11, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", "Test Set 1 Description", DateTime.UtcNow.AddDays(1), TestRun.TestRunTypeEnum.Manual, null, null);
			int testSetId2 = testSetManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, null, 12, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, TestSet.TestSetStatusEnum.NotStarted, "Test Set 2", "Test Set 2 Description", DateTime.UtcNow.AddDays(1), TestRun.TestRunTypeEnum.Manual, null, null);

			//Verify that we can retrieve by their parent release
			filters.Clear();
			filters.Add("ReleaseId", 2);
			testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, null);
			Assert.AreEqual(2, testSets.Count);
			Assert.AreEqual(testSetId1, testSets[0].TestSetId);
			Assert.AreEqual(testSetId2, testSets[1].TestSetId);

			//Verify that we can retrieve for a specific id with release-specific execution data
			TestSetReleaseView testSetRelease = testSetManager.RetrieveByIdForRelease(PROJECT_ID, testSetId1, 2);
			Assert.IsNotNull(testSetRelease);

			//Clean up by deleting the test sets
			testSetManager.DeleteFromDatabase(testSetId1, USER_ID_FRED_BLOGGS);
			testSetManager.DeleteFromDatabase(testSetId2, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(350)
		]
		public void _02_RetrieveSummaryData()
		{
			//Test that we can retrieve the list of test sets owned by a user - cross project
			List<TestSetView> testSets = testSetManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual(5, testSets.Count);
			Assert.AreEqual("Regression Testing for Windows 8", testSets[0].Name);
			Assert.AreEqual("Completed", testSets[0].TestSetStatusName);
			Assert.IsTrue(testSets[0].PlannedDate.IsNull());
			Assert.AreEqual("Library Information System (Sample)", testSets[0].ProjectName);

			//Test that we can retrieve the list of test sets owned by a user - specific project
			testSets = testSetManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, PROJECT_ID);
			Assert.AreEqual(3, testSets.Count);
			Assert.AreEqual("Regression Testing for Windows 8", testSets[0].Name);
			Assert.AreEqual("Completed", testSets[0].TestSetStatusName);
			Assert.IsTrue(testSets[0].PlannedDate.IsNull());
			Assert.AreEqual("Library Information System (Sample)", testSets[0].ProjectName);

			//Test that we can get the test set execution status summary, used for the project home page
			//First for all releases
			List<TestSet_ExecutionStatusSummary> executionStatusSummary = testSetManager.RetrieveExecutionStatusSummary(PROJECT_ID, null);
			//Make sure the data is as expected
			Assert.AreEqual(6, executionStatusSummary.Count);
			Assert.AreEqual("Failed", executionStatusSummary[0].ExecutionStatusName);
			Assert.AreEqual(2, executionStatusSummary[0].StatusCount);
			Assert.AreEqual("Passed", executionStatusSummary[1].ExecutionStatusName);
			Assert.AreEqual(4, executionStatusSummary[1].StatusCount);
			Assert.AreEqual("Not Run", executionStatusSummary[2].ExecutionStatusName);
			Assert.AreEqual(24, executionStatusSummary[2].StatusCount);
			Assert.AreEqual("Blocked", executionStatusSummary[3].ExecutionStatusName);
			Assert.AreEqual(0, executionStatusSummary[3].StatusCount);
			Assert.AreEqual("Caution", executionStatusSummary[4].ExecutionStatusName);
			Assert.AreEqual(0, executionStatusSummary[4].StatusCount);

			//Next for a release that contains iterations
			executionStatusSummary = testSetManager.RetrieveExecutionStatusSummary(PROJECT_ID, 1);
			//Make sure the data is as expected
			Assert.AreEqual(6, executionStatusSummary.Count);
			Assert.AreEqual("Failed", executionStatusSummary[0].ExecutionStatusName);
			Assert.AreEqual(2, executionStatusSummary[0].StatusCount);
			Assert.AreEqual("Passed", executionStatusSummary[1].ExecutionStatusName);
			Assert.AreEqual(1, executionStatusSummary[1].StatusCount);
			Assert.AreEqual("Not Run", executionStatusSummary[2].ExecutionStatusName);
			Assert.AreEqual(4, executionStatusSummary[2].StatusCount);
			Assert.AreEqual("Blocked", executionStatusSummary[3].ExecutionStatusName);
			Assert.AreEqual(0, executionStatusSummary[3].StatusCount);
			Assert.AreEqual("Caution", executionStatusSummary[4].ExecutionStatusName);
			Assert.AreEqual(0, executionStatusSummary[4].StatusCount);

			//Get the list of overdue test sets (all releases)
			testSets = testSetManager.RetrieveOverdue(PROJECT_ID, null);
			Assert.AreEqual(3, testSets.Count);

			//Get the list of overdue test sets (specific releases)
			testSets = testSetManager.RetrieveOverdue(PROJECT_ID, 1);
			Assert.AreEqual(1, testSets.Count);

			//Get the test set schedule graph - all releases
			Dictionary<string, int> testSetSchedule = testSetManager.RetrieveScheduleSummary(PROJECT_ID, null);
			Assert.AreEqual(3, testSetSchedule.Count);
			Assert.AreEqual(3, testSetSchedule.FirstOrDefault().Value);

			//For a specific release
			testSetSchedule = testSetManager.RetrieveScheduleSummary(PROJECT_ID, 1);
			Assert.AreEqual(3, testSetSchedule.Count);
			Assert.AreEqual(1, testSetSchedule.FirstOrDefault().Value);
		}

		[
		Test,
		SpiraTestCase(391)
		]
		public void _03_InsertUpdateDeleteTestSetFolders()
		{
			//First lets try inserting a new test set folder at the top-level
			testSetFolderId1 = testSetManager.TestSetFolder_Create("Test Set Folder 1", projectId, null).TestSetFolderId;

			//Verify that it inserted correctly
			TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(testSetFolderId1, testSetFolder.TestSetFolderId);
			Assert.AreEqual("Test Set Folder 1", testSetFolder.Name);
			Assert.IsTrue(testSetFolder.Description.IsNull());
			Assert.IsTrue(testSetFolder.ExecutionDate.IsNull());
			Assert.IsNull(testSetFolder.ActualDuration);
			Assert.IsNull(testSetFolder.EstimatedDuration);
			Assert.IsNull(testSetFolder.ExecutionDate);
			Assert.IsNull(testSetFolder.ParentTestSetFolderId);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(0, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);

			//Next lets try inserting another top-level folder, with a description
			testSetFolderId2 = testSetManager.TestSetFolder_Create("Test Set Folder 2", projectId, "This is the second test set folder").TestSetFolderId;

			//Verify that it inserted correctly
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId2);
			Assert.AreEqual(testSetFolderId2, testSetFolder.TestSetFolderId);
			Assert.AreEqual("Test Set Folder 2", testSetFolder.Name);
			Assert.AreEqual("This is the second test set folder", testSetFolder.Description);
			Assert.IsTrue(testSetFolder.ExecutionDate.IsNull());
			Assert.IsNull(testSetFolder.ActualDuration);
			Assert.IsNull(testSetFolder.EstimatedDuration);
			Assert.IsNull(testSetFolder.ExecutionDate);
			Assert.IsNull(testSetFolder.ParentTestSetFolderId);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(0, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);

			//Now lets add a child test set folder of the first one
			testSetFolderId3 = testSetManager.TestSetFolder_Create("Test Set Folder 3", projectId, null, testSetFolderId1).TestSetFolderId;

			//Verify that it inserted correctly
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId3);
			Assert.AreEqual(testSetFolderId3, testSetFolder.TestSetFolderId);
			Assert.AreEqual("Test Set Folder 3", testSetFolder.Name);
			Assert.IsTrue(testSetFolder.Description.IsNull());
			Assert.IsTrue(testSetFolder.ExecutionDate.IsNull());
			Assert.IsNull(testSetFolder.ActualDuration);
			Assert.IsNull(testSetFolder.EstimatedDuration);
			Assert.IsNull(testSetFolder.ExecutionDate);
			Assert.AreEqual(testSetFolderId1, testSetFolder.ParentTestSetFolderId);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(0, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);

			//Now lets verify the entire test set hierarchy
			List<TestSetFolderHierarchyView> testSetFolderHierarchy = testSetManager.TestSetFolder_GetList(projectId);
			Assert.AreEqual(3, testSetFolderHierarchy.Count);
			Assert.AreEqual("Test Set Folder 1", testSetFolderHierarchy[0].Name);
			Assert.AreEqual("AAA", testSetFolderHierarchy[0].IndentLevel);
			Assert.AreEqual("Test Set Folder 3", testSetFolderHierarchy[1].Name);
			Assert.AreEqual("AAAAAA", testSetFolderHierarchy[1].IndentLevel);
			Assert.AreEqual("Test Set Folder 2", testSetFolderHierarchy[2].Name);
			Assert.AreEqual("AAB", testSetFolderHierarchy[2].IndentLevel);

			//Now test that we can update the details of a test set folder
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId3);
			testSetFolder.StartTracking();
			testSetFolder.Description = "This is the third folder";
			testSetManager.TestSetFolder_Update(testSetFolder);

			//Verify that it updated correctly
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId3);
			Assert.AreEqual(testSetFolderId3, testSetFolder.TestSetFolderId);
			Assert.AreEqual("Test Set Folder 3", testSetFolder.Name);
			Assert.AreEqual("This is the third folder", testSetFolder.Description);
			Assert.IsTrue(testSetFolder.ExecutionDate.IsNull());
			Assert.IsNull(testSetFolder.ActualDuration);
			Assert.IsNull(testSetFolder.EstimatedDuration);
			Assert.IsNull(testSetFolder.ExecutionDate);
			Assert.AreEqual(testSetFolderId1, testSetFolder.ParentTestSetFolderId);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(0, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);

			//Now create a new test set folder under the other parent
			int testSetFolderId = testSetManager.TestSetFolder_Create("Test Set Folder Temp", projectId, null, testSetFolderId2).TestSetFolderId;

			//Now lets verify the entire test set hierarchy
			testSetFolderHierarchy = testSetManager.TestSetFolder_GetList(projectId);
			Assert.AreEqual(4, testSetFolderHierarchy.Count);
			Assert.AreEqual("Test Set Folder 1", testSetFolderHierarchy[0].Name);
			Assert.AreEqual("AAA", testSetFolderHierarchy[0].IndentLevel);
			Assert.AreEqual("Test Set Folder 3", testSetFolderHierarchy[1].Name);
			Assert.AreEqual("AAAAAA", testSetFolderHierarchy[1].IndentLevel);
			Assert.AreEqual("Test Set Folder 2", testSetFolderHierarchy[2].Name);
			Assert.AreEqual("AAB", testSetFolderHierarchy[2].IndentLevel);
			Assert.AreEqual("Test Set Folder Temp", testSetFolderHierarchy[3].Name);
			Assert.AreEqual("AABAAA", testSetFolderHierarchy[3].IndentLevel);

			//Now move the test set folder under a different parent
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId);
			testSetFolder.StartTracking();
			testSetFolder.ParentTestSetFolderId = testSetFolderId3;
			testSetManager.TestSetFolder_Update(testSetFolder);

			//Now lets verify the entire test set hierarchy
			testSetFolderHierarchy = testSetManager.TestSetFolder_GetList(projectId);
			Assert.AreEqual(4, testSetFolderHierarchy.Count);
			Assert.AreEqual("Test Set Folder 1", testSetFolderHierarchy[0].Name);
			Assert.AreEqual("AAA", testSetFolderHierarchy[0].IndentLevel);
			Assert.AreEqual("Test Set Folder 3", testSetFolderHierarchy[1].Name);
			Assert.AreEqual("AAAAAA", testSetFolderHierarchy[1].IndentLevel);
			Assert.AreEqual("Test Set Folder Temp", testSetFolderHierarchy[2].Name);
			Assert.AreEqual("AAAAAAAAA", testSetFolderHierarchy[2].IndentLevel);
			Assert.AreEqual("Test Set Folder 2", testSetFolderHierarchy[3].Name);
			Assert.AreEqual("AAB", testSetFolderHierarchy[3].IndentLevel);

			//Make sure the system doesn't let us put folders in a recursive loop
			bool exceptionThrown = false;
			try
			{
				testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId3);
				testSetFolder.StartTracking();
				testSetFolder.ParentTestSetFolderId = testSetFolderId;
				testSetManager.TestSetFolder_Update(testSetFolder);
			}
			catch (FolderCircularReferenceException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Now test some of the other folder retrieve functions
			//Root folders
			List<TestSetFolder> testSetFolders = testSetManager.TestSetFolder_GetByParentId(projectId, null, "Name", true, null);
			Assert.AreEqual(2, testSetFolders.Count);
			Assert.AreEqual("Test Set Folder 1", testSetFolders[0].Name);
			Assert.AreEqual("Test Set Folder 2", testSetFolders[1].Name);

			//Sub folders
			testSetFolders = testSetManager.TestSetFolder_GetByParentId(projectId, testSetFolderId1, "Name", true, null);
			Assert.AreEqual(1, testSetFolders.Count);
			Assert.AreEqual("Test Set Folder 3", testSetFolders[0].Name);

			//Self and parents
			testSetFolderHierarchy = testSetManager.TestSetFolder_GetParents(projectId, testSetFolderId, true);
			Assert.AreEqual(3, testSetFolderHierarchy.Count);
			Assert.AreEqual("Test Set Folder 1", testSetFolderHierarchy[0].Name);
			Assert.AreEqual("AAA", testSetFolderHierarchy[0].IndentLevel);
			Assert.AreEqual("Test Set Folder 3", testSetFolderHierarchy[1].Name);
			Assert.AreEqual("AAAAAA", testSetFolderHierarchy[1].IndentLevel);
			Assert.AreEqual("Test Set Folder Temp", testSetFolderHierarchy[2].Name);
			Assert.AreEqual("AAAAAAAAA", testSetFolderHierarchy[2].IndentLevel);

			//Parents only
			testSetFolderHierarchy = testSetManager.TestSetFolder_GetParents(projectId, testSetFolderId, false);
			Assert.AreEqual(2, testSetFolderHierarchy.Count);
			Assert.AreEqual("Test Set Folder 1", testSetFolderHierarchy[0].Name);
			Assert.AreEqual("AAA", testSetFolderHierarchy[0].IndentLevel);
			Assert.AreEqual("Test Set Folder 3", testSetFolderHierarchy[1].Name);
			Assert.AreEqual("AAAAAA", testSetFolderHierarchy[1].IndentLevel);

			//Finally delete this one test set folder and verify it deleted successfully
			testSetManager.TestSetFolder_Delete(projectId, testSetFolderId, USER_ID_FRED_BLOGGS);
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId);
			Assert.IsNull(testSetFolder, "Test Set Folder Not Deleted Correctly");

			//Now lets verify the entire test set hierarchy
			testSetFolderHierarchy = testSetManager.TestSetFolder_GetList(projectId);
			Assert.AreEqual(3, testSetFolderHierarchy.Count);
			Assert.AreEqual("Test Set Folder 1", testSetFolderHierarchy[0].Name);
			Assert.AreEqual("AAA", testSetFolderHierarchy[0].IndentLevel);
			Assert.AreEqual("Test Set Folder 3", testSetFolderHierarchy[1].Name);
			Assert.AreEqual("AAAAAA", testSetFolderHierarchy[1].IndentLevel);
			Assert.AreEqual("Test Set Folder 2", testSetFolderHierarchy[2].Name);
			Assert.AreEqual("AAB", testSetFolderHierarchy[2].IndentLevel);

			//Check if a folder exists or not
			Assert.AreEqual(true, testSetManager.TestSetFolder_Exists(projectId, testSetFolderId2), "Task folder should exist");
			Assert.AreEqual(false, testSetManager.TestSetFolder_Exists(projectId + 1, testSetFolderId2), "Task folder does not exist in specified product");
			Assert.AreEqual(false, testSetManager.TestSetFolder_Exists(projectId, testSetFolderId2 + 10000000), "Task folder should NOT exist");
		}

		[
		Test,
		SpiraTestCase(342)
		]
		public void _04_InsertUpdateDeleteTestSets()
		{
			//First lets try inserting a new test set at at the root level
			testSetId1 = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);

			//We add one test case to it so that it has a count
			TestCaseManager testCaseManager = new TestCaseManager();
			int tc_priorityMediumId = testCaseManager.TestCasePriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 3).TestCasePriorityId;
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, null, null, null);
			testSetManager.AddTestCase(projectId, testSetId1, testCaseId1, null, null);

			//Verify that it inserted correctly
			TestSetView testSetView = testSetManager.RetrieveById(projectId, testSetId1);
			Assert.AreEqual(testSetId1, testSetView.TestSetId, "TestSetId");
			Assert.AreEqual(null, testSetView.TestSetFolderId);
			Assert.AreEqual("Fred Bloggs", testSetView.CreatorName, "CreatorName");
			Assert.AreEqual("Test Set 1", testSetView.Name, "Name");
			Assert.AreEqual("Not Started", testSetView.TestSetStatusName, "TestSetStatusName");
			Assert.IsTrue(testSetView.Description.IsNull(), "Description");
			Assert.IsTrue(testSetView.ReleaseId.IsNull(), "ReleaseId");
			Assert.IsTrue(testSetView.OwnerName.IsNull(), "OwnerName");
			Assert.IsTrue(testSetView.PlannedDate.IsNull(), "PlannedDate");
			Assert.IsTrue(testSetView.ExecutionDate.IsNull(), "ExecutionDate");

			//Next lets try inserting a new test set under one of the folders and associated to a release
			ReleaseManager releaseManager = new ReleaseManager();
			releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			testSetId2 = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, testSetFolderId1, releaseId1, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, TestSet.TestSetStatusEnum.InProgress, "Test Set 2", "Test Set 2 Description", DateTime.Now.AddDays(1), TestRun.TestRunTypeEnum.Manual, null, null);
			testSetManager.AddTestCase(projectId, testSetId2, testCaseId1, null, null);

			//Verify that it inserted correctly
			testSetView = testSetManager.RetrieveById(projectId, testSetId2);
			Assert.AreEqual(testSetId2, testSetView.TestSetId, "TestSetId");
			Assert.AreEqual(testSetFolderId1, testSetView.TestSetFolderId);
			Assert.AreEqual("Fred Bloggs", testSetView.CreatorName, "CreatorName");
			Assert.AreEqual("Test Set 2", testSetView.Name, "Name");
			Assert.AreEqual("In Progress", testSetView.TestSetStatusName, "TestSetStatusName");
			Assert.AreEqual("Test Set 2 Description", testSetView.Description, "Description");
			Assert.AreEqual(releaseId1, testSetView.ReleaseId, "ReleaseId");
			Assert.AreEqual("1.0.0.0", testSetView.ReleaseVersionNumber, "ReleaseVersionNumber");
			Assert.AreEqual("Joe P Smith", testSetView.OwnerName, "OwnerName");
			Assert.IsFalse(testSetView.PlannedDate.IsNull(), "PlannedDate");
			Assert.IsTrue(testSetView.ExecutionDate.IsNull(), "ExecutionDate");

			//Verify the folder counts/execution data
			TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(1, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
			Assert.IsNull(testSetFolder.ActualDuration);
			Assert.IsNull(testSetFolder.ExecutionDate);

			//Now test that we can update the details of a test set
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId1);
			testSet.StartTracking();
			DateTime oldLastUpdateDate = testSet.LastUpdateDate;
			testSet.TestSetFolderId = testSetFolderId1;
			testSet.ReleaseId = releaseId1;
			testSet.Name = "Test Set 1.0";
			testSet.Description = "Test Set 1 Description";
			testSet.PlannedDate = DateTime.Parse("1/1/2007");
			testSet.OwnerId = USER_ID_FRED_BLOGGS;
			testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Deferred;
			testSet.CreatorId = USER_ID_JOE_SMITH;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Verify that it updated correctly
			testSetView = testSetManager.RetrieveById(projectId, testSetId1);
			Assert.AreEqual(testSetFolderId1, testSetView.TestSetFolderId);
			Assert.AreEqual("Joe P Smith", testSetView.CreatorName, "CreatorName");
			Assert.AreEqual("Test Set 1.0", testSetView.Name, "Name");
			Assert.AreEqual("Deferred", testSetView.TestSetStatusName, "TestSetStatusName");
			Assert.AreEqual("Test Set 1 Description", testSetView.Description, "Description");
			Assert.AreEqual(releaseId1, testSetView.ReleaseId, "ReleaseId");
			Assert.AreEqual("1.0.0.0", testSetView.ReleaseVersionNumber, "ReleaseVersionNumber");
			Assert.AreEqual("Fred Bloggs", testSetView.OwnerName, "OwnerName");
			Assert.AreEqual("1/1/2007", testSetView.PlannedDate.Value.ToShortDateString(), "PlannedDate");
			Assert.IsTrue(testSetView.ExecutionDate.IsNull(), "ExecutionDate");
			Assert.AreNotEqual(oldLastUpdateDate, testSetView.LastUpdateDate, "LastUpdateDate");

			//Verify the folder counts/execution data
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(2, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
			Assert.IsNull(testSetFolder.ActualDuration);
			Assert.IsNull(testSetFolder.ExecutionDate);

			//Finally delete a test set and make sure that they deleted and the folder counts updated correctly
			testSetManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testSetId2);
			bool artifactExists = true;
			try
			{
				testSetView = testSetManager.RetrieveById(projectId, testSetId2);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Test Set Not Deleted Correctly");

			//Verify the folder counts/execution data
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(1, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
			Assert.IsNull(testSetFolder.ActualDuration);
			Assert.IsNull(testSetFolder.ExecutionDate);

			//Undelete
			testSetManager.UnDelete(testSetId2, USER_ID_FRED_BLOGGS, -1, false);

			//Verify the counts reverted
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(2, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
		}

		[
		Test,
		SpiraTestCase(341)
		]
		public void _05_MoveTestSets()
		{
			//Verify the initial counts
			TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(2, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId2);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(0, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);

			//First lets move a test set to another folder
			testSetManager.TestSet_UpdateFolder(testSetId1, testSetFolderId2);

			//Verify the counts have updated
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(1, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId2);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(1, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);

			//First lets move a test set to the root folder
			testSetManager.TestSet_UpdateFolder(testSetId1, null);

			//Verify the counts have updated
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(1, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId2);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(0, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);

			//Now move it back to starting position
			testSetManager.TestSet_UpdateFolder(testSetId1, testSetFolderId1);

			//Verify the counts have updated
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId1);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(2, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
			testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId2);
			Assert.AreEqual(0, testSetFolder.CountBlocked);
			Assert.AreEqual(0, testSetFolder.CountCaution);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountNotApplicable);
			Assert.AreEqual(0, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountPassed);
		}

		[
		Test,
		SpiraTestCase(344)
		]
		public void _06_AddRemoveReorderTestCases()
		{
			//First lets create a new test set to work with and verify that it has no test cases
			testSetId3 = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, testSetFolderId1, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 3", null, null, TestRun.TestRunTypeEnum.Manual, null, null);
			List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual(0, testSetTestCases.Count);

			//Also lets create some new test cases
			TestCaseManager testCaseManager = new TestCaseManager();
			testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Sample Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Sample Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			testCaseId3 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Sample Test Case 3", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			//Now lets test that we can add a test case to the test set and verify
			testSetManager.AddTestCase(projectId, testSetId3, testCaseId1, null, null);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual(1, testSetTestCases.Count);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[0].Name);
			Assert.IsTrue(testSetTestCases[0].OwnerName.IsNull());

			//Now lets add another test case to the test set and verify that it was added to the end
			testSetManager.AddTestCase(projectId, testSetId3, testCaseId2, null, null);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual(2, testSetTestCases.Count);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[0].Name);
			Assert.IsTrue(testSetTestCases[0].OwnerName.IsNull());
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[1].Name);
			Assert.IsTrue(testSetTestCases[1].OwnerName.IsNull());

			//Now lets test that we can add the same test case again at the end, specifying an owner explicitly
			testSetManager.AddTestCase(projectId, testSetId3, testCaseId1, USER_ID_FRED_BLOGGS, null);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual(3, testSetTestCases.Count);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[0].Name);
			Assert.IsTrue(testSetTestCases[0].OwnerName.IsNull());
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[1].Name);
			Assert.IsTrue(testSetTestCases[1].OwnerName.IsNull());
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[2].Name);
			Assert.AreEqual("Fred Bloggs", testSetTestCases[2].OwnerName);

			//Capture the unique record id for use later
			int testSetTestCaseId1 = testSetTestCases[0].TestSetTestCaseId;
			int testSetTestCaseId2 = testSetTestCases[1].TestSetTestCaseId;
			int testSetTestCaseId3 = testSetTestCases[2].TestSetTestCaseId;

			//Now lets test that we can reorder the test cases
			//First lets move one test case up a position and verify
			testSetManager.MoveTestCase(testSetId3, testSetTestCaseId2, testSetTestCaseId1);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[0].Name);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[1].Name);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[2].Name);

			//Next lets move it to the end of the list and verify
			testSetManager.MoveTestCase(testSetId3, testSetTestCaseId2, null);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[0].Name);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[1].Name);
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[2].Name);

			//Now lets move it back to where it started
			testSetManager.MoveTestCase(testSetId3, testSetTestCaseId2, testSetTestCaseId3);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[0].Name);
			Assert.IsTrue(testSetTestCases[0].OwnerName.IsNull());
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[1].Name);
			Assert.IsTrue(testSetTestCases[1].OwnerName.IsNull());
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[2].Name);
			Assert.AreEqual("Fred Bloggs", testSetTestCases[2].OwnerName);

			//Need to test that we can update the override owner field of the test cases
			List<TestSetTestCase> testSetTestCases2 = testSetManager.RetrieveTestCases2(testSetId3);
			testSetTestCases2[1].StartTracking();
			testSetTestCases2[1].OwnerId = USER_ID_JOE_SMITH;
			testSetManager.UpdateTestCases(testSetTestCases2, USER_ID_FRED_BLOGGS);

			//Verify the update
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[0].Name);
			Assert.IsTrue(testSetTestCases[0].OwnerName.IsNull());
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[1].Name);
			Assert.AreEqual("Joe P Smith", testSetTestCases[1].OwnerName);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[2].Name);
			Assert.AreEqual("Fred Bloggs", testSetTestCases[2].OwnerName);
			int existingTestSetTestCaseId = testSetTestCases[1].TestSetTestCaseId;

			//Need to test that we can add another test cases to the middle of the list by choosing the folder's id
			testSetManager.AddTestCase(projectId, testSetId3, testCaseId3, null, existingTestSetTestCaseId);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual(4, testSetTestCases.Count);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[0].Name);
			Assert.AreEqual("Sample Test Case 3", testSetTestCases[1].Name);
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[2].Name);
			Assert.AreEqual("Sample Test Case 1", testSetTestCases[3].Name);

			//Need to test that deleting a test case contained in a test set deletes the associations
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId3);
			Assert.AreEqual(2, testSetTestCases.Count);
			Assert.AreEqual("Sample Test Case 3", testSetTestCases[0].Name);
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[1].Name);

			//Finally, verify that removing all the test cases in a set, changes the test set count to zero (0)
			//This previously did not work [IN:3631]
			testSetManager.RemoveTestCase(projectId, testSetId3, testSetTestCases[0].TestSetTestCaseId, USER_ID_FRED_BLOGGS);
			testSetManager.RemoveTestCase(projectId, testSetId3, testSetTestCases[1].TestSetTestCaseId, USER_ID_FRED_BLOGGS);

			//Verify the counts
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId3);
			Assert.AreEqual(0, testSet.TestCaseCount);
		}

		[
		Test,
		SpiraTestCase(343)
		]
		public void _07_CopyTestSets()
		{
			//Lets make a copy of one of the test sets that contains test cases and verify that it copied correctly
			//We'll just copy it to the root folder. The execution data gets reset since the test runs are not linked
			//We do this in the sample project as it would otherwise require too much data to be setup ahead of time!!
			int newTestSetId = testSetManager.TestSet_Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, null);
			TestSetView testSetView = testSetManager.RetrieveById(PROJECT_ID, newTestSetId);
			Assert.AreEqual("Testing Cycle for Release 1.0 - Copy", testSetView.Name);
			Assert.AreEqual(null, testSetView.TestSetFolderId);
			Assert.AreEqual("Fred Bloggs", testSetView.CreatorName);
			Assert.AreEqual("Joe P Smith", testSetView.OwnerName);
			Assert.AreEqual("1.0.0.0", testSetView.ReleaseVersionNumber);
			//Assert.IsTrue(testSetView.PlannedDate.Value >= DateTime.UtcNow.AddDays(-88) && testSetView.PlannedDate.Value <= DateTime.UtcNow.AddDays(-86));
			Assert.AreEqual("In Progress", testSetView.TestSetStatusName);
			Assert.AreEqual(7, testSetView.CountNotRun);
			Assert.AreEqual(0, testSetView.CountFailed);
			Assert.AreEqual(0, testSetView.CountCaution);

			//Verify the parameters copied
			List<TestSetParameter> sourceTestSetParameters = testSetManager.RetrieveParameterValues(1);
			List<TestSetParameter> destTestSetParameters = testSetManager.RetrieveParameterValues(newTestSetId);
			Assert.AreEqual(sourceTestSetParameters.Count, destTestSetParameters.Count);

			//Now delete the new test set
			testSetManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, newTestSetId);
			testSetManager.DeleteFromDatabase(newTestSetId, USER_ID_FRED_BLOGGS);

			//First lets copy a test set to a new folder
			int newTestSetFolderId = testSetManager.TestSetFolder_Create("Test Folder", PROJECT_ID, null).TestSetFolderId;
			newTestSetId = testSetManager.TestSet_Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, newTestSetFolderId);
			testSetView = testSetManager.RetrieveById(PROJECT_ID, newTestSetId);
			Assert.AreEqual("Testing Cycle for Release 1.0 - Copy", testSetView.Name);
			Assert.AreEqual(newTestSetFolderId, testSetView.TestSetFolderId);
			Assert.AreEqual("Fred Bloggs", testSetView.CreatorName);
			Assert.AreEqual("Joe P Smith", testSetView.OwnerName);
			Assert.AreEqual("1.0.0.0", testSetView.ReleaseVersionNumber);
			//Assert.IsTrue(testSetView.PlannedDate.Value >= DateTime.UtcNow.AddDays(-88) && testSetView.PlannedDate.Value <= DateTime.UtcNow.AddDays(-86));
			Assert.AreEqual("In Progress", testSetView.TestSetStatusName);
			Assert.AreEqual(7, testSetView.CountNotRun);
			Assert.AreEqual(0, testSetView.CountFailed);
			Assert.AreEqual(0, testSetView.CountCaution);

			//Verify the folder counts
			TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(newTestSetFolderId);
			Assert.AreEqual(7, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountCaution);

			//Now delete the new test set
			testSetManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, newTestSetId);
			testSetManager.DeleteFromDatabase(newTestSetId, USER_ID_FRED_BLOGGS);

			//Verify the folder counts
			testSetFolder = testSetManager.TestSetFolder_GetById(newTestSetFolderId);
			Assert.AreEqual(0, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountCaution);

			//Now copy a whole folder into this folder
			int newCopiedTestSetFolderId = testSetManager.TestSetFolder_Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, newTestSetFolderId);

			//Verify the folder counts
			testSetFolder = testSetManager.TestSetFolder_GetById(newCopiedTestSetFolderId);
			Assert.AreEqual(22, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountPassed);
			testSetFolder = testSetManager.TestSetFolder_GetById(newTestSetFolderId);
			Assert.AreEqual(22, testSetFolder.CountNotRun);
			Assert.AreEqual(0, testSetFolder.CountFailed);
			Assert.AreEqual(0, testSetFolder.CountPassed);

			//Verify the individual test sets
			List<TestSetView> testSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, 0, newCopiedTestSetFolderId);
			Assert.AreEqual(4, testSets.Count);
			Assert.AreEqual(2, testSets[0].CountNotRun);
			Assert.AreEqual(7, testSets[1].CountNotRun);
			Assert.AreEqual(9, testSets[2].CountNotRun);
			Assert.AreEqual(4, testSets[3].CountNotRun);

			//Delete the test sets
			foreach (TestSetView testSet in testSets)
			{
				testSetManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, testSet.TestSetId);
				testSetManager.DeleteFromDatabase(testSet.TestSetId, USER_ID_FRED_BLOGGS);
			}

			//Delete the folders
			testSetManager.TestSetFolder_Delete(PROJECT_ID, newCopiedTestSetFolderId, USER_ID_FRED_BLOGGS);
			testSetManager.TestSetFolder_Delete(PROJECT_ID, newTestSetFolderId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(345)
		]
		public void _08_CreateTestSetFromRelease()
		{
			//Lets test that we can create a new test set from an existing release
			int testSetId = testSetManager.CreateFromRelease(PROJECT_ID, 1, USER_ID_FRED_BLOGGS);

			//Verify the data (including the execution data)
			TestSetView testSetRow = testSetManager.RetrieveById(PROJECT_ID, testSetId);
			Assert.AreEqual("New test set based on release 1.0.0.0", testSetRow.Name);
			Assert.AreEqual("Library System Release 1", testSetRow.Description);
			Assert.AreEqual("Fred Bloggs", testSetRow.CreatorName);
			Assert.IsTrue(testSetRow.OwnerName.IsNull());
			Assert.AreEqual("1.0.0.0", testSetRow.ReleaseVersionNumber);
			Assert.IsTrue(testSetRow.PlannedDate.IsNull());
			Assert.AreEqual("Not Started", testSetRow.TestSetStatusName);
			Assert.AreEqual(7, testSetRow.CountNotRun);
			Assert.AreEqual(0, testSetRow.CountFailed);
			Assert.AreEqual(0, testSetRow.CountCaution);

			//Now delete the new test set
			testSetManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, testSetId);
			testSetManager.DeleteFromDatabase(testSetId, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Verify that multiple, concurrent updates to test sets are handled correctly
		/// in accordance with optimistic concurrency rules
		/// </summary>
		[
		Test,
		SpiraTestCase(669)
		]
		public void _09_Concurrency_Handling()
		{
			//First we need to create a new test set to verify the handling
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);

			//Now retrieve the testSet back and keep a copy of the dataset
			TestSet testSet1 = testSetManager.RetrieveById2(projectId, testSetId);
			TestSet testSet2 = testSetManager.RetrieveById2(projectId, testSetId);

			//Now make a change to field and update
			testSet1.StartTracking();
			testSet1.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Blocked;
			testSetManager.Update(testSet1, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			TestSet testSet3 = testSetManager.RetrieveById2(projectId, testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Blocked, testSet3.TestSetStatusId);

			//Now try making a change using the out of date dataset (has the wrong ConcurrencyDate)
			bool exceptionThrown = false;
			try
			{
				testSet2.StartTracking();
				testSet2.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Deferred;
				testSetManager.Update(testSet2, USER_ID_FRED_BLOGGS);
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			testSet3 = testSetManager.RetrieveById2(projectId, testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Blocked, testSet3.TestSetStatusId);

			//Now refresh the old dataset and try again and verify it works
			testSet2 = testSetManager.RetrieveById2(projectId, testSetId);
			testSet2.StartTracking();
			testSet2.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Deferred;
			testSetManager.Update(testSet2, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			testSet3 = testSetManager.RetrieveById2(projectId, testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Deferred, testSet3.TestSetStatusId);
		}

		/// <summary>
		/// Tests that we can specify the parameters for test cases when they are part of a test set
		/// </summary>
		[
		Test,
		SpiraTestCase(684)
		]
		public void _10_TestCaseParameters()
		{
			//First lets create a new test set to work with and verify that it has no test cases
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);
			List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);
			Assert.AreEqual(0, testSetTestCases.Count);

			//Also lets create two new test cases
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Sample Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Sample Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			//Lets add some parameters to the first test case
			testCaseManager.InsertParameter(projectId, testCaseId1, "param1", "default1", USER_ID_FRED_BLOGGS);
			testCaseManager.InsertParameter(projectId, testCaseId1, "param2", "default2", USER_ID_FRED_BLOGGS);

			//Lets add a parameter to the second test case
			testCaseManager.InsertParameter(projectId, testCaseId2, "param3", "default3", USER_ID_FRED_BLOGGS);

			//Now lets have the second test case call the first one as a link and specify one of the values
			Dictionary<string, string> parameterValues = new Dictionary<string, string>();
			parameterValues["param1"] = "value1A";
			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId2, null, testCaseId1, parameterValues);

			//Now lets test that we can add a test case to the test set and specify a override value for the parameter
			parameterValues.Clear();
			parameterValues["param1"] = "value1B";
			parameterValues["param2"] = "value2B";
			parameterValues["param3"] = "value3B";
			testSetManager.AddTestCase(projectId, testSetId, testCaseId2, null, null, parameterValues);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId);
			Assert.AreEqual(1, testSetTestCases.Count);
			Assert.AreEqual("Sample Test Case 2", testSetTestCases[0].Name);
			Assert.IsTrue(testSetTestCases[0].OwnerName.IsNull());
			int testSetTestCaseId = testSetTestCases[0].TestSetTestCaseId;

			//Verify the parameter values
			List<TestSetTestCaseParameter> testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
			Assert.AreEqual(3, testSetTestCaseParameters.Count);
			Assert.AreEqual("param1", testSetTestCaseParameters[0].Name);
			Assert.AreEqual("value1B", testSetTestCaseParameters[0].Value);
			Assert.AreEqual("param2", testSetTestCaseParameters[1].Name);
			Assert.AreEqual("value2B", testSetTestCaseParameters[1].Value);
			Assert.AreEqual("param3", testSetTestCaseParameters[2].Name);
			Assert.AreEqual("value3B", testSetTestCaseParameters[2].Value);

			//Now test that we can update the parameter values
			testSetTestCaseParameters[0].StartTracking();
			testSetTestCaseParameters[0].Value = "value1C";
			testSetTestCaseParameters[1].StartTracking();
			testSetTestCaseParameters[1].Value = "value2C";
			testSetManager.SaveTestCaseParameterValues(testSetTestCaseParameters);

			//Verify the parameters
			testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
			Assert.AreEqual(3, testSetTestCaseParameters.Count);
			Assert.AreEqual("param1", testSetTestCaseParameters[0].Name);
			Assert.AreEqual("value1C", testSetTestCaseParameters[0].Value);
			Assert.AreEqual("param2", testSetTestCaseParameters[1].Name);
			Assert.AreEqual("value2C", testSetTestCaseParameters[1].Value);
			Assert.AreEqual("param3", testSetTestCaseParameters[2].Name);
			Assert.AreEqual("value3B", testSetTestCaseParameters[2].Value);

			//Test that we can remove a value then add it back
			int deletedTestCaseParameterId = testSetTestCaseParameters[2].TestCaseParameterId;
			testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
			testSetTestCaseParameters.Remove(testSetTestCaseParameters[2]);
			testSetManager.SaveTestCaseParameterValues(testSetTestCaseParameters);
			testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
			Assert.AreEqual(2, testSetTestCaseParameters.Count);
			Assert.AreEqual("param1", testSetTestCaseParameters[0].Name);
			Assert.AreEqual("value1C", testSetTestCaseParameters[0].Value);
			Assert.AreEqual("param2", testSetTestCaseParameters[1].Name);
			Assert.AreEqual("value2C", testSetTestCaseParameters[1].Value);

			testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
			testSetTestCaseParameters.Add(new TestSetTestCaseParameter() { TestSetTestCaseId = testSetTestCaseId, TestCaseParameterId = deletedTestCaseParameterId, Value = "value3C" });
			testSetManager.SaveTestCaseParameterValues(testSetTestCaseParameters);
			testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
			Assert.AreEqual(3, testSetTestCaseParameters.Count);
			Assert.AreEqual("param1", testSetTestCaseParameters[0].Name);
			Assert.AreEqual("value1C", testSetTestCaseParameters[0].Value);
			Assert.AreEqual("param2", testSetTestCaseParameters[1].Name);
			Assert.AreEqual("value2C", testSetTestCaseParameters[1].Value);
			Assert.AreEqual("param3", testSetTestCaseParameters[2].Name);
			Assert.AreEqual("value3C", testSetTestCaseParameters[2].Value);

			//Test that copying the test set also copies the test case parameters
			int testSetId2 = testSetManager.TestSet_Copy(USER_ID_FRED_BLOGGS, projectId, testSetId, null);
			testSetTestCases = testSetManager.RetrieveTestCases(testSetId2);
			testSetTestCaseId = testSetTestCases[0].TestSetTestCaseId;
			testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
			Assert.AreEqual(3, testSetTestCaseParameters.Count);
			Assert.AreEqual("param1", testSetTestCaseParameters[0].Name);
			Assert.AreEqual("value1C", testSetTestCaseParameters[0].Value);
			Assert.AreEqual("param2", testSetTestCaseParameters[1].Name);
			Assert.AreEqual("value2C", testSetTestCaseParameters[1].Value);
			Assert.AreEqual("param3", testSetTestCaseParameters[2].Name);
			Assert.AreEqual("value3C", testSetTestCaseParameters[2].Value);

			//Finally verify that we can remove all the parameters in one go
			testSetManager.RemoveTestCaseParameterValues(testSetTestCaseId);
			testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
			Assert.AreEqual(0, testSetTestCaseParameters.Count);
		}

		/// <summary>
		/// Tests the handling of the test set (not test case-level) parameters
		/// </summary>
		[
		Test,
		SpiraTestCase(1409)
		]
		public void _11_TestSetParameters()
		{
			//First lets create a new test set to work with and verify that it has no test cases
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);
			List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);
			Assert.AreEqual(0, testSetTestCases.Count);

			//Also lets create two new test cases
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Sample Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Sample Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			//Lets add some parameters to the first test case
			int testCaseParameterId1 = testCaseManager.InsertParameter(projectId, testCaseId1, "param1", "default1", USER_ID_FRED_BLOGGS);
			int testCaseParameterId2 = testCaseManager.InsertParameter(projectId, testCaseId1, "param2", "default2", USER_ID_FRED_BLOGGS);

			//Lets add parameters to the second test case
			int testCaseParameterId3 = testCaseManager.InsertParameter(projectId, testCaseId2, "param1", null, USER_ID_FRED_BLOGGS);
			int testCaseParameterId4 = testCaseManager.InsertParameter(projectId, testCaseId2, "param3", "default3", USER_ID_FRED_BLOGGS);

			//Now lets test that we can add the test cases to the test set (no values specified)
			testSetManager.AddTestCases(projectId, testSetId, new List<int>() { testCaseId1, testCaseId2 }, null, null, USER_ID_FRED_BLOGGS);

			//Verify that no values are currently set
			List<TestSetParameter> testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);
			Assert.AreEqual(0, testSetParameterValues.Count);

			//Add some values
			testSetManager.AddTestSetParameter(testSetId, testCaseParameterId1, "value1", projectId, USER_ID_FRED_BLOGGS);
			testSetManager.AddTestSetParameter(testSetId, testCaseParameterId2, "value2", projectId, USER_ID_FRED_BLOGGS);
			testSetManager.AddTestSetParameter(testSetId, testCaseParameterId3, "value3", projectId, USER_ID_FRED_BLOGGS);
			testSetManager.AddTestSetParameter(testSetId, testCaseParameterId4, "value4", projectId, USER_ID_FRED_BLOGGS);

			//Verify the values are set
			testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);
			Assert.AreEqual(4, testSetParameterValues.Count);
			Assert.AreEqual("param1", testSetParameterValues[0].Name);
			Assert.AreEqual("value1", testSetParameterValues[0].Value);
			Assert.AreEqual("param1", testSetParameterValues[1].Name);
			Assert.AreEqual("value3", testSetParameterValues[1].Value);
			Assert.AreEqual("param2", testSetParameterValues[2].Name);
			Assert.AreEqual("value2", testSetParameterValues[2].Value);
			Assert.AreEqual("param3", testSetParameterValues[3].Name);
			Assert.AreEqual("value4", testSetParameterValues[3].Value);

			//Verify that you can retrieve all places a specific test case parameter is set on a test set
			testSetParameterValues = testSetManager.RetrieveParameterValuesByParameter(testCaseParameterId1);
			Assert.AreEqual(1, testSetParameterValues.Count);
			Assert.AreEqual("param1", testSetParameterValues[0].Name);
			Assert.AreEqual("value1", testSetParameterValues[0].Value);

			testSetParameterValues = testSetManager.RetrieveParameterValuesByParameter(testCaseParameterId4 + 99999);
			Assert.AreEqual(0, testSetParameterValues.Count);

			//Update a value
			testSetManager.UpdateTestSetParameter(testSetId, testCaseParameterId1, "value1a", USER_ID_FRED_BLOGGS);

			//Verify
			testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);
			Assert.AreEqual(4, testSetParameterValues.Count);
			Assert.AreEqual("param1", testSetParameterValues[0].Name);
			Assert.AreEqual("value1a", testSetParameterValues[0].Value);
			Assert.AreEqual("param1", testSetParameterValues[1].Name);
			Assert.AreEqual("value3", testSetParameterValues[1].Value);
			Assert.AreEqual("param2", testSetParameterValues[2].Name);
			Assert.AreEqual("value2", testSetParameterValues[2].Value);
			Assert.AreEqual("param3", testSetParameterValues[3].Name);
			Assert.AreEqual("value4", testSetParameterValues[3].Value);

			//Unset a value
			testSetManager.DeleteTestSetParameter(testSetId, testCaseParameterId3, projectId, USER_ID_FRED_BLOGGS);

			//Verify
			testSetParameterValues = testSetManager.RetrieveParameterValues(testSetId);
			Assert.AreEqual(3, testSetParameterValues.Count);
			Assert.AreEqual("param1", testSetParameterValues[0].Name);
			Assert.AreEqual("value1a", testSetParameterValues[0].Value);
			Assert.AreEqual("param2", testSetParameterValues[1].Name);
			Assert.AreEqual("value2", testSetParameterValues[1].Value);
			Assert.AreEqual("param3", testSetParameterValues[2].Name);
			Assert.AreEqual("value4", testSetParameterValues[2].Value);
		}

		/// <summary>
		/// Tests that we can specify the automation host and planned date for an automated test set
		/// </summary>
		[
		Test,
		SpiraTestCase(714)
		]
		public void _12_AutomatedTestExecution()
		{
			//Create a new host
			AutomationManager automationManager = new AutomationManager();
			int automationHostId = automationManager.InsertHost(projectId, "Windows Server 1", "Win1", "", true, USER_ID_FRED_BLOGGS);

			//Create a new test and point it to the automation host
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, DateTime.Now, TestRun.TestRunTypeEnum.Automated, automationHostId, null);

			//Verify the data
			TestSetView testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual("Test Set 1", testSetView.Name);
			Assert.AreEqual(automationHostId, testSetView.AutomationHostId);
			Assert.AreEqual("Windows Server 1", testSetView.AutomationHostName);
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Automated, testSetView.TestRunTypeId);
			Assert.AreEqual("Automated", testSetView.TestRunTypeName);

			//Test that we can turn it back to a manual test
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.AutomationHostId = null;
			testSet.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Manual;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Verify the data
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual("Test Set 1", testSetView.Name);
			Assert.IsTrue(testSetView.AutomationHostId.IsNull());
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Manual, testSetView.TestRunTypeId);
			Assert.AreEqual("Manual", testSetView.TestRunTypeName);

			//Test that we can turn it back to an automated test
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.AutomationHostId = automationHostId;
			testSet.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Automated;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Verify the data
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual("Test Set 1", testSetView.Name);
			Assert.AreEqual(automationHostId, testSetView.AutomationHostId);
			Assert.AreEqual("Windows Server 1", testSetView.AutomationHostName);
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Automated, testSetView.TestRunTypeId);
			Assert.AreEqual("Automated", testSetView.TestRunTypeName);

			//Now delete the host, verifying that the relationship to the test set is handled correctly
			automationManager.MarkHostAsDeleted(projectId, automationHostId, USER_ID_FRED_BLOGGS);
			automationManager.DeleteHostFromDatabase(automationHostId, USER_ID_FRED_BLOGGS);

			//Delete the test set
			testSetManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testSetId);
		}

		/// <summary>
		/// Tests that we can create a test set from a requirement
		/// </summary>
		[
		Test,
		SpiraTestCase(768)
		]
		public void _13_CreateFromRequirement()
		{
			//First lets create a new requirement in the empty project
			RequirementManager requirementManager = new RequirementManager();
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Test Requirement", null, 120, USER_ID_FRED_BLOGGS);

			//Now lets add two test cases to it
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			List<int> testCases = new List<int>();
			testCases.Add(testCaseId1);
			testCases.Add(testCaseId2);
			testCaseManager.AddToRequirement(projectId, requirementId, testCases, 1);

			//Then lets create a test set from the requirement - it should contain the two test cases
			List<int> requirements = new List<int>();
			requirements.Add(requirementId);
			int testSetId = testSetManager.CreateFromRequirements(USER_ID_FRED_BLOGGS, projectId, requirements);
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId, false, true);
			Assert.IsNotNull(testSet);
			Assert.AreEqual(2, testSet.TestSetTestCases.Count);
			Assert.AreEqual("Sample Test Case 1", testSet.TestSetTestCases[0].TestCase.Name);
			Assert.AreEqual("Sample Test Case 2", testSet.TestSetTestCases[1].TestCase.Name);
		}

		/// <summary>
		/// Tests that you can have test sets with recurring schedules
		/// </summary>
		[
		Test,
		SpiraTestCase(806)
		]
		public void _14_RecurringSchedules()
		{
			//First we need to create a new test set with a recurrence specified
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, DateTime.Now, TestRun.TestRunTypeEnum.Manual, null, TestSet.RecurrenceEnum.Hourly);

			//Verify recurrence
			TestSetView testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual((int)TestSet.RecurrenceEnum.Hourly, testSetView.RecurrenceId);
			Assert.AreEqual("Hourly", testSetView.RecurrenceName);

			//Now test that we can change the recurrence
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.RecurrenceId = (int)TestSet.RecurrenceEnum.Monthly;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Verify recurrence
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual((int)TestSet.RecurrenceEnum.Monthly, testSetView.RecurrenceId);
			Assert.AreEqual("Monthly", testSetView.RecurrenceName);

			//Now test that we can unset the recurrence
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.RecurrenceId = null;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Verify recurrence
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.IsTrue(testSetView.RecurrenceId.IsNull());

			//Now we add a test case, set the recurrence and assign the test set
			//Make sure that concurrency errors are not thrown when we do it in this order
			TestCaseManager testCaseManager = new TestCaseManager();
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Sample Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Step 1", "Works", "");
			testSetManager.AddTestCase(projectId, testSetId, testCaseId, null, null);
			testSet.StartTracking();
			testSet.RecurrenceId = (int)TestSet.RecurrenceEnum.Daily;
			testSet.PlannedDate = DateTime.Now.Date;
			testSet.OwnerId = USER_ID_FRED_BLOGGS;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Now execute the test set and verify that the status has switched back to 'Not Run'
			//and the Planned Date has been appropriately advanced. The owner field should be left
			TestRunManager testRunManager = new TestRunManager();
			TestRunsPending testRunsPending = testRunManager.CreateFromTestSet(USER_ID_FRED_BLOGGS, projectId, testSetId, true);
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, DateTime.Now, true);
			testRunsPending = testRunManager.RetrievePendingById(testRunsPending.TestRunsPendingId, false);
			testRunManager.CompletePending(testRunsPending.TestRunsPendingId, USER_ID_FRED_BLOGGS);

			//Verify the test set
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual((int)TestSet.RecurrenceEnum.Daily, testSetView.RecurrenceId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.NotStarted, testSetView.TestSetStatusId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, testSetView.OwnerId);
			Assert.AreEqual(DateTime.Now.Date.AddDays(1), testSetView.PlannedDate);

			//Verify that the correct history records were also created
			//(since there was an issue with this previously - [IN:4539]
			HistoryManager historyManager = new HistoryManager();
			HistoryChangeSetResponse historyChangeSet = historyManager.RetrieveLastChangeSetForArtifactId(testSetId, Artifact.ArtifactTypeEnum.TestSet, true);
			Assert.IsNotNull(historyChangeSet);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSet.ChangeTypeId);

			//We should have a record for status = Completed > Not Started
			//HistoryDetail historyDetail = historyChangeSet.Details.FirstOrDefault(h => h.FieldName == "TestSetStatusId");
			//Assert.IsNotNull(historyDetail);
			//Assert.AreEqual((int)TestSet.TestSetStatusEnum.Completed, historyDetail.OldValueInt);
			//Assert.AreEqual((int)TestSet.TestSetStatusEnum.NotStarted, historyDetail.NewValueInt);

			//Now remove the recurrence and execute the test set again
			//This time the planned date and owner should be blank and the status should remain Completed
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.RecurrenceId = null;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);
			testRunsPending = testRunManager.CreateFromTestSet(USER_ID_FRED_BLOGGS, projectId, testSetId, true);
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, DateTime.Now, true);
			testRunsPending = testRunManager.RetrievePendingById(testRunsPending.TestRunsPendingId, false);
			testRunManager.CompletePending(testRunsPending.TestRunsPendingId, USER_ID_FRED_BLOGGS);

			//Verify the test set
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.IsTrue(testSetView.RecurrenceId.IsNull());
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Completed, testSetView.TestSetStatusId);
			Assert.IsTrue(testSetView.OwnerId.IsNull());
			Assert.IsTrue(testSetView.PlannedDate.IsNull());

			//Now set the recurrence again and try an automated test run
			//This is the case that RemoteLaunch uses
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.RecurrenceId = (int)TestSet.RecurrenceEnum.Daily;
			testSet.PlannedDate = DateTime.Now.Date;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId, null, testSetId, null, DateTime.Now, DateTime.Now, (int)TestCase.ExecutionStatusEnum.Passed, "Test Suite", "Test", 0, "OK", "OK", null, null, null, TestRun.TestRunFormatEnum.PlainText, null);
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Completed;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Verify the test set
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual((int)TestSet.RecurrenceEnum.Daily, testSetView.RecurrenceId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.NotStarted, testSetView.TestSetStatusId);
			Assert.AreEqual(DateTime.Now.Date.AddDays(1), testSetView.PlannedDate);
		}

		[
		Test,
		SpiraTestCase(340)
		]
		public void _15_RetrieveTestCasesByTestSet()
		{
			//Test that we can retrieve the test cases for a specific test set, both for specific release
			//and also where no release is specified

			//Retrieve for a test set, specifying no release
			List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases3(PROJECT_ID, 1, null);
			Assert.AreEqual(7, testSetTestCases.Count);
			Assert.AreEqual("Ability to create new book", testSetTestCases[0].Name);
			Assert.AreEqual("Failed", testSetTestCases[0].ExecutionStatusName);
			Assert.AreEqual(10, testSetTestCases[0].EstimatedDuration);
			Assert.AreEqual(75, testSetTestCases[0].ActualDuration);
			Assert.AreEqual("Ability to edit existing book", testSetTestCases[1].Name);
			Assert.AreEqual("Passed", testSetTestCases[1].ExecutionStatusName);
			Assert.AreEqual(5, testSetTestCases[1].EstimatedDuration);
			Assert.AreEqual(90, testSetTestCases[1].ActualDuration);
			Assert.AreEqual("Author management", testSetTestCases[6].Name);
			Assert.AreEqual("Not Run", testSetTestCases[6].ExecutionStatusName);

			//Retrieve for a test set, specifying a release
			testSetTestCases = testSetManager.RetrieveTestCases3(PROJECT_ID, 1, 1);
			Assert.AreEqual(7, testSetTestCases.Count);
			Assert.AreEqual("Ability to create new book", testSetTestCases[0].Name);
			Assert.AreEqual("Failed", testSetTestCases[0].ExecutionStatusName);
			Assert.AreEqual("Ability to edit existing book", testSetTestCases[1].Name);
			Assert.AreEqual("Passed", testSetTestCases[1].ExecutionStatusName);
			Assert.AreEqual("Author management", testSetTestCases[6].Name);
			Assert.AreEqual("Not Run", testSetTestCases[6].ExecutionStatusName);

			//Verify that we can also retrieve it by its unique TestSetTestCaseId
			int testSetTestCaseId = testSetTestCases[0].TestSetTestCaseId;
			TestSetTestCase testSetTestCase = testSetManager.RetrieveTestCaseById(testSetTestCaseId);
			Assert.AreEqual("Ability to create new book", testSetTestCase.TestCase.Name);
		}
	}
}
