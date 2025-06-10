/***************************************************************
**	Insert script for table TST_TEST_CASE_PREPARATION_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_TEST_CASE_PREPARATION_STATUS ON; 

INSERT INTO TST_TEST_CASE_PREPARATION_STATUS
(
TEST_CASE_PREPARATION_STATUS_ID, NAME, IS_ACTIVE, POSITION
)
VALUES
(
1, 'Not Recorded', 1, 1
),
(
2, 'Recorded - Failed', 1, 2
),
(
3, 'Recorded - Issue', 1, 3
),
(
4, 'Recorded - Passed', 1, 4
)
GO

SET IDENTITY_INSERT TST_TEST_CASE_PREPARATION_STATUS OFF; 

