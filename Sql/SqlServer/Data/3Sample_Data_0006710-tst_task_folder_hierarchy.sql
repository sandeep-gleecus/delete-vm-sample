/***************************************************************
**	Insert script for table TST_TASK_FOLDER_HIERARCHY
***************************************************************/
INSERT INTO TST_TASK_FOLDER_HIERARCHY
(
TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
)
VALUES
(
1, 1, 'Development Tasks', NULL, 1, 'AAA'
),
(
2, 1, 'Front-End Development', 1, 2, 'AAAAAB'
),
(
3, 1, 'Back-End Development', 1, 2, 'AAAAAA'
),
(
4, 1, 'Oversight', NULL, 1, 'AAC'
),
(
5, 1, 'Infrastructure Tasks', NULL, 1, 'AAB'
),
(
6, 1, 'Database', 5, 2, 'AABAAB'
),
(
7, 1, 'Application', 5, 2, 'AABAAA'
)
GO

