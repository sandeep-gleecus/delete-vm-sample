/***************************************************************
**	Insert script for table TST_USER_PROFILE
***************************************************************/
INSERT INTO TST_USER_PROFILE
(
USER_ID, FIRST_NAME, LAST_NAME, MIDDLE_INITIAL, LAST_OPENED_PROJECT_ID, IS_ADMIN, IS_EMAIL_ENABLED, DEPARTMENT, LAST_UPDATE_DATE, IS_BUSY, IS_AWAY, UNREAD_MESSAGES, IS_RESOURCE_ADMIN, IS_PORTFOLIO_ADMIN, IS_RESTRICTED, IS_REPORT_ADMIN
)
VALUES
(
1, 'System', 'Administrator', NULL, NULL, 1, 1, NULL, '2011-01-01T00:00:00', 0, 0, 0, 0, 1, 0, 1
)
GO

