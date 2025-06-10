/***************************************************************
**	Insert script for table TST_EXECUTION_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_EXECUTION_STATUS ON; 

INSERT INTO TST_EXECUTION_STATUS
(
EXECUTION_STATUS_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Failed', 1
),
(
2, 'Passed', 1
),
(
3, 'Not Run', 1
),
(
4, 'N/A', 1
),
(
5, 'Blocked', 1
),
(
6, 'Caution', 1
),
(
7, 'InProgress', 1
)
GO

SET IDENTITY_INSERT TST_EXECUTION_STATUS OFF; 

