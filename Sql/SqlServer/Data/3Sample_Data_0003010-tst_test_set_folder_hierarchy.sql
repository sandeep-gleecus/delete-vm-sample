/***************************************************************
**	Insert script for table TST_TEST_SET_FOLDER_HIERARCHY
***************************************************************/
INSERT INTO TST_TEST_SET_FOLDER_HIERARCHY
(
TEST_SET_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_SET_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
)
VALUES
(
1, 1, 'Functional Test Sets', NULL, 1, 'AAA'
),
(
2, 1, 'Regression Test Sets', NULL, 1, 'AAB'
)
GO

