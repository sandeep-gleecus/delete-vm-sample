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
    /// the requirement workflows that are used to determine the lifecycle of requirements in the system
    /// </summary>
    public class RequirementWorkflowManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.RequirementWorkflowManager::";

        #region Static Methods

        /// <summary>
        /// Converts requirement workflow field collection to the generic kind
        /// </summary>
        /// <param name="requirementWorkflowCustomProps">The requirement workflow fields</param>
        /// <returns>The workflow fields</returns>
        public static List<WorkflowField> ConvertFields(List<RequirementWorkflowField> requirementWorkflowFields)
        {
            //Handle null case safely
            if (requirementWorkflowFields == null)
            {
                return null;
            }

            List<WorkflowField> workflowFields = new List<WorkflowField>();
            foreach (RequirementWorkflowField requirementWorkflowField in requirementWorkflowFields)
            {
                WorkflowField workflowField = new WorkflowField();
                workflowField.ArtifactFieldId = requirementWorkflowField.ArtifactFieldId;
                workflowField.IncidentStatusId = requirementWorkflowField.RequirementStatusId;
                workflowField.WorkflowId = requirementWorkflowField.RequirementWorkflowId;
                workflowField.WorkflowFieldStateId = requirementWorkflowField.WorkflowFieldStateId;
                workflowField.Field = new ArtifactField();
                workflowField.Field.Name = requirementWorkflowField.ArtifactField.Name;
                workflowFields.Add(workflowField);
            }

            return workflowFields;
        }

        /// <summary>
        /// Converts requirement workflow custom properties collection to the generic kind
        /// </summary>
        /// <param name="requirementWorkflowCustomProps">The requirement workflow custom properties</param>
        /// <returns>The workflow custom properties</returns>
        public static List<WorkflowCustomProperty> ConvertFields(List<RequirementWorkflowCustomProperty> requirementWorkflowCustomProps)
        {
            //Handle null case safely
            if (requirementWorkflowCustomProps == null)
            {
                return null;
            }

            List<WorkflowCustomProperty> workflowCustomProperties = new List<WorkflowCustomProperty>();
            foreach (RequirementWorkflowCustomProperty requirementWorkflowCustomProp in requirementWorkflowCustomProps)
            {
                WorkflowCustomProperty workflowCustomProperty = new WorkflowCustomProperty();
                workflowCustomProperty.CustomPropertyId = requirementWorkflowCustomProp.CustomPropertyId;
                workflowCustomProperty.IncidentStatusId = requirementWorkflowCustomProp.RequirementStatusId;
                workflowCustomProperty.WorkflowId = requirementWorkflowCustomProp.RequirementWorkflowId;
                workflowCustomProperty.WorkflowFieldStateId = requirementWorkflowCustomProp.WorkflowFieldStateId;
                workflowCustomProperty.CustomProperty = new CustomProperty();
                workflowCustomProperty.CustomProperty.PropertyNumber = requirementWorkflowCustomProp.CustomProperty.PropertyNumber;
                workflowCustomProperties.Add(workflowCustomProperty);
            }

            return workflowCustomProperties;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the workflow fields and custom properties associated with a particular requirement status
        /// </summary>
        /// <param name="status">The status whose fields and custom properties we want to update</param>
        public void Status_UpdateFieldsAndCustomProperties(RequirementStatus status)
        {
            const string METHOD_NAME = "Status_UpdateFieldsAndCustomProperties";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Attach the changed entity to the EF context and persist changes
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    context.RequirementStati.ApplyChanges(status);
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

        /// <summary>Retrieves the requirement status by the given ID.</summary>
        /// <param name="requirementStatusId">The status ID to retrieve.</param>
        /// <returns>The RequirementStatus, or null if not found.</returns>
        /// <remarks>Will return deleted items.</remarks>
        /// <param name="includeWorkflowFieldsForWorkflowId">Should we include the linked workflow fields</param>
        public RequirementStatus Status_RetrieveById(int requirementStatusId, int? includeWorkflowFieldsForWorkflowId = null)
        {
            const string METHOD_NAME = "Status_RetrieveById()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RequirementStatus retStatus = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from r in context.RequirementStati
                                where r.RequirementStatusId == requirementStatusId
                                select r;

                    retStatus = query.FirstOrDefault();

                    //Get the fields and custom properties, joined by EF 'fix-up'
                    if (includeWorkflowFieldsForWorkflowId.HasValue)
                    {
                        int workflowId = includeWorkflowFieldsForWorkflowId.Value;
                        var query2 = from w in context.RequirementWorkflowFields
                                     where w.RequirementStatusId == requirementStatusId && w.RequirementWorkflowId == workflowId && w.ArtifactField.IsActive
                                     select w;
                        query2.ToList();
                        var query3 = from w in context.RequirementWorkflowCustomProperties
                                     where w.RequirementStatusId == requirementStatusId && w.RequirementWorkflowId == workflowId && !w.CustomProperty.IsDeleted
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
        public RequirementWorkflow Workflow_GetDefault(int projectTemplateId)
		{
            const string METHOD_NAME = "Workflow_GetDefault";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from w in context.RequirementWorkflows
                                where w.IsActive && w.IsDefault && w.ProjectTemplateId == projectTemplateId
                                select w;

                    RequirementWorkflow workflow = query.FirstOrDefault();
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
        /// Gets the workflow for a specific requirement type
        /// </summary>
        /// <param name="requirementTypeId">The requirement type we're interested in</param>
        /// <returns>The workflow id</returns>
        public int Workflow_GetForRequirementType(int requirementTypeId)
		{
            const string METHOD_NAME = "Workflow_GetForRequirementType";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the type
                    var query = from r in context.RequirementTypes.Include(r => r.Workflow)
                                where r.RequirementTypeId == requirementTypeId
                                select r;

                    RequirementType requirementType = query.FirstOrDefault();
                    if (requirementType == null || requirementType.Workflow == null || !requirementType.RequirementWorkflowId.HasValue)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve workflow for requirement type " + requirementTypeId + ".");
                    }
                    else
                    {
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return requirementType.RequirementWorkflowId.Value;
                    }
                }
            }
            catch (ArtifactNotExistsException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
		}

		/// <summary>
		/// Retrieves the list of inactive, hidden and required fields for an requirement based on its position
		/// in the workflow.
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="requirementStatusId">The current step (i.e. requirement status) in the workflow</param>
        /// <remarks>Workflow needs to be active</remarks>
		public List<RequirementWorkflowField> Workflow_RetrieveFieldStates(int workflowId, int requirementStatusId)
		{
            const string METHOD_NAME = "Workflow_RetrieveFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.RequirementWorkflowFields.Include("FieldState").Include("ArtifactField")
                                where w.RequirementWorkflowId == workflowId && w.Workflow.IsActive && w.ArtifactField.IsActive && w.RequirementStatusId == requirementStatusId
                                orderby w.WorkflowFieldStateId, w.ArtifactFieldId
                                select w;

                    List<RequirementWorkflowField> workflowFields = query.ToList();

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
        /// Retrieves the list of inactive, hidden and required custom properties for an requirement based on its position
        /// in the workflow.
        /// </summary>
        /// <param name="workflowId">The workflow we're using</param>
        /// <param name="requirementStatusId">The current step (i.e. status) in the workflow</param>
        /// <returns>List of inactive, hidden and required custom properties for the current workflow step</returns>
        /// <remarks>Workflow needs to be active</remarks>
        public List<RequirementWorkflowCustomProperty> Workflow_RetrieveCustomPropertyStates(int workflowId, int requirementStatusId)
        {
            const string METHOD_NAME = "Workflow_RetrieveCustomPropertyStates";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.RequirementWorkflowCustomProperties.Include("FieldState").Include("CustomProperty")
                                where w.RequirementWorkflowId == workflowId &&
                                    w.Workflow.IsActive &&
                                    w.RequirementStatusId == requirementStatusId &&
                                    !w.CustomProperty.IsDeleted
                                orderby w.WorkflowFieldStateId, w.CustomPropertyId
                                select w;

                    List<RequirementWorkflowCustomProperty> workflowCustomProps = query.ToList();

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
        /// Retrieves a workflow transition by its id, includes the transition roles and requirement status names
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="workflowTransitionId">The id of the transition</param>
        /// <returns></returns>
        public RequirementWorkflowTransition WorkflowTransition_RetrieveById(int workflowId, int workflowTransitionId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RequirementWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RequirementWorkflowTransitions.Include("InputRequirementStatus").Include("OutputRequirementStatus").Include("TransitionRoles").Include("TransitionRoles.Role")
                                where t.RequirementWorkflowId == workflowId && t.WorkflowTransitionId == workflowTransitionId
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

		public List<TST_REQUIREMENT_APPROVAL_USERS> WorkflowTransition_RetrieveIsRequiredApproversByInputStatus(int userId, int projectId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveApproversByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				List<TST_REQUIREMENT_APPROVAL_USERS> approvalUsers = new List<TST_REQUIREMENT_APPROVAL_USERS>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RequirementApprovalUsers
								select t;

					var esignature = query.Where(x => x.IS_ACTIVE == true).FirstOrDefault();
					if (esignature != null)
					{
						approvalUsers = context.RequirementApprovalUsers.Where(x => x.PROJECT_ID == projectId && x.IS_ACTIVE).ToList();
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}

				return approvalUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

		}

		public List<TST_REQUIREMENT_APPROVAL_USERS> RetrieveRequiredApproversByProjectId(int projectId)
		{
			const string METHOD_NAME = "RetrieveRequiredApproversByProjectId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				List<TST_REQUIREMENT_APPROVAL_USERS> approvalUsers = new List<TST_REQUIREMENT_APPROVAL_USERS>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RequirementApprovalUsers
								select t;

					var esignature = query.Where(x => x.IS_ACTIVE == true).FirstOrDefault();
					if (esignature != null)
					{
						approvalUsers = context.RequirementApprovalUsers.Where(x => x.PROJECT_ID == projectId && x.IS_ACTIVE).ToList();
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}

				return approvalUsers;
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
		public RequirementWorkflow Workflow_RetrieveById(int workflowId, bool includeTransitions = false, bool includeFieldStates = false)
        {
            const string METHOD_NAME = "Workflow_RetrieveById(int,[bool],[bool])";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See if we need to include transitions
                    ObjectQuery<RequirementWorkflow> workflowSet = context.RequirementWorkflows;
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
                                where w.RequirementWorkflowId == workflowId
                                select w;

                    RequirementWorkflow workflow = query.FirstOrDefault();
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
		/// Determines if a workflow is in use (i.e. associated with one or more requirement type)
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
                    //Get the workflow by the requirement type
                    var query = from it in context.RequirementTypes
                                where it.RequirementWorkflowId == workflowId
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
        public List<RequirementWorkflow> Workflow_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
            const string METHOD_NAME = "Workflow_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<RequirementWorkflow> workflows;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflows
                    var query = from w in context.RequirementWorkflows
                                where (!activeOnly || w.IsActive) && w.ProjectTemplateId == projectTemplateId
                                select w;

                    //For the active case (i.e. a lookup) order by name, otherwise in the admin case, order by id
                    if (activeOnly)
                    {
                        query = query.OrderBy(w => w.Name).ThenBy(w => w.RequirementWorkflowId);
                    }
                    else
                    {
                        query = query.OrderBy(w => w.RequirementWorkflowId);
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
        /// Gets the list of workflow transitions for the specified requirement status where the user has a specific role or is the author/owner
        /// </summary>
        /// <param name="inputRequirementStatusId">The input status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>List of matching workflow transitions</returns>
        /// <param name="isAuthor">Are we the author of the requirement</param>
        /// <param name="isOwner">Are we the owner of the requirement</param>
        /// <param name="projectRoleId">What project role are we</param>
        /// <remarks>This overload considers user permissions/roles when returning the list</remarks>
        public List<RequirementWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputRequirementStatusId, int projectRoleId, bool isAuthor, bool isOwner)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RequirementWorkflowTransitions.Include("InputRequirementStatus").Include("OutputRequirementStatus")
								where t.OutputRequirementStatus.IsActive && t.RequirementWorkflowId == workflowId && t.InputRequirementStatusId == inputRequirementStatusId
                                select t;

                    //Create the expresion if we're the author, owner or in one of the roles allowed to execute the transition
                    List<Expression<Func<RequirementWorkflowTransition, bool>>> expressionList = new List<Expression<Func<RequirementWorkflowTransition, bool>>>();
                    if (isAuthor)
                    {
                        Expression<Func<RequirementWorkflowTransition, bool>> clause = t => t.IsExecuteByCreator;
                        expressionList.Add(clause);
                    }
                    if (isOwner)
                    {
                        Expression<Func<RequirementWorkflowTransition, bool>> clause = t => t.IsExecuteByOwner;
                        expressionList.Add(clause);
                    }

                    //Now the project role
                    Expression<Func<RequirementWorkflowTransition, bool>> clause2 = t => t.TransitionRoles.Any(i => i.Role.ProjectRoleId == projectRoleId && i.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);
                    expressionList.Add(clause2);

                    if (expressionList.Count > 0)
                    {
                        //OR together the different cases and apply to the where clause
                        Expression<Func<RequirementWorkflowTransition, bool>> aggregatedClause = Manager.BuildOr(expressionList.ToArray());
                        query = query.Where(aggregatedClause);
                    }

                    //Add the sorts
                    query = query.OrderBy(t => t.Name).ThenBy(t => t.WorkflowTransitionId);

                    List<RequirementWorkflowTransition> workflowTransitions = query.ToList();

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

		public List<RequirementStatusResponse> WorkflowTransition_RetrieveByInputStatusById(int workflowId, int inputRequirementStatusId, int projectRoleId, bool isAuthor, bool isOwner)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatusById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RequirementWorkflowTransitions.Include("InputRequirementStatus").Include("OutputRequirementStatus")
								join i in context.ReleaseStati
								on t.InputRequirementStatusId equals i.ReleaseStatusId
								join o in context.ReleaseStati
								on t.OutputRequirementStatusId equals o.ReleaseStatusId
								where t.OutputRequirementStatus.IsActive && t.RequirementWorkflowId == workflowId && t.InputRequirementStatusId == inputRequirementStatusId
								select new RequirementStatusResponse
								{
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									RequirementWorkflowId = t.RequirementWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,	
									IsNotifyCreator = t.IsNotifyCreator,	
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
								};

					//Create the expresion if we're the author, owner or in one of the roles allowed to execute the transition
					List<Expression<Func<RequirementStatusResponse, bool>>> expressionList = new List<Expression<Func<RequirementStatusResponse, bool>>>();
					if (isAuthor)
					{
						Expression<Func<RequirementStatusResponse, bool>> clause = t => t.IsExecuteByCreator;
						expressionList.Add(clause);
					}
					if (isOwner)
					{
						Expression<Func<RequirementStatusResponse, bool>> clause = t => t.IsExecuteByOwner;
						expressionList.Add(clause);
					}

					//Now the project role
					Expression<Func<RequirementStatusResponse, bool>> clause2 = t => t.TransitionRoles.Any(i => i.Role.ProjectRoleId == projectRoleId && i.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);
					expressionList.Add(clause2);

					if (expressionList.Count > 0)
					{
						//OR together the different cases and apply to the where clause
						Expression<Func<RequirementStatusResponse, bool>> aggregatedClause = Manager.BuildOr(expressionList.ToArray());
						query = query.Where(aggregatedClause);
					}

					//Add the sorts
					query = query.OrderBy(t => t.TransitionName).ThenBy(t => t.WorkflowTransitionId);

					List<RequirementStatusResponse> workflowTransitions = query.ToList();

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
		/// <param name="inputRequirementStatusId">The input status</param>
		/// <param name="outputRequirementStatusId">The output status</param>
		/// <param name="workflowId">The workflow we're using</param>
		/// <returns>The workflow transition</returns>
		public RequirementWorkflowTransition WorkflowTransition_RetrieveByStatuses(int workflowId, int inputRequirementStatusId, int outputRequirementStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RequirementWorkflowTransitions.Include("InputRequirementStatus").Include("OutputRequirementStatus")
                                where t.RequirementWorkflowId == workflowId &&
                                      t.InputRequirementStatusId == inputRequirementStatusId &&
                                      t.OutputRequirementStatusId == outputRequirementStatusId
                                select t;

                    RequirementWorkflowTransition workflowTransition = query.FirstOrDefault();

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
        /// Gets the list of workflow transitions for the specified requirement status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="inputRequirementStatusId">The id of the input status</param>
        /// <returns>List of transitions</returns>
        public List<RequirementWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputRequirementStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RequirementWorkflowTransitions.Include("InputRequirementStatus").Include("OutputRequirementStatus")
                                where t.OutputRequirementStatus.IsActive && t.RequirementWorkflowId == workflowId && t.InputRequirementStatusId == inputRequirementStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<RequirementWorkflowTransition> workflowTransitions = query.ToList();

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

		public List<RequirementStatusResponse> WorkflowTransition_RetrieveByInputStatusById(int workflowId, int inputRequirementStatusId, int projectTemplateId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RequirementWorkflowTransitions.Include("InputRequirementStatus").Include("OutputRequirementStatus")
								join i in context.RequirementStati
								on t.InputRequirementStatusId equals i.RequirementStatusId
								join o in context.RequirementStati
								on t.OutputRequirementStatusId equals o.RequirementStatusId
								where t.OutputRequirementStatus.IsActive && t.RequirementWorkflowId == workflowId && t.InputRequirementStatusId == inputRequirementStatusId
								orderby t.Name, t.WorkflowTransitionId
								select new RequirementStatusResponse
								{
									InputRequirementStatusId = t.InputRequirementStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									RequirementWorkflowId = t.RequirementWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
									URL = "",
									OutURL = "",
									InURL = "",
									OutputRequirementStatusId = t.OutputRequirementStatusId,
								};

					var workflows = query.ToList();

					foreach (var c in workflows)
					{
						c.InURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/RequirementWorkflowStep.aspx?workflowId=" + workflowId + "&requirementStatusId=" + c.InputRequirementStatusId;
						c.URL = "/ValidationMaster/pt/" + projectTemplateId +"/Administration/RequirementWorkflowTransition.aspx?workflowId=" + workflowId + "&workflowTransitionId=" +c.WorkflowTransitionId;
						c.OutURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/RequirementWorkflowStep.aspx?workflowId=" + workflowId + "&requirementStatusId=" + c.OutputRequirementStatusId;
					}

					query = workflows.AsQueryable();

					List<RequirementStatusResponse> workflowTransitions = query.ToList();

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

		public List<RequirementStatusResponse> WorkflowTransition_RetrieveAllStatuses(int workflowId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RequirementWorkflowTransitions.Include("InputRequirementStatus").Include("OutputRequirementStatus")
								join i in context.RequirementStati
								on t.InputRequirementStatusId equals i.RequirementStatusId
								join o in context.RequirementStati
								on t.OutputRequirementStatusId equals o.RequirementStatusId
								where t.OutputRequirementStatus.IsActive && t.RequirementWorkflowId == workflowId
								orderby i.Position, t.WorkflowTransitionId
								select new RequirementStatusResponse
								{
									InputRequirementStatusId = t.InputRequirementStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									RequirementWorkflowId = t.RequirementWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
								};

					List<RequirementStatusResponse> workflowTransitions = query.ToList();

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
		/// Gets the list of workflow transitions for the specified requirement output status
		/// </summary>
		/// <param name="workflowId">The id of the workflow</param>
		/// <param name="outputRequirementStatusId">The id of the output status</param>
		/// <returns>List of transitions</returns>
		public List<RequirementWorkflowTransition> WorkflowTransition_RetrieveByOutputStatus(int workflowId, int outputRequirementStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByOutputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RequirementWorkflowTransitions.Include("InputRequirementStatus").Include("OutputRequirementStatus")
                                where t.OutputRequirementStatus.IsActive && t.RequirementWorkflowId == workflowId && t.OutputRequirementStatusId == outputRequirementStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<RequirementWorkflowTransition> workflowTransitions = query.ToList();

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
        public void Workflow_Update(RequirementWorkflow workflow)
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
            if (!workflow.IsActive && IsInUse(workflow.RequirementWorkflowId))
            {
                throw new WorkflowInUseException("You cannot make a workflow that is in use inactive");
            }

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.RequirementWorkflows.ApplyChanges(workflow);
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
        /// Inserts a new workflow into the system with all the standard transitions and field states
        /// </summary>
        /// <param name="projectTemplateId">The project template the workflow is associated with</param>
        /// <param name="name">The name of the workflow</param>
        /// <param name="isDefault">Is this the default workflow for the project</param>
        /// <param name="isActive">Is this workflow active</param>
        /// <returns>The newly inserted workflow</returns>
        public RequirementWorkflow Workflow_InsertWithDefaultEntries(int projectTemplateId, string name, bool isDefault, bool isActive = true)
        {
            const string METHOD_NAME = "Workflow_InsertWithDefaultEntries";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First create the workflow itself
                RequirementWorkflow workflow = this.Workflow_Insert(projectTemplateId, name, isDefault, isActive);
                int workflowId = workflow.RequirementWorkflowId;

                //Now populate the transitions and fields

                //Workflow Transitions (inc. roles)
                int workflowTransitionId1 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Requested, (int)Requirement.RequirementStatusEnum.UnderReview, "Review Requirement", false, false, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId2 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Requested, (int)Requirement.RequirementStatusEnum.Accepted, "Accept Requirement", false, false, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId3 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.UnderReview, (int)Requirement.RequirementStatusEnum.Rejected, "Reject Requirement", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId4 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.UnderReview, (int)Requirement.RequirementStatusEnum.Accepted, "Accept Requirement", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId5 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Rejected, (int)Requirement.RequirementStatusEnum.UnderReview, "Return to Review", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId6 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Accepted, (int)Requirement.RequirementStatusEnum.Planned, "Assign Release", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId7 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Planned, (int)Requirement.RequirementStatusEnum.InProgress, "Start Development", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId8 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.InProgress, (int)Requirement.RequirementStatusEnum.Developed, "Finish Development", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId9 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.InProgress, (int)Requirement.RequirementStatusEnum.Completed, "Mark as Completed", false, false, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId10 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Developed, (int)Requirement.RequirementStatusEnum.Tested, "Mark as Tested", false, false, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId11 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Developed, (int)Requirement.RequirementStatusEnum.Completed, "Mark as Completed", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId12 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Tested, (int)Requirement.RequirementStatusEnum.Completed, "Mark as Completed", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId13 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Completed, (int)Requirement.RequirementStatusEnum.Obsolete, "Mark as Obsolete", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId14 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Obsolete, (int)Requirement.RequirementStatusEnum.UnderReview, "Return to Review", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                //Reverse direction transitions
                int workflowTransitionId15 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Accepted, (int)Requirement.RequirementStatusEnum.UnderReview, "Return to Review", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId16 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Planned, (int)Requirement.RequirementStatusEnum.Accepted, "Remove from Plan", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId17 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.InProgress, (int)Requirement.RequirementStatusEnum.Planned, "Cancel Development", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId18 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Developed, (int)Requirement.RequirementStatusEnum.InProgress, "Continue Development", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId19 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Completed, (int)Requirement.RequirementStatusEnum.InProgress, "Continue Development", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId20 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Completed, (int)Requirement.RequirementStatusEnum.Tested, "Not Completed", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId21 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Completed, (int)Requirement.RequirementStatusEnum.Developed, "Continue Testing", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId22 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Tested, (int)Requirement.RequirementStatusEnum.Developed, "Continue Testing", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId23 = WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Obsolete, (int)Requirement.RequirementStatusEnum.Completed, "Not Obsolete", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                //Workflow Fields

                //All fields are active, visible and not-required by default, so only need to populate the ones
                //that are exceptions to that case
                //Accepted - Required:Importance
                RequirementStatus requirementStatus = Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
                requirementStatus.StartTracking();
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 18, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                Status_UpdateFieldsAndCustomProperties(requirementStatus);
                //Rejected - Disabled:Importance,ReleaseId,EstimatePoints,Name,Owner,Author,Component,Type,Description
                requirementStatus = Status_RetrieveById((int)Requirement.RequirementStatusEnum.Rejected, workflowId);
                requirementStatus.StartTracking();
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 18, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 19, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 142, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 86, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 73, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 17, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 141, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 140, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 104, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(requirementStatus);
                //Planned - Required:Importance,ReleaseId,EstimatePoints
                requirementStatus = Status_RetrieveById((int)Requirement.RequirementStatusEnum.Planned, workflowId);
                requirementStatus.StartTracking();
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 18, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 19, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 142, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                Status_UpdateFieldsAndCustomProperties(requirementStatus);
                //In Progress - Required:Importance,ReleaseId,EstimatePoints
                requirementStatus = Status_RetrieveById((int)Requirement.RequirementStatusEnum.InProgress, workflowId);
                requirementStatus.StartTracking();
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 18, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 19, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 142, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                Status_UpdateFieldsAndCustomProperties(requirementStatus);
                //Developed - Required:Importance,EstimatePoints Disabled:ReleaseId
                requirementStatus = Status_RetrieveById((int)Requirement.RequirementStatusEnum.Developed, workflowId);
                requirementStatus.StartTracking();
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 18, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 142, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 19, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(requirementStatus);
                //Tested - Required:Importance,EstimatePoints Disabled:ReleaseId
                requirementStatus = Status_RetrieveById((int)Requirement.RequirementStatusEnum.Tested, workflowId);
                requirementStatus.StartTracking();
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 18, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 142, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 19, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(requirementStatus);
                //Completed - Disabled:Importance,ReleaseId,EstimatePoints,Name,Owner,Author,Component,Type,Description
                requirementStatus = Status_RetrieveById((int)Requirement.RequirementStatusEnum.Completed, workflowId);
                requirementStatus.StartTracking();
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 18, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 19, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 142, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 86, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 73, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 17, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 141, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 140, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 104, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(requirementStatus);
                //Obsolete - Disabled:Importance,ReleaseId,EstimatePoints,Name,Owner,Author,Component,Type,Description
                requirementStatus = Status_RetrieveById((int)Requirement.RequirementStatusEnum.Obsolete, workflowId);
                requirementStatus.StartTracking();
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 18, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 19, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 142, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 86, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 73, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 17, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 141, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 140, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                requirementStatus.WorkflowFields.Add(new RequirementWorkflowField() { RequirementWorkflowId = workflowId, ArtifactFieldId = 104, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(requirementStatus);

                //Return the workflow
                Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
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
        /// Inserts a new workflow into the system
        /// </summary>
        /// <param name="projectTemplateId">The project template the workflow is associated with</param>
        /// <param name="name">The name of the workflow</param>
        /// <param name="isDefault">Is this the default workflow for the project</param>
        /// <param name="isActive">Is this workflow active</param>
        /// <returns>The newly inserted workflow</returns>
        public RequirementWorkflow Workflow_Insert(int projectTemplateId, string name, bool isDefault, bool isActive = true)
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
                RequirementWorkflow workflow;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow entity
                    workflow = new RequirementWorkflow();
                    workflow.ProjectTemplateId = projectTemplateId;
                    workflow.Name = name;
                    workflow.IsActive = isActive;
                    workflow.IsDefault = isDefault;

                    //Persist the new workflow
                    context.RequirementWorkflows.AddObject(workflow);
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
        /// <param name="inputRequirementStatusId">The input requirement status for the transition</param>
        /// <param name="outputRequirementStatusId">The output requirement status for the transition</param>
        /// <param name="name">The name of the transition</param>
        /// <param name="executeByCreator"></param>
        /// <param name="executeByOwner"></param>
        /// <param name="roles">The list of any roles that can also execute the transition</param>
        /// <returns>The new workflow transition</returns>
        public RequirementWorkflowTransition WorkflowTransition_Insert(int workflowId, int inputRequirementStatusId, int outputRequirementStatusId, string name, bool executeByCreator = false, bool executeByOwner = true, List<int> roles = null)
        {
            const string METHOD_NAME = "Workflow_Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RequirementWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow transition entity
                    workflowTransition = new RequirementWorkflowTransition();
                    workflowTransition.Name = name;
                    workflowTransition.RequirementWorkflowId = workflowId;
                    workflowTransition.InputRequirementStatusId = inputRequirementStatusId;
                    workflowTransition.OutputRequirementStatusId = outputRequirementStatusId;
                    workflowTransition.IsExecuteByOwner = executeByOwner;
                    workflowTransition.IsExecuteByCreator = executeByCreator;

                    //Add the transition roles if any provided
                    if (roles != null && roles.Count > 0)
                    {
                        foreach (int projectRoleId in roles)
                        {
                            //For requirement workflows, all transition roles are execute transitions (not notify)
                            RequirementWorkflowTransitionRole transitionRole = new RequirementWorkflowTransitionRole();
                            transitionRole.ProjectRoleId = projectRoleId;
                            transitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
                            workflowTransition.TransitionRoles.Add(transitionRole);
                        }
                    }

                    //Persist the new workflow transition
                    context.RequirementWorkflowTransitions.AddObject(workflowTransition);
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
                    var query = from wt in context.RequirementWorkflowTransitions.Include("TransitionRoles")
                                where wt.WorkflowTransitionId == workflowTransitionId
                                select wt;

                    RequirementWorkflowTransition workflowTransition = query.FirstOrDefault();

                    //Make sure we have a workflow transition
                    if (workflowTransition != null)
                    {
                        //Delete the workflow transition and dependent entities
                        context.RequirementWorkflowTransitions.DeleteObject(workflowTransition);
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
        public void WorkflowTransition_Update(RequirementWorkflowTransition workflowTransition)
        {
            const string METHOD_NAME = CLASS_NAME + "WorkflowTransition_Update";

            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.RequirementWorkflowTransitions.ApplyChanges(workflowTransition);
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
        public RequirementWorkflow Workflow_Copy(int sourceWorkflowId)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RequirementWorkflow destWorkflow = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow and associated transitions, fields and custom properties
                    var query = from w in context.RequirementWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties")
                                where w.RequirementWorkflowId == sourceWorkflowId
                                select w;

                    RequirementWorkflow sourceWorkflow = query.FirstOrDefault();
                    if (sourceWorkflow == null)
                    {
                        throw new ArtifactNotExistsException("The workflow being copied no longer exists");
                    }

                    //Create the copied workflow
                    //except that we will always insert it with default=No
                    destWorkflow = new RequirementWorkflow();
                    destWorkflow.ProjectTemplateId = sourceWorkflow.ProjectTemplateId;
                    destWorkflow.Name = GlobalResources.General.Global_CopyOf + " " + sourceWorkflow.Name;
                    destWorkflow.IsActive = sourceWorkflow.IsActive;
                    destWorkflow.IsDefault = false;

                    //Now add the transitions (including the transition roles)
                    for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                    {
                        RequirementWorkflowTransition workflowTransition = new RequirementWorkflowTransition();
                        workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                        workflowTransition.InputRequirementStatusId = sourceWorkflow.Transitions[i].InputRequirementStatusId;
                        workflowTransition.OutputRequirementStatusId = sourceWorkflow.Transitions[i].OutputRequirementStatusId;
                        workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                        workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                        workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                        destWorkflow.Transitions.Add(workflowTransition);

                        //Now the roles
                        for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                        {
                            RequirementWorkflowTransitionRole workflowTransitionRole = new RequirementWorkflowTransitionRole();
                            workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                            workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                            workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                        }
                    }

                    //Now add the fields
                    for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                    {
                        RequirementWorkflowField workflowField = new RequirementWorkflowField();
                        workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                        workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                        workflowField.RequirementStatusId = sourceWorkflow.Fields[i].RequirementStatusId;
                        destWorkflow.Fields.Add(workflowField);
                    }

                    //Now add the custom properties
                    for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                    {
                        RequirementWorkflowCustomProperty workflowCustomProperty = new RequirementWorkflowCustomProperty();
                        workflowCustomProperty.CustomPropertyId = sourceWorkflow.CustomProperties[i].CustomPropertyId;
                        workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                        workflowCustomProperty.RequirementStatusId = sourceWorkflow.CustomProperties[i].RequirementStatusId;
                        destWorkflow.CustomProperties.Add(workflowCustomProperty);
                    }

                    //Save the new workflow
                    context.RequirementWorkflows.AddObject(destWorkflow);
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
        /// Copies all the workflows from one project template to another (used when making a new project based on an existing one)
        /// </summary>
        /// <param name="requirementWorkflowMapping">The mapping of workflow ids between the old and new templates</param>
        /// <param name="sourceProjectTemplateId">The project template to copy the workflows from</param>
        /// <param name="destProjectTemplateId">The destination project template</param>
        /// <param name="customPropertyIdMapping">The mapping of old vs. new custom properties between the two project templates</param>
        public void Workflow_Copy(int sourceProjectTemplateId, int destProjectTemplateId, Dictionary<int, int> customPropertyIdMapping, Dictionary<int, int> requirementWorkflowMapping)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Dictionary<int, RequirementWorkflow> tempMapping = new Dictionary<int, RequirementWorkflow>();
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflows and associated transitions, fields requirement types, and custom properties
                    var query = from w in context.RequirementWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties").Include("RequirementTypes")
                                where w.ProjectTemplateId == sourceProjectTemplateId
                                orderby w.RequirementWorkflowId
                                select w;

                    List<RequirementWorkflow> sourceWorkflows = query.ToList();

                    //Loop through the various workflows
                    foreach (RequirementWorkflow sourceWorkflow in sourceWorkflows)
                    {
                        //Create the copied workflow
                        RequirementWorkflow destWorkflow = new RequirementWorkflow();
                        destWorkflow.ProjectTemplateId = destProjectTemplateId;
                        destWorkflow.Name = sourceWorkflow.Name;
                        destWorkflow.IsActive = sourceWorkflow.IsActive;
                        destWorkflow.IsDefault = sourceWorkflow.IsDefault;

                        //Add to mapping
                        tempMapping.Add(sourceWorkflow.RequirementWorkflowId, destWorkflow);

                        //Now add the transitions (including the transition roles)
                        for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                        {
                            RequirementWorkflowTransition workflowTransition = new RequirementWorkflowTransition();
                            workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                            workflowTransition.InputRequirementStatusId = sourceWorkflow.Transitions[i].InputRequirementStatusId;
                            workflowTransition.OutputRequirementStatusId = sourceWorkflow.Transitions[i].OutputRequirementStatusId;
                            workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                            workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                            workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                            destWorkflow.Transitions.Add(workflowTransition);

                            //Now the roles
                            for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                            {
                                RequirementWorkflowTransitionRole workflowTransitionRole = new RequirementWorkflowTransitionRole();
                                workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                                workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                                workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                            }
                        }

                        //Now add the fields
                        for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                        {
                            RequirementWorkflowField workflowField = new RequirementWorkflowField();
                            workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                            workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                            workflowField.RequirementStatusId = sourceWorkflow.Fields[i].RequirementStatusId;
                            destWorkflow.Fields.Add(workflowField);
                        }

                        //Now add the custom properties
                        for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                        {
                            //Get the corresponding custom property in the new project
                            if (customPropertyIdMapping.ContainsKey(sourceWorkflow.CustomProperties[i].CustomPropertyId))
                            {
                                int destCustomPropertyId = customPropertyIdMapping[sourceWorkflow.CustomProperties[i].CustomPropertyId];
                                RequirementWorkflowCustomProperty workflowCustomProperty = new RequirementWorkflowCustomProperty();
                                workflowCustomProperty.CustomPropertyId = destCustomPropertyId;
                                workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                                workflowCustomProperty.RequirementStatusId = sourceWorkflow.CustomProperties[i].RequirementStatusId;
                                destWorkflow.CustomProperties.Add(workflowCustomProperty);
                            }
                        }

                        //Add the new workflow
                        context.RequirementWorkflows.AddObject(destWorkflow);
                    }

                    //Save the changes
                    context.SaveChanges();
                }
                
                //Finally populate the external mapping dictionary (id based)
                foreach(KeyValuePair<int, RequirementWorkflow> kvp in tempMapping)
                {
                    requirementWorkflowMapping.Add(kvp.Key, kvp.Value.RequirementWorkflowId);
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
        /// Deletes all the workflows in the project template (called by the project delete)
        /// </summary>
        /// <param name="projectTemplateId">The project template to delete workflow for</param>
        /// <remarks>It requires that the associated requirement types are already deleted</remarks>
        protected internal void Workflow_DeleteAllForProjectTemplate(int projectTemplateId)
        {
            const string METHOD_NAME = "Workflow_DeleteAllForProjectTemplate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow
                    var query = from w in context.RequirementWorkflows
                                where w.ProjectTemplateId == projectTemplateId
                                orderby w.RequirementWorkflowId
                                select w;

                    List<RequirementWorkflow> workflows = query.ToList();
                    for (int i = 0; i < workflows.Count; i++)
                    {
                        context.RequirementWorkflows.DeleteObject(workflows[i]);
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
                    var query = from w in context.RequirementWorkflows
                                where w.RequirementWorkflowId == workflowId
                                select w;

                    RequirementWorkflow workflow = query.FirstOrDefault();

                    //Make sure we have a workflow
                    if (workflow != null)
                    {
                        //If the workflow is marked as default, you can't delete it
                        if (workflow.IsDefault)
                        {
                            throw new WorkflowInUseException("You cannot delete a workflow that is marked as the default");
                        }

                        //Delete the workflow. The database cascades will handle the dependent entities
                        context.RequirementWorkflows.DeleteObject(workflow);
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

		public List<TST_REQUIREMENT_APPROVAL_USERS> GetApproversForRequirementWorkflowTransition(int projectId, int workflowTransitionId)
		{
			const string METHOD_NAME = "GetApproversForTestCaseWorkflowTransition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the workflow
					var query = from w in context.RequirementApprovalUsers
								where w.PROJECT_ID == projectId && w.IS_ACTIVE && w.WORKFLOW_TRANSITION_ID == workflowTransitionId && w.IS_ACTIVE
								orderby w.ORDER_ID
								select w;

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

					return query.ToList();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void UpdateApproversForRequirementWorkflowTransition(List<TST_REQUIREMENT_APPROVAL_USERS> updatedEntities)
		{

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				foreach (var item in updatedEntities)
				{
					context.RequirementApprovalUsers.ApplyChanges(item);
				}

				context.SaveChanges();
			}
		}
		#endregion
	}
}
