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
	/// This fixture tests the RiskWorkflowManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class RiskWorkflowManagerTest
	{
		protected static RiskWorkflowManager workflowManager;
		protected static RiskManager riskManager = new RiskManager();

		protected static RiskWorkflow workflow;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static List<ArtifactField> currentArtifactFields = null;
		protected static List<CustomProperty> currentCustomProperties = null;
		protected static RiskStatus riskStatus;
		protected static int projectId;
		protected static int projectTemplateId;
		protected static int workflowId;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private static int statusIdIdentified;
		private static int statusIdAnalyzed;
		private static int statusIdEvaluated;
		private static int statusIdOpen;
		private static int statusIdClosed;
		private static int statusIdRejected;

		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the classes being tested
			workflowManager = new RiskWorkflowManager();
			riskManager = new RiskManager();

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Create a new project for testing with (only some tests currently use this)
			//This will create the default workflow entries (for standard fields)
			ProjectManager projectManager = new ProjectManager();
			projectId = projectManager.Insert("RiskWorkflowManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Need to add some custom properties for risks in this new project
			CustomPropertyManager cpm = new CustomPropertyManager();
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "Difficulty", null, null, null);
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, (int)CustomProperty.CustomPropertyTypeEnum.User, 2, "Reviewer", null, null, null);

			//We need to load in the standard field and custom property states first
			currentArtifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Risk);
			currentCustomProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, false);

			//Write out the statuses for use in tests
			List<RiskStatus> statuses = riskManager.RiskStatus_Retrieve(projectTemplateId, false, false);
			statusIdIdentified = statuses.FirstOrDefault(c => c.Name == "Identified").RiskStatusId;
			statusIdAnalyzed = statuses.FirstOrDefault(c => c.Name == "Analyzed").RiskStatusId;
			statusIdEvaluated = statuses.FirstOrDefault(c => c.Name == "Evaluated").RiskStatusId;
			statusIdOpen = statuses.FirstOrDefault(c => c.Name == "Open").RiskStatusId;
			statusIdClosed = statuses.FirstOrDefault(c => c.Name == "Closed").RiskStatusId;
			statusIdRejected = statuses.FirstOrDefault(c => c.Name == "Rejected").RiskStatusId;
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
		SpiraTestCase(1772)
		]
		public void _01_Retrieve_Lookups()
		{
			//Verify that we can get the default workflow for a project
			RiskWorkflow workflow = workflowManager.Workflow_GetDefault(projectTemplateId);
			Assert.AreEqual("Default Workflow", workflow.Name);
			Assert.AreEqual(true, workflow.IsActive);
			Assert.AreEqual(true, workflow.IsDefault);
			int defaultWorkflowId = workflow.RiskWorkflowId;

			RiskManager riskManager = new RiskManager();
			List<RiskType> types = riskManager.RiskType_Retrieve(projectTemplateId);

			//Verify that we can get the workflow associated with a specific type
			workflowId = workflowManager.Workflow_GetForRiskType(types[0].RiskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForRiskType(types[1].RiskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForRiskType(types[2].RiskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForRiskType(types[3].RiskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForRiskType(types[4].RiskTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
		}

		[
		Test,
		SpiraTestCase(1773)
		]
		public void _02_Retrieve_Field_States()
		{
			//Get the default workflow for this project
			workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).RiskWorkflowId;

			//Load the field state for RiskStatus=Identified
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));

			//Load the field state for RiskStatus=Analyzed
			riskStatus = workflowManager.Status_RetrieveById(statusIdAnalyzed, workflowId);
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("RiskProbabilityId"));
			Assert.AreEqual(true, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));

			//Load the field state for RiskStatus=Closed
			riskStatus = workflowManager.Status_RetrieveById(statusIdClosed, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(true, IsFieldDisabled("RiskProbabilityId"));
			Assert.AreEqual(true, IsFieldDisabled("OwnerId"));

			//Load the field state for RiskStatus=Open
			riskStatus = workflowManager.Status_RetrieveById(statusIdOpen, workflowId);
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("RiskProbabilityId"));
			Assert.AreEqual(true, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));

			//Load the field state for RiskStatus=Rejected
			riskStatus = workflowManager.Status_RetrieveById(statusIdRejected, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("ReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("ReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("RiskProbabilityId"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("ReleaseId"));
			Assert.AreEqual(true, IsFieldDisabled("RiskProbabilityId"));
			Assert.AreEqual(true, IsFieldDisabled("OwnerId"));
		}

		[
		Test,
		SpiraTestCase(1774)
		]
		public void _03_Retrieve_Custom_Property_States()
		{
			//Load the custom property state for RiskStatus=Identified
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer

			//Load the custom property state for RiskStatus=Analyzed
			riskStatus = workflowManager.Status_RetrieveById(statusIdAnalyzed, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer

			//Load the custom property state for RiskStatus=Closed
			riskStatus = workflowManager.Status_RetrieveById(statusIdClosed, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer

			//Load the custom property state for RiskStatus=Open
			riskStatus = workflowManager.Status_RetrieveById(statusIdOpen, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Reviewer
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Reviewer

			//Load the custom property state for RiskStatus=Rejected
			riskStatus = workflowManager.Status_RetrieveById(statusIdRejected, workflowId);
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
		SpiraTestCase(1775)
		]
		public void _04_RetrievesTransitionsForStep()
		{
			//Verify the output transitions returned for a given input statuses (varies by role)
			//InputStatus=Identified, Role=Manager
			List<RiskWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdIdentified, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Analyze Risk", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Risk", workflowTransitions[1].Name);

			//InputStatus=Identified, Role=Developer
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdIdentified, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, false);
			Assert.AreEqual(0, workflowTransitions.Count);

			//InputStatus=Identified, Role=Developer, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdIdentified, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, true);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Analyze Risk", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Risk", workflowTransitions[1].Name);

			//InputStatus=Analyzed, Role=Manager
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdAnalyzed, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Evaluate Risk", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Risk", workflowTransitions[1].Name);

			//InputStatus=Analyzed, Role=Developer, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdAnalyzed, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, true);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Evaluate Risk", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Risk", workflowTransitions[1].Name);

			//InputStatus=Analyzed, Role=Developer, Creator=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdAnalyzed, InternalRoutines.PROJECT_ROLE_DEVELOPER, true, false);
			Assert.AreEqual(0, workflowTransitions.Count);
		}

		[
		Test,
		SpiraTestCase(1776)
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
			//Identified > Analyzed
			RiskWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, statusIdIdentified, statusIdAnalyzed);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Analyze Risk", workflowTransition.Name);
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);

			//Lets test that we can retrieve a transition based on input and output status
			//Analyzed > Rejected
			workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, statusIdAnalyzed, statusIdRejected);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Reject Risk", workflowTransition.Name);
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);
		}

		[
		Test,
		SpiraTestCase(1777)
		]
		public void _07_ViewModifyWorkflowStepFields()
		{
			//First verify that we can retrieve all the workflow-configurable fields for a risk
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Risk);
			Assert.AreEqual(13, artifactFields.Count);

			//Now lets verify that a particular field is required/active (Release)
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));

			//Now lets change the disabled setting for this field
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "ReleaseId").ArtifactFieldId;
			//Disabled=True
			riskStatus.StartTracking();
			RiskWorkflowField field = new RiskWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.RiskWorkflowId = workflowId;
			riskStatus.WorkflowFields.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

			//Now refresh the data and verify
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsTrue(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));

			//Now lets change the required setting for this field
			//Required=True
			riskStatus.StartTracking();
			field = new RiskWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.RiskWorkflowId = workflowId;
			riskStatus.WorkflowFields.Add(field);
			//Disabled=False
			riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.RiskWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

			//Now refresh the data and verify
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsTrue(IsFieldRequired("ReleaseId"));

			//Finally change the data back
			riskStatus.StartTracking();
			riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.RiskWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

			//Now refresh the data and verify it was returned back
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			Assert.IsFalse(IsFieldHidden("ReleaseId"));
			Assert.IsFalse(IsFieldDisabled("ReleaseId"));
			Assert.IsFalse(IsFieldRequired("ReleaseId"));
		}

		[
		Test,
		SpiraTestCase(1778)
		]
		public void _08_ViewModifyWorkflowStepCustomProperties()
		{
			//First verify that we can retrieve all the custom properties for an risk
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, false);
			Assert.AreEqual(2, customProperties.Count);

			//Now lets verify that a particular property is required/active (Difficulty=Custom_01)
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=True
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			riskStatus.StartTracking();
			RiskWorkflowCustomProperty field = new RiskWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.RiskWorkflowId = workflowId;
			riskStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

			//Now refresh the data and verify
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsTrue(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=False, Required=True
			riskStatus.StartTracking();
			field = new RiskWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.RiskWorkflowId = workflowId;
			riskStatus.WorkflowCustomProperties.Add(field);
			riskStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.RiskWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

			//Now refresh the data and verify
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsTrue(IsCustomPropertyRequired("Custom_01"));

			//Finally change the data back
			riskStatus.StartTracking();
			riskStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.RiskWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

			//Now refresh the data and verify that it's cleaned up
			riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));
		}

		[
		Test,
		SpiraTestCase(1779)
		]
		public void _09_ViewModifyWorkflowRoles()
		{
			//First lets get the various assigned roles for a specific transition
			//For this test we'll use the NotStarted > InProgress
			RiskWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			RiskWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputRiskStatusId == statusIdIdentified && t.OutputRiskStatusId == statusIdAnalyzed);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			//Test that we can get the transition by its ID
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			List<RiskWorkflowTransitionRole> executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].ProjectRole.Name);
			Assert.AreEqual("Manager", executeRoles[1].ProjectRole.Name);

			//Now lets add a couple of roles
			workflowTransition.StartTracking();
			RiskWorkflowTransitionRole workflowTransitionRole = new RiskWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowTransitionRole = new RiskWorkflowTransitionRole();
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
			Assert.AreEqual("Product Owner", executeRoles[0].ProjectRole.Name);
			Assert.AreEqual("Manager", executeRoles[1].ProjectRole.Name);
			Assert.AreEqual("Developer", executeRoles[2].ProjectRole.Name);

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
			Assert.AreEqual("Product Owner", executeRoles[0].ProjectRole.Name);
			Assert.AreEqual("Manager", executeRoles[1].ProjectRole.Name);
		}

		[
		Test,
		SpiraTestCase(1780)
		]
		public void _10_ViewModifyWorkflowTransitions()
		{
			//First lets retrieve a single workflow transition and verify its data
			//For this test we'll use the Identified > Analyzed
			RiskWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			RiskWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputRiskStatusId == statusIdIdentified && t.OutputRiskStatusId == statusIdAnalyzed);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Start Risk
			Assert.AreEqual("Analyze Risk", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator, "ExecuteByCreator");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwner");
			Assert.AreEqual(false, workflowTransition.IsSignature);
			Assert.AreEqual("Identified", workflowTransition.InputStatus.Name, "InputRiskStatusName");
			Assert.AreEqual("Analyzed", workflowTransition.OutputStatus.Name, "OutputRiskStatusName");

			//Now lets modify the data and then save the changes
			//Note that you cannot change the input and output risk statuses
			workflowTransition.StartTracking();
			workflowTransition.Name = "Start Work";
			workflowTransition.IsExecuteByCreator = false;
			workflowTransition.IsExecuteByOwner = false;
			workflowTransition.IsSignature = true;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Start Risk
			Assert.AreEqual("Start Work", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetector");
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner, "ExecuteByOwner");
			Assert.AreEqual(true, workflowTransition.IsSignature);
			Assert.AreEqual("Identified", workflowTransition.InputStatus.Name, "InputRiskStatusName");
			Assert.AreEqual("Analyzed", workflowTransition.OutputStatus.Name, "OutputRiskStatusName");

			//Now lets return the data back to what it was before
			workflowTransition.StartTracking();
			workflowTransition.Name = "Analyze Risk";
			workflowTransition.IsExecuteByCreator = true;
			workflowTransition.IsExecuteByOwner = true;
			workflowTransition.IsSignature = false;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Start Risk
			Assert.AreEqual("Analyze Risk", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator, "ExecuteByDetector");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwner");
			Assert.AreEqual(false, workflowTransition.IsSignature);
			Assert.AreEqual("Identified", workflowTransition.InputStatus.Name, "InputRiskStatusName");
			Assert.AreEqual("Analyzed", workflowTransition.OutputStatus.Name, "OutputRiskStatusName");

			//Now lets create a new workflow transition (Identified > Closed)
			int newWorkflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId, statusIdIdentified, statusIdClosed, "Mark As Closed", false, true, new List<int> { ProjectManager.ProjectRoleTester }).WorkflowTransitionId;

			//Verify that it was created
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, newWorkflowTransitionId);
			Assert.AreEqual("Mark As Closed", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetector");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwner");
			Assert.AreEqual("Identified", workflowTransition.InputStatus.Name, "InputRiskStatusName");
			Assert.AreEqual("Closed", workflowTransition.OutputStatus.Name, "OutputRiskStatusName");

			//Verify that the Tester role was added
			Assert.AreEqual(1, workflowTransition.TransitionRoles.Count);
			Assert.AreEqual((int)ProjectManager.ProjectRoleTester, workflowTransition.TransitionRoles[0].ProjectRoleId);
			Assert.AreEqual("Tester", workflowTransition.TransitionRoles[0].ProjectRole.Name);

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
		SpiraTestCase(1781)
		]
		public void _11_ViewModifyWorkflows()
		{
			//First lets test that we can create a new workflow
			int workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test Workflow", true, true).RiskWorkflowId;

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
			int workflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId2, statusIdIdentified, statusIdAnalyzed, "Execute Risk").WorkflowTransitionId;
			RiskWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId2, workflowTransitionId);
			workflowTransition.StartTracking();
			RiskWorkflowTransitionRole workflowTransitionRole = new RiskWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now lets add some workflow field and custom property states
			RiskStatus riskStatus = workflowManager.Status_RetrieveById(statusIdIdentified, workflowId2); //Not-Started Status
			riskStatus.StartTracking();
			//Field=Required
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Risk);
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "ReleaseId").ArtifactFieldId;
			RiskWorkflowField workflowField = new RiskWorkflowField();
			workflowField.ArtifactFieldId = artifactFieldId;
			workflowField.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			workflowField.RiskWorkflowId = workflowId2;
			riskStatus.WorkflowFields.Add(workflowField);
			//Custom Property = Hidden
			RiskWorkflowCustomProperty workflowCustomProperty = new RiskWorkflowCustomProperty();
			workflowCustomProperty.CustomPropertyId = 1;
			workflowCustomProperty.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			workflowCustomProperty.RiskWorkflowId = workflowId2;
			riskStatus.WorkflowCustomProperties.Add(workflowCustomProperty);
			workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).RiskWorkflowId;
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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).RiskWorkflowId;
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

			//Finally lets try and delete a workflow that is in use (linked to at least one risk type)
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
		SpiraTestCase(1782)
		]
		public void _12_CopyWorkflows()
		{
			//Get the default workflow (so that we can compare the copy against it)
			RiskWorkflow oldWorkflow = workflowManager.Workflow_RetrieveById(workflowId, true);

			//First, we need to make a copy of the default workflow for the project
			int newWorkflowId = workflowManager.Workflow_Copy(workflowId).RiskWorkflowId;

			//Now lets verify some of the counts
			workflow = workflowManager.Workflow_RetrieveById(newWorkflowId, true);
			Assert.IsNotNull(workflow);
			Assert.AreEqual(oldWorkflow.Transitions.Count, workflow.Transitions.Count);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(newWorkflowId);
		}

		[
		Test,
		SpiraTestCase(1783)
		]
		public void _13_GetEnabledFields()
		{
			//Pull fields.
			List<RiskWorkflowField> transitionFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusIdAnalyzed);

			//Verify that the ones we select match up.
			RiskWorkflowField workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "ReleaseId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Release", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);
			workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "RiskImpactId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Impact", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);

			//Make one custom property hidden and one disabled
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, false);
			riskStatus = workflowManager.Status_RetrieveById(statusIdAnalyzed, workflowId);
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			riskStatus.StartTracking();
			RiskWorkflowCustomProperty field = new RiskWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.RiskWorkflowId = workflowId;
			riskStatus.WorkflowCustomProperties.Add(field);
			customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_02").CustomPropertyId;
			riskStatus.StartTracking();
			field = new RiskWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.RiskWorkflowId = workflowId;
			riskStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(riskStatus);

			//Pull custom properties.
			List<RiskWorkflowCustomProperty> transitionCustomProperties = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusIdAnalyzed);

			//Verify that the ones we select match up.
			RiskWorkflowCustomProperty workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_01");
			Assert.AreEqual("Difficulty", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden, workflowCustomProperty.WorkflowFieldStateId);
			workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_02");
			Assert.AreEqual("Reviewer", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive, workflowCustomProperty.WorkflowFieldStateId);
		}

		#region Helper Functions

		/// <summary>
		/// Determines if a field is disabled for the current workflow and risk status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldDisabled(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a field is hidden for the current workflow and risk status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldHidden(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a field is required for the current workflow and risk status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldRequired(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (riskStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		/// <summary>
		/// Determines if a custom property is disabled for the current workflow and risk status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyDisabled(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a custom property is hidden for the current workflow and risk status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyHidden(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a custom property is required for the current workflow and risk status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyRequired(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (riskStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		#endregion
	}
}
