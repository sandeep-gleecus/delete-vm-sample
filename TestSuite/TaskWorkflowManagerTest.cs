using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the TaskWorkflowManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class TaskWorkflowManagerTest
	{
		protected static TaskWorkflowManager workflowManager;
		protected static TaskWorkflow workflow;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static List<ArtifactField> currentArtifactFields = null;
		protected static List<CustomProperty> currentCustomProperties = null;
		protected static TaskStatus taskStatus;
		protected static int projectId;
		protected static int projectTemplateId;
		protected static int workflowId;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;


		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the class being tested
			workflowManager = new TaskWorkflowManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Create a new project for testing with (only some tests currently use this)
			//This will create the default workflow entries (for standard fields)
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("TaskWorkflowManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Need to add some custom properties for tasks in this new project
			CustomPropertyManager cpm = new CustomPropertyManager();
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Task, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "Difficulty", null, null, null);
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Task, (int)CustomProperty.CustomPropertyTypeEnum.User, 2, "Reviewer", null, null, null);

			//We need to load in the standard field and custom property states first
			currentArtifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Task);
			currentCustomProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		[
		Test,
		SpiraTestCase(1255)
		]
		public void _01_Retrieve_Lookups()
		{
			//Verify that we can get the default workflow for a project
			TaskWorkflow workflow = workflowManager.Workflow_GetDefault(projectTemplateId);
			Assert.AreEqual("Default Workflow", workflow.Name);
			Assert.AreEqual(true, workflow.IsActive);
			Assert.AreEqual(true, workflow.IsDefault);
			int defaultWorkflowId = workflow.TaskWorkflowId;

			TaskManager taskManager = new TaskManager();
			List<TaskType> types = taskManager.TaskType_Retrieve(projectTemplateId);

			//Verify that we can get the workflow associated with a specific type
			workflowId = workflowManager.Workflow_GetForTaskType(types[0].TaskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForTaskType(types[1].TaskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForTaskType(types[2].TaskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForTaskType(types[3].TaskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForTaskType(types[4].TaskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
		}

		[
		Test,
		SpiraTestCase(1256)
		]
		public void _02_Retrieve_Field_States()
		{
			//Get the default workflow for this project
			workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).TaskWorkflowId;

			//Load the field state for TaskStatus=NotStarted
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("ActualEffort"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(true, IsFieldHidden("ActualEffort"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("StartDate"));
			Assert.AreEqual(false, IsFieldDisabled("ActualEffort"));

			//Load the field state for TaskStatus=In-Progress
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.InProgress, workflowId);
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("ActualEffort"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("ActualEffort"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("StartDate"));
			Assert.AreEqual(false, IsFieldDisabled("ActualEffort"));

			//Load the field state for TaskStatus=Completed
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.Completed, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("StartDate"));
			Assert.AreEqual(true, IsFieldRequired("ActualEffort"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("ActualEffort"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("StartDate"));
			Assert.AreEqual(false, IsFieldDisabled("ActualEffort"));

			//Load the field state for TaskStatus=Deferred
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.Deferred, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("ActualEffort"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("ActualEffort"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("StartDate"));
			Assert.AreEqual(false, IsFieldDisabled("ActualEffort"));

			//Load the field state for TaskStatus=Blocked
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.Blocked, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("ActualEffort"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("ActualEffort"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("StartDate"));
			Assert.AreEqual(false, IsFieldDisabled("ActualEffort"));
		}

		[
		Test,
		SpiraTestCase(1257)
		]
		public void _03_Retrieve_Custom_Property_States()
		{
			//Load the custom property state for TaskStatus=NotStarted
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer

			//Load the custom property state for TaskStatus=InProgress
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.InProgress, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer

			//Load the custom property state for TaskStatus=Completed
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.Completed, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer

			//Load the custom property state for TaskStatus=Deferred
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.Deferred, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer

			//Load the custom property state for TaskStatus=Blocked
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.Blocked, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer
		}

		[
		Test,
		SpiraTestCase(1258)
		]
		public void _04_RetrievesTransitionsForStep()
		{
			//Verify the output transitions returned for a given input statuses (varies by role)
			//InputStatus=NotStarted, Role=Manager
			List<TaskWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Task.TaskStatusEnum.NotStarted, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Defer Task", workflowTransitions[0].Name);
			Assert.AreEqual("Start Task", workflowTransitions[1].Name);

			//InputStatus=NotStarted, Role=Developer
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Task.TaskStatusEnum.NotStarted, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, false);
			Assert.AreEqual(0, workflowTransitions.Count);

			//InputStatus=NotStarted, Role=Developer, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Task.TaskStatusEnum.NotStarted, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, true);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Defer Task", workflowTransitions[0].Name);
			Assert.AreEqual("Start Task", workflowTransitions[1].Name);

			//InputStatus=InProgress, Role=Manager
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Task.TaskStatusEnum.InProgress, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(4, workflowTransitions.Count);
			Assert.AreEqual("Block Task", workflowTransitions[0].Name);
			Assert.AreEqual("Complete Task", workflowTransitions[1].Name);
			Assert.AreEqual("Defer Task", workflowTransitions[2].Name);
			Assert.AreEqual("Restart Development", workflowTransitions[3].Name);

			//InputStatus=InProgress, Role=Developer, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Task.TaskStatusEnum.InProgress, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, true);
			Assert.AreEqual(4, workflowTransitions.Count);
			Assert.AreEqual("Block Task", workflowTransitions[0].Name);
			Assert.AreEqual("Complete Task", workflowTransitions[1].Name);
			Assert.AreEqual("Defer Task", workflowTransitions[2].Name);
			Assert.AreEqual("Restart Development", workflowTransitions[3].Name);

			//InputStatus=InProgress, Role=Developer, Creator=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Task.TaskStatusEnum.InProgress, InternalRoutines.PROJECT_ROLE_DEVELOPER, true, false);
			Assert.AreEqual(1, workflowTransitions.Count);
			Assert.AreEqual("Defer Task", workflowTransitions[0].Name);
		}

		[
		Test,
		SpiraTestCase(1259)
		]
		public void _05_Retrieves()
		{
			//Lets test that we can retrieve a workflow by id
			workflow = workflowManager.Workflow_RetrieveById(workflowId);

			//Verify data
			Assert.IsNotNull(workflow);
			Assert.AreEqual("Default Workflow", workflow.Name);
			Assert.AreEqual(true, workflow.IsDefault);
			Assert.AreEqual(true, workflow.IsActive);

			//Lets test that we can retrieve a transition based on input and output status
			//In Progress > Deferred
			TaskWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, (int)Task.TaskStatusEnum.InProgress, (int)Task.TaskStatusEnum.Deferred);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Defer Task", workflowTransition.Name);
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);

			//Lets test that we can retrieve a transition based on input and output status
			//InProgress > Completed
			workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, (int)Task.TaskStatusEnum.InProgress, (int)Task.TaskStatusEnum.Completed);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Complete Task", workflowTransition.Name);
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);
		}

		[
		Test,
		SpiraTestCase(1260)
		]
		public void _07_ViewModifyWorkflowStepFields()
		{
			//First verify that we can retrieve all the workflow-configurable fields for a task
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(15, artifactFields.Count);

			//Now lets verify that a particular field is required/active (Release)
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));

			//Now lets change the disabled setting for this field
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "ReleaseId").ArtifactFieldId;
			//Disabled=True
			taskStatus.StartTracking();
			TaskWorkflowField field = new TaskWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.TaskWorkflowId = workflowId;
			taskStatus.WorkflowFields.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(taskStatus);

			//Now refresh the data and verify
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsTrue(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));

			//Now lets change the required setting for this field
			//Required=True
			taskStatus.StartTracking();
			field = new TaskWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.TaskWorkflowId = workflowId;
			taskStatus.WorkflowFields.Add(field);
			//Disabled=False
			taskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.TaskWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(taskStatus);

			//Now refresh the data and verify
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsTrue(IsFieldRequired("ReleaseId"));

			//Finally change the data back
			taskStatus.StartTracking();
			taskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.TaskWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(taskStatus);

			//Now refresh the data and verify it was returned back
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));
		}

		[
		Test,
		SpiraTestCase(1261)
		]
		public void _08_ViewModifyWorkflowStepCustomProperties()
		{
			//First verify that we can retrieve all the custom properties for an task
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
			Assert.AreEqual(2, customProperties.Count);

			//Now lets verify that a particular property is required/active (Difficulty=Custom_01)
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=True
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			taskStatus.StartTracking();
			TaskWorkflowCustomProperty field = new TaskWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.TaskWorkflowId = workflowId;
			taskStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(taskStatus);

			//Now refresh the data and verify
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsTrue(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=False, Required=True
			taskStatus.StartTracking();
			field = new TaskWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.TaskWorkflowId = workflowId;
			taskStatus.WorkflowCustomProperties.Add(field);
			taskStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.TaskWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(taskStatus);

			//Now refresh the data and verify
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsTrue(IsCustomPropertyRequired("Custom_01"));

			//Finally change the data back
			taskStatus.StartTracking();
			taskStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.TaskWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(taskStatus);

			//Now refresh the data and verify that it's cleaned up
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));
		}

		[
		Test,
		SpiraTestCase(1262)
		]
		public void _09_ViewModifyWorkflowRoles()
		{
			//First lets get the various assigned roles for a specific transition
			//For this test we'll use the NotStarted > InProgress
			TaskWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			TaskWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputTaskStatusId == (int)Task.TaskStatusEnum.NotStarted && t.OutputTaskStatusId == (int)Task.TaskStatusEnum.InProgress);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			//Test that we can get the transition by its ID
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			List<TaskWorkflowTransitionRole> executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);

			//Now lets add a couple of roles
			workflowTransition.StartTracking();
			TaskWorkflowTransitionRole workflowTransitionRole = new TaskWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowTransitionRole = new TaskWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_MANAGER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Verify the changes
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual(4, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(3, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);
			Assert.AreEqual("Developer", executeRoles[2].Role.Name);

			//Now delete the ones we just added
			workflowTransition.StartTracking();
			workflowTransition.TransitionRoles.FirstOrDefault(t => t.ProjectRoleId == InternalRoutines.PROJECT_ROLE_DEVELOPER && t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).MarkAsDeleted();
			workflowTransition.TransitionRoles.FirstOrDefault(t => t.ProjectRoleId == InternalRoutines.PROJECT_ROLE_MANAGER && t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify).MarkAsDeleted();
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Verify the changes
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);
		}

		[
		Test,
		SpiraTestCase(1263)
		]
		public void _10_ViewModifyWorkflowTransitions()
		{
			//First lets retrieve a single workflow transition and verify its data
			//For this test we'll use the NotStarted > InProgress
			TaskWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			TaskWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputTaskStatusId == (int)Task.TaskStatusEnum.NotStarted && t.OutputTaskStatusId == (int)Task.TaskStatusEnum.InProgress);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Start Task
			Assert.AreEqual("Start Task", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByCreator");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwner");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Not Started", workflowTransition.InputTaskStatus.Name, "InputTaskStatusName");
			Assert.AreEqual("In Progress", workflowTransition.OutputTaskStatus.Name, "OutputTaskStatusName");

			//Now lets modify the data and then save the changes
			//Note that you cannot change the input and output task statuses
			workflowTransition.StartTracking();
			workflowTransition.Name = "Start Work";
			workflowTransition.IsExecuteByCreator = true;
			workflowTransition.IsExecuteByOwner = true;
			workflowTransition.IsSignatureRequired = true;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Start Task
			Assert.AreEqual("Start Work", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator, "ExecuteByDetector");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwner");
			Assert.AreEqual(true, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Not Started", workflowTransition.InputTaskStatus.Name, "InputTaskStatusName");
			Assert.AreEqual("In Progress", workflowTransition.OutputTaskStatus.Name, "OutputTaskStatusName");

			//Now lets return the data back to what it was before
			workflowTransition.StartTracking();
			workflowTransition.Name = "Start Task";
			workflowTransition.IsExecuteByCreator = false;
			workflowTransition.IsExecuteByOwner = true;
			workflowTransition.IsSignatureRequired = false;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Start Task
			Assert.AreEqual("Start Task", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetector");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwner");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Not Started", workflowTransition.InputTaskStatus.Name, "InputTaskStatusName");
			Assert.AreEqual("In Progress", workflowTransition.OutputTaskStatus.Name, "OutputTaskStatusName");

			//Now lets create a new workflow transition (Not Started > Completed)
			int newWorkflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId, (int)Task.TaskStatusEnum.NotStarted, (int)Task.TaskStatusEnum.Completed, "Mark As Complete", false, true, new List<int> { ProjectManager.ProjectRoleTester }).WorkflowTransitionId;

			//Verify that it was created
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, newWorkflowTransitionId);
			Assert.AreEqual("Mark As Complete", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetector");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwner");
			Assert.AreEqual("Not Started", workflowTransition.InputTaskStatus.Name, "InputTaskStatusName");
			Assert.AreEqual("Completed", workflowTransition.OutputTaskStatus.Name, "OutputTaskStatusName");

			//Verify that the Tester role was added
			Assert.AreEqual(1, workflowTransition.TransitionRoles.Count);
			Assert.AreEqual((int)ProjectManager.ProjectRoleTester, workflowTransition.TransitionRoles[0].ProjectRoleId);
			Assert.AreEqual("Tester", workflowTransition.TransitionRoles[0].Role.Name);

			//Now delete the transition
			workflowManager.WorkflowTransition_Delete(newWorkflowTransitionId);

			//Verify the delete
			bool isDeleted = false;
			try
			{
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, newWorkflowTransitionId);
			}
			catch (ArtifactNotExistsException)
			{
				isDeleted = true;
			}
			Assert.IsTrue(isDeleted);
		}

		[
		Test,
		SpiraTestCase(1264)
		]
		public void _11_ViewModifyWorkflows()
		{
			//First lets test that we can create a new workflow
			int workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test Workflow", true, true).TaskWorkflowId;

			//Now verify that it inserted correctly
			workflow = workflowManager.Workflow_RetrieveById(workflowId2);
			Assert.AreEqual("Test Workflow", workflow.Name);
			Assert.AreEqual(true, workflow.IsDefault);
			Assert.AreEqual(true, workflow.IsActive);

			//Now lets update the workflow
			workflow.StartTracking();
			workflow.Name = "Customized Workflow";
			workflow.IsDefault = false;
			workflow.IsActive = false;
			workflowManager.Workflow_Update(workflow);

			//Now verify that it updated correctly
			workflow = workflowManager.Workflow_RetrieveById(workflowId2);
			Assert.AreEqual("Customized Workflow", workflow.Name);
			Assert.AreEqual(false, workflow.IsDefault);
			Assert.AreEqual(false, workflow.IsActive);

			//Now lets add some transitions and transition roles
			int workflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId2, (int)Task.TaskStatusEnum.NotStarted, (int)Task.TaskStatusEnum.InProgress, "Execute Task").WorkflowTransitionId;
			TaskWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId2, workflowTransitionId);
			workflowTransition.StartTracking();
			TaskWorkflowTransitionRole workflowTransitionRole = new TaskWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now lets add some workflow field and custom property states
			TaskStatus taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.NotStarted, workflowId2); //Not-Started Status
			taskStatus.StartTracking();
			//Field=Required
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Task);
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "ReleaseId").ArtifactFieldId;
			TaskWorkflowField workflowField = new TaskWorkflowField();
			workflowField.ArtifactFieldId = artifactFieldId;
			workflowField.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			workflowField.TaskWorkflowId = workflowId2;
			taskStatus.WorkflowFields.Add(workflowField);
			//Custom Property = Hidden
			TaskWorkflowCustomProperty workflowCustomProperty = new TaskWorkflowCustomProperty();
			workflowCustomProperty.CustomPropertyId = 1;
			workflowCustomProperty.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			workflowCustomProperty.TaskWorkflowId = workflowId2;
			taskStatus.WorkflowCustomProperties.Add(workflowCustomProperty);
			workflowManager.Status_UpdateFieldsAndCustomProperties(taskStatus);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(workflowId2);

			//Now we need to verify that the various exception cases are detected and thrown by the business object
			//First lets try and insert a new workflow that is marked as default but inactive
			bool exceptionMatch = false;
			try
			{
				workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, false);
			}
			catch (Business.WorkflowMustBeActiveException)
			{
				exceptionMatch = true;
			}
			finally
			{
				//Test case to ensure you can't insert an inactive default workflow
				Assert.IsTrue(exceptionMatch, "Must not be able to insert inactive default workflow");
			}

			//Next lets try and update an existing workflow that is marked as default but inactive
			exceptionMatch = false;
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).TaskWorkflowId;
			try
			{
				workflow = workflowManager.Workflow_RetrieveById(workflowId2);
				workflow.StartTracking();
				workflow.IsActive = false;
				workflowManager.Workflow_Update(workflow);
			}
			catch (Business.WorkflowMustBeActiveException)
			{
				exceptionMatch = true;
			}
			finally
			{
				//Test case to ensure you can't update a default workflow to inactive
				Assert.IsTrue(exceptionMatch, "Must not be able to update a default workflow to inactive");
			}

			//Next lets try and delete a workflow that is marked as default
			exceptionMatch = false;
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).TaskWorkflowId;
			try
			{
				workflowManager.Workflow_Delete(workflowId2);
			}
			catch (Business.WorkflowInUseException)
			{
				exceptionMatch = true;
			}
			finally
			{
				//Test case to ensure you can't delete a default workflow
				Assert.IsTrue(exceptionMatch, "Must not be able to delete a default workflow");
			}

			//Now we need to make it inactive and then delete it
			workflow = workflowManager.Workflow_RetrieveById(workflowId2);
			workflow.StartTracking();
			workflow.IsActive = false;
			workflow.IsDefault = false;
			workflowManager.Workflow_Update(workflow);
			workflowManager.Workflow_Delete(workflowId2);

			//Finally lets try and delete a workflow that is in use (linked to at least one task type)
			exceptionMatch = false;
			try
			{
				workflowManager.Workflow_Delete(workflowId);
			}
			catch (Business.WorkflowInUseException)
			{
				exceptionMatch = true;
			}
			finally
			{
				//Test case to ensure you can't delete a workflow that is in use
				Assert.IsTrue(exceptionMatch, "Must not be able to delete a workflow that is in use");
			}
		}

		[
		Test,
		SpiraTestCase(1265)
		]
		public void _12_CopyWorkflows()
		{
			//Get the default workflow (so that we can compare the copy against it)
			TaskWorkflow oldWorkflow = workflowManager.Workflow_RetrieveById(workflowId, true);

			//First, we need to make a copy of the default workflow for the project
			int newWorkflowId = workflowManager.Workflow_Copy(workflowId).TaskWorkflowId;

			//Now lets verify some of the counts
			workflow = workflowManager.Workflow_RetrieveById(newWorkflowId, true);
			Assert.IsNotNull(workflow);
			Assert.AreEqual(oldWorkflow.Transitions.Count, workflow.Transitions.Count);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(newWorkflowId);
		}

		[
		Test,
		SpiraTestCase(1266)
		]
		public void _13_GetEnabledFields()
		{
			//Pull fields.
			List<TaskWorkflowField> transitionFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, (int)Task.TaskStatusEnum.InProgress);

			//Verify that the ones we select match up.
			TaskWorkflowField workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "ReleaseId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Release", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);
			workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "EstimatedEffort");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Est. Effort", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);

			//Make one custom property hidden and one disabled
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
			taskStatus = workflowManager.Status_RetrieveById((int)Task.TaskStatusEnum.InProgress, workflowId);
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			taskStatus.StartTracking();
			TaskWorkflowCustomProperty field = new TaskWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.TaskWorkflowId = workflowId;
			taskStatus.WorkflowCustomProperties.Add(field);
			customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_02").CustomPropertyId;
			taskStatus.StartTracking();
			field = new TaskWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.TaskWorkflowId = workflowId;
			taskStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(taskStatus);

			//Pull custom properties.
			List<TaskWorkflowCustomProperty> transitionCustomProperties = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, (int)Task.TaskStatusEnum.InProgress);

			//Verify that the ones we select match up.
			TaskWorkflowCustomProperty workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_01");
			Assert.AreEqual("Difficulty", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden, workflowCustomProperty.WorkflowFieldStateId);
			workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_02");
			Assert.AreEqual("Reviewer", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive, workflowCustomProperty.WorkflowFieldStateId);
		}

		#region Helper Functions

		/// <summary>
		/// Determines if a field is disabled for the current workflow and task status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldDisabled(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (taskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a field is hidden for the current workflow and task status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldHidden(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (taskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a field is required for the current workflow and task status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldRequired(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (taskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		/// <summary>
		/// Determines if a custom property is disabled for the current workflow and task status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyDisabled(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (taskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a custom property is hidden for the current workflow and task status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyHidden(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (taskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a custom property is required for the current workflow and task status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyRequired(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (taskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		#endregion
	}
}
