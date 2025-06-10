using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using NUnit.Framework;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the TestRun business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class TestRunTest
	{
		private static TestRunManager testRunManager;
		private static TestCaseManager testCaseManager;
		private static IncidentManager incidentManager;

		protected static int testFolderId1;
		protected static int testFolderId2;
		protected static int testFolderId3;
		protected static int testFolderId4;
		protected static int testFolderId5;

		private static int testCaseId1;
		private static int testCaseId2;
		private static int testCaseId3;
		private static int testCaseId4;
		private static int testCaseId5;
		private static int testCaseId6;
		private static int testCaseId7;

		private static int testStepId1;

		private static int testRunId1;
		private static int testRunId2;
		private static int testRunId3;
		private static int testRunId4;

		private static int incidentId1;

		private static int projectId;
		private static int projectTemplateId;

		private static int releaseId1;
		private static int releaseId2;

		private static int requirementId1;
		private static int requirementId2;

		private static int customListId1;
		private static int customListId2;

		private static int customValueId11;
		private static int customValueId12;
		private static int customValueId13;
		private static int customValueId14;

		private static int customValueId21;
		private static int customValueId22;
		private static int customValueId23;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		private const int PROJECT_ID = 1;

		private static int automationEngineId;

		private static int tc_priorityCriticalId;
		private static int tc_priorityHighId;
		private static int tc_priorityMediumId;
		private static int tc_priorityLowId;

		private static int tc_typeScenarioId;
		private static int tc_typeAcceptanceId;
		private static int tc_typeUnitId;
		private static int tc_typeIntegrationId;
		private static int tc_typeExploratoryId;

		[TestFixtureSetUp]
		public void Init()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			testRunManager = new TestRunManager();
			testCaseManager = new TestCaseManager();
			incidentManager = new IncidentManager();

			//Create a new project for testing with
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("TestRunTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Add some custom lists for testing platforms and notes
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			customListId1 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Operating Systems").CustomPropertyListId;
			customValueId11 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Windows 7").CustomPropertyValueId;
			customValueId12 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Windows 8").CustomPropertyValueId;
			customValueId13 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "MacOS X").CustomPropertyValueId;
			customValueId14 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Linuz").CustomPropertyValueId;

			customListId2 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Web Browsers").CustomPropertyListId;
			customValueId21 = customPropertyManager.CustomPropertyList_AddValue(customListId2, "Firefox").CustomPropertyValueId;
			customValueId22 = customPropertyManager.CustomPropertyList_AddValue(customListId2, "Chrome").CustomPropertyValueId;
			customValueId23 = customPropertyManager.CustomPropertyList_AddValue(customListId2, "Safari").CustomPropertyValueId;

			//Add the custom properties to Incidents, test runs and test sets
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.Text, 1, "Notes", null, null, null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.List, 2, "OS", null, null, customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.MultiList, 4, "Browser", null, null, customListId2);

			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, (int)CustomProperty.CustomPropertyTypeEnum.Text, 1, "Notes", null, null, null);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, (int)CustomProperty.CustomPropertyTypeEnum.List, 2, "OS", null, null, customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, (int)CustomProperty.CustomPropertyTypeEnum.MultiList, 4, "Browser", null, null, customListId2);

			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "OS", null, null, customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, (int)CustomProperty.CustomPropertyTypeEnum.MultiList, 2, "Browser", null, null, customListId2);

			//Create a new automation engine to use, unless it already exists            
			AutomationManager automationManager = new AutomationManager();
			try
			{
				AutomationEngine engine = automationManager.RetrieveEngineByToken("Engine1");
				automationEngineId = engine.AutomationEngineId;
			}
			catch (ArtifactNotExistsException)
			{
				string adminSectionName1 = "Test Automation";
				var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

				int adminSectionId1 = adminSection1.ADMIN_SECTION_ID;
				automationEngineId = automationManager.InsertEngine("Automation Engine 1", "Engine1", "", true, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, adminSectionId1, "Inserted Test Automation");
			}

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
			tc_typeExploratoryId = types.FirstOrDefault(t => t.Name == "Exploratory").TestCaseTypeId;
			tc_typeUnitId = types.FirstOrDefault(t => t.Name == "Unit").TestCaseTypeId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);

			//Delete the automation engine
			AutomationManager automationManager = new AutomationManager();
			automationManager.DeleteEngine(automationEngineId, USER_ID_FRED_BLOGGS);

		}

		/// <summary>
		/// Tests that we can create a test run shell from a particular set of test cases and that any links are recursively
		/// handled with all link parameters resolved correctly
		/// </summary>
		[
		Test,
		SpiraTestCase(238)
		]
		public void _01_CreateFromTestCase()
		{
			//First we need to create some new test cases in a single folder in this project
			testFolderId1 = testCaseManager.TestCaseFolder_Create("Functional Tests", projectId, null, null).TestCaseFolderId;
			testFolderId2 = testCaseManager.TestCaseFolder_Create("Regression Tests", projectId, "Tests that the application works on different platforms", null).TestCaseFolderId;
			testFolderId3 = testCaseManager.TestCaseFolder_Create("Unit Tests", projectId, "All of the NUnit and jUnit automated unit tests", null).TestCaseFolderId;
			testFolderId4 = testCaseManager.TestCaseFolder_Create("Acceptance Tests", projectId, null, testFolderId1).TestCaseFolderId;
			testFolderId5 = testCaseManager.TestCaseFolder_Create("Common Tests", projectId, null, testFolderId1).TestCaseFolderId;

			//Now create some test cases, assign some to Fred/Joe
			testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Ability to create new book", "Description of ability to create new book", null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityCriticalId, testFolderId1, 20, null, null);
			testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Ability to update existing book", "Description of ability to update existing book", null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityCriticalId, testFolderId1, 30, null, null);
			testCaseId3 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Ability to delete existing book", "Description of ability to delete existing book", null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityCriticalId, testFolderId1, 10, null, null);
			testCaseId4 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, "Book management testing", "Tests the overall book management functionality", tc_typeScenarioId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityCriticalId, testFolderId4, 40, null, null);
			testCaseId5 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, "Author management testing", "Tests the overall author management functionality", tc_typeScenarioId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityCriticalId, testFolderId4, 40, null, null);

			//Create some common tests (for use in links)
			testCaseId6 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, "Open up web browser", "Generic test case for opening web browser", null, TestCase.TestCaseStatusEnum.Approved, tc_priorityCriticalId, testFolderId5, 10, null, null);
			testCaseId7 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, "Login to application", "Generic login test case", null, TestCase.TestCaseStatusEnum.Approved, tc_priorityCriticalId, testFolderId5, 15, null, null);

			//Now add some parameters to these tests
			testCaseManager.InsertParameter(projectId, testCaseId6, "browserName", "Firefox", USER_ID_FRED_BLOGGS);
			testCaseManager.InsertParameter(projectId, testCaseId7, "login", null, USER_ID_FRED_BLOGGS);
			testCaseManager.InsertParameter(projectId, testCaseId7, "password", null, USER_ID_FRED_BLOGGS);

			//Now add the test steps and test links to these tests
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId6, null, "Open up ${browserName} web browser", "${browserName} successfully opens", null);

			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId7, null, testCaseId6, null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId7, null, "Enter the login and password", "Successfully logged in and taken to home page", "Login: ${login}, Password: ${password}");

			Dictionary<string, string> parameterValues = new Dictionary<string, string>();
			parameterValues.Add("login", "librarian");
			parameterValues.Add("password", "changeme");
			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId1, null, testCaseId7, parameterValues);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, null, "Choose option to create book", "Book creation screen displayed", null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, null, "Enter in book information", "Data accepted", "Some sample data");
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, null, "User clicks Submit button", "Book is created", null);

			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId2, null, testCaseId7, parameterValues);
			testStepId1 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId2, null, "Choose option to edit book", "Book update screen displayed", null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId2, null, "Update book information", "Data accepted", "Some sample data");
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId2, null, "User clicks Submit button", "Book is updated", null);

			parameterValues["login"] = "admin";
			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId3, null, testCaseId7, parameterValues);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId3, null, "Choose option to delete book", "Book list screen displayed", null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId3, null, "User clicks Delete button next to book", "Book is deleted", null);

			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId4, null, testCaseId1, null);
			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId4, null, testCaseId2, null);
			testCaseManager.InsertLink(USER_ID_FRED_BLOGGS, testCaseId4, null, testCaseId3, null);

			//Leave the last case without steps

			//Obtain the highest History entry to verify that it was set properly.
			long? latestHistory = GetHistoryId();

			//Now test that we can create a set of test runs with associated test run steps from a list of test cases
			//For this run we don't want to associate it with a particular release
			List<int> testCaseIdList = new List<int>();
			testCaseIdList.Add(testCaseId1);
			testCaseIdList.Add(testCaseId2);
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, null, testCaseIdList, false);

			//First make sure we have the expected number of test cases and test steps
			Assert.AreEqual(2, testRunsPending.TestRuns.Count);
			Assert.AreEqual(10, testRunsPending.TestRuns.Sum(t => t.TestRunSteps.Count));

			//Now make sure we have the test case names, descriptions, estimated durations, and the ChansetSetId we expect
			Assert.AreEqual("Ability to create new book", testRunsPending.TestRuns[0].Name);
			Assert.AreEqual("Description of ability to create new book", testRunsPending.TestRuns[0].Description);
			Assert.AreEqual(20, testRunsPending.TestRuns[0].EstimatedDuration);
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[0].ChangeSetId, "TestRun's Changeset did not match what was expected.");
			Assert.AreEqual("Ability to update existing book", testRunsPending.TestRuns[1].Name);
			Assert.AreEqual("Description of ability to update existing book", testRunsPending.TestRuns[1].Description);
			Assert.AreEqual(30, testRunsPending.TestRuns[1].EstimatedDuration);
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[1].ChangeSetId, "TestRun's Changeset did not match what was expected.");

			//Now make sure we have the test step names and positions we expect and parameter values are as expected

			//First test case
			Assert.AreEqual("Open up Firefox web browser", testRunsPending.TestRuns[0].TestRunSteps[0].Description);
			Assert.AreEqual("Firefox successfully opens", testRunsPending.TestRuns[0].TestRunSteps[0].ExpectedResult);
			Assert.AreEqual(1, testRunsPending.TestRuns[0].TestRunSteps[0].Position);
			Assert.AreEqual("Enter the login and password", testRunsPending.TestRuns[0].TestRunSteps[1].Description);
			Assert.AreEqual("Login: librarian, Password: changeme", testRunsPending.TestRuns[0].TestRunSteps[1].SampleData);
			Assert.AreEqual(2, testRunsPending.TestRuns[0].TestRunSteps[1].Position);
			Assert.AreEqual("Choose option to create book", testRunsPending.TestRuns[0].TestRunSteps[2].Description);
			Assert.AreEqual("Book creation screen displayed", testRunsPending.TestRuns[0].TestRunSteps[2].ExpectedResult);
			Assert.AreEqual(3, testRunsPending.TestRuns[0].TestRunSteps[2].Position);

			//Second test case
			Assert.AreEqual("Open up Firefox web browser", testRunsPending.TestRuns[1].TestRunSteps[0].Description);
			Assert.AreEqual("Firefox successfully opens", testRunsPending.TestRuns[1].TestRunSteps[0].ExpectedResult);
			Assert.AreEqual(1, testRunsPending.TestRuns[1].TestRunSteps[0].Position);
			Assert.AreEqual("Enter the login and password", testRunsPending.TestRuns[1].TestRunSteps[1].Description);
			Assert.AreEqual("Login: librarian, Password: changeme", testRunsPending.TestRuns[1].TestRunSteps[1].SampleData);
			Assert.AreEqual(2, testRunsPending.TestRuns[1].TestRunSteps[1].Position);
			Assert.AreEqual("Choose option to edit book", testRunsPending.TestRuns[1].TestRunSteps[2].Description);
			Assert.AreEqual("Book update screen displayed", testRunsPending.TestRuns[1].TestRunSteps[2].ExpectedResult);
			Assert.AreEqual(3, testRunsPending.TestRuns[1].TestRunSteps[2].Position);

			//Make sure all execution status codes are set to not run
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[0].TestRunSteps[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[0].TestRunSteps[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[0].TestRunSteps[3].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[0].TestRunSteps[4].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].TestRunSteps[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].TestRunSteps[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].TestRunSteps[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].TestRunSteps[3].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].TestRunSteps[4].ExecutionStatusId);

			//Make sure all execution (end) dates are not set
			Assert.IsNotNull(testRunsPending.TestRuns[0].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[0].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[0].TestRunSteps[0].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[0].TestRunSteps[1].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[0].TestRunSteps[2].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[0].TestRunSteps[3].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[0].TestRunSteps[4].EndDate);
			Assert.IsNotNull(testRunsPending.TestRuns[1].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[1].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[1].TestRunSteps[0].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[1].TestRunSteps[1].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[1].TestRunSteps[2].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[1].TestRunSteps[3].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[1].TestRunSteps[4].EndDate);

			//Finally make sure we can also create a pending run set from a whole folder
			List<int> testFolderExecutionList = new List<int>();
			testFolderExecutionList.Add(testFolderId1);
			testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, null, null, false, testFolderExecutionList);

			//First make sure we have the expected number of test cases and test steps
			Assert.AreEqual(7, testRunsPending.TestRuns.Count);
			Assert.AreEqual(31, testRunsPending.TestRuns.Sum(t => t.TestRunSteps.Count));
		}

		[
		Test,
		SpiraTestCase(239)
		]
		public void _02_SaveTestRun()
		{
			//Let's create two releases to track our results
			ReleaseManager releaseManager = new ReleaseManager();
			releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(2), 2, 0, null, false);
			releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0", null, "2.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow.AddMonths(2), DateTime.UtcNow.AddMonths(4), 2, 0, null, false);

			//Also link the releases to some requirements
			RequirementManager requirementManager = new RequirementManager();
			requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);

			//Obtain the highest History entry to verify that it was set properly.
			long? latestHistory = GetHistoryId();

			//Now lets create the test run from our three functional test cases
			List<int> testCaseExecutionList = new List<int>();
			testCaseExecutionList.Add(testCaseId1);
			testCaseExecutionList.Add(testCaseId2);
			testCaseExecutionList.Add(testCaseId3);
			testCaseExecutionList.Add(testCaseId4);
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, releaseId1, testCaseExecutionList, true);

			//Add the test cases to both releases
			testCaseManager.AddToRelease(projectId, releaseId1, testCaseExecutionList, USER_ID_SYS_ADMIN);
			testCaseManager.AddToRelease(projectId, releaseId2, testCaseExecutionList, USER_ID_SYS_ADMIN);

			//Add the test cases to the requirements
			testCaseManager.AddToRequirement(projectId, requirementId1, new List<int>() { testCaseId1, testCaseId2 }, 1);
			testCaseManager.AddToRequirement(projectId, requirementId2, new List<int>() { testCaseId3, testCaseId4 }, 1);

			//Verify that it saved the pending set of test runs and retrieve back
			int pendingId = testRunsPending.TestRunsPendingId;
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			Assert.AreEqual(4, testRunsPending.CountNotRun);
			Assert.AreEqual(4, testRunsPending.TestRuns.Count);

			//Make sure all execution status codes are set to not run and that the end date is null
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[3].ExecutionStatusId);
			Assert.IsNotNull(testRunsPending.TestRuns[0].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[0].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[0].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[1].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[1].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[1].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[2].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[2].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[2].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[3].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[3].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[3].ActualDuration);

			//Verify the ChangeSet ID on these.
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[0].ChangeSetId, "ChangeSetId of Test Run did not match.");
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[1].ChangeSetId, "ChangeSetId of Test Run did not match.");
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[2].ChangeSetId, "ChangeSetId of Test Run did not match.");
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[3].ChangeSetId, "ChangeSetId of Test Run did not match.");

			//Make sure it's associated with a specific release
			Assert.AreEqual(releaseId1, testRunsPending.TestRuns[0].ReleaseId);
			Assert.AreEqual(releaseId1, testRunsPending.TestRuns[1].ReleaseId);
			Assert.AreEqual(releaseId1, testRunsPending.TestRuns[2].ReleaseId);
			Assert.AreEqual(releaseId1, testRunsPending.TestRuns[3].ReleaseId);

			//TODO: Test that any default custom property values are set on the test run

			//For the first test run we shall pass a couple of steps and leave one step as not run
			//also need to set the dates and durations
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[0].TestRunSteps[0].StartDate = DateTime.UtcNow;
			testRunsPending.TestRuns[0].TestRunSteps[0].EndDate = DateTime.UtcNow.AddMinutes(1);
			testRunsPending.TestRuns[0].TestRunSteps[0].ActualDuration = 1;
			testRunsPending.TestRuns[0].TestRunSteps[1].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[0].TestRunSteps[1].StartDate = DateTime.UtcNow.AddMinutes(1);
			testRunsPending.TestRuns[0].TestRunSteps[1].EndDate = DateTime.UtcNow.AddMinutes(4);
			testRunsPending.TestRuns[0].TestRunSteps[1].ActualDuration = 2;
			testRunsPending.TestRuns[0].TestRunSteps[2].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[2].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[0].TestRunSteps[2].StartDate = DateTime.UtcNow.AddMinutes(5);
			testRunsPending.TestRuns[0].TestRunSteps[2].EndDate = DateTime.UtcNow.AddMinutes(7);
			testRunsPending.TestRuns[0].TestRunSteps[2].ActualDuration = 1;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, null, true, false);

			//Verify the saved data so far, overall status should be not run
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			Assert.AreEqual(4, testRunsPending.CountNotRun);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[3].ExecutionStatusId);
			Assert.IsNotNull(testRunsPending.TestRuns[0].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[0].EndDate);
			Assert.AreEqual(4, testRunsPending.TestRuns[0].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[1].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[1].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[1].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[2].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[2].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[2].ActualDuration);

			//Now mark the last steps as Passed and N/A, that makes the whole run PASSED
			testRunsPending.TestRuns[0].TestRunSteps[3].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[3].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[0].TestRunSteps[3].StartDate = DateTime.UtcNow.AddMinutes(7);
			testRunsPending.TestRuns[0].TestRunSteps[3].EndDate = DateTime.UtcNow.AddMinutes(8);
			testRunsPending.TestRuns[0].TestRunSteps[3].ActualDuration = 1;
			testRunsPending.TestRuns[0].TestRunSteps[4].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[4].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
			testRunsPending.TestRuns[0].TestRunSteps[4].StartDate = DateTime.UtcNow.AddMinutes(8);
			testRunsPending.TestRuns[0].TestRunSteps[4].EndDate = DateTime.UtcNow.AddMinutes(10);
			testRunsPending.TestRuns[0].TestRunSteps[4].ActualDuration = 2;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, null, true, false);

			//Verify the saved data so far
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			Assert.AreEqual(3, testRunsPending.CountNotRun);
			Assert.AreEqual(1, testRunsPending.CountPassed);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRunsPending.TestRuns[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[3].ExecutionStatusId);
			Assert.IsNotNull(testRunsPending.TestRuns[0].StartDate);
			Assert.IsNotNull(testRunsPending.TestRuns[0].EndDate);
			Assert.AreEqual(7, testRunsPending.TestRuns[0].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[1].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[1].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[1].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[2].StartDate);
			Assert.IsNull(testRunsPending.TestRuns[2].EndDate);
			Assert.IsNull(testRunsPending.TestRuns[2].ActualDuration);

			//For the second test run we'll pass some steps and then log a failure together with an incident
			testRunsPending.TestRuns[1].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[1].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[1].TestRunSteps[0].StartDate = DateTime.UtcNow;
			testRunsPending.TestRuns[1].TestRunSteps[0].EndDate = DateTime.UtcNow.AddMinutes(1);
			testRunsPending.TestRuns[1].TestRunSteps[0].ActualDuration = 1;
			testRunsPending.TestRuns[1].TestRunSteps[1].StartTracking();
			testRunsPending.TestRuns[1].TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[1].TestRunSteps[1].StartDate = DateTime.UtcNow.AddMinutes(1);
			testRunsPending.TestRuns[1].TestRunSteps[1].EndDate = DateTime.UtcNow.AddMinutes(4);
			testRunsPending.TestRuns[1].TestRunSteps[1].ActualDuration = 2;
			testRunsPending.TestRuns[1].TestRunSteps[2].StartTracking();
			testRunsPending.TestRuns[1].TestRunSteps[2].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			testRunsPending.TestRuns[1].TestRunSteps[2].ActualResult = "Did not work correctly";
			testRunsPending.TestRuns[1].TestRunSteps[2].StartDate = DateTime.UtcNow.AddMinutes(5);
			testRunsPending.TestRuns[1].TestRunSteps[2].EndDate = DateTime.UtcNow.AddMinutes(10);
			testRunsPending.TestRuns[1].TestRunSteps[2].ActualDuration = 4;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 1, null, true, false);
			TestRunStep testRunStep = testRunsPending.TestRuns[1].TestRunSteps[2];

			//Now add the incident
			incidentId1 = incidentManager.Insert(
				projectId,
				null,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				new List<int>() { testRunStep.TestRunStepId },
				"Sample Test Doesn't Work",
				testRunStep.Description + " - " + testRunStep.ExpectedResult + " - " + testRunStep.ActualResult,
				testRunsPending.TestRuns[0].ReleaseId,
				null,
				null,
				null,
				null,
				DateTime.UtcNow,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				USER_ID_FRED_BLOGGS
				);

			//Add the incident custom properties, first verify that we don't have a record already existing
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId1, Artifact.ArtifactTypeEnum.Incident);
			Assert.IsNull(artifactCustomProperty);
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);
			artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId1, customProperties);
			artifactCustomProperty.SetCustomProperty(1, "These are some sample notes");     //Notes
			artifactCustomProperty.SetCustomProperty(2, new List<int>() { customValueId12 });           //Operating System
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//For the third run we'll simply block it
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunsPending.TestRuns[2].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[2].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[2].TestRunSteps[0].StartDate = DateTime.UtcNow;
			testRunsPending.TestRuns[2].TestRunSteps[0].EndDate = DateTime.UtcNow.AddMinutes(1);
			testRunsPending.TestRuns[2].TestRunSteps[0].ActualDuration = 1;
			testRunsPending.TestRuns[2].TestRunSteps[1].StartTracking();
			testRunsPending.TestRuns[2].TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
			testRunsPending.TestRuns[2].TestRunSteps[1].StartDate = DateTime.UtcNow.AddMinutes(1);
			testRunsPending.TestRuns[2].TestRunSteps[1].EndDate = DateTime.UtcNow.AddMinutes(4);
			testRunsPending.TestRuns[2].TestRunSteps[1].ActualDuration = 2;
			testRunsPending.TestRuns[2].TestRunSteps[1].ActualResult = "Unable to get any further because of prior failure";
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 2, null, true, false);

			//Verify the data so far
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			Assert.AreEqual(1, testRunsPending.CountNotRun);
			Assert.AreEqual(1, testRunsPending.CountPassed);
			Assert.AreEqual(1, testRunsPending.CountBlocked);
			Assert.AreEqual(1, testRunsPending.CountFailed);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRunsPending.TestRuns[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testRunsPending.TestRuns[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testRunsPending.TestRuns[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunsPending.TestRuns[3].ExecutionStatusId);
			Assert.IsNotNull(testRunsPending.TestRuns[0].StartDate);
			Assert.IsNotNull(testRunsPending.TestRuns[0].EndDate);
			Assert.AreEqual(7, testRunsPending.TestRuns[0].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[1].StartDate);
			Assert.IsNotNull(testRunsPending.TestRuns[1].EndDate);
			Assert.AreEqual(7, testRunsPending.TestRuns[1].ActualDuration);
			Assert.IsNotNull(testRunsPending.TestRuns[2].StartDate);
			Assert.IsNotNull(testRunsPending.TestRuns[2].EndDate);
			Assert.AreEqual(3, testRunsPending.TestRuns[2].ActualDuration);

			//Finally Commit to the database - removes pending entry and not-run test runs
			testRunId1 = testRunsPending.TestRuns[0].TestRunId;
			testRunId2 = testRunsPending.TestRuns[1].TestRunId;
			testRunId3 = testRunsPending.TestRuns[2].TestRunId;
			testRunId4 = testRunsPending.TestRuns[3].TestRunId;
			testRunManager.CompletePending(pendingId, USER_ID_FRED_BLOGGS);

			//First make sure the final 'not-run' test run was deleted
			bool wasDeleted = false;
			try
			{
				testRunManager.RetrieveById(testRunId4);
			}
			catch (ArtifactNotExistsException)
			{
				wasDeleted = true;
			}
			Assert.IsTrue(wasDeleted);

			//Now we need to retrieve the test runs to make sure they saved correctly
			TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId1);
			//Make sure we have the correct number of rows
			Assert.AreEqual(5, testRun.TestRunSteps.Count);
			//Verify the name and status of the first run and that end-date is not null
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRun.ExecutionStatusId);
			Assert.AreEqual("Ability to create new book", testRun.Name);
			Assert.AreEqual("Description of ability to create new book", testRun.Description);
			Assert.AreEqual(20, testRun.EstimatedDuration);
			Assert.AreEqual(7, testRun.ActualDuration);
			Assert.IsNotNull(testRun.EndDate);
			//Verify the release number
			Assert.AreEqual(releaseId1, testRun.ReleaseId);
			//Veryify that it was saved as a manual run
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Manual, testRun.TestRunTypeId);
			Assert.IsNull(testRun.RunnerName);
			Assert.IsNull(testRun.RunnerAssertCount);
			Assert.IsNull(testRun.RunnerMessage);
			Assert.IsNull(testRun.RunnerStackTrace);

			//Get the lookup names using the test run view
			TestRunView testRunView = testRunManager.RetrieveById(testRunId1);
			Assert.AreEqual("Release 1.0", testRunView.ReleaseName);
			Assert.AreEqual("1.0.0.0", testRunView.ReleaseVersionNumber);
			Assert.AreEqual("Manual", testRunView.TestRunTypeName);

			//Now we need to retrieve the second test runs to make sure it saved correctly
			testRun = testRunManager.RetrieveByIdWithSteps(testRunId2);
			//Make sure we have the correct number of rows
			Assert.AreEqual(5, testRun.TestRunSteps.Count);
			//Verify the name and status of the second run and that end-date is not null
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testRun.ExecutionStatusId);
			Assert.AreEqual("Ability to update existing book", testRun.Name);
			Assert.AreEqual("Description of ability to update existing book", testRun.Description);
			Assert.AreEqual(30, testRun.EstimatedDuration);
			Assert.AreEqual(7, testRun.ActualDuration);
			Assert.IsNotNull(testRun.EndDate);
			//Verify the release number
			Assert.AreEqual(releaseId1, testRun.ReleaseId);

			//Veryify that it was saved as a manual run
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Manual, testRun.TestRunTypeId);
			Assert.IsNull(testRun.RunnerName);
			Assert.IsNull(testRun.RunnerAssertCount);
			Assert.IsNull(testRun.RunnerMessage);
			Assert.IsNull(testRun.RunnerStackTrace);

			//Get the lookup names using the test run view
			testRunView = testRunManager.RetrieveById(testRunId2);
			Assert.AreEqual("Release 1.0", testRunView.ReleaseName);
			Assert.AreEqual("1.0.0.0", testRunView.ReleaseVersionNumber);
			Assert.AreEqual("Manual", testRunView.TestRunTypeName);

			//Verify the third test run
			testRun = testRunManager.RetrieveByIdWithSteps(testRunId3);
			//Make sure we have the correct number of rows
			Assert.AreEqual(4, testRun.TestRunSteps.Count);
			//Verify the name and status of the second run and that end-date is not null
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testRun.ExecutionStatusId);
			Assert.AreEqual("Ability to delete existing book", testRun.Name);
			Assert.AreEqual("Description of ability to delete existing book", testRun.Description);
			Assert.AreEqual(10, testRun.EstimatedDuration);
			Assert.AreEqual(3, testRun.ActualDuration);
			Assert.IsNotNull(testRun.EndDate);
			//Verify the release number
			Assert.AreEqual(releaseId1, testRun.ReleaseId);

			//Veryify that it was saved as a manual run
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Manual, testRun.TestRunTypeId);
			Assert.IsNull(testRun.RunnerName);
			Assert.IsNull(testRun.RunnerAssertCount);
			Assert.IsNull(testRun.RunnerMessage);
			Assert.IsNull(testRun.RunnerStackTrace);

			//Finally we need to verify that we can retrieve the test run without its steps using the retrieve by id option
			testRun = testRunManager.RetrieveById2(testRunId3);
			//Verify the name and status of the second run and that end-date is not null
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testRun.ExecutionStatusId);
			Assert.AreEqual("Ability to delete existing book", testRun.Name);
			Assert.AreEqual("Description of ability to delete existing book", testRun.Description);
			Assert.AreEqual(10, testRun.EstimatedDuration);
			Assert.AreEqual(3, testRun.ActualDuration);
			Assert.IsNotNull(testRun.EndDate);
			//Verify the release number
			Assert.AreEqual(releaseId1, testRun.ReleaseId);

			//Verify we can use the record method the TestRun_Insert method - this creates a full clone of a test run
			testRun = testRunManager.RetrieveByIdWithSteps(testRunId3);
			int testRunId5 = testRunManager.TestRun_Insert(
				projectId,
				USER_ID_FRED_BLOGGS,
				testRun.TestCaseId,
				testRun.ReleaseId,
				testRun.TestSetId,
				testRun.TestSetTestCaseId,
				testRun.StartDate,
				(DateTime)testRun.EndDate,
				testRun.ExecutionStatusId,
				(TestRun.TestRunTypeEnum)testRun.TestRunTypeId,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				testRun.TestRunSteps.ToList(),
				true,
				false);

			//Check that the two test runs match
			TestRun testRun2 = testRunManager.RetrieveByIdWithSteps(testRunId5);
			Assert.AreEqual(testRun.TestCaseId, testRun2.TestCaseId);
			Assert.AreEqual(testRun.ReleaseId, testRun2.ReleaseId);
			Assert.AreEqual(testRun.ExecutionStatusId, testRun2.ExecutionStatusId);
			Assert.AreEqual(testRun.TestRunTypeId, testRun2.TestRunTypeId);
			Assert.AreEqual(testRun.TestRunSteps.Count, testRun2.TestRunSteps.Count);
			Assert.AreEqual(testRun.TestRunSteps[0].TestCaseId, testRun2.TestRunSteps[0].TestCaseId);
			Assert.AreEqual(testRun.TestRunSteps[0].TestStepId, testRun2.TestRunSteps[0].TestStepId);
			Assert.AreEqual(testRun.TestRunSteps[0].ExecutionStatusId, testRun2.TestRunSteps[0].ExecutionStatusId);
			Assert.AreEqual(testRun.TestRunSteps[2].TestCaseId, testRun2.TestRunSteps[2].TestCaseId);
			Assert.AreEqual(testRun.TestRunSteps[2].TestStepId, testRun2.TestRunSteps[2].TestStepId);
			Assert.AreEqual(testRun.TestRunSteps[2].ExecutionStatusId, testRun2.TestRunSteps[2].ExecutionStatusId);
		}

		[
		Test,
		SpiraTestCase(240)
		]
		public void _03_VerifyTestCaseUpdates()
		{
			//Now we need to verify that the execution status and execution date of the underlying test cases
			//has been modified, since they had NULL existing execution dates. Also the owner's name should have
			//been automatically unset if the test case passed

			//First Test Case
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.ExecutionStatusId);
			//Assert.IsNull(testCase.OwnerId);
			Assert.IsNotNull(testCase.ExecutionDate);
			Assert.AreEqual(20, testCase.EstimatedDuration);
			Assert.AreEqual(7, testCase.ActualDuration);
			//Test Steps (links are always listed as N/A)
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, testCase.TestSteps[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.TestSteps[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.TestSteps[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, testCase.TestSteps[3].ExecutionStatusId);

			//Second Test Case
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId2);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCase.ExecutionStatusId);
			//Assert.IsNotNull(testCase.OwnerId);
			Assert.IsNotNull(testCase.ExecutionDate);
			Assert.AreEqual(30, testCase.EstimatedDuration);
			Assert.AreEqual(7, testCase.ActualDuration);

			//Test Steps (links are always listed as N/A)
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, testCase.TestSteps[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCase.TestSteps[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.TestSteps[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.TestSteps[3].ExecutionStatusId);

			//Third Test Case
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId3);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testCase.ExecutionStatusId);
			//Assert.IsNotNull(testCase.OwnerId);
			Assert.IsNotNull(testCase.ExecutionDate);
			Assert.AreEqual(10, testCase.EstimatedDuration);
			Assert.AreEqual(3, testCase.ActualDuration);

			//Fourth Test Case (not run)
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId4);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);
			Assert.IsNotNull(testCase.OwnerId);
			Assert.IsNull(testCase.ExecutionDate);
			Assert.AreEqual(40, testCase.EstimatedDuration);
			Assert.IsNull(testCase.ActualDuration);

			//Verify that we can't update over a newer test run
			//Lets create a new test run from one of the test cases
			//This time we'll not create a pending entry, but instead save it at the end in one go
			List<int> testCaseIdList = new List<int>();
			testCaseIdList.Add(testCaseId1);
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, releaseId1, testCaseIdList, false);

			//Lets create a new run with an older end date and save it
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[0].TestRunSteps[0].ActualResult = "Works";
			testRunsPending.TestRuns[0].TestRunSteps[1].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunsPending.TestRuns[0].TestRunSteps[1].ActualResult = "Works";
			testRunsPending.TestRuns[0].TestRunSteps[2].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[2].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			testRunsPending.TestRuns[0].TestRunSteps[2].ActualResult = "Doesn't Work";
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, DateTime.Parse("1/1/2000"), false);
			testRunManager.Save(testRunsPending, projectId, false);

			//Verify that the test-case execution status remains failed and the exectution date is not 1/1/2000
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.ExecutionStatusId);
			Assert.AreNotEqual(testCase.ExecutionDate, DateTime.Parse("1/1/2000"));
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, testCase.TestSteps[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.TestSteps[1].ExecutionStatusId);

			//Now get a list of the two test runs for this test case
			Hashtable filters = new Hashtable();
			filters.Add("TestCaseId", testCaseId1);
			List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "ExecutionDate", false, 1, 20, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, testRuns.Count, "TestRun.Count");
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRuns[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testRuns[1].ExecutionStatusId);

			//Verify that the count (without pagination) works correctly
			int testRunCount = testRunManager.Count(projectId, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, testRunCount, "TestRunCount");

			//Next, verify that the common test cases were not updated, this was changed in v5.0
			//The individual steps will show as updated
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId6);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);
			Assert.IsNull(testCase.ExecutionDate);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.TestSteps[0].ExecutionStatusId);
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId7);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);
			Assert.IsNull(testCase.ExecutionDate);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, testCase.TestSteps[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testCase.TestSteps[1].ExecutionStatusId);

			//Verify that changes are populated to the test case parent folder
			//Also the the execution date should no longer be NULL, since it rolls up
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual(1, testCaseFolder.CountPassed, "FolderCountPassed15");
			Assert.AreEqual(1, testCaseFolder.CountFailed, "FolderCountFailed15");
			Assert.AreEqual(0, testCaseFolder.CountCaution, "FolderCountCaution15");
			Assert.AreEqual(1, testCaseFolder.CountBlocked, "FolderCountBlocked15");
			Assert.AreEqual(4, testCaseFolder.CountNotRun, "FolderCountNotRun15");
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable, "FolderCountNotApplicable15");
			Assert.IsNotNull(testCaseFolder.ExecutionDate);
			Assert.AreEqual(165, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(17, testCaseFolder.ActualDuration);

			//Also check the folder containing the common steps
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId5);
			Assert.AreEqual(0, testCaseFolder.CountPassed, "FolderCountPassed15");
			Assert.AreEqual(0, testCaseFolder.CountFailed, "FolderCountFailed15");
			Assert.AreEqual(0, testCaseFolder.CountCaution, "FolderCountCaution15");
			Assert.AreEqual(0, testCaseFolder.CountBlocked, "FolderCountBlocked15");
			Assert.AreEqual(2, testCaseFolder.CountNotRun, "FolderCountNotRun15");
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable, "FolderCountNotApplicable15");
			Assert.IsNull(testCaseFolder.ExecutionDate);
			Assert.AreEqual(25, testCaseFolder.EstimatedDuration);
			Assert.IsNull(testCaseFolder.ActualDuration);

			//Now we need to verify the execution status when retrieved by the release as well
			List<TestCaseReleaseView> testCasesByRelease = testCaseManager.RetrieveByReleaseId(projectId, releaseId1, "Name", true, 1, Int32.MaxValue, null, 0, testFolderId1);
			Assert.AreEqual(3, testCasesByRelease.Count);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCasesByRelease[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testCasesByRelease[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCasesByRelease[2].ExecutionStatusId);

			//Verify they are still not run for the other release
			testCasesByRelease = testCaseManager.RetrieveByReleaseId(projectId, releaseId2, "Name", true, 1, Int32.MaxValue, null, 0, testFolderId1);
			Assert.AreEqual(3, testCasesByRelease.Count);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCasesByRelease[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCasesByRelease[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCasesByRelease[2].ExecutionStatusId);

			//Now check that the release-folders are also updated for the release
			TestCaseFolderReleaseView testCaseFolderForRelease = testCaseManager.TestCaseFolder_GetByParentIdForRelease(projectId, null, releaseId1).First();
			Assert.AreEqual(1, testCaseFolderForRelease.CountPassed, "FolderCountPassed15");
			Assert.AreEqual(1, testCaseFolderForRelease.CountFailed, "FolderCountFailed15");
			Assert.AreEqual(0, testCaseFolderForRelease.CountCaution, "FolderCountCaution15");
			Assert.AreEqual(1, testCaseFolderForRelease.CountBlocked, "FolderCountBlocked15");
			Assert.AreEqual(1, testCaseFolderForRelease.CountNotRun, "FolderCountNotRun15");
			Assert.AreEqual(0, testCaseFolderForRelease.CountNotApplicable, "FolderCountNotApplicable15");
			Assert.IsNotNull(testCaseFolderForRelease.ExecutionDate);
			Assert.AreEqual(165, testCaseFolderForRelease.EstimatedDuration);
			Assert.AreEqual(17, testCaseFolderForRelease.ActualDuration);

			//Also check that the release-folders are also NOT updated for the other release
			testCaseFolderForRelease = testCaseManager.TestCaseFolder_GetByParentIdForRelease(projectId, null, releaseId2).First();
			Assert.AreEqual(0, testCaseFolderForRelease.CountPassed, "FolderCountPassed15");
			Assert.AreEqual(0, testCaseFolderForRelease.CountFailed, "FolderCountFailed15");
			Assert.AreEqual(0, testCaseFolderForRelease.CountCaution, "FolderCountCaution15");
			Assert.AreEqual(0, testCaseFolderForRelease.CountBlocked, "FolderCountBlocked15");
			Assert.AreEqual(4, testCaseFolderForRelease.CountNotRun, "FolderCountNotRun15");
			Assert.AreEqual(0, testCaseFolderForRelease.CountNotApplicable, "FolderCountNotApplicable15");
			Assert.IsNull(testCaseFolderForRelease.ExecutionDate);
			Assert.AreEqual(165, testCaseFolderForRelease.EstimatedDuration);
			Assert.IsNull(testCaseFolderForRelease.ActualDuration);

			//Complete the run
			testRunManager.CompletePending(testRunsPending.TestRunsPendingId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(241)
		]
		public void _04_VerifyIncidentCreation()
		{
			//Retrieve the incidents associated with the test run step
			TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId2);
			int testRunStepId = testRun.TestRunSteps[2].TestRunStepId;
			List<IncidentView> incidents = incidentManager.RetrieveByTestRunStepId(testRunStepId);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual("Sample Test Doesn't Work", incidents[0].Name);
			Assert.AreEqual("Choose option to edit book - Book update screen displayed - Did not work correctly", incidents[0].Description);
			Assert.AreEqual(incidentManager.GetDefaultIncidentType(projectTemplateId), incidents[0].IncidentTypeId);
			Assert.IsNull(incidents[0].PriorityId);
			Assert.IsNull(incidents[0].SeverityId);

			//Need to verify that we have linked test run steps to this incident
			List<TestRunStepIncidentView> testRunStepIncidents = incidentManager.Incident_RetrieveTestRunSteps(incidents[0].IncidentId);
			Assert.AreEqual(1, testRunStepIncidents.Count);
			Assert.AreEqual(incidentId1, testRunStepIncidents[0].IncidentId);
			Assert.AreEqual(testRunStepId, testRunStepIncidents[0].TestRunStepId);

			//Now verify the custom properties for this incident
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidents[0].IncidentId, Artifact.ArtifactTypeEnum.Incident, true);
			Assert.IsNotNull(artifactCustomProperty);
			Assert.AreEqual("These are some sample notes", (string)artifactCustomProperty.CustomProperty(1));
			int customProperyValueId = (int)artifactCustomProperty.CustomProperty(2);
			Assert.AreEqual(customProperyValueId, customValueId12);

			//Now test that we can link another test run step to this same incident (added in v5.0)
			List<int> incidentIds = new List<int>() { incidentId1 };
			incidentManager.Incident_AssociateToTestRunStep(projectId, testRun.TestRunSteps[3].TestRunStepId, incidentIds, USER_ID_FRED_BLOGGS);

			//Verify the link was added
			incidents = incidentManager.RetrieveByTestRunStepId(testRun.TestRunSteps[3].TestRunStepId);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual("Sample Test Doesn't Work", incidents[0].Name);
			Assert.AreEqual("Choose option to edit book - Book update screen displayed - Did not work correctly", incidents[0].Description);
			Assert.AreEqual(incidentManager.GetDefaultIncidentType(projectTemplateId), incidents[0].IncidentTypeId);
			Assert.IsNull(incidents[0].PriorityId);
			Assert.IsNull(incidents[0].SeverityId);

			//Need to verify that we have both linked test run steps to this incident
			testRunStepIncidents = incidentManager.Incident_RetrieveTestRunSteps(incidents[0].IncidentId);
			Assert.AreEqual(2, testRunStepIncidents.Count);
		}

		/// <summary>
		/// Verify that the mapped requirement and release are updated
		/// </summary>
		[
		Test,
		SpiraTestCase(1398)
		]
		public void _05_VerifyCoverageUpdates()
		{
			//First verify the two releases
			ReleaseManager releaseManager = new ReleaseManager();
			ReleaseView release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual(1, release.CountBlocked);
			Assert.AreEqual(0, release.CountCaution);
			Assert.AreEqual(1, release.CountFailed);
			Assert.AreEqual(0, release.CountNotApplicable);
			Assert.AreEqual(1, release.CountNotRun);
			Assert.AreEqual(1, release.CountPassed);

			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId2);
			Assert.AreEqual(0, release.CountBlocked);
			Assert.AreEqual(0, release.CountCaution);
			Assert.AreEqual(0, release.CountFailed);
			Assert.AreEqual(0, release.CountNotApplicable);
			Assert.AreEqual(4, release.CountNotRun);
			Assert.AreEqual(0, release.CountPassed);

			//Next verify the mapped requirements
			RequirementManager requirementManager = new RequirementManager();
			RequirementView requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(2, requirement.CoverageCountTotal);
			Assert.AreEqual(0, requirement.CoverageCountBlocked);
			Assert.AreEqual(0, requirement.CoverageCountCaution);
			Assert.AreEqual(1, requirement.CoverageCountFailed);
			Assert.AreEqual(1, requirement.CoverageCountPassed);

			requirement = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId2);
			Assert.AreEqual(2, requirement.CoverageCountTotal);
			Assert.AreEqual(1, requirement.CoverageCountBlocked);
			Assert.AreEqual(0, requirement.CoverageCountCaution);
			Assert.AreEqual(0, requirement.CoverageCountFailed);
			Assert.AreEqual(0, requirement.CoverageCountPassed);
		}

		[
		Test,
		SpiraTestCase(242)
		]
		public void _06_RecordAutomatedTestRun()
		{
			//Create a test set
			TestSetManager testSetManager = new TestSetManager();
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, releaseId2, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Automated, null, null);

			//Obtain the highest History entry to verify that it was set properly.
			long? latestHistory = GetHistoryId();

			//This tests that we can create a successful automated test run with no release information, but passing test set instead (but not the id of the unique test case in the set)
			//Also verify that we can pass test step level information
			List<TestRunStepInfo> testRunSteps = new List<TestRunStepInfo>();
			//Step #1 - Failed - linked to a real test step
			TestRunStepInfo testRunStepInfo = new TestRunStepInfo();
			testRunStepInfo.Position = 1;
			testRunStepInfo.Description = "Verifying that winMain displays title";
			testRunStepInfo.ExpectedResult = "Title displayed";
			testRunStepInfo.SampleData = "test 123";
			testRunStepInfo.ActualResult = "Object Not Available on Screen";
			testRunStepInfo.ExecutionStatusId = 1;   //Failed
			testRunStepInfo.TestStepId = testStepId1;
			testRunSteps.Add(testRunStepInfo);
			//Step #2 -  Caution - not linked to a test step
			testRunStepInfo = new TestRunStepInfo();
			testRunStepInfo.Position = 2;
			testRunStepInfo.Description = "txtName = 'The Scowrers'";
			testRunStepInfo.ExpectedResult = "The Scowrers";
			testRunStepInfo.ActualResult = "the scowrers";
			testRunStepInfo.ExecutionStatusId = 6;   //Caution
			testRunSteps.Add(testRunStepInfo);
			//Step #3 - Passed - not linked to a test step
			testRunStepInfo = new TestRunStepInfo();
			testRunStepInfo.Position = 3;
			testRunStepInfo.Description = "cboAuthor Matches Author";
			testRunStepInfo.ExpectedResult = "AuthorId=5";
			testRunStepInfo.ExecutionStatusId = 2;   //Passed
			testRunSteps.Add(testRunStepInfo);
			testRunId3 = testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId2, null, testSetId, null, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(1).AddMinutes(2), (int)TestCase.ExecutionStatusEnum.Passed, "TestSuite", "01_TestCaseName", null, null, null, null, null, null, TestRun.TestRunFormatEnum.PlainText, testRunSteps);

			//Now retrieve the test run to check that it saved correctly
			TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId3);
			TestRunView testRunView = testRunManager.RetrieveById(testRunId3);

			//Verify Counts
			Assert.IsNotNull(testRun);
			Assert.AreEqual(3, testRun.TestRunSteps.Count);

			//Now verify the data, including that the release id is picked up from the test set
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRun.ExecutionStatusId);
			Assert.AreEqual("Passed", testRunView.ExecutionStatusName);
			Assert.AreEqual("2.0.0.0", testRunView.ReleaseVersionNumber);
			Assert.AreEqual(TestRun.TestRunTypeEnum.Automated.GetHashCode(), testRun.TestRunTypeId);
			Assert.AreEqual("Automated", testRunView.TestRunTypeName);
			Assert.AreEqual("TestSuite", testRun.RunnerName);
			Assert.AreEqual("01_TestCaseName", testRun.RunnerTestName);
			Assert.IsTrue(testRun.ActualDuration > 0);
			Assert.IsTrue(testRun.RunnerAssertCount.IsNull());
			Assert.IsTrue(testRun.RunnerMessage.IsNull());
			Assert.IsTrue(testRun.RunnerStackTrace.IsNull());
			Assert.AreEqual((int)TestRun.TestRunFormatEnum.PlainText, testRun.TestRunFormatId);
			//Assert.AreEqual(latestHistory, testRun.ChangeSetId, "History ChangeSet ID did not match what was expected.");

			//Verify the test run steps
			//Step 1
			Assert.AreEqual(1, testRun.TestRunSteps[0].Position);
			Assert.AreEqual("Verifying that winMain displays title", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("Title displayed", testRun.TestRunSteps[0].ExpectedResult);
			Assert.AreEqual("test 123", testRun.TestRunSteps[0].SampleData);
			Assert.AreEqual(1, testRun.TestRunSteps[0].ExecutionStatusId);
			Assert.AreEqual("Object Not Available on Screen", testRun.TestRunSteps[0].ActualResult);
			Assert.AreEqual(testStepId1, testRun.TestRunSteps[0].TestStepId);
			//Step 2
			Assert.AreEqual(2, testRun.TestRunSteps[1].Position);
			Assert.AreEqual("txtName = 'The Scowrers'", testRun.TestRunSteps[1].Description);
			Assert.AreEqual("The Scowrers", testRun.TestRunSteps[1].ExpectedResult);
			Assert.IsTrue(testRun.TestRunSteps[1].SampleData.IsNull());
			Assert.AreEqual(6, testRun.TestRunSteps[1].ExecutionStatusId);
			Assert.AreEqual("the scowrers", testRun.TestRunSteps[1].ActualResult);
			Assert.IsTrue(testRun.TestRunSteps[1].TestStepId.IsNull());
			//Step 3
			Assert.AreEqual(3, testRun.TestRunSteps[2].Position);
			Assert.AreEqual("cboAuthor Matches Author", testRun.TestRunSteps[2].Description);
			Assert.AreEqual("AuthorId=5", testRun.TestRunSteps[2].ExpectedResult);
			Assert.IsTrue(testRun.TestRunSteps[2].SampleData.IsNull());
			Assert.AreEqual(2, testRun.TestRunSteps[2].ExecutionStatusId);
			Assert.IsTrue(testRun.TestRunSteps[2].ActualResult.IsNull());
			Assert.IsTrue(testRun.TestRunSteps[2].TestStepId.IsNull());

			//Now verify that the test case and test steps were updated
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId2);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.ExecutionStatusId);
			Assert.AreEqual("Passed", testCase.ExecutionStatusName);
			Assert.AreEqual(testStepId1, testCase.TestSteps[1].TestStepId);
			Assert.AreEqual("Failed", testCase.TestSteps[1].ExecutionStatusName);

			//This tests that we can create a failed automated test run with release information
			testRunId4 = testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId1, 1, null, null, DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(2).AddMinutes(2), (int)TestCase.ExecutionStatusEnum.Failed, "TestSuite", "01_TestCaseName", 5, "Expected 1, Found 0", "Error Stack Trace........", null, null, null, TestRun.TestRunFormatEnum.PlainText, null);

			//Now retrieve the test run to check that it saved correctly
			testRun = testRunManager.RetrieveByIdWithSteps(testRunId4);
			testRunView = testRunManager.RetrieveById(testRunId4);

			//Verify Counts (no steps for automated runs)
			Assert.IsNotNull(testRun);
			Assert.AreEqual(0, testRun.TestRunSteps.Count);

			//Now verify the data
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testRun.ExecutionStatusId);
			Assert.AreEqual("Failed", testRunView.ExecutionStatusName);
			Assert.AreEqual(1, testRun.ReleaseId);
			Assert.AreEqual(TestRun.TestRunTypeEnum.Automated.GetHashCode(), testRun.TestRunTypeId);
			Assert.AreEqual("Automated", testRunView.TestRunTypeName);
			Assert.AreEqual("TestSuite", testRun.RunnerName);
			Assert.AreEqual("01_TestCaseName", testRun.RunnerTestName);
			Assert.IsTrue(testRun.ActualDuration > 0);
			Assert.AreEqual(5, testRun.RunnerAssertCount);
			Assert.AreEqual("Expected 1, Found 0", testRun.RunnerMessage);
			Assert.AreEqual("Error Stack Trace........", testRun.RunnerStackTrace);
			Assert.AreEqual((int)TestRun.TestRunFormatEnum.PlainText, testRun.TestRunFormatId);
			//Assert.AreEqual(latestHistory, testRun.ChangeSetId, "History ChangeSet ID did not match what was expected.");

			//Now verify that the test case itself updated
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCaseView.ExecutionStatusId);
			Assert.AreEqual("Failed", testCaseView.ExecutionStatusName);

			//Now verify that we can pass through a specific automation host id and engine id
			//This is used for the new automated launching test execution capability

			//Create a new host, engine and test case
			AutomationManager automationManager = new AutomationManager();
			int automationHostId = automationManager.InsertHost(projectId, "Windows Server 1", "Win1", "", true, USER_ID_FRED_BLOGGS);
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, tc_typeUnitId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, null, null, null);
			testCaseManager.AddUpdateAutomationScript(USER_ID_FRED_BLOGGS, projectId, testCaseId, automationEngineId, "file://c:/tests/mytest.qtp", "", null, "1.0", null, null);

			//Obtain the highest History entry to verify that it was set properly.
			latestHistory = GetHistoryId();

			//Now execute the test case
			int testRunId = testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(2), (int)TestCase.ExecutionStatusEnum.Failed, "Engine1", "01_TestCaseName", 5, "Expected 1, Found 0", "Error Stack Trace........", automationHostId, automationEngineId, null, TestRun.TestRunFormatEnum.HTML, null);

			//Verify that the automation host and engine were set
			testRunView = testRunManager.RetrieveById(testRunId);
			Assert.AreEqual(automationEngineId, testRunView.AutomationEngineId);
			Assert.AreEqual("Engine1", testRunView.RunnerName);
			Assert.AreEqual(automationHostId, testRunView.AutomationHostId);
			Assert.AreEqual("Windows Server 1", testRunView.AutomationHostName);
			Assert.AreEqual((int)TestRun.TestRunFormatEnum.HTML, testRunView.TestRunFormatId);
			//Assert.AreEqual(latestHistory, testRunView.ChangeSetId, "History ChangeSet ID did not match what was expected.");


			//Test that retrieving a list of test runs also includes that information
			Hashtable filters = new Hashtable();
			filters.Add("TestRunId", testRunId);
			List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "TestRunId", true, 1, 10, filters, InternalRoutines.UTC_OFFSET);
			testRunView = testRuns[0];
			Assert.AreEqual(automationEngineId, testRunView.AutomationEngineId);
			Assert.AreEqual("Engine1", testRunView.RunnerName);
			Assert.AreEqual(automationHostId, testRunView.AutomationHostId);
			Assert.AreEqual("Windows Server 1", testRunView.AutomationHostName);
		}

		[
		Test,
		SpiraTestCase(244)
		]
		public void _07_DeleteTestCasesWithRuns()
		{
			//Verify that deleting test steps delinks the test run step

			//First verify initial state of data
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId2);
			Assert.AreEqual(4, testCase.TestSteps.Count);
			TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId2);
			Assert.AreEqual(testCase.TestSteps[1].TestStepId, testRun.TestRunSteps[2].TestStepId, "TestStepId");

			//Actually perform the delete
			testCaseManager.MarkStepAsDeleted(USER_ID_FRED_BLOGGS, testCase.TestSteps[1].TestCaseId, testCase.TestSteps[1].TestStepId);

			//Verify delete left data in expected state
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId2);
			Assert.AreEqual(3, testCase.TestSteps.Count);
			testRun = testRunManager.RetrieveByIdWithSteps(testRunId2);
			Assert.IsTrue(testRun.TestRunSteps[2].TestStep.IsDeleted);  //Client needs to check and display correctly

			//Verify that deleting the test case deletes the associated test runs (and changes folder execution status)
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId2);
			bool artifactExists = true;
			try
			{
				TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId2);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "TestCase not deleted");

			//Verify that changes are populated to the test case parent folders in turn
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual(0, testCaseFolder.CountPassed, "FolderCountPassed15");
			Assert.AreEqual(1, testCaseFolder.CountFailed, "FolderCountFailed15");
			Assert.AreEqual(0, testCaseFolder.CountCaution, "FolderCountCaution15");
			Assert.AreEqual(1, testCaseFolder.CountBlocked, "FolderCountBlocked15");
			Assert.AreEqual(4, testCaseFolder.CountNotRun, "FolderCountNotRun15");
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable, "FolderCountNotApplicable15");
			Assert.IsNotNull(testCaseFolder.ExecutionDate);
			Assert.AreEqual(135, testCaseFolder.EstimatedDuration);
			Assert.AreEqual(5, testCaseFolder.ActualDuration);

			//Verify that deleting the test case cascades to all associated test case runs
			Hashtable filters = new Hashtable();
			filters.Add("TestCaseId", testCaseId2);
			List<TestRunView> testRuns = testRunManager.Retrieve(projectId, null, true, 1, Int32.MaxValue, filters, 0);
			Assert.AreEqual(0, testRuns.Count);
		}

		[
		Test,
		SpiraTestCase(245)
		]
		public void _08_VerifyIncidentUnAssociated()
		{
			//Verify that incident no longer has test run step associated
			Incident incident = incidentManager.RetrieveById(incidentId1, false);
			Assert.IsNotNull(incident);
			Assert.AreEqual("Sample Test Doesn't Work", incident.Name);
			Assert.AreEqual("Choose option to edit book - Book update screen displayed - Did not work correctly", incident.Description);
			Assert.AreEqual(0, incident.TestRunSteps.Count);
		}

		[
		Test,
		SpiraTestCase(329)
		]
		public void _09_PauseTestRun()
		{
			//Verify that we can initiate a test-run, have it saved as pending and then complete it later
			//Need to also test that the pending not-run test runs don't show up in the test run lists

			//First get the test run counts for the test cases we're going to add to make sure we don't see the
			//pending runs until completed
			Hashtable filters = new Hashtable();
			filters.Add("TestCaseId", testCaseId1);
			int testRunCount1 = testRunManager.Retrieve(projectId, "ExecutionDate", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET).Count;
			filters.Clear();
			filters.Add("ReleaseId", releaseId1);
			int testRunCount2 = testRunManager.Retrieve(projectId, "ExecutionDate", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET).Count;
			int testRunCount3 = testRunManager.RetrieveDailyCount(projectId, -5, null).Count;

			//First create a new test run
			List<int> testCaseIdList = new List<int>();
			testCaseIdList.Add(testCaseId1);
			testCaseIdList.Add(testCaseId3);
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, releaseId1, testCaseIdList, true);
			int pendingId = testRunsPending.TestRunsPendingId;

			//First make sure we have the expected number of test cases and test steps
			Assert.AreEqual(2, testRunsPending.CountNotRun);
			Assert.AreEqual(2, testRunsPending.TestRuns.Count);
			Assert.AreEqual(5, testRunsPending.TestRuns[0].TestRunSteps.Count);
			Assert.AreEqual(4, testRunsPending.TestRuns[1].TestRunSteps.Count);

			//Now get the list of pending runs for this user - cross project
			List<TestRunsPendingView> testRunsPendings = testRunManager.RetrievePendingByUserId(USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual(1, testRunsPendings.Count);
			Assert.AreEqual(2, testRunsPendings[0].CountNotRun);
			Assert.AreEqual("TestRunTest Project", testRunsPendings[0].ProjectName);

			//Now get the list of pending runs for this user - specific project
			testRunsPendings = testRunManager.RetrievePendingByUserId(USER_ID_FRED_BLOGGS, projectId);
			Assert.AreEqual(1, testRunsPendings.Count);
			Assert.AreEqual(2, testRunsPendings[0].CountNotRun);
			Assert.AreEqual("TestRunTest Project", testRunsPendings[0].ProjectName);

			//Now restore this test run and verify the data
			pendingId = testRunsPendings[0].TestRunsPendingId;
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			Assert.AreEqual(2, testRunsPending.CountNotRun);
			Assert.AreEqual(2, testRunsPending.TestRuns.Count);
			Assert.AreEqual(5, testRunsPending.TestRuns[0].TestRunSteps.Count);
			Assert.AreEqual(4, testRunsPending.TestRuns[1].TestRunSteps.Count);

			//Also verify that these pending runs haven't modified the test run counts anywhere
			filters.Clear();
			filters.Add("TestCaseId", testCaseId1);
			Assert.AreEqual(testRunCount1, testRunManager.Retrieve(projectId, "ExecutionDate", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET).Count, "TestRunCount1");
			filters.Clear();
			filters.Add("ReleaseId", releaseId1);
			Assert.AreEqual(testRunCount2, testRunManager.Retrieve(projectId, "ExecutionDate", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET).Count, "TestRunCount2");
			Assert.AreEqual(testRunCount3, testRunManager.RetrieveDailyCount(projectId, -5, null).Count, "TestRunCount3");

			//Store one of the test run ids for later
			testRunId1 = testRunsPending.TestRuns[0].TestRunId;

			//Need to verify that the pending counts changes when a test is passed/failed/blocked
			TestRunStep testRunStep = testRunsPending.TestRuns[0].TestRunSteps[0];
			testRunStep.StartTracking();
			testRunStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
			testRunStep.ActualResult = "Doesn't Work";
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, DateTime.UtcNow.AddHours(3), true, false);
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			Assert.AreEqual(1, testRunsPending.CountNotRun);
			Assert.AreEqual(1, testRunsPending.CountBlocked);

			//Also verify that the execution status of the test case itself changed
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testCaseView.ExecutionStatusId);

			//Finally complete the saved test run and verify that it deleted successfully
			//and that the one completed test run remains
			testRunManager.CompletePending(pendingId, USER_ID_FRED_BLOGGS);
			testRunsPendings = testRunManager.RetrievePendingByUserId(USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual(0, testRunsPendings.Count);
			filters.Clear();
			filters.Add("ReleaseId", releaseId1);
			Assert.AreEqual(testRunCount2 + 1, testRunManager.Retrieve(projectId, "ExecutionDate", false, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET).Count);


			//Create 2 test runs from a single test case id
			List<int> testCaseIdSingle = new List<int>();
			testCaseIdList.Clear();
			testCaseIdList.Add(testCaseId1);
			testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, releaseId1, testCaseIdList, true);
			pendingId = testRunsPending.TestRunsPendingId;
			testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, releaseId1, testCaseIdList, true);
			int pendingId2 = testRunsPending.TestRunsPendingId;

			//Check that we can get back a test runs pending for a user and against a specific test case
			testRunsPendings = testRunManager.RetrievePendingByUserIdAndTestCase(USER_ID_FRED_BLOGGS, testCaseId1);
			Assert.AreEqual(2, testRunsPendings.Count, "Expected count of 2 testrunspending for USER_ID_FRED_BLOGGS and testCaseId1");
			Assert.AreEqual(1, testRunsPendings[0].CountNotRun, "Expected count of 1 not run test run in the first testrunspending for USER_ID_FRED_BLOGGS and testCaseId1");
			Assert.AreEqual("TestRunTest Project", testRunsPendings[0].ProjectName, "Check that the first testrunspending is in the correct project");

			//Cleanup - mark this new test runs pending as complete
			testRunManager.CompletePending(pendingId, USER_ID_FRED_BLOGGS);
			testRunManager.CompletePending(pendingId2, USER_ID_FRED_BLOGGS);
			testRunsPendings = testRunManager.RetrievePendingByUserId(USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual(0, testRunsPendings.Count, "There should be zero test runs pending for USER_ID_FRED_BLOGGS");
			testRunsPendings = testRunManager.RetrievePendingByUserIdAndTestCase(USER_ID_FRED_BLOGGS, testCaseId1);
			Assert.AreEqual(0, testRunsPendings.Count, "There should be zero test runs pending for USER_ID_FRED_BLOGGS and testCaseId1");
		}

		/// <summary>
		/// Test that we can edit the details of an existing test run
		/// </summary>
		[
		Test,
		SpiraTestCase(402)
		]
		public void _10_EditTestRun()
		{
			//Retrieve the test run, make some changes and then save the update
			TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId1);
			testRun.StartTracking();
			testRun.TesterId = USER_ID_JOE_SMITH;
			testRun.EstimatedDuration = 100;
			testRun.ActualDuration = 120;
			testRun.ReleaseId = releaseId2;
			testRun.StartDate = DateTime.UtcNow.AddHours(5);
			testRun.EndDate = DateTime.UtcNow.AddHours(5).AddMinutes(10);
			testRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;
			testRunManager.Update(projectId, testRun, USER_ID_FRED_BLOGGS);

			//Verify the changes
			testRun = testRunManager.RetrieveByIdWithSteps(testRunId1);
			Assert.AreEqual(testCaseId1, testRun.TestCaseId);
			Assert.AreEqual(USER_ID_JOE_SMITH, testRun.TesterId, "TesterId");
			Assert.AreEqual(100, testRun.EstimatedDuration, "EstimatedDuration");
			Assert.AreEqual(120, testRun.ActualDuration, "ActualDuration");
			Assert.AreEqual(releaseId2, testRun.ReleaseId, "ReleaseId");
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testRun.ExecutionStatusId);

			//Verify the status of the test case changed
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCaseView.ExecutionStatusId);

			//Verify the release summary updated
			ReleaseManager releaseManager = new ReleaseManager();
			ReleaseView releaseView = releaseManager.RetrieveById(User.UserInternal, projectId, releaseId1);
			Assert.AreEqual(0, releaseView.CountCaution);
			releaseView = releaseManager.RetrieveById(User.UserInternal, projectId, releaseId2);
			Assert.AreEqual(1, releaseView.CountCaution);

			//Put the release back
			testRun.StartTracking();
			testRun.ReleaseId = releaseId1;
			testRunManager.Update(projectId, testRun, USER_ID_FRED_BLOGGS);

			//Verify the release summary updated
			releaseView = releaseManager.RetrieveById(User.UserInternal, projectId, releaseId1);
			Assert.AreEqual(1, releaseView.CountCaution);
			releaseView = releaseManager.RetrieveById(User.UserInternal, projectId, releaseId2);
			Assert.AreEqual(0, releaseView.CountCaution);
		}

		/// <summary>
		/// Test that we can delete an existing test run, in which case the status of the test case
		/// reverts to the most recent run
		/// </summary>
		[
		Test,
		SpiraTestCase(403)
		]
		public void _11_DeleteTestRun()
		{
			//Delete the test run and verify that it deleted correctly
			testRunManager.Delete(testRunId1, USER_ID_FRED_BLOGGS, projectId);
			bool artifactExists = true;
			try
			{
				TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Test Run Not Deleted Correctly");

			//Finally verify that the execution status of the test case itself changed back
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCaseView.ExecutionStatusId);
		}

		[
		Test,
		SpiraTestCase(351)
		]
		public void _12_CreateFromTestSet()
		{
			//We need to verify that we can create a test run from a single test set that
			//is assigned to a release and also contains test cases.
			//First make a copy of the test set that we can use
			TestSetManager testSetManager = new TestSetManager();
			int testSetFolderId = testSetManager.TestSetFolder_Create("Test Set Folder 1", projectId, null, null).TestSetFolderId;
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, testSetFolderId, releaseId1, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, DateTime.UtcNow, TestRun.TestRunTypeEnum.Manual, null, null);

			//Also set the Operating System custom property of the test set to a value
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectId, Artifact.ArtifactTypeEnum.TestSet, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestSet, testSetId, customProperties);
			artifactCustomProperty.SetCustomProperty(1, customValueId11);   //Windows 7
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Add some test cases to the test set
			testSetManager.AddTestCase(projectId, testSetId, testCaseId1, null, null);
			testSetManager.AddTestCase(projectId, testSetId, testCaseId3, null, null);
			testSetManager.AddTestCase(projectId, testSetId, testCaseId4, null, null);

			//Verify the execution data of the test set
			TestSetView testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual(3, testSetView.CountNotRun);
			Assert.AreEqual(70, testSetView.EstimatedDuration);
			Assert.IsNull(testSetView.ExecutionDate);
			Assert.IsNull(testSetView.ActualDuration);

			//Verify the folder
			TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderId);
			Assert.AreEqual(3, testSetFolder.CountNotRun);
			Assert.AreEqual(70, testSetFolder.EstimatedDuration);
			Assert.IsNull(testSetFolder.ExecutionDate);
			Assert.IsNull(testSetFolder.ActualDuration);

			//Obtain the highest History entry to verify that it was set properly.
			long? latestHistory = GetHistoryId();

			//Now create the pending test run record from the test set
			TestRunsPending testRunsPending = testRunManager.CreateFromTestSet(USER_ID_FRED_BLOGGS, projectId, testSetId, true);

			//First make sure we have the expected number of test cases and test steps
			int pendingId = testRunsPending.TestRunsPendingId;
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			Assert.AreEqual(3, testRunsPending.TestRuns.Count);
			Assert.AreEqual(5, testRunsPending.TestRuns[0].TestRunSteps.Count);
			Assert.AreEqual(4, testRunsPending.TestRuns[1].TestRunSteps.Count);
			Assert.AreEqual(9, testRunsPending.TestRuns[2].TestRunSteps.Count);

			//Verify the summary counts
			Assert.AreEqual(3, testRunsPending.CountNotRun);

			//Also verify that the corresponding list custom properties are set to match the test set
			foreach (TestRun testRun in testRunsPending.TestRuns)
			{
				TestRunView testRunView = testRunManager.RetrieveById(testRun.TestRunId);
				Assert.AreEqual(customValueId11, testRunView.Custom_02.FromDatabaseSerialization_Int32());
			}

			//Next make sure that the status of the test set switched to 'In Progress'
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, testSetView.OwnerId, "OwnerId1");
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.InProgress, testSetView.TestSetStatusId, "TestSetStatusId1");

			//Now make sure we have the test case names, test set id and release id that we expect
			Assert.AreEqual("Ability to create new book", testRunsPending.TestRuns[0].Name);
			Assert.AreEqual(releaseId1, testRunsPending.TestRuns[0].ReleaseId);
			Assert.AreEqual(testSetId, testRunsPending.TestRuns[0].TestSetId);
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[0].ChangeSetId, "ChangesetID was not what was expected!");
			Assert.AreEqual("Ability to delete existing book", testRunsPending.TestRuns[1].Name);
			Assert.AreEqual(releaseId1, testRunsPending.TestRuns[1].ReleaseId);
			Assert.AreEqual(testSetId, testRunsPending.TestRuns[1].TestSetId);
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[1].ChangeSetId, "ChangesetID was not what was expected!");
			Assert.AreEqual("Book management testing", testRunsPending.TestRuns[2].Name);
			Assert.AreEqual(releaseId1, testRunsPending.TestRuns[2].ReleaseId);
			Assert.AreEqual(testSetId, testRunsPending.TestRuns[2].TestSetId);
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[2].ChangeSetId, "ChangesetID was not what was expected!");

			//If delete the pending item now (by completing the test run) it will mark it as deferred since not all tests run
			testRunManager.CompletePending(pendingId, USER_ID_FRED_BLOGGS);
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Deferred, testSetView.TestSetStatusId, "TestSetStatusId2");
			Assert.AreEqual(USER_ID_FRED_BLOGGS, testSetView.OwnerId);

			//Now we need to test what happens when we complete a test set. Need to create a new run again
			testRunsPending = testRunManager.CreateFromTestSet(USER_ID_FRED_BLOGGS, projectId, testSetId, true);
			pendingId = testRunsPending.TestRunsPendingId;

			//Next make sure that the status of the test set switched to 'In Progress'
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.InProgress, testSetView.TestSetStatusId, "TestSetStatusId3");

			//Now fail the test and then complete the test run
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, DateTime.UtcNow, true, false);
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunsPending.TestRuns[1].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[1].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 1, DateTime.UtcNow, true, false);
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunsPending.TestRuns[2].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[2].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 2, DateTime.UtcNow, true, false);
			testRunManager.CompletePending(pendingId, USER_ID_FRED_BLOGGS);

			//Finally verify that the test set is now marked as completed, with the owner and planned date unset
			testSetView = testSetManager.RetrieveById(projectId, testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Completed, testSetView.TestSetStatusId, "TestSetStatusId4");
			Assert.IsTrue(testSetView.PlannedDate.IsNull(), "PlannedDate");
			Assert.IsTrue(testSetView.OwnerId.IsNull(), "OwnerId2");
		}

		/// <summary>
		/// Tests that executing a test case or test set against a release will add the unmapped
		/// test cases to the selected release. New feature in v2.2. Since v4.0 also rolls-up from iteration > release
		/// </summary>
		[
		Test,
		SpiraTestCase(435)
		]
		public void _13_AddsToReleaseWhenExecuted()
		{
			//To test this out we need to create a new release in the current project
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Empty Release", "", "1.2", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1, 0, null, false);

			//Verify that there are no mapped test cases
			List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId);
			Assert.AreEqual(0, mappedTestCases.Count);

			//Now create a test run from a test case using this release
			List<int> testCaseList = new List<int>();
			testCaseList.Add(testCaseId1);
			testCaseList.Add(testCaseId3);
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, releaseId, testCaseList, true);
			testRunManager.CompletePending(testRunsPending.TestRunsPendingId, USER_ID_FRED_BLOGGS);

			//Verify that the tests were added to the release
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId);
			Assert.AreEqual(2, mappedTestCases.Count);

			//Now create a test set and add some test cases (one of which is already in the release)
			TestSetManager testSetManager = new TestSetManager();
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, releaseId, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "New Test Set", "", DateTime.UtcNow, TestRun.TestRunTypeEnum.Manual, null, null);
			testSetManager.AddTestCase(projectId, testSetId, testCaseId1, null, null);
			testSetManager.AddTestCase(projectId, testSetId, testCaseId4, null, null);

			//Now create a test run from a test set using this release
			testRunsPending = testRunManager.CreateFromTestSet(USER_ID_FRED_BLOGGS, projectId, testSetId, true);
			testRunManager.CompletePending(testRunsPending.TestRunsPendingId, USER_ID_FRED_BLOGGS);

			//Verify that the tests were added to the release
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId);
			Assert.AreEqual(3, mappedTestCases.Count);

			//Clean up by unmapping the tests from the release
			testCaseManager.RemoveFromRelease(projectId, releaseId, new List<int>() { testCaseId1, testCaseId3, testCaseId4 }, USER_ID_SYS_ADMIN);
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId);
			Assert.AreEqual(0, mappedTestCases.Count);

			//Now we need to verify that executing a child iteration of the release adds the test cases to both
			//the iteration and the release itself
			int iterationId = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Empty Release #0001", "", "1.2 #0001", releaseId, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1, 0, null, false);

			//Verify that there are no mapped test cases for either the release or iteration
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId);
			Assert.AreEqual(0, mappedTestCases.Count);
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, iterationId);
			Assert.AreEqual(0, mappedTestCases.Count);

			//Now create a test run from a test case using this iteration
			testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, iterationId, testCaseList, true);
			testRunManager.CompletePending(testRunsPending.TestRunsPendingId, USER_ID_FRED_BLOGGS);

			//Verify that the test cases were automatically added to both the release and iteration
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, iterationId);
			Assert.AreEqual(2, mappedTestCases.Count);
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId);
			Assert.AreEqual(2, mappedTestCases.Count);
		}

		/// <summary>
		/// Tests that you can retrieve a filtered, sorted list of test runs, and also retrieve certain lookups
		/// </summary>
		[
		Test,
		SpiraTestCase(685)
		]
		public void _14_CreateFromTestCasesInSet()
		{
			//We need to verify that we can create a test run from a group of test cases inside a test set
			//useful when we want to just rerun a few test cases from the set
			TestSetManager testSetManager = new TestSetManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, null, null, null);
			int testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, null, null, null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, null, "Step 1.1", "Expected", null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId2, null, "Step 2.1", "Expected", null);
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, DateTime.UtcNow, TestRun.TestRunTypeEnum.Manual, null, null);
			testSetManager.AddTestCase(projectId, testSetId, testCaseId1, USER_ID_FRED_BLOGGS, null);
			testSetManager.AddTestCase(projectId, testSetId, testCaseId2, USER_ID_JOE_SMITH, null);
			testSetManager.AddTestCase(projectId, testSetId, testCaseId1, null, null);
			testSetManager.AddTestCase(projectId, testSetId, testCaseId2, USER_ID_JOE_SMITH, null);

			//Set the Operating System custom property of the test set to a value
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectId, Artifact.ArtifactTypeEnum.TestSet, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestSet, testSetId, customProperties);
			artifactCustomProperty.SetCustomProperty(1, customValueId12);   //Windows 8
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, USER_ID_FRED_BLOGGS);

			//Obtain the highest History entry to verify that it was set properly.
			long? latestHistory = GetHistoryId();

			//Now create the pending test run record from two of the test cases in the set
			List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId);
			List<int> testSetTestCaseIds = new List<int>();
			testSetTestCaseIds.Add(testSetTestCases[0].TestSetTestCaseId);
			testSetTestCaseIds.Add(testSetTestCases[1].TestSetTestCaseId);
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCasesInSet(USER_ID_FRED_BLOGGS, projectId, testSetId, testSetTestCaseIds, true);

			//First make sure we have the expected number of test cases and test steps
			int pendingId = testRunsPending.TestRunsPendingId;
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			Assert.AreEqual(2, testRunsPending.TestRuns.Count);

			//Verify the ChangeSetId
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[0].ChangeSetId, "ChangesetID was not what was expected!");
			//Assert.AreEqual(latestHistory, testRunsPending.TestRuns[1].ChangeSetId, "ChangesetID was not what was expected!");

			//Verify that the owners are set correctly (we don't use the individual owners in this use case)
			Assert.AreEqual(USER_ID_FRED_BLOGGS, testRunsPending.TestRuns[0].TesterId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, testRunsPending.TestRuns[1].TesterId);

			//Also verify that the corresponding list custom properties are set to match the test set
			//Also verify that the test set is set correctly
			foreach (TestRun testRun in testRunsPending.TestRuns)
			{
				TestRunView testRunView = testRunManager.RetrieveById(testRun.TestRunId);
				Assert.AreEqual(customValueId12, testRunView.Custom_02.FromDatabaseSerialization_Int32());
				Assert.AreEqual(testSetId, testRunView.TestSetId);
			}

			//Now fail the first test, block the second and then complete the test run
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			testRunsPending.TestRuns[0].TestRunSteps[0].ActualResult = "Failed";
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, DateTime.UtcNow, true, false);
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunsPending.TestRuns[1].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[1].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
			testRunsPending.TestRuns[1].TestRunSteps[0].ActualResult = "Blocked";
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 1, DateTime.UtcNow, true, false);
			testRunManager.CompletePending(pendingId, USER_ID_FRED_BLOGGS);

			//Verify the status of the test cases in the test set. Make sure that the fail/block status only
			//affected the first two test cases in the set, even though the other two are actually the same two 
			//test cases repeated
			testSetTestCases = testSetManager.RetrieveTestCases3(projectId, testSetId, null);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testSetTestCases[0].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, testSetTestCases[1].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testSetTestCases[2].ExecutionStatusId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testSetTestCases[3].ExecutionStatusId);
		}

		/// <summary>
		/// Tests that you can associate a manual and automated test run against a build
		/// </summary>
		[
		Test,
		SpiraTestCase(812)
		]
		public void _15_TestRunsLinkedToBuilds()
		{
			//First we need to create a new iteration to associate any builds with
			ReleaseManager releaseManager = new ReleaseManager();
			int iterationId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.0.0.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1, 0, null);

			//Now create a build for this iteration
			BuildManager buildManager = new BuildManager();
			int buildId = buildManager.Insert(projectId, iterationId, "Build 0001", "Nice Build", DateTime.UtcNow, Build.BuildStatusEnum.Succeeded, USER_ID_FRED_BLOGGS).BuildId;

			//Create a new test case with one test step
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, null, null, null, null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, null, "Step 1.1", "Expected", null);

			//Now record a manual test run
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, iterationId, new List<int>() { testCaseId1 }, true);

			//Now update the build associated with the test run
			testRunsPending = testRunManager.RetrievePendingById(testRunsPending.TestRunsPendingId, true);
			testRunsPending.TestRuns[0].StartTracking();
			testRunsPending.TestRuns[0].BuildId = buildId;
			testRunManager.Update(testRunsPending, USER_ID_FRED_BLOGGS);

			//Now pass the step and complete the run
			testRunsPending = testRunManager.RetrievePendingById(testRunsPending.TestRunsPendingId, true);
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, DateTime.UtcNow, true, false);
			testRunManager.CompletePending(testRunsPending.TestRunsPendingId, USER_ID_FRED_BLOGGS);

			//Now retrieve the run by build id
			Hashtable filters = new Hashtable();
			filters.Add("BuildId", buildId);
			List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "TestRunId", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, testRuns.Count);
			Assert.AreEqual("Build 0001", testRuns[0].BuildName);

			//Test that phsically deleting the release, cleans up the build-incident relationships
			releaseManager.DeleteFromDatabase(iterationId, USER_ID_FRED_BLOGGS);

			//Clean up
			testCaseManager.DeleteFromDatabase(testCaseId1, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(246)
		]
		public void _16_RetrieveSummaryData()
		{
			//First get the daily test run count untyped dataset - all releases
			List<TestRun_DailyCount> testRunDailyCounts = testRunManager.RetrieveDailyCount(PROJECT_ID, -5, null);
			//Make sure the data is as expected
			Assert.AreEqual(5, testRunDailyCounts.Count);
			Assert.IsTrue(testRunDailyCounts[3].ExecutionCount > 0);
			Assert.IsTrue(testRunDailyCounts[3].ExecutionDate.HasValue);

			//First get the daily test run count untyped dataset - specific release
			testRunDailyCounts = testRunManager.RetrieveDailyCount(PROJECT_ID, -5, 1);
			//Make sure the data is as expected
			Assert.GreaterOrEqual(4, testRunDailyCounts.Count);
			Assert.IsTrue(testRunDailyCounts[0].ExecutionCount > 0);
			Assert.IsTrue(testRunDailyCounts[0].ExecutionDate.HasValue);

			//First get the daily test run count untyped dataset - release that has runs against only its iterations
			testRunDailyCounts = testRunManager.RetrieveDailyCount(PROJECT_ID, -5, 4);
			//Make sure the data is as expected
			Assert.AreEqual(5, testRunDailyCounts.Count);
			Assert.IsTrue(testRunDailyCounts[0].ExecutionCount > 0);
			Assert.IsTrue(testRunDailyCounts[0].ExecutionDate.HasValue);

			//Now get the total number of test runs and verify
			int runCount = testRunManager.Count(PROJECT_ID);
			Assert.AreEqual(30, runCount);
		}

		[
		Test,
		SpiraTestCase(211)
		]
		public void _17_RetrieveTestRunsByRelease()
		{
			//Verify that we can retrieve all the runs associated with a particular release
			Hashtable filters = new Hashtable();
			filters.Add("ReleaseId", 1);
			List<TestRunView> testRuns = testRunManager.Retrieve(PROJECT_ID, "ExecutionDate", false, 1, 10, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(7, testRuns.Count, "TestRunCount1");
			Assert.AreEqual(6, testRuns[0].TestRunId, "TestRunId1");
			Assert.AreEqual("Ability to edit existing book", testRuns[0].Name);
			Assert.AreEqual(7, testRuns[1].TestRunId, "TestRunId2");
			Assert.AreEqual("Adding new book and author to library", testRuns[1].Name);
			Assert.AreEqual(9, testRuns[2].TestRunId, "TestRunId3");
			Assert.AreEqual("Ability to create new book", testRuns[2].Name);

			//Verify that the count (without pagination) works correctly
			int testRunCount = testRunManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(7, testRunCount, "TestRunCount2");

			//Verify that the count for active releases only works correctly
			testRunCount = testRunManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET, true);
			Assert.AreEqual(18, testRunCount, "testRunManager.Count for active releases only");
			testRunCount = testRunManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET, false);
			Assert.AreEqual(30, testRunCount, "testRunManager.Count for all releases");
		}

		/// <summary>
		/// Tests that you can retrieve a filtered, sorted list of test runs, and also retrieve certain lookups
		/// </summary>
		[
		Test,
		SpiraTestCase(555)
		]
		public void _18_RetrieveFilterSort()
		{
			//First get the list of available test run types
			List<TestRunType> testRunTypes = testRunManager.RetrieveTypes();
			Assert.AreEqual(2, testRunTypes.Count);
			Assert.AreEqual("Automated", testRunTypes[0].Name);
			Assert.AreEqual("Manual", testRunTypes[1].Name);

			//Next get a list of test runs in the project unfiltered and sorted by ID ascending
			List<TestRunView> testRuns = testRunManager.Retrieve(PROJECT_ID, "TestRunId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(30, testRuns.Count, "Count1");
			Assert.AreEqual(30, testRunManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET), "Count2");
			//Record 0
			Assert.AreEqual(8, testRuns[0].TestCaseId);
			Assert.AreEqual("Book management", testRuns[0].Name);
			Assert.AreEqual("1.0.1.0", testRuns[0].ReleaseVersionNumber);
			//Record 1
			Assert.AreEqual(5, testRuns[1].TestCaseId);
			Assert.AreEqual("Ability to edit existing author", testRuns[1].Name);
			Assert.AreEqual("1.0.1.0", testRuns[1].ReleaseVersionNumber);
			//Record 17
			Assert.AreEqual(18, testRuns[17].TestRunId);
			Assert.AreEqual(2, testRuns[17].TestCaseId);
			Assert.AreEqual("Ability to create new book", testRuns[17].Name);
			Assert.AreEqual("1.1.2.0", testRuns[17].ReleaseVersionNumber);

			//Next get a list of test runs filtered by tester, where status = failed,blocked,caution
			//order by execution date descending
			Hashtable filters = new Hashtable();
			filters.Add("TesterId", USER_ID_FRED_BLOGGS);
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add((int)TestCase.ExecutionStatusEnum.Failed);
			multiValueFilter.Values.Add((int)TestCase.ExecutionStatusEnum.Blocked);
			multiValueFilter.Values.Add((int)TestCase.ExecutionStatusEnum.Caution);
			filters.Add("ExecutionStatusId", multiValueFilter);
			testRuns = testRunManager.Retrieve(PROJECT_ID, "ExecutionDate", false, 1, 99999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(9, testRuns.Count, "Count1");
			Assert.AreEqual(9, testRunManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET), "Count2");
			//Record 0
			Assert.AreEqual(2, testRuns[0].TestRunId);
			Assert.AreEqual(5, testRuns[0].TestCaseId);
			Assert.AreEqual("Ability to edit existing author", testRuns[0].Name);
			Assert.AreEqual("1.0.1.0", testRuns[0].ReleaseVersionNumber);
			//Record 1
			Assert.AreEqual(12, testRuns[1].TestRunId);
			Assert.AreEqual(3, testRuns[1].TestCaseId);
			Assert.AreEqual("Ability to edit existing book", testRuns[1].Name);
			Assert.AreEqual("1.0.0.0", testRuns[1].ReleaseVersionNumber);
			//Record 5
			Assert.AreEqual(21, testRuns[5].TestRunId);
			Assert.AreEqual(8, testRuns[5].TestCaseId);
			Assert.AreEqual("Book management", testRuns[5].Name);
			Assert.AreEqual("1.1.2.0", testRuns[5].ReleaseVersionNumber);

			//Next get a list of test runs filtered by date-range and type=automated
			//order by estimated duration ascending
			filters.Clear();
			filters.Add("TestRunTypeId", (int)TestRun.TestRunTypeEnum.Automated);
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.AddDays(-30);
			dateRange.EndDate = DateTime.UtcNow.AddDays(-10);
			filters.Add("EndDate", dateRange);
			testRuns = testRunManager.Retrieve(PROJECT_ID, "EstimatedDuration", true, 1, 99999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, testRuns.Count, "Count1");
			Assert.AreEqual(8, testRunManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET), "Count2");
			//Record 0
			Assert.AreEqual(16, testRuns[0].TestRunId);
			Assert.AreEqual(2, testRuns[0].TestCaseId);
			Assert.AreEqual("Ability to create new book", testRuns[0].Name);
			Assert.AreEqual("1.1.0.0.0001", testRuns[0].ReleaseVersionNumber);
			//Record 1
			Assert.AreEqual(25, testRuns[1].TestRunId);
			Assert.AreEqual(2, testRuns[1].TestCaseId);
			Assert.AreEqual("Ability to create new book", testRuns[1].Name);
			Assert.AreEqual("1.1.0.0.0002", testRuns[1].ReleaseVersionNumber);
			//Record 2
			//Assert.AreEqual(13, testRuns[2].TestRunId);
			//Assert.AreEqual(2, testRuns[2].TestCaseId);
			//Assert.AreEqual("Ability to create new book", testRuns[2].Name);
			//Assert.AreEqual("1.0.0.0", testRuns[2].ReleaseVersionNumber);
		}

		[
		Test,
		SpiraTestCase(404)
		]
		public void _19_RetrieveTestRunsByTestSet()
		{
			//Verify that we can retrieve all the runs associated with a particular test set
			Hashtable filters = new Hashtable();
			filters.Add("TestSetId", 1);
			List<TestRunView> testRuns = testRunManager.Retrieve(PROJECT_ID, "ExecutionDate", false, 1, 10, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, testRuns.Count, "TestRunCount1");

			Assert.AreEqual(9, testRuns[0].TestRunId);
			Assert.AreEqual("Ability to create new book", testRuns[0].Name);
			Assert.AreEqual("Failed", testRuns[0].ExecutionStatusName);
			Assert.AreEqual("1.0.0.0", testRuns[0].ReleaseVersionNumber);

			Assert.AreEqual(10, testRuns[1].TestRunId);
			Assert.AreEqual("Ability to edit existing book", testRuns[1].Name);
			Assert.AreEqual("Passed", testRuns[1].ExecutionStatusName);
			Assert.AreEqual("1.0.0.0", testRuns[1].ReleaseVersionNumber);

			Assert.AreEqual(11, testRuns[2].TestRunId);
			Assert.AreEqual("Ability to create new author", testRuns[2].Name);
			Assert.AreEqual("Failed", testRuns[2].ExecutionStatusName);
			Assert.AreEqual("1.0.0.0", testRuns[2].ReleaseVersionNumber);

			//Verify that the count (without pagination) works correctly
			int testRunCount = testRunManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, testRunCount, "TestRunCount2");
		}

		/// <summary>
		/// Tests that we can execute exploratory tests, the task creation/association part is tested elsewhere
		/// </summary>
		[
		Test,
		SpiraTestCase(1611)
		]
		public void _20_ExploratoryTestExecution()
		{
			//First we have to create a new test case, we'll add some initial steps to it
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Exploring the book creation pages", "Charter that describes how to do this", tc_typeExploratoryId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityCriticalId, null, 20, null, null);
			int testStepId1 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Login to the app", "It works OK", null);
			int testStepId2 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Go to the main menu", "It works OK", null);
			int testStepId3 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Choose to create a book", "It works OK", null);
			int testStepId4 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Fill in the book details", "It works OK", null);
			int testStepId5 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, null, "Submit the book", "It works OK", null);

			//Then we execute it to create a test run
			TestRunManager testRunManager = new TestRunManager();
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, null, new List<int>() { testCaseId }, true);
			int pendingId = testRunsPending.TestRunsPendingId;

			//Verify that the ChangeSetID is *NOT* set.,
			Assert.IsNull(testRunsPending.TestRuns[0].ChangeSetId, "ChangeSetId was not null!");

			//Now we need to pass the first step (not updating the test case)
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			TestRun testRun = testRunsPending.TestRuns[0];
			TestRunStep testRunStep = testRun.TestRunSteps[0];
			testRunStep.StartTracking();
			testRunStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunStep.ActualResult = null;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, null, true, false, false);

			//Now we need to pass and change the second step
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRun = testRunsPending.TestRuns[0];
			testRunStep = testRun.TestRunSteps[1];
			testRunStep.StartTracking();
			testRunStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunStep.Description = "Click on Book Management in the navigation bar";
			testRunStep.ExpectedResult = "You are taken to the book list page";
			testRunStep.ActualResult = null;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, null, true, false, false);

			//Now we need to change the test run and third step without recording a status
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunsPending.TestRuns[0].StartTracking();
			testRunsPending.TestRuns[0].Name = "Testing the book creation pages";
			testRunsPending.TestRuns[0].TestRunSteps[2].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[2].Description = "Click on the 'create new book' hyperlink";
			testRunManager.UpdateExploratoryTestRun(projectId, USER_ID_FRED_BLOGGS, testRunsPending);

			//Now we need to mark this step as caution but otherwise not change it
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRun = testRunsPending.TestRuns[0];
			testRunStep = testRun.TestRunSteps[2];
			testRunStep.StartTracking();
			testRunStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;
			testRunStep.ActualResult = "The create new book hyperlink is misspelt";
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, null, true, false, false);

			//Now add a step to the run (gets added to the end)
			int newTestRunStepId1 = testRunManager.CreateNewExploratoryTestRunStep(testRun.TestRunId, testRun.TestCaseId);

			//Now we need to update its text
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunStep = testRunsPending.TestRuns[0].TestRunSteps.FirstOrDefault(t => t.TestRunStepId == newTestRunStepId1);
			testRunStep.StartTracking();
			testRunStep.Description = "Verify that the book was successfully created";
			testRunStep.ExpectedResult = "The book is created with all fields matching";
			testRunStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			testRunManager.UpdateExploratoryTestRun(projectId, USER_ID_FRED_BLOGGS, testRunsPending);

			//Lets create a new step and move it to the first position
			int newTestRunStepId2 = testRunManager.CreateNewExploratoryTestRunStep(testRun.TestRunId, testRun.TestCaseId);
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunStep = testRunsPending.TestRuns[0].TestRunSteps.FirstOrDefault(t => t.TestRunStepId == newTestRunStepId2);
			testRunStep.StartTracking();
			testRunStep.Description = "Go to the application login page";
			testRunStep.ExpectedResult = "The login form is displayed";
			testRunManager.UpdateExploratoryTestRun(projectId, USER_ID_FRED_BLOGGS, testRunsPending);

			//Now actually move the step
			List<TestRunStepPosition> testRunStepPositions = new List<TestRunStepPosition>();
			testRunStepPositions.Add(new TestRunStepPosition() { TestRunStepId = newTestRunStepId2, Position = 1 });
			testRunStepPositions.Add(new TestRunStepPosition() { TestRunStepId = testRunsPending.TestRuns[0].TestRunSteps[0].TestRunStepId, Position = 2 });
			testRunStepPositions.Add(new TestRunStepPosition() { TestRunStepId = testRunsPending.TestRuns[0].TestRunSteps[1].TestRunStepId, Position = 3 });
			testRunStepPositions.Add(new TestRunStepPosition() { TestRunStepId = testRunsPending.TestRuns[0].TestRunSteps[2].TestRunStepId, Position = 4 });
			testRunStepPositions.Add(new TestRunStepPosition() { TestRunStepId = testRunsPending.TestRuns[0].TestRunSteps[3].TestRunStepId, Position = 5 });
			testRunStepPositions.Add(new TestRunStepPosition() { TestRunStepId = testRunsPending.TestRuns[0].TestRunSteps[4].TestRunStepId, Position = 6 });
			testRunStepPositions.Add(new TestRunStepPosition() { TestRunStepId = testRunsPending.TestRuns[0].TestRunSteps[5].TestRunStepId, Position = 7 });
			testRunManager.UpdateExploratoryTestRunStepPositions(testRun.TestRunId, testRunStepPositions);

			//Now lets verify the current position and state of the pending test run
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);

			//First verify the test run itself
			testRun = testRunsPending.TestRuns[0];
			Assert.AreEqual("Testing the book creation pages", testRun.Name);
			Assert.AreEqual("Charter that describes how to do this", testRun.Description);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testRun.ExecutionStatusId);

			//Now the steps

			//Step 1
			testRunStep = testRun.TestRunSteps[0];
			Assert.AreEqual(newTestRunStepId2, testRunStep.TestRunStepId);
			Assert.AreEqual(1, testRunStep.Position);
			Assert.AreEqual("Go to the application login page", testRunStep.Description);
			Assert.AreEqual("The login form is displayed", testRunStep.ExpectedResult);
			Assert.AreEqual(null, testRunStep.ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunStep.ExecutionStatusId);
			//Step 2
			testRunStep = testRun.TestRunSteps[1];
			Assert.AreEqual(2, testRunStep.Position);
			Assert.AreEqual("Login to the app", testRunStep.Description);
			Assert.AreEqual("It works OK", testRunStep.ExpectedResult);
			Assert.AreEqual(null, testRunStep.ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRunStep.ExecutionStatusId);
			//Step 3
			testRunStep = testRun.TestRunSteps[2];
			Assert.AreEqual(3, testRunStep.Position);
			Assert.AreEqual("Click on Book Management in the navigation bar", testRunStep.Description);
			Assert.AreEqual("You are taken to the book list page", testRunStep.ExpectedResult);
			Assert.AreEqual(null, testRunStep.ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRunStep.ExecutionStatusId);
			//Step 4
			testRunStep = testRun.TestRunSteps[3];
			Assert.AreEqual(4, testRunStep.Position);
			Assert.AreEqual("Click on the 'create new book' hyperlink", testRunStep.Description);
			Assert.AreEqual("It works OK", testRunStep.ExpectedResult);
			Assert.AreEqual("The create new book hyperlink is misspelt", testRunStep.ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testRunStep.ExecutionStatusId);
			//Step 5
			testRunStep = testRun.TestRunSteps[4];
			Assert.AreEqual(5, testRunStep.Position);
			Assert.AreEqual("Fill in the book details", testRunStep.Description);
			Assert.AreEqual("It works OK", testRunStep.ExpectedResult);
			Assert.AreEqual(null, testRunStep.ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunStep.ExecutionStatusId);
			//Step 6
			testRunStep = testRun.TestRunSteps[5];
			Assert.AreEqual(6, testRunStep.Position);
			Assert.AreEqual("Submit the book", testRunStep.Description);
			Assert.AreEqual("It works OK", testRunStep.ExpectedResult);
			Assert.AreEqual(null, testRunStep.ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testRunStep.ExecutionStatusId);
			//Step 7
			testRunStep = testRun.TestRunSteps[6];
			Assert.AreEqual(newTestRunStepId1, testRunStep.TestRunStepId);
			Assert.AreEqual(7, testRunStep.Position);
			Assert.AreEqual("Verify that the book was successfully created", testRunStep.Description);
			Assert.AreEqual("The book is created with all fields matching", testRunStep.ExpectedResult);
			Assert.AreEqual(null, testRunStep.ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRunStep.ExecutionStatusId);

			//Now lets clone one of the steps
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRun = testRunsPending.TestRuns[0];
			testRunStep = testRun.TestRunSteps[0];
			int newTestRunStepId3 = testRunManager.CloneExploratoryTestRunStep(testRun.TestRunId, testRun.TestCaseId, testRunStep.Description, testRunStep.ExpectedResult, testRunStep.SampleData);

			//Verify that it cloned OK
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRun = testRunsPending.TestRuns[0];
			testRunStep = testRun.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == newTestRunStepId3);
			Assert.AreEqual(8, testRunStep.Position);
			Assert.AreEqual(testRun.TestRunSteps[0].Description, testRunStep.Description);
			Assert.AreEqual(testRun.TestRunSteps[0].ExpectedResult, testRunStep.ExpectedResult);
			Assert.AreEqual(testRun.TestRunSteps[0].SampleData, testRunStep.SampleData);

			//Now lets delete the clone
			testRunManager.DeleteExploratoryTestRunStep(testRun.TestRunId, newTestRunStepId3);

			//Verify that it deleted OK
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRun = testRunsPending.TestRuns[0];
			testRunStep = testRun.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == newTestRunStepId3);
			Assert.IsNull(testRunStep);

			//Now we need to verify that the overall run status is Caution, but the test case is not changed (unlike normal test cases)
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRun = testRunsPending.TestRuns[0];
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testRun.ExecutionStatusId);
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCaseView.ExecutionStatusId);

			//Now we need to call the function that will update the status of the test case, but not change
			//the test case itself
			testRunManager.UpdateExploratoryTestCaseFromRun(projectId, testRun.TestRunId, USER_ID_FRED_BLOGGS, true);

			//Verify the status of the test run and test case now
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRun = testRunsPending.TestRuns[0];
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testRun.ExecutionStatusId);
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCaseView.ExecutionStatusId);

			//Retrieve the test case and steps and verify they are not changed
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			//Test Case
			Assert.AreEqual("Exploring the book creation pages", testCase.Name);
			Assert.AreEqual("Charter that describes how to do this", testCase.Description);
			//Test Steps
			Assert.AreEqual(5, testCase.TestSteps.Count);
			Assert.AreEqual("Login to the app", testCase.TestSteps[0].Description);
			Assert.AreEqual("Go to the main menu", testCase.TestSteps[1].Description);
			Assert.AreEqual("Choose to create a book", testCase.TestSteps[2].Description);
			Assert.AreEqual("Fill in the book details", testCase.TestSteps[3].Description);
			Assert.AreEqual("Submit the book", testCase.TestSteps[4].Description);

			//Now we need to call the function that will finalize the test run and update the status of the test case and change its steps
			testRunManager.UpdateExploratoryTestCaseFromRun(projectId, testRun.TestRunId, USER_ID_FRED_BLOGGS, false);

			//Verify that the test cases and steps were changed based on the test run
			//The execution status should also be updated
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			//Test Case
			Assert.AreEqual("Testing the book creation pages", testCase.Name);
			Assert.AreEqual("Charter that describes how to do this", testCase.Description);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCase.ExecutionStatusId);
			//Test Steps
			Assert.AreEqual(7, testCase.TestSteps.Count);
			Assert.AreEqual("Go to the application login page", testCase.TestSteps[0].Description);
			Assert.AreEqual(1, testCase.TestSteps[0].Position);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.TestSteps[0].ExecutionStatusId);
			Assert.AreEqual("Login to the app", testCase.TestSteps[1].Description);
			Assert.AreEqual(2, testCase.TestSteps[1].Position);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.TestSteps[1].ExecutionStatusId);
			Assert.AreEqual("Click on Book Management in the navigation bar", testCase.TestSteps[2].Description);
			Assert.AreEqual(3, testCase.TestSteps[2].Position);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.TestSteps[2].ExecutionStatusId);
			Assert.AreEqual("Click on the 'create new book' hyperlink", testCase.TestSteps[3].Description);
			Assert.AreEqual(4, testCase.TestSteps[3].Position);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, testCase.TestSteps[3].ExecutionStatusId);
			Assert.AreEqual("Fill in the book details", testCase.TestSteps[4].Description);
			Assert.AreEqual(5, testCase.TestSteps[4].Position);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.TestSteps[4].ExecutionStatusId);
			Assert.AreEqual("Submit the book", testCase.TestSteps[5].Description);
			Assert.AreEqual(6, testCase.TestSteps[5].Position);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.TestSteps[5].ExecutionStatusId);
			Assert.AreEqual("Verify that the book was successfully created", testCase.TestSteps[6].Description);
			Assert.AreEqual(7, testCase.TestSteps[6].Position);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.TestSteps[6].ExecutionStatusId);

			//Verify that the expected history entries were logged
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(testCaseId, Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(1, historyChangeSets.Count);
			//Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[0].ChangeTypeId);
			//Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[1].ChangeTypeId);
			//Assert.IsTrue(historyChangeSets[0].Details.Any(h => h.FieldName == "Name"));

			historyChangeSets = historyManager.RetrieveByArtifactId(testCase.TestSteps[2].TestStepId, Artifact.ArtifactTypeEnum.TestStep);
			Assert.AreEqual(1, historyChangeSets.Count);
			//Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[0].ChangeTypeId);
			//Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[1].ChangeTypeId);
			//Assert.IsTrue(historyChangeSets[0].Details.Any(h => h.FieldName == "Description"));

			//Finally remove the pending entry (when the user clicks FINISH)
			testRunManager.CompletePending(pendingId, USER_ID_FRED_BLOGGS);

			//Verify that it can no longer be retrieved
			bool exceptionThrown = false;
			try
			{
				testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);
		}


		/// <summary>Common funciton to get the latest ChangeSet ID for verification of history logging.</summary>
		/// <returns></returns>
		private long? GetHistoryId()
		{
			long? retValue = null;

			//Obtain the highest History entry to verify that it was set properly.
			var histList = new HistoryManager().RetrieveSetsByProjectId(projectId, 0);
			if (histList.Count > 0)
				retValue = histList.OrderByDescending(f => f.ChangeSetId).FirstOrDefault().ChangeSetId;

			return retValue;
		}
	}
}
