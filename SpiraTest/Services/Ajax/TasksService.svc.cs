using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Communicates with the SortableGrid AJAX component for displaying/updating tasks data
	/// </summary>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class TasksService : SortedListServiceBase, ITasksService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TasksService::";

		protected internal const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_TASK_TASK_PAGINATION_SIZE;

		#region ITaskService Native Methods

		/// <summary>
		/// Returns the data needed for the simple task progress graph on the list page
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release (optional)</param>
		/// <returns></returns>
		public List<GraphEntry> Task_RetrieveProgress(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "Task_RetrieveProgress";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view tasks
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now get the task progress graph information
				List<GraphEntry> graphEntries = new List<GraphEntry>();

				//See if we have a release (works differently)
				if (releaseId.HasValue)
				{
					ReleaseView release = new ReleaseManager().RetrieveById(User.UserInternal, projectId, releaseId.Value);
					if (release != null)
					{
						//On Time
						GraphEntry graphEntry = new GraphEntry();
						graphEntry.Name = "TaskPercentOnTime";
						graphEntry.Caption = Resources.Main.Task_OnSchedule;
						graphEntry.Count = release.TaskPercentOnTime;
						graphEntry.Color = TaskManager.GetProgressColor(1);
						graphEntries.Add(graphEntry);

						//Late Finish
						graphEntry = new GraphEntry();
						graphEntry.Name = "TaskPercentLateFinish";
						graphEntry.Caption = Resources.Main.Task_RunningLate;
						graphEntry.Count = release.TaskPercentLateFinish;
						graphEntry.Color = TaskManager.GetProgressColor(2);
						graphEntries.Add(graphEntry);

						//Late Start
						graphEntry = new GraphEntry();
						graphEntry.Name = "TaskPercentLateStart";
						graphEntry.Caption = Resources.Main.Task_StartingLate;
						graphEntry.Count = release.TaskPercentLateStart;
						graphEntry.Color = TaskManager.GetProgressColor(3);
						graphEntries.Add(graphEntry);

						//Not Started
						graphEntry = new GraphEntry();
						graphEntry.Name = "TaskPercentNotStart";
						graphEntry.Caption = Resources.Main.Task_NotStarted;
						graphEntry.Count = release.TaskPercentNotStart;
						graphEntry.Color = TaskManager.GetProgressColor(4);
						graphEntries.Add(graphEntry);
					}
				}
				else
				{
					ProjectTaskProgressEntryView taskProjectSummary = new TaskManager().RetrieveProjectProgressSummary(projectId);
					if (taskProjectSummary != null)
					{
						//On Time
						GraphEntry graphEntry = new GraphEntry();
						graphEntry.Name = "TaskPercentOnTime";
						graphEntry.Caption = Resources.Main.Task_OnSchedule;
						graphEntry.Count = taskProjectSummary.TaskPercentOnTime.Value;
						graphEntry.Color = TaskManager.GetProgressColor(1);
						graphEntries.Add(graphEntry);

						//Late Finish
						graphEntry = new GraphEntry();
						graphEntry.Name = "TaskPercentLateFinish";
						graphEntry.Caption = Resources.Main.Task_RunningLate;
						graphEntry.Count = taskProjectSummary.TaskPercentLateFinish.Value;
						graphEntry.Color = TaskManager.GetProgressColor(2);
						graphEntries.Add(graphEntry);

						//Late Start
						graphEntry = new GraphEntry();
						graphEntry.Name = "TaskPercentLateStart";
						graphEntry.Caption = Resources.Main.Task_StartingLate;
						graphEntry.Count = taskProjectSummary.TaskPercentLateStart.Value;
						graphEntry.Color = TaskManager.GetProgressColor(3);
						graphEntries.Add(graphEntry);

						//Not Started
						graphEntry = new GraphEntry();
						graphEntry.Name = "TaskPercentNotStart";
						graphEntry.Caption = Resources.Main.Task_NotStarted;
						graphEntry.Count = taskProjectSummary.TaskPercentNotStart.Value;
						graphEntry.Color = TaskManager.GetProgressColor(4);
						graphEntries.Add(graphEntry);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return graphEntries;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns the data needed for the simple task burndown graph on the list page
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release (optional)</param>
		/// <returns></returns>
		public GraphData Task_RetrieveBurndown(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "Task_RetrieveBurndown";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view tasks
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now get the requirements test coverage list
				List<GraphEntry> graphEntries = new List<GraphEntry>();
				DataSet graphDataSet = new GraphManager().RetrieveTaskBurndown(projectId, releaseId);

				if (graphDataSet == null)
				{
					return null;
				}

				//Create the graph data
				GraphData graphData = new GraphData();

				//Generate the number of different data-series
				DataTable dataTable = graphDataSet.Tables[0];
				foreach (DataColumn dataColumn in dataTable.Columns)
				{
					//We don't add the key columns as they are used for the axis
					if (dataTable.PrimaryKey.Contains(dataColumn))
					{
						graphData.XAxisCaption = dataColumn.Caption;
					}
					else
					{
						DataSeries series = new DataSeries();
						series.Name = dataColumn.ColumnName;
						series.Caption = dataColumn.Caption;
						//See if a color is included in the series
						if (dataColumn.ExtendedProperties.ContainsKey("Color"))
						{
							series.Color = (string)dataColumn.ExtendedProperties["Color"];
						}

						//See if a style of series is specified (defaults to bar if not specified)
						series.Type = (int)Graph.GraphSeriesTypeEnum.Bar;
						if (dataColumn.ExtendedProperties.ContainsKey("Type"))
						{
							series.Type = (int)dataColumn.ExtendedProperties["Type"];
						}

						graphData.Series.Add(series);
					}
				}

				//Populate the data items
				foreach (DataRow dataRow in dataTable.Rows)
				{
					//First we need to add the actual data x-axis values
					//Get the key column
					DataColumn dataColumn = dataTable.PrimaryKey[0];
					GraphAxisPosition axisPosition = new GraphAxisPosition();
					axisPosition.Id = dataTable.Rows.IndexOf(dataRow);
					axisPosition.StringValue = ((string)dataRow[dataColumn]);
					graphData.XAxis.Add(axisPosition);

					//Now add the data series
					foreach (DataSeries series in graphData.Series)
					{
						if (dataRow[series.Name] != null)
						{
							object value = dataRow[series.Name];
							if (value.GetType() == typeof(decimal))
							{
								series.Values.Add(axisPosition.Id.ToString(), Decimal.Round((decimal)value, 1));
							}
							else if (value.GetType() == typeof(double))
							{
								double doubleValue = (double)value;
								series.Values.Add(axisPosition.Id.ToString(), Decimal.Round(new Decimal(doubleValue), 1));
							}
							else if (value.GetType() == typeof(int))
							{
								int intValue = (int)value;
								series.Values.Add(axisPosition.Id.ToString(), (decimal)intValue);
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return graphData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns the task progress graph data for an entire project group
		/// </summary>
		/// <param name="activeReleasesOnly">do we only want the active releases' information</param>
		/// <param name="projectGroupId"></param>
		/// <returns></returns>
		public List<GraphEntry> Task_RetrieveGroupProgress(int projectGroupId, bool activeReleasesOnly)
		{
			const string METHOD_NAME = "Task_RetrieveGroupProgress";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized for this group
			ProjectGroupManager projectGroupManager = new ProjectGroupManager();
			if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now get the task progress graph information
				List<GraphEntry> graphEntries = new List<GraphEntry>();
				List<Task_GroupSummary> taskGroupSummary = new TaskManager().RetrieveProgressSummary(projectGroupId, activeReleasesOnly);
				if (taskGroupSummary != null)
				{
					foreach (Task_GroupSummary entry in taskGroupSummary)
					{
						if (entry.TaskCount.HasValue)
						{
							GraphEntry graphEntry = new GraphEntry();
							graphEntry.Name = entry.ProgressOrderId.ToString();
							graphEntry.Caption = entry.ProgressCaption;
							graphEntry.Count = entry.TaskCount.Value;
							graphEntry.Color = TaskManager.GetProgressColor(entry.ProgressOrderId);
							graphEntries.Add(graphEntry);
						}
					}
				}


				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return graphEntries;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Splits a task into two smaller tasks
		/// </summary>
		/// <param name="effortPercentage">The % of the effort values to assign to the NEW task (null uses the auto calculation)</param>
		/// <param name="ownerId">The owner of the NEW task, leaving null uses the existing task's owner</param>
		/// <param name="taskId">The id of the task to split</param>
		/// <param name="name">The name of the new task</param>
		/// <param name="comment">The comment to add to the association between the two tasks (optional)</param>
		/// <param name="projectId">The id of the current project</param>
		/// <returns></returns>
		public int Task_Split(int projectId, int taskId, string name, int? effortPercentage, int? ownerId, string comment)
		{
			const string METHOD_NAME = "Task_Split";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (modify owned (limited) is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				TaskManager taskManager = new TaskManager();

				//First retrieve the task and make sure it exists and is in the specified project
				Task task = taskManager.RetrieveById(taskId);
				if (task == null)
				{
					throw new ArtifactNotExistsException(Resources.Messages.TaskDetails_ArtifactNotExists);
				}
				if (task.ProjectId != projectId)
				{
					throw new Exception(Resources.Messages.Services_ItemNotBelongToProject);
				}

				//Make sure we have a valid name
				if (String.IsNullOrEmpty(name))
				{
					throw new Exception(String.Format(Resources.Messages.ListServiceBase_FieldRequired, Resources.Fields.Name));
				}

				//Now do the split and capture the new ID
				int newTaskId = taskManager.Split(taskId, name, userId, effortPercentage, ownerId, comment);
				return newTaskId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Retrieves a list of tasks that are related to a specific test run</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunId">The id of the test run</param>
		/// <returns>List of data items</returns>
		public SortedData Task_RetrieveByTestRunId(int projectId, int testRunId)
		{
			const string METHOD_NAME = "Task_RetrieveByTestRunId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the array of data items
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				TaskManager taskManager = new TaskManager();
				List<TaskView> tasks = taskManager.RetrieveByTestRunId(projectId, testRunId);


				//Populate the data items
				foreach (TaskView task in tasks)
				{
					SortedDataItem dataItem = new SortedDataItem();
					dataItems.Add(dataItem);
					dataItem.PrimaryKey = task.TaskId;

					//The TaskId field
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "TaskId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItemField.IntValue = task.TaskId;
					dataItemField.TextValue = task.ArtifactToken;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The TaskStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "TaskStatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItemField.IntValue = task.TaskStatusId;
					dataItemField.TextValue = task.TaskStatusName;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The TaskTypeId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "TaskTypeId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItemField.IntValue = task.TaskTypeId;
					dataItemField.TextValue = task.TaskTypeName;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The PriorityId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "TaskPriorityId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
					if (task.TaskPriorityId.HasValue)
					{
						dataItemField.IntValue = task.TaskPriorityId;
						dataItemField.TextValue = task.TaskPriorityName;
					}
					dataItemField.CssClass = task.TaskPriorityColor;

					//The Name field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.NameDescription;
					dataItemField.TextValue = task.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The CreationDate field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "CreationDate";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.DateTime;
					dataItemField.DateValue = task.CreationDate;
					dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, task.CreationDate);
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OpenerId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "CreatorId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItemField.IntValue = task.CreatorId;
					dataItemField.TextValue = task.CreatorName;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OwnerId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OwnerId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
					if (task.OwnerId.HasValue)
					{
						dataItemField.IntValue = task.OwnerId.Value;
						dataItemField.TextValue = task.OwnerName;
					}
				}

				return sortedData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

		}

		/// <summary>
		/// Counts the number of tasks (used to display the asterisk in tabs)
		/// </summary>
		/// <param name="projectId">The project id</param>
		/// <param name="artifact">The artifact we want the tasks for</param>
		/// <returns>The count</returns>
		public int Task_Count(int projectId, ArtifactReference artifact)
		{
			const string METHOD_NAME = "Task_Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view tasks
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			//Limited OK because we need to display the 'has data' in tabs
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Depending on the artifact that these incidents are for (e.g. requirement)
				//we need to set the grid properties accordingly and also indicate if we have any data
				int taskCount = 0;
				if (artifact.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement)
				{
					//Get the count of tasks (filtered)
					Hashtable taskFilters = new Hashtable();
					taskFilters.Add("RequirementId", artifact.ArtifactId);
					taskCount = new TaskManager().Count(projectId, taskFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				}
				if (artifact.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Risk)
				{
					//Get the count of tasks (filtered)
					Hashtable taskFilters = new Hashtable();
					taskFilters.Add("RiskId", artifact.ArtifactId);
					taskCount = new TaskManager().Count(projectId, taskFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				}

				return taskCount;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the list of releases for a specific requirement, or all releases in the project if no requirement specified
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="requirementId">The current requirement</param>
		/// <returns>Dictionary that can be used to populate a dropdown list</returns>
		public JsonDictionaryOfStrings GetReleasesForTaskRequirement(int projectId, int? requirementId)
		{
			const string METHOD_NAME = "GetReleasesForTaskRequirement";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited view is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Release);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				ReleaseManager releaseManager = new ReleaseManager();
				JsonDictionaryOfStrings lookupValues = null;
				if (requirementId.HasValue)
				{
					try
					{
						RequirementView requirementView = new RequirementManager().RetrieveById2(projectId, requirementId.Value);
						if (requirementView != null && requirementView.ReleaseId.HasValue)
						{
							List<ReleaseView> releases = new ReleaseManager().RetrieveSelfAndIterations(projectId, requirementView.ReleaseId.Value, true);
							lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
						}
					}
					catch (ArtifactNotExistsException)
					{
						//Ignore and just display the full release list
					}
				}
				if (lookupValues == null)
				{
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false);
					lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
				}
				return lookupValues;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region IFormService methods

		/// <summary>
		/// Deletes the current task and returns the ID of the item to redirect to (if any)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <returns>The id to redirect to</returns>
		public override int? Form_Delete(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to delete the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//First we need to determine which task id to redirect the user to after the delete
				int? newTaskId = null;

				//Look through the current dataset to see what is the next task in the list
				//If we are the last one on the list then we need to simply use the one before

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST);

				//Get the sort information
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TaskId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_TASK_PAGINATION_SIZE);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 500;
				int currentPage = 1;
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE] != null)
				{
					paginationSize = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE];
				}
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] != null)
				{
					currentPage = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE];
				}
				//Get the number of tasks in the project
				TaskManager taskManager = new TaskManager();
				int artifactCount = taskManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				//Get the tasks list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}

				List<TaskView> taskNavigationList = taskManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				bool matchFound = false;
				int previousTaskId = -1;
				foreach (TaskView task in taskNavigationList)
				{
					int testTaskId = task.TaskId;
					if (testTaskId == artifactId)
					{
						matchFound = true;
					}
					else
					{
						//If we found a match on the previous iteration, then we want to this (next) task
						if (matchFound)
						{
							newTaskId = testTaskId;
							break;
						}

						//If this matches the current task, set flag
						if (testTaskId == artifactId)
						{
							matchFound = true;
						}
						if (!matchFound)
						{
							previousTaskId = testTaskId;
						}
					}
				}
				if (!newTaskId.HasValue && previousTaskId != -1)
				{
					newTaskId = previousTaskId;
				}

				//Next we need to delete the current task
				taskManager.MarkAsDeleted(projectId, artifactId, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return newTaskId;
			}
			catch (ArtifactNotExistsException)
			{
				//The item does not exist, so return null
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}


		/// <summary>
		/// Creates a new task and returns it to the form
		/// </summary>
		/// <param name="artifactId">The id of the task we were on</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The id of the new task</returns>
		public override int? Form_New(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_New";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to create the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Get the existing artifact and get its folder to insert in
				TaskManager taskManager = new TaskManager();
				Task task;
				int? folderId = null;
				try
				{
					task = taskManager.RetrieveById(artifactId);
					folderId = task.TaskFolderId;
				}
				catch (ArtifactNotExistsException)
				{
					//Ignore, leave indent level as null;
				}

				//Now we need to create the task and then navigate to it
				int taskId = taskManager.Insert(
					projectId,
					userId,
					Task.TaskStatusEnum.NotStarted,
					null,
					folderId,
					null,
					null,
					null,
					null,
					"",
					"",
					null,
					null,
					null,
					null,
					null,
					userId
					);
				task = taskManager.RetrieveById(taskId);

				//We now need to populate the appropriate default custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId, Artifact.ArtifactTypeEnum.Task, true);
				if (task != null)
				{
					//If the artifact custom property row is null, create a new one and populate the defaults
					if (artifactCustomProperty == null)
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, taskId, customProperties);
						artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					//Save the custom properties
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return taskId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Clones the current task and returns the ID of the item to redirect to
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <returns>The id to redirect to</returns>
		public override int? Form_Clone(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_Clone";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to create the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now we need to copy the task and then navigate to it
				TaskManager task = new TaskManager();
				int newTaskId = task.Copy(userId, artifactId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newTaskId;
			}
			catch (ArtifactNotExistsException)
			{
				//The item does not exist, so return null
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Returns a single task data record (all columns) for use by the FormManager control</summary>
		/// <param name="artifactId">The id of the current task</param>
		/// <returns>A task data item</returns>
		public DataItem Form_Retrieve(int projectId, int? artifactId)
		{
			const string METHOD_NAME = CLASS_NAME + "Form_Retrieve";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited edit or full edit)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business classes
				TaskManager taskManager = new TaskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				TaskWorkflowManager workflowManager = new TaskWorkflowManager();

				//Create the data item record (no filter items)
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

				//Need to add the empty column to capture any new comments added
				if (!dataItem.Fields.ContainsKey("NewComment"))
				{
					dataItem.Fields.Add("NewComment", new DataItemField() { FieldName = "NewComment", Required = false, Editable = true, Hidden = false });
				}

				//Get the task for the specific task id or just create a temporary one
				TaskView taskView;
				int ownerId = -1;
				ArtifactCustomProperty artifactCustomProperty;
				if (artifactId.HasValue)
				{
					//Get the task for the specific task id
					taskView = taskManager.TaskView_RetrieveById(artifactId.Value);

					//The main dataset does not have the custom properties, they need to be retrieved separately
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, Artifact.ArtifactTypeEnum.Task, true);

					//Make sure the user is authorized for this item
					if (taskView.OwnerId.HasValue)
					{
						ownerId = taskView.OwnerId.Value;
					}
					if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && taskView.CreatorId != userId)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
				}
				else
				{
					//Insert Case, need to create the new task - only occurs in the task board
					taskView = taskManager.Task_New(projectId, userId, null);

					//Also we need to populate any default Artifact Custom Properties
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, true, false);
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}

				//Get the list of workflow fields and custom properties
				int workflowId = workflowManager.Workflow_GetForTaskType(taskView.TaskTypeId);
				int statusId = taskView.TaskStatusId;
				List<TaskWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusId);
				List<TaskWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusId);

				//See if we have any existing artifact custom properties for this row
				if (artifactCustomProperty == null)
				{
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, true, false);
					PopulateRow(dataItem, taskView, customProperties, true, (ArtifactCustomProperty)null, workflowFields, workflowCustomProps);
				}
				else
				{
					PopulateRow(dataItem, taskView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, workflowFields, workflowCustomProps);
				}

				//The New Comments field is not part of the entity so needs to be handled separately for workflow
				if (dataItem.Fields.ContainsKey("NewComment"))
				{
					DataItemField newCommentField = dataItem.Fields["NewComment"];
					if (workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
					{
						newCommentField.Required = true;
					}
					if (workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
					{
						newCommentField.Hidden = true;
					}
					if (!workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
					{
						newCommentField.Editable = true;
					}
				}

				//Also need to return back a special field to denote if the user is the owner or creator of the artifact
				bool isArtifactCreatorOrOwner = (ownerId == userId || taskView.CreatorId == userId);
				dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });

				//Populate any data mapping values are not part of the standard 'shape'
				if (artifactId.HasValue)
				{
					DataMappingManager dataMappingManager = new DataMappingManager();
					List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Task, artifactId.Value);
					foreach (DataSyncArtifactMapping artifactMapping in artifactMappings)
					{
						DataItemField dataItemField = new DataItemField();
						dataItemField.FieldName = DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId;
						dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
						if (String.IsNullOrEmpty(artifactMapping.ExternalKey))
						{
							dataItemField.TextValue = "";
						}
						else
						{
							dataItemField.TextValue = artifactMapping.ExternalKey;
						}
						dataItemField.Editable = (SpiraContext.Current.IsProjectAdmin); //Read-only unless project admin
						dataItemField.Hidden = false;   //Always visible
						dataItem.Fields.Add(DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId, dataItemField);
					}

					//Populate the folder path as a special field
					if (taskView.TaskFolderId.HasValue)
					{
						List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskView.ProjectId, taskView.TaskFolderId.Value, true);
						string pathArray = "[";
						bool isFirst = true;
						foreach (TaskFolderHierarchyView parentFolder in parentFolders)
						{
							if (isFirst)
							{
								isFirst = false;
							}
							else
							{
								pathArray += ",";
							}
							pathArray += "{ \"name\": \"" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "\", \"id\": " + parentFolder.TaskFolderId + " }";
						}
						pathArray += "]";
						dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath", TextValue = pathArray });
					}
					else
					{
						//send a blank folder path object back so client knows this artifact has folders
						dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath" });
					}
				}

				//If we have a release/iteration set, also display the release/iteration date range
				//Add the 'ReleasesDates' field that holds the date range of the release in parenthesis
				string releaseDates = "";
				if (taskView.ReleaseId.HasValue)
				{
					//Get the release record
					try
					{
						ReleaseView release = new ReleaseManager().RetrieveById2(projectId, taskView.ReleaseId.Value);
						if (release != null)
						{
							releaseDates = "(" + String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, GlobalFunctions.LocalizeDate(release.StartDate)) + " - " + String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, GlobalFunctions.LocalizeDate(release.EndDate)) + ")";
						}
					}
					catch (ArtifactNotExistsException)
					{
						//Do nothing
					}
				}
				DataItemField dataItemField2 = new DataItemField();
				dataItemField2.FieldName = "ReleasesDates";
				dataItemField2.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
				dataItemField2.TextValue = releaseDates;
				dataItemField2.Editable = false;
				dataItemField2.Hidden = false;
				dataItem.Fields.Add(dataItemField2.FieldName, dataItemField2);

				Logger.LogExitingEvent(METHOD_NAME);
				Logger.Flush();

				return dataItem;
			}
			catch (ArtifactNotExistsException)
			{
				//Just return no data back
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Saves a single task data item</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The task to save</param>
		/// <param name="operation">The type of save operation ('new', 'close', '', etc.)</param>
		/// <returns>Any error message or null if successful</returns>
		/// <param name="signature">Any digital signature</param>
		public List<ValidationMessage> Form_Save(int projectId, DataItem dataItem, string operation, Signature signature)
		{
			const string METHOD_NAME = CLASS_NAME + "Form_Save";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The return list..
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited is OK, we check that later)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Get the task id
			int taskId = dataItem.PrimaryKey;

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business classes
				TaskManager taskManager = new TaskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				TaskWorkflowManager workflowManager = new TaskWorkflowManager();

				//Load the custom property definitions (once, not per artifact)
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);

				//If we have a zero/negative primary key it means that it's actually a new item being inserted
				Task task;
				ArtifactCustomProperty artifactCustomProperty;
				if (taskId < 1)
				{
					//Insert Case, need to use the Task_New() method since we need the mandatory fields populated
					TaskView taskView = taskManager.Task_New(projectId, userId, null);
					//Convert from the read-only view to the read/write entity
					task = taskView.ConvertTo<TaskView, Task>();

					//Also we need to populate any default Artifact Custom Properties
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}
				else
				{
					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					task = taskManager.RetrieveById(taskId);

					//Make sure the user is authorized for this item if they only have limited permissions
					int ownerId = -1;
					if (task.OwnerId.HasValue)
					{
						ownerId = task.OwnerId.Value;
					}
					if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && task.CreatorId != userId)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Create a new artifact custom property row if one doesn't already exist
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId, Artifact.ArtifactTypeEnum.Task, false, customProperties);
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, taskId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}
				}

				//For saving, need to use the current status and type of the dataItem which may be different to the one retrieved
				int currentStatusId = (dataItem.Fields["TaskStatusId"].IntValue.HasValue) ? dataItem.Fields["TaskStatusId"].IntValue.Value : -1;
				int originalStatusId = task.TaskStatusId;
				int taskTypeId = (dataItem.Fields["TaskTypeId"].IntValue.HasValue) ? dataItem.Fields["TaskTypeId"].IntValue.Value : -1;

				//Get the list of workflow fields and custom properties
				int workflowId;
				if (taskTypeId < 1)
				{
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).TaskWorkflowId;
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForTaskType(taskTypeId);
				}
				List<TaskWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, currentStatusId);
				List<TaskWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, currentStatusId);

				//Convert the workflow lists into the type expected by the ListServiceBase function
				List<WorkflowField> workflowFields2 = TaskWorkflowManager.ConvertFields(workflowFields);
				List<WorkflowCustomProperty> workflowCustomProps2 = TaskWorkflowManager.ConvertFields(workflowCustomProps);

				//If the workflow status changed, check to see if we need a digital signature and if it was provided and is valid
				if (currentStatusId != originalStatusId)
				{
					//Only attempt to verify signature requirements if we have no concurrency date or if the client side concurrency matches that from the DB
					bool shouldVerifyDigitalSignature = true;
					if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
					{
						DateTime concurrencyDateTimeValue;
						if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
						{
							shouldVerifyDigitalSignature = task.ConcurrencyDate == concurrencyDateTimeValue;
						}
					}

					if (shouldVerifyDigitalSignature)
					{
						bool? valid = VerifyDigitalSignature(workflowId, originalStatusId, currentStatusId, signature, task.CreatorId, task.OwnerId);
						if (valid.HasValue)
						{
							if (valid.Value)
							{
								//Add the meaning to the artifact so that it can be recorded
								task.SignatureMeaning = signature.Meaning;
							}
							else
							{
								//Let the user know that the digital signature is not valid
								return CreateSimpleValidationMessage(Resources.Messages.Services_DigitalSignatureNotValid);
							}
						}
					}
				}

				//Need to set the original date of this record to match the concurrency date
				//The value is already in UTC so no need to convert
				if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
				{
					DateTime concurrencyDateTimeValue;
					if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
					{
						task.ConcurrencyDate = concurrencyDateTimeValue;
						task.AcceptChanges();
					}
				}

				//Now we can start tracking any changes
				task.StartTracking();

				//Update the field values, tracking changes
				List<string> fieldsToIgnore = new List<string>();
				fieldsToIgnore.Add("NewComment");
				fieldsToIgnore.Add("Comments");
				fieldsToIgnore.Add("ReleasesDates");
				fieldsToIgnore.Add("CreationDate");
				fieldsToIgnore.Add("LastUpdateDate");   //Breaks concurrency otherwise

				//Need to handle any data-mapping fields (project-admin only)
				if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
				{
					DataMappingManager dataMappingManager = new DataMappingManager();
					List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Task, taskId);
					foreach (KeyValuePair<string, DataItemField> kvp in dataItem.Fields)
					{
						DataItemField dataItemField = kvp.Value;
						if (dataItemField.FieldName.SafeSubstring(0, DataMappingManager.FIELD_PREPEND.Length) == DataMappingManager.FIELD_PREPEND)
						{
							//See if we have a matching row
							foreach (DataSyncArtifactMapping artifactMapping in artifactMappings)
							{
								if (DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId == dataItemField.FieldName)
								{
									artifactMapping.StartTracking();
									if (String.IsNullOrWhiteSpace(dataItemField.TextValue))
									{
										artifactMapping.ExternalKey = null;
									}
									else
									{
										artifactMapping.ExternalKey = dataItemField.TextValue;
									}
								}
							}
						}
					}

					//Now save the data
					dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);
				}

				//Update the field values
				UpdateFields(validationMessages, dataItem, task, customProperties, artifactCustomProperty, projectId, taskId, 0, fieldsToIgnore, workflowFields2, workflowCustomProps2);

				//Check to see if a comment was required and if so, verify it was provided. It's not handled as part of 'UpdateFields'
				//because there is no Comments field on the Task entity
				if (workflowFields != null && workflowFields.Any(w => w.ArtifactField.Name == "Comments" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
				{
					//Comment is required, so check that it's present
					if (String.IsNullOrWhiteSpace(dataItem.Fields["NewComment"].TextValue))
					{
						AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = "NewComment", Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, Resources.Fields.Comment) });
					}
				}

				//Now verify the options for the custom properties to make sure all rules have been followed
				Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
				foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
				{
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = customPropOptionMessage.Key;
					newMsg.Message = customPropOptionMessage.Value;
					AddUniqueMessage(validationMessages, newMsg);
				}

				//Perform any business level validations on the datarow
				Dictionary<string, string> businessValidationMessages = taskManager.Validate(task);
				foreach (KeyValuePair<string, string> businessValidationMessage in businessValidationMessages)
				{
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = businessValidationMessage.Key;
					newMsg.Message = businessValidationMessage.Value;
					AddUniqueMessage(validationMessages, newMsg);
				}

				//If we have validation messages, stop now
				if (validationMessages.Count > 0)
				{
					return validationMessages;
				}

				//Get copies of everything..
				Artifact notificationArt = task.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Either insert or update the task and cystom properties
				if (taskId < 1)
				{
					//Handle enums
					Task.TaskStatusEnum taskStatus = (Task.TaskStatusEnum)task.TaskStatusId;

					//Submit the new task in the root folder
					taskId = taskManager.Insert(
						projectId,
						userId,
						taskStatus,
						task.TaskTypeId,
						task.TaskFolderId,
						task.RequirementId,
						task.ReleaseId,
						task.OwnerId,
						task.TaskPriorityId,
						task.Name,
						task.Description,
						task.StartDate,
						task.EndDate,
						task.EstimatedEffort,
						task.ActualEffort,
						task.RemainingEffort,
						userId
						);

					//Now save the custom properties
					artifactCustomProperty.ArtifactId = taskId;
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//We don't need to worry about attachments for new requirements added through the planning board

					//We need to encode the new artifact id as a 'pseudo' validation message
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "$NewArtifactId";
					newMsg.Message = taskId.ToString();
					AddUniqueMessage(validationMessages, newMsg);

					//Add the ID for notifications to work (in the planning board insert case only)
					((Task)notificationArt).MarkAsAdded();
					((Task)notificationArt).TaskId = taskId;
				}
				else
				{
					try
					{
						taskManager.Update(task, userId);
					}
					catch (TaskDateOutOfBoundsException)
					{
						return CreateSimpleValidationMessage(Resources.Messages.TaskDetails_DatesOutsideReleaseDates);
					}
					catch (OptimisticConcurrencyException)
					{
						return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
					}
					catch (EntityForeignKeyException)
					{
						return CreateSimpleValidationMessage(Resources.Messages.Global_DependentArtifactDeleted);
					}
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				//See if we have a new comment encoded in the list of fields
				string notificationComment = null;
				if (dataItem.Fields.ContainsKey("NewComment"))
				{
					string newComment = dataItem.Fields["NewComment"].TextValue;

					if (!String.IsNullOrWhiteSpace(newComment))
					{
						new DiscussionManager().Insert(userId, taskId, Artifact.ArtifactTypeEnum.Task, newComment, DateTime.UtcNow, projectId, false, false);
						notificationComment = newComment;
					}
				}

				//Call notifications..
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, notificationComment);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Task #" + task.TaskId + ".");
				}

				//If we're asked to save and create a new task, need to do the insert and send back the new id
				if (operation == "new")
				{
					//Get the values from the existing task that we want to set on the new one (not status)
					//Now we need to create a new task in the list and then navigate to it
					int newTaskId = taskManager.Insert(
						projectId,
						userId,
						Task.TaskStatusEnum.NotStarted,
						task.TaskTypeId,
						task.TaskFolderId,
						task.RequirementId,
						task.ReleaseId,
						task.OwnerId,
						task.TaskPriorityId,
						"",
						null,
						null,
						null,
						null,
						null,
						null,
						userId);

					//We need to populate any custom property default values
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, newTaskId, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//We need to encode the new artifact id as a 'pseudo' validation message
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "$NewArtifactId";
					newMsg.Message = newTaskId.ToString();
					AddUniqueMessage(validationMessages, newMsg);
				}

				//Return back any messages. For success it should only contain a new artifact ID if we're inserting
				return validationMessages;
			}
			catch (ArtifactNotExistsException)
			{
				//Let the user know that the ticket no inter exists
				return CreateSimpleValidationMessage(String.Format(Resources.Messages.TasksService_TaskNotFound, taskId));
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns the list of workflow field states separate from the main retrieve (used when changing workflow only)
		/// </summary>
		/// <param name="typeId">The id of the current task type</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="stepId">The id of the current step/status</param>
		/// <returns>The list of workflow states only</returns>
		public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
		{
			const string METHOD_NAME = "Form_RetrieveWorkflowFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				List<DataItemField> dataItemFields = new List<DataItemField>();

				//Get the list of artifact fields and custom properties
				ArtifactManager artifactManager = new ArtifactManager();
				List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Task);
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);

				//Get the list of workflow fields and custom properties for the specified type and step
				TaskWorkflowManager workflowManager = new TaskWorkflowManager();
				int workflowId = workflowManager.Workflow_GetForTaskType(typeId);
				List<TaskWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, stepId);
				List<TaskWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, stepId);

				//First the standard fields
				foreach (ArtifactField artifactField in artifactFields)
				{
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = artifactField.Name;
					dataItemFields.Add(dataItemField);

					//Set the workflow state
					//Specify which fields are editable or required
					dataItemField.Editable = true;
					dataItemField.Required = false;
					dataItemField.Hidden = false;
					if (workflowFields != null)
					{
						if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
						{
							dataItemField.Editable = false;
						}
						if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						{
							dataItemField.Required = true;
						}
						if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
						{
							dataItemField.Hidden = true;
						}
					}
				}

				//Now the custom properties
				foreach (CustomProperty customProperty in customProperties)
				{
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = customProperty.CustomPropertyFieldName;
					dataItemFields.Add(dataItemField);

					//Set the workflow state
					//Specify which fields are editable or required
					dataItemField.Editable = true;
					dataItemField.Required = false;
					dataItemField.Hidden = false;

					//First see if the custom property is required due to its definition
					if (customProperty.Options != null)
					{
						CustomPropertyOptionValue customPropOptionValue = customProperty.Options.FirstOrDefault(co => co.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty);
						if (customPropOptionValue != null)
						{
							bool? allowEmpty = customPropOptionValue.Value.FromDatabaseSerialization_Boolean();
							if (allowEmpty.HasValue)
							{
								dataItemField.Required = !allowEmpty.Value;
							}
						}
					}

					//Now check the workflow states
					if (workflowCustomProps != null)
					{
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
						{
							dataItemField.Editable = false;
						}
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						{
							dataItemField.Required = true;
						}
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
						{
							dataItemField.Hidden = true;
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItemFields;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region ICommentService Methods

		/// <summary>
		/// Retrieves the list of comments associated with a task
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the task</param>
		/// <returns>The list of comments</returns>
		public List<CommentItem> Comment_Retrieve(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Comment_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the new list of comments
				List<CommentItem> commentItems = new List<CommentItem>();

				//Get the task (to verify permissions) and also the comments
				TaskManager taskManager = new TaskManager();
				UserManager userManager = new UserManager();
				DiscussionManager discussion = new DiscussionManager();
				Task task = taskManager.RetrieveById(artifactId);
				List<IDiscussion> comments = discussion.Retrieve(artifactId, Artifact.ArtifactTypeEnum.Task).ToList();

				//Make sure the user is either the owner or author if limited permissions
				int ownerId = -1;
				if (task.OwnerId.HasValue)
				{
					ownerId = task.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && task.CreatorId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//See if we're sorting ascending or descending
				SortDirection sortDirection = (SortDirection)GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)SortDirection.Descending);

				int startIndex;
				int increment;
				if (sortDirection == SortDirection.Ascending)
				{
					startIndex = 0;
					increment = 1;
				}
				else
				{
					startIndex = comments.Count - 1;
					increment = -1;
				}
				for (var i = startIndex; (increment == 1 && i < comments.Count) || (increment == -1 && i >= 0); i += increment)
				{
					IDiscussion discussionRow = comments[i];
					//Add a new comment
					CommentItem commentItem = new CommentItem();
					commentItem.primaryKey = discussionRow.DiscussionId;
					commentItem.text = discussionRow.Text;
					commentItem.creatorId = discussionRow.CreatorId;
					commentItem.creatorName = discussionRow.CreatorName;
					commentItem.creationDate = GlobalFunctions.LocalizeDate(discussionRow.CreationDate);
					commentItem.creationDateText = GlobalFunctions.LocalizeDate(discussionRow.CreationDate).ToNiceString(GlobalFunctions.LocalizeDate(DateTime.UtcNow));
					commentItem.sortDirection = (int)sortDirection;

					//Specify if the user can delete the item
					if (!discussionRow.IsPermanent && (discussionRow.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)))
					{
						commentItem.deleteable = true;
					}
					else
					{
						commentItem.deleteable = false;
					}

					commentItems.Add(commentItem);
				}

				//Return the comments
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return commentItems;
			}
			catch (ArtifactNotExistsException)
			{
				//The incident doesn't exist, so just return null
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates the sort direction of the comments list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="sortDirectionId">The new direction for the sort</param>
		public void Comment_UpdateSortDirection(int projectId, int sortDirectionId)
		{
			const string METHOD_NAME = "Comment_UpdateSortDirection";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Update the setting
				SortDirection sortDirection = (SortDirection)sortDirectionId;
				SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)sortDirectionId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a specific comment in the comment list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="commentId">The id of the comment</param>
		/// <param name="artifactId">The id of the task</param>
		public void Comment_Delete(int projectId, int artifactId, int commentId)
		{
			const string METHOD_NAME = "Comment_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Delete the comment, making sure we have permissions
				DiscussionManager discussion = new DiscussionManager();
				IDiscussion comment = discussion.RetrieveById(commentId, Artifact.ArtifactTypeEnum.Task);
				//If the comment no longer exists do nothing
				if (comment != null && !comment.IsPermanent)
				{
					if (comment.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
					{
						discussion.DeleteDiscussionId(commentId, Artifact.ArtifactTypeEnum.Task);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Adds a comment to an artifact
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="comment">The comment being added</param>
		/// <returns>The id of the newly added comment</returns>
		public int Comment_Add(int projectId, int artifactId, string comment)
		{
			const string METHOD_NAME = "Comment_Add";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view the item (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Make sure we're allowed to add comments
			if (IsAuthorizedToAddComments(projectId) == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Add the comment
				string cleanedComment = GlobalFunctions.HtmlScrubInput(comment);
				DiscussionManager discussion = new DiscussionManager();
				int commentId = discussion.Insert(userId, artifactId, Artifact.ArtifactTypeEnum.Task, cleanedComment, projectId, false, true);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return commentId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region ISortedList methods

		/// <summary>
		/// Updates records of data in the system
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dataItems">The updated data records</param>
		/// <returns>The list of any validation messages</returns>
		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			const string METHOD_NAME = "SortedList_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Used to store any validation messages
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Iterate through each data item and make the updates
				TaskManager taskManager = new TaskManager();
				//Load the custom property definitions (once, not per artifact)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);

				foreach (SortedDataItem dataItem in dataItems)
				{
					//Get the task id
					int taskId = dataItem.PrimaryKey;

					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					Task task = taskManager.RetrieveById(taskId);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId, Artifact.ArtifactTypeEnum.Task, false, customProperties);

					//Create a new artifact custom property row if one doesn't already exist
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, taskId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					if (task != null)
					{
						//Need to set the original date of this record to match the concurrency date
						if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
						{
							DateTime concurrencyDateTimeValue;
							if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
							{
								task.ConcurrencyDate = concurrencyDateTimeValue;
								task.AcceptChanges();
							}
						}

						//Update the field values
						List<string> fieldsToIgnore = new List<string>();
						fieldsToIgnore.Add("CreationDate");
						UpdateFields(validationMessages, dataItem, task, customProperties, artifactCustomProperty, projectId, taskId, 0, fieldsToIgnore);

						//Now verify the options for the custom properties to make sure all rules have been followed
						Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
						foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
						{
							ValidationMessage newMsg = new ValidationMessage();
							newMsg.FieldName = customPropOptionMessage.Key;
							newMsg.Message = customPropOptionMessage.Value;
							AddUniqueMessage(validationMessages, newMsg);
						}

						//Perform any business level validations on the datarow
						Dictionary<string, string> businessMessages = taskManager.Validate(task);
						foreach (KeyValuePair<string, string> businessMessage in businessMessages)
						{
							ValidationMessage newMsg = new ValidationMessage();
							newMsg.FieldName = businessMessage.Key;
							newMsg.Message = businessMessage.Value;
							AddUniqueMessage(validationMessages, newMsg);
						}

						//Make sure we have no validation messages before updating
						if (validationMessages.Count == 0)
						{
							//Get copies of everything..
							Artifact notificationArt = task.Clone();
							ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

							//Persist to database, catching any business exceptions and displaying them
							try
							{
								taskManager.Update(task, userId);
							}
							catch (DataValidationException exception)
							{
								return CreateSimpleValidationMessage(exception.Message);
							}
							catch (OptimisticConcurrencyException)
							{
								return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
							}
							catch (TaskDateOutOfBoundsException)
							{
								return CreateSimpleValidationMessage(Resources.Messages.TasksService_DatesOutsideBounds);
							}
							customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

							//Call notifications..
							try
							{
								new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Task #" + task.TaskId + ".");
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return validationMessages;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of tasks in the system for the specific user/project
		/// </summary>
		/// <param name="userId">The user we're viewing the tasks as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">Any standard filters that need to be set</param>
		/// <param name="displayTypeId">The location of the list we are on - to distinguish them from one another for display/filtering purposes</param>
		/// <returns>Collection of dataitems</returns>
		public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the task and custom property business objects
				TaskManager taskManager = new TaskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//Now get the list of populated filters and the current sort
				string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST;
				string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION;
				if (displayTypeId.HasValue && displayTypeId.Value == (int)Artifact.DisplayTypeEnum.Requirement_Tasks)
				{
					filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DETAILS_TASKS_FILTERS;
					sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DETAILS_TASKS_GENERAL;
				}
				if (displayTypeId.HasValue && displayTypeId.Value == (int)Artifact.DisplayTypeEnum.Risk_Tasks)
				{
					filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_RISK_DETAILS_TASKS_FILTERS;
					sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_RISK_DETAILS_TASKS_GENERAL;
				}

				Hashtable filterList = GetProjectSettings(userId, projectId, filtersSettingsCollection);
				string sortCommand = GetProjectSetting(userId, projectId, sortSettingsCollection, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TaskId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Add any standard filters
				int? folderId = null;
				if (standardFilters != null && standardFilters.Count > 0)
				{
					Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
					foreach (KeyValuePair<string, object> filter in deserializedFilters)
					{
                        //Passed in folder Id
                        if (filter.Key == GlobalFunctions.SPECIAL_FILTER_FOLDER_ID && filter.Value is Int32)
                        {
                            int intValue = (int)(filter.Value);
                            if (intValue > 0 && taskManager.TaskFolder_Exists(projectId, intValue))
                            {
                                folderId =  intValue;
                            }
                            else
                            {
                                //Root Folder (i.e. no folder tasks only)
                                folderId = ManagerBase.NoneFilterValue;
                            }
                        }
                        else
                        {
                            filterList[filter.Key] = filter.Value;
                        }
                    }
				}
				else
				{
					//See if we have a folder to filter on, not applied if we have a standard filter
					//because those screens don't display the folders on the left-hand side

					//-1 = no filter
					//0 = root folder
					int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
					if (selectedNodeId < 1)
					{
						//Root Folder (i.e. no folder tasks only)
						folderId = ManagerBase.NoneFilterValue;
					}
					else
					{
                        if (taskManager.TaskFolder_Exists(projectId, selectedNodeId))
                        {
                            //Filter by specific Folder
						    folderId = selectedNodeId;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder tasks only) and update the projectsetting
                            folderId = ManagerBase.NoneFilterValue;
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
					}
				}

				//Create the filter item first - we can clone it later
				SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
				dataItems.Add(filterItem);
				sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Task);

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 15;
				int currentPage = 1;
				if (paginationSettings["NumberRowsPerPage"] != null)
				{
					paginationSize = (int)paginationSettings["NumberRowsPerPage"];
				}
				if (paginationSettings["CurrentPage"] != null)
				{
					currentPage = (int)paginationSettings["CurrentPage"];
				}
				//Get the number of tasks in the project, unless we have the RequirementId standard filter,
				//in which case, get the count of tasks attached to the requirement
				int artifactCount = taskManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);

				//Make sure that the current page is not larger than the number of pages or less than 1
				if (currentPage > pageCount)
				{
					currentPage = pageCount;
					paginationSettings["CurrentPage"] = currentPage;
					paginationSettings.Save();
				}
				if (currentPage < 1)
				{
					currentPage = 1;
					paginationSettings["CurrentPage"] = currentPage;
					paginationSettings.Save();
				}

				//**** Now we need to actually populate the rows of data to be returned ****
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				List<TaskView> tasks = taskManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);

				//Display the pagination information
				sortedData.CurrPage = currentPage;
				sortedData.PageCount = pageCount;
				sortedData.StartRow = startRow;

				//Display the visible and total count of artifacts
				sortedData.VisibleCount = tasks.Count;
				sortedData.TotalCount = artifactCount;

				//Display the sort information
				sortedData.SortProperty = sortProperty;
				sortedData.SortAscending = sortAscending;

				//Now get the list of custom property options and lookup values for this artifact type / project
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, true, false, true);

				//Get the list of subfolders if we are displaying folders on the left
				if (folderId.HasValue)
				{
					//We also need to get and sub-folders in this folder
					List<TaskFolder> taskFolders = taskManager.TaskFolder_GetByParentId(projectId, (folderId == ManagerBase.NoneFilterValue) ? null : folderId, sortProperty, sortAscending, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

					//Iterate through all the task folders and populate the data items
					foreach (TaskFolder taskFolder in taskFolders)
					{
						//We clone the template item as the basis of all the new items
						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateRow(dataItem, taskFolder);
						dataItems.Add(dataItem);
					}
				}

				//Iterate through all the tasks and populate the dataitem
				foreach (TaskView task in tasks)
				{
					//We clone the template item as the basis of all the new items
					SortedDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateRow(dataItem, task, customProperties, false, null);
					dataItems.Add(dataItem);
				}

				//Also include the pagination info
				sortedData.PaginationOptions = RetrievePaginationOptions(projectId);

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return sortedData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Allows sorted lists with folders to focus on a specific item and open its containing folder
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="artifactId">Id of a test case (or negative for a folder)</param>
		/// <returns>The id of the folder (if any)</returns>
		public override int? SortedList_FocusOn(int projectId, int artifactId, bool clearFilters)
		{
			const string METHOD_NAME = "SortedList_FocusOn";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view tasks
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//See if we have a folder or task
				TaskManager taskManager = new TaskManager();
				if (artifactId > 0)
				{
					int taskId = artifactId;

					//Retrieve this task
					Task task = taskManager.RetrieveById(taskId);
					if (task != null)
					{
						//Get the folder
						int folderId = (task.TaskFolderId.HasValue) ? task.TaskFolderId.Value : -1;

						//Unset the current filters and then set the current folder to this one
						bool isInitialFilter = false;
						string result = UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST, Artifact.ArtifactTypeEnum.Task, out isInitialFilter);
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, folderId);
						return folderId;
					}
				}
				if (artifactId < 0)
				{
					int taskFolderId = -artifactId;

					//Retrieve this task folder
					TaskFolder taskFolder = taskManager.TaskFolder_GetById(taskFolderId);
					if (taskFolder != null)
					{
						//Unset the current filters and then set the current folder to this one
						if (clearFilters)
						{
							bool isInitialFilter = false;
							string result = UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST, Artifact.ArtifactTypeEnum.Task, out isInitialFilter);
						}
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, taskFolderId);
						return taskFolderId;
					}
				}
				return null;
			}
			catch (ArtifactNotExistsException)
			{
				//Ignore, do not log
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region ITreeViewService Methods

		/// <summary>Called when a task is dropped onto a folder in the treeview</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The current user</param>
		/// <param name="artifactIds">The ids of the tasks</param>
		/// <param name="nodeId">The id of the folder (or 0 for the root)</param>
		public void TreeView_DragDestination(int projectId, int[] artifactIds, int nodeId)
		{
			const string METHOD_NAME = "TreeView_DragDestination";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to modify tasks (limited view insufficient)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the folder id (or root if < 1)
				int? folderId = null;
				if (nodeId > 0)
				{
					folderId = nodeId;
				}

				//Make sure the folder exists (unless root)
				TaskManager taskManager = new TaskManager();
				TaskFolder taskFolder = null;
				if (folderId.HasValue)
				{
					//Moving to existing folder
					taskFolder = taskManager.TaskFolder_GetById(folderId.Value);
				}
				if (taskFolder != null || !folderId.HasValue)
				{
					//Retrieve each task in the list and move to the specified folder
					foreach (int taskId in artifactIds)
					{
						//See if we have a folder or task
						if (taskId > 0)
						{
							//Task
							taskManager.Task_UpdateFolder(taskId, folderId);
						}
						else
						{
							//Task Folder
							int taskFolderId = -taskId;
							TaskFolder taskFolderBeingMoved = taskManager.TaskFolder_GetById(taskFolderId);
							if (taskFolderBeingMoved != null)
							{
								taskFolderBeingMoved.StartTracking();
								//Make sure you don't try and set a folder to be its own parent (!)
								if (!folderId.HasValue || folderId.Value != taskFolderId)
								{
									taskFolderBeingMoved.ParentTaskFolderId = folderId;
								}
								taskManager.TaskFolder_Update(taskFolderBeingMoved);
							}
						}
					}
				}
			}
			catch (ArtifactNotExistsException)
			{
				//Fail quietly
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Returns the tooltip for a node (used if not provided when node created)</summary>
		/// <param name="nodeId">The id of the node</param>
		/// <returns>The tooltip</returns>
		public string TreeView_GetNodeTooltip(string nodeId)
		{
			return null;
		}

		/// <summary>Returns the list of task folders contained in a parent node</summary>
		/// <param name="userId">The current user</param>
		/// <param name="parentId">The id of the parent folder</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The list of treeview nodes to display</returns>
		public List<TreeViewNode> TreeView_GetNodes(int projectId, string parentId)
		{
			const string METHOD_NAME = "TreeView_GetNodes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view tasks (limited view insufficient)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				List<TreeViewNode> nodes = new List<TreeViewNode>();

				//Get the list of project task folders from the business object
				TaskManager taskManager = new TaskManager();

				//See if we need the root folder (folderId = 0)
				if (String.IsNullOrEmpty(parentId))
				{
					nodes.Add(new TreeViewNode(0.ToString(), Resources.Main.Global_Root, Resources.Main.Global_Root));
				}
				else
				{
					int? parentFolderId = null;
					if (!String.IsNullOrEmpty(parentId))
					{
						parentFolderId = Int32.Parse(parentId);
						if (parentFolderId == 0)
						{
							//We want the direct children of the root, so set to NULL
							parentFolderId = null;
						}
					}
					List<TaskFolder> taskFolders = taskManager.TaskFolder_GetByParentId(projectId, parentFolderId);

					foreach (TaskFolder taskFolder in taskFolders)
					{
						nodes.Add(new TreeViewNode(taskFolder.TaskFolderId.ToString(), taskFolder.Name, taskFolder.Name));
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return nodes;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Sets the currently selected node so that it can be persisted for future page loads</summary>
		/// <param name="nodeId">The id of the node to persist</param>
		/// <param name="projectId">The id of the project</param>
		public void TreeView_SetSelectedNode(int projectId, string nodeId)
		{
			const string METHOD_NAME = "TreeView_SetSelectedNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view tasks (limited view insufficient)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//We simply store this in a project setting
				int folderId = -1;
				if (!String.IsNullOrEmpty(nodeId))
				{
					folderId = Int32.Parse(nodeId);
				}
				SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, folderId);
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Gets a comma-separated list of parent nodes that are to be expanded based on the selected node stored in the project settings collection. Used when the page is first loaded or when refresh is clicked</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the project</param>
		public List<string> TreeView_GetExpandedNodes(int projectId)
		{
			const string METHOD_NAME = "TreeView_GetExpandedNodes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				List<string> nodeList = new List<string>();
				//Get the currently selected node (if there is one)
				int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
				if (selectedNodeId != -1)
				{
					//Get the list of all folders in the project and locate the selected item
					TaskManager taskManager = new TaskManager();
					List<TaskFolderHierarchyView> taskFolders = taskManager.TaskFolder_GetList(projectId);
					TaskFolderHierarchyView taskFolder = taskFolders.FirstOrDefault(f => f.TaskFolderId == selectedNodeId);

					//Now iterate through successive parents to get the folder path
					while (taskFolder != null)
					{
						nodeList.Insert(0, taskFolder.TaskFolderId.ToString());
						Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Added node : " + taskFolder.TaskFolderId + " to list");
						if (taskFolder.ParentTaskFolderId.HasValue)
						{
							taskFolder = taskFolders.FirstOrDefault(f => f.TaskFolderId == taskFolder.ParentTaskFolderId.Value);
						}
						else
						{
							taskFolder = null;
						}
					}

					//Finally add the root folder
					nodeList.Insert(0, 0.ToString());
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return nodeList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets all the nodes in the treeview as a simple hierarchical lookup dictionary
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The datasource for the dropdown hierarchy control</returns>
		public JsonDictionaryOfStrings TreeView_GetAllNodes(int projectId)
		{
			const string METHOD_NAME = "TreeView_GetAllNodes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the list of all folders in the project and locate the selected item
				TaskManager taskManager = new TaskManager();
				List<TaskFolderHierarchyView> taskFolders = taskManager.TaskFolder_GetList(projectId);

				//Convert to the necessary lookup
				JsonDictionaryOfStrings taskFolderDic = ConvertLookupValues(taskFolders.OfType<Entity>().ToList(), "TaskFolderId", "Name", "IndentLevel");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return taskFolderDic;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Adds a new node to the tree
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="name">The name of the new node</param>
		/// <param name="description">Not used</param>
		/// <param name="parentNodeId">The id of the parent node to add it under (optional)</param>
		/// <returns>The id of the new node</returns>
		public string TreeView_AddNode(int projectId, string name, string parentNodeId, string description)
		{
			const string METHOD_NAME = "TreeView_AddNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (need to have create permissions)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				int? parentTaskFolderId = null;
				if (!String.IsNullOrEmpty(parentNodeId))
				{
					int intValue;
					if (Int32.TryParse(parentNodeId, out intValue))
					{
						parentTaskFolderId = intValue;
					}
					else
					{
						throw new FaultException(Resources.Messages.TasksService_TaskFolderIdNotInteger);
					}
				}

				if (String.IsNullOrWhiteSpace(name))
				{
					throw new FaultException(Resources.Messages.TasksService_TaskFolderNameRequired);
				}
				else
				{
					//Add the new folder and return the new node id
					TaskManager taskManager = new TaskManager();
					int newTaskFolderId = taskManager.TaskFolder_Create(name.Trim().SafeSubstring(0, 50), projectId, parentTaskFolderId).TaskFolderId;

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return newTaskFolderId.ToString();
				}

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates an existing node in the tree
		/// </summary>
		/// <param name="nodeId">The id of the node to update</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="name">The name of the new node</param>
		/// <param name="description">Not used</param>
		/// <param name="parentNodeId">The id of the parent node to add it under (optional)</param>
		public void TreeView_UpdateNode(int projectId, string nodeId, string name, string parentNodeId, string description)
		{
			const string METHOD_NAME = "TreeView_UpdateNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (need to have modify all)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			int taskFolderId = 0;
			if (Int32.TryParse(nodeId, out taskFolderId) && taskFolderId > 0)
			{
				try
				{
					int? parentTaskFolderId = null;
					if (!String.IsNullOrEmpty(parentNodeId))
					{
						int intValue;
						if (Int32.TryParse(parentNodeId, out intValue))
						{
							parentTaskFolderId = intValue;
						}
						else
						{
							throw new FaultException(Resources.Messages.TasksService_TaskFolderIdNotInteger);
						}
					}

					if (String.IsNullOrWhiteSpace(name))
					{
						throw new FaultException(Resources.Messages.TasksService_TaskFolderNameRequired);
					}
					else
					{
						//Update the existing folder (assuming that it exists)
						TaskManager taskManager = new TaskManager();
						TaskFolder taskFolder = taskManager.TaskFolder_GetById(taskFolderId);
						if (taskFolder != null)
						{
							taskFolder.StartTracking();
							taskFolder.Name = name.Trim().SafeSubstring(0, 50);
							//Make sure you don't try and set a folder to be its own parent (!)
							if (!parentTaskFolderId.HasValue || parentTaskFolderId != taskFolderId)
							{
								taskFolder.ParentTaskFolderId = parentTaskFolderId;
							}
							taskManager.TaskFolder_Update(taskFolder);
						}

						Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
						Logger.Flush();
					}
				}
				catch (ArtifactNotExistsException)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format("Unable to update task folder '{0}' as it does not exist in the system ", nodeId));
					//Fail quietly
				}
				catch (FolderCircularReferenceException)
				{
					throw new InvalidOperationException(Resources.Messages.TasksService_CannotMoveFolderUnderItself);
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
		}

		/// <summary>
		/// Deletes a task folder
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="nodeId">The node id of the folder to be deleted</param>
		public void TreeView_DeleteNode(int projectId, string nodeId)
		{
			const string METHOD_NAME = "TreeView_DeleteNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (need to have delete permissions)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			int taskFolderId = 0;
			if (Int32.TryParse(nodeId, out taskFolderId) && taskFolderId > 0)
			{
				try
				{
					//Delete the specified folder
					TaskManager taskManager = new TaskManager();
					taskManager.TaskFolder_Delete(projectId, taskFolderId);

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
		}

		/// <summary>
		/// Returns the parent node (if any) of the current node
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="nodeId">The node we're interested in</param>
		/// <returns>The parent node</returns>
		public string TreeView_GetParentNode(int projectId, string nodeId)
		{
			const string METHOD_NAME = "TreeView_GetParentNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			int taskFolderId = 0;
			if (Int32.TryParse(nodeId, out taskFolderId) && taskFolderId > 0)
			{
				try
				{
					string parentNodeId = "";
					//Get the parent of the specified folder
					TaskManager taskManager = new TaskManager();
					TaskFolder taskFolder = taskManager.TaskFolder_GetById(taskFolderId);
					if (taskFolder != null && taskFolder.ParentTaskFolderId.HasValue)
					{
						parentNodeId = taskFolder.ParentTaskFolderId.ToString();
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return parentNodeId;
				}
				catch (ArtifactNotExistsException)
				{
					return "";
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			else
			{
				return "";
			}
		}

		#endregion

		#region WorkflowOperations Methods

		/// <summary>
		/// Retrieves the list of workflow operations for the current task
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="typeId">The task type</param>
		/// <param name="artifactId">The id of the task</param>
		/// <returns>The list of available workflow operations</returns>
		/// <remarks>Pass a specific type id if the user has changed the type of the task, but not saved it yet.</remarks>
		public List<DataItem> WorkflowOperations_Retrieve(int projectId, int artifactId, int? typeId)
		{
			const string METHOD_NAME = "WorkflowOperations_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the array of data items to store the workflow operations
				List<DataItem> dataItems = new List<DataItem>();

				//Get the list of available transitions for the current step in the workflow
				TaskManager taskManager = new TaskManager();
				TaskWorkflowManager workflowManager = new TaskWorkflowManager();
				TaskView taskView = taskManager.TaskView_RetrieveById(artifactId);
				int workflowId;
				if (typeId.HasValue)
				{
					workflowId = workflowManager.Workflow_GetForTaskType(typeId.Value);
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForTaskType(taskView.TaskTypeId);
				}

				//Get the current user's role
				int projectRoleId = (SpiraContext.Current.ProjectRoleId.HasValue) ? SpiraContext.Current.ProjectRoleId.Value : -1;

				//Determine if the current user is the author or owner of the task
				bool isAuthor = false;
				if (taskView.CreatorId == CurrentUserId.Value)
				{
					isAuthor = true;
				}
				bool isOwner = false;
				if (taskView.OwnerId.HasValue && taskView.OwnerId.Value == CurrentUserId.Value)
				{
					isOwner = true;
				}
				int statusId = taskView.TaskStatusId;
				List<TaskWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusId, projectRoleId, isAuthor, isOwner);

				//Populate the data items list
				foreach (TaskWorkflowTransition workflowTransition in workflowTransitions)
				{
					//The data item itself
					DataItem dataItem = new DataItem();
					dataItem.PrimaryKey = (int)workflowTransition.WorkflowTransitionId;
					dataItems.Add(dataItem);

					//The WorkflowId field
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "WorkflowId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.TaskWorkflowId;
					dataItem.Fields.Add("WorkflowId", dataItemField);

					//The Name field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
					dataItemField.TextValue = workflowTransition.Name;
					dataItem.Fields.Add("Name", dataItemField);

					//The InputStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "InputStatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.InputTaskStatusId;
					dataItemField.TextValue = workflowTransition.InputTaskStatus.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OutputStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OutputStatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.OutputTaskStatusId;
					dataItemField.TextValue = workflowTransition.OutputTaskStatus.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OutputStatusOpenYn field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OutputStatusOpenYn";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
					dataItemField.TextValue = (workflowTransition.OutputTaskStatusId == (int)Task.TaskStatusEnum.Rejected || workflowTransition.OutputTaskStatusId == (int)Task.TaskStatusEnum.Blocked || workflowTransition.OutputTaskStatusId == (int)Task.TaskStatusEnum.Completed || workflowTransition.OutputTaskStatusId == (int)Task.TaskStatusEnum.Duplicate || workflowTransition.OutputTaskStatusId == (int)Task.TaskStatusEnum.Deferred) ? "N" : "Y";
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The SignatureYn field (does it need a signature)
					dataItemField = new DataItemField();
					dataItemField.FieldName = "SignatureYn";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
					dataItemField.TextValue = (workflowTransition.IsSignatureRequired) ? "Y" : "N";
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				}

				return dataItems;
			}
			catch (ArtifactNotExistsException)
			{
				//Just return nothing back
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region IWorkspace Methods

		/// <summary>
		/// Displays the overall progress of the tasks in the project for active releases
		/// </summary>
		/// <param name="workspaceId">The id of the project</param>
		/// <returns>The workspace overview, or null if not found</returns>
		public WorkspaceData Workspace_RetrieveCompletionData(int workspaceId)
		{
			const string METHOD_NAME = "Workspace_RetrieveCompletionData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view tasks and releases
			int projectId = workspaceId;
			//Tasks
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}
			//Releases
			authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Release);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//The data being returned
				WorkspaceData workspace = null;

				//Get the release id from settings, see if we're displaying for project, release or sprint
				int? releaseId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
				if (releaseId.HasValue && releaseId < 0)
				{
					releaseId = null;
				}

				//First lets retrieve the project
				ProjectManager projectManager = new ProjectManager();
				TaskManager taskManager = new TaskManager();
				Project project = projectManager.RetrieveById(projectId);
				if (project != null)
				{
					workspace = new WorkspaceData();

					//If we are filtering by release, then display sprints, otherwise don't
					if (releaseId.HasValue && releaseId > 0)
					{
						workspace.DisplaySprints = true;
					}
					else
					{
						workspace.DisplaySprints = false;
					}

					//See if we have a release/sprint or project
					if (releaseId.HasValue)
					{
						//Release/Sprint
						ReleaseManager releaseManager = new ReleaseManager();
						ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId.Value);
						if (release != null)
						{
							//Add the primary information about the release
							workspace.Workspace = new WorkspaceItem();
							workspace.Workspace.WorkspaceId = release.ReleaseId;
							workspace.Workspace.WorkspaceName = release.Name;
							workspace.Workspace.StartDate = release.StartDate;
							workspace.Workspace.EndDate = release.EndDate;
							workspace.Workspace.RequirementsAll = release.RequirementCount;
							workspace.Workspace.PercentComplete = release.PercentComplete;

							//If we have a release, get the child releases and sprints
							if (!release.IsIterationOrPhase)
							{
								List<ReleaseView> releasesAndSprints = releaseManager.RetrieveSelfAndChildren(projectId, release.ReleaseId, true, true);

								//Sort into releases and iterations/sprints/phases and return as two sep

								//Releases and Tasks
								workspace.Releases = new List<WorkspaceItem>();
								workspace.ReleaseTasks = new List<WorkspaceItem>();
								foreach (ReleaseView childRelease in releasesAndSprints.Where(r => !r.IsIterationOrPhase && r.ReleaseId != release.ReleaseId))
								{
									int? releaseParentId = releaseManager.GetParentReleaseId(release.ReleaseId);
									WorkspaceItem releaseItem = new WorkspaceItem();
									if (releaseParentId == null)
									{
										releaseItem.ParentId = release.ProjectId;
									}
									else
									{
										releaseItem.ParentId = releaseParentId;
										releaseItem.ParentIsSameType = true;
									}
									releaseItem.ParentId = release.ReleaseId;
									releaseItem.WorkspaceId = childRelease.ReleaseId;
									releaseItem.WorkspaceName = childRelease.Name;
									releaseItem.StartDate = childRelease.StartDate;
									releaseItem.EndDate = childRelease.EndDate;
									releaseItem.RequirementsAll = childRelease.TaskCount;    //We are using tasks not requirements for this gantt chart
									releaseItem.PercentComplete = childRelease.TaskPercentComplete;    //The started task %
									workspace.Releases.Add(releaseItem);

									//Next get the tasks in this release and add to workspace
									List<TaskView> tasks = taskManager.RetrieveByReleaseId(projectId, release.ReleaseId);
									if (tasks.Count > 0)
									{
										foreach (TaskView task in tasks)
										{
											WorkspaceItem taskItem = new WorkspaceItem();
											taskItem.WorkspaceId = task.TaskId;
											taskItem.ParentId = release.ReleaseId;
											taskItem.ParentIsSameType = false;
											taskItem.WorkspaceName = task.Name;
											taskItem.StartDate = task.StartDate;
											taskItem.EndDate = task.EndDate;
											taskItem.RequirementsAll = 1;   //1 Task
											taskItem.PercentComplete = task.CompletionPercent;
											workspace.ReleaseTasks.Add(taskItem);
										}
									}
								}

								//Sprints/Phases/Iterations and Tasks
								workspace.Sprints = new List<WorkspaceItem>();
								workspace.SprintTasks = new List<WorkspaceItem>();
								foreach (ReleaseView sprint in releasesAndSprints.Where(r => r.IsIterationOrPhase))
								{
									//Find the parent by indent level
									string parentIndentLevel = sprint.IndentLevel.SafeSubstring(0, sprint.IndentLevel.Length - 3);
									ReleaseView parent = releasesAndSprints.FirstOrDefault(r => r.IndentLevel == parentIndentLevel);

									WorkspaceItem sprintItem = new WorkspaceItem();
									if (parent != null)
									{
										sprintItem.ParentId = parent.ReleaseId;
									}
									sprintItem.WorkspaceId = sprint.ReleaseId;
									sprintItem.WorkspaceName = sprint.Name;
									sprintItem.StartDate = sprint.StartDate;
									sprintItem.EndDate = sprint.EndDate;
									sprintItem.RequirementsAll = sprint.TaskCount;    //We are using tasks not requirements for this gantt chart
									sprintItem.PercentComplete = sprint.TaskPercentComplete;    //The started task %
									workspace.Sprints.Add(sprintItem);

									//Next get the tasks in this sprint
									List<TaskView> tasks = taskManager.RetrieveByReleaseId(projectId, sprint.ReleaseId);
									if (tasks.Count > 0)
									{
										foreach (TaskView task in tasks)
										{
											WorkspaceItem taskItem = new WorkspaceItem();
											taskItem.WorkspaceId = task.TaskId;
											taskItem.ParentId = sprint.ReleaseId;
											taskItem.ParentIsSameType = false;
											taskItem.WorkspaceName = task.Name;
											taskItem.StartDate = task.StartDate;
											taskItem.EndDate = task.EndDate;
											taskItem.RequirementsAll = 1;   //1 Task
											taskItem.PercentComplete = task.CompletionPercent;
											workspace.SprintTasks.Add(taskItem);
										}
									}
								}

							}
						}
					}
					else
					{
						//Project
						//Add the primary information about the project
						workspace.Workspace = new WorkspaceItem();
						workspace.Workspace.WorkspaceId = project.ProjectId;
						workspace.Workspace.WorkspaceName = project.Name;
						workspace.Workspace.StartDate = project.StartDate;
						workspace.Workspace.EndDate = project.EndDate;
						workspace.Workspace.RequirementsAll = project.RequirementCount;
						workspace.Workspace.PercentComplete = project.PercentComplete;

						//Next lets get the releases and sprints in the project
						ReleaseManager releaseManager = new ReleaseManager();
						List<ReleaseView> releasesAndSprints = releaseManager.RetrieveByProjectId(projectId, true, true);

						//Sort into releases and iterations/sprints/phases and return as two separate collections

						//Releases and Tasks
						workspace.Releases = new List<WorkspaceItem>();
						workspace.ReleaseTasks = new List<WorkspaceItem>();
						foreach (ReleaseView release in releasesAndSprints.Where(r => !r.IsIterationOrPhase))
						{
							int? releaseParentId = releaseManager.GetParentReleaseId(release.ReleaseId);
							WorkspaceItem releaseItem = new WorkspaceItem();
							if (releaseParentId == null)
							{
								releaseItem.ParentId = release.ProjectId;
							}
							else
							{
								releaseItem.ParentId = releaseParentId;
								releaseItem.ParentIsSameType = true;
							}
							releaseItem.WorkspaceId = release.ReleaseId;
							releaseItem.WorkspaceName = release.Name;
							releaseItem.StartDate = release.StartDate;
							releaseItem.EndDate = release.EndDate;
							releaseItem.RequirementsAll = release.TaskCount;    //We are using tasks not requirements for this gantt chart
							releaseItem.PercentComplete = release.TaskPercentComplete;    //The started task %
							workspace.Releases.Add(releaseItem);

							//Next get the tasks in this release and add to workspace
							List<TaskView> tasks = taskManager.RetrieveByReleaseId(projectId, release.ReleaseId);
							if (tasks.Count > 0)
							{
								foreach (TaskView task in tasks)
								{
									WorkspaceItem taskItem = new WorkspaceItem();
									taskItem.WorkspaceId = task.TaskId;
									taskItem.ParentId = release.ReleaseId;
									taskItem.ParentIsSameType = false;
									taskItem.WorkspaceName = task.Name;
									taskItem.StartDate = task.StartDate;
									taskItem.EndDate = task.EndDate;
									taskItem.RequirementsAll = 1;   //1 Task
									taskItem.PercentComplete = task.CompletionPercent;
									workspace.ReleaseTasks.Add(taskItem);
								}
							}
						}

						//Sprints/Phases/Iterations and Tasks
						workspace.Sprints = new List<WorkspaceItem>();
						workspace.SprintTasks = new List<WorkspaceItem>();
						foreach (ReleaseView sprint in releasesAndSprints.Where(r => r.IsIterationOrPhase))
						{
							//Find the parent by indent level
							string parentIndentLevel = sprint.IndentLevel.SafeSubstring(0, sprint.IndentLevel.Length - 3);
							ReleaseView parent = releasesAndSprints.FirstOrDefault(r => r.IndentLevel == parentIndentLevel);

							WorkspaceItem sprintItem = new WorkspaceItem();
							if (parent != null)
							{
								sprintItem.ParentId = parent.ReleaseId;
							}
							sprintItem.WorkspaceId = sprint.ReleaseId;
							sprintItem.WorkspaceName = sprint.Name;
							sprintItem.StartDate = sprint.StartDate;
							sprintItem.EndDate = sprint.EndDate;
							sprintItem.RequirementsAll = sprint.TaskCount;    //We are using tasks not requirements for this gantt chart
							sprintItem.PercentComplete = sprint.TaskPercentComplete;    //The started task %
							workspace.Sprints.Add(sprintItem);

							//Next get the tasks in this sprint
							List<TaskView> tasks = taskManager.RetrieveByReleaseId(projectId, sprint.ReleaseId);
							if (tasks.Count > 0)
							{
								foreach (TaskView task in tasks)
								{
									WorkspaceItem taskItem = new WorkspaceItem();
									taskItem.WorkspaceId = task.TaskId;
									taskItem.ParentId = sprint.ReleaseId;
									taskItem.ParentIsSameType = false;
									taskItem.WorkspaceName = task.Name;
									taskItem.StartDate = task.StartDate;
									taskItem.EndDate = task.EndDate;
									taskItem.RequirementsAll = 1;   //1 Task
									taskItem.PercentComplete = task.CompletionPercent;
									workspace.SprintTasks.Add(taskItem);
								}
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return workspace;
			}
			catch (ArtifactNotExistsException)
			{
				//Return no data
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		/// <summary>
		/// Handles custom list operations used by the task list screen, specifically removing tasks from releases/requirements
		/// </summary>
		/// <param name="operation">
		/// The operation being executed:
		///     RemoveFromRelease - removes the task from the release specified
		///     RemoveFromRequirement - removes the task from the requirement specified
		/// </param>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="destId">The destination item id</param>
		/// <param name="items">The list of source items</param>
		public override string CustomListOperation(string operation, int projectId, int destId, List<string> items)
		{
			const string METHOD_NAME = "CustomListOperation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			try
			{
				//See which operation we have
				if (operation == "RemoveFromRelease")
				{
					//Remove their release association
					TaskManager taskManager = new TaskManager();
					List<int> taskIds = new List<int>();
					foreach (string item in items)
					{
						int taskId;
						if (Int32.TryParse(item, out taskId))
						{
							taskIds.Add(taskId);
						}
					}
					taskManager.RemoveReleaseAssociation(taskIds, userId);
				}
				else if (operation == "RemoveFromRequirement")
				{
					//Iterate through all the passed in tasks and remove their requirement association
					TaskManager taskManager = new TaskManager();
					foreach (string item in items)
					{
						int taskId = Int32.Parse(item);
						taskManager.RemoveRequirementAssociation(taskId, userId);
					}
				}
				else if (operation == "RemoveFromRisk")
				{
					//Iterate through all the passed in tasks and remove their risk association
					TaskManager taskManager = new TaskManager();
					foreach (string item in items)
					{
						int taskId = Int32.Parse(item);
						taskManager.RemoveRiskAssociation(taskId, userId);
					}
				}
				else
				{
					throw new NotImplementedException("Operation '" + operation + "' is not currently supported");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return "";  //Success
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
		/// </summary>
		/// <param name="taskId">The id of the task to get the data for</param>
		/// <returns>The name and description converted to plain-text</returns>
		public string RetrieveNameDesc(int? projectId, int taskId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate the task business object
				TaskManager taskManager = new TaskManager();

				//Now retrieve the specific task - handle quietly if it doesn't exist
				try
				{
					string tooltip = "";
					//See if we have a task or folder
					if (taskId < 0)
					{
						//Task folder IDs are negative
						int taskFolderId = -taskId;

						TaskFolder taskFolder = taskManager.TaskFolder_GetById(taskFolderId);

						//See if we have any parent folders
						List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskFolder.ProjectId, taskFolder.TaskFolderId, false);
						foreach (TaskFolderHierarchyView parentFolder in parentFolders)
						{
							tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
						}

						tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(taskFolder.Name) + "</u>";
					}
					else
					{
						//We have a task
						TaskView taskView = taskManager.TaskView_RetrieveById(taskId);
						if (String.IsNullOrEmpty(taskView.Description))
						{
							//See if we have a requirement or folder it belongs to
							if (taskView.RequirementId.HasValue)
							{
								tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(taskView.RequirementName) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + " &gt; ";
							}
							else if (taskView.TaskFolderId.HasValue)
							{
								List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskView.ProjectId, taskView.TaskFolderId.Value, true);
								foreach (TaskFolderHierarchyView parentFolder in parentFolders)
								{
									tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
								}
							}
							tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(taskView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true);
						}
						else
						{
							//See if we have a requirement or folder it belongs to
							if (taskView.RequirementId.HasValue)
							{
								tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(taskView.RequirementName) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, taskView.RequirementId.Value, true) + "</u> &gt; ";
							}
							else if (taskView.TaskFolderId.HasValue)
							{
								List<TaskFolderHierarchyView> parentFolders = taskManager.TaskFolder_GetParents(taskView.ProjectId, taskView.TaskFolderId.Value, true);
								foreach (TaskFolderHierarchyView parentFolder in parentFolders)
								{
									tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
								}
							}

							tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(taskView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, taskView.TaskId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(taskView.Description);
						}

						//See if we have any comments to append
						IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(taskId, Artifact.ArtifactTypeEnum.Task, false);
						if (comments.Count() > 0)
						{
							IDiscussion lastComment = comments.Last();
							tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
								GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
								GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
								Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
								);
						}
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return tooltip;
				}
				catch (ArtifactNotExistsException)
				{
					//This is the case where the client still displays the task, but it has already been deleted on the server
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for task");
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return Resources.Messages.Global_TooltipNotAvailable;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates the current sort stored in the system (property and direction)
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The artifact property we want to sort on</param>
		/// <param name="sortAscending">Are we sorting ascending or not</param>
		/// <returns>Any error messages</returns>
		public string SortedList_UpdateSort(int projectId, string sortProperty, bool sortAscending, int? displayTypeId)
		{
			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION;
			if (displayTypeId.HasValue && displayTypeId.Value == (int)Artifact.DisplayTypeEnum.Requirement_Tasks)
			{
				sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DETAILS_TASKS_GENERAL;
			}
			if (displayTypeId.HasValue && displayTypeId.Value == (int)Artifact.DisplayTypeEnum.Risk_Tasks)
			{
				sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_RISK_DETAILS_TASKS_GENERAL;
			}

			//Call the base method with the appropriate settings collection
			return UpdateSort(userId, projectId, sortProperty, sortAscending, sortSettingsCollection);
		}

		/// <summary>
		/// Updates the filters stored in the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		/// <returns>Any error messages</returns>
		public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
		{
			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);

			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST;
			if (displayTypeId.HasValue && displayTypeId.Value == (int)Artifact.DisplayTypeEnum.Risk_Tasks)
				filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_RISK_DETAILS_TASKS_FILTERS;
			else if (displayTypeId.HasValue && displayTypeId.Value == (int)Artifact.DisplayTypeEnum.Requirement_Tasks)
				filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DETAILS_TASKS_FILTERS;


			//Call the base method with the appropriate settings collection
			return UpdateFilters(userId, projectId, filters, filtersSettingsCollection, Artifact.ArtifactTypeEnum.Task);
		}

		/// <summary>
		/// Saves the current filters with the specified name
		/// </summary>
		/// <param name="includeColumns">Should we include the column selection</param>
		/// <param name="existingSavedFilterId">Populated if we're updating an existing saved filter</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="name">The name of the filter</param>
		/// <param name="isShared">Is this a shared filter</param>
		/// <returns>Validation/error message (or empty string if none)</returns>
		public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
		{
			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			return SaveFilter(userId, projectId, name, Artifact.ArtifactTypeEnum.Task, GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST, GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, isShared, existingSavedFilterId, includeColumns);
		}

		/// <summary>
		/// Retrieves a list of saved filters for the current user/project
		/// </summary>
		/// <param name="includeShared">Should we include shared ones</param>
		/// <param name="projectId">The current project</param>
		/// <returns>Dictionary of saved filters</returns>
		public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
		{
			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Delegate to the generic implementation
			return RetrieveFilters(userId, projectId, Artifact.ArtifactTypeEnum.Task, includeShared);
		}

		/// <summary>
		/// Returns the latest information on a single task in the system
		/// </summary>
		/// <param name="userId">The user we're viewing the task as</param>
		/// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>A single dataitem object</returns>
		public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
		{
			const string METHOD_NAME = "Refresh";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the task and custom property business objects
				TaskManager taskManager = new TaskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Create the data item record (no filter items)
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

				//Get the task record for the specific task id
				TaskView task = taskManager.TaskView_RetrieveById(artifactId);

				//Make sure the user is authorized for this item
				int ownerId = -1;
				if (task.OwnerId.HasValue)
				{
					ownerId = task.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && task.CreatorId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//The main dataset does not have the custom properties, they need to be retrieved separately
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, Artifact.ArtifactTypeEnum.Task, true);

				//Finally populate the dataitem from the dataset
				if (task != null)
				{
					//See if we already have an artifact custom property row
					if (artifactCustomProperty != null)
					{
						PopulateRow(dataItem, task, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
					}
					else
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, true, false);
						PopulateRow(dataItem, task, customProperties, true, null);
					}

					//If a requirement is specified for this task, need to limit the list of possible releases to just the
					//release associated with the requirement and its child iterations
					if (task.RequirementId.HasValue)
					{
						try
						{
							RequirementView requirementView = new RequirementManager().RetrieveById2(projectId, task.RequirementId.Value);
							if (requirementView != null && requirementView.ReleaseId.HasValue && dataItem.Fields.ContainsKey("ReleaseId"))
							{
								List<ReleaseView> releases = new ReleaseManager().RetrieveSelfAndIterations(projectId, requirementView.ReleaseId.Value, true);
								dataItem.Fields["ReleaseId"].Lookups = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
							}
						}
						catch (ArtifactNotExistsException)
						{
							//Ignore and just display the full release list
						}
					}

					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (!templateSettings.Workflow_BulkEditCanChangeStatus && dataItem.Fields.ContainsKey("TaskStatusId"))
					{
						dataItem.Fields["TaskStatusId"].Editable = false;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItem;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Adds/removes a column from the list of fields displayed in the current view
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="fieldName">The name of the column we displaying/hiding</param>
		public void ToggleColumnVisibility(int projectId, string fieldName)
		{
			const string METHOD_NAME = "ToggleColumnVisibility";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//See if we have a custom property (they need to be handled differently)
				if (CustomPropertyManager.IsFieldCustomProperty(fieldName).HasValue)
				{
					//Toggle the status of the appropriate custom property
					CustomPropertyManager customPropertyManager = new CustomPropertyManager();
					customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, Artifact.ArtifactTypeEnum.Task, fieldName);
				}
				else
				{
					//Toggle the status of the appropriate field name
					ArtifactManager artifactManager = new ArtifactManager();
					artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, Artifact.ArtifactTypeEnum.Task, fieldName);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#region IList interface methods

		/// <summary>
		/// Changes the width of a column in a grid. Needs to be overidden by the subclass
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="fieldName">The name of the column being moved</param>
		/// <param name="width">The new width of the column (in pixels)</param>
		public override void List_ChangeColumnWidth(int projectId, string fieldName, int width)
		{
			const string METHOD_NAME = "List_ChangeColumnWidth";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Change the width of the appropriate artifact field or custom property
				ArtifactManager artifactManager = new ArtifactManager();
				artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, Artifact.ArtifactTypeEnum.Task, fieldName, width);
			}
			catch (InvalidOperationException)
			{
				//The field cannot be found, so fail quietly
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Changes the order of columns in the task list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="fieldName">The name of the column being moved</param>
		/// <param name="newIndex">The new index of the column's position</param>
		public override void List_ChangeColumnPosition(int projectId, string fieldName, int newIndex)
		{
			const string METHOD_NAME = "List_ChangeColumnPosition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//The field position may be different to the index because index is zero-based
				int newPosition = newIndex + 1;

				//Toggle the status of the appropriate artifact field or custom property
				ArtifactManager artifactManager = new ArtifactManager();
				artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, Artifact.ArtifactTypeEnum.Task, fieldName, newPosition);
			}
			catch (InvalidOperationException)
			{
				//The field cannot be found, so fail quietly
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		/// <summary>
		/// Inserts a new task into the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="artifact">The type of artifact we're inserting</param>
		/// <param name="standardFilters">Any standard filters that are set by the page</param>
		/// <param name="displayTypeId">The location of the list we are on - to distinguish them from one another for display/filtering purposes</param>
		/// <returns>The id of the new task</returns>
		public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business object(s)
				TaskManager taskManager = new TaskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//See if we have the release, risk or requirement standard filter
				//Otherwise we'll insert into the current folder
				int? releaseId = null;
				int? requirementId = null;
				int? riskId = null;
				bool insertIntoCurrentFolder = true;
				if (standardFilters != null)
				{
					//The values need to be deserialized into integers
					if (standardFilters.ContainsKey("ReleaseId"))
					{
						string filterValue = standardFilters["ReleaseId"];
						releaseId = (int)GlobalFunctions.DeSerializeValue(filterValue);
						//Handle the (None) case correctly
						if (releaseId == ManagerBase.NoneFilterValue)
						{
							releaseId = null;
						}
						insertIntoCurrentFolder = false;
					}
					if (standardFilters.ContainsKey("RequirementId"))
					{
						string filterValue = standardFilters["RequirementId"];
						requirementId = (int)GlobalFunctions.DeSerializeValue(filterValue);
						//Handle the (None) case correctly
						if (requirementId == ManagerBase.NoneFilterValue)
						{
							requirementId = null;
						}
						insertIntoCurrentFolder = false;
					}
					if (standardFilters.ContainsKey("RiskId"))
					{
						string filterValue = standardFilters["RiskId"];
						riskId = (int)GlobalFunctions.DeSerializeValue(filterValue);
						//Handle the (None) case correctly
						if (riskId == ManagerBase.NoneFilterValue)
						{
							riskId = null;
						}
						insertIntoCurrentFolder = false;
					}
				}

				int? folderId = null;
				if (insertIntoCurrentFolder)
				{
                    //-1 = no filter
                    //0 = root folder
                    //First, check if a folder was passed in via the filters
                    int? passedInFolderId = null;
                    if (standardFilters != null && standardFilters.Count > 0)
                    {
                        Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                        foreach (KeyValuePair<string, object> filter in deserializedFilters)
                        {
                            //See if we have the folder id passed through as a filter
                            if (filter.Key == GlobalFunctions.SPECIAL_FILTER_FOLDER_ID && filter.Value is Int32)
                            {
                                passedInFolderId = (int)(filter.Value);
                            }
                        }
                    }
                    //See if we have a folder to filter by
                    //0 = root folder
                    if (passedInFolderId.HasValue && passedInFolderId.Value > 0)
                    {
                        // set the folder id and update the selected node (set to root folder if the folder does not exist)
                        int intValue = (int)(passedInFolderId.Value);
                        folderId = taskManager.TaskFolder_Exists(projectId, intValue) ? intValue : ManagerBase.NoneFilterValue;
                        this.TreeView_SetSelectedNode(projectId, folderId.ToString());
                    }
                    else
                    {
                        int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        if (selectedNodeId > 0)
                        {
                            if (taskManager.TaskFolder_Exists(projectId, selectedNodeId))
                            {
                                //Filter by specific Folder
                                folderId = selectedNodeId;
                            }
                            else
                            {
                                //Set to the Root Folder (i.e. no folder tasks only) and update the projectsetting
                                folderId = ManagerBase.NoneFilterValue;
                                SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                            }
                        }
                    }
    			}

				//Simply insert the new item into the task list
				int taskId = taskManager.Insert(
					projectId,
					userId,
					Task.TaskStatusEnum.NotStarted,
					null,
					folderId,
					requirementId,
					releaseId,
					null,
					null,
					"",
					"",
					null,
					null,
					null,
					null,
					null,
					userId,
					true,
					riskId
					);

				//We now need to populate the appropriate default custom properties
				Task task = taskManager.RetrieveById(taskId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId, Artifact.ArtifactTypeEnum.Task, true);
				if (task != null)
				{
					//If the artifact custom property row is null, create a new one and populate the defaults
					if (artifactCustomProperty == null)
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, taskId, customProperties);
						artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					//If we have filters currently applied to the view, then we need to set this new task to the same value (make sure to use the correct filter
					//(if possible) so that it will show up in the list
					ProjectSettingsCollection filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST);
					if (displayTypeId.HasValue)
					{
						if (displayTypeId.Value == (int)Artifact.DisplayTypeEnum.Requirement_Tasks)
						{
							filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DETAILS_TASKS_FILTERS);
						}
						else if (displayTypeId.Value == (int)Artifact.DisplayTypeEnum.Risk_Tasks)
						{
							filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RISK_DETAILS_TASKS_FILTERS);
						}
					}

					if (filterList.Count > 0)
					{
						task.StartTracking();
						//We need to tell it to ignore any filtering by the ID, creation date since we cannot set that on a new item
						List<string> fieldsToIgnore = new List<string>() { "TaskId", "CreationDate" };
						UpdateToMatchFilters(projectId, filterList, taskId, task, artifactCustomProperty, fieldsToIgnore);
						taskManager.Update(task, userId);
					}

					//Save the custom properties
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				return taskId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Copies a set of tasks
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="userId">The ID of the user making the copy</param>
		/// <param name="items">The items to copy</param>
		public void SortedList_Copy(int projectId, List<string> items)
		{
			const string METHOD_NAME = "SortedList_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				TaskManager taskManager = new TaskManager();

				//Get the current folder
				//0 = root folder
				int? currentFolderId = null;
				int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
				if (selectedNodeId != 0)
				{
					currentFolderId = selectedNodeId;
				}

				//Get the list of folders, not needed if moving to root
				List<TaskFolderHierarchyView> taskFolders = null;
				if (currentFolderId.HasValue)
				{
					taskFolders = taskManager.TaskFolder_GetList(projectId);
				}

				//Iterate through all the items to be copied
				foreach (string item in items)
				{
					//Get the task / task folder ID
					int artifactId;
					if (Int32.TryParse(item, out artifactId))
					{
						//See if we have a folder or tasks
						if (artifactId > 0)
						{
							//Task
							int taskId = artifactId;

							//Copy the single task
							taskManager.Copy(userId, taskId);
						}
						else if (artifactId < 0)
						{
							//Task Folder
							int taskFolderId = -artifactId;

							//Check to make sure we're not making it's parent either this folder
							//or one of its children
							if (currentFolderId.HasValue && taskFolders != null && taskFolders.Count > 0)
							{
								string folderIndent = taskFolders.FirstOrDefault(f => f.TaskFolderId == taskFolderId).IndentLevel;
								string newParentIndent = taskFolders.FirstOrDefault(f => f.TaskFolderId == currentFolderId.Value).IndentLevel;

								if (newParentIndent.Length >= folderIndent.Length && newParentIndent.Substring(0, folderIndent.Length) == folderIndent)
								{
									//Throw a meaningful exception
									throw new DataValidationExceptionEx(CreateSimpleValidationMessage(Resources.Messages.TasksService_CannotMoveFolderUnderItself));
								}
							}

							//Copy the task folder
							taskManager.TaskFolder_Copy(userId, projectId, taskFolderId, currentFolderId);
						}
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Exports a set of tasks to another project
		/// </summary>
		/// <param name="items">The items to export</param>
		/// <param name="destProjectId">The project to export them to</param>
		public void SortedList_Export(int destProjectId, List<string> items)
		{
			const string METHOD_NAME = "Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(destProjectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be exported
				TaskManager taskManager = new TaskManager();

				//We need to keep track of the mapping between the new and old test cases in case
				//we have linked test cases that need to have the links maintained
				Dictionary<int, int> taskMapping = new Dictionary<int, int>();
				Dictionary<int, int> taskFolderMapping = new Dictionary<int, int>();

				foreach (string itemValue in items)
				{
					//Get the task ID
					int artifactId;
					//taskManager.Export(taskId, destProjectId, userId);

					if (Int32.TryParse(itemValue, out artifactId))
					{
						//See if we have a folder or task
						if (artifactId > 0)
						{
							//Task
							int taskId = artifactId;
							Task task = taskManager.RetrieveById(taskId);
							if (task != null)
							{
								//Export the single test case
								taskManager.Export(taskId, destProjectId, userId);
							}
						}
						if (artifactId < 0)
						{
							//Task Folder
							int taskFolderId = -artifactId;
							TaskFolder taskFolder = taskManager.TaskFolder_GetById(taskFolderId);
							if (taskFolder != null)
							{
								//Export the whole folder
								taskManager.TaskFolder_Export(userId, taskFolder.ProjectId, taskFolderId, destProjectId, taskFolderMapping);
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a set of tasks
		/// </summary>
		/// <param name="items">The items to delete</param>
		/// <param name="projectId">The id of the project (not used)</param>
		public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be deleted
				TaskManager taskManager = new TaskManager();
				foreach (string item in items)
				{
					int artifactId;
					if (Int32.TryParse(item, out artifactId))
					{
						//See if we have a folder or test case
						if (artifactId > 0)
						{
							//Test Case
							int taskId = artifactId;

							//Delete the test case
							try
							{
								taskManager.MarkAsDeleted(projectId, taskId, userId);
							}
							catch (ArtifactNotExistsException)
							{
								//Ignore any errors due to deleting a folder and some of its children at the same time
							}
						}
						if (artifactId < 0)
						{
							//Test Folder
							int taskFolderId = -artifactId;

							//Delete the folder
							try
							{
								taskManager.TaskFolder_Delete(projectId, taskFolderId);
							}
							catch (ArtifactNotExistsException)
							{
								//Ignore any errors due to deleting a folder and some of its children at the same time
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = "RetrievePaginationOptions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = RetrievePaginationOptions(projectId, userId, PROJECT_SETTINGS_PAGINATION, "", GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE, 500);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return paginationDictionary;
		}

		/// <summary>
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = "RetrievePaginationOptions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = RetrievePaginationOptions(projectId, userId, PROJECT_SETTINGS_PAGINATION);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return paginationDictionary;
		}

		/// <summary>
		/// Updates the size of pages returned and the currently selected page
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
		/// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
		public void UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			const string METHOD_NAME = "UpdatePagination";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the pagination settings collection and update
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
				paginationSettings.Restore();
				if (pageSize != -1)
				{
					paginationSettings["NumberRowsPerPage"] = pageSize;
				}
				if (currentPage != -1)
				{
					paginationSettings["CurrentPage"] = currentPage;
				}
				paginationSettings.Save();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Populates a data item from the entity
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="taskFolder">The entity containing the data</param>
		protected void PopulateRow(SortedDataItem dataItem, TaskFolder taskFolder)
		{
			//Set the primary key (negative for folders)
			dataItem.PrimaryKey = -taskFolder.TaskFolderId;
			dataItem.Folder = true;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (taskFolder.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the entity
					PopulateFieldRow(dataItem, dataItemField, taskFolder, null, null, false, null);
				}
			}
		}

		/// <summary>
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="taskView">The entity containing the data</param>
		/// <param name="customProperties">The list of custom property definitions and values</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		/// <param name="workflowCustomProps">The custom properties workflow states</param>
		/// <param name="workflowFields">The standard fields workflow states</param>
		/// <param name="artifactCustomProperty">The artifatc's custom property data (if not provided as part of dataitem) - pass null if not used</param>
		protected void PopulateRow(SortedDataItem dataItem, TaskView taskView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, List<TaskWorkflowField> workflowFields = null, List<TaskWorkflowCustomProperty> workflowCustomProps = null)
		{
			//Set the primary key and concurrency value
			dataItem.PrimaryKey = taskView.TaskId;
			dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, taskView.ConcurrencyDate);

			//Specify if it has an attachment or not
			dataItem.Attachment = taskView.IsAttachments;

            //Specify if it is a special pull request task or not
            dataItem.Alternate = taskView.IsPullRequest;

            //Convert the workflow lists into the type expected by the ListServiceBase function
            List<WorkflowField> workflowFields2 = TaskWorkflowManager.ConvertFields(workflowFields);
			List<WorkflowCustomProperty> workflowCustomProps2 = TaskWorkflowManager.ConvertFields(workflowCustomProps);

			//The date and task effort fields are not editable for tasks
			List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdateDate", "CompletionPercent", "ComponentId", "ProjectedEffort" };

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (taskView.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
					PopulateFieldRow(dataItem, dataItemField, taskView, customProperties, artifactCustomProperty, editable, PopulateEqualizer, workflowFields2, workflowCustomProps2, readOnlyFields);

					//If we're on the list page (no workflow) then requirement is also not editable
					if (fieldName == "RequirementId" && workflowFields == null)
					{
						dataItemField.Editable = false;
					}

					//Apply the conditional formatting to the priority column (if displayed)
					if (dataItemField.FieldName == "TaskPriorityId" && taskView.TaskPriorityId.HasValue)
					{
						dataItemField.CssClass = "#" + taskView.TaskPriorityColor;
					}

					//Apply the conditional formatting to the Start Date column (if displayed)
					if (dataItemField.FieldName == "StartDate" && taskView.StartDate.HasValue && taskView.StartDate < DateTime.UtcNow && taskView.TaskStatusId == (int)Task.TaskStatusEnum.NotStarted)
					{
						dataItemField.CssClass = "Warning";
					}

					//Apply the conditional formatting to the End Date column (if displayed)
					if (dataItemField.FieldName == "EndDate" && taskView.EndDate.HasValue && taskView.EndDate < DateTime.UtcNow && (taskView.TaskStatusId == (int)Task.TaskStatusEnum.NotStarted || taskView.TaskStatusId == (int)Task.TaskStatusEnum.InProgress))
					{
						dataItemField.CssClass = "Warning";
					}
				}
			}
		}

		/// <summary>
		/// Gets the list of lookup values and names for a specific lookup
		/// </summary>
		/// <param name="lookupName">The name of the lookup</param>
		/// <param name="projectId">The id of the project - needed for some lookups</param>
		/// <returns>The name/value pairs</returns>
		protected JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectId, int projectTemplateId)
		{
			const string METHOD_NAME = "GetLookupValues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				JsonDictionaryOfStrings lookupValues = null;
				ReleaseManager releaseManager = new ReleaseManager();
				UserManager user = new UserManager();
				TaskManager taskManager = new TaskManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				if (lookupName == "CreatorId" || lookupName == "OwnerId")
				{
					List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
				}
				if (lookupName == "ProgressId")
				{
					lookupValues = new JsonDictionaryOfStrings(taskManager.RetrieveProgressFiltersLookup());
				}
				if (lookupName == "ComponentId")
				{
					List<DataModel.Component> components = new ComponentManager().Component_Retrieve(projectId);
					lookupValues = ConvertLookupValues(components.OfType<Entity>().ToList(), "ComponentId", "Name");
				}

				if (lookupName == "TaskStatusId")
				{
					List<TaskStatus> statuses = taskManager.RetrieveStatuses();
					lookupValues = ConvertLookupValues(statuses.OfType<Entity>().ToList(), "TaskStatusId", "Name");
				}
				if (lookupName == "TaskTypeId")
				{
					List<TaskType> types = taskManager.TaskType_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(types.OfType<Entity>().ToList(), "TaskTypeId", "Name");
				}
				if (lookupName == "TaskPriorityId")
				{
					List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(priorities.OfType<Entity>().ToList(), "TaskPriorityId", "Name");
				}
				if (lookupName == "ReleaseId")
				{
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false);
					lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
				}
				if (lookupName == "RequirementId")
				{
					RequirementManager requirementManager = new RequirementManager();
					Dictionary<string, string> requirementLookup = requirementManager.RetrieveForLookups(projectId);
					lookupValues = new JsonDictionaryOfStrings(requirementLookup);
				}

				//The custom property lookups
				int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
				if (customPropertyNumber.HasValue)
				{
					CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Task, customPropertyNumber.Value, true);
					if (customProperty != null)
					{
						//Handle the case of normal lists
						if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
						{
							if (customProperty.List != null && customProperty.List.Values.Count > 0)
							{
								lookupValues = ConvertLookupValues(CustomPropertyManager.SortCustomListValuesForLookups(customProperty.List), "CustomPropertyValueId", "Name");
							}
						}

						//Handle the case of user lists
						if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.User)
						{
							List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
							lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
						}

						//Handle the case of flags
						if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Boolean)
						{
							lookupValues = new JsonDictionaryOfStrings(GlobalFunctions.YesNoList());
						}
					}
				}

				return lookupValues;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Verifies the digital signature on a workflow status change if it is required
		/// </summary>
		/// <param name="workflowId">The id of the workflow</param>
		/// <param name="originalStatusId">The original status</param>
		/// <param name="currentStatusId">The new status</param>
		/// <param name="signature">The digital signature</param>
		/// <param name="creatorId">The creator of the task</param>
		/// <param name="ownerId">The owner of the task</param>
		/// <returns>True for a valid signature, Null if no signature required and False if invalid signature</returns>
		protected bool? VerifyDigitalSignature(int workflowId, int originalStatusId, int currentStatusId, Signature signature, int creatorId, int? ownerId)
		{
			const string METHOD_NAME = "VerifyDigitalSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TaskWorkflowManager taskWorkflowManager = new TaskWorkflowManager();
				TaskWorkflowTransition workflowTransition = taskWorkflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, originalStatusId, currentStatusId);
				if (workflowTransition == null)
				{
					//No transition possible, so return failure
					return false;
				}
				if (!workflowTransition.IsSignatureRequired)
				{
					//No signature required, so return null
					return null;
				}

				//Make sure we have a signature at this point
				if (signature == null)
				{
					return false;
				}

				//Make sure the login/password was valid
				string lowerUser = signature.Login.ToLower(System.Globalization.CultureInfo.InvariantCulture);
				bool isValidUser = Membership.ValidateUser(lowerUser, signature.Password);

				//If the password check does not return, lets see if the password is a GUID and test it against RSS/API Key
				Guid passGu;
				if (!isValidUser && Guid.TryParse(signature.Password, out passGu))
				{
					SpiraMembershipProvider prov = (SpiraMembershipProvider)Membership.Provider;
					if (prov != null)
					{
						isValidUser = prov.ValidateUserByRssToken(lowerUser, signature.Password, true, true);
					}
				}

                if (!isValidUser)
                {
                    //User's login/password does not match
                    return false;
                }

                //Make sure the login is for the current user
                MembershipUser user = Membership.GetUser();
				if (user == null)
				{
					//Not authenticated (should't ever hit this point)
					return false;
				}
				if (user.UserName != signature.Login)
				{
					//Signed login does not match current user
					return false;
				}
				int userId = (int)user.ProviderUserKey;
				int? projectRoleId = SpiraContext.Current.ProjectRoleId;

				//Make sure the user can execute this transition
				bool isAllowed = false;
				workflowTransition = taskWorkflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransition.WorkflowTransitionId);
				if (workflowTransition.IsExecuteByCreator && creatorId == userId)
				{
					isAllowed = true;
				}
				else if (workflowTransition.IsExecuteByOwner && ownerId.HasValue && ownerId.Value == userId)
				{
					isAllowed = true;
				}
				else if (projectRoleId.HasValue && workflowTransition.TransitionRoles.Any(r => r.ProjectRoleId == projectRoleId.Value))
				{
					isAllowed = true;
				}
				return isAllowed;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Populates the equalizer type graph for the task progress
		/// </summary>
		/// <param name="dataItemField">The field being populated</param>
		/// <param name="artifact">The data row</param>
		protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
		{
			//Explicitly recast the entity to the type we're expecting
			TaskView taskView = (TaskView)artifact;

			//Calculate the information to display
			int percentGreen;
			int percentRed;
			int percentYellow;
			int percentGray;
			Task task = taskView.ConvertTo<TaskView, Task>();
			string tooltipText = TaskManager.CalculateProgress(task, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out percentGreen, out percentRed, out percentYellow, out percentGray);

			//Now populate the equalizer graph
			dataItemField.EqualizerGreen = percentGreen;
			dataItemField.EqualizerRed = percentRed;
			dataItemField.EqualizerYellow = percentYellow;
			dataItemField.EqualizerGray = percentGray;

			//Populate Tooltip
			dataItemField.TextValue = "";
			dataItemField.Tooltip = tooltipText;
		}

		/// <summary>
		/// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the tasks as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		/// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
		{
			//We need to dynamically add the various columns from the field list
			LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
			AddDynamicColumns(Artifact.ArtifactTypeEnum.Task, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, PopulateEqualizerShape, returnJustListFields);
		}

		/// <summary>
		/// Used to populate the shape of the special compound fields used to display the information
		/// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
		/// </summary>
		/// <param name="projectTemplateId">the id of the project template</param>
		/// <param name="dataItemField">The field whose shape we're populating</param>
		/// <param name="fieldName">The field name we're handling</param>
		/// <param name="filterList">The list of filters</param>
		/// <param name="projectId">The project we're interested in</param>
		protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId)
		{
			//Check to see if this is a field we can handle
			if (fieldName == "ProgressId")
			{
				dataItemField.FieldName = "ProgressId";
				string filterLookupName = fieldName;
				dataItemField.Lookups = GetLookupValues(filterLookupName, projectId, projectTemplateId);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(filterLookupName))
				{
					dataItemField.IntValue = (int)filterList[filterLookupName];
				}
			}
		}

		#region INavigationService

		/// <summary>
		/// Returns a list of tasks for display in the navigation bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="indentLevel">Not used since we always return the whole list on load</param>
		/// <returns>List of tasks, potentially including parent release/requirements</returns>
		/// <param name="displayMode">
		/// The display mode of the navigation list:
		/// 1 = Filtered List - Only tasks, reflects current filters/sorts
		/// 2 = All Items - Shows parent release/iteration/requirement, no filter, but uses current sort
		/// 3 = Assigned to the Current User
		/// </param>
		/// <param name="selectedItemId">The id of the currently selected item</param>
		/// <remarks>
		/// Returns just the child items of the passed-in requirement indent-level
		/// </remarks>
		public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
		{
			const string METHOD_NAME = "NavigationBar_RetrieveList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Task);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the business objects
				TaskManager taskManager = new TaskManager();
				RequirementManager requirementManager = new RequirementManager();
				ReleaseManager releaseManager = new ReleaseManager();

				//Create the array of data items
				List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();
				int? releaseId = null;
				RequirementView selectedRequirement = null;

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST);

				//Get the sort information
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "TaskId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Get the current folder
				int? folderId = null;   //No filter
				int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
				if (selectedNodeId < 1)
				{
					//Root Folder (i.e. no folder tasks only)
					folderId = ManagerBase.NoneFilterValue;
				}
				else
				{
					//Filter by specific Folder
					folderId = selectedNodeId;
				}

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 500;
				int currentPage = 1;
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE] != null)
				{
					paginationSize = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE];
				}
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] != null)
				{
					currentPage = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE];
				}
				//Get the number of tasks in the project
				int artifactCount = taskManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the requirements list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}
				List<TaskView> taskList = new List<TaskView>(); //Default to empty list
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.FilteredList)
				{
					//Filtered List
					if (authorizationState == Project.AuthorizationState.Limited)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
					taskList = taskManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				}
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.AllItems)
				{
					//All Items
					if (authorizationState == Project.AuthorizationState.Limited)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
					taskList = taskManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				}
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned)
				{
					//Assigned to User
					taskList = taskManager.RetrieveByOwnerId(userId, projectId, null, false);
				}
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.ForRelease)
				{
					//Part of the current Release

					//See if we have an item set. If not, nothing is returned
					if (selectedItemId.HasValue)
					{
						//We need to retrieve the current task to see if it has a release/iteration set
						try
						{
							Task selectedTask = taskManager.RetrieveById(selectedItemId.Value);
							releaseId = selectedTask.ReleaseId;
							if (releaseId.HasValue)
							{
								taskList = taskManager.RetrieveByReleaseId(projectId, releaseId.Value);
							}
						}
						catch (ArtifactNotExistsException)
						{
							//Ignore, nothing will be returned
						}
					}
				}
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.ForRequirement)
				{
					//Belong to the same Requirement

					//See if we have an item set. If not, nothing is returned
					if (selectedItemId.HasValue)
					{
						//We need to retrieve the current task to see if it has a parent requirement set
						try
						{
							Task selectedTask = taskManager.RetrieveById(selectedItemId.Value);
							if (selectedTask.RequirementId.HasValue)
							{
								//See if we're passed a req indent-level. If so, just get the child tasks
								if (String.IsNullOrEmpty(indentLevel))
								{
									//We have a requirement set, so get all the tasks in this requirement
									int requirementId = selectedTask.RequirementId.Value;
									taskList = taskManager.RetrieveByRequirementId(requirementId);

									//See if this requirement has a release set
									selectedRequirement = requirementManager.RetrieveById(UserManager.UserInternal, projectId, requirementId);
									if (selectedRequirement.ReleaseId.HasValue)
									{
										releaseId = selectedRequirement.ReleaseId.Value;
									}
								}
								else
								{
									RequirementView indentLevelReq = requirementManager.RetrieveByIndentLevel2(projectId, indentLevel);
									if (indentLevelReq == null)
									{
										//Just return all Items
										taskList = taskManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, 0);
									}
									else
									{
										int requirementId = indentLevelReq.RequirementId;
										taskList = taskManager.RetrieveByRequirementId(requirementId);
									}
								}
							}
						}
						catch (ArtifactNotExistsException)
						{
							//Just return all Items
							taskList = taskManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, 0);
						}
					}
				}

				int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
				//Make sure that the current page is not larger than the number of pages or less than 1
				if (currentPage > pageCount)
				{
					currentPage = pageCount;
					paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] = currentPage;
					paginationSettings.Save();
				}
				if (currentPage < 1)
				{
					currentPage = 1;
					paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] = currentPage;
					paginationSettings.Save();
				}

				//If we are displaying the assigned for a user, simply nest by their associated requirement, if there is one
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned)
				{
					//We need to group by requirement name, since we are going to essentially group them this way
					List<IGrouping<int?, TaskView>> groupedTasksByRequirements = taskList.GroupBy(t => t.RequirementId).ToList();
					string runningIndentLevel = "AAA";
					foreach (IGrouping<int?, TaskView> groupedTasksByRequirement in groupedTasksByRequirements)
					{
						int? requirementId = groupedTasksByRequirement.Key;
						List<TaskView> tasksForRequirement = groupedTasksByRequirement.ToList();
						if (requirementId.HasValue)
						{
							//First populate the parent requirement
							//Create the data-item
							HierarchicalDataItem dataItem = new HierarchicalDataItem();

							//Get the requirement name from the tast list
							string requirementName = Resources.Main.Global_Unknown;
							if (tasksForRequirement.Count > 0)
							{
								requirementName = tasksForRequirement[0].RequirementName;
							}

							//Populate the necessary fields
							dataItem.PrimaryKey = requirementId.Value;
							dataItem.Indent = runningIndentLevel;
							dataItem.Expanded = true;

							//We need to pass through an override URL since the requirements have a different URL
							dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, projectId, dataItem.PrimaryKey));

							//Name/Desc
							DataItemField dataItemField = new DataItemField();
							dataItemField.FieldName = "Name";
							dataItemField.TextValue = requirementName;
							dataItemField.Tooltip = "Images/artifact-Requirement.svg";
							dataItem.Summary = true;    //All requirements are listed as summary items
							dataItem.Alternate = false;
							dataItem.Fields.Add("Name", dataItemField);

							//Add to the items collection
							dataItems.Add(dataItem);

							//Now the child tasks
							PopulateTasks(tasksForRequirement, dataItems, runningIndentLevel);
						}
						else
						{
							//These tasks are root-level and don't have a parent requirement
							runningIndentLevel = PopulateTasks(tasksForRequirement, dataItems, "");
						}

						//Increment the indent level
						runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
					}
				}
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.AllItems || displayMode == (int)ServerControls.NavigationBar.DisplayModes.FilteredList)
				{
					//All Items / Filtered List - we just need the tasks, nothing else
					if (taskList != null && taskList.Count > 0)
					{
						PopulateTasks(taskList, dataItems, "");
					}
				}
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.ForRelease)
				{
					//For a specific release/iteration
					//See if we have a release id set
					if (releaseId.HasValue)
					{
						//Retrieve the release and populate as the first item
						try
						{
							ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId.Value);

							//Create the data-item
							HierarchicalDataItem dataItem = new HierarchicalDataItem();

							//Populate the necessary fields
							dataItem.PrimaryKey = release.ReleaseId;
							dataItem.Indent = "AAA";
							dataItem.Expanded = true;

							//We need to pass through an override URL since the releases have a different URL
							dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, projectId, dataItem.PrimaryKey));

							//Name/Desc
							DataItemField dataItemField = new DataItemField();
							dataItemField.FieldName = "Name";
							dataItemField.TextValue = release.Name;
							dataItemField.Tooltip = (release.IsIterationOrPhase) ? "Images/artifact-Iteration.svg" : "Images/artifact-Release.svg";
							dataItem.Summary = true;    //All releases are listed as summary items
							dataItem.Fields.Add("Name", dataItemField);

							//Add to the items collection
							dataItems.Add(dataItem);

							//Now populate the tasks underneath
							PopulateTasks(taskList, dataItems, "AAA");
						}
						catch (ArtifactNotExistsException)
						{
							//Just get the basic list of tasks
							PopulateTasks(taskList, dataItems, "");
						}
					}
				}
				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.ForRequirement)
				{
					//For a specific requirement
					if (selectedRequirement != null)
					{
						List<RequirementView> requirementList;
						int startIndex;
						if (releaseId.HasValue)
						{
							//Get the requirements in this release/iteration
							requirementList = requirementManager.RetrieveByReleaseId(projectId, releaseId.Value);
							startIndex = 0;
						}
						else
						{
							//Get the peer-requirements in the tree
							requirementList = requirementManager.RetrievePeersAndParent(UserManager.UserInternal, projectId, selectedRequirement.IndentLevel, false);
							startIndex = 1;
						}
						//Iterate through all the reqs (except the parent) and populate the dataitem (only some columns are needed)
						for (int i = startIndex; i < requirementList.Count; i++)
						{
							RequirementView requirement = requirementList[i];
							//Create the data-item
							HierarchicalDataItem dataItem = new HierarchicalDataItem();

							//Populate the necessary fields
							dataItem.PrimaryKey = requirement.RequirementId;
							dataItem.Indent = requirement.IndentLevel;
							dataItem.Expanded = (requirement.RequirementId == selectedRequirement.RequirementId);

							//We need to pass through an override URL since the requirements have a different URL
							dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, projectId, dataItem.PrimaryKey));

							//Name/Desc
							DataItemField dataItemField = new DataItemField();
							dataItemField.FieldName = "Name";
							dataItemField.TextValue = requirement.Name;
							dataItemField.Tooltip = "Images/artifact-Requirement.svg";
							dataItem.Summary = true;    //All requirements are listed as summary items
							dataItem.Alternate = false;
							dataItem.Fields.Add("Name", dataItemField);

							//Add to the items collection
							dataItems.Add(dataItem);

							//Now see if we have any tasks under this requirement
							if (requirement.RequirementId == selectedRequirement.RequirementId)
							{
								PopulateTasks(taskList, dataItems, requirement.IndentLevel);
							}
						}
					}
					else
					{
						PopulateTasks(taskList, dataItems, "");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItems;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Populates the list of tasks
		/// </summary>
		/// <param name="tasks">The task list</param>
		/// <param name="dataItems">The list of nav-bar items</param>
		/// <param name="reqIndentLevel">The requirement indent level (if there is one)</param>
		/// <returns>The last used indent level</returns>
		protected string PopulateTasks(List<TaskView> tasks, List<HierarchicalDataItem> dataItems, string reqIndentLevel)
		{
			//Iterate through all the tasks and populate the dataitem (only some columns are needed)
			string taskIndentLevel = reqIndentLevel + "AAA"; //Add on to the req indent level
			foreach (TaskView task in tasks)
			{
				//Create the data-item
				HierarchicalDataItem dataItem = new HierarchicalDataItem();

				//Populate the necessary fields
				dataItem.PrimaryKey = task.TaskId;
				dataItem.Indent = taskIndentLevel;
                dataItem.Alternate = task.IsPullRequest;

				//Name/Desc
				DataItemField dataItemField = new DataItemField();
				dataItemField.FieldName = "Name";
				dataItemField.TextValue = task.Name;
				dataItem.Summary = false;
				dataItem.Fields.Add("Name", dataItemField);

				//Add to the items collection
				dataItems.Add(dataItem);

				//Increment the indent level
				taskIndentLevel = HierarchicalList.IncrementIndentLevel(taskIndentLevel);
			}

			return taskIndentLevel;
		}

		/// <summary>
		/// Updates the size of pages returned and the currently selected page
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
		/// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
		public void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			const string METHOD_NAME = "NavigationBar_UpdatePagination";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the pagination settings collection and update
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
				paginationSettings.Restore();
				if (pageSize != -1)
				{
					paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE] = pageSize;
				}
				if (currentPage != -1)
				{
					paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] = currentPage;
				}
				paginationSettings.Save();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates the display settings used by the Navigation Bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="displayMode">The current display mode</param>
		/// <param name="displayWidth">The display width</param>
		/// <param name="minimized">Is the navigation bar minimized or visible</param>
		public void NavigationBar_UpdateSettings(int projectId, int? displayMode, int? displayWidth, bool? minimized)
		{
			const string METHOD_NAME = "NavigationBar_UpdateSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Update the user's project settings
				bool changed = false;
				ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION);
				if (displayMode.HasValue)
				{
					settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = displayMode.Value;
					changed = true;
				}
				if (minimized.HasValue)
				{
					settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED] = minimized.Value;
					changed = true;
				}
				if (displayWidth.HasValue)
				{
					settings[GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH] = displayWidth.Value;
					changed = true;
				}
				if (changed)
				{
					settings.Save();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion
	}
}
