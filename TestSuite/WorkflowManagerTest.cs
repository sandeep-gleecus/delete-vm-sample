using System;
using System.Linq;
using System.Data;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the WorkflowManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class WorkflowManagerTest
	{
		protected static WorkflowManager workflowManager;
		protected static Workflow workflow;
		protected static System.Data.DataSet lookupDataSet;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static IncidentStatus incidentStatus;
		protected static int currentWorkflowId = -1;
		protected static List<ArtifactField> currentArtifactFields = null;
		protected static List<CustomProperty> currentCustomProperties = null;

		private const int PROJECT_ID = 1;
		private const int PROJECT_EMPTY_ID = 3;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;


		[TestFixtureSetUp]
		public void Init()
		{
			workflowManager = new Business.WorkflowManager();

			//Get the last artifact id
			Business.HistoryManager history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);

			//We need to load in the standard field and custom property states first
			currentArtifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Incident);
			currentCustomProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(PROJECT_ID, Artifact.ArtifactTypeEnum.Incident, false);
		}

		[
		Test,
		SpiraTestCase(187)
		]
		public void _01_Retrieve_Lookups()
		{
			//Verify that we can get the default workflow for a project (used when a new incident status is created)
			int workflowId = workflowManager.Workflow_GetDefault(PROJECT_ID).WorkflowId;
			Assert.AreEqual(1, workflowId);

			//Verify that we can get the workflow associated with a specific type
			workflowId = workflowManager.Workflow_GetForIncidentType(1);
			Assert.AreEqual(1, workflowId);
			workflowId = workflowManager.Workflow_GetForIncidentType(2);
			Assert.AreEqual(1, workflowId);
			workflowId = workflowManager.Workflow_GetForIncidentType(3);
			Assert.AreEqual(1, workflowId);
			workflowId = workflowManager.Workflow_GetForIncidentType(4);
			Assert.AreEqual(1, workflowId);
			workflowId = workflowManager.Workflow_GetForIncidentType(5);
			Assert.AreEqual(1, workflowId);
			workflowId = workflowManager.Workflow_GetForIncidentType(6);
			Assert.AreEqual(1, workflowId);
			workflowId = workflowManager.Workflow_GetForIncidentType(7);
			Assert.AreEqual(1, workflowId);
			workflowId = workflowManager.Workflow_GetForIncidentType(8);
			Assert.AreEqual(1, workflowId);
		}

		[
		Test,
		SpiraTestCase(189)
		]
		public void _02_Retrieve_Field_States()
		{
			//Load the field state for IncidentStatus=New
			currentWorkflowId = 1;
			IncidentManager incidentManager = new IncidentManager();
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(1, true);
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("Name"));
			Assert.AreEqual(false, IsFieldDisabled("Description"));
			Assert.AreEqual(true, IsFieldDisabled("ActualEffort"));
			Assert.AreEqual(true, IsFieldDisabled("RemainingEffort"));
			Assert.AreEqual(false, IsFieldDisabled("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("VerifiedReleaseId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("Name"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(true, IsFieldHidden("BuildId"));
			Assert.AreEqual(true, IsFieldHidden("ClosedDate"));
			Assert.AreEqual(true, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("DetectedReleaseId"));
			Assert.AreEqual(true, IsFieldHidden("ResolvedReleaseId"));
			Assert.AreEqual(true, IsFieldHidden("VerifiedReleaseId"));
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("Name"));
			Assert.AreEqual(true, IsFieldRequired("Description"));
			Assert.AreEqual(true, IsFieldRequired("IncidentTypeId"));
			Assert.AreEqual(true, IsFieldRequired("OpenerId"));
			Assert.AreEqual(true, IsFieldRequired("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("VerifiedReleaseId"));

			//Load the field state for IncidentStatus=Open
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(2, true);
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("Name"));
			Assert.AreEqual(false, IsFieldDisabled("Description"));
			Assert.AreEqual(true, IsFieldDisabled("ActualEffort"));
			Assert.AreEqual(true, IsFieldDisabled("RemainingEffort"));
			Assert.AreEqual(false, IsFieldDisabled("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("VerifiedReleaseId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("Name"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("BuildId"));
			Assert.AreEqual(true, IsFieldHidden("ClosedDate"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("ResolvedReleaseId"));
			Assert.AreEqual(true, IsFieldHidden("VerifiedReleaseId"));
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("Name"));
			Assert.AreEqual(true, IsFieldRequired("Description"));
			Assert.AreEqual(true, IsFieldRequired("IncidentTypeId"));
			Assert.AreEqual(true, IsFieldRequired("OpenerId"));
			Assert.AreEqual(false, IsFieldRequired("DetectedReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("PriorityId"));
			Assert.AreEqual(false, IsFieldRequired("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("VerifiedReleaseId"));

			//Load the field state for IncidentStatus=Assigned
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true);
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("Name"));
			Assert.AreEqual(false, IsFieldDisabled("Description"));
			Assert.AreEqual(true, IsFieldDisabled("ActualEffort"));
			Assert.AreEqual(true, IsFieldDisabled("RemainingEffort"));
			Assert.AreEqual(false, IsFieldDisabled("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("VerifiedReleaseId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("Name"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("BuildId"));
			Assert.AreEqual(false, IsFieldHidden("ClosedDate"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("VerifiedReleaseId"));
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("Name"));
			Assert.AreEqual(true, IsFieldRequired("Description"));
			Assert.AreEqual(true, IsFieldRequired("IncidentTypeId"));
			Assert.AreEqual(true, IsFieldRequired("OpenerId"));
			Assert.AreEqual(true, IsFieldRequired("PriorityId"));
			Assert.AreEqual(false, IsFieldRequired("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("VerifiedReleaseId"));

			//Load the field state for IncidentStatus=Resolved
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(4, true);
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("Name"));
			Assert.AreEqual(false, IsFieldDisabled("Description"));
			Assert.AreEqual(false, IsFieldDisabled("ActualEffort"));
			Assert.AreEqual(false, IsFieldDisabled("RemainingEffort"));
			Assert.AreEqual(false, IsFieldDisabled("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("VerifiedReleaseId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("Name"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("BuildId"));
			Assert.AreEqual(false, IsFieldHidden("ClosedDate"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("VerifiedReleaseId"));
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("Name"));
			Assert.AreEqual(true, IsFieldRequired("Description"));
			Assert.AreEqual(true, IsFieldRequired("IncidentTypeId"));
			Assert.AreEqual(true, IsFieldRequired("OpenerId"));
			Assert.AreEqual(true, IsFieldRequired("PriorityId"));
			Assert.AreEqual(false, IsFieldRequired("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("VerifiedReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("StartDate"));

			//Load the field state for IncidentStatus=Closed
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(5, true);
			//Verify disabled fields
			Assert.AreEqual(false, IsFieldDisabled("Name"));
			Assert.AreEqual(false, IsFieldDisabled("Description"));
			Assert.AreEqual(false, IsFieldDisabled("ActualEffort"));
			Assert.AreEqual(false, IsFieldDisabled("RemainingEffort"));
			Assert.AreEqual(false, IsFieldDisabled("ClosedDate"));
			Assert.AreEqual(true, IsFieldDisabled("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldDisabled("VerifiedReleaseId"));
			Assert.AreEqual(true, IsFieldDisabled("PriorityId"));
			//Verify hidden fields
			Assert.AreEqual(false, IsFieldHidden("Name"));
			Assert.AreEqual(false, IsFieldHidden("Description"));
			Assert.AreEqual(false, IsFieldHidden("BuildId"));
			Assert.AreEqual(false, IsFieldHidden("ClosedDate"));
			Assert.AreEqual(false, IsFieldHidden("OwnerId"));
			Assert.AreEqual(false, IsFieldHidden("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldHidden("VerifiedReleaseId"));
			//Verify required fields
			Assert.AreEqual(true, IsFieldRequired("Name"));
			Assert.AreEqual(true, IsFieldRequired("Description"));
			Assert.AreEqual(true, IsFieldRequired("IncidentTypeId"));
			Assert.AreEqual(true, IsFieldRequired("OpenerId"));
			Assert.AreEqual(false, IsFieldRequired("PriorityId"));
			Assert.AreEqual(false, IsFieldRequired("DetectedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("ResolvedReleaseId"));
			Assert.AreEqual(false, IsFieldRequired("VerifiedReleaseId"));
			Assert.AreEqual(true, IsFieldRequired("ClosedDate"));
		}

		[
		Test,
		SpiraTestCase(190)
		]
		public void _03_Retrieve_Custom_Property_States()
		{
			//Load the custom property state for IncidentStatus=New
			currentWorkflowId = 1;
			IncidentManager incidentManager = new IncidentManager();
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(1, true);
			//Verify hidden fields
			Assert.AreEqual(true, IsCustomPropertyHidden("Custom_05"));   //Rank
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_09"));  //Decimal
																		  //Verify disabled fields
			Assert.AreEqual(true, IsCustomPropertyDisabled("Custom_09"));  //Decimal
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Operating System
																		   //Verify required fields
			Assert.AreEqual(true, IsCustomPropertyRequired("Custom_02"));  //Operating System
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_05")); //Rank

			//Load the custom property state for IncidentStatus=Open
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(2, true);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_05"));   //Rank
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_09"));  //Decimal
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_09"));  //Decimal
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Operating System
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Operating System
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_05")); //Rank

			//Load the custom property state for IncidentStatus=Assigned
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_05"));   //Rank
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_09"));  //Decimal
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_09"));  //Decimal
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Operating System
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Operating System
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_05")); //Rank

			//Load the custom property state for IncidentStatus=Resolved
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(4, true);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_05"));   //Rank
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_09"));  //Decimal
																		  //Verify disabled fields
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_09"));  //Decimal
			Assert.AreEqual(false, IsCustomPropertyDisabled("Custom_02")); //Operating System
																		   //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Operating System
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_05")); //Rank
			Assert.AreEqual(true, IsCustomPropertyRequired("Custom_01")); //Notes

			//Load the custom property state for IncidentStatus=Closed
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(5, true);
			//Verify hidden fields
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_05"));   //Rank
			Assert.AreEqual(false, IsCustomPropertyHidden("Custom_09"));  //Decimal
																		  //Verify disabled fields
			Assert.AreEqual(true, IsCustomPropertyDisabled("Custom_09"));  //Decimal
			Assert.AreEqual(true, IsCustomPropertyDisabled("Custom_02")); //Operating System
																		  //Verify required fields
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_02"));  //Operating System
			Assert.AreEqual(false, IsCustomPropertyRequired("Custom_05")); //Rank
			Assert.AreEqual(true, IsCustomPropertyRequired("Custom_01")); //Notes
		}

		[
		Test,
		SpiraTestCase(188)
		]
		public void _04_RetrievesTransitionsForStep()
		{
			//Verify the output transitions returned for a given input statuses (varies by role)
			//InputStatus=New, Role=Manager
			List<WorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(1, 1, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(2, workflowTransitions.Count);
			Assert.AreEqual("Assign Incident", workflowTransitions[0].Name);
			Assert.AreEqual("Review Incident", workflowTransitions[1].Name);

			//InputStatus=Open, Role=Manager
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(1, 2, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(1, workflowTransitions.Count);
			Assert.AreEqual("Assign Incident", workflowTransitions[0].Name);

			//InputStatus=Assigned, Role=Manager
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(1, 3, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(0, workflowTransitions.Count);

			//InputStatus=Assigned, Role=Manager, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(1, 3, InternalRoutines.PROJECT_ROLE_MANAGER, false, true);
			Assert.AreEqual(3, workflowTransitions.Count);
			Assert.AreEqual("Duplicate Incident", workflowTransitions[0].Name);
			Assert.AreEqual("Resolve Incident", workflowTransitions[1].Name);
			Assert.AreEqual("Unable to Reproduce", workflowTransitions[2].Name);

			//InputStatus=Resolved, Role=Manager
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(1, 4, InternalRoutines.PROJECT_ROLE_MANAGER, false, false);
			Assert.AreEqual(1, workflowTransitions.Count);
			Assert.AreEqual("Close Incident", workflowTransitions[0].Name);

			//InputStatus=Resolved, Role=Developer
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(1, 4, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, false);
			Assert.AreEqual(0, workflowTransitions.Count);

			//InputStatus=Resolved, Role=Developer, Owner=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(1, 4, InternalRoutines.PROJECT_ROLE_DEVELOPER, false, true);
			//Assert.AreEqual(1, workflowTransitions.Count);
			//Assert.AreEqual("Reopen Incident", workflowTransitions[0].Name);

			//InputStatus=Resolved, Role=Developer, Detector=Y
			workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(1, 4, InternalRoutines.PROJECT_ROLE_DEVELOPER, true, false);
			Assert.AreEqual(1, workflowTransitions.Count);
			Assert.AreEqual("Close Incident", workflowTransitions[0].Name);
			//Assert.AreEqual("Reopen Incident", workflowTransitions[1].Name);
		}

		[
		Test,
		SpiraTestCase(192)
		]
		public void _05_Retrieves()
		{
			//Lets test that we can retrieve a workflow by id
			workflow = workflowManager.Workflow_RetrieveById(2);

			//Verify data
			Assert.IsNotNull(workflow);
			Assert.AreEqual("Default Workflow", workflow.Name);
			Assert.AreEqual(false, workflow.IsNotify);
			Assert.AreEqual(true, workflow.IsActive);
			Assert.AreEqual(true, workflow.IsDefault);

			//Lets test that we can retrieve a transition based on input and output status
			//New > Open
			WorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(1, 1, 2);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Review Incident", workflowTransition.Name);
			Assert.IsTrue(String.IsNullOrEmpty(workflowTransition.NotifySubject));
			Assert.AreEqual(false, workflowTransition.IsNotifyDetector);
			Assert.AreEqual(false, workflowTransition.IsNotifyOwner);
			Assert.AreEqual(false, workflowTransition.IsExecuteByDetector);
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner);

			//Lets test that we can retrieve a transition based on input and output status
			//Open > Assigned
			workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(1, 2, 3);

			//Verify data
			Assert.IsNotNull(workflowTransition);
			Assert.AreEqual("Assign Incident", workflowTransition.Name);
			Assert.AreEqual("A ${IncidentTypeId} has been assigned to ${Owner} in product ${ProjectName}", workflowTransition.NotifySubject);
			Assert.AreEqual(false, workflowTransition.IsNotifyDetector);
			Assert.AreEqual(true, workflowTransition.IsNotifyOwner);
			Assert.AreEqual(false, workflowTransition.IsExecuteByDetector);
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner);
		}

		[
		Test,
		SpiraTestCase(183)
		]
		public void _07_ViewModifyWorkflowStepFields()
		{
			//First verify that we can retrieve all the workflow-configurable fields for an incident
			List<ArtifactField> artifactFields = new ArtifactManager().ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(18, artifactFields.Count);

			//Now lets verify that a particular field is required/active (DetectedReleaseId=7)
			IncidentManager incidentManager = new IncidentManager();
			currentWorkflowId = 1;
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			Assert.IsFalse(IsFieldHidden("DetectedReleaseId"));
			Assert.IsFalse(IsFieldDisabled("DetectedReleaseId"));
			Assert.IsFalse(IsFieldRequired("DetectedReleaseId"));

			//Now lets change the disabled setting for this field
			int artifactFieldId = artifactFields.FirstOrDefault(f => f.Name == "DetectedReleaseId").ArtifactFieldId;
			//Disabled=True
			incidentStatus.StartTracking();
			WorkflowField field = new WorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive;
			field.WorkflowId = currentWorkflowId;
			incidentStatus.WorkflowFields.Add(field);
			incidentManager.IncidentStatus_Update(incidentStatus);

			//Now refresh the data and verify
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			Assert.IsFalse(IsFieldHidden("DetectedReleaseId"));
			Assert.IsTrue(IsFieldDisabled("DetectedReleaseId"));
			Assert.IsFalse(IsFieldRequired("DetectedReleaseId"));

			//Now lets change the required setting for this field
			//Required=True
			incidentStatus.StartTracking();
			field = new WorkflowField();
			field.ArtifactFieldId = artifactFieldId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.WorkflowId = currentWorkflowId;
			incidentStatus.WorkflowFields.Add(field);
			//Disabled=False
			incidentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowId == currentWorkflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive).MarkAsDeleted();
			incidentManager.IncidentStatus_Update(incidentStatus);

			//Now refresh the data and verify
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			Assert.IsFalse(IsFieldHidden("DetectedReleaseId"));
			Assert.IsFalse(IsFieldDisabled("DetectedReleaseId"));
			Assert.IsTrue(IsFieldRequired("DetectedReleaseId"));

			//Finally change the data back
			incidentStatus.StartTracking();
			incidentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowId == currentWorkflowId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			incidentManager.IncidentStatus_Update(incidentStatus);

			//Now refresh the data and verify it was returned back
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			Assert.IsFalse(IsFieldHidden("DetectedReleaseId"));
			Assert.IsFalse(IsFieldDisabled("DetectedReleaseId"));
			Assert.IsFalse(IsFieldRequired("DetectedReleaseId"));
		}

		[
		Test,
		SpiraTestCase(197)
		]
		public void _08_ViewModifyWorkflowStepCustomProperties()
		{
			//First verify that we can retrieve all the custom properties for an incident
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(PROJECT_ID, Artifact.ArtifactTypeEnum.Incident, false);
			Assert.AreEqual(9, customProperties.Count);

			//Now lets verify that a particular property is required/active (Notes=Custom_01)
			IncidentManager incidentManager = new IncidentManager();
			currentWorkflowId = 1;
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=True
			int customPropertyId = customProperties.FirstOrDefault(c => c.CustomPropertyFieldName == "Custom_01").CustomPropertyId;
			incidentStatus.StartTracking();
			WorkflowCustomProperty field = new WorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			field.WorkflowId = currentWorkflowId;
			incidentStatus.WorkflowCustomProperties.Add(field);
			incidentManager.IncidentStatus_Update(incidentStatus);

			//Now refresh the data and verify
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsTrue(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));

			//Now lets change the workflow state for this custom property
			//Hidden=False, Required=True
			incidentStatus.StartTracking();
			field = new WorkflowCustomProperty();
			field.CustomPropertyId = customPropertyId;
			field.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			field.WorkflowId = currentWorkflowId;
			incidentStatus.WorkflowCustomProperties.Add(field);
			incidentStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.WorkflowId == currentWorkflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden).MarkAsDeleted();
			incidentManager.IncidentStatus_Update(incidentStatus);

			//Now refresh the data and verify
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsTrue(IsCustomPropertyRequired("Custom_01"));

			//Finally change the data back
			incidentStatus.StartTracking();
			incidentStatus.WorkflowCustomProperties.FirstOrDefault(c => c.CustomPropertyId == customPropertyId && c.WorkflowId == currentWorkflowId && c.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required).MarkAsDeleted();
			incidentManager.IncidentStatus_Update(incidentStatus);

			//Now refresh the data and verify that it's cleaned up
			incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			Assert.IsFalse(IsCustomPropertyDisabled("Custom_01"));
			Assert.IsFalse(IsCustomPropertyHidden("Custom_01"));
			Assert.IsFalse(IsCustomPropertyRequired("Custom_01"));
		}

		[
		Test,
		SpiraTestCase(198)
		]
		public void _09_ViewModifyWorkflowRoles()
		{
			//First lets get the various assigned roles for a specific transition
			//For this test we'll use the Duplicate > Reopen transition (WT12)
			WorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, 12);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			List<WorkflowTransitionRole> executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);

			//Now the notify roles
			List<WorkflowTransitionRole> notifyRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify).OrderBy(t => t.ProjectRoleId).ToList();
			//Assert.AreEqual(0, notifyRoles.Count);

			//Now lets add a couple of roles
			workflowTransition.StartTracking();
			WorkflowTransitionRole workflowTransitionRole = new WorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowTransitionRole = new WorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_MANAGER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Verify the changes
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, 12);
			Assert.AreEqual(4, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(3, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);
			Assert.AreEqual("Developer", executeRoles[2].Role.Name);

			//Now the notify roles
			notifyRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(1, notifyRoles.Count);
			Assert.AreEqual("Manager", notifyRoles[0].Role.Name);

			//Now delete the ones we just added
			workflowTransition.StartTracking();
			workflowTransition.TransitionRoles.FirstOrDefault(t => t.ProjectRoleId == InternalRoutines.PROJECT_ROLE_DEVELOPER && t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).MarkAsDeleted();
			workflowTransition.TransitionRoles.FirstOrDefault(t => t.ProjectRoleId == InternalRoutines.PROJECT_ROLE_MANAGER && t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify).MarkAsDeleted();
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Verify the changes
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, 12);
			Assert.AreEqual(2, workflowTransition.TransitionRoles.Count);

			//First the execute roles
			executeRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(2, executeRoles.Count);
			Assert.AreEqual("Product Owner", executeRoles[0].Role.Name);
			Assert.AreEqual("Manager", executeRoles[1].Role.Name);

			//Now the notify roles
			notifyRoles = workflowTransition.TransitionRoles.Where(t => t.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify).OrderBy(t => t.ProjectRoleId).ToList();
			Assert.AreEqual(0, notifyRoles.Count);
		}

		[
		Test,
		SpiraTestCase(199)
		]
		public void _10_ViewModifyWorkflowTransitions()
		{
			//First lets retrieve a single workflow transition and verify its data
			WorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, 8);  //Close Incident
			Assert.AreEqual("Close Incident", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByDetector, "ExecuteByDetectorYn");
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsNotifyDetector, "NotifyDetectorYn");
			Assert.AreEqual(false, workflowTransition.IsNotifyOwner, "NotifyOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.IsTrue(String.IsNullOrEmpty(workflowTransition.NotifySubject));
			Assert.AreEqual("Resolved", workflowTransition.InputStatus.Name, "InputIncidentStatusName");
			Assert.AreEqual("Closed", workflowTransition.OutputStatus.Name, "OutputIncidentStatusName");

			//Now lets modify the data and then save the changes
			//Note that you cannot change the input and output incident statuses
			workflowTransition.StartTracking();
			workflowTransition.Name = "Close The Incident";
			workflowTransition.IsExecuteByDetector = false;
			workflowTransition.IsExecuteByOwner = true;
			workflowTransition.IsNotifyDetector = true;
			workflowTransition.IsNotifyOwner = true;
			workflowTransition.IsSignatureRequired = true;
			workflowTransition.NotifySubject = "Incident IN${IncidentId} was closed";
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, 8); //Close Incident
			Assert.AreEqual("Close The Incident", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByDetector, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(true, workflowTransition.IsNotifyDetector, "NotifyDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsNotifyOwner, "NotifyOwnerYn");
			Assert.AreEqual(true, workflowTransition.IsSignatureRequired);
			Assert.AreEqual("Incident IN${IncidentId} was closed", workflowTransition.NotifySubject);
			Assert.AreEqual("Resolved", workflowTransition.InputStatus.Name, "InputIncidentStatusName");
			Assert.AreEqual("Closed", workflowTransition.OutputStatus.Name, "OutputIncidentStatusName");

			//Now lets return the data back to what it was before
			workflowTransition.StartTracking();
			workflowTransition.Name = "Close Incident";
			workflowTransition.IsExecuteByDetector = true;
			workflowTransition.IsExecuteByOwner = false;
			workflowTransition.IsNotifyDetector = false;
			workflowTransition.IsNotifyOwner = false;
			workflowTransition.NotifySubject = null;
			workflowTransition.IsSignatureRequired = false;
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now verify the updates
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, 8); //Close Incident
			Assert.AreEqual("Close Incident", workflowTransition.Name, "Name");
			Assert.AreEqual(true, workflowTransition.IsExecuteByDetector, "ExecuteByDetectorYn");
			Assert.AreEqual(false, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsNotifyDetector, "NotifyDetectorYn");
			Assert.AreEqual(false, workflowTransition.IsNotifyOwner, "NotifyOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsSignatureRequired);
			Assert.IsTrue(String.IsNullOrEmpty(workflowTransition.NotifySubject));
			Assert.AreEqual("Resolved", workflowTransition.InputStatus.Name, "InputIncidentStatusName");
			Assert.AreEqual("Closed", workflowTransition.OutputStatus.Name, "OutputIncidentStatusName");

			//Now lets create a new workflow transition (Not Reproducible > Duplicate)
			int workflowTransitionId = workflowManager.WorkflowTransition_Insert(1, 6, 7, "Duplicate & Not Reproducible").WorkflowTransitionId;

			//Verify that it was created
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, workflowTransitionId);
			Assert.AreEqual("Duplicate & Not Reproducible", workflowTransition.Name, "Name");
			Assert.AreEqual(false, workflowTransition.IsExecuteByDetector, "ExecuteByDetectorYn");
			Assert.AreEqual(true, workflowTransition.IsExecuteByOwner, "ExecuteByOwnerYn");
			Assert.AreEqual(false, workflowTransition.IsNotifyDetector, "NotifyDetectorYn");
			Assert.AreEqual(false, workflowTransition.IsNotifyOwner, "NotifyOwnerYn");
			Assert.IsTrue(String.IsNullOrEmpty(workflowTransition.NotifySubject));
			Assert.AreEqual("Assigned", workflowTransition.InputStatus.Name, "InputIncidentStatusName");
			Assert.AreEqual("Duplicate", workflowTransition.OutputStatus.Name, "OutputIncidentStatusName");

			//Now lets add a couple of roles (to make sure they are deleted with the transition)
			workflowTransition.StartTracking();
			WorkflowTransitionRole workflowTransitionRole = new WorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowTransitionRole = new WorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_MANAGER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now delete the transition
			workflowManager.WorkflowTransition_Delete(workflowTransitionId);

			//Verify the delete
			bool isDeleted = false;
			try
			{
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, workflowTransitionId);
			}
			catch (ArtifactNotExistsException)
			{
				isDeleted = true;
			}
			Assert.IsTrue(isDeleted);
		}

		[
		Test,
		SpiraTestCase(200)
		]
		public void _11_ViewModifyWorkflows()
		{
			//First lets test that we can create a new workflow
			int workflowId = workflowManager.Workflow_Insert(PROJECT_ID, "Test Workflow", true, true, true).WorkflowId;

			//Now verify that it inserted correctly
			workflow = workflowManager.Workflow_RetrieveById(workflowId);
			Assert.AreEqual("Test Workflow", workflow.Name);
			Assert.AreEqual(true, workflow.IsDefault);
			Assert.AreEqual(true, workflow.IsActive);
			Assert.AreEqual(true, workflow.IsNotify);

			//Now lets update the workflow
			workflow.StartTracking();
			workflow.Name = "Customized Workflow";
			workflow.IsDefault = false;
			workflow.IsActive = false;
			workflow.IsNotify = false;
			workflowManager.Workflow_Update(workflow);

			//Now verify that it updated correctly
			workflow = workflowManager.Workflow_RetrieveById(workflowId);
			Assert.AreEqual("Customized Workflow", workflow.Name);
			Assert.AreEqual(false, workflow.IsDefault);
			Assert.AreEqual(false, workflow.IsActive);
			Assert.AreEqual(false, workflow.IsNotify);

			//Now lets add some transitions and transition roles
			int workflowTransitionId = workflowManager.WorkflowTransition_Insert(workflowId, 6, 7, "Duplicate & Not Reproducible").WorkflowTransitionId;
			WorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId);
			workflowTransition.StartTracking();
			WorkflowTransitionRole workflowTransitionRole = new WorkflowTransitionRole();
			workflowTransitionRole.WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute;
			workflowTransitionRole.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			workflowTransition.TransitionRoles.Add(workflowTransitionRole);
			workflowManager.WorkflowTransition_Update(workflowTransition);

			//Now lets add some workflow field and custom property states
			IncidentManager incidentManager = new IncidentManager();
			IncidentStatus incidentStatus = incidentManager.IncidentStatus_RetrieveById(3, true); //Assigned Status
			incidentStatus.StartTracking();
			//Field=Required
			WorkflowField workflowField = new WorkflowField();
			workflowField.ArtifactFieldId = 7;
			workflowField.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required;
			workflowField.WorkflowId = workflowId;
			incidentStatus.WorkflowFields.Add(workflowField);
			//Custom Property = Hidden
			WorkflowCustomProperty workflowCustomProperty = new WorkflowCustomProperty();
			workflowCustomProperty.CustomPropertyId = 1;
			workflowCustomProperty.WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden;
			workflowCustomProperty.WorkflowId = workflowId;
			incidentStatus.WorkflowCustomProperties.Add(workflowCustomProperty);
			incidentManager.IncidentStatus_Update(incidentStatus);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(workflowId);

			//Now we need to verify that the various exception cases are detected and thrown by the business object
			//First lets try and insert a new workflow that is marked as default but inactive
			bool exceptionMatch = false;
			try
			{
				workflowManager.Workflow_Insert(PROJECT_ID, "Test exception workflow", true, true, false);
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
			workflowId = workflowManager.Workflow_Insert(PROJECT_ID, "Test exception workflow", true, true, true).WorkflowId;
			try
			{
				workflow = workflowManager.Workflow_RetrieveById(workflowId);
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
			workflowId = workflowManager.Workflow_Insert(PROJECT_ID, "Test exception workflow", true, true, true).WorkflowId;
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
				//Test case to ensure you can't delete a default workflow
				Assert.IsTrue(exceptionMatch, "Must not be able to delete a default workflow");
			}

			//Now we need to make it inactive and then delete it
			workflow = workflowManager.Workflow_RetrieveById(workflowId);
			workflow.StartTracking();
			workflow.IsActive = false;
			workflow.IsDefault = false;
			workflowManager.Workflow_Update(workflow);
			workflowManager.Workflow_Delete(workflowId);

			//Finally lets try and delete a workflow that is in use (linked to at least one incident type)
			exceptionMatch = false;
			try
			{
				workflowManager.Workflow_Delete(1);
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
		SpiraTestCase(201)
		]
		public void _12_CopyWorkflows()
		{
			//First, we need to make a copy of the default workflow for the project
			int newWorkflowId = workflowManager.Workflow_Copy(1).WorkflowId;

			//Now lets verify some of the counts
			workflow = workflowManager.Workflow_RetrieveById(newWorkflowId, true);
			Assert.IsNotNull(workflow);
			Assert.AreEqual(16, workflow.Transitions.Count);

			//Finally lets delete the workflow
			workflowManager.Workflow_Delete(newWorkflowId);
		}

		[
		Test,
		SpiraTestCase(584)
		]
		public void _13_GetEnabledFields()
		{
			//Pull a test incident.
			IncidentManager incidentManager = new IncidentManager();
			Workflow workflow = new Workflow();
			Incident incident = incidentManager.RetrieveById(3, false);
			//Get the workflowID for the incident type.
			IncidentType incidentType = incidentManager.RetrieveIncidentTypeById(incident.IncidentTypeId);
			int workflowId = incidentType.WorkflowId;

			//Pull fields.
			List<WorkflowField> transitionFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, incident.IncidentStatusId);

			//Verify that the ones we select match up.
			WorkflowField workflowField = transitionFields.FirstOrDefault(f => f.Field.Name == "ResolvedReleaseId");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Planned Release", workflowField.Field.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden, workflowField.WorkflowFieldStateId);
			workflowField = transitionFields.FirstOrDefault(f => f.Field.Name == "Name");
			Assert.IsNotNull(workflowField);
			Assert.AreEqual("Name", workflowField.Field.Caption);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowField.WorkflowFieldStateId);

			//Pull custom properties.
			List<WorkflowCustomProperty> transitionCustomProperties = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, incident.IncidentStatusId);

			//Verify that the ones we select match up.
			WorkflowCustomProperty workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_05");
			Assert.AreEqual("Ranking", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden, workflowCustomProperty.WorkflowFieldStateId);
			workflowCustomProperty = transitionCustomProperties.FirstOrDefault(c => c.CustomProperty.CustomPropertyFieldName == "Custom_02");
			Assert.AreEqual("Operating System", workflowCustomProperty.CustomProperty.Name);
			Assert.AreEqual((int)WorkflowFieldState.WorkflowFieldStateEnum.Required, workflowCustomProperty.WorkflowFieldStateId);
		}

		[Test]
		public void _XX_CleanUp()
		{
			//We need to delete any artifact history items created during the test run
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());
		}

		#region Helper Functions

		/// <summary>
		/// Determines if a field is disabled for the current workflow and incident status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldDisabled(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (incidentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.WorkflowId == currentWorkflowId) != null);
		}

		/// <summary>
		/// Determines if a field is hidden for the current workflow and incident status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldHidden(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (incidentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.WorkflowId == currentWorkflowId) != null);
		}

		/// <summary>
		/// Determines if a field is required for the current workflow and incident status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsFieldRequired(string fieldName)
		{
			int artifactFieldId = currentArtifactFields.FirstOrDefault(f => f.Name == fieldName).ArtifactFieldId;
			return (incidentStatus.WorkflowFields.FirstOrDefault(f => f.ArtifactFieldId == artifactFieldId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.WorkflowId == currentWorkflowId) != null);
		}

		/// <summary>
		/// Determines if a custom property is disabled for the current workflow and incident status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyDisabled(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (incidentStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.WorkflowId == currentWorkflowId) != null);
		}

		/// <summary>
		/// Determines if a custom property is hidden for the current workflow and incident status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyHidden(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (incidentStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden && f.WorkflowId == currentWorkflowId) != null);
		}

		/// <summary>
		/// Determines if a custom property is required for the current workflow and incident status
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>Whether it is or not</returns>
		protected bool IsCustomPropertyRequired(string fieldName)
		{
			int customPropertyId = currentCustomProperties.FirstOrDefault(c => c.CustomPropertyFieldName == fieldName).CustomPropertyId;
			return (incidentStatus.WorkflowCustomProperties.FirstOrDefault(f => f.CustomPropertyId == customPropertyId && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required && f.WorkflowId == currentWorkflowId) != null);
		}

		#endregion
	}
}
