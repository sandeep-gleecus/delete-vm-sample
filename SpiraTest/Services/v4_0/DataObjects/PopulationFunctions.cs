using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Collections;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Helper class that populates the RemoteObjects from internal data classes
    /// </summary>
    public static class PopulationFunctions
    {
        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteDataSyncSystem">The API data object</param>
        /// <param name="dataSyncSystem">The internal datarow</param>
        public static void PopulateDataSyncSystem(RemoteDataSyncSystem remoteDataSyncSystem, DataSyncSystem dataSyncSystem)
        {
            remoteDataSyncSystem.DataSyncSystemId = dataSyncSystem.DataSyncSystemId;
            remoteDataSyncSystem.DataSyncStatusId = dataSyncSystem.DataSyncStatusId;
            remoteDataSyncSystem.Name = dataSyncSystem.Name;
            remoteDataSyncSystem.Description = dataSyncSystem.Description;
            remoteDataSyncSystem.ConnectionString = dataSyncSystem.ConnectionString;
            remoteDataSyncSystem.Login = dataSyncSystem.ExternalLogin;
            remoteDataSyncSystem.Password = dataSyncSystem.ExternalPassword;
            remoteDataSyncSystem.TimeOffsetHours = dataSyncSystem.TimeOffsetHours;
            remoteDataSyncSystem.LastSyncDate = dataSyncSystem.LastSyncDate;
            remoteDataSyncSystem.Custom01 = dataSyncSystem.Custom01;
            remoteDataSyncSystem.Custom02 = dataSyncSystem.Custom02;
            remoteDataSyncSystem.Custom03 = dataSyncSystem.Custom03;
            remoteDataSyncSystem.Custom04 = dataSyncSystem.Custom04;
            remoteDataSyncSystem.Custom05 = dataSyncSystem.Custom05;
            remoteDataSyncSystem.AutoMapUsers = (dataSyncSystem.AutoMapUsersYn == "Y");
            remoteDataSyncSystem.DataSyncStatusName = dataSyncSystem.DataSyncStatusName;
        }

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the current project</param>
		/// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
		public static void PopulateManualTestRun(RemoteManualTestRun remoteTestRun, TestRun testRun, int projectId)
		{
			//First populate the parts that are common to manual and automated test runs
			PopulateTestRun(remoteTestRun, testRun, projectId);

			//Next any manual test run steps
            if (testRun.TestRunSteps != null && testRun.TestRunSteps.Count > 0)
			{
				remoteTestRun.TestRunSteps = new List<RemoteTestRunStep>();
                foreach (TestRunStep testRunStep in testRun.TestRunSteps)
				{
					RemoteTestRunStep remoteTestRunStep = new RemoteTestRunStep();
					remoteTestRun.TestRunSteps.Add(remoteTestRunStep);
					//Populate the item
					remoteTestRunStep.TestRunStepId = testRunStep.TestRunStepId;
					remoteTestRunStep.TestRunId = testRunStep.TestRunId;
					remoteTestRunStep.TestStepId = testRunStep.TestStepId;
					remoteTestRunStep.TestCaseId = testRunStep.TestCaseId;
					remoteTestRunStep.ExecutionStatusId = testRunStep.ExecutionStatusId;
					remoteTestRunStep.Position = testRunStep.Position;
					remoteTestRunStep.Description = testRunStep.Description;
					remoteTestRunStep.ExpectedResult = testRunStep.ExpectedResult;
					remoteTestRunStep.SampleData = testRunStep.SampleData;
					remoteTestRunStep.ActualResult = testRunStep.ActualResult;
				}
			}
		}

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateAutomatedTestRun(RemoteAutomatedTestRun remoteTestRun, TestRun testRun, int projectId)
        {
            //First populate the parts that are common to manual and automated test runs
            PopulateTestRun(remoteTestRun, testRun, projectId);

            //Now the automation-specific info
            remoteTestRun.RunnerName = testRun.RunnerName;
            remoteTestRun.RunnerTestName = testRun.RunnerTestName;
            remoteTestRun.RunnerAssertCount = testRun.RunnerAssertCount;
            remoteTestRun.RunnerMessage = testRun.RunnerMessage;
            remoteTestRun.RunnerStackTrace = testRun.RunnerStackTrace;
            remoteTestRun.AutomationHostId = testRun.AutomationHostId;
            remoteTestRun.AutomationEngineId = testRun.AutomationEngineId;
            remoteTestRun.TestRunFormatId = (testRun.TestRunFormatId.HasValue) ? testRun.TestRunFormatId.Value : (int)TestRun.TestRunFormatEnum.PlainText;
            remoteTestRun.AutomationAttachmentId = null;

            //Next any automated test run steps
            if (testRun.TestRunSteps != null && testRun.TestRunSteps.Count > 0)
            {
                remoteTestRun.TestRunSteps = new List<RemoteTestRunStep>();
                foreach (TestRunStep testRunStep in testRun.TestRunSteps)
                {
                    RemoteTestRunStep remoteTestRunStep = new RemoteTestRunStep();
                    remoteTestRun.TestRunSteps.Add(remoteTestRunStep);
                    //Populate the item
                    remoteTestRunStep.TestRunStepId = testRunStep.TestRunStepId;
                    remoteTestRunStep.TestRunId = testRunStep.TestRunId;
                    remoteTestRunStep.TestStepId = testRunStep.TestStepId;
                    remoteTestRunStep.ExecutionStatusId = testRunStep.ExecutionStatusId;
                    remoteTestRunStep.Position = testRunStep.Position;
                    remoteTestRunStep.Description = testRunStep.Description;
                    remoteTestRunStep.ExpectedResult = testRunStep.ExpectedResult;
                    remoteTestRunStep.SampleData = testRunStep.SampleData;
                    remoteTestRunStep.ActualResult = testRunStep.ActualResult;
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateAutomatedTestRun(RemoteAutomatedTestRun remoteTestRun, TestRunView testRun, int projectId)
        {
            //First populate the parts that are common to manual and automated test runs
            PopulateTestRun(remoteTestRun, testRun, projectId);

            //Now the automation-specific info
            remoteTestRun.RunnerName = testRun.RunnerName;
            remoteTestRun.RunnerTestName = testRun.RunnerTestName;
            remoteTestRun.RunnerAssertCount = testRun.RunnerAssertCount;
            remoteTestRun.RunnerMessage = testRun.RunnerMessage;
            remoteTestRun.RunnerStackTrace = testRun.RunnerStackTrace;
            remoteTestRun.AutomationHostId = testRun.AutomationHostId;
            remoteTestRun.AutomationEngineId = testRun.AutomationEngineId;
            remoteTestRun.TestRunFormatId = (testRun.TestRunFormatId.HasValue) ? testRun.TestRunFormatId.Value : (int)TestRun.TestRunFormatEnum.PlainText;
            remoteTestRun.AutomationAttachmentId = null;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the currernt project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateTestRun(RemoteTestRun remoteTestRun, TestRun testRun, int projectId)
        {
            //Artifact Fields
            remoteTestRun.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestRun;

            //Populate the parts that are common to manual and automated test runs
            remoteTestRun.TestRunId = testRun.TestRunId;
            remoteTestRun.ProjectId = projectId;
            remoteTestRun.Name = testRun.Name;
            remoteTestRun.TestCaseId = testRun.TestCaseId;
            remoteTestRun.TestRunTypeId = testRun.TestRunTypeId;
            remoteTestRun.TesterId = testRun.TesterId;
            remoteTestRun.ExecutionStatusId = testRun.ExecutionStatusId;
            remoteTestRun.ReleaseId = testRun.ReleaseId;
            remoteTestRun.TestSetId = testRun.TestSetId;
            remoteTestRun.TestSetTestCaseId = testRun.TestSetTestCaseId;
            remoteTestRun.StartDate = testRun.StartDate;
            remoteTestRun.EndDate = testRun.EndDate;
            remoteTestRun.BuildId = testRun.BuildId;
            remoteTestRun.EstimatedDuration = testRun.EstimatedDuration;
            remoteTestRun.ActualDuration = testRun.ActualDuration;

            //Concurrency management
            remoteTestRun.ConcurrencyDate = testRun.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the currernt project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateTestRun(RemoteTestRun remoteTestRun, TestRunView testRun, int projectId)
        {
            //Artifact Fields
            remoteTestRun.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestRun;

            //Populate the parts that are common to manual and automated test runs
            remoteTestRun.TestRunId = testRun.TestRunId;
            remoteTestRun.ProjectId = projectId;
            remoteTestRun.Name = testRun.Name;
            remoteTestRun.TestCaseId = testRun.TestCaseId;
            remoteTestRun.TestRunTypeId = testRun.TestRunTypeId;
            remoteTestRun.TesterId = testRun.TesterId;
            remoteTestRun.ExecutionStatusId = testRun.ExecutionStatusId;
            remoteTestRun.ReleaseId = testRun.ReleaseId;
            remoteTestRun.TestSetId = testRun.TestSetId;
            remoteTestRun.TestSetTestCaseId = testRun.TestSetTestCaseId;
            remoteTestRun.StartDate = testRun.StartDate;
            remoteTestRun.EndDate = testRun.EndDate;
            remoteTestRun.BuildId = testRun.BuildId;
            remoteTestRun.EstimatedDuration = testRun.EstimatedDuration;
            remoteTestRun.ActualDuration = testRun.ActualDuration;

            //Concurrency management
            remoteTestRun.ConcurrencyDate = testRun.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteAutomationEngine">The API data object</param>
        /// <param name="automationEngine">The internal datarow</param>
        public static void PopulateAutomationEngine(RemoteAutomationEngine remoteAutomationEngine, AutomationEngine automationEngine)
        {
            remoteAutomationEngine.AutomationEngineId = automationEngine.AutomationEngineId;
            remoteAutomationEngine.Name = automationEngine.Name;
            remoteAutomationEngine.Description = automationEngine.Description;
            remoteAutomationEngine.Token = automationEngine.Token;
            remoteAutomationEngine.Active = automationEngine.IsActive;
        }

        /// <summary>
        /// Populates an API object from the internal entity
        /// </summary>
        /// <param name="remoteBuild">The API data object</param>
        /// <param name="build">The internal entity</param>
        public static void PopulateBuild(RemoteBuild remoteBuild, Build build)
        {
            remoteBuild.BuildId = build.BuildId;
            remoteBuild.BuildStatusId = build.BuildStatusId;
            remoteBuild.ProjectId = build.ProjectId;
            remoteBuild.ReleaseId = build.ReleaseId;
            remoteBuild.Name = build.Name;
            remoteBuild.Description = build.Description;
            remoteBuild.LastUpdateDate = build.LastUpdateDate;
            remoteBuild.CreationDate = build.CreationDate;
            remoteBuild.BuildStatusName = build.BuildStatusName;
        }

        /// <summary>
        /// Converts the API sort object into an internal sort expression
        /// </summary>
        /// <param name="remoteSort">The API sort object</param>
        /// <returns>The sort expression</returns>
        public static string PopulateSort(RemoteSort remoteSort)
        {
            return remoteSort.PropertyName + " " + ((remoteSort.SortAscending) ? "ASC" : "DESC");
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTask">The API data object</param>
        /// <param name="task">The internal entity</param>
        public static void PopulateTask(RemoteTask remoteTask, TaskView task)
        {
            //Artifact Fields
            remoteTask.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Task;

            remoteTask.TaskId = task.TaskId;
            remoteTask.ProjectId = task.ProjectId;
            remoteTask.TaskStatusId = task.TaskStatusId;
            remoteTask.RequirementId = task.RequirementId;
            remoteTask.ReleaseId = task.ReleaseId;
            remoteTask.CreatorId = task.CreatorId;
            remoteTask.OwnerId = task.OwnerId;
            remoteTask.TaskPriorityId = task.TaskPriorityId;
            remoteTask.Name = task.Name;
            remoteTask.Description = task.Description;
            remoteTask.CreationDate = task.CreationDate;
            remoteTask.LastUpdateDate = task.LastUpdateDate;
            remoteTask.StartDate = task.StartDate;
            remoteTask.EndDate = task.EndDate;
            remoteTask.CompletionPercent = task.CompletionPercent;
            remoteTask.EstimatedEffort = task.EstimatedEffort;
            remoteTask.ActualEffort = task.ActualEffort;
            remoteTask.RemainingEffort = task.RemainingEffort;
            remoteTask.ProjectedEffort = task.ProjectedEffort;
            remoteTask.TaskStatusName = task.TaskStatusName;
            remoteTask.OwnerName = task.OwnerName;
            remoteTask.TaskPriorityName = task.TaskPriorityName;
            remoteTask.ProjectName = task.ProjectName;
            remoteTask.ReleaseVersionNumber = task.ReleaseVersionNumber;
            remoteTask.RequirementName = task.RequirementName;

            //Concurrency Management
            remoteTask.ConcurrencyDate = task.ConcurrencyDate;
        }

        /// <summary>
        /// Converts the API filter object into the kinds that can be consumed internally
        /// </summary>
        /// <param name="remoteFilters">The list of API filters</param>
        /// <returns>The populated Hashtable of internal filters</returns>
        public static Hashtable PopulateFilters(List<RemoteFilter> remoteFilters)
        {
            //Handle the null case
            if (remoteFilters == null)
            {
                return null;
            }
            Hashtable filters = new Hashtable();
            foreach (RemoteFilter remoteFilter in remoteFilters)
            {
                if (!String.IsNullOrEmpty(remoteFilter.PropertyName))
                {
                    //See what type we have and populate accordingly
                    if (remoteFilter.IntValue.HasValue)
                    {
                        filters.Add(remoteFilter.PropertyName, remoteFilter.IntValue.Value);
                    }
                    else if (!String.IsNullOrEmpty(remoteFilter.StringValue))
                    {
                        filters.Add(remoteFilter.PropertyName, remoteFilter.StringValue);
                    }
                    else if (remoteFilter.MultiValue != null)
                    {
                        filters.Add(remoteFilter.PropertyName, remoteFilter.MultiValue.Internal);
                    }
                    else if (remoteFilter.DateRangeValue != null)
                    {
                        filters.Add(remoteFilter.PropertyName, remoteFilter.DateRangeValue.Internal);
                    }

                }
            }
            return filters;
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from the internal artifact custom property entity
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="artifactCustomProperty">The internal custom property entity</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, ArtifactCustomProperty artifactCustomProperty)
        {
            //Make sure we have a custom propery record and definitions
            if (artifactCustomProperty != null && artifactCustomProperty.CustomPropertyDefinitions != null && remoteArtifact.ProjectId.HasValue)
            {
                //See if we have an existing API custom property collection, otherwise create one
                if (remoteArtifact.CustomProperties == null)
                {
                    remoteArtifact.CustomProperties = new List<RemoteArtifactCustomProperty>();
                }

                //Loop through all the defined custom properties
                foreach (CustomProperty customProperty in artifactCustomProperty.CustomPropertyDefinitions)
                {
                    //Get the template associated with the project
                    int projectTemplateId = new TemplateManager().RetrieveForProject(remoteArtifact.ProjectId.Value).ProjectTemplateId;

                    //Make sure the projects match (we will have multiple project definitions in some of the retrieves)
                    if (remoteArtifact.ProjectId.HasValue && customProperty.ProjectTemplateId == projectTemplateId)
                    {
                        int propertyNumber = customProperty.PropertyNumber;
                        CustomProperty.CustomPropertyTypeEnum customPropertyType = (CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId;

                        //See if we have an existing API custom property entry, otherwise create one
                        RemoteArtifactCustomProperty remoteArtifactCustomProperty = remoteArtifact.CustomProperties.FirstOrDefault(p => p.PropertyNumber == propertyNumber);
                        if (remoteArtifactCustomProperty == null)
                        {
                            remoteArtifactCustomProperty = new RemoteArtifactCustomProperty();
                            remoteArtifactCustomProperty.PropertyNumber = propertyNumber;
                            remoteArtifactCustomProperty.Definition = new RemoteCustomProperty(remoteArtifact.ProjectId.Value, customProperty);
                            remoteArtifact.CustomProperties.Add(remoteArtifactCustomProperty);
                        }

                        //Now we need to populate the appropriate value
                        switch (customPropertyType)
                        {
                            case CustomProperty.CustomPropertyTypeEnum.Boolean:
                                remoteArtifactCustomProperty.BooleanValue = (bool?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Date:
                                remoteArtifactCustomProperty.DateTimeValue = (DateTime?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Decimal:
                                remoteArtifactCustomProperty.DecimalValue = (decimal?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Integer:
                                remoteArtifactCustomProperty.IntegerValue = (int?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.List:
                                remoteArtifactCustomProperty.IntegerValue = (int?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.MultiList:
                                remoteArtifactCustomProperty.IntegerListValue = (List<int>)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Text:
                                remoteArtifactCustomProperty.StringValue = (string)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.User:
                                remoteArtifactCustomProperty.IntegerValue = (int?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from the internal datarow
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="dataRow">The internal datarow</param>
        /// <param name="customProperties">The custom property definitions for this artifact type</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, DataRow dataRow, List<CustomProperty> customProperties)
        {
            //Make sure we have a custom propery definitions
            if (customProperties != null && remoteArtifact.ProjectId.HasValue)
            {
                //See if we have an existing API custom property collection, otherwise create one
                if (remoteArtifact.CustomProperties == null)
                {
                    remoteArtifact.CustomProperties = new List<RemoteArtifactCustomProperty>();
                }

                //Loop through all the defined custom properties
                foreach (CustomProperty customProperty in customProperties)
                {
                    //Get the template associated with the project
                    int projectTemplateId = new TemplateManager().RetrieveForProject(remoteArtifact.ProjectId.Value).ProjectTemplateId;

                    //Make sure the projects match (we will have multiple project definitions in some of the retrieves)
                    if (remoteArtifact.ProjectId.HasValue && customProperty.ProjectTemplateId == projectTemplateId)
                    {
                        int propertyNumber = customProperty.PropertyNumber;
                        CustomProperty.CustomPropertyTypeEnum customPropertyType = (CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId;

                        //See if we have an existing API custom property entry, otherwise create one
                        RemoteArtifactCustomProperty remoteArtifactCustomProperty = remoteArtifact.CustomProperties.FirstOrDefault(p => p.PropertyNumber == propertyNumber);
                        if (remoteArtifactCustomProperty == null)
                        {
                            remoteArtifactCustomProperty = new RemoteArtifactCustomProperty();
                            remoteArtifactCustomProperty.PropertyNumber = propertyNumber;
                            remoteArtifactCustomProperty.Definition = new RemoteCustomProperty(remoteArtifact.ProjectId.Value, customProperty);
                            remoteArtifact.CustomProperties.Add(remoteArtifactCustomProperty);
                        }

                        //Make sure the datarow has the custom property column defined
                        //Now we need to populate the appropriate value
                        if (dataRow.Table.Columns[customProperty.CustomPropertyFieldName] != null)
                        {
                            string serializedValue;
                            if (dataRow[customProperty.CustomPropertyFieldName] == DBNull.Value)
                            {
                                serializedValue = null;
                            }
                            else
                            {
                                serializedValue = (string)dataRow[customProperty.CustomPropertyFieldName];
                            }

                            switch (customPropertyType)
                            {
                                case CustomProperty.CustomPropertyTypeEnum.Boolean:
                                    remoteArtifactCustomProperty.BooleanValue = serializedValue.FromDatabaseSerialization_Boolean();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Date:
                                    remoteArtifactCustomProperty.DateTimeValue = serializedValue.FromDatabaseSerialization_DateTime();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Decimal:
                                    remoteArtifactCustomProperty.DecimalValue = serializedValue.FromDatabaseSerialization_Decimal();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Integer:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.List:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.MultiList:
                                    remoteArtifactCustomProperty.IntegerListValue = serializedValue.FromDatabaseSerialization_List_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Text:
                                    remoteArtifactCustomProperty.StringValue = serializedValue.FromDatabaseSerialization_String();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.User:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from the internal artifact entity
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="artifact">The internal artifact</param>
        /// <param name="customProperties">The custom property definitions for this artifact type</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, Artifact artifact, List<CustomProperty> customProperties)
        {
            //Make sure we have a custom propery definitions
            if (customProperties != null && remoteArtifact.ProjectId.HasValue)
            {
                //See if we have an existing API custom property collection, otherwise create one
                if (remoteArtifact.CustomProperties == null)
                {
                    remoteArtifact.CustomProperties = new List<RemoteArtifactCustomProperty>();
                }

                //Loop through all the defined custom properties
                foreach (CustomProperty customProperty in customProperties)
                {
                    //Get the template associated with the project
                    int projectTemplateId = new TemplateManager().RetrieveForProject(remoteArtifact.ProjectId.Value).ProjectTemplateId;

                    //Make sure the projects match (we will have multiple project definitions in some of the retrieves)
                    if (remoteArtifact.ProjectId.HasValue && customProperty.ProjectTemplateId == projectTemplateId)
                    {
                        int propertyNumber = customProperty.PropertyNumber;
                        CustomProperty.CustomPropertyTypeEnum customPropertyType = (CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId;

                        //See if we have an existing API custom property entry, otherwise create one
                        RemoteArtifactCustomProperty remoteArtifactCustomProperty = remoteArtifact.CustomProperties.FirstOrDefault(p => p.PropertyNumber == propertyNumber);
                        if (remoteArtifactCustomProperty == null)
                        {
                            remoteArtifactCustomProperty = new RemoteArtifactCustomProperty();
                            remoteArtifactCustomProperty.PropertyNumber = propertyNumber;
                            remoteArtifactCustomProperty.Definition = new RemoteCustomProperty(remoteArtifact.ProjectId.Value, customProperty);
                            remoteArtifact.CustomProperties.Add(remoteArtifactCustomProperty);
                        }

                        //Make sure the datarow has the custom property column defined
                        //Now we need to populate the appropriate value
                        if (artifact.ContainsProperty(customProperty.CustomPropertyFieldName))
                        {
                            string serializedValue;
                            if (artifact[customProperty.CustomPropertyFieldName] == null)
                            {
                                serializedValue = null;
                            }
                            else
                            {
                                serializedValue = (string)artifact[customProperty.CustomPropertyFieldName];
                            }

                            switch (customPropertyType)
                            {
                                case CustomProperty.CustomPropertyTypeEnum.Boolean:
                                    remoteArtifactCustomProperty.BooleanValue = serializedValue.FromDatabaseSerialization_Boolean();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Date:
                                    remoteArtifactCustomProperty.DateTimeValue = serializedValue.FromDatabaseSerialization_DateTime();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Decimal:
                                    remoteArtifactCustomProperty.DecimalValue = serializedValue.FromDatabaseSerialization_Decimal();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Integer:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.List:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.MultiList:
                                    remoteArtifactCustomProperty.IntegerListValue = serializedValue.FromDatabaseSerialization_List_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Text:
                                    remoteArtifactCustomProperty.StringValue = serializedValue.FromDatabaseSerialization_String();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.User:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteIncident">The API data object</param>
        /// <param name="incident">The internal entity</param>
        public static void PopulateIncident(RemoteIncident remoteIncident, IncidentView incident, int? testRunStepId = null)
        {
            //Artifact Fields
            remoteIncident.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Incident;

            remoteIncident.IncidentId = incident.IncidentId;
            remoteIncident.ProjectId = incident.ProjectId;
            remoteIncident.PriorityId = incident.PriorityId;
            remoteIncident.SeverityId = incident.SeverityId;
            remoteIncident.IncidentStatusId = incident.IncidentStatusId;
            remoteIncident.IncidentTypeId = incident.IncidentTypeId;
            remoteIncident.OpenerId = incident.OpenerId;
            remoteIncident.OwnerId = incident.OwnerId;
            remoteIncident.TestRunStepId = testRunStepId;
            remoteIncident.DetectedReleaseId = incident.DetectedReleaseId;
            remoteIncident.ResolvedReleaseId = incident.ResolvedReleaseId;
            remoteIncident.VerifiedReleaseId = incident.VerifiedReleaseId;
            remoteIncident.Name = incident.Name;
            remoteIncident.Description = incident.Description;
            remoteIncident.CreationDate = incident.CreationDate;
            remoteIncident.StartDate = incident.StartDate;
            remoteIncident.ClosedDate = incident.ClosedDate;
            remoteIncident.CompletionPercent = incident.CompletionPercent;
            remoteIncident.EstimatedEffort = incident.EstimatedEffort;
            remoteIncident.ActualEffort = incident.ActualEffort;
            remoteIncident.ProjectedEffort = incident.ProjectedEffort;
            remoteIncident.RemainingEffort = incident.RemainingEffort;
            remoteIncident.LastUpdateDate = incident.LastUpdateDate;
            remoteIncident.PriorityName = incident.PriorityName;
            remoteIncident.SeverityName = incident.SeverityName;
            remoteIncident.IncidentStatusName = incident.IncidentStatusName;
            remoteIncident.IncidentTypeName = incident.IncidentTypeName;
            remoteIncident.OpenerName = incident.OpenerName;
            remoteIncident.OwnerName = incident.OwnerName;
            remoteIncident.ProjectName = incident.ProjectName;
            remoteIncident.DetectedReleaseVersionNumber = incident.DetectedReleaseVersionNumber;
            remoteIncident.ResolvedReleaseVersionNumber = incident.ResolvedReleaseVersionNumber;
            remoteIncident.VerifiedReleaseVersionNumber = incident.VerifiedReleaseVersionNumber;
            remoteIncident.IncidentStatusOpenStatus = incident.IncidentStatusIsOpenStatus;
            remoteIncident.FixedBuildId = incident.BuildId;
            remoteIncident.FixedBuildName = incident.BuildName;

            //Concurrency Management
            remoteIncident.ConcurrencyDate = incident.ConcurrencyDate;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocument">The API data object</param>
        /// <param name="projectAttachmentView">The internal datarow</param>
        public static void PopulateDocument(RemoteDocument remoteDocument, ProjectAttachmentView projectAttachmentView)
        {
            remoteDocument.AttachmentId = projectAttachmentView.AttachmentId;
            remoteDocument.AttachmentTypeId = projectAttachmentView.AttachmentTypeId;
            remoteDocument.AuthorId = projectAttachmentView.AuthorId;
            remoteDocument.EditorId = projectAttachmentView.EditorId;
            remoteDocument.FilenameOrUrl = projectAttachmentView.Filename;
            remoteDocument.Description = projectAttachmentView.Description;
            remoteDocument.UploadDate = projectAttachmentView.UploadDate;
            remoteDocument.EditedDate = projectAttachmentView.EditedDate;
            remoteDocument.Size = projectAttachmentView.Size;
            remoteDocument.CurrentVersion = projectAttachmentView.CurrentVersion;
            remoteDocument.Tags = projectAttachmentView.Tags;
            remoteDocument.AttachmentTypeName = projectAttachmentView.AttachmentTypeName;
            remoteDocument.ProjectAttachmentFolderId = projectAttachmentView.ProjectAttachmentFolderId;
            remoteDocument.ProjectAttachmentTypeId = projectAttachmentView.DocumentTypeId;
            remoteDocument.ProjectAttachmentTypeName = projectAttachmentView.DocumentTypeName;
            remoteDocument.AuthorName = projectAttachmentView.AuthorName;
            remoteDocument.EditorName = projectAttachmentView.EditorName;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentType">The API data object</param>
        /// <param name="dataRow">The internal data row</param>
        public static void PopulateDocumentType(int projectId, RemoteDocumentType remoteDocumentType, DocumentType documentType)
        {
            remoteDocumentType.ProjectAttachmentTypeId = documentType.DocumentTypeId;
            remoteDocumentType.ProjectId = projectId;
            remoteDocumentType.Name = documentType.Name;
            remoteDocumentType.Description = documentType.Description;
            remoteDocumentType.Active = documentType.IsActive;
            remoteDocumentType.Default = documentType.IsDefault;
        }

        /// <summary>
        /// Populates a project user API object from the internal datarow
        /// </summary>
        /// <param name="remoteProjectUser">The API data object</param>
        /// <param name="projectUserRow">The internal datarow</param>
        public static void PopulateProjectUser(RemoteProjectUser remoteProjectUser, ProjectUser projectUserRow)
        {
            remoteProjectUser.UserId = projectUserRow.UserId;
            remoteProjectUser.ProjectId = projectUserRow.ProjectId;
            remoteProjectUser.ProjectRoleId = projectUserRow.ProjectRoleId;
            remoteProjectUser.ProjectRoleName = projectUserRow.ProjectRoleName;
            remoteProjectUser.UserName = projectUserRow.UserName;
            if (projectUserRow.User != null)
            {
                remoteProjectUser.EmailAddress = projectUserRow.User.EmailAddress;
                remoteProjectUser.Active = projectUserRow.User.IsActive;
                remoteProjectUser.LdapDn = projectUserRow.User.LdapDn;
                if (projectUserRow.User.Profile != null)
                {
                    remoteProjectUser.FirstName = projectUserRow.User.Profile.FirstName;
                    remoteProjectUser.MiddleInitial = projectUserRow.User.Profile.MiddleInitial;
                    remoteProjectUser.LastName = projectUserRow.User.Profile.LastName;
                    remoteProjectUser.Admin = projectUserRow.User.Profile.IsAdmin;
                    remoteProjectUser.FullName = projectUserRow.User.Profile.FullName;
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteRelease">The API data object</param>
        /// <param name="dataRow">The internal datarow</param>
        public static void PopulateRelease(RemoteRelease remoteRelease, ReleaseView release)
        {
            //Artifact Fields
            remoteRelease.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Release;

            remoteRelease.ReleaseId = release.ReleaseId;
            remoteRelease.ProjectId = release.ProjectId;
            remoteRelease.IndentLevel = release.IndentLevel;
            remoteRelease.CreatorId = release.CreatorId;
            remoteRelease.Name = release.Name;
            remoteRelease.Description = release.Description;
            remoteRelease.VersionNumber = release.VersionNumber;
            remoteRelease.CreationDate = release.CreationDate;
            remoteRelease.LastUpdateDate = release.LastUpdateDate;
            remoteRelease.Summary = release.IsSummary;
            remoteRelease.Active = release.IsActive;
            remoteRelease.Iteration = release.IsIteration;
            remoteRelease.StartDate = release.StartDate;
            remoteRelease.EndDate = release.EndDate;
            remoteRelease.ResourceCount = (int)release.ResourceCount;
            remoteRelease.DaysNonWorking = (int)release.DaysNonWorking;
            remoteRelease.PlannedEffort = release.PlannedEffort;
            remoteRelease.AvailableEffort = release.AvailableEffort;
            remoteRelease.TaskEstimatedEffort = release.TaskEstimatedEffort;
            remoteRelease.TaskActualEffort = release.TaskActualEffort;
            remoteRelease.TaskCount = release.TaskCount;
            remoteRelease.CreatorName = release.CreatorName;
            remoteRelease.FullName = release.FullName;

            //Concurrency Management
            remoteRelease.ConcurrencyDate = release.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteRequirement">The API data object</param>
        /// <param name="requirement">The internal entity</param>
        public static void PopulateRequirement(RemoteRequirement remoteRequirement, RequirementView requirement)
        {
            //Artifact Fields
            remoteRequirement.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Requirement;

            remoteRequirement.RequirementId = requirement.RequirementId;
            remoteRequirement.StatusId = requirement.RequirementStatusId;
            remoteRequirement.ProjectId = requirement.ProjectId;
            remoteRequirement.IndentLevel = requirement.IndentLevel;
            remoteRequirement.AuthorId = requirement.AuthorId;
            remoteRequirement.OwnerId = requirement.OwnerId;
            remoteRequirement.ImportanceId = requirement.ImportanceId;
            remoteRequirement.ReleaseId = requirement.ReleaseId;
            remoteRequirement.Name = requirement.Name;
            remoteRequirement.Description =requirement.Description;
            remoteRequirement.CreationDate = requirement.CreationDate;
            remoteRequirement.LastUpdateDate = requirement.LastUpdateDate;
            remoteRequirement.Summary = requirement.IsSummary;
            remoteRequirement.CoverageCountTotal = requirement.CoverageCountTotal;
            remoteRequirement.CoverageCountPassed = requirement.CoverageCountPassed;
            remoteRequirement.CoverageCountFailed = requirement.CoverageCountFailed;
            remoteRequirement.CoverageCountCaution = requirement.CoverageCountCaution;
            remoteRequirement.CoverageCountBlocked = requirement.CoverageCountBlocked;
            remoteRequirement.PlannedEffort = requirement.EstimatedEffort;
            remoteRequirement.TaskEstimatedEffort = requirement.TaskEstimatedEffort;
            remoteRequirement.TaskActualEffort = requirement.TaskActualEffort;
            remoteRequirement.TaskCount = requirement.TaskCount;
            remoteRequirement.ReleaseVersionNumber = requirement.ReleaseVersionNumber;
            remoteRequirement.AuthorName = requirement.AuthorName;
            remoteRequirement.OwnerName = requirement.OwnerName;
            remoteRequirement.StatusName = requirement.RequirementStatusName;
            remoteRequirement.ImportanceName = requirement.ImportanceName;
            remoteRequirement.ProjectName = requirement.ProjectName;

            //Concurrency Management
            remoteRequirement.ConcurrencyDate = requirement.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCaseView">The internal datarow</param>
        /// <param name="indentLevel">The fake indent level needed for older API clients</param>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseView testCaseView, string indentLevel = "")
        {
            //Artifact Fields
            remoteTestCase.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestCase;

            remoteTestCase.TestCaseId = testCaseView.TestCaseId;
            remoteTestCase.ProjectId = testCaseView.ProjectId;
            remoteTestCase.IndentLevel = indentLevel;
            remoteTestCase.ExecutionStatusId = testCaseView.ExecutionStatusId;
            remoteTestCase.AuthorId = testCaseView.AuthorId;
            remoteTestCase.OwnerId = testCaseView.OwnerId;
            remoteTestCase.TestCasePriorityId = testCaseView.TestCasePriorityId;
            remoteTestCase.AutomationEngineId = testCaseView.AutomationEngineId;
            remoteTestCase.AutomationAttachmentId = testCaseView.AutomationAttachmentId;
            remoteTestCase.Name = testCaseView.Name;
            remoteTestCase.Description = testCaseView.Description;
            remoteTestCase.Folder = false;  //Separate object now;
            remoteTestCase.CreationDate = testCaseView.CreationDate;
            remoteTestCase.LastUpdateDate = testCaseView.LastUpdateDate;
            remoteTestCase.ExecutionDate = testCaseView.ExecutionDate;
            remoteTestCase.EstimatedDuration = testCaseView.EstimatedDuration;
            remoteTestCase.Active = testCaseView.IsActive;
            remoteTestCase.AuthorName = testCaseView.AuthorName;
            remoteTestCase.OwnerName = testCaseView.OwnerName;
            remoteTestCase.ProjectName = testCaseView.ProjectName;
            remoteTestCase.TestCasePriorityName = testCaseView.TestCasePriorityName;

            //Concurrency Management
            remoteTestCase.ConcurrencyDate = testCaseView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCaseView">The internal datarow</param>
        /// <param name="indentLevel">The fake indent level needed for older API clients</param>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseReleaseView testCaseView)
        {
            //Artifact Fields
            remoteTestCase.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestCase;

            remoteTestCase.TestCaseId = testCaseView.TestCaseId;
            remoteTestCase.ProjectId = testCaseView.ProjectId;
            remoteTestCase.ExecutionStatusId = testCaseView.ExecutionStatusId;
            remoteTestCase.AuthorId = testCaseView.AuthorId;
            remoteTestCase.OwnerId = testCaseView.OwnerId;
            remoteTestCase.TestCasePriorityId = testCaseView.TestCasePriorityId;
            remoteTestCase.AutomationEngineId = testCaseView.AutomationEngineId;
            remoteTestCase.AutomationAttachmentId = testCaseView.AutomationAttachmentId;
            remoteTestCase.Name = testCaseView.Name;
            remoteTestCase.Description = testCaseView.Description;
            remoteTestCase.Folder = false;  //Separate object now;
            remoteTestCase.CreationDate = testCaseView.CreationDate;
            remoteTestCase.LastUpdateDate = testCaseView.LastUpdateDate;
            remoteTestCase.ExecutionDate = testCaseView.ExecutionDate;
            remoteTestCase.EstimatedDuration = testCaseView.EstimatedDuration;
            remoteTestCase.Active = testCaseView.IsActive;
            remoteTestCase.AuthorName = testCaseView.AuthorName;
            remoteTestCase.OwnerName = testCaseView.OwnerName;
            remoteTestCase.TestCasePriorityName = testCaseView.TestCasePriorityName;

            //Concurrency Management
            remoteTestCase.ConcurrencyDate = testCaseView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder (makes the id negative)
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCaseFolder">The internal datarow</param>
        /// <param name="indentLevel">The legacy indent level</param>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseFolder testCaseFolder, string indentLevel = "")
        {
            //Artifact Fields
            remoteTestCase.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestCase;

            remoteTestCase.TestCaseId = -testCaseFolder.TestCaseFolderId;
            remoteTestCase.ProjectId = testCaseFolder.ProjectId;
            remoteTestCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
            remoteTestCase.Name = testCaseFolder.Name;
            remoteTestCase.Description = testCaseFolder.Description;
            remoteTestCase.Folder = true;
            remoteTestCase.LastUpdateDate = testCaseFolder.LastUpdateDate;
            remoteTestCase.ExecutionDate = testCaseFolder.ExecutionDate;
            remoteTestCase.EstimatedDuration = testCaseFolder.EstimatedDuration;
            remoteTestCase.Active = true;
            remoteTestCase.IndentLevel = indentLevel;
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder (makes the id negative)
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCaseFolder">The internal datarow</param>
        public static void PopulateTestCase(int projectId, RemoteTestCase remoteTestCase, TestCaseFolderHierarchyView testCaseFolder)
        {
            //Artifact Fields
            remoteTestCase.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestCase;

            remoteTestCase.TestCaseId = -testCaseFolder.TestCaseFolderId;
            remoteTestCase.ProjectId = projectId;
            remoteTestCase.Name = testCaseFolder.Name;
            remoteTestCase.Folder = true;
            remoteTestCase.Active = true;
            remoteTestCase.IndentLevel = testCaseFolder.IndentLevel;
            remoteTestCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestSet">The API data object</param>
        /// <param name="testSetView">The internal datarow</param>
        /// <param name="indentLevel">The fake indent level needed for older API clients</param>
        public static void PopulateTestSet(RemoteTestSet remoteTestSet, TestSetView testSetView, string indentLevel = "")
        {
            //Artifact Fields
            remoteTestSet.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestSet;

            remoteTestSet.TestSetId = testSetView.TestSetId;
            remoteTestSet.ProjectId = testSetView.ProjectId;
            remoteTestSet.IndentLevel = indentLevel;
            remoteTestSet.TestSetStatusId = testSetView.TestSetStatusId;
            remoteTestSet.CreatorId = testSetView.CreatorId;
            remoteTestSet.OwnerId = testSetView.OwnerId;
            remoteTestSet.ReleaseId = testSetView.ReleaseId;
            remoteTestSet.AutomationHostId = testSetView.AutomationHostId;
            remoteTestSet.TestRunTypeId = testSetView.TestRunTypeId;
            remoteTestSet.RecurrenceId = testSetView.RecurrenceId;
            remoteTestSet.Name = testSetView.Name;
            remoteTestSet.Description = testSetView.Description;
            remoteTestSet.Folder = false;  //Separate object now
            remoteTestSet.CreationDate = testSetView.CreationDate;
            remoteTestSet.LastUpdateDate = testSetView.LastUpdateDate;
            remoteTestSet.ExecutionDate = testSetView.ExecutionDate;
            remoteTestSet.PlannedDate = testSetView.PlannedDate;
            remoteTestSet.CountPassed = testSetView.CountPassed;
            remoteTestSet.CountFailed = testSetView.CountFailed;
            remoteTestSet.CountCaution = testSetView.CountCaution;
            remoteTestSet.CountBlocked = testSetView.CountBlocked;
            remoteTestSet.CountNotRun = testSetView.CountNotRun;
            remoteTestSet.CountNotApplicable = testSetView.CountNotApplicable;
            remoteTestSet.CreatorName = testSetView.CreatorName;
            remoteTestSet.OwnerName = testSetView.OwnerName;
            remoteTestSet.ProjectName = testSetView.ProjectName;
            remoteTestSet.TestSetStatusName = testSetView.TestSetStatusName;
            remoteTestSet.ReleaseVersionNumber = testSetView.ReleaseVersionNumber;
            remoteTestSet.RecurrenceName = testSetView.RecurrenceName;

            //Concurrency Management
            remoteTestSet.ConcurrencyDate = testSetView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder (makes the id negative)
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="remoteTestSet">The API data object</param>
        /// <param name="testSetFolder">The internal datarow</param>
        public static void PopulateTestSet(int projectId, RemoteTestSet remoteTestSet, TestSetFolderHierarchyView testSetFolder)
        {
            //Artifact Fields
            remoteTestSet.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestSet;

            remoteTestSet.TestSetId = -testSetFolder.TestSetFolderId;
            remoteTestSet.ProjectId = projectId;
            remoteTestSet.IndentLevel = testSetFolder.IndentLevel;
            remoteTestSet.Name = testSetFolder.Name;
            remoteTestSet.Folder = true;
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder (makes the id negative)
        /// </summary>
        /// <param name="remoteTestSet">The API data object</param>
        /// <param name="testSetFolder">The internal datarow</param>
        public static void PopulateTestSet(RemoteTestSet remoteTestSet, TestSetFolder testSetFolder)
        {
            //Artifact Fields
            remoteTestSet.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestSet;

            remoteTestSet.TestSetId = -testSetFolder.TestSetFolderId;
            remoteTestSet.ProjectId = testSetFolder.ProjectId;
            remoteTestSet.Name = testSetFolder.Name;
            remoteTestSet.Description = testSetFolder.Description;
            remoteTestSet.Folder = true;
            remoteTestSet.LastUpdateDate = testSetFolder.LastUpdateDate;
            remoteTestSet.ExecutionDate = testSetFolder.ExecutionDate;
            remoteTestSet.CountBlocked = testSetFolder.CountBlocked;
            remoteTestSet.CountCaution = testSetFolder.CountCaution;
            remoteTestSet.CountFailed = testSetFolder.CountFailed;
            remoteTestSet.CountNotApplicable = testSetFolder.CountNotApplicable;
            remoteTestSet.CountNotRun = testSetFolder.CountNotRun;
            remoteTestSet.CountPassed = testSetFolder.CountPassed;
            remoteTestSet.CreationDate = testSetFolder.CreationDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestStep">The API data object</param>
        /// <param name="testStepView">The internal datarow</param>
        /// <param name="projectId">The id of the current project</param>
        public static void PopulateTestStep(RemoteTestStep remoteTestStep, TestStepView testStepView, int projectId)
        {
            //Artifact Fields
            remoteTestStep.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestStep;

            remoteTestStep.TestStepId = testStepView.TestStepId;
            remoteTestStep.ProjectId = projectId;
            remoteTestStep.TestCaseId = testStepView.TestCaseId;
            remoteTestStep.ExecutionStatusId = testStepView.ExecutionStatusId;
            remoteTestStep.Position = testStepView.Position;
            remoteTestStep.Description = testStepView.Description;
            remoteTestStep.ExpectedResult = testStepView.ExpectedResult;
            remoteTestStep.SampleData = testStepView.SampleData;
            remoteTestStep.LinkedTestCaseId = testStepView.LinkedTestCaseId;
            remoteTestStep.LastUpdateDate = testStepView.LastUpdateDate;

            //Concurrency Management
            remoteTestStep.ConcurrencyDate = testStepView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestStep">The API data object</param>
        /// <param name="testStepView">The internal datarow</param>
        /// <param name="projectId">The id of the current project</param>
        public static void PopulateTestStep(RemoteTestStep remoteTestStep, TestStep testStep, int projectId)
        {
            //Artifact Fields
            remoteTestStep.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestStep;

            remoteTestStep.TestStepId = testStep.TestStepId;
            remoteTestStep.ProjectId = projectId;
            remoteTestStep.TestCaseId = testStep.TestCaseId;
            remoteTestStep.ExecutionStatusId = testStep.ExecutionStatusId;
            remoteTestStep.Position = testStep.Position;
            remoteTestStep.Description = testStep.Description;
            remoteTestStep.ExpectedResult = testStep.ExpectedResult;
            remoteTestStep.SampleData = testStep.SampleData;
            remoteTestStep.LinkedTestCaseId = testStep.LinkedTestCaseId;
            remoteTestStep.LastUpdateDate = testStep.LastUpdateDate;

            //Concurrency Management
            remoteTestStep.ConcurrencyDate = testStep.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a user API object from the internal datarow
        /// </summary>
        /// <param name="remoteUser">The API data object</param>
        /// <param name="isSysAdmin">is the logged-in user a sysadmin</param>
        /// <param name="user">The internal user entity object</param>
        /// <remarks>It only populates the RSS token when the user calling the API is a sysadmin for security reasons</remarks>
        public static void PopulateUser(RemoteUser remoteUser, User user, bool isSysAdmin)
        {
            remoteUser.UserId = user.UserId;
            remoteUser.FirstName = user.Profile.FirstName;
            remoteUser.LastName = user.Profile.LastName;
            remoteUser.MiddleInitial = user.Profile.MiddleInitial;
            remoteUser.UserName = user.UserName;
            remoteUser.LdapDn = user.LdapDn;
            remoteUser.EmailAddress = user.EmailAddress;
            remoteUser.Active = (user.IsActive);
            remoteUser.Admin = (user.Profile.IsAdmin);
            remoteUser.Department = user.Profile.Department;
            remoteUser.Approved = user.IsApproved;
            remoteUser.Locked = user.IsLocked;
            if (isSysAdmin)
            {
                remoteUser.RssToken = user.RssToken;
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testSetTestCaseView">The internal datarow</param>
        /// <remarks>Used to translate the test set test case datarow into a normal test case API object</remarks>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestSetTestCaseView testSetTestCaseView)
        {
            //Artifact Fields
            remoteTestCase.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestCase;

            remoteTestCase.TestCaseId = testSetTestCaseView.TestCaseId;
            remoteTestCase.ProjectId = testSetTestCaseView.ProjectId;
            remoteTestCase.ExecutionStatusId = testSetTestCaseView.ExecutionStatusId;
            remoteTestCase.AuthorId = testSetTestCaseView.AuthorId;
            remoteTestCase.OwnerId = testSetTestCaseView.OwnerId;
            remoteTestCase.TestCasePriorityId = testSetTestCaseView.TestCasePriorityId;
            remoteTestCase.Name = testSetTestCaseView.Name;
            remoteTestCase.Description = testSetTestCaseView.Description;
            remoteTestCase.Folder = false;  //There are no folders in a test set
            remoteTestCase.CreationDate = testSetTestCaseView.CreationDate;
            remoteTestCase.LastUpdateDate = testSetTestCaseView.LastUpdateDate;
            remoteTestCase.ExecutionDate = testSetTestCaseView.ExecutionDate;
            remoteTestCase.EstimatedDuration = testSetTestCaseView.EstimatedDuration;
            remoteTestCase.Active = true;   //Always active if in test set

            //Concurrency Management
            remoteTestCase.ConcurrencyDate = testSetTestCaseView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentFolder">The API data object</param>
        /// <param name="projectAttachmentFolder">The internal data row</param>
        public static void PopulateDocumentFolder(RemoteDocumentFolder remoteDocumentFolder, ProjectAttachmentFolderHierarchy projectAttachmentFolder)
        {
            remoteDocumentFolder.ProjectAttachmentFolderId = projectAttachmentFolder.ProjectAttachmentFolderId;
            remoteDocumentFolder.ProjectId = projectAttachmentFolder.ProjectId;
            remoteDocumentFolder.ParentProjectAttachmentFolderId = projectAttachmentFolder.ParentProjectAttachmentFolderId;
            remoteDocumentFolder.Name = projectAttachmentFolder.Name;
            remoteDocumentFolder.IndentLevel = projectAttachmentFolder.IndentLevel;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentFolder">The API data object</param>
        /// <param name="projectAttachmentFolder">The internal data row</param>
        public static void PopulateDocumentFolder(RemoteDocumentFolder remoteDocumentFolder, ProjectAttachmentFolder projectAttachmentFolder)
        {
            remoteDocumentFolder.ProjectAttachmentFolderId = projectAttachmentFolder.ProjectAttachmentFolderId;
            remoteDocumentFolder.ProjectId = projectAttachmentFolder.ProjectId;
            remoteDocumentFolder.ParentProjectAttachmentFolderId = projectAttachmentFolder.ParentProjectAttachmentFolderId;
            remoteDocumentFolder.Name = projectAttachmentFolder.Name;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentVersion">The API data object</param>
        /// <param name="attachmentVersion">The internal datarow</param>
        public static void PopulateDocumentVersion(RemoteDocumentVersion remoteDocumentVersion, AttachmentVersionView attachmentVersion)
        {
            remoteDocumentVersion.AttachmentVersionId = attachmentVersion.AttachmentVersionId;
            remoteDocumentVersion.AttachmentId = attachmentVersion.AttachmentId;
            remoteDocumentVersion.AuthorId = attachmentVersion.AuthorId;
            remoteDocumentVersion.FilenameOrUrl = attachmentVersion.Filename;
            remoteDocumentVersion.Description = attachmentVersion.Description;
            remoteDocumentVersion.UploadDate = attachmentVersion.UploadDate;
            remoteDocumentVersion.Size = attachmentVersion.Size;
            remoteDocumentVersion.VersionNumber = attachmentVersion.VersionNumber;
            remoteDocumentVersion.AuthorName = attachmentVersion.AuthorName;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal entity
        /// </summary>
        /// <param name="remoteBuildSourceCode">The API data object</param>
        /// <param name="buildSourceCode">The internal entity</param>
        public static void PopulateBuildSourceCode(RemoteBuildSourceCode remoteBuildSourceCode, BuildSourceCode buildSourceCode)
        {
            remoteBuildSourceCode.BuildId = buildSourceCode.BuildId;
            remoteBuildSourceCode.RevisionKey = buildSourceCode.RevisionKey;
            remoteBuildSourceCode.CreationDate = buildSourceCode.CreationDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteAutomationHost">The API data object</param>
        /// <param name="automationHost">The internal datarow</param>
        public static void PopulateAutomationHost(RemoteAutomationHost remoteAutomationHost, AutomationHostView automationHost)
        {
            //Artifact Fields
            remoteAutomationHost.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.AutomationHost;

            remoteAutomationHost.AutomationHostId = automationHost.AutomationHostId;
            remoteAutomationHost.ProjectId = automationHost.ProjectId;
            remoteAutomationHost.Name = automationHost.Name;
            remoteAutomationHost.Description = automationHost.Description;
            remoteAutomationHost.Token = automationHost.Token;
            remoteAutomationHost.LastUpdateDate = automationHost.LastUpdateDate;
            remoteAutomationHost.Active = automationHost.IsActive;
            remoteAutomationHost.LastContactDate = automationHost.LastContactDate;

            //Concurrency Management
            remoteAutomationHost.ConcurrencyDate = automationHost.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a project API object from the internal datarow
        /// </summary>
        /// <param name="remoteProject">The API data object</param>
        /// <param name="projectRow">The internal project entity</param>
        public static void PopulateProject(RemoteProject remoteProject, Project project)
        {
            remoteProject.ProjectId = project.ProjectId;
            remoteProject.Name = project.Name;
            remoteProject.Description = project.Description;
            remoteProject.Website = project.Website;
            remoteProject.CreationDate = project.CreationDate;
            remoteProject.Active = project.IsActive;
            remoteProject.WorkingHours = project.WorkingHours;
            remoteProject.WorkingDays = project.WorkingDays;
            remoteProject.NonWorkingHours = project.NonWorkingHours;
        }

        /// <summary>
        /// Populates a project API object from the internal datarow
        /// </summary>
        /// <param name="remoteProjectRole">The API data object</param>
        /// <param name="projectRoleRow">The internal project role datarow</param>
        public static void PopulateProjectRole(RemoteProjectRole remoteProjectRole, ProjectRole projectRoleRow)
        {
            remoteProjectRole.ProjectRoleId = projectRoleRow.ProjectRoleId;
            remoteProjectRole.Name = projectRoleRow.Name;
            remoteProjectRole.Description = projectRoleRow.Description;
            remoteProjectRole.Active = (projectRoleRow.IsActive);
            remoteProjectRole.Admin = (projectRoleRow.IsAdmin);
            remoteProjectRole.DocumentsAdd = (projectRoleRow.RolePermissions.Any(p => p.PermissionId == (int)Project.PermissionEnum.Create && p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document));
            remoteProjectRole.DocumentsDelete = (projectRoleRow.RolePermissions.Any(p => p.PermissionId == (int)Project.PermissionEnum.Modify && p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document));
            remoteProjectRole.DocumentsEdit = (projectRoleRow.RolePermissions.Any(p => p.PermissionId == (int)Project.PermissionEnum.Delete && p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document));
            remoteProjectRole.DiscussionsAdd = (projectRoleRow.IsDiscussionsAdd);
            remoteProjectRole.SourceCodeView = (projectRoleRow.IsSourceCodeView);
        }
    }
}