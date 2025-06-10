using System;
using System.Collections.Generic;
using System.Linq;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using NUnit.Framework;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the requirements, releases and test case coverage functionality.
	/// </summary>
	/// <remarks>This is a separate class because it needs to test the TestCase, Release and Requirement classes</remarks>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class CoverageTest
	{
		protected static Business.RequirementManager requirementManager;
		protected static Business.TestCaseManager testCaseManager;
		protected static Business.ReleaseManager releaseManager;

		protected static int testCaseId1;
		protected static int testCaseId2;
		protected static int testCaseId3;
		protected static int testCaseId4;

		protected static int requirementId1;
		protected static int requirementId2;
		protected static int requirementId3;
		protected static int requirementId4;

		protected static int releaseId1;
		protected static int releaseId2;
		protected static int releaseId3;
		protected static int releaseId4;
		protected static int releaseId5;
		protected static int iterationId11;
		protected static int iterationId21;
		protected static int iterationId31;
		protected static int iterationId41;

		protected static int tc_priorityMediumId;
		protected static int rq_typeUseCaseId;

		protected static int projectId;
		protected static int projectId2;
		protected static int projectTemplateId;
		protected static int summaryRequirementId;
		protected static int testCaseFolderId;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		private const int PROJECT_ID = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			requirementManager = new RequirementManager();
			testCaseManager = new TestCaseManager();
			releaseManager = new ReleaseManager();

			//Create a new project for testing with
			ProjectManager projectManager = new ProjectManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("CoverageTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//We need to test cross-project coverage, so need to create a second project that artifacts will be shared with
			//can use the same template
			projectId2 = projectManager.Insert("CoverageTest Project 2", null, null, null, true, projectTemplateId, 1,
					adminSectionId,
					"Inserted Project");

			//Get some of the standard lookup values
			tc_priorityMediumId = testCaseManager.TestCasePriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 3).TestCasePriorityId;
			rq_typeUseCaseId = requirementManager.RequirementType_RetrieveUseCases(projectTemplateId).FirstOrDefault().RequirementTypeId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary projects and template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId2);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		[
		Test,
		SpiraTestCase(16)
		]
		public void _01_CreateTestAndRequirement()
		{
			//First we need to create a new test case and requirement to map coverage between
			//Also create a package and folder that the test case and requirement rollup to
			summaryRequirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Summary Requirement for Coverage", null, 120, USER_ID_FRED_BLOGGS);
			requirementId1 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, null, null, summaryRequirementId, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, 120, USER_ID_FRED_BLOGGS);
			testCaseFolderId = testCaseManager.TestCaseFolder_Create("Test Folder", projectId, null).TestCaseFolderId;
			testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, testCaseFolderId, null, null, null);

			//Verify they were created successfully
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(requirementId1, requirementView.RequirementId);
			Assert.AreEqual(summaryRequirementId, requirementManager.RetrieveParents(USER_ID_FRED_BLOGGS, projectId, requirementView.IndentLevel).First().RequirementId);
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual(testCaseId1, testCaseView.TestCaseId);
			Assert.AreEqual(testCaseFolderId, testCaseView.TestCaseFolderId);

			//Verify requirement summary coverage information
			Assert.AreEqual(0, requirementView.CoverageCountTotal);
			Assert.AreEqual(0, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(0, requirementView.CoverageCountCaution);
			Assert.AreEqual(0, requirementView.CoverageCountBlocked);

			//Verify folder summary execution information
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(0, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(0, testCaseFolder.CountPassed);

			//Verify that we can display their list of parents (used in the coverage tooltips)
			List<RequirementView> parentRequirements = requirementManager.RetrieveParents(USER_ID_FRED_BLOGGS, projectId, requirementView.IndentLevel);
			Assert.AreEqual("Test Summary Requirement for Coverage", parentRequirements[0].Name);

			List<TestCaseFolderHierarchyView> parentFolders = testCaseManager.TestCaseFolder_GetParents(projectId, testCaseFolderId, true);
			Assert.AreEqual("Test Folder", parentFolders[0].Name);
		}

		[
		Test,
		SpiraTestCase(35)
		]
		public void _02_AddTestCoverageToRequirement()
		{
			//Lets get the list of covered test cases for the requirement
			List<TestCase> testCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId1);
			//Verify counts
			Assert.AreEqual(0, testCases.Count);

			//Now lets add coverage for this requirement, both the original test case and two others that have execution statuses
			testCaseId2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, testCaseFolderId, null, null, null);
			testCaseId3 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 3", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, testCaseFolderId, null, null, null);
			TestRunManager testRunManager = new TestRunManager();
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId2, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), (int)TestCase.ExecutionStatusEnum.Passed, null, null, null, null, null, null, null, null, TestRun.TestRunFormatEnum.PlainText, null);
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId3, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), (int)TestCase.ExecutionStatusEnum.Caution, null, null, null, null, null, null, null, null, TestRun.TestRunFormatEnum.PlainText, null);

			List<int> coveredTestCases = new List<int>();
			coveredTestCases.Add(testCaseId1);
			coveredTestCases.Add(testCaseId2);
			coveredTestCases.Add(testCaseId3);

			//Finally lets save this coverage information and then verify that the covered/available lists are updated
			testCaseManager.AddToRequirement(projectId, requirementId1, coveredTestCases, 1);

			//Lets get the list of covered test cases for the requirement
			testCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId1);
			//Verify counts
			Assert.AreEqual(3, testCases.Count);

			//Verify requirement summary coverage information
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(3, requirementView.CoverageCountTotal, "CoverageCountTotal");
			Assert.AreEqual(1, requirementView.CoverageCountPassed, "CoverageCountPassed");
			Assert.AreEqual(1, requirementView.CoverageCountCaution, "CoverageCountCaution");
			Assert.AreEqual(0, requirementView.CoverageCountBlocked, "CoverageCountBlocked");
			Assert.AreEqual(0, requirementView.CoverageCountFailed, "CoverageCountFailed");

			//Verify the folder status as well
			testCaseManager.RefreshFolderCounts(projectId);
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(0, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);

			//Add an association between project 1 and project 2 (for cross-project tests) in both directions
			new ProjectManager().ProjectAssociation_Add(projectId2, projectId, new List<int>() { (int)Artifact.ArtifactTypeEnum.TestCase });
			new ProjectManager().ProjectAssociation_Add(projectId, projectId2, new List<int>() { (int)Artifact.ArtifactTypeEnum.Requirement });

			//Now add a test case in project 2 and link it to project 1, make sure it correctly accounts for it
			int testCaseId_project2 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId2, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 1 Project 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			testCaseManager.AddToRequirement(projectId, requirementId1, new List<int> { testCaseId_project2 }, USER_ID_SYS_ADMIN);

			//Make sure it was correctly associated
			testCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId1);
			Assert.IsTrue(testCases.Any(t => t.TestCaseId == testCaseId_project2));

			//Verify the coverage is correct
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(4, requirementView.CoverageCountTotal, "CoverageCountTotal");
			Assert.AreEqual(1, requirementView.CoverageCountPassed, "CoverageCountPassed");
			Assert.AreEqual(1, requirementView.CoverageCountCaution, "CoverageCountCaution");
			Assert.AreEqual(0, requirementView.CoverageCountBlocked, "CoverageCountBlocked");
			Assert.AreEqual(0, requirementView.CoverageCountFailed, "CoverageCountFailed");

			//Now deassociate this test case
			testCaseManager.RemoveFromRequirement(projectId, requirementId1, new List<int> { testCaseId_project2 }, USER_ID_SYS_ADMIN);

			//Make sure it was correctly de-associated
			testCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId1);
			Assert.IsFalse(testCases.Any(t => t.TestCaseId == testCaseId_project2));

			//Verify the coverage is correct
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(3, requirementView.CoverageCountTotal, "CoverageCountTotal");
			Assert.AreEqual(1, requirementView.CoverageCountPassed, "CoverageCountPassed");
			Assert.AreEqual(1, requirementView.CoverageCountCaution, "CoverageCountCaution");
			Assert.AreEqual(0, requirementView.CoverageCountBlocked, "CoverageCountBlocked");
			Assert.AreEqual(0, requirementView.CoverageCountFailed, "CoverageCountFailed");

			//Now add the requirement to the test case and repeat
			requirementManager.AddToTestCase(projectId2, testCaseId_project2, new List<int> { requirementId1 }, USER_ID_SYS_ADMIN);

			//Make sure it was correctly associated
			testCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId1);
			Assert.IsTrue(testCases.Any(t => t.TestCaseId == testCaseId_project2));

			//Verify the coverage is correct
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(4, requirementView.CoverageCountTotal, "CoverageCountTotal");
			Assert.AreEqual(1, requirementView.CoverageCountPassed, "CoverageCountPassed");
			Assert.AreEqual(1, requirementView.CoverageCountCaution, "CoverageCountCaution");
			Assert.AreEqual(0, requirementView.CoverageCountBlocked, "CoverageCountBlocked");
			Assert.AreEqual(0, requirementView.CoverageCountFailed, "CoverageCountFailed");

			//Now delete this test case
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId2, testCaseId_project2);

			//Make sure it was correctly de-associated
			testCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId1);
			Assert.IsFalse(testCases.Any(t => t.TestCaseId == testCaseId_project2));

			//Verify the coverage is correct
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(3, requirementView.CoverageCountTotal, "CoverageCountTotal");
			Assert.AreEqual(1, requirementView.CoverageCountPassed, "CoverageCountPassed");
			Assert.AreEqual(1, requirementView.CoverageCountCaution, "CoverageCountCaution");
			Assert.AreEqual(0, requirementView.CoverageCountBlocked, "CoverageCountBlocked");
			Assert.AreEqual(0, requirementView.CoverageCountFailed, "CoverageCountFailed");
		}

		[
		Test,
		SpiraTestCase(36)
		]
		public void _03_ModifyTestCoverageOfRequirement()
		{
			//Lets get the list of covered test cases for the requirement
			List<TestCase> coveredTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId1);
			Assert.AreEqual(3, coveredTestCases.Count);

			//Create a new test case for coverage testing
			testCaseId4 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 4", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityMediumId, testCaseFolderId, null, null, null);
			TestRunManager testRunManager = new TestRunManager();
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId4, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), (int)TestCase.ExecutionStatusEnum.Failed, null, null, null, null, null, null, null, null, TestRun.TestRunFormatEnum.PlainText, null);

			//Now lets remove and add coverage for this requirement
			List<int> coveredTestCaseIds = new List<int>();
			coveredTestCaseIds.Add(testCaseId2);
			coveredTestCaseIds.Add(testCaseId3);
			testCaseManager.RemoveFromRequirement(projectId, requirementId1, coveredTestCaseIds, 1);
			coveredTestCaseIds.Clear();
			coveredTestCaseIds.Add(testCaseId4);
			testCaseManager.AddToRequirement(projectId, requirementId1, coveredTestCaseIds, 1);

			//Lets get the list of covered test cases for the requirement
			coveredTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId1);
			//Verify counts
			Assert.AreEqual(2, coveredTestCases.Count);

			//Verify the coverage items
			Assert.AreEqual(testCaseId1, coveredTestCases[0].TestCaseId);
			Assert.AreEqual(testCaseId4, coveredTestCases[1].TestCaseId);

			//Verify requirement summary coverage information
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(0, requirementView.CoverageCountPassed);
			Assert.AreEqual(1, requirementView.CoverageCountFailed);

			//Verify the folder status as well
			testCaseManager.RefreshFolderCounts(projectId);
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);

			//Verify that we can add a folder of test cases to a requirement (need a new req for this)
			//test case folders need to be passed as negative numbers
			int requirementId5 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 5", null, 120, USER_ID_FRED_BLOGGS);
			testCaseManager.AddToRequirement(projectId, requirementId5, new List<int>() { -testCaseFolderId }, 1);

			//Verify
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId5);
			Assert.AreEqual(4, requirementView.CoverageCountTotal);
			Assert.AreEqual(1, requirementView.CoverageCountPassed);
			Assert.AreEqual(1, requirementView.CoverageCountFailed);
			Assert.AreEqual(1, requirementView.CoverageCountCaution);

			//Verify the folder counts
			testCaseManager.RefreshFolderCounts(projectId);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);

			//Remove the test cases by folder
			testCaseManager.RemoveFromRequirement(projectId, requirementId5, new List<int>() { -testCaseFolderId }, 1);

			//Verify
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId5);
			Assert.AreEqual(0, requirementView.CoverageCountTotal);
			Assert.AreEqual(0, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);
			Assert.AreEqual(0, requirementView.CoverageCountCaution);

			//clean up by deleting
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId5);

			//Verify the folder counts
			testCaseManager.RefreshFolderCounts(projectId);
			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);
		}

		[
		Test,
		SpiraTestCase(37)
		]
		public void _04_AddRequirementsCoverageToTestCase()
		{
			//Lets get the list of covered requirements for a specific test case (the reverse case)
			List<RequirementView> coveredRequirements = requirementManager.RetrieveCoveredByTestCaseId(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			//Verify counts
			Assert.AreEqual(1, coveredRequirements.Count);

			//We need some additional requirements
			requirementId2 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, null, null, summaryRequirementId, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 2", null, 120, USER_ID_FRED_BLOGGS);
			requirementId3 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, null, null, summaryRequirementId, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 3", null, 120, USER_ID_FRED_BLOGGS);

			//Now lets add coverage for this test case
			List<int> coveredRequirementIds = new List<int>();
			coveredRequirementIds.Add(requirementId2);
			coveredRequirementIds.Add(requirementId3);

			//Finally lets save this coverage information and then verify that the covered/available lists are updated
			requirementManager.AddToTestCase(projectId, testCaseId1, coveredRequirementIds, 1);

			//Lets get the list of covered test cases for the requirement
			coveredRequirements = requirementManager.RetrieveCoveredByTestCaseId(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			//Verify counts
			Assert.AreEqual(3, coveredRequirements.Count);

			//Verify requirement summary coverage information
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(0, requirementView.CoverageCountPassed);
			Assert.AreEqual(1, requirementView.CoverageCountFailed);

			//Verify the folder counts
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);
		}

		[
		Test,
		SpiraTestCase(38)
		]
		public void _05_ModifyRequirementsCoverageOfTestCase()
		{
			//Lets get the list of available/covered requirements for the test case
			List<RequirementView> coveredRequirements = requirementManager.RetrieveCoveredByTestCaseId(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			Assert.AreEqual(3, coveredRequirements.Count);

			//We need another test requirement
			requirementId4 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, null, null, summaryRequirementId, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 4", null, 120, USER_ID_FRED_BLOGGS);

			//Now lets remove and add coverage for this test case
			List<int> coveredRequirementIds = new List<int>();
			coveredRequirementIds.Add(requirementId3);
			requirementManager.RemoveFromTestCase(projectId, testCaseId1, coveredRequirementIds, 1);
			coveredRequirementIds.Clear();
			coveredRequirementIds.Add(requirementId4);
			requirementManager.AddToTestCase(projectId, testCaseId1, coveredRequirementIds, 1);

			//Lets get the list of covered requirements for the test case
			coveredRequirements = requirementManager.RetrieveCoveredByTestCaseId(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			//Verify counts
			Assert.AreEqual(3, coveredRequirements.Count);

			//Verify the coverage items
			Assert.AreEqual(requirementId1, coveredRequirements[0].RequirementId);
			Assert.AreEqual(requirementId2, coveredRequirements[1].RequirementId);
			Assert.AreEqual(requirementId4, coveredRequirements[2].RequirementId);

			//Verify requirement summary coverage information
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			Assert.AreEqual(2, requirementView.CoverageCountTotal);
			Assert.AreEqual(0, requirementView.CoverageCountPassed);
			Assert.AreEqual(1, requirementView.CoverageCountFailed);

			//Verify the folder counts
			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);
		}

		[
		Test,
		SpiraTestCase(171)
		]
		public void _06_DeleteTestAndRequirement()
		{
			//Verify the coverage of the summary and folder
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, summaryRequirementId);
			Assert.AreEqual(4, requirementView.CoverageCountTotal);
			Assert.AreEqual(0, requirementView.CoverageCountPassed);
			Assert.AreEqual(1, requirementView.CoverageCountFailed);
			Assert.AreEqual(0, requirementView.CoverageCountBlocked);
			Assert.AreEqual(0, requirementView.CoverageCountCaution);

			TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);

			//Finally we need to delete the test case and requirement and verify that coverage was deleted
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);

			//Verify they were deleted successfully
			bool artifactExists = true;
			try
			{
				requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Requirement not deleted correctly");
			artifactExists = true;
			try
			{
				TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Test Case not deleted correctly");

			//Verify the coverage of the summary and folder
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, summaryRequirementId);
			Assert.AreEqual(0, requirementView.CoverageCountTotal);
			Assert.AreEqual(0, requirementView.CoverageCountPassed);
			Assert.AreEqual(0, requirementView.CoverageCountFailed);

			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(0, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);

			//Now undelete and verify it went back
			requirementManager.UnDelete(requirementId1, USER_ID_FRED_BLOGGS, -1, false);
			testCaseManager.UnDelete(testCaseId1, USER_ID_FRED_BLOGGS, -1, false);

			//Verify the coverage of the summary and folder
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, summaryRequirementId);
			Assert.AreEqual(4, requirementView.CoverageCountTotal);
			Assert.AreEqual(0, requirementView.CoverageCountPassed);
			Assert.AreEqual(1, requirementView.CoverageCountFailed);
			Assert.AreEqual(0, requirementView.CoverageCountBlocked);
			Assert.AreEqual(0, requirementView.CoverageCountCaution);

			testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
			Assert.AreEqual(0, testCaseFolder.CountBlocked);
			Assert.AreEqual(1, testCaseFolder.CountCaution);
			Assert.AreEqual(1, testCaseFolder.CountFailed);
			Assert.AreEqual(0, testCaseFolder.CountNotApplicable);
			Assert.AreEqual(1, testCaseFolder.CountNotRun);
			Assert.AreEqual(1, testCaseFolder.CountPassed);
		}

		[
		Test,
		SpiraTestCase(172)
		]
		public void _07_CreateIterationAndRelease()
		{
			//First we need to create a new release and iteration to map coverage between
			releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			iterationId11 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0 Sprint 1", null, "2.0.0.0 001", releaseId1, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);

			//Verify they were created successfully
			ReleaseView release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			Assert.AreEqual(releaseId1, release.ReleaseId);
			ReleaseView iteration = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, iterationId11);
			Assert.AreEqual(releaseId1, release.ReleaseId);
			TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId1);
			Assert.AreEqual(testCaseId1, testCase.TestCaseId);

			//Verify that we can display their list of parents (used in the coverage tooltips)
			List<ReleaseView> parentReleases = releaseManager.RetrieveParents(projectId, release.IndentLevel, false);
			Assert.AreEqual(0, parentReleases.Count);
			parentReleases = releaseManager.RetrieveParents(projectId, iteration.IndentLevel, false);
			Assert.AreEqual(1, parentReleases.Count);
			Assert.AreEqual("Release 1.0", parentReleases[0].Name);

			List<TestCaseFolderHierarchyView> parentFolders = testCaseManager.TestCaseFolder_GetParents(projectId, testCaseFolderId, true);
			Assert.AreEqual("Test Folder", parentFolders[0].Name);
		}

		[
		Test,
		SpiraTestCase(173)
		]
		public void _08_AddTestCoverageToRelease()
		{
			//Lets get the list of covered test cases for the release
			List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId1);
			//Verify counts
			Assert.AreEqual(0, mappedTestCases.Count);

			//Now lets add coverage for this release, both the new test case id plus one from sample data
			List<int> testCases = new List<int>();
			testCases.Add(testCaseId1);
			testCases.Add(testCaseId2);
			testCases.Add(testCaseId3);

			//Finally lets save this coverage information and then verify that the covered/available lists are updated
			testCaseManager.AddToRelease(projectId, releaseId1, testCases, USER_ID_SYS_ADMIN);

			//Lets get the list of covered test cases for the release
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId1);

			//Verify counts
			Assert.AreEqual(3, mappedTestCases.Count);
		}

		[
		Test,
		SpiraTestCase(174)
		]
		public void _09_ModifyTestCoverageOfRelease()
		{
			//Lets get the list of covered test cases for the release
			List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId1);
			Assert.AreEqual(3, mappedTestCases.Count);

			//Now lets remove and add coverage for this release
			List<int> testCases = new List<int>();
			testCases.Add(testCaseId2);
			testCases.Add(testCaseId3);
			testCaseManager.RemoveFromRelease(projectId, releaseId1, testCases, USER_ID_SYS_ADMIN);
			testCases.Clear();
			testCases.Add(testCaseId4);
			testCaseManager.AddToRelease(projectId, releaseId1, testCases, USER_ID_SYS_ADMIN);

			//Lets get the list of covered test cases for the release
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId1);
			//Verify counts
			Assert.AreEqual(2, mappedTestCases.Count);

			//Verify the coverage items
			Assert.AreEqual(testCaseId1, mappedTestCases[0].TestCaseId);
			Assert.AreEqual(testCaseId4, mappedTestCases[1].TestCaseId);

			//Also need to test that we can add/remove a whole folder of test cases to a release and that the release is refreshed ok
			//We need a new clean release for this
			//We have to pass the test case folder as a negative id
			releaseId5 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 5.0", null, "5.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			testCaseManager.AddToRelease(projectId, releaseId5, new List<int>() { -testCaseFolderId }, USER_ID_SYS_ADMIN);

			//Verify the test case count, folder count and release count
			TestCaseFolderReleaseView testCaseFolder = testCaseManager.TestCaseFolder_GetByParentIdForRelease(projectId, null, releaseId5).FirstOrDefault(t => t.TestCaseFolderId == testCaseFolderId);
			Assert.AreEqual(4, testCaseFolder.CountNotRun);

			ReleaseView release = releaseManager.RetrieveById(User.UserInternal, projectId, releaseId5);
			Assert.AreEqual(4, release.CountNotRun);

			//Now remove from the release
			testCaseManager.RemoveFromRelease(projectId, releaseId5, new List<int>() { -testCaseFolderId }, USER_ID_SYS_ADMIN);

			//Verify the test case count, folder count and release count
			testCaseFolder = testCaseManager.TestCaseFolder_GetByParentIdForRelease(projectId, null, releaseId5).FirstOrDefault(t => t.TestCaseFolderId == testCaseFolderId);
			Assert.IsNull(testCaseFolder);

			release = releaseManager.RetrieveById(User.UserInternal, projectId, releaseId5);
			Assert.AreEqual(0, release.CountNotRun);
		}

		[
		Test,
		SpiraTestCase(175)
		]
		public void _10_AddReleaseCoverageToTestCase()
		{
			//Lets get the list of covered releases for a specific test case (the reverse case)
			List<ReleaseView> mappedReleases = releaseManager.RetrieveMappedByTestCaseId(Business.UserManager.UserInternal, projectId, testCaseId1);
			//Verify counts
			Assert.AreEqual(1, mappedReleases.Count);

			//Create some more releases and iterations
			releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0", null, "2.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			iterationId21 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0 Sprint 1", null, "2.0.0.0 001", releaseId2, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			releaseId3 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 3.0", null, "3.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			iterationId31 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 3.0 Sprint 1", null, "3.0.0.0 001", releaseId3, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);

			//Now lets add coverage for this test case, from sample data
			//Note that the releases included iterations which will also get brought across
			List<int> releases = new List<int>();
			releases.Add(releaseId2);
			releases.Add(releaseId3);

			//Finally lets save this coverage information and then verify that the covered lists are updated
			releaseManager.AddToTestCase(projectId, testCaseId1, releases, USER_ID_SYS_ADMIN);

			//Lets get the list of covered test cases for the release
			mappedReleases = releaseManager.RetrieveMappedByTestCaseId(Business.UserManager.UserInternal, projectId, testCaseId1);
			//Verify counts
			Assert.AreEqual(5, mappedReleases.Count);
		}

		[
		Test,
		SpiraTestCase(176)
		]
		public void _11_ModifyReleaseCoverageOfTestCase()
		{
			//Lets get the list of covered releases for the test case
			List<ReleaseView> mappedReleases = releaseManager.RetrieveMappedByTestCaseId(Business.UserManager.UserInternal, projectId, testCaseId1);
			Assert.AreEqual(5, mappedReleases.Count);

			//Create a new release/iteration that we shall need
			releaseId4 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 4.0", null, "4.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			iterationId41 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 4.0 Sprint 1", null, "4.0.0.0 001", releaseId4, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);


			//Now lets remove and add coverage for this test case
			List<int> releases = new List<int>();
			releases.Add(releaseId2);    //Doesn't remove the child iterations
			releaseManager.RemoveFromTestCase(projectId, testCaseId1, releases, USER_ID_SYS_ADMIN);
			releases.Clear();
			releases.Add(releaseId4);    //Includes the child iterations
			releaseManager.AddToTestCase(projectId, testCaseId1, releases, USER_ID_SYS_ADMIN);

			//Lets get the list of covered releases for the test case
			mappedReleases = releaseManager.RetrieveMappedByTestCaseId(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			//Verify counts
			Assert.AreEqual(6, mappedReleases.Count);

			//Verify the mapped items
			Assert.AreEqual(releaseId1, mappedReleases[0].ReleaseId);
			Assert.AreEqual(iterationId21, mappedReleases[1].ReleaseId);
			Assert.AreEqual(releaseId3, mappedReleases[2].ReleaseId);
			Assert.AreEqual(iterationId31, mappedReleases[3].ReleaseId);
			Assert.AreEqual(releaseId4, mappedReleases[4].ReleaseId);
			Assert.AreEqual(iterationId41, mappedReleases[5].ReleaseId);
		}

		[
		Test,
		SpiraTestCase(177)
		]
		public void _12_DeleteTestAndRelease()
		{
			//Finally we need to delete the test case and release and verify that coverage was deleted
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);

			//Verify they were deleted successfully
			bool artifactExists = true;
			try
			{
				ReleaseView release = releaseManager.RetrieveById2(projectId, requirementId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Release not deleted correctly");

			artifactExists = true;
			try
			{
				TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Test Case not deleted correctly");

			//Now undelete and verify it went back
			releaseManager.UnDelete(releaseId1, USER_ID_FRED_BLOGGS, -1, false);
			testCaseManager.UnDelete(testCaseId1, USER_ID_FRED_BLOGGS, -1, false);
		}

		/// <summary>
		/// Tests that we can create a requirement from a test case and vice-versa
		/// </summary>
		[
		Test,
		SpiraTestCase(556)
		]
		public void _13_CreateTestFromRequirement()
		{
			//Create a requirement from a test case
			int requirementId = requirementManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, testCaseId1, null);
			TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId1);

			//Verify that it has the same description as the original test case
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Assert.AreEqual(testCaseView.Description, requirementView.Description);

			//Verify that a comment was created describing that one was created from the other
			DiscussionManager discussionManager = new DiscussionManager();
			IEnumerable<IDiscussion> comments = discussionManager.Retrieve(requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(1, comments.Count());

			//Verify that it was created and that it's mapped to the original test case
			Assert.AreEqual(requirementId, requirementView.RequirementId);
			List<TestCase> coveredTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId);
			Assert.AreEqual(testCaseId1, coveredTestCases[0].TestCaseId);

			//Now create a test case from a requirement
			int testCaseId = testCaseManager.CreateFromRequirement(USER_ID_FRED_BLOGGS, projectId, requirementId1, null);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId1);

			//Verify that it has the same description as the original test case
			testCaseView = testCaseManager.RetrieveById(projectId, testCaseId);
			Assert.AreEqual(requirementView.Description, testCaseView.Description);

			//Verify that a comment was created describing that one was created from the other
			comments = discussionManager.Retrieve(testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(1, comments.Count());

			//Verify that it was created and that it's mapped to the original test case
			Assert.AreEqual(testCaseId, testCaseView.TestCaseId);
			List<RequirementView> requirements = requirementManager.RetrieveCoveredByTestCaseId(Business.UserManager.UserInternal, projectId, testCaseId);
			Assert.AreEqual(1, requirements.Count);
			Assert.AreEqual(requirementId1, requirements[0].RequirementId);
		}

		/// <summary>
		/// Test that we can create a test case from a use case requirement that has steps
		/// the test case steps should be populated from the use case steps
		/// </summary>
		[Test]
		[SpiraTestCase(1217)]
		public void _14_CreateTestCaseFromUseCase()
		{
			//First create a new requirement that we'll add steps to
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, rq_typeUseCaseId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", "The system must do XYZ", null, USER_ID_FRED_BLOGGS);

			//Now lets add some steps to this requirement
			int requirementStepId1 = requirementManager.InsertStep(projectId, requirementId, null, "First do this", USER_ID_FRED_BLOGGS);
			int requirementStepId2 = requirementManager.InsertStep(projectId, requirementId, null, "Then do this", USER_ID_FRED_BLOGGS);
			int requirementStepId3 = requirementManager.InsertStep(projectId, requirementId, null, "Finally do this", USER_ID_FRED_BLOGGS);

			//Now create a test case from this requirement
			int testCaseId = testCaseManager.CreateFromRequirement(USER_ID_FRED_BLOGGS, projectId, requirementId, null);

			//Now retrieve the test case and verify that the description and steps populated
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			//Verify the test case
			Assert.AreEqual("Verify: Requirement 1", testCase.Name);
			Assert.AreEqual("The system must do XYZ", testCase.Description);
			Assert.IsTrue(testCase.IsTestSteps);

			//Verify the steps
			Assert.AreEqual(3, testCase.TestSteps.Count);
			Assert.AreEqual("First do this", testCase.TestSteps[0].Description);
			Assert.AreEqual("Works as expected.", testCase.TestSteps[0].ExpectedResult);
			Assert.AreEqual("Then do this", testCase.TestSteps[1].Description);
			Assert.AreEqual("Works as expected.", testCase.TestSteps[1].ExpectedResult);
			Assert.AreEqual("Finally do this", testCase.TestSteps[2].Description);
			Assert.AreEqual("Works as expected.", testCase.TestSteps[2].ExpectedResult);

			//Verify that they are associated with each other
			List<RequirementView> requirements = requirementManager.RetrieveCoveredByTestCaseId(Business.UserManager.UserInternal, projectId, testCaseId);
			Assert.AreEqual(requirementId, requirements[0].RequirementId);
		}

		/// <summary>
		/// This test method uses the sample project
		/// </summary>
		[
		Test,
		SpiraTestCase(39)
		]
		public void _15_RetrieveSummaryData()
		{
			//First get the requirement coverage summary - all releases
			List<RequirementCoverageSummary> coverageSummary = requirementManager.RetrieveCoverageSummary(PROJECT_ID, null, false);

			//Make sure the data is as expected
			Assert.AreEqual(6, coverageSummary.Count);
			Assert.AreEqual("Passed", coverageSummary[0].CoverageStatus);
			Assert.AreEqual(8.1, System.Math.Round((double)(coverageSummary[0].CoverageCount), 1));
			Assert.AreEqual("Not Covered", coverageSummary[5].CoverageStatus);
			Assert.AreEqual(18.0, System.Math.Round((double)(coverageSummary[5].CoverageCount), 1));

			//Next get the requirement coverage summary - active releases only
			coverageSummary = requirementManager.RetrieveCoverageSummary(PROJECT_ID, RequirementManager.RELEASE_ID_ACTIVE_RELEASES_ONLY, false);

			//Make sure the data is as expected
			Assert.AreEqual(6, coverageSummary.Count);
			Assert.AreEqual("Passed", coverageSummary[0].CoverageStatus);
			Assert.AreEqual(5.8, System.Math.Round((double)(coverageSummary[0].CoverageCount), 1));
			Assert.AreEqual("Not Covered", coverageSummary[5].CoverageStatus);
			Assert.AreEqual(10.0, System.Math.Round((double)(coverageSummary[5].CoverageCount), 1));

			//First get the requirement coverage summary untyped dataset - specific requirement release
			coverageSummary = requirementManager.RetrieveCoverageSummary(PROJECT_ID, 1, false);

			//Make sure the data is as expected
			Assert.AreEqual(6, coverageSummary.Count);
			Assert.AreEqual("Passed", coverageSummary[0].CoverageStatus);
			Assert.AreEqual(3.7, System.Math.Round((double)(coverageSummary[0].CoverageCount), 1));
			Assert.AreEqual("Not Covered", coverageSummary[5].CoverageStatus);
			Assert.AreEqual(5.0, System.Math.Round((double)(coverageSummary[5].CoverageCount), 1));

			//Now get the requirement coverage summary for a release with iterations - specific requirement release
			coverageSummary = requirementManager.RetrieveCoverageSummary(PROJECT_ID, 4, false);

			//Make sure the data is as expected
			Assert.AreEqual(6, coverageSummary.Count);
			Assert.AreEqual("Passed", coverageSummary[0].CoverageStatus);
			Assert.AreEqual(2.2, System.Math.Round((double)(coverageSummary[0].CoverageCount), 1));
			Assert.AreEqual("Failed", coverageSummary[1].CoverageStatus);
			Assert.AreEqual(0.0, System.Math.Round((double)(coverageSummary[1].CoverageCount), 1));
			Assert.AreEqual("Not Covered", coverageSummary[5].CoverageStatus);
			Assert.AreEqual(5.0, System.Math.Round((double)(coverageSummary[5].CoverageCount), 1));

			//Now get the requirement coverage summary for a release with iterations - specific test case release
			coverageSummary = requirementManager.RetrieveCoverageSummary(PROJECT_ID, 4, true);

			//Make sure the data is as expected
			Assert.AreEqual(7, coverageSummary.Count);
			Assert.AreEqual("Passed", coverageSummary[0].CoverageStatus);
			Assert.AreEqual(4.8, System.Math.Round((double)(coverageSummary[0].CoverageCount), 1));
			Assert.AreEqual("Failed", coverageSummary[1].CoverageStatus);
			Assert.AreEqual(2.8, System.Math.Round((double)(coverageSummary[1].CoverageCount), 1));
			Assert.AreEqual("Not Covered", coverageSummary[6].CoverageStatus);
			Assert.AreEqual(24.0, System.Math.Round((double)(coverageSummary[6].CoverageCount), 1));
		}

		/// <summary>
		/// Tests that changing a linked requirement will mark the test case as 'suspect'
		/// </summary>
		[Test]
		[SpiraTestCase(1550)]
		public void _16_MarkTestCaseAsSuspect()
		{
			//First create a new requirement and test case in the new project
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, 120, USER_ID_FRED_BLOGGS);
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 1", null, null, TestCase.TestCaseStatusEnum.Draft, tc_priorityMediumId, null, null, null, null);

			//Verify that the test case is not marked as 'suspect'
			TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.IsFalse(testCase.IsSuspect);

			//Now link the requirement to the test case
			requirementManager.AddToTestCase(projectId, testCaseId, new List<int>() { requirementId }, 1);

			//Verify that the test case is not marked as 'suspect'
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.IsFalse(testCase.IsSuspect);

			//Now change the requirement in some way
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.Description = "Changed";
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement }, null, true);

			//Verify that the test case is not suspect because the requirement and test case are not yet approved
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.IsFalse(testCase.IsSuspect);

			//Change the status of the requirement
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Accepted;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement }, null, true);

			//Verify that the test case is not suspect because the test case is not yet approved
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.IsFalse(testCase.IsSuspect);

			//Change the test case to Approved
			testCase.StartTracking();
			testCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.Approved;
			testCaseManager.Update(testCase, USER_ID_FRED_BLOGGS, null, true);

			//Now change the requirement in some way
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.Description = "Changed Again";
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement }, null, true);

			//Verify that the test case is now marked as 'suspect'
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.IsTrue(testCase.IsSuspect);

			//Change the test case to not suspect and verify it is accepted
			testCase.StartTracking();
			testCase.IsSuspect = false;
			testCaseManager.Update(testCase, USER_ID_FRED_BLOGGS, null, true);

			testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			Assert.IsFalse(testCase.IsSuspect);

			//Clean up by deleting the requirement and test case
			requirementManager.DeleteFromDatabase(requirementId, USER_ID_FRED_BLOGGS);
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// This test case is used to test for some unusual edge cases
		/// </summary>
		[Test]
		[SpiraTestCase(2164)]
		public void _17_EdgeCases()
		{
			//First [IN:4801]
			/*
            Create a major release (1.0) with an iteration in it(1.0.0.1).Create a test case. Add the test case to both 1.0 and 1.0.0.1.Execute the test case. It doesn't matter what the execution status is, but let's say it failed.At this point, everything looks correct and consistent.On the release list, the execution status for both 1.0 and 1.0.0.1 is red, # failed = 1. On the test case list, the execution status is failed, no matter whether you select All Releases, 1.0, or 1.0.0.1 from the dropdown. All is well.
            Now, on the release list, do either A or B:
                A.Create a new iteration somewhere that is not within 1.0.Then move and indent it so that it is within 1.0.Or...
                B.Create a new iteration within 1.0, by using Insert->Child Iteration.Then drag the new iteration above the old iteration. 
            At this point, the test coverage column for 1.0 changes to all gray..I expected the test coverage for the major release to remain the same as it had been.See screenshot.Now check the test case list: As expected, if you choose All Releases or 1.0.0.1, the execution status is Failed.But if you select 1.0, the execution status is Not Run.It should be Failed.
            The test run is still intact, so if you Refresh the Test Status Cache, the test coverage for 1.0 gets corrected.That is, it becomes red again.
            */

			//Create a new release and iteration
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0", null, "2.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			int iterationId1 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0 Sprint 1", null, "2.0.0.1", releaseId, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);

			//Create a test case with a default step and add to both release and iteration
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 2", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null, true, true);
			//Finally lets save this coverage information and then verify that the covered/available lists are updated
			releaseManager.AddToTestCase(projectId, testCaseId, new List<int> { releaseId, iterationId1 }, USER_ID_SYS_ADMIN);

			//Execute the test case against the iteration
			TestRunManager testRunManager = new TestRunManager();
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId, iterationId1, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), (int)TestCase.ExecutionStatusEnum.Failed, "Unit Test", "Unit Test", 1, "Bad", "Bad", null, null, null, TestRun.TestRunFormatEnum.PlainText, null, true);

			//Verify the release' execution status
			Release release = releaseManager.RetrieveById3(projectId, releaseId);
			Assert.AreEqual(1, release.CountFailed);
			Release iteration = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual(1, release.CountFailed);

			//Now add a new iteration under this release
			int iterationId2 = releaseManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0 Sprint 2", null, "2.0.0.2", releaseId, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);

			//Verify that the execution statuses are not affected
			release = releaseManager.RetrieveById3(projectId, releaseId);
			Assert.AreEqual(1, release.CountFailed);
			iteration = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual(1, iteration.CountFailed);
			iteration = releaseManager.RetrieveById3(projectId, iterationId2);
			Assert.AreEqual(0, iteration.CountFailed);
			Assert.AreEqual(0, iteration.CountNotRun);

			//Now move the new iteration above the original one
			releaseManager.Move(USER_ID_FRED_BLOGGS, projectId, iterationId2, iterationId1);

			//Verify that the execution statuses are not affected
			release = releaseManager.RetrieveById3(projectId, releaseId);
			Assert.AreEqual(1, release.CountFailed);
			iteration = releaseManager.RetrieveById3(projectId, iterationId1);
			Assert.AreEqual(1, iteration.CountFailed);
			iteration = releaseManager.RetrieveById3(projectId, iterationId2);
			Assert.AreEqual(0, iteration.CountFailed);
			Assert.AreEqual(0, iteration.CountNotRun);
		}

		/// <summary>
		/// Test that a test case passing will switch the requirement and its parents to tested (if all tests are passed)
		/// </summary>
		[Test]
		[SpiraTestCase(2744)]
		public void _18_RequirementAutoChangeToTested()
		{
			//Create a two-level requirement hierarchy of developed requirements and one test case
			int releaseId18 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 18", null, "18.0.0.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			int summaryRequirementId18 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId18, null, (int?)null, Requirement.RequirementStatusEnum.Developed, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Summary Requirement for Coverage 18", null, 120, USER_ID_FRED_BLOGGS);
			int requirementId18 = requirementManager.InsertChild(USER_ID_FRED_BLOGGS, projectId, releaseId18, null, summaryRequirementId18, Requirement.RequirementStatusEnum.Developed, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 18", null, 120, USER_ID_FRED_BLOGGS);
			int testCaseId18 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Test Case 18", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);

			//Associate the test case with the requirement
			requirementManager.AddToTestCase(projectId, testCaseId18, new List<int>() { requirementId18 }, USER_ID_FRED_BLOGGS);

			//Verify the status and coverage of the requirement and its parent
			RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId18);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirement.RequirementStatusId);
			Assert.AreEqual(1, requirement.CoverageCountTotal);
			Assert.AreEqual(0, requirement.CoverageCountPassed);
			requirement = requirementManager.RetrieveById2(projectId, summaryRequirementId18);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, requirement.RequirementStatusId);
			Assert.AreEqual(1, requirement.CoverageCountTotal);
			Assert.AreEqual(0, requirement.CoverageCountPassed);

			//Execute and pass this requirement
			TestRunManager testRunManager = new TestRunManager();
			testRunManager.Record(projectId, USER_ID_FRED_BLOGGS, testCaseId18, null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), (int)TestCase.ExecutionStatusEnum.Passed, null, null, null, null, null, null, null, null, TestRun.TestRunFormatEnum.PlainText, null);

			//Verify the status and coverage of the requirement and its parent
			requirement = requirementManager.RetrieveById2(projectId, requirementId18);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Tested, requirement.RequirementStatusId);
			Assert.AreEqual(1, requirement.CoverageCountTotal);
			Assert.AreEqual(1, requirement.CoverageCountPassed);
			requirement = requirementManager.RetrieveById2(projectId, summaryRequirementId18);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Tested, requirement.RequirementStatusId);
			Assert.AreEqual(1, requirement.CoverageCountTotal);
			Assert.AreEqual(1, requirement.CoverageCountPassed);
		}
	}
}
