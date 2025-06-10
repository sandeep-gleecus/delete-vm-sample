/***************************************************************
**	Insert script for table TST_RELEASE_TEST_SET_FOLDER
***************************************************************/
INSERT INTO TST_RELEASE_TEST_SET_FOLDER
(
RELEASE_ID, TEST_SET_FOLDER_ID, ACTUAL_DURATION, COUNT_BLOCKED, COUNT_CAUTION, COUNT_FAILED, COUNT_NOT_APPLICABLE, COUNT_NOT_RUN, COUNT_PASSED, EXECUTION_DATE
)
VALUES
(
1, 1, 525, 0, 0, 2, 0, 0, 4, DATEADD(day, -90, SYSUTCDATETIME())
),
(
2, 1, 270, 0, 0, 0, 0, 0, 3, DATEADD(day, -90, SYSUTCDATETIME())
)
GO

