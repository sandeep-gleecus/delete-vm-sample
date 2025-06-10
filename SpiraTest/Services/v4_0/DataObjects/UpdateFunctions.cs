using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Helper class that populates the internal data classes from the RemoteObjects
    /// </summary>
    public static class UpdateFunctions
    {
        /// <summary>
        /// Converts a requirement static ID to be template-based
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="remoteRequirement">The requirement object</param>
        public static void ConvertLegacyStaticIds(int projectTemplateId, RemoteRequirement remoteRequirement)
        {
            //First importance
            RequirementManager requirementManager = new RequirementManager();
            List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
            Importance importance = importances.FirstOrDefault(p => p.Score == remoteRequirement.ImportanceId || p.ImportanceId == remoteRequirement.ImportanceId);
            if (importance == null)
            {
                //No match, so leave blank
                remoteRequirement.ImportanceId = null;
            }
            else
            {
                remoteRequirement.ImportanceId = importance.ImportanceId;
            }
        }

        /// <summary>
        /// Converts a task static ID to be template-based
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="remoteTask">The task object</param>
        public static void ConvertLegacyStaticIds(int projectTemplateId, RemoteTask remoteTask)
        {
            //First priority
            TaskManager taskManager = new TaskManager();
            List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
            TaskPriority priority = priorities.FirstOrDefault(p => p.Score == remoteTask.TaskPriorityId || p.TaskPriorityId == remoteTask.TaskPriorityId);
            if (priority == null)
            {
                //No match, so leave blank
                remoteTask.TaskPriorityId = null;
            }
            else
            {
                remoteTask.TaskPriorityId = priority.TaskPriorityId;
            }
        }

        /// <summary>
        /// Converts a task static ID to be template-based
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="remoteTestCase">The task object</param>
        public static void ConvertLegacyStaticIds(int projectTemplateId, RemoteTestCase remoteTestCase)
        {
            //First priority
            TestCaseManager taskManager = new TestCaseManager();
            List<TestCasePriority> priorities = taskManager.TestCasePriority_Retrieve(projectTemplateId);
            TestCasePriority priority = priorities.FirstOrDefault(p => p.Score == remoteTestCase.TestCasePriorityId || p.TestCasePriorityId == remoteTestCase.TestCasePriorityId);
            if (priority == null)
            {
                //No match, so leave blank
                remoteTestCase.TestCasePriorityId = null;
            }
            else
            {
                remoteTestCase.TestCasePriorityId = priority.TestCasePriorityId;
            }
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
                    testRun.TestRunSteps.Add(testRunStep);
                }
            }

            //Concurrency management
            testRun.ConcurrencyDate = remoteTestRun.ConcurrencyDate;
        }

        /// <summary>
        /// Adds the test set custom property values to the automated test run
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="projectTemplateId">The id of the project template</param>
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
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="remoteRelease">The API data object</param>
        /// <param name="release">The internal datarow</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateReleaseData(Release release, RemoteRelease remoteRelease)
        {
            if (remoteRelease.CreatorId.HasValue)
            {
                release.CreatorId = remoteRelease.CreatorId.Value;
            }
            release.Name = remoteRelease.Name;
            release.Description = remoteRelease.Description;
            release.VersionNumber = remoteRelease.VersionNumber;
            release.CreationDate = remoteRelease.CreationDate;
            release.LastUpdateDate = remoteRelease.LastUpdateDate;
            release.IsSummary = remoteRelease.Summary;
            release.ReleaseStatusId = remoteRelease.Active ? (int)Release.ReleaseStatusEnum.InProgress : (int)Release.ReleaseStatusEnum.Completed;
            release.ReleaseTypeId = remoteRelease.Iteration ? (int)Release.ReleaseTypeEnum.Iteration : (int)Release.ReleaseTypeEnum.MajorRelease;
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
        /// <param name="projectTemplateId">The ID of the project template</param>
        /// <param name="remoteTask">The API data object</param>
        /// <param name="task">The internal entity</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateTaskData(int projectTemplateId, Task task, RemoteTask remoteTask)
        {
            //See if we have any IDs that used to be static and that are now template/project based
            //If so, we need to convert them
            if (remoteTask.TaskPriorityId >= 1 && remoteTask.TaskPriorityId <= 4)
            {
                UpdateFunctions.ConvertLegacyStaticIds(projectTemplateId, remoteTask);
            }

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
        }

        /// <summary>
        /// Populates the custom property dataset from the API data object
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="artifactCustomProperty">The custom property entity (reference)</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <param name="artifactType">The type of artifact</param>
        /// <param name="projectId">The current project</param>
        /// <param name="projectTemplateId">the id of the project template</param>
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
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="remoteRequirement">The API data object</param>
        /// <param name="requirement">The internal entity</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateRequirementData(int projectTemplateId, Requirement requirement, RemoteRequirement remoteRequirement)
        {
            //See if we have any IDs that used to be static and that are now template/project based
            //If so, we need to convert them
            if (remoteRequirement.ImportanceId >= 1 && remoteRequirement.ImportanceId <= 4)
            {
                UpdateFunctions.ConvertLegacyStaticIds(projectTemplateId, remoteRequirement);
            }

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
            requirement.ImportanceId = remoteRequirement.ImportanceId;
            requirement.ReleaseId = remoteRequirement.ReleaseId;
            requirement.Name = remoteRequirement.Name;
            requirement.Description = remoteRequirement.Description;
            requirement.CreationDate = remoteRequirement.CreationDate;
            requirement.LastUpdateDate = remoteRequirement.LastUpdateDate;
            requirement.IsSummary = remoteRequirement.Summary;
        }

        /// <summary>
        /// PopulationFunctions.Populates the internal datarow from the API data object
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCase">The internal datarow</param>
        /// <remarks>The artifact's primary key and project id are not updated</remarks>
        public static void UpdateTestCaseData(int projectTemplateId, TestCase testCase, RemoteTestCase remoteTestCase)
        {
            //See if we have any IDs that used to be static and that are now template/project based
            //If so, we need to convert them
            if (remoteTestCase.TestCasePriorityId >= 1 && remoteTestCase.TestCasePriorityId <= 4)
            {
                UpdateFunctions.ConvertLegacyStaticIds(projectTemplateId, remoteTestCase);
            }

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
        }
    }
}
