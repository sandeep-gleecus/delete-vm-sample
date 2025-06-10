/***************************************************************
**	Insert script for table TST_TEST_SET_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_TEST_SET_STATUS ON; 

INSERT INTO TST_TEST_SET_STATUS
(
TEST_SET_STATUS_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Not Started', 1
),
(
2, 'In Progress', 1
),
(
3, 'Completed', 1
),
(
4, 'Blocked', 1
),
(
5, 'Deferred', 1
)
GO

SET IDENTITY_INSERT TST_TEST_SET_STATUS OFF; 

