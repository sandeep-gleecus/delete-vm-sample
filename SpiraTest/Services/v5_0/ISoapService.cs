using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.v5_0.DataObjects;
using Inflectra.SpiraTest.Web.Services.Wsdl.Documentation;

#pragma warning disable 1591

namespace Inflectra.SpiraTest.Web.Services.v5_0
{
	[ServiceContract(Namespace = "http://www.inflectra.com/SpiraTest/Services/v5.0/", SessionMode = SessionMode.Allowed)]
	[XmlComments(XmlCommentFormat.Default, "Inflectra.SpiraTest.Web.Services.v5_0.SoapService")]
    public interface ISoapService : IService
    {
        #region Data Sync System

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        List<RemoteDataSyncSystem> DataSyncSystem_Retrieve();

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        RemoteDataSyncSystem DataSyncSystem_RetrieveById(int dataSyncSystemId);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        RemoteDataSyncSystem DataSyncSystem_Create(RemoteDataSyncSystem remoteDataSyncSystem);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_Update(RemoteDataSyncSystem remoteDataSyncSystem);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunFailure(int dataSyncSystemId);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunSuccess(int dataSyncSystemId, DateTime lastRunDate);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunWarning(int dataSyncSystemId, DateTime lastRunDate);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_WriteEvent(string message, string details, int eventLogEntryType);

        #endregion

        #region Custom Property
        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteCustomProperty> CustomProperty_RetrieveForArtifactType(int artifactTypeId, bool includeDeleted);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteCustomList CustomProperty_AddCustomList(RemoteCustomList remoteCustomList);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteCustomListValue CustomProperty_AddCustomListValue(RemoteCustomListValue remoteCustomListValue);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteCustomProperty CustomProperty_AddDefinition(RemoteCustomProperty remoteCustomProperty, int? customListId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void CustomProperty_UpdateDefinition(RemoteCustomProperty remoteCustomProperty);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void CustomProperty_DeleteDefinition(int customPropertyId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteCustomList CustomProperty_RetrieveCustomListById(int customListId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteCustomList> CustomProperty_RetrieveCustomLists();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void CustomProperty_UpdateCustomList(RemoteCustomList remoteCustomList);
		#endregion

		#region Data Mapping
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void DataMapping_AddArtifactMappings(int dataSyncSystemId, int artifactTypeId, List<RemoteDataMapping> remoteDataMappings);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void DataMapping_AddUserMappings(int dataSyncSystemId, List<RemoteDataMapping> remoteDataMappings);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void DataMapping_RemoveArtifactMappings(int dataSyncSystemId, int artifactTypeId, List<RemoteDataMapping> remoteDataMappings);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveArtifactMappings(int dataSyncSystemId, int artifactTypeId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDataMapping DataMapping_RetrieveCustomPropertyMapping(int dataSyncSystemId, int artifactTypeId, int customPropertyId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveCustomPropertyValueMappings(int dataSyncSystemId, int artifactTypeId, int customPropertyId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveFieldValueMappings(int dataSyncSystemId, int artifactFieldId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveProjectMappings(int dataSyncSystemId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveUserMappings(int dataSyncSystemId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteProjectArtifact> DataMapping_SearchArtifactMappings(int dataSyncSystemId, int artifactTypeId, string externalKey);

		#endregion

		#region Document
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		byte[] Document_OpenFile(int attachmentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocument Document_AddFile(RemoteDocument remoteDocument, byte[] binaryData);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocument Document_AddUrl(RemoteDocument remoteDocument);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentVersion Document_AddFileVersion(RemoteDocumentVersion remoteDocumentVersion, byte[] binaryData, bool makeCurrent);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentVersion Document_AddUrlVersion(RemoteDocumentVersion remoteDocumentVersion, bool makeCurrent);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_DeleteFromArtifact(int attachmentId, int artifactTypeId, int artifactId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_Delete(int attachmentId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteDocument> Document_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDocument> Document_RetrieveForFolder(int folderId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDocument> Document_RetrieveForArtifact(int artifactTypeId, int artifactId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_AddToArtifactId(int artifactTypeId, int artifactId, int attachmentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocument Document_RetrieveById(int attachmentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDocumentType> Document_RetrieveTypes(bool activeOnly);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDocumentFolder> Document_RetrieveFolders();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentFolder Document_RetrieveFolderById(int folderId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentFolder Document_AddFolder(RemoteDocumentFolder remoteDocumentFolder);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_DeleteFolder(int projectAttachmentFolderId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_UpdateFolder(RemoteDocumentFolder remoteDocumentFolder);
		#endregion

		#region Association
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAssociation Association_Create(RemoteAssociation remoteAssociation);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Association_Update(RemoteAssociation remoteAssociation);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAssociation> Association_RetrieveForArtifact(int artifactTypeId, int artifactId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort);
		#endregion

		#region Incident
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncidentPriority Incident_AddPriority(RemoteIncidentPriority remoteIncidentPriority);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Incident_Count(List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Incident_AddComments(List<RemoteComment> remoteComments);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncidentSeverity Incident_AddSeverity(RemoteIncidentSeverity remoteIncidentSeverity);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncidentStatus Incident_AddStatus(RemoteIncidentStatus remoteIncidentStatus);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncidentType Incident_AddType(RemoteIncidentType remoteIncidentType);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteIncident Incident_Create(RemoteIncident remoteIncident);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncident Incident_RetrieveById(int incidentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveByTestCase(int testCaseId, bool openOnly);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveByTestRunStep(int testRunStepId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveByTestStep(int testStepId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveForOwner();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveNew(DateTime creationDate, int startRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncidentPriority> Incident_RetrievePriorities();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Incident_RetrieveComments(int incidentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncidentSeverity> Incident_RetrieveSeverities();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncidentStatus> Incident_RetrieveStatuses();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncidentType> Incident_RetrieveTypes();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowField> Incident_RetrieveWorkflowFields(int currentTypeId, int currentStatusId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowTransition> Incident_RetrieveWorkflowTransitions(int currentTypeId, int currentStatusId, bool isDetector, bool isOwner);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowCustomProperty> Incident_RetrieveWorkflowCustomProperties(int currentTypeId, int currentStatusId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Incident_Update(RemoteIncident remoteIncident);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Incident_Delete(int incidentId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteIncidentStatus Incident_RetrieveDefaultStatus();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteIncidentType Incident_RetrieveDefaultType();

		#endregion

		#region Project
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteProject Project_Create(RemoteProject remoteProject, int? existingProjectId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Project_Delete(int projectId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteProject> Project_Retrieve();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteProject Project_RetrieveById(int projectId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteProjectUser> Project_RetrieveUserMembership();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Project_RefreshProgressExecutionStatusCaches(int? releaseId, bool runInBackground);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Project_AddUserMembership(RemoteProjectUser remoteProjectUser);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Project_UpdateUserMembership(RemoteProjectUser remoteProjectUser);

		#endregion

		#region ProjectRole
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteProjectRole> ProjectRole_Retrieve();
		#endregion

		#region Release
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_AddTestMapping(RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_AddTestMapping2(RemoteReleaseTestCaseMapping[] remoteReleaseTestCaseMappings);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Release_Count(List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteRelease Release_Create(RemoteRelease remoteRelease, int? parentReleaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_RemoveTestMapping(RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRelease> Release_Retrieve(bool activeOnly);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRelease> Release_Retrieve2(List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteRelease Release_RetrieveById(int releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Release_Update(RemoteRelease remoteRelease);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_Delete(int releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_Move(int releaseId, int? destinationReleaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Release_Indent(int releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Release_Outdent(int releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Release_RetrieveComments(int ReleaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteReleaseTestCaseMapping> Release_RetrieveTestMapping(int releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment Release_CreateComment(RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteReleaseStatus> Release_RetrieveStatuses();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteReleaseType> Release_RetrieveTypes();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowField> Release_RetrieveWorkflowFields(int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowTransition> Release_RetrieveWorkflowTransitions(int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowCustomProperty> Release_RetrieveWorkflowCustomProperties(int currentTypeId, int currentStatusId);

		#endregion

		#region Requirement
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Requirement_AddTestCoverage(RemoteRequirementTestCaseMapping remoteReqTestCaseMapping);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Requirement_Count(List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteRequirement Requirement_Create1(RemoteRequirement remoteRequirement, int indentPosition);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteRequirement Requirement_Create2(RemoteRequirement remoteRequirement, int? parentRequirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Requirement_RemoveTestCoverage(RemoteRequirementTestCaseMapping remoteReqTestCaseMapping);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRequirement> Requirement_Retrieve(List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteRequirement Requirement_RetrieveById(int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRequirement> Requirement_RetrieveForOwner();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRequirementTestCaseMapping> Requirement_RetrieveTestCoverage(int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Requirement_Update(RemoteRequirement remoteRequirement);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Requirement_Delete(int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Requirement_Move(int requirementId, int? destinationRequirementId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_Indent(int requirementId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_Outdent(int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Requirement_RetrieveComments(int RequirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment Requirement_CreateComment(RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementStatus> Requirement_RetrieveStatuses();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementType> Requirement_RetrieveTypes();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowField> Requirement_RetrieveWorkflowFields(int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowTransition> Requirement_RetrieveWorkflowTransitions(int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowCustomProperty> Requirement_RetrieveWorkflowCustomProperties(int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementStep> Requirement_RetrieveSteps(int requirementId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteRequirementStep Requirement_RetrieveStepById(int requirementStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteRequirementStep Requirement_AddStep(RemoteRequirementStep remoteRequirementStep, int? existingRequirementStepId, int? creatorId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_UpdateStep(RemoteRequirementStep remoteRequirementStep);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_MoveStep(int requirementId, int sourceRequirementStepId, int? destinationRequirementStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_DeleteStep(int requirementId, int requirementStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_AddTestStepCoverage(RemoteRequirementTestStepMapping remoteReqTestStepMapping);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementTestStepMapping> Requirement_RetrieveTestStepCoverage(int requirementId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_RemoveTestStepCoverage(RemoteRequirementTestStepMapping remoteReqTestStepMapping);

        #endregion

        #region Test Step

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementTestStepMapping> TestStep_RetrieveRequirementCoverage(int testStepId);

        #endregion

        #region System
        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteVersion System_GetProductVersion();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteSetting> System_GetSettings();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		string System_GetArtifactUrl(int navigationLinkId, int projectId, int artifactId, string tabName);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		int System_GetProjectIdForArtifact(int artifactTypeId, int artifactId);
		#endregion

		#region Task
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteTask Task_Create(RemoteTask remoteTask);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTask> Task_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteTask Task_RetrieveById(int taskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTask> Task_RetrieveForOwner();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTask> Task_RetrieveNew(DateTime creationDate, int startRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Task_Update(RemoteTask remoteTask);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Task_Count(List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Task_Delete(int taskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Task_RetrieveComments(int TaskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment Task_CreateComment(RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskStatus> Task_RetrieveStatuses();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskType> Task_RetrieveTypes();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowField> Task_RetrieveWorkflowFields(int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowTransition> Task_RetrieveWorkflowTransitions(int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowCustomProperty> Task_RetrieveWorkflowCustomProperties(int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskFolder> Task_RetrieveFolders();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskFolder> Task_RetrieveFoldersByParent(int? parentTaskFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTaskFolder Task_CreateFolder(RemoteTaskFolder remoteTaskFolder);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTaskFolder Task_RetrieveFolderById(int taskFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Task_DeleteFolder(int taskFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void Task_UpdateFolder(RemoteTaskFolder remoteTaskFolder);

		#endregion

		#region Test Case

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTestCase TestCase_Create(RemoteTestCase remoteTestCase);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTestCaseFolder TestCase_CreateFolder(RemoteTestCaseFolder remoteTestCaseFolder);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestCase TestCase_RetrieveById(int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestCaseFolder TestCase_RetrieveFolderById(int testCaseFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void TestCase_Update(RemoteTestCase remoteTestCase);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_DeleteFolder(int testCaseFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void TestCase_UpdateFolder(RemoteTestCaseFolder remoteTestCaseFolder);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_Delete(int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestCaseParameter TestCase_AddParameter(RemoteTestCaseParameter remoteTestCaseParameter);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        string TestCase_CreateParameterToken(string parameterName);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseParameter> TestCase_RetrieveParameters(int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestStepParameter> TestCase_RetrieveStepParameters(int testCaseId, int testStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestStep> TestCase_RetrieveSteps(int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestStep TestCase_RetrieveStepById(int testStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_MoveStep(int testCaseId, int sourceTestStepId, int? destinationTestStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_DeleteStep(int testCaseId, int testStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestStep TestCase_AddStep(RemoteTestStep remoteTestStep, int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_UpdateStep(RemoteTestStep remoteTestStep);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        int TestCase_AddLink(int testCaseId, int position, int linkedTestCaseId, List<RemoteTestStepParameter> parameters);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_AddUpdateAutomationScript(int testCaseId, Nullable<int> automationEngineId, string urlOrFilename, string description, byte[] binaryData, string version, Nullable<int> projectAttachmentTypeId, Nullable<int> projectAttachmentFolderId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        long TestCase_Count(List<RemoteFilter> remoteFilters, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        long TestCase_CountForFolder(int? testCaseFolderId, List<RemoteFilter> remoteFilters, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCase> TestCase_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCase> TestCase_RetrieveByFolder(int? testCaseFolderId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestCase> TestCase_RetrieveByTestSetId(int testSetId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestCase> TestCase_RetrieveForOwner();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestCase_Move(int testCaseId, int? testCaseFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseFolder> TestCase_RetrieveFolders();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseFolder> TestCase_RetrieveFoldersByParent(int? parentTestCaseFolderId, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> TestCase_RetrieveComments(int TestCaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment TestCase_CreateComment(RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseStatus> TestCase_RetrieveStatuses();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseType> TestCase_RetrieveTypes();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowField> TestCase_RetrieveWorkflowFields(int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowTransition> TestCase_RetrieveWorkflowTransitions(int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowCustomProperty> TestCase_RetrieveWorkflowCustomProperties(int currentTypeId, int currentStatusId);

		#endregion

		#region Test Run

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteManualTestRun> TestRun_CreateFromTestCases(List<int> testCaseIds, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long TestRun_Count(List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteManualTestRun> TestRun_CreateFromTestSet(int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestRun_Delete(int testRunId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomatedTestRun> TestRun_CreateForAutomationHost(string automationHostToken, DataObjects.DateRange dateRange);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomatedTestRun> TestRun_CreateForAutomatedTestSet(int testSetId, string automationHostToken);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteAutomatedTestRun TestRun_RecordAutomated1(RemoteAutomatedTestRun remoteTestRun);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		int TestRun_RecordAutomated2(string userName, string password, int projectId, int? testerUserId, int testCaseId, Nullable<int> releaseId, Nullable<int> testSetId, Nullable<int> testSetTestCaseId, int? buildId, DateTime startDate, DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace, int testRunFormatId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		List<RemoteAutomatedTestRun> TestRun_RecordAutomated3(List<RemoteAutomatedTestRun> remoteTestRuns);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestRun> TestRun_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteManualTestRun> TestRun_RetrieveManual(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteTestRun TestRun_RetrieveById(int testRunId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestRun> TestRun_RetrieveByTestCaseId(int testCaseId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomatedTestRun TestRun_RetrieveAutomatedById(int testRunId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteManualTestRun TestRun_RetrieveManualById(int testRunId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		List<RemoteManualTestRun> TestRun_Save(List<RemoteManualTestRun> remoteTestRuns, DateTime? endDate);

		#endregion

		#region Test Set
        
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestSetTestCaseMapping> TestSet_AddTestMapping(RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping, Nullable<int> existingTestSetTestCaseId, List<RemoteTestSetTestCaseParameter> parameters);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestSet_RemoveTestMapping(RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetTestCaseMapping> TestSet_RetrieveTestCaseMapping(int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTestSet TestSet_Create(RemoteTestSet remoteTestSet);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTestSetFolder TestSet_CreateFolder(RemoteTestSetFolder remoteTestSetFolder);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        long TestSet_Count(List<RemoteFilter> remoteFilters, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        long TestSet_CountForFolder(int? testSetFolderId, List<RemoteFilter> remoteFilters, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSet> TestSet_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSet> TestSet_RetrieveByFolder(int? testSetFolderId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void TestSet_Update(RemoteTestSet remoteTestSet);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestSet_Delete(int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void TestSet_UpdateFolder(RemoteTestSetFolder remoteTestSetFolder);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestSet_DeleteFolder(int testSetFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestSet TestSet_RetrieveById(int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestSetFolder TestSet_RetrieveFolderById(int testSetFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetFolder> TestSet_RetrieveFolders();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetFolder> TestSet_RetrieveFoldersByParent(int? parentTestSetFolderId, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestSet> TestSet_RetrieveForOwner();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestSet_Move(int testSetId, int? destinationTestSetId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> TestSet_RetrieveComments(int TestSetId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment TestSet_CreateComment(RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetParameter> TestSet_RetrieveParameters(int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetTestCaseParameter> TestSet_RetrieveTestCaseParameters(int testSetId, int testSetTestCaseId);

		#endregion

        #region Test Configurations

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestConfigurationSet TestConfiguration_RetrieveForTestSet(int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestConfigurationSet TestConfiguration_RetrieveSetById(int testConfigurationSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestConfigurationSet> TestConfiguration_RetrieveSets();

        #endregion

        #region User
        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteUser User_Create(RemoteUser remoteUser, string password, string passwordQuestion, string passwordAnswer, int? projectRoleId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteUser User_RetrieveById(int userId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        RemoteUser User_RetrieveByUserName(string userName, bool includeInactive);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void User_Delete(int userId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void User_Update(RemoteUser remoteUser);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteUser> User_Retrieve();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteUser> User_RetrieveContacts();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void User_AddContact(int userId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void User_RemoveContact(int userId);

		#endregion

		#region Automation Host
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomationHost> AutomationHost_Retrieve(List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationHost AutomationHost_RetrieveById(int automationHostId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationHost AutomationHost_RetrieveByToken(string token);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteAutomationHost AutomationHost_Create(RemoteAutomationHost remoteAutomationHost);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void AutomationHost_Update(RemoteAutomationHost remoteAutomationHost);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void AutomationHost_Delete(int automationHostId);

		#endregion

		#region Automation Engine
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationEngine AutomationEngine_RetrieveByToken(string token);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomationEngine> AutomationEngine_Retrieve(bool activeOnly);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationEngine AutomationEngine_Create(RemoteAutomationEngine remoteEngine);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationEngine AutomationEngine_RetrieveById(int automationEngineId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void AutomationEngine_Update(RemoteAutomationEngine remoteEngine);

		#endregion

        #region Component

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteComponent> Component_Retrieve(bool activeOnly, bool includeDeleted);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteComponent Component_RetrieveById(int componentId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteComponent Component_Create(RemoteComponent remoteComponent);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Component_Update(RemoteComponent remoteComponent);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Component_Delete(int componentId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Component_Undelete(int componentId);

        #endregion

        #region Build
        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteBuild> Build_RetrieveByReleaseId(int releaseId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteBuild Build_RetrieveById(int releaseId, int buildId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteBuild Build_Create(RemoteBuild remoteBuild);
		#endregion

        #region Source Code

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeBranch> SourceCode_RetrieveBranches();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeFolder> SourceCode_RetrieveFoldersByParent(string branchId, string parentFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesByFolder(string branchId, string folderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesByRevision(string branchId, string revisionId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteSourceCodeFile SourceCode_RetrieveFileById(string branchId, string fileId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesForArtifact(string branchId, int artifactTypeId, int artifactId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        byte[] SourceCode_OpenFileById(string branchId, string fileId, string revisionId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisions(string branchId, int startRow, int numberRows, RemoteSort remoteSort, List<RemoteFilter> remoteFilters);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisionsForFile(string branchId, string fileId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteSourceCodeRevision SourceCode_RetrieveRevisionById(string branchId, string revisionId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisionsForArtifact(string branchId, int artifactTypeId, int artifactId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteLinkedArtifact> SourceCode_RetrieveArtifactsForRevision(string branchId, string revisionId);

        #endregion

        #region Instant Messenger

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteMessageInfo Message_GetInfo();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        long Message_PostNew(int recipientUserId, string message);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Message_MarkAllAsRead(int senderUserId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteUserMessage> Message_GetUnreadMessageSenders();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteMessage> Message_RetrieveUnread();

        #endregion

        #region History

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteHistoryChange> History_RetrieveForArtifact(int artifactTypeId, int artifactId, int startingRow, int numberOfRows, RemoteSort remoteSort, List<RemoteFilter> remoteFilters);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteHistoryChangeSet History_RetrieveById(int historyChangeSetId);

        #endregion

        #region Subscriptions

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Subscription_SubscribeToArtifact(int artifactTypeId, int artifactId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Subscription_UnsubscribeFromArtifact(int artifactTypeId, int artifactId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteArtifactSubscription> Subscription_RetrieveForUser();

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteArtifactSubscription> Subscription_RetrieveForArtifact(int artifactTypeId, int artifactId);

        #endregion

        #region Saved Filters

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSavedFilter> SavedFilter_RetrieveForUser();

        #endregion
    }
}
