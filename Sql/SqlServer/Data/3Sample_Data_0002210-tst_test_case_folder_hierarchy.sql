/***************************************************************
**	Insert script for table TST_TEST_CASE_FOLDER_HIERARCHY
***************************************************************/
INSERT INTO TST_TEST_CASE_FOLDER_HIERARCHY
(
TEST_CASE_FOLDER_ID, PROJECT_ID, NAME, PARENT_TEST_CASE_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
)
VALUES
(
1, 1, 'Functional Tests', NULL, 1, 'AAB'
),
(
2, 1, 'Regression Tests', NULL, 1, 'AAC'
),
(
3, 1, 'Scenario Tests', NULL, 1, 'AAD'
),
(
4, 1, 'Exception Scenario Tests', 3, 2, 'AADAAA'
),
(
5, 1, 'Common Tests', NULL, 1, 'AAA'
)
GO

