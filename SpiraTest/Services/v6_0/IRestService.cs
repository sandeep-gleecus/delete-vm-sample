using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

using Inflectra.SpiraTest.Web.Services.v6_0.DataObjects;
using Inflectra.SpiraTest.Web.Services.Rest;

namespace Inflectra.SpiraTest.Web.Services.v6_0
{
    /// <summary>
    /// RESTful service interface for SpiraTeam v6.0+. Alternative to the ImportExport.svc SOAP API
    /// </summary>
    /// <remarks>
    /// You can tell the service whether you are using XML or JSON representation
    /// by passing through the appropriate HTTP Headers:
    ///     accept: application/xml
    ///     accept: application/json
    /// </remarks>
    [
    ServiceContract(Namespace = "http://www.inflectra.com/SpiraTest/Services/v6.0/", SessionMode = SessionMode.NotAllowed)
    ]
    public interface IRestService
    {
		#region System operations

		[
        RestResource("System", "This resource contains information on the overall product instance, including version and settings information"),
        OperationContract,
        WebGet(UriTemplate = "system/product-version")
        ]
        RemoteVersion System_GetProductVersion();

		[
		RestResource("System"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "system/notifications")
		]
		void System_ProcessNotifications();

		[
		RestResource("System"),
        OperationContract,
        WebGet(UriTemplate = "system/settings")
        ]
        List<RemoteSetting> System_GetSettings();

        [
        RestResource("System"),
        OperationContract,
        WebGet(UriTemplate = "system/artifact-types/{navigation_link_id}/project/{project_id}/artifact/{artifact_id}?tab_name={tab_name}")
        ]
        string System_GetArtifactUrl(string navigation_link_id, string project_id, string artifact_id, string tab_name);

        [
        RestResource("System"),
        OperationContract,
        WebGet(UriTemplate = "system/artifact-types/{artifact_type_id}/{artifact_id}/project-id")
        ]
        int System_GetProjectIdForArtifact(string artifact_type_id, string artifact_id);

        [
        RestResource("System"),
        OperationContract,
        WebGet(UriTemplate = "system/settings/product-name")
        ]
        string System_GetProductName();

        [
        RestResource("System"),
        OperationContract,
        WebGet(UriTemplate = "system/settings/server-date-time")
        ]
        DateTime System_GetServerDateTime();

        [
        RestResource("System"),
        OperationContract,
        WebGet(UriTemplate = "system/settings/web-server-url")
        ]
        string System_GetWebServerUrl();

		[
		RestResource("System"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "system/events/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
		]
		List<RemoteEvent2> System_RetrieveEvents(string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

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
        RestResource("Incident"),
        WebGet(UriTemplate = "projects/{project_id}/incidents/search-by-test-case/{test_case_id}?open_only={open_only}")
        ]
        List<RemoteIncident> Incident_RetrieveByTestCase(string project_id, string test_case_id, string open_only);

        [
        OperationContract,
        RestResource("Incident"),
        WebGet(UriTemplate = "projects/{project_id}/incidents/search-by-test-run-step/{test_run_step_id}")
        ]
        List<RemoteIncident> Incident_RetrieveByTestRunStep(string project_id, string test_run_step_id);

        [
        OperationContract,
        RestResource("Incident"),
        WebGet(UriTemplate = "projects/{project_id}/incidents/search-by-test-step/{test_step_id}")
        ]
        List<RemoteIncident> Incident_RetrieveByTestStep(string project_id, string test_step_id);

        [
        OperationContract,
        RestResource("Incident Priority", "This resource allows you to retrieve/create incident priorities defined in a specific project template"),
        WebGet(UriTemplate = "project-templates/{project_template_id}/incidents/priorities")
        ]
        List<RemoteIncidentPriority> Incident_RetrievePriorities(string project_template_id);

        [
        RestResource("Incident Priority"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "project-templates/{project_template_id}/incidents/priorities")
        ]
        RemoteIncidentPriority Incident_AddPriority(string project_template_id, RemoteIncidentPriority remoteIncidentPriority);

        [
        OperationContract,
        RestResource("Incident Severity", "This resource allows you to retrieve/create incident severities defined in a specific project template"),
        WebGet(UriTemplate = "project-templates/{project_template_id}/incidents/severities")
        ]
        List<RemoteIncidentSeverity> Incident_RetrieveSeverities(string project_template_id);

        [
        RestResource("Incident Severity"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "project-templates/{project_template_id}/incidents/severities")
        ]
        RemoteIncidentSeverity Incident_AddSeverity(string project_template_id, RemoteIncidentSeverity remoteIncidentSeverity);

        [
        OperationContract,
        RestResource("Incident Status", "This resource allows you to retrieve/create incident statuses defined in a specific project template"),
        WebGet(UriTemplate = "project-templates/{project_template_id}/incidents/statuses")
        ]
        List<RemoteIncidentStatus> Incident_RetrieveStatuses(string project_template_id);

        [
        RestResource("Incident Status"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "project-templates/{project_template_id}/incidents/statuses")
        ]
        RemoteIncidentStatus Incident_AddStatus(string project_template_id, RemoteIncidentStatus remoteIncidentStatus);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "project-templates/{project_template_id}/incidents/types")
        ]
        RemoteIncidentType Incident_AddType(string project_template_id, RemoteIncidentType remoteIncidentType);

        [
        RestResource("Incident Type", "This resource allows you to retrieve/create incident types defined in a specific project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/incidents/types")
        ]
        List<RemoteIncidentType> Incident_RetrieveTypes(string project_template_id);

        [
        RestResource("Incident Status"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/incidents/statuses/default")
        ]
        RemoteIncidentStatus Incident_RetrieveDefaultStatus(string project_template_id);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/incidents/types/default")
        ]
        RemoteIncidentType Incident_RetrieveDefaultType(string project_template_id);

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
        List<RemoteWorkflowTransition> Incident_RetrieveWorkflowTransitions(string project_id, string incident_type_id, string incident_status_id, string is_detector, string is_owner);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/incidents/types/{incident_type_id}/workflow/fields?status_id={incident_status_id}")
        ]
        List<RemoteWorkflowField> Incident_RetrieveWorkflowFields(string project_template_id, string incident_type_id, string incident_status_id);

        [
        RestResource("Incident Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/incidents/types/{incident_type_id}/workflow/custom-properties?status_id={incident_status_id}")
        ]
        List<RemoteWorkflowCustomProperty> Incident_RetrieveWorkflowCustomProperties(string project_template_id, string incident_type_id, string incident_status_id);

        [
        RestResource("Incident"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/incidents/count")
        ]
        long Incident_Count(string project_id);

        [
        RestResource("Incident"),
        OperationContract,
        WebGet(UriTemplate = "incidents")
        ]
        List<RemoteIncident> Incident_RetrieveForOwner();

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
        RestResource("Custom Property", "This resource allows you to retrieve and edit the custom properties defined in a specific project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/custom-properties/{artifact_type_name}")
        ]
        List<RemoteCustomProperty> CustomProperty_RetrieveForArtifactType(string project_template_id, string artifact_type_name);

        [
        RestResource("Custom List", "This resource allows you to retrieve and edit the custom lists defined in a specific project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/custom-lists/{custom_list_id}")
        ]
        RemoteCustomList CustomProperty_RetrieveCustomListById(string project_template_id, string custom_list_id);

        [
        RestResource("Custom List"),
        OperationContract,
        WebInvoke(Method="POST", UriTemplate = "project-templates/{project_template_id}/custom-lists")
        ]
        RemoteCustomList CustomProperty_AddCustomList(string project_template_id, RemoteCustomList remoteCustomList);

        [
        RestResource("Custom List Value", "This resource allows you to add custom list values to a custom list in a project template"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "project-templates/{project_template_id}/custom-lists/{custom_list_id}/values")
        ]
        RemoteCustomListValue CustomProperty_AddCustomListValue(string project_template_id, string custom_list_id, RemoteCustomListValue remoteCustomListValue);

        [
        RestResource("Custom Property"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "project-templates/{project_template_id}/custom-properties?custom_list_id={custom_list_id}")
        ]
        RemoteCustomProperty CustomProperty_AddDefinition(string project_template_id, string custom_list_id, RemoteCustomProperty remoteCustomProperty);

        [
        RestResource("Custom Property"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "project-templates/{project_template_id}/custom-properties/{custom_property_id}")
        ]
        void CustomProperty_UpdateDefinition(string project_template_id, string custom_property_id, RemoteCustomProperty remoteCustomProperty);

        [
        RestResource("Custom Property"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "project-templates/{project_template_id}/custom-properties/{custom_property_id}")
        ]
        void CustomProperty_DeleteDefinition(string project_template_id, string custom_property_id);

        [
        RestResource("Custom List"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/custom-lists")
        ]
        List<RemoteCustomList> CustomProperty_RetrieveCustomLists(string project_template_id);

        [
        RestResource("Custom List"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "project-templates/{project_template_id}/custom-lists/{custom_list_id}")
        ]
        void CustomProperty_UpdateCustomList(string project_template_id, string custom_list_id, RemoteCustomList remoteCustomList);

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
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{release_id}/indent")
        ]
        void Release_Indent(string project_id, string release_id);

        [
        RestResource("Release"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{release_id}/outdent")
        ]
        void Release_Outdent(string project_id, string release_id);

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

        [
        RestResource("Release Status", "This resource lets you retrieve the list of release statuses in the system"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/releases/statuses")
        ]
        List<RemoteReleaseStatus> Release_RetrieveStatuses(string project_template_id);

        [
        RestResource("Release Type", "This resource lets you retrieve the list of release types in the system"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/releases/types")
        ]
        List<RemoteReleaseType> Release_RetrieveTypes(string project_template_id);

        [
        RestResource("Release Type"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/releases/types/{release_type_id}/workflow/transitions?status_id={release_status_id}&is_creator={is_creator}&isOwner={is_owner}")
        ]
        List<RemoteWorkflowTransition> Release_RetrieveWorkflowTransitions(string project_id, string release_type_id, string release_status_id, string is_creator, string is_owner);

        [
        RestResource("Release Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/releases/types/{release_type_id}/workflow/fields?status_id={release_status_id}")
        ]
        List<RemoteWorkflowField> Release_RetrieveWorkflowFields(string project_template_id, string release_type_id, string release_status_id);

        [
        RestResource("Release Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/releases/types/{release_type_id}/workflow/custom-properties?status_id={release_status_id}")
        ]
        List<RemoteWorkflowCustomProperty> Release_RetrieveWorkflowCustomProperties(string project_template_id, string release_type_id, string release_status_id);

        #endregion

        #region Project Template

        [
        RestResource("Project Template", "This resource allows you to retrieve, create, delete, and update project templates in the system"),
        OperationContract,
        WebGet(UriTemplate = "project-templates")
        ]
        List<RemoteProjectTemplate> ProjectTemplate_Retrieve();

        [
        RestResource("Project Template"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "project-templates?existing_project_template_id={existing_project_template_id}")
        ]
        RemoteProjectTemplate ProjectTemplate_Create(string existing_project_template_id, RemoteProjectTemplate remoteProjectTemplate);

        [
        RestResource("Project Template"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "project-templates/{project_template_id}")
        ]
        void ProjectTemplate_Delete(string project_template_id);

        [
        RestResource("Project Template"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "project-templates/{project_template_id}")
        ]
        void ProjectTemplate_Update(string project_template_id, RemoteProjectTemplate remoteProjectTemplate);

        [
        RestResource("Project Template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}")
        ]
        RemoteProjectTemplate ProjectTemplate_RetrieveById(string project_template_id);

        #endregion

        #region Project operations

        [
        RestResource("Project User", "This resource allows you to retrieve and modify the users associated with a specific project"),
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
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}")
        ]
        void Project_Update(string project_id, RemoteProject remoteProject);

        [
        RestResource("Project"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/refresh-caches?run_async={run_async}")
        ]
        void Project_RefreshProgressExecutionStatusCaches1(string project_id, string run_async);

        [
        RestResource("Project"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/refresh-caches/{release_id}?run_async={run_async}")
        ]
        void Project_RefreshProgressExecutionStatusCaches2(string project_id, string release_id, string run_async);

        [
        RestResource("Project User"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/users")
        ]
        void Project_AddUserMembership(string project_id, RemoteProjectUser remoteProjectUser);

        [
        RestResource("Project User"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/users")
        ]
        void Project_UpdateUserMembership(string project_id, RemoteProjectUser remoteProjectUser);

        [
        RestResource("Project User"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/users/{user_id}")
        ]
        void Project_RemoveUserMembership(string project_id, string user_id);

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
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/file")
        ]
        RemoteDocument Document_AddFile(string project_id, RemoteDocumentFile remoteDocument);

        [
        RestResource("Document"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/url")
        ]
        RemoteDocument Document_AddUrl(string project_id, RemoteDocument remoteDocument);

        [
        RestResource("Document Version", "This resource lets you add new versions of documents"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/{document_id}/versions/file?make_current={make_current}")
        ]
        RemoteDocumentVersion Document_AddFileVersion(string project_id, string document_id, string make_current, RemoteDocumentVersionFile remoteDocumentVersion);

        [
        RestResource("Document Version"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/documents/{document_id}/versions/url?make_current={make_current}")
        ]
        RemoteDocumentVersion Document_AddUrlVersion(string project_id, string document_id, string make_current, RemoteDocumentVersion remoteDocumentVersion);

		[
		RestResource("Document Version"),
		OperationContract,
		WebGet(UriTemplate = "projects/{project_id}/documents/versions/{attachment_version_id}/open")
		]
		byte[] Document_OpenVersion(string project_id, string attachment_version_id);

		[
		RestResource("Document Version"),
		OperationContract,
		WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/documents/versions/{attachment_version_id}")
		]
		void Document_DeleteVersion(string project_id, string attachment_version_id);

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
        List<RemoteDocument> Document_RetrieveForArtifact1(string project_id, string artifact_type_id, string artifact_id);

        [
        RestResource("Artifact Document"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents/search?sort_by={sort_by}")
        ]
        List<RemoteDocument> Document_RetrieveForArtifact2(string project_id, string artifact_type_id, string artifact_id, string sort_by, List<RemoteFilter> remoteFilters);

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
        WebGet(UriTemplate = "project-templates/{project_template_id}/document-types?active_only={active_only}")
        ]
        List<RemoteDocumentType> Document_RetrieveTypes(string project_template_id, string active_only);

		[
		RestResource("Document Type"),
		OperationContract,
		WebGet(UriTemplate = "project-templates/{project_template_id}/document-types/default")
		]
		RemoteDocumentType Document_RetrieveDefaultType(string project_template_id);

		[
		RestResource("Document Type"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "project-templates/{project_template_id}/document-types")
		]
		RemoteDocumentType Document_AddType(string project_template_id, RemoteDocumentType remoteDocumentType);

		[
		RestResource("Document Type"),
		OperationContract,
		WebInvoke(Method = "PUT", UriTemplate = "project-templates/{project_template_id}/document-types")
		]
		void Document_UpdateType(string project_template_id, RemoteDocumentType remoteDocumentType);

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
        WebGet(UriTemplate = "projects/{project_id}/document-folders/children?parent_folder_id={parent_folder_id}")
        ]
        List<RemoteDocumentFolder> Document_RetrieveFoldersByParentFolderId(string project_id, string parent_folder_id);

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
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/move?destination_requirement_id={destination_requirement_id}")
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

        [
        RestResource("Requirement Status", "This resource lets you retrieve the list of requirement statuses in the project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/requirements/statuses")
        ]
        List<RemoteRequirementStatus> Requirement_RetrieveStatuses(string project_template_id);

        [
        RestResource("Requirement Importance", "This resource lets you retrieve the list of requirement importances in the project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/requirements/importances")
        ]
        List<RemoteRequirementImportance> Requirement_RetrieveImportances(string project_template_id);

        [
        RestResource("Requirement Type", "This resource lets you retrieve the list of requirement types in the project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/requirements/types")
        ]
        List<RemoteRequirementType> Requirement_RetrieveTypes(string project_template_id);

        [
        RestResource("Requirement Type"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements/types/{requirement_type_id}/workflow/transitions?status_id={requirement_status_id}&is_creator={is_creator}&isOwner={is_owner}")
        ]
        List<RemoteWorkflowTransition> Requirement_RetrieveWorkflowTransitions(string project_id, string requirement_type_id, string requirement_status_id, string is_creator, string is_owner);

        [
        RestResource("Requirement Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/requirements/types/{requirement_type_id}/workflow/fields?status_id={requirement_status_id}")
        ]
        List<RemoteWorkflowField> Requirement_RetrieveWorkflowFields(string project_template_id, string requirement_type_id, string requirement_status_id);

        [
        RestResource("Requirement Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/requirements/types/{requirement_type_id}/workflow/custom-properties?status_id={requirement_status_id}")
        ]
        List<RemoteWorkflowCustomProperty> Requirement_RetrieveWorkflowCustomProperties(string project_template_id, string requirement_type_id, string requirement_status_id);

        [
        RestResource("Requirement Step", "This resource lets you create, modify and retrieve requirement steps"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements/{requirement_id}/steps")
        ]
        List<RemoteRequirementStep> Requirement_RetrieveSteps(string project_id, string requirement_id);

        [
        RestResource("Requirement Step"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements/{requirement_id}/steps/{requirement_step_id}")
        ]
        RemoteRequirementStep Requirement_RetrieveStepById(string project_id, string requirement_id, string requirement_step_id);

        [
        RestResource("Requirement Step"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/steps?existing_requirement_step_id={existing_requirement_step_id}&creator_id={creator_id}")
        ]
        RemoteRequirementStep Requirement_AddStep(string project_id, string requirement_id, string existing_requirement_step_id, string creator_id, RemoteRequirementStep remoteRequirementStep);

        [
        RestResource("Requirement Step"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/steps")
        ]
        void Requirement_UpdateStep(string project_id, string requirement_id, RemoteRequirementStep remoteRequirementStep);

        [
        RestResource("Requirement Step"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/steps/{source_requirement_step_id}/move?destination_requirement_step_id={destination_requirement_step_id}")
        ]
        void Requirement_MoveStep(string project_id, string requirement_id, string source_requirement_step_id, string destination_requirement_step_id);

        [
        RestResource("Requirement Step"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/requirements/{requirement_id}/steps/{requirement_step_id}")
        ]
        void Requirement_DeleteStep(string project_id, string requirement_id, string requirement_step_id);

        [
        RestResource("Requirement Test Step Coverage", "This resource lets you view, add and remove test steps from requirements."),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/requirements/test-steps")
        ]
        void Requirement_AddTestStepCoverage(string project_id, RemoteRequirementTestStepMapping remoteReqTestStepMapping);

        [
        RestResource("Requirement Test Step Coverage"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/requirements/{requirement_id}/test-steps")
        ]
        List<RemoteRequirementTestStepMapping> Requirement_RetrieveTestStepCoverage(string project_id, string requirement_id);

        [
        RestResource("Requirement Test Step Coverage"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/requirements/test-steps")
        ]
        void Requirement_RemoveTestStepCoverage(string project_id, RemoteRequirementTestStepMapping remoteReqTestStepMapping);

        #endregion

        #region Test Step

        [
        RestResource("Test Step Requirements Coverage"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-steps/{test_step_id}/requirements")
        ]
        List<RemoteRequirementTestStepMapping> TestStep_RetrieveRequirementCoverage(string project_id, string test_step_id);

        #endregion

        #region Test Case

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases")
        ]
        RemoteTestCase TestCase_Create(string project_id, RemoteTestCase remoteTestCase);

        [
        RestResource("Test Case"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/{test_case_id}")
        ]
        RemoteTestCase TestCase_RetrieveById(string project_id, string test_case_id);

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
        RestResource("Test Case Folder", "This resource allows you to retrieve, create, modify and delete test case folders in the project"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-folders")
        ]
        RemoteTestCaseFolder TestCase_CreateFolder(string project_id, RemoteTestCaseFolder remoteTestCaseFolder);

        [
        RestResource("Test Case Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-folders/{test_case_folder_id}")
        ]
        RemoteTestCaseFolder TestCase_RetrieveFolderById(string project_id, string test_case_folder_id);

        [
        RestResource("Test Case Folder"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-folders/{test_case_folder_id}")
        ]
        void TestCase_DeleteFolder(string project_id, string test_case_folder_id);

        [
        RestResource("Test Case Folder"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-folders")
        ]
        void TestCase_UpdateFolder(string project_id, RemoteTestCaseFolder remoteTestCaseFolder);

        [
        RestResource("Test Case Parameter", "This resource allows you to add/remove parameters to test cases"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/parameters")
        ]
        RemoteTestCaseParameter TestCase_AddParameter(string project_id, RemoteTestCaseParameter remoteTestCaseParameter);

		[
		RestResource("Test Case Parameter", "This resource allows you to update a test case parameter"),
		OperationContract,
		WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-cases/parameters")
		]
		void TestCase_UpdateParameter(string project_id, RemoteTestCaseParameter remoteTestCaseParameter);

		[
		RestResource("Test Case Parameter", "This resource allows you to delete a test case parameter"),
		OperationContract,
		WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-cases/parameters")
		]
		void TestCase_DeleteParameter(string project_id, RemoteTestCaseParameter remoteTestCaseParameter);

		[
        RestResource("Test Case Parameter"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/parameters/create-token?parameter_name={parameter_name}")
        ]
        string TestCase_CreateParameterToken(string project_id, string parameter_name);

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
		RestResource("Test Step Parameter", "This resource lets you add new parameters to a specific linked test step"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}/parameters")
		]
		void TestCase_AddStepParameters(string project_id, string test_case_id, string test_step_id, List<RemoteTestStepParameter> testStepParameters);

		[
		RestResource("Test Step Parameter", "This resource lets you update existing parameters on a specific linked test step"),
		OperationContract,
		WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}/parameters")
		]
		void TestCase_UpdateStepParameters(string project_id, string test_case_id, string test_step_id, List<RemoteTestStepParameter> testStepParameters);

		[
		RestResource("Test Step Parameter", "This resource lets you delete a parameter on a specific linked test step"),
		OperationContract,
		WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}/parameters")
		]
		void TestCase_DeleteStepParameters(string project_id, string test_case_id, string test_step_id, List<RemoteTestStepParameter> testStepParameters);

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
        RestResource("Test Step"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps")
        ]
        List<RemoteTestStep> TestCase_RetrieveSteps(string project_id, string test_case_id);

        [
        RestResource("Test Step"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}")
        ]
        RemoteTestStep TestCase_RetrieveStepById(string project_id, string test_case_id, string test_step_id);

        [
        RestResource("Test Step"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-steps")
        ]
        void TestCase_UpdateStep(string project_id, string test_case_id, RemoteTestStep remoteTestStep);

        [
        RestResource("Test Case", "This resource allows you to retrieve, create, modify and delete test cases in a specific project in the system."),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/count?release_id={release_id}")
        ]
        long TestCase_Count1(string project_id, string release_id);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/count?release_id={release_id}")
        ]
        long TestCase_Count2(string project_id, string release_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Case Folder Test Cases", "This resources lets you retrieve test cases and count test cases by their containing folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-folders/{test_folder_id}/count?release_id={release_id}")
        ]
        long TestCase_CountForFolder1(string project_id, string test_folder_id, string release_id);

        [
        RestResource("Test Case Folder Test Cases"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-folders/{test_folder_id}/count?release_id={release_id}")
        ]
        long TestCase_CountForFolder2(string project_id, string test_folder_id, string release_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Case"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}")
        ]
        List<RemoteTestCase> TestCase_Retrieve1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, string release_id);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}")
        ]
        List<RemoteTestCase> TestCase_Retrieve2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, string release_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Case Folder Test Cases"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-folders/{test_case_folder_id}/test-cases?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}")
         ]
        List<RemoteTestCase> TestCase_RetrieveByFolder1(string project_id, string test_case_folder_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, string release_id);

        [
        RestResource("Test Case Folder Test Cases"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-folders/{test_case_folder_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}")
        ]
        List<RemoteTestCase> TestCase_RetrieveByFolder2(string project_id, string test_case_folder_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, string release_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/move?test_case_folder_id={test_case_folder_id}")
        ]
        void TestCase_Move(string project_id, string test_case_id, string test_case_folder_id);

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
        RestResource("Test Case Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-folders")
        ]
        List<RemoteTestCaseFolder> TestCase_RetrieveFolders(string project_id);

        [
        RestResource("Test Case Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-folders/{parent_test_case_folder_id}/children?release_id={release_id}")
        ]
        List<RemoteTestCaseFolder> TestCase_RetrieveFoldersByParent(string project_id, string parent_test_case_folder_id, string release_id);

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

        [
        RestResource("Test Case Status", "This resource lets you retrieve the list of Test Case statuses in the system"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/test-cases/statuses")
        ]
        List<RemoteTestCaseStatus> TestCase_RetrieveStatuses(string project_template_id);

        [
        RestResource("Test Case Priority", "This resource lets you retrieve the list of Test Case priorities in the project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/test-cases/priorities")
        ]
        List<RemoteTestCasePriority> TestCase_RetrievePriorities(string project_template_id);

        [
        RestResource("Test Case Type", "This resource lets you retrieve the list of Test Case types in the project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/test-cases/types")
        ]
        List<RemoteTestCaseType> TestCase_RetrieveTypes(string project_template_id);

        [
        RestResource("Test Case Type"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-cases/types/{test_case_type_id}/workflow/transitions?status_id={test_case_status_id}&is_creator={is_creator}&isOwner={is_owner}")
        ]
        List<RemoteWorkflowTransition> TestCase_RetrieveWorkflowTransitions(string project_id, string test_case_type_id, string test_case_status_id, string is_creator, string is_owner);

        [
        RestResource("Test Case Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/test-cases/types/{test_case_type_id}/workflow/fields?status_id={test_case_status_id}")
        ]
        List<RemoteWorkflowField> TestCase_RetrieveWorkflowFields(string project_template_id, string test_case_type_id, string test_case_status_id);

        [
        RestResource("Test Case Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/test-cases/types/{test_case_type_id}/workflow/custom-properties?status_id={test_case_status_id}")
        ]
        List<RemoteWorkflowCustomProperty> TestCase_RetrieveWorkflowCustomProperties(string project_template_id, string test_case_type_id, string test_case_status_id);
         
        #endregion

        #region Test Run

        [
        RestResource("Manual Test Run", "This resource allows you to retrieve, create, modify and delete manual test runs in a specific project in the system."),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/create?release_id={release_id}")
        ]
        List<RemoteManualTestRun> TestRun_CreateFromTestCases(string project_id, string release_id, List<int> testCaseIds);

        [
        RestResource("Manual Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/create/test_set/{test_set_id}")
        ]
        List<RemoteManualTestRun> TestRun_CreateFromTestSet(string project_id, string test_set_id);

        [
        RestResource("Automated Test Run", "This resource allows you to retrieve, create, modify and delete automated test runs in a specific project in the system."),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/create/automation_host/{automation_host_token}")
        ]
        List<RemoteAutomatedTestRun> TestRun_CreateForAutomationHost(string project_id, string automation_host_token, DataObjects.DateRange dateRange);

        [
        RestResource("Automated Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/create/test_set/{test_set_id}/automation_host/{automation_host_token}")
        ]
        List<RemoteAutomatedTestRun> TestRun_CreateForAutomatedTestSet(string project_id, string test_set_id, string automation_host_token);

        [
        RestResource("Test Run", "This resource allows you to retrieve, create, modify and delete test runs in a specific project in the system."),
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
        RestResource("Automated Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/record")
        ]
        RemoteAutomatedTestRun TestRun_RecordAutomated1(string project_id, RemoteAutomatedTestRun remoteTestRun);

        [
        RestResource("Automated Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/record-multiple")
        ]
        List<RemoteAutomatedTestRun> TestRun_RecordAutomated2(string project_id, List<RemoteAutomatedTestRun> remoteTestRuns);

        [
        RestResource("Manual Test Run"),
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
        RestResource("Automated Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/{test_run_id}/automated")
        ]
        RemoteAutomatedTestRun TestRun_RetrieveAutomatedById(string project_id, string test_run_id);

        [
        RestResource("Manual Test Run"),
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
        RestResource("Manual Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/manual?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteManualTestRun> TestRun_RetrieveManual1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction);

        [
        RestResource("Manual Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/search/manual?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteManualTestRun> TestRun_RetrieveManual2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("Automated Test Run"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-runs/automated?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction);

        [
        RestResource("Automated Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-runs/search/automated?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteAutomatedTestRun> TestRun_RetrieveAutomated2(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-runs/{test_run_id}")
        ]
        void TestRun_Delete(string project_id, string test_run_id);

        [
        RestResource("Test Run"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-cases/{test_case_id}/test-runs/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteTestRun> TestRun_RetrieveByTestCaseId(string project_id, string test_case_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

		#endregion

		#region Risk

		[
		RestResource("Risk", "This resource allows you to retrieve, create, modify and delete risks in a specific project in the system."),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/risks")
		]
		RemoteRisk Risk_Create(string project_id, RemoteRisk remoteRisk);

		[
		RestResource("Risk"),
		OperationContract,
		WebGet(UriTemplate = "projects/{project_id}/risks/count")
		]
		long Risk_Count1(string project_id);

		[
		RestResource("Risk"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/risks/count")
		]
		long Risk_Count2(string project_id, List<RemoteFilter> remoteFilters);

		[
		RestResource("Risk"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/risks?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}")
		]
		List<RemoteRisk> Risk_Retrieve(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

		[
		RestResource("Risk"),
		OperationContract,
		WebGet(UriTemplate = "projects/{project_id}/risks/{risk_id}")
		]
		RemoteRisk Risk_RetrieveById(string project_id, string risk_id);

		[
		RestResource("Risk"),
		OperationContract,
		WebGet(UriTemplate = "risks")
		]
		List<RemoteRisk> Risk_RetrieveForOwner();

		[
		RestResource("Risk"),
		OperationContract,
		WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/risks")
		]
		void Risk_Update(string project_id, RemoteRisk remoteRisk);

		[
		RestResource("Risk"),
		OperationContract,
		WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/risks/{risk_id}")
		]
		void Risk_Delete(string project_id, string risk_id);

		[
		RestResource("Risk Comment", "This resource allows you to add and retrieve risk comments in the system"),
		OperationContract,
		WebGet(UriTemplate = "projects/{project_id}/risks/{risk_id}/comments")
		]
		List<RemoteComment> Risk_RetrieveComments(string project_id, string risk_id);

		[
		RestResource("Risk Comment"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/risks/{risk_id}/comments")
		]
		RemoteComment Risk_CreateComment(string project_id, string risk_id, RemoteComment remoteComment);

		[
		RestResource("Risk Status", "This resource lets you retrieve the list of Risk statuses in the system"),
		OperationContract,
		WebGet(UriTemplate = "project-templates/{project_template_id}/risks/statuses")
		]
		List<RemoteRiskStatus> Risk_RetrieveStatuses(string project_template_id);

		[
		RestResource("Risk Probability", "This resource lets you retrieve the list of risk probabilities in the project template"),
		OperationContract,
		WebGet(UriTemplate = "project-templates/{project_template_id}/risks/probabilities")
		]
		List<RemoteRiskProbability> Risk_RetrieveProbabilities(string project_template_id);

		[
		RestResource("Risk Impact", "This resource lets you retrieve the list of risk impacts in the project template"),
		OperationContract,
		WebGet(UriTemplate = "project-templates/{project_template_id}/risks/impacts")
		]
		List<RemoteRiskImpact> Risk_RetrieveImpacts(string project_template_id);

		[
		RestResource("Risk Type", "This resource lets you retrieve the list of Risk types in the project template"),
		OperationContract,
		WebGet(UriTemplate = "project-templates/{project_template_id}/risks/types")
		]
		List<RemoteRiskType> Risk_RetrieveTypes(string project_template_id);

		[
		RestResource("Risk Type"),
		OperationContract,
		WebGet(UriTemplate = "projects/{project_id}/risks/types/{risk_type_id}/workflow/transitions?status_id={risk_status_id}&is_creator={is_creator}&isOwner={is_owner}")
		]
		List<RemoteWorkflowTransition> Risk_RetrieveWorkflowTransitions(string project_id, string risk_type_id, string risk_status_id, string is_creator, string is_owner);

		[
		RestResource("Risk Type"),
		OperationContract,
		WebGet(UriTemplate = "project-templates/{project_template_id}/risks/types/{risk_type_id}/workflow/fields?status_id={risk_status_id}")
		]
		List<RemoteWorkflowField> Risk_RetrieveWorkflowFields(string project_template_id, string risk_type_id, string risk_status_id);

		[
		RestResource("Risk Type"),
		OperationContract,
		WebGet(UriTemplate = "project-templates/{project_template_id}/risks/types/{risk_type_id}/workflow/custom-properties?status_id={risk_status_id}")
		]
		List<RemoteWorkflowCustomProperty> Risk_RetrieveWorkflowCustomProperties(string project_template_id, string risk_type_id, string risk_status_id);

		[
		  RestResource("Risk Mitigation", "This resource lets you create, modify and retrieve risk mitigations"),
		  OperationContract,
		  WebGet(UriTemplate = "projects/{project_id}/risks/{risk_id}/mitigations")
		  ]
		List<RemoteRiskMitigation> Risk_RetrieveMitigations(string project_id, string risk_id);

		[
		RestResource("Risk Mitigation"),
		OperationContract,
		WebGet(UriTemplate = "projects/{project_id}/risks/{risk_id}/mitigations/{risk_mitigation_id}")
		]
		RemoteRiskMitigation Risk_RetrieveMitigationById(string project_id, string risk_id, string risk_mitigation_id);

		[
		RestResource("Risk Mitigation"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/risks/{risk_id}/mitigations?existing_risk_mitigation_id={existing_risk_mitigation_id}&creator_id={creator_id}")
		]
		RemoteRiskMitigation Risk_AddMitigation(string project_id, string risk_id, string existing_risk_mitigation_id, string creator_id, RemoteRiskMitigation remoteRiskMitigation);

		[
		RestResource("Risk Mitigation"),
		OperationContract,
		WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/risks/{risk_id}/mitigations")
		]
		void Risk_UpdateMitigation(string project_id, string risk_id, RemoteRiskMitigation remoteRiskMitigation);

		[
		RestResource("Risk Mitigation"),
		OperationContract,
		WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/risks/{risk_id}/mitigations/{risk_mitigation_id}")
		]
		void Risk_DeleteMitigation(string project_id, string risk_id, string risk_mitigation_id);

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
        RestResource("Task Comment"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/tasks/{task_id}/comments")
        ]
        RemoteComment Task_CreateComment(string project_id, string task_id, RemoteComment remoteComment);

        [
        RestResource("Task Status", "This resource lets you retrieve the list of Task statuses in the system"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/tasks/statuses")
        ]
        List<RemoteTaskStatus> Task_RetrieveStatuses(string project_template_id);

        [
        RestResource("Task Priority", "This resource lets you retrieve the list of task priorities in the project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/tasks/priorities")
        ]
        List<RemoteTaskPriority> Task_RetrievePriorities(string project_template_id);

        [
        RestResource("Task Type", "This resource lets you retrieve the list of Task types in the project template"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/tasks/types")
        ]
        List<RemoteTaskType> Task_RetrieveTypes(string project_template_id);

        [
        RestResource("Task Type"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/tasks/types/{task_type_id}/workflow/transitions?status_id={task_status_id}&is_creator={is_creator}&isOwner={is_owner}")
        ]
        List<RemoteWorkflowTransition> Task_RetrieveWorkflowTransitions(string project_id, string task_type_id, string task_status_id, string is_creator, string is_owner);

        [
        RestResource("Task Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/tasks/types/{task_type_id}/workflow/fields?status_id={task_status_id}")
        ]
        List<RemoteWorkflowField> Task_RetrieveWorkflowFields(string project_template_id, string task_type_id, string task_status_id);

        [
        RestResource("Task Type"),
        OperationContract,
        WebGet(UriTemplate = "project-templates/{project_template_id}/tasks/types/{task_type_id}/workflow/custom-properties?status_id={task_status_id}")
        ]
        List<RemoteWorkflowCustomProperty> Task_RetrieveWorkflowCustomProperties(string project_template_id, string task_type_id, string task_status_id);

        [
        RestResource("Task Folder", "This resource allows you to retrieve, create, modify and delete task folders in the project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/task-folders")
        ]
        List<RemoteTaskFolder> Task_RetrieveFolders(string project_id);

        [
        RestResource("Task Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/task-folders/{parent_task_folder_id}/children")
        ]
        List<RemoteTaskFolder> Task_RetrieveFoldersByParent(string project_id, string parent_task_folder_id);

        [
        RestResource("Task Folder"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/task-folders")
        ]
        RemoteTaskFolder Task_CreateFolder(string project_id, RemoteTaskFolder remoteTaskFolder);

        [
        RestResource("Task Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/task-folders/{task_folder_id}")
        ]
        RemoteTaskFolder Task_RetrieveFolderById(string project_id, string task_folder_id);

        [
        RestResource("Task Folder"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/task-folders/{task_folder_id}")
        ]
        void Task_DeleteFolder(string project_id, string task_folder_id);

        [
        RestResource("Task Folder"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/task-folders")
        ]
        void Task_UpdateFolder(string project_id, RemoteTaskFolder remoteTaskFolder);

        #endregion

        #region Test Set

        [
        RestResource("Test Set Test Case"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-case-mapping/{test_case_id}?owner_id={owner_id}&existing_test_set_test_case_id={existing_test_set_test_case_id}")
        ]
        List<RemoteTestSetTestCaseMapping> TestSet_AddTestMapping(string project_id, string test_set_id, string test_case_id, string owner_id, string existing_test_set_test_case_id, List<RemoteTestSetTestCaseParameter> parameters);

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
        RestResource("Test Set", "This resource lets you create, view, edit and delete test sets in the system"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets")
        ]
        RemoteTestSet TestSet_Create(string project_id, RemoteTestSet remoteTestSet);

        [
        RestResource("Test Set Folder", "This resource lets you create, view, edit and delete test set folders in the system"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-set-folders")
        ]
        RemoteTestSetFolder TestSet_CreateFolder(string project_id, RemoteTestSetFolder remoteTestSetFolder);

        [
        RestResource("Test Set"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}")
        ]
        RemoteTestSet TestSet_RetrieveById(string project_id, string test_set_id);

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
         RestResource("Test Set Folder"),
         OperationContract,
         WebGet(UriTemplate = "projects/{project_id}/test-set-folders/{test_set_folder_id}")
         ]
        RemoteTestSetFolder TestSet_RetrieveFolderById(string project_id, string test_set_folder_id);

        [
        RestResource("Test Set Folder"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-set-folders/{test_set_folder_id}")
        ]
        void TestSet_DeleteFolder(string project_id, string test_set_folder_id);

        [
        RestResource("Test Set Folder"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-set-folders")
        ]
        void TestSet_UpdateFolder(string project_id, RemoteTestSetFolder remoteTestSetFolder);

        [
        RestResource("Test Set"),
        OperationContract,
        WebGet(UriTemplate = "test-sets")
        ]
        List<RemoteTestSet> TestSet_RetrieveForOwner();

        [
        RestResource("Test Set"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/move?test_set_folder_id={test_set_folder_id}")
        ]
        void TestSet_Move(string project_id, string test_set_id, string test_set_folder_id);

        [
         RestResource("Test Set", "This resource allows you to retrieve, create, modify and delete test sets in a specific project in the system."),
         OperationContract,
         WebGet(UriTemplate = "projects/{project_id}/test-sets/count?release_id={release_id}")
         ]
        long TestSet_Count1(string project_id, string release_id);

        [
        RestResource("Test Set"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/count?release_id={release_id}")
        ]
        long TestSet_Count2(string project_id, string release_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Set Folder Test Sets", "This resources lets you retrieve test sets and count test sets by their containing folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-set-folders/{test_folder_id}/count?release_id={release_id}")
        ]
        long TestSet_CountForFolder1(string project_id, string test_folder_id, string release_id);

        [
        RestResource("Test Set Folder Test Sets"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-set-folders/{test_folder_id}/count?release_id={release_id}")
        ]
        long TestSet_CountForFolder2(string project_id, string test_folder_id, string release_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Set"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}")
        ]
        List<RemoteTestSet> TestSet_Retrieve1(string project_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, string release_id);

        [
        RestResource("Test Set"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/search?starting_row={starting_row}&number_of_rows={number_of_rows}&release_id={release_id}&sort_field={sort_field}&sort_direction={sort_direction}")
        ]
        List<RemoteTestSet> TestSet_Retrieve2(string project_id, string starting_row, string number_of_rows, string release_id, string sort_field, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Set Folder Test Sets"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-set-folders/{test_set_folder_id}/test-sets?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}")
         ]
        List<RemoteTestSet> TestSet_RetrieveByFolder1(string project_id, string test_set_folder_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, string release_id);

        [
        RestResource("Test Set Folder Test Sets"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-set-folders/{test_set_folder_id}/test-sets/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}")
        ]
        List<RemoteTestSet> TestSet_RetrieveByFolder2(string project_id, string test_set_folder_id, string starting_row, string number_of_rows, string sort_field, string sort_direction, string release_id, List<RemoteFilter> remoteFilters);

        [
        RestResource("Test Set Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-set-folders")
        ]
        List<RemoteTestSetFolder> TestSet_RetrieveFolders(string project_id);

        [
        RestResource("Test Set Folder"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-set-folders/{parent_test_set_folder_id}/children?release_id={release_id}")
        ]
        List<RemoteTestSetFolder> TestSet_RetrieveFoldersByParent(string project_id, string parent_test_set_folder_id, string release_id);

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

        [
        RestResource("Test Set Parameter", "This resource lets you the retrieve test parameter values associated with test sets"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/parameters")
        ]
        List<RemoteTestSetParameter> TestSet_RetrieveParameters(string project_id, string test_set_id);

		[
		RestResource("Test Set Parameter", "This resource allows you to add/remove parameters to test sets"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/parameters")
		]
		void TestSet_AddParameter(string project_id, RemoteTestSetParameter remoteTestSetParameter);

		[
		RestResource("Test Set Parameter", "This resource allows you to update a test set parameter"),
		OperationContract,
		WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-sets/parameters")
		]
		void TestSet_UpdateParameter(string project_id, RemoteTestSetParameter remoteTestSetParameter);

		[
		RestResource("Test Set Parameter", "This resource allows you to delete a test set parameter"),
		OperationContract,
		WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-sets/parameters")
		]
		void TestSet_DeleteParameter(string project_id, RemoteTestSetParameter remoteTestSetParameter);

		[
        RestResource("Test Set Test Case Parameter", "This resource lets you retrieve the test parameter values associated with the test cases inside a test set"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-cases/{test_set_test_case_id}/parameters")
        ]
        List<RemoteTestSetTestCaseParameter> TestSet_RetrieveTestCaseParameters(string project_id, string test_set_id, string test_set_test_case_id);

		[
		RestResource("Test Set Test Case Parameter", "This resource allows you to add/remove parameters to a test case in a test set"),
		OperationContract,
		WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-cases/parameters")
		]
		void TestSet_AddTestCaseParameter(string project_id, string test_set_id, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter);

		[
		RestResource("Test Set Test Case Parameter", "This resource allows you to update a parameter of a test case in a test set"),
		OperationContract,
		WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-cases/parameters")
		]
		void TestSet_UpdateTestCaseParameter(string project_id, string test_set_id, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter);

		[
		RestResource("Test Set Test Case Parameter", "This resource allows you to delete a parameter of a test case in a test set"),
		OperationContract,
		WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-cases/parameters")
		]
		void TestSet_DeleteTestCaseParameter(string project_id, string test_set_id, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter);

		[
        RestResource("Test Set Test Case"),
        OperationContract,
        WebInvoke(Method="PUT", UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-cases/{test_set_test_case_id}/in-use")
        ]
        void TestSet_SetInUseStatus(string project_id, string test_set_id, string test_set_test_case_id, string is_in_use);

        [
        RestResource("Test Set Test Case"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-cases/{test_set_test_case_id}/in-use")
        ]
        bool TestSet_CheckInUseStatus(string project_id, string test_set_id, string test_set_test_case_id);

        #endregion

        #region Test Configurations

        [
        RestResource("Test Set"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-sets/{test_set_id}/test-configuration-sets")
        ]
        RemoteTestConfigurationSet TestConfiguration_RetrieveForTestSet(string project_id, string test_set_id);

        [
        RestResource("Test Configuration Set", "This resource lets you retrieve test configuration sets"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-configuration-sets/{test_configuration_set_id}")
        ]
        RemoteTestConfigurationSet TestConfiguration_RetrieveSetById(string project_id, string test_configuration_set_id);

        [
        RestResource("Test Configuration Set"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/test-configuration-sets")
        ]
        List<RemoteTestConfigurationSet> TestConfiguration_RetrieveSets(string project_id);

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
        WebGet(UriTemplate = "users/usernames/{user_name}?include_inactive={include_inactive}")
        ]
        RemoteUser User_RetrieveByUserName(string user_name, string include_inactive);

        [
        RestResource("User"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "users/{user_id}")
        ]
        void User_Delete(string user_id);

        [
        RestResource("User"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "users/{user_id}")
        ]
        void User_Update(string user_id, RemoteUser remoteUser);

        [
        RestResource("User"),
        OperationContract,
        WebGet(UriTemplate = "users/all")
        ]
        List<RemoteUser> User_RetrieveAll();

        [
        RestResource("User Contact", "This resource lets you view a list of contacts, add a contact and remove one"),
        OperationContract,
        WebGet(UriTemplate = "users/contacts")
        ]
        List<RemoteUser> User_RetrieveContacts();

        [
        RestResource("User Contact"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "users/contacts/{user_id}")
        ]
        void User_AddContact(string user_id);

        [
        RestResource("User Contact"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "users/contacts/{user_id}")
        ]
        void User_RemoveContact(string user_id);


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

        [
        RestResource("Automation Engine"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "automation-engines")
        ]
        void AutomationEngine_Update(RemoteAutomationEngine remoteEngine);

        #endregion

        #region Component

        [
        RestResource("Component", "This resource lets you create, edit, delete and view components in a project"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/components?active_only={active_only}&include_deleted={include_deleted}")
        ]
        List<RemoteComponent> Component_Retrieve(string project_id, string active_only, string include_deleted);

        [
        RestResource("Component"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/components/{component_id}")
        ]
        RemoteComponent Component_RetrieveById(string project_id, string component_id);

        [
        RestResource("Component"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/components")
        ]
        RemoteComponent Component_Create(string project_id, RemoteComponent remoteComponent);

        [
        RestResource("Component"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "projects/{project_id}/components/{component_id}")
        ]
        void Component_Update(string project_id, string component_id, RemoteComponent remoteComponent);

        [
        RestResource("Component"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/components/{component_id}")
        ]
        void Component_Delete(string project_id, string component_id);

        [
        RestResource("Component"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/components/{component_id}/undelete")
        ]
        void Component_Undelete(string project_id, string component_id);

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
		WebGet(UriTemplate = "projects/{project_id}/releases/{release_id}/builds/{build_id}/no_description")
		]
		RemoteBuild Build_RetrieveById_NoDescription(string project_id, string release_id, string build_id);

		[
        RestResource("Release Build"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/releases/{release_id}/builds")
        ]
        List<RemoteBuild> Build_RetrieveByReleaseId1(string project_id, string release_id);

        [
        RestResource("Release Build"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/releases/{release_id}/builds?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_by={sort_by}")
        ]
        List<RemoteBuild> Build_RetrieveByReleaseId2(string project_id, string release_id, string starting_row, string number_of_rows, string sort_by, List<RemoteFilter> remoteFilters);

		[
		RestResource("Release Build"),
		OperationContract,
		WebGet(UriTemplate = "projects/{project_id}/releases/{release_id}/builds/no_description")
		]
		List<RemoteBuild> Build_RetrieveByReleaseId_NoDescription(string project_id, string release_id);

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
        List<RemoteAssociation> Association_RetrieveForArtifact1(string project_id, string artifact_type_id, string artifact_id);

        [
        RestResource("Association"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/associations/{artifact_type_id}/{artifact_id}/search?sort_by={sort_by}")
        ]
        List<RemoteAssociation> Association_RetrieveForArtifact2(string project_id, string artifact_type_id, string artifact_id, string sort_by, List<RemoteFilter> remoteFilters);

		[
		RestResource("Association"),
		OperationContract,
		WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/associations/{artifact_link_id}")
		]
		void Association_Delete(string project_id, string artifact_link_id);

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
        WebInvoke(Method = "POST", UriTemplate = "data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}/search")
        ]
        List<RemoteProjectArtifact> DataMapping_SearchArtifactMappings(string data_sync_system_id, string artifact_type_id, string externalKey);

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
        WebGet(UriTemplate = "data-syncs/{data_sync_system_id}")
        ]
        RemoteDataSyncSystem DataSyncSystem_RetrieveById(string data_sync_system_id);

        [
        RestResource("Data Sync"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "data-syncs")
        ]
        RemoteDataSyncSystem DataSyncSystem_Create(RemoteDataSyncSystem remoteDataSyncSystem);

        [
        RestResource("Data Sync"),
        OperationContract,
        WebInvoke(Method = "PUT", UriTemplate = "data-syncs/{data_sync_system_id}")
        ]
        void DataSyncSystem_Update(string data_sync_system_id, RemoteDataSyncSystem remoteDataSyncSystem);

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

		#region Source Code

		[
        RestResource("Source Code", "This resource lets you view changes and code in linked source code repositories"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code")
        ]
        List<RemoteSourceCodeBranch> SourceCode_RetrieveBranches(string project_id);

        [
        RestResource("Source Code Folder", "This resource lets you view the source code folders"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/folders?parent_folder_id={parent_folder_id}")
        ]
        List<RemoteSourceCodeFolder> SourceCode_RetrieveFoldersByParent(string project_id, string branch_id, string parent_folder_id);

        [
        RestResource("Source Code File", "This resource lets you view the source code files"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/folders/{folder_id}/files")
        ]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesByFolder(string project_id, string branch_id, string folder_id);

        [
        RestResource("Source Code File"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/revisions/{revision_id}/files")
        ]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesByRevision(string project_id, string branch_id, string revision_id);

        [
        RestResource("Source Code File"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/files/{file_id}")
        ]
        RemoteSourceCodeFile SourceCode_RetrieveFileById(string project_id, string branch_id, string file_id);

        [
        RestResource("Source Code File"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/files?artifact_type_id={artifact_type_id}&artifact_id={artifact_id}")
        ]
        List<RemoteSourceCodeFile> SourceCode_RetrieveFilesForArtifact(string project_id, string branch_id, string artifact_type_id, string artifact_id);

        [
        RestResource("Source Code File"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/files/{file_id}/open?revision_id={revision_id}")
        ]
        byte[] SourceCode_OpenFileById(string project_id, string branch_id, string file_id, string revision_id);

        [
        RestResource("Source Code Revision", "This resource lets you view the revisions in linked source code repositories"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/source-code/{branch_id}/revisions?start_Row={start_Row}&number_rows={number_rows}&sort_property={sort_property}&sort_direction={sort_direction}")
        ]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisions(string project_id, string branch_id, string start_Row, string number_rows, string sort_property, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("Source Code Revision"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/files/{file_id}/revisions")
        ]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisionsForFile(string project_id, string branch_id, string file_id);

        [
        RestResource("Source Code Revision"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/revisions/{revision_id}")
        ]
        RemoteSourceCodeRevision SourceCode_RetrieveRevisionById(string project_id, string branch_id, string revision_id);

        [
        RestResource("Source Code Revision"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/revisions?artifact_type_id={artifact_type_id}&artifact_id={artifact_id}")
        ]
        List<RemoteSourceCodeRevision> SourceCode_RetrieveRevisionsForArtifact(string project_id, string branch_id, string artifact_type_id, string artifact_id);

        [
        RestResource("Source Code Revision"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/source-code/{branch_id}/revisions/{revision_id}/associations")
        ]
        List<RemoteLinkedArtifact> SourceCode_RetrieveArtifactsForRevision(string project_id, string branch_id, string revision_id);

		[
		RestResource("Source Code"),
		OperationContract,
		WebGet(UriTemplate = "projects/{project_id}/source-code/connection")
		]
		RemoteSourceCodeConnection SourceCode_RetrieveConnectionInformation(string project_id);

		#endregion

		#region Instant Messenger

		[
		RestResource("Message", "This resource lets you receive and send instant messages"),
        OperationContract,
        WebGet(UriTemplate = "messages")
        ]
        RemoteMessageInfo Message_GetInfo();

        [
        RestResource("Message"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "messages")
        ]
        long Message_PostNew(RemoteMessageIndividual remoteMessage);

        [
        RestResource("Message"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "messages/senders/{sender_user_id}")
        ]
        void Message_MarkAllAsRead(string sender_user_id);

        [
        RestResource("Message"),
        OperationContract,
        WebGet(UriTemplate = "messages/senders")
        ]
        List<RemoteUserMessage> Message_GetUnreadMessageSenders();

        [
        RestResource("Message"),
        OperationContract,
        WebGet(UriTemplate = "messages/unread")
        ]
        List<RemoteMessage> Message_RetrieveUnread();

        #endregion

        #region History

        [
        RestResource("History"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/history?start_row={start_row}&number_rows={number_rows}&sort_property={sort_property}&sort_direction={sort_direction}")
        ]
        List<RemoteHistoryChange> History_RetrieveForArtifact1(string project_id, string artifact_type_id, string artifact_id, string start_row, string number_rows, string sort_property, string sort_direction);

        [
        RestResource("History"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/history?start_row={start_row}&number_rows={number_rows}&sort_property={sort_property}&sort_direction={sort_direction}")
        ]
        List<RemoteHistoryChange> History_RetrieveForArtifact2(string project_id, string artifact_type_id, string artifact_id, string start_row, string number_rows, string sort_property, string sort_direction, List<RemoteFilter> remoteFilters);

        [
        RestResource("History", "This resource lets you view the history of changes made to artifacts"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/history/{history_change_set_id}")
        ]
        RemoteHistoryChangeSet History_RetrieveById(string project_id, string history_change_set_id);

        #endregion

        #region Subscriptions

        [
        RestResource("Subscriptions"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/subscriptions")
        ]
        void Subscription_SubscribeToArtifact(string project_id, string artifact_type_id, string artifact_id);

        [
        RestResource("Subscriptions"),
        OperationContract,
        WebInvoke(Method = "DELETE", UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/subscriptions")
        ]
        void Subscription_UnsubscribeFromArtifact(string project_id, string artifact_type_id, string artifact_id);

        [
        RestResource("Subscriptions"),
        OperationContract,
        WebGet(UriTemplate = "subscriptions")
        ]
        List<RemoteArtifactSubscription> Subscription_RetrieveForUser();

        [
        RestResource("Subscriptions", "This resource lets you view subscribed items as well as subscribe/unsubscribe to artifacts"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/subscriptions")
        ]
        List<RemoteArtifactSubscription> Subscription_RetrieveForArtifact(string project_id, string artifact_type_id, string artifact_id);

        #endregion

        #region Saved Filters

        [
        RestResource("Saved Filters", "This resource lets you retrieve the saved filters for a user"),
        OperationContract,
        WebGet(UriTemplate = "saved-filters")
        ]
        List<RemoteSavedFilter> SavedFilter_RetrieveForUser();

        #endregion

        #region Reports

        [
        RestResource("Graphs", "This resource lets you retrieve custom graph data and images"),
        OperationContract,
        WebGet(UriTemplate = "graphs/{custom_graph_id}/data?project_id={project_id}&project_group_id={project_group_id}")
        ]
        RemoteTableData Reports_RetrieveCustomGraphData(string custom_graph_id, string project_id, string project_group_id);

        [
        RestResource("Custom Query", "This resource lets you create custom ESQL queries and return data"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "query")
        ]
        RemoteTableData Reports_RetrieveESQLQueryData(string esql_query);

        [
        RestResource("Reports: Saved", "This resource lets you retrieve and generate a saved report"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/reports/saved?include_shared={include_shared}")
        ]
        List<RemoteSavedReport> Reports_RetrieveSaved(string project_id, string include_shared);

        [
        RestResource("Reports: Saved"),
        OperationContract,
        WebInvoke(Method = "POST", UriTemplate = "projects/{project_id}/reports/saved/{saved_report_id}/generate")
        ]
        string Reports_GenerateSavedReport(string project_id, string saved_report_id);

        [
        RestResource("Reports: Generated", "This resource lets you check the status and retrieve a generated reports"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/reports/generated?report_generation_id={report_generation_id}")
        ]
        int Reports_CheckGeneratedReportStatus(string project_id, string report_generation_id);

        [
        RestResource("Reports: Generated"),
        OperationContract,
        WebGet(UriTemplate = "projects/{project_id}/reports/generated/{generated_report_id}")
        ]
        byte[] Reports_RetrieveGeneratedReport(string project_id, string generated_report_id);

        #endregion
    }
}
