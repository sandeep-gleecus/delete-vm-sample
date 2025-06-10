using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.TestSuite;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;

namespace Inflectra.SpiraTest.ApiTestSuite.API_Tests
{
	/// <summary>
	/// This fixture tests that the v1.5.2 import/export web service interface works correctly
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class V1_ApiTest
	{
		protected static SpiraImport152.Import spiraImport;
		protected static CookieContainer cookieContainer;
		protected static int newProjectId;
		protected static int projectId3;
		protected static int projectTemplateId;
		protected static int projectTemplateId3;

		protected static int requirementId1;
		protected static int requirementId2;
		protected static int requirementId3;
		protected static int requirementId4;
		protected static int requirementId5;

		protected static int testFolderId1;
		protected static int testFolderId2;
		protected static int testCaseId1;
		protected static int testCaseId2;
		protected static int testCaseId3;
		protected static int testCaseId4;

		protected static int testStepId1;
		protected static int testStepId2;
		protected static int testStepId3;
		protected static int testStepId4;
		protected static int testStepId5;
		protected static int testStepId6;

		protected static int testSetFolderId1;
		protected static int testSetId1;
		protected static int testSetId2;

		protected static int incidentId1;
		protected static int incidentId2;
		protected static int incidentId3;

		protected static int userId1;
		protected static int userId2;

		protected static int testRunId;

		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;

		private const int PROJECT_ID = 1;
		private const int USER_ID_SYSTEM_ADMIN = 1;

		/// <summary>
		/// Sets up the web service interface
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the web-service proxy class and set the URL from the .config file
			spiraImport = new SpiraImport152.Import();
			spiraImport.Url = Properties.Settings.Default.WebServiceUrl + "Import.asmx";

			//Create a new cookie container to hold the session handle
			cookieContainer = new CookieContainer();
			spiraImport.CookieContainer = cookieContainer;

			//Get the last artifact id
			Business.HistoryManager history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);
		}

		/// <summary>
		/// Cleans up any changes to the test data
		/// </summary>
		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the project
			Business.ProjectManager projectManager = new Business.ProjectManager();
			projectManager.Delete(Business.UserManager.UserSystemAdministrator, newProjectId);

			//Delete any other projects
			if (projectId3 > 0)
			{
				projectManager.Delete(Business.UserManager.UserSystemAdministrator, projectId3);
			}

			//Delete the template (no v1.0 api call available for this)
			TemplateManager templateManager = new TemplateManager();
			templateManager.Delete(Business.UserManager.UserSystemAdministrator, projectTemplateId);

			//Delete any other project templates
			if (projectTemplateId3 > 0)
			{
				templateManager.Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId3);
			}

			//Now delete the two users
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE USER_ID = " + userId1.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE USER_ID = " + userId2.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER WHERE USER_ID = " + userId1.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER WHERE USER_ID = " + userId2.ToString());

			//We need to delete any artifact history items created during the test run
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());
		}

		/// <summary>
		/// Verifies that you can authenticate as a specific user
		/// </summary>
		[
		Test,
		SpiraTestCase(144)
		]
		public void _01_Authentication()
		{
			//First Authenticate as Fred Bloggs (should succeed)
			bool success = spiraImport.Authenticate("fredbloggs", "PleaseChange");
			Assert.IsTrue(success, "Authentication 1 Failed");

			//Next Authenticate as System Administrator (should succeed)
			success = spiraImport.Authenticate("Administrator", "Welcome123$");
			Assert.IsTrue(success, "Authentication 2 Failed");

			//Finally Make sure that an invalid authentication fails
			success = spiraImport.Authenticate("testtest", "wrongwrong");
			Assert.IsFalse(success, "Authentication 3 Should Have Failed");

			//Test an RSS key.
			success = spiraImport.Authenticate("fredbloggs", "{7A05FD06-83C3-4436-B37F-51BCF0060483}");
			Assert.IsTrue(success, "Authentication 4 Failed");
			success = spiraImport.Authenticate("fredbloggs", "{7A44FD06-83C3-4436-B37F-51BCF0060483}");
			Assert.IsFalse(success, "Authentication 5 Should Have Failed");
			success = spiraImport.Authenticate("donnaharkness", "{7A44FD06-83C3-4436-B37F-51BCF0060483}");
			Assert.IsFalse(success, "Authentication 6 Should Have Failed");
		}

		/// <summary>
		/// Verifies that you can retrieve the list of projects for different users
		/// </summary>
		[
		Test,
		SpiraTestCase(18)
		]
		public void _02_RetrieveProjectList()
		{
			//Download the project list for a normal user with single-case password and username
			spiraImport.Authenticate("fredbloggs", "PleaseChange");
			SpiraImport152.ProjectData projectDataSet = spiraImport.RetrieveProjectList();

			//Check the number of projects
			Assert.AreEqual(7, projectDataSet.Project.Length, "Project List 1");

            //Check some of the data
            Assert.AreEqual("Company Website", projectDataSet.Project[0].Name);
			Assert.AreEqual("Customer Relationship Management (CRM)", projectDataSet.Project[1].Name);
			Assert.AreEqual("ERP: Financials", projectDataSet.Project[2].Name);

			//Download the project list for an Admin user with mixed-case password and username
			spiraImport.Authenticate("Administrator", "Welcome123$");
			projectDataSet = spiraImport.RetrieveProjectList();

			//Check the number of projects
			Assert.AreEqual(7, projectDataSet.Project.Length, "Project List 2");

			//Check some of the data
			Assert.AreEqual("Company Website", projectDataSet.Project[0].Name);
			Assert.AreEqual("Customer Relationship Management (CRM)", projectDataSet.Project[1].Name);
			Assert.AreEqual("ERP: Financials", projectDataSet.Project[2].Name);


			//Make sure that you can't get project list if not authenticated
			spiraImport.Disconnect();
			bool errorCaught = false;
			try
			{
				projectDataSet = spiraImport.RetrieveProjectList();
			}
			catch (SoapException exception)
			{
				//ASP.NET 2.0 wraps our message in other stuff
				if (exception.Message.IndexOf("Session Not Authenticated") > -1)
				{
					errorCaught = true;
				}
				else
				{
					Console.WriteLine(exception.Message);
				}
			}
			Assert.IsTrue(errorCaught, "Should not retrieve unless authenticated");

			//Make sure that you can't get project list if authentication failed
			spiraImport.Authenticate("wrong", "wrong");
			errorCaught = false;
			try
			{
				projectDataSet = spiraImport.RetrieveProjectList();
			}
			catch (SoapException exception)
			{
				//ASP.NET 2.0 wraps our message in other stuff
				if (exception.Message.IndexOf("Session Not Authenticated") > -1)
				{
					errorCaught = true;
				}
				else
				{
					Console.WriteLine(exception.Message);
				}
			}
			Assert.IsTrue(errorCaught, "Should not retrieve unless authenticated");
		}

		/// <summary>
		/// Verifies that you can create a new project for imported artifacts
		/// </summary>
		[
		Test,
		SpiraTestCase(145)
		]
		public void _03_CreateProject()
		{
			//Create a new project into which we will import various other artifacts
			spiraImport.Authenticate("Administrator", "Welcome123$");
			newProjectId = spiraImport.CreateProject("Imported Project", "This project was imported by the Importer Unit Test", "www.tempuri.org");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(newProjectId).ProjectTemplateId;

			//Now lets test logging in to another project that we're a member of
			bool success = spiraImport.ConnectToProject(1);
			Assert.IsTrue(success, "Authorization 1 Failed");

			//Finally lets test connecting to the new project
			success = spiraImport.ConnectToProject(newProjectId);
			Assert.IsTrue(success, "Authorization 2 Failed");
			spiraImport.Disconnect();
		}

		/// <summary>
		/// Verifies that you can import users to a particular project
		/// </summary>
		[
		Test,
		SpiraTestCase(147)
		]
		public void _04_ImportUsers()
		{
			//First lets authenticate and connect to the project
			spiraImport.Authenticate("Administrator", "Welcome123$");
			spiraImport.ConnectToProject(newProjectId);

			//Now lets add a couple of users to the project
			userId1 = spiraImport.AddUser("Adam", "", "Ant", "aant", "aant@antmusic.com", "aant123456789", false, true, InternalRoutines.PROJECT_ROLE_PROJECT_OWNER);
			userId2 = spiraImport.AddUser("Martha", "T", "Muffin", "mmuffin", "mmuffin@echobeach.biz", "echobeach123456789", true, true, InternalRoutines.PROJECT_ROLE_TESTER);

			//Verify that they inserted correctly
			Business.UserManager userManager = new Business.UserManager();
			User user = userManager.GetUserById(userId1);
			Assert.AreEqual("Adam", user.Profile.FirstName);
			Assert.AreEqual("Ant", user.Profile.LastName);
			Assert.AreEqual("aant", user.UserName);
			Assert.AreEqual("aant@antmusic.com", user.EmailAddress);

			user = userManager.GetUserById(userId2);
			Assert.AreEqual("Martha", user.Profile.FirstName);
			Assert.AreEqual("Muffin", user.Profile.LastName);
			Assert.AreEqual("mmuffin", user.UserName);
			Assert.AreEqual("mmuffin@echobeach.biz", user.EmailAddress);

			//Now verify their roles
			Business.ProjectManager projectManager = new Business.ProjectManager();
			ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(newProjectId, userId1);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_PROJECT_OWNER, projectUser.ProjectRoleId);

			projectUser = projectManager.RetrieveUserMembershipById(newProjectId, userId2);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_TESTER, projectUser.ProjectRoleId);
			spiraImport.Disconnect();
		}

		/// <summary>
		/// Verifies that you can import new requirements into the system
		/// </summary>
		[
		Test,
		SpiraTestCase(148)
		]
		public void _05_ImportRequirements()
		{
			int rq_importanceCriticalId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 1).ImportanceId;
			int rq_importanceHighId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 2).ImportanceId;
			int rq_importanceMediumId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 3).ImportanceId;

			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Lets add a nested tree of requirements
			requirementId1 = spiraImport.AddRequirement(-1, 1, -1, "Functionality Area", "", 0, -1);
			requirementId2 = spiraImport.AddRequirement(-1, 2, rq_importanceCriticalId, "Requirement 1", "Requirement Description 1", 1, userId1);
			requirementId3 = spiraImport.AddRequirement(-1, 1, rq_importanceHighId, "Requirement 2", "Requirement Description 2", 0, userId1);
			requirementId4 = spiraImport.AddRequirement(-1, 4, rq_importanceHighId, "Requirement 3", "Requirement Description 3", 1, userId2);
			requirementId5 = spiraImport.AddRequirement(-1, 3, rq_importanceMediumId, "Requirement 4", "", -1, userId2);

			//Now verify that the import was successful
			Business.RequirementManager requirementManager = new Business.RequirementManager();

			//First retrieve the parent requirement
			RequirementView requirement = requirementManager.RetrieveById(UserManager.UserInternal, newProjectId, requirementId1);
			Assert.AreEqual("Functionality Area", requirement.Name);
			Assert.AreEqual("AAA", requirement.IndentLevel);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, requirement.RequirementStatusId);
			Assert.AreEqual("Adam Ant", requirement.AuthorName);
			Assert.IsFalse(requirement.ImportanceId.HasValue);

			//Now retrieve a first-level indent requirement
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, newProjectId, requirementId3);
			Assert.AreEqual("Requirement 2", requirement.Name);
			Assert.AreEqual("Requirement Description 2", requirement.Description);
			Assert.AreEqual("AAAAAB", requirement.IndentLevel);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirement.RequirementStatusId);
			Assert.AreEqual("Adam Ant", requirement.AuthorName);
			Assert.AreEqual("2 - High", requirement.ImportanceName);

			//Now retrieve a second-level indent requirement
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, newProjectId, requirementId4);
			Assert.AreEqual("Requirement 3", requirement.Name);
			Assert.AreEqual("Requirement Description 3", requirement.Description);
			Assert.AreEqual("AAAAABAAA", requirement.IndentLevel);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirement.RequirementStatusId);
			Assert.AreEqual("Martha T Muffin", requirement.AuthorName);
			Assert.AreEqual("2 - High", requirement.ImportanceName);

			//Now test that we can insert a requirement using the 'insert under parent' method
			int requirementId6 = spiraImport.AddRequirement2(-1, 3, 1, "Test Child 1", "", requirementId5, userId1);
			//Verify insertion data
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, newProjectId, requirementId6);
			Assert.AreEqual("Test Child 1", requirement.Name);
			Assert.AreEqual("AAAAACAAA", requirement.IndentLevel);
			//Verify that parent became a summary
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, newProjectId, requirementId5);
			Assert.AreEqual("AAAAAC", requirement.IndentLevel);
			Assert.AreEqual(true, requirement.IsSummary);

			//Now test that we can insert a requirement under the same folder as an existing one
			int requirementId7 = spiraImport.AddRequirement2(-1, 3, 1, "Test Child 2", "Test Child Description 2", requirementId5, userId1);
			//Verify insertion data
			requirement = requirementManager.RetrieveById(UserManager.UserInternal, newProjectId, requirementId7);
			Assert.AreEqual("Test Child 2", requirement.Name);
			Assert.AreEqual("AAAAACAAB", requirement.IndentLevel);

			//Finally disconnect from the project
			spiraImport.Disconnect();
		}

		/// <summary>
		/// Verifies that you can import new test folders and test cases into the system
		/// </summary>
		[
		Test,
		SpiraTestCase(149)
		]
		public void _06_ImportTestCases()
		{
			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Lets add two nested test folders with two test cases in each
			testFolderId1 = spiraImport.AddTestFolder("Test Folder A", "", -1, -1, -1, -1);
			testFolderId2 = spiraImport.AddTestFolder("Test Folder B", "", testFolderId1, userId1, userId2, -1);
			testCaseId1 = spiraImport.AddTestCase("Test Case 1", "Test Case Description 1", testFolderId1, -1, -1, true, 1, 30);
			testCaseId2 = spiraImport.AddTestCase("Test Case 2", "Test Case Description 2", testFolderId1, userId1, userId2, true, 2, 25);
			testCaseId3 = spiraImport.AddTestCase("Test Case 3", "Test Case Description 3", testFolderId2, userId2, userId1, true, -1, -1);
			testCaseId4 = spiraImport.AddTestCase("Test Case 4", "", testFolderId2, userId2, userId2, true, -1, -1);

			//Now verify that the import was successful
			TestCaseManager testCaseManager = new TestCaseManager();

			//First retrieve the top-level folder
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId1);
			Assert.AreEqual("Test Folder A", testCaseFolder.Name);
			Assert.AreEqual(4, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.IsNull(testCaseFolder.ParentTestCaseFolderId);

			//Now retrieve the second-level folder
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId2);
			Assert.AreEqual("Test Folder B", testCaseFolder.Name);
			Assert.AreEqual(2, testCaseFolder.CountNotRun);
			Assert.AreEqual(testFolderId1, testCaseFolder.ParentTestCaseFolderId);

			//Now retrieve a test case
			TestCaseView testCase = testCaseManager.RetrieveById(newProjectId, testCaseId4);
			Assert.AreEqual("Test Case 4", testCase.Name);
			Assert.IsTrue(testCase.Description.IsNull());
			Assert.AreEqual(testFolderId2, testCase.TestCaseFolderId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);
			Assert.AreEqual("Martha T Muffin", testCase.AuthorName);
			Assert.AreEqual("Martha T Muffin", testCase.OwnerName);
			Assert.IsTrue(testCase.TestCasePriorityName.IsNull());
			Assert.IsTrue(testCase.EstimatedDuration.IsNull());

			//Now we need to add some parameters to test case 2
			spiraImport.AddTestCaseParameter(testCaseId2, "param1", "nothing");
			spiraImport.AddTestCaseParameter(testCaseId2, "param2", "");

			//Finally disconnect from the project
			spiraImport.Disconnect();
		}

		/// <summary>
		/// Verifies that you can import new test steps into the previously created test cases
		/// </summary>
		[
		Test,
		SpiraTestCase(150)
		]
		public void _07_ImportTestSteps()
		{
			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Lets add two test steps to two of the test cases
			testStepId1 = spiraImport.AddTestStep(testCaseId3, 1, "Test Step 1", "It should work", "test 123");
			testStepId2 = spiraImport.AddTestStep(testCaseId3, 2, "Test Step 2", "It should work", "");
			testStepId3 = spiraImport.AddTestStep(testCaseId4, 1, "Test Step 4", "It should work", "test 123");
			testStepId4 = spiraImport.AddTestStep(testCaseId4, 1, "Test Step 3", "It should work", "");

			//Now verify that the import was successful
			TestCaseManager testCaseManager = new TestCaseManager();

			//Retrieve the first test case with its steps
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(newProjectId, testCaseId3);
			Assert.AreEqual(2, testCase.TestSteps.Count, "Test Step Count 1");
			Assert.AreEqual("Test Step 1", testCase.TestSteps[0].Description);
			Assert.AreEqual(1, testCase.TestSteps[0].Position, "Position 1");
			Assert.AreEqual("Test Step 2", testCase.TestSteps[1].Description);
			Assert.AreEqual(2, testCase.TestSteps[1].Position, "Position 2");

			//Retrieve the second test case with its steps
			testCase = testCaseManager.RetrieveByIdWithSteps(newProjectId, testCaseId4);
			Assert.AreEqual(2, testCase.TestSteps.Count, "Test Step Count 2");
			Assert.AreEqual("Test Step 3", testCase.TestSteps[0].Description);
			Assert.AreEqual(1, testCase.TestSteps[0].Position, "Position 3");
			Assert.AreEqual("Test Step 4", testCase.TestSteps[1].Description);
			Assert.AreEqual(2, testCase.TestSteps[1].Position, "Position 4");

			//Finally disconnect from the project
			spiraImport.Disconnect();
		}

		/// <summary>
		/// Verifies that you can import requirements test coverage into the project
		/// </summary>
		[
		Test,
		SpiraTestCase(151)
		]
		public void _08_ImportCoverage()
		{
			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Next lets add some coverage entries for a requirement
			spiraImport.AddRequirementTestCoverage(requirementId2, testCaseId1);
			spiraImport.AddRequirementTestCoverage(requirementId2, testCaseId2);
			spiraImport.AddRequirementTestCoverage(requirementId2, testCaseId3);

			//Try adding a duplicate entry - should fail quietly
			spiraImport.AddRequirementTestCoverage(requirementId2, testCaseId1);

			//Now verify the coverage data
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCase> mappedTestCases = testCaseManager.RetrieveCoveredByRequirementId(newProjectId, requirementId2);
			Assert.AreEqual(3, mappedTestCases.Count);
			Assert.AreEqual(testCaseId1, mappedTestCases[0].TestCaseId);
			Assert.AreEqual(testCaseId2, mappedTestCases[1].TestCaseId);
			Assert.AreEqual(testCaseId3, mappedTestCases[2].TestCaseId);

			//Finally remove the coverage
			spiraImport.RemoveRequirementTestCoverage(requirementId2, testCaseId1);
			spiraImport.RemoveRequirementTestCoverage(requirementId2, testCaseId2);
			spiraImport.RemoveRequirementTestCoverage(requirementId2, testCaseId3);

			//Now verify the coverage data
			mappedTestCases = testCaseManager.RetrieveCoveredByRequirementId(newProjectId, requirementId2);
			Assert.AreEqual(0, mappedTestCases.Count);
		}

		/// <summary>
		/// Tests that we can record automated test results using the v1.x API
		/// </summary>
		[
		Test,
		SpiraTestCase(243)
		]
		public void _09_TestAutomatedTestWebService()
		{
			//This is similar to the previous set of tests, except that it uses the web services interface

			//Instantiate the web-service proxy class and set the URL from the .config file
			SpiraTestExecute12.TestExecute spiraTestExecute = new SpiraTestExecute12.TestExecute();
			spiraTestExecute.Url = Properties.Settings.Default.WebServiceUrl + "TestExecute.asmx";

			//Create a new cookie container to hold the session handle
			CookieContainer cookieContainer = new CookieContainer();
			spiraTestExecute.CookieContainer = cookieContainer;

			//Next authenticate with the web service and connect to the project
			spiraTestExecute.Authenticate("aant", "aant123456789");
			spiraTestExecute.ConnectToProject(newProjectId);

			//This tests that we can create a successful automated test run with no release information
			int testRunId3 = spiraTestExecute.RecordTestRun(-1, testCaseId1, -1, DateTime.Now, DateTime.Now.AddMinutes(2), (int)TestCase.ExecutionStatusEnum.Passed, "TestSuite", "02_Test_Method", -1, "", "");

			//Now retrieve the test run to check that it saved correctly
			TestRunManager testRunManager = new TestRunManager();
			TestRun testRun = testRunManager.RetrieveByIdWithSteps(testRunId3);
			TestRunView testRunView = testRunManager.RetrieveById(testRunId3);

			//Verify Counts (no steps for automated runs)
			Assert.IsNotNull(testRun);
			Assert.AreEqual(0, testRun.TestRunSteps.Count);

			//Now verify the data, have to use the view for some of the fields
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testRun.ExecutionStatusId);
			Assert.AreEqual("Passed", testRunView.ExecutionStatusName);
			Assert.IsTrue(testRun.ReleaseId.IsNull());
			Assert.AreEqual(TestRun.TestRunTypeEnum.Automated.GetHashCode(), testRun.TestRunTypeId);
			Assert.AreEqual("Automated", testRunView.TestRunTypeName);
			Assert.AreEqual("TestSuite", testRun.RunnerName);
			Assert.AreEqual("02_Test_Method", testRun.RunnerTestName);
			Assert.IsTrue(testRun.RunnerAssertCount.IsNull());
			Assert.IsTrue(testRun.RunnerMessage.IsNull());
			Assert.IsTrue(testRun.RunnerStackTrace.IsNull());

			//Now verify that the test case itself updated
			TestCaseManager testCaseManager = new TestCaseManager();
			TestCaseView testCase = testCaseManager.RetrieveById(newProjectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, testCase.ExecutionStatusId);
			Assert.AreEqual("Passed", testCase.ExecutionStatusName);

			//This tests that we can create a failed automated test run with release information
			int testRunId4 = spiraTestExecute.RecordTestRun(userId1, testCaseId1, 1, DateTime.Now, DateTime.Now.AddMinutes(2), (int)TestCase.ExecutionStatusEnum.Failed, "TestSuite", "02_Test_Method", 5, "Expected 1, Found 0", "Error Stack Trace........");

			//Now retrieve the test run to check that it saved correctly - using the web service
			testRun = testRunManager.RetrieveByIdWithSteps(testRunId4);
			testRunView = testRunManager.RetrieveById(testRunId4);

			//Verify Counts (no steps for automated runs)
			Assert.IsNotNull(testRun);
			Assert.AreEqual(0, testRun.TestRunSteps.Count);

			//Now verify the data have to view for some of them)
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testRun.ExecutionStatusId);
			Assert.AreEqual("Failed", testRunView.ExecutionStatusName);
			Assert.AreEqual(1, testRun.ReleaseId);
			Assert.AreEqual(TestRun.TestRunTypeEnum.Automated.GetHashCode(), testRun.TestRunTypeId);
			Assert.AreEqual("Automated", testRunView.TestRunTypeName);
			Assert.AreEqual("TestSuite", testRun.RunnerName);
			Assert.AreEqual("02_Test_Method", testRun.RunnerTestName);
			Assert.AreEqual(5, testRun.RunnerAssertCount);
			Assert.AreEqual("Expected 1, Found 0", testRun.RunnerMessage);
			Assert.AreEqual("Error Stack Trace........", testRun.RunnerStackTrace);

			//Now verify that the test case itself updated
			testCase = testCaseManager.RetrieveById(newProjectId, testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, testCase.ExecutionStatusId);
			Assert.AreEqual("Failed", testCase.ExecutionStatusName);

			//This tests that we can create a failed automated test run using the sessionless method overload
			spiraTestExecute.CookieContainer = null;
			int testRunId5 = spiraTestExecute.RecordTestRun2("aant", "aant123456789", newProjectId, userId1, testCaseId1, 1, DateTime.Now, DateTime.Now.AddSeconds(20), (int)TestCase.ExecutionStatusEnum.Failed, "TestSuite", "02_Test_Method", 5, "Expected 1, Found 0", "Error Stack Trace........");

			//Now retrieve the test run to check that it saved correctly - using the web service
			//Need to restore the use of cookies first
			spiraTestExecute.CookieContainer = cookieContainer;
			testRun = testRunManager.RetrieveByIdWithSteps(testRunId5);

			//Verify Counts (no steps for automated runs)
			Assert.IsNotNull(testRun);
			Assert.AreEqual(0, testRun.TestRunSteps.Count);
		}

		/// <summary>
		/// Verifies that you can import incidents into the project
		/// </summary>
		[
		Test,
		SpiraTestCase(153)
		]
		public void _10_ImportIncidents()
		{
			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Now lets add a custom incident type, status, priority and severity (which we'll then use to import with)
			int incidentTypeId = spiraImport.AddIncidentType("Problem");
			int incidentStatusId = spiraImport.AddIncidentStatus("Indeterminate");
			int priorityId = spiraImport.AddIncidentPriority("Not Good", "000000");
			int severityId = spiraImport.AddIncidentSeverity("Difficult", "000000");

			//Lets add a new incident, an open issue and a closed bug (mapped to a test run step)
			incidentId1 = spiraImport.AddIncident(incidentTypeId, -1, -1, incidentStatusId, "New Incident", "This is a test new incident", -1, -1, -1, DateTime.Now, false);
			incidentId2 = spiraImport.AddIncident(incidentTypeId, priorityId, -1, incidentStatusId, "Open Issue", "This is a test open issue", -1, userId1, userId2, DateTime.Now, false);
			incidentId3 = spiraImport.AddIncident(incidentTypeId, priorityId, severityId, incidentStatusId, "Closed Bug", "This is a test closed bug", -1, userId1, userId2, DateTime.Now, true);

			//Next add some custom properties to one of the incidents
			spiraImport.AddCustomProperties(incidentId1, 3, "test value", "", "", "", "", "", "", "", "", "", 1, -1, -1, -1, -1, -1, -1, -1, -1, -1);

			//Finally add some resolutions to the incidents
			spiraImport.AddIncidentResolution(incidentId3, "Resolution 1", DateTime.Now.AddSeconds(-2), userId1);
			spiraImport.AddIncidentResolution(incidentId3, "Resolution 2", DateTime.Now.AddSeconds(2), userId1);

			//Now verify that the import was successful

			//Retrieve the first incident (there are no supported v1.x API methods for this)
			IncidentManager incidentManager = new IncidentManager();
			Incident incident = incidentManager.RetrieveById(incidentId1, true);
			IncidentView incidentView = incidentManager.RetrieveById2(incidentId1);
			Assert.AreEqual(incidentTypeId, incident.IncidentTypeId, "IncidentTypeId");
			Assert.AreEqual(incidentStatusId, incident.IncidentStatusId, "");
			Assert.AreEqual("New Incident", incident.Name, "Name");
			Assert.IsFalse(incident.ClosedDate.HasValue, "IsClosedDateNull");
			Assert.AreEqual("Adam Ant", incidentView.OpenerName, "OpenerName");
			Assert.IsTrue(String.IsNullOrEmpty(incidentView.OwnerName), "IsOwnerNameNull");
			Assert.AreEqual(0, incident.Resolutions.Count);

			//Retrieve the second incident
			incident = incidentManager.RetrieveById(incidentId2, true);
			incidentView = incidentManager.RetrieveById2(incidentId2);
			Assert.AreEqual(incidentTypeId, incident.IncidentTypeId, "IncidentTypeId");
			Assert.AreEqual(incidentStatusId, incident.IncidentStatusId, "IncidentStatusId");
			Assert.AreEqual("Open Issue", incident.Name, "Name");
			Assert.IsFalse(incident.ClosedDate.HasValue, "IsClosedDateNull");
			Assert.AreEqual("Adam Ant", incidentView.OpenerName, "OpenerName");
			Assert.AreEqual(incidentView.OwnerName, "Martha T Muffin", "OwnerName");
			Assert.AreEqual(0, incident.Resolutions.Count);

			//Retrieve the third incident
			incident = incidentManager.RetrieveById(incidentId3, true);
			incidentView = incidentManager.RetrieveById2(incidentId3);
			Assert.AreEqual(incidentTypeId, incident.IncidentTypeId, "IncidentTypeId");
			Assert.AreEqual(incidentStatusId, incident.IncidentStatusId, "IncidentStatusId");
			Assert.AreEqual(priorityId, incident.PriorityId, "PriorityId");
			Assert.AreEqual(severityId, incident.SeverityId, "SeverityId");
			Assert.AreEqual("Closed Bug", incident.Name, "Name");
			Assert.IsTrue(incident.ClosedDate.HasValue, "IsClosedDateNull");
			Assert.AreEqual("Adam Ant", incidentView.OpenerName, "OpenerName");
			Assert.AreEqual("Martha T Muffin", incidentView.OwnerName, "OwnerName");
			Assert.AreEqual(2, incident.Resolutions.Count);
			List<IncidentResolution> incidentResolutions = incident.Resolutions.OrderBy(r => r.CreationDate).ToList();
			Assert.AreEqual("Resolution 1", incidentResolutions[0].Resolution);
			Assert.AreEqual("Resolution 2", incidentResolutions[1].Resolution);

			//Finally disconnect from the project
			spiraImport.Disconnect();
		}

		/// <summary>
		/// Verifies that you can import test sets into the project
		/// </summary>
		[
		Test,
		SpiraTestCase(353)
		]
		public void _11_ImportTestSets()
		{
			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Lets try importing a test set folder and put some test sets in it
			testSetFolderId1 = spiraImport.AddTestSetFolder(-1, userId1, "Test Set Folder", "");

			//First lets try inserting a new test set into this folder
			testSetId1 = spiraImport.AddTestSet(testSetFolderId1, -1, userId1, -1, (int)TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", "", DateTime.Now, true);

			//Verify that it inserted correctly
			TestSetManager testSetManager = new TestSetManager();
			TestSetView testSet = testSetManager.RetrieveById(newProjectId, testSetId1);
			Assert.AreEqual(testSetId1, testSet.TestSetId, "TestSetId");
			Assert.AreEqual(testSetFolderId1, testSet.TestSetFolderId, "TestSetFolderId");
			Assert.AreEqual("Adam Ant", testSet.CreatorName, "CreatorName");
			Assert.AreEqual("Test Set 1", testSet.Name, "Name");
			Assert.AreEqual("Not Started", testSet.TestSetStatusName, "TestSetStatusName");
			Assert.IsTrue(testSet.Description.IsNull(), "Description");
			Assert.IsTrue(testSet.ReleaseId.IsNull());
			Assert.IsTrue(testSet.OwnerName.IsNull(), "OwnerName");
			Assert.IsTrue(testSet.PlannedDate.IsNull(), "PlannedDate");
			Assert.IsTrue(testSet.ExecutionDate.IsNull(), "ExecutionDate");

			//Next lets try inserting a new test set after it in the folder
			testSetId2 = spiraImport.AddTestSet(testSetFolderId1, -1, userId1, userId2, (int)TestSet.TestSetStatusEnum.InProgress, "Test Set 2", "Test Set 2 Description", DateTime.Now.AddDays(1), false);
			//Verify that it inserted correctly
			testSet = testSetManager.RetrieveById(newProjectId, testSetId2);
			Assert.AreEqual(testSetId2, testSet.TestSetId, "TestSetId");
			Assert.AreEqual(testSetFolderId1, testSet.TestSetFolderId, "TestSetFolderId");
			Assert.AreEqual("Adam Ant", testSet.CreatorName, "CreatorName");
			Assert.AreEqual("Test Set 2", testSet.Name, "Name");
			Assert.AreEqual("In Progress", testSet.TestSetStatusName, "TestSetStatusName");
			Assert.AreEqual("Test Set 2 Description", testSet.Description, "Description");
			Assert.IsTrue(testSet.ReleaseId.IsNull());
			Assert.AreEqual("Martha T Muffin", testSet.OwnerName, "OwnerName");
			Assert.IsFalse(testSet.PlannedDate.IsNull(), "PlannedDate");
			Assert.IsTrue(testSet.ExecutionDate.IsNull(), "ExecutionDate");

			//Now test that we can add some test cases to one of the test set
			spiraImport.AddTestSetTestCaseMapping(testSetId1, testCaseId1);
			spiraImport.AddTestSetTestCaseMapping(testSetId1, testCaseId2);

			//Now verify the data
			List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId1);
			Assert.AreEqual(2, testSetTestCases.Count);
			Assert.AreEqual("Test Case 1", testSetTestCases[0].Name);
			Assert.AreEqual("Test Case 2", testSetTestCases[1].Name);
		}

		/// <summary>
		/// Verifies that you can import attachments to project artifacts
		/// </summary>
		[
		Test,
		SpiraTestCase(354)
		]
		public void _12_ImportAttachments()
		{
			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Lets try adding and attachment to a test case
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			int attachmentId = spiraImport.AddAttachment("test_data.xls", "Sample Test Case Attachment", userId1, attachmentData, testCaseId1, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase);

			//Now lets get the attachment meta-data and verify
			Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(newProjectId, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.xls", attachments[0].Filename);
			Assert.AreEqual("Sample Test Case Attachment", attachments[0].Description);
			Assert.AreEqual("Adam Ant", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);
		}

		/// <summary>
		/// Verifies that you can update a requirement entry
		/// </summary>
		[
		Test,
		SpiraTestCase(154)
		]
		public void _13_UpdateRequirements()
		{
			int rq_importanceCriticalId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 1).ImportanceId;

			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Update a requirement
			spiraImport.UpdateRequirement(requirementId3, (int)Requirement.RequirementStatusEnum.InProgress, rq_importanceCriticalId, "Modified Requirement 2", userId2);

			//Verify the changes - status won't change because it's a summary requirement and the child requirements override
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			RequirementView requirement = requirementManager.RetrieveById(UserManager.UserInternal, newProjectId, requirementId3);
			Assert.AreEqual("Modified Requirement 2", requirement.Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirement.RequirementStatusId);
			Assert.AreEqual("Martha T Muffin", requirement.AuthorName);
			Assert.AreEqual(rq_importanceCriticalId, requirement.ImportanceId);
		}

		/// <summary>
		/// Verifies that you can update a test case entry
		/// </summary>
		[
		Test,
		SpiraTestCase(155)
		]
		public void _14_UpdateTestCases()
		{
			//First lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Update a test case
			spiraImport.UpdateTestCase(testCaseId4, "Modified Test Case 4", userId1, -1);

			//Verify the changes
			TestCaseManager testCaseManager = new TestCaseManager();
			TestCaseView testCase = testCaseManager.RetrieveById(newProjectId, testCaseId4);
			Assert.AreEqual("Modified Test Case 4", testCase.Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);
			Assert.AreEqual("Adam Ant", testCase.AuthorName);
			Assert.IsTrue(testCase.OwnerName.IsNull());
		}

		/// <summary>
		/// Tests that we can import tasks into SpiraTeam/Plan
		/// </summary>
		[
		Test,
		SpiraTestCase(387)
		]
		public void _18_ImportTasks()
		{
			//For tasks we need to first create one requirement and one release
			RequirementManager requirementManager = new RequirementManager();
			int requirementId = requirementManager.Insert(userId1, newProjectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, userId1, null, null, "Requirement 1", null, 300, 1);
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(userId1, newProjectId, userId1, "Release 1", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddDays(10), 1, 0, null, false);

			//Next lets authenticate and connect to the project
			spiraImport.Authenticate("aant", "aant123456789");
			spiraImport.ConnectToProject(newProjectId);

			//Now lets add a task that has all values set
			TaskManager taskManager = new TaskManager();
			int tk_priorityMediumId = taskManager.TaskPriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 3).TaskPriorityId;
			int taskId1 = spiraImport.AddTask("New Task 1", "Task 1 Description", (int)Task.TaskStatusEnum.InProgress, tk_priorityMediumId, requirementId, releaseId, userId1, DateTime.Now, DateTime.Now.AddDays(5), 25, 100, 120);

			//Verify the data
			TaskView taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.AreEqual("New Task 1", taskView.Name);
			Assert.AreEqual("Task 1 Description", taskView.Description);
			Assert.AreEqual("In Progress", taskView.TaskStatusName);
			Assert.AreEqual("3 - Medium", taskView.TaskPriorityName);
			Assert.AreEqual("1.0", taskView.ReleaseVersionNumber);
			Assert.AreEqual(requirementId, taskView.RequirementId);
			Assert.IsTrue(taskView.StartDate.HasValue);
			Assert.IsTrue(taskView.EndDate.HasValue);
			Assert.AreEqual(25, taskView.CompletionPercent);
			Assert.AreEqual(100, taskView.EstimatedEffort);
			Assert.AreEqual(120, taskView.ActualEffort);

			//Now lets add a task that has some values null
			int taskId2 = spiraImport.AddTask("New Task 2", "", (int)Task.TaskStatusEnum.NotStarted, -1, -1, -1, userId1, null, null, 0, -1, -1);

			//Verify the data
			taskView = taskManager.TaskView_RetrieveById(taskId2);
			Assert.AreEqual("New Task 2", taskView.Name);
			Assert.IsTrue(String.IsNullOrEmpty(taskView.Description));
			Assert.AreEqual("Not Started", taskView.TaskStatusName);
			Assert.IsTrue(String.IsNullOrEmpty(taskView.TaskPriorityName));
			Assert.IsTrue(String.IsNullOrEmpty(taskView.ReleaseVersionNumber));
			Assert.IsFalse(taskView.RequirementId.HasValue);
			Assert.IsFalse(taskView.StartDate.HasValue);
			Assert.IsFalse(taskView.EndDate.HasValue);
			Assert.AreEqual(0, taskView.CompletionPercent);
			Assert.IsFalse(taskView.EstimatedEffort.HasValue);
			Assert.IsFalse(taskView.ActualEffort.HasValue);

			//Finally delete the tasks, requirements and releases
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, userId1);
			taskManager.MarkAsDeleted(PROJECT_ID, taskId2, userId1);
			requirementManager.MarkAsDeleted(userId1, newProjectId, requirementId);
			releaseManager.MarkAsDeleted(userId1, newProjectId, releaseId);
		}

		/// <summary>
		/// Tests that when we use the old static ID values for the following artifact fields, the API handles them correctly:
		/// - Requirement
		///   - Importance
		/// - Task
		///   - Priority
		/// - Test Case
		///   - Priority
		/// </summary>
		/// <remarks>
		/// If there is no match, the default value should be used. Instead of an ID from the incorrect project
		/// </remarks>
		[Test]
		[SpiraTestCase(1861)]
		public void _19_CheckCompatibilityWithOldStaticIDs()
		{
			//First lets authenticate and create a new project
			spiraImport.Authenticate("administrator", "Welcome123$");
			projectId3 = spiraImport.CreateProject("API Compatibility Test Project", null, null);
			spiraImport.ConnectToProject(projectId3);

			//Get the template associated with the project
			projectTemplateId3 = new TemplateManager().RetrieveForProject(projectId3).ProjectTemplateId;

			//Lets add a new requirement with the old static IDs
			int requirementId = spiraImport.AddRequirement(-1, /*Requested*/1, /*High*/2, "Test Requirement", null, 0, 1);

			//Verify the data
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			DataModel.RequirementView requirement = requirementManager.RetrieveById(User.UserInternal, null, requirementId);
			Assert.AreNotEqual(1, requirement.ImportanceId);

			//Lets add a new requirement using the other API method
			requirementId = spiraImport.AddRequirement2(-1, /*Requested*/1, /*High*/2, "Test Requirement 2", null, -1, 1);

			//Verify the data
			requirement = requirementManager.RetrieveById(User.UserInternal, null, requirementId);
			Assert.AreNotEqual(1, requirement.ImportanceId);

			//Now try and update it back and verify it remains with the correct ID
			spiraImport.UpdateRequirement(requirementId, 1, /*High*/2, "Test Requirement 2", 1);

			//Verify the data
			requirement = requirementManager.RetrieveById(User.UserInternal, null, requirementId);
			Assert.AreNotEqual(1, requirement.ImportanceId);

			//Now create a task with the old static IDs
			int taskId = spiraImport.AddTask("Sample Task", null, 1, /*Medium*/3, -1, -1, 1, null, null, 0, 0, 0);

			//Verify the data
			DataModel.Task task = new TaskManager().RetrieveById(taskId);
			Assert.AreNotEqual(3, task.TaskPriorityId);

			//Now create a test case with the old static IDs
			int testCaseId = spiraImport.AddTestCase("Sample Test Case", null, -1, 1, -1, true, /*Medium*/3, 0);

			//Verify the data
			DataModel.TestCaseView testCase = new TestCaseManager().RetrieveById(null, testCaseId);
			Assert.AreNotEqual(3, testCase.TestCasePriorityId);
		}
	}
}
