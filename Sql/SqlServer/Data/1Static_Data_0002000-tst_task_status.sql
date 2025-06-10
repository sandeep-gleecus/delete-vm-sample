/***************************************************************
**	Insert script for table TST_TASK_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_TASK_STATUS ON; 

INSERT INTO TST_TASK_STATUS
(
TASK_STATUS_ID, NAME, POSITION, IS_ACTIVE
)
VALUES
(
1, 'Not Started', 1, 1
),
(
2, 'In Progress', 2, 1
),
(
3, 'Completed', 4, 1
),
(
4, 'Blocked', 5, 1
),
(
5, 'Deferred', 6, 1
),
(
6, 'Rejected', 7, 1
),
(
7, 'Duplicate', 8, 1
),
(
8, 'Under Review', 9, 1
),
(
9, 'Obsolete', 10, 1
)
GO

SET IDENTITY_INSERT TST_TASK_STATUS OFF; 

