/***************************************************************
**	Insert script for table TST_SAVED_FILTER
***************************************************************/
INSERT INTO TST_SAVED_FILTER
(
USER_ID, PROJECT_ID, ARTIFACT_TYPE_ID, NAME, IS_SHARED
)
VALUES
(
2, 1, 1, 'Critical Not-Covered Requirements', 1
),
(
2, 1, 2, 'Failed Active Test Cases', 0
),
(
2, 1, 3, 'New Unassigned Incidents', 0
),
(
2, 1, 3, 'All Reopened Incidents', 0
),
(
2, 1, 6, 'High Priority Late Tasks', 1
),
(
2, 1, 8, 'Not Executed Test Sets', 0
)
GO

