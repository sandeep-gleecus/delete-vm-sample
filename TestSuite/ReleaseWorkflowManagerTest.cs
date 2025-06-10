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
	/// This fixture tests the ReleaseWorkflowManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class ReleaseWorkflowManagerTest
	{
		protected static ReleaseWorkflowManager workflowManager;
		protected static ReleaseWorkflow workflow;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static List<ArtifactField> currentArtifactFields = null;
		protected static List<CustomProperty> currentCustomProperties = null;
		protected static ReleaseStatus releaseStatus;
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
			workflowManager = new ReleaseWorkflowManager();

			//Create a new project for testing with (only some tests currently use this)
			//This will create the default workflow entries (for standard fields)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			projectId = projectManager.Insert("ReleaseWorkflowManagerTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Need to add some custom properties for releases in this new project
			CustomPropertyManager cpm = new CustomPropertyManager();
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Release, (int)CustomProperty.CustomPropertyTypeEnum.Text, 1, "Notes", null, null, null);
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Release, (int)CustomProperty.CustomPropertyTypeEnum.Decimal, 2, "Rank", null, null, null);

			//We need to load in the standard field and custom property states first
			currentArtifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Release);
			currentCustomProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);
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
		SpiraTestCase(1359)
		]
		public void _01_Retrieve_Lookups()
		{
			//Verify that we can get the default workflow for a project
			ReleaseWorkflow workflow = workflowManager.Workflow_GetDefault(projectTemplateId);
			Assert.AreEqual("Default Workflow", workflow.Name);
			Assert.AreEqual(true, workflow.IsActive);
			Assert.AreEqual(true, workflow.IsDefault);
			int defaultWorkflowId = workflow.ReleaseWorkflowId;

			//Verify that we can get the workflow associated with a specific type
			workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, (int)Release.ReleaseTypeEnum.Iteration);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, (int)Release.ReleaseTypeEnum.MajorRelease);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, (int)Release.ReleaseTypeEnum.MinorRelease);
			Assert.AreEqual(defaultWorkflowId, workflowId);
			workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, (int)Release.ReleaseTypeEnum.Phase);
			Assert.AreEqual(defaultWorkflowId, workflowId);
		}

		[
		Test,
		SpiraTestCase(1360)
		]
		public void _02_Retrieve_Field_States()
		{
			//Get the default workflow for this project
			workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).ReleaseWorkflowId;

			//Load the field state for ReleaseStatus=Planned
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("VersionNumber"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("VersionNumber"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));
			Assert.AreEqual(false, IsFieldDisabled("StartDate"));
			Assert.AreEqual(false, IsFieldDisabled("VersionNumber"));

			//Load the field state for ReleaseStatus=InProgress
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.InProgress, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("VersionNumber"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("VersionNumber"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));
			Assert.AreEqual(false, IsFieldDisabled("StartDate"));
			Assert.AreEqual(false, IsFieldDisabled("VersionNumber"));

			//Load the field state for ReleaseStatus=Completed
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Completed, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("VersionNumber"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("VersionNumber"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("OwnerId"));
			Assert.AreEqual(true, IsFieldDisabled("StartDate"));
			Assert.AreEqual(true, IsFieldDisabled("VersionNumber"));

			//Load the field state for ReleaseStatus=Closed
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Closed, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("VersionNumber"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("VersionNumber"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("OwnerId"));
			Assert.AreEqual(true, IsFieldDisabled("StartDate"));
			Assert.AreEqual(true, IsFieldDisabled("VersionNumber"));

			//Load the field state for ReleaseStatus=Deferred
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Deferred, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("VersionNumber"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("VersionNumber"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("OwnerId"));
			Assert.AreEqual(false, IsFieldDisabled("StartDate"));
			Assert.AreEqual(false, IsFieldDisabled("VersionNumber"));

			//Load the field state for ReleaseStatus=Cancelled
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Cancelled, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("OwnerId"));
			Assert.AreEqual(false, IsFieldRequired("StartDate"));
			Assert.AreEqual(false, IsFieldRequired("VersionNumber"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("StartDate"));
			Assert.AreEqual(false, IsFieldHidden("VersionNumber"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("OwnerId"));
			Assert.AreEqual(true, IsFieldDisabled("StartDate"));
			Assert.AreEqual(true, IsFieldDisabled("VersionNumber"));
		}

		[
		Test,
		SpiraTestCase(1361)
		]
		public void _03_Retrieve_Custom_Property_States()
		{
			//Load the custom property state for ReleaseStatus=Planned
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for ReleaseStatus=InProgress
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.InProgress, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for ReleaseStatus=Completed
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Completed, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for ReleaseStatus=In Closed
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Closed, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for ReleaseStatus=Deferred
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Deferred, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Notes
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Rank
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Rank
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Notes
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02")); //Rank

			//Load the custom property state for ReleaseStatus=Cancelled
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Cancelled, workflowId);
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
		SpiraTestCase(1362)
		]
		public void _04_RetrievesTransitionsForStep()
		{
			//Verify the output transitions returned for a given input statuses (varies by role)
			//InputStatus=Planned, Role=Manager
			List<ReleaseWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Release.ReleaseStatusEnum.Planned, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(3, workflowTransitions.Count);
			Assert.AreEqual("Cancel Release", workflowTransitions[0].Name);
			Assert.AreEqual("Defer Release", workflowTransitions[1].Name);
			Assert.AreEqual("Start Release", workflowTransitions[2].Name);

			//InputStatus=InProgress, Role=Manager
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Release.ReleaseStatusEnum.InProgress, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(3, workflowTransitions.Count);
			Assert.AreEqual("Cancel Release", workflowTransitions[0].Name);
			Assert.AreEqual("Defer Release", workflowTransitions[1].Name);
			Assert.AreEqual("Finish Release", workflowTransitions[2].Name);

			//InputStatus=InProgress, Role=Developer, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Release.ReleaseStatusEnum.InProgress, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, true);
			Assert.AreEqual(3, workflowTransitions.Count);
			Assert.AreEqual("Cancel Release", workflowTransitions[0].Name);
			Assert.AreEqual("Defer Release", workflowTransitions[1].Name);
			Assert.AreEqual("Finish Release", workflowTransitions[2].Name);

			//InputStatus=Completed, Role=Developer, Creator=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, (int)Release.ReleaseStatusEnum.Completed, InternalRoutines.PROJECT_ROLE_DEVELOPER, true, false);
			Assert.AreEqual(1, workflowTransitions.Count);
			Assert.AreEqual("Continue Release", workflowTransitions[0].Name);
		}

		[
		Test,
		SpiraTestCase(1363)
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
			//Planned > In Progress
			ReleaseWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, (int)Release.ReleaseStatusEnum.Planned, (int)Release.ReleaseStatusEnum.InProgress);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Start Release", workflowTransition.Name);
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);

			//Lets test that we can retrieve a transition based on input and output status
			//In Progress > Completed
			workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, (int)Release.ReleaseStatusEnum.InProgress, (int)Release.ReleaseStatusEnum.Completed);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Finish Release", workflowTransition.Name);
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator);
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner);
		}

		[
		Test,
		SpiraTestCase(1364)
		]
		public void _07_ViewModifyWorkflowStepFields()
		{
			//First verify that we can retrieve all the workflow-configurable fields for an release
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Release);
			Assert.AreEqual(15, artifactFields.Count);

			//Now lets verify that a particular field is required/active (Release)
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			Assert.IsFalse(IsFieldHidden("OwnerId"));
			Assert.IsFalse(IsFieldDisabled("OwnerId"));
			Assert.IsFalse(IsFieldRequired("OwnerId"));

			//Now lets change the disabled setting for this field
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "OwnerId").ArtifactFieldId;
			//Disabled=True
			releaseStatus.StartTracking();
			ReleaseWorkflowField field = new ReleaseWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.ReleaseWorkflowId = workflowId;
			releaseStatus.WorkflowFields.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(releaseStatus);

			//Now refresh the data and verify
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			Assert.IsFalse(IsFieldHidden("OwnerId"));
			Assert.IsTrue(IsFieldDisabled("OwnerId"));
			Assert.IsFalse(IsFieldRequired("OwnerId"));

			//Now lets change the required setting for this field
			//Required=True
			releaseStatus.StartTracking();
			field = new ReleaseWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.ReleaseWorkflowId = workflowId;
			releaseStatus.WorkflowFields.Add(field);
			//Disabled=False
			releaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.ReleaseWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(releaseStatus);

			//Now refresh the data and verify
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			Assert.IsFalse(IsFieldHidden("OwnerId"));
			Assert.IsFalse(IsFieldDisabled("OwnerId"));
			Assert.IsTrue(IsFieldRequired("OwnerId"));

			//Finally change the data back
			releaseStatus.StartTracking();
			releaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.ReleaseWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(releaseStatus);

			//Now refresh the data and verify it was returned back
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			Assert.IsFalse(IsFieldHidden("OwnerId"));
			Assert.IsFalse(IsFieldDisabled("OwnerId"));
			Assert.IsFalse(IsFieldRequired("OwnerId"));
		}

		[
		Test,
		SpiraTestCase(1365)
		]
		public void _08_ViewModifyWorkflowStepCustomProperties()
		{
			//First verify that we can retrieve all the custom properties for an release
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);
			Assert.AreEqual(2, customProperties.Count);

			//Now lets verify that a particular property is required/active (Notes=Custom_01)
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=True
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			releaseStatus.StartTracking();
			ReleaseWorkflowCustomProperty field = new ReleaseWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.ReleaseWorkflowId = workflowId;
			releaseStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(releaseStatus);

			//Now refresh the data and verify
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsTrue(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=False, Required=True
			releaseStatus.StartTracking();
			field = new ReleaseWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.ReleaseWorkflowId = workflowId;
			releaseStatus.WorkflowCustomProperties.Add(field);
			releaseStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.ReleaseWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(releaseStatus);

			//Now refresh the data and verify
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsTrue(IsCustomPropertyRequired("Custom_01"));

			//Finally change the data back
			releaseStatus.StartTracking();
			releaseStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.ReleaseWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(releaseStatus);

			//Now refresh the data and verify that it's cleaned up
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));
		}

		[
		Test,
		SpiraTestCase(1366)
		]
		public void _09_ViewModifyWorkflowRoles()
		{
			//First lets get the various assigned roles for a specific transition
			//For this test we'll use the Planned > In Progress
			ReleaseWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			ReleaseWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned && t.OutputReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			//Test that we can get the transition by its ID
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			List<ReleaseWorkflowTransitionRole> executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);

			//Now lets add a couple of roles
			workflowTransition.StartTracking();
			ReleaseWorkflowTransitionRole workflowTransitionRole = new ReleaseWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowTransitionRole = new ReleaseWorkflowTransitionRole();
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
		SpiraTestCase(1367)
		]
		public void _10_ViewModifyWorkflowTransitions()
		{
			//First lets retrieve a single workflow transition and verify its data
			//For this test we'll use the Planned > In Progress
			ReleaseWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			ReleaseWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned && t.OutputReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Accept Release
			Assert.AreEqual("Start Release", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Planned", workflowTransition.InputReleaseStatus.Name, "InputReleaseStatusName");
			Assert.AreEqual("In Progress", workflowTransition.OutputReleaseStatus.Name, "OutputReleaseStatusName");

			//Now lets modify the data and then save the changes
			//Note that you cannot change the input and output release statuses
			workflowTransition.StartTracking();
			workflowTransition.Name = "Commence Release";
			workflowTransition.IsExecuteByCreator = false;
			workflowTransition.IsExecuteByOwner = true;
			workflowTransition.IsSignatureRequired = true;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Accept Release
			Assert.AreEqual("Commence Release", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(true, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Planned", workflowTransition.InputReleaseStatus.Name, "InputReleaseStatusName");
			Assert.AreEqual("In Progress", workflowTransition.OutputReleaseStatus.Name, "OutputReleaseStatusName");

			//Now lets return the data back to what it was before
			workflowTransition.StartTracking();
			workflowTransition.Name = "Start Release";
			workflowTransition.IsExecuteByCreator = true;
			workflowTransition.IsExecuteByOwner = true;
			workflowTransition.IsSignatureRequired = false;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Accept Release
			Assert.AreEqual("Start Release", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Planned", workflowTransition.InputReleaseStatus.Name, "InputReleaseStatusName");
			Assert.AreEqual("In Progress", workflowTransition.OutputReleaseStatus.Name, "OutputReleaseStatusName");

			//Now lets create a new workflow transition (Planned > Completed)
			int newWorkflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId, (int)Release.ReleaseStatusEnum.Planned, (int)Release.ReleaseStatusEnum.Completed, "Mark As Complete", false, true, new List<int> { ProjectManager.ProjectRoleTester }).WorkflowTransitionId;

			//Verify that it was created
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, newWorkflowTransitionId);
			Assert.AreEqual("Mark As Complete", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByCreator, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual("Planned", workflowTransition.InputReleaseStatus.Name, "InputReleaseStatusName");
			Assert.AreEqual("Completed", workflowTransition.OutputReleaseStatus.Name, "OutputReleaseStatusName");

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
		SpiraTestCase(1368)
		]
		public void _11_ViewModifyWorkflows()
		{
			//First lets test that we can create a new workflow
			int workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test Workflow", true, true).ReleaseWorkflowId;

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
			int workflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId2, (int)Release.ReleaseStatusEnum.Planned, (int)Release.ReleaseStatusEnum.Planned, "Plan Release").WorkflowTransitionId;
			ReleaseWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId2, workflowTransitionId);
			workflowTransition.StartTracking();
			ReleaseWorkflowTransitionRole workflowTransitionRole = new ReleaseWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now lets add some workflow field and custom property states
			ReleaseStatus releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId2); //Requested Status
			releaseStatus.StartTracking();
			//Field=Required
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Release);
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "OwnerId").ArtifactFieldId;
			ReleaseWorkflowField workflowField = new ReleaseWorkflowField();
			workflowField.ArtifactFieldId = artifactFieldId;
			workflowField.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			workflowField.ReleaseWorkflowId = workflowId2;
			releaseStatus.WorkflowFields.Add(workflowField);
			//Custom Property = Hidden
			ReleaseWorkflowCustomProperty workflowCustomProperty = new ReleaseWorkflowCustomProperty();
			workflowCustomProperty.CustomPropertyId = 1;
			workflowCustomProperty.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			workflowCustomProperty.ReleaseWorkflowId = workflowId2;
			releaseStatus.WorkflowCustomProperties.Add(workflowCustomProperty);
			workflowManager.Status_UpdateFieldsAndCustomProperties(releaseStatus);

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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).ReleaseWorkflowId;
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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).ReleaseWorkflowId;
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

			//Finally lets try and delete a workflow that is in use (linked to at least one release type)
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
		SpiraTestCase(1369)
		]
		public void _12_CopyWorkflows()
		{
			//Get the default workflow (so that we can compare the copy against it)
			ReleaseWorkflow oldWorkflow = workflowManager.Workflow_RetrieveById(workflowId, true);

			//First, we need to make a copy of the default workflow for the project
			int newWorkflowId = workflowManager.Workflow_Copy(workflowId).ReleaseWorkflowId;

			//Now lets verify some of the counts
			workflow = workflowManager.Workflow_RetrieveById(newWorkflowId, true);
			Assert.IsNotNull(workflow);
			Assert.AreEqual(oldWorkflow.Transitions.Count, workflow.Transitions.Count);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(newWorkflowId);
		}

		[
		Test,
		SpiraTestCase(1370)
		]
		public void _13_GetEnabledFields()
		{
			//Pull fields.
			List<ReleaseWorkflowField> transitionFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, (int)Release.ReleaseStatusEnum.Completed);

			//Verify that the ones we select match up.
			ReleaseWorkflowField workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "StartDate");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Start Date", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive, workflowField.WorkflowFieldStateId);
			workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "OwnerId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Owned By", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive, workflowField.WorkflowFieldStateId);

			//Make one custom property hidden and one disabled
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);
			releaseStatus = workflowManager.Status_RetrieveById((int)Release.ReleaseStatusEnum.Planned, workflowId);
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			releaseStatus.StartTracking();
			ReleaseWorkflowCustomProperty field = new ReleaseWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.ReleaseWorkflowId = workflowId;
			releaseStatus.WorkflowCustomProperties.Add(field);
			customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_02").CustomPropertyId;
			releaseStatus.StartTracking();
			field = new ReleaseWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.ReleaseWorkflowId = workflowId;
			releaseStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(releaseStatus);

			//Pull custom properties.
			List<ReleaseWorkflowCustomProperty> transitionCustomProperties = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, (int)Release.ReleaseStatusEnum.Planned);

			//Verify that the ones we select match up.
			ReleaseWorkflowCustomProperty workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_01");
			Assert.AreEqual("Notes", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden, workflowCustomProperty.WorkflowFieldStateId);
			workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_02");
			Assert.AreEqual("Rank", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive, workflowCustomProperty.WorkflowFieldStateId);
		}

		#region Helper Functions

		/// <summary>
		/// Determines if a field is disabled for the current workflow and release status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldDisabled(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (releaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a field is hidden for the current workflow and release status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldHidden(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (releaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a field is required for the current workflow and release status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldRequired(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (releaseStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		/// <summary>
		/// Determines if a custom property is disabled for the current workflow and release status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyDisabled(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (releaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a custom property is hidden for the current workflow and release status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyHidden(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (releaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a custom property is required for the current workflow and release status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyRequired(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (releaseStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		#endregion
	}
}
