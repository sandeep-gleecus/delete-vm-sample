using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Objects;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// This class encapsulates all the data access and business logic for accessing and managing
    /// the task workflows that are used to determine the lifecycle of tasks in the system
    /// </summary>
    public class TaskWorkflowManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.TaskWorkflowManager::";

        #region Static Methods

        /// <summary>
        /// Converts task workflow field collection to the generic kind
        /// </summary>
        /// <param name="taskWorkflowCustomProps">The task workflow fields</param>
        /// <returns>The workflow fields</returns>
        public static List<WorkflowField> ConvertFields(List<TaskWorkflowField> taskWorkflowFields)
        {
            //Handle null case safely
            if (taskWorkflowFields == null)
            {
                return null;
            }

            List<WorkflowField> workflowFields = new List<WorkflowField>();
            foreach (TaskWorkflowField taskWorkflowField in taskWorkflowFields)
            {
                WorkflowField workflowField = new WorkflowField();
                workflowField.ArtifactFieldId = taskWorkflowField.ArtifactFieldId;
                workflowField.IncidentStatusId = taskWorkflowField.TaskStatusId;
                workflowField.WorkflowId = taskWorkflowField.TaskWorkflowId;
                workflowField.WorkflowFieldStateId = taskWorkflowField.WorkflowFieldStateId;
                workflowField.Field = new ArtifactField();
                workflowField.Field.Name = taskWorkflowField.ArtifactField.Name;
                workflowFields.Add(workflowField);
            }

            return workflowFields;
        }

        /// <summary>
        /// Converts task workflow custom properties collection to the generic kind
        /// </summary>
        /// <param name="taskWorkflowCustomProps">The task workflow custom properties</param>
        /// <returns>The workflow custom properties</returns>
        public static List<WorkflowCustomProperty> ConvertFields(List<TaskWorkflowCustomProperty> taskWorkflowCustomProps)
        {
            //Handle null case safely
            if (taskWorkflowCustomProps == null)
            {
                return null;
            }

            List<WorkflowCustomProperty> workflowCustomProperties = new List<WorkflowCustomProperty>();
            foreach (TaskWorkflowCustomProperty taskWorkflowCustomProp in taskWorkflowCustomProps)
            {
                WorkflowCustomProperty workflowCustomProperty = new WorkflowCustomProperty();
                workflowCustomProperty.CustomPropertyId = taskWorkflowCustomProp.CustomPropertyId;
                workflowCustomProperty.IncidentStatusId = taskWorkflowCustomProp.TaskStatusId;
                workflowCustomProperty.WorkflowId = taskWorkflowCustomProp.TaskWorkflowId;
                workflowCustomProperty.WorkflowFieldStateId = taskWorkflowCustomProp.WorkflowFieldStateId;
                workflowCustomProperty.CustomProperty = new CustomProperty();
                workflowCustomProperty.CustomProperty.PropertyNumber = taskWorkflowCustomProp.CustomProperty.PropertyNumber;
                workflowCustomProperties.Add(workflowCustomProperty);
            }

            return workflowCustomProperties;
        }

		#endregion

		#region Public Methods


		public List<TaskStatusResponse> WorkflowTransition_RetrieveByInputStatusById(int workflowId, int inputTaskStatusId, int projectTemplateId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.TaskWorkflowTransitions.Include("InputTaskStatus").Include("OutputTaskStatus")
								join i in context.TaskStati
								on t.InputTaskStatusId equals i.TaskStatusId
								join o in context.TaskStati
								on t.OutputTaskStatusId equals o.TaskStatusId
								where t.OutputTaskStatus.IsActive && t.TaskWorkflowId == workflowId && t.InputTaskStatusId == inputTaskStatusId
								orderby t.Name, t.WorkflowTransitionId
								select new TaskStatusResponse
								{
									InputTaskStatusId = t.InputTaskStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									TaskWorkflowId = t.TaskWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
									URL = "",
									OutURL = "",
									InURL = "",
									OutputTaskStatusId = t.OutputTaskStatusId,
								};

					var workflows = query.ToList();

					foreach (var c in workflows)
					{
						c.InURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/TaskWorkflowStep.aspx?workflowId=" + workflowId + "&taskStatusId=" + c.InputTaskStatusId;
						c.URL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/TaskWorkflowTransition.aspx?workflowId=" + workflowId + "&workflowTransitionId=" + c.WorkflowTransitionId;
						c.OutURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/TaskWorkflowStep.aspx?workflowId=" + workflowId + "&taskStatusId=" + c.OutputTaskStatusId;
					}

					query = workflows.AsQueryable();

					List<TaskStatusResponse> workflowTransitions = query.ToList();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return workflowTransitions;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public List<TaskStatusResponse> WorkflowTransition_RetrieveAllStatuses(int workflowId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.TaskWorkflowTransitions.Include("InputTaskStatus").Include("OutputTaskStatus")
								join i in context.TaskStati
								on t.InputTaskStatusId equals i.TaskStatusId
								join o in context.TaskStati
								on t.OutputTaskStatusId equals o.TaskStatusId
								where t.OutputTaskStatus.IsActive && t.TaskWorkflowId == workflowId
								orderby i.Position, t.WorkflowTransitionId
								select new TaskStatusResponse
								{
									InputTaskStatusId = t.InputTaskStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									TaskWorkflowId = t.TaskWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
								};

					List<TaskStatusResponse> workflowTransitions = query.ToList();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return workflowTransitions;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}


		/// <summary>
		/// Inserts a new workflow into the system with all the standard transitions and field states
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="name">The name of the workflow</param>
		/// <param name="isDefault">Is this the default workflow for the project</param>
		/// <param name="isActive">Is this workflow active</param>
		/// <returns>The newly inserted workflow</returns>
		public TaskWorkflow Workflow_InsertWithDefaultEntries(int projectTemplateId, string name, bool isDefault, bool isActive = true)
        {
            const string METHOD_NAME = "Workflow_InsertWithDefaultEntries";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First create the workflow itself
                TaskWorkflow workflow = this.Workflow_Insert(projectTemplateId, name, isDefault, isActive);
                int workflowId = workflow.TaskWorkflowId;

                //Workflow Transitions (inc. roles)
                int workflowTransitionId1 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.NotStarted, (int)Task.TaskStatusEnum.InProgress, "Start Task", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId2 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.NotStarted, (int)Task.TaskStatusEnum.Deferred, "Defer Task", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId3 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.InProgress, (int)Task.TaskStatusEnum.Completed, "Complete Task", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId4 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.InProgress, (int)Task.TaskStatusEnum.Deferred, "Defer Task", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId5 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.InProgress, (int)Task.TaskStatusEnum.Blocked, "Block Task", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId6 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.Completed, (int)Task.TaskStatusEnum.InProgress, "Reopen Task", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId8 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.Deferred, (int)Task.TaskStatusEnum.InProgress, "Resume Task", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId9 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.Blocked, (int)Task.TaskStatusEnum.InProgress, "Unblock Task", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId10 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.InProgress, (int)Task.TaskStatusEnum.NotStarted, "Restart Development", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId11 = WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.Deferred, (int)Task.TaskStatusEnum.NotStarted, "Undefer Task", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                //Workflow Fields

                //All fields are active, visible and not-required by default, so only need to populate the ones
                //that are exceptions to that case

                //NotStarted - Hidden: ActualEffort, RemainingEffort
                TaskStatus taskStatus = Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
                taskStatus.StartTracking();
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 66, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 130, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
                Status_UpdateFieldsAndCustomProperties(taskStatus);

                //InProgress - Required: ReleaseId, StartDate,EndDate,EstimatedEffort, OwnerId
                taskStatus = Status_RetrieveById((int)Task.TaskStatusEnum.InProgress, workflowId);
                taskStatus.StartTracking();
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 67, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 62, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 63, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 65, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 58, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                Status_UpdateFieldsAndCustomProperties(taskStatus);

				//Completed - Required:StartDate,EndDate,EstimatedEffort,RemainingEffort,ActualEffort, Disabled:ReleaseId, OwnerId
				taskStatus = Status_RetrieveById((int)Task.TaskStatusEnum.Completed, workflowId);
                taskStatus.StartTracking();
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 62, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 63, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 65, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 130, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 66, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 67, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                taskStatus.WorkflowFields.Add(new TaskWorkflowField() { TaskWorkflowId = workflowId, ArtifactFieldId = 58, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(taskStatus);

                //Deferred/Blocked - All fields have default state (visible, optional, enabled)

                //Return the new workflow
                Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
                return workflow;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates the workflow fields and custom properties associated with a particular task status
        /// </summary>
        /// <param name="status">The status whose fields and custom properties we want to update</param>
        public void Status_UpdateFieldsAndCustomProperties(TaskStatus status)
        {
            const string METHOD_NAME = "Status_UpdateFieldsAndCustomProperties";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Attach the changed entity to the EF context and persist changes
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    context.TaskStati.ApplyChanges(status);
                    context.SaveChanges();
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the task status by the given ID.</summary>
        /// <param name="taskStatusId">The status ID to retrieve.</param>
        /// <returns>The TaskStatus, or null if not found.</returns>
        /// <remarks>Will return deleted items.</remarks>
        /// <param name="includeWorkflowFieldsForWorkflowId">Should we include the linked workflow fields</param>
        public TaskStatus Status_RetrieveById(int taskStatusId, int? includeWorkflowFieldsForWorkflowId = null)
        {
            const string METHOD_NAME = "Status_RetrieveById()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TaskStatus retStatus = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from r in context.TaskStati
                                where r.TaskStatusId == taskStatusId
                                select r;

                    retStatus = query.FirstOrDefault();

                    //Get the fields and custom properties, joined by EF 'fix-up'
                    if (includeWorkflowFieldsForWorkflowId.HasValue)
                    {
                        int workflowId = includeWorkflowFieldsForWorkflowId.Value;
                        var query2 = from w in context.TaskWorkflowFields
                                     where w.TaskStatusId == taskStatusId && w.TaskWorkflowId == workflowId && w.ArtifactField.IsActive
                                     select w;
                        query2.ToList();
                        var query3 = from w in context.TaskWorkflowCustomProperties
                                     where w.TaskStatusId == taskStatusId && w.TaskWorkflowId == workflowId && !w.CustomProperty.IsDeleted
                                     select w;
                        query3.ToList();
                    }

                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return retStatus;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
		/// Gets the default workflow for a specific project template
		/// </summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <returns>The default workflow</returns>
        public TaskWorkflow Workflow_GetDefault(int projectTemplateId)
		{
            const string METHOD_NAME = "Workflow_GetDefault";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from w in context.TaskWorkflows
                                where w.IsActive && w.IsDefault && w.ProjectTemplateId == projectTemplateId
                                select w;

                    TaskWorkflow workflow = query.FirstOrDefault();
                    if (workflow == null)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve default workflow for project template PT" + projectTemplateId + ".");
                    }
                    else
                    {
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return workflow;
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
		/// Gets the workflow for a specific task type
		/// </summary>
		/// <param name="taskTypeId">The task type we're interested in</param>
		/// <returns>The workflow id</returns>
        public int Workflow_GetForTaskType(int taskTypeId)
		{
            const string METHOD_NAME = "Workflow_GetForTaskType";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from r in context.TaskTypes.Include(t => t.Workflow)
                                where r.TaskTypeId == taskTypeId
                                select r;

                    TaskType taskType = query.FirstOrDefault();
                    if (taskType == null || taskType.Workflow == null)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve workflow for task type " + taskTypeId + ".");
                    }
                    else
                    {
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return taskType.TaskWorkflowId;
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
		/// Retrieves the list of inactive, hidden and required fields for an task based on its position
		/// in the workflow.
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="taskStatusId">The current step (i.e. task status) in the workflow</param>
        /// <remarks>Workflow needs to be active</remarks>
		public List<TaskWorkflowField> Workflow_RetrieveFieldStates(int workflowId, int taskStatusId)
		{
            const string METHOD_NAME = "Workflow_RetrieveFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.TaskWorkflowFields.Include("FieldState").Include("ArtifactField")
                                where w.TaskWorkflowId == workflowId && w.Workflow.IsActive && w.ArtifactField.IsActive && w.TaskStatusId == taskStatusId
                                orderby w.WorkflowFieldStateId, w.ArtifactFieldId
                                select w;

                    List<TaskWorkflowField> workflowFields = query.ToList();

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return workflowFields;
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
        /// Retrieves the list of inactive, hidden and required custom properties for an task based on its position
        /// in the workflow.
        /// </summary>
        /// <param name="workflowId">The workflow we're using</param>
        /// <param name="taskStatusId">The current step (i.e. status) in the workflow</param>
        /// <returns>List of inactive, hidden and required custom properties for the current workflow step</returns>
        /// <remarks>Workflow needs to be active</remarks>
        public List<TaskWorkflowCustomProperty> Workflow_RetrieveCustomPropertyStates(int workflowId, int taskStatusId)
        {
            const string METHOD_NAME = "Workflow_RetrieveCustomPropertyStates";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.TaskWorkflowCustomProperties.Include("FieldState").Include("CustomProperty")
                                where w.TaskWorkflowId == workflowId &&
                                    w.Workflow.IsActive &&
                                    w.TaskStatusId == taskStatusId &&
                                    !w.CustomProperty.IsDeleted
                                orderby w.WorkflowFieldStateId, w.CustomPropertyId
                                select w;

                    List<TaskWorkflowCustomProperty> workflowCustomProps = query.ToList();

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return workflowCustomProps;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a workflow transition by its id, includes the transition roles and task status names
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="workflowTransitionId">The id of the transition</param>
        /// <returns></returns>
        public TaskWorkflowTransition WorkflowTransition_RetrieveById(int workflowId, int workflowTransitionId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TaskWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TaskWorkflowTransitions.Include("InputTaskStatus").Include("OutputTaskStatus").Include("TransitionRoles").Include("TransitionRoles.Role")
                                where t.TaskWorkflowId == workflowId && t.WorkflowTransitionId == workflowTransitionId
                                select t;

                    workflowTransition = query.FirstOrDefault();

                    if (workflowTransition == null)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve workflow transition with ID= " + workflowTransitionId);
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return workflowTransition;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets a specific workflow by its id
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="includeTransitions">Should we include the list of transitions and transition roles</param>
        /// <param name="includeFieldStates">Should we include the list of field and custom property states</param>
        /// <returns>The workflow object</returns>
        public TaskWorkflow Workflow_RetrieveById(int workflowId, bool includeTransitions = false, bool includeFieldStates = false)
        {
            const string METHOD_NAME = "Workflow_RetrieveById(int,[bool],[bool])";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See if we need to include transitions
                    ObjectQuery<TaskWorkflow> workflowSet = context.TaskWorkflows;
                    if (includeTransitions)
                    {
                        workflowSet = workflowSet.Include("Transitions").Include("Transitions.TransitionRoles");
                    }
                    if (includeFieldStates)
                    {
                        workflowSet = workflowSet.Include("Fields").Include("CustomProperties");
                    }

                    //Get the workflow by its id (active and inactive)
                    var query = from w in workflowSet
                                where w.TaskWorkflowId == workflowId
                                select w;

                    TaskWorkflow workflow = query.FirstOrDefault();
                    if (workflow == null)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve workflow with ID= " + workflowId);
                    }
                    else
                    {
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return workflow;
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
		/// Determines if a workflow is in use (i.e. associated with one or more task type)
		/// </summary>
		/// <param name="workflowId">The ID of the workflow being tested</param>
		/// <returns>True if in use, otherwise False</returns>
		protected bool IsInUse(int workflowId)
		{
			const string METHOD_NAME = "IsInUse";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Default to false
            bool isInUse = false;

            try
            {
                //Create the select command to retrieve the workflow used by the task type
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the task type
                    var query = from it in context.TaskTypes
                                where it.TaskWorkflowId == workflowId
                                select it;

                    isInUse = (query.Count() > 0);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return (isInUse);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves all the workflows for a project template
        /// </summary>
        /// <param name="projectTemplateId">The project template we're interested in</param>
        /// <param name="activeOnly">Whether to only retrieve active types</param>
        /// <returns>List of workflow entities</returns>
        public List<TaskWorkflow> Workflow_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
            const string METHOD_NAME = "Workflow_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TaskWorkflow> workflows;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflows
                    var query = from w in context.TaskWorkflows
                                where (!activeOnly || w.IsActive) && w.ProjectTemplateId == projectTemplateId
                                select w;

                    //For the active case (i.e. a lookup) order by name, otherwise in the admin case, order by id
                    if (activeOnly)
                    {
                        query = query.OrderBy(w => w.Name).ThenBy(w => w.TaskWorkflowId);
                    }
                    else
                    {
                        query = query.OrderBy(w => w.TaskWorkflowId);
                    }
                    workflows = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return workflows;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
		}

        /// <summary>
        /// Gets the list of workflow transitions for the specified task status where the user has a specific role or is the author/owner
        /// </summary>
        /// <param name="inputTaskStatusId">The input status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>List of matching workflow transitions</returns>
        /// <param name="isAuthor">Are we the author of the task</param>
        /// <param name="isOwner">Are we the owner of the task</param>
        /// <param name="projectRoleId">What project role are we</param>
        /// <remarks>This overload considers user permissions/roles when returning the list</remarks>
        public List<TaskWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputTaskStatusId, int projectRoleId, bool isAuthor, bool isOwner)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TaskWorkflowTransitions.Include("InputTaskStatus").Include("OutputTaskStatus")
                                where t.OutputTaskStatus.IsActive && t.TaskWorkflowId == workflowId && t.InputTaskStatusId == inputTaskStatusId
                                select t;

                    //Create the expresion if we're the author, owner or in one of the roles allowed to execute the transition
                    List<Expression<Func<TaskWorkflowTransition, bool>>> expressionList = new List<Expression<Func<TaskWorkflowTransition, bool>>>();
                    if (isAuthor)
                    {
                        Expression<Func<TaskWorkflowTransition, bool>> clause = t => t.IsExecuteByCreator;
                        expressionList.Add(clause);
                    }
                    if (isOwner)
                    {
                        Expression<Func<TaskWorkflowTransition, bool>> clause = t => t.IsExecuteByOwner;
                        expressionList.Add(clause);
                    }

                    //Now the project role
                    Expression<Func<TaskWorkflowTransition, bool>> clause2 = t => t.TransitionRoles.Any(i => i.Role.ProjectRoleId == projectRoleId && i.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);
                    expressionList.Add(clause2);

                    if (expressionList.Count > 0)
                    {
                        //OR together the different cases and apply to the where clause
                        Expression<Func<TaskWorkflowTransition, bool>> aggregatedClause = Manager.BuildOr(expressionList.ToArray());
                        query = query.Where(aggregatedClause);
                    }

                    //Add the sorts
                    query = query.OrderBy(t => t.Name).ThenBy(t => t.WorkflowTransitionId);

                    List<TaskWorkflowTransition> workflowTransitions = query.ToList();

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return workflowTransitions;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the workflow transition associated with the change between two statuses
        /// </summary>
        /// <param name="inputTaskStatusId">The input status</param>
        /// <param name="outputTaskStatusId">The output status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>The workflow transition</returns>
        public TaskWorkflowTransition WorkflowTransition_RetrieveByStatuses(int workflowId, int inputTaskStatusId, int outputTaskStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TaskWorkflowTransitions.Include("InputTaskStatus").Include("OutputTaskStatus")
                                where t.TaskWorkflowId == workflowId &&
                                      t.InputTaskStatusId == inputTaskStatusId &&
                                      t.OutputTaskStatusId == outputTaskStatusId
                                select t;

                    TaskWorkflowTransition workflowTransition = query.FirstOrDefault();

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return workflowTransition;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of workflow transitions for the specified task status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="inputTaskStatusId">The id of the input status</param>
        /// <returns>List of transitions</returns>
        public List<TaskWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputTaskStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TaskWorkflowTransitions.Include("InputTaskStatus").Include("OutputTaskStatus")
                                where t.OutputTaskStatus.IsActive && t.TaskWorkflowId == workflowId && t.InputTaskStatusId == inputTaskStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<TaskWorkflowTransition> workflowTransitions = query.ToList();

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return workflowTransitions;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }


        /// <summary>
        /// Gets the list of workflow transitions for the specified task output status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="outputTaskStatusId">The id of the output status</param>
        /// <returns>List of transitions</returns>
        public List<TaskWorkflowTransition> WorkflowTransition_RetrieveByOutputStatus(int workflowId, int outputTaskStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByOutputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TaskWorkflowTransitions.Include("InputTaskStatus").Include("OutputTaskStatus")
                                where t.OutputTaskStatus.IsActive && t.TaskWorkflowId == workflowId && t.OutputTaskStatusId == outputTaskStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<TaskWorkflowTransition> workflowTransitions = query.ToList();

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return workflowTransitions;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Commits any changes made to a workflow entity
        /// </summary>
        /// <param name="workflow">The workflow entity</param>
        public void Workflow_Update(TaskWorkflow workflow)
        {
            const string METHOD_NAME = CLASS_NAME + "Workflow_Update";

            Logger.LogEnteringEvent(METHOD_NAME);

            //Verify certain workflow updates

            //If any of the workflows is marked as default and inactive, throw an exception
            if (workflow.IsDefault && !workflow.IsActive)
            {
                throw new WorkflowMustBeActiveException("You cannot make an inactive workflow default");
            }

            //If the workflow is in use, throw an exception if there is an attempt to make inactive
            if (!workflow.IsActive && IsInUse(workflow.TaskWorkflowId))
            {
                throw new WorkflowInUseException("You cannot make a workflow that is in use inactive");
            }

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.TaskWorkflows.ApplyChanges(workflow);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEvent(METHOD_NAME, ex, "Saving workflow:");
                    throw;
                }
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>
        /// Inserts a new workflow into the system
        /// </summary>
        /// <param name="projectTemplateId">The project template the workflow is associated with</param>
        /// <param name="name">The name of the workflow</param>
        /// <param name="isDefault">Is this the default workflow for the project</param>
        /// <param name="isActive">Is this workflow active</param>
        /// <returns>The newly inserted workflow</returns>
        public TaskWorkflow Workflow_Insert(int projectTemplateId, string name, bool isDefault, bool isActive = true)
        {
            const string METHOD_NAME = "Workflow_Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //If the workflow is marked as default and inactive, throw an exception
            if (isDefault && !isActive)
            {
                throw new WorkflowMustBeActiveException("You cannot make an inactive workflow default");
            }

            try
            {
                TaskWorkflow workflow;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow entity
                    workflow = new TaskWorkflow();
                    workflow.ProjectTemplateId = projectTemplateId;
                    workflow.Name = name;
                    workflow.IsActive = isActive;
                    workflow.IsDefault = isDefault;

                    //Persist the new workflow
                    context.TaskWorkflows.AddObject(workflow);
                    context.SaveChanges();
                }
                return workflow;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Inserts a new workflow transition into the system
        /// </summary>
        /// <param name="workflowId">The workflow the transition is to be associated with</param>
        /// <param name="inputTaskStatusId">The input task status for the transition</param>
        /// <param name="outputTaskStatusId">The output task status for the transition</param>
        /// <param name="name">The name of the transition</param>
        /// <param name="executeByCreator"></param>
        /// <param name="executeByOwner"></param>
        /// <param name="roles">The list of any roles that can also execute the transition</param>
        /// <returns>The new workflow transition</returns>
        public TaskWorkflowTransition WorkflowTransition_Insert(int workflowId, int inputTaskStatusId, int outputTaskStatusId, string name, bool executeByCreator = false, bool executeByOwner = true, List<int> roles = null)
        {
            const string METHOD_NAME = "Workflow_Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TaskWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow transition entity
                    workflowTransition = new TaskWorkflowTransition();
                    workflowTransition.Name = name;
                    workflowTransition.TaskWorkflowId = workflowId;
                    workflowTransition.InputTaskStatusId = inputTaskStatusId;
                    workflowTransition.OutputTaskStatusId = outputTaskStatusId;
                    workflowTransition.IsExecuteByOwner = executeByOwner;
                    workflowTransition.IsExecuteByCreator = executeByCreator;

                    //Add the transition roles if any provided
                    if (roles != null && roles.Count > 0)
                    {
                        foreach (int projectRoleId in roles)
                        {
                            //For task workflows, all transition roles are execute transitions (not notify)
                            TaskWorkflowTransitionRole transitionRole = new TaskWorkflowTransitionRole();
                            transitionRole.ProjectRoleId = projectRoleId;
                            transitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
                            workflowTransition.TransitionRoles.Add(transitionRole);
                        }
                    }

                    //Persist the new workflow transition
                    context.TaskWorkflowTransitions.AddObject(workflowTransition);
                    context.SaveChanges();
                }
                return workflowTransition;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Deletes the specified transition
        /// </summary>
        /// <param name="workflowTransitionId"></param>
        public void WorkflowTransition_Delete(int workflowTransitionId)
        {
            const string METHOD_NAME = "WorkflowTransition_Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow and associated transitions, fields and custom properties
                    var query = from wt in context.TaskWorkflowTransitions.Include("TransitionRoles")
                                where wt.WorkflowTransitionId == workflowTransitionId
                                select wt;

                    TaskWorkflowTransition workflowTransition = query.FirstOrDefault();

                    //Make sure we have a workflow transition
                    if (workflowTransition != null)
                    {
                        //Delete the workflow transition and dependent entities
                        context.TaskWorkflowTransitions.DeleteObject(workflowTransition);
                        context.SaveChanges();
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
        /// Updates the changes in the workflow transition entity (including roles)
        /// </summary>
        /// <param name="workflowTransition">The workflow transition being updated</param>
        public void WorkflowTransition_Update(TaskWorkflowTransition workflowTransition)
        {
            const string METHOD_NAME = CLASS_NAME + "WorkflowTransition_Update";

            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.TaskWorkflowTransitions.ApplyChanges(workflowTransition);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEvent(METHOD_NAME, ex, "Saving workflow transition:");
                    throw;
                }
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>
        /// Copies all the workflows from one project template to another (used when making a new project based on an existing one)
        /// </summary>
        /// <param name="taskWorkflowMapping">The mapping of workflow ids between the old and new templates</param>
        /// <param name="sourceProjectTemplateId">The project template to copy the workflows from</param>
        /// <param name="destProjectTemplateId">The destination project template</param>
        /// <param name="customPropertyIdMapping">The mapping of old vs. new custom properties between the two project templates</param>
        public void Workflow_Copy(int sourceProjectTemplateId, int destProjectTemplateId, Dictionary<int, int> customPropertyIdMapping, Dictionary<int, int> taskWorkflowMapping)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Dictionary<int, TaskWorkflow> tempMapping = new Dictionary<int, TaskWorkflow>();
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflows and associated transitions, fields task types, and custom properties
                    var query = from w in context.TaskWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties").Include("TaskTypes")
                                where w.ProjectTemplateId == sourceProjectTemplateId
                                orderby w.TaskWorkflowId
                                select w;

                    List<TaskWorkflow> sourceWorkflows = query.ToList();

                    //Loop through the various workflows
                    foreach (TaskWorkflow sourceWorkflow in sourceWorkflows)
                    {
                        //Create the copied workflow
                        TaskWorkflow destWorkflow = new TaskWorkflow();
                        destWorkflow.ProjectTemplateId = destProjectTemplateId;
                        destWorkflow.Name = sourceWorkflow.Name;
                        destWorkflow.IsActive = sourceWorkflow.IsActive;
                        destWorkflow.IsDefault = sourceWorkflow.IsDefault;

                        //Add to mapping
                        tempMapping.Add(sourceWorkflow.TaskWorkflowId, destWorkflow);

                        //Now add the transitions (including the transition roles)
                        for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                        {
                            TaskWorkflowTransition workflowTransition = new TaskWorkflowTransition();
                            workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                            workflowTransition.InputTaskStatusId = sourceWorkflow.Transitions[i].InputTaskStatusId;
                            workflowTransition.OutputTaskStatusId = sourceWorkflow.Transitions[i].OutputTaskStatusId;
                            workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                            workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                            workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                            destWorkflow.Transitions.Add(workflowTransition);

                            //Now the roles
                            for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                            {
                                TaskWorkflowTransitionRole workflowTransitionRole = new TaskWorkflowTransitionRole();
                                workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                                workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                                workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                            }
                        }

                        //Now add the fields
                        for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                        {
                            TaskWorkflowField workflowField = new TaskWorkflowField();
                            workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                            workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                            workflowField.TaskStatusId = sourceWorkflow.Fields[i].TaskStatusId;
                            destWorkflow.Fields.Add(workflowField);
                        }

                        //Now add the custom properties
                        for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                        {
                            //Get the corresponding custom property in the new project
                            if (customPropertyIdMapping.ContainsKey(sourceWorkflow.CustomProperties[i].CustomPropertyId))
                            {
                                int destCustomPropertyId = customPropertyIdMapping[sourceWorkflow.CustomProperties[i].CustomPropertyId];
                                TaskWorkflowCustomProperty workflowCustomProperty = new TaskWorkflowCustomProperty();
                                workflowCustomProperty.CustomPropertyId = destCustomPropertyId;
                                workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                                workflowCustomProperty.TaskStatusId = sourceWorkflow.CustomProperties[i].TaskStatusId;
                                destWorkflow.CustomProperties.Add(workflowCustomProperty);
                            }
                        }

                        //Add the new workflow
                        context.TaskWorkflows.AddObject(destWorkflow);
                    }

                    //Save the changes
                    context.SaveChanges();
                }

                //Finally populate the external mapping dictionary (id based)
                foreach (KeyValuePair<int, TaskWorkflow> kvp in tempMapping)
                {
                    taskWorkflowMapping.Add(kvp.Key, kvp.Value.TaskWorkflowId);
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
        /// Makes a copy of the specified workflow
        /// </summary>
        /// <param name="sourceWorkflowId">The workflow to copy</param>
        /// <returns>The copy of the workflow</returns>
        public TaskWorkflow Workflow_Copy(int sourceWorkflowId)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TaskWorkflow destWorkflow = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow and associated transitions, fields and custom properties
                    var query = from w in context.TaskWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties")
                                where w.TaskWorkflowId == sourceWorkflowId
                                select w;

                    TaskWorkflow sourceWorkflow = query.FirstOrDefault();
                    if (sourceWorkflow == null)
                    {
                        throw new ArtifactNotExistsException("The workflow being copied no longer exists");
                    }

                    //Create the copied workflow
                    //except that we will always insert it with default=No
                    destWorkflow = new TaskWorkflow();
                    destWorkflow.ProjectTemplateId = sourceWorkflow.ProjectTemplateId;
                    destWorkflow.Name = GlobalResources.General.Global_CopyOf + " " + sourceWorkflow.Name;
                    destWorkflow.IsActive = sourceWorkflow.IsActive;
                    destWorkflow.IsDefault = false;

                    //Now add the transitions (including the transition roles)
                    for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                    {
                        TaskWorkflowTransition workflowTransition = new TaskWorkflowTransition();
                        workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                        workflowTransition.InputTaskStatusId = sourceWorkflow.Transitions[i].InputTaskStatusId;
                        workflowTransition.OutputTaskStatusId = sourceWorkflow.Transitions[i].OutputTaskStatusId;
                        workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                        workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                        workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                        destWorkflow.Transitions.Add(workflowTransition);

                        //Now the roles
                        for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                        {
                            TaskWorkflowTransitionRole workflowTransitionRole = new TaskWorkflowTransitionRole();
                            workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                            workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                            workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                        }
                    }

                    //Now add the fields
                    for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                    {
                        TaskWorkflowField workflowField = new TaskWorkflowField();
                        workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                        workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                        workflowField.TaskStatusId = sourceWorkflow.Fields[i].TaskStatusId;
                        destWorkflow.Fields.Add(workflowField);
                    }

                    //Now add the custom properties
                    for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                    {
                        TaskWorkflowCustomProperty workflowCustomProperty = new TaskWorkflowCustomProperty();
                        workflowCustomProperty.CustomPropertyId = sourceWorkflow.CustomProperties[i].CustomPropertyId;
                        workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                        workflowCustomProperty.TaskStatusId = sourceWorkflow.CustomProperties[i].TaskStatusId;
                        destWorkflow.CustomProperties.Add(workflowCustomProperty);
                    }

                    //Save the new workflow
                    context.TaskWorkflows.AddObject(destWorkflow);
                    context.SaveChanges();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return destWorkflow;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Deletes all the workflows in the project template (called by the project delete)
        /// </summary>
        /// <param name="projectTemplateId">The project to delete workflow for</param>
        /// <remarks>The associated task types need to have been deleted first</remarks>
        protected internal void Workflow_DeleteAllForProjectTemplate(int projectTemplateId)
        {
            const string METHOD_NAME = "Workflow_DeleteAllForProjectTemplate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow
                    var query = from w in context.TaskWorkflows
                                where w.ProjectTemplateId == projectTemplateId
                                orderby w.TaskWorkflowId
                                select w;

                    List<TaskWorkflow> workflows = query.ToList();
                    for (int i = 0; i < workflows.Count; i++)
                    {
                        context.TaskWorkflows.DeleteObject(workflows[i]);
                    }

                    //Commit the deletes
                    context.SaveChanges();
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
        /// Deletes the specified workflow
        /// </summary>
        /// <param name="workflowId">The workflow to delete</param>
        public void Workflow_Delete(int workflowId)
        {
            const string METHOD_NAME = "Workflow_Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //If the workflow is in use, throw an exception if there is an attempt to delete
                if (IsInUse(workflowId))
                {
                    throw new WorkflowInUseException("You cannot delete a workflow that is in use");
                }

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow
                    var query = from w in context.TaskWorkflows
                                where w.TaskWorkflowId == workflowId
                                select w;

                    TaskWorkflow workflow = query.FirstOrDefault();

                    //Make sure we have a workflow
                    if (workflow != null)
                    {
                        //If the workflow is marked as default, you can't delete it
                        if (workflow.IsDefault)
                        {
                            throw new WorkflowInUseException("You cannot delete a workflow that is marked as the default");
                        }

                        //Delete the workflow. The database cascades will handle the dependent entities
                        context.TaskWorkflows.DeleteObject(workflow);
                        context.SaveChanges();
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

		#endregion
    }
}
