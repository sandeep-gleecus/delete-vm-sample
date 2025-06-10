using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using NUnit.Framework;
using System.Threading;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Pull Request business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class PullRequestManagerTest
	{
		private static int projectId;
		private static int projectTemplateId;

		private static int releaseId1;

		private static int taskId1;
		private static int taskId2;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		private const int VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			//Create a new project for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("PullRequestManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Add a new release
			releaseId1 = new ReleaseManager().Insert(USER_ID_SYS_ADMIN, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1M, 0M, null);

			//We need to initialize the source code cache
			SourceCodeManager sourceCodeManager = new SourceCodeManager();
			
			string adminSectionName1 = "Source Code";
			var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId1 = adminSection1.ADMIN_SECTION_ID;
			sourceCodeManager.InsertProjectSettings(VERSION_CONTROL_SYSTEM_TEST_PROVIDER_ID, projectId, true, "test://MyRepository", "fredbloggs", "PleaseChange", null, null, null, null, null, null, 1, adminSectionId1, "Inserted Project Settings in Source Code");
			sourceCodeManager = new SourceCodeManager(projectId);
			if (!SourceCodeManager.IsCacheUpdateRunning)
			{
				Logger.LogInformationalEvent("SourceCodeTester", "Refreshing Cache");
				sourceCodeManager.ClearCacheAndRefresh();
			}

			//Wait five seconds to make sure the caching is finished.
			Thread.Sleep(5 * 1000);

			//Do one final cache update to make sure IDs are stable
			if (!SourceCodeManager.IsCacheUpdateRunning)
			{
				sourceCodeManager.LaunchCacheRefresh();
			}

			//Wait five seconds to make sure the caching is finished.
			Thread.Sleep(5 * 1000);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		/// <summary>
		/// Creates a new pull request
		/// </summary>
		[Test]
		[SpiraTestCase(2729)]
		public void _01_CreatePullRequest()
		{
			PullRequestManager pullRequestManager = new PullRequestManager();

			//Create a pull request from develop > main with no release or owner set
			taskId1 = pullRequestManager.PullRequest_Create(projectId, "merge from develop into main", USER_ID_SYS_ADMIN, "develop", "main", null, null);

			//Create a pull request from feature/sprint-2 > develop with a release and owner set
			taskId2 = pullRequestManager.PullRequest_Create(projectId, "merge from feature/sprint-2 into develop", USER_ID_SYS_ADMIN, "feature/sprint-2", "develop", releaseId1, USER_ID_FRED_BLOGGS);

			//Verify both were created
			PullRequest pullRequest;
			pullRequest = pullRequestManager.PullRequest_RetrieveById(taskId1);
			Assert.AreEqual("develop", pullRequest.SourceBranchName);
			Assert.AreEqual("main", pullRequest.DestBranchName);
			Assert.AreEqual("Not Started", pullRequest.TaskStatusName);
			Assert.AreEqual("Pull Request", pullRequest.TaskTypeName);
			Assert.IsNull(pullRequest.OwnerName);
			Assert.IsNull(pullRequest.ReleaseVersionNumber);

			pullRequest = pullRequestManager.PullRequest_RetrieveById(taskId2);
			Assert.AreEqual("feature/sprint-2", pullRequest.SourceBranchName);
			Assert.AreEqual("develop", pullRequest.DestBranchName);
			Assert.AreEqual("Not Started", pullRequest.TaskStatusName);
			Assert.AreEqual("Pull Request", pullRequest.TaskTypeName);
			Assert.AreEqual("Fred Bloggs", pullRequest.OwnerName);
			Assert.AreEqual("1.0", pullRequest.ReleaseVersionNumber);
		}

		/// <summary>
		/// Retrieves pull requests with sorts and filters
		/// </summary>
		[Test]
		[SpiraTestCase(2730)]
		public void _02_RetrievePullRequests()
		{
			PullRequestManager pullRequestManager = new PullRequestManager();

			//Verify the count of pull requests
			int count = pullRequestManager.PullRequest_Count(projectId, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, count);

			//Retrieve all the pull requests
			List<PullRequest> pullRequests = pullRequestManager.PullRequest_Retrieve(projectId, null, true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, pullRequests.Count);

			//Test that we can filter
			Hashtable filters = new Hashtable();

			//Get the list of branches
			List<VersionControlBranch> branches = new SourceCodeManager().RetrieveBranches2(projectId);

			//By name
			filters.Clear();
			filters.Add("Name", "main");
			pullRequests = pullRequestManager.PullRequest_Retrieve(projectId, null, true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, pullRequests.Count);
			Assert.AreEqual(taskId1, pullRequests[0].TaskId);

			//By Source Branch
			filters.Clear();
			filters.Add("SourceBranchId", branches.FirstOrDefault(b => b.Name == "develop").BranchId);
			pullRequests = pullRequestManager.PullRequest_Retrieve(projectId, null, true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, pullRequests.Count);
			Assert.AreEqual(taskId1, pullRequests[0].TaskId);

			//By Dest Branch
			filters.Clear();
			filters.Add("DestBranchId", branches.FirstOrDefault(b => b.Name == "develop").BranchId);
			pullRequests = pullRequestManager.PullRequest_Retrieve(projectId, null, true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, pullRequests.Count);
			Assert.AreEqual(taskId2, pullRequests[0].TaskId);
		}

		/// <summary>
		/// Modifies an existing pull request
		/// </summary>
		[Test]
		[SpiraTestCase(2731)]
		public void _03_EditPullRequests()
		{
			PullRequestManager pullRequestManager = new PullRequestManager();

			//Get the list of branches
			List<VersionControlBranch> branches = new SourceCodeManager().RetrieveBranches2(projectId);

			//Retrieve and update one of the pull request tasks
			DataModel.Task task = pullRequestManager.Task_RetrieveById(taskId1);
			task.StartTracking();
			task.ReleaseId = releaseId1;
			task.OwnerId = USER_ID_JOE_SMITH;
			VersionControlPullRequest vcpr = task.PullRequests.FirstOrDefault();
			vcpr.SourceBranchId = branches.FirstOrDefault(b => b.Name == "feature/sprint-3").BranchId;
			vcpr.DestBranchId = branches.FirstOrDefault(b => b.Name == "develop").BranchId;
			pullRequestManager.Task_Update(task, USER_ID_SYS_ADMIN);

			//Verify the changes
			PullRequest pullRequest = pullRequestManager.PullRequest_RetrieveById(taskId1);
			Assert.AreEqual("feature/sprint-3", pullRequest.SourceBranchName);
			Assert.AreEqual("develop", pullRequest.DestBranchName);
			Assert.AreEqual("Not Started", pullRequest.TaskStatusName);
			Assert.AreEqual("Pull Request", pullRequest.TaskTypeName);
			Assert.AreEqual("Joe P Smith", pullRequest.OwnerName);
			Assert.AreEqual("1.0", pullRequest.ReleaseVersionNumber);

			//Change the branches back
			task = pullRequestManager.Task_RetrieveById(taskId1);
			task.StartTracking();
			vcpr = task.PullRequests.FirstOrDefault();
			vcpr.SourceBranchId = branches.FirstOrDefault(b => b.Name == "develop").BranchId;
			vcpr.DestBranchId = branches.FirstOrDefault(b => b.Name == "main").BranchId;
			pullRequestManager.Task_Update(task, USER_ID_SYS_ADMIN);

			//Verify the changes
			pullRequest = pullRequestManager.PullRequest_RetrieveById(taskId1);
			Assert.AreEqual("develop", pullRequest.SourceBranchName);
			Assert.AreEqual("main", pullRequest.DestBranchName);
			Assert.AreEqual("Not Started", pullRequest.TaskStatusName);
			Assert.AreEqual("Pull Request", pullRequest.TaskTypeName);
			Assert.AreEqual("Joe P Smith", pullRequest.OwnerName);
			Assert.AreEqual("1.0", pullRequest.ReleaseVersionNumber);

			//Verify that we can clone a pull request and it copies over the branch information along with the main task details
			int taskId3 = new TaskManager().Copy(USER_ID_FRED_BLOGGS, taskId1);

			//Verify the clone
			pullRequest = pullRequestManager.PullRequest_RetrieveById(taskId3);
			Assert.AreEqual("develop", pullRequest.SourceBranchName);
			Assert.AreEqual("main", pullRequest.DestBranchName);
			Assert.AreEqual("Not Started", pullRequest.TaskStatusName);
			Assert.AreEqual("Pull Request", pullRequest.TaskTypeName);
			Assert.AreEqual("Joe P Smith", pullRequest.OwnerName);
			Assert.AreEqual("1.0", pullRequest.ReleaseVersionNumber);
		}

		/// <summary>
		/// Views the commits in a pull requests
		/// </summary>
		[Test]
		[SpiraTestCase(2733)]
		public void _04_ViewCommitsInPullRequest()
		{
			SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
			int totalCount;
			List<SourceCodeCommit> commits = sourceCodeManager.RetrieveRevisionsForPullRequest(taskId1, null, true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, out totalCount);
			Assert.AreEqual(4, commits.Count);
			Assert.IsTrue(commits.Any(c => c.Name == "rev0016"));
			Assert.IsTrue(commits.Any(c => c.Name == "rev0015"));
			Assert.IsTrue(commits.Any(c => c.Name == "rev0014"));
			Assert.IsTrue(commits.Any(c => c.Name == "rev0013"));
			commits = sourceCodeManager.RetrieveRevisionsForPullRequest(taskId2, null, true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, out totalCount);
			Assert.AreEqual(0, commits.Count);
		}

		/// <summary>
		/// Deletes a new pull request
		/// </summary>
		[Test]
		[SpiraTestCase(2732)]
		public void _05_DeletePullRequest()
		{
			//Verify that we can delete a task that has an associated pull request
			TaskManager taskManager = new TaskManager();
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_SYS_ADMIN);
			taskManager.DeleteFromDatabase(taskId1, USER_ID_SYS_ADMIN);

			//Verify deleted
			bool exceptionThrown = false;
			try
			{
				taskManager.RetrieveById(taskId1);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);
		}
	}
}
