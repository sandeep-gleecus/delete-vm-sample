using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Collections;

using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Helper class that populates the RemoteObjects from internal data classes
    /// </summary>
    public static class PopulationFunctions
    {
		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteTestRun">The API data object</param>
		/// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
		public static void PopulateManualTestRun(RemoteManualTestRun remoteTestRun, TestRun testRun)
		{
			//First populate the parts that are common to manual and automated test runs
			PopulateTestRun(remoteTestRun, testRun);

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
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateAutomatedTestRun(RemoteAutomatedTestRun remoteTestRun, TestRun testRun)
        {
            //First populate the parts that are common to manual and automated test runs
            PopulateTestRun(remoteTestRun, testRun);

            //Now the automation-specific info
            remoteTestRun.RunnerName = testRun.RunnerName;
            remoteTestRun.RunnerTestName = testRun.RunnerTestName;
            remoteTestRun.RunnerAssertCount = testRun.RunnerAssertCount;
            remoteTestRun.RunnerMessage = testRun.RunnerMessage;
            remoteTestRun.RunnerStackTrace = testRun.RunnerStackTrace;
            remoteTestRun.AutomationHostId = testRun.AutomationHostId;
            remoteTestRun.AutomationEngineId = testRun.AutomationEngineId;
            remoteTestRun.AutomationAttachmentId = null;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateAutomatedTestRun(RemoteAutomatedTestRun remoteTestRun, TestRunView testRun)
        {
            //First populate the parts that are common to manual and automated test runs
            PopulateTestRun(remoteTestRun, testRun);

            //Now the automation-specific info
            remoteTestRun.RunnerName = testRun.RunnerName;
            remoteTestRun.RunnerTestName = testRun.RunnerTestName;
            remoteTestRun.RunnerAssertCount = testRun.RunnerAssertCount;
            remoteTestRun.RunnerMessage = testRun.RunnerMessage;
            remoteTestRun.RunnerStackTrace = testRun.RunnerStackTrace;
            remoteTestRun.AutomationHostId = testRun.AutomationHostId;
            remoteTestRun.AutomationEngineId = testRun.AutomationEngineId;
            remoteTestRun.AutomationAttachmentId = null;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateTestRun(RemoteTestRun remoteTestRun, TestRun testRun)
        {
            //Populate the parts that are common to manual and automated test runs
            remoteTestRun.TestRunId = testRun.TestRunId;
            remoteTestRun.Name = testRun.Name;
            remoteTestRun.TestCaseId = testRun.TestCaseId;
            remoteTestRun.TestRunTypeId = testRun.TestRunTypeId;
            remoteTestRun.TesterId = testRun.TesterId;
            remoteTestRun.ExecutionStatusId = testRun.ExecutionStatusId;
            remoteTestRun.ReleaseId = testRun.ReleaseId;
            remoteTestRun.TestSetId = testRun.TestSetId;
            remoteTestRun.TestSetTestCaseId = testRun.TestSetTestCaseId;
            remoteTestRun.StartDate = GlobalFunctions.LocalizeDate(testRun.StartDate);
            remoteTestRun.EndDate = GlobalFunctions.LocalizeDate(testRun.EndDate);
            remoteTestRun.BuildId = testRun.BuildId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateTestRun(RemoteTestRun remoteTestRun, TestRunView testRun)
        {
            //Populate the parts that are common to manual and automated test runs
            remoteTestRun.TestRunId = testRun.TestRunId;
            remoteTestRun.Name = testRun.Name;
            remoteTestRun.TestCaseId = testRun.TestCaseId;
            remoteTestRun.TestRunTypeId = testRun.TestRunTypeId;
            remoteTestRun.TesterId = testRun.TesterId;
            remoteTestRun.ExecutionStatusId = testRun.ExecutionStatusId;
            remoteTestRun.ReleaseId = testRun.ReleaseId;
            remoteTestRun.TestSetId = testRun.TestSetId;
            remoteTestRun.TestSetTestCaseId = testRun.TestSetTestCaseId;
            remoteTestRun.StartDate = GlobalFunctions.LocalizeDate(testRun.StartDate);
            remoteTestRun.EndDate = GlobalFunctions.LocalizeDate(testRun.EndDate);
            remoteTestRun.BuildId = testRun.BuildId;
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
        /// Populates a data-sync system API object from the internal entity
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
            remoteBuild.LastUpdateDate = GlobalFunctions.LocalizeDate(build.LastUpdateDate);
            remoteBuild.CreationDate = GlobalFunctions.LocalizeDate(build.CreationDate);
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
        /// <param name="requirement">The requirement entity (optional)</param>
        public static void PopulateTask(RemoteTask remoteTask, TaskView task)
        {
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
            remoteTask.CreationDate = GlobalFunctions.LocalizeDate(task.CreationDate);
            remoteTask.LastUpdateDate = GlobalFunctions.LocalizeDate(task.LastUpdateDate);
            remoteTask.StartDate = GlobalFunctions.LocalizeDate(task.StartDate);
            remoteTask.EndDate = GlobalFunctions.LocalizeDate(task.EndDate);
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

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteTask.LastUpdateDate = GlobalFunctions.LocalizeDate(task.ConcurrencyDate);
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
                        //Convert dates to UTC
                        remoteFilter.DateRangeValue.Internal.StartDate = GlobalFunctions.UniversalizeDate(remoteFilter.DateRangeValue.Internal.StartDate);
                        remoteFilter.DateRangeValue.Internal.EndDate = GlobalFunctions.UniversalizeDate(remoteFilter.DateRangeValue.Internal.EndDate);
                        filters.Add(remoteFilter.PropertyName, remoteFilter.DateRangeValue.Internal);
                    }
                }
            }
            return filters;
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from an artifact entity
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="artifact">The internal artifact entity (that also contains associated custom properties)</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        /// <param name="customPropertyDefinitions">The list of custom property definitions for this artifact type</param>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, Artifact artifact, List<CustomProperty> customPropertyDefinitions)
        {
            remoteArtifact.Text01 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_01", customPropertyDefinitions);
            remoteArtifact.Text02 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_02", customPropertyDefinitions);
            remoteArtifact.Text03 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_03", customPropertyDefinitions);
            remoteArtifact.Text04 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_04", customPropertyDefinitions);
            remoteArtifact.Text05 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_05", customPropertyDefinitions);
            remoteArtifact.Text06 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_06", customPropertyDefinitions);
            remoteArtifact.Text07 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_07", customPropertyDefinitions);
            remoteArtifact.Text08 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_08", customPropertyDefinitions);
            remoteArtifact.Text09 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_09", customPropertyDefinitions);
            remoteArtifact.Text10 = GetCustomTextPropertyValueFromArtifact(artifact, "TEXT_10", customPropertyDefinitions);

            remoteArtifact.List01 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_01", customPropertyDefinitions);
            remoteArtifact.List02 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_02", customPropertyDefinitions);
            remoteArtifact.List03 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_03", customPropertyDefinitions);
            remoteArtifact.List04 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_04", customPropertyDefinitions);
            remoteArtifact.List05 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_05", customPropertyDefinitions);
            remoteArtifact.List06 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_06", customPropertyDefinitions);
            remoteArtifact.List07 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_07", customPropertyDefinitions);
            remoteArtifact.List08 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_08", customPropertyDefinitions);
            remoteArtifact.List09 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_09", customPropertyDefinitions);
            remoteArtifact.List10 = GetCustomListPropertyValueFromArtifact(artifact, "LIST_10", customPropertyDefinitions);
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from the internal datarow
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="artifactCustomProperty">The internal custom property entity</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, ArtifactCustomProperty artifactCustomProperty)
        {
            if (artifactCustomProperty != null)
            {
                remoteArtifact.Text01 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_01");
                remoteArtifact.Text02 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_02");
                remoteArtifact.Text03 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_03");
                remoteArtifact.Text04 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_04");
                remoteArtifact.Text05 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_05");
                remoteArtifact.Text06 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_06");
                remoteArtifact.Text07 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_07");
                remoteArtifact.Text08 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_08");
                remoteArtifact.Text09 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_09");
                remoteArtifact.Text10 = artifactCustomProperty.LegacyCustomTextProperty("TEXT_10");

                remoteArtifact.List01 = artifactCustomProperty.LegacyCustomListProperty("LIST_01");
                remoteArtifact.List02 = artifactCustomProperty.LegacyCustomListProperty("LIST_02");
                remoteArtifact.List03 = artifactCustomProperty.LegacyCustomListProperty("LIST_03");
                remoteArtifact.List04 = artifactCustomProperty.LegacyCustomListProperty("LIST_04");
                remoteArtifact.List05 = artifactCustomProperty.LegacyCustomListProperty("LIST_05");
                remoteArtifact.List06 = artifactCustomProperty.LegacyCustomListProperty("LIST_06");
                remoteArtifact.List07 = artifactCustomProperty.LegacyCustomListProperty("LIST_07");
                remoteArtifact.List08 = artifactCustomProperty.LegacyCustomListProperty("LIST_08");
                remoteArtifact.List09 = artifactCustomProperty.LegacyCustomListProperty("LIST_09");
                remoteArtifact.List10 = artifactCustomProperty.LegacyCustomListProperty("LIST_10");
            }
        }

        /// <summary>
        /// Gets the value for a specific text custom field
        /// </summary>
        /// <param name="dataRow">The artifact datarow</param>
        /// <param name="legacyName">The legacy custom property field name ("TEXT_01")</param>
        /// <customPropertyDefinitions>Custom property definitions</customPropertyDefinitions>
        /// <returns>The string value or null</returns>
        internal static string GetCustomTextPropertyValueFromDataRow(DataRow dataRow, string legacyName, List<CustomProperty> customPropertyDefinitions)
        {
            //Find the custom property that uses this legacy name
            CustomProperty customProperty = customPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text);
            if (customProperty != null)
            {
                if (dataRow[customProperty.CustomPropertyFieldName] == DBNull.Value)
                {
                    return null;
                }
                else
                {
                    return (string)dataRow[customProperty.CustomPropertyFieldName];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the value for a specific text custom field
        /// </summary>
        /// <param name="artifact">The artifact enity</param>
        /// <param name="legacyName">The legacy custom property field name ("TEXT_01")</param>
        /// <customPropertyDefinitions>Custom property definitions</customPropertyDefinitions>
        /// <returns>The string value or null</returns>
        internal static string GetCustomTextPropertyValueFromArtifact(Artifact artifact, string legacyName, List<CustomProperty> customPropertyDefinitions)
        {
            //Find the custom property that uses this legacy name
            CustomProperty customProperty = customPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Text);
            if (customProperty != null)
            {
                if (!artifact.ContainsProperty(customProperty.CustomPropertyFieldName))
                {
                    return null;
                }
                else
                {
                    return (string)artifact[customProperty.CustomPropertyFieldName];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the value for a specific list custom field
        /// </summary>
        /// <param name="artifact">The artifact enity</param>
        /// <param name="legacyName">The legacy custom property field name ("LIST_01")</param>
        /// <customPropertyDefinitions>Custom property definitions</customPropertyDefinitions>
        /// <returns>The string value or null</returns>
        internal static int? GetCustomListPropertyValueFromArtifact(Artifact artifact, string legacyName, List<CustomProperty> customPropertyDefinitions)
        {
            //Find the custom property that uses this legacy name
            CustomProperty customProperty = customPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List);
            if (customProperty != null)
            {
                if (!artifact.ContainsProperty(customProperty.CustomPropertyFieldName))
                {
                    return null;
                }
                else
                {
                    string serializedValue = (string)artifact[customProperty.CustomPropertyFieldName];
                    int? value = serializedValue.FromDatabaseSerialization_Int32();
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the value for a specific list custom field
        /// </summary>
        /// <param name="dataRow">The artifact datarow</param>
        /// <param name="legacyName">The legacy custom property field name ("LIST_01")</param>
        /// <customPropertyDefinitions>Custom property definitions</customPropertyDefinitions>
        /// <returns>The string value or null</returns>
        internal static int? GetCustomListPropertyValueFromDataRow(DataRow dataRow, string legacyName, List<CustomProperty> customPropertyDefinitions)
        {
            //Find the custom property that uses this legacy name
            CustomProperty customProperty = customPropertyDefinitions.FirstOrDefault(cp => cp.LegacyName == legacyName && cp.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List);
            if (customProperty != null)
            {
                if (dataRow[customProperty.CustomPropertyFieldName] == DBNull.Value)
                {
                    return null;
                }
                else
                {
                    string serializedValue = (string)dataRow[customProperty.CustomPropertyFieldName];
                    int? value = serializedValue.FromDatabaseSerialization_Int32();
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from the internal datarow
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="dataRow">The internal artifact data row (that also contains associated custom properties)</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        /// <param name="customPropertyDefinitions">The list of custom property definitions for this artifact type</param>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, DataRow dataRow, List<CustomProperty> customPropertyDefinitions)
        {
            remoteArtifact.Text01 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_01", customPropertyDefinitions);
            remoteArtifact.Text02 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_02", customPropertyDefinitions);
            remoteArtifact.Text03 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_03", customPropertyDefinitions);
            remoteArtifact.Text04 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_04", customPropertyDefinitions);
            remoteArtifact.Text05 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_05", customPropertyDefinitions);
            remoteArtifact.Text06 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_06", customPropertyDefinitions);
            remoteArtifact.Text07 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_07", customPropertyDefinitions);
            remoteArtifact.Text08 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_08", customPropertyDefinitions);
            remoteArtifact.Text09 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_09", customPropertyDefinitions);
            remoteArtifact.Text10 = GetCustomTextPropertyValueFromDataRow(dataRow, "TEXT_10", customPropertyDefinitions);

            remoteArtifact.List01 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_01", customPropertyDefinitions);
            remoteArtifact.List02 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_02", customPropertyDefinitions);
            remoteArtifact.List03 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_03", customPropertyDefinitions);
            remoteArtifact.List04 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_04", customPropertyDefinitions);
            remoteArtifact.List05 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_05", customPropertyDefinitions);
            remoteArtifact.List06 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_06", customPropertyDefinitions);
            remoteArtifact.List07 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_07", customPropertyDefinitions);
            remoteArtifact.List08 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_08", customPropertyDefinitions);
            remoteArtifact.List09 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_09", customPropertyDefinitions);
            remoteArtifact.List10 = GetCustomListPropertyValueFromDataRow(dataRow, "LIST_10", customPropertyDefinitions);
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteIncident">The API data object</param>
        /// <param name="incident">The internal entity</param>
        public static void PopulateIncident(RemoteIncident remoteIncident, IncidentView incident, int? testRunStepId = null)
        {
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
            remoteIncident.CreationDate = GlobalFunctions.LocalizeDate(incident.CreationDate);
            remoteIncident.StartDate = GlobalFunctions.LocalizeDate(incident.StartDate);
            remoteIncident.ClosedDate = GlobalFunctions.LocalizeDate(incident.ClosedDate);
            remoteIncident.CompletionPercent = incident.CompletionPercent;
            remoteIncident.EstimatedEffort = incident.EstimatedEffort;
            remoteIncident.ActualEffort = incident.ActualEffort;
            remoteIncident.ProjectedEffort = incident.ProjectedEffort;
            remoteIncident.RemainingEffort = incident.RemainingEffort;
            remoteIncident.LastUpdateDate = GlobalFunctions.LocalizeDate(incident.LastUpdateDate);
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

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteIncident.LastUpdateDate = GlobalFunctions.LocalizeDate(incident.ConcurrencyDate);
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocument">The API data object</param>
        /// <param name="projectAttachment">The internal datarow</param>
        public static void PopulateDocument(RemoteDocument remoteDocument, ProjectAttachmentView projectAttachment)
        {
            remoteDocument.AttachmentId = projectAttachment.AttachmentId;
            remoteDocument.AttachmentTypeId = projectAttachment.AttachmentTypeId;
            remoteDocument.AuthorId = projectAttachment.AuthorId;
            remoteDocument.EditorId = projectAttachment.EditorId;
            remoteDocument.FilenameOrUrl = projectAttachment.Filename;
            remoteDocument.Description = projectAttachment.Description;
            remoteDocument.UploadDate = GlobalFunctions.LocalizeDate(projectAttachment.UploadDate);
            remoteDocument.EditedDate = GlobalFunctions.LocalizeDate(projectAttachment.EditedDate);
            remoteDocument.Size = projectAttachment.Size;
            remoteDocument.CurrentVersion = projectAttachment.CurrentVersion;
            remoteDocument.Tags = projectAttachment.Tags;
            remoteDocument.AttachmentTypeName = projectAttachment.AttachmentTypeName;
            remoteDocument.ProjectAttachmentFolderId = projectAttachment.ProjectAttachmentFolderId;
            remoteDocument.ProjectAttachmentTypeId = projectAttachment.DocumentTypeId;
            remoteDocument.ProjectAttachmentTypeName = projectAttachment.DocumentTypeName;
            remoteDocument.AuthorName = projectAttachment.AuthorName;
            remoteDocument.EditorName = projectAttachment.EditorName;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="projectId">the id of the project</param>
        /// <param name="remoteDocumentType">The API data object</param>
        /// <param name="documentType">The internal data row</param>
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
                remoteProjectUser.Password = "";
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
        /// <param name="release">The internal datarow</param>
        public static void PopulateRelease(RemoteRelease remoteRelease, ReleaseView release)
        {
            remoteRelease.ReleaseId = release.ReleaseId;
            remoteRelease.ProjectId = release.ProjectId;
            remoteRelease.IndentLevel = release.IndentLevel;
            remoteRelease.CreatorId = release.CreatorId;
            remoteRelease.Name = release.Name;
            remoteRelease.Description = release.Description;
            remoteRelease.VersionNumber = release.VersionNumber;
            remoteRelease.CreationDate = GlobalFunctions.LocalizeDate(release.CreationDate);
            remoteRelease.LastUpdateDate = GlobalFunctions.LocalizeDate(release.LastUpdateDate);
            remoteRelease.Summary = release.IsSummary;
            remoteRelease.Active = release.IsActive;
            remoteRelease.Iteration = release.IsIteration;
            remoteRelease.StartDate = GlobalFunctions.LocalizeDate(release.StartDate);
            remoteRelease.EndDate = GlobalFunctions.LocalizeDate(release.EndDate);
            remoteRelease.ResourceCount = (int)release.ResourceCount;
            remoteRelease.DaysNonWorking = (int)release.DaysNonWorking;
            remoteRelease.PlannedEffort = release.PlannedEffort;
            remoteRelease.AvailableEffort = release.AvailableEffort;
            remoteRelease.TaskEstimatedEffort = release.TaskEstimatedEffort;
            remoteRelease.TaskActualEffort = release.TaskActualEffort;
            remoteRelease.TaskCount = release.TaskCount;
            remoteRelease.CreatorName = release.CreatorName;
            remoteRelease.FullName = release.FullName;

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteRelease.LastUpdateDate = GlobalFunctions.LocalizeDate(release.ConcurrencyDate);
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteRequirement">The API data object</param>
        /// <param name="requirement">The internal entity</param>
        public static void PopulateRequirement(RemoteRequirement remoteRequirement, RequirementView requirement)
        {
            remoteRequirement.RequirementId = requirement.RequirementId;
            remoteRequirement.StatusId = requirement.RequirementStatusId;
            remoteRequirement.ProjectId = requirement.ProjectId;
            remoteRequirement.IndentLevel = requirement.IndentLevel;
            remoteRequirement.AuthorId = requirement.AuthorId;
            remoteRequirement.OwnerId = requirement.OwnerId;
            remoteRequirement.ImportanceId = requirement.ImportanceId;
            remoteRequirement.ReleaseId = requirement.ReleaseId;
            remoteRequirement.Name = requirement.Name;
            remoteRequirement.Description = requirement.Description;
            remoteRequirement.CreationDate = GlobalFunctions.LocalizeDate(requirement.CreationDate);
            remoteRequirement.LastUpdateDate = GlobalFunctions.LocalizeDate(requirement.LastUpdateDate);
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

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteRequirement.LastUpdateDate = GlobalFunctions.LocalizeDate(requirement.ConcurrencyDate);
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCaseView">The internal datarow</param>
        /// <param name="indentLevel">The fake indent level needed for older API clients</param>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseView testCaseView, string indentLevel = "")
        {
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
            remoteTestCase.Folder = false;  //Separate object now
            remoteTestCase.CreationDate = GlobalFunctions.LocalizeDate(testCaseView.CreationDate);
            remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseView.LastUpdateDate);
            remoteTestCase.ExecutionDate = GlobalFunctions.LocalizeDate(testCaseView.ExecutionDate);
            remoteTestCase.EstimatedDuration = testCaseView.EstimatedDuration;
            remoteTestCase.Active = testCaseView.IsActive;
            remoteTestCase.AuthorName = testCaseView.AuthorName;
            remoteTestCase.OwnerName = testCaseView.OwnerName;
            remoteTestCase.ProjectName = testCaseView.ProjectName;
            remoteTestCase.TestCasePriorityName = testCaseView.TestCasePriorityName;

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseView.ConcurrencyDate);
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCaseView">The internal datarow</param>
        /// <param name="indentLevel">The fake indent level needed for older API clients</param>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseReleaseView testCaseView)
        {
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
            remoteTestCase.Folder = false;  //Separate object now
            remoteTestCase.CreationDate = GlobalFunctions.LocalizeDate(testCaseView.CreationDate);
            remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseView.LastUpdateDate);
            remoteTestCase.ExecutionDate = GlobalFunctions.LocalizeDate(testCaseView.ExecutionDate);
            remoteTestCase.EstimatedDuration = testCaseView.EstimatedDuration;
            remoteTestCase.Active = testCaseView.IsActive;
            remoteTestCase.AuthorName = testCaseView.AuthorName;
            remoteTestCase.OwnerName = testCaseView.OwnerName;
            remoteTestCase.TestCasePriorityName = testCaseView.TestCasePriorityName;

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseView.ConcurrencyDate);
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestSet">The API data object</param>
        /// <param name="testSetView">The internal datarow</param>
        /// <param name="indentLevel">The fake indent level needed for older API clients</param>
        public static void PopulateTestSet(RemoteTestSet remoteTestSet, TestSetView testSetView, string indentLevel = "")
        {
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
            remoteTestSet.CreationDate = GlobalFunctions.LocalizeDate(testSetView.CreationDate);
            remoteTestSet.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetView.LastUpdateDate);
            remoteTestSet.ExecutionDate = GlobalFunctions.LocalizeDate(testSetView.ExecutionDate);
            remoteTestSet.PlannedDate = GlobalFunctions.LocalizeDate(testSetView.PlannedDate);
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

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteTestSet.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetView.ConcurrencyDate);
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder (makes the id negative)
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="remoteTestSet">The API data object</param>
        /// <param name="testSetFolder">The internal datarow</param>
        public static void PopulateTestSet(int projectId, RemoteTestSet remoteTestSet, TestSetFolderHierarchyView testSetFolder)
        {
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
            remoteTestSet.TestSetId = -testSetFolder.TestSetFolderId;
            remoteTestSet.ProjectId = testSetFolder.ProjectId;
            remoteTestSet.Name = testSetFolder.Name;
            remoteTestSet.Description = testSetFolder.Description;
            remoteTestSet.Folder = true;
            remoteTestSet.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetFolder.LastUpdateDate);
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
        public static void PopulateTestStep(RemoteTestStep remoteTestStep, TestStepView testStepView)
        {
            remoteTestStep.TestStepId = testStepView.TestStepId;
            remoteTestStep.TestCaseId = testStepView.TestCaseId;
            remoteTestStep.ExecutionStatusId = testStepView.ExecutionStatusId;
            remoteTestStep.Position = testStepView.Position;
            remoteTestStep.Description = testStepView.Description;
            remoteTestStep.ExpectedResult = testStepView.ExpectedResult;
            remoteTestStep.SampleData = testStepView.SampleData;
            remoteTestStep.LinkedTestCaseId = testStepView.LinkedTestCaseId;
            remoteTestStep.LastUpdateDate = GlobalFunctions.LocalizeDate(testStepView.LastUpdateDate);

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteTestStep.LastUpdateDate = GlobalFunctions.LocalizeDate(testStepView.ConcurrencyDate);
        }

        /// <summary>
        /// Populates a user API object from the internal datarow
        /// </summary>
        /// <param name="remoteUser">The API data object</param>
        /// <param name="user">The internal user entity object</param>
        public static void PopulateUser(RemoteUser remoteUser, User user)
        {
            remoteUser.UserId = user.UserId;
            remoteUser.FirstName = user.Profile.FirstName;
            remoteUser.LastName = user.Profile.LastName;
            remoteUser.MiddleInitial = user.Profile.MiddleInitial;
            remoteUser.UserName = user.UserName;
            remoteUser.Password = "";   //Not returned for security reasons
            remoteUser.LdapDn = user.LdapDn;
            remoteUser.EmailAddress = user.EmailAddress;
            remoteUser.Active = (user.IsActive);
            remoteUser.Admin = (user.Profile.IsAdmin);
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testSetTestCaseView">The internal datarow</param>
        /// <remarks>Used to translate the test set test case datarow into a normal test case API object</remarks>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestSetTestCaseView testSetTestCaseView)
        {
            remoteTestCase.TestCaseId = testSetTestCaseView.TestCaseId;
            remoteTestCase.ProjectId = testSetTestCaseView.ProjectId;
            remoteTestCase.ExecutionStatusId = testSetTestCaseView.ExecutionStatusId;
            remoteTestCase.AuthorId = testSetTestCaseView.AuthorId;
            remoteTestCase.OwnerId = testSetTestCaseView.OwnerId;
            remoteTestCase.TestCasePriorityId = testSetTestCaseView.TestCasePriorityId;
            remoteTestCase.Name = testSetTestCaseView.Name;
            remoteTestCase.Description = testSetTestCaseView.Description;
            remoteTestCase.Folder = false;  //There are no folders in a test set
            remoteTestCase.CreationDate = GlobalFunctions.LocalizeDate(testSetTestCaseView.CreationDate);
            remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetTestCaseView.LastUpdateDate);
            remoteTestCase.ExecutionDate = GlobalFunctions.LocalizeDate(testSetTestCaseView.ExecutionDate);
            remoteTestCase.EstimatedDuration = testSetTestCaseView.EstimatedDuration;
            remoteTestCase.Active = true;   //Always active if in test set

            //The remote object uses LastUpdateDate for concurrency, so need to override the value
            remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testSetTestCaseView.ConcurrencyDate);
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder (makes the id negative)
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCaseFolder">The internal datarow</param>
        /// <param name="indentLevel">The legacy indent level</param>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseFolder testCaseFolder, string indentLevel = "")
        {
            remoteTestCase.TestCaseId = -testCaseFolder.TestCaseFolderId;
            remoteTestCase.ProjectId = testCaseFolder.ProjectId;
            remoteTestCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
            remoteTestCase.Name = testCaseFolder.Name;
            remoteTestCase.Description = testCaseFolder.Description;
            remoteTestCase.Folder = true;
            remoteTestCase.LastUpdateDate = GlobalFunctions.LocalizeDate(testCaseFolder.LastUpdateDate);
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
            remoteTestCase.TestCaseId = -testCaseFolder.TestCaseFolderId;
            remoteTestCase.ProjectId = projectId;
            remoteTestCase.Name = testCaseFolder.Name;
            remoteTestCase.Folder = true;
            remoteTestCase.Active = true;
            remoteTestCase.IndentLevel = testCaseFolder.IndentLevel;
            remoteTestCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentFolder">The API data object</param>
        /// <param name="dataRow">The internal data row</param>
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
        /// <param name="dataRow">The internal data row</param>
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
        /// <param name="dataRow">The internal datarow</param>
        public static void PopulateDocumentVersion(RemoteDocumentVersion remoteDocumentVersion, AttachmentVersionView attachmentVersion)
        {
            remoteDocumentVersion.AttachmentVersionId = attachmentVersion.AttachmentVersionId;
            remoteDocumentVersion.AttachmentId = attachmentVersion.AttachmentId;
            remoteDocumentVersion.AuthorId = attachmentVersion.AuthorId;
            remoteDocumentVersion.FilenameOrUrl = attachmentVersion.Filename;
            remoteDocumentVersion.Description = attachmentVersion.Description;
            remoteDocumentVersion.UploadDate = GlobalFunctions.LocalizeDate(attachmentVersion.UploadDate);
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
            remoteBuildSourceCode.CreationDate = GlobalFunctions.LocalizeDate(buildSourceCode.CreationDate);
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteAutomationHost">The API data object</param>
        /// <param name="automationHost">The internal datarow</param>
        public static void PopulateAutomationHost(RemoteAutomationHost remoteAutomationHost, AutomationHostView automationHost)
        {
            remoteAutomationHost.AutomationHostId = automationHost.AutomationHostId;
            remoteAutomationHost.ProjectId = automationHost.ProjectId;
            remoteAutomationHost.Name = automationHost.Name;
            remoteAutomationHost.Description = automationHost.Description;
            remoteAutomationHost.Token = automationHost.Token;
            remoteAutomationHost.LastUpdateDate = automationHost.LastUpdateDate;
            remoteAutomationHost.Active = automationHost.IsActive;

            //Concurrency Management
            remoteAutomationHost.LastUpdateDate = GlobalFunctions.LocalizeDate(automationHost.ConcurrencyDate);
        }

        /// <summary>
        /// Populates a project API object from the internal datarow
        /// </summary>
        /// <param name="remoteProject">The API data object</param>
        /// <param name="project">The internal project entity</param>
        public static void PopulateProject(RemoteProject remoteProject, Project project)
        {
            remoteProject.ProjectId = project.ProjectId;
            remoteProject.Name = project.Name;
            remoteProject.Description = project.Description;
            remoteProject.Website = project.Website;
            remoteProject.CreationDate = GlobalFunctions.LocalizeDate(project.CreationDate);
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