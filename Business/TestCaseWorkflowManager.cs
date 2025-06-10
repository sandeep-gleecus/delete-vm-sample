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
    /// the test case workflows that are used to determine the lifecycle of test cases in the system
    /// </summary>
    public class TestCaseWorkflowManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.TestCaseWorkflowManager::";

        #region Static Methods

        /// <summary>
        /// Converts testCase workflow field collection to the generic kind
        /// </summary>
        /// <param name="testCaseWorkflowCustomProps">The testCase workflow fields</param>
        /// <returns>The workflow fields</returns>
        public static List<WorkflowField> ConvertFields(List<TestCaseWorkflowField> testCaseWorkflowFields)
        {
            //Handle null case safely
            if (testCaseWorkflowFields == null)
            {
                return null;
            }

            List<WorkflowField> workflowFields = new List<WorkflowField>();
            foreach (TestCaseWorkflowField testCaseWorkflowField in testCaseWorkflowFields)
            {
                WorkflowField workflowField = new WorkflowField();
                workflowField.ArtifactFieldId = testCaseWorkflowField.ArtifactFieldId;
                workflowField.IncidentStatusId = testCaseWorkflowField.TestCaseStatusId;
                workflowField.WorkflowId = testCaseWorkflowField.TestCaseWorkflowId;
                workflowField.WorkflowFieldStateId = testCaseWorkflowField.WorkflowFieldStateId;
                workflowField.Field = new ArtifactField();
                workflowField.Field.Name = testCaseWorkflowField.ArtifactField.Name;
                workflowFields.Add(workflowField);
            }

            return workflowFields;
        }

        /// <summary>
        /// Converts testCase workflow custom properties collection to the generic kind
        /// </summary>
        /// <param name="testCaseWorkflowCustomProps">The testCase workflow custom properties</param>
        /// <returns>The workflow custom properties</returns>
        public static List<WorkflowCustomProperty> ConvertFields(List<TestCaseWorkflowCustomProperty> testCaseWorkflowCustomProps)
        {
            //Handle null case safely
            if (testCaseWorkflowCustomProps == null)
            {
                return null;
            }

            List<WorkflowCustomProperty> workflowCustomProperties = new List<WorkflowCustomProperty>();
            foreach (TestCaseWorkflowCustomProperty testCaseWorkflowCustomProp in testCaseWorkflowCustomProps)
            {
                WorkflowCustomProperty workflowCustomProperty = new WorkflowCustomProperty();
                workflowCustomProperty.CustomPropertyId = testCaseWorkflowCustomProp.CustomPropertyId;
                workflowCustomProperty.IncidentStatusId = testCaseWorkflowCustomProp.TestCaseStatusId;
                workflowCustomProperty.WorkflowId = testCaseWorkflowCustomProp.TestCaseWorkflowId;
                workflowCustomProperty.WorkflowFieldStateId = testCaseWorkflowCustomProp.WorkflowFieldStateId;
                workflowCustomProperty.CustomProperty = new CustomProperty();
                workflowCustomProperty.CustomProperty.PropertyNumber = testCaseWorkflowCustomProp.CustomProperty.PropertyNumber;
                workflowCustomProperties.Add(workflowCustomProperty);
            }

            return workflowCustomProperties;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Inserts a new workflow into the system with all the standard transitions and field states
        /// </summary>
        /// <param name="projectTemplateId">The project template the workflow is associated with</param>
        /// <param name="name">The name of the workflow</param>
        /// <param name="isDefault">Is this the default workflow for the project</param>
        /// <param name="isActive">Is this workflow active</param>
        /// <returns>The newly inserted workflow</returns>
        internal TestCaseWorkflow Workflow_InsertWithDefaultEntries(int projectTemplateId, string name, bool isDefault, bool isActive = true)
        {
            const string METHOD_NAME = "Workflow_InsertWithDefaultEntries";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First create the workflow itself
                TestCaseWorkflow workflow = this.Workflow_Insert(projectTemplateId, name, isDefault, isActive);
                int workflowId = workflow.TestCaseWorkflowId;

                //Workflow Transitions

                int workflowTransitionId1 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.Draft, (int)TestCase.TestCaseStatusEnum.ReadyForReview, "Review Test Case", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId2 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.Draft, (int)TestCase.TestCaseStatusEnum.Rejected, "Reject Test Case", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId3 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForReview, (int)TestCase.TestCaseStatusEnum.Approved, "Approve Test Case", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId4 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForReview, (int)TestCase.TestCaseStatusEnum.Rejected, "Reject Test Case", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId5 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForReview, (int)TestCase.TestCaseStatusEnum.Draft, "Return to Draft", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId6 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.Approved, (int)TestCase.TestCaseStatusEnum.ReadyForTest, "Start Testing", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId7 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.Approved, (int)TestCase.TestCaseStatusEnum.ReadyForReview, "Return to Review", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId8 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.Rejected, (int)TestCase.TestCaseStatusEnum.ReadyForReview, "Return to Review", true, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId9 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForTest, (int)TestCase.TestCaseStatusEnum.Obsolete, "Retire Test Case", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId10 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForTest, (int)TestCase.TestCaseStatusEnum.Approved, "Pause Testing", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;
                int workflowTransitionId11 = WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.Obsolete, (int)TestCase.TestCaseStatusEnum.ReadyForTest, "Return to Review", false, true, new List<int> { (int)ProjectManager.ProjectRoleProjectOwner, (int)ProjectManager.ProjectRoleManager }).WorkflowTransitionId;

                //Workflow Fields

                //All fields are active, visible and not-required by default, so only need to populate the ones
                //that are exceptions to that case

                //None for test case workflows

                //Return the workflow
                Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
                return workflow;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

		#endregion

		#region Public Methods


		public List<TestCaseStatusResponse> WorkflowTransition_RetrieveByInputStatusById(int workflowId, int inputTestCaseStatusId, int projectTemplateId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatusById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.TestCaseWorkflowTransitions.Include("InputTestCaseStatus").Include("OutputTestCaseStatus")
								join i in context.TestCaseStati
								on t.InputTestCaseStatusId equals i.TestCaseStatusId
								join o in context.TestCaseStati
								on t.OutputTestCaseStatusId equals o.TestCaseStatusId
								where t.OutputTestCaseStatus.IsActive && t.TestCaseWorkflowId == workflowId && t.InputTestCaseStatusId == inputTestCaseStatusId
								orderby t.Name, t.WorkflowTransitionId
								select new TestCaseStatusResponse
								{
									InputTestCaseStatusId = t.InputTestCaseStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									TestCaseWorkflowId = t.TestCaseWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
									URL = "",
									OutURL = "",
									InURL = "",
									OutputTestCaseStatusId = t.OutputTestCaseStatusId,
								};

					var workflows = query.ToList();

					foreach (var c in workflows)
					{
						c.InURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/TestCaseWorkflowStep.aspx?workflowId=" + workflowId + "&testCaseStatusId=" + c.InputTestCaseStatusId;
						c.URL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/TestCaseWorkflowTransition.aspx?workflowId=" + workflowId + "&workflowTransitionId=" + c.WorkflowTransitionId;
						c.OutURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/TestCaseWorkflowStep.aspx?workflowId=" + workflowId + "&testCaseStatusId=" + c.OutputTestCaseStatusId;
					}

					query = workflows.AsQueryable();

					List<TestCaseStatusResponse> workflowTransitions = query.ToList();

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

		public List<TestCaseStatusResponse> WorkflowTransition_RetrieveAllStatuses(int workflowId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.TestCaseWorkflowTransitions.Include("InputTestCaseStatus").Include("OutputTestCaseStatus")
								join i in context.TestCaseStati
								on t.InputTestCaseStatusId equals i.TestCaseStatusId
								join o in context.TestCaseStati
								on t.OutputTestCaseStatusId equals o.TestCaseStatusId
								where t.OutputTestCaseStatus.IsActive && t.TestCaseWorkflowId == workflowId
								orderby i.Position, t.WorkflowTransitionId
								select new TestCaseStatusResponse
								{
									InputTestCaseStatusId = t.InputTestCaseStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByCreator,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									TestCaseWorkflowId = t.TestCaseWorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyCreator,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
								};

					List<TestCaseStatusResponse> workflowTransitions = query.ToList();

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
		/// Updates the workflow fields and custom properties associated with a particular testCase status
		/// </summary>
		/// <param name="status">The status whose fields and custom properties we want to update</param>
		public void Status_UpdateFieldsAndCustomProperties(TestCaseStatus status)
        {
            const string METHOD_NAME = "Status_UpdateFieldsAndCustomProperties";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Attach the changed entity to the EF context and persist changes
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    context.TestCaseStati.ApplyChanges(status);
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

        /// <summary>Retrieves the testCase status by the given ID.</summary>
        /// <param name="testCaseStatusId">The status ID to retrieve.</param>
        /// <returns>The TestCaseStatus, or null if not found.</returns>
        /// <remarks>Will return deleted items.</remarks>
        /// <param name="includeWorkflowFieldsForWorkflowId">Should we include the linked workflow fields</param>
        public TestCaseStatus Status_RetrieveById(int testCaseStatusId, int? includeWorkflowFieldsForWorkflowId = null)
        {
            const string METHOD_NAME = "Status_RetrieveById()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestCaseStatus retStatus = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from r in context.TestCaseStati
                                where r.TestCaseStatusId == testCaseStatusId
                                select r;

                    retStatus = query.FirstOrDefault();

                    //Get the fields and custom properties, joined by EF 'fix-up'
                    if (includeWorkflowFieldsForWorkflowId.HasValue)
                    {
                        int workflowId = includeWorkflowFieldsForWorkflowId.Value;
                        var query2 = from w in context.TestCaseWorkflowFields
                                     where w.TestCaseStatusId == testCaseStatusId && w.TestCaseWorkflowId == workflowId && w.ArtifactField.IsActive
                                     select w;
                        query2.ToList();
                        var query3 = from w in context.TestCaseWorkflowCustomProperties
                                     where w.TestCaseStatusId == testCaseStatusId && w.TestCaseWorkflowId == workflowId && !w.CustomProperty.IsDeleted
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
		/// <param name="projectTemplateId">The project we're interested in</param>
		/// <returns>The default workflow</returns>
        public TestCaseWorkflow Workflow_GetDefault(int projectTemplateId)
		{
            const string METHOD_NAME = "Workflow_GetDefault";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from w in context.TestCaseWorkflows
                                where w.IsActive && w.IsDefault && w.ProjectTemplateId == projectTemplateId
                                select w;

                    TestCaseWorkflow workflow = query.FirstOrDefault();
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
		/// Gets the workflow for a specific testCase type
		/// </summary>
		/// <param name="testCaseTypeId">The testCase type we're interested in</param>
		/// <returns>The workflow id</returns>
        public int Workflow_GetForTestCaseType(int testCaseTypeId)
		{
            const string METHOD_NAME = "Workflow_GetForTestCaseType";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow by the product
                    var query = from r in context.TestCaseTypes.Include (t => t.Workflow)
                                where r.TestCaseTypeId == testCaseTypeId
                                select r;

                    TestCaseType testCaseType = query.FirstOrDefault();
                    if (testCaseType == null || testCaseType.Workflow == null)
                    {
                        throw new ArtifactNotExistsException("Unable to retrieve workflow for testCase type " + testCaseTypeId + ".");
                    }
                    else
                    {
                        Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                        Logger.Flush();
                        return testCaseType.TestCaseWorkflowId;
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
		/// Retrieves the list of inactive, hidden and required fields for an testCase based on its position
		/// in the workflow.
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="testCaseStatusId">The current step (i.e. testCase status) in the workflow</param>
        /// <remarks>Workflow needs to be active</remarks>
		public List<TestCaseWorkflowField> Workflow_RetrieveFieldStates(int workflowId, int testCaseStatusId)
		{
            const string METHOD_NAME = "Workflow_RetrieveFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.TestCaseWorkflowFields.Include("FieldState").Include("ArtifactField")
                                where w.TestCaseWorkflowId == workflowId && w.Workflow.IsActive && w.ArtifactField.IsActive && w.TestCaseStatusId == testCaseStatusId
                                orderby w.WorkflowFieldStateId, w.ArtifactFieldId
                                select w;

                    List<TestCaseWorkflowField> workflowFields = query.ToList();
                    
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
        /// Retrieves the list of inactive, hidden and required custom properties for an testCase based on its position
        /// in the workflow.
        /// </summary>
        /// <param name="workflowId">The workflow we're using</param>
        /// <param name="testCaseStatusId">The current step (i.e. status) in the workflow</param>
        /// <returns>List of inactive, hidden and required custom properties for the current workflow step</returns>
        /// <remarks>Workflow needs to be active</remarks>
        public List<TestCaseWorkflowCustomProperty> Workflow_RetrieveCustomPropertyStates(int workflowId, int testCaseStatusId)
        {
            const string METHOD_NAME = "Workflow_RetrieveCustomPropertyStates";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow fields for the specified workflow and status
                    var query = from w in context.TestCaseWorkflowCustomProperties.Include("FieldState").Include("CustomProperty")
                                where w.TestCaseWorkflowId == workflowId &&
                                    w.Workflow.IsActive &&
                                    w.TestCaseStatusId == testCaseStatusId &&
                                    !w.CustomProperty.IsDeleted
                                orderby w.WorkflowFieldStateId, w.CustomPropertyId
                                select w;

                    List<TestCaseWorkflowCustomProperty> workflowCustomProps = query.ToList();

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
        /// Retrieves a workflow transition by its id, includes the transition roles and testCase status names
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="workflowTransitionId">The id of the transition</param>
        /// <returns></returns>
        public TestCaseWorkflowTransition WorkflowTransition_RetrieveById(int workflowId, int workflowTransitionId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestCaseWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TestCaseWorkflowTransitions.Include("InputTestCaseStatus").Include("OutputTestCaseStatus").Include("TransitionRoles").Include("TransitionRoles.Role")
                                where t.TestCaseWorkflowId == workflowId && t.WorkflowTransitionId == workflowTransitionId
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
        public TestCaseWorkflow Workflow_RetrieveById(int workflowId, bool includeTransitions = false, bool includeFieldStates = false)
        {
            const string METHOD_NAME = "Workflow_RetrieveById(int,[bool],[bool])";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See if we need to include transitions
                    ObjectQuery<TestCaseWorkflow> workflowSet = context.TestCaseWorkflows;
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
                                where w.TestCaseWorkflowId == workflowId
                                select w;

                    TestCaseWorkflow workflow = query.FirstOrDefault();
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
		/// Determines if a workflow is in use (i.e. associated with one or more testCase type)
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
                    //Get the workflow by the testCase type
                    var query = from it in context.TestCaseTypes
                                where it.TestCaseWorkflowId == workflowId
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
        public List<TestCaseWorkflow> Workflow_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
            const string METHOD_NAME = "Workflow_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestCaseWorkflow> workflows;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflows
                    var query = from w in context.TestCaseWorkflows
                                where (!activeOnly || w.IsActive) && w.ProjectTemplateId == projectTemplateId
                                select w;

                    //For the active case (i.e. a lookup) order by name, otherwise in the admin case, order by id
                    if (activeOnly)
                    {
                        query = query.OrderBy(w => w.Name).ThenBy(w => w.TestCaseWorkflowId);
                    }
                    else
                    {
                        query = query.OrderBy(w => w.TestCaseWorkflowId);
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

		public List<TestCaseApprovalUser> TestCaseApprovalUsersByProjectId(int transitionId, int projectId)
		{
			const string METHOD_NAME = "TestCaseApprovalUsersByProjectId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				List<TestCaseApprovalUser> approvalUsers = new List<TestCaseApprovalUser>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.TestCaseApprovalUsers
								where t.IsActive && t.TransitionId == transitionId && t.ProjectId == projectId
								select t;
					query = query.OrderBy(t => t.OrderId);

					approvalUsers = query.ToList();

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

		public List<TestCaseApprovalUser> WorkflowTransition_RetrieveIsRequiredApproversByInputStatus(int workflowId, int inputTestCaseStatusId, int outputTestCaseStatusId, int projectId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveApproversByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				List<TestCaseApprovalUser> approvalUsers = new List<TestCaseApprovalUser>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.TestCaseWorkflowTransitions
								where t.OutputTestCaseStatus.IsActive &&  t.TestCaseWorkflowId == workflowId && t.InputTestCaseStatusId == inputTestCaseStatusId && t.OutputTestCaseStatusId == outputTestCaseStatusId
								select t;

					var esignature = query.Where(x => x.IsSignatureRequired == true).FirstOrDefault();
					if (esignature != null)
					{
						approvalUsers = context.TestCaseApprovalUsers.Where(x => x.IsActive && x.ProjectId == projectId && x.TransitionId == esignature.WorkflowTransitionId).GroupBy(x => x.UserId).Select(y => y.FirstOrDefault()).OrderBy(x => x.OrderId).OrderByDescending(x => x.UpdateDate).ToList();
						//approvalUsers = approvalUsers.Select(x => x.UserId).Distinct();
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

		public List<TestCaseApprovalUser> WorkflowTransition_RetrieveApproversByInputStatus(int workflowId, int inputTestCaseStatusId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveApproversByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				List<TestCaseApprovalUser> approvalUsers = new List<TestCaseApprovalUser>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.TestCaseWorkflowTransitions
								where t.OutputTestCaseStatus.IsActive && t.TestCaseWorkflowId == workflowId && t.InputTestCaseStatusId == inputTestCaseStatusId
								select t;

					var esignature = query.Where(x => x.IsSignatureRequired).FirstOrDefault();
					if(esignature != null)
					{
						approvalUsers = context.TestCaseApprovalUsers.Where(x => x.TransitionId == esignature.WorkflowTransitionId && x.IsActive).ToList();
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

		public List<int> WorkflowTransition_RetrieveApproversByInputStatus1(int workflowId, int inputTestCaseStatusId, int projectId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveApproversByInputStatus1";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				// Initialize approvalUsers as a flat list for storing unique user IDs
				List<int> approvalUserIds = new List<int>();  // Assuming UserId is of type int

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					// Get the workflow transitions
					var query = from t in context.TestCaseWorkflowTransitions
								where t.OutputTestCaseStatus.IsActive
									  && t.TestCaseWorkflowId == workflowId
									  && t.InputTestCaseStatusId == inputTestCaseStatusId
								select t;

					var esignature = query.Where(x => x.IsSignatureRequired).ToList();

					// Check if there are any signature-required transitions
					if (esignature != null && esignature.Any())  // Fix the null check and ensure the list is not empty
					{
						foreach (var esign in esignature)
						{
							// Get the approval users for each signature-required transition
							List<TestCaseApprovalUser> eusers = context.TestCaseApprovalUsers
								.Where(x => x.TransitionId == esign.WorkflowTransitionId && x.IsActive && x.ProjectId == projectId)
								.ToList();

							// Add the UserId of each approval user to the approvalUserIds list
							approvalUserIds.AddRange(eusers.Select(user => user.UserId));  // Select only UserId
						}

						// Remove duplicates by using Distinct()
						approvalUserIds = approvalUserIds.Distinct().ToList();  // Ensure uniqueness
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}

				return approvalUserIds;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

		}

		public List<int> WorkflowTransition_RetrieveApproversByOutputStatus1(int workflowId, int outputTestCaseStatusId, int projectId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveApproversByInputStatus1";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				// Initialize approvalUsers as a flat list for storing unique user IDs
				List<int> approvalUserIds = new List<int>();  // Assuming UserId is of type int

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					// Get the workflow transitions
					var query = from t in context.TestCaseWorkflowTransitions
								where t.OutputTestCaseStatus.IsActive
									  && t.TestCaseWorkflowId == workflowId
									  && t.OutputTestCaseStatusId == outputTestCaseStatusId
								select t;

					var esignature = query.Where(x => x.IsSignatureRequired).ToList();

					// Check if there are any signature-required transitions
					if (esignature != null && esignature.Any())  // Fix the null check and ensure the list is not empty
					{
						foreach (var esign in esignature)
						{
							// Get the approval users for each signature-required transition
							List<TestCaseApprovalUser> eusers = context.TestCaseApprovalUsers
								.Where(x => x.TransitionId == esign.WorkflowTransitionId && x.IsActive && x.ProjectId == projectId)
								.ToList();

							// Add the UserId of each approval user to the approvalUserIds list
							approvalUserIds.AddRange(eusers.Select(user => user.UserId));  // Select only UserId
						}

						// Remove duplicates by using Distinct()
						approvalUserIds = approvalUserIds.Distinct().ToList();  // Ensure uniqueness
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}

				return approvalUserIds;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

		}

		public List<int> WorkflowTransition_RetrieveApproversIdByInputStatus(int workflowId, int inputTestCaseStatusId, int outputTestCaseStatusId, int projectId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveApproversIdByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				// Initialize approvalUsers as a flat list for storing unique user IDs
				List<int> approvalUserIds = new List<int>();  // Assuming UserId is of type int

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					// Get the workflow transitions
					var query = from t in context.TestCaseWorkflowTransitions
								where t.OutputTestCaseStatus.IsActive
									  && t.TestCaseWorkflowId == workflowId
									  && t.InputTestCaseStatusId == inputTestCaseStatusId
									  && t.OutputTestCaseStatusId == outputTestCaseStatusId
								select t;

					var esignature = query.Where(x => x.IsSignatureRequired).ToList();

					// Check if there are any signature-required transitions
					if (esignature != null && esignature.Any())  // Fix the null check and ensure the list is not empty
					{
						foreach (var esign in esignature)
						{
							// Get the approval users for each signature-required transition
							List<TestCaseApprovalUser> eusers = context.TestCaseApprovalUsers
								.Where(x => x.TransitionId == esign.WorkflowTransitionId && x.IsActive && x.ProjectId == projectId)
								.ToList();

							// Add the UserId of each approval user to the approvalUserIds list
							approvalUserIds.AddRange(eusers.Select(user => user.UserId));  // Select only UserId
						}

						// Remove duplicates by using Distinct()
						approvalUserIds = approvalUserIds.Distinct().ToList();  // Ensure uniqueness
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}

				return approvalUserIds;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

		}

		/// <summary>
		/// Gets the list of workflow transitions for the specified testCase status where the user has a specific role or is the author/owner
		/// </summary>
		/// <param name="inputTestCaseStatusId">The input status</param>
		/// <param name="workflowId">The workflow we're using</param>
		/// <returns>List of matching workflow transitions</returns>
		/// <param name="isAuthor">Are we the author of the testCase</param>
		/// <param name="isOwner">Are we the owner of the testCase</param>
		/// <param name="projectRoleId">What project role are we</param>
		/// <remarks>This overload considers user permissions/roles when returning the list</remarks>
		public List<TestCaseWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputTestCaseStatusId, int projectRoleId, bool isAuthor, bool isOwner)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TestCaseWorkflowTransitions.Include("InputTestCaseStatus").Include("OutputTestCaseStatus")
                                where t.OutputTestCaseStatus.IsActive && t.TestCaseWorkflowId == workflowId && t.InputTestCaseStatusId == inputTestCaseStatusId
                                select t;

                    //Create the expresion if we're the author, owner or in one of the roles allowed to execute the transition
                    List<Expression<Func<TestCaseWorkflowTransition, bool>>> expressionList = new List<Expression<Func<TestCaseWorkflowTransition, bool>>>();
                    if (isAuthor)
                    {
                        Expression<Func<TestCaseWorkflowTransition, bool>> clause = t => t.IsExecuteByCreator;
                        expressionList.Add(clause);
                    }
                    if (isOwner)
                    {
                        Expression<Func<TestCaseWorkflowTransition, bool>> clause = t => t.IsExecuteByOwner;
                        expressionList.Add(clause);
                    }

                    //Now the project role
                    Expression<Func<TestCaseWorkflowTransition, bool>> clause2 = t => t.TransitionRoles.Any(i => i.Role.ProjectRoleId == projectRoleId && i.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute);
                    expressionList.Add(clause2);

                    if (expressionList.Count > 0)
                    {
                        //OR together the different cases and apply to the where clause
                        Expression<Func<TestCaseWorkflowTransition, bool>> aggregatedClause = Manager.BuildOr(expressionList.ToArray());
                        query = query.Where(aggregatedClause);
                    }

                    //Add the sorts
                    query = query.OrderBy(t => t.Name).ThenBy(t => t.WorkflowTransitionId);

                    List<TestCaseWorkflowTransition> workflowTransitions = query.ToList();

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
        /// <param name="inputTestCaseStatusId">The input status</param>
        /// <param name="outputTestCaseStatusId">The output status</param>
        /// <param name="workflowId">The workflow we're using</param>
        /// <returns>The workflow transition</returns>
        public TestCaseWorkflowTransition WorkflowTransition_RetrieveByStatuses(int workflowId, int inputTestCaseStatusId, int outputTestCaseStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TestCaseWorkflowTransitions.Include("InputTestCaseStatus").Include("OutputTestCaseStatus")
                                where t.TestCaseWorkflowId == workflowId &&
                                      t.InputTestCaseStatusId == inputTestCaseStatusId &&
                                      t.OutputTestCaseStatusId == outputTestCaseStatusId
                                select t;

                    TestCaseWorkflowTransition workflowTransition = query.FirstOrDefault();

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
        /// Gets the list of workflow transitions for the specified testCase status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="inputTestCaseStatusId">The id of the input status</param>
        /// <returns>List of transitions</returns>
        public List<TestCaseWorkflowTransition> WorkflowTransition_RetrieveByInputStatus(int workflowId, int inputTestCaseStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TestCaseWorkflowTransitions.Include("InputTestCaseStatus").Include("OutputTestCaseStatus")
                                where t.OutputTestCaseStatus.IsActive && t.TestCaseWorkflowId == workflowId && t.InputTestCaseStatusId == inputTestCaseStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<TestCaseWorkflowTransition> workflowTransitions = query.ToList();

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
        /// Gets the list of workflow transitions for the specified testCase output status
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="outputTestCaseStatusId">The id of the output status</param>
        /// <returns>List of transitions</returns>
        public List<TestCaseWorkflowTransition> WorkflowTransition_RetrieveByOutputStatus(int workflowId, int outputTestCaseStatusId)
        {
            const string METHOD_NAME = "WorkflowTransition_RetrieveByOutputStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the workflow transitions
                    var query = from t in context.TestCaseWorkflowTransitions.Include("InputTestCaseStatus").Include("OutputTestCaseStatus")
                                where t.OutputTestCaseStatus.IsActive && t.TestCaseWorkflowId == workflowId && t.OutputTestCaseStatusId == outputTestCaseStatusId
                                orderby t.Name, t.WorkflowTransitionId
                                select t;

                    List<TestCaseWorkflowTransition> workflowTransitions = query.ToList();

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
        public void Workflow_Update(TestCaseWorkflow workflow)
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
            if (!workflow.IsActive && IsInUse(workflow.TestCaseWorkflowId))
            {
                throw new WorkflowInUseException("You cannot make a workflow that is in use inactive");
            }

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.TestCaseWorkflows.ApplyChanges(workflow);
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
        public TestCaseWorkflow Workflow_Insert(int projectTemplateId, string name, bool isDefault, bool isActive = true)
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
                TestCaseWorkflow workflow;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow entity
                    workflow = new TestCaseWorkflow();
                    workflow.ProjectTemplateId = projectTemplateId;
                    workflow.Name = name;
                    workflow.IsActive = isActive;
                    workflow.IsDefault = isDefault;

                    //Persist the new workflow
                    context.TestCaseWorkflows.AddObject(workflow);
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
        /// <param name="inputTestCaseStatusId">The input testCase status for the transition</param>
        /// <param name="outputTestCaseStatusId">The output testCase status for the transition</param>
        /// <param name="name">The name of the transition</param>
        /// <param name="executeByCreator"></param>
        /// <param name="executeByOwner"></param>
        /// <param name="roles">The list of any roles that can also execute the transition</param>
        /// <returns>The new workflow transition</returns>
        public TestCaseWorkflowTransition WorkflowTransition_Insert(int workflowId, int inputTestCaseStatusId, int outputTestCaseStatusId, string name, bool executeByCreator = false, bool executeByOwner = true, List<int> roles = null)
        {
            const string METHOD_NAME = "Workflow_Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestCaseWorkflowTransition workflowTransition;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new workflow transition entity
                    workflowTransition = new TestCaseWorkflowTransition();
                    workflowTransition.Name = name;
                    workflowTransition.TestCaseWorkflowId = workflowId;
                    workflowTransition.InputTestCaseStatusId = inputTestCaseStatusId;
                    workflowTransition.OutputTestCaseStatusId = outputTestCaseStatusId;
                    workflowTransition.IsExecuteByOwner = executeByOwner;
                    workflowTransition.IsExecuteByCreator = executeByCreator;

                    //Add the transition roles if any provided
                    if (roles != null && roles.Count > 0)
                    {
                        foreach (int projectRoleId in roles)
                        {
                            //For testCase workflows, all transition roles are execute transitions (not notify)
                            TestCaseWorkflowTransitionRole transitionRole = new TestCaseWorkflowTransitionRole();
                            transitionRole.ProjectRoleId = projectRoleId;
                            transitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
                            workflowTransition.TransitionRoles.Add(transitionRole);
                        }
                    }

                    //Persist the new workflow transition
                    context.TestCaseWorkflowTransitions.AddObject(workflowTransition);
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
                    var query = from wt in context.TestCaseWorkflowTransitions.Include("TransitionRoles")
                                where wt.WorkflowTransitionId == workflowTransitionId
                                select wt;

                    TestCaseWorkflowTransition workflowTransition = query.FirstOrDefault();

                    //Make sure we have a workflow transition
                    if (workflowTransition != null)
                    {
                        //Delete the workflow transition and dependent entities
                        context.TestCaseWorkflowTransitions.DeleteObject(workflowTransition);
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
        public void WorkflowTransition_Update(TestCaseWorkflowTransition workflowTransition)
        {
            const string METHOD_NAME = CLASS_NAME + "WorkflowTransition_Update";

            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    context.TestCaseWorkflowTransitions.ApplyChanges(workflowTransition);
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
        public TestCaseWorkflow Workflow_Copy(int sourceWorkflowId)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestCaseWorkflow destWorkflow = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow and associated transitions, fields and custom properties
                    var query = from w in context.TestCaseWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties")
                                where w.TestCaseWorkflowId == sourceWorkflowId
                                select w;

                    TestCaseWorkflow sourceWorkflow = query.FirstOrDefault();
                    if (sourceWorkflow == null)
                    {
                        throw new ArtifactNotExistsException("The workflow being copied no longer exists");
                    }

                    //Create the copied workflow
                    //except that we will always insert it with default=No
                    destWorkflow = new TestCaseWorkflow();
                    destWorkflow.ProjectTemplateId = sourceWorkflow.ProjectTemplateId;
                    destWorkflow.Name = GlobalResources.General.Global_CopyOf + " " + sourceWorkflow.Name;
                    destWorkflow.IsActive = sourceWorkflow.IsActive;
                    destWorkflow.IsDefault = false;

                    //Now add the transitions (including the transition roles)
                    for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                    {
                        TestCaseWorkflowTransition workflowTransition = new TestCaseWorkflowTransition();
                        workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                        workflowTransition.InputTestCaseStatusId = sourceWorkflow.Transitions[i].InputTestCaseStatusId;
                        workflowTransition.OutputTestCaseStatusId = sourceWorkflow.Transitions[i].OutputTestCaseStatusId;
                        workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                        workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                        workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                        destWorkflow.Transitions.Add(workflowTransition);

                        //Now the roles
                        for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                        {
                            TestCaseWorkflowTransitionRole workflowTransitionRole = new TestCaseWorkflowTransitionRole();
                            workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                            workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                            workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                        }
                    }

                    //Now add the fields
                    for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                    {
                        TestCaseWorkflowField workflowField = new TestCaseWorkflowField();
                        workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                        workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                        workflowField.TestCaseStatusId = sourceWorkflow.Fields[i].TestCaseStatusId;
                        destWorkflow.Fields.Add(workflowField);
                    }

                    //Now add the custom properties
                    for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                    {
                        TestCaseWorkflowCustomProperty workflowCustomProperty = new TestCaseWorkflowCustomProperty();
                        workflowCustomProperty.CustomPropertyId = sourceWorkflow.CustomProperties[i].CustomPropertyId;
                        workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                        workflowCustomProperty.TestCaseStatusId = sourceWorkflow.CustomProperties[i].TestCaseStatusId;
                        destWorkflow.CustomProperties.Add(workflowCustomProperty);
                    }

                    //Save the new workflow
                    context.TestCaseWorkflows.AddObject(destWorkflow);
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
        /// <param name="testCaseWorkflowMapping">The mapping of workflow ids between the old and new templates</param>
        /// <param name="sourceProjectTemplateId">The project template to copy the workflows from</param>
        /// <param name="destProjectTemplateId">The destination project template</param>
        /// <param name="customPropertyIdMapping">The mapping of old vs. new custom properties between the two project templates</param>
        public void Workflow_Copy(int sourceProjectTemplateId, int destProjectTemplateId, Dictionary<int, int> customPropertyIdMapping, Dictionary<int, int> testCaseWorkflowMapping)
        {
            const string METHOD_NAME = "Workflow_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Dictionary<int, TestCaseWorkflow> tempMapping = new Dictionary<int, TestCaseWorkflow>();
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflows and associated transitions, fields testCase types, and custom properties
                    var query = from w in context.TestCaseWorkflows.Include("Transitions").Include("Transitions.TransitionRoles").Include("Fields").Include("CustomProperties").Include("TestCaseTypes")
                                where w.ProjectTemplateId == sourceProjectTemplateId
                                orderby w.TestCaseWorkflowId
                                select w;

                    List<TestCaseWorkflow> sourceWorkflows = query.ToList();

                    //Loop through the various workflows
                    foreach (TestCaseWorkflow sourceWorkflow in sourceWorkflows)
                    {
                        //Create the copied workflow
                        TestCaseWorkflow destWorkflow = new TestCaseWorkflow();
                        destWorkflow.ProjectTemplateId = destProjectTemplateId;
                        destWorkflow.Name = sourceWorkflow.Name;
                        destWorkflow.IsActive = sourceWorkflow.IsActive;
                        destWorkflow.IsDefault = sourceWorkflow.IsDefault;

                        //Add to mapping
                        tempMapping.Add(sourceWorkflow.TestCaseWorkflowId, destWorkflow);

                        //Now add the transitions (including the transition roles)
                        for (int i = 0; i < sourceWorkflow.Transitions.Count; i++)
                        {
                            TestCaseWorkflowTransition workflowTransition = new TestCaseWorkflowTransition();
                            workflowTransition.Name = sourceWorkflow.Transitions[i].Name;
                            workflowTransition.InputTestCaseStatusId = sourceWorkflow.Transitions[i].InputTestCaseStatusId;
                            workflowTransition.OutputTestCaseStatusId = sourceWorkflow.Transitions[i].OutputTestCaseStatusId;
                            workflowTransition.IsExecuteByCreator = sourceWorkflow.Transitions[i].IsExecuteByCreator;
                            workflowTransition.IsExecuteByOwner = sourceWorkflow.Transitions[i].IsExecuteByOwner;
                            workflowTransition.IsSignatureRequired = sourceWorkflow.Transitions[i].IsSignatureRequired;
                            destWorkflow.Transitions.Add(workflowTransition);

                            //Now the roles
                            for (int j = 0; j < sourceWorkflow.Transitions[i].TransitionRoles.Count; j++)
                            {
                                TestCaseWorkflowTransitionRole workflowTransitionRole = new TestCaseWorkflowTransitionRole();
                                workflowTransitionRole.WorkflowTransitionRoleTypeId = sourceWorkflow.Transitions[i].TransitionRoles[j].WorkflowTransitionRoleTypeId;
                                workflowTransitionRole.ProjectRoleId = sourceWorkflow.Transitions[i].TransitionRoles[j].ProjectRoleId;
                                workflowTransition.TransitionRoles.Add(workflowTransitionRole);
                            }
                        }

                        //Now add the fields
                        for (int i = 0; i < sourceWorkflow.Fields.Count; i++)
                        {
                            TestCaseWorkflowField workflowField = new TestCaseWorkflowField();
                            workflowField.ArtifactFieldId = sourceWorkflow.Fields[i].ArtifactFieldId;
                            workflowField.WorkflowFieldStateId = sourceWorkflow.Fields[i].WorkflowFieldStateId;
                            workflowField.TestCaseStatusId = sourceWorkflow.Fields[i].TestCaseStatusId;
                            destWorkflow.Fields.Add(workflowField);
                        }

                        //Now add the custom properties
                        for (int i = 0; i < sourceWorkflow.CustomProperties.Count; i++)
                        {
                            //Get the corresponding custom property in the new project
                            if (customPropertyIdMapping.ContainsKey(sourceWorkflow.CustomProperties[i].CustomPropertyId))
                            {
                                int destCustomPropertyId = customPropertyIdMapping[sourceWorkflow.CustomProperties[i].CustomPropertyId];
                                TestCaseWorkflowCustomProperty workflowCustomProperty = new TestCaseWorkflowCustomProperty();
                                workflowCustomProperty.CustomPropertyId = destCustomPropertyId;
                                workflowCustomProperty.WorkflowFieldStateId = sourceWorkflow.CustomProperties[i].WorkflowFieldStateId;
                                workflowCustomProperty.TestCaseStatusId = sourceWorkflow.CustomProperties[i].TestCaseStatusId;
                                destWorkflow.CustomProperties.Add(workflowCustomProperty);
                            }
                        }

                        //Add the new workflow
                        context.TestCaseWorkflows.AddObject(destWorkflow);
                    }

                    //Save the changes
                    context.SaveChanges();
                }

                //Finally populate the external mapping dictionary (id based)
                foreach (KeyValuePair<int, TestCaseWorkflow> kvp in tempMapping)
                {
                    testCaseWorkflowMapping.Add(kvp.Key, kvp.Value.TestCaseWorkflowId);
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

		//TODO: UPDATE-TESTCASE
		public List<ProjectSignature> GetApproversForTestCase(int projectId)
		{
			const string METHOD_NAME = "GetApproversForTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			int artifact_type_id = 2;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the workflow
					var query = from w in context.ProjectSignatures
								where w.ProjectId == projectId && w.IsActive && w.ArtifactTypeId == artifact_type_id
								orderby w.OrderId
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

		public List<TestCaseApprovalUser> GetApproversForTestCaseWorkflowTransition(int projectId, int workflowTransitionId)
		{
			const string METHOD_NAME = "GetApproversForTestCaseWorkflowTransition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			 
			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the workflow
					var query = from w in context.TestCaseApprovalUsers
								where w.ProjectId == projectId && w.IsActive && w.TransitionId == workflowTransitionId && w.IsActive
								orderby w.OrderId
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

		public List<TestCaseApprovalUser> GetApproversToDeactive(int projectId, int userId)
		{
			const string METHOD_NAME = "GetApproversToDeactive";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the workflow
					var query = from w in context.TestCaseApprovalUsers
								where w.ProjectId == projectId && w.UserId == userId
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

		public TestCaseApprovalUser GetApproversForTestCaseWorkflowTransitionByUserId(int projectId, int workflowTransitionId, int userId)
		{
			const string METHOD_NAME = "GetApproversForTestCaseWorkflowTransition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the workflow
					var query = from w in context.TestCaseApprovalUsers
								where w.ProjectId == projectId && w.IsActive && w.TransitionId == workflowTransitionId && w.IsActive && w.UserId == userId
								orderby w.OrderId
								select w;

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

					return query.FirstOrDefault();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TestCaseApprovalUser GetApproversForTestCaseWorkflowTransitionByOrderId(int projectId, int workflowTransitionId, int orderId)
		{
			const string METHOD_NAME = "GetApproversForTestCaseWorkflowTransitionByOrderId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the workflow
					var query = from w in context.TestCaseApprovalUsers
								where w.ProjectId == projectId && w.IsActive && w.TransitionId == workflowTransitionId && w.IsActive && w.OrderId == orderId
								select w;

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

					return query.FirstOrDefault();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void UpdateApproversForTestCaseWorkflowTransition(List<TestCaseApprovalUser> updatedEntities)
		{

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				foreach (var item in updatedEntities)
				{
					context.TestCaseApprovalUsers.ApplyChanges(item);
				}

				context.SaveChanges();
			}
		}

		/// <summary>
		/// Deletes all the workflows in the project template  (called by the project template delete)
		/// </summary>
		/// <param name="projectTemplateId">The project to delete workflow for</param>
		/// <remarks>Does not delete the test case types, they need to be deleted first</remarks>
		protected internal void Workflow_DeleteAllForProjectTemplate(int projectTemplateId)
        {
            const string METHOD_NAME = "Workflow_DeleteAllForProjectTemplate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the workflow
                    var query = from w in context.TestCaseWorkflows
                                where w.ProjectTemplateId == projectTemplateId
                                orderby w.TestCaseWorkflowId
                                select w;

                    List<TestCaseWorkflow> workflows = query.ToList();
                    for (int i = 0; i < workflows.Count; i++)
                    {
                        context.TestCaseWorkflows.DeleteObject(workflows[i]);
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
                    var query = from w in context.TestCaseWorkflows
                                where w.TestCaseWorkflowId == workflowId
                                select w;

                    TestCaseWorkflow workflow = query.FirstOrDefault();

                    //Make sure we have a workflow
                    if (workflow != null)
                    {
                        //If the workflow is marked as default, you can't delete it
                        if (workflow.IsDefault)
                        {
                            throw new WorkflowInUseException("You cannot delete a workflow that is marked as the default");
                        }

                        //Delete the workflow. The database cascades will handle the dependent entities
                        context.TestCaseWorkflows.DeleteObject(workflow);
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
