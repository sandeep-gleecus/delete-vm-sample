/***************************************************************
**	Insert script for table TST_TASK_FOLDER
***************************************************************/
SET IDENTITY_INSERT TST_TASK_FOLDER ON; 

INSERT INTO TST_TASK_FOLDER
(
TASK_FOLDER_ID, PROJECT_ID, PARENT_TASK_FOLDER_ID, NAME
)
VALUES
(
1, 1, NULL, 'Development Tasks'
),
(
2, 1, 1, 'Front-End Development'
),
(
3, 1, 1, 'Back-End Development'
),
(
4, 1, NULL, 'Oversight'
),
(
5, 1, NULL, 'Infrastructure Tasks'
),
(
6, 1, 5, 'Database'
),
(
7, 1, 5, 'Application'
)
GO

SET IDENTITY_INSERT TST_TASK_FOLDER OFF; 

