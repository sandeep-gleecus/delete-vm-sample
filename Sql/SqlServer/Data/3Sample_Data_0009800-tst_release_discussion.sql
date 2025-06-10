/***************************************************************
**	Insert script for table TST_RELEASE_DISCUSSION
***************************************************************/
SET IDENTITY_INSERT TST_RELEASE_DISCUSSION ON; 

INSERT INTO TST_RELEASE_DISCUSSION
(
DISCUSSION_ID, ARTIFACT_ID, TEXT, CREATOR_ID, CREATION_DATE, IS_DELETED, IS_PERMANENT
)
VALUES
(
1, 1, 'This is the first version of the system, we need to make sure we have a good feedback process in place.', 2, DATEADD(minute, -215083, SYSUTCDATETIME()), 0, 0
),
(
2, 1, 'We will use the incident tracking module to capture the feedback and the cycle the items back into the next release.', 3, DATEADD(minute, -213973, SYSUTCDATETIME()), 0, 0
),
(
3, 1, 'That sounds like a good plan!', 2, DATEADD(minute, -213833, SYSUTCDATETIME()), 0, 0
)
GO

SET IDENTITY_INSERT TST_RELEASE_DISCUSSION OFF; 

