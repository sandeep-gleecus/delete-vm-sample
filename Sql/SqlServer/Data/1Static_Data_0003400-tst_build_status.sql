/***************************************************************
**	Insert script for table TST_BUILD_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_BUILD_STATUS ON; 

INSERT INTO TST_BUILD_STATUS
(
BUILD_STATUS_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Failed', 1
),
(
2, 'Succeeded', 1
),
(
3, 'Unstable', 1
),
(
4, 'Aborted', 1
)
GO

SET IDENTITY_INSERT TST_BUILD_STATUS OFF; 

