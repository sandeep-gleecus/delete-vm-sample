using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Helper class that populates the internal data classes from the RemoteObjects
    /// </summary>
    public static class UpdateFunctions
    {
        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteDataSyncSystem">The API data object</param>
        /// <param name="dataSyncSystem">The internal datarow</param>
        public static void UpdateDataSyncSystem(DataSyncSystem dataSyncSystem, RemoteDataSyncSystem remoteDataSyncSystem)
        {
            //Start Tracking
            dataSyncSystem.StartTracking();

            dataSyncSystem.Name = remoteDataSyncSystem.Name;
            dataSyncSystem.Caption = remoteDataSyncSystem.DisplayName;
            dataSyncSystem.Description = remoteDataSyncSystem.Description;
            dataSyncSystem.ConnectionString = remoteDataSyncSystem.ConnectionString;
            dataSyncSystem.ExternalLogin = remoteDataSyncSystem.Login;
            dataSyncSystem.ExternalPassword = remoteDataSyncSystem.Password;
            dataSyncSystem.TimeOffsetHours = remoteDataSyncSystem.TimeOffsetHours;
            dataSyncSystem.Custom01 = remoteDataSyncSystem.Custom01;
            dataSyncSystem.Custom02 = remoteDataSyncSystem.Custom02;
            dataSyncSystem.Custom03 = remoteDataSyncSystem.Custom03;
            dataSyncSystem.Custom04 = remoteDataSyncSystem.Custom04;
            dataSyncSystem.Custom05 = remoteDataSyncSystem.Custom05;
            dataSyncSystem.AutoMapUsersYn = (remoteDataSyncSystem.AutoMapUsers) ? "Y" : "N";
            dataSyncSystem.IsActive = remoteDataSyncSystem.IsActive;
        }   

        /// <summary>
        /// Updates the component from the API object
        /// </summary>
        /// <param name="component"></param>
        /// <param name="remoteComponent"></param>
        /// <remarks>We don't update the deleted flag, that is done using a delete API call instead</remarks>
        public static void UpdateComponent(Component component, RemoteComponent remoteComponent)
        {
            component.StartTracking();
            component.Name = remoteComponent.Name;
            component.IsActive = remoteComponent.IsActive;
        }

        /// <summary>
        /// Updates the pending test run from the passing in test run API objects
        /// </summary>
        /// <param name="testRunsPending">The pending test run set</param>
        /// <param name="remoteTestRuns">The API objects</param>
        /// <param name="projectId">The id of the project</param>
        public static void UpdatePendingTestRun(TestRunsPending testRunsPending, List<RemoteManualTestRun> remoteTestRuns, int projectId, int userId)
        {
            if (remoteTestRuns.Count > 0)
            {
                testRunsPending.TesterId = userId;
                testRunsPending.Name = remoteTestRuns[0].Name;  //Will be deleted when pending run completed, so name doesn't really matter
                testRunsPending.CreationDate = DateTime.UtcNow;
                testRunsPending.LastUpdateDate = DateTime.UtcNow;
                testRunsPending.ProjectId = projectId;
                testRunsPending.TestSetId = remoteTestRuns[0].TestSetId;
                testRunsPending.CountNotRun = remoteTestRuns.Count;
            }
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="testRunsPending">The internal dataset</param>
        /// <remarks>Updates both the test run and the test run steps</remarks>
        public static void UpdateManualTestRunData(TestRunsPending testRunsPending, RemoteManualTestRun remoteTestRun)
        {
            //First populate the test run itself
            TestRun testRun = new TestRun();
            testRunsPending.TestRuns.Add(testRun);
            testRun.Name = remoteTestRun.Name;
            testRun.TestCaseId = remoteTestRun.TestCaseId;
            testRun.TestRunTypeId = remoteTestRun.TestRunTypeId;
            if (remoteTestRun.TesterId.HasValue)
            {
                testRun.TesterId = remoteTestRun.TesterId.Value;
            }
            testRun.ExecutionStatusId = remoteTestRun.ExecutionStatusId;
            testRun.ReleaseId = remoteTestRun.ReleaseId;
            testRun.TestSetId = remoteTestRun.TestSetId;
            testRun.TestSetTestCaseId = remoteTestRun.TestSetTestCaseId;
            testRun.StartDate = remoteTestRun.StartDate;
            testRun.EndDate = remoteTestRun.EndDate;
            testRun.IsAttachments = false;
            testRun.BuildId = remoteTestRun.BuildId;
            testRun.EstimatedDuration = remoteTestRun.EstimatedDuration;
            testRun.ActualDuration = remoteTestRun.ActualDuration;

            //Next the test run steps
            if (remoteTestRun.TestRunSteps != null && remoteTestRun.TestRunSteps.Count > 0)
            {
                foreach (RemoteTestRunStep remoteTestRunStep in remoteTestRun.TestRunSteps)
                {
                    //Add a new test run step row
                    TestRunStep testRunStep = new TestRunStep();
                    testRunStep.ExecutionStatusId = remoteTestRunStep.ExecutionStatusId;
                    testRunStep.Position = remoteTestRunStep.Position;
                    testRunStep.Description = remoteTestRunStep.Description;
                    testRunStep.TestCaseId = remoteTestRunStep.TestCaseId;
                    testRunStep.TestStepId = remoteTestRunStep.TestStepId;
                    testRunStep.ExpectedResult = remoteTestRunStep.ExpectedResult;
                    testRunStep.SampleData = remoteTestRunStep.SampleData;
                    testRunStep.ActualResult = remoteTestRunStep.ActualResult;
                    testRunStep.ActualDuration = remoteTestRunStep.ActualDuration;
                    testRunStep.StartDate = remoteTestRunStep.StartDate;
                    testRunStep.EndDate = remoteTestRunStep.EndDate;
                    testRun.TestRunSteps.Add(testRunStep);
                }
            }

            //Concurrency management
            testRun.ConcurrencyDate = remoteTestRun.ConcurrencyDate;
        }

        /// <summary>
        /// Adds the test set custom property values to the automated test run
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="remoteTestRuns">The list of test run api data objects</param>
        public static void AddCustomPropertyValuesToAutomatedTestRun(int projectId, int projectTemplateId, int testSetId, List<RemoteAutomatedTestRun> remoteTestRuns)
        {
            //If the test set had some list custom properties set then we should look for corresponding
            //lists in the test run and if there are matching lists we should set the values to be the same
            CustomPropertyManager customPropertyManager = new CustomPropertyManager();
            ArtifactCustomProperty testSetCustomProperties = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, true);

            //Make sure we have some custom properties on the test set
            if (testSetCustomProperties != null)
            {
                //Iterate through the list of test runs and set the appropriate custom properties (if any)
                List<CustomProperty> testRunCustomPropertyDefinitions = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);
                foreach (RemoteAutomatedTestRun remoteTestRun in remoteTestRuns)
                {
                    if (remoteTestRun.TestSetId.HasValue && remoteTestRun.TestSetId == testSetId)
                    {
                        foreach (CustomProperty testSetCustomPropertyDefinition in testSetCustomProperties.CustomPropertyDefinitions)
                        {
                            //See if we have a matching property in the test run custom properties (only works for lists)
                            if (testSetCustomPropertyDefinition.CustomPropertyListId.HasValue)
                            {
                                foreach (CustomProperty testRunCustomPropertyDefinition in testRunCustomPropertyDefinitions)
                                {
                                    if (testRunCustomPropertyDefinition.CustomPropertyListId.HasValue && testRunCustomPropertyDefinition.CustomPropertyListId.Value == testSetCustomPropertyDefinition.CustomPropertyListId.Value
                                        && testRunCustomPropertyDefinition.CustomPropertyTypeId == testSetCustomPropertyDefinition.CustomPropertyTypeId)
                                    {
                                        //We have a matching custom list between the test set and test run
                                        //So set the value on the matching test run property
                                        if (testRunCustomPropertyDefinition.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List)
                                        {
                                            if (remoteTestRun.CustomProperties == null)
                                            {
                                                remoteTestRun.CustomProperties = new List<RemoteArtifactCustomProperty>();
                                            }

                                            RemoteArtifactCustomProperty remoteArtifactCustomProperty = new RemoteArtifactCustomProperty();
                                            remoteArtifactCustomProperty.PropertyNumber = testRunCustomPropertyDefinition.PropertyNumber;
                                            int? customPropertyValue = (int?)testSetCustomProperties.CustomProperty(testSetCustomPropertyDefinition.PropertyNumber);
                                            remoteArtifactCustomProperty.IntegerValue = customPropertyValue;
                                            remoteTestRun.CustomProperties.Add(remoteArtifactCustomProperty);
                                        }
                                        if (testRunCustomPropertyDefinition.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
                                        {
                                            if (remoteTestRun.CustomProperties == null)
                                            {
                                                remoteTestRun.CustomProperties = new List<RemoteArtifactCustomProperty>();
                                            }

                                            RemoteArtifactCustomProperty remoteArtifactCustomProperty = new RemoteArtifactCustomProperty();
                                            remoteArtifactCustomProperty.PropertyNumber = testRunCustomPropertyDefinition.PropertyNumber;
                                            List<int> customPropertyValue = (List<int>)testSetCustomProperties.CustomProperty(testSetCustomPropertyDefinition.PropertyNumber);
                                            remoteArtifactCustomProperty.IntegerListValue = customPropertyValue;
                                            remoteTestRun.CustomProperties.Add(remoteArtifactCustomProperty);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds both test set and test case parameters to the API test run object
        /// </summary>
        /// <param name="remoteTestRun">The Test Run API object</param>
        /// <param name="testCaseParameters">The default test case parameter values</param>
        /// <param name="testSetParameterValues">The list of test set parameter values</param>
        /// <param name="testSetTestCaseParameterValues">The list of test set parameter values</param>
        public static void AddParameterValues(RemoteAutomatedTestRun remoteTestRun, List<TestCaseParameter> testCaseParameters, List<TestSetParameter> testSetParameterValues, List<TestSetTestCaseParameter> testSetTestCaseParameterValues)
        {
            if (testSetParameterValues.Count > 0 || testSetTestCaseParameterValues.Count > 0 || testCaseParameters.Count > 0)
            {
                remoteTestRun.Parameters = new List<RemoteTestSetTestCaseParameter>();
                //Test Set/Case Parameters
                foreach (TestSetTestCaseParameter parameterValue in testSetTestCaseParameterValues)
                {
                    RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter = new RemoteTestSetTestCaseParameter();
                    remoteTestSetTestCaseParameter.Name = parameterValue.Name;
                    remoteTestSetTestCaseParameter.Value = parameterValue.Value;
                    remoteTestRun.Parameters.Add(remoteTestSetTestCaseParameter);
                }
                //Test Set Parameters
                foreach (TestSetParameter parameterValue in testSetParameterValues)
                {
                    if (!remoteTestRun.Parameters.Any(p => p.Name == parameterValue.Name))
                    {
                        RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter = new RemoteTestSetTestCaseParameter();
                        remoteTestSetTestCaseParameter.Name = parameterValue.Name;
                        remoteTestSetTestCaseParameter.Value = parameterValue.Value;
                        remoteTestRun.Parameters.Add(remoteTestSetTestCaseParameter);
                    }
                }

                //Test Case default values
                foreach (TestCaseParameter testCaseParameter in testCaseParameters)
                {
                    if (!String.IsNullOrEmpty(testCaseParameter.DefaultValue) && !remoteTestRun.Parameters.Any(p => p.Name == testCaseParameter.Name))
                    {
                        RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter = new RemoteTestSetTestCaseParameter();
                        remoteTestSetTestCaseParameter.Name = testCaseParameter.Name;
                        remoteTestSetTestCaseParameter.Value = testCaseParameter.DefaultValue;
                        remoteTestRun.Parameters.Add(remoteTestSetTestCaseParameter);
                    }
                }
            }
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteTestSet">The API data object</param>
        /// <param name="testSet">The internal datarow</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateTestSetData(TestSet testSet, RemoteTestSet remoteTestSet)
        {
            //Concurrency Management
            testSet.ConcurrencyDate = remoteTestSet.ConcurrencyDate;
            testSet.StartTracking();

            if (remoteTestSet.CreatorId.HasValue)
            {
                testSet.CreatorId = remoteTestSet.CreatorId.Value;
            }
            testSet.OwnerId = remoteTestSet.OwnerId;
            testSet.ReleaseId = remoteTestSet.ReleaseId;
            testSet.AutomationHostId = remoteTestSet.AutomationHostId;
            testSet.TestRunTypeId = remoteTestSet.TestRunTypeId;
            testSet.Name = remoteTestSet.Name;
            testSet.TestSetStatusId = remoteTestSet.TestSetStatusId;
            testSet.Description = remoteTestSet.Description;
            testSet.CreationDate = remoteTestSet.CreationDate;
            testSet.LastUpdateDate = remoteTestSet.LastUpdateDate;
            testSet.PlannedDate = remoteTestSet.PlannedDate;
            testSet.RecurrenceId = remoteTestSet.RecurrenceId;
            testSet.TestConfigurationSetId = remoteTestSet.TestConfigurationSetId;
			testSet.TestSetFolderId = remoteTestSet.TestSetFolderId;
			testSet.RecurrenceId = remoteTestSet.RecurrenceId;
        }

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteTestCase">The API data object</param>
		/// <param name="testCase">The internal datarow</param>
		/// <param name="projectTemplateId">The id of the template attached to the project</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		public static void UpdateTestCaseData(TestCase testCase, RemoteTestCase remoteTestCase, int projectTemplateId)
        {
            //Concurrency Management
            testCase.ConcurrencyDate = remoteTestCase.ConcurrencyDate;
            testCase.StartTracking();

            if (remoteTestCase.AuthorId.HasValue)
            {
                testCase.AuthorId = remoteTestCase.AuthorId.Value;
            }
            testCase.OwnerId = remoteTestCase.OwnerId;
            testCase.TestCasePriorityId = remoteTestCase.TestCasePriorityId;
            testCase.AutomationEngineId = remoteTestCase.AutomationEngineId;
            testCase.AutomationAttachmentId = remoteTestCase.AutomationAttachmentId;
            testCase.Name = remoteTestCase.Name;
            testCase.Description = remoteTestCase.Description;
            testCase.CreationDate = remoteTestCase.CreationDate;
            testCase.LastUpdateDate = remoteTestCase.LastUpdateDate;
            testCase.EstimatedDuration = remoteTestCase.EstimatedDuration;
            if (remoteTestCase.TestCaseTypeId.HasValue)
            {
                testCase.TestCaseTypeId = remoteTestCase.TestCaseTypeId.Value;
            }

			//If the template setting does not allow bulk edit of status ensure the original status is used during the update
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);
			if (projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
			{ 
				testCase.TestCaseStatusId = remoteTestCase.TestCaseStatusId;
			}

            testCase.TestCaseFolderId = remoteTestCase.TestCaseFolderId;
            testCase.IsSuspect = remoteTestCase.IsSuspect;
            testCase.ComponentIds = remoteTestCase.ComponentIds.ToDatabaseSerialization();
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteTestCaseFolder">The API data object</param>
        /// <param name="testCaseFolder">The internal datarow</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateTestCaseFolderData(TestCaseFolder testCaseFolder, RemoteTestCaseFolder remoteTestCaseFolder)
        {
            testCaseFolder.StartTracking();
            testCaseFolder.ParentTestCaseFolderId = remoteTestCaseFolder.ParentTestCaseFolderId;
            testCaseFolder.Name = remoteTestCaseFolder.Name;
            testCaseFolder.Description = remoteTestCaseFolder.Description;
            testCaseFolder.LastUpdateDate = remoteTestCaseFolder.LastUpdateDate;
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteTaskFolder">The API data object</param>
        /// <param name="taskFolder">The internal datarow</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateTaskFolderData(TaskFolder taskFolder, RemoteTaskFolder remoteTaskFolder)
        {
            taskFolder.StartTracking();
            taskFolder.ParentTaskFolderId = remoteTaskFolder.ParentTaskFolderId;
            taskFolder.Name = remoteTaskFolder.Name;
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteTestSetFolder">The API data object</param>
        /// <param name="testSetFolder">The internal datarow</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateTestSetFolderData(TestSetFolder testSetFolder, RemoteTestSetFolder remoteTestSetFolder)
        {
            testSetFolder.StartTracking();
            testSetFolder.ParentTestSetFolderId = remoteTestSetFolder.ParentTestSetFolderId;
            testSetFolder.Name = remoteTestSetFolder.Name;
            testSetFolder.Description = remoteTestSetFolder.Description;
            testSetFolder.LastUpdateDate = remoteTestSetFolder.LastUpdateDate;
            testSetFolder.CreationDate = remoteTestSetFolder.CreationDate;
        }


        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteTestStep">The API data object</param>
        /// <param name="testStep">The internal datarow</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateTestStepData(TestStep testStep, RemoteTestStep remoteTestStep)
        {
            //Concurrency Management
            testStep.ConcurrencyDate = remoteTestStep.ConcurrencyDate;
            testStep.StartTracking();

            testStep.TestCaseId = remoteTestStep.TestCaseId;
            testStep.Position = remoteTestStep.Position;
            testStep.Description = remoteTestStep.Description;
            testStep.ExpectedResult = remoteTestStep.ExpectedResult;
            testStep.SampleData = remoteTestStep.SampleData;
            testStep.LinkedTestCaseId = remoteTestStep.LinkedTestCaseId;
            testStep.LastUpdateDate = remoteTestStep.LastUpdateDate;
            testStep.Precondition = remoteTestStep.Precondition;
        }

		/// <summary>
		/// Updates existing test step parameters
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testStep">The test step object identified by id in the API request</param>
		/// <param name="testStepParameters">Array of parameter objects (names and values) passed in with the API call</param>
		public static void UpdateTestStepParameter(int projectId, TestStep testStep, List<RemoteTestStepParameter> remoteTestStepParameters)
		{
			//Call the business object to actually retrieve the test case parameters
			TestCaseManager testCaseManager = new TestCaseManager();

			//Next retrieve the parameters set on the linked test step for the specific test case
			List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStep.TestStepId);

			//Get the parameters of the test case itself that is a linked test step (so we can access the proper IDs)
			List<TestCaseParameter> testCaseParameters = new List<TestCaseParameter>();
			testCaseParameters = testCaseManager.RetrieveParameters(testStep.TestCaseId, true, true);

			//Loop through remote parameters
			List<TestStepParameter> testStepParametersToUpdate = new List<TestStepParameter>();
			//Update existing parameters (do not add new ones)
			foreach (TestCaseParameter testCaseParameter in testCaseParameters)
			{
				TestStepParameter testStepParameter = testStepParameterValues.FirstOrDefault(p => p.TestCaseParameterId == testCaseParameter.TestCaseParameterId);
				// if the parameter is already set check if the remote parameters update it
				if (testStepParameter != null)
				{
					//Check for a match in the remote parameters
					RemoteTestStepParameter remoteTestStepParameter = remoteTestStepParameters.FirstOrDefault(p => p.Name == testStepParameter.Name);
					//If so, update the parameter value
					if (remoteTestStepParameter != null)
					{
						testStepParameter.Value = remoteTestStepParameter.Value;
					}
					testStepParametersToUpdate.Add(testStepParameter);
				}
			}

			//Update the parameters to the linked test step
			testCaseManager.SaveParameterValues(projectId, testStep.TestStepId, testStepParametersToUpdate);
		}

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteRelease">The API data object</param>
		/// <param name="release">The internal datarow</param>
		/// <param name="projectTemplateId">The id of the template attached to the project</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		public static void UpdateReleaseData(Release release, RemoteRelease remoteRelease, int projectTemplateId)
        {
            if (remoteRelease.CreatorId.HasValue)
            {
                release.CreatorId = remoteRelease.CreatorId.Value;
            }

			//If the template setting does not allow bulk edit of status ensure the original status is used during the update
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);
			if (projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
			{
				release.ReleaseStatusId = remoteRelease.ReleaseStatusId;
			}

			release.Name = remoteRelease.Name;
            release.Description = remoteRelease.Description;
            release.VersionNumber = remoteRelease.VersionNumber;
            release.CreationDate = remoteRelease.CreationDate;
            release.LastUpdateDate = remoteRelease.LastUpdateDate;
            release.OwnerId = remoteRelease.OwnerId;
            release.IsSummary = remoteRelease.Summary; ;
            release.ReleaseTypeId = remoteRelease.ReleaseTypeId;
            release.StartDate = remoteRelease.StartDate;
            release.EndDate = remoteRelease.EndDate;
            release.ResourceCount = remoteRelease.ResourceCount;
            release.DaysNonWorking = remoteRelease.DaysNonWorking;
            release.TaskEstimatedEffort = remoteRelease.TaskEstimatedEffort;
            release.TaskActualEffort = remoteRelease.TaskActualEffort;

            //Concurrency Management
            release.ConcurrencyDate = remoteRelease.ConcurrencyDate;
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteProjectTemplate">The API data object</param>
        /// <param name="projectTemplate">The internal entity</param>
        /// <remarks>The artifact's primary key is not updated</remarks>
        public static void UpdateProjectTemplate(ProjectTemplate projectTemplate, RemoteProjectTemplate remoteProjectTemplate)
        {
            //Project Templates don't have concurrency tracking
            projectTemplate.StartTracking();
            projectTemplate.Name = remoteProjectTemplate.Name;
            projectTemplate.Description = remoteProjectTemplate.Description;
            projectTemplate.IsActive = remoteProjectTemplate.IsActive;
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteProject">The API data object</param>
        /// <param name="project">The internal entity</param>
        /// <remarks>The artifact's primary key and template id are not updated</remarks>
        public static void UpdateProject(Project project, RemoteProject remoteProject)
        {
            //Projects don't have concurrency tracking
            project.StartTracking();
            project.Name = remoteProject.Name;
            project.Description = remoteProject.Description;
            project.IsActive = remoteProject.Active;
            project.Website = remoteProject.Website;
            project.WorkingHours = remoteProject.WorkingHours;
            project.WorkingDays = remoteProject.WorkingDays;
            project.NonWorkingHours = remoteProject.NonWorkingHours;
            project.StartDate = remoteProject.StartDate;
            project.EndDate = remoteProject.EndDate;
            project.PercentComplete = remoteProject.PercentComplete;
            if (remoteProject.ProjectGroupId.HasValue)
            {
                project.ProjectGroupId = remoteProject.ProjectGroupId.Value;
            }

            //TODO: Once we support moving projects between templates, re-enable
            //project.ProjectTemplateId = remoteProject.ProjectTemplateId;
        }

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteTask">The API data object</param>
		/// <param name="task">The internal entity</param>
		/// <param name="projectTemplateId">The id of the template attached to the project</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		public static void UpdateTaskData(Task task, RemoteTask remoteTask, int projectTemplateId)
        {

			//We first need to update the concurrency information before we start tracking
			task.ConcurrencyDate = remoteTask.ConcurrencyDate;
            task.StartTracking();

			//If the template setting does not allow bulk edit of status ensure the original status is used during the update
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);
			if (projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
			{
				task.TaskStatusId = remoteTask.TaskStatusId;
			}

            if (remoteTask.CreatorId.HasValue)
            {
                task.CreatorId = remoteTask.CreatorId.Value;
            }
            task.RequirementId = remoteTask.RequirementId;
            task.ReleaseId = remoteTask.ReleaseId;
            task.OwnerId = remoteTask.OwnerId;
            if (remoteTask.TaskTypeId.HasValue)
            {
                task.TaskTypeId = remoteTask.TaskTypeId.Value;
            }
            task.TaskFolderId = remoteTask.TaskFolderId;
            task.TaskPriorityId = remoteTask.TaskPriorityId;
            task.Name = remoteTask.Name;
            task.Description = remoteTask.Description;
            task.CreationDate = remoteTask.CreationDate;
            task.LastUpdateDate = remoteTask.LastUpdateDate;
            task.StartDate = remoteTask.StartDate;
            task.EndDate = remoteTask.EndDate;
            task.EstimatedEffort = remoteTask.EstimatedEffort;
            task.ActualEffort = remoteTask.ActualEffort;
            task.RemainingEffort = remoteTask.RemainingEffort;
            task.RiskId = remoteTask.RiskId;
        }

        /// <summary>
        /// Populates the custom property dataset from the API data object
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="artifactCustomProperty">The custom property entity (reference)</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <param name="artifactType">The type of artifact</param>
        /// <param name="projectId">The current project</param>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <remarks>List of any validation messages</remarks>
        public static Dictionary<string, string> UpdateCustomPropertyData(ref ArtifactCustomProperty artifactCustomProperty, RemoteArtifact remoteArtifact, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, int projectTemplateId)
        {
            //First, create an artifact custom property entity if we need to
            CustomPropertyManager customPropertyManager = new CustomPropertyManager();
            if (artifactCustomProperty == null)
            {
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifactType, false, false);
                artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, artifactType, artifactId, customProperties);
                artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
            }
            else
            {
                artifactCustomProperty.StartTracking();
            }

            //Now iterate through and set the appropriate values on the entity
            if (remoteArtifact.CustomProperties != null)
            {
                //Loop through all the defined custom properties
                foreach (CustomProperty customProperty in artifactCustomProperty.CustomPropertyDefinitions)
                {
                    int propertyNumber = customProperty.PropertyNumber;
                    CustomProperty.CustomPropertyTypeEnum customPropertyType = (CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId;

                    //See if we have a matching entry in the API object
                    RemoteArtifactCustomProperty remoteArtifactCustomProperty = remoteArtifact.CustomProperties.FirstOrDefault(c => c.PropertyNumber == propertyNumber);
                    if (remoteArtifactCustomProperty != null)
                    {
                        switch (customPropertyType)
                        {
                            case CustomProperty.CustomPropertyTypeEnum.Boolean:
                                artifactCustomProperty.SetCustomProperty(propertyNumber, remoteArtifactCustomProperty.BooleanValue);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Date:
                                artifactCustomProperty.SetCustomProperty(propertyNumber, remoteArtifactCustomProperty.DateTimeValue);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Decimal:
                                artifactCustomProperty.SetCustomProperty(propertyNumber, remoteArtifactCustomProperty.DecimalValue);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Integer:
                                artifactCustomProperty.SetCustomProperty(propertyNumber, remoteArtifactCustomProperty.IntegerValue);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.List:
                                artifactCustomProperty.SetCustomProperty(propertyNumber, remoteArtifactCustomProperty.IntegerValue);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.MultiList:
                                artifactCustomProperty.SetCustomProperty(propertyNumber, remoteArtifactCustomProperty.IntegerListValue);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Text:
                                artifactCustomProperty.SetCustomProperty(propertyNumber, remoteArtifactCustomProperty.StringValue);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.User:
                                artifactCustomProperty.SetCustomProperty(propertyNumber, remoteArtifactCustomProperty.IntegerValue);
                                break;
                        }
                    }
                }

                //Now check that all the rules have been followed, return any
                Dictionary<string, string> validationMessages = customPropertyManager.CustomProperty_Check(artifactCustomProperty.CustomPropertyDefinitions, artifactCustomProperty);
                return validationMessages;
            }
            return null;
        }

        /// <summary>
        /// Updates the user object (including profile) with the API dat
        /// </summary>
        public static void UpdateUser(User user, RemoteUser remoteUser)
        {
            user.StartTracking();
            user.UserName = remoteUser.UserName;
            user.IsActive = remoteUser.Active;
            user.IsApproved = remoteUser.Approved;
            user.EmailAddress = remoteUser.EmailAddress;
            user.LdapDn = remoteUser.LdapDn;
            user.IsLocked = remoteUser.Locked;
            user.RssToken = remoteUser.RssToken;

            //Profile
            if (user.Profile != null)
            {
                user.Profile.StartTracking();
                user.Profile.IsAdmin = remoteUser.Admin;
                user.Profile.Department = remoteUser.Department;
                user.Profile.FirstName = remoteUser.FirstName;
                user.Profile.MiddleInitial = remoteUser.MiddleInitial;
                user.Profile.LastName = remoteUser.LastName;
            }
        }

		/// <summary>
		/// Populates the internal entity from the API data object
		/// </summary>
		/// <param name="remoteIncident">The API data object</param>
		/// <param name="incident">The internal entity</param>
		/// <param name="projectTemplateId">The id of the template attached to the project</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		public static void UpdateIncidentData(Incident incident, RemoteIncident remoteIncident, int projectTemplateId)
        {

			//We first need to update the concurrency information before we start tracking
			incident.ConcurrencyDate = remoteIncident.ConcurrencyDate;
            incident.StartTracking();

            incident.PriorityId = remoteIncident.PriorityId;
            incident.SeverityId = remoteIncident.SeverityId;

			//If the template setting does not allow bulk edit of status ensure the original status is used during the update
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);
            if (remoteIncident.IncidentStatusId.HasValue && projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
            {
                incident.IncidentStatusId = remoteIncident.IncidentStatusId.Value;
            }

            if (remoteIncident.IncidentTypeId.HasValue)
            {
                incident.IncidentTypeId = remoteIncident.IncidentTypeId.Value;
            }
            if (remoteIncident.OpenerId.HasValue)
            {
                incident.OpenerId = remoteIncident.OpenerId.Value;
            }
            incident.OwnerId = remoteIncident.OwnerId;
            incident.DetectedReleaseId = remoteIncident.DetectedReleaseId;
            incident.ResolvedReleaseId = remoteIncident.ResolvedReleaseId;
            incident.VerifiedReleaseId = remoteIncident.VerifiedReleaseId;
            incident.Name = remoteIncident.Name;
            incident.Description = remoteIncident.Description;
            if (remoteIncident.CreationDate.HasValue)
            {
                incident.CreationDate = remoteIncident.CreationDate.Value;
            }
            incident.StartDate = remoteIncident.StartDate;
            incident.ClosedDate = remoteIncident.ClosedDate;
            incident.LastUpdateDate = remoteIncident.LastUpdateDate;
            incident.EstimatedEffort = remoteIncident.EstimatedEffort;
            incident.ActualEffort = remoteIncident.ActualEffort;
            incident.RemainingEffort = remoteIncident.RemainingEffort;
            incident.BuildId = remoteIncident.FixedBuildId;
            incident.ComponentIds = remoteIncident.ComponentIds.ToDatabaseSerialization();
        }

        /// <summary>
        /// Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteAssociation">The API data object</param>
        /// <param name="artifactLink">The internal datarow</param>
        /// <remarks>Only the comment and creator fields are updated</remarks>
        public static void UpdateAssociationData(ArtifactLink artifactLink, RemoteAssociation remoteAssociation)
        {
            artifactLink.StartTracking();
            if (remoteAssociation.CreatorId.HasValue)
            {
                artifactLink.CreatorId = remoteAssociation.CreatorId.Value;
            }
            artifactLink.Comment = remoteAssociation.Comment.MaxLength(255);
            artifactLink.ArtifactLinkTypeId = remoteAssociation.ArtifactLinkTypeId;
        }

        /// <summary>
        /// Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteAutomationHost">The API data object</param>
        /// <param name="automationHost">The internal datarow</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateAutomationHostData(AutomationHost automationHost, RemoteAutomationHost remoteAutomationHost)
        {
            //We first need to update the concurrency information before we start tracking
            automationHost.ConcurrencyDate = remoteAutomationHost.ConcurrencyDate;

            automationHost.StartTracking();
            automationHost.Name = remoteAutomationHost.Name;
            automationHost.Token = remoteAutomationHost.Token;
            automationHost.Description = remoteAutomationHost.Description;
            automationHost.IsActive = remoteAutomationHost.Active;
            automationHost.LastUpdateDate = remoteAutomationHost.LastUpdateDate;
            automationHost.LastContactDate = remoteAutomationHost.LastContactDate;
        }

		/// <summary>
		/// PopulationFunctions.Populates the internal datarow from the API data object
		/// </summary>
		/// <param name="remoteRequirement">The API data object</param>
		/// <param name="requirement">The internal entity</param>
		/// <param name="projectTemplateId">The id of the template attached to the project</param>
		/// <remarks>The artifact's primary key and project id are not updated</remarks>
		public static void UpdateRequirementData(Requirement requirement, RemoteRequirement remoteRequirement, int projectTemplateId)
        {
            //We first need to update the concurrency information before we start tracking
            requirement.ConcurrencyDate = remoteRequirement.ConcurrencyDate;
            requirement.StartTracking();

			//If the template setting does not allow bulk edit of status ensure the original status is used during the update
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);

			if (remoteRequirement.StatusId.HasValue && projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
            {
                requirement.RequirementStatusId = remoteRequirement.StatusId.Value;
            }
            if (remoteRequirement.AuthorId.HasValue)
            {
                requirement.AuthorId = remoteRequirement.AuthorId.Value;
            }
            requirement.OwnerId = remoteRequirement.OwnerId;
            if (remoteRequirement.RequirementTypeId.HasValue)
            {
                requirement.RequirementTypeId = remoteRequirement.RequirementTypeId.Value;
            }
            requirement.ImportanceId = remoteRequirement.ImportanceId;
            requirement.ReleaseId = remoteRequirement.ReleaseId;
            requirement.ComponentId = remoteRequirement.ComponentId;
            requirement.Name = remoteRequirement.Name;
            requirement.Description = remoteRequirement.Description;
            requirement.CreationDate = remoteRequirement.CreationDate;
            requirement.LastUpdateDate = remoteRequirement.LastUpdateDate;
            requirement.IsSummary = remoteRequirement.Summary;
            requirement.EstimatePoints = remoteRequirement.EstimatePoints;
        }

		/// <summary>
		/// Updates the risk entity from the API data object
		/// </summary>
		/// <param name="risk">The entity to be updated</param>
		/// <param name="remoteRisk">The API object</param>
		/// <param name="projectTemplateId">The id of the template attached to the project</param>
		internal static void UpdateRiskData(Risk risk, RemoteRisk remoteRisk, int projectTemplateId)
		{
			//We first need to update the concurrency information before we start tracking
			risk.ConcurrencyDate = remoteRisk.ConcurrencyDate;
			risk.StartTracking();

			risk.ClosedDate = remoteRisk.ClosedDate;
			risk.ComponentId = remoteRisk.ComponentId;
			risk.CreationDate = remoteRisk.CreationDate;
			risk.CreatorId = remoteRisk.CreatorId;
			risk.Description = remoteRisk.Description;
			risk.LastUpdateDate = remoteRisk.LastUpdateDate;
			risk.Name = remoteRisk.Name;
			risk.OwnerId = remoteRisk.OwnerId;
			risk.ReleaseId = remoteRisk.ReleaseId;
			risk.ReviewDate = remoteRisk.ReviewDate;
			risk.RiskImpactId = remoteRisk.RiskImpactId;
			risk.RiskProbabilityId = remoteRisk.RiskProbabilityId;

			//If the template setting does not allow bulk edit of status ensure the original status is used during the update
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId);
			if (remoteRisk.RiskStatusId.HasValue && projectTemplateSettings.Workflow_BulkEditCanChangeStatus)
			{
				risk.RiskStatusId = remoteRisk.RiskStatusId.Value;
			}

			if (remoteRisk.RiskTypeId.HasValue)
			{
				risk.RiskTypeId = remoteRisk.RiskTypeId.Value;
			}
		}
	}
}
