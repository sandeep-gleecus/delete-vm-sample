using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
	public class TestCaseTest
	{
		protected static TestCaseManager testCaseManager;

		protected static int projectId;
		protected static int projectTemplateId;

		protected static int testCaseId1;
		protected static int testCaseId2;
		protected static int testCaseId3;
		protected static int testCaseId4;
		protected static int testCaseId5;
		protected static int testCaseId6;
		protected static int testCaseId7;
		protected static int testCaseId8;

		protected static int testFolderId1;
		protected static int testFolderId2;
		protected static int testFolderId3;
		protected static int testFolderId4;
		protected static int testFolderId5;
		protected static int testFolderId6;
		protected static int testFolderId7;
		protected static int testFolderId8;
		protected static int testFolderId9;
		protected static int testFolderId10;

		protected static int testStepId1;
		protected static int testStepId2;
		protected static int testStepId3;
		protected static int testStepId4;

		private const int PROJECT_ID = 1;
		private const int PROJECT_TEMPLATE_ID = 1;

		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYSTEM_ADMIN = 1;

		private static int tc_priorityCriticalId;
		private static int tc_priorityHighId;
		private static int tc_priorityMediumId;
		private static int tc_priorityLowId;

		private static int tc_typeScenarioId;
		private static int tc_typeAcceptanceId;
		private static int tc_typeUnitId;
		private static int tc_typeIntegrationId;

		[TestFixtureSetUp]
		public void Init()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Create the test manager business
			testCaseManager = new TestCaseManager();

			//Create a new project for testing with
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("TestCaseTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the test case priorities for this template
			List<TestCasePriority> priorities = testCaseManager.TestCasePriority_Retrieve(projectTemplateId);
			tc_priorityCriticalId = priorities.FirstOrDefault(p => p.Score == 1).TestCasePriorityId;
			tc_priorityHighId = priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId;
			tc_priorityMediumId = priorities.FirstOrDefault(p => p.Score == 3).TestCasePriorityId;
			tc_priorityLowId = priorities.FirstOrDefault(p => p.Score == 4).TestCasePriorityId;

			//Get the test case types for this template
			List<TestCaseType> types = testCaseManager.TestCaseType_Retrieve(projectTemplateId);
			tc_typeScenarioId = types.FirstOrDefault(t => t.Name == "Scenario").TestCaseTypeId;
			tc_typeAcceptanceId = types.FirstOrDefault(t => t.Name == "Acceptance").TestCaseTypeId;
			tc_typeIntegrationId = types.FirstOrDefault(t => t.Name == "Integration").TestCaseTypeId;
			tc_typeUnitId = types.FirstOrDefault(t => t.Name == "Unit").TestCaseTypeId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYSTEM_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId);
		}

		[
		Test,
		SpiraTestCase(233)
		]
		public void _00_RetrieveSummaryData()
		{
			//First get the test execution coverage summary untyped dataset - all releases
			List<TestCase_ExecutionStatusSummary> executionStatusSummary = testCaseManager.RetrieveExecutionStatusSummary(PROJECT_ID, null);
			//Make sure the data is as expected
			Assert.AreEqual(7, executionStatusSummary.Count);
			Assert.AreEqual("Failed", executionStatusSummary[0].ExecutionStatusName);
			Assert.AreEqual(2, executionStatusSummary[0].StatusCount);
			Assert.AreEqual("Passed", executionStatusSummary[1].ExecutionStatusName);
			Assert.AreEqual(4, executionStatusSummary[1].StatusCount);
			Assert.AreEqual("Not Run", executionStatusSummary[2].ExecutionStatusName);
			Assert.AreEqual(7, executionStatusSummary[2].StatusCount);
			Assert.AreEqual("Blocked", executionStatusSummary[4].ExecutionStatusName);
			Assert.AreEqual(0, executionStatusSummary[4].StatusCount);
			Assert.AreEqual("Caution", executionStatusSummary[5].ExecutionStatusName);
			Assert.AreEqual(2, executionStatusSummary[5].StatusCount);

			//First get the test execution status summary untyped dataset - specific release
			executionStatusSummary = testCaseManager.RetrieveExecutionStatusSummary(PROJECT_ID, 1);
			//Make sure the data is as expected
			Assert.AreEqual(6, executionStatusSummary.Count);
			Assert.AreEqual("Failed", executionStatusSummary[0].ExecutionStatusName);
			Assert.AreEqual(2, executionStatusSummary[0].StatusCount);
			Assert.AreEqual("Passed", executionStatusSummary[1].ExecutionStatusName);
			Assert.AreEqual(6, executionStatusSummary[1].StatusCount);
			Assert.AreEqual("Not Run", executionStatusSummary[2].ExecutionStatusName);
			Assert.AreEqual(0, executionStatusSummary[2].StatusCount);
			Assert.AreEqual("Blocked", executionStatusSummary[3].ExecutionStatusName);
			Assert.AreEqual(0, executionStatusSummary[3].StatusCount);
			Assert.AreEqual("Caution", executionStatusSummary[4].ExecutionStatusName);
			Assert.AreEqual(1, executionStatusSummary[4].StatusCount);

			//Next get the test execution coverage summary untyped dataset - active releases
			executionStatusSummary = testCaseManager.RetrieveExecutionStatusSummary(PROJECT_ID, TestCaseManager.RELEASE_ID_ACTIVE_RELEASES_ONLY);
			//Make sure the data is as expected
			Assert.AreEqual(6, executionStatusSummary.Count);
			Assert.AreEqual("Failed", executionStatusSummary[0].ExecutionStatusName);
			Assert.AreEqual(6, executionStatusSummary[0].StatusCount);
			Assert.AreEqual("Passed", executionStatusSummary[1].ExecutionStatusName);
			Assert.AreEqual(14, executionStatusSummary[1].StatusCount);
			Assert.AreEqual("Not Run", executionStatusSummary[2].ExecutionStatusName);
			Assert.AreEqual(17, executionStatusSummary[2].StatusCount);
			Assert.AreEqual("Blocked", executionStatusSummary[3].ExecutionStatusName);
			Assert.AreEqual(1, executionStatusSummary[3].StatusCount);
			Assert.AreEqual("Caution", executionStatusSummary[4].ExecutionStatusName);
			Assert.AreEqual(5, executionStatusSummary[4].StatusCount);

			//Now get the list of tests owned by a particular user - cross project
			List<TestCaseView> testCases = testCaseManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual(6, testCases.Count);
			Assert.AreEqual("Ability to create new book", testCases[0].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[0].ExecutionStatusId);
			Assert.AreEqual("Ability to edit existing book", testCases[1].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[1].ExecutionStatusId);
			Assert.AreEqual("Library Information System (Sample)", testCases[1].ProjectName);

			//Now get the list of tests owned by a particular user - specific project
			testCases = testCaseManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, PROJECT_ID);
			Assert.AreEqual(2, testCases.Count);
			Assert.AreEqual("Ability to create new book", testCases[0].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[0].ExecutionStatusId);
			Assert.AreEqual("Ability to edit existing book", testCases[1].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[1].ExecutionStatusId);
			Assert.AreEqual("Library Information System (Sample)", testCases[1].ProjectName);

			//Now test that we can get the project-group test execution summary - for project
			executionStatusSummary = testCaseManager.RetrieveExecutionStatusSummary(2, false);
			Assert.AreEqual(6, executionStatusSummary.Count);
			Assert.AreEqual("Failed", executionStatusSummary[0].ExecutionStatusName);
			Assert.AreEqual(2, executionStatusSummary[0].StatusCount);
			Assert.AreEqual("Passed", executionStatusSummary[1].ExecutionStatusName);
			Assert.AreEqual(4, executionStatusSummary[1].StatusCount);
			Assert.AreEqual("Not Run", executionStatusSummary[2].ExecutionStatusName);
			Assert.AreEqual(37, executionStatusSummary[2].StatusCount);
			Assert.AreEqual("Blocked", executionStatusSummary[3].ExecutionStatusName);
			Assert.AreEqual(0, executionStatusSummary[3].StatusCount);
			Assert.AreEqual("Caution", executionStatusSummary[4].ExecutionStatusName);
			Assert.AreEqual(2, executionStatusSummary[4].StatusCount);

			//Now test that we can get the project-group test execution summary - for all active releases
			executionStatusSummary = testCaseManager.RetrieveExecutionStatusSummary(2, true);
			Assert.AreEqual(6, executionStatusSummary.Count);
			Assert.AreEqual("Failed", executionStatusSummary[0].ExecutionStatusName);
			Assert.AreEqual(6, executionStatusSummary[0].StatusCount);
			Assert.AreEqual("Passed", executionStatusSummary[1].ExecutionStatusName);
			Assert.AreEqual(14, executionStatusSummary[1].StatusCount);
			Assert.AreEqual("Not Run", executionStatusSummary[2].ExecutionStatusName);
			Assert.AreEqual(17, executionStatusSummary[2].StatusCount);
			Assert.AreEqual("Blocked", executionStatusSummary[3].ExecutionStatusName);
			Assert.AreEqual(1, executionStatusSummary[3].StatusCount);
			Assert.AreEqual("Caution", executionStatusSummary[4].ExecutionStatusName);
			Assert.AreEqual(5, executionStatusSummary[4].StatusCount);
		}

		/// <summary>
		/// Loads the various lookup lists
		/// </summary>
		[
		Test,
		SpiraTestCase(231)
		]
		public void _01_LoadLookups()
		{
			//Lets test that we can load the Execution Status lookup list
			List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(7, executionStati.Count);
			Assert.AreEqual(1, executionStati[0].ExecutionStatusId);
			Assert.AreEqual("Failed", executionStati[0].Name);
			Assert.AreEqual(5, executionStati[4].ExecutionStatusId);
			Assert.AreEqual("Blocked", executionStati[4].Name);
			Assert.AreEqual(6, executionStati[5].ExecutionStatusId);
			Assert.AreEqual("Caution", executionStati[5].Name);

			//Verify that we can retrieve a single execution status
			ExecutionStatus executionStatus = testCaseManager.RetrieveExecutionStatusById(2);
			Assert.AreEqual("Passed", executionStatus.Name);

			//Lets test that we can load the Test Case priority lookup list
			List<TestCasePriority> priorities = testCaseManager.TestCasePriority_Retrieve(projectTemplateId);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(4, priorities.Count);
			Assert.AreEqual(tc_priorityCriticalId, priorities[3].TestCasePriorityId);
			Assert.AreEqual("1 - Critical", priorities[3].Name);

			//Verify that we can retrieve a single priority in the sample project
			TestCasePriority priority = testCaseManager.TestCasePriority_RetrieveById(2);
			Assert.AreEqual("2 - High", priority.Name);

			//Lets test that we can load the Test Case status lookup list
			List<TestCaseStatus> stati = testCaseManager.RetrieveStatuses();
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(8, stati.Count);
			Assert.AreEqual((int)TestCase.TestCaseStatusEnum.Approved, stati[3].TestCaseStatusId);
			Assert.AreEqual("Approved", stati[3].Name);

			//Verify that we can retrieve a single status
			TestCaseStatus status = testCaseManager.RetrieveStatusById(2);
			Assert.AreEqual("Ready for Review", status.Name);

			//Lets test that we can load the Test Case type lookup list
			List<TestCaseType> types = testCaseManager.TestCaseType_Retrieve(projectTemplateId);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(12, types.Count);
			Assert.AreEqual(tc_typeIntegrationId, types[8].TestCaseTypeId);
			Assert.AreEqual("Integration", types[8].Name);

			//Verify that we can retrieve a single type
			TestCaseType type = testCaseManager.TestCaseType_RetrieveById(3);
			Assert.AreEqual("Functional", type.Name);
		}

		/// <summary>
		/// This test creates some sample test folders
		/// </summary>
		[
		Test,
		SpiraTestCase(212)
		]
		public void _02_CreateTestFolder()
		{
			//First lets test creating some new top-level test-case folders
			testFolderId1 = testCaseManager.TestCaseFolder_Create("Functional Tests", projectId, null).TestCaseFolderId;
			testFolderId2 = testCaseManager.TestCaseFolder_Create("Regression Tests", projectId, "Tests that the application works on different platforms", null).TestCaseFolderId;
			testFolderId3 = testCaseManager.TestCaseFolder_Create("Unit Tests", projectId, "All of the NUnit and jUnit automated unit tests", null).TestCaseFolderId;
			testFolderId4 = testCaseManager.TestCaseFolder_Create("Acceptance Tests", projectId, null).TestCaseFolderId;

			//Now lets retrieve the list of top-level folders ordered by name/asc
			List<TestCaseFolder> testCaseFolders = testCaseManager.TestCaseFolder_GetByParentId(projectId, null, null, true);
			Assert.AreEqual(4, testCaseFolders.Count);
			Assert.AreEqual("Acceptance Tests", testCaseFolders[0].Name);
			Assert.AreEqual("Functional Tests", testCaseFolders[1].Name);
			Assert.AreEqual("Regression Tests", testCaseFolders[2].Name);
			Assert.AreEqual("Unit Tests", testCaseFolders[3].Name);

			//Now lets retrieve a single folder and verify its properties
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(null, testCaseFolder.Description);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(0, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(null, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);
			Assert.AreEqual(null, testCaseFolder.ParentTestCaseFolderId);

			//Inserting sub folders
			testFolderId5 = testCaseManager.TestCaseFolder_Create("Reservations", projectId, "Tests the reservations process", testFolderId1).TestCaseFolderId;
			testFolderId6 = testCaseManager.TestCaseFolder_Create("Flight Status", projectId, "Tests that we can get current flight status", testFolderId1).TestCaseFolderId;
			testFolderId7 = testCaseManager.TestCaseFolder_Create("Customer Support", projectId, null, testFolderId1).TestCaseFolderId;

			testFolderId8 = testCaseManager.TestCaseFolder_Create("jUnit Tests", projectId, null, testFolderId3).TestCaseFolderId;
			testFolderId9 = testCaseManager.TestCaseFolder_Create("NUnit Tests", projectId, null, testFolderId3).TestCaseFolderId;

			//Now lets retrieve the list of sub-folders ordered by name/asc
			testCaseFolders = testCaseManager.TestCaseFolder_GetByParentId(projectId, testFolderId1, null, true);
			Assert.AreEqual(3, testCaseFolders.Count);
			Assert.AreEqual("Customer Support", testCaseFolders[0].Name);
			Assert.AreEqual("Flight Status", testCaseFolders[1].Name);
			Assert.AreEqual("Reservations", testCaseFolders[2].Name);

			testCaseFolders = testCaseManager.TestCaseFolder_GetByParentId(projectId, testFolderId3, null, true);
			Assert.AreEqual(2, testCaseFolders.Count);
			Assert.AreEqual("jUnit Tests", testCaseFolders[0].Name);
			Assert.AreEqual("NUnit Tests", testCaseFolders[1].Name);

			//Now lets retrieve a single folder and verify its properties
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId6);
			Assert.AreEqual("Flight Status", testCaseFolder.Name);
			Assert.AreEqual("Tests that we can get current flight status", testCaseFolder.Description);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(0, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(null, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);
			Assert.AreEqual(testFolderId1, testCaseFolder.ParentTestCaseFolderId);

			//Let's create a subfolder of a subfolder
			testFolderId10 = testCaseManager.TestCaseFolder_Create("NUnit API Tests", projectId, null, testFolderId9).TestCaseFolderId;

			//Now let's retrieve the parents of this test case, with and without itself
			List<TestCaseFolderHierarchyView> folderHierarchy = testCaseManager.TestCaseFolder_GetParents(projectId, testFolderId10, true);
			Assert.AreEqual(3, folderHierarchy.Count);
			Assert.AreEqual("Unit Tests", folderHierarchy[0].Name);
			Assert.AreEqual(null, folderHierarchy[0].ParentTestCaseFolderId);
			Assert.AreEqual("AAD", folderHierarchy[0].IndentLevel);
			Assert.AreEqual("NUnit Tests", folderHierarchy[1].Name);
			Assert.AreEqual(testFolderId3, folderHierarchy[1].ParentTestCaseFolderId);
			Assert.AreEqual("AADAAB", folderHierarchy[1].IndentLevel);
			Assert.AreEqual("NUnit API Tests", folderHierarchy[2].Name);
			Assert.AreEqual(testFolderId9, folderHierarchy[2].ParentTestCaseFolderId);
			Assert.AreEqual("AADAABAAA", folderHierarchy[2].IndentLevel);

			folderHierarchy = testCaseManager.TestCaseFolder_GetParents(projectId, testFolderId10, false);
			Assert.AreEqual(2, folderHierarchy.Count);
			Assert.AreEqual("Unit Tests", folderHierarchy[0].Name);
			Assert.AreEqual(null, folderHierarchy[0].ParentTestCaseFolderId);
			Assert.AreEqual("AAD", folderHierarchy[0].IndentLevel);
			Assert.AreEqual("NUnit Tests", folderHierarchy[1].Name);
			Assert.AreEqual(testFolderId3, folderHierarchy[1].ParentTestCaseFolderId);
			Assert.AreEqual("AADAAB", folderHierarchy[1].IndentLevel);

			//Now lets retrieve the whole folder hierarchy with indent levels
			folderHierarchy = testCaseManager.TestCaseFolder_GetList(projectId);
			Assert.AreEqual(10, folderHierarchy.Count);
			Assert.AreEqual("Acceptance Tests", folderHierarchy[0].Name);
			Assert.AreEqual("AAA", folderHierarchy[0].IndentLevel);
			Assert.AreEqual(null, folderHierarchy[0].ParentTestCaseFolderId);
			Assert.AreEqual("Unit Tests", folderHierarchy[6].Name);
			Assert.AreEqual("AAD", folderHierarchy[6].IndentLevel);
			Assert.AreEqual(null, folderHierarchy[6].ParentTestCaseFolderId);
			Assert.AreEqual("NUnit Tests", folderHierarchy[8].Name);
			Assert.AreEqual("AADAAB", folderHierarchy[8].IndentLevel);
			Assert.AreEqual(testFolderId3, folderHierarchy[8].ParentTestCaseFolderId);
			Assert.AreEqual("NUnit API Tests", folderHierarchy[9].Name);
			Assert.AreEqual("AADAABAAA", folderHierarchy[9].IndentLevel);
			Assert.AreEqual(testFolderId9, folderHierarchy[9].ParentTestCaseFolderId);

			//Check if a folder exists or not
			Assert.AreEqual(true, testCaseManager.TestCaseFolder_Exists(projectId, testFolderId6), "Task folder should exist");
			Assert.AreEqual(false, testCaseManager.TestCaseFolder_Exists(projectId + 1, testFolderId6), "Task folder does not exist in specified product");
			Assert.AreEqual(false, testCaseManager.TestCaseFolder_Exists(projectId, testFolderId6 + 10000000), "Task folder should NOT exist");
		}

		/// <summary>
		/// Tests that we can modify and rearrange the test case folders
		/// </summary>
		[
		Test,
		SpiraTestCase(214)
		]
		public void _03_ModifyTestFolder()
		{
			//Retrieve one of the earlier test folders
			//Then make some changes
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId6);
			testCaseFolder.StartTracking();
			testCaseFolder.Name = "Flight Status Tests";
			testCaseFolder.Description = null;
			testCaseManager.TestCaseFolder_Update(testCaseFolder);

			//Now lets make sure the changes happened
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId6);
			Assert.AreEqual("Flight Status Tests", testCaseFolder.Name);
			Assert.AreEqual(null, testCaseFolder.Description);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(0, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(null, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);
			Assert.AreEqual(testFolderId1, testCaseFolder.ParentTestCaseFolderId);

			//Now let's move the unit tests under regression tests
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId3);
			testCaseFolder.StartTracking();
			testCaseFolder.ParentTestCaseFolderId = testFolderId2;
			testCaseManager.TestCaseFolder_Update(testCaseFolder);

			//Now verify that it moved successfully
			List<TestCaseFolder> testCaseFolders = testCaseManager.TestCaseFolder_GetByParentId(projectId, null, null, true);
			Assert.AreEqual(3, testCaseFolders.Count);
			Assert.AreEqual("Acceptance Tests", testCaseFolders[0].Name);
			Assert.AreEqual("Functional Tests", testCaseFolders[1].Name);
			Assert.AreEqual("Regression Tests", testCaseFolders[2].Name);

			testCaseFolders = testCaseManager.TestCaseFolder_GetByParentId(projectId, testFolderId2, null, true);
			Assert.AreEqual(1, testCaseFolders.Count);
			Assert.AreEqual("Unit Tests", testCaseFolders[0].Name);

			//Now lets retrieve the whole folder hierarchy with the updated indent levels
			List<TestCaseFolderHierarchyView> folderHierarchy = testCaseManager.TestCaseFolder_GetList(projectId);
			Assert.AreEqual(10, folderHierarchy.Count);
			Assert.AreEqual("Acceptance Tests", folderHierarchy[0].Name);
			Assert.AreEqual("AAA", folderHierarchy[0].IndentLevel);
			Assert.AreEqual(null, folderHierarchy[0].ParentTestCaseFolderId);
			Assert.AreEqual("Regression Tests", folderHierarchy[5].Name);
			Assert.AreEqual("AAC", folderHierarchy[5].IndentLevel);
			Assert.AreEqual(null, folderHierarchy[5].ParentTestCaseFolderId);
			Assert.AreEqual("Unit Tests", folderHierarchy[6].Name);
			Assert.AreEqual("AACAAA", folderHierarchy[6].IndentLevel);
			Assert.AreEqual(testFolderId2, folderHierarchy[6].ParentTestCaseFolderId);
			Assert.AreEqual("NUnit Tests", folderHierarchy[8].Name);
			Assert.AreEqual("AACAAAAAB", folderHierarchy[8].IndentLevel);
			Assert.AreEqual(testFolderId3, folderHierarchy[8].ParentTestCaseFolderId);
			Assert.AreEqual("NUnit API Tests", folderHierarchy[9].Name);
			Assert.AreEqual("AACAAAAABAAA", folderHierarchy[9].IndentLevel);
			Assert.AreEqual(testFolderId9, folderHierarchy[9].ParentTestCaseFolderId);

			//Make sure the system doesn't let us put folders in a recursive loop
			bool exceptionThrown = false;
			try
			{
				testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId3);
				testCaseFolder.StartTracking();
				testCaseFolder.ParentTestCaseFolderId = testFolderId9;
				testCaseManager.TestCaseFolder_Update(testCaseFolder);
			}
			catch (FolderCircularReferenceException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);
		}

		/// <summary>
		/// Tests that we can delete test case folders
		/// </summary>
		[
		Test,
		SpiraTestCase(215)
		]
		public void _04_DeleteTestFolder()
		{
			//First lets add a couple of new folders, one a child of another
			int testFolderId11 = testCaseManager.TestCaseFolder_Create("Dummy Test 1", projectId, null, null).TestCaseFolderId;
			int testFolderId12 = testCaseManager.TestCaseFolder_Create("Dummy Test 2", projectId, null, testFolderId11).TestCaseFolderId;

			//Verify that we have the two folders in a hierarchy
			List<TestCaseFolderHierarchyView> testFolderHierarchy = testCaseManager.TestCaseFolder_GetParents(projectId, testFolderId12, true);
			Assert.AreEqual(2, testFolderHierarchy.Count);
			Assert.AreEqual("Dummy Test 1", testFolderHierarchy[0].Name);
			Assert.AreEqual(null, testFolderHierarchy[0].ParentTestCaseFolderId);
			Assert.AreEqual("Dummy Test 2", testFolderHierarchy[1].Name);
			Assert.AreEqual(testFolderId11, testFolderHierarchy[1].ParentTestCaseFolderId);

			//Now lets add a test case into each folder
			testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Dummy Test 1", null, null, TestCase.TestCaseStatusEnum.Draft, null, testFolderId11, null, null, null);
			testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Dummy Test 2", null, null, TestCase.TestCaseStatusEnum.Draft, null, testFolderId12, null, null, null);

			//Now lets delete both folders recursively and verify they deleted OK
			testCaseManager.TestCaseFolder_Delete(projectId, testFolderId11, USER_ID_FRED_BLOGGS);

			Assert.IsNull(testCaseManager.TestCaseFolder_GetById(testFolderId11));
			Assert.IsNull(testCaseManager.TestCaseFolder_GetById(testFolderId12));

			//Verify the two test cases are de-associated
			TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.IsNull(testCase.TestCaseFolderId);
			testCase = testCaseManager.RetrieveById(projectId, testCaseId2);
			Assert.IsNull(testCase.TestCaseFolderId);

			//Now clean up by deleting these two test cases
			testCaseManager.DeleteFromDatabase(testCaseId1, USER_ID_FRED_BLOGGS);
			testCaseManager.DeleteFromDatabase(testCaseId2, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(209)
		]
		public void _05_CreateTestCase()
		{
			//Lets add some test cases to the various folders, including the root folder

			//First lets test inserting a new test-case in the root folder
			//Also test that the name of the test case can be upto 255 characters
			//and that the description can be upto 1000 characters
			testCaseId1 = testCaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				null,
				"Simple application test" + InternalRoutines.RepeatString("tes\u05d0", 56),
				InternalRoutines.RepeatString("tes\u05d0", 250),
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				tc_priorityMediumId,
				null,
				null,
				null,
				null
				);

			//Now lets make sure we can retrieve the submitted test case
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual(testCaseId1, testCaseView.TestCaseId, "Test Case ID");
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCaseView.ExecutionStatusId, "Execution Status ID");
			Assert.AreEqual(2, testCaseView.AuthorId, "Author ID");
			Assert.IsNull(testCaseView.OwnerId);
			Assert.AreEqual("Simple application test" + InternalRoutines.RepeatString("tes\u05d0", 56), testCaseView.Name, "Name");
			Assert.AreEqual(InternalRoutines.RepeatString("tes\u05d0", 250), testCaseView.Description, "Description");
			Assert.AreEqual(null, testCaseView.ExecutionDate, "ExecutionDate");
			Assert.AreEqual("Fred Bloggs", testCaseView.AuthorName, "AuthorName");
			Assert.IsNull(testCaseView.OwnerName);
			Assert.AreEqual("Not Run", testCaseView.ExecutionStatusName, "ExecutionStatusName");
			Assert.AreEqual("Ready for Test", testCaseView.TestCaseStatusName);
			Assert.AreEqual("Functional", testCaseView.TestCaseTypeName);
			Assert.AreEqual("3 - Medium", testCaseView.TestCasePriorityName);
			Assert.AreEqual(projectId, testCaseView.ProjectId);
			Assert.IsTrue(testCaseView.EstimatedDuration.IsNull());
			Assert.IsFalse(testCaseView.IsTestSteps);
			Assert.IsNull(testCaseView.TestCaseFolderId);

			//Next lets test inserting a new test-case inside an existing test folder
			testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, "Ability to update existing book", "Description of ability to update existing book", tc_typeAcceptanceId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityCriticalId, testFolderId1, 20, null, null);

			//Now lets make sure we can retrieve the submitted test case
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId2);
			Assert.AreEqual(testCaseId2, testCaseView.TestCaseId, "Test Case ID");
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCaseView.ExecutionStatusId, "Execution Status ID");
			Assert.AreEqual(2, testCaseView.AuthorId, "Author ID");
			Assert.AreEqual(3, testCaseView.OwnerId, "Owner ID");
			Assert.AreEqual("Ability to update existing book", testCaseView.Name, "Name");
			Assert.AreEqual("Description of ability to update existing book", testCaseView.Description, "Description");
			Assert.IsNull(testCaseView.ExecutionDate);
			Assert.AreEqual("Fred Bloggs", testCaseView.AuthorName, "Author Name");
			Assert.AreEqual("Joe P Smith", testCaseView.OwnerName, "Owner Name");
			Assert.AreEqual("Not Run", testCaseView.ExecutionStatusName, "Execution Status Name");
			Assert.AreEqual(tc_priorityCriticalId, testCaseView.TestCasePriorityId, "TestCasePriorityId");
			Assert.AreEqual("1 - Critical", testCaseView.TestCasePriorityName, "TestCasePriorityName");
			Assert.AreEqual("Ready for Test", testCaseView.TestCaseStatusName);
			Assert.AreEqual("Acceptance", testCaseView.TestCaseTypeName);
			Assert.AreEqual(20, testCaseView.EstimatedDuration, "EstimatedDuration");
			Assert.IsFalse(testCaseView.IsTestSteps);
			Assert.AreEqual(testFolderId1, testCaseView.TestCaseFolderId);

			//Next lets lets inserting another test case inside this folder
			testCaseId3 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, "Ability to modify selected books", null, tc_typeScenarioId, TestCase.TestCaseStatusEnum.Approved, null, testFolderId1, null, null, null);

			//Now lets make sure we can retrieve the submitted test case
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId3);
			Assert.AreEqual("Ability to modify selected books", testCaseView.Name, "Name");
			Assert.AreEqual(testFolderId1, testCaseView.TestCaseFolderId);
			Assert.IsFalse(testCaseView.IsTestSteps);

			//Next lets lets inserting a new test case into a subfolder
			testCaseId4 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, "Ability to modify authors", null, tc_typeUnitId, TestCase.TestCaseStatusEnum.Approved, tc_priorityMediumId, testFolderId8, 20, null, null);

			//Now lets make sure we can retrieve the submitted test case
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId4);
			Assert.AreEqual("Ability to modify authors", testCaseView.Name, "Name");
			Assert.AreEqual(testFolderId8, testCaseView.TestCaseFolderId);
			Assert.IsFalse(testCaseView.IsTestSteps);

			//Next test inserting a test case in the parent folder of the previous one
			testCaseId6 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_JOE_SMITH, null, "Tests basic authentication", null, tc_typeUnitId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, testFolderId3, 30, null, null);

			//Now lets make sure we can retrieve the submitted test case
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId6);
			Assert.AreEqual("Tests basic authentication", testCaseView.Name, "Name");
			Assert.AreEqual(testFolderId3, testCaseView.TestCaseFolderId);
			Assert.IsFalse(testCaseView.IsTestSteps);

			//Finally lets try inserting another test case in the root folder, with a default test step
			testCaseId5 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_JOE_SMITH, USER_ID_JOE_SMITH, "General navigation test", "Tests the basic navigation in the application", tc_typeScenarioId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityHighId, null, 45, null, null, true, true);

			//Now lets make sure we can retrieve the submitted test case (using method 2)
			//Make sure it is marked as having test steps
			TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId5);
			Assert.AreEqual("General navigation test", testCase.Name, "Name");
			Assert.IsNull(testCase.TestCaseFolderId);
			Assert.IsTrue(testCase.IsTestSteps);

			//Verify that one test step was created
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId5);
			Assert.AreEqual(1, testCase.TestSteps.Count);
			Assert.AreEqual("General navigation test", testCase.TestSteps[0].Description);
			Assert.AreEqual("Works as expected.", testCase.TestSteps[0].ExpectedResult);
			Assert.IsNull(testCase.TestSteps[0].SampleData);

			//Make sure the folder counts are correct
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(20, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId3);
			Assert.AreEqual("Unit Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId8);
			Assert.AreEqual("jUnit Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(20, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);
		}

		[
		Test,
		SpiraTestCase(213)
		]
		public void _06_ModifyTestCase()
		{
			//Make some changes to one of the test cases then call update
			//We are also changing its folder, so need to verify the folder counts
			TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId1);
			DateTime oldLastUpdatedDate = testCase.LastUpdateDate;
			testCase.StartTracking();
			testCase.Name = "Ability to delete the existing book";
			testCase.Description = "Description of the ability to delete the existing book";
			testCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.Obsolete;
			testCase.TestCaseTypeId = tc_typeIntegrationId;
			testCase.AuthorId = USER_ID_JOE_SMITH;
			testCase.OwnerId = USER_ID_FRED_BLOGGS;
			testCase.TestCasePriorityId = tc_priorityHighId;
			testCase.EstimatedDuration = 35;    //35 minutes
			testCase.TestCaseFolderId = testFolderId2;
			testCaseManager.Update(testCase, USER_ID_FRED_BLOGGS);

			//Now lets make sure the changes happened
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual(USER_ID_JOE_SMITH, testCaseView.AuthorId, "Author ID");
			Assert.AreEqual(USER_ID_FRED_BLOGGS, testCaseView.OwnerId, "Owner ID");
			Assert.AreEqual("Ability to delete the existing book", testCaseView.Name, "Name");
			Assert.AreEqual("Description of the ability to delete the existing book", testCaseView.Description, "Description");
			Assert.IsNull(testCaseView.ExecutionDate, "Execution Date");
			Assert.AreEqual("Joe P Smith", testCaseView.AuthorName, "Author Name");
			Assert.AreEqual("Fred Bloggs", testCaseView.OwnerName, "Owner Name");
			Assert.AreEqual(35, testCaseView.EstimatedDuration, "EstimatedDuration");
			Assert.AreEqual(tc_priorityHighId, testCaseView.TestCasePriorityId, "TestCasePriorityId");
			Assert.AreEqual("2 - High", testCaseView.TestCasePriorityName, "TestCasePriorityName");
			Assert.AreEqual("Obsolete", testCaseView.TestCaseStatusName);
			Assert.AreEqual("Integration", testCaseView.TestCaseTypeName);
			Assert.AreEqual(testFolderId2, testCaseView.TestCaseFolderId);
			Assert.IsTrue(testCaseView.LastUpdateDate > oldLastUpdatedDate);

			//Make sure its status switched to N/A from Not Run
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, testCaseView.ExecutionStatusId, "Execution Status ID");
			Assert.AreEqual("N/A", testCaseView.ExecutionStatusName, "Execution Status Name");

			//Now verify the folder counts
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(1, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(85, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			//Move to a different folder and switch to an active status
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId1);
			testCase.StartTracking();
			testCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.ReadyForTest;
			testCase.TestCaseFolderId = testFolderId1;
			testCaseManager.Update(testCase, USER_ID_FRED_BLOGGS);

			//Verify the changes
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual("Ready for Test", testCaseView.TestCaseStatusName);
			Assert.AreEqual(testFolderId1, testCaseView.TestCaseFolderId);

			//Verify that its status switched back to 'Not Run'
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCaseView.ExecutionStatusId, "Execution Status ID");
			Assert.AreEqual("Not Run", testCaseView.ExecutionStatusName, "Execution Status Name");

			//Now verify the folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(3, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(55, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);
		}

		[
		Test,
		SpiraTestCase(216)
		]
		public void _07_DeleteTestCase()
		{
			//First lets create a new test case with a test step
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_JOE_SMITH, USER_ID_JOE_SMITH, "Dummy test case", null, tc_typeScenarioId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityHighId, testFolderId4, 45, null, null, true, true);

			//Now lets make sure we can retrieve the submitted test case
			//Make sure it is marked as having test steps
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			Assert.AreEqual("Dummy test case", testCase.Name, "Name");
			Assert.AreEqual(testFolderId4, testCase.TestCaseFolderId);
			Assert.IsTrue(testCase.IsTestSteps);
			Assert.AreEqual(1, testCase.TestSteps.Count);

			//Verify the new folder counts
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId4);
			Assert.AreEqual("Acceptance Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(45, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			//Test deleting the test case
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId);
			//Make sure that it was actually deleted
			bool artifactExists = true;
			try
			{
				testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "TestCase not deleted");

			//Verify the new folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId4);
			Assert.AreEqual("Acceptance Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(0, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(null, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			//We also need to make sure that we can delete one that has been linked to by another test case (as a template step)
			testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_JOE_SMITH, USER_ID_JOE_SMITH, "Dummy test case", null, tc_typeScenarioId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityHighId, testFolderId4, 45, null, null, true, true);
			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId1, null, testCaseId, null);

			//Verify that we have steps and steps flag set
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			Assert.AreEqual("Dummy test case", testCase.Name, "Name");
			Assert.AreEqual(testFolderId4, testCase.TestCaseFolderId);
			Assert.IsTrue(testCase.IsTestSteps);
			Assert.AreEqual(1, testCase.TestSteps.Count);

			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			Assert.AreEqual("Ability to delete the existing book", testCase.Name, "Name");
			Assert.IsTrue(testCase.IsTestSteps);
			Assert.AreEqual(1, testCase.TestSteps.Count);

			//Verify the new folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId4);
			Assert.AreEqual("Acceptance Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(45, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId);
			//Make sure that it was actually deleted
			artifactExists = true;
			try
			{
				testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "TestCase not deleted");

			//Verify the new folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId4);
			Assert.AreEqual("Acceptance Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(0, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(null, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			//Make sure the test case is now listed as having no steps and the flag is correct
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			Assert.AreEqual("Ability to delete the existing book", testCase.Name, "Name");
			Assert.IsFalse(testCase.IsTestSteps);
			Assert.AreEqual(0, testCase.TestSteps.Count);

			//Finally need to purge the deletes since they will affect the POSITION values
			new HistoryManager().PurgeAllDeleted(projectId, USER_ID_SYSTEM_ADMIN);
		}

		[
		Test,
		SpiraTestCase(225)
		]
		public void _08_InsertTestStep()
		{
			//Lets test that we can insert a new test step into a test case
			//First lets retrieve a test case with its associated test steps and verify that there are no steps currently
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			Assert.IsFalse(testCase.IsTestSteps);
			Assert.AreEqual(0, testCase.TestSteps.Count);

			//Since adding a step will set a passed test case back to Not Run, we'll fake this
			//by setting the execution status of the test case to passed
			testCase.StartTracking();
			testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testCaseManager.Update(testCase, USER_ID_SYSTEM_ADMIN, null, false);

			//Verify
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual("Passed", testCaseView.ExecutionStatusName);

			//Next lets actually insert the test steps - first with sample data and a position number
			//Need to test that it can hold upto 2000 chars for description and expected result
			//and upto 1000 characters for sample data
			testStepId1 = testCaseManager.InsertStep(
				USER_ID_FRED_BLOGGS,
				testCaseId1,
				1,
				"User Clicks to Authors module" + InternalRoutines.RepeatString("tes\u05d0", 492),
				"Authors home page displayed" + InternalRoutines.RepeatString("tes\u05d0", 492),
				"Some sample data" + InternalRoutines.RepeatString("tes\u05d0", 246)
				);

			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			//Check to see if existing test step moved and count increased
			Assert.IsTrue(testCase.IsTestSteps);
			Assert.AreEqual(1, testCase.TestSteps.Count);
			Assert.AreEqual(testStepId1, testCase.TestSteps[0].TestStepId);
			Assert.AreEqual(testCaseId1, testCase.TestSteps[0].TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.TestSteps[0].ExecutionStatusId);
			Assert.AreEqual("User Clicks to Authors module" + InternalRoutines.RepeatString("tes\u05d0", 492), testCase.TestSteps[0].Description);
			Assert.AreEqual(1, testCase.TestSteps[0].Position);
			Assert.AreEqual("Authors home page displayed" + InternalRoutines.RepeatString("tes\u05d0", 492), testCase.TestSteps[0].ExpectedResult);
			Assert.AreEqual("Some sample data" + InternalRoutines.RepeatString("tes\u05d0", 246), testCase.TestSteps[0].SampleData);
			Assert.AreEqual("Not Run", testCase.TestSteps[0].ExecutionStatusName);
			Assert.IsNull(testCase.TestSteps[0].LinkedTestCaseId);

			//Check to make sure that the execution status changed to 'Not Run' after adding the step
			Assert.AreEqual("Not Run", testCase.ExecutionStatusName);

			//Next with sample data and no position number
			testStepId2 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, null, "User Clicks Finish button", "Returned to home page", "Some sample data");
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);

			//Check to see if count increased
			Assert.AreEqual(2, testCase.TestSteps.Count);

			//Now check new test step
			Assert.AreEqual(testStepId2, testCase.TestSteps[1].TestStepId);
			Assert.AreEqual(testCaseId1, testCase.TestSteps[1].TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.TestSteps[1].ExecutionStatusId);
			Assert.AreEqual("User Clicks Finish button", testCase.TestSteps[1].Description);
			Assert.AreEqual(2, testCase.TestSteps[1].Position);
			Assert.AreEqual("Returned to home page", testCase.TestSteps[1].ExpectedResult);
			Assert.AreEqual("Some sample data", testCase.TestSteps[1].SampleData);
			Assert.AreEqual("Not Run", testCase.TestSteps[1].ExecutionStatusName);

			//Next lets actually insert the test steps - with a position number and no sample data
			testStepId3 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, 1, "Dummy Test Step", "Dummy result expected", null);
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			//Check to see if count increased
			Assert.AreEqual(3, testCase.TestSteps.Count);
			//Now check new test step
			Assert.AreEqual(testStepId3, testCase.TestSteps[0].TestStepId);
			Assert.AreEqual(testCaseId1, testCase.TestSteps[0].TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.TestSteps[0].ExecutionStatusId);
			Assert.AreEqual("Dummy Test Step", testCase.TestSteps[0].Description);
			Assert.AreEqual(1, testCase.TestSteps[0].Position);
			Assert.AreEqual("Dummy result expected", testCase.TestSteps[0].ExpectedResult);
			Assert.IsNull(testCase.TestSteps[0].SampleData, "Sample Data Not Null");
			Assert.AreEqual("Not Run", testCase.TestSteps[0].ExecutionStatusName);

			//Make sure the other test cases are in the correct position
			Assert.AreEqual(testStepId1, testCase.TestSteps[1].TestStepId);
			Assert.AreEqual(2, testCase.TestSteps[1].Position);
			Assert.AreEqual(testStepId2, testCase.TestSteps[2].TestStepId);
			Assert.AreEqual(3, testCase.TestSteps[2].Position);

			//Finally we need to test that inserting a test step into a test case with no existing steps changes the steps flag
			//Also set its status to 'Failed' and make sure that adding a step does not affect the status
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId2);
			Assert.IsFalse(testCase.IsTestSteps);
			testCase.StartTracking();
			testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			testCaseManager.Update(testCase, USER_ID_SYSTEM_ADMIN, null, false);

			//Verify
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId2);
			Assert.IsFalse(testCaseView.IsTestSteps);
			Assert.AreEqual("Failed", testCaseView.ExecutionStatusName);

			//Now add a test step and verify the flag changed but the status did not
			int testStepId5 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId2, null, "Dummy Test Step", "It Works", null);
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId2);
			Assert.IsTrue(testCaseView.IsTestSteps);
			Assert.AreEqual("Failed", testCaseView.ExecutionStatusName);

			//Finally remove the step and verify that the flag changed back
			testCaseManager.MarkStepAsDeleted(USER_ID_FRED_BLOGGS, testCaseId2, testStepId5);
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId2);
			Assert.IsFalse(testCaseView.IsTestSteps);
			Assert.AreEqual("Failed", testCaseView.ExecutionStatusName);

			//Put the status back to 'Not Run'
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId2);
			testCase.StartTracking();
			testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
			testCaseManager.Update(testCase, USER_ID_SYSTEM_ADMIN, null, false);
		}

		[
		Test,
		SpiraTestCase(226)
		]
		public void _09_InsertLinkedTestCase()
		{
			//First we need to create a new test case that we're going to define parameters for
			int templateCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case For Linking", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);

			//Now add some parameters to this test case
			List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(templateCaseId);
			Assert.AreEqual(0, testCaseParameters.Count);
			testCaseManager.InsertParameter(projectId, templateCaseId, "param1", "default", USER_ID_FRED_BLOGGS);
			testCaseManager.InsertParameter(projectId, templateCaseId, "param2", null, USER_ID_FRED_BLOGGS);

			//Verify that it has parameters
			testCaseParameters = testCaseManager.RetrieveParameters(templateCaseId);
			Assert.AreEqual(2, testCaseParameters.Count);
			Assert.AreEqual("param1", testCaseParameters[0].Name);
			Assert.AreEqual("param2", testCaseParameters[1].Name);
			Assert.AreEqual("default", testCaseParameters[0].DefaultValue);
			Assert.IsTrue(String.IsNullOrEmpty(testCaseParameters[1].DefaultValue));

			//Verify that we can modify the test case parameters
			testCaseManager.UpdateParameter(projectId, testCaseParameters[0].TestCaseParameterId, "param1a", "value123", USER_ID_FRED_BLOGGS);

			//Verify
			testCaseParameters = testCaseManager.RetrieveParameters(templateCaseId);
			Assert.AreEqual(2, testCaseParameters.Count);
			Assert.AreEqual("param1a", testCaseParameters[0].Name);
			Assert.AreEqual("param2", testCaseParameters[1].Name);
			Assert.AreEqual("value123", testCaseParameters[0].DefaultValue);
			Assert.IsTrue(String.IsNullOrEmpty(testCaseParameters[1].DefaultValue));

			//Put them back the way they were
			testCaseManager.UpdateParameter(projectId, testCaseParameters[0].TestCaseParameterId, "param1", "default", USER_ID_FRED_BLOGGS);

			//Verify
			testCaseParameters = testCaseManager.RetrieveParameters(templateCaseId);
			Assert.AreEqual(2, testCaseParameters.Count);
			Assert.AreEqual("param1", testCaseParameters[0].Name);
			Assert.AreEqual("param2", testCaseParameters[1].Name);
			Assert.AreEqual("default", testCaseParameters[0].DefaultValue);
			Assert.IsTrue(String.IsNullOrEmpty(testCaseParameters[1].DefaultValue));

			//Now lets create a new test case that will link in this
			int parentTestCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Parent TestCase", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);

			//Lets test that we can insert a link to an existing test case into another test case
			//First lets retrieve a test case with its associated test steps and verify the count and step flag
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, parentTestCaseId);
			Assert.IsFalse(testCase.IsTestSteps);
			Assert.AreEqual(0, testCase.TestSteps.Count);

			//Next lets actually insert the link to a test case
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("param1", "valueA");
			parameters.Add("param2", "valueB");
			int testStepId = testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, parentTestCaseId, null, templateCaseId, parameters);

			//Check to see if flag changed and count increased
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, parentTestCaseId);
			Assert.IsTrue(testCase.IsTestSteps);
			Assert.AreEqual(1, testCase.TestSteps.Count);

			//Now check new linked test case
			List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(parentTestCaseId);
			Assert.AreEqual(testStepId, testSteps[0].TestStepId);
			Assert.AreEqual(parentTestCaseId, testSteps[0].TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, testSteps[0].ExecutionStatusId);
			Assert.AreEqual("Call", testSteps[0].Description);
			Assert.AreEqual(0, testSteps[0].Position);
			Assert.AreEqual(templateCaseId, testSteps[0].LinkedTestCaseId);
			Assert.AreEqual("Sample Test Case For Linking", testSteps[0].LinkedTestCaseName);
			Assert.AreEqual("N/A", testSteps[0].ExecutionStatusName);
			Assert.IsNull(testSteps[0].ExpectedResult);
			Assert.IsNull(testSteps[0].SampleData);

			//Verify the parameters
			List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStepId);
			Assert.AreEqual(2, testStepParameterValues.Count);
			Assert.AreEqual("param1", testStepParameterValues[0].Name);
			Assert.AreEqual("param2", testStepParameterValues[1].Name);
			Assert.AreEqual("valueA", testStepParameterValues[0].Value);
			Assert.AreEqual("valueB", testStepParameterValues[1].Value);

			//Now test that we can update the parameter values
			testStepParameterValues[0].Value = "valueC";
			testStepParameterValues[1].Value = "valueD";
			testCaseManager.SaveParameterValues(projectId, testStepId, testStepParameterValues);

			//Verify the parameters
			testStepParameterValues = testCaseManager.RetrieveParameterValues(testStepId);
			Assert.AreEqual(2, testStepParameterValues.Count);
			Assert.AreEqual("param1", testStepParameterValues[0].Name);
			Assert.AreEqual("param2", testStepParameterValues[1].Name);
			Assert.AreEqual("valueC", testStepParameterValues[0].Value);
			Assert.AreEqual("valueD", testStepParameterValues[1].Value);

			//Test that we can remove a value then add it back
			int deletedTestCaseParameterId = testStepParameterValues[0].TestCaseParameterId;
			testStepParameterValues = testCaseManager.RetrieveParameterValues(testStepId);
			testStepParameterValues.Remove(testStepParameterValues[0]);
			testCaseManager.SaveParameterValues(projectId, testStepId, testStepParameterValues);

			//Verify
			testStepParameterValues = testCaseManager.RetrieveParameterValues(testStepId);
			Assert.AreEqual(1, testStepParameterValues.Count);
			Assert.AreEqual("param2", testStepParameterValues[0].Name);
			Assert.AreEqual("valueD", testStepParameterValues[0].Value);

			testStepParameterValues = testCaseManager.RetrieveParameterValues(testStepId);
			testStepParameterValues.Add(new TestStepParameter() { TestStepId = testStepId, TestCaseParameterId = deletedTestCaseParameterId, Value = "valueE" });
			testCaseManager.SaveParameterValues(projectId, testStepId, testStepParameterValues);

			//Verify
			testStepParameterValues = testCaseManager.RetrieveParameterValues(testStepId);
			Assert.AreEqual(2, testStepParameterValues.Count);
			Assert.AreEqual("param1", testStepParameterValues[0].Name);
			Assert.AreEqual("param2", testStepParameterValues[1].Name);
			Assert.AreEqual("valueE", testStepParameterValues[0].Value);
			Assert.AreEqual("valueD", testStepParameterValues[1].Value);

			//Now we need to delete the link and make sure the count/flag updated
			testCaseManager.MarkStepAsDeleted(USER_ID_FRED_BLOGGS, parentTestCaseId, testStepId);

			//Check to see if existing test step moved and count decreased
			//Positions are not altered when we do a 'mark-as-deleted'
			//they only change when change history is purged
			//So cannot use position for display purposes
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, parentTestCaseId);
			Assert.IsFalse(testCase.IsTestSteps);
			Assert.AreEqual(0, testCase.TestSteps.Count);

			//Finally delete the template test case and the child test case
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, templateCaseId);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, parentTestCaseId);
		}

		[
		Test,
		SpiraTestCase(227)
		]
		public void _10_EditTestStep()
		{
			//Make some changes to the data then call update
			TestStep testStep = testCaseManager.RetrieveStepById(projectId, testStepId1);
			testStep.StartTracking();
			testStep.Description = "User Clicks to Authors tab";
			testStep.ExpectedResult = "Authors tab displayed";
			testStep.SampleData = null;
			testCaseManager.UpdateStep(testStep, USER_ID_FRED_BLOGGS);

			//Now lets make sure the changes happened
			TestStepView testStepView = testCaseManager.RetrieveStepById2(testCaseId1, testStepId1);
			Assert.AreEqual(testStepId1, testStepView.TestStepId);
			Assert.AreEqual(testCaseId1, testStepView.TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testStepView.ExecutionStatusId);
			Assert.AreEqual("User Clicks to Authors tab", testStepView.Description);
			Assert.AreEqual(2, testStepView.Position);
			Assert.AreEqual("Authors tab displayed", testStepView.ExpectedResult);
			Assert.IsNull(testStepView.SampleData);
			Assert.AreEqual("Not Run", testStepView.ExecutionStatusName);
		}

		[
		Test,
		SpiraTestCase(228)
		]
		public void _11_DeleteTestStep()
		{
			//Create a new test case with a bunch of steps
			testCaseId8 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case For Deleting Steps", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);
			int testStepToDeleteId1 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId8, null, "Step 1", "Result 1", null);
			int testStepToDeleteId2 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId8, null, "Step 2", "Result 2", null);
			int testStepToDeleteId3 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId8, null, "Step 3", "Result 3", null);
			int testStepToDeleteId4 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId8, null, "Step 4", "Result 4", null);
			int testStepToDeleteId5 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId8, null, "Step 5", "Result 5", null);
			int testStepToDeleteId6 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId8, null, "Step 6", "Result 6", null);

			//Verify the current count of test steps prior to deletion
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.IsTrue(testCase.IsTestSteps);
			Assert.AreEqual(6, testCase.TestSteps.Count);

			//Test deleting a test step in the middle of the list
			testCaseManager.MarkStepAsDeleted(USER_ID_FRED_BLOGGS, testCaseId8, testStepToDeleteId4);
			//Make sure that it was actually deleted
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual(5, testCase.TestSteps.Count);
			Assert.AreNotEqual(testCase.TestSteps[3].TestStepId, testStepToDeleteId4);
			//Make sure that the subsequent test steps kept their position (since soft delete)
			Assert.AreEqual(5, testCase.TestSteps[4].Position);

			//Test deleting a test step at the end of the list
			testCaseManager.MarkStepAsDeleted(USER_ID_FRED_BLOGGS, testCaseId8, testStepToDeleteId6);
			//Make sure that it was actually deleted
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual(4, testCase.TestSteps.Count);
			Assert.AreNotEqual(testCase.TestSteps[3].TestStepId, testStepToDeleteId6);

			//Test deleting a test step at the start of the list
			testCaseManager.MarkStepAsDeleted(USER_ID_FRED_BLOGGS, testCaseId8, testStepToDeleteId1);
			//Make sure that it was actually deleted
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual(3, testCase.TestSteps.Count);
			Assert.AreNotEqual(testCase.TestSteps[0].TestStepId, testStepToDeleteId1);
		}

		[
		Test,
		SpiraTestCase(229)
		]
		public void _12_MoveStep()
		{
			//First get the test case from the past test
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual(3, testCase.TestSteps.Count);

			//Add two more steps
			int testStepIdA = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId8, null, "Step A", "Result A", null);
			int testStepIdB = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId8, null, "Step B", "Result B", null);

			//Verify the starting positions
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual(5, testCase.TestSteps.Count);
			Assert.AreEqual("Step 2", testCase.TestSteps[0].Description);
			Assert.AreEqual("Step 3", testCase.TestSteps[1].Description);
			Assert.AreEqual("Step 5", testCase.TestSteps[2].Description);
			Assert.AreEqual("Step A", testCase.TestSteps[3].Description);
			Assert.AreEqual("Step B", testCase.TestSteps[4].Description);

			//Lets test that we can move a test step up and down in position

			//Now move the middle-test up one position and verify change
			testCaseManager.MoveStep(testCaseId8, testCase.TestSteps[2].TestStepId, testCase.TestSteps[1].TestStepId, USER_ID_SYSTEM_ADMIN);
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual("Step 2", testCase.TestSteps[0].Description);
			Assert.AreEqual("Step 5", testCase.TestSteps[1].Description);
			Assert.AreEqual("Step 3", testCase.TestSteps[2].Description);
			Assert.AreEqual("Step A", testCase.TestSteps[3].Description);
			Assert.AreEqual("Step B", testCase.TestSteps[4].Description);

			//Now move the same test down one position and verify change
			testCaseManager.MoveStep(testCaseId8, testCase.TestSteps[1].TestStepId, testCase.TestSteps[3].TestStepId, USER_ID_SYSTEM_ADMIN);
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual("Step 2", testCase.TestSteps[0].Description);
			Assert.AreEqual("Step 3", testCase.TestSteps[1].Description);
			Assert.AreEqual("Step 5", testCase.TestSteps[2].Description);
			Assert.AreEqual("Step A", testCase.TestSteps[3].Description);
			Assert.AreEqual("Step B", testCase.TestSteps[4].Description);

			//Now move the same test down to the end of the list and verify change
			testCaseManager.MoveStep(testCaseId8, testCase.TestSteps[2].TestStepId, null, USER_ID_SYSTEM_ADMIN);
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual("Step 2", testCase.TestSteps[0].Description);
			Assert.AreEqual("Step 3", testCase.TestSteps[1].Description);
			Assert.AreEqual("Step A", testCase.TestSteps[2].Description);
			Assert.AreEqual("Step B", testCase.TestSteps[3].Description);
			Assert.AreEqual("Step 5", testCase.TestSteps[4].Description);

			//Now move the same test back to its original position and verify change
			testCaseManager.MoveStep(testCaseId8, testCase.TestSteps[4].TestStepId, testCase.TestSteps[2].TestStepId, USER_ID_SYSTEM_ADMIN);
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId8);
			Assert.AreEqual("Step 2", testCase.TestSteps[0].Description);
			Assert.AreEqual("Step 3", testCase.TestSteps[1].Description);
			Assert.AreEqual("Step 5", testCase.TestSteps[2].Description);
			Assert.AreEqual("Step A", testCase.TestSteps[3].Description);
			Assert.AreEqual("Step B", testCase.TestSteps[4].Description);
		}

		/// <summary>
		/// Tests that we can make copies of test steps, both normal and linked ones
		/// </summary>
		[
		Test,
		SpiraTestCase(578)
		]
		public void _13_CopyStep()
		{
			//First we need to create a new test case that we're going to define parameters for
			int templateCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case For Linking", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);

			//Now add some parameters to this test case
			List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(templateCaseId);
			Assert.AreEqual(0, testCaseParameters.Count);
			testCaseManager.InsertParameter(projectId, templateCaseId, "param1", "default", USER_ID_FRED_BLOGGS);
			testCaseManager.InsertParameter(projectId, templateCaseId, "param2", null, USER_ID_FRED_BLOGGS);

			//Next create a test case with one step and one link to our template
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case For Copying", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);
			int stepId = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, 1, "Sample Step 1", "It Works", "Sample Data");
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("param1", "valueA");
			parameters.Add("param2", "valueB");
			int linkId = testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId, 2, templateCaseId, parameters);

			//Verify data
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			Assert.AreEqual(2, testCase.TestSteps.Count);
			Assert.AreEqual("Sample Step 1", testCase.TestSteps[0].Description);
			Assert.AreEqual(templateCaseId, testCase.TestSteps[1].LinkedTestCaseId);

			//Verify parameters
			List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(linkId);
			Assert.AreEqual(2, testStepParameterValues.Count);
			Assert.AreEqual("param1", testStepParameterValues[0].Name);
			Assert.AreEqual("param2", testStepParameterValues[1].Name);
			Assert.AreEqual("valueA", testStepParameterValues[0].Value);
			Assert.AreEqual("valueB", testStepParameterValues[1].Value);

			//Now make a copy of the step
			testCaseManager.CopyTestStep(USER_ID_FRED_BLOGGS, projectId, testCaseId, stepId);

			//Verify data
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			Assert.AreEqual(3, testCase.TestSteps.Count);
			Assert.AreEqual("Sample Step 1", testCase.TestSteps[0].Description);
			Assert.AreEqual("Sample Step 1", testCase.TestSteps[1].Description);
			Assert.AreEqual(templateCaseId, testCase.TestSteps[2].LinkedTestCaseId);

			//Now make a copy of the link
			int copiedTestLinkId = testCaseManager.CopyTestStep(USER_ID_FRED_BLOGGS, projectId, testCaseId, linkId);

			//Verify data
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			Assert.AreEqual(4, testCase.TestSteps.Count);
			Assert.AreEqual("Sample Step 1", testCase.TestSteps[0].Description);
			Assert.AreEqual("Sample Step 1", testCase.TestSteps[1].Description);
			Assert.AreEqual(templateCaseId, testCase.TestSteps[2].LinkedTestCaseId);
			Assert.AreEqual(templateCaseId, testCase.TestSteps[3].LinkedTestCaseId);

			//Verify that parameter values are copied
			testStepParameterValues = testCaseManager.RetrieveParameterValues(copiedTestLinkId);
			Assert.AreEqual(2, testStepParameterValues.Count);
			Assert.AreEqual("param1", testStepParameterValues[0].Name);
			Assert.AreEqual("param2", testStepParameterValues[1].Name);
			Assert.AreEqual("valueA", testStepParameterValues[0].Value);
			Assert.AreEqual("valueB", testStepParameterValues[1].Value);

			//Finally delete the test case
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId);
		}

		[
		Test,
		SpiraTestCase(230)
		]
		public void _14_DeleteTestWithSteps()
		{
			//Make sure that deleting a test case cascades a delete of its steps
			//First create a test case with two test steps
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case For Deleting", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, 1, "Sample Step 1", "It Works", "Sample Data");
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, 2, "Sample Step 2", "It Works", "Sample Data");

			//Now delete the test case and make sure it can be deleted
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId);
			bool artifactExists = true;
			try
			{
				TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "TestCase not deleted");

			//Now using the second retrieve function
			artifactExists = true;
			try
			{
				TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "TestCase not deleted");
		}

		[
		Test,
		SpiraTestCase(235)
		]
		public void _15_MoveTestCases()
		{
			//First get the high level folder count
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(3, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(55, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			//Verify the new folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId4);
			Assert.AreEqual("Acceptance Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(0, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(null, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(null, testCaseFolder.ActualDuration);
			Assert.AreEqual(null, testCaseFolder.ExecutionDate);

			//******** Moving a Single Test Case ***********

			//Verify the current folder
			TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId1);
			Assert.AreEqual(testFolderId1, testCase.TestCaseFolderId);

			//First lets try moving a single test case to a different folder
			testCaseManager.TestCase_UpdateFolder(testCaseId1, testFolderId2);

			//Verify the folder change
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId1);
			Assert.AreEqual(testFolderId2, testCase.TestCaseFolderId);

			//Verify the folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(20, testCaseFolder.EstimatedDuration);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(3, testCaseFolder.CountNotRun);
			Assert.AreEqual(85, testCaseFolder.EstimatedDuration);

			//Now move the test case back
			testCaseManager.TestCase_UpdateFolder(testCaseId1, testFolderId1);

			//Verify the change
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId1);
			Assert.AreEqual(testFolderId1, testCase.TestCaseFolderId);

			//Verify the folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(3, testCaseFolder.CountNotRun);
			Assert.AreEqual(55, testCaseFolder.EstimatedDuration);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);

			//Move a test case to the root folder
			testCaseManager.TestCase_UpdateFolder(testCaseId1, null);

			//Verify the change
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId1);
			Assert.IsNull(testCase.TestCaseFolderId);

			//Verify the folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(20, testCaseFolder.EstimatedDuration);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);

			//Now move the test case back
			testCaseManager.TestCase_UpdateFolder(testCaseId1, testFolderId1);

			//Verify the change
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId1);
			Assert.AreEqual(testFolderId1, testCase.TestCaseFolderId);

			//Verify the folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(3, testCaseFolder.CountNotRun);
			Assert.AreEqual(55, testCaseFolder.EstimatedDuration);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);

			//******** Moving a Test Folder ***********

			//Verify the current parent folders of our folders
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.IsNull(testCaseFolder.ParentTestCaseFolderId);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.IsNull(testCaseFolder.ParentTestCaseFolderId);

			//Lets move folder 2 under folder 1
			testCaseFolder.StartTracking();
			testCaseFolder.ParentTestCaseFolderId = testFolderId1;
			testCaseManager.TestCaseFolder_Update(testCaseFolder);

			//Verify the parent folders of our folders
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.IsNull(testCaseFolder.ParentTestCaseFolderId);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual(testFolderId1, testCaseFolder.ParentTestCaseFolderId);

			//Verify the folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(5, testCaseFolder.CountNotRun);
			Assert.AreEqual(105, testCaseFolder.EstimatedDuration);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);

			//Move it back
			testCaseFolder.StartTracking();
			testCaseFolder.ParentTestCaseFolderId = null;
			testCaseManager.TestCaseFolder_Update(testCaseFolder);

			//Verify the parent folders of our folders
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.IsNull(testCaseFolder.ParentTestCaseFolderId);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.IsNull(testCaseFolder.ParentTestCaseFolderId);

			//Verify the folder counts
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(3, testCaseFolder.CountNotRun);
			Assert.AreEqual(55, testCaseFolder.EstimatedDuration);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(50, testCaseFolder.EstimatedDuration);
		}

		[
		Test,
		SpiraTestCase(236)
		]
		public void _16_CopyTestCases()
		{
			//******** Copying a Single Test Case ***********

			//First lets try copying a single test case to a specific folder
			int copiedTestCaseId1 = testCaseManager.TestCase_Copy(USER_ID_FRED_BLOGGS, projectId, testCaseId1, testFolderId2);

			//Verify the copied test case
			TestCaseView testCase = testCaseManager.RetrieveById(projectId, copiedTestCaseId1);
			Assert.AreEqual("Ability to delete the existing book - Copy", testCase.Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);
			Assert.IsNull(testCase.ExecutionDate, "Execution Date");
			Assert.AreEqual("Joe P Smith", testCase.AuthorName, "Author Name");
			Assert.AreEqual("Fred Bloggs", testCase.OwnerName, "Owner Name");
			Assert.AreEqual(35, testCase.EstimatedDuration, "EstimatedDuration");
			Assert.AreEqual(tc_priorityHighId, testCase.TestCasePriorityId, "TestCasePriorityId");
			Assert.AreEqual("2 - High", testCase.TestCasePriorityName, "TestCasePriorityName");
			Assert.AreEqual("Ready for Test", testCase.TestCaseStatusName);
			Assert.AreEqual("Integration", testCase.TestCaseTypeName);
			Assert.AreEqual(testFolderId2, testCase.TestCaseFolderId);

			//Verify the steps
			TestCase testCase1 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			TestCase testCase2 = testCaseManager.RetrieveByIdWithSteps(projectId, copiedTestCaseId1);
			Assert.IsTrue(testCase2.IsTestSteps);
			Assert.AreEqual(testCase1.TestSteps.Count, testCase2.TestSteps.Count);
			Assert.AreEqual(testCase1.TestSteps[0].Description, testCase2.TestSteps[0].Description);
			Assert.AreEqual(testCase1.TestSteps[0].ExpectedResult, testCase2.TestSteps[0].ExpectedResult);
			Assert.AreEqual(testCase1.TestSteps[0].SampleData, testCase2.TestSteps[0].SampleData);

			//Verify the folder counts
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(3, testCaseFolder.CountNotRun);
			Assert.AreEqual(55, testCaseFolder.EstimatedDuration);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Regression Tests", testCaseFolder.Name);
			Assert.AreEqual(3, testCaseFolder.CountNotRun);
			Assert.AreEqual(85, testCaseFolder.EstimatedDuration);

			//Now add a linked test step and attachment and make sure that copies across correctly
			AttachmentManager attachmentManager = new AttachmentManager();
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("isbn", "1234");
			parameters.Add("genre", "fiction");
			int newTestLinkId = testCaseManager.TestCase_CreateNewLinkedTestCase(USER_ID_FRED_BLOGGS, projectId, testCaseId1, null, testFolderId2, "Find Book", parameters);
			attachmentManager.Insert(projectId, "www.test1.com", "desc1", USER_ID_FRED_BLOGGS, testCaseId1, Artifact.ArtifactTypeEnum.TestCase, "1.0", null, null, null, null);
			attachmentManager.Insert(projectId, "www.test2.com", "desc2", USER_ID_FRED_BLOGGS, testCase1.TestSteps[0].TestStepId, Artifact.ArtifactTypeEnum.TestStep, "1.0", null, null, null, null);

			//Verify the link
			testCase1 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			TestStep originalTestLink = testCase1.TestSteps.Last();
			Assert.AreEqual(newTestLinkId, originalTestLink.TestStepId);
			Assert.IsNotNull(originalTestLink.LinkedTestCaseId);
			int childTestCaseId = originalTestLink.LinkedTestCaseId.Value;

			List<TestStepParameter> testStepParameters = testCaseManager.RetrieveParameterValues(originalTestLink.TestStepId);
			Assert.AreEqual(2, testStepParameters.Count);
			Assert.AreEqual("genre", testStepParameters[0].Name);
			Assert.AreEqual("fiction", testStepParameters[0].Value);
			Assert.AreEqual("isbn", testStepParameters[1].Name);
			Assert.AreEqual("1234", testStepParameters[1].Value);

			//Also now map this test case to a requirement, to check that coverage gets copied
			RequirementManager requirementManager = new RequirementManager();
			int rq_importanceLowId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 4).ImportanceId;
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, rq_importanceLowId, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			int requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Accepted, null, USER_ID_FRED_BLOGGS, null, rq_importanceLowId, "Requirement 2", null, null, USER_ID_FRED_BLOGGS);
			testCaseManager.AddToRequirement(projectId, requirementId1, new List<int>() { testCaseId1 }, 1);
			testCaseManager.AddToRequirement(projectId, requirementId2, new List<int>() { testCaseId1 }, 1);

			//Now make another copy
			int copiedTestCaseId2 = testCaseManager.TestCase_Copy(USER_ID_FRED_BLOGGS, projectId, testCaseId1, testFolderId2);

			//Verify the steps and attachments
			testCase1 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			Assert.IsTrue(testCase1.IsTestSteps);
			Assert.IsTrue(testCase1.IsAttachments);
			Assert.IsTrue(testCase1.TestSteps[0].IsAttachments);
			testCase2 = testCaseManager.RetrieveByIdWithSteps(projectId, copiedTestCaseId2);
			Assert.IsTrue(testCase2.IsTestSteps);
			Assert.IsTrue(testCase2.IsAttachments);
			Assert.IsTrue(testCase2.TestSteps[0].IsAttachments);
			Assert.AreEqual(testCase1.TestSteps.Count, testCase2.TestSteps.Count);

			//Verify the details of the step
			TestStep copiedTestLink = testCase2.TestSteps.Last();
			Assert.AreEqual(childTestCaseId, copiedTestLink.LinkedTestCaseId);
			Assert.AreEqual(copiedTestLink.Description, originalTestLink.Description);
			Assert.AreEqual(copiedTestLink.ExpectedResult, originalTestLink.ExpectedResult);
			Assert.AreEqual(copiedTestLink.SampleData, originalTestLink.SampleData);

			//Verify the parameters
			testStepParameters = testCaseManager.RetrieveParameterValues(copiedTestLink.TestStepId);
			Assert.AreEqual(2, testStepParameters.Count);
			Assert.AreEqual("genre", testStepParameters[0].Name);
			Assert.AreEqual("fiction", testStepParameters[0].Value);
			Assert.AreEqual("isbn", testStepParameters[1].Name);
			Assert.AreEqual("1234", testStepParameters[1].Value);

			//Verify the attachments
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, copiedTestCaseId2, Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testCase2.TestSteps[0].TestStepId, Artifact.ArtifactTypeEnum.TestStep, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);

			//Now verify that the coverage information copied across correctly
			List<RequirementView> coverageRequirements = requirementManager.RetrieveCoveredByTestCaseId(USER_ID_FRED_BLOGGS, projectId, copiedTestCaseId2);
			Assert.AreEqual(2, coverageRequirements.Count);

			//******** Copying a Test Folder containing Test Cases ***********

			//Lets copy an entire folder of test cases (folder 1 copied under folder 2)
			int copiedTestFolderId = testCaseManager.TestCaseFolder_Copy(USER_ID_FRED_BLOGGS, projectId, testFolderId1, testFolderId2, true);

			//Verify the new folder name and counts
			TestCaseFolder copiedTestFolder = testCaseManager.TestCaseFolder_GetById(copiedTestFolderId);
			Assert.AreEqual("Functional Tests - Copy", copiedTestFolder.Name);
			Assert.AreEqual(3, copiedTestFolder.CountNotRun);
			Assert.AreEqual(55, copiedTestFolder.EstimatedDuration);

			//Verify the individual test cases
			List<TestCase> testCases = testCaseManager.RetrieveAllInFolder(projectId, copiedTestFolderId);
			Assert.AreEqual(3, testCases.Count);
			Assert.AreEqual("Ability to delete the existing book", testCases[0].Name);
			Assert.AreEqual("Ability to modify selected books", testCases[1].Name);
			Assert.AreEqual("Ability to update existing book", testCases[2].Name);

			//Verify the one test case and its links, parameters and steps
			copiedTestCaseId2 = testCases[0].TestCaseId;

			//Verify the steps and attachments
			testCase2 = testCaseManager.RetrieveByIdWithSteps(projectId, copiedTestCaseId2);
			Assert.IsTrue(testCase2.IsTestSteps);
			Assert.IsTrue(testCase2.IsAttachments);
			Assert.IsTrue(testCase2.TestSteps[0].IsAttachments);
			Assert.AreEqual(testCase1.TestSteps.Count, testCase2.TestSteps.Count);

			//Verify the details of the step
			copiedTestLink = testCase2.TestSteps.Last();
			Assert.AreEqual(childTestCaseId, copiedTestLink.LinkedTestCaseId);
			Assert.AreEqual(copiedTestLink.Description, originalTestLink.Description);
			Assert.AreEqual(copiedTestLink.ExpectedResult, originalTestLink.ExpectedResult);
			Assert.AreEqual(copiedTestLink.SampleData, originalTestLink.SampleData);

			//Verify the parameters
			testStepParameters = testCaseManager.RetrieveParameterValues(copiedTestLink.TestStepId);
			Assert.AreEqual(2, testStepParameters.Count);
			Assert.AreEqual("genre", testStepParameters[0].Name);
			Assert.AreEqual("fiction", testStepParameters[0].Value);
			Assert.AreEqual("isbn", testStepParameters[1].Name);
			Assert.AreEqual("1234", testStepParameters[1].Value);

			//Verify the attachments
			attachments = attachmentManager.RetrieveByArtifactId(projectId, copiedTestCaseId2, Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			attachments = attachmentManager.RetrieveByArtifactId(projectId, testCase2.TestSteps[0].TestStepId, Artifact.ArtifactTypeEnum.TestStep, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);

			//Now verify that the coverage information copied across correctly
			coverageRequirements = requirementManager.RetrieveCoveredByTestCaseId(USER_ID_FRED_BLOGGS, projectId, copiedTestCaseId2);
			Assert.AreEqual(2, coverageRequirements.Count);
		}

		/// <summary>
		/// Tests that we use the feature to 'quickly' block and unblock a test case
		/// </summary>
		[
		Test,
		SpiraTestCase(769)
		]
		public void _17_QuickBlockUnblock()
		{
			//First create a new test case in the new project
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.Approved, tc_priorityMediumId, testFolderId4, null, null, null);

			//Verify that its status is 'not-run'
			TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);

			//Verify there are no blocked test cases in the folder
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId4);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);

			//Now block the test case and verify
			testCaseManager.Block(USER_ID_FRED_BLOGGS, projectId, testCaseId);
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testCase.ExecutionStatusId);

			//Verify that there is one blocked test in the folder
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId4);
			Assert.AreEqual(1, testCaseFolder.CountBlocked);

			//Now unblock the test case and verify
			testCaseManager.UnBlock(USER_ID_FRED_BLOGGS, projectId, testCaseId);
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);

			//Verify there are no blocked test cases in the folder
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId4);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);

			//Finally verify that you can't 'unblock' a test case that's in a different status
			TestRunManager testRunManager = new TestRunManager();
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), (int)TestCase.ExecutionStatusEnum.Failed, "Unit Tests", "Unit Tests", 1, "Test Failure", "Test Failure", null, null, null, TestRun.TestRunFormatEnum.PlainText, null);
			testCaseManager.UnBlock(USER_ID_FRED_BLOGGS, projectId, testCaseId);
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCase.ExecutionStatusId);

			//Clean up
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Verify that multiple, concurrent updates to test cases and test steps are handled correctly
		/// in accordance with optimistic concurrency rules
		/// </summary>
		[
		Test,
		SpiraTestCase(668)
		]
		public void _18_Concurrency_Handling()
		{
			//First we need to create a new test case to verify the handling
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.Approved, tc_priorityLowId, null, null, null, null);

			//Now retrieve the testCase back and keep a copy of the dataset
			TestCase testCase1 = testCaseManager.RetrieveById2(projectId, testCaseId);
			TestCase testCase2 = testCaseManager.RetrieveById2(projectId, testCaseId);

			//Now make a change to field and update
			testCase1.StartTracking();
			testCase1.TestCasePriorityId = tc_priorityHighId;
			testCaseManager.Update(testCase1, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			TestCase testCase3 = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.AreEqual(tc_priorityHighId, testCase3.TestCasePriorityId);

			//Now try making a change using the out of date dataset (has the wrong ConcurrencyDate)
			bool exceptionThrown = false;
			try
			{
				testCase2.StartTracking();
				testCase2.TestCasePriorityId = tc_priorityCriticalId;
				testCaseManager.Update(testCase2, USER_ID_FRED_BLOGGS);
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			testCase3 = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.AreEqual(tc_priorityHighId, testCase3.TestCasePriorityId);

			//Now refresh the old dataset and try again and verify it works
			testCase2 = testCaseManager.RetrieveById2(projectId, testCaseId);
			testCase2.StartTracking();
			testCase2.TestCasePriorityId = tc_priorityCriticalId;
			testCaseManager.Update(testCase2, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			testCase3 = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.AreEqual(tc_priorityCriticalId, testCase3.TestCasePriorityId);

			//Next we need to create a new test step to verify the handling of steps
			int testStepId = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Desc 1", "Expected 1", "");

			//Now retrieve the test step back and keep a copy of the dataset
			testCase1 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			testCase2 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);

			//Now make a change to field and update
			testCase1.TestSteps[0].StartTracking();
			testCase1.TestSteps[0].Description = "Desc 2";
			testCaseManager.Update(testCase1, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			testCase3 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			Assert.AreEqual("Desc 2", testCase3.TestSteps[0].Description);

			//Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
			exceptionThrown = false;
			try
			{
				testCase2.TestSteps[0].StartTracking();
				testCase2.TestSteps[0].Description = "Desc 3";
				testCaseManager.Update(testCase2, USER_ID_FRED_BLOGGS);
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			testCase3 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			Assert.AreEqual("Desc 2", testCase3.TestSteps[0].Description);

			//Now refresh the old dataset and try again and verify it works
			testCase2 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			testCase2.TestSteps[0].StartTracking();
			testCase2.TestSteps[0].Description = "Desc 3";
			testCaseManager.Update(testCase2, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			testCase3 = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			Assert.AreEqual("Desc 3", testCase3.TestSteps[0].Description);

			//Clean up
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId);
		}

		/// <summary>
		/// Tests that we can store an automated test script with a test case and also specify the test engine
		/// </summary>
		[
		Test,
		SpiraTestCase(712)
		]
		public void _19_Automated_Test_Scripts()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Test Automation";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Create a new automation engine and a new test case
			AutomationManager automationManager = new AutomationManager();
			AttachmentManager attachmentManager = new AttachmentManager();
			int automationEngineId = automationManager.InsertEngine("Automation Engine 1", "Engine1", "", true, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, adminSectionId, "Inserted Test Automation");
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, tc_typeUnitId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityHighId, null, null, null, null);

			//Verify that the test case has no automation engine set
			TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.IsNull(testCase.AutomationEngineId);
			Assert.IsNull(testCase.AutomationEngineName);
			Assert.IsNull(testCase.AutomationAttachmentId);

			//Now we need to specify the engine and upload the script
			testCaseManager.AddUpdateAutomationScript(USER_ID_FRED_BLOGGS, projectId, testCaseId, automationEngineId, "file://c:/tests/mytest.qtp", "", null, "1.0", null, null);

			//Verify that the test script was added
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual(automationEngineId, testCase.AutomationEngineId);
			Assert.AreEqual("Automation Engine 1", testCase.AutomationEngineName);
			int attachmentId = testCase.AutomationAttachmentId.Value;
			Attachment attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.AreEqual("file://c:/tests/mytest.qtp", attachment.Filename);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.URL, attachment.AttachmentTypeId);
			Assert.AreEqual("1.0", attachment.CurrentVersion);
			List<AttachmentVersionView> attachmentVersions = attachmentManager.RetrieveVersions(attachmentId);
			Assert.AreEqual(1, attachmentVersions.Count);

			//Now try uploading a newer revision
			testCaseManager.AddUpdateAutomationScript(USER_ID_FRED_BLOGGS, projectId, testCaseId, automationEngineId, "file://c:/tests/mytest2.qtp", "", null, "1.1", null, null);

			//Verify that the test script was updated
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual(automationEngineId, testCase.AutomationEngineId);
			Assert.AreEqual("Automation Engine 1", testCase.AutomationEngineName);
			attachmentId = testCase.AutomationAttachmentId.Value;
			attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.AreEqual("file://c:/tests/mytest2.qtp", attachment.Filename);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.URL, attachment.AttachmentTypeId);
			Assert.AreEqual("1.1", attachment.CurrentVersion);
			attachmentVersions = attachmentManager.RetrieveVersions(attachmentId);
			Assert.AreEqual(2, attachmentVersions.Count);

			//Now try converting to a file attachment
			byte[] data = UnicodeEncoding.UTF8.GetBytes("test string");
			testCaseManager.AddUpdateAutomationScript(USER_ID_FRED_BLOGGS, projectId, testCaseId, automationEngineId, "newtest3.qtp", "", data, "1.2", null, null);

			//Verify that the test script was updated
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual(automationEngineId, testCase.AutomationEngineId);
			Assert.AreEqual("Automation Engine 1", testCase.AutomationEngineName);
			attachmentId = testCase.AutomationAttachmentId.Value;
			attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.AreEqual("newtest3.qtp", attachment.Filename);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachment.AttachmentTypeId);
			Assert.AreEqual("1.2", attachment.CurrentVersion);
			attachmentVersions = attachmentManager.RetrieveVersions(attachmentId);
			Assert.AreEqual(3, attachmentVersions.Count);

			//Now try removing the test script altogether
			testCaseManager.AddUpdateAutomationScript(USER_ID_FRED_BLOGGS, projectId, testCaseId, null, "", "", null, "", null, null);

			//Verify that the test case has no automation engine set
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.IsNull(testCase.AutomationEngineId);
			Assert.IsNull(testCase.AutomationEngineName);
			Assert.IsNull(testCase.AutomationAttachmentId);

			//Verify the attachment was deleted as well
			bool exceptionCaught = false;
			try
			{
				attachment = attachmentManager.RetrieveById(attachmentId);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionCaught = true;
			}
			Assert.IsTrue(exceptionCaught, "ArtifactNotExistsException");

			//Now try adding a new file attachment test script
			testCaseManager.AddUpdateAutomationScript(USER_ID_FRED_BLOGGS, projectId, testCaseId, automationEngineId, "newtest4.qtp", "", data, "1.0", null, null);

			//Verify that the test script was addedd
			testCase = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual(automationEngineId, testCase.AutomationEngineId);
			Assert.AreEqual("Automation Engine 1", testCase.AutomationEngineName);
			attachmentId = testCase.AutomationAttachmentId.Value;
			attachment = attachmentManager.RetrieveById(attachmentId);
			Assert.AreEqual("newtest4.qtp", attachment.Filename);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachment.AttachmentTypeId);
			Assert.AreEqual("1.0", attachment.CurrentVersion);
			attachmentVersions = attachmentManager.RetrieveVersions(attachmentId);
			Assert.AreEqual(1, attachmentVersions.Count);

			//Delete the engine (test that it correctly handles relationship to test case)
			automationManager.DeleteEngine(automationEngineId, USER_ID_FRED_BLOGGS);

			//Delete the test case
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId);

			//Delete the attachment
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(258)
		]
		public void _20_LinkedTestCaseParameters()
		{
			//First, verify that we can retrieve all the parameters in the project
			List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveAllParameters(PROJECT_ID);
			Assert.AreEqual(10, testCaseParameters.Count, "Count");
			Assert.AreEqual("age", testCaseParameters[0].Name, "Name");
			Assert.AreEqual("105", testCaseParameters[0].DefaultValue);
			Assert.AreEqual("url", testCaseParameters[9].Name, "Name");
			Assert.AreEqual("http://www.libraryinformationsystem.com", testCaseParameters[9].DefaultValue);

			//Next verify that we can retrieve the non-inherited parameters for a test case (no values)
			testCaseParameters = testCaseManager.RetrieveParameters(17);
			Assert.AreEqual(2, testCaseParameters.Count, "Count");
			Assert.AreEqual("login", testCaseParameters[0].Name, "Name");
			Assert.IsTrue(testCaseParameters[0].DefaultValue.IsNull(), "DefaultValue");
			Assert.AreEqual("password", testCaseParameters[1].Name, "Name");
			Assert.IsTrue(testCaseParameters[1].DefaultValue.IsNull(), "DefaultValue");

			//Next verify that we can retrieve the all the parameters for a test case (including inherited)
			testCaseParameters = testCaseManager.RetrieveParameters(17, true, true);
			Assert.AreEqual(5, testCaseParameters.Count, "Count");
			Assert.AreEqual("browserName", testCaseParameters[0].Name, "Name");
			Assert.AreEqual("browser", testCaseParameters[0].DefaultValue, "DefaultValue");
			Assert.AreEqual("login", testCaseParameters[1].Name, "Name");
			Assert.IsTrue(testCaseParameters[1].DefaultValue.IsNull(), "DefaultValue");
			Assert.AreEqual("operatingSystem", testCaseParameters[2].Name, "Name");
			Assert.IsTrue(testCaseParameters[2].DefaultValue.IsNull(), "DefaultValue");
			Assert.AreEqual("password", testCaseParameters[3].Name, "Name");
			Assert.IsTrue(testCaseParameters[3].DefaultValue.IsNull(), "DefaultValue");
			Assert.AreEqual("url", testCaseParameters[4].Name, "Name");
			Assert.AreEqual("http://www.libraryinformationsystem.com", testCaseParameters[4].DefaultValue, "DefaultValue");

			//Next verify that we can retrieve the all the parameters for a test case (including inherited)
			//that do not already have a value set at a lower-level in the hierarchy
			testCaseParameters = testCaseManager.RetrieveParameters(17, true, false);
			Assert.AreEqual(4, testCaseParameters.Count, "Count");
			Assert.AreEqual("browserName", testCaseParameters[0].Name, "Name");
			Assert.AreEqual("browser", testCaseParameters[0].DefaultValue, "DefaultValue");
			Assert.AreEqual("login", testCaseParameters[1].Name, "Name");
			Assert.IsTrue(testCaseParameters[2].DefaultValue.IsNull(), "DefaultValue");
			Assert.AreEqual("operatingSystem", testCaseParameters[2].Name, "Name");
			Assert.IsTrue(testCaseParameters[2].DefaultValue.IsNull(), "DefaultValue");
			Assert.AreEqual("password", testCaseParameters[3].Name, "Name");
			Assert.IsTrue(testCaseParameters[3].DefaultValue.IsNull(), "DefaultValue");

			//First verify that we can retrieve the parameters for a test case where there is a default value
			testCaseParameters = testCaseManager.RetrieveParameters(16);
			Assert.AreEqual(3, testCaseParameters.Count, "Count");
			Assert.AreEqual("browserName", testCaseParameters[0].Name, "Name");
			Assert.AreEqual("browser", testCaseParameters[0].DefaultValue, "DefaultValue");
			Assert.AreEqual("operatingSystem", testCaseParameters[1].Name, "Name");
			Assert.IsTrue(testCaseParameters[1].DefaultValue.IsNull(), "DefaultValue");
			Assert.AreEqual("url", testCaseParameters[2].Name, "Name");
			Assert.AreEqual("http://www.libraryinformationsystem.com", testCaseParameters[2].DefaultValue, "DefaultValue");

			//Now verify that we can retrieve the specific values of that parameter for a test step link
			List<TestStepParameter> testStepParameters = testCaseManager.RetrieveParameterValues(1);
			Assert.AreEqual(3, testStepParameters.Count, "Count");
			Assert.AreEqual("browserName", testStepParameters[0].Name, "Name");
			Assert.AreEqual("Internet Explorer", testStepParameters[0].Value, "Value");
			Assert.AreEqual("login", testStepParameters[1].Name, "Name");
			Assert.AreEqual("librarian", testStepParameters[1].Value, "Value");
			Assert.AreEqual("password", testStepParameters[2].Name, "Name");
			Assert.AreEqual("librarian", testStepParameters[2].Value, "Value");

			//Now lets try and add parameters to an existing test case
			testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case With Parameters", "Test Case With Parameters Description", null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);
			testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Derived Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);
			int testCaseParameter1 = testCaseManager.InsertParameter(projectId, testCaseId1, "TestParameter1", null, USER_ID_FRED_BLOGGS);
			int testCaseParameter2 = testCaseManager.InsertParameter(projectId, testCaseId1, "TestParameter2", "DefaultValue2", USER_ID_FRED_BLOGGS);
			int testCaseParameter3 = testCaseManager.InsertParameter(projectId, testCaseId1, "TestParameter3", "DefaultValue3", USER_ID_FRED_BLOGGS);

			//Verify that they were inserted correctly
			testCaseParameters = testCaseManager.RetrieveParameters(testCaseId1);
			Assert.AreEqual(3, testCaseParameters.Count);
			Assert.AreEqual("TestParameter1", testCaseParameters[0].Name);
			Assert.IsTrue(testCaseParameters[0].DefaultValue.IsNull());
			Assert.AreEqual("TestParameter2", testCaseParameters[1].Name);
			Assert.AreEqual("DefaultValue2", testCaseParameters[1].DefaultValue);
			Assert.AreEqual("TestParameter3", testCaseParameters[2].Name);
			Assert.AreEqual("DefaultValue3", testCaseParameters[2].DefaultValue);

			//Now lets add two linked test steps to the test case with two of the parameters filled out
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("TestParameter1", "TestValue1A");
			parameters.Add("TestParameter2", "TestValue2A");
			testStepId1 = testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId2, null, testCaseId1, parameters);
			parameters.Clear();
			parameters.Add("TestParameter1", "TestValue1B");
			parameters.Add("TestParameter2", "TestValue2B");
			testStepId2 = testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId2, null, testCaseId1, parameters);

			//Verify the data
			testStepParameters = testCaseManager.RetrieveParameterValues(testStepId1);
			Assert.AreEqual(2, testStepParameters.Count);
			Assert.AreEqual("TestParameter1", testStepParameters[0].Name);
			Assert.AreEqual("TestValue1A", testStepParameters[0].Value);
			Assert.AreEqual("TestParameter2", testStepParameters[1].Name);
			Assert.AreEqual("TestValue2A", testStepParameters[1].Value);
			testStepParameters = testCaseManager.RetrieveParameterValues(testStepId2);
			Assert.AreEqual(2, testStepParameters.Count);
			Assert.AreEqual("TestParameter1", testStepParameters[0].Name);
			Assert.AreEqual("TestValue1B", testStepParameters[0].Value);
			Assert.AreEqual("TestParameter2", testStepParameters[1].Name);
			Assert.AreEqual("TestValue2B", testStepParameters[1].Value);

			//Now lets try adding a new test case that links to the parent test case
			//It should have the ability to populate the third inherited parameter that has not been currently populated
			testCaseId3 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Derived Test Case", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, 30, null, null);
			parameters.Clear();
			parameters.Add("TestParameter3", "TestValue3");
			testStepId3 = testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId3, null, testCaseId2, parameters);
			//Verify the data
			testStepParameters = testCaseManager.RetrieveParameterValues(testStepId3);
			Assert.AreEqual(1, testStepParameters.Count);
			Assert.AreEqual("TestParameter3", testStepParameters[0].Name);
			Assert.AreEqual("TestValue3", testStepParameters[0].Value);

			//Now delete a test case parameter
			testCaseManager.DeleteParameter(projectId, testCaseParameter2, USER_ID_FRED_BLOGGS);
			testCaseParameters = testCaseManager.RetrieveParameters(testCaseId1);
			Assert.AreEqual(2, testCaseParameters.Count);

			//Now delete one of the test steps
			testCaseManager.MarkStepAsDeleted(USER_ID_FRED_BLOGGS, testCaseId2, testStepId2);

			//Finally clean up by deleting the test cases. Make sure that we can delete a test case with a parameter
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, testCaseId3);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, testCaseId2);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, testCaseId1);
		}

		[
		Test,
		SpiraTestCase(357)
		]
		public void _21_ExportTestCases()
		{
			//First export a whole folder of test cases over from the sample project to our new project
			Dictionary<int, int> testCaseMapping = new Dictionary<int, int>();
			Dictionary<int, int> testFolderMapping = new Dictionary<int, int>();
			int exportedTestFolderId1 = testCaseManager.TestCaseFolder_Export(USER_ID_FRED_BLOGGS, PROJECT_ID, 1, projectId, testCaseMapping, testFolderMapping);

			//Now verify that they exported correctly

			//First the test folder itself - two additional linked tests are also copied across,
			//but they are in different folders
			TestCaseFolder exportedTestFolder = testCaseManager.TestCaseFolder_GetById(exportedTestFolderId1);
			Assert.AreEqual("Functional Tests", exportedTestFolder.Name);
			Assert.AreEqual(5, exportedTestFolder.CountNotRun);
			Assert.AreEqual(36, exportedTestFolder.EstimatedDuration);

			//Next verify one of the child test cases
			List<TestCase> childTestCases = testCaseManager.RetrieveAllInFolder(projectId, exportedTestFolderId1);
			Assert.AreEqual(5, childTestCases.Count);
			Assert.AreEqual("Ability to create new author", childTestCases[0].Name);
			Assert.AreEqual("Ability to create new book", childTestCases[1].Name);
			Assert.AreEqual("Ability to edit existing author", childTestCases[2].Name);
			Assert.AreEqual("Ability to edit existing book", childTestCases[3].Name);
			Assert.AreEqual("Ability to reassign book to different author", childTestCases[4].Name);

			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, childTestCases[1].TestCaseId);
			Assert.AreEqual("Ability to create new book", testCase.Name);
			Assert.AreEqual("Not Run", testCase.ExecutionStatusName);
			Assert.IsTrue(testCase.IsAttachments);
			Assert.IsTrue(testCase.IsTestSteps);

			//Now verify the test steps and store a reference to any linked test cases
			Assert.AreEqual(5, testCase.TestSteps.Count);
			Assert.IsNotNull(testCase.TestSteps[0].LinkedTestCaseId);
			int linkedTestCaseId = testCase.TestSteps[0].LinkedTestCaseId.Value;
			int linkedTestStepId = testCase.TestSteps[0].TestStepId;
			Assert.AreEqual("User clicks link to create book", testCase.TestSteps[1].Description);
			Assert.AreEqual(false, testCase.TestSteps[1].IsAttachments, "AttachmentsYn");
			Assert.AreEqual("User enters books name and author, then clicks Next", testCase.TestSteps[2].Description);
			Assert.AreEqual(true, testCase.TestSteps[2].IsAttachments, "AttachmentsYn");

			//Verify that the linked test case was also exported to the new project
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, linkedTestCaseId);
			Assert.AreEqual("Login to Application", testCaseView.Name);
			Assert.AreEqual("Not Run", testCaseView.ExecutionStatusName);
			Assert.IsFalse(testCaseView.IsAttachments, "AttachmentsYn");
			Assert.IsTrue(testCaseView.IsTestSteps);

			//Now verify that the test case and test step parameters were also copied across correctly
			List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(linkedTestCaseId);
			Assert.AreEqual(2, testCaseParameters.Count);
			Assert.AreEqual("login", testCaseParameters[0].Name);
			Assert.AreEqual("password", testCaseParameters[1].Name);

			List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(linkedTestStepId);
			Assert.AreEqual(3, testStepParameterValues.Count);
			Assert.AreEqual("browserName", testStepParameterValues[0].Name);
			Assert.AreEqual("Internet Explorer", testStepParameterValues[0].Value);
			Assert.AreEqual("login", testStepParameterValues[1].Name);
			Assert.AreEqual("librarian", testStepParameterValues[1].Value);
			Assert.AreEqual("password", testStepParameterValues[2].Name);
			Assert.AreEqual("librarian", testStepParameterValues[2].Value);

			//Test that we can export a selection of individual test cases and that the
			//linked test case is also handled correctly - put into folder 2
			int exportedTestCaseId1 = testCaseManager.TestCase_Export(USER_ID_FRED_BLOGGS, PROJECT_ID, 2, projectId, testCaseMapping, testFolderId2);

			//Verify that it exported
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, exportedTestCaseId1);
			Assert.AreEqual("Ability to create new book", testCase.Name);
			Assert.IsTrue(testCase.IsTestSteps);
			Assert.IsTrue(testCase.IsAttachments);

			//Now verify the test steps and that it used the previous reference to any linked test cases
			Assert.AreEqual(5, testCase.TestSteps.Count);
			Assert.IsNotNull(testCase.TestSteps[0].LinkedTestCaseId);
			Assert.AreEqual(linkedTestCaseId, testCase.TestSteps[0].LinkedTestCaseId.Value);
			linkedTestStepId = testCase.TestSteps[0].TestStepId;
			Assert.AreEqual("User clicks link to create book", testCase.TestSteps[1].Description);
			Assert.AreEqual(false, testCase.TestSteps[1].IsAttachments, "AttachmentsYn");
			Assert.AreEqual("User enters books name and author, then clicks Next", testCase.TestSteps[2].Description);
			Assert.AreEqual(true, testCase.TestSteps[2].IsAttachments, "AttachmentsYn");

			//Verify the test step parameters were exported correctly
			testStepParameterValues = testCaseManager.RetrieveParameterValues(linkedTestStepId);
			Assert.AreEqual(3, testStepParameterValues.Count);
			Assert.AreEqual("browserName", testStepParameterValues[0].Name);
			Assert.AreEqual("Internet Explorer", testStepParameterValues[0].Value);
			Assert.AreEqual("login", testStepParameterValues[1].Name);
			Assert.AreEqual("librarian", testStepParameterValues[1].Value);
			Assert.AreEqual("password", testStepParameterValues[2].Name);
			Assert.AreEqual("librarian", testStepParameterValues[2].Value);

			//Now we need to test that a repository Rapise test gets exported correctly
			//This was fixed in v5.0
			testCase = testCaseManager.RetrieveById2(projectId, exportedTestCaseId1);
			AttachmentManager attachmentManager = new AttachmentManager();
			Assert.IsNotNull(testCase.AutomationAttachmentId);
			int automationAttachmentId = testCase.AutomationAttachmentId.Value;

			//Get the attachment
			ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, automationAttachmentId);
			Assert.AreEqual(projectId, projectAttachment.ProjectId);
			Assert.AreEqual("CreateNewBook.sstest", projectAttachment.Filename);
			Attachment attachment = attachmentManager.RetrieveById(automationAttachmentId);
			Assert.AreEqual("CreateNewBook.sstest", attachment.Filename);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachment.AttachmentTypeId);

			//Make sure it's not the same attachment id as the source project (needs to be branched)
			testCase = testCaseManager.RetrieveById2(PROJECT_ID, 9);
			Assert.AreNotEqual(testCase.AutomationAttachmentId, automationAttachmentId);

			//Make sure the whole folder was copied over
			int destProjectFolderId = projectAttachment.ProjectAttachmentFolderId;
			ProjectAttachmentFolder destAttachmentFolder = attachmentManager.RetrieveFolderById(destProjectFolderId);
			Assert.AreEqual("CreateNewBook", destAttachmentFolder.Name);
			Assert.AreEqual(projectId, destAttachmentFolder.ProjectId);
			Assert.AreEqual(6, attachmentManager.CountForProject(projectId, destProjectFolderId, null, 0));

			//Also make sure that none of the documents are linked to the versions in the old project
			List<ProjectAttachmentView> projectAttachments = attachmentManager.RetrieveByArtifactId(projectId, exportedTestCaseId1, Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.IsFalse(projectAttachments.Any(p => p.AttachmentId == testCase.AutomationAttachmentId));

			//Now we need to test that the linked/attached test scripts get exported correctly
			//This was fixed in v5.0
			//We need to first export a different test case
			int exportedTestCaseId2 = testCaseManager.TestCase_Export(USER_ID_FRED_BLOGGS, PROJECT_ID, 9, projectId, testCaseMapping, testFolderId2);
			testCase = testCaseManager.RetrieveById2(projectId, exportedTestCaseId2);
			Assert.IsNotNull(testCase.AutomationAttachmentId);
			automationAttachmentId = testCase.AutomationAttachmentId.Value;

			//Get the attachment
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId, automationAttachmentId);
			Assert.AreEqual(projectId, projectAttachment.ProjectId);
			Assert.AreEqual("Web 01 SmarteATM Login.ses", projectAttachment.Filename);
			attachment = attachmentManager.RetrieveById(automationAttachmentId);
			Assert.AreEqual("Web 01 SmarteATM Login.ses", attachment.Filename);
			Assert.AreEqual((int)Attachment.AttachmentTypeEnum.File, attachment.AttachmentTypeId);

			//Make sure it's not the same attachment id as the source project (needs to be branched)
			testCase = testCaseManager.RetrieveById2(PROJECT_ID, 9);
			Assert.AreNotEqual(testCase.AutomationAttachmentId, automationAttachmentId);

			//Also make sure that none of the documents are linked to the versions in the old project
			projectAttachments = attachmentManager.RetrieveByArtifactId(projectId, exportedTestCaseId2, Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.IsFalse(projectAttachments.Any(p => p.AttachmentId == testCase.AutomationAttachmentId));
		}

		/// <summary>
		/// Test that we can bulk refresh all the folder counts in a project
		/// </summary>
		[
		Test,
		SpiraTestCase(390)
		]
		public void _22_RefreshFolderCounts()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Lets create a temporary new project
			ProjectManager projectManager = new ProjectManager();
			TemplateManager templateManager = new TemplateManager();
			int tempProjectId = projectManager.Insert("TestCaseTest Temp Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			int tempTemplateId = templateManager.RetrieveForProject(tempProjectId).ProjectTemplateId;

			//First we need to create a folder and some new test cases in an empty project
			int tempTestFolderId1 = testCaseManager.TestCaseFolder_Create("Test Folder", tempProjectId, null).TestCaseFolderId;

			//Add two test cases and indent under folder
			int tempTestCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, tempProjectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, tempTestFolderId1, null, null, null);
			int tempTestCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, tempProjectId, USER_ID_FRED_BLOGGS, null, "Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, tempTestFolderId1, null, null, null);

			//Verify the folder test case count
			TestCaseFolder testFolder = testCaseManager.TestCaseFolder_GetById(tempTestFolderId1);
			Assert.AreEqual(2, testFolder.CountNotRun);

			//Now delete one of the test cases directly in the database
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_TEST_CASE WHERE TEST_CASE_ID = " + tempTestCaseId1.ToString());

			//Verify the folder test case count is not incorrect
			testFolder = testCaseManager.TestCaseFolder_GetById(tempTestFolderId1);
			Assert.AreEqual(2, testFolder.CountNotRun);

			//Then force the complete refresh of the project test count data
			testCaseManager.RefreshFolderCounts(tempProjectId);

			//Now check that it updated the folder count
			testFolder = testCaseManager.TestCaseFolder_GetById(tempTestFolderId1);
			Assert.AreEqual(1, testFolder.CountNotRun);

			//Delete the temporary project and template
			projectManager.Delete(USER_ID_FRED_BLOGGS, tempProjectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, tempTemplateId);
		}

		[
		Test,
		SpiraTestCase(218)
		]
		public void _23_ListTestCases()
		{
			//Lets test that we can get a list of all visible test-cases regardless of folder
			//There should only be 11 visible by default
			List<TestCaseView> testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(15, testCases.Count);

			//Lets test that we can retrieve the test cases in a specific folder
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, 0, 1);
			Assert.AreEqual(5, testCases.Count);
			Assert.AreEqual("Ability to create new author", testCases[0].Name);
			Assert.AreEqual("Ability to reassign book to different author", testCases[4].Name);

			//Test that we can retrieve all the root test cases (none)
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, 0, null);
			Assert.AreEqual(0, testCases.Count);

			//Test that we can retrieve all the top-level folders
			List<TestCaseFolder> testCaseFolders = testCaseManager.TestCaseFolder_GetByParentId(PROJECT_ID, null);
			Assert.AreEqual(4, testCaseFolders.Count);
			Assert.AreEqual("Common Tests", testCaseFolders[0].Name);
			Assert.AreEqual("Scenario Tests", testCaseFolders[3].Name);
			int testFolderId = testCaseFolders[3].TestCaseFolderId;

			//Test that we can retrieve all the test folders in a folder
			testCaseFolders = testCaseManager.TestCaseFolder_GetByParentId(PROJECT_ID, testFolderId);
			Assert.AreEqual(1, testCaseFolders.Count);
			Assert.AreEqual("Exception Scenario Tests", testCaseFolders[0].Name);
		}

		[
		Test,
		SpiraTestCase(222)
		]
		public void _24_ListTestSteps()
		{
			//Lets test that we can retrieve a test case with its associated test steps
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(PROJECT_ID, 4);
			Assert.IsNotNull(testCase);
			Assert.AreEqual(true, testCase.IsTestSteps);
			Assert.AreEqual(5, testCase.TestSteps.Count);
			Assert.AreEqual(2, testCase.TestSteps[1].ExecutionStatusId);
			Assert.AreEqual("User clicks link to create author", testCase.TestSteps[1].Description);
			Assert.AreEqual(2, testCase.TestSteps[1].Position);
			Assert.AreEqual("User taken to first screen in wizard", testCase.TestSteps[1].ExpectedResult);
			Assert.AreEqual(false, testCase.TestSteps[1].IsAttachments);
			Assert.IsTrue(testCase.TestSteps[1].SampleData.IsNull());
			Assert.AreEqual("Passed", testCase.TestSteps[1].ExecutionStatusName);

			//Lets test that we can retrieve a test case that has test steps with custom property values set
			List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(2);
			Assert.AreEqual("User enters books name and author, then clicks Next", testSteps[2].Description);
			Assert.AreEqual("Some Data", testSteps[2].Custom_01);

			//Now test that we can retrieve a single test step
			TestStep testStep = testCaseManager.RetrieveStepById(PROJECT_ID, 7);

			//Verify the data
			Assert.AreEqual("User clicks link to create author", testStep.Description);
			Assert.AreEqual("User taken to first screen in wizard", testStep.ExpectedResult);
			Assert.IsTrue(testStep.SampleData.IsNull());
			Assert.IsFalse(testStep.LinkedTestCaseId.HasValue);

			//Now test that we can retrieve a single test step view
			TestStepView testStepView = testCaseManager.RetrieveStepById2(4, 7);

			//Verify the data
			Assert.AreEqual("User clicks link to create author", testStepView.Description);
			Assert.AreEqual("User taken to first screen in wizard", testStepView.ExpectedResult);
			Assert.IsTrue(testStepView.SampleData.IsNull());
			Assert.IsFalse(testStepView.LinkedTestCaseId.HasValue);
		}

		[
		Test,
		SpiraTestCase(234)
		]
		public void _26_RetrieveTestCasesWithFilters()
		{
			//First lets test that we can retrieve the test cases without any specific filters in a folder
			//Verify some of the rollup information
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(1);
			List<TestCaseView> testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, 0, 1);

			//The top-level folder
			Assert.AreEqual(1, testCaseFolder.CountPassed, "FolderCountPassed");
			Assert.AreEqual(2, testCaseFolder.CountFailed, "FolderCountFailed");
			Assert.AreEqual(2, testCaseFolder.CountCaution, "FolderCountCaution");
			Assert.AreEqual(0, testCaseFolder.CountBlocked, "FolderCountBlocked");
			Assert.AreEqual(0, testCaseFolder.CountNotRun, "FolderCountNotRun");
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);

			//The constituent test cases
			Assert.AreEqual(5, testCases.Count);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCases[0].ExecutionStatusId, "Caution - TC:" + testCases[0].TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[1].ExecutionStatusId, "Failed - TC:" + testCases[1].TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCases[2].ExecutionStatusId, "Caution - TC:" + testCases[2].TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[3].ExecutionStatusId, "Failed - TC:" + testCases[3].TestCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCases[4].ExecutionStatusId, "Passed - TC:" + testCases[4].TestCaseId);

			//Lets test that we can retrieve the count of test cases in the system (all folders) with a filter applied
			//Also test that we can retrieve the count independent of pagination and also the total test case count
			//independent of filters
			Hashtable filters = new Hashtable();

			//Filter by ExecutionStatus=Failed
			filters.Add("ExecutionStatusId", 1);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(2, testCases.Count);
			Assert.AreEqual(2, testCaseManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, null, false, true));
			Assert.AreEqual(15, testCaseManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET, null, false, true));

			//Filter by Owner=FredBloggs
			filters.Clear();
			filters.Add("OwnerId", USER_ID_FRED_BLOGGS);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(2, testCases.Count, "Owner filter failed");
			Assert.AreEqual(2, testCaseManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, null, false, true));
			Assert.AreEqual(15, testCaseManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET, null, false, true));

			//Filter by Author=Joe Smith
			filters.Clear();
			filters.Add("AuthorId", USER_ID_JOE_SMITH);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(6, testCases.Count);
			Assert.AreEqual(6, testCaseManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET, null, false, true));
			Assert.AreEqual(15, testCaseManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET, null, false, true));

			//Filter by Name LIKE 'book'
			filters.Clear();
			filters.Add("Name", "book");
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(9, testCases.Count);

			//Filter by TestCaseId=3
			filters.Clear();
			filters.Add("TestCaseId", 3);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(1, testCases.Count);

			//Filter by CreationDate
			filters.Clear();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.AddDays(-125);
			filters.Add("CreationDate", dateRange);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(15, testCases.Count);

			//Filter by ExecutionDate
			filters.Clear();
			dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.AddDays(-20);
			dateRange.EndDate = DateTime.UtcNow;
			filters.Add("ExecutionDate", dateRange);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(4, testCases.Count, "ExecutionDate");

			//Filter by Status = Ready To Test
			filters.Clear();
			filters.Add("TestCaseStatusId", (int)TestCase.TestCaseStatusEnum.ReadyForTest);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(10, testCases.Count);

			//Filter by Status = Obsolete
			filters.Clear();
			filters.Add("TestCaseStatusId", (int)TestCase.TestCaseStatusEnum.Obsolete);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(0, testCases.Count);

			//Lets test that we can apply more than one filter
			filters.Clear();
			filters.Add("Name", "book");
			filters.Add("ExecutionStatusId", 2);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(3, testCases.Count, "More than one filter failed");

			//Now test that we can retrieve test cases with multiple execution statuses
			//Lets get all the test cases that have Passed or Not Run
			filters.Clear();
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add((int)TestCase.ExecutionStatusEnum.Passed);
			multiValueFilter.Values.Add((int)TestCase.ExecutionStatusEnum.NotRun);

			//Add the status list to the filter
			filters.Add("ExecutionStatusId", multiValueFilter);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(11, testCases.Count, "ExecutionStatusId");
			//Make sure we only have the expected status codes
			for (int i = 0; i < testCases.Count; i++)
			{
				Assert.IsTrue(testCases[i].ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Passed || testCases[i].ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun);
			}

			//Now lets try filtering on some of the custom properties
			//Filter on Test Type=Functional Test
			filters.Clear();
			filters.Add("Custom_02", 6);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(1, testCases.Count);
			Assert.AreEqual("Ability to create new book", testCases[0].Name);
			Assert.AreEqual(2, testCases[0].TestCaseId);
			Assert.AreEqual(6, testCases[0].Custom_02.FromDatabaseSerialization_Int32().Value);
			Assert.IsTrue(testCases[0].Custom_03.IsNull());
			Assert.AreEqual("http://www.libraryreferences.org", testCases[0].Custom_01);

			//Filter on URL LIKE 'http://www'
			filters.Clear();
			filters.Add("Custom_01", "http://www");
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(1, testCases.Count);
			Assert.AreEqual("Ability to create new book", testCases[0].Name);
			Assert.AreEqual(2, testCases[0].TestCaseId);
			Assert.AreEqual(6, testCases[0].Custom_02.FromDatabaseSerialization_Int32().Value);
			Assert.IsTrue(testCases[0].Custom_03.IsNull());
			Assert.AreEqual("http://www.libraryreferences.org", testCases[0].Custom_01);

			//Next lets get all test cases that have certain fields set to null
			filters.Clear();
			MultiValueFilter multiFilterValue = new MultiValueFilter();
			multiFilterValue.IsNone = true;
			filters.Add("OwnerId", multiFilterValue);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(10, testCases.Count);

			//Now lets test that we can get several test cases by their IDs using a multivalue filter
			multiFilterValue.Clear();
			multiFilterValue.Values.Add(3);
			multiFilterValue.Values.Add(5);
			filters.Clear();
			filters.Add("TestCaseId", multiFilterValue);
			testCases = testCaseManager.Retrieve(PROJECT_ID, "TestCaseId", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			Assert.AreEqual(2, testCases.Count);
			Assert.AreEqual(3, testCases[0].TestCaseId);
			Assert.AreEqual(5, testCases[1].TestCaseId);

			//Now lets test that filtering on a test case folder will include its child test cases
			testCases = testCaseManager.Retrieve(PROJECT_ID, "TestCaseId", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, 2);
			Assert.AreEqual(2, testCases.Count);
			Assert.AreEqual(8, testCases[0].TestCaseId);
			Assert.AreEqual(9, testCases[1].TestCaseId);
		}

		[
		Test,
		SpiraTestCase(237)
		]
		public void _27_RetrieveTestCasesByRelease()
		{
			//Lets test that we can retrieve test cases for a specific release (no additional filters)
			TestCaseFolderReleaseView testCaseFolder = testCaseManager.TestCaseFolder_GetByParentIdForRelease(PROJECT_ID, null, 1).First();
			List<TestCaseReleaseView> testCases = testCaseManager.RetrieveByReleaseId(PROJECT_ID, 1, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, 1);

			//Folder
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountPassed);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			//Test Cases
			Assert.AreEqual(5, testCases.Count);
			//Row 1
			Assert.AreEqual("Ability to create new author", testCases[0].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[0].ExecutionStatusId);
			Assert.AreEqual("Failed", testCases[0].ExecutionStatusName);
			//Row 2
			Assert.AreEqual("Ability to create new book", testCases[1].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[1].ExecutionStatusId);
			Assert.AreEqual("Failed", testCases[1].ExecutionStatusName);
			//Row 3
			Assert.AreEqual("Ability to edit existing author", testCases[2].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCases[2].ExecutionStatusId);
			Assert.AreEqual("Passed", testCases[2].ExecutionStatusName);
			//Row 4
			Assert.AreEqual("Ability to edit existing book", testCases[3].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCases[3].ExecutionStatusId);
			Assert.AreEqual("Caution", testCases[3].ExecutionStatusName);

			//Now verify that we can filter this by execution status
			Hashtable filters = new Hashtable();
			filters.Add("ExecutionStatusId", (int)TestCase.ExecutionStatusEnum.Failed);
			testCases = testCaseManager.RetrieveByReleaseId(PROJECT_ID, 1, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, 1);
			int artifactCount = testCaseManager.CountByRelease(PROJECT_ID, 1, filters, InternalRoutines.UTC_OFFSET, 1);

			//Verify the count as well as the execution status
			Assert.AreEqual(2, testCases.Count);
			Assert.AreEqual(2, artifactCount);
			Assert.AreEqual("Ability to create new author", testCases[0].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[0].ExecutionStatusId);
			Assert.AreEqual("Failed", testCases[0].ExecutionStatusName);

			//Now verify the count independent of pagination and the count independent of filters
			Assert.AreEqual(2, artifactCount);
			Assert.AreEqual(7, testCaseManager.CountByRelease(PROJECT_ID, 1, null, InternalRoutines.UTC_OFFSET, null, false, true));

			//Lets test that we can retrieve test cases for a release that has execution tracked by its
			//child iterations rather than the release itself - test that they rollup correctly
			testCaseFolder = testCaseManager.TestCaseFolder_GetByParentIdForRelease(PROJECT_ID, null, 4).First();
			testCases = testCaseManager.RetrieveByReleaseId(PROJECT_ID, 4, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, 1);

			//Verify the count as well as the execution status
			//Folder
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(2, testCaseFolder.CountFailed);
			Assert.AreEqual(2, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			//Test Cases
			Assert.AreEqual(5, testCases.Count);
			//Row 1
			Assert.AreEqual("Ability to create new author", testCases[0].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCases[0].ExecutionStatusId);
			Assert.AreEqual("Caution", testCases[0].ExecutionStatusName);
			//Row 2
			Assert.AreEqual("Ability to create new book", testCases[1].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[1].ExecutionStatusId);
			Assert.AreEqual("Failed", testCases[1].ExecutionStatusName);
			//Row 3
			Assert.AreEqual("Ability to edit existing author", testCases[2].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCases[2].ExecutionStatusId);
			Assert.AreEqual("Caution", testCases[2].ExecutionStatusName);
			//Row 4
			Assert.AreEqual("Ability to edit existing book", testCases[3].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCases[3].ExecutionStatusId);
			Assert.AreEqual("Failed", testCases[3].ExecutionStatusName);

			//Now verify that we can filter by release and execution status = not run
			//Previously this failed because of our use of outer joins. Fixed in v2.0.1
			filters.Clear();
			filters.Add("ExecutionStatusId", (int)TestCase.ExecutionStatusEnum.NotRun);
			testCaseFolder = testCaseManager.TestCaseFolder_GetByParentIdForRelease(PROJECT_ID, null, 4, null, true, filters, InternalRoutines.UTC_OFFSET).First();
			testCases = testCaseManager.RetrieveByReleaseId(PROJECT_ID, 4, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET, 1);

			//Folder
			Assert.AreEqual("Functional Tests", testCaseFolder.Name);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(2, testCaseFolder.CountFailed);
			Assert.AreEqual(2, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);

			//Test Cases
			Assert.AreEqual(1, testCases.Count);
			//Row 1
			Assert.AreEqual("Ability to reassign book to different author", testCases[0].Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCases[0].ExecutionStatusId);
			Assert.AreEqual("Not Run", testCases[0].ExecutionStatusName);

		}


		/// <summary>
		/// Tests that you can create, edit, delete testCase types
		/// </summary>
		[Test]
		[SpiraTestCase(1730)]
		public void _28_EditTypes()
		{
			//First lets get the list of types in the current template
			List<TestCaseType> types = testCaseManager.TestCaseType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(12, types.Count);

			//Next lets add a new type
			int testCaseTypeId1 = testCaseManager.TestCaseType_Insert(
				projectTemplateId,
				"Smoke Test",
				null,
				false,
				true
				);

			//Verify that it was created
			TestCaseType testCaseType = testCaseManager.TestCaseType_RetrieveById(testCaseTypeId1);
			Assert.IsNotNull(testCaseType);
			Assert.AreEqual("Smoke Test", testCaseType.Name);
			Assert.AreEqual(true, testCaseType.IsActive);
			Assert.AreEqual(false, testCaseType.IsDefault);

			//Next lets add another new type
			int testCaseTypeId2 = testCaseManager.TestCaseType_Insert(
				projectTemplateId,
				"Edge Test",
				null,
				false,
				true
				);

			//Verify that it was created
			testCaseType = testCaseManager.TestCaseType_RetrieveById(testCaseTypeId2);
			Assert.IsNotNull(testCaseType);
			Assert.AreEqual("Edge Test", testCaseType.Name);
			Assert.AreEqual(true, testCaseType.IsActive);
			Assert.AreEqual(false, testCaseType.IsDefault);

			//Make changes
			testCaseType.StartTracking();
			testCaseType.Name = "Boundary Test";
			testCaseType.IsActive = false;
			testCaseManager.TestCaseType_Update(testCaseType);

			//Verify the changes
			//Verify that it was created
			testCaseType = testCaseManager.TestCaseType_RetrieveById(testCaseTypeId2);
			Assert.IsNotNull(testCaseType);
			Assert.AreEqual("Boundary Test", testCaseType.Name);
			Assert.AreEqual(false, testCaseType.IsActive);
			Assert.AreEqual(false, testCaseType.IsDefault);

			//Verify that we can get the total count of types
			types = testCaseManager.TestCaseType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(14, types.Count);

			//Verify that we can get the total count of active types
			types = testCaseManager.TestCaseType_Retrieve(projectTemplateId, true);
			Assert.AreEqual(13, types.Count);

			//Verify that we can get the default type
			testCaseType = testCaseManager.TestCaseType_RetrieveDefault(projectTemplateId);
			Assert.IsNotNull(testCaseType);
			Assert.AreEqual("Functional", testCaseType.Name);
			Assert.AreEqual(true, testCaseType.IsActive);
			Assert.AreEqual(true, testCaseType.IsDefault);

			//Delete our new types (internal function only, not possible in the UI)
			testCaseManager.TestCaseType_Delete(testCaseTypeId1);
			testCaseManager.TestCaseType_Delete(testCaseTypeId2);

			//Verify the count
			types = testCaseManager.TestCaseType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(12, types.Count);
		}

		/// <summary>
		/// Tests that you can create, edit, delete testCase priorities
		/// </summary>
		[Test]
		[SpiraTestCase(1731)]
		public void _29_EditPriorities()
		{
			//First lets get the list of priorities in the current template
			List<TestCasePriority> priorities = testCaseManager.TestCasePriority_Retrieve(projectTemplateId);
			Assert.AreEqual(4, priorities.Count);

			//Next lets add a new priority
			int priorityId1 = testCaseManager.TestCasePriority_Insert(
				projectTemplateId,
				"5 - Minor",
				"eeeeee",
				true,
				5
				);

			//Verify that it was created
			TestCasePriority priority = testCaseManager.TestCasePriority_RetrieveById(priorityId1);
			Assert.IsNotNull(priority);
			Assert.AreEqual("5 - Minor", priority.Name);
			Assert.AreEqual(true, priority.IsActive);
			Assert.AreEqual("eeeeee", priority.Color);
			Assert.AreEqual(5, priority.Score);

			//Make changes
			priority.StartTracking();
			priority.Name = "5 - Cosmetic";
			priority.Color = "dddddd";
			priority.Score = 6;
			testCaseManager.TestCasePriority_Update(priority);

			//Verify the changes
			priority = testCaseManager.TestCasePriority_RetrieveById(priorityId1);
			Assert.IsNotNull(priority);
			Assert.AreEqual("5 - Cosmetic", priority.Name);
			Assert.AreEqual(true, priority.IsActive);
			Assert.AreEqual("dddddd", priority.Color);
			Assert.AreEqual(6, priority.Score);

			//Verify that we can get the total count of priorities
			priorities = testCaseManager.TestCasePriority_Retrieve(projectTemplateId);
			Assert.AreEqual(5, priorities.Count);

			//Delete our new priorities (internal function only, not possible in the UI)
			testCaseManager.TestCasePriority_Delete(priorityId1);

			//Verify the count
			priorities = testCaseManager.TestCasePriority_Retrieve(projectTemplateId);
			Assert.AreEqual(4, priorities.Count);
		}


		[Test]
		[SpiraTestCase(1732)]
		public void _30_PreRequisites()
		{
			string result = "";

			//CustomPropertyList customListId3 = new CustomPropertyManager().CustomPropertyList_Add(projectTemplateId, "Test Prerequisites", false, true);

			CustomProperty customPropertyId3 = new CustomPropertyManager().CustomPropertyDefinition_AddToArtifact(
					projectTemplateId,
					Artifact.ArtifactTypeEnum.TestCase,
					(int)CustomProperty.CustomPropertyTypeEnum.User,
					1,
					"Test Prerequisites",
					"Test Prerequisites",
					null,
					null);

			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

			ArtifactCustomProperty artifactCustomProperty = new CustomPropertyManager().ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, false, customProperties);
			if (artifactCustomProperty == null)
			{

				customProperties.Add(customPropertyId3);
				//customProperties.Add(customListId3);
				artifactCustomProperty = new CustomPropertyManager().ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestCase, testCaseId1, customProperties);
			}
			new CustomPropertyManager().ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_SYSTEM_ADMIN);

			ArtifactCustomProperty artifactCustomProperty1 = new CustomPropertyManager().ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, true);

			foreach (var def in artifactCustomProperty1.CustomPropertyDefinitions)
			{
				var name = def.Name;
				if (name == "Test Prerequisites")
				{
					result = def.Description;
				}
				else if (name == "Pre-Requisites")
				{
					result = def.Description;
				}
			}
			Assert.AreEqual("Test Prerequisites", result);
		}
	}
}

