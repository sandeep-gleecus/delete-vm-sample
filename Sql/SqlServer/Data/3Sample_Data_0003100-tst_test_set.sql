/***************************************************************
**	Insert script for table TST_TEST_SET
***************************************************************/
SET IDENTITY_INSERT TST_TEST_SET ON; 

INSERT INTO TST_TEST_SET
(
TEST_SET_ID, PROJECT_ID, TEST_SET_FOLDER_ID, RELEASE_ID, TEST_SET_STATUS_ID, CREATOR_ID, OWNER_ID, TEST_RUN_TYPE_ID, RECURRENCE_ID, AUTOMATION_HOST_ID, NAME, DESCRIPTION, IS_ATTACHMENTS, CREATION_DATE, PLANNED_DATE, LAST_UPDATE_DATE, CONCURRENCY_DATE, BUILD_EXECUTE_TIME_INTERVAL, EXECUTION_DATE, ESTIMATED_DURATION, ACTUAL_DURATION, COUNT_PASSED, COUNT_FAILED, COUNT_CAUTION, COUNT_BLOCKED, COUNT_NOT_RUN, COUNT_NOT_APPLICABLE, IS_DYNAMIC, IS_AUTO_SCHEDULED
)
VALUES
(
1, 1, 1, 1, 2, 2, 3, 1, NULL, NULL, 'Testing Cycle for Release 1.0', 'This tests the functionality introduced in release 1.0 of the library system', 0, DATEADD(day, -143, SYSUTCDATETIME()), DATEADD(day, -87, SYSUTCDATETIME()), DATEADD(day, -6, SYSUTCDATETIME()), DATEADD(day, -6, SYSUTCDATETIME()), NULL, DATEADD(day, -25, SYSUTCDATETIME()), 44, 255, 1, 2, 0, 0, 4, 0, 0, 0
),
(
2, 1, 1, 4, 1, 2, 3, 1, NULL, NULL, 'Testing Cycle for Release 1.1', 'This tests the functionality introduced in release 2.0 of the library system', 0, DATEADD(day, -107, SYSUTCDATETIME()), DATEADD(day, -9, SYSUTCDATETIME()), DATEADD(day, -9, SYSUTCDATETIME()), DATEADD(day, -9, SYSUTCDATETIME()), NULL, DATEADD(day, -90, SYSUTCDATETIME()), 44, 270, 3, 0, 0, 0, 6, 0, 0, 0
),
(
3, 1, 2, NULL, 3, 2, 2, 1, NULL, NULL, 'Regression Testing for Windows 8', NULL, 0, DATEADD(day, -107, SYSUTCDATETIME()), NULL, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(day, -105, SYSUTCDATETIME()), NULL, NULL, 16, NULL, 0, 0, 0, 0, 4, 0, 0, 0
),
(
4, 1, 2, NULL, 3, 3, NULL, 1, NULL, NULL, 'Regression Testing for Windows Vista', NULL, 0, DATEADD(day, -107, SYSUTCDATETIME()), NULL, DATEADD(day, -104, SYSUTCDATETIME()), DATEADD(day, -104, SYSUTCDATETIME()), NULL, NULL, 16, NULL, 0, 0, 0, 0, 4, 0, 0, 0
),
(
5, 1, 1, 6, 2, 3, 2, 1, NULL, NULL, 'Testing New Functionality', 'This set contains all the new features introduced in the last 3 sub-versions', 0, DATEADD(day, -142, SYSUTCDATETIME()), DATEADD(day, -125, SYSUTCDATETIME()), DATEADD(day, -135, SYSUTCDATETIME()), DATEADD(day, -135, SYSUTCDATETIME()), NULL, NULL, 16, NULL, 0, 0, 0, 0, 4, 0, 0, 0
),
(
6, 1, 1, NULL, 5, 3, 2, 1, NULL, NULL, 'Exploratory Testing', NULL, 0, DATEADD(day, -142, SYSUTCDATETIME()), NULL, DATEADD(day, -142, SYSUTCDATETIME()), DATEADD(day, -142, SYSUTCDATETIME()), NULL, NULL, NULL, NULL, 0, 0, 0, 0, 2, 0, 0, 0
)
GO

SET IDENTITY_INSERT TST_TEST_SET OFF; 

