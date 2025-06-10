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
	/// Tests the test case workflow manager
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class TestCaseWorkflowTest
	{
		protected static TestCaseWorkflowManager workflowManager;
		protected static TestCaseWorkflow workflow;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static List<ArtifactField> currentArtifactFields = null;
		protected static List<CustomProperty> currentCustomProperties = null;
		protected static TestCaseStatus testCaseStatus;
		protected static int projectId;
		protected static int projectTemplateId;
		protected static int workflowId;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;


		[TestFixtureSetUp]
		public void Init()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Instantiate the class being tested
			workflowManager = new TestCaseWorkflowManager();

			//Create a new project for testing with (only some tests currently use this)
			//This will create the default workflow entries (for standard fields)
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("TestCaseWorkflowManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Need to add some custom properties for testCases in this new project
			CustomPropertyManager cpm = new CustomPropertyManager();
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, (int)CustomProperty.CustomPropertyTypeEnum.Text, 1, "Notes", null, null, null);
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, (int)CustomProperty.CustomPropertyTypeEnum.Decimal, 2, "Rank", null, null, null);

			//We need to load in the standard field and custom property states first
			currentArtifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.TestCase);
			currentCustomProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
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
		SpiraTestCase(1379)
		]
		public void _01_Retrieve_Lookups()
		{
			//Verify that we can get the default workflow for a project
			TestCaseWorkflow workflow = workflowManager.Workflow_GetDefault(projectTemplateId);
			Assert.AreEqual("Default Workflow", workflow.Name);
			Assert.AreEqual(true, workflow.IsActive);
			Assert.AreEqual(true, workflow.IsDefault);
			int defaultWorkflowId = workflow.TestCaseWorkflowId;

			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseType> types = testCaseManager.TestCaseType_Retrieve(projectTemplateId);

			//Verify that we can get the workflow associated with a specific type
			workflowId = workflowManager.Workflow_GetForTestCaseType(types[0].TestCaseTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForTestCaseType(types[1].TestCaseTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForTestCaseType(types[2].TestCaseTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForTestCaseType(types[3].TestCaseTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForTestCaseType(types[4].TestCaseTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
		}

		[
		Test,
		SpiraTestCase(1380)
		]
		public void _02_Retrieve_Field_States()
		{
			//Get the default workflow for this project
			workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).TestCaseWorkflowId;

			//Load the field state for TestCaseStatus=Draft
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Draft, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldRequired("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldHidden("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldDisabled("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));

			//Load the field state for TestCaseStatus=ReadyForReview
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.ReadyForReview, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldRequired("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldHidden("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldDisabled("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));

			//Load the field state for TestCaseStatus=Approved
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldRequired("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldHidden("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldDisabled("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));

			//Load the field state for TestCaseStatus=ReadyForTest
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.ReadyForTest, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldRequired("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldHidden("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldDisabled("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));

			//Load the field state for TestCaseStatus=Obsolete
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Obsolete, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldRequired("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldHidden("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("EstimatedDuration"));
			Assert.AreEqual(false, IsFieldDisabled("TestCasePriorityId"));
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));
		}

		[
		Test,
		SpiraTestCase(1381)
		]
		public void _03_Retrieve_Custom_Property_States()
		{
			//Load the custom property state for TestCaseStatus=Draft
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Draft, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for TestCaseStatus=ReadyForReview
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.ReadyForReview, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for TestCaseStatus=Approved
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for TestCaseStatus=ReadyForTest
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.ReadyForTest, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for TestCaseStatus=Obsolete
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Obsolete, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for TestCaseStatus=Rejected
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Rejected, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank
		}

		[
		Test,
		SpiraTestCase(1382)
		]
		public void _04_RetrievesTransitionsForStep()
		{
			//Verify the output transitions returned for a given input statuses (varies by role)
			//InputStatus=Draft, Role=Manager
			List<TestCaseWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)TestCase.TestCaseStatusEnum.Draft, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Reject Test Case", workflowTransitions[0].Name);
			Assert.AreEqual("Review Test Case", workflowTransitions[1].Name);

			//InputStatus=ReadyForReview, Role=Project Owner
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForReview, InternalRoutines.PROJECT_ROLE_PROJECT_OWNER, false, false);
			Assert.AreEqual(3, workflowTransitions.Count);
			Assert.AreEqual("Approve Test Case", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Test Case", workflowTransitions[1].Name);
			Assert.AreEqual("Return to Draft", workflowTransitions[2].Name);

			//InputStatus=ReadyForReview, Role=Developer, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForReview, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, true);
			Assert.AreEqual(3, workflowTransitions.Count);
			Assert.AreEqual("Approve Test Case", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Test Case", workflowTransitions[1].Name);
			Assert.AreEqual("Return to Draft", workflowTransitions[2].Name);

			//InputStatus=ReadyForReview, Role=Developer, Creator=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForReview, InternalRoutines.PROJECT_ROLE_DEVELOPER, true, false);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Reject Test Case", workflowTransitions[0].Name);
			Assert.AreEqual("Return to Draft", workflowTransitions[1].Name);
		}

		[
		Test,
		SpiraTestCase(1383)
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
			//Draft > ReadyForReview
			TestCaseWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, (int)TestCase.TestCaseStatusEnum.Draft, (int)TestCase.TestCaseStatusEnum.ReadyForReview);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Review Test Case", workflowTransition.Name);
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);

			//Lets test that we can retrieve a transition based on input and output status
			//ReadyForReview > Approved
			workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, (int)TestCase.TestCaseStatusEnum.ReadyForReview, (int)TestCase.TestCaseStatusEnum.Approved);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Approve Test Case", workflowTransition.Name);
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);
		}

		[
		Test,
		SpiraTestCase(1384)
		]
		public void _07_ViewModifyWorkflowStepFields()
		{
			//First verify that we can retrieve all the workflow-configurable fields for a test case
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.TestCase);
			Assert.AreEqual(14, artifactFields.Count);

			//Now lets verify that a particular field is required/active (OwnerId)
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			Assert.IsFalse(IsFieldHidden("OwnerId"));
			Assert.IsFalse(IsFieldDisabled("OwnerId"));
			Assert.IsFalse(IsFieldRequired("OwnerId"));

			//Now lets change the disabled setting for this field
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "OwnerId").ArtifactFieldId;
			//Disabled=True
			testCaseStatus.StartTracking();
			TestCaseWorkflowField field = new TestCaseWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.TestCaseWorkflowId = workflowId;
			testCaseStatus.WorkflowFields.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

			//Now refresh the data and verify
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			Assert.IsFalse(IsFieldHidden("OwnerId"));
			Assert.IsTrue(IsFieldDisabled("OwnerId"));
			Assert.IsFalse(IsFieldRequired("OwnerId"));

			//Now lets change the required setting for this field
			//Required=True
			testCaseStatus.StartTracking();
			field = new TestCaseWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.TestCaseWorkflowId = workflowId;
			testCaseStatus.WorkflowFields.Add(field);
			//Disabled=False
			testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.TestCaseWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

			//Now refresh the data and verify
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			Assert.IsFalse(IsFieldHidden("OwnerId"));
			Assert.IsFalse(IsFieldDisabled("OwnerId"));
			Assert.IsTrue(IsFieldRequired("OwnerId"));

			//Finally change the data back
			testCaseStatus.StartTracking();
			testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.TestCaseWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

			//Now refresh the data and verify it was returned back
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			Assert.IsFalse(IsFieldHidden("OwnerId"));
			Assert.IsFalse(IsFieldDisabled("OwnerId"));
			Assert.IsFalse(IsFieldRequired("OwnerId"));
		}

		[
		Test,
		SpiraTestCase(1385)
		]
		public void _08_ViewModifyWorkflowStepCustomProperties()
		{
			//First verify that we can retrieve all the custom properties for a test case
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
			Assert.AreEqual(2, customProperties.Count);

			//Now lets verify that a particular property is required/active (Notes=Custom_01)
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=True
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			testCaseStatus.StartTracking();
			TestCaseWorkflowCustomProperty field = new TestCaseWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.TestCaseWorkflowId = workflowId;
			testCaseStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

			//Now refresh the data and verify
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsTrue(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=False, Required=True
			testCaseStatus.StartTracking();
			field = new TestCaseWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.TestCaseWorkflowId = workflowId;
			testCaseStatus.WorkflowCustomProperties.Add(field);
			testCaseStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.TestCaseWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

			//Now refresh the data and verify
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsTrue(IsCustomPropertyRequired("Custom_01"));

			//Finally change the data back
			testCaseStatus.StartTracking();
			testCaseStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.TestCaseWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

			//Now refresh the data and verify that it's cleaned up
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));
		}

		[
		Test,
		SpiraTestCase(1386)
		]
		public void _09_ViewModifyWorkflowRoles()
		{
			//First lets get the various assigned roles for a specific transition
			//For this test we'll use the Draft > ReadyForReview
			TestCaseWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			TestCaseWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputTestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Draft && t.OutputTestCaseStatusId == (int)TestCase.TestCaseStatusEnum.ReadyForReview);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			//Test that we can get the transition by its ID
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			List<TestCaseWorkflowTransitionRole> executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);

			//Now lets add a couple of roles
			workflowTransition.StartTracking();
			TestCaseWorkflowTransitionRole workflowTransitionRole = new TestCaseWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowTransitionRole = new TestCaseWorkflowTransitionRole();
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
		SpiraTestCase(1387)
		]
		public void _10_ViewModifyWorkflowTransitions()
		{
			//First lets retrieve a single workflow transition and verify its data
			//For this test we'll use the Draft > ReadyForReview
			TestCaseWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			TestCaseWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputTestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Draft && t.OutputTestCaseStatusId == (int)TestCase.TestCaseStatusEnum.ReadyForReview);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Review Test Case
			Assert.AreEqual("Review Test Case", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Draft", workflowTransition.InputTestCaseStatus.Name, "InputTestCaseStatusName");
			Assert.AreEqual("Ready for Review", workflowTransition.OutputTestCaseStatus.Name, "OutputTestCaseStatusName");

			//Now lets modify the data and then save the changes
			//Note that you cannot change the input and output testCase statuses
			workflowTransition.StartTracking();
			workflowTransition.Name = "Send Test Case to Review";
			workflowTransition.IsExecuteByCreator = false;
			workflowTransition.IsExecuteByOwner = false;
			workflowTransition.IsSignatureRequired = true;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual("Send Test Case to Review", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(true, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Draft", workflowTransition.InputTestCaseStatus.Name, "InputTestCaseStatusName");
			Assert.AreEqual("Ready for Review", workflowTransition.OutputTestCaseStatus.Name, "OutputTestCaseStatusName");

			//Now lets return the data back to what it was before
			workflowTransition.StartTracking();
			workflowTransition.Name = "Review Test Case";
			workflowTransition.IsExecuteByCreator = true;
			workflowTransition.IsExecuteByOwner = true;
			workflowTransition.IsSignatureRequired = false;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Accept TestCase
			Assert.AreEqual("Review Test Case", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Draft", workflowTransition.InputTestCaseStatus.Name, "InputTestCaseStatusName");
			Assert.AreEqual("Ready for Review", workflowTransition.OutputTestCaseStatus.Name, "OutputTestCaseStatusName");

			//Now lets create a new workflow transition (Draft > Approved)
			int newWorkflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId, (int)TestCase.TestCaseStatusEnum.Draft, (int)TestCase.TestCaseStatusEnum.Approved, "Quick Approval", false, true, new List<int> { ProjectManager.ProjectRoleTester }).WorkflowTransitionId;

			//Verify that it was created
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, newWorkflowTransitionId);
			Assert.AreEqual("Quick Approval", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual("Draft", workflowTransition.InputTestCaseStatus.Name, "InputTestCaseStatusName");
			Assert.AreEqual("Approved", workflowTransition.OutputTestCaseStatus.Name, "OutputTestCaseStatusName");

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
		SpiraTestCase(1388)
		]
		public void _11_ViewModifyWorkflows()
		{
			//First lets test that we can create a new workflow
			int workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test Workflow", true, true).TestCaseWorkflowId;

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
			int workflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId2, (int)TestCase.TestCaseStatusEnum.Draft, (int)TestCase.TestCaseStatusEnum.Approved, "Approve Test Case").WorkflowTransitionId;
			TestCaseWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId2, workflowTransitionId);
			workflowTransition.StartTracking();
			TestCaseWorkflowTransitionRole workflowTransitionRole = new TestCaseWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now lets add some workflow field and custom property states
			TestCaseStatus testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Draft, workflowId2); //Draft Status
			testCaseStatus.StartTracking();
			//Field=Required
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.TestCase);
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "OwnerId").ArtifactFieldId;
			TestCaseWorkflowField workflowField = new TestCaseWorkflowField();
			workflowField.ArtifactFieldId = artifactFieldId;
			workflowField.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			workflowField.TestCaseWorkflowId = workflowId2;
			testCaseStatus.WorkflowFields.Add(workflowField);
			//Custom Property = Hidden
			TestCaseWorkflowCustomProperty workflowCustomProperty = new TestCaseWorkflowCustomProperty();
			workflowCustomProperty.CustomPropertyId = 1;
			workflowCustomProperty.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			workflowCustomProperty.TestCaseWorkflowId = workflowId2;
			testCaseStatus.WorkflowCustomProperties.Add(workflowCustomProperty);
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).TestCaseWorkflowId;
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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).TestCaseWorkflowId;
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

			//Finally lets try and delete a workflow that is in use (linked to at least one test case type)
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
		SpiraTestCase(1389)
		]
		public void _12_CopyWorkflows()
		{
			//Get the default workflow (so that we can compare the copy against it)
			TestCaseWorkflow oldWorkflow = workflowManager.Workflow_RetrieveById(workflowId, true);

			//First, we need to make a copy of the default workflow for the project
			int newWorkflowId = workflowManager.Workflow_Copy(workflowId).TestCaseWorkflowId;

			//Now lets verify some of the counts
			workflow = workflowManager.Workflow_RetrieveById(newWorkflowId, true);
			Assert.IsNotNull(workflow);
			Assert.AreEqual(oldWorkflow.Transitions.Count, workflow.Transitions.Count);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(newWorkflowId);
		}

		[
		Test,
		SpiraTestCase(1390)
		]
		public void _13_GetEnabledFields()
		{
			//Make two standard fields required
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			testCaseStatus.StartTracking();
			TestCaseWorkflowField field = new TestCaseWorkflowField();
			field.ArtifactFieldId = 23;  //OwnerId
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.TestCaseWorkflowId = workflowId;
			testCaseStatus.WorkflowFields.Add(field);
			testCaseStatus.StartTracking();
			field = new TestCaseWorkflowField();
			field.ArtifactFieldId = 24;   //TestCasePriorityId
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.TestCaseWorkflowId = workflowId;
			testCaseStatus.WorkflowFields.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

			//Pull fields.
			List<TestCaseWorkflowField> transitionFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, (int)TestCase.TestCaseStatusEnum.Approved);

			//Verify that the ones we select match up.
			TestCaseWorkflowField workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "OwnerId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Owner", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);
			workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "TestCasePriorityId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Priority", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);

			//Make one custom property hidden and one disabled
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
			testCaseStatus = workflowManager.Status_RetrieveById((int)TestCase.TestCaseStatusEnum.Approved, workflowId);
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			testCaseStatus.StartTracking();
			TestCaseWorkflowCustomProperty customProp = new TestCaseWorkflowCustomProperty();
			customProp.CustomPropertyId = customPropertyId;
			customProp.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			customProp.TestCaseWorkflowId = workflowId;
			testCaseStatus.WorkflowCustomProperties.Add(customProp);
			customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_02").CustomPropertyId;
			testCaseStatus.StartTracking();
			customProp = new TestCaseWorkflowCustomProperty();
			customProp.CustomPropertyId = customPropertyId;
			customProp.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			customProp.TestCaseWorkflowId = workflowId;
			testCaseStatus.WorkflowCustomProperties.Add(customProp);
			workflowManager.Status_UpdateFieldsAndCustomProperties(testCaseStatus);

			//Pull custom properties.
			List<TestCaseWorkflowCustomProperty> transitionCustomProperties = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, (int)TestCase.TestCaseStatusEnum.Approved);

			//Verify that the ones we select match up.
			TestCaseWorkflowCustomProperty workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_01");
			Assert.AreEqual("Notes", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden, workflowCustomProperty.WorkflowFieldStateId);
			workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_02");
			Assert.AreEqual("Rank", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive, workflowCustomProperty.WorkflowFieldStateId);
		}

		#region Helper Functions

		/// <summary>
		/// Determines if a field is disabled for the current workflow and testCase status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldDisabled(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a field is hidden for the current workflow and testCase status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldHidden(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a field is required for the current workflow and testCase status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldRequired(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (testCaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		/// <summary>
		/// Determines if a custom property is disabled for the current workflow and testCase status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyDisabled(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a custom property is hidden for the current workflow and testCase status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyHidden(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a custom property is required for the current workflow and testCase status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyRequired(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (testCaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		#endregion
	}
}
