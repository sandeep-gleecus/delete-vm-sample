IF OBJECT_ID ( 'VW_TASK_FOLDER_HIERARCHY', 'V' ) IS NOT NULL 
    DROP VIEW [VW_TASK_FOLDER_HIERARCHY];
GO
CREATE VIEW [VW_TASK_FOLDER_HIERARCHY]
AS
SELECT TASK_FOLDER_ID AS TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
FROM TST_TASK_FOLDER_HIERARCHY
GO
