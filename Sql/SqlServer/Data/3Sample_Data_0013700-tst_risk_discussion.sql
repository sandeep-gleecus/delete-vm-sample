/***************************************************************
**	Insert script for table TST_RISK_DISCUSSION
***************************************************************/
SET IDENTITY_INSERT TST_RISK_DISCUSSION ON; 

INSERT INTO TST_RISK_DISCUSSION
(
DISCUSSION_ID, ARTIFACT_ID, TEXT, CREATION_DATE, IS_DELETED, IS_PERMANENT, CREATOR_ID
)
VALUES
(
1, 1, '<p>The v1.1 release is very important, we should look into ways to find additional people.</p>', DATEADD(day, -6, SYSUTCDATETIME()), 0, 0, 2
),
(
2, 3, '<p>The database is an industry standard RDBMS system and it should handle the volume, but we should check.</p>', DATEADD(day, -4, SYSUTCDATETIME()), 0, 0, 2
)
GO

SET IDENTITY_INSERT TST_RISK_DISCUSSION OFF; 

