using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.App_Models;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// Contains any global static functions and constants that need to be accessed from any web page,
	/// web form, server control or user control inside the Web assembly
	/// </summary>
	public static class GlobalFunctions
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.GlobalFunctions";

		#region Constants
		//Global Display constants (displays as DISPLAY_SOFTWARE_VERSION.DISPLAY_SOFTWARE_VERSION_BUILD)
		public const string DISPLAY_SOFTWARE_VERSION = Common.Global.VERSION_STRING_NO_BUILD;
		public const int DISPLAY_SOFTWARE_VERSION_BUILD = Common.Global.VERSION_NUMBER_BUILD;
		public const string SYSTEM_VERSION_NUMBER = Common.Global.VERSION_STRING_SYSTEM;   //Used to version URLs, etc.

        //XSRF Constants
        public const string AntiXsrfHeader = "X-AntiXsrfToken";
        public const string AntiXsrfTokenKey = "__AntiXsrfToken";
        public const string AntiXsrfUserNameKey = "__AntiXsrfUserName";

        //Artifact ID prefixes
        public const string ARTIFACT_PREFIX_INCIDENT = "IN";
		public const string ARTIFACT_PREFIX_REQUIREMENT = "RQ";
		public const string ARTIFACT_PREFIX_TEST_CASE = "TC";
		public const string ARTIFACT_PREFIX_TEST_STEP = "TS";
		public const string ARTIFACT_PREFIX_TEST_SET = "TX";
		public const string ARTIFACT_PREFIX_TEST_RUN = "TR";
		public const string ARTIFACT_PREFIX_TEST_RUN_STEP = "RS";
		public const string ARTIFACT_PREFIX_RELEASE = "RL";
		public const string ARTIFACT_PREFIX_PROJECT = "PR";
		public const string ARTIFACT_PREFIX_USER = "US";
		public const string ARTIFACT_PREFIX_INCIDENT_TYPE = "IT";
		public const string ARTIFACT_PREFIX_INCIDENT_STATUS = "IS";
		public const string ARTIFACT_PREFIX_INCIDENT_PRIORITY = "IP";
		public const string ARTIFACT_PREFIX_INCIDENT_SEVERITY = "IV";
		public const string ARTIFACT_PREFIX_WORKFLOW = "WK";
		public const string ARTIFACT_PREFIX_WORKFLOW_TRANSITION = "WT";
		public const string ARTIFACT_PREFIX_PROJECT_ROLE = "RX";
		public const string ARTIFACT_PREFIX_TASK = "TK";
		public const string ARTIFACT_PREFIX_DOCUMENT = "DC";
		public const string ARTIFACT_PREFIX_PROJECT_GROUP = "PG";
		public const string ARTIFACT_PREFIX_PORTFOLIO = "PF";
		public const string ARTIFACT_PREFIX_DOCUMENT_TYPE = "DT";
		public const string ARTIFACT_PREFIX_DOCUMENT_FOLDER = "DF";
		public const string ARTIFACT_PREFIX_AUTOMATION_HOST = "AH";
		public const string ARTIFACT_PREFIX_HISTORY_RECORD = "";
		public const string ARTIFACT_PREFIX_TEST_CONFIGURATION_SET = "TG";
		public const string ARTIFACT_PREFIX_RISK = "RK";
        public const string ARTIFACT_PREFIX_BUILD = "BL";

        //Image icons common to multiple pages
        public const string URL_IMAGE_EXPANDED_Y = "Images/ExpandedY.gif";
		public const string URL_IMAGE_EXPANDED_N = "Images/ExpandedN.gif";
		public const string URL_IMAGE_EXPANDED_NA = "Images/ExpandedX.gif";
		public const string URL_IMAGE_TEST_CASE = "Images/artifact-TestCase.svg";
		public const string URL_IMAGE_TEST_CASE_FOLDER = "Images/Folder.svg";
		public const string URL_IMAGE_TEST_CASE_FOLDER_EXPAND = "Images/FolderExpanded.svg";
		public const string URL_IMAGE_TEST_CASE_NO_STEPS = "Images/artifact-TestCaseNoSteps.svg";
		public const string URL_IMAGE_REQUIREMENT = "Images/artifact-Requirement.svg";
		public const string URL_IMAGE_REQUIREMENT_SUMMARY = "Images/artifact-RequirementSummary.svg";
		public const string URL_IMAGE_RELEASE = "Images/artifact-Release.svg";
		public const string URL_IMAGE_ITERATION = "Images/artifact-Iteration.svg";

		//Project Settings Collections

		//Sorts
		public const string PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION = "IncidentSortExpression";
		public const string PROJECT_SETTINGS_TASK_SORT_EXPRESSION = "TaskSortExpression";
		public const string PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION = "TestRunList.SortExpression";
		public const string PROJECT_SETTINGS_ARTIFACT_LINKS_SORT_EXPRESSION = "ArtifactLinks.SortExpression";
		public const string PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION = "AutomationHosts.SortExpression";
		public const string PROJECT_SETTINGS_HISTORY_SORT_EXPRESSION = "History.SortExpression";
		public const string PROJECT_SETTINGS_HISTORY_ADMINSETSORT_EXPRESSION = "History.AdminSetSortExpression";
		public const string PROJECT_SETTINGS_RESOURCES_ACTIONSORT_EXPRESSION = "Resources.ActionsSortExpression";
		public const string PROJECT_SETTINGS_ASSOCIATIONS_ARTIFACT_SORT_EXPRESSION = "AssociationsArtifact.SortExpression";
		public const string PROJECT_SETTINGS_ASSOCIATIONS_TEST_CASE_COVERAGE_SORT_EXPRESSION = "AssociationsTestCaseCoverage.SortExpression";
		public const string PROJECT_SETTINGS_ASSOCIATIONS_REQUIREMENT_COVERAGE_SORT_EXPRESSION = "AssociationsRequirementCoverage.SortExpression";
		public const string PROJECT_SETTINGS_ASSOCIATIONS_RELEASE_COVERAGE_SORT_EXPRESSION = "AssociationsReleaseCoverage.SortExpression";
		public const string PROJECT_SETTINGS_BASELINE_SORT_EXPRESSION = "Baseline.SortExpression";
		public const string PROJECT_SETTINGS_BASELINEADMIN_SORT_EXPRESSION = "Admin.Baseline.SortExpression";
		public const string PROJECT_SETTINGS_BASELINEARTIFACT_SORT_EXPRESSION = "Artifact.Baseline.SortExpression";

		//Filters
		public const string PROJECT_SETTINGS_INCIDENT_FILTERS_LIST = "IncidentFiltersList";
		public const string PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST = "RequirementFiltersList";
		public const string PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST = "TestCaseFiltersList";
		public const string PROJECT_SETTINGS_RELEASE_FILTERS_LIST = "ReleaseFiltersList";
		public const string PROJECT_SETTINGS_TEST_SET_FILTERS_LIST = "TestSetFiltersList";
		public const string PROJECT_SETTINGS_TASK_FILTERS_LIST = "TaskFiltersList";
		public const string PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST = "DocumentList.Filters";
		public const string PROJECT_SETTINGS_RESOURCES_FILTERS_LIST = "Resources.Filters";
		public const string PROJECT_SETTINGS_PROJECT_HOME_SETTINGS = "ProjectHome.GeneralSettings";
		public const string PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST = "TestRunList.Filters";
		public const string PROJECT_SETTINGS_ARTIFACT_LINKS_FILTERS_LIST = "ArtifactLinks.Filters";
		public const string PROJECT_SETTINGS_ASSOCIATIONS_ARTIFACT_FILTERS_LIST = "AssociationsArtifact.Filters";
		public const string PROJECT_SETTINGS_ASSOCIATIONS_TEST_CASE_COVERAGE_FILTERS_LIST = "AssociationsTestCaseCoverage.Filters";
		public const string PROJECT_SETTINGS_ASSOCIATIONS_REQUIREMENT_COVERAGE_FILTERS_LIST = "AssociationsRequirementCoverage.Filters";
		public const string PROJECT_SETTINGS_ASSOCIATIONS_RELEASE_COVERAGE_FILTERS_LIST = "AssociationsReleaseCoverage.Filters";
		public const string PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST = "AutomationHosts.Filters";
		public const string PROJECT_SETTINGS_REQUIREMENTS_TASKS_FILTERS_LIST = "RequirementsTaskList.Filters";
		public const string PROJECT_SETTINGS_HISTORY_FILTERS_LIST = "History.Filters";
		public const string PROJECT_SETTINGS_RESOURCES_ACTIONFILTERS_LIST = "Resources.ActionFilters";
		public const string PROJECT_SETTINGS_HISTORY_ADMINFILTERS_LIST = "History.AdminSetFilters";
		public const string PROJECT_SETTINGS_BUILD_FILTERS_LIST = "Build.Filters";
		public const string PROJECT_SETTINGS_REQUIREMENT_STEP_FILTERS = "RequirementStep.Filters";
		public const string PROJECT_SETTINGS_REQUIREMENT_DETAILS_TASKS_FILTERS = "RequirementDetails.Tasks.Filters";
		public const string PROJECT_SETTINGS_REQUIREMENT_DETAILS_TASKS_GENERAL = "RequirementDetails.Tasks.General";
		public const string PROJECT_SETTINGS_RISK_DETAILS_TASKS_FILTERS = "RiskDetails.Tasks.Filters";
		public const string PROJECT_SETTINGS_RISK_DETAILS_TASKS_GENERAL = "RiskDetails.Tasks.General";
		public const string PROJECT_SETTINGS_TEST_CASE_DETAILS_TEST_RUNS_FILTERS = "TestCaseDetails.TestRuns.Filters";
		public const string PROJECT_SETTINGS_TEST_CASE_DETAILS_TEST_RUNS_GENERAL = "TestCaseDetails.TestRuns.General";
		public const string PROJECT_SETTINGS_TEST_SET_DETAILS_TEST_RUNS_FILTERS = "TestSetDetails.TestRuns.Filters";
		public const string PROJECT_SETTINGS_TEST_SET_DETAILS_TEST_RUNS_GENERAL = "TestSetDetails.TestRuns.General";
		public const string PROJECT_SETTINGS_TEST_CASE_DETAILS_INCIDENTS_FILTERS = "TestCaseDetails.Incidents.Filters";
		public const string PROJECT_SETTINGS_TEST_CASE_DETAILS_INCIDENTS_GENERAL = "TestCaseDetails.Incidents.General";
		public const string PROJECT_SETTINGS_TEST_SET_DETAILS_INCIDENTS_FILTERS = "TestSetDetails.Incidents.Filters";
		public const string PROJECT_SETTINGS_TEST_SET_DETAILS_INCIDENTS_GENERAL = "TestSetDetails.Incidents.General";
		public const string PROJECT_SETTINGS_TEST_RUN_DETAILS_INCIDENTS_FILTERS = "TestRunDetails.Incidents.Filters";
		public const string PROJECT_SETTINGS_TEST_RUN_DETAILS_INCIDENTS_GENERAL = "TestRunDetails.Incidents.General";
		public const string PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_FILTERS = "TestConfigurationSets.Filters";
		public const string PROJECT_SETTINGS_TEST_CONFIGURATION_SETS_GENERAL = "TestConfigurationSets.General";
		public const string PROJECT_SETTINGS_TEST_CONFIGURATION_FILTERS = "TestConfigurations.Filters";
		public const string PROJECT_SETTINGS_TEST_CONFIGURATION_GENERAL = "TestConfigurations.General";
		public const string PROJECT_SETTINGS_RELEASE_DETAILS_TEST_CASES_FILTERS_LIST = "ReleaseDetails.TestCases.Filters";
		public const string PROJECT_SETTINGS_ATTACHMENT_ASSOCIATIONS_FILTERS_LIST = "Attachments.Associations.Filters";
		public const string PROJECT_SETTINGS_RISK_LIST_FILTERS = "RiskList.Filters";
		public const string PROJECT_SETTINGS_BASELINE_FILTERS_LIST = "Baseline.Filters";
		public const string PROJECT_SETTINGS_BASELINEADMIN_FILTERS_LIST = "Admin.Baseline.Filters";
		public const string PROJECT_SETTINGS_BASELINEARTIFACT_FILTERS_LIST = "Artifact.Baseline.Filters";
        public const string PROJECT_SETTINGS_PULLREQUEST_FILTERS_LIST = "PullRequests.Filters";

        //Pagination Sizes
        public const string PROJECT_SETTINGS_INCIDENT_INCIDENT_PAGINATION_SIZE = "IncidentPaginationSize";
		public const string PROJECT_SETTINGS_TASK_TASK_PAGINATION_SIZE = "TaskPaginationSize";
		public const string PROJECT_SETTINGS_TEST_SET_TEST_SET_PAGINATION_SIZE = "TestSetList.Pagination";
		public const string PROJECT_SETTINGS_TEST_CASE_TEST_CASE_PAGINATION_SIZE = "TestCaseList.Pagination";
		public const string PROJECT_SETTINGS_TEST_RUN_TEST_RUN_PAGINATION_SIZE = "TestRunList.Pagination";
		public const string PROJECT_SETTINGS_REQUIREMENT_REQUIREMENT_PAGINATION_SIZE = "RequirementsList.Pagination";
		public const string PROJECT_SETTINGS_RELEASE_RELEASE_PAGINATION_SIZE = "ReleaseList.Pagination";
		public const string PROJECT_SETTINGS_TEST_SET_TEST_CASE_PAGINATION = "TestSetDetails.TestCases.Pagination";
		public const string PROJECT_SETTINGS_ARTIFACT_LINKS_PAGINATION = "ArtifactLinks.Pagination";
		public const string PROJECT_SETTINGS_TEST_CASE_TEST_STEP_PAGINATION = "TestCaseDetails.TestSteps.Pagination";
		public const string PROJECT_SETTINGS_AUTOMATION_HOST_PAGINATION_SIZE = "AutomationHosts.Pagination";
		public const string PROJECT_SETTINGS_REQUIREMENTS_TASKS_PAGINATION = "RequirementsTaskList.Pagination";
		public const string PROJECT_SETTINGS_HISTORY_PAGINATION = "History.Pagination";
		public const string PROJECT_SETTINGS_HISTORYSET_ADMINPAGINATION = "History.AdminSetPagination";
		public const string PROJECT_SETTINGS_BASELINE_PAGINATION = "Baseline.Pagination";
		public const string PROJECT_SETTINGS_BASELINEADMIN_PAGINATION = "Admin.Baseline.Pagination";
		public const string PROJECT_SETTINGS_BASELINEARTIFACT_PAGINATION = "Artifact.Baseline.Pagination";

		//Other
		public const string PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE = "IterationPlan.SelectedRelease";
		public const string PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS = "DocumentList.GeneralSettings";
		public const string PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS = "Resources.GeneralSettings";
		public const string PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS = "RequirementsList.GeneralSettings";
		public const string PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS = "ReleaseList.GeneralSettings";
		public const string PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS = "TestCaseList.GeneralSettings";
		public const string PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS = "TestSetList.GeneralSettings";
		public const string PROJECT_SETTINGS_PLANNING_BOARD_EXPANDED_GROUPS = "PlanningBoard.ExpandedGroups";
		public const string PROJECT_SETTINGS_TEST_STEP_GENERAL_SETTINGS = "TestStepList.GeneralSettings";
		public const string PROJECT_SETTINGS_TEST_EXECUTION_GENERAL_SETTINGS = "TestExecution.GeneralSettings";
		public const string PROJECT_SETTINGS_BUILD_GENERAL_SETTINGS = "Build.GeneralSettings";
		public const string PROJECT_SETTINGS_REPORTS_GENERAL_SETTINGS = "Reports.GeneralSettings";
		public const string PROJECT_SETTINGS_REQUIREMENT_STEP_GENERAL_SETTINGS = "RequirementStep.GeneralSettings";
		public const string PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL = "Global.SidebarPanel";
		public const string PROJECT_SETTINGS_TASKS_GENERAL_SETTINGS = "Tasks.GeneralSettings";
		public const string PROJECT_SETTINGS_REQUIREMENT_TASKS_EXPANDED = "RequirementTasks.ExpandedList";
		public const string PROJECT_SETTINGS_SOURCE_CODE_LIST_GENERAL_SETTINGS = "SourceCodeList.GeneralSettings";
		public const string PROJECT_SETTINGS_SOURCE_CODE_LIST_COMMIT_SETTINGS = "SourceCodeList.CommitList";
		public const string PROJECT_SETTINGS_SOURCE_CODE_LIST_FILE_SETTINGS = "SourceCodeList.FileList";
		public const string PROJECT_SETTINGS_RELEASE_DETAILS_TEST_CASES_GENERAL = "ReleaseDetails.TestCases.General";
		public const string PROJECT_SETTINGS_ATTACHMENT_ASSOCIATIONS_GENERAL = "Attachments.Associations.General";
		public const string PROJECT_SETTINGS_TASK_BOARD_SETTINGS = "TaskBoard.General";
		public const string PROJECT_SETTINGS_TASK_BOARD_EXPANDED_GROUPS = "TaskBoard.ExpandedGroups";
		public const string PROJECT_SETTINGS_INCIDENT_BOARD_SETTINGS = "IncidentBoard.General";
		public const string PROJECT_SETTINGS_INCIDENT_BOARD_EXPANDED_GROUPS = "IncidentBoard.ExpandedGroups";
		public const string PROJECT_SETTINGS_RISK_LIST_GENERAL = "RiskList.General";
        public const string PROJECT_SETTINGS_PULLREQUEST_GENERAL = "PullRequests.General";
		public const string PROJECT_SETTINGS_REQUIREMENT_DOCUMENT_VIEW = "RequirementsList.DocumentView";

		//Project Settings Keys
		public const string PROJECT_SETTINGS_KEY_SORT_EXPRESSION = "SortExpression";
		public const string PROJECT_SETTINGS_KEY_NUMBER_ROWS_PER_PAGE = "NumberRowsPerPage";
		public const string PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID = "SelectedReleaseId";
		public const string PROJECT_SETTINGS_KEY_SELECTED_NODE_ID = "SelectedNodeId";
		public const string PROJECT_SETTINGS_KEY_STARTING_INDEX = "StartingIndex";
		public const string PROJECT_SETTINGS_KEY_INCIDENT_TYPE = "IncidentType";
		public const string PROJECT_SETTINGS_KEY_INCLUDE_TASKS = "IncludeTasks";
		public const string PROJECT_SETTINGS_KEY_INCLUDE_INCIDENTS = "IncludeIncidents";
		public const string PROJECT_SETTINGS_KEY_INCLUDE_TEST_CASES = "IncludeTestCases";
		public const string PROJECT_SETTINGS_KEY_INCLUDE_DETAILS = "IncludeDetails";
		public const string PROJECT_SETTINGS_KEY_GROUP_BY_OPTION = "GroupByOption";
		public const string PROJECT_SETTINGS_KEY_DISPLAY_MODE = "DisplayMode";
		public const string PROJECT_SETTINGS_KEY_TE_DISPLAY_MODE_MAIN = "DisplayModeMain";
		public const string PROJECT_SETTINGS_KEY_TE_DISPLAY_MODE_SUB = "DisplayModeSub";
		public const string PROJECT_SETTINGS_KEY_TE_ALWAYS_SHOW_TEST_RUN = "alwaysShowTestRun";
		public const string PROJECT_SETTINGS_KEY_TE_SHOW_CASE_DESCRIPTION = "showCaseDescription";
		public const string PROJECT_SETTINGS_KEY_TE_SHOW_EXPECTED_RESULT = "showExpectedResults";
		public const string PROJECT_SETTINGS_KEY_TE_SHOW_SAMPLE_DATA = "showSampleData";
		public const string PROJECT_SETTINGS_KEY_TE_SHOW_CUSTOM_PROPERTIES = "showCustomProperties";
		public const string PROJECT_SETTINGS_KEY_TE_SHOW_LAST_RESULT = "showLastResult";
		public const string PROJECT_SETTINGS_KEY_TE_CURRENT_TEST_RUN_ID = "currentTestRunId";
		public const string PROJECT_SETTINGS_KEY_TE_CURRENT_TEST_RUN_STEP_ID = "currentTestRunStepId";
		public const string PROJECT_SETTINGS_KEY_MINIMIZED = "Minimized";
		public const string PROJECT_SETTINGS_KEY_WIDTH = "Width";
		public const string PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION = "CommentsSortDirection";
		public const string PROJECT_SETTINGS_KEY_INCIDENTS_RELEASE_FILTER_TYPE = "IncidentsReleaseFilterType";
		public const string PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE = "Navigation.PageSize";
		public const string PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE = "Navigation.CurrentPage";
		public const string PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS = "ExpandedProjects";
		public const string PROJECT_SETTINGS_KEY_HOME_PAGE = "ProjectHomePage";
		public const string PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL = "Map.OpenLevel";
		public const string PROJECT_SETTINGS_KEY_MIND_MAP_INCLUDE_ASSOCIATIONS = "Map.IncludeAssociations";
        public const string PROJECT_SETTINGS_KEY_DIFF_MODE = "DiffMode";
		public const string PROJECT_SETTINGS_KEY_PARENT_REQUIREMENT_ID = "ParentRequirementId";

		//Querystring parameters used to pass data between pages
		public const string PARAMETER_LOGIN_RETURN_URL = "ReturnUrl";
		public const string PARAMETER_USER_ID = "userId";
        public const string PARAMETER_USERNAME = "username";
        public const string PARAMETER_WORKFLOW_ID = "workflowId";
		public const string PARAMETER_WORKFLOW_TRANSITION_ID = "workflowTransitionId";
		public const string PARAMETER_INCIDENT_STATUS_ID = "incidentStatusId";
		public const string PARAMETER_CHART_TYPE = "chartType";
		public const string PARAMETER_REPORT_TYPE = "reportType";
		public const string PARAMETER_VIEW_VERSIONS = "viewVersions";
		public const string PARAMETER_ERROR_MESSAGE = "errorMessage";
		public const string PARAMETER_PAGINATION_START_ROW = "paginationStartRow";
		public const string PARAMETER_PAGINATION_START_PAGE = "paginationStartPage";
		public const string PARAMETER_REFERER_TASK_LIST = "referrerTaskList";
		public const string PARAMETER_REFERER_TEST_CASE_LIST = "referrerTestCaseList";
		public const string PARAMETER_REFERER_PLANNING_BOARD = "referrerPlanningBoard";
		public const string PARAMETER_REFERER_TEST_SET_LIST = "referrerTestSetList";
		public const string PARAMETER_CUSTOM_PROPERTY_LIST_ID = "customPropertyListId";
		public const string PARAMETER_ATTACHMENT_VERSION_ID = "attachmentVersionId";
		public const string PARAMETER_PROJECT_ATTACHMENT_FOLDER_ID = "projectAttachmentFolderId";
		public const string PARAMETER_DATA_SYNC_SYSTEM_ID = "dataSyncSystemId";
		public const string PARAMETER_ARTIFACT_ID = "artifactId";
		public const string PARAMETER_ARTIFACT_TYPE_ID = "artifactTypeId";
		public const string PARAMETER_ARTIFACT_FIELD_ID = "artifactFieldId";
		public const string PARAMETER_CUSTOM_PROPERTY_ID = "customPropertyId";
		public const string PARAMETER_PROJECT_GROUP_ID = "projectGroupId";
		public const string PARAMETER_REPORT_ID = "reportId";
		public const string PARAMETER_REPORT_FORMAT_ID = "reportFormatId";
		public const string PARAMETER_REPORT_SECTION_ID = "reportSectionId";
		public const string PARAMETER_REFERER_TEST_SET_DETAILS = "referrerTestSetDetails";
		public const string PARAMETER_SOURCE_CODE_FILE_SESSION_ID = "sourceCodeFileSessionId";
		public const string PARAMETER_SOURCE_CODE_REVISION_SESSION_ID = "sourceCodeRevSessionId";
		public const string PARAMETER_SOURCE_CODE_FILE_KEY = "sourceCodeFileKey";
		public const string PARAMETER_SOURCE_CODE_REVISION_KEY = "sourceCodeRevisionKey";
		public const string PARAMETER_SOURCE_CODE_BRANCH_KEY = "sourceCodeBranch";
		public const string PARAMETER_SOURCE_CODE_ARTIFACT_LINK_ID = "sourceCodeArtifactLinkId";
		public const string PARAMETER_VERSION_CONTROL_SYSTEM_ID = "versionControlSystemId";
		public const string PARAMETER_TAB_NAME = "tab";
		public const string PARAMETER_EVENT_ID = "eventId";
		public const string PARAMETER_TEST_RUNS_PENDING_ID = "testRunsPendingId";
		public const string PARAMETER_AUTOMATION_HOST_ID = "automationHostId";
		public const string PARAMETER_AUTOMATION_ENGINE_ID = "automationEngineId";
		public const string PARAMETER_INCIDENT_ID = "incidentId";
		public const string PARAMETER_RISK_ID = "riskId";
		public const string PARAMETER_HELP_URL = "helpUrl";
		public const string PARAMETER_HELP_SECTION = "section";
		public const string PARAMETER_DEMO_LOGIN = "demoLogin";
		public const string PARAMETER_ATTACHMENT_ID = "attachmentId";
		public const string PARAMETER_TEST_CASE_ID = "testCaseId";
		public const string PARAMETER_TEST_STEP_ID = "testStepId";
		public const string PARAMETER_TEST_RUN_ID = "testRunId";
		public const string PARAMETER_RELEASE_ID = "releaseId";
		public const string PARAMETER_VIEW_FILES = "viewFiles";
		public const string PARAMETER_VIEW_ASSOCIATIONSSC = "viewAssociations";
		public const string PARAMETER_VIEW_REVISIONS = "viewRevisions";
		public const string PARAMETER_REQUIREMENT_ID = "requirementId";
		public const string PARAMETER_REQUIREMENT_STATUS_ID = "requirementStatusId";
		public const string PARAMETER_PROJECT_ROLE_ID = "projectRoleId";
		public const string PARAMETER_PROJECT_ID = "projectId";
		public const string PARAMETER_TASK_ID = "taskId";
		public const string PARAMETER_TASK_STATUS_ID = "taskStatusId";
		public const string PARAMETER_REFERER_REQUIREMENT_DETAILS = "referrerRequirementDetails";
		public const string PARAMETER_REFERER_RELEASE_DETAILS = "referrerReleaseDetails";
		public const string PARAMETER_REFERER_ITERATION_PLAN = "referrerIterationPlan";
		public const string PARAMETER_REFERER_PROJECT_LIST = "referrerProjectList";
		public const string PARAMETER_REFERER_PROJECT_LIST_DETECTED = "referrerMyDetected";
		public const string PARAMETER_REFERER_PROJECT_HOME = "referrerProjectHome";
		public const string PARAMETER_TEST_SET_ID = "testSetId";
		public const string PARAMETER_TEST_SET_TEST_CASE_ID = "testSetTestCaseId";
		public const string PARAMETER_ORIGINAL_URL = "originalUrl";
		public const string PARAMETER_LICENSE_ERROR_TYPE = "licenseError";
		public const string PARAMETER_CHANGESET_ID = "change";
		public const string PARAMETER_GRAPH_ID = "graphId";
		public const string PARAMETER_CUSTOM_GRAPH_ID = "customGraphId";
		public const string PARAMETER_RSS_TOKEN = "rssToken";
		public const string PARAMETER_SAVED_FILTER_ID = "savedFilterId";
		public const string PARAMETER_BUILD_ID = "buildId";
		public const string PARAMETER_THEME_NAME = "themeName";
		public const string PARAMETER_REPOSITORY_PATH = "path";
		public const string PARAMETER_EVENT_CATEGORY = "category";
		public const string PARAMETER_REPORT_GENERATED_ID = "reportGeneratedId";
		public const string PARAMETER_REPORT_CONFIGURATION_SPECIFIED = "reportConfigSpecified";
		public const string PARAMETER_RELEASE_STATUS_ID = "releaseStatusId";
		public const string PARAMETER_TEST_CASE_STATUS_ID = "testCaseStatusId";
		public const string PARAMETER_TEST_CONFIGURATION_SET_ID = "testConfigurationSetId";
		public const string PARAMETER_PROJECT_TEMPLATE_ID = "projectTemplateId";
		public const string PARAMETER_PORTFOLIO_ID = "portfolioId";
		public const string PARAMETER_DOCUMENT_STATUS_ID = "documentStatusId";
		public const string PARAMETER_FOLDER_ID = "folderId";
		public const string PARAMETER_BASELINE_ID = "baselineId";
		public const string PARAMETER_SAVED_REPORT_ID = "reportSavedId";

		//Querystring TabNames
		public const string PARAMETER_TAB_TESTSTEP = "TestSteps";
		public const string PARAMETER_TAB_TESTRUN = "TestRuns";
		public const string PARAMETER_TAB_COVERAGE = "Coverage";
		public const string PARAMETER_TAB_ATTACHMENTS = "Attachments";
		public const string PARAMETER_TAB_HISTORY = "History";
		public const string PARAMETER_TAB_CUSTOMPROP = "CustomProp";
		public const string PARAMETER_TAB_RELEASE = "Releases";
		public const string PARAMETER_TAB_TASK = "Tasks";
		public const string PARAMETER_TAB_ASSOCIATION = "Associations";
		public const string PARAMETER_TAB_INCIDENT = "Incidents";
		public const string PARAMETER_TAB_SCHEDULE = "Schedule";
		public const string PARAMETER_TAB_DISCUSS = "Discussion";
		public const string PARAMETER_TAB_VERSION = "Versions";
        public const string PARAMETER_TAB_PREVIEW = "Preview";
        public const string PARAMETER_TAB_AUTOMATION = "Automation";
		public const string PARAMETER_TAB_TESTCASE = "TestCase";
		public const string PARAMETER_TAB_REQUIREMENT = "Requirement";
		public const string PARAMETER_TAB_TESTSET = "TestSet";
		public const string PARAMETER_TAB_BUILD = "Builds";
		public const string PARAMETER_TAB_REVISION = "Revisions";
		public const string PARAMETER_TAB_OVERVIEW = "Overview";
        public const string PARAMETER_TAB_BASELINE = "Baselines";
		public const string PARAMETER_TAB_EDIT = "Edit";
		public const string PARAMETER_TAB_ACTION = "Actions";
		public const string PARAMETER_TAB_FILE = "Files";
		public const string PARAMETER_TAB_BRANCH = "Branches";
		public const string PARAMETER_TAB_DIAGRAM = "Diagram";
		public const string PARAMETER_TAB_SIGNATURES = "Signatures";
		//Report 'tab' names
		public const string PARAMETER_TAB_REPORT_CONFIGURATION = "Configure";
		public const string PARAMETER_TAB_REPORT_VIEWER = "View";

		//Parameter values
		public const string PARAMETER_VALUE_TRUE = "true";
		public const string PARAMETER_VALUE_FALSE = "false";

		//Special filters
		public const string SPECIAL_FILTER_FOLDER_ID = "_FolderId";

		//User Settings Collections
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_FILTERS = "Administration.ProjectListFilters";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_LIST_PAGINATION = "Administration.ProjectPagination";
		public const string USER_SETTINGS_ADMINISTRATION_USER_LIST_FILTERS = "Administration.UserListFilters";
		public const string USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_FILTERS = "Administration.UserRequests.Filters";
		public const string USER_SETTINGS_ADMINISTRATION_USER_REQUESTS_GENERAL = "Administration.UserRequests.General";
		public const string USER_SETTINGS_ADMINISTRATION_USER_LIST_PAGINATION = "Administration.UserPagination";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_FILTERS = "Administration.ProjectGroupListFilters";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_LIST_PAGINATION = "Administration.ProjectGroupListPagination";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_FILTERS = "ProjectMembershipAdd.Filters";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_MEMBERSHIP_ADD_PAGINATION = "ProjectMembershipAdd.Pagination";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_MEMBERSHIP_ADD_FILTERS = "ProjectGroupMembershipAdd.Filters";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_GROUP_MEMBERSHIP_ADD_PAGINATION = "ProjectGroupMembershipAdd.Pagination";
		public const string USER_SETTINGS_ADMINISTRATION_EVENT_LOG_FILTERS = "Administration.EventLog.Filters";
		public const string USER_SETTINGS_ADMINISTRATION_EVENT_LOG_GENERAL = "Administration.EventLog.General";
		public const string USER_SETTINGS_MY_PAGE_SETTINGS = "MyPage.GeneralSettings";
		public const string USER_SETTINGS_USER_PROFILE_SETTINGS = "UserProfile.GeneralSettings";
		public const string USER_SETTINGS_USER_PROFILE_ACTIONFILTER = "UserProfile.Action.Filter";
		public const string USER_SETTINGS_COLLAPSIBLE_PANEL_STATE = "CollapsiblePanel.State";
		public const string USER_SETTINGS_TAB_CONTROL_STATE = "TabControl.State";
		public const string USER_SETTINGS_GUIDED_TOURS_STATE = "GuidedTours.State";
		public const string USER_SETTINGS_PROJECT_GROUP_PLANNING_BOARD = "ProjectGroup.PlanningBoard";
		public const string USER_SETTINGS_PLANNING_BOARD_EXPANDED_GROUPS = "ProjectGroup.ExpandedGroups";
		public const string USER_SETTINGS_GROUP_INCIDENT_FILTERS = "GroupIncidents.Filters";
		public const string USER_SETTINGS_GROUP_INCIDENT_GENERAL = "GroupIncidents.General";
		public const string USER_SETTINGS_GROUP_INCIDENT_COLUMNS = "GroupIncidents.Columns";
		public const string USER_SETTINGS_GROUP_RELEASE_FILTERS = "GroupReleases.Filters";
		public const string USER_SETTINGS_GROUP_RELEASE_GENERAL = "GroupReleases.General";
		public const string USER_SETTINGS_GROUP_RELEASE_COLUMNS = "GroupReleases.Columns";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_PAGINATION = "Administration.ProjectTemplateListPagination";
		public const string USER_SETTINGS_ADMINISTRATION_PROJECT_TEMPLATE_LIST_FILTERS = "Administration.ProjectTemplateListFilters";
		public const string USER_SETTINGS_PROJECT_GROUP_HOME_GENERAL = "ProjectGroupHome.General";
		public const string USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_FILTERS = "Administration.PortfolioList.Filters";
		public const string USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_GENERAL = "Administration.PortfolioList.General";

		//User Settings Keys
		public const string USER_SETTINGS_KEY_PAGINATION_START_ROW = "StartRow";
		public const string USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE = "PageSize";
		public const string USER_SETTINGS_KEY_SORT_EXPRESSION = "SortExpression";
		public const string USER_SETTINGS_KEY_FILTER_BY_PROJECT = "FilterByProject";
		public const string USER_SETTINGS_KEY_CURRENT_CULTURE = "CurrentCulture";
		public const string USER_SETTINGS_KEY_START_PAGE = "StartPage";
		public const string USER_SETTINGS_KEY_CURRENT_COLOR_MODE = "CurrentColorMode";
		public const string USER_SETTINGS_GUIDED_TOURS_TEST_EXECUTION_SEEN = "GuidedToursTestExecutionSeen";
		public const string USER_SETTINGS_GUIDED_TOURS_TEST_EXECUTION_EXPLORATORY_SEEN = "GuidedToursTestExecutionSeen";
		public const string USER_SETTINGS_GUIDED_TOURS_NAVIGATION_BAR_SEEN = "GuidedToursNavigationBarSeen";
		public const string USER_SETTINGS_KEY_SELECTED_PROJECT = "SelectedProjectId";

		//Validation regular expressions
		public const string VALIDATION_REGEX_URL = @"^((https|http|file|ftp|sftp|ssh)://)?[a-zA-Z0-9\._\-]+(:[0-9]+)?[a-zA-Z0-9\._$\-%/ ]*([\?a-zA-Z0-9~@&=_:;#!\-/%\.\+]+)?";
		public const string VALIDATION_REGEX_TAGS = @"^[0-9a-zA-Z, ]+$";
		public const string VALIDATION_REGEX_VERSION_NUMBER = @"^[0-9a-zA-Z \.]+";
		public const string VALIDATION_REGEX_EFFORT_HOURS = @"^[0-9]*([\.,][0-9]+)?$";
		public const string VALIDATION_REGEX_EFFORT_HOURS_NEGATIVE_ALLOWED = @"^-?[0-9]*([\.,][0-9]+)?$";
		public const string VALIDATION_REGEX_ESTIMATE_POINTS = @"^[0-9]*([\.,][0-9]+)?$";
		public const string VALIDATION_REGEX_INTEGER = @"^[0-9]*$";
		public const string VALIDATION_REGEX_INTEGER_NEGATIVE = @"^-?[0-9]*$";
		public const string VALIDATION_REGEX_INTEGER_LIST = @"(\d+)(,\s*\d+)*$";
		public const string VALIDATION_REGEX_EMAIL_ADDRESS = @"^([\w'-\.&']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,20}|[0-9]{1,3})(\]?)$";
		public const string VALIDATION_REGEX_USER_NAME = @"^[a-zA-Z0-9\._\-@]+$";
		public const string VALIDATION_REGEX_HTML_COLOR = @"^[0-9A-Fa-f]{6}$";  //Does not include the # prefix
		public const string VALIDATION_REGEX_DOMAIN_LIST = @"^[0-9a-zA-Z \.,_\-\*:/]+$";
		public const string VALIDATION_REGEX_FILENAME_XML = @"^[a-zA-Z0-9\-_]+$";
        public const string VALIDATION_REGEX_TOUR_NAME = @"^[a-zA-Z0-9\-_\.]+$";
        public const string VALIDATION_REGEX_INTEGER_GREATER_THAN_ZERO = @"^[1-9][0-9]*$";
		public const string VALIDATION_REGEX_MFA_OTP = @"^[0-9]{6}$";

		//Other regular expressions
		public const string REGEX_TIME_EDITABLE_12H = @"(?<Hour>\d{2}):(?<Min>\d{2}):(?<Sec>\d{2}) (?<Meridian>AM|PM)";
		public const string REGEX_TIME_EDITABLE_24H = @"(?<Hour>\d{2}):(?<Min>\d{2}):(?<Sec>\d{2})";
		public const string REGEX_USERAGENT_BROWSER = @"android|avantgo|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od|ad)|iris|kindle|lge |maemo|midp|mmp|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\\/|plucker|pocket|psp|symbian|treo|up\\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino";
		public const string REGEX_USERAGENT_VERSION = @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\\-(n|u)|c55\\/|capi|ccwa|cdm\\-|cell|chtm|cldc|cmd\\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\\-s|devi|dica|dmob|do(c|p)o|ds(12|\\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\\-|_)|g1 u|g560|gene|gf\\-5|g\\-mo|go(\\.w|od)|gr(ad|un)|haie|hcit|hd\\-(m|p|t)|hei\\-|hi(pt|ta)|hp( i|ip)|hs\\-c|ht(c(\\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\\-(20|go|ma)|i230|iac( |\\-|\\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\\/)|klon|kpt |kwc\\-|kyo(c|k)|le(no|xi)|lg( g|\\/(k|l|u)|50|54|e\\-|e\\/|\\-[a-w])|libw|lynx|m1\\-w|m3ga|m50\\/|ma(te|ui|xo)|mc(01|21|ca)|m\\-cr|me(di|rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\\-2|po(ck|rt|se)|prox|psio|pt\\-g|qa\\-a|qc(07|12|21|32|60|\\-[2-7]|i\\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\\-|oo|p\\-)|sdk\\/|se(c(\\-|0|1)|47|mc|nd|ri)|sgh\\-|shar|sie(\\-|m)|sk\\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\\-|v\\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\\-|tdg\\-|tel(i|m)|tim\\-|t\\-mo|to(pl|sh)|ts(70|m\\-|m3|m5)|tx\\-9|up(\\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|xda(\\-|2|g)|yas\\-|your|zeto|zte\\-";

		//Display format strings
		public const string FORMAT_DATE = "{0:d-MMM-yyyy}"; //Use custom format that works in all cultures
		public const string FORMAT_DATE_EDITABLE = "{0:d}"; //Use standard short form - so culture specific
		public const string FORMAT_DATE_TIME = "{0:G}"; //Use standard general form - so culture specific
		public const string FORMAT_ID = "{0:########}";  //Format of token with 0's.
		public const string FORMAT_SHORT_ID = "{0:########}"; //Forma ot token without 0's
		public const string FORMAT_TIME_INTERVAL_HOURS = "{0:0.0}h"; //Time interval values (hours only)
		public const string FORMAT_TIME_INTERVAL_HOURS_EDITABLE = "{0:0.00}"; //Time interval values (hours only)
		public const string FORMAT_TIME_EDITABLE_12 = "{0:hh:mm:ss tt}";  //Used for time maskededit controls
		public const string FORMAT_TIME_EDITABLE_24 = "{0:HH:mm:ss}";  //Used for time maskededit controls
		public const string FORMAT_DATE_TIME_INVARIANT = "{0:yyyy-MM-ddTHH:mm:ss.fff}";   //Used when passing system datetimes to AJAX controls
		public const string FORMAT_DATE_RSS = "{0:r}";
		public const string FORMAT_DATE_ATOM = "{0:yyyy-MM-ddTHH:mm:sszzz}";
		public const string FORMAT_POINTS = "{0:0.0}";
        public const string FORMAT_DECIMAL_1DP = "{0:0.0}";

        //Other constants
        public const string DISPLAY_SINGLE_QUOTE = "'";
		public const string WEBPART_SUBTITLE_RSS = "RSS";   //Fake subtitle used to tell WebParts to display an RSS icon
		public const string REPORT_WINDOW_OPTIONS = "height=600, width=800,status=yes, resizable=yes, scrollbars=yes, toolbar=yes,location=yes,menubar=yes";
		public const string REPOSITORY_TEST_EXTENSION = ".sstest";
		public const string DEFAULT_THEME = "ValidationMasterTheme";   //Used when we cannot access the current theme name programmatically
		public const string ARTIFACT_ID_TOKEN = "{art}";
		public const string MFA_ISSUER_NAME = "`[OnShore to replace]";

		//Compiles regexes
		public static Regex BrowserRegex = new Regex(GlobalFunctions.REGEX_USERAGENT_BROWSER, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		public static Regex VersionRegex = new Regex(GlobalFunctions.REGEX_USERAGENT_VERSION, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

		#endregion

		#region private variables/constants

		private const string PROJECT_ROLE_ARTIFACT_IDS = "projectRoleArtifactIds_";

        #endregion

        /// <summary>
        /// Displays the current copyright year range of the application
        /// </summary>
        public static string CopyrightYear
		{
			get
			{
				//return "2006-" + DateTime.Now.Year.ToString();
				return DateTime.Now.Year.ToString();
			}
		}

		/// <summary>
		/// Makes a string safe for use as a Javascript string argument
		/// </summary>
		/// <param name="input">The input string</param>
		/// <param name="containsMarkup">Does the input string contain HTML markup or not</param>
		/// <param name="enclose">Do we need to enclose in single quotes</param>
		/// <returns>The string with all single quotes escaped and other characters handled correctly</returns>
		public static string JSEncode(string input, bool containsMarkup = false, bool enclose = false)
		{
			if (input == null)
			{
				if (enclose)
				{
					return "''";
				}
				else
				{
					return "";
				}
			}
			if (containsMarkup)
			{
				//For input text that has markup we don't add any <BR> tags since they will be in the markup
				string output = input;
				output = output.Replace("\\", "\\\\");
				output = output.Replace("'", @"\'");
				output = output.Replace("\n", "");
				output = output.Replace("\r", "");
				if (enclose)
				{
					return "'" + output + "'";
				}
				else
				{
					return output;
				}
			}
			else
			{
				//For input text that has no markup we need to convert <BR> tags into newlines
				string output = input;
				output = output.Replace("\\", "\\\\");
				output = output.Replace("'", @"\'");
				output = output.Replace("\n", "<br>");
				output = output.Replace("\r", "");
				if (enclose)
				{
					return "'" + output + "'";
				}
				else
				{
					return output;
				}
			}
		}

		/// <summary>
		/// Makes sure that the XML files we load dynamically do not contain path modifiers (/ or ..) since it could
		/// be someone trying to hack the website
		/// </summary>
		/// <param name="filename">The filename</param>
		/// <returns>True if OK</returns>
		public static bool VerifyFilenameForPathChars(string filename)
		{
			//First check for null and ones that are too long
			if (filename == null || filename.Length > 245)
			{
				return false;
			}
			Regex regex = new Regex(VALIDATION_REGEX_FILENAME_XML);
			return regex.IsMatch(filename);
		}

		/// <summary>
		/// Returns the MIME Type to use for a specific attachment filename
		/// </summary>
		/// <param name="filename">The filename being displayed</param>
		/// <returns>The MIME type</returns>
		public static string GetFileMimeType(string filename)
		{
			Dictionary<string, string> fileInfo = GlobalFunctions.GetFileTypeInformation(filename);

			return fileInfo["mimetype"];
		}

		/// <summary>
		/// Returns the icon image for a particular artifact type
		/// </summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <returns>The path to the image that represents this type of artifact</returns>
		/// <remarks>
		/// 1) Doesn't distinguish between folder items and child items
		/// 2) Doesn't distinguish between releases and iterations
		/// </remarks>
		public static string GetIconForArtifactType(int artifactTypeId)
		{
			return GetIconForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
		}

		/// <summary>
		/// Determines if the string ends with the specified character
		/// </summary>
		/// <param name="s">The string being tested</param>
		/// <param name="c">The character we're looking for</param>
		/// <returns>True if it does contain the character</returns>
		public static bool StringEndsWith(string s, char c)
		{
			int length = s.Length;
			return ((length != 0) && (s[length - 1] == c));
		}

		/// <summary>
		/// Generates a masked version of a password for displaying in a text box
		/// </summary>
		/// <param name="password">The real password</param>
		/// <returns>The masked version</returns>
		public static string MaskPassword(string password)
		{
			if (String.IsNullOrEmpty(password))
			{
				return "";
			}
			else
			{
				string mask = "";
				for (int i = 0; i < password.Length; i++)
				{
					mask += "*";
				}
				return mask;
			}
		}

		/// <summary>
		/// Determines if we have a mask or real password
		/// </summary>
		/// <param name="password">The password/mask</param>
		/// <returns>True if the password is masked</returns>
		public static bool IsMasked(string password)
		{
			for (int i = 0; i < password.Length; i++)
			{
				if (password[i] != '*')
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Makes a string safe for use in XML (e.g. web service)
		/// </summary>
		/// <param name="input">The input string (as object)</param>
		/// <returns>The output string</returns>
		public static string MakeXmlSafe(object input)
		{
			//Handle null reference case
			if (input == null)
			{
				return "";
			}

			//Handle empty string case
			string inputString = (string)input;
			if (inputString == "")
			{
				return inputString;
			}

			string output = inputString.Replace("\x00", "");
			output = output.Replace("\n", "");
			output = output.Replace("\r", "");
			output = output.Replace("\x01", "");
			output = output.Replace("\x02", "");
			output = output.Replace("\x03", "");
			output = output.Replace("\x04", "");
			output = output.Replace("\x05", "");
			output = output.Replace("\x06", "");
			output = output.Replace("\x07", "");
			output = output.Replace("\x08", "");
			output = output.Replace("\x0B", "");
			output = output.Replace("\x0C", "");
			output = output.Replace("\x0E", "");
			output = output.Replace("\x0F", "");
			output = output.Replace("\x10", "");
			output = output.Replace("\x11", "");
			output = output.Replace("\x12", "");
			output = output.Replace("\x13", "");
			output = output.Replace("\x14", "");
			output = output.Replace("\x15", "");
			output = output.Replace("\x16", "");
			output = output.Replace("\x17", "");
			output = output.Replace("\x18", "");
			output = output.Replace("\x19", "");
			output = output.Replace("\x1A", "");
			output = output.Replace("\x1B", "");
			output = output.Replace("\x1C", "");
			output = output.Replace("\x1D", "");
			output = output.Replace("\x1E", "");
			output = output.Replace("\x1F", "");
			return output;
		}

		/// <summary>
		/// Returns the image file to use for a specific filename
		/// </summary>
		/// <param name="filename">The filename being displayed</param>
		/// <returns>The image file (not including any paths)</returns>
		public static string GetFileTypeImage(string filename)
		{
			Dictionary<string, string> fileInfo = GlobalFunctions.GetFileTypeInformation(filename);

			return fileInfo["image"];
		}

		/// <summary>The basic Yes/No List</summary>
		/// <returns>A disctionary containing the values for Yes and No</returns>
		public static Dictionary<string, string> YesNoList()
		{
			Dictionary<string, string> retList = new Dictionary<string, string>();

			retList.Add("Y", Resources.Main.Global_Yes);
			retList.Add("N", Resources.Main.Global_No);

			return retList;
		}

		/// <summary>Returns the list to specify sorting for the custom list values.</summary>
		/// <returns></returns>
		public static Dictionary<string, string> CustomListSortByList()
		{
			Dictionary<string, string> retList = new Dictionary<string, string>();

			retList.Add("0", Resources.Main.CustomListSort_ById);
			retList.Add("1", Resources.Main.CustomListSort_Alphabetical);

			return retList;
		}

		/// <summary>Returns a dictionary of numbers.</summary>
		/// <param name="min">The lowest, or minimum number, inclusive.</param>
		/// <param name="max">The highest, or maximum number, inclusive.</param>
		/// <returns>Dictionary of numbers.</returns>
		public static Dictionary<string, string> NumberList(int min, int max)
		{
			Dictionary<string, string> retList = new Dictionary<string, string>();

			for (int i = min; i <= max; i++)
			{
				retList.Add(i.ToString(), i.ToString());
			}

			return retList;
		}

		/// <summary>Gets display information for the specified file.</summary>
		/// <param name="filename">The filename.</param>
		/// <returns>Dictionary of Strings for the file. Available fields are "image", "mimetype", "description", and "extension"</returns>
		public static Dictionary<string, string> GetFileTypeInformation(string filename)
		{
			Dictionary<string, string> retDict = new Dictionary<string, string>();

			//Get the file's extension.
			string ext = "";
			if (!String.IsNullOrEmpty(filename))
			{
				try
				{
					ext = Path.GetExtension(filename);
				}
				catch (ArgumentException)
				{
					//Windows function failed, so try doing the old fashioned way
					int pos = filename.LastIndexOf('.');
					if (pos != -1 && pos < filename.Length - 1)
					{
						ext = filename.Substring(pos + 1);
					}
				}
			}

			//Get the file type object for this type
			Filetype fileExt = new FileTypeManager().GetFileTypeInfo(ext);

			if (fileExt != null)
			{
				retDict.Add("image", fileExt.Icon);
				retDict.Add("mimetype", fileExt.Mime);
				retDict.Add("description", fileExt.Description);
				retDict.Add("extension", fileExt.FileExtension);
			}
			else
			{
				//Hard-coded empty values..
				retDict.Add("image", "Unknown.gif");
				retDict.Add("mimetype", "application/octet-stream");
				retDict.Add("description", "Other File");
				retDict.Add("extension", " ");
			}

			return retDict;
		}

		/// <summary>Returns the icon image for a particular artifact type</summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <returns>The path to the image that represents this type of artifact</returns>
		/// <remarks>1) Doesn't distinguish between folder items and child items
		public static string GetIconForArtifactType(DataModel.Artifact.ArtifactTypeEnum artifactType, bool useAlternate = false)
		{
			switch (artifactType)
			{
				case DataModel.Artifact.ArtifactTypeEnum.Requirement:
					{
						if (useAlternate)
						{
							return "artifact-UseCase.svg";
						}
						else
						{
							return "artifact-Requirement.svg";
						}
					}

				case DataModel.Artifact.ArtifactTypeEnum.RequirementStep:
					return "artifact-RequirementStep.svg";

				case DataModel.Artifact.ArtifactTypeEnum.Incident:
					return "artifact-Incident.svg";

				case DataModel.Artifact.ArtifactTypeEnum.TestRun:
					return "artifact-TestRun.svg";

				case DataModel.Artifact.ArtifactTypeEnum.TestCase:
					return "artifact-TestCase.svg";

				case DataModel.Artifact.ArtifactTypeEnum.TestStep:
					return "artifact-TestStep.svg";

				case DataModel.Artifact.ArtifactTypeEnum.TestSet:
					return "artifact-TestSet.svg";

				case DataModel.Artifact.ArtifactTypeEnum.Release:
					if (useAlternate)
					{
						return "artifact-Iteration.svg";
					}
					else
					{
						return "artifact-Release.svg";
					}

				case DataModel.Artifact.ArtifactTypeEnum.Task:
					return "artifact-Task.svg";

				case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
					return "artifact-AutomationHost.svg";

				case DataModel.Artifact.ArtifactTypeEnum.Risk:
					return "artifact-Risk.svg";

				case DataModel.Artifact.ArtifactTypeEnum.RiskMitigation:
					return "artifact-RiskMitigation.svg";

				case DataModel.Artifact.ArtifactTypeEnum.User:
					return "artifact-Resource.svg";

				case DataModel.Artifact.ArtifactTypeEnum.Document:
					return "artifact-Document.svg";

                case DataModel.Artifact.ArtifactTypeEnum.Build:
                    return "artifact-Build.svg";
            }
			return "";
		}

		/// <summary>
		/// Displays a nullable boolean flag value as Y/N for use in dropdown lists
		/// </summary>
		/// <param name="boolValue">The value of the flag</param>
		/// <returns>Either 'Y', 'N' or ''</returns>
		public static string GetYnFlagForDropdown(bool? boolValue)
		{
			if (boolValue.HasValue)
			{
				return (boolValue.Value) ? "Y" : "N";
			}
			else
			{
				return "-";
			}
		}

		/// <summary>Displays a Y/N flag value as Yes/No</summary>
		/// <param name="flagYn">The value of the flag</param>
		/// <returns>Either 'Yes' or 'No'</returns>
		public static string DisplayYnFlag(string flagYn)
		{
			string display;

			display = "";
			if (flagYn == "Y")
			{
				display = Resources.Fields.Yes;
			}
			if (flagYn == "N")
			{
				display = Resources.Fields.No;
			}
			return display;
		}

		/// <summary>
		/// Gets the current timezone for the user
		/// </summary>
		/// <returns>The current timezone</returns>
		/// <remarks>Safer than directly checking SpiraContext because it handles NULL cases correctly</remarks>
		public static TimeZoneInfo GetTimezone()
		{
			if (SpiraContext.Current == null || String.IsNullOrEmpty(SpiraContext.Current.TimezoneId))
			{
				//Fallback to using the system local time
				return TimeZoneInfo.Local;
			}
			TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(SpiraContext.Current.TimezoneId);
			if (timeZone == null)
			{
				//Fallback to using the system local time
				return TimeZoneInfo.Local;
			}
			return timeZone;
		}

		/// <summary>
		/// Does a lookup of the Fields.resx resource file for a specific field name
		/// </summary>
		/// <param name="field">The string to localize</param>
		/// <returns>The localized field name/value, if unavailable, just returns the same as the input</returns>
		public static string LocalizeFields(string field)
		{
			string localizedField = Resources.Fields.ResourceManager.GetString(field);
			if (String.IsNullOrEmpty(localizedField))
			{
				return field;
			}
			return localizedField;
		}

		/// <summary>
		/// Localizes a UTC date for the specific user's timezone
		/// </summary>
		/// <param name="utcDate">The universal date</param>
		/// <returns>The localized date</returns>
		public static DateTime LocalizeDate(DateTime utcDate)
		{
			//If we have an unspecified date, force to utc
			if (utcDate.Kind == DateTimeKind.Unspecified)
			{
				utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
			}

			//If we have a local date already, do nothing
			if (utcDate.Kind == DateTimeKind.Local)
			{
				return utcDate;
			}

			if (SpiraContext.Current == null || String.IsNullOrEmpty(SpiraContext.Current.TimezoneId))
			{
				//Fallback to using the system local time
				return utcDate.ToLocalTime();
			}
			TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(SpiraContext.Current.TimezoneId);
			if (timeZone == null)
			{
				//Fallback to using the system local time
				return utcDate.ToLocalTime();
			}
			return TimeZoneInfo.ConvertTimeFromUtc(utcDate, timeZone);
		}

		/// <summary>
		/// Returns the current timezone offset from UTC in total hours
		/// </summary>
		/// <returns>The UTC offset</returns>
		public static double GetCurrentTimezoneUtcOffset()
		{
			TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
			if (HttpContext.Current != null && SpiraContext.Current != null && !String.IsNullOrEmpty(SpiraContext.Current.TimezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(SpiraContext.Current.TimezoneId);
				if (timeZone != null)
				{
					localTimeZone = timeZone;
				}
			}
			return localTimeZone.GetUtcOffset(DateTime.Now).TotalHours;
		}

		/// <summary>
		/// Localizes a UTC date for the specific user's timezone where the date doesn't have a time component, so we
		/// just want to make sure that we adjust the day depending on which side of UTC it's on.
		/// </summary>
		/// <param name="utcDate">The universal date</param>
		/// <returns>The localized date</returns>
		public static DateTime LocalizeDateWithoutTime(DateTime utcDate)
		{
			//If we have an unspecified date, force to utc
			if (utcDate.Kind == DateTimeKind.Unspecified)
			{
				utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
			}

			//If we have a local date already, do nothing
			if (utcDate.Kind == DateTimeKind.Local)
			{
				return utcDate;
			}

			TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
			if (HttpContext.Current != null && SpiraContext.Current != null && !String.IsNullOrEmpty(SpiraContext.Current.TimezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(SpiraContext.Current.TimezoneId);
				if (timeZone != null)
				{
					localTimeZone = timeZone;
				}
			}

			//We just need to add/remove a day. We then strip off the time component to prevent this function being 'misused'
			if (localTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes > 0)
			{
				return utcDate.Date.AddDays(1);
			}
			else
			{
				return utcDate.Date;
			}
		}

		/// <summary>
		/// Converts a local datetime in the current user's timezone back into UTC
		/// </summary>
		/// <param name="localDate">The local date</param>
		/// <returns>The UTC date</returns>
		public static DateTime? UniversalizeDate(DateTime? localDate)
		{
			if (localDate == null)
			{
				return null;
			}
			return UniversalizeDate(localDate.Value);
		}

		/// <summary>
		/// Localizes a UTC date for the specific user's timezone
		/// </summary>
		/// <param name="utcDate">The universal date</param>
		/// <returns>The localized date</returns>
		public static DateTime? LocalizeDate(DateTime? utcDate)
		{
			if (utcDate == null)
			{
				return null;
			}
			return LocalizeDate(utcDate.Value);
		}

		/// <summary>
		/// Converts a local datetime in the current user's timezone back into UTC
		/// </summary>
		/// <param name="localDate">The local date</param>
		/// <returns>The UTC date</returns>
		/// <remarks>Makes sure the dates don't go earlier than 1/1/1900 since SQL Server no like it</remarks>
		public static DateTime UniversalizeDate(DateTime localDate)
		{
			//If we have a universal date already, do nothing
			if (localDate.Kind == DateTimeKind.Utc)
			{
				return localDate;
			}

			DateTime utcDate;
			if (SpiraContext.Current == null || String.IsNullOrEmpty(SpiraContext.Current.TimezoneId))
			{
				//Fallback to using the system local time
				utcDate = localDate.ToUniversalTime();
			}
			else
			{
				TimeZoneInfo timeZone = null;
				try
				{
					timeZone = TimeZoneInfo.FindSystemTimeZoneById(SpiraContext.Current.TimezoneId);
					if (timeZone == null)
					{
						//Fallback to using the system local time
						utcDate = localDate.ToUniversalTime();
					}
					else
					{
						//Force the source date timezone kinds to to match
						if (timeZone == TimeZoneInfo.Local)
						{
							localDate = DateTime.SpecifyKind(localDate, DateTimeKind.Local);
						}
						else
						{
							localDate = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);
						}

						//Make sure the timezone is local
						utcDate = TimeZoneInfo.ConvertTimeToUtc(localDate, timeZone);
					}
				}
				catch (Exception exception)
				{
					if (timeZone == null)
					{
						Logger.LogErrorEvent("GlobalFunctions.UniversalizeDate", exception.Message + " - " + localDate.ToString("g") + ":" + localDate.Kind);
					}
					else
					{
						Logger.LogErrorEvent("GlobalFunctions.UniversalizeDate", exception.Message + " - " + localDate.ToString("g") + ":" + localDate.Kind + "," + timeZone.Id);
					}
					throw;
				}
			}

			//Make sure the date is not earlier than 1/1/1900
			DateTime minDateAllowed = DateTime.ParseExact("19000101", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
			if (utcDate < minDateAllowed)
			{
				utcDate = minDateAllowed;
			}

			return utcDate;
		}

		/// <summary>
		/// Calculates the intermediate color of two colors (used for risk exposure colors)
		/// </summary>
		/// <param name="color1">The hex code of color 1 (RRGGBB)</param>
		/// <param name="color2">The hex code of color 2 (RRGGBB)</param>
		/// <returns>The hex code (RRGGBB) of the intermediate color</returns>
		public static string InterpolateColorAsHtml(string color1, string color2)
		{
			try
			{
				Color result = InterpolateColor(color1, color2);
				return ColorTranslator.ToHtml(result);
			}
			catch (ArgumentException)
			{
				//Happens if a bad hex code was input
				return "";
			}
		}

		/// <summary>
		/// Calculates the intermediate color of two colors (used for risk exposure colors)
		/// </summary>
		/// <param name="color1">The hex code of color 1 (RRGGBB)</param>
		/// <param name="color2">The hex code of color 2 (RRGGBB)</param>
		/// <returns>The color of the intermediate color</returns>
		public static Color InterpolateColor(string color1, string color2)
		{
			try
			{
				Color col1 = ColorTranslator.FromHtml("#" + color1);
				Color col2 = ColorTranslator.FromHtml("#" + color2);
				int red = (col1.R + col2.R) / 2;
				int green = (col1.G + col2.G) / 2;
				int blue = (col1.B + col2.B) / 2;
				int alpha = (col1.A + col2.A) / 2;
				Color result = Color.FromArgb(alpha, red, green, blue);
				return result;
			}
			catch (ArgumentException)
			{
				//Happens if a bad hex code was input
				return new Color();
			}
		}

		/// <summary>
		/// Calculates the intermediate color of two colors (used for risk exposure colors)
		/// </summary>
		/// <param name="color1">The hex code of color 1 (RRGGBB)</param>
		/// <param name="color2">The hex code of color 2 (RRGGBB)</param>
		/// <returns>The color of the intermediate color</returns>
		public static Color InterpolateColor2(int probabilityScore, int impactScore)
		{
			try
			{
				int weight = probabilityScore * impactScore;
				if(weight >= 15)
				{
					return ColorTranslator.FromHtml("#FF5733");
				}
				else if(weight >= 7)
				{
					return ColorTranslator.FromHtml("#32CD32");
				}
				else
				{
					return ColorTranslator.FromHtml("#FF" +
						"BF00");
				}
			}
			catch (ArgumentException)
			{
				//Happens if a bad hex code was input
				return new Color();
			}
		}

		/// <summary>Displays a boolean flag value as Yes/No</summary>
		/// <param name="flag">The value of the flag</param>
		/// <returns>Either 'Yes' or 'No'</returns>
		public static string DisplayYnFlag(bool flag)
		{
			return (flag) ? Resources.Fields.Yes : Resources.Fields.No;
		}

        /// <summary>
        /// Gets the CSS for an active/inactive field
        /// </summary>
		/// <param name="flag">The value of the flag</param>
        /// <returns>The appropriate CSS class name</returns>
        public static string GetActiveFlagCssClass(bool flag)
        {
            return (flag) ? "bg-success" : "bg-warning";
        }

        /// <summary>
        /// Displays the CSS class for the build status
        /// </summary>
        /// <param name="buildStatus">The build status</param>
        /// <returns>The appropriate CSS class name</returns>
        public static string GetBuildStatusCssClass(Build.BuildStatusEnum buildStatus)
		{
			string display = "ExecutionStatusNotRun";

			switch (buildStatus)
			{
				case Build.BuildStatusEnum.Succeeded:
					display = "ExecutionStatusPassed";
					break;
				case Build.BuildStatusEnum.Failed:
					display = "ExecutionStatusFailed";
					break;
				case Build.BuildStatusEnum.Unstable:
					display = "ExecutionStatusCaution";
					break;
				case Build.BuildStatusEnum.Aborted:
					display = "ExecutionStatusBlocked";
					break;
			}
			return display;
		}

		/// <summary>
		/// Displays the Css Class for the execution status
		/// </summary>
		/// <param name="executionStatusId">The execution status ID for the test case being displayed</param>
		/// <returns>The appropriate Css Class Name</returns>
		public static string GetExecutionStatusCssClass(int executionStatusId)
		{
			string display = "ExecutionStatusNotRun";

			switch (executionStatusId)
			{
				case (int)TestCase.ExecutionStatusEnum.NotRun:
					display = "ExecutionStatusNotRun";
					break;
				case (int)TestCase.ExecutionStatusEnum.Passed:
					display = "ExecutionStatusPassed";
					break;
				case (int)TestCase.ExecutionStatusEnum.Failed:
					display = "ExecutionStatusFailed";
					break;
				case (int)TestCase.ExecutionStatusEnum.NotApplicable:
					display = "ExecutionStatusNotApplicable";
					break;
				case (int)TestCase.ExecutionStatusEnum.Caution:
					display = "ExecutionStatusCaution";
					break;
				case (int)TestCase.ExecutionStatusEnum.Blocked:
					display = "ExecutionStatusBlocked";
					break;
			}
			return display;
		}

		/// <summary>Gets the artifact token used for matching artifacts against strings in tools such as Subversion</summary>
		/// <param name="artifactPrefix">The two-letter prefix</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="trimZeros">Whethe ror not to trip zeroes afterwards.</param>
		/// <returns>The formed token</returns>
		public static string GetTokenForArtifact(string artifactPrefix, int artifactId, bool trimZeros = false)
		{
			string retValue = "";
			if (trimZeros)
				retValue = "[" + artifactPrefix + ":" + String.Format(FORMAT_SHORT_ID, artifactId) + "]";
			else
				retValue = "[" + artifactPrefix + ":" + String.Format(FORMAT_ID, artifactId) + "]";

			return retValue;
		}

		/// 
		/// Fixes up URLs that include the ~ starting character and expanding 
		/// to a full server relative path
		/// <param name="url">The URL to fix up</param>
		public static string xFixupUrl(string url)
		{
			if (url.StartsWith("~"))
			{
				return (HttpContext.Current.Request.ApplicationPath +
						url.Substring(1)).Replace("//", "/");
			}
			return url;
		}

		/// <summary>
		/// Constructs a querystring from a namevalue collection
		/// </summary>
		/// <param name="Params"></param>
		/// <returns></returns>
		public static string ConstructQueryString(this System.Collections.Specialized.NameValueCollection Params)
		{
			List<string> items = new List<string>();
			foreach (string name in Params)
				items.Add(String.Concat(name, "=", System.Web.HttpUtility.UrlEncode(Params[name])));
			return string.Join("&", items.ToArray());
		}

		/// <summary>
		/// Substring function that handles the case of a string that is too short safely
		/// </summary>
		/// <param name="input">The input string to be truncated</param>
		/// <param name="maxLength">The max-length to truncate to</param>
		/// <returns>The truncated string</returns>
		public static string SafeSubstring(string input, int maxLength)
		{
			if (input.Length > maxLength)
			{
				string output = input.Substring(0, maxLength);
				return output;
			}
			else
			{
				return input;
			}
		}

		/// <summary>
		/// Finds a specific sitemap node from its parent and URL
		/// </summary>
		/// <param name="url">The url to find</param>
		/// <param name="parent">The parent node</param>
		/// <returns>The matching node</returns>
		public static SiteMapNode FindSiteMapNode(string url, SiteMapNode parent)
		{
			SiteMapNode foundNode = null;

			if (parent.Url.ToLowerInvariant() == url.ToLowerInvariant())
				return parent;

			foreach (SiteMapNode child in parent.ChildNodes)
			{
				if (child.Url.ToLowerInvariant() == url.ToLowerInvariant())
					return child;

				if (child.HasChildNodes)
				{
					foundNode = FindSiteMapNode(url, child);
					if (foundNode != null)
					{
						break;
					}
				}
			}
			return foundNode;
		}

		/// <summary>
		/// Finds a specific sitemap node from its parent and value
		/// </summary>
		/// <param name="value">The value to find</param>
		/// <param name="parent">The parent node</param>
		/// <returns>The matching node</returns>
		public static SiteMapNode FindSiteMapNode(int value, SiteMapNode parent)
		{
			SiteMapNode foundNode = null;

			if (parent["value"] == value.ToString())
				return parent;

			foreach (SiteMapNode child in parent.ChildNodes)
			{
				if (child["value"] == value.ToString())
					return child;

				if (child.HasChildNodes)
					foundNode = FindSiteMapNode(value, child);
			}
			return foundNode;
		}

		/// <summary>
		/// Gets a simplifed sitemap tree from a given node
		/// </summary>
		/// <param name="node">The starting sitemap node to use</param>
		/// <returns>The matching node</returns>
		public static JSON_SiteMapNode GetFullJsonSiteMapNode(SiteMapNode node)
		{
			// set standard data on the new object 
			JSON_SiteMapNode newNode = new JSON_SiteMapNode()
			{
				Id = node["value"],
				Name = node.Title,
				Description = node.Description,
				Url = node.Url
			};
			// handle cases with child nodes
			if (node.HasChildNodes)
			{
				newNode.Children = new List<JSON_SiteMapNode>();
				foreach (SiteMapNode child in node.ChildNodes)
				{
					// check the child does not match the parent
					// to avoid infinite loops - in cases where a child's reference (and node) is the same as the parent
					if (child != node)
					{
						// call this method on the child if the passed in node has children
						JSON_SiteMapNode newChildNode = new JSON_SiteMapNode();
						newChildNode = GetFullJsonSiteMapNode(child);
						// add the child nodes to the children list
						newNode.Children.Add(newChildNode);
					}
				}
			}
			return newNode;
		}

		/// <summary>
		/// appends a string class to the html controls class attribute
		/// </summary>
		/// <param name="control"></param>
		/// <param name="newClass"></param>
		public static void AddClass(this HtmlControl control, string newClass)
		{
			if (!String.IsNullOrEmpty(control.Attributes["class"]))
			{
				control.Attributes["class"] += " " + newClass;
			}
			else
			{
				control.Attributes["class"] = newClass;
			}
		}

		/// <summary>
		/// recursively finds a child control of the specified parent.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Control FindControlRecursive(this Control control, string id)
		{
			if (control == null)
			{
				return null;
			}
			//try to find the control at the current level
			Control ctrl = control.FindControl(id);
			if (ctrl == null)
			{
				//search the children                 
				foreach (Control child in control.Controls)
				{
					ctrl = FindControlRecursive(child, id);
					if (ctrl != null) break;
				}
			}
			return ctrl;
		}

		/// <summary>
		/// Recursively gets all the child controls for a specific type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static List<T> GetAllControlsByType<T>(this Control parent)
		{
			var query = parent.GetAllControls()
								.OfType<T>();
			return query.ToList();
		}

		/// <summary>
		/// Recursively gets all the child controls for a specific type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static List<T> GetAllControlsByType<T>(this Page page)
		{
			var query = page.Form.GetAllControls()
								.OfType<T>();
			return query.ToList();
		}


		/// <summary>
		/// recursively finds all the child controls of a control
		/// </summary>
		/// <param name="control"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static IEnumerable<Control> GetAllControls(this Control parent)
		{
			foreach (Control control in parent.Controls)
			{
				yield return control;
				foreach (Control descendant in control.GetAllControls())
				{
					yield return descendant;
				}
			}
		}

		/// <summary>
		/// recursively finds a child control of the specified page.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Control FindControlRecursive(this Page page, string id)
		{
			if (page == null || id == null)
			{
				return null;
			}
			//try to find the control at the current level
			Control ctrl = page.FindControl(id);
			if (ctrl == null)
			{
				//search the children                 
				foreach (Control child in page.Controls)
				{
					ctrl = FindControlRecursive(child, id);
					if (ctrl != null) break;
				}
			}
			return ctrl;
		}

		/// <summary>
		/// Renders HTML context as rich text with all the markup displayed.
		/// Used when loading rich-text content into TextBoxes that can support rich-text
		/// or into controls that can correctly display it. This needs to handle certain
		/// characters such as unclosed quotes and less-than/greater-than operators
		/// that can get confused with markup.
		/// </summary>
		/// <param name="input">The input text or HTML markup</param>
		/// <param name="containsMarkup">Does the input text contain markup</param>
		/// <returns>The safe to display version of the markup</returns>
		/// <remarks>
		/// This version will optionally convert \n to HTML BR tags when the input string itself doesn't contain HTML markup
		/// </remarks>
		public static string HtmlRenderAsRichText(string input, bool containsMarkup = true)
		{
			if (containsMarkup)
			{
				//Need to scrub out any dangerous <script> tags
				return HtmlScrubInput(input);
			}
			else
			{
				//If the input is just plain text then we need to handle newlines correctly
				string output = input.Replace("\n", "<br />");
				return output;
			}
		}

		/// <summary>
		/// Truncates the name of an entity to allow display in navigation, etc.
		/// </summary>
		/// <param name="name">The input string to be truncated</param>
		/// <returns>The truncated string</returns>
		/// <remarks>Currently truncates to 40 characters and adds ...</remarks>
		public static string TruncateName(string name)
		{
			return TruncateName(name, 40);
		}

		/// <summary>
		/// Truncates the name of an entity to allow display in navigation, etc.
		/// </summary>
		/// <param name="name">The input string to be truncated</param>
		/// <param name="length">The length to truncate to</param>
		/// <returns>The truncated string</returns>
		/// <remarks>Converts any markup to plain text first</remarks>
		public static string TruncateName(string name, int length)
		{
			//Convert to plain text first
			name = HtmlRenderAsPlainText(name);

			if (name.Length > length)
			{
				string truncatedName = name.Substring(0, length) + "...";
				return truncatedName;
			}
			else
			{
				return name;
			}
		}

		/// <summary>
		/// Creates a navigatable URL. If we pass in http://www.x.com then it does nothing
		/// however if we pass www.x.com then it prepends http:// (by default)
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string FormNavigatableUrl(string url)
		{
			if (String.IsNullOrEmpty(url))
			{
				return "";
			}
			if (url.Contains("://"))
			{
				return url;
			}
			else
			{
				return "http://" + url;
			}
		}

		/// <summary>
		/// Displays the indent level with no-break spaces
		/// </summary>
		/// <param name="indentLevel">The level of indentation, where every level denoted by AAA's</param>
		/// <returns>The display string using &nbsp; characters</returns>
		public static string DisplayIndent(string indentLevel)
		{
			string display = "";
			int count;

			for (count = 1; count < indentLevel.Length / 3; count++)
			{
				display += "&nbsp;&nbsp;&nbsp;";
			}
			return display;
		}

		/// <summary>
		/// Displays a single indent level if flag passed in
		/// </summary>
		/// <param name="indent">Whether to indent one level or not</param>
		/// <returns>The display string using &nbsp; characters</returns>
		public static string DisplayIndent(bool indent)
		{
			string display;

			display = (indent) ? "&nbsp;&nbsp;&nbsp;" : "";
			return display;
		}

		/// <summary>
		/// Generates a new GUID used in screens where we have to enter a new GUID
		/// </summary>
		/// <returns></returns>
		public static string GenerateGuid()
		{
			return "{" + System.Guid.NewGuid().ToString().ToUpper() + "}";
		}

		/// <summary>
		/// Coverts effort values from minutes to fractional hours using the global format string
		/// </summary>
		/// <param name="minutes">Effort in minutes</param>
		/// <returns>The string form of the effort</returns>
		public static string GetEffortInFractionalHours(int minutes)
		{
			decimal fractionalHours = ((decimal)minutes) / (decimal)60;
			return String.Format("{0:0.0}", fractionalHours);
		}

		/// <summary>
		/// Retrieves a list of artifact type ids that a role can peform a permission on in the specified project
		/// </summary>
		/// <param name="projectRoleId">The id of the current role</param>
		/// <param name="permission">The permission the user needs to have (e.g. view)</param>
		/// <returns>The list of artifact type ids</returns>
		public static List<int> GetArtifactTypesForPermission(int projectRoleId, Project.PermissionEnum permission)
		{
			const string METHOD_NAME = "GetArtifactTypesForPermission";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//See if we have the IDs cached for this request (for performance)
			if (HttpContext.Current != null && HttpContext.Current.Items[PROJECT_ROLE_ARTIFACT_IDS + projectRoleId] != null)
			{
				return (List<int>)HttpContext.Current.Items[PROJECT_ROLE_ARTIFACT_IDS + projectRoleId];
			}

			List<int> artifactTypeIds = new List<int>();
			ProjectManager projectManager = new ProjectManager();
			ProjectRole projectRole = projectManager.RetrieveRolePermissions(projectRoleId);
			if (projectRole != null && projectRole.RolePermissions != null)
			{
				artifactTypeIds = projectRole.RolePermissions.Where(p => p.PermissionId == (int)permission).Select(p => p.ArtifactTypeId).ToList();
			}
			if (HttpContext.Current != null)
			{
				HttpContext.Current.Items[PROJECT_ROLE_ARTIFACT_IDS + projectRoleId] = artifactTypeIds;
			}

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			return artifactTypeIds;
		}

		/// <summary>
		/// Displays the the appropriate icon for the requirements list
		/// </summary>
		/// <param name="summaryYn">Whether the item is a summary item</param>
		/// <returns>The image icon filename to display</returns>
		public static string DisplayRequirementIcon(bool isSummary)
		{
			string iconFilename = URL_IMAGE_REQUIREMENT;

			if (isSummary)
			{
				iconFilename = URL_IMAGE_REQUIREMENT_SUMMARY;
			}

			return iconFilename;
		}

		/// <summary>
		/// Displays the the appropriate icon for the release list
		/// </summary>
		/// <param name="releaseType">The type of release</param>
		/// <returns>The image icon filename to display</returns>
		public static string DisplayReleaseIcon(Release.ReleaseTypeEnum releaseType)
		{
			string iconFilename = URL_IMAGE_RELEASE;

			if (releaseType == Release.ReleaseTypeEnum.Iteration || releaseType == Release.ReleaseTypeEnum.Phase)
			{
				iconFilename = URL_IMAGE_ITERATION;
			}

			return iconFilename;
		}

		/// <summary>
		/// Displays the the appropriate icon for the release list
		/// </summary>
		/// <param name="releaseTypeId">The id of the release type</param>
		/// <returns>The image icon filename to display</returns>
		public static string DisplayReleaseIcon(int releaseTypeId)
		{
			return DisplayReleaseIcon((Release.ReleaseTypeEnum)releaseTypeId);
		}

        /// <summary>
        /// Displays the size of a file in KB
        /// </summary>
        /// <param name="bytes">The size of the file in bytes</param>
        /// <returns>The friendly string format, rounded up (to avoid 0 KB)</returns>
        /// <remarks>
        /// The size is in bytes so we need to adjust to KB for display, always round up (e.g. 1.01KB = 2KB)
        /// </remarks>
        public static string DisplayFileSizeBytes(this int bytes)
        {
            string sizeInKB = ((int)(Math.Ceiling((decimal)bytes / 1024M))).ToString();
            return sizeInKB + " KB";
        }

        /// <summary>
        /// Displays the size of a file in KB
        /// </summary>
        /// <param name="kb">The size of the file in KB</param>
        /// <returns>The friendly string format</returns>
        public static string DisplayFileSizeKB(this int? kb)
        {
            if (kb.HasValue)
            {
                return kb.Value + " KB";
            }
            else
            {
                return "-";
            }
        }

        /// <summary>
        /// Displays the the appropriate icon for the test case list
        /// </summary>
        /// <param name="expandedYn">Whether the item should be expanded or not</param>
        /// <param name="folderYn">Whether the item is a folder item</param>
        /// <param name="testStepsYn">Whether the item has test steps or not</param>
        /// <returns>The image icon filename to display</returns>
        public static string DisplayTestCaseIcon(string expandedYn, string folderYn, string testStepsYn)
		{
			string iconFilename = URL_IMAGE_TEST_CASE;

			if (folderYn == "Y")
			{
				if (expandedYn == "Y")
				{
					iconFilename = URL_IMAGE_TEST_CASE_FOLDER_EXPAND;
				}
				else
				{
					iconFilename = URL_IMAGE_TEST_CASE_FOLDER;
				}
			}
			else if (testStepsYn == "N")
			{
				iconFilename = URL_IMAGE_TEST_CASE_NO_STEPS;
			}

			return iconFilename;
		}

		/// <summary>
		/// Displays the expansion icon if appropriate
		/// </summary>
		/// <param name="expandedYn">Whether the item should be expanded or not</param>
		/// <param name="summaryYn">Whether the item is a summary item</param>
		/// <returns>The image icon filename to display</returns>
		public static string DisplayExpand(string expandedYn, string summaryYn)
		{
			string iconFilename = "Images/ExpandedX.gif";

			if (summaryYn == "Y")
			{
				if (expandedYn == "Y")
				{
					iconFilename = URL_IMAGE_EXPANDED_Y;
				}
				else
				{
					iconFilename = URL_IMAGE_EXPANDED_N;
				}
			}

			return iconFilename;
		}

		/// <summary>
		/// Scrubs dangerous code from input HTML so that it can be persisted for use in the application
		/// </summary>
		/// <param name="input">The input from the user</param>
		/// <returns>The 'safe' HTML that can be persisted</returns>
		public static string HtmlScrubInput(string input)
		{
			//If input is null or whitspace, return it.
			if (string.IsNullOrWhiteSpace(input)) return input;

			//We need to scrub out any dangerous code from the HTML
			string result = input;
			try
			{
				// remove all scripts (prepare first by clearing attributes)
				result = System.Text.RegularExpressions.Regex.Replace(result,
					@"<( )*script([^>])*>", "<script>",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
					@"(<( )*(/)( )*script( )*>)", "</script>",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				//result = System.Text.RegularExpressions.Regex.Replace(result, 
				//         @"(<script>)([^(<script>\.</script>)])*(</script>)",
				//         string.Empty, 
				//         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				result = System.Text.RegularExpressions.Regex.Replace(result,
					@"(<script>).*(</script>)", string.Empty,
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				return result;
			}
			catch
			{
				return input;
			}
		}

        /// <summary>
        /// Converts Markdown to HTML
        /// </summary>
        public static string Md2Html(string md)
        {
            if (md == null)
            {
                return "";
            }

            CommonMark.CommonMarkSettings settings = CommonMark.CommonMarkSettings.Default.Clone();
            settings.OutputFormat = CommonMark.OutputFormat.Html;
            string html = CommonMark.CommonMarkConverter.Convert(md, settings);
            return html;
        }

        /// <summary>
        /// Renders HTML content as plain text, used to display titles, etc.
        /// </summary>
        /// <param name="source">The HTML markup</param>
        /// <returns>Plain text representation</returns>
        /// <remarks>Handles line-breaks, etc.</remarks>
        public static string HtmlRenderAsPlainText(string source)
		{
			return Strings.StripHTML(source);
		}

		/// <summary>
		/// Converts plain text so that it displays nicely as HTML
		/// </summary>
		/// <param name="plainText">The input plain text</param>
		/// <returns>The output html friendly text</returns>
		/// <remarks>Turns newlines into <br /> tags</remarks>
		public static string TextRenderAsHtml(string plainText)
		{
			if (plainText == null)
			{
				return plainText;
			}
			else
			{
				return plainText.Replace("\n", "<br />\n");
			}
		}

		/// <summary>Returns the two-letter prefix for a particular artifact type</summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <returns>The prefix that should be displayed</returns>
		public static string GetPrefixForArtifactType(DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			switch (artifactType)
			{
				case DataModel.Artifact.ArtifactTypeEnum.Requirement:
					return GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT;

				case DataModel.Artifact.ArtifactTypeEnum.Release:
					return GlobalFunctions.ARTIFACT_PREFIX_RELEASE;

				case DataModel.Artifact.ArtifactTypeEnum.Incident:
					return GlobalFunctions.ARTIFACT_PREFIX_INCIDENT;

				case DataModel.Artifact.ArtifactTypeEnum.Task:
					return GlobalFunctions.ARTIFACT_PREFIX_TASK;

				case DataModel.Artifact.ArtifactTypeEnum.TestRun:
					return GlobalFunctions.ARTIFACT_PREFIX_TEST_RUN;

				case DataModel.Artifact.ArtifactTypeEnum.TestCase:
					return GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE;

				case DataModel.Artifact.ArtifactTypeEnum.TestStep:
					return GlobalFunctions.ARTIFACT_PREFIX_TEST_STEP;

				case DataModel.Artifact.ArtifactTypeEnum.TestSet:
					return GlobalFunctions.ARTIFACT_PREFIX_TEST_SET;

				case DataModel.Artifact.ArtifactTypeEnum.AutomationEngine:
				case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
					return GlobalFunctions.ARTIFACT_PREFIX_AUTOMATION_HOST;

				case DataModel.Artifact.ArtifactTypeEnum.Project:
					return GlobalFunctions.ARTIFACT_PREFIX_PROJECT;

				case DataModel.Artifact.ArtifactTypeEnum.User:
					return GlobalFunctions.ARTIFACT_PREFIX_USER;

				case DataModel.Artifact.ArtifactTypeEnum.Document:
					return GlobalFunctions.ARTIFACT_PREFIX_DOCUMENT;

				case DataModel.Artifact.ArtifactTypeEnum.Risk:
					return GlobalFunctions.ARTIFACT_PREFIX_RISK;

                case DataModel.Artifact.ArtifactTypeEnum.Build:
                    return GlobalFunctions.ARTIFACT_PREFIX_BUILD;
            }
			return "";
		}

		/// <summary>Returns the two-letter prefix for a particular artifact type</summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <returns>The prefix that should be displayed</returns>
		public static string GetPrefixForArtifactType(int artifactTypeId)
		{
			DataModel.Artifact.ArtifactTypeEnum artType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;

			return GlobalFunctions.GetPrefixForArtifactType(artType);
		}


		/// <summary>
		/// This function decodes a column of a gridview that contains checkboxes and
		/// converts it into a list of the selected checkboxes' text property and item index.
		/// This usually contains the primary key identifier of the row
		/// </summary>
		/// <param name="gridView">The gridview containing the checkboxes</param>
		/// <param name="checkBoxCellIndex">The index of the cell containing the checkbox</param>
		/// <param name="checkBoxControlIndex">The index of the control containing the checkbox</param>
		/// <param name="resetControls">If set, it will un-check the check-boxes after reading</param>
		/// <returns>A collection of item IDs and index ids</returns>
		public static List<string> DecodeCheckBoxes(GridView gridView, int checkBoxCellIndex, int checkBoxControlIndex, bool resetControls)
		{
			List<string> checkBoxList = new List<string>();
			//Loop through the list of checkboxes, and add those that are checked to
			//the collection of selected items
			foreach (GridViewRow item in gridView.Rows)
			{
				CheckBoxEx checkBox = (CheckBoxEx)(item.Cells[checkBoxCellIndex].Controls[checkBoxControlIndex]);
				string primaryKey = checkBox.MetaData;
				if (checkBox.Checked)
				{
					//The value stored in the list is the requirement id and the row index, separated by colons
					string listValue = primaryKey + ":" + item.RowIndex.ToString();
					checkBoxList.Add(listValue);

					//Uncheck the box if requested
					if (resetControls)
					{
						checkBox.Checked = false;
					}
				}
			}
			return (checkBoxList);
		}

		/// <summary>
		/// Converts a collection of objects to a collection of strings that can be passed as JSON
		/// </summary>
		/// <param name="inputCollection">The collection of objects</param>
		/// <returns>The collection of strings</returns>
		/// <remarks>We encode the typecode as the first four characters of the key</remarks>
		public static JsonDictionaryOfStrings SerializeCollection(Dictionary<string, object> inputCollection)
		{
			JsonDictionaryOfStrings outputCollection = new JsonDictionaryOfStrings();
			foreach (KeyValuePair<string, object> entry in inputCollection)
			{
				if (entry.Value != null)
				{
					System.TypeCode typeCode;
					string stringValue = SerializeValue(entry.Value, out typeCode);
					string typeCodeString = ((int)typeCode).ToString("0000");
					outputCollection.Add(entry.Key, typeCodeString + stringValue);
				}
			}
			return outputCollection;
		}

		/// <summary>
		/// Converts a collection of strings (from JSON) to a collection of objects 
		/// </summary>
		/// <param name="inputCollection">The collection of strings</param>
		/// <returns>The collection of objects</returns>
		/// <remarks>We decode the typecode as the first four characters of the key</remarks>
		public static Dictionary<string, object> DeSerializeCollection(JsonDictionaryOfStrings inputCollection)
		{
			Dictionary<string, object> outputCollection = new Dictionary<string, object>();
			foreach (KeyValuePair<string, string> entry in inputCollection)
			{
				string typeCodeString = entry.Value.Substring(0, 4);
				string valueString = entry.Value.Substring(4, entry.Value.Length - 4);
				//Custom types are prefixed with an 'x' so that they're not confused for native .NET typecodes
				if (typeCodeString.Substring(0, 1).ToLowerInvariant() == "x")
				{
					object objectValue = DeSerializeValue(valueString, typeCodeString);
					outputCollection.Add(entry.Key, objectValue);
				}
				else
				{
					System.TypeCode typeCode = (System.TypeCode)Int32.Parse(typeCodeString);
					object objectValue = DeSerializeValue(valueString, typeCode);
					outputCollection.Add(entry.Key, objectValue);
				}
			}
			return outputCollection;
		}

		/// <summary>
		/// Converts the native object into a string and associated type-code
		/// </summary>
		/// <param name="entryValue">The native object value</param>
		/// <param name="typeCode">The type code of the native object [out]</param>
		/// <returns>The string representation of the object</returns>
		public static string SerializeValue(object entryValue, out TypeCode typeCode)
		{
			//First we need to get the typecode of the object
			typeCode = System.Type.GetTypeCode(entryValue.GetType());

			//Next we need to convert the value to string. Need to handle date-time differently
			if (typeCode == TypeCode.DateTime)
			{
				return ((DateTime)entryValue).ToString("yyyyMMddTHHmmss");
			}
			else
			{
				return entryValue.ToString();
			}
		}

		/// <summary>
		/// Converts a string representation into the native object
		/// </summary>
		/// <param name="valueWithTypeCode">The string version of the data prefixed by the typecode</param>
		/// <returns>The data in its native form</returns>
		public static object DeSerializeValue(string valueWithTypeCode)
		{
			if (valueWithTypeCode.Length < 5)
			{
				return null;
			}
			string typeCodeString = valueWithTypeCode.Substring(0, 4);
			string valueString = valueWithTypeCode.Substring(4, valueWithTypeCode.Length - 4);
			System.TypeCode typeCode = (System.TypeCode)Int32.Parse(typeCodeString);
			return DeSerializeValue(valueString, typeCode);
		}

		/// <summary>
		/// Converts a string representation into the native object
		/// </summary>
		/// <param name="entryValue">The string version of the data</param>
		/// <param name="typeCode">The type of the object we want</param>
		/// <returns>The data in its native form</returns>
		public static object DeSerializeValue(string entryValue, TypeCode typeCode)
		{
			if (typeCode == TypeCode.Boolean)
			{
				return Boolean.Parse(entryValue);
			}
			else if (typeCode == TypeCode.Int32)
			{
				return Int32.Parse(entryValue);
			}
			else if (typeCode == TypeCode.Int16)
			{
				return Int16.Parse(entryValue);
			}
			else if (typeCode == TypeCode.Int64)
			{
				return Int64.Parse(entryValue);
			}
			else if (typeCode == TypeCode.DateTime)
			{
				return DateTime.ParseExact(entryValue, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			}
			else
			{
				//Keep as a string value
				return entryValue;
			}
		}

		/// <summary>
		/// Deserializes a custom data-type from string to native object
		/// </summary>
		/// <param name="entryValue">The value</param>
		/// <param name="customPrefix">The type prefix</param>
		/// <returns>The native object</returns>
		public static object DeSerializeValue(string entryValue, string customPrefix)
		{
			if (customPrefix == "x-dr")
			{
				//See if we have a custom typecode
				DateRange result;
				DateRange.TryParse(entryValue, out result);
				return result;
			}
			else
			{
				//Keep as string
				return entryValue;
			}
		}

		/// <summary>Determines if a string value is an integer or not. Nulls and empty strings are NOT numeric.</summary>
		/// <param name="theValue">The string representation</param>
		/// <returns>True if integer, false if not.</returns>
		public static bool IsInteger(string theValue)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(theValue))
				{
					return theValue.All<char>(char.IsNumber);
				}
				else
					return false;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>Determines if an input string is a valid date</summary>
		/// <param name="dateString">The string containing the date</param>
		/// <returns>TRUE if the date is valid</returns>
		public static bool IsValidDate(string dateString)
		{
			try
			{
				DateTime dateParsed;
				return DateTime.TryParse(dateString, out dateParsed);
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
