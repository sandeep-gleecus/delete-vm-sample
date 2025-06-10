/* ---------------------------------------------------------------------- */
/* Script generated with: DeZign for Databases V9.0.0                     */
/* Target DBMS:           MS SQL Server 2008                              */
/* Project file:          tst_schema.dez                                  */
/* Project name:          SpiraTest Database Schema                       */
/* Author:                Inflectra Corporation                           */
/* Script type:           Database creation script                        */
/* Created on:            2021-04-27 13:28                                */
/* ---------------------------------------------------------------------- */


/* ---------------------------------------------------------------------- */
/* Add tables                                                             */
/* ---------------------------------------------------------------------- */

GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_STATUS"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_STATUS] (
    [TASK_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [XPKTST_TASK_STATUS] PRIMARY KEY ([TASK_STATUS_ID])
)
GO



/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_COLLECTION"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_COLLECTION] (
    [PROJECT_COLLECTION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [ACTIVE_YN] CHAR(1) NOT NULL,
    CONSTRAINT [XPKTST_PROJECT_COLLECTION] PRIMARY KEY ([PROJECT_COLLECTION_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_SETTING"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_SETTING] (
    [GLOBAL_SETTING_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    CONSTRAINT [XPKTST_GLOBAL_SETTING] PRIMARY KEY ([GLOBAL_SETTING_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TST_GLOBAL_SETTING_NAME] ON [TST_GLOBAL_SETTING] ([NAME])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_TYPE"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_TYPE] (
    [ARTIFACT_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [PREFIX] NVARCHAR(2) NOT NULL,
    [IS_NOTIFY] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DATA_SYNC] BIT NOT NULL,
    [IS_ATTACHMENTS] BIT NOT NULL,
    [IS_CUSTOM_PROPERTIES] BIT NOT NULL,
    [IS_GLOBAL_ITEM] BIT NOT NULL,
    CONSTRAINT [XPKTST_ARTIFACT_TYPE] PRIMARY KEY ([ARTIFACT_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_ROLE"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_ROLE] (
    [PROJECT_ROLE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ADMIN] BIT NOT NULL,
    [IS_DISCUSSIONS_ADD] BIT NOT NULL,
    [IS_SOURCE_CODE_VIEW] BIT NOT NULL,
    [IS_SOURCE_CODE_EDIT] BIT NOT NULL,
    [IS_DOCUMENT_FOLDERS_EDIT] BIT NOT NULL,
    [IS_LIMITED_VIEW] BIT NOT NULL,
    [IS_TEMPLATE_ADMIN] BIT NOT NULL,
    CONSTRAINT [XPKTST_PROJECT_ROLE] PRIMARY KEY ([PROJECT_ROLE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_STATUS"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_STATUS] (
    [REQUIREMENT_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [XPKTST_SCOPE_LEVEL] PRIMARY KEY ([REQUIREMENT_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_EXECUTION_STATUS"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_EXECUTION_STATUS] (
    [EXECUTION_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [XPKTST_EXECUTION_STATUS] PRIMARY KEY ([EXECUTION_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_RUN_TYPE"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_RUN_TYPE] (
    [TEST_RUN_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [XPKTST_TEST_RUN_TYPE] PRIMARY KEY ([TEST_RUN_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW_TRANSITION_ROLE_TYPE"                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW_TRANSITION_ROLE_TYPE] (
    [WORKFLOW_TRANSITION_ROLE_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [XPKTST_WORKFLOW_TRANSITION_ROLE_TYPE] PRIMARY KEY ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW_FIELD_STATE"                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW_FIELD_STATE] (
    [WORKFLOW_FIELD_STATE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [XPKTST_WORKFLOW_FIELD_STATE] PRIMARY KEY ([WORKFLOW_FIELD_STATE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PERMISSION"                                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PERMISSION] (
    [PERMISSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(40) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [XPKTST_PERMISSION] PRIMARY KEY ([PERMISSION_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_ARTIFACT_USER_TYPE"                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_ARTIFACT_USER_TYPE] (
    [PROJECT_ARTIFACT_NOTIFY_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(40) NOT NULL,
    [ACTIVE_YN] CHAR(1) NOT NULL,
    CONSTRAINT [PK_TST_NOTIFICATION_ARTIFACT_USER_TYPE] PRIMARY KEY ([PROJECT_ARTIFACT_NOTIFY_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_COLLECTION"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_COLLECTION] (
    [USER_COLLECTION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [ACTIVE_YN] CHAR(1) NOT NULL,
    CONSTRAINT [PK_TST_USER_COLLECTION] PRIMARY KEY ([USER_COLLECTION_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DATA_SYNC_STATUS"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DATA_SYNC_STATUS] (
    [DATA_SYNC_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(40) NOT NULL,
    [ACTIVE_YN] CHAR(20) NOT NULL,
    CONSTRAINT [PK_TST_DATA_SYNC_STATUS] PRIMARY KEY ([DATA_SYNC_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_SET_STATUS"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_SET_STATUS] (
    [TEST_SET_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_TEST_SET_STATUS] PRIMARY KEY ([TEST_SET_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PRODUCT_TYPE"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PRODUCT_TYPE] (
    [PRODUCT_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [PRODUCT_LICENSE_NUMBER] INTEGER NOT NULL,
    [ACTIVE_YN] CHAR(1) NOT NULL,
    CONSTRAINT [PK_TST_PRODUCT_TYPE] PRIMARY KEY ([PRODUCT_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_FIELD_TYPE"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_FIELD_TYPE] (
    [ARTIFACT_FIELD_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_ARTIFACT_FIELD_TYPE] PRIMARY KEY ([ARTIFACT_FIELD_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ATTACHMENT_TYPE"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ATTACHMENT_TYPE] (
    [ATTACHMENT_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(80) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_ATTACHMENT_TYPE] PRIMARY KEY ([ATTACHMENT_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_ROLE"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_ROLE] (
    [PROJECT_GROUP_ROLE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [ACTIVE_YN] CHAR(1) NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GROUP_ROLE] PRIMARY KEY ([PROJECT_GROUP_ROLE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DASHBOARD"                                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DASHBOARD] (
    [DASHBOARD_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PATH] NVARCHAR(256),
    [LOWERED_PATH] NVARCHAR(256),
    CONSTRAINT [PK_TST_DASHBOARD] PRIMARY KEY ([DASHBOARD_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DASHBOARD_GLOBAL_PERSONALIZATION"                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DASHBOARD_GLOBAL_PERSONALIZATION] (
    [DASHBOARD_ID] INTEGER NOT NULL,
    [PAGE_SETTINGS] IMAGE NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_DASHBOARD_GLOBAL_PERSONALIZATION] PRIMARY KEY ([DASHBOARD_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_SECTION"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_SECTION] (
    [REPORT_SECTION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER,
    [TOKEN] NVARCHAR(20) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [DEFAULT_TEMPLATE] NVARCHAR(max) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_REPORT_SECTION] PRIMARY KEY ([REPORT_SECTION_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_SECTION_1] ON [TST_REPORT_SECTION] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_FORMAT"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_FORMAT] (
    [REPORT_FORMAT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TOKEN] NVARCHAR(20) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [ICON_FILENAME] NVARCHAR(50) NOT NULL,
    [CONTENT_TYPE] NVARCHAR(100) NOT NULL,
    [CONTENT_DISPOSITION] NVARCHAR(100),
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_REPORT_FORMAT] PRIMARY KEY ([REPORT_FORMAT_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_ELEMENT"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_ELEMENT] (
    [REPORT_ELEMENT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TOKEN] NVARCHAR(20) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER,
    CONSTRAINT [PK_TST_REPORT_ELEMENT] PRIMARY KEY ([REPORT_ELEMENT_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_ELEMENT_1] ON [TST_REPORT_ELEMENT] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_VERSION_CONTROL_SYSTEM"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_VERSION_CONTROL_SYSTEM] (
    [VERSION_CONTROL_SYSTEM_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [CONNECTION_STRING] NVARCHAR(255) NOT NULL,
    [LOGIN] NVARCHAR(255) NOT NULL,
    [PASSWORD] NVARCHAR(max) NOT NULL,
    [DOMAIN] VARCHAR(50),
    [CUSTOM_01] NVARCHAR(50),
    [CUSTOM_02] NVARCHAR(50),
    [CUSTOM_03] NVARCHAR(50),
    [CUSTOM_04] NVARCHAR(50),
    [CUSTOM_05] NVARCHAR(50),
    [IS_ENCRYPTED] BIT CONSTRAINT [DEF_TST_VERSION_CONTROL_SYSTEM_IS_ENCRYPTED] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_TST_VERSION_CONTROL_SYSTEM] PRIMARY KEY ([VERSION_CONTROL_SYSTEM_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_AUTOMATION_ENGINE"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_AUTOMATION_ENGINE] (
    [AUTOMATION_ENGINE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [TOKEN] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_AUTOMATION_ENGINE_IS_DELETED] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_TST_AUTOMATION_ENGINE] PRIMARY KEY ([AUTOMATION_ENGINE_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TST_AUTOMATION_ENGINE_TOKEN] ON [TST_AUTOMATION_ENGINE] ([TOKEN])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_HISTORY_CHANGESET_TYPE"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_HISTORY_CHANGESET_TYPE] (
    [CHANGETYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [CHANGE_NAME] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_TST_HISTORY_CHANGESET_TYPE] PRIMARY KEY ([CHANGETYPE_ID])
)
GO


CREATE  INDEX [AK_TST_HISTORY_CHANGESET_TYPE_1] ON [TST_HISTORY_CHANGESET_TYPE] ([CHANGETYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_FILETYPES"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_FILETYPES] (
    [FILETYPE_ID] INTEGER IDENTITY(0,1) NOT NULL,
    [FILETYPE_MIME] NVARCHAR(255) NOT NULL,
    [FILETYPE_ICON] NVARCHAR(255) NOT NULL,
    [FILE_EXTENSION] NVARCHAR(15) NOT NULL,
    [FILE_DESCRIPTION] NVARCHAR(32) NOT NULL,
    CONSTRAINT [PK_TST_GLOBAL_FILETYPES] PRIMARY KEY ([FILETYPE_ID]),
    CONSTRAINT [TUC_TST_GLOBAL_FILETYPES_1] UNIQUE ([FILE_EXTENSION])
)
GO


CREATE  INDEX [AK_TST_GLOBAL_FILETYPES_1] ON [TST_GLOBAL_FILETYPES] ([FILETYPE_ID],[FILE_EXTENSION])
GO


CREATE  INDEX [AK_TST_GLOBAL_FILETYPES_2] ON [TST_GLOBAL_FILETYPES] ([FILE_EXTENSION])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GRAPH_TYPE"                                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GRAPH_TYPE] (
    [GRAPH_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_GRAPH_TYPE] PRIMARY KEY ([GRAPH_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_SOURCE_CODE_FILE"                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_SOURCE_CODE_FILE] (
    [ARTIFACT_SOURCE_CODE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [FILE_KEY] NVARCHAR(255) NOT NULL,
    [COMMENT] NVARCHAR(255),
    [CREATION_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_ARTIFACT_SOURCE_CODE_FILE] PRIMARY KEY ([ARTIFACT_SOURCE_CODE_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TST_ARTIFACT_SOURCE_CODE_FILE_1] ON [TST_ARTIFACT_SOURCE_CODE_FILE] ([ARTIFACT_ID],[FILE_KEY])
GO


CREATE  INDEX [AK_TST_ARTIFACT_SOURCE_CODE_FILE_2] ON [TST_ARTIFACT_SOURCE_CODE_FILE] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RECURRENCE"                                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RECURRENCE] (
    [RECURRENCE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_RECURRENCE] PRIMARY KEY ([RECURRENCE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_BUILD_STATUS"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_BUILD_STATUS] (
    [BUILD_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_BUILD_STATUS] PRIMARY KEY ([BUILD_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_EVENT_TYPE"                                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_EVENT_TYPE] (
    [EVENT_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_EVENT_TYPE] PRIMARY KEY ([EVENT_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_CUSTOM_PROPERTY_OPTION"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_CUSTOM_PROPERTY_OPTION] (
    [CUSTOM_PROPERTY_OPTION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_CUSTOM_PROPERTY_OPTION] PRIMARY KEY ([CUSTOM_PROPERTY_OPTION_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_RUN_FORMAT"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_RUN_FORMAT] (
    [TEST_RUN_FORMAT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_TEST_RUN_FORMAT] PRIMARY KEY ([TEST_RUN_FORMAT_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TVAULT_TYPE"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TVAULT_TYPE] (
    [TVAULT_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [TOKEN] NCHAR(3) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_TVAULT_TYPE] PRIMARY KEY ([TVAULT_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_SOURCE_CODE_REVISION"                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_SOURCE_CODE_REVISION] (
    [ARTIFACT_SOURCE_CODE_REVISION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [REVISION_KEY] NVARCHAR(255) NOT NULL,
    [COMMENT] NVARCHAR(255),
    [CREATION_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_ARTIFACT_SOURCE_CODE_REVISION] PRIMARY KEY ([ARTIFACT_SOURCE_CODE_REVISION_ID])
)
GO


CREATE  INDEX [AK_TST_ARTIFACT_SOURCE_CODE_REVISION_1] ON [TST_ARTIFACT_SOURCE_CODE_REVISION] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_LINK_TYPE"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_LINK_TYPE] (
    [ARTIFACT_LINK_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(40) NOT NULL,
    [REVERSE_NAME] NVARCHAR(40) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_ARTIFACT_LINK_TYPE] PRIMARY KEY ([ARTIFACT_LINK_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_SESSION"                                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_SESSION] (
    [SESSION_ID] NVARCHAR(40) NOT NULL,
    [EXPIRES] DATETIME NOT NULL,
    [STATIC_OBJECTS] NVARCHAR(max),
    [ITEMS] NVARCHAR(max),
    CONSTRAINT [PK_TST_SESSION] PRIMARY KEY ([SESSION_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_STATUS"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_STATUS] (
    [RELEASE_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_RELEASE_STATUS] PRIMARY KEY ([RELEASE_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_TYPE"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_TYPE] (
    [RELEASE_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_RELEASE_TYPE] PRIMARY KEY ([RELEASE_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_STATUS"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_STATUS] (
    [TEST_CASE_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [POSITION] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_CASE_STATUS] PRIMARY KEY ([TEST_CASE_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_SETTING_SECURE"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_SETTING_SECURE] (
    [GLOBAL_SETTING_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    [IS_ENCRYPTED] BIT NOT NULL,
    CONSTRAINT [PK_TST_GLOBAL_SETTING_SECURE] PRIMARY KEY ([GLOBAL_SETTING_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GRAPH_CUSTOM"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GRAPH_CUSTOM] (
    [GRAPH_ID] INTEGER IDENTITY(1000,1) NOT NULL,
    [GRAPH_TYPE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [QUERY] NVARCHAR(max) NOT NULL,
    CONSTRAINT [PK_TST_GRAPH_CUSTOM] PRIMARY KEY ([GRAPH_ID])
)
GO


CREATE  INDEX [IDX_TST_GRAPH_CUSTOM_1_FK] ON [TST_GRAPH_CUSTOM] ([GRAPH_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_SETTING"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_SETTING] (
    [PROJECT_SETTING_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_SETTING] PRIMARY KEY ([PROJECT_SETTING_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DASHBOARD_CUSTOM_TYPE"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DASHBOARD_CUSTOM_TYPE] (
    [DASHBOARD_CUSTOM_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_DASHBOARD_CUSTOM_TYPE] PRIMARY KEY ([DASHBOARD_CUSTOM_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DASHBOARD_CUSTOM"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DASHBOARD_CUSTOM] (
    [DASHBOARD_CUSTOM_ID] INTEGER NOT NULL,
    [DASHBOARD_CUSTOM_TYPE_ID] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [CODE] NVARCHAR(max),
    [AUTHOR] NVARCHAR(128),
    [COPYRIGHT] NVARCHAR(128),
    [USAGE] NVARCHAR(128),
    CONSTRAINT [PK_TST_DASHBOARD_CUSTOM] PRIMARY KEY ([DASHBOARD_CUSTOM_ID])
)
GO


CREATE  INDEX [IDX_TST_DASHBOARD_CUSTOM_1_FK] ON [TST_DASHBOARD_CUSTOM] ([DASHBOARD_CUSTOM_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DASHBOARD_CUSTOM_PERMISSION"                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DASHBOARD_CUSTOM_PERMISSION] (
    [DASHBOARD_CUSTOM_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [PERMISSION_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_DASHBOARD_CUSTOM_PERMISSION] PRIMARY KEY ([DASHBOARD_CUSTOM_ID], [ARTIFACT_TYPE_ID], [PERMISSION_ID])
)
GO


CREATE  INDEX [IDX_TST_DASHBOARD_CUSTOM_PERMISSION_1_FK] ON [TST_DASHBOARD_CUSTOM_PERMISSION] ([DASHBOARD_CUSTOM_ID])
GO


CREATE  INDEX [IDX_TST_DASHBOARD_CUSTOM_PERMISSION_2_FK] ON [TST_DASHBOARD_CUSTOM_PERMISSION] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_DASHBOARD_CUSTOM_PERMISSION_3_FK] ON [TST_DASHBOARD_CUSTOM_PERMISSION] ([PERMISSION_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_TEMPLATE"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_TEMPLATE] (
    [PROJECT_TEMPLATE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    PRIMARY KEY ([PROJECT_TEMPLATE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DOCUMENT_WORKFLOW"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DOCUMENT_WORKFLOW] (
    [DOCUMENT_WORKFLOW_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    CONSTRAINT [PK_TST_DOCUMENT_WORKFLOW] PRIMARY KEY ([DOCUMENT_WORKFLOW_ID])
)
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_1_FK] ON [TST_DOCUMENT_WORKFLOW] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DOCUMENT_STATUS"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DOCUMENT_STATUS] (
    [DOCUMENT_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_OPEN_STATUS] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    CONSTRAINT [PK_TST_DOCUMENT_STATUS] PRIMARY KEY ([DOCUMENT_STATUS_ID])
)
GO


CREATE  INDEX [IDX_TST_DOCUMENT_STATUS_1_FK] ON [TST_DOCUMENT_STATUS] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_PROBABILITY"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_PROBABILITY] (
    [RISK_PROBABILITY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(40) NOT NULL,
    [COLOR] CHAR(6) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [SCORE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_PROBABILITY] PRIMARY KEY ([RISK_PROBABILITY_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_PROBABILITY_1_FK] ON [TST_RISK_PROBABILITY] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_IMPACT"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_IMPACT] (
    [RISK_IMPACT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(40) NOT NULL,
    [COLOR] CHAR(6) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [SCORE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_IMPACT] PRIMARY KEY ([RISK_IMPACT_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_IMPACT_1_FK] ON [TST_RISK_IMPACT] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_OAUTH_PROVIDERS"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_OAUTH_PROVIDERS] (
    [OAUTH_PROVIDER_ID] UNIQUEIDENTIFIER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max) NOT NULL,
    [URL_AUTHORIZATION] NVARCHAR(255),
    [URL_TOKEN] NVARCHAR(255),
    [URL_PROFILE] NVARCHAR(255),
    [CLIENT_ID] NVARCHAR(max),
    [CLIENT_SECRET] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_LOADED] BIT NOT NULL,
    [IMAGE_URL] NVARCHAR(255),
    [IMAGE_DATA] NVARCHAR(max),
    [CUSTOM_1] NVARCHAR(255),
    [CUSTOM_2] NVARCHAR(255),
    [CUSTOM_3] NVARCHAR(255),
    [CUSTOM_4] NVARCHAR(255),
    [CUSTOM_5] NVARCHAR(255),
    CONSTRAINT [PK_TST_GLOBAL_OAUTH_PROVIDERS] PRIMARY KEY ([OAUTH_PROVIDER_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_STATUS"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_STATUS] (
    [RISK_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_OPEN] BIT NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_STATUS] PRIMARY KEY ([RISK_STATUS_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_STATUS_1_FK] ON [TST_RISK_STATUS] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_WORKFLOW"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_WORKFLOW] (
    [RISK_WORKFLOW_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_RISK_WORKFLOW] PRIMARY KEY ([RISK_WORKFLOW_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_1_FK] ON [TST_RISK_WORKFLOW] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_WORKFLOW_TRANSITION"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_WORKFLOW_TRANSITION] (
    [WORKFLOW_TRANSITION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [RISK_WORKFLOW_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_EXECUTE_BY_CREATOR] BIT NOT NULL,
    [IS_EXECUTE_BY_OWNER] BIT NOT NULL,
    [IS_SIGNATURE_REQUIRED] BIT NOT NULL,
    [INPUT_RISK_STATUS_ID] INTEGER NOT NULL,
    [OUTPUT_RISK_STATUS_ID] INTEGER NOT NULL,
    [IS_BLANK_OWNER] BIT CONSTRAINT [DEF_TST_RISK_WORKFLOW_TRANSITION_IS_BLANK_OWNER] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_CREATOR] BIT CONSTRAINT [DEF_TST_RISK_WORKFLOW_TRANSITION_IS_NOTIFY_CREATOR] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_OWNER] BIT CONSTRAINT [DEF_TST_RISK_WORKFLOW_TRANSITION_IS_NOTIFY_OWNER] DEFAULT 0 NOT NULL,
    [NOTIFY_SUBJECT] NVARCHAR(128),
    CONSTRAINT [PK_TST_RISK_WORKFLOW_TRANSITION] PRIMARY KEY ([WORKFLOW_TRANSITION_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_TRANSITION_1_FK] ON [TST_RISK_WORKFLOW_TRANSITION] ([RISK_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_TRANSITION_2_FK] ON [TST_RISK_WORKFLOW_TRANSITION] ([INPUT_RISK_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_TRANSITION_3_FK] ON [TST_RISK_WORKFLOW_TRANSITION] ([OUTPUT_RISK_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_WORKFLOW_TRANSITION_ROLE"                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_WORKFLOW_TRANSITION_ROLE] (
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ROLE_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_WORKFLOW_TRANSITION_ROLE] PRIMARY KEY ([WORKFLOW_TRANSITION_ID], [WORKFLOW_TRANSITION_ROLE_TYPE_ID], [PROJECT_ROLE_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_TRANSITION_ROLE_1_FK] ON [TST_RISK_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_TRANSITION_ROLE_2_FK] ON [TST_RISK_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_TRANSITION_ROLE_3_FK] ON [TST_RISK_WORKFLOW_TRANSITION_ROLE] ([PROJECT_ROLE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_THEME_STATUS"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_THEME_STATUS] (
    [THEME_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_THEME_STATUS] PRIMARY KEY ([THEME_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_THEME_PRIORITY"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_THEME_PRIORITY] (
    [THEME_PRIORITY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [COLOR] CHAR(6) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [SCORE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_THEME_PRIORITY] PRIMARY KEY ([THEME_PRIORITY_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RESOURCE_CATEGORY"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RESOURCE_CATEGORY] (
    [RESOURCE_CATEGORY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [NOTIONAL_COST] DECIMAL,
    CONSTRAINT [PK_TST_RESOURCE_CATEGORY] PRIMARY KEY ([RESOURCE_CATEGORY_ID])
)
GO


CREATE  INDEX [IDX_TST_RESOURCE_CATEGORY_1_FK] ON [TST_RESOURCE_CATEGORY] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RESOURCE_TRACK"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RESOURCE_TRACK] (
    [RESOURCE_TRACK_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RESOURCE_TRACK] PRIMARY KEY ([RESOURCE_TRACK_ID])
)
GO


CREATE  INDEX [IDX_TST_RESOURCE_TRACK_1_FK] ON [TST_RESOURCE_TRACK] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TIMECARD_ENTRY_TYPE"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TIMECARD_ENTRY_TYPE] (
    [TIMECARD_ENTRY_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(40) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER,
    [DESCRIPTION] NVARCHAR(max),
    CONSTRAINT [PK_TST_TIMECARD_ENTRY_TYPE] PRIMARY KEY ([TIMECARD_ENTRY_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_TIMECARD_ENTRY_TYPE_1_FK] ON [TST_TIMECARD_ENTRY_TYPE] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_ARTIFACT_CUSTOM_PROPERTY"                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_ARTIFACT_CUSTOM_PROPERTY] (
    [ARTIFACT_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [CUST_01] NVARCHAR(max),
    [CUST_02] NVARCHAR(max),
    [CUST_03] NVARCHAR(max),
    [CUST_04] NVARCHAR(max),
    [CUST_05] NVARCHAR(max),
    [CUST_06] NVARCHAR(max),
    [CUST_07] NVARCHAR(max),
    [CUST_08] NVARCHAR(max),
    [CUST_09] NVARCHAR(max),
    [CUST_10] NVARCHAR(max),
    [CUST_11] NVARCHAR(max),
    [CUST_12] NVARCHAR(max),
    [CUST_13] NVARCHAR(max),
    [CUST_14] NVARCHAR(max),
    [CUST_15] NVARCHAR(max),
    [CUST_16] NVARCHAR(max),
    [CUST_17] NVARCHAR(max),
    [CUST_18] NVARCHAR(max),
    [CUST_19] NVARCHAR(max),
    [CUST_20] NVARCHAR(max),
    [CUST_21] NVARCHAR(max),
    [CUST_22] NVARCHAR(max),
    [CUST_23] NVARCHAR(max),
    [CUST_24] NVARCHAR(max),
    [CUST_25] NVARCHAR(max),
    [CUST_26] NVARCHAR(max),
    [CUST_27] NVARCHAR(max),
    [CUST_28] NVARCHAR(max),
    [CUST_29] NVARCHAR(max),
    [CUST_30] NVARCHAR(max),
    CONSTRAINT [XPKTST_GLOBAL_ARTIFACT_CUSTOM_PROPERTY] PRIMARY KEY ([ARTIFACT_ID], [ARTIFACT_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_GLOBAL_ARTIFACT_CUSTOM_PROPERTY_1_FK] ON [TST_GLOBAL_ARTIFACT_CUSTOM_PROPERTY] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_CUSTOM_PROPERTY_LIST"                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_CUSTOM_PROPERTY_LIST] (
    [CUSTOM_PROPERTY_LIST_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_SORTED_ON_VALUE] BIT NOT NULL,
    CONSTRAINT [PK_TST_GLOBAL_CUSTOM_PROPERTY_LIST] PRIMARY KEY ([CUSTOM_PROPERTY_LIST_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PORTFOLIO"                                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PORTFOLIO] (
    [PORTFOLIO_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [PERCENT_COMPLETE] INTEGER NOT NULL,
    [REQUIREMENT_COUNT] INTEGER CONSTRAINT [DEF_TST_PORTFOLIO_REQUIREMENT_COUNT] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_TST_PORTFOLIO] PRIMARY KEY ([PORTFOLIO_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT"                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT] (
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [DESCRIPTION] NVARCHAR(max) NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT] PRIMARY KEY ([PROJECT_TEMPLATE_ID], [ARTIFACT_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT_1_FK] ON [TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT] ([PROJECT_TEMPLATE_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT_2_FK] ON [TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_TAGS"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_TAGS] (
    [ARTIFACT_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [TAGS] NVARCHAR(max) NOT NULL,
    CONSTRAINT [PK_TST_GLOBAL_TAGS] PRIMARY KEY ([ARTIFACT_ID], [ARTIFACT_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_GLOBAL_TAGS_1_FK] ON [TST_GLOBAL_TAGS] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_MULTI_APPROVER_TYPE"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_MULTI_APPROVER_TYPE] (
    [MULTI_APPROVER_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER,
    [NAME] NVARCHAR(40) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_MULTI_APPROVER_TYPE] PRIMARY KEY ([MULTI_APPROVER_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_MULTI_APPROVER_TYPE_1_FK] ON [TST_MULTI_APPROVER_TYPE] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_TEMPLATE_SETTING"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_TEMPLATE_SETTING] (
    [PROJECT_TEMPLATE_SETTING_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_TEMPLATE_SETTING] PRIMARY KEY ([PROJECT_TEMPLATE_SETTING_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_TEMPLATE_SETTING_VALUE"                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_TEMPLATE_SETTING_VALUE] (
    [PROJECT_TEMPLATE_SETTING_ID] INTEGER NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_TEMPLATE_SETTING_VALUE] PRIMARY KEY ([PROJECT_TEMPLATE_SETTING_ID], [PROJECT_TEMPLATE_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_TEMPLATE_SETTING_VALUE_1_FK] ON [TST_PROJECT_TEMPLATE_SETTING_VALUE] ([PROJECT_TEMPLATE_SETTING_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_TEMPLATE_SETTING_VALUE_2_FK] ON [TST_PROJECT_TEMPLATE_SETTING_VALUE] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_SETTING"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_SETTING] (
    [PROJECT_GROUP_SETTING_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    PRIMARY KEY ([PROJECT_GROUP_SETTING_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PORTFOLIO_SETTING"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PORTFOLIO_SETTING] (
    [PORTFOLIO_SETTING_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    PRIMARY KEY ([PORTFOLIO_SETTING_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PORTFOLIO_SETTING_VALUE"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PORTFOLIO_SETTING_VALUE] (
    [PORTFOLIO_SETTING_ID] INTEGER NOT NULL,
    [PORTFOLIO_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    PRIMARY KEY ([PORTFOLIO_SETTING_ID], [PORTFOLIO_ID])
)
GO


CREATE  INDEX [IDX_TST_PORTFOLIO_SETTING_VALUE_1_FK] ON [TST_PORTFOLIO_SETTING_VALUE] ([PORTFOLIO_SETTING_ID])
GO


CREATE  INDEX [IDX_TST_PORTFOLIO_SETTING_VALUE_2_FK] ON [TST_PORTFOLIO_SETTING_VALUE] ([PORTFOLIO_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_DETECTABILITY"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_DETECTABILITY] (
    [RISK_DETECTABILITY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(40) NOT NULL,
    [COLOR] CHAR(6) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [SCORE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_DETECTABILITY] PRIMARY KEY ([RISK_DETECTABILITY_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_DETECTABILITY_1_FK] ON [TST_RISK_DETECTABILITY] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_STANDARD_TASK_SET"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_STANDARD_TASK_SET] (
    [STANDARD_TASK_SET_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_STANDARD_TASK_SET] PRIMARY KEY ([STANDARD_TASK_SET_ID])
)
GO


CREATE  INDEX [IDX_TST_STANDARD_TASK_SET_1_FK] ON [TST_STANDARD_TASK_SET] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_STANDARD_TEST_CASE_SET"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_STANDARD_TEST_CASE_SET] (
    [STANDARD_TEST_CASE_SET_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_STANDARD_TEST_CASE_SET] PRIMARY KEY ([STANDARD_TEST_CASE_SET_ID])
)
GO


CREATE  INDEX [IDX_TST_STANDARD_TEST_CASE_SET_1_FK] ON [TST_STANDARD_TEST_CASE_SET] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW_TRANSITION_STANDARD_TASK"                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW_TRANSITION_STANDARD_TASK] (
    [WORKFLOW_TRANSITION_STANDARD_TASK_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [STANDARD_TASK_SET_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [IS_INPUT_TRANSITION] BIT NOT NULL,
    CONSTRAINT [PK_TST_WORKFLOW_TRANSITION_STANDARD_TASK] PRIMARY KEY ([WORKFLOW_TRANSITION_STANDARD_TASK_ID])
)
GO


CREATE  INDEX [IDX_TST_WORKFLOW_TRANSITION_STANDARD_TASK_1_FK] ON [TST_WORKFLOW_TRANSITION_STANDARD_TASK] ([STANDARD_TASK_SET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW_TRANSITION_STANDARD_TEST_CASE"                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW_TRANSITION_STANDARD_TEST_CASE] (
    [WORKFLOW_TRANSITION_STANDARD_TEST_CASE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [STANDARD_TEST_CASE_SET_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [IS_INPUT_TRANSITION] BIT NOT NULL,
    CONSTRAINT [PK_TST_WORKFLOW_TRANSITION_STANDARD_TEST_CASE] PRIMARY KEY ([WORKFLOW_TRANSITION_STANDARD_TEST_CASE_ID])
)
GO


CREATE  INDEX [IDX_TST_WORKFLOW_TRANSITION_STANDARD_TEST_CASE_1_FK] ON [TST_WORKFLOW_TRANSITION_STANDARD_TEST_CASE] ([STANDARD_TEST_CASE_SET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKSPACE_TYPE"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKSPACE_TYPE] (
    [WORKSPACE_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_WORKSPACE_TYPE] PRIMARY KEY ([WORKSPACE_TYPE_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TIMECARD_STATUS"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TIMECARD_STATUS] (
    [TIMECARD_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_TIMECARD_STATUS] PRIMARY KEY ([TIMECARD_STATUS_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_PRIORITY"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_PRIORITY] (
    [TASK_PRIORITY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [COLOR] CHAR(6) NOT NULL,
    [SCORE] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_TASK_PRIORITY] PRIMARY KEY ([TASK_PRIORITY_ID])
)
GO


CREATE  INDEX [IDX_TST_TASK_PRIORITY_1_FK] ON [TST_TASK_PRIORITY] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW"                                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW] (
    [WORKFLOW_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(100) NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_NOTIFY] BIT NOT NULL,
    CONSTRAINT [XPKTST_WORKFLOW] PRIMARY KEY ([WORKFLOW_ID])
)
GO


CREATE  INDEX [IDX_TST_WORKFLOW_1_FK] ON [TST_WORKFLOW] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_ROLE_PERMISSION"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_ROLE_PERMISSION] (
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [PERMISSION_ID] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_PROJECT_ROLE_PERMISSION] PRIMARY KEY ([PROJECT_ROLE_ID], [ARTIFACT_TYPE_ID], [PERMISSION_ID])
)
GO


CREATE  INDEX [AK_TST_PROJECT_ROLE_PERMISSION_1] ON [TST_PROJECT_ROLE_PERMISSION] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_ROLE_PERMISSION_2] ON [TST_PROJECT_ROLE_PERMISSION] ([PROJECT_ROLE_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_ROLE_PERMISSION_3] ON [TST_PROJECT_ROLE_PERMISSION] ([PERMISSION_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_FIELD"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_FIELD] (
    [ARTIFACT_FIELD_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_FIELD_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [CAPTION] NVARCHAR(50) NOT NULL,
    [IS_WORKFLOW_CONFIG] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_LIST_CONFIG] BIT NOT NULL,
    [IS_LIST_DEFAULT] BIT NOT NULL,
    [LIST_DEFAULT_POSITION] INTEGER,
    [IS_DATA_MAPPING] BIT NOT NULL,
    [IS_REPORT] BIT NOT NULL,
    [IS_NOTIFY] BIT CONSTRAINT [DEF_TST_ARTIFACT_FIELD_IS_NOTIFY] DEFAULT 0 NOT NULL,
    [DESCRIPTION] NVARCHAR(255),
    [LOOKUP_PROPERTY] NVARCHAR(50),
    [IS_HISTORY_RECORDED] BIT NOT NULL,
    CONSTRAINT [XPKTST_ARTIFACT_FIELD] PRIMARY KEY ([ARTIFACT_FIELD_ID])
)
GO


CREATE  INDEX [AK_TST_ARTIFACT_FIELD_1] ON [TST_ARTIFACT_FIELD] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_ARTIFACT_FIELD_2] ON [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_INCIDENT_STATUS"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_INCIDENT_STATUS] (
    [INCIDENT_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_OPEN_STATUS] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    CONSTRAINT [XPKTST_INCIDENT_STATUS] PRIMARY KEY ([INCIDENT_STATUS_ID])
)
GO


CREATE  INDEX [IDX_TST_INCIDENT_STATUS_1_FK] ON [TST_INCIDENT_STATUS] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER"                                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER] (
    [USER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [USER_NAME] NVARCHAR(50) NOT NULL,
    [PASSWORD] NVARCHAR(128),
    [PASSWORD_SALT] NVARCHAR(128),
    [EMAIL_ADDRESS] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_LOGIN_DATE] DATETIME,
    [LAST_ACTIVITY_DATE] DATETIME,
    [LAST_LOCKOUT_DATE] DATETIME,
    [LAST_PASSWORD_CHANGED_DATE] DATETIME,
    [LDAP_DN] NVARCHAR(255),
    [RSS_TOKEN] NVARCHAR(50),
    [IS_APPROVED] BIT NOT NULL,
    [IS_LOCKED] BIT NOT NULL,
    [COMMENT] NVARCHAR(255),
    [PASSWORD_QUESTION] NVARCHAR(255),
    [PASSWORD_ANSWER] NVARCHAR(255),
    [PASSWORD_FORMAT] INTEGER NOT NULL,
    [FAILED_PASSWORD_ATTEMPT_COUNT] INTEGER NOT NULL,
    [FAILED_PASSWORD_ATTEMPT_WINDOW_START] DATETIME,
    [FAILED_PASSWORD_ANSWER_ATTEMPT_COUNT] INTEGER NOT NULL,
    [FAILED_PASSWORD_ANSWER_ATTEMPT_WINDOW_START] DATETIME,
    [IS_LEGACY_FORMAT] BIT NOT NULL,
    [OAUTH_ACCESS_TOKEN] NVARCHAR(255),
    [OAUTH_PROVIDER_ID] UNIQUEIDENTIFIER,
    [MFA_PHONE] NVARCHAR(255),
    [MFA_TOKEN] NVARCHAR(255),
    CONSTRAINT [XPKTST_USER] PRIMARY KEY ([USER_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TEST_USER_USER_NAME] ON [TST_USER] ([USER_NAME])
GO


CREATE  INDEX [IDX_TST_USER_2_FK] ON [TST_USER] ([OAUTH_PROVIDER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_IMPORTANCE"                                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_IMPORTANCE] (
    [IMPORTANCE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [COLOR] CHAR(6) NOT NULL,
    [SCORE] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_IMPORTANCE] PRIMARY KEY ([IMPORTANCE_ID])
)
GO


CREATE  INDEX [IDX_TST_IMPORTANCE_1_FK] ON [TST_IMPORTANCE] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_INCIDENT_SEVERITY"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_INCIDENT_SEVERITY] (
    [SEVERITY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [COLOR] NVARCHAR(6) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [SCORE] INTEGER,
    CONSTRAINT [XPKTST_INCIDENT_SEVERITY] PRIMARY KEY ([SEVERITY_ID])
)
GO


CREATE  INDEX [IDX_TST_INCIDENT_SEVERITY_1_FK] ON [TST_INCIDENT_SEVERITY] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_INCIDENT_PRIORITY"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_INCIDENT_PRIORITY] (
    [PRIORITY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [COLOR] NVARCHAR(6) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [SCORE] INTEGER,
    CONSTRAINT [XPKTST_INCIDENT_PRIORITY] PRIMARY KEY ([PRIORITY_ID])
)
GO


CREATE  INDEX [IDX_TST_INCIDENT_PRIORITY_1_FK] ON [TST_INCIDENT_PRIORITY] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_CUSTOM_PROPERTY_TYPE"                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_CUSTOM_PROPERTY_TYPE] (
    [CUSTOM_PROPERTY_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_FIELD_TYPE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [SYSTEM_TYPE] NVARCHAR(255),
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [XPKTST_CUSTOM_PROPERTY_TYPE] PRIMARY KEY ([CUSTOM_PROPERTY_TYPE_ID])
)
GO


CREATE  INDEX [AK_TST_CUSTOM_PROPERTY_TYPE_1] ON [TST_CUSTOM_PROPERTY_TYPE] ([ARTIFACT_FIELD_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_PRIORITY"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_PRIORITY] (
    [TEST_CASE_PRIORITY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [COLOR] CHAR(6) NOT NULL,
    [SCORE] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_TEST_CASE_PRIORITY] PRIMARY KEY ([TEST_CASE_PRIORITY_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_PRIORITY_1_FK] ON [TST_TEST_CASE_PRIORITY] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_LINK"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_LINK] (
    [ARTIFACT_LINK_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_LINK_TYPE_ID] INTEGER NOT NULL,
    [SOURCE_ARTIFACT_ID] INTEGER NOT NULL,
    [SOURCE_ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [DEST_ARTIFACT_ID] INTEGER NOT NULL,
    [DEST_ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [COMMENT] NVARCHAR(255),
    CONSTRAINT [PK_TST_ARTIFACT_LINK] PRIMARY KEY ([ARTIFACT_LINK_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TST_ARTIFACT_LINK_1] ON [TST_ARTIFACT_LINK] ([SOURCE_ARTIFACT_ID],[SOURCE_ARTIFACT_TYPE_ID],[DEST_ARTIFACT_ID],[DEST_ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_ARTIFACT_LINK_2] ON [TST_ARTIFACT_LINK] ([SOURCE_ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_ARTIFACT_LINK_3] ON [TST_ARTIFACT_LINK] ([DEST_ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_ARTIFACT_LINK_4] ON [TST_ARTIFACT_LINK] ([CREATOR_ID])
GO


CREATE  INDEX [AK_TST_ARTIFACT_LINK_5] ON [TST_ARTIFACT_LINK] ([ARTIFACT_LINK_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_COLLECTION_ENTRY"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_COLLECTION_ENTRY] (
    [USER_COLLECTION_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [ENTRY_KEY] NVARCHAR(255) NOT NULL,
    [ENTRY_VALUE] NVARCHAR(255) NOT NULL,
    [ENTRY_TYPE_CODE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_USER_COLLECTION_ENTRY] PRIMARY KEY ([USER_COLLECTION_ID], [USER_ID], [ENTRY_KEY])
)
GO


CREATE  INDEX [AK_TST_USER_COLLECTION_ENTRY_1] ON [TST_USER_COLLECTION_ENTRY] ([USER_COLLECTION_ID])
GO


CREATE  INDEX [AK_TST_USER_COLLECTION_ENTRY_2] ON [TST_USER_COLLECTION_ENTRY] ([USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DATA_SYNC_SYSTEM"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DATA_SYNC_SYSTEM] (
    [DATA_SYNC_SYSTEM_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [DATA_SYNC_STATUS_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [CONNECTION_STRING] NVARCHAR(255) NOT NULL,
    [EXTERNAL_LOGIN] NVARCHAR(50) NOT NULL,
    [EXTERNAL_PASSWORD] NVARCHAR(max),
    [TIME_OFFSET_HOURS] INTEGER NOT NULL,
    [LAST_SYNC_DATE] DATETIME,
    [CUSTOM_01] NVARCHAR(255),
    [CUSTOM_02] NVARCHAR(255),
    [CUSTOM_03] NVARCHAR(255),
    [CUSTOM_04] NVARCHAR(255),
    [CUSTOM_05] NVARCHAR(255),
    [AUTO_MAP_USERS_YN] CHAR(1) NOT NULL,
    [CAPTION] NVARCHAR(100),
    [IS_ENCRYPTED] BIT CONSTRAINT [DEF_TST_DATA_SYNC_SYSTEM_IS_ENCRYPTED] DEFAULT 0 NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_DATA_SYNC_SYSTEM] PRIMARY KEY ([DATA_SYNC_SYSTEM_ID])
)
GO


CREATE  INDEX [AK_TST_DATA_SYNC_SYSTEM_1] ON [TST_DATA_SYNC_SYSTEM] ([DATA_SYNC_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_CUSTOM_PROPERTY_LIST"                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_CUSTOM_PROPERTY_LIST] (
    [CUSTOM_PROPERTY_LIST_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_SORTED_ON_VALUE] BIT NOT NULL,
    CONSTRAINT [PK_TST_CUSTOM_PROPERTY_LIST] PRIMARY KEY ([CUSTOM_PROPERTY_LIST_ID])
)
GO


CREATE  INDEX [IDX_TST_CUSTOM_PROPERTY_LIST_1_FK] ON [TST_CUSTOM_PROPERTY_LIST] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DOCUMENT_TYPE"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DOCUMENT_TYPE] (
    [DOCUMENT_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [DOCUMENT_WORKFLOW_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    CONSTRAINT [PK_TST_DOCUMENT_TYPE] PRIMARY KEY ([DOCUMENT_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_DOCUMENT_TYPE_1_FK] ON [TST_DOCUMENT_TYPE] ([PROJECT_TEMPLATE_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_TYPE_2_FK] ON [TST_DOCUMENT_TYPE] ([DOCUMENT_WORKFLOW_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DATA_SYNC_USER_MAPPING"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DATA_SYNC_USER_MAPPING] (
    [DATA_SYNC_SYSTEM_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [EXTERNAL_KEY] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_TST_DATA_SYNC_USER_MAPPING] PRIMARY KEY ([DATA_SYNC_SYSTEM_ID], [USER_ID])
)
GO


CREATE  INDEX [AK_TST_DATA_SYNC_USER_MAPPING_1] ON [TST_DATA_SYNC_USER_MAPPING] ([DATA_SYNC_SYSTEM_ID])
GO


CREATE  INDEX [AK_TST_DATA_SYNC_USER_MAPPING_2] ON [TST_DATA_SYNC_USER_MAPPING] ([USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP] (
    [PROJECT_GROUP_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [WEBSITE] NVARCHAR(255),
    [PROJECT_TEMPLATE_ID] INTEGER,
    [PERCENT_COMPLETE] INTEGER NOT NULL,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [PORTFOLIO_ID] INTEGER,
    [REQUIREMENT_COUNT] INTEGER CONSTRAINT [DEF_TST_PROJECT_GROUP_REQUIREMENT_COUNT] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GROUP] PRIMARY KEY ([PROJECT_GROUP_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_1_FK] ON [TST_PROJECT_GROUP] ([PROJECT_TEMPLATE_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_2_FK] ON [TST_PROJECT_GROUP] ([PORTFOLIO_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_USER"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_USER] (
    [PROJECT_GROUP_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [PROJECT_GROUP_ROLE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GROUP_USER] PRIMARY KEY ([PROJECT_GROUP_ID], [USER_ID], [PROJECT_GROUP_ROLE_ID])
)
GO


CREATE  INDEX [AK_TST_PROJECT_GROUP_USER_1] ON [TST_PROJECT_GROUP_USER] ([PROJECT_GROUP_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_GROUP_USER_2] ON [TST_PROJECT_GROUP_USER] ([USER_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_GROUP_USER_3] ON [TST_PROJECT_GROUP_USER] ([PROJECT_GROUP_ROLE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DASHBOARD_USER_PERSONALIZATION"                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DASHBOARD_USER_PERSONALIZATION] (
    [DASHBOARD_USER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [DASHBOARD_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [PAGE_SETTINGS] IMAGE NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_DASHBOARD_USER_PERSONALIZATION] PRIMARY KEY ([DASHBOARD_USER_ID])
)
GO


CREATE  INDEX [AK_TST_DASHBOARD_USER_PERSONALIZATION_1] ON [TST_DASHBOARD_USER_PERSONALIZATION] ([DASHBOARD_ID])
GO


CREATE  INDEX [AK_TST_DASHBOARD_USER_PERSONALIZATION_2] ON [TST_DASHBOARD_USER_PERSONALIZATION] ([USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_CATEGORY"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_CATEGORY] (
    [REPORT_CATEGORY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER,
    [NAME] NVARCHAR(50) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [WORKSPACE_TYPE_ID] INTEGER CONSTRAINT [DEF_TST_REPORT_CATEGORY_WORKSPACE_TYPE_ID] DEFAULT 1 NOT NULL,
    CONSTRAINT [PK_TST_REPORT_CATEGORY] PRIMARY KEY ([REPORT_CATEGORY_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_CATEGORY_1] ON [TST_REPORT_CATEGORY] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_REPORT_CATEGORY_2_FK] ON [TST_REPORT_CATEGORY] ([WORKSPACE_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_SECTION_ELEMENT"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_SECTION_ELEMENT] (
    [REPORT_SECTION_ID] INTEGER NOT NULL,
    [REPORT_ELEMENT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_REPORT_SECTION_ELEMENT] PRIMARY KEY ([REPORT_SECTION_ID], [REPORT_ELEMENT_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_SECTION_ELEMENT_1] ON [TST_REPORT_SECTION_ELEMENT] ([REPORT_SECTION_ID])
GO


CREATE  INDEX [AK_TST_REPORT_SECTION_ELEMENT_2] ON [TST_REPORT_SECTION_ELEMENT] ([REPORT_ELEMENT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_EVENT"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_EVENT] (
    [NOTIFICATION_EVENT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [IS_ARTIFACT_CREATION] BIT CONSTRAINT [DEF_TST_NOTIFICATION_EVENT_IS_ARTIFACT_CREATION] DEFAULT 0 NOT NULL,
    [IS_ACTIVE] BIT CONSTRAINT [DEF_TST_NOTIFICATION_EVENT_IS_ACTIVE] DEFAULT 1 NOT NULL,
    [NAME] VARCHAR(40) NOT NULL,
    [EMAIL_SUBJECT] VARCHAR(200) NOT NULL,
    CONSTRAINT [PK_TST_NOTIFICATION_EVENT] PRIMARY KEY ([NOTIFICATION_EVENT_ID])
)
GO


CREATE  INDEX [AK_TST_NOTIFICATION_EVENT_1] ON [TST_NOTIFICATION_EVENT] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_NOTIFICATION_EVENT_2_FK] ON [TST_NOTIFICATION_EVENT] ([PROJECT_TEMPLATE_ID])
GO


EXECUTE sp_addextendedproperty N'MS_Description', N'Each event created by the user has a list of fields, defined by the Project_ID and Artifact_ID', 'SCHEMA', N'dbo', 'TABLE', N'TST_NOTIFICATION_EVENT', NULL, NULL
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_EVENT_FIELD"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_EVENT_FIELD] (
    [NOTIFICATION_EVENT_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_NOTIFICATION_EVENT_FIELD] PRIMARY KEY ([NOTIFICATION_EVENT_ID], [ARTIFACT_FIELD_ID])
)
GO


CREATE  INDEX [AK_TST_NOTIFICATION_EVENT_FIELD_1] ON [TST_NOTIFICATION_EVENT_FIELD] ([NOTIFICATION_EVENT_ID])
GO


CREATE  INDEX [AK_TST_NOTIFICATION_EVENT_FIELD_2] ON [TST_NOTIFICATION_EVENT_FIELD] ([ARTIFACT_FIELD_ID])
GO


EXECUTE sp_addextendedproperty N'MS_Description', N'Entries of fields linked to an Event_ID', 'SCHEMA', N'dbo', 'TABLE', N'TST_NOTIFICATION_EVENT_FIELD', NULL, NULL
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_ARTIFACT_TEMPLATE"                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_ARTIFACT_TEMPLATE] (
    [NOTIFICATION_TEMPLATE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER,
    [TEMPLATE_TEXT] NVARCHAR(max) NOT NULL,
    CONSTRAINT [PK_TST_NOTIFICATION_ARTIFACT_TEMPLATE] PRIMARY KEY ([NOTIFICATION_TEMPLATE_ID])
)
GO


CREATE  INDEX [IDX_TST_NOTIFICATION_ARTIFACT_TEMPLATE_1_FK] ON [TST_NOTIFICATION_ARTIFACT_TEMPLATE] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_NOTIFICATION_ARTIFACT_TEMPLATE_2_FK] ON [TST_NOTIFICATION_ARTIFACT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_USER_SUBSCRIPTION"                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_USER_SUBSCRIPTION] (
    [USER_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_NOTIFICATION_USER_SUBSCRIPTION] PRIMARY KEY ([USER_ID], [ARTIFACT_TYPE_ID], [ARTIFACT_ID])
)
GO


CREATE  INDEX [AK_TST_NOTIFICATION_USER_SUBSCRIPTION_1] ON [TST_NOTIFICATION_USER_SUBSCRIPTION] ([USER_ID])
GO


CREATE  INDEX [AK_TST_NOTIFICATION_USER_SUBSCRIPTION_2] ON [TST_NOTIFICATION_USER_SUBSCRIPTION] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GRAPH"                                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GRAPH] (
    [GRAPH_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [GRAPH_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_GRAPH] PRIMARY KEY ([GRAPH_ID]),
    CONSTRAINT [TUC_TST_GRAPH_1] UNIQUE ([POSITION], [GRAPH_TYPE_ID])
)
GO


CREATE  INDEX [AK_TST_GRAPH_1] ON [TST_GRAPH] ([GRAPH_TYPE_ID])
GO


CREATE  INDEX [AK_TST_GRAPH_2] ON [TST_GRAPH] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_EVENT"                                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_EVENT] (
    [EVENT_ID] CHAR(32) NOT NULL,
    [EVENT_TYPE_ID] INTEGER NOT NULL,
    [EVENT_TIME_UTC] DATETIME NOT NULL,
    [EVENT_TIME] DATETIME NOT NULL,
    [EVENT_CATEGORY] NVARCHAR(256) NOT NULL,
    [EVENT_SEQUENCE] DECIMAL NOT NULL,
    [EVENT_OCCURRENCE] DECIMAL(19) NOT NULL,
    [EVENT_CODE] INTEGER NOT NULL,
    [EVENT_DETAIL_CODE] INTEGER NOT NULL,
    [MESSAGE] NVARCHAR(1024),
    [APPLICATION_PATH] NVARCHAR(256),
    [APPLICATION_VIRTUAL_PATH] NVARCHAR(256),
    [MACHINE_NAME] NVARCHAR(256) NOT NULL,
    [REQUEST_URL] NVARCHAR(1024),
    [EXCEPTION_TYPE] NVARCHAR(256),
    [DETAILS] NVARCHAR(max),
    CONSTRAINT [PK_TST_EVENT] PRIMARY KEY ([EVENT_ID])
)
GO


CREATE  INDEX [AK_TST_EVENT_1] ON [TST_EVENT] ([EVENT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_WORKFLOW"                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_WORKFLOW] (
    [REQUIREMENT_WORKFLOW_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_REQUIREMENT_WORKFLOW] PRIMARY KEY ([REQUIREMENT_WORKFLOW_ID])
)
GO


CREATE  INDEX [IDX_TST_REQUIREMENT_WORKFLOW_1_FK] ON [TST_REQUIREMENT_WORKFLOW] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_WORKFLOW_FIELD"                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_WORKFLOW_FIELD] (
    [REQUIREMENT_WORKFLOW_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [REQUIREMENT_STATUS_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_REQUIREMENT_WORKFLOW_FIELD] PRIMARY KEY ([REQUIREMENT_WORKFLOW_ID], [ARTIFACT_FIELD_ID], [WORKFLOW_FIELD_STATE_ID], [REQUIREMENT_STATUS_ID])
)
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_FIELD_1] ON [TST_REQUIREMENT_WORKFLOW_FIELD] ([REQUIREMENT_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_FIELD_2] ON [TST_REQUIREMENT_WORKFLOW_FIELD] ([ARTIFACT_FIELD_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_FIELD_3] ON [TST_REQUIREMENT_WORKFLOW_FIELD] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_FIELD_4] ON [TST_REQUIREMENT_WORKFLOW_FIELD] ([REQUIREMENT_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_WORKFLOW_TRANSITION"                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION] (
    [WORKFLOW_TRANSITION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [REQUIREMENT_WORKFLOW_ID] INTEGER,
    [INPUT_REQUIREMENT_STATUS_ID] INTEGER NOT NULL,
    [OUTPUT_REQUIREMENT_STATUS_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_EXECUTE_BY_CREATOR] BIT NOT NULL,
    [IS_EXECUTE_BY_OWNER] BIT NOT NULL,
    [IS_SIGNATURE_REQUIRED] BIT NOT NULL,
    [IS_BLANK_OWNER] BIT CONSTRAINT [DEF_TST_REQUIREMENT_WORKFLOW_TRANSITION_IS_BLANK_OWNER] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_CREATOR] BIT CONSTRAINT [DEF_TST_REQUIREMENT_WORKFLOW_TRANSITION_IS_NOTIFY_CREATOR] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_OWNER] BIT CONSTRAINT [DEF_TST_REQUIREMENT_WORKFLOW_TRANSITION_IS_NOTIFY_OWNER] DEFAULT 0 NOT NULL,
    [NOTIFY_SUBJECT] NVARCHAR(128),
    CONSTRAINT [PK_TST_REQUIREMENT_WORKFLOW_TRANSITION] PRIMARY KEY ([WORKFLOW_TRANSITION_ID])
)
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_TRANSITION_1] ON [TST_REQUIREMENT_WORKFLOW_TRANSITION] ([REQUIREMENT_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_TRANSITION_2] ON [TST_REQUIREMENT_WORKFLOW_TRANSITION] ([INPUT_REQUIREMENT_STATUS_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_TRANSITION_3] ON [TST_REQUIREMENT_WORKFLOW_TRANSITION] ([OUTPUT_REQUIREMENT_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE"                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] (
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ROLE_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] PRIMARY KEY ([WORKFLOW_TRANSITION_ID], [WORKFLOW_TRANSITION_ROLE_TYPE_ID], [PROJECT_ROLE_ID])
)
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE_1] ON [TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE_2] ON [TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] ([PROJECT_ROLE_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE_3] ON [TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_WORKFLOW"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_WORKFLOW] (
    [TASK_WORKFLOW_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_TASK_WORKFLOW] PRIMARY KEY ([TASK_WORKFLOW_ID])
)
GO


CREATE  INDEX [IDX_TST_TASK_WORKFLOW_1_FK] ON [TST_TASK_WORKFLOW] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_WORKFLOW_TRANSITION"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_WORKFLOW_TRANSITION] (
    [WORKFLOW_TRANSITION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TASK_WORKFLOW_ID] INTEGER NOT NULL,
    [INPUT_TASK_STATUS_ID] INTEGER NOT NULL,
    [OUTPUT_TASK_STATUS_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_EXECUTE_BY_CREATOR] BIT NOT NULL,
    [IS_EXECUTE_BY_OWNER] BIT NOT NULL,
    [IS_SIGNATURE_REQUIRED] BIT NOT NULL,
    [IS_BLANK_OWNER] BIT CONSTRAINT [DEF_TST_TASK_WORKFLOW_TRANSITION_IS_BLANK_OWNER] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_CREATOR] BIT CONSTRAINT [DEF_TST_TASK_WORKFLOW_TRANSITION_IS_NOTIFY_CREATOR] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_OWNER] BIT CONSTRAINT [DEF_TST_TASK_WORKFLOW_TRANSITION_IS_NOTIFY_OWNER] DEFAULT 0 NOT NULL,
    [NOTIFY_SUBJECT] NVARCHAR(128),
    CONSTRAINT [PK_TST_TASK_WORKFLOW_TRANSITION] PRIMARY KEY ([WORKFLOW_TRANSITION_ID])
)
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_TRANSITION_1] ON [TST_TASK_WORKFLOW_TRANSITION] ([TASK_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_TRANSITION_2] ON [TST_TASK_WORKFLOW_TRANSITION] ([INPUT_TASK_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_TRANSITION_3] ON [TST_TASK_WORKFLOW_TRANSITION] ([OUTPUT_TASK_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_WORKFLOW_TRANSITION_ROLE"                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_WORKFLOW_TRANSITION_ROLE] (
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ROLE_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TASK_WORKFLOW_TRANSITION_ROLE] PRIMARY KEY ([WORKFLOW_TRANSITION_ID], [WORKFLOW_TRANSITION_ROLE_TYPE_ID], [PROJECT_ROLE_ID])
)
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_TRANSITION_ROLE_1] ON [TST_TASK_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_TRANSITION_ROLE_2] ON [TST_TASK_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_TRANSITION_ROLE_3] ON [TST_TASK_WORKFLOW_TRANSITION_ROLE] ([PROJECT_ROLE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_WORKFLOW_FIELD"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_WORKFLOW_FIELD] (
    [TASK_WORKFLOW_ID] INTEGER NOT NULL,
    [TASK_STATUS_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TASK_WORKFLOW_FIELD] PRIMARY KEY ([TASK_WORKFLOW_ID], [TASK_STATUS_ID], [WORKFLOW_FIELD_STATE_ID], [ARTIFACT_FIELD_ID])
)
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_FIELD_1] ON [TST_TASK_WORKFLOW_FIELD] ([TASK_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_FIELD_2] ON [TST_TASK_WORKFLOW_FIELD] ([TASK_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_FIELD_3] ON [TST_TASK_WORKFLOW_FIELD] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_FIELD_4] ON [TST_TASK_WORKFLOW_FIELD] ([ARTIFACT_FIELD_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_MESSAGE"                                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_MESSAGE] (
    [MESSAGE_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [SENDER_USER_ID] INTEGER NOT NULL,
    [RECIPIENT_USER_ID] INTEGER NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [BODY] NVARCHAR(max) NOT NULL,
    [IS_READ] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    CONSTRAINT [PK_TST_MESSAGE] PRIMARY KEY ([MESSAGE_ID])
)
GO


CREATE  INDEX [AK_TST_MESSAGE_1] ON [TST_MESSAGE] ([SENDER_USER_ID])
GO


CREATE  INDEX [AK_TST_MESSAGE_2] ON [TST_MESSAGE] ([RECIPIENT_USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_CONTACT"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_CONTACT] (
    [CREATOR_USER_ID] INTEGER NOT NULL,
    [CONTACT_USER_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_USER_CONTACT] PRIMARY KEY ([CREATOR_USER_ID], [CONTACT_USER_ID])
)
GO


CREATE  INDEX [AK_TST_USER_CONTACT_1] ON [TST_USER_CONTACT] ([CONTACT_USER_ID])
GO


CREATE  INDEX [AK_TST_USER_CONTACT_2] ON [TST_USER_CONTACT] ([CREATOR_USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TVAULT_USER"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TVAULT_USER] (
    [USER_ID] INTEGER NOT NULL,
    [TARAVAULT_USERID] NVARCHAR(max) NOT NULL,
    [TARAVAULT_ISACTIVE] BIT CONSTRAINT [DEF_TST_TVAULT_USER_TARAVAULT_ISACTIVE] DEFAULT 1 NOT NULL,
    [TARAVAULT_PASSWORD] NVARCHAR(max) NOT NULL,
    [TARAVAULT_USERLOGIN] NVARCHAR(max) NOT NULL,
    CONSTRAINT [PK_TST_TVAULT_USER] PRIMARY KEY ([USER_ID])
)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_WORKFLOW"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_WORKFLOW] (
    [RELEASE_WORKFLOW_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_RELEASE_WORKFLOW] PRIMARY KEY ([RELEASE_WORKFLOW_ID])
)
GO


CREATE  INDEX [IDX_TST_RELEASE_WORKFLOW_1_FK] ON [TST_RELEASE_WORKFLOW] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_WORKFLOW_TRANSITION"                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_WORKFLOW_TRANSITION] (
    [WORKFLOW_TRANSITION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [RELEASE_WORKFLOW_ID] INTEGER NOT NULL,
    [INPUT_RELEASE_STATUS_ID] INTEGER NOT NULL,
    [OUTPUT_RELEASE_STATUS_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_EXECUTE_BY_CREATOR] BIT NOT NULL,
    [IS_EXECUTE_BY_OWNER] BIT NOT NULL,
    [IS_SIGNATURE_REQUIRED] BIT NOT NULL,
    [IS_BLANK_OWNER] BIT CONSTRAINT [DEF_TST_RELEASE_WORKFLOW_TRANSITION_IS_BLANK_OWNER] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_CREATOR] BIT CONSTRAINT [DEF_TST_RELEASE_WORKFLOW_TRANSITION_IS_NOTIFY_CREATOR] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_OWNER] BIT CONSTRAINT [DEF_TST_RELEASE_WORKFLOW_TRANSITION_IS_NOTIFY_OWNER] DEFAULT 0 NOT NULL,
    [NOTIFY_SUBJECT] NVARCHAR(128),
    CONSTRAINT [PK_TST_RELEASE_WORKFLOW_TRANSITION] PRIMARY KEY ([WORKFLOW_TRANSITION_ID])
)
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_TRANSITION_1] ON [TST_RELEASE_WORKFLOW_TRANSITION] ([RELEASE_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_TRANSITION_2] ON [TST_RELEASE_WORKFLOW_TRANSITION] ([INPUT_RELEASE_STATUS_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_TRANSITION_3] ON [TST_RELEASE_WORKFLOW_TRANSITION] ([OUTPUT_RELEASE_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_WORKFLOW_FIELD"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_WORKFLOW_FIELD] (
    [RELEASE_WORKFLOW_ID] INTEGER NOT NULL,
    [RELEASE_STATUS_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RELEASE_WORKFLOW_FIELD] PRIMARY KEY ([RELEASE_WORKFLOW_ID], [RELEASE_STATUS_ID], [ARTIFACT_FIELD_ID], [WORKFLOW_FIELD_STATE_ID])
)
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_FIELD_1] ON [TST_RELEASE_WORKFLOW_FIELD] ([RELEASE_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_FIELD_2] ON [TST_RELEASE_WORKFLOW_FIELD] ([RELEASE_STATUS_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_FIELD_3] ON [TST_RELEASE_WORKFLOW_FIELD] ([ARTIFACT_FIELD_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_FIELD_4] ON [TST_RELEASE_WORKFLOW_FIELD] ([WORKFLOW_FIELD_STATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_WORKFLOW_TRANSITION_ROLE"                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_WORKFLOW_TRANSITION_ROLE] (
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ROLE_TYPE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RELEASE_WORKFLOW_TRANSITION_ROLE] PRIMARY KEY ([WORKFLOW_TRANSITION_ID], [PROJECT_ROLE_ID], [WORKFLOW_TRANSITION_ROLE_TYPE_ID])
)
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_TRANSITION_ROLE_1] ON [TST_RELEASE_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_TRANSITION_ROLE_2] ON [TST_RELEASE_WORKFLOW_TRANSITION_ROLE] ([PROJECT_ROLE_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_TRANSITION_ROLE_3] ON [TST_RELEASE_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_WORKFLOW"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_WORKFLOW] (
    [TEST_CASE_WORKFLOW_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_TEST_CASE_WORKFLOW] PRIMARY KEY ([TEST_CASE_WORKFLOW_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_1_FK] ON [TST_TEST_CASE_WORKFLOW] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_WORKFLOW_TRANSITION"                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION] (
    [WORKFLOW_TRANSITION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TEST_CASE_WORKFLOW_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_EXECUTE_BY_CREATOR] BIT NOT NULL,
    [IS_EXECUTE_BY_OWNER] BIT NOT NULL,
    [IS_SIGNATURE_REQUIRED] BIT NOT NULL,
    [INPUT_TEST_CASE_STATUS_ID] INTEGER NOT NULL,
    [OUTPUT_TEST_CASE_STATUS_ID] INTEGER NOT NULL,
    [IS_BLANK_OWNER] BIT CONSTRAINT [DEF_TST_TEST_CASE_WORKFLOW_TRANSITION_IS_BLANK_OWNER] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_CREATOR] BIT CONSTRAINT [DEF_TST_TEST_CASE_WORKFLOW_TRANSITION_IS_NOTIFY_CREATOR] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_OWNER] BIT CONSTRAINT [DEF_TST_TEST_CASE_WORKFLOW_TRANSITION_IS_NOTIFY_OWNER] DEFAULT 0 NOT NULL,
    [NOTIFY_SUBJECT] NVARCHAR(128),
    CONSTRAINT [PK_TST_TEST_CASE_WORKFLOW_TRANSITION] PRIMARY KEY ([WORKFLOW_TRANSITION_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_TRANSITION_1_FK] ON [TST_TEST_CASE_WORKFLOW_TRANSITION] ([TEST_CASE_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_TRANSITION_2_FK] ON [TST_TEST_CASE_WORKFLOW_TRANSITION] ([INPUT_TEST_CASE_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_TRANSITION_3_FK] ON [TST_TEST_CASE_WORKFLOW_TRANSITION] ([OUTPUT_TEST_CASE_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_WORKFLOW_FIELD"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_WORKFLOW_FIELD] (
    [TEST_CASE_WORKFLOW_ID] INTEGER NOT NULL,
    [TEST_CASE_STATUS_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_CASE_WORKFLOW_FIELD] PRIMARY KEY ([TEST_CASE_WORKFLOW_ID], [TEST_CASE_STATUS_ID], [WORKFLOW_FIELD_STATE_ID], [ARTIFACT_FIELD_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_FIELD_1_FK] ON [TST_TEST_CASE_WORKFLOW_FIELD] ([TEST_CASE_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_FIELD_2_FK] ON [TST_TEST_CASE_WORKFLOW_FIELD] ([TEST_CASE_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_FIELD_3_FK] ON [TST_TEST_CASE_WORKFLOW_FIELD] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_FIELD_4_FK] ON [TST_TEST_CASE_WORKFLOW_FIELD] ([ARTIFACT_FIELD_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE"                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] (
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ROLE_TYPE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] PRIMARY KEY ([WORKFLOW_TRANSITION_ID], [PROJECT_ROLE_ID], [WORKFLOW_TRANSITION_ROLE_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE_1_FK] ON [TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE_2_FK] ON [TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] ([PROJECT_ROLE_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE_3_FK] ON [TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_TYPE"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_TYPE] (
    [TEST_CASE_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [TEST_CASE_WORKFLOW_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_EXPLORATORY] BIT NOT NULL,
    [IS_BDD] BIT NOT NULL,
    CONSTRAINT [PK_TST_TEST_CASE_TYPE] PRIMARY KEY ([TEST_CASE_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_TYPE_1_FK] ON [TST_TEST_CASE_TYPE] ([PROJECT_TEMPLATE_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_TYPE_2_FK] ON [TST_TEST_CASE_TYPE] ([TEST_CASE_WORKFLOW_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_MESSAGE_ARTIFACT"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_MESSAGE_ARTIFACT] (
    [MESSAGE_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [SENDER_USER_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [BODY] NVARCHAR(max) NOT NULL,
    [IS_READ] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    CONSTRAINT [PK_TST_MESSAGE_ARTIFACT] PRIMARY KEY ([MESSAGE_ID])
)
GO


CREATE  INDEX [IDX_TST_MESSAGE_ARTIFACT_1_FK] ON [TST_MESSAGE_ARTIFACT] ([SENDER_USER_ID])
GO


CREATE  INDEX [IDX_TST_MESSAGE_ARTIFACT_2_FK] ON [TST_MESSAGE_ARTIFACT] ([ARTIFACT_TYPE_ID])
GO


EXECUTE sp_addextendedproperty N'MS_Description', N'This is used to hold messages to artifacts, projects and project groups vs. individuals', 'SCHEMA', N'dbo', 'TABLE', N'TST_MESSAGE_ARTIFACT', NULL, NULL
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_TYPE_WORKFLOW"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_TYPE_WORKFLOW] (
    [RELEASE_TYPE_ID] INTEGER NOT NULL,
    [RELEASE_WORKFLOW_ID] INTEGER NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RELEASE_TYPE_WORKFLOW] PRIMARY KEY ([RELEASE_TYPE_ID], [RELEASE_WORKFLOW_ID])
)
GO


CREATE  INDEX [IDX_TST_RELEASE_TYPE_WORKFLOW_1_FK] ON [TST_RELEASE_TYPE_WORKFLOW] ([RELEASE_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_RELEASE_TYPE_WORKFLOW_2_FK] ON [TST_RELEASE_TYPE_WORKFLOW] ([RELEASE_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_RELEASE_TYPE_WORKFLOW_3_FK] ON [TST_RELEASE_TYPE_WORKFLOW] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DOCUMENT_WORKFLOW_TRANSITION"                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION] (
    [WORKFLOW_TRANSITION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [DOCUMENT_WORKFLOW_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_EXECUTE_BY_AUTHOR] BIT NOT NULL,
    [IS_EXECUTE_BY_EDITOR] BIT NOT NULL,
    [IS_SIGNATURE_REQUIRED] BIT NOT NULL,
    [INPUT_DOCUMENT_STATUS_ID] INTEGER NOT NULL,
    [OUTPUT_DOCUMENT_STATUS_ID] INTEGER NOT NULL,
    [IS_BLANK_OWNER] BIT CONSTRAINT [DEF_TST_DOCUMENT_WORKFLOW_TRANSITION_IS_BLANK_OWNER] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_CREATOR] BIT CONSTRAINT [DEF_TST_DOCUMENT_WORKFLOW_TRANSITION_IS_NOTIFY_CREATOR] DEFAULT 0 NOT NULL,
    [IS_NOTIFY_OWNER] BIT CONSTRAINT [DEF_TST_DOCUMENT_WORKFLOW_TRANSITION_IS_NOTIFY_OWNER] DEFAULT 0 NOT NULL,
    [NOTIFY_SUBJECT] NVARCHAR(128),
    PRIMARY KEY ([WORKFLOW_TRANSITION_ID])
)
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_TRANSITION_1_FK] ON [TST_DOCUMENT_WORKFLOW_TRANSITION] ([DOCUMENT_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_TRANSITION_2_FK] ON [TST_DOCUMENT_WORKFLOW_TRANSITION] ([INPUT_DOCUMENT_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_TRANSITION_3_FK] ON [TST_DOCUMENT_WORKFLOW_TRANSITION] ([OUTPUT_DOCUMENT_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE"                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] (
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ROLE_TYPE_ID] INTEGER NOT NULL,
    PRIMARY KEY ([WORKFLOW_TRANSITION_ID], [PROJECT_ROLE_ID], [WORKFLOW_TRANSITION_ROLE_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE_1_FK] ON [TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE_2_FK] ON [TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] ([PROJECT_ROLE_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE_3_FK] ON [TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DOCUMENT_WORKFLOW_FIELD"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DOCUMENT_WORKFLOW_FIELD] (
    [DOCUMENT_WORKFLOW_ID] INTEGER NOT NULL,
    [DOCUMENT_STATUS_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    PRIMARY KEY ([DOCUMENT_WORKFLOW_ID], [DOCUMENT_STATUS_ID], [WORKFLOW_FIELD_STATE_ID], [ARTIFACT_FIELD_ID])
)
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_FIELD_1_FK] ON [TST_DOCUMENT_WORKFLOW_FIELD] ([DOCUMENT_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_FIELD_2_FK] ON [TST_DOCUMENT_WORKFLOW_FIELD] ([DOCUMENT_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_FIELD_3_FK] ON [TST_DOCUMENT_WORKFLOW_FIELD] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_FIELD_4_FK] ON [TST_DOCUMENT_WORKFLOW_FIELD] ([ARTIFACT_FIELD_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_EVENT_WEBHOOK"                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_EVENT_WEBHOOK] (
    [NOTIFICATION_EVENT_WEBHOOK_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NOTIFICATION_EVENT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [URL] NVARCHAR(255) NOT NULL,
    [LOGIN] NVARCHAR(50),
    [PASSWORD] NVARCHAR(max),
    [OAUTH_ACCESS_TOKEN] NVARCHAR(255),
    [OAUTH_PROVIDER_ID] UNIQUEIDENTIFIER,
    [BODY] NVARCHAR(max),
    [METHOD] NVARCHAR(10),
    PRIMARY KEY ([NOTIFICATION_EVENT_WEBHOOK_ID])
)
GO


CREATE  INDEX [IDX_TST_NOTIFICATION_EVENT_WEBHOOK_1_FK] ON [TST_NOTIFICATION_EVENT_WEBHOOK] ([NOTIFICATION_EVENT_ID])
GO


CREATE  INDEX [IDX_TST_NOTIFICATION_EVENT_WEBHOOK_2_FK] ON [TST_NOTIFICATION_EVENT_WEBHOOK] ([OAUTH_PROVIDER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_TYPE"                                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_TYPE] (
    [RISK_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [RISK_WORKFLOW_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_TYPE] PRIMARY KEY ([RISK_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_TYPE_1_FK] ON [TST_RISK_TYPE] ([PROJECT_TEMPLATE_ID])
GO


CREATE  INDEX [IDX_TST_RISK_TYPE_2_FK] ON [TST_RISK_TYPE] ([RISK_WORKFLOW_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_WORKFLOW_FIELD"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_WORKFLOW_FIELD] (
    [RISK_WORKFLOW_ID] INTEGER NOT NULL,
    [RISK_STATUS_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_WORKFLOW_FIELD] PRIMARY KEY ([RISK_WORKFLOW_ID], [RISK_STATUS_ID], [ARTIFACT_FIELD_ID], [WORKFLOW_FIELD_STATE_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_FIELD_1_FK] ON [TST_RISK_WORKFLOW_FIELD] ([RISK_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_FIELD_2_FK] ON [TST_RISK_WORKFLOW_FIELD] ([RISK_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_FIELD_3_FK] ON [TST_RISK_WORKFLOW_FIELD] ([ARTIFACT_FIELD_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_FIELD_4_FK] ON [TST_RISK_WORKFLOW_FIELD] ([WORKFLOW_FIELD_STATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_GOAL"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_GOAL] (
    [GOAL_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [PROJECT_GROUP_ID] INTEGER NOT NULL,
    [POSITION] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GROUP_GOAL] PRIMARY KEY ([GOAL_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_GOAL_1_FK] ON [TST_PROJECT_GROUP_GOAL] ([PROJECT_GROUP_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_ROADMAP"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_ROADMAP] (
    [ROADMAP_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_GROUP_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GROUP_ROADMAP] PRIMARY KEY ([ROADMAP_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_ROADMAP_1_FK] ON [TST_PROJECT_GROUP_ROADMAP] ([PROJECT_GROUP_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_IDEA"                                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_IDEA] (
    [IDEA_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [IS_RICH_TEXT] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_USER_IDEA] PRIMARY KEY ([IDEA_ID])
)
GO


CREATE  INDEX [IDX_TST_USER_IDEA_1_FK] ON [TST_USER_IDEA] ([USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TIMECARD"                                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TIMECARD] (
    [TIMECARD_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [SUBMITTER_USER_ID] INTEGER NOT NULL,
    [APPROVER_USER_ID] INTEGER,
    [TIMECARD_STATUS_ID] INTEGER,
    [IS_DELETED] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [APPROVAL_DATE] DATETIME,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [COMMENTS] NVARCHAR(max),
    [APPROVER_COMMENTS] NVARCHAR(max),
    CONSTRAINT [PK_TST_TIMECARD] PRIMARY KEY ([TIMECARD_ID])
)
GO


CREATE  INDEX [IDX_TST_TIMECARD_1_FK] ON [TST_TIMECARD] ([SUBMITTER_USER_ID])
GO


CREATE  INDEX [IDX_TST_TIMECARD_2_FK] ON [TST_TIMECARD] ([TIMECARD_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_TIMECARD_3_FK] ON [TST_TIMECARD] ([APPROVER_USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_CUSTOM_PROPERTY"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_CUSTOM_PROPERTY] (
    [CUSTOM_PROPERTY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [CUSTOM_PROPERTY_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(100) NOT NULL,
    [PROPERTY_NUMBER] INTEGER NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [CUSTOM_PROPERTY_LIST_ID] INTEGER,
    [POSITION] INTEGER,
    CONSTRAINT [XPKTST_GLOBAL_CUSTOM_PROPERTY] PRIMARY KEY ([CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [AK_TST_GLOBAL_CUSTOM_PROPERTY_1] ON [TST_GLOBAL_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_TYPE_ID])
GO


CREATE  INDEX [AK_TST_GLOBAL_CUSTOM_PROPERTY_2] ON [TST_GLOBAL_CUSTOM_PROPERTY] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_GLOBAL_CUSTOM_PROPERTY_3] ON [TST_GLOBAL_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_LIST_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_CUSTOM_PROPERTY_VALUE"                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_CUSTOM_PROPERTY_VALUE] (
    [CUSTOM_PROPERTY_VALUE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [CUSTOM_PROPERTY_LIST_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    CONSTRAINT [XPKTST_GLOBAL_CUSTOM_PROPERTY_VALUE] PRIMARY KEY ([CUSTOM_PROPERTY_VALUE_ID])
)
GO


CREATE  INDEX [AK_TST_GLOBAL_CUSTOM_PROPERTY_VALUE_1] ON [TST_GLOBAL_CUSTOM_PROPERTY_VALUE] ([CUSTOM_PROPERTY_LIST_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE"                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE] (
    [CUSTOM_PROPERTY_OPTION_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE] PRIMARY KEY ([CUSTOM_PROPERTY_OPTION_ID], [CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [AK_TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE_1] ON [TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE] ([CUSTOM_PROPERTY_ID])
GO


CREATE  INDEX [AK_TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE_2] ON [TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE] ([CUSTOM_PROPERTY_OPTION_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_TEMPLATE_USER"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_TEMPLATE_USER] (
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_TEMPLATE_USER] PRIMARY KEY ([PROJECT_TEMPLATE_ID], [USER_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_TEMPLATE_USER_1_FK] ON [TST_PROJECT_TEMPLATE_USER] ([PROJECT_TEMPLATE_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_TEMPLATE_USER_2_FK] ON [TST_PROJECT_TEMPLATE_USER] ([USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_MESSAGE_TRACK"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_MESSAGE_TRACK] (
    [MESSAGE_ID] BIGINT NOT NULL,
    [RESOURCE_TRACK_ID] INTEGER NOT NULL,
    [SENDER_USER_ID] INTEGER NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [BODY] NVARCHAR(max) NOT NULL,
    [IS_READ] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    CONSTRAINT [PK_TST_MESSAGE_TRACK] PRIMARY KEY ([MESSAGE_ID])
)
GO


CREATE  INDEX [IDX_TST_MESSAGE_TRACK_1_FK] ON [TST_MESSAGE_TRACK] ([RESOURCE_TRACK_ID])
GO


CREATE  INDEX [IDX_TST_MESSAGE_TRACK_2_FK] ON [TST_MESSAGE_TRACK] ([SENDER_USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_PAGE_VIEWED"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_PAGE_VIEWED] (
    [USER_ID] INTEGER NOT NULL,
    [VIEWED_URL] NVARCHAR(255) NOT NULL,
    [VIEWED_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_USER_PAGE_VIEWED] PRIMARY KEY ([USER_ID], [VIEWED_URL])
)
GO


CREATE  INDEX [IDX_TST_USER_PAGE_VIEWED_1_FK] ON [TST_USER_PAGE_VIEWED] ([USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_MULTI_APPROVER"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_MULTI_APPROVER] (
    [MULTI_APPROVER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [MULTI_APPROVER_TYPE_ID] INTEGER NOT NULL,
    [IS_REQUIRED] BIT NOT NULL,
    CONSTRAINT [PK_TST_MULTI_APPROVER] PRIMARY KEY ([MULTI_APPROVER_ID])
)
GO


CREATE  INDEX [IDX_TST_MULTI_APPROVER_1_FK] ON [TST_MULTI_APPROVER] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_MULTI_APPROVER_2_FK] ON [TST_MULTI_APPROVER] ([MULTI_APPROVER_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_MULTI_APPROVER_EXECUTED"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_MULTI_APPROVER_EXECUTED] (
    [MULTI_APPROVER_EXECUTED_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [MULTI_APPROVER_ID] INTEGER,
    [USER_ID] INTEGER NOT NULL,
    [APPROVAL_DATE] DATETIME NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ID] INTEGER,
    [MEANING] NVARCHAR(max) NOT NULL,
    [SIGNATURE_HASH] NVARCHAR(max),
    [IS_COMPLETED] BIT NOT NULL,
    CONSTRAINT [PK_TST_MULTI_APPROVER_EXECUTED] PRIMARY KEY ([MULTI_APPROVER_EXECUTED_ID])
)
GO


CREATE  INDEX [IDX_TST_MULTI_APPROVER_EXECUTED_1_FK] ON [TST_MULTI_APPROVER_EXECUTED] ([MULTI_APPROVER_ID])
GO


CREATE  INDEX [IDX_TST_MULTI_APPROVER_EXECUTED_2_FK] ON [TST_MULTI_APPROVER_EXECUTED] ([USER_ID])
GO


CREATE  INDEX [IDX_TST_MULTI_APPROVER_EXECUTED_3_FK] ON [TST_MULTI_APPROVER_EXECUTED] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_SETTING_VALUE"                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_SETTING_VALUE] (
    [PROJECT_GROUP_SETTING_ID] INTEGER NOT NULL,
    [PROJECT_GROUP_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    PRIMARY KEY ([PROJECT_GROUP_SETTING_ID], [PROJECT_GROUP_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_SETTING_VALUE_1_FK] ON [TST_PROJECT_GROUP_SETTING_VALUE] ([PROJECT_GROUP_SETTING_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_SETTING_VALUE_2_FK] ON [TST_PROJECT_GROUP_SETTING_VALUE] ([PROJECT_GROUP_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_STANDARD_TEST_CASE"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_STANDARD_TEST_CASE] (
    [STANDARD_TEST_CASE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [STANDARD_TEST_CASE_SET_ID] INTEGER NOT NULL,
    [TEST_CASE_TYPE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPION] NVARCHAR(max),
    CONSTRAINT [PK_TST_STANDARD_TEST_CASE] PRIMARY KEY ([STANDARD_TEST_CASE_ID])
)
GO


CREATE  INDEX [IDX_TST_STANDARD_TEST_CASE_1_FK] ON [TST_STANDARD_TEST_CASE] ([STANDARD_TEST_CASE_SET_ID])
GO


CREATE  INDEX [IDX_TST_STANDARD_TEST_CASE_2_FK] ON [TST_STANDARD_TEST_CASE] ([TEST_CASE_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_HISTORY_CHANGESET"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_HISTORY_CHANGESET] (
    [CHANGESET_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [USER_ID] INTEGER CONSTRAINT [DEF_TST_GLOBAL_HISTORY_CHANGESET_USER_ID] DEFAULT 1 NOT NULL,
    [WORKSPACE_TYPE_ID] INTEGER CONSTRAINT [DEF_TST_GLOBAL_HISTORY_CHANGESET_WORKSPACE_TYPE_ID] DEFAULT 0 NOT NULL,
    [WORKSPACE_ID] INTEGER NOT NULL,
    [CHANGE_DATE] DATETIME NOT NULL,
    [CHANGETYPE_ID] INTEGER DEFAULT 1 NOT NULL,
    [ARTIFACT_DESC] NVARCHAR(255),
    CONSTRAINT [PK_TST_GLOBAL_HISTORY_CHANGESET] PRIMARY KEY ([CHANGESET_ID])
)
GO


CREATE  INDEX [AK_TST_GLOBAL_HISTORY_CHANGESET_1] ON [TST_GLOBAL_HISTORY_CHANGESET] ([CHANGESET_ID],[CHANGE_DATE])
GO


CREATE  INDEX [AK_TST_GLOBAL_HISTORY_CHANGESET_2] ON [TST_GLOBAL_HISTORY_CHANGESET] ([CHANGESET_ID],[CHANGETYPE_ID])
GO


CREATE NONCLUSTERED INDEX [AK_TST_GLOBAL_HISTORY_CHANGESET_3] ON [TST_GLOBAL_HISTORY_CHANGESET] ([WORKSPACE_TYPE_ID],[WORKSPACE_ID]) INCLUDE ( [CHANGESET_ID],
[USER_ID],
[CHANGETYPE_ID]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_GLOBAL_HISTORY_DETAILS"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_GLOBAL_HISTORY_DETAILS] (
    [ARTIFACT_HISTORY_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [CHANGESET_ID] BIGINT NOT NULL,
    [FIELD_NAME] NVARCHAR(50) NOT NULL,
    [FIELD_SUMMARY] NVARCHAR(255),
    [OLD_VALUE] NVARCHAR(max),
    [NEW_VALUE] NVARCHAR(max),
    [FIELD_TYPE_ID] INTEGER DEFAULT 1 NOT NULL,
    CONSTRAINT [PK_TST_GLOBAL_HISTORY_DETAILS] PRIMARY KEY ([ARTIFACT_HISTORY_ID])
)
GO


CREATE  INDEX [AK_TST_GLOBAL_HISTORY_DETAILS_1] ON [TST_GLOBAL_HISTORY_DETAILS] ([CHANGESET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW_TRANSITION"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW_TRANSITION] (
    [WORKFLOW_TRANSITION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [WORKFLOW_ID] INTEGER NOT NULL,
    [INPUT_INCIDENT_STATUS_ID] INTEGER NOT NULL,
    [OUTPUT_INCIDENT_STATUS_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [IS_EXECUTE_BY_DETECTOR] BIT NOT NULL,
    [IS_EXECUTE_BY_OWNER] BIT NOT NULL,
    [IS_NOTIFY_DETECTOR] BIT NOT NULL,
    [IS_NOTIFY_OWNER] BIT NOT NULL,
    [NOTIFY_SUBJECT] NVARCHAR(255),
    [IS_SIGNATURE_REQUIRED] BIT NOT NULL,
    [IS_BLANK_OWNER] BIT CONSTRAINT [DEF_TST_WORKFLOW_TRANSITION_IS_BLANK_OWNER] DEFAULT 0 NOT NULL,
    CONSTRAINT [XPKTST_WORKFLOW_TRANSITION] PRIMARY KEY ([WORKFLOW_TRANSITION_ID])
)
GO


CREATE  INDEX [AK_TST_WORKFLOW_TRANSITION_1] ON [TST_WORKFLOW_TRANSITION] ([INPUT_INCIDENT_STATUS_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_TRANSITION_2] ON [TST_WORKFLOW_TRANSITION] ([WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_TRANSITION_3] ON [TST_WORKFLOW_TRANSITION] ([OUTPUT_INCIDENT_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW_FIELD"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW_FIELD] (
    [WORKFLOW_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    [INCIDENT_STATUS_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_WORKFLOW_FIELD] PRIMARY KEY ([WORKFLOW_ID], [ARTIFACT_FIELD_ID], [INCIDENT_STATUS_ID], [WORKFLOW_FIELD_STATE_ID])
)
GO


CREATE  INDEX [AK_TST_WORKFLOW_FIELD_1] ON [TST_WORKFLOW_FIELD] ([INCIDENT_STATUS_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_FIELD_2] ON [TST_WORKFLOW_FIELD] ([WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_FIELD_3] ON [TST_WORKFLOW_FIELD] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_FIELD_4] ON [TST_WORKFLOW_FIELD] ([ARTIFACT_FIELD_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_INCIDENT_TYPE"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_INCIDENT_TYPE] (
    [INCIDENT_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [WORKFLOW_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_ISSUE] BIT NOT NULL,
    [IS_RISK] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    CONSTRAINT [XPKTST_INCIDENT_TYPE] PRIMARY KEY ([INCIDENT_TYPE_ID])
)
GO


CREATE  INDEX [AK_TST_INCIDENT_TYPE_1] ON [TST_INCIDENT_TYPE] ([WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_INCIDENT_TYPE_2_FK] ON [TST_INCIDENT_TYPE] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_CUSTOM_PROPERTY_VALUE"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_CUSTOM_PROPERTY_VALUE] (
    [CUSTOM_PROPERTY_VALUE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [CUSTOM_PROPERTY_LIST_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [DEPENDENT_CUSTOM_PROPERTY_LIST_ID] INTEGER,
    [PARENT_CUSTOM_PROPERTY_VALUE_ID] INTEGER,
    CONSTRAINT [XPKTST_CUSTOM_PROPERTY_VALUE] PRIMARY KEY ([CUSTOM_PROPERTY_VALUE_ID])
)
GO


CREATE  INDEX [AK_TST_CUSTOM_PROPERTY_VALUE_1] ON [TST_CUSTOM_PROPERTY_VALUE] ([CUSTOM_PROPERTY_LIST_ID])
GO


CREATE  INDEX [IDX_TST_CUSTOM_PROPERTY_VALUE_2_FK] ON [TST_CUSTOM_PROPERTY_VALUE] ([DEPENDENT_CUSTOM_PROPERTY_LIST_ID])
GO


CREATE  INDEX [IDX_TST_CUSTOM_PROPERTY_VALUE_3_FK] ON [TST_CUSTOM_PROPERTY_VALUE] ([PARENT_CUSTOM_PROPERTY_VALUE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ATTACHMENT"                                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ATTACHMENT] (
    [ATTACHMENT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ATTACHMENT_TYPE_ID] INTEGER NOT NULL,
    [AUTHOR_ID] INTEGER NOT NULL,
    [EDITOR_ID] INTEGER NOT NULL,
    [FILENAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [UPLOAD_DATE] DATETIME NOT NULL,
    [EDITED_DATE] DATETIME NOT NULL,
    [SIZE] INTEGER NOT NULL,
    [CURRENT_VERSION] NVARCHAR(5) NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [DOCUMENT_STATUS_ID] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_ATTACHMENT] PRIMARY KEY ([ATTACHMENT_ID])
)
GO


CREATE  INDEX [AK_TST_ATTACHMENT_1] ON [TST_ATTACHMENT] ([AUTHOR_ID])
GO


CREATE  INDEX [AK_TST_ATTACHMENT_2] ON [TST_ATTACHMENT] ([ATTACHMENT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_ATTACHMENT_3] ON [TST_ATTACHMENT] ([EDITOR_ID])
GO


CREATE  INDEX [IDX_TST_ATTACHMENT_4_FK] ON [TST_ATTACHMENT] ([DOCUMENT_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT"                                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT] (
    [PROJECT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_GROUP_ID] INTEGER NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [CREATION_DATE] DATETIME NOT NULL,
    [WEBSITE] NVARCHAR(255),
    [IS_ACTIVE] BIT NOT NULL,
    [WORKING_HOURS] INTEGER NOT NULL,
    [WORKING_DAYS] INTEGER NOT NULL,
    [NON_WORKING_HOURS] INTEGER NOT NULL,
    [IS_TIME_TRACK_INCIDENTS] BIT NOT NULL,
    [IS_TIME_TRACK_TASKS] BIT NOT NULL,
    [IS_EFFORT_INCIDENTS] BIT NOT NULL,
    [IS_EFFORT_TASKS] BIT NOT NULL,
    [IS_TASKS_AUTO_CREATE] BIT NOT NULL,
    [REQ_DEFAULT_ESTIMATE] DECIMAL(4,1),
    [REQ_POINT_EFFORT] INTEGER NOT NULL,
    [TASK_DEFAULT_EFFORT] INTEGER,
    [IS_REQ_STATUS_BY_TASKS] BIT NOT NULL,
    [IS_REQ_STATUS_BY_TEST_CASES] BIT NOT NULL,
    [IS_EFFORT_TEST_CASES] BIT NOT NULL,
    [IS_REQ_STATUS_AUTO_PLANNED] BIT NOT NULL,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [PERCENT_COMPLETE] INTEGER NOT NULL,
    [REQUIREMENT_COUNT] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_PROJECT] PRIMARY KEY ([PROJECT_ID])
)
GO


CREATE  INDEX [AK_TST_PROJECT_1] ON [TST_PROJECT] ([PROJECT_GROUP_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_2_FK] ON [TST_PROJECT] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_CUSTOM_PROPERTY"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_CUSTOM_PROPERTY] (
    [CUSTOM_PROPERTY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [CUSTOM_PROPERTY_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(100) NOT NULL,
    [PROPERTY_NUMBER] INTEGER NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [CUSTOM_PROPERTY_LIST_ID] INTEGER,
    [LEGACY_NAME] NVARCHAR(7),
    [DEPENDENT_CUSTOM_PROPERTY_ID] INTEGER,
    [POSITION] INTEGER,
    [DESCRIPTION] NVARCHAR(512),
    CONSTRAINT [XPKTST_CUSTOM_PROPERTY] PRIMARY KEY ([CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [AK_TST_CUSTOM_PROPERTY_1] ON [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_TYPE_ID])
GO


CREATE  INDEX [AK_TST_CUSTOM_PROPERTY_2] ON [TST_CUSTOM_PROPERTY] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_CUSTOM_PROPERTY_3] ON [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_LIST_ID])
GO


CREATE  INDEX [IDX_TST_CUSTOM_PROPERTY_4_FK] ON [TST_CUSTOM_PROPERTY] ([DEPENDENT_CUSTOM_PROPERTY_ID])
GO


CREATE  INDEX [IDX_TST_CUSTOM_PROPERTY_5_FK] ON [TST_CUSTOM_PROPERTY] ([PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_ARTIFACT_USER"                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_ARTIFACT_USER] (
    [NOTIFICATION_USER_TYPE_ID] INTEGER NOT NULL,
    [NOTIFICATION_EVENT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_NOTIFICATION_ARTIFACT_USER] PRIMARY KEY ([NOTIFICATION_USER_TYPE_ID], [NOTIFICATION_EVENT_ID])
)
GO


CREATE  INDEX [AK_TST_NOTIFICATION_ARTIFACT_USER_1] ON [TST_NOTIFICATION_ARTIFACT_USER] ([NOTIFICATION_USER_TYPE_ID])
GO


CREATE  INDEX [AK_TST_NOTIFICATION_ARTIFACT_USER_2] ON [TST_NOTIFICATION_ARTIFACT_USER] ([NOTIFICATION_EVENT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_PROJECT_ROLE"                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_PROJECT_ROLE] (
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    [NOTIFICATION_EVENT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_NOTIFICATION_PROJECT_ROLE] PRIMARY KEY ([PROJECT_ROLE_ID], [NOTIFICATION_EVENT_ID])
)
GO


CREATE  INDEX [AK_TST_NOTIFICATION_PROJECT_ROLE_1] ON [TST_NOTIFICATION_PROJECT_ROLE] ([PROJECT_ROLE_ID])
GO


CREATE  INDEX [AK_TST_NOTIFICATION_PROJECT_ROLE_2] ON [TST_NOTIFICATION_PROJECT_ROLE] ([NOTIFICATION_EVENT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_SAVED_FILTER"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_SAVED_FILTER] (
    [SAVED_FILTER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_SHARED] BIT NOT NULL,
    CONSTRAINT [PK_TST_SAVED_FILTER] PRIMARY KEY ([SAVED_FILTER_ID])
)
GO


CREATE  INDEX [AK_TST_SAVED_FILTER_1] ON [TST_SAVED_FILTER] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_SAVED_FILTER_2] ON [TST_SAVED_FILTER] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_SAVED_FILTER_3] ON [TST_SAVED_FILTER] ([USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_SAVED_FILTER_ENTRY"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_SAVED_FILTER_ENTRY] (
    [SAVED_FILTER_ID] INTEGER NOT NULL,
    [ENTRY_KEY] NVARCHAR(50) NOT NULL,
    [ENTRY_VALUE] NVARCHAR(255) NOT NULL,
    [ENTRY_TYPE_CODE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_SAVED_FILTER_ENTRY] PRIMARY KEY ([SAVED_FILTER_ID], [ENTRY_KEY])
)
GO


CREATE  INDEX [AK_TST_SAVED_FILTER_ENTRY_1] ON [TST_SAVED_FILTER_ENTRY] ([SAVED_FILTER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_ATTACHMENT_FOLDER"                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_ATTACHMENT_FOLDER] (
    [PROJECT_ATTACHMENT_FOLDER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [PARENT_PROJECT_ATTACHMENT_FOLDER_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_ATTACHMENT_FOLDER] PRIMARY KEY ([PROJECT_ATTACHMENT_FOLDER_ID])
)
GO


CREATE  INDEX [AK_TST_PROJECT_ATTACHMENT_FOLDER_1] ON [TST_PROJECT_ATTACHMENT_FOLDER] ([PARENT_PROJECT_ATTACHMENT_FOLDER_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_ATTACHMENT_FOLDER_2] ON [TST_PROJECT_ATTACHMENT_FOLDER] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DATA_SYNC_PROJECT"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DATA_SYNC_PROJECT] (
    [DATA_SYNC_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [EXTERNAL_KEY] NVARCHAR(255) NOT NULL,
    [ACTIVE_YN] CHAR(1) NOT NULL,
    CONSTRAINT [PK_TST_DATA_SYNC_PROJECT] PRIMARY KEY ([DATA_SYNC_SYSTEM_ID], [PROJECT_ID])
)
GO


CREATE  INDEX [AK_TST_DATA_SYNC_PROJECT_1] ON [TST_DATA_SYNC_PROJECT] ([DATA_SYNC_SYSTEM_ID])
GO


CREATE  INDEX [AK_TST_DATA_SYNC_PROJECT_2] ON [TST_DATA_SYNC_PROJECT] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING"                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING] (
    [DATA_SYNC_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    [ARTIFACT_FIELD_VALUE] INTEGER NOT NULL,
    [EXTERNAL_KEY] NVARCHAR(255) NOT NULL,
    [PRIMARY_YN] CHAR(1) NOT NULL,
    CONSTRAINT [PK_TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING] PRIMARY KEY ([DATA_SYNC_SYSTEM_ID], [PROJECT_ID], [ARTIFACT_FIELD_ID], [ARTIFACT_FIELD_VALUE])
)
GO


CREATE  INDEX [AK_TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING_1] ON [TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING] ([ARTIFACT_FIELD_ID])
GO


CREATE  INDEX [AK_TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING_2] ON [TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING] ([DATA_SYNC_SYSTEM_ID],[PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING"                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING] (
    [DATA_SYNC_SYSTEM_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [EXTERNAL_KEY] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING] PRIMARY KEY ([DATA_SYNC_SYSTEM_ID], [CUSTOM_PROPERTY_ID], [PROJECT_ID])
)
GO


CREATE  INDEX [AK_TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING_1] ON [TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING] ([DATA_SYNC_SYSTEM_ID],[PROJECT_ID])
GO


CREATE  INDEX [AK_TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING_2] ON [TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING] ([CUSTOM_PROPERTY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING"                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING] (
    [DATA_SYNC_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_VALUE_ID] INTEGER NOT NULL,
    [EXTERNAL_KEY] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING] PRIMARY KEY ([DATA_SYNC_SYSTEM_ID], [PROJECT_ID], [CUSTOM_PROPERTY_VALUE_ID])
)
GO


CREATE  INDEX [AK_TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING_1] ON [TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING] ([CUSTOM_PROPERTY_VALUE_ID])
GO


CREATE  INDEX [AK_TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING_2] ON [TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING] ([DATA_SYNC_SYSTEM_ID],[PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT"                                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT] (
    [REPORT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [REPORT_CATEGORY_ID] INTEGER NOT NULL,
    [TOKEN] NVARCHAR(40),
    [NAME] NVARCHAR(50) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [HEADER] NVARCHAR(max),
    [FOOTER] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_REPORT] PRIMARY KEY ([REPORT_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_1] ON [TST_REPORT] ([REPORT_CATEGORY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_AVAILABLE_FORMAT"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_AVAILABLE_FORMAT] (
    [REPORT_ID] INTEGER NOT NULL,
    [REPORT_FORMAT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_REPORT_AVAILABLE_FORMAT] PRIMARY KEY ([REPORT_ID], [REPORT_FORMAT_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_AVAILABLE_FORMAT_1] ON [TST_REPORT_AVAILABLE_FORMAT] ([REPORT_ID])
GO


CREATE  INDEX [AK_TST_REPORT_AVAILABLE_FORMAT_2] ON [TST_REPORT_AVAILABLE_FORMAT] ([REPORT_FORMAT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_AVAILABLE_SECTION"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_AVAILABLE_SECTION] (
    [REPORT_AVAILABLE_SECTION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [REPORT_ID] INTEGER NOT NULL,
    [REPORT_SECTION_ID] INTEGER NOT NULL,
    [HEADER] NVARCHAR(max),
    [FOOTER] NVARCHAR(max),
    [TEMPLATE] NVARCHAR(max),
    [POSITION] INTEGER,
    CONSTRAINT [PK_TST_REPORT_AVAILABLE_SECTION] PRIMARY KEY ([REPORT_AVAILABLE_SECTION_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_AVAILABLE_SECTION_1] ON [TST_REPORT_AVAILABLE_SECTION] ([REPORT_ID])
GO


CREATE  INDEX [AK_TST_REPORT_AVAILABLE_SECTION_2] ON [TST_REPORT_AVAILABLE_SECTION] ([REPORT_SECTION_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_SAVED"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_SAVED] (
    [REPORT_SAVED_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [REPORT_ID] INTEGER NOT NULL,
    [REPORT_FORMAT_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER,
    [NAME] NVARCHAR(50) NOT NULL,
    [PARAMETERS] NVARCHAR(max),
    [IS_SHARED] BIT NOT NULL,
    CONSTRAINT [PK_TST_REPORT_SAVED] PRIMARY KEY ([REPORT_SAVED_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_SAVED_1] ON [TST_REPORT_SAVED] ([REPORT_ID])
GO


CREATE  INDEX [AK_TST_REPORT_SAVED_2] ON [TST_REPORT_SAVED] ([USER_ID])
GO


CREATE  INDEX [AK_TST_REPORT_SAVED_3] ON [TST_REPORT_SAVED] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_REPORT_SAVED_4] ON [TST_REPORT_SAVED] ([REPORT_FORMAT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_VERSION_CONTROL_PROJECT"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_VERSION_CONTROL_PROJECT] (
    [VERSION_CONTROL_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [CONNECTION_STRING] NVARCHAR(255),
    [LOGIN] NVARCHAR(255),
    [PASSWORD] NVARCHAR(max),
    [DOMAIN] NVARCHAR(50),
    [CUSTOM_01] NVARCHAR(50),
    [CUSTOM_02] NVARCHAR(50),
    [CUSTOM_03] NVARCHAR(50),
    [CUSTOM_04] NVARCHAR(50),
    [CUSTOM_05] NVARCHAR(50),
    [IS_ENCRYPTED] BIT CONSTRAINT [DEF_TST_VERSION_CONTROL_PROJECT_IS_ENCRYPTED] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_TST_VERSION_CONTROL_PROJECT] PRIMARY KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID])
)
GO


CREATE  INDEX [AK_TST_VERSION_CONTROL_PROJECT_1] ON [TST_VERSION_CONTROL_PROJECT] ([VERSION_CONTROL_SYSTEM_ID])
GO


CREATE  INDEX [AK_TST_VERSION_CONTROL_PROJECT_2] ON [TST_VERSION_CONTROL_PROJECT] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_AUTOMATION_HOST"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_AUTOMATION_HOST] (
    [AUTOMATION_HOST_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [TOKEN] NVARCHAR(20) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_ATTACHMENTS] BIT NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_AUTOMATION_HOST_IS_DELETED] DEFAULT 0 NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [LAST_CONTACT_DATE] DATETIME,
    CONSTRAINT [PK_TST_AUTOMATION_HOST] PRIMARY KEY ([AUTOMATION_HOST_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TST_AUTOMATION_HOST_TOKEN] ON [TST_AUTOMATION_HOST] ([PROJECT_ID],[TOKEN])
GO


CREATE  INDEX [AK_TST_AUTOMATION_HOST_2] ON [TST_AUTOMATION_HOST] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_HISTORY_CHANGESET"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_HISTORY_CHANGESET] (
    [CHANGESET_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [USER_ID] INTEGER CONSTRAINT [DEF_TST_HISTORY_CHANGESET_USER_ID] DEFAULT 1 NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER CONSTRAINT [DEF_TST_HISTORY_CHANGESET_ARTIFACT_TYPE_ID] DEFAULT 0 NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [CHANGE_DATE] DATETIME NOT NULL,
    [CHANGETYPE_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER,
    [REVERT_ID] BIGINT,
    [ARTIFACT_DESC] NVARCHAR(255),
    [SIGNATURE_HASH] NVARCHAR(255),
    [MEANING] NVARCHAR(max),
    CONSTRAINT [PK_TST_HISTORY_CHANGESET] PRIMARY KEY ([CHANGESET_ID])
)
GO


CREATE  INDEX [AK_TST_HISTORY_CHANGESET_1] ON [TST_HISTORY_CHANGESET] ([CHANGESET_ID],[CHANGE_DATE],[PROJECT_ID])
GO


CREATE  INDEX [AK_TST_HISTORY_CHANGESET_2] ON [TST_HISTORY_CHANGESET] ([CHANGESET_ID],[CHANGETYPE_ID])
GO


CREATE NONCLUSTERED INDEX [AK_TST_HISTORY_CHANGESET_3] ON [TST_HISTORY_CHANGESET] ([ARTIFACT_TYPE_ID],[ARTIFACT_ID]) INCLUDE ( [CHANGESET_ID],
[USER_ID],
[CHANGETYPE_ID]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO


CREATE  INDEX [AK_TST_HISTORY_CHANGESET_4] ON [TST_HISTORY_CHANGESET] ([USER_ID])
GO


CREATE  INDEX [AK_TST_HISTORY_CHANGESET_5] ON [TST_HISTORY_CHANGESET] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_HISTORY_CHANGESET_6] ON [TST_HISTORY_CHANGESET] ([CHANGETYPE_ID])
GO


CREATE  INDEX [AK_TST_HISTORY_CHANGESET_7] ON [TST_HISTORY_CHANGESET] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_HISTORY_CHANGESET_8] ON [TST_HISTORY_CHANGESET] ([REVERT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_TAG_FREQUENCY"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_TAG_FREQUENCY] (
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [FREQUENCY] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_TAG_FREQUENCY] PRIMARY KEY ([PROJECT_ID], [NAME])
)
GO


CREATE  INDEX [AK_TST_PROJECT_TAG_FREQUENCY_1] ON [TST_PROJECT_TAG_FREQUENCY] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_PROFILE"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_PROFILE] (
    [USER_ID] INTEGER NOT NULL,
    [LAST_OPENED_PROJECT_ID] INTEGER,
    [FIRST_NAME] NVARCHAR(50) NOT NULL,
    [MIDDLE_INITIAL] NVARCHAR(1),
    [LAST_NAME] NVARCHAR(50) NOT NULL,
    [IS_ADMIN] BIT NOT NULL,
    [IS_EMAIL_ENABLED] BIT NOT NULL,
    [DEPARTMENT] NVARCHAR(50),
    [ORGANIZATION] NVARCHAR(100),
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [TIMEZONE] NVARCHAR(255),
    [AVATAR_IMAGE] NVARCHAR(max),
    [IS_BUSY] BIT NOT NULL,
    [IS_AWAY] BIT NOT NULL,
    [UNREAD_MESSAGES] INTEGER NOT NULL,
    [AVATAR_MIME_TYPE] NVARCHAR(20),
    [LAST_OPENED_PROJECT_GROUP_ID] INTEGER,
    [LAST_OPENED_PROJECT_TEMPLATE_ID] INTEGER,
    [IS_RESOURCE_ADMIN] BIT NOT NULL,
    [IS_PORTFOLIO_ADMIN] BIT NOT NULL,
    [IS_RESTRICTED] BIT NOT NULL,
    [IS_REPORT_ADMIN] BIT CONSTRAINT [DEF_TST_USER_PROFILE_IS_REPORT_ADMIN] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_TST_USER_PROFILE] PRIMARY KEY ([USER_ID])
)
GO


CREATE  INDEX [AK_TST_USER_PROFILE_1] ON [TST_USER_PROFILE] ([LAST_OPENED_PROJECT_ID])
GO


CREATE  INDEX [IDX_TST_USER_PROFILE_2_FK] ON [TST_USER_PROFILE] ([LAST_OPENED_PROJECT_GROUP_ID])
GO


CREATE  INDEX [IDX_TST_USER_PROFILE_3_FK] ON [TST_USER_PROFILE] ([LAST_OPENED_PROJECT_TEMPLATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_CUSTOM_PROPERTY_OPTION_VALUE"                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_CUSTOM_PROPERTY_OPTION_VALUE] (
    [CUSTOM_PROPERTY_OPTION_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_CUSTOM_PROPERTY_OPTION_VALUE] PRIMARY KEY ([CUSTOM_PROPERTY_OPTION_ID], [CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [AK_TST_CUSTOM_PROPERTY_OPTION_VALUE_1] ON [TST_CUSTOM_PROPERTY_OPTION_VALUE] ([CUSTOM_PROPERTY_OPTION_ID])
GO


CREATE  INDEX [AK_TST_CUSTOM_PROPERTY_OPTION_VALUE_2] ON [TST_CUSTOM_PROPERTY_OPTION_VALUE] ([CUSTOM_PROPERTY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PLACEHOLDER"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PLACEHOLDER] (
    [PLACEHOLDER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_PLACEHOLDER] PRIMARY KEY ([PLACEHOLDER_ID])
)
GO


CREATE  INDEX [AK_TST_PLACEHOLDER_1] ON [TST_PLACEHOLDER] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_CUSTOM_SECTION"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_CUSTOM_SECTION] (
    [REPORT_CUSTOM_SECTION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [REPORT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(100) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [TEMPLATE] NVARCHAR(max) NOT NULL,
    [QUERY] NVARCHAR(max) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [HEADER] NVARCHAR(max),
    [FOOTER] NVARCHAR(max),
    [POSITION] INTEGER,
    CONSTRAINT [PK_TST_REPORT_CUSTOM_SECTION] PRIMARY KEY ([REPORT_CUSTOM_SECTION_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_CUSTOM_SECTION_1] ON [TST_REPORT_CUSTOM_SECTION] ([REPORT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REPORT_GENERATED"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REPORT_GENERATED] (
    [REPORT_GENERATED_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [REPORT_ID] INTEGER NOT NULL,
    [REPORT_FORMAT_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_REPORT_GENERATED] PRIMARY KEY ([REPORT_GENERATED_ID])
)
GO


CREATE  INDEX [AK_TST_REPORT_GENERATED_1] ON [TST_REPORT_GENERATED] ([REPORT_ID])
GO


CREATE  INDEX [AK_TST_REPORT_GENERATED_2] ON [TST_REPORT_GENERATED] ([REPORT_FORMAT_ID])
GO


CREATE  INDEX [AK_TST_REPORT_GENERATED_3] ON [TST_REPORT_GENERATED] ([USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_TYPE"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_TYPE] (
    [REQUIREMENT_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [REQUIREMENT_WORKFLOW_ID] INTEGER,
    [PROJECT_TEMPLATE_ID] INTEGER,
    [NAME] NVARCHAR(50) NOT NULL,
    [ICON] NVARCHAR(40) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_STEPS] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_KEY_TYPE] BIT NOT NULL,
    CONSTRAINT [PK_TST_REQUIREMENT_TYPE] PRIMARY KEY ([REQUIREMENT_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_REQUIREMENT_TYPE_1_FK] ON [TST_REQUIREMENT_TYPE] ([PROJECT_TEMPLATE_ID])
GO


CREATE  INDEX [IDX_TST_REQUIREMENT_TYPE_2_FK] ON [TST_REQUIREMENT_TYPE] ([REQUIREMENT_WORKFLOW_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_FOLDER"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_FOLDER] (
    [TASK_FOLDER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [PARENT_TASK_FOLDER_ID] INTEGER,
    [NAME] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_TST_TASK_FOLDER] PRIMARY KEY ([TASK_FOLDER_ID])
)
GO


CREATE  INDEX [AK_TST_TASK_FOLDER_1] ON [TST_TASK_FOLDER] ([PARENT_TASK_FOLDER_ID])
GO


CREATE  INDEX [AK_TST_TASK_FOLDER_2] ON [TST_TASK_FOLDER] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_COMPONENT"                                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_COMPONENT] (
    [COMPONENT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    CONSTRAINT [PK_TST_COMPONENT] PRIMARY KEY ([COMPONENT_ID])
)
GO


CREATE  INDEX [AK_TST_COMPONENT_1] ON [TST_COMPONENT] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY"                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] (
    [REQUIREMENT_WORKFLOW_ID] INTEGER NOT NULL,
    [REQUIREMENT_STATUS_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] PRIMARY KEY ([REQUIREMENT_WORKFLOW_ID], [REQUIREMENT_STATUS_ID], [WORKFLOW_FIELD_STATE_ID], [CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY_1] ON [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] ([REQUIREMENT_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY_2] ON [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] ([REQUIREMENT_STATUS_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY_3] ON [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY_4] ON [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_TYPE"                                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_TYPE] (
    [TASK_TYPE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_TEMPLATE_ID] INTEGER NOT NULL,
    [TASK_WORKFLOW_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DEFAULT] BIT NOT NULL,
    [IS_CODE_REVIEW] BIT NOT NULL,
    [IS_PULL_REQUEST] BIT NOT NULL,
    CONSTRAINT [PK_TST_TASK_TYPE] PRIMARY KEY ([TASK_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_TASK_TYPE_1_FK] ON [TST_TASK_TYPE] ([PROJECT_TEMPLATE_ID])
GO


CREATE  INDEX [IDX_TST_TASK_TYPE_2_FK] ON [TST_TASK_TYPE] ([TASK_WORKFLOW_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_WORKFLOW_CUSTOM_PROPERTY"                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] (
    [TASK_WORKFLOW_ID] INTEGER NOT NULL,
    [TASK_STATUS_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TASK_WORKFLOW_CUSTOM_PROPERTY] PRIMARY KEY ([TASK_WORKFLOW_ID], [TASK_STATUS_ID], [WORKFLOW_FIELD_STATE_ID], [CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_CUSTOM_PROPERTY_1] ON [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] ([TASK_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_CUSTOM_PROPERTY_2] ON [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] ([TASK_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_CUSTOM_PROPERTY_3] ON [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [AK_TST_TASK_WORKFLOW_CUSTOM_PROPERTY_4] ON [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TVAULT_PROJECT"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TVAULT_PROJECT] (
    [PROJECT_ID] INTEGER NOT NULL,
    [TVAULT_TYPE_ID] INTEGER NOT NULL,
    [TARAVAULT_ID] BIGINT NOT NULL,
    [TARAVAULT_NAME] NVARCHAR(40) NOT NULL,
    CONSTRAINT [PK_TST_TVAULT_PROJECT] PRIMARY KEY ([PROJECT_ID])
)
GO


CREATE  INDEX [AK_TST_TVAULT_PROJECT_1] ON [TST_TVAULT_PROJECT] ([TVAULT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TVAULT_PROJECT_USER"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TVAULT_PROJECT_USER] (
    [USER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TVAULT_PROJECT_USER] PRIMARY KEY ([USER_ID], [PROJECT_ID])
)
GO


CREATE  INDEX [AK_TST_TVAULT_PROJECT_USER_1] ON [TST_TVAULT_PROJECT_USER] ([USER_ID])
GO


CREATE  INDEX [AK_TST_TVAULT_PROJECT_USER_2] ON [TST_TVAULT_PROJECT_USER] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY"                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] (
    [RELEASE_WORKFLOW_ID] INTEGER NOT NULL,
    [RELEASE_STATUS_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] PRIMARY KEY ([RELEASE_WORKFLOW_ID], [RELEASE_STATUS_ID], [CUSTOM_PROPERTY_ID], [WORKFLOW_FIELD_STATE_ID])
)
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY_1] ON [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] ([RELEASE_WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY_2] ON [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] ([RELEASE_STATUS_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY_3] ON [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY_4] ON [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] ([WORKFLOW_FIELD_STATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY"                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] (
    [TEST_CASE_WORKFLOW_ID] INTEGER NOT NULL,
    [TEST_CASE_STATUS_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] PRIMARY KEY ([TEST_CASE_WORKFLOW_ID], [TEST_CASE_STATUS_ID], [WORKFLOW_FIELD_STATE_ID], [CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY_1_FK] ON [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] ([TEST_CASE_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY_2_FK] ON [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] ([TEST_CASE_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY_3_FK] ON [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY_4_FK] ON [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_FOLDER"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_FOLDER] (
    [TEST_CASE_FOLDER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PARENT_TEST_CASE_FOLDER_ID] INTEGER,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [EXECUTION_DATE] DATETIME,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [ESTIMATED_DURATION] INTEGER,
    [ACTUAL_DURATION] INTEGER,
    [COUNT_PASSED] INTEGER NOT NULL,
    [COUNT_FAILED] INTEGER NOT NULL,
    [COUNT_BLOCKED] INTEGER NOT NULL,
    [COUNT_CAUTION] INTEGER NOT NULL,
    [COUNT_NOT_RUN] INTEGER NOT NULL,
    [COUNT_NOT_APPLICABLE] INTEGER NOT NULL,
    PRIMARY KEY ([TEST_CASE_FOLDER_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_FOLDER_1_FK] ON [TST_TEST_CASE_FOLDER] ([PARENT_TEST_CASE_FOLDER_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_FOLDER_2_FK] ON [TST_TEST_CASE_FOLDER] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_SET_FOLDER"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_SET_FOLDER] (
    [TEST_SET_FOLDER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [PARENT_TEST_SET_FOLDER_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [EXECUTION_DATE] DATETIME,
    [ESTIMATED_DURATION] INTEGER,
    [ACTUAL_DURATION] INTEGER,
    [COUNT_PASSED] INTEGER NOT NULL,
    [COUNT_FAILED] INTEGER NOT NULL,
    [COUNT_CAUTION] INTEGER NOT NULL,
    [COUNT_BLOCKED] INTEGER NOT NULL,
    [COUNT_NOT_RUN] INTEGER NOT NULL,
    [COUNT_NOT_APPLICABLE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_SET_FOLDER] PRIMARY KEY ([TEST_SET_FOLDER_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_SET_FOLDER_1_FK] ON [TST_TEST_SET_FOLDER] ([PROJECT_ID])
GO


CREATE  INDEX [IDX_TST_TEST_SET_FOLDER_2_FK] ON [TST_TEST_SET_FOLDER] ([PARENT_TEST_SET_FOLDER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_ARTIFACT_SHARING"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_ARTIFACT_SHARING] (
    [SOURCE_PROJECT_ID] INTEGER NOT NULL,
    [DEST_PROJECT_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_ARTIFACT_SHARING] PRIMARY KEY ([SOURCE_PROJECT_ID], [DEST_PROJECT_ID], [ARTIFACT_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_ARTIFACT_SHARING_1_FK] ON [TST_PROJECT_ARTIFACT_SHARING] ([SOURCE_PROJECT_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_ARTIFACT_SHARING_2_FK] ON [TST_PROJECT_ARTIFACT_SHARING] ([DEST_PROJECT_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_ARTIFACT_SHARING_3_FK] ON [TST_PROJECT_ARTIFACT_SHARING] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_SETTING_VALUE"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_SETTING_VALUE] (
    [PROJECT_SETTING_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_SETTING_VALUE] PRIMARY KEY ([PROJECT_SETTING_ID], [PROJECT_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_SETTING_VALUE_1_FK] ON [TST_PROJECT_SETTING_VALUE] ([PROJECT_SETTING_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_SETTING_VALUE_2_FK] ON [TST_PROJECT_SETTING_VALUE] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_CUSTOM_PROPERTY_DEPENDENCY"                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_CUSTOM_PROPERTY_DEPENDENCY] (
    [SOURCE_CUSTOM_PROPERTY_VALUE_ID] INTEGER NOT NULL,
    [DEST_CUSTOM_PROPERTY_VALUE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_CUSTOM_PROPERTY_DEPENDENCY] PRIMARY KEY ([SOURCE_CUSTOM_PROPERTY_VALUE_ID], [DEST_CUSTOM_PROPERTY_VALUE_ID])
)
GO


CREATE  INDEX [IDX_TST_CUSTOM_PROPERTY_DEPENDENCY_1_FK] ON [TST_CUSTOM_PROPERTY_DEPENDENCY] ([SOURCE_CUSTOM_PROPERTY_VALUE_ID])
GO


CREATE  INDEX [IDX_TST_CUSTOM_PROPERTY_DEPENDENCY_2_FK] ON [TST_CUSTOM_PROPERTY_DEPENDENCY] ([DEST_CUSTOM_PROPERTY_VALUE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CONFIGURATION_SET"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CONFIGURATION_SET] (
    [TEST_CONFIGURATION_SET_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATED_DATE] DATETIME NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_TEST_CONFIGURATION_SET] PRIMARY KEY ([TEST_CONFIGURATION_SET_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CONFIGURATION_SET_1_FK] ON [TST_TEST_CONFIGURATION_SET] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DOCUMENT_DISCUSSION"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DOCUMENT_DISCUSSION] (
    [DISCUSSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [TEXT] NVARCHAR(max) NOT NULL,
    [CREATON_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [IS_PERMANENT] BIT NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    PRIMARY KEY ([DISCUSSION_ID])
)
GO


CREATE  INDEX [IDX_TST_DOCUMENT_DISCUSSION_1_FK] ON [TST_DOCUMENT_DISCUSSION] ([CREATOR_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_DISCUSSION_2_FK] ON [TST_DOCUMENT_DISCUSSION] ([ARTIFACT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD"                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] (
    [DOCUMENT_WORKFLOW_ID] INTEGER NOT NULL,
    [DOCUMENT_STATUS_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] PRIMARY KEY ([DOCUMENT_WORKFLOW_ID], [DOCUMENT_STATUS_ID], [WORKFLOW_FIELD_STATE_ID], [CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD_1_FK] ON [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] ([DOCUMENT_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD_2_FK] ON [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] ([DOCUMENT_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD_3_FK] ON [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] ([WORKFLOW_FIELD_STATE_ID])
GO


CREATE  INDEX [IDX_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD_4_FK] ON [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] ([CUSTOM_PROPERTY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY"                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY] (
    [NOTIFICATION_EVENT_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY] PRIMARY KEY ([NOTIFICATION_EVENT_ID], [CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [IDX_TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY_1_FK] ON [TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY] ([NOTIFICATION_EVENT_ID])
GO


CREATE  INDEX [IDX_TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY_2_FK] ON [TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_WORKFLOW_CUSTOM_PROPERTY"                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] (
    [RISK_WORKFLOW_ID] INTEGER NOT NULL,
    [RISK_STATUS_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_WORKFLOW_CUSTOM_PROPERTY] PRIMARY KEY ([RISK_WORKFLOW_ID], [RISK_STATUS_ID], [CUSTOM_PROPERTY_ID], [WORKFLOW_FIELD_STATE_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_CUSTOM_PROPERTY_1_FK] ON [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] ([RISK_WORKFLOW_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_CUSTOM_PROPERTY_2_FK] ON [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] ([RISK_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_CUSTOM_PROPERTY_3_FK] ON [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


CREATE  INDEX [IDX_TST_RISK_WORKFLOW_CUSTOM_PROPERTY_4_FK] ON [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] ([WORKFLOW_FIELD_STATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_DOCUMENT"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_DOCUMENT] (
    [PROJECT_GROUP_ID] INTEGER NOT NULL,
    [ATTACHMENT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GROUP_DOCUMENT] PRIMARY KEY ([PROJECT_GROUP_ID], [ATTACHMENT_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_DOCUMENT_1_FK] ON [TST_PROJECT_GROUP_DOCUMENT] ([PROJECT_GROUP_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_DOCUMENT_2_FK] ON [TST_PROJECT_GROUP_DOCUMENT] ([ATTACHMENT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GOAL"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GOAL] (
    [GOAL_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [POSITION] IMAGE NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GOAL] PRIMARY KEY ([GOAL_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_GOAL_1_FK] ON [TST_PROJECT_GOAL] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_MILESTONE"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_MILESTONE] (
    [MILESTONE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ROADMAP_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [PERCENT_COMPLETE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GROUP_MILESTONE] PRIMARY KEY ([MILESTONE_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_MILESTONE_1_FK] ON [TST_PROJECT_GROUP_MILESTONE] ([ROADMAP_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_GROUP_THEME"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_GROUP_THEME] (
    [THEME_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ROADMAP_ID] INTEGER NOT NULL,
    [MILESTONE_ID] INTEGER,
    [THEME_STATUS_ID] INTEGER NOT NULL,
    [THEME_PRIORITY_ID] INTEGER,
    [GOAL_ID] INTEGER,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_DELETED] BIT NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_GROUP_THEME] PRIMARY KEY ([THEME_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_THEME_1_FK] ON [TST_PROJECT_GROUP_THEME] ([ROADMAP_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_THEME_2_FK] ON [TST_PROJECT_GROUP_THEME] ([MILESTONE_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_THEME_3_FK] ON [TST_PROJECT_GROUP_THEME] ([GOAL_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_THEME_4_FK] ON [TST_PROJECT_GROUP_THEME] ([THEME_PRIORITY_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_GROUP_THEME_5_FK] ON [TST_PROJECT_GROUP_THEME] ([THEME_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TIMECARD_ENTRY"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TIMECARD_ENTRY] (
    [TIMECARD_ENTRY_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TIMECARD_ID] INTEGER,
    [PROJECT_GROUP_ID] INTEGER,
    [PROJECT_ID] INTEGER,
    [ARTIFACT_TYPE_ID] INTEGER,
    [USER_ID] INTEGER NOT NULL,
    [TIMECARD_ENTRY_TYPE_ID] INTEGER,
    [RESOURCE_CATEGORY_ID] INTEGER,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [EFFORT] INTEGER NOT NULL,
    [EFFORT_DATE] DATETIME NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    CONSTRAINT [PK_TST_TIMECARD_ENTRY] PRIMARY KEY ([TIMECARD_ENTRY_ID])
)
GO


CREATE  INDEX [IDX_TST_TIMECARD_ENTRY_1_FK] ON [TST_TIMECARD_ENTRY] ([TIMECARD_ID])
GO


CREATE  INDEX [IDX_TST_TIMECARD_ENTRY_2_FK] ON [TST_TIMECARD_ENTRY] ([PROJECT_ID])
GO


CREATE  INDEX [IDX_TST_TIMECARD_ENTRY_3_FK] ON [TST_TIMECARD_ENTRY] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_TIMECARD_ENTRY_4_FK] ON [TST_TIMECARD_ENTRY] ([USER_ID])
GO


CREATE  INDEX [IDX_TST_TIMECARD_ENTRY_5_FK] ON [TST_TIMECARD_ENTRY] ([RESOURCE_CATEGORY_ID])
GO


CREATE  INDEX [IDX_TST_TIMECARD_ENTRY_6_FK] ON [TST_TIMECARD_ENTRY] ([TIMECARD_ENTRY_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_TIMECARD_ENTRY_7_FK] ON [TST_TIMECARD_ENTRY] ([PROJECT_GROUP_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_HISTORY_ASSOCIATION"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_HISTORY_ASSOCIATION] (
    [ASSOCIATION_HISTORY_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [CHANGESET_ID] BIGINT NOT NULL,
    [SOURCE_ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [SOURCE_ARTIFACT_ID] INTEGER NOT NULL,
    [DEST_ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [DEST_ARTIFACT_ID] INTEGER NOT NULL,
    [ARTIFACT_LINK_ID] INTEGER,
    [OLD_COMMENT] NVARCHAR(max),
    [OLD_ARTIFACT_LINK_TYPE_ID] INTEGER,
    [NEW_COMMENT] NVARCHAR(max),
    [NEW_ARTIFACT_LINK_TYPE_ID] INTEGER,
    CONSTRAINT [PK_TST_HISTORY_ASSOCIATION] PRIMARY KEY ([ASSOCIATION_HISTORY_ID])
)
GO


CREATE  INDEX [IDX_TST_HISTORY_ASSOCIATION_1_FK] ON [TST_HISTORY_ASSOCIATION] ([CHANGESET_ID])
GO


CREATE  INDEX [IDX_TST_HISTORY_ASSOCIATION_2_FK] ON [TST_HISTORY_ASSOCIATION] ([SOURCE_ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_HISTORY_ASSOCIATION_3_FK] ON [TST_HISTORY_ASSOCIATION] ([DEST_ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_HISTORY_ASSOCIATION_4_FK] ON [TST_HISTORY_ASSOCIATION] ([ARTIFACT_LINK_ID])
GO


CREATE  INDEX [IDX_TST_HISTORY_ASSOCIATION_5_FK] ON [TST_HISTORY_ASSOCIATION] ([OLD_ARTIFACT_LINK_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_HISTORY_ASSOCIATION_6_FK] ON [TST_HISTORY_ASSOCIATION] ([NEW_ARTIFACT_LINK_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_VERSION_CONTROL_BRANCH"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_VERSION_CONTROL_BRANCH] (
    [BRANCH_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [VERSION_CONTROL_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [PATH] NVARCHAR(255) NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [IS_HEAD] BIT NOT NULL,
    CONSTRAINT [PK_TST_VERSION_CONTROL_BRANCH] PRIMARY KEY ([BRANCH_ID])
)
GO


CREATE  INDEX [IDX_TST_VERSION_CONTROL_BRANCH_1_FK] ON [TST_VERSION_CONTROL_BRANCH] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_HISTORY_POSITION"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_HISTORY_POSITION] (
    [HISTORY_POSITION_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [CHANGESET_ID] BIGINT NOT NULL,
    [CHILD_ARTIFACT_ID] INTEGER NOT NULL,
    [CHILD_ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [OLD_POSITION] INTEGER NOT NULL,
    [NEW_POSITION] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_HISTORY_POSITION] PRIMARY KEY ([HISTORY_POSITION_ID])
)
GO


CREATE  INDEX [IDX_TST_HISTORY_POSITION_1_FK] ON [TST_HISTORY_POSITION] ([CHANGESET_ID])
GO


CREATE  INDEX [IDX_TST_HISTORY_POSITION_2_FK] ON [TST_HISTORY_POSITION] ([CHILD_ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_TAGS"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_TAGS] (
    [ARTIFACT_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [TAGS] NVARCHAR(max),
    [PROJECT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_ARTIFACT_TAGS] PRIMARY KEY ([ARTIFACT_ID], [ARTIFACT_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_ARTIFACT_TAGS_1_FK] ON [TST_ARTIFACT_TAGS] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_ARTIFACT_TAGS_2_FK] ON [TST_ARTIFACT_TAGS] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_HISTORY_DISCUSSION"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_HISTORY_DISCUSSION] (
    [HISTORY_DISCUSSION_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [CHANGESET_ID] BIGINT NOT NULL,
    [DISCUSSION_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_HISTORY_DISCUSSION] PRIMARY KEY ([HISTORY_DISCUSSION_ID])
)
GO


CREATE  INDEX [IDX_TST_HISTORY_DISCUSSION_1_FK] ON [TST_HISTORY_DISCUSSION] ([CHANGESET_ID])
GO


CREATE  INDEX [IDX_TST_HISTORY_DISCUSSION_2_FK] ON [TST_HISTORY_DISCUSSION] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_HISTORY_TAGS"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_HISTORY_TAGS] (
    [HISTORY_TAGS_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [CHANGESET_ID] BIGINT NOT NULL,
    [OLD_TAGS] NVARCHAR(max),
    [NEW_TAGS] NVARCHAR(max),
    CONSTRAINT [PK_TST_HISTORY_TAGS] PRIMARY KEY ([HISTORY_TAGS_ID])
)
GO


CREATE  INDEX [IDX_TST_HISTORY_TAGS_1_FK] ON [TST_HISTORY_TAGS] ([CHANGESET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_FOLDER_HIERARCHY"                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_FOLDER_HIERARCHY] (
    [TEST_CASE_FOLDER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255),
    [PARENT_TEST_CASE_FOLDER_ID] INTEGER,
    [HIERARCHY_LEVEL] INTEGER,
    [INDENT_LEVEL] NVARCHAR(255) COLLATE Latin1_General_BIN,
    CONSTRAINT [PK_TST_TEST_CASE_FOLDER_HIERARCHY] PRIMARY KEY ([TEST_CASE_FOLDER_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_FOLDER_HIERARCHY_1_FK] ON [TST_TEST_CASE_FOLDER_HIERARCHY] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_SET_FOLDER_HIERARCHY"                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_SET_FOLDER_HIERARCHY] (
    [TEST_SET_FOLDER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255),
    [PARENT_TEST_SET_FOLDER_ID] INTEGER,
    [HIERARCHY_LEVEL] INTEGER,
    [INDENT_LEVEL] NVARCHAR(255) COLLATE Latin1_General_BIN,
    CONSTRAINT [PK_TST_TEST_SET_FOLDER_HIERARCHY] PRIMARY KEY ([TEST_SET_FOLDER_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_SET_FOLDER_HIERARCHY_1_FK] ON [TST_TEST_SET_FOLDER_HIERARCHY] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY"                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY] (
    [PROJECT_ATTACHMENT_FOLDER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255),
    [PARENT_PROJECT_ATTACHMENT_FOLDER_ID] INTEGER,
    [HIERARCHY_LEVEL] INTEGER,
    [INDENT_LEVEL] NVARCHAR(255) COLLATE Latin1_General_BIN,
    CONSTRAINT [PK_TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY] PRIMARY KEY ([PROJECT_ATTACHMENT_FOLDER_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY_1_FK] ON [TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_FOLDER_HIERARCHY"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_FOLDER_HIERARCHY] (
    [TASK_FOLDER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255),
    [PARENT_TASK_FOLDER_ID] INTEGER,
    [HIERARCHY_LEVEL] INTEGER,
    [INDENT_LEVEL] NVARCHAR(255) COLLATE Latin1_General_BIN,
    CONSTRAINT [PK_TST_TASK_FOLDER_HIERARCHY] PRIMARY KEY ([TASK_FOLDER_ID])
)
GO


CREATE  INDEX [IDX_TST_TASK_FOLDER_HIERARCHY_1_FK] ON [TST_TASK_FOLDER_HIERARCHY] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_PARAMETER_HIERARCHY"                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_PARAMETER_HIERARCHY] (
    [TEST_CASE_PARAMETER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [TEST_CASE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DEFAULT_VALUE] NVARCHAR(255),
    PRIMARY KEY ([TEST_CASE_PARAMETER_ID], [PROJECT_ID], [TEST_CASE_ID], [NAME])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_PARAMETER_HIERARCHY_1_FK] ON [TST_TEST_CASE_PARAMETER_HIERARCHY] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET"              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET] (
    [TEST_CASE_PARAMETER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [TEST_CASE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DEFAULT_VALUE] NVARCHAR(255),
    PRIMARY KEY ([TEST_CASE_PARAMETER_ID], [PROJECT_ID], [TEST_CASE_ID], [NAME])
)
GO


CREATE  INDEX [IDX_TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET_1_FK] ON [TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_SOURCE_CODE_COMMIT"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_SOURCE_CODE_COMMIT] (
    [VERSION_CONTROL_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [REVISION_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [REVISION_KEY] NVARCHAR(255) NOT NULL,
    [AUTHOR_NAME] NVARCHAR(255) NOT NULL,
    [MESSAGE] NVARCHAR(max),
    [UPDATE_DATE] DATETIME NOT NULL,
    [CONTENT_CHANGED] BIT NOT NULL,
    [PROPERTIES_CHANGED] BIT NOT NULL,
    CONSTRAINT [PK_TST_SOURCE_CODE_COMMIT] PRIMARY KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID], [REVISION_ID])
)
GO


CREATE  INDEX [IDX_TST_SOURCE_CODE_COMMIT_1_FK] ON [TST_SOURCE_CODE_COMMIT] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID])
GO


CREATE NONCLUSTERED INDEX [AK_TST_SOURCE_CODE_COMMIT_2] ON [TST_SOURCE_CODE_COMMIT] ([UPDATE_DATE] DESC)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_SOURCE_CODE_FILE_ENTRY"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_SOURCE_CODE_FILE_ENTRY] (
    [FILE_KEY] NVARCHAR(255) NOT NULL,
    [VERSION_CONTROL_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [REVISION_ID] INTEGER NOT NULL,
    [ACTION] NVARCHAR(255),
    CONSTRAINT [PK_TST_SOURCE_CODE_FILE_ENTRY] PRIMARY KEY ([FILE_KEY], [VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID], [REVISION_ID])
)
GO


CREATE  INDEX [IDX_TST_SOURCE_CODE_FILE_ENTRY_1_FK] ON [TST_SOURCE_CODE_FILE_ENTRY] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID],[REVISION_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_SOURCE_CODE_COMMIT_BRANCH"                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_SOURCE_CODE_COMMIT_BRANCH] (
    [VERSION_CONTROL_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [REVISION_ID] INTEGER NOT NULL,
    [BRANCH_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_SOURCE_CODE_COMMIT_BRANCH] PRIMARY KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID], [REVISION_ID], [BRANCH_ID])
)
GO


CREATE  INDEX [IDX_TST_SOURCE_CODE_COMMIT_BRANCH_1_FK] ON [TST_SOURCE_CODE_COMMIT_BRANCH] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID],[REVISION_ID])
GO


CREATE  INDEX [IDX_TST_SOURCE_CODE_COMMIT_BRANCH_2_FK] ON [TST_SOURCE_CODE_COMMIT_BRANCH] ([BRANCH_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_SOURCE_CODE_COMMIT_ARTIFACT"                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_SOURCE_CODE_COMMIT_ARTIFACT] (
    [VERSION_CONTROL_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [REVISION_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_SOURCE_CODE_COMMIT_ARTIFACT] PRIMARY KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID], [REVISION_ID], [ARTIFACT_TYPE_ID], [ARTIFACT_ID])
)
GO


CREATE  INDEX [IDX_TST_SOURCE_CODE_COMMIT_ARTIFACT_1_FK] ON [TST_SOURCE_CODE_COMMIT_ARTIFACT] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID],[REVISION_ID])
GO


CREATE  INDEX [IDX_TST_SOURCE_CODE_COMMIT_ARTIFACT_2_FK] ON [TST_SOURCE_CODE_COMMIT_ARTIFACT] ([ARTIFACT_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_RECENT_ARTIFACT"                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_RECENT_ARTIFACT] (
    [USER_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [LAST_ACCESS_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_USER_RECENT_ARTIFACT] PRIMARY KEY ([USER_ID], [ARTIFACT_ID], [ARTIFACT_TYPE_ID])
)
GO


CREATE  INDEX [IDX_TST_USER_RECENT_ARTIFACT_1_FK] ON [TST_USER_RECENT_ARTIFACT] ([USER_ID])
GO


CREATE  INDEX [IDX_TST_USER_RECENT_ARTIFACT_2_FK] ON [TST_USER_RECENT_ARTIFACT] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_USER_RECENT_ARTIFACT_3_FK] ON [TST_USER_RECENT_ARTIFACT] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_RECENT_PROJECT"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_RECENT_PROJECT] (
    [USER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [LAST_ACCESS_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_USER_RECENT_PROJECT] PRIMARY KEY ([USER_ID], [PROJECT_ID])
)
GO


CREATE  INDEX [IDX_TST_USER_RECENT_PROJECT_1_FK] ON [TST_USER_RECENT_PROJECT] ([USER_ID])
GO


CREATE  INDEX [IDX_TST_USER_RECENT_PROJECT_2_FK] ON [TST_USER_RECENT_PROJECT] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_STANDARD_TASK"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_STANDARD_TASK] (
    [STANDARD_TASK_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [STANDARD_TASK_SET_ID] INTEGER NOT NULL,
    [TASK_TYPE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    CONSTRAINT [PK_TST_STANDARD_TASK] PRIMARY KEY ([STANDARD_TASK_ID])
)
GO


CREATE  INDEX [IDX_TST_STANDARD_TASK_1_FK] ON [TST_STANDARD_TASK] ([STANDARD_TASK_SET_ID])
GO


CREATE  INDEX [IDX_TST_STANDARD_TASK_2_FK] ON [TST_STANDARD_TASK] ([TASK_TYPE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_DATA_SYNC_ARTIFACT_MAPPING"                             */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_DATA_SYNC_ARTIFACT_MAPPING] (
    [DATA_SYNC_SYSTEM_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [EXTERNAL_KEY] NVARCHAR(255) NOT NULL,
    CONSTRAINT [XPKTST_DATA_SYNC_ARTIFACT_MAPPING] PRIMARY KEY ([DATA_SYNC_SYSTEM_ID], [PROJECT_ID], [ARTIFACT_TYPE_ID], [ARTIFACT_ID])
)
GO


CREATE  INDEX [AK_TST_DATA_SYNC_ARTIFACT_MAPPING_1] ON [TST_DATA_SYNC_ARTIFACT_MAPPING] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_DATA_SYNC_ARTIFACT_MAPPING_2] ON [TST_DATA_SYNC_ARTIFACT_MAPPING] ([DATA_SYNC_SYSTEM_ID],[PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_COLLECTION_ENTRY"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_COLLECTION_ENTRY] (
    [USER_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [PROJECT_COLLECTION_ID] INTEGER NOT NULL,
    [ENTRY_KEY] NVARCHAR(255) NOT NULL,
    [ENTRY_VALUE] NVARCHAR(255) NOT NULL,
    [ENTRY_TYPE_CODE] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_PROJECT_COLLECTION_ENTRY] PRIMARY KEY ([USER_ID], [PROJECT_ID], [PROJECT_COLLECTION_ID], [ENTRY_KEY])
)
GO


CREATE  INDEX [AK_TST_PROJECT_COLLECTION_ENTRY_1] ON [TST_PROJECT_COLLECTION_ENTRY] ([USER_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_COLLECTION_ENTRY_2] ON [TST_PROJECT_COLLECTION_ENTRY] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_COLLECTION_ENTRY_3] ON [TST_PROJECT_COLLECTION_ENTRY] ([PROJECT_COLLECTION_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_ARTIFACT_FIELD"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_ARTIFACT_FIELD] (
    [ARTIFACT_FIELD_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [IS_VISIBLE] BIT NOT NULL,
    [LIST_POSITION] INTEGER,
    [WIDTH] INTEGER,
    CONSTRAINT [XPKTST_USER_ARTIFACT_FIELD] PRIMARY KEY ([ARTIFACT_FIELD_ID], [PROJECT_ID], [USER_ID])
)
GO


CREATE  INDEX [AK_TST_USER_ARTIFACT_FIELD_1] ON [TST_USER_ARTIFACT_FIELD] ([USER_ID])
GO


CREATE  INDEX [AK_TST_USER_ARTIFACT_FIELD_2] ON [TST_USER_ARTIFACT_FIELD] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_USER_ARTIFACT_FIELD_3] ON [TST_USER_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW_TRANSITION_ROLE"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW_TRANSITION_ROLE] (
    [WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    [WORKFLOW_TRANSITION_ROLE_TYPE_ID] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_WORKFLOW_TRANSITION_ROLE] PRIMARY KEY ([WORKFLOW_TRANSITION_ID], [PROJECT_ROLE_ID], [WORKFLOW_TRANSITION_ROLE_TYPE_ID])
)
GO


CREATE  INDEX [AK_TST_WORKFLOW_TRANSITION_ROLE_1] ON [TST_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_TRANSITION_ROLE_2] ON [TST_WORKFLOW_TRANSITION_ROLE] ([PROJECT_ROLE_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_TRANSITION_ROLE_3] ON [TST_WORKFLOW_TRANSITION_ROLE] ([WORKFLOW_TRANSITION_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_WORKFLOW_CUSTOM_PROPERTY"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_WORKFLOW_CUSTOM_PROPERTY] (
    [WORKFLOW_FIELD_STATE_ID] INTEGER NOT NULL,
    [WORKFLOW_ID] INTEGER NOT NULL,
    [INCIDENT_STATUS_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_WORKFLOW_CUSTOM_PROPERTY] PRIMARY KEY ([WORKFLOW_FIELD_STATE_ID], [WORKFLOW_ID], [INCIDENT_STATUS_ID], [CUSTOM_PROPERTY_ID])
)
GO


CREATE  INDEX [AK_TST_WORKFLOW_CUSTOM_PROPERTY_1] ON [TST_WORKFLOW_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_CUSTOM_PROPERTY_2] ON [TST_WORKFLOW_CUSTOM_PROPERTY] ([INCIDENT_STATUS_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_CUSTOM_PROPERTY_3] ON [TST_WORKFLOW_CUSTOM_PROPERTY] ([WORKFLOW_ID])
GO


CREATE  INDEX [AK_TST_WORKFLOW_CUSTOM_PROPERTY_4] ON [TST_WORKFLOW_CUSTOM_PROPERTY] ([WORKFLOW_FIELD_STATE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_HISTORY_DETAIL"                                         */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_HISTORY_DETAIL] (
    [ARTIFACT_HISTORY_ID] BIGINT IDENTITY(1,1) NOT NULL,
    [FIELD_NAME] NVARCHAR(50) NOT NULL,
    [OLD_VALUE] NVARCHAR(max),
    [FIELD_CAPTION] NVARCHAR(50) NOT NULL,
    [NEW_VALUE] NVARCHAR(max),
    [OLD_VALUE_INT] INTEGER,
    [OLD_VALUE_DATE] DATETIME,
    [NEW_VALUE_INT] INTEGER,
    [NEW_VALUE_DATE] DATETIME,
    [CHANGESET_ID] BIGINT NOT NULL,
    [FIELD_ID] INTEGER,
    [CUSTOM_PROPERTY_ID] INTEGER,
    CONSTRAINT [XPKTST_ARTIFACT_HISTORY] PRIMARY KEY ([ARTIFACT_HISTORY_ID])
)
GO


CREATE  INDEX [AK_TST_HISTORY_DETAIL_1] ON [TST_HISTORY_DETAIL] ([CHANGESET_ID])
GO


CREATE  INDEX [AK_TST_HISTORY_DETAIL_2] ON [TST_HISTORY_DETAIL] ([FIELD_ID])
GO


CREATE  INDEX [AK_TST_HISTORY_DETAIL_3] ON [TST_HISTORY_DETAIL] ([CUSTOM_PROPERTY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_USER_CUSTOM_PROPERTY"                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_USER_CUSTOM_PROPERTY] (
    [USER_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [IS_VISIBLE] BIT NOT NULL,
    [LIST_POSITION] INTEGER,
    [WIDTH] INTEGER,
    CONSTRAINT [XPKTST_USER_CUSTOM_PROPERTY] PRIMARY KEY ([USER_ID], [CUSTOM_PROPERTY_ID], [PROJECT_ID])
)
GO


CREATE  INDEX [AK_TST_USER_CUSTOM_PROPERTY_1] ON [TST_USER_CUSTOM_PROPERTY] ([USER_ID])
GO


CREATE  INDEX [AK_TST_USER_CUSTOM_PROPERTY_2] ON [TST_USER_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


CREATE  INDEX [AK_TST_USER_CUSTOM_PROPERTY_3] ON [TST_USER_CUSTOM_PROPERTY] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_CUSTOM_PROPERTY"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_CUSTOM_PROPERTY] (
    [ARTIFACT_ID] INTEGER NOT NULL,
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [CUST_01] NVARCHAR(max),
    [CUST_02] NVARCHAR(max),
    [CUST_03] NVARCHAR(max),
    [CUST_04] NVARCHAR(max),
    [CUST_05] NVARCHAR(max),
    [CUST_06] NVARCHAR(max),
    [CUST_07] NVARCHAR(max),
    [CUST_08] NVARCHAR(max),
    [CUST_09] NVARCHAR(max),
    [CUST_10] NVARCHAR(max),
    [CUST_11] NVARCHAR(max),
    [CUST_12] NVARCHAR(max),
    [CUST_13] NVARCHAR(max),
    [CUST_14] NVARCHAR(max),
    [CUST_15] NVARCHAR(max),
    [CUST_16] NVARCHAR(max),
    [CUST_17] NVARCHAR(max),
    [CUST_18] NVARCHAR(max),
    [CUST_19] NVARCHAR(max),
    [CUST_20] NVARCHAR(max),
    [CUST_21] NVARCHAR(max),
    [CUST_22] NVARCHAR(max),
    [CUST_23] NVARCHAR(max),
    [CUST_24] NVARCHAR(max),
    [CUST_25] NVARCHAR(max),
    [CUST_26] NVARCHAR(max),
    [CUST_27] NVARCHAR(max),
    [CUST_28] NVARCHAR(max),
    [CUST_29] NVARCHAR(max),
    [CUST_30] NVARCHAR(max),
    [CUST_31] NVARCHAR(max),
    [CUST_32] NVARCHAR(max),
    [CUST_33] NVARCHAR(max),
    [CUST_34] NVARCHAR(max),
    [CUST_35] NVARCHAR(max),
    [CUST_36] NVARCHAR(max),
    [CUST_37] NVARCHAR(max),
    [CUST_38] NVARCHAR(max),
    [CUST_39] NVARCHAR(max),
    [CUST_40] NVARCHAR(max),
    [CUST_41] NVARCHAR(max),
    [CUST_42] NVARCHAR(max),
    [CUST_43] NVARCHAR(max),
    [CUST_44] NVARCHAR(max),
    [CUST_45] NVARCHAR(max),
    [CUST_46] NVARCHAR(max),
    [CUST_47] NVARCHAR(max),
    [CUST_48] NVARCHAR(max),
    [CUST_49] NVARCHAR(max),
    [CUST_50] NVARCHAR(max),
    [CUST_51] NVARCHAR(max),
    [CUST_52] NVARCHAR(max),
    [CUST_53] NVARCHAR(max),
    [CUST_54] NVARCHAR(max),
    [CUST_55] NVARCHAR(max),
    [CUST_56] NVARCHAR(max),
    [CUST_57] NVARCHAR(max),
    [CUST_58] NVARCHAR(max),
    [CUST_59] NVARCHAR(max),
    [CUST_60] NVARCHAR(max),
    [CUST_61] NVARCHAR(max),
    [CUST_62] NVARCHAR(max),
    [CUST_63] NVARCHAR(max),
    [CUST_64] NVARCHAR(max),
    [CUST_65] NVARCHAR(max),
    [CUST_66] NVARCHAR(max),
    [CUST_67] NVARCHAR(max),
    [CUST_68] NVARCHAR(max),
    [CUST_69] NVARCHAR(max),
    [CUST_70] NVARCHAR(max),
    [CUST_71] NVARCHAR(max),
    [CUST_72] NVARCHAR(max),
    [CUST_73] NVARCHAR(max),
    [CUST_74] NVARCHAR(max),
    [CUST_75] NVARCHAR(max),
    [CUST_76] NVARCHAR(max),
    [CUST_77] NVARCHAR(max),
    [CUST_78] NVARCHAR(max),
    [CUST_79] NVARCHAR(max),
    [CUST_80] NVARCHAR(max),
    [CUST_81] NVARCHAR(max),
    [CUST_82] NVARCHAR(max),
    [CUST_83] NVARCHAR(max),
    [CUST_84] NVARCHAR(max),
    [CUST_85] NVARCHAR(max),
    [CUST_86] NVARCHAR(max),
    [CUST_87] NVARCHAR(max),
    [CUST_88] NVARCHAR(max),
    [CUST_89] NVARCHAR(max),
    [CUST_90] NVARCHAR(max),
    [CUST_91] NVARCHAR(max),
    [CUST_92] NVARCHAR(max),
    [CUST_93] NVARCHAR(max),
    [CUST_94] NVARCHAR(max),
    [CUST_95] NVARCHAR(max),
    [CUST_96] NVARCHAR(max),
    [CUST_97] NVARCHAR(max),
    [CUST_98] NVARCHAR(max),
    [CUST_99] NVARCHAR(max),
    CONSTRAINT [XPKTST_ARTIFACT_CUSTOM_PROPERTY] PRIMARY KEY ([ARTIFACT_ID], [ARTIFACT_TYPE_ID])
)
GO


CREATE  INDEX [AK_TST_ARTIFACT_CUSTOM_PROPERTY_1] ON [TST_ARTIFACT_CUSTOM_PROPERTY] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_ARTIFACT_CUSTOM_PROPERTY_2_FK] ON [TST_ARTIFACT_CUSTOM_PROPERTY] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ARTIFACT_ATTACHMENT"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ARTIFACT_ATTACHMENT] (
    [ARTIFACT_TYPE_ID] INTEGER NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [ATTACHMENT_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_ARTIFACT_ATTACHMENT] PRIMARY KEY ([ARTIFACT_TYPE_ID], [ARTIFACT_ID], [ATTACHMENT_ID])
)
GO


CREATE  INDEX [AK_TST_ARTIFACT_ATTACHMENT_1] ON [TST_ARTIFACT_ATTACHMENT] ([ARTIFACT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_ARTIFACT_ATTACHMENT_2] ON [TST_ARTIFACT_ATTACHMENT] ([ATTACHMENT_ID])
GO


CREATE  INDEX [AK_TST_ARTIFACT_ATTACHMENT_3] ON [TST_ARTIFACT_ATTACHMENT] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_USER"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_USER] (
    [PROJECT_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [PROJECT_ROLE_ID] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_PROJECT_USER] PRIMARY KEY ([PROJECT_ID], [USER_ID])
)
GO


CREATE  INDEX [AK_TST_PROJECT_USER_1] ON [TST_PROJECT_USER] ([PROJECT_ROLE_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_USER_2] ON [TST_PROJECT_USER] ([USER_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_USER_3] ON [TST_PROJECT_USER] ([PROJECT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE"                                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE] (
    [RELEASE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    [RELEASE_TYPE_ID] INTEGER NOT NULL,
    [RELEASE_STATUS_ID] INTEGER NOT NULL,
    [OWNER_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    [VERSION_NUMBER] NVARCHAR(20) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [INDENT_LEVEL] NVARCHAR(100) COLLATE Latin1_General_BIN NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [IS_SUMMARY] BIT NOT NULL,
    [IS_ATTACHMENTS] BIT NOT NULL,
    [START_DATE] DATETIME NOT NULL,
    [END_DATE] DATETIME NOT NULL,
    [RESOURCE_COUNT] DECIMAL NOT NULL,
    [DAYS_NON_WORKING] DECIMAL NOT NULL,
    [PLANNED_EFFORT] INTEGER NOT NULL,
    [AVAILABLE_EFFORT] INTEGER NOT NULL,
    [COUNT_BLOCKED] INTEGER NOT NULL,
    [COUNT_CAUTION] INTEGER NOT NULL,
    [COUNT_FAILED] INTEGER NOT NULL,
    [COUNT_NOT_APPLICABLE] INTEGER NOT NULL,
    [COUNT_NOT_RUN] INTEGER NOT NULL,
    [COUNT_PASSED] INTEGER NOT NULL,
    [TASK_COUNT] INTEGER NOT NULL,
    [TASK_PERCENT_ON_TIME] INTEGER NOT NULL,
    [TASK_PERCENT_LATE_FINISH] INTEGER NOT NULL,
    [TASK_PERCENT_NOT_START] INTEGER NOT NULL,
    [TASK_PERCENT_LATE_START] INTEGER NOT NULL,
    [TASK_ESTIMATED_EFFORT] INTEGER,
    [TASK_ACTUAL_EFFORT] INTEGER,
    [TASK_PROJECTED_EFFORT] INTEGER,
    [TASK_REMAINING_EFFORT] INTEGER,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_RELEASE_IS_DELETED] DEFAULT 0 NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [MILESTONE_ID] INTEGER,
    [PERCENT_COMPLETE] INTEGER NOT NULL,
    [PLANNED_POINTS] DECIMAL(9,1),
    [REQUIREMENT_POINTS] DECIMAL(9,1),
    [REQUIREMENT_COUNT] INTEGER CONSTRAINT [DEF_TST_RELEASE_REQUIREMENT_COUNT] DEFAULT 0 NOT NULL,
    [BRANCH_ID] INTEGER,
	[PERIODIC_REVIEW_ALERT_ID] INTEGER NULL,
	[PERIODIC_REVIEW_DATE] DATETIME NULL
    CONSTRAINT [XPKTST_RELEASE] PRIMARY KEY ([RELEASE_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TST_RELEASE_INDENT_LEVEL] ON [TST_RELEASE] ([PROJECT_ID],[INDENT_LEVEL])
GO


CREATE UNIQUE  INDEX [AK_TST_RELEASE_VERSION_NUMBER] ON [TST_RELEASE] ([PROJECT_ID],[VERSION_NUMBER])
GO


CREATE  INDEX [AK_TST_RELEASE_3] ON [TST_RELEASE] ([CREATOR_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_4] ON [TST_RELEASE] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_5] ON [TST_RELEASE] ([RELEASE_STATUS_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_6] ON [TST_RELEASE] ([RELEASE_TYPE_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_7] ON [TST_RELEASE] ([OWNER_ID])
GO


CREATE  INDEX [IDX_TST_RELEASE_8_FK] ON [TST_RELEASE] ([MILESTONE_ID])
GO


CREATE  INDEX [IDX_TST_RELEASE_9_FK] ON [TST_RELEASE] ([BRANCH_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE"                                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE] (
    [TEST_CASE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [EXECUTION_STATUS_ID] INTEGER NOT NULL,
    [TEST_CASE_PRIORITY_ID] INTEGER,
    [PROJECT_ID] INTEGER NOT NULL,
    [AUTHOR_ID] INTEGER NOT NULL,
    [TEST_CASE_STATUS_ID] INTEGER NOT NULL,
    [TEST_CASE_TYPE_ID] INTEGER NOT NULL,
    [TEST_CASE_FOLDER_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    [OWNER_ID] INTEGER,
    [DESCRIPTION] NVARCHAR(max),
    [EXECUTION_DATE] DATETIME,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [AUTOMATION_ENGINE_ID] INTEGER,
    [AUTOMATION_ATTACHMENT_ID] INTEGER,
    [IS_ATTACHMENTS] BIT NOT NULL,
    [IS_TEST_STEPS] BIT NOT NULL,
    [ESTIMATED_DURATION] INTEGER,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_TEST_CASE_IS_DELETED] DEFAULT 0 NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [ACTUAL_DURATION] INTEGER,
    [COMPONENT_IDS] NVARCHAR(max),
    [IS_SUSPECT] BIT NOT NULL,
	[TEST_CASE_PREPARATION_STATUS_ID] INTEGER NULL,
	CONSTRAINT [XPKTST_TEST_CASE] PRIMARY KEY ([TEST_CASE_ID])
)
GO


CREATE NONCLUSTERED INDEX [AK_TST_TEST_CASE_1] ON [TST_TEST_CASE] ([TEST_CASE_ID]) INCLUDE ([IS_DELETED])
GO


CREATE  INDEX [AK_TST_TEST_CASE_2] ON [TST_TEST_CASE] ([TEST_CASE_PRIORITY_ID])
GO


CREATE  INDEX [AK_TST_TEST_CASE_3] ON [TST_TEST_CASE] ([OWNER_ID])
GO


CREATE  INDEX [AK_TST_TEST_CASE_4] ON [TST_TEST_CASE] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_TEST_CASE_5] ON [TST_TEST_CASE] ([EXECUTION_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TEST_CASE_6] ON [TST_TEST_CASE] ([AUTHOR_ID])
GO


CREATE  INDEX [AK_TST_TEST_CASE_7] ON [TST_TEST_CASE] ([AUTOMATION_ENGINE_ID])
GO


CREATE  INDEX [AK_TST_TEST_CASE_8] ON [TST_TEST_CASE] ([AUTOMATION_ATTACHMENT_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_9_FK] ON [TST_TEST_CASE] ([TEST_CASE_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_10_FK] ON [TST_TEST_CASE] ([TEST_CASE_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CASE_11_FK] ON [TST_TEST_CASE] ([TEST_CASE_FOLDER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_SET"                                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_SET] (
    [TEST_SET_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [RELEASE_ID] INTEGER,
    [TEST_SET_STATUS_ID] INTEGER NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    [OWNER_ID] INTEGER,
    [AUTOMATION_HOST_ID] INTEGER,
    [TEST_RUN_TYPE_ID] INTEGER,
    [RECURRENCE_ID] INTEGER,
    [TEST_SET_FOLDER_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [CREATION_DATE] DATETIME NOT NULL,
    [PLANNED_DATE] DATETIME,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [IS_ATTACHMENTS] BIT NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_TEST_SET_IS_DELETED] DEFAULT 0 NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [BUILD_EXECUTE_TIME_INTERVAL] INTEGER,
    [ESTIMATED_DURATION] INTEGER,
    [ACTUAL_DURATION] INTEGER,
    [COUNT_PASSED] INTEGER NOT NULL,
    [COUNT_FAILED] INTEGER NOT NULL,
    [COUNT_CAUTION] INTEGER NOT NULL,
    [COUNT_BLOCKED] INTEGER NOT NULL,
    [COUNT_NOT_RUN] INTEGER NOT NULL,
    [COUNT_NOT_APPLICABLE] INTEGER NOT NULL,
    [EXECUTION_DATE] DATETIME,
    [IS_DYNAMIC] BIT NOT NULL,
    [DYNAMIC_QUERY] NVARCHAR(1000),
    [IS_AUTO_SCHEDULED] BIT NOT NULL,
    [TEST_CONFIGURATION_SET_ID] INTEGER,
    CONSTRAINT [PK_TST_TEST_SET] PRIMARY KEY ([TEST_SET_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_SET_1] ON [TST_TEST_SET] ([RELEASE_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_2] ON [TST_TEST_SET] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_3] ON [TST_TEST_SET] ([TEST_SET_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_4] ON [TST_TEST_SET] ([CREATOR_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_5] ON [TST_TEST_SET] ([OWNER_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_6] ON [TST_TEST_SET] ([AUTOMATION_HOST_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_7] ON [TST_TEST_SET] ([TEST_RUN_TYPE_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_8] ON [TST_TEST_SET] ([RECURRENCE_ID])
GO


CREATE  INDEX [IDX_TST_TEST_SET_9_FK] ON [TST_TEST_SET] ([TEST_SET_FOLDER_ID])
GO


CREATE  INDEX [IDX_TST_TEST_SET_10_FK] ON [TST_TEST_SET] ([TEST_CONFIGURATION_SET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_SET_TEST_CASE"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_SET_TEST_CASE] (
    [TEST_SET_TEST_CASE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TEST_SET_ID] INTEGER NOT NULL,
    [TEST_CASE_ID] INTEGER NOT NULL,
    [OWNER_ID] INTEGER,
    [POSITION] INTEGER NOT NULL,
    [PLANNED_DATE] DATETIME,
    [IS_SETUP_TEARDOWN] BIT NOT NULL,
    CONSTRAINT [PK_TST_TEST_SET_TEST_CASE] PRIMARY KEY ([TEST_SET_TEST_CASE_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TST_TEST_SET_TEST_CASE_POSITION] ON [TST_TEST_SET_TEST_CASE] ([TEST_SET_ID],[POSITION])
GO


CREATE  INDEX [AK_TST_TEST_SET_TEST_CASE_2] ON [TST_TEST_SET_TEST_CASE] ([TEST_SET_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_TEST_CASE_3] ON [TST_TEST_SET_TEST_CASE] ([TEST_CASE_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_TEST_CASE_4] ON [TST_TEST_SET_TEST_CASE] ([OWNER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_RUNS_PENDING"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_RUNS_PENDING] (
    [TEST_RUNS_PENDING_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [TEST_SET_ID] INTEGER,
    [TESTER_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [COUNT_PASSED] INTEGER NOT NULL,
    [COUNT_FAILED] INTEGER NOT NULL,
    [COUNT_BLOCKED] INTEGER NOT NULL,
    [COUNT_CAUTION] INTEGER NOT NULL,
    [COUNT_NOT_RUN] INTEGER NOT NULL,
    [COUNT_NOT_APPLICABLE] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_RUNS_PENDING] PRIMARY KEY ([TEST_RUNS_PENDING_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_RUNS_PENDING_1] ON [TST_TEST_RUNS_PENDING] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUNS_PENDING_2] ON [TST_TEST_RUNS_PENDING] ([TESTER_ID],[CREATION_DATE] DESC)
GO


CREATE  INDEX [AK_TST_TEST_RUNS_PENDING_3] ON [TST_TEST_RUNS_PENDING] ([TEST_SET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ATTACHMENT_VERSION"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ATTACHMENT_VERSION] (
    [ATTACHMENT_VERSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ATTACHMENT_ID] INTEGER NOT NULL,
    [AUTHOR_ID] INTEGER NOT NULL,
    [FILENAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [UPLOAD_DATE] DATETIME NOT NULL,
    [SIZE] INTEGER NOT NULL,
    [VERSION_NUMBER] NVARCHAR(5) NOT NULL,
    [IS_CURRENT] BIT NOT NULL,
    [CHANGESET_ID] BIGINT,
    CONSTRAINT [PK_TST_ATTACHMENT_VERSION] PRIMARY KEY ([ATTACHMENT_VERSION_ID])
)
GO


CREATE  INDEX [AK_TST_ATTACHMENT_VERSION_1] ON [TST_ATTACHMENT_VERSION] ([ATTACHMENT_ID])
GO


CREATE  INDEX [AK_TST_ATTACHMENT_VERSION_2] ON [TST_ATTACHMENT_VERSION] ([AUTHOR_ID])
GO


CREATE  INDEX [IDX_TST_ATTACHMENT_VERSION_3_FK] ON [TST_ATTACHMENT_VERSION] ([CHANGESET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_ATTACHMENT"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_ATTACHMENT] (
    [ATTACHMENT_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [DOCUMENT_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_ATTACHMENT_FOLDER_ID] INTEGER NOT NULL,
    [IS_KEY_DOCUMENT] BIT CONSTRAINT [DEF_TST_PROJECT_ATTACHMENT_IS_KEY_DOCUMENT] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_ATTACHMENT] PRIMARY KEY ([ATTACHMENT_ID], [PROJECT_ID])
)
GO


CREATE  INDEX [AK_TST_PROJECT_ATTACHMENT_1] ON [TST_PROJECT_ATTACHMENT] ([ATTACHMENT_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_ATTACHMENT_2] ON [TST_PROJECT_ATTACHMENT] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_ATTACHMENT_3] ON [TST_PROJECT_ATTACHMENT] ([DOCUMENT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_PROJECT_ATTACHMENT_4] ON [TST_PROJECT_ATTACHMENT] ([PROJECT_ATTACHMENT_FOLDER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_DISCUSSION"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_DISCUSSION] (
    [DISCUSSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [TEXT] NVARCHAR(max) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_RELEASE_DISCUSSION_IS_DELETED] DEFAULT 0 NOT NULL,
    [IS_PERMANENT] BIT NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RELEASE_DISCUSSION] PRIMARY KEY ([DISCUSSION_ID])
)
GO


CREATE  INDEX [AK_TST_RELEASE_DISCUSSION_1] ON [TST_RELEASE_DISCUSSION] ([ARTIFACT_ID],[IS_DELETED])
GO


CREATE  INDEX [AK_TST_RELEASE_DISCUSSION_2] ON [TST_RELEASE_DISCUSSION] ([ARTIFACT_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_DISCUSSION_3] ON [TST_RELEASE_DISCUSSION] ([CREATOR_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_DISCUSSION"                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_DISCUSSION] (
    [DISCUSSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [TEXT] NVARCHAR(max) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_TEST_CASE_DISCUSSION_IS_DELETED] DEFAULT 0 NOT NULL,
    [IS_PERMANENT] BIT NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_CASE_DISCUSSION] PRIMARY KEY ([DISCUSSION_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_CASE_DISCUSSION_1] ON [TST_TEST_CASE_DISCUSSION] ([ARTIFACT_ID],[IS_DELETED])
GO


CREATE  INDEX [AK_TST_TEST_CASE_DISCUSSION_2] ON [TST_TEST_CASE_DISCUSSION] ([ARTIFACT_ID])
GO


CREATE  INDEX [AK_TST_TEST_CASE_DISCUSSION_3] ON [TST_TEST_CASE_DISCUSSION] ([CREATOR_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_SET_DISCUSSION"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_SET_DISCUSSION] (
    [DISCUSSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [TEXT] NVARCHAR(max) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_TEST_SET_DISCUSSION_IS_DELETED] DEFAULT 0 NOT NULL,
    [IS_PERMANENT] BIT NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_SET_DISCUSSION] PRIMARY KEY ([DISCUSSION_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_SET_DISCUSSION_1] ON [TST_TEST_SET_DISCUSSION] ([ARTIFACT_ID],[IS_DELETED])
GO


CREATE  INDEX [AK_TST_TEST_SET_DISCUSSION_2] ON [TST_TEST_SET_DISCUSSION] ([ARTIFACT_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_DISCUSSION_3] ON [TST_TEST_SET_DISCUSSION] ([CREATOR_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_BUILD"                                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_BUILD] (
    [BUILD_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [BUILD_STATUS_ID] INTEGER NOT NULL,
    [RELEASE_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_DELETED] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_BUILD] PRIMARY KEY ([BUILD_ID])
)
GO


CREATE NONCLUSTERED INDEX [AK_TST_BUILD_1] ON [TST_BUILD] ([RELEASE_ID] ASC,[BUILD_STATUS_ID] ASC,[PROJECT_ID] ASC,[CREATION_DATE] DESC)
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_BUILD_SOURCE_CODE"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_BUILD_SOURCE_CODE] (
    [BUILD_ID] INTEGER NOT NULL,
    [REVISION_KEY] NVARCHAR(255) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_BUILD_SOURCE_CODE] PRIMARY KEY ([BUILD_ID], [REVISION_KEY])
)
GO


CREATE  INDEX [AK_TST_BUILD_SOURCE_CODE_1] ON [TST_BUILD_SOURCE_CODE] ([BUILD_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_TEST_CASE_FOLDER"                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_TEST_CASE_FOLDER] (
    [RELEASE_ID] INTEGER NOT NULL,
    [TEST_CASE_FOLDER_ID] INTEGER NOT NULL,
    [EXECUTION_DATE] DATETIME,
    [ACTUAL_DURATION] INTEGER,
    [COUNT_PASSED] INTEGER NOT NULL,
    [COUNT_FAILED] INTEGER NOT NULL,
    [COUNT_BLOCKED] INTEGER NOT NULL,
    [COUNT_CAUTION] INTEGER NOT NULL,
    [COUNT_NOT_RUN] INTEGER NOT NULL,
    [COUNT_NOT_APPLICABLE] INTEGER NOT NULL,
    PRIMARY KEY ([RELEASE_ID], [TEST_CASE_FOLDER_ID])
)
GO


CREATE  INDEX [IDX_TST_RELEASE_TEST_CASE_FOLDER_1_FK] ON [TST_RELEASE_TEST_CASE_FOLDER] ([RELEASE_ID])
GO


CREATE  INDEX [IDX_TST_RELEASE_TEST_CASE_FOLDER_2_FK] ON [TST_RELEASE_TEST_CASE_FOLDER] ([TEST_CASE_FOLDER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_TEST_SET"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_TEST_SET] (
    [RELEASE_ID] INTEGER NOT NULL,
    [TEST_SET_ID] INTEGER NOT NULL,
    [ACTUAL_DURATION] INTEGER,
    [COUNT_PASSED] INTEGER NOT NULL,
    [COUNT_FAILED] INTEGER NOT NULL,
    [COUNT_CAUTION] INTEGER NOT NULL,
    [COUNT_BLOCKED] INTEGER NOT NULL,
    [COUNT_NOT_RUN] INTEGER NOT NULL,
    [COUNT_NOT_APPLICABLE] INTEGER NOT NULL,
    [EXECUTION_DATE] DATETIME,
    CONSTRAINT [PK_TST_RELEASE_TEST_SET] PRIMARY KEY ([RELEASE_ID], [TEST_SET_ID])
)
GO


CREATE  INDEX [IDX_TST_RELEASE_TEST_SET_1_FK] ON [TST_RELEASE_TEST_SET] ([RELEASE_ID])
GO


CREATE  INDEX [IDX_TST_RELEASE_TEST_SET_2_FK] ON [TST_RELEASE_TEST_SET] ([TEST_SET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_TEST_SET_FOLDER"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_TEST_SET_FOLDER] (
    [RELEASE_ID] INTEGER NOT NULL,
    [TEST_SET_FOLDER_ID] INTEGER NOT NULL,
    [ACTUAL_DURATION] INTEGER,
    [COUNT_BLOCKED] INTEGER NOT NULL,
    [COUNT_CAUTION] INTEGER NOT NULL,
    [COUNT_FAILED] INTEGER NOT NULL,
    [COUNT_NOT_APPLICABLE] INTEGER NOT NULL,
    [COUNT_NOT_RUN] INTEGER NOT NULL,
    [COUNT_PASSED] INTEGER NOT NULL,
    [EXECUTION_DATE] DATETIME,
    CONSTRAINT [PK_TST_RELEASE_TEST_SET_FOLDER] PRIMARY KEY ([RELEASE_ID], [TEST_SET_FOLDER_ID])
)
GO


CREATE  INDEX [IDX_TST_RELEASE_TEST_SET_FOLDER_1_FK] ON [TST_RELEASE_TEST_SET_FOLDER] ([RELEASE_ID])
GO


CREATE  INDEX [IDX_TST_RELEASE_TEST_SET_FOLDER_2_FK] ON [TST_RELEASE_TEST_SET_FOLDER] ([TEST_SET_FOLDER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CONFIGURATION"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CONFIGURATION] (
    [TEST_CONFIGURATION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TEST_CONFIGURATION_SET_ID] INTEGER NOT NULL,
    [POSITION] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_CONFIGURATION] PRIMARY KEY ([TEST_CONFIGURATION_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CONFIGURATION_1_FK] ON [TST_TEST_CONFIGURATION] ([TEST_CONFIGURATION_SET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK"                                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK] (
    [RISK_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [RISK_IMPACT_ID] INTEGER,
    [RISK_STATUS_ID] INTEGER NOT NULL,
    [RISK_PROBABILITY_ID] INTEGER,
    [RISK_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    [OWNER_ID] INTEGER,
    [PROJECT_GROUP_ID] INTEGER,
    [RELEASE_ID] INTEGER,
    [COMPONENT_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_DELETED] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [REVIEW_DATE] DATETIME,
    [GOAL_ID] INTEGER,
    [RISK_DETECTABILITY_ID] INTEGER,
    [CLOSED_DATE] DATETIME,
    [IS_ATTACHMENTS] BIT NOT NULL,
    CONSTRAINT [PK_TST_RISK] PRIMARY KEY ([RISK_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_1_FK] ON [TST_RISK] ([RISK_IMPACT_ID])
GO


CREATE  INDEX [IDX_TST_RISK_2_FK] ON [TST_RISK] ([RISK_STATUS_ID])
GO


CREATE  INDEX [IDX_TST_RISK_3_FK] ON [TST_RISK] ([RISK_PROBABILITY_ID])
GO


CREATE  INDEX [IDX_TST_RISK_4_FK] ON [TST_RISK] ([RISK_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_RISK_5_FK] ON [TST_RISK] ([PROJECT_ID])
GO


CREATE  INDEX [IDX_TST_RISK_6_FK] ON [TST_RISK] ([RELEASE_ID])
GO


CREATE  INDEX [IDX_TST_RISK_7_FK] ON [TST_RISK] ([COMPONENT_ID])
GO


CREATE  INDEX [IDX_TST_RISK_8_FK] ON [TST_RISK] ([CREATOR_ID])
GO


CREATE  INDEX [IDX_TST_RISK_9_FK] ON [TST_RISK] ([OWNER_ID])
GO


CREATE  INDEX [IDX_TST_RISK_10_FK] ON [TST_RISK] ([PROJECT_GROUP_ID])
GO


CREATE  INDEX [IDX_TST_RISK_11_FK] ON [TST_RISK] ([GOAL_ID])
GO


CREATE  INDEX [IDX_TST_RISK_12_FK] ON [TST_RISK] ([RISK_DETECTABILITY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_DISCUSSION"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_DISCUSSION] (
    [DISCUSSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [TEXT] NVARCHAR(max) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [IS_PERMANENT] BIT NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_RISK_DISCUSSION] PRIMARY KEY ([DISCUSSION_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_DISCUSSION_1_FK] ON [TST_RISK_DISCUSSION] ([ARTIFACT_ID])
GO


CREATE  INDEX [IDX_TST_RISK_DISCUSSION_2_FK] ON [TST_RISK_DISCUSSION] ([CREATOR_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RISK_MITIGATION"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RISK_MITIGATION] (
    [RISK_MITIGATION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [RISK_ID] INTEGER NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_DELETED] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [IS_ACTIVE] BIT NOT NULL,
    [REVIEW_DATE] DATETIME,
    CONSTRAINT [PK_TST_RISK_MITIGATION] PRIMARY KEY ([RISK_MITIGATION_ID])
)
GO


CREATE  INDEX [IDX_TST_RISK_MITIGATION_1_FK] ON [TST_RISK_MITIGATION] ([RISK_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ALLOCATION_PLANNED"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ALLOCATION_PLANNED] (
    [ALLOCATION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [RELEASE_ID] INTEGER NOT NULL,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [PERCENT_ALLOCATED] INTEGER NOT NULL,
    [RESOURCE_CATEGORY_ID] INTEGER,
    [RESOURCE_TRACK_ID] INTEGER,
    CONSTRAINT [PK_TST_ALLOCATION_PLANNED] PRIMARY KEY ([ALLOCATION_ID])
)
GO


CREATE  INDEX [IDX_TST_ALLOCATION_PLANNED_1_FK] ON [TST_ALLOCATION_PLANNED] ([RELEASE_ID])
GO


CREATE  INDEX [IDX_TST_ALLOCATION_PLANNED_2_FK] ON [TST_ALLOCATION_PLANNED] ([RESOURCE_CATEGORY_ID])
GO


CREATE  INDEX [IDX_TST_ALLOCATION_PLANNED_3_FK] ON [TST_ALLOCATION_PLANNED] ([RESOURCE_TRACK_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ALLOCATION_ACTUAL"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_ALLOCATION_ACTUAL] (
    [ALLOCATION_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [PERCENT_ALLOCATED] INTEGER,
    [RESOURCE_CATEGORY_ID] INTEGER,
    CONSTRAINT [PK_TST_ALLOCATION_ACTUAL] PRIMARY KEY ([ALLOCATION_ID], [USER_ID])
)
GO


CREATE  INDEX [IDX_TST_ALLOCATION_ACTUAL_1_FK] ON [TST_ALLOCATION_ACTUAL] ([ALLOCATION_ID])
GO


CREATE  INDEX [IDX_TST_ALLOCATION_ACTUAL_2_FK] ON [TST_ALLOCATION_ACTUAL] ([USER_ID])
GO


CREATE  INDEX [IDX_TST_ALLOCATION_ACTUAL_3_FK] ON [TST_ALLOCATION_ACTUAL] ([RESOURCE_CATEGORY_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_BASELINE"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_PROJECT_BASELINE] (
    [BASELINE_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [CREATOR_USER_ID] INTEGER NOT NULL,
    [CHANGESET_ID] BIGINT NOT NULL,
    [RELEASE_ID] INTEGER,
    [NAME] NVARCHAR(128) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_ACTIVE] BIT NOT NULL,
    [IS_APPROVED] BIT NOT NULL,
    [IS_DELETED] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_PROJECT_BASELINE] PRIMARY KEY ([BASELINE_ID])
)
GO


CREATE  INDEX [IDX_TST_PROJECT_BASELINE_1_FK] ON [TST_PROJECT_BASELINE] ([PROJECT_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_BASELINE_2_FK] ON [TST_PROJECT_BASELINE] ([RELEASE_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_BASELINE_3_FK] ON [TST_PROJECT_BASELINE] ([CHANGESET_ID])
GO


CREATE  INDEX [IDX_TST_PROJECT_BASELINE_4_FK] ON [TST_PROJECT_BASELINE] ([CREATOR_USER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_USER"                                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_USER] (
    [RELEASE_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [IS_EXPANDED] BIT NOT NULL,
    [IS_VISIBLE] BIT NOT NULL,
    CONSTRAINT [XPKTST_RELEASE_USER] PRIMARY KEY ([RELEASE_ID], [USER_ID])
)
GO


CREATE  INDEX [AK_TST_RELEASE_USER_1] ON [TST_RELEASE_USER] ([USER_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_USER_2] ON [TST_RELEASE_USER] ([RELEASE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_INCIDENT"                                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_INCIDENT] (
    [INCIDENT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [SEVERITY_ID] INTEGER,
    [DETECTED_RELEASE_ID] INTEGER,
    [DETECTED_BUILD_ID] INTEGER,
    [PRIORITY_ID] INTEGER,
    [PROJECT_ID] INTEGER NOT NULL,
    [INCIDENT_STATUS_ID] INTEGER NOT NULL,
    [INCIDENT_TYPE_ID] INTEGER NOT NULL,
    [OPENER_ID] INTEGER NOT NULL,
    [OWNER_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    [RESOLVED_RELEASE_ID] INTEGER,
    [RESOLVED_BUILD_ID] INTEGER,
    [DESCRIPTION] NVARCHAR(max) NOT NULL,
    [VERIFIED_RELEASE_ID] INTEGER,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [IS_ATTACHMENTS] BIT NOT NULL,
    [START_DATE] DATETIME,
    [CLOSED_DATE] DATETIME,
    [COMPLETION_PERCENT] INTEGER NOT NULL,
    [ESTIMATED_EFFORT] INTEGER,
    [ACTUAL_EFFORT] INTEGER,
    [PROJECTED_EFFORT] INTEGER,
    [REMAINING_EFFORT] INTEGER,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_INCIDENT_IS_DELETED] DEFAULT 0 NOT NULL,
    [COMPONENT_IDS] NVARCHAR(max),
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [RANK] INTEGER,
    [END_DATE] DATETIME,
    CONSTRAINT [XPKTST_INCIDENT] PRIMARY KEY ([INCIDENT_ID])
)
GO


CREATE  INDEX [AK_TST_INCIDENT_1] ON [TST_INCIDENT] ([SEVERITY_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_2] ON [TST_INCIDENT] ([PRIORITY_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_3] ON [TST_INCIDENT] ([VERIFIED_RELEASE_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_4] ON [TST_INCIDENT] ([RESOLVED_RELEASE_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_5] ON [TST_INCIDENT] ([DETECTED_RELEASE_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_6] ON [TST_INCIDENT] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_7] ON [TST_INCIDENT] ([OWNER_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_8] ON [TST_INCIDENT] ([OPENER_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_9] ON [TST_INCIDENT] ([INCIDENT_STATUS_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_10] ON [TST_INCIDENT] ([INCIDENT_TYPE_ID])
GO


CREATE  INDEX [IDX_TST_INCIDENT_11_FK] ON [TST_INCIDENT] ([RESOLVED_BUILD_ID])
GO


CREATE  INDEX [IDX_TST_INCIDENT_12_FK] ON [TST_INCIDENT] ([DETECTED_BUILD_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_STEP"                                              */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_STEP] (
    [TEST_STEP_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TEST_CASE_ID] INTEGER NOT NULL,
    [EXECUTION_STATUS_ID] INTEGER NOT NULL,
    [DESCRIPTION] NVARCHAR(max) NOT NULL,
    [LINKED_TEST_CASE_ID] INTEGER,
    [POSITION] INTEGER NOT NULL,
    [EXPECTED_RESULT] NVARCHAR(max),
    [SAMPLE_DATA] NVARCHAR(max),
    [IS_ATTACHMENTS] BIT NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_TEST_STEP_IS_DELETED] DEFAULT 0 NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [PRECONDITION] NVARCHAR(max),
    CONSTRAINT [XPKTST_TEST_STEP] PRIMARY KEY ([TEST_STEP_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_STEP_1] ON [TST_TEST_STEP] ([LINKED_TEST_CASE_ID])
GO


CREATE  INDEX [AK_TST_TEST_STEP_2] ON [TST_TEST_STEP] ([EXECUTION_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TEST_STEP_3] ON [TST_TEST_STEP] ([TEST_CASE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT"                                            */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT] (
    [REQUIREMENT_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [AUTHOR_ID] INTEGER NOT NULL,
    [OWNER_ID] INTEGER,
    [RELEASE_ID] INTEGER,
    [PROJECT_ID] INTEGER NOT NULL,
    [REQUIREMENT_TYPE_ID] INTEGER NOT NULL,
    [REQUIREMENT_STATUS_ID] INTEGER NOT NULL,
    [COMPONENT_ID] INTEGER,
    [IMPORTANCE_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [INDENT_LEVEL] NVARCHAR(100) COLLATE Latin1_General_BIN NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [IS_SUMMARY] BIT NOT NULL,
    [IS_ATTACHMENTS] BIT NOT NULL,
    [COVERAGE_COUNT_TOTAL] INTEGER NOT NULL,
    [COVERAGE_COUNT_PASSED] INTEGER NOT NULL,
    [COVERAGE_COUNT_FAILED] INTEGER NOT NULL,
    [COVERAGE_COUNT_CAUTION] INTEGER NOT NULL,
    [COVERAGE_COUNT_BLOCKED] INTEGER NOT NULL,
    [ESTIMATE_POINTS] DECIMAL(9,1),
    [ESTIMATED_EFFORT] INTEGER,
    [TASK_COUNT] INTEGER NOT NULL,
    [TASK_ESTIMATED_EFFORT] INTEGER,
    [TASK_ACTUAL_EFFORT] INTEGER,
    [TASK_PROJECTED_EFFORT] INTEGER,
    [TASK_REMAINING_EFFORT] INTEGER,
    [TASK_PERCENT_ON_TIME] INTEGER NOT NULL,
    [TASK_PERCENT_LATE_FINISH] INTEGER NOT NULL,
    [TASK_PERCENT_NOT_START] INTEGER NOT NULL,
    [TASK_PERCENT_LATE_START] INTEGER NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_REQUIREMENT_IS_DELETED] DEFAULT 0 NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [RANK] INTEGER,
    [THEME_ID] INTEGER,
    [GOAL_ID] INTEGER,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [PERCENT_COMPLETE] INTEGER,
    [IS_SUSPECT] BIT CONSTRAINT [DEF_TST_REQUIREMENT_IS_SUSPECT] DEFAULT 0 NOT NULL,
    CONSTRAINT [XPKTST_REQUIREMENT] PRIMARY KEY ([REQUIREMENT_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TST_REQUIREMENT_INDENT_LEVEL] ON [TST_REQUIREMENT] ([PROJECT_ID],[INDENT_LEVEL])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_2] ON [TST_REQUIREMENT] ([RELEASE_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_3] ON [TST_REQUIREMENT] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_4] ON [TST_REQUIREMENT] ([REQUIREMENT_STATUS_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_5] ON [TST_REQUIREMENT] ([AUTHOR_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_6] ON [TST_REQUIREMENT] ([IMPORTANCE_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_7] ON [TST_REQUIREMENT] ([OWNER_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_8] ON [TST_REQUIREMENT] ([REQUIREMENT_TYPE_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_9] ON [TST_REQUIREMENT] ([COMPONENT_ID])
GO


CREATE  INDEX [IDX_TST_REQUIREMENT_10_FK] ON [TST_REQUIREMENT] ([THEME_ID])
GO


CREATE  INDEX [IDX_TST_REQUIREMENT_11_FK] ON [TST_REQUIREMENT] ([GOAL_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_RELEASE_TEST_CASE"                                      */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_RELEASE_TEST_CASE] (
    [RELEASE_ID] INTEGER NOT NULL,
    [TEST_CASE_ID] INTEGER NOT NULL,
    [EXECUTION_STATUS_ID] INTEGER NOT NULL,
    [EXECUTION_DATE] DATETIME,
    [ACTUAL_DURATION] INTEGER,
    CONSTRAINT [XPKTST_RELEASE_TEST_CASE] PRIMARY KEY ([RELEASE_ID], [TEST_CASE_ID])
)
GO


CREATE  INDEX [AK_TST_RELEASE_TEST_CASE_1] ON [TST_RELEASE_TEST_CASE] ([TEST_CASE_ID])
GO


CREATE  INDEX [AK_TST_RELEASE_TEST_CASE_2] ON [TST_RELEASE_TEST_CASE] ([RELEASE_ID])
GO


CREATE  INDEX [IDX_TST_RELEASE_TEST_CASE_3_FK] ON [TST_RELEASE_TEST_CASE] ([EXECUTION_STATUS_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_RUN"                                               */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_RUN] (
    [TEST_RUN_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TEST_CASE_ID] INTEGER NOT NULL,
    [TEST_RUN_TYPE_ID] INTEGER NOT NULL,
    [RELEASE_ID] INTEGER,
    [TEST_SET_ID] INTEGER,
    [TEST_SET_TEST_CASE_ID] INTEGER,
    [TESTER_ID] INTEGER NOT NULL,
    [TEST_RUNS_PENDING_ID] INTEGER,
    [EXECUTION_STATUS_ID] INTEGER NOT NULL,
    [START_DATE] DATETIME NOT NULL,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [END_DATE] DATETIME,
    [AUTOMATION_HOST_ID] INTEGER,
    [AUTOMATION_ENGINE_ID] INTEGER,
    [BUILD_ID] INTEGER,
    [CHANGESET_ID] BIGINT,
    [TEST_RUN_FORMAT_ID] INTEGER,
    [RUNNER_NAME] NVARCHAR(20),
    [ESTIMATED_DURATION] INTEGER,
    [ACTUAL_DURATION] INTEGER,
    [RUNNER_ASSERT_COUNT] INTEGER,
    [RUNNER_TEST_NAME] NVARCHAR(255),
    [RUNNER_MESSAGE] NVARCHAR(255),
    [RUNNER_STACK_TRACE] NVARCHAR(max),
    [IS_ATTACHMENTS] BIT NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [TEST_CONFIGURATION_ID] INTEGER,
    CONSTRAINT [XPKTST_TEST_RUN] PRIMARY KEY ([TEST_RUN_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_RUN_1] ON [TST_TEST_RUN] ([END_DATE])
GO


CREATE  INDEX [AK_TST_TEST_RUN_2] ON [TST_TEST_RUN] ([TEST_RUN_ID],[RELEASE_ID],[EXECUTION_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_3] ON [TST_TEST_RUN] ([TEST_CASE_ID],[END_DATE] DESC)
GO


CREATE  INDEX [AK_TST_TEST_RUN_4] ON [TST_TEST_RUN] ([END_DATE],[RELEASE_ID],[EXECUTION_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_5] ON [TST_TEST_RUN] ([TEST_CASE_ID],[END_DATE] DESC,[RELEASE_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_6] ON [TST_TEST_RUN] ([TEST_RUN_TYPE_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_7] ON [TST_TEST_RUN] ([RELEASE_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_8] ON [TST_TEST_RUN] ([TESTER_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_9] ON [TST_TEST_RUN] ([EXECUTION_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_10] ON [TST_TEST_RUN] ([TEST_CASE_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_11] ON [TST_TEST_RUN] ([TEST_SET_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_12] ON [TST_TEST_RUN] ([TEST_RUNS_PENDING_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_13] ON [TST_TEST_RUN] ([TEST_SET_TEST_CASE_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_14] ON [TST_TEST_RUN] ([AUTOMATION_HOST_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_15] ON [TST_TEST_RUN] ([AUTOMATION_ENGINE_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_16] ON [TST_TEST_RUN] ([BUILD_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_17] ON [TST_TEST_RUN] ([TEST_RUN_FORMAT_ID])
GO


CREATE  INDEX [IDX_TST_TEST_RUN_18_FK] ON [TST_TEST_RUN] ([CHANGESET_ID])
GO


CREATE  INDEX [IDX_TST_TEST_RUN_19_FK] ON [TST_TEST_RUN] ([TEST_CONFIGURATION_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_PARAMETER"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CASE_PARAMETER] (
    [TEST_CASE_PARAMETER_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TEST_CASE_ID] INTEGER NOT NULL,
    [NAME] NVARCHAR(50) NOT NULL,
    [DEFAULT_VALUE] NVARCHAR(255),
    CONSTRAINT [XPKTST_TEST_CASE_PARAMETER] PRIMARY KEY ([TEST_CASE_PARAMETER_ID])
)
GO


CREATE UNIQUE  INDEX [AK_TEST_CASE_PARAMETER_NAME] ON [TST_TEST_CASE_PARAMETER] ([TEST_CASE_ID],[NAME])
GO


CREATE  INDEX [AK_TST_TEST_CASE_PARAMETER_2] ON [TST_TEST_CASE_PARAMETER] ([TEST_CASE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_INCIDENT_RESOLUTION"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_INCIDENT_RESOLUTION] (
    [INCIDENT_RESOLUTION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [INCIDENT_ID] INTEGER NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    [RESOLUTION] NVARCHAR(max) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_INCIDENT_RESOLUTION] PRIMARY KEY ([INCIDENT_RESOLUTION_ID])
)
GO


CREATE  INDEX [AK_TST_INCIDENT_RESOLUTION_1] ON [TST_INCIDENT_RESOLUTION] ([INCIDENT_ID])
GO


CREATE  INDEX [AK_TST_INCIDENT_RESOLUTION_2] ON [TST_INCIDENT_RESOLUTION] ([CREATOR_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_SET_TEST_CASE_PARAMETER"                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_SET_TEST_CASE_PARAMETER] (
    [TEST_SET_TEST_CASE_ID] INTEGER NOT NULL,
    [TEST_CASE_PARAMETER_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_TEST_SET_TEST_CASE_PARAMETER] PRIMARY KEY ([TEST_SET_TEST_CASE_ID], [TEST_CASE_PARAMETER_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_SET_TEST_CASE_PARAMETER_1] ON [TST_TEST_SET_TEST_CASE_PARAMETER] ([TEST_SET_TEST_CASE_ID])
GO


CREATE  INDEX [AK_TST_TEST_SET_TEST_CASE_PARAMETER_2] ON [TST_TEST_SET_TEST_CASE_PARAMETER] ([TEST_CASE_PARAMETER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_DISCUSSION"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_DISCUSSION] (
    [DISCUSSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [TEXT] NVARCHAR(max) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_REQUIREMENT_DISCUSSION_IS_DELETED] DEFAULT 0 NOT NULL,
    [IS_PERMANENT] BIT NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_REQUIREMENT_DISCUSSION] PRIMARY KEY ([DISCUSSION_ID])
)
GO


CREATE  INDEX [AK_TST_REQUIREMENT_DISCUSSION_1] ON [TST_REQUIREMENT_DISCUSSION] ([ARTIFACT_ID],[IS_DELETED])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_DISCUSSION_2] ON [TST_REQUIREMENT_DISCUSSION] ([ARTIFACT_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_DISCUSSION_3] ON [TST_REQUIREMENT_DISCUSSION] ([CREATOR_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_STEP"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_STEP] (
    [REQUIREMENT_STEP_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [REQUIREMENT_ID] INTEGER NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [IS_DELETED] BIT NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    CONSTRAINT [PK_TST_REQUIREMENT_STEP] PRIMARY KEY ([REQUIREMENT_STEP_ID])
)
GO


CREATE  INDEX [AK_TST_REQUIREMENT_STEP_POSITION] ON [TST_REQUIREMENT_STEP] ([REQUIREMENT_ID],[POSITION])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_STEP_2] ON [TST_REQUIREMENT_STEP] ([REQUIREMENT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_TEST_STEP"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_TEST_STEP] (
    [REQUIREMENT_ID] INTEGER NOT NULL,
    [TEST_STEP_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_REQUIREMENT_TEST_STEP] PRIMARY KEY ([REQUIREMENT_ID], [TEST_STEP_ID])
)
GO


CREATE  INDEX [IDX_TST_REQUIREMENT_TEST_STEP_1_FK] ON [TST_REQUIREMENT_TEST_STEP] ([REQUIREMENT_ID])
GO


CREATE  INDEX [IDX_TST_REQUIREMENT_TEST_STEP_2_FK] ON [TST_REQUIREMENT_TEST_STEP] ([TEST_STEP_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_SET_PARAMETER"                                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_SET_PARAMETER] (
    [TEST_CASE_PARAMETER_ID] INTEGER NOT NULL,
    [TEST_SET_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_TST_TEST_SET_PARAMETER] PRIMARY KEY ([TEST_CASE_PARAMETER_ID], [TEST_SET_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_SET_PARAMETER_1_FK] ON [TST_TEST_SET_PARAMETER] ([TEST_CASE_PARAMETER_ID])
GO


CREATE  INDEX [IDX_TST_TEST_SET_PARAMETER_2_FK] ON [TST_TEST_SET_PARAMETER] ([TEST_SET_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CONFIGURATION_SET_PARAMETER"                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CONFIGURATION_SET_PARAMETER] (
    [TEST_CONFIGURATION_SET_ID] INTEGER NOT NULL,
    [TEST_CASE_PARAMETER_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_CONFIGURATION_SET_PARAMETER] PRIMARY KEY ([TEST_CONFIGURATION_SET_ID], [TEST_CASE_PARAMETER_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CONFIGURATION_SET_PARAMETER_1_FK] ON [TST_TEST_CONFIGURATION_SET_PARAMETER] ([TEST_CONFIGURATION_SET_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CONFIGURATION_SET_PARAMETER_2_FK] ON [TST_TEST_CONFIGURATION_SET_PARAMETER] ([TEST_CASE_PARAMETER_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CONFIGURATION_PARAMETER_VALUE"                     */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_CONFIGURATION_PARAMETER_VALUE] (
    [TEST_CONFIGURATION_SET_ID] INTEGER NOT NULL,
    [TEST_CASE_PARAMETER_ID] INTEGER NOT NULL,
    [TEST_CONFIGURATION_ID] INTEGER NOT NULL,
    [CUSTOM_PROPERTY_VALUE_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_CONFIGURATION_PARAMETER_VALUE] PRIMARY KEY ([TEST_CONFIGURATION_SET_ID], [TEST_CASE_PARAMETER_ID], [TEST_CONFIGURATION_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_CONFIGURATION_PARAMETER_VALUE_1_FK] ON [TST_TEST_CONFIGURATION_PARAMETER_VALUE] ([TEST_CONFIGURATION_SET_ID],[TEST_CASE_PARAMETER_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CONFIGURATION_PARAMETER_VALUE_2_FK] ON [TST_TEST_CONFIGURATION_PARAMETER_VALUE] ([TEST_CONFIGURATION_ID])
GO


CREATE  INDEX [IDX_TST_TEST_CONFIGURATION_PARAMETER_VALUE_3_FK] ON [TST_TEST_CONFIGURATION_PARAMETER_VALUE] ([CUSTOM_PROPERTY_VALUE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK"                                                   */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK] (
    [TASK_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [TASK_STATUS_ID] INTEGER NOT NULL,
    [TASK_TYPE_ID] INTEGER NOT NULL,
    [PROJECT_ID] INTEGER NOT NULL,
    [REQUIREMENT_ID] INTEGER,
    [RELEASE_ID] INTEGER,
    [CREATOR_ID] INTEGER NOT NULL,
    [OWNER_ID] INTEGER,
    [TASK_FOLDER_ID] INTEGER,
    [TASK_PRIORITY_ID] INTEGER,
    [NAME] NVARCHAR(255) NOT NULL,
    [DESCRIPTION] NVARCHAR(max),
    [CREATION_DATE] DATETIME NOT NULL,
    [LAST_UPDATE_DATE] DATETIME NOT NULL,
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [COMPLETION_PERCENT] INTEGER NOT NULL,
    [ESTIMATED_EFFORT] INTEGER,
    [ACTUAL_EFFORT] INTEGER,
    [IS_ATTACHMENTS] BIT NOT NULL,
    [PROJECTED_EFFORT] INTEGER,
    [REMAINING_EFFORT] INTEGER,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_TASK_IS_DELETED] DEFAULT 0 NOT NULL,
    [CONCURRENCY_DATE] DATETIME NOT NULL,
    [RISK_ID] INTEGER,
    CONSTRAINT [XPKTST_TASK] PRIMARY KEY ([TASK_ID])
)
GO


CREATE  INDEX [AK_TST_TASK_1] ON [TST_TASK] ([TASK_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TASK_2] ON [TST_TASK] ([PROJECT_ID])
GO


CREATE  INDEX [AK_TST_TASK_3] ON [TST_TASK] ([RELEASE_ID])
GO


CREATE  INDEX [AK_TST_TASK_4] ON [TST_TASK] ([OWNER_ID])
GO


CREATE  INDEX [AK_TST_TASK_5] ON [TST_TASK] ([REQUIREMENT_ID])
GO


CREATE  INDEX [AK_TST_TASK_6] ON [TST_TASK] ([TASK_PRIORITY_ID])
GO


CREATE  INDEX [AK_TST_TASK_7] ON [TST_TASK] ([CREATOR_ID])
GO


CREATE  INDEX [AK_TST_TASK_8] ON [TST_TASK] ([TASK_TYPE_ID])
GO


CREATE  INDEX [AK_TST_TASK_9] ON [TST_TASK] ([TASK_FOLDER_ID])
GO


CREATE  INDEX [IDX_TST_TASK_10_FK] ON [TST_TASK] ([RISK_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_USER"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_USER] (
    [REQUIREMENT_ID] INTEGER NOT NULL,
    [USER_ID] INTEGER NOT NULL,
    [IS_EXPANDED] BIT NOT NULL,
    [IS_VISIBLE] BIT NOT NULL,
    CONSTRAINT [XPKTST_REQUIREMENT_USER] PRIMARY KEY ([REQUIREMENT_ID], [USER_ID])
)
GO


CREATE  INDEX [AK_TST_REQUIREMENT_USER_1] ON [TST_REQUIREMENT_USER] ([USER_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_USER_2] ON [TST_REQUIREMENT_USER] ([REQUIREMENT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_STEP_PARAMETER"                                    */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_STEP_PARAMETER] (
    [TEST_STEP_ID] INTEGER NOT NULL,
    [TEST_CASE_PARAMETER_ID] INTEGER NOT NULL,
    [VALUE] NVARCHAR(255) NOT NULL,
    CONSTRAINT [XPKTST_TEST_STEP_PARAMETER] PRIMARY KEY ([TEST_STEP_ID], [TEST_CASE_PARAMETER_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_STEP_PARAMETER_1] ON [TST_TEST_STEP_PARAMETER] ([TEST_CASE_PARAMETER_ID])
GO


CREATE  INDEX [AK_TST_TEST_STEP_PARAMETER_2] ON [TST_TEST_STEP_PARAMETER] ([TEST_STEP_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_RUN_STEP"                                          */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_RUN_STEP] (
    [TEST_RUN_STEP_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [EXECUTION_STATUS_ID] INTEGER NOT NULL,
    [TEST_CASE_ID] INTEGER,
    [TEST_STEP_ID] INTEGER,
    [TEST_RUN_ID] INTEGER NOT NULL,
    [DESCRIPTION] NVARCHAR(max) NOT NULL,
    [POSITION] INTEGER NOT NULL,
    [EXPECTED_RESULT] NVARCHAR(max),
    [SAMPLE_DATA] NVARCHAR(max),
    [ACTUAL_RESULT] NVARCHAR(max),
    [START_DATE] DATETIME,
    [END_DATE] DATETIME,
    [ACTUAL_DURATION] INTEGER,
    CONSTRAINT [XPKTST_TEST_RUN_STEP] PRIMARY KEY ([TEST_RUN_STEP_ID])
)
GO


CREATE  INDEX [AK_TST_TEST_RUN_STEP_1] ON [TST_TEST_RUN_STEP] ([TEST_STEP_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_STEP_2] ON [TST_TEST_RUN_STEP] ([EXECUTION_STATUS_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_STEP_3] ON [TST_TEST_RUN_STEP] ([TEST_RUN_ID])
GO


CREATE  INDEX [AK_TST_TEST_RUN_STEP_4] ON [TST_TEST_RUN_STEP] ([TEST_CASE_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_REQUIREMENT_TEST_CASE"                                  */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_REQUIREMENT_TEST_CASE] (
    [REQUIREMENT_ID] INTEGER NOT NULL,
    [TEST_CASE_ID] INTEGER NOT NULL,
    CONSTRAINT [XPKTST_REQUIREMENT_TEST_CASE] PRIMARY KEY ([REQUIREMENT_ID], [TEST_CASE_ID])
)
GO


CREATE  INDEX [AK_TST_REQUIREMENT_TEST_CASE_1] ON [TST_REQUIREMENT_TEST_CASE] ([TEST_CASE_ID])
GO


CREATE  INDEX [AK_TST_REQUIREMENT_TEST_CASE_2] ON [TST_REQUIREMENT_TEST_CASE] ([REQUIREMENT_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TASK_DISCUSSION"                                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TASK_DISCUSSION] (
    [DISCUSSION_ID] INTEGER IDENTITY(1,1) NOT NULL,
    [ARTIFACT_ID] INTEGER NOT NULL,
    [TEXT] NVARCHAR(max) NOT NULL,
    [CREATION_DATE] DATETIME NOT NULL,
    [IS_DELETED] BIT CONSTRAINT [DEF_TST_TASK_DISCUSSION_IS_DELETED] DEFAULT 0 NOT NULL,
    [IS_PERMANENT] BIT NOT NULL,
    [CREATOR_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TASK_DISCUSSION] PRIMARY KEY ([DISCUSSION_ID])
)
GO


CREATE  INDEX [AK_TST_TASK_DISCUSSION_1] ON [TST_TASK_DISCUSSION] ([ARTIFACT_ID],[IS_DELETED])
GO


CREATE  INDEX [AK_TST_TASK_DISCUSSION_2] ON [TST_TASK_DISCUSSION] ([ARTIFACT_ID])
GO


CREATE  INDEX [AK_TST_TASK_DISCUSSION_3] ON [TST_TASK_DISCUSSION] ([CREATOR_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_RUN_STEP_INCIDENT"                                 */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_TEST_RUN_STEP_INCIDENT] (
    [TEST_RUN_STEP_ID] INTEGER NOT NULL,
    [INCIDENT_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_TEST_RUN_STEP_INCIDENT] PRIMARY KEY ([TEST_RUN_STEP_ID], [INCIDENT_ID])
)
GO


CREATE  INDEX [IDX_TST_TEST_RUN_STEP_INCIDENT_1_FK] ON [TST_TEST_RUN_STEP_INCIDENT] ([INCIDENT_ID])
GO


CREATE  INDEX [IDX_TST_TEST_RUN_STEP_INCIDENT_2_FK] ON [TST_TEST_RUN_STEP_INCIDENT] ([TEST_RUN_STEP_ID])
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_VERSION_CONTROL_PULL_REQUEST"                           */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [TST_VERSION_CONTROL_PULL_REQUEST] (
    [SOURCE_BRANCH_ID] INTEGER NOT NULL,
    [DEST_BRANCH_ID] INTEGER NOT NULL,
    [TASK_ID] INTEGER NOT NULL,
    CONSTRAINT [PK_TST_VERSION_CONTROL_PULL_REQUEST] PRIMARY KEY ([SOURCE_BRANCH_ID], [DEST_BRANCH_ID], [TASK_ID])
)
GO


CREATE  INDEX [IDX_TST_VERSION_CONTROL_PULL_REQUEST_1_FK] ON [TST_VERSION_CONTROL_PULL_REQUEST] ([SOURCE_BRANCH_ID])
GO


CREATE  INDEX [IDX_TST_VERSION_CONTROL_PULL_REQUEST_2_FK] ON [TST_VERSION_CONTROL_PULL_REQUEST] ([TASK_ID])
GO


CREATE  INDEX [IDX_TST_VERSION_CONTROL_PULL_REQUEST_3_FK] ON [TST_VERSION_CONTROL_PULL_REQUEST] ([DEST_BRANCH_ID])
GO

/* ---------------------------------------------------------------------- */
/* Add table "[TST_PERIODIC_REVIEW_ALERT]"                                */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [dbo].[TST_PERIODIC_REVIEW_ALERT](
	[AlertId] INTEGER IDENTITY(1,1) NOT NULL,
	[Name] NVARCHAR(40) NOT NULL,
	[Position] [smallint] NOT NULL,
	[AlertInDays] [smallint] NOT NULL,
	[IsActive] BIT NOT NULL,
 CONSTRAINT [XPKTST_PERIODIC_REVIEW_ALERT] PRIMARY KEY CLUSTERED ([AlertId])
) ON [PRIMARY]
GO

/* ---------------------------------------------------------------------- */
/* Add table "TST_PERIODIC_REVIEW_NOTIFICATIONS"                        */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [dbo].[TST_PERIODIC_REVIEW_NOTIFICATIONS](
	[NOTIFICATION_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[RELEASE_ID] [bigint] NOT NULL,
	[SCHEDULED_DATE] [datetime] NOT NULL,
	[STATUS] NVARCHAR(20) NOT NULL,
	[IS_ACTIVE] BIT NOT NULL,
 CONSTRAINT [XPKTST_PERIODIC_REVIEW_NOTIFICATIONS] PRIMARY KEY ([NOTIFICATION_ID])
) ON [PRIMARY]
GO



/* ---------------------------------------------------------------------- */
/* Add table "TST_PROJECT_USER_SIGNATURE"                                 */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_PROJECT_USER_SIGNATURE](
	[PROJECT_USER_SIGNATURE_ID] INTEGER IDENTITY(1,1) NOT NULL,
	[PROJECT_ID] INTEGER NOT NULL,
	[USER_ID] INTEGER NOT NULL,
	[IS_TESTCASE_SIGNATURE_REQUIRED] BIT NULL,
	[LAST_UPDATE_DATE] [datetime] NOT NULL,
	[ARTIFACT_TYPE_ID] INTEGER NOT NULL,
	[TRANSITION_ID] INTEGER NULL,
	[ORDER_ID] INTEGER NULL,
	[IS_ACTIVE] BIT NOT NULL,
 CONSTRAINT [PK_TST_PROJECT_USER_SIGNATURE] PRIMARY KEY CLUSTERED 
(
	[PROJECT_USER_SIGNATURE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TST_PROJECT_USER_SIGNATURE] ADD  CONSTRAINT [DF_TST_PROJECT_USER_SIGNATURE_LAST_UPDATE_DATE]  DEFAULT (getdate()) FOR [LAST_UPDATE_DATE]
GO

ALTER TABLE [dbo].[TST_PROJECT_USER_SIGNATURE]  WITH CHECK ADD  CONSTRAINT [FK_TST_PROJECT_USER_SIGNATURE_TST_PROJECT] FOREIGN KEY([PROJECT_ID])
REFERENCES [dbo].[TST_PROJECT] ([PROJECT_ID])
GO

ALTER TABLE [dbo].[TST_PROJECT_USER_SIGNATURE] CHECK CONSTRAINT [FK_TST_PROJECT_USER_SIGNATURE_TST_PROJECT]
GO

ALTER TABLE [dbo].[TST_PROJECT_USER_SIGNATURE]  WITH CHECK ADD  CONSTRAINT [FK_TST_PROJECT_USER_SIGNATURE_TST_USER] FOREIGN KEY([USER_ID])
REFERENCES [dbo].[TST_USER] ([USER_ID])
GO

ALTER TABLE [dbo].[TST_PROJECT_USER_SIGNATURE] CHECK CONSTRAINT [FK_TST_PROJECT_USER_SIGNATURE_TST_USER]
GO



/* ---------------------------------------------------------------------- */
/* Add table "TST_SHAREPOINT_USERS"                        */
/* ---------------------------------------------------------------------- */

GO
CREATE TABLE [dbo].[TST_SHAREPOINT_USERS](
	[USERNAME] [varchar](50) NULL,
	[PASSWORD] [varchar](50) NULL,
	[TST_USERNAME] [varchar](50) NULL,
	[SH_URL] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO



/* ---------------------------------------------------------------------- */
/* Add table "TST_TEMPLATE"                                               */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_TEMPLATE](
	[TemplateId] INTEGER IDENTITY(1,1) NOT NULL,
	[TemplateName] NVARCHAR(255) NULL,
	[IsCustom] BIT NULL,
	[TemplateLocation] NVARCHAR(255) NULL,
	[PodLocation] NVARCHAR(255) NULL,
	[Active] BIT NULL,
	[CreationDate] [datetime] NOT NULL,
	[ReportCategory] NVARCHAR(50) NULL,
	[CategoryGroup] INTEGER NULL,
 CONSTRAINT [PK_TST_TEMPLATE] PRIMARY KEY CLUSTERED 
(
	[TemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEMPLATE_DATASOURCE"                                    */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_TEMPLATE_DATASOURCE](
	[TemplateDataSourceId] INTEGER IDENTITY(1,1) NOT NULL,
	[TemplateId] INTEGER NOT NULL,
	[Name] NVARCHAR(100) NULL,
	[Type] NVARCHAR(100) NULL,
	[ProviderClass] NVARCHAR(255) NULL,
	[ConnectionString] NVARCHAR(255) NULL,
 CONSTRAINT [PK_TST_TEMPLATE_DATASOURCE] PRIMARY KEY CLUSTERED 
(
	[TemplateDataSourceId] ASC,
	[TemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



/* ---------------------------------------------------------------------- */
/* Add table "TST_TEMPLATE_OUTTYPE"                                       */
/* ---------------------------------------------------------------------- */

GO


CREATE TABLE [dbo].[TST_TEMPLATE_OUTTYPE](
	[OutputTypeId] INTEGER IDENTITY(1,1) NOT NULL,
	[TemplateId] INTEGER NOT NULL,
	[TypeDescription] NVARCHAR(255) NULL,
 CONSTRAINT [PK_TST_TEMPLATE_OUTTYPE] PRIMARY KEY CLUSTERED 
(
	[OutputTypeId] ASC,
	[TemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



/* ---------------------------------------------------------------------- */
/* Add table "TST_TEMPLATE_PARAMETER"                                     */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_TEMPLATE_PARAMETER](
	[ParameterId] INTEGER IDENTITY(1,1) NOT NULL,
	[TemplateId] INTEGER NOT NULL,
	[ParameterLabel] NVARCHAR(255) NULL,
	[ParameterType] NVARCHAR(255) NULL,
 CONSTRAINT [PK_TST_TEMPLATE_PARAMETER] PRIMARY KEY CLUSTERED 
(
	[ParameterId] ASC,
	[TemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_SCHEDULES"                                              */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_SCHEDULES](
	[SCHEDULEID] INTEGER IDENTITY(1,1) NOT NULL,
	[TEMPLATEID] INTEGER NULL,
	[DELIVERYTYPE] NVARCHAR(MAX) NULL,
	[OUTPUTTYPE] NVARCHAR(MAX) NULL,
	[PARAMETERS] NVARCHAR(MAX) NULL,
	[SCHEDULEGROUPID] [UNIQUEIDENTIFIER] NOT NULL,
	[SCHEDULEDTIME] [DATETIME] NOT NULL,
	[STATUS] NVARCHAR(64) NULL,
	[TEMPLATENAME] NVARCHAR(MAX) NULL,
	[USER] NVARCHAR(64) NULL,
	[SERIESID] [UNIQUEIDENTIFIER] NOT NULL,
	[TEMPLATEUNID] NVARCHAR(64) NULL,
	[EMPLOYEENUMBER] NVARCHAR(16) NULL,
	[CALLINGAPP] NVARCHAR(32) NULL,
	[DELIVERYLOCATION] NVARCHAR(32) NULL,
	[DOCUMENTSETID] [UNIQUEIDENTIFIER] NOT NULL,
	[CREATEDDATE] [DATETIME] NOT NULL,

	CONSTRAINT [XPK_TST_SCHEDULES] PRIMARY KEY ([ScheduleId])
)
--PRIMARY KEY ([TEST_RUN_STEP_ID], [INCIDENT_ID])
--CONSTRAINT [PK_TST_SCHEDULES] PRIMARY KEY CLUSTERED 
--(
--	[ScheduleId] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
--) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_ERROR_LOG"                                              */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_ERROR_LOG](
	[ErrorLogId] INTEGER IDENTITY(1,1) NOT NULL,
	[ErrorMessage] NVARCHAR(255) NULL,
	[ErrorContext] NVARCHAR(255) NULL,
	[CreatedDateTimeUtc] [datetime] NOT NULL,
	[CreatedBy] NVARCHAR(255) NULL,
	[CallingApplication] NVARCHAR(255) NULL,
 CONSTRAINT [PK_TST_ERROR_LOG] PRIMARY KEY CLUSTERED 
(
	[ErrorLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



/* ---------------------------------------------------------------------- */
/* Add table "TST_USAGE_LOG"                                              */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_USAGE_LOG](
	[LOGID] INTEGER IDENTITY(1,1) NOT NULL,
	[SCHEDULEID] INTEGER NOT NULL,
	[TEMPLATEID] INTEGER NOT NULL,
	[TEMPLATENAME] NVARCHAR(255) NULL,
	[LOOKUPKEYS] NVARCHAR(255) NULL,
	[SCHEDULEGROUPID] [UNIQUEIDENTIFIER] NULL,
	[SCHEDULEDDATETIME] [DATETIME] NOT NULL,
	[LOGACTION] NVARCHAR(255) NULL,
	[ACTIONRESULT] NVARCHAR(255) NULL,
	[CREATEDDATETIME] [DATETIME] NOT NULL,
	[USER] NVARCHAR(255) NULL,
	CONSTRAINT [XPK_TST_USAGELOG] PRIMARY KEY ([LogId])
)
-- CONSTRAINT [PK_TST_USAGE_LOG] PRIMARY KEY CLUSTERED 
--(
--	[LogId] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
--) ON [PRIMARY]
GO

CREATE  INDEX [AK_TST_USAGELOG_1] ON [TST_USAGE_LOG] ([SCHEDULEID])
GO

CREATE  INDEX [AK_TST_USAGELOG_2] ON [TST_USAGE_LOG] ([TEMPLATEID])
GO

--ALTER TABLE [dbo].[TST_USAGE_LOG]  WITH CHECK ADD  CONSTRAINT [FK_TST_USAGE_LOG_TST_TEMPLATE] FOREIGN KEY([TemplateId])
--REFERENCES [dbo].[TST_TEMPLATE] ([TemplateId])
--GO

--ALTER TABLE [dbo].[TST_USAGE_LOG] CHECK CONSTRAINT [FK_TST_USAGE_LOG_TST_TEMPLATE]
--GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_APPROVAL_WORKFLOW"                            */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_TEST_CASE_APPROVAL_WORKFLOW](
	[TEST_CASE_APPROVAL_WORKFLOW_ID] INTEGER IDENTITY(1,1) NOT NULL,
	[TEST_CASE_ID] INTEGER NOT NULL,
	[IS_ACTIVE] BIT NOT NULL,
	[UPDATE_DATE] [datetime] NOT NULL,
 CONSTRAINT [PK_TST_TEST_CASE_APPROVAL_WORKFLOW] PRIMARY KEY CLUSTERED 
(
	[TEST_CASE_APPROVAL_WORKFLOW_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

 
/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_APPROVAL_USERS"                               */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_TEST_CASE_APPROVAL_USERS](
	[TEST_CASE_APPROVAL_USER_ID] INTEGER IDENTITY(1,1) NOT NULL,
	[PROJECT_ID] INTEGER NOT NULL,
	[USER_ID] INTEGER NOT NULL,
	[WORKFLOW_TRANSITION_ID] INTEGER NOT NULL,
	[IS_ACTIVE] BIT NOT NULL,
	[UPDATE_DATE] [datetime] NOT NULL,
	[ORDER_ID] [smallint] NULL,
 CONSTRAINT [PK_TST_TEST_CASE_APPROVAL_USERS] PRIMARY KEY CLUSTERED 
(
	[TEST_CASE_APPROVAL_USER_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_SIGNATURE"                                    */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_TEST_CASE_SIGNATURE](
	[TST_TEST_CASE_SIGNATURE_ID] INTEGER IDENTITY(1,1) NOT NULL,
	[TEST_CASE_ID] INTEGER NOT NULL,
	[USER_ID] INTEGER NOT NULL,
	[STATUS_ID] INTEGER NOT NULL,
	[REQUESTED_DATE] [datetime] NULL,
	[MEANING] NVARCHAR(512) NULL,
	[UPDATE_DATE] [datetime] NOT NULL,
	[TEST_CASE_APPROVAL_WORKFLOW_ID] INTEGER NOT NULL,
 CONSTRAINT [PK_TST_TEST_CASE_SIGNATURE] PRIMARY KEY CLUSTERED 
(
	[TST_TEST_CASE_SIGNATURE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


/* ---------------------------------------------------------------------- */
/* Add table "TST_TEST_CASE_PREPARATION_STATUS"                           */
/* ---------------------------------------------------------------------- */

GO

CREATE TABLE [dbo].[TST_TEST_CASE_PREPARATION_STATUS](
	[TEST_CASE_PREPARATION_STATUS_ID] INTEGER IDENTITY(1,1) NOT NULL,
	[NAME] NVARCHAR(50) NOT NULL,
	[IS_ACTIVE] BIT NOT NULL,
	[POSITION] INTEGER NOT NULL,
 CONSTRAINT [PK_TST_TEST_CASE_PREPARATION_STATUS] PRIMARY KEY CLUSTERED 
(
	[TEST_CASE_PREPARATION_STATUS_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/*************************************************************/

--ALTER TABLE [dbo].[TST_TEST_CASE_PREPARATION_STATUS]  WITH CHECK ADD  CONSTRAINT [FK_TST_TEST_CASE_TST_TEST_CASE_PREPARATION_STATUS] FOREIGN KEY([TEST_CASE_PREPARATION_STATUS_ID])
--REFERENCES [dbo].[TST_TEST_CASE] ([TEST_CASE_PREPARATION_STATUS_ID])
--GO
ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT  [FK_TST_TEST_CASE_TST_TEST_CASE_PREPARATION_STATUS] 
	FOREIGN KEY([TEST_CASE_PREPARATION_STATUS_ID])
    REFERENCES [TST_TEST_CASE_PREPARATION_STATUS] ([TEST_CASE_PREPARATION_STATUS_ID])
GO



/*************************************************************/


ALTER TABLE [dbo].[TST_TEST_CASE_APPROVAL_USERS]  WITH CHECK ADD  CONSTRAINT [FK_TST_TEST_CASE_APPROVAL_USERS_TST_USER] FOREIGN KEY([USER_ID])
REFERENCES [dbo].[TST_USER] ([USER_ID])
GO

ALTER TABLE [dbo].[TST_TEST_CASE_APPROVAL_USERS] CHECK CONSTRAINT [FK_TST_TEST_CASE_APPROVAL_USERS_TST_USER]
GO


/*************************************************************/


ALTER TABLE [dbo].[TST_TEST_CASE_SIGNATURE]  WITH CHECK ADD  CONSTRAINT [FK_TST_TEST_CASE_SIGNATURE_TST_TEST_CASE] FOREIGN KEY([TEST_CASE_ID])
REFERENCES [dbo].[TST_TEST_CASE] ([TEST_CASE_ID])
GO

ALTER TABLE [dbo].[TST_TEST_CASE_SIGNATURE] CHECK CONSTRAINT [FK_TST_TEST_CASE_SIGNATURE_TST_TEST_CASE]
GO

ALTER TABLE [dbo].[TST_TEST_CASE_SIGNATURE]  WITH CHECK ADD  CONSTRAINT [FK_TST_TEST_CASE_SIGNATURE_TST_TEST_CASE_APPROVAL_WORKFLOW] FOREIGN KEY([TEST_CASE_APPROVAL_WORKFLOW_ID])
REFERENCES [dbo].[TST_TEST_CASE_APPROVAL_WORKFLOW] ([TEST_CASE_APPROVAL_WORKFLOW_ID])
GO

ALTER TABLE [dbo].[TST_TEST_CASE_SIGNATURE] CHECK CONSTRAINT [FK_TST_TEST_CASE_SIGNATURE_TST_TEST_CASE_APPROVAL_WORKFLOW]
GO

ALTER TABLE [dbo].[TST_TEST_CASE_SIGNATURE]  WITH CHECK ADD  CONSTRAINT [FK_TST_TEST_CASE_SIGNATURE_TST_USER] FOREIGN KEY([USER_ID])
REFERENCES [dbo].[TST_USER] ([USER_ID])
GO

ALTER TABLE [dbo].[TST_TEST_CASE_SIGNATURE] CHECK CONSTRAINT [FK_TST_TEST_CASE_SIGNATURE_TST_USER]
GO


/* ---------------------------------------------------------------------- */
/* Add Attributes and defaults to tables.                                 */
/* ---------------------------------------------------------------------- */


ALTER TABLE [dbo].[TST_SCHEDULES] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [SeriesId]
GO

ALTER TABLE [dbo].[TST_SCHEDULES] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [DocumentSetId]
GO

ALTER TABLE [dbo].[TST_SCHEDULES] ADD  DEFAULT ('0001-01-01T00:00:00.000') FOR [CreatedDate]
GO

ALTER TABLE [dbo].[TST_TEMPLATE_DATASOURCE]  WITH CHECK ADD  CONSTRAINT [FK_TST_TEMPLATE_TST_TEMPLATE_DATASOURCE] FOREIGN KEY([TemplateId])
REFERENCES [dbo].[TST_TEMPLATE] ([TemplateId])
GO

ALTER TABLE [dbo].[TST_TEMPLATE_DATASOURCE] CHECK CONSTRAINT [FK_TST_TEMPLATE_TST_TEMPLATE_DATASOURCE]
GO

ALTER TABLE [dbo].[TST_TEMPLATE_OUTTYPE]  WITH CHECK ADD  CONSTRAINT [FK_TST_TEMPLATE_TST_TEMPLATE_OUTTYPE] FOREIGN KEY([TemplateId])
REFERENCES [dbo].[TST_TEMPLATE] ([TemplateId])
GO

ALTER TABLE [dbo].[TST_TEMPLATE_OUTTYPE] CHECK CONSTRAINT [FK_TST_TEMPLATE_TST_TEMPLATE_OUTTYPE]
GO

ALTER TABLE [dbo].[TST_TEMPLATE_PARAMETER]  WITH CHECK ADD  CONSTRAINT [FK_TST_TEMPLATE_TST_TEMPLATE_PARAMETER] FOREIGN KEY([TemplateId])
REFERENCES [dbo].[TST_TEMPLATE] ([TemplateId])
GO

ALTER TABLE [dbo].[TST_TEMPLATE_PARAMETER] CHECK CONSTRAINT [FK_TST_TEMPLATE_TST_TEMPLATE_PARAMETER]
GO


/* ---------------------------------------------------------------------- */
/* Add foreign key constraints                                            */
/* ---------------------------------------------------------------------- */

GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_TASK_TASK_STATUS] 
    FOREIGN KEY ([TASK_STATUS_ID]) REFERENCES [TST_TASK_STATUS] ([TASK_STATUS_ID])
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_TASK_PROJECT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_RELEASE_TASK] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_USER_TASK] 
    FOREIGN KEY ([OWNER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_REQUIREMENT_TASK] 
    FOREIGN KEY ([REQUIREMENT_ID]) REFERENCES [TST_REQUIREMENT] ([REQUIREMENT_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_TASK_TASK_PRIORITY] 
    FOREIGN KEY ([TASK_PRIORITY_ID]) REFERENCES [TST_TASK_PRIORITY] ([TASK_PRIORITY_ID])
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_TST_USER_TST_TASK] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_TST_TASK_TYPE_TST_TASK] 
    FOREIGN KEY ([TASK_TYPE_ID]) REFERENCES [TST_TASK_TYPE] ([TASK_TYPE_ID])
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_TST_TASK_FOLDER_TST_TASK] 
    FOREIGN KEY ([TASK_FOLDER_ID]) REFERENCES [TST_TASK_FOLDER] ([TASK_FOLDER_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TASK] ADD CONSTRAINT [FK_TST_RISK_TST_TASK] 
    FOREIGN KEY ([RISK_ID]) REFERENCES [TST_RISK] ([RISK_ID])
GO


ALTER TABLE [TST_TASK_PRIORITY] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_TASK_PRIORITY] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DATA_SYNC_ARTIFACT_MAPPING] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_DATA_SYNC_ARTIFACT_MAPPING] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_DATA_SYNC_ARTIFACT_MAPPING] ADD CONSTRAINT [FK_TST_DATA_SYNC_PROJECT_TST_DATA_SYNC_ARTIFACT_MAPPING] 
    FOREIGN KEY ([DATA_SYNC_SYSTEM_ID], [PROJECT_ID]) REFERENCES [TST_DATA_SYNC_PROJECT] ([DATA_SYNC_SYSTEM_ID],[PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_USER] ADD CONSTRAINT [FK_USER_RELEASE_USER] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_USER] ADD CONSTRAINT [FK_RELEASE_RELEASE_USER] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_COLLECTION_ENTRY] ADD CONSTRAINT [FK_USER_PROJECT_COLLECTION_ENTRY] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_PROJECT_COLLECTION_ENTRY] ADD CONSTRAINT [FK_PROJECT_PROJECT_COLLECTION_ENTRY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_COLLECTION_ENTRY] ADD CONSTRAINT [FK_PROJECT_COLLECTION_PROJECT_COLLECTION_ENTRY] 
    FOREIGN KEY ([PROJECT_COLLECTION_ID]) REFERENCES [TST_PROJECT_COLLECTION] ([PROJECT_COLLECTION_ID])
GO


ALTER TABLE [TST_USER_ARTIFACT_FIELD] ADD CONSTRAINT [FK_USER_USER_ARTIFACT_FIELD] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_ARTIFACT_FIELD] ADD CONSTRAINT [FK_PROJECT_USER_ARTIFACT_FIELD] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_ARTIFACT_FIELD] ADD CONSTRAINT [FK_ARTIFACT_FIELD_USER_ARTIFACT_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TRANSITION_ROLE_TYPE_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ROLE_TYPE_ID]) REFERENCES [TST_WORKFLOW_TRANSITION_ROLE_TYPE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_PROJECT_ROLE_TRANSITION_ROLE] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID])
GO


ALTER TABLE [TST_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TRANSITION_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ID]) REFERENCES [TST_WORKFLOW_TRANSITION] ([WORKFLOW_TRANSITION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_WORKFLOW_TRANSITION_INPUT_INCIDENT_STATUS] 
    FOREIGN KEY ([INPUT_INCIDENT_STATUS_ID]) REFERENCES [TST_INCIDENT_STATUS] ([INCIDENT_STATUS_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_WORKFLOW_WORKFLOW_TRANSITION] 
    FOREIGN KEY ([WORKFLOW_ID]) REFERENCES [TST_WORKFLOW] ([WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_WORKFLOW_TRANSITION_OUTPUT_INCIDENT_STATUS] 
    FOREIGN KEY ([OUTPUT_INCIDENT_STATUS_ID]) REFERENCES [TST_INCIDENT_STATUS] ([INCIDENT_STATUS_ID])
GO


ALTER TABLE [TST_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_CUSTOM_PROPERTY_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_INCIDENT_STATUS_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([INCIDENT_STATUS_ID]) REFERENCES [TST_INCIDENT_STATUS] ([INCIDENT_STATUS_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_WORKFLOW_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([WORKFLOW_ID]) REFERENCES [TST_WORKFLOW] ([WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_WORKFLOW_FIELD_STATE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_WORKFLOW_FIELD] ADD CONSTRAINT [FK_WORKFLOW_FIELD_INCIDENT_STATUS] 
    FOREIGN KEY ([INCIDENT_STATUS_ID]) REFERENCES [TST_INCIDENT_STATUS] ([INCIDENT_STATUS_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_FIELD] ADD CONSTRAINT [FK_WORKFLOW_WORKFLOW_FIELD] 
    FOREIGN KEY ([WORKFLOW_ID]) REFERENCES [TST_WORKFLOW] ([WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_FIELD] ADD CONSTRAINT [FK_WORKFLOW_FIELD_WORKFLOW_FIELD_STATE] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_WORKFLOW_FIELD] ADD CONSTRAINT [FK_ARTIFACT_FIELD_WORKFLOW_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_INCIDENT_SEVERITY_INCIDENT] 
    FOREIGN KEY ([SEVERITY_ID]) REFERENCES [TST_INCIDENT_SEVERITY] ([SEVERITY_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_INCIDENT_PRIORITY_INCIDENT] 
    FOREIGN KEY ([PRIORITY_ID]) REFERENCES [TST_INCIDENT_PRIORITY] ([PRIORITY_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_INCIDENT_VERIFIED_RELEASE] 
    FOREIGN KEY ([VERIFIED_RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_INCIDENT_RESOLVED_RELEASE] 
    FOREIGN KEY ([RESOLVED_RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_INCIDENT_DETECTED_RELEASE] 
    FOREIGN KEY ([DETECTED_RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_PROJECT_INCIDENT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_USER_INCIDENT] 
    FOREIGN KEY ([OWNER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_INCIDENT_OPENER] 
    FOREIGN KEY ([OPENER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_INCIDENT_STATUS_INCIDENT] 
    FOREIGN KEY ([INCIDENT_STATUS_ID]) REFERENCES [TST_INCIDENT_STATUS] ([INCIDENT_STATUS_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_INCIDENT_TYPE_INCIDENT] 
    FOREIGN KEY ([INCIDENT_TYPE_ID]) REFERENCES [TST_INCIDENT_TYPE] ([INCIDENT_TYPE_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_TST_BUILD_TST_INCIDENT_RESOLVED] 
    FOREIGN KEY ([RESOLVED_BUILD_ID]) REFERENCES [TST_BUILD] ([BUILD_ID])
GO


ALTER TABLE [TST_INCIDENT] ADD CONSTRAINT [FK_TST_BUILD_TST_INCIDENT_DETECTED] 
    FOREIGN KEY ([DETECTED_BUILD_ID]) REFERENCES [TST_BUILD] ([BUILD_ID])
GO


ALTER TABLE [TST_INCIDENT_TYPE] ADD CONSTRAINT [FK_WORKFLOW_INCIDENT_TYPE] 
    FOREIGN KEY ([WORKFLOW_ID]) REFERENCES [TST_WORKFLOW] ([WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_INCIDENT_TYPE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_INCIDENT_TYPE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_WORKFLOW] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_HISTORY_DETAIL] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_DETAIL] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_HISTORY_DETAIL] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_HISTORY_DETAIL] 
    FOREIGN KEY ([FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_HISTORY_DETAIL] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_HISTORY_DETAIL] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_PROJECT_ROLE_PERMISSION] ADD CONSTRAINT [FK_ARTIFACT_TYPE_PROJECT_ROLE_PERMISSION] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_PROJECT_ROLE_PERMISSION] ADD CONSTRAINT [FK_PROJECT_ROLE_PROJECT_ROLE_PERMISSION] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID])
GO


ALTER TABLE [TST_PROJECT_ROLE_PERMISSION] ADD CONSTRAINT [FK_PERMISSION_PROJECT_ROLE_PERMISSION] 
    FOREIGN KEY ([PERMISSION_ID]) REFERENCES [TST_PERMISSION] ([PERMISSION_ID])
GO


ALTER TABLE [TST_ARTIFACT_FIELD] ADD CONSTRAINT [FK_ARTIFACT_FIELD_ARTIFACT_TYPE] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_ARTIFACT_FIELD] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TYPE_TST_ARTIFACT_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_TYPE_ID]) REFERENCES [TST_ARTIFACT_FIELD_TYPE] ([ARTIFACT_FIELD_TYPE_ID])
GO


ALTER TABLE [TST_USER_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_USER_USER_CUSTOM_PROPERTY] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_USER_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_PROJECT_TST_USER_CUSTOM_PROPERTY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_ARTIFACT_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_ARTIFACT_TYPE_ARTIFACT_CUSTOM_PROPERTY] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_ARTIFACT_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_PROJECT_TST_ARTIFACT_CUSTOM_PROPERTY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_VALUE] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_LIST_TST_CUSTOM_PROPERTY_VALUE_PARENT] 
    FOREIGN KEY ([CUSTOM_PROPERTY_LIST_ID]) REFERENCES [TST_CUSTOM_PROPERTY_LIST] ([CUSTOM_PROPERTY_LIST_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_VALUE] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_LIST_TST_CUSTOM_PROPERTY_VALUE_DEPENDENT] 
    FOREIGN KEY ([DEPENDENT_CUSTOM_PROPERTY_LIST_ID]) REFERENCES [TST_CUSTOM_PROPERTY_LIST] ([CUSTOM_PROPERTY_LIST_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_VALUE] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_VALUE_TST_CUSTOM_PROPERTY_VALUE] 
    FOREIGN KEY ([PARENT_CUSTOM_PROPERTY_VALUE_ID]) REFERENCES [TST_CUSTOM_PROPERTY_VALUE] ([CUSTOM_PROPERTY_VALUE_ID])
GO


ALTER TABLE [TST_ARTIFACT_ATTACHMENT] ADD CONSTRAINT [FK_ARTIFACT_TYPE_ARTIFACT_ATTACHMENT] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_ARTIFACT_ATTACHMENT] ADD CONSTRAINT [FK_ATTACHMENT_ARTIFACT_ATTACHMENT] 
    FOREIGN KEY ([ATTACHMENT_ID]) REFERENCES [TST_ATTACHMENT] ([ATTACHMENT_ID])
GO


ALTER TABLE [TST_ARTIFACT_ATTACHMENT] ADD CONSTRAINT [FK_TST_PROJECT_TST_ARTIFACT_ATTACHMENT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_ATTACHMENT] ADD CONSTRAINT [FK_USER_ATTACHMENT] 
    FOREIGN KEY ([AUTHOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_ATTACHMENT] ADD CONSTRAINT [FK_TST_ATTACHMENT_TYPE_TST_ATTACHMENT] 
    FOREIGN KEY ([ATTACHMENT_TYPE_ID]) REFERENCES [TST_ATTACHMENT_TYPE] ([ATTACHMENT_TYPE_ID])
GO


ALTER TABLE [TST_ATTACHMENT] ADD CONSTRAINT [FK_TST_USER_TST_ATTACHMENT] 
    FOREIGN KEY ([EDITOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_ATTACHMENT] ADD CONSTRAINT [FK_TST_DOCUMENT_STATUS_TST_ATTACHMENT] 
    FOREIGN KEY ([DOCUMENT_STATUS_ID]) REFERENCES [TST_DOCUMENT_STATUS] ([DOCUMENT_STATUS_ID])
GO


ALTER TABLE [TST_PROJECT_USER] ADD CONSTRAINT [FK_PROJECT_ROLE_PROJECT_USER] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_USER] ADD CONSTRAINT [FK_USER_PROJECT_USER] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_USER] ADD CONSTRAINT [FK_PROJECT_PROJECT_USER] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_USER] ADD CONSTRAINT [FK_USER_REQUIREMENT_USER] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_USER] ADD CONSTRAINT [FK_REQUIREMENT_REQUIREMENT_USER] 
    FOREIGN KEY ([REQUIREMENT_ID]) REFERENCES [TST_REQUIREMENT] ([REQUIREMENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_INCIDENT_STATUS] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_INCIDENT_STATUS] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_STEP_PARAMETER] ADD CONSTRAINT [FK_TEST_CASE_PARAMETER_TEST_STEP_PARAMETER] 
    FOREIGN KEY ([TEST_CASE_PARAMETER_ID]) REFERENCES [TST_TEST_CASE_PARAMETER] ([TEST_CASE_PARAMETER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_STEP_PARAMETER] ADD CONSTRAINT [FK_TEST_STEP_TEST_STEP_PARAMETER] 
    FOREIGN KEY ([TEST_STEP_ID]) REFERENCES [TST_TEST_STEP] ([TEST_STEP_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_RUN_STEP] ADD CONSTRAINT [FK_TEST_STEP_TEST_RUN_STEP] 
    FOREIGN KEY ([TEST_STEP_ID]) REFERENCES [TST_TEST_STEP] ([TEST_STEP_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_RUN_STEP] ADD CONSTRAINT [FK_EXECUTION_STATUS_TEST_RUN_STEP] 
    FOREIGN KEY ([EXECUTION_STATUS_ID]) REFERENCES [TST_EXECUTION_STATUS] ([EXECUTION_STATUS_ID])
GO


ALTER TABLE [TST_TEST_RUN_STEP] ADD CONSTRAINT [FK_TEST_RUN_TEST_RUN_STEP] 
    FOREIGN KEY ([TEST_RUN_ID]) REFERENCES [TST_TEST_RUN] ([TEST_RUN_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_RUN_STEP] ADD CONSTRAINT [FK_TST_TEST_CASE_TST_TEST_RUN_STEP] 
    FOREIGN KEY ([TEST_CASE_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_STEP] ADD CONSTRAINT [FK_TEST_STEP_LINKED_TEST_CASE] 
    FOREIGN KEY ([LINKED_TEST_CASE_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID])
GO


ALTER TABLE [TST_TEST_STEP] ADD CONSTRAINT [FK_EXECUTION_STATUS_TEST_STEP] 
    FOREIGN KEY ([EXECUTION_STATUS_ID]) REFERENCES [TST_EXECUTION_STATUS] ([EXECUTION_STATUS_ID])
GO


ALTER TABLE [TST_TEST_STEP] ADD CONSTRAINT [FK_TEST_STEP_TEST_CASE] 
    FOREIGN KEY ([TEST_CASE_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID])
GO


ALTER TABLE [TST_REQUIREMENT_TEST_CASE] ADD CONSTRAINT [FK_TST_TEST_CASE_TST_REQUIREMENT_TEST_CASE] 
    FOREIGN KEY ([TEST_CASE_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_TEST_CASE] ADD CONSTRAINT [FK_TST_REQUIREMENT_TST_REQUIREMENT_TEST_CASE] 
    FOREIGN KEY ([REQUIREMENT_ID]) REFERENCES [TST_REQUIREMENT] ([REQUIREMENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_RELEASE_REQUIREMENT] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_PROJECT_REQUIREMENT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_REQUIREMENT_SCOPE_LEVEL] 
    FOREIGN KEY ([REQUIREMENT_STATUS_ID]) REFERENCES [TST_REQUIREMENT_STATUS] ([REQUIREMENT_STATUS_ID])
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_REQUIREMENT_AUTHOR] 
    FOREIGN KEY ([AUTHOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_REQUIREMENT_IMPORTANCE] 
    FOREIGN KEY ([IMPORTANCE_ID]) REFERENCES [TST_IMPORTANCE] ([IMPORTANCE_ID])
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_TST_USER_TST_REQUIREMENT_OWNER] 
    FOREIGN KEY ([OWNER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_TST_REQUIREMENT_TYPE_TST_REQUIREMENT] 
    FOREIGN KEY ([REQUIREMENT_TYPE_ID]) REFERENCES [TST_REQUIREMENT_TYPE] ([REQUIREMENT_TYPE_ID])
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_TST_COMPONENT_TST_REQUIREMENT] 
    FOREIGN KEY ([COMPONENT_ID]) REFERENCES [TST_COMPONENT] ([COMPONENT_ID])
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_THEME_TST_REQUIREMENT] 
    FOREIGN KEY ([THEME_ID]) REFERENCES [TST_PROJECT_GROUP_THEME] ([THEME_ID])
GO


ALTER TABLE [TST_REQUIREMENT] ADD CONSTRAINT [FK_TST_PROJECT_GOAL_TST_REQUIREMENT] 
    FOREIGN KEY ([GOAL_ID]) REFERENCES [TST_PROJECT_GOAL] ([GOAL_ID])
GO


ALTER TABLE [TST_RELEASE_TEST_CASE] ADD CONSTRAINT [FK_TST_TEST_CASE_TST_RELEASE_TEST_CASE] 
    FOREIGN KEY ([TEST_CASE_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_TEST_CASE] ADD CONSTRAINT [FK_TST_RELEASE_TST_RELEASE_TEST_CASE] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_TEST_CASE] ADD CONSTRAINT [FK_TST_EXECUTION_STATUS_TST_RELEASE_TEST_CASE] 
    FOREIGN KEY ([EXECUTION_STATUS_ID]) REFERENCES [TST_EXECUTION_STATUS] ([EXECUTION_STATUS_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TEST_RUN_TYPE_TEST_RUN] 
    FOREIGN KEY ([TEST_RUN_TYPE_ID]) REFERENCES [TST_TEST_RUN_TYPE] ([TEST_RUN_TYPE_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_RELEASE_TEST_RUN] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_USER_TEST_RUN] 
    FOREIGN KEY ([TESTER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_EXECUTION_STATUS_TEST_RUN] 
    FOREIGN KEY ([EXECUTION_STATUS_ID]) REFERENCES [TST_EXECUTION_STATUS] ([EXECUTION_STATUS_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TEST_CASE_TEST_RUN] 
    FOREIGN KEY ([TEST_CASE_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_TEST_SET_TST_TEST_RUN] 
    FOREIGN KEY ([TEST_SET_ID]) REFERENCES [TST_TEST_SET] ([TEST_SET_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_TEST_RUNS_PENDING_TST_TEST_RUN] 
    FOREIGN KEY ([TEST_RUNS_PENDING_ID]) REFERENCES [TST_TEST_RUNS_PENDING] ([TEST_RUNS_PENDING_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_TEST_SET_TEST_CASE_TST_TEST_RUN] 
    FOREIGN KEY ([TEST_SET_TEST_CASE_ID]) REFERENCES [TST_TEST_SET_TEST_CASE] ([TEST_SET_TEST_CASE_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_AUTOMATION_HOST_TST_TEST_RUN] 
    FOREIGN KEY ([AUTOMATION_HOST_ID]) REFERENCES [TST_AUTOMATION_HOST] ([AUTOMATION_HOST_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_AUTOMATION_ENGINE_TST_TEST_RUN] 
    FOREIGN KEY ([AUTOMATION_ENGINE_ID]) REFERENCES [TST_AUTOMATION_ENGINE] ([AUTOMATION_ENGINE_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_BUILD_TST_TEST_RUN] 
    FOREIGN KEY ([BUILD_ID]) REFERENCES [TST_BUILD] ([BUILD_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_TEST_RUN_FORMAT_TST_TEST_RUN] 
    FOREIGN KEY ([TEST_RUN_FORMAT_ID]) REFERENCES [TST_TEST_RUN_FORMAT] ([TEST_RUN_FORMAT_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_TEST_RUN] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID])
GO


ALTER TABLE [TST_TEST_RUN] ADD CONSTRAINT [FK_TST_TEST_CONFIGURATION_TST_TEST_RUN] 
    FOREIGN KEY ([TEST_CONFIGURATION_ID]) REFERENCES [TST_TEST_CONFIGURATION] ([TEST_CONFIGURATION_ID])
GO


ALTER TABLE [TST_RELEASE] ADD CONSTRAINT [FK_CREATOR_RELEASE] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_RELEASE] ADD CONSTRAINT [FK_PROJECT_RELEASE] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_RELEASE] ADD CONSTRAINT [FK_TST_RELEASE_STATUS_TST_RELEASE] 
    FOREIGN KEY ([RELEASE_STATUS_ID]) REFERENCES [TST_RELEASE_STATUS] ([RELEASE_STATUS_ID])
GO


ALTER TABLE [TST_RELEASE] ADD CONSTRAINT [FK_TST_RELEASE_TYPE_TST_RELEASE] 
    FOREIGN KEY ([RELEASE_TYPE_ID]) REFERENCES [TST_RELEASE_TYPE] ([RELEASE_TYPE_ID])
GO


ALTER TABLE [TST_RELEASE] ADD CONSTRAINT [FK_TST_USER_TST_RELEASE_OWNER] 
    FOREIGN KEY ([OWNER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_RELEASE] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_MILESTONE_TST_RELEASE] 
    FOREIGN KEY ([MILESTONE_ID]) REFERENCES [TST_PROJECT_GROUP_MILESTONE] ([MILESTONE_ID])
GO


ALTER TABLE [TST_RELEASE] ADD CONSTRAINT [FK_TST_VERSION_CONTROL_BRANCH_TST_RELEASE] 
    FOREIGN KEY ([BRANCH_ID]) REFERENCES [TST_VERSION_CONTROL_BRANCH] ([BRANCH_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_CASE_PARAMETER] ADD CONSTRAINT [FK_TEST_CASE_TEST_CASE_PARAMETER] 
    FOREIGN KEY ([TEST_CASE_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_TEST_CASE_TEST_CASE_PRIORITY] 
    FOREIGN KEY ([TEST_CASE_PRIORITY_ID]) REFERENCES [TST_TEST_CASE_PRIORITY] ([TEST_CASE_PRIORITY_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_USER_TEST_CASE] 
    FOREIGN KEY ([OWNER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_PROJECT_TEST_CASE] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_EXECUTION_STATUS_TEST_CASE] 
    FOREIGN KEY ([EXECUTION_STATUS_ID]) REFERENCES [TST_EXECUTION_STATUS] ([EXECUTION_STATUS_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_TEST_CASE_AUTHOR] 
    FOREIGN KEY ([AUTHOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_TST_AUTOMATION_ENGINE_TST_TEST_CASE] 
    FOREIGN KEY ([AUTOMATION_ENGINE_ID]) REFERENCES [TST_AUTOMATION_ENGINE] ([AUTOMATION_ENGINE_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_TST_ATTACHMENT_TST_TEST_CASE] 
    FOREIGN KEY ([AUTOMATION_ATTACHMENT_ID]) REFERENCES [TST_ATTACHMENT] ([ATTACHMENT_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_TST_TEST_CASE_STATUS_TST_TEST_CASE] 
    FOREIGN KEY ([TEST_CASE_STATUS_ID]) REFERENCES [TST_TEST_CASE_STATUS] ([TEST_CASE_STATUS_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_TST_TEST_CASE_TYPE_TST_TEST_CASE] 
    FOREIGN KEY ([TEST_CASE_TYPE_ID]) REFERENCES [TST_TEST_CASE_TYPE] ([TEST_CASE_TYPE_ID])
GO


ALTER TABLE [TST_TEST_CASE] ADD CONSTRAINT [FK_TST_TEST_CASE_FOLDER_TST_TEST_CASE] 
    FOREIGN KEY ([TEST_CASE_FOLDER_ID]) REFERENCES [TST_TEST_CASE_FOLDER] ([TEST_CASE_FOLDER_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_USER] ADD CONSTRAINT [FK_TST_GLOBAL_OAUTH_PROVIDERS_TST_USER] 
    FOREIGN KEY ([OAUTH_PROVIDER_ID]) REFERENCES [TST_GLOBAL_OAUTH_PROVIDERS] ([OAUTH_PROVIDER_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_IMPORTANCE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_IMPORTANCE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_INCIDENT_SEVERITY] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_INCIDENT_SEVERITY] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_INCIDENT_PRIORITY] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_INCIDENT_PRIORITY] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_PROJECT] 
    FOREIGN KEY ([PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID])
GO


ALTER TABLE [TST_PROJECT] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_PROJECT] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_CUSTOM_PROPERTY_TYPE_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_TYPE_ID]) REFERENCES [TST_CUSTOM_PROPERTY_TYPE] ([CUSTOM_PROPERTY_TYPE_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_CUSTOM_PROPERTY] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_LIST_TST_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_LIST_ID]) REFERENCES [TST_CUSTOM_PROPERTY_LIST] ([CUSTOM_PROPERTY_LIST_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_CUSTOM_PROPERTY] 
    FOREIGN KEY ([DEPENDENT_CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_CUSTOM_PROPERTY] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_TYPE] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TYPE_TST_CUSTOM_PROPERTY_TYPE] 
    FOREIGN KEY ([ARTIFACT_FIELD_TYPE_ID]) REFERENCES [TST_ARTIFACT_FIELD_TYPE] ([ARTIFACT_FIELD_TYPE_ID])
GO


ALTER TABLE [TST_TEST_CASE_PRIORITY] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_TEST_CASE_PRIORITY] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_ARTIFACT_LINK] ADD CONSTRAINT [FK_TST_SOURCE_ARTIFACT_TYPE_TST_ARTIFACT_LINK] 
    FOREIGN KEY ([SOURCE_ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_ARTIFACT_LINK] ADD CONSTRAINT [FK_TST_DEST_ARTIFACT_TYPE_TST_ARTIFACT_LINK] 
    FOREIGN KEY ([DEST_ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_ARTIFACT_LINK] ADD CONSTRAINT [FK_TST_USER_TST_ARTIFACT_LINK] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_ARTIFACT_LINK] ADD CONSTRAINT [FK_TST_ARTIFACT_LINK_TYPE_TST_ARTIFACT_LINK] 
    FOREIGN KEY ([ARTIFACT_LINK_TYPE_ID]) REFERENCES [TST_ARTIFACT_LINK_TYPE] ([ARTIFACT_LINK_TYPE_ID])
GO


ALTER TABLE [TST_NOTIFICATION_ARTIFACT_USER] ADD CONSTRAINT [FK_TST_NOTIFICATION_ARTIFACT_USER_TYPE_TST_NOTIFICATION_ARTIFACT_USER] 
    FOREIGN KEY ([NOTIFICATION_USER_TYPE_ID]) REFERENCES [TST_NOTIFICATION_ARTIFACT_USER_TYPE] ([PROJECT_ARTIFACT_NOTIFY_TYPE_ID])
GO


ALTER TABLE [TST_NOTIFICATION_ARTIFACT_USER] ADD CONSTRAINT [FK_TST_NOTIFICATION_EVENT_TST_NOTIFICATION_ARTIFACT_USER] 
    FOREIGN KEY ([NOTIFICATION_EVENT_ID]) REFERENCES [TST_NOTIFICATION_EVENT] ([NOTIFICATION_EVENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_NOTIFICATION_PROJECT_ROLE] ADD CONSTRAINT [FK_TST_PROJECT_ROLE_TST_NOTIFICATION_PROJECT_ROLE] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_NOTIFICATION_PROJECT_ROLE] ADD CONSTRAINT [FK_TST_NOTIFICATION_EVENT_TST_NOTIFICATION_PROJECT_ROLE] 
    FOREIGN KEY ([NOTIFICATION_EVENT_ID]) REFERENCES [TST_NOTIFICATION_EVENT] ([NOTIFICATION_EVENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_COLLECTION_ENTRY] ADD CONSTRAINT [FK_TST_USER_COLLECTION_TST_USER_COLLECTION_ENTRY] 
    FOREIGN KEY ([USER_COLLECTION_ID]) REFERENCES [TST_USER_COLLECTION] ([USER_COLLECTION_ID])
GO


ALTER TABLE [TST_USER_COLLECTION_ENTRY] ADD CONSTRAINT [FK_TST_USER_TST_USER_COLLECTION_ENTRY] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_DATA_SYNC_SYSTEM] ADD CONSTRAINT [FK_TST_DATA_SYNC_STATUS_TST_DATA_SYNC_SYSTEM] 
    FOREIGN KEY ([DATA_SYNC_STATUS_ID]) REFERENCES [TST_DATA_SYNC_STATUS] ([DATA_SYNC_STATUS_ID])
GO


ALTER TABLE [TST_INCIDENT_RESOLUTION] ADD CONSTRAINT [FK_TST_INCIDENT_TST_INCIDENT_RESOLUTION] 
    FOREIGN KEY ([INCIDENT_ID]) REFERENCES [TST_INCIDENT] ([INCIDENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_INCIDENT_RESOLUTION] ADD CONSTRAINT [FK_TST_USER_TST_INCIDENT_RESOLUTION] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_RELEASE_TST_TEST_SET] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_SET] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_TEST_SET_STATUS_TST_TEST_SET] 
    FOREIGN KEY ([TEST_SET_STATUS_ID]) REFERENCES [TST_TEST_SET_STATUS] ([TEST_SET_STATUS_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_USER_TST_TEST_SET_CREATOR] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_USER_TST_TEST_SET_OWNER] 
    FOREIGN KEY ([OWNER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_AUTOMATION_HOST_TST_TEST_SET] 
    FOREIGN KEY ([AUTOMATION_HOST_ID]) REFERENCES [TST_AUTOMATION_HOST] ([AUTOMATION_HOST_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_TEST_RUN_TYPE_TST_TEST_SET] 
    FOREIGN KEY ([TEST_RUN_TYPE_ID]) REFERENCES [TST_TEST_RUN_TYPE] ([TEST_RUN_TYPE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_RECURRENCE_TST_TEST_SET] 
    FOREIGN KEY ([RECURRENCE_ID]) REFERENCES [TST_RECURRENCE] ([RECURRENCE_ID])
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_TEST_SET_FOLDER_TST_TEST_SET] 
    FOREIGN KEY ([TEST_SET_FOLDER_ID]) REFERENCES [TST_TEST_SET_FOLDER] ([TEST_SET_FOLDER_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_TEST_SET] ADD CONSTRAINT [FK_TST_TEST_CONFIGURATION_SET_TST_TEST_SET] 
    FOREIGN KEY ([TEST_CONFIGURATION_SET_ID]) REFERENCES [TST_TEST_CONFIGURATION_SET] ([TEST_CONFIGURATION_SET_ID])
GO


ALTER TABLE [TST_TEST_SET_TEST_CASE] ADD CONSTRAINT [FK_TST_TEST_SET_TST_TEST_SET_TEST_CASE] 
    FOREIGN KEY ([TEST_SET_ID]) REFERENCES [TST_TEST_SET] ([TEST_SET_ID])
GO


ALTER TABLE [TST_TEST_SET_TEST_CASE] ADD CONSTRAINT [FK_TST_TEST_CASE_TST_TEST_SET_TEST_CASE] 
    FOREIGN KEY ([TEST_CASE_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET_TEST_CASE] ADD CONSTRAINT [FK_TST_USER_TST_TEST_SET_TEST_CASE] 
    FOREIGN KEY ([OWNER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_SAVED_FILTER] ADD CONSTRAINT [FK_TST_PROJECT_TST_SAVED_FILTER] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_SAVED_FILTER] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_SAVED_FILTER] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_SAVED_FILTER] ADD CONSTRAINT [FK_TST_USER_TST_SAVED_FILTER] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_SAVED_FILTER_ENTRY] ADD CONSTRAINT [FK_TST_SAVED_FILTER_TST_SAVED_FILTER_ENTRY] 
    FOREIGN KEY ([SAVED_FILTER_ID]) REFERENCES [TST_SAVED_FILTER] ([SAVED_FILTER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_RUNS_PENDING] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_RUNS_PENDING] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_RUNS_PENDING] ADD CONSTRAINT [FK_TST_USER_TST_TEST_RUNS_PENDING] 
    FOREIGN KEY ([TESTER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TEST_RUNS_PENDING] ADD CONSTRAINT [FK_TST_TEST_SET_TST_TEST_RUNS_PENDING] 
    FOREIGN KEY ([TEST_SET_ID]) REFERENCES [TST_TEST_SET] ([TEST_SET_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_LIST] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_CUSTOM_PROPERTY_LIST] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_ATTACHMENT_VERSION] ADD CONSTRAINT [FK_TST_ATTACHMENT_TST_ATTACHMENT_VERSION] 
    FOREIGN KEY ([ATTACHMENT_ID]) REFERENCES [TST_ATTACHMENT] ([ATTACHMENT_ID])
GO


ALTER TABLE [TST_ATTACHMENT_VERSION] ADD CONSTRAINT [FK_TST_USER_TST_ATTACHMENT_VERSION] 
    FOREIGN KEY ([AUTHOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_ATTACHMENT_VERSION] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_ATTACHMENT_VERSION] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID])
GO


ALTER TABLE [TST_PROJECT_ATTACHMENT] ADD CONSTRAINT [FK_TST_ATTACHMENT_TST_PROJECT_ATTACHMENT] 
    FOREIGN KEY ([ATTACHMENT_ID]) REFERENCES [TST_ATTACHMENT] ([ATTACHMENT_ID])
GO


ALTER TABLE [TST_PROJECT_ATTACHMENT] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_ATTACHMENT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_PROJECT_ATTACHMENT] ADD CONSTRAINT [FK_TST_DOCUMENT_TYPE_TST_PROJECT_ATTACHMENT] 
    FOREIGN KEY ([DOCUMENT_TYPE_ID]) REFERENCES [TST_DOCUMENT_TYPE] ([DOCUMENT_TYPE_ID])
GO


ALTER TABLE [TST_PROJECT_ATTACHMENT] ADD CONSTRAINT [FK_TST_PROJECT_ATTACHMENT_FOLDER_TST_PROJECT_ATTACHMENT] 
    FOREIGN KEY ([PROJECT_ATTACHMENT_FOLDER_ID]) REFERENCES [TST_PROJECT_ATTACHMENT_FOLDER] ([PROJECT_ATTACHMENT_FOLDER_ID])
GO


ALTER TABLE [TST_DOCUMENT_TYPE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_DOCUMENT_TYPE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_DOCUMENT_TYPE] ADD CONSTRAINT [FK_TST_DOCUMENT_WORKFLOW_TST_DOCUMENT_TYPE] 
    FOREIGN KEY ([DOCUMENT_WORKFLOW_ID]) REFERENCES [TST_DOCUMENT_WORKFLOW] ([DOCUMENT_WORKFLOW_ID])
GO


ALTER TABLE [TST_PROJECT_ATTACHMENT_FOLDER] ADD CONSTRAINT [FK_TST_PROJECT_ATTACHMENT_FOLDER_TST_PROJECT_ATTACHMENT_FOLDER] 
    FOREIGN KEY ([PARENT_PROJECT_ATTACHMENT_FOLDER_ID]) REFERENCES [TST_PROJECT_ATTACHMENT_FOLDER] ([PROJECT_ATTACHMENT_FOLDER_ID])
GO


ALTER TABLE [TST_PROJECT_ATTACHMENT_FOLDER] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_ATTACHMENT_FOLDER] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_DATA_SYNC_PROJECT] ADD CONSTRAINT [FK_TST_DATA_SYNC_SYSTEM_TST_DATA_SYNC_PROJECT] 
    FOREIGN KEY ([DATA_SYNC_SYSTEM_ID]) REFERENCES [TST_DATA_SYNC_SYSTEM] ([DATA_SYNC_SYSTEM_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DATA_SYNC_PROJECT] ADD CONSTRAINT [FK_TST_PROJECT_TST_DATA_SYNC_PROJECT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DATA_SYNC_USER_MAPPING] ADD CONSTRAINT [FK_TST_DATA_SYNC_SYSTEM_TST_DATA_SYNC_USER_MAPPING] 
    FOREIGN KEY ([DATA_SYNC_SYSTEM_ID]) REFERENCES [TST_DATA_SYNC_SYSTEM] ([DATA_SYNC_SYSTEM_ID])
GO


ALTER TABLE [TST_DATA_SYNC_USER_MAPPING] ADD CONSTRAINT [FK_TST_USER_TST_DATA_SYNC_USER_MAPPING] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING] ADD CONSTRAINT [FK_TST_DATA_SYNC_PROJECT_TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING] 
    FOREIGN KEY ([DATA_SYNC_SYSTEM_ID], [PROJECT_ID]) REFERENCES [TST_DATA_SYNC_PROJECT] ([DATA_SYNC_SYSTEM_ID],[PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING] ADD CONSTRAINT [FK_TST_DATA_SYNC_PROJECT_TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING] 
    FOREIGN KEY ([DATA_SYNC_SYSTEM_ID], [PROJECT_ID]) REFERENCES [TST_DATA_SYNC_PROJECT] ([DATA_SYNC_SYSTEM_ID],[PROJECT_ID])
GO


ALTER TABLE [TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_VALUE_TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING] 
    FOREIGN KEY ([CUSTOM_PROPERTY_VALUE_ID]) REFERENCES [TST_CUSTOM_PROPERTY_VALUE] ([CUSTOM_PROPERTY_VALUE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING] ADD CONSTRAINT [FK_TST_DATA_SYNC_PROJECT_TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING] 
    FOREIGN KEY ([DATA_SYNC_SYSTEM_ID], [PROJECT_ID]) REFERENCES [TST_DATA_SYNC_PROJECT] ([DATA_SYNC_SYSTEM_ID],[PROJECT_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_PROJECT_GROUP] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_PROJECT_GROUP] ADD CONSTRAINT [FK_TST_PORTFOLIO_TST_PROJECT_GROUP] 
    FOREIGN KEY ([PORTFOLIO_ID]) REFERENCES [TST_PORTFOLIO] ([PORTFOLIO_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_USER] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_PROJECT_GROUP_USER] 
    FOREIGN KEY ([PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_USER] ADD CONSTRAINT [FK_TST_USER_TST_PROJECT_GROUP_USER] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_USER] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_ROLE_TST_PROJECT_GROUP_USER] 
    FOREIGN KEY ([PROJECT_GROUP_ROLE_ID]) REFERENCES [TST_PROJECT_GROUP_ROLE] ([PROJECT_GROUP_ROLE_ID])
GO


ALTER TABLE [TST_DASHBOARD_GLOBAL_PERSONALIZATION] ADD CONSTRAINT [FK_TST_DASHBOARD_TST_DASHBOARD_GLOBAL_PERSONALIZATION] 
    FOREIGN KEY ([DASHBOARD_ID]) REFERENCES [TST_DASHBOARD] ([DASHBOARD_ID])
GO


ALTER TABLE [TST_DASHBOARD_USER_PERSONALIZATION] ADD CONSTRAINT [FK_TST_DASHBOARD_TST_DASHBOARD_USER_PERSONALIZATION] 
    FOREIGN KEY ([DASHBOARD_ID]) REFERENCES [TST_DASHBOARD] ([DASHBOARD_ID])
GO


ALTER TABLE [TST_DASHBOARD_USER_PERSONALIZATION] ADD CONSTRAINT [FK_TST_USER_TST_DASHBOARD_USER_PERSONALIZATION] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_REPORT] ADD CONSTRAINT [FK_TST_REPORT_CATEGORY_TST_REPORT] 
    FOREIGN KEY ([REPORT_CATEGORY_ID]) REFERENCES [TST_REPORT_CATEGORY] ([REPORT_CATEGORY_ID])
GO


ALTER TABLE [TST_REPORT_SECTION] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_REPORT_SECTION] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_REPORT_AVAILABLE_FORMAT] ADD CONSTRAINT [FK_TST_REPORT_TST_REPORT_AVAILABLE_FORMAT] 
    FOREIGN KEY ([REPORT_ID]) REFERENCES [TST_REPORT] ([REPORT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REPORT_AVAILABLE_FORMAT] ADD CONSTRAINT [FK_TST_REPORT_FORMAT_TST_REPORT_AVAILABLE_FORMAT] 
    FOREIGN KEY ([REPORT_FORMAT_ID]) REFERENCES [TST_REPORT_FORMAT] ([REPORT_FORMAT_ID])
GO


ALTER TABLE [TST_REPORT_AVAILABLE_SECTION] ADD CONSTRAINT [FK_TST_REPORT_TST_REPORT_AVAILABLE_SECTION] 
    FOREIGN KEY ([REPORT_ID]) REFERENCES [TST_REPORT] ([REPORT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REPORT_AVAILABLE_SECTION] ADD CONSTRAINT [FK_TST_REPORT_SECTION_TST_REPORT_AVAILABLE_SECTION] 
    FOREIGN KEY ([REPORT_SECTION_ID]) REFERENCES [TST_REPORT_SECTION] ([REPORT_SECTION_ID])
GO


ALTER TABLE [TST_REPORT_CATEGORY] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_REPORT_CATEGORY] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_REPORT_CATEGORY] ADD CONSTRAINT [FK_TST_WORKSPACE_TYPE_TST_REPORT_CATEGORY] 
    FOREIGN KEY ([WORKSPACE_TYPE_ID]) REFERENCES [TST_WORKSPACE_TYPE] ([WORKSPACE_TYPE_ID])
GO


ALTER TABLE [TST_REPORT_SECTION_ELEMENT] ADD CONSTRAINT [FK_TST_REPORT_SECTION_TST_REPORT_SECTION_ELEMENT] 
    FOREIGN KEY ([REPORT_SECTION_ID]) REFERENCES [TST_REPORT_SECTION] ([REPORT_SECTION_ID])
GO


ALTER TABLE [TST_REPORT_SECTION_ELEMENT] ADD CONSTRAINT [FK_TST_REPORT_ELEMENT_TST_REPORT_SECTION_ELEMENT] 
    FOREIGN KEY ([REPORT_ELEMENT_ID]) REFERENCES [TST_REPORT_ELEMENT] ([REPORT_ELEMENT_ID])
GO


ALTER TABLE [TST_REPORT_ELEMENT] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_REPORT_ELEMENT] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_REPORT_SAVED] ADD CONSTRAINT [FK_TST_REPORT_TST_REPORT_SAVED] 
    FOREIGN KEY ([REPORT_ID]) REFERENCES [TST_REPORT] ([REPORT_ID])
GO


ALTER TABLE [TST_REPORT_SAVED] ADD CONSTRAINT [FK_TST_USER_TST_REPORT_SAVED] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_REPORT_SAVED] ADD CONSTRAINT [FK_TST_PROJECT_TST_REPORT_SAVED] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_REPORT_SAVED] ADD CONSTRAINT [FK_TST_REPORT_FORMAT_TST_REPORT_SAVED] 
    FOREIGN KEY ([REPORT_FORMAT_ID]) REFERENCES [TST_REPORT_FORMAT] ([REPORT_FORMAT_ID])
GO


ALTER TABLE [TST_VERSION_CONTROL_PROJECT] ADD CONSTRAINT [FK_TST_VERSION_CONTROL_SYSTEM_TST_VERSION_CONTROL_PROJECT] 
    FOREIGN KEY ([VERSION_CONTROL_SYSTEM_ID]) REFERENCES [TST_VERSION_CONTROL_SYSTEM] ([VERSION_CONTROL_SYSTEM_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_VERSION_CONTROL_PROJECT] ADD CONSTRAINT [FK_TST_PROJECT_TST_VERSION_CONTROL_PROJECT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET_TEST_CASE_PARAMETER] ADD CONSTRAINT [FK_TST_TEST_SET_TEST_CASE_TST_TEST_SET_TEST_CASE_PARAMETER] 
    FOREIGN KEY ([TEST_SET_TEST_CASE_ID]) REFERENCES [TST_TEST_SET_TEST_CASE] ([TEST_SET_TEST_CASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET_TEST_CASE_PARAMETER] ADD CONSTRAINT [FK_TST_TEST_CASE_PARAMETER_TST_TEST_SET_TEST_CASE_PARAMETER] 
    FOREIGN KEY ([TEST_CASE_PARAMETER_ID]) REFERENCES [TST_TEST_CASE_PARAMETER] ([TEST_CASE_PARAMETER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_NOTIFICATION_EVENT] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_NOTIFICATION_EVENT] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_NOTIFICATION_EVENT] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_NOTIFICATION_EVENT] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_NOTIFICATION_EVENT_FIELD] ADD CONSTRAINT [FK_TST_NOTIFICATION_EVENT_TST_NOTIFICATION_EVENT_FIELD] 
    FOREIGN KEY ([NOTIFICATION_EVENT_ID]) REFERENCES [TST_NOTIFICATION_EVENT] ([NOTIFICATION_EVENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_NOTIFICATION_EVENT_FIELD] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_NOTIFICATION_EVENT_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_NOTIFICATION_ARTIFACT_TEMPLATE] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_NOTIFICATION_ARTIFACT_TEMPLATE] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_NOTIFICATION_ARTIFACT_TEMPLATE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_NOTIFICATION_ARTIFACT_TEMPLATE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_AUTOMATION_HOST] ADD CONSTRAINT [FK_TST_PROJECT_TST_AUTOMATION_HOST] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_NOTIFICATION_USER_SUBSCRIPTION] ADD CONSTRAINT [FK_TST_USER_TST_NOTIFICATION_USER_SUBSCRIPTION] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_NOTIFICATION_USER_SUBSCRIPTION] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_NOTIFICATION_USER_SUBSCRIPTION] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_DISCUSSION] ADD CONSTRAINT [FK_TST_REQUIREMENT_TST_REQUIREMENT_DISCUSSION] 
    FOREIGN KEY ([ARTIFACT_ID]) REFERENCES [TST_REQUIREMENT] ([REQUIREMENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_DISCUSSION] ADD CONSTRAINT [FK_TST_USER_TST_REQUIREMENT_DISCUSSION] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_RELEASE_DISCUSSION] ADD CONSTRAINT [FK_TST_RELEASE_TST_RELEASE_DISCUSSION] 
    FOREIGN KEY ([ARTIFACT_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_DISCUSSION] ADD CONSTRAINT [FK_TST_USER_TST_RELEASE_DISCUSSION] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TEST_CASE_DISCUSSION] ADD CONSTRAINT [FK_TST_TEST_CASE_TST_TEST_CASE_DISCUSSION] 
    FOREIGN KEY ([ARTIFACT_ID]) REFERENCES [TST_TEST_CASE] ([TEST_CASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_CASE_DISCUSSION] ADD CONSTRAINT [FK_TST_USER_TST_TEST_CASE_DISCUSSION] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TEST_SET_DISCUSSION] ADD CONSTRAINT [FK_TST_TEST_SET_TST_TEST_SET_DISCUSSION] 
    FOREIGN KEY ([ARTIFACT_ID]) REFERENCES [TST_TEST_SET] ([TEST_SET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET_DISCUSSION] ADD CONSTRAINT [FK_TST_USER_TST_TEST_SET_DISCUSSION] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TASK_DISCUSSION] ADD CONSTRAINT [FK_TST_TASK_TST_TASK_DISCUSSION] 
    FOREIGN KEY ([ARTIFACT_ID]) REFERENCES [TST_TASK] ([TASK_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK_DISCUSSION] ADD CONSTRAINT [FK_TST_USER_TST_TASK_DISCUSSION] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_HISTORY_CHANGESET] ADD CONSTRAINT [FK_TST_USER_TST_HISTORY_CHANGESET] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE SET DEFAULT
GO


ALTER TABLE [TST_HISTORY_CHANGESET] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_HISTORY_CHANGESET] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID]) ON DELETE SET DEFAULT
GO


ALTER TABLE [TST_HISTORY_CHANGESET] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TYPE_TST_HISTORY_CHANGESET] 
    FOREIGN KEY ([CHANGETYPE_ID]) REFERENCES [TST_HISTORY_CHANGESET_TYPE] ([CHANGETYPE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_HISTORY_CHANGESET] ADD CONSTRAINT [FK_TST_PROJECT_TST_HISTORY_CHANGESET] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_HISTORY_CHANGESET] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_CHANGESET] 
    FOREIGN KEY ([REVERT_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID])
GO


ALTER TABLE [TST_GRAPH] ADD CONSTRAINT [FK_TST_GRAPH_TYPE_TST_GRAPH] 
    FOREIGN KEY ([GRAPH_TYPE_ID]) REFERENCES [TST_GRAPH_TYPE] ([GRAPH_TYPE_ID])
GO


ALTER TABLE [TST_GRAPH] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_GRAPH] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_PROJECT_TAG_FREQUENCY] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_TAG_FREQUENCY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_ARTIFACT_SOURCE_CODE_FILE] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_ARTIFACT_SOURCE_CODE_FILE] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_BUILD] ADD CONSTRAINT [FK_TST_RELEASE_TST_BUILD] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_BUILD] ADD CONSTRAINT [FK_TST_BUILD_STATUS_TST_BUILD] 
    FOREIGN KEY ([BUILD_STATUS_ID]) REFERENCES [TST_BUILD_STATUS] ([BUILD_STATUS_ID])
GO


ALTER TABLE [TST_BUILD] ADD CONSTRAINT [FK_TST_PROJECT_TST_BUILD] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_BUILD_SOURCE_CODE] ADD CONSTRAINT [FK_TST_BUILD_TST_BUILD_SOURCE_CODE] 
    FOREIGN KEY ([BUILD_ID]) REFERENCES [TST_BUILD] ([BUILD_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_PROFILE] ADD CONSTRAINT [FK_TST_USER_TST_USER_PROFILE] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_PROFILE] ADD CONSTRAINT [FK_TST_PROJECT_TST_USER_PROFILE] 
    FOREIGN KEY ([LAST_OPENED_PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_USER_PROFILE] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_USER_PROFILE] 
    FOREIGN KEY ([LAST_OPENED_PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_USER_PROFILE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_USER_PROFILE] 
    FOREIGN KEY ([LAST_OPENED_PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_EVENT] ADD CONSTRAINT [FK_TST_EVENT_TYPE_TST_EVENT] 
    FOREIGN KEY ([EVENT_TYPE_ID]) REFERENCES [TST_EVENT_TYPE] ([EVENT_TYPE_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_OPTION_VALUE] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_OPTION_TST_CUSTOM_PROPERTY_OPTION_VALUE] 
    FOREIGN KEY ([CUSTOM_PROPERTY_OPTION_ID]) REFERENCES [TST_CUSTOM_PROPERTY_OPTION] ([CUSTOM_PROPERTY_OPTION_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_OPTION_VALUE] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_CUSTOM_PROPERTY_OPTION_VALUE] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PLACEHOLDER] ADD CONSTRAINT [FK_TST_PROJECT_TST_PLACEHOLDER] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_REPORT_CUSTOM_SECTION] ADD CONSTRAINT [FK_TST_REPORT_TST_REPORT_CUSTOM_SECTION] 
    FOREIGN KEY ([REPORT_ID]) REFERENCES [TST_REPORT] ([REPORT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REPORT_GENERATED] ADD CONSTRAINT [FK_TST_REPORT_TST_REPORT_GENERATED] 
    FOREIGN KEY ([REPORT_ID]) REFERENCES [TST_REPORT] ([REPORT_ID])
GO


ALTER TABLE [TST_REPORT_GENERATED] ADD CONSTRAINT [FK_TST_REPORT_FORMAT_TST_REPORT_GENERATED] 
    FOREIGN KEY ([REPORT_FORMAT_ID]) REFERENCES [TST_REPORT_FORMAT] ([REPORT_FORMAT_ID])
GO


ALTER TABLE [TST_REPORT_GENERATED] ADD CONSTRAINT [FK_TST_USER_TST_REPORT_GENERATED] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_REQUIREMENT_TYPE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_REQUIREMENT_TYPE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_TYPE] ADD CONSTRAINT [FK_TST_REQUIREMENT_WORKFLOW_TST_REQUIREMENT_TYPE] 
    FOREIGN KEY ([REQUIREMENT_WORKFLOW_ID]) REFERENCES [TST_REQUIREMENT_WORKFLOW] ([REQUIREMENT_WORKFLOW_ID])
GO


ALTER TABLE [TST_REQUIREMENT_STEP] ADD CONSTRAINT [FK_TST_REQUIREMENT_TST_REQUIREMENT_STEP] 
    FOREIGN KEY ([REQUIREMENT_ID]) REFERENCES [TST_REQUIREMENT] ([REQUIREMENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK_FOLDER] ADD CONSTRAINT [FK_TST_TASK_FOLDER_TST_TASK_FOLDER] 
    FOREIGN KEY ([PARENT_TASK_FOLDER_ID]) REFERENCES [TST_TASK_FOLDER] ([TASK_FOLDER_ID])
GO


ALTER TABLE [TST_TASK_FOLDER] ADD CONSTRAINT [FK_TST_PROJECT_TST_TASK_FOLDER] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_COMPONENT] ADD CONSTRAINT [FK_TST_PROJECT_TST_COMPONENT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_REQUIREMENT_WORKFLOW] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_REQUIREMENT_WORKFLOW_TST_REQUIREMENT_WORKFLOW_FIELD] 
    FOREIGN KEY ([REQUIREMENT_WORKFLOW_ID]) REFERENCES [TST_REQUIREMENT_WORKFLOW] ([REQUIREMENT_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_REQUIREMENT_WORKFLOW_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_REQUIREMENT_WORKFLOW_FIELD] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_REQUIREMENT_STATUS_TST_REQUIREMENT_WORKFLOW_FIELD] 
    FOREIGN KEY ([REQUIREMENT_STATUS_ID]) REFERENCES [TST_REQUIREMENT_STATUS] ([REQUIREMENT_STATUS_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_REQUIREMENT_WORKFLOW_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([REQUIREMENT_WORKFLOW_ID]) REFERENCES [TST_REQUIREMENT_WORKFLOW] ([REQUIREMENT_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_REQUIREMENT_STATUS_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([REQUIREMENT_STATUS_ID]) REFERENCES [TST_REQUIREMENT_STATUS] ([REQUIREMENT_STATUS_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_REQUIREMENT_WORKFLOW_TST_REQUIREMENT_WORKFLOW_TRANSITION] 
    FOREIGN KEY ([REQUIREMENT_WORKFLOW_ID]) REFERENCES [TST_REQUIREMENT_WORKFLOW] ([REQUIREMENT_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_REQUIREMENT_STATUS_TST_REQUIREMENT_WORKFLOW_TRANSITION_INPUT] 
    FOREIGN KEY ([INPUT_REQUIREMENT_STATUS_ID]) REFERENCES [TST_REQUIREMENT_STATUS] ([REQUIREMENT_STATUS_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_REQUIREMENT_STATUS_TST_REQUIREMENT_WORKFLOW_TRANSITION_OUTPUT] 
    FOREIGN KEY ([OUTPUT_REQUIREMENT_STATUS_ID]) REFERENCES [TST_REQUIREMENT_STATUS] ([REQUIREMENT_STATUS_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_REQUIREMENT_WORKFLOW_TRANSITION_TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ID]) REFERENCES [TST_REQUIREMENT_WORKFLOW_TRANSITION] ([WORKFLOW_TRANSITION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_PROJECT_ROLE_TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID])
GO


ALTER TABLE [TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_WORKFLOW_TRANSITION_ROLE_TYPE_TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ROLE_TYPE_ID]) REFERENCES [TST_WORKFLOW_TRANSITION_ROLE_TYPE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


ALTER TABLE [TST_TASK_TYPE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_TASK_TYPE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK_TYPE] ADD CONSTRAINT [FK_TST_TASK_WORKFLOW_TST_TASK_TYPE] 
    FOREIGN KEY ([TASK_WORKFLOW_ID]) REFERENCES [TST_TASK_WORKFLOW] ([TASK_WORKFLOW_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_TASK_WORKFLOW] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_TASK_WORKFLOW_TST_TASK_WORKFLOW_TRANSITION] 
    FOREIGN KEY ([TASK_WORKFLOW_ID]) REFERENCES [TST_TASK_WORKFLOW] ([TASK_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_TASK_STATUS_TST_TASK_WORKFLOW_TRANSITION_INPUT] 
    FOREIGN KEY ([INPUT_TASK_STATUS_ID]) REFERENCES [TST_TASK_STATUS] ([TASK_STATUS_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_TASK_STATUS_TST_TASK_WORKFLOW_TRANSITION_OUTPUT] 
    FOREIGN KEY ([OUTPUT_TASK_STATUS_ID]) REFERENCES [TST_TASK_STATUS] ([TASK_STATUS_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_TASK_WORKFLOW_TRANSITION_TST_TASK_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ID]) REFERENCES [TST_TASK_WORKFLOW_TRANSITION] ([WORKFLOW_TRANSITION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_WORKFLOW_TRANSITION_ROLE_TYPE_TST_TASK_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ROLE_TYPE_ID]) REFERENCES [TST_WORKFLOW_TRANSITION_ROLE_TYPE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_PROJECT_ROLE_TST_TASK_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_TASK_WORKFLOW_TST_TASK_WORKFLOW_FIELD] 
    FOREIGN KEY ([TASK_WORKFLOW_ID]) REFERENCES [TST_TASK_WORKFLOW] ([TASK_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_TASK_STATUS_TST_TASK_WORKFLOW_FIELD] 
    FOREIGN KEY ([TASK_STATUS_ID]) REFERENCES [TST_TASK_STATUS] ([TASK_STATUS_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_TASK_WORKFLOW_FIELD] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_TASK_WORKFLOW_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_TASK_WORKFLOW_TST_TASK_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([TASK_WORKFLOW_ID]) REFERENCES [TST_TASK_WORKFLOW] ([TASK_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_TASK_STATUS_TST_TASK_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([TASK_STATUS_ID]) REFERENCES [TST_TASK_STATUS] ([TASK_STATUS_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_TASK_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_TASK_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_TASK_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_MESSAGE] ADD CONSTRAINT [FK_TST_USER_TST_MESSAGE_SENDER] 
    FOREIGN KEY ([SENDER_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_MESSAGE] ADD CONSTRAINT [FK_TST_USER_TST_MESSAGE_RECIPIENT] 
    FOREIGN KEY ([RECIPIENT_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_USER_CONTACT] ADD CONSTRAINT [FK_TST_USER_TST_USER_CONTACT_CONTACT] 
    FOREIGN KEY ([CONTACT_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_USER_CONTACT] ADD CONSTRAINT [FK_TST_USER_TST_USER_CONTACT_CREATOR] 
    FOREIGN KEY ([CREATOR_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TVAULT_PROJECT] ADD CONSTRAINT [FK_TST_PROJECT_TST_TVAULT_PROJECT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_TVAULT_PROJECT] ADD CONSTRAINT [FK_TST_TVAULT_TYPE_TST_TVAULT_PROJECT] 
    FOREIGN KEY ([TVAULT_TYPE_ID]) REFERENCES [TST_TVAULT_TYPE] ([TVAULT_TYPE_ID])
GO


ALTER TABLE [TST_TVAULT_USER] ADD CONSTRAINT [FK_TST_USER_TST_TVAULT_USER] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TVAULT_PROJECT_USER] ADD CONSTRAINT [FK_TST_TVAULT_USER_TST_TVAULT_PROJECT_USER] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_TVAULT_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TVAULT_PROJECT_USER] ADD CONSTRAINT [FK_TST_TVAULT_PROJECT_TST_TVAULT_PROJECT_USER] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_TVAULT_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_ARTIFACT_SOURCE_CODE_REVISION] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_ARTIFACT_SOURCE_CODE_REVISION] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RELEASE_WORKFLOW] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_RELEASE_WORKFLOW_TST_RELEASE_WORKFLOW_TRANSITION] 
    FOREIGN KEY ([RELEASE_WORKFLOW_ID]) REFERENCES [TST_RELEASE_WORKFLOW] ([RELEASE_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_RELEASE_STATUS_TST_RELEASE_WORKFLOW_TRANSITION_INPUT] 
    FOREIGN KEY ([INPUT_RELEASE_STATUS_ID]) REFERENCES [TST_RELEASE_STATUS] ([RELEASE_STATUS_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_RELEASE_STATUS_TST_RELEASE_WORKFLOW_TRANSITION_OUTPUT] 
    FOREIGN KEY ([OUTPUT_RELEASE_STATUS_ID]) REFERENCES [TST_RELEASE_STATUS] ([RELEASE_STATUS_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_RELEASE_WORKFLOW_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([RELEASE_WORKFLOW_ID]) REFERENCES [TST_RELEASE_WORKFLOW] ([RELEASE_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_RELEASE_STATUS_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([RELEASE_STATUS_ID]) REFERENCES [TST_RELEASE_STATUS] ([RELEASE_STATUS_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_RELEASE_WORKFLOW_TST_RELEASE_WORKFLOW_FIELD] 
    FOREIGN KEY ([RELEASE_WORKFLOW_ID]) REFERENCES [TST_RELEASE_WORKFLOW] ([RELEASE_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_RELEASE_STATUS_TST_RELEASE_WORKFLOW_FIELD] 
    FOREIGN KEY ([RELEASE_STATUS_ID]) REFERENCES [TST_RELEASE_STATUS] ([RELEASE_STATUS_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_RELEASE_WORKFLOW_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_RELEASE_WORKFLOW_FIELD] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_RELEASE_WORKFLOW_TRANSITION_TST_RELEASE_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ID]) REFERENCES [TST_RELEASE_WORKFLOW_TRANSITION] ([WORKFLOW_TRANSITION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_PROJECT_ROLE_TST_RELEASE_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID])
GO


ALTER TABLE [TST_RELEASE_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_WORKFLOW_TRANSITION_ROLE_TYPE_TST_RELEASE_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ROLE_TYPE_ID]) REFERENCES [TST_WORKFLOW_TRANSITION_ROLE_TYPE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_TEST_CASE_WORKFLOW] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_TEST_CASE_WORKFLOW_TST_TEST_CASE_WORKFLOW_TRANSITION] 
    FOREIGN KEY ([TEST_CASE_WORKFLOW_ID]) REFERENCES [TST_TEST_CASE_WORKFLOW] ([TEST_CASE_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_TEST_CASE_STATUS_TST_TEST_CASE_WORKFLOW_TRANSITION_INPUT] 
    FOREIGN KEY ([INPUT_TEST_CASE_STATUS_ID]) REFERENCES [TST_TEST_CASE_STATUS] ([TEST_CASE_STATUS_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_TEST_CASE_STATUS_TST_TEST_CASE_WORKFLOW_TRANSITION_OUTPUT] 
    FOREIGN KEY ([OUTPUT_TEST_CASE_STATUS_ID]) REFERENCES [TST_TEST_CASE_STATUS] ([TEST_CASE_STATUS_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_TEST_CASE_WORKFLOW_TST_TEST_CASE_WORKFLOW_FIELD] 
    FOREIGN KEY ([TEST_CASE_WORKFLOW_ID]) REFERENCES [TST_TEST_CASE_WORKFLOW] ([TEST_CASE_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_TEST_CASE_STATUS_TST_TEST_CASE_WORKFLOW_FIELD] 
    FOREIGN KEY ([TEST_CASE_STATUS_ID]) REFERENCES [TST_TEST_CASE_STATUS] ([TEST_CASE_STATUS_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_TEST_CASE_WORKFLOW_FIELD] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_TEST_CASE_WORKFLOW_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_TEST_CASE_WORKFLOW_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([TEST_CASE_WORKFLOW_ID]) REFERENCES [TST_TEST_CASE_WORKFLOW] ([TEST_CASE_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_TEST_CASE_STATUS_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([TEST_CASE_STATUS_ID]) REFERENCES [TST_TEST_CASE_STATUS] ([TEST_CASE_STATUS_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_TEST_CASE_WORKFLOW_TRANSITION_TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ID]) REFERENCES [TST_TEST_CASE_WORKFLOW_TRANSITION] ([WORKFLOW_TRANSITION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_PROJECT_ROLE_TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID])
GO


ALTER TABLE [TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_WORKFLOW_TRANSITION_ROLE_TYPE_TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ROLE_TYPE_ID]) REFERENCES [TST_WORKFLOW_TRANSITION_ROLE_TYPE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


ALTER TABLE [TST_TEST_CASE_TYPE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_TEST_CASE_TYPE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_CASE_TYPE] ADD CONSTRAINT [FK_TST_TEST_CASE_WORKFLOW_TST_TEST_CASE_TYPE] 
    FOREIGN KEY ([TEST_CASE_WORKFLOW_ID]) REFERENCES [TST_TEST_CASE_WORKFLOW] ([TEST_CASE_WORKFLOW_ID])
GO


ALTER TABLE [TST_TEST_CASE_FOLDER] ADD CONSTRAINT [FK_TST_TEST_CASE_FOLDER_TST_TEST_CASE_FOLDER] 
    FOREIGN KEY ([PARENT_TEST_CASE_FOLDER_ID]) REFERENCES [TST_TEST_CASE_FOLDER] ([TEST_CASE_FOLDER_ID])
GO


ALTER TABLE [TST_TEST_CASE_FOLDER] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_CASE_FOLDER] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_REQUIREMENT_TEST_STEP] ADD CONSTRAINT [FK_TST_REQUIREMENT_TST_REQUIREMENT_TEST_STEP] 
    FOREIGN KEY ([REQUIREMENT_ID]) REFERENCES [TST_REQUIREMENT] ([REQUIREMENT_ID])
GO


ALTER TABLE [TST_REQUIREMENT_TEST_STEP] ADD CONSTRAINT [FK_TST_TEST_STEP_TST_REQUIREMENT_TEST_STEP] 
    FOREIGN KEY ([TEST_STEP_ID]) REFERENCES [TST_TEST_STEP] ([TEST_STEP_ID])
GO


ALTER TABLE [TST_RELEASE_TEST_CASE_FOLDER] ADD CONSTRAINT [FK_TST_RELEASE_TST_RELEASE_TEST_CASE_FOLDER] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_TEST_CASE_FOLDER] ADD CONSTRAINT [FK_TST_TEST_CASE_FOLDER_TST_RELEASE_TEST_CASE_FOLDER] 
    FOREIGN KEY ([TEST_CASE_FOLDER_ID]) REFERENCES [TST_TEST_CASE_FOLDER] ([TEST_CASE_FOLDER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_RUN_STEP_INCIDENT] ADD CONSTRAINT [FK_TST_INCIDENT_TST_TEST_RUN_STEP_INCIDENT] 
    FOREIGN KEY ([INCIDENT_ID]) REFERENCES [TST_INCIDENT] ([INCIDENT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_RUN_STEP_INCIDENT] ADD CONSTRAINT [FK_TST_TEST_RUN_STEP_TST_TEST_RUN_STEP_INCIDENT] 
    FOREIGN KEY ([TEST_RUN_STEP_ID]) REFERENCES [TST_TEST_RUN_STEP] ([TEST_RUN_STEP_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET_FOLDER] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_SET_FOLDER] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_TEST_SET_FOLDER] ADD CONSTRAINT [FK_TST_TEST_SET_FOLDER_TST_TEST_SET_FOLDER] 
    FOREIGN KEY ([PARENT_TEST_SET_FOLDER_ID]) REFERENCES [TST_TEST_SET_FOLDER] ([TEST_SET_FOLDER_ID])
GO


ALTER TABLE [TST_TEST_SET_PARAMETER] ADD CONSTRAINT [FK_TST_TEST_CASE_PARAMETER_TST_TEST_SET_PARAMETER] 
    FOREIGN KEY ([TEST_CASE_PARAMETER_ID]) REFERENCES [TST_TEST_CASE_PARAMETER] ([TEST_CASE_PARAMETER_ID])
GO


ALTER TABLE [TST_TEST_SET_PARAMETER] ADD CONSTRAINT [FK_TST_TEST_SET_TST_TEST_SET_PARAMETER] 
    FOREIGN KEY ([TEST_SET_ID]) REFERENCES [TST_TEST_SET] ([TEST_SET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_TEST_SET] ADD CONSTRAINT [FK_TST_RELEASE_TST_RELEASE_TEST_SET] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID])
GO


ALTER TABLE [TST_RELEASE_TEST_SET] ADD CONSTRAINT [FK_TST_TEST_SET_TST_RELEASE_TEST_SET] 
    FOREIGN KEY ([TEST_SET_ID]) REFERENCES [TST_TEST_SET] ([TEST_SET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_TEST_SET_FOLDER] ADD CONSTRAINT [FK_TST_RELEASE_TST_RELEASE_TEST_SET_FOLDER] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID])
GO


ALTER TABLE [TST_RELEASE_TEST_SET_FOLDER] ADD CONSTRAINT [FK_TST_TEST_SET_FOLDER_TST_RELEASE_TEST_SET_FOLDER] 
    FOREIGN KEY ([TEST_SET_FOLDER_ID]) REFERENCES [TST_TEST_SET_FOLDER] ([TEST_SET_FOLDER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_ARTIFACT_SHARING] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_ARTIFACT_SHARING_SOURCE] 
    FOREIGN KEY ([SOURCE_PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_PROJECT_ARTIFACT_SHARING] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_ARTIFACT_SHARING_DEST] 
    FOREIGN KEY ([DEST_PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_PROJECT_ARTIFACT_SHARING] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_PROJECT_ARTIFACT_SHARING] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_GRAPH_CUSTOM] ADD CONSTRAINT [FK_TST_GRAPH_TYPE_TST_GRAPH_CUSTOM] 
    FOREIGN KEY ([GRAPH_TYPE_ID]) REFERENCES [TST_GRAPH_TYPE] ([GRAPH_TYPE_ID])
GO


ALTER TABLE [TST_PROJECT_SETTING_VALUE] ADD CONSTRAINT [FK_TST_PROJECT_SETTING_TST_PROJECT_SETTING_VALUE] 
    FOREIGN KEY ([PROJECT_SETTING_ID]) REFERENCES [TST_PROJECT_SETTING] ([PROJECT_SETTING_ID])
GO


ALTER TABLE [TST_PROJECT_SETTING_VALUE] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_SETTING_VALUE] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_DASHBOARD_CUSTOM] ADD CONSTRAINT [FK_TST_DASHBOARD_CUSTOM_TYPE_TST_DASHBOARD_CUSTOM] 
    FOREIGN KEY ([DASHBOARD_CUSTOM_TYPE_ID]) REFERENCES [TST_DASHBOARD_CUSTOM_TYPE] ([DASHBOARD_CUSTOM_TYPE_ID])
GO


ALTER TABLE [TST_DASHBOARD_CUSTOM_PERMISSION] ADD CONSTRAINT [FK_TST_DASHBOARD_CUSTOM_TST_DASHBOARD_CUSTOM_PERMISSION] 
    FOREIGN KEY ([DASHBOARD_CUSTOM_ID]) REFERENCES [TST_DASHBOARD_CUSTOM] ([DASHBOARD_CUSTOM_ID])
GO


ALTER TABLE [TST_DASHBOARD_CUSTOM_PERMISSION] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_DASHBOARD_CUSTOM_PERMISSION] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_DASHBOARD_CUSTOM_PERMISSION] ADD CONSTRAINT [FK_TST_PERMISSION_TST_DASHBOARD_CUSTOM_PERMISSION] 
    FOREIGN KEY ([PERMISSION_ID]) REFERENCES [TST_PERMISSION] ([PERMISSION_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_DEPENDENCY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_VALUE_TST_CUSTOM_PROPERTY_DEPENDENCY_SOURCE] 
    FOREIGN KEY ([SOURCE_CUSTOM_PROPERTY_VALUE_ID]) REFERENCES [TST_CUSTOM_PROPERTY_VALUE] ([CUSTOM_PROPERTY_VALUE_ID])
GO


ALTER TABLE [TST_CUSTOM_PROPERTY_DEPENDENCY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_VALUE_TST_CUSTOM_PROPERTY_DEPENDENCY_DEST] 
    FOREIGN KEY ([DEST_CUSTOM_PROPERTY_VALUE_ID]) REFERENCES [TST_CUSTOM_PROPERTY_VALUE] ([CUSTOM_PROPERTY_VALUE_ID])
GO


ALTER TABLE [TST_TEST_CONFIGURATION] ADD CONSTRAINT [FK_TST_TEST_CONFIGURATION_SET_TST_TEST_CONFIGURATION] 
    FOREIGN KEY ([TEST_CONFIGURATION_SET_ID]) REFERENCES [TST_TEST_CONFIGURATION_SET] ([TEST_CONFIGURATION_SET_ID])
GO


ALTER TABLE [TST_TEST_CONFIGURATION_SET] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_CONFIGURATION_SET] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_TEST_CONFIGURATION_SET_PARAMETER] ADD CONSTRAINT [FK_TST_TEST_CONFIGURATION_SET_TST_TEST_CONFIGURATION_SET_PARAMETER] 
    FOREIGN KEY ([TEST_CONFIGURATION_SET_ID]) REFERENCES [TST_TEST_CONFIGURATION_SET] ([TEST_CONFIGURATION_SET_ID])
GO


ALTER TABLE [TST_TEST_CONFIGURATION_SET_PARAMETER] ADD CONSTRAINT [FK_TST_TEST_CASE_PARAMETER_TST_TEST_CONFIGURATION_SET_PARAMETER] 
    FOREIGN KEY ([TEST_CASE_PARAMETER_ID]) REFERENCES [TST_TEST_CASE_PARAMETER] ([TEST_CASE_PARAMETER_ID])
GO


ALTER TABLE [TST_TEST_CONFIGURATION_PARAMETER_VALUE] ADD CONSTRAINT [FK_TST_TEST_CONFIGURATION_SET_PARAMETER_TST_TEST_CONFIGURATION_PARAMETER_VALUE] 
    FOREIGN KEY ([TEST_CONFIGURATION_SET_ID], [TEST_CASE_PARAMETER_ID]) REFERENCES [TST_TEST_CONFIGURATION_SET_PARAMETER] ([TEST_CONFIGURATION_SET_ID],[TEST_CASE_PARAMETER_ID])
GO


ALTER TABLE [TST_TEST_CONFIGURATION_PARAMETER_VALUE] ADD CONSTRAINT [FK_TST_TEST_CONFIGURATION_TST_TEST_CONFIGURATION_PARAMETER_VALUE] 
    FOREIGN KEY ([TEST_CONFIGURATION_ID]) REFERENCES [TST_TEST_CONFIGURATION] ([TEST_CONFIGURATION_ID])
GO


ALTER TABLE [TST_TEST_CONFIGURATION_PARAMETER_VALUE] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_VALUE_TST_TEST_CONFIGURATION_PARAMETER_VALUE] 
    FOREIGN KEY ([CUSTOM_PROPERTY_VALUE_ID]) REFERENCES [TST_CUSTOM_PROPERTY_VALUE] ([CUSTOM_PROPERTY_VALUE_ID])
GO


ALTER TABLE [TST_MESSAGE_ARTIFACT] ADD CONSTRAINT [FK_TST_USER_TST_MESSAGE_ARTIFACT] 
    FOREIGN KEY ([SENDER_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_MESSAGE_ARTIFACT] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_MESSAGE_ARTIFACT] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_RELEASE_TYPE_WORKFLOW] ADD CONSTRAINT [FK_TST_RELEASE_WORKFLOW_TST_RELEASE_TYPE_WORKFLOW] 
    FOREIGN KEY ([RELEASE_WORKFLOW_ID]) REFERENCES [TST_RELEASE_WORKFLOW] ([RELEASE_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RELEASE_TYPE_WORKFLOW] ADD CONSTRAINT [FK_TST_RELEASE_TYPE_TST_RELEASE_TYPE_WORKFLOW] 
    FOREIGN KEY ([RELEASE_TYPE_ID]) REFERENCES [TST_RELEASE_TYPE] ([RELEASE_TYPE_ID])
GO


ALTER TABLE [TST_RELEASE_TYPE_WORKFLOW] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RELEASE_TYPE_WORKFLOW] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_DOCUMENT_WORKFLOW] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_DOCUMENT_WORKFLOW_TST_DOCUMENT_WORKFLOW_TRANSITION] 
    FOREIGN KEY ([DOCUMENT_WORKFLOW_ID]) REFERENCES [TST_DOCUMENT_WORKFLOW] ([DOCUMENT_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_DOCUMENT_STATUS_TST_DOCUMENT_WORKFLOW_TRANSITION_INPUT] 
    FOREIGN KEY ([INPUT_DOCUMENT_STATUS_ID]) REFERENCES [TST_DOCUMENT_STATUS] ([DOCUMENT_STATUS_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_DOCUMENT_STATUS_TST_DOCUMENT_WORKFLOW_TRANSITION_OUTPUT] 
    FOREIGN KEY ([OUTPUT_DOCUMENT_STATUS_ID]) REFERENCES [TST_DOCUMENT_STATUS] ([DOCUMENT_STATUS_ID])
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_DOCUMENT_WORKFLOW_TRANSITION_TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ID]) REFERENCES [TST_DOCUMENT_WORKFLOW_TRANSITION] ([WORKFLOW_TRANSITION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_PROJECT_ROLE_TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_WORKFLOW_TRANSITION_ROLE_TYPE_TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ROLE_TYPE_ID]) REFERENCES [TST_WORKFLOW_TRANSITION_ROLE_TYPE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_DOCUMENT_WORKFLOW_TST_DOCUMENT_WORKFLOW_FIELD] 
    FOREIGN KEY ([DOCUMENT_WORKFLOW_ID]) REFERENCES [TST_DOCUMENT_WORKFLOW] ([DOCUMENT_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_DOCUMENT_STATUS_TST_DOCUMENT_WORKFLOW_FIELD] 
    FOREIGN KEY ([DOCUMENT_STATUS_ID]) REFERENCES [TST_DOCUMENT_STATUS] ([DOCUMENT_STATUS_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_DOCUMENT_WORKFLOW_FIELD] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_DOCUMENT_WORKFLOW_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_DOCUMENT_DISCUSSION] ADD CONSTRAINT [FK_TST_USER_TST_DOCUMENT_DISCUSSION] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_DOCUMENT_DISCUSSION] ADD CONSTRAINT [FK_TST_ATTACHMENT_TST_DOCUMENT_DISCUSSION] 
    FOREIGN KEY ([ARTIFACT_ID]) REFERENCES [TST_ATTACHMENT] ([ATTACHMENT_ID])
GO


ALTER TABLE [TST_DOCUMENT_STATUS] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_DOCUMENT_STATUS] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] ADD CONSTRAINT [FK_TST_DOCUMENT_WORKFLOW_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] 
    FOREIGN KEY ([DOCUMENT_WORKFLOW_ID]) REFERENCES [TST_DOCUMENT_WORKFLOW] ([DOCUMENT_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] ADD CONSTRAINT [FK_TST_DOCUMENT_STATUS_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] 
    FOREIGN KEY ([DOCUMENT_STATUS_ID]) REFERENCES [TST_DOCUMENT_STATUS] ([DOCUMENT_STATUS_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_RISK_PROBABILITY] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RISK_PROBABILITY] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_IMPACT] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RISK_IMPACT] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_NOTIFICATION_EVENT_TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY] 
    FOREIGN KEY ([NOTIFICATION_EVENT_ID]) REFERENCES [TST_NOTIFICATION_EVENT] ([NOTIFICATION_EVENT_ID])
GO


ALTER TABLE [TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_NOTIFICATION_EVENT_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_NOTIFICATION_EVENT_WEBHOOK] ADD CONSTRAINT [FK_TST_NOTIFICATION_EVENT_TST_NOTIFICATION_EVENT_WEBHOOK] 
    FOREIGN KEY ([NOTIFICATION_EVENT_ID]) REFERENCES [TST_NOTIFICATION_EVENT] ([NOTIFICATION_EVENT_ID])
GO


ALTER TABLE [TST_NOTIFICATION_EVENT_WEBHOOK] ADD CONSTRAINT [FK_TST_GLOBAL_OAUTH_PROVIDERS_TST_NOTIFICATION_EVENT_WEBHOOK] 
    FOREIGN KEY ([OAUTH_PROVIDER_ID]) REFERENCES [TST_GLOBAL_OAUTH_PROVIDERS] ([OAUTH_PROVIDER_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_RISK_IMPACT_TST_RISK] 
    FOREIGN KEY ([RISK_IMPACT_ID]) REFERENCES [TST_RISK_IMPACT] ([RISK_IMPACT_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_RISK_STATUS_TST_RISK] 
    FOREIGN KEY ([RISK_STATUS_ID]) REFERENCES [TST_RISK_STATUS] ([RISK_STATUS_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_RISK_PROBABILITY_TST_RISK] 
    FOREIGN KEY ([RISK_PROBABILITY_ID]) REFERENCES [TST_RISK_PROBABILITY] ([RISK_PROBABILITY_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_RISK_TYPE_TST_RISK] 
    FOREIGN KEY ([RISK_TYPE_ID]) REFERENCES [TST_RISK_TYPE] ([RISK_TYPE_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_PROJECT_TST_RISK] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_RELEASE_TST_RISK] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_COMPONENT_TST_RISK] 
    FOREIGN KEY ([COMPONENT_ID]) REFERENCES [TST_COMPONENT] ([COMPONENT_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_USER_TST_RISK_CREATOR] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_USER_TST_RISK_OWNER] 
    FOREIGN KEY ([OWNER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_RISK] 
    FOREIGN KEY ([PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID]) ON DELETE SET NULL
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_PROJECT_GOAL_TST_RISK] 
    FOREIGN KEY ([GOAL_ID]) REFERENCES [TST_PROJECT_GOAL] ([GOAL_ID])
GO


ALTER TABLE [TST_RISK] ADD CONSTRAINT [FK_TST_RISK_DETECTABILITY_TST_RISK] 
    FOREIGN KEY ([RISK_DETECTABILITY_ID]) REFERENCES [TST_RISK_DETECTABILITY] ([RISK_DETECTABILITY_ID])
GO


ALTER TABLE [TST_RISK_STATUS] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RISK_STATUS] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_TYPE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RISK_TYPE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_RISK_TYPE] ADD CONSTRAINT [FK_TST_RISK_WORKFLOW_TST_RISK_TYPE] 
    FOREIGN KEY ([RISK_WORKFLOW_ID]) REFERENCES [TST_RISK_WORKFLOW] ([RISK_WORKFLOW_ID])
GO


ALTER TABLE [TST_RISK_DISCUSSION] ADD CONSTRAINT [FK_TST_RISK_TST_RISK_DISCUSSION] 
    FOREIGN KEY ([ARTIFACT_ID]) REFERENCES [TST_RISK] ([RISK_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_DISCUSSION] ADD CONSTRAINT [FK_TST_USER_TST_RISK_DISCUSSION] 
    FOREIGN KEY ([CREATOR_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_RISK_MITIGATION] ADD CONSTRAINT [FK_TST_RISK_TST_RISK_MITIGATION] 
    FOREIGN KEY ([RISK_ID]) REFERENCES [TST_RISK] ([RISK_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_WORKFLOW] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RISK_WORKFLOW] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_RISK_WORKFLOW_TST_RISK_WORKFLOW_TRANSITION] 
    FOREIGN KEY ([RISK_WORKFLOW_ID]) REFERENCES [TST_RISK_WORKFLOW] ([RISK_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_RISK_STATUS_TST_RISK_WORKFLOW_TRANSITION_INPUT] 
    FOREIGN KEY ([INPUT_RISK_STATUS_ID]) REFERENCES [TST_RISK_STATUS] ([RISK_STATUS_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_TRANSITION] ADD CONSTRAINT [FK_TST_RISK_STATUS_TST_RISK_WORKFLOW_TRANSITION_OUTPUT] 
    FOREIGN KEY ([OUTPUT_RISK_STATUS_ID]) REFERENCES [TST_RISK_STATUS] ([RISK_STATUS_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_RISK_WORKFLOW_TRANSITION_TST_RISK_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ID]) REFERENCES [TST_RISK_WORKFLOW_TRANSITION] ([WORKFLOW_TRANSITION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_WORKFLOW_TRANSITION_ROLE_TYPE_TST_RISK_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([WORKFLOW_TRANSITION_ROLE_TYPE_ID]) REFERENCES [TST_WORKFLOW_TRANSITION_ROLE_TYPE] ([WORKFLOW_TRANSITION_ROLE_TYPE_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_TRANSITION_ROLE] ADD CONSTRAINT [FK_TST_PROJECT_ROLE_TST_RISK_WORKFLOW_TRANSITION_ROLE] 
    FOREIGN KEY ([PROJECT_ROLE_ID]) REFERENCES [TST_PROJECT_ROLE] ([PROJECT_ROLE_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_RISK_WORKFLOW_TST_RISK_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([RISK_WORKFLOW_ID]) REFERENCES [TST_RISK_WORKFLOW] ([RISK_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_RISK_STATUS_TST_RISK_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([RISK_STATUS_ID]) REFERENCES [TST_RISK_STATUS] ([RISK_STATUS_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TST_RISK_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_RISK_WORKFLOW_CUSTOM_PROPERTY] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_RISK_WORKFLOW_TST_RISK_WORKFLOW_FIELD] 
    FOREIGN KEY ([RISK_WORKFLOW_ID]) REFERENCES [TST_RISK_WORKFLOW] ([RISK_WORKFLOW_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_RISK_STATUS_TST_RISK_WORKFLOW_FIELD] 
    FOREIGN KEY ([RISK_STATUS_ID]) REFERENCES [TST_RISK_STATUS] ([RISK_STATUS_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TST_RISK_WORKFLOW_FIELD] 
    FOREIGN KEY ([ARTIFACT_FIELD_ID]) REFERENCES [TST_ARTIFACT_FIELD] ([ARTIFACT_FIELD_ID])
GO


ALTER TABLE [TST_RISK_WORKFLOW_FIELD] ADD CONSTRAINT [FK_TST_WORKFLOW_FIELD_STATE_TST_RISK_WORKFLOW_FIELD] 
    FOREIGN KEY ([WORKFLOW_FIELD_STATE_ID]) REFERENCES [TST_WORKFLOW_FIELD_STATE] ([WORKFLOW_FIELD_STATE_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_DOCUMENT] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_PROJECT_GROUP_DOCUMENT] 
    FOREIGN KEY ([PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_DOCUMENT] ADD CONSTRAINT [FK_TST_ATTACHMENT_TST_PROJECT_GROUP_DOCUMENT] 
    FOREIGN KEY ([ATTACHMENT_ID]) REFERENCES [TST_ATTACHMENT] ([ATTACHMENT_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_GOAL] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_PROJECT_GROUP_GOAL] 
    FOREIGN KEY ([PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID])
GO


ALTER TABLE [TST_PROJECT_GOAL] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_GOAL] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_MILESTONE] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_ROADMAP_TST_PROJECT_GROUP_MILESTONE] 
    FOREIGN KEY ([ROADMAP_ID]) REFERENCES [TST_PROJECT_GROUP_ROADMAP] ([ROADMAP_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_ROADMAP] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_PROJECT_GROUP_ROADMAP] 
    FOREIGN KEY ([PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_THEME] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_ROADMAP_TST_PROJECT_GROUP_THEME] 
    FOREIGN KEY ([ROADMAP_ID]) REFERENCES [TST_PROJECT_GROUP_ROADMAP] ([ROADMAP_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_THEME] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_MILESTONE_TST_PROJECT_GROUP_THEME] 
    FOREIGN KEY ([MILESTONE_ID]) REFERENCES [TST_PROJECT_GROUP_MILESTONE] ([MILESTONE_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_THEME] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_GOAL_TST_PROJECT_GROUP_THEME] 
    FOREIGN KEY ([GOAL_ID]) REFERENCES [TST_PROJECT_GROUP_GOAL] ([GOAL_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_THEME] ADD CONSTRAINT [FK_TST_THEME_PRIORITY_TST_PROJECT_GROUP_THEME] 
    FOREIGN KEY ([THEME_PRIORITY_ID]) REFERENCES [TST_THEME_PRIORITY] ([THEME_PRIORITY_ID])
GO


ALTER TABLE [TST_PROJECT_GROUP_THEME] ADD CONSTRAINT [FK_TST_THEME_STATUS_TST_PROJECT_GROUP_THEME] 
    FOREIGN KEY ([THEME_STATUS_ID]) REFERENCES [TST_THEME_STATUS] ([THEME_STATUS_ID])
GO


ALTER TABLE [TST_USER_IDEA] ADD CONSTRAINT [FK_TST_USER_TST_USER_IDEA] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_ALLOCATION_PLANNED] ADD CONSTRAINT [FK_TST_RELEASE_TST_ALLOCATION_PLANNED] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID])
GO


ALTER TABLE [TST_ALLOCATION_PLANNED] ADD CONSTRAINT [FK_TST_RESOURCE_CATEGORY_TST_ALLOCATION_PLANNED] 
    FOREIGN KEY ([RESOURCE_CATEGORY_ID]) REFERENCES [TST_RESOURCE_CATEGORY] ([RESOURCE_CATEGORY_ID])
GO


ALTER TABLE [TST_ALLOCATION_PLANNED] ADD CONSTRAINT [FK_TST_RESOURCE_TRACK_TST_ALLOCATION_PLANNED] 
    FOREIGN KEY ([RESOURCE_TRACK_ID]) REFERENCES [TST_RESOURCE_TRACK] ([RESOURCE_TRACK_ID])
GO


ALTER TABLE [TST_RESOURCE_CATEGORY] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RESOURCE_CATEGORY] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_RESOURCE_TRACK] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RESOURCE_TRACK] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_ALLOCATION_ACTUAL] ADD CONSTRAINT [FK_TST_ALLOCATION_PLANNED_TST_ALLOCATION_ACTUAL] 
    FOREIGN KEY ([ALLOCATION_ID]) REFERENCES [TST_ALLOCATION_PLANNED] ([ALLOCATION_ID])
GO


ALTER TABLE [TST_ALLOCATION_ACTUAL] ADD CONSTRAINT [FK_TST_USER_TST_ALLOCATION_ACTUAL] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_ALLOCATION_ACTUAL] ADD CONSTRAINT [FK_TST_RESOURCE_CATEGORY_TST_ALLOCATION_ACTUAL] 
    FOREIGN KEY ([RESOURCE_CATEGORY_ID]) REFERENCES [TST_RESOURCE_CATEGORY] ([RESOURCE_CATEGORY_ID])
GO


ALTER TABLE [TST_TIMECARD] ADD CONSTRAINT [FK_TST_USER_TST_TIMECARD] 
    FOREIGN KEY ([SUBMITTER_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TIMECARD] ADD CONSTRAINT [FK_TST_TIMECARD_STATUS_TST_TIMECARD] 
    FOREIGN KEY ([TIMECARD_STATUS_ID]) REFERENCES [TST_TIMECARD_STATUS] ([TIMECARD_STATUS_ID])
GO


ALTER TABLE [TST_TIMECARD] ADD CONSTRAINT [FK_TST_USER_TST_TIMECARD_APPROVER] 
    FOREIGN KEY ([APPROVER_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TIMECARD_ENTRY] ADD CONSTRAINT [FK_TST_TIMECARD_TST_TIMECARD_ENTRY] 
    FOREIGN KEY ([TIMECARD_ID]) REFERENCES [TST_TIMECARD] ([TIMECARD_ID])
GO


ALTER TABLE [TST_TIMECARD_ENTRY] ADD CONSTRAINT [FK_TST_PROJECT_TST_TIMECARD_ENTRY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_TIMECARD_ENTRY] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_TIMECARD_ENTRY] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_TIMECARD_ENTRY] ADD CONSTRAINT [FK_TST_USER_TST_TIMECARD_ENTRY] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_TIMECARD_ENTRY] ADD CONSTRAINT [FK_TST_RESOURCE_CATEGORY_TST_TIMECARD_ENTRY] 
    FOREIGN KEY ([RESOURCE_CATEGORY_ID]) REFERENCES [TST_RESOURCE_CATEGORY] ([RESOURCE_CATEGORY_ID])
GO


ALTER TABLE [TST_TIMECARD_ENTRY] ADD CONSTRAINT [FK_TST_TIMECARD_ENTRY_TYPE_TST_TIMECARD_ENTRY] 
    FOREIGN KEY ([TIMECARD_ENTRY_TYPE_ID]) REFERENCES [TST_TIMECARD_ENTRY_TYPE] ([TIMECARD_ENTRY_TYPE_ID])
GO


ALTER TABLE [TST_TIMECARD_ENTRY] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_TIMECARD_ENTRY] 
    FOREIGN KEY ([PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID])
GO


ALTER TABLE [TST_TIMECARD_ENTRY_TYPE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_TIMECARD_ENTRY_TYPE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_HISTORY_ASSOCIATION] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_ASSOCIATION] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_HISTORY_ASSOCIATION] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_HISTORY_ASSOCIATION_SOURCE] 
    FOREIGN KEY ([SOURCE_ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_HISTORY_ASSOCIATION] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_HISTORY_ASSOCIATION_DEST] 
    FOREIGN KEY ([DEST_ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_HISTORY_ASSOCIATION] ADD CONSTRAINT [FK_TST_ARTIFACT_LINK_TST_HISTORY_ASSOCIATION] 
    FOREIGN KEY ([ARTIFACT_LINK_ID]) REFERENCES [TST_ARTIFACT_LINK] ([ARTIFACT_LINK_ID])
GO


ALTER TABLE [TST_HISTORY_ASSOCIATION] ADD CONSTRAINT [FK_TST_ARTIFACT_LINK_TYPE_TST_HISTORY_ASSOCIATION_OLD] 
    FOREIGN KEY ([OLD_ARTIFACT_LINK_TYPE_ID]) REFERENCES [TST_ARTIFACT_LINK_TYPE] ([ARTIFACT_LINK_TYPE_ID])
GO


ALTER TABLE [TST_HISTORY_ASSOCIATION] ADD CONSTRAINT [FK_TST_ARTIFACT_LINK_TYPE_TST_HISTORY_ASSOCIATION_NEW] 
    FOREIGN KEY ([NEW_ARTIFACT_LINK_TYPE_ID]) REFERENCES [TST_ARTIFACT_LINK_TYPE] ([ARTIFACT_LINK_TYPE_ID])
GO


ALTER TABLE [TST_PROJECT_BASELINE] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_BASELINE] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_PROJECT_BASELINE] ADD CONSTRAINT [FK_TST_RELEASE_TST_PROJECT_BASELINE] 
    FOREIGN KEY ([RELEASE_ID]) REFERENCES [TST_RELEASE] ([RELEASE_ID])
GO


ALTER TABLE [TST_PROJECT_BASELINE] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_PROJECT_BASELINE] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_BASELINE] ADD CONSTRAINT [FK_TST_USER_TST_PROJECT_BASELINE_CREATOR] 
    FOREIGN KEY ([CREATOR_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_VERSION_CONTROL_BRANCH] ADD CONSTRAINT [FK_TST_VERSION_CONTROL_PROJECT_TST_VERSION_CONTROL_BRANCH] 
    FOREIGN KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID]) REFERENCES [TST_VERSION_CONTROL_PROJECT] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_VERSION_CONTROL_PULL_REQUEST] ADD CONSTRAINT [FK_TST_VERSION_CONTROL_BRANCH_TST_VERSION_CONTROL_PULL_REQUEST_SOURCE] 
    FOREIGN KEY ([SOURCE_BRANCH_ID]) REFERENCES [TST_VERSION_CONTROL_BRANCH] ([BRANCH_ID])
GO


ALTER TABLE [TST_VERSION_CONTROL_PULL_REQUEST] ADD CONSTRAINT [FK_TST_TASK_TST_VERSION_CONTROL_PULL_REQUEST] 
    FOREIGN KEY ([TASK_ID]) REFERENCES [TST_TASK] ([TASK_ID])
GO


ALTER TABLE [TST_VERSION_CONTROL_PULL_REQUEST] ADD CONSTRAINT [FK_TST_VERSION_CONTROL_BRANCH_TST_VERSION_CONTROL_PULL_REQUEST_DEST] 
    FOREIGN KEY ([DEST_BRANCH_ID]) REFERENCES [TST_VERSION_CONTROL_BRANCH] ([BRANCH_ID])
GO


ALTER TABLE [TST_GLOBAL_ARTIFACT_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_GLOBAL_ARTIFACT_CUSTOM_PROPERTY] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_GLOBAL_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_TYPE_TST_GLOBAL_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_TYPE_ID]) REFERENCES [TST_CUSTOM_PROPERTY_TYPE] ([CUSTOM_PROPERTY_TYPE_ID])
GO


ALTER TABLE [TST_GLOBAL_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_GLOBAL_CUSTOM_PROPERTY] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_GLOBAL_CUSTOM_PROPERTY] ADD CONSTRAINT [FK_TST_GLOBAL_CUSTOM_PROPERTY_LIST_TST_GLOBAL_CUSTOM_PROPERTY] 
    FOREIGN KEY ([CUSTOM_PROPERTY_LIST_ID]) REFERENCES [TST_GLOBAL_CUSTOM_PROPERTY_LIST] ([CUSTOM_PROPERTY_LIST_ID])
GO


ALTER TABLE [TST_GLOBAL_CUSTOM_PROPERTY_VALUE] ADD CONSTRAINT [FK_TST_GLOBAL_CUSTOM_PROPERTY_LIST_TST_GLOBAL_CUSTOM_PROPERTY_VALUE] 
    FOREIGN KEY ([CUSTOM_PROPERTY_LIST_ID]) REFERENCES [TST_GLOBAL_CUSTOM_PROPERTY_LIST] ([CUSTOM_PROPERTY_LIST_ID])
GO


ALTER TABLE [TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE] ADD CONSTRAINT [FK_TST_GLOBAL_CUSTOM_PROPERTY_TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE] 
    FOREIGN KEY ([CUSTOM_PROPERTY_ID]) REFERENCES [TST_GLOBAL_CUSTOM_PROPERTY] ([CUSTOM_PROPERTY_ID])
GO


ALTER TABLE [TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE] ADD CONSTRAINT [FK_TST_CUSTOM_PROPERTY_OPTION_TST_GLOBAL_CUSTOM_PROPERTY_OPTION_VALUE] 
    FOREIGN KEY ([CUSTOM_PROPERTY_OPTION_ID]) REFERENCES [TST_CUSTOM_PROPERTY_OPTION] ([CUSTOM_PROPERTY_OPTION_ID])
GO


ALTER TABLE [TST_HISTORY_POSITION] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_POSITION] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_HISTORY_POSITION] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_HISTORY_POSITION_CHILD] 
    FOREIGN KEY ([CHILD_ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID])
GO


ALTER TABLE [TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_PROJECT_TEMPLATE_ARTIFACT_DEFAULT] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_PROJECT_TEMPLATE_USER] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_PROJECT_TEMPLATE_USER] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_TEMPLATE_USER] ADD CONSTRAINT [FK_TST_USER_TST_PROJECT_TEMPLATE_USER] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_ARTIFACT_TAGS] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_ARTIFACT_TAGS] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_ARTIFACT_TAGS] ADD CONSTRAINT [FK_TST_PROJECT_TST_ARTIFACT_TAGS] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID])
GO


ALTER TABLE [TST_GLOBAL_TAGS] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_GLOBAL_TAGS] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_HISTORY_DISCUSSION] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_DISCUSSION] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID])
GO


ALTER TABLE [TST_HISTORY_DISCUSSION] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_HISTORY_DISCUSSION] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_MESSAGE_TRACK] ADD CONSTRAINT [FK_TST_RESOURCE_TRACK_TST_MESSAGE_TRACK] 
    FOREIGN KEY ([RESOURCE_TRACK_ID]) REFERENCES [TST_RESOURCE_TRACK] ([RESOURCE_TRACK_ID])
GO


ALTER TABLE [TST_MESSAGE_TRACK] ADD CONSTRAINT [FK_TST_USER_TST_MESSAGE_TRACK] 
    FOREIGN KEY ([SENDER_USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_USER_PAGE_VIEWED] ADD CONSTRAINT [FK_TST_USER_TST_USER_PAGE_VIEWED] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_MULTI_APPROVER] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_MULTI_APPROVER] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_MULTI_APPROVER] ADD CONSTRAINT [FK_TST_MULTI_APPROVER_TYPE_TST_MULTI_APPROVER] 
    FOREIGN KEY ([MULTI_APPROVER_TYPE_ID]) REFERENCES [TST_MULTI_APPROVER_TYPE] ([MULTI_APPROVER_TYPE_ID])
GO


ALTER TABLE [TST_MULTI_APPROVER_TYPE] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_MULTI_APPROVER_TYPE] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_MULTI_APPROVER_EXECUTED] ADD CONSTRAINT [FK_TST_MULTI_APPROVER_TST_MULTI_APPROVER_EXECUTED] 
    FOREIGN KEY ([MULTI_APPROVER_ID]) REFERENCES [TST_MULTI_APPROVER] ([MULTI_APPROVER_ID])
GO


ALTER TABLE [TST_MULTI_APPROVER_EXECUTED] ADD CONSTRAINT [FK_TST_USER_TST_MULTI_APPROVER_EXECUTED] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID])
GO


ALTER TABLE [TST_MULTI_APPROVER_EXECUTED] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_MULTI_APPROVER_EXECUTED] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_HISTORY_TAGS] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_TAGS] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_HISTORY_CHANGESET] ([CHANGESET_ID])
GO


ALTER TABLE [TST_TEST_CASE_FOLDER_HIERARCHY] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_CASE_FOLDER_HIERARCHY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_SET_FOLDER_HIERARCHY] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_SET_FOLDER_HIERARCHY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY] ADD CONSTRAINT [FK_TST_PROJECT_TST_PROJECT_ATTACHMENT_FOLDER_HIERARCHY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TASK_FOLDER_HIERARCHY] ADD CONSTRAINT [FK_TST_PROJECT_TST_TASK_FOLDER_HIERARCHY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_CASE_PARAMETER_HIERARCHY] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_CASE_PARAMETER_HIERARCHY] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET] ADD CONSTRAINT [FK_TST_PROJECT_TST_TEST_CASE_PARAMETER_HIERARCHY_ALREADY_SET] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_SOURCE_CODE_COMMIT] ADD CONSTRAINT [FK_TST_VERSION_CONTROL_PROJECT_TST_SOURCE_CODE_COMMIT] 
    FOREIGN KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID]) REFERENCES [TST_VERSION_CONTROL_PROJECT] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_SOURCE_CODE_FILE_ENTRY] ADD CONSTRAINT [FK_TST_SOURCE_CODE_COMMIT_TST_SOURCE_CODE_FILE_ENTRY] 
    FOREIGN KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID], [REVISION_ID]) REFERENCES [TST_SOURCE_CODE_COMMIT] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID],[REVISION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_SOURCE_CODE_COMMIT_BRANCH] ADD CONSTRAINT [FK_TST_SOURCE_CODE_COMMIT_TST_SOURCE_CODE_COMMIT_BRANCH] 
    FOREIGN KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID], [REVISION_ID]) REFERENCES [TST_SOURCE_CODE_COMMIT] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID],[REVISION_ID])
GO


ALTER TABLE [TST_SOURCE_CODE_COMMIT_BRANCH] ADD CONSTRAINT [FK_TST_VERSION_CONTROL_BRANCH_TST_SOURCE_CODE_COMMIT_BRANCH] 
    FOREIGN KEY ([BRANCH_ID]) REFERENCES [TST_VERSION_CONTROL_BRANCH] ([BRANCH_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_SOURCE_CODE_COMMIT_ARTIFACT] ADD CONSTRAINT [FK_TST_SOURCE_CODE_COMMIT_TST_SOURCE_CODE_COMMIT_ARTIFACT] 
    FOREIGN KEY ([VERSION_CONTROL_SYSTEM_ID], [PROJECT_ID], [REVISION_ID]) REFERENCES [TST_SOURCE_CODE_COMMIT] ([VERSION_CONTROL_SYSTEM_ID],[PROJECT_ID],[REVISION_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_SOURCE_CODE_COMMIT_ARTIFACT] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_SOURCE_CODE_COMMIT_ARTIFACT] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_PROJECT_TEMPLATE_SETTING_VALUE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_SETTING_TST_PROJECT_TEMPLATE_SETTING_VALUE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_SETTING_ID]) REFERENCES [TST_PROJECT_TEMPLATE_SETTING] ([PROJECT_TEMPLATE_SETTING_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_TEMPLATE_SETTING_VALUE] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_PROJECT_TEMPLATE_SETTING_VALUE] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_GROUP_SETTING_VALUE] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_SETTING_TST_PROJECT_GROUP_SETTING_VALUE] 
    FOREIGN KEY ([PROJECT_GROUP_SETTING_ID]) REFERENCES [TST_PROJECT_GROUP_SETTING] ([PROJECT_GROUP_SETTING_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PROJECT_GROUP_SETTING_VALUE] ADD CONSTRAINT [FK_TST_PROJECT_GROUP_TST_PROJECT_GROUP_SETTING_VALUE] 
    FOREIGN KEY ([PROJECT_GROUP_ID]) REFERENCES [TST_PROJECT_GROUP] ([PROJECT_GROUP_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PORTFOLIO_SETTING_VALUE] ADD CONSTRAINT [FK_TST_PORTFOLIO_SETTING_TST_PORTFOLIO_SETTING_VALUE] 
    FOREIGN KEY ([PORTFOLIO_SETTING_ID]) REFERENCES [TST_PORTFOLIO_SETTING] ([PORTFOLIO_SETTING_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_PORTFOLIO_SETTING_VALUE] ADD CONSTRAINT [FK_TST_PORTFOLIO_TST_PORTFOLIO_SETTING_VALUE] 
    FOREIGN KEY ([PORTFOLIO_ID]) REFERENCES [TST_PORTFOLIO] ([PORTFOLIO_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_RISK_DETECTABILITY] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_RISK_DETECTABILITY] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_RECENT_ARTIFACT] ADD CONSTRAINT [FK_TST_USER_TST_USER_RECENT_ARTIFACT] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_RECENT_ARTIFACT] ADD CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_USER_RECENT_ARTIFACT] 
    FOREIGN KEY ([ARTIFACT_TYPE_ID]) REFERENCES [TST_ARTIFACT_TYPE] ([ARTIFACT_TYPE_ID])
GO


ALTER TABLE [TST_USER_RECENT_ARTIFACT] ADD CONSTRAINT [FK_TST_PROJECT_TST_USER_RECENT_ARTIFACT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_RECENT_PROJECT] ADD CONSTRAINT [FK_TST_USER_TST_USER_RECENT_PROJECT] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_USER_RECENT_PROJECT] ADD CONSTRAINT [FK_TST_PROJECT_TST_USER_RECENT_PROJECT] 
    FOREIGN KEY ([PROJECT_ID]) REFERENCES [TST_PROJECT] ([PROJECT_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_STANDARD_TASK_SET] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_STANDARD_TASK_SET] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_STANDARD_TASK] ADD CONSTRAINT [FK_TST_STANDARD_TASK_SET_TST_STANDARD_TASK] 
    FOREIGN KEY ([STANDARD_TASK_SET_ID]) REFERENCES [TST_STANDARD_TASK_SET] ([STANDARD_TASK_SET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_STANDARD_TASK] ADD CONSTRAINT [FK_TST_TASK_TYPE_TST_STANDARD_TASK] 
    FOREIGN KEY ([TASK_TYPE_ID]) REFERENCES [TST_TASK_TYPE] ([TASK_TYPE_ID])
GO


ALTER TABLE [TST_STANDARD_TEST_CASE_SET] ADD CONSTRAINT [FK_TST_PROJECT_TEMPLATE_TST_STANDARD_TEST_CASE_SET] 
    FOREIGN KEY ([PROJECT_TEMPLATE_ID]) REFERENCES [TST_PROJECT_TEMPLATE] ([PROJECT_TEMPLATE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_STANDARD_TEST_CASE] ADD CONSTRAINT [FK_TST_STANDARD_TEST_CASE_SET_TST_STANDARD_TEST_CASE] 
    FOREIGN KEY ([STANDARD_TEST_CASE_SET_ID]) REFERENCES [TST_STANDARD_TEST_CASE_SET] ([STANDARD_TEST_CASE_SET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_STANDARD_TEST_CASE] ADD CONSTRAINT [FK_TST_TEST_CASE_TYPE_TST_STANDARD_TEST_CASE] 
    FOREIGN KEY ([TEST_CASE_TYPE_ID]) REFERENCES [TST_TEST_CASE_TYPE] ([TEST_CASE_TYPE_ID])
GO


ALTER TABLE [TST_WORKFLOW_TRANSITION_STANDARD_TASK] ADD CONSTRAINT [FK_TST_STANDARD_TASK_SET_TST_WORKFLOW_TRANSITION_STANDARD_TASK] 
    FOREIGN KEY ([STANDARD_TASK_SET_ID]) REFERENCES [TST_STANDARD_TASK_SET] ([STANDARD_TASK_SET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_WORKFLOW_TRANSITION_STANDARD_TEST_CASE] ADD CONSTRAINT [FK_TST_STANDARD_TEST_CASE_SET_TST_WORKFLOW_TRANSITION_STANDARD_TEST_CASE] 
    FOREIGN KEY ([STANDARD_TEST_CASE_SET_ID]) REFERENCES [TST_STANDARD_TEST_CASE_SET] ([STANDARD_TEST_CASE_SET_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_GLOBAL_HISTORY_CHANGESET] ADD CONSTRAINT [FK_TST_WORKSPACE_TYPE_TST_GLOBAL_HISTORY_CHANGESET] 
    FOREIGN KEY ([WORKSPACE_TYPE_ID]) REFERENCES [TST_WORKSPACE_TYPE] ([WORKSPACE_TYPE_ID]) ON DELETE CASCADE
GO


ALTER TABLE [TST_GLOBAL_HISTORY_CHANGESET] ADD CONSTRAINT [FK_TST_USER_TST_GLOBAL_HISTORY_CHANGESET] 
    FOREIGN KEY ([USER_ID]) REFERENCES [TST_USER] ([USER_ID]) ON DELETE SET DEFAULT
GO


ALTER TABLE [TST_GLOBAL_HISTORY_CHANGESET] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TYPE_TST_GLOBAL_HISTORY_CHANGESET] 
    FOREIGN KEY ([CHANGETYPE_ID]) REFERENCES [TST_HISTORY_CHANGESET_TYPE] ([CHANGETYPE_ID]) ON DELETE SET DEFAULT
GO


ALTER TABLE [TST_GLOBAL_HISTORY_DETAILS] ADD CONSTRAINT [FK_TST_ARTIFACT_FIELD_TYPE_TST_GLOBAL_HISTORY_DETAILS] 
    FOREIGN KEY ([FIELD_TYPE_ID]) REFERENCES [TST_ARTIFACT_FIELD_TYPE] ([ARTIFACT_FIELD_TYPE_ID]) ON DELETE SET DEFAULT
GO


ALTER TABLE [TST_GLOBAL_HISTORY_DETAILS] ADD CONSTRAINT [FK_TST_GLOBAL_HISTORY_CHANGESET_TST_GLOBAL_HISTORY_DETAILS] 
    FOREIGN KEY ([CHANGESET_ID]) REFERENCES [TST_GLOBAL_HISTORY_CHANGESET] ([CHANGESET_ID]) ON DELETE CASCADE
GO

