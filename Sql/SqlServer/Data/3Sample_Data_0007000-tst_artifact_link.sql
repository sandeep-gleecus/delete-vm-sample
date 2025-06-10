/***************************************************************
**	Insert script for table TST_ARTIFACT_LINK
***************************************************************/
SET IDENTITY_INSERT TST_ARTIFACT_LINK ON; 

INSERT INTO TST_ARTIFACT_LINK
(
ARTIFACT_LINK_ID, SOURCE_ARTIFACT_ID, SOURCE_ARTIFACT_TYPE_ID, DEST_ARTIFACT_ID, DEST_ARTIFACT_TYPE_ID, CREATOR_ID, CREATION_DATE, ARTIFACT_LINK_TYPE_ID, COMMENT
)
VALUES
(
1, 4, 1, 6, 1, 2, DATEADD(day, -133, SYSUTCDATETIME()), 1, 'These two requirements are related'
),
(
2, 5, 1, 7, 1, 2, DATEADD(day, -132, SYSUTCDATETIME()), 2, NULL
),
(
3, 4, 1, 5, 3, 2, DATEADD(day, -131, SYSUTCDATETIME()), 1, 'This bug affects the requirement'
),
(
4, 5, 1, 6, 3, 2, DATEADD(day, -130, SYSUTCDATETIME()), 1, NULL
),
(
5, 1, 3, 6, 3, 3, DATEADD(day, -129, SYSUTCDATETIME()), 1, 'This incident and bug are related'
),
(
6, 6, 3, 7, 3, 3, DATEADD(day, -128, SYSUTCDATETIME()), 1, NULL
),
(
7, 6, 3, 8, 1, 3, DATEADD(day, -127, SYSUTCDATETIME()), 1, NULL
),
(
8, 7, 3, 9, 1, 3, DATEADD(day, -126, SYSUTCDATETIME()), 1, NULL
),
(
9, 2, 7, 7, 3, 2, DATEADD(day, -126, SYSUTCDATETIME()), 1, 'This incident is related to the test step'
),
(
10, 4, 1, 30, 1, 2, DATEADD(day, -125, SYSUTCDATETIME()), 1, 'This use case defines the steps for creating a book'
),
(
11, 5, 1, 31, 1, 2, DATEADD(day, -125, SYSUTCDATETIME()), 1, 'This use case defines the steps for editing a book'
),
(
12, 7, 6, 1, 6, 2, DATEADD(day, -112, SYSUTCDATETIME()), 2, 'Need to create the screen before refactoring'
),
(
13, 14, 6, 4, 6, 3, DATEADD(day, -112, SYSUTCDATETIME()), 2, 'Need to create the screen before refactoring'
),
(
14, 15, 6, 3, 6, 2, DATEADD(day, -80, SYSUTCDATETIME()), 2, NULL
),
(
15, 11, 3, 14, 6, 3, DATEADD(day, -80, SYSUTCDATETIME()), 1, 'Need to refactor the screen before fixing the bug'
)
GO

SET IDENTITY_INSERT TST_ARTIFACT_LINK OFF; 

