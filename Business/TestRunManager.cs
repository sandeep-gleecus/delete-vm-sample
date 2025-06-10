using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing Test Runs and Test Run Steps that are executed and managed in the system
	/// </summary>
	public class TestRunManager : ManagerBase
	{
		private const string CLASS_NAME = "Business.TestRunManager::";

		#region Static Methods

		/// <summary>
		/// Does the description field contain a special #{...} guid used to map test run embedded images
		/// </summary>
		/// <param name="description">The attachment description</param>
		/// <returns>True if it's a match</returns>
		public static bool IsDescriptionGuidUrl(string description)
		{
			if (String.IsNullOrWhiteSpace(description))
			{
				return false;
			}
			Regex regex = new Regex(@"^(#\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");
			return regex.IsMatch(description);
		}

		#endregion

		#region Test Run Step methods

		/// <summary>
		///	Retrieves a specific test run step by its id
		/// </summary>
		/// <param name="testRunStepId">The ID of the test run step</param>
		/// <returns>Test Run Step entity</returns>
		public TestRunStep TestRunStep_RetrieveById(int testRunStepId)
		{
			const string METHOD_NAME = "TestRunStep_RetrieveForTestRun";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestRunStep testRunStep;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We get the steps for the test run
					var query = from s in context.TestRunSteps
									.Include(s => s.TestRun)
									.Include(s => s.TestRun.Tester)
									.Include(s => s.TestRun.Tester.Profile)
								where s.TestRunStepId == testRunStepId
								select s;

					//Execute the query
					testRunStep = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunStep;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		///	Retrieves a list of test run steps for a specific test run
		/// </summary>
		/// <param name="testRunId">The ID of the test run</param>
		/// <returns>Test Run Step list</returns>
		public List<TestRunStepView> TestRunStep_RetrieveForTestRun(int testRunId)
		{
			const string METHOD_NAME = "TestRunStep_RetrieveForTestRun";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunStepView> testRunSteps;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We get the steps for the test run
					var query = from s in context.TestRunStepsView
								where s.TestRunId == testRunId
								orderby s.Position, s.TestRunStepId
								select s;

					//Fire the second query to initiate 'fix-up'
					testRunSteps = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunSteps;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Test Run Population Functions

		/// <summary>
		/// Marks a set of pending test runs as completed, which involves deleting the pending entry
		/// and potentially changing the status of the test set it was linked to (if there is one)
		/// </summary>
		/// <param name="testRunsPendingId">The id of the set of pending test runs to be completed</param>
		/// <param name="userId">The id of the user marking it as completed</param>
		public void CompletePending(int testRunsPendingId, int userId)
		{
			const string METHOD_NAME = "CompletePending";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the pending record together with its associated test runs
				TestRunsPending testRunsPending = this.RetrievePendingById(testRunsPendingId, true);

				//Get the project settings collection for testRunsPending - to use later in the method
				ProjectSettings projectSettings = null;
				if (testRunsPending.ProjectId > 0)
				{
					projectSettings = new ProjectSettings(testRunsPending.ProjectId);
				}

				//See if we're linked to a test set or not
				int? testSetId = testRunsPending.TestSetId;
				bool allTestRunsCompleted = true;
				List<int> completedTestCases = new List<int>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach to the context
					context.TestRunsPendings.Attach(testRunsPending);
					testRunsPending.StartTracking();

					//First we need to iterate through the list of test runs associated with the pending test runs id
					//and delete if in the not-run state, otherwise just de-associate with the pending id
					List<TestRun> testRunsToDelete = new List<TestRun>();
					List<TestRun> testRunsToDisconnect = new List<TestRun>();
					foreach (TestRun testRun in testRunsPending.TestRuns)
					{
						testRun.StartTracking();
						if (testRun.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun)
						{
							//Delete the row together with its child test run steps
							testRunsToDelete.Add(testRun);
							allTestRunsCompleted = false;
						}
						else
						{
							//**********************************************************testRun.ExecutionStatusId = (int)TestRun.ExecutionStatusEnum.;
							//Deassociate with the pending record
							testRunsToDisconnect.Add(testRun);

							//Add this to the list of test cases that has been completed, so that we can unset
							//the owner field if it's the same as the user completing the test runs
							if (!completedTestCases.Contains(testRun.TestCaseId))
							{
								completedTestCases.Add(testRun.TestCaseId);
							}

							//Update concurrency date
							testRun.ConcurrencyDate = DateTime.UtcNow;
						}
						if (testRun.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.InProgress)
						{
							testRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
						}
					}

					//Disconnect the completed test runs
					foreach (TestRun testRun in testRunsToDisconnect)
					{
						testRun.TestRunsPendingId = null;
						if (testRun.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.InProgress)
						{
							testRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
						}
					}

					//Delete any of the incomplete test runs
					foreach (TestRun testRun in testRunsToDelete)
					{
						context.TestRuns.DeleteObject(testRun);
					}

					//Next delete the record from the test run pending table itself
					//We do a fresh retrieve without the 
					context.TestRunsPendings.DeleteObject(testRunsPending);

					try
					{
						//Commit all the changes (no history changes needed to be recorded)
						context.SaveChanges();
					}
					catch (Exception exception)
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME + "~Save-DeleteObject", exception);
						Logger.Flush();
					}
				}

				//If the test run was from a test set, then update the status of the test set itself
				if (testSetId.HasValue)
				{
					//If all tests completed, mark as completed, otherwise mark as deferred since some
					//tests in the run were not done by the user, but the run was finished prematurely
					TestSetManager testSetManager = new TestSetManager();
					TestSet testSet = testSetManager.RetrieveById2(null, testSetId.Value);

					testSet.StartTracking();
					if (allTestRunsCompleted)
					{
						//Unset the planned date, unset the owner and mark as completed
						if (!testSet.RecurrenceId.HasValue)
						{
							testSet.PlannedDate = null;
							if (testSet.OwnerId.HasValue && projectSettings != null && projectSettings.Testing_CompletingTestSetUnassigns)
							{
								//Only unset the owner if it's currently set to the user completing the testing
								//And if no recurrence pattern is set and also make sure the user has this functionality enabled
								if (testSet.OwnerId.Value == userId)
								{
									testSet.OwnerId = null;
								}
							}
						}
						testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Completed;

					}
					else
					{
						//Mark as deferred as long as we're still the current owner
						if (testSet.OwnerId.HasValue)
						{
							if (testSet.OwnerId.Value == userId)
							{
								testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Deferred;
							}
						}
					}

					//Extract changes for notifications
					Dictionary<string, object> changes = testSet.ExtractChanges();

					//Persist the changes
					testSetManager.Update(testSet, userId);

					//Send a notification
					try
					{
						testSet.ApplyChanges(changes);
						new NotificationManager().SendNotificationForArtifact(testSet, null, null);
					}
					catch (Exception exception)
					{
						//Log but don't throw
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					}
				}

				//Finally iterate through all the test cases that have been executed and if any are owned
				//by the current user AND passed we need to unset the owner field, since they have been completed
				if (projectSettings != null && projectSettings.Testing_PassingTestCaseUnassigns)
				{
					TestCaseManager testCaseManager = new TestCaseManager();
					foreach (int testCaseId in completedTestCases)
					{
						try
						{
							TestCase testCase = testCaseManager.RetrieveById2(null, testCaseId);
							testCase.StartTracking();

							if (testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.InProgress)
							{
								testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
								testCaseManager.Update(testCase, userId, null, true);
							}

							//if (testCase.OwnerId.HasValue)
							//{
							//	if (testCase.OwnerId.Value == userId && testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Passed)
							//	{
							//		testCase.OwnerId = userId;
							//		testCaseManager.Update(testCase, userId, null, true);
							//	}
							//}
						}
						catch (ArtifactNotExistsException)
						{
							//Ignore as the test cases may have been deleted
						}
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Populates a new record into the pending test runs datatable of the test run dataset
		/// </summary>
		/// <param name="projectId">The project that the test run belongs to</param>
		/// <param name="testRunsPending">The test run pending set that we want to populate</param>
		/// <param name="testSetId">The id of the test set (optional)</param>
		/// <param name="userId">The user who is executing the test</param>
		/// <param name="name">The name to give to the pending test run</param>
		/// <remarks>The execution counts are always created with all tests marked as 'not-run'</remarks>
		protected void PopulatePending(int projectId, int userId, TestRunsPending testRunsPending, string name, int? testSetId = null)
		{
			const string METHOD_NAME = "PopulatePending";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int testRunCount = 0;
				if (testRunsPending.TestRuns != null)
				{
					testRunCount = testRunsPending.TestRuns.Count;
				}

				//Fill out pending table with data for the new record
				testRunsPending.ProjectId = projectId;
				testRunsPending.TestSetId = testSetId;
				testRunsPending.TesterId = userId;
				testRunsPending.Name = name;
				testRunsPending.CreationDate = DateTime.UtcNow;
				testRunsPending.LastUpdateDate = DateTime.UtcNow;
				testRunsPending.CountNotRun = testRunCount;
				testRunsPending.CountBlocked = 0;
				testRunsPending.CountCaution = 0;
				testRunsPending.CountFailed = 0;
				testRunsPending.CountNotApplicable = 0;
				testRunsPending.CountPassed = 0;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Populates a test run from the test cases in a test case folder, and recursively searches sub-folders
		/// </summary>
		/// <param name="releaseId">The current release (optional)</param>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseFolderId">The id of the test case folder</param>
		/// <param name="testRunsPending">The test runs set that we're adding to</param>
		protected void PopulateFromTestFolder(int projectId, int testCaseFolderId, TestRunsPending testRunsPending, int userId, int? releaseId)
		{
			TestCaseManager testCaseManager = new TestCaseManager();

			//We need to first get any sub-folders
			List<TestCaseFolder> childFolders = testCaseManager.TestCaseFolder_GetByParentId(projectId, testCaseFolderId);

			if (childFolders != null && childFolders.Count > 0)
			{
				foreach (TestCaseFolder childFolder in childFolders)
				{
					PopulateFromTestFolder(projectId, childFolder.TestCaseFolderId, testRunsPending, userId, releaseId);
				}
			}

			//Retrieve the testcases in the folder
			List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, null, true, 1, Int32.MaxValue, null, 0, testCaseFolderId);

			//Loop through the test cases in the folder
			foreach (TestCaseView testCaseView in testCases)
			{
				TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseView.TestCaseId);

				//Now iterate through the test cases and add any that aren't already there to the test run
				this.PopulateTestRun(testCase, testRunsPending, userId, releaseId, null, null, null, null);
			}
		}

		/// <summary>
		/// Populates a test run from the passed in test case
		/// </summary>
		/// <param name="testConfigurationEntry">The test configuration entry (option) if we're using one</param>
		/// <param name="testRunsPending">the test run dataset we're populating</param>
		/// <param name="userId">The user that is running this test</param>
		/// <param name="releaseId">The release being tested (Optional)</param>
		/// <param name="testSetId">The test set being tested (Optional)</param>
		/// <param name="testSetTestCaseId">The id of the unique instance of the test case in the test set (optional)</param>
		/// <param name="automationHostId">The id of the host being executed on</param>
		/// <param name="buildId">The id of the build being executed on</param>
		/// <param name="testCase">The test case we're populating from</param>
		/// <remarks>Used for manual test runs only</remarks>
		protected void PopulateTestRun(TestCase testCase, TestRunsPending testRunsPending, int userId, int? releaseId, int? testSetId, int? testSetTestCaseId, int? automationHostId, int? buildId, IGrouping<int, TestConfigurationEntry> testConfigurationEntry = null)
		{
			//Add the new test run to the pending set
			TestRun testRun = new TestRun();
			testRunsPending.TestRuns.Add(testRun);
			testRun.Name = testCase.Name;
			testRun.Description = testCase.Description;
			testRun.TestCaseId = testCase.TestCaseId;
			testRun.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Manual;
			testRun.TesterId = userId;
			testRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
			testRun.ReleaseId = releaseId;
			testRun.TestSetId = testSetId;
			testRun.TestSetTestCaseId = testSetTestCaseId;
			testRun.AutomationHostId = automationHostId;
			testRun.BuildId = buildId;
			testRun.EstimatedDuration = testCase.EstimatedDuration;
			testRun.StartDate = DateTime.UtcNow;
			testRun.ConcurrencyDate = DateTime.UtcNow;

			//Get the latest ChangeSet ID for this project.
			using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			{
				//We need to see if this Test Case is exploratory.
				bool isExplore = ct.TestCaseTypes
					.Any(t =>
						t.IsExploratory &&
						t.TestCaseTypeId == testCase.TestCaseTypeId
					);

				//Exploratory tests are not fixed, and therefore, we are NOT (at this time) recording ChangeSetId.
				if (!isExplore)
					testRun.ChangeSetId = ct.HistoryChangeSets
						.Where(f => f.ProjectId == testCase.ProjectId)
						.OrderByDescending(f => f.ChangeSetId)
						.Select(f => (long?)f.ChangeSetId) //We cast this to a nullable long, because otherwise 'default' is "0".
						.FirstOrDefault();
				//That may look clunky instead of something easier like the Max() LINQ, but the MAX() would cause it to 
				//  loop through everything in-memory, instead of handling it on the SQL Server side.
			}

			//See if we have any parameters for this test case (including any inherited from linked child test cases)
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(testCase.TestCaseId, true, true);
			Dictionary<string, TestRunParameter> parameters = new Dictionary<string, TestRunParameter>();
			if (testCaseParameters.Count > 0)
			{
				parameters = new Dictionary<string, TestRunParameter>();
				foreach (TestCaseParameter testCaseParameter in testCaseParameters)
				{
					//Add all parameters to the collection. If no default value specified, just
					//leave as empty string
					string parameterValue = String.Empty;
					if (!String.IsNullOrEmpty(testCaseParameter.DefaultValue))
					{
						parameterValue = testCaseParameter.DefaultValue;
					}

					//Prevent duplicates (shouldn't happen in theory) throwing an exception
					if (!parameters.ContainsKey(testCaseParameter.Name))
					{
						TestRunParameter parameter = new TestRunParameter(testCaseParameter.Name, parameterValue, TestRunParameter.ParameterValueType.Default);
						parameters.Add(testCaseParameter.Name, parameter);
					}
				}
			}

			//Now see if any test parameter values were set by the test set as a whole
			if (testSetId.HasValue)
			{
				TestSetManager testSetManager = new TestSetManager();
				List<TestSetParameter> testSetParameters = testSetManager.RetrieveParameterValues(testSetId.Value);
				foreach (TestSetParameter testSetParameter in testSetParameters)
				{
					//Find the matching parameter entry and update the value (and value type)
					if (parameters.ContainsKey(testSetParameter.Name) && parameters[testSetParameter.Name] != null)
					{
						parameters[testSetParameter.Name].Value = testSetParameter.Value;
						parameters[testSetParameter.Name].Type = TestRunParameter.ParameterValueType.TestSet;
					}
				}
			}

			//Now see if any test parameter values were set by the test set for the test case specifically
			//These override the test set general ones
			if (testSetTestCaseId.HasValue)
			{
				TestSetManager testSetManager = new TestSetManager();
				List<TestSetTestCaseParameter> testSetTestCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId.Value);
				foreach (TestSetTestCaseParameter testSetTestCaseParameter in testSetTestCaseParameters)
				{
					//Find the matching parameter entry and update the value (and value type)
					if (parameters.ContainsKey(testSetTestCaseParameter.Name) && parameters[testSetTestCaseParameter.Name] != null)
					{
						parameters[testSetTestCaseParameter.Name].Value = testSetTestCaseParameter.Value;
						parameters[testSetTestCaseParameter.Name].Type = TestRunParameter.ParameterValueType.TestSet;
					}
				}
			}

			//Finally, if we're using a test configuration, see if we have any parameter values to use,
			//they override any of the ones already set
			if (testConfigurationEntry != null)
			{
				foreach (KeyValuePair<string, TestRunParameter> parameter in parameters)
				{
					TestConfigurationEntry testConfigurationEntryParam = testConfigurationEntry.FirstOrDefault(t => t.ParameterName == parameter.Key);
					if (testConfigurationEntryParam != null)
					{
						parameters[parameter.Key].Value = testConfigurationEntryParam.ParameterValue;
						parameters[parameter.Key].Type = TestRunParameter.ParameterValueType.TestSet;
					}
				}
			}

			//Now load the test run steps
			PopulateTestRunSteps(testCase, testRun, parameters);
		}

		/// <summary>
		/// Loads the test run steps in for a provided test case
		/// </summary>
		/// <param name="testCase">The current test case</param>
		/// <param name="testRun">The currently populated test run</param>
		/// <param name="testRunsPending">The test run dataset we need to populate</param>
		/// <param name="parameters">A collection of parameter name,value pairs to search & replace on</param>
		protected void PopulateTestRunSteps(TestCase testCase, TestRun testRun, Dictionary<string, TestRunParameter> parameters)
		{
			//Now we want to handle any nested test steps
			foreach (TestStep testStep in testCase.TestSteps)
			{
				//Make a temp copy of the parameters dictionary so that we keep the main param "parameters" immutable
				Dictionary<string, TestRunParameter> parametersMutableClone = new Dictionary<string, TestRunParameter>();
				if (parameters.Count > 0)
				{
					foreach (KeyValuePair<string, TestRunParameter> parameter in parameters)
					{
						TestRunParameter testRunParameter = new TestRunParameter(parameter.Value.Name, parameter.Value.Value, parameter.Value.Type);
						parametersMutableClone.Add(parameter.Key, testRunParameter);
					}
				}

				//First we need to see if this is real test step or a reference to a linked test case
				if (testStep.LinkedTestCaseId.HasValue)
				{

					//See if we have any parameter values set for this link, if so, set the value
					//(overriding any default values)
					TestCaseManager testCaseManager = new TestCaseManager();
					List<TestStepParameter> testStepParameters = testCaseManager.RetrieveParameterValues(testStep.TestStepId);
					if (testStepParameters != null && testStepParameters.Count > 0)
					{
						foreach (TestStepParameter testStepParameter in testStepParameters)
						{
							string parameterName = testStepParameter.Name;
							string parameterValue = testStepParameter.Value;
							//See if we have a matching parameter and update the value unless it was a test set value
							//which takes precedence over a test link value
							if (parametersMutableClone.ContainsKey(parameterName) && parametersMutableClone[parameterName] != null && parametersMutableClone[parameterName].Type != TestRunParameter.ParameterValueType.TestSet)
							{
								parametersMutableClone[parameterName].Value = parameterValue;
								parametersMutableClone[parameterName].Type = TestRunParameter.ParameterValueType.TestLink;
							}
						}
					}

					//We need to recursively call this method to add the linked test cases
					TestCase linkedTestCase = testCaseManager.RetrieveByIdWithSteps(testCase.ProjectId, testStep.LinkedTestCaseId.Value);
					if (linkedTestCase != null && linkedTestCase.TestSteps.Count > 0)
					{
						PopulateTestRunSteps(linkedTestCase, testRun, parametersMutableClone);
					}
				}
				else
				{
					//Add the real test step row as new test run step - handle NULLs safely
					string expectedResult = null;
					string sampleData = null;
					string description = TestCaseManager.ReplaceParameters(testStep.Description, parameters);
					if (!String.IsNullOrEmpty(testStep.ExpectedResult))
					{
						expectedResult = TestCaseManager.ReplaceParameters(testStep.ExpectedResult, parameters);
					}
					if (!String.IsNullOrEmpty(testStep.SampleData))
					{
						sampleData = TestCaseManager.ReplaceParameters(testStep.SampleData, parameters);
					}
					int position = testRun.TestRunSteps.Count + 1;
					TestRunStep testRunStep = new TestRunStep();
					testRun.TestRunSteps.Add(testRunStep);
					testRunStep.TestStepId = testStep.TestStepId;
					testRunStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
					testRunStep.Description = description;
					testRunStep.ExpectedResult = expectedResult;
					testRunStep.SampleData = sampleData;
					testRunStep.TestCaseId = testCase.TestCaseId;
					testRunStep.Position = position;
				}
			}
		}

		#endregion

		#region Automated Test Recording Methods

		/// <summary>
		/// Updates the placeholder #GUID based URLs uploaded in test run step actual results with real attachment URLs
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <param name="attachmentId">The id of the attachment</param>
		/// <param name="guid">The #{....} format guid</param>
		/// <param name="projectId">The id of the project</param>
		public void TestRun_UpdateAttachmentImgUrls(int projectId, int testRunId, string guid, int attachmentId)
		{
			const string METHOD_NAME = "Record";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRunSteps
								where t.TestRunId == testRunId && t.TestRun.TestCase.ProjectId == projectId
								orderby t.Position, t.TestRunStepId
								select t;

					//Scan the text for guids and make the updates
					List<TestRunStep> testRunSteps = query.ToList();
					string baseUrl = UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Attachment, projectId, attachmentId, null);
					string appRoot = ConfigurationSettings.Default.General_WebServerUrl;
					string realUrl = baseUrl.Replace("~", appRoot);
					foreach (TestRunStep testRunStep in testRunSteps)
					{
						if (!String.IsNullOrWhiteSpace(testRunStep.ActualResult))
						{
							testRunStep.StartTracking();
							testRunStep.ActualResult = testRunStep.ActualResult.Replace(guid, realUrl);
						}
					}
					//Save changes without any notifications or history tracking
					context.SaveChanges();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Records a new automated test run for the given test case / release
		/// In addition to saving the test-run, depending on how the start/end dates
		/// compare against the underlying test case, it may also update the overall
		/// test case status codes
		/// </summary>
		/// <param name="projectId">The project the test case belongs to</param>
		/// <param name="testerUserId">The user id of the person who's running the test</param>
		/// <param name="testCaseId">The test case being executed</param>
		/// <param name="releaseId">The release being executed against</param>
		/// <param name="executionStatusId">The status of the test run (pass/fail/not run)</param>
		/// <param name="runnerName">The name of the automated testing tool</param>
		/// <param name="runnerAssertCount">The number of assertions</param>
		/// <param name="runnerMessage">The failure message (if appropriate)</param>
		/// <param name="runnerStackTrace">The error stack trace (if any)s</param>
		/// <param name="runnerTestName">The name of the test within the runner</param>
		/// <param name="endDate">When the test run ended</param>
		/// <param name="startDate">When the test run started</param>
		/// <param name="testSetId">The id of the test set to record the run against (optional)</param>
		/// <param name="testSetTestCaseId">The unique id of the test case in the test set (optional)</param>
		/// <param name="automationEngineId">The id of the automation engine being used</param>
		/// <param name="automationHostId">The id of the automation host it was executed on</param>
		/// <param name="updateProjectTestStatus">Should we update the summary test status of the associated requirements, releases and test cases</param>
		/// <param name="buildId">The id of the build the test is being executed on</param>
		/// <param name="testRunFormat">The format of the test run (for automated test runs only)</param>
		/// <param name="testRunSteps">Any automated test run steps</param>
		/// <returns>The newly created test run id</returns>
		/// <remarks>
		/// This method should only be used for legacy Automated test runs. Use the CreateFromTestCase
		/// and Save methods for executing manual runs.
		/// </remarks>
		public int Record(int projectId, int testerUserId, int testCaseId, int? releaseId, int? testSetId, int? testSetTestCaseId, DateTime startDate, DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int? runnerAssertCount, string runnerMessage, string runnerStackTrace, int? automationHostId, int? automationEngineId, int? buildId, TestRun.TestRunFormatEnum testRunFormat, List<TestRunStepInfo> testRunSteps, bool updateProjectTestStatus = true)
		{
			const string METHOD_NAME = "Record";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create a test case business object and instantiate the passed in test case
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
				int testRunId;

				//If we are passed a test set, get that and override the release if necessary
				if (testSetId.HasValue)
				{
					TestSetManager testSetManager = new TestSetManager();
					TestSetView testSetView = testSetManager.RetrieveById(projectId, testSetId.Value);
					if (testSetView.ReleaseId.HasValue)
					{
						releaseId = testSetView.ReleaseId.Value;
					}

					//If the test set is populated, but not the test set test case id, we need to get the first matching test case in the set
					//This allows the older v1.x and v2.2 APIs to still use this method
					if (!testSetTestCaseId.HasValue)
					{
						List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId.Value);
						foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
						{
							if (testSetTestCase.TestCaseId == testCaseId)
							{
								testSetTestCaseId = testSetTestCase.TestSetTestCaseId;
								break;
							}
						}
					}
					if (!testSetTestCaseId.HasValue)
					{
						//If no match found, unset the test set id as well
						testSetId = null;
					}
				}

				TestRun testRun;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create a new test run
					testRun = new TestRun();

					//Calculate the actual duration (in whole minutes)
					int actualDuration = (int)endDate.Subtract(startDate).TotalMinutes;

					//Populate the automated test run
					testRun.Name = testCase.Name;
					testRun.Description = testCase.Description;
					testRun.TestCaseId = testCase.TestCaseId;
					testRun.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Automated;
					testRun.TesterId = testerUserId;
					testRun.ExecutionStatusId = executionStatusId;
					testRun.ReleaseId = releaseId;
					testRun.TestSetId = testSetId;
					testRun.TestSetTestCaseId = testSetTestCaseId;
					testRun.AutomationHostId = automationHostId;
					testRun.AutomationEngineId = automationEngineId;
					testRun.BuildId = buildId;
					testRun.TestRunFormatId = (int)testRunFormat;
					testRun.EstimatedDuration = testCase.EstimatedDuration;
					testRun.ActualDuration = actualDuration;
					testRun.StartDate = startDate;
					testRun.EndDate = endDate;
					testRun.RunnerName = runnerName;
					testRun.RunnerTestName = runnerTestName;
					testRun.RunnerAssertCount = runnerAssertCount;
					testRun.RunnerMessage = runnerMessage;
					testRun.RunnerStackTrace = runnerStackTrace;
					testRun.IsAttachments = false;
					testRun.ConcurrencyDate = DateTime.UtcNow;

					//Get the latest ChangeSet ID for this project.
					testRun.ChangeSetId = context.HistoryChangeSets
						.Where(f => f.ProjectId == testCase.ProjectId)
						.OrderByDescending(f => f.ChangeSetId)
						.Select(f => (long?)f.ChangeSetId) //We cast this to a nullable long, because otherwise 'default' is "0".
						.FirstOrDefault();
					//That may look clunky instead of something easier like the Max() LINQ, but the MAX() would cause it to 
					//  loop through everything in-memory, instead of handling it on the SQL Server side.

					//If we have any test steps we need to persist them
					if (testRunSteps != null && testRunSteps.Count > 0)
					{
						foreach (TestRunStepInfo testRunStep in testRunSteps)
						{
							//Make sure that the test step id exists in the test case
							int? testStepId = null;
							if (testRunStep.TestStepId.HasValue && testCase.TestSteps.Any(s => s.TestStepId == testRunStep.TestStepId.Value))
							{
								testStepId = testRunStep.TestStepId.Value;
							}

							//Record the step and populate the test run step id
							//Add the new row to the existing dataset
							TestRunStep testRunStepRow = new TestRunStep();
							testRunStepRow.TestStepId = testStepId;
							testRunStepRow.ExecutionStatusId = testRunStep.ExecutionStatusId;
							testRunStepRow.Description = testRunStep.Description;
							testRunStepRow.Position = testRunStep.Position;
							testRunStepRow.ExpectedResult = testRunStep.ExpectedResult;
							testRunStepRow.SampleData = testRunStep.SampleData;
							testRunStepRow.ActualResult = testRunStep.ActualResult;
							testRunStepRow.StartDate = testRunStep.StartDate;
							testRunStepRow.EndDate = testRunStep.EndDate;
							testRunStepRow.ActualDuration = testRunStep.ActualDuration;
							testRunStepRow.TestCaseId = null;

							//Add to test run
							testRun.TestRunSteps.Add(testRunStepRow);
						}
					}

					//Now lets actually persist the test run with its steps
					context.TestRuns.AddObject(testRun);
					context.SaveChanges();

					testRunId = testRun.TestRunId;

					//Also populate the matching test run steps
					if (testRunSteps != null)
					{
						foreach (TestRunStepInfo testRunStepInfo in testRunSteps)
						{
							TestRunStep testRunStep = testRun.TestRunSteps.FirstOrDefault(s => s.Position == testRunStepInfo.Position);
							if (testRunStep != null)
							{
								testRunStepInfo.TestRunStepId = testRunStep.TestRunStepId;
							}
						}
					}
				}

				//Now we need to update the test case 'last run' information if appropriate
				//(if we have a more recent end-date than the last-execution-date)
				RefreshTestCaseExecutionStatus2(projectId, testRunId);

				//Fire test case notifications for status changes
				try
				{
					NotificationManager notificationManager = new NotificationManager();

					if (testRun != null)
					{
						//We apply the changes to the test case, for any matching properties
						testCase.StartTracking();
						if (testRun.ExecutionStatusId != testCase.ExecutionStatusId)
						{
							testCase.ExecutionStatusId = testRun.ExecutionStatusId;
						}
						if (testRun.EndDate != testCase.ExecutionDate)
						{
							testCase.ExecutionDate = testRun.EndDate;
						}
						if (testRun.ActualDuration != testCase.ActualDuration)
						{
							testCase.ActualDuration = testRun.ActualDuration;
						}
						notificationManager.SendNotificationForArtifact(testCase, null, null);
					}
				}
				catch (Exception exception)
				{
					//Log but don't throw
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}


		#endregion

		#region Test Run methods

		/// <summary>Handles any test run specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleTestRunSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			string filterProperty = filter.Key;
			object filterValue = filter.Value;

			//Handle the special case of release filters where we want to also retrieve child iterations
			if (filterProperty == "ReleaseId" && (int)filterValue != NoneFilterValue && projectId.HasValue)
			{
				//Get the release and its child iterations
				int releaseId = (int)filterValue;
				List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId.Value, releaseId);
				ConstantExpression releaseIdsExpression = LambdaExpression.Constant(releaseIds);
				MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "ReleaseId");
				//Equivalent to: p => releaseIds.Contains(p.ReleaseId) i.e. (RELEASE_ID IN (1,2,3))
				Expression releaseExpression = Expression.Call(releaseIdsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
				expressionList.Add(releaseExpression);
				return true;
			}

			return false;
		}

		/// <summary>
		///	Retrieves a single test run in the system that has a certain ID
		/// </summary>
		/// <param name="testRunId">The ID of the test run to be returned</param>
		/// <returns>Test Run view</returns>
		public TestRunView RetrieveById1(int testRunId)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestRunView testRun;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Actually execute the query and get the entity
					var query = from t in context.TestRunsView
								where t.TestRunId == testRunId
								select t;

					testRun = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (testRun == null)
				{
					throw new ArtifactNotExistsException("Test Run " + testRunId + " doesn't exist in the system.");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRun;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		///	Retrieves a single test run in the system that has a certain ID
		/// </summary>
		/// <param name="testRunId">The ID of the test run to be returned</param>
		/// <returns>Test Run view</returns>
		public TestRunView RetrieveById(int testRunId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestRunView testRun;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Actually execute the query and get the entity
					var query = from t in context.TestRunsView
								where t.TestRunId == testRunId && (!t.IsDeleted || includeDeleted)
								select t;

					testRun = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (testRun == null)
				{
					throw new ArtifactNotExistsException("Test Run " + testRunId + " doesn't exist in the system.");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRun;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TestRun RetrieveById2(int testRunId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestRun testRun;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Actually execute the query and get the entity
					var query = from t in context.TestRuns
								where t.TestRunId == testRunId && (!t.IS_DELETED || includeDeleted)
								select t;

					testRun = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (testRun == null)
				{
					throw new ArtifactNotExistsException("Test Run " + testRunId + " doesn't exist in the system.");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRun;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		///	Counts all the test runs in the project
		/// </summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>The total number of test runs</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Count(int projectId, Hashtable filters = null, double utcOffset = 0, bool activeReleasesOnly = false)
		{
			const string METHOD_NAME = "Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int count = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query, exclude test runs that are 'not run'!
					var query = from t in context.TestRunsView
								where t.ProjectId == projectId && !t.IsDeleted && t.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun
								select t;

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TestRunView, bool>> filterClause = CreateFilterExpression<TestRunView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, filters, utcOffset, null, HandleTestRunSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TestRunView>)query.Where(filterClause);
						}
					}

					//Filter out test runs run against inactive releases
					if (activeReleasesOnly)
					{
						int[] inactiveReleaseStatuses = new int[] {
							(int)DataModel.Release.ReleaseStatusEnum.Closed,
							(int)DataModel.Release.ReleaseStatusEnum.Deferred,
							(int)DataModel.Release.ReleaseStatusEnum.Cancelled
						};
						query = from t in query
								join r in context.Releases on t.ReleaseId equals r.ReleaseId
								where !inactiveReleaseStatuses.Contains(r.ReleaseStatusId)
								select t;
					}

					//Get the count
					count = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Counts all the test cases in the release</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="folderId">
		/// The folder to filter by, null = root folder, TEST_CASE_FOLDER_ID_ALL_TEST_CASES = all test cases
		/// </param>
		/// <param name="countAllFolders">Should we count the test cases in all folders in the project</param>
		/// <returns>The total number of test cases</returns>
		/// <remarks>Used to help with pagination</remarks>
		//public int CountByRelease(int projectId, int releaseId, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool countAllFolders = false)
		//{
		//	const string METHOD_NAME = "CountByRelease";

		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

		//	try
		//	{
		//		int testCaseCount = 0;
		//		using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
		//		{
		//			//Build the base query
		//			var query = from t in context.TestReleasesView
		//						where
		//							(!t.IsDeleted || includeDeleted) &&
		//							t.ProjectId == projectId &&
		//							t.ReleaseId == releaseId
		//						select t;

		//			//Add the dynamic filters
		//			if (filters != null)
		//			{
		//				//Get the template for this project
		//				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

		//				//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
		//				Expression<Func<TestCaseReleaseView, bool>> filterClause = CreateFilterExpression<TestCaseReleaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, filters, utcOffset, null, HandleTestCaseSpecificFilters);
		//				if (filterClause != null)
		//				{
		//					query = (IOrderedQueryable<TestCaseReleaseView>)query.Where(filterClause);
		//				}
		//			}

		//			//See if we need to filter by folder
		//			if (folderId.HasValue)
		//			{
		//				int folderIdValue = folderId.Value;
		//				if (folderIdValue != TEST_CASE_FOLDER_ID_ALL_TEST_CASES)
		//				{
		//					query = query.Where(t => t.TestCaseFolderId == folderIdValue);
		//				}
		//			}
		//			else if ((filters == null || filters.Count == 0) && !countAllFolders)
		//			{
		//				//test cases that have no folder (i.e. root), unless we have filters in which case show all
		//				query = query.Where(t => !t.TestCaseFolderId.HasValue);
		//			}

		//			//Get the count
		//			testCaseCount = query.Count();
		//		}

		//		Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//		return testCaseCount;
		//	}
		//	catch (Exception exception)
		//	{
		//		Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
		//		Logger.Flush();
		//		throw;
		//	}
		//}


		/// <summary>
		/// Creates a test run from the test cases contained within a test set. Also sets the test run to the same
		/// release as specified by the test set (if so specified)
		/// </summary>
		/// <param name="userId">The user executing the test</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="testSetId">The test set we want to execute</param>
		/// <param name="savePending">Save a copy of this test run shell to the TestRunsPending table</param>
		/// <param name="updateBackgroundProcessStatus">Callback used to report back the status of the function</param>
		/// <returns>The newly populated test run dataset</returns>
		/// <remarks>
		/// 1) Only use this method for executing manual runs
		/// 2) Set the savePending flag to true when actually executing a test run through the user interface,
		///     and leave it as false when using this function to build a temporary instance of a test run
		///     (e.g. in the printable test scripts report) or when being used by the import API
		/// 3) If the test set has an associated test configuration set, we use those parameter values rather than those in the
		///     test set or the test cases, if we have a conflict
		/// </remarks>
		public TestRunsPending CreateFromTestSet(int userId, int projectId, int testSetId, bool savePending, UpdateBackgroundProcessStatus updateBackgroundProcessStatus = null)
		{
			const string METHOD_NAME = "CreateFromTestSet";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(0, GlobalResources.Messages.TestRun_CreatingTestRuns);
				}

				//First lets retrieve the test set in question
				TestSetManager testSetManager = new Business.TestSetManager();
				TestCaseManager testCaseManager = new Business.TestCaseManager();

				//The RetrieveById will throw an exception if the test set doesn't exist, so no additional check needed here
				//It also gets the test cases associated with the test set
				TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId, false, true);
				string testSetName = testSet.Name;

				//If we have an automated test set this can't be executed within the application
				//so throw a special exception that will get caught by the page and handled
				if (testSet.TestRunTypeId == (int)TestRun.TestRunTypeEnum.Automated)
				{
					throw new TestSetNotManualException("This function can only handle manual test sets", testSetId);
				}

				//See if we have a release or automation host associated with the test set
				int? releaseId = testSet.ReleaseId;
				int? automationHostId = testSet.AutomationHostId;

				//Make sure we have at least one test case
				TestRunsPending testRunsPending = new TestRunsPending();
				if (testSet.TestSetTestCases.Count < 1)
				{
					throw new TestRunNoTestCasesException(GlobalResources.Messages.TestRun_TestSetNotContainTestCases);
				}

				//See if we have a test configuration set, if so, then we need to loop through all those combinations
				if (testSet.TestConfigurationSetId.HasValue)
				{
					TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
					List<TestConfigurationEntry> testConfigurationEntries = testConfigurationManager.RetrieveEntries(projectId, testSet.TestConfigurationSetId.Value);
					if (testConfigurationEntries != null && testConfigurationEntries.Count > 0)
					{
						int count = 0;
						List<IGrouping<int, TestConfigurationEntry>> testConfigurationsGroupedByConfiguration = testConfigurationEntries.GroupBy(t => t.TestCaseConfigurationId).ToList();
						int total = testConfigurationsGroupedByConfiguration.Count * testSet.TestSetTestCases.Count;
						foreach (IGrouping<int, TestConfigurationEntry> testConfigurationEntry in testConfigurationsGroupedByConfiguration)
						{
							//Iterate through the test set's test cases and populate the test run
							foreach (TestSetTestCase testSetTestCase in testSet.TestSetTestCases)
							{
								if (testCaseManager.IsTestCaseInExecutableStatus(testSetTestCase.TestCaseId))
								{
									//We need to retrieve the test case itself with steps, also check if we have an override owner
									int testerId = userId;
									if (testSetTestCase.OwnerId.HasValue)
									{
										testerId = testSetTestCase.OwnerId.Value;
									}
									try
									{
										TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testSetTestCase.TestCaseId);
										PopulateTestRun(testCase, testRunsPending, testerId, releaseId, testSetId, testSetTestCase.TestSetTestCaseId, automationHostId, null, testConfigurationEntry);
									}
									catch (ArtifactNotExistsException)
									{
										//Occurs if the test case has been marked as deleted, just ignore and continue
									}
								}

								//Update the progress
								if (updateBackgroundProcessStatus != null)
								{
									//The last 5% is reserved for the saving of the pending test run
									int percentProgress = count * 95;
									percentProgress /= total;
									updateBackgroundProcessStatus(percentProgress, GlobalResources.Messages.TestRun_CreatingTestRuns);
								}
								count++;
							}
						}
					}
					else
					{
						//Stop and show message if there is a test configuration set but that configuration has no entries 
						throw new TestRunNoTestCasesException(GlobalResources.Messages.TestRun_TestConfigurationBlank);
					}
				}
				else
				{
					//Iterate through the test set's test cases and populate the test run
					int count = 0;
					foreach (TestSetTestCase testSetTestCase in testSet.TestSetTestCases)
					{
						//Make sure this test case can be executed due to its workflow status
						if (testCaseManager.IsTestCaseInExecutableStatus(testSetTestCase.TestCaseId))
						{
							//We need to retrieve the test case itself with steps, also check if we have an override owner
							int testerId = userId;
							if (testSetTestCase.OwnerId.HasValue)
							{
								testerId = testSetTestCase.OwnerId.Value;
							}
							try
							{
								TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testSetTestCase.TestCaseId);
								PopulateTestRun(testCase, testRunsPending, testerId, releaseId, testSetId, testSetTestCase.TestSetTestCaseId, automationHostId, null);
							}
							catch (ArtifactNotExistsException)
							{
								//Occurs if the test case has been marked as deleted, just ignore and continue
							}
						}

						//Update the progress
						if (updateBackgroundProcessStatus != null)
						{
							//The last 5% is reserved for the saving of the pending test run
							int percentProgress = count * 95;
							percentProgress /= testSet.TestSetTestCases.Count;
							updateBackgroundProcessStatus(percentProgress, GlobalResources.Messages.TestRun_CreatingTestRuns);
						}
						count++;
					}
				}

				//Persist the test runs and pending record if appropriate
				if (savePending)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(99, GlobalResources.Messages.TestRun_SavingPendingEntry);
					}
					SavePendingForTestSet(testSet, testRunsPending, projectId, userId, testSetId, testSetName, releaseId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunsPending;
			}
			catch (TestSetNotManualException)
			{
				//We don't log as it's an expected case
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a test run from a selected group of test cases in a test set
		/// </summary>
		/// <param name="userId">The user executing the test</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="testSetId">The test set we want to execute</param>
		/// <param name="testSetTestCaseIds">The list of TestSetTestCaseIds to execute from the set</param>
		/// <param name="updateBackgroundProcessStatus">Callback used to report back the status of the function</param>
		/// <param name="savePending">Save a copy of this test run shell to the TestRunsPending table</param>
		/// <returns>The newly populated test run dataset</returns>
		public TestRunsPending CreateFromTestCasesInSet(int userId, int projectId, int testSetId, List<int> testSetTestCaseIds, bool savePending, UpdateBackgroundProcessStatus updateBackgroundProcessStatus = null)
		{
			const string METHOD_NAME = "CreateFromTestSet";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(0, GlobalResources.Messages.TestRun_CreatingTestRuns);
				}

				//First lets retrieve the test set in question
				TestSetManager testSetManager = new Business.TestSetManager();
				TestCaseManager testCaseManager = new Business.TestCaseManager();

				//The RetrieveById will throw an exception if the test set doesn't exist, so no additional check needed here
				//It also gets the test cases associated with the test set
				TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId, false, true);
				string testSetName = testSet.Name;

				//See if we have a release or automation host associated with the test set
				int? releaseId = testSet.ReleaseId;
				int? automationHostId = testSet.AutomationHostId;

				//Make sure we have at least one test case
				TestRunsPending testRunsPending = new TestRunsPending();
				if (testSet.TestSetTestCases.Count < 1)
				{
					throw new TestRunNoTestCasesException(GlobalResources.Messages.TestRun_TestSetNotContainTestCases);
				}

				//Iterate through the test set's test cases and populate the test run
				int count = 0;
				foreach (TestSetTestCase testSetTestCase in testSet.TestSetTestCases)
				{
					//We don't use override owner when executing a specific group of tests in the set
					//Need to make sure that the id of the test case is in our list
					if (testSetTestCaseIds.Contains(testSetTestCase.TestSetTestCaseId) && testCaseManager.IsTestCaseInExecutableStatus(testSetTestCase.TestCaseId))
					{
						TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testSetTestCase.TestCaseId);
						PopulateTestRun(testCase, testRunsPending, userId, releaseId, testSetId, testSetTestCase.TestSetTestCaseId, automationHostId, null);
					}
					//Update the progress
					if (updateBackgroundProcessStatus != null)
					{
						//The last 5% is reserved for the saving of the pending test run
						int percentProgress = count * 95;
						percentProgress /= testSet.TestSetTestCases.Count;
						updateBackgroundProcessStatus(percentProgress, GlobalResources.Messages.TestRun_CreatingTestRuns);
					}
					count++;
				}

				//Persist the test runs and pending record if appropriate
				if (savePending)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(99, GlobalResources.Messages.TestRun_SavingPendingEntry);
					}
					SavePendingForTestSet(testSet, testRunsPending, projectId, userId, testSetId, testSetName, releaseId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunsPending;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a shell manual test run with associated test run steps
		/// from a passed set of test case IDs and/or folder IDs
		/// </summary>
		/// <param name="projectId">The project that the test case(s) belong to</param>
		/// <param name="userId">The user that is currently logged in</param>
		/// <param name="releaseId">The release being tested (Optional)</param>
		/// <param name="testFolderExecutionList">The passed in list of test folder IDs (optional)</param>
		/// <param name="testCaseExecutionList">The passed in list of test case IDs</param>
		/// <param name="savePending">Save a copy of this test run shell to the TestRunsPending table</param>
		/// <param name="updateBackgroundProcessStatus">Callback used to report back the status of the function</param>
		/// <returns>A shell test run data set</returns>
		/// <remarks>
		/// 1) Only use this method for executing manual runs
		/// 2) Set the savePending flag to true when actually executing a test run through the user interface,
		///     and leave it as false when using this function to build a temporary instance of a test run
		///     (e.g. in the printable test scripts report) or when being used by the import API
		/// </remarks>
		public TestRunsPending CreateFromTestCase(int userId, int projectId, int? releaseId, List<int> testCaseExecutionList, bool savePending, List<int> testFolderExecutionList = null, UpdateBackgroundProcessStatus updateBackgroundProcessStatus = null)
		{
			const string METHOD_NAME = "CreateFromTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have at least one test case or folder
			if ((testCaseExecutionList == null || testCaseExecutionList.Count < 1) && (testFolderExecutionList == null || testFolderExecutionList.Count < 1))
			{
				throw new TestRunNoTestCasesException("You need to pass in at least one test case / folder");
			}

			//Create a test case business object and empty test run dataset
			TestCaseManager testCaseManager = new TestCaseManager();
			TestRunsPending testRunsPending = new TestRunsPending();

			try
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(0, GlobalResources.Messages.TestRun_CreatingTestRuns);
				}

				//See how many we have to run through
				int totalCount = 0;
				if (testCaseExecutionList != null)
				{
					totalCount += testCaseExecutionList.Count;
				}
				if (testFolderExecutionList != null)
				{
					totalCount += testFolderExecutionList.Count;
				}
				int runningCount = 0;
				//Loop through the test folders
				if (testFolderExecutionList != null)
				{
					foreach (int testCaseFolderId in testFolderExecutionList)
					{
						PopulateFromTestFolder(projectId, testCaseFolderId, testRunsPending, userId, releaseId);

						//Update the progress
						if (updateBackgroundProcessStatus != null)
						{
							//The last 5% is reserved for the saving of the pending test run
							int percentProgress = runningCount * 95;
							percentProgress /= totalCount;
							updateBackgroundProcessStatus(percentProgress, GlobalResources.Messages.TestRun_CreatingTestRuns);
						}
						runningCount++;
					}
				}

				//Loop through the test cases
				if (testCaseExecutionList != null)
				{
					foreach (int testCaseId in testCaseExecutionList)
					{
						//Make sure the test case is in an executable status
						if (testCaseManager.IsTestCaseInExecutableStatus(testCaseId))
						{
							//Retrieve the testcase, its children and associated steps
							TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);

							//Now iterate through the test cases and add any that aren't already there to the test run
							this.PopulateTestRun(testCase, testRunsPending, userId, releaseId, null, null, null, null);
						}

						//Update the progress
						if (updateBackgroundProcessStatus != null)
						{
							//The last 5% is reserved for the saving of the pending test run
							int percentProgress = runningCount * 95;
							percentProgress /= testCaseExecutionList.Count;
							updateBackgroundProcessStatus(percentProgress, GlobalResources.Messages.TestRun_CreatingTestRuns);
						}
						runningCount++;
					}
				}

				//Make sure we have at least one test run populated
				if (testRunsPending.TestRuns.Count < 1)
				{
					throw new TestRunNoTestCasesException("You need to pass in at least one test case");
				}


				//If we have more than one test case (not part of a test set) then
				//We use a generic entry as the name of the list of pending test runs
				if (testRunsPending.TestRuns.Count > 1)
				{
					PopulatePending(projectId, userId, testRunsPending, GlobalResources.General.TestRun_VariousTestCases);
				}
				else
				{
					//Otherwise use the name of the test case / run itself
					PopulatePending(projectId, userId, testRunsPending, testRunsPending.TestRuns[0].Name);
				}

				//Persist the test runs and pending entry if appropriate
				if (savePending)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(99, GlobalResources.Messages.TestRun_SavingPendingEntry);
					}

					Save(testRunsPending, projectId);

					//Also need to check to see if any test cases (not folders) need to get added to the selected release
					if (releaseId.HasValue)
					{
						List<int> testCaseIds = new List<int>();
						List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId.Value);
						List<int> testCasesBeingExecuted = testRunsPending.TestRuns.Select(t => t.TestCaseId).ToList();
						foreach (int testCaseId in testCasesBeingExecuted)
						{
							//If the test case is not already mapped, add it to the list
							if (!mappedTestCases.Any(t => t.TestCaseId == testCaseId))
							{
								testCaseIds.Add(testCaseId);
							}
						}
						testCaseManager.AddToRelease(projectId, releaseId.Value, testCaseIds, userId);

						//If we have an iteration/phase, also need to add to the parent release
						try
						{
							ReleaseManager releaseManager = new ReleaseManager();
							ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId.Value);
							if ((release.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || release.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase) && release.IndentLevel.Length > 3)
							{
								ReleaseView parentRelease = releaseManager.RetrieveByIndentLevel(User.UserInternal, projectId, release.IndentLevel.SafeSubstring(0, release.IndentLevel.Length - 3)).FirstOrDefault();
								if (parentRelease != null)
								{
									testCaseManager.AddToRelease(projectId, parentRelease.ReleaseId, testCaseIds, userId);
								}
							}
						}
						catch (ArtifactNotExistsException exception)
						{
							//The release/iteration no longer exists, just log a warning and ignore
							Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
						}
					}
				}

				//Update progress
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(100, GlobalResources.Messages.TestRun_CreationCompleted);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				return testRunsPending;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		///	Retrieves a single test run in the system and includes the associated test steps
		/// </summary>
		/// <param name="testRunId">The ID of the test run to be returned</param>
		/// <returns>Test Run</returns>
		public TestRun RetrieveByIdWithSteps(int testRunId)
		{
			const string METHOD_NAME = "RetrieveByIdWithSteps";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestRun testRun;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we get the test run itself
					var query1 = from t in context.TestRuns
									.Include(t => t.ExecutionStatus)
								 where t.TestRunId == testRunId
								 select t;

					testRun = query1.FirstOrDefault();

					//Now we get the steps, which are implicitly joined by EF4 'fix-up'
					var query2 = from s in context.TestRunSteps
									.Include(s => s.ExecutionStatus)
									.Include(s => s.TestStep)
								 where s.TestRunId == testRunId
								 orderby s.Position, s.TestRunStepId
								 select s;

					//Fire the second query to initiate 'fix-up'
					query2.ToList();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (testRun == null)
				{
					throw new ArtifactNotExistsException("Test Run " + testRunId + " doesn't exist in the system.");

				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRun;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		///	Retrieves a single test run in the system that has a certain ID
		/// </summary>
		/// <param name="testRunId">The ID of the test run to be returned</param>
		/// <returns>Test Run</returns>
		public TestRun RetrieveById2(int testRunId)
		{
			const string METHOD_NAME = "RetrieveById2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestRun testRun;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Actually execute the query and get the entity
					var query = from t in context.TestRuns
								where t.TestRunId == testRunId
								select t;

					testRun = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (testRun == null)
				{
					throw new ArtifactNotExistsException("Test Run " + testRunId + " doesn't exist in the system.");

				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRun;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes a test runs and its associated test run steps, custom properties and attachments
		/// </summary>
		/// <param name="testRunId">The test run that we want to delete</param>
		/// <param name="projectId">The id of the project the test run belongs to</param>
		public void Delete(int testRunId, int? userId  = null, int? testrunProjectId = null)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the test run to get its test case information
				TestRun testRun = this.RetrieveById2(testRunId);
				string testRunName = testRun.Name;
				int testCaseId = testRun.TestCaseId;
				int projectId = 0;
				//Capture the project id
				if (testRun.ProjectId == 0)
				{
					projectId = (int)testrunProjectId;
				}
				else
				{
					projectId = testRun.ProjectId;
				}
				//First we need to delete any attachments associated with the test run
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.DeleteByArtifactId(testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun);

				//Next we need to delete any custom properties associated with the test run
				new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun);

				//Now perform the delete using a stored procedure
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.TestRun_Delete(testRunId);
				}
				//Log the purge.
				new HistoryManager().LogPurge(projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRunId, DateTime.UtcNow, testRunName);

				//Finally we need to refresh the execution status of the underlying test case from all its test runs
				RefreshTestCaseExecutionStatus3(projectId, testCaseId);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Marks a test run set as being deleted.</summary>
		/// <param name="testRunId">The test run set ID.</param>
		/// <param name="userId">The user performing the delete.</param>
		public void MarkTestRunAsDeleted(int projectId, int testRunId, int userId)
		{
			const string METHOD_NAME = "MarkTestRunAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the automation host to see that it exists
				bool deletePerformed = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRuns
								where t.TestRunId == testRunId
								select t;

					TestRun testRun = query.FirstOrDefault();
					if (testRun != null)
					{
						//Mark as deleted
						testRun.StartTracking();
						testRun.IS_DELETED = true;
						context.SaveChanges();
						deletePerformed = true;
					}
				}

				if (deletePerformed)
				{
					//Add a changeset to mark it as deleted.
					new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRunId, DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Deletes all test runs and test run steps associated with a particular test case
		/// </summary>
		/// <param name="testCaseId">The test case that all runs are to be deleted for</param>
		/// <remarks>This method can only be called by other business components</remarks>
		protected internal void DeleteByTestCaseId(int testCaseId)
		{
			const string METHOD_NAME = "DeleteByTestCaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Setup the parameterized database command to map to the dataset columns
				//We need to remove any links that incidents have to the deleted test run steps
				//And any associated custom properties
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.TestRun_DeleteByTestCase(testCaseId, (int)DataModel.Artifact.ArtifactTypeEnum.TestRun);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns>List of soft-deleted tasks</returns>
		public List<TestRunView> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunView> testRuns;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TestRunsView
								where t.ProjectId == projectId && t.IsDeleted
								orderby t.TestRunId
								select t;

					testRuns = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRuns;
			}
			catch (Exception ex)
			{
				//Do not rethrow, just return an empty list
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				return new List<TestRunView>();
			}
		}

		/// <summary>Deletes a task in the system that has the specified ID</summary>
		/// <param name="taskId">The ID of the task to be deleted</param>
		public void DeleteFromDatabase(int testRunId, int userId)
		{
			const string METHOD_NAME = "DeleteFromDatabase()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the task to see if it's linked to a requirement
				TestRunView testRun = null;
				try
				{
					testRun = RetrieveById(testRunId, true);
				}
				catch (ArtifactNotExistsException)
				{
					//If it's already deleted, just fail quietly
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}
				int projectId = testRun.ProjectId;
				string testRunName = testRun.Name;

				//Next we need to delete any custom properties associated with the task			
				new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun);

				//Finally call the stored procedure to delete the task itself
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.TestRun_Delete(testRunId);
				}

				//Log the purge.
				new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRunId, DateTime.UtcNow, testRunName);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Refreshes the execution status and last-run date of the test cases linked to the test runs being updated
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="testRunsPending">The set of test runs</param>
		/// <param name="runAsync">Run asynchronously (default = true)</param>
		/// <param name="currentTestRun">The current test run (otherwise assume all)</param>
		/// <param name="testRunChanges">The test run changes (used by notifications)</param>
		/// <remarks>
		/// Since the user doesn't need to see the results on this, we run it as a background thread rather than
		/// in the main ASP.NET thread itself. This gives a more responsive user interface
		/// </remarks>
		protected void RefreshTestCaseExecutionStatus(int projectId, TestRunsPending testRunsPending, bool runAsync = true, TestRun currentTestRun = null, Dictionary<string, object> testRunChanges = null)
		{
			const string METHOD_NAME = "RefreshTestCaseExecutionStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have not disabled rollups, if so log.
			if (new ProjectSettings(projectId).RollupCalculationsDisabled)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
				return;
			}

			//Need to pass a single object to the method so need to package into a state collection
			Dictionary<string, object> state = new Dictionary<string, object>();
			state.Add("projectId", projectId);
			state.Add("testRunsPendingId", testRunsPending.TestRunsPendingId);
			if (currentTestRun != null)
			{
				state.Add("currentTestRun", currentTestRun);
			}
			if (testRunChanges != null)
			{
				state.Add("testRunChanges", testRunChanges);
			}

			//See if we can queue this operation in the thread-pool. If not then just call directly as a backup option
			if (runAsync && ThreadPool.QueueUserWorkItem(new WaitCallback(RefreshTestCaseExecutionStatus_CallBack), state))
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "RefreshTestCaseExecutionStatus_CallBack initiated as a background thread");
			}
			else
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "RefreshTestCaseExecutionStatus_CallBack initiated as part of main thread");
				RefreshTestCaseExecutionStatus_CallBack(state);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// The implementation of the RefreshTestCaseExecutionStatus method that can be called synchronously or asychronously
		/// </summary>
		/// <param name="state">The context passed to the asynchronous handler</param>
		private void RefreshTestCaseExecutionStatus_CallBack(object state)
		{
			const string METHOD_NAME = "RefreshTestCaseExecutionStatus_CallBack";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Unpack the testrunspending set and project from the state dictionary
				Dictionary<string, object> stateContext = (Dictionary<string, object>)state;

				int projectId = (int)stateContext["projectId"];
				int testRunsPendingId = (int)stateContext["testRunsPendingId"];
				TestRun currentTestRun = null;
				if (stateContext.ContainsKey("currentTestRun"))
				{
					currentTestRun = (TestRun)stateContext["currentTestRun"];
				}
				Dictionary<string, object> testRunChanges = null;
				if (stateContext.ContainsKey("testRunChanges"))
				{
					testRunChanges = (Dictionary<string, object>)stateContext["testRunChanges"];
				}

				//Call the stored procedure to update the test cases
				//and get a list of all the folders in the pending run
				List<int> testCaseFolderIds;
				TestRunsPending testRunsPending;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;

					//Restore the pending id and make sure it still exists
					var query1 = from t in context.TestRunsPendings.Include(t => t.TestRuns)
								 where t.TestRunsPendingId == testRunsPendingId
								 select t;

					testRunsPending = query1.FirstOrDefault();
					if (testRunsPending == null)
					{
						return;
					}

					//Update the test case execution status
					context.TestRun_RefreshTestCaseExecutionStatus(projectId, testRunsPendingId);

					var query2 = from t in context.TestCases
								 where
									 t.TestCaseFolderId.HasValue &&
									 (t.TestRuns.Any(r => r.TestRunsPendingId == testRunsPendingId) ||
									 t.TestRunSteps.Any(s => s.TestRun.TestRunsPendingId == testRunsPendingId))
								 select t.TestCaseFolderId.Value;

					testCaseFolderIds = query2.Distinct().ToList();
				}

				//Update the folders summary status
				TestCaseManager testCaseManager = new TestCaseManager();
				foreach (int testCaseFolderId in testCaseFolderIds)
				{
					testCaseManager.RefreshFolderExecutionStatus(projectId, testCaseFolderId);
				}

				//Update the release status of any affected test sets
				TestRun testRun = testRunsPending.TestRuns.FirstOrDefault();
				if (testRun.TestSetId.HasValue)
				{
					TestSetManager testSetManager = new TestSetManager();
					testSetManager.TestSet_RefreshExecutionData(projectId, testRun.TestSetId.Value, testRun.ReleaseId);
				}

				//See which test cases were modified, then modify the requirements test coverage status
				RequirementManager requirementManager = new RequirementManager();
				IEnumerable<int> affectedTestCaseIds = testRunsPending.TestRuns.Select(t => t.TestCaseId).Distinct();
				foreach (int testCaseId in affectedTestCaseIds)
				{
					List<RequirementView> requirements = requirementManager.RetrieveCoveredByTestCaseId(UserManager.UserInternal, null, testCaseId);
					foreach (RequirementView requirement in requirements)
					{
						//We need to use the requirement's project ID in case we have cross-project requirements test coverage
						requirementManager.RefreshTaskProgressAndTestCoverage(requirement.ProjectId, requirement.RequirementId);
					}
				}

				//Update the release status of any affected releases
				//Needs to happen after the requirements, in case any requirement changed from Developed > Tested
				ReleaseManager releaseManager = new ReleaseManager();
				if (testRun != null && testRun.ReleaseId.HasValue)
				{
					releaseManager.RefreshProgressEffortTestStatus(projectId, testRun.ReleaseId.Value, false, true);
				}

				//Fire test case notifications for status changes
				try
				{
					NotificationManager notificationManager = new NotificationManager();

					if (currentTestRun != null && testRunChanges != null && testRunChanges.Count > 0)
					{
						TestCase testCase = new TestCaseManager().RetrieveById2(null, currentTestRun.TestCaseId);
						testCase.StartTracking();

						//We apply the changes to the test case, for any matching properties
						foreach (KeyValuePair<string, object> testRunChange in testRunChanges)
						{
							if (testRunChange.Key == "ExecutionStatusId")
							{
								int executionStatusId = testCase.ExecutionStatusId;
								testCase.ChangeTracker.RecordOriginalValue("ExecutionStatusId", -1);
								testCase.ExecutionStatusId = executionStatusId;
								testCase.MarkAsModified();
							}
							if (testRunChange.Key == "EndDate")
							{
								testCase.ChangeTracker.RecordOriginalValue("ExecutionDate", null);
								testCase.ExecutionDate = DateTime.UtcNow;
								testCase.MarkAsModified();
							}
							if (testRunChange.Key == "ActualDuration")
							{
								testCase.ChangeTracker.RecordOriginalValue("ActualDuration", null);
								testCase.ActualDuration = currentTestRun.ActualDuration;
								testCase.MarkAsModified();
							}
						}
						notificationManager.SendNotificationForArtifact(testCase, null, null);
					}
				}
				catch (Exception exception)
				{
					//Log but don't throw
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				//Log the error but don't throw as that can cause issues during async operations
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>
		/// Updates the test execution status of a particular test case from its linked test runs
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseId">The id of the test case we want to refresh the status of</param>
		/// <param name="affectedReleaseId">The id of any affected release</param>
		/// <param name="affectedTestSetId">The id of any affected test set</param>
		/// <param name="updateSpecificReleaseId">Do we want to update the test state of a specific release, or all releases (null = all)</param>
		/// <remarks>This overload is called when a test run is deleted or updated typically</remarks>
		protected internal void RefreshTestCaseExecutionStatus3(int projectId, int testCaseId, int? updateSpecificReleaseId = null)
		{
			const string METHOD_NAME = "RefreshTestCaseExecutionStatus3";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				//First get the test cases
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCases
								where t.TestCaseId == testCaseId && !t.IsDeleted
								select t;

					TestCase testCase = query.FirstOrDefault();

					if (testCase != null)
					{
						//Update the test cases
						context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
						context.TestRun_RefreshTestCaseExecutionStatus3(projectId, testCase.TestCaseId);

						//Update any affected folders
						if (testCase.TestCaseFolderId.HasValue)
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							testCaseManager.RefreshFolderExecutionStatus(projectId, testCase.TestCaseFolderId.Value);
						}

						//Update the release status of any affected releases
						ReleaseManager releaseManager = new ReleaseManager();
						if (updateSpecificReleaseId.HasValue)
						{
							releaseManager.RefreshProgressEffortTestStatus(projectId, updateSpecificReleaseId.Value);
						}
						else
						{
							List<ReleaseView> releases = releaseManager.RetrieveMappedByTestCaseId(UserManager.UserInternal, projectId, testCaseId);
							List<int> releaseIds = releases.Select(r => r.ReleaseId).ToList();
							releaseManager.RefreshProgressEffortTestStatus(projectId, releaseIds);
						}

						//Update the release status of any affected test sets (all releases)
						TestSetManager testSetManager = new TestSetManager();
						List<TestSetView> testSets = testSetManager.RetrieveByTestCaseId(projectId, testCaseId, null, true, 1, Int32.MaxValue, null, 0);
						foreach (TestSetView testSet in testSets)
						{
							testSetManager.TestSet_RefreshExecutionData(projectId, testSet.TestSetId, updateSpecificReleaseId);
						}

						//See which test cases were modified, then modify the requirements test coverage status
						//We need to get requirements in all projects since v5.1 because of cross-project test coverage
						RequirementManager requirementManager = new RequirementManager();
						List<RequirementView> requirements = requirementManager.RetrieveCoveredByTestCaseId(UserManager.UserInternal, null, testCaseId);
						foreach (RequirementView requirement in requirements)
						{
							//We need to use the requirement's project ID in case we have cross-project requirements test coverage
							requirementManager.RefreshTaskProgressAndTestCoverage(requirement.ProjectId, requirement.RequirementId);
						}
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the test execution status of a particular test case based on a specific run
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseIds">The ids of the test run we want to refresh the status of</param>
		/// <remarks>This overload is called when an automated test run is executed</remarks>
		protected void RefreshTestCaseExecutionStatus2(int projectId, int testRunId)
		{
			const string METHOD_NAME = "RefreshTestCaseExecutionStatus2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				//First get the test run
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRuns.Include(t => t.TestCase)
								where t.TestRunId == testRunId
								select t;

					TestRun testRun = query.FirstOrDefault();

					if (testRun != null)
					{
						//First Update the test case
						context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
						context.TestRun_RefreshTestCaseExecutionStatus2(projectId, testRun.TestRunId);
						if (testRun.TestCase != null && testRun.TestCase.TestCaseFolderId.HasValue)
						{
							TestCaseManager testCaseManager = new TestCaseManager();
							testCaseManager.RefreshFolderExecutionStatus(projectId, testRun.TestCase.TestCaseFolderId.Value);
						}

						//Update the release status of any affected releases
						if (testRun.ReleaseId.HasValue)
						{
							ReleaseManager releaseManager = new ReleaseManager();
							releaseManager.RefreshProgressEffortTestStatus(projectId, testRun.ReleaseId.Value);
						}

						//Update the release status of any affected test sets
						if (testRun.TestSetId.HasValue)
						{
							TestSetManager testSetManager = new TestSetManager();
							testSetManager.TestSet_RefreshExecutionData(projectId, testRun.TestSetId.Value, testRun.ReleaseId);
						}

						//See which test cases were modified, then modify the requirements test coverage status
						//Since v5.1 we have to check all projects' requirements because of cross-project requirements coverage
						RequirementManager requirementManager = new RequirementManager();
						List<RequirementView> requirements = requirementManager.RetrieveCoveredByTestCaseId(UserManager.UserInternal, null, testRun.TestCaseId);
						foreach (RequirementView requirement in requirements)
						{
							requirementManager.RefreshTaskProgressAndTestCoverage(requirement.ProjectId, requirement.RequirementId);
						}
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the status of the overall test run from the individual
		/// test run step status codes
		/// </summary>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="testRunsPending">The test run dataset containing test runs and associated steps</param>
		/// <param name="testRunIndex">The current test run dataset index</param>
		/// <param name="endDate">The effective end-date of the test run (leave null to just use the value in the test run)</param>
		/// <param name="persistChanges">Should we persist the changes in the database</param>
		/// <param name="projectId">The current project</param>
		/// <param name="runAsync">Should we run asynchronously (default = true)</param>
		/// <param name="updateTestCase">Should update test case (default = true)</param>
		/// <remarks>Also updates the counts of the pending test runs record</remarks>
		public int UpdateExecutionStatus(int projectId, int userId, TestRunsPending testRunsPending, int testRunIndex, DateTime? endDate, bool persistChanges, bool runAsync = true, bool updateTestCase = true)
		{
			const string METHOD_NAME = "UpdateExecutionStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Default to "Passed" status
			int executionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;

			//Loop through all the test steps in the current row
			//If we have a failure then switch to failure
			//If we have a caution and no failures, then switch to caution
			//If we have a blocked and no cautions or failures, then switch to blocked
			TestRun currentTestRun = testRunsPending.TestRuns[testRunIndex];
			currentTestRun.StartTracking();
			foreach (TestRunStep testRunStep in currentTestRun.TestRunSteps)
			{
				int stepExecutionStatusId = testRunStep.ExecutionStatusId;
				//Any failure will switch overall status to fail
				if (stepExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Failed)
				{
					executionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
				}
				//If we have a step marked as caution and overall status is pass or not run then mark as caution
				if (stepExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Caution && (executionStatusId == (int)TestCase.ExecutionStatusEnum.Passed || executionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun))
				{
					executionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;
				}
				//If we have a step marked as blocked and overall status is pass, caution or not run then mark as blocked
				if (stepExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked && (executionStatusId == (int)TestCase.ExecutionStatusEnum.Passed || executionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun || executionStatusId == (int)TestCase.ExecutionStatusEnum.Caution))
				{
					executionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
				}
				//If we have a step marked as not run and overall status is still pass then mark as not run
				if (stepExecutionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun && executionStatusId == (int)TestCase.ExecutionStatusEnum.Passed)
				{
					executionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
				}
			}

			//Steps marked as N/A don't affect the status, unless they are all N/A, in which case it is N/A
			if (currentTestRun.TestRunSteps.All(t => t.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.NotApplicable))
			{
				executionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
			}

			//Update the end-date and actual duration if the status is anything but not-run
			if (executionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun)
			{
				currentTestRun.ConcurrencyDate = DateTime.UtcNow;

				//See if we've been provided with an end-date
				if (endDate.HasValue)
				{
					currentTestRun.EndDate = endDate.Value;
				}
				else
				{
					//Simply get the latest date from the steps
					TestRunStep latestTestRunStep = currentTestRun.TestRunSteps.Where(t => t.EndDate.HasValue).OrderByDescending(t => t.EndDate).FirstOrDefault();
					if (latestTestRunStep != null && latestTestRunStep.EndDate.HasValue)
					{
						currentTestRun.EndDate = latestTestRunStep.EndDate.Value;
					}
				}

				//Also if the next test run is 'not run', advance its start date so that the next test run
				//doesn't include the time taken for this run
				if (testRunsPending.TestRuns.Count > testRunIndex + 1 && endDate.HasValue)
				{
					if (testRunsPending.TestRuns[testRunIndex + 1].ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun && !testRunsPending.TestRuns[testRunIndex + 1].EndDate.HasValue)
					{
						testRunsPending.TestRuns[testRunIndex + 1].StartDate = endDate.Value;
					}
				}
			}

			//Regardless of execution status, need to sum up the actual durations for each step and add to the test run
			int? actualDuration = null;
			foreach (TestRunStep testRunStep in currentTestRun.TestRunSteps.Where(t => t.ActualDuration.HasValue))
			{
				if (actualDuration.HasValue)
				{
					actualDuration += testRunStep.ActualDuration;
				}
				else
				{
					actualDuration = testRunStep.ActualDuration;
				}
			}

			//If the test run duration was not provided at the step level
			//which is the case from older API versions, just backup to using the test run value
			if (actualDuration.HasValue)
			{
				currentTestRun.ActualDuration = actualDuration.Value;
			}


			int passcount = 0, notruncount = 0, othercount = 0;
			foreach (TestRunStep testRunStep in currentTestRun.TestRunSteps)
			{
				switch (testRunStep.ExecutionStatusId)
				{
					case (int)TestCase.ExecutionStatusEnum.NotRun:
						notruncount += 1;
						break;
					case (int)TestCase.ExecutionStatusEnum.Blocked:
						notruncount += 1;
						break;
						case (int)TestCase.ExecutionStatusEnum.NotApplicable:
						notruncount += 1;
						break;
					case (int)TestCase.ExecutionStatusEnum.Failed:
						notruncount += 1;
						break;
					case (int)TestCase.ExecutionStatusEnum.Caution:
						notruncount += 1;
						break;
					case (int)TestCase.ExecutionStatusEnum.InProgress:
						passcount += 1;
						break;
					case (int)TestCase.ExecutionStatusEnum.Passed:
						notruncount += 1;
						break;
					default:
						othercount += 1;
						break;
				}
			}
			/**************************************************************************************/
			//Now update the overall test run status
			if (notruncount > 0)
			{
				// More Test Runs to complete. Use existing status.
				currentTestRun.ExecutionStatusId = executionStatusId;
			}
			else
			{
				// Test Runs complete. Change status to complete.
				currentTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.InProgress;
			}

			//Before saving, get the notification changes
			Dictionary<string, object> testRunChanges = currentTestRun.ExtractChanges();
			/**************************************************************************************/


			/**************************************************************************/

			//Update the pending test run summary record itself
			testRunsPending.StartTracking();

			//Iterate through all the test runs to get the counts
			testRunsPending.CountBlocked = 0;
			testRunsPending.CountCaution = 0;
			testRunsPending.CountFailed = 0;
			testRunsPending.CountNotRun = 0;
			testRunsPending.CountPassed = 0;
			testRunsPending.CountNotApplicable = 0;
			foreach (TestRun testRunRow in testRunsPending.TestRuns)
			{
				//Store the current project id for use by the history manager
				testRunRow.ProjectId = projectId;
				switch (testRunRow.ExecutionStatusId)
				{
					case (int)TestCase.ExecutionStatusEnum.Blocked:
						testRunsPending.CountBlocked++;
						break;

					case (int)TestCase.ExecutionStatusEnum.Caution:
						testRunsPending.CountCaution++;
						break;

					case (int)TestCase.ExecutionStatusEnum.Failed:
						testRunsPending.CountFailed++;
						break;

					case (int)TestCase.ExecutionStatusEnum.NotRun:
						testRunsPending.CountNotRun++;
						break;

					case (int)TestCase.ExecutionStatusEnum.NotApplicable:
						testRunsPending.CountNotApplicable++;
						break;

					case (int)TestCase.ExecutionStatusEnum.Passed:
						testRunsPending.CountPassed++;
						break;
				}
			}
			//Update the date/time it was last updated
			testRunsPending.LastUpdateDate = DateTime.UtcNow;



			//Persist the changes if necessary
			if (persistChanges)
			{
				//Attach to the pending run and save changes
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach the disconnected data
					context.TestRunsPendings.ApplyChanges(testRunsPending);
					try
					{
						//Save the changes, recording any history changes, and sending any notifications
						context.SaveChanges(userId, true, true, null);
					}
					catch (Exception exception)
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME + "~Save-TestRunsPendings", exception);
						Logger.Flush();
					}
					if (updateTestCase)
					{
						//Update the status of the linked test cases, requirements and releases so that they
						//reflect the status of the test run changes
						RefreshTestCaseExecutionStatus(projectId, testRunsPending, runAsync, currentTestRun, testRunChanges);
					}
				}
			}

			//retrieve the newly updated testRunsPending - if it has a real ID. It may not (if we are not persisting changes in the database)
			if (testRunsPending.TestRunsPendingId > 0)
			{
				testRunsPending = RetrievePendingById(testRunsPending.TestRunsPendingId, true);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			//return the new execution status
			return testRunsPending.TestRuns[0].ExecutionStatusId;
		}

		/// <summary>
		/// Updates the status of the overall test run from the individual
		/// test run step status codes
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="testRunsPending">The test run dataset containing test runs and associated steps</param>
		/// <remarks>Also updates the counts of the pending test runs record</remarks>
		public void UpdateExploratoryTestRun(int projectId, int userId, TestRunsPending testRunsPending)
		{
			const string METHOD_NAME = "UpdateExploratoryStepFields";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Persist the changes
			//Attach to the pending run and save changes
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//Attach the disconnected data
				context.TestRunsPendings.ApplyChanges(testRunsPending);

				//Save the changes, and don't bother recording any history changes or sending any notifications
				context.SaveChanges(userId, false, false, null);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Updates the positions of all test run steps in a run
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <param name="testRunStepPositions">A list of objects containing test run step ids and their position</param>
		public void UpdateExploratoryTestRunStepPositions(int testRunId, List<TestRunStepPosition> testRunStepPositions)
		{
			const string METHOD_NAME = "UpdateExploratoryTestRunStepPositions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Persist the changes
				//Attach to the relevant pending test run
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Get the list of test run steps
						var query = from t in context.TestRunSteps
									where t.TestRunId == testRunId
									orderby t.Position, t.TestRunStepId
									select t;

						List<TestRunStep> testRunSteps = query.ToList();

						//Make sure all returned positions are in the valid range (greater than zero and less than the number of steps in the test run
						if (testRunStepPositions.Any(step => step.Position < 1 || step.Position > testRunSteps.Count))
						{
							Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "Not all position values are within accepted range");
						}

						//we need match each test run step to the step/position list sent in to update the position data
						foreach (TestRunStep step in testRunSteps)
						{
							foreach (TestRunStepPosition stepPosition in testRunStepPositions)
							{
								if (step.TestRunStepId == stepPosition.TestRunStepId)
								{
									step.Position = stepPosition.Position;
								}
							}
						}

						//Save the changes, and don't bother recording any history changes or sending any notifications
						context.SaveChanges();
					}

					//Commit transaction - needed to maintain integrity of positions and avoiding the chance of having duplicates in the same test run
					transactionScope.Complete();
				}



			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Deletes a test run step from a test run
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <param name="testRunStepId">A list of objects containing test run step ids and their position</param>
		public int DeleteExploratoryTestRunStep(int testRunId, int testRunStepId)
		{
			const string METHOD_NAME = "DeleteExploratoryTestRunStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Persist the changes
				//Attach to the relevant pending test run
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Get the test run step marked for deletion
						var query = from t in context.TestRunSteps
									where t.TestRunStepId == testRunStepId
									select t;

						List<TestRunStep> testRunSteps = query.ToList();
						context.TestRunSteps.DeleteObject(testRunSteps[0]);

						//Save the changes, and don't bother recording any history changes or sending any notifications
						context.SaveChanges();
					}

					//Commit transaction - needed to maintain integrity of positions and avoiding the chance of having duplicates in the same test run
					transactionScope.Complete();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

					return testRunStepId;
				}



			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>
		/// Creates a new test run step and links it to the relevant test run
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <param name="testCaseId">The id of the test case the run is associated with</param>
		public int CreateNewExploratoryTestRunStep(int testRunId, int testCaseId)
		{
			const string METHOD_NAME = "CreateNewExploratoryTestRunStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (TransactionScope transactionScope = new TransactionScope())
				{
					//create a new test run step object that can be accessed at the end of the using - to return
					TestRunStep testRunStep = new TestRunStep();
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{

						//get the value for the new position - putting this new step at the end of the list
						int position = context.TestRunSteps.Where(step => step.TestRunId == testRunId).Count() + 1;

						//assign the test run id and position
						testRunStep.TestRunId = testRunId;
						testRunStep.TestCaseId = testCaseId;
						testRunStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
						testRunStep.Position = position;
						testRunStep.Description = GlobalResources.General.Description;

						//add the step to the database
						context.TestRunSteps.AddObject(testRunStep);

						//Save the changes, and don't bother recording any history changes or sending any notifications
						context.SaveChanges();
					}

					//Commit transaction - needed to maintain integrity of positions and avoiding the chance of having duplicates in the same test run
					transactionScope.Complete();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

					return testRunStep.TestRunStepId;
				}



			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>
		/// Creates a new test run step and links it to the relevant test run
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <param name="testCaseId">The id of the test case the run is associated with</param>
		/// <param name="description">The description of the relevant test run step to be cloned</param>
		/// <param name="expectedResult">The expectedResult of the relevant test run step to be cloned</param>
		/// <param name="sampleData">The sampleData of the relevant test run step to be cloned</param>
		public int CloneExploratoryTestRunStep(int testRunId, int testCaseId, string description, string expectedResult, string sampleData)
		{
			const string METHOD_NAME = "CloneExploratoryTestRunStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (TransactionScope transactionScope = new TransactionScope())
				{
					//create a new test run step object that can be accessed at the end of the using - to return
					TestRunStep testRunStep = new TestRunStep();
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{

						//get the value for the new position - putting this new step at the end of the list
						int position = context.TestRunSteps.Where(step => step.TestRunId == testRunId).Count() + 1;

						//assign the test run id and position
						testRunStep.TestRunId = testRunId;
						testRunStep.TestCaseId = testCaseId;
						testRunStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
						testRunStep.Position = position;
						testRunStep.Description = description;
						testRunStep.ExpectedResult = expectedResult;
						testRunStep.SampleData = sampleData;

						//add the step to the database
						context.TestRunSteps.AddObject(testRunStep);

						//Save the changes, and don't bother recording any history changes or sending any notifications
						context.SaveChanges();
					}

					//Commit transaction - needed to maintain integrity of positions and avoiding the chance of having duplicates in the same test run
					transactionScope.Complete();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

					return testRunStep.TestRunStepId;
				}



			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>
		/// Retrieves all the saved pending test runs for a particular user
		/// </summary>
		/// <param name="numberRows">The number of rows to return</param>
		/// <param name="userId">The user in question</param>
		/// <param name="projectId">The id of the project, or null for all</param>
		/// <returns>An dataset of pending test runs</returns>
		public List<TestRunsPendingView> RetrievePendingByUserId(int userId, int? projectId, int numberRows = 500)
		{
			const string METHOD_NAME = "RetrievePendingByUserId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunsPendingView> pendingRuns;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRunsPendingsView
								where t.TesterId == userId
								select t;

					//Add the project filter
					if (projectId.HasValue)
					{
						query = query.Where(t => t.ProjectId == projectId.Value);
					}

					//Sort by date
					query = query.OrderByDescending(t => t.CreationDate).ThenBy(t => t.TestRunsPendingId);

					//Execute
					pendingRuns = query.Take(numberRows).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return pendingRuns;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all pending test runs for a particular user and a particular test case
		/// </summary>
		/// <param name="userId">The user in question</param>
		/// <param name="projectId">The test case in questoin</param>
		/// <returns>A dataset of pending test runs</returns>
		public List<TestRunsPendingView> RetrievePendingByUserIdAndTestCase(int userId, int testCaseId)
		{
			const string METHOD_NAME = "RetrievePendingByUserIdAndTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunsPendingView> pendingRuns = new List<TestRunsPendingView>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve test runs with the matching test case id and user, that are still pending
					var query = context.TestRuns
							   .Where(t => t.TesterId == userId && t.TestCaseId == testCaseId && t.TestRunsPendingId != null)
							   .Select(t => t.TestRunsPendingId);

					//Execute the query and return just the ids
					List<int?> queryList = query.ToList();

					//If we got any matches carry out the second query
					if (queryList.Count > 0)
					{
						//Retrieve test runs pending that match the ids returned from query 1, then order then by date
						var query2 = context.TestRunsPendingsView
							.Where(t => queryList.Contains(t.TestRunsPendingId))
							.OrderByDescending(t => t.CreationDate).ThenBy(t => t.TestRunsPendingId);

						//Execute
						pendingRuns = query2.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return pendingRuns;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the pending test runs set by its id
		/// </summary>
		/// <param name="testRunsPendingId">The id of the pending test run</param>
		/// <param name="retrieveTestRuns">Retrieve the actual test run records as well</param>
		/// <returns>The pending test run record</returns>
		public TestRunsPending RetrievePendingById(int testRunsPendingId, bool retrieveTestRuns)
		{
			const string METHOD_NAME = "RetrievePendingById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestRunsPending testRunsPending;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the pending test runs
					var query = from t in context.TestRunsPendings
									.Include(t => t.TestSet)
									.Include(t => t.Project)
								where t.TestRunsPendingId == testRunsPendingId
								select t;

					//Get the data
					testRunsPending = query.FirstOrDefault();

					//If we don't have a record, throw a specific exception (since client will be expecting one record)
					if (testRunsPending == null)
					{
						throw new ArtifactNotExistsException("Pending Test Run " + testRunsPendingId + " doesn't exist in the system.");
					}

					//Now see if we need to retrieve the actual test runs with their steps
					//The data is joined by 'fix-up'
					if (retrieveTestRuns)
					{
						//Create select command for retrieving test runs
						var query2 = from t in context.TestRuns
										.Include(t => t.Release)
									 where t.TestRunsPendingId == testRunsPendingId
									 orderby t.TestRunId
									 select t;

						//Execute the query
						query2.ToList();

						//Create select command for retrieving test run steps
						var query3 = from s in context.TestRunSteps
									 where s.TestRun.TestRunsPendingId == testRunsPendingId
									 orderby s.Position, s.TestRunStepId
									 select s;

						//Execute the query
						query3.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunsPending;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Handles the persisting of a list of test runs linked to a test set
		/// </summary>
		/// <param name="testRunDataSet">The test run dataset</param>
		/// <param name="testSetDataSet">The test set dataset</param>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The user running the test</param>
		/// <param name="testSetId">The id of the test set</param>
		/// <param name="testSetName">The name of the test set</param>
		/// <param name="releaseId">The id of the release (optional)</param>
		protected void SavePendingForTestSet(TestSet testSet, TestRunsPending testRunsPending, int projectId, int userId, int testSetId, string testSetName, int? releaseId)
		{
			const string METHOD_NAME = "SavePendingForTestSet";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Use the name of the test set as the name of the pending set of test runs
				PopulatePending(projectId, userId, testRunsPending, testSetName, testSetId);
				Save(testRunsPending, projectId);

				//If the test set had some list custom properties set then we should look for corresponding
				//lists in the test run and if there are matching lists we should set the values to be the same
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				ArtifactCustomProperty testSetCustomProperties = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

				//Make sure we have some custom properties on the test set
				if (testSetCustomProperties != null)
				{
					//Iterate through the list of test runs and set the appropriate custom properties (if any)
					List<CustomProperty> testRunCustomPropertyDefinitions = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);
					foreach (TestRun testRun in testRunsPending.TestRuns)
					{
						ArtifactCustomProperty testRunCustomProperties = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRun.TestRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, false, testRunCustomPropertyDefinitions);
						//Add a new custom property row if necessary
						if (testRunCustomProperties == null)
						{
							testRunCustomProperties = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestRun, testRun.TestRunId, testRunCustomPropertyDefinitions);
							testRunCustomProperties = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, testRunCustomProperties);
						}
						else
						{
							testRunCustomProperties.StartTracking();
						}

						foreach (CustomProperty testSetCustomPropertyDefinition in testSetCustomProperties.CustomPropertyDefinitions)
						{
							//See if we have a matching property in the test run custom properties (only works for lists)
							if (testSetCustomPropertyDefinition.CustomPropertyListId.HasValue)
							{
								foreach (CustomProperty testRunCustomPropertyDefinition in testRunCustomProperties.CustomPropertyDefinitions)
								{
									if (testRunCustomPropertyDefinition.CustomPropertyListId.HasValue &&
										testRunCustomPropertyDefinition.CustomPropertyListId.Value == testSetCustomPropertyDefinition.CustomPropertyListId.Value &&
										testRunCustomPropertyDefinition.Name.ToLowerInvariant() == testSetCustomPropertyDefinition.Name.ToLowerInvariant()
										)
									{
										//We have a matching custom list between the test set and test run
										//So set the value on the matching test run property
										//We also check the name of the property (in case the list used multiple times) using a case invariant check
										object customPropertyValue = testSetCustomProperties.CustomProperty(testSetCustomPropertyDefinition.PropertyNumber);
										testRunCustomProperties.SetCustomProperty(testRunCustomPropertyDefinition.PropertyNumber, customPropertyValue);
									}
								}
							}
						}

						//Persist the changes
						customPropertyManager.ArtifactCustomProperty_Save(testRunCustomProperties, userId);
					}
				}

				//Also need to check to see if any test cases need to get added to the selected release
				if (releaseId.HasValue)
				{
					List<int> testCaseIds = new List<int>();
					TestCaseManager testCaseManager = new TestCaseManager();
					List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId.Value);
					foreach (TestSetTestCase testSetTestCase in testSet.TestSetTestCases)
					{
						//If the test case is not already mapped, add it to the list
						if (!mappedTestCases.Any(t => t.TestCaseId == testSetTestCase.TestCaseId))
						{
							testCaseIds.Add(testSetTestCase.TestCaseId);
						}
					}
					testCaseManager.AddToRelease(projectId, releaseId.Value, testCaseIds, userId);

					//If we have an iteration/phase (rather than a true release), also add the test cases in question to the parent release
					try
					{
						ReleaseManager releaseManager = new ReleaseManager();
						ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId.Value);
						if ((releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase) && releaseOrIteration.IndentLevel.Length > 3)
						{
							//Get the id of the parent release
							string releaseIndentLevel = releaseOrIteration.IndentLevel.SafeSubstring(0, releaseOrIteration.IndentLevel.Length - 3);
							ReleaseView parentRelease = releaseManager.RetrieveByIndentLevel(User.UserInternal, projectId, releaseIndentLevel).FirstOrDefault();

							//If the test case is not already mapped, add it to the release
							if (parentRelease != null)
							{
								mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, parentRelease.ReleaseId);
								foreach (TestSetTestCase testSetTestCase in testSet.TestSetTestCases)
								{
									if (!mappedTestCases.Any(t => t.TestCaseId == testSetTestCase.TestCaseId))
									{
										testCaseIds.Add(testSetTestCase.TestCaseId);
									}
								}
								testCaseManager.AddToRelease(projectId, parentRelease.ReleaseId, testCaseIds, userId);
							}
						}
					}
					catch (ArtifactNotExistsException exception)
					{
						//Ignore and just don't add the test cases to the iteration
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
					}
				}

				//Automatically mark the test set as 'In Progress'
				TestSetManager testSetManager = new TestSetManager();
				TestSet testSet2 = testSetManager.RetrieveById2(projectId, testSetId);
				//Mark as in-progress as long as the test set is owned by the current user doing the testing
				if (testSet2.OwnerId.HasValue)
				{
					if (testSet2.OwnerId.Value == userId)
					{
						testSet2.StartTracking();
						testSet2.TestSetStatusId = (int)TestSet.TestSetStatusEnum.InProgress;
						testSet2.LastUpdateDate = DateTime.UtcNow;

						//Persist the change
						testSetManager.Update(testSet2, userId);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Saves the passed in test run pending set (with associated test runs and test run steps)
		/// </summary>
		/// <param name="testRunsPending">A test run pending set with test runs and test run steps</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="runAsync">Should we run the test case updates as a background process</param>
		/// <remarks>
		/// 1)	This method should only be used for Manual test runs. Use the Record method
		///		for executing automated runs.
		/// 2)	This can only be used for the initial Insert, to make subsequent changes, use
		///     the Update() method
		/// </remarks>
		public void Save(TestRunsPending testRunsPending, int projectId, bool runAsync = true)
		{
			const string METHOD_NAME = "Save";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach the pending set and save all changes
					context.TestRunsPendings.AddObject(testRunsPending);
					context.SaveChanges();
				}

				//Update the status of the linked test cases, requirements and releases so that they
				//reflect the status of the test run changes
				RefreshTestCaseExecutionStatus(projectId, testRunsPending, runAsync);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//throw;
			}
		}

		/// <summary>
		/// Updates an exploratory test case from its run, either just the status or the actual test case and steps
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testRunId">The id of the test run</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="updateStatusOnly">Should we update just the status, or the test case and steps as well</param>
		public void UpdateExploratoryTestCaseFromRun(int projectId, int testRunId, int userId, bool updateStatusOnly = true)
		{
			const string METHOD_NAME = "UpdateExploratoryTestCaseFromRun";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to finalize the end date of the run
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRuns.Include(t => t.TestRunSteps)
								where
									t.TestRunId == testRunId &&
									t.TestCase.ProjectId == projectId &&
									t.TestCase.Type.IsExploratory &&
									!t.TestCase.IsDeleted
								select t;

					TestRun testRun = query.FirstOrDefault();
					if (testRun == null)
					{
						throw new ArtifactNotExistsException("Test Run " + testRunId + " doesn't exist in the system.");
					}

					testRun.StartTracking();
					//Update the end-date and actual duration if the status is anything but not-run
					if (testRun.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun)
					{
						testRun.ConcurrencyDate = DateTime.UtcNow;

						//Try and get the latest date from the steps
						TestRunStep latestTestRunStep = testRun.TestRunSteps.Where(t => t.EndDate.HasValue).OrderByDescending(t => t.EndDate).FirstOrDefault();
						if (latestTestRunStep != null && latestTestRunStep.EndDate.HasValue)
						{
							testRun.EndDate = latestTestRunStep.EndDate.Value;
						}

						//Otherwise just use the current date
						if (!testRun.EndDate.HasValue)
						{
							testRun.EndDate = DateTime.UtcNow;
						}
					}

					context.SaveChanges(userId, false, false, null);

					//Change the test case, unless just updating the status
					if (!updateStatusOnly)
					{
						//Maintain a mapping between test case step and test run steps
						Dictionary<int, int> testStepMapping = new Dictionary<int, int>();

						//We need to first retrieve the test case in question
						var query2 = from t in context.TestCases.Include(t => t.TestSteps)
									 where
										 t.TestCaseId == testRun.TestCaseId &&
										 t.ProjectId == projectId &&
										 t.Type.IsExploratory &&
										 !t.IsDeleted
									 select t;

						TestCase testCase = query2.FirstOrDefault();

						//First update the test case itself
						testCase.StartTracking();
						testCase.Name = testRun.Name;
						testCase.Description = testRun.Description;
						testCase.LastUpdateDate = DateTime.UtcNow;
						testCase.ConcurrencyDate = DateTime.UtcNow;

						//Now we update the test steps
						for (int i = 0; i < testRun.TestRunSteps.Count; i++)
						{
							TestRunStep testRunStep = testRun.TestRunSteps[i];

							//See if we have a matching test step (it also must not be a linked step)
							if (testRunStep.TestStepId.HasValue)
							{
								TestStep testStep = testCase.TestSteps.FirstOrDefault(t => t.TestStepId == testRunStep.TestStepId.Value);
								if (testStep != null && !testStep.LinkedTestCaseId.HasValue)
								{
									testStep.StartTracking();
									testStep.Description = testRunStep.Description;
									testStep.ExpectedResult = testRunStep.ExpectedResult;
									testStep.SampleData = testRunStep.SampleData;
									testStep.LastUpdateDate = DateTime.UtcNow;
									testStep.ConcurrencyDate = DateTime.UtcNow;
									if (!testStepMapping.ContainsKey(testRunStep.TestRunStepId))
									{
										testStepMapping.Add(testRunStep.TestRunStepId, testStep.TestStepId);
									}

									//We give it a temporary (larger) position (to avoid constraint issues later)
									testStep.Position = testRunStep.Position + 10000;
								}
							}
						}

						//Save the changes and ensure change tracker up to date
						context.DetectChanges();
						try
						{
							context.SaveChanges(userId, true, true, null);
						}
						catch (Exception exception)
						{
							Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME + "~Save-ExploratoryTracker", exception);
							Logger.Flush();
						}
						//Now we need to add new steps (we add them at the end and do the repositioning later
						//Need to any test run steps that came from expanded test links
						TestCaseManager testCaseManager = new TestCaseManager();
						for (int i = 0; i < testRun.TestRunSteps.Count; i++)
						{
							TestRunStep testRunStep = testRun.TestRunSteps[i];
							if (!testRunStep.TestStepId.HasValue || (!testCase.TestSteps.Any(t => t.TestStepId == testRunStep.TestStepId.Value) && testRunStep.TestCaseId == testCase.TestCaseId))
							{
								//Add the step
								int testStepId = testCaseManager.InsertStep(userId, testCase.TestCaseId, null, testRunStep.Description, testRunStep.ExpectedResult, testRunStep.SampleData, true, testRunStep.ExecutionStatusId);
								if (!testStepMapping.ContainsKey(testRunStep.TestRunStepId))
								{
									testStepMapping.Add(testRunStep.TestRunStepId, testStepId);
								}
							}
						}

						//Now we need to delete removed steps, don't delete linked test steps
						for (int i = 0; i < testCase.TestSteps.Count; i++)
						{
							TestStep testStep = testCase.TestSteps[i];
							if (!testStep.IsDeleted && !testStep.LinkedTestCaseId.HasValue && !testRun.TestRunSteps.Any(t => t.TestStepId == testStep.TestStepId))
							{
								testCaseManager.MarkStepAsDeleted(userId, testCase.TestCaseId, testStep.TestStepId);
							}
						}

						//Finally we need to reload the test case and update all the positions in one go
						testCase = query2.FirstOrDefault();

						for (int i = testRun.TestRunSteps.Count - 1; i >= 0; i--)
						{
							TestRunStep testRunStep = testRun.TestRunSteps[i];
							//Get the matching test step
							if (testStepMapping.ContainsKey(testRunStep.TestRunStepId))
							{
								int testStepId = testStepMapping[testRunStep.TestRunStepId];
								TestStep testStep = testCase.TestSteps.FirstOrDefault(t => t.TestStepId == testStepId);
								if (testStep != null)
								{
									testStep.StartTracking();
									testStep.Position = testRunStep.Position;
									testStep.ConcurrencyDate = DateTime.UtcNow;
								}
							}
						}

						//Make sure we have no duplicates, if so, move them to the end
						List<int> positions = new List<int>();
						int maxPosition = testCase.TestSteps.OrderByDescending(t => t.Position).First().Position;
						foreach (TestStep testStep in testCase.TestSteps)
						{
							if (!positions.Contains(testStep.Position))
							{
								positions.Add(testStep.Position);
							}
							else
							{
								testStep.StartTracking();
								testStep.Position = maxPosition + 1;
								maxPosition++;
								positions.Add(testStep.Position);
							}
						}

						try
						{
							//Save the changes
							context.SaveChanges();
						}
						catch (Exception exception)
						{
							Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME + "~Save-ExploratoryTCfromRun", exception);
							Logger.Flush();
						}
					}
				}

				//If we're just updating the test case status, do that
				this.RefreshTestCaseExecutionStatus2(projectId, testRunId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a test run (containing test run steps) in the system
		/// </summary>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testRun">The test run entity to be updated</param>
		/// <remarks>Used for making changes after the initial save</remarks>
		public void Update(int projectId, TestRun testRun, int userId)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (testRun == null)
			{
				return;
			}

			//Store the project ID on the object (used by history tracking)
			testRun.ProjectId = projectId;

			try
			{
				//See if any fields changed that require us to roll-up the status
				bool updateExecutionStatus = false;
				if (testRun.ChangeTracker.OriginalValues.ContainsKey("ExecutionStatusId") ||
					testRun.ChangeTracker.OriginalValues.ContainsKey("TestSetId") ||
					testRun.ChangeTracker.OriginalValues.ContainsKey("ReleaseId"))
				{
					updateExecutionStatus = true;
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Start tracking changes
					testRun.StartTracking();

					//Update the concurrency date
					testRun.ConcurrencyDate = DateTime.UtcNow;

					//Now apply the changes (will auto-insert any provided test run steps)
					context.TestRuns.ApplyChanges(testRun);

					try
					{
						//Save the changes, recording any history changes, and sending any notifications
						context.SaveChanges(userId, true, true, null);
					}
					catch (Exception exception)
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME + "~Save-Update", exception);
						Logger.Flush();
					}
				}

				//Finally we need to refresh the execution status of the underlying test case from all its test runs
				//if the execution status, test set or release changed
				if (updateExecutionStatus)
				{
					RefreshTestCaseExecutionStatus3(projectId, testRun.TestCaseId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a pending test run (containing test runs and test run steps) in the system
		/// </summary>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testRunsPending">The test run entity to be updated</param>
		/// <remarks>Used for making changes after the initial save</remarks>
		/// <param name="releaseToRefreshId">If execution statuses need updating, is there a specific release we need to update</param>
		public void Update(TestRunsPending testRunsPending, int userId, int? releaseToRefreshId = null)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (testRunsPending == null)
			{
				return;
			}

			try
			{
				//See if any fields changed that require us to roll-up the status
				List<int> testCasesNeedingRefresh = new List<int>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach to the context
					context.TestRunsPendings.ApplyChanges(testRunsPending);

					//Start tracking changes
					testRunsPending.StartTracking();

					//Update the concurrency date
					foreach (TestRun testRun in testRunsPending.TestRuns)
					{
						//See if any fields changed that require us to roll-up the status
						if (testRun.ChangeTracker.OriginalValues.ContainsKey("ExecutionStatusId") ||
							testRun.ChangeTracker.OriginalValues.ContainsKey("TestSetId") ||
							testRun.ChangeTracker.OriginalValues.ContainsKey("ReleaseId"))
						{
							//If the current execution status is not run, and did not change, don't
							//update the status, even if release/test set changed (since nothing effectively changed)
							if ((testRun.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun ||
								testRun.ChangeTracker.OriginalValues.ContainsKey("ExecutionStatusId")) &&
								!testCasesNeedingRefresh.Contains(testRun.TestCaseId))
							{
								testCasesNeedingRefresh.Add(testRun.TestCaseId);
							}
						}

						testRun.ConcurrencyDate = DateTime.UtcNow;

						//Store the project ID on the test run object (used by history tracking)
						testRun.ProjectId = testRunsPending.ProjectId;
					}

					try
					{
						//Save the changes, recording any history changes, and sending any notifications
						context.SaveChanges(userId, true, true, null);
					}
					catch (Exception exception)
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME + "~Save-Update", exception);
						Logger.Flush();
					}
				}

				//Finally we need to refresh the execution status of the underlying test case from all its test runs
				//if the execution status, test set or release changed
				foreach (int testCaseId in testCasesNeedingRefresh)
				{
					RefreshTestCaseExecutionStatus3(testRunsPending.ProjectId, testCaseId, releaseToRefreshId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		///	Retrieves a list of all test runs in the system (for a project)
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <returns>Test Run dataset</returns>
		/// <remarks>
		/// 1) Typically used to retrieve for a test case, test set or release by passing the appropriate filters
		/// 2) When filtering by a release, includes runs for the child iterations
		/// </remarks>
		public List<TestRunView> Retrieve(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunView> testRuns;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TestRunsView
								where t.ProjectId == projectId && !t.IsDeleted && t.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun
								select t;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by execution date descending
						query = query.OrderByDescending(t => t.EndDate).ThenBy(t => t.TestRunId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "TestRunId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TestRunView, bool>> filterClause = CreateFilterExpression<TestRunView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, filters, utcOffset, null, HandleTestRunSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TestRunView>)query.Where(filterClause);
						}
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1)
					{
						startRow = 1;
					}
					if (startRow > artifactCount)
					{
						return new List<TestRunView>();
					}

					//Execute the query
					testRuns = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRuns;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		public List<TestRunView> RetrieveAllTaskRun(int Projectid)
		{
			const string METHOD_NAME = "RetrieveAllTaskRun";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunView> testRun=new List<TestRunView>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Actually execute the query and get the entity
					var query = from t in context.TestRunsView
								where t.ProjectId == Projectid
								&& t.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun
								&& !t.IsDeleted
								select t;

					testRun = query.ToList();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (testRun == null)
				{
					throw new ArtifactNotExistsException("Test Run " + Projectid + " doesn't exist in the system.");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRun;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Records a new test run based on passed in information
		/// In addition to saving the test-run, depending on how the start/end dates
		/// compare against the underlying test case, it may also update the overall
		/// test case status codes
		/// </summary>
		/// <param name="projectId">The project the test case belongs to</param>
		/// <param name="testerUserId">The user id of the person who's running the test</param>
		/// <param name="testCaseId">The test case being executed</param>
		/// <param name="releaseId">The release being executed against</param>
		/// <param name="executionStatusId">The status of the test run (pass/fail/not run)</param>
		/// <param name="runnerName">The name of the automated testing tool</param>
		/// <param name="runnerAssertCount">The number of assertions</param>
		/// <param name="runnerMessage">The failure message (if appropriate)</param>
		/// <param name="runnerStackTrace">The error stack trace (if any)s</param>
		/// <param name="runnerTestName">The name of the test within the runner</param>
		/// <param name="endDate">When the test run ended</param>
		/// <param name="startDate">When the test run started</param>
		/// <param name="testSetId">The id of the test set to record the run against (optional)</param>
		/// <param name="testSetTestCaseId">The unique id of the test case in the test set (optional)</param>
		/// <param name="automationEngineId">The id of the automation engine being used</param>
		/// <param name="automationHostId">The id of the automation host it was executed on</param>
		/// <param name="updateProjectTestStatus">Should we update the summary test status of the associated requirements, releases and test cases</param>
		/// <param name="buildId">The id of the build the test is being executed on</param>
		/// <param name="testRunFormat">The format of the test run (for automated test runs only)</param>
		/// <param name="testRunSteps">Any automated test run steps</param>
		/// <param name="updateProjectTestStatus">By default, update the project test status on saving the test run. Set to false if adding in bulk to speed things up</param>
		/// <param name="triggerNotificationEvents">Defaults to false - so that no notifications are sent</param>
		/// <param name="testSetOverridesRelease">Defaults to false - if you specify a test set id and want the current release of that test set to override the release passed in then set this param to true</param>
		/// <returns>The newly created test run id</returns>
		/// <remarks>
		/// This method should not normally be used - its use is limited to cases where you want to CLONE test runs (eg when cloning a product)
		/// </remarks>
		public int TestRun_Insert(int projectId, int testerUserId, int testCaseId, int? releaseId, int? testSetId, int? testSetTestCaseId, DateTime startDate, DateTime endDate, int executionStatusId, TestRun.TestRunTypeEnum testRunTypeId, string runnerName, string runnerTestName, int? runnerAssertCount, string runnerMessage, string runnerStackTrace, int? automationHostId, int? automationEngineId, int? buildId, TestRun.TestRunFormatEnum? testRunFormat, List<TestRunStep> testRunSteps, bool updateProjectTestStatus = true, bool triggerNotificationEvents = false, bool testSetOverridesRelease = false)
		{
			const string METHOD_NAME = "TestRun_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create a test case business object and instantiate the passed in test case
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
				int testRunId;

				//If we are passed a test set, get that and override the release if necessary
				if (testSetId.HasValue)
				{
					TestSetManager testSetManager = new TestSetManager();
					TestSetView testSetView = testSetManager.RetrieveById(projectId, testSetId.Value);
					if (testSetOverridesRelease && testSetView.ReleaseId.HasValue)
					{
						releaseId = testSetView.ReleaseId.Value;
					}

					//If the test set is populated, but not the test set test case id, we need to get the first matching test case in the set
					//This allows the older v1.x and v2.2 APIs to still use this method
					if (!testSetTestCaseId.HasValue)
					{
						List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId.Value);
						foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
						{
							if (testSetTestCase.TestCaseId == testCaseId)
							{
								testSetTestCaseId = testSetTestCase.TestSetTestCaseId;
								break;
							}
						}
					}
					if (!testSetTestCaseId.HasValue)
					{
						//If no match found, unset the test set id as well
						testSetId = null;
					}
				}

				TestRun testRun;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create a new test run
					testRun = new TestRun();

					//Calculate the actual duration (in whole minutes)
					int actualDuration = (int)endDate.Subtract(startDate).TotalMinutes;

					//Set the format value
					int? testRunFormatValue = null;
					if (testRunFormat != null)
					{
						testRunFormatValue = (int)testRunFormat;
					}

					//Populate the automated test run
					testRun.ProjectId = projectId;
					testRun.Name = testCase.Name;
					testRun.Description = testCase.Description;
					testRun.TestCaseId = testCase.TestCaseId;
					testRun.TestRunTypeId = (int)testRunTypeId;
					testRun.TesterId = testerUserId;
					testRun.ExecutionStatusId = executionStatusId;
					testRun.ReleaseId = releaseId;
					testRun.TestSetId = testSetId;
					testRun.TestSetTestCaseId = testSetTestCaseId;
					testRun.AutomationHostId = automationHostId;
					testRun.AutomationEngineId = automationEngineId;
					testRun.BuildId = buildId;
					testRun.TestRunFormatId = testRunFormatValue;
					testRun.EstimatedDuration = testCase.EstimatedDuration;
					testRun.ActualDuration = actualDuration;
					testRun.StartDate = startDate;
					testRun.EndDate = endDate;
					testRun.RunnerName = runnerName;
					testRun.RunnerTestName = runnerTestName;
					testRun.RunnerAssertCount = runnerAssertCount;
					testRun.RunnerMessage = runnerMessage;
					testRun.RunnerStackTrace = runnerStackTrace;
					testRun.IsAttachments = false;
					testRun.ConcurrencyDate = DateTime.UtcNow;

					//Get the latest ChangeSet ID for this project.
					testRun.ChangeSetId = context.HistoryChangeSets
						.Where(f => f.ProjectId == testCase.ProjectId)
						.OrderByDescending(f => f.ChangeSetId)
						.Select(f => (long?)f.ChangeSetId) //We cast this to a nullable long, because otherwise 'default' is "0".
						.FirstOrDefault();
					//That may look clunky instead of something easier like the Max() LINQ, but the MAX() would cause it to 
					//  loop through everything in-memory, instead of handling it on the SQL Server side.

					//If we have any test steps we need to persist them
					if (testRunSteps != null && testRunSteps.Count > 0)
					{
						foreach (TestRunStep testRunStep in testRunSteps)
						{
							//Make sure that the test step id is present
							int? testStepId = null;
							if (testRunStep.TestStepId.HasValue)
							{
								testStepId = testRunStep.TestStepId.Value;
							}

							//Record the step and populate the test run step id
							//Add the new row to the existing dataset
							TestRunStep testRunStepRow = new TestRunStep();
							testRunStepRow.TestStepId = testStepId;
							testRunStepRow.ExecutionStatusId = testRunStep.ExecutionStatusId;
							testRunStepRow.Description = testRunStep.Description;
							testRunStepRow.Position = testRunStep.Position;
							testRunStepRow.ExpectedResult = testRunStep.ExpectedResult;
							testRunStepRow.SampleData = testRunStep.SampleData;
							testRunStepRow.ActualResult = testRunStep.ActualResult;
							testRunStepRow.StartDate = testRunStep.StartDate;
							testRunStepRow.EndDate = testRunStep.EndDate;
							testRunStepRow.ActualDuration = testRunStep.ActualDuration;
							testRunStepRow.TestCaseId = testRunStep.TestCaseId;

							//Add to test run
							testRun.TestRunSteps.Add(testRunStepRow);

						}
					}

					//Now lets actually persist the test run with its steps
					context.TestRuns.AddObject(testRun);
					context.SaveChanges();

					testRunId = testRun.TestRunId;

				}

				//Now we need to update the test case 'last run' information if appropriate
				//(if we have a more recent end-date than the last-execution-date)
				if (updateProjectTestStatus)
				{
					RefreshTestCaseExecutionStatus2(projectId, testRunId);
				}

				if (triggerNotificationEvents)
				{
					//Fire test case notifications for status changes
					try
					{
						NotificationManager notificationManager = new NotificationManager();

						if (testRun != null)
						{
							//We apply the changes to the test case, for any matching properties
							testCase.StartTracking();
							if (testRun.ExecutionStatusId != testCase.ExecutionStatusId)
							{
								testCase.ExecutionStatusId = testRun.ExecutionStatusId;
							}
							if (testRun.EndDate != testCase.ExecutionDate)
							{
								testCase.ExecutionDate = testRun.EndDate;
							}
							if (testRun.ActualDuration != testCase.ActualDuration)
							{
								testCase.ActualDuration = testRun.ActualDuration;
							}
							notificationManager.SendNotificationForArtifact(testCase, null, null);
						}
					}
					catch (Exception exception)
					{
						//Log but don't throw
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Test Run Type Methods

		/// <summary>
		/// Retrieves a list of test run types
		/// </summary>
		/// <returns>List of test run types</returns>
		public List<TestRunType> RetrieveTypes()
		{
			const string METHOD_NAME = "RetrieveTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunType> testRunTypes;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRunTypes
								where t.IsActive
								orderby t.Name, t.TestRunTypeId
								select t;

					testRunTypes = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a test run type by its ID
		/// </summary>
		/// <param name="typeId">The ID of the type</param>
		/// <returns>List of test run types</returns>
		public TestRunType RetrieveTypeById(int typeId)
		{
			const string METHOD_NAME = "RetrieveTypeById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestRunType testRunType;
				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRunTypes
								where t.TestRunTypeId == typeId
								select t;

					testRunType = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunType;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Adhoc Retrieval Methods

		/// <summary>
		/// Retrieves a list of the daily count of test-runs for a day in the specified timezone offset
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release we want to filter on (null for all)</param>
		/// <param name="utcOffset">The number of hours the requested timezone is offset from UTC</param>
		/// <returns>List of test runs per day totalled</returns>
		public List<TestRun_DailyCount> RetrieveDailyCount(int projectId, double utcOffset, int? releaseId)
		{
			const string METHOD_NAME = "RetrieveDailyCount";

			System.Data.DataSet testRunCountDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the whole number of hours and minutes that the UTC offset is
				int utcOffsetHours = (int)Math.Truncate(utcOffset);
				double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
				int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

				//Execute the stored procedure
				List<TestRun_DailyCount> testRunDailyCounts;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					testRunDailyCounts = context.TestRun_RetrieveDailyCount(projectId, releaseId, utcOffsetHours, utcOffsetMinutes).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunDailyCounts;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region TestRuns Pending Methods

		/// <summary>
		/// Retrieves all the active pending test runs for a particular project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>A list of pending test runs</returns>
		public List<TestRunsPending> RetrievePending(int projectId)
		{
			const string METHOD_NAME = "RetrievePendingByUserId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunsPending> pendingRuns;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRunsPendings
									.Include(t => t.Tester)
									.Include(t => t.Tester.Profile)
								where t.ProjectId == projectId
								select t;

					//Sort by date
					query = query.OrderByDescending(t => t.CreationDate).ThenBy(t => t.TestRunsPendingId);

					//Execute
					pendingRuns = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return pendingRuns;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Reassigns an existing test run pending to a different user
		/// </summary>
		/// <param name="testRunsPendingId">The id of the pending test run</param>
		/// <param name="newAssigneeId">The id of the user it is to be assigned to</param>
		/// <param name="userId">The id of the user making the change (not currently used)</param>
		public void ReassignPending(int testRunsPendingId, int newAssigneeId, int userId)
		{
			const string METHOD_NAME = "ReassignPending";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the pending test run by its ID
					var query = from t in context.TestRunsPendings.Include(t => t.TestRuns)
								where t.TestRunsPendingId == testRunsPendingId
								select t;

					TestRunsPending testRunsPending = query.FirstOrDefault();
					if (testRunsPending != null)
					{
						//Get the list of active users in the project
						List<User> activeUsers = new UserManager().RetrieveActiveByProjectId(testRunsPending.ProjectId);

						//Make sure the new assignee is in the project
						if (!activeUsers.Any(u => u.UserId == userId))
						{
							throw new InvalidOperationException("The specified user is not a member of the project!");
						}

						//Make the change to the pending run and the test runs
						//For now we assign all the runs, not just the not started ones
						//We might change that in the future
						//We only assign the individual runs that are assigned to the primary tester
						int oldAssigneeId = testRunsPending.TesterId;
						testRunsPending.StartTracking();
						testRunsPending.TesterId = newAssigneeId;
						foreach (TestRun testRun in testRunsPending.TestRuns)
						{
							if (testRun.TesterId == oldAssigneeId)
							{
								testRun.StartTracking();
								testRun.TesterId = newAssigneeId;
							}
						}
						context.SaveChanges();
					}
				}

				//This change doesn't affect the execution status, so no additional tasks needed

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion
	}

	/// <summary>
	/// Used to store a test run step for an automated test run
	/// </summary>
	public class TestRunStepInfo
	{
		/// <summary>
		/// The id of the test run step
		/// </summary>
		public Nullable<int> TestRunStepId;

		/// <summary>
		/// The id of the test step the test run step is based on
		/// </summary>
		public Nullable<int> TestStepId;

		/// <summary>
		/// The id of the execution status of the test run step result
		/// </summary>
		/// <remarks>
		/// Failed = 1;
		/// Passed = 2;
		/// NotRun = 3;
		/// NotApplicable = 4;
		/// Blocked = 5;
		/// Caution = 6;
		/// </remarks>
		public int ExecutionStatusId;

		/// <summary>
		/// The positional order of the test run step in the test run
		/// </summary>
		public int Position;

		/// <summary>
		/// The description of what the tester should do when executing the step
		/// </summary>
		public string Description;

		/// <summary>
		/// The expected result that should oocur when the tester executes the step
		/// </summary>
		public string ExpectedResult;

		/// <summary>
		/// The sample data that should be used by the tester
		/// </summary>
		public string SampleData;

		/// <summary>
		/// The actual result that occurs when the tester executes the step
		/// </summary>
		public string ActualResult;

		/// <summary>
		/// The time the test run step was started
		/// </summary>
		public DateTime? StartDate;

		/// <summary>
		/// The time the test run step was finished
		/// </summary>
		public DateTime? EndDate;

		/// <summary>
		/// How long the specific test run step took
		/// </summary>
		public int? ActualDuration;
	}


	/// <summary>
	/// Used to manage rearranging test run step positions
	/// e.g. during exploratory testing
	/// this class aligns with the data object defined in the ajax service
	/// </summary>
	public class TestRunStepPosition
	{
		/// <summary>
		/// The id of the test run step
		/// </summary>
		public int TestRunStepId;

		/// <summary>
		/// The id of the test step the test run step is based on
		/// </summary>
		public int Position;
	}


}
