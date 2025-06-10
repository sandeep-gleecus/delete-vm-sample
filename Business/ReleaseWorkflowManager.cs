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
    /// the release workflows that are used to determine the lifecycle of releases in the system
    /// </summary>
    public class ReleaseWorkflowManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ReleaseWorkflowManager::";

        #region Static Methods

        /// <summary>
        /// Converts release workflow field collection to the generic kind
        /// </summary>
        /// <param name="releaseWorkflowCustomProps">The release workflow fields</param>
        /// <returns>The workflow fields</returns>
        public static List<WorkflowField> ConvertFields(List<ReleaseWorkflowField> releaseWorkflowFields)
        {
            //Handle null case safely
            if (releaseWorkflowFields == null)
            {
                return null;
            }

            List<WorkflowField> workflowFields = new List<WorkflowField>();
            foreach (ReleaseWorkflowField releaseWorkflowField in releaseWorkflowFields)
            {
                WorkflowField workflowField = new WorkflowField();
                workflowField.ArtifactFieldId = releaseWorkflowField.ArtifactFieldId;
                workflowField.IncidentStatusId = releaseWorkflowField.ReleaseStatusId;
                workflowField.WorkflowId = releaseWorkflowField.ReleaseWorkflowId;
                workflowField.WorkflowFieldStateId = releaseWorkflowField.WorkflowFieldStateId;
                workflowField.Field = new ArtifactField();
                workflowField.Field.Name = releaseWorkflowField.ArtifactField.Name;
                workflowFields.Add(workflowField);
            }

            return workflowFields;
        }

        /// <summary>
        /// Converts release workflow custom properties collection to the generic kind
        /// </summary>
        /// <param name="releaseWorkflowCustomProps">The release workflow custom properties</param>
        /// <returns>The workflow custom properties</returns>
        public static List<WorkflowCustomProperty> ConvertFields(List<ReleaseWorkflowCustomProperty> releaseWorkflowCustomProps)
        {
            //Handle null case safely
            if (releaseWorkflowCustomProps == null)
            {
                return null;
            }

            List<WorkflowCustomProperty> workflowCustomProperties = new List<WorkflowCustomProperty>();
            foreach (ReleaseWorkflowCustomProperty releaseWorkflowCustomProp in releaseWorkflowCustomProps)
            {
                WorkflowCustomProperty workflowCustomProperty = new WorkflowCustomProperty();
                workflowCustomProperty.CustomPropertyId = releaseWorkflowCustomProp.CustomPropertyId;
                workflowCustomProperty.IncidentStatusId = releaseWorkflowCustomProp.ReleaseStatusId;
                workflowCustomProperty.WorkflowId = releaseWorkflowCustomProp.ReleaseWorkflowId;
                workflowCustomProperty.WorkflowFieldStateId = releaseWorkflowCustomProp.WorkflowFieldStateId;
                workflowCustomProperty.CustomProperty = new CustomProperty();
                workflowCustomProperty.CustomProperty.PropertyNumber = releaseWorkflowCustomProp.CustomProperty.PropertyNumber;
                workflowCustomProperties.Add(workflowCustomProperty);
            }

            return workflowCustomProperties;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Creates the default workflow, transitions and field states for a new project using the default template
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        internal void Workflow_CreateDefaultEntriesForProjectTemplate(int projectTemplateId)
        {
            const string METHOD_NAME = "Workflow_CreateDefaultEntriesForProjectTemplate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to create a default workflow for a project
                int workflowId = Workflow_Insert(projectTemplateId, GlobalResources.General.Workflow_DefaultWorflow, true).ReleaseWorkflowId;

                //Next we need to associate this with all the types for this project
                Workflow_AssociateWithReleaseType(projectTemplateId, Release.ReleaseTypeEnum.MajorRelease, workflowId);
                Workflow_AssociateWithReleaseType(projectTemplateId, Release.ReleaseTypeEnum.MinorRelease, workflowId);
                Workflow_AssociateWithReleaseType(projectTemplateId, Release.ReleaseTypeEnum.Iteration, workflowId);
                Workflow_AssociateWithReleaseType(projectTemplateId, Release.ReleaseTypeEnum.Phase, workflowId);

                //Workflow Transitions (inc. roles)
                int workflowTransitionId1 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Planned, (int)Release.ReleaseStatusEnum.InProgress, "Start Release", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId2 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.InProgress, (int)Release.ReleaseStatusEnum.Completed, "Finish Release", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId3 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Completed, (int)Release.ReleaseStatusEnum.Closed, "Close Release", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId4 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Planned, (int)Release.ReleaseStatusEnum.Deferred, "Defer Release", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId5 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Planned, (int)Release.ReleaseStatusEnum.Cancelled, "Cancel Release", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId6 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.InProgress, (int)Release.ReleaseStatusEnum.Deferred, "Defer Release", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId7 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.InProgress, (int)Release.ReleaseStatusEnum.Cancelled, "Cancel Release", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId8 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Completed, (int)Release.ReleaseStatusEnum.InProgress, "Continue Release", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId9 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Closed, (int)Release.ReleaseStatusEnum.Completed, "Open Release", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId10 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Deferred, (int)Release.ReleaseStatusEnum.InProgress, "Continue Release", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId11 = WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Cancelled, (int)Release.ReleaseStatusEnum.Planned, "Uncancel Release", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                //Workflow Fields

                //All fields are active, visible and not-required by default, so only need to populate the ones
                //that are exceptions to that case

                //Completed
                ReleaseStatus releaseStatus = Status_RetrieveById((int)Release.ReleaseStatusEnum.Completed, workflowId);
                releaseStatus.StartTracking();
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 30, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 31, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 80, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 81, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 82, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 83, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 99, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 107, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 164, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(releaseStatus);

                //Closed
                releaseStatus = Status_RetrieveById((int)Release.ReleaseStatusEnum.Closed, workflowId);
                releaseStatus.StartTracking();
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 30, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 31, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 80, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 81, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 82, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 83, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 99, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 107, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 164, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(releaseStatus);

                //Cancelled
                releaseStatus = Status_RetrieveById((int)Release.ReleaseStatusEnum.Cancelled, workflowId);
                releaseStatus.StartTracking();
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 30, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 31, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 80, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 81, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 82, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 83, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 99, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 107, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                releaseStatus.WorkflowFields.Add(new ReleaseWorkflowField() { ReleaseWorkflowId = workflowId, ArtifactFieldId = 164, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(releaseStatus);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the workflow fields and custom properties associated with a particular release status
        /// </summary>
        /// <param name="status">The status whose fields and custom properties we want to update</param>
        public void Status_UpdateFieldsAndCustomProperties(ReleaseStatus status)
        {
            const string METHOD_NAME = "Status_UpdateFieldsAndCustomProperties";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Attach the changed entity to the EF context and persist changes
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    context.ReleaseStati.ApplyChanges(status);
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

		public List<ReleaseStatusResponse> WorkflowTransition_RetrieveByInputStatusById(int workflowId, int inputReleaseStatusId, int projectTemplateId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatusById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.ReleaseWorkflowTransitions.Include("InputReleaseStatus").Include("OutputReleaseStatus")
								join i in context.ReleaseStati
								on t.InputReleaseStatusId equals i.ReleaseStatusId
								join o in context.ReleaseStati
								on t.OutputReleaseStatusId equals o.ReleaseStatusId
								where t.OutputReleaseStatus.IsActive && t.ReleaseWorkflowId == workflowId && t.InputReleaseStatusId == inputReleaseStatusId
								orderby t.Name, t.WorkflowTransitionId
								select new ReleaseStatusResponse
								{
									InputReleaseStatusId = t.InputReleaseStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									ReleaseWorkflowId = t.ReleaseWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									URL = "",
									OutURL = "",
									InURL = "",
									OutputReleaseStatusId = t.OutputReleaseStatusId,
								};

					var workflows = query.ToList();

					foreach (var c in workflows)
					{
						c.InURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/ReleaseWorkflowStep.aspx?workflowId=" + workflowId + "&releaseStatusId=" + c.InputReleaseStatusId;
						c.URL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/ReleaseWorkflowTransition.aspx?workflowId=" + workflowId + "&workflowTransitionId=" + c.WorkflowTransitionId;
						c.OutURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/ReleaseWorkflowStep.aspx?workflowId=" + workflowId + "&releaseStatusId=" + c.OutputReleaseStatusId;
					}

					query = workflows.AsQueryable();

					List<ReleaseStatusResponse> workflowTransitions = query.ToList();

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

		public List<ReleaseStatusResponse> WorkflowTransition_RetrieveAllStatuses(int workflowId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.ReleaseWorkflowTransitions.Include("InputReleaseStatus").Include("OutputReleaseStatus")
								join i in context.ReleaseStati
								on t.InputReleaseStatusId equals i.ReleaseStatusId
								join o in context.ReleaseStati
								on t.OutputReleaseStatusId equals o.ReleaseStatusId
								where t.OutputReleaseStatus.IsActive && t.ReleaseWorkflowId == workflowId
								orderby i.Position, t.WorkflowTransitionId
								select new ReleaseStatusResponse
								{
									InputReleaseStatusId = t.InputReleaseStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									ReleaseWorkflowId = t.ReleaseWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
								};

					List<ReleaseStatusResponse> workflowTransitions = query.ToList();

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


		/// <summary>Retrieves the release status by the given ID.</summary>
		/// <param name="releaseStatusId">The status ID to retrieve.</param>
		/// <returns>The ReleaseStatus, or null if not found.</returns>
		/// <remarks>Will return deleted items.</remarks>
		/// <param name="includeWorkflowFieldsForWorkflowId">Should we include the linked workflow fields</param>
		public ReleaseStatus Status_RetrieveById(int releaseStatusId, int? includeWorkflowFieldsForWorkflowId = null)
        {
            const string METHOD_NAME = "Status_RetrieveById()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                ReleaseStatus retStatus = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from r in context.ReleaseStati
                                where r.ReleaseStatusId == releaseStatusId
                                select r;

                    retStatus = query.FirstOrDefault();

                    //Get the fields and custom properties, joined by EF 'fix-up'
                    if (includeWorkflowFieldsForWorkflowId.HasValue)
                    {
                        int workflowId = includeWorkflowFieldsForWorkflowId.Value;
                        var query2 = from w in context.ReleaseWorkflowFields
                                     where w.ReleaseStatusId == releaseStatusId && w.ReleaseWorkflowId == workflowId && w.ArtifactField.IsActive
                                     select w;
                        query2.ToList();
                        var query3 = from w in context.ReleaseWorkflowCustomProperties
                                     where w.ReleaseStatusId == releaseStatusId && w.ReleaseWorkflowId == workflowId && !w.CustomProperty.IsDeleted
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
        /// Create a new mapping between a release type and a workflow, for a specific project
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="releaseType">The type of release</param>
        /// <param name="workflowId">The workflow</param>
        public void Workflow_AssociateWithReleaseType(int projectTemplateId, Release.ReleaseTypeEnum releaseType, int workflowId)
        {
            const string METHOD_NAME = "Workflow_AssociateWithReleaseType";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //We need to associate this with all the types for this project template
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First we need to see if this type is associated with some other workflow
                    var query = from r in context.ReleaseTypeWorkflows
                                where r.ReleaseTypeId == (int)releaseType && r.ProjectTemplateId == projectTemplateId
                                select r;

                    ReleaseTypeWorkflow releaseTypeWorkflow = query.FirstOrDefault();
                    if (releaseTypeWorkflow == null)
                    {
                        //Create the new mapping object
                        releaseTypeWorkflow = new ReleaseTypeWorkflow();
                        releaseTypeWorkflow.ProjectTemplateId = projectTemplateId;
                        releaseTypeWorkflow.ReleaseTypeId = (int)releaseType;
                        releaseTypeWorkflow.ReleaseWorkflowId = workflowId;

                        //Persist the new object
                        context.ReleaseTypeWorkflows.AddObject(releaseTypeWorkflow);
                        context.SaveChanges();
                    }
                    else
                    {
                        //If the workflow has changed, update the mapping
                        if (releaseTypeWorkflow.ReleaseWorkflowId != workflowId)
                        {
                            //First remove the old mapping
                            releaseTypeWorkflow.StartTracking();
                            context.ReleaseTypeWorkflows.DeleteObject(releaseTypeWorkflow);
                            context.SaveChanges();

                            //Create the new mapping object
                            releaseTypeWorkflow = new ReleaseTypeWorkflow();
                            releaseTypeWorkflow.ProjectTemplateId = projectTemplateId;
                            releaseTypeWorkflow.ReleaseTypeId = (int)releaseType;
                            releaseTypeWorkflow.ReleaseWorkflowId = workflowId;

                            //Persist the new object
                            context.ReleaseTypeWorkflows.AddObject(releaseTypeWorkflow);
                            context.SaveChanges();
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
		/// Gets the default workflow for a specific project template
		/// </summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <returns>The default workflow</returns>
        public ReleaseWorkflow Workflow_GetDefault(int projectTemplateId)
		{
            const string METHOD_NAME = "Workflow_GetDefault";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from w in context.ReleaseWorkflows
                                where w.IsActive && w.IsDefault && w.ProjectTemplateId == projectTemplateId
                                select w;

                    ReleaseWorkflow workflow = query.FirstOrDefault();
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
		/// Gets the workflow for a specific release type
		/// </summary>
        /// <param name="projectTemplateId">The id of the current project template</param>
		/// <param name="releaseTypeId">The release type we're interested in</param>
		/// <returns>The workflow id</returns>
        public int Workflow_GetForReleaseType(int projectTemplateId, int releaseTypeId)
		{
            const string METHOD_NAME = "Workflow_GetForReleaseType";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from r in context.ReleaseTypeWorkflows
                                where r.ProjectTemplateId == projectTemplateId && r.ReleaseTypeId == releaseTypeId
                                select r;

                    ReleaseTypeWorkflow releaseTypeWorkflow = query.FirstOrDefault();
                    if (releaseTypeWorkflow == null)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve workflow for release type " + releaseTypeId + " in project PT" + projectTemplateId + ".");
                    }
                    else
                    {
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return releaseTypeWorkflow.ReleaseWorkflowId;
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
		/// Retrieves the list of inactive, hidden and required fields for an release based on its position
		/// in the workflow.
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="releaseStatusId">The current step (i.e. release status) in the workflow</param>
        /// <remarks>Workflow needs to be active</remarks>
		public List<ReleaseWorkflowField> Workflow_RetrieveFieldStates(int workflowId, int releaseStatusId)
		{
            const string METHOD_NAME = "Workflow_RetrieveFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.ReleaseWorkflowFields.Include("FieldState").Include("ArtifactField")
                                where w.ReleaseWorkflowId == workflowId && w.Workflow.IsActive && w.ArtifactField.IsActive && w.ReleaseStatusId == releaseStatusId
                                orderby w.WorkflowFieldStateId, w.ArtifactFieldId
                                select w;

                    List<ReleaseWorkflowField> workflowFields = query.ToList();
                    
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
        /// Retrieves the list of inactive, hidden and required custom properties for an release based on its position
        /// in the workflow.
        /// </summary>
        /// <param name="workflowId">The workflow we're using</param>
        /// <param name="releaseStatusId">The current step (i.e. status) in the workflow</param>
        /// <returns>List of inactive, hidden and required custom properties for the current workflow step</returns>
        /// <remarks>Workflow needs to be active</remarks>
        public List<ReleaseWorkflowCustomProperty> Workflow_RetrieveCustomPropertyStates(int workflowId, int releaseStatusId)
        {
            const string METHOD_NAME = "Workflow_RetrieveCustomPropertyStates";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.ReleaseWorkflowCustomProperties.Include("FieldState").Include("CustomProperty")
                                where w.ReleaseWorkflowId == workflowId &&
                                    w.Workflow.IsActive &&
                                    w.ReleaseStatusId == releaseStatusId &&
                                    !w.CustomProperty.IsDeleted
                                orderby w.WorkflowFieldStateId, w.CustomPropertyId
                                select w;

                    List<ReleaseWorkflowCustomProperty> workflowCustomProps = query.ToList();

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
        /// Retrieves a workflow transition by its id, includes the transition roles and release status names
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="workflowTransitionId">The id of the transition</param>
        /// <returns></returns>
        public ReleaseWorkflowTransition WorkflowTransition_RetrieveById(int workflowId, int workflowTransitionId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                ReleaseWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.ReleaseWorkflowTransitions.Include("InputReleaseStatus").Include("OutputReleaseStatus").Include("TransitionRoles").Include("TransitionRoles.Role")
                                where t.ReleaseWorkflowId == workflowId && t.WorkflowTransitionId == workflowTransitionId
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
        public ReleaseWorkflow Workflow_RetrieveById(int workflowId, bool includeTransitions = false, bool includeFieldStates = false)
        {
            const string METHOD_NAME = "Workflow_RetrieveById(int,[bool],[bool])";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See if we need to include transitions
                    ObjectQuery<ReleaseWorkflow> workflowSet = context.ReleaseWorkflows;
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
                                where w.ReleaseWorkflowId == workflowId
                                select w;

                    ReleaseWorkflow workflow = query.FirstOrDefault();
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
		/// Determines if a workflow is in use (i.e. associated with one or more release type)
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
                //Create the select command to retrieve the workflow used by release types
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the release type
                    var query = from it in context.ReleaseTypeWorkflows
                                where it.ReleaseWorkflowId == workflowId
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
        public List<ReleaseWorkflow> Workflow_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
            const string METHOD_NAME = "Workflow_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<ReleaseWorkflow> workflows;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflows
                    var query = from w in context.ReleaseWorkflows
                                where (!activeOnly || w.IsActive) && w.ProjectTemplateId == projectTemplateId
                                select w;

                    //For the active case (i.e. a lookup) order by name, otherwise in the admin case, order by id
                    if (activeOnly)
                    {
                        query = query.OrderBy(w => w.Name).ThenBy(w => w.ReleaseWorkflowId);
                    }
                    else
                    {
                        query = query.OrderBy(w => w.ReleaseWorkflowId);
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
        /// Gets the list of workflow transitions for the specified release status where the user has a specific role or is the author/owner
        /// </summary>
        /// <param name="inputReleaseStatusId">The input status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>List of matching workflow transitions</returns>
        /// <param name="isAuthor">Are we the author of the release</param>
        /// <param name="isOwner">Are we the owner of the release</param>
        /// <param name="projectRoleId">What project role are we</param>
        /// <remarks>This overload considers user permissions/roles when returning the list</remarks>
        public List<ReleaseWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputReleaseStatusId, int projectRoleId, bool isAuthor, bool isOwner)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.ReleaseWorkflowTransitions.Include("InputReleaseStatus").Include("OutputReleaseStatus")
                                where t.OutputReleaseStatus.IsActive && t.ReleaseWorkflowId == workflowId && t.InputReleaseStatusId == inputReleaseStatusId
                                select t;

                    //Create the expresion if we're the author, owner or in one of the roles allowed to execute the transition
                    List<Expression<Func<ReleaseWorkflowTransition, bool>>> expressionList = new List<Expression<Func<ReleaseWorkflowTransition, bool>>>();
                    if (isAuthor)
                    {
                        Expression<Func<ReleaseWorkflowTransition, bool>> clause = t => t.IsExecuteByCreator;
                        expressionList.Add(clause);
                    }
                    if (isOwner)
                    {
                        Expression<Func<ReleaseWorkflowTransition, bool>> clause = t => t.IsExecuteByOwner;
                        expressionList.Add(clause);
                    }

                    //Now the project role
                    Expression<Func<ReleaseWorkflowTransition, bool>> clause2 = t => t.TransitionRoles.Any(i => i.Role.ProjectRoleId == projectRoleId && i.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);
                    expressionList.Add(clause2);

                    if (expressionList.Count > 0)
                    {
                        //OR together the different cases and apply to the where clause
                        Expression<Func<ReleaseWorkflowTransition, bool>> aggregatedClause = Manager.BuildOr(expressionList.ToArray());
                        query = query.Where(aggregatedClause);
                    }

                    //Add the sorts
                    query = query.OrderBy(t => t.Name).ThenBy(t => t.WorkflowTransitionId);

                    List<ReleaseWorkflowTransition> workflowTransitions = query.ToList();

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
        /// <param name="inputReleaseStatusId">The input status</param>
        /// <param name="outputReleaseStatusId">The output status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>The workflow transition</returns>
        public ReleaseWorkflowTransition WorkflowTransition_RetrieveByStatuses(int workflowId, int inputReleaseStatusId, int outputReleaseStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.ReleaseWorkflowTransitions.Include("InputReleaseStatus").Include("OutputReleaseStatus")
                                where t.ReleaseWorkflowId == workflowId &&
                                      t.InputReleaseStatusId == inputReleaseStatusId &&
                                      t.OutputReleaseStatusId == outputReleaseStatusId
                                select t;

                    ReleaseWorkflowTransition workflowTransition = query.FirstOrDefault();

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
        /// Gets the list of workflow transitions for the specified release status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="inputReleaseStatusId">The id of the input status</param>
        /// <returns>List of transitions</returns>
        public List<ReleaseWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputReleaseStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.ReleaseWorkflowTransitions.Include("InputReleaseStatus").Include("OutputReleaseStatus")
                                where t.OutputReleaseStatus.IsActive && t.ReleaseWorkflowId == workflowId && t.InputReleaseStatusId == inputReleaseStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<ReleaseWorkflowTransition> workflowTransitions = query.ToList();

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
        /// Gets the list of workflow transitions for the specified release output status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="outputReleaseStatusId">The id of the output status</param>
        /// <returns>List of transitions</returns>
        public List<ReleaseWorkflowTransition> WorkflowTransition_RetrieveByOutputStatus(int workflowId, int outputReleaseStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByOutputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.ReleaseWorkflowTransitions.Include("InputReleaseStatus").Include("OutputReleaseStatus")
                                where t.OutputReleaseStatus.IsActive && t.ReleaseWorkflowId == workflowId && t.OutputReleaseStatusId == outputReleaseStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<ReleaseWorkflowTransition> workflowTransitions = query.ToList();

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
        public void Workflow_Update(ReleaseWorkflow workflow)
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
            if (!workflow.IsActive && IsInUse(workflow.ReleaseWorkflowId))
            {
                throw new WorkflowInUseException("You cannot make a workflow that is in use inactive");
            }

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.ReleaseWorkflows.ApplyChanges(workflow);
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
        public ReleaseWorkflow Workflow_Insert(int projectTemplateId, string name, bool isDefault, bool isActive = true)
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
                ReleaseWorkflow workflow;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow entity
                    workflow = new ReleaseWorkflow();
                    workflow.ProjectTemplateId = projectTemplateId;
                    workflow.Name = name;
                    workflow.IsActive = isActive;
                    workflow.IsDefault = isDefault;

                    //Persist the new workflow
                    context.ReleaseWorkflows.AddObject(workflow);
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
        /// <param name="inputReleaseStatusId">The input release status for the transition</param>
        /// <param name="outputReleaseStatusId">The output release status for the transition</param>
        /// <param name="name">The name of the transition</param>
        /// <param name="executeByCreator"></param>
        /// <param name="executeByOwner"></param>
        /// <param name="roles">The list of any roles that can also execute the transition</param>
        /// <returns>The new workflow transition</returns>
        public ReleaseWorkflowTransition WorkflowTransition_Insert(int workflowId, int inputReleaseStatusId, int outputReleaseStatusId, string name, bool executeByCreator = false, bool executeByOwner = true, List<int> roles = null)
        {
            const string METHOD_NAME = "Workflow_Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                ReleaseWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow transition entity
                    workflowTransition = new ReleaseWorkflowTransition();
                    workflowTransition.Name = name;
                    workflowTransition.ReleaseWorkflowId = workflowId;
                    workflowTransition.InputReleaseStatusId = inputReleaseStatusId;
                    workflowTransition.OutputReleaseStatusId = outputReleaseStatusId;
                    workflowTransition.IsExecuteByOwner = executeByOwner;
                    workflowTransition.IsExecuteByCreator = executeByCreator;

                    //Add the transition roles if any provided
                    if (roles != null && roles.Count > 0)
                    {
                        foreach (int projectRoleId in roles)
                        {
                            //For release workflows, all transition roles are execute transitions (not notify)
                            ReleaseWorkflowTransitionRole transitionRole = new ReleaseWorkflowTransitionRole();
                            transitionRole.ProjectRoleId = projectRoleId;
                            transitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
                            workflowTransition.TransitionRoles.Add(transitionRole);
                        }
                    }

                    //Persist the new workflow transition
                    context.ReleaseWorkflowTransitions.AddObject(workflowTransition);
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
                    var query = from wt in context.ReleaseWorkflowTransitions.Include("TransitionRoles")
                                where wt.WorkflowTransitionId == workflowTransitionId
                                select wt;

                    ReleaseWorkflowTransition workflowTransition = query.FirstOrDefault();

                    //Make sure we have a workflow transition
                    if (workflowTransition != null)
                    {
                        //Delete the workflow transition and dependent entities
                        context.ReleaseWorkflowTransitions.DeleteObject(workflowTransition);
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
        public void WorkflowTransition_Update(ReleaseWorkflowTransition workflowTransition)
        {
            const string METHOD_NAME = CLASS_NAME + "WorkflowTransition_Update";

            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.ReleaseWorkflowTransitions.ApplyChanges(workflowTransition);
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
        public ReleaseWorkflow Workflow_Copy(int sourceWorkflowId)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                ReleaseWorkflow destWorkflow = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow and associated transitions, fields and custom properties
                    var query = from w in context.ReleaseWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties")
                                where w.ReleaseWorkflowId == sourceWorkflowId
                                select w;

                    ReleaseWorkflow sourceWorkflow = query.FirstOrDefault();
                    if (sourceWorkflow == null)
                    {
                        throw new ArtifactNotExistsException("The workflow being copied no longer exists");
                    }

                    //Create the copied workflow
                    //except that we will always insert it with default=No
                    destWorkflow = new ReleaseWorkflow();
                    destWorkflow.ProjectTemplateId = sourceWorkflow.ProjectTemplateId;
                    destWorkflow.Name = GlobalResources.General.Global_CopyOf + " " + sourceWorkflow.Name;
                    destWorkflow.IsActive = sourceWorkflow.IsActive;
                    destWorkflow.IsDefault = false;

                    //Now add the transitions (including the transition roles)
                    for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                    {
                        ReleaseWorkflowTransition workflowTransition = new ReleaseWorkflowTransition();
                        workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                        workflowTransition.InputReleaseStatusId = sourceWorkflow.Transitions[i].InputReleaseStatusId;
                        workflowTransition.OutputReleaseStatusId = sourceWorkflow.Transitions[i].OutputReleaseStatusId;
                        workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                        workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                        workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                        destWorkflow.Transitions.Add(workflowTransition);

                        //Now the roles
                        for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                        {
                            ReleaseWorkflowTransitionRole workflowTransitionRole = new ReleaseWorkflowTransitionRole();
                            workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                            workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                            workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                        }
                    }

                    //Now add the fields
                    for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                    {
                        ReleaseWorkflowField workflowField = new ReleaseWorkflowField();
                        workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                        workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                        workflowField.ReleaseStatusId = sourceWorkflow.Fields[i].ReleaseStatusId;
                        destWorkflow.Fields.Add(workflowField);
                    }

                    //Now add the custom properties
                    for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                    {
                        ReleaseWorkflowCustomProperty workflowCustomProperty = new ReleaseWorkflowCustomProperty();
                        workflowCustomProperty.CustomPropertyId = sourceWorkflow.CustomProperties[i].CustomPropertyId;
                        workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                        workflowCustomProperty.ReleaseStatusId = sourceWorkflow.CustomProperties[i].ReleaseStatusId;
                        destWorkflow.CustomProperties.Add(workflowCustomProperty);
                    }

                    //Save the new workflow
                    context.ReleaseWorkflows.AddObject(destWorkflow);
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
        /// Copies all the workflows from one project to another (used when making a new project based on an existing one)
        /// </summary>
        /// <param name="sourceProjectTemplateId">The project template to copy the workflows from</param>
        /// <param name="destProjectTemplateId">The destination project template</param>
        /// <param name="customPropertyIdMapping">The mapping of old vs. new custom properties between the two projects</param>
        public void Workflow_Copy(int sourceProjectTemplateId, int destProjectTemplateId, Dictionary<int, int> customPropertyIdMapping)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflows and associated transitions, fields release types, and custom properties
                    var query = from w in context.ReleaseWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties").Include("ReleaseTypes")
                                where w.ProjectTemplateId == sourceProjectTemplateId
                                orderby w.ReleaseWorkflowId
                                select w;

                    List<ReleaseWorkflow> sourceWorkflows = query.ToList();

                    //Loop through the various workflows
                    foreach (ReleaseWorkflow sourceWorkflow in sourceWorkflows)
                    {
                        //Create the copied workflow
                        ReleaseWorkflow destWorkflow = new ReleaseWorkflow();
                        destWorkflow.ProjectTemplateId = destProjectTemplateId;
                        destWorkflow.Name = sourceWorkflow.Name;
                        destWorkflow.IsActive = sourceWorkflow.IsActive;
                        destWorkflow.IsDefault = sourceWorkflow.IsDefault;

                        //Now add the transitions (including the transition roles)
                        for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                        {
                            ReleaseWorkflowTransition workflowTransition = new ReleaseWorkflowTransition();
                            workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                            workflowTransition.InputReleaseStatusId = sourceWorkflow.Transitions[i].InputReleaseStatusId;
                            workflowTransition.OutputReleaseStatusId = sourceWorkflow.Transitions[i].OutputReleaseStatusId;
                            workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                            workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                            workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                            destWorkflow.Transitions.Add(workflowTransition);

                            //Now the roles
                            for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                            {
                                ReleaseWorkflowTransitionRole workflowTransitionRole = new ReleaseWorkflowTransitionRole();
                                workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                                workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                                workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                            }
                        }

                        //Now add the fields
                        for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                        {
                            ReleaseWorkflowField workflowField = new ReleaseWorkflowField();
                            workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                            workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                            workflowField.ReleaseStatusId = sourceWorkflow.Fields[i].ReleaseStatusId;
                            destWorkflow.Fields.Add(workflowField);
                        }

                        //Now add the custom properties
                        for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                        {
                            //Get the corresponding custom property in the new project
                            if (customPropertyIdMapping.ContainsKey(sourceWorkflow.CustomProperties[i].CustomPropertyId))
                            {
                                int destCustomPropertyId = customPropertyIdMapping[sourceWorkflow.CustomProperties[i].CustomPropertyId];
                                ReleaseWorkflowCustomProperty workflowCustomProperty = new ReleaseWorkflowCustomProperty();
                                workflowCustomProperty.CustomPropertyId = destCustomPropertyId;
                                workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                                workflowCustomProperty.ReleaseStatusId = sourceWorkflow.CustomProperties[i].ReleaseStatusId;
                                destWorkflow.CustomProperties.Add(workflowCustomProperty);
                            }
                        }

                        //Add the mapping to the static release types
                        foreach (ReleaseTypeWorkflow sourceReleaseType in sourceWorkflow.ReleaseTypes)
                        {
                            ReleaseTypeWorkflow destReleaseType = new ReleaseTypeWorkflow();
                            destReleaseType.ProjectTemplateId = destProjectTemplateId;
                            destReleaseType.ReleaseTypeId = sourceReleaseType.ReleaseTypeId;
                            destWorkflow.ReleaseTypes.Add(destReleaseType);
                        }

                        //Add the new workflow
                        context.ReleaseWorkflows.AddObject(destWorkflow);
                    }

                    //Save the changes
                    context.SaveChanges();
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
        /// Deletes all the workflows in the project template (called by the project template delete)
        /// </summary>
        /// <param name="projectTemplateId">The project template to delete workflow for</param>
        protected internal void Workflow_DeleteAllForProjectTemplate(int projectTemplateId)
        {
            const string METHOD_NAME = "Workflow_DeleteAllForProjectTemplate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow
                    var query = from w in context.ReleaseWorkflows.Include(r => r.ReleaseTypes)
                                where w.ProjectTemplateId == projectTemplateId
                                orderby w.ReleaseWorkflowId
                                select w;

                    List<ReleaseWorkflow> workflows = query.ToList();
                    for (int i = 0; i < workflows.Count; i++)
                    {
                        context.ReleaseWorkflows.DeleteObject(workflows[i]);
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
                    var query = from w in context.ReleaseWorkflows
                                where w.ReleaseWorkflowId == workflowId
                                select w;

                    ReleaseWorkflow workflow = query.FirstOrDefault();

                    //Make sure we have a workflow
                    if (workflow != null)
                    {
                        //If the workflow is marked as default, you can't delete it
                        if (workflow.IsDefault)
                        {
                            throw new WorkflowInUseException("You cannot delete a workflow that is marked as the default");
                        }

                        //Delete the workflow. The database cascades will handle the dependent entities
                        context.ReleaseWorkflows.DeleteObject(workflow);
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
