using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Activation;
using System.Threading;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Allows ASP.NET AJAX pages to run time-consuming tasks without the page timing out and also
	/// provides a mechanism for notifying the user what is going on
	/// </summary>
	/// <remarks>
	/// See the following references:
	/// 1) http://msdn.microsoft.com/en-us/library/ms978607#diforwc-ap02_plag_howtomultithread
	/// 2) http://devarchive.net/displaying_progress_bar_for_long_running_processes.aspx
	/// </remarks>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class BackgroundProcessService : AjaxWebServiceBase, IBackgroundProcessService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService::";


		/// <summary>
		/// Stores the list of currently active background tasks
		/// </summary>
		protected static ConcurrentDictionary<string, ProcessStatus> processStatusList = new ConcurrentDictionary<string, ProcessStatus>();

		/// <summary>
		/// Starts a new background process
		/// </summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the current project (optional)</param>
		/// <param name="parameter1">Any operation-specific parameter value (optional)</param>
		/// <param name="parameter2">Any operation-specific array parameter value (optional)</param>
		/// <param name="parameter3">Any operation-specific array parameter value (optional)</param>
		/// <param name="operation">The name of the operation</param>
		/// <returns>The id of the new process (GUID)</returns>
		public string LaunchNewProcess(int? projectId, string operation, int? parameter1, List<int> parameter2, string parameter3)
		{
			const string METHOD_NAME = "LaunchNewProcess";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			//Need to check the permissions based on the type of operation
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the current timezone (some tasks need this information)
				string timezoneId = SpiraContext.Current.TimezoneId;

				//Get the current culture (tasks need that to localize correctly)
				string cultureName = Thread.CurrentThread.CurrentCulture.Name;

				//Get the windows folder corresponding to the application root
				string appRootPath = System.Web.HttpContext.Current.Server.MapPath("~");

				//Get the template associated with the project (if a project specified)
				int? projectTemplateId = null;
				if (projectId.HasValue)
				{
					projectTemplateId = new TemplateManager().RetrieveForProject(projectId.Value).ProjectTemplateId;
				}

				//Create a new process status class
				ProcessStatus newProcess = new ProcessStatus(operation);

				//Prevent synchronization issues by locking the dictionary
				lock (processStatusList)
				{
					//Add to the list of active tasks
					processStatusList.TryAdd(newProcess.ProcessId, newProcess);

					//Package up the data being passed
					object state = new object[] { newProcess, userId, projectId, parameter1, parameter2, parameter3, timezoneId, appRootPath, cultureName, projectTemplateId };

					//Actually start the appropriate task
					ThreadPool.QueueUserWorkItem(new WaitCallback(LaunchNewProcess_StartProcessing), state);
				}

				return newProcess.ProcessId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>The actual function called by the backrgound thread</summary>
		/// <param name="data">The process data</param>
		public void LaunchNewProcess_StartProcessing(object data)
		{
			const string METHOD_NAME = "LaunchNewProcess_StartProcessing";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			bool standardSuccess = true;

			try
			{
				//Get the process status object and other parameters
				object[] array = (object[])data;
				ProcessStatus processStatus = (ProcessStatus)array[0];
				int userId = (int)array[1];
				int? projectId = (int?)array[2];
				int? parameter1 = (int?)array[3];
				List<int> parameter2 = (List<int>)array[4];
				string parameter3 = (string)array[5];
				string timezoneId = (string)array[6];
				string appRootPath = (string)array[7];
				string cultureName = (string)array[8];
				int? projectTemplateId = (int?)array[9];

				//Set the culture if specified
				if (!string.IsNullOrEmpty(cultureName) && Thread.CurrentThread.CurrentCulture.Name != cultureName)
				{
					Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
					Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
				}

				//See what operation we have
				try
				{
					switch (processStatus.Name.ToLowerInvariant().Trim())
					{
						#region Project: Copy
						case "project_copy":
							{
								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);

								//Copy the project
								ProjectManager project = new ProjectManager();
								//We use parameter1 as that's the project to be copied vs. the current project
								project.Copy(userId, parameter1.Value, false, processStatus.Update);
								break;
							}
						#endregion

						#region Project and Template: Copy
						case "project_copy_template_copy":
							{
								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);

								//Copy the project and also the template (passing in TRUE for the third param of the copy method
								ProjectManager project = new ProjectManager();
								//We use parameter1 as that's the project to be copied vs. the current project
								project.Copy(userId, parameter1.Value, true, processStatus.Update);
								break;
							}
						#endregion

						#region Project: Copy and Reset
						case "project_copyreset":
							{
								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);

								//Copy and reset the project
								ProjectManager project = new ProjectManager();
								//We use parameter1 as that's the project to be copied vs. the current project
								project.CopyReset(userId, parameter1.Value, false, processStatus.Update);
								break;
							}
						#endregion

						#region Project and Template: Copy and Reset
						case "project_copyreset_template_copy":
							{
								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);

								//Copy and reset the project and also copy the template (passing in TRUE for the third param of the CopyReset method
								ProjectManager project = new ProjectManager();
								//We use parameter1 as that's the project to be copied vs. the current project
								project.CopyReset(userId, parameter1.Value, true, processStatus.Update);
								break;
							}
						#endregion

						#region Project: Delete
						case "project_delete":
							{
								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);

								//Delete the project
								ProjectManager projectManager = new ProjectManager();
								//We use parameter1 as that's the project to be deleted vs. the current project
								projectManager.Delete(userId, parameter1.Value, processStatus.Update);
								break;
							}
						#endregion

						#region Project: Remap Template
						case "project_remaptemplate":
							{
								//Make sure we have a project
								if (!projectId.HasValue)
								{
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);
								}

								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);

								//Remap the project
								int newTemplateId = parameter1.Value;
								TemplateManager templateManager = new TemplateManager();
								//We use parameter1 as that's the project to be deleted vs. the current project
								templateManager.ChangeProjectTemplate(projectId.Value, newTemplateId, userId, processStatus.Update);
								break;
							}
						#endregion

						#region Data Tools: Refresh Folder Hierarchies
						case "datatool_refreshfolderhieararchy":
							{
								//Make sure we have a project
								if (!projectId.HasValue)
								{
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);
								}

								//Test Case Folders
								processStatus.Progress = 25;
								processStatus.Message = Resources.Messages.Admin_DataTools_UpdatingTestCaseFolderHierarchy;
								new Business.TestCaseManager().TestCaseFolder_RefreshHierarchy(projectId.Value);

								//Test Set Folders
								processStatus.Progress = 50;
								processStatus.Message = Resources.Messages.Admin_DataTools_UpdatingTestSetFolderHierarchy;
								new Business.TestSetManager().TestSetFolder_RefreshHierarchy(projectId.Value);

								//Task Folders
								processStatus.Progress = 75;
								processStatus.Message = Resources.Messages.Admin_DataTools_UpdatingTaskFolderHierarchy;
								new Business.TaskManager().TaskFolder_RefreshHierarchy(projectId.Value);

								//Document Folders
								processStatus.Progress = 100;
								processStatus.Message = Resources.Messages.Admin_DataTools_UpdatingDocumentFolderHierarchy;
								new Business.AttachmentManager().ProjectAttachmentFolder_RefreshHierarchy(projectId.Value);

								break;
							}
						#endregion

						#region Data Tools: Refresh Parameter Hierarchies
						case "datatool_refreshtestcaseparameters":
							{
								//Make sure we have a project
								if (!projectId.HasValue)
								{
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);
								}

								//Set progress..
								processStatus.Progress = 50;
								processStatus.Message = Resources.Messages.Admin_DataTools_UpdatingTestCaseParameterHierarchy;

								//Do the actual refresh
								new Business.TestCaseManager().TestCase_RefreshParameterHierarchyForProject(projectId.Value);

								break;
							}
						#endregion

						#region Data Tools: Refresh Progress/Status
						case "datatool_refreshteststatustaskprogress":
							{
								//Set progress..
								processStatus.Progress = 50;
								processStatus.Message = Resources.Messages.Admin_DataTools_UpdatingTaskProgressAndTestStatus;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Call the method to refresh the data, we do synchronously,since already on background thread
								new Business.ProjectManager().RefreshTestStatusAndTaskProgressCache(projectId.Value, false);

								break;
							}
						#endregion

						#region Test Case: Execute
						case "testcase_execute":
							{
								//Make sure we have a project id
								if (!projectId.HasValue)
								{
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);
								}

								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
								{
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);
								}
								int testCaseId = parameter1.Value;

								//Make sure that  the user is authorized to execute test cases in this project
								ProjectManager projectManager = new ProjectManager();
								ProjectUserView projectRole = projectManager.RetrieveUserMembershipById(projectId.Value, userId);
								if (projectRole == null || projectManager.IsAuthorized(projectRole.ProjectRoleId, Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create) == Project.AuthorizationState.Prohibited)
								{
									processStatus.Progress = 100;
									processStatus.setStatus_Error();
									processStatus.Message = Resources.Messages.Services_NotAuthorizedCreateTestRuns;
									return;
								}

								//Make sure that this test case is in a workflow status that allows execution
								if (!new TestCaseManager().IsTestCaseInExecutableStatus(testCaseId))
								{
									processStatus.Progress = 100;
									processStatus.setStatus_Error();
									processStatus.Message = Resources.Messages.Services_TestCase_NotInExecutableStatus;
									return;
								}

								//Execute the test case
								TestRunManager testRunManager = new TestRunManager();

								//Create a shell test run set from the list of test case IDs
								List<int> testCases = new List<int>() { testCaseId };
								TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(userId, projectId.Value, null, testCases, true, null, processStatus.Update);

								//Get test run pending record and then use that to return as the success value
								//The page will then do the appropriate redirect
								if (testRunsPending != null)
								{
									int testRunsPendingId = testRunsPending.TestRunsPendingId;
									processStatus.ReturnCode = testRunsPendingId;

									//checks to see if this should be an exploratory test run or not
									bool executeAsExploratory = CheckTestExecutionExploratory(testRunsPending, projectId, projectManager, projectRole);
									if (executeAsExploratory)
									{
										processStatus.ReturnMeta = "testcase_executeexploratory";
									}
								}
								break;
							}
						#endregion

						#region Test Case: Multiple Cases Execute
						case "testcase_executemultiple":
							{
								//Make sure we have a project id
								if (!projectId.HasValue)
								{
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);
								}

								//Parameter 1 is the release and is optional
								int? releaseId = parameter1;

								//Make sure we have a parameter 2 (list of test cases)
								if (parameter2 == null || parameter2.Count == 0)
								{
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter2NotSpecified);
								}

								//Make sure that  the user is authorized to execute test cases in this project
								ProjectManager projectManager = new ProjectManager();
								ProjectUserView projectRole = projectManager.RetrieveUserMembershipById(projectId.Value, userId);
								if (projectRole == null || projectManager.IsAuthorized(projectRole.ProjectRoleId, Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create) == Project.AuthorizationState.Prohibited)
								{
									processStatus.Progress = 100;
									processStatus.setStatus_Error();
									processStatus.Message = Resources.Messages.Services_NotAuthorizedCreateTestRuns;
									return;
								}

								//The folder ids are negative
								List<int> testCaseAndFolderIds = parameter2;
								List<int> testCaseIds = testCaseAndFolderIds.Where(t => t > 0).ToList();
								List<int> testFolderIds = testCaseAndFolderIds.Where(t => t < 0).Select(t => -t).ToList();

								//Execute the test case
								Business.TestRunManager testRunManager = new Business.TestRunManager();

								//Create a shell test run dataset from the list of test case IDs
								TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(userId, projectId.Value, releaseId, testCaseIds, true, testFolderIds, processStatus.Update);

								//Get test run pending record and then use that to return as the success value
								//The page will then do the appropriate redirect
								if (testRunsPending != null)
								{
									int testRunsPendingId = testRunsPending.TestRunsPendingId;
									processStatus.ReturnCode = testRunsPendingId;

									//checks to see if this should be an exploratory test run or not
									bool executeAsExploratory = CheckTestExecutionExploratory(testRunsPending, projectId, projectManager, projectRole);
									if (executeAsExploratory)
									{
										processStatus.ReturnMeta = "testcase_executeexploratory";
									}
								}
								break;
							}
						#endregion

						#region Test Set: Execute
						case "testset_execute":
							{
								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);
								int testSetId = parameter1.Value;

								//Make sure that  the user is authorized to execute test cases in this project
								ProjectManager projectManager = new ProjectManager();
								ProjectUserView projectRole = projectManager.RetrieveUserMembershipById(projectId.Value, userId);
								if (projectRole == null || projectManager.IsAuthorized(projectRole.ProjectRoleId, Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create) == Project.AuthorizationState.Prohibited)
								{
									processStatus.Progress = 100;
									processStatus.setStatus_Error();
									processStatus.Message = Resources.Messages.Services_NotAuthorizedCreateTestRuns;
									return;
								}

								//Execute the test set
								TestRunManager testRunManager = new TestRunManager();

								//Create a shell test run dataset from the test set
								try
								{
									TestRunsPending testRunsPending = testRunManager.CreateFromTestSet(userId, projectId.Value, testSetId, true, processStatus.Update);

									//Get test run pending record and then use that to return as the success value
									//The page will then do the appropriate redirect
									if (testRunsPending != null)
									{
										int testRunsPendingId = testRunsPending.TestRunsPendingId;
										processStatus.ReturnCode = testRunsPendingId;
									}
								}
								catch (TestSetNotManualException)
								{
									//We just return -2 as the return code so that the calling page knows to handle it correctly
									processStatus.ReturnCode = -2;
								}
								break;
							}
						#endregion

						#region Test Set / Test Case: Execute
						case "testset_testcaseexecute":
							{
								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Make sure we have a parameter 1
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);
								int testSetId = parameter1.Value;

								//Make sure we have a parameter 2
								if (parameter2 == null || parameter2.Count == 0)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter2NotSpecified);

								//Make sure that  the user is authorized to execute test cases in this project
								ProjectManager projectManager = new ProjectManager();
								ProjectUserView projectRole = projectManager.RetrieveUserMembershipById(projectId.Value, userId);
								if (projectRole == null || projectManager.IsAuthorized(projectRole.ProjectRoleId, Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.Create) == Project.AuthorizationState.Prohibited)
								{
									processStatus.Progress = 100;
									processStatus.setStatus_Error();
									processStatus.Message = Resources.Messages.Services_NotAuthorizedCreateTestRuns;
									return;
								}

								//Execute the test set
								TestRunManager testRunManager = new TestRunManager();

								//Create a shell test run dataset from the test cases in the test set
								TestRunsPending testRunsPending = testRunManager.CreateFromTestCasesInSet(userId, projectId.Value, testSetId, parameter2, true, processStatus.Update);

								//Get test run pending record and then use that to return as the success value. The page will then do the appropriate redirect
								if (testRunsPending != null)
								{
									int testRunsPendingId = testRunsPending.TestRunsPendingId;
									processStatus.ReturnCode = testRunsPendingId;

									//checks to see if this should be an exploratory test run or not
									bool executeAsExploratory = CheckTestExecutionExploratory(testRunsPending, projectId, projectManager, projectRole);
									if (executeAsExploratory)
									{
										processStatus.ReturnMeta = "testcase_executeexploratory";
									}
								}
								break;
							}
						#endregion

						#region Report: Generate
						case "report_generate":
							{
								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);
								if (!projectTemplateId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectTemplateIdNotSpecified);

								//Make sure we have a report id
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.ReportViewer_ReportIdNotValid);
								int reportId = parameter1.Value;

								//Make sure we have a report querystring
								if (String.IsNullOrEmpty(parameter3))
									throw new InvalidOperationException(Resources.Messages.ReportViewer_QueryStringEmpty);
								string queryString = parameter3;

								//Actually generate the report
								ReportManager reportManager = new ReportManager();
								int generatedReportId = reportManager.GenerateReport(userId, projectId.Value, projectTemplateId.Value, reportId, queryString, timezoneId, appRootPath, processStatus.Update);
								processStatus.ReturnCode = generatedReportId;

								break;
							}
						#endregion

						#region Testing: Test Functions
						case "testoperation":
							{
								processStatus.Progress = 0;
								processStatus.Message = "";
								Thread.Sleep(2000);
								processStatus.Progress = 20;
								processStatus.Message = "Copied Stuff";
								Thread.Sleep(2000);
								processStatus.Progress = 40;
								processStatus.Message = "Fixed Things";
								Thread.Sleep(2000);
								processStatus.Progress = 60;
								processStatus.Message = "Cleaning Up";
								Thread.Sleep(2000);
								processStatus.Progress = 80;
								processStatus.Message = "Finalizing";
								Thread.Sleep(2000);
								processStatus.Progress = 100;
								processStatus.Message = "";

								break;
							}

						case "testfailure":
							{
								processStatus.Progress = 0;
								processStatus.Message = "";
								Thread.Sleep(2000);
								processStatus.Progress = 20;
								processStatus.Message = "Copied Stuff";
								Thread.Sleep(2000);
								processStatus.Progress = 40;
								processStatus.Message = "Fixed Things";
								Thread.Sleep(2000);
								processStatus.Progress = 60;
								processStatus.Message = "Cleaning Up";
								Thread.Sleep(2000);

								//Now Fail
								throw new Exception("Unable to complete task, error 56!");
							}

						case "testwarning":
							{
								processStatus.Progress = 0;
								processStatus.Message = "";
								Thread.Sleep(2000);
								processStatus.Progress = 20;
								processStatus.Message = "Copied Stuff";
								Thread.Sleep(2000);
								processStatus.Progress = 40;
								processStatus.Message = "Fixed Things";
								Thread.Sleep(2000);
								processStatus.Progress = 60;
								processStatus.Message = "Cleaning Up";
								Thread.Sleep(2000);

								processStatus.Condition = ProcessStatus.ProcessStatusCondition.Warning;
								processStatus.Progress = 100;

								break;
							}
						#endregion

						#region Test Case: Refresh Status
						case "datatool_refreshteststatus":
							{
								//Set progress..
								processStatus.Progress = 50;
								processStatus.Message = Resources.Messages.Admin_DataTools_UpdatingTestStatus;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Call the method to refresh the data
								new Business.ProjectManager().RefreshTestStatusCache(projectId.Value);

								break;
							}
						#endregion

						#region Task: Refresh Progress
						case "datatool_refreshtaskprogress":
							{
								//Set progress..
								processStatus.Progress = 50;
								processStatus.Message = Resources.Messages.Admin_DataTools_UpdatingTaskProgress;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Call the method to refresh the data
								new Business.ProjectManager().RefreshTaskProgressCache(projectId.Value);

								break;
							}
						#endregion

						#region Requirements: Check Indentations
						case "datatool_checkindentationrequirement":
							{
								//Set progress..
								processStatus.Progress = 0;
								processStatus.Message = Resources.Messages.Admin_DataTools_IndentFixingRequirement;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Update indents..
								bool isCorrect = new RequirementManager().CheckIndention(projectId.Value, false, bkgProcess: processStatus);
								processStatus.ReturnCode = (isCorrect) ? 1 : 0;
								processStatus.ReturnMeta = "datatool_checkindentationrequirement";
							}
							break;

						#endregion

						#region Requirements: Correct Indentations
						case "datatool_indentationrequirement":
							{
								//Set progress..
								processStatus.Progress = 0;
								processStatus.Message = Resources.Messages.Admin_DataTools_IndentFixingRequirement;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Update indents.. then check
								RequirementManager requirementManager = new RequirementManager();
								requirementManager.CheckIndention(projectId.Value, true, bkgProcess: processStatus);
								bool isCorrect = requirementManager.CheckIndention(projectId.Value, false, bkgProcess: processStatus);
								processStatus.ReturnCode = (isCorrect) ? 1 : 0;
								processStatus.ReturnMeta = "datatool_indentationrequirement";

								break;
							}
						#endregion

						#region System: Refresh Database Indexes

						case "datatool_refreshdatabaseindexes":
							{
								//Set progress..
								processStatus.Progress = 0;
								processStatus.Message = Resources.Messages.Admin_DataTools_RefreshingDatabaseIndexes;

								//Refresh Indexes, no progress displayable
								new SystemManager().Database_RefreshIndexes();
								break;
							}

						#endregion

						#region System: Delete Sample Data

						case "system_deletesampledata":
							{
								//Set progress..
								processStatus.Progress = 50;
								processStatus.Message = Resources.Messages.Admin_SystemInfo_DeletingSampleData;
								new SystemManager().Database_DeleteSampleData(userId);
								processStatus.Progress = 100;
								break;
							}

						#endregion

						#region Releases: Check Indentations
						case "datatool_checkindentationrelease":
							{
								//Set progress..
								processStatus.Progress = 0;
								processStatus.Message = Resources.Messages.Admin_DataTools_IndentFixingRelease;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Update indents..
								bool isCorrect = new ReleaseManager().CheckIndention(projectId.Value, false, bkgProcess: processStatus);
								processStatus.ReturnCode = (isCorrect) ? 1 : 0;
								processStatus.ReturnMeta = "datatool_checkindentationrelease";
							}
							break;

						#endregion

						#region Release: Correct Indentations
						case "datatool_indentationrelease":
							{
								//Set progress..
								processStatus.Progress = 0;
								processStatus.Message = Resources.Messages.Admin_DataTools_IndentFixingRelease;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//Update indents, then check
								ReleaseManager releaseManager = new ReleaseManager();
								releaseManager.CheckIndention(projectId.Value, true, bkgProcess: processStatus);
								bool isCorrect = releaseManager.CheckIndention(projectId.Value, false, bkgProcess: processStatus);
								processStatus.ReturnCode = (isCorrect) ? 1 : 0;
								processStatus.ReturnMeta = "datatool_indentationrelease";

								break;
							}
						#endregion

						#region History: Purge All
						case "project_purgehistory":
							{
								//Set progress..
								processStatus.Progress = 0;
								processStatus.Message = Resources.Messages.Admin_History_PurgingAll;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//If Baselining is enabled, just abort.
								ProjectSettings projSettings = new ProjectSettings(projectId.Value);
								if (projSettings.BaseliningEnabled && Common.Global.Feature_Baselines)
									throw new InvalidOperationException("Baselines: Event can not execute.");

								//Update indents..
								new HistoryManager().PurgeAllDeleted(projectId.Value, userId, processStatus);

								break;
							}
						#endregion

						#region History: Restore Selected Items
						case "project_revertselected":
							{
								//Set progress..
								processStatus.Progress = 0;
								processStatus.Message = Resources.Messages.Admin_History_Restoring;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);
								if (!projectTemplateId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectTemplateIdNotSpecified);
								if (parameter2 == null || parameter2.Count < 1)
									throw new InvalidOperationException(Resources.Messages.Admin_History_RestoreError);

								//If Baselining is enabled, make sure we only process items that are later than the last baseline.
								ProjectSettings projSettings = new ProjectSettings(projectId.Value);
								long? baselineChangeSetId = null;
								if (projSettings.BaseliningEnabled && Common.Global.Feature_Baselines)
								{
									//baselineChangeSetId = new BaselineManager().Baseline_RetrieveForProduct(projectId)?.Select(b => b.BaselineId)?.Max();
									var baselines = new BaselineManager().Baseline_RetrieveForProduct(projectId.Value);
									if (baselines != null && baselines.Count > 0)
										baselineChangeSetId = baselines.Select(b => b.ChangeSetId).Max();
								}

								//The history manager..
								HistoryManager historyManager = new HistoryManager();

								if (parameter2.Count == 1)
								{
									int changeSetId = parameter2[0];

									//Check that we either don't have any baselines OR the change set is after the highest baseline change set id => means we can revert
									if (baselineChangeSetId == null || changeSetId >= baselineChangeSetId)
									{
										//Otherwise, we have one. Get details and roll it back.
										HistoryChangeSet historyChangeSet = historyManager.RetrieveChangeSetById(changeSetId, false);

										if (historyChangeSet != null)
										{
											//Revert the changeset.
											string rollbackLog = "";
											HistoryManager.RollbackResultEnum hisResult = historyManager.RollbackHistory(projectId.Value, projectTemplateId.Value, (DataModel.Artifact.ArtifactTypeEnum)historyChangeSet.ArtifactTypeId, historyChangeSet.ArtifactId, changeSetId, userId, ref rollbackLog);

											//Now see if it was rolled back correctly or not.
											standardSuccess = false;
											switch (hisResult)
											{
												case HistoryManager.RollbackResultEnum.Error:
													processStatus.Condition = ProcessStatus.ProcessStatusCondition.Error;
													processStatus.Progress = 100;
													processStatus.Message = "There was an error restoring the changeset.";
													break;
												case HistoryManager.RollbackResultEnum.Warning:
													processStatus.Condition = ProcessStatus.ProcessStatusCondition.Warning;
													processStatus.Progress = 100;
													processStatus.Message = "Changeset was successfully rolled back, however not all fields could be reverted.";
													break;
												case HistoryManager.RollbackResultEnum.Success:
													processStatus.Condition = ProcessStatus.ProcessStatusCondition.Completed;
													processStatus.Progress = 100;
													processStatus.Message = "Changeset was successfully rolled back.";
													break;
											}
										}
									}
									else
									{
										throw new Exception(Resources.Messages.Admin_History_BaselineCantRevert);
									}
								}
								else if (parameter2.Count > 1)
								{
									//Create our dictionary
									List<HistoryRecord> lstHist = new List<HistoryRecord>();

									//Loop through each one checked, and get our consolidated list, first.
									int count = 0;
									foreach (int changeSetId in parameter2)
									{
										//Update process..
										processStatus.Progress = (int)(++count / (float)parameter2.Count - 1) * 100;

										//Check that it's within our latest Baseline. If not, we just skip it.
										if (!baselineChangeSetId.HasValue ||
											changeSetId >= baselineChangeSetId.Value)
										{

											//Get the history changeset..
											HistoryChangeSet historyChangeSet = historyManager.RetrieveChangeSetById(changeSetId, false);

											//See if the artifact already exists in our list.
											bool foundItem = false;
											for (int i = 0; i < lstHist.Count; i++)
											{
												if (lstHist[i].ArtifactId == historyChangeSet.ArtifactId && lstHist[i].ArtifactType == historyChangeSet.ArtifactTypeId)
												{
													foundItem = true;
													//Updat the changeset only if it's lower.
													if (lstHist[i].ChangeSet >= historyChangeSet.ChangeSetId)
														lstHist[i].ChangeSet = historyChangeSet.ChangeSetId;
												}
											}
											//If we didn't find out, we need to add it.
											if (!foundItem)
											{
												lstHist.Add(new HistoryRecord()
												{
													ArtifactId = historyChangeSet.ArtifactId,
													ArtifactType = historyChangeSet.ArtifactTypeId,
													ChangeSet = historyChangeSet.ChangeSetId
												});
											}
										}
									}

									//Now that we have our list, we'll loop throough each one, and roll it back.
									HistoryManager.RollbackResultEnum totalStatus = HistoryManager.RollbackResultEnum.Success;
									foreach (HistoryRecord hstRec in lstHist)
									{
										string rollbackLog = "";
										HistoryManager.RollbackResultEnum hisResult = new HistoryManager().RollbackHistory(projectId.Value, projectTemplateId.Value, (DataModel.Artifact.ArtifactTypeEnum)hstRec.ArtifactType, hstRec.ArtifactId, hstRec.ChangeSet, userId, ref rollbackLog);

										//Update the status, if needed.
										if (hisResult > totalStatus)
											totalStatus = hisResult;
									}

									//Now see if it was rolled back correctly or not.
									standardSuccess = false;
									switch (totalStatus)
									{
										case HistoryManager.RollbackResultEnum.Error:
											processStatus.Condition = ProcessStatus.ProcessStatusCondition.Error;
											processStatus.Progress = 100;
											processStatus.Message = "There was an error restoring one or more of the changesets.";
											break;
										case HistoryManager.RollbackResultEnum.Warning:
											processStatus.Condition = ProcessStatus.ProcessStatusCondition.Warning;
											processStatus.Progress = 100;
											processStatus.Message = "Changesets were successfully rolled back. However, not all fields could be reverted.";
											break;
										case HistoryManager.RollbackResultEnum.Success:
											processStatus.Condition = ProcessStatus.ProcessStatusCondition.Completed;
											processStatus.Progress = 100;
											processStatus.Message = "All Changesets were successfully rolled back.";
											break;
									}
								}

								break;
							}
						#endregion

						#region History: Purge Item
						case "project_purgeitem":
							{
								//Set progress..
								processStatus.Progress = 0;
								processStatus.Message = Resources.ClientScript.Admin_History_Processing3;

								//Make sure we have a project id
								if (!projectId.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_ProjectIdNotSpecified);

								//We should get the ChangeSetId from the call.
								if (!parameter1.HasValue)
									throw new InvalidOperationException(Resources.Messages.BackgroundProcessService_Parameter1NotSpecified);

								//Purge the item..
								new HistoryManager().PurgeItem(projectId.Value, parameter1.Value, userId, processStatus);
								break;
							}
						#endregion

						#region System: URL Test
#if DEBUG
						case "system_urltimer":
							{
								processStatus.Progress = 0;
								processStatus.Message = "Hitting URLs...";

								string output = URLTimeTest.RunTest(ConfigurationSettings.Default.General_WebServerUrl, processStatus);

								processStatus.Progress = 100;
								processStatus.Message = "Done!";
								processStatus.ReturnMeta = output;

								break;
							}
#endif
							#endregion
					}

					//Mark the operation as completed
					if (standardSuccess)
					{
						processStatus.Condition = ProcessStatus.ProcessStatusCondition.Completed;
						processStatus.Message = Resources.Messages.BackrgroundProcessService_CompletedSuccessfully;
						processStatus.Progress = 100;
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				}
				catch (Exception exception)
				{
					//Log the error but don't throw as that can cause issues during async operations
					processStatus.Condition = ProcessStatus.ProcessStatusCondition.Error;
					processStatus.Message = exception.Message;
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				}
			}
			catch (Exception exception)
			{
				//Log the error but don't throw as that can cause issues during async operations
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
			}
		}

		/// <summary>
		/// Gets an object that contains the status of the requested process
		/// </summary>
		/// <param name="processId">The id of the background process (GUID)</param>
		/// <returns>The status object</returns>
		public ProcessStatus GetProcessStatus(string processId)
		{
			const string METHOD_NAME = "GetProcessStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProcessStatus processStatus = null;

				//See if we have the process requested
				if (processStatusList.ContainsKey(processId))
				{
					processStatus = processStatusList[processId];

					//If the status is error or completed, now we need to remove it from the dictionary
					if (processStatus.Condition == ProcessStatus.ProcessStatusCondition.Completed || processStatus.Condition == ProcessStatus.ProcessStatusCondition.Error)
					{
						ProcessStatus removed;
						processStatusList.TryRemove(processId, out removed);
					}
				}

				return processStatus;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Stores a record of artifact types and IDs, to find the earliest one.</summary>
		private class HistoryRecord
		{
			public int ArtifactId
			{
				get;
				set;
			}
			public int ArtifactType
			{
				get;
				set;
			}
			public long ChangeSet
			{
				get;
				set;
			}
		}


		/// <summary>
		/// Checks to see whether the test passed in should be executed as exploratory or not
		/// </summary>
		/// <param name="testRunsPending">the skeleton test run - used to check the count of test runs and get the relevant test case id</param>
		/// <param name="projectId">Project ID (optional)</param>
		/// <param name="projectManager">passed in to help with permissions checking</param>
		/// <param name="projectRole">passed in to help with permissions checking</param>
		/// <returns>a boolean of true if it should be exploratory and there are sufficient permissions</returns>
		private bool CheckTestExecutionExploratory(TestRunsPending testRunsPending, int? projectId, ProjectManager projectManager, ProjectUserView projectRole)
		{
			bool isExecutionExploratory = false;

			//exploratory only possible when a single test case is selected
			bool onlyOneTestCase = testRunsPending.TestRuns.Count == 1;

			if (onlyOneTestCase)
			{
				//check permissions
				bool canExecuteExploratory = projectManager.IsAuthorized(projectRole.ProjectRoleId, Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.Create) != Project.AuthorizationState.Prohibited;
				if (canExecuteExploratory)
				{
					//if it looks like this could be run as an exploratory test, then we need to check if the test case is actually an exploratory test
					TestCaseManager testCaseManager = new TestCaseManager();

					bool isOfTypeExploratory = testCaseManager.RetrieveById2(projectId.Value, testRunsPending.TestRuns[0].TestCaseId, true, true).Type.IsExploratory;

					//if it is then add return data so that the client can manage required url redirect
					if (isOfTypeExploratory)
					{
						isExecutionExploratory = true;
					}
				}

			}


			return isExecutionExploratory;
		}
	}
}
