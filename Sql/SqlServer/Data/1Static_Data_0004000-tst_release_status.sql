/***************************************************************
**	Insert script for table TST_RELEASE_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_RELEASE_STATUS ON; 

INSERT INTO TST_RELEASE_STATUS
(
RELEASE_STATUS_ID, NAME, POSITION, IS_ACTIVE
)
VALUES
(
1, 'Planned', 1, 1
),
(
2, 'In Progress', 2, 1
),
(
3, 'Completed', 3, 1
),
(
4, 'Closed', 4, 1
),
(
5, 'Deferred', 5, 1
),
(
6, 'Cancelled', 6, 1
)
GO

SET IDENTITY_INSERT TST_RELEASE_STATUS OFF; 

