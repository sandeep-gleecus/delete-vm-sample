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
    /// the risk workflows that are used to determine the lifecycle of risks in the system
    /// </summary>
    public class RiskWorkflowManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.RiskWorkflowManager::";

        #region Static Methods

        /// <summary>
        /// Converts risk workflow field collection to the generic kind
        /// </summary>
        /// <param name="riskWorkflowCustomProps">The risk workflow fields</param>
        /// <returns>The workflow fields</returns>
        public static List<WorkflowField> ConvertFields(List<RiskWorkflowField> riskWorkflowFields)
        {
            //Handle null case safely
            if (riskWorkflowFields == null)
            {
                return null;
            }

            List<WorkflowField> workflowFields = new List<WorkflowField>();
            foreach (RiskWorkflowField riskWorkflowField in riskWorkflowFields)
            {
                WorkflowField workflowField = new WorkflowField();
                workflowField.ArtifactFieldId = riskWorkflowField.ArtifactFieldId;
                workflowField.IncidentStatusId = riskWorkflowField.RiskStatusId;
                workflowField.WorkflowId = riskWorkflowField.RiskWorkflowId;
                workflowField.WorkflowFieldStateId = riskWorkflowField.WorkflowFieldStateId;
                workflowField.Field = new ArtifactField();
                workflowField.Field.Name = riskWorkflowField.ArtifactField.Name;
                workflowFields.Add(workflowField);
            }

            return workflowFields;
        }

        /// <summary>
        /// Converts risk workflow custom properties collection to the generic kind
        /// </summary>
        /// <param name="riskWorkflowCustomProps">The risk workflow custom properties</param>
        /// <returns>The workflow custom properties</returns>
        public static List<WorkflowCustomProperty> ConvertFields(List<RiskWorkflowCustomProperty> riskWorkflowCustomProps)
        {
            //Handle null case safely
            if (riskWorkflowCustomProps == null)
            {
                return null;
            }

            List<WorkflowCustomProperty> workflowCustomProperties = new List<WorkflowCustomProperty>();
            foreach (RiskWorkflowCustomProperty riskWorkflowCustomProp in riskWorkflowCustomProps)
            {
                WorkflowCustomProperty workflowCustomProperty = new WorkflowCustomProperty();
                workflowCustomProperty.CustomPropertyId = riskWorkflowCustomProp.CustomPropertyId;
                workflowCustomProperty.IncidentStatusId = riskWorkflowCustomProp.RiskStatusId;
                workflowCustomProperty.WorkflowId = riskWorkflowCustomProp.RiskWorkflowId;
                workflowCustomProperty.WorkflowFieldStateId = riskWorkflowCustomProp.WorkflowFieldStateId;
                workflowCustomProperty.CustomProperty = new CustomProperty();
                workflowCustomProperty.CustomProperty.PropertyNumber = riskWorkflowCustomProp.CustomProperty.PropertyNumber;
                workflowCustomProperties.Add(workflowCustomProperty);
            }

            return workflowCustomProperties;
        }

		#endregion

		#region Public Methods

		public List<RiskStatusResponse> WorkflowTransition_RetrieveByInputStatusById(int workflowId, int inputRiskStatusId, int projectTemplateId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatusById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RiskWorkflowTransitions.Include("InputRiskStatus").Include("OutputRiskStatus")
								join i in context.RiskStati
								on t.InputRiskStatusId equals i.RiskStatusId
								join o in context.RiskStati
								on t.OutputRiskStatusId equals o.RiskStatusId
								where t.OutputStatus.IsActive && t.RiskWorkflowId == workflowId && t.InputRiskStatusId == inputRiskStatusId
								orderby t.Name, t.WorkflowTransitionId
								select new RiskStatusResponse
								{
									InputRiskStatusId = t.InputRiskStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									RiskWorkflowId = t.RiskWorkflowId,
									IsBlankOwner = t.IS_BLANK_OWNER,
									NotifySubject = t.NOTIFY_SUBJECT,
									IsNotifyCreator = t.IS_NOTIFY_CREATOR,
									IsNotifyOwner = t.IS_NOTIFY_OWNER,
									TransitionRoles = t.TransitionRoles,
									Workflow = null,
									URL = "",
									OutURL = "",
									InURL = "",
									OutputRiskStatusId = t.OutputRiskStatusId,
								};

					var workflows = query.ToList();

					foreach (var c in workflows)
					{
						c.InURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/RiskWorkflowStep.aspx?workflowId=" + workflowId + "&taskStatusId=" + c.InputRiskStatusId;
						c.URL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/RiskWorkflowTransition.aspx?workflowId=" + workflowId + "&workflowTransitionId=" + c.WorkflowTransitionId;
						c.OutURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/RiskWorkflowStep.aspx?workflowId=" + workflowId + "&taskStatusId=" + c.OutputRiskStatusId;
					}

					query = workflows.AsQueryable();

					List<RiskStatusResponse> workflowTransitions = query.ToList();

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

		public List<RiskStatusResponse> WorkflowTransition_RetrieveAllStatuses(int workflowId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RiskWorkflowTransitions.Include("InputRiskStatus").Include("OutputStatus")
								join i in context.RiskStati
								on t.InputRiskStatusId equals i.RiskStatusId
								join o in context.RiskStati
								on t.OutputRiskStatusId equals o.RiskStatusId
								where t.OutputStatus.IsActive && t.RiskWorkflowId == workflowId
								orderby i.Position, t.WorkflowTransitionId
								select new RiskStatusResponse
								{
									InputRiskStatusId = t.InputRiskStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									RiskWorkflowId = t.RiskWorkflowId,
									IsBlankOwner = t.IS_BLANK_OWNER,
									NotifySubject = t.NOTIFY_SUBJECT,
									IsNotifyCreator = t.IS_NOTIFY_CREATOR,
									IsNotifyOwner = t.IS_NOTIFY_OWNER,
									TransitionRoles = t.TransitionRoles,
									Workflow = null,
								};

					List<RiskStatusResponse> workflowTransitions = query.ToList();

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
		/// <param name="riskStatuses">The risk statuses</param>
		/// <returns>The newly inserted workflow</returns>
		public RiskWorkflow Workflow_InsertWithDefaultEntries(int projectTemplateId, string name, bool isDefault, Dictionary<int, int> riskStatuses, bool isActive = true)
        {
            const string METHOD_NAME = "Workflow_InsertWithDefaultEntries";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First create the workflow itself
                RiskWorkflow workflow = this.Workflow_Insert(projectTemplateId, name, isDefault, isActive);
                int workflowId = workflow.RiskWorkflowId;

                //Workflow Transitions (inc. roles)
                int workflowTransitionId1 = WorkflowTransition_Insert(workflowId, riskStatuses[1], riskStatuses[2], "Analyze Risk", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId2 = WorkflowTransition_Insert(workflowId, riskStatuses[2], riskStatuses[3], "Evaluate Risk", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId3 = WorkflowTransition_Insert(workflowId, riskStatuses[3], riskStatuses[4], "Treat Risk", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId4 = WorkflowTransition_Insert(workflowId, riskStatuses[4], riskStatuses[5], "Close Risk", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId5 = WorkflowTransition_Insert(workflowId, riskStatuses[1], riskStatuses[6], "Reject Risk", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId6 = WorkflowTransition_Insert(workflowId, riskStatuses[2], riskStatuses[6], "Reject Risk", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId7 = WorkflowTransition_Insert(workflowId, riskStatuses[3], riskStatuses[6], "Reject Risk", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId8 = WorkflowTransition_Insert(workflowId, riskStatuses[4], riskStatuses[6], "Reject Risk", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId9 = WorkflowTransition_Insert(workflowId, riskStatuses[5], riskStatuses[4], "Reopen Risk", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId10 = WorkflowTransition_Insert(workflowId, riskStatuses[6], riskStatuses[1], "Reopen Risk", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                //Workflow Fields

                //All fields are active, visible and not-required by default, so only need to populate the ones
                //that are exceptions to that case

                //Identified - Hidden: ReviewDate, ClosedDate
                RiskStatus riskStatus = Status_RetrieveById(riskStatuses[1], workflowId);
                riskStatus.StartTracking();
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 196, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 198, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
                Status_UpdateFieldsAndCustomProperties(riskStatus);

                //Analyzed - Required: ReleaseId, OwnerId, RiskProbabilityId, RiskImpactId
                riskStatus = Status_RetrieveById(riskStatuses[2], workflowId);
                riskStatus.StartTracking();
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 184, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 185, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 189, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 190, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                //Analyzed - Hidden: ClosedDate
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 196, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
                Status_UpdateFieldsAndCustomProperties(riskStatus);

				//Evaluated - Required: ReleaseId, OwnerId, RiskProbabilityId, RiskImpactId, ReviewDate
				riskStatus = Status_RetrieveById(riskStatuses[3], workflowId);
                riskStatus.StartTracking();
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 184, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 185, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 189, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 190, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 198, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                //Evaluated - Hidden: ClosedDate
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 196, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
                Status_UpdateFieldsAndCustomProperties(riskStatus);

                //Open - Required: ReleaseId, OwnerId, RiskProbabilityId, RiskImpactId, ReviewDate
                riskStatus = Status_RetrieveById(riskStatuses[4], workflowId);
                riskStatus.StartTracking();
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 184, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 185, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 189, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 190, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 198, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                //Open - Hidden: ClosedDate
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 196, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
                Status_UpdateFieldsAndCustomProperties(riskStatus);

                //Closed - Required: ClosedDate
                riskStatus = Status_RetrieveById(riskStatuses[5], workflowId);
                riskStatus.StartTracking();
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 196, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                //Closed - Disabled: All workflow fields except closed date
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 184, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 185, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 187, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 188, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 189, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 190, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 191, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 192, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 193, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 194, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 198, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(riskStatus);

                //Rejected - Required: ReviewDate
                riskStatus = Status_RetrieveById(riskStatuses[6], workflowId);
                riskStatus.StartTracking();
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 198, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
                //Rejected - Disabled: Most fields apart from review date
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 184, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 185, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 187, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 188, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 189, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 190, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 191, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 192, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 193, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 194, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                riskStatus.WorkflowFields.Add(new RiskWorkflowField() { RiskWorkflowId = workflowId, ArtifactFieldId = 196, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
                Status_UpdateFieldsAndCustomProperties(riskStatus);

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
        /// Updates the workflow fields and custom properties associated with a particular risk status
        /// </summary>
        /// <param name="status">The status whose fields and custom properties we want to update</param>
        public void Status_UpdateFieldsAndCustomProperties(RiskStatus status)
        {
            const string METHOD_NAME = "Status_UpdateFieldsAndCustomProperties";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Attach the changed entity to the EF context and persist changes
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    context.RiskStati.ApplyChanges(status);
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

        /// <summary>Retrieves the risk status by the given ID.</summary>
        /// <param name="riskStatusId">The status ID to retrieve.</param>
        /// <returns>The RiskStatus, or null if not found.</returns>
        /// <remarks>Will return deleted items.</remarks>
        /// <param name="includeWorkflowFieldsForWorkflowId">Should we include the linked workflow fields</param>
        public RiskStatus Status_RetrieveById(int riskStatusId, int? includeWorkflowFieldsForWorkflowId = null)
        {
            const string METHOD_NAME = "Status_RetrieveById()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RiskStatus retStatus = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from r in context.RiskStati
                                where r.RiskStatusId == riskStatusId
                                select r;

                    retStatus = query.FirstOrDefault();

                    //Get the fields and custom properties, joined by EF 'fix-up'
                    if (includeWorkflowFieldsForWorkflowId.HasValue)
                    {
                        int workflowId = includeWorkflowFieldsForWorkflowId.Value;
                        var query2 = from w in context.RiskWorkflowFields
                                     where w.RiskStatusId == riskStatusId && w.RiskWorkflowId == workflowId && w.ArtifactField.IsActive
                                     select w;
                        query2.ToList();
                        var query3 = from w in context.RiskWorkflowCustomProperties
                                     where w.RiskStatusId == riskStatusId && w.RiskWorkflowId == workflowId && !w.CustomProperty.IsDeleted
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
        public RiskWorkflow Workflow_GetDefault(int projectTemplateId)
		{
            const string METHOD_NAME = "Workflow_GetDefault";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from w in context.RiskWorkflows
                                where w.IsActive && w.IsDefault && w.ProjectTemplateId == projectTemplateId
                                select w;

                    RiskWorkflow workflow = query.FirstOrDefault();
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
		/// Gets the workflow for a specific risk type
		/// </summary>
		/// <param name="riskTypeId">The risk type we're interested in</param>
		/// <returns>The workflow id</returns>
        public int Workflow_GetForRiskType(int riskTypeId)
		{
            const string METHOD_NAME = "Workflow_GetForRiskType";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from r in context.RiskTypes.Include(t => t.Workflow)
                                where r.RiskTypeId == riskTypeId
                                select r;

                    RiskType riskType = query.FirstOrDefault();
                    if (riskType == null || riskType.Workflow == null)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve workflow for risk type " + riskTypeId + ".");
                    }
                    else
                    {
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return riskType.RiskWorkflowId;
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
		/// Retrieves the list of inactive, hidden and required fields for an risk based on its position
		/// in the workflow.
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="riskStatusId">The current step (i.e. risk status) in the workflow</param>
        /// <remarks>Workflow needs to be active</remarks>
		public List<RiskWorkflowField> Workflow_RetrieveFieldStates(int workflowId, int riskStatusId)
		{
            const string METHOD_NAME = "Workflow_RetrieveFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.RiskWorkflowFields
                                    .Include(w => w.FieldState)
                                    .Include(w => w.ArtifactField)
                                where w.RiskWorkflowId == workflowId && w.Workflow.IsActive && w.ArtifactField.IsActive && w.RiskStatusId == riskStatusId
                                orderby w.WorkflowFieldStateId, w.ArtifactFieldId
                                select w;

                    List<RiskWorkflowField> workflowFields = query.ToList();

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
        /// Retrieves the list of inactive, hidden and required custom properties for an risk based on its position
        /// in the workflow.
        /// </summary>
        /// <param name="workflowId">The workflow we're using</param>
        /// <param name="riskStatusId">The current step (i.e. status) in the workflow</param>
        /// <returns>List of inactive, hidden and required custom properties for the current workflow step</returns>
        /// <remarks>Workflow needs to be active</remarks>
        public List<RiskWorkflowCustomProperty> Workflow_RetrieveCustomPropertyStates(int workflowId, int riskStatusId)
        {
            const string METHOD_NAME = "Workflow_RetrieveCustomPropertyStates";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.RiskWorkflowCustomProperties
                                    .Include(w => w.FieldState)
                                    .Include(w => w.CustomProperty)
                                where w.RiskWorkflowId == workflowId &&
                                    w.Workflow.IsActive &&
                                    w.RiskStatusId == riskStatusId &&
                                    !w.CustomProperty.IsDeleted
                                orderby w.WorkflowFieldStateId, w.CustomPropertyId
                                select w;

                    List<RiskWorkflowCustomProperty> workflowCustomProps = query.ToList();

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
        /// Retrieves a workflow transition by its id, includes the transition roles and risk status names
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="workflowTransitionId">The id of the transition</param>
        /// <returns></returns>
        public RiskWorkflowTransition WorkflowTransition_RetrieveById(int workflowId, int workflowTransitionId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RiskWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RiskWorkflowTransitions
                                    .Include(r => r.InputStatus)
                                    .Include(r => r.OutputStatus)
                                    .Include(r => r.TransitionRoles)
                                    .Include("TransitionRoles.ProjectRole")
                                where t.RiskWorkflowId == workflowId && t.WorkflowTransitionId == workflowTransitionId
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
        public RiskWorkflow Workflow_RetrieveById(int workflowId, bool includeTransitions = false, bool includeFieldStates = false)
        {
            const string METHOD_NAME = "Workflow_RetrieveById(int,[bool],[bool])";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See if we need to include transitions
                    ObjectQuery<RiskWorkflow> workflowSet = context.RiskWorkflows;
                    if (includeTransitions)
                    {
                        workflowSet = workflowSet
                            .Include(w => w.Transitions)
                            .Include("Transitions.TransitionRoles");
                    }
                    if (includeFieldStates)
                    {
                        workflowSet = workflowSet
                            .Include(w => w.Fields)
                            .Include(w => w.CustomProperties);
                    }

                    //Get the workflow by its id (active and inactive)
                    var query = from w in workflowSet
                                where w.RiskWorkflowId == workflowId
                                select w;

                    RiskWorkflow workflow = query.FirstOrDefault();
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
		/// Determines if a workflow is in use (i.e. associated with one or more risk type)
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
                //Create the select command to retrieve the workflow used by the risk type
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the risk type
                    var query = from it in context.RiskTypes
                                where it.RiskWorkflowId == workflowId
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
        public List<RiskWorkflow> Workflow_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
            const string METHOD_NAME = "Workflow_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<RiskWorkflow> workflows;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflows
                    var query = from w in context.RiskWorkflows
                                where (!activeOnly || w.IsActive) && w.ProjectTemplateId == projectTemplateId
                                select w;

                    //For the active case (i.e. a lookup) order by name, otherwise in the admin case, order by id
                    if (activeOnly)
                    {
                        query = query.OrderBy(w => w.Name).ThenBy(w => w.RiskWorkflowId);
                    }
                    else
                    {
                        query = query.OrderBy(w => w.RiskWorkflowId);
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
        /// Gets the list of workflow transitions for the specified risk status where the user has a specific role or is the author/owner
        /// </summary>
        /// <param name="inputRiskStatusId">The input status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>List of matching workflow transitions</returns>
        /// <param name="isAuthor">Are we the author of the risk</param>
        /// <param name="isOwner">Are we the owner of the risk</param>
        /// <param name="projectRoleId">What project role are we</param>
        /// <remarks>This overload considers user permissions/roles when returning the list</remarks>
        public List<RiskWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputRiskStatusId, int projectRoleId, bool isAuthor, bool isOwner)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RiskWorkflowTransitions
                                    .Include(r => r.InputStatus)
                                    .Include(r => r.OutputStatus)
                                where t.OutputStatus.IsActive && t.RiskWorkflowId == workflowId && t.InputRiskStatusId == inputRiskStatusId
                                select t;

                    //Create the expresion if we're the author, owner or in one of the roles allowed to execute the transition
                    List<Expression<Func<RiskWorkflowTransition, bool>>> expressionList = new List<Expression<Func<RiskWorkflowTransition, bool>>>();
                    if (isAuthor)
                    {
                        Expression<Func<RiskWorkflowTransition, bool>> clause = t => t.IsExecuteByCreator;
                        expressionList.Add(clause);
                    }
                    if (isOwner)
                    {
                        Expression<Func<RiskWorkflowTransition, bool>> clause = t => t.IsExecuteByOwner;
                        expressionList.Add(clause);
                    }

                    //Now the project role
                    Expression<Func<RiskWorkflowTransition, bool>> clause2 = t => t.TransitionRoles.Any(i => i.ProjectRole.ProjectRoleId == projectRoleId && i.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);
                    expressionList.Add(clause2);

                    if (expressionList.Count > 0)
                    {
                        //OR together the different cases and apply to the where clause
                        Expression<Func<RiskWorkflowTransition, bool>> aggregatedClause = Manager.BuildOr(expressionList.ToArray());
                        query = query.Where(aggregatedClause);
                    }

                    //Add the sorts
                    query = query.OrderBy(t => t.Name).ThenBy(t => t.WorkflowTransitionId);

                    List<RiskWorkflowTransition> workflowTransitions = query.ToList();

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
        /// <param name="inputRiskStatusId">The input status</param>
        /// <param name="outputRiskStatusId">The output status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>The workflow transition</returns>
        public RiskWorkflowTransition WorkflowTransition_RetrieveByStatuses(int workflowId, int inputRiskStatusId, int outputRiskStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RiskWorkflowTransitions
                                    .Include(r => r.InputStatus)
                                    .Include(r => r.OutputStatus)
                                where t.RiskWorkflowId == workflowId &&
                                      t.InputRiskStatusId == inputRiskStatusId &&
                                      t.OutputRiskStatusId == outputRiskStatusId
                                select t;

                    RiskWorkflowTransition workflowTransition = query.FirstOrDefault();

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
        /// Gets the list of workflow transitions for the specified risk status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="inputRiskStatusId">The id of the input status</param>
        /// <returns>List of transitions</returns>
        public List<RiskWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputRiskStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RiskWorkflowTransitions
                                    .Include(r => r.InputStatus)
                                    .Include(r => r.OutputStatus)
                                where t.OutputStatus.IsActive && t.RiskWorkflowId == workflowId && t.InputRiskStatusId == inputRiskStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<RiskWorkflowTransition> workflowTransitions = query.ToList();

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
        /// Gets the list of workflow transitions for the specified risk output status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="outputRiskStatusId">The id of the output status</param>
        /// <returns>List of transitions</returns>
        public List<RiskWorkflowTransition> WorkflowTransition_RetrieveByOutputStatus(int workflowId, int outputRiskStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByOutputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.RiskWorkflowTransitions
                                    .Include(r => r.InputStatus)
                                    .Include(r => r.OutputStatus)
                                where t.OutputStatus.IsActive && t.RiskWorkflowId == workflowId && t.OutputRiskStatusId == outputRiskStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<RiskWorkflowTransition> workflowTransitions = query.ToList();

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
        public void Workflow_Update(RiskWorkflow workflow)
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
            if (!workflow.IsActive && IsInUse(workflow.RiskWorkflowId))
            {
                throw new WorkflowInUseException("You cannot make a workflow that is in use inactive");
            }

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.RiskWorkflows.ApplyChanges(workflow);
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
        public RiskWorkflow Workflow_Insert(int projectTemplateId, string name, bool isDefault, bool isActive = true)
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
                RiskWorkflow workflow;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow entity
                    workflow = new RiskWorkflow();
                    workflow.ProjectTemplateId = projectTemplateId;
                    workflow.Name = name;
                    workflow.IsActive = isActive;
                    workflow.IsDefault = isDefault;

                    //Persist the new workflow
                    context.RiskWorkflows.AddObject(workflow);
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
        /// <param name="inputRiskStatusId">The input risk status for the transition</param>
        /// <param name="outputRiskStatusId">The output risk status for the transition</param>
        /// <param name="name">The name of the transition</param>
        /// <param name="executeByCreator"></param>
        /// <param name="executeByOwner"></param>
        /// <param name="roles">The list of any roles that can also execute the transition</param>
        /// <returns>The new workflow transition</returns>
        public RiskWorkflowTransition WorkflowTransition_Insert(int workflowId, int inputRiskStatusId, int outputRiskStatusId, string name, bool executeByCreator = false, bool executeByOwner = true, List<int> roles = null)
        {
            const string METHOD_NAME = "Workflow_Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RiskWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow transition entity
                    workflowTransition = new RiskWorkflowTransition();
                    workflowTransition.Name = name;
                    workflowTransition.RiskWorkflowId = workflowId;
                    workflowTransition.InputRiskStatusId = inputRiskStatusId;
                    workflowTransition.OutputRiskStatusId = outputRiskStatusId;
                    workflowTransition.IsExecuteByOwner = executeByOwner;
                    workflowTransition.IsExecuteByCreator = executeByCreator;

                    //Add the transition roles if any provided
                    if (roles != null && roles.Count > 0)
                    {
                        foreach (int projectRoleId in roles)
                        {
                            //For risk workflows, all transition roles are execute transitions (not notify)
                            RiskWorkflowTransitionRole transitionRole = new RiskWorkflowTransitionRole();
                            transitionRole.ProjectRoleId = projectRoleId;
                            transitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
                            workflowTransition.TransitionRoles.Add(transitionRole);
                        }
                    }

                    //Persist the new workflow transition
                    context.RiskWorkflowTransitions.AddObject(workflowTransition);
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
                    var query = from wt in context.RiskWorkflowTransitions
                                    .Include(w => w.TransitionRoles)
                                where wt.WorkflowTransitionId == workflowTransitionId
                                select wt;

                    RiskWorkflowTransition workflowTransition = query.FirstOrDefault();

                    //Make sure we have a workflow transition
                    if (workflowTransition != null)
                    {
                        //Delete the workflow transition and dependent entities
                        context.RiskWorkflowTransitions.DeleteObject(workflowTransition);
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
        public void WorkflowTransition_Update(RiskWorkflowTransition workflowTransition)
        {
            const string METHOD_NAME = CLASS_NAME + "WorkflowTransition_Update";

            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.RiskWorkflowTransitions.ApplyChanges(workflowTransition);
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
        /// <param name="riskWorkflowMapping">The mapping of workflow ids between the old and new templates</param>
        /// <param name="sourceProjectTemplateId">The project template to copy the workflows from</param>
        /// <param name="riskStatusMapping">The risk status mapping</param>
        /// <param name="destProjectTemplateId">The destination project template</param>
        /// <param name="customPropertyIdMapping">The mapping of old vs. new custom properties between the two project templates</param>
        public void Workflow_Copy(int sourceProjectTemplateId, int destProjectTemplateId, Dictionary<int, int> customPropertyIdMapping, Dictionary<int, int> riskWorkflowMapping, Dictionary<int, int> riskStatusMapping)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Dictionary<int, RiskWorkflow> tempMapping = new Dictionary<int, RiskWorkflow>();
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflows and associated transitions, fields risk types, and custom properties
                    var query = from w in context.RiskWorkflows
                                    .Include(w => w.Transitions)
                                    .Include("Transitions.TransitionRoles")
                                    .Include(w => w.Fields)
                                    .Include(w => w.CustomProperties)
                                    .Include(w => w.RiskTypes)
                                where w.ProjectTemplateId == sourceProjectTemplateId
                                orderby w.RiskWorkflowId
                                select w;

                    List<RiskWorkflow> sourceWorkflows = query.ToList();

                    //Loop through the various workflows
                    foreach (RiskWorkflow sourceWorkflow in sourceWorkflows)
                    {
                        //Create the copied workflow
                        RiskWorkflow destWorkflow = new RiskWorkflow();
                        destWorkflow.ProjectTemplateId = destProjectTemplateId;
                        destWorkflow.Name = sourceWorkflow.Name;
                        destWorkflow.IsActive = sourceWorkflow.IsActive;
                        destWorkflow.IsDefault = sourceWorkflow.IsDefault;

                        //Add to mapping
                        tempMapping.Add(sourceWorkflow.RiskWorkflowId, destWorkflow);

                        //Now add the transitions (including the transition roles)
                        for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                        {
                            //Lookup the mapped statuses
                            if (riskStatusMapping.ContainsKey(sourceWorkflow.Transitions[i].InputRiskStatusId))
                            {
                                int inputRiskStatusId = riskStatusMapping[sourceWorkflow.Transitions[i].InputRiskStatusId];
                                if (riskStatusMapping.ContainsKey(sourceWorkflow.Transitions[i].OutputRiskStatusId))
                                {
                                    int outputRiskStatusId = riskStatusMapping[sourceWorkflow.Transitions[i].OutputRiskStatusId];

                                    RiskWorkflowTransition workflowTransition = new RiskWorkflowTransition();
                                    workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                                    workflowTransition.InputRiskStatusId = inputRiskStatusId;
                                    workflowTransition.OutputRiskStatusId = outputRiskStatusId;
                                    workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                                    workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                                    workflowTransition.IsSignature = sourceWorkflow.Transitions[i].IsSignature;
                                    destWorkflow.Transitions.Add(workflowTransition);

                                    //Now the roles
                                    for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                                    {
                                        RiskWorkflowTransitionRole workflowTransitionRole = new RiskWorkflowTransitionRole();
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
                            //Lookup the mapped statuses
                            if (riskStatusMapping.ContainsKey(sourceWorkflow.Fields[i].RiskStatusId))
                            {
                                int riskStatusId = riskStatusMapping[sourceWorkflow.Fields[i].RiskStatusId];

                                RiskWorkflowField workflowField = new RiskWorkflowField();
                                workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                                workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                                workflowField.RiskStatusId = riskStatusId;
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

                                //Lookup the mapped statuses
                                if (riskStatusMapping.ContainsKey(sourceWorkflow.CustomProperties[i].RiskStatusId))
                                {
                                    int riskStatusId = riskStatusMapping[sourceWorkflow.CustomProperties[i].RiskStatusId];

                                    RiskWorkflowCustomProperty workflowCustomProperty = new RiskWorkflowCustomProperty();
                                    workflowCustomProperty.CustomPropertyId = destCustomPropertyId;
                                    workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                                    workflowCustomProperty.RiskStatusId = riskStatusId;
                                    destWorkflow.CustomProperties.Add(workflowCustomProperty);
                                }
                            }
                        }

                        //Add the new workflow
                        context.RiskWorkflows.AddObject(destWorkflow);
                    }

                    //Save the changes
                    context.SaveChanges();
                }

                //Finally populate the external mapping dictionary (id based)
                foreach (KeyValuePair<int, RiskWorkflow> kvp in tempMapping)
                {
                    riskWorkflowMapping.Add(kvp.Key, kvp.Value.RiskWorkflowId);
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
        public RiskWorkflow Workflow_Copy(int sourceWorkflowId)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RiskWorkflow destWorkflow = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow and associated transitions, fields and custom properties
                    var query = from w in context.RiskWorkflows
                                    .Include(w => w.Transitions)
                                    .Include("Transitions.TransitionRoles")
                                    .Include(w => w.Fields)
                                    .Include(w => w.CustomProperties)
                                where w.RiskWorkflowId == sourceWorkflowId
                                select w;

                    RiskWorkflow sourceWorkflow = query.FirstOrDefault();
                    if (sourceWorkflow == null)
                    {
                        throw new ArtifactNotExistsException("The workflow being copied no longer exists");
                    }

                    //Create the copied workflow
                    //except that we will always insert it with default=No
                    destWorkflow = new RiskWorkflow();
                    destWorkflow.ProjectTemplateId = sourceWorkflow.ProjectTemplateId;
                    destWorkflow.Name = GlobalResources.General.Global_CopyOf + " " + sourceWorkflow.Name;
                    destWorkflow.IsActive = sourceWorkflow.IsActive;
                    destWorkflow.IsDefault = false;

                    //Now add the transitions (including the transition roles)
                    for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                    {
                        RiskWorkflowTransition workflowTransition = new RiskWorkflowTransition();
                        workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                        workflowTransition.InputRiskStatusId = sourceWorkflow.Transitions[i].InputRiskStatusId;
                        workflowTransition.OutputRiskStatusId = sourceWorkflow.Transitions[i].OutputRiskStatusId;
                        workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                        workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                        workflowTransition.IsSignature = sourceWorkflow.Transitions[i].IsSignature;
                        destWorkflow.Transitions.Add(workflowTransition);

                        //Now the roles
                        for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                        {
                            RiskWorkflowTransitionRole workflowTransitionRole = new RiskWorkflowTransitionRole();
                            workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                            workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                            workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                        }
                    }

                    //Now add the fields
                    for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                    {
                        RiskWorkflowField workflowField = new RiskWorkflowField();
                        workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                        workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                        workflowField.RiskStatusId = sourceWorkflow.Fields[i].RiskStatusId;
                        destWorkflow.Fields.Add(workflowField);
                    }

                    //Now add the custom properties
                    for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                    {
                        RiskWorkflowCustomProperty workflowCustomProperty = new RiskWorkflowCustomProperty();
                        workflowCustomProperty.CustomPropertyId = sourceWorkflow.CustomProperties[i].CustomPropertyId;
                        workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                        workflowCustomProperty.RiskStatusId = sourceWorkflow.CustomProperties[i].RiskStatusId;
                        destWorkflow.CustomProperties.Add(workflowCustomProperty);
                    }

                    //Save the new workflow
                    context.RiskWorkflows.AddObject(destWorkflow);
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
        /// <remarks>The associated risk types need to have been deleted first</remarks>
        protected internal void Workflow_DeleteAllForProjectTemplate(int projectTemplateId)
        {
            const string METHOD_NAME = "Workflow_DeleteAllForProjectTemplate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow
                    var query = from w in context.RiskWorkflows
                                where w.ProjectTemplateId == projectTemplateId
                                orderby w.RiskWorkflowId
                                select w;

                    List<RiskWorkflow> workflows = query.ToList();
                    for (int i = 0; i < workflows.Count; i++)
                    {
                        context.RiskWorkflows.DeleteObject(workflows[i]);
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
                    var query = from w in context.RiskWorkflows
                                where w.RiskWorkflowId == workflowId
                                select w;

                    RiskWorkflow workflow = query.FirstOrDefault();

                    //Make sure we have a workflow
                    if (workflow != null)
                    {
                        //If the workflow is marked as default, you can't delete it
                        if (workflow.IsDefault)
                        {
                            throw new WorkflowInUseException("You cannot delete a workflow that is marked as the default");
                        }

                        //Delete the workflow. The database cascades will handle the dependent entities
                        context.RiskWorkflows.DeleteObject(workflow);
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
