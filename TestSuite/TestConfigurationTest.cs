using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using NUnit.Framework;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the TestConfiguration business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class TestConfigurationTest
	{
		private const int USER_ID_SYSTEM_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;

		private static TestConfigurationManager testConfigurationManager;

		private static int projectId;
		private static int projectTemplateId;
		private static int customList_OS_id;
		private static int customList_Browser_id;
		private static int testCaseId1;
		private static int testCaseId2;
		private static int testCaseParameter_OS_id1;
		private static int testCaseParameter_Browser_id1;
		private static int testCaseParameter_OS_id2;
		private static int testCaseParameter_Browser_id2;

		[TestFixtureSetUp]
		public void Init()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Create a new project for our testing and instantiate the business manager class being tested
			testConfigurationManager = new TestConfigurationManager();
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("TestConfigurationTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//We need to add some custom lists
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			customList_OS_id = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Operating Systems").CustomPropertyListId;
			customList_Browser_id = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Web Browsers").CustomPropertyListId;
			customPropertyManager.CustomPropertyList_AddValue(customList_OS_id, "Windows");
			customPropertyManager.CustomPropertyList_AddValue(customList_OS_id, "MacOS X");
			customPropertyManager.CustomPropertyList_AddValue(customList_Browser_id, "IE");
			customPropertyManager.CustomPropertyList_AddValue(customList_Browser_id, "Safari");
			customPropertyManager.CustomPropertyList_AddValue(customList_Browser_id, "Firefox");
			customPropertyManager.CustomPropertyList_AddValue(customList_Browser_id, "Chrome");

			//We need to add some test cases and parameters
			TestCaseManager testCaseManager = new TestCaseManager();
			int tc_typeRegressionId = testCaseManager.TestCaseType_Retrieve(projectTemplateId).FirstOrDefault(t => t.Name == "Regression").TestCaseTypeId;
			int tc_priorityHighId = testCaseManager.TestCasePriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 2).TestCasePriorityId;
			testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, tc_typeRegressionId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityHighId, null, null, null, null);
			testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 2", null, tc_typeRegressionId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityHighId, null, null, null, null);
			testCaseParameter_OS_id1 = testCaseManager.InsertParameter(projectId, testCaseId1, "os", null, 1);
			testCaseParameter_Browser_id1 = testCaseManager.InsertParameter(projectId, testCaseId1, "browser", null, 1);
			testCaseParameter_OS_id2 = testCaseManager.InsertParameter(projectId, testCaseId2, "os", null, 1);
			testCaseParameter_Browser_id2 = testCaseManager.InsertParameter(projectId, testCaseId2, "browser", null, 1);

			//We need to add some test steps that use these parameters as well
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, null, "Open the ${browser} browser on the ${os} computer.", "It works", null);
			testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId2, null, "Choose ${browser} running on ${os}.", "It launches", null);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYSTEM_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId);
		}

		/// <summary>
		/// Tests that we can created, edit, delete and retrieve test configuration sets
		/// </summary>
		[Test]
		[SpiraTestCase(1615)]
		public void _01_ManageTestConfigurationSets()
		{
			//First lets create two new test configuration sets
			int testConfigurationSetId1 = testConfigurationManager.InsertSet(projectId, "Sample Set 1", null, true, USER_ID_FRED_BLOGGS);
			int testConfigurationSetId2 = testConfigurationManager.InsertSet(projectId, "Sample Set 2", "This is a sample set", true, USER_ID_FRED_BLOGGS);

			//Verify that we can retrieve a set
			TestConfigurationSet testConfigurationSet = testConfigurationManager.RetrieveSetById(testConfigurationSetId2);
			Assert.AreEqual("Sample Set 2", testConfigurationSet.Name);
			Assert.AreEqual("This is a sample set", testConfigurationSet.Description);
			Assert.AreEqual(true, testConfigurationSet.IsActive);
			Assert.AreEqual(false, testConfigurationSet.IsDeleted);

			//Verify that we can make changes
			testConfigurationSet.StartTracking();
			testConfigurationSet.Name = "Sample Set 2a";
			testConfigurationSet.Description = null;
			testConfigurationSet.IsActive = false;
			testConfigurationManager.UpdateSet(testConfigurationSet, USER_ID_FRED_BLOGGS);

			//Verify
			testConfigurationSet = testConfigurationManager.RetrieveSetById(testConfigurationSetId2);
			Assert.AreEqual("Sample Set 2a", testConfigurationSet.Name);
			Assert.AreEqual(null, testConfigurationSet.Description);
			Assert.AreEqual(false, testConfigurationSet.IsActive);
			Assert.AreEqual(false, testConfigurationSet.IsDeleted);

			//Verify that we can retrieve a list of all the active sets in a project
			List<TestConfigurationSet> testConfigurationSets = testConfigurationManager.RetrieveSets(projectId);
			Assert.AreEqual(1, testConfigurationSets.Count);

			//Now retrieve all the sets in the project, sorted by name
			testConfigurationSets = testConfigurationManager.RetrieveSets(projectId, "Name", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, testConfigurationSets.Count);
			Assert.AreEqual("Sample Set 1", testConfigurationSets[0].Name);
			Assert.AreEqual("Sample Set 2a", testConfigurationSets[1].Name);

			//Now retrieve a filtered list
			Hashtable filters = new Hashtable();
			filters.Add("Name", "Set 1");
			testConfigurationSets = testConfigurationManager.RetrieveSets(projectId, "Name", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, testConfigurationSets.Count);
			Assert.AreEqual("Sample Set 1", testConfigurationSets[0].Name);

			//Test that we can count the list also
			int count = testConfigurationManager.CountSets(projectId, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, count);

			//Verify that we can delete a set
			testConfigurationManager.MarkSetAsDeleted(projectId, testConfigurationSetId2, USER_ID_FRED_BLOGGS);

			//Verify that it is actually deleted
			testConfigurationSet = testConfigurationManager.RetrieveSetById(testConfigurationSetId2);
			Assert.IsNull(testConfigurationSet);

			//Get the list of all deleted sets
			testConfigurationSets = testConfigurationManager.RetrieveDeletedSets(projectId);
			Assert.AreEqual(1, testConfigurationSets.Count);
			Assert.AreEqual("Sample Set 2a", testConfigurationSets[0].Name);

			//Undelete
			testConfigurationManager.UnDeleteSet(testConfigurationSetId2, USER_ID_FRED_BLOGGS);

			//Verify that it is undeleted
			testConfigurationSet = testConfigurationManager.RetrieveSetById(testConfigurationSetId2);
			Assert.IsNotNull(testConfigurationSet);
			Assert.AreEqual("Sample Set 2a", testConfigurationSet.Name);
		}

		/// <summary>
		/// Tests that we can populate and edit the test configurations in a set
		/// </summary>
		[Test]
		[SpiraTestCase(1616)]
		public void _02_PopulateTestConfigurations()
		{
			//First lets create a new test configuration set
			int testConfigurationSetId = testConfigurationManager.InsertSet(projectId, "Sample Set 3", null, true, USER_ID_FRED_BLOGGS);

			//Verify that it has no configuration entries
			List<TestConfigurationEntry> testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId, testConfigurationSetId);
			Assert.AreEqual(0, testConfigurationEntries.Count);

			//Now lets populate some entries
			Dictionary<int, int> testParametersDic = new Dictionary<int, int>();
			testParametersDic.Add(testCaseParameter_OS_id1, customList_OS_id);
			testParametersDic.Add(testCaseParameter_Browser_id1, customList_Browser_id);
			testConfigurationManager.PopulateTestConfigurations(projectId, testConfigurationSetId, testParametersDic);

			//Verify that we now have some entries
			testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId, testConfigurationSetId);
			if (testConfigurationEntries != null)
			{
				Assert.AreEqual(16, testConfigurationEntries.Count);
				List<IGrouping<int, TestConfigurationEntry>> testConfigurationsGroupedByConfiguration = testConfigurationEntries.GroupBy(t => t.TestCaseConfigurationId).ToList();
				Assert.AreEqual(8, testConfigurationsGroupedByConfiguration.Count);
				//Row 0
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[0].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Chrome", testConfigurationsGroupedByConfiguration[0].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 1
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[1].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Firefox", testConfigurationsGroupedByConfiguration[1].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 2
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[2].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("IE", testConfigurationsGroupedByConfiguration[2].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 3
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[3].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Safari", testConfigurationsGroupedByConfiguration[3].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 4
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[4].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Chrome", testConfigurationsGroupedByConfiguration[4].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 5
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[5].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Firefox", testConfigurationsGroupedByConfiguration[5].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 6
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[6].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("IE", testConfigurationsGroupedByConfiguration[6].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 7
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[7].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Safari", testConfigurationsGroupedByConfiguration[7].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);

				//Now lets remove the invalid combinations
				List<int> testConfigurationIds = new List<int>();
				testConfigurationIds.Add(testConfigurationsGroupedByConfiguration[2].Key);
				testConfigurationIds.Add(testConfigurationsGroupedByConfiguration[7].Key);
				testConfigurationManager.RemoveTestConfigurationsFromSet(projectId, testConfigurationSetId, testConfigurationIds);

				//Verify the results
				testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId, testConfigurationSetId);
				testConfigurationsGroupedByConfiguration = testConfigurationEntries.GroupBy(t => t.TestCaseConfigurationId).ToList();
				Assert.AreEqual(6, testConfigurationsGroupedByConfiguration.Count);
				//Row 0
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[0].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Chrome", testConfigurationsGroupedByConfiguration[0].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 1
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[1].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Firefox", testConfigurationsGroupedByConfiguration[1].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 2
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[2].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Safari", testConfigurationsGroupedByConfiguration[2].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 3
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[3].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Chrome", testConfigurationsGroupedByConfiguration[3].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 4
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[4].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Firefox", testConfigurationsGroupedByConfiguration[4].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 5
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[5].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("IE", testConfigurationsGroupedByConfiguration[5].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);

				//Verify that we can reorder the entries
				testConfigurationManager.MoveTestConfigurations(testConfigurationSetId, testConfigurationsGroupedByConfiguration[5].Key, testConfigurationsGroupedByConfiguration[0].Key);
				testConfigurationManager.MoveTestConfigurations(testConfigurationSetId, testConfigurationsGroupedByConfiguration[2].Key, null);

				//Verify the new order
				testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId, testConfigurationSetId);
				testConfigurationsGroupedByConfiguration = testConfigurationEntries.GroupBy(t => t.TestCaseConfigurationId).ToList();
				Assert.AreEqual(6, testConfigurationsGroupedByConfiguration.Count);
				//Row 0
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[0].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("IE", testConfigurationsGroupedByConfiguration[0].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 1
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[1].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Chrome", testConfigurationsGroupedByConfiguration[1].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 2
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[2].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Firefox", testConfigurationsGroupedByConfiguration[2].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 3
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[3].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Chrome", testConfigurationsGroupedByConfiguration[3].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 4
				Assert.AreEqual("Windows", testConfigurationsGroupedByConfiguration[4].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Firefox", testConfigurationsGroupedByConfiguration[4].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
				//Row 5
				Assert.AreEqual("MacOS X", testConfigurationsGroupedByConfiguration[5].FirstOrDefault(t => t.ParameterName == "os").ParameterValue);
				Assert.AreEqual("Safari", testConfigurationsGroupedByConfiguration[5].FirstOrDefault(t => t.ParameterName == "browser").ParameterValue);
			}
		}

		/// <summary>
		/// Tests that we can map a test set to a test configuration set and retrieve in both directions
		/// </summary>
		[Test]
		[SpiraTestCase(1617)]
		public void _03_MapTestSetsToConfigurationSets()
		{
			//First lets create a new test configuration set
			int testConfigurationSetId = testConfigurationManager.InsertSet(projectId, "Sample Set 4", null, true, USER_ID_FRED_BLOGGS);

			//Next create new test sets
			TestSetManager testSetManager = new TestSetManager();
			int testSetId1 = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);
			int testSetId2 = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 2", null, null, TestRun.TestRunTypeEnum.Manual, null, null);

			//Now associate them with the configuration set
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId1);
			testSet.StartTracking();
			testSet.TestConfigurationSetId = testConfigurationSetId;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);
			testSet = testSetManager.RetrieveById2(projectId, testSetId2);
			testSet.StartTracking();
			testSet.TestConfigurationSetId = testConfigurationSetId;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Verify the data was updated
			TestSetView testSetView = testSetManager.RetrieveById(projectId, testSetId1);
			Assert.AreEqual("Sample Set 4", testSetView.TestConfigurationSetName);
			testSetView = testSetManager.RetrieveById(projectId, testSetId2);
			Assert.AreEqual("Sample Set 4", testSetView.TestConfigurationSetName);

			//Now verify that we can also retrieve the test sets for a specific configuration set
			int count = testSetManager.CountByTestConfigurationSet(projectId, testConfigurationSetId);
			Assert.AreEqual(2, count);
			Hashtable filters = new Hashtable();
			filters["TestConfigurationSetId"] = testConfigurationSetId;
			List<TestSetView> testSets = testSetManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, testSets.Count);
		}

		/// <summary>
		/// Tests that we can execute a manual test set that uses test configurations
		/// </summary>
		[Test]
		[SpiraTestCase(1618)]
		public void _04_ExecuteManualTestRunsWithConfigurationSet()
		{
			//First lets create a new test configuration set
			int testConfigurationSetId = testConfigurationManager.InsertSet(projectId, "Sample Set 5", null, true, USER_ID_FRED_BLOGGS);

			//Now lets populate some entries
			Dictionary<int, int> testParametersDic = new Dictionary<int, int>();
			testParametersDic.Add(testCaseParameter_OS_id1, customList_OS_id);
			testParametersDic.Add(testCaseParameter_Browser_id1, customList_Browser_id);
			testConfigurationManager.PopulateTestConfigurations(projectId, testConfigurationSetId, testParametersDic);

			//Now lets remove the invalid combinations
			List<TestConfigurationEntry> testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId, testConfigurationSetId);
			if (testConfigurationEntries != null)
			{
				List<IGrouping<int, TestConfigurationEntry>> testConfigurationsGroupedByConfiguration = testConfigurationEntries.GroupBy(t => t.TestCaseConfigurationId).ToList();
				List<int> testConfigurationIds = new List<int>();
				testConfigurationIds.Add(testConfigurationsGroupedByConfiguration[2].Key);
				testConfigurationIds.Add(testConfigurationsGroupedByConfiguration[7].Key);
				testConfigurationManager.RemoveTestConfigurationsFromSet(projectId, testConfigurationSetId, testConfigurationIds);

				//Verify the results
				testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId, testConfigurationSetId);
				testConfigurationsGroupedByConfiguration = testConfigurationEntries.GroupBy(t => t.TestCaseConfigurationId).ToList();
				Assert.AreEqual(6, testConfigurationsGroupedByConfiguration.Count);
			}
			//Next create a new test set
			TestSetManager testSetManager = new TestSetManager();
			int testSetId = testSetManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, USER_ID_FRED_BLOGGS, null, TestSet.TestSetStatusEnum.NotStarted, "Test Set 1", null, null, TestRun.TestRunTypeEnum.Manual, null, null);

			//Now associate it with the configuration set
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.TestConfigurationSetId = testConfigurationSetId;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Finally we need to add the test cases to the set
			int testSetTestCaseId1 = testSetManager.AddTestCase(projectId, testSetId, testCaseId1, null, null);
			int testSetTestCaseId2 = testSetManager.AddTestCase(projectId, testSetId, testCaseId2, null, null);

			//Now we need to execute this test set and ensure that a run for each test case and test configuration is created
			TestRunManager testRunManager = new TestRunManager();
			TestRunsPending testRunsPending = testRunManager.CreateFromTestSet(USER_ID_FRED_BLOGGS, projectId, testSetId, true);
			int testRunsPendingId = testRunsPending.TestRunsPendingId;

			//Verify the count of test runs
			testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);
			Assert.AreEqual(12, testRunsPending.TestRuns.Count);

			//Now verify some of the test runs and test run steps
			TestRun testRun = testRunsPending.TestRuns[0];
			Assert.AreEqual("Open the Chrome browser on the MacOS X computer.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It works", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[1];
			Assert.AreEqual("Choose Chrome running on MacOS X.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It launches", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[2];
			Assert.AreEqual("Open the Firefox browser on the MacOS X computer.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It works", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[3];
			Assert.AreEqual("Choose Firefox running on MacOS X.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It launches", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[10];
			Assert.AreEqual("Open the IE browser on the Windows computer.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It works", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[11];
			Assert.AreEqual("Choose IE running on Windows.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It launches", testRun.TestRunSteps[0].ExpectedResult);

			//Test that it overrides any test set or test set / test case parameter values
			testSetManager.AddTestSetParameter(testSetId, testCaseParameter_OS_id1, "Linux", projectId, 1);
			List<TestSetTestCaseParameter> testSetTestCaseParameters = new List<TestSetTestCaseParameter>();
			TestSetTestCaseParameter testSetTestCaseParameter = new TestSetTestCaseParameter();
			testSetTestCaseParameter.TestCaseParameterId = testCaseParameter_Browser_id1;
			testSetTestCaseParameter.TestSetTestCaseId = testSetTestCaseId1;
			testSetTestCaseParameter.Value = "Edge";
			testSetTestCaseParameters.Add(testSetTestCaseParameter);
			testSetManager.SaveTestCaseParameterValues(testSetTestCaseParameters);

			//Verify the data
			testRunsPending = testRunManager.CreateFromTestSet(USER_ID_FRED_BLOGGS, projectId, testSetId, true);
			testRunsPendingId = testRunsPending.TestRunsPendingId;

			//Verify the count of test runs
			testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);
			Assert.AreEqual(12, testRunsPending.TestRuns.Count);

			//Now verify some of the test runs and test run steps
			testRun = testRunsPending.TestRuns[0];
			Assert.AreEqual("Open the Chrome browser on the MacOS X computer.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It works", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[1];
			Assert.AreEqual("Choose Chrome running on MacOS X.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It launches", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[2];
			Assert.AreEqual("Open the Firefox browser on the MacOS X computer.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It works", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[3];
			Assert.AreEqual("Choose Firefox running on MacOS X.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It launches", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[10];
			Assert.AreEqual("Open the IE browser on the Windows computer.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It works", testRun.TestRunSteps[0].ExpectedResult);
			testRun = testRunsPending.TestRuns[11];
			Assert.AreEqual("Choose IE running on Windows.", testRun.TestRunSteps[0].Description);
			Assert.AreEqual("It launches", testRun.TestRunSteps[0].ExpectedResult);
		}
	}
}
