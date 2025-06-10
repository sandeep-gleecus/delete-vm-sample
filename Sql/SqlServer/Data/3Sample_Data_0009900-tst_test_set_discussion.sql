/***************************************************************
**	Insert script for table TST_TEST_SET_DISCUSSION
***************************************************************/
SET IDENTITY_INSERT TST_TEST_SET_DISCUSSION ON; 

INSERT INTO TST_TEST_SET_DISCUSSION
(
DISCUSSION_ID, ARTIFACT_ID, TEXT, CREATOR_ID, CREATION_DATE, IS_DELETED, IS_PERMANENT
)
VALUES
(
1, 1, 'We need to make sure we get through this test set by the first release.', 2, DATEADD(minute, -203474, SYSUTCDATETIME()), 0, 0
),
(
2, 1, 'OK will make sure that we do.', 3, DATEADD(minute, -202484, SYSUTCDATETIME()), 0, 0
),
(
3, 1, 'Thanks, appreciate it!', 2, DATEADD(minute, -202433, SYSUTCDATETIME()), 0, 0
)
GO

SET IDENTITY_INSERT TST_TEST_SET_DISCUSSION OFF; 

