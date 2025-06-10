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
	/// This fixture tests the DocumentWorkflowManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class DocumentWorkflowManagerTest
	{
		protected static DocumentWorkflowManager workflowManager;
		protected static DocumentWorkflow workflow;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static List<ArtifactField> currentArtifactFields = null;
		protected static List<CustomProperty> currentCustomProperties = null;
		protected static DocumentStatus DocumentStatus;
		protected static int projectId;
		protected static int projectTemplateId;
		protected static int workflowId;

		//statuses for the project template
		protected static int statusIdDraft;
		protected static int statusIdUnderReview;
		protected static int statusIdApproved;
		protected static int statusIdCompleted;
		protected static int statusIdRejected;
		protected static int statusIdRetired;
		protected static int statusIdCheckedOut;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;


		[TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the class being tested
			workflowManager = new DocumentWorkflowManager();

			//Create a new project for testing with (only some tests currently use this)
			//This will create the default workflow entries (for standard fields)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("DocumentWorkflowManagerTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Need to add some custom properties for Documents in this new project
			CustomPropertyManager cpm = new CustomPropertyManager();
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Document, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "Difficulty", null, null, null);
			cpm.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Document, (int)CustomProperty.CustomPropertyTypeEnum.User, 2, "Reviewer", null, null, null);

			//We need to load in the standard field and custom property states first
			currentArtifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Document);
			currentCustomProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, false);

			//Write out the statuses for use in tests
			AttachmentManager attachmentManager = new AttachmentManager();
			List<DocumentStatus> statuses = attachmentManager.DocumentStatus_Retrieve(projectTemplateId, false, false);
			statusIdDraft = statuses.FirstOrDefault(c => c.Name == "Draft").DocumentStatusId;
			statusIdUnderReview = statuses.FirstOrDefault(c => c.Name == "Under Review").DocumentStatusId;
			statusIdApproved = statuses.FirstOrDefault(c => c.Name == "Approved").DocumentStatusId;
			statusIdCompleted = statuses.FirstOrDefault(c => c.Name == "Completed").DocumentStatusId;
			statusIdRejected = statuses.FirstOrDefault(c => c.Name == "Rejected").DocumentStatusId;
			statusIdRetired = statuses.FirstOrDefault(c => c.Name == "Retired").DocumentStatusId;
			statusIdCheckedOut = statuses.FirstOrDefault(c => c.Name == "Checked Out").DocumentStatusId;
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
		SpiraTestCase(1741)
		]
		public void _01_Retrieve_Lookups()
		{
			//Verify that we can get the default workflow for a project
			DocumentWorkflow workflow = workflowManager.Workflow_GetDefault(projectTemplateId);
			Assert.AreEqual("Default Workflow", workflow.Name, "Testing Workflow Name");
			Assert.AreEqual(true, workflow.IsActive, "is workflow active");
			Assert.AreEqual(true, workflow.IsDefault, "is workflow the default");
			int defaultWorkflowId = workflow.DocumentWorkflowId;

			AttachmentManager documentManager = new AttachmentManager();
			List<DocumentType> types = documentManager.RetrieveDocumentTypes(projectTemplateId, true);

			//Verify that we can get the workflow associated with a specific type
			workflowId = workflowManager.Workflow_GetForDocumentType(types[0].DocumentTypeId);
			Assert.AreEqual(defaultWorkflowId, workflowId);
		}

		[
		Test,
		SpiraTestCase(1746)
		]
		public void _02_Retrieve_Field_States()
		{
			//Get the default workflow for this project
			workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).DocumentWorkflowId;

			//Load the field state for DocumentStatus=Draft
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EditorId"));
			Assert.AreEqual(false, IsFieldRequired("Description"));
			Assert.AreEqual(true, IsFieldRequired("DocumentTypeId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EditorId"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("DocumentTypeId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("EditorId"));
			Assert.AreEqual(false, IsFieldDisabled("Description"));
			Assert.AreEqual(false, IsFieldDisabled("DocumentTypeId"));

			//Load the field state for DocumentStatus=UnderReview
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdUnderReview, workflowId);
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("EditorId"));
			Assert.AreEqual(false, IsFieldRequired("Description"));
			Assert.AreEqual(true, IsFieldRequired("DocumentTypeId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EditorId"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("DocumentTypeId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("EditorId"));
			Assert.AreEqual(false, IsFieldDisabled("Description"));
			Assert.AreEqual(false, IsFieldDisabled("DocumentTypeId"));

			//Load the field state for DocumentStatus=Completed
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdCompleted, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EditorId"));
			Assert.AreEqual(false, IsFieldRequired("Description"));
			Assert.AreEqual(false, IsFieldRequired("DocumentTypeId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EditorId"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("DocumentTypeId"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("EditorId"));
			Assert.AreEqual(true, IsFieldDisabled("Description"));
			Assert.AreEqual(true, IsFieldDisabled("DocumentTypeId"));

			//Load the field state for DocumentStatus=Rejected
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdRejected, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EditorId"));
			Assert.AreEqual(false, IsFieldRequired("Description"));
			Assert.AreEqual(true, IsFieldRequired("DocumentTypeId"));
			//Verify hidden fields
			Assert.AreEqual(true, IsFieldHidden("EditorId"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("DocumentTypeId"));
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("EditorId"));
			Assert.AreEqual(false, IsFieldDisabled("Description"));
			Assert.AreEqual(false, IsFieldDisabled("DocumentTypeId"));

			//Load the field state for DocumentStatus=CheckedOut
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdCheckedOut, workflowId);
			//Verify required fields
			Assert.AreEqual(false, IsFieldRequired("EditorId"));
			Assert.AreEqual(false, IsFieldRequired("Description"));
			Assert.AreEqual(false, IsFieldRequired("DocumentTypeId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("EditorId"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("DocumentTypeId"));
			//Verify disabled fields
			Assert.AreEqual(true, IsFieldDisabled("EditorId"));
			Assert.AreEqual(true, IsFieldDisabled("Description"));
			Assert.AreEqual(true, IsFieldDisabled("DocumentTypeId"));
		}

		[
		Test,
		SpiraTestCase(1745)
		]
		public void _03_Retrieve_Custom_Property_States()
		{
			//Load the custom property state for DocumentStatus=Draft
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));    //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));    //Reviewer
																			//Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02"));  //Reviewer
																			//Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Reviewer

			//Load the custom property state for DocumentStatus=UnderReview
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdUnderReview, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));   //Reviewer
																		   //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02"));  //Reviewer
																			//Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Reviewer

			//Load the custom property state for DocumentStatus=Completed
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdCompleted, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02"));  //Reviewer
																			//Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Reviewer

			//Load the custom property state for DocumentStatus=Rejected
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdRejected, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02"));  //Reviewer
																			//Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Reviewer

			//Load the custom property state for DocumentStatus=CheckedOut
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdCheckedOut, workflowId);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_01"));   //Difficulty
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_02"));  //Reviewer
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02"));  //Reviewer
																			//Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_01"));  //Difficulty
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Reviewer
		}

		[
		Test,
		SpiraTestCase(1749)
		]
		public void _04_RetrievesTransitionsForStep()
		{
			//Verify the output transitions returned for a given input statuses (varies by role)
			//InputStatus=Draft, Role=Manager
			List<DocumentWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdDraft, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(1, workflowTransitions.Count);
			Assert.AreEqual("Review Document", workflowTransitions[0].Name);

			//InputStatus=Draft, Role=Developer
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdDraft, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, false);
			Assert.AreEqual(0, workflowTransitions.Count);

			//InputStatus=UnderReview, Role=Manager
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdUnderReview, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(3, workflowTransitions.Count);
			Assert.AreEqual("Approve Document", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Document", workflowTransitions[1].Name);
			Assert.AreEqual("Return to Draft", workflowTransitions[2].Name);

			//InputStatus=UnderReview, Role=Developer
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdUnderReview, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, false);
			Assert.AreEqual(0, workflowTransitions.Count);

			//InputStatus=Approved, Role=Developer, Author=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdUnderReview, InternalRoutines.PROJECT_ROLE_DEVELOPER, true, false);
			Assert.AreEqual(1, workflowTransitions.Count);
			Assert.AreEqual("Return to Draft", workflowTransitions[0].Name);

			//InputStatus=Approved, Role=Tester, Editor=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusIdUnderReview, InternalRoutines.PROJECT_ROLE_TESTER, false, true);
			Assert.AreEqual(3, workflowTransitions.Count);
			Assert.AreEqual("Approve Document", workflowTransitions[0].Name);
			Assert.AreEqual("Reject Document", workflowTransitions[1].Name);
			Assert.AreEqual("Return to Draft", workflowTransitions[2].Name);
		}

		[
		Test,
		SpiraTestCase(1748)
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
			//UnderReview > Approved
			DocumentWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, statusIdUnderReview, statusIdApproved);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Approve Document", workflowTransition.Name);
			Assert.AreEqual(false, workflowTransition.IsExecuteByAuthor);
			Assert.AreEqual(true, workflowTransition.IsExecuteByEditor);

			//Lets test that we can retrieve a transition based on input and output status
			//Rejected > UnderReview
			workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, statusIdRejected, statusIdUnderReview);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Return to Review", workflowTransition.Name);
			Assert.AreEqual(true, workflowTransition.IsExecuteByAuthor);
			Assert.AreEqual(true, workflowTransition.IsExecuteByEditor);
		}

		[
		Test,
		SpiraTestCase(1752)
		]
		public void _07_ViewModifyWorkflowStepFields()
		{
			//First verify that we can retrieve all the workflow-configurable fields for a Document
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Document);
			Assert.AreEqual(9, artifactFields.Count);

			//Now lets verify that a particular field is required/active (Release)
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			Assert.IsFalse(IsFieldHidden("Description"));
			Assert.IsFalse(IsFieldDisabled("Description"));
			Assert.IsFalse(IsFieldRequired("Description"));

			//Now lets change the disabled setting for this field
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "Description").ArtifactFieldId;
			//Disabled=True
			DocumentStatus.StartTracking();
			DocumentWorkflowField field = new DocumentWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.DocumentWorkflowId = workflowId;
			DocumentStatus.WorkflowFields.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(DocumentStatus);

			//Now refresh the data and verify
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			Assert.IsFalse(IsFieldHidden("Description"));
			Assert.IsTrue(IsFieldDisabled("Description"));
			Assert.IsFalse(IsFieldRequired("Description"));

			//Now lets change the required setting for this field
			//Required=True
			DocumentStatus.StartTracking();
			field = new DocumentWorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.DocumentWorkflowId = workflowId;
			DocumentStatus.WorkflowFields.Add(field);
			//Disabled=False
			DocumentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.DocumentWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(DocumentStatus);

			//Now refresh the data and verify
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			Assert.IsFalse(IsFieldHidden("Description"));
			Assert.IsFalse(IsFieldDisabled("Description"));
			Assert.IsTrue(IsFieldRequired("Description"));

			//Finally change the data back
			DocumentStatus.StartTracking();
			DocumentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.DocumentWorkflowId == workflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(DocumentStatus);

			//Now refresh the data and verify it was returned back
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			Assert.IsFalse(IsFieldHidden("Description"));
			Assert.IsFalse(IsFieldDisabled("Description"));
			Assert.IsFalse(IsFieldRequired("Description"));
		}

		[
		Test,
		SpiraTestCase(1751)
		]
		public void _08_ViewModifyWorkflowStepCustomProperties()
		{
			//First verify that we can retrieve all the custom properties for an Document
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, false);
			Assert.AreEqual(2, customProperties.Count);

			//Now lets verify that a particular property is required/active (Difficulty=Custom_01)
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=True
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			DocumentStatus.StartTracking();
			DocumentWorkflowCustomProperty field = new DocumentWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.DocumentWorkflowId = workflowId;
			DocumentStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(DocumentStatus);

			//Now refresh the data and verify
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsTrue(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=False, Required=True
			DocumentStatus.StartTracking();
			field = new DocumentWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.DocumentWorkflowId = workflowId;
			DocumentStatus.WorkflowCustomProperties.Add(field);
			DocumentStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.DocumentWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(DocumentStatus);

			//Now refresh the data and verify
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsTrue(IsCustomPropertyRequired("Custom_01"));

			//Finally change the data back
			DocumentStatus.StartTracking();
			DocumentStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.DocumentWorkflowId == workflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			workflowManager.Status_UpdateFieldsAndCustomProperties(DocumentStatus);

			//Now refresh the data and verify that it's cleaned up
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));
		}

		[
		Test,
		SpiraTestCase(1750)
		]
		public void _09_ViewModifyWorkflowRoles()
		{
			//First lets get the various assigned roles for a specific transition
			//For this test we'll use the Draft > UnderReview
			DocumentWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			DocumentWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputDocumentStatusId == statusIdDraft && t.OutputDocumentStatusId == statusIdUnderReview);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			//Test that we can get the transition by its ID
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			List<DocumentWorkflowTransitionRole> executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);

			//Now lets add a couple of roles
			workflowTransition.StartTracking();
			DocumentWorkflowTransitionRole workflowTransitionRole = new DocumentWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowTransitionRole = new DocumentWorkflowTransitionRole();
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
		SpiraTestCase(1753)
		]
		public void _10_ViewModifyWorkflowTransitions()
		{
			//First lets retrieve a single workflow transition and verify its data
			//For this test we'll use the Draft > UnderReview
			DocumentWorkflow workflow = workflowManager.Workflow_RetrieveById(workflowId, true);
			DocumentWorkflowTransition workflowTransition = workflow.Transitions.FirstOrDefault(t => t.InputDocumentStatusId == statusIdDraft && t.OutputDocumentStatusId == statusIdUnderReview);
			int workflowTransitionId = workflowTransition.WorkflowTransitionId;

			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Review Document
			Assert.AreEqual("Review Document", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByAuthor, "ExecuteByAuthor");
			Assert.AreEqual(true, workflowTransition.IsExecuteByEditor, "ExecuteByEditor");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Draft", workflowTransition.InputDocumentStatus.Name, "InputDocumentStatusName");
			Assert.AreEqual("Under Review", workflowTransition.OutputDocumentStatus.Name, "OutputDocumentStatusName");

			//Now lets modify the data and then save the changes
			//Note that you cannot change the input and output Document statuses
			workflowTransition.StartTracking();
			workflowTransition.Name = "Start Work";
			workflowTransition.IsExecuteByAuthor = true;
			workflowTransition.IsExecuteByEditor = true;
			workflowTransition.IsSignatureRequired = true;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Review Document
			Assert.AreEqual("Start Work", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByAuthor, "ExecuteByAuthor");
			Assert.AreEqual(true, workflowTransition.IsExecuteByEditor, "ExecuteByEditor");
			Assert.AreEqual(true, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Draft", workflowTransition.InputDocumentStatus.Name, "InputDocumentStatusName");
			Assert.AreEqual("Under Review", workflowTransition.OutputDocumentStatus.Name, "OutputDocumentStatusName");

			//Now lets return the data back to what it was before
			workflowTransition.StartTracking();
			workflowTransition.Name = "Start Document";
			workflowTransition.IsExecuteByAuthor = false;
			workflowTransition.IsExecuteByEditor = true;
			workflowTransition.IsSignatureRequired = false;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId); //Review Document
			Assert.AreEqual("Start Document", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByAuthor, "ExecuteByAuthor");
			Assert.AreEqual(true, workflowTransition.IsExecuteByEditor, "ExecuteByEditor");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Draft", workflowTransition.InputDocumentStatus.Name, "InputDocumentStatusName");
			Assert.AreEqual("Under Review", workflowTransition.OutputDocumentStatus.Name, "OutputDocumentStatusName");

			//Now lets create a new workflow transition (Draft > Approved)
			int newWorkflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId, statusIdDraft, statusIdApproved, "Mark As Approved", false, true, new List<int> { ProjectManager.ProjectRoleTester }).WorkflowTransitionId;

			//Verify that it was created
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, newWorkflowTransitionId);
			Assert.AreEqual("Mark As Approved", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByAuthor, "ExecuteByAuthor");
			Assert.AreEqual(true, workflowTransition.IsExecuteByEditor, "ExecuteByEditor");
			Assert.AreEqual("Draft", workflowTransition.InputDocumentStatus.Name, "InputDocumentStatusName");
			Assert.AreEqual("Approved", workflowTransition.OutputDocumentStatus.Name, "OutputDocumentStatusName");

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
		SpiraTestCase(1754)
		]
		public void _11_ViewModifyWorkflows()
		{
			//First lets test that we can create a new workflow
			int workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test Workflow", true, true).DocumentWorkflowId;

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
			int workflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId2, statusIdDraft, statusIdUnderReview, "Move From Draft To Review").WorkflowTransitionId;
			DocumentWorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId2, workflowTransitionId);
			workflowTransition.StartTracking();
			DocumentWorkflowTransitionRole workflowTransitionRole = new DocumentWorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now lets add some workflow field and custom property states
			DocumentStatus DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId2);
			DocumentStatus.StartTracking();
			//Field=Required
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Document);
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "DocumentTypeId").ArtifactFieldId;
			DocumentWorkflowField workflowField = new DocumentWorkflowField();
			workflowField.ArtifactFieldId = artifactFieldId;
			workflowField.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			workflowField.DocumentWorkflowId = workflowId2;
			DocumentStatus.WorkflowFields.Add(workflowField);
			//Custom Property = Hidden
			DocumentWorkflowCustomProperty workflowCustomProperty = new DocumentWorkflowCustomProperty();
			workflowCustomProperty.CustomPropertyId = 1;
			workflowCustomProperty.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			workflowCustomProperty.DocumentWorkflowId = workflowId2;
			DocumentStatus.WorkflowCustomProperties.Add(workflowCustomProperty);
			workflowManager.Status_UpdateFieldsAndCustomProperties(DocumentStatus);

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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).DocumentWorkflowId;
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
			workflowId2 = workflowManager.Workflow_Insert(projectTemplateId, "Test exception workflow", true, true).DocumentWorkflowId;
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

			//Finally lets try and delete a workflow that is in use (linked to at least one Document type)
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
		SpiraTestCase(1743)
		]
		public void _12_CopyWorkflows()
		{
			//Get the default workflow (so that we can compare the copy against it)
			DocumentWorkflow oldWorkflow = workflowManager.Workflow_RetrieveById(workflowId, true);

			//First, we need to make a copy of the default workflow for the project
			int newWorkflowId = workflowManager.Workflow_Copy(workflowId).DocumentWorkflowId;

			//Now lets verify some of the counts
			workflow = workflowManager.Workflow_RetrieveById(newWorkflowId, true);
			Assert.IsNotNull(workflow);
			Assert.AreEqual(oldWorkflow.Transitions.Count, workflow.Transitions.Count);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(newWorkflowId);
		}

		[
		Test,
		SpiraTestCase(1744)
		]
		public void _13_GetEnabledFields()
		{
			//Pull fields.
			List<DocumentWorkflowField> transitionFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusIdDraft);

			//Verify that the ones we select match up.
			DocumentWorkflowField workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "DocumentTypeId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Type", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);

			workflowField = transitionFields.FirstOrDefault(f => f.ArtifactField.Name == "AuthorId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Author", workflowField.ArtifactField.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);

			//Make one custom property hidden and one disabled
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Document, false);
			DocumentStatus = workflowManager.Status_RetrieveById(statusIdDraft, workflowId);
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			DocumentStatus.StartTracking();
			DocumentWorkflowCustomProperty field = new DocumentWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.DocumentWorkflowId = workflowId;
			DocumentStatus.WorkflowCustomProperties.Add(field);
			customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_02").CustomPropertyId;
			DocumentStatus.StartTracking();
			field = new DocumentWorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.DocumentWorkflowId = workflowId;
			DocumentStatus.WorkflowCustomProperties.Add(field);
			workflowManager.Status_UpdateFieldsAndCustomProperties(DocumentStatus);

			//Pull custom properties.
			List<DocumentWorkflowCustomProperty> transitionCustomProperties = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusIdDraft);

			//Verify that the ones we select match up.
			DocumentWorkflowCustomProperty workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_01");
			Assert.AreEqual("Difficulty", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden, workflowCustomProperty.WorkflowFieldStateId);
			workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_02");
			Assert.AreEqual("Reviewer", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive, workflowCustomProperty.WorkflowFieldStateId);
		}

		#region Helper Functions

		/// <summary>
		/// Determines if a field is disabled for the current workflow and Document status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldDisabled(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (DocumentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a field is hidden for the current workflow and Document status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldHidden(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (DocumentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a field is required for the current workflow and Document status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldRequired(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (DocumentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		/// <summary>
		/// Determines if a custom property is disabled for the current workflow and Document status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyDisabled(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (DocumentStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive) != null);
		}

		/// <summary>
		/// Determines if a custom property is hidden for the current workflow and Document status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyHidden(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (DocumentStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden) != null);
		}

		/// <summary>
		/// Determines if a custom property is required for the current workflow and Document status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyRequired(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (DocumentStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) != null);
		}

		#endregion
	}
}
