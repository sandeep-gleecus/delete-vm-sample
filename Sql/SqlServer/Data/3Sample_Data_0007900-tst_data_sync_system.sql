/***************************************************************
**	Insert script for table TST_DATA_SYNC_SYSTEM
***************************************************************/
SET IDENTITY_INSERT TST_DATA_SYNC_SYSTEM ON; 

INSERT INTO TST_DATA_SYNC_SYSTEM
(
DATA_SYNC_SYSTEM_ID, DATA_SYNC_STATUS_ID, NAME, CAPTION, DESCRIPTION, CONNECTION_STRING, EXTERNAL_LOGIN, IS_ACTIVE, EXTERNAL_PASSWORD, TIME_OFFSET_HOURS, LAST_SYNC_DATE, AUTO_MAP_USERS_YN, CUSTOM_01, CUSTOM_02, CUSTOM_03, CUSTOM_04, CUSTOM_05, IS_ENCRYPTED
)
VALUES
(
1, 4, 'JiraDataSync', 'Jira', 'This plug-in allows incidents in the system to be synchronized with the JIRA issue-tracking system', 'https://myinstance.atlassian.net', 'username', 0, NULL, 0, NULL, 'N', NULL, NULL, NULL, NULL, NULL, 0
),
(
2, 4, 'GitHubDataSync', 'GitHub', 'This plug-in allows incidents in the system to be synchronized with the GitHub bug-tracking system', 'myrepository', 'username', 0, NULL, 0, NULL, 'N', NULL, NULL, NULL, NULL, NULL, 0
),
(
3, 4, 'MsTfsDataSync', 'Microsoft TFS', 'This plug-in allows incidents and tasks in the system to be synchronized with Microsoft Team Foundation Server (TFS)', 'http://inflectrasvr03:8080/tfs/DefaultCollection', 'username', 0, NULL, 0, NULL, 'N', 'DOMAIN', NULL, NULL, NULL, NULL, 0
)
GO

SET IDENTITY_INSERT TST_DATA_SYNC_SYSTEM OFF; 

