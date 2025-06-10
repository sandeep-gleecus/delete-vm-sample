using System;
using System.Collections;
using System.Data;
using System.Linq;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using NUnit.Framework;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Task business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class TaskManagerTest
	{
		protected static Business.TaskManager taskManager;
		protected static Business.HistoryManager history;
		protected static List<TaskView> tasks;
		protected static TaskView taskView;
		protected static Task task;
		private static int projectId;
		private static int projectTemplateId;


		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static int taskId1;
		protected static int taskId2;
		protected static int taskId3;
		protected static int taskId4;
		protected static int taskId5;
		protected static DateTime startDate;
		protected static DateTime endDate;

		private const int PROJECT_ID = 1;
		private const int PROJECT_EMPTY_ID = 2;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			taskManager = new Business.TaskManager();

			//Create a new project for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("TaskManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the last artifact id
			history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Purge any deleted items in the projects
			Business.HistoryManager history = new Business.HistoryManager();
			history.PurgeAllDeleted(PROJECT_ID, USER_ID_FRED_BLOGGS);
			history.PurgeAllDeleted(PROJECT_EMPTY_ID, USER_ID_FRED_BLOGGS);

			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);

			//We need to delete any artifact history items created
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());
		}

		[
		Test,
		SpiraTestCase(317)
		]
		public void _01_RetrieveTasks()
		{
			//First lets verify that we can retrieve the list of tasks associated with a requirement
			tasks = taskManager.RetrieveByRequirementId(4);
			Assert.AreEqual(3, tasks.Count);

			Assert.AreEqual(1, tasks[0].TaskId);
			Assert.AreEqual("Develop new book entry screen", tasks[0].Name);
			Assert.AreEqual("Completed", tasks[0].TaskStatusName);
			Assert.AreEqual("1 - Critical", tasks[0].TaskPriorityName);
			//Assert.IsTrue(tasks[0].StartDate >= DateTime.UtcNow.AddDays(-45) && tasks[0].StartDate <= DateTime.UtcNow.AddDays(-40));
			//Assert.IsTrue(tasks[0].EndDate >= DateTime.UtcNow.AddDays(-41) && tasks[0].EndDate <= DateTime.UtcNow.AddDays(-35));
			Assert.AreEqual("Fred Bloggs", tasks[0].OwnerName);

			Assert.AreEqual(2, tasks[1].TaskId);
			Assert.AreEqual("Create book object insert method", tasks[1].Name);
			Assert.AreEqual("Completed", tasks[1].TaskStatusName);
			Assert.AreEqual("1 - Critical", tasks[1].TaskPriorityName);
			//Assert.IsTrue(tasks[1].StartDate >= DateTime.UtcNow.AddDays(-45) && tasks[1].StartDate <= DateTime.UtcNow.AddDays(-40));
			//Assert.IsTrue(tasks[1].EndDate >= DateTime.UtcNow.AddDays(-44) && tasks[1].EndDate <= DateTime.UtcNow.AddDays(-40));
			Assert.AreEqual("Fred Bloggs", tasks[1].OwnerName);

			Assert.AreEqual(3, tasks[2].TaskId);
			Assert.AreEqual("Write book object insert queries", tasks[2].Name);
			Assert.AreEqual("Completed", tasks[2].TaskStatusName);
			Assert.AreEqual("1 - Critical", tasks[2].TaskPriorityName);
			//Assert.IsTrue(tasks[2].StartDate >= DateTime.UtcNow.AddDays(-45) && tasks[2].StartDate <= DateTime.UtcNow.AddDays(-40));
			//Assert.IsTrue(tasks[2].EndDate >= DateTime.UtcNow.AddDays(-43) && tasks[2].EndDate <= DateTime.UtcNow.AddDays(-38));
			Assert.AreEqual("Fred Bloggs", tasks[2].OwnerName);

			//Then lets verify that we can retrieve the list of tasks associated with a release
			tasks = taskManager.RetrieveByReleaseId(PROJECT_ID, 8);
			Assert.AreEqual(6, tasks.Count);

			Assert.AreEqual(1, tasks[0].TaskId);
			Assert.AreEqual("Develop new book entry screen", tasks[0].Name);
			Assert.AreEqual("Completed", tasks[0].TaskStatusName);
			Assert.AreEqual("1 - Critical", tasks[0].TaskPriorityName);
			//Assert.IsTrue(tasks[0].StartDate >= DateTime.UtcNow.AddDays(-45) && tasks[0].StartDate <= DateTime.UtcNow.AddDays(-40));
			//Assert.IsTrue(tasks[0].EndDate >= DateTime.UtcNow.AddDays(-41) && tasks[0].EndDate <= DateTime.UtcNow.AddDays(-35));
			Assert.AreEqual("Fred Bloggs", tasks[0].OwnerName);

			Assert.AreEqual(2, tasks[1].TaskId);
			Assert.AreEqual("Create book object insert method", tasks[1].Name);
			Assert.AreEqual("Completed", tasks[1].TaskStatusName);
			Assert.AreEqual("1 - Critical", tasks[1].TaskPriorityName);
			//Assert.IsTrue(tasks[1].StartDate >= DateTime.UtcNow.AddDays(-45) && tasks[1].StartDate <= DateTime.UtcNow.AddDays(-40));
			//Assert.IsTrue(tasks[1].EndDate >= DateTime.UtcNow.AddDays(-44) && tasks[1].EndDate <= DateTime.UtcNow.AddDays(-40));
			Assert.AreEqual("Fred Bloggs", tasks[1].OwnerName);

			Assert.AreEqual(3, tasks[2].TaskId);
			Assert.AreEqual("Write book object insert queries", tasks[2].Name);
			Assert.AreEqual("Completed", tasks[2].TaskStatusName);
			Assert.AreEqual("1 - Critical", tasks[2].TaskPriorityName);
			//Assert.IsTrue(tasks[2].StartDate >= DateTime.UtcNow.AddDays(-45) && tasks[2].StartDate <= DateTime.UtcNow.AddDays(-40));
			//Assert.IsTrue(tasks[2].EndDate >= DateTime.UtcNow.AddDays(-42) && tasks[2].EndDate <= DateTime.UtcNow.AddDays(-38));
			Assert.AreEqual("Fred Bloggs", tasks[2].OwnerName);

			//Next lets verify that we can retrieve the list of tasks associated with a risk
			tasks = taskManager.RetrieveByRiskId(2);
			Assert.AreEqual(2, tasks.Count);

			Assert.AreEqual(47, tasks[0].TaskId);
			Assert.AreEqual("Plan marketing efforts for initial product release", tasks[0].Name);
			Assert.AreEqual("Not Started", tasks[0].TaskStatusName);
			Assert.AreEqual("2 - High", tasks[0].TaskPriorityName);
			Assert.AreEqual("Fred Bloggs", tasks[0].OwnerName);

			Assert.AreEqual(48, tasks[1].TaskId);
			Assert.AreEqual("Contact marketing data providers to get database of authors", tasks[1].Name);
			Assert.AreEqual("Not Started", tasks[1].TaskStatusName);
			Assert.AreEqual("3 - Medium", tasks[1].TaskPriorityName);
			Assert.AreEqual("Joe P Smith", tasks[1].OwnerName);

			//Now lets verify that we can retrieve the list of tasks NOT associated with a release
			tasks = taskManager.RetrieveByReleaseId(PROJECT_ID, null);
			Assert.AreEqual(4, tasks.Count);
			Assert.IsTrue(tasks[0].ReleaseId.IsNull());

			//Now verify that we can retrieve the details of a task by id
			taskView = taskManager.TaskView_RetrieveById(1);
			Assert.AreEqual("Develop new book entry screen", taskView.Name);
			Assert.AreEqual("Create a new dynamic page that allows the user to enter the details of a new book", taskView.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.Completed, taskView.TaskStatusId);
			Assert.AreEqual(1, taskView.TaskPriorityId);
			Assert.AreEqual(2, taskView.OwnerId);
			Assert.AreEqual(4, taskView.RequirementId);
			Assert.AreEqual(8, taskView.ReleaseId);
			//Assert.IsTrue(taskView.LastUpdateDate >= DateTime.UtcNow.AddDays(-39) && taskView.LastUpdateDate <= DateTime.UtcNow.AddDays(-35));
			//Assert.IsTrue(taskView.CreationDate >= DateTime.UtcNow.AddDays(-123) && taskView.CreationDate <= DateTime.UtcNow.AddDays(-118));
			//Assert.IsTrue(taskView.StartDate >= DateTime.UtcNow.AddDays(-45) && taskView.StartDate <= DateTime.UtcNow.AddDays(-40));
			//Assert.IsTrue(taskView.EndDate >= DateTime.UtcNow.AddDays(-39) && taskView.EndDate <= DateTime.UtcNow.AddDays(-35));
			Assert.AreEqual(100, taskView.CompletionPercent);
			Assert.AreEqual(480, taskView.EstimatedEffort);
			Assert.AreEqual(440, taskView.ActualEffort);

			//Now verify that we can retrieve the entire list of tasks for a project
			//irrespective of the release and/or requirement
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskId", true, 1, 9999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(48, tasks.Count);
			Assert.AreEqual("Develop new book entry screen", tasks[0].Name);
			Assert.AreEqual("Create a new dynamic page that allows the user to enter the details of a new book", tasks[0].Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.Completed, tasks[0].TaskStatusId);
			Assert.AreEqual(1, tasks[0].TaskPriorityId);
			Assert.AreEqual(2, tasks[0].OwnerId);
			Assert.AreEqual(4, tasks[0].RequirementId);
			Assert.AreEqual(8, tasks[0].ReleaseId);
			//Assert.IsTrue(tasks[0].LastUpdateDate >= DateTime.UtcNow.AddDays(-39) && tasks[0].LastUpdateDate <= DateTime.UtcNow.AddDays(-35));
			//Assert.IsTrue(tasks[0].CreationDate >= DateTime.UtcNow.AddDays(-125) && tasks[0].CreationDate <= DateTime.UtcNow.AddDays(-115));
			//Assert.IsTrue(tasks[0].StartDate >= DateTime.UtcNow.AddDays(-45) && tasks[0].StartDate <= DateTime.UtcNow.AddDays(-40));
			//Assert.IsTrue(tasks[0].EndDate >= DateTime.UtcNow.AddDays(-39) && tasks[0].EndDate <= DateTime.UtcNow.AddDays(-35));
			Assert.AreEqual(100, tasks[0].CompletionPercent);
			Assert.AreEqual(480, tasks[0].EstimatedEffort);
			Assert.AreEqual(440, tasks[0].ActualEffort);
			Assert.IsTrue(tasks[0].Custom_01.IsNull());
			Assert.IsTrue(tasks[0].Custom_02.IsNull());

			//Also verify that we can get the total task count for the project (used by pagination functions)
			int taskCount = taskManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(48, taskCount);

			//Finally verify that we can get the list of tasks created since a certain date
			Hashtable filters = new Hashtable();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.AddDays(-200);
			dateRange.ConsiderTimes = true;
			filters.Add("CreationDate", dateRange);
			tasks = taskManager.Retrieve(PROJECT_ID, "CreationDate", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(48, tasks.Count);

			filters.Clear();
			dateRange.Clear();
			dateRange.StartDate = DateTime.UtcNow;
			dateRange.ConsiderTimes = true;
			filters.Add("CreationDate", dateRange);
			tasks = taskManager.Retrieve(PROJECT_ID, "CreationDate", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, tasks.Count);
		}

		[
		Test,
		SpiraTestCase(318)
		]
		public void _02_InsertTask()
		{
			//Lets try inserting a new task for an existing requirement
			taskId1 = taskManager.Insert(
				PROJECT_ID,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				4,
				null,
				null,
				3,
				"Test Task 1",
				"Description of test task",
				null,
				null,
				300,
				null,
				null, USER_ID_FRED_BLOGGS);

			//Verify that it inserted correctly and that it inherited some of its properties from the parent requirement
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.AreEqual("Test Task 1", taskView.Name);
			Assert.AreEqual("Description of test task", taskView.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, taskView.TaskStatusId);
			Assert.AreEqual(3, taskView.TaskPriorityId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, taskView.CreatorId);
			Assert.AreEqual(USER_ID_JOE_SMITH, taskView.OwnerId);
			Assert.AreEqual(4, taskView.RequirementId);
			Assert.AreEqual(8, taskView.ReleaseId);
			Assert.IsFalse(taskView.StartDate.IsNull());
			Assert.IsFalse(taskView.EndDate.IsNull());
			Assert.AreEqual(0, taskView.CompletionPercent);
			Assert.AreEqual(300, taskView.EstimatedEffort);
			Assert.IsTrue(taskView.ActualEffort.IsNull());
			Assert.AreEqual(300, taskView.RemainingEffort);
			Assert.AreEqual(300, taskView.ProjectedEffort);

			//Now lets test that we can insert a new task for a release (no requirement specified)
			taskId2 = taskManager.Insert(
				PROJECT_ID,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				null,
				1,
				null,
				3,
				"Test Task 2",
				"Description of test task",
				null,
				null,
				300,
				null,
				null,
				USER_ID_FRED_BLOGGS);

			//Verify that it inserted correctly
			taskView = taskManager.TaskView_RetrieveById(taskId2);
			Assert.AreEqual("Test Task 2", taskView.Name);
			Assert.AreEqual("Description of test task", taskView.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, taskView.TaskStatusId);
			Assert.AreEqual(3, taskView.TaskPriorityId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, taskView.CreatorId);
			Assert.IsTrue(taskView.OwnerId.IsNull());
			Assert.IsTrue(taskView.RequirementId.IsNull());
			Assert.AreEqual(1, taskView.ReleaseId);
			Assert.IsFalse(taskView.StartDate.IsNull());
			Assert.IsFalse(taskView.EndDate.IsNull());
			Assert.AreEqual(0, taskView.CompletionPercent);
			Assert.AreEqual(300, taskView.EstimatedEffort);
			Assert.IsTrue(taskView.ActualEffort.IsNull());
			Assert.AreEqual(300, taskView.RemainingEffort);
			Assert.AreEqual(300, taskView.ProjectedEffort);

			//Now lets test that we can insert a new task where both release and requirement specified
			taskId3 = taskManager.Insert(
				PROJECT_ID,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				4,
				1,
				null,
				3,
				"Test Task 3",
				"Description of test task",
				null,
				null,
				300,
				null,
				null, USER_ID_FRED_BLOGGS);

			//Verify that it inserted correctly
			taskView = taskManager.TaskView_RetrieveById(taskId3);
			Assert.AreEqual("Test Task 3", taskView.Name);
			Assert.AreEqual("Description of test task", taskView.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, taskView.TaskStatusId);
			Assert.AreEqual(3, taskView.TaskPriorityId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, taskView.CreatorId);
			Assert.AreEqual(USER_ID_JOE_SMITH, taskView.OwnerId);
			Assert.AreEqual(4, taskView.RequirementId);
			Assert.AreEqual(1, taskView.ReleaseId);
			Assert.IsFalse(taskView.StartDate.IsNull());
			Assert.IsFalse(taskView.EndDate.IsNull());
			Assert.AreEqual(0, taskView.CompletionPercent);
			Assert.AreEqual(300, taskView.EstimatedEffort);
			Assert.IsTrue(taskView.ActualEffort.IsNull());
			Assert.AreEqual(300, taskView.RemainingEffort);
			Assert.AreEqual(300, taskView.ProjectedEffort);

			//Finally lets test that we can insert a new task where neither release nor requirement specified
			taskId4 = taskManager.Insert(
				PROJECT_ID,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				null,
				null,
				null,
				3,
				"Test Task 4",
				"Description of test task",
				null,
				null,
				300,
				null,
				null, USER_ID_FRED_BLOGGS);

			//Verify that it inserted correctly
			taskView = taskManager.TaskView_RetrieveById(taskId4);
			Assert.AreEqual("Test Task 4", taskView.Name);
			Assert.AreEqual("Description of test task", taskView.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, taskView.TaskStatusId);
			Assert.AreEqual(3, taskView.TaskPriorityId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, taskView.CreatorId);
			Assert.IsTrue(taskView.OwnerId.IsNull());
			Assert.IsTrue(taskView.RequirementId.IsNull());
			Assert.IsTrue(taskView.ReleaseId.IsNull());
			Assert.IsTrue(taskView.StartDate.IsNull());
			Assert.IsTrue(taskView.EndDate.IsNull());
			Assert.AreEqual(0, taskView.CompletionPercent);
			Assert.AreEqual(300, taskView.EstimatedEffort);
			Assert.IsTrue(taskView.ActualEffort.IsNull());
			Assert.AreEqual(300, taskView.RemainingEffort);
			Assert.AreEqual(300, taskView.ProjectedEffort);

			//Now test insert a new task for a release but do not use the release dates for the task dates
			taskId5 = taskManager.Insert(
				PROJECT_ID,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				null,
				1,
				null,
				3,
				"Test Task 5",
				"Description of test task",
				null,
				null,
				300,
				null,
				null,
				USER_ID_FRED_BLOGGS,
				true,
				null,
				false);

			//Verify that it inserted correctly
			taskView = taskManager.TaskView_RetrieveById(taskId5);
			Assert.AreEqual("Test Task 5", taskView.Name);
			Assert.AreEqual("Description of test task", taskView.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, taskView.TaskStatusId);
			Assert.AreEqual(3, taskView.TaskPriorityId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, taskView.CreatorId);
			Assert.IsNull(taskView.OwnerId);
			Assert.IsNull(taskView.RequirementId);
			Assert.AreEqual(1, taskView.ReleaseId);
			Assert.IsNull(taskView.StartDate);
			Assert.IsNull(taskView.EndDate);

			//Cleanup
			taskManager.MarkAsDeleted(PROJECT_ID, taskId5, USER_ID_SYS_ADMIN);
		}

		[
		Test,
		SpiraTestCase(319)
		]
		public void _03_UpdateTask()
		{
			//Get the start date of the release (dates have to be in those bounds)
			ReleaseView releaseView = new ReleaseManager().RetrieveById(USER_ID_FRED_BLOGGS, PROJECT_ID, 1);

			//Lets retrieve the newly inserted task and change it to in-progress
			task = taskManager.RetrieveById(taskId1);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			task.TaskPriorityId = 2;
			task.OwnerId = USER_ID_JOE_SMITH;
			task.ReleaseId = 1;
			startDate = releaseView.StartDate.AddDays(1);
			endDate = startDate.AddDays(5);
			task.StartDate = startDate;
			task.EndDate = endDate;
			task.RemainingEffort = 150;
			task.ActualEffort = 200;

			//Verify that the data is good
			Dictionary<string, string> messages = taskManager.Validate(task);
			Assert.AreEqual(0, messages.Count, "Validate");

			//Make the update
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify the change, and also that history entries were added
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual("Test Task 1", task.Name);
			Assert.AreEqual("Description of test task", task.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.InProgress, task.TaskStatusId);
			Assert.AreEqual(2, task.TaskPriorityId);
			Assert.AreEqual(USER_ID_JOE_SMITH, task.OwnerId);
			Assert.AreEqual(4, task.RequirementId);
			Assert.AreEqual(1, task.ReleaseId);
			Assert.AreEqual(startDate.ToShortDateString(), task.StartDate.Value.ToShortDateString());
			Assert.AreEqual(endDate.ToShortDateString(), task.EndDate.Value.ToShortDateString());
			Assert.AreEqual(50, task.CompletionPercent);
			Assert.AreEqual(300, task.EstimatedEffort);
			Assert.AreEqual(200, task.ActualEffort);
			Assert.AreEqual(150, task.RemainingEffort);
			Assert.AreEqual(350, task.ProjectedEffort);

			List<HistoryChangeSetResponse> historyChangeSets = history.RetrieveByArtifactId(taskId1, Artifact.ArtifactTypeEnum.Task);
			Assert.AreNotEqual(0, historyChangeSets.Count);

			//Lets retrieve the newly inserted task and change it to completed
			task = taskManager.RetrieveById(taskId1);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
			endDate = endDate.AddDays(1);
			task.EndDate = endDate; //The schedule slipped a day!
			task.RemainingEffort = 0;   //100%
			task.ActualEffort = 360;

			//Verify that the data is good
			messages = taskManager.Validate(task);
			Assert.AreEqual(0, messages.Count, "Validate");

			//Make the update
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify the change
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual("Test Task 1", task.Name);
			Assert.AreEqual("Description of test task", task.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.Completed, task.TaskStatusId);
			Assert.AreEqual(2, task.TaskPriorityId);
			Assert.AreEqual(USER_ID_JOE_SMITH, task.OwnerId);
			Assert.AreEqual(4, task.RequirementId);
			Assert.AreEqual(1, task.ReleaseId);
			Assert.AreEqual(startDate.ToShortDateString(), task.StartDate.Value.ToShortDateString());
			Assert.AreEqual(endDate.ToShortDateString(), task.EndDate.Value.ToShortDateString());
			Assert.AreEqual(100, task.CompletionPercent);
			Assert.AreEqual(300, task.EstimatedEffort);
			Assert.AreEqual(360, task.ActualEffort);
			Assert.AreEqual(0, task.RemainingEffort);
			Assert.AreEqual(360, task.ProjectedEffort);

			//Next verify that we can remove a task from a release
			taskManager.RemoveReleaseAssociation(new List<int> { taskId1 }, USER_ID_FRED_BLOGGS);
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.IsTrue(taskView.ReleaseId.IsNull());

			//Next verify that we can remove a task from a requirement
			taskManager.RemoveRequirementAssociation(taskId1, USER_ID_FRED_BLOGGS);
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.IsTrue(taskView.RequirementId.IsNull());

			//Now we need to verify that the validation routines catch certain issues
			//We first call the Validate() function to get the message
			//and then we test that trying to do the update regardless
			//will throw a DataValidationException
			bool validationThrown = false;
			task = taskManager.RetrieveById(1);

			//Completion percentage in range 0-100%
			task.StartTracking();
			task.CompletionPercent = 101;
			messages = taskManager.Validate(task);
			Assert.IsTrue(messages.Count > 0);
			validationThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (DataValidationException)
			{
				validationThrown = true;
			}
			Assert.IsTrue(validationThrown, "Data Validation Should have been thrown: " + messages.FirstOrDefault().Value);

			task.CompletionPercent = -2;
			messages = taskManager.Validate(task);
			Assert.IsTrue(messages.Count > 0);
			validationThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (DataValidationException)
			{
				validationThrown = true;
			}
			Assert.IsTrue(validationThrown, "Data Validation Should have been thrown: " + messages.FirstOrDefault().Value);

			//Start date can't be after end date
			task.StartDate = DateTime.Parse("1/5/2004");
			task.EndDate = DateTime.Parse("1/1/2004");
			messages = taskManager.Validate(task);
			Assert.IsTrue(messages.Count > 0);
			validationThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (DataValidationException)
			{
				validationThrown = true;
			}
			Assert.IsTrue(validationThrown, "Data Validation Should have been thrown: " + messages.FirstOrDefault().Value);

			//Can't make it in-progress without a start-date and release and %complete > 0
			task.StartTracking();
			task.StartDate = null;
			task.ReleaseId = 1;
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			messages = taskManager.Validate(task);
			Assert.IsTrue(messages.Count > 0);
			validationThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (DataValidationException)
			{
				validationThrown = true;
			}
			Assert.IsTrue(validationThrown, "Data Validation Should have been thrown: " + messages.FirstOrDefault().Value);

			task.StartTracking();
			task.ReleaseId = null;
			task.StartDate = DateTime.Parse("1/1/2004");
			messages = taskManager.Validate(task);
			Assert.IsTrue(messages.Count > 0);
			validationThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (DataValidationException)
			{
				validationThrown = true;
			}
			Assert.IsTrue(validationThrown, "Data Validation Should have been thrown: " + messages.FirstOrDefault().Value);

			//Test %complete
			task.StartTracking();
			task.ReleaseId = 1;
			task.StartDate = DateTime.Parse("1/1/2004");
			task.RemainingEffort = task.EstimatedEffort;  //0% complete
			messages = taskManager.Validate(task);
			Assert.IsTrue(messages.Count > 0, "%Complete");
			validationThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (DataValidationException)
			{
				validationThrown = true;
			}
			Assert.IsTrue(validationThrown, "Data Validation Should have been thrown: " + messages.FirstOrDefault().Value);

			//Can't make it completed without a release and actual effort
			//The end-date is now prepopulated
			task.StartTracking();
			task.StartDate = DateTime.Parse("1/1/2004");

			//Test Release Id
			task.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
			task.ReleaseId = null;
			task.EndDate = DateTime.Parse("1/10/2004");
			messages = taskManager.Validate(task);
			Assert.IsTrue(messages.Count > 0, "ReleaseId");
			validationThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (DataValidationException)
			{
				validationThrown = true;
			}
			Assert.IsTrue(validationThrown, "Data Validation Should have been thrown: " + messages.FirstOrDefault().Value);

			//Test Actual Effort
			task.StartTracking();
			task.ReleaseId = 1;
			task.EndDate = DateTime.Parse("1/10/2004");
			task.ActualEffort = null;
			messages = taskManager.Validate(task);
			Assert.IsTrue(messages.Count > 0, "ActualEffort");
			validationThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (DataValidationException)
			{
				validationThrown = true;
			}
			Assert.IsTrue(validationThrown, "Data Validation Should have been thrown: " + messages.FirstOrDefault().Value);
		}

		[
		Test,
		SpiraTestCase(320)
		]
		public void _04_DeleteTask()
		{
			//Delete the newly created tasks
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(PROJECT_ID, taskId2, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(PROJECT_ID, taskId3, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(PROJECT_ID, taskId4, USER_ID_FRED_BLOGGS);

			//Verify that it deleted them
			bool artifactExists = true;
			try
			{
				taskView = taskManager.TaskView_RetrieveById(taskId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Task 1 didn't delete correctly");
			artifactExists = true;
			try
			{
				taskView = taskManager.TaskView_RetrieveById(taskId2);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Task 2 didn't delete correctly");
			artifactExists = true;
			try
			{
				taskView = taskManager.TaskView_RetrieveById(taskId3);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Task 3 didn't delete correctly");
			artifactExists = true;
			try
			{
				taskView = taskManager.TaskView_RetrieveById(taskId4);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Task 4 didn't delete correctly");

			//Lets create a new requirement, release and task and link them
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, null, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Requirement", null, 0.25M, USER_ID_FRED_BLOGGS);
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_ID, USER_ID_FRED_BLOGGS, "Test Release", null, "2.0.0.1", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 2, 0, null, false);
			int taskId = taskManager.Insert(PROJECT_ID, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId, releaseId, USER_ID_FRED_BLOGGS, null, "Test Task", null, null, null, null, null, null, USER_ID_FRED_BLOGGS);

			//Now we need to verify that deleting a release that has an attached task, delinks the task first
			artifactExists = true;
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, releaseId);
			try
			{
				taskView = taskManager.TaskView_RetrieveById(taskId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsTrue(artifactExists, "Deleting release shouldn't delete the task (just delink)");

			//Now we need to verify that deleting a requirement that has an attached task delinks the task first
			artifactExists = true;
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_ID, requirementId);
			try
			{
				taskView = taskManager.TaskView_RetrieveById(taskId);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsTrue(artifactExists, "Deleting the requirement shouldn't delete the task (just delink)");

			//Now clean up by deleting that task
			taskManager.MarkAsDeleted(PROJECT_ID, taskId, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(328)
		]
		public void _05_RetrieveSummaryData()
		{
			//Get the list of "open" tasks owned by a particular user - cross project
			tasks = taskManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, null, null, false);
			Assert.AreEqual(5, tasks.Count);
			Assert.AreEqual("Schedule meeting with customer to discuss scope", tasks[0].Name);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, tasks[0].TaskStatusId);
			Assert.AreEqual("Plan marketing efforts for initial product release", tasks[1].Name);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, tasks[1].TaskStatusId);
			Assert.AreEqual("Library Information System (Sample)", tasks[1].ProjectName);

			//Get the list of "open" tasks owned by a particular user - specific project
			tasks = taskManager.RetrieveByOwnerId(USER_ID_FRED_BLOGGS, PROJECT_ID, null, false);
			Assert.AreEqual(5, tasks.Count);
			Assert.AreEqual("Schedule meeting with customer to discuss scope", tasks[0].Name);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, tasks[0].TaskStatusId);
			Assert.AreEqual("Plan marketing efforts for initial product release", tasks[1].Name);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, tasks[1].TaskStatusId);
			Assert.AreEqual("Library Information System (Sample)", tasks[1].ProjectName);

			//Get the list of "open" tasks not owned by anyone
			tasks = taskManager.RetrieveByOwnerId(null, PROJECT_ID, null, false);
			Assert.AreEqual(3, tasks.Count);
			Assert.AreEqual("Refactor author details page to include contact info", tasks[0].Name);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, tasks[0].TaskStatusId);
			Assert.AreEqual("Update author select query to include contact info", tasks[1].Name);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, tasks[1].TaskStatusId);
			Assert.AreEqual("Library Information System (Sample)", tasks[1].ProjectName);

			//Get the list of "open" tasks not owned by anyone in a specific release
			tasks = taskManager.RetrieveByOwnerId(null, PROJECT_ID, 1, false);
			Assert.AreEqual(0, tasks.Count);

			//Get the task progress summary for a project group as a whole and its constituent projects
			List<Task_GroupSummary> summaryResults = taskManager.RetrieveProgressSummary(2, false);
			List<ProjectTaskProgressEntryView> projectResults = taskManager.RetrieveProgressByProject(2, false);

			//First verify the group summary
			Assert.AreEqual(4, summaryResults.Count);
			Assert.AreEqual("On Schedule", summaryResults[0].ProgressCaption);
			Assert.IsTrue(summaryResults[0].TaskCount > 0);
			Assert.AreEqual("Late Finish", summaryResults[1].ProgressCaption);
			Assert.IsTrue(summaryResults[1].TaskCount == 0);
			Assert.AreEqual("Late Start", summaryResults[2].ProgressCaption);
			Assert.IsTrue(summaryResults[2].TaskCount > 0);
			Assert.AreEqual("Not Started", summaryResults[3].ProgressCaption);
			Assert.IsTrue(summaryResults[3].TaskCount > 0);

			//Now verify the project-level detail
			Assert.AreEqual(3, projectResults.Count);
			Assert.AreEqual("Library Information System (Sample)", projectResults[0].ProjectName);
			Assert.IsTrue(projectResults[0].TaskCount > 0);
			Assert.IsTrue(projectResults[0].TaskPercentOnTime > 0);
			Assert.IsTrue(projectResults[0].TaskPercentLateFinish == 0);
			Assert.IsTrue(projectResults[0].TaskPercentNotStart > 0);
			Assert.IsTrue(projectResults[0].TaskPercentLateStart > 0);
			Assert.IsTrue(projectResults[0].TaskEstimatedEffort > 0);
			Assert.IsTrue(projectResults[0].TaskActualEffort > 0);
			Assert.IsTrue(projectResults[0].TaskRemainingEffort > 0);
			Assert.IsTrue(projectResults[0].TaskProjectedEffort > 0);

			//Get the task progress summary for a project group as a whole and its constituent projects - active releases only
			summaryResults = taskManager.RetrieveProgressSummary(2, true);
			projectResults = taskManager.RetrieveProgressByProject(2, true);

			//First verify the group summary
			Assert.AreEqual(4, summaryResults.Count);
			Assert.AreEqual("On Schedule", summaryResults[0].ProgressCaption);
			Assert.IsTrue(summaryResults[0].TaskCount > 0);
			Assert.AreEqual("Late Finish", summaryResults[1].ProgressCaption);
			Assert.IsTrue(summaryResults[1].TaskCount == 0);
			Assert.AreEqual("Late Start", summaryResults[2].ProgressCaption);
			Assert.IsTrue(summaryResults[2].TaskCount > 0);
			Assert.AreEqual("Not Started", summaryResults[3].ProgressCaption);
			Assert.IsTrue(summaryResults[3].TaskCount > 0);

			////Now verify the project-level detail
			Assert.AreEqual(3, projectResults.Count);
			Assert.AreEqual("Library Information System (Sample)", projectResults[0].ProjectName);
			Assert.IsTrue(projectResults[0].TaskCount > 0);
			Assert.IsTrue(projectResults[0].TaskPercentOnTime > 0);
			Assert.IsTrue(projectResults[0].TaskPercentLateFinish == 0);
			Assert.IsTrue(projectResults[0].TaskPercentNotStart > 0);
			Assert.IsTrue(projectResults[0].TaskPercentLateStart > 0);
			Assert.IsTrue(projectResults[0].TaskEstimatedEffort > 0);
			Assert.IsTrue(projectResults[0].TaskActualEffort > 0);
			Assert.IsTrue(projectResults[0].TaskRemainingEffort > 0);
			Assert.IsTrue(projectResults[0].TaskProjectedEffort > 0);

			////Also verify that we can retrieve for a single project
			ProjectTaskProgressEntryView projectTaskProgressEntry = taskManager.RetrieveProjectProgressSummary(1);
			Assert.AreEqual("Library Information System (Sample)", projectTaskProgressEntry.ProjectName);
			Assert.IsTrue(projectTaskProgressEntry.TaskCount > 0);
			Assert.IsTrue(projectTaskProgressEntry.TaskPercentOnTime > 0);
			Assert.IsTrue(projectTaskProgressEntry.TaskPercentLateFinish == 0);
			Assert.IsTrue(projectTaskProgressEntry.TaskPercentNotStart > 0);
			Assert.IsTrue(projectTaskProgressEntry.TaskPercentLateStart > 0);
			Assert.IsTrue(projectTaskProgressEntry.TaskEstimatedEffort > 0);
			Assert.IsTrue(projectTaskProgressEntry.TaskActualEffort > 0);
			Assert.IsTrue(projectTaskProgressEntry.TaskRemainingEffort > 0);
			Assert.IsTrue(projectTaskProgressEntry.TaskProjectedEffort > 0);
		}

		[
		Test,
		SpiraTestCase(359)
		]
		public void _06_FilterAndSort()
		{
			//No filter, sort by task id
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskId", true, 1, 9999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(48, tasks.Count);
			Assert.AreEqual("Develop new book entry screen", tasks[0].Name);

			//Status=Not Started, Priority=High, Sort by Name
			Hashtable filters = new Hashtable();
			filters.Add("TaskStatusId", (int)Task.TaskStatusEnum.NotStarted);
			filters.Add("TaskPriorityId", 2);
			tasks = taskManager.Retrieve(PROJECT_ID, "Name", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, tasks.Count);
			Assert.AreEqual("Create author object update method", tasks[0].Name);

			//Name LIKE subject, Owner=Fred Bloggs, Sort by Priority
			filters.Clear();
			filters.Add("Name", "subject");
			filters.Add("OwnerId", USER_ID_FRED_BLOGGS);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityId", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(10, tasks.Count);
			Assert.AreEqual("1 - Critical", tasks[0].TaskPriorityName);

			//Name LIKE subject, Owner=Fred Bloggs, Sort by Priority Descending
			filters.Clear();
			filters.Add("Name", "subject");
			filters.Add("OwnerId", USER_ID_FRED_BLOGGS);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityId", false, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(10, tasks.Count);
			Assert.AreEqual("3 - Medium", tasks[0].TaskPriorityName);

			//Now test that we can filter/sort by dates
			filters.Clear();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.Date.AddDays(-45);
			dateRange.EndDate = DateTime.UtcNow.Date.AddDays(-40);
			filters.Add("StartDate", dateRange);
			tasks = taskManager.Retrieve(PROJECT_ID, "EndDate", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.IsTrue(tasks.Count > 0);
			//Assert.IsTrue(tasks[0].StartDate.Value.Date >= DateTime.UtcNow.AddDays(-45) && tasks[0].StartDate.Value.Date <= DateTime.UtcNow.AddDays(-40));

			//Now test that we can filter by id (bug found in v2.0.1)
			filters.Clear();
			filters.Add("TaskId", "1");
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskId", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, tasks.Count);
			Assert.AreEqual(1, tasks[0].TaskId);

			//Now test that we can filter for all tasks that are not associated by release
			//and have not started. Used in the Iteration Plan screen
			filters.Clear();
			filters.Add("TaskStatusId", (int)Task.TaskStatusEnum.NotStarted);
			filters.Add("ReleaseId", TaskManager.NoneFilterValue);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 999999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, tasks.Count);
			Assert.AreEqual(30, tasks[0].TaskId);
			Assert.AreEqual(31, tasks[1].TaskId);
			Assert.AreEqual(32, tasks[2].TaskId);
			Assert.AreEqual(43, tasks[3].TaskId);

			//Now lets test that we can use a multi-valued filter
			filters.Clear();
			MultiValueFilter multiFilterValue = new MultiValueFilter();
			multiFilterValue.Values.Add(3);
			multiFilterValue.Values.Add(4);
			filters.Add("TaskPriorityId", multiFilterValue);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskId", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, tasks.Count);

			//Finally test that we can filter by the progress indicator
			int percentGreen;
			int percentRed;
			int percentYellow;
			int percentGray;
			//Not Started
			filters.Clear();
			filters.Add("ProgressId", 1);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, tasks.Count);
			//Assert.AreEqual(39, tasks[0].TaskId);
			//Assert.AreEqual(38, tasks[1].TaskId);
			//Assert.AreEqual(39, tasks[2].TaskId);
			//Assert.AreEqual(40, tasks[3].TaskId);

			//Starting Late
			filters.Clear();
			filters.Add("ProgressId", 2);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(10, tasks.Count);
			Assert.AreEqual(0, tasks[0].CompletionPercent);
			Assert.IsTrue(tasks[0].StartDate < DateTime.UtcNow);
			TaskManager.CalculateProgress(tasks[0].ConvertTo<TaskView, Task>(), InternalRoutines.UTC_OFFSET, out percentGreen, out percentRed, out percentYellow, out percentGray);
			Assert.AreEqual(100, percentYellow);

			//On Schedule
			filters.Clear();
			filters.Add("ProgressId", 3);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, tasks.Count);

			//Running Late
			filters.Clear();
			filters.Add("ProgressId", 4);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, tasks.Count);

			//Completed
			filters.Clear();
			filters.Add("ProgressId", 5);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(30, tasks.Count);
			Assert.AreEqual(100, tasks[0].CompletionPercent);
			Assert.AreEqual("Completed", tasks[0].TaskStatusName);
			TaskManager.CalculateProgress(tasks[0].ConvertTo<TaskView, Task>(), InternalRoutines.UTC_OFFSET, out percentGreen, out percentRed, out percentYellow, out percentGray);
			Assert.AreEqual(100, percentGreen);

			//Now lets verify that we can retrieve/update the filters and pagination options for tasks
			//Filters
			ProjectSettingsCollection projectSettingsCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_FRED_BLOGGS, "TaskFiltersList");
			Assert.AreEqual(0, projectSettingsCollection.Count);
			projectSettingsCollection.Add("OwnerId", USER_ID_FRED_BLOGGS);
			projectSettingsCollection.Save();
			projectSettingsCollection.Restore();
			Assert.AreEqual(1, projectSettingsCollection.Count);
			//Now reset back
			projectSettingsCollection.Clear();
			projectSettingsCollection.Save();
			projectSettingsCollection.Restore();
			Assert.AreEqual(0, projectSettingsCollection.Count);

			//Sort Expression
			projectSettingsCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_FRED_BLOGGS, "TaskSortExpression");
			Assert.AreEqual(0, projectSettingsCollection.Count);
			projectSettingsCollection.Add("SortExpression", "Name ASC");
			projectSettingsCollection.Save();
			projectSettingsCollection.Restore();
			Assert.AreEqual(1, projectSettingsCollection.Count);
			//Now reset back
			projectSettingsCollection.Clear();
			projectSettingsCollection.Save();
			projectSettingsCollection.Restore();
			Assert.AreEqual(0, projectSettingsCollection.Count);

			//Pagination Size
			projectSettingsCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_FRED_BLOGGS, "TaskPaginationSize");
			Assert.AreEqual(0, projectSettingsCollection.Count);
			projectSettingsCollection.Add("NumberRowsPerPage", 40);
			projectSettingsCollection.Save();
			projectSettingsCollection.Restore();
			Assert.AreEqual(1, projectSettingsCollection.Count);
			//Now reset back
			projectSettingsCollection.Clear();
			projectSettingsCollection.Save();
			projectSettingsCollection.Restore();
			Assert.AreEqual(0, projectSettingsCollection.Count);

			//Now lets test that we can filter by Release, which will include any child iterations
			filters.Clear();
			filters.Add("ReleaseId", 1);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(20, tasks.Count);

			//Now lets tets that we can filter by component (which is actually the requirement's component)
			filters.Clear();
			multiFilterValue = new MultiValueFilter();
			multiFilterValue.Values.Add(1);
			filters.Add("ComponentId", multiFilterValue);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(29, tasks.Count);
			Assert.AreEqual(1, tasks[0].ComponentId);
			Assert.AreEqual("Book Management", tasks[0].ComponentName);

			//Now filter by two components
			filters.Clear();
			multiFilterValue = new MultiValueFilter();
			multiFilterValue.Values.Add(1);
			multiFilterValue.Values.Add(2);
			filters.Add("ComponentId", multiFilterValue);
			tasks = taskManager.Retrieve(PROJECT_ID, "TaskPriorityName", true, 1, 9999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(42, tasks.Count);
			Assert.AreEqual(1, tasks[0].ComponentId);
			Assert.AreEqual("Book Management", tasks[0].ComponentName);
		}

		[
		Test,
		SpiraTestCase(360)
		]
		public void _07_ShowHideColumns()
		{
			//First get a list of the currently hidden/visible columns
			ArtifactManager artifactManager = new ArtifactManager();
			List<ArtifactListFieldDisplay> artifactFields = artifactManager.ArtifactField_RetrieveForLists(PROJECT_ID, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(22, artifactFields.Count);
			Assert.AreEqual("StartDate", artifactFields[0].Name);

			//Now make StartDate visible
			artifactManager.ArtifactField_ToggleListVisibility(PROJECT_ID, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Task, "StartDate");
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(PROJECT_ID, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(22, artifactFields.Count);
			Assert.AreEqual("EndDate", artifactFields[0].Name);

			//Now make StartDate hidden again
			artifactManager.ArtifactField_ToggleListVisibility(PROJECT_ID, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Task, "StartDate");
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(PROJECT_ID, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(22, artifactFields.Count);
			Assert.AreEqual("StartDate", artifactFields[0].Name);
		}

		[
		Test,
		SpiraTestCase(384)
		]
		public void _08_IterationScheduling()
		{
			//This tests that we can associate tasks with an iteration and have the start/end dates populate
			//Also once a task is associated with a release/iteration its start/end dates cannot be modified
			//outside the range set by the iteration/release itself

			//First create a new iteration inside the empty project
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_EMPTY_ID, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.0.0000", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);

			//Now lets create a couple of unassociated tasks
			taskId1 = taskManager.Insert(PROJECT_EMPTY_ID, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, USER_ID_FRED_BLOGGS, 6, "Task 1", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);
			taskId2 = taskManager.Insert(PROJECT_EMPTY_ID, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, USER_ID_FRED_BLOGGS, 6, "Task 1", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);

			//Verify that the tasks are unassociated
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.IsTrue(taskView.ReleaseId.IsNull());
			Assert.IsTrue(taskView.StartDate.IsNull());
			Assert.IsTrue(taskView.EndDate.IsNull());

			taskView = taskManager.TaskView_RetrieveById(taskId2);
			Assert.IsTrue(taskView.ReleaseId.IsNull());
			Assert.IsTrue(taskView.StartDate.IsNull());
			Assert.IsTrue(taskView.EndDate.IsNull());

			//Now associate the tasks with the iteration
			taskManager.AssociateToIteration(new List<int> { taskId1, taskId2 }, releaseId, USER_ID_FRED_BLOGGS);

			//Verify that they are associated and that the start dates were set
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.AreEqual(releaseId, taskView.ReleaseId);
			Assert.AreEqual(DateTime.Parse("4/1/2004"), taskView.StartDate);
			Assert.AreEqual(DateTime.Parse("4/30/2004"), taskView.EndDate);

			taskView = taskManager.TaskView_RetrieveById(taskId2);
			Assert.AreEqual(releaseId, taskView.ReleaseId);
			Assert.AreEqual(DateTime.Parse("4/1/2004"), taskView.StartDate);
			Assert.AreEqual(DateTime.Parse("4/30/2004"), taskView.EndDate);

			//Now we need to verify that setting the dates of the task beyond that of the iteration (1-day allowed) throws an exception
			//Start Date
			task = taskManager.RetrieveById(taskId2);
			task.StartTracking();
			task.StartDate = DateTime.Parse("3/28/2004");
			bool exceptionThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (TaskDateOutOfBoundsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "TaskDateOutOfBoundsException not thrown");

			//End Date
			task = taskManager.RetrieveById(taskId2);
			task.StartTracking();
			task.EndDate = DateTime.Parse("5/3/2004");
			exceptionThrown = false;
			try
			{
				taskManager.Update(task, USER_ID_FRED_BLOGGS);
			}
			catch (TaskDateOutOfBoundsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "TaskDateOutOfBoundsException not thrown");

			//Finally verify that we can deassociate the tasks
			taskManager.RemoveReleaseAssociation(new List<int> { taskId1, taskId2 }, USER_ID_FRED_BLOGGS);

			//Verify that the tasks are unassociated
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.IsTrue(taskView.ReleaseId.IsNull());
			taskView = taskManager.TaskView_RetrieveById(taskId2);
			Assert.IsTrue(taskView.ReleaseId.IsNull());

			//Test that we can assign a task to a user (used in planning board)
			taskManager.AssignToUser(taskId1, USER_ID_JOE_SMITH, USER_ID_FRED_BLOGGS);
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.AreEqual(USER_ID_JOE_SMITH, taskView.OwnerId);

			//Test that we can unassign the task
			taskManager.AssignToUser(taskId1, null, USER_ID_FRED_BLOGGS);
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.IsTrue(taskView.OwnerId.IsNull());

			//Clean up
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(PROJECT_ID, taskId2, USER_ID_FRED_BLOGGS);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_EMPTY_ID, releaseId);
		}

		/// <summary>
		/// Tests that we can copy tasks within the same project
		/// </summary>
		[
		Test,
		SpiraTestCase(397)
		]
		public void _10_CopyTasks()
		{
			//Lets make a copy of an existing task, then verify that the details copied across correctly.
			taskId1 = taskManager.Copy(USER_ID_FRED_BLOGGS, 2);

			//Verify the data
			taskView = taskManager.TaskView_RetrieveById(taskId1);

			//Task Details
			Assert.AreEqual(PROJECT_ID, taskView.ProjectId);
			Assert.AreEqual("Create book object insert method - Copy", taskView.Name);
			Assert.AreEqual("Code the business object that inserts a new book row in the database", taskView.Description);
			Assert.AreEqual(1, taskView.TaskPriorityId);
			Assert.AreEqual(4, taskView.RequirementId);
			Assert.AreEqual(8, taskView.ReleaseId);
			Assert.AreEqual((int)Task.TaskStatusEnum.Completed, taskView.TaskStatusId);
			Assert.AreEqual(100, taskView.CompletionPercent);
			Assert.AreEqual(300, taskView.EstimatedEffort);
			Assert.AreEqual(320, taskView.ActualEffort);

			//Now clean up by deleting the task
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, USER_ID_FRED_BLOGGS);

			//Verify the risk id field gets properly copied over
			taskId1 = taskManager.Copy(USER_ID_FRED_BLOGGS, 44);
			taskView = taskManager.TaskView_RetrieveById(taskId1);
			Assert.AreEqual(1, taskView.RiskId);

			//Now clean up by deleting the task
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, USER_ID_FRED_BLOGGS);

			//Verify we can create a copy but in a different folder and without appending anything to the name
			taskId1 = taskManager.Copy(USER_ID_FRED_BLOGGS, 2, 4, false);
			taskView = taskManager.TaskView_RetrieveById(taskId1);

			//Task Details
			Assert.AreEqual(PROJECT_ID, taskView.ProjectId);
			Assert.AreEqual("Create book object insert method", taskView.Name);
			Assert.AreEqual(4, taskView.TaskFolderId);
			Assert.AreEqual(8, taskView.ReleaseId);

			//Now clean up by deleting the task
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that we can export tasks between two projects
		/// </summary>
		[
		Test,
		SpiraTestCase(396)
		]
		public void _11_ExportTasks()
		{
			//Lets make a copy of an existing task, then verify that the details exported correctly.
			taskId1 = taskManager.Export(2, PROJECT_EMPTY_ID, USER_ID_FRED_BLOGGS);

			//Verify the data
			taskView = taskManager.TaskView_RetrieveById(taskId1);

			//Task Details
			Assert.AreEqual(PROJECT_EMPTY_ID, taskView.ProjectId);
			Assert.AreEqual("Create book object insert method", taskView.Name);
			Assert.AreEqual("Code the business object that inserts a new book row in the database", taskView.Description);
			Assert.AreEqual(1, taskView.TaskPriorityId); //Will be the same since same template used by projects
			Assert.IsTrue(taskView.RequirementId.IsNull());
			Assert.IsTrue(taskView.ReleaseId.IsNull());
			Assert.AreEqual((int)Task.TaskStatusEnum.Completed, taskView.TaskStatusId);
			Assert.AreEqual(100, taskView.CompletionPercent);
			Assert.AreEqual(300, taskView.EstimatedEffort);
			Assert.AreEqual(320, taskView.ActualEffort);

			//Export a folder of tasks
			int folderId1 = taskManager.TaskFolder_Create("Exporting Tasks", projectId).TaskFolderId;
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.InProgress, null, folderId1, null, null, USER_ID_FRED_BLOGGS, null, "Write screen X", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);
			int taskId3 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.Completed, null, folderId1, null, null, USER_ID_FRED_BLOGGS, null, "Write screen Y", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);

			Dictionary<int, int> taskFolderMapping = new Dictionary<int, int>();
			int destinationFolderId = taskManager.TaskFolder_Export(USER_ID_FRED_BLOGGS, projectId, folderId1, PROJECT_EMPTY_ID, taskFolderMapping);

			//Verify
			List<TaskView> sourceTasks = taskManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, 0, folderId1);
			List<TaskView> destinationTasks = taskManager.Retrieve(PROJECT_EMPTY_ID, "Name", true, 1, Int32.MaxValue, null, 0, destinationFolderId);
			Assert.AreEqual(sourceTasks.Count, destinationTasks.Count);
			Assert.AreEqual(sourceTasks[0].Name, destinationTasks[0].Name);
			Assert.AreEqual(sourceTasks[0].TaskStatusId, sourceTasks[0].TaskStatusId);
			Assert.AreEqual(sourceTasks[1].Name, destinationTasks[1].Name);
			Assert.AreEqual(sourceTasks[1].TaskStatusId, sourceTasks[1].TaskStatusId);

			//Clean up
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId3, USER_ID_FRED_BLOGGS);
			taskManager.TaskFolder_Delete(projectId, folderId1);

			taskManager.MarkAsDeleted(PROJECT_EMPTY_ID, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(PROJECT_EMPTY_ID, sourceTasks[0].TaskId, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(PROJECT_EMPTY_ID, sourceTasks[1].TaskId, USER_ID_FRED_BLOGGS);
			taskManager.TaskFolder_Delete(PROJECT_EMPTY_ID, destinationFolderId);
		}

		/// <summary>
		/// Verify that when we update the task, certain fields get automatically set for us
		/// </summary>
		[
		Test,
		SpiraTestCase(441)
		]
		public void _12_PrefillFields()
		{
			//First create new iterations
			ReleaseManager releaseManager = new ReleaseManager();
			int iterationId1 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				PROJECT_EMPTY_ID,
				USER_ID_FRED_BLOGGS,
				"New Iteration",
				"",
				"1.0.0.0 0050",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.UtcNow.Date,
				DateTime.UtcNow.AddDays(5).Date,
				1,
				0,
				null,
				false
				);

			int iterationId2 = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				PROJECT_EMPTY_ID,
				USER_ID_FRED_BLOGGS,
				"New Iteration 2",
				"",
				"1.0.0.0 0051",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.UtcNow.AddDays(6).Date,
				DateTime.UtcNow.AddDays(10).Date,
				1,
				0,
				null,
				false
				);

			//First create a new task
			taskId1 = taskManager.Insert(
				PROJECT_EMPTY_ID,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				null,
				null,
				null,
				7,
				"Test Task 1",
				"Description of test task",
				null,
				null,
				300,
				null,
				null,
				USER_ID_FRED_BLOGGS);

			//Verify that it inserted correctly
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual("Test Task 1", task.Name);
			Assert.AreEqual("Description of test task", task.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, task.TaskStatusId);
			Assert.AreEqual(7, task.TaskPriorityId);
			Assert.IsTrue(task.OwnerId.IsNull());
			Assert.IsTrue(task.ReleaseId.IsNull());
			Assert.IsTrue(task.StartDate.IsNull());
			Assert.IsTrue(task.EndDate.IsNull());
			Assert.AreEqual(0, task.CompletionPercent);
			Assert.AreEqual(300, task.EstimatedEffort);
			Assert.IsTrue(task.ActualEffort.IsNull());
			Assert.AreEqual(300, task.ProjectedEffort);
			Assert.AreEqual(300, task.RemainingEffort);

			//First if we set the task to In-Progress, it will automatically set the remaining effort to the estimated
			//and assign it the current start date
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			task.ReleaseId = iterationId1;
			Assert.AreEqual("", taskManager.Validate(task), "Validation Failed");
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify that it prefilled correctly
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual("Test Task 1", task.Name);
			Assert.AreEqual("Description of test task", task.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.InProgress, task.TaskStatusId);
			Assert.AreEqual(iterationId1, task.ReleaseId);
			Assert.AreEqual(DateTime.UtcNow.Date, task.StartDate.Value.Date, "StartDate1");
			Assert.AreEqual(DateTime.UtcNow.Date, task.EndDate.Value.Date, "EndDate1");
			Assert.AreEqual(0, task.CompletionPercent);
			Assert.AreEqual(300, task.EstimatedEffort);
			Assert.IsTrue(task.ActualEffort.IsNull());
			Assert.AreEqual(300, task.ProjectedEffort);
			Assert.AreEqual(300, task.RemainingEffort);

			//If we change it to a different iteration/release, then it will take on the start
			//date of that iteration (using the Update method) if the dates are not inside the new iteration
			//It will keep the date range as long
			//as the end-date doesn't exceed the end date of the iteration
			task.StartTracking();
			task.EndDate = DateTime.UtcNow.AddDays(3).Date;
			task.ReleaseId = iterationId2;
			Assert.AreEqual("", taskManager.Validate(task), "Validation Failed");
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify that it prefilled correctly
			task = taskManager.RetrieveById(taskId1);

			Assert.AreEqual("Test Task 1", task.Name);
			Assert.AreEqual("Description of test task", task.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.InProgress, task.TaskStatusId);
			Assert.AreEqual(iterationId2, task.ReleaseId);
			Assert.AreEqual(DateTime.UtcNow.AddDays(6).Date, task.StartDate.Value.Date, "StartDate2");
			Assert.AreEqual(DateTime.UtcNow.AddDays(9).Date, task.EndDate.Value.Date, "EndDate2");
			Assert.AreEqual(0, task.CompletionPercent);
			Assert.AreEqual(300, task.EstimatedEffort);
			Assert.IsTrue(task.ActualEffort.IsNull());
			Assert.AreEqual(300, task.ProjectedEffort);
			Assert.AreEqual(300, task.RemainingEffort);

			//Now move it back
			task.StartTracking();
			task.ReleaseId = iterationId1;
			Assert.AreEqual("", taskManager.Validate(task), "Validation Failed");
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify that it prefilled correctly
			task = taskManager.RetrieveById(taskId1);

			Assert.AreEqual("Test Task 1", task.Name);
			Assert.AreEqual("Description of test task", task.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.InProgress, task.TaskStatusId);
			Assert.AreEqual(iterationId1, task.ReleaseId);
			Assert.AreEqual(DateTime.UtcNow.Date, task.StartDate.Value.Date, "StartDate3");
			Assert.AreEqual(DateTime.UtcNow.AddDays(3).Date, task.EndDate.Value.Date, "EndDate3");
			Assert.AreEqual(0, task.CompletionPercent);
			Assert.AreEqual(300, task.EstimatedEffort);
			Assert.IsTrue(task.ActualEffort.IsNull());
			Assert.AreEqual(300, task.ProjectedEffort);
			Assert.AreEqual(300, task.RemainingEffort);

			//Next if we set the task to Completed, it will automatically set the progress to 100%
			//And assign it the current date as the end date if it doesn't have one
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
			task.StartDate = DateTime.UtcNow.Date;
			task.EndDate = null;
			task.ActualEffort = 350;
			Assert.AreEqual("", taskManager.Validate(task), "Validation Failed");
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify that it prefilled correctly
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual("Test Task 1", task.Name);
			Assert.AreEqual("Description of test task", task.Description);
			Assert.AreEqual((int)Task.TaskStatusEnum.Completed, task.TaskStatusId);
			Assert.AreEqual(iterationId1, task.ReleaseId);
			Assert.AreEqual(DateTime.UtcNow.Date, task.StartDate.Value.Date, "StartDate4");
			Assert.AreEqual(DateTime.UtcNow.Date, task.EndDate.Value.Date, "EndDate4");
			Assert.AreEqual(100, task.CompletionPercent);
			Assert.AreEqual(300, task.EstimatedEffort);
			Assert.AreEqual(350, task.ActualEffort);
			Assert.AreEqual(350, task.ProjectedEffort);
			Assert.AreEqual(0, task.RemainingEffort);

			//Clean up
			taskManager.MarkAsDeleted(PROJECT_ID, taskId1, USER_ID_FRED_BLOGGS);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_EMPTY_ID, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_EMPTY_ID, iterationId2);
		}

		/// <summary>
		/// Verify that multiple, concurrent updates to tasks are handled correctly
		/// in accordance with optimistic concurrency rules
		/// </summary>
		[
		Test,
		SpiraTestCase(581)
		]
		public void _13_Concurrency_Handling()
		{
			//First we need to create a new task to verify the handling
			int taskId = taskManager.Insert(
				PROJECT_EMPTY_ID,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				null,
				null,
				null,
				7,
				"Test Task 1",
				"Description of test task",
				null,
				null,
				300,
				null,
				null, USER_ID_FRED_BLOGGS);

			//Now retrieve the task back and keep a copy of the dataset
			Task task1 = taskManager.RetrieveById(taskId);
			Task task2 = ArtifactHelper.Clone<Task>(task1);

			//Now make a change to field and update
			task1.StartTracking();
			task1.TaskPriorityId = 6;
			taskManager.Update(task1, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			Task task3 = taskManager.RetrieveById(taskId);
			Assert.AreEqual(6, task3.TaskPriorityId);

			//Now try making a change using the out of date dataset (has the wrong ConcurrencyDate)
			bool exceptionThrown = false;
			try
			{
				task2.StartTracking();
				task2.TaskPriorityId = 8;
				taskManager.Update(task2, USER_ID_FRED_BLOGGS);
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate entity
			task3 = taskManager.RetrieveById(taskId);
			Assert.AreEqual(6, task3.TaskPriorityId);

			//Now refresh the old entity and try again and verify it works
			task2 = taskManager.RetrieveById(taskId);
			task2.StartTracking();
			task2.TaskPriorityId = 8;
			taskManager.Update(task2, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate entity
			task3 = taskManager.RetrieveById(taskId);
			Assert.AreEqual(8, task3.TaskPriorityId);

			//Clean up
			taskManager.MarkAsDeleted(PROJECT_EMPTY_ID, taskId, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that you can create, modify, delete and retrieve task folders
		/// </summary>
		[
		Test,
		SpiraTestCase(1267)
		]
		public void _14_CreateModifyViewTaskFolders()
		{
			//First lets verify that there are no tasks in the project initially
			List<TaskFolderHierarchyView> folderHierarchy = taskManager.TaskFolder_GetList(projectId);
			Assert.AreEqual(0, folderHierarchy.Count);

			//Now lets create a simple folder hierarchy
			int folderId1 = taskManager.TaskFolder_Create("Development Tasks", projectId).TaskFolderId;
			int folderId2 = taskManager.TaskFolder_Create("Testing Tasks", projectId).TaskFolderId;
			int folderId3 = taskManager.TaskFolder_Create("Other Tasks", projectId).TaskFolderId;
			int folderId4 = taskManager.TaskFolder_Create("Coding", projectId, folderId1).TaskFolderId;
			int folderId5 = taskManager.TaskFolder_Create("Design", projectId, folderId1).TaskFolderId;
			int folderId6 = taskManager.TaskFolder_Create("Functional Testing", projectId, folderId2).TaskFolderId;
			int folderId7 = taskManager.TaskFolder_Create("Performance Testing", projectId, folderId2).TaskFolderId;
			int folderId8 = taskManager.TaskFolder_Create("Speed Test", projectId, folderId7).TaskFolderId;

			//Verify that the hierarchy was created as expected
			folderHierarchy = taskManager.TaskFolder_GetList(projectId);
			Assert.AreEqual(8, folderHierarchy.Count, "taskManager.TaskFolder_GetList returned incorrect folder count.");
			Assert.AreEqual("Development Tasks", folderHierarchy[0].Name, "taskManager.TaskFolder_GetList returned incorrect name of Development Tasks Task folder.");
			Assert.AreEqual(1, folderHierarchy[0].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number Tasks folder hierarchyLevel.");
			Assert.AreEqual("Coding", folderHierarchy[1].Name, "taskManager.TaskFolder_GetList returned incorrect name of Coding Task folder.");
			Assert.AreEqual(2, folderHierarchy[1].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Design", folderHierarchy[2].Name, "taskManager.TaskFolder_GetList returned incorrect name of Design Task folder.");
			Assert.AreEqual(2, folderHierarchy[2].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Other Tasks", folderHierarchy[3].Name, "taskManager.TaskFolder_GetList returned incorrect name of a Other Tasks Tasks folder.");
			Assert.AreEqual(1, folderHierarchy[3].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Testing Tasks", folderHierarchy[4].Name, "taskManager.TaskFolder_GetList returned incorrect name of  Testing Tasks Task folder.");
			Assert.AreEqual(1, folderHierarchy[4].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Functional Testing", folderHierarchy[5].Name, "taskManager.TaskFolder_GetList returned incorrect name of  Functional Testing Tasks folder.");
			Assert.AreEqual(2, folderHierarchy[5].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Performance Testing", folderHierarchy[6].Name, "taskManager.TaskFolder_GetList returned incorrect name of  Performance Testing Tasks folder.");
			Assert.AreEqual(2, folderHierarchy[6].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Speed Test", folderHierarchy[7].Name, "taskManager.TaskFolder_GetList returned incorrect name of  Speed Test Tasks folder.");
			Assert.AreEqual(3, folderHierarchy[7].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number for Speed Test Tasks folder HierarchyLevel.");

			//Verify that you can retrieve a single folder
			//Top-level folder
			TaskFolder folder = taskManager.TaskFolder_GetById(folderId1);
			Assert.AreEqual(folderId1, folder.TaskFolderId);
			Assert.AreEqual(projectId, folder.ProjectId);
			Assert.AreEqual("Development Tasks", folder.Name);
			Assert.IsNull(folder.ParentTaskFolderId);
			//Second-level folder
			folder = taskManager.TaskFolder_GetById(folderId5);
			Assert.AreEqual(folderId5, folder.TaskFolderId);
			Assert.AreEqual(projectId, folder.ProjectId);
			Assert.AreEqual("Design", folder.Name);
			Assert.AreEqual(folderId1, folder.ParentTaskFolderId);

			//Get a list of top-level folders
			List<TaskFolder> folders = taskManager.TaskFolder_GetByParentId(projectId, null);
			Assert.AreEqual(3, folders.Count);
			Assert.AreEqual("Development Tasks", folders[0].Name);
			Assert.AreEqual("Other Tasks", folders[1].Name);
			Assert.AreEqual("Testing Tasks", folders[2].Name);

			//Get the list of child folders for a task folder
			folders = taskManager.TaskFolder_GetByParentId(projectId, folderId2);
			Assert.AreEqual(2, folders.Count);
			Assert.AreEqual("Functional Testing", folders[0].Name);
			Assert.AreEqual("Performance Testing", folders[1].Name);

			//Self and parents
			folderHierarchy = taskManager.TaskFolder_GetParents(projectId, folderId8, true);
			Assert.AreEqual(3, folderHierarchy.Count, "taskManager.TaskFolder_GetParents returned incorrect folder count.");
			Assert.AreEqual("Testing Tasks", folderHierarchy[0].Name, "taskManager.TaskFolder_GetParents returned incorrect name of a Testing Tasks Tasks folder Name.");
			Assert.AreEqual("AAC", folderHierarchy[0].IndentLevel, "taskManager.TaskFolder_GetParents returned incorrect folder IndentLevel.");
			Assert.AreEqual("Performance Testing", folderHierarchy[1].Name, "taskManager.TaskFolder_GetParents returned incorrect name of a Tasks Performance Testing folder Name.");
			Assert.AreEqual("AACAAB", folderHierarchy[1].IndentLevel, "taskManager.TaskFolder_GetParents returned incorrect folder IndentLevel.");
			Assert.AreEqual("Speed Test", folderHierarchy[2].Name, "taskManager.TaskFolder_GetParents returned incorrect name of a Speed Test Tasks folder Name.");
			Assert.AreEqual("AACAABAAA", folderHierarchy[2].IndentLevel, "taskManager.TaskFolder_GetParents returned incorrect folder IndentLevel.");

			//Check if a folder exists or not
			Assert.AreEqual(true, taskManager.TaskFolder_Exists(projectId, folderId2), "Task folder should exist");
			Assert.AreEqual(false, taskManager.TaskFolder_Exists(projectId + 1, folderId2), "Task folder does not exist in specified product");
			Assert.AreEqual(false, taskManager.TaskFolder_Exists(projectId, folderId2 + 10000000), "Task folder should NOT exist");

			//Make sure the system doesn't let us put folders in a recursive loop
			bool exceptionThrown = false;
			try
			{
				folder = taskManager.TaskFolder_GetById(folderId1);
				folder.StartTracking();
				folder.ParentTaskFolderId = folderId4;
				taskManager.TaskFolder_Update(folder);
			}
			catch (FolderCircularReferenceException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Try making a change to a task folder
			folder = taskManager.TaskFolder_GetById(folderId3);
			folder.StartTracking();
			folder.Name = "Misc Tasks";
			taskManager.TaskFolder_Update(folder);

			//Verify the change
			folder = taskManager.TaskFolder_GetById(folderId3);
			Assert.AreEqual(folderId3, folder.TaskFolderId);
			Assert.AreEqual(projectId, folder.ProjectId);
			Assert.AreEqual("Misc Tasks", folder.Name);
			Assert.IsNull(folder.ParentTaskFolderId);

			//Verify that we can move this folder under another one
			folder = taskManager.TaskFolder_GetById(folderId3);
			folder.StartTracking();
			folder.ParentTaskFolderId = folderId1;
			taskManager.TaskFolder_Update(folder);

			//Verify the change
			folderHierarchy = taskManager.TaskFolder_GetList(projectId);
			Assert.AreEqual(8, folderHierarchy.Count, "taskManager.TaskFolder_GetList returned incorrect folder count.");

			Assert.AreEqual("Development Tasks", folderHierarchy[0].Name, "taskManager.TaskFolder_GetList returned incorrect name of Development Tasks Task folder.");
			Assert.AreEqual(1, folderHierarchy[0].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number Tasks folder hierarchyLevel.");

			Assert.AreEqual("Testing Tasks", folderHierarchy[4].Name, "taskManager.TaskFolder_GetList returned incorrect name of  Testing Tasks Task folder.");
			Assert.AreEqual(1, folderHierarchy[4].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");

			Assert.AreEqual("Coding", folderHierarchy[1].Name, "taskManager.TaskFolder_GetList returned incorrect name of Coding Task folder.");
			Assert.AreEqual(2, folderHierarchy[1].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Design", folderHierarchy[2].Name, "taskManager.TaskFolder_GetList returned incorrect name of Design Task folder.");
			Assert.AreEqual(2, folderHierarchy[2].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Misc Tasks", folderHierarchy[3].Name, "taskManager.TaskFolder_GetList returned incorrect name of a Misc Tasks Tasks folder.");
			Assert.AreEqual(2, folderHierarchy[3].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Functional Testing", folderHierarchy[5].Name, "taskManager.TaskFolder_GetList returned incorrect name of  Functional Testing Tasks folder.");
			Assert.AreEqual(2, folderHierarchy[5].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");
			Assert.AreEqual("Performance Testing", folderHierarchy[6].Name, "taskManager.TaskFolder_GetList returned incorrect name of  Performance Testing Tasks folder.");
			Assert.AreEqual(2, folderHierarchy[6].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number of a Tasks folder HierarchyLevel.");

			Assert.AreEqual("Speed Test", folderHierarchy[7].Name, "taskManager.TaskFolder_GetList returned incorrect name of  Speed Test Tasks folder.");
			Assert.AreEqual(3, folderHierarchy[7].HierarchyLevel, "taskManager.TaskFolder_GetList returned incorrect number for Speed Test Tasks folder HierarchyLevel.");

			//Verify that we can delete a branch of task folders
			//Right now can only delete one level down, so need to delete Speed Test out first
			taskManager.TaskFolder_Delete(projectId, folderId8);

			//Verify that we can delete a branch of task folders
			taskManager.TaskFolder_Delete(projectId, folderId2);

			folderHierarchy = taskManager.TaskFolder_GetList(projectId);

			Assert.AreEqual(4, folderHierarchy.Count, "taskManagerTaskFolder_Delete(folderId2) did not successfully delete Testing Tasks Tasks folder.");
			Assert.AreEqual("Development Tasks", folderHierarchy[0].Name);
			Assert.AreEqual(1, folderHierarchy[0].HierarchyLevel);
			Assert.AreEqual("Coding", folderHierarchy[1].Name);
			Assert.AreEqual(2, folderHierarchy[1].HierarchyLevel);
			Assert.AreEqual("Design", folderHierarchy[2].Name);
			Assert.AreEqual(2, folderHierarchy[2].HierarchyLevel);
			Assert.AreEqual("Misc Tasks", folderHierarchy[3].Name);
			Assert.AreEqual(2, folderHierarchy[3].HierarchyLevel);

			//Delete the remaining folders
			taskManager.TaskFolder_Delete(projectId, folderId1);
			folderHierarchy = taskManager.TaskFolder_GetList(projectId);
			Assert.AreEqual(0, folderHierarchy.Count, "taskManagerTaskFolder_Delete(folderId1) did not successfully delete Development Tasks Task folder.");
		}

		/// <summary>
		/// Tests that you can organize tasks by folder
		/// </summary>
		[
		Test,
		SpiraTestCase(1268)
		]
		public void _15_OrganizeTasksByFolder()
		{
			//First lets verify that there are no tasks in the project initially
			List<TaskFolderHierarchyView> folderHierarchy = taskManager.TaskFolder_GetList(projectId);
			Assert.AreEqual(0, folderHierarchy.Count);

			//Now lets create a simple folder hierarchy
			int folderId1 = taskManager.TaskFolder_Create("Development Tasks", projectId).TaskFolderId;
			int folderId2 = taskManager.TaskFolder_Create("Testing Tasks", projectId).TaskFolderId;
			int folderId3 = taskManager.TaskFolder_Create("Other Tasks", projectId).TaskFolderId;
			int folderId4 = taskManager.TaskFolder_Create("Coding", projectId, folderId1).TaskFolderId;
			int folderId5 = taskManager.TaskFolder_Create("Design", projectId, folderId1).TaskFolderId;
			int folderId6 = taskManager.TaskFolder_Create("Functional Testing", projectId, folderId2).TaskFolderId;
			int folderId7 = taskManager.TaskFolder_Create("Performance Testing", projectId, folderId2).TaskFolderId;
			//int folderId8 = taskManager.TaskFolder_Create("Speed Test", projectId, folderId7).TaskFolderId;

			//Now lets create some tasks under some of the folders and one task not under a folder
			List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
			int tk_priorityCriticalId = priorities.FirstOrDefault(p => p.Score == 1).TaskPriorityId;
			int tk_priorityHighId = priorities.FirstOrDefault(p => p.Score == 2).TaskPriorityId;
			int tk_priorityMediumId = priorities.FirstOrDefault(p => p.Score == 3).TaskPriorityId;
			List<TaskType> types = taskManager.TaskType_Retrieve(projectTemplateId);
			int tk_typeTestingId = types.FirstOrDefault(t => t.Name == "Testing").TaskTypeId;
			int tk_typeOtherId = types.FirstOrDefault(t => t.Name == "Other").TaskTypeId;

			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, folderId4, null, null, USER_ID_FRED_BLOGGS, tk_priorityHighId, "Write screen X", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, folderId4, null, null, USER_ID_FRED_BLOGGS, tk_priorityMediumId, "Write screen Y", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);
			int taskId3 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, tk_typeTestingId, folderId2, null, null, USER_ID_FRED_BLOGGS, tk_priorityHighId, "Define the overall testing process", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);
			int taskId4 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, tk_typeTestingId, folderId6, null, null, USER_ID_FRED_BLOGGS, tk_priorityCriticalId, "Write the Rapise functional test for X", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);
			int taskId5 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, tk_typeOtherId, null, null, null, USER_ID_FRED_BLOGGS, tk_priorityCriticalId, "Define the overall project plan", "", null, null, 120, null, null, USER_ID_FRED_BLOGGS);

			//Verify that we can retrieve the tasks under a folder
			List<TaskView> tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, folderId4);
			Assert.AreEqual(2, tasks.Count);

			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, folderId2);
			Assert.AreEqual(1, tasks.Count);

			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, folderId6);
			Assert.AreEqual(1, tasks.Count);

			//Verify that we can retrieve all the tasks regardless of folder
			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(5, tasks.Count);

			//Verify that we can retrieve all of the tasks not in a folder
			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, TaskManager.NoneFilterValue);
			Assert.AreEqual(1, tasks.Count);

			//Try moving a task to a different folder
			taskManager.Task_UpdateFolder(taskId4, folderId2);

			//Verify
			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, folderId2);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, folderId6);
			Assert.AreEqual(0, tasks.Count);

			//Try moving a task to not be in a folder
			taskManager.Task_UpdateFolder(taskId4, null);

			//Verify
			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, folderId2);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, TaskManager.NoneFilterValue);
			Assert.AreEqual(2, tasks.Count);

			//Try cloning a folder and its tasks
			int folderId8 = taskManager.TaskFolder_Copy(USER_ID_FRED_BLOGGS, projectId, folderId4, null, false);

			//Verify
			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, folderId8);
			Assert.AreEqual(2, tasks.Count);
			Assert.AreEqual("Write screen X", tasks[0].Name);
			Assert.AreEqual(tk_priorityHighId, tasks[0].TaskPriorityId);
			Assert.AreEqual("Write screen Y", tasks[1].Name);
			Assert.AreEqual(tk_priorityMediumId, tasks[1].TaskPriorityId);
			int taskId6 = tasks[0].TaskId;
			int taskId7 = tasks[1].TaskId;

			//Delete a folder containing tasks and verify that the tasks are simply unlinked from the folder
			taskManager.TaskFolder_Delete(projectId, folderId2);

			//Verify
			tasks = taskManager.Retrieve(projectId, "TaskPriorityId", true, 1, 99999, null, InternalRoutines.UTC_OFFSET, TaskManager.NoneFilterValue);
			Assert.AreEqual(3, tasks.Count);

			//Delete the tasks (the folders get deleted in the dispose function)
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId3, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId4, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId5, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId6, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId7, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests the functions used by the planning board
		/// </summary>
		[
		Test,
		SpiraTestCase(1312)
		]
		public void _16_PlanningBoardTests()
		{
			//First lets create a requirement
			RequirementManager requirementManager = new RequirementManager();
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);

			//Now lets add some tasks to the requirement
			TaskManager taskManager = new TaskManager();
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId1, null, null, null, "Task 1", null, null, null, 4000, null, null);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId1, null, null, null, "Task 2", null, null, null, 3000, null, null);

			//Lets create a releases, with two iterations
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(2), 2, 0, null, false);
			int iterationId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 001", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 2, 0, null, false);
			int iterationId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 002", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(2), 2, 0, null, false);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Test the starting case - tasks not assigned to release or user
			List<TaskView> tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, null, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId2, null, false);
			Assert.AreEqual(0, tasks.Count);

			//Now assign a task to the release
			taskManager.AssociateToIteration(new List<int>() { taskId1 }, releaseId1, USER_ID_FRED_BLOGGS);

			//Verify the data
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, false);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, null, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId2, null, false);
			Assert.AreEqual(0, tasks.Count);

			//Now assign a task to the iteration
			taskManager.AssociateToIteration(new List<int>() { taskId2 }, iterationId1, USER_ID_FRED_BLOGGS);

			//Verify the data
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.NotStarted);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, false);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, null, false);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId2, null, false);
			Assert.AreEqual(0, tasks.Count);

			//Now assign one of the tasks to a user
			taskManager.AssignToUser(taskId1, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS);

			//Verify the data
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, null, false);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, tasks.Count);

			//Now assign the other tasks to a user
			taskManager.AssignToUser(taskId2, USER_ID_JOE_SMITH, USER_ID_FRED_BLOGGS);

			//Verify the data
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, null, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(1, tasks.Count);

			//Lets move the task to a different status
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.NotStarted);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.InProgress);
			Assert.AreEqual(0, tasks.Count);

			task = taskManager.RetrieveById(taskId2);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.NotStarted);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.InProgress);
			Assert.AreEqual(1, tasks.Count);

			//Verify the requirement associated with the task
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual(requirementId1, task.RequirementId);

			//Now create a second requirement and move the task to that requirement
			int requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 2", null, null, USER_ID_FRED_BLOGGS);
			taskManager.Task_AssociateWithRequirement(taskId1, requirementId2, USER_ID_FRED_BLOGGS);

			//Verify
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual(requirementId2, task.RequirementId);

			//Now move it back, and verify
			taskManager.Task_AssociateWithRequirement(taskId1, requirementId1, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual(requirementId1, task.RequirementId);

			//Clean up by deleting the created items
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
		}

		/// <summary>
		/// Tests that you can split tasks
		/// </summary>
		[
		Test,
		SpiraTestCase(1326)
		]
		public void _17_SplitTasks()
		{
			//First lets create a release
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(2), 2, 0, null, false);

			//Next lets create a requirement
			RequirementManager requirementManager = new RequirementManager();
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId1, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);

			//Next create a risk
			RiskManager riskManager = new RiskManager();
			int riskId1 = riskManager.Risk_Insert(projectId, null, null, null, null, USER_ID_FRED_BLOGGS, null, "Risk linked to task", "blank", null, null, DateTime.UtcNow, null, null);

			//Now lets add a task to the requirement, release, and risk (not started, no owner)
			TaskManager taskManager = new TaskManager();
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId1, releaseId1, null, null, "Task 1", null, null, null, 4000, null, null, null, true, riskId1);

			//Now lets split the task in half
			int taskId2 = taskManager.Split(taskId1, "Task 2", USER_ID_FRED_BLOGGS);

			//Verify
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			Task task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual(2000, task.EstimatedEffort);
			task = taskManager.RetrieveById(taskId2);
			Assert.AreEqual(2000, task.EstimatedEffort);
			Assert.AreEqual(requirementId1, task.RequirementId);
			Assert.AreEqual(releaseId1, task.ReleaseId);
			Assert.AreEqual(riskId1, task.RiskId);
			List<ArtifactLinkView> associations = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.Task, taskId2, "", true, null);
			Assert.AreEqual(1, associations.Count);
			Assert.AreEqual(null, associations[0].Comment);

			//Now we make the second task in-progress
			task.StartTracking();
			task.OwnerId = USER_ID_JOE_SMITH;
			task.RemainingEffort = 1000;
			task.ActualEffort = 1500;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Verify
			task = taskManager.RetrieveById(taskId2);
			Assert.AreEqual(2000, task.EstimatedEffort);
			Assert.AreEqual(1500, task.ActualEffort);
			Assert.AreEqual(1000, task.RemainingEffort);
			Assert.AreEqual(2500, task.ProjectedEffort);
			Assert.AreEqual(USER_ID_JOE_SMITH, task.OwnerId);
			Assert.AreEqual((int)Task.TaskStatusEnum.InProgress, task.TaskStatusId);

			//Now lets split off the remaining work into its own task and assign to Fred
			int taskId3 = taskManager.Split(taskId2, "Task 3", USER_ID_FRED_BLOGGS, null, USER_ID_FRED_BLOGGS, "Giving rest to Fred");

			//Verify
			task = taskManager.RetrieveById(taskId2);
			Assert.AreEqual(1000, task.EstimatedEffort);
			Assert.AreEqual(1500, task.ActualEffort);
			Assert.AreEqual(0, task.RemainingEffort);
			Assert.AreEqual(1500, task.ProjectedEffort);
			Assert.AreEqual(USER_ID_JOE_SMITH, task.OwnerId);
			Assert.AreEqual((int)Task.TaskStatusEnum.Completed, task.TaskStatusId);
			task = taskManager.RetrieveById(taskId3);
			Assert.AreEqual(1000, task.EstimatedEffort);
			Assert.AreEqual(0, task.ActualEffort);
			Assert.AreEqual(1000, task.RemainingEffort);
			Assert.AreEqual(1000, task.ProjectedEffort);
			Assert.AreEqual(requirementId1, task.RequirementId);
			Assert.AreEqual(releaseId1, task.ReleaseId);
			Assert.AreEqual(riskId1, task.RiskId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, task.OwnerId);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, task.TaskStatusId);
			associations = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.Task, taskId3, "", true, null);
			Assert.AreEqual(1, associations.Count);
			Assert.AreEqual("Giving rest to Fred", associations[0].Comment);

			//Finally lets explicitly split one by a certain percentage
			int taskId4 = taskManager.Split(taskId1, "Task 4", USER_ID_FRED_BLOGGS, 25, USER_ID_FRED_BLOGGS, "Giving some to Fred");

			//Verify
			task = taskManager.RetrieveById(taskId1);
			Assert.AreEqual(1500, task.EstimatedEffort);
			Assert.AreEqual(1500, task.RemainingEffort);
			Assert.AreEqual(1500, task.ProjectedEffort);
			task = taskManager.RetrieveById(taskId4);
			Assert.AreEqual(500, task.EstimatedEffort);
			Assert.AreEqual(500, task.RemainingEffort);
			Assert.AreEqual(500, task.ProjectedEffort);
			Assert.AreEqual(requirementId1, task.RequirementId);
			Assert.AreEqual(releaseId1, task.ReleaseId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, task.OwnerId);
			associations = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.Task, taskId4, "", true, null);
			Assert.AreEqual(1, associations.Count);
			Assert.AreEqual("Giving some to Fred", associations[0].Comment);

			//Clean up by deleting the created items
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId3, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId4, USER_ID_FRED_BLOGGS);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			riskManager.Risk_MarkAsDeleted(projectId, riskId1, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that you can retrieve the tasks for a test run
		/// </summary>
		[
		Test,
		SpiraTestCase(1610)
		]
		public void _18_RetrieveTasksForTestRun()
		{

			//First we have to create a new test case
			TestCaseManager testCaseManager = new TestCaseManager();
			int tc_typeExploratoryId = testCaseManager.TestCaseType_Retrieve(projectTemplateId).FirstOrDefault(t => t.IsExploratory).TestCaseTypeId;
			int tc_priorityCriticalId = testCaseManager.TestCasePriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 1).TestCasePriorityId;
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Ability to create new book", "Description of ability to create new book", tc_typeExploratoryId, TestCase.TestCaseStatusEnum.ReadyForTest, tc_priorityCriticalId, null, 20, null, null);
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

			//Now we create the tasks and link to the test run
			int testRunId = testRunsPending.TestRuns[0].TestRunId;
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 1", null, null, null, 4000, null, null);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 2", null, null, null, 4000, null, null);
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Task, taskId1, Artifact.ArtifactTypeEnum.TestRun, testRunId, USER_ID_FRED_BLOGGS, null, DateTime.UtcNow);
			artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Task, taskId2, Artifact.ArtifactTypeEnum.TestRun, testRunId, USER_ID_FRED_BLOGGS, null, DateTime.UtcNow);

			//Finally verify that we can retrieve for the test run
			List<TaskView> tasks = taskManager.RetrieveByTestRunId(projectId, testRunId);
			Assert.AreEqual(2, tasks.Count);
			Assert.IsTrue(tasks.Any(t => t.TaskId == taskId1));
			Assert.IsTrue(tasks.Any(t => t.TaskId == taskId2));

			//Clean up by deleting the tasks
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests the code that populates/updates the items in the Task board views
		/// </summary>
		[Test, SpiraTestCase(1644)]
		public void _19_TaskBoardTests()
		{
			List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
			int tk_priorityCriticalId = priorities.FirstOrDefault(p => p.Score == 1).TaskPriorityId;
			int tk_priorityHighId = priorities.FirstOrDefault(p => p.Score == 2).TaskPriorityId;
			int tk_priorityMediumId = priorities.FirstOrDefault(p => p.Score == 3).TaskPriorityId;
			int tk_priorityLowId = priorities.FirstOrDefault(p => p.Score == 4).TaskPriorityId;

			int defaultTaskStatusId = (int)Task.TaskStatusEnum.NotStarted;
			int defaultTaskTypeId = taskManager.TaskType_RetrieveDefault(projectTemplateId).TaskTypeId;

			//Get the list of active statuses (involved in at least one workflow)
			List<TaskStatus> statuses = taskManager.RetrieveStatusesInUse(projectTemplateId);
			Assert.AreEqual(5, statuses.Count);

			//First test that we can create a new blank task record (used in the popup task creation screen)
			TaskView taskView = taskManager.Task_New(projectId, USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual("", taskView.Name);
			Assert.AreEqual("", taskView.Description);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, taskView.CreatorId);
			Assert.AreEqual(projectId, taskView.ProjectId);
			Assert.AreEqual(defaultTaskStatusId, taskView.TaskStatusId);
			Assert.AreEqual(defaultTaskTypeId, taskView.TaskTypeId);

			//Next lets create some tasks
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 1", null, null, null, 4000, null, null);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 2", null, null, null, 4000, null, null);
			int taskId3 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 3", null, null, null, 4000, null, null);
			int taskId4 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 4", null, null, null, 4000, null, null);
			int taskId5 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 5", null, null, null, 4000, null, null);
			int taskId6 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 6", null, null, null, 4000, null, null);
			int taskId7 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 7", null, null, null, 4000, null, null);
			int taskId8 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 8", null, null, null, 4000, null, null);
			int taskId9 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 9", null, null, null, 4000, null, null);
			int taskId10 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 10", null, null, null, 4000, null, null);

			//Lets create two releases, with the first one having two iterations
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddMonths(2), 2, 0, null, false);
			int iterationId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 001", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 2, 0, null, false);
			int iterationId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 002", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(2), 2, 0, null, false);
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0", null, "2.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow.AddMonths(2), DateTime.UtcNow.AddMonths(4), 2, 0, null, false);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Now lets verify that no releases/iterations have tasks
			List<TaskView> tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, null, false);
			Assert.AreEqual(10, tasks.Count);
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, releaseId1, true);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, iterationId1, true);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, iterationId2, true);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, releaseId2, true);
			Assert.AreEqual(0, tasks.Count);

			//Verify that all the tasks are in the initial status and have no release set
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, null, defaultTaskStatusId);
			Assert.AreEqual(10, tasks.Count);

			//Verify that all the tasks have no priority set
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, null, null);
			Assert.AreEqual(10, tasks.Count);

			//Verify that all the tasks are unassigned
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, null, null, true);
			Assert.AreEqual(10, tasks.Count);

			//Now lets assign the priority to some of the tasks
			Task task = taskManager.RetrieveById(taskId3, false);
			task.StartTracking();
			task.TaskPriorityId = tk_priorityCriticalId;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId4, false);
			task.StartTracking();
			task.TaskPriorityId = tk_priorityCriticalId;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId5, false);
			task.StartTracking();
			task.TaskPriorityId = tk_priorityHighId;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId6, false);
			task.StartTracking();
			task.TaskPriorityId = tk_priorityHighId;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Now lets assign the tasks to various releases and iterations
			taskManager.AssociateToIteration(new List<int> { taskId1, taskId2 }, releaseId1, USER_ID_FRED_BLOGGS);
			taskManager.AssociateToIteration(new List<int> { taskId3, taskId4, taskId5 }, iterationId1, USER_ID_FRED_BLOGGS);
			taskManager.AssociateToIteration(new List<int> { taskId6, taskId7, taskId8 }, iterationId2, USER_ID_FRED_BLOGGS);
			taskManager.AssociateToIteration(new List<int> { taskId9, taskId10 }, releaseId2, USER_ID_FRED_BLOGGS);

			//Now lets assign the status to some of the tasks
			task = taskManager.RetrieveById(taskId3, false);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.Deferred;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId4, false);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId5, false);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId6, false);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId1, false);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.Blocked;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId2, false);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
			taskManager.Update(task, USER_ID_FRED_BLOGGS);

			//Assign some of the tasks to users
			taskManager.AssignToUser(taskId3, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS);
			taskManager.AssignToUser(taskId4, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS);
			taskManager.AssignToUser(taskId5, USER_ID_JOE_SMITH, USER_ID_FRED_BLOGGS);

			//Verify that the history item is recorded (wasn't previously due to a bug)
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(taskId3, Artifact.ArtifactTypeEnum.Task);
			//Assert.IsTrue(historyChangeSets.Any(h => h.Details.Any(d => d.FieldName == "OwnerId")));

			//Now lets verify the distribution of non-deferred tasks by release
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, null, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, releaseId1, true);
			Assert.AreEqual(7, tasks.Count);
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, iterationId1, true);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, iterationId2, true);
			Assert.AreEqual(3, tasks.Count);
			tasks = taskManager.Task_RetrieveByReleaseIdWithIterations(projectId, releaseId2, true);
			Assert.AreEqual(2, tasks.Count);

			//Now lets verify the distribution of tasks by status

			//No Release
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, null, defaultTaskStatusId);
			Assert.AreEqual(0, tasks.Count);
			//All Releases
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, -2, (int)Task.TaskStatusEnum.NotStarted);
			Assert.AreEqual(4, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, -2, (int)Task.TaskStatusEnum.Deferred);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, -2, (int)Task.TaskStatusEnum.InProgress);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, -2, (int)Task.TaskStatusEnum.Completed);
			Assert.AreEqual(2, tasks.Count);
			//Release 1
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Task.TaskStatusEnum.NotStarted);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Task.TaskStatusEnum.Deferred);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Task.TaskStatusEnum.InProgress);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, releaseId1, (int)Task.TaskStatusEnum.Completed);
			Assert.AreEqual(2, tasks.Count);
			//Iteration 1
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.NotStarted);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.Deferred);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.InProgress);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByStatusId(projectId, iterationId1, (int)Task.TaskStatusEnum.Completed);
			Assert.AreEqual(0, tasks.Count);

			//Now lets verify the distribution of non-deferred tasks by priority

			//All Releases (open statuses)
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, null, null);
			Assert.AreEqual(4, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, null, tk_priorityCriticalId);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, null, tk_priorityHighId);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, null, tk_priorityMediumId);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, null, tk_priorityLowId);
			Assert.AreEqual(0, tasks.Count);

			//Specific Release
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, releaseId1, null);
			Assert.AreEqual(4, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, releaseId1, tk_priorityCriticalId);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, releaseId1, tk_priorityHighId);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, releaseId1, tk_priorityMediumId);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, releaseId1, tk_priorityLowId);
			Assert.AreEqual(0, tasks.Count);

			//Specific Iteration
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, iterationId1, null);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, iterationId1, tk_priorityCriticalId);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, iterationId1, tk_priorityHighId);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, iterationId1, tk_priorityMediumId);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveByPriorityId(projectId, -2, tk_priorityLowId);
			Assert.AreEqual(0, tasks.Count);

			//Now lets verify the distribution of tasks by person
			//No Release
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, null, null, true);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, null, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, null, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(0, tasks.Count);
			//All Releases
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, -2, null, true);
			Assert.AreEqual(7, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, -2, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, -2, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(1, tasks.Count);
			//Specific Release (including child iterations)
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(5, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(1, tasks.Count);
			//Specific Release (excluding child iterations)
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, null, false);
			Assert.AreEqual(2, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(0, tasks.Count);
			//Specific Iteration
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, null, true);
			Assert.AreEqual(0, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(1, tasks.Count);
			tasks = taskManager.Task_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(1, tasks.Count);

			//Clean up by deleting the created tasks
			taskManager.MarkAsDeleted(projectId, taskId1, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId2, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId3, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId4, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId5, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId6, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId7, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId8, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId9, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId10, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that you can create, edit, delete task types
		/// </summary>
		[Test]
		[SpiraTestCase(1734)]
		public void _20_EditTypes()
		{
			//First lets get the list of types in the current template
			List<TaskType> types = taskManager.TaskType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(7, types.Count);

			//Next lets add a new type
			int taskTypeId1 = taskManager.TaskType_Insert(
				projectTemplateId,
				"Code Review 2",
				null,
				false,
				true
				);

			//Verify that it was created
			TaskType taskType = taskManager.TaskType_RetrieveById(taskTypeId1);
			Assert.IsNotNull(taskType);
			Assert.AreEqual("Code Review 2", taskType.Name);
			Assert.AreEqual(true, taskType.IsActive);
			Assert.AreEqual(false, taskType.IsDefault);

			//Next lets add another new type
			int taskTypeId2 = taskManager.TaskType_Insert(
				projectTemplateId,
				"Pull Request 2",
				null,
				false,
				true
				);

			//Verify that it was created
			taskType = taskManager.TaskType_RetrieveById(taskTypeId2);
			Assert.IsNotNull(taskType);
			Assert.AreEqual("Pull Request 2", taskType.Name);
			Assert.AreEqual(true, taskType.IsActive);
			Assert.AreEqual(false, taskType.IsDefault);

			//Make changes
			taskType.StartTracking();
			taskType.Name = "Code Fork";
			taskType.IsActive = false;
			taskManager.TaskType_Update(taskType);

			//Verify the changes
			//Verify that it was created
			taskType = taskManager.TaskType_RetrieveById(taskTypeId2);
			Assert.IsNotNull(taskType);
			Assert.AreEqual("Code Fork", taskType.Name);
			Assert.AreEqual(false, taskType.IsActive);
			Assert.AreEqual(false, taskType.IsDefault);

			//Verify that we can get the total count of types
			types = taskManager.TaskType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(9, types.Count);

			//Verify that we can get the total count of active types
			types = taskManager.TaskType_Retrieve(projectTemplateId, true);
			Assert.AreEqual(8, types.Count);

			//Verify that we can get the default type
			taskType = taskManager.TaskType_RetrieveDefault(projectTemplateId);
			Assert.IsNotNull(taskType);
			Assert.AreEqual("Development", taskType.Name);
			Assert.AreEqual(true, taskType.IsActive);
			Assert.AreEqual(true, taskType.IsDefault);

			//Delete our new types (internal function only, not possible in the UI)
			taskManager.TaskType_Delete(taskTypeId1);
			taskManager.TaskType_Delete(taskTypeId2);

			//Verify the count
			types = taskManager.TaskType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(7, types.Count);
		}

		/// <summary>
		/// Tests that you can create, edit, delete task priorities
		/// </summary>
		[Test]
		[SpiraTestCase(1735)]
		public void _21_EditPriorities()
		{
			//First lets get the list of priorities in the current template
			List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
			Assert.AreEqual(4, priorities.Count);

			//Next lets add a new priority
			int priorityId1 = taskManager.TaskPriority_Insert(
				projectTemplateId,
				"5 - Minor",
				"eeeeee",
				true,
				5
				);

			//Verify that it was created
			TaskPriority priority = taskManager.TaskPriority_RetrieveById(priorityId1);
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
			taskManager.TaskPriority_Update(priority);

			//Verify the changes
			priority = taskManager.TaskPriority_RetrieveById(priorityId1);
			Assert.IsNotNull(priority);
			Assert.AreEqual("5 - Cosmetic", priority.Name);
			Assert.AreEqual(true, priority.IsActive);
			Assert.AreEqual("dddddd", priority.Color);
			Assert.AreEqual(6, priority.Score);

			//Verify that we can get the total count of priorities
			priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
			Assert.AreEqual(5, priorities.Count);

			//Delete our new priorities (internal function only, not possible in the UI)
			taskManager.TaskPriority_Delete(priorityId1);

			//Verify the count
			priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
			Assert.AreEqual(4, priorities.Count);
		}

		/// <summary>
		/// Tests that you can create, view, delete tasks associated with project risks
		/// </summary>
		[Test]
		[SpiraTestCase(1784)]
		public void _22_TasksLinkedToRisks()
		{
			//First lets create a new risk in the project template
			RiskManager riskManager = new RiskManager();
			int riskId = riskManager.Risk_Insert(projectId, null, null, null, null, USER_ID_FRED_BLOGGS, null, "Sample Risk", null, null, null, DateTime.UtcNow, null, null);

			//Create two tasks associated with the risk
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 1", null, null, null, null, null, null, USER_ID_FRED_BLOGGS, true, riskId);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, null, "Task 2", null, null, null, null, null, null, USER_ID_FRED_BLOGGS, true, riskId);

			//Verify that we can view the tasks associated with the risk
			Hashtable filters = new Hashtable();
			filters.Add("RiskId", riskId);
			int count = taskManager.Count(projectId, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, count);
			List<TaskView> tasks = taskManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, tasks.Count);

			//Now deassociate one of the tasks
			taskManager.RemoveRiskAssociation(taskId1, riskId);

			//Verify
			count = taskManager.Count(projectId, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, count);
			tasks = taskManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, tasks.Count);
			Assert.AreEqual(taskId2, tasks.First().TaskId);
		}
	}
}
