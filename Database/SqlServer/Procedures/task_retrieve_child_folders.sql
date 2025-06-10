-- =====================================================================
-- Author:			Inflectra Corporation
-- Business Object: Task
-- Description:		Gets the list of all children of the specified folder in hierarchy order
-- =====================================================================
IF OBJECT_ID ( 'TASK_RETRIEVE_CHILD_FOLDERS', 'P' ) IS NOT NULL 
    DROP PROCEDURE [TASK_RETRIEVE_CHILD_FOLDERS];
GO
CREATE PROCEDURE [TASK_RETRIEVE_CHILD_FOLDERS]
	@ProjectId INT,
	@FolderId INT,
	@IsIncludeSelf BIT
AS
DECLARE
	@IndentLevel NVARCHAR(MAX)
BEGIN	
	--First get the selected folder
	SELECT @IndentLevel = INDENT_LEVEL FROM TST_TASK_FOLDER_HIERARCHY WHERE TASK_FOLDER_ID = @FolderId;

	--Now get the child folders
	SELECT TASK_FOLDER_ID AS TASK_FOLDER_ID, PROJECT_ID, NAME, PARENT_TASK_FOLDER_ID, HIERARCHY_LEVEL, INDENT_LEVEL
	FROM TST_TASK_FOLDER_HIERARCHY
	WHERE SUBSTRING(INDENT_LEVEL, 1, LEN(@IndentLevel)) = @IndentLevel
	AND (LEN(INDENT_LEVEL) > LEN(@IndentLevel) OR (LEN(INDENT_LEVEL) = LEN(@IndentLevel) AND @IsIncludeSelf = 1))
	AND PROJECT_ID = @ProjectId
	ORDER BY INDENT_LEVEL, TASK_FOLDER_ID
END
GO
