using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access and business logic for accessing and managing
	/// the incident workflows that are used to determine the lifecycle of incidents in the system
	/// </summary>
	public class WorkflowManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.WorkflowManager::";

		#region Public Methods

		/// <summary>
		///	Constructor method for class.
		/// </summary>
		public WorkflowManager()
			: base()
		{
			const string METHOD_NAME = "Workflow";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

        /// <summary>
        /// Gets the default workflow for a specific project template
        /// </summary>
        /// <param name="projectTemplateId">The project template we're interested in</param>
        /// <returns>The default workflow</returns>
        public Workflow Workflow_GetDefault(int projectTemplateId)
		{
			const string METHOD_NAME = "Workflow_GetDefault";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow by the product
					var query = from w in context.Workflows
								where w.IsActive && w.IsDefault && w.ProjectTemplateId == projectTemplateId
                                select w;

					Workflow workflow = query.FirstOrDefault();
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
		/// Gets the workflow for a specific incident type
		/// </summary>
		/// <param name="incidentTypeId">The incident type we're interested in</param>
		/// <returns>The workflow id</returns>
		public int Workflow_GetForIncidentType(int incidentTypeId)
		{
			const string METHOD_NAME = "Workflow_GetForIncidentType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Retrieve the incident type record
			IncidentManager incidentManager = new IncidentManager();
			IncidentType incidentType = incidentManager.RetrieveIncidentTypeById(incidentTypeId);
			int workflowId = incidentType.WorkflowId;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return (workflowId);
		}

		/// <summary>
		/// Retrieves the list of inactive, hidden and required fields for an incident based on its position
		/// in the workflow.
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="incidentStatusId">The current step (i.e. incident status) in the workflow</param>
		/// <remarks>Workflow needs to be active</remarks>
		public List<WorkflowField> Workflow_RetrieveFieldStates(int workflowId, int incidentStatusId)
		{
			const string METHOD_NAME = "Workflow_RetrieveFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow fields for the specified workflow and status
					var query = from w in context.WorkflowFields.Include("State").Include("Field")
								where w.WorkflowId == workflowId && w.Workflow.IsActive && w.Field.IsActive && w.IncidentStatusId == incidentStatusId
								orderby w.WorkflowFieldStateId, w.ArtifactFieldId
								select w;

					List<WorkflowField> workflowFields = query.ToList();

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
		/// Retrieves the list of inactive, hidden and required custom properties for an incident based on its position
		/// in the workflow.
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="incidentStatusId">The current step (i.e. status) in the workflow</param>
		/// <returns>List of inactive, hidden and required custom properties for the current workflow step</returns>
		/// <remarks>Workflow needs to be active</remarks>
		public List<WorkflowCustomProperty> Workflow_RetrieveCustomPropertyStates(int workflowId, int incidentStatusId)
		{
			const string METHOD_NAME = "Workflow_RetrieveCustomPropertyStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow fields for the specified workflow and status
					var query = from w in context.WorkflowCustomProperties.Include("State").Include("CustomProperty")
								where w.WorkflowId == workflowId &&
									w.Workflow.IsActive &&
									w.IncidentStatusId == incidentStatusId &&
									!w.CustomProperty.IsDeleted
								orderby w.WorkflowFieldStateId, w.CustomPropertyId
								select w;

					List<WorkflowCustomProperty> workflowCustomProps = query.ToList();

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
		/// Retrieves a workflow transition by its id, includes the transition roles and ticket status names
		/// </summary>
		/// <param name="workflowId">The id of the workflow</param>
		/// <param name="workflowTransitionId">The id of the transition</param>
		/// <returns></returns>
		public WorkflowTransition WorkflowTransition_RetrieveById(int workflowId, int workflowTransitionId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				WorkflowTransition workflowTransition;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.WorkflowTransitions.Include("InputStatus").Include("OutputStatus").Include("TransitionRoles").Include("TransitionRoles.Role")
								where t.WorkflowId == workflowId && t.WorkflowTransitionId == workflowTransitionId
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
		public Workflow Workflow_RetrieveById(int workflowId, bool includeTransitions = false, bool includeFieldStates = false)
		{
			const string METHOD_NAME = "Workflow_RetrieveById(int,[bool],[bool])";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we need to include transitions
					ObjectQuery<Workflow> workflowSet = context.Workflows;
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
								where w.WorkflowId == workflowId
								select w;

					Workflow workflow = query.FirstOrDefault();
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
		/// Determines if a workflow is in use (i.e. associated with one or more incident type)
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
				//Create the select command to retrieve the workflow used by product records
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow by the incident type
					var query = from it in context.IncidentTypes
								where it.WorkflowId == workflowId
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

		public List<IncidentStatusResponse> WorkflowTransition_RetrieveAllStatuses(int workflowId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveAllStatuses";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.WorkflowTransitions.Include("InputStatus").Include("OutputStatus")
								join i in context.IncidentStati
								on t.InputIncidentStatusId equals i.IncidentStatusId
								join o in context.IncidentStati
								on t.OutputIncidentStatusId equals o.IncidentStatusId
								where t.OutputStatus.IsActive && t.WorkflowId == workflowId
								orderby t.WorkflowId
								select new IncidentStatusResponse
								{
									InputIncidentStatusId = t.InputIncidentStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByDetector,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									IncidentWorkflowId = t.WorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsExecuteByDetector,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
								};

					List<IncidentStatusResponse> workflowTransitions = query.ToList();

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
		/// Retrieves all the workflows for a project template
		/// </summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <param name="activeOnly">Whether to only retrieve active types</param>
		/// <returns>List of workflow entities</returns>
		public List<Workflow> Workflow_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "Workflow_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Workflow> workflows;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflows
					var query = from w in context.Workflows
								where (!activeOnly || w.IsActive) && w.ProjectTemplateId == projectTemplateId
                                select w;

					//For the active case (i.e. a lookup) order by name, otherwise in the admin case, order by id
					if (activeOnly)
					{
						query = query.OrderBy(w => w.Name).ThenBy(w => w.WorkflowId);
					}
					else
					{
						query = query.OrderBy(w => w.WorkflowId);
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
		/// Gets the list of workflow transitions for the specified ticket status where the user has a specific role or is the detector/owner
		/// </summary>
		/// <param name="inputIncidentStatusId">The input status</param>
		/// <param name="workflowId">The workflow we're using</param>
		/// <returns>List of matching workflow transitions</returns>
		/// <param name="isDetector">Are we the detector of the incident</param>
		/// <param name="isOwner">Are we the owner of the incident</param>
		/// <param name="projectRoleId">What project role are we</param>
		/// <remarks>This overload considers user permissions/roles when returning the list</remarks>
		public List<WorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputIncidentStatusId, int projectRoleId, bool isDetector, bool isOwner)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.WorkflowTransitions.Include("InputStatus").Include("OutputStatus")
								where t.OutputStatus.IsActive && t.WorkflowId == workflowId && t.InputIncidentStatusId == inputIncidentStatusId
								select t;

					//Create the expresion if we're the detector, owner or in one of the roles allowed to execute the transition
					List<Expression<Func<WorkflowTransition, bool>>> expressionList = new List<Expression<Func<WorkflowTransition, bool>>>();
					if (isDetector)
					{
						Expression<Func<WorkflowTransition, bool>> clause = t => t.IsExecuteByDetector;
						expressionList.Add(clause);
					}
					if (isOwner)
					{
						Expression<Func<WorkflowTransition, bool>> clause = t => t.IsExecuteByOwner;
						expressionList.Add(clause);
					}

					//Now the project role
					Expression<Func<WorkflowTransition, bool>> clause2 = t => t.TransitionRoles.Any(i => i.Role.ProjectRoleId == projectRoleId && i.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);
					expressionList.Add(clause2);

					if (expressionList.Count > 0)
					{
						//OR together the different cases and apply to the where clause
						Expression<Func<WorkflowTransition, bool>> aggregatedClause = Manager.BuildOr(expressionList.ToArray());
						query = query.Where(aggregatedClause);
					}

					//Add the sorts
					query = query.OrderBy(t => t.Name).ThenBy(t => t.WorkflowTransitionId);

					List<WorkflowTransition> workflowTransitions = query.ToList();

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
		/// <param name="inputIncidentStatusId">The input status</param>
		/// <param name="outputIncidentStatusId">The output status</param>
		/// <param name="workflowId">The workflow we're using</param>
		/// <returns>The workflow transition</returns>
		public WorkflowTransition WorkflowTransition_RetrieveByStatuses(int workflowId, int inputIncidentStatusId, int outputIncidentStatusId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByStatuses";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.WorkflowTransitions.Include("InputStatus").Include("OutputStatus").Include("Workflow")
								where t.WorkflowId == workflowId &&
									  t.InputIncidentStatusId == inputIncidentStatusId &&
									  t.OutputIncidentStatusId == outputIncidentStatusId
								select t;

					WorkflowTransition workflowTransition = query.FirstOrDefault();

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
		/// Gets the list of workflow transitions for the specified ticket status
		/// </summary>
		/// <param name="workflowId">The id of the workflow</param>
		/// <param name="inputIncidentStatusId">The id of the input status</param>
		/// <returns>List of transitions</returns>
		public List<WorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputIncidentStatusId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.WorkflowTransitions.Include("InputStatus").Include("OutputStatus")
								where t.OutputStatus.IsActive && t.WorkflowId == workflowId && t.InputIncidentStatusId == inputIncidentStatusId
								orderby t.Name, t.WorkflowTransitionId
								select t;

					List<WorkflowTransition> workflowTransitions = query.ToList();

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
		/// Gets the list of workflow transitions for the specified incident output status
		/// </summary>
		/// <param name="workflowId">The id of the workflow</param>
		/// <param name="outputIncidentStatusId">The id of the output status</param>
		/// <returns>List of transitions</returns>
		public List<WorkflowTransition> WorkflowTransition_RetrieveByOutputStatus(int workflowId, int outputIncidentStatusId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByOutputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.WorkflowTransitions.Include("InputStatus").Include("OutputStatus")
								where t.OutputStatus.IsActive && t.WorkflowId == workflowId && t.OutputIncidentStatusId == outputIncidentStatusId
								orderby t.Name, t.WorkflowTransitionId
								select t;

					List<WorkflowTransition> workflowTransitions = query.ToList();

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
		public void Workflow_Update(Workflow workflow)
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
			if (!workflow.IsActive && IsInUse(workflow.WorkflowId))
			{
				throw new WorkflowInUseException("You cannot make a workflow that is in use inactive");
			}

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					context.Workflows.ApplyChanges(workflow);
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
        /// <param name="isNotifyEnabled">Should email notifications be turned on for this workflow</param>
        /// <param name="isActive">Is this workflow active</param>
        /// <returns>The newly inserted workflow</returns>
        public Workflow Workflow_Insert(int projectTemplateId, string name, bool isDefault, bool isNotifyEnabled, bool isActive = true)
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
				Workflow workflow;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create a new workflow entity
					workflow = new Workflow();
					workflow.ProjectTemplateId = projectTemplateId;
					workflow.Name = name;
					workflow.IsActive = isActive;
					workflow.IsDefault = isDefault;
					workflow.IsNotify = isNotifyEnabled;

					//Persist the new workflow
					context.Workflows.AddObject(workflow);
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
		/// <param name="inputIncidentStatusId">The input incident status for the transition</param>
		/// <param name="outputIncidentStatusId">The output incident status for the transition</param>
		/// <param name="name">The name of the transition</param>
		/// <param name="executeByDetector"></param>
		/// <param name="executeByOwner"></param>
		/// <param name="notifyDetector"></param>
		/// <param name="notifyOwner"></param>
        /// <param name="notifySubject">The email subject line to use (leave blank to use the default)</param>
        /// <param name="signatureRequired">Is an electronic signature required</param>
		/// <returns>The new workflow transition</returns>
		public WorkflowTransition WorkflowTransition_Insert(int workflowId, int inputIncidentStatusId, int outputIncidentStatusId, string name, bool executeByDetector = false, bool executeByOwner = true, bool notifyDetector = false, bool notifyOwner = false, string notifySubject = null, bool signatureRequired = false)
		{
			const string METHOD_NAME = "Workflow_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				WorkflowTransition workflowTransition;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create a new workflow transition entity
					workflowTransition = new WorkflowTransition();
					workflowTransition.Name = name;
					workflowTransition.WorkflowId = workflowId;
					workflowTransition.InputIncidentStatusId = inputIncidentStatusId;
					workflowTransition.OutputIncidentStatusId = outputIncidentStatusId;
					workflowTransition.IsExecuteByOwner = executeByOwner;
					workflowTransition.IsExecuteByDetector = executeByDetector;
					workflowTransition.IsNotifyOwner = notifyOwner;
					workflowTransition.IsNotifyDetector = notifyDetector;
					workflowTransition.NotifySubject = notifySubject;
                    workflowTransition.IsSignatureRequired = signatureRequired;

					//Persist the new workflow transition
					context.WorkflowTransitions.AddObject(workflowTransition);
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
					var query = from wt in context.WorkflowTransitions.Include("TransitionRoles")
								where wt.WorkflowTransitionId == workflowTransitionId
								select wt;

					WorkflowTransition workflowTransition = query.FirstOrDefault();

					//Make sure we have a workflow transition
					if (workflowTransition != null)
					{
						//Delete the workflow transition and dependent entities
						context.WorkflowTransitions.DeleteObject(workflowTransition);
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
		public void WorkflowTransition_Update(WorkflowTransition workflowTransition)
		{
			const string METHOD_NAME = CLASS_NAME + "WorkflowTransition_Update";

			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					context.WorkflowTransitions.ApplyChanges(workflowTransition);
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
		/// Makes a copy of the specified workflow
		/// </summary>
		/// <param name="sourceWorkflowId">The workflow to copy</param>
		/// <returns>The copy of the workflow</returns>
		public Workflow Workflow_Copy(int sourceWorkflowId)
		{
			const string METHOD_NAME = "Workflow_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Workflow destWorkflow = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the workflow and associated transitions, fields and custom properties
					var query = from w in context.Workflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties")
								where w.WorkflowId == sourceWorkflowId
								select w;

					Workflow sourceWorkflow = query.FirstOrDefault();
					if (sourceWorkflow == null)
					{
						throw new ArtifactNotExistsException("The workflow being copied no longer exists");
					}

					//Create the copied workflow
					//except that we will always insert it with default=No
					destWorkflow = new Workflow();
					destWorkflow.ProjectTemplateId = sourceWorkflow.ProjectTemplateId;
					destWorkflow.Name = GlobalResources.General.Global_CopyOf + " " + sourceWorkflow.Name;
					destWorkflow.IsActive = sourceWorkflow.IsActive;
					destWorkflow.IsDefault = false;
					destWorkflow.IsNotify = sourceWorkflow.IsNotify;

					//Now add the transitions (including the transition roles)
					for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
					{
						WorkflowTransition workflowTransition = new WorkflowTransition();
						workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
						workflowTransition.InputIncidentStatusId = sourceWorkflow.Transitions[i].InputIncidentStatusId;
						workflowTransition.OutputIncidentStatusId = sourceWorkflow.Transitions[i].OutputIncidentStatusId;
						workflowTransition.IsExecuteByDetector = sourceWorkflow.Transitions[i].IsExecuteByDetector;
						workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                        workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
						workflowTransition.IsNotifyDetector = sourceWorkflow.Transitions[i].IsNotifyDetector;
						workflowTransition.IsNotifyOwner = sourceWorkflow.Transitions[i].IsNotifyOwner;
						workflowTransition.NotifySubject = sourceWorkflow.Transitions[i].NotifySubject;
						destWorkflow.Transitions.Add(workflowTransition);

						//Now the roles
						for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
						{
							WorkflowTransitionRole workflowTransitionRole = new WorkflowTransitionRole();
							workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
							workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
							workflowTransition.TransitionRoles.Add(workflowTransitionRole);
						}
					}

					//Now add the fields
					for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
					{
						WorkflowField workflowField = new WorkflowField();
						workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
						workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
						workflowField.IncidentStatusId = sourceWorkflow.Fields[i].IncidentStatusId;
						destWorkflow.Fields.Add(workflowField);
					}

					//Now add the custom properties
					for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
					{
						WorkflowCustomProperty workflowCustomProperty = new WorkflowCustomProperty();
						workflowCustomProperty.CustomPropertyId = sourceWorkflow.CustomProperties[i].CustomPropertyId;
						workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
						workflowCustomProperty.IncidentStatusId = sourceWorkflow.CustomProperties[i].IncidentStatusId;
						destWorkflow.CustomProperties.Add(workflowCustomProperty);
					}

					//Save the new workflow
					context.Workflows.AddObject(destWorkflow);
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
        /// Deletes all the workflows in a specific template
        /// </summary>
        /// <param name="projectTemplateId">The id of the template</param>
        protected internal void Workflow_DeleteAllForProjectTemplate (int projectTemplateId)
        {
			const string METHOD_NAME = "Workflow_DeleteAllForProjectTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
            {
                List<Workflow> workflows = this.Workflow_Retrieve(projectTemplateId, false);
                for (int i = 0; i < workflows.Count; i++)
                {
                    //First we need to make it inactive and not-default
                    Workflow workflow = workflows[i];
                    workflow.StartTracking();
                    workflow.IsDefault = false;
                    workflow.IsActive = false;
                    this.Workflow_Update(workflow);
                    this.Workflow_Delete(workflow.WorkflowId);
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
					var query = from w in context.Workflows
								where w.WorkflowId == workflowId
								select w;

					Workflow workflow = query.FirstOrDefault();

					//Make sure we have a workflow
					if (workflow != null)
					{
						//If the workflow is marked as default, you can't delete it
						if (workflow.IsDefault)
						{
							throw new WorkflowInUseException("You cannot delete a workflow that is marked as the default");
						}

						//Delete the workflow. The database cascades will handle the dependent entities
						context.Workflows.DeleteObject(workflow);
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

		/// <summary>Notifies users when an incident status changes. This is based on workflow-rules</summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="incident">The incident artifact</param>
		/// <param name="incidentId">The id of the incident</param>
		/// <returns>The number of notifications sent</returns>
		/// <remarks>This method handles exceptions quietly, just logging them out</remarks>
		public int Workflow_NotifyStatusChange(int incidentId, int workflowId, Incident incident)
		{
			const string METHOD_NAME = "Workflow_NotifyStatusChange()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Generate the email class.
            NotificationManager.EmailMessageDetails msgToSend = new NotificationManager.EmailMessageDetails();
			msgToSend.projectToken = "PR-";
			msgToSend.artifactTokens.Add(0, "IN-" + incidentId.ToString());

			int notifyCount = 0;
			try
			{
                //First lets retrieve the transition controlling these two status codes
                int projectId = incident.ProjectId;
                int currentIncidentStatusId = incident.IncidentStatusId;
				int originalIncidentStatusId = currentIncidentStatusId;
				if (incident.ChangeTracker.OriginalValues.ContainsKey("IncidentStatusId"))
				{
					originalIncidentStatusId = (int)incident.ChangeTracker.OriginalValues["IncidentStatusId"];
				}
				WorkflowTransition workflowTransition = WorkflowTransition_RetrieveByStatuses(workflowId, originalIncidentStatusId, currentIncidentStatusId);

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //First make sure that notifications are turned on for this workflow
                if (workflowTransition != null && workflowTransition.Workflow.IsNotify)
				{
					IncidentManager incidentManager = new IncidentManager();
					//Create the subject list, add our default subject to it.
					string oldStatusName = incidentManager.IncidentStatus_RetrieveById(originalIncidentStatusId).Name;

					//Get the current version of the Incident..
					IncidentView incidentView = incidentManager.RetrieveById2(incidentId);
					if (incidentView != null)
					{
						//Get all avaialble fields..
						List<ArtifactField> incidentFields = new ArtifactManager().ArtifactField_RetrieveAll((int)Artifact.ArtifactTypeEnum.Incident, false, false);

						//Set the subject, see if we have a custom one specified
						string subject;
						if (String.IsNullOrWhiteSpace(workflowTransition.NotifySubject))
						{
							string newStatusName = incidentView.IncidentStatusName;
							subject = ConfigurationSettings.Default.License_ProductType + " | " + GlobalResources.General.Global_Incident + " IN-" + incidentId.ToString() + " " + String.Format(GlobalResources.General.Workflow_ChangeStatusSubject, oldStatusName, newStatusName);
						}
						else
						{
							subject = workflowTransition.NotifySubject;
						}
						//Translate the subject..
                        NotificationManager notMgr = new NotificationManager();
						subject = notMgr.TranslateTemplate(incidentView, subject, incidentFields, false);

						msgToSend.subjectList.Add(0, subject);

						//Update the project header.
						msgToSend.projectToken += incidentView.ProjectId.ToString();

						//Get the body of the e-mail.
                        string strBody = notMgr.TranslateTemplate(incidentView, notMgr.RetrieveTemplateTextById(projectTemplateId, (int)Artifact.ArtifactTypeEnum.Incident), incidentFields, true);

						// - Opener.
						if (workflowTransition.IsNotifyDetector)
						{
							try
							{
								User user = new UserManager().GetUserById(incident.OpenerId);
								//Check to make sure the user is set to receive emails first.
								if (!ConfigurationSettings.Default.EmailSettings_AllowUserControl ||
									(ConfigurationSettings.Default.EmailSettings_AllowUserControl && user.Profile.IsEmailEnabled))
								{
									//Make sure they don't already exist.
									if (msgToSend.toUserList.Where(u => u.UserId == user.UserId).Count() == 0)
									{
                                        msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
										{
											Address = user.EmailAddress,
											ArtifactTokenId = 0,
											Name = user.FullName,
											SubjectId = 0,
											UserId = user.UserId,
											Source = "<WK-" + workflowId.ToSafeString() + " WT-" + workflowTransition.WorkflowTransitionId.ToSafeString() + ">"
											// Can't access Web.GlobalFunctions here to use variables. 
										});
										notifyCount++;
									}
								}
							}
							catch (Exception exception)
							{
								//We handle the exceptions quietly in case the mail-server is not setup correctly
								Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
							}
						}

						// - Owner.
						if (workflowTransition.IsNotifyOwner && incident.OwnerId.HasValue)
						{
							try
							{
								User user = new UserManager().GetUserById(incident.OwnerId.Value);
								//Check to make sure the user is set to receive emails first.
								if (!ConfigurationSettings.Default.EmailSettings_AllowUserControl ||
									(ConfigurationSettings.Default.EmailSettings_AllowUserControl && user.Profile.IsEmailEnabled))
								{
									//Make sure they don't already exist.
									if (msgToSend.toUserList.Where(u => u.UserId == user.UserId).Count() == 0)
									{
                                        msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
										{
											Address = user.EmailAddress,
											ArtifactTokenId = 0,
											Name = user.FullName,
											SubjectId = 0,
											UserId = user.UserId,
											Source = "<WK-" + workflowId.ToSafeString() + " WT-" + workflowTransition.WorkflowTransitionId.ToSafeString() + ">"
											// Can't access Web.GlobalFunctions here to use variables. 
										});
										notifyCount++;
									}
								}
							}
							catch (Exception exception)
							{
								//We handle the exceptions quietly in case the mail-server is not setup correctly
								Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
							}
						}

						// - Role users.
						List<User> users = new UserManager().RetrieveNotifyListForWorkflowRole(projectId, workflowId, originalIncidentStatusId, currentIncidentStatusId);
						foreach (User user in users)
						{
							//Check to make sure the user is set to receive emails first.
							if (!ConfigurationSettings.Default.EmailSettings_AllowUserControl ||
								(ConfigurationSettings.Default.EmailSettings_AllowUserControl && user.Profile.IsEmailEnabled))
							{
								//Make sure they don't already exist.
								if (msgToSend.toUserList.Where(u => u.UserId == user.UserId).Count() == 0)
								{
                                    msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
									{
										Address = user.EmailAddress,
										ArtifactTokenId = 0,
										Name = user.FullName,
										SubjectId = 0,
										UserId = user.UserId,
										Source = "<WK-" + workflowId.ToSafeString() + " WT-" + workflowTransition.WorkflowTransitionId.ToSafeString() + ">"
									});
									notifyCount++;
								}
							}
						}

						//Now, call the Send email function.
						notMgr.SendEmail(msgToSend, strBody);
					}
				}
			}
			catch (System.Exception exception)
			{
				//We handle the exceptions quietly in case the mail-server is not setup correctly
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return notifyCount;
		}

		#endregion
	}

	/// <summary>
	/// This exception is thrown when you try to insert a default workflow that is not marked as active
	/// </summary>
	public class WorkflowMustBeActiveException : ApplicationException
	{
		public WorkflowMustBeActiveException()
		{
		}
		public WorkflowMustBeActiveException(string message)
			: base(message)
		{
		}
		public WorkflowMustBeActiveException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown when you try to mark a workflow inactive that is used by at least one incident type
	/// </summary>
	public class WorkflowInUseException : ApplicationException
	{
		public WorkflowInUseException()
		{
		}
		public WorkflowInUseException(string message)
			: base(message)
		{
		}
		public WorkflowInUseException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
