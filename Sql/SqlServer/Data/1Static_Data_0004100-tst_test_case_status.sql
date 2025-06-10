/***************************************************************
**	Insert script for table TST_TEST_CASE_STATUS
***************************************************************/
SET IDENTITY_INSERT TST_TEST_CASE_STATUS ON; 

INSERT INTO TST_TEST_CASE_STATUS
(
TEST_CASE_STATUS_ID, NAME, POSITION, IS_ACTIVE
)
VALUES
(
1, 'Draft', 1, 1
),
(
2, 'Ready for Review', 2, 1
),
(
3, 'Rejected', 3, 1
),
(
4, 'Approved', 4, 1
),
(
5, 'Ready for Test', 5, 1
),
(
6, 'Obsolete', 8, 1
),
(
7, 'Tested', 6, 1
),
(
8, 'Verified', 7, 1
)
GO

SET IDENTITY_INSERT TST_TEST_CASE_STATUS OFF; 

