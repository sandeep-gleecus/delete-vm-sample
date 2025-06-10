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
    /// the document workflows that are used to determine the lifecycle of documents in the system
    /// </summary>
    public class DocumentWorkflowManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.DocumentWorkflowManager::";

        #region Static Methods

        /// <summary>
        /// Converts document workflow field collection to the generic kind
        /// </summary>
        /// <param name="documentWorkflowCustomProps">The document workflow fields</param>
        /// <returns>The workflow fields</returns>
        public static List<WorkflowField> ConvertFields(List<DocumentWorkflowField> documentWorkflowFields)
        {
            //Handle null case safely
            if (documentWorkflowFields == null)
            {
                return null;
            }

            List<WorkflowField> workflowFields = new List<WorkflowField>();
            foreach (DocumentWorkflowField documentWorkflowField in documentWorkflowFields)
            {
                WorkflowField workflowField = new WorkflowField();
                workflowField.ArtifactFieldId = documentWorkflowField.ArtifactFieldId;
                workflowField.IncidentStatusId = documentWorkflowField.DocumentStatusId;
                workflowField.WorkflowId = documentWorkflowField.DocumentWorkflowId;
                workflowField.WorkflowFieldStateId = documentWorkflowField.WorkflowFieldStateId;
                workflowField.Field = new ArtifactField();
                workflowField.Field.Name = documentWorkflowField.ArtifactField.Name;
                workflowFields.Add(workflowField);
            }

            return workflowFields;
        }

        /// <summary>
        /// Converts document workflow custom properties collection to the generic kind
        /// </summary>
        /// <param name="documentWorkflowCustomProps">The document workflow custom properties</param>
        /// <returns>The workflow custom properties</returns>
        public static List<WorkflowCustomProperty> ConvertFields(List<DocumentWorkflowCustomProperty> documentWorkflowCustomProps)
        {
            //Handle null case safely
            if (documentWorkflowCustomProps == null)
            {
                return null;
            }

            List<WorkflowCustomProperty> workflowCustomProperties = new List<WorkflowCustomProperty>();
            foreach (DocumentWorkflowCustomProperty documentWorkflowCustomProp in documentWorkflowCustomProps)
            {
                WorkflowCustomProperty workflowCustomProperty = new WorkflowCustomProperty();
                workflowCustomProperty.CustomPropertyId = documentWorkflowCustomProp.CustomPropertyId;
                workflowCustomProperty.IncidentStatusId = documentWorkflowCustomProp.DocumentStatusId;
                workflowCustomProperty.WorkflowId = documentWorkflowCustomProp.DocumentWorkflowId;
                workflowCustomProperty.WorkflowFieldStateId = documentWorkflowCustomProp.WorkflowFieldStateId;
                workflowCustomProperty.CustomProperty = new CustomProperty();
                workflowCustomProperty.CustomProperty.PropertyNumber = documentWorkflowCustomProp.CustomProperty.PropertyNumber;
                workflowCustomProperties.Add(workflowCustomProperty);
            }

            return workflowCustomProperties;
        }

		#endregion

		#region Public Methods

		public List<DocumentStatusResponse> WorkflowTransition_RetrieveByInputStatusById(int workflowId, int inputDocumentStatusId, int projectTemplateId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.DocumentWorkflowTransitions.Include("InputDocumentStatus").Include("OutputDocumentStatus")
								join i in context.DocumentStati
								on t.InputDocumentStatusId equals i.DocumentStatusId
								join o in context.DocumentStati
								on t.OutputDocumentStatusId equals o.DocumentStatusId
								where t.OutputDocumentStatus.IsActive && t.DocumentWorkflowId == workflowId && t.InputDocumentStatusId == inputDocumentStatusId
								orderby t.Name, t.WorkflowTransitionId
								select new DocumentStatusResponse
								{
									InputDocumentStatusId = t.InputDocumentStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByEditor,
									IsExecuteByOwner = t.IsExecuteByAuthor,
									WorkflowTransitionId = t.WorkflowTransitionId,
									DocumentWorkflowId = t.DocumentWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
									URL = "",
									OutURL = "",
									InURL = "",
									OutputDocumentStatusId = t.OutputDocumentStatusId,
								};

					var workflows = query.ToList();

					foreach (var c in workflows)
					{
						c.InURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/DocumentWorkflowStep.aspx?workflowId=" + workflowId + "&documentStatusId=" + c.InputDocumentStatusId;
						c.URL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/DocumentWorkflowTransition.aspx?workflowId=" + workflowId + "&workflowTransitionId=" + c.WorkflowTransitionId;
						c.OutURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/DocumentWorkflowStep.aspx?workflowId=" + workflowId + "&documentStatusId=" + c.OutputDocumentStatusId;
					}

					query = workflows.AsQueryable();

					List<DocumentStatusResponse> workflowTransitions = query.ToList();

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

		public List<DocumentStatusResponse> WorkflowTransition_RetrieveAllStatuses(int workflowId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.DocumentWorkflowTransitions.Include("InputDocumentStatus").Include("OutputDocumentStatus")
								join i in context.DocumentStati
								on t.InputDocumentStatusId equals i.DocumentStatusId
								join o in context.DocumentStati
								on t.OutputDocumentStatusId equals o.DocumentStatusId
								where t.OutputDocumentStatus.IsActive && t.DocumentWorkflowId == workflowId
								orderby i.Position, t.WorkflowTransitionId
								select new DocumentStatusResponse
								{
									InputDocumentStatusId = t.InputDocumentStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByEditor,
									IsExecuteByOwner = t.IsExecuteByAuthor,
									WorkflowTransitionId = t.WorkflowTransitionId,
									DocumentWorkflowId = t.DocumentWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
								};

					List<DocumentStatusResponse> workflowTransitions = query.ToList();

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
		/// Updates the workflow fields and custom properties associated with a particular document status
		/// </summary>
		/// <param name="status">The status whose fields and custom properties we want to update</param>
		public void Status_UpdateFieldsAndCustomProperties(DocumentStatus status)
        {
            const string METHOD_NAME = "Status_UpdateFieldsAndCustomProperties";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Attach the changed entity to the EF context and persist changes
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    context.DocumentStati.ApplyChanges(status);
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

        /// <summary>Retrieves the document status by the given ID.</summary>
        /// <param name="documentStatusId">The status ID to retrieve.</param>
        /// <returns>The DocumentStatus, or null if not found.</returns>
        /// <remarks>Will return deleted items.</remarks>
        /// <param name="includeWorkflowFieldsForWorkflowId">Should we include the linked workflow fields</param>
        public DocumentStatus Status_RetrieveById(int documentStatusId, int? includeWorkflowFieldsForWorkflowId = null)
        {
            const string METHOD_NAME = "Status_RetrieveById()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                DocumentStatus retStatus = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from r in context.DocumentStati
                                where r.DocumentStatusId == documentStatusId
                                select r;

                    retStatus = query.FirstOrDefault();

                    //Get the fields and custom properties, joined by EF 'fix-up'
                    if (includeWorkflowFieldsForWorkflowId.HasValue)
                    {
                        int workflowId = includeWorkflowFieldsForWorkflowId.Value;
                        var query2 = from w in context.DocumentWorkflowFields
                                     where w.DocumentStatusId == documentStatusId && w.DocumentWorkflowId == workflowId && w.ArtifactField.IsActive
                                     select w;
                        query2.ToList();
                        var query3 = from w in context.DocumentWorkflowCustomProperties
                                     where w.DocumentStatusId == documentStatusId && w.DocumentWorkflowId == workflowId && !w.CustomProperty.IsDeleted
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
        public DocumentWorkflow Workflow_GetDefault(int projectTemplateId)
		{
            const string METHOD_NAME = "Workflow_GetDefault";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from w in context.DocumentWorkflows
                                where w.IsActive && w.IsDefault && w.ProjectTemplateId == projectTemplateId
                                select w;

                    DocumentWorkflow workflow = query.FirstOrDefault();
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
		/// Gets the workflow for a specific document type
		/// </summary>
		/// <param name="documentTypeId">The document type we're interested in</param>
		/// <returns>The workflow id</returns>
        public int Workflow_GetForDocumentType(int documentTypeId)
		{
            const string METHOD_NAME = "Workflow_GetForDocumentType";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from r in context.DocumentTypes.Include(t => t.Workflow)
                                where r.DocumentTypeId == documentTypeId
                                select r;

                    DocumentType documentType = query.FirstOrDefault();
                    if (documentType == null || documentType.Workflow == null)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve workflow for document type " + documentTypeId + ".");
                    }
                    else
                    {
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return documentType.DocumentWorkflowId;
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
		/// Retrieves the list of inactive, hidden and required fields for an document based on its position
		/// in the workflow.
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="documentStatusId">The current step (i.e. document status) in the workflow</param>
        /// <remarks>Workflow needs to be active</remarks>
		public List<DocumentWorkflowField> Workflow_RetrieveFieldStates(int workflowId, int documentStatusId)
		{
            const string METHOD_NAME = "Workflow_RetrieveFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.DocumentWorkflowFields.Include("FieldState").Include("ArtifactField")
                                where w.DocumentWorkflowId == workflowId && w.Workflow.IsActive && w.ArtifactField.IsActive && w.DocumentStatusId == documentStatusId
                                orderby w.WorkflowFieldStateId, w.ArtifactFieldId
                                select w;

                    List<DocumentWorkflowField> workflowFields = query.ToList();

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
        /// Retrieves the list of inactive, hidden and required custom properties for an document based on its position
        /// in the workflow.
        /// </summary>
        /// <param name="workflowId">The workflow we're using</param>
        /// <param name="documentStatusId">The current step (i.e. status) in the workflow</param>
        /// <returns>List of inactive, hidden and required custom properties for the current workflow step</returns>
        /// <remarks>Workflow needs to be active</remarks>
        public List<DocumentWorkflowCustomProperty> Workflow_RetrieveCustomPropertyStates(int workflowId, int documentStatusId)
        {
            const string METHOD_NAME = "Workflow_RetrieveCustomPropertyStates";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.DocumentWorkflowCustomProperties.Include("FieldState").Include("CustomProperty")
                                where w.DocumentWorkflowId == workflowId &&
                                    w.Workflow.IsActive &&
                                    w.DocumentStatusId == documentStatusId &&
                                    !w.CustomProperty.IsDeleted
                                orderby w.WorkflowFieldStateId, w.CustomPropertyId
                                select w;

                    List<DocumentWorkflowCustomProperty> workflowCustomProps = query.ToList();

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
        /// Retrieves a workflow transition by its id, includes the transition roles and document status names
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="workflowTransitionId">The id of the transition</param>
        /// <returns></returns>
        public DocumentWorkflowTransition WorkflowTransition_RetrieveById(int workflowId, int workflowTransitionId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                DocumentWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.DocumentWorkflowTransitions.Include("InputDocumentStatus").Include("OutputDocumentStatus").Include("TransitionRoles").Include("TransitionRoles.Role")
                                where t.DocumentWorkflowId == workflowId && t.WorkflowTransitionId == workflowTransitionId
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
        public DocumentWorkflow Workflow_RetrieveById(int workflowId, bool includeTransitions = false, bool includeFieldStates = false)
        {
            const string METHOD_NAME = "Workflow_RetrieveById(int,[bool],[bool])";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See if we need to include transitions
                    ObjectQuery<DocumentWorkflow> workflowSet = context.DocumentWorkflows;
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
                                where w.DocumentWorkflowId == workflowId
                                select w;

                    DocumentWorkflow workflow = query.FirstOrDefault();
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
		/// Determines if a workflow is in use (i.e. associated with one or more document type)
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
                //Create the select command to retrieve the workflow used by the document type
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the document type
                    var query = from it in context.DocumentTypes
                                where it.DocumentWorkflowId == workflowId
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
        public List<DocumentWorkflow> Workflow_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
            const string METHOD_NAME = "Workflow_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<DocumentWorkflow> workflows;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflows
                    var query = from w in context.DocumentWorkflows
                                where (!activeOnly || w.IsActive) && w.ProjectTemplateId == projectTemplateId
                                select w;

                    //For the active case (i.e. a lookup) order by name, otherwise in the admin case, order by id
                    if (activeOnly)
                    {
                        query = query.OrderBy(w => w.Name).ThenBy(w => w.DocumentWorkflowId);
                    }
                    else
                    {
                        query = query.OrderBy(w => w.DocumentWorkflowId);
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
        /// Gets the list of workflow transitions for the specified document status where the user has a specific role or is the author/owner
        /// </summary>
        /// <param name="inputDocumentStatusId">The input status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>List of matching workflow transitions</returns>
        /// <param name="isAuthor">Are we the author of the document</param>
        /// <param name="isOwner">Are we the owner of the document</param>
        /// <param name="projectRoleId">What project role are we</param>
        /// <remarks>This overload considers user permissions/roles when returning the list</remarks>
        public List<DocumentWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputDocumentStatusId, int projectRoleId, bool isAuthor, bool isOwner)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.DocumentWorkflowTransitions
                                .Include(d => d.InputDocumentStatus)
                                .Include(d => d.OutputDocumentStatus)
                                where t.OutputDocumentStatus.IsActive && t.DocumentWorkflowId == workflowId && t.InputDocumentStatusId == inputDocumentStatusId
                                select t;

                    //Create the expresion if we're the author, owner or in one of the roles allowed to execute the transition
                    List<Expression<Func<DocumentWorkflowTransition, bool>>> expressionList = new List<Expression<Func<DocumentWorkflowTransition, bool>>>();
                    if (isAuthor)
                    {
                        Expression<Func<DocumentWorkflowTransition, bool>> clause = t => t.IsExecuteByAuthor;
                        expressionList.Add(clause);
                    }
                    if (isOwner)
                    {
                        Expression<Func<DocumentWorkflowTransition, bool>> clause = t => t.IsExecuteByEditor;
                        expressionList.Add(clause);
                    }

                    //Now the project role
                    Expression<Func<DocumentWorkflowTransition, bool>> clause2 = t => t.TransitionRoles.Any(i => i.Role.ProjectRoleId == projectRoleId && i.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);
                    expressionList.Add(clause2);

                    if (expressionList.Count > 0)
                    {
                        //OR together the different cases and apply to the where clause
                        Expression<Func<DocumentWorkflowTransition, bool>> aggregatedClause = Manager.BuildOr(expressionList.ToArray());
                        query = query.Where(aggregatedClause);
                    }

                    //Add the sorts
                    query = query.OrderBy(t => t.Name).ThenBy(t => t.WorkflowTransitionId);

                    List<DocumentWorkflowTransition> workflowTransitions = query.ToList();

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
        /// <param name="inputDocumentStatusId">The input status</param>
        /// <param name="outputDocumentStatusId">The output status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>The workflow transition</returns>
        public DocumentWorkflowTransition WorkflowTransition_RetrieveByStatuses(int workflowId, int inputDocumentStatusId, int outputDocumentStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.DocumentWorkflowTransitions.Include("InputDocumentStatus").Include("OutputDocumentStatus")
                                where t.DocumentWorkflowId == workflowId &&
                                      t.InputDocumentStatusId == inputDocumentStatusId &&
                                      t.OutputDocumentStatusId == outputDocumentStatusId
                                select t;

                    DocumentWorkflowTransition workflowTransition = query.FirstOrDefault();

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
        /// Gets the list of workflow transitions for the specified document status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="inputDocumentStatusId">The id of the input status</param>
        /// <returns>List of transitions</returns>
        public List<DocumentWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputDocumentStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.DocumentWorkflowTransitions.Include("InputDocumentStatus").Include("OutputDocumentStatus")
                                where t.OutputDocumentStatus.IsActive && t.DocumentWorkflowId == workflowId && t.InputDocumentStatusId == inputDocumentStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<DocumentWorkflowTransition> workflowTransitions = query.ToList();

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
        /// Gets the list of workflow transitions for the specified document output status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="outputDocumentStatusId">The id of the output status</param>
        /// <returns>List of transitions</returns>
        public List<DocumentWorkflowTransition> WorkflowTransition_RetrieveByOutputStatus(int workflowId, int outputDocumentStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByOutputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.DocumentWorkflowTransitions.Include("InputDocumentStatus").Include("OutputDocumentStatus")
                                where t.OutputDocumentStatus.IsActive && t.DocumentWorkflowId == workflowId && t.OutputDocumentStatusId == outputDocumentStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<DocumentWorkflowTransition> workflowTransitions = query.ToList();

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
        public void Workflow_Update(DocumentWorkflow workflow)
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
            if (!workflow.IsActive && IsInUse(workflow.DocumentWorkflowId))
            {
                throw new WorkflowInUseException("You cannot make a workflow that is in use inactive");
            }

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.DocumentWorkflows.ApplyChanges(workflow);
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
        public DocumentWorkflow Workflow_Insert(int projectTemplateId, string name, bool isDefault, bool isActive = true)
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
                DocumentWorkflow workflow;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow entity
                    workflow = new DocumentWorkflow();
                    workflow.ProjectTemplateId = projectTemplateId;
                    workflow.Name = name;
                    workflow.IsActive = isActive;
                    workflow.IsDefault = isDefault;

                    //Persist the new workflow
                    context.DocumentWorkflows.AddObject(workflow);
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
        /// Inserts a new workflow into the system with all the standard transitions and field states
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="name">The name of the workflow</param>
        /// <param name="isDefault">Is this the default workflow for the project</param>
        /// <param name="isActive">Is this workflow active</param>
        /// <returns>The newly inserted workflow</returns>
        public DocumentWorkflow Workflow_InsertWithDefaultEntries(int projectTemplateId, string name, bool isDefault, bool isActive = true)
        {
            const string METHOD_NAME = "Workflow_InsertWithDefaultEntries";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First create the workflow itself
                DocumentWorkflow workflow = this.Workflow_Insert(projectTemplateId, name, isDefault, isActive);
                int workflowId = workflow.DocumentWorkflowId;

                //For reference get the statuses for the project template
                AttachmentManager attachmentManager = new AttachmentManager();
                List<DocumentStatus> statuses = attachmentManager.DocumentStatus_Retrieve(projectTemplateId, false, false);
                int statusIdDraft = statuses.FirstOrDefault(c => c.Name == "Draft").DocumentStatusId;
                int statusIdUnderReview = statuses.FirstOrDefault(c => c.Name == "Under Review").DocumentStatusId;
                int statusIdApproved = statuses.FirstOrDefault(c => c.Name == "Approved").DocumentStatusId;
                int statusIdCompleted = statuses.FirstOrDefault(c => c.Name == "Completed").DocumentStatusId;
                int statusIdRejected = statuses.FirstOrDefault(c => c.Name == "Rejected").DocumentStatusId;
                int statusIdRetired = statuses.FirstOrDefault(c => c.Name == "Retired").DocumentStatusId;
                int statusIdCheckedOut = statuses.FirstOrDefault(c => c.Name == "Checked Out").DocumentStatusId;

                //Workflow Transitions (inc. roles)
                int workflowTransitionId1 = WorkflowTransition_Insert(workflowId, statusIdDraft, statusIdUnderReview, "Review Document", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                int workflowTransitionId2 = WorkflowTransition_Insert(workflowId, statusIdUnderReview, statusIdApproved, "Approve Document", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId3 = WorkflowTransition_Insert(workflowId, statusIdUnderReview, statusIdRejected, "Reject Document", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId4 = WorkflowTransition_Insert(workflowId, statusIdUnderReview, statusIdDraft, "Return to Draft", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                int workflowTransitionId5 = WorkflowTransition_Insert(workflowId, statusIdApproved, statusIdCheckedOut, "Checkout", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner }).WorkflowTransitionId;
                int workflowTransitionId6 = WorkflowTransition_Insert(workflowId, statusIdApproved, statusIdCompleted, "Complete Document", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId7 = WorkflowTransition_Insert(workflowId, statusIdApproved, statusIdUnderReview, "Return to Draft", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                int workflowTransitionId8 = WorkflowTransition_Insert(workflowId, statusIdCompleted, statusIdRetired, "Retire Document", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId9 = WorkflowTransition_Insert(workflowId, statusIdCompleted, statusIdUnderReview, "Return to Review", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                int workflowTransitionId10 = WorkflowTransition_Insert(workflowId, statusIdRejected, statusIdUnderReview, "Return to Review", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                int workflowTransitionId11 = WorkflowTransition_Insert(workflowId, statusIdRetired, statusIdUnderReview, "Return to Review", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                int workflowTransitionId12 = WorkflowTransition_Insert(workflowId, statusIdCheckedOut, statusIdApproved, "Checkin", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner }).WorkflowTransitionId;


                //Workflow Fields

                //All fields are active, visible and not-required by default, so only need to populate the ones
                //that are exceptions to that case

                //Draft - Required: AuthorId, Type, DocumentName
                DocumentStatus documentStatus = Status_RetrieveById(statusIdDraft, workflowId);
                documentStatus.StartTracking();
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 154, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 150, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 149, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                Status_UpdateFieldsAndCustomProperties(documentStatus);

                //UnderReview - Required: AuthorId, Type, DocumentName, EditedBy
                documentStatus = Status_RetrieveById(statusIdUnderReview, workflowId);
                documentStatus.StartTracking();
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 154, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 150, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 149, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 152, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                Status_UpdateFieldsAndCustomProperties(documentStatus);

                //Approved - Required: AuthorId, Type, Versions, DocumentName, EditedBy, Comments
                documentStatus = Status_RetrieveById(statusIdApproved, workflowId);
                documentStatus.StartTracking();
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 154, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 150, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 149, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 152, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 180, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                Status_UpdateFieldsAndCustomProperties(documentStatus);

                //Completed - Disabled: AuthorId, Type, Versions, DocumentName, EditedBy, Description, Tags
                documentStatus = Status_RetrieveById(statusIdCompleted, workflowId);
                documentStatus.StartTracking();
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 154, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 150, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 149, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 152, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 161, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 159, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(documentStatus);

                //Rejected - Required: AuthorId, Type, DocumentName; Hidden: EditedBy
                documentStatus = Status_RetrieveById(statusIdRejected, workflowId);
                documentStatus.StartTracking();
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 154, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 150, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 149, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 152, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
                Status_UpdateFieldsAndCustomProperties(documentStatus);

                //Retired - Disabled: ALL STANDARD FIELDS (AuthorId, Type, Versions, DocumentName, EditedBy, Description,  Tags, Comments)
                documentStatus = Status_RetrieveById(statusIdRetired, workflowId);
                documentStatus.StartTracking();
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 154, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 150, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 149, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 152, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 161, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 159, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 180, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 179, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(documentStatus);

                //CheckedOut - Disabled: AuthorId, Type, Versions, DocumentName, EditedBy, Description,  Tags
                documentStatus = Status_RetrieveById(statusIdCheckedOut, workflowId);
                documentStatus.StartTracking();
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 154, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 150, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 149, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 152, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 161, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 159, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                documentStatus.WorkflowFields.Add(new DocumentWorkflowField() { DocumentWorkflowId = workflowId, ArtifactFieldId = 179, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(documentStatus);

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
        /// Inserts a new workflow transition into the system
        /// </summary>
        /// <param name="workflowId">The workflow the transition is to be associated with</param>
        /// <param name="inputDocumentStatusId">The input document status for the transition</param>
        /// <param name="outputDocumentStatusId">The output document status for the transition</param>
        /// <param name="name">The name of the transition</param>
        /// <param name="executeByCreator"></param>
        /// <param name="executeByOwner"></param>
        /// <param name="roles">The list of any roles that can also execute the transition</param>
        /// <returns>The new workflow transition</returns>
        public DocumentWorkflowTransition WorkflowTransition_Insert(int workflowId, int inputDocumentStatusId, int outputDocumentStatusId, string name, bool executeByCreator = false, bool executeByOwner = true, List<int> roles = null)
        {
            const string METHOD_NAME = "Workflow_Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                DocumentWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow transition entity
                    workflowTransition = new DocumentWorkflowTransition();
                    workflowTransition.Name = name;
                    workflowTransition.DocumentWorkflowId = workflowId;
                    workflowTransition.InputDocumentStatusId = inputDocumentStatusId;
                    workflowTransition.OutputDocumentStatusId = outputDocumentStatusId;
                    workflowTransition.IsExecuteByEditor = executeByOwner;
                    workflowTransition.IsExecuteByAuthor = executeByCreator;

                    //Add the transition roles if any provided
                    if (roles != null && roles.Count > 0)
                    {
                        foreach (int projectRoleId in roles)
                        {
                            //For document workflows, all transition roles are execute transitions (not notify)
                            DocumentWorkflowTransitionRole transitionRole = new DocumentWorkflowTransitionRole();
                            transitionRole.ProjectRoleId = projectRoleId;
                            transitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
                            workflowTransition.TransitionRoles.Add(transitionRole);
                        }
                    }

                    //Persist the new workflow transition
                    context.DocumentWorkflowTransitions.AddObject(workflowTransition);
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
                    var query = from wt in context.DocumentWorkflowTransitions.Include("TransitionRoles")
                                where wt.WorkflowTransitionId == workflowTransitionId
                                select wt;

                    DocumentWorkflowTransition workflowTransition = query.FirstOrDefault();

                    //Make sure we have a workflow transition
                    if (workflowTransition != null)
                    {
                        //Delete the workflow transition and dependent entities
                        context.DocumentWorkflowTransitions.DeleteObject(workflowTransition);
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
        public void WorkflowTransition_Update(DocumentWorkflowTransition workflowTransition)
        {
            const string METHOD_NAME = CLASS_NAME + "WorkflowTransition_Update";

            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.DocumentWorkflowTransitions.ApplyChanges(workflowTransition);
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
        /// <param name="documentWorkflowMapping">The mapping of workflow ids between the old and new templates</param>
        /// <param name="sourceProjectTemplateId">The project template to copy the workflows from</param>
        /// <param name="destProjectTemplateId">The destination project template</param>
        /// <param name="customPropertyIdMapping">The mapping of old vs. new custom properties between the two project templates</param>
        /// <param name="documentStatusMapping">The document status mapping</param>
        public void Workflow_Copy(int sourceProjectTemplateId, int destProjectTemplateId, Dictionary<int, int> customPropertyIdMapping, Dictionary<int, int> documentWorkflowMapping, Dictionary<int, int> documentStatusMapping)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Dictionary<int, DocumentWorkflow> tempMapping = new Dictionary<int, DocumentWorkflow>();
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflows and associated transitions, fields document types, and custom properties
                    var query = from w in context.DocumentWorkflows
                                    .Include("Transitions")
                                    .Include("Transitions.TransitionRoles")
                                    .Include("Fields")
                                    .Include("CustomProperties")
                                    .Include("DocumentTypes")
                                where w.ProjectTemplateId == sourceProjectTemplateId
                                orderby w.DocumentWorkflowId
                                select w;

                    List<DocumentWorkflow> sourceWorkflows = query.ToList();

                    //Loop through the various workflows
                    foreach (DocumentWorkflow sourceWorkflow in sourceWorkflows)
                    {
                        //Create the copied workflow
                        DocumentWorkflow destWorkflow = new DocumentWorkflow();
                        destWorkflow.ProjectTemplateId = destProjectTemplateId;
                        destWorkflow.Name = sourceWorkflow.Name;
                        destWorkflow.IsActive = sourceWorkflow.IsActive;
                        destWorkflow.IsDefault = sourceWorkflow.IsDefault;

                        //Add to mapping
                        tempMapping.Add(sourceWorkflow.DocumentWorkflowId, destWorkflow);

                        //Now add the transitions (including the transition roles)
                        for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                        {
                            //Lookup the mapped statuses
                            if (documentStatusMapping.ContainsKey(sourceWorkflow.Transitions[i].InputDocumentStatusId))
                            {
                                int inputDocumentStatusId = documentStatusMapping[sourceWorkflow.Transitions[i].InputDocumentStatusId];
                                if (documentStatusMapping.ContainsKey(sourceWorkflow.Transitions[i].OutputDocumentStatusId))
                                {
                                    int outputDocumentStatusId = documentStatusMapping[sourceWorkflow.Transitions[i].OutputDocumentStatusId];

                                    DocumentWorkflowTransition workflowTransition = new DocumentWorkflowTransition();
                                    workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                                    workflowTransition.InputDocumentStatusId = inputDocumentStatusId;
                                    workflowTransition.OutputDocumentStatusId = outputDocumentStatusId;
                                    workflowTransition.IsExecuteByAuthor = sourceWorkflow.Transitions[i].IsExecuteByAuthor;
                                    workflowTransition.IsExecuteByEditor = sourceWorkflow.Transitions[i].IsExecuteByEditor;
                                    workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                                    destWorkflow.Transitions.Add(workflowTransition);

                                    //Now the roles
                                    for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                                    {
                                        DocumentWorkflowTransitionRole workflowTransitionRole = new DocumentWorkflowTransitionRole();
                                        workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                                        workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                                        workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                                    }
                                }
                            }
                        }

                        //Now add the fields
                        for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                        {
                            //Get the mapped document status
                            int oldDocumentStatusId = sourceWorkflow.Fields[i].DocumentStatusId;
                            if (documentStatusMapping.ContainsKey(oldDocumentStatusId))
                            {
                                int newDocumentStatusId = (int)documentStatusMapping[oldDocumentStatusId];

                                DocumentWorkflowField workflowField = new DocumentWorkflowField();
                                workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                                workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                                workflowField.DocumentStatusId = newDocumentStatusId;
                                destWorkflow.Fields.Add(workflowField);
                            }
                        }

                        //Now add the custom properties
                        for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                        {
                            //Get the corresponding custom property in the new project
                            if (customPropertyIdMapping.ContainsKey(sourceWorkflow.CustomProperties[i].CustomPropertyId))
                            {
                                int destCustomPropertyId = customPropertyIdMapping[sourceWorkflow.CustomProperties[i].CustomPropertyId];
                                DocumentWorkflowCustomProperty workflowCustomProperty = new DocumentWorkflowCustomProperty();
                                workflowCustomProperty.CustomPropertyId = destCustomPropertyId;
                                workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                                workflowCustomProperty.DocumentStatusId = sourceWorkflow.CustomProperties[i].DocumentStatusId;
                                destWorkflow.CustomProperties.Add(workflowCustomProperty);
                            }
                        }

                        //Add the new workflow
                        context.DocumentWorkflows.AddObject(destWorkflow);
                    }

                    //Save the changes
                    context.SaveChanges();
                }

                //Finally populate the external mapping dictionary (id based)
                foreach (KeyValuePair<int, DocumentWorkflow> kvp in tempMapping)
                {
                    documentWorkflowMapping.Add(kvp.Key, kvp.Value.DocumentWorkflowId);
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
        public DocumentWorkflow Workflow_Copy(int sourceWorkflowId)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                DocumentWorkflow destWorkflow = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow and associated transitions, fields and custom properties
                    var query = from w in context.DocumentWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties")
                                where w.DocumentWorkflowId == sourceWorkflowId
                                select w;

                    DocumentWorkflow sourceWorkflow = query.FirstOrDefault();
                    if (sourceWorkflow == null)
                    {
                        throw new ArtifactNotExistsException("The workflow being copied no longer exists");
                    }

                    //Create the copied workflow
                    //except that we will always insert it with default=No
                    destWorkflow = new DocumentWorkflow();
                    destWorkflow.ProjectTemplateId = sourceWorkflow.ProjectTemplateId;
                    destWorkflow.Name = GlobalResources.General.Global_CopyOf + " " + sourceWorkflow.Name;
                    destWorkflow.IsActive = sourceWorkflow.IsActive;
                    destWorkflow.IsDefault = false;

                    //Now add the transitions (including the transition roles)
                    for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                    {
                        DocumentWorkflowTransition workflowTransition = new DocumentWorkflowTransition();
                        workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                        workflowTransition.InputDocumentStatusId = sourceWorkflow.Transitions[i].InputDocumentStatusId;
                        workflowTransition.OutputDocumentStatusId = sourceWorkflow.Transitions[i].OutputDocumentStatusId;
                        workflowTransition.IsExecuteByAuthor = sourceWorkflow.Transitions[i].IsExecuteByAuthor;
                        workflowTransition.IsExecuteByEditor = sourceWorkflow.Transitions[i].IsExecuteByEditor;
                        workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                        destWorkflow.Transitions.Add(workflowTransition);

                        //Now the roles
                        for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                        {
                            DocumentWorkflowTransitionRole workflowTransitionRole = new DocumentWorkflowTransitionRole();
                            workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                            workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                            workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                        }
                    }

                    //Now add the fields
                    for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                    {
                        DocumentWorkflowField workflowField = new DocumentWorkflowField();
                        workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                        workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                        workflowField.DocumentStatusId = sourceWorkflow.Fields[i].DocumentStatusId;
                        destWorkflow.Fields.Add(workflowField);
                    }

                    //Now add the custom properties
                    for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                    {
                        DocumentWorkflowCustomProperty workflowCustomProperty = new DocumentWorkflowCustomProperty();
                        workflowCustomProperty.CustomPropertyId = sourceWorkflow.CustomProperties[i].CustomPropertyId;
                        workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                        workflowCustomProperty.DocumentStatusId = sourceWorkflow.CustomProperties[i].DocumentStatusId;
                        destWorkflow.CustomProperties.Add(workflowCustomProperty);
                    }

                    //Save the new workflow
                    context.DocumentWorkflows.AddObject(destWorkflow);
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
        /// <remarks>The associated document types need to have been deleted first</remarks>
        protected internal void Workflow_DeleteAllForProjectTemplate(int projectTemplateId)
        {
            const string METHOD_NAME = "Workflow_DeleteAllForProjectTemplate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow
                    var query = from w in context.DocumentWorkflows
                                where w.ProjectTemplateId == projectTemplateId
                                orderby w.DocumentWorkflowId
                                select w;

                    List<DocumentWorkflow> workflows = query.ToList();
                    for (int i = 0; i < workflows.Count; i++)
                    {
                        context.DocumentWorkflows.DeleteObject(workflows[i]);
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
                    var query = from w in context.DocumentWorkflows
                                where w.DocumentWorkflowId == workflowId
                                select w;

                    DocumentWorkflow workflow = query.FirstOrDefault();

                    //Make sure we have a workflow
                    if (workflow != null)
                    {
                        //If the workflow is marked as default, you can't delete it
                        if (workflow.IsDefault)
                        {
                            throw new WorkflowInUseException("You cannot delete a workflow that is marked as the default");
                        }

                        //Delete the workflow. The database cascades will handle the dependent entities
                        context.DocumentWorkflows.DeleteObject(workflow);
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
