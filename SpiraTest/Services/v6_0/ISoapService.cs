using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Web.Services.v6_0.DataObjects;
using Inflectra.SpiraTest.Web.Services.Wsdl.Documentation;

#pragma warning disable 1591

namespace Inflectra.SpiraTest.Web.Services.v6_0
{
	[ServiceContract(Namespace = "http://www.inflectra.com/SpiraTest/Services/v6.0/", SessionMode = SessionMode.Allowed)]
	[XmlComments(XmlCommentFormat.Default, "Inflectra.SpiraTest.Web.Services.v6_0.SoapService")]
    public interface ISoapService
    {
        #region Authentication

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        RemoteCredentials Connection_Authenticate1(string userName, string password, string plugInName);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        RemoteCredentials Connection_Authenticate2(string userName, string apiKey, string plugInName);

		#endregion

		#region System

		[
		OperationContract,
		FaultContract(typeof(ServiceFaultMessage))
		]
		void System_ProcessNotifications();

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        string System_GetProductName();

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        DateTime System_GetServerDateTime();

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        string System_GetWebServerUrl();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteVersion System_GetProductVersion();

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteSetting> System_GetSettings(RemoteCredentials credentials);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		string System_GetArtifactUrl(int navigationLinkId, int projectId, int artifactId, string tabName);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		int System_GetProjectIdForArtifact(RemoteCredentials credentials, int artifactTypeId, int artifactId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteEvent2> System_RetrieveEvents(RemoteCredentials credentials, int startingRow, int numberOfRows, RemoteSort remoteSort, List<RemoteFilter> remoteFilters);

		#endregion

		#region Data Sync System

		[
		OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        List<RemoteDataSyncSystem> DataSyncSystem_Retrieve(RemoteCredentials credentials);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        RemoteDataSyncSystem DataSyncSystem_RetrieveById(RemoteCredentials credentials, int dataSyncSystemId);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        RemoteDataSyncSystem DataSyncSystem_Create(RemoteCredentials credentials, RemoteDataSyncSystem remoteDataSyncSystem);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_Update(RemoteCredentials credentials, RemoteDataSyncSystem remoteDataSyncSystem);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunFailure(RemoteCredentials credentials, int dataSyncSystemId);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunSuccess(RemoteCredentials credentials, int dataSyncSystemId, DateTime lastRunDate);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_SaveRunWarning(RemoteCredentials credentials, int dataSyncSystemId, DateTime lastRunDate);

        [
        OperationContract,
        FaultContract(typeof(ServiceFaultMessage))
        ]
        void DataSyncSystem_WriteEvent(RemoteCredentials credentials, string message, string details, int eventLogEntryType);

        #endregion

        #region Custom Property

        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteCustomProperty> CustomProperty_RetrieveForArtifactType(RemoteCredentials credentials, int projectTemplateId, int artifactTypeId, bool includeDeleted);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteCustomList CustomProperty_AddCustomList(RemoteCredentials credentials, RemoteCustomList remoteCustomList);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteCustomListValue CustomProperty_AddCustomListValue(RemoteCredentials credentials, int projectTemplateId, RemoteCustomListValue remoteCustomListValue);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteCustomProperty CustomProperty_AddDefinition(RemoteCredentials credentials, RemoteCustomProperty remoteCustomProperty, int? customListId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void CustomProperty_UpdateDefinition(RemoteCredentials credentials, RemoteCustomProperty remoteCustomProperty);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void CustomProperty_DeleteDefinition(RemoteCredentials credentials, int projectTemplateId, int customPropertyId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteCustomList CustomProperty_RetrieveCustomListById(RemoteCredentials credentials, int projectTemplateId, int customListId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteCustomList> CustomProperty_RetrieveCustomLists(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void CustomProperty_UpdateCustomList(RemoteCredentials credentials, RemoteCustomList remoteCustomList);

		#endregion

		#region Data Mapping
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void DataMapping_AddArtifactMappings(RemoteCredentials credentials, int projectId, int dataSyncSystemId, int artifactTypeId, List<RemoteDataMapping> remoteDataMappings);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void DataMapping_AddUserMappings(RemoteCredentials credentials, int dataSyncSystemId, List<RemoteDataMapping> remoteDataMappings);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void DataMapping_RemoveArtifactMappings(RemoteCredentials credentials, int projectId, int dataSyncSystemId, int artifactTypeId, List<RemoteDataMapping> remoteDataMappings);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveArtifactMappings(RemoteCredentials credentials, int projectId, int dataSyncSystemId, int artifactTypeId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDataMapping DataMapping_RetrieveCustomPropertyMapping(RemoteCredentials credentials, int projectId, int dataSyncSystemId, int artifactTypeId, int customPropertyId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveCustomPropertyValueMappings(RemoteCredentials credentials, int projectId, int dataSyncSystemId, int artifactTypeId, int customPropertyId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveFieldValueMappings(RemoteCredentials credentials, int projectId, int dataSyncSystemId, int artifactFieldId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveProjectMappings(RemoteCredentials credentials, int dataSyncSystemId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDataMapping> DataMapping_RetrieveUserMappings(RemoteCredentials credentials, int dataSyncSystemId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteProjectArtifact> DataMapping_SearchArtifactMappings(RemoteCredentials credentials, int dataSyncSystemId, int artifactTypeId, string externalKey);

		#endregion

		#region Document
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		byte[] Document_OpenFile(RemoteCredentials credentials, int projectId, int attachmentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocument Document_AddFile(RemoteCredentials credentials, RemoteDocument remoteDocument, byte[] binaryData);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocument Document_AddUrl(RemoteCredentials credentials, RemoteDocument remoteDocument);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentVersion Document_AddFileVersion(RemoteCredentials credentials, int projectId, RemoteDocumentVersion remoteDocumentVersion, byte[] binaryData, bool makeCurrent);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentVersion Document_AddUrlVersion(RemoteCredentials credentials, int projectId, RemoteDocumentVersion remoteDocumentVersion, bool makeCurrent);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		byte[] Document_OpenVersion(RemoteCredentials credentials, int projectId, int attachmentVersionId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_DeleteVersion(RemoteCredentials credentials, int projectId, int attachmentVersionId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_DeleteFromArtifact(RemoteCredentials credentials, int projectId, int attachmentId, int artifactTypeId, int artifactId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_Delete(RemoteCredentials credentials, int projectId, int attachmentId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteDocument> Document_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDocument> Document_RetrieveForFolder(RemoteCredentials credentials, int projectId, int folderId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDocument> Document_RetrieveForArtifact(RemoteCredentials credentials, int projectId, int artifactTypeId, int artifactId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_AddToArtifactId(RemoteCredentials credentials, int projectId, int artifactTypeId, int artifactId, int attachmentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocument Document_RetrieveById(RemoteCredentials credentials, int projectId, int attachmentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDocumentType> Document_RetrieveTypes(RemoteCredentials credentials, int projectTemplateId, bool activeOnly);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentType Document_RetrieveDefaultType(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentType Document_AddType(RemoteCredentials credentials, int projectTemplateId, RemoteDocumentType remoteDocumentType);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_UpdateType(RemoteCredentials credentials, int projectTemplateId, RemoteDocumentType remoteDocumentType);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteDocumentFolder> Document_RetrieveFolders(RemoteCredentials credentials, int projectId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentFolder Document_RetrieveFolderById(RemoteCredentials credentials, int projectId, int folderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteDocumentFolder> Document_RetrieveFoldersByParentFolderId(RemoteCredentials credentials, int projectId, int? parentFolderId);

        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteDocumentFolder Document_AddFolder(RemoteCredentials credentials, RemoteDocumentFolder remoteDocumentFolder);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_DeleteFolder(RemoteCredentials credentials, int projectId, int projectAttachmentFolderId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Document_UpdateFolder(RemoteCredentials credentials, RemoteDocumentFolder remoteDocumentFolder);
		#endregion

		#region Association

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAssociation Association_Create(RemoteCredentials credentials, int projectId, RemoteAssociation remoteAssociation);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Association_Update(RemoteCredentials credentials, int projectId, RemoteAssociation remoteAssociation);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAssociation> Association_RetrieveForArtifact(RemoteCredentials credentials, int projectId, int artifactTypeId, int artifactId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Association_Delete(RemoteCredentials credentials, int projectId, int artifactLinkId);

		#endregion

		#region Incident
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncidentPriority Incident_AddPriority(RemoteCredentials credentials, int projectTemplateId, RemoteIncidentPriority remoteIncidentPriority);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Incident_Count(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Incident_AddComments(RemoteCredentials credentials, int projectId, List<RemoteComment> remoteComments);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncidentSeverity Incident_AddSeverity(RemoteCredentials credentials, int projectTemplateId, RemoteIncidentSeverity remoteIncidentSeverity);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncidentStatus Incident_AddStatus(RemoteCredentials credentials, int projectTemplateId, RemoteIncidentStatus remoteIncidentStatus);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncidentType Incident_AddType(RemoteCredentials credentials, int projectTemplateId, RemoteIncidentType remoteIncidentType);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteIncident Incident_Create(RemoteCredentials credentials, RemoteIncident remoteIncident);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteIncident Incident_RetrieveById(RemoteCredentials credentials, int projectId, int incidentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveByTestCase(RemoteCredentials credentials, int projectId, int testCaseId, bool openOnly);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveByTestRunStep(RemoteCredentials credentials, int projectId, int testRunStepId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveByTestStep(RemoteCredentials credentials, int projectId, int testStepId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveForOwner(RemoteCredentials credentials);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncident> Incident_RetrieveNew(RemoteCredentials credentials, int projectId, DateTime creationDate, int startRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncidentPriority> Incident_RetrievePriorities(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Incident_RetrieveComments(RemoteCredentials credentials, int projectId, int incidentId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncidentSeverity> Incident_RetrieveSeverities(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncidentStatus> Incident_RetrieveStatuses(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteIncidentType> Incident_RetrieveTypes(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowField> Incident_RetrieveWorkflowFields(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowTransition> Incident_RetrieveWorkflowTransitions(RemoteCredentials credentials, int projectId, int currentTypeId, int currentStatusId, bool isDetector, bool isOwner);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowCustomProperty> Incident_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Incident_Update(RemoteCredentials credentials, RemoteIncident remoteIncident);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Incident_Delete(RemoteCredentials credentials, int projectId, int incidentId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteIncidentStatus Incident_RetrieveDefaultStatus(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteIncidentType Incident_RetrieveDefaultType(RemoteCredentials credentials, int projectTemplateId);

        #endregion

        #region Project Template

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteProjectTemplate ProjectTemplate_Create(RemoteCredentials credentials, RemoteProjectTemplate remoteProjectTemplate, int? existingProjectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void ProjectTemplate_Delete(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void ProjectTemplate_Update(RemoteCredentials credentials, RemoteProjectTemplate remoteProjectTemplate);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteProjectTemplate> ProjectTemplate_Retrieve(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteProjectTemplate ProjectTemplate_RetrieveById(RemoteCredentials credentials, int projectTemplateId);

        #endregion

        #region Project
        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteProject Project_Create(RemoteCredentials credentials, RemoteProject remoteProject, int? existingProjectId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Project_Delete(RemoteCredentials credentials, int projectId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void Project_Update(RemoteCredentials credentials, RemoteProject remoteProject);

        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteProject> Project_Retrieve(RemoteCredentials credentials);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteProject Project_RetrieveById(RemoteCredentials credentials, int projectId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteProjectUser> Project_RetrieveUserMembership(RemoteCredentials credentials, int projectId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Project_RefreshProgressExecutionStatusCaches(RemoteCredentials credentials, int projectId, int? releaseId, bool runInBackground);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Project_AddUserMembership(RemoteCredentials credentials, RemoteProjectUser remoteProjectUser);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Project_UpdateUserMembership(RemoteCredentials credentials, RemoteProjectUser remoteProjectUser);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Project_RemoveUserMembership(RemoteCredentials credentials, RemoteProjectUser remoteProjectUser);

        #endregion

        #region ProjectRole

        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteProjectRole> ProjectRole_Retrieve(RemoteCredentials credentials);
		
        #endregion

		#region Release
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_AddTestMapping(RemoteCredentials credentials, int projectId, RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_AddTestMapping2(RemoteCredentials credentials, int projectId, RemoteReleaseTestCaseMapping[] remoteReleaseTestCaseMappings);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Release_Count(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteRelease Release_Create(RemoteCredentials credentials, RemoteRelease remoteRelease, int? parentReleaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_RemoveTestMapping(RemoteCredentials credentials, int projectId, RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRelease> Release_Retrieve(RemoteCredentials credentials, int projectId, bool activeOnly);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRelease> Release_Retrieve2(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteRelease Release_RetrieveById(RemoteCredentials credentials, int projectId, int releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Release_Update(RemoteCredentials credentials, RemoteRelease remoteRelease);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_Delete(RemoteCredentials credentials, int projectId, int releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Release_Move(RemoteCredentials credentials, int projectId, int releaseId, int? destinationReleaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Release_Indent(RemoteCredentials credentials, int projectId, int releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Release_Outdent(RemoteCredentials credentials, int projectId, int releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Release_RetrieveComments(RemoteCredentials credentials, int projectId, int ReleaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteReleaseTestCaseMapping> Release_RetrieveTestMapping(RemoteCredentials credentials, int projectId, int releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment Release_CreateComment(RemoteCredentials credentials, int projectId, RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteReleaseStatus> Release_RetrieveStatuses(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteReleaseType> Release_RetrieveTypes(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowField> Release_RetrieveWorkflowFields(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowTransition> Release_RetrieveWorkflowTransitions(RemoteCredentials credentials, int projectId, int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowCustomProperty> Release_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

		#endregion

		#region Requirement
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Requirement_AddTestCoverage(RemoteCredentials credentials, int projectId, RemoteRequirementTestCaseMapping remoteReqTestCaseMapping);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Requirement_Count(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteRequirement Requirement_Create1(RemoteCredentials credentials, RemoteRequirement remoteRequirement, int indentPosition);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteRequirement Requirement_Create2(RemoteCredentials credentials, RemoteRequirement remoteRequirement, int? parentRequirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Requirement_RemoveTestCoverage(RemoteCredentials credentials, int projectId, RemoteRequirementTestCaseMapping remoteReqTestCaseMapping);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRequirement> Requirement_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteRequirement Requirement_RetrieveById(RemoteCredentials credentials, int projectId, int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRequirement> Requirement_RetrieveForOwner(RemoteCredentials credentials);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRequirementTestCaseMapping> Requirement_RetrieveTestCoverage(RemoteCredentials credentials, int projectId, int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Requirement_Update(RemoteCredentials credentials, RemoteRequirement remoteRequirement);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Requirement_Delete(RemoteCredentials credentials, int projectId, int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Requirement_Move(RemoteCredentials credentials, int projectId, int requirementId, int? destinationRequirementId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_Indent(RemoteCredentials credentials, int projectId, int requirementId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_Outdent(RemoteCredentials credentials, int projectId, int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Requirement_RetrieveComments(RemoteCredentials credentials, int projectId, int requirementId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment Requirement_CreateComment(RemoteCredentials credentials, int projectId, RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementStatus> Requirement_RetrieveStatuses(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementType> Requirement_RetrieveTypes(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementImportance> Requirement_RetrieveImportances(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowField> Requirement_RetrieveWorkflowFields(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowTransition> Requirement_RetrieveWorkflowTransitions(RemoteCredentials credentials, int projectId, int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowCustomProperty> Requirement_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementStep> Requirement_RetrieveSteps(RemoteCredentials credentials, int projectId, int requirementId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteRequirementStep Requirement_RetrieveStepById(RemoteCredentials credentials, int projectId, int requirementStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteRequirementStep Requirement_AddStep(RemoteCredentials credentials, int projectId, RemoteRequirementStep remoteRequirementStep, int? existingRequirementStepId, int? creatorId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_UpdateStep(RemoteCredentials credentials, int projectId, RemoteRequirementStep remoteRequirementStep);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_MoveStep(RemoteCredentials credentials, int projectId, int requirementId, int sourceRequirementStepId, int? destinationRequirementStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_DeleteStep(RemoteCredentials credentials, int projectId, int requirementId, int requirementStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_AddTestStepCoverage(RemoteCredentials credentials, int projectId, RemoteRequirementTestStepMapping remoteReqTestStepMapping);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementTestStepMapping> Requirement_RetrieveTestStepCoverage(RemoteCredentials credentials, int projectId, int requirementId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Requirement_RemoveTestStepCoverage(RemoteCredentials credentials, int projectId, RemoteRequirementTestStepMapping remoteReqTestStepMapping);

		#endregion

		#region Risk

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteRisk Risk_Create(RemoteCredentials credentials, RemoteRisk remoteRisk);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRisk> Risk_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteRisk Risk_RetrieveById(RemoteCredentials credentials, int projectId, int riskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRisk> Risk_RetrieveForOwner(RemoteCredentials credentials);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Risk_Update(RemoteCredentials credentials, RemoteRisk remoteRisk);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Risk_Count(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Risk_Delete(RemoteCredentials credentials, int projectId, int riskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Risk_RetrieveComments(RemoteCredentials credentials, int projectId, int RiskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment Risk_CreateComment(RemoteCredentials credentials, int projectId, RemoteComment remoteComment);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRiskStatus> Risk_RetrieveStatuses(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRiskType> Risk_RetrieveTypes(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRiskImpact> Risk_RetrieveImpacts(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRiskProbability> Risk_RetrieveProbabilities(RemoteCredentials credentials, int projectTemplateId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowField> Risk_RetrieveWorkflowFields(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowTransition> Risk_RetrieveWorkflowTransitions(RemoteCredentials credentials, int projectId, int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteWorkflowCustomProperty> Risk_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteRiskMitigation> Risk_RetrieveMitigations(RemoteCredentials credentials, int projectId, int riskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteRiskMitigation Risk_RetrieveMitigationById(RemoteCredentials credentials, int projectId, int riskMitigationId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteRiskMitigation Risk_AddMitigation(RemoteCredentials credentials, int projectId, RemoteRiskMitigation remoteRiskMitigation, int? existingRiskMitigationId, int? creatorId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Risk_UpdateMitigation(RemoteCredentials credentials, int projectId, RemoteRiskMitigation remoteRiskMitigation);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Risk_DeleteMitigation(RemoteCredentials credentials, int projectId, int riskId, int riskMitigationId);

		#endregion

		#region Test Step

		[OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteRequirementTestStepMapping> TestStep_RetrieveRequirementCoverage(RemoteCredentials credentials, int projectId, int testStepId);

        #endregion

		#region Task
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteTask Task_Create(RemoteCredentials credentials, RemoteTask remoteTask);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTask> Task_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteTask Task_RetrieveById(RemoteCredentials credentials, int projectId, int taskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTask> Task_RetrieveForOwner(RemoteCredentials credentials);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTask> Task_RetrieveNew(RemoteCredentials credentials, int projectId, DateTime creationDate, int startRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void Task_Update(RemoteCredentials credentials, RemoteTask remoteTask);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long Task_Count(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void Task_Delete(RemoteCredentials credentials, int projectId, int taskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> Task_RetrieveComments(RemoteCredentials credentials, int projectId, int TaskId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment Task_CreateComment(RemoteCredentials credentials, int projectId, RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskStatus> Task_RetrieveStatuses(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskType> Task_RetrieveTypes(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskPriority> Task_RetrievePriorities(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowField> Task_RetrieveWorkflowFields(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowTransition> Task_RetrieveWorkflowTransitions(RemoteCredentials credentials, int projectId, int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowCustomProperty> Task_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskFolder> Task_RetrieveFolders(RemoteCredentials credentials, int projectId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTaskFolder> Task_RetrieveFoldersByParent(RemoteCredentials credentials, int projectId, int? parentTaskFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTaskFolder Task_CreateFolder(RemoteCredentials credentials, RemoteTaskFolder remoteTaskFolder);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTaskFolder Task_RetrieveFolderById(RemoteCredentials credentials, int projectId, int taskFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Task_DeleteFolder(RemoteCredentials credentials, int projectId, int taskFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void Task_UpdateFolder(RemoteCredentials credentials, RemoteTaskFolder remoteTaskFolder);

		#endregion

		#region Test Case

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTestCase TestCase_Create(RemoteCredentials credentials, RemoteTestCase remoteTestCase);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTestCaseFolder TestCase_CreateFolder(RemoteCredentials credentials, RemoteTestCaseFolder remoteTestCaseFolder);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestCase TestCase_RetrieveById(RemoteCredentials credentials, int projectId, int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestCaseFolder TestCase_RetrieveFolderById(RemoteCredentials credentials, int projectId, int testCaseFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void TestCase_Update(RemoteCredentials credentials, RemoteTestCase remoteTestCase);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_DeleteFolder(RemoteCredentials credentials, int projectId, int testCaseFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void TestCase_UpdateFolder(RemoteCredentials credentials, RemoteTestCaseFolder remoteTestCaseFolder);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_Delete(RemoteCredentials credentials, int projectId, int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestCaseParameter TestCase_AddParameter(RemoteCredentials credentials, int projectId, RemoteTestCaseParameter remoteTestCaseParameter);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestCase_UpdateParameter(RemoteCredentials credentials, int projectId, RemoteTestCaseParameter remoteTestCaseParameter);
		
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestCase_DeleteParameter(RemoteCredentials credentials, int projectId, RemoteTestCaseParameter remoteTestCaseParameter);

		[OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        string TestCase_CreateParameterToken(string parameterName);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseParameter> TestCase_RetrieveParameters(RemoteCredentials credentials, int projectId, int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestStepParameter> TestCase_RetrieveStepParameters(RemoteCredentials credentials, int projectId, int testCaseId, int testStepId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestCase_AddStepParameters(RemoteCredentials credentials, int projectId, int testCaseId, int testStepId, List<RemoteTestStepParameter> testStepParameters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestCase_UpdateStepParameters(RemoteCredentials credentials, int projectId, int testCaseId, int testStepId, List<RemoteTestStepParameter> testStepParameters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestCase_DeleteStepParameters(RemoteCredentials credentials, int projectId, int testCaseId, int testStepId, List<RemoteTestStepParameter> testStepParameters);

		[OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestStep> TestCase_RetrieveSteps(RemoteCredentials credentials, int projectId, int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestStep TestCase_RetrieveStepById(RemoteCredentials credentials, int projectId, int testStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_MoveStep(RemoteCredentials credentials, int projectId, int testCaseId, int sourceTestStepId, int? destinationTestStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_DeleteStep(RemoteCredentials credentials, int projectId, int testCaseId, int testStepId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestStep TestCase_AddStep(RemoteCredentials credentials, RemoteTestStep remoteTestStep, int testCaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_UpdateStep(RemoteCredentials credentials, RemoteTestStep remoteTestStep);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        int TestCase_AddLink(RemoteCredentials credentials, int projectId, int testCaseId, int position, int linkedTestCaseId, List<RemoteTestStepParameter> parameters);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestCase_AddUpdateAutomationScript(RemoteCredentials credentials, int projectId, int testCaseId, Nullable<int> automationEngineId, string urlOrFilename, string description, byte[] binaryData, string version, Nullable<int> documentTypeId, Nullable<int> projectAttachmentFolderId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        long TestCase_Count(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        long TestCase_CountForFolder(RemoteCredentials credentials, int projectId, int? testCaseFolderId, List<RemoteFilter> remoteFilters, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCase> TestCase_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCase> TestCase_RetrieveByFolder(RemoteCredentials credentials, int projectId, int? testCaseFolderId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestCase> TestCase_RetrieveByTestSetId(RemoteCredentials credentials, int projectId, int testSetId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestCase> TestCase_RetrieveForOwner(RemoteCredentials credentials);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestCase_Move(RemoteCredentials credentials, int projectId, int testCaseId, int? testCaseFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseFolder> TestCase_RetrieveFolders(RemoteCredentials credentials, int projectId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseFolder> TestCase_RetrieveFoldersByParent(RemoteCredentials credentials, int projectId, int? parentTestCaseFolderId, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> TestCase_RetrieveComments(RemoteCredentials credentials, int projectId, int TestCaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment TestCase_CreateComment(RemoteCredentials credentials, int projectId, RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseStatus> TestCase_RetrieveStatuses(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCaseType> TestCase_RetrieveTypes(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestCasePriority> TestCase_RetrievePriorities(RemoteCredentials credentials, int projectTemplateId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowField> TestCase_RetrieveWorkflowFields(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowTransition> TestCase_RetrieveWorkflowTransitions(RemoteCredentials credentials, int projectId, int currentTypeId, int currentStatusId, bool isCreator, bool isOwner);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteWorkflowCustomProperty> TestCase_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, int projectTemplateId, int currentTypeId, int currentStatusId);

		#endregion

		#region Test Run

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteManualTestRun> TestRun_CreateFromTestCases(RemoteCredentials credentials, int projectId, List<int> testCaseIds, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		long TestRun_Count(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteManualTestRun> TestRun_CreateFromTestSet(RemoteCredentials credentials, int projectId, int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestRun_Delete(RemoteCredentials credentials, int projectId, int testRunId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomatedTestRun> TestRun_CreateForAutomationHost(RemoteCredentials credentials, int projectId, string automationHostToken, DataObjects.DateRange dateRange);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomatedTestRun> TestRun_CreateForAutomatedTestSet(RemoteCredentials credentials, int projectId, int testSetId, string automationHostToken);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteAutomatedTestRun TestRun_RecordAutomated1(RemoteCredentials credentials, RemoteAutomatedTestRun remoteTestRun);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		int TestRun_RecordAutomated2(string userName, string password, int projectId, int? testerUserId, int testCaseId, Nullable<int> releaseId, Nullable<int> testSetId, Nullable<int> testSetTestCaseId, int? buildId, DateTime startDate, DateTime endDate, int executionStatusId, string runnerName, string runnerTestName, int runnerAssertCount, string runnerMessage, string runnerStackTrace, int testRunFormatId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		List<RemoteAutomatedTestRun> TestRun_RecordAutomated3(RemoteCredentials credentials, int projectId, List<RemoteAutomatedTestRun> remoteTestRuns);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestRun> TestRun_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteManualTestRun> TestRun_RetrieveManual(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteTestRun TestRun_RetrieveById(RemoteCredentials credentials, int projectId, int testRunId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestRun> TestRun_RetrieveByTestCaseId(RemoteCredentials credentials, int projectId, int testCaseId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomatedTestRun TestRun_RetrieveAutomatedById(RemoteCredentials credentials, int projectId, int testRunId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteManualTestRun TestRun_RetrieveManualById(RemoteCredentials credentials, int projectId, int testRunId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		List<RemoteManualTestRun> TestRun_Save(RemoteCredentials credentials, int projectId, List<RemoteManualTestRun> remoteTestRuns, DateTime? endDate);

		#endregion

		#region Test Set
        
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestSetTestCaseMapping> TestSet_AddTestMapping(RemoteCredentials credentials, int projectId, RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping, Nullable<int> existingTestSetTestCaseId, List<RemoteTestSetTestCaseParameter> parameters);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestSet_RemoveTestMapping(RemoteCredentials credentials, int projectId, RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetTestCaseMapping> TestSet_RetrieveTestCaseMapping(RemoteCredentials credentials, int projectId, int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTestSet TestSet_Create(RemoteCredentials credentials, RemoteTestSet remoteTestSet);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        RemoteTestSetFolder TestSet_CreateFolder(RemoteCredentials credentials, RemoteTestSetFolder remoteTestSetFolder);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        long TestSet_Count(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        long TestSet_CountForFolder(RemoteCredentials credentials, int projectId, int? testSetFolderId, List<RemoteFilter> remoteFilters, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSet> TestSet_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSet> TestSet_RetrieveByFolder(RemoteCredentials credentials, int projectId, int? testSetFolderId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows, int? releaseId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void TestSet_Update(RemoteCredentials credentials, RemoteTestSet remoteTestSet);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestSet_Delete(RemoteCredentials credentials, int projectId, int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        [FaultContract(typeof(ValidationFaultMessage))]
        void TestSet_UpdateFolder(RemoteCredentials credentials, RemoteTestSetFolder remoteTestSetFolder);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestSet_DeleteFolder(RemoteCredentials credentials, int projectId, int testSetFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestSet TestSet_RetrieveById(RemoteCredentials credentials, int projectId, int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestSetFolder TestSet_RetrieveFolderById(RemoteCredentials credentials, int projectId, int testSetFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetFolder> TestSet_RetrieveFolders(RemoteCredentials credentials, int projectId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetFolder> TestSet_RetrieveFoldersByParent(RemoteCredentials credentials, int projectId, int? parentTestSetFolderId, int? releaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteTestSet> TestSet_RetrieveForOwner(RemoteCredentials credentials);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestSet_Move(RemoteCredentials credentials, int projectId, int testSetId, int? destinationTestSetId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteComment> TestSet_RetrieveComments(RemoteCredentials credentials, int projectId, int TestSetId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteComment TestSet_CreateComment(RemoteCredentials credentials, int projectId, RemoteComment remoteComment);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetParameter> TestSet_RetrieveParameters(RemoteCredentials credentials, int projectId, int testSetId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestSet_AddParameter(RemoteCredentials credentials, int projectId, RemoteTestSetParameter remoteTestSetParameter);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestSet_UpdateParameter(RemoteCredentials credentials, int projectId, RemoteTestSetParameter remoteTestSetParameter);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestSet_DeleteParameter(RemoteCredentials credentials, int projectId, RemoteTestSetParameter remoteTestSetParameter);

		[OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestSetTestCaseParameter> TestSet_RetrieveTestCaseParameters(RemoteCredentials credentials, int projectId, int testSetId, int testSetTestCaseId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestSet_AddTestCaseParameter(RemoteCredentials credentials, int projectId, int testSetId, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestSet_UpdateTestCaseParameter(RemoteCredentials credentials, int projectId, int testSetId, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void TestSet_DeleteTestCaseParameter(RemoteCredentials credentials, int projectId, int testSetId, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter);

		[OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void TestSet_SetInUseStatus(RemoteCredentials credentials, int projectId, int testSetId, int testSetTestCaseId, bool isInUse);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        bool TestSet_CheckInUseStatus(RemoteCredentials credentials, int projectId, int testSetId, int testSetTestCaseId);

        #endregion

        #region Test Configurations

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestConfigurationSet TestConfiguration_RetrieveForTestSet(RemoteCredentials credentials, int projectId, int testSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTestConfigurationSet TestConfiguration_RetrieveSetById(RemoteCredentials credentials, int projectId, int testConfigurationSetId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteTestConfigurationSet> TestConfiguration_RetrieveSets(RemoteCredentials credentials, int projectId);

        #endregion

        #region User
        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteUser User_Create(RemoteCredentials credentials, RemoteUser remoteUser, string password, string passwordQuestion, string passwordAnswer, int? projectId, int? projectRoleId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteUser User_RetrieveById(RemoteCredentials credentials, int userId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
        RemoteUser User_RetrieveByUserName(RemoteCredentials credentials, string userName, bool includeInactive);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void User_Delete(RemoteCredentials credentials, int userId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void User_Update(RemoteCredentials credentials, RemoteUser remoteUser);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteUser> User_Retrieve(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteUser> User_RetrieveContacts(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void User_AddContact(RemoteCredentials credentials, int userId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void User_RemoveContact(RemoteCredentials credentials, int userId);

		#endregion

		#region Automation Host
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomationHost> AutomationHost_Retrieve(RemoteCredentials credentials, int projectId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationHost AutomationHost_RetrieveById(RemoteCredentials credentials, int projectId, int automationHostId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationHost AutomationHost_RetrieveByToken(RemoteCredentials credentials, int projectId, string token);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		RemoteAutomationHost AutomationHost_Create(RemoteCredentials credentials, RemoteAutomationHost remoteAutomationHost);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		[FaultContract(typeof(ValidationFaultMessage))]
		void AutomationHost_Update(RemoteCredentials credentials, RemoteAutomationHost remoteAutomationHost);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		void AutomationHost_Delete(RemoteCredentials credentials, int projectId, int automationHostId);

		#endregion

		#region Automation Engine
		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationEngine AutomationEngine_RetrieveByToken(RemoteCredentials credentials, string token);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteAutomationEngine> AutomationEngine_Retrieve(RemoteCredentials credentials, bool activeOnly);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationEngine AutomationEngine_Create(RemoteCredentials credentials, RemoteAutomationEngine remoteEngine);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteAutomationEngine AutomationEngine_RetrieveById(RemoteCredentials credentials, int automationEngineId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void AutomationEngine_Update(RemoteCredentials credentials, RemoteAutomationEngine remoteEngine);

		#endregion

        #region Component

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteComponent> Component_Retrieve(RemoteCredentials credentials, int projectId, bool activeOnly, bool includeDeleted);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteComponent Component_RetrieveById(RemoteCredentials credentials, int projectId, int componentId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteComponent Component_Create(RemoteCredentials credentials, RemoteComponent remoteComponent);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Component_Update(RemoteCredentials credentials, RemoteComponent remoteComponent);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Component_Delete(RemoteCredentials credentials, int projectId, int componentId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Component_Undelete(RemoteCredentials credentials, int projectId, int componentId);

        #endregion

        #region Build
        [OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteBuild> Build_RetrieveByReleaseId(RemoteCredentials credentials, int projectId, int releaseId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		List<RemoteBuild> Build_RetrieveByReleaseId_NoDescription(RemoteCredentials credentials, int projectId, int releaseId, List<RemoteFilter> remoteFilters, RemoteSort remoteSort, int startingRow, int numberOfRows);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteBuild Build_RetrieveById(RemoteCredentials credentials, int projectId, int releaseId, int buildId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteBuild Build_RetrieveById_NoDescription(RemoteCredentials credentials, int projectId, int releaseId, int buildId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteBuild Build_Create(RemoteCredentials credentials, RemoteBuild remoteBuild);
		#endregion

        #region Source Code

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeBranch> SourceCode_RetrieveBranches(RemoteCredentials credentials, int projectId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeFolder> SourceCode_RetrieveFoldersByParent(RemoteCredentials credentials, int projectId, string branchId, string parentFolderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesByFolder(RemoteCredentials credentials, int projectId, string branchId, string folderId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesByRevision(RemoteCredentials credentials, int projectId, string branchId, string revisionId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteSourceCodeFile SourceCode_RetrieveFileById(RemoteCredentials credentials, int projectId, string branchId, string fileId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesForArtifact(RemoteCredentials credentials, int projectId, string branchId, int artifactTypeId, int artifactId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        byte[] SourceCode_OpenFileById(RemoteCredentials credentials, int projectId, string branchId, string fileId, string revisionId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisions(RemoteCredentials credentials, int projectId, string branchId, int startRow, int numberRows, RemoteSort remoteSort, List<RemoteFilter> remoteFilters);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisionsForFile(RemoteCredentials credentials, int projectId, string branchId, string fileId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteSourceCodeRevision SourceCode_RetrieveRevisionById(RemoteCredentials credentials, int projectId, string branchId, string revisionId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisionsForArtifact(RemoteCredentials credentials, int projectId, string branchId, int artifactTypeId, int artifactId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteLinkedArtifact> SourceCode_RetrieveArtifactsForRevision(RemoteCredentials credentials, int projectId, string branchId, string revisionId);

		[OperationContract]
		[FaultContract(typeof(ServiceFaultMessage))]
		RemoteSourceCodeConnection SourceCode_RetrieveConnectionInformation(RemoteCredentials credentials, int projectId);

        #endregion

        #region Instant Messenger

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteMessageInfo Message_GetInfo(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        long Message_PostNew(RemoteCredentials credentials, int recipientUserId, string message);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Message_MarkAllAsRead(RemoteCredentials credentials, int senderUserId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteUserMessage> Message_GetUnreadMessageSenders(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteMessage> Message_RetrieveUnread(RemoteCredentials credentials);

        #endregion

        #region History

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteHistoryChange> History_RetrieveForArtifact(RemoteCredentials credentials, int projectId, int artifactTypeId, int artifactId, int startingRow, int numberOfRows, RemoteSort remoteSort, List<RemoteFilter> remoteFilters);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteHistoryChangeSet History_RetrieveById(RemoteCredentials credentials, int projectId, int historyChangeSetId);

        #endregion

        #region Subscriptions

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Subscription_SubscribeToArtifact(RemoteCredentials credentials, int projectId, int artifactTypeId, int artifactId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        void Subscription_UnsubscribeFromArtifact(RemoteCredentials credentials, int projectId, int artifactTypeId, int artifactId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteArtifactSubscription> Subscription_RetrieveForUser(RemoteCredentials credentials);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteArtifactSubscription> Subscription_RetrieveForArtifact(RemoteCredentials credentials, int projectId, int artifactTypeId, int artifactId);

        #endregion

        #region Saved Filters

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSavedFilter> SavedFilter_RetrieveForUser(RemoteCredentials credentials);

        #endregion

        #region Reports

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTableData Reports_RetrieveCustomGraphData(RemoteCredentials credentials, int customGraphId, int? projectId, int? projectGroupId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        RemoteTableData Reports_RetrieveESQLQueryData(RemoteCredentials credentials, string query);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        List<RemoteSavedReport> Reports_RetrieveSaved(RemoteCredentials credentials, int projectId, bool includeShared);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        Guid Reports_GenerateSavedReport(RemoteCredentials credentials, int projectId, int savedReportId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        int Reports_CheckGeneratedReportStatus(RemoteCredentials credentials, int projectId, Guid reportGenerationId);

        [OperationContract]
        [FaultContract(typeof(ServiceFaultMessage))]
        byte[] Reports_RetrieveGeneratedReport(RemoteCredentials credentials, int projectId, int generatedReportId);

        #endregion
    }
}
