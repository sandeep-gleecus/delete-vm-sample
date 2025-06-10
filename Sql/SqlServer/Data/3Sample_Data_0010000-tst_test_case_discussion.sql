/***************************************************************
**	Insert script for table TST_TEST_CASE_DISCUSSION
***************************************************************/
SET IDENTITY_INSERT TST_TEST_CASE_DISCUSSION ON; 

INSERT INTO TST_TEST_CASE_DISCUSSION
(
DISCUSSION_ID, ARTIFACT_ID, TEXT, CREATOR_ID, CREATION_DATE, IS_DELETED, IS_PERMANENT
)
VALUES
(
1, 2, 'We need to add a better definition of the pre-conditions and post-conditions', 2, DATEADD(minute, -212468, SYSUTCDATETIME()), 0, 0
),
(
2, 2, 'OK will make sure that we do.', 3, DATEADD(minute, -212451, SYSUTCDATETIME()), 0, 0
),
(
3, 2, 'Thanks, appreciate it!', 2, DATEADD(minute, -212353, SYSUTCDATETIME()), 0, 0
)
GO

SET IDENTITY_INSERT TST_TEST_CASE_DISCUSSION OFF; 

