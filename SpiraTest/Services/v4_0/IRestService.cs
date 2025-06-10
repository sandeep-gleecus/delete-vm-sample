using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

using Inflectra.SpiraTest.Web.Services.v4_0.DataObjects;
using Inflectra.SpiraTest.Web.Services.Rest;

namespace Inflectra.SpiraTest.Web.Services.v4_0
{
    /// <summary>
    /// RESTful service interface for KronoDesk v1.0+. Alternative to the ImportExport.svc SOAP API
    /// </summary>
    /// <remarks>
    /// You can tell the service whether you are using XML or JSON representation
    /// by passing through the appropriate HTTP Headers:
    ///     accept: application/xml
    ///     accept: application/json
    /// </remarks>
    [
    ServiceContract(Namespace = "http://www.inflectra.com/SpiraTest/Services/v4.0/", SessionMode = SessionMode.NotAllowed)
    ]
    public interface IRestService
    {
        #region System operations

        [
        RestResource("System", "This resource contains information on the overall product instance, including version and settings information"),
        OperationContract,
        WebGet(UriTemplate = "system/productVersion")
        ]
        RemoteVersion System_GetProductVersion();

        #endregion

        #region Incident operations

        [
        RestResource("Incident", "This resource allows you to retrieve, create, modify and delete incidents in a specific project in the system."),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/{incident_id}")
        ]
        RemoteIncident Incident_RetrieveById(string project_id, string incident_id);

        [
        OperationContract,
        RestResource("Incident"),
        WebGet(UriTemplate = "projects/{project_id}/incidents/search-by-ids?ids={incident_ids}")
        ]
        List<RemoteIncident> Incident_RetrieveByIdList(string project_id, string incident_ids);

        [
        OperationContract,
        RestResource("Incident Priority", "This resource allows you to retrieve/create incident priorities defined in a specific project"),
        WebGet(UriTemplate = "projects/{project_id}/incidents/priorities")
        ]
        List<RemoteIncidentPriority> Incident_RetrievePriorities(string project_id);

        [
        RestResource("Incident Priority"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/incidents/priorities")
        ]
        RemoteIncidentPriority Incident_AddPriority(string project_id, RemoteIncidentPriority remoteIncidentPriority);

        [
        OperationContract,
        RestResource("Incident Severity", "This resource allows you to retrieve/create incident severities defined in a specific project"),
        WebGet(UriTemplate = "projects/{project_id}/incidents/severities")
        ]
        List<RemoteIncidentSeverity> Incident_RetrieveSeverities(string project_id);

        [
        RestResource("Incident Severity"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/incidents/severities")
        ]
        RemoteIncidentSeverity Incident_AddSeverity(string project_id, RemoteIncidentSeverity remoteIncidentSeverity);

        [
        OperationContract,
        RestResource("Incident Status", "This resource allows you to retrieve/create incident statuses defined in a specific project"),
        WebGet(UriTemplate = "projects/{project_id}/incidents/statuses")
        ]
        List<RemoteIncidentStatus> Incident_RetrieveStatuses(string project_id);

        [
        RestResource("Incident Status"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/incidents/statuses")
        ]
        RemoteIncidentStatus Incident_AddStatus(string project_id, RemoteIncidentStatus remoteIncidentStatus);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/incidents/types")
        ]
        RemoteIncidentType Incident_AddType(string project_id, RemoteIncidentType remoteIncidentType);

        [
        RestResource("Incident Type", "This resource allows you to retrieve/create incident types defined in a specific project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/types")
        ]
        List<RemoteIncidentType> Incident_RetrieveTypes(string project_id);

        [
        RestResource("Incident Status"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/statuses/default")
        ]
        RemoteIncidentStatus Incident_RetrieveDefaultStatus(string project_id);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/types/default")
        ]
        RemoteIncidentType Incident_RetrieveDefaultType(string project_id);

        [
        RestResource("Incident Comment", "This resource allows you to retrieve the comments associated with an incident as well as add new comments to an existing incident."),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/{incident_id}/comments")
        ]
        List<RemoteComment> Incident_RetrieveComments(string project_id, string incident_id);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/types/{incident_type_id}/workflow/transitions?status_id={incident_status_id}&is_detector={is_detector}&isOwner={is_owner}")
        ]
        List<RemoteWorkflowIncidentTransition> Incident_RetrieveWorkflowTransitions(string project_id, string incident_type_id, string incident_status_id, string is_detector, string is_owner);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/types/{incident_type_id}/workflow/fields?status_id={incident_status_id}")
        ]
        List<RemoteWorkflowIncidentFields> Incident_RetrieveWorkflowFields(string project_id, string incident_type_id, string incident_status_id);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/types/{incident_type_id}/workflow/custom-properties?status_id={incident_status_id}")
        ]
        List<RemoteWorkflowIncidentCustomProperties> Incident_RetrieveWorkflowCustomProperties(string project_id, string incident_type_id, string incident_status_id);

        [
        RestResource("Incident"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/count")
        ]
        long Incident_Count(string project_id);

        [
        RestResource("Incident"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents")
        ]
        List<RemoteIncident> Incident_Retrieve1(string project_id);

        [
        RestResource("Incident"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/search?start_row={start_row}&number_rows={number_rows}&sort_by={sort_by}")
        ]
        List<RemoteIncident> Incident_Retrieve2(string project_id, string start_row, string number_rows, string sort_by);

        [
        RestResource("Incident"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/recent?start_row={start_row}&number_rows={number_rows}&creation_date={creation_date}")
        ]
        List<RemoteIncident> Incident_RetrieveNew(string project_id, string start_row, string number_rows, string creation_date);

        [
        RestResource("Incident"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/incidents/search?start_row={start_row}&number_rows={number_rows}&sort_by={sort_by}")
        ]
        List<RemoteIncident> Incident_Retrieve3(string project_id, string start_row, string number_rows, string sort_by, List<RemoteFilter> remoteFilters);

        [
        RestResource("Incident Comment"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/incidents/{incident_id}/comments")
        ]
        List<RemoteComment> Incident_AddComments(string project_id, string incident_id, List<RemoteComment> remoteComments);

        [
        RestResource("Incident"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/incidents")
        ]
        RemoteIncident Incident_Create(string project_id, RemoteIncident remoteIncident);

        [
        RestResource("Incident"),
        OperationContract,
        WebInvoke(Method = "PUT" , UriTemplate = "projects/{project_id}/incidents/{incident_id}")
        ]
        void Incident_Update(string project_id, string incident_id, RemoteIncident remoteIncident);

        [
        RestResource("Incident"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/incidents/{incident_id}")
        ]
        void Incident_Delete(string project_id, string incident_id);

        #endregion

        #region CustomProperty operations

        [
        RestResource("Custom Property", "This resource allows you to retrieve and edit the custom properties defined in a specific project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/custom-properties/{artifact_type_name}")
        ]
        List<RemoteCustomProperty> CustomProperty_RetrieveForArtifactType(string project_id, string artifact_type_name);

        [
        RestResource("Custom List", "This resource allows you to retrieve and edit the custom lists defined in a specific project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/custom-lists/{custom_list_id}")
        ]
        RemoteCustomList CustomProperty_RetrieveCustomListById(string project_id, string custom_list_id);

        [
        RestResource("Custom List"),
        OperationContract,
        WebInvoke(Method="POST", UriTemplate = "projects/{project_id}/custom-lists")
        ]
        RemoteCustomList CustomProperty_AddCustomList(string project_id, RemoteCustomList remoteCustomList);

        [
        RestResource("Custom List Value", "This resource allows you to add custom list values to a custom list in a project"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/custom-lists/{custom_list_id}/values")
        ]
        RemoteCustomListValue CustomProperty_AddCustomListValue(string project_id, string custom_list_id, RemoteCustomListValue remoteCustomListValue);

        [
        RestResource("Custom Property"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/custom-properties?custom_list_id={custom_list_id}")
        ]
        RemoteCustomProperty CustomProperty_AddDefinition(string project_id, string custom_list_id, RemoteCustomProperty remoteCustomProperty);

        [
        RestResource("Custom Property"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/custom-properties/{custom_property_id}")
        ]
        void CustomProperty_UpdateDefinition(string project_id, string custom_property_id, RemoteCustomProperty remoteCustomProperty);

        [
        RestResource("Custom Property"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/custom-properties/{custom_property_id}")
        ]
        void CustomProperty_DeleteDefinition(string project_id, string custom_property_id);

        [
        RestResource("Custom List"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/custom-lists")
        ]
        List<RemoteCustomList> CustomProperty_RetrieveCustomLists(string project_id);

        [
        RestResource("Custom List"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/custom-lists/{custom_list_id}")
        ]
        void CustomProperty_UpdateCustomList(string project_id, string custom_list_id, RemoteCustomList remoteCustomList);

        #endregion

        #region Release operations

        [
        RestResource("Release", "This resource allows you to retrieve and manage the releases and iterations defined in a specific project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/releases")
        ]
        List<RemoteRelease> Release_Retrieve1(string project_id);

        [
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/search?start_row={start_row}&number_rows={number_rows}")
        ]
        List<RemoteRelease> Release_Retrieve2(string project_id, string start_row, string number_rows, List<RemoteFilter> remoteFilters);

        [
        RestResource("Release"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/releases/{release_id}")
        ]
        RemoteRelease Release_RetrieveById(string project_id, string release_id);

        [
        RestResource("Release Test Cases", "This resource lets you retrieve, add and remove test cases to/from releases"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{release_id}/test-cases")
        ]
        void Release_AddTestMapping(string project_id, string release_id, int[] testCaseIds);

        [
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/count")
        ]
        long Release_Count(string project_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases")
        ]
        RemoteRelease Release_Create1(string project_id, RemoteRelease remoteRelease);

        [
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{parent_release_id}")
        ]
        RemoteRelease Release_Create2(string project_id, string parent_release_id, RemoteRelease remoteRelease);

        [
        RestResource("Release Test Cases"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/releases/{release_id}/test-cases/{test_case_id}")
        ]
        void Release_RemoveTestMapping(string project_id, string release_id, string test_case_id);

        [
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/releases")
        ]
        void Release_Update(string project_id, RemoteRelease remoteRelease);

        [
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/releases/{release_id}")
        ]
        void Release_Delete(string project_id, string release_id);

        [
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{release_id}/move?destination_release_id={destination_release_id}")
        ]
        void Release_Move(string project_id, string release_id, string destination_release_id);

        [
        RestResource("Release Comments", "This resource allows you to retrieve the comments associated with a release as well as add new comments to an existing release. "),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/releases/{release_id}/comments")
        ]
        List<RemoteComment> Release_RetrieveComments(string project_id, string release_id);

        [
        RestResource("Release Test Cases"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/releases/{release_id}/test-cases")
        ]
        List<RemoteReleaseTestCaseMapping> Release_RetrieveTestMapping(string project_id, string release_id);

        [
        RestResource("Release Comments"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{release_id}/comments")
        ]
        RemoteComment Release_CreateComment(string project_id, string release_id, RemoteComment remoteComment);

        #endregion

        #region Project operations

        [
        RestResource("Project User", "This resource allows you to retrieve the users associated with a specific project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/users")
        ]
        List<RemoteProjectUser> Project_RetrieveUserMembership(string project_id);

        [
        RestResource("Project", "This resource allows you to retrieve projects in the system"),
        OperationContract,
        WebGet(UriTemplate = "projects")
        ]
        List<RemoteProject> Project_Retrieve();

        [
        RestResource("Project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}")
        ]
        RemoteProject Project_RetrieveById(string project_id);

        [
        RestResource("Project"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects?existing_project_id={existing_project_id}")
        ]
        RemoteProject Project_Create(string existing_project_id, RemoteProject remoteProject);

        [
        RestResource("Project"),
        OperationContract,
        WebInvoke(Method="DELETE", UriTemplate = "projects/{project_id}")
        ]
        void Project_Delete(string project_id);

        [
        RestResource("Project"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/refresh-caches/{release_id}?run_async={run_async}")
        ]
        void Project_RefreshProgressExecutionStatusCaches(string project_id, string release_id, string run_async);

        #endregion

        #region Document

        [
        RestResource("Document"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/documents/{document_id}/open")
        ]
        byte[] Document_OpenFile(string project_id, string document_id);

        [
        RestResource("Document"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/file?filename={filename}&tags={tags}&folder_id={folder_id}&document_type_id={document_type_id}&artifact_type_id={artifact_type_id}&artifact_id={artifact_id}")
        ]
        RemoteDocument Document_AddFile(string project_id, string filename, string tags, string folder_id, string document_type_id, string artifact_type_id, string artifact_id, byte[] binaryData);

        [
        RestResource("Document"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/url?url={url}&tags={tags}&folder_id={folder_id}&document_type_id={document_type_id}&artifact_type_id={artifact_type_id}&artifact_id={artifact_id}")
        ]
        RemoteDocument Document_AddUrl(string project_id, string url, string tags, string folder_id, string document_type_id, string artifact_type_id, string artifact_id);

        [
        RestResource("Document Version", "This resource lets you add new versions of documents"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/{document_id}/versions/file?filename={filename}&version={version}&make_current={make_current}")
        ]
        int Document_AddFileVersion(string project_id, string document_id, string filename, string version, string make_current, byte[] binaryData);

        [
        RestResource("Document Version"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/{document_id}/versions/url?url={url}&version={version}&make_current={make_current}")
        ]
        int Document_AddUrlVersion(string project_id, string document_id, string url, string version, string make_current);

        [
        RestResource("Document"),
        OperationContract,
        WebInvoke(Method="DELETE", UriTemplate = "projects/{project_id}/documents/{document_id}")
        ]
        void Document_Delete(string project_id, string document_id);

        [
        RestResource("Document", "This resource allows access to the document attachments in the system"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/documents")
        ]
        List<RemoteDocument> Document_Retrieve1(string project_id);

        [
        RestResource("Document"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/search?start_row={start_row}&number_rows={number_rows}&sort_by={sort_by}")
        ]
        List<RemoteDocument> Document_Retrieve2(string project_id, string start_row, string number_rows, string sort_by, List<RemoteFilter> remoteFilters);

        [
        RestResource("Document"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/document-folders/{folder_id}/documents/search?start_row={start_row}&number_rows={number_rows}&sort_by={sort_by}")
        ]
        List<RemoteDocument> Document_RetrieveForFolder(string project_id, string folder_id, string start_row, string number_rows, string sort_by, List<RemoteFilter> remoteFilters);

        [
        RestResource("Artifact Documents", "This resource lets you view/add documents associated to an artifact"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents")
        ]
        List<RemoteDocument> Document_RetrieveForArtifact(string project_id, string artifact_type_id, string artifact_id);

        [
        RestResource("Artifact Documents"),
        OperationContract,
        WebInvoke(Method="POST", UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents/{document_id}")
        ]
        void Document_AddToArtifactId(string project_id, string artifact_type_id, string artifact_id, string document_id);

        [
        RestResource("Artifact Documents"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents/{document_id}")
        ]
        void Document_DeleteFromArtifact(string project_id, string artifact_type_id, string artifact_id, string document_id);

        [
        RestResource("Document"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/documents/{document_id}")
        ]
        RemoteDocument Document_RetrieveById(string project_id, string document_id);

        [
        RestResource("Document Type", "This resources provides access to the active document types in the project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/document-types?active_only={active_only}")
        ]
        List<RemoteDocumentType> Document_RetrieveTypes(string project_id, string active_only);

        [
        RestResource("Document Folder", "This resources provides access to the active document folders in the project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/document-folders")
        ]
        List<RemoteDocumentFolder> Document_RetrieveFolders(string project_id);

        [
        RestResource("Document Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/document-folders/{folder_id}")
        ]
        RemoteDocumentFolder Document_RetrieveFolderById(string project_id, string folder_id);

        [
        RestResource("Document Folder"),
        OperationContract,
        WebInvoke(Method="POST", UriTemplate = "projects/{project_id}/document-folders")
        ]
        RemoteDocumentFolder Document_AddFolder(string project_id, RemoteDocumentFolder remoteDocumentFolder);

        [
        RestResource("Document Folder"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/document-folders/{folder_id}")
        ]
        void Document_DeleteFolder(string project_id, string folder_id);

        [
        RestResource("Document Folder"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/document-folders/{folder_id}")
        ]
        void Document_UpdateFolder(string project_id, string folder_id, RemoteDocumentFolder remoteDocumentFolder);

        #endregion

        #region Requirement

        [
        RestResource("Requirement", "This resource allows you to retrieve, create, modify and delete requirements in a specific project in the system."),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements/count")
        ]
        long Requirement_Count1(string project_id);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method="POST", UriTemplate = "projects/{project_id}/requirements/count")
        ]
        long Requirement_Count2(string project_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Requirement Test Coverage", "This resource lets you view, add and remove test cases from requirements."),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/test-cases")
        ]        
        void Requirement_AddTestCoverage(string project_id, RemoteRequirementTestCaseMapping remoteReqTestCaseMapping);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements")
        ]
        RemoteRequirement Requirement_Create1(string project_id, RemoteRequirement remoteRequirement);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/indent/{indent_position}")
        ]
        RemoteRequirement Requirement_Create2(string project_id, string indent_position, RemoteRequirement remoteRequirement);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/parent/{parent_requirement_id}")
        ]
        RemoteRequirement Requirement_Create3(string project_id, string parent_requirement_id, RemoteRequirement remoteRequirement);

        [
        RestResource("Requirement Test Coverage"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/requirements/test-cases")
        ]
        void Requirement_RemoveTestCoverage(string project_id, RemoteRequirementTestCaseMapping remoteReqTestCaseMapping);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/search?starting_row={starting_row}&number_of_rows={number_of_rows}")
        ]
        List<RemoteRequirement> Requirement_Retrieve1(string project_id, string starting_row, string number_of_rows, List<RemoteFilter> remoteFilters);

        [
        RestResource("Requirement"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements?starting_row={starting_row}&number_of_rows={number_of_rows}")
        ]
        List<RemoteRequirement> Requirement_Retrieve2(string project_id, string starting_row, string number_of_rows);

        [
        RestResource("Requirement"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements/{requirement_id}")
        ]
        RemoteRequirement Requirement_RetrieveById(string project_id, string requirement_id);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method="PUT", UriTemplate = "projects/{project_id}/requirements")
        ]
        void Requirement_Update(string project_id, RemoteRequirement remoteRequirement);

        [
        RestResource("Requirement"),
        OperationContract,
        WebGet(UriTemplate = "requirements")
        ]
        List<RemoteRequirement> Requirement_RetrieveForOwner();

        [
        RestResource("Requirement Test Coverage"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements/{requirement_id}/test-cases")
        ]
        List<RemoteRequirementTestCaseMapping> Requirement_RetrieveTestCoverage(string project_id, string requirement_id);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/requirements/{requirement_id}")
        ]
        void Requirement_Delete(string project_id, string requirement_id);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/move/{destination_requirement_id}")
        ]
        void Requirement_Move(string project_id, string requirement_id, string destination_requirement_id);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/indent")
        ]
        void Requirement_Indent(string project_id, string requirement_id);

        [
        RestResource("Requirement"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/outdent")
        ]
        void Requirement_Outdent(string project_id, string requirement_id);

        [
        RestResource("Requirement Comment", "This resource lets you retrieve and add comments to a requirement in the system."),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements/{requirement_id}/comments")
        ]
        List<RemoteComment> Requirement_RetrieveComments(string project_id, string requirement_id);

        [
        RestResource("Requirement Comment"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/comments")
        ]
        RemoteComment Requirement_CreateComment(string project_id, string requirement_id, RemoteComment remoteComment);

        #endregion

        #region Test Case

        [
        RestResource("Test Case", "This resource allows you to retrieve, create, modify and delete test cases in a specific project in the system."),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/count")
        ]
        long TestCase_Count1(string project_id);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/count")
        ]
        long TestCase_Count2(string project_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Case Parameter", "This resource allows you to add/remove parameters to test cases"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/parameters")
        ]
        RemoteTestCaseParameter TestCase_AddParameter(string project_id, RemoteTestCaseParameter remoteTestCaseParameter);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases?parent_test_folder_id={parent_test_folder_id}")
        ]
        RemoteTestCase TestCase_Create(string project_id, string parent_test_folder_id, RemoteTestCase remoteTestCase);

        [
        RestResource("Test Case Folder", "This resource allows you to create test case folders in the project"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-folders?parent_test_folder_id={parent_test_folder_id}")
        ]
        RemoteTestCase TestCase_CreateFolder(string project_id, string parent_test_folder_id, RemoteTestCase remoteTestCase);

        [
        RestResource("Test Case Parameter"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/parameters/create-token?parameter_name={parameter_name}")
        ]
        string TestCase_CreateParameterToken(string project_id, string parameter_name);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}")
        ]
        List<RemoteTestCase> TestCase_Retrieve1(string project_id, string starting_row, string number_of_rows, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Case"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}")
        ]
        List<RemoteTestCase> TestCase_Retrieve2(string project_id, string starting_row, string number_of_rows);

        [
        RestResource("Test Case Parameter"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/parameters")
        ]
        List<RemoteTestCaseParameter> TestCase_RetrieveParameters(string project_id, string test_case_id);

        [
        RestResource("Test Step Parameter", "This resource lets you retrieve the parameters associated with a specific test step"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}/parameters")
        ]
        List<RemoteTestStepParameter> TestCase_RetrieveStepParameters(string project_id, string test_case_id, string test_step_id);

        [
        RestResource("Test Case"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/{test_case_id}")
        ]
        RemoteTestCase TestCase_RetrieveById(string project_id, string test_case_id);

        [
        RestResource("Release Test Cases"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{release_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}")
        ]
        List<RemoteTestCase> TestCase_RetrieveByReleaseId(string project_id, string release_id, string starting_row, string number_of_rows, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Set Test Case", "This resource lets you create, view, modify the test cases associated with test sets"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-cases")
        ]
        List<RemoteTestCase> TestCase_RetrieveByTestSetId(string project_id, string test_set_id);

        [
        RestResource("Test Case"),
        OperationContract,
        WebGet(UriTemplate = "test-cases")
        ]
        List<RemoteTestCase> TestCase_RetrieveForOwner();

        [
        RestResource("Test Case"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-folders/{test_case_folder_id}/test-cases")
        ]
        List<RemoteTestCase> TestCase_RetrieveByFolder(string project_id, string test_case_folder_id);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-cases")
        ]
        void TestCase_Update(string project_id, RemoteTestCase remoteTestCase);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}")
        ]
        void TestCase_Delete(string project_id, string test_case_id);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/move?destination_test_case_folder_id={destination_test_case_folder_id}")
        ]
        void TestCase_Move(string project_id, string test_case_id, string destination_test_case_folder_id);

        [
        RestResource("Test Step", "This resource lets you create and delete test steps from test cases in the system"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{source_test_step_id}/move?destination_test_step_id={destination_test_step_id}")
        ]
        void TestCase_MoveStep(string project_id, string test_case_id, string source_test_step_id, string destination_test_step_id);

        [
        RestResource("Test Step"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}")
        ]
        void TestCase_DeleteStep(string project_id, string test_case_id, string test_step_id);

        [
        RestResource("Test Step"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps")
        ]
        RemoteTestStep TestCase_AddStep(string project_id, string test_case_id, RemoteTestStep remoteTestStep);

        [
        RestResource("Test Case Link", "This resource lets you add linked test cases to a test case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-links/{linked_test_case_id}?position={position}")
        ]
        int TestCase_AddLink(string project_id, string test_case_id, string linked_test_case_id, string position, List<RemoteTestStepParameter> parameters);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/add-update-automation-script?automation_engine_id={automation_engine_id}&url_or_filename={url_or_filename}&description={description}&version={version}&project_attachment_type_id={project_attachment_type_id}&project_attachment_folder_id={project_attachment_folder_id}")
        ]
        void TestCase_AddUpdateAutomationScript(string project_id, string test_case_id, string automation_engine_id, string url_or_filename, string description, string version, string project_attachment_type_id, string project_attachment_folder_id, byte[] binaryData);

        [
        RestResource("Test Case Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-folders/{test_folder_id}/count")
        ]
        long TestCase_CountForFolder1(string project_id, string test_folder_id);

        [
        RestResource("Test Case Folder"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-folders/{test_folder_id}/count")
        ]
        long TestCase_CountForFolder2(string project_id, string test_folder_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Case Comment", "This resource lets you add and view comments associated with test cases"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/comments")
        ]
        List<RemoteComment> TestCase_RetrieveComments(string project_id, string test_case_id);

        [
        RestResource("Test Case Comment"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/comments")
        ]
        RemoteComment TestCase_CreateComment(string project_id, string test_case_id, RemoteComment remoteComment);

        #endregion

        #region Test Run

        [
        RestResource("Test Run", "This resource allows you to retrieve, create, modify and delete test runs in a specific project in the system."),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/create?release_id={release_id}")
        ]
        List<RemoteManualTestRun> TestRun_CreateFromTestCases(string project_id, string release_id, List<int> testCaseIds);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/create/test_set/{test_set_id}")
        ]
        List<RemoteManualTestRun> TestRun_CreateFromTestSet(string project_id, string test_set_id);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/create/automation_host/{automation_host_token}")
        ]
        List<RemoteAutomatedTestRun> TestRun_CreateForAutomationHost(string project_id, string automation_host_token, DataObjects.DateRange dateRange);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/create/test_set/{test_set_id}/automation_host/{automation_host_token}")
        ]
        List<RemoteAutomatedTestRun> TestRun_CreateForAutomatedTestSet(string project_id, string test_set_id, string automation_host_token);

        [
        RestResource("Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/count")
        ]
        long TestRun_Count1(string project_id);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/count")
        ]
        long TestRun_Count2(string project_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/record")
        ]
        RemoteAutomatedTestRun TestRun_RecordAutomated1(string project_id, RemoteAutomatedTestRun remoteTestRun);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/record-multiple")
        ]
        List<RemoteAutomatedTestRun> TestRun_RecordAutomated2(string project_id, List<RemoteAutomatedTestRun> remoteTestRuns);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-runs?end_date={end_date}")
        ]
        List<RemoteManualTestRun> TestRun_Save(string project_id, string end_date, List<RemoteManualTestRun> remoteTestRuns);

        [
        RestResource("Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/{test_run_id}")
        ]
        RemoteTestRun TestRun_RetrieveById(string project_id, string test_run_id);

        [
        RestResource("Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/{test_run_id}/automated")
        ]
        RemoteAutomatedTestRun TestRun_RetrieveAutomatedById(string project_id, string test_run_id);

        [
        RestResource("Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/{test_run_id}/manual")
        ]
        RemoteManualTestRun TestRun_RetrieveManualById(string project_id, string test_run_id);

        [
        RestResource("Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteTestRun> TestRun_Retrieve1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteTestRun> TestRun_Retrieve2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/manual?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteManualTestRun> TestRun_RetrieveManual1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/search/manual?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteManualTestRun> TestRun_RetrieveManual2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/automated??starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/search/automated??starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

        #endregion

        #region Task

        [
        RestResource("Task", "This resource allows you to retrieve, create, modify and delete tasks in a specific project in the system."),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/tasks")
        ]
        RemoteTask Task_Create(string project_id, RemoteTask remoteTask);

        [
        RestResource("Task"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/tasks/count")
        ]
        long Task_Count1(string project_id);

        [
        RestResource("Task"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/tasks/count")
        ]
        long Task_Count2(string project_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Task"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/tasks?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteTask> Task_Retrieve(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("Task"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/tasks/{task_id}")
        ]
        RemoteTask Task_RetrieveById(string project_id, string task_id);

        [
        RestResource("Task"),
        OperationContract,
        WebGet(UriTemplate = "tasks")
        ]
        List<RemoteTask> Task_RetrieveForOwner();

        [
        RestResource("Task"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/tasks/new?creation_date={creation_date}&start_row={start_row}&number_of_rows={number_of_rows}")
        ]
        List<RemoteTask> Task_RetrieveNew(string project_id, string creation_date, string start_row, string number_of_rows);

        [
        RestResource("Task"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/tasks")
        ]
        void Task_Update(string project_id, RemoteTask remoteTask);

        [
        RestResource("Task"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/tasks/{task_id}")
        ]
        void Task_Delete(string project_id, string task_id);

        [
        RestResource("Task Comment", "This resource allows you to add and retrieve task comments in the system"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/tasks/{task_id}/comments")
        ]
        List<RemoteComment> Task_RetrieveComments(string project_id, string task_id);

        [
        RestResource("Task"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/tasks/{task_id}/comments")
        ]
        RemoteComment Task_CreateComment(string project_id, string task_id, RemoteComment remoteComment);

        #endregion

        #region Test Set

        [
        RestResource("Test Set Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-case-mapping/{test_case_id}?owner_id={owner_id}&existing_test_set_test_case_id={existing_test_set_test_case_id}")
        ]
        List<RemoteTestSetTestCaseMapping> TestSet_AddTestMapping(string project_id, string test_set_id, string test_case_id, string owner_id, string existing_test_set_test_case_id, List<RemoteTestSetTestCaseParameter> parameters);

        [
        RestResource("Test Set", "This resource lets you create, view, edit and delete test sets in the system"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets?parent_test_set_folder_id={parent_test_set_folder_id}")
        ]
        RemoteTestSet TestSet_Create(string project_id, string parent_test_set_folder_id, RemoteTestSet remoteTestSet);

        [
        RestResource("Test Set Folder", "This resource lets you create test set folders in the system"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-set-folders?parent_test_set_folder_id={parent_test_set_folder_id}")
        ]
        RemoteTestSet TestSet_CreateFolder(string project_id, string parent_test_set_folder_id, RemoteTestSet remoteTestSet);

        [
        RestResource("Test Set Test Case"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-cases/{test_set_test_case_id}")
        ]
        void TestSet_RemoveTestMapping(string project_id, string test_set_id, string test_set_test_case_id);

        [
        RestResource("Test Set Test Case"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-case-mapping")
        ]
        List<RemoteTestSetTestCaseMapping> TestSet_RetrieveTestCaseMapping(string project_id, string test_set_id);

        [
        RestResource("Test Set"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets")
        ]
        List<RemoteTestSet> TestSet_Retrieve1(string project_id);

        [
        RestResource("Test Set"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/search?starting_row={starting_row}&number_of_rows={number_of_rows}")
        ]
        List<RemoteTestSet> TestSet_Retrieve2(string project_id, string starting_row, string number_of_rows, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Set"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}")
        ]
        RemoteTestSet TestSet_RetrieveById(string project_id, string test_set_id);

        [
        RestResource("Test Set"),
        OperationContract,
        WebGet(UriTemplate = "test-sets")
        ]
        List<RemoteTestSet> TestSet_RetrieveForOwner();

        [
        RestResource("Test Set"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-sets/")
        ]
        void TestSet_Update(string project_id, RemoteTestSet remoteTestSet);

        [
        RestResource("Test Set"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}")
        ]
        void TestSet_Delete(string project_id, string test_set_id);

        [
        RestResource("Test Set"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/move?destination_test_set_folder_id={destination_test_set_folder_id}")
        ]
        void TestSet_Move(string project_id, string test_set_id, string destination_test_set_folder_id);

        [
        RestResource("Test Set"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/count")
        ]
        long TestSet_Count1(string project_id);

        [
        RestResource("Test Set"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/count")
        ]
        long TestSet_Count2(string project_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Set Comment", "This resource lets you add and view comments associated with test sets"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/comments")
        ]
        List<RemoteComment> TestSet_RetrieveComments(string project_id, string test_set_id);

        [
        RestResource("Test Set Comment"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/comments")
        ]
        RemoteComment TestSet_CreateComment(string project_id, string test_set_id, RemoteComment remoteComment);

        #endregion

        #region User

        [
        RestResource("User", "This resource lets you create, retrieve and delete users in the system"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "users?password={password}&password_question={password_question}&password_answer={password_answer}&project_id={project_id}&project_role_id={project_role_id}")
        ]
        RemoteUser User_Create(string password, string password_question, string password_answer, string project_id, string project_role_id, RemoteUser remoteUser);

        [
        RestResource("User"),
        OperationContract,
        WebGet(UriTemplate = "users")
        ]
        RemoteUser User_Retrieve();

        [
        RestResource("User"),
        OperationContract,
        WebGet(UriTemplate = "users/{user_id}")
        ]
        RemoteUser User_RetrieveById(string user_id);

        [
        RestResource("User"),
        OperationContract,
        WebGet(UriTemplate = "users/usernames/{user_name}")
        ]
        RemoteUser User_RetrieveByUserName(string user_name);

        [
        RestResource("User"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "users/{user_id}")
        ]
        void User_Delete(string user_id);

        #endregion

        #region Automation Host

        [
        RestResource("Automation Host", "This resource lets you create, view, update and delete automation hosts in the system"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/automation-hosts")
        ]
        List<RemoteAutomationHost> AutomationHost_Retrieve1(string project_id);

        [
        RestResource("Automation Host"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/automation-hosts/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteAutomationHost> AutomationHost_Retrieve2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("Automation Host"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/automation-hosts/{automation_host_id}")
        ]
        RemoteAutomationHost AutomationHost_RetrieveById(string project_id, string automation_host_id);

        [
        RestResource("Automation Host"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/automation-hosts/tokens/{token}")
        ]
        RemoteAutomationHost AutomationHost_RetrieveByToken(string project_id, string token);

        [
        RestResource("Automation Host"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/automation-hosts")
        ]
        RemoteAutomationHost AutomationHost_Create(string project_id, RemoteAutomationHost remoteAutomationHost);

        [
        RestResource("Automation Host"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/automation-hosts")
        ]
        void AutomationHost_Update(string project_id, RemoteAutomationHost remoteAutomationHost);

        [
        RestResource("Automation Host"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/automation-hosts/{automation_host_id}")
        ]
        void AutomationHost_Delete(string project_id, string automation_host_id);

        #endregion

        #region Automation Engine

        [
        RestResource("Automation Engine", "This resource lets you create, and view automation engines in the system"),
        OperationContract,
        WebGet(UriTemplate = "automation-engines/tokens{token}")
        ]
        RemoteAutomationEngine AutomationEngine_RetrieveByToken(string token);

        [
        RestResource("Automation Engine"),
        OperationContract,
        WebGet(UriTemplate = "automation-engines?active_only={active_only}")
        ]
        List<RemoteAutomationEngine> AutomationEngine_Retrieve(string active_only);

        [
        RestResource("Automation Engine"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "automation-engines")
        ]
        RemoteAutomationEngine AutomationEngine_Create(RemoteAutomationEngine remoteEngine);

        [
        RestResource("Automation Engine"),
        OperationContract,
        WebGet(UriTemplate = "automation-engines/{automation_engine_id}")
        ]
        RemoteAutomationEngine AutomationEngine_RetrieveById(string automation_engine_id);
        
        #endregion

        #region Build

        [
        RestResource("Release Build", "This resource lets you create, and view automated builds in the system"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/releases/{release_id}/builds/{build_id}")
        ]
        RemoteBuild Build_RetrieveById(string project_id, string release_id, string build_id);

        [
        RestResource("Release Build"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/releases/{release_id}/builds")
        ]
        List<RemoteBuild> Build_RetrieveByReleaseId(string project_id, string release_id);

        [
        RestResource("Release Build"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{release_id}/builds")
        ]
        RemoteBuild Build_Create(string project_id, string release_id, RemoteBuild remoteBuild);

        #endregion
        
		#region ProjectRole

        [
        RestResource("Project Role", "This resource lets you view the list of project roles in the system"),
        OperationContract,
        WebGet(UriTemplate = "projects-roles")
        ]
		List<RemoteProjectRole> ProjectRole_Retrieve();

		#endregion

		#region Association

        [
        RestResource("Association", "This resource lets you create, view and update artifact associations"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/associations")
        ]
		RemoteAssociation Association_Create(string project_id, RemoteAssociation remoteAssociation);

        [
        RestResource("Association", "This resource lets you create, view and update artifact associations"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/associations")
        ]
        void Association_Update(string project_id, RemoteAssociation remoteAssociation);

        [
        RestResource("Association"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/associations/{artifact_type_id}/{artifact_id}")
        ]
        List<RemoteAssociation> Association_RetrieveForArtifact(string project_id, string artifact_type_id, string artifact_id);

		#endregion

		#region Data Mapping

        [
        RestResource("Data Mapping Artifacts", "This resource lets you create and retrieve the data mapping for artifacts"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}")
        ]
        List<RemoteDataMapping> DataMapping_RetrieveArtifactMappings(string project_id, string data_sync_system_id, string artifact_type_id);

        [
        RestResource("Data Mapping Artifacts"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}")
        ]
        void DataMapping_AddArtifactMappings(string project_id, string data_sync_system_id, string artifact_type_id, List<RemoteDataMapping> remoteDataMappings);

        [
        RestResource("Data Mapping Artifacts"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}")
        ]
        void DataMapping_RemoveArtifactMappings(string project_id, string data_sync_system_id, string artifact_type_id, List<RemoteDataMapping> remoteDataMappings);

        [
        RestResource("Data Mapping Users", "This resource lets you view and add user data mappings"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "data-mappings/{data_sync_system_id}/users")
        ]
        void DataMapping_AddUserMappings(string data_sync_system_id, List<RemoteDataMapping> remoteDataMappings);

        [
        RestResource("Data Mapping Users", "This resource lets you view the user data mappings"),
        OperationContract,
        WebGet(UriTemplate = "data-mappings/{data_sync_system_id}/users")
        ]
        List<RemoteDataMapping> DataMapping_RetrieveUserMappings(string data_sync_system_id);

        [
        RestResource("Data Mapping Custom Properties", "This resource lets you view the custom property data mappings"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}/custom-properties/{custom_property_id}")
        ]
        RemoteDataMapping DataMapping_RetrieveCustomPropertyMapping(string project_id, string data_sync_system_id, string artifact_type_id, string custom_property_id);

        [
        RestResource("Data Mapping Custom Property Values", "This resource lets you view the custom property value data mappings"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}/custom-properties/{custom_property_id}/values")
        ]
        List<RemoteDataMapping> DataMapping_RetrieveCustomPropertyValueMappings(string project_id, string data_sync_system_id, string artifact_type_id, string custom_property_id);

        [
        RestResource("Data Mapping Field Values", "This resource lets you view the standard field data mappings"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/data-mappings/{data_sync_system_id}/field-values/{artifact_field_id}")
        ]
        List<RemoteDataMapping> DataMapping_RetrieveFieldValueMappings(string project_id, string data_sync_system_id, string artifact_field_id);

        [
        RestResource("Data Mapping Projects", "This resource lets you view the project data mappings"),
        OperationContract,
        WebGet(UriTemplate = "data-mappings/{data_sync_system_id}/projects")
        ]
        List<RemoteDataMapping> DataMapping_RetrieveProjectMappings(string data_sync_system_id);

		#endregion

        #region DataSync

        [
        RestResource("Data Sync", "This resource lets you retrieve the data synchronization plugins configured and update the synchronization status"),
        OperationContract,
        WebGet(UriTemplate = "data-syncs")
        ]
        List<RemoteDataSyncSystem> DataSyncSystem_Retrieve();

        [
        RestResource("Data Sync"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "data-syncs/{data_sync_system_id}/failure")
        ]
        void DataSyncSystem_SaveRunFailure(string data_sync_system_id);

        [
        RestResource("Data Sync"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "data-syncs/{data_sync_system_id}/success")
        ]
        void DataSyncSystem_SaveRunSuccess(string data_sync_system_id, DateTime lastRunDate);

        [
        RestResource("Data Sync"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "data-syncs/{data_sync_system_id}/warning")
        ]
        void DataSyncSystem_SaveRunWarning(string data_sync_system_id, DateTime lastRunDate);

        [
        RestResource("Data Sync Event", "This resource lets you record events in Spira from data-synchronization activities"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "data-syncs/events")
        ]
        void DataSyncSystem_WriteEvent(RemoteEvent remoteEvent);

        #endregion
    }
}
