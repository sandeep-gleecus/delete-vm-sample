/***************************************************************
**	Insert script for table TST_RELEASE_TEST_CASE_FOLDER
***************************************************************/
INSERT INTO TST_RELEASE_TEST_CASE_FOLDER
(
RELEASE_ID, TEST_CASE_FOLDER_ID, EXECUTION_DATE, ACTUAL_DURATION, COUNT_PASSED, COUNT_FAILED, COUNT_BLOCKED, COUNT_CAUTION, COUNT_NOT_RUN, COUNT_NOT_APPLICABLE
)
VALUES
(
1, 1, DATEADD(day, -60, SYSUTCDATETIME()), 390, 2, 1, 1, 0, 0, 0
),
(
1, 2, DATEADD(day, -90, SYSUTCDATETIME()), 180, 2, 0, 0, 0, 0, 0
),
(
2, 1, DATEADD(day, -85, SYSUTCDATETIME()), 275, 2, 0, 1, 0, 2, 0
),
(
2, 2, DATEADD(day, -90, SYSUTCDATETIME()), 90, 1, 0, 0, 0, 1, 0
),
(
3, 1, DATEADD(day, -42, SYSUTCDATETIME()), 90, 1, 0, 0, 0, 0, 0
),
(
3, 2, DATEADD(day, -26, SYSUTCDATETIME()), 90, 1, 0, 0, 0, 0, 0
),
(
4, 1, DATEADD(day, -10, SYSUTCDATETIME()), 275, 0, 2, 0, 2, 1, 0
),
(
4, 2, DATEADD(day, -10, SYSUTCDATETIME()), 75, 2, 0, 0, 0, 0, 0
),
(
4, 3, DATEADD(day, -15, SYSUTCDATETIME()), 1, 1, 0, 0, 0, 1, 0
),
(
4, 4, DATEADD(day, -15, SYSUTCDATETIME()), 1, 1, 0, 0, 0, 1, 0
),
(
5, 1, NULL, NULL, 0, 0, 0, 0, 3, 0
),
(
5, 2, NULL, NULL, 0, 0, 0, 0, 2, 0
),
(
5, 3, NULL, NULL, 0, 0, 0, 0, 2, 0
),
(
5, 4, NULL, NULL, 0, 0, 0, 0, 2, 0
),
(
6, 1, NULL, NULL, 0, 0, 0, 0, 4, 0
),
(
6, 2, NULL, NULL, 0, 0, 0, 0, 1, 0
),
(
6, 3, NULL, NULL, 0, 0, 0, 0, 2, 0
),
(
6, 4, NULL, NULL, 0, 0, 0, 0, 2, 0
),
(
8, 1, DATEADD(day, -42, SYSUTCDATETIME()), 0, 1, 0, 0, 0, 0, 0
),
(
8, 3, DATEADD(day, -40, SYSUTCDATETIME()), 0, 1, 0, 0, 0, 0, 0
),
(
8, 4, DATEADD(day, -40, SYSUTCDATETIME()), 0, 1, 0, 0, 0, 0, 0
),
(
17, 1, DATEADD(day, -14, SYSUTCDATETIME()), 170, 3, 0, 0, 0, 0, 0
),
(
17, 2, DATEADD(day, -10, SYSUTCDATETIME()), 0, 1, 0, 0, 0, 0, 0
),
(
18, 1, DATEADD(day, -2, SYSUTCDATETIME()), 210, 1, 0, 1, 1, 0, 0
),
(
19, 1, DATEADD(day, 0, SYSUTCDATETIME()), 210, 0, 2, 0, 1, 1, 0
)
GO

