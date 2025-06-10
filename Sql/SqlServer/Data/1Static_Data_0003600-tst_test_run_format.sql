/***************************************************************
**	Insert script for table TST_TEST_RUN_FORMAT
***************************************************************/
SET IDENTITY_INSERT TST_TEST_RUN_FORMAT ON; 

INSERT INTO TST_TEST_RUN_FORMAT
(
TEST_RUN_FORMAT_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Plain Text', 1
),
(
2, 'HTML', 1
)
GO

SET IDENTITY_INSERT TST_TEST_RUN_FORMAT OFF; 

