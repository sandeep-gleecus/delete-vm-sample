/***************************************************************
**	Insert script for table TST_REQUIREMENT_DISCUSSION
***************************************************************/
SET IDENTITY_INSERT TST_REQUIREMENT_DISCUSSION ON; 

INSERT INTO TST_REQUIREMENT_DISCUSSION
(
DISCUSSION_ID, ARTIFACT_ID, TEXT, CREATOR_ID, CREATION_DATE, IS_DELETED, IS_PERMANENT
)
VALUES
(
1, 4, 'Need to write a better definition of this requirement, it has too many loose-ends.', 2, DATEADD(minute, -213755, SYSUTCDATETIME()), 0, 0
),
(
2, 4, 'You''re right, I have added some more detail and linked to some test cases that define how the functionality is expected to work.', 3, DATEADD(minute, -213653, SYSUTCDATETIME()), 0, 0
),
(
3, 4, 'OK, that''s much better, thanks for adding the additional information.', 2, DATEADD(minute, -212466, SYSUTCDATETIME()), 0, 0
)
GO

SET IDENTITY_INSERT TST_REQUIREMENT_DISCUSSION OFF; 

