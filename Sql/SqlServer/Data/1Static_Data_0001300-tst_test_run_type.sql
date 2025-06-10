/***************************************************************
**	Insert script for table TST_TEST_RUN_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_TEST_RUN_TYPE ON; 

INSERT INTO TST_TEST_RUN_TYPE
(
TEST_RUN_TYPE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Manual', 1
),
(
2, 'Automated', 1
)
GO

SET IDENTITY_INSERT TST_TEST_RUN_TYPE OFF; 

