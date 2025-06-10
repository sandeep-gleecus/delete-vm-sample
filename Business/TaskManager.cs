using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>This class encapsulates all the data access functionality for adding, modifying and deleting project tasks</summary>
	public class TaskManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.TaskManager::";

		protected Dictionary<string, string> progressFiltersList;   /* Cannot be static since localized */

		public static List<TaskStatus> _staticTaskStatuses = null;

		#region Internal Methods

		/// <summary>
		/// Copies across the task fields and workflows from one project template to another
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of the existing project template</param>
		/// <param name="newProjectTemplateId">The id of the new project template</param>
		/// <param name="taskWorkflowMapping">The workflow mapping</param>
		/// <param name="taskTypeMapping">The type mapping</param>
		/// <param name="taskPriorityMapping">The priority mapping</param>
		/// <param name="customPropertyIdMapping">The custom property mapping</param>
		protected internal void CopyToProjectTemplate(int existingProjectTemplateId, int newProjectTemplateId, Dictionary<int, int> taskWorkflowMapping, Dictionary<int, int> taskTypeMapping, Dictionary<int, int> taskPriorityMapping, Dictionary<int, int> customPropertyIdMapping)
		{
			//***** Now we need to copy across the task workflows *****
			TaskWorkflowManager workflowManager = new TaskWorkflowManager();
			workflowManager.Workflow_Copy(existingProjectTemplateId, newProjectTemplateId, customPropertyIdMapping, taskWorkflowMapping);

			//***** Now we need to copy across the task types *****
			List<TaskType> taskTypes = this.TaskType_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < taskTypes.Count; i++)
			{
				//Need to retrieve the mapped workflow for this type
				if (taskWorkflowMapping.ContainsKey(taskTypes[i].TaskWorkflowId))
				{
					int workflowId = (int)taskWorkflowMapping[taskTypes[i].TaskWorkflowId];
					int newTaskTypeId = this.TaskType_Insert(
						newProjectTemplateId,
						taskTypes[i].Name,
						workflowId,
						taskTypes[i].IsDefault,
						taskTypes[i].IsActive,
						taskTypes[i].IsCodeReview,
						taskTypes[i].IsPullRequest
						);
					taskTypeMapping.Add(taskTypes[i].TaskTypeId, newTaskTypeId);
				}
			}

			//***** Now we need to copy across the task priorities *****
			List<TaskPriority> taskPriorities = this.TaskPriority_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < taskPriorities.Count; i++)
			{
				int newPriorityId = this.TaskPriority_Insert(
					newProjectTemplateId,
					taskPriorities[i].Name,
					taskPriorities[i].Color,
					taskPriorities[i].IsActive,
					taskPriorities[i].Score);
				taskPriorityMapping.Add(taskPriorities[i].TaskPriorityId, newPriorityId);
			}
		}

		/// <summary>
		/// Creates the task types, priorities, default workflow, transitions and field states
		/// for a new project template using the default template
		/// </summary>
		/// <param name="projectTemplateId">The id of the project</param>
		internal void CreateDefaultEntriesForProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = "CreateDefaultEntriesForProjectTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to create the task priorities
				this.TaskPriority_Insert(projectTemplateId, "1 - Critical", "F69419", true, 1);
				this.TaskPriority_Insert(projectTemplateId, "2 - High", "F8A947", true, 2);
				this.TaskPriority_Insert(projectTemplateId, "3 - Medium", "FABF75", true, 3);
				this.TaskPriority_Insert(projectTemplateId, "4 - Low", "FBD4A3", true, 4);

				//Next we need to create a default workflow for a project
				TaskWorkflowManager workflowManager = new TaskWorkflowManager();
				int workflowId = workflowManager.Workflow_InsertWithDefaultEntries(projectTemplateId, GlobalResources.General.Workflow_DefaultWorflow, true).TaskWorkflowId;

				//Next we need to create the task types, associated with this workflow
				TaskType_Insert(projectTemplateId, "Development", workflowId, true, true);
				TaskType_Insert(projectTemplateId, "Testing", workflowId, false, true);
				TaskType_Insert(projectTemplateId, "Management", workflowId, false, true);
				TaskType_Insert(projectTemplateId, "Infrastructure", workflowId, false, true);
				TaskType_Insert(projectTemplateId, "Other", workflowId, false, true);
				TaskType_Insert(projectTemplateId, "Code Review", workflowId, false, true, true, false);
				TaskType_Insert(projectTemplateId, "Pull Request", workflowId, false, true, false, true);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Task Priority Functions

		/// <summary>Retrieves a list of task priorities</summary>
		/// <returns>List of priorities</returns>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		public List<TaskPriority> TaskPriority_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "TaskPriority_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<TaskPriority> taskPriorities;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TaskPriorities
								where (t.IsActive || !activeOnly) && t.ProjectTemplateId == projectTemplateId
								orderby t.TaskPriorityId, t.Score
								select t;

					taskPriorities = query.OrderByDescending(i => i.TaskPriorityId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return taskPriorities;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public PeriodicReviewAlertType PeriodicReviewAlertType_Retrieve(int alertId, bool activeOnly = true)
		{
			const string METHOD_NAME = "TaskPriority_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				PeriodicReviewAlertType PeriodicReviewAlertType;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.PeriodicReviewAlertTypes
								where (t.IsActive || !activeOnly) && t.PeriodicReviewAlertId== alertId
								select t;

					PeriodicReviewAlertType = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return PeriodicReviewAlertType;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>Retrieves an task priority by its id</summary>
		/// <param name="priorityId">The id of the priority</param>
		/// <returns>task priority</returns>
		public TaskPriority TaskPriority_RetrieveById(int priorityId)
		{
			const string METHOD_NAME = "TaskPriority_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TaskPriority taskPriority;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.TaskPriorities
								where i.TaskPriorityId == priorityId
								select i;

					taskPriority = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return taskPriority;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the task priorities for a project</summary>
		/// <param name="taskPriority">The task priority to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void TaskPriority_Update(TaskPriority taskPriority)
		{
			const string METHOD_NAME = "TaskPriority_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.TaskPriorities.ApplyChanges(taskPriority);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes a task priority</summary>
		/// <param name="priorityId">The priority to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void TaskPriority_Delete(int priorityId)
		{
			const string METHOD_NAME = "TaskPriority_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from t in context.TaskPriorities
								where t.TaskPriorityId == priorityId
								select t;

					TaskPriority priority = query.FirstOrDefault();
					if (priority != null)
					{
						context.TaskPriorities.DeleteObject(priority);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a new task priority for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the task priority belongs to</param>
		/// <param name="name">The display name of the task priority</param>
		/// <param name="active">Whether the task priority is active or not</param>
		/// <param name="color">The color code for the priority (in rrggbb hex format)</param>
		/// <param name="score">The numeric score value (weight) of the priority</param>
		/// <returns>The newly created task priority id</returns>
		public int TaskPriority_Insert(int projectTemplateId, string name, string color, bool active, int score = 0)
		{
			const string METHOD_NAME = "TaskPriority_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int priorityId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new task priority
					TaskPriority taskPriority = new TaskPriority();
					taskPriority.ProjectTemplateId = projectTemplateId;
					taskPriority.Name = name.MaxLength(20);
					taskPriority.Color = color.MaxLength(6);
					taskPriority.IsActive = active;
					taskPriority.Score = score;

					context.TaskPriorities.AddObject(taskPriority);
					context.SaveChanges();
					priorityId = taskPriority.TaskPriorityId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return priorityId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Task Type Methods

		/// <summary>Inserts a new task type for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the task type belongs to</param>
		/// <param name="name">The display name of the task type</param>
		/// <param name="active">Whether the task type is active or not</param>
		/// <param name="workflowId">The workflow id (pass null for project default)</param>
		/// <param name="defaultType">Is this the default (initial) type of newly created tasks</param>
		/// <param name="isCodeReview">Is this a code review type of task</param>
		/// <param name="isPullRequest">Is this a pull request type of task</param>
		/// <returns>The newly created task type id</returns>
		public int TaskType_Insert(int projectTemplateId, string name, int? workflowId, bool defaultType, bool active, bool isCodeReview = false, bool isPullRequest = false)
		{
			const string METHOD_NAME = "TaskType_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If no workflow provided, simply use the project default workflow
				if (!workflowId.HasValue)
				{
					TaskWorkflowManager workflowManager = new TaskWorkflowManager();
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).TaskWorkflowId;
				}

				int taskTypeId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					TaskType taskType = new TaskType();
					taskType.ProjectTemplateId = projectTemplateId;
					taskType.Name = name.MaxLength(20);
					taskType.IsDefault = defaultType;
					taskType.IsActive = active;
					taskType.TaskWorkflowId = workflowId.Value;
					taskType.IsCodeReview = isCodeReview;
					taskType.IsPullRequest = isPullRequest;

					context.TaskTypes.AddObject(taskType);
					context.SaveChanges();
					taskTypeId = taskType.TaskTypeId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return taskTypeId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the task types for a project</summary>
		/// <param name="taskType">The task type to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void TaskType_Update(TaskType taskType)
		{
			const string METHOD_NAME = "TaskType_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.TaskTypes.ApplyChanges(taskType);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of task types</summary>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>List of types</returns>
		public List<TaskType> TaskType_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "TaskType_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<TaskType> taskTypes;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TaskTypes
								where (t.IsActive || !activeOnly) && t.ProjectTemplateId == projectTemplateId
								orderby t.TaskTypeId, t.Name
								select t;

					taskTypes = query.OrderByDescending(i => i.TaskTypeId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return taskTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the default task type for the specified template
		/// </summary>
		/// <param name="projectTemplateId">The id of the template</param>
		/// <returns>The default task type</returns>
		public TaskType TaskType_RetrieveDefault(int projectTemplateId)
		{
			const string METHOD_NAME = "TaskType_RetrieveDefault";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TaskType type;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from r in context.TaskTypes
								where r.ProjectTemplateId == projectTemplateId && r.IsDefault
								select r;

					type = query.FirstOrDefault();
					if (type == null)
					{
						throw new ApplicationException(String.Format(GlobalResources.Messages.Task_NoDefaultTypeForProjectTemplate, projectTemplateId));
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return type;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a task type by its ID
		/// </summary>
		/// <param name="taskTypeId">The id of the task type</param>
		/// <returns>The task type</returns>
		public TaskType TaskType_RetrieveById(int taskTypeId)
		{
			const string METHOD_NAME = "TaskType_RetrieveById";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TaskType type;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from r in context.TaskTypes
								where r.TaskTypeId == taskTypeId
								select r;

					type = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return type;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes the task types for a project template</summary>
		/// <param name="taskTypeId">The task type to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void TaskType_Delete(int taskTypeId)
		{
			const string METHOD_NAME = "TaskType_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from r in context.TaskTypes
								where r.TaskTypeId == taskTypeId
								select r;

					TaskType type = query.FirstOrDefault();
					if (type != null)
					{
						context.TaskTypes.DeleteObject(type);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		/// <summary>Associates tasks with an iteration/release</summary>
		/// <param name="taskIds">The tasks being associated</param>
		/// <param name="releaseId">The id of the iteration/release being associated with</param>
		/// <param name="userId">The ID of the user making the change</param>
		/// <remarks>Associating with an iteration also sets the start/end dates to match the iteration</remarks>
		public void AssociateToIteration(List<int> taskIds, int releaseId, int userId)
		{
			const string METHOD_NAME = "AssociateToIteration";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Retrieve the release - if we get an exception need to rethrow
				ReleaseView release = null;
				ReleaseManager releaseManager = new ReleaseManager();
				try
				{
					release = releaseManager.RetrieveById2(null, releaseId);
				}
				catch (ArtifactNotExistsException)
				{
					throw new ApplicationException("The release/iteration you're trying to associate with no longer exists.");
				}

				int? projectId = null;
				List<int> releaseIds = new List<int>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the task
					var query = from t in context.Tasks
								where taskIds.Contains(t.TaskId) && !t.IsDeleted && (t.ReleaseId != releaseId || !t.ReleaseId.HasValue)
								select t;

					//Get the tasks
					List<Task> tasks = query.ToList();
					foreach (Task task in tasks)
					{
						//Store the release and project for use later
						projectId = task.ProjectId;
						if (task.ReleaseId.HasValue && !releaseIds.Contains(task.ReleaseId.Value))
						{
							releaseIds.Add(task.ReleaseId.Value);
						}

						//Update the release id
						task.StartTracking();
						task.ReleaseId = releaseId;
						task.StartDate = release.StartDate;
						task.EndDate = release.EndDate;
						task.LastUpdateDate = DateTime.UtcNow;
						task.ConcurrencyDate = DateTime.UtcNow;

						//Commit the changes,(logging history)
						context.SaveChanges(userId, true, true, null);
					}

					if (!releaseIds.Contains(releaseId))
					{
						releaseIds.Add(releaseId);
					}
				}

				//Next refresh the releases that were changed (source and destination)
				if (projectId.HasValue && releaseIds.Count > 0)
				{
					releaseManager.RefreshProgressEffortTestStatus(projectId.Value, releaseIds);
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

		/// <summary>Assigns a task to a user</summary>
		/// <param name="taskId">The task being associated</param>
		/// <param name="ownerId">The id of the user it's being assigned to (or null to deassign)</param>
		/// <param name="changerId">The ID of the user making the change</param>
		public void AssignToUser(int taskId, Nullable<int> ownerId, int changerId)
		{
			const string METHOD_NAME = "AssignToUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the task - if we get an artifact not exists exception just ignore
					var query = from t in context.Tasks
								where t.TaskId == taskId
								select t;

					//Get the task, if it doesn't exist, just ignore
					Task task = query.FirstOrDefault();
					if (task != null)
					{
						//Update the task
						task.StartTracking();
						task.OwnerId = ownerId;
						task.LastUpdateDate = DateTime.UtcNow;
						task.ConcurrencyDate = DateTime.UtcNow;

						//Commit the changes,(logging history)
						//Need to force a detection of changes
						context.DetectChanges();
						context.SaveChanges(changerId, true, true, null);
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
		/// Splits up a task into two separate tasks
		/// </summary>
		/// <param name="effortPercentage">The % of the effort values to assign to the NEW task (null uses the auto calculation)</param>
		/// <param name="ownerId">The owner of the NEW task, leaving null uses the existing task's owner</param>
		/// <param name="taskId">The id of the task to split</param>
		/// <param name="name">The name of the new task</param>
		/// <param name="userId">The id of the user performing the split</param>
		/// <param name="comment">The comment to add to the association between the two tasks (optional)</param>
		/// <returns>The id of the newly created second task</returns>
		public int Split(int taskId, string name, int userId, int? effortPercentage = null, int? ownerId = null, string comment = null)
		{
			const string METHOD_NAME = "Split";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure the percentage is in the right range
				if (effortPercentage.HasValue)
				{
					if (effortPercentage < 0)
					{
						effortPercentage = 0;
					}
					if (effortPercentage > 100)
					{
						effortPercentage = 100;
					}
				}

				//First we need to retrieve the task we want to copy
				Task existingTask = this.RetrieveById(taskId);

				//Get the template for this project
				int projectId = existingTask.ProjectId;
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Track Changes
				existingTask.StartTracking();

				//See if an effort split was specified
				int? newEstimatedEffort = null;
				int? newActualEffort = null;
				int? newRemainingEffort = null;
				Task.TaskStatusEnum newTaskStatus = (Task.TaskStatusEnum)existingTask.TaskStatusId;
				if (effortPercentage.HasValue)
				{
					if (existingTask.EstimatedEffort.HasValue)
					{
						newEstimatedEffort = (existingTask.EstimatedEffort.Value * effortPercentage.Value) / 100;
						existingTask.EstimatedEffort = (existingTask.EstimatedEffort.Value * (100 - effortPercentage.Value)) / 100;
					}
					if (existingTask.ActualEffort.HasValue)
					{
						newActualEffort = (existingTask.ActualEffort.Value * effortPercentage.Value) / 100;
						existingTask.ActualEffort = (existingTask.ActualEffort.Value * (100 - effortPercentage.Value)) / 100;
					}
					if (existingTask.RemainingEffort.HasValue)
					{
						newRemainingEffort = (existingTask.RemainingEffort.Value * effortPercentage.Value) / 100;
						existingTask.RemainingEffort = (existingTask.RemainingEffort.Value * (100 - effortPercentage.Value)) / 100;
					}
				}
				else
				{
					//If the task is either 100% done or 0% done then we need to simply split 50/50
					if (existingTask.CompletionPercent <= 0 || existingTask.CompletionPercent >= 100)
					{
						if (existingTask.EstimatedEffort.HasValue)
						{
							newEstimatedEffort = (existingTask.EstimatedEffort.Value * 50) / 100;
							existingTask.EstimatedEffort = (existingTask.EstimatedEffort.Value * 50) / 100;
						}
						if (existingTask.ActualEffort.HasValue)
						{
							newActualEffort = (existingTask.ActualEffort.Value * 50) / 100;
							existingTask.ActualEffort = (existingTask.ActualEffort.Value * 50) / 100;
						}
						if (existingTask.RemainingEffort.HasValue)
						{
							newRemainingEffort = (existingTask.RemainingEffort.Value * 50) / 100;
							existingTask.RemainingEffort = (existingTask.RemainingEffort.Value * 50) / 100;
						}
					}
					else
					{
						//We split off just the remaining effort portion and adjust the status
						//First the new task
						newTaskStatus = Task.TaskStatusEnum.NotStarted;
						newEstimatedEffort = existingTask.RemainingEffort;
						newActualEffort = 0;
						newRemainingEffort = newEstimatedEffort;

						//Now 'close' the old task
						if (existingTask.EstimatedEffort.HasValue && existingTask.RemainingEffort.HasValue)
						{
							existingTask.EstimatedEffort = existingTask.EstimatedEffort.Value - existingTask.RemainingEffort.Value;
						}
						existingTask.RemainingEffort = 0;
						existingTask.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
					}
				}

				//Actually perform the insert of the new task
				int newTaskId = this.Insert(
					existingTask.ProjectId,
					existingTask.CreatorId,
					newTaskStatus,
					existingTask.TaskTypeId,
					existingTask.TaskFolderId,
					existingTask.RequirementId,
					existingTask.ReleaseId,
					((ownerId.HasValue) ? ownerId : existingTask.OwnerId),
					existingTask.TaskPriorityId,
					name,
					existingTask.Description,
					existingTask.StartDate,
					existingTask.EndDate,
					newEstimatedEffort,
					newActualEffort,
					newRemainingEffort,
					userId,
					true,
					existingTask.RiskId
					);

				//Now we need to copy across any custom properties
				new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, taskId, newTaskId, DataModel.Artifact.ArtifactTypeEnum.Task, userId);

				//Now we need to copy across any linked attachments
				AttachmentManager attachment = new AttachmentManager();
				attachment.Copy(existingTask.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, newTaskId);

				//Next update the effort values of the existing task
				this.Update(existingTask, userId);

				//Finally add an association between the tasks
				new ArtifactLinkManager().Insert(existingTask.ProjectId, Artifact.ArtifactTypeEnum.Task, existingTask.TaskId, Artifact.ArtifactTypeEnum.Task, newTaskId, userId, comment, DateTime.UtcNow, ArtifactLink.ArtifactLinkTypeEnum.RelatedTo);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the task id of the newly created one
				return newTaskId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Makes a copy of a task within the same project</summary>
		/// <param name="userId">The id of the user making the copy</param>
		/// <param name="taskId">The id of the task we want to make a copy of</param>
		/// <param name="destTaskFolderId">The folder we want to copy it into (null = root)</param>
		/// <param name="appendName">Should we append '- Copy' to name</param>
		/// <returns>The id of the newly created copy</returns>
		public int Copy(int userId, int taskId, int? destTaskFolderId = null, bool appendName = true)
		{
			const string METHOD_NAME = "Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the task we want to copy
				Task existingTask = this.RetrieveById(taskId);

				//Get the template for this project
				int projectId = existingTask.ProjectId;
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Actually perform the insert of the copy
				int copiedTaskId = this.Insert(
					existingTask.ProjectId,
					existingTask.CreatorId,
					(Task.TaskStatusEnum)existingTask.TaskStatusId,
					existingTask.TaskTypeId,
					destTaskFolderId.HasValue ? destTaskFolderId.Value : existingTask.TaskFolderId,
					existingTask.RequirementId,
					existingTask.ReleaseId,
					existingTask.OwnerId,
					existingTask.TaskPriorityId,
					existingTask.Name + (appendName ? CopiedArtifactNameSuffix : ""),
					existingTask.Description,
					existingTask.StartDate,
					existingTask.EndDate,
					existingTask.EstimatedEffort,
					existingTask.ActualEffort,
					existingTask.RemainingEffort,
					userId,
					true,
					existingTask.RiskId
					);

				//Now we need to copy across any custom properties
				new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, taskId, copiedTaskId, DataModel.Artifact.ArtifactTypeEnum.Task, userId);

				//Now we need to copy across any linked attachments
				AttachmentManager attachment = new AttachmentManager();
				attachment.Copy(existingTask.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, copiedTaskId);

				//Copy across any branches
				new PullRequestManager().CopyBranchInfo(taskId, copiedTaskId);

				//Send a notification
				this.SendCreationNotification(copiedTaskId, null, null);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the task id of the copy
				return copiedTaskId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Exports a task from one project to another</summary>
		/// <param name="taskId">The id of the task we want to make a copy of</param>
		/// <param name="destProjectId">The project we want to export it to</param>
		/// <param name="destTaskFolderId">The id of the destination folder or null for root</param>
		/// <returns>The id of the newly created copy</returns>
		/// <remarks>The owner, release, and requirements are always unset since they may not exist in the destination project</remarks>
		public int Export(int taskId, int destProjectId, int userId, int? destTaskFolderId = null)
		{
			const string METHOD_NAME = "Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the task we want to copy
				Task existingTask = this.RetrieveById(taskId);

				//We need to get the source and destination project templates
				//If they are the same, then certain additional values can get copied across
				int sourceProjectTemplateId = new TemplateManager().RetrieveForProject(existingTask.ProjectId).ProjectTemplateId;
				int destProjectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
				bool templatesSame = (sourceProjectTemplateId == destProjectTemplateId);

				//Handle the enum fields
				Task.TaskStatusEnum taskStatus = (Task.TaskStatusEnum)existingTask.TaskStatusId;

				//Actually perform the insert of the copy
				int copiedTaskId = this.Insert(
					destProjectId,
					existingTask.CreatorId,
					taskStatus,
					(templatesSame) ? (int?)existingTask.TaskTypeId : null,
					destTaskFolderId,
					null,
					null,
					null,
					(templatesSame) ? (int?)existingTask.TaskPriorityId : null,
					existingTask.Name,
					existingTask.Description,
					existingTask.StartDate,
					existingTask.EndDate,
					existingTask.EstimatedEffort,
					existingTask.ActualEffort,
					existingTask.RemainingEffort,
					userId
					);

				//Add history item manually..
				new HistoryManager().LogImport(destProjectId, existingTask.ProjectId, existingTask.TaskId, userId, DataModel.Artifact.ArtifactTypeEnum.Task, copiedTaskId, DateTime.UtcNow);

				//We copy custom properties if the templates are the same
				if (templatesSame)
				{
					//Now we need to copy across any custom properties
					new CustomPropertyManager().ArtifactCustomProperty_Export(sourceProjectTemplateId, existingTask.ProjectId, existingTask.TaskId, destProjectId, copiedTaskId, DataModel.Artifact.ArtifactTypeEnum.Task, userId);
				}

				//Now we need to copy across any linked attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Export(existingTask.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, destProjectId, copiedTaskId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the task id of the copy
				return copiedTaskId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Validates a task entity prior to it being sent for update</summary>
		/// <param name="taskRow">The task row being validated</param>
		/// <returns>A list of messages if a failure, otherwise an empty dictionary</returns>
		/// <remarks>Required fields now handled by workflow rather than this function</remarks>
		public Dictionary<string, string> Validate(Task task)
		{
			const string METHOD_NAME = "Validate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			Dictionary<string, string> messages = new Dictionary<string, string>();

			try
			{
				//Check the percentage range
				if (task.CompletionPercent < 0 || task.CompletionPercent > 100)
				{
					messages.Add("CompletionPercent", GlobalResources.Messages.Task_PercentCompleteNotInRange);
				}

				//Make sure that the end date is not before the start date (!)
				if (task.StartDate.HasValue && task.EndDate.HasValue)
				{
					if (task.StartDate.Value.Date > task.EndDate.Value.Date)
					{
						messages.Add("EndDate", GlobalResources.Messages.Task_EndDateCannotBeBeforeStartDate);
					}
				}

				//If the status hasn't changed, but the remaining effort has changed to 0, automatically make Completed
				if (!task.ChangeTracker.OriginalValues.ContainsKey("TaskStatusId") && task.RemainingEffort.HasValue)
				{
					if (task.ChangeTracker.OriginalValues.ContainsKey("RemainingEffort") && task.ChangeTracker.OriginalValues["RemainingEffort"] != null && (int)task.ChangeTracker.OriginalValues["RemainingEffort"] != 0 && task.RemainingEffort.HasValue && task.RemainingEffort == 0)
					{
						task.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
					}
				}

				//Depending on the status there are some additional business rules that we enforce
				if (task.TaskStatusId == (int)Task.TaskStatusEnum.NotStarted)
				{
					//If we previously were in a different status, now need to change the %complete to 0%
					//the remaining effort goes to the estimated effort and projected effort is recalculated
					if (task.ChangeTracker.OriginalValues.ContainsKey("TaskStatusId") && (int)task.ChangeTracker.OriginalValues["TaskStatusId"] != (int)Task.TaskStatusEnum.NotStarted)
					{
						task.CompletionPercent = 0;
						if (task.EstimatedEffort.HasValue)
						{
							task.RemainingEffort = task.EstimatedEffort.Value;
							if (task.ActualEffort.HasValue)
							{
								task.ProjectedEffort = task.EstimatedEffort.Value + task.ActualEffort.Value;
							}
							else
							{
								task.ProjectedEffort = task.EstimatedEffort.Value;
							}
						}
						else
						{
							task.RemainingEffort = null;
							task.ProjectedEffort = null;
						}
					}
					else
					{
						//If the remaining effort has changed to be less than the estimated effort, need to make the task
						//change status to 'in-progress automatically'.
						if (task.ChangeTracker.OriginalValues.ContainsKey("RemainingEffort") && task.RemainingEffort.HasValue && task.EstimatedEffort.HasValue)
						{
							if (task.RemainingEffort.Value < task.EstimatedEffort.Value)
							{
								task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
							}
						}
						else
						{
							//If the estimated effort changed, but the remaining effort did not and we have no actual effort recorded
							//need to update the 'remaining effort' to match the estimated effort, to avoid it being started 'accidentally'
							if (task.ChangeTracker.OriginalValues.ContainsKey("EstimatedEffort") && task.RemainingEffort.HasValue && task.EstimatedEffort.HasValue && task.RemainingEffort.Value != task.EstimatedEffort.Value)
							{
								task.RemainingEffort = task.EstimatedEffort.Value;
							}
						}
					}
				}

				if (task.TaskStatusId == (int)Task.TaskStatusEnum.InProgress)
				{
					//If we switched from Not Started to In-Progress, set the remaining effort to the estimated
					//if the former is not set
					if (task.ChangeTracker.OriginalValues.ContainsKey("TaskStatusId"))
					{
						if ((int)task.ChangeTracker.OriginalValues["TaskStatusId"] == (int)Task.TaskStatusEnum.NotStarted)
						{
							if (task.EstimatedEffort.HasValue && !task.RemainingEffort.HasValue)
							{
								task.RemainingEffort = task.EstimatedEffort.Value;
							}
						}

						//If the status was previously 'Not Started', 'Blocked' or 'Deferred' then
						//automatically prefill the start/end dates
						if ((int)task.ChangeTracker.OriginalValues["TaskStatusId"] == (int)Task.TaskStatusEnum.NotStarted ||
							(int)task.ChangeTracker.OriginalValues["TaskStatusId"] == (int)Task.TaskStatusEnum.Blocked ||
							(int)task.ChangeTracker.OriginalValues["TaskStatusId"] == (int)Task.TaskStatusEnum.Deferred)
						{
							if (!task.StartDate.HasValue)
							{
								task.StartDate = DateTime.UtcNow.Date;
								task.EndDate = DateTime.UtcNow.Date;
							}
							else if (!task.EndDate.HasValue)
							{
								//Default the end date to the start date (1-day task)
								task.EndDate = task.StartDate;
							}
						}
					}
				}

				if (task.TaskStatusId == (int)Task.TaskStatusEnum.Completed)
				{
					//Auto-populate the remaining and projected efforts and set the end date
					task.RemainingEffort = 0;
					task.ProjectedEffort = task.ActualEffort;
					if (!task.EndDate.HasValue)
					{
						task.EndDate = DateTime.UtcNow.Date;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return messages;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of tasks owned by a user and/or release/iteration
		/// sorted by priority then id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user (null = unassigned)</param>
		/// <param name="releaseId">The id of the current release/iteration (null = no release/iteration, -2 = all releases)</param>
		/// <param name="considerChildIterations">Should we consider child iterations, only used when releaseId specified</param>
		/// <returns>The list of tasks</returns>
		/// <remarks>
		/// 1) Does not include tasks in the Rejected or Deferred status
		/// </remarks>
		public List<TaskView> Task_RetrieveBacklogByUserId(int projectId, int? releaseId, int? userId, bool considerChildIterations, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Task_RetrieveBacklogByUserId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TasksView
								where
									t.TaskStatusId != (int)Task.TaskStatusEnum.Rejected &&
									t.TaskStatusId != (int)Task.TaskStatusEnum.Deferred &&
									t.ProjectId == projectId &&
									!t.IsDeleted
								select t;

					//Add the user filter
					if (userId.HasValue)
					{
						query = query.Where(t => t.OwnerId.Value == userId.Value);
					}
					else
					{
						query = query.Where(t => !t.OwnerId.HasValue);
					}

					//Add the release/iteration filter
					if (releaseId.HasValue)
					{
						//Check for the 'all releases' (-2) case
						if (releaseId != -2)
						{
							//Get the child iterations if required
							if (considerChildIterations)
							{
								List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
								query = query.Where(t => releaseAndIterations.Contains(t.ReleaseId.Value));
							}
							else
							{
								query = query.Where(t => t.ReleaseId.Value == releaseId.Value);
							}
						}
					}
					else
					{
						query = query.Where(t => !t.ReleaseId.HasValue);
					}

					//Order by priority (tasks have no rank)
					query = query.OrderBy(t => t.TaskPriorityName).ThenBy(t => t.TaskId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					tasks = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the folder that a task is linked to
		/// </summary>
		/// <param name="taskId">The id of the task</param>
		/// <param name="folderId">The id of the folder</param>
		/// <remarks>Folder changes are not tracked in the artifact history</remarks>
		public void Task_UpdateFolder(int taskId, int? folderId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Task_UpdateFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the task
					var query = from t in context.Tasks
								where t.TaskId == taskId && (!t.IsDeleted || includeDeleted)
								select t;

					Task task = query.FirstOrDefault();
					if (task != null)
					{
						task.StartTracking();
						task.TaskFolderId = folderId;

						//We don't need to log history for this
						context.SaveChanges();
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

		/// <summary>Updates a task that is passed-in</summary>
		/// <param name="task">The task to be persisted</param>
		/// <param name="userId">The user making the change</param>
		/// <param name="isRollback">Whether the update is a rollback or not. Default: FALSE</param>
		/// <param name="rollbackId">Whether or not to update history. Default: TRUE</param>
		public void Update(Task task, int userId, long? rollbackId = null, bool updateHist = true)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (task == null)
			{
				return;
			}

			//Next we need to formally validate the data
			Dictionary<string, string> validationMessages = Validate(task);
			if (validationMessages.Count > 0)
			{
				//We need to return these messages back as special exceptions
				//We just sent back the first message
				string validationMessage = validationMessages.First().Value;
				throw new DataValidationException(validationMessage);
			}

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Start tracking changes
					task.StartTracking();

					//Make sure that the task has its dates inside the correct bounds if they are associated with releases/iterations
					ReleaseManager releaseManager = new ReleaseManager();

					//We dynamically set the %completion and projected effort based on the status and user-supplied
					//effort fields. Any values passed into the dataset will be overwritten
					CalculateCompletion(task);

					if (task.ReleaseId.HasValue)
					{
						//Retrieve the parent release/iteration - if the release no longer exists de-associate
						ReleaseView release = null;
						try
						{
							release = releaseManager.RetrieveById2(task.ProjectId, task.ReleaseId.Value);
						}
						catch (ArtifactNotExistsException)
						{
							task.ReleaseId = null;
						}
						if (release != null)
						{
							//If the release was just changed, instead of throwing exceptions, switch the start date instead
							if (task.ChangeTracker.OriginalValues.ContainsKey("ReleaseId"))
							{
								if (!task.StartDate.HasValue || !task.EndDate.HasValue || task.StartDate.Value < release.StartDate || task.EndDate.Value > release.EndDate)
								{
									//Get the current time-range of the task and use that added to the release start date, otherwise just use the release dates
									if (!task.StartDate.HasValue || !task.EndDate.HasValue)
									{
										task.StartDate = release.StartDate;
										task.EndDate = release.EndDate;
									}
									else
									{
										TimeSpan timeInterval = task.EndDate.Value.Subtract(task.StartDate.Value);
										task.StartDate = release.StartDate;
										task.EndDate = task.StartDate.Value.Add(timeInterval);
										if (task.EndDate.Value > release.EndDate)
										{
											task.EndDate = release.EndDate;
										}
									}
								}
							}

							//Check to make sure the dates of the task are within the release/iteration, with one day
							//of 'wiggle' room because of the timezone differences
							//Start Date
							if (task.StartDate.HasValue)
							{
								if (task.StartDate.Value < release.StartDate.AddDays(-1))
								{
									throw new TaskDateOutOfBoundsException(GlobalResources.Messages.Task_StartDateOutsideReleaseRange);
								}
							}
							//End Date
							if (task.EndDate.HasValue)
							{
								if (task.EndDate.Value > release.EndDate.AddDays(1))
								{
									throw new TaskDateOutOfBoundsException(GlobalResources.Messages.Task_EndDateOutsideReleaseRange);
								}
							}
						}
					}

					//See if we need to refresh the requirement or release after updating
					int? oldReleaseId = null;
					int? oldRequirementId = null;
					int? newReleaseId = null;
					int? newRequirementId = null;
					if (task.ChangeTracker.OriginalValues.ContainsKey("ReleaseId"))
					{
						//Check old/new values
						if (task.ReleaseId.HasValue)
						{
							newReleaseId = task.ReleaseId.Value;
						}
						if (task.ChangeTracker.OriginalValues["ReleaseId"] != null && task.ChangeTracker.OriginalValues["ReleaseId"] is Int32)
						{
							oldReleaseId = (int)task.ChangeTracker.OriginalValues["ReleaseId"];
						}
					}
					if (task.ChangeTracker.OriginalValues.ContainsKey("RequirementId"))
					{
						//Check old/new values
						if (task.RequirementId.HasValue)
						{
							newRequirementId = task.RequirementId.Value;
						}
						if (task.ChangeTracker.OriginalValues["RequirementId"] != null && task.ChangeTracker.OriginalValues["RequirementId"] is Int32)
						{
							oldRequirementId = (int)task.ChangeTracker.OriginalValues["RequirementId"];
						}
					}

					//If any of the effort/date/status fields have changed, need to at least update the current requirement/release
					if (task.ChangeTracker.OriginalValues.ContainsKey("EstimatedEffort") ||
						task.ChangeTracker.OriginalValues.ContainsKey("ActualEffort") ||
						task.ChangeTracker.OriginalValues.ContainsKey("RemainingEffort") ||
						task.ChangeTracker.OriginalValues.ContainsKey("StartDate") ||
						task.ChangeTracker.OriginalValues.ContainsKey("EndDate") ||
						task.ChangeTracker.OriginalValues.ContainsKey("TaskStatusId"))
					{
						if (task.RequirementId.HasValue && !newRequirementId.HasValue)
						{
							newRequirementId = task.RequirementId.Value;
						}
						if (task.ReleaseId.HasValue && !newReleaseId.HasValue)
						{
							newReleaseId = task.ReleaseId.Value;
						}
					}

					//Update the last-update and concurrency dates
					task.LastUpdateDate = DateTime.UtcNow;
					task.ConcurrencyDate = DateTime.UtcNow;

					//Now apply the changes
					context.Tasks.ApplyChanges(task);

					//Save the changes, recording any history changes, and sending any notifications
					context.SaveChanges(userId, true, true, rollbackId);

					//Now look to see if we need to update the progress/effort info for any requirements or releases
					RequirementManager requirementManager = new RequirementManager();
					if (oldRequirementId.HasValue)
					{
						requirementManager.RefreshTaskProgressAndTestCoverage(task.ProjectId, oldRequirementId.Value);
					}
					if (newRequirementId.HasValue)
					{
						requirementManager.RefreshTaskProgressAndTestCoverage(task.ProjectId, newRequirementId.Value);
					}
					List<int> releaseIds = new List<int>();
					if (oldReleaseId.HasValue)
					{
						releaseIds.Add(oldReleaseId.Value);
					}
					if (newReleaseId.HasValue)
					{
						releaseIds.Add(newReleaseId.Value);
					}
					releaseManager.RefreshProgressEffortTestStatus(task.ProjectId, releaseIds);
				}
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Sends a creation notification, typically only used for API creation calls where we need to retrieve and force it as 'added'
		/// </summary>
		/// <param name="taskId">The id of the task</param>
		/// <param name="artifactCustomProperty">The custom property row</param>
		/// <param name="newComment">The new comment (if any)</param>
		/// <remarks>Fails quietly but logs errors</remarks>
		public void SendCreationNotification(int taskId, ArtifactCustomProperty artifactCustomProperty, string newComment)
		{
			const string METHOD_NAME = "SendCreationNotification";
			//Send a notification
			try
			{
				TaskView notificationArt = TaskView_RetrieveById(taskId);
				notificationArt.MarkAsAdded();
				new NotificationManager().SendNotificationForArtifact(notificationArt, artifactCustomProperty, newComment);
			}
			catch (Exception exception)
			{
				//Log, but don't throw;
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>Inserts a new task into the system</summary>
		/// <param name="projectId">The project the task belongs to</param>
		/// <param name="taskStatus">The status of the task</param>
		/// <param name="requirementId">The requirement that the task belongs to (Optional)</param>
		/// <param name="releaseId">The release the task is scheduled for (Optional)</param>
		/// <param name="ownerId">The user who owns the task (Optional)</param>
		/// <param name="taskPriorityId">The priority of the task (Optional)</param>
		/// <param name="name">The name of the task</param>
		/// <param name="description">The long description of the task (Optional)</param>
		/// <param name="startDate">The date the task is scheduled to start (Optional)</param>
		/// <param name="endDate">The date the task is scheduled to end (Optional)</param>
		/// <param name="estimatedEffort">The level of effort/work that the task is estimated to need to complete</param>
		/// <param name="actualEffort">The level of effort/work that has been expanded so far</param>
		/// <param name="remainingEffort">The amount of effort remaining</param>
		/// <param name="creatorId">The id of the person who created the task</param>
		/// <param name="userId">The user that's actually creating the task, for history.</param>
		/// <param name="logHistory">Should we log an 'added' event in the history</param>
		/// <param name="taskFolderId">The folder the task belongs to (optional)</param>
		/// <param name="taskTypeId">The type of task (null = project default)</param>
		/// <param name="riskId">Should we associate the new task with a risk</param>
		/// <returns>The ID of the newly created task</returns>
		public int Insert(int projectId, int creatorId, Task.TaskStatusEnum taskStatus, int? taskTypeId, int? taskFolderId, int? requirementId, int? releaseId, int? ownerId, int? taskPriorityId, string name, string description, DateTime? startDate, DateTime? endDate, int? estimatedEffort, int? actualEffort, int? remainingEffort, int? userId = null, bool logHistory = true, int? riskId = null, bool useReleaseDates = true)
		{
			const string METHOD_NAME = "Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Inititialization
			int taskId = -1;

			try
			{
				//Get the template for the specified project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//See if we have a project default estimated effort to use
				if (!estimatedEffort.HasValue)
				{
					//If we are using SpiraPlan/Team, see if we have a project default value to use
					//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
					//if (License.LicenseProductName != LicenseProductNameEnum.SpiraTest)
					{
						try
						{
							Project project = new ProjectManager().RetrieveById(projectId);
							if (project.TaskDefaultEffort.HasValue)
							{
								estimatedEffort = project.TaskDefaultEffort.Value;
							}
						}
						catch (ArtifactNotExistsException)
						{
							//Project no longer exists
							throw new EntityForeignKeyException(String.Format("The project PR{0} associated with this requirement has been deleted.", projectId));
						}
					}
				}

				//If we have a estimated effort and a null value for remaining, set the remaining to the estimate
				if (estimatedEffort.HasValue && estimatedEffort.Value > 0 && !remainingEffort.HasValue)
				{
					remainingEffort = estimatedEffort;
				}

				//We need to calculate the %complete and projected effort based on the status and supplied effort
				int completionPercent = 0;
				Nullable<int> projectedEffort = null;
				CalculateCompletion(taskStatus, estimatedEffort, actualEffort, remainingEffort, out completionPercent, out projectedEffort);

				//If we have a requirement specified, we can supply some of the missing values as a convenience
				if (requirementId.HasValue)
				{
					RequirementManager requirementManager = new RequirementManager();
					try
					{
						RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId.Value);
						if (!ownerId.HasValue && requirement.OwnerId.HasValue)
						{
							ownerId = requirement.OwnerId.Value;
						}
						if (!releaseId.HasValue && requirement.ReleaseId.HasValue)
						{
							releaseId = requirement.ReleaseId.Value;
						}
						//Set the task priority based on the requirement if not set on the task explicitly
						if (!taskPriorityId.HasValue && requirement.ImportanceId.HasValue)
						{
							Importance importance = requirementManager.RequirementImportance_RetrieveById(requirement.ImportanceId.Value);
							if (importance != null)
							{
								List<TaskPriority> taskPriorities = new TaskManager().TaskPriority_Retrieve(projectTemplateId);
								TaskPriority taskPriority = taskPriorities.FirstOrDefault(p => p.Score == importance.Score);
								if (taskPriority != null)
								{
									taskPriorityId = taskPriority.TaskPriorityId;
								}
							}
						}
					}
					catch (ArtifactNotExistsException)
					{
						//The requirement no longer exists
						requirementId = null;
					}
				}
				//If instead we have a risk specified, we can supply some of the missing values as a convenience
				else if (riskId.HasValue)
				{
					RiskManager riskManager = new RiskManager();
					try
					{
						RiskView risk = riskManager.Risk_RetrieveById2(riskId.Value);
						if (!ownerId.HasValue && risk.OwnerId.HasValue)
						{
							ownerId = risk.OwnerId.Value;
						}
						if (!releaseId.HasValue && risk.ReleaseId.HasValue)
						{
							releaseId = risk.ReleaseId.Value;
						}
					}
					catch (ArtifactNotExistsException)
					{
						//The risk no longer exists
						riskId = null;
					}
				}


				//Retrieve the release/iteration - if one specified, if it doesn't exist, just unset the release
				if (releaseId.HasValue)
				{
					ReleaseView release = null;
					ReleaseManager releaseManager = new ReleaseManager();
					try
					{
						release = releaseManager.RetrieveById2(projectId, releaseId.Value);
						//Update the start/end dates if not value currently specified
						if (!startDate.HasValue && useReleaseDates)
						{
							startDate = release.StartDate;
						}
						if (!endDate.HasValue && useReleaseDates)
						{
							endDate = release.EndDate;
						}
					}
					catch (ArtifactNotExistsException)
					{
						releaseId = null;
					}
				}

				//If no task type specified, get the default one for the current project template
				if (!taskTypeId.HasValue)
				{
					TaskType type = this.TaskType_RetrieveDefault(projectTemplateId);
					taskTypeId = type.TaskTypeId;
				}

				//Fill out dataset with data for new task
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					Task task = new Task();
					task.TaskStatusId = (int)taskStatus;
					task.TaskTypeId = taskTypeId.Value;
					task.ProjectId = projectId;
					task.TaskFolderId = taskFolderId;
					task.RequirementId = requirementId;
					task.RiskId = riskId;
					task.ReleaseId = releaseId;
					task.CreatorId = creatorId;
					task.OwnerId = ownerId;
					task.TaskPriorityId = taskPriorityId;
					task.Name = name.MaxLength(255);
					task.Description = description;
					task.CreationDate = DateTime.UtcNow;
					task.LastUpdateDate = DateTime.UtcNow;
					task.ConcurrencyDate = DateTime.UtcNow;
					task.StartDate = startDate;
					task.EndDate = endDate;
					task.CompletionPercent = completionPercent;
					task.EstimatedEffort = estimatedEffort;
					task.ActualEffort = actualEffort;
					task.ProjectedEffort = projectedEffort;
					task.RemainingEffort = remainingEffort;
					task.IsAttachments = false;
					task.IsDeleted = false;

					//Save task and capture ID
					context.Tasks.AddObject(task);
					context.SaveChanges();
					taskId = task.TaskId;
				}

				//Add a history record for the inserted task.
				if (logHistory)
				{
					HistoryManager historyManager = new HistoryManager();
					historyManager.LogCreation(projectId, ((userId.HasValue) ? userId.Value : creatorId), DataModel.Artifact.ArtifactTypeEnum.Task, taskId, DateTime.UtcNow);
				}

				//Now refresh the requirement's and release's progress/effort information if appropriate
				if (requirementId.HasValue)
				{
					RequirementManager requirement = new RequirementManager();
					requirement.RefreshTaskProgressAndTestCoverage(projectId, requirementId.Value);
				}
				if (releaseId.HasValue)
				{
					ReleaseManager releaseManager = new ReleaseManager();
					releaseManager.RefreshProgressEffortTestStatus(projectId, releaseId.Value, false, true);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return taskId;
		}

		/// <summary>De-associates a task from its currently assigned risk</summary>
		/// <param name="taskId">The ID of the task to be de-associated</param>
		/// <param name="changerId">The ID of the person making the change</param>
		public void RemoveRiskAssociation(int taskId, int changerId)
		{
			const string METHOD_NAME = "RemoveRiskAssociation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the task to see if it's linked to a risk
					var query = from t in context.Tasks
								where t.TaskId == taskId
								select t;

					//Get the task, if it doesn't exist, just ignore
					Task task = query.FirstOrDefault();
					if (task != null && task.RiskId.HasValue)
					{
						//Store the risk and project for use later then set to null
						int projectId = task.ProjectId;
						int riskId = task.RiskId.Value;
						task.StartTracking();
						task.RiskId = null;
						task.LastUpdateDate = DateTime.UtcNow;
						task.ConcurrencyDate = DateTime.UtcNow;

						//Make sure changes are tracked in history
						context.DetectChanges();

						//Save changes
						context.SaveChanges(changerId, true, false, null);

						new HistoryManager().LogDeletion(projectId, changerId, DataModel.Artifact.ArtifactTypeEnum.Task, task.TaskId, DateTime.UtcNow);
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

		/// <summary>De-associates a task from its currently assigned requirement</summary>
		/// <param name="taskId">The ID of the task to be de-associated</param>
		/// <param name="changerId">The ID of the person making the change</param>
		public void RemoveRequirementAssociation(int taskId, int changerId)
		{
			const string METHOD_NAME = "RemoveRequirementAssociation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the task to see if it's linked to a requirement
					var query = from t in context.Tasks
								where t.TaskId == taskId
								select t;

					//Get the task, if it doesn't exist, just ignore
					Task task = query.FirstOrDefault();
					if (task != null && task.RequirementId.HasValue)
					{
						//Store the requirement and project for use later then set to null
						int projectId = task.ProjectId;
						int requirementId = task.RequirementId.Value;
						task.StartTracking();
						task.RequirementId = null;
						task.LastUpdateDate = DateTime.UtcNow;
						task.ConcurrencyDate = DateTime.UtcNow;

						//Make sure changes are tracked in history
						context.DetectChanges();

						//Save changes
						context.SaveChanges(changerId, true, false, null);

						new HistoryManager().LogDeletion(projectId, changerId, DataModel.Artifact.ArtifactTypeEnum.Task, task.TaskId, DateTime.UtcNow);

						//Now refresh the linked requirement if appropriate
						RequirementManager requirement = new RequirementManager();
						requirement.RefreshTaskProgressAndTestCoverage(projectId, requirementId);
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

		/// <summary>Associates a task from its currently assigned requirement to a new one</summary>
		/// <param name="taskId">The ID of the task to be associated</param>
		/// <param name="requirementId">The id of the requirement</param>
		/// <param name="changerId">The ID of the person making the change</param>
		public void Task_AssociateWithRequirement(int taskId, int requirementId, int changerId)
		{
			const string METHOD_NAME = "Task_AssociateWithRequirement";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					int? existingRequirementId = null;
					//We need to initially retrieve the task to see if it's linked to a requirement
					var query = from t in context.Tasks
								where t.TaskId == taskId
								select t;

					//Get the task, if it doesn't exist, just ignore
					Task task = query.FirstOrDefault();
					if (task != null)
					{
						//Store the requirement and project for use later
						int projectId = task.ProjectId;
						existingRequirementId = task.RequirementId;
						task.StartTracking();
						task.RequirementId = requirementId;
						task.LastUpdateDate = DateTime.UtcNow;
						task.ConcurrencyDate = DateTime.UtcNow;

						//Save changes
						context.SaveChanges(changerId, true, false, null);

						//Now refresh the linked requirements if appropriate
						RequirementManager requirement = new RequirementManager();
						requirement.RefreshTaskProgressAndTestCoverage(projectId, requirementId);
						if (existingRequirementId.HasValue)
						{
							requirement.RefreshTaskProgressAndTestCoverage(projectId, existingRequirementId.Value);
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

		/// <summary>De-associates tasks from their currently assigned release</summary>
		/// <param name="taskIds">The ID of the tasks to be de-associated</param>
		/// <param name="changerId">The ID of the person making the change</param>
		public void RemoveReleaseAssociation(List<int> taskIds, int changerId)
		{
			const string METHOD_NAME = "RemoveReleaseAssociation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? projectId = null;
				List<int> releaseIds = new List<int>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the task to see if it's linked to a release
					var query = from t in context.Tasks
								where taskIds.Contains(t.TaskId) && t.ReleaseId.HasValue && !t.IsDeleted
								select t;

					//Get the tasks
					List<Task> tasks = query.ToList();
					foreach (Task task in tasks)
					{
						//Store the release and project for use later then set to null
						projectId = task.ProjectId;
						if (!releaseIds.Contains(task.ReleaseId.Value))
						{
							releaseIds.Add(task.ReleaseId.Value);
						}
						task.StartTracking();
						task.ReleaseId = null;
						task.LastUpdateDate = DateTime.UtcNow;
						task.ConcurrencyDate = DateTime.UtcNow;

						//Save changes
						context.SaveChanges(changerId, true, false, null);
					}
				}

				//Now refresh the linked release(s) if appropriate
				ReleaseManager releaseManager = new ReleaseManager();
				if (projectId.HasValue && releaseIds.Count > 0)
				{
					releaseManager.RefreshProgressEffortTestStatus(projectId.Value, releaseIds);
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

		/// <summary>Marks a Task as being deleted and no longer available in the system.</summary>
		/// <param name="taskId">The ID to mark as 'deleted'.</param>
		/// <param name="userId">The userId of the user performing the delete.</param>
		public void MarkAsDeleted(int projectId, int taskId, int userId)
		{
			const string METHOD_NAME = "MarkAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the task (cannot be already deleted)
					var query = from t in context.Tasks
								where t.TaskId == taskId && !t.IsDeleted
								select t;

					//Get the task
					Task task = query.FirstOrDefault();
					if (task != null)
					{
						//See if we need to refresh the requirement's progress/effort information
						int? requirementId = task.RequirementId;
						int? releaseId = task.ReleaseId;

						//Mark as deleted
						task.StartTracking();
						task.LastUpdateDate = DateTime.UtcNow;
						task.IsDeleted = true;

						//Save changes, no history logged, that's done later for the delete
						context.SaveChanges();

						//Add a changeset to mark it as deleted.
						//new HistoryManager().LogDeletion(projectId, userId, Artifact.ArtifactTypeEnum.Task, taskId, DateTime.UtcNow);

						//Add a changeset to mark it as deleted.
						new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, DateTime.UtcNow);

						//Now refresh the linked requirement if appropriate
						if (requirementId.HasValue)
						{
							new RequirementManager().RefreshTaskProgressAndTestCoverage(projectId, requirementId.Value);
						}

						//Now refresh the linked release if appropriate
						if (releaseId.HasValue)
						{
							new ReleaseManager().RefreshProgressEffortTestStatus(projectId, releaseId.Value);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Deletes a task in the system that has the specified ID</summary>
		/// <param name="taskId">The ID of the task to be deleted</param>
		public void DeleteFromDatabase(int taskId, int userId)
		{
			const string METHOD_NAME = "DeleteFromDatabase()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the task to see if it's linked to a requirement
				Task task = null;
				try
				{
					task = RetrieveById(taskId, true);
				}
				catch (ArtifactNotExistsException)
				{
					//If it's already deleted, just fail quietly
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}
				int projectId = task.ProjectId;

				//First we need to delete any attachments associated with the task
				Business.AttachmentManager attachment = new Business.AttachmentManager();
				attachment.DeleteByArtifactId(taskId, DataModel.Artifact.ArtifactTypeEnum.Task);

				//Next we need to delete any custom properties associated with the task			
				new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(taskId, DataModel.Artifact.ArtifactTypeEnum.Task);

				//Finally call the stored procedure to delete the task itself
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Task_Delete(taskId);
				}

				//Log the purge.
				new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, DateTime.UtcNow, task.Name);
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
		/// Retrieves the task progress by project for a specific project group
		/// </summary>
		/// <param name="activeReleasesOnly">Do we only want the data for active releases</param>
		/// <param name="projectGroupId">The id of the project group we're interested in</param>
		/// <returns>The task progress per-project</returns>
		public List<ProjectTaskProgressEntryView> RetrieveProgressByProject(int projectGroupId, bool activeReleasesOnly)
		{
			const string METHOD_NAME = "RetrieveProgressByProject()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectTaskProgressEntryView> results;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure to populate the view
					results = context.Task_RetrieveGroupSummaryByProject(projectGroupId, activeReleasesOnly).ToList();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return results;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the task progress summary for a specific project
		/// </summary>
		/// <param name="projectId">The id of the project we're interested in</param>
		/// <returns>The task progress</returns>
		public ProjectTaskProgressEntryView RetrieveProjectProgressSummary(int projectId)
		{
			const string METHOD_NAME = "RetrieveProjectProgressSummary()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectTaskProgressEntryView results;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Now get the task progress per-project
					var query = from t in context.ProjectTaskProgressEntriesView
								where t.ProjectId == projectId
								select t;

					results = query.FirstOrDefault();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return results;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ProjectTaskProgressEntryView RetrieveByReleaseidProjectProgressSummary(int projectId,int? Releaseid)
		{
			const string METHOD_NAME = "RetrieveProjectProgressSummary()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectTaskProgressEntryView results;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Now get the task progress per-project
					var query = from t in context.ProjectTaskProgressEntriesView
								where t.ProjectId == projectId &&
								t.ReleaseId== Releaseid
								select t;

					results = query.FirstOrDefault();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return results;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TST_ARTIFACT_SIGNATURE RetrieveTaskSignature(int taskId, int artifactTypeId)
		{
			const string METHOD_NAME = "RetrieveTaskSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of task in the project
				TST_ARTIFACT_SIGNATURE taskSignature;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.ArtifactSignatures
								where t.ARTIFACT_ID == taskId && t.ARTIFACT_TYPE_ID == artifactTypeId
								select t;

					query = query.OrderByDescending(r => r.UPDATE_DATE);

					taskSignature = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return taskSignature;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void TaskSignatureInsert(int projectId, int currentStatusId, Task task, string meaning, int? loggedinUserId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				DateTime updatedDate = DateTime.Now;

				var newReqSignature = new TST_ARTIFACT_SIGNATURE
				{
					STATUS_ID = currentStatusId,
					ARTIFACT_ID = task.TaskId,
					ARTIFACT_TYPE_ID = (int)ArtifactTypeEnum.Task,
					USER_ID = (int)loggedinUserId,
					UPDATE_DATE = DateTime.Now,
					MEANING = meaning,
				};

				context.ArtifactSignatures.AddObject(newReqSignature);

				context.SaveChanges();
				//log history
				new HistoryManager().LogCreation(projectId, (int)loggedinUserId, Artifact.ArtifactTypeEnum.TaskSignature, task.TaskId, DateTime.UtcNow);

			}
		}

		public TaskStatus RetrieveStatusById(int statusId)
		{
			const string METHOD_NAME = "RetrieveStatusById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				TaskStatus status;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TaskStati
								where t.TaskStatusId == statusId
								select t;

					status = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return status;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the task progress summary for a specific project group as well as the count by group
		/// </summary>
		/// <param name="activeReleasesOnly">Do we only want active releases</param>
		/// <param name="projectGroupId">The id of the project group we're interested in</param>
		/// <returns>A dataset of two tables, one summary and one per-project</returns>
		/// <remarks>It actually sums up the information cached on the TST_RELEASE table</remarks>
		public List<Task_GroupSummary> RetrieveProgressSummary(int projectGroupId, bool activeReleasesOnly)
		{
			const string METHOD_NAME = "RetrieveProgressSummary()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Task_GroupSummary> results;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure for retrieving the group summary data
					results = context.Task_RetrieveGroupSummary(projectGroupId, activeReleasesOnly).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return results;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Calculates the %complete and projected effort columns for a task datarow
		/// </summary>
		/// <param name="task">The task entity</param>
		public static void CalculateCompletion(Task task)
		{
			//Need to convert the datarow fields into standard .net types
			Task.TaskStatusEnum taskStatus = (Task.TaskStatusEnum)task.TaskStatusId;
			int? estimatedEffort = task.EstimatedEffort;
			int? actualEffort = task.ActualEffort;
			int? remainingEffort = task.RemainingEffort;
			int? projectedEffort = null;
			int completionPercentage = 0;
			CalculateCompletion(taskStatus, estimatedEffort, actualEffort, remainingEffort, out completionPercentage, out projectedEffort);
			task.CompletionPercent = completionPercentage;
			task.ProjectedEffort = projectedEffort;
		}

		/// <summary>Calculates the %complete and projected effort</summary>
		/// <param name="estimatedEffort">The estimated effort</param>
		/// <param name="actualEffort">The actual effort</param>
		/// <param name="remainingEffort">The remaining effort</param>
		/// <param name="completionPercentage">The completion percentage (out)</param>
		/// <param name="projectedEffort">The projected effort (out)</param>
		/// <param name="taskStatus">The status of the task</param>
		public static void CalculateCompletion(Task.TaskStatusEnum taskStatus, Nullable<int> estimatedEffort, Nullable<int> actualEffort, Nullable<int> remainingEffort, out int completionPercentage, out Nullable<int> projectedEffort)
		{
			//Handle the special cases of Completed or Not Started tasks
			if (taskStatus == Task.TaskStatusEnum.NotStarted)
			{
				//If the task is not started, show 0% complete with all the effort values in their starting point
				completionPercentage = 0;
				actualEffort = null;
				remainingEffort = estimatedEffort;
				projectedEffort = actualEffort;
				if (!projectedEffort.HasValue)
				{
					projectedEffort = estimatedEffort;
				}
				return;
			}

			if (taskStatus == Task.TaskStatusEnum.Completed)
			{
				//If the task is completed, it's 100% complete with projected effort the same as actual
				//unless there is no actual, in which case it's the same as estimated
				completionPercentage = 100;
				if (actualEffort.HasValue)
				{
					projectedEffort = actualEffort.Value;
				}
				else if (estimatedEffort.HasValue)
				{
					projectedEffort = estimatedEffort.Value;
				}
				else
				{
					projectedEffort = null;
				}
				return;
			}

			//If we have no estimated effort then default to 0% complete
			if (!estimatedEffort.HasValue)
			{
				completionPercentage = 0;
				projectedEffort = null;
				return;
			}
			//Handle the special case of a zero-effort task
			if (estimatedEffort.Value == 0)
			{
				completionPercentage = 100;
				projectedEffort = 0;
			}
			if (!remainingEffort.HasValue)
			{
				//If we have a remaining effort value not set then assume 0
				remainingEffort = 0;
			}
			//Now we know we have an estimated effort and remaining effort value
			double percentRemaining = ((double)remainingEffort.Value / (double)estimatedEffort.Value) * 100D;
			completionPercentage = (int)(100D - percentRemaining);

			//Now we need to handle the projected effort
			if (actualEffort.HasValue)
			{
				projectedEffort = actualEffort + remainingEffort;
			}
			else
			{
				//If we have no actual hours logged, we don't truly know if this task will take longer or not
				//So for now we shall simply set the projected effort to the estimated effort
				//In future we might be able to calculate it based on the resource dates, etc.
				projectedEffort = estimatedEffort;
			}

			//Make sure %completion is in the range 0% - 100%
			if (completionPercentage < 0)
			{
				completionPercentage = 0;
			}
			if (completionPercentage > 100)
			{
				completionPercentage = 100;
			}
		}

		/// <summary>Calculates the colors to display in the task progress bar</summary>
		/// <param name="percentGreen">The % that should be displayed as green</param>
		/// <param name="percentRed">The % that should be displayed as red</param>
		/// <param name="percentYellow">The % that should be displayed as yellow</param>
		/// <param name="percentGray">The % that should be displayed as gray</param>
		/// <returns>The textual version of the progress information</returns>
		/// <param name="task">The task entity</param>
		/// <param name="utcOffset">The offset from UTC</param>
		public static string CalculateProgress(Task task, double utcOffset, out int percentGreen, out int percentRed, out int percentYellow, out int percentGray)
		{
			//Set the default percents
			percentGreen = 0;
			percentRed = 0;
			percentYellow = 0;
			percentGray = 0;

			//We need to get the start-date, end-date and % completion
			DateTime? startDate = task.StartDate;
			DateTime? endDate = task.EndDate;
			int percentComplete = task.CompletionPercent;

			//Now populate the equalizer graph
			string tooltipText = percentComplete + GlobalResources.General.Task_ProgressPercentComplete;

			//See if we're completed or not
			if (percentComplete == 0)
			{
				//We've not yet started so display as gray unless the start date has already been passed
				//and the task is in the not started status (vs. deferred)
				//If the end-date has passed and we're in-progress, show as running late
				if (!startDate.HasValue || startDate >= DateTime.UtcNow || task.TaskStatusId == (int)Task.TaskStatusEnum.Deferred)
				{
					percentGray = 100;
					tooltipText += ", " + GlobalResources.General.Task_ProgressNotStarted;
				}
				else if (task.TaskStatusId == (int)Task.TaskStatusEnum.InProgress)
				{
					percentGray = 100;
					if (endDate.HasValue)
					{
						if (endDate < DateTime.UtcNow.Date)
						{
							tooltipText += ", " + GlobalResources.General.Task_Progress_ShouldHaveFinishedOn + " " + String.Format(Dates.FORMAT_DATE, endDate.Value.ToLocalDate(utcOffset));
						}
						else
						{
							tooltipText += ", " + GlobalResources.General.Task_ProgressScheduledEndOn + " " + String.Format(Dates.FORMAT_DATE, endDate.Value.ToLocalDate(utcOffset));
						}
					}
				}
				else
				{
					percentYellow = 100;
					tooltipText += ", " + GlobalResources.General.Task_ProgressShouldHaveStartedOn + " " + String.Format(Dates.FORMAT_DATE, startDate.Value.ToLocalDate(utcOffset));
				}
			}
			else if (percentComplete < 100)
			{
				if (endDate.HasValue)
				{
					//If not complete, show bar to indicate progress
					//If the end date has passed, show red bar, otherwise show green
					if (endDate >= DateTime.UtcNow)
					{
						percentGreen = percentComplete;
						tooltipText += ", " + GlobalResources.General.Task_ProgressScheduledEndOn + " " + String.Format(Dates.FORMAT_DATE, endDate.Value.ToLocalDate(utcOffset));
					}
					else
					{
						percentRed = percentComplete;
						tooltipText += ", " + GlobalResources.General.Task_Progress_ShouldHaveFinishedOn + " " + String.Format(Dates.FORMAT_DATE, endDate.Value.ToLocalDate(utcOffset));
					}
				}
				else
				{
					//No end date, so show progress as green and don't mention date in tooltip
					percentGreen = percentComplete;
				}
				percentGray = 100 - percentComplete;
			}
			else
			{
				//If completed, just show green bar
				percentGreen = 100;
			}
			return tooltipText;
		}

		/// <summary>Handles any Task specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">The current project template</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleTaskSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			string filterProperty = filter.Key;
			object filterValue = filter.Value;

			//Handle the special case of progress, since it doesn't map to a single column
			if (filterProperty == "ProgressId")
			{
				if (filterValue is Int32)
				{
					switch ((int)filterValue)
					{
						//Not Started
						case 1:
							{
								//AND TSK.COMPLETION_PERCENT = 0 AND (TSK.START_DATE >= GETDATE() OR TSK.TASK_STATUS_ID = " + (int)Task.TaskStatusEnum.Deferred + ") ";
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression startDateExp = LambdaExpression.PropertyOrField(p, "StartDate");
								MemberExpression taskStatusIdExp = LambdaExpression.PropertyOrField(p, "TaskStatusId");
								Expression expression1 = Expression.Equal(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2a = Expression.GreaterThanOrEqual(startDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								Expression expression2b = Expression.Equal(taskStatusIdExp, LambdaExpression.Constant((int)Task.TaskStatusEnum.Deferred));
								Expression expression2 = Expression.Or(expression2a, expression2b);
								expressionList.Add(expression2);
								break;
							}
						//Late Starting
						case 2:
							{
								//AND TSK.COMPLETION_PERCENT = 0 AND TSK.START_DATE < GETDATE() AND TSK.TASK_STATUS_ID = " + ((int)Task.TaskStatusEnum.NotStarted).ToString() + " ";
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression startDateExp = LambdaExpression.PropertyOrField(p, "StartDate");
								MemberExpression taskStatusIdExp = LambdaExpression.PropertyOrField(p, "TaskStatusId");
								Expression expression1 = Expression.Equal(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(startDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								expressionList.Add(expression2);
								Expression expression3 = Expression.Equal(taskStatusIdExp, LambdaExpression.Constant((int)Task.TaskStatusEnum.NotStarted));
								expressionList.Add(expression3);
								break;
							}

						//On Schedule
						case 3:
							{
								//TSK.COMPLETION_PERCENT > 0 AND TSK.COMPLETION_PERCENT < 100 AND TSK.END_DATE >= GETDATE() ";
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression endDateExp = LambdaExpression.PropertyOrField(p, "EndDate");
								Expression expression1 = Expression.GreaterThan(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression2);
								Expression expression3 = Expression.GreaterThanOrEqual(endDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								expressionList.Add(expression3);
								break;
							}

						//Late Finishing
						case 4:
							{
								//TSK.COMPLETION_PERCENT < 100 AND TSK.END_DATE < GETDATE() AND (TSK.TASK_STATUS_ID = " + (int)Task.TaskStatusEnum.InProgress + " OR TSK.TASK_STATUS_ID = " + (int)Task.TaskStatusEnum.Blocked + ")
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression endDateExp = LambdaExpression.PropertyOrField(p, "EndDate");
								MemberExpression taskStatusIdExp = LambdaExpression.PropertyOrField(p, "TaskStatusId");
								Expression expression1 = Expression.LessThan(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(endDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								expressionList.Add(expression2);
								ConstantExpression statusListExp = LambdaExpression.Constant(new List<int>() { (int)Task.TaskStatusEnum.InProgress, (int)Task.TaskStatusEnum.Blocked });
								Expression expression3 = Expression.Call(statusListExp, "Contains", null, taskStatusIdExp);
								expressionList.Add(expression3);
								break;
							}

						//Completed
						case 5:
							{
								//COMPLETION_PERCENT = 100
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								Expression expression = Expression.Equal(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression);
							}
							break;
					}
				}
				return true;
			}

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

			//By default, let the generic filter convertor handle the filter
			return false;
		}

		/// <summary>
		/// Gets the HTML color code for each task progress indicator (used in graphs)
		/// </summary>
		/// <param name="progressId">The id of the progress</param>
		/// <returns>The html hex color code</returns>
		/// <remarks>Cannot use css with the graphs</remarks>
		public static string GetProgressColor(int progressId)
		{
			switch (progressId)
			{
				case 1:
					/* On Schedule */
					return "43BB52";

				case 2:
					/* Late Finish */
					return "f47457";

				case 3:
					/* Late Start */
					return "F62525";

				case 4:
					/* Not Started */
					return "e0e0e0";

				case 5:
					/* Completed */
					return "2E8E20";

				default:
					return "";
			}
		}

		/// <summary>Counts all the tasks in the project</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="folderId">
		/// Should we filter by Folder. Provide null to not filter, provide NoneFilterValue to filter by
		/// tasks that are not in a folder
		/// </param>
		/// <returns>The total number of tasks</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Count(int projectId, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int taskCount = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TasksView
								where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
								select t;

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TaskView, bool>> filterClause = CreateFilterExpression<TaskView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Task, filters, utcOffset, null, HandleTaskSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TaskView>)query.Where(filterClause);
						}
					}

					//See if we need to filter by folder
					//If the folderId == NoneFilterValue and we have other filters set, ignore
					if (folderId.HasValue)
					{
						int folderIdValue = folderId.Value;
						if (folderIdValue == NoneFilterValue)
						{
							if (filters == null || filters.Count == 0)
							{
								query = query.Where(t => !t.TaskFolderId.HasValue);
							}
						}
						else
						{
							query = query.Where(t => t.TaskFolderId == folderIdValue);
						}
					}

					//Get the count
					taskCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return taskCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<TaskView> RetrieveTask(int projectId, int? releaseId = null, int? folderId = null)
		{
			const string METHOD_NAME = "RetrieveTask";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TasksView
								where (!t.IsDeleted || false) && t.ProjectId == projectId
								select t;

					if (folderId.HasValue)
					{
						int folderIdValue = folderId.Value;
						query = query.Where(t => t.TaskFolderId == folderIdValue);
					}

					if (releaseId > 0)
					{
						query = query.Where(r => r.ReleaseId == releaseId);
					}
					
					//Get the count
					int artifactCount = query.Count();

					//Execute the query
					tasks = query
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<TaskView> RetrieveTask1(int projectId, int? folderId = null)
		{
			const string METHOD_NAME = "RetrieveTask1";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TasksView
								where (!t.IsDeleted || false) && t.ProjectId == projectId
								select t;


					//See if we need to filter by folder
					//If the folderId == NoneFilterValue and we have other filters set, ignore
					if (folderId.HasValue)
					{
						int folderIdValue = folderId.Value;
						if (folderIdValue == NoneFilterValue)
						{
							query = query.Where(t => !t.TaskFolderId.HasValue);						
						}
						else
						{
							query = query.Where(t => t.TaskFolderId == folderIdValue);
						}
					}
					//Get the count
					int artifactCount = query.Count();

					//Execute the query
					tasks = query
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>Retrieves a list of all tasks in the system (for a project)</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="folderId">
		/// Should we filter by Folder. Provide null to not filter, provide NoneFilterValue to filter by
		/// tasks that are not in a folder
		/// </param>
		/// <param name="includeDeleted">Should we include deleted tasks</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <returns>TaskView list</returns>
		/// <remarks>Also brings across any associated custom properties</remarks>
		/// <param name="ignoreRootFolderIfFilterSet">Should we ignore the root folder restriction if a filter is applied (default: TRUE)</param>
		public List<TaskView> Retrieve(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool ignoreRootFolderIfFilterSet = true)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TasksView
								where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
								select t;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by last updated date descending
						query = query.OrderByDescending(t => t.LastUpdateDate).ThenBy(t => t.TaskId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "TaskId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TaskView, bool>> filterClause = CreateFilterExpression<TaskView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Task, filters, utcOffset, null, HandleTaskSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TaskView>)query.Where(filterClause);
						}
					}

					//See if we need to filter by folder
					//If the folderId == NoneFilterValue and we have other filters set, ignore
					if (folderId.HasValue)
					{
						int folderIdValue = folderId.Value;
						if (folderIdValue == NoneFilterValue)
						{
							if (filters == null || filters.Count == 0 || ignoreRootFolderIfFilterSet == false)
							{
								query = query.Where(t => !t.TaskFolderId.HasValue);
							}
						}
						else
						{
							query = query.Where(t => t.TaskFolderId == folderIdValue);
						}
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > artifactCount)
					{
						startRow = 1;
					}

					//Execute the query
					tasks = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns a sorted list of values to populate the lookup for the task progress filter</summary>
		/// <returns>Dictionary containing filter values</returns>
		public Dictionary<string, string> RetrieveProgressFiltersLookup()
		{
			const string METHOD_NAME = "RetrieveProgressFiltersLookup";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If we don't have the filters list populated, then create, otherwise just return
				if (this.progressFiltersList == null)
				{
					this.progressFiltersList = new Dictionary<string, string>();
					this.progressFiltersList.Add("1", GlobalResources.General.Task_NotStarted);
					this.progressFiltersList.Add("2", GlobalResources.General.Task_StartingLate);
					this.progressFiltersList.Add("3", GlobalResources.General.Task_OnSchedule);
					this.progressFiltersList.Add("4", GlobalResources.General.Task_RunningLate);
					this.progressFiltersList.Add("5", GlobalResources.General.Task_Completed);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return this.progressFiltersList;
		}

		/// <summary>Retrieves a particular task entity by its ID</summary>
		/// <param name="taskId">The ID of the task we want to retrieve</param>
		/// <returns>A task entity</returns>
		/// <seealso cref="TaskView_RetrieveById"/>
		public Task RetrieveById(int taskId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Task task;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task entity
					var query = from t in context.Tasks
								where t.TaskId == taskId && (!t.IsDeleted || includeDeleted)
								select t;

					task = query.FirstOrDefault();
				}
				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (task == null)
				{
					throw new ArtifactNotExistsException("Task " + taskId.ToString() + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the task
				return task;
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

		/// <summary>Retrieves a particular task view entity by its ID</summary>
		/// <param name="taskId">The ID of the task we want to retrieve</param>
		/// <param name="includeDeleted">Should we include deleted tasks</param>
		/// <returns>A task view entity</returns>
		/// <seealso cref="RetrieveById"/>
		/// <remarks>Use this option when you need all the lookup fields from the view</remarks>
		public TaskView TaskView_RetrieveById(int taskId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "TaskView_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TaskView task;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task entity
					var query = from t in context.TasksView
								where t.TaskId == taskId && (!t.IsDeleted || includeDeleted)
								select t;

					task = query.FirstOrDefault();
				}
				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (task == null)
				{
					throw new ArtifactNotExistsException("Task " + taskId.ToString() + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the task
				return task;
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

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns>List of soft-deleted tasks</returns>
		public List<TaskView> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TasksView
								where t.ProjectId == projectId && t.IsDeleted
								orderby t.TaskId
								select t;

					tasks = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return tasks;
			}
			catch (Exception ex)
			{
				//Do not rethrow, just return an empty list
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				return new List<TaskView>();
			}
		}

		/// <summary>Retrieves a list of all the open tasks for a user irrespective of project</summary>
		/// <param name="ownerId">The ID of the user we want to retrieve tasks for (pass null to retrieve all unassigned)</param>
		/// <param name="projectId">The id of the project, or null for all</param>
		/// <param name="releaseId">The id of the release (null for all)</param>
		/// <param name="includeCompleted">Include tasks that are completed</param>
		/// <param name="includeDeleted">Include deleted tasks</param>
		/// <param name="includeDeferred">Include deferred tasks</param>
		/// <param name="includeBlocked">Include blocked tasks</param>
		/// <param name="numberRows">The number of rows to return</param>
		/// <returns>A task list</returns>
		public List<TaskView> RetrieveByOwnerId(int? ownerId, int? projectId, int? releaseId, bool includeCompleted, int numberRows = 500, bool includeDeferred = true, bool includeBlocked = true, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByOwnerId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TasksView
								where (!t.IsDeleted || includeDeleted)
								orderby t.TaskPriorityName, t.TaskId
								select t;

					//See if we have a project specified
					if (projectId.HasValue)
					{
						query = (IOrderedQueryable<TaskView>)query.Where(t => t.ProjectId == projectId.Value);
						//See if we have the release id set
						if (releaseId.HasValue)
						{
							//For this function, we don't want to include child iterations
							query = (IOrderedQueryable<TaskView>)query.Where(t => t.ReleaseId == releaseId.Value);
						}
					}

					//See if we have the owner specified
					if (ownerId.HasValue)
					{
						query = (IOrderedQueryable<TaskView>)query.Where(t => t.OwnerId == ownerId.Value);
					}
					else
					{
						query = (IOrderedQueryable<TaskView>)query.Where(t => !t.OwnerId.HasValue);
					}

					if (!includeCompleted)
					{
						//See if we need to filter the statuses to just 'open' ones
						query = (IOrderedQueryable<TaskView>)query.Where(t => t.TaskStatusId != (int)Task.TaskStatusEnum.Completed && t.TaskStatusId != (int)Task.TaskStatusEnum.Obsolete && t.TaskStatusId != (int)Task.TaskStatusEnum.Rejected && t.TaskStatusId != (int)Task.TaskStatusEnum.Duplicate);
					}
					if (!includeDeferred)
					{
						//See if we need to filter the statuses to exclude deferred
						query = (IOrderedQueryable<TaskView>)query.Where(t => t.TaskStatusId != (int)Task.TaskStatusEnum.Deferred);
					}
					if (!includeBlocked)
					{
						//See if we need to filter the statuses to exclude blocked
						query = (IOrderedQueryable<TaskView>)query.Where(t => t.TaskStatusId != (int)Task.TaskStatusEnum.Blocked);
					}

					tasks = query.Take(numberRows).ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of task statuses that are in use by any workflows for the current template
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The list of statuses</returns>
		/// <remarks>Used in the task board to know which statuses to show</remarks>
		public List<TaskStatus> RetrieveStatusesInUse(int projectTemplateId)
		{
			const string METHOD_NAME = "RetrieveStatusesInUse()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskStatus> statuses;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.TaskStati
								where
									r.IsActive &&
									((r.WorkflowTransitionsInput.Any(w => w.Workflow.ProjectTemplateId == projectTemplateId) &&
									r.WorkflowTransitionsOutput.Any(w => w.Workflow.ProjectTemplateId == projectTemplateId)) ||
									r.TaskStatusId == (int)Task.TaskStatusEnum.NotStarted)
								orderby r.Position
								select r;

					statuses = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statuses;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of task statuses</summary>
		/// <returns>List of statuses</returns>
		public List<TaskStatus> RetrieveStatuses()
		{
			const string METHOD_NAME = "RetrieveStatuses";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				if (_staticTaskStatuses == null)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from t in context.TaskStati
									where t.IsActive
									orderby t.Position, t.TaskStatusId
									select t;

						_staticTaskStatuses = query.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return _staticTaskStatuses;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a new blank requirement record
		/// </summary>
		/// <param name="authorId">The id of the current user who will be initially set as the author</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The new blank incident entity with a single datarow</returns>
		/// <param name="typeId">The type of the new task (null = default)</param>
		public TaskView Task_New(int projectId, int authorId, int? typeId)
		{
			const string METHOD_NAME = "Task_New";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the project planning options
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Create the new entity
				TaskView task = new TaskView();
				TaskStatus defaultStatus = this.RetrieveStatuses().Where(r => r.TaskStatusId == (int)Task.TaskStatusEnum.NotStarted).FirstOrDefault();

				//If no task type specified, get the default one for the current project template
				if (!typeId.HasValue)
				{
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
					TaskType type = this.TaskType_RetrieveDefault(projectTemplateId);
					typeId = type.TaskTypeId;
				}

				//Populate the new requirement
				task.ProjectId = projectId;
				task.CreatorId = authorId;
				task.TaskStatusId = defaultStatus.TaskStatusId;
				task.TaskStatusName = defaultStatus.Name;
				task.TaskTypeId = typeId.Value;
				task.CreationDate = task.LastUpdateDate = task.ConcurrencyDate = DateTime.UtcNow;
				task.Name = "";
				task.Description = "";
				task.IsAttachments = false;
				task.EstimatedEffort = project.TaskDefaultEffort;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return task;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of tasks associated with a specific status, for either a specific release/iteration or for the product backlog
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="statusId">The id of the task status (null = backlog)</param>
		/// <param name="releaseId">The id of the release (null = product backlog, -2 = all/any release)</param>
		/// <returns>The list of tasks</returns>
		/// <remarks>
		/// Includes the child iterations of the release
		/// </remarks>
		public List<TaskView> Task_RetrieveBacklogByStatusId(int projectId, int? releaseId, int? statusId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Task_RetrieveBacklogByStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TasksView
								where
									t.ProjectId == projectId &&
									!t.IsDeleted
								select t;

					if (releaseId.HasValue)
					{
						//Add the status filter
						if (statusId.HasValue)
						{
							//Check for the 'all releases' (-2) case
							if (releaseId != -2)
							{
								//Need to be in the status and part of the release/child iteration
								List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
								query = query.Where(t => releaseAndIterations.Contains(t.ReleaseId.Value));
							}
							query = query.Where(t => t.TaskStatusId == statusId.Value);
						}
						else
						{
							//If no status, then only show items that are not part of the release
							query = query.Where(t => !t.ReleaseId.HasValue);
						}
					}
					else
					{
						//Only get items that have no release set (product backlog grouped by status)
						query = query.Where(t => !t.ReleaseId.HasValue);
						if (statusId.HasValue)
						{
							query = query.Where(t => t.TaskStatusId == statusId.Value);
						}
						else
						{
							//return no items in this case
							return new List<TaskView>();
						}
					}

					//Order by rank then ID
					query = query.OrderBy(t => t.TaskPriorityName).ThenBy(t => t.TaskId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					tasks = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of tasks associated with a specific status, for either a specific release/iteration or for all releases
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="priorityId">The id of the task priority (null = no priority set)</param>
		/// <param name="releaseId">The id of the release (null = all releases)</param>
		/// <returns>The list of tasks</returns>
		/// <remarks>
		/// Includes the child iterations of the release. Does not include rejected/deferred tasks
		/// </remarks>
		public List<TaskView> Task_RetrieveByPriorityId(int projectId, int? releaseId, int? priorityId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Task_RetrieveByPriorityId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;

				//Create the LINQ query for all undeleted, active tasks
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TasksView
								where
									t.ProjectId == projectId &&
									t.TaskStatusId != (int)Task.TaskStatusEnum.Rejected &&
									t.TaskStatusId != (int)Task.TaskStatusEnum.Deferred &&
									!t.IsDeleted
								select t;

					//Filter by release if necessary
					if (releaseId.HasValue)
					{
						//Need to be part of the release/child iteration (all statuses)
						List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
						query = query.Where(t => releaseAndIterations.Contains(t.ReleaseId.Value));
					}
					else
					{
						//Get only items in an active status (otherwise there will be too many)
						query = query.Where(t => (t.TaskStatusId == (int)Task.TaskStatusEnum.NotStarted ||
									t.TaskStatusId == (int)Task.TaskStatusEnum.UnderReview ||
									t.TaskStatusId == (int)Task.TaskStatusEnum.InProgress));
					}

					//Add the priority filter
					if (priorityId.HasValue)
					{
						query = query.Where(t => t.TaskPriorityId.Value == priorityId.Value);
					}
					else
					{
						query = query.Where(t => !t.TaskPriorityId.HasValue);
					}

					//Order by rank then ID
					query = query.OrderBy(t => t.TaskPriorityName).ThenBy(t => t.TaskId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					tasks = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all the tasks associated with a specific test run
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testRunId">The id of the test run</param>
		/// <returns>A task list</returns>
		public List<TaskView> RetrieveByTestRunId(int projectId, int testRunId)
		{
			const string METHOD_NAME = "RetrieveByTestRunId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TasksView
								join al in context.ArtifactLinks on t.TaskId equals al.SourceArtifactId
								where
									t.ProjectId == projectId &&
									!t.IsDeleted &&
									al.SourceArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task &&
									al.DestArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestRun &&
									al.DestArtifactId == testRunId
								orderby t.StartDate, t.EndDate, t.TaskId
								select t;

					tasks = query.ToList();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all the tasks for a particular release/iteration</summary>
		/// <param name="includeDeleted">Include deleted items</param>
		/// <param name="projectId">The current project</param>
		/// <param name="releaseId">The ID of the release/iteration we want to retrieve the tasks for</param>
		/// <returns>A task list</returns>
		/// <remarks>
		/// 1) Passing null for release, returns all tasks that have no release set
		/// 2) Does include child iterations, but not minor releases
		/// 3) Does not include rejected/deferred tasks
		/// </remarks>
		public List<TaskView> Task_RetrieveByReleaseIdWithIterations(int projectId, int? releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Task_RetrieveByReleaseIdWithIterations()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TasksView
								where t.ProjectId == projectId && (!t.IsDeleted || includeDeleted) &&
									t.TaskStatusId != (int)Task.TaskStatusEnum.Rejected &&
									t.TaskStatusId != (int)Task.TaskStatusEnum.Deferred
								orderby t.TaskPriorityName, t.TaskId
								select t;

					//See if we have the release id set
					if (releaseId.HasValue)
					{
						//Need to be part of the release/child iteration (all statuses)
						List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
						query = (IOrderedQueryable<TaskView>)query.Where(t => releaseAndIterations.Contains(t.ReleaseId.Value));
					}
					else
					{
						query = (IOrderedQueryable<TaskView>)query.Where(t => !t.ReleaseId.HasValue);
					}

					tasks = query.ToList();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>Retrieves all the tasks for a particular release/iteration</summary>
		/// <param name="includeDeleted">Include deleted items</param>
		/// <param name="projectId">The current project</param>
		/// <param name="releaseId">The ID of the release/iteration we want to retrieve the tasks for</param>
		/// <returns>A task list</returns>
		/// <remarks>
		/// 1) Passing null for release, returns all tasks that have no release set
		/// 2) Does NOT include child iterations
		/// </remarks>
		public List<TaskView> RetrieveByReleaseId(int projectId, Nullable<int> releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByReleaseId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TasksView
								where t.ProjectId == projectId && (!t.IsDeleted || includeDeleted)
								orderby t.StartDate, t.EndDate, t.TaskId
								select t;

					//See if we have the release id set
					if (releaseId.HasValue)
					{
						query = (IOrderedQueryable<TaskView>)query.Where(t => t.ReleaseId == releaseId.Value);
					}
					else
					{
						query = (IOrderedQueryable<TaskView>)query.Where(t => !t.ReleaseId.HasValue);
					}

					tasks = query.ToList();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all the tasks for a particular requirement</summary>
		/// <param name="releaseId">The ID of the requirement we want to retrieve the tasks for</param>
		/// <returns>A task list</returns>
		public List<TaskView> RetrieveByRequirementId(int requirementId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByRequirementId()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TasksView
								where (!t.IsDeleted || includeDeleted) &&
									t.RequirementId == requirementId
								orderby t.StartDate, t.EndDate, t.TaskId
								select t;
					tasks = query.ToList();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all the tasks for a particular risk</summary>
		/// <param name="riskId">The ID of the risk we want to retrieve the tasks for</param>
		/// <returns>A task list</returns>
		public List<TaskView> RetrieveByRiskId(int riskId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByRequirementId()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskView> tasks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TasksView
								where (!t.IsDeleted || includeDeleted) &&
									t.RiskId == riskId
								orderby t.StartDate, t.EndDate, t.TaskId
								select t;
					tasks = query.ToList();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return tasks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Undeletes a task, making it available to users.</summary>
		/// <param name="taskId">The task ID to undelete.</param>
		/// <param name="userId">The userId performing the undelete.</param>
		/// <param name="logHistory">Whether to log this to history or not. Default: TRUE</param>
		public void UnDelete(int taskId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//We need to initially retrieve the task (needs to be marked as deleted)
				var query = from t in context.Tasks
							where t.TaskId == taskId && t.IsDeleted
							select t;

				//Get the task
				Task task = query.FirstOrDefault();
				if (task != null)
				{
					//See if we need to refresh the requirement's progress/effort information
					int? requirementId = task.RequirementId;
					int? releaseId = task.ReleaseId;
					int projectId = task.ProjectId;

					//Mark as undeleted
					task.StartTracking();
					task.LastUpdateDate = DateTime.UtcNow;
					task.IsDeleted = false;

					//Save changes, no history logged, that's done later
					context.SaveChanges();

					//Log the undelete
					if (logHistory)
					{
						//Okay, mark it as being undeleted.
						new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, rollbackId, DateTime.UtcNow);

						//Now refresh the linked requirement if appropriate
						if (requirementId.HasValue)
							new RequirementManager().RefreshTaskProgressAndTestCoverage(projectId, requirementId.Value);

						//Now refresh the linked release if appropriate
						if (releaseId.HasValue)
							new ReleaseManager().RefreshProgressEffortTestStatus(projectId, releaseId.Value);
					}
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		#region TaskFolder functions

		/// <summary>
		/// Gets the list of child folders for a parent folder. If the folder id is null it gets the root folder
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="folderId">The parent folder id (or null for root folder)</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <returns>List of folders</returns>
		public List<TaskFolder> TaskFolder_GetByParentId(int projectId, int? folderId, string sortProperty = null, bool sortAscending = true, Hashtable filters = null, double utcOffset = 0)
		{
			const string METHOD_NAME = "TaskFolder_GetByParentId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskFolder> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We have to use this syntax because using int? == int? comparison in EF4 LINQ will
					//result in invalid SQL (== NULL instead of IS NULL)
					IQueryable<TaskFolder> query;
					if (folderId.HasValue)
					{
						query = from f in context.TaskFolders
								where f.ParentTaskFolderId == folderId.Value && f.ProjectId == projectId
								orderby f.Name, f.TaskFolderId
								select f;
					}
					else
					{
						query = from f in context.TaskFolders
								where f.ParentTaskFolderId == null && f.ProjectId == projectId
								orderby f.Name
								select f;
					}

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by name ascending
						query = query.OrderBy(f => f.Name).ThenBy(f => f.TaskFolderId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "TaskFolderId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TaskFolder, bool>> filterClause = CreateFilterExpression<TaskFolder>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Task, filters, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TaskFolder>)query.Where(filterClause);
						}
					}

					//Execute the query
					folders = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Commits any changes made to a folder entity
		/// </summary>
		/// <param name="folder">The folder entity</param>
		/// <remarks>Can be used for both reordering the folder and making updates to the specific one</remarks>
		public void TaskFolder_Update(TaskFolder folder)
		{
			const string METHOD_NAME = CLASS_NAME + "TaskFolder_Update(TaskFolder)";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//If the folder has switched from a root one, make sure that there is at least one other folder in the project
					if (folder.ChangeTracker.OriginalValues.ContainsKey("ParentTaskFolderId"))
					{
						if (folder.ChangeTracker.OriginalValues["ParentTaskFolderId"] == null && folder.ParentTaskFolderId.HasValue)
						{
							//See how many root folders we have
							var query = from t in context.TaskFolders
										where t.ProjectId == folder.ProjectId && !t.ParentTaskFolderId.HasValue && t.TaskFolderId != folder.TaskFolderId
										select t;

							//Make sure we have one remaining
							if (query.Count() < 1)
							{
								throw new ProjectDefaultTaskFolderException("You need to have at least one top-level task folder in the project");
							}
						}

						//Make sure the new parent folder is not a child of the current one (would create circular loop)
						if (folder.ParentTaskFolderId.HasValue)
						{
							List<TaskFolderHierarchyView> childFolders = this.TaskFolder_GetChildren(folder.ProjectId, folder.TaskFolderId, false);
							if (childFolders.Any(f => f.TaskFolderId == folder.ParentTaskFolderId.Value))
							{
								throw new FolderCircularReferenceException(GlobalResources.Messages.Task_CannotMakeParentChildOfCurrentTask);
							}
						}
					}

					context.TaskFolders.ApplyChanges(folder);
					context.SaveChanges();

					//Next refresh the folder hierarchy cache
					TaskFolder_RefreshHierarchy(folder.ProjectId);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Saving folder:");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Deletes a folder in the system, cascading any deletes to the child folders
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="folderId">The id of the folder</param>
		public void TaskFolder_Delete(int projectId, int folderId)
		{
			const string METHOD_NAME = "TaskFolder_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get this folder and any child ones
					var query = from f in context.TaskFoldersHierarchyView
								where f.ParentTaskFolderId == folderId || f.TaskFolderId == folderId
								orderby f.HierarchyLevel descending, f.TaskFolderId ascending
								select f;
					List<TaskFolderHierarchyView> childFolders = query.ToList();

					//If we have child folders, we need to delete them recursively
					for (int i = 0; i < childFolders.Count; i++)
					{
						//The database will unset the Folder Id of the tasks using the database cascade

						//Just do a 'detached' object delete by id
						TaskFolder folderToDelete = new TaskFolder();
						folderToDelete.TaskFolderId = childFolders[i].TaskFolderId;
						context.TaskFolders.Attach(folderToDelete);
						context.ObjectStateManager.ChangeObjectState(folderToDelete, EntityState.Deleted);
						context.SaveChanges();
					}
				}

				//Next refresh the folder hierarchy cache
				TaskFolder_RefreshHierarchy(projectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies a task folder (and its child tasks) from one location to another. Will not copy deleted tasks.</summary>
		/// <param name="userId">The user that is making the copy</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceTaskFolderId">The folder we want to copy</param>
		/// <param name="destTaskFolderId">The folder we want to copy it into (null = root)</param>
		/// <param name="appendName">Should we append ' - Copy' to name</param>
		/// <returns>The id of the copy of the task folder</returns>
		public int TaskFolder_Copy(int userId, int projectId, int sourceTaskFolderId, int? destTaskFolderId, bool appendName = true)
		{
			const string METHOD_NAME = "TaskFolder_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the task folder being copied
				TaskFolder sourceTaskFolder = TaskFolder_GetById(sourceTaskFolderId);

				string name = sourceTaskFolder.Name;
				if (appendName)
				{
					name = name + CopiedArtifactNameSuffix;
				}

				//Firstly copy the task folder itself
				int copiedTaskFolderId = TaskFolder_Create(
					name,
					projectId,
					destTaskFolderId).TaskFolderId;

				//Next we need to copy all the child tasks
				List<TaskView> childTasks = Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, 0, sourceTaskFolderId);
				foreach (TaskView childTask in childTasks)
				{
					//Copy the test case, leaving its name unchanged
					Copy(userId, childTask.TaskId, copiedTaskFolderId, false);
				}

				//Next we need to recursively call this function for any child task folders
				List<TaskFolder> childTaskFolders = TaskFolder_GetByParentId(projectId, sourceTaskFolderId);
				if (childTaskFolders != null && childTaskFolders.Count > 0)
				{
					foreach (TaskFolder childTaskFolder in childTaskFolders)
					{
						//Copy the folder into the new folder, leaving the name unchanged
						TaskFolder_Copy(userId, projectId, childTaskFolder.TaskFolderId, copiedTaskFolderId, false);
					}
				}

				//Finally refresh the folder hierarchy cache
				TaskFolder_RefreshHierarchy(projectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the test case id of the copy
				return copiedTaskFolderId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Exports a task folder (and their children) from one project to another</summary>
		/// <param name="userId">The user exporting the test folder</param>
		/// <param name="sourceProjectId">The project we're exporting from</param>
		/// <param name="sourceTaskFolderId">The id of the test folder being exported</param>
		/// <param name="destProjectId">The project we're exporting to</param>
		/// <param name="taskFolderMapping">A dictionary used to keep track of any exported task folders</param>
		/// <returns>The id of the task folder in the new project</returns>
		public int TaskFolder_Export(int userId, int sourceProjectId, int sourceTaskFolderId, int destProjectId, Dictionary<int, int> taskFolderMapping)
		{
			const string METHOD_NAME = "TaskFolder_Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the task folder being copied
				TaskFolder sourceTaskFolder = TaskFolder_GetById(sourceTaskFolderId);

				int? destParentFolderId = null;
				if (sourceTaskFolder.ParentTaskFolderId.HasValue)
				{
					if (taskFolderMapping.ContainsKey(sourceTaskFolder.ParentTaskFolderId.Value))
					{
						destParentFolderId = taskFolderMapping[sourceTaskFolder.ParentTaskFolderId.Value];
					}
				}

				//Firstly export the task folder itself
				int exportedTaskFolderId = TaskFolder_Create(
					sourceTaskFolder.Name,
					destProjectId,
					destParentFolderId).TaskFolderId;

				//Add to the mapping
				if (!taskFolderMapping.ContainsKey(sourceTaskFolderId))
				{
					taskFolderMapping.Add(sourceTaskFolderId, exportedTaskFolderId);
				}

				//Next we need to export all the child tasks
				List<TaskView> childTasks = Retrieve(sourceProjectId, "Name", true, 1, Int32.MaxValue, null, 0, sourceTaskFolderId);
				foreach (TaskView childTask in childTasks)
				{
					//Copy the task, leaving its name unchanged
					Export(childTask.TaskId, destProjectId, userId, exportedTaskFolderId);
				}

				//Next we need to recursively call this function for any child task folders
				List<TaskFolder> childTaskFolders = TaskFolder_GetByParentId(sourceProjectId, sourceTaskFolderId);
				if (childTaskFolders != null && childTaskFolders.Count > 0)
				{
					foreach (TaskFolder childTaskFolder in childTaskFolders)
					{
						//Copy the folder into the new folder, leaving the name unchanged
						TaskFolder_Export(userId, sourceProjectId, childTaskFolder.TaskFolderId, destProjectId, taskFolderMapping);
					}
				}

				//Finally refresh the folder hierarchy cache
				TaskFolder_RefreshHierarchy(destProjectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the test folder id of the exported version
				return exportedTaskFolderId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Creates a new task folder</summary>
		/// <param name="name">Name of the folder.</param>
		/// <returns>The newly created task folder</returns>
		/// <param name="projectId">The id of the project</param>
		/// <param name="parentTaskFolderId">The id of a parent folder (null = top-level folder)</param>
		public TaskFolder TaskFolder_Create(string name, int projectId, int? parentTaskFolderId = null)
		{
			const string METHOD_NAME = "TaskFolder_Create";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			TaskFolder taskFolder = new TaskFolder();

			try
			{

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create a new task folder entity
					Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Creating new Task Folder Entity");

					taskFolder.Name = name;
					taskFolder.ProjectId = projectId;
					taskFolder.ParentTaskFolderId = parentTaskFolderId;

					//Persist the new article folder
					context.TaskFolders.AddObject(taskFolder);
					context.SaveChanges();

					//Persist changes
					context.SaveChanges();
				}

				//Next refresh the folder hierarchy cache
				TaskFolder_RefreshHierarchy(projectId);
			}
			catch (EntityForeignKeyException exception)
			{
				//This exception occurs if the parent has been deleted, throw a business exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new ArtifactNotExistsException("The parent task folder specified no longer exists", exception);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return taskFolder;
		}

		/// <summary>
		/// Gets a folder by its id
		/// </summary>
		/// <param name="folderId">The folder id</param>
		/// <returns>List of folders</returns>
		public TaskFolder TaskFolder_GetById(int folderId)
		{
			const string METHOD_NAME = "TaskFolder_GetById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TaskFolder folder;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.TaskFolders
								where f.TaskFolderId == folderId
								select f;
					folder = query.FirstOrDefault();
				}

				//Make sure data was returned
				if (folder == null)
				{
					throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.TaskManager_FolderNotExists, folderId));
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folder;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Checks if a folder exists by its id in the specified project
		/// </summary>
		/// <param name="projectId">The folder id</param> 
		/// <param name="folderId">The folder id</param>
		/// <returns>bool of true if the folder exists in the project</returns>
		public bool TaskFolder_Exists(int projectId, int folderId)
		{
			const string METHOD_NAME = "TaskFolder_Exists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TaskFolder folder;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.TaskFolders
								where f.TaskFolderId == folderId && f.ProjectId == projectId
								select f;
					folder = query.FirstOrDefault();
				}

				//Make sure data was returned
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folder == null ? false : true;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, "Looking for TaskFolder");
				throw;
			}
		}

		/// <summary>
		/// Gets the list of all parents of the specified folder in hierarchy order
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>List of folders</returns>
		public List<TaskFolderHierarchyView> TaskFolder_GetParents(int projectId, int testFolderId, bool includeSelf = false)
		{
			const string METHOD_NAME = "TaskFolder_GetParents";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskFolderHierarchyView> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					folders = context.Task_RetrieveParentFolders(projectId, testFolderId, includeSelf).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets the list of all children of the specified folder in hierarchy order
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>List of folders</returns>
		public List<TaskFolderHierarchyView> TaskFolder_GetChildren(int projectId, int taskFolderId, bool includeSelf = false)
		{
			const string METHOD_NAME = "TaskFolder_GetChildren";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskFolderHierarchyView> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					folders = context.Task_RetrieveChildFolders(projectId, taskFolderId, includeSelf).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets the list of all folders in the project according to their hierarchical relationship, in alphabetical order (per-level)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>List of folders</returns>
		public List<TaskFolderHierarchyView> TaskFolder_GetList(int projectId)
		{
			const string METHOD_NAME = "TaskFolder_GetList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TaskFolderHierarchyView> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.TaskFoldersHierarchyView
								where f.ProjectId == projectId
								orderby f.IndentLevel, f.TaskFolderId
								select f;

					folders = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Refreshes the task folder hierachy in a project after folders are changed
		/// </summary>
		/// <param name="projectId">The ID of the current project</param>
		public void TaskFolder_RefreshHierarchy(int projectId)
		{
			const string METHOD_NAME = "TaskFolder_RefreshHierarchy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Set a longer timeout for this as it's run infrequently to speed up retrieves
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
					context.Task_RefreshFolderHierarchy(projectId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
		}

		#endregion
	}

	/// <summary>This exception is thrown when you try and set the start or end date of task such that it lies outside of the range of its associated iteration/release</summary>
	public class TaskDateOutOfBoundsException : ApplicationException
	{
		public TaskDateOutOfBoundsException()
		{
		}
		public TaskDateOutOfBoundsException(string message)
			: base(message)
		{
		}
		public TaskDateOutOfBoundsException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown if you try and perform an operation that would result in there being no remaining top-level task folder
	/// </summary>
	public class ProjectDefaultTaskFolderException : ApplicationException
	{
		public ProjectDefaultTaskFolderException()
		{
		}
		public ProjectDefaultTaskFolderException(string message)
			: base(message)
		{
		}
		public ProjectDefaultTaskFolderException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
