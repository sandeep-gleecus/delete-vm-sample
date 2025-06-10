/***************************************************************
**	Insert script for table TST_RELEASE_TEST_SET
***************************************************************/
INSERT INTO TST_RELEASE_TEST_SET
(
RELEASE_ID, TEST_SET_ID, ACTUAL_DURATION, COUNT_PASSED, COUNT_FAILED, COUNT_BLOCKED, COUNT_CAUTION, COUNT_NOT_RUN, COUNT_NOT_APPLICABLE, EXECUTION_DATE
)
VALUES
(
1, 1, 255, 1, 2, 0, 0, 0, 0, DATEADD(day, -25, SYSUTCDATETIME())
),
(
1, 2, 270, 3, 0, 0, 0, 0, 0, DATEADD(day, -90, SYSUTCDATETIME())
),
(
2, 2, 270, 3, 0, 0, 0, 0, 0, DATEADD(day, -90, SYSUTCDATETIME())
)
GO

