/***************************************************************
**	Insert script for table TST_DATA_SYNC_ARTIFACT_MAPPING
***************************************************************/
INSERT INTO TST_DATA_SYNC_ARTIFACT_MAPPING
(
DATA_SYNC_SYSTEM_ID, PROJECT_ID, ARTIFACT_TYPE_ID, ARTIFACT_ID, EXTERNAL_KEY
)
VALUES
(
1, 1, 4, 1, '10000'
),
(
1, 1, 4, 4, '10001'
),
(
2, 1, 4, 1, 'Version 1.0'
),
(
2, 1, 4, 2, 'Version 1.1'
),
(
3, 1, 4, 1, 'Library Information System'
),
(
3, 1, 4, 2, 'Library Information System\Iteration 0'
),
(
3, 1, 4, 3, 'Library Information System\Iteration 1'
)
GO

