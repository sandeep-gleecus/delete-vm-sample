/***************************************************************
**	Insert script for table TST_TASK_DISCUSSION
***************************************************************/
SET IDENTITY_INSERT TST_TASK_DISCUSSION ON; 

INSERT INTO TST_TASK_DISCUSSION
(
DISCUSSION_ID, ARTIFACT_ID, TEXT, CREATOR_ID, CREATION_DATE, IS_DELETED, IS_PERMANENT
)
VALUES
(
1, 1, 'Do you think we need to use different tabs for the book details and the author details?', 2, DATEADD(minute, -209454, SYSUTCDATETIME()), 0, 0
),
(
2, 1, 'How about we use expander/collapser controls instead of tabs. That way you can see both at once?', 3, DATEADD(minute, -209452, SYSUTCDATETIME()), 0, 0
),
(
3, 1, 'OK, that make sense, thanks for the advice.', 2, DATEADD(minute, -209449, SYSUTCDATETIME()), 0, 0
)
GO

SET IDENTITY_INSERT TST_TASK_DISCUSSION OFF; 

