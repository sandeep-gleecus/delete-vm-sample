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
	/// This fixture tests the RequirementWorkflowManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class RequirementWorkflowManagerTest
	{
		protected static RequirementWorkflowManager workflowManager;
		protected static RequirementWorkflow workflow;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static List<ArtifactField> currentArtifactFields = null;
		protected static List<CustomProperty> currentCustomProperties = null;
		protected static RequirementStatus requirementStatus;
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
			workflowManager = new RequirementWorkflowManager();

			//Create a new project for testing with (only some tests currently use this)
			//This will create the default workflow entries (for standard fields)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("RequirementWorkflowManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Need to add some custom properties for requirements in this new project
			CustomPropertyManager cpm = new CustomPropertyManager();
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.Text, 1, "Notes", null, null, null);
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.Decimal, 2, "Rank", null, null, null);

			//We need to load in the standard field and custom property states first
			currentArtifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Requirement);
			currentCustomProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
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
		SpiraTestCase(1218)
		]
		public void _01_Retrieve_Lookups()
		{
			//Verify that we can get the default workflow for a project
			RequirementWorkflow workflow = workflowManager.Workflow_GetDefault(projectTemplateId);
			Assert.AreEqual("Default Workflow", workflow.Name);
			Assert.AreEqual(true, workflow.IsActive);
			Assert.AreEqual(true, workflow.IsDefault);
			int defaultWorkflowId = workflow.RequirementWorkflowId;

			RequirementManager requirementManager = new RequirementManager();
			List<RequirementType> types = requirementManager.RequirementType_Retrieve(projectTemplateId, false);

			//Verify that we can get the workflow associated with a specific type
			workflowId = workflowManager.Workflow_GetForRequirementType(types[0].RequirementTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForRequirementType(types[1].RequirementTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForRequirementType(types[2].RequirementTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForRequirementType(types[3].RequirementTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForRequirementType(types[4].RequirementTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
		}

		[
		Test,
		SpiraTestCase(1220)
		]
		public void _02_Retrieve_Field_States()
		{
			//Get the default workflow for this project
			workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).RequirementWorkflowId;

			//Load the field state for RequirementStatus=Requested
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Requested, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("EstimatePoints"));
			Assert.AreEqual(false, IsFieldRequired("ImportanceId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("EstimatePoints"));
			Assert.AreEqual(false, IsFieldHidden("ImportanceId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("EstimatePoints"));
			Assert.AreEqual(false, IsFieldDisabled("ImportanceId"));

			//Load the field state for RequirementStatus=Accepted
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("EstimatePoints"));
			Assert.AreEqual(true, IsFieldRequired("ImportanceId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("EstimatePoints"));
			Assert.AreEqual(false, IsFieldHidden("ImportanceId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("EstimatePoints"));
			Assert.AreEqual(false, IsFieldDisabled("ImportanceId"));

			//Load the field state for RequirementStatus=Planned
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Planned, workflowId);
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("EstimatePoints"));
			Assert.AreEqual(true, IsFieldRequired("ImportanceId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("EstimatePoints"));
			Assert.AreEqual(false, IsFieldHidden("ImportanceId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("EstimatePoints"));
			Assert.AreEqual(false, IsFieldDisabled("ImportanceId"));

			//Load the field state for RequirementStatus=InProgress
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.InProgress, workflowId);
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("EstimatePoints"));
			Assert.AreEqual(true, IsFieldRequired("ImportanceId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("EstimatePoints"));
			Assert.AreEqual(false, IsFieldHidden("ImportanceId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("EstimatePoints"));
			Assert.AreEqual(false, IsFieldDisabled("ImportanceId"));

			//Load the field state for RequirementStatus=Completed
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Completed, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("EstimatePoints"));
			Assert.AreEqual(false, IsFieldRequired("ImportanceId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("EstimatePoints"));
			Assert.AreEqual(false, IsFieldHidden("ImportanceId"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(true, IsFieldDisabled("EstimatePoints"));
			Assert.AreEqual(true, IsFieldDisabled("ImportanceId"));
		}

		[
		Test,
		SpiraTestCase(1221)
		]
		public void _03_Retrieve_Custom_Property_States()
		{
			//Load the custom property state for RequirementStatus=Requested
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Requested, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for RequirementStatus=Accepted
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for RequirementStatus=Planned
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Planned, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for RequirementStatus=In Progress
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.InProgress, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for RequirementStatus=Developed
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Developed, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for RequirementStatus=Completed
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Completed, workflowId);
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
		SpiraTestCase(1222)
		]
		public void _04_RetrievesTransitionsForStep()
		{
			//Verify the output transitions returned for a given input statuses (varies by role)
			//InputStatus=Requested, Role=Manager
			List<RequirementWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Requirement.RequirementStatusEnum.Requested, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Accept Requirement", workflowTransitions[0].Name);
			Assert.AreEqual("Review Requirement", workflowTransitions[1].Name);

			//InputStatus=UnderReview, Role=Manager
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Requirement.RequirementStatusEnum.UnderReview, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Accept Requirement", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Requirement", workflowTransitions[1].Name);

			//InputStatus=UnderReview, Role=Developer, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Requirement.RequirementStatusEnum.UnderReview, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, true);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Accept Requirement", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Requirement", workflowTransitions[1].Name);

			//InputStatus=UnderReview, Role=Developer, Creator=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Requirement.RequirementStatusEnum.UnderReview, InternalRoutines.PROJECT_ROLE_DEVELOPER, true, false);
			Assert.AreEqual(1, workflowTransitions.Count);
			Assert.AreEqual("Reject Requirement", workflowTransitions[0].Name);
		}

		[
		Test,
		SpiraTestCase(1223)
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
			//Requested > Accepted
			RequirementWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, (int)Requirement.RequirementStatusEnum.Requested, (int)Requirement.RequirementStatusEnum.Accepted);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Accept Requirement", workflowTransition.Name);
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner);

			//Lets test that we can retrieve a transition based on input and output status
			//UnderReview > Accepted
			workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, (int)Requirement.RequirementStatusEnum.UnderReview, (int)Requirement.RequirementStatusEnum.Accepted);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Accept Requirement", workflowTransition.Name);
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);
		}

		[
		Test,
		SpiraTestCase(1224)
		]
		public void _07_ViewModifyWorkflowStepFields()
		{
			//First verify that we can retrieve all the workflow-configurable fields for an requirement
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(11, artifactFields.Count);

			//Now lets verify that a particular field is required/active (Release)
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));

			//Now lets change the disabled setting for this field
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "ReleaseId").ArtifactFieldId;
			//Disabled=True
			requirementStatus.StartTracking();
			RequirementWorkflowField field = new RequirementWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.RequirementWorkflowId = workflowId;
			requirementStatus.WorkflowFields.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(requirementStatus);

			//Now refresh the data and verify
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsTrue(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));

			//Now lets change the required setting for this field
			//Required=True
			requirementStatus.StartTracking();
			field = new RequirementWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.RequirementWorkflowId = workflowId;
			requirementStatus.WorkflowFields.Add(field);
			//Disabled=False
			requirementStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.RequirementWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(requirementStatus);

			//Now refresh the data and verify
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsTrue(IsFieldRequired("ReleaseId"));

			//Finally change the data back
			requirementStatus.StartTracking();
			requirementStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.RequirementWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(requirementStatus);

			//Now refresh the data and verify it was returned back
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));
		}

		[
		Test,
		SpiraTestCase(1225)
		]
		public void _08_ViewModifyWorkflowStepCustomProperties()
		{
			//First verify that we can retrieve all the custom properties for an requirement
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			Assert.AreEqual(2, customProperties.Count);

			//Now lets verify that a particular property is required/active (Notes=Custom_01)
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=True
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			requirementStatus.StartTracking();
			RequirementWorkflowCustomProperty field = new RequirementWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.RequirementWorkflowId = workflowId;
			requirementStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(requirementStatus);

			//Now refresh the data and verify
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsTrue(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=False, Required=True
			requirementStatus.StartTracking();
			field = new RequirementWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.RequirementWorkflowId = workflowId;
			requirementStatus.WorkflowCustomProperties.Add(field);
			requirementStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.RequirementWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(requirementStatus);

			//Now refresh the data and verify
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsTrue(IsCustomPropertyRequired("Custom_01"));

			//Finally change the data back
			requirementStatus.StartTracking();
			requirementStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.RequirementWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(requirementStatus);

			//Now refresh the data and verify that it's cleaned up
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Accepted, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));
		}

		[
		Test,
		SpiraTestCase(1226)
		]
		public void _09_ViewModifyWorkflowRoles()
		{
			//First lets get the various assigned roles for a specific transition
			//For this test we'll use the Requested > Accepted
			RequirementWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			RequirementWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputRequirementStatusId == (int)Requirement.RequirementStatusEnum.Requested && t.OutputRequirementStatusId == (int)Requirement.RequirementStatusEnum.Accepted);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			//Test that we can get the transition by its ID
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			List<RequirementWorkflowTransitionRole> executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);

			//Now lets add a couple of roles
			workflowTransition.StartTracking();
			RequirementWorkflowTransitionRole workflowTransitionRole = new RequirementWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowTransitionRole = new RequirementWorkflowTransitionRole();
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
		SpiraTestCase(1227)
		]
		public void _10_ViewModifyWorkflowTransitions()
		{
			//First lets retrieve a single workflow transition and verify its data
			//For this test we'll use the Requested > Accepted
			RequirementWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			RequirementWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputRequirementStatusId == (int)Requirement.RequirementStatusEnum.Requested && t.OutputRequirementStatusId == (int)Requirement.RequirementStatusEnum.Accepted);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Accept Requirement
			Assert.AreEqual("Accept Requirement", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Requested", workflowTransition.InputRequirementStatus.Name, "InputRequirementStatusName");
			Assert.AreEqual("Accepted", workflowTransition.OutputRequirementStatus.Name, "OutputRequirementStatusName");

			//Now lets modify the data and then save the changes
			//Note that you cannot change the input and output requirement statuses
			workflowTransition.StartTracking();
			workflowTransition.Name = "Approve Requirement";
			workflowTransition.IsExecuteByCreator = false;
			workflowTransition.IsExecuteByOwner = true;
			workflowTransition.IsSignatureRequired = true;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Accept Requirement
			Assert.AreEqual("Approve Requirement", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(true, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Requested", workflowTransition.InputRequirementStatus.Name, "InputRequirementStatusName");
			Assert.AreEqual("Accepted", workflowTransition.OutputRequirementStatus.Name, "OutputRequirementStatusName");

			//Now lets return the data back to what it was before
			workflowTransition.StartTracking();
			workflowTransition.Name = "Accept Requirement";
			workflowTransition.IsExecuteByCreator = false;
			workflowTransition.IsExecuteByOwner = false;
			workflowTransition.IsSignatureRequired = false;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Accept Requirement
			Assert.AreEqual("Accept Requirement", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Requested", workflowTransition.InputRequirementStatus.Name, "InputRequirementStatusName");
			Assert.AreEqual("Accepted", workflowTransition.OutputRequirementStatus.Name, "OutputRequirementStatusName");

			//Now lets create a new workflow transition (Requested > Completed)
			int newWorkflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId, (int)Requirement.RequirementStatusEnum.Requested, (int)Requirement.RequirementStatusEnum.Completed, "Mark As Complete", false, true, new List<int> { ProjectManager.ProjectRoleTester }).WorkflowTransitionId;

			//Verify that it was created
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, newWorkflowTransitionId);
			Assert.AreEqual("Mark As Complete", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual("Requested", workflowTransition.InputRequirementStatus.Name, "InputRequirementStatusName");
			Assert.AreEqual("Completed", workflowTransition.OutputRequirementStatus.Name, "OutputRequirementStatusName");

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
		SpiraTestCase(1228)
		]
		public void _11_ViewModifyWorkflows()
		{
			//First lets test that we can create a new workflow
			int workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test Workflow", true, true).RequirementWorkflowId;

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
			int workflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId2, (int)Requirement.RequirementStatusEnum.Requested, (int)Requirement.RequirementStatusEnum.Planned, "Plan Requirement").WorkflowTransitionId;
			RequirementWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId2, workflowTransitionId);
			workflowTransition.StartTracking();
			RequirementWorkflowTransitionRole workflowTransitionRole = new RequirementWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now lets add some workflow field and custom property states
			RequirementStatus requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Requested, workflowId2); //Requested Status
			requirementStatus.StartTracking();
			//Field=Required
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Requirement);
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "ReleaseId").ArtifactFieldId;
			RequirementWorkflowField workflowField = new RequirementWorkflowField();
			workflowField.ArtifactFieldId = artifactFieldId;
			workflowField.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			workflowField.RequirementWorkflowId = workflowId2;
			requirementStatus.WorkflowFields.Add(workflowField);
			//Custom Property = Hidden
			RequirementWorkflowCustomProperty workflowCustomProperty = new RequirementWorkflowCustomProperty();
			workflowCustomProperty.CustomPropertyId = 1;
			workflowCustomProperty.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			workflowCustomProperty.RequirementWorkflowId = workflowId2;
			requirementStatus.WorkflowCustomProperties.Add(workflowCustomProperty);
			workflowManager.Status_UpdateFieldsAndCustomProperties(requirementStatus);

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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).RequirementWorkflowId;
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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).RequirementWorkflowId;
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

			//Finally lets try and delete a workflow that is in use (linked to at least one requirement type)
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
		SpiraTestCase(1229)
		]
		public void _12_CopyWorkflows()
		{
			//Get the default workflow (so that we can compare the copy against it)
			RequirementWorkflow oldWorkflow = workflowManager.Workflow_RetrieveById(workflowId, true);

			//First, we need to make a copy of the default workflow for the project
			int newWorkflowId = workflowManager.Workflow_Copy(workflowId).RequirementWorkflowId;

			//Now lets verify some of the counts
			workflow = workflowManager.Workflow_RetrieveById(newWorkflowId, true);
			Assert.IsNotNull(workflow);
			Assert.AreEqual(oldWorkflow.Transitions.Count, workflow.Transitions.Count);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(newWorkflowId);
		}

		[
		Test,
		SpiraTestCase(1230)
		]
		public void _13_GetEnabledFields()
		{
			//Pull fields.
			List<RequirementWorkflowField> transitionFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, (int)Requirement.RequirementStatusEnum.Planned);

			//Verify that the ones we select match up.
			RequirementWorkflowField workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "ReleaseId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Release", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);
			workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "ImportanceId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Importance", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);

			//Make one custom property hidden and one disabled
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			requirementStatus = workflowManager.Status_RetrieveById((int)Requirement.RequirementStatusEnum.Planned, workflowId);
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			requirementStatus.StartTracking();
			RequirementWorkflowCustomProperty field = new RequirementWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.RequirementWorkflowId = workflowId;
			requirementStatus.WorkflowCustomProperties.Add(field);
			customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_02").CustomPropertyId;
			requirementStatus.StartTracking();
			field = new RequirementWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.RequirementWorkflowId = workflowId;
			requirementStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(requirementStatus);

			//Pull custom properties.
			List<RequirementWorkflowCustomProperty> transitionCustomProperties = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, (int)Requirement.RequirementStatusEnum.Planned);

			//Verify that the ones we select match up.
			RequirementWorkflowCustomProperty workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_01");
			Assert.AreEqual("Notes", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden, workflowCustomProperty.WorkflowFieldStateId);
			workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_02");
			Assert.AreEqual("Rank", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive, workflowCustomProperty.WorkflowFieldStateId);
		}

		#region Helper Functions

		/// <summary>
		/// Determines if a field is disabled for the current workflow and requirement status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldDisabled(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (requirementStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a field is hidden for the current workflow and requirement status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldHidden(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (requirementStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a field is required for the current workflow and requirement status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldRequired(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (requirementStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		/// <summary>
		/// Determines if a custom property is disabled for the current workflow and requirement status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyDisabled(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (requirementStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a custom property is hidden for the current workflow and requirement status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyHidden(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (requirementStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a custom property is required for the current workflow and requirement status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyRequired(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (requirementStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		#endregion
	}
}
